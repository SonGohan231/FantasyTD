using UnityEngine;

namespace FantasyTD.V2.Terrain
{
    /// <summary>Individually editable target for terrain-only selection; combat remains unchanged.</summary>
    public sealed class Forest01TerrainBuildSite : MonoBehaviour
    {
        [SerializeField] private string siteId;
        [SerializeField] private int cellX;
        [SerializeField] private int cellY;
        [SerializeField] private SpriteRenderer marker;
        [SerializeField] private bool selected;

        public string SiteId => siteId;
        public Vector2Int Cell => new Vector2Int(cellX, cellY);
        public bool IsSelected => selected;

        public void Configure(string id, Forest01TerrainLayout.Cell cell, SpriteRenderer renderer)
        {
            siteId = id;
            cellX = cell.X;
            cellY = cell.Y;
            marker = renderer;
            SetSelected(false);
        }

        public void SetSelected(bool value)
        {
            selected = value;
            if (marker != null)
                marker.color = value ? new Color(1f, .95f, .46f, 1f)
                                     : new Color(.60f, .94f, .69f, .96f);
        }
    }

}
