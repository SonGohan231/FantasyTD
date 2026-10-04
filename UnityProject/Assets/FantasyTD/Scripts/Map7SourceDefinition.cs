using UnityEngine;

namespace FantasyTD.MapA
{
    // Source is an old rejected full-layout concept. This separate composition is a QA candidate.
    public static class Map7SourceDefinition
    {
        public const string Resource = "Higgsfield/Maps/MAP7_RECOVERED_CONCEPT";
        // Three primary lanes plus the two canonical links open in state A.
        public static Vector2[][] InitialRoutes => new[]
        {
            new[] { new Vector2(0f, 8.5f), new Vector2(5f, 8.5f), new Vector2(7f, 13.5f), new Vector2(12f, 14f), new Vector2(17f, 13f), new Vector2(22f, 11f), new Vector2(25f, 8.5f), new Vector2(30f, 8.5f) },
            new[] { new Vector2(0f, 8.5f), new Vector2(5f, 8.5f), new Vector2(9f, 8.5f), new Vector2(14f, 8.5f), new Vector2(19f, 8.5f), new Vector2(25f, 8.5f), new Vector2(30f, 8.5f) },
            new[] { new Vector2(0f, 8.5f), new Vector2(5f, 8.5f), new Vector2(7f, 3.5f), new Vector2(12f, 3f), new Vector2(17f, 4f), new Vector2(22f, 6f), new Vector2(25f, 8.5f), new Vector2(30f, 8.5f) },
            new[] { new Vector2(0f, 8.5f), new Vector2(5f, 8.5f), new Vector2(7f, 13.5f), new Vector2(12f, 14f), new Vector2(14f, 8.5f), new Vector2(19f, 8.5f), new Vector2(25f, 8.5f), new Vector2(30f, 8.5f) },
            new[] { new Vector2(0f, 8.5f), new Vector2(5f, 8.5f), new Vector2(7f, 13.5f), new Vector2(12f, 14f), new Vector2(17f, 13f), new Vector2(19f, 8.5f), new Vector2(25f, 8.5f), new Vector2(30f, 8.5f) }
        };
        public static CampaignMapDefinition Definition => new CampaignMapDefinition(
            "MAP7", "Świątynia Słońca i Księżyca", "MAP7_TEMPLE_ANDROID", 30f, 17f,
            InitialRoutes[0], new[]
            {
                new PlantingSpotDefinition("T1", new Vector2(2.8f, 11.2f), 0f),
                new PlantingSpotDefinition("T2", new Vector2(3f, 5.8f), 0f),
                new PlantingSpotDefinition("T3", new Vector2(8.8f, 15.5f), 0f),
                new PlantingSpotDefinition("T4", new Vector2(14.8f, 15.2f), 0f),
                new PlantingSpotDefinition("T5", new Vector2(9.5f, 10.2f), 0f),
                new PlantingSpotDefinition("T6", new Vector2(16f, 10.3f), 0f),
                new PlantingSpotDefinition("T7", new Vector2(8.8f, 1.5f), 0f),
                new PlantingSpotDefinition("T8", new Vector2(15.2f, 1.8f), 0f),
                new PlantingSpotDefinition("T9", new Vector2(20.5f, 13.3f), 0f),
                new PlantingSpotDefinition("T10", new Vector2(20.5f, 4f), 0f),
                new PlantingSpotDefinition("T11", new Vector2(24.5f, 11.2f), 0f),
                new PlantingSpotDefinition("T12", new Vector2(25f, 5.8f), 0f)
            }, false, Resource, alternateRoutes: InitialRoutes);
    }
}
