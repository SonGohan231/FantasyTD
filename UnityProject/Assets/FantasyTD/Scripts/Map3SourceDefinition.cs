using UnityEngine;

namespace FantasyTD.MapA
{
    // Approved topology; visual/gameplay acceptance remains pending.
    public static class Map3SourceDefinition
    {
        public const string SceneName = "MAP_3_TWILIGHT";
        public const string TerrainResource = "Higgsfield/Maps/MAP3_TERRAIN";
        public const string GateResource = "Higgsfield/Maps/MAP3_GATE";
        public static Vector2[][] Routes => new[]
        {
            new[] { new Vector2(0f, 7f), new Vector2(1.5f, 7f), new Vector2(3f, 7f), new Vector2(6f, 10.5f), new Vector2(11f, 11.5f), new Vector2(15f, 9.5f), new Vector2(19f, 8.8f), new Vector2(22f, 7f), new Vector2(24f, 7f) },
            new[] { new Vector2(0f, 7f), new Vector2(1.5f, 7f), new Vector2(3f, 7f), new Vector2(6f, 3.5f), new Vector2(10f, 2.5f), new Vector2(14f, 4f), new Vector2(18f, 5.2f), new Vector2(22f, 7f), new Vector2(24f, 7f) }
        };
        public static CampaignMapDefinition Definition => new CampaignMapDefinition(
            "MAP3", "Zmierzchowy Gaj", SceneName, 24f, 14f, Routes[0],
            new[]
            {
                new PlantingSpotDefinition("T1", new Vector2(1.5f, 9.2f), 0f),
                new PlantingSpotDefinition("T2", new Vector2(2f, 4.8f), 0f),
                new PlantingSpotDefinition("T3", new Vector2(6.8f, 8.6f), 0f),
                new PlantingSpotDefinition("T4", new Vector2(10f, 9.4f), 0f),
                new PlantingSpotDefinition("T5", new Vector2(14f, 12.2f), 0f),
                new PlantingSpotDefinition("T6", new Vector2(18.2f, 10.6f), 0f),
                new PlantingSpotDefinition("T7", new Vector2(6.2f, 5.5f), 0f),
                new PlantingSpotDefinition("T8", new Vector2(9.5f, 4.8f), 0f),
                new PlantingSpotDefinition("T9", new Vector2(14.2f, 2.1f), 0f),
                new PlantingSpotDefinition("T10", new Vector2(18.2f, 3.4f), 0f),
                new PlantingSpotDefinition("T11", new Vector2(21f, 9.6f), 0f),
                new PlantingSpotDefinition("T12", new Vector2(22.3f, 4.9f), 0f)
            }, false, TerrainResource, alternateRoutes: Routes);
    }
}
