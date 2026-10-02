using UnityEngine;

namespace FantasyTD.MapA
{
    public static class FantasyVisualFactory
    {
        public static GameObject CreatePlant(PlantKind kind, Vector2 position)
        {
            GameObject root = new GameObject(Balance.DisplayName(kind));
            root.transform.position = new Vector3(position.x, position.y, -0.25f);
            root.AddComponent<OrganicBob>();

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
                    Creature(root.transform, new Color(0.38f,0.62f,0.18f), 0.62f, false, false);
                    break;
                case EnemyKind.Runner:
                    Creature(root.transform, new Color(0.63f,0.30f,0.64f), 0.52f, true, false);
                    break;
                case EnemyKind.Brute:
                    Creature(root.transform, new Color(0.40f,0.32f,0.17f), 0.92f, false, false);
                    break;
                case EnemyKind.Armored:
                    Creature(root.transform, new Color(0.32f,0.56f,0.22f), 0.68f, false, true);
                    break;
                case EnemyKind.Boss:
                    Creature(root.transform, new Color(0.25f,0.43f,0.12f), 1.30f, false, true);
                    Crown(root.transform);
                    break;
            }

            return root;
        }

        private static void SeedShooter(Transform root)
        {
            RuntimeArt.SpriteObject("Stem", root, new Vector2(0f,-0.18f), new Vector2(0.18f,0.60f), new Color(0.16f,0.46f,0.13f), 11, false);
            RuntimeArt.SpriteObject("LeafL", root, new Vector2(-0.22f,-0.28f), new Vector2(0.42f,0.20f), new Color(0.21f,0.60f,0.16f), 12);
            RuntimeArt.SpriteObject("LeafR", root, new Vector2(0.20f,-0.36f), new Vector2(0.38f,0.18f), new Color(0.18f,0.53f,0.14f), 12);
            RuntimeArt.SpriteObject("Head", root, new Vector2(0f,0.20f), new Vector2(0.64f,0.58f), new Color(0.31f,0.74f,0.20f), 13);
            RuntimeArt.SpriteObject("Muzzle", root, new Vector2(0.32f,0.20f), new Vector2(0.30f,0.28f), new Color(0.22f,0.58f,0.15f), 14);
            Eyes(root, new Vector2(-0.10f,0.27f), 15);
        }

