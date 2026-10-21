using System;
using System.Collections.Generic;
using FantasyTD.V2.Content;
using FantasyTD.V2.Core;

internal static class Program
{
    private static int Main()
    {
        try
        {
            VerifyStarterLevel();
            VerifyCampaignRegistry();
            VerifyBattleFeedbackCues();
            Console.WriteLine("V2CoreSmoke PASS: first forest simulation and all 30 campaign definitions.");
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("V2CoreSmoke FAILED: " + e);
            return 1;
        }
    }

    private static void VerifyStarterLevel()
    {
        var level = StarterLevel.Create();
        Require(level.Sites.Count == 12, "starter has 12 sites");
        Require(level.Enemies.Count >= 2, "starter has at least 2 enemies");
        Require(level.Waves.Count == 3, "starter has 3 waves");
        Require(level.Towers.Count == 3, "starter has 3 towers");
        Require(level.StartingBaseHp == 15, "starter begins with 15 HP");

        var battle = new BattleSimulator(level);
        Require(battle.TryPlaceTower("a01", "thorn"), "place thorn");
        Require(!battle.TryPlaceTower("a01", "frost"), "no site overlap");
        Require(!battle.TryPlaceTower("bad", "thorn"), "reject invalid site");
        Require(battle.Essence == 175, "tower cost");
        Require(battle.StartNextWave(), "start wave 1");
        Require(!battle.StartNextWave(), "no overlapping waves");
        battle.Step();
        Require(battle.Enemies.Count == 1, "first enemy spawns");
        for (int i = 0; i < 2500 && battle.WaveActive; i++) battle.Step();
        Require(!battle.WaveActive, "wave 1 completes");
        Require(battle.Result == BattleResult.InProgress, "wave 1 base survives");
        Require(battle.CanStartWave, "second wave available");

        var replay = new BattleSimulator(level);
        Require(replay.TryPlaceTower("a01", "thorn"), "replay placement");
        Require(replay.StartNextWave(), "replay starts");
        for (int i = 0; i < battle.TotalTicks; i++) replay.Step();
        Require(replay.TotalTicks == battle.TotalTicks, "deterministic ticks");
        Require(replay.Kills == battle.Kills && replay.Leaks == battle.Leaks &&
                replay.Essence == battle.Essence && replay.BaseHp == battle.BaseHp,
                "deterministic replay state");
        Console.WriteLine("StarterLevel PASS: planting, spawn, wave clear, replay.");
    }

    private static void VerifyCampaignRegistry()
    {
        Require(CampaignCatalog.WorldCount == 6, "6 worlds expected");
        Require(CampaignCatalog.StagesPerWorld == 5, "5 stages per world");
        Require(CampaignCatalog.LevelCount == 30, "30 stages expected");
        Require(CampaignCatalog.BossIds.Length == 6, "six boss identifiers");
        Require(CampaignCatalog.RegisteredOrdinaryEnemies.Count == 24,
            "24 enemy archetype identifiers");
        Require(CampaignCatalog.RegisteredTowers.Count == 12,
            "12 defender family identifiers");
        var all = CampaignCatalog.CreateAll();
        Require(all.Count == 30, "CreateAll must include 30 stages");

        var routes = new HashSet<string>(StringComparer.Ordinal);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        for (int levelNumber = 1; levelNumber <= 30; levelNumber++)
        {
            LevelDefinition level = CampaignCatalog.CreateLevel(levelNumber);
            Require(level != null, "null level " + levelNumber);
            Require(level.Id == CampaignCatalog.LevelId(levelNumber),
                "incorrect level ID " + levelNumber);
            Require(ids.Add(level.Id), "duplicate ID " + level.Id);
            Require(level.PathLength >= 1000, "too-short level path");
            Require(level.Path.Count >= 8, "path needs actual turns");
            Require(level.Sites.Count == 12, "12 planting sites required");
            Require(level.Waves.Count >= 3, "at least 3 waves");
            Require(level.Towers.Count >= 3, "at least 3 accessible towers");
            Require(level.StartingBaseHp > 0, "base health must be positive");
            string routeKey = "";
            foreach (MapPoint point in level.Path)
                routeKey += point.X + ":" + point.Y + ";";
            Require(routes.Add(routeKey), "duplicate waypoint path " + level.Id);

            bool hasBoss = false;
            foreach (var wave in level.Waves)
            {
                Require(wave.Spawns.Count > 0, "empty wave");
                foreach (SpawnOrder order in wave.Spawns)
                    for (int b = 0; b < CampaignCatalog.BossIds.Length; b++)
                        if (order.EnemyId == CampaignCatalog.BossIds[b]) hasBoss = true;
            }
            Require(hasBoss == CampaignCatalog.IsBossStage(levelNumber),
                "boss appears on wrong stage: " + levelNumber);
            if (levelNumber > 1)
            {
                Require(level.Towers.Count == 12, "catalog needs 12 tower definitions");
                Require(level.Enemies.Count == 30, "24 ordinary + 6 bosses required");
            }

            var battle = new BattleSimulator(level);
            Require(battle.CanStartWave, "battle wave not ready");
            Require(battle.StartNextWave(), "cannot start first wave");
            battle.Step();
            Require(battle.Enemies.Count >= 1,
                "first enemy missing in " + level.Id);
        }
        Require(routes.Count == 30, "thirty distinct paths");
        Console.WriteLine("CampaignCatalog PASS: 6 worlds, 30 unique routes, 12 defenders, " +
            "24 enemy IDs, 6 bosses, 6 boss encounters, validated wave spawns.");
        // This test validates data and basic spawning, NOT balanced difficulty,
        // boss phase mechanics, artwork, save system, or actual device play.
    }


