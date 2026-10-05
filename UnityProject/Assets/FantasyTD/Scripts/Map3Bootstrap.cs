using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Map3Bootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = true;
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.10f, 0.06f, 0.16f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<MobileCameraFitter>();

            Texture2D terrain = RequiredTexture(Map3SourceDefinition.TerrainResource);
            Sprite plate = Sprite.Create(terrain, new Rect(0f, 0f, terrain.width, terrain.height), Vector2.one * 0.5f, 100f);
            Place("MAP3_ApprovedTerrain", plate, new Vector2(12f, 7f), new Vector2(24f, 14f), -100);
            AuthoredPathRenderer.Create("MAP3", Map3SourceDefinition.Routes, "Higgsfield/Maps/MAP1_ROAD_SOURCE",
                new Rect(990f, 690f, 160f, 50f), new Vector2(2048f, 1152f));
            List<PlantingSpot> spots = BuildSpots();
            Sprite gate = HiggsfieldSpriteFactory.SourceCropUntrimmed(Map3SourceDefinition.GateResource,
                new Rect(370f / 1344f, 58f / 768f, 630f / 1344f, 650f / 768f), "MAP3_AuthoredMoonGate");
            if (gate == null) throw new InvalidOperationException("Required MAP3 moon gate crop missing.");
            SpriteRenderer entry = Place("MAP3_EntryGate", gate, new Vector2(0.8f, 7.6f), new Vector2(1.7f, 2.3f), 9);
            entry.flipX = true;
            Place("MAP3_BaseGate", gate, new Vector2(23.2f, 7.6f), new Vector2(1.7f, 2.3f), 9);
            GameSession session = new GameObject("GameSession").AddComponent<GameSession>();
            session.Initialize(spots, Map3SourceDefinition.Definition);
        }

        private static Texture2D RequiredTexture(string resource)
        {
            Texture2D texture = Resources.Load<Texture2D>(resource);
            if (texture == null) throw new InvalidOperationException("Required approved map texture missing: " + resource);
            return texture;
        }

        private static List<PlantingSpot> BuildSpots()
        {
            Sprite patch = HiggsfieldSpriteFactory.SourceCropUntrimmed(Map3SourceDefinition.TerrainResource,
                new Rect(231f / 1344f, (768f - 528f) / 768f, 203f / 1344f, 57f / 768f),
                "MAP3_AuthoredBuildDepression");
            if (patch == null) throw new InvalidOperationException("Required MAP3 planting depression missing.");
            List<PlantingSpot> spots = new List<PlantingSpot>();
            foreach (PlantingSpotDefinition definition in Map3SourceDefinition.Definition.PlantingSpots)
            {
                Place("MAP3_AuthoredPatch_" + definition.Id, patch, definition.Position, new Vector2(1.55f, 0.7f), -80);
                GameObject anchor = new GameObject("PlantingSpot_" + definition.Id);
                anchor.transform.position = definition.Position;
                PlantingSpot spot = anchor.AddComponent<PlantingSpot>();
                spot.Initialize(definition.Id, definition.Epc);
                spots.Add(spot);
            }
            return spots;
        }

        private static SpriteRenderer Place(string name, Sprite sprite, Vector2 position, Vector2 size, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.position = position;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            go.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);
            return renderer;
        }
    }
}
