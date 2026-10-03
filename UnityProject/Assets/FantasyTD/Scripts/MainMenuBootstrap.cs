using FantasyTD.MapA;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.Product
{
    [DefaultExecutionOrder(-900)]
    public sealed class MainMenuBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            ConfigureMobile();
            EnsureEventSystem();
            BuildUi();
        }

        private static void ConfigureMobile()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void BuildUi()
        {
            GameObject canvasGo = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Texture2D mapTexture = Resources.Load<Texture2D>("Higgsfield/MAP_A");
            if (mapTexture != null)
            {
                GameObject bgGo = new GameObject("HiggsfieldMenuBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                bgGo.transform.SetParent(canvasGo.transform, false);
                RectTransform bg = bgGo.GetComponent<RectTransform>();
                Stretch(bg);
                RawImage raw = bgGo.GetComponent<RawImage>();
                raw.texture = mapTexture;
                raw.color = new Color(0.78f, 0.86f, 0.78f, 1f);
                raw.raycastTarget = false;
            }

            GameObject shadeGo = new GameObject("MenuShade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadeGo.transform.SetParent(canvasGo.transform, false);
            RectTransform shadeRt = shadeGo.GetComponent<RectTransform>();
            Stretch(shadeRt);
            Image shade = shadeGo.GetComponent<Image>();
            shade.sprite = null;
            shade.color = new Color(0.015f, 0.035f, 0.018f, 0.68f);
            shade.raycastTarget = false;

            GameObject safeGo = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            safeGo.transform.SetParent(canvasGo.transform, false);
            RectTransform safe = safeGo.GetComponent<RectTransform>();
            Stretch(safe);

            Text title = RuntimeArt.CreateText(safe, "Title", 96, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.55f));
            SetRect(title.rectTransform, new Vector2(0.18f, 0.58f), new Vector2(0.82f, 0.80f));
            title.text = "FANTASY TD";
            title.fontStyle = FontStyle.Bold;

            Text subtitle = RuntimeArt.CreateText(safe, "Subtitle", 32, TextAnchor.MiddleCenter, new Color(0.92f, 0.96f, 0.86f));
            SetRect(subtitle.rectTransform, new Vector2(0.20f, 0.48f), new Vector2(0.80f, 0.60f));
            subtitle.text = "Obroń żywy świat przed Królem Cierni";

            Button play = RuntimeArt.CreateButton(safe, "GRAJ", new Color(0.30f, 0.62f, 0.18f), 42);
            play.gameObject.name = "Button_GRAJ";
            RectTransform pr = play.GetComponent<RectTransform>();
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.30f);
            pr.pivot = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(520f, 150f);
            play.onClick.AddListener(ProductFlowScenes.OpenCampaign);

            Text footer = RuntimeArt.CreateText(safe, "Footer", 22, TextAnchor.MiddleCenter, new Color(0.82f, 0.88f, 0.76f, 0.82f));
            SetRect(footer.rectTransform, new Vector2(0.25f, 0.05f), new Vector2(0.75f, 0.13f));
            footer.text = "Kampania • Wieże • Ulepszenia • Bossowie";
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
