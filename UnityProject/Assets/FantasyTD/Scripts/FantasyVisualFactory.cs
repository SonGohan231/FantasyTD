using System;
using UnityEngine;

namespace FantasyTD.MapA
{
    public static class FantasyVisualFactory
    {
        public static GameObject CreatePlant(PlantKind kind, Vector2 position)
        {
            Sprite sprite = HiggsfieldSpriteFactory.Plant(kind);
            Require(sprite, Balance.DisplayName(kind));

            GameObject root = new GameObject(Balance.DisplayName(kind));
            root.transform.position = new Vector3(position.x, position.y, -0.25f);

            float height = kind == PlantKind.OakGuardian ? 1.55f : 1.42f;

            SpriteRenderer renderer = AddHiggsfieldVisual(
                root.transform, sprite, height, new Vector2(0f, -0.49f), 24);
            root.AddComponent<HiggsfieldUnitAnimation>().Initialize(renderer, height);

            return root;
        }

        public static GameObject CreateEnemy(EnemyKind kind)
        {
            Sprite sprite = HiggsfieldSpriteFactory.Enemy(kind);
            Require(sprite, Balance.EnemyName(kind));

            GameObject root = new GameObject(Balance.EnemyName(kind));

            float targetHeight;
            Vector2 offset;

            switch (kind)
            {
                case EnemyKind.Runner:
                    targetHeight = 1.02f;
                    offset = new Vector2(0f, -0.40f);
                    break;
                case EnemyKind.Brute:
                    targetHeight = 1.55f;
                    offset = new Vector2(0f, -0.55f);
                    break;
                case EnemyKind.Armored:
                    targetHeight = 1.20f;
                    offset = new Vector2(0f, -0.45f);
                    break;
                case EnemyKind.Boss:
                    targetHeight = 2.05f;
                    offset = new Vector2(0f, -0.70f);
                    break;
                default:
                    targetHeight = 1.16f;
                    offset = new Vector2(0f, -0.44f);
                    break;
            }


            SpriteRenderer renderer = AddHiggsfieldVisual(root.transform, sprite, targetHeight, offset, 24);
            root.AddComponent<HiggsfieldUnitAnimation>().Initialize(
                renderer, targetHeight, kind == EnemyKind.Wanderer ? HiggsfieldSpriteFactory.WandererWalk : null);
            return root;
        }

        public static GameObject CreatePortal(Vector2 position)
        {
            Sprite sprite = HiggsfieldSpriteFactory.Portal;
            Require(sprite, "Portal");

            GameObject root = new GameObject("PortalCore");
            root.transform.position = new Vector3(position.x, position.y, -0.18f);
            // Spawn is exactly on the top route boundary; keep the portal art inside the camera.
            AddHiggsfieldVisual(root.transform, sprite, 1.85f, new Vector2(0.20f, -2.48f), 18);
            return root;
        }

        public static GameObject CreateHomeTree(Vector2 position)
        {
            Sprite sprite = HiggsfieldSpriteFactory.HomeTree;
            Require(sprite, "Baza-drzewo");

            GameObject root = new GameObject("HomeTree_Higgsfield");
            root.transform.position = new Vector3(position.x, position.y, -0.18f);
            // The route ends at the lower-right boundary; pull presentation art inward
            // without changing authoritative enemy movement geometry.
            AddHiggsfieldVisual(root.transform, sprite, 2.05f, new Vector2(-1.78f, 1.36f), 20);
            return root;
        }

        private static SpriteRenderer AddHiggsfieldVisual(
            Transform root,
            Sprite sprite,
            float targetHeight,
            Vector2 localPosition,
            int sortingOrder)
        {
            GameObject visual = new GameObject("HiggsfieldVisual");
            visual.transform.SetParent(root, false);
            visual.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = Color.white;

            float height = Mathf.Max(0.001f, sprite.bounds.size.y);
            float scale = targetHeight / height;
            visual.transform.localScale = new Vector3(scale, scale, 1f);
            return renderer;
        }

        private static void Require(Sprite sprite, string label)
        {
            if (sprite == null)
                throw new InvalidOperationException(
                    "T0012 requires Higgsfield production art for: " + label);
        }
    }
}
