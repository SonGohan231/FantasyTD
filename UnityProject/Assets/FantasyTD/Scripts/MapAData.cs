using System;
using UnityEngine;

namespace FantasyTD.MapA
{
    public static class MapAData
    {
        public const float Width = 24f;
        public const float Height = 14f;
        public const float RoadWidth = 1f;

        // T0012: gameplay geometry aligned to the visible road in the bundled
        // Higgsfield MAP-A artwork. These points remain invisible in production.
        public static readonly Vector2[] Waypoints =
        {
            new Vector2(3.05f,14.00f),
            new Vector2(3.81f,12.66f),
            new Vector2(4.51f,11.20f),
            new Vector2(5.33f,9.62f),
            new Vector2(6.39f,7.92f),
            new Vector2(7.97f,6.71f),
            new Vector2(9.96f,6.10f),
            new Vector2(12.30f,5.74f),
            new Vector2(14.65f,5.25f),
            new Vector2(16.99f,4.76f),
            new Vector2(19.22f,3.91f),
            new Vector2(21.09f,2.70f),
            new Vector2(22.62f,1.24f),
            new Vector2(24.00f,0.15f)
        };

        // T0012: centres of the twelve authored soil patches in artifact
        // 11219593382 (2048 x 1152). Pixel origin is top-left; the background
        // stretches to 24 x 14 RU, so x = px / 2048 * 24 and y = 14 - py / 1152 * 14.
        // The art itself provides the visible planting surface; no overlay rings.
        // Epc is retained legacy metadata, not a recomputed coverage guarantee.
        public static readonly PlantingSpotDefinition[] PlantingSpots =
        {
            new PlantingSpotDefinition("A1", new Vector2(2.637f, 9.807f), 4.88f), // pixel (225, 345)
            new PlantingSpotDefinition("A2", new Vector2(8.203f, 12.785f), 5.30f), // pixel (700, 100)
            new PlantingSpotDefinition("A3", new Vector2(11.602f, 10.755f), 6.25f), // pixel (990, 267)
            new PlantingSpotDefinition("A4", new Vector2(2.473f, 5.092f), 4.83f), // pixel (211, 733)
            new PlantingSpotDefinition("A5", new Vector2(5.801f, 4.703f), 6.03f), // pixel (495, 765)
            new PlantingSpotDefinition("A6", new Vector2(15.000f, 9.953f), 5.37f), // pixel (1280, 333)
            new PlantingSpotDefinition("A7", new Vector2(17.906f, 7.875f), 5.37f), // pixel (1528, 504)
            new PlantingSpotDefinition("A8", new Vector2(8.133f, 1.179f), 5.53f), // pixel (694, 1055)
            new PlantingSpotDefinition("A9", new Vector2(14.355f, 2.333f), 5.14f), // pixel (1225, 960)
            new PlantingSpotDefinition("A10", new Vector2(22.910f, 9.017f), 5.95f), // pixel (1955, 410)
            new PlantingSpotDefinition("A11", new Vector2(21.023f, 6.101f), 5.08f), // pixel (1794, 650)
            new PlantingSpotDefinition("A12", new Vector2(23.355f, 4.399f), 5.57f), // pixel (1993, 790)
        };

        public static readonly ObstacleDefinition[] Obstacles =
        {
            new ObstacleDefinition("ArtOnly", new Vector2(12f,7f), 0f)
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
