using UnityEngine;

namespace FantasyTD.MapA
{
    public static class FantasyVisualFactory
    {
        private static readonly Color Ink = new Color(0.08f, 0.075f, 0.045f, 0.94f);
        private static readonly Color WarmHighlight = new Color(1f, 0.94f, 0.68f, 0.72f);

        public static GameObject CreatePlant(PlantKind kind, Vector2 position)
        {
            GameObject root = new GameObject(Balance.DisplayName(kind));
            root.transform.position = new Vector3(position.x, position.y, -0.25f);
            root.AddComponent<OrganicBob>();

            RuntimeArt.ShadowObject(
                "PlantShadow", root.transform, new Vector2(0f, -0.42f),
                new Vector2(1.18f, 0.48f), 8, 0.34f);

            switch (kind)
            {
                case PlantKind.SeedShooter:
                    SeedShooter(root.transform);
                    break;
                case PlantKind.EssenceFlower:
                    EssenceFlower(root.transform);
                    break;
                case PlantKind.OakGuardian:
                    OakGuardian(root.transform);
                    break;
                case PlantKind.FrostBud:
                    FrostBud(root.transform);
                    break;
            }

            return root;
        }

        public static GameObject CreateEnemy(EnemyKind kind)
        {
            GameObject root = new GameObject(Balance.EnemyName(kind));
            root.AddComponent<OrganicBob>();

            switch (kind)
            {
                case EnemyKind.Wanderer:
                    Wanderer(root.transform);
                    break;
                case EnemyKind.Runner:
                    Runner(root.transform);
                    break;
                case EnemyKind.Brute:
                    Brute(root.transform);
                    break;
                case EnemyKind.Armored:
                    Armored(root.transform);
                    break;
                case EnemyKind.Boss:
                    ThornKing(root.transform);
                    break;
            }

            return root;
        }

        private static void SeedShooter(Transform root)
        {
            Sprite higgsfield = HiggsfieldSpriteFactory.SeedShooter;
            if (higgsfield != null)
            {
                AddHiggsfieldVisual(root, higgsfield, 1.65f, new Vector2(0f, -0.46f), 18);
                return;
            }

            Color dark = new Color(0.10f, 0.32f, 0.09f);
            Color green = new Color(0.30f, 0.72f, 0.18f);
            Color lime = new Color(0.57f, 0.88f, 0.24f);

            RuntimeArt.SpriteObject("StemOutline", root, new Vector2(0f, -0.17f),
                new Vector2(0.22f, 0.68f), Ink, 10, false);
            RuntimeArt.SpriteObject("Stem", root, new Vector2(0f, -0.16f),
                new Vector2(0.13f, 0.62f), dark, 11, false);

            RuntimeArt.SpriteObject("LeafLShadow", root, new Vector2(-0.30f, -0.24f),
                new Vector2(0.58f, 0.34f), Ink, 11, true, RuntimeArt.Leaf, -63f);
            RuntimeArt.SpriteObject("LeafL", root, new Vector2(-0.28f, -0.22f),
                new Vector2(0.50f, 0.28f), new Color(0.23f, 0.60f, 0.15f), 12, true, RuntimeArt.Leaf, -63f);
            RuntimeArt.SpriteObject("LeafRShadow", root, new Vector2(0.26f, -0.34f),
                new Vector2(0.53f, 0.30f), Ink, 11, true, RuntimeArt.Leaf, 64f);
            RuntimeArt.SpriteObject("LeafR", root, new Vector2(0.25f, -0.32f),
                new Vector2(0.46f, 0.24f), new Color(0.18f, 0.52f, 0.13f), 12, true, RuntimeArt.Leaf, 64f);

            RuntimeArt.SpriteObject("HeadOutline", root, new Vector2(-0.02f, 0.22f),
                new Vector2(0.78f, 0.68f), Ink, 13);
            RuntimeArt.SpriteObject("Head", root, new Vector2(-0.02f, 0.23f),
                new Vector2(0.68f, 0.59f), green, 14);
            RuntimeArt.SpriteObject("HeadLight", root, new Vector2(-0.15f, 0.36f),
                new Vector2(0.28f, 0.18f), new Color(0.77f, 1f, 0.46f, 0.52f), 15, true, RuntimeArt.SoftCircle);

            RuntimeArt.SpriteObject("MuzzleOutline", root, new Vector2(0.36f, 0.22f),
                new Vector2(0.40f, 0.36f), Ink, 15);
            RuntimeArt.SpriteObject("Muzzle", root, new Vector2(0.38f, 0.22f),
                new Vector2(0.33f, 0.29f), dark, 16);
            RuntimeArt.SpriteObject("MuzzleGlow", root, new Vector2(0.45f, 0.22f),
                new Vector2(0.15f, 0.13f), lime, 17, true, RuntimeArt.SoftCircle);

            EyePair(root, new Vector2(-0.16f, 0.28f), 17, 0.095f);
        }

