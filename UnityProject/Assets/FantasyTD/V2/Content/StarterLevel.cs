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
            var path = new List<MapPoint>
            {
                new MapPoint(-1180, 320),
                new MapPoint(-840, 320),
                new MapPoint(-840, -250),
                new MapPoint(-330, -250),
                new MapPoint(-330, 280),
                new MapPoint(300, 280),
                new MapPoint(300, -220),
                new MapPoint(880, -220),
                new MapPoint(1170, -220)
            };
            var sites = new Dictionary<string, MapPoint>
            {
                {"a01", new MapPoint(-1050, 460)},
                {"a02", new MapPoint(-670, 400)},
                {"a03", new MapPoint(-1050, -40)},
                {"a04", new MapPoint(-620, -470)},
                {"a05", new MapPoint(-470, 5)},
                {"a06", new MapPoint(-85, -440)},
                {"a07", new MapPoint(-75, 80)},
                {"a08", new MapPoint(-25, 470)},
                {"a09", new MapPoint(530, 470)},
                {"a10", new MapPoint(540, 80)},
                {"a11", new MapPoint(580, -450)},
                {"a12", new MapPoint(1030, 20)}
            };
            var enemies = new Dictionary<string, EnemyDefinition>
            {
                {"imp", new EnemyDefinition("imp", 90, 7, 0, 20, 1)},
                {"runner", new EnemyDefinition("runner", 65, 12, 0, 18, 1)},
                {"armored", new EnemyDefinition("armored", 180, 5, 40, 30, 2)}
            };
            var towers = new Dictionary<string, TowerDefinition>
            {
                {"thorn", new TowerDefinition("thorn", 55, 24, Element.Physical, 290, 8)},
                {"frost", new TowerDefinition("frost", 75, 16, Element.Frost, 280, 12,
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
