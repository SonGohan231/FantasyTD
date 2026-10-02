using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public static class RuntimeArt
    {
        private enum ShapeKind
        {
            Circle,
            SoftCircle,
            Leaf,
            Petal,
            Diamond,
            Thorn,
            Ring
        }

        private static Sprite _white;
        private static Sprite _circle;
        private static Sprite _softCircle;
        private static Sprite _leaf;
        private static Sprite _petal;
        private static Sprite _diamond;
        private static Sprite _thorn;
        private static Sprite _ring;
        private static Font _font;

        public static Sprite White
        {
            get
            {
                if (_white != null) return _white;
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.name = "RuntimeWhite";
                tex.SetPixel(0, 0, Color.white);
                tex.Apply(false, true);
                _white = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
                return _white;
            }
        }

        public static Sprite Circle => GetShape(ref _circle, "RuntimeOrganicCircle", ShapeKind.Circle);
        public static Sprite SoftCircle => GetShape(ref _softCircle, "RuntimeSoftGlow", ShapeKind.SoftCircle);
        public static Sprite Leaf => GetShape(ref _leaf, "RuntimeLeaf", ShapeKind.Leaf);
        public static Sprite Petal => GetShape(ref _petal, "RuntimePetal", ShapeKind.Petal);
        public static Sprite Diamond => GetShape(ref _diamond, "RuntimeCrystal", ShapeKind.Diamond);
        public static Sprite Thorn => GetShape(ref _thorn, "RuntimeThorn", ShapeKind.Thorn);
        public static Sprite Ring => GetShape(ref _ring, "RuntimeRing", ShapeKind.Ring);

        public static Font Font
        {
            get
            {
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static Material SpriteMaterial(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        public static GameObject SpriteObject(
            string name, Transform parent, Vector2 localPosition, Vector2 size,
            Color color, int sortingOrder, bool circle = true, Sprite sprite = null, float rotation = 0f)
        {
            GameObject go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : (circle ? Circle : White);
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        public static GameObject ShadowObject(
            string name, Transform parent, Vector2 localPosition, Vector2 size, int sortingOrder, float alpha = 0.28f)
        {
            return SpriteObject(
                name, parent, localPosition, size,
                new Color(0.045f, 0.055f, 0.035f, alpha), sortingOrder, true, SoftCircle);
        }

        public static Text CreateText(Transform parent, string name, int fontSize, TextAnchor anchor, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = Font;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(Transform parent, string label, Color background, int fontSize)
        {
            GameObject go = new GameObject(
                "Button_" + label,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(Outline));

            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.sprite = Circle;
            image.type = Image.Type.Simple;
            image.color = background;

            Outline outline = go.GetComponent<Outline>();
            outline.effectColor = new Color(0.035f, 0.025f, 0.015f, 0.78f);
            outline.effectDistance = new Vector2(3f, -4f);
            outline.useGraphicAlpha = true;

            GameObject sheenGo = new GameObject(
                "Sheen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sheenGo.transform.SetParent(go.transform, false);
            RectTransform sheenRect = sheenGo.GetComponent<RectTransform>();
            sheenRect.anchorMin = new Vector2(0.08f, 0.56f);
            sheenRect.anchorMax = new Vector2(0.92f, 0.92f);
            sheenRect.offsetMin = Vector2.zero;
            sheenRect.offsetMax = Vector2.zero;
            Image sheen = sheenGo.GetComponent<Image>();
            sheen.sprite = Circle;
            sheen.color = new Color(1f, 1f, 1f, 0.10f);
            sheen.raycastTarget = false;

            Button button = go.GetComponent<Button>();
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            cb.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.72f);
            button.colors = cb;
            button.targetGraphic = image;

            Text t = CreateText(go.transform, "Label", fontSize, TextAnchor.MiddleCenter, Color.white);
            RectTransform tr = t.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(8f, 8f);
            tr.offsetMax = new Vector2(-8f, -8f);
            t.text = label;
            t.fontStyle = FontStyle.Bold;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 10;
            t.resizeTextMaxSize = fontSize;

            Shadow shadow = t.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.62f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return button;
        }

        private static Sprite GetShape(ref Sprite cache, string name, ShapeKind shape)
        {
            if (cache != null) return cache;

            const int size = 96;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = name;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float px = ((x + 0.5f) / size) * 2f - 1f;
                float py = ((y + 0.5f) / size) * 2f - 1f;
                float signedDistance = SignedDistance(shape, px, py);
                float alpha;

                if (shape == ShapeKind.SoftCircle)
                {
                    float r = Mathf.Sqrt(px * px + py * py);
                    alpha = Mathf.Clamp01((1f - r) / 0.30f);
                    alpha *= alpha;
                }
                else
                {
                    alpha = Mathf.Clamp01(0.5f - signedDistance * size * 0.30f);
                }

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }

            tex.Apply(false, true);
            cache = Sprite.Create(
                tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return cache;
        }

        private static float SignedDistance(ShapeKind shape, float x, float y)
        {
            switch (shape)
            {
                case ShapeKind.Circle:
                    return Mathf.Sqrt(x * x + y * y) - 0.92f;

                case ShapeKind.Leaf:
                {
                    float ay = Mathf.Abs(y);
                    float halfWidth = 0.12f + 0.70f * Mathf.Max(0f, 1f - ay);
                    float dx = Mathf.Abs(x) - halfWidth;
                    float dy = ay - 0.94f;
                    return Mathf.Max(dx, dy);
                }

                case ShapeKind.Petal:
                {
                    float yy = y + 0.10f;
                    float ellipse = Mathf.Sqrt(
                        (x * x) / (0.62f * 0.62f) +
                        (yy * yy) / (0.92f * 0.92f)) - 1f;
                    float taper = Mathf.Abs(x) - (0.15f + (y + 1f) * 0.30f);
                    return Mathf.Max(ellipse, y < -0.35f ? taper : ellipse);
                }

                case ShapeKind.Diamond:
                    return Mathf.Abs(x) + Mathf.Abs(y) - 0.93f;

                case ShapeKind.Thorn:
                {
                    float yy = Mathf.Clamp01((y + 0.92f) / 1.84f);
                    float width = Mathf.Lerp(0.70f, 0.03f, yy);
                    return Mathf.Max(Mathf.Abs(x) - width, Mathf.Max(-0.92f - y, y - 0.95f));
                }

                case ShapeKind.Ring:
                {
                    float r = Mathf.Sqrt(x * x + y * y);
                    return Mathf.Abs(r - 0.72f) - 0.095f;
                }

                default:
                    return Mathf.Sqrt(x * x + y * y) - 0.92f;
            }
        }
    }
}
