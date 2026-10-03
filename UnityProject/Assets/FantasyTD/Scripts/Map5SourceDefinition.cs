using UnityEngine;

namespace FantasyTD.MapA
{
    public static class Map5SourceDefinition
    {
        public static CampaignMapDefinition Definition => new CampaignMapDefinition(
            "MAP5", "Grzybowe Miasto", "MAP5_FUNGAL_ANDROID", 28f, 16f,
            new[] { new Vector2(0f,8f), new Vector2(3f,8f), new Vector2(5f,8f), new Vector2(7f,12f), new Vector2(11f,13f), new Vector2(15f,12f), new Vector2(18f,10f), new Vector2(22f,9f), new Vector2(25f,9f), new Vector2(28f,8f) },
            new[]
            {
                new PlantingSpotDefinition("F1", new Vector2(2.4f,10.7f), 0f),
                new PlantingSpotDefinition("F2", new Vector2(3.1f,5.8f), 0f),
                new PlantingSpotDefinition("F3", new Vector2(8.6f,14.1f), 0f),
                new PlantingSpotDefinition("F4", new Vector2(14f,13.8f), 0f),
                new PlantingSpotDefinition("F5", new Vector2(9.9f,9.7f), 0f),
                new PlantingSpotDefinition("F6", new Vector2(16.5f,9.7f), 0f),
                new PlantingSpotDefinition("F7", new Vector2(8.5f,1.9f), 0f),
                new PlantingSpotDefinition("F8", new Vector2(14.6f,2.2f), 0f),
                new PlantingSpotDefinition("F9", new Vector2(20f,11.6f), 0f),
                new PlantingSpotDefinition("F10", new Vector2(20.2f,6.6f), 0f),
                new PlantingSpotDefinition("F11", new Vector2(24.2f,11.5f), 0f),
                new PlantingSpotDefinition("F12", new Vector2(25.2f,6.2f), 0f)
            },
            false, "Higgsfield/Maps/MAP5_TERRAIN", alternateRoutes: new[]
            {
                new[] { new Vector2(0f,8f), new Vector2(3f,8f), new Vector2(5f,8f), new Vector2(7f,12f), new Vector2(11f,13f), new Vector2(15f,12f), new Vector2(18f,10f), new Vector2(22f,9f), new Vector2(25f,9f), new Vector2(28f,8f) },
                new[] { new Vector2(0f,8f), new Vector2(3f,8f), new Vector2(5f,8f), new Vector2(9f,8f), new Vector2(13f,8f), new Vector2(17f,8f), new Vector2(22f,9f), new Vector2(25f,9f), new Vector2(28f,8f) },
                new[] { new Vector2(0f,8f), new Vector2(3f,8f), new Vector2(5f,8f), new Vector2(7f,4f), new Vector2(11f,3f), new Vector2(15f,4f), new Vector2(18f,6f), new Vector2(22f,9f), new Vector2(25f,9f), new Vector2(28f,8f) }
            });
    }
}
