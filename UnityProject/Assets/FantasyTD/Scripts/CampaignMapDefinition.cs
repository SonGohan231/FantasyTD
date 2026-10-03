using System;
using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class CampaignMapDefinition
    {
        private readonly Vector2[] _waypoints;
        private readonly PlantingSpotDefinition[] _spots;
        private readonly Vector2[][] _spawnRoutes;
        public string Id { get; }
        public string DisplayName { get; }
        public string SceneName { get; }
        public string BackgroundResource { get; }
        public float Width { get; }
        public float Height { get; }
        public bool IsValidatedPlayable { get; }
        public Vector2[] Waypoints => (Vector2[])_waypoints.Clone();
        public PlantingSpotDefinition[] PlantingSpots => (PlantingSpotDefinition[])_spots.Clone();

        public CampaignMapDefinition(string id, string displayName, string sceneName, float width, float height,
            Vector2[] waypoints, PlantingSpotDefinition[] spots, bool isValidatedPlayable, string backgroundResource = null,
            Vector2[][] alternateRoutes = null)
        {
            if (string.IsNullOrEmpty(id) || waypoints == null || waypoints.Length < 2 || spots == null)
                throw new ArgumentException("A campaign map needs an ID, route and planting data.");
            Id = id;
            DisplayName = displayName;
            SceneName = sceneName;
            BackgroundResource = backgroundResource;
            Width = width;
            Height = height;
            IsValidatedPlayable = isValidatedPlayable;
            _waypoints = (Vector2[])waypoints.Clone();
            _spots = (PlantingSpotDefinition[])spots.Clone();
            if (alternateRoutes != null && alternateRoutes.Length > 0)
            {
                _spawnRoutes = new Vector2[alternateRoutes.Length][];
                for (int i = 0; i < alternateRoutes.Length; i++)
                    _spawnRoutes[i] = GameSession.SnapshotRoute(alternateRoutes[i]);
            }
            else _spawnRoutes = new[] { GameSession.SnapshotRoute(waypoints) };
        }

        public Vector2[] GetSpawnRoute(int ordinal)
        {
            if (ordinal < 0) throw new ArgumentOutOfRangeException(nameof(ordinal));
            return (Vector2[])_spawnRoutes[ordinal % _spawnRoutes.Length].Clone();
        }

        public static CampaignMapDefinition Map1 => new CampaignMapDefinition("MAP1", "Zielony Trakt",
            FantasyTD.Product.ProductFlowScenes.MapASceneName, MapAData.Width, MapAData.Height,
            MapAData.Waypoints, MapAData.PlantingSpots, true, "Higgsfield/MAP_A");

        // Source-authored asphalt: straight crop plus mirrored/original L bends.
        // Every curved waypoint follows a measured centerline in artifact 11227420838.
        public static CampaignMapDefinition Map2 => new CampaignMapDefinition("MAP2", "Odzyskane Ruiny",
            FantasyTD.Product.ProductFlowScenes.Map2SceneName, 24f, 14f,
            new[]
            {
                new Vector2(0f,10.5f), new Vector2(17f,10.5f),
                new Vector2(17.85f,10.4625f), new Vector2(18.6f,10.3f),
                new Vector2(19.1375f,9.7625f), new Vector2(19.4125f,9f),
                new Vector2(19.4125f,4.30625f), new Vector2(19.1375f,3.54375f),
                new Vector2(18.6f,3.00625f), new Vector2(17.85f,2.84375f),
                new Vector2(17f,2.80625f), new Vector2(0f,2.80625f)
            },
            new[]
            {
                Spot("B1",3f,12.15f), Spot("B2",7f,12.15f), Spot("B3",11f,12.15f), Spot("B4",15f,12.15f),
                Spot("B5",4f,8.8f), Spot("B6",8f,8.8f), Spot("B7",12f,8.8f), Spot("B8",16f,8.8f),
                Spot("B9",17.8f,6.1f), Spot("B10",15f,4.5f), Spot("B11",10f,4.5f), Spot("B12",5f,4.5f)
            }, false, "Higgsfield/Maps/MAP2_TERRAIN");

        private static PlantingSpotDefinition Spot(string id, float x, float y) =>
            new PlantingSpotDefinition(id, new Vector2(x, y), 0f);
    }
}
