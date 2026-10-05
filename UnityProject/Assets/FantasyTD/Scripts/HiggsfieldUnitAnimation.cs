using UnityEngine;

namespace FantasyTD.MapA
{
    /// <summary>Plays only existing authored poses; all other reactions transform imported art.</summary>
    public sealed class HiggsfieldUnitAnimation : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Sprite[] _walk;
        private float _height;
        private float _walkTime;
        private float _movementRate;
        private float _actionUntil;
        private float _hitUntil;
        private float _deathAt = -1f;
        private Vector3 _basePosition;
        private Vector3 _baseScale;
        public int WalkFrameIndex { get; private set; } = -1;
        public bool HasAuthoredWalk => _walk != null && _walk.Length > 1;
        public Sprite CurrentSprite => _renderer != null ? _renderer.sprite : null;

        public void Initialize(SpriteRenderer renderer, float height, Sprite[] walk = null)
        {
            _renderer = renderer;
            _walk = walk;
            _height = height;
            _basePosition = renderer.transform.localPosition;
            _baseScale = renderer.transform.localScale;
        }

        public void SetMovement(Vector2 direction, float speedMultiplier)
        {
            _movementRate = Mathf.Max(0f, speedMultiplier);
            if (_renderer != null && Mathf.Abs(direction.x) > 0.01f)
                _renderer.flipX = direction.x < 0f;
        }

        public void PlayAction() => _actionUntil = Time.time + 0.22f;
        public void PlayHit() => _hitUntil = Time.time + 0.12f;
        public void PlayDeath()
        {
            _movementRate = 0f;
            _deathAt = Time.time;
        }

        private void Update()
        {
            if (_renderer == null) return;
            if (_deathAt >= 0f)
            {
                Color fade = _renderer.color;
                fade.a = Mathf.Clamp01(1f - (Time.time - _deathAt) / 0.24f);
                _renderer.color = fade;
                return;
            }

            // Rooted/finished enemies stop their authored cycle immediately. No new idle
            // frame is invented: retain the last grounded walk pose until movement resumes.
            if (HasAuthoredWalk && _movementRate > 0f)
            {
                _walkTime += Time.deltaTime * _movementRate;
                int index = Mathf.FloorToInt(_walkTime * 5f) % _walk.Length;
                if (index != WalkFrameIndex)
                {
                    WalkFrameIndex = index;
                    _renderer.sprite = _walk[index];
                    float scale = _height / Mathf.Max(0.001f, _walk[index].bounds.size.y);
                    _baseScale = new Vector3(scale, scale, 1f);
                }
            }

            float action = Mathf.Clamp01((_actionUntil - Time.time) / 0.22f);
            float recoil = Mathf.Sin(action * Mathf.PI);
            _renderer.transform.localPosition = _basePosition + Vector3.up * (recoil * 0.06f);
            _renderer.transform.localScale = Vector3.Scale(
                _baseScale, new Vector3(1f + recoil * 0.045f, 1f - recoil * 0.035f, 1f));
            _renderer.color = Time.time < _hitUntil ? new Color(1f, 0.62f, 0.62f, 1f) : Color.white;
        }
    }
}
