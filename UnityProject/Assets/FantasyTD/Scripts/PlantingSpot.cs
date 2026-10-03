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

        public bool IsSelected { get; private set; }

        public void Initialize(string id, float epc)
        {
            Id = id;
            Epc = epc;
            IsSelected = false;
        }

        public void Occupy(PlantKind kind, TowerInstance tower)
        {
            Occupied = true;
            Plant = kind;
            Tower = tower;
        }

        public void ClearTower(TowerInstance tower)
        {
            if (Tower != null && tower != null && Tower != tower)
                return;

            Occupied = false;
            Plant = null;
            Tower = null;
            SetSelected(false);
        }

        // The build/tower menu communicates selection. Soil patches are part
        // of the authored map texture and are never recoloured or drawn here.
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
        }
    }
}
