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
        private LineRenderer _rangeIndicator;

        public PlantKind Kind { get; private set; }
        public int Level { get; private set; } = 1;
        public int TotalInvested { get; private set; }

        public PlantingSpot Spot => _spot;
        public bool CanUpgrade => Level < TowerUpgradeBalance.MaxLevel;
        public int NextUpgradeCost => CanUpgrade ? TowerUpgradeBalance.UpgradeCost(Kind, Level) : 0;
        public int SellRefund => TowerUpgradeBalance.SellRefund(TotalInvested);
        public string StatsSummary => TowerUpgradeBalance.StatsSummary(Kind, Level);
        public float AttackRange => TowerUpgradeBalance.AttackRange(Kind, Level);

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
            if (_rangeIndicator != null && _rangeIndicator.enabled)
                RefreshRangeIndicator();
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

        public void SetSelectedVisual(bool selected)
        {
            if (!selected || AttackRange <= 0f)
            {
                if (_rangeIndicator != null)
                    _rangeIndicator.enabled = false;
                return;
            }

            if (_rangeIndicator == null)
            {
                GameObject ring = new GameObject("AttackRangeIndicator");
                ring.transform.SetParent(transform, false);
                _rangeIndicator = ring.AddComponent<LineRenderer>();
                _rangeIndicator.useWorldSpace = false;
                _rangeIndicator.loop = true;
                _rangeIndicator.positionCount = 65;
                _rangeIndicator.startWidth = 0.065f;
                _rangeIndicator.endWidth = 0.065f;
                _rangeIndicator.numCornerVertices = 2;
                _rangeIndicator.numCapVertices = 2;
                Color c = new Color(0.78f, 0.96f, 0.55f, 0.72f);
                _rangeIndicator.material = RuntimeArt.SpriteMaterial(c);
                _rangeIndicator.startColor = c;
                _rangeIndicator.endColor = c;
                _rangeIndicator.sortingOrder = 12;
            }

            RefreshRangeIndicator();
            _rangeIndicator.enabled = true;
        }

        private void RefreshRangeIndicator()
        {
            if (_rangeIndicator == null) return;
            float radius = AttackRange;
            for (int i = 0; i < _rangeIndicator.positionCount; i++)
            {
                float angle = (Mathf.PI * 2f * i) / (_rangeIndicator.positionCount - 1);
                _rangeIndicator.SetPosition(i, new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f));
            }
        }

        private void RefreshLevelMarkers()
        {
            if (_levelMarkers != null)
                Destroy(_levelMarkers.gameObject);
            _levelMarkers = null;

            if (Level <= 1)
                return;

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
