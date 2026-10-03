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
            root.AddComponent<OrganicBob>();

            float height = kind == PlantKind.OakGuardian ? 1.55f : 1.42f;
            float shadowWidth = kind == PlantKind.OakGuardian ? 1.15f : 0.92f;
            RuntimeArt.ShadowObject(
                "PlantShadow",
                root.transform,
                new Vector2(0f, -0.43f),
                new Vector2(shadowWidth, 0.34f),
                8,
                0.30f);

            AddHiggsfieldVisual(
                root.transform,
                sprite,
                height,
                new Vector2(0f, -0.49f),
                24);

            return root;
        }

        public static GameObject CreateEnemy(EnemyKind kind)
        {
            Sprite sprite = HiggsfieldSpriteFactory.Enemy(kind);
            Require(sprite, Balance.EnemyName(kind));

            GameObject root = new GameObject(Balance.EnemyName(kind));
            root.AddComponent<OrganicBob>();

            float targetHeight;
            float shadowWidth;
            Vector2 offset;

            switch (kind)
            {
                case EnemyKind.Runner:
                    targetHeight = 1.02f;
                    shadowWidth = 0.72f;
                    offset = new Vector2(0f, -0.40f);
                    break;
                case EnemyKind.Brute:
                    targetHeight = 1.55f;
                    shadowWidth = 1.18f;
                    offset = new Vector2(0f, -0.55f);
                    break;
                case EnemyKind.Armored:
                    targetHeight = 1.20f;
                    shadowWidth = 0.86f;
                    offset = new Vector2(0f, -0.45f);
                    break;
                case EnemyKind.Boss:
                    targetHeight = 2.05f;
                    shadowWidth = 1.62f;
                    offset = new Vector2(0f, -0.70f);
                    break;
                default:
                    targetHeight = 1.16f;
                    shadowWidth = 0.82f;
                    offset = new Vector2(0f, -0.44f);
                    break;
            }

            RuntimeArt.ShadowObject(
                kind == EnemyKind.Boss ? "BossShadow" : "EnemyShadow",
                root.transform,
                new Vector2(0f, kind == EnemyKind.Boss ? -0.58f : -0.40f),
                new Vector2(shadowWidth, shadowWidth * 0.31f),
                15,
                kind == EnemyKind.Boss ? 0.42f : 0.30f);

            AddHiggsfieldVisual(root.transform, sprite, targetHeight, offset, 24);
            return root;
        }

        public static GameObject CreatePortal(Vector2 position)
        {
            Sprite sprite = HiggsfieldSpriteFactory.Portal;
            Require(sprite, "Portal");

            GameObject root = new GameObject("PortalCore");
            root.transform.position = new Vector3(position.x, position.y, -0.18f);
            AddHiggsfieldVisual(root.transform, sprite, 1.85f, new Vector2(0f, -0.42f), 18);
            return root;
        }

        public static GameObject CreateHomeTree(Vector2 position)
        {
            Sprite sprite = HiggsfieldSpriteFactory.HomeTree;
            Require(sprite, "Baza-drzewo");

            GameObject root = new GameObject("HomeTree_Higgsfield");
            root.transform.position = new Vector3(position.x, position.y, -0.18f);
            RuntimeArt.ShadowObject(
                "HomeShadow",
                root.transform,
                new Vector2(0f, -0.50f),
                new Vector2(1.55f, 0.46f),
                16,
                0.30f);
            AddHiggsfieldVisual(root.transform, sprite, 2.05f, new Vector2(0f, -0.51f), 20);
            return root;
        }

        private static void AddHiggsfieldVisual(
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
        }

        private static void Require(Sprite sprite, string label)
        {
            if (sprite == null)
                throw new InvalidOperationException(
                    "T0012 requires Higgsfield production art for: " + label);
        }
    }
}
