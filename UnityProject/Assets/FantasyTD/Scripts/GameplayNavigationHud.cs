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
                safeArea, "MAPY", new Color(0.16f, 0.28f, 0.14f, 0.96f), 22);
            maps.gameObject.name = "Button_MAPY";

            RectTransform rt = maps.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(180f, 66f);
            rt.anchoredPosition = new Vector2(32f, -122f);

            maps.onClick.AddListener(ProductFlowScenes.OpenCampaign);
        }
    }
}
