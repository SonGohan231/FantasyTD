using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class PlantingSpot : MonoBehaviour
    {
        public string Id { get; private set; }
        public float Epc { get; private set; }
        public bool Occupied { get; private set; }
        public PlantKind? Plant { get; private set; }
        public TowerInstance Tower { get; private set; }

        private SpriteRenderer _soil;
        private SpriteRenderer _ring;

        public void Initialize(string id, float epc)
        {
            Id = id;
            Epc = epc;
            _soil = GetComponent<SpriteRenderer>();
            Transform ringTransform = transform.Find("Ring");
            _ring = ringTransform != null ? ringTransform.GetComponent<SpriteRenderer>() : null;
            RefreshVisual();
        }

        public void Occupy(PlantKind kind, TowerInstance tower)
        {
            Occupied = true;
            Plant = kind;
            Tower = tower;
            RefreshVisual();
        }

        public void ClearTower(TowerInstance tower)
        {
            if (Tower != null && tower != null && Tower != tower)
                return;

            Occupied = false;
            Plant = null;
            Tower = null;
            SetSelected(false);
            RefreshVisual();
        }

        public void SetSelected(bool selected)
        {
            if (_ring != null)
            {
                _ring.color = selected
                    ? new Color(1f,0.92f,0.30f,0.96f)
                    : (Occupied
                        ? new Color(0.50f,0.84f,0.24f,0.34f)
                        : new Color(1f,0.86f,0.34f,0.26f));
            }
        }

        private void RefreshVisual()
        {
            if (_soil != null)
            {
                _soil.color = new Color(1f, 1f, 1f, 0f);
            }

            if (_ring != null)
            {
                _ring.color = Occupied
                    ? new Color(0.50f,0.84f,0.24f,0.34f)
                    : new Color(1f,0.86f,0.34f,0.26f);
            }
        }
    }
}
