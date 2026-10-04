using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Map7Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            CampaignMapDefinition definition = Map7SourceDefinition.Definition;
            Map56SourceScene.ConfigureCamera(definition);
            string source = Map7SourceDefinition.Resource;
            Map56SourceScene.RepeatGround("MAP7",source,new Rect(510f,880f,50f,50f),30f,17f);
            Vector2[][] routes = Map7SourceDefinition.InitialRoutes;
            AuthoredPathRenderer.Create("MAP7_RecoveredTempleStone",routes,source,
                new Rect(250f,1000f,70f,40f),new Vector2(2048f,1152f),1.05f,1.5f);
            // Source-only decorative symbols stay outside every combat lane and pad.
            PlaceRaw("MAP7_MoonWall",new Rect(710f,147f,104f,110f),new Vector2(28f,15f),new Vector2(1.8f,1.9f),-75);
            PlaceRaw("MAP7_SunWall",new Rect(1021f,345f,96f,86f),new Vector2(28f,2f),new Vector2(1.8f,1.6f),-75);
            List<PlantingSpot> spots = new List<PlantingSpot>();
            foreach (PlantingSpotDefinition data in definition.PlantingSpots)
            {
                // Keep bright source stone pixels: background-keying would erase the platform.
                SpriteRenderer platform = PlaceRaw("PlantingSpot_"+data.Id,new Rect(660f,540f,130f,50f),
                    data.Position,new Vector2(1.35f,0.65f),-80);
                PlantingSpot spot = platform.gameObject.AddComponent<PlantingSpot>();
                spot.Initialize(data.Id,data.Epc);
                spots.Add(spot);
            }
            Map56SourceScene.Endpoints(definition,routes);
            new GameObject("GameSession").AddComponent<GameSession>().Initialize(spots,definition);
        }

        private static SpriteRenderer PlaceRaw(string name,Rect topLeft,Vector2 position,Vector2 size,int order)
        {
            Texture2D texture = Resources.Load<Texture2D>(Map7SourceDefinition.Resource);
            if (texture == null || texture.width != 2048 || texture.height != 1152)
                throw new InvalidOperationException("Recovered MAP7 source must be the exact 2048x1152 PNG.");
            Sprite sprite = Sprite.Create(texture,new Rect(topLeft.x,1152f-topLeft.yMax,topLeft.width,topLeft.height),
                Vector2.one*0.5f,100f);
            sprite.name = name+"_RecoveredSourceCrop";
            GameObject go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
