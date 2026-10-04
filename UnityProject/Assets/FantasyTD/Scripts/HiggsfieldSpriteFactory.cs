using System;
using UnityEngine;

namespace FantasyTD.MapA
{
    public static class HiggsfieldSpriteFactory
    {
        private static Sprite _seedShooter;
        private static Sprite _essenceFlower;
        private static Sprite _oakGuardian;
        private static Sprite _frostBud;
        private static Sprite _wanderer;
        private static Sprite _runner;
        private static Sprite _brute;
        private static Sprite _armored;
        private static Sprite _boss;
        private static Sprite _portal;
        private static Sprite _homeTree;
        private static Sprite[] _wandererWalk;
        private static Sprite _seedShot;
        private static Sprite _mutationTower;

        public static Sprite MutationTower => _mutationTower ??= CreateCutout(
            "Higgsfield/Maps/MAP5_MUTATION_TOWER", 0.21f,0.52f,0.49f,0.96f,"Higgsfield_MutacyjnyPak", isolateForeground:true);

        // Authored locomotion poses on the bottom row of artifact 11219935723.
        // The last pose faces left in the source, so it is mirrored to match the others.
        public static Sprite[] WandererWalk => _wandererWalk ??= new[]
        {
            CreateCutout("Higgsfield/WANDERER_SHEET", 0.455f, 0f, 0.635f, 0.47f, "Chochlik_Walk_0", false, true),
            CreateCutout("Higgsfield/WANDERER_SHEET", 0.635f, 0f, 0.808f, 0.47f, "Chochlik_Walk_1", false, true),
            CreateCutout("Higgsfield/WANDERER_SHEET", 0.808f, 0f, 1f, 0.47f, "Chochlik_Walk_2", true, true)
        };

        // The isolated flying seed above the small bud in artifact 11219567888.
        // This is a source crop, not a generated primitive or a new illustration.
        public static Sprite SeedShot => _seedShot ??= CreateCutout(
            "Higgsfield/SEED_SHOOTER_SHEET", 0.70f, 0.458f, 0.719f, 0.496f,
            "Pedostrzal_AuthoredSeedShot", isolateForeground: true);

        public static Sprite SeedShooter => _seedShooter ??= CreateCutout(
            "Higgsfield/SEED_SHOOTER_SHEET", 0f, 0f, 0.515f, 1f, "Higgsfield_Pedostrzal", isolateForeground: true);

        public static Sprite EssenceFlower => _essenceFlower ??= CreateCutout(
            "Higgsfield/ESSENCE_FLOWER", 0f, 0f, 1f, 1f, "Higgsfield_KwiatEsencji", isolateForeground: true);

        public static Sprite OakGuardian => _oakGuardian ??= CreateCutout(
            "Higgsfield/OAK_GUARDIAN", 0f, 0f, 1f, 1f, "Higgsfield_DebowyStraznik", isolateForeground: true);

        public static Sprite FrostBud => _frostBud ??= CreateCutout(
            "Higgsfield/FROST_BUD", 0f, 0f, 1f, 1f, "Higgsfield_MroznyPak", isolateForeground: true);

        public static Sprite Wanderer => _wanderer ??= CreateCutout(
            "Higgsfield/WANDERER_SHEET", 0f, 0f, 0.455f, 1f, "Higgsfield_Chochlik", isolateForeground: true);

        public static Sprite Runner => _runner ??= CreateCutout(
            "Higgsfield/RUNNER", 0f, 0f, 1f, 1f, "Higgsfield_WichrowySkrzat", isolateForeground: true);

        public static Sprite Brute => _brute ??= CreateCutout(
            "Higgsfield/BRUTE", 0f, 0f, 1f, 1f, "Higgsfield_OgrZarosli", isolateForeground: true);

        public static Sprite Armored => _armored ??= CreateCutout(
            "Higgsfield/ARMORED", 0f, 0f, 1f, 1f, "Higgsfield_ZelaznyGoblin", isolateForeground: true);

