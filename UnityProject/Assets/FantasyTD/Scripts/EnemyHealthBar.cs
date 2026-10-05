using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        private EnemyUnit _enemy;
        private RectTransform _fill;
        private Image _fillImage;
        private Text _status;
        private float _width;

        // Exposed to runtime acceptance checks without depending on rendering primitives.
        public Transform FillTransform => _fill;

        public void Initialize(EnemyUnit enemy, bool boss)
        {
            _enemy = enemy;
            _width = (boss ? 1.55f : 0.86f) * 100f;
            float y = (boss ? 1.42f : (enemy.Kind == EnemyKind.Brute ? 1.08f : 0.82f)) * 100f;
            float h = (boss ? 0.13f : 0.09f) * 100f;

            GameObject root = new GameObject("HealthCanvas", typeof(RectTransform), typeof(Canvas));
            root.transform.SetParent(transform, false);
            root.transform.localScale = Vector3.one * 0.01f;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 200f);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 95;

            CreateBar(root.transform, "HpBarBack", new Vector2(0f, y),
                new Vector2(_width + 8f, h + 5f), new Color(0.06f, 0.035f, 0.025f, 0.92f));
            _fillImage = CreateBar(root.transform, "HpBarFill", new Vector2(0f, y),
                new Vector2(_width, h), boss ? new Color(0.82f, 0.18f, 0.24f) : new Color(0.34f, 0.84f, 0.24f));
            _fill = _fillImage.rectTransform;

            _status = RuntimeArt.CreateText(root.transform, "StatusCue", boss ? 20 : 17,
                TextAnchor.MiddleCenter, Color.white);
            _status.rectTransform.anchorMin = _status.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _status.rectTransform.sizeDelta = new Vector2(140f, 28f);
            _status.rectTransform.anchoredPosition = new Vector2(0f, y + (boss ? 25f : 21f));
            _status.fontStyle = FontStyle.Bold;
            Shadow shadow = _status.gameObject.AddComponent<Shadow>();
            shadow.effectColor = Color.black;
            shadow.effectDistance = new Vector2(1f, -1f);
            Refresh();
        }

        private static Image CreateBar(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            // A plain UI progress fill is functional UI, not generated decorative art.
            image.sprite = null;
            image.color = color;
            image.raycastTarget = false;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            image.rectTransform.sizeDelta = size;
            image.rectTransform.anchoredPosition = position;
            return image;
        }

        public void Refresh()
        {
            if (_enemy == null || _fill == null) return;
            float ratio = _enemy.MaxHp <= 0f ? 0f : Mathf.Clamp01(_enemy.CurrentHp / _enemy.MaxHp);
            _fill.localScale = new Vector3(ratio, 1f, 1f);
            Vector2 p = _fill.anchoredPosition;
            p.x = -(_width * (1f - ratio)) * 0.5f;
            _fill.anchoredPosition = p;
            if (!_enemy.IsBoss)
                _fillImage.color = ratio > 0.55f ? new Color(0.34f, 0.84f, 0.24f)
                    : (ratio > 0.25f ? new Color(0.95f, 0.67f, 0.16f) : new Color(0.90f, 0.20f, 0.16f));
        }

        private void LateUpdate()
        {
            if (_enemy == null || _status == null) return;
            _status.text = _enemy.IsRooted ? "KORZENIE" : (_enemy.IsSlowed ? "MRÓZ" : "");
            _status.color = _enemy.IsRooted ? new Color(0.75f, 1f, 0.38f) : new Color(0.55f, 0.94f, 1f);
        }
    }
}
