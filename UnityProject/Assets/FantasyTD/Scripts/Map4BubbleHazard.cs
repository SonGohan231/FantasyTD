using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    // Canonical MAP4 event: an authored hot bubble crosses one branch, absorbs
    // projectiles and bursts. Only one bubble exists at a time; other lanes remain free.
    public sealed class Map4BubbleHazard : MonoBehaviour
    {
        private static readonly List<Map4BubbleHazard> Instances = new List<Map4BubbleHazard>();
        private GameSession _session;
        private SpriteRenderer _renderer;
        private Vector3 _baseScale;
        private Vector2 _from;
        private Vector2 _to;
        private float _startedAt;
        private float _nextAt;
        private float _burstAt = -1f;
        private int _eventOrdinal;
        public bool BubbleActive { get; private set; }
        public int AbsorbedProjectileCount { get; private set; }

        public void Initialize(GameSession session, Sprite bubble)
        {
            _session = session;
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = bubble;
            _renderer.sortingOrder = 65;
            _baseScale = new Vector3(1.05f / bubble.bounds.size.x, 1.05f / bubble.bounds.size.y, 1f);
            _renderer.enabled = false;
            _nextAt = Time.time + 12f;
            Instances.Add(this);
        }

        private void OnDestroy() => Instances.Remove(this);

        private void Update()
        {
            if (_session == null || _session.IsFinished)
            {
                BubbleActive = false;
                if (_renderer != null) _renderer.enabled = false;
                return;
            }
            if (!BubbleActive)
            {
                if (_session.Waves.Running && _session.Waves.CurrentWave >= 2 && Time.time >= _nextAt)
                    BeginBubble();
                return;
            }
            float age = Time.time - _startedAt;
            if (_burstAt < 0f && age >= 4.5f) _burstAt = Time.time;
            float burstAge = _burstAt >= 0f ? Time.time - _burstAt : 0f;
            if (_burstAt >= 0f && burstAge >= 0.25f)
            {
                BubbleActive = false;
                _renderer.enabled = false;
                _nextAt = Time.time + 12f;
                return;
            }
            transform.position = Vector2.Lerp(_from, _to, Mathf.Clamp01(age / 4.5f));
            float formation = Mathf.Clamp01(age / 0.35f);
            float burstScale = _burstAt >= 0f ? 1f + burstAge * 2f : 1f;
            transform.localScale = _baseScale * Mathf.Max(0.1f, formation) * burstScale;
            _renderer.color = new Color(1f, 1f, 1f, _burstAt >= 0f ? 1f - burstAge / 0.25f : 1f);
        }

        private void BeginBubble()
        {
            bool upper = (_eventOrdinal++ & 1) == 0;
            _from = upper ? new Vector2(4.8f, 8.6f) : new Vector2(17.1f, 5.4f);
            _to = upper ? new Vector2(7.4f, 9.5f) : new Vector2(20.3f, 4.7f);
            _startedAt = Time.time;
            _burstAt = -1f;
            BubbleActive = true;
            transform.position = _from;
            transform.localScale = _baseScale * 0.1f;
            _renderer.color = Color.white;
            _renderer.enabled = true;
        }

        public static bool TryAbsorbSegment(Vector2 from, Vector2 to)
        {
            foreach (Map4BubbleHazard hazard in Instances)
            {
                if (hazard == null || !hazard.BubbleActive || hazard._burstAt >= 0f ||
                    Time.time - hazard._startedAt < 0.35f || hazard._session == null || hazard._session.IsFinished)
                    continue;
                Vector2 delta = to - from;
                Vector2 centre = hazard.transform.position;
                float t = delta.sqrMagnitude > 0.000001f
                    ? Mathf.Clamp01(Vector2.Dot(centre - from, delta) / delta.sqrMagnitude) : 0f;
                if (Vector2.Distance(from + delta * t, centre) > 0.50f) continue;
                hazard.AbsorbedProjectileCount++;
                hazard._burstAt = Time.time;
                return true;
            }
            return false;
        }
    }
}