        private static void EssenceFlower(Transform root)
        {
            Color stem = new Color(0.18f, 0.48f, 0.12f);
            Color gold = new Color(1f, 0.74f, 0.10f);
            Color pale = new Color(1f, 0.94f, 0.42f);
            Color core = new Color(0.62f, 0.27f, 0.06f);

            RuntimeArt.SpriteObject("StemOutline", root, new Vector2(0f, -0.24f),
                new Vector2(0.22f, 0.70f), Ink, 10, false);
            RuntimeArt.SpriteObject("Stem", root, new Vector2(0f, -0.23f),
                new Vector2(0.13f, 0.64f), stem, 11, false);
            RuntimeArt.SpriteObject("Leaf", root, new Vector2(-0.24f, -0.30f),
                new Vector2(0.46f, 0.25f), new Color(0.25f, 0.62f, 0.16f), 12, true, RuntimeArt.Leaf, -58f);

            RuntimeArt.SpriteObject("Halo", root, new Vector2(0f, 0.20f),
                new Vector2(1.06f, 1.06f), new Color(1f, 0.75f, 0.12f, 0.18f), 11, true, RuntimeArt.SoftCircle);

            for (int i = 0; i < 9; i++)
            {
                float angle = i * 40f;
                float radians = angle * Mathf.Deg2Rad;
                Vector2 p = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * 0.30f + new Vector2(0f, 0.20f);

                RuntimeArt.SpriteObject("PetalShadow_" + i, root, p,
                    new Vector2(0.30f, 0.46f), Ink, 12, true, RuntimeArt.Petal, angle - 90f);
                RuntimeArt.SpriteObject("Petal_" + i, root, p,
                    new Vector2(0.25f, 0.40f), i % 2 == 0 ? pale : gold, 13, true, RuntimeArt.Petal, angle - 90f);
            }

            RuntimeArt.SpriteObject("CoreOutline", root, new Vector2(0f, 0.20f),
                new Vector2(0.50f, 0.50f), Ink, 14);
            RuntimeArt.SpriteObject("Core", root, new Vector2(0f, 0.20f),
                new Vector2(0.42f, 0.42f), core, 15);
            RuntimeArt.SpriteObject("CoreLight", root, new Vector2(-0.08f, 0.29f),
                new Vector2(0.18f, 0.14f), WarmHighlight, 16, true, RuntimeArt.SoftCircle);

            Spark(root, new Vector2(-0.48f, 0.52f), 0.12f, 16, gold);
            Spark(root, new Vector2(0.46f, 0.58f), 0.09f, 16, pale);
            Spark(root, new Vector2(0.38f, -0.05f), 0.07f, 16, pale);

            EyePair(root, new Vector2(0f, 0.21f), 17, 0.075f);
        }

