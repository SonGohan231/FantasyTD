#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    [InitializeOnLoad]
    internal static class PlayableReleasePlayModeValidator
    {
        private const string ActiveKey = "FantasyTD.T0012.Active";
        private const string PhaseKey = "FantasyTD.T0012.Phase";
        private const string StartedKey = "FantasyTD.T0012.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.T0012.ExitCode";
        private const string PlayModeEnteredKey = "FantasyTD.T0012.PlayModeEntered";
        private const string ReportPath = "Artifacts/UnityExecutor/playable-release.txt";
        private const string ScreenshotPath = "Artifacts/UnityExecutor/playable-release-1920x1080.png";
        private const float TimeoutSeconds = 240f;

        private static readonly List<string> Facts = new List<string>();
        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> RuntimeWarnings = new List<string>();

        private static bool _subscribed;
        private static int _warmupFrames;
        private static GameSession _session;
        private static IEnumerator _flow;

        static PlayableReleasePlayModeValidator()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            if (SessionState.GetBool(ActiveKey, false))
                SubscribeLog();
        }

        internal static void Start()
        {
            Facts.Clear();
            Errors.Clear();
            RuntimeWarnings.Clear();
            _warmupFrames = 0;
            _session = null;
            _flow = null;

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
                FinishImmediately(false);
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
                return;

            if (TimedOut())
            {
                Errors.Add("timeout=" + TimeoutSeconds.ToString("0", CultureInfo.InvariantCulture));
                Complete(false);
                return;
            }

            string phase = SessionState.GetString(PhaseKey, "enter-play");
            if (phase == "exit-editor" &&
                !EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
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
                if (_warmupFrames < 6)
                    return;

                _session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if (_session == null ||
                    _session.Economy == null ||
                    _session.Planting == null ||
                    _session.Hud == null ||
                    _session.TowerPanel == null ||
                    Camera.main == null ||
                    EventSystem.current == null)
                    return;

                _flow = RunFlow();
                SessionState.SetString(PhaseKey, "validating");
            }

            try
            {
                if (_flow != null && !_flow.MoveNext())
                    Complete(Errors.Count == 0);
            }
            catch (Exception ex)
            {
                Errors.Add("validatorException=" + ex.GetType().Name + ": " + ex.Message);
                Complete(false);
            }
        }

        private static IEnumerator RunFlow()
        {
            Screen.SetResolution(1920, 1080, false);
            for (int i = 0; i < 8; i++) yield return null;

            AddFact("screenWidth", Screen.width);
            AddFact("screenHeight", Screen.height);
            Check(Screen.width == 1920 && Screen.height == 1080,
                "T0012 render validation did not reach 1920x1080.");

            ValidateMapArt();

            IEnumerator hp = ValidateEnemyHpAndControl();
            while (hp.MoveNext()) yield return null;

            IEnumerator tower = ValidateTowerLifecycle();
            while (tower.MoveNext()) yield return null;

            Directory.CreateDirectory(Path.GetDirectoryName(ScreenshotPath) ?? "Artifacts/UnityExecutor");
            if (File.Exists(ScreenshotPath))
                File.Delete(ScreenshotPath);

            ScreenCapture.CaptureScreenshot(ScreenshotPath);
            for (int i = 0; i < 30 && !File.Exists(ScreenshotPath); i++)
                yield return null;

            bool screenshotExists = File.Exists(ScreenshotPath) &&
                                    new FileInfo(ScreenshotPath).Length > 10000;
            AddFact("screenshotPath", ScreenshotPath);
            AddFact("screenshotExists", screenshotExists);
            Check(screenshotExists, "T0012 render screenshot was not produced.");

            AddFact("runtimeWarningCount", RuntimeWarnings.Count);
            AddFact("validationErrorCount", Errors.Count);
        }

        private static void ValidateMapArt()
        {
            Texture2D mapTexture = Resources.Load<Texture2D>("Higgsfield/MAP_A");
            GameObject grass = GameObject.Find("Grass");
            SpriteRenderer renderer = grass != null ? grass.GetComponent<SpriteRenderer>() : null;

            bool texturePresent = mapTexture != null && mapTexture.width >= 1024 && mapTexture.height >= 512;
            bool rendered = renderer != null &&
                            renderer.enabled &&
                            renderer.sprite != null &&
                            renderer.sprite.name == "Higgsfield_MAP_A";
            bool bounds = renderer != null &&
                          Mathf.Abs(renderer.bounds.min.x - 0f) <= 0.03f &&
                          Mathf.Abs(renderer.bounds.max.x - 24f) <= 0.03f &&
                          Mathf.Abs(renderer.bounds.min.y - 0f) <= 0.03f &&
                          Mathf.Abs(renderer.bounds.max.y - 14f) <= 0.03f;

            AddFact("map.texturePresent", texturePresent);
            AddFact("map.textureSize", mapTexture != null ? mapTexture.width + "x" + mapTexture.height : "missing");
            AddFact("map.higgsfieldSpriteRendered", rendered);
            AddFact("map.bounds24x14", bounds);
            AddFact("map.pathLengthRU", MapAData.TotalPathLength.ToString("0.000", CultureInfo.InvariantCulture));
            AddFact("map.spotCount", MapAData.PlantingSpots.Length);

            Check(texturePresent, "Higgsfield MAP-A texture is missing or unexpectedly small.");
            Check(rendered, "Grass renderer is not using the Higgsfield MAP-A sprite.");
            Check(bounds, "Higgsfield MAP-A renderer does not cover 24x14 logical bounds.");
            Check(MapAData.PlantingSpots.Length == 12, "T0012 requires exactly 12 planting spots.");
        }

        private static IEnumerator ValidateEnemyHpAndControl()
        {
            EnemyUnit enemy = _session.SpawnEnemy(EnemyKind.Wanderer);
            yield return null;

            EnemyHealthBar bar = enemy != null ? enemy.GetComponent<EnemyHealthBar>() : null;
            Transform fill = enemy != null ? enemy.transform.Find("HpBarFill") : null;

            float beforeHp = enemy != null ? enemy.CurrentHp : -1f;
            float maxHp = enemy != null ? enemy.MaxHp : -1f;
            float beforeScale = fill != null ? fill.localScale.x : -1f;

            Check(enemy != null, "Enemy spawn failed in T0012 HP scenario.");
            Check(bar != null, "EnemyHealthBar component missing.");
            Check(fill != null, "HpBarFill child missing.");
            CheckApprox(beforeHp, 170f, 0.01f, "Wanderer initial HP");
            CheckApprox(maxHp, 170f, 0.01f, "Wanderer max HP");

            if (enemy == null)
                yield break;

            enemy.TakeDamage(22f, DamageType.Physical);
            yield return null;

            float afterHp = enemy.CurrentHp;
            float afterScale = fill != null ? fill.localScale.x : -1f;

            AddFact("enemy.hpBefore", beforeHp);
            AddFact("enemy.hpAfter22", afterHp);
            AddFact("enemy.hpBarScaleBefore", beforeScale);
            AddFact("enemy.hpBarScaleAfter", afterScale);

            CheckApprox(afterHp, 148f, 0.05f, "Wanderer HP after 22 physical");
            Check(fill != null && afterScale < beforeScale, "HP bar did not shrink after damage.");

            enemy.ApplySlow(0.35f, 2.5f);
            yield return null;
            Check(enemy.IsSlowed, "Enemy slow state cue source did not become active.");

            enemy.ApplyRoot(1.75f);
            yield return null;
            Check(enemy.IsRooted, "Enemy root state cue source did not become active.");

            AddFact("enemy.slowPublicState", enemy.IsSlowed);
            AddFact("enemy.rootPublicState", enemy.IsRooted);

            UnityEngine.Object.Destroy(enemy.gameObject);
            yield return null;
            yield return null;
        }

        private static IEnumerator ValidateTowerLifecycle()
        {
            PlantingSpot spot = FindSpot("A1");
            Check(spot != null, "A1 planting spot missing.");
            if (spot == null)
                yield break;

            int essenceStart = _session.Economy.Essence;
            Check(essenceStart == Balance.StartEssence,
                "Unexpected Essence before T0012 tower lifecycle: " + essenceStart);

            // Empty spot -> production build radial -> SeedShooter button.
            bool tapAccepted = _session.Planting.TryHandleScreenPress(
                Camera.main.WorldToScreenPoint(spot.transform.position));
            yield return null;

            Button build = FindButton("PlantOption_SeedShooter");
            Check(tapAccepted, "Empty planting spot tap was not accepted.");
            Check(_session.Planting.SelectedSpot == spot, "A1 not selected for build.");
            Check(build != null, "SeedShooter build button missing.");

            Check(DispatchClick(build), "SeedShooter build click could not be dispatched.");
            yield return null;
            yield return null;

            TowerInstance tower = spot.Tower;
            Check(spot.Occupied, "A1 not occupied after production build.");
            Check(tower != null, "TowerInstance missing after production build.");
            Check(tower != null && tower.Level == 1, "Built tower did not start at L1.");
            Check(_session.Economy.Essence == essenceStart - Balance.Cost(PlantKind.SeedShooter),
                "Initial build did not spend exact SeedShooter cost.");

            if (tower == null)
                yield break;

            int afterBuild = _session.Economy.Essence;

            // Occupied spot -> tower panel.
            bool occupiedTap = _session.Planting.TryHandleScreenPress(
                Camera.main.WorldToScreenPoint(spot.transform.position));
            yield return null;

            GameObject panel = GameObject.Find("TowerPanel");
            Check(occupiedTap, "Occupied tower tap was not accepted.");
            Check(_session.Planting.SelectedTower == tower, "Occupied tap did not select the tower.");
            Check(_session.TowerPanel.IsOpen && panel != null, "Tower panel did not open.");

            int cost12 = TowerUpgradeBalance.UpgradeCost(PlantKind.SeedShooter, 1);
            int cost23 = TowerUpgradeBalance.UpgradeCost(PlantKind.SeedShooter, 2);
            Check(cost12 == 75, "Seed L1->L2 expected cost 75.");
            Check(cost23 == 100, "Seed L2->L3 expected cost 100.");

            Button upgrade = FindButton("Button_ULEPSZ");
            Check(upgrade != null, "Tower upgrade button missing.");
            Check(DispatchClick(upgrade), "L1->L2 upgrade click failed.");
            yield return null;

            Check(tower.Level == 2, "Tower did not reach L2.");
            Check(_session.Economy.Essence == afterBuild - cost12,
                "L1->L2 upgrade did not spend exact cost.");
            CheckApprox(TowerUpgradeBalance.SeedDamage(tower.Level), 30f, 0.01f, "Seed L2 damage");

            upgrade = FindButton("Button_ULEPSZ");
            Check(DispatchClick(upgrade), "L2->L3 upgrade click failed.");
            yield return null;

            Check(tower.Level == 3, "Tower did not reach L3.");
            Check(_session.Economy.Essence == afterBuild - cost12 - cost23,
                "L2->L3 upgrade did not spend exact cost.");
            CheckApprox(TowerUpgradeBalance.SeedDamage(tower.Level), 40f, 0.01f, "Seed L3 damage");
            CheckApprox(TowerUpgradeBalance.SeedCooldown(tower.Level), 0.85f, 0.001f, "Seed L3 cooldown");

            int invested = tower.TotalInvested;
            int expectedInvested = Balance.Cost(PlantKind.SeedShooter) + cost12 + cost23;
            int refund = tower.SellRefund;
            int expectedRefund = Mathf.FloorToInt(expectedInvested * 0.70f);
            Check(invested == expectedInvested, "Tower investment tracking mismatch.");
            Check(refund == expectedRefund, "Tower sell refund mismatch.");

            int beforeSell = _session.Economy.Essence;
            Button sell = FindButton("Button_SPRZEDAJ");
            Check(sell != null, "Tower sell button missing.");
            Check(DispatchClick(sell), "Tower sell click failed.");
            yield return null;
            yield return null;

            Check(!spot.Occupied && spot.Tower == null, "Selling did not clear the planting spot.");
            Check(_session.Economy.Essence == beforeSell + expectedRefund,
                "Selling did not refund exactly floor(70% invested).");
            Check(!_session.TowerPanel.IsOpen, "Tower panel remained open after sale.");

            // Rebuild the same spot through production UI.
            bool rebuildTap = _session.Planting.TryHandleScreenPress(
                Camera.main.WorldToScreenPoint(spot.transform.position));
            yield return null;
            build = FindButton("PlantOption_SeedShooter");
            Check(rebuildTap && build != null, "Sold spot could not reopen build radial.");
            Check(DispatchClick(build), "Rebuild click failed.");
            yield return null;
            yield return null;

            int expectedFinal =
                essenceStart
                - Balance.Cost(PlantKind.SeedShooter)
                - cost12
                - cost23
                + expectedRefund
                - Balance.Cost(PlantKind.SeedShooter);

            AddFact("tower.essenceStart", essenceStart);
            AddFact("tower.upgradeCost12", cost12);
            AddFact("tower.upgradeCost23", cost23);
            AddFact("tower.investedBeforeSell", expectedInvested);
            AddFact("tower.sellRefund", expectedRefund);
            AddFact("tower.expectedFinalEssence", expectedFinal);
            AddFact("tower.actualFinalEssence", _session.Economy.Essence);
            AddFact("tower.rebuildOccupied", spot.Occupied);

            Check(spot.Occupied && spot.Tower != null && spot.Tower.Level == 1,
                "Rebuild did not create a fresh L1 tower.");
            Check(_session.Economy.Essence == expectedFinal,
                "T0012 economy conservation failed across build/upgrades/sell/rebuild.");
        }

        private static PlantingSpot FindSpot(string id)
        {
            return UnityEngine.Object.FindObjectsByType<PlantingSpot>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(s => s != null && s.Id == id);
        }

        private static Button FindButton(string name)
        {
            return UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(b => b != null &&
                                     b.gameObject.name == name &&
                                     b.gameObject.activeInHierarchy);
        }

        private static bool DispatchClick(Button button)
        {
            if (button == null || !button.gameObject.activeInHierarchy || !button.interactable)
                return false;

            if (EventSystem.current == null)
                return false;

            PointerEventData data = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
            return true;
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
                Errors.Add(message);
        }

        private static void CheckApprox(float actual, float expected, float tolerance, string label)
        {
            if (Mathf.Abs(actual - expected) > tolerance)
                Errors.Add(label + " expected=" + F(expected) + " actual=" + F(actual));
        }

        private static void AddFact(string key, object value)
        {
            Facts.Add(key + "=" + (value == null ? "null" : value.ToString()));
        }

        private static string F(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void Complete(bool success)
        {
            AddFact("playModeEntered", SessionState.GetBool(PlayModeEnteredKey, false));
            WriteReport(success && Errors.Count == 0);

            SessionState.SetInt(ExitCodeKey, success && Errors.Count == 0 ? 0 : 1);
            SessionState.SetString(PhaseKey, "exit-editor");

            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void FinishImmediately(bool success)
        {
            WriteReport(success);
            SessionState.SetInt(ExitCodeKey, success ? 0 : 1);
            SessionState.SetString(PhaseKey, "exit-editor");

            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else
            {
                SessionState.SetBool(ActiveKey, false);
                UnsubscribeLog();
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static void WriteReport(bool success)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");

            List<string> lines = new List<string>();
            lines.Add("DYRYGENT UNITY EXECUTOR T0012 PLAYABLE RELEASE");
            lines.Add("status=" + (success ? "PASS" : "FAIL"));
            lines.Add("timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            lines.Add("unityVersion=" + Application.unityVersion);
            lines.AddRange(Facts);
            lines.Add("runtimeWarningCount=" + RuntimeWarnings.Count);
            for (int i = 0; i < RuntimeWarnings.Count; i++)
                lines.Add("runtimeWarning." + (i + 1) + "=" + OneLine(RuntimeWarnings[i]));
            lines.Add("validationErrorCount=" + Errors.Count);
            for (int i = 0; i < Errors.Count; i++)
                lines.Add("validationError." + (i + 1) + "=" + OneLine(Errors[i]));

            File.WriteAllLines(ReportPath, lines.ToArray());
            Debug.Log(string.Join("\n", lines.ToArray()));
        }

        private static bool TimedOut()
        {
            DateTime started;
            if (!DateTime.TryParse(
                    SessionState.GetString(StartedKey, string.Empty),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out started))
                return false;

            return (DateTime.UtcNow - started).TotalSeconds > TimeoutSeconds;
        }

        private static string OneLine(string value)
        {
            return (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        }

        private static void SubscribeLog()
        {
            if (_subscribed) return;
            Application.logMessageReceived += OnLog;
            _subscribed = true;
        }

        private static void UnsubscribeLog()
        {
            if (!_subscribed) return;
            Application.logMessageReceived -= OnLog;
            _subscribed = false;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Warning)
                RuntimeWarnings.Add(condition);
            else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Errors.Add("console=" + condition);
        }
    }
}
#endif
