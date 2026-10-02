using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public static class RuntimeArt
    {
        private static Sprite _white;
        private static Sprite _circle;
        private static Font _font;

        public static Sprite White
        {
            get
            {
                if (_white != null) return _white;
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.name = "RuntimeWhite";
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _white = Sprite.Create(tex, new Rect(0,0,1,1), new Vector2(0.5f,0.5f), 1f);
                return _white;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (_circle != null) return _circle;
                const int size = 96;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.name = "RuntimeOrganicCircle";
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;

                Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                float r = size * 0.47f;

                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x,y), c);
                    float a = Mathf.Clamp01((r - d) * 1.6f);
                    tex.SetPixel(x,y,new Color(1f,1f,1f,a));
                }

                tex.Apply();
                _circle = Sprite.Create(tex, new Rect(0,0,size,size), new Vector2(0.5f,0.5f), size);
                return _circle;
            }
        }

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
            Color color, int sortingOrder, bool circle = true)
        {
            GameObject go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = circle ? Circle : White;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return go;
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
            GameObject go = new GameObject("Button_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = Circle;
            image.type = Image.Type.Simple;
            image.color = background;

            Button button = go.GetComponent<Button>();
            ColorBlock cb = button.colors;
            cb.highlightedColor = background * 1.10f;
            cb.pressedColor = background * 0.80f;
            button.colors = cb;

            Text t = CreateText(go.transform, "Label", fontSize, TextAnchor.MiddleCenter, Color.white);
            RectTransform tr = t.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(8f, 8f);
            tr.offsetMax = new Vector2(-8f, -8f);
            t.text = label;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 10;
            t.resizeTextMaxSize = fontSize;

            return button;
        }
    }
}