        private static void OakGuardian(Transform root)
        {
            Color barkDark = new Color(0.30f, 0.16f, 0.06f);
            Color bark = new Color(0.50f, 0.30f, 0.12f);
            Color moss = new Color(0.18f, 0.48f, 0.12f);
            Color leaf = new Color(0.12f, 0.39f, 0.10f);

            RuntimeArt.SpriteObject("RootL", root, new Vector2(-0.30f, -0.46f),
                new Vector2(0.58f, 0.18f), barkDark, 10, true, RuntimeArt.Leaf, -74f);
            RuntimeArt.SpriteObject("RootR", root, new Vector2(0.30f, -0.46f),
                new Vector2(0.58f, 0.18f), barkDark, 10, true, RuntimeArt.Leaf, 74f);

            RuntimeArt.SpriteObject("TrunkOutline", root, new Vector2(0f, -0.05f),
                new Vector2(0.78f, 0.96f), Ink, 11);
            RuntimeArt.SpriteObject("Trunk", root, new Vector2(0f, -0.04f),
                new Vector2(0.68f, 0.88f), bark, 12);
            RuntimeArt.SpriteObject("BarkStripe", root, new Vector2(-0.12f, -0.02f),
                new Vector2(0.12f, 0.62f), new Color(0.68f, 0.42f, 0.18f, 0.55f), 13, false);

            RuntimeArt.SpriteObject("BranchL", root, new Vector2(-0.43f, 0.05f),
                new Vector2(0.50f, 0.17f), barkDark, 11, true, RuntimeArt.Leaf, -72f);
            RuntimeArt.SpriteObject("BranchR", root, new Vector2(0.43f, 0.07f),
                new Vector2(0.50f, 0.17f), barkDark, 11, true, RuntimeArt.Leaf, 72f);

            RuntimeArt.SpriteObject("CrownBack", root, new Vector2(0f, 0.46f),
                new Vector2(1.15f, 0.78f), Ink, 13);
            RuntimeArt.SpriteObject("CrownL", root, new Vector2(-0.34f, 0.42f),
                new Vector2(0.62f, 0.58f), leaf, 14);
            RuntimeArt.SpriteObject("CrownR", root, new Vector2(0.34f, 0.43f),
                new Vector2(0.66f, 0.60f), new Color(0.15f, 0.46f, 0.11f), 14);
            RuntimeArt.SpriteObject("CrownTop", root, new Vector2(0f, 0.67f),
                new Vector2(0.72f, 0.55f), moss, 15);
            RuntimeArt.SpriteObject("MossLight", root, new Vector2(-0.18f, 0.72f),
                new Vector2(0.30f, 0.18f), new Color(0.48f, 0.72f, 0.24f, 0.56f), 16, true, RuntimeArt.SoftCircle);

            EyePair(root, new Vector2(0f, 0.02f), 16, 0.085f);
            RuntimeArt.SpriteObject("Mouth", root, new Vector2(0.03f, -0.17f),
                new Vector2(0.24f, 0.06f), new Color(0.11f, 0.07f, 0.035f), 16, false);
        }

        private static void FrostBud(Transform root)
        {
            Color stem = new Color(0.10f, 0.40f, 0.32f);
            Color ice = new Color(0.48f, 0.88f, 1f);
            Color iceDark = new Color(0.10f, 0.48f, 0.78f);
            Color whiteBlue = new Color(0.84f, 0.98f, 1f);

            RuntimeArt.SpriteObject("ColdHalo", root, new Vector2(0f, 0.20f),
                new Vector2(1.12f, 1.12f), new Color(0.31f, 0.80f, 1f, 0.20f), 9, true, RuntimeArt.SoftCircle);
            RuntimeArt.SpriteObject("StemOutline", root, new Vector2(0f, -0.21f),
                new Vector2(0.21f, 0.68f), Ink, 10, false);
            RuntimeArt.SpriteObject("Stem", root, new Vector2(0f, -0.20f),
                new Vector2(0.13f, 0.62f), stem, 11, false);

            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                float r = angle * Mathf.Deg2Rad;
                Vector2 p = new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * 0.27f + new Vector2(0f, 0.21f);
                RuntimeArt.SpriteObject("IcePetalOutline_" + i, root, p,
                    new Vector2(0.28f, 0.48f), Ink, 12, true, RuntimeArt.Diamond, angle);
                RuntimeArt.SpriteObject("IcePetal_" + i, root, p,
                    new Vector2(0.22f, 0.42f), i % 2 == 0 ? ice : whiteBlue, 13, true, RuntimeArt.Diamond, angle);
            }

