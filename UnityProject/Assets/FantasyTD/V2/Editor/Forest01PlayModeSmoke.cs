#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using FantasyTD.V2.Core;
using FantasyTD.V2.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FantasyTD.V2.Editor
{
    /// <summary>Enters real Play Mode and exercises the complete Forest01 battle loop.</summary>
    [InitializeOnLoad]
    public static class Forest01PlayModeSmoke
    {
        private const string ActiveKey = "FantasyTD.Forest01.PlayMode.Active";
        private const string PhaseKey = "FantasyTD.Forest01.PlayMode.Phase";
        private const string RenderKey = "FantasyTD.Forest01.PlayMode.Render";
        private const string ExitCodeKey = "FantasyTD.Forest01.PlayMode.ExitCode";
        private static int _warmupFrames;
        private static DateTime _startedUtc;

        static Forest01PlayModeSmoke()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        public static void RunSmoke() { Begin(false); }
        public static void RunSmokeAndRender() { Begin(true); }

        private static void Begin(bool render)
        {
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(RenderKey, render);
            SessionState.SetString(PhaseKey, "enter-play");
            SessionState.SetInt(ExitCodeKey, 1);
            _warmupFrames = 0;
            _startedUtc = DateTime.UtcNow;
            Directory.CreateDirectory(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, "Logs"));
            EditorSceneManager.OpenScene(Forest01Smoke.ScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            if (_startedUtc == default(DateTime)) _startedUtc = DateTime.UtcNow;
            if ((DateTime.UtcNow - _startedUtc).TotalMinutes > 12)
            {
                Finish(false, "Play Mode validator exceeded 12 minutes.");
                return;
            }
            string phase = SessionState.GetString(PhaseKey, "enter-play");
            if (phase == "exit-editor" && !EditorApplication.isPlaying &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int code = SessionState.GetInt(ExitCodeKey, 1);
                SessionState.SetBool(ActiveKey, false);
                EditorApplication.Exit(code);
                return;
            }
            if (!EditorApplication.isPlaying || phase == "validating") return;
            _warmupFrames++;
            if (_warmupFrames < 8) return;
            SessionState.SetString(PhaseKey, "validating");
            try
            {
                ValidateRuntime();
                Finish(true, "Forest01 real Play Mode integration PASS");
            }
            catch (Exception ex)
            {
                Finish(false, ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private static void ValidateRuntime()
        {
            ForestBattlefield game = UnityEngine.Object.FindFirstObjectByType<ForestBattlefield>();
            Require(game != null, "ForestBattlefield did not start in Play Mode.");
            Require(SceneManager.GetActiveScene().path == Forest01Smoke.ScenePath,
                "Forest01 is not the active Play Mode scene.");
            Require(game.RuntimeSiteCount == 10, "Expected exactly 10 runtime build sites.");
            Require(game.RuntimeTotalWaves == 6, "Expected exactly 6 runtime waves.");
            Require(game.RuntimeEssence == 300 && game.RuntimeBaseHp == 20,
                "Initial economy/base state is incorrect.");

            Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            Require(canvas != null && canvas.GetComponent<CanvasScaler>() != null,
                "Touch HUD canvas/scaler missing.");
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            Require(buttons.Length >= 9, "Expected tower, upgrade, wave, pause and stage controls.");
            foreach (Button button in buttons)
            {
                RectTransform rect = button.transform as RectTransform;
                Require(rect != null && rect.sizeDelta.x >= 200f && rect.sizeDelta.y >= 70f,
                    "Touch target is smaller than the Forest01 mobile contract: " + button.name);
            }
            Require(UnityEngine.Object.FindFirstObjectByType<AudioSource>() != null,
                "Battle feedback AudioSource missing.");

            int initialEssence = game.RuntimeEssence;
            Require(game.SelectSite("B01") && game.TryBuildSelected("thorn"),
                "Could not build Pędostrzał on B01.");
            Require(game.SelectSite("B01") && game.TryUpgradeSelected(),
                "Could not upgrade B01.");
            Require(game.RuntimeTowerLevel("B01") == 2 && game.RuntimeEssence < initialEssence,
                "Upgrade did not spend Essence or change level.");
            Require(game.SelectSite("B02") && game.TryBuildSelected("bloom"),
                "Could not build Kwiat Esencji on B02.");
            Require(game.SelectSite("B03") && game.TryBuildSelected("frost"),
                "Could not build Mroźny Pąk on B03.");
            Require(game.RuntimeTowerCount == 3, "Runtime tower count did not reach three.");
            Require(game.TryStartNextWave(), "Wave 1 did not start.");
            game.StepTicksForValidation(4000);
            Require(!game.RuntimeWaveActive && game.RuntimeKills > 0 &&
                    game.RuntimeResult != BattleResult.Defeat,
                "Wave 1 did not complete with real damage and kills.");

            game.RestartBattle();
            BuildWinningOpening(game);
            for (int wave = 0; wave < 6 && game.RuntimeResult == BattleResult.InProgress; wave++)
            {
                Require(game.TryStartNextWave(), "Winning route could not start wave " + (wave + 1));
                game.StepTicksForValidation(12000);
                Require(!game.RuntimeWaveActive, "Winning route wave timed out: " + (wave + 1));
                if (game.RuntimeResult == BattleResult.InProgress) SpendAvailableEssence(game);
            }
            Require(game.RuntimeResult == BattleResult.Victory,
                "Configured Forest01 defense did not reach victory.");

            game.RestartBattle();
            for (int wave = 0; wave < 6 && game.RuntimeResult == BattleResult.InProgress; wave++)
            {
                Require(game.TryStartNextWave(), "Defeat route could not start wave " + (wave + 1));
                game.StepTicksForValidation(12000);
            }
            Require(game.RuntimeResult == BattleResult.Defeat,
                "Unopposed Forest01 waves did not reach defeat.");

            if (SessionState.GetBool(RenderKey, false))
            {
                game.RestartBattle();
                BuildWinningOpening(game);
                Camera camera = Camera.main;
                Require(camera != null, "Main camera missing for Play Mode render.");
                RenderMode oldMode = canvas.renderMode;
                Camera oldCamera = canvas.worldCamera;
                float oldDistance = canvas.planeDistance;
                try
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 1f;
                    Require(Forest01TerrainGenerator.RenderFrame(camera, 1600, 900,
                        "Forest01_PlayMode_1600x900.png"),
                        "Play Mode camera render failed.");
                }
                finally
                {
                    canvas.renderMode = oldMode;
                    canvas.worldCamera = oldCamera;
                    canvas.planeDistance = oldDistance;
                }
            }
        }

        private static void BuildWinningOpening(ForestBattlefield game)
        {
            Build(game, "B02", "bloom");
            Build(game, "B01", "thorn");
            Build(game, "B03", "frost");
            Build(game, "B05", "oak");
        }

        private static void SpendAvailableEssence(ForestBattlefield game)
        {
            foreach (string site in new[] { "B01", "B03", "B05", "B02" })
            {
                game.SelectSite(site);
                while (game.TryUpgradeSelected()) { }
            }
            string[] sites = { "B04", "B06", "B07", "B08", "B09", "B10" };
            string[] towers = { "thorn", "frost", "thorn", "oak", "frost", "thorn" };
            for (int i = 0; i < sites.Length; i++)
                if (game.RuntimeTowerLevel(sites[i]) == 0)
                    Build(game, sites[i], towers[i], false);
        }

        private static void Build(ForestBattlefield game, string site, string tower,
            bool required = true)
        {
            bool built = game.SelectSite(site) && game.TryBuildSelected(tower);
            if (required) Require(built, "Could not build " + tower + " on " + site + ".");
        }

        private static void Finish(bool success, string message)
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string report = Path.Combine(root, "Logs", "forest01-playmode-integration.txt");
            File.WriteAllText(report,
                "status=" + (success ? "PASS" : "FAIL") + "\n" +
                "unity=" + Application.unityVersion + "\n" +
                "playModeEntered=" + EditorApplication.isPlaying + "\n" +
                "message=" + message + "\n");
            if (success) Debug.Log("FT2 Forest01 PLAY MODE: PASS — " + message);
            else Debug.LogError("FT2 Forest01 PLAY MODE: FAIL — " + message);
            SessionState.SetInt(ExitCodeKey, success ? 0 : 1);
            SessionState.SetString(PhaseKey, "exit-editor");
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.ExitPlaymode();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
