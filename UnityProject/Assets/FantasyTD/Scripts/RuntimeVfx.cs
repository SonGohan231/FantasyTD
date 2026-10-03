using UnityEngine;

namespace FantasyTD.MapA
{
    public static class RuntimeVfx
    {
        public static GameObject CreateProjectile(string name, Vector2 position, bool frost)
        {
            Sprite sprite = HiggsfieldSpriteFactory.SeedShot;
            if (sprite == null)
                throw new System.InvalidOperationException("Required authored seed projectile is missing.");
            GameObject projectile = new GameObject(name);
            projectile.transform.position = new Vector3(position.x, position.y, -0.35f);
            SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 50;
            renderer.color = frost ? new Color(0.65f, 0.92f, 1f) : Color.white;
            float scale = 0.26f / Mathf.Max(0.001f, sprite.bounds.size.y);
            projectile.transform.localScale = new Vector3(scale, scale, 1f);
            return projectile;
        }

        public static void Impact(Vector2 position, DamageType type)
        {
            // The victim's real imported sprite reacts in EnemyUnit.TakeDamage.
            // No approved impact, root or essence VFX sheet is available for MAP-A.
            // Do not fill that gap with code-drawn leaves, rings or diamonds.
        }
    }
}
