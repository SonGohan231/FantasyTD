using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class PlantingSpot : MonoBehaviour
    {
        public string Id { get; private set; }
        public float Epc { get; private set; }
        public bool Occupied { get; private set; }
        public PlantKind? Plant { get; private set; }

        private SpriteRenderer _soil;
        private SpriteRenderer _ring;

        public void Initialize(string id, float epc)
        {
            Id = id;
            Epc = epc;
            _soil = GetComponent<SpriteRenderer>();
            _ring = transform.Find("Ring").GetComponent<SpriteRenderer>();
            RefreshVisual();
        }

        public void Occupy(PlantKind kind)
        {
            Occupied = true;
            Plant = kind;
            RefreshVisual();
        }

        public void SetSelected(bool selected)
        {
            if (_ring != null)
            {
                _ring.color = selected
                    ? new Color(1f,0.94f,0.36f,0.95f)
                    : new Color(0.87f,0.74f,0.40f,0.58f);
            }
        }

        private void RefreshVisual()
        {
            if (_soil == null) return;
            _soil.color = Occupied
                ? new Color(0.28f,0.18f,0.08f,0.45f)
                : new Color(0.43f,0.28f,0.12f,0.92f);
        }
    }
}
