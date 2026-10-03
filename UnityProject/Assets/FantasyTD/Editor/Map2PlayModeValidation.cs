#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FantasyTD.MapA;
using FantasyTD.Product;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    internal static class Map2PlayModeValidation
    {
        // Run after MAP1 scenarios inside the existing render-capable Play Mode driver.
        internal static IEnumerator Run(Action<bool,string> check, Action<string> fact)
        {
            SceneManager.LoadScene(ProductFlowScenes.Map2SceneName);
            GameSession session = null;
            for (int frame = 0; frame < 120; frame++)
            {
                yield return null;
                session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if (session != null && session.MapDefinition != null && session.MapDefinition.Id == "MAP2" && session.Hud != null)
                    break;
            }
            check(session != null && session.MapDefinition != null && session.MapDefinition.Id == "MAP2", "MAP2 fresh session did not initialize.");
            if (session == null || session.MapDefinition == null || session.MapDefinition.Id != "MAP2") yield break;
            check(session.Economy.Essence == Balance.StartEssence && session.Base.Hp == Balance.BaseHp && session.Waves.CurrentWave == 0 && !session.IsFinished,
                "MAP2 retained state from preceding MAP1 scene.");
            if (!CampaignMapDefinition.Map2.IsValidatedPlayable)
            {
                bool priorVictory = CampaignProgress.HasVictory("MAP2");
                CampaignProgress.RecordVictory("MAP2");
                check(CampaignProgress.HasVictory("MAP2") == priorVictory, "An unvalidated MAP2 accepted a progression write.");
            }

            Vector2[] route = CampaignMapDefinition.Map2.Waypoints;
            check(session.Waypoints.Count == route.Length, "MAP2 session did not use its authored route.");
            check(Vector2.Distance(session.Waypoints[0], route[0]) < 0.01f &&
                Vector2.Distance(session.Waypoints[session.Waypoints.Count-1], route[route.Length-1]) < 0.01f,
                "MAP2 route endpoints differ from its definition.");
            SpriteRenderer terrain = GameObject.Find("MAP2_ApprovedTerrain")?.GetComponent<SpriteRenderer>();
            check(terrain != null && terrain.bounds.size.x >= 23.99f && terrain.bounds.size.y >= 13.99f, "Approved MAP2 terrain does not cover gameplay bounds.");
            foreach (string piece in new[] { "MAP2_UpperRoad", "MAP2_UpperBend", "MAP2_RightRoad", "MAP2_LowerBend", "MAP2_LowerRoad" })
                check(GameObject.Find(piece)?.GetComponent<SpriteRenderer>()?.sprite != null, "Authored MAP2 road piece missing: " + piece);

            // Source pixel checks establish that measured curve samples are asphalt,
            // not blank sheet background. Runtime transforms share these sample positions.
            Texture2D roadSheet = Resources.Load<Texture2D>("Higgsfield/Maps/MAP2_ROAD_KIT");
            check(roadSheet != null && roadSheet.isReadable, "MAP2 road source is unavailable for pixel provenance checks.");
            if (roadSheet != null && roadSheet.isReadable)
            {
                int[,] samples = { {232,682}, {300,679}, {360,666}, {403,623}, {425,562}, {277,97} };
                for (int i = 0; i < samples.GetLength(0); i++)
                {
                    Color pixel = roadSheet.GetPixel(samples[i,0], roadSheet.height-1-samples[i,1]);
                    check(pixel.r < 0.65f && pixel.g < 0.65f && pixel.b < 0.65f,
                        "MAP2 road centerline sample is not authored asphalt: " + samples[i,0] + "," + samples[i,1]);
                }
            }

            PlantingSpot[] spots = UnityEngine.Object.FindObjectsByType<PlantingSpot>(FindObjectsSortMode.None);
            check(spots.Length == 12, "MAP2 does not have exactly 12 active authored planting spots.");
            PlantingSpot buildSpot = null;
            foreach (PlantingSpot spot in spots)
            {
                if (spot.Id == "B5") buildSpot = spot;
                bool accepted = session.Planting.TryHandleScreenPress(Camera.main.WorldToScreenPoint(spot.transform.position));
                yield return null;
                check(accepted && session.Planting.SelectedSpot == spot && session.Hud.RadialRoot != null,
                    "MAP2 planting spot cannot be selected through production input: " + spot.Id);
                Button close = GameObject.Find("RadialCloseButton")?.GetComponent<Button>();
                check(Click(close), "MAP2 radial close failed: " + spot.Id);
                yield return null;
                yield return null;
            }
            check(buildSpot != null, "MAP2 strategic build spot B5 missing.");
            if (buildSpot == null) yield break;
            int beforeBuild = session.Economy.Essence;
            session.Planting.TryHandleScreenPress(Camera.main.WorldToScreenPoint(buildSpot.transform.position));
            yield return null;
            check(Click(GameObject.Find("PlantOption_SeedShooter")?.GetComponent<Button>()), "MAP2 SeedShooter production build click failed.");
            yield return null;
            yield return null;
            check(buildSpot.Occupied && buildSpot.Tower != null && buildSpot.Tower.Kind == PlantKind.SeedShooter,
                "MAP2 production planting did not create a playable tower.");
            check(session.Economy.Essence == beforeBuild-Balance.Cost(PlantKind.SeedShooter), "MAP2 tower build cost mismatch.");

            check(Click(GameObject.Find("Button_START")?.GetComponent<Button>()), "MAP2 first-wave production button failed.");
            yield return null;
            check(session.Waves.CurrentWave == 1 && session.Waves.Running, "MAP2 wave did not enter production spawn flow.");
            Directory.CreateDirectory("Artifacts/UnityExecutor");
            ScreenCapture.CaptureScreenshot("Artifacts/UnityExecutor/map2-first-wave-1920x1080.png");
            HashSet<int> observedEnemies = new HashSet<int>();
            bool observedDamage = false;
            bool observedForwardMovement = false;
            float priorScale = Time.timeScale;
            Time.timeScale = 4f;
            float deadline = Time.realtimeSinceStartup + 35f;
            try
            {
                while (Time.realtimeSinceStartup < deadline && session.Waves.Running && !session.IsFinished)
                {
                    foreach (EnemyUnit enemy in session.Enemies)
                    {
                        observedEnemies.Add(enemy.GetInstanceID());
                        if (enemy.CurrentHp < enemy.MaxHp) observedDamage = true;
                        if (enemy.ProgressDistance > 0.5f && enemy.transform.position.y > 9.7f) observedForwardMovement = true;
                    }
                    yield return null;
                }
            }
            finally { Time.timeScale = priorScale; }
            check(observedEnemies.Count == 6, "MAP2 natural wave did not observe exactly six production spawns.");
            check(observedForwardMovement, "MAP2 enemy did not follow the upper asphalt lane.");
            check(observedDamage, "MAP2 planted tower did not damage a naturally spawned enemy through production combat.");
            check(session.Waves.CurrentWave == 1 && session.Waves.ReadyForNextWave && session.Enemies.Count == 0 && !session.IsFinished,
                "MAP2 natural wave did not settle into a playable next-wave state.");
            fact("map2.observedSpawns=" + observedEnemies.Count);
            fact("map2.productionTowerDamage=" + observedDamage);
            fact("map2.baseHpAfterNaturalWave=" + session.Base.Hp);
            fact("map2.nextWaveReady=" + session.Waves.ReadyForNextWave);
            fact("map2.campaignEnabled=" + CampaignMapDefinition.Map2.IsValidatedPlayable);
            Directory.CreateDirectory("Artifacts/UnityExecutor");
            ScreenCapture.CaptureScreenshot("Artifacts/UnityExecutor/map2-natural-wave-1920x1080.png");
            for (int i = 0; i < 8; i++) yield return null;
        }

        private static bool Click(Button button)
        {
            if (button == null || !button.interactable || EventSystem.current == null) return false;
            PointerEventData pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            return ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
    }
}
#endif
