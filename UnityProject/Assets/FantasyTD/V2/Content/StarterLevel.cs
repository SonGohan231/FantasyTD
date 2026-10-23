using System.Collections.Generic;
using FantasyTD.V2.Core;

namespace FantasyTD.V2.Content
{
    /// <summary>
    /// Author-authored first playable vertical slice, shared by Unity view and validation.
    /// All coordinates use integer hundredths of world units; no engine-side game rules.
    /// </summary>
    public static class StarterLevel
    {
        public const string Id = "FT2-FOREST-01";

        public static LevelDefinition Create()
        {
            // Production adapter for the approved 28x20 Forest01 terrain plan.
            // X uses one world unit per cell; Y is scaled to the 16:9 battlefield.
            var path = new List<MapPoint>
            {
                new MapPoint(-1350, 280), new MapPoint(-950, 280),
                new MapPoint(-950, 120), new MapPoint(-450, 120),
                new MapPoint(-450, 280), new MapPoint(-50, 280),
                new MapPoint(-50, -40), new MapPoint(450, -40),
                new MapPoint(450, -200), new MapPoint(850, -200),
                new MapPoint(850, -40), new MapPoint(1350, -40)
            };
            var sites = new Dictionary<string, MapPoint>
            {
                {"B01", new MapPoint(-1150, 520)},
                {"B02", new MapPoint(-850, 440)},
                {"B03", new MapPoint(-650, -40)},
                {"B04", new MapPoint(-350, 520)},
                {"B05", new MapPoint(-250, 40)},
                {"B06", new MapPoint(150, 440)},
                {"B07", new MapPoint(150, -200)},
                {"B08", new MapPoint(650, 120)},
                {"B09", new MapPoint(950, -360)},
                {"B10", new MapPoint(1150, 200)}
            };
            var enemies = new Dictionary<string, EnemyDefinition>
            {
                {"imp", new EnemyDefinition("imp", 90, 7, 0, 20, 1)},
                {"runner", new EnemyDefinition("runner", 65, 12, 0, 18, 1)},
                {"armored", new EnemyDefinition("armored", 180, 5, 40, 30, 2)},
                {"swarm", new EnemyDefinition("swarm", 48, 10, 0, 10, 1)},
                {"root-warden", new EnemyDefinition("root-warden", 1100, 3, 30, 220, 6)}
            };
            var towers = new Dictionary<string, TowerDefinition>
            {
                {"thorn", new TowerDefinition("thorn", 55, 24, Element.Physical, 570, 8)},
                {"frost", new TowerDefinition("frost", 75, 16, Element.Frost, 600, 12,
                    slowPercent: 45, slowTicks: 30)},
                {"bloom", new TowerDefinition("bloom", 45, 0, Element.Nature, 0, 1,
                    incomeAmount: 15, incomeEveryTicks: 80)},
                {"oak", new TowerDefinition("oak", 105, 42, Element.Physical, 480, 17,
                    slowPercent: 20, slowTicks: 12)}
            };
            var waves = new List<WaveDefinition>
            {
                new WaveDefinition(new List<SpawnOrder>
                {
                    new SpawnOrder("imp", 0),
                    new SpawnOrder("imp", 34),
                    new SpawnOrder("runner", 70),
                    new SpawnOrder("imp", 108)
                }),
                new WaveDefinition(new List<SpawnOrder>
                {
                    new SpawnOrder("runner", 0),
                    new SpawnOrder("imp", 24),
                    new SpawnOrder("runner", 50),
                    new SpawnOrder("imp", 80),
                    new SpawnOrder("armored", 130),
                    new SpawnOrder("runner", 160)
                }),
                new WaveDefinition(new List<SpawnOrder>
                {
                    new SpawnOrder("imp", 0),
                    new SpawnOrder("runner", 20),
                    new SpawnOrder("armored", 40),
                    new SpawnOrder("runner", 70),
                    new SpawnOrder("armored", 105),
                    new SpawnOrder("imp", 130),
                    new SpawnOrder("runner", 155),
                    new SpawnOrder("armored", 180)
                }),
                new WaveDefinition(new List<SpawnOrder>
                {
                    new SpawnOrder("swarm", 0), new SpawnOrder("swarm", 14),
                    new SpawnOrder("runner", 30), new SpawnOrder("swarm", 44),
                    new SpawnOrder("armored", 70), new SpawnOrder("runner", 95),
                    new SpawnOrder("armored", 130)
                }),
                new WaveDefinition(new List<SpawnOrder>
                {
                    new SpawnOrder("armored", 0), new SpawnOrder("runner", 25),
                    new SpawnOrder("armored", 50), new SpawnOrder("imp", 78),
                    new SpawnOrder("armored", 105), new SpawnOrder("runner", 132),
                    new SpawnOrder("armored", 165), new SpawnOrder("swarm", 190)
                }),
                new WaveDefinition(new List<SpawnOrder>
                {
                    new SpawnOrder("armored", 0), new SpawnOrder("runner", 22),
                    new SpawnOrder("swarm", 44), new SpawnOrder("armored", 68),
                    new SpawnOrder("runner", 92), new SpawnOrder("armored", 118),
                    new SpawnOrder("root-warden", 170)
                })
            };
            return new LevelDefinition(Id, path, sites, enemies, towers, waves,
                startingEssence: 300, startingBaseHp: 20);
        }
    }
}
