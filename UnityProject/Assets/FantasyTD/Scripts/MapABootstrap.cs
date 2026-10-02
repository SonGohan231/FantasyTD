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
            cam.backgroundColor = new Color(0.065f, 0.12f, 0.065f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private static void BuildMap()
        {
            RuntimeArt.SpriteObject(
                "Grass", null,
                new Vector2(MapAData.Width * 0.5f, MapAData.Height * 0.5f),
                new Vector2(MapAData.Width, MapAData.Height),
                new Color(0.28f, 0.57f, 0.17f), -30, false);

            TerrainPatch(new Vector2(4.2f, 8.6f), new Vector2(6.4f, 4.8f), new Color(0.18f, 0.42f, 0.12f, 0.32f), -29);
            TerrainPatch(new Vector2(14.8f, 10.7f), new Vector2(8.2f, 4.2f), new Color(0.44f, 0.66f, 0.20f, 0.24f), -29);
            TerrainPatch(new Vector2(18.0f, 4.5f), new Vector2(8.6f, 5.0f), new Color(0.15f, 0.37f, 0.14f, 0.30f), -29);
            TerrainPatch(new Vector2(8.6f, 1.5f), new Vector2(7.4f, 2.9f), new Color(0.52f, 0.66f, 0.24f, 0.18f), -29);

            AddMapEdgeShade();

            // Keep exactly the three established route LineRenderers and their geometry.
            CreateRoad("RoadShadow", 1.28f, new Color(0.20f, 0.125f, 0.055f), -20);
            CreateRoad("Road", 1.03f, new Color(0.60f, 0.40f, 0.19f), -19);
            CreateRoad("RoadDust", 0.76f, new Color(0.80f, 0.60f, 0.31f), -18);
            AddRoadDetails();

            Pond(new Vector2(18.5f, 10.5f), 1.5f);
            RockCluster(new Vector2(2.2f, 6.5f), 1.0f);
            RockCluster(new Vector2(16f, 6.2f), 0.9f);
            Grove(new Vector2(22f, 7f), 1.1f);
            MushroomCluster(new Vector2(9.2f, 7.7f), 0.9f);
            MushroomCluster(new Vector2(20.4f, 12.0f), 0.7f);

            AddGroundDecor();

            SpawnPortal(MapAData.Waypoints[0]);
            HomeTree(MapAData.Waypoints[MapAData.Waypoints.Length - 1]);
        }

        private void BuildPlantingSpots()
        {
            for (int i = 0; i < MapAData.PlantingSpots.Length; i++)
            {
                PlantingSpotDefinition def = MapAData.PlantingSpots[i];

                RuntimeArt.ShadowObject(
                    "SoilShadow_" + def.Id, null,
                    def.Position + new Vector2(0f, -0.05f),
                    new Vector2(1.30f, 0.72f), -5, 0.34f);

                GameObject soil = RuntimeArt.SpriteObject(
                    "PlantingSpot_" + def.Id, null, def.Position,
                    new Vector2(1.08f, 0.74f),
                    new Color(0.36f, 0.22f, 0.09f, 0.96f), -3);

                GameObject ring = RuntimeArt.SpriteObject(
                    "Ring", soil.transform, Vector2.zero,
                    new Vector2(1.25f, 0.90f),
                    new Color(0.87f, 0.74f, 0.40f, 0.58f), -4, true, RuntimeArt.Ring);
                ring.transform.localPosition = Vector3.zero;

                RuntimeArt.SpriteObject(
                    "SoilLight", soil.transform, new Vector2(-0.10f, 0.09f),
                    new Vector2(0.72f, 0.34f),
                    new Color(0.70f, 0.48f, 0.22f, 0.20f), -2, true, RuntimeArt.SoftCircle);

                RuntimeArt.SpriteObject(
                    "Pebble1", soil.transform, new Vector2(-0.38f, 0.12f),
                    new Vector2(0.16f, 0.12f), new Color(0.54f, 0.47f, 0.34f), -1);
                RuntimeArt.SpriteObject(
                    "Pebble2", soil.transform, new Vector2(0.34f, -0.10f),
                    new Vector2(0.14f, 0.10f), new Color(0.46f, 0.40f, 0.29f), -1);
                RuntimeArt.SpriteObject(
                    "TinySprout", soil.transform, new Vector2(0.10f, 0.08f),
                    new Vector2(0.14f, 0.20f), new Color(0.35f, 0.66f, 0.18f, 0.72f), -1, true, RuntimeArt.Leaf, 18f);

                PlantingSpot spot = soil.AddComponent<PlantingSpot>();
                spot.Initialize(def.Id, def.Epc);
                _spots.Add(spot);
            }
        }

        private static void CreateRoad(string name, float width, Color color, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = MapAData.Waypoints.Length;
            lr.useWorldSpace = true;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.numCornerVertices = 14;
            lr.numCapVertices = 14;
            lr.material = RuntimeArt.SpriteMaterial(color);
            lr.startColor = color;
            lr.endColor = color;
            lr.sortingOrder = sortingOrder;

            for (int i = 0; i < MapAData.Waypoints.Length; i++)
            {
                Vector2 p = MapAData.Waypoints[i];
                lr.SetPosition(i, new Vector3(p.x, p.y, 0f));
            }
        }

        private static void AddRoadDetails()
        {
            for (int i = 0; i < MapAData.Waypoints.Length - 1; i++)
            {
                Vector2 a = MapAData.Waypoints[i];
                Vector2 b = MapAData.Waypoints[i + 1];
                Vector2 dir = (b - a).normalized;
                Vector2 normal = new Vector2(-dir.y, dir.x);

                for (int j = 1; j <= 2; j++)
                {
                    float t = j / 3f;
                    Vector2 center = Vector2.Lerp(a, b, t);
                    float side = ((i + j) % 2 == 0) ? 1f : -1f;
                    Vector2 p = center + normal * (0.25f * side);

                    RuntimeArt.SpriteObject(
                        "RoadStone", null, p,
                        new Vector2(0.18f + 0.03f * j, 0.10f + 0.02f * j),
                        new Color(0.39f, 0.28f, 0.17f, 0.42f), -17);
                }
            }
        }

        private static void AddGroundDecor()
        {
            for (int i = 0; i < 44; i++)
            {
                float x = (i * 4.37f) % 23.2f + 0.4f;
                float y = (i * 6.11f) % 13.2f + 0.4f;
                Vector2 p = new Vector2(x, y);
                if (DistanceToPath(p) <= 1.35f || NearPlantingSpot(p, 0.82f))
                    continue;

                if (i % 4 == 0)
                {
                    Flower(
                        p,
                        i % 8 == 0
                            ? new Color(1f, 0.71f, 0.17f)
                            : new Color(0.84f, 0.42f, 0.76f),
                        0.12f + (i % 3) * 0.018f);
                }
                else
                {
                    GrassTuft(
                        p,
                        0.24f + (i % 5) * 0.035f,
                        i % 2 == 0
                            ? new Color(0.13f, 0.42f, 0.10f, 0.86f)
                            : new Color(0.24f, 0.52f, 0.13f, 0.82f));
                }
            }
        }

        private static void TerrainPatch(Vector2 position, Vector2 size, Color color, int order)
        {
            RuntimeArt.SpriteObject(
                "TerrainPatch", null, position, size,
                color, order, true, RuntimeArt.SoftCircle);
        }

        private static void AddMapEdgeShade()
        {
            Color shade = new Color(0.025f, 0.065f, 0.025f, 0.22f);
            RuntimeArt.SpriteObject("EdgeShadeTop", null, new Vector2(12f, 13.72f),
                new Vector2(24f, 0.56f), shade, -27, false);
            RuntimeArt.SpriteObject("EdgeShadeBottom", null, new Vector2(12f, 0.28f),
                new Vector2(24f, 0.56f), shade, -27, false);
            RuntimeArt.SpriteObject("EdgeShadeLeft", null, new Vector2(0.24f, 7f),
                new Vector2(0.48f, 14f), shade, -27, false);
            RuntimeArt.SpriteObject("EdgeShadeRight", null, new Vector2(23.76f, 7f),
                new Vector2(0.48f, 14f), shade, -27, false);
        }

        private static void Pond(Vector2 p, float r)
        {
            RuntimeArt.ShadowObject(
                "PondShadow", null, p + new Vector2(0f, -0.10f),
                new Vector2(r * 2.55f, r * 1.95f), -13, 0.30f);
            RuntimeArt.SpriteObject(
                "PondRim", null, p,
                new Vector2(r * 2.28f, r * 1.82f),
                new Color(0.10f, 0.32f, 0.16f), -12);
            RuntimeArt.SpriteObject(
                "Pond", null, p,
                new Vector2(r * 1.98f, r * 1.50f),
                new Color(0.13f, 0.47f, 0.67f), -11);
            RuntimeArt.SpriteObject(
                "PondGlow", null, p + new Vector2(-0.30f, 0.22f),
                new Vector2(r * 1.16f, r * 0.55f),
                new Color(0.63f, 0.94f, 1f, 0.30f), -10, true, RuntimeArt.SoftCircle);

            RuntimeArt.SpriteObject(
                "Lily", null, p + new Vector2(0.45f, 0.20f),
                new Vector2(0.34f, 0.24f),
                new Color(0.20f, 0.56f, 0.18f), -9, true, RuntimeArt.Leaf, 72f);
            RuntimeArt.SpriteObject(
                "Lily2", null, p + new Vector2(-0.55f, -0.26f),
                new Vector2(0.28f, 0.20f),
                new Color(0.16f, 0.48f, 0.15f), -9, true, RuntimeArt.Leaf, -46f);

            for (int i = 0; i < 4; i++)
            {
                float dx = -1.05f + i * 0.20f;
                RuntimeArt.SpriteObject(
                    "Reed", null, p + new Vector2(dx, -0.12f + i * 0.05f),
                    new Vector2(0.08f, 0.54f),
                    new Color(0.20f, 0.43f, 0.12f), -8, true, RuntimeArt.Leaf, -7f + i * 5f);
            }
        }

        private static void RockCluster(Vector2 p, float r)
        {
            RuntimeArt.ShadowObject(
                "RockShadow", null, p + new Vector2(0f, -0.18f),
                new Vector2(r * 1.65f, r * 0.62f), -10, 0.28f);
            RuntimeArt.SpriteObject(
                "RockA", null, p,
                new Vector2(r * 1.12f, r * 0.78f),
                new Color(0.40f, 0.40f, 0.36f), -9);
            RuntimeArt.SpriteObject(
                "RockAHighlight", null, p + new Vector2(-0.17f * r, 0.18f * r),
                new Vector2(r * 0.55f, r * 0.26f),
                new Color(0.72f, 0.70f, 0.61f, 0.40f), -8, true, RuntimeArt.SoftCircle);
            RuntimeArt.SpriteObject(
                "RockB", null, p + new Vector2(0.45f * r, 0.25f * r),
                new Vector2(r * 0.66f, r * 0.56f),
                new Color(0.53f, 0.50f, 0.44f), -8);
            RuntimeArt.SpriteObject(
                "Moss", null, p + new Vector2(-0.15f * r, 0.34f * r),
                new Vector2(r * 0.52f, r * 0.24f),
                new Color(0.25f, 0.48f, 0.16f), -7, true, RuntimeArt.Leaf, 72f);
        }

        private static void Grove(Vector2 p, float r)
        {
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f;
                Vector2 o = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * 0.42f;
                RuntimeArt.ShadowObject(
                    "TreeShadow", null, p + o + new Vector2(0f, -0.36f),
                    new Vector2(0.80f, 0.30f), -11, 0.26f);
                RuntimeArt.SpriteObject(
                    "TreeTrunk", null, p + o + new Vector2(0f, -0.25f),
                    new Vector2(0.22f, 0.65f),
                    new Color(0.35f, 0.20f, 0.075f), -10, false);
                RuntimeArt.SpriteObject(
                    "TreeCrownBack", null, p + o,
                    new Vector2(0.86f, 0.80f),
                    new Color(0.055f, 0.26f, 0.08f), -9);
                RuntimeArt.SpriteObject(
                    "TreeCrown", null, p + o + new Vector2(-0.06f, 0.07f),
                    new Vector2(0.72f, 0.66f),
                    new Color(0.10f + i * 0.018f, 0.38f + i * 0.018f, 0.11f), -8);
                RuntimeArt.SpriteObject(
                    "TreeLight", null, p + o + new Vector2(-0.17f, 0.19f),
                    new Vector2(0.28f, 0.18f),
                    new Color(0.58f, 0.80f, 0.28f, 0.30f), -7, true, RuntimeArt.SoftCircle);
            }
        }

        private static void MushroomCluster(Vector2 p, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                float dx = (i - 1) * 0.24f * scale;
                float h = (0.22f + i * 0.05f) * scale;
                RuntimeArt.SpriteObject(
                    "MushroomStem", null, p + new Vector2(dx, -0.08f),
                    new Vector2(0.08f * scale, h),
                    new Color(0.82f, 0.72f, 0.52f), -7, false);
                RuntimeArt.SpriteObject(
                    "MushroomCap", null, p + new Vector2(dx, 0.07f + i * 0.02f),
                    new Vector2(0.25f * scale, 0.16f * scale),
                    i == 1
                        ? new Color(0.64f, 0.22f, 0.38f)
                        : new Color(0.80f, 0.40f, 0.18f), -6);
            }
        }

        private static void GrassTuft(Vector2 p, float scale, Color color)
        {
            RuntimeArt.SpriteObject(
                "GrassBladeL", null, p + new Vector2(-scale * 0.10f, 0f),
                new Vector2(scale * 0.24f, scale),
                color, -6, true, RuntimeArt.Leaf, -18f);
            RuntimeArt.SpriteObject(
                "GrassBladeM", null, p + new Vector2(0f, scale * 0.02f),
                new Vector2(scale * 0.22f, scale * 1.12f),
                color, -6, true, RuntimeArt.Leaf, 0f);
            RuntimeArt.SpriteObject(
                "GrassBladeR", null, p + new Vector2(scale * 0.10f, 0f),
                new Vector2(scale * 0.24f, scale),
                color, -6, true, RuntimeArt.Leaf, 18f);
        }

        private static void Flower(Vector2 p, Color color, float scale)
        {
            RuntimeArt.SpriteObject(
                "FlowerStem", null, p + new Vector2(0f, -scale * 0.65f),
                new Vector2(scale * 0.22f, scale * 1.25f),
                new Color(0.13f, 0.40f, 0.09f), -6, false);
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f * Mathf.Deg2Rad;
                RuntimeArt.SpriteObject(
                    "WildFlower", null,
                    p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * scale * 0.65f,
                    new Vector2(scale, scale * 1.30f),
                    color, -5, true, RuntimeArt.Petal, i * 72f - 90f);
            }
            RuntimeArt.SpriteObject(
                "FlowerCore", null, p,
                new Vector2(scale * 0.82f, scale * 0.82f),
                new Color(0.95f, 0.73f, 0.12f), -4);
        }

        private static void SpawnPortal(Vector2 p)
        {
            RuntimeArt.SpriteObject(
                "PortalAura", null, p,
                new Vector2(1.75f, 1.75f),
                new Color(0.61f, 0.18f, 0.78f, 0.26f), 1, true, RuntimeArt.SoftCircle);
            RuntimeArt.SpriteObject(
                "PortalOuter", null, p,
                new Vector2(1.30f, 1.30f),
                new Color(0.21f, 0.075f, 0.26f), 2);
            RuntimeArt.SpriteObject(
                "PortalRing", null, p,
                new Vector2(1.10f, 1.10f),
                new Color(0.87f, 0.38f, 0.98f, 0.88f), 3, true, RuntimeArt.Ring);
            RuntimeArt.SpriteObject(
                "PortalInner", null, p,
                new Vector2(0.80f, 0.80f),
                new Color(0.49f, 0.14f, 0.64f), 3);
            RuntimeArt.SpriteObject(
                "PortalCore", null, p,
                new Vector2(0.42f, 0.42f),
                new Color(0.96f, 0.72f, 1f), 4, true, RuntimeArt.SoftCircle);

            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f;
                float r = a * Mathf.Deg2Rad;
                Vector2 q = p + new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * 0.77f;
                RuntimeArt.SpriteObject(
                    "PortalThorn", null, q,
                    new Vector2(0.18f, 0.46f),
                    new Color(0.31f, 0.16f, 0.07f), 5, true, RuntimeArt.Thorn, a - 90f);
            }
        }

        private static void HomeTree(Vector2 p)
        {
            RuntimeArt.ShadowObject(
                "HomeShadow", null, p + new Vector2(0f, -0.48f),
                new Vector2(1.92f, 0.62f), 1, 0.38f);

            RuntimeArt.SpriteObject(
                "HomeRootL", null, p + new Vector2(-0.46f, -0.46f),
                new Vector2(0.86f, 0.20f),
                new Color(0.29f, 0.16f, 0.06f), 2, true, RuntimeArt.Leaf, -74f);
            RuntimeArt.SpriteObject(
                "HomeRootR", null, p + new Vector2(0.46f, -0.46f),
                new Vector2(0.86f, 0.20f),
                new Color(0.29f, 0.16f, 0.06f), 2, true, RuntimeArt.Leaf, 74f);

            RuntimeArt.SpriteObject(
                "HomeTrunkOutline", null, p + new Vector2(0f, -0.10f),
                new Vector2(0.86f, 1.42f),
                new Color(0.09f, 0.065f, 0.035f), 2);
            RuntimeArt.SpriteObject(
                "HomeTrunk", null, p + new Vector2(0f, -0.08f),
                new Vector2(0.72f, 1.30f),
                new Color(0.45f, 0.27f, 0.10f), 3);

            RuntimeArt.SpriteObject(
                "HomeCrownBack", null, p + new Vector2(0f, 0.64f),
                new Vector2(1.80f, 1.28f),
                new Color(0.055f, 0.27f, 0.075f), 3);
            RuntimeArt.SpriteObject(
                "HomeCrownL", null, p + new Vector2(-0.46f, 0.63f),
                new Vector2(1.00f, 0.88f),
                new Color(0.10f, 0.39f, 0.10f), 4);
            RuntimeArt.SpriteObject(
                "HomeCrownR", null, p + new Vector2(0.46f, 0.67f),
                new Vector2(1.00f, 0.90f),
                new Color(0.14f, 0.45f, 0.11f), 4);
            RuntimeArt.SpriteObject(
                "HomeCrownTop", null, p + new Vector2(0f, 0.93f),
                new Vector2(1.05f, 0.78f),
                new Color(0.17f, 0.51f, 0.12f), 5);
            RuntimeArt.SpriteObject(
                "HomeGlow", null, p + new Vector2(0f, 0.45f),
                new Vector2(1.20f, 1.20f),
                new Color(0.64f, 0.90f, 0.30f, 0.14f), 5, true, RuntimeArt.SoftCircle);

            RuntimeArt.SpriteObject(
                "Door", null, p + new Vector2(0f, -0.34f),
                new Vector2(0.31f, 0.54f),
                new Color(0.10f, 0.055f, 0.025f), 6);
            RuntimeArt.SpriteObject(
                "DoorGlow", null, p + new Vector2(0f, -0.28f),
                new Vector2(0.18f, 0.31f),
                new Color(1f, 0.70f, 0.20f, 0.36f), 7, true, RuntimeArt.SoftCircle);
        }

        private static bool NearPlantingSpot(Vector2 point, float radius)
        {
            float r2 = radius * radius;
            for (int i = 0; i < MapAData.PlantingSpots.Length; i++)
            {
                if ((MapAData.PlantingSpots[i].Position - point).sqrMagnitude <= r2)
                    return true;
            }
            return false;
        }

        private static float DistanceToPath(Vector2 point)
        {
            float best = float.PositiveInfinity;
            for (int i = 0; i < MapAData.Waypoints.Length - 1; i++)
                best = Mathf.Min(best, DistancePointSegment(point, MapAData.Waypoints[i], MapAData.Waypoints[i + 1]));
            return best;
        }

        private static float DistancePointSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float denom = Vector2.Dot(ab, ab);
            if (denom <= Mathf.Epsilon) return Vector2.Distance(p, a);
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / denom);
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
