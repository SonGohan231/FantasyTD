using UnityEngine;

namespace FantasyTD.MapA
{
    // Approved topology; visual/gameplay acceptance remains pending.
    public static class Map4SourceDefinition
    {
        public const string SceneName = "MAP_4_ABYSSAL";
        public const string TerrainResource = "Higgsfield/Maps/MAP4_TERRAIN";
        public const string PropsResource = "Higgsfield/Maps/MAP4_PROPS";
        public static Vector2[][] Routes => new[]
        {
            new[] { new Vector2(0f, 7f), new Vector2(2.5f, 7f), new Vector2(5.5f, 10.2f), new Vector2(9f, 11f), new Vector2(11.2f, 8.2f), new Vector2(12f, 7f), new Vector2(14f, 7f), new Vector2(17f, 9.6f), new Vector2(20f, 10f), new Vector2(22f, 7f), new Vector2(24f, 7f) },
            new[] { new Vector2(0f, 7f), new Vector2(2.5f, 7f), new Vector2(5.5f, 10.2f), new Vector2(9f, 11f), new Vector2(11.2f, 8.2f), new Vector2(12f, 7f), new Vector2(14f, 7f), new Vector2(17f, 4.4f), new Vector2(20f, 4f), new Vector2(22f, 7f), new Vector2(24f, 7f) },
            new[] { new Vector2(0f, 7f), new Vector2(2.5f, 7f), new Vector2(5.5f, 3.8f), new Vector2(9f, 3f), new Vector2(11.2f, 5.8f), new Vector2(12f, 7f), new Vector2(14f, 7f), new Vector2(17f, 9.6f), new Vector2(20f, 10f), new Vector2(22f, 7f), new Vector2(24f, 7f) },
            new[] { new Vector2(0f, 7f), new Vector2(2.5f, 7f), new Vector2(5.5f, 3.8f), new Vector2(9f, 3f), new Vector2(11.2f, 5.8f), new Vector2(12f, 7f), new Vector2(14f, 7f), new Vector2(17f, 4.4f), new Vector2(20f, 4f), new Vector2(22f, 7f), new Vector2(24f, 7f) }
        };
        public static CampaignMapDefinition Definition => new CampaignMapDefinition(
            "MAP4", "Otchłań Pereł", SceneName, 24f, 14f, Routes[0],
            new[]
            {
                new PlantingSpotDefinition("A1", new Vector2(1.3f, 9.1f), 0f),
                new PlantingSpotDefinition("A2", new Vector2(1.6f, 4.9f), 0f),
                new PlantingSpotDefinition("A3", new Vector2(5.2f, 8.1f), 0f),
                new PlantingSpotDefinition("A4", new Vector2(8.1f, 9f), 0f),
                new PlantingSpotDefinition("A5", new Vector2(10.2f, 12.2f), 0f),
                new PlantingSpotDefinition("A6", new Vector2(5.2f, 5.9f), 0f),
                new PlantingSpotDefinition("A7", new Vector2(8.1f, 5f), 0f),
                new PlantingSpotDefinition("A8", new Vector2(10.2f, 1.8f), 0f),
                new PlantingSpotDefinition("A9", new Vector2(13f, 9.7f), 0f),
                new PlantingSpotDefinition("A10", new Vector2(17.4f, 11.6f), 0f),
                new PlantingSpotDefinition("A11", new Vector2(20.2f, 8.3f), 0f),
                new PlantingSpotDefinition("A12", new Vector2(17.4f, 2.4f), 0f),
                new PlantingSpotDefinition("A13", new Vector2(20.2f, 5.7f), 0f)
            }, false, TerrainResource, alternateRoutes: Routes);
    }
}