            RuntimeArt.SpriteObject("BudOutline", root, new Vector2(0f, 0.21f),
                new Vector2(0.50f, 0.50f), Ink, 14);
            RuntimeArt.SpriteObject("Bud", root, new Vector2(0f, 0.21f),
                new Vector2(0.42f, 0.42f), iceDark, 15);
            RuntimeArt.SpriteObject("BudLight", root, new Vector2(-0.09f, 0.31f),
                new Vector2(0.19f, 0.15f), new Color(1f, 1f, 1f, 0.66f), 16, true, RuntimeArt.SoftCircle);

            Spark(root, new Vector2(-0.43f, 0.52f), 0.08f, 16, whiteBlue);
            Spark(root, new Vector2(0.44f, 0.48f), 0.08f, 16, ice);
            EyePair(root, new Vector2(0f, 0.22f), 17, 0.072f);
        }

        private static void Wanderer(Transform root)
        {
            Sprite higgsfield = HiggsfieldSpriteFactory.Wanderer;
            if (higgsfield != null)
            {
                EnemyShadow(root, 0.84f);
                AddHiggsfieldVisual(root, higgsfield, 1.18f, new Vector2(0f, -0.48f), 24);
                return;
            }

            float s = 0.66f;
            Color body = new Color(0.36f, 0.64f, 0.16f);
            EnemyShadow(root, 0.72f);

            RuntimeArt.SpriteObject("EarL", root, new Vector2(-0.27f, 0.18f),
                new Vector2(0.22f, 0.40f), Ink, 19, true, RuntimeArt.Thorn, -35f);
            RuntimeArt.SpriteObject("EarR", root, new Vector2(0.27f, 0.18f),
                new Vector2(0.22f, 0.40f), Ink, 19, true, RuntimeArt.Thorn, 35f);
            RuntimeArt.SpriteObject("BodyOutline", root, Vector2.zero,
                new Vector2(s * 1.12f, s), Ink, 20);
            RuntimeArt.SpriteObject("Body", root, new Vector2(0f, 0.01f),
                new Vector2(s, s * 0.88f), body, 21);

            RuntimeArt.SpriteObject("Belly", root, new Vector2(0f, -0.13f),
                new Vector2(0.42f, 0.28f), new Color(0.56f, 0.75f, 0.28f, 0.62f), 22);
            RuntimeArt.SpriteObject("LegL", root, new Vector2(-0.17f, -0.40f),
                new Vector2(0.14f, 0.34f), body * 0.72f, 18, false);
            RuntimeArt.SpriteObject("LegR", root, new Vector2(0.17f, -0.40f),
                new Vector2(0.14f, 0.34f), body * 0.72f, 18, false);
            EyePair(root, new Vector2(0f, 0.12f), 23, 0.082f);
            RuntimeArt.SpriteObject("Grin", root, new Vector2(0f, -0.10f),
                new Vector2(0.24f, 0.055f), new Color(0.10f, 0.07f, 0.035f), 23, false);
        }

