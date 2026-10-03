#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    [InitializeOnLoad]
    internal static class BossEndStatesPlayModeValidator
    {
        private const string ActiveKey = "FantasyTD.T0006.Active";
        private const string PhaseKey = "FantasyTD.T0006.Phase";
        private const string StartedKey = "FantasyTD.T0006.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.T0006.ExitCode";
        private const string PlayModeEnteredKey = "FantasyTD.T0006.PlayModeEntered";
        private const string ReportPath = "Artifacts/UnityExecutor/boss-end-states.txt";
        private const float TimeoutSeconds = 600f;

        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> Facts = new List<string>();
        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> RuntimeWarnings = new List<string>();
        private static readonly HashSet<int> Wave6SeenIds = new HashSet<int>();
        private static readonly List<EnemyKind> Wave6Observed = new List<EnemyKind>();
        private static readonly Dictionary<int, float> DefeatLastProgress = new Dictionary<int, float>();
        private static readonly Dictionary<int, EnemyKind> DefeatKinds = new Dictionary<int, EnemyKind>();
        private static HashSet<int> DefeatPreviousIds = new HashSet<int>();
        private static readonly List<string> DefeatBaseTransitions = new List<string>();
        private static readonly List<int> DefeatWavesStarted = new List<int>();

        private static readonly EnemyKind[] ExpectedWave6 =
        {
            EnemyKind.Armored, EnemyKind.Armored, EnemyKind.Armored, EnemyKind.Armored,
            EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner,
            EnemyKind.Brute, EnemyKind.Brute,
            EnemyKind.Boss
        };

        private static readonly string[] PreDefenseSeedSpots = { "A3", "A7", "A11" };
        private static readonly string[] ExtraDefenseSeedSpots = { "A3", "A12" };

        private enum Phase
        {
            SetupStart,
            SetupRunning,
            PrepareDefense,
            StartWave6,
            ObserveWave6,
            BossBaseline,
            SeedWaitRange,
            SeedWaitProjectile,
            SeedWaitImpact,
            FrostWaitRange,
            FrostWaitProjectile,
            FrostWaitImpact,
            FrostSpeedDuring,
            FrostWaitExpiry,
            FrostSpeedAfter,
            OakWaitRange,
            OakWaitApply,
            OakSpeedDuring,
            OakWaitExpiry,
            OakSpeedAfter,
            VictoryWaitNonBoss,
            VictoryExtraDefense,
            VictoryWait,
            VictoryVerify,
            VictoryRestartClick,
            VictoryRestartWait,
            VictoryRestartVerify,
            DefeatWave1Proof,
            DefeatRun,
            DefeatVerify,
            DefeatRestartClick,
            DefeatRestartWait,
            DefeatRestartVerify,
            FinalWave1Proof,
            Finalize,
            Done,
            Failed
        }

        private static bool _logSubscribed;
        private static bool _initialized;
        private static int _warmupFrames;
        private static Phase _phase;
        private static GameSession _session;
        private static MobileHud _hud;
        private static Button _waveButton;
        private static Text _enemyText;
        private static Text _waveText;
        private static GameObject _driverObject;
        private static List<PlantingSpot> _spots = new List<PlantingSpot>();

        private static int _initialBaseHp;
        private static int _initialEssence;
        private static int _setupWave;
        private static bool _setupSawPositive;
        private static int _defensePlantIndex;
        private static int _extraDefensePlantIndex;

        private static int _wave6BeforeCurrentWave;
        private static int _wave6AfterCurrentWave;
        private static int _wave6MaxActive;
        private static string _bossRegisteredHudText;
        private static EnemyUnit _boss;
        private static int _bossInstanceId;
        private static float _bossHpInitial;
        private static float _bossSpeedConfigured;
        private static float _bossSpeedMeasured;
        private static int _bossReward;
        private static int _bossBaseDamage;
        private static float _bossResistance;
        private static float _bossProgressAtSpawn;

        private static float _sampleStartTime;
        private static float _sampleStartProgress;

        private static float _seedHpBefore;
        private static float _seedHpAfter;
        private static float _seedRawDamage;
        private static float _seedFinalDamage;
        private static float _seedProjectileSpeed;
        private static DamageType _seedDamageType;
        private static bool _seedProjectileReached;

        private static float _frostHpBefore;
        private static float _frostHpAfter;
        private static float _frostRawDamage;
        private static float _frostFinalDamage;
        private static float _frostRequestedSlow;
        private static float _frostAppliedSlow;
        private static float _frostRequestedDuration;
        private static float _frostObservedDuration;
        private static float _frostSlowUntil;
        private static float _frostSpeedDuring;
        private static float _frostSpeedAfter;
        private static bool _frostExpired;

        private static float _oakHpBefore;
        private static float _oakHpAfter;
        private static float _oakDamage;
        private static float _oakRootUntil;
        private static float _oakRootObservedDuration;
        private static float _oakSpeedDuring;
        private static float _oakSpeedAfter;

        private static int _bossFinalEssenceBefore;
        private static int _bossFinalEssenceAfter;
        private static int _bossFinalBaseBefore;
        private static int _bossFinalBaseAfter;
        private static bool _bossDefeatedByProduction;

        private static int _victoryOldSessionId;
        private static bool _victoryRestartClicked;
        private static bool _victoryResetVerified;
        private static bool _victoryRestartWave1Proof;

        private static int _defeatPreviousBaseHp;
        private static int _defeatLeakCount;
        private static int _defeatWave;
        private static float _defeatFinalLeakLastProgress;
        private static EnemyKind _defeatFinalLeakKind;
        private static bool _defeatInputBlocked;
        private static int _defeatOldSessionId;
        private static bool _defeatRestartClicked;
        private static bool _defeatResetVerified;
        private static bool _finalWave1Proof;

        private static int _reloadWarmup;
        private static int _oldSessionIdWaitingForReload;

        private static float _originalTimeScale;
        private static float _originalMaximumDeltaTime;

        static BossEndStatesPlayModeValidator()
        {
            EditorApplication.update -= EditorTick;
            EditorApplication.update += EditorTick;
            if (SessionState.GetBool(ActiveKey, false))
                SubscribeLog();
        }

        internal static void Start()
        {
            ResetStaticState();
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

        private static void EditorTick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
                return;

            if (TimedOut())
            {
                FailAndLeavePlayMode("Validator wall-clock timeout exceeded " + F(TimeoutSeconds) + " seconds.");
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

            if (_initialized)
                return;

            _warmupFrames++;
            if (_warmupFrames < 5)
                return;

            GameSession session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
            if (session == null || session.Hud == null || session.Waves == null || session.Base == null ||
                session.Economy == null || session.Planting == null)
                return;

            try
            {
                AcquireSession(session, true);
                _initialBaseHp = _session.Base.Hp;
                _initialEssence = _session.Economy.Essence;

                Check(_session.Base.Hp == _session.Base.MaxHp, "preflight base HP is not full");
                Check(_session.Waves.CurrentWave == 0, "preflight CurrentWave expected 0");
                Check(_session.Waves.ReadyForNextWave, "preflight wave1 not ready");
                Check(!_session.Waves.Running, "preflight WaveDirector unexpectedly running");
                Check(_session.Enemies.Count == 0, "preflight active enemies not zero");
                Check(!_session.IsFinished, "preflight session already finished");
                Check(!_session.IsVictory, "preflight victory flag unexpectedly set");
                Check(_enemyText.text == "WROGOWIE  0", "preflight enemy HUD mismatch: " + _enemyText.text);
                Check(_waveText.text == "FALA  0/6", "preflight wave HUD mismatch: " + _waveText.text);

                AddFact("preflight.baseHp", _initialBaseHp);
                AddFact("preflight.essence", _initialEssence);
                AddFact("preflight.currentWave", _session.Waves.CurrentWave);
                AddFact("preflight.ready", _session.Waves.ReadyForNextWave);
                AddFact("preflight.hudEnemy", _enemyText.text);
                AddFact("preflight.hudWave", _waveText.text);

                _originalTimeScale = Time.timeScale;
                _originalMaximumDeltaTime = Time.maximumDeltaTime;
                Time.timeScale = 16f;
                Time.maximumDeltaTime = 0.20f;

                _setupWave = 1;
                _phase = Phase.SetupStart;

                _driverObject = new GameObject("T0006 Boss End States Validation Driver");
                BossEndStatesValidationDriver.PlayerFrame = PlayerFrame;
                _driverObject.AddComponent<BossEndStatesValidationDriver>();

                _initialized = true;
                SessionState.SetString(PhaseKey, "validating");
            }
            catch (Exception ex)
            {
                FailAndLeavePlayMode("initializeException=" + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static void AcquireSession(GameSession session, bool requireFresh)
        {
            _session = session;
            _hud = _session.Hud;
            _waveButton = (Button)RequireField(typeof(MobileHud), "_waveButton").GetValue(_hud);
            _enemyText = (Text)RequireField(typeof(MobileHud), "_enemyText").GetValue(_hud);
            _waveText = (Text)RequireField(typeof(MobileHud), "_waveText").GetValue(_hud);

            PlantingSpot[] found = UnityEngine.Object.FindObjectsByType<PlantingSpot>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            _spots = found.OrderBy(s => s.Id).ToList();

            if (requireFresh)
            {
                Check(_waveButton != null, "HUD wave button missing");
                Check(_enemyText != null, "HUD enemy text missing");
                Check(_waveText != null, "HUD wave text missing");
                Check(_spots.Count == 12, "expected 12 planting spots, got " + _spots.Count);
            }
        }

        internal static void PlayerFrame()
        {
            if (!SessionState.GetBool(ActiveKey, false) || !_initialized)
                return;

            try
            {
                ProcessPlayerFrame();
            }
            catch (Exception ex)
            {
                FailAndLeavePlayMode("playerFrameException=" + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static void ProcessPlayerFrame()
        {
            if (_session != null && _session.Waves != null && _session.Waves.CurrentWave == 6)
            {
                ObserveWave6Spawns();
                _wave6MaxActive = Mathf.Max(_wave6MaxActive, _session.Enemies.Count);
            }

            switch (_phase)
            {
                case Phase.SetupStart:
                    RunSetupStart();
                    break;
                case Phase.SetupRunning:
                    RunSetupRunning();
                    break;
                case Phase.PrepareDefense:
                    RunPrepareDefense();
                    break;
                case Phase.StartWave6:
                    RunStartWave6();
                    break;
                case Phase.ObserveWave6:
                    RunObserveWave6();
                    break;
                case Phase.BossBaseline:
                    RunBossBaseline();
                    break;
                case Phase.SeedWaitRange:
                    RunSeedWaitRange();
                    break;
                case Phase.SeedWaitProjectile:
                    RunSeedWaitProjectile();
                    break;
                case Phase.SeedWaitImpact:
                    RunSeedWaitImpact();
                    break;
                case Phase.FrostWaitRange:
                    RunFrostWaitRange();
                    break;
                case Phase.FrostWaitProjectile:
                    RunFrostWaitProjectile();
                    break;
                case Phase.FrostWaitImpact:
                    RunFrostWaitImpact();
                    break;
                case Phase.FrostSpeedDuring:
                    RunFrostSpeedDuring();
                    break;
                case Phase.FrostWaitExpiry:
                    RunFrostWaitExpiry();
                    break;
                case Phase.FrostSpeedAfter:
                    RunFrostSpeedAfter();
                    break;
                case Phase.OakWaitRange:
                    RunOakWaitRange();
                    break;
                case Phase.OakWaitApply:
                    RunOakWaitApply();
                    break;
                case Phase.OakSpeedDuring:
                    RunOakSpeedDuring();
                    break;
                case Phase.OakWaitExpiry:
                    RunOakWaitExpiry();
                    break;
                case Phase.OakSpeedAfter:
                    RunOakSpeedAfter();
                    break;
                case Phase.VictoryWaitNonBoss:
                    RunVictoryWaitNonBoss();
                    break;
                case Phase.VictoryExtraDefense:
                    RunVictoryExtraDefense();
                    break;
                case Phase.VictoryWait:
                    RunVictoryWait();
                    break;
                case Phase.VictoryVerify:
                    RunVictoryVerify();
                    break;
                case Phase.VictoryRestartClick:
                    RunVictoryRestartClick();
                    break;
                case Phase.VictoryRestartWait:
                    RunVictoryRestartWait();
                    break;
                case Phase.VictoryRestartVerify:
                    RunVictoryRestartVerify();
                    break;
                case Phase.DefeatWave1Proof:
                    RunDefeatWave1Proof();
                    break;
                case Phase.DefeatRun:
                    RunDefeatRun();
                    break;
                case Phase.DefeatVerify:
                    RunDefeatVerify();
                    break;
                case Phase.DefeatRestartClick:
                    RunDefeatRestartClick();
                    break;
                case Phase.DefeatRestartWait:
                    RunDefeatRestartWait();
                    break;
                case Phase.DefeatRestartVerify:
                    RunDefeatRestartVerify();
                    break;
                case Phase.FinalWave1Proof:
                    RunFinalWave1Proof();
                    break;
                case Phase.Finalize:
                    FinalizeSuccess();
                    break;
            }
        }

        private static void RunSetupStart()
        {
            if (_session.IsFinished)
            {
                Errors.Add("session finished during setup waves");
                _phase = Phase.Failed;
                FailAndLeavePlayMode("T0006 setup could not reach wave6.");
                return;
            }

            if (!_session.Waves.ReadyForNextWave || _session.Waves.Running || _session.Enemies.Count != 0)
                return;

            Check(_session.Waves.CurrentWave == _setupWave - 1,
                "setup before wave " + _setupWave + " CurrentWave mismatch");

            bool clicked = DispatchButtonClick(_waveButton);
            Check(clicked, "setup wave " + _setupWave + " HUD click failed");
            Check(_session.Waves.CurrentWave == _setupWave,
                "setup wave " + _setupWave + " did not start through HUD");
            Check(_session.Waves.Running, "setup wave " + _setupWave + " not running after click");

            _setupSawPositive = _session.Enemies.Count > 0;
            _phase = Phase.SetupRunning;
        }

        private static void RunSetupRunning()
        {
            if (_session.Enemies.Count > 0)
            {
                _setupSawPositive = true;
                Check(_enemyText.text == "WROGOWIE  " + _session.Enemies.Count,
                    "setup HUD count mismatch wave " + _setupWave);

                List<EnemyUnit> enemies = SnapshotEnemies();
                for (int i = 0; i < enemies.Count; i++)
                    if (enemies[i] != null && !enemies[i].IsDead)
                        enemies[i].TakeDamage(99999f, DamageType.Root);
            }

            if (_session.Waves.Running || !_session.Waves.ReadyForNextWave || _session.Enemies.Count != 0)
                return;

            Check(_setupSawPositive, "setup wave " + _setupWave + " never exposed a positive enemy HUD count");
            AddFact("setup.wave" + _setupWave + ".complete", true);

            if (_setupWave < 5)
            {
                _setupWave++;
                _phase = Phase.SetupStart;
                return;
            }

            AddFact("setup.essenceAfterWaves1To5", _session.Economy.Essence);
            AddFact("setup.currentWave", _session.Waves.CurrentWave);
            Check(_session.Waves.CurrentWave == 5, "setup did not stop at wave5");
            Check(_session.Waves.ReadyForNextWave, "wave6 is not ready after setup wave5");

            _defensePlantIndex = 0;
            _phase = Phase.PrepareDefense;
        }

        private static void RunPrepareDefense()
        {
            Time.timeScale = 1f;
            if (_defensePlantIndex >= PreDefenseSeedSpots.Length)
            {
                AddFact("victoryDefense.preWave6SeedCount", CountPlantsOfType<SeedShooterPlant>());
                _phase = Phase.StartWave6;
                return;
            }

            string id = PreDefenseSeedSpots[_defensePlantIndex];
            PlantingSpot spot = FindSpot(id);
            bool planted = TryPlantThroughProductionUi(spot, PlantKind.SeedShooter);
            Check(planted, "failed to plant pre-wave6 SeedShooter at " + id);
            if (planted)
            {
                Check(spot.Occupied && spot.Plant == PlantKind.SeedShooter,
                    "pre-wave6 planting state incorrect at " + id);
                _defensePlantIndex++;
            }
        }

        private static void RunStartWave6()
        {
            Check(_session.Waves.ReadyForNextWave, "wave6 not ready before click");
            Check(IsWaveButtonActive(), "wave6 button inactive before click");

            _wave6BeforeCurrentWave = _session.Waves.CurrentWave;
            bool clicked = DispatchButtonClick(_waveButton);
            _wave6AfterCurrentWave = _session.Waves.CurrentWave;

            AddFact("wave6.beforeCurrentWave", _wave6BeforeCurrentWave);
            AddFact("wave6.afterCurrentWave", _wave6AfterCurrentWave);
            AddFact("wave6.clickDispatched", clicked);

            Check(clicked, "wave6 HUD pointer click failed");
            Check(_wave6BeforeCurrentWave == 5, "wave6 before CurrentWave expected5");
            Check(_wave6AfterCurrentWave == 6, "wave6 did not transition CurrentWave 5->6");
            Check(_session.Waves.Running, "wave6 not running after click");
            Check(!_session.Waves.ReadyForNextWave, "wave6 remained ready after start");
            Check(!IsWaveButtonActive(), "next-wave button remained active during wave6");

            Time.timeScale = 4f;
            Time.maximumDeltaTime = 0.20f;
            _phase = Phase.ObserveWave6;
        }

        private static void ObserveWave6Spawns()
        {
            if (_session == null) return;
            List<EnemyUnit> enemies = SnapshotEnemies();
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyUnit enemy = enemies[i];
                if (enemy == null) continue;
                int id = enemy.GetInstanceID();
                if (!Wave6SeenIds.Add(id))
                    continue;

                Wave6Observed.Add(enemy.Kind);

                if (enemy.Kind == EnemyKind.Boss)
                {
                    _boss = enemy;
                    _bossInstanceId = id;
                    _bossHpInitial = ReadEnemyFloat(enemy, "_hp");
                    _bossSpeedConfigured = ReadEnemyFloat(enemy, "_speed");
                    _bossReward = ReadEnemyInt(enemy, "_reward");
                    _bossBaseDamage = ReadEnemyInt(enemy, "_baseDamage");
                    _bossResistance = ReadEnemyFloat(enemy, "_physicalResistance");
                    _bossProgressAtSpawn = enemy.ProgressDistance;
                    _bossRegisteredHudText = _enemyText.text;

                    Check(_bossRegisteredHudText == "WROGOWIE  " + _session.Enemies.Count,
                        "boss registration HUD count mismatch");
                }
            }
        }

        private static void RunObserveWave6()
        {
            if (Wave6Observed.Count < ExpectedWave6.Length || _boss == null)
                return;

            Check(Wave6Observed.Count == ExpectedWave6.Length,
                "wave6 observed spawn count expected12 actual=" + Wave6Observed.Count);

            int count = Mathf.Min(Wave6Observed.Count, ExpectedWave6.Length);
            for (int i = 0; i < count; i++)
            {
                Check(Wave6Observed[i] == ExpectedWave6[i],
                    "wave6 spawn order index=" + i + " expected=" + ExpectedWave6[i] +
                    " actual=" + Wave6Observed[i]);
            }

            AddFact("wave6.observedSpawnCount", Wave6Observed.Count);
            AddFact("wave6.observedSequence", SequenceText(Wave6Observed));
            AddFact("wave6.armoredCount", Wave6Observed.Count(k => k == EnemyKind.Armored));
            AddFact("wave6.runnerCount", Wave6Observed.Count(k => k == EnemyKind.Runner));
            AddFact("wave6.bruteCount", Wave6Observed.Count(k => k == EnemyKind.Brute));
            AddFact("wave6.bossCount", Wave6Observed.Count(k => k == EnemyKind.Boss));
            AddFact("wave6.bossRegisteredHudText", _bossRegisteredHudText);
            AddFact("wave6.readyDuring", _session.Waves.ReadyForNextWave);
            AddFact("wave6.buttonActiveDuring", IsWaveButtonActive());

            AddFact("boss.kind", _boss.Kind);
            AddFact("boss.hpInitial", _bossHpInitial);
            AddFact("boss.speedConfigured", _bossSpeedConfigured);
            AddFact("boss.reward", _bossReward);
            AddFact("boss.baseDamage", _bossBaseDamage);
            AddFact("boss.physicalResistance", _bossResistance);
            AddFact("boss.progressAtSpawn", _bossProgressAtSpawn);

            Check(_boss.Kind == EnemyKind.Boss, "observed wave6 boss kind mismatch");
            Check(_bossHpInitial > 0f, "boss initial HP not positive");
            Check(_bossSpeedConfigured > 0f, "boss configured speed not positive");
            Check(_bossReward > 0, "boss reward not positive");
            Check(_bossBaseDamage > 0, "boss base damage not positive");
            Check(_bossResistance > 0f && _bossResistance < 1f, "boss physical resistance out of range");

            Time.timeScale = 1f;
            Time.maximumDeltaTime = 0.10f;
            _sampleStartTime = -1f;
            _phase = Phase.BossBaseline;
        }

        private static void RunBossBaseline()
        {
            EnsureBossAlive("baseline");

            if (_sampleStartTime < 0f)
            {
                _sampleStartTime = Time.time;
                _sampleStartProgress = _boss.ProgressDistance;
                return;
            }

            float elapsed = Time.time - _sampleStartTime;
            if (elapsed < 0.80f)
                return;

            _bossSpeedMeasured = (_boss.ProgressDistance - _sampleStartProgress) / Mathf.Max(0.001f, elapsed);
            AddFact("boss.speedMeasured", _bossSpeedMeasured);
            CheckApprox(_bossSpeedMeasured, _bossSpeedConfigured, 0.05f, "boss baseline speed");

            _phase = Phase.SeedWaitRange;
        }

        private static void RunSeedWaitRange()
        {
            EnsureBossAlive("seed range");
            PlantingSpot spot = FindSpot("A5");
            float distance = Vector2.Distance(_boss.transform.position, spot.transform.position);
            if (distance > 3.05f)
                return;

            bool planted = TryPlantThroughProductionUi(spot, PlantKind.SeedShooter);
            Check(planted, "failed to plant SeedShooter A9 for boss physical evidence");
            if (!planted) return;

            AddFact("boss.seed.spot", spot.Id);
            AddFact("boss.seed.distanceAtPlantRU", distance);
            _phase = Phase.SeedWaitProjectile;
        }

        private static void RunSeedWaitProjectile()
        {
            EnsureBossAlive("seed projectile");
            PlantingSpot spot = FindSpot("A1");
            SeedProjectile projectile = FindProjectileTargetingBoss("MagicSeed", spot.transform.position, 0.75f);
            if (projectile == null)
                return;

            _seedHpBefore = ReadEnemyFloat(_boss, "_hp");
            _seedRawDamage = ReadProjectileFloat(projectile, "_damage");
            _seedProjectileSpeed = ReadProjectileFloat(projectile, "_speed");
            _seedDamageType = ReadProjectileDamageType(projectile);

            EnemyUnit target = ReadProjectileTarget(projectile);
            Check(target == _boss, "Seed projectile did not target boss");

            DestroyPlantAt<SeedShooterPlant>(spot);

            AddFact("boss.seed.hpBefore", _seedHpBefore);
            AddFact("boss.seed.rawDamage", _seedRawDamage);
            AddFact("boss.seed.projectileSpeed", _seedProjectileSpeed);
            AddFact("boss.seed.damageType", _seedDamageType);
            AddFact("boss.seed.resistance", _bossResistance);

            _phase = Phase.SeedWaitImpact;
        }

        private static void RunSeedWaitImpact()
        {
            EnsureBossAlive("seed impact");
            float hp = ReadEnemyFloat(_boss, "_hp");
            if (Mathf.Abs(hp - _seedHpBefore) < 0.01f)
                return;

            _seedHpAfter = hp;
            _seedFinalDamage = _seedHpBefore - _seedHpAfter;
            _seedProjectileReached = _seedFinalDamage > 0f;

            AddFact("boss.seed.hpAfter", _seedHpAfter);
            AddFact("boss.seed.finalDamage", _seedFinalDamage);
            AddFact("boss.seed.projectileReached", _seedProjectileReached);

            Check(_seedProjectileReached, "Seed projectile did not damage boss");
            Check(_seedDamageType == DamageType.Physical, "Seed projectile damage type not Physical");
            CheckApprox(_seedRawDamage, 22f, 0.01f, "Seed raw damage");
            CheckApprox(_seedFinalDamage, _seedRawDamage * (1f - _bossResistance), 0.06f,
                "boss physical resistance final damage");

            _phase = Phase.FrostWaitRange;
        }

        private static void RunFrostWaitRange()
        {
            EnsureBossAlive("frost range");
            PlantingSpot spot = FindSpot("A1");
            float distance = Vector2.Distance(_boss.transform.position, spot.transform.position);
            if (distance > 3.05f)
            {
                Time.timeScale = 4f;
                return;
            }

            Time.timeScale = 1f;
            bool planted = TryPlantThroughProductionUi(spot, PlantKind.FrostBud);
            Check(planted, "failed to plant FrostBud A1 for boss slow evidence");
            if (!planted) return;

            AddFact("boss.frost.spot", spot.Id);
            AddFact("boss.frost.distanceAtPlantRU", distance);
            _phase = Phase.FrostWaitProjectile;
        }

        private static void RunFrostWaitProjectile()
        {
            EnsureBossAlive("frost projectile");
            PlantingSpot spot = FindSpot("A5");
            SeedProjectile projectile = FindProjectileTargetingBoss("FrostSeed", spot.transform.position, 0.75f);
            if (projectile == null)
                return;

            _frostHpBefore = ReadEnemyFloat(_boss, "_hp");
            _frostRawDamage = ReadProjectileFloat(projectile, "_damage");
            _frostRequestedSlow = ReadProjectileFloat(projectile, "_slow");
            _frostRequestedDuration = ReadProjectileFloat(projectile, "_slowDuration");
            DamageType type = ReadProjectileDamageType(projectile);

            Check(type == DamageType.Frost, "Frost projectile damage type not Frost");
            Check(ReadProjectileTarget(projectile) == _boss, "Frost projectile did not target boss");

            DestroyPlantAt<FrostBudPlant>(spot);

            AddFact("boss.frost.hpBefore", _frostHpBefore);
            AddFact("boss.frost.rawDamage", _frostRawDamage);
            AddFact("boss.frost.requestedSlow", _frostRequestedSlow);
            AddFact("boss.frost.requestedDuration", _frostRequestedDuration);
            _phase = Phase.FrostWaitImpact;
        }

        private static void RunFrostWaitImpact()
        {
            EnsureBossAlive("frost impact");
            float hp = ReadEnemyFloat(_boss, "_hp");
            if (Mathf.Abs(hp - _frostHpBefore) < 0.01f)
                return;

            _frostHpAfter = hp;
            _frostFinalDamage = _frostHpBefore - _frostHpAfter;
            _frostAppliedSlow = ReadEnemyFloat(_boss, "_slowStrength");
            _frostSlowUntil = ReadEnemyFloat(_boss, "_slowUntil");
            _frostObservedDuration = _frostSlowUntil - Time.time;

            AddFact("boss.frost.hpAfter", _frostHpAfter);
            AddFact("boss.frost.finalDamage", _frostFinalDamage);
            AddFact("boss.frost.appliedSlow", _frostAppliedSlow);
            AddFact("boss.frost.durationObserved", _frostObservedDuration);

            CheckApprox(_frostRawDamage, 18f, 0.01f, "Frost raw damage");
            CheckApprox(_frostFinalDamage, _frostRawDamage, 0.06f, "Frost final damage");
            CheckApprox(_frostRequestedSlow, 0.35f, 0.001f, "Frost requested slow");
            CheckApprox(_frostAppliedSlow, 0.20f, 0.001f, "boss slow cap");
            CheckApprox(_frostRequestedDuration, 2.5f, 0.01f, "Frost requested duration");
            CheckApprox(_frostObservedDuration, 2.5f, 0.15f, "boss slow duration");

            _sampleStartTime = Time.time;
            _sampleStartProgress = _boss.ProgressDistance;
            _phase = Phase.FrostSpeedDuring;
        }

        private static void RunFrostSpeedDuring()
        {
            EnsureBossAlive("frost speed during");
            float elapsed = Time.time - _sampleStartTime;
            if (elapsed < 0.80f)
                return;

            _frostSpeedDuring = (_boss.ProgressDistance - _sampleStartProgress) / Mathf.Max(0.001f, elapsed);
            AddFact("boss.frost.speedBefore", _bossSpeedMeasured);
            AddFact("boss.frost.speedDuring", _frostSpeedDuring);
            CheckApprox(_frostSpeedDuring, _bossSpeedConfigured * (1f - _frostAppliedSlow), 0.05f,
                "boss speed during Frost slow");
            _phase = Phase.FrostWaitExpiry;
        }

        private static void RunFrostWaitExpiry()
        {
            EnsureBossAlive("frost expiry");
            if (Time.time < _frostSlowUntil + 0.08f)
                return;

            _frostExpired = true;
            _sampleStartTime = Time.time;
            _sampleStartProgress = _boss.ProgressDistance;
            _phase = Phase.FrostSpeedAfter;
        }

        private static void RunFrostSpeedAfter()
        {
            EnsureBossAlive("frost speed after");
            float elapsed = Time.time - _sampleStartTime;
            if (elapsed < 0.80f)
                return;

            _frostSpeedAfter = (_boss.ProgressDistance - _sampleStartProgress) / Mathf.Max(0.001f, elapsed);
            AddFact("boss.frost.expired", _frostExpired);
            AddFact("boss.frost.speedAfter", _frostSpeedAfter);
            Check(_frostExpired, "boss Frost slow did not expire");
            CheckApprox(_frostSpeedAfter, _bossSpeedConfigured, 0.05f, "boss speed after Frost");

            Time.timeScale = 8f;
            _phase = Phase.OakWaitRange;
        }

        private static void RunOakWaitRange()
        {
            EnsureBossAlive("oak range");
            PlantingSpot spot = FindSpot("A9");
            float distance = Vector2.Distance(_boss.transform.position, spot.transform.position);
            if (distance > 2.25f || AnyNonBossWithin(spot.transform.position, 2.4f))
                return;

            Time.timeScale = 1f;
            _oakHpBefore = ReadEnemyFloat(_boss, "_hp");
            bool planted = TryPlantThroughProductionUi(spot, PlantKind.OakGuardian);
            Check(planted, "failed to plant OakGuardian A2 for boss root evidence");
            if (!planted) return;

            AddFact("boss.oak.spot", spot.Id);
            AddFact("boss.oak.distanceAtPlantRU", distance);
            AddFact("boss.oak.hpBefore", _oakHpBefore);
            _phase = Phase.OakWaitApply;
        }

        private static void RunOakWaitApply()
        {
            EnsureBossAlive("oak apply");
            float hp = ReadEnemyFloat(_boss, "_hp");
            float rootUntil = ReadEnemyFloat(_boss, "_rootUntil");
            if (Mathf.Abs(hp - _oakHpBefore) < 0.01f || rootUntil <= Time.time)
                return;

            _oakHpAfter = hp;
            _oakDamage = _oakHpBefore - _oakHpAfter;
            _oakRootUntil = rootUntil;
            _oakRootObservedDuration = _oakRootUntil - Time.time;

            DestroyPlantAt<OakGuardianPlant>(FindSpot("A9"));

            AddFact("boss.oak.hpAfter", _oakHpAfter);
            AddFact("boss.oak.damage", _oakDamage);
            AddFact("boss.oak.rootDurationExpected", 1.75f * 0.35f);
            AddFact("boss.oak.rootDurationObserved", _oakRootObservedDuration);

            CheckApprox(_oakDamage, 8f, 0.06f, "Oak root damage");
            CheckApprox(_oakRootObservedDuration, 1.75f * 0.35f, 0.12f, "boss root duration");

            _sampleStartTime = Time.time;
            _sampleStartProgress = _boss.ProgressDistance;
            _phase = Phase.OakSpeedDuring;
        }

        private static void RunOakSpeedDuring()
        {
            EnsureBossAlive("oak speed during");
            float elapsed = Time.time - _sampleStartTime;
            if (elapsed < 0.25f)
                return;

            _oakSpeedDuring = (_boss.ProgressDistance - _sampleStartProgress) / Mathf.Max(0.001f, elapsed);
            AddFact("boss.oak.speedBefore", _bossSpeedMeasured);
            AddFact("boss.oak.speedDuring", _oakSpeedDuring);
            Check(Mathf.Abs(_oakSpeedDuring) <= 0.04f, "boss moved during root");

            _phase = Phase.OakWaitExpiry;
        }

        private static void RunOakWaitExpiry()
        {
            EnsureBossAlive("oak expiry");
            if (Time.time < _oakRootUntil + 0.08f)
                return;

            _sampleStartTime = Time.time;
            _sampleStartProgress = _boss.ProgressDistance;
            _phase = Phase.OakSpeedAfter;
        }

        private static void RunOakSpeedAfter()
        {
            EnsureBossAlive("oak speed after");
            float elapsed = Time.time - _sampleStartTime;
            if (elapsed < 0.80f)
                return;

            _oakSpeedAfter = (_boss.ProgressDistance - _sampleStartProgress) / Mathf.Max(0.001f, elapsed);
            AddFact("boss.oak.speedAfter", _oakSpeedAfter);
            CheckApprox(_oakSpeedAfter, _bossSpeedConfigured, 0.05f, "boss speed after root");

            Time.timeScale = 8f;
            _phase = Phase.VictoryWaitNonBoss;
        }

        private static void RunVictoryWaitNonBoss()
        {
            EnsureBossAlive("victory nonboss wait");
            if (CountNonBossEnemies() > 0)
                return;

            _extraDefensePlantIndex = 0;
            _phase = Phase.VictoryExtraDefense;
        }

        private static void RunVictoryExtraDefense()
        {
            if (_extraDefensePlantIndex >= ExtraDefenseSeedSpots.Length)
            {
                _bossFinalEssenceBefore = _session.Economy.Essence;
                _bossFinalBaseBefore = _session.Base.Hp;
                AddFact("victory.bossOnlyEssenceBefore", _bossFinalEssenceBefore);
                AddFact("victory.bossOnlyBaseBefore", _bossFinalBaseBefore);

                Time.timeScale = 16f;
                Time.maximumDeltaTime = 0.20f;
                _phase = Phase.VictoryWait;
                return;
            }

            string id = ExtraDefenseSeedSpots[_extraDefensePlantIndex];
            PlantingSpot spot = FindSpot(id);

            if (_session.Economy.Essence < 100)
            {
                AddFact("victory.extraDefenseSkipped." + id, "insufficientEssence");
                _extraDefensePlantIndex++;
                return;
            }

            bool planted = TryPlantThroughProductionUi(spot, PlantKind.SeedShooter);
            Check(planted, "failed to plant extra victory SeedShooter at " + id);
            if (planted)
                _extraDefensePlantIndex++;
        }

        private static void RunVictoryWait()
        {
            if (_session == null)
                return;

            int bossCount = _session.Enemies.Count(e => e != null && e.Kind == EnemyKind.Boss);
            if (bossCount == 0 && !_bossDefeatedByProduction)
            {
                _bossFinalEssenceAfter = _session.Economy.Essence;
                _bossFinalBaseAfter = _session.Base.Hp;
                int rewardDelta = _bossFinalEssenceAfter - _bossFinalEssenceBefore;
                int baseDelta = _bossFinalBaseBefore - _bossFinalBaseAfter;

                _bossDefeatedByProduction = rewardDelta == _bossReward && baseDelta == 0;

                AddFact("victory.bossEssenceAfter", _bossFinalEssenceAfter);
                AddFact("victory.bossRewardDelta", rewardDelta);
                AddFact("victory.bossBaseAfter", _bossFinalBaseAfter);
                AddFact("victory.bossBaseDelta", baseDelta);
                AddFact("victory.bossDefeatedByProduction", _bossDefeatedByProduction);

                Check(rewardDelta == _bossReward,
                    "boss disappearance did not produce isolated configured reward; expected=" + _bossReward +
                    " actual=" + rewardDelta);
                Check(baseDelta == 0, "boss appears to have leaked instead of dying; baseDelta=" + baseDelta);
            }

            if (!_session.IsFinished)
                return;

            _phase = Phase.VictoryVerify;
        }

        private static void RunVictoryVerify()
        {
            GameObject endPanel = GameObject.Find("EndPanel");
            string title = FindEndPanelTitle(endPanel);
            Button restart = FindRestartButton(endPanel);

            AddFact("wave6.maxActiveCount", _wave6MaxActive);
            AddFact("victory.sessionFinished", _session.IsFinished);
            AddFact("victory.isVictory", _session.IsVictory);
            AddFact("victory.currentWave", _session.Waves.CurrentWave);
            AddFact("victory.ready", _session.Waves.ReadyForNextWave);
            AddFact("victory.running", _session.Waves.Running);
            AddFact("victory.activeEnemies", _session.Enemies.Count);
            AddFact("victory.hudEnemy", _enemyText.text);
            AddFact("victory.hudWave", _waveText.text);
            AddFact("victory.endPanel", endPanel != null);
            AddFact("victory.title", title);
            AddFact("victory.restartButton", restart != null);
            AddFact("victory.waveButtonActive", IsWaveButtonActive());
            AddFact("victory.wave7Available", _session.Waves.ReadyForNextWave && _session.Waves.CurrentWave >= 6);

            Check(_bossDefeatedByProduction, "boss was not proven killed by production defense");
            Check(_session.IsFinished, "victory session not finished");
            Check(_session.IsVictory, "victory flag false");
            Check(_session.Waves.CurrentWave == 6, "victory CurrentWave not6");
            Check(!_session.Waves.ReadyForNextWave, "wave7 became ready after victory");
            Check(!_session.Waves.Running, "WaveDirector still running after victory");
            Check(_session.Enemies.Count == 0, "active enemies not zero at victory");
            Check(_enemyText.text == "WROGOWIE  0", "victory enemy HUD not zero");
            Check(endPanel != null, "victory EndPanel missing");
            Check(title == "ZIELONY TRAKT OBRONIONY!", "victory title mismatch: " + title);
            Check(restart != null, "victory restart button missing");
            Check(!IsWaveButtonActive(), "next-wave button active after victory");

            _phase = Phase.VictoryRestartClick;
        }

        private static void RunVictoryRestartClick()
        {
            GameObject panel = GameObject.Find("EndPanel");
            Button restart = FindRestartButton(panel);
            if (restart == null)
            {
                Errors.Add("victory restart button missing at click");
                FailAndLeavePlayMode("Cannot execute victory restart.");
                return;
            }

            _victoryOldSessionId = _session.GetInstanceID();
            AddFact("restartVictory.oldSessionId", _victoryOldSessionId);
            AddFact("restartVictory.occupiedBeforeRestart", _spots.Count(s => s != null && s.Occupied));
            AddFact("restartVictory.plantsBeforeRestart", CountAllPlants());

            _victoryRestartClicked = DispatchButtonClick(restart);
            AddFact("restartVictory.clickDispatched", _victoryRestartClicked);
            Check(_victoryRestartClicked, "victory restart pointer click failed");

            _oldSessionIdWaitingForReload = _victoryOldSessionId;
            _reloadWarmup = 0;
            Time.timeScale = 1f;
            _phase = Phase.VictoryRestartWait;
        }

        private static void RunVictoryRestartWait()
        {
            if (!TryAcquireReloadedSession(_oldSessionIdWaitingForReload))
                return;

            _reloadWarmup++;
            if (_reloadWarmup < 4)
                return;

            _phase = Phase.VictoryRestartVerify;
        }

        private static void RunVictoryRestartVerify()
        {
            _victoryResetVerified = VerifyFreshReset("restartVictory", _victoryOldSessionId);
            Check(_victoryResetVerified, "victory restart reset evidence failed");
            _phase = Phase.DefeatWave1Proof;
        }

        private static void RunDefeatWave1Proof()
        {
            if (!_session.Waves.ReadyForNextWave || _session.Waves.CurrentWave != 0)
                return;

            bool clicked = DispatchButtonClick(_waveButton);
            _victoryRestartWave1Proof = clicked && _session.Waves.CurrentWave == 1 && _session.Waves.Running;

            AddFact("restartVictory.wave1ClickProof", _victoryRestartWave1Proof);
            Check(_victoryRestartWave1Proof, "wave1 could not start after victory restart");

            _defeatPreviousBaseHp = _session.Base.Hp;
            DefeatPreviousIds = new HashSet<int>(SnapshotEnemies().Select(e => e.GetInstanceID()));
            DefeatLastProgress.Clear();
            DefeatKinds.Clear();
            DefeatBaseTransitions.Clear();
            DefeatWavesStarted.Clear();
            DefeatWavesStarted.Add(1);
            _defeatLeakCount = 0;
            _defeatFinalLeakLastProgress = 0f;
            _defeatWave = 0;

            Time.timeScale = 16f;
            Time.maximumDeltaTime = 0.20f;
            _phase = Phase.DefeatRun;
        }

        private static void RunDefeatRun()
        {
            TrackNaturalDefeatLeaks();

            if (_session.IsFinished)
            {
                _defeatWave = _session.Waves.CurrentWave;
                _phase = Phase.DefeatVerify;
                return;
            }

            if (!_session.Waves.Running && _session.Waves.ReadyForNextWave && _session.Enemies.Count == 0)
            {
                int next = _session.Waves.CurrentWave + 1;
                Check(next <= 5, "defeat scenario reached boss wave unexpectedly");
                if (next > 5)
                {
                    FailAndLeavePlayMode("Natural defeat did not occur before T0006 boss boundary.");
                    return;
                }

                bool clicked = DispatchButtonClick(_waveButton);
                Check(clicked, "defeat scenario next-wave click failed for wave " + next);
                Check(_session.Waves.CurrentWave == next, "defeat scenario wave transition failed");
                DefeatWavesStarted.Add(next);
            }
        }

        private static void TrackNaturalDefeatLeaks()
        {
            List<EnemyUnit> current = SnapshotEnemies();
            HashSet<int> currentIds = new HashSet<int>();

            for (int i = 0; i < current.Count; i++)
            {
                EnemyUnit enemy = current[i];
                if (enemy == null) continue;
                int id = enemy.GetInstanceID();
                currentIds.Add(id);
                DefeatLastProgress[id] = enemy.ProgressDistance;
                DefeatKinds[id] = enemy.Kind;
            }

            int currentBase = _session.Base.Hp;
            if (currentBase < _defeatPreviousBaseHp)
            {
                int delta = _defeatPreviousBaseHp - currentBase;
                int chosenId = 0;
                float bestProgress = -1f;
                EnemyKind chosenKind = EnemyKind.Wanderer;

                foreach (int previousId in DefeatPreviousIds)
                {
                    if (currentIds.Contains(previousId))
                        continue;

                    float progress;
                    if (DefeatLastProgress.TryGetValue(previousId, out progress) && progress > bestProgress)
                    {
                        bestProgress = progress;
                        chosenId = previousId;
                        EnemyKind kind;
                        if (DefeatKinds.TryGetValue(previousId, out kind))
                            chosenKind = kind;
                    }
                }

                _defeatLeakCount++;
                _defeatFinalLeakLastProgress = bestProgress;
                _defeatFinalLeakKind = chosenKind;
                DefeatBaseTransitions.Add(
                    _defeatPreviousBaseHp + "->" + currentBase +
                    ":damage=" + delta +
                    ":kind=" + chosenKind +
                    ":lastProgress=" + F(bestProgress));

                Check(bestProgress >= MapAData.TotalPathLength - 0.50f,
                    "defeat leak did not have near-base progress; lastProgress=" + F(bestProgress));
            }

            _defeatPreviousBaseHp = currentBase;
            DefeatPreviousIds = currentIds;
        }

        private static void RunDefeatVerify()
        {
            TrackNaturalDefeatLeaks();

            GameObject endPanel = GameObject.Find("EndPanel");
            string title = FindEndPanelTitle(endPanel);
            Button restart = FindRestartButton(endPanel);

            PlantingSpot inputSpot = _spots.FirstOrDefault(s => s != null && !s.Occupied);
            if (inputSpot != null && Camera.main != null)
            {
                Vector2 screen = Camera.main.WorldToScreenPoint(inputSpot.transform.position);
                _defeatInputBlocked = !_session.Planting.TryHandleScreenPress(screen);
            }

            AddFact("defeat.leakCount", _defeatLeakCount);
            AddFact("defeat.baseTransitions", string.Join("|", DefeatBaseTransitions.ToArray()));
            AddFact("defeat.defeatWave", _defeatWave);
            AddFact("defeat.finalLeakKind", _defeatFinalLeakKind);
            AddFact("defeat.finalLeakLastProgress", _defeatFinalLeakLastProgress);
            AddFact("defeat.baseHp", _session.Base.Hp);
            AddFact("defeat.sessionFinished", _session.IsFinished);
            AddFact("defeat.isVictory", _session.IsVictory);
            AddFact("defeat.ready", _session.Waves.ReadyForNextWave);
            AddFact("defeat.running", _session.Waves.Running);
            AddFact("defeat.waveButtonActive", IsWaveButtonActive());
            AddFact("defeat.inputBlocked", _defeatInputBlocked);
            AddFact("defeat.endPanel", endPanel != null);
            AddFact("defeat.title", title);
            AddFact("defeat.restartButton", restart != null);
            AddFact("defeat.wavesStarted", string.Join(",", DefeatWavesStarted.Select(x => x.ToString()).ToArray()));

            Check(_session.Base.Hp == 0, "defeat Base HP not zero");
            Check(_session.IsFinished, "defeat session not finished");
            Check(!_session.IsVictory, "defeat incorrectly marked victory");
            Check(!_session.Waves.ReadyForNextWave, "waves remain ready after defeat");
            Check(!IsWaveButtonActive(), "wave button active after defeat");
            Check(_defeatInputBlocked, "planting input accepted after defeat");
            Check(endPanel != null, "defeat EndPanel missing");
            Check(title == "KRÓL CIERNI PRZEDARŁ SIĘ DO BAZY", "defeat title mismatch: " + title);
            Check(restart != null, "defeat restart button missing");
            Check(_defeatFinalLeakLastProgress >= MapAData.TotalPathLength - 0.50f,
                "final defeat leak lacks natural ReachBase evidence");

            _phase = Phase.DefeatRestartClick;
        }

        private static void RunDefeatRestartClick()
        {
            Button restart = FindRestartButton(GameObject.Find("EndPanel"));
            if (restart == null)
            {
                Errors.Add("defeat restart button missing at click");
                FailAndLeavePlayMode("Cannot execute defeat restart.");
                return;
            }

            _defeatOldSessionId = _session.GetInstanceID();
            AddFact("restartDefeat.oldSessionId", _defeatOldSessionId);

            _defeatRestartClicked = DispatchButtonClick(restart);
            AddFact("restartDefeat.clickDispatched", _defeatRestartClicked);
            Check(_defeatRestartClicked, "defeat restart pointer click failed");

            _oldSessionIdWaitingForReload = _defeatOldSessionId;
            _reloadWarmup = 0;
            Time.timeScale = 1f;
            _phase = Phase.DefeatRestartWait;
        }

        private static void RunDefeatRestartWait()
        {
            if (!TryAcquireReloadedSession(_oldSessionIdWaitingForReload))
                return;

            _reloadWarmup++;
            if (_reloadWarmup < 4)
                return;

            _phase = Phase.DefeatRestartVerify;
        }

        private static void RunDefeatRestartVerify()
        {
            _defeatResetVerified = VerifyFreshReset("restartDefeat", _defeatOldSessionId);
            Check(_defeatResetVerified, "defeat restart reset evidence failed");
            _phase = Phase.FinalWave1Proof;
        }

        private static void RunFinalWave1Proof()
        {
            if (!_session.Waves.ReadyForNextWave || _session.Waves.CurrentWave != 0)
                return;

            bool clicked = DispatchButtonClick(_waveButton);
            _finalWave1Proof = clicked && _session.Waves.CurrentWave == 1 && _session.Waves.Running;
            AddFact("restartDefeat.wave1ClickProof", _finalWave1Proof);
            Check(_finalWave1Proof, "wave1 could not start after defeat restart");

            if (_session.Enemies.Count <= 0)
                return;

            Check(_enemyText.text == "WROGOWIE  " + _session.Enemies.Count,
                "final second-session HUD enemy count mismatch");
            AddFact("restartDefeat.wave1HudEnemy", _enemyText.text);
            AddFact("restartDefeat.secondSessionPlayable", true);

            _phase = Phase.Finalize;
        }

        private static bool VerifyFreshReset(string prefix, int oldSessionId)
        {
            bool differentSession = _session != null && _session.GetInstanceID() != oldSessionId;
            int occupied = _spots.Count(s => s != null && s.Occupied);
            int plantCount = CountAllPlants();
            int projectileCount = UnityEngine.Object.FindObjectsByType<SeedProjectile>(
                FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            int enemyObjects = UnityEngine.Object.FindObjectsByType<EnemyUnit>(
                FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            bool endPanelAbsent = GameObject.Find("EndPanel") == null;

            AddFact(prefix + ".newSessionId", _session != null ? _session.GetInstanceID() : 0);
            AddFact(prefix + ".differentSession", differentSession);
            AddFact(prefix + ".sessionFinished", _session != null && _session.IsFinished);
            AddFact(prefix + ".isVictory", _session != null && _session.IsVictory);
            AddFact(prefix + ".baseHp", _session != null ? _session.Base.Hp : -1);
            AddFact(prefix + ".essence", _session != null ? _session.Economy.Essence : -1);
            AddFact(prefix + ".currentWave", _session != null ? _session.Waves.CurrentWave : -1);
            AddFact(prefix + ".ready", _session != null && _session.Waves.ReadyForNextWave);
            AddFact(prefix + ".activeEnemies", _session != null ? _session.Enemies.Count : -1);
            AddFact(prefix + ".hudEnemy", _enemyText != null ? _enemyText.text : "null");
            AddFact(prefix + ".hudWave", _waveText != null ? _waveText.text : "null");
            AddFact(prefix + ".occupiedSpots", occupied);
            AddFact(prefix + ".plantCount", plantCount);
            AddFact(prefix + ".projectileCount", projectileCount);
            AddFact(prefix + ".enemyObjectCount", enemyObjects);
            AddFact(prefix + ".endPanelAbsent", endPanelAbsent);
            AddFact(prefix + ".waveButtonActive", IsWaveButtonActive());

            bool ok =
                differentSession &&
                !_session.IsFinished &&
                !_session.IsVictory &&
                _session.Base.Hp == _initialBaseHp &&
                _session.Economy.Essence == _initialEssence &&
                _session.Waves.CurrentWave == 0 &&
                _session.Waves.ReadyForNextWave &&
                !_session.Waves.Running &&
                _session.Enemies.Count == 0 &&
                _enemyText.text == "WROGOWIE  0" &&
                _waveText.text == "FALA  0/6" &&
                occupied == 0 &&
                plantCount == 0 &&
                projectileCount == 0 &&
                enemyObjects == 0 &&
                endPanelAbsent &&
                IsWaveButtonActive();

            return ok;
        }

        private static bool TryAcquireReloadedSession(int oldSessionId)
        {
            GameSession candidate = UnityEngine.Object.FindFirstObjectByType<GameSession>();
            if (candidate == null || candidate.GetInstanceID() == oldSessionId ||
                candidate.Hud == null || candidate.Waves == null || candidate.Base == null ||
                candidate.Economy == null || candidate.Planting == null)
                return false;

            AcquireSession(candidate, true);
            return true;
        }

        private static bool TryPlantThroughProductionUi(PlantingSpot spot, PlantKind kind)
        {
            if (spot == null || spot.Occupied || Camera.main == null)
                return false;

            Vector2 screen = Camera.main.WorldToScreenPoint(spot.transform.position);
            bool accepted = _session.Planting.TryHandleScreenPress(screen);
            if (!accepted)
                return false;

            Canvas.ForceUpdateCanvases();
            RectTransform radial = _hud.RadialRoot;
            if (radial == null)
                return false;

            Button option = radial.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(b => b.gameObject.name == "PlantOption_" + kind);

            if (option == null || !option.interactable)
                return false;

            bool clicked = DispatchButtonClick(option);
            Canvas.ForceUpdateCanvases();
            return clicked && spot.Occupied && spot.Plant == kind;
        }

        private static SeedProjectile FindProjectileTargetingBoss(string objectName, Vector2 origin, float originTolerance)
        {
            SeedProjectile[] projectiles = UnityEngine.Object.FindObjectsByType<SeedProjectile>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            for (int i = 0; i < projectiles.Length; i++)
            {
                SeedProjectile projectile = projectiles[i];
                if (projectile == null || projectile.gameObject.name != objectName)
                    continue;
                if (ReadProjectileTarget(projectile) != _boss)
                    continue;
                if (Vector2.Distance(projectile.transform.position, origin) > originTolerance)
                    continue;
                return projectile;
            }

            return null;
        }

        private static void DestroyPlantAt<T>(PlantingSpot spot) where T : PlantBehaviour
        {
            T[] plants = UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < plants.Length; i++)
            {
                T plant = plants[i];
                if (plant != null && Vector2.Distance(plant.transform.position, spot.transform.position) < 0.15f)
                {
                    UnityEngine.Object.Destroy(plant.gameObject);
                    return;
                }
            }
        }

        private static bool AnyNonBossWithin(Vector2 position, float range)
        {
            for (int i = 0; i < _session.Enemies.Count; i++)
            {
                EnemyUnit enemy = _session.Enemies[i];
                if (enemy == null || enemy.Kind == EnemyKind.Boss)
                    continue;
                if (Vector2.Distance(enemy.transform.position, position) <= range)
                    return true;
            }
            return false;
        }

        private static int CountNonBossEnemies()
        {
            int count = 0;
            for (int i = 0; i < _session.Enemies.Count; i++)
            {
                EnemyUnit enemy = _session.Enemies[i];
                if (enemy != null && enemy.Kind != EnemyKind.Boss)
                    count++;
            }
            return count;
        }

        private static PlantingSpot FindSpot(string id)
        {
            return _spots.FirstOrDefault(s => s != null && s.Id == id);
        }

        private static int CountAllPlants()
        {
            return UnityEngine.Object.FindObjectsByType<PlantBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        }

        private static int CountPlantsOfType<T>() where T : PlantBehaviour
        {
            return UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        }

        private static List<EnemyUnit> SnapshotEnemies()
        {
            List<EnemyUnit> result = new List<EnemyUnit>();
            if (_session == null) return result;
            for (int i = 0; i < _session.Enemies.Count; i++)
            {
                EnemyUnit enemy = _session.Enemies[i];
                if (enemy != null)
                    result.Add(enemy);
            }
            return result;
        }

        private static bool IsWaveButtonActive()
        {
            return _waveButton != null && _waveButton.gameObject.activeInHierarchy;
        }

        private static bool DispatchButtonClick(Button button)
        {
            if (button == null || EventSystem.current == null)
                return false;

            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, button.transform.position)
            };

            return ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static GameObject FindEndPanel()
        {
            return GameObject.Find("EndPanel");
        }

        private static string FindEndPanelTitle(GameObject panel)
        {
            if (panel == null) return string.Empty;
            Text[] texts = panel.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                Text text = texts[i];
                if (text != null && text.gameObject.name == "Result")
                    return text.text;
            }
            return string.Empty;
        }

        private static Button FindRestartButton(GameObject panel)
        {
            if (panel == null) return null;
            Button[] buttons = panel.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Text text = buttons[i].GetComponentInChildren<Text>(true);
                if (text != null && text.text == "ZAGRAJ PONOWNIE")
                    return buttons[i];
            }
            return null;
        }

        private static float ReadEnemyFloat(EnemyUnit enemy, string fieldName)
        {
            return (float)RequireField(typeof(EnemyUnit), fieldName).GetValue(enemy);
        }

        private static int ReadEnemyInt(EnemyUnit enemy, string fieldName)
        {
            return (int)RequireField(typeof(EnemyUnit), fieldName).GetValue(enemy);
        }

        private static EnemyUnit ReadProjectileTarget(SeedProjectile projectile)
        {
            return (EnemyUnit)RequireField(typeof(SeedProjectile), "_target").GetValue(projectile);
        }

        private static float ReadProjectileFloat(SeedProjectile projectile, string fieldName)
        {
            return (float)RequireField(typeof(SeedProjectile), fieldName).GetValue(projectile);
        }

        private static DamageType ReadProjectileDamageType(SeedProjectile projectile)
        {
            return (DamageType)RequireField(typeof(SeedProjectile), "_type").GetValue(projectile);
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(type.FullName, fieldName);
            return field;
        }

        private static void EnsureBossAlive(string context)
        {
            if (_boss == null || _boss.IsDead || _session == null || !_session.Enemies.Contains(_boss))
                throw new InvalidOperationException("Boss lost before " + context + ".");
        }

        private static string SequenceText(List<EnemyKind> sequence)
        {
            return string.Join(">", sequence.Select(k => k.ToString()).ToArray());
        }

        private static void FinalizeSuccess()
        {
            AddFact("console.runtimeWarningCount", RuntimeWarnings.Count);
            AddFact("console.validationErrorCount", Errors.Count);
            bool success = Errors.Count == 0;
            WriteReport(success);
            LeavePlayMode(success ? 0 : 1);
            _phase = success ? Phase.Done : Phase.Failed;
        }

        private static void FailAndLeavePlayMode(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Errors.Add(message);
            WriteReport(false);
            LeavePlayMode(1);
            _phase = Phase.Failed;
        }

        private static void LeavePlayMode(int exitCode)
        {
            RestoreTiming();

            BossEndStatesValidationDriver.PlayerFrame = null;
            if (_driverObject != null)
                UnityEngine.Object.Destroy(_driverObject);

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

        private static void RestoreTiming()
        {
            if (!_initialized) return;
            Time.timeScale = _originalTimeScale;
            Time.maximumDeltaTime = _originalMaximumDeltaTime;
        }

        private static void WriteReport(bool success)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");
            List<string> lines = new List<string>
            {
                "DYRYGENT UNITY EXECUTOR T0006 BOSS WIN LOSE RESTART",
                "timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "unityVersion=" + Application.unityVersion,
                "status=" + (success ? "PASS" : "FAIL"),
                "playModeEntered=" + SessionState.GetBool(PlayModeEnteredKey, false),
                "androidBuildRun=False",
                "t0007LayoutTestsRun=False"
            };

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

        private static void Check(bool condition, string message)
        {
            if (!condition)
                Errors.Add(message);
        }

        private static void CheckApprox(float actual, float expected, float tolerance, string label)
        {
            if (Mathf.Abs(actual - expected) > tolerance)
                Errors.Add(label + " expected=" + F(expected) + " actual=" + F(actual) +
                           " tolerance=" + F(tolerance));
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

        private static void ResetStaticState()
        {
            Facts.Clear();
            Errors.Clear();
            RuntimeWarnings.Clear();
            Wave6SeenIds.Clear();
            Wave6Observed.Clear();
            DefeatLastProgress.Clear();
            DefeatKinds.Clear();
            DefeatPreviousIds.Clear();
            DefeatBaseTransitions.Clear();
            DefeatWavesStarted.Clear();

            _initialized = false;
            _warmupFrames = 0;
            _session = null;
            _hud = null;
            _waveButton = null;
            _enemyText = null;
            _waveText = null;
            _driverObject = null;
            _spots = new List<PlantingSpot>();
            _boss = null;
            _bossInstanceId = 0;
            _bossDefeatedByProduction = false;
            _victoryRestartClicked = false;
            _victoryResetVerified = false;
            _victoryRestartWave1Proof = false;
            _defeatRestartClicked = false;
            _defeatResetVerified = false;
            _finalWave1Proof = false;
            _reloadWarmup = 0;
            _oldSessionIdWaitingForReload = 0;
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
