using System;
using FantasyTD.V2.Content;
using FantasyTD.V2.Core;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var level = StarterLevel.Create();
            Require(level.Sites.Count == 12, "12 sites");
            Require(level.Enemies.Count >= 2, "at least 2 enemies");
            Require(level.Waves.Count == 3, "3 waves");
            Require(level.Towers.Count == 3, "3 towers");
            Require(level.StartingBaseHp == 15, "15 starting HP");

            var battle = new BattleSimulator(level);
            Require(battle.TryPlaceTower("a01", "thorn"), "place thorn");
            Require(!battle.TryPlaceTower("a01", "frost"), "block site overlap");
            Require(!battle.TryPlaceTower("bad", "thorn"), "block invalid site");
            Require(battle.Essence == 175, "tower cost deducted");
            Require(battle.StartNextWave(), "wave 1 starts");
            Require(!battle.StartNextWave(), "cannot overlap waves");
            battle.Step();
            Require(battle.Enemies.Count == 1, "first enemy spawned");
            for (int i = 0; i < 2500 && battle.WaveActive; i++)
                battle.Step();
            Require(!battle.WaveActive, "wave 1 completes");
            Require(battle.Result == BattleResult.InProgress, "wave 1 doesn't defeat 15 HP base");
            Require(battle.CanStartWave, "wave 2 now available");

            var replay = new BattleSimulator(level);
            Require(replay.TryPlaceTower("a01", "thorn"), "replay placement");
            Require(replay.StartNextWave(), "replay starts");
            for (int i = 0; i < battle.TotalTicks; i++) replay.Step();
            Require(replay.TotalTicks == battle.TotalTicks, "deterministic ticks");
            Require(replay.Kills == battle.Kills && replay.Leaks == battle.Leaks &&
                    replay.Essence == battle.Essence &&
                    replay.BaseHp == battle.BaseHp,
                    "deterministic game state");
            Console.WriteLine("V2CoreSmoke PASS: 12 sites; 3 defenders; 3 waves; placement; enemy spawn; wave resolution; deterministic replay.");
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("V2CoreSmoke FAILED: " + e);
            return 1;
        }
    }

    private static void Require(bool ok, string message)
    {
        if (!ok) throw new InvalidOperationException(message);
    }
}
