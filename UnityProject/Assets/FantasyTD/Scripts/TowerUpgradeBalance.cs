using UnityEngine;

namespace FantasyTD.MapA
{
    public static class TowerUpgradeBalance
    {
        public const int MaxLevel = 3;

        public static int UpgradeCost(PlantKind kind, int currentLevel)
        {
            if (currentLevel >= MaxLevel) return 0;
            float factor = currentLevel == 1 ? 0.75f : 1.0f;
            float raw = Balance.Cost(kind) * factor;
            return Mathf.Max(25, 25 * Mathf.FloorToInt(raw / 25f + 0.5f));
        }

        public static int SellRefund(int totalInvested)
        {
            return Mathf.Max(0, Mathf.FloorToInt(totalInvested * 0.70f));
        }

        public static float SeedDamage(int level)
        {
            return level <= 1 ? 34f : (level == 2 ? 46f : 62f);
        }

        public static float SeedCooldown(int level)
        {
            return level <= 1 ? 0.90f : (level == 2 ? 0.78f : 0.68f);
        }

        public static float SeedRange(int level)
        {
            return level <= 1 ? 4.0f : (level == 2 ? 4.35f : 4.70f);
        }

        public static int FlowerIncome(int level)
        {
            return level <= 1 ? 20 : (level == 2 ? 25 : 30);
        }

        public static float FlowerInterval(int level)
        {
            return level >= 3 ? 18f : 20f;
        }

        public static float OakDamage(int level)
        {
            return level <= 1 ? 12f : (level == 2 ? 18f : 26f);
        }

        public static float OakRootDuration(int level)
        {
            return level <= 1 ? 2.0f : (level == 2 ? 2.2f : 2.4f);
        }

        public static float OakCadence(int level)
        {
            return level <= 1 ? 4.3f : (level == 2 ? 3.8f : 3.4f);
        }

        public static float OakRange(int level)
        {
            return level <= 1 ? 3.4f : (level == 2 ? 3.7f : 4.0f);
        }

        public static float FrostDamage(int level)
        {
            return level <= 1 ? 26f : (level == 2 ? 34f : 44f);
        }

        public static float FrostSlow(int level)
        {
            return level <= 1 ? 0.35f : (level == 2 ? 0.40f : 0.45f);
        }

        public static float FrostCooldown(int level)
        {
            return level <= 1 ? 0.95f : (level == 2 ? 0.85f : 0.75f);
        }

        public static float FrostRange(int level)
        {
            return level <= 1 ? 3.8f : (level == 2 ? 4.15f : 4.5f);
        }

        public static float FrostDuration(int level) { return 2.5f; }

        public static float AttackRange(PlantKind kind, int level)
        {
            switch (kind)
            {
                case PlantKind.SeedShooter:
                case PlantKind.MutationTower: return SeedRange(level);
                case PlantKind.OakGuardian: return OakRange(level);
                case PlantKind.FrostBud: return FrostRange(level);
                default: return 0f;
            }
        }

        public static string StatsSummary(PlantKind kind, int level)
        {
            switch (kind)
            {
                case PlantKind.SeedShooter:
                case PlantKind.MutationTower:
                    return "DMG " + SeedDamage(level).ToString("0") +
                           "  •  " + SeedCooldown(level).ToString("0.##") + " s" +
                           "  •  Zasięg " + SeedRange(level).ToString("0.0");

                case PlantKind.EssenceFlower:
                    return "+" + FlowerIncome(level) + " Esencji / " +
                           FlowerInterval(level).ToString("0.#") + " s";

                case PlantKind.OakGuardian:
                    return "DMG " + OakDamage(level).ToString("0") +
                           "  •  Root " + OakRootDuration(level).ToString("0.##") + " s" +
                           "  •  Zasięg " + OakRange(level).ToString("0.0");

                case PlantKind.FrostBud:
                    return "DMG " + FrostDamage(level).ToString("0") +
                           "  •  Slow " + Mathf.RoundToInt(FrostSlow(level) * 100f) + "%" +
                           "  •  Zasięg " + FrostRange(level).ToString("0.0");

                default:
                    return string.Empty;
            }
        }
    }
}