        private static void EssenceFlower(Transform root)
        {
            RuntimeArt.SpriteObject("Stem", root, new Vector2(0f,-0.22f), new Vector2(0.16f,0.64f), new Color(0.20f,0.48f,0.13f), 11, false);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                Vector2 p = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 0.27f + new Vector2(0f,0.18f);
                RuntimeArt.SpriteObject("Petal", root, p, new Vector2(0.28f,0.20f), new Color(1f,0.76f,0.12f), 12);
            }
            RuntimeArt.SpriteObject("Core", root, new Vector2(0f,0.18f), new Vector2(0.42f,0.42f), new Color(0.67f,0.32f,0.08f), 13);
            Eyes(root, new Vector2(0f,0.22f), 14);
        }

        private static void OakGuardian(Transform root)
        {
            RuntimeArt.SpriteObject("Trunk", root, new Vector2(0f,-0.05f), new Vector2(0.66f,0.86f), new Color(0.43f,0.25f,0.10f), 12);
            RuntimeArt.SpriteObject("Crown", root, new Vector2(0f,0.35f), new Vector2(0.92f,0.58f), new Color(0.16f,0.43f,0.12f), 13);
            RuntimeArt.SpriteObject("BranchL", root, new Vector2(-0.38f,0.05f), new Vector2(0.36f,0.14f), new Color(0.36f,0.20f,0.09f), 11);
            RuntimeArt.SpriteObject("BranchR", root, new Vector2(0.38f,0.05f), new Vector2(0.36f,0.14f), new Color(0.36f,0.20f,0.09f), 11);
            Eyes(root, new Vector2(0f,0.05f), 14);
        }

        private static void FrostBud(Transform root)
        {
            RuntimeArt.SpriteObject("Stem", root, new Vector2(0f,-0.20f), new Vector2(0.16f,0.62f), new Color(0.18f,0.48f,0.36f), 11, false);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f;
                Vector2 p = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 0.25f + new Vector2(0f,0.20f);
                RuntimeArt.SpriteObject("IcePetal", root, p, new Vector2(0.24f,0.38f), new Color(0.48f,0.85f,1f), 12);
            }
            RuntimeArt.SpriteObject("Bud", root, new Vector2(0f,0.20f), new Vector2(0.44f,0.44f), new Color(0.18f,0.55f,0.82f), 13);
            Eyes(root, new Vector2(0f,0.23f), 14);
        }

        private static void Creature(Transform root, Color body, float size, bool longLegs, bool armor)
        {
            RuntimeArt.SpriteObject("Body", root, new Vector2(0f,0f), new Vector2(size, size * 0.92f), body, 20);
            RuntimeArt.SpriteObject("EarL", root, new Vector2(-size*0.38f, size*0.22f), new Vector2(size*0.25f,size*0.38f), body*0.85f, 19);
            RuntimeArt.SpriteObject("EarR", root, new Vector2(size*0.38f, size*0.22f), new Vector2(size*0.25f,size*0.38f), body*0.85f, 19);
            float legY = -size*0.52f;
            float legH = longLegs ? size*0.52f : size*0.34f;
            RuntimeArt.SpriteObject("LegL", root, new Vector2(-size*0.20f,legY), new Vector2(size*0.14f,legH), body*0.75f, 18, false);
            RuntimeArt.SpriteObject("LegR", root, new Vector2(size*0.20f,legY), new Vector2(size*0.14f,legH), body*0.75f, 18, false);

            RuntimeArt.SpriteObject("EyeL", root, new Vector2(-size*0.14f,size*0.10f), new Vector2(size*0.12f,size*0.14f), Color.white, 22);
            RuntimeArt.SpriteObject("EyeR", root, new Vector2(size*0.14f,size*0.10f), new Vector2(size*0.12f,size*0.14f), Color.white, 22);
            RuntimeArt.SpriteObject("PupilL", root, new Vector2(-size*0.12f,size*0.10f), new Vector2(size*0.05f,size*0.07f), new Color(0.08f,0.06f,0.04f), 23);
            RuntimeArt.SpriteObject("PupilR", root, new Vector2(size*0.16f,size*0.10f), new Vector2(size*0.05f,size*0.07f), new Color(0.08f,0.06f,0.04f), 23);

            if (armor)
            {
                RuntimeArt.SpriteObject("Helmet", root, new Vector2(0f,size*0.30f), new Vector2(size*0.78f,size*0.30f), new Color(0.32f,0.34f,0.32f), 24);
                RuntimeArt.SpriteObject("HelmetBand", root, new Vector2(0f,size*0.20f), new Vector2(size*0.85f,size*0.10f), new Color(0.18f,0.19f,0.18f), 25, false);
            }
        }

        private static void Crown(Transform root)
        {
            RuntimeArt.SpriteObject("CrownBase", root, new Vector2(0f,0.82f), new Vector2(0.72f,0.20f), new Color(0.75f,0.52f,0.08f), 30, false);
            RuntimeArt.SpriteObject("CrownTip1", root, new Vector2(-0.24f,1.00f), new Vector2(0.20f,0.38f), new Color(0.85f,0.62f,0.10f), 29);
            RuntimeArt.SpriteObject("CrownTip2", root, new Vector2(0f,1.06f), new Vector2(0.20f,0.46f), new Color(0.92f,0.68f,0.10f), 29);
            RuntimeArt.SpriteObject("CrownTip3", root, new Vector2(0.24f,1.00f), new Vector2(0.20f,0.38f), new Color(0.85f,0.62f,0.10f), 29);
        }

        private static void Eyes(Transform root, Vector2 center, int order)
        {
            RuntimeArt.SpriteObject("EyeL", root, center + new Vector2(-0.10f,0f), new Vector2(0.11f,0.14f), Color.white, order);
            RuntimeArt.SpriteObject("EyeR", root, center + new Vector2(0.10f,0f), new Vector2(0.11f,0.14f), Color.white, order);
            RuntimeArt.SpriteObject("PupilL", root, center + new Vector2(-0.08f,0f), new Vector2(0.045f,0.065f), new Color(0.05f,0.05f,0.04f), order+1);
            RuntimeArt.SpriteObject("PupilR", root, center + new Vector2(0.12f,0f), new Vector2(0.045f,0.065f), new Color(0.05f,0.05f,0.04f), order+1);
        }
    }
}
