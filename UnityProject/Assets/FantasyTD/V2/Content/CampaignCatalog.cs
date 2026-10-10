using System;
using System.Collections.Generic;
using FantasyTD.V2.Core;

namespace FantasyTD.V2.Content
{
    /// <summary>
    /// Authoring foundation for the 30-stage campaign.
    /// IMPORTANT: enemy/boss IDs are data definitions, NOT full behavior or boss phases.
    /// Art/audio, hero mechanics, world hazards, upgrades and device testing remain open.
    /// </summary>
    public static class CampaignCatalog
    {
        public const int WorldCount = 6;
        public const int StagesPerWorld = 5;
        public const int LevelCount = WorldCount * StagesPerWorld;

        public static readonly string[] WorldNames =
        {
            "Zielona Dolina", "Nocny Las", "Zalane Ruiny",
            "Mgielne Mokradla", "Cytadela Cierni", "Pekniety Swiat"
        };

        public static readonly string[] BossIds =
        {
            "root-warden", "twilight-owl", "swamp-colossus",
            "mist-queen", "thorn-general", "rift-heart"
        };

        // Four ordinary archetypes introduced per world.
        private static readonly string[][] WorldEnemies =
        {
            new[] { "imp", "runner", "armored", "swarm" },
            new[] { "shade", "nightfang", "duskwing", "specter" },
            new[] { "bogling", "tidewalker", "shellback", "floodseer" },
            new[] { "mistling", "harrier", "sporeborn", "wraith" },
            new[] { "spikeguard", "breaker", "catapult", "shieldbearer" },
            new[] { "riftling", "phasewalker", "voidknight", "starcaller" }
        };

        private static readonly string[] TowerIds =
        {
            "thorn", "bloom", "oak", "frost", "mushroom", "holly",
            "fern", "lily", "pumpkin", "rose", "willow", "cactus"
        };

        // Six distinct biome paths, varied per stage by a deterministic route modifier.
        // Each authored map must eventually receive its own custom environment and events.
        private static readonly MapPoint[][] WorldRoutes =
        {
            new[] { P(-1180, 280), P(-870, 280), P(-870, -270),
                P(-290, -270), P(-290, 310), P(330, 310),
                P(330, -210), P(900, -210), P(1170, -210) },
            new[] { P(-1190, -280), P(-860, -280), P(-860, 240),
                P(-500, 240), P(-500, -90), P(-20, -90),
                P(-20, 360), P(560, 360), P(560, -230), P(1160, -230) },
            new[] { P(-1180, 300), P(-720, 300), P(-720, -350),
                P(-170, -350), P(-170, 180), P(220, 180),
                P(220, -90), P(720, -90), P(720, 330), P(1160, 330) },
            new[] { P(-1160, -130), P(-750, -130), P(-750, 410),
                P(-240, 410), P(-240, -400), P(240, -400),
                P(240, 110), P(790, 110), P(790, -320), P(1170, -320) },
            new[] { P(-1170, 240), P(-660, 240), P(-660, -120),
                P(-1010, -120), P(-1010, -480), P(-100, -480),
                P(-100, 320), P(580, 320), P(580, -180), P(1150, -180) },
            new[] { P(-1190, -310), P(-920, -310), P(-920, 430),
                P(-420, 430), P(-420, -250), P(110, -250),
                P(110, 370), P(660, 370), P(660, -420), P(1170, -420) }
        };

        public static string LevelId(int oneBasedLevel)
        {
            ValidateIndex(oneBasedLevel);
            return oneBasedLevel == 1 ? StarterLevel.Id
                : "FT2-CAMPAIGN-" + oneBasedLevel.ToString("D2");
        }

        public static string WorldName(int oneBasedLevel)
        {
            ValidateIndex(oneBasedLevel);
            return WorldNames[(oneBasedLevel - 1) / StagesPerWorld];
        }

        public static bool IsBossStage(int oneBasedLevel)
        {
            ValidateIndex(oneBasedLevel);
            return oneBasedLevel % StagesPerWorld == 0;
        }

        public static IReadOnlyList<string> RegisteredOrdinaryEnemies
        {
            get
            {
                var entries = new List<string>();
                foreach (string[] world in WorldEnemies)
                    entries.AddRange(world);
                return entries.AsReadOnly();
            }
        }

        public static IReadOnlyList<string> RegisteredTowers
        {
            get { return Array.AsReadOnly(TowerIds); }
        }

        public static LevelDefinition CreateLevel(int oneBasedLevel)
        {
            ValidateIndex(oneBasedLevel);
            if (oneBasedLevel == 1)
                return StarterLevel.Create();

            int world = (oneBasedLevel - 1) / StagesPerWorld;
            int stage = (oneBasedLevel - 1) % StagesPerWorld;
            // Data-only enemy definitions. Unique movement/skill AI still needs engineering.
            var enemies = DefineEnemies();
            var towers = DefineTowers();
            var route = DefineRoute(world, stage);
            var sites = DefineSites(world, stage);
            var waves = DefineWaves(world, stage, oneBasedLevel);
            int initialEssence = 230 + world * 45 + stage * 15;
            int baseHp = Math.Max(8, 15 - world / 2);
            return new LevelDefinition(LevelId(oneBasedLevel), route, sites,
                enemies, towers, waves, initialEssence, baseHp);
        }

        public static IReadOnlyList<LevelDefinition> CreateAll()
        {
            var list = new List<LevelDefinition>(LevelCount);
            for (int i = 1; i <= LevelCount; i++) list.Add(CreateLevel(i));
            return list.AsReadOnly();
        }

        private static void ValidateIndex(int index)
        {
            if (index < 1 || index > LevelCount)
                throw new ArgumentOutOfRangeException("index",
                    "Campaign level must be between 1 and 30.");
        }

