using FantasyTD.MapA;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.Product
{
    [DefaultExecutionOrder(-900)]
    public sealed class CampaignMapSelectBootstrap : MonoBehaviour
    {
        private static readonly string[] MapNames =
        {
            "ZIELONY TRAKT", "ODZYSKANE RUINY", "ZMIERZCH", "DNO OCEANICZNE",
            "GRZYBOWE MIASTO", "LAS BAMBUSOWY", "ŚWIĄTYNIA SŁOŃCA I KSIĘŻYCA", "LOCHY SMOKA"
        };

        private void Awake()
        {
            EnsureEventSystem();
            BuildUi();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void BuildUi()
        {
            GameObject canvasGo = new GameObject("CampaignCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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
                GameObject bgGo = new GameObject("CampaignBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
                bgGo.transform.SetParent(canvasGo.transform, false);
                RectTransform bg = bgGo.GetComponent<RectTransform>();
                Stretch(bg);
                RawImage raw = bgGo.GetComponent<RawImage>();
                raw.texture = mapTexture;
                raw.color = new Color(0.58f, 0.66f, 0.58f, 1f);
                raw.raycastTarget = false;
            }

            GameObject shadeGo = new GameObject("CampaignShade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shadeGo.transform.SetParent(canvasGo.transform, false);
            RectTransform shadeRt = shadeGo.GetComponent<RectTransform>();
            Stretch(shadeRt);
            Image shade = shadeGo.GetComponent<Image>();
            shade.sprite = null;
            shade.color = new Color(0.018f, 0.040f, 0.020f, 0.78f);
            shade.raycastTarget = false;

            GameObject safeGo = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            safeGo.transform.SetParent(canvasGo.transform, false);
            RectTransform safe = safeGo.GetComponent<RectTransform>();
            Stretch(safe);

            Text title = RuntimeArt.CreateText(safe, "CampaignTitle", 62, TextAnchor.MiddleCenter, new Color(1f, 0.91f, 0.48f));
            SetRect(title.rectTransform, new Vector2(0.20f, 0.86f), new Vector2(0.80f, 0.98f));
            title.text = "WYBIERZ MAPĘ";
            title.fontStyle = FontStyle.Bold;

            Button back = RuntimeArt.CreateButton(safe, "WSTECZ", new Color(0.20f, 0.30f, 0.17f), 26);
            back.gameObject.name = "Button_WSTECZ";
            RectTransform br = back.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = new Vector2(0.03f, 0.90f);
            br.pivot = new Vector2(0f, 0.5f);
            br.sizeDelta = new Vector2(230f, 88f);
            back.onClick.AddListener(ProductFlowScenes.OpenMainMenu);

            const float left = 0.07f, right = 0.93f, bottom = 0.08f, top = 0.82f;
            const float gapX = 0.025f, gapY = 0.04f;
            float cellW = (right - left - gapX * 3f) / 4f;
            float cellH = (top - bottom - gapY) / 2f;

            for (int i = 0; i < MapNames.Length; i++)
            {
                int col = i % 4;
                int row = i / 4;
                float x0 = left + col * (cellW + gapX);
                float y1 = top - row * (cellH + gapY);
                float y0 = y1 - cellH;
                CampaignMapDefinition definition = i == 0 ? CampaignMapDefinition.Map1 : (i == 1 ? CampaignMapDefinition.Map2 : null);
                bool validated = definition != null && definition.IsValidatedPlayable;
                bool unlocked = definition != null && CampaignProgress.IsUnlocked(definition.Id);
                bool playable = validated && unlocked;

                string label = (i + 1) + ". " + MapNames[i] + (playable ? "\nGRAJ" : (validated ? "\nUKOŃCZ POPRZEDNIĄ MAPĘ" : "\nW PRODUKCJI"));
                Button button = RuntimeArt.CreateButton(
                    safe, label,
                    playable ? new Color(0.30f, 0.58f, 0.18f) : new Color(0.16f, 0.20f, 0.14f),
                    25);
                button.gameObject.name = "MapButton_MAP" + (i + 1);
                RectTransform rt = button.GetComponent<RectTransform>();
                SetRect(rt, new Vector2(x0, y0), new Vector2(x0 + cellW, y1));
                button.interactable = playable;
                if (playable)
                {
                    string sceneName = definition.SceneName;
                    button.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName));
                }
            }
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
