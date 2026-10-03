using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public sealed class TowerInstance : MonoBehaviour
    {
        private GameSession _session;
        private PlantingSpot _spot;
        private PlantBehaviour _behaviour;
        private Transform _levelMarkers;

        public PlantKind Kind { get; private set; }
        public int Level { get; private set; } = 1;
        public int TotalInvested { get; private set; }

        public PlantingSpot Spot => _spot;
        public bool CanUpgrade => Level < TowerUpgradeBalance.MaxLevel;
        public int NextUpgradeCost => CanUpgrade ? TowerUpgradeBalance.UpgradeCost(Kind, Level) : 0;
        public int SellRefund => TowerUpgradeBalance.SellRefund(TotalInvested);
        public string StatsSummary => TowerUpgradeBalance.StatsSummary(Kind, Level);

        public void Initialize(
            GameSession session,
            PlantingSpot spot,
            PlantKind kind,
            PlantBehaviour behaviour,
            int initialCost)
        {
            _session = session;
            _spot = spot;
            Kind = kind;
            _behaviour = behaviour;
            Level = 1;
            TotalInvested = Mathf.Max(0, initialCost);
            RefreshLevelMarkers();
        }

        public bool TryUpgrade()
        {
            if (_session == null || !CanUpgrade)
                return false;

            int cost = NextUpgradeCost;
            if (!_session.Economy.TrySpend(cost))
                return false;

            int previousLevel = Level;
            Level++;
            TotalInvested += cost;

            if (_behaviour != null)
                _behaviour.OnTowerLevelChanged(previousLevel, Level);

            RefreshLevelMarkers();
            return true;
        }

        public void Sell()
        {
            if (_session == null) return;

            int refund = SellRefund;
            if (refund > 0)
                _session.Economy.Add(refund);

            if (_spot != null)
                _spot.ClearTower(this);

            Destroy(gameObject);
        }

        private void RefreshLevelMarkers()
        {
            if (_levelMarkers != null)
                Destroy(_levelMarkers.gameObject);

            GameObject root = new GameObject("LevelMarkers", typeof(RectTransform), typeof(Canvas));
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, 1.16f, 0f);
            root.transform.localScale = Vector3.one * 0.01f;
            _levelMarkers = root.transform;
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 90;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(84f, 28f);

            Text label = RuntimeArt.CreateText(root.transform, "TowerLevelLabel", 22, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.35f));
            label.text = "L" + Level;
            label.fontStyle = FontStyle.Bold;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            Shadow shadow = label.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }
}
