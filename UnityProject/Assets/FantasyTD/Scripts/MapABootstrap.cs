using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class MapABootstrap : MonoBehaviour
    {
        private const string MapResourcePath = "Higgsfield/MAP_A";
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
            cam.backgroundColor = new Color(0.055f, 0.095f, 0.040f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private static void BuildMap()
        {
            Texture2D mapTexture = Resources.Load<Texture2D>(MapResourcePath);
            if (mapTexture == null)
            {
                Debug.LogError(
                    "T0012: required Higgsfield MAP-A asset is missing at Resources/" +
                    MapResourcePath + ". The build/validation pipeline must materialize it.");
                throw new InvalidOperationException("Required Higgsfield MAP-A art is missing.");
            }

            GameObject map = new GameObject("Grass");
            map.transform.position = new Vector3(
                MapAData.Width * 0.5f,
                MapAData.Height * 0.5f,
                0f);

            SpriteRenderer mapRenderer = map.AddComponent<SpriteRenderer>();
            Sprite mapSprite = Sprite.Create(
                mapTexture,
                new Rect(0f, 0f, mapTexture.width, mapTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            mapSprite.name = "Higgsfield_MAP_A";
            mapRenderer.sprite = mapSprite;
            mapRenderer.sortingOrder = -100;
            mapRenderer.color = Color.white;

            Vector2 spriteSize = mapSprite.bounds.size;
            Vector3 exactScale = new Vector3(
                MapAData.Width / Mathf.Max(0.001f, spriteSize.x),
                MapAData.Height / Mathf.Max(0.001f, spriteSize.y),
                1f);
            map.transform.localScale = exactScale;

            // Full-bleed presentation without disturbing the exact foreground map/route alignment:
            // a slightly enlarged, darkened copy of the same approved Higgsfield map fills
            // aspect-ratio margins behind the authoritative artwork.
            GameObject bleed = new GameObject("HiggsfieldMapBleed");
            bleed.transform.position = map.transform.position + new Vector3(0f, 0f, 0.05f);
            bleed.transform.localScale = new Vector3(exactScale.x * 1.09f, exactScale.y * 1.09f, 1f);
            SpriteRenderer bleedRenderer = bleed.AddComponent<SpriteRenderer>();
            bleedRenderer.sprite = mapSprite;
            bleedRenderer.sortingOrder = -101;
            bleedRenderer.color = new Color(0.72f, 0.78f, 0.68f, 1f);

            // Keep exact route objects for editor/CI geometry validation and production
            // movement evidence, but they are deliberately invisible in the shipped map.
            CreateHiddenRoad("RoadShadow", 1.28f, -90);
            CreateHiddenRoad("Road", 1.03f, -89);
            CreateHiddenRoad("RoadDust", 0.76f, -88);

            SpawnPortal(MapAData.Waypoints[0]);
            HomeTree(MapAData.Waypoints[MapAData.Waypoints.Length - 1]);
        }

        private void BuildPlantingSpots()
        {
            for (int i = 0; i < MapAData.PlantingSpots.Length; i++)
            {
                PlantingSpotDefinition def = MapAData.PlantingSpots[i];

                // Soil is authored in MAP_A. A logical anchor retains the
                // production PlantingSystem distance-based touch hit testing.
                GameObject anchor = new GameObject("PlantingSpot_" + def.Id);
                anchor.transform.position = new Vector3(def.Position.x, def.Position.y, 0f);
                PlantingSpot spot = anchor.AddComponent<PlantingSpot>();
                spot.Initialize(def.Id, def.Epc);
                _spots.Add(spot);
            }
        }

        private static void CreateHiddenRoad(string name, float width, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = MapAData.Waypoints.Length;
            lr.useWorldSpace = true;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.numCornerVertices = 8;
            lr.numCapVertices = 8;

            Color invisible = new Color(1f, 1f, 1f, 0f);
            lr.material = RuntimeArt.SpriteMaterial(invisible);
            lr.startColor = invisible;
            lr.endColor = invisible;
            lr.sortingOrder = sortingOrder;

            for (int i = 0; i < MapAData.Waypoints.Length; i++)
            {
                Vector2 p = MapAData.Waypoints[i];
                lr.SetPosition(i, new Vector3(p.x, p.y, 0f));
            }
        }

        private static void SpawnPortal(Vector2 p)
        {
            FantasyVisualFactory.CreatePortal(p);
        }

        private static void HomeTree(Vector2 p)
        {
            FantasyVisualFactory.CreateHomeTree(p);
        }
    }
}
