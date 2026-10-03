using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Map2Bootstrap : MonoBehaviour
    {
        private const string ResourceRoot = "Higgsfield/Maps/MAP2_";

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = true;
            CampaignMapDefinition definition = CampaignMapDefinition.Map2;

            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.backgroundColor = new Color(0.19f, 0.24f, 0.16f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.AddComponent<MobileCameraFitter>();

            BuildTerrain();
            BuildRoad();
            CreateEndpointLabel("WEJŚCIE", new Vector2(1.2f,11.6f));
            CreateEndpointLabel("BAZA", new Vector2(1.2f,1.65f));
            List<PlantingSpot> spots = BuildSpots(definition);
            GameSession session = new GameObject("GameSession").AddComponent<GameSession>();
            session.Initialize(spots, definition);
        }

        private static Sprite RequiredSprite(string suffix)
        {
            Rect pixels;
            switch (suffix)
            {
                case "ROAD_STRAIGHT": pixels = new Rect(40f,38f,475f,119f); break;
                case "ROAD_BEND": pixels = new Rect(190f,515f,300f,223f); break;
                case "BUILD_PATCH": pixels = new Rect(575f,25f,200f,140f); break;
                default: throw new ArgumentException("Unknown MAP2 authored crop: " + suffix);
            }
            Rect normalized = new Rect(pixels.x / 1344f, (768f - pixels.y - pixels.height) / 768f,
                pixels.width / 1344f, pixels.height / 768f);
            Sprite sprite = HiggsfieldSpriteFactory.SourceCropUntrimmed(ResourceRoot + "ROAD_KIT", normalized, "Approved_MAP2_" + suffix);
            if (sprite == null) throw new InvalidOperationException("Required approved MAP2 crop missing: " + suffix);
            return sprite;
        }

        private static void BuildTerrain()
        {
            Texture2D source = Resources.Load<Texture2D>(ResourceRoot + "TERRAIN");
            if (source == null) throw new InvalidOperationException("Required approved MAP2 terrain missing.");
            // Exclude the unfinished white area above source image row164.
            // This is an authored source crop, not a filled/generated background.
            float cropHeight = source.height * (604f / 768f);
            Sprite terrain = Sprite.Create(source, new Rect(0f, 0f, source.width, cropHeight), Vector2.one * 0.5f, 100f);
            Place("MAP2_ApprovedTerrain", terrain, new Vector2(12f,7f), new Vector2(24f,14f), -100);
        }

        private static void BuildRoad()
        {
            Sprite straight = RequiredSprite("ROAD_STRAIGHT");
            Sprite bend = RequiredSprite("ROAD_BEND");
            // Crop canvases remain exact source dimensions; endpoint math uses them.
            PlaceStraight("MAP2_UpperRoad", straight, new Vector2(0f,10.5f), new Vector2(17f,10.5f));
            SpriteRenderer upper = Place("MAP2_UpperBend", bend, new Vector2(18.35f,9.80625f), new Vector2(3.75f,2.7875f), -89);
            upper.flipY = true;
            PlaceStraight("MAP2_RightRoad", straight, new Vector2(19.4125f,9f), new Vector2(19.4125f,4.30625f));
            Place("MAP2_LowerBend", bend, new Vector2(18.35f,3.5f), new Vector2(3.75f,2.7875f), -89);
            PlaceStraight("MAP2_LowerRoad", straight, new Vector2(17f,2.80625f), new Vector2(0f,2.80625f));
        }

        private static void PlaceStraight(string name, Sprite sprite, Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            int count = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / 5.9375f));
            float segmentLength = delta.magnitude / count;
            for (int i = 0; i < count; i++)
            {
                Vector2 center = from + delta * ((i + 0.5f) / count);
                SpriteRenderer renderer = Place(i == 0 ? name : name + "_" + (i+1), sprite, center,
                    new Vector2(segmentLength + 0.12f, 1.4875f), -90);
                renderer.transform.localRotation = Quaternion.Euler(0f,0f,Mathf.Atan2(delta.y,delta.x) * Mathf.Rad2Deg);
            }
        }

        private static List<PlantingSpot> BuildSpots(CampaignMapDefinition definition)
        {
            Sprite patch = RequiredSprite("BUILD_PATCH");
            List<PlantingSpot> spots = new List<PlantingSpot>();
            foreach (PlantingSpotDefinition data in definition.PlantingSpots)
            {
                SpriteRenderer renderer = Place("PlantingSpot_" + data.Id, patch, data.Position, new Vector2(1.5f,1.05f), -80);
                PlantingSpot spot = renderer.gameObject.AddComponent<PlantingSpot>();
                spot.Initialize(data.Id, data.Epc);
                spots.Add(spot);
            }
            return spots;
        }

        private static void CreateEndpointLabel(string label, Vector2 position)
        {
            GameObject root = new GameObject("MAP2_" + label, typeof(RectTransform), typeof(Canvas));
            root.transform.position = position;
            root.transform.localScale = Vector3.one * 0.01f;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(190f,40f);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 80;
            Text text = RuntimeArt.CreateText(root.transform, "EndpointLabel", 24, TextAnchor.MiddleCenter, Color.white);
            text.text = label;
            text.fontStyle = FontStyle.Bold;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(2f,-2f);
        }

        private static SpriteRenderer Place(string name, Sprite sprite, Vector2 center, Vector2 size, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.position = new Vector3(center.x,center.y,0f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            Vector2 bounds = sprite.bounds.size;
            go.transform.localScale = new Vector3(size.x / bounds.x, size.y / bounds.y, 1f);
            return renderer;
        }
    }
}
