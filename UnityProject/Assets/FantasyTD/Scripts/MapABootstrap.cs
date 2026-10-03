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
            cam.backgroundColor = new Color(0.035f, 0.055f, 0.025f);
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
            map.transform.localScale = new Vector3(
                MapAData.Width / Mathf.Max(0.001f, spriteSize.x),
                MapAData.Height / Mathf.Max(0.001f, spriteSize.y),
                1f);

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

                GameObject soil = RuntimeArt.SpriteObject(
                    "PlantingSpot_" + def.Id,
                    null,
                    def.Position,
                    new Vector2(1.15f, 0.88f),
                    new Color(1f, 1f, 1f, 0f),
                    5);

                GameObject ring = RuntimeArt.SpriteObject(
                    "Ring",
                    soil.transform,
                    Vector2.zero,
                    new Vector2(1.28f, 0.98f),
                    new Color(1f, 0.86f, 0.34f, 0.26f),
                    6,
                    true,
                    RuntimeArt.Ring);
                ring.transform.localPosition = Vector3.zero;

                PlantingSpot spot = soil.AddComponent<PlantingSpot>();
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
            RuntimeArt.SpriteObject(
                "PortalAura",
                null,
                p,
                new Vector2(1.38f, 1.38f),
                new Color(0.64f, 0.22f, 0.86f, 0.18f),
                8,
                true,
                RuntimeArt.SoftCircle);

            RuntimeArt.SpriteObject(
                "PortalOuter",
                null,
                p,
                new Vector2(0.82f, 0.82f),
                new Color(0.46f, 0.13f, 0.62f, 0.92f),
                9,
                true,
                RuntimeArt.Ring);

            RuntimeArt.SpriteObject(
                "PortalCore",
                null,
                p,
                new Vector2(0.42f, 0.42f),
                new Color(0.93f, 0.65f, 1f, 0.82f),
                10,
                true,
                RuntimeArt.SoftCircle);
        }

        private static void HomeTree(Vector2 p)
        {
            // The validator treats HomeTrunk.position + (0,0.18) as the route anchor.
            Vector2 trunkPosition = p + new Vector2(0f, -0.18f);

            RuntimeArt.ShadowObject(
                "HomeShadow",
                null,
                p + new Vector2(0f, -0.40f),
                new Vector2(1.34f, 0.46f),
                7,
                0.28f);

            RuntimeArt.SpriteObject(
                "HomeTrunk",
                null,
                trunkPosition,
                new Vector2(0.50f, 0.92f),
                new Color(0.37f, 0.21f, 0.075f),
                9);

            RuntimeArt.SpriteObject(
                "HomeCrown",
                null,
                p + new Vector2(0f, 0.38f),
                new Vector2(1.12f, 0.88f),
                new Color(0.12f, 0.43f, 0.10f),
                10);

            RuntimeArt.SpriteObject(
                "HomeGlow",
                null,
                p + new Vector2(0f, 0.30f),
                new Vector2(1.35f, 1.35f),
                new Color(0.58f, 0.88f, 0.24f, 0.13f),
                11,
                true,
                RuntimeArt.SoftCircle);
        }
    }
}
