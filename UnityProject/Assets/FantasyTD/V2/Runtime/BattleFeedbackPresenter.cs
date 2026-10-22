using System;
using System.Collections.Generic;
using FantasyTD.MapA;
using FantasyTD.V2.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.V2.Runtime
{
    /// <summary>
    /// Non-authoritative battle feedback. Never edits economy, health, movement or wave state.
    /// Short-lived cues are deliberately capped to avoid saturating mobile devices.
    /// </summary>
    public sealed class BattleFeedbackPresenter : MonoBehaviour
    {
        private sealed class FloatingCue
        {
            public Text Text;
            public RectTransform Rect;
            public SpriteRenderer Flash;
            public Vector2 Origin;
            public Color Tint;
            public float Age;
            public float Lifetime;
        }

        private readonly List<FloatingCue> _active = new List<FloatingCue>();
        private readonly Dictionary<int, float> _lastHit = new Dictionary<int, float>();
        private readonly Dictionary<FeedbackKind, AudioClip> _clips =
            new Dictionary<FeedbackKind, AudioClip>();
        private Canvas _canvas;
        private Camera _camera;
        private Transform _world;
        private AudioSource _audio;
        private float _lastSound;

        public void Initialize(Canvas canvas, Camera camera)
        {
            _canvas = canvas;
            _camera = camera;
            _audio = GetComponent<AudioSource>();
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
            _audio.volume = .25f;
        }

        public void BindWorld(Transform world)
        {
            Clear();
            _world = world;
        }

        public void Clear()
        {
            foreach (FloatingCue cue in _active)
            {
                if (cue.Text != null) Destroy(cue.Text.gameObject);
                if (cue.Flash != null) Destroy(cue.Flash.gameObject);
            }
            _active.Clear();
            _lastHit.Clear();
        }

        public void Show(BattleEvent battleEvent, Vector3 worldPosition)
        {
            FeedbackCue cue = BattleFeedbackCatalog.Map(battleEvent);
            if (cue.Kind == FeedbackKind.None || _canvas == null) return;

            if (cue.Kind == FeedbackKind.Hit)
            {
                float previous;
                if (_lastHit.TryGetValue(battleEvent.EntityId, out previous) &&
                    Time.unscaledTime - previous < .22f)
                    return;
                _lastHit[battleEvent.EntityId] = Time.unscaledTime;
            }

            Color color = CueColor(cue.Kind);
            float lifetime = cue.Kind == FeedbackKind.Victory ||
                cue.Kind == FeedbackKind.Defeat ? 2f : 1.05f;
            Vector2 screenPoint = _camera != null
                ? (Vector2)_camera.WorldToScreenPoint(worldPosition)
                : new Vector2(Screen.width * .5f, Screen.height * .5f);
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform, screenPoint, null, out local))
                local = Vector2.zero;

            if (_active.Count >= 32) RemoveCue(0);
            Text label = RuntimeArt.CreateText(_canvas.transform,
                "Battle cue " + cue.Kind, 34, TextAnchor.MiddleCenter, color);
            label.text = cue.Message;
            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 18;
            label.resizeTextMaxSize = 36;
            label.raycastTarget = false;
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.05f, .09f, .08f, .9f);
            outline.effectDistance = new Vector2(2f, -2f);
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(
                cue.Kind == FeedbackKind.Victory || cue.Kind == FeedbackKind.Defeat
                    ? 650f : 170f, 76f);
            rect.anchoredPosition = local;

            SpriteRenderer flash = null;
            if (_world != null &&
                (cue.Kind == FeedbackKind.Plant || cue.Kind == FeedbackKind.Upgrade ||
                 cue.Kind == FeedbackKind.Hit ||
                 cue.Kind == FeedbackKind.Kill || cue.Kind == FeedbackKind.Leak))
            {
                GameObject ring = RuntimeArt.SpriteObject(
                    "FX " + cue.Kind, _world, worldPosition,
                    Vector2.one * .34f,
                    new Color(color.r, color.g, color.b, .38f), 45, true);
                flash = ring.GetComponent<SpriteRenderer>();
            }
            _active.Add(new FloatingCue {
                Text = label, Rect = rect, Flash = flash,
                Origin = local, Tint = color, Lifetime = lifetime
            });
            if (cue.Audible) PlayCue(cue.Kind);
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, .1f);
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                FloatingCue cue = _active[i];
                cue.Age += dt;
                float t = Mathf.Clamp01(cue.Age / cue.Lifetime);
                cue.Rect.anchoredPosition = cue.Origin + Vector2.up * (70f * t);
                Color tint = cue.Tint;
                tint.a = Mathf.Clamp01((1f - t) * 1.35f);
                cue.Text.color = tint;
                if (cue.Flash != null)
                {
                    float s = .4f + 1.25f * t;
                    cue.Flash.transform.localScale = new Vector3(s, s, 1f);
                    Color flash = tint;
                    flash.a *= .35f;
                    cue.Flash.color = flash;
                }
                if (cue.Age >= cue.Lifetime) RemoveCue(i);
            }
        }

        private void RemoveCue(int index)
        {
            FloatingCue cue = _active[index];
            if (cue.Text != null) Destroy(cue.Text.gameObject);
            if (cue.Flash != null) Destroy(cue.Flash.gameObject);
            _active.RemoveAt(index);
        }

        private static Color CueColor(FeedbackKind kind)
        {
            switch (kind)
            {
                case FeedbackKind.Hit: return new Color(1f, .89f, .64f);
                case FeedbackKind.Kill: return new Color(.69f, 1f, .48f);
                case FeedbackKind.Income: return new Color(.96f, .86f, .40f);
                case FeedbackKind.Plant: return new Color(.6f, 1f, .6f);
                case FeedbackKind.Upgrade: return new Color(.55f, .92f, 1f);
                case FeedbackKind.Leak:
                case FeedbackKind.Defeat: return new Color(1f, .36f, .31f);
                case FeedbackKind.Victory: return new Color(.98f, .83f, .28f);
                default: return Color.white;
            }
        }

        private void PlayCue(FeedbackKind kind)
        {
            if (_audio == null || Time.unscaledTime - _lastSound < .08f) return;
            _lastSound = Time.unscaledTime;
            AudioClip clip;
            if (!_clips.TryGetValue(kind, out clip))
            {
                float pitch = kind == FeedbackKind.Hit ? 430f :
                    kind == FeedbackKind.Kill ? 710f :
                    kind == FeedbackKind.Leak || kind == FeedbackKind.Defeat ? 210f :
                    kind == FeedbackKind.Victory ? 960f : 560f;
                float seconds = kind == FeedbackKind.Victory ? .3f : .12f;
                clip = MakeTone(kind.ToString(), pitch, seconds);
                _clips.Add(kind, clip);
            }
            _audio.PlayOneShot(clip, kind == FeedbackKind.Hit ? .35f : .65f);
        }

        private static AudioClip MakeTone(string name, float pitch, float seconds)
        {
            const int rate = 22050;
            int count = Mathf.Max(1, Mathf.RoundToInt(rate * seconds));
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                float envelope = Mathf.Pow(1f - i / (float)count, 2f);
                samples[i] = envelope * .20f * (
                    Mathf.Sin(2f * Mathf.PI * pitch * t) +
                    .22f * Mathf.Sin(4f * Mathf.PI * pitch * t));
            }
            AudioClip clip = AudioClip.Create("FT2_" + name, count, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            Clear();
            foreach (AudioClip clip in _clips.Values)
                if (clip != null) Destroy(clip);
            _clips.Clear();
        }
    }
}