        public static Sprite Boss => _boss ??= CreateCutout(
            "Higgsfield/BOSS", 0f, 0f, 1f, 1f, "Higgsfield_KrolCierni", isolateForeground: true);

        public static Sprite Portal => _portal ??= CreateCutout(
            "Higgsfield/PORTAL", 0f, 0f, 1f, 1f, "Higgsfield_Portal", isolateForeground: true);

        public static Sprite HomeTree => _homeTree ??= CreateCutout(
            "Higgsfield/HOME_TREE", 0f, 0f, 1f, 1f, "Higgsfield_HomeTree", isolateForeground: true);

        public static Sprite Plant(PlantKind kind)
        {
            return kind switch
            {
                PlantKind.SeedShooter => SeedShooter,
                PlantKind.EssenceFlower => EssenceFlower,
                PlantKind.OakGuardian => OakGuardian,
                PlantKind.FrostBud => FrostBud,
                PlantKind.MutationTower => MutationTower,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        public static Sprite Enemy(EnemyKind kind)
        {
            return kind switch
            {
                EnemyKind.Wanderer => Wanderer,
                EnemyKind.Runner => Runner,
                EnemyKind.Brute => Brute,
                EnemyKind.Armored => Armored,
                EnemyKind.Boss => Boss,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        public static void ValidateProductionAssets()
        {
            Require(SeedShooter, "Pędostrzał");
            Require(EssenceFlower, "Kwiat Esencji");
            Require(OakGuardian, "Dębowy Strażnik");
            Require(FrostBud, "Mroźny Pąk");
            Require(Wanderer, "Chochlik");
            Require(Runner, "Wichrowy Skrzat");
            Require(Brute, "Ogr Zarośli");
            Require(Armored, "Żelazny Goblin");
            Require(Boss, "Król Cierni");
            Require(Portal, "Portal");
            Require(HomeTree, "Baza-drzewo");
            Require(SeedShot, "Źródłowy pocisk nasienny");
            foreach (Sprite frame in WandererWalk) Require(frame, "Źródłowa poza chodu Chochlika");
        }

        private static void Require(Sprite sprite, string label)
        {
            if (sprite == null)
                throw new InvalidOperationException("Missing required Higgsfield production sprite: " + label);
        }

        // Retain exact approved crop dimensions for authored map tile placement.
        public static Sprite SourceCropUntrimmed(string resourcePath, Rect normalizedCrop, string name)
        {
            return CreateCutout(resourcePath, normalizedCrop.xMin, normalizedCrop.yMin,
                normalizedCrop.xMax, normalizedCrop.yMax, name, false, false, false, true);
        }

        private static Sprite CreateCutout(
            string resourcePath,
            float xMinN,
            float yMinN,
            float xMaxN,
            float yMaxN,
            string name,
            bool mirror = false,
            bool isolateForeground = false,
            bool trim = true,
            bool centeredPivot = false)
        {
            Texture2D sheet = Resources.Load<Texture2D>(resourcePath);
            if (sheet == null)
            {
                Debug.LogError("Required Higgsfield texture missing: " + resourcePath);
                return null;
            }

            if (!sheet.isReadable)
            {
                Debug.LogError("Higgsfield texture is not readable: " + resourcePath);
                return null;
            }

            int x0 = Mathf.Clamp(Mathf.RoundToInt(sheet.width * xMinN), 0, sheet.width - 1);
            int y0 = Mathf.Clamp(Mathf.RoundToInt(sheet.height * yMinN), 0, sheet.height - 1);
            int x1 = Mathf.Clamp(Mathf.RoundToInt(sheet.width * xMaxN), x0 + 1, sheet.width);
            int y1 = Mathf.Clamp(Mathf.RoundToInt(sheet.height * yMaxN), y0 + 1, sheet.height);
            int width = x1 - x0;
            int height = y1 - y0;

            Color32[] pixels = sheet.GetPixels32();

            Color32[] cropped = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                int sourceRow = (y0 + y) * sheet.width + x0;
                int targetRow = y * width;
                for (int x = 0; x < width; x++)
                    cropped[targetRow + x] = pixels[sourceRow + x];
            }

            Color32[] backgroundPalette = SampleBackgroundPalette(cropped, width, height);
            bool[] connectedBackground = FindConnectedBackground(cropped, width, height, backgroundPalette);
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Color32 output = cropped[index];
                    float paletteDistance = DistanceToPalette(output, backgroundPalette);
                    bool background = connectedBackground[index] || paletteDistance <= 0.075f;
                    output.a = background ? (byte)0 : (byte)255;
                    cropped[index] = output;

                    if (output.a >= 48)
                    {
                        minX = Mathf.Min(minX, x);
                        minY = Mathf.Min(minY, y);
                        maxX = Mathf.Max(maxX, x);
                        maxY = Mathf.Max(maxY, y);
                    }
                }
            }

            FeatherForegroundEdge(cropped, width, height);

            if (isolateForeground)
            {
                KeepLargestForeground(cropped, width, height);
                minX = width; minY = height; maxX = -1; maxY = -1;
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    if (cropped[y * width + x].a < 48) continue;
                    minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y);
                }
            }

