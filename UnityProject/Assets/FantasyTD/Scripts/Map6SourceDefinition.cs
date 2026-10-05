using UnityEngine;

namespace FantasyTD.MapA
{
    public static class Map6SourceDefinition
    {
        public static CampaignMapDefinition Definition => new CampaignMapDefinition(
            "MAP6", "Las Bambusowy", "MAP6_BAMBOO_ANDROID", 30f, 16f,
            new[] { new Vector2(0f,13f), new Vector2(4f,13f), new Vector2(8f,12f), new Vector2(11f,10.5f), new Vector2(13f,10f), new Vector2(17f,10f), new Vector2(20f,9f), new Vector2(22f,8f), new Vector2(26f,8f), new Vector2(30f,8f) },
            new[]
            {
                new PlantingSpotDefinition("B1", new Vector2(2.3f,14.2f), 0f),
                new PlantingSpotDefinition("B2", new Vector2(5.8f,11.1f), 0f),
                new PlantingSpotDefinition("B3", new Vector2(9.8f,14f), 0f),
                new PlantingSpotDefinition("B4", new Vector2(2.5f,9.8f), 0f),
                new PlantingSpotDefinition("B5", new Vector2(6.3f,6.7f), 0f),
                new PlantingSpotDefinition("B6", new Vector2(10.1f,7.7f), 0f),
                new PlantingSpotDefinition("B7", new Vector2(2.4f,2.4f), 0f),
                new PlantingSpotDefinition("B8", new Vector2(6.6f,5.4f), 0f),
                new PlantingSpotDefinition("B9", new Vector2(11f,2.7f), 0f),
                new PlantingSpotDefinition("B10", new Vector2(15.4f,12.4f), 0f),
                new PlantingSpotDefinition("B11", new Vector2(17.2f,4.2f), 0f),
                new PlantingSpotDefinition("B12", new Vector2(21.1f,10.9f), 0f),
                new PlantingSpotDefinition("B13", new Vector2(25.5f,6.2f), 0f)
            },
            false, "Higgsfield/Maps/MAP6_TERRAIN", alternateRoutes: new[]
            {
                new[] { new Vector2(0f,13f), new Vector2(4f,13f), new Vector2(8f,12f), new Vector2(11f,10.5f), new Vector2(13f,10f), new Vector2(17f,10f), new Vector2(20f,9f), new Vector2(22f,8f), new Vector2(26f,8f), new Vector2(30f,8f) },
                new[] { new Vector2(0f,8f), new Vector2(4f,8f), new Vector2(8f,9f), new Vector2(11f,9.5f), new Vector2(13f,10f), new Vector2(17f,10f), new Vector2(20f,9f), new Vector2(22f,8f), new Vector2(26f,8f), new Vector2(30f,8f) },
                new[] { new Vector2(0f,3f), new Vector2(4f,3f), new Vector2(8f,4f), new Vector2(12f,5f), new Vector2(15f,6f), new Vector2(18f,6f), new Vector2(20f,7f), new Vector2(22f,8f), new Vector2(26f,8f), new Vector2(30f,8f) }
            });
    }
}
