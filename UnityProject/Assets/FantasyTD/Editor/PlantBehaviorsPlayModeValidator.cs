#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEditor;
using UnityEngine;

namespace FantasyTD.CI
{
    [InitializeOnLoad]
    internal static class PlantBehaviorsPlayModeValidator
    {
        private const string ActiveKey = "FantasyTD.T0004.Active";
        private const string PhaseKey = "FantasyTD.T0004.Phase";
        private const string StartedKey = "FantasyTD.T0004.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.T0004.ExitCode";
        private const string PlayModeEnteredKey = "FantasyTD.T0004.PlayModeEntered";
        private const string ReportPath = "Artifacts/UnityExecutor/plant-behaviors.txt";
        private const float TimeoutSeconds = 300f;

        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> Facts = new List<string>();
        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> RuntimeWarnings = new List<string>();

        private static bool _logSubscribed;
        private static int _warmupFrames;
        private static GameSession _session;
        private static IEnumerator _flow;

        static PlantBehaviorsPlayModeValidator()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            if (SessionState.GetBool(ActiveKey, false))
                SubscribeLog();
        }

        internal static void Start()
        {
            ResetRuntimeState();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(PlayModeEnteredKey, false);
            SessionState.SetString(PhaseKey, "enter-play");
            SessionState.SetString(StartedKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            SessionState.SetInt(ExitCodeKey, 1);
            SubscribeLog();

            try
            {
                MapAEditorSetup.OpenMap();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex)
            {
                Errors.Add("startException=" + ex.GetType().Name + ": " + ex.Message);
                WriteReport(false);
                SessionState.SetBool(ActiveKey, false);
                UnsubscribeLog();
                EditorApplication.Exit(1);
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
                return;

            if (TimedOut())
            {
                FailAndLeavePlayMode("Validator timeout exceeded " + F(TimeoutSeconds) + " seconds.");
                return;
            }

            string phase = SessionState.GetString(PhaseKey, "enter-play");
            if (phase == "exit-editor" && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int exitCode = SessionState.GetInt(ExitCodeKey, 1);
                SessionState.SetBool(ActiveKey, false);
                UnsubscribeLog();
                EditorApplication.Exit(exitCode);
                return;
            }

            if (!EditorApplication.isPlaying)
                return;

            SessionState.SetBool(PlayModeEnteredKey, true);
            SubscribeLog();

            if (_session == null)
            {
                _warmupFrames++;
                if (_warmupFrames < 5)
                    return;

                _session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if (_session == null || _session.Economy == null || _session.Waves == null)
                    return;

                AddFact("waveFlowStarted", _session.Waves.Running);
                Check(!_session.Waves.Running, "waveDirector unexpectedly running at T0004 start");
                _flow = RunScenarios();
                SessionState.SetString(PhaseKey, "validating");
            }

            try
            {
                if (_flow != null && !_flow.MoveNext())
                    Complete();
            }
            catch (Exception ex)
            {
                FailAndLeavePlayMode("validatorException=" + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static IEnumerator RunScenarios()
        {
            IEnumerator step;

            step = ValidateSeedShooter();
            while (step.MoveNext()) yield return null;

            step = ValidateEssenceFlower();
            while (step.MoveNext()) yield return null;

            step = ValidateOakGuardianNormal();
            while (step.MoveNext()) yield return null;

            step = ValidateOakGuardianBoss();
            while (step.MoveNext()) yield return null;

            step = ValidateFrostBudNormal();
            while (step.MoveNext()) yield return null;

            step = ValidateFrostBudBoss();
            while (step.MoveNext()) yield return null;
        }

        private static IEnumerator ValidateSeedShooter()
        {
            int startErrors = Errors.Count;
            Time.timeScale = 1f;
            DestroyScenarioObjects();
            yield return null;
            yield return null;

            AddFact("seed.rangeRU", 3.2f);
            AddFact("seed.expectedRawDamage", 22f);
            AddFact("seed.expectedProjectileSpeed", 7f);
            AddFact("seed.targetPriority", "First");

            Vector2 plantPos = PadPosition("A1");
            EnemyUnit front = SpawnFixtureEnemy(EnemyKind.Wanderer, 4.5f, 0f, "T0004_Seed_Front");
            EnemyUnit back = SpawnFixtureEnemy(EnemyKind.Wanderer, 1.5f, 0f, "T0004_Seed_Back");
            EnemyUnit outside = SpawnFixtureEnemy(EnemyKind.Wanderer, 10f, 0f, "T0004_Seed_Outside");

            float frontBefore = ReadEnemyFloat(front, "_hp");
            float backBefore = ReadEnemyFloat(back, "_hp");
            float outsideBefore = ReadEnemyFloat(outside, "_hp");
            float frontDistance = Vector2.Distance(plantPos, front.transform.position);
            float backDistance = Vector2.Distance(plantPos, back.transform.position);
            float outsideDistance = Vector2.Distance(plantPos, outside.transform.position);

            AddFact("seed.targeting.frontProgress", front.ProgressDistance);
            AddFact("seed.targeting.backProgress", back.ProgressDistance);
            AddFact("seed.targeting.outsideProgress", outside.ProgressDistance);
            AddFact("seed.targeting.frontDistanceRU", frontDistance);
            AddFact("seed.targeting.backDistanceRU", backDistance);
            AddFact("seed.targeting.outsideDistanceRU", outsideDistance);

            SeedShooterPlant plant = SpawnPlant<SeedShooterPlant>(PlantKind.SeedShooter, plantPos);
            SeedProjectile projectile = null;
            float deadline = Time.time + 1.5f;
            while (projectile == null && Time.time < deadline)
            {
                projectile = FindAnyProjectile();
                if (projectile == null) yield return null;
            }

            if (projectile == null)
            {
                Errors.Add("seed.targeting.projectileNotSpawned");
                AddFact("seed.targeting.result", "FAIL");
                DestroyScenarioObjects();
                yield return null;
                yield break;
            }

            EnemyUnit projectileTarget = ReadProjectileTarget(projectile);
            float rawDamage = ReadProjectileFloat(projectile, "_damage");
            float projectileSpeed = ReadProjectileFloat(projectile, "_speed");
            float shotDistance = projectileTarget != null
                ? Vector2.Distance(projectile.transform.position, projectileTarget.transform.position)
                : -1f;

            AddFact("seed.targeting.projectileTarget", projectileTarget != null ? projectileTarget.gameObject.name : "null");
            AddFact("seed.targeting.targetIsFront", projectileTarget == front);
            AddFact("seed.targeting.outsideHighestProgressExcluded", projectileTarget == front && outside.ProgressDistance > front.ProgressDistance);
            AddFact("seed.projectile.rawDamage", rawDamage);
            AddFact("seed.projectile.speedRUPerSec", projectileSpeed);
            AddFact("seed.projectile.shotDistanceRU", shotDistance);

            Check(frontDistance <= 3.2f + 0.001f, "seed front target fixture is not in range");
            Check(backDistance <= 3.2f + 0.001f, "seed back target fixture is not in range");
            Check(outsideDistance > 3.2f, "seed outside target fixture is not outside range");
            Check(projectileTarget == front, "seed First/range targeting did not choose highest-progress in-range enemy");
            CheckApprox(rawDamage, 22f, 0.01f, "seed raw projectile damage");
            CheckApprox(projectileSpeed, 7f, 0.01f, "seed projectile speed");

            UnityEngine.Object.Destroy(plant.gameObject);

            deadline = Time.time + 2.0f;
            while (Mathf.Abs(ReadEnemyFloat(front, "_hp") - frontBefore) < 0.01f && Time.time < deadline)
                yield return null;

            float frontAfter = ReadEnemyFloat(front, "_hp");
            float backAfter = ReadEnemyFloat(back, "_hp");
            float outsideAfter = ReadEnemyFloat(outside, "_hp");
            float finalDamage = frontBefore - frontAfter;
            bool reached = finalDamage > 0.01f;

            AddFact("seed.projectile.reachedTarget", reached);
            AddFact("seed.targeting.frontHpBefore", frontBefore);
            AddFact("seed.targeting.frontHpAfter", frontAfter);
            AddFact("seed.targeting.backHpBefore", backBefore);
            AddFact("seed.targeting.backHpAfter", backAfter);
            AddFact("seed.targeting.outsideHpBefore", outsideBefore);
            AddFact("seed.targeting.outsideHpAfter", outsideAfter);
            AddFact("seed.targeting.finalDamage", finalDamage);

            Check(reached, "seed projectile did not reach/damage intended target");
            CheckApprox(finalDamage, 22f, 0.05f, "seed unarmored final damage");
            CheckApprox(backAfter, backBefore, 0.01f, "seed back enemy should remain undamaged");
            CheckApprox(outsideAfter, outsideBefore, 0.01f, "seed out-of-range enemy should remain undamaged");

            DestroyScenarioObjects();
            yield return null;
            yield return null;

            EnemyUnit armored = SpawnFixtureEnemy(EnemyKind.Armored, 3f, 0f, "T0004_Seed_Armored");
            float armoredBefore = ReadEnemyFloat(armored, "_hp");
            float resistance = ReadEnemyFloat(armored, "_physicalResistance");
            SeedShooterPlant armoredPlant = SpawnPlant<SeedShooterPlant>(PlantKind.SeedShooter, PadPosition("A1"));

            projectile = null;
            deadline = Time.time + 1.5f;
            while (projectile == null && Time.time < deadline)
            {
                projectile = FindAnyProjectile();
                if (projectile == null) yield return null;
            }

            if (projectile == null)
            {
                Errors.Add("seed.resistance.projectileNotSpawned");
                AddFact("seed.result", "FAIL");
                DestroyScenarioObjects();
                yield return null;
                yield break;
            }

            rawDamage = ReadProjectileFloat(projectile, "_damage");
            projectileSpeed = ReadProjectileFloat(projectile, "_speed");
            float armoredDistance = Vector2.Distance(armoredPlant.transform.position, armored.transform.position);
            UnityEngine.Object.Destroy(armoredPlant.gameObject);

            deadline = Time.time + 2f;
            while (Mathf.Abs(ReadEnemyFloat(armored, "_hp") - armoredBefore) < 0.01f && Time.time < deadline)
                yield return null;

            float armoredAfter = ReadEnemyFloat(armored, "_hp");
            float armoredFinalDamage = armoredBefore - armoredAfter;
            float expectedFinalDamage = rawDamage * (1f - resistance);

            AddFact("seed.resistance.target", "Armored");
            AddFact("seed.resistance.rangeRU", 3.2f);
            AddFact("seed.resistance.distanceRU", armoredDistance);
            AddFact("seed.resistance.hpBefore", armoredBefore);
            AddFact("seed.resistance.hpAfter", armoredAfter);
            AddFact("seed.resistance.rawDamage", rawDamage);
            AddFact("seed.resistance.physicalResistance", resistance);
            AddFact("seed.resistance.expectedFinalDamage", expectedFinalDamage);
            AddFact("seed.resistance.finalDamage", armoredFinalDamage);
            AddFact("seed.resistance.damageReduced", armoredFinalDamage < rawDamage);

            Check(armoredDistance <= 3.2f + 0.001f, "seed armored fixture outside range");
            CheckApprox(armoredBefore, 260f, 0.05f, "armored initial HP");
            CheckApprox(resistance, 0.15f, 0.001f, "armored physical resistance");
            CheckApprox(rawDamage, 22f, 0.01f, "seed armored raw damage");
            CheckApprox(armoredFinalDamage, 18.7f, 0.05f, "seed armored final damage");
            Check(armoredFinalDamage < rawDamage, "physical resistance did not reduce seed damage");

            AddFact("seed.result", Errors.Count == startErrors ? "PASS" : "FAIL");

            DestroyScenarioObjects();
            yield return null;
            yield return null;
        }

        private static IEnumerator ValidateEssenceFlower()
        {
            int startErrors = Errors.Count;
            Time.timeScale = 1f;
            DestroyScenarioObjects();
            yield return null;
            yield return null;

            int before = _session.Economy.Essence;
            EssenceFlowerPlant flower = SpawnPlant<EssenceFlowerPlant>(PlantKind.EssenceFlower, PadPosition("A2"));
            float startGameTime = Time.time;
            Time.timeScale = 8f;

            int afterFirst = before;
            float firstElapsed = -1f;
            float deadline = Time.time + 25f;
            while (_session.Economy.Essence < before + 20 && Time.time < deadline)
                yield return null;

            afterFirst = _session.Economy.Essence;
            firstElapsed = Time.time - startGameTime;

            deadline = startGameTime + 45f;
            while (_session.Economy.Essence < before + 40 && Time.time < deadline)
                yield return null;

            int afterSecond = _session.Economy.Essence;
            float secondElapsed = Time.time - startGameTime;
            float secondInterval = secondElapsed - firstElapsed;
            Time.timeScale = 1f;

            AddFact("flower.incomeAmount", 20);
            AddFact("flower.incomeIntervalConfiguredGameSec", 20f);
            AddFact("flower.testTimeScale", 8f);
            AddFact("flower.essenceBefore", before);
            AddFact("flower.essenceAfterFirst", afterFirst);
            AddFact("flower.essenceAfterSecond", afterSecond);
            AddFact("flower.firstIncomeElapsedGameSec", firstElapsed);
            AddFact("flower.secondIncomeElapsedGameSec", secondElapsed);
            AddFact("flower.secondIntervalGameSec", secondInterval);

            Check(afterFirst == before + 20, "flower first interval did not add exactly 20 Essence");
            Check(afterSecond == before + 40, "flower second interval did not add exactly another 20 Essence");
            CheckApprox(firstElapsed, 20f, 0.5f, "flower first income interval");
            CheckApprox(secondInterval, 20f, 0.5f, "flower second income interval");

            AddFact("flower.result", Errors.Count == startErrors ? "PASS" : "FAIL");

            if (flower != null) UnityEngine.Object.Destroy(flower.gameObject);
            yield return null;
            yield return null;
        }

        private static IEnumerator ValidateOakGuardianNormal()
        {
            int startErrors = Errors.Count;
            Time.timeScale = 1f;
            DestroyScenarioObjects();
            yield return null;
            yield return null;

            EnemyUnit enemy = SpawnFixtureEnemy(EnemyKind.Wanderer, 3f, null, "T0004_Oak_Normal");
            float hpBefore = ReadEnemyFloat(enemy, "_hp");
            float speedBefore = 0f;

            float t0 = Time.time;
            float p0 = enemy.ProgressDistance;
            while (Time.time - t0 < 0.35f) yield return null;
            speedBefore = (enemy.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            Vector2 plantPos = PadPosition("A1");
            float initialDistance = Vector2.Distance(plantPos, enemy.transform.position);
            OakGuardianPlant plant = SpawnPlant<OakGuardianPlant>(PlantKind.OakGuardian, plantPos);

            float deadline = Time.time + 2.5f;
            while (Mathf.Abs(ReadEnemyFloat(enemy, "_hp") - hpBefore) < 0.01f && Time.time < deadline)
                yield return null;

            float rootObservedStart = Time.time;
            float rootUntil = ReadEnemyFloat(enemy, "_rootUntil");
            float rootDurationObserved = rootUntil - rootObservedStart;
            float hpAfter = ReadEnemyFloat(enemy, "_hp");
            float finalDamage = hpBefore - hpAfter;
            UnityEngine.Object.Destroy(plant.gameObject);

            t0 = Time.time;
            p0 = enemy.ProgressDistance;
            while (Time.time - t0 < 0.30f && Time.time < rootUntil - 0.03f) yield return null;
            float speedDuring = (enemy.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            while (Time.time < rootUntil + 0.08f) yield return null;
            t0 = Time.time;
            p0 = enemy.ProgressDistance;
            while (Time.time - t0 < 0.35f) yield return null;
            float speedAfter = (enemy.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            AddFact("oak.normal.rangeRU", 2.4f);
            AddFact("oak.normal.distanceAtPlantRU", initialDistance);
            AddFact("oak.normal.hpBefore", hpBefore);
            AddFact("oak.normal.hpAfter", hpAfter);
            AddFact("oak.normal.rawDamage", 8f);
            AddFact("oak.normal.finalDamage", finalDamage);
            AddFact("oak.normal.rootDurationConfiguredSec", 1.75f);
            AddFact("oak.normal.rootDurationObservedSec", rootDurationObserved);
            AddFact("oak.normal.speedBefore", speedBefore);
            AddFact("oak.normal.speedDuringRoot", speedDuring);
            AddFact("oak.normal.speedAfterRoot", speedAfter);
            AddFact("oak.normal.movementResumed", speedAfter > 0.6f);

            Check(initialDistance <= 2.4f + 0.001f, "oak normal enemy was outside range at plant creation");
            CheckApprox(hpBefore, 170f, 0.05f, "oak normal initial HP");
            CheckApprox(finalDamage, 8f, 0.05f, "oak normal damage");
            CheckApprox(rootDurationObserved, 1.75f, 0.12f, "oak normal root duration");
            CheckApprox(speedBefore, 0.8f, 0.08f, "oak normal speed before root");
            Check(Mathf.Abs(speedDuring) <= 0.05f, "oak normal enemy moved during root");
            CheckApprox(speedAfter, 0.8f, 0.08f, "oak normal speed after root");

            AddFact("oak.normal.result", Errors.Count == startErrors ? "PASS" : "FAIL");

            DestroyScenarioObjects();
            yield return null;
            yield return null;
        }

        private static IEnumerator ValidateOakGuardianBoss()
        {
            int startErrors = Errors.Count;
            Time.timeScale = 1f;
            DestroyScenarioObjects();
            yield return null;
            yield return null;

            EnemyUnit boss = SpawnFixtureEnemy(EnemyKind.Boss, 3f, null, "T0004_Oak_Boss");
            float hpBefore = ReadEnemyFloat(boss, "_hp");

            float t0 = Time.time;
            float p0 = boss.ProgressDistance;
            while (Time.time - t0 < 0.35f) yield return null;
            float speedBefore = (boss.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            Vector2 plantPos = PadPosition("A1");
            float initialDistance = Vector2.Distance(plantPos, boss.transform.position);
            OakGuardianPlant plant = SpawnPlant<OakGuardianPlant>(PlantKind.OakGuardian, plantPos);

            float deadline = Time.time + 2.5f;
            while (Mathf.Abs(ReadEnemyFloat(boss, "_hp") - hpBefore) < 0.01f && Time.time < deadline)
                yield return null;

            float rootObservedStart = Time.time;
            float rootUntil = ReadEnemyFloat(boss, "_rootUntil");
            float rootDurationObserved = rootUntil - rootObservedStart;
            float hpAfter = ReadEnemyFloat(boss, "_hp");
            float finalDamage = hpBefore - hpAfter;
            UnityEngine.Object.Destroy(plant.gameObject);

            t0 = Time.time;
            p0 = boss.ProgressDistance;
            while (Time.time - t0 < 0.25f && Time.time < rootUntil - 0.03f) yield return null;
            float speedDuring = (boss.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            while (Time.time < rootUntil + 0.08f) yield return null;
            t0 = Time.time;
            p0 = boss.ProgressDistance;
            while (Time.time - t0 < 0.35f) yield return null;
            float speedAfter = (boss.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            AddFact("oak.boss.rangeRU", 2.4f);
            AddFact("oak.boss.distanceAtPlantRU", initialDistance);
            AddFact("oak.boss.hpBefore", hpBefore);
            AddFact("oak.boss.hpAfter", hpAfter);
            AddFact("oak.boss.rawDamage", 8f);
            AddFact("oak.boss.finalDamage", finalDamage);
            AddFact("oak.boss.rootReductionMultiplier", 0.35f);
            AddFact("oak.boss.rootDurationExpectedSec", 1.75f * 0.35f);
            AddFact("oak.boss.rootDurationObservedSec", rootDurationObserved);
            AddFact("oak.boss.speedBefore", speedBefore);
            AddFact("oak.boss.speedDuringRoot", speedDuring);
            AddFact("oak.boss.speedAfterRoot", speedAfter);
            AddFact("oak.boss.controlShorterThanNormal", rootDurationObserved < 1.0f);

            Check(initialDistance <= 2.4f + 0.001f, "oak boss was outside range at plant creation");
            CheckApprox(hpBefore, 2200f, 0.05f, "oak boss initial HP");
            CheckApprox(finalDamage, 8f, 0.05f, "oak boss damage");
            CheckApprox(rootDurationObserved, 0.6125f, 0.10f, "oak boss root duration");
            CheckApprox(speedBefore, 0.30f, 0.05f, "oak boss speed before root");
            Check(Mathf.Abs(speedDuring) <= 0.04f, "oak boss moved during root");
            CheckApprox(speedAfter, 0.30f, 0.05f, "oak boss speed after root");

            AddFact("oak.boss.result", Errors.Count == startErrors ? "PASS" : "FAIL");

            DestroyScenarioObjects();
            yield return null;
            yield return null;
        }

        private static IEnumerator ValidateFrostBudNormal()
        {
            int startErrors = Errors.Count;
            Time.timeScale = 1f;
            DestroyScenarioObjects();
            yield return null;
            yield return null;

            EnemyUnit enemy = SpawnFixtureEnemy(EnemyKind.Wanderer, 3f, null, "T0004_Frost_Normal");
            float hpBefore = ReadEnemyFloat(enemy, "_hp");

            float t0 = Time.time;
            float p0 = enemy.ProgressDistance;
            while (Time.time - t0 < 0.35f) yield return null;
            float speedBefore = (enemy.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            Vector2 plantPos = PadPosition("A1");
            float shotDistanceFixture = Vector2.Distance(plantPos, enemy.transform.position);
            FrostBudPlant plant = SpawnPlant<FrostBudPlant>(PlantKind.FrostBud, plantPos);

            SeedProjectile projectile = null;
            float deadline = Time.time + 1.5f;
            while (projectile == null && Time.time < deadline)
            {
                projectile = FindAnyProjectile();
                if (projectile == null) yield return null;
            }

            if (projectile == null)
            {
                Errors.Add("frost.normal.projectileNotSpawned");
                AddFact("frost.normal.result", "FAIL");
                DestroyScenarioObjects();
                yield return null;
                yield break;
            }

            float rawDamage = ReadProjectileFloat(projectile, "_damage");
            float projectileSpeed = ReadProjectileFloat(projectile, "_speed");
            float requestedSlow = ReadProjectileFloat(projectile, "_slow");
            float requestedDuration = ReadProjectileFloat(projectile, "_slowDuration");
            UnityEngine.Object.Destroy(plant.gameObject);

            deadline = Time.time + 2f;
            while (Mathf.Abs(ReadEnemyFloat(enemy, "_hp") - hpBefore) < 0.01f && Time.time < deadline)
                yield return null;

            float slowObservedStart = Time.time;
            float slowStrength = ReadEnemyFloat(enemy, "_slowStrength");
            float slowUntil = ReadEnemyFloat(enemy, "_slowUntil");
            float slowDurationObserved = slowUntil - slowObservedStart;
            float hpAfter = ReadEnemyFloat(enemy, "_hp");
            float finalDamage = hpBefore - hpAfter;

            t0 = Time.time;
            p0 = enemy.ProgressDistance;
            while (Time.time - t0 < 0.50f && Time.time < slowUntil - 0.05f) yield return null;
            float speedDuring = (enemy.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            while (Time.time < slowUntil + 0.08f) yield return null;
            bool slowExpired = Time.time >= slowUntil;
            t0 = Time.time;
            p0 = enemy.ProgressDistance;
            while (Time.time - t0 < 0.40f) yield return null;
            float speedAfter = (enemy.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            AddFact("frost.normal.rangeRU", 3.2f);
            AddFact("frost.normal.distanceAtShotFixtureRU", shotDistanceFixture);
            AddFact("frost.normal.hpBefore", hpBefore);
            AddFact("frost.normal.hpAfter", hpAfter);
            AddFact("frost.normal.rawDamage", rawDamage);
            AddFact("frost.normal.finalDamage", finalDamage);
            AddFact("frost.normal.projectileSpeedRUPerSec", projectileSpeed);
            AddFact("frost.normal.requestedSlowStrength", requestedSlow);
            AddFact("frost.normal.appliedSlowStrength", slowStrength);
            AddFact("frost.normal.requestedSlowDurationSec", requestedDuration);
            AddFact("frost.normal.slowDurationObservedSec", slowDurationObserved);
            AddFact("frost.normal.speedBefore", speedBefore);
            AddFact("frost.normal.speedDuringSlow", speedDuring);
            AddFact("frost.normal.expectedSpeedDuringSlow", 0.8f * (1f - 0.35f));
            AddFact("frost.normal.slowExpired", slowExpired);
            AddFact("frost.normal.speedAfterSlow", speedAfter);

            Check(shotDistanceFixture <= 3.2f + 0.001f, "frost normal fixture outside range");
            CheckApprox(hpBefore, 170f, 0.05f, "frost normal initial HP");
            CheckApprox(rawDamage, 18f, 0.01f, "frost normal raw damage");
            CheckApprox(finalDamage, 18f, 0.05f, "frost normal final damage");
            CheckApprox(requestedSlow, 0.35f, 0.001f, "frost normal requested slow");
            CheckApprox(slowStrength, 0.35f, 0.001f, "frost normal applied slow");
            CheckApprox(slowDurationObserved, 2.5f, 0.15f, "frost normal slow duration");
            CheckApprox(speedBefore, 0.8f, 0.08f, "frost normal speed before");
            CheckApprox(speedDuring, 0.52f, 0.08f, "frost normal speed during");
            Check(slowExpired, "frost normal slow did not expire");
            CheckApprox(speedAfter, 0.8f, 0.08f, "frost normal speed after");

            AddFact("frost.normal.result", Errors.Count == startErrors ? "PASS" : "FAIL");

            DestroyScenarioObjects();
            yield return null;
            yield return null;
        }

        private static IEnumerator ValidateFrostBudBoss()
        {
            int startErrors = Errors.Count;
            Time.timeScale = 1f;
            DestroyScenarioObjects();
            yield return null;
            yield return null;

            EnemyUnit boss = SpawnFixtureEnemy(EnemyKind.Boss, 3f, null, "T0004_Frost_Boss");
            float hpBefore = ReadEnemyFloat(boss, "_hp");
            float physicalResistance = ReadEnemyFloat(boss, "_physicalResistance");

            float t0 = Time.time;
            float p0 = boss.ProgressDistance;
            while (Time.time - t0 < 0.35f) yield return null;
            float speedBefore = (boss.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            Vector2 plantPos = PadPosition("A1");
            float shotDistanceFixture = Vector2.Distance(plantPos, boss.transform.position);
            FrostBudPlant plant = SpawnPlant<FrostBudPlant>(PlantKind.FrostBud, plantPos);

            SeedProjectile projectile = null;
            float deadline = Time.time + 1.5f;
            while (projectile == null && Time.time < deadline)
            {
                projectile = FindAnyProjectile();
                if (projectile == null) yield return null;
            }

            if (projectile == null)
            {
                Errors.Add("frost.boss.projectileNotSpawned");
                AddFact("frost.boss.result", "FAIL");
                DestroyScenarioObjects();
                yield return null;
                yield break;
            }

            float rawDamage = ReadProjectileFloat(projectile, "_damage");
            float requestedSlow = ReadProjectileFloat(projectile, "_slow");
            float requestedDuration = ReadProjectileFloat(projectile, "_slowDuration");
            UnityEngine.Object.Destroy(plant.gameObject);

            deadline = Time.time + 2f;
            while (Mathf.Abs(ReadEnemyFloat(boss, "_hp") - hpBefore) < 0.01f && Time.time < deadline)
                yield return null;

            float slowObservedStart = Time.time;
            float slowStrength = ReadEnemyFloat(boss, "_slowStrength");
            float slowUntil = ReadEnemyFloat(boss, "_slowUntil");
            float slowDurationObserved = slowUntil - slowObservedStart;
            float hpAfter = ReadEnemyFloat(boss, "_hp");
            float finalDamage = hpBefore - hpAfter;

            t0 = Time.time;
            p0 = boss.ProgressDistance;
            while (Time.time - t0 < 0.50f && Time.time < slowUntil - 0.05f) yield return null;
            float speedDuring = (boss.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            while (Time.time < slowUntil + 0.08f) yield return null;
            bool slowExpired = Time.time >= slowUntil;
            t0 = Time.time;
            p0 = boss.ProgressDistance;
            while (Time.time - t0 < 0.40f) yield return null;
            float speedAfter = (boss.ProgressDistance - p0) / Mathf.Max(0.001f, Time.time - t0);

            AddFact("frost.boss.rangeRU", 3.2f);
            AddFact("frost.boss.distanceAtShotFixtureRU", shotDistanceFixture);
            AddFact("frost.boss.hpBefore", hpBefore);
            AddFact("frost.boss.hpAfter", hpAfter);
            AddFact("frost.boss.rawDamage", rawDamage);
            AddFact("frost.boss.finalDamage", finalDamage);
            AddFact("frost.boss.physicalResistance", physicalResistance);
            AddFact("frost.boss.damageTypeIgnoresPhysicalResistance", Mathf.Abs(finalDamage - rawDamage) <= 0.05f);
            AddFact("frost.boss.requestedSlowStrength", requestedSlow);
            AddFact("frost.boss.appliedSlowStrength", slowStrength);
            AddFact("frost.boss.slowCap", 0.20f);
            AddFact("frost.boss.requestedSlowDurationSec", requestedDuration);
            AddFact("frost.boss.slowDurationObservedSec", slowDurationObserved);
            AddFact("frost.boss.speedBefore", speedBefore);
            AddFact("frost.boss.speedDuringSlow", speedDuring);
            AddFact("frost.boss.expectedSpeedDuringSlow", 0.30f * (1f - 0.20f));
            AddFact("frost.boss.slowExpired", slowExpired);
            AddFact("frost.boss.speedAfterSlow", speedAfter);
            AddFact("frost.boss.slowWeakerThanNormal", slowStrength < 0.35f);

            Check(shotDistanceFixture <= 3.2f + 0.001f, "frost boss fixture outside range");
            CheckApprox(hpBefore, 2200f, 0.05f, "frost boss initial HP");
            CheckApprox(rawDamage, 18f, 0.01f, "frost boss raw damage");
            CheckApprox(finalDamage, 18f, 0.05f, "frost boss final damage");
            CheckApprox(physicalResistance, 0.20f, 0.001f, "boss physical resistance");
            CheckApprox(requestedSlow, 0.35f, 0.001f, "frost boss requested slow");
            CheckApprox(slowStrength, 0.20f, 0.001f, "frost boss applied slow");
            CheckApprox(slowDurationObserved, 2.5f, 0.15f, "frost boss slow duration");
            CheckApprox(speedBefore, 0.30f, 0.05f, "frost boss speed before");
            CheckApprox(speedDuring, 0.24f, 0.05f, "frost boss speed during");
            Check(slowExpired, "frost boss slow did not expire");
            CheckApprox(speedAfter, 0.30f, 0.05f, "frost boss speed after");
            Check(slowStrength < 0.35f, "boss slow was not weaker than normal slow");

            AddFact("frost.boss.result", Errors.Count == startErrors ? "PASS" : "FAIL");

            DestroyScenarioObjects();
            yield return null;
            yield return null;
        }

        private static EnemyUnit SpawnFixtureEnemy(
            EnemyKind kind,
            float progress,
            float? speedOverride,
            string name)
        {
            EnemyUnit enemy = _session.SpawnEnemy(kind);
            enemy.gameObject.name = name;
            int segment;
            Vector2 position = RoutePosition(progress, out segment);
            enemy.transform.position = new Vector3(position.x, position.y, -0.20f);
            SetEnemyInt(enemy, "_segmentIndex", segment);
            SetEnemyFloat(enemy, "_progressDistance", progress);
            if (speedOverride.HasValue)
                SetEnemyFloat(enemy, "_speed", speedOverride.Value);
            return enemy;
        }

        private static Vector2 PadPosition(string id)
        {
            foreach (PlantingSpotDefinition spot in MapAData.PlantingSpots)
                if (spot.Id == id)
                    return spot.Position;
            throw new InvalidOperationException("Required authored planting pad missing: " + id);
        }

        private static Vector2 RoutePosition(float progress, out int segment)
        {
            float remaining = progress;
            for (segment = 0; segment < MapAData.Waypoints.Length - 1; segment++)
            {
                Vector2 start = MapAData.Waypoints[segment];
                Vector2 end = MapAData.Waypoints[segment + 1];
                float length = Vector2.Distance(start, end);
                if (remaining <= length)
                    return Vector2.Lerp(start, end, remaining / Mathf.Max(0.001f, length));
                remaining -= length;
            }
            throw new ArgumentOutOfRangeException(nameof(progress), "Fixture progress exceeds the production route.");
        }

        private static T SpawnPlant<T>(PlantKind kind, Vector2 position) where T : PlantBehaviour
        {
            Check(Array.Exists(MapAData.PlantingSpots, s => Vector2.Distance(s.Position, position) <= 0.001f),
                "component-test tower must be located on an authored planting pad");
            GameObject go = FantasyVisualFactory.CreatePlant(kind, position);
            T behavior = go.AddComponent<T>();
            behavior.Initialize(_session);
            return behavior;
        }

        private static SeedProjectile FindAnyProjectile()
        {
            SeedProjectile[] projectiles = UnityEngine.Object.FindObjectsByType<SeedProjectile>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            return projectiles.Length > 0 ? projectiles[0] : null;
        }

        private static void DestroyScenarioObjects()
        {
            Time.timeScale = 1f;

            SeedProjectile[] projectiles = UnityEngine.Object.FindObjectsByType<SeedProjectile>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < projectiles.Length; i++)
                if (projectiles[i] != null) UnityEngine.Object.Destroy(projectiles[i].gameObject);

            PlantBehaviour[] plants = UnityEngine.Object.FindObjectsByType<PlantBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < plants.Length; i++)
                if (plants[i] != null) UnityEngine.Object.Destroy(plants[i].gameObject);

            EnemyUnit[] enemies = UnityEngine.Object.FindObjectsByType<EnemyUnit>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
                if (enemies[i] != null) UnityEngine.Object.Destroy(enemies[i].gameObject);
        }

        private static float ReadEnemyFloat(EnemyUnit enemy, string fieldName)
        {
            return (float)RequireField(typeof(EnemyUnit), fieldName).GetValue(enemy);
        }

        private static void SetEnemyFloat(EnemyUnit enemy, string fieldName, float value)
        {
            RequireField(typeof(EnemyUnit), fieldName).SetValue(enemy, value);
        }

        private static void SetEnemyInt(EnemyUnit enemy, string fieldName, int value)
        {
            RequireField(typeof(EnemyUnit), fieldName).SetValue(enemy, value);
        }

        private static EnemyUnit ReadProjectileTarget(SeedProjectile projectile)
        {
            return (EnemyUnit)RequireField(typeof(SeedProjectile), "_target").GetValue(projectile);
        }

        private static float ReadProjectileFloat(SeedProjectile projectile, string fieldName)
        {
            return (float)RequireField(typeof(SeedProjectile), fieldName).GetValue(projectile);
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(type.FullName, fieldName);
            return field;
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
                Errors.Add(message);
        }

        private static void CheckApprox(float actual, float expected, float tolerance, string label)
        {
            if (Mathf.Abs(actual - expected) > tolerance)
                Errors.Add(label + " expected=" + F(expected) + " actual=" + F(actual) + " tolerance=" + F(tolerance));
        }

        private static void AddFact(string key, object value)
        {
            if (value is float)
                Facts.Add(key + "=" + F((float)value));
            else if (value is double)
                Facts.Add(key + "=" + ((double)value).ToString("0.###", CultureInfo.InvariantCulture));
            else
                Facts.Add(key + "=" + (value == null ? "null" : value.ToString()));
        }

        private static void Complete()
        {
            Time.timeScale = 1f;
            DestroyScenarioObjects();
            bool success = Errors.Count == 0;
            WriteReport(success);
            LeavePlayMode(success ? 0 : 1);
        }

        private static void FailAndLeavePlayMode(string message)
        {
            Time.timeScale = 1f;
            if (!string.IsNullOrEmpty(message))
                Errors.Add(message);
            WriteReport(false);
            LeavePlayMode(1);
        }

        private static void LeavePlayMode(int exitCode)
        {
            SessionState.SetInt(ExitCodeKey, exitCode);
            SessionState.SetString(PhaseKey, "exit-editor");
            UnsubscribeLog();

            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.ExitPlaymode();
            else
            {
                SessionState.SetBool(ActiveKey, false);
                EditorApplication.Exit(exitCode);
            }
        }

        private static void WriteReport(bool success)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");

            List<string> lines = new List<string>();
            lines.Add("DYRYGENT UNITY EXECUTOR T0004 FOUR PLANT BEHAVIORS");
            lines.Add("timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            lines.Add("unityVersion=" + Application.unityVersion);
            lines.Add("status=" + (success ? "PASS" : "FAIL"));
            lines.Add("playModeEntered=" + SessionState.GetBool(PlayModeEnteredKey, false));
            lines.Add("androidBuildRun=False");
            lines.Add("waveFlowStarted=False");
            lines.AddRange(Facts);
            lines.Add("runtimeWarningCount=" + RuntimeWarnings.Count);
            for (int i = 0; i < RuntimeWarnings.Count; i++)
                lines.Add("runtimeWarning." + (i + 1) + "=" + Sanitize(RuntimeWarnings[i]));
            lines.Add("validationErrorCount=" + Errors.Count);
            for (int i = 0; i < Errors.Count; i++)
                lines.Add("validationError." + (i + 1) + "=" + Sanitize(Errors[i]));

            string report = string.Join("\n", lines) + "\n";
            File.WriteAllText(ReportPath, report);
            Debug.Log(report);
        }

        private static void SubscribeLog()
        {
            if (_logSubscribed) return;
            Application.logMessageReceived += OnLog;
            _logSubscribed = true;
        }

        private static void UnsubscribeLog()
        {
            if (!_logSubscribed) return;
            Application.logMessageReceived -= OnLog;
            _logSubscribed = false;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Errors.Add("console." + type + "=" + condition);
            else if (type == LogType.Warning)
                RuntimeWarnings.Add(condition);
        }

        private static bool TimedOut()
        {
            string started = SessionState.GetString(StartedKey, string.Empty);
            DateTime parsed;
            if (!DateTime.TryParse(started, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
                return false;
            return (DateTime.UtcNow - parsed.ToUniversalTime()).TotalSeconds > TimeoutSeconds;
        }

        private static void ResetRuntimeState()
        {
            Facts.Clear();
            Errors.Clear();
            RuntimeWarnings.Clear();
            _warmupFrames = 0;
            _session = null;
            _flow = null;
        }

        private static string F(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string Sanitize(string value)
        {
            return (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        }
    }
}
#endif