            if (maxX < minX || maxY < minY)
            {
                Debug.LogError("Higgsfield cutout produced no foreground pixels: " + resourcePath);
                return null;
            }

            const int padding = 12;
            minX = Mathf.Max(0, minX - padding);
            minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(width - 1, maxX + padding);
            maxY = Mathf.Min(height - 1, maxY + padding);

            if (!trim)
            {
                minX = 0; minY = 0; maxX = width - 1; maxY = height - 1;
            }

            int finalWidth = maxX - minX + 1;
            int finalHeight = maxY - minY + 1;
            Texture2D result = new Texture2D(finalWidth, finalHeight, TextureFormat.RGBA32, false);
            result.name = name + "_Texture";
            result.filterMode = FilterMode.Bilinear;
            result.wrapMode = TextureWrapMode.Clamp;

            Color32[] finalPixels = new Color32[finalWidth * finalHeight];
            for (int y = 0; y < finalHeight; y++)
            {
                int sourceRow = (minY + y) * width;
                int targetRow = y * finalWidth;
                for (int x = 0; x < finalWidth; x++)
                    finalPixels[targetRow + x] = cropped[sourceRow + minX + (mirror ? finalWidth - 1 - x : x)];
            }

            result.SetPixels32(finalPixels);
            result.Apply(false, true);

