#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using FantasyTD.V2.Core;

namespace FantasyTD.V2.Editor
{
    /// <summary>
    /// Invokable via Unity batchmode -executeMethod
    /// FantasyTD.V2.Editor.BattleSimulationSmoke.RunSmoke.
    /// This does not claim gameplay-scene or device coverage.
    /// </summary>
    public static class BattleSimulationSmoke
    {
        [MenuItem("Tools/Fantasy TD V2/Run Deterministic Core Smoke")]
        public static void RunSmoke()
        {
            LevelDefinition level = MakeFixture();

            BattleSimulator a = new BattleSimulator(level);
            BattleSimulator b = new BattleSimulator(level);
            string traceA = RunWinningScenario(a);
            string traceB = RunWinningScenario(b);
            Require(traceA == traceB, "Identical commands must produce identical event traces.");
            Require(a.Result == BattleResult.Victory, "Expected scripted winning fixture.");
            Require(a.Kills == 2 && a.Leaks == 0, "Wrong kill/leak counts.");
            Require(a.BaseHp == 1, "Base HP changed on a winning fixture.");
            Require(a.Essence == 74, "Kill rewards or placement costs are incorrect.");
            Require(!a.StartNextWave(), "Cannot start another wave after victory.");
            Require(!a.TryPlaceTower("reserve", "shooter"),
                "No placement allowed after terminal state.");

            BattleSimulator noDefense = new BattleSimulator(level);
            Require(noDefense.StartNextWave(), "Wave must start.");
            AdvanceUntilTerminal(noDefense, 500);
            Require(noDefense.Result == BattleResult.Defeat,
                "Without defenders and with one life, enemy leak should defeat.");
            Require(noDefense.Leaks == 1 && noDefense.BaseHp == 0,
                "Defeat must result from a real leak.");

            BattleSimulator income = new BattleSimulator(level);
            Require(income.TryPlaceTower("reserve", "flower"), "Economy tower must place.");
            Require(income.Essence == 60, "Cost deduction incorrect.");
            Require(income.StartNextWave(), "Income scenario start failed.");
            for (int tick = 0; tick < 45; tick++) income.Step();
            Require(income.Essence == 80,
                "Passive generator must pay exactly at ticks 20 and 40.");
            Require(!income.TryPlaceTower("reserve", "flower"),
                "Occupied build spot cannot accept another tower.");
            Require(!income.TryPlaceTower("invalid-site", "shooter"),
                "Unknown build site must be rejected.");

            Debug.Log("FantasyTD V2 deterministic battle core smoke: PASS (two deterministic replays; victory/rewards; defeat/leak; economy; invalid placement).");
        }

        private static string RunWinningScenario(BattleSimulator sim)
        {
            var output = new StringBuilder();
            sim.EventRaised += evt => output.Append((int)evt.Kind).Append(':')
                .Append(evt.EntityId).Append(':').Append(evt.Value).Append(';');
            Require(sim.TryPlaceTower("front", "shooter"),
                "Shooter must place on empty site.");
            Require(!sim.TryPlaceTower("front", "shooter"),
                "Cannot place twice at same build site.");
            Require(sim.StartNextWave(), "Cannot start first wave.");
            AdvanceUntilTerminal(sim, 400);
            return output.ToString();
        }

        private static void AdvanceUntilTerminal(BattleSimulator sim, int maxSteps)
        {
            for (int tick = 0; tick < maxSteps &&
                sim.Result == BattleResult.InProgress; tick++)
                sim.Step();
            Require(sim.Result != BattleResult.InProgress,
                "Battle failed to resolve inside bounded smoke horizon.");
        }

        private static LevelDefinition MakeFixture()
        {
            var path = new List<MapPoint> {
                new MapPoint(0, 0), new MapPoint(1000, 0)
            };
            var sites = new Dictionary<string, MapPoint> {
                { "front", new MapPoint(500, 0) },
                { "reserve", new MapPoint(400, 120) }
            };
            var enemies = new Dictionary<string, EnemyDefinition> {
                { "wanderer", new EnemyDefinition("wanderer", 100, 3, 0, 12, 1) }
            };
            var towers = new Dictionary<string, TowerDefinition> {
                { "shooter", new TowerDefinition("shooter", 50, 20, Element.Physical,
                    300, 3) },
                { "flower", new TowerDefinition("flower", 40, 0, Element.Nature,
                    0, 1, incomeAmount: 10, incomeEveryTicks: 20) }
            };
            var waves = new List<WaveDefinition> {
                new WaveDefinition(new List<SpawnOrder> {
                    new SpawnOrder("wanderer", 0),
                    new SpawnOrder("wanderer", 20)
                })
            };
            return new LevelDefinition("FT2-SMOKE-01", path, sites, enemies,
                towers, waves, startingEssence: 100, startingBaseHp: 1);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("V2 smoke FAILED: " + message);
        }
    }
}
#endif
