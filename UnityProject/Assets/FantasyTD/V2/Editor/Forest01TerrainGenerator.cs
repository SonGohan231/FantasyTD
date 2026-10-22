#if UNITY_EDITOR
using System;
using System.IO;
using FantasyTD.V2.Terrain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace FantasyTD.V2.Editor
{
    /// <summary>
    /// Generates a separate, editable 28x20 isometric terrain scene.
    /// Never touches the existing playable Forest01.unity or FT2 source images.
    /// </summary>
    public static class Forest01TerrainGenerator
    {
        public const string ScenePath =
            "Assets/FantasyTD/V2/Scenes/Forest01_TerrainTest.unity";
        private const string Generated =
            "Assets/FantasyTD/V2/TerrainTest/Generated";
        private const int TileWidth = 128;
        private const int TileHeight = 64;

        [MenuItem("Tools/Fantasy TD V2/Generate Forest01 Isometric Terrain")]
        public static void BuildScene()
        {
            Forest01TerrainLayout.Validate();
            EnsureFolders();

            Tile grassA = EnsureTile("Grass_A", EnsureSprite("Grass_A", "grassA", TileWidth, TileHeight));
            Tile grassB = EnsureTile("Grass_B", EnsureSprite("Grass_B", "grassB", TileWidth, TileHeight));
            Tile sand = EnsureTile("Sand_Path", EnsureSprite("Sand_Path", "sand", TileWidth, TileHeight));
            Tile waterTile = EnsureTile("Water", EnsureSprite("Water", "water", TileWidth, TileHeight));

            Sprite siteSprite = EnsureSprite("Build_Site", "site", TileWidth, TileHeight);
            Sprite treeSprite = EnsureSprite("Tree", "tree", 128, 128);
            Sprite rockSprite = EnsureSprite("Rock", "rock", 128, 128);
            Sprite gateSprite = EnsureSprite("Gate", "gate", 128, 128);
            Sprite bridgeSprite = EnsureSprite("Bridge", "bridge", 128, 128);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject world = new GameObject("Forest01_TerrainRoot");
            Grid grid = world.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(1f, .5f, 1f);
            world.AddComponent<Forest01TerrainInteraction>();

            Tilemap ground = Layer(grid, "Ground_560_Tiles", 0);
            Tilemap water = Layer(grid, "Water_Pond", 1);
            Tilemap road = Layer(grid, "Road_Continuous", 2);

            for (int x = 0; x < Forest01TerrainLayout.Width; x++)
                for (int y = 0; y < Forest01TerrainLayout.Height; y++)
                    ground.SetTile(new Vector3Int(x, y, 0),
                        ((x * 13 + y * 7) % 9 == 0) ? grassB : grassA);

            foreach (Forest01TerrainLayout.Cell cell in Forest01TerrainLayout.Water)
                water.SetTile(cell.Vector, waterTile);
            foreach (Forest01TerrainLayout.Cell cell in Forest01TerrainLayout.Road)
                road.SetTile(cell.Vector, sand);
            ground.CompressBounds();
            water.CompressBounds();
            road.CompressBounds();

            GameObject decorations = new GameObject("Decorations_Editable");
            decorations.transform.SetParent(world.transform, false);
            for (int i = 0; i < Forest01TerrainLayout.Trees.Length; i++)
                Decorate(decorations.transform, grid, "Tree_" + (i + 1).ToString("00"),
                    Forest01TerrainLayout.Trees[i], treeSprite, 10, new Vector2(1f, 1.05f));
            for (int i = 0; i < Forest01TerrainLayout.Rocks.Length; i++)
                Decorate(decorations.transform, grid, "Rock_" + (i + 1).ToString("00"),
                    Forest01TerrainLayout.Rocks[i], rockSprite, 9, new Vector2(.7f, .7f));
            Decorate(decorations.transform, grid, "Bridge_Decoration",
                Forest01TerrainLayout.Bridge, bridgeSprite, 12, new Vector2(1.4f, .8f));
            Decorate(decorations.transform, grid, "Start_Gate",
                Forest01TerrainLayout.Road[0], gateSprite, 13, new Vector2(1.3f, 1.4f));
            Decorate(decorations.transform, grid, "Exit_Gate",
                Forest01TerrainLayout.Road[Forest01TerrainLayout.Road.Length - 1],
                gateSprite, 13, new Vector2(1.3f, 1.4f));

            GameObject sites = new GameObject("BuildSites_10_Interactive");
            sites.transform.SetParent(world.transform, false);
            for (int i = 0; i < Forest01TerrainLayout.Sites.Length; i++)
            {
                var cell = Forest01TerrainLayout.Sites[i];
                string id = "B" + (i + 1).ToString("00");
                GameObject marker = new GameObject("BuildSite_" + id);
                marker.transform.SetParent(sites.transform, false);
                marker.transform.position = grid.GetCellCenterWorld(cell.Vector);
                marker.transform.localScale = new Vector3(.75f, .75f, 1f);
                var sprite = marker.AddComponent<SpriteRenderer>();
                sprite.sprite = siteSprite;
                sprite.sortingOrder = 15;
                var collider = marker.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(.94f, .46f);
                collider.isTrigger = true;
                var site = marker.AddComponent<Forest01TerrainBuildSite>();
                site.Configure(id, cell, sprite);
            }

            SetupCamera(grid);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save terrain scene.");
            AssetDatabase.SaveAssets();
            Debug.Log("FT2 Forest01 Terrain: GENERATED " + ScenePath +
                      " (28x20, 560 grass, 40 sand path, 10 sites)");
        }

        private static Tilemap Layer(Grid parent, string name, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            Tilemap map = go.AddComponent<Tilemap>();
            TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = order;
            return map;
        }

        private static void Decorate(Transform parent, Grid grid, string name,
            Forest01TerrainLayout.Cell cell, Sprite image, int order, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.position = grid.GetCellCenterWorld(cell.Vector);
            obj.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = image;
            renderer.sortingOrder = order;
        }

        private static void SetupCamera(Grid grid)
        {
            Vector3[] corners =
            {
                grid.GetCellCenterWorld(new Vector3Int(0, 0, 0)),
                grid.GetCellCenterWorld(new Vector3Int(27, 0, 0)),
                grid.GetCellCenterWorld(new Vector3Int(0, 19, 0)),
                grid.GetCellCenterWorld(new Vector3Int(27, 19, 0))
            };
            Vector3 minimum = corners[0], maximum = corners[0];
            foreach (Vector3 pos in corners)
            {
                minimum = Vector3.Min(minimum, pos);
                maximum = Vector3.Max(maximum, pos);
            }
            Vector3 center = (minimum + maximum) * .5f;
            float width = maximum.x - minimum.x + 2.4f;
            float height = maximum.y - minimum.y + 2.4f;

            GameObject go = new GameObject("Terrain_Test_Orthographic_Camera");
            go.tag = "MainCamera";
            Camera camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(height * .5f,
                width / (16f / 9f) * .5f);
            camera.transform.position = new Vector3(center.x, center.y, -20f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.08f, .15f, .14f, 1f);
            camera.nearClipPlane = .01f;
            camera.farClipPlane = 60f;
        }

        private static void EnsureFolders()
        {
            string folder = Path.Combine(Application.dataPath,
                "FantasyTD/V2/TerrainTest/Generated");
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static Tile EnsureTile(string name, Sprite sprite)
        {
            string path = Generated + "/" + name + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = sprite;
            tile.color = Color.white;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        private static Sprite EnsureSprite(string name, string kind, int width, int height)
        {
            string path = Generated + "/" + name + ".png";
            string absolute = Path.Combine(Application.dataPath,
                "FantasyTD/V2/TerrainTest/Generated", name + ".png");
            if (!File.Exists(absolute))
            {
                Texture2D texture = new Texture2D(width, height,
                    TextureFormat.RGBA32, false);
                Color[] pixels = new Color[width * height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        pixels[y * width + x] = Pixel(kind, x, y, width, height);
                texture.SetPixels(pixels);
                texture.Apply(false);
                File.WriteAllBytes(absolute, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException("No TextureImporter: " + path);
            if (importer.textureType != TextureImporterType.Sprite ||
                importer.textureShape != TextureImporterShape.Texture2D ||
                importer.spritePixelsPerUnit != 128f ||
                importer.mipmapEnabled)
            {
                importer.textureShape = TextureImporterShape.Texture2D;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 128f;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null || sprite.texture == null)
                throw new InvalidOperationException("Sprite failed import: " + path);
            return sprite;
        }

        private static Color Pixel(string kind, int x, int y, int width, int height)
        {
            float nx = (x + .5f - width * .5f) / (width * .5f);
            float ny = (y + .5f - height * .5f) / (height * .5f);
            float seed = (Mathf.Sin(x * 17.41f + y * 41.3f) + 1f) * .5f;

            if (kind == "grassA" || kind == "grassB" ||
                kind == "sand" || kind == "water" || kind == "site")
            {
                float edge = Mathf.Abs(nx) + Mathf.Abs(ny);
                if (edge > .987f) return Color.clear;
                Color baseColor;
                if (kind == "sand")
                    baseColor = edge > .86f
                        ? new Color(.44f, .29f, .16f) : new Color(.75f, .58f, .35f);
                else if (kind == "water")
                    baseColor = edge > .86f
                        ? new Color(.14f, .36f, .38f) : new Color(.24f, .53f, .60f);
                else if (kind == "site")
                    baseColor = edge > .8f
                        ? new Color(.12f, .31f, .21f) : new Color(.35f, .61f, .37f);
                else
                    baseColor = edge > .91f
                        ? new Color(.20f, .34f, .17f)
                        : kind == "grassA" ? new Color(.32f, .53f, .27f)
                                           : new Color(.27f, .49f, .25f);
                float tint = .96f + seed * .035f + Mathf.Clamp01(ny + 1f) * .035f;
                if (kind == "water" && (x + y) % 19 < 3) tint += .11f;
                return new Color(Mathf.Clamp01(baseColor.r * tint),
                    Mathf.Clamp01(baseColor.g * tint),
                    Mathf.Clamp01(baseColor.b * tint), 1f);
            }

            // Simple legible, editable placeholder sprites, not final character art.
            if (kind == "tree")
            {
                bool trunk = Mathf.Abs(nx) < .095f && ny > -.75f && ny < .15f;
                bool crown = (nx * nx / .56f + (ny - .2f) * (ny - .2f) / .48f) < 1f;
                if (trunk) return new Color(.36f, .22f, .11f, 1f);
                if (crown) return new Color(.16f + .09f * seed, .44f + .13f * seed, .21f, 1f);
                return Color.clear;
            }
            if (kind == "rock")
            {
                if (nx * nx / .51f + (ny + .27f) * (ny + .27f) / .24f < 1f)
                    return new Color(.44f + seed * .06f, .49f + seed * .06f, .42f, 1f);
                return Color.clear;
            }
            if (kind == "gate")
            {
                bool frame = ((Mathf.Abs(nx) > .48f && Mathf.Abs(nx) < .69f &&
                               ny > -.8f && ny < .48f) ||
                              (Mathf.Abs(nx) < .69f && ny > .30f && ny < .53f));
                if (frame) return new Color(.45f, .29f, .14f, 1f);
                if (Mathf.Abs(nx) < .48f && ny > -.65f && ny < .30f)
                    return new Color(.20f, .33f + .08f * seed, .24f, .75f);
                return Color.clear;
            }
            if (kind == "bridge")
            {
                if (Mathf.Abs(nx) < .89f && Mathf.Abs(ny) < .39f)
                {
                    bool join = Mathf.Abs((x % 19) - 9.5f) > 8f;
                    return join ? new Color(.34f, .20f, .11f)
                                : new Color(.66f + seed * .045f, .42f, .22f);
                }
                return Color.clear;
            }
            return Color.clear;
        }

        /// <summary>
        /// Capture the actual Unity camera in landscape 16:9 and 20:9.
        /// A PNG is not accepted if it renders as an empty/flat frame.
        /// </summary>
        public static bool RenderPreview(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return false;
            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null) return false;
            return RenderFrame(camera, 1280, 720, "Forest01_TerrainTest_Preview.png")
                && RenderFrame(camera, 1600, 720, "Forest01_TerrainTest_Preview_20x9.png");
        }

        public static bool RenderFrame(Camera camera, int width, int height, string fileName)
        {
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture oldCamera = camera.targetTexture;
            float oldAspect = camera.aspect;
            try
            {
                if (!target.Create())
                    throw new InvalidOperationException("RenderTexture.Create failed: " + fileName);
                camera.targetTexture = target;
                camera.aspect = (float)width / height;
                camera.Render();

                // ReadPixels aborts inside the Vulkan driver when Unity runs through
                // FEX on the ARM64 development host. AsyncGPUReadback still reads the
                // real Camera.Render target, but uses Unity's supported Vulkan transfer
                // path instead of the crashing framebuffer readback path.
                AsyncGPUReadbackRequest readback = AsyncGPUReadback.Request(
                    target, 0, TextureFormat.RGBA32);
                readback.WaitForCompletion();
                if (readback.hasError)
                    throw new InvalidOperationException(
                        "Unity camera GPU readback failed: " + fileName);
                screenshot.LoadRawTextureData(readback.GetData<byte>());
                screenshot.Apply(false);

                Color32 baseline = (Color32)screenshot.GetPixel(0, 0);
                int different = 0;
                for (int sy = 0; sy < 12; sy++)
                    for (int sx = 0; sx < 20; sx++)
                    {
                        Color32 c = (Color32)screenshot.GetPixel(
                            (sx * (width - 1)) / 19, (sy * (height - 1)) / 11);
                        int rgbDistance = Math.Abs(c.r - baseline.r)
                                        + Math.Abs(c.g - baseline.g)
                                        + Math.Abs(c.b - baseline.b);
                        if (rgbDistance > 24) different++;
                    }
                if (different < 10)
                    throw new InvalidOperationException(
                        "Unity camera output is blank/uniform: " + fileName);

                byte[] png = screenshot.EncodeToPNG();
                if (png == null || png.Length < 4096)
                    throw new InvalidOperationException(
                        "Unity camera output PNG is unexpectedly small: " + fileName);
                string logs = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs");
                Directory.CreateDirectory(logs);
                File.WriteAllBytes(Path.Combine(logs, fileName), png);
                Debug.Log("Forest01 real Unity camera preview: " + fileName
                          + " (" + width + "x" + height + ", " + png.Length + " bytes)");
                return true;
            }
            finally
            {
                camera.targetTexture = oldCamera;
                camera.aspect = oldAspect;
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(screenshot);
            }
        }
    }
}
#endif