        private static void Runner(Transform root)
        {
            Color body = new Color(0.62f, 0.28f, 0.72f);
            Color wind = new Color(0.76f, 0.91f, 1f, 0.78f);
            EnemyShadow(root, 0.62f);

            RuntimeArt.SpriteObject("WindTailA", root, new Vector2(-0.38f, 0.03f),
                new Vector2(0.50f, 0.09f), wind, 17, true, RuntimeArt.Leaf, -82f);
            RuntimeArt.SpriteObject("WindTailB", root, new Vector2(-0.32f, -0.18f),
                new Vector2(0.40f, 0.07f), new Color(0.74f, 0.88f, 1f, 0.50f), 17, true, RuntimeArt.Leaf, -82f);

            RuntimeArt.SpriteObject("BodyOutline", root, new Vector2(0f, 0.07f),
                new Vector2(0.62f, 0.60f), Ink, 20);
            RuntimeArt.SpriteObject("Body", root, new Vector2(0f, 0.08f),
                new Vector2(0.54f, 0.52f), body, 21);
            RuntimeArt.SpriteObject("EarL", root, new Vector2(-0.22f, 0.26f),
                new Vector2(0.18f, 0.38f), body * 0.76f, 19, true, RuntimeArt.Thorn, -28f);
            RuntimeArt.SpriteObject("EarR", root, new Vector2(0.22f, 0.26f),
                new Vector2(0.18f, 0.38f), body * 0.76f, 19, true, RuntimeArt.Thorn, 28f);

            RuntimeArt.SpriteObject("LegL", root, new Vector2(-0.13f, -0.35f),
                new Vector2(0.11f, 0.52f), new Color(0.38f, 0.17f, 0.46f), 18, false);
            RuntimeArt.SpriteObject("LegR", root, new Vector2(0.13f, -0.35f),
                new Vector2(0.11f, 0.52f), new Color(0.38f, 0.17f, 0.46f), 18, false);
            RuntimeArt.SpriteObject("ForeheadMark", root, new Vector2(0f, 0.25f),
                new Vector2(0.14f, 0.18f), wind, 23, true, RuntimeArt.Diamond);
            EyePair(root, new Vector2(0f, 0.10f), 24, 0.072f);
        }

        private static void Brute(Transform root)
        {
            Color bark = new Color(0.40f, 0.28f, 0.12f);
            Color moss = new Color(0.28f, 0.48f, 0.15f);
            EnemyShadow(root, 1.12f);

            RuntimeArt.SpriteObject("ShoulderL", root, new Vector2(-0.44f, 0.10f),
                new Vector2(0.58f, 0.68f), Ink, 19);
            RuntimeArt.SpriteObject("ShoulderR", root, new Vector2(0.44f, 0.10f),
                new Vector2(0.58f, 0.68f), Ink, 19);
            RuntimeArt.SpriteObject("BodyOutline", root, Vector2.zero,
                new Vector2(1.08f, 1.02f), Ink, 20);
            RuntimeArt.SpriteObject("Body", root, new Vector2(0f, 0.01f),
                new Vector2(0.96f, 0.92f), bark, 21);

            RuntimeArt.SpriteObject("MossL", root, new Vector2(-0.34f, 0.27f),
                new Vector2(0.50f, 0.28f), moss, 22, true, RuntimeArt.Leaf, -55f);
            RuntimeArt.SpriteObject("MossR", root, new Vector2(0.31f, 0.34f),
                new Vector2(0.48f, 0.27f), new Color(0.36f, 0.55f, 0.18f), 22, true, RuntimeArt.Leaf, 55f);
            RuntimeArt.SpriteObject("BarkPlate", root, new Vector2(0f, -0.12f),
                new Vector2(0.54f, 0.42f), new Color(0.58f, 0.37f, 0.16f), 22);

            RuntimeArt.SpriteObject("HornL", root, new Vector2(-0.34f, 0.50f),
                new Vector2(0.22f, 0.46f), new Color(0.70f, 0.61f, 0.40f), 23, true, RuntimeArt.Thorn, -25f);
            RuntimeArt.SpriteObject("HornR", root, new Vector2(0.34f, 0.50f),
                new Vector2(0.22f, 0.46f), new Color(0.70f, 0.61f, 0.40f), 23, true, RuntimeArt.Thorn, 25f);
            EyePair(root, new Vector2(0f, 0.16f), 24, 0.10f);
        }

