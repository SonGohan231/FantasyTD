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
            return level <= 1 ? 22f : (level == 2 ? 30f : 40f);
        }

        public static float SeedCooldown(int level)
        {
            return level >= 3 ? 0.85f : 1.0f;
        }

        public static float SeedRange(int level) { return 3.2f; }

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
            return level <= 1 ? 8f : (level == 2 ? 12f : 16f);
        }

        public static float OakRootDuration(int level)
        {
            return level <= 1 ? 1.75f : (level == 2 ? 1.9f : 2.1f);
        }

        public static float OakCadence(int level)
        {
            return level >= 3 ? 4.5f : 5f;
        }

        public static float OakRange(int level) { return 2.4f; }

        public static float FrostDamage(int level)
        {
            return level <= 1 ? 18f : (level == 2 ? 24f : 30f);
        }

        public static float FrostSlow(int level)
        {
            return level <= 1 ? 0.35f : (level == 2 ? 0.375f : 0.40f);
        }

        public static float FrostCooldown(int level)
        {
            return level >= 3 ? 0.9f : 1f;
        }

        public static float FrostRange(int level) { return 3.2f; }
        public static float FrostDuration(int level) { return 2.5f; }

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
                           "  •  " + OakCadence(level).ToString("0.#") + " s";

                case PlantKind.FrostBud:
                    return "DMG " + FrostDamage(level).ToString("0") +
                           "  •  Slow " + Mathf.RoundToInt(FrostSlow(level) * 100f) + "%" +
                           "  •  " + FrostCooldown(level).ToString("0.##") + " s";

                default:
                    return string.Empty;
            }
        }
    }
}
