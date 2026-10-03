using UnityEngine;

namespace FantasyTD.MapA
{
    public static class HiggsfieldArt
    {
        public static Sprite MapBackground()
        {
            return Resources.Load<Sprite>("Higgsfield/MAP_A/MapBackground");
        }

        public static Sprite Plant(PlantKind kind)
        {
            switch (kind)
            {
                case PlantKind.SeedShooter: return Resources.Load<Sprite>("Higgsfield/MAP_A/Towers/Pedostrzal");
                case PlantKind.EssenceFlower: return Resources.Load<Sprite>("Higgsfield/MAP_A/Towers/KwiatEsencji");
                case PlantKind.OakGuardian: return Resources.Load<Sprite>("Higgsfield/MAP_A/Towers/DebowyStraznik");
                case PlantKind.FrostBud: return Resources.Load<Sprite>("Higgsfield/MAP_A/Towers/MroznyPak");
                default: return null;
            }
        }

        public static Sprite Enemy(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Wanderer: return Resources.Load<Sprite>("Higgsfield/MAP_A/Enemies/Chochlik");
                case EnemyKind.Runner: return Resources.Load<Sprite>("Higgsfield/MAP_A/Enemies/WichrowySkrzat");
                case EnemyKind.Brute: return Resources.Load<Sprite>("Higgsfield/MAP_A/Enemies/OgrZarosli");
                case EnemyKind.Armored: return Resources.Load<Sprite>("Higgsfield/MAP_A/Enemies/ZelaznyGoblin");
                case EnemyKind.Boss: return Resources.Load<Sprite>("Higgsfield/MAP_A/Enemies/KrolCierni");
                default: return null;
            }
        }

        public static Sprite SpawnPortal()
        {
            return Resources.Load<Sprite>("Higgsfield/MAP_A/Environment/SpawnPortal");
        }

        public static Sprite HomeTree()
        {
            return Resources.Load<Sprite>("Higgsfield/MAP_A/Environment/HomeTree");
        }
    }
}
