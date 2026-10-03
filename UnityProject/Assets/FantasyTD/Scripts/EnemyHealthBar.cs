using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        private EnemyUnit _enemy;
        private Transform _fill;
        private SpriteRenderer _status;
        private float _width;

        public void Initialize(EnemyUnit enemy, bool boss)
        {
            _enemy = enemy;
            _width = boss ? 1.55f : 0.86f;
            float y = boss ? 1.05f : 0.64f;
            float h = boss ? 0.13f : 0.09f;

            GameObject back = RuntimeArt.SpriteObject(
                "HpBarBack", transform, new Vector2(0f, y),
                new Vector2(_width + 0.08f, h + 0.05f),
                new Color(0.06f, 0.035f, 0.025f, 0.92f), 95, false);

            GameObject fill = RuntimeArt.SpriteObject(
                "HpBarFill", transform, new Vector2(0f, y),
                new Vector2(_width, h),
                boss ? new Color(0.82f, 0.18f, 0.24f) : new Color(0.34f, 0.84f, 0.24f),
                96, false);
            _fill = fill.transform;

            GameObject status = RuntimeArt.SpriteObject(
                "StatusCue", transform, new Vector2(0f, y + (boss ? 0.21f : 0.15f)),
                new Vector2(boss ? 0.26f : 0.18f, boss ? 0.26f : 0.18f),
                new Color(1f, 1f, 1f, 0f), 97, true, RuntimeArt.Ring);
            _status = status.GetComponent<SpriteRenderer>();

            Refresh();
        }

        public void Refresh()
        {
            if (_enemy == null || _fill == null) return;

            float ratio = _enemy.MaxHp <= 0f ? 0f : Mathf.Clamp01(_enemy.CurrentHp / _enemy.MaxHp);
            Vector3 s = _fill.localScale;
            s.x = _width * ratio;
            _fill.localScale = s;

            Vector3 p = _fill.localPosition;
            p.x = -(_width * (1f - ratio)) * 0.5f;
            _fill.localPosition = p;

            SpriteRenderer fillRenderer = _fill.GetComponent<SpriteRenderer>();
            if (fillRenderer != null && !_enemy.IsBoss)
            {
                fillRenderer.color = ratio > 0.55f
                    ? new Color(0.34f, 0.84f, 0.24f)
                    : (ratio > 0.25f
                        ? new Color(0.95f, 0.67f, 0.16f)
                        : new Color(0.90f, 0.20f, 0.16f));
            }
        }

        private void LateUpdate()
        {
            if (_enemy == null || _status == null) return;

            if (_enemy.IsRooted)
                _status.color = new Color(0.55f, 0.82f, 0.20f, 0.95f);
            else if (_enemy.IsSlowed)
                _status.color = new Color(0.44f, 0.88f, 1f, 0.95f);
            else
                _status.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}