        private static void Armored(Transform root)
        {
            Color body = new Color(0.29f, 0.57f, 0.20f);
            Color steel = new Color(0.40f, 0.45f, 0.43f);
            Color steelLight = new Color(0.68f, 0.72f, 0.67f);
            EnemyShadow(root, 0.78f);

            RuntimeArt.SpriteObject("BodyOutline", root, new Vector2(-0.04f, -0.03f),
                new Vector2(0.75f, 0.72f), Ink, 20);
            RuntimeArt.SpriteObject("Body", root, new Vector2(-0.04f, -0.02f),
                new Vector2(0.66f, 0.64f), body, 21);

            RuntimeArt.SpriteObject("HelmetOutline", root, new Vector2(-0.03f, 0.27f),
                new Vector2(0.78f, 0.42f), Ink, 23);
            RuntimeArt.SpriteObject("Helmet", root, new Vector2(-0.03f, 0.28f),
                new Vector2(0.70f, 0.34f), steel, 24);
            RuntimeArt.SpriteObject("HelmetHighlight", root, new Vector2(-0.14f, 0.36f),
                new Vector2(0.30f, 0.10f), steelLight, 25, true, RuntimeArt.SoftCircle);
            RuntimeArt.SpriteObject("HelmetSpike", root, new Vector2(-0.03f, 0.54f),
                new Vector2(0.20f, 0.37f), steelLight, 24, true, RuntimeArt.Thorn);

            RuntimeArt.SpriteObject("ShieldOutline", root, new Vector2(0.36f, -0.04f),
                new Vector2(0.48f, 0.58f), Ink, 25, true, RuntimeArt.Diamond);
            RuntimeArt.SpriteObject("Shield", root, new Vector2(0.36f, -0.04f),
                new Vector2(0.40f, 0.50f), new Color(0.50f, 0.53f, 0.48f), 26, true, RuntimeArt.Diamond);
            RuntimeArt.SpriteObject("ShieldBoss", root, new Vector2(0.36f, -0.04f),
                new Vector2(0.12f, 0.12f), new Color(0.82f, 0.68f, 0.24f), 27);
            EyePair(root, new Vector2(-0.09f, 0.09f), 26, 0.068f);
        }

        private static void ThornKing(Transform root)
        {
            Color bark = new Color(0.22f, 0.36f, 0.09f);
            Color barkLight = new Color(0.40f, 0.54f, 0.14f);
            Color thorn = new Color(0.58f, 0.43f, 0.13f);
            Color crown = new Color(0.90f, 0.65f, 0.12f);
            Color magic = new Color(0.78f, 0.30f, 0.92f);

            RuntimeArt.ShadowObject("BossShadow", root, new Vector2(0f, -0.61f),
                new Vector2(1.72f, 0.68f), 15, 0.44f);
            RuntimeArt.SpriteObject("BossAura", root, new Vector2(0f, 0.12f),
                new Vector2(1.90f, 1.90f), new Color(0.62f, 0.17f, 0.82f, 0.18f), 16, true, RuntimeArt.SoftCircle);

            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f;
                float r = angle * Mathf.Deg2Rad;
                Vector2 p = new Vector2(Mathf.Cos(r) * 0.72f, Mathf.Sin(r) * 0.58f + 0.05f);
                RuntimeArt.SpriteObject("Thorn_" + i, root, p,
                    new Vector2(0.27f, 0.55f), thorn, 19, true, RuntimeArt.Thorn, angle - 90f);
            }

            RuntimeArt.SpriteObject("BossBodyOutline", root, new Vector2(0f, -0.02f),
                new Vector2(1.42f, 1.34f), Ink, 20);
            RuntimeArt.SpriteObject("BossBody", root, new Vector2(0f, -0.01f),
                new Vector2(1.30f, 1.22f), bark, 21);
            RuntimeArt.SpriteObject("BarkArmorL", root, new Vector2(-0.38f, 0.06f),
                new Vector2(0.48f, 0.72f), barkLight, 22, true, RuntimeArt.Leaf, -20f);
            RuntimeArt.SpriteObject("BarkArmorR", root, new Vector2(0.38f, 0.06f),
                new Vector2(0.48f, 0.72f), barkLight, 22, true, RuntimeArt.Leaf, 20f);