            Sprite sprite = Sprite.Create(
                result,
                new Rect(0f, 0f, finalWidth, finalHeight),
                centeredPivot ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0.08f),
                100f);
            sprite.name = name;
            return sprite;
        }

        // Remove disconnected fragments of neighboring drawings, never repair/synthesize pixels.
        private static void KeepLargestForeground(Color32[] pixels, int width, int height)
        {
            int[] labels = new int[pixels.Length];
            int[] queue = new int[pixels.Length];
            int label = 0, largestLabel = 0, largestCount = 0;
            for (int start = 0; start < pixels.Length; start++)
            {
                if (pixels[start].a < 48 || labels[start] != 0) continue;
                label++;
                int read = 0, count = 1;
                queue[0] = start; labels[start] = label;
                while (read < count)
                {
                    int index = queue[read++], px = index % width, py = index / width;
                    for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int x = px + dx, y = py + dy;
                        if (x < 0 || x >= width || y < 0 || y >= height) continue;
                        int next = y * width + x;
                        if (pixels[next].a < 48 || labels[next] != 0) continue;
                        labels[next] = label; queue[count++] = next;
                    }
                }
                if (count > largestCount) { largestCount = count; largestLabel = label; }
            }
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (labels[index] == largestLabel) continue;
                bool edge = false;
                // Retain only the original antialias fringe adjoining the retained figure.
                if (pixels[index].a < 48)
                    for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && nx < width && ny >= 0 && ny < height &&
                            labels[ny * width + nx] == largestLabel) edge = true;
                    }
                if (!edge) pixels[index].a = 0;
            }
        }

        private static Color32[] SampleBackgroundPalette(Color32[] pixels, int width, int height)
        {
            const int samplesPerEdge = 16;
            Color32[] palette = new Color32[samplesPerEdge * 4];
            int insetX = Mathf.Min(Mathf.Max(1, width / 100), Mathf.Max(0, width - 1));
            int insetY = Mathf.Min(Mathf.Max(1, height / 100), Mathf.Max(0, height - 1));
            int index = 0;
            for (int i = 0; i < samplesPerEdge; i++)
            {
                float t = samplesPerEdge == 1 ? 0.5f : (float)i / (samplesPerEdge - 1);
                int x = Mathf.Clamp(Mathf.RoundToInt(t * (width - 1)), 0, width - 1);
                int y = Mathf.Clamp(Mathf.RoundToInt(t * (height - 1)), 0, height - 1);
                palette[index++] = pixels[insetY * width + x];
                palette[index++] = pixels[(height - 1 - insetY) * width + x];
                palette[index++] = pixels[y * width + insetX];
                palette[index++] = pixels[y * width + width - 1 - insetX];
            }
            return palette;
        }

        private static bool[] FindConnectedBackground(
            Color32[] pixels, int width, int height, Color32[] palette)
        {
            bool[] background = new bool[pixels.Length];
            int[] queue = new int[pixels.Length];
            int read = 0;
            int count = 0;

            for (int x = 0; x < width; x++)
            {
                TryQueueBackground(x, 0, pixels, width, height, palette, background, queue, ref count);
                TryQueueBackground(x, height - 1, pixels, width, height, palette, background, queue, ref count);
            }
            for (int y = 1; y < height - 1; y++)
            {
                TryQueueBackground(0, y, pixels, width, height, palette, background, queue, ref count);
                TryQueueBackground(width - 1, y, pixels, width, height, palette, background, queue, ref count);
            }

            while (read < count)
            {
                int current = queue[read++];
                int x = current % width;
                int y = current / width;
                TryQueueBackground(x - 1, y, pixels, width, height, palette, background, queue, ref count);
                TryQueueBackground(x + 1, y, pixels, width, height, palette, background, queue, ref count);
                TryQueueBackground(x, y - 1, pixels, width, height, palette, background, queue, ref count);
                TryQueueBackground(x, y + 1, pixels, width, height, palette, background, queue, ref count);
            }

            return background;
        }

        private static void TryQueueBackground(
            int x,
            int y,
            Color32[] pixels,
            int width,
            int height,
            Color32[] palette,
            bool[] background,
            int[] queue,
            ref int count)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            int index = y * width + x;
            if (background[index] || DistanceToPalette(pixels[index], palette) > 0.17f) return;
            background[index] = true;
            queue[count++] = index;
        }

        private static float DistanceToPalette(Color32 color, Color32[] palette)
        {
            float nearest = float.MaxValue;
            for (int i = 0; i < palette.Length; i++)
            {
                float dr = (color.r - palette[i].r) / 255f;
                float dg = (color.g - palette[i].g) / 255f;
                float db = (color.b - palette[i].b) / 255f;
                float distance = Mathf.Sqrt(dr * dr + dg * dg + db * db);
                if (distance < nearest) nearest = distance;
            }
            return nearest;
        }

        private static void FeatherForegroundEdge(Color32[] pixels, int width, int height)
        {
            Color32[] source = (Color32[])pixels.Clone();
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (source[index].a == 0) continue;

                int transparentNeighbors = 0;
                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height ||
                        source[ny * width + nx].a == 0)
                        transparentNeighbors++;
                }

                if (transparentNeighbors == 0) continue;
                Color32 softened = pixels[index];
                softened.a = (byte)Mathf.Clamp(255 - transparentNeighbors * 18, 96, 220);
                pixels[index] = softened;
            }

        }
    }
}
