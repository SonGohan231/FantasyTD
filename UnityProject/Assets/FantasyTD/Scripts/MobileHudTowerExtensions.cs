using System;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public sealed class TowerPanelState : MonoBehaviour
    {
        public RectTransform Root;
    }

    public static class MobileHudTowerExtensions
    {
        public static void OpenTowerPanel(
            this MobileHud hud,
            PlantInstance plant,
            Action onUpgrade,
            Action onSell,
            Action onClose)
        {
            if (hud == null || hud.SafeArea == null || plant == null)
                return;

            hud.CloseTowerPanel();

            TowerPanelState state = hud.GetComponent<TowerPanelState>();
            if (state == null)
                state = hud.gameObject.AddComponent<TowerPanelState>();

            GameObject panel = new GameObject(
                "TowerActionPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline));
            panel.transform.SetParent(hud.SafeArea, false);

            RectTransform pr = panel.GetComponent<RectTransform>();
            pr.sizeDelta = new Vector2(520f, 310f);

            Vector3 screen = Camera.main != null
                ? Camera.main.WorldToScreenPoint(plant.transform.position)
                : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                hud.SafeArea,
                new Vector2(screen.x, screen.y),
                null,
                out local);

            pr.anchoredPosition = local + new Vector2(0f, 210f);

            Image image = panel.GetComponent<Image>();
            image.sprite = RuntimeArt.Circle;
            image.color = new Color(0.055f, 0.10f, 0.045f, 0.97f);

            Outline outline = panel.GetComponent<Outline>();
            outline.effectColor = new Color(0.72f, 0.58f, 0.22f, 0.65f);
            outline.effectDistance = new Vector2(3f, -3f);

            Text title = RuntimeArt.CreateText(
                panel.transform,
                "TowerTitle",
                30,
                TextAnchor.MiddleCenter,
                Color.white);
            RectTransform tr = title.rectTransform;
            tr.anchorMin = new Vector2(0.08f, 0.72f);
            tr.anchorMax = new Vector2(0.92f, 0.96f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            title.fontStyle = FontStyle.Bold;
            title.text = Balance.DisplayName(plant.Kind) + "   LV " + plant.Level + "/" + PlantInstance.MaxLevel;

            Text stats = RuntimeArt.CreateText(
                panel.transform,
                "TowerStats",
                22,
                TextAnchor.MiddleCenter,
                new Color(0.94f, 0.93f, 0.82f));
            RectTransform sr = stats.rectTransform;
            sr.anchorMin = new Vector2(0.08f, 0.38f);
            sr.anchorMax = new Vector2(0.92f, 0.73f);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = Vector2.zero;
            stats.text = plant.StatsText();

            Button upgrade = RuntimeArt.CreateButton(
                panel.transform,
                plant.CanUpgrade ? "ULEPSZ\n" + plant.UpgradeCost : "MAX\nPOZIOM",
                new Color(0.28f, 0.58f, 0.18f),
                23);
            upgrade.gameObject.name = "TowerUpgradeButton";
            RectTransform ur = upgrade.GetComponent<RectTransform>();
            ur.anchorMin = new Vector2(0.27f, 0.17f);
            ur.anchorMax = new Vector2(0.27f, 0.17f);
            ur.sizeDelta = new Vector2(205f, 90f);
            upgrade.interactable = plant.CanUpgrade &&
                                   plant.Session != null &&
                                   plant.Session.Economy.Essence >= plant.UpgradeCost;
            if (onUpgrade != null)
                upgrade.onClick.AddListener(delegate { onUpgrade(); });

            Button sell = RuntimeArt.CreateButton(
                panel.transform,
                "SPRZEDAJ\n+" + plant.SellValue,
                new Color(0.64f, 0.30f, 0.12f),
                23);
            sell.gameObject.name = "TowerSellButton";
            RectTransform sellRect = sell.GetComponent<RectTransform>();
            sellRect.anchorMin = new Vector2(0.73f, 0.17f);
            sellRect.anchorMax = new Vector2(0.73f, 0.17f);
            sellRect.sizeDelta = new Vector2(205f, 90f);
            if (onSell != null)
                sell.onClick.AddListener(delegate { onSell(); });

            Button close = RuntimeArt.CreateButton(
                panel.transform,
                "×",
                new Color(0.20f, 0.13f, 0.07f),
                28);
            close.gameObject.name = "TowerPanelCloseButton";
            RectTransform cr = close.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.94f, 0.90f);
            cr.anchorMax = new Vector2(0.94f, 0.90f);
            cr.sizeDelta = new Vector2(60f, 60f);
            if (onClose != null)
                close.onClick.AddListener(delegate { onClose(); });

            state.Root = pr;

            Canvas.ForceUpdateCanvases();
            ClampToSafeArea(hud.SafeArea, pr);
            Canvas.ForceUpdateCanvases();
        }

        public static void CloseTowerPanel(this MobileHud hud)
        {
            if (hud == null) return;
            TowerPanelState state = hud.GetComponent<TowerPanelState>();
            if (state != null && state.Root != null)
                UnityEngine.Object.Destroy(state.Root.gameObject);
            if (state != null)
                state.Root = null;
        }

        private static void ClampToSafeArea(RectTransform safe, RectTransform root)
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(safe, root);
            Rect safeRect = safe.rect;
            Vector2 shift = Vector2.zero;

            if (bounds.min.x < safeRect.xMin) shift.x += safeRect.xMin - bounds.min.x;
            if (bounds.max.x > safeRect.xMax) shift.x -= bounds.max.x - safeRect.xMax;
            if (bounds.min.y < safeRect.yMin) shift.y += safeRect.yMin - bounds.min.y;
            if (bounds.max.y > safeRect.yMax) shift.y -= bounds.max.y - safeRect.yMax;

            root.anchoredPosition += shift;
        }
    }
}
