using UnityEngine;

namespace FantasyTD.MapA
{
    // Candidate composition from existing rejected-layout source; requires fresh Unity visual QA.
    public static class Map8SourceDefinition
    {
        public const string Resource = "Higgsfield/Maps/MAP8_RECOVERED_CONCEPT";
        public static Vector2[][] InitialRoutes => new[]
        {
                new[] { new Vector2(0f, 15f), new Vector2(5f, 15f), new Vector2(9f, 14f), new Vector2(13f, 13f), new Vector2(17f, 13f), new Vector2(21f, 12f), new Vector2(24f, 10f), new Vector2(26f, 9f), new Vector2(29f, 9f), new Vector2(32f, 9f) },
                new[] { new Vector2(0f, 11f), new Vector2(5f, 11f), new Vector2(9f, 11f), new Vector2(13f, 12f), new Vector2(17f, 13f), new Vector2(21f, 12f), new Vector2(24f, 10f), new Vector2(26f, 9f), new Vector2(29f, 9f), new Vector2(32f, 9f) },
                new[] { new Vector2(0f, 7f), new Vector2(5f, 7f), new Vector2(9f, 7f), new Vector2(13f, 6f), new Vector2(17f, 6f), new Vector2(21f, 7f), new Vector2(24f, 8f), new Vector2(26f, 9f), new Vector2(29f, 9f), new Vector2(32f, 9f) },
                new[] { new Vector2(0f, 3f), new Vector2(5f, 3f), new Vector2(9f, 4f), new Vector2(13f, 5f), new Vector2(17f, 6f), new Vector2(21f, 7f), new Vector2(24f, 8f), new Vector2(26f, 9f), new Vector2(29f, 9f), new Vector2(32f, 9f) }
        };
        public static CampaignMapDefinition Definition => new CampaignMapDefinition(
            "MAP8", "Lochy Smoka", "MAP8_DRAGON_DUNGEONS_ANDROID", 32f, 18f,
            InitialRoutes[0], new[]
            {
                new PlantingSpotDefinition("D1", new Vector2(2.5f, 16.8f), 0f),
                new PlantingSpotDefinition("D2", new Vector2(5.8f, 13.3f), 0f),
                new PlantingSpotDefinition("D3", new Vector2(3f, 9.3f), 0f),
                new PlantingSpotDefinition("D4", new Vector2(8f, 12.2f), 0f),
                new PlantingSpotDefinition("D5", new Vector2(3f, 5.2f), 0f),
                new PlantingSpotDefinition("D6", new Vector2(8f, 5.8f), 0f),
                new PlantingSpotDefinition("D7", new Vector2(2.5f, 1.3f), 0f),
                new PlantingSpotDefinition("D8", new Vector2(11.5f, 15.5f), 0f),
                new PlantingSpotDefinition("D9", new Vector2(12.3f, 2.8f), 0f),
                new PlantingSpotDefinition("D10", new Vector2(17.5f, 9.7f), 0f),
                new PlantingSpotDefinition("D11", new Vector2(21.5f, 14.5f), 0f),
                new PlantingSpotDefinition("D12", new Vector2(21.5f, 4f), 0f),
                new PlantingSpotDefinition("D13", new Vector2(27.3f, 6.6f), 0f)
            }, false, Resource, alternateRoutes: InitialRoutes);
    }
}
