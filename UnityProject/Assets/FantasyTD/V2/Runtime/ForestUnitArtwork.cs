using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.V2.Runtime
{
    /// <summary>
    /// Uses existing approved forest illustration sources. Generates a
    /// cached alpha-masked, main-character cutout instead of rendering an
    /// entire beige concept sheet (and its detail panels) as a game unit.
    /// Source images are capped at 1024 pixels by the dedicated FT2 importer.
    /// </summary>
    public static class ForestUnitArtwork
    {
        private static readonly Dictionary<string, Sprite> Cache =
            new Dictionary<string, Sprite>();

        public static Sprite Defender(string id)
        {
            switch (id)
            {
                case "thorn": return Get("ThornShooter", .515f);
                case "frost": return Get("FrostBud", 1f);
                case "bloom": return Get("EssenceFlower", 1f);
                default: return null;
            }
        }

        public static Sprite Enemy(string id)
        {
            switch (id)
            {
                case "imp": return Get("Wanderer", .455f);
                case "runner": return Get("Runner", 1f);
                case "armored": return Get("Armored", 1f);
                default: return null;
            }
        }

        private static Sprite Get(string name, float mainFigureFraction)
        {
            Sprite cached;
            if (Cache.TryGetValue(name, out cached)) return cached;

            string path = "FT2/Units/" + name;
            Sprite source = Resources.Load<Sprite>(path);
            if (source == null)
            {
                Debug.LogWarning("FT2 optional visual missing: Resources/" + path);
                return null;
            }

            Texture2D texture = source.texture;
            if (texture == null || !texture.isReadable)
            {
                Debug.LogWarning("FT2 art texture is not readable; showing imported sprite: " + path);
                Cache[name] = source;
                return source;
            }

            int width = Mathf.Clamp(Mathf.RoundToInt(texture.width * mainFigureFraction),
                1, texture.width);
            int height = texture.height;
            Color32[] input = texture.GetPixels32();
            Color32[] cutout = new Color32[width * height];

            // Corners sample the neutral backdrop of the authored concept sheets.
            // This is an approximation for the first vertical slice, not a final
            // hand-masked/rigged sprite production process.
            Color32 a = input[(height - 1) * texture.width];
            Color32 b = input[(height - 1) * texture.width + width - 1];
            Color32 c = input[0];
            int bgR = (a.r + b.r + c.r) / 3;
            int bgG = (a.g + b.g + c.g) / 3;
            int bgB = (a.b + b.b + c.b) / 3;

            for (int y = 0; y < height; y++)
            {
                int row = y * texture.width;
                for (int x = 0; x < width; x++)
                {
                    Color32 color = input[row + x];
                    float difference = (Mathf.Abs(color.r - bgR) +
                        Mathf.Abs(color.g - bgG) + Mathf.Abs(color.b - bgB)) / 3f;
                    float opacity = Mathf.SmoothStep(0f, 1f,
                        Mathf.Clamp01((difference - 12f) / 48f));
                    color.a = (byte)Mathf.Clamp(Mathf.RoundToInt(
                        color.a * opacity), 0, 255);
                    cutout[y * width + x] = color;
                }
            }

            // Trim empty margins so imported portrait sheets look like small
            // battlefield silhouettes and don't cover the path with a giant quad.
            int minX = width, minY = height, maxX = -1, maxY = -1;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (cutout[y * width + x].a > 70)
                {
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            if (maxX < minX || maxY < minY)
            {
                Cache[name] = source;
                return source;
            }

            int outputWidth = maxX - minX + 1;
            int outputHeight = maxY - minY + 1;
            var output = new Texture2D(outputWidth, outputHeight,
                TextureFormat.RGBA32, false);
            output.name = "FT2_" + name + "_Cutout";
            output.filterMode = FilterMode.Bilinear;
            var outputPixels = new Color32[outputWidth * outputHeight];
            for (int y = 0; y < outputHeight; y++)
                Array.Copy(cutout, (y + minY) * width + minX,
                    outputPixels, y * outputWidth, outputWidth);
            output.SetPixels32(outputPixels);
            output.Apply(false, true);
            Sprite result = Sprite.Create(output,
                new Rect(0f, 0f, outputWidth, outputHeight),
                new Vector2(.5f, .09f), 100f);
            result.name = "FT2_" + name + "_Sprite";
            Cache[name] = result;
            return result;
        }
    }
}
