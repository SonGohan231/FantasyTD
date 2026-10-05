using System;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    // Discovery uses an actual source-art vault. No nonexistent opening frames are implied.
    public sealed class Map8TreasurySecret : MonoBehaviour
    {
        public const string FoundKey = "FantasyTD.Secrets.v1.DragonTreasuryFound";
        public bool IsDiscovered { get; private set; }
        private GameSession _session;
        private Text _label;
        private Button _button;
        private static readonly Vector2 Position = new Vector2(29.9f,3.4f);

        public void Initialize(GameSession session)
        {
            if (session == null || session.MapDefinition.Id != "MAP8")
                throw new ArgumentException("Dragon treasury belongs to MAP8.");
            _session = session;
            Texture2D texture = Resources.Load<Texture2D>(Map8SourceDefinition.Resource);
            if (texture == null || texture.width != 2048 || texture.height != 1152)
                throw new InvalidOperationException("Recovered MAP8 source must be the exact 2048x1152 PNG.");
            Rect crop = new Rect(650f,747f,213f,371f);
            Sprite vault = Sprite.Create(texture,new Rect(crop.x,1152f-crop.yMax,crop.width,crop.height),
                Vector2.one*0.5f,100f);
            vault.name = "MAP8_RecoveredVaultDoor";
            GameObject canvasGo = new GameObject("RecoveredTreasuryCanvas",typeof(RectTransform),typeof(Canvas));
            canvasGo.transform.SetParent(transform,false);
            canvasGo.transform.position = Position;
            canvasGo.transform.localScale = Vector3.one*0.01f;
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 25;
            canvasGo.AddComponent<GraphicRaycaster>();
            GameObject target = new GameObject("TreasuryDoorButton",typeof(RectTransform),typeof(Image),typeof(Button));
            target.transform.SetParent(canvasGo.transform,false);
            Image image = target.GetComponent<Image>();
            image.sprite = vault;
            image.preserveAspect = true;
            image.rectTransform.sizeDelta = new Vector2(165f,265f);
            _button = target.GetComponent<Button>();
            _button.transition = Selectable.Transition.None;
            _button.onClick.AddListener(Discover);
            _label = RuntimeArt.CreateText(canvasGo.transform,"TreasuryHint",20,TextAnchor.MiddleCenter,Color.white);
            _label.raycastTarget = false;
            _label.text = "TAJEMNICZE WROTA";
            _label.rectTransform.sizeDelta = new Vector2(260f,50f);
            _label.rectTransform.anchoredPosition = new Vector2(0f,-152f);
        }

        private void Discover() => TryDiscover();

        public bool TryDiscover()
        {
            if (_session == null || _session.IsFinished || IsDiscovered) return false;
            IsDiscovered = true;
            _button.interactable = false;
            bool first = PlayerPrefs.GetInt(FoundKey,0) == 0;
            if (first)
            {
                PlayerPrefs.SetInt(FoundKey,1);
                PlayerPrefs.Save();
            }
            _label.text = first ? "SKARBIEC ODKRYTY" : "SKARBIEC JUŻ ODKRYTY";
            _session.Hud.ShowFloatingMessage(Position,first ? "Odnaleziono smoczy skarbiec" : "Skarbiec zapisany w odkryciach",
                new Color(1f,0.85f,0.35f));
            return true;
        }

        private void Update()
        {
            if (_session != null && _session.IsFinished && _button != null) _button.interactable = false;
        }
    }
}
