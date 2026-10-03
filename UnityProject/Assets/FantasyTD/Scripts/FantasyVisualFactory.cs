using UnityEngine;

namespace FantasyTD.MapA
{
    public static class FantasyVisualFactory
    {
        private static readonly Color Shadow = new Color(0.03f, 0.035f, 0.025f, 0.30f);

        public static GameObject CreatePlant(PlantKind kind, Vector2 position)
        {
            GameObject root = new GameObject(Balance.DisplayName(kind));
            root.transform.position = new Vector3(position.x, position.y, -0.25f);

            Sprite sprite = HiggsfieldArt.Plant(kind);
            if (sprite == null)
            {
                Debug.LogError(
                    "[T0012][Higgsfield] Missing required plant asset: " +
                    Balance.DisplayName(kind) +
                    ". Procedural character fallback is intentionally disabled.");
                return root;
            }

            AddGroundShadow(root.transform, kind == PlantKind.OakGuardian ? 1.05f : 0.86f, 8);

            GameObject visual = new GameObject("HiggsfieldVisual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 14;

            float targetHeight = kind == PlantKind.OakGuardian ? 1.22f : 1.05f;
            ScaleSpriteToHeight(visual.transform, sprite, targetHeight);

            visual.transform.localPosition = new Vector3(0f, kind == PlantKind.OakGuardian ? 0.11f : 0.08f, 0f);
            visual.AddComponent<OrganicBob>();
            return root;
        }

        public static GameObject CreateEnemy(EnemyKind kind)
        {
            GameObject root = new GameObject(Balance.EnemyName(kind));

            Sprite sprite = HiggsfieldArt.Enemy(kind);
            if (sprite == null)
            {
                Debug.LogError(
                    "[T0012][Higgsfield] Missing required enemy asset: " +
                    Balance.EnemyName(kind) +
                    ". Procedural character fallback is intentionally disabled.");
                return root;
            }

            bool boss = kind == EnemyKind.Boss;
            float shadowWidth;
            float targetHeight;

            switch (kind)
            {
                case EnemyKind.Runner:
                    shadowWidth = 0.62f;
                    targetHeight = 0.86f;
                    break;
                case EnemyKind.Brute:
                    shadowWidth = 1.10f;
                    targetHeight = 1.35f;
                    break;
                case EnemyKind.Armored:
                    shadowWidth = 0.82f;
                    targetHeight = 1.00f;
                    break;
                case EnemyKind.Boss:
                    shadowWidth = 1.65f;
                    targetHeight = 2.00f;
                    break;
                default:
                    shadowWidth = 0.74f;
                    targetHeight = 0.92f;
                    break;
            }

            AddGroundShadow(root.transform, shadowWidth, 15);

            GameObject visual = new GameObject("HiggsfieldVisual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = boss ? 31 : 22;

            ScaleSpriteToHeight(visual.transform, sprite, targetHeight);
            visual.transform.localPosition = new Vector3(0f, boss ? 0.24f : 0.10f, 0f);
            visual.AddComponent<OrganicBob>();

            if (boss)
            {
                RuntimeArt.SpriteObject(
                    "BossAura",
                    root.transform,
                    new Vector2(0f, 0.20f),
                    new Vector2(1.95f, 1.95f),
                    new Color(0.65f, 0.20f, 0.82f, 0.12f),
                    16,
                    true,
                    RuntimeArt.SoftCircle);
            }

            return root;
        }

        public static GameObject CreateEnvironmentSprite(
            string name,
            Sprite sprite,
            Vector2 worldPosition,
            Vector2 worldSize,
            int sortingOrder)
        {
            if (sprite == null)
            {
                Debug.LogError("[T0012][Higgsfield] Missing required environment asset: " + name);
                return null;
            }

            GameObject go = new GameObject(name);
            go.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = sortingOrder;

            Vector2 native = sprite.bounds.size;
            if (native.x > 0.0001f && native.y > 0.0001f)
            {
                go.transform.localScale = new Vector3(
                    worldSize.x / native.x,
                    worldSize.y / native.y,
                    1f);
            }

            return go;
        }

        private static void AddGroundShadow(Transform parent, float width, int sortingOrder)
        {
            RuntimeArt.SpriteObject(
                "GroundShadow",
                parent,
                new Vector2(0f, -0.36f),
                new Vector2(width, width * 0.30f),
                Shadow,
                sortingOrder,
                true,
                RuntimeArt.SoftCircle);
        }

        private static void ScaleSpriteToHeight(Transform transform, Sprite sprite, float targetHeight)
        {
            float h = sprite.bounds.size.y;
            if (h <= 0.0001f) return;
            float scale = targetHeight / h;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
