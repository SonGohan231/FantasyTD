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
            VerifyTowerUpgradeEconomy();
            VerifyCampaignRegistry();
            VerifyBattleFeedbackCues();
            VerifyCompleteForestSession();
            VerifyPauseResume();
            VerifyForestArtGeometry();
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
        Require(level.Sites.Count == 10, "approved Forest01 has 10 build sites");
        Require(level.Enemies.Count == 5, "Forest01 has four enemies and one boss");
        Require(level.Waves.Count == 6, "Forest01 has 6 waves");
        Require(level.Towers.Count == 4, "Forest01 has four defenders");
        Require(level.StartingBaseHp == 20, "Forest01 begins with 20 HP");

        var battle = new BattleSimulator(level);
        Require(battle.TryPlaceTower("B01", "thorn"), "place thorn");
        Require(!battle.TryPlaceTower("B01", "frost"), "no site overlap");
        Require(!battle.TryPlaceTower("bad", "thorn"), "reject invalid site");
        Require(battle.Essence == 245, "first thorn deducts 55 of 300 Essence");
        Require(battle.StartNextWave(), "start wave 1");
        Require(!battle.StartNextWave(), "no overlapping waves");
        battle.Step();
        Require(battle.Enemies.Count == 1, "first enemy spawns");
        for (int i = 0; i < 2500 && battle.WaveActive; i++) battle.Step();
        Require(!battle.WaveActive, "wave 1 completes");
        Require(battle.Result == BattleResult.InProgress, "wave 1 base survives");
        Require(battle.CanStartWave, "second wave available");

        var replay = new BattleSimulator(level);
        Require(replay.TryPlaceTower("B01", "thorn"), "replay placement");
        Require(replay.StartNextWave(), "replay starts");
        for (int i = 0; i < battle.TotalTicks; i++) replay.Step();
        Require(replay.TotalTicks == battle.TotalTicks, "deterministic ticks");
        Require(replay.Kills == battle.Kills && replay.Leaks == battle.Leaks &&
                replay.Essence == battle.Essence && replay.BaseHp == battle.BaseHp,
                "deterministic replay state");
        Console.WriteLine("StarterLevel PASS: planting, spawn, wave clear, replay.");
    }

    private static void VerifyTowerUpgradeEconomy()
    {
        var battle = new BattleSimulator(StarterLevel.Create());
        int beginning = battle.Essence;
        int upgradedEvents = 0;
        battle.EventRaised += evt => { if (evt.Kind == BattleEventKind.TowerUpgraded) upgradedEvents++; };
        Require(battle.TryPlaceTower("B01", "thorn"), "build first defender for upgrade test");
        var tower = battle.PlacedTowers["B01"];
        int originalDamage = tower.Damage;
        int originalRange = tower.RangeCentimeters;
        int firstUpgradeCost = tower.UpgradeCost;
        Require(battle.TryUpgradeTower("B01"), "first level upgrade should succeed");
        Require(tower.Level == 2 && battle.Essence == beginning - tower.Definition.Cost - firstUpgradeCost,
            "first upgrade must change level and deduct exact Essence cost");
        int secondUpgradeCost = tower.UpgradeCost;
        Require(battle.TryUpgradeTower("B01"), "second level upgrade should succeed");
        Require(tower.Level == TowerState.MaximumLevel &&
            battle.Essence == beginning - tower.Definition.Cost - firstUpgradeCost - secondUpgradeCost,
            "second upgrade must reach level cap and deduct exact cost");
        Require(tower.Damage > originalDamage && tower.RangeCentimeters > originalRange,
            "upgrades must improve authoritative combat statistics");
        int cappedEssence = battle.Essence;
        Require(!battle.TryUpgradeTower("B01") && !battle.TryUpgradeTower("not-a-site") &&
            battle.Essence == cappedEssence && upgradedEvents == 2,
            "MAX or invalid site must not charge Essence or produce upgrade events");
        Console.WriteLine("TowerUpgrade PASS: L1 to L3, exact spending, stat increases, cap and events.");
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
            Require(level.Sites.Count == (levelNumber == 1 ? 10 : 12), "Forest01 has 10, campaign definitions have 12 sites");
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
            Require(hasBoss == (levelNumber == 1 || CampaignCatalog.IsBossStage(levelNumber)),
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


    private static void VerifyCompleteForestSession()
    {
        LevelDefinition level = StarterLevel.Create();
        string[] strategicPositions =
        {
            "B01", "B05", "B07", "B09", "B02", "B06", "B08", "B10", "B03", "B04"
        };
        var defended = new BattleSimulator(level);
        int nextPosition = 0;
        while (nextPosition < 4)
            Require(defended.TryPlaceTower(strategicPositions[nextPosition++], "thorn"),
                "starter defender placement must be affordable");

        for (int wave = 1; wave <= 6; wave++)
        {
            Require(defended.StartNextWave(), "start defended wave " + wave);
            int ticks = 0;
            while (defended.WaveActive && defended.Result == BattleResult.InProgress &&
                   ticks < 4000)
            {
                if (ticks % 20 == 0 && nextPosition < strategicPositions.Length &&
                    defended.Essence >= level.Towers["thorn"].Cost)
                    Require(defended.TryPlaceTower(strategicPositions[nextPosition++], "thorn"),
                        "reinforcement placement failed");
                defended.Step();
                ticks++;
            }
            Require(!defended.WaveActive, "defended wave " + wave + " did not end");
            Require(defended.Result != BattleResult.Defeat,
                "a well-funded defensive strategy must survive wave " + wave);
        }
        Require(defended.Result == BattleResult.Victory,
            "defended full Forest01 session must reach victory");
        Require(defended.Kills >= 1, "defended session must actually fight enemies");
        Require(!defended.CanStartWave, "completed level must block extra wave");
        Require(!defended.TryPlaceTower("B06", "thorn"),
            "completed level must reject further placements");

        // An undefended base must fail through actual enemy leaks.
        var undefended = new BattleSimulator(level);
        for (int wave = 1; wave <= 6 &&
             undefended.Result == BattleResult.InProgress; wave++)
        {
            Require(undefended.StartNextWave(),
                "start undefended wave " + wave);
            int ticks = 0;
            while (undefended.WaveActive && ticks++ < 4000)
                undefended.Step();
            Require(!undefended.WaveActive, "undefended wave timeout");
        }
        Require(undefended.Result == BattleResult.Defeat,
            "undefended Forest01 must lead to defeat, not accidental victory");

        var retry = new BattleSimulator(level);
        Require(retry.Essence == level.StartingEssence &&
                retry.BaseHp == level.StartingBaseHp && retry.WaveNumber == 0 &&
                retry.Result == BattleResult.InProgress &&
                retry.TryPlaceTower("B01", "thorn"),
                "restart must reset combat and economy state");
        Console.WriteLine("ForestSession PASS: six-wave victory, real defeat, clean restart.");
    }


    private static void VerifyPauseResume()
    {
        LevelDefinition level = StarterLevel.Create();
        var battle = new BattleSimulator(level);
        var gate = new BattlePauseGate();
        Require(gate.CanAdvance, "new battle starts unpaused");
        Require(battle.StartNextWave(), "paused scenario starts its wave");
        battle.Step();
        int before = battle.TotalTicks;
        gate.Toggle();
        Require(gate.IsPaused && !gate.CanAdvance, "pause blocks simulation");
        for (int i = 0; i < 50; i++)
            if (gate.CanAdvance) battle.Step();
        Require(battle.TotalTicks == before, "time must not advance while paused");
        gate.Toggle();
        Require(gate.CanAdvance && !gate.IsPaused, "resume restores simulation");
        if (gate.CanAdvance) battle.Step();
        Require(battle.TotalTicks == before + 1, "resume advances exactly one tick");
        gate.Toggle();
        gate.Reset();
        Require(gate.CanAdvance, "restart must clear pause state");
        Console.WriteLine("PauseResume PASS: freeze, resume and reset.");
    }

    private static void VerifyForestArtGeometry()
    {
        // Current approved Forest01 adapter: 28x20 terrain, 12 routed waypoints,
        // ten fixed B01..B10 sites. This validates model geometry, not pixels.
        LevelDefinition level = StarterLevel.Create();
        Require(level.Path.Count == 12,
            "Forest01 path must follow all 12 approved waypoints");
        Require(level.Path[0].X == -1350 && level.Path[0].Y == 280,
            "Forest01 spawn must align with artwork north gate");
        MapPoint destination = level.Path[level.Path.Count - 1];
        Require(destination.X == 1350 && destination.Y == -40,
            "Forest01 exit must align with artwork southeast gate");
        Require(level.Sites["B01"].X == -1150 &&
                level.Sites["B01"].Y == 520,
            "B01 build anchor must match approved terrain");
        Require(level.Sites["B10"].X == 1150 &&
                level.Sites["B10"].Y == 200,
            "B10 build anchor must match approved terrain");
        Console.WriteLine("ForestArtGeometry PASS: 12 road points, both gates, B01/B10 anchors.");
    }

    private static void Require(bool ok, string message)
    {
        if (!ok) throw new InvalidOperationException(message);
    }
}
