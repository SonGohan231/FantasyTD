using System;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public sealed class TowerPanelHud : MonoBehaviour
    {
        private GameSession _session;
        private RectTransform _safe;
        private RectTransform _panel;
        private TowerInstance _tower;
        private Text _title;
        private Text _level;
        private Text _stats;
        private Text _upgradeLabel;
        private Text _sellLabel;
        private Button _upgradeButton;
        private Button _sellButton;
        private Action _onUpgrade;
        private Action _onSell;
        private Action _onClose;

        public bool IsOpen => _panel != null && _tower != null;
        public TowerInstance CurrentTower => _tower;

        public void Initialize(GameSession session, RectTransform safeArea)
        {
            _session = session;
            _safe = safeArea;
        }

        public void Open(
            TowerInstance tower,
            Action onUpgrade,
            Action onSell,
            Action onClose)
        {
            Close();
            if (tower == null || _safe == null) return;

            _tower = tower;
            _onUpgrade = onUpgrade;
            _onSell = onSell;
            _onClose = onClose;

            GameObject panelGo = new GameObject(
                "TowerPanel",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline));
            panelGo.transform.SetParent(_safe, false);
            _panel = panelGo.GetComponent<RectTransform>();
            _panel.anchorMin = new Vector2(1f, 0f);
            _panel.anchorMax = new Vector2(1f, 0f);
            _panel.pivot = new Vector2(1f, 0f);
            _panel.sizeDelta = new Vector2(560f, 340f);
            _panel.anchoredPosition = new Vector2(-28f, 28f);

            Image bg = panelGo.GetComponent<Image>();
            bg.sprite = RuntimeArt.Circle;
            bg.color = new Color(0.045f, 0.085f, 0.035f, 0.97f);
            bg.raycastTarget = true;

            Outline outline = panelGo.GetComponent<Outline>();
            outline.effectColor = new Color(0.78f, 0.66f, 0.25f, 0.62f);
            outline.effectDistance = new Vector2(3f, -3f);

            _title = RuntimeArt.CreateText(panelGo.transform, "TowerName", 34, TextAnchor.MiddleLeft, Color.white);
            SetRect(_title.rectTransform, new Vector2(0.07f, 0.73f), new Vector2(0.74f, 0.94f));

            _level = RuntimeArt.CreateText(panelGo.transform, "TowerLevel", 27, TextAnchor.MiddleRight, new Color(1f, 0.84f, 0.28f));
            SetRect(_level.rectTransform, new Vector2(0.70f, 0.73f), new Vector2(0.93f, 0.94f));

            _stats = RuntimeArt.CreateText(panelGo.transform, "TowerStats", 25, TextAnchor.UpperLeft, new Color(0.94f, 0.96f, 0.87f));
            SetRect(_stats.rectTransform, new Vector2(0.07f, 0.42f), new Vector2(0.93f, 0.73f));
            _stats.horizontalOverflow = HorizontalWrapMode.Wrap;
            _stats.verticalOverflow = VerticalWrapMode.Overflow;

            _upgradeButton = RuntimeArt.CreateButton(
                panelGo.transform,
                "ULEPSZ",
                new Color(0.34f, 0.60f, 0.16f),
                26);
            RectTransform ur = _upgradeButton.GetComponent<RectTransform>();
            ur.anchorMin = new Vector2(0.06f, 0.08f);
            ur.anchorMax = new Vector2(0.53f, 0.34f);
            ur.offsetMin = Vector2.zero;
            ur.offsetMax = Vector2.zero;
            _upgradeLabel = _upgradeButton.GetComponentInChildren<Text>();
            _upgradeButton.onClick.AddListener(delegate
            {
                if (_tower == null) return;
                if (_onUpgrade != null) _onUpgrade();
                Refresh();
            });

            _sellButton = RuntimeArt.CreateButton(
                panelGo.transform,
                "SPRZEDAJ",
                new Color(0.58f, 0.24f, 0.10f),
                26);
            RectTransform sr = _sellButton.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0.55f, 0.08f);
            sr.anchorMax = new Vector2(0.92f, 0.34f);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = Vector2.zero;
            _sellLabel = _sellButton.GetComponentInChildren<Text>();
            _sellButton.onClick.AddListener(delegate
            {
                if (_tower == null) return;
                if (_onSell != null) _onSell();
                Close();
            });

            Button close = RuntimeArt.CreateButton(
                panelGo.transform,
                "×",
                new Color(0.17f, 0.12f, 0.06f, 0.96f),
                30);
            close.gameObject.name = "TowerPanelClose";
            RectTransform cr = close.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.92f, 0.83f);
            cr.anchorMax = new Vector2(0.92f, 0.83f);
            cr.sizeDelta = new Vector2(66f, 66f);
            close.onClick.AddListener(delegate
            {
                if (_onClose != null) _onClose();
            });

            Refresh();
        }

        public void Refresh()
        {
            if (_tower == null || _panel == null)
            {
                if (_panel != null) Close();
                return;
            }

            _title.text = Balance.DisplayName(_tower.Kind);
            _level.text = "POZIOM " + _tower.Level + "/3";
            _stats.text =
                _tower.StatsSummary +
                "\nZainwestowano: " + _tower.TotalInvested + " Esencji";

            if (_tower.CanUpgrade)
            {
                int cost = _tower.NextUpgradeCost;
                _upgradeLabel.text = "ULEPSZ\n" + cost;
                _upgradeButton.interactable = _session != null && _session.Economy.Essence >= cost;
            }
            else
            {
                _upgradeLabel.text = "MAX";
                _upgradeButton.interactable = false;
            }

            _sellLabel.text = "SPRZEDAJ\n+" + _tower.SellRefund;
        }

        public void Close()
        {
            if (_panel != null)
                Destroy(_panel.gameObject);

            _panel = null;
            _tower = null;
            _title = null;
            _level = null;
            _stats = null;
            _upgradeLabel = null;
            _sellLabel = null;
            _upgradeButton = null;
            _sellButton = null;
            _onUpgrade = null;
            _onSell = null;
            _onClose = null;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
