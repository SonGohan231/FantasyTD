using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class PlantInstance : MonoBehaviour
    {
        public const int MaxLevel = 3;

        public GameSession Session { get; private set; }
        public PlantingSpot Spot { get; private set; }
        public PlantKind Kind { get; private set; }
        public int Level { get; private set; } = 1;
        public int TotalInvested { get; private set; }

        public bool CanUpgrade => Level < MaxLevel;
        public float DamageMultiplier => 1f + 0.25f * (Level - 1);
        public float AttackSpeedMultiplier => 1f + 0.12f * (Level - 1);
        public float RangeBonus => 0.15f * (Level - 1);
        public float ControlMultiplier => 1f + 0.15f * (Level - 1);
        public float EconomyMultiplier => 1f + 0.25f * (Level - 1);

        public int UpgradeCost
        {
            get
            {
                if (!CanUpgrade) return 0;
                float multiplier = Level == 1 ? 0.75f : 1.0f;
                return RoundToFive(Balance.Cost(Kind) * multiplier);
            }
        }

        public int SellValue => Mathf.FloorToInt(TotalInvested * 0.70f);

        public void Initialize(GameSession session, PlantingSpot spot, PlantKind kind, int purchaseCost)
        {
            Session = session;
            Spot = spot;
            Kind = kind;
            Level = 1;
            TotalInvested = Mathf.Max(0, purchaseCost);
        }

        public void ApplyUpgrade(int paidCost)
        {
            if (!CanUpgrade) return;
            Level++;
            TotalInvested += Mathf.Max(0, paidCost);
        }

        public string StatsText()
        {
            switch (Kind)
            {
                case PlantKind.SeedShooter:
                    return $"DMG {22f * DamageMultiplier:0}  |  Zasięg {3.2f + RangeBonus:0.0}\nAtak co {1f / AttackSpeedMultiplier:0.00}s";
                case PlantKind.EssenceFlower:
                    return $"+{Mathf.RoundToInt(20f * EconomyMultiplier)} Esencji\nco {20f / (1f + 0.08f * (Level - 1)):0.0}s";
                case PlantKind.OakGuardian:
                    return $"DMG {8f * DamageMultiplier:0}  |  Zasięg {2.4f + RangeBonus:0.0}\nKorzenie {1.75f * ControlMultiplier:0.00}s";
                case PlantKind.FrostBud:
                    return $"DMG {18f * DamageMultiplier:0}  |  Zasięg {3.2f + RangeBonus:0.0}\nSpowolnienie 35% / {2.5f * ControlMultiplier:0.0}s";
                default:
                    return string.Empty;
            }
        }

        private static int RoundToFive(float value)
        {
            return Mathf.Max(5, Mathf.RoundToInt(value / 5f) * 5);
        }
    }
}
