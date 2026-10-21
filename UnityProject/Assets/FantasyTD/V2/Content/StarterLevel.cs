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
            // Aligned to the recovered approved MAP1 texture's 24x14 authored geometry.
            // Map coordinates scaled to FT2: (x / 24 * 29 - 14.5, y / 14 * 16 - 8).
            var path = new List<MapPoint>
            {
                new MapPoint(-1081, 800),
                new MapPoint(-990, 647),
                new MapPoint(-905, 480),
                new MapPoint(-806, 299),
                new MapPoint(-678, 105),
                new MapPoint(-487, -33),
                new MapPoint(-246, -103),
                new MapPoint(36, -144),
                new MapPoint(320, -200),
                new MapPoint(603, -256),
                new MapPoint(872, -353),
                new MapPoint(1098, -491),
                new MapPoint(1283, -658),
                new MapPoint(1450, -783)
            };
            var sites = new Dictionary<string, MapPoint>
            {
                {"a01", new MapPoint(-1131, 321)},
                {"a02", new MapPoint(-459, 661)},
                {"a03", new MapPoint(-48, 429)},
                {"a04", new MapPoint(-1151, -218)},
                {"a05", new MapPoint(-749, -263)},
                {"a06", new MapPoint(362, 337)},
                {"a07", new MapPoint(714, 100)},
                {"a08", new MapPoint(-467, -665)},
                {"a09", new MapPoint(285, -533)},
                {"a10", new MapPoint(1318, 231)},
                {"a11", new MapPoint(1090, -103)},
                {"a12", new MapPoint(1372, -297)}
            };
            var enemies = new Dictionary<string, EnemyDefinition>
            {
                {"imp", new EnemyDefinition("imp", 90, 7, 0, 20, 1)},
                {"runner", new EnemyDefinition("runner", 65, 12, 0, 18, 1)},
                {"armored", new EnemyDefinition("armored", 180, 5, 40, 30, 2)}
            };
            var towers = new Dictionary<string, TowerDefinition>
            {
                {"thorn", new TowerDefinition("thorn", 55, 24, Element.Physical, 570, 8)},
                {"frost", new TowerDefinition("frost", 75, 16, Element.Frost, 600, 12,
                    slowPercent: 45, slowTicks: 30)},
                {"bloom", new TowerDefinition("bloom", 45, 0, Element.Nature, 0, 1,
                    incomeAmount: 15, incomeEveryTicks: 80)}
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
                })
            };
            return new LevelDefinition(Id, path, sites, enemies, towers, waves,
                startingEssence: 230, startingBaseHp: 15);
        }
    }
}
