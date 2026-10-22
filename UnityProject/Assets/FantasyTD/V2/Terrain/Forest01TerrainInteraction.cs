using UnityEngine;

namespace FantasyTD.V2.Terrain
{
    /// <summary>Touch/mouse selection of terrain prototype build sites, without UI or battle mutations.</summary>
    public sealed class Forest01TerrainInteraction : MonoBehaviour
    {
        [SerializeField] private string selectedSiteId;
        public string SelectedSiteId => selectedSiteId;

        private void Update()
        {
            Vector3 screen;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Began) return;
                screen = touch.position;
            }
            else
            {
                if (!Input.GetMouseButtonDown(0)) return;
                screen = Input.mousePosition;
            }

            Camera camera = Camera.main;
            if (camera == null) return;
            Vector3 position = camera.ScreenToWorldPoint(screen);
            TrySelect(new Vector2(position.x, position.y));
        }

        public bool TrySelect(Vector2 worldPosition)
        {
            Forest01TerrainBuildSite[] sites =
                GetComponentsInChildren<Forest01TerrainBuildSite>(true);
            Forest01TerrainBuildSite chosen = null;
            float distance = .48f;
            foreach (Forest01TerrainBuildSite site in sites)
            {
                float d = Vector2.Distance(worldPosition, site.transform.position);
                if (d < distance)
                {
                    chosen = site;
                    distance = d;
                }
            }
            if (chosen == null) return false;
            foreach (Forest01TerrainBuildSite site in sites)
                site.SetSelected(site == chosen);
            selectedSiteId = chosen.SiteId;
            Debug.Log("FT2 terrain selected build site " + selectedSiteId);
            return true;
        }
    }
}
