using UnityEngine;

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

            GameObject root = new GameObject("LevelMarkers");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, 0.68f, 0f);
            _levelMarkers = root.transform;

            float spacing = 0.18f;
            float start = -(Level - 1) * spacing * 0.5f;
            for (int i = 0; i < Level; i++)
            {
                RuntimeArt.SpriteObject(
                    "LevelLeaf_" + (i + 1),
                    _levelMarkers,
                    new Vector2(start + i * spacing, 0f),
                    new Vector2(0.13f, 0.19f),
                    new Color(1f, 0.82f, 0.20f, 0.95f),
                    90,
                    true,
                    RuntimeArt.Leaf,
                    i % 2 == 0 ? -18f : 18f);
            }
        }
    }
}
