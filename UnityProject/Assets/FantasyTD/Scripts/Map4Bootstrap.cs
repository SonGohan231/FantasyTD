using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    [DefaultExecutionOrder(-1000)]
    public sealed class Map4Bootstrap : MonoBehaviour
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
            camera.backgroundColor = new Color(0.08f, 0.16f, 0.22f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<MobileCameraFitter>();
            Texture2D terrain = Resources.Load<Texture2D>(Map4SourceDefinition.TerrainResource);
            if (terrain == null || terrain.width != 1344 || terrain.height != 768)
                throw new InvalidOperationException("Required exact 1344x768 approved MAP4 terrain missing.");

            // Authored road-free seabed pixels tiled at small scale. The full plate's
            // incompatible diagonal road is excluded; no replacement pixels are generated.
            Sprite field = Sprite.Create(terrain, new Rect(330f, 205f, 86f, 44f), Vector2.one * 0.5f, 100f);
            for (int y = 0; y < 14; y++)
            for (int x = 0; x < 16; x++)
            {
                SpriteRenderer tile = Place("MAP4_AuthoredSeabedTile_" + x + "_" + y, field,
                    new Vector2((x + 0.5f) * 1.5f, y + 0.5f), new Vector2(1.5f, 1f), -100);
                tile.flipX = (x & 1) != 0;
                tile.flipY = (y & 1) != 0;
            }
            AuthoredPathRenderer.Create("MAP4", Map4SourceDefinition.Routes, Map4SourceDefinition.TerrainResource,
                new Rect(600f, 337f, 110f, 42f), new Vector2(1344f, 768f));
            BuildDressing();
            List<PlantingSpot> spots = BuildSpots();
            Sprite gateway = Crop(new Rect(195f, 5f, 230f, 170f), "MAP4_ApprovedGateway");
            Place("MAP4_EntryGateway", gateway, new Vector2(0.85f, 7.55f), new Vector2(1.8f, 1.5f), 9);
            Place("MAP4_BaseGateway", gateway, new Vector2(23.15f, 7.55f), new Vector2(1.8f, 1.5f), 9);
            GameSession session = new GameObject("GameSession").AddComponent<GameSession>();
            session.Initialize(spots, Map4SourceDefinition.Definition);
            BuildShrine();
            Sprite bubble = Crop(new Rect(265f, 371f, 80f, 78f), "MAP4_AuthoredHotBubble");
            new GameObject("MAP4_HydrothermalBubbleEvent").AddComponent<Map4BubbleHazard>().Initialize(session, bubble);
        }

        private static Sprite RequiredState(string resource, string name)
        {
            Sprite sprite = HiggsfieldSpriteFactory.SourceCropUntrimmed(resource, new Rect(0f, 0f, 1f, 1f), name);
            if (sprite == null) throw new InvalidOperationException("Required authored MAP4 state missing: " + resource);
            return sprite;
        }

        private static void BuildShrine()
        {
            Sprite closed = RequiredState("Higgsfield/Maps/MAP4_SHRINE_CLOSED", "MAP4_PearlShrineClosed");
            Sprite opened = RequiredState("Higgsfield/Maps/MAP4_SHRINE_OPEN_FINAL", "MAP4_PearlShrineOpenFinal");
            Vector2 size = new Vector2(2.2f, 1.75f);
            SpriteRenderer renderer = Place("MAP4_AncientPearlShrine", closed, new Vector2(13f, 2.1f), size, 10);
            renderer.gameObject.AddComponent<Map4ShrineSecret>().Initialize(renderer, opened, size);
        }

        private static Sprite Crop(Rect pixels, string name)
        {
            Rect normalized = new Rect(pixels.x / 1344f, (768f - pixels.yMax) / 768f,
                pixels.width / 1344f, pixels.height / 768f);
            Sprite sprite = HiggsfieldSpriteFactory.SourceCropUntrimmed(Map4SourceDefinition.PropsResource, normalized, name);
            if (sprite == null) throw new InvalidOperationException("Required authored MAP4 crop missing: " + name);
            return sprite;
        }

        private static void BuildDressing()
        {
            Sprite seaweed = Crop(new Rect(470f, 5f, 140f, 175f), "MAP4_AuthoredSeaweed");
            Sprite vent = Crop(new Rect(422f, 230f, 245f, 90f), "MAP4_AuthoredVent");
            Sprite activeVent = Crop(new Rect(675f, 162f, 242f, 158f), "MAP4_AuthoredActiveVent");
            Sprite pearlPile = Crop(new Rect(200f, 485f, 212f, 140f), "MAP4_AuthoredPearls");
            Sprite fish = Crop(new Rect(700f, 650f, 165f, 95f), "MAP4_AuthoredFish");
            foreach (float x in new[] { 3f, 7f, 15f, 21f })
            {
                Place("MAP4_BorderSeaweed", seaweed, new Vector2(x, 0.7f), new Vector2(1.1f, 1.7f), -50);
                Place("MAP4_BorderSeaweed", seaweed, new Vector2(x, 13.3f), new Vector2(1.1f, 1.7f), -50);
            }
            Place("MAP4_Vent", vent, new Vector2(3.2f, 11.9f), new Vector2(2.1f, 0.75f), -50);
            Place("MAP4_ActiveVent", activeVent, new Vector2(13f, 12f), new Vector2(2.1f, 1.3f), -50);
            Place("MAP4_BubbleVentUpper", vent, new Vector2(4.8f, 8.6f), new Vector2(0.7f, 0.3f), -50);
            Place("MAP4_BubbleVentLower", vent, new Vector2(17.1f, 5.4f), new Vector2(0.7f, 0.3f), -50);
            Place("MAP4_PearlPile", pearlPile, new Vector2(21.8f, 12.8f), new Vector2(1.5f, 1f), -50);
            Place("MAP4_Fish", fish, new Vector2(11.3f, 11.6f), new Vector2(1.9f, 1f), -50);
        }

        private static List<PlantingSpot> BuildSpots()
        {
            Sprite rock = Crop(new Rect(930f, 63f, 190f, 100f), "MAP4_AuthoredBuildRocks");
            List<PlantingSpot> spots = new List<PlantingSpot>();
            foreach (PlantingSpotDefinition definition in Map4SourceDefinition.Definition.PlantingSpots)
            {
                Place("MAP4_AuthoredPatch_" + definition.Id, rock, definition.Position, new Vector2(1.6f, 0.85f), -80);
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