        private static MapPoint P(int x, int y) { return new MapPoint(x, y); }

        private static List<MapPoint> DefineRoute(int world, int stage)
        {
            var result = new List<MapPoint>();
            MapPoint[] original = WorldRoutes[world];
            for (int i = 0; i < original.Length; i++)
            {
                // Alternating lateral geometry bends; signed perturbations retain
                // 2D path clearance and distinct integer coordinates per stage.
                int xShift = i == 0 || i == original.Length - 1
                    ? 0 : ((i % 3) - 1) * 26 * stage;
                int yShift = i == 0 || i == original.Length - 1
                    ? 0 : ((i % 4) - 2) * 24 * stage;
                result.Add(P(original[i].X + xShift, original[i].Y + yShift));
            }
            return result;
        }

        private static Dictionary<string, MapPoint> DefineSites(int world, int stage)
        {
            // Authored tactical node coordinates are distinct from the route;
            // collision/coverage simulation will be a required acceptance test.
            var sites = new Dictionary<string, MapPoint>(StringComparer.Ordinal);
            var baseX = new[] { -1040, -660, -1010, -625, -455, -80, -50, -50, 510, 500, 580, 1020 };
            var baseY = new[] { 470, 455, 50, -440, 40, -465, 65, 480, 475, 80, -465, 50 };
            for (int i = 0; i < 12; i++)
            {
                int x = baseX[i] + (world % 2 == 0 ? 1 : -1) * stage * (9 + (i % 4) * 3);
                int y = baseY[i] + ((world + i) % 3 - 1) * stage * 12;
                sites.Add("s" + (i + 1).ToString("D2"), P(x, y));
            }
            return sites;
        }

        private static Dictionary<string, TowerDefinition> DefineTowers()
        {
            var data = new Dictionary<string, TowerDefinition>(StringComparer.Ordinal);
            // First three defender families from Forest01 plus nine distinct stat
            // profiles. Advanced class skills/upgrades are separate future work.
            data.Add("thorn", new TowerDefinition("thorn", 55, 24,
                Element.Physical, 290, 8));
            data.Add("bloom", new TowerDefinition("bloom", 45, 0,
                Element.Nature, 0, 1, incomeAmount: 15, incomeEveryTicks: 80));
            data.Add("oak", new TowerDefinition("oak", 105, 42,
                Element.Physical, 225, 17, slowPercent: 20, slowTicks: 12));
            data.Add("frost", new TowerDefinition("frost", 75, 16,
                Element.Frost, 280, 12, slowPercent: 45, slowTicks: 30));
            data.Add("mushroom", new TowerDefinition("mushroom", 80, 19,
                Element.Nature, 265, 7));
            data.Add("holly", new TowerDefinition("holly", 100, 39,
                Element.Physical, 275, 11));
            data.Add("fern", new TowerDefinition("fern", 115, 32,
                Element.Arcane, 325, 16));
            data.Add("lily", new TowerDefinition("lily", 90, 14,
                Element.Arcane, 345, 7));
            data.Add("pumpkin", new TowerDefinition("pumpkin", 120, 27,
                Element.Physical, 215, 19));
            data.Add("rose", new TowerDefinition("rose", 110, 36,
                Element.Fire, 270, 15));
            data.Add("willow", new TowerDefinition("willow", 95, 20,
                Element.Nature, 370, 9, slowPercent: 15, slowTicks: 15));
            data.Add("cactus", new TowerDefinition("cactus", 135, 68,
                Element.Physical, 460, 28));
            return data;
        }

        private static Dictionary<string, EnemyDefinition> DefineEnemies()
        {
            var result = new Dictionary<string, EnemyDefinition>(StringComparer.Ordinal);
            for (int world = 0; world < WorldCount; world++)
            {
                for (int i = 0; i < 4; i++)
                {
                    string id = WorldEnemies[world][i];
                    int health = 75 + world * 52 + i * 44;
                    int speed = 7 + ((i * 3 + world) % 7);
                    int resistance = i == 2 ? Math.Min(70, 35 + world * 4)
                        : (i == 3 ? 12 : 0);
                    int reward = 13 + world * 5 + i * 5;
                    result.Add(id, new EnemyDefinition(id, health, speed,
                        resistance, reward, 1 + (i == 2 ? 1 : 0)));
                }
                // Boss stats, not implemented phase or special abilities.
                string bossId = BossIds[world];
                result.Add(bossId, new EnemyDefinition(bossId,
                    750 + world * 450, 3 + world / 2, 25 + world * 5,
                    180 + world * 90, 5));
            }
            return result;
        }

        private static List<WaveDefinition> DefineWaves(int world, int stage, int level)
        {
            int waveCount = 3 + stage / 2 + (world >= 3 ? 1 : 0);
            var waves = new List<WaveDefinition>();
            for (int wave = 0; wave < waveCount; wave++)
            {
                var spawns = new List<SpawnOrder>();
                int enemyCount = 4 + stage + world + wave * 2;
                int spacing = Math.Max(16, 39 - world * 2 - stage * 2);
                for (int i = 0; i < enemyCount; i++)
                {
                    int sourceWorld = (world + i + wave) % (world + 1);
                    int archetype = (i * 3 + wave + stage) % 4;
                    string id = WorldEnemies[sourceWorld][archetype];
                    spawns.Add(new SpawnOrder(id, i * spacing));
                }
                if (stage == 4 && wave == waveCount - 1)
                    spawns.Add(new SpawnOrder(BossIds[world],
                        enemyCount * spacing + 55));
                waves.Add(new WaveDefinition(spawns));
            }
            return waves;
        }
    }
}
