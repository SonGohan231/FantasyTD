#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    [InitializeOnLoad]
    internal static class T0012PlayablePlayModeValidator
    {
        private const string ActiveKey = "FantasyTD.T0012.Active";
        private const string PhaseKey = "FantasyTD.T0012.Phase";
        private const string StartedKey = "FantasyTD.T0012.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.T0012.ExitCode";
        private const string ReportPath = "Artifacts/UnityExecutor/t0012-playable.txt";
        private const float TimeoutSeconds = 240f;

        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> Warnings = new List<string>();

        private static bool _logSubscribed;
        private static bool _initialized;
        private static int _warmupFrames;
        private static int _stage;
        private static int _stageFrames;

        private static GameSession _session;
        private static PlantingSystem _planting;
        private static MobileHud _hud;
        private static PlantingSpot _spot;
        private static PlantInstance _plant;
        private static EnemyUnit _enemy;

        private static int _essenceAtStart;
        private static int _essenceAfterBuild;
        private static int _essenceAfterUpgrade;
        private static int _essenceAfterSell;
        private static int _expectedUpgradeCost;
        private static int _expectedSellValue;
        private static float _enemyHpBefore;
        private static float _enemyHpAfter;
        private static float _hpFillScaleBefore;
        private static float _hpFillScaleAfter;

        private static bool _allRequiredHiggsfieldAssets;
        private static bool _mapBackgroundPresent;
        private static bool _buildWorked;
        private static bool _towerPanelOpened;
        private static bool _upgradeWorked;
        private static bool _sellWorked;
        private static bool _enemyHiggsfieldVisual;
        private static bool _enemyHpBarWorked;

        static T0012PlayablePlayModeValidator()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            if (SessionState.GetBool(ActiveKey, false))
                SubscribeLog();
        }

        internal static void Start()
        {
            ResetState();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");
            SessionState.SetBool(ActiveKey, true);
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
                Finish(false, false);
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
                return;

            if (TimedOut())
            {
                Errors.Add("timeout=true");
                Finish(false, EditorApplication.isPlaying);
                return;
            }

            string phase = SessionState.GetString(PhaseKey, "enter-play");
            if (phase == "exit-editor" &&
                !EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int code = SessionState.GetInt(ExitCodeKey, 1);
                SessionState.SetBool(ActiveKey, false);
                UnsubscribeLog();
                EditorApplication.Exit(code);
                return;
            }

            if (!EditorApplication.isPlaying)
                return;

            SubscribeLog();

            if (!_initialized)
            {
                _warmupFrames++;
                if (_warmupFrames < 6)
                    return;

                _session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if (_session == null || _session.Planting == null || _session.Hud == null || Camera.main == null)
                    return;

                _planting = _session.Planting;
                _hud = _session.Hud;
                _spot = UnityEngine.Object.FindObjectsByType<PlantingSpot>(
                        FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(s => s.Id == "A1");

                if (_spot == null)
                {
                    Errors.Add("missingSpot=A1");
                    Finish(false, true);
                    return;
                }

                _initialized = true;
                _stage = 0;
                _stageFrames = 0;
                SessionState.SetString(PhaseKey, "validating");
            }

            _stageFrames++;

            try
            {
                RunStage();
            }
            catch (Exception ex)
            {
                Errors.Add("validatorException=" + ex.GetType().Name + ": " + ex.Message);
                Finish(false, true);
            }
        }

        private static void RunStage()
        {
            switch (_stage)
            {
                case 0:
                    ValidateHiggsfieldReadiness();
                    if (!_allRequiredHiggsfieldAssets)
                    {
                        Finish(false, true);
                        return;
                    }
                    _essenceAtStart = _session.Economy.Essence;
                    Vector2 buildScreen = Camera.main.WorldToScreenPoint(_spot.transform.position);
                    bool accepted = _planting.TryHandleScreenPress(buildScreen);
                    Canvas.ForceUpdateCanvases();
                    Button seed = FindButton("PlantOption_" + PlantKind.SeedShooter);
                    if (!accepted || seed == null || !seed.interactable)
                    {
                        Errors.Add("buildMenuUnavailable=true");
                        Finish(false, true);
                        return;
                    }
                    seed.onClick.Invoke();
                    NextStage();
                    break;

                case 1:
                    if (_stageFrames < 2) return;
                    _plant = _spot.Instance;
                    _essenceAfterBuild = _session.Economy.Essence;
                    _buildWorked =
                        _spot.Occupied &&
                        _spot.Plant == PlantKind.SeedShooter &&
                        _plant != null &&
                        _plant.Level == 1 &&
                        _essenceAfterBuild == _essenceAtStart - Balance.Cost(PlantKind.SeedShooter) &&
                        HasHiggsfieldVisual(_plant.gameObject);

                    if (!_buildWorked)
                    {
                        Errors.Add("buildFlowFailed=true");
                        Finish(false, true);
                        return;
                    }

                    Vector2 selectScreen = Camera.main.WorldToScreenPoint(_spot.transform.position);
                    _planting.TryHandleScreenPress(selectScreen);
                    Canvas.ForceUpdateCanvases();
                    NextStage();
                    break;

                case 2:
                    if (_stageFrames < 2) return;
                    GameObject panel = GameObject.Find("TowerActionPanel");
                    Button upgrade = FindGlobalButton("TowerUpgradeButton");
                    Button sell = FindGlobalButton("TowerSellButton");
                    _towerPanelOpened =
                        panel != null &&
                        upgrade != null &&
                        sell != null &&
                        _planting.SelectedSpot == _spot;

                    if (!_towerPanelOpened)
                    {
                        Errors.Add("towerPanelOpenFailed=true");
                        Finish(false, true);
                        return;
                    }

                    _expectedUpgradeCost = _plant.UpgradeCost;
                    if (!upgrade.interactable)
                    {
                        Errors.Add("upgradeButtonNotInteractable=true");
                        Finish(false, true);
                        return;
                    }

                    upgrade.onClick.Invoke();
                    NextStage();
                    break;

                case 3:
                    if (_stageFrames < 2) return;
                    _essenceAfterUpgrade = _session.Economy.Essence;
                    _upgradeWorked =
                        _plant != null &&
                        _plant.Level == 2 &&
                        _plant.TotalInvested == Balance.Cost(PlantKind.SeedShooter) + _expectedUpgradeCost &&
                        _essenceAfterUpgrade == _essenceAfterBuild - _expectedUpgradeCost;

                    if (!_upgradeWorked)
                    {
                        Errors.Add("upgradeFlowFailed=true");
                        Finish(false, true);
                        return;
                    }

                    _expectedSellValue = _plant.SellValue;
                    Button sellAfterUpgrade = FindGlobalButton("TowerSellButton");
                    if (sellAfterUpgrade == null)
                    {
                        Errors.Add("sellButtonMissingAfterUpgrade=true");
                        Finish(false, true);
                        return;
                    }

                    sellAfterUpgrade.onClick.Invoke();
                    NextStage();
                    break;

                case 4:
                    if (_stageFrames < 3) return;
                    _essenceAfterSell = _session.Economy.Essence;
                    _sellWorked =
                        !_spot.Occupied &&
                        _spot.Instance == null &&
                        _essenceAfterSell == _essenceAfterUpgrade + _expectedSellValue;

                    if (!_sellWorked)
                    {
                        Errors.Add("sellFlowFailed=true");
                        Finish(false, true);
                        return;
                    }

                    _enemy = _session.SpawnEnemy(EnemyKind.Wanderer);
                    _enemyHiggsfieldVisual = _enemy != null && HasHiggsfieldVisual(_enemy.gameObject);
                    if (!_enemyHiggsfieldVisual)
                    {
                        Errors.Add("enemyHiggsfieldVisualMissing=true");
                        Finish(false, true);
                        return;
                    }

                    _enemyHpBefore = _enemy.CurrentHp;
                    Transform fill = _enemy.transform.Find("HpBarFill");
                    _hpFillScaleBefore = fill != null ? fill.localScale.x : -1f;
                    _enemy.TakeDamage(22f, DamageType.Physical);
                    _enemyHpAfter = _enemy.CurrentHp;
                    _hpFillScaleAfter = fill != null ? fill.localScale.x : -1f;
                    NextStage();
                    break;

                case 5:
                    if (_stageFrames < 2) return;
                    _enemyHpBarWorked =
                        _enemy != null &&
                        _enemyHpAfter < _enemyHpBefore &&
                        _hpFillScaleBefore > 0f &&
                        _hpFillScaleAfter > 0f &&
                        _hpFillScaleAfter < _hpFillScaleBefore;

                    if (!_enemyHpBarWorked)
                        Errors.Add("enemyHpBarDidNotUpdate=true");

                    bool success =
                        _allRequiredHiggsfieldAssets &&
                        _mapBackgroundPresent &&
                        _buildWorked &&
                        _towerPanelOpened &&
                        _upgradeWorked &&
                        _sellWorked &&
                        _enemyHiggsfieldVisual &&
                        _enemyHpBarWorked &&
                        Errors.Count == 0;

                    Finish(success, true);
                    break;
            }
        }

        private static void ValidateHiggsfieldReadiness()
        {
            _mapBackgroundPresent = HiggsfieldArt.MapBackground() != null;

            bool plants =
                HiggsfieldArt.Plant(PlantKind.SeedShooter) != null &&
                HiggsfieldArt.Plant(PlantKind.EssenceFlower) != null &&
                HiggsfieldArt.Plant(PlantKind.OakGuardian) != null &&
                HiggsfieldArt.Plant(PlantKind.FrostBud) != null;

            bool enemies =
                HiggsfieldArt.Enemy(EnemyKind.Wanderer) != null &&
                HiggsfieldArt.Enemy(EnemyKind.Runner) != null &&
                HiggsfieldArt.Enemy(EnemyKind.Brute) != null &&
                HiggsfieldArt.Enemy(EnemyKind.Armored) != null &&
                HiggsfieldArt.Enemy(EnemyKind.Boss) != null;

            bool environment =
                HiggsfieldArt.SpawnPortal() != null &&
                HiggsfieldArt.HomeTree() != null;

            _allRequiredHiggsfieldAssets =
                _mapBackgroundPresent && plants && enemies && environment;

            if (!_mapBackgroundPresent) Errors.Add("assetMissing=MapBackground");
            if (HiggsfieldArt.Plant(PlantKind.SeedShooter) == null) Errors.Add("assetMissing=Pedostrzal");
            if (HiggsfieldArt.Plant(PlantKind.EssenceFlower) == null) Errors.Add("assetMissing=KwiatEsencji");
            if (HiggsfieldArt.Plant(PlantKind.OakGuardian) == null) Errors.Add("assetMissing=DebowyStraznik");
            if (HiggsfieldArt.Plant(PlantKind.FrostBud) == null) Errors.Add("assetMissing=MroznyPak");
            if (HiggsfieldArt.Enemy(EnemyKind.Wanderer) == null) Errors.Add("assetMissing=Chochlik");
            if (HiggsfieldArt.Enemy(EnemyKind.Runner) == null) Errors.Add("assetMissing=WichrowySkrzat");
            if (HiggsfieldArt.Enemy(EnemyKind.Brute) == null) Errors.Add("assetMissing=OgrZarosli");
            if (HiggsfieldArt.Enemy(EnemyKind.Armored) == null) Errors.Add("assetMissing=ZelaznyGoblin");
            if (HiggsfieldArt.Enemy(EnemyKind.Boss) == null) Errors.Add("assetMissing=KrolCierni");
            if (HiggsfieldArt.SpawnPortal() == null) Errors.Add("assetMissing=SpawnPortal");
            if (HiggsfieldArt.HomeTree() == null) Errors.Add("assetMissing=HomeTree");
        }

        private static bool HasHiggsfieldVisual(GameObject root)
        {
            if (root == null) return false;
            Transform visual = root.transform.Find("HiggsfieldVisual");
            SpriteRenderer renderer = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
            return renderer != null && renderer.sprite != null;
        }

        private static Button FindButton(string name)
        {
            if (_hud == null || _hud.RadialRoot == null) return null;
            return _hud.RadialRoot.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(b => b.gameObject.name == name);
        }

        private static Button FindGlobalButton(string name)
        {
            return UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(b => b.gameObject.name == name);
        }

        private static void NextStage()
        {
            _stage++;
            _stageFrames = 0;
        }

        private static void Finish(bool success, bool playModeEntered)
        {
            WriteReport(success, playModeEntered);
            SessionState.SetInt(ExitCodeKey, success ? 0 : 1);
            SessionState.SetString(PhaseKey, "exit-editor");
            UnsubscribeLog();

            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.ExitPlaymode();
            else
            {
                SessionState.SetBool(ActiveKey, false);
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static void WriteReport(bool success, bool playModeEntered)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DYRYGENT UNITY EXECUTOR T0012 HIGGSFIELD PLAYABLE SYSTEMS");
            sb.AppendLine("timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            sb.AppendLine("unityVersion=" + Application.unityVersion);
            sb.AppendLine("status=" + (success ? "PASS" : "FAIL"));
            sb.AppendLine("playModeEntered=" + playModeEntered);
            sb.AppendLine("allRequiredHiggsfieldAssets=" + _allRequiredHiggsfieldAssets);
            sb.AppendLine("mapBackgroundPresent=" + _mapBackgroundPresent);
            sb.AppendLine("buildWorked=" + _buildWorked);
            sb.AppendLine("towerPanelOpened=" + _towerPanelOpened);
            sb.AppendLine("upgradeWorked=" + _upgradeWorked);
            sb.AppendLine("sellWorked=" + _sellWorked);
            sb.AppendLine("enemyHiggsfieldVisual=" + _enemyHiggsfieldVisual);
            sb.AppendLine("enemyHpBarWorked=" + _enemyHpBarWorked);
            sb.AppendLine("essenceAtStart=" + _essenceAtStart);
            sb.AppendLine("essenceAfterBuild=" + _essenceAfterBuild);
            sb.AppendLine("upgradeCost=" + _expectedUpgradeCost);
            sb.AppendLine("essenceAfterUpgrade=" + _essenceAfterUpgrade);
            sb.AppendLine("sellValue=" + _expectedSellValue);
            sb.AppendLine("essenceAfterSell=" + _essenceAfterSell);
            sb.AppendLine("enemyHpBefore=" + _enemyHpBefore.ToString("0.###", CultureInfo.InvariantCulture));
            sb.AppendLine("enemyHpAfter=" + _enemyHpAfter.ToString("0.###", CultureInfo.InvariantCulture));
            sb.AppendLine("hpFillScaleBefore=" + _hpFillScaleBefore.ToString("0.###", CultureInfo.InvariantCulture));
            sb.AppendLine("hpFillScaleAfter=" + _hpFillScaleAfter.ToString("0.###", CultureInfo.InvariantCulture));
            sb.AppendLine("runtimeWarningCount=" + Warnings.Count);
            for (int i = 0; i < Warnings.Count; i++)
                sb.AppendLine("runtimeWarning." + (i + 1) + "=" + Sanitize(Warnings[i]));
            sb.AppendLine("validationErrorCount=" + Errors.Count);
            for (int i = 0; i < Errors.Count; i++)
                sb.AppendLine("validationError." + (i + 1) + "=" + Sanitize(Errors[i]));

            File.WriteAllText(ReportPath, sb.ToString());
            Debug.Log(sb.ToString());
        }

        private static bool TimedOut()
        {
            string raw = SessionState.GetString(StartedKey, string.Empty);
            DateTime started;
            if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out started))
                return false;
            return (DateTime.UtcNow - started).TotalSeconds > TimeoutSeconds;
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
            if (type == LogType.Warning)
                Warnings.Add(condition);
            else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Errors.Add("console." + type + "=" + condition);
        }

        private static string Sanitize(string value)
        {
            return (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        }

        private static void ResetState()
        {
            Errors.Clear();
            Warnings.Clear();
            _logSubscribed = false;
            _initialized = false;
            _warmupFrames = 0;
            _stage = 0;
            _stageFrames = 0;
            _session = null;
            _planting = null;
            _hud = null;
            _spot = null;
            _plant = null;
            _enemy = null;
            _essenceAtStart = 0;
            _essenceAfterBuild = 0;
            _essenceAfterUpgrade = 0;
            _essenceAfterSell = 0;
            _expectedUpgradeCost = 0;
            _expectedSellValue = 0;
            _enemyHpBefore = 0f;
            _enemyHpAfter = 0f;
            _hpFillScaleBefore = 0f;
            _hpFillScaleAfter = 0f;
            _allRequiredHiggsfieldAssets = false;
            _mapBackgroundPresent = false;
            _buildWorked = false;
            _towerPanelOpened = false;
            _upgradeWorked = false;
            _sellWorked = false;
            _enemyHiggsfieldVisual = false;
            _enemyHpBarWorked = false;
        }
    }
}
#endif
