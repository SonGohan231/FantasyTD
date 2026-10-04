using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Map8Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            CampaignMapDefinition definition = Map8SourceDefinition.Definition;
            Map56SourceScene.ConfigureCamera(definition);
            string source = Map8SourceDefinition.Resource;
            // The original single-road concept is never rendered as a battlefield.
            Map56SourceScene.RepeatGround("MAP8", source, new Rect(1590f,730f,50f,50f),32f,18f);
            Vector2[][] routes = Map8SourceDefinition.InitialRoutes;
            AuthoredPathRenderer.Create("MAP8_RecoveredStone",routes,source,
                new Rect(720f,481f,50f,22f),new Vector2(2048f,1152f),1.05f,1.5f);
            Map56SourceScene.Decoration("MAP8_CrystalSouth",source,new Rect(1135f,933f,251f,214f),
                new Vector2(16.6f,1.5f),new Vector2(2.6f,2.2f));
            Map56SourceScene.Decoration("MAP8_HoardNorth",source,new Rect(1320f,335f,188f,153f),
                new Vector2(26.8f,16f),new Vector2(2.5f,1.9f));
            Map56SourceScene.Decoration("MAP8_DragonSanctum",source,new Rect(1240f,81f,256f,273f),
                new Vector2(15f,16.3f),new Vector2(3f,2.8f));
            List<PlantingSpot> spots = Map56SourceScene.BuildSpots(definition,source,
                new Rect(1110f,453f,146f,112f),new Vector2(1.3f,1f));
            Map56SourceScene.Endpoints(definition,routes);
            GameSession session = new GameObject("GameSession").AddComponent<GameSession>();
            session.Initialize(spots,definition);
            new GameObject("MAP8_TreasurySecret").AddComponent<Map8TreasurySecret>().Initialize(session);
        }
    }
}
