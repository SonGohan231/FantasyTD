#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FantasyTD.MapA;
using FantasyTD.Product;
using FantasyTD.Product.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    [InitializeOnLoad]
    internal static class ProductVisualAcceptanceValidator
    {
        private const string ActiveKey = "FantasyTD.ProductVisual.Active";
        private const string PhaseKey = "FantasyTD.ProductVisual.Phase";
        private const string StartedKey = "FantasyTD.ProductVisual.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.ProductVisual.ExitCode";
        private const string ReportPath = "Artifacts/UnityExecutor/visual-acceptance.txt";
        private const string ShotDir = "Artifacts/UnityExecutor/VisualAcceptance";
        private const float TimeoutSeconds = 300f;

        private static readonly List<string> Facts = new List<string>();
        private static readonly List<string> Errors = new List<string>();
        private static IEnumerator _flow;

        static ProductVisualAcceptanceValidator()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        internal static void Start()
        {
            Facts.Clear();
            Errors.Clear();
            _flow = null;
            Directory.CreateDirectory(ShotDir);
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetString(PhaseKey, "enter-play");
            SessionState.SetString(StartedKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            SessionState.SetInt(ExitCodeKey, 1);

            try
            {
                ProductFlowEditorSetup.OpenMainMenu();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex)
            {
                Errors.Add("startException=" + ex.GetType().Name + ": " + ex.Message);
                Finish(false);
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;

            if (TimedOut())
            {
                Errors.Add("timeout=" + TimeoutSeconds);
                Finish(false);
                return;
            }

            if (SessionState.GetString(PhaseKey, string.Empty) == "exit-editor" &&
                !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int exitCode = SessionState.GetInt(ExitCodeKey, 1);
                SessionState.SetBool(ActiveKey, false);
                EditorApplication.Exit(exitCode);
                return;
            }

            if (!EditorApplication.isPlaying) return;

            if (_flow == null)
            {
                _flow = RunFlow();
                SessionState.SetString(PhaseKey, "validating");
            }

            try
            {
                if (!_flow.MoveNext()) Finish(Errors.Count == 0);
            }
            catch (Exception ex)
            {
                Errors.Add("validatorException=" + ex.GetType().Name + ": " + ex.Message);
                Finish(false);
            }
        }

        private static IEnumerator RunFlow()
        {
            string details;
            bool configured = GameViewResolutionHelper.SetFixedResolution(
                1920, 1080, "PRODUCT 1920x1080", out details);
            Fact("gameViewConfigured", configured);
            Fact("gameViewDetails", details);
            Check(configured, "Could not configure 1920x1080 Game View.");

            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
            for (int i = 0; i < 90; i++) yield return null;

            Check(GameObject.Find("Button_GRAJ") != null, "Main menu GRAJ button missing.");
            IEnumerator shot = Capture("01-main-menu.png", false);
            while (shot.MoveNext()) yield return null;

            SceneManager.LoadScene(ProductFlowScenes.CampaignSceneName);
            for (int i = 0; i < 12; i++) yield return null;
            Check(GameObject.Find("MapButton_MAP1") != null, "Campaign MAP1 button missing.");
            Button map2 = FindButton("MapButton_MAP2");
            Check(map2 != null && !map2.interactable, "MAP2 must be visible but locked.");
            shot = Capture("02-campaign-map-select.png", false);
            while (shot.MoveNext()) yield return null;

            SceneManager.LoadScene(ProductFlowScenes.MapASceneName);
            GameSession session = null;
            for (int i = 0; i < 120 && session == null; i++)
            {
                yield return null;
                session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
            }

            Check(session != null, "MAP-A GameSession did not initialize.");
            Texture2D mapTexture = Resources.Load<Texture2D>("Higgsfield/MAP_A");
            Check(mapTexture != null, "Higgsfield MAP-A texture missing.");
            try
            {
                HiggsfieldSpriteFactory.ValidateProductionAssets();
                Fact("productionHiggsfieldAssetSet", "validated");
            }
            catch (Exception ex)
            {
                Check(false, "Higgsfield production asset validation failed: " + ex.Message);
            }

            GameObject grass = GameObject.Find("Grass");
            SpriteRenderer grassRenderer = grass != null ? grass.GetComponent<SpriteRenderer>() : null;
            Check(grassRenderer != null && grassRenderer.sprite != null, "MAP-A background renderer missing.");
            Check(grassRenderer != null && grassRenderer.sprite != null &&
                  grassRenderer.sprite.name == "Higgsfield_MAP_A",
                "MAP-A is not rendering the Higgsfield background sprite.");
            Fact("mapRendererSprite", grassRenderer != null && grassRenderer.sprite != null
                ? grassRenderer.sprite.name : "MISSING");
            Check(GameObject.Find("Button_MAPY") != null, "Gameplay MAPY navigation button missing.");

            shot = Capture("03-map-a-higgsfield-gameplay.png", true);
            while (shot.MoveNext()) yield return null;

            if (session != null)
            {
                PlantingSpot a1 = FindSpot("A1");
                PlantingSpot a5 = FindSpot("A5");
                PlantingSpot a9 = FindSpot("A9");
                Check(a1 != null && a5 != null && a9 != null,
                    "Representative planting spots A1/A5/A9 missing.");

                if (a1 != null && Camera.main != null)
                {
                    Check(session.Planting.TryHandleScreenPress(
                        Camera.main.WorldToScreenPoint(a1.transform.position)),
                        "A1 production planting press was blocked.");
                    yield return null;
                    Button firstBuild = FindButton("PlantOption_SeedShooter");
                    Check(firstBuild != null && firstBuild.interactable,
                        "Pędostrzał radial option missing/unaffordable.");
                    shot = Capture("04-planting-radial-higgsfield-map.png", true);
                    while (shot.MoveNext()) yield return null;
                    Check(DispatchClick(firstBuild),
                        "Could not build A1 Pędostrzał through production radial.");
                    for (int i = 0; i < 4; i++) yield return null;
                }

                IEnumerator build = BuildTowerThroughProduction(session, a5, PlantKind.SeedShooter);
                while (build.MoveNext()) yield return null;
                build = BuildTowerThroughProduction(session, a9, PlantKind.EssenceFlower);
                while (build.MoveNext()) yield return null;

                PlantingSpot[] representative = { a1, a5, a9 };
                int visibleTowerCount = 0;
                foreach (PlantingSpot built in representative)
                {
                    if (built == null || built.Tower == null) continue;
                    SpriteRenderer renderer = built.Tower.GetComponentsInChildren<SpriteRenderer>()
                        .FirstOrDefault(sr => sr != null && sr.sprite != null &&
                            sr.gameObject.name == "HiggsfieldVisual");
                    string spriteName = renderer != null && renderer.sprite != null
                        ? renderer.sprite.name : "MISSING";
                    Fact("tower." + built.Id + ".sprite", spriteName);
                    Check(renderer != null &&
                          spriteName.StartsWith("Higgsfield_", StringComparison.Ordinal),
                        built.Id + " is not rendering a Higgsfield production tower sprite.");
                    if (renderer != null && IsRendererReadableOnPhone(renderer, 72f))
                        visibleTowerCount++;
                }
                Fact("visibleReadableHiggsfieldTowers", visibleTowerCount);
                Check(visibleTowerCount >= 3,
                    "Product screenshot requires at least three readable Higgsfield towers in view.");

                shot = Capture("05-three-higgsfield-towers.png", true);
                while (shot.MoveNext()) yield return null;

                if (a5 != null && a5.Tower != null && Camera.main != null)
                {
                    Check(session.Planting.TryHandleScreenPress(
                        Camera.main.WorldToScreenPoint(a5.transform.position)),
                        "Could not select representative combat tower.");
                    for (int i = 0; i < 3; i++) yield return null;
                    Check(session.Planting.SelectedTower == a5.Tower,
                        "Representative combat tower selection did not persist.");

                    Transform range = a5.Tower.transform.Find("AttackRangeIndicator");
                    LineRenderer rangeLine = range != null ? range.GetComponent<LineRenderer>() : null;
                    Check(rangeLine != null && rangeLine.enabled,
                        "Selected combat tower attack-range indicator is not visible.");

                    // Keep the production selection/range ring but hide only the detail plaque
                    // so the combat capture matches the reference composition.
                    session.TowerPanel.Close();
                    yield return null;
                    shot = Capture("06-selected-tower-range.png", true);
                    while (shot.MoveNext()) yield return null;
                }

                Button startWave = FindButton("Button_START");
                Check(DispatchClick(startWave), "Could not start wave 1 through production UI.");

                bool combatCompositionReady = false;
                int readableEnemyCount = 0;
                bool projectileVisible = false;
                for (int i = 0; i < 1800; i++)
                {
                    yield return null;
                    readableEnemyCount = 0;
                    foreach (EnemyUnit activeEnemy in session.Enemies.Where(
                                 e => e != null && !e.IsDead))
                    {
                        SpriteRenderer renderer = activeEnemy.GetComponentsInChildren<SpriteRenderer>()
                            .FirstOrDefault(sr => sr != null && sr.sprite != null &&
                                sr.gameObject.name == "HiggsfieldVisual");
                        EnemyHealthBar hp = activeEnemy.GetComponent<EnemyHealthBar>();
                        if (renderer != null && hp != null &&
                            IsRendererReadableOnPhone(renderer, 50f))
                            readableEnemyCount++;
                    }

                    projectileVisible =
                        UnityEngine.Object.FindFirstObjectByType<SeedProjectile>() != null;

                    if (session.Waves.CurrentWave == 1 &&
                        readableEnemyCount >= 4 &&
                        projectileVisible)
                    {
                        combatCompositionReady = true;
                        break;
                    }
                }

                Fact("combatReadableEnemyCount", readableEnemyCount);
                Fact("combatProjectileVisible", projectileVisible);
                Check(session.Waves.CurrentWave == 1, "Production wave 1 did not start.");
                Check(readableEnemyCount >= 4,
                    "Product combat screenshot requires at least four readable enemies with HP bars in view.");
                Check(projectileVisible,
                    "Product combat screenshot requires a real production projectile/VFX in flight.");
                Check(combatCompositionReady,
                    "Representative production combat composition was not reached before capture deadline.");

                foreach (EnemyUnit activeEnemy in session.Enemies.Where(e => e != null && !e.IsDead))
                {
                    SpriteRenderer renderer = activeEnemy.GetComponentsInChildren<SpriteRenderer>()
                        .FirstOrDefault(sr => sr != null && sr.sprite != null &&
                            sr.gameObject.name == "HiggsfieldVisual");
                    string spriteName = renderer != null && renderer.sprite != null
                        ? renderer.sprite.name : "MISSING";
                    Check(renderer != null &&
                          (spriteName.StartsWith("Higgsfield_", StringComparison.Ordinal) ||
                           spriteName.StartsWith("Chochlik_", StringComparison.Ordinal)),
                        "Visible enemy is not using Higgsfield production art: " + spriteName);
                    Check(activeEnemy.GetComponent<EnemyHealthBar>() != null,
                        "Visible production enemy HP bar missing.");
                }

                shot = Capture("07-active-wave-commercial-composition.png", true);
                while (shot.MoveNext()) yield return null;

                Fact("bossEndStateScreenshot",
                    "not forced in visual gate; natural boss/victory is validated by full-campaign gate");
            }

            Fact("validationErrorCount", Errors.Count);
        }

        private static IEnumerator Capture(string fileName, bool requireRichGameplay)
        {
            string path = Path.Combine(ShotDir, fileName);
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            for (int i = 0; i < 90 && !File.Exists(path); i++) yield return null;
            for (int i = 0; i < 3; i++) yield return null;

            bool ok = File.Exists(path) && new FileInfo(path).Length > 10000;
            Fact("screenshot." + fileName, ok ? path : "MISSING");
            Check(ok, "Screenshot not produced: " + fileName);

            if (ok && requireRichGameplay)
                ValidateRichGameplayScreenshot(path, fileName);
        }

        private static void ValidateRichGameplayScreenshot(string path, string fileName)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Fact("screenshotBytes." + fileName, bytes.Length);
            Check(bytes.Length >= 100000,
                "Gameplay screenshot is suspiciously small and may be placeholder/blank: " + fileName);

            Texture2D image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool loaded = image.LoadImage(bytes, false);
            Check(loaded, "Could not decode gameplay screenshot: " + fileName);
            if (!loaded)
            {
                UnityEngine.Object.DestroyImmediate(image);
                return;
            }

            HashSet<int> bins = new HashSet<int>();
            int x0 = image.width / 10;
            int x1 = image.width * 9 / 10;
            int y0 = image.height / 10;
            int y1 = image.height * 9 / 10;
            int step = Mathf.Max(4, Mathf.Min(image.width, image.height) / 90);
            for (int y = y0; y < y1; y += step)
            for (int x = x0; x < x1; x += step)
            {
                Color32 c = image.GetPixel(x, y);
                int key = ((c.r >> 4) << 8) | ((c.g >> 4) << 4) | (c.b >> 4);
                bins.Add(key);
            }

            Fact("quantizedColorBins." + fileName, bins.Count);
            Check(bins.Count >= 128,
                "Gameplay screenshot lacks expected visual color/detail richness; old flat placeholder may be rendering: " +
                fileName + " bins=" + bins.Count);
            UnityEngine.Object.DestroyImmediate(image);
        }

        private static IEnumerator BuildTowerThroughProduction(
            GameSession session,
            PlantingSpot spot,
            PlantKind kind)
        {
            Check(session != null && spot != null && Camera.main != null,
                "Cannot build representative tower: missing session/spot/camera.");
            if (session == null || spot == null || Camera.main == null)
                yield break;

            Check(session.Planting.TryHandleScreenPress(
                Camera.main.WorldToScreenPoint(spot.transform.position)),
                spot.Id + " production planting press was blocked.");
            yield return null;

            Button build = FindButton("PlantOption_" + kind);
            Check(build != null && build.interactable,
                spot.Id + " build option unavailable for " + kind + ".");
            Check(DispatchClick(build),
                "Could not build " + kind + " at " + spot.Id + " through production radial.");
            for (int i = 0; i < 4; i++) yield return null;

            Check(spot.Occupied && spot.Tower != null && spot.Tower.Kind == kind,
                "Production tower build failed at " + spot.Id + ".");
        }

        private static bool IsRendererReadableOnPhone(
            SpriteRenderer renderer,
            float minimumPixelHeight)
        {
            if (renderer == null || Camera.main == null || renderer.sprite == null)
                return false;

            Bounds b = renderer.bounds;
            Vector3 bottom = Camera.main.WorldToScreenPoint(
                new Vector3(b.center.x, b.min.y, b.center.z));
            Vector3 top = Camera.main.WorldToScreenPoint(
                new Vector3(b.center.x, b.max.y, b.center.z));
            Vector3 center = Camera.main.WorldToScreenPoint(b.center);

            float pixelHeight = Mathf.Abs(top.y - bottom.y);
            bool inView = center.z > 0f &&
                          center.x >= 0f && center.x <= Screen.width &&
                          center.y >= 0f && center.y <= Screen.height;
            return inView && pixelHeight >= minimumPixelHeight;
        }

        private static PlantingSpot FindSpot(string id)
        {
            return UnityEngine.Object.FindObjectsByType<PlantingSpot>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(s => s != null && s.Id == id);
        }

        private static Button FindButton(string name)
        {
            return UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(b => b != null && b.gameObject.name == name && b.gameObject.activeInHierarchy);
        }

        private static bool DispatchClick(Button button)
        {
            if (button == null || !button.interactable || EventSystem.current == null) return false;
            PointerEventData data = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
            return true;
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) Errors.Add(message);
        }

        private static void Fact(string key, object value)
        {
            Facts.Add(key + "=" + (value == null ? "null" : value.ToString()));
        }

        private static void Finish(bool success)
        {
            bool final = success && Errors.Count == 0;
            List<string> lines = new List<string>
            {
                "DYRYGENT PRODUCT VISUAL ACCEPTANCE",
                "status=" + (final ? "PASS" : "FAIL"),
                "timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "unityVersion=" + Application.unityVersion
            };
            lines.AddRange(Facts);
            lines.Add("validationErrorCount=" + Errors.Count);
            for (int i = 0; i < Errors.Count; i++)
                lines.Add("validationError." + (i + 1) + "=" + Errors[i].Replace("\n", " "));

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");
            File.WriteAllLines(ReportPath, lines.ToArray());
            Debug.Log(string.Join("\n", lines.ToArray()));

            SessionState.SetInt(ExitCodeKey, final ? 0 : 1);
            SessionState.SetString(PhaseKey, "exit-editor");
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
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
    }
}
#endif