    private static void VerifyBattleFeedbackCues()
    {
        var placement = BattleFeedbackCatalog.Map(
            new BattleEvent(BattleEventKind.TowerPlaced, 0, "thorn", -55, 1));
        Require(placement.Kind == FeedbackKind.Plant &&
                placement.Message == "-55" && placement.Audible,
                "placing a defender must give distinct feedback");

        var damaged = BattleFeedbackCatalog.Map(
            new BattleEvent(BattleEventKind.EnemyDamaged, 7, "thorn", 24, 1));
        Require(damaged.Kind == FeedbackKind.Hit &&
                damaged.Message == "-24" && damaged.Audible,
                "hits must have readable damage and sound");

        var killed = BattleFeedbackCatalog.Map(
            new BattleEvent(BattleEventKind.EnemyKilled, 7, "imp", 20, 1));
        Require(killed.Kind == FeedbackKind.Kill &&
                killed.Message == "+20" && killed.Audible,
                "kills must be visually distinct and show reward");

        var leaked = BattleFeedbackCatalog.Map(
            new BattleEvent(BattleEventKind.EnemyLeaked, 8, "runner", 2, 1));
        Require(leaked.Kind == FeedbackKind.Leak &&
                leaked.Message == "-2" && leaked.Audible,
                "base damage must give urgent feedback");

        var income = BattleFeedbackCatalog.Map(
            new BattleEvent(BattleEventKind.Income, 0, "bloom", 15, 1));
        Require(income.Kind == FeedbackKind.Income &&
                income.Message == "+15" && !income.Audible,
                "income cue must be distinct and not spam audio");

        Require(BattleFeedbackCatalog.Map(
            new BattleEvent(BattleEventKind.WaveStarted, 0, "forest", 1, 1))
            .Kind == FeedbackKind.Wave, "wave start cue missing");
        Require(BattleFeedbackCatalog.Map(
            new BattleEvent(BattleEventKind.Victory, 0, "forest", 10, 3))
            .Kind == FeedbackKind.Victory, "victory cue missing");
        Require(BattleFeedbackCatalog.Map(
            new BattleEvent(BattleEventKind.Defeat, 0, "forest", 0, 3))
            .Kind == FeedbackKind.Defeat, "defeat cue missing");
        Console.WriteLine("BattleFeedback PASS: plant, hit, kill, leak, income, wave, result.");
    }

    private static void Require(bool ok, string message)
    {
        if (!ok) throw new InvalidOperationException(message);
    }
}
