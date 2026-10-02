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
            _waveButtonText.text = _session.Waves.CurrentWave == 0 ? "ROZPOCZNIJ\n1. FALĘ" : "NASTĘPNA\nFALA";
        }

        public void OpenBuildRadial(PlantingSpot spot, Action<PlantKind> onBuild, Action onClose)
        {
            _onBuild = onBuild;
            _onClose = onClose;
            CloseBuildRadial();

            GameObject root = new GameObject("RadialBuildMenu", typeof(RectTransform));
            root.transform.SetParent(_safe,false);
            _radialRoot = root.GetComponent<RectTransform>();
            _radialRoot.sizeDelta = Vector2.zero;

            Vector2 local;
            Vector3 screen = Camera.main.WorldToScreenPoint(spot.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_safe,new Vector2(screen.x,screen.y),null,out local);

            float maxX = _safe.rect.width * 0.5f - 170f;
            float maxY = _safe.rect.height * 0.5f - 170f;
            local.x = Mathf.Clamp(local.x,-maxX,maxX);
            local.y = Mathf.Clamp(local.y,-maxY,maxY);
            _radialRoot.anchoredPosition = local;

            CreateRadialButton(PlantKind.SeedShooter,new Vector2(-125f,0f),new Color(0.28f,0.64f,0.20f));
            CreateRadialButton(PlantKind.EssenceFlower,new Vector2(0f,125f),new Color(0.92f,0.64f,0.12f));
            CreateRadialButton(PlantKind.OakGuardian,new Vector2(125f,0f),new Color(0.42f,0.26f,0.10f));
            CreateRadialButton(PlantKind.FrostBud,new Vector2(0f,-125f),new Color(0.24f,0.62f,0.82f));

            Button close = RuntimeArt.CreateButton(_radialRoot,"×",new Color(0.25f,0.16f,0.10f,0.95f),38);
            RectTransform cr = close.GetComponent<RectTransform>();
            cr.sizeDelta = new Vector2(74f,74f);
            cr.anchoredPosition = Vector2.zero;
            close.onClick.AddListener(delegate {
                if (_onClose != null) _onClose();
            });
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
            StartCoroutine(FloatingMessageRoutine(world,message,color));
        }

        public void ShowEndPanel(bool victory)
        {
            CloseBuildRadial();
            GameObject panel = new GameObject("EndPanel",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            panel.transform.SetParent(_safe,false);
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(0.5f,0.5f);
            pr.anchorMax = new Vector2(0.5f,0.5f);
            pr.sizeDelta = new Vector2(760f,400f);
            panel.GetComponent<Image>().color = new Color(0.12f,0.08f,0.04f,0.94f);

            Text title = RuntimeArt.CreateText(panel.transform,"Result",54,TextAnchor.MiddleCenter,Color.white);
            title.rectTransform.anchorMin = new Vector2(0.08f,0.38f);
            title.rectTransform.anchorMax = new Vector2(0.92f,0.90f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;
            title.text = victory ? "ZIELONY TRAKT OBRONIONY!" : "KRÓL CIERNI PRZEDARŁ SIĘ DO BAZY";

            Button restart = RuntimeArt.CreateButton(panel.transform,"ZAGRAJ PONOWNIE",new Color(0.28f,0.58f,0.20f),28);
            RectTransform rr = restart.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(0.5f,0.18f);
            rr.anchorMax = new Vector2(0.5f,0.18f);
            rr.sizeDelta = new Vector2(360f,100f);
            restart.onClick.AddListener(delegate {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }

        private void BuildCanvas()
        {
            GameObject canvasGo = new GameObject("MobileCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f,1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject safeGo = new GameObject("SafeArea",typeof(RectTransform),typeof(SafeAreaFitter));
            safeGo.transform.SetParent(canvasGo.transform,false);
            _safe = safeGo.GetComponent<RectTransform>();
            _safe.anchorMin = Vector2.zero;
            _safe.anchorMax = Vector2.one;
            _safe.offsetMin = Vector2.zero;
            _safe.offsetMax = Vector2.zero;

            CreateTopPill(out _essenceText,"Essence",new Vector2(30f,-28f),new Vector2(340f,82f),TextAnchor.MiddleLeft);
            CreateTopPill(out _waveText,"Wave",new Vector2(0f,-28f),new Vector2(270f,82f),TextAnchor.MiddleCenter,0.5f);
            CreateTopPill(out _hpText,"HP",new Vector2(-30f,-28f),new Vector2(260f,82f),TextAnchor.MiddleRight,1f);

            _enemyText = RuntimeArt.CreateText(_safe,"EnemyCount",24,TextAnchor.UpperCenter,new Color(0.94f,0.92f,0.80f));
            RectTransform er = _enemyText.rectTransform;
            er.anchorMin = new Vector2(0.5f,1f);
            er.anchorMax = new Vector2(0.5f,1f);
            er.pivot = new Vector2(0.5f,1f);
            er.sizeDelta = new Vector2(400f,50f);
            er.anchoredPosition = new Vector2(0f,-106f);

            _waveButton = RuntimeArt.CreateButton(_safe,"START",new Color(0.76f,0.34f,0.12f),30);
            RectTransform wr = _waveButton.GetComponent<RectTransform>();
            wr.anchorMin = new Vector2(0f,0f);
            wr.anchorMax = new Vector2(0f,0f);
            wr.pivot = new Vector2(0f,0f);
            wr.sizeDelta = new Vector2(300f,126f);
            wr.anchoredPosition = new Vector2(34f,34f);
            _waveButtonText = _waveButton.GetComponentInChildren<Text>();
            _waveButton.onClick.AddListener(delegate { _session.Waves.StartNextWave(); });

            Text hint = RuntimeArt.CreateText(_safe,"Hint",24,TextAnchor.LowerRight,new Color(1f,1f,1f,0.82f));
            RectTransform hr = hint.rectTransform;
            hr.anchorMin = new Vector2(1f,0f);
            hr.anchorMax = new Vector2(1f,0f);
            hr.pivot = new Vector2(1f,0f);
            hr.sizeDelta = new Vector2(600f,100f);
            hr.anchoredPosition = new Vector2(-28f,30f);
            hint.text = "Dotknij kręgu ziemi, aby posadzić obrońcę.";
        }

        private void CreateTopPill(out Text text, string name, Vector2 pos, Vector2 size, TextAnchor align, float anchorX = 0f)
        {
            GameObject panel = new GameObject(name + "Panel",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            panel.transform.SetParent(_safe,false);
            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.anchorMin = new Vector2(anchorX,1f);
            pr.anchorMax = new Vector2(anchorX,1f);
            pr.pivot = new Vector2(anchorX,1f);
            pr.sizeDelta = size;
            pr.anchoredPosition = pos;
            panel.GetComponent<Image>().sprite = RuntimeArt.Circle;
            panel.GetComponent<Image>().color = new Color(0.10f,0.07f,0.035f,0.87f);

            text = RuntimeArt.CreateText(panel.transform,name+"Text",30,align,Color.white);
            RectTransform tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(28f,8f);
            tr.offsetMax = new Vector2(-28f,-8f);
        }

        private void CreateRadialButton(PlantKind kind, Vector2 offset, Color color)
        {
            int cost = Balance.Cost(kind);
            string label = Balance.DisplayName(kind) + "\n" + cost;
            Button button = RuntimeArt.CreateButton(_radialRoot,label,color,24);
            RectTransform r = button.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(185f,112f);
            r.anchoredPosition = offset;

            bool affordable = _session.Economy.Essence >= cost;
            button.interactable = affordable;

            PlantKind captured = kind;
            button.onClick.AddListener(delegate {
                if (_onBuild != null) _onBuild(captured);
            });
        }

        private IEnumerator PulseText()
        {
            Color original = _essenceText.color;
            for (float t=0f;t<0.45f;t+=Time.unscaledDeltaTime)
            {
                _essenceText.color = Color.Lerp(Color.red,original,t/0.45f);
                yield return null;
            }
            _essenceText.color = original;
            _essencePulse = null;
        }

        private IEnumerator FloatingMessageRoutine(Vector2 world, string message, Color color)
        {
            GameObject go = new GameObject("FloatingMessage",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
            go.transform.SetParent(_safe,false);
            Text t = go.GetComponent<Text>();
            t.font = RuntimeArt.Font;
            t.fontSize = 28;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.text = message;
            t.raycastTarget = false;

            RectTransform rt = t.rectTransform;
            rt.sizeDelta = new Vector2(320f,70f);

            Vector3 screen = Camera.main.WorldToScreenPoint(world);
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_safe,new Vector2(screen.x,screen.y),null,out local);
            rt.anchoredPosition = local;

            float duration = 0.9f;
            Vector2 start = rt.anchoredPosition;
            for (float elapsed=0f; elapsed<duration; elapsed+=Time.unscaledDeltaTime)
            {
                float p = elapsed/duration;
                rt.anchoredPosition = start + Vector2.up * 80f * p;
                Color c = color;
                c.a = 1f-p;
                t.color = c;
                yield return null;
            }
            Destroy(go);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            GameObject es = new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        }
    }
}
