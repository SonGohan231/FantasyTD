using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Map6Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            CampaignMapDefinition definition=Map6SourceDefinition.Definition;
            Map56SourceScene.ConfigureCamera(definition);
            Map56SourceScene.RepeatGround("MAP6","Higgsfield/Maps/MAP6_TERRAIN",
                new Rect(455f,317f,106f,70f),definition.Width,definition.Height);
            Vector2[][] routes=Map56SourceScene.Routes(definition);
            AuthoredPathRenderer.Create("MAP6_AuthoredSand",routes,"Higgsfield/Maps/MAP1_ROAD_SOURCE",
                new Rect(990f,690f,160f,50f),new Vector2(2048f,1152f),1.05f,1.5f);
            Rect bamboo=new Rect(15f,10f,281f,261f);
            Map56SourceScene.Decoration("MAP6_BambooNW","Higgsfield/Maps/MAP6_PROPS",bamboo,new Vector2(0.85f,15.1f),new Vector2(1.5f,1.7f));
            Map56SourceScene.Decoration("MAP6_BambooNE","Higgsfield/Maps/MAP6_PROPS",bamboo,new Vector2(28.8f,15.1f),new Vector2(1.8f,1.7f));
            Map56SourceScene.Decoration("MAP6_BambooSW","Higgsfield/Maps/MAP6_PROPS",bamboo,new Vector2(0.8f,0.8f),new Vector2(1.5f,1.5f));
            Map56SourceScene.Decoration("MAP6_BambooSE","Higgsfield/Maps/MAP6_PROPS",bamboo,new Vector2(28.8f,0.8f),new Vector2(1.8f,1.5f));
            Map56SourceScene.Decoration("MAP6_PondNorth","Higgsfield/Maps/MAP6_PROPS",
                new Rect(25f,591f,339f,164f),new Vector2(23.8f,14.5f),new Vector2(3.7f,1.6f));
            Map56SourceScene.Decoration("MAP6_BambooSouth","Higgsfield/Maps/MAP6_PROPS",bamboo,new Vector2(15.5f,0.9f),new Vector2(2f,1.8f));
            List<PlantingSpot> spots=Map56SourceScene.BuildSpots(definition,"Higgsfield/Maps/MAP6_PROPS",
                new Rect(50f,278f,196f,157f),new Vector2(1.35f,1.0f));
            Map56SourceScene.Endpoints(definition,routes);
            new GameObject("GameSession").AddComponent<GameSession>().Initialize(spots,definition);
        }
    }
}
