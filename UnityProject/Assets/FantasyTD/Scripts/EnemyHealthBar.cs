using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        private Transform _fill;
        private float _width;
        private float _height;

        public void Initialize(bool boss)
        {
            _width = boss ? 1.55f : 0.88f;
            _height = boss ? 0.11f : 0.085f;
            float y = boss ? 0.98f : 0.62f;

            RuntimeArt.SpriteObject(
                "HpBarShadow", transform, new Vector2(0f, y - 0.018f),
                new Vector2(_width + 0.09f, _height + 0.07f),
                new Color(0.02f, 0.02f, 0.02f, 0.82f), 90, false);

            GameObject back = RuntimeArt.SpriteObject(
                "HpBarBack", transform, new Vector2(0f, y),
                new Vector2(_width, _height),
                new Color(0.24f, 0.055f, 0.045f, 0.95f), 91, false);
            back.transform.localPosition = new Vector3(0f, y, -0.02f);

            GameObject fill = RuntimeArt.SpriteObject(
                "HpBarFill", transform, new Vector2(0f, y),
                new Vector2(_width, _height),
                boss ? new Color(0.74f, 0.24f, 0.92f) : new Color(0.31f, 0.82f, 0.21f),
                92, false);
            fill.transform.localPosition = new Vector3(0f, y, -0.03f);
            _fill = fill.transform;
        }

        public void SetFraction(float fraction)
        {
            if (_fill == null) return;
            fraction = Mathf.Clamp01(fraction);
            _fill.localScale = new Vector3(_width * fraction, _height, 1f);
            _fill.localPosition = new Vector3(-_width * (1f - fraction) * 0.5f, _fill.localPosition.y, _fill.localPosition.z);
        }
    }
}
