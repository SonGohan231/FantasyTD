using System;
using UnityEngine;

namespace FantasyTD.MapA
{
    public static class MapAData
    {
        public const float Width = 24f;
        public const float Height = 14f;
        public const float RoadWidth = 1f;

        public static readonly Vector2[] Waypoints =
        {
            new Vector2(0f, 11f),
            new Vector2(5f, 11f),
            new Vector2(10f, 11f),
            new Vector2(13f, 9f),
            new Vector2(13f, 6f),
            new Vector2(10f, 4f),
            new Vector2(5f, 4f),
            new Vector2(5f, 2f),
            new Vector2(10f, 2f),
            new Vector2(16f, 2f),
            new Vector2(22f, 2f),
            new Vector2(24f, 2f)
        };

        public static readonly PlantingSpotDefinition[] PlantingSpots =
        {
            new PlantingSpotDefinition("A1", new Vector2(4f, 10f), 6.08f),
            new PlantingSpotDefinition("A2", new Vector2(11f, 9f), 7.60f),
            new PlantingSpotDefinition("A3", new Vector2(12f, 7f), 8.19f),
            new PlantingSpotDefinition("A4", new Vector2(9f, 5.5f), 6.47f),
            new PlantingSpotDefinition("A5", new Vector2(7f, 3f), 12.08f),
            new PlantingSpotDefinition("A6", new Vector2(12.5f, 3.5f), 10.58f),
            new PlantingSpotDefinition("A7", new Vector2(18f, 3.5f), 5.65f),
            new PlantingSpotDefinition("A8", new Vector2(21f, 4f), 5.00f),
            new PlantingSpotDefinition("A9", new Vector2(2.5f, 12.5f), 5.20f),
            new PlantingSpotDefinition("A10", new Vector2(15f, 4.4f), 6.40f),
            new PlantingSpotDefinition("A11", new Vector2(2.8f, 8.5f), 5.10f),
            new PlantingSpotDefinition("A12", new Vector2(22.2f, 5.1f), 4.80f)
        };

        public static readonly ObstacleDefinition[] Obstacles =
        {
            new ObstacleDefinition("Pond", new Vector2(18.5f, 10.5f), 1.5f),
            new ObstacleDefinition("RocksA", new Vector2(2.2f, 6.5f), 1.0f),
            new ObstacleDefinition("RocksB", new Vector2(16f, 6.2f), 0.9f),
            new ObstacleDefinition("Grove", new Vector2(22f, 7f), 1.1f)
        };

        public static float TotalPathLength
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < Waypoints.Length - 1; i++)
                    total += Vector2.Distance(Waypoints[i], Waypoints[i + 1]);
                return total;
            }
        }
    }

    [Serializable]
    public struct PlantingSpotDefinition
    {
        public string Id;
        public Vector2 Position;
        public float Epc;

        public PlantingSpotDefinition(string id, Vector2 position, float epc)
        {
            Id = id;
            Position = position;
            Epc = epc;
        }
    }

    [Serializable]
    public struct ObstacleDefinition
    {
        public string Id;
        public Vector2 Center;
        public float Radius;

        public ObstacleDefinition(string id, Vector2 center, float radius)
        {
            Id = id;
            Center = center;
            Radius = radius;
        }
    }
}
