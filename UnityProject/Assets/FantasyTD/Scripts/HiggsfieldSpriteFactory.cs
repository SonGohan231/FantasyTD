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

        public static Sprite SeedShooter => _seedShooter ??= CreateCutout(
            "Higgsfield/SEED_SHOOTER_SHEET", 0f, 0f, 0.515f, 1f, "Higgsfield_Pedostrzal");

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
        }

        private static void Require(Sprite sprite, string label)
        {
            if (sprite == null)
                throw new InvalidOperationException("Missing required Higgsfield production sprite: " + label);
        }

        private static Sprite CreateCutout(
            string resourcePath,
            float xMinN,
            float yMinN,
            float xMaxN,
            float yMaxN,
            string name)
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
                    finalPixels[targetRow + x] = cropped[sourceRow + minX + x];
            }

            result.SetPixels32(finalPixels);
            result.Apply(false, true);

            Sprite sprite = Sprite.Create(
                result,
                new Rect(0f, 0f, finalWidth, finalHeight),
                new Vector2(0.5f, 0.08f),
                100f);
            sprite.name = name;
            return sprite;
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
