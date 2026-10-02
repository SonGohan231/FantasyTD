namespace FantasyTD.MapA
{
    public enum PlantKind
    {
        SeedShooter,
        EssenceFlower,
        OakGuardian,
        FrostBud
    }

    public enum EnemyKind
    {
        Wanderer,
        Runner,
        Brute,
        Armored,
        Boss
    }

    public enum DamageType
    {
        Physical,
        Frost,
        Root
    }

    public static class Balance
    {
        public const int StartEssence = 300;
        public const int BaseHp = 20;

        public static int Cost(PlantKind kind)
        {
            switch (kind)
            {
                case PlantKind.SeedShooter: return 100;
                case PlantKind.EssenceFlower: return 100;
                case PlantKind.OakGuardian: return 100;
                case PlantKind.FrostBud: return 150;
                default: return 9999;
            }
        }

        public static string DisplayName(PlantKind kind)
        {
            switch (kind)
            {
                case PlantKind.SeedShooter: return "Pędostrzał";
                case PlantKind.EssenceFlower: return "Kwiat Esencji";
                case PlantKind.OakGuardian: return "Dębowy Strażnik";
                case PlantKind.FrostBud: return "Mroźny Pąk";
                default: return kind.ToString();
            }
        }

        public static string EnemyName(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Wanderer: return "Chochlik";
                case EnemyKind.Runner: return "Wichrowy Skrzat";
                case EnemyKind.Brute: return "Ogr Zarośli";
                case EnemyKind.Armored: return "Żelazny Goblin";
                case EnemyKind.Boss: return "Król Cierni";
                default: return kind.ToString();
            }
        }
    }
}
