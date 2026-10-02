using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class MapABootstrap : MonoBehaviour
    {
        private readonly List<PlantingSpot> _spots = new List<PlantingSpot>();

        private void Awake()
        {
            ConfigureMobile();
            BuildCamera();
            BuildMap();
            BuildPlantingSpots();

            GameObject sessionObject = new GameObject("GameSession");
            GameSession session = sessionObject.AddComponent<GameSession>();
            session.Initialize(_spots);
        }

        private static void ConfigureMobile()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        private static void BuildCamera()
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            Camera cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            go.AddComponent<MobileCameraFitter>();

            cam.orthographic = true;
            cam.backgroundColor = new Color(0.12f,0.22f,0.11f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private static void BuildMap()
        {
            RuntimeArt.SpriteObject("Grass",null,new Vector2(MapAData.Width*0.5f,MapAData.Height*0.5f),
                new Vector2(MapAData.Width,MapAData.Height),new Color(0.36f,0.66f,0.20f),-30,false);

            CreateRoad("RoadShadow",1.28f,new Color(0.28f,0.18f,0.07f),-20);
            CreateRoad("Road",1.03f,new Color(0.73f,0.52f,0.26f),-19);
            CreateRoad("RoadDust",0.76f,new Color(0.82f,0.63f,0.34f),-18);

            // fantasy scenery
            Pond(new Vector2(18.5f,10.5f),1.5f);
            RockCluster(new Vector2(2.2f,6.5f),1.0f);
            RockCluster(new Vector2(16f,6.2f),0.9f);
            Grove(new Vector2(22f,7f),1.1f);

            for (int i=0;i<24;i++)
            {
                float x = (i*3.7f)%23.2f + 0.4f;
                float y = (i*5.3f)%13.2f + 0.4f;
                Vector2 p = new Vector2(x,y);
                if (DistanceToPath(p) > 1.4f)
                    RuntimeArt.SpriteObject("WildFlower",null,p,new Vector2(0.12f,0.12f),
                        i%2==0 ? new Color(0.92f,0.58f,0.16f) : new Color(0.75f,0.32f,0.68f),-7);
            }

            SpawnPortal(MapAData.Waypoints[0]);
            HomeTree(MapAData.Waypoints[MapAData.Waypoints.Length-1]);
        }

        private void BuildPlantingSpots()
        {
            for (int i=0;i<MapAData.PlantingSpots.Length;i++)
            {
                PlantingSpotDefinition def = MapAData.PlantingSpots[i];
                GameObject soil = RuntimeArt.SpriteObject("PlantingSpot_"+def.Id,null,def.Position,
                    new Vector2(1.08f,0.74f),new Color(0.43f,0.28f,0.12f,0.92f),-3);

                GameObject ring = RuntimeArt.SpriteObject("Ring",soil.transform,Vector2.zero,
                    new Vector2(1.25f,0.90f),new Color(0.87f,0.74f,0.40f,0.58f),-4);
                ring.transform.localPosition = Vector3.zero;

                RuntimeArt.SpriteObject("Pebble1",soil.transform,new Vector2(-0.38f,0.12f),new Vector2(0.16f,0.12f),new Color(0.48f,0.42f,0.31f),-2);
                RuntimeArt.SpriteObject("Pebble2",soil.transform,new Vector2(0.34f,-0.10f),new Vector2(0.14f,0.10f),new Color(0.51f,0.45f,0.34f),-2);

                PlantingSpot spot = soil.AddComponent<PlantingSpot>();
                spot.Initialize(def.Id,def.Epc);
                _spots.Add(spot);
            }
        }

        private static void CreateRoad(string name, float width, Color color, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = MapAData.Waypoints.Length;
            lr.useWorldSpace = true;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.numCornerVertices = 14;
            lr.numCapVertices = 14;
            lr.material = RuntimeArt.SpriteMaterial(color);
            lr.startColor = color;
            lr.endColor = color;
            lr.sortingOrder = sortingOrder;

            for (int i=0;i<MapAData.Waypoints.Length;i++)
            {
                Vector2 p = MapAData.Waypoints[i];
                lr.SetPosition(i,new Vector3(p.x,p.y,0f));
            }
        }

        private static void Pond(Vector2 p, float r)
        {
            RuntimeArt.SpriteObject("PondRim",null,p,new Vector2(r*2.2f,r*1.75f),new Color(0.16f,0.38f,0.18f),-12);
            RuntimeArt.SpriteObject("Pond",null,p,new Vector2(r*1.9f,r*1.45f),new Color(0.18f,0.56f,0.72f),-11);
            RuntimeArt.SpriteObject("Lily",null,p+new Vector2(0.45f,0.20f),new Vector2(0.34f,0.24f),new Color(0.20f,0.56f,0.18f),-10);
            RuntimeArt.SpriteObject("Lily2",null,p+new Vector2(-0.55f,-0.26f),new Vector2(0.26f,0.19f),new Color(0.16f,0.48f,0.15f),-10);
        }

        private static void RockCluster(Vector2 p, float r)
        {
            RuntimeArt.SpriteObject("RockA",null,p,new Vector2(r*1.1f,r*0.75f),new Color(0.45f,0.43f,0.38f),-9);
            RuntimeArt.SpriteObject("RockB",null,p+new Vector2(0.45f*r,0.25f*r),new Vector2(r*0.65f,r*0.55f),new Color(0.55f,0.52f,0.46f),-8);
            RuntimeArt.SpriteObject("Moss",null,p+new Vector2(-0.15f*r,0.34f*r),new Vector2(r*0.50f,r*0.22f),new Color(0.25f,0.48f,0.16f),-7);
        }

        private static void Grove(Vector2 p, float r)
        {
            for (int i=0;i<4;i++)
            {
                float a = i*Mathf.PI*0.5f;
                Vector2 o = new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*0.42f;
                RuntimeArt.SpriteObject("TreeTrunk",null,p+o+new Vector2(0,-0.25f),new Vector2(0.22f,0.65f),new Color(0.37f,0.22f,0.09f),-10,false);
                RuntimeArt.SpriteObject("TreeCrown",null,p+o,new Vector2(0.78f,0.72f),new Color(0.10f,0.38f,0.11f),-9);
            }
        }

        private static void SpawnPortal(Vector2 p)
        {
            RuntimeArt.SpriteObject("PortalOuter",null,p,new Vector2(1.18f,1.18f),new Color(0.25f,0.10f,0.31f),2);
            RuntimeArt.SpriteObject("PortalInner",null,p,new Vector2(0.78f,0.78f),new Color(0.70f,0.26f,0.78f),3);
            RuntimeArt.SpriteObject("PortalCore",null,p,new Vector2(0.40f,0.40f),new Color(0.94f,0.66f,1f),4);
        }

        private static void HomeTree(Vector2 p)
        {
            RuntimeArt.SpriteObject("HomeTrunk",null,p+new Vector2(0,-0.18f),new Vector2(0.70f,1.20f),new Color(0.43f,0.25f,0.10f),2);
            RuntimeArt.SpriteObject("HomeCrown",null,p+new Vector2(0,0.55f),new Vector2(1.55f,1.10f),new Color(0.12f,0.42f,0.12f),3);
            RuntimeArt.SpriteObject("Door",null,p+new Vector2(0,-0.32f),new Vector2(0.30f,0.50f),new Color(0.12f,0.07f,0.03f),4);
        }

        private static float DistanceToPath(Vector2 point)
        {
            float best = float.PositiveInfinity;
            for (int i=0;i<MapAData.Waypoints.Length-1;i++)
                best = Mathf.Min(best,DistancePointSegment(point,MapAData.Waypoints[i],MapAData.Waypoints[i+1]));
            return best;
        }

        private static float DistancePointSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b-a;
            float denom = Vector2.Dot(ab,ab);
            if (denom <= Mathf.Epsilon) return Vector2.Distance(p,a);
            float t = Mathf.Clamp01(Vector2.Dot(p-a,ab)/denom);
            return Vector2.Distance(p,a+ab*t);
        }
    }
}
