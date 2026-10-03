using UnityEngine;

namespace FantasyTD.MapA
{
    public static class HiggsfieldSpriteFactory
    {
        private static Sprite _seedShooter;
        private static Sprite _wanderer;
        private static bool _seedAttempted;
        private static bool _wandererAttempted;

        public static Sprite SeedShooter
        {
            get
            {
                if (!_seedAttempted)
                {
                    _seedAttempted = true;
                    _seedShooter = CreateCutout(
                        "Higgsfield/SEED_SHOOTER_SHEET",
                        0f, 0f, 0.515f, 1f,
                        "Higgsfield_Pedostrzal");
                }
                return _seedShooter;
            }
        }

        public static Sprite Wanderer
        {
            get
            {
                if (!_wandererAttempted)
                {
                    _wandererAttempted = true;
                    _wanderer = CreateCutout(
                        "Higgsfield/WANDERER_SHEET",
                        0f, 0f, 0.455f, 1f,
                        "Higgsfield_Chochlik");
                }
                return _wanderer;
            }
        }

        public static bool HasSeedShooter => SeedShooter != null;
        public static bool HasWanderer => Wanderer != null;

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
                return null;

            if (!sheet.isReadable)
            {
                Debug.LogError("Higgsfield sheet is not readable: " + resourcePath);
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

                    float alpha = Mathf.InverseLerp(0.07f, 0.18f, distance);
                    alpha = Mathf.Clamp01(alpha);
                    alpha *= alpha * (3f - 2f * alpha);

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
                return null;

            const int padding = 10;
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
