using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public sealed class MobileHud : MonoBehaviour
    {
        private GameSession _session;
        private Canvas _canvas;
        private RectTransform _safe;
        private Text _essenceText;
        private Text _hpText;
        private Text _waveText;
        private Text _enemyText;
        private Button _waveButton;
        private Text _waveButtonText;
        private RectTransform _radialRoot;
        private Action<PlantKind> _onBuild;
        private Action _onClose;
        private Coroutine _essencePulse;

        public RectTransform RadialRoot => _radialRoot;
        public RectTransform SafeArea => _safe;

        public void Initialize(GameSession session)
        {
            _session = session;
            EnsureEventSystem();
            BuildCanvas();
        }

        public void Refresh()
        {
            if (_session == null || _essenceText == null) return;

            _essenceText.text = "✦ " + _session.Economy.Essence + "  ESENCJI";
            _hpText.text = "♥ " + _session.Base.Hp + "/" + _session.Base.MaxHp;
            _waveText.text = "FALA  " + _session.Waves.CurrentWave + "/" + _session.Waves.TotalWaves;
            _enemyText.text = "WROGOWIE  " + _session.Enemies.Count;

            bool ready = _session.Waves.ReadyForNextWave;
            _waveButton.gameObject.SetActive(ready && !_session.IsFinished);
            _waveButtonText.text = _session.Waves.CurrentWave == 0 ? "START 1. FALI" : "NASTĘPNA FALA";
        }

        public void OpenBuildRadial(PlantingSpot spot, Action<PlantKind> onBuild, Action onClose)
        {
            _onBuild = onBuild;
            _onClose = onClose;
            CloseBuildRadial();

            GameObject root = new GameObject("RadialBuildMenu", typeof(RectTransform));
            root.transform.SetParent(_safe, false);
            _radialRoot = root.GetComponent<RectTransform>();
            _radialRoot.sizeDelta = Vector2.zero;

            Vector2 local;
            Vector3 screen = Camera.main.WorldToScreenPoint(spot.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _safe, new Vector2(screen.x, screen.y), null, out local);
            _radialRoot.anchoredPosition = local;

            CreateRadialButton(PlantKind.SeedShooter, new Vector2(-125f, 0f), new Color(0.28f, 0.64f, 0.20f));
            CreateRadialButton(PlantKind.EssenceFlower, new Vector2(0f, 125f), new Color(0.92f, 0.64f, 0.12f));
            CreateRadialButton(PlantKind.OakGuardian, new Vector2(125f, 0f), new Color(0.42f, 0.26f, 0.10f));
            CreateRadialButton(PlantKind.FrostBud, new Vector2(0f, -125f), new Color(0.24f, 0.62f, 0.82f));
            if (Map5SecretController.MutationTowerUnlocked)
                CreateRadialButton(PlantKind.MutationTower, new Vector2(0f, -250f), new Color(0.66f,0.20f,0.48f));

            Button close = RuntimeArt.CreateButton(
                _radialRoot, "×", new Color(0.23f, 0.14f, 0.075f, 0.98f), 38);
            close.gameObject.name = "RadialCloseButton";
            RectTransform cr = close.GetComponent<RectTransform>();
            cr.sizeDelta = new Vector2(74f, 74f);
            cr.anchoredPosition = Vector2.zero;
            close.onClick.AddListener(delegate
            {
                if (_onClose != null) _onClose();
            });

            Canvas.ForceUpdateCanvases();
            ClampRadialToSafeArea();
            Canvas.ForceUpdateCanvases();
        }

        public void CloseBuildRadial()
        {
            if (_radialRoot != null)
                Destroy(_radialRoot.gameObject);
            _radialRoot = null;
        }

        public void PulseEssence()
        {
            if (_essencePulse != null) StopCoroutine(_essencePulse);
            _essencePulse = StartCoroutine(PulseText());
        }

        public void ShowFloatingMessage(Vector2 world, string message, Color color)
        {
            StartCoroutine(FloatingMessageRoutine(world, message, color));
        }

        public void ShowEndPanel(bool victory)
        {
            CloseBuildRadial();

            GameObject panel = new GameObject(
                "EndPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline));
            panel.transform.SetParent(_safe, false);

            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f, 0.5f);
            pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(760f, 400f);

            Image panelImage = panel.GetComponent<Image>();
            panelImage.sprite = null;
            panelImage.color = victory
                ? new Color(0.075f, 0.18f, 0.075f, 0.97f)
                : new Color(0.20f, 0.075f, 0.055f, 0.97f);

            Outline outline = panel.GetComponent<Outline>();
            outline.effectColor = victory
                ? new Color(0.58f, 0.78f, 0.22f, 0.72f)
                : new Color(0.72f, 0.28f, 0.13f, 0.72f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text title = RuntimeArt.CreateText(
                panel.transform, "Result", 54, TextAnchor.MiddleCenter, Color.white);
            title.rectTransform.anchorMin = new Vector2(0.08f, 0.38f);
            title.rectTransform.anchorMax = new Vector2(0.92f, 0.90f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.fontStyle = FontStyle.Bold;
            bool map2 = _session.MapDefinition != null && _session.MapDefinition.Id == "MAP2";
            title.text = victory
                ? (map2 ? "ODZYSKANE RUINY OBRONIONE!" : "ZIELONY TRAKT OBRONIONY!")
                : "KRÓL CIERNI PRZEDARŁ SIĘ DO BAZY";

            Shadow titleShadow = title.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0f, 0f, 0f, 0.68f);
            titleShadow.effectDistance = new Vector2(3f, -3f);

            Button restart = RuntimeArt.CreateButton(
                panel.transform,
                "ZAGRAJ PONOWNIE",
                victory ? new Color(0.30f, 0.62f, 0.19f) : new Color(0.66f, 0.28f, 0.13f),
                28);
            RectTransform rr = restart.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(0.5f, 0.18f);
            rr.anchorMax = new Vector2(0.5f, 0.18f);
            rr.sizeDelta = new Vector2(360f, 100f);
            restart.onClick.AddListener(delegate
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }

        private void BuildCanvas()
        {
            GameObject canvasGo = new GameObject(
                "MobileCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject safeGo = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            safeGo.transform.SetParent(canvasGo.transform, false);
            _safe = safeGo.GetComponent<RectTransform>();
            _safe.anchorMin = Vector2.zero;
            _safe.anchorMax = Vector2.one;
            _safe.offsetMin = Vector2.zero;
            _safe.offsetMax = Vector2.zero;

            CreateTopPill(
                out _essenceText, "Essence",
                new Vector2(22f, -18f), new Vector2(320f, 76f),
                TextAnchor.MiddleLeft);
            CreateTopPill(
                out _waveText, "Wave",
                new Vector2(0f, -18f), new Vector2(250f, 94f),
                TextAnchor.MiddleCenter, 0.5f);
            CreateTopPill(
                out _hpText, "HP",
                new Vector2(-22f, -18f), new Vector2(235f, 76f),
                TextAnchor.MiddleRight, 1f);

            _enemyText = RuntimeArt.CreateText(
                _safe, "EnemyCount", 20, TextAnchor.UpperCenter,
                new Color(0.98f, 0.94f, 0.78f));
            RectTransform er = _enemyText.rectTransform;
            er.anchorMin = new Vector2(0.5f, 1f);
            er.anchorMax = new Vector2(0.5f, 1f);
            er.pivot = new Vector2(0.5f, 1f);
            er.sizeDelta = new Vector2(250f, 30f);
            er.anchoredPosition = new Vector2(0f, -122f);
            _enemyText.fontStyle = FontStyle.Bold;
            Shadow enemyShadow = _enemyText.gameObject.AddComponent<Shadow>();
            enemyShadow.effectColor = new Color(0f, 0f, 0f, 0.68f);
            enemyShadow.effectDistance = new Vector2(2f, -2f);

            _waveButton = RuntimeArt.CreateButton(
                _safe, "START", new Color(0.88f, 0.30f, 0.055f), 28);
            RectTransform wr = _waveButton.GetComponent<RectTransform>();
            wr.anchorMin = new Vector2(0f, 0f);
            wr.anchorMax = new Vector2(0f, 0f);
            wr.pivot = new Vector2(0f, 0f);
            wr.sizeDelta = new Vector2(310f, 108f);
            wr.anchoredPosition = new Vector2(26f, 26f);
            _waveButtonText = _waveButton.GetComponentInChildren<Text>();
            _waveButton.onClick.AddListener(delegate { _session.Waves.StartNextWave(); });

            GameObject hintPanel = new GameObject(
                "HintPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline), typeof(Shadow));
            hintPanel.transform.SetParent(_safe, false);
            RectTransform hpr = hintPanel.GetComponent<RectTransform>();
            hpr.anchorMin = hpr.anchorMax = new Vector2(1f, 0f);
            hpr.pivot = new Vector2(1f, 0f);
            hpr.sizeDelta = new Vector2(520f, 72f);
            hpr.anchoredPosition = new Vector2(-22f, 22f);
            Image hintBg = hintPanel.GetComponent<Image>();
            hintBg.color = new Color(0.055f, 0.105f, 0.050f, 0.90f);
            Outline hintOutline = hintPanel.GetComponent<Outline>();
            hintOutline.effectColor = new Color(0.76f, 0.62f, 0.20f, 0.90f);
            hintOutline.effectDistance = new Vector2(3f, -3f);
            Shadow hintPanelShadow = hintPanel.GetComponent<Shadow>();
            hintPanelShadow.effectColor = new Color(0f, 0f, 0f, 0.56f);
            hintPanelShadow.effectDistance = new Vector2(5f, -5f);

            Text hint = RuntimeArt.CreateText(
                hintPanel.transform, "Hint", 20, TextAnchor.MiddleRight,
                new Color(1f, 0.98f, 0.88f, 0.96f));
            hint.rectTransform.anchorMin = Vector2.zero;
            hint.rectTransform.anchorMax = Vector2.one;
            hint.rectTransform.offsetMin = new Vector2(22f, 8f);
            hint.rectTransform.offsetMax = new Vector2(-22f, -8f);
            hint.text = (_session.MapDefinition != null ? _session.MapDefinition.DisplayName + "\n" : "") +
                "Dotknij grządki, aby posadzić obrońcę.";
            hint.fontStyle = FontStyle.Bold;
            Shadow hintShadow = hint.gameObject.AddComponent<Shadow>();
            hintShadow.effectColor = new Color(0f, 0f, 0f, 0.76f);
            hintShadow.effectDistance = new Vector2(2f, -2f);
        }

        private void CreateTopShade()
        {
            GameObject shadeGo = new GameObject(
                "HudTopShade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadeGo.transform.SetParent(_safe, false);
            RectTransform rt = shadeGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 104f);
            rt.anchoredPosition = Vector2.zero;

            Image image = shadeGo.GetComponent<Image>();
            image.sprite = null;
            image.color = new Color(0.025f, 0.055f, 0.025f, 0.36f);
            image.raycastTarget = false;
        }

        private void ClampRadialToSafeArea()
        {
            if (_safe == null || _radialRoot == null) return;

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_safe, _radialRoot);
            Rect safeRect = _safe.rect;
            Vector2 shift = Vector2.zero;

            if (bounds.min.x < safeRect.xMin) shift.x += safeRect.xMin - bounds.min.x;
            if (bounds.max.x > safeRect.xMax) shift.x -= bounds.max.x - safeRect.xMax;
            if (bounds.min.y < safeRect.yMin) shift.y += safeRect.yMin - bounds.min.y;
            if (bounds.max.y > safeRect.yMax) shift.y -= bounds.max.y - safeRect.yMax;

            _radialRoot.anchoredPosition += shift;
        }

        private void CreateTopPill(
            out Text text, string name, Vector2 pos, Vector2 size,
            TextAnchor align, float anchorX = 0f)
        {
            GameObject panel = new GameObject(
                name + "Panel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline));
            panel.transform.SetParent(_safe, false);

            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(anchorX, 1f);
            pr.anchorMax = new Vector2(anchorX, 1f);
            pr.pivot = new Vector2(anchorX, 1f);
            pr.sizeDelta = size;
            pr.anchoredPosition = pos;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.sprite = null;
            panelImage.color = new Color(0.045f, 0.095f, 0.040f, 0.94f);

            Outline outline = panel.GetComponent<Outline>();
            outline.effectColor = new Color(0.80f, 0.66f, 0.22f, 0.94f);
            outline.effectDistance = new Vector2(3f, -3f);

            Shadow panelShadow = panel.AddComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.56f);
            panelShadow.effectDistance = new Vector2(5f, -5f);

            text = RuntimeArt.CreateText(panel.transform, name + "Text", 25, align, Color.white);
            RectTransform tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(28f, 8f);
            tr.offsetMax = new Vector2(-28f, -8f);
            text.fontStyle = FontStyle.Bold;

            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.62f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        private void CreateRadialButton(PlantKind kind, Vector2 offset, Color color)
        {
            int cost = Balance.Cost(kind);
            string label = Balance.DisplayName(kind) + "\n" + cost;
            Button button = RuntimeArt.CreateButton(_radialRoot, label, color, 24);
            button.gameObject.name = "PlantOption_" + kind;

            RectTransform r = button.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(185f, 112f);
            r.anchoredPosition = offset;

            Image portrait = RuntimeArt.CreateArtImage(button.transform, "PlantPortrait", HiggsfieldSpriteFactory.Plant(kind));
            RectTransform portraitRect = portrait.rectTransform;
            portraitRect.anchorMin = new Vector2(0.03f, 0.13f);
            portraitRect.anchorMax = new Vector2(0.31f, 0.87f);
            portraitRect.offsetMin = portraitRect.offsetMax = Vector2.zero;
            Text optionLabel = button.GetComponentInChildren<Text>();
            optionLabel.rectTransform.anchorMin = new Vector2(0.32f, 0f);
            optionLabel.rectTransform.offsetMin = new Vector2(3f, 8f);
            optionLabel.rectTransform.offsetMax = new Vector2(-6f, -8f);

            bool affordable = _session.Economy.Essence >= cost;
            button.interactable = affordable;

            PlantKind captured = kind;
            button.onClick.AddListener(delegate
            {
                if (_onBuild != null) _onBuild(captured);
            });
        }

        private IEnumerator PulseText()
        {
            Color original = _essenceText.color;
            for (float t = 0f; t < 0.45f; t += Time.unscaledDeltaTime)
            {
                _essenceText.color = Color.Lerp(new Color(1f, 0.30f, 0.16f), original, t / 0.45f);
                yield return null;
            }
            _essenceText.color = original;
            _essencePulse = null;
        }

        private IEnumerator FloatingMessageRoutine(Vector2 world, string message, Color color)
        {
            GameObject go = new GameObject(
                "FloatingMessage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(_safe, false);

            Text t = go.GetComponent<Text>();
            t.font = RuntimeArt.Font;
            t.fontSize = 28;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.text = message;
            t.raycastTarget = false;

            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.74f);
            shadow.effectDistance = new Vector2(2f, -2f);

            RectTransform rt = t.rectTransform;
            rt.sizeDelta = new Vector2(320f, 70f);

            Vector3 screen = Camera.main.WorldToScreenPoint(world);
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _safe, new Vector2(screen.x, screen.y), null, out local);
            rt.anchoredPosition = local;

            float duration = 0.9f;
            Vector2 start = rt.anchoredPosition;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float p = elapsed / duration;
                rt.anchoredPosition = start + Vector2.up * 80f * p;
                Color c = color;
                c.a = 1f - p;
                t.color = c;
                yield return null;
            }

            Destroy(go);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
