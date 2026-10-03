using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public static class RuntimeArt
    {
        // UI uses Unity's functional solid-color Images. Decorative art must be imported,
        // never synthesized with textures or geometric sprite fallbacks.
        private static Font _font;

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
            sr.sprite = sprite;
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
            GameObject go = new GameObject(
                "Button_" + label,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));

            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = background;

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

        public static Image CreateArtImage(Transform parent, string name, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = sprite != null;
            return image;
        }
    }
}
