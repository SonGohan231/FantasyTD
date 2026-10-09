using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FantasyTD.MapA
{
    /// <summary>
    /// Real Unity Tilemaps for MAP-A. All layers are generated from the same
    /// authoritative geometry used by EnemyUnit and the mobile planting UI.
    /// No editor-only dependencies; works in the Android player.
    /// </summary>
    public static class MapATilemapLayers
    {
        public const string RootName = "MAP-A World Layers";
        public const int WidthCells = 24;
        public const int HeightCells = 14;
        private const float EnemyClearance = 0.35f;

        public static void Build()
        {
            GameObject root = new GameObject(RootName);
            Grid grid = root.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap grass = MakeLayer(grid.transform, "Ground_Tilemap", -29, true);
            Tilemap borders = MakeLayer(grid.transform, "PathEdge_Tilemap", -22, true);
            Tilemap accents = MakeLayer(grid.transform, "Foliage_Tilemap", -16, true);
            Tilemap solid = MakeLayer(grid.transform, "SolidObstacles_Tilemap", -15, false);

            Tile[] meadow =
            {
                MakeTile("MeadowDark", new Color(0.30f, 0.56f, 0.21f)),
                MakeTile("MeadowMid", new Color(0.35f, 0.62f, 0.24f)),
                MakeTile("MeadowLight", new Color(0.38f, 0.64f, 0.27f)),
                MakeTile("MeadowWarm", new Color(0.39f, 0.61f, 0.22f))
            };
            Tile roadEdge = MakeTile("WornPathShoulder", new Color(0.49f, 0.45f, 0.26f));
            Tile foliage = MakeTile("LeafCarpet", new Color(0.23f, 0.47f, 0.19f, 0.5f));
            Tile blocker = MakeTile("CollisionOnly", Color.clear);
            blocker.colliderType = Tile.ColliderType.Grid;

            for (int y = 0; y < HeightCells; y++)
            for (int x = 0; x < WidthCells; x++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector2 center = new Vector2(x + 0.5f, y + 0.5f);
                int noise = (x * 92821 ^ y * 68917 ^ x * y * 73) & 0x7fffffff;
                grass.SetTile(cell, meadow[noise % meadow.Length]);

                float pathDistance = DistanceToRoute(center);
                if (pathDistance < 0.96f && pathDistance > 0.47f)
                    borders.SetTile(cell, roadEdge);
                else if (pathDistance > 1.65f && noise % 11 == 0)
                    accents.SetTile(cell, foliage);

                for (int i = 0; i < MapAData.Obstacles.Length; i++)
                {
                    ObstacleDefinition obstacle = MapAData.Obstacles[i];
                    float solidRadius = Mathf.Max(0.45f, obstacle.Radius * 0.58f);
                    if (Vector2.Distance(center, obstacle.Center) <= solidRadius &&
                        pathDistance > EnemyClearance + 0.65f)
                    {
                        solid.SetTile(cell, blocker);
                        break;
                    }
                }
            }

            solid.gameObject.AddComponent<TilemapCollider2D>();
        }

        public static bool ValidateNavigation(out string failure)
        {
            if (MapAData.Waypoints.Length < 2)
            {
                failure = "Enemy route requires at least two waypoints.";
                return false;
            }
            for (int i = 0; i < MapAData.Waypoints.Length - 1; i++)
            {
                Vector2 a = MapAData.Waypoints[i];
                Vector2 b = MapAData.Waypoints[i + 1];
                if (!Inside(a) || !Inside(b) || (b - a).sqrMagnitude < 0.0001f)
                {
                    failure = "Waypoint segment invalid at index " + i;
                    return false;
                }
                for (int j = 0; j < MapAData.Obstacles.Length; j++)
                {
                    ObstacleDefinition o = MapAData.Obstacles[j];
                    if (PointSegmentDistance(o.Center, a, b) <= o.Radius * 0.58f + EnemyClearance)
                    {
                        failure = "Obstacle " + o.Id + " blocks enemy segment " + i;
                        return false;
                    }
                }
            }
            for (int i = 0; i < MapAData.PlantingSpots.Length; i++)
            {
                Vector2 p = MapAData.PlantingSpots[i].Position;
                if (!Inside(p) || DistanceToRoute(p) < 0.40f)
                {
                    failure = "Build position " + MapAData.PlantingSpots[i].Id + " is unsafe.";
                    return false;
                }
                for (int j = 0; j < MapAData.Obstacles.Length; j++)
                    if (Vector2.Distance(p, MapAData.Obstacles[j].Center) <
                        MapAData.Obstacles[j].Radius * 0.58f + 0.35f)
                    {
                        failure = "Build position " + MapAData.PlantingSpots[i].Id + " overlaps " +
                                  MapAData.Obstacles[j].Id;
                        return false;
                    }
            }
            failure = string.Empty;
            return true;
        }

        private static bool Inside(Vector2 p)
        {
            return p.x >= 0f && p.y >= 0f && p.x <= MapAData.Width && p.y <= MapAData.Height;
        }

        private static float DistanceToRoute(Vector2 p)
        {
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < MapAData.Waypoints.Length - 1; i++)
                nearest = Mathf.Min(nearest, PointSegmentDistance(p,
                    MapAData.Waypoints[i], MapAData.Waypoints[i + 1]));
            return nearest;
        }

        private static float PointSegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) /
                                    Mathf.Max(Vector2.Dot(ab, ab), 0.0001f));
            return Vector2.Distance(p, a + ab * t);
        }

        private static Tile MakeTile(string name, Color tint)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = name;
            tile.sprite = RuntimeArt.White;
            tile.color = tint;
            tile.colliderType = Tile.ColliderType.None;
            tile.flags = TileFlags.LockColor;
            return tile;
        }

        private static Tilemap MakeLayer(Transform parent, string name, int sortingOrder, bool visible)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Tilemap map = go.AddComponent<Tilemap>();
            TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = visible;
            return map;
        }
    }
}