            RuntimeArt.SpriteObject("HeartGlow", root, new Vector2(0f, -0.18f),
                new Vector2(0.62f, 0.62f), new Color(0.76f, 0.22f, 0.92f, 0.40f), 23, true, RuntimeArt.SoftCircle);
            RuntimeArt.SpriteObject("HeartCore", root, new Vector2(0f, -0.18f),
                new Vector2(0.24f, 0.30f), magic, 24, true, RuntimeArt.Diamond);

            RuntimeArt.SpriteObject("CrownBandOutline", root, new Vector2(0f, 0.67f),
                new Vector2(0.92f, 0.24f), Ink, 27, false);
            RuntimeArt.SpriteObject("CrownBand", root, new Vector2(0f, 0.68f),
                new Vector2(0.82f, 0.16f), crown, 28, false);

            for (int i = -2; i <= 2; i++)
            {
                float h = i == 0 ? 0.62f : (Mathf.Abs(i) == 1 ? 0.48f : 0.40f);
                RuntimeArt.SpriteObject("CrownTip_" + i, root,
                    new Vector2(i * 0.19f, 0.83f + h * 0.20f),
                    new Vector2(0.18f, h), i == 0 ? new Color(1f, 0.78f, 0.18f) : crown,
                    29, true, RuntimeArt.Thorn);
            }

            RuntimeArt.SpriteObject("CrownGem", root, new Vector2(0f, 0.72f),
                new Vector2(0.16f, 0.19f), magic, 30, true, RuntimeArt.Diamond);
            EyePair(root, new Vector2(0f, 0.20f), 31, 0.13f);

            RuntimeArt.SpriteObject("EyeGlowL", root, new Vector2(-0.18f, 0.20f),
                new Vector2(0.26f, 0.20f), new Color(0.92f, 0.42f, 1f, 0.34f), 30, true, RuntimeArt.SoftCircle);
            RuntimeArt.SpriteObject("EyeGlowR", root, new Vector2(0.18f, 0.20f),
                new Vector2(0.26f, 0.20f), new Color(0.92f, 0.42f, 1f, 0.34f), 30, true, RuntimeArt.SoftCircle);
        }


        private static void AddHiggsfieldVisual(
            Transform root,
            Sprite sprite,
            float targetHeight,
            Vector2 localPosition,
            int sortingOrder)
        {
            if (sprite == null) return;

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

        private static void EnemyShadow(Transform root, float width)
        {
            RuntimeArt.ShadowObject(
                "EnemyShadow", root, new Vector2(0f, -0.42f),
                new Vector2(width, width * 0.34f), 15, 0.34f);
        }

        private static void EyePair(Transform root, Vector2 center, int order, float radius)
        {
            float spacing = radius * 1.35f;
            RuntimeArt.SpriteObject("EyeL", root, center + new Vector2(-spacing, 0f),
                new Vector2(radius * 1.65f, radius * 1.85f), Color.white, order);
            RuntimeArt.SpriteObject("EyeR", root, center + new Vector2(spacing, 0f),
                new Vector2(radius * 1.65f, radius * 1.85f), Color.white, order);
            RuntimeArt.SpriteObject("PupilL", root, center + new Vector2(-spacing + radius * 0.18f, 0f),
                new Vector2(radius * 0.62f, radius * 0.82f), new Color(0.055f, 0.045f, 0.025f), order + 1);
            RuntimeArt.SpriteObject("PupilR", root, center + new Vector2(spacing + radius * 0.18f, 0f),
                new Vector2(radius * 0.62f, radius * 0.82f), new Color(0.055f, 0.045f, 0.025f), order + 1);
        }

        private static void Spark(Transform root, Vector2 position, float size, int order, Color color)
        {
            RuntimeArt.SpriteObject("Spark", root, position,
                new Vector2(size, size), color, order, true, RuntimeArt.Diamond, 45f);
        }
    }
}
