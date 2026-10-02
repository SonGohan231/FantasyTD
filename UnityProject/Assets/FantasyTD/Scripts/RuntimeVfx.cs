using UnityEngine;

namespace FantasyTD.MapA
{
    public static class RuntimeVfx
    {
        public static void AttachProjectile(GameObject projectile, Color color, bool frost)
        {
            if (projectile == null) return;

            RuntimeArt.SpriteObject(
                frost ? "FrostHalo" : "SeedHalo",
                projectile.transform,
                Vector2.zero,
                frost ? new Vector2(0.46f, 0.46f) : new Vector2(0.38f, 0.38f),
                new Color(color.r, color.g, color.b, frost ? 0.28f : 0.20f),
                48,
                true,
                RuntimeArt.SoftCircle);

            RuntimeArt.SpriteObject(
                frost ? "FrostCore" : "SeedCore",
                projectile.transform,
                Vector2.zero,
                frost ? new Vector2(0.20f, 0.20f) : new Vector2(0.15f, 0.15f),
                frost ? new Color(0.88f, 0.99f, 1f) : new Color(0.95f, 1f, 0.58f),
                51,
                true,
                frost ? RuntimeArt.Diamond : RuntimeArt.Circle,
                frost ? 45f : 0f);

            RuntimeArt.SpriteObject(
                "ProjectileTail",
                projectile.transform,
                new Vector2(-0.14f, 0f),
                new Vector2(0.28f, frost ? 0.11f : 0.08f),
                new Color(color.r, color.g, color.b, 0.42f),
                47,
                true,
                RuntimeArt.Leaf,
                -90f);
        }

        public static void Impact(Vector2 position, DamageType type)
        {
            Color color;
            Sprite sprite;

            switch (type)
            {
                case DamageType.Frost:
                    color = new Color(0.55f, 0.90f, 1f);
                    sprite = RuntimeArt.Diamond;
                    break;
                case DamageType.Root:
                    color = new Color(0.42f, 0.73f, 0.20f);
                    sprite = RuntimeArt.Thorn;
                    break;
                default:
                    color = new Color(0.68f, 0.94f, 0.26f);
                    sprite = RuntimeArt.Leaf;
                    break;
            }

            SpawnRing(position, color, 0.42f, 0.34f);

            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                GameObject particle = RuntimeArt.SpriteObject(
                    "ImpactSpark",
                    null,
                    position + dir * 0.07f,
                    type == DamageType.Frost ? new Vector2(0.11f, 0.18f) : new Vector2(0.10f, 0.13f),
                    color,
                    60,
                    true,
                    sprite,
                    i * 60f);

                TransientVisual fx = particle.AddComponent<TransientVisual>();
                fx.Configure(0.30f, dir * 0.72f, 155f * (i % 2 == 0 ? 1f : -1f), 0.65f);
            }
        }

        public static void RootBurst(Vector2 position)
        {
            SpawnRing(position, new Color(0.43f, 0.76f, 0.20f), 0.74f, 0.52f);

            for (int i = 0; i < 5; i++)
            {
                float angle = -55f + i * 27.5f;
                float r = angle * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Sin(r), Mathf.Abs(Mathf.Cos(r)) * 0.42f - 0.18f);
                GameObject root = RuntimeArt.SpriteObject(
                    "RootVfx",
                    null,
                    position + offset * 0.42f,
                    new Vector2(0.15f, 0.46f),
                    new Color(0.40f, 0.62f, 0.15f, 0.88f),
                    58,
                    true,
                    RuntimeArt.Thorn,
                    -angle);

                TransientVisual fx = root.AddComponent<TransientVisual>();
                fx.Configure(0.48f, Vector2.up * 0.08f, 0f, 0.38f);
            }
        }

        public static void EssencePulse(Vector2 position)
        {
            SpawnRing(position, new Color(1f, 0.82f, 0.18f), 0.92f, 0.62f);

            for (int i = 0; i < 4; i++)
            {
                float a = (45f + i * 90f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                GameObject spark = RuntimeArt.SpriteObject(
                    "EssenceSpark",
                    null,
                    position + dir * 0.28f,
                    new Vector2(0.11f, 0.11f),
                    new Color(1f, 0.91f, 0.35f),
                    59,
                    true,
                    RuntimeArt.Diamond,
                    45f);

                TransientVisual fx = spark.AddComponent<TransientVisual>();
                fx.Configure(0.62f, dir * 0.18f + Vector2.up * 0.16f, 90f, 0.50f);
            }
        }

        private static void SpawnRing(Vector2 position, Color color, float size, float lifetime)
        {
            GameObject ring = RuntimeArt.SpriteObject(
                "ImpactRing",
                null,
                position,
                new Vector2(size, size),
                new Color(color.r, color.g, color.b, 0.72f),
                57,
                true,
                RuntimeArt.Ring);

            TransientVisual fx = ring.AddComponent<TransientVisual>();
            fx.Configure(lifetime, Vector2.zero, 0f, 0.90f);
        }
    }

    public sealed class TransientVisual : MonoBehaviour
    {
        private float _lifetime;
        private float _elapsed;
        private Vector2 _velocity;
        private float _spin;
        private float _scaleGrowth;
        private Vector3 _startScale;
        private SpriteRenderer _renderer;
        private Color _startColor;

        public void Configure(
            float lifetime,
            Vector2 velocity,
            float spinDegreesPerSecond,
            float scaleGrowth)
        {
            _lifetime = Mathf.Max(0.05f, lifetime);
            _velocity = velocity;
            _spin = spinDegreesPerSecond;
            _scaleGrowth = scaleGrowth;
            _startScale = transform.localScale;
            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null)
                _startColor = _renderer.color;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
            {
                Destroy(gameObject);
                return;
            }

            float p = Mathf.Clamp01(_elapsed / _lifetime);
            transform.position += (Vector3)(_velocity * Time.deltaTime);
            transform.Rotate(0f, 0f, _spin * Time.deltaTime);
            transform.localScale = _startScale * (1f + _scaleGrowth * p);

            if (_renderer != null)
            {
                Color c = _startColor;
                c.a *= 1f - p;
                _renderer.color = c;
            }
        }
    }
}
