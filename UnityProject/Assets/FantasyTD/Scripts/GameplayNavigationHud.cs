using FantasyTD.MapA;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.Product
{
    public sealed class GameplayNavigationHud : MonoBehaviour
    {
        public void Initialize(RectTransform safeArea)
        {
            if (safeArea == null || GameObject.Find("Button_MAPY") != null) return;

            Button maps = RuntimeArt.CreateButton(
                safeArea, "MAPY", new Color(0.08f, 0.23f, 0.09f, 0.97f), 24);
            maps.gameObject.name = "Button_MAPY";

            RectTransform rt = maps.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(150f, 64f);
            // Keep navigation in the top HUD lane, to the right of the essence
            // pill. The former second-row placement obscured the MAP1 portal.
            rt.anchoredPosition = new Vector2(356f, -24f);

            maps.onClick.AddListener(ProductFlowScenes.OpenCampaign);
        }
    }
}
