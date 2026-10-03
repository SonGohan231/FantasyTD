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

        // Functional spots are kept close enough to the visible road for the
        // validated L1 ranges. Their subtle rings are the only generated map overlay.
        public static readonly PlantingSpotDefinition[] PlantingSpots =
        {
            new PlantingSpotDefinition("A1",  new Vector2(5.60f,12.86f), 4.88f),
            new PlantingSpotDefinition("A2",  new Vector2(3.47f,9.52f), 5.30f),
            new PlantingSpotDefinition("A3",  new Vector2(7.44f,9.45f), 6.25f),
            new PlantingSpotDefinition("A4",  new Vector2(6.39f,5.78f), 4.83f),
            new PlantingSpotDefinition("A5",  new Vector2(9.75f,7.94f), 6.03f),
            new PlantingSpotDefinition("A6",  new Vector2(10.98f,4.22f), 5.37f),
            new PlantingSpotDefinition("A7",  new Vector2(13.59f,7.21f), 5.37f),
            new PlantingSpotDefinition("A8",  new Vector2(14.89f,3.46f), 5.53f),
            new PlantingSpotDefinition("A9",  new Vector2(17.83f,6.26f), 5.14f),
            new PlantingSpotDefinition("A10", new Vector2(18.52f,2.36f), 5.95f),
            new PlantingSpotDefinition("A11", new Vector2(21.77f,4.29f), 5.08f),
            new PlantingSpotDefinition("A12", new Vector2(21.18f,0.80f), 5.57f)
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
