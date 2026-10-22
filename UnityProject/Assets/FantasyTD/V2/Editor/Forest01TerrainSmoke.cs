#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using FantasyTD.V2.Terrain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace FantasyTD.V2.Editor
{
    public static class Forest01TerrainSmoke
    {
        public const string PassMarker = "FT2 Forest01 Terrain 28x20: PASS";

        [MenuItem("Tools/Fantasy TD V2/Validate Forest01 Isometric Terrain")]
        public static void RunSmoke()
        {
            Forest01TerrainLayout.Validate();
            Require(Forest01TerrainLayout.Width == 28 && Forest01TerrainLayout.Height == 20,
                "Incorrect dimensions");
            Forest01TerrainGenerator.BuildScene();
            Scene scene = EditorSceneManager.OpenScene(Forest01TerrainGenerator.ScenePath,
                OpenSceneMode.Single);
            Require(scene.IsValid() && scene.isLoaded, "Scene did not load");
            Grid grid = UnityEngine.Object.FindFirstObjectByType<Grid>();
            Require(grid != null && grid.cellLayout == GridLayout.CellLayout.Isometric,
                "Missing isometric Grid");
            var ground = GameObject.Find("Ground_560_Tiles").GetComponent<Tilemap>();
            var road = GameObject.Find("Road_Continuous").GetComponent<Tilemap>();
            var water = GameObject.Find("Water_Pond").GetComponent<Tilemap>();
            Require(ground != null && road != null && water != null, "Tilemap missing");
            int groundCount = 0;
            for (int x = 0; x < Forest01TerrainLayout.Width; ++x)
                for (int y = 0; y < Forest01TerrainLayout.Height; ++y)
                {
                    Require(ground.HasTile(new Vector3Int(x, y, 0)),
                        "Missing grass tile " + x + "," + y);
                    groundCount++;
                }
            Require(groundCount == 560, "Not 560 grass cells");
            foreach (var cell in Forest01TerrainLayout.Road)
                Require(road.HasTile(cell.Vector), "Missing road " + cell);
            foreach (var cell in Forest01TerrainLayout.Water)
                Require(water.HasTile(cell.Vector), "Missing water " + cell);
            var buildSites = UnityEngine.Object.FindObjectsByType<Forest01TerrainBuildSite>(
                FindObjectsSortMode.None);
            Require(buildSites.Length == 10, "Exactly 10 separate build objects");
            var ids = new HashSet<string>();
            foreach (var site in buildSites)
            {
                Require(ids.Add(site.SiteId), "Duplicate build site ID");
                Require(site.GetComponent<Collider2D>() != null, "Build site collider missing");
            }
            var interaction = UnityEngine.Object.FindFirstObjectByType<Forest01TerrainInteraction>();
            Require(interaction != null, "Missing build site interaction controller");
            Require(interaction.TrySelect(buildSites[0].transform.position),
                "First build site cannot be selected");
            Require(interaction.SelectedSiteId == buildSites[0].SiteId,
                "Selection controller did not store site ID");
            Require(UnityEngine.Object.FindFirstObjectByType<Camera>() != null, "Camera missing");
            Require(GameObject.Find("Start_Gate") != null &&
                    GameObject.Find("Exit_Gate") != null &&
                    GameObject.Find("Bridge_Decoration") != null, "Scene decorations missing");
            Require(Forest01TerrainGenerator.RenderPreview(scene),
                "Camera preview could not be rendered");
            Debug.Log(PassMarker + " (560 grass, " +
                      Forest01TerrainLayout.Road.Length + " road, 10 sites, preview PNG)");
        }

        private static void Require(bool ok, string text)
        {
            if (!ok) throw new InvalidOperationException("Forest01 Terrain: " + text);
        }
    }
}
#endif
