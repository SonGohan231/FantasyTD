using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class MapABootstrap : MonoBehaviour
    {
        private readonly List<PlantingSpot> _spots = new List<PlantingSpot>();

        private void Awake()
        {
            ConfigureMobile();
            BuildCamera();
            BuildMap();
            BuildPlantingSpots();

            GameObject sessionObject = new GameObject("GameSession");
            GameSession session = sessionObject.AddComponent<GameSession>();
            session.Initialize(_spots);
        }

        private static void ConfigureMobile()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        private static void BuildCamera()
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            Camera cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            go.AddComponent<MobileCameraFitter>();

            cam.orthographic = true;
            cam.backgroundColor = new Color(0.035f, 0.055f, 0.035f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private static void BuildMap()
        {
            Sprite map = HiggsfieldArt.MapBackground();
            if (map != null)
            {
                FantasyVisualFactory.CreateEnvironmentSprite(
                    "HiggsfieldMapBackground",
                    map,
                    new Vector2(MapAData.Width * 0.5f, MapAData.Height * 0.5f),
                    new Vector2(MapAData.Width, MapAData.Height),
                    -30);
            }
            else
            {
                Debug.LogError(
                    "[T0012][Higgsfield] Topology-locked MAP-A background is missing. " +
                    "Final release is blocked; no procedural road/map fallback is rendered.");
            }

            // Preserve exact route topology for validation and runtime evidence,
            // but do not render a second road over the Higgsfield background.
            CreateLogicalRoad("RoadShadow", 1.28f, -20);
            CreateLogicalRoad("Road", 1.03f, -19);
            CreateLogicalRoad("RoadDust", 0.76f, -18);

            Vector2 spawn = MapAData.Waypoints[0];
            Sprite portal = HiggsfieldArt.SpawnPortal();
            if (portal != null)
            {
                FantasyVisualFactory.CreateEnvironmentSprite(
                    "SpawnPortal",
                    portal,
                    spawn,
                    new Vector2(1.55f, 1.55f),
                    4);
            }
            else
            {
                Debug.LogError("[T0012][Higgsfield] Spawn portal asset is missing.");
            }

            Vector2 home = MapAData.Waypoints[MapAData.Waypoints.Length - 1];
            Sprite homeTree = HiggsfieldArt.HomeTree();
            if (homeTree != null)
            {
                FantasyVisualFactory.CreateEnvironmentSprite(
                    "HomeTree",
                    home,
                    home,
                    new Vector2(2.15f, 2.15f),
                    4);
            }
            else
            {
                Debug.LogError(
                    "[T0012][Higgsfield] HomeTree asset is missing. " +
                    "Final release remains asset-blocked until Higgsfield credits are restored.");
            }
        }

        private void BuildPlantingSpots()
        {
            for (int i = 0; i < MapAData.PlantingSpots.Length; i++)
            {
                PlantingSpotDefinition def = MapAData.PlantingSpots[i];

                GameObject spot = RuntimeArt.SpriteObject(
                    "PlantingSpot_" + def.Id,
                    null,
                    def.Position,
                    new Vector2(1.02f, 0.72f),
                    new Color(0.24f, 0.13f, 0.055f, 0.46f),
                    -3);

                RuntimeArt.SpriteObject(
                    "Ring",
                    spot.transform,
                    Vector2.zero,
                    new Vector2(1.20f, 0.86f),
                    new Color(0.93f, 0.78f, 0.32f, 0.52f),
                    -2,
                    true,
                    RuntimeArt.Ring);

                PlantingSpot plantingSpot = spot.AddComponent<PlantingSpot>();
                plantingSpot.Initialize(def.Id, def.Epc);
                _spots.Add(plantingSpot);
            }
        }

        private static void CreateLogicalRoad(string name, float width, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.hideFlags = HideFlags.None;

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = MapAData.Waypoints.Length;
            lr.useWorldSpace = true;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.numCornerVertices = 14;
            lr.numCapVertices = 14;

            Color transparent = new Color(0f, 0f, 0f, 0f);
            lr.material = RuntimeArt.SpriteMaterial(transparent);
            lr.startColor = transparent;
            lr.endColor = transparent;
            lr.sortingOrder = sortingOrder;

            for (int i = 0; i < MapAData.Waypoints.Length; i++)
            {
                Vector2 p = MapAData.Waypoints[i];
                lr.SetPosition(i, new Vector3(p.x, p.y, 0f));
            }
        }
    }
}
