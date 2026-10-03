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
            "Higgsfield/SEED_SHOOTER_SHEET", 0.70f, 0.458f, 0.719f, 0.496f, "Pedostrzal_AuthoredSeedShot");

        public static Sprite SeedShooter => _seedShooter ??= CreateCutout(
            "Higgsfield/SEED_SHOOTER_SHEET", 0f, 0f, 0.515f, 1f, "Higgsfield_Pedostrzal", isolateForeground: true);

        public static Sprite EssenceFlower => _essenceFlower ??= CreateCutout(
            "Higgsfield/ESSENCE_FLOWER", 0f, 0f, 1f, 1f, "Higgsfield_KwiatEsencji");

        public static Sprite OakGuardian => _oakGuardian ??= CreateCutout(
            "Higgsfield/OAK_GUARDIAN", 0f, 0f, 1f, 1f, "Higgsfield_DebowyStraznik");

        public static Sprite FrostBud => _frostBud ??= CreateCutout(
            "Higgsfield/FROST_BUD", 0f, 0f, 1f, 1f, "Higgsfield_MroznyPak");

        public static Sprite Wanderer => _wanderer ??= CreateCutout(
            "Higgsfield/WANDERER_SHEET", 0f, 0f, 0.455f, 1f, "Higgsfield_Chochlik");

        public static Sprite Runner => _runner ??= CreateCutout(
            "Higgsfield/RUNNER", 0f, 0f, 1f, 1f, "Higgsfield_WichrowySkrzat");

        public static Sprite Brute => _brute ??= CreateCutout(
            "Higgsfield/BRUTE", 0f, 0f, 1f, 1f, "Higgsfield_OgrZarosli");

        public static Sprite Armored => _armored ??= CreateCutout(
            "Higgsfield/ARMORED", 0f, 0f, 1f, 1f, "Higgsfield_ZelaznyGoblin");

        public static Sprite Boss => _boss ??= CreateCutout(
            "Higgsfield/BOSS", 0f, 0f, 1f, 1f, "Higgsfield_KrolCierni");

        public static Sprite Portal => _portal ??= CreateCutout(
            "Higgsfield/PORTAL", 0f, 0f, 1f, 1f, "Higgsfield_Portal");

        public static Sprite HomeTree => _homeTree ??= CreateCutout(
            "Higgsfield/HOME_TREE", 0f, 0f, 1f, 1f, "Higgsfield_HomeTree");

        public static Sprite Plant(PlantKind kind)
        {
            return kind switch
            {
                PlantKind.SeedShooter => SeedShooter,
                PlantKind.EssenceFlower => EssenceFlower,
                PlantKind.OakGuardian => OakGuardian,
                PlantKind.FrostBud => FrostBud,
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
            Color bg = AverageBackground(pixels, sheet.width, sheet.height, x0, y0, x1, y1);

            Color32[] cropped = new Color32[width * height];
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < height; y++)
            {
                int sourceY = y0 + y;
                for (int x = 0; x < width; x++)
                {
                    int sourceX = x0 + x;
                    Color c = pixels[sourceY * sheet.width + sourceX];
                    float distance = Mathf.Sqrt(
                        (c.r - bg.r) * (c.r - bg.r) +
                        (c.g - bg.g) * (c.g - bg.g) +
                        (c.b - bg.b) * (c.b - bg.b));

                    float alpha = Mathf.InverseLerp(0.055f, 0.17f, distance);
                    alpha = Mathf.Clamp01(alpha);
                    alpha = alpha * alpha * (3f - 2f * alpha);

                    Color32 output = c;
                    output.a = (byte)Mathf.RoundToInt(alpha * 255f);
                    cropped[y * width + x] = output;

                    if (output.a >= 48)
                    {
                        minX = Mathf.Min(minX, x);
                        minY = Mathf.Min(minY, y);
                        maxX = Mathf.Max(maxX, x);
                        maxY = Mathf.Max(maxY, y);
                    }
                }
            }

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

        private static Color AverageBackground(
            Color32[] pixels,
            int textureWidth,
            int textureHeight,
            int x0,
            int y0,
            int x1,
            int y1)
        {
            int insetX = Mathf.Max(2, (x1 - x0) / 50);
            int insetY = Mathf.Max(2, (y1 - y0) / 50);
            Color total = Color.black;
            int count = 0;

            int[] xs = { x0 + insetX, x1 - 1 - insetX };
            int[] ys = { y0 + insetY, y1 - 1 - insetY };

            for (int yi = 0; yi < ys.Length; yi++)
            for (int xi = 0; xi < xs.Length; xi++)
            {
                int x = Mathf.Clamp(xs[xi], 0, textureWidth - 1);
                int y = Mathf.Clamp(ys[yi], 0, textureHeight - 1);
                total += (Color)pixels[y * textureWidth + x];
                count++;
            }

            return count > 0 ? total / count : Color.white;
        }
    }
}
