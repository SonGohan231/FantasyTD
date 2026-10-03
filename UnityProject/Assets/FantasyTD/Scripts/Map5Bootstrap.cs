using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Map5Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            CampaignMapDefinition definition = Map5SourceDefinition.Definition;
            Map56SourceScene.ConfigureCamera(definition);
            Map56SourceScene.RepeatGround("MAP5", "Higgsfield/Maps/MAP5_TERRAIN",
                new Rect(551f,217f,88f,91f), definition.Width, definition.Height);
            Vector2[][] routes = Map56SourceScene.Routes(definition);
            AuthoredPathRenderer.Create("MAP5_AuthoredRoad", routes,
                "Higgsfield/Maps/MAP2_ROAD_KIT", new Rect(70f,66f,400f,61f), new Vector2(1344f,768f), 1.05f, 4f);
            Map56SourceScene.Decoration("MAP5_CityNorth", "Higgsfield/Maps/MAP5_PROPS",
                new Rect(1080f,18f,244f,277f), new Vector2(2.1f,14.55f), new Vector2(2.8f,2.7f));
            Map56SourceScene.Decoration("MAP5_MushroomsNorth", "Higgsfield/Maps/MAP5_PROPS",
                new Rect(0f,0f,343f,344f), new Vector2(25.5f,14.8f), new Vector2(2.6f,2.4f));
            Map56SourceScene.Decoration("MAP5_FungalSlabSouth", "Higgsfield/Maps/MAP5_PROPS",
                new Rect(1071f,493f,257f,252f), new Vector2(3f,1.25f), new Vector2(2.4f,2f));
            Map56SourceScene.Decoration("MAP5_LampSouth", "Higgsfield/Maps/MAP5_PROPS",
                new Rect(946f,20f,132f,266f), new Vector2(25.5f,1.55f), new Vector2(1f,2.3f));
            List<PlantingSpot> spots = Map56SourceScene.BuildSpots(definition,
                "Higgsfield/Maps/MAP5_PROPS", new Rect(754f,297f,206f,184f), new Vector2(1.6f,1.18f));
            Map56SourceScene.Endpoints(definition, routes);
            GameSession session=new GameObject("GameSession").AddComponent<GameSession>();
            session.Initialize(spots,definition);
            new GameObject("MAP5_SecretController").AddComponent<Map5SecretController>().Initialize(session);
        }
    }

    internal static class Map56SourceScene
    {
        internal static void ConfigureCamera(CampaignMapDefinition definition)
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = true;
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.backgroundColor = new Color(0.06f,0.10f,0.06f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            go.AddComponent<AudioListener>();
            go.AddComponent<MobileCameraFitter>().Configure(definition.Width,definition.Height);
        }

        internal static Vector2[][] Routes(CampaignMapDefinition definition) => new[]
        {
            definition.GetSpawnRoute(0), definition.GetSpawnRoute(1), definition.GetSpawnRoute(2)
        };

        internal static void RepeatGround(string name, string resource, Rect cropTopLeft, float width, float height)
        {
            Texture2D texture = RequiredTexture(resource);
            Sprite source = Sprite.Create(texture,
                new Rect(cropTopLeft.x, texture.height-cropTopLeft.y-cropTopLeft.height,cropTopLeft.width,cropTopLeft.height),
                Vector2.one*0.5f,100f);
            source.name = name + "_OriginalGroundCrop";
            int columns = Mathf.CeilToInt(width/3f), rows = Mathf.CeilToInt(height/3f);
            float tileWidth = width/columns, tileHeight = height/rows;
            for (int y=0;y<rows;y++)
            for (int x=0;x<columns;x++)
                Place(name+"_Ground_"+x+"_"+y,source,
                    new Vector2((x+0.5f)*tileWidth,(y+0.5f)*tileHeight),new Vector2(tileWidth+0.004f,tileHeight+0.004f),-100);
        }

        internal static void Decoration(string name, string resource, Rect crop, Vector2 position, Vector2 size)
        {
            Place(name, IsolatedCrop(resource,crop,name+"_Source"),position,size,-75);
        }

        internal static List<PlantingSpot> BuildSpots(CampaignMapDefinition definition, string resource, Rect crop, Vector2 size)
        {
            Sprite sprite = IsolatedCrop(resource,crop,definition.Id+"_AuthoredBuildPad");
            List<PlantingSpot> result = new List<PlantingSpot>();
            foreach (PlantingSpotDefinition data in definition.PlantingSpots)
            {
                SpriteRenderer renderer=Place("PlantingSpot_"+data.Id,sprite,data.Position,size,-80);
                PlantingSpot spot=renderer.gameObject.AddComponent<PlantingSpot>();
                spot.Initialize(data.Id,data.Epc);
                result.Add(spot);
            }
            return result;
        }

        internal static void Endpoints(CampaignMapDefinition definition, Vector2[][] routes)
        {
            List<Vector2> starts=new List<Vector2>();
            foreach (Vector2[] route in routes)
            {
                Vector2 start=route[0];
                if (starts.Contains(start)) continue;
                starts.Add(start);
                Place(definition.Id+"_Portal_"+starts.Count,HiggsfieldSpriteFactory.Portal,
                    start+new Vector2(0.55f,-0.50f),new Vector2(1.05f,1.55f),18);
            }
            Vector2 goal=routes[0][routes[0].Length-1];
            Place(definition.Id+"_HomeTree",HiggsfieldSpriteFactory.HomeTree,
                goal+new Vector2(-0.90f,-0.45f),new Vector2(1.5f,1.8f),20);
        }

        private static Sprite IsolatedCrop(string resource, Rect crop, string name)
        {
            Texture2D texture=RequiredTexture(resource);
            Sprite sprite=HiggsfieldSpriteFactory.SourceCropUntrimmed(resource,
                new Rect(crop.x/texture.width,(texture.height-crop.y-crop.height)/texture.height,
                    crop.width/texture.width,crop.height/texture.height),name);
            if (sprite==null) throw new InvalidOperationException("Required approved source crop missing: "+resource+" "+name);
            return sprite;
        }

        private static Texture2D RequiredTexture(string resource)
        {
            Texture2D texture=Resources.Load<Texture2D>(resource);
            if (texture==null) throw new InvalidOperationException("Required approved map art missing: "+resource);
            return texture;
        }

        private static SpriteRenderer Place(string name, Sprite sprite, Vector2 position, Vector2 size, int order)
        {
            if (sprite==null) throw new InvalidOperationException("Required approved sprite missing: "+name);
            GameObject go=new GameObject(name);
            go.transform.position=new Vector3(position.x,position.y,0f);
            SpriteRenderer renderer=go.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite; renderer.sortingOrder=order;
            Vector2 bounds=sprite.bounds.size;
            go.transform.localScale=new Vector3(size.x/bounds.x,size.y/bounds.y,1f);
            return renderer;
        }
    }
}
