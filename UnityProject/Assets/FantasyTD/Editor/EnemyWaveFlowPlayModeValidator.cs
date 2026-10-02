#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    internal static class EnemyWaveFlowPlayModeValidator
    {
        private const string ActiveKey = "FantasyTD.T0005.Active";
        private const string PhaseKey = "FantasyTD.T0005.Phase";
        private const string StartedKey = "FantasyTD.T0005.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.T0005.ExitCode";
        private const string PlayModeEnteredKey = "FantasyTD.T0005.PlayModeEntered";
        private const string ReportPath = "Artifacts/UnityExecutor/enemy-wave-flow.txt";
        private const float TimeoutSeconds = 600f;
        private const float ValidationTimeScale = 16f;
        private const float ValidationMaximumDeltaTime = 0.20f;
        private const float SpeedSampleGameSeconds = 0.50f;

        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> Facts = new List<string>();
        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> RuntimeWarnings = new List<string>();
        private static readonly HashSet<int> SeenEnemyIds = new HashSet<int>();
        private static readonly Dictionary<int, EnemyTrack> Tracks = new Dictionary<int, EnemyTrack>();
        private static readonly Dictionary<EnemyKind, KindRecord> KindRecords = new Dictionary<EnemyKind, KindRecord>();
        private static readonly Dictionary<int, WaveRecord> WaveRecords = new Dictionary<int, WaveRecord>();

        private static bool _logSubscribed;
        private static int _warmupFrames;
        private static GameSession _session;
        private static MobileHud _hud;
        private static Button _waveButton;
        private static Text _enemyText;
        private static Text _waveText;
        private static GameObject _driverObject;
        private static int _activeWave;
        private static int _nextWaveToStart;
        private static int _totalObservedSpawns;
        private static int _controlledDeathCount;
        private static bool _bossObserved;
        private static bool _initialized;
        private static float _originalTimeScale;
        private static float _originalMaximumDeltaTime;

        private enum TrackRole
        {
            Reward,
            Leak,
            Cleanup
        }

        private sealed class EnemyTrack
        {
            public int Id;
            public EnemyKind Kind;
            public EnemyUnit Enemy;
            public TrackRole Role;
            public int Wave;
            public int BornFrame;
            public float SampleStartTime;
            public float SampleStartProgress;
            public bool SpeedMeasured;
            public bool Resolved;
        }

        private sealed class KindRecord
        {
            public EnemyKind Kind;
            public string DisplayName;
            public float ExpectedHp;
            public float ExpectedSpeed;
            public int ExpectedReward;
            public int ExpectedBaseDamage;
            public float ExpectedResistance;

            public bool RewardAssigned;
            public bool RewardDone;
            public bool LeakAssigned;
            public bool LeakDone;
            public bool SpawnedThroughWave;

            public float HpInitial;
            public float SpeedConfigured;
            public float SpeedMeasured;
            public int RewardConfigured;
            public int BaseDamageConfigured;
            public float ResistanceConfigured;
            public float RewardStartProgress;
            public int RewardEssenceBefore;
            public int RewardEssenceAfter;
            public int RewardDelta;

            public float LeakStartProgress;
            public float LeakLastProgress;
            public float LeakStartX;
            public float LeakStartY;
            public int LeakBaseHpBefore;
            public int LeakBaseHpAfter;
            public int LeakDelta;
            public bool MovementReachedBase;

            public float PhysicalRawDamage;
            public float PhysicalHpBefore;
            public float PhysicalHpAfter;
            public float PhysicalFinalDamage;
        }

        private sealed class WaveRecord
        {
            public int Wave;
            public EnemyKind[] Expected;
            public readonly List<EnemyKind> Observed = new List<EnemyKind>();
            public bool ButtonReadyBefore;
            public bool ButtonActiveBefore;
            public bool ButtonInteractableBefore;
            public bool ClickDispatched;
            public bool ButtonReadyDuring;
            public bool ButtonActiveDuring;
            public bool ButtonReadyAfter;
            public bool ButtonActiveAfter;
            public int HudMaxEnemyCount;
            public bool HudPositiveSeen;
            public string HudFinalText;
            public string WaveTextAfter;
            public bool Complete;
        }

        static EnemyWaveFlowPlayModeValidator()
        {
            EditorApplication.update -= EditorTick;
            EditorApplication.update += EditorTick;
            if (SessionState.GetBool(ActiveKey, false))
                SubscribeLog();
        }

        internal static void Start()
        {
            ResetEditorState();
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

            _session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
            if (_session == null || _session.Waves == null || _session.Economy == null || _session.Base == null || _session.Hud == null)
                return;

            try
            {
                InitializeContext();
                _initialized = true;
                SessionState.SetString(PhaseKey, "validating");
            }
            catch (Exception ex)
            {
                FailAndLeavePlayMode("initializeException=" + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static void InitializeContext()
        {
            Facts.Clear();
            Errors.Clear();
            RuntimeWarnings.Clear();
            SeenEnemyIds.Clear();
            Tracks.Clear();
            KindRecords.Clear();
            WaveRecords.Clear();

            _hud = _session.Hud;
            _waveButton = (Button)RequireField(typeof(MobileHud), "_waveButton").GetValue(_hud);
            _enemyText = (Text)RequireField(typeof(MobileHud), "_enemyText").GetValue(_hud);
            _waveText = (Text)RequireField(typeof(MobileHud), "_waveText").GetValue(_hud);

            Check(_waveButton != null, "HUD wave button missing");
            Check(_enemyText != null, "HUD enemy text missing");
            Check(_waveText != null, "HUD wave text missing");

            InitializeKindRecords();
            InitializeWaveRecords();

            _activeWave = 0;
            _nextWaveToStart = 1;
            _totalObservedSpawns = 0;
            _controlledDeathCount = 0;
            _bossObserved = false;

            _originalTimeScale = Time.timeScale;
            _originalMaximumDeltaTime = Time.maximumDeltaTime;
            Time.timeScale = ValidationTimeScale;
            Time.maximumDeltaTime = ValidationMaximumDeltaTime;

            AddFact("validationTimeScale", ValidationTimeScale);
            AddFact("validationMaximumDeltaTime", ValidationMaximumDeltaTime);
            AddFact("pathLengthRU", MapAData.TotalPathLength);
            AddFact("initialBaseHp", _session.Base.Hp);
            AddFact("initialEssence", _session.Economy.Essence);
            AddFact("initialWave", _session.Waves.CurrentWave);
            AddFact("initialReadyForNextWave", _session.Waves.ReadyForNextWave);
            AddFact("initialHudEnemyText", _enemyText != null ? _enemyText.text : "null");
            AddFact("initialWaveButtonActive", IsWaveButtonActive());

            Check(_session.Base.Hp == 20, "expected initial base HP 20");
            Check(_session.Economy.Essence == 300, "expected initial Essence 300");
            Check(_session.Waves.CurrentWave == 0, "expected CurrentWave 0 before wave1");
            Check(_session.Waves.ReadyForNextWave, "wave1 not ready initially");
            Check(!_session.Waves.Running, "WaveDirector unexpectedly running before T0005");
            Check(_session.Enemies.Count == 0, "expected zero initial enemies");
            ValidateHudCountNow("initial");

            _driverObject = new GameObject("T0005 Enemy Wave Flow Validation Driver");
            UnityEngine.Object.DontDestroyOnLoad(_driverObject);
            EnemyWaveFlowValidationDriver.PlayerFrame = PlayerFrame;
            _driverObject.AddComponent<EnemyWaveFlowValidationDriver>();
        }

        private static void InitializeKindRecords()
        {
            AddKindRecord(EnemyKind.Wanderer, "Chochlik", 170f, 0.80f, 12, 1, 0f);
            AddKindRecord(EnemyKind.Runner, "Wichrowy Skrzat", 110f, 1.15f, 12, 1, 0f);
            AddKindRecord(EnemyKind.Brute, "Ogr Zarośli", 550f, 0.45f, 26, 2, 0f);
            AddKindRecord(EnemyKind.Armored, "Żelazny Goblin", 260f, 0.55f, 22, 1, 0.15f);
        }

        private static void AddKindRecord(
            EnemyKind kind,
            string displayName,
            float hp,
            float speed,
            int reward,
            int baseDamage,
            float resistance)
        {
            KindRecords[kind] = new KindRecord
            {
                Kind = kind,
                DisplayName = displayName,
                ExpectedHp = hp,
                ExpectedSpeed = speed,
                ExpectedReward = reward,
                ExpectedBaseDamage = baseDamage,
                ExpectedResistance = resistance
            };
        }

        private static void InitializeWaveRecords()
        {
            WaveRecords[1] = NewWave(1,
                EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer,
                EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer);

            WaveRecords[2] = NewWave(2,
                EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer,
                EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner);

            WaveRecords[3] = NewWave(3,
                EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer,
                EnemyKind.Armored, EnemyKind.Armored, EnemyKind.Armored, EnemyKind.Armored);

            WaveRecords[4] = NewWave(4,
                EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner,
                EnemyKind.Brute, EnemyKind.Brute, EnemyKind.Brute,
                EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer, EnemyKind.Wanderer);

            WaveRecords[5] = NewWave(5,
                EnemyKind.Armored, EnemyKind.Armored, EnemyKind.Armored, EnemyKind.Armored,
                EnemyKind.Brute, EnemyKind.Brute, EnemyKind.Brute,
                EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner,
                EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner, EnemyKind.Runner);
        }

        private static WaveRecord NewWave(int wave, params EnemyKind[] expected)
        {
            return new WaveRecord { Wave = wave, Expected = expected };
        }

        internal static void PlayerFrame()
        {
            if (!SessionState.GetBool(ActiveKey, false) || !_initialized || _session == null)
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
            List<EnemyUnit> currentEnemies = SnapshotEnemies();
            HashSet<int> currentIds = BuildIdSet(currentEnemies);

            DetectCompletedLeaks(currentIds);
            DetectNewSpawns(currentEnemies);
            ValidateHudCountNow("before-resolution");
            UpdateWaveHudMax();

            ProcessTracks(currentIds);

            ValidateHudCountNow("after-resolution");
            UpdateWaveHudMax();

            if (_activeWave == 0)
            {
                if (_nextWaveToStart <= 5)
                {
                    if (_session.Enemies.Count == 0 && !_session.Waves.Running && _session.Waves.ReadyForNextWave)
                        StartNextWaveThroughHud(_nextWaveToStart);
                }
                else
                {
                    FinalizeAfterWaveFive();
                }

                return;
            }

            if (!_session.Waves.Running && _session.Waves.ReadyForNextWave && _session.Enemies.Count == 0)
                CompleteActiveWave();
        }

        private static void DetectNewSpawns(List<EnemyUnit> currentEnemies)
        {
            for (int i = 0; i < currentEnemies.Count; i++)
            {
                EnemyUnit enemy = currentEnemies[i];
                if (enemy == null) continue;

                int id = enemy.GetInstanceID();
                if (!SeenEnemyIds.Add(id))
                    continue;

                int wave = _session.Waves.CurrentWave;
                _totalObservedSpawns++;

                if (wave < 1 || wave > 5)
                {
                    Errors.Add("enemy spawned outside T0005 waves1-5: kind=" + enemy.Kind + " wave=" + wave);
                    if (enemy.Kind == EnemyKind.Boss) _bossObserved = true;
                    continue;
                }

                if (!WaveRecords.ContainsKey(wave))
                {
                    Errors.Add("missing WaveRecord for observed wave " + wave);
                    continue;
                }

                WaveRecord waveRecord = WaveRecords[wave];
                waveRecord.Observed.Add(enemy.Kind);

                if (enemy.Kind == EnemyKind.Boss)
                {
                    _bossObserved = true;
                    Errors.Add("Boss spawned during T0005 waves1-5");
                    continue;
                }

                KindRecord kindRecord;
                if (!KindRecords.TryGetValue(enemy.Kind, out kindRecord))
                {
                    Errors.Add("unexpected enemy kind during T0005: " + enemy.Kind);
                    continue;
                }

                kindRecord.SpawnedThroughWave = true;

                EnemyTrack track = new EnemyTrack
                {
                    Id = id,
                    Kind = enemy.Kind,
                    Enemy = enemy,
                    Wave = wave,
                    BornFrame = Time.frameCount
                };

                if (!kindRecord.RewardAssigned)
                {
                    kindRecord.RewardAssigned = true;
                    track.Role = TrackRole.Reward;
                    track.SampleStartTime = Time.time;
                    track.SampleStartProgress = enemy.ProgressDistance;

                    kindRecord.HpInitial = ReadEnemyFloat(enemy, "_hp");
                    kindRecord.SpeedConfigured = ReadEnemyFloat(enemy, "_speed");
                    kindRecord.RewardConfigured = ReadEnemyInt(enemy, "_reward");
                    kindRecord.BaseDamageConfigured = ReadEnemyInt(enemy, "_baseDamage");
                    kindRecord.ResistanceConfigured = ReadEnemyFloat(enemy, "_physicalResistance");
                    kindRecord.RewardStartProgress = enemy.ProgressDistance;
                }
                else if (!kindRecord.LeakAssigned)
                {
                    kindRecord.LeakAssigned = true;
                    track.Role = TrackRole.Leak;

                    kindRecord.LeakStartProgress = enemy.ProgressDistance;
                    kindRecord.LeakLastProgress = enemy.ProgressDistance;
                    kindRecord.LeakStartX = enemy.transform.position.x;
                    kindRecord.LeakStartY = enemy.transform.position.y;
                    kindRecord.LeakBaseHpBefore = _session.Base.Hp;
                }
                else
                {
                    track.Role = TrackRole.Cleanup;
                }

                Tracks[id] = track;

                ValidateHudCountNow("spawn-" + wave + "-" + enemy.Kind);
                if (_session.Enemies.Count > 0)
                    waveRecord.HudPositiveSeen = true;
            }
        }

        private static void ProcessTracks(HashSet<int> currentIds)
        {
            List<EnemyTrack> snapshot = new List<EnemyTrack>(Tracks.Values);
            for (int i = 0; i < snapshot.Count; i++)
            {
                EnemyTrack track = snapshot[i];
                if (track.Resolved) continue;
                if (!currentIds.Contains(track.Id)) continue;
                if (track.Enemy == null) continue;

                KindRecord kindRecord = KindRecords[track.Kind];

                if (track.Role == TrackRole.Leak)
                {
                    kindRecord.LeakLastProgress = track.Enemy.ProgressDistance;
                    continue;
                }

                if (track.Role == TrackRole.Cleanup)
                {
                    if (Time.frameCount <= track.BornFrame)
                        continue;

                    track.Enemy.TakeDamage(99999f, DamageType.Root);
                    track.Resolved = true;
                    _controlledDeathCount++;
                    continue;
                }

                if (!track.SpeedMeasured)
                {
                    float elapsed = Time.time - track.SampleStartTime;
                    if (elapsed < SpeedSampleGameSeconds)
                        continue;

                    float progressDelta = track.Enemy.ProgressDistance - track.SampleStartProgress;
                    kindRecord.SpeedMeasured = progressDelta / Mathf.Max(0.001f, elapsed);
                    track.SpeedMeasured = true;

                    CheckApprox(kindRecord.SpeedMeasured, kindRecord.ExpectedSpeed, 0.08f,
                        kindRecord.DisplayName + " measured speed");
                    CheckApprox(kindRecord.SpeedConfigured, kindRecord.ExpectedSpeed, 0.001f,
                        kindRecord.DisplayName + " configured speed");
                }

                if (!track.SpeedMeasured || kindRecord.RewardDone)
                    continue;

                if (track.Kind == EnemyKind.Armored)
                {
                    kindRecord.PhysicalRawDamage = 100f;
                    kindRecord.PhysicalHpBefore = ReadEnemyFloat(track.Enemy, "_hp");
                    track.Enemy.TakeDamage(100f, DamageType.Physical);
                    kindRecord.PhysicalHpAfter = ReadEnemyFloat(track.Enemy, "_hp");
                    kindRecord.PhysicalFinalDamage = kindRecord.PhysicalHpBefore - kindRecord.PhysicalHpAfter;

                    CheckApprox(kindRecord.PhysicalHpBefore, 260f, 0.05f, "Armored HP before physical resistance test");
                    CheckApprox(kindRecord.PhysicalHpAfter, 175f, 0.05f, "Armored HP after raw100 physical");
                    CheckApprox(kindRecord.PhysicalFinalDamage, 85f, 0.05f, "Armored final physical damage");
                    CheckApprox(kindRecord.ResistanceConfigured, 0.15f, 0.001f, "Armored physical resistance");
                }

                kindRecord.RewardEssenceBefore = _session.Economy.Essence;
                track.Enemy.TakeDamage(99999f, DamageType.Root);
                kindRecord.RewardEssenceAfter = _session.Economy.Essence;
                kindRecord.RewardDelta = kindRecord.RewardEssenceAfter - kindRecord.RewardEssenceBefore;
                kindRecord.RewardDone = true;
                track.Resolved = true;
                _controlledDeathCount++;

                Check(kindRecord.RewardDelta == kindRecord.ExpectedReward,
                    kindRecord.DisplayName + " reward delta expected=" + kindRecord.ExpectedReward +
                    " actual=" + kindRecord.RewardDelta);
            }
        }

        private static void DetectCompletedLeaks(HashSet<int> currentIds)
        {
            foreach (KeyValuePair<EnemyKind, KindRecord> pair in KindRecords)
            {
                KindRecord record = pair.Value;
                if (!record.LeakAssigned || record.LeakDone)
                    continue;

                EnemyTrack leakTrack = null;
                foreach (EnemyTrack candidate in Tracks.Values)
                {
                    if (candidate.Kind == record.Kind && candidate.Role == TrackRole.Leak)
                    {
                        leakTrack = candidate;
                        break;
                    }
                }

                if (leakTrack == null || currentIds.Contains(leakTrack.Id))
                    continue;

                record.LeakBaseHpAfter = _session.Base.Hp;
                record.LeakDelta = record.LeakBaseHpBefore - record.LeakBaseHpAfter;
                record.LeakDone = true;
                leakTrack.Resolved = true;

                float remaining = MapAData.TotalPathLength - record.LeakLastProgress;
                record.MovementReachedBase =
                    record.LeakDelta == record.ExpectedBaseDamage &&
                    remaining >= -0.05f &&
                    remaining <= 0.50f;

                Check(record.LeakDelta == record.ExpectedBaseDamage,
                    record.DisplayName + " leak base damage expected=" + record.ExpectedBaseDamage +
                    " actual=" + record.LeakDelta);
                Check(record.MovementReachedBase,
                    record.DisplayName + " natural leak did not provide full spawn-to-base evidence; lastProgress=" +
                    F(record.LeakLastProgress) + " path=" + F(MapAData.TotalPathLength));
            }
        }

        private static void StartNextWaveThroughHud(int expectedWave)
        {
            WaveRecord record = WaveRecords[expectedWave];

            record.ButtonReadyBefore = _session.Waves.ReadyForNextWave;
            record.ButtonActiveBefore = IsWaveButtonActive();
            record.ButtonInteractableBefore = _waveButton != null && _waveButton.interactable;

            Check(record.ButtonReadyBefore, "wave " + expectedWave + " ReadyForNextWave false before click");
            Check(record.ButtonActiveBefore, "wave " + expectedWave + " button inactive before click");
            Check(record.ButtonInteractableBefore, "wave " + expectedWave + " button not interactable before click");

            record.ClickDispatched = DispatchWaveButtonClick();
            Check(record.ClickDispatched, "wave " + expectedWave + " HUD pointer click dispatch failed");

            _activeWave = expectedWave;
            _nextWaveToStart = expectedWave + 1;

            record.ButtonReadyDuring = _session.Waves.ReadyForNextWave;
            record.ButtonActiveDuring = IsWaveButtonActive();

            Check(_session.Waves.CurrentWave == expectedWave,
                "after click expected CurrentWave=" + expectedWave + " actual=" + _session.Waves.CurrentWave);
            Check(_session.Waves.Running, "wave " + expectedWave + " did not enter Running state after HUD click");
            Check(!record.ButtonReadyDuring, "wave " + expectedWave + " remained ready while running");
            Check(!record.ButtonActiveDuring, "wave " + expectedWave + " button remained active while running");

            ValidateHudCountNow("after-wave-click-" + expectedWave);
        }

        private static bool DispatchWaveButtonClick()
        {
            if (_waveButton == null || EventSystem.current == null)
                return false;

            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, _waveButton.transform.position)
            };

            return ExecuteEvents.Execute(_waveButton.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static void CompleteActiveWave()
        {
            WaveRecord record = WaveRecords[_activeWave];

            record.ButtonReadyAfter = _session.Waves.ReadyForNextWave;
            record.ButtonActiveAfter = IsWaveButtonActive();
            record.HudFinalText = _enemyText != null ? _enemyText.text : "null";
            record.WaveTextAfter = _waveText != null ? _waveText.text : "null";

            Check(record.ButtonReadyAfter, "wave " + record.Wave + " not ready after completion");
            Check(record.ButtonActiveAfter, "wave " + record.Wave + " button inactive after completion");
            Check(record.HudPositiveSeen, "wave " + record.Wave + " HUD never showed positive enemy count");
            Check(record.HudFinalText == "WROGOWIE  0",
                "wave " + record.Wave + " HUD final enemy text mismatch: " + record.HudFinalText);

            ValidateWaveSequence(record);
            record.Complete = true;
            _activeWave = 0;
        }

        private static void ValidateWaveSequence(WaveRecord record)
        {
            Check(record.Observed.Count == record.Expected.Length,
                "wave " + record.Wave + " spawn count expected=" + record.Expected.Length +
                " actual=" + record.Observed.Count);

            int count = Mathf.Min(record.Observed.Count, record.Expected.Length);
            for (int i = 0; i < count; i++)
            {
                if (record.Observed[i] != record.Expected[i])
                    Errors.Add("wave " + record.Wave + " spawn order mismatch index=" + i +
                               " expected=" + record.Expected[i] + " actual=" + record.Observed[i]);
            }
        }

        private static void FinalizeAfterWaveFive()
        {
            if (!WaveRecords[5].Complete)
                return;

            foreach (KindRecord record in KindRecords.Values)
            {
                Check(record.SpawnedThroughWave, record.DisplayName + " never spawned through normal waves1-5");
                Check(record.RewardDone, record.DisplayName + " reward specimen incomplete");
                Check(record.LeakDone, record.DisplayName + " leak specimen incomplete");
                CheckApprox(record.HpInitial, record.ExpectedHp, 0.05f, record.DisplayName + " initial HP");
                Check(record.RewardConfigured == record.ExpectedReward,
                    record.DisplayName + " configured reward expected=" + record.ExpectedReward +
                    " actual=" + record.RewardConfigured);
                Check(record.BaseDamageConfigured == record.ExpectedBaseDamage,
                    record.DisplayName + " configured base damage expected=" + record.ExpectedBaseDamage +
                    " actual=" + record.BaseDamageConfigured);
                CheckApprox(record.ResistanceConfigured, record.ExpectedResistance, 0.001f,
                    record.DisplayName + " configured physical resistance");
            }

            Check(_totalObservedSpawns == 51, "waves1-5 total observed spawns expected=51 actual=" + _totalObservedSpawns);
            Check(!_bossObserved, "Boss was observed during T0005");
            Check(_controlledDeathCount == 47,
                "controlled reward/cleanup deaths expected=47 actual=" + _controlledDeathCount);
            Check(_session.Base.Hp == 15, "final base HP expected=15 actual=" + _session.Base.Hp);
            Check(_session.Economy.Essence == 1004,
                "final Essence expected=1004 actual=" + _session.Economy.Essence);
            Check(!_session.IsFinished, "session finished during T0005; T0006 state entered unexpectedly");
            Check(_session.Waves.CurrentWave == 5, "expected to stop after wave5");
            Check(_session.Waves.ReadyForNextWave, "wave6 availability should be ready after wave5");
            Check(!_session.Waves.Running, "WaveDirector still running after wave5");
            Check(_session.Enemies.Count == 0, "active enemies remain after wave5");
            ValidateHudCountNow("final");

            WriteAllEvidence();
            Complete();
        }

        private static void WriteAllEvidence()
        {
            AddFact("totalObservedSpawns", _totalObservedSpawns);
            AddFact("controlledDeathCount", _controlledDeathCount);
            AddFact("naturalLeakCount", 4);
            AddFact("bossObserved", _bossObserved);
            AddFact("finalBaseHp", _session.Base.Hp);
            AddFact("finalEssence", _session.Economy.Essence);
            AddFact("finalWave", _session.Waves.CurrentWave);
            AddFact("wave6ReadyButNotStarted", _session.Waves.CurrentWave == 5 && _session.Waves.ReadyForNextWave);
            AddFact("sessionFinished", _session.IsFinished);
            AddFact("finalHudEnemyText", _enemyText != null ? _enemyText.text : "null");
            AddFact("finalWaveText", _waveText != null ? _waveText.text : "null");

            foreach (EnemyKind kind in new[] { EnemyKind.Wanderer, EnemyKind.Runner, EnemyKind.Brute, EnemyKind.Armored })
            {
                KindRecord r = KindRecords[kind];
                string p = "enemy." + kind + ".";
                AddFact(p + "displayName", r.DisplayName);
                AddFact(p + "hpInitial", r.HpInitial);
                AddFact(p + "speedConfigured", r.SpeedConfigured);
                AddFact(p + "speedMeasured", r.SpeedMeasured);
                AddFact(p + "reward", r.RewardConfigured);
                AddFact(p + "baseDamage", r.BaseDamageConfigured);
                AddFact(p + "physicalResistance", r.ResistanceConfigured);
                AddFact(p + "rewardStartProgress", r.RewardStartProgress);
                AddFact(p + "rewardEssenceBefore", r.RewardEssenceBefore);
                AddFact(p + "rewardEssenceAfter", r.RewardEssenceAfter);
                AddFact(p + "rewardDelta", r.RewardDelta);
                AddFact(p + "leakStartProgress", r.LeakStartProgress);
                AddFact(p + "leakLastProgress", r.LeakLastProgress);
                AddFact(p + "leakStartX", r.LeakStartX);
                AddFact(p + "leakStartY", r.LeakStartY);
                AddFact(p + "leakBaseHpBefore", r.LeakBaseHpBefore);
                AddFact(p + "leakBaseHpAfter", r.LeakBaseHpAfter);
                AddFact(p + "leakDelta", r.LeakDelta);
                AddFact(p + "movementReachedBase", r.MovementReachedBase);
                AddFact(p + "rewardPass", r.RewardDone && r.RewardDelta == r.ExpectedReward);
                AddFact(p + "leakPass", r.LeakDone && r.MovementReachedBase);

                if (kind == EnemyKind.Armored)
                {
                    AddFact(p + "physicalRawDamage", r.PhysicalRawDamage);
                    AddFact(p + "physicalHpBefore", r.PhysicalHpBefore);
                    AddFact(p + "physicalHpAfter", r.PhysicalHpAfter);
                    AddFact(p + "physicalFinalDamage", r.PhysicalFinalDamage);
                }
            }

            for (int wave = 1; wave <= 5; wave++)
            {
                WaveRecord r = WaveRecords[wave];
                string p = "wave." + wave + ".";
                AddFact(p + "expectedSpawnCount", r.Expected.Length);
                AddFact(p + "observedSpawnCount", r.Observed.Count);
                AddFact(p + "expectedSequence", SequenceText(r.Expected));
                AddFact(p + "observedSequence", SequenceText(r.Observed.ToArray()));
                AddFact(p + "wandererCount", CountKind(r.Observed, EnemyKind.Wanderer));
                AddFact(p + "runnerCount", CountKind(r.Observed, EnemyKind.Runner));
                AddFact(p + "bruteCount", CountKind(r.Observed, EnemyKind.Brute));
                AddFact(p + "armoredCount", CountKind(r.Observed, EnemyKind.Armored));
                AddFact(p + "buttonReadyBefore", r.ButtonReadyBefore);
                AddFact(p + "buttonActiveBefore", r.ButtonActiveBefore);
                AddFact(p + "buttonInteractableBefore", r.ButtonInteractableBefore);
                AddFact(p + "clickDispatched", r.ClickDispatched);
                AddFact(p + "buttonReadyDuring", r.ButtonReadyDuring);
                AddFact(p + "buttonActiveDuring", r.ButtonActiveDuring);
                AddFact(p + "buttonReadyAfter", r.ButtonReadyAfter);
                AddFact(p + "buttonActiveAfter", r.ButtonActiveAfter);
                AddFact(p + "hudMaxEnemyCount", r.HudMaxEnemyCount);
                AddFact(p + "hudPositiveSeen", r.HudPositiveSeen);
                AddFact(p + "hudFinalText", r.HudFinalText);
                AddFact(p + "waveTextAfter", r.WaveTextAfter);
                AddFact(p + "complete", r.Complete);
            }
        }

        private static void UpdateWaveHudMax()
        {
            int wave = _session.Waves.CurrentWave;
            if (wave < 1 || wave > 5 || !WaveRecords.ContainsKey(wave))
                return;

            WaveRecord record = WaveRecords[wave];
            record.HudMaxEnemyCount = Mathf.Max(record.HudMaxEnemyCount, _session.Enemies.Count);
            if (_session.Enemies.Count > 0)
                record.HudPositiveSeen = true;
        }

        private static void ValidateHudCountNow(string context)
        {
            if (_enemyText == null)
            {
                Errors.Add("HUD enemy text missing at " + context);
                return;
            }

            string expected = "WROGOWIE  " + _session.Enemies.Count;
            if (_enemyText.text != expected)
                Errors.Add("HUD enemy count mismatch at " + context +
                           " expected='" + expected + "' actual='" + _enemyText.text + "'");
        }

        private static List<EnemyUnit> SnapshotEnemies()
        {
            List<EnemyUnit> result = new List<EnemyUnit>();
            for (int i = 0; i < _session.Enemies.Count; i++)
            {
                EnemyUnit enemy = _session.Enemies[i];
                if (enemy != null)
                    result.Add(enemy);
            }
            return result;
        }

        private static HashSet<int> BuildIdSet(List<EnemyUnit> enemies)
        {
            HashSet<int> ids = new HashSet<int>();
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i] != null)
                    ids.Add(enemies[i].GetInstanceID());
            return ids;
        }

        private static bool IsWaveButtonActive()
        {
            return _waveButton != null && _waveButton.gameObject.activeInHierarchy;
        }

        private static int CountKind(List<EnemyKind> kinds, EnemyKind kind)
        {
            int count = 0;
            for (int i = 0; i < kinds.Count; i++)
                if (kinds[i] == kind) count++;
            return count;
        }

        private static string SequenceText(EnemyKind[] sequence)
        {
            if (sequence == null || sequence.Length == 0) return string.Empty;
            string[] values = new string[sequence.Length];
            for (int i = 0; i < sequence.Length; i++)
                values[i] = sequence[i].ToString();
            return string.Join(">", values);
        }

        private static float ReadEnemyFloat(EnemyUnit enemy, string fieldName)
        {
            return (float)RequireField(typeof(EnemyUnit), fieldName).GetValue(enemy);
        }

        private static int ReadEnemyInt(EnemyUnit enemy, string fieldName)
        {
            return (int)RequireField(typeof(EnemyUnit), fieldName).GetValue(enemy);
        }

        private static FieldInfo RequireField(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(type.FullName, fieldName);
            return field;
        }

        private static void Complete()
        {
            bool success = Errors.Count == 0;
            WriteReport(success);
            LeavePlayMode(success ? 0 : 1);
        }

        private static void FailAndLeavePlayMode(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Errors.Add(message);
            WriteReport(false);
            LeavePlayMode(1);
        }

        private static void LeavePlayMode(int exitCode)
        {
            RestoreTiming();

            EnemyWaveFlowValidationDriver.PlayerFrame = null;
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

            List<string> lines = new List<string>();
            lines.Add("DYRYGENT UNITY EXECUTOR T0005 ENEMY RUNTIME AND WAVE FLOW");
            lines.Add("timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            lines.Add("unityVersion=" + Application.unityVersion);
            lines.Add("status=" + (success ? "PASS" : "FAIL"));
            lines.Add("playModeEntered=" + SessionState.GetBool(PlayModeEnteredKey, false));
            lines.Add("androidBuildRun=False");
            lines.Add("bossWaveStarted=False");
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

        private static void ResetEditorState()
        {
            _warmupFrames = 0;
            _session = null;
            _hud = null;
            _waveButton = null;
            _enemyText = null;
            _waveText = null;
            _driverObject = null;
            _initialized = false;
            _activeWave = 0;
            _nextWaveToStart = 1;
            _totalObservedSpawns = 0;
            _controlledDeathCount = 0;
            _bossObserved = false;
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
