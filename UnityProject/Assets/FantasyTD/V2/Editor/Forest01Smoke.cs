#if UNITY_EDITOR
using System;
using FantasyTD.V2.Content;
using FantasyTD.V2.Core;
using FantasyTD.V2.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FantasyTD.V2.Editor
{
    /// <summary>Real Unity Editor scene import/integrity plus core interaction smoke.</summary>
    public static class Forest01Smoke
    {
        public const string ScenePath =
            "Assets/FantasyTD/V2/Scenes/Forest01.unity";

        [MenuItem("Tools/Fantasy TD V2/Verify Forest 01")]
        public static void RunSmoke()
        {
            LevelDefinition level = StarterLevel.Create();
            Require(level.Sites.Count == 12, "Exactly 12 build sites required.");
            Require(level.Towers.Count >= 3, "At least three defenders required.");
            Require(level.Enemies.Count >= 2, "At least two enemies required.");
            Require(level.Waves.Count >= 3, "At least three waves required.");
            Require(level.PathLength > 2000, "Path is unexpectedly short.");

            const string artworkPath = "Assets/Resources/FT2/Forest01Art.png";
            // Regression: source art was present and imported, but the old test
            // insisted on Texture2D and failed for a Sprite-imported PNG.
            TextureImporter importer = AssetImporter.GetAtPath(artworkPath)
                as TextureImporter;
            Require(importer != null,
                "Recovered forest artwork has no TextureImporter.");
            Sprite mapArtwork = AssetDatabase.LoadAssetAtPath<Sprite>(artworkPath);
            Require(mapArtwork != null && mapArtwork.texture != null &&
                    mapArtwork.texture.width >= 512,
                "Recovered source artwork must be a loadable Sprite with >=512px width.");
            Require(UnityEngine.Resources.Load<Sprite>("FT2/Forest01Art") != null,
                "Forest01 sprite must resolve through Resources.Load at runtime.");

            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Require(asset != null, "Tracked forest scene failed to import.");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                bool bootstrapFound = false;
                foreach (GameObject root in scene.GetRootGameObjects())
                    if (root.GetComponent<ForestBattlefield>() != null)
                        bootstrapFound = true;
                Require(bootstrapFound, "Scene does not contain ForestBattlefield bootstrap.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            BattleSimulator battle = new BattleSimulator(level);
            Require(battle.TryPlaceTower("a01", "thorn"),
                "First defender placement rejected.");
            Require(!battle.TryPlaceTower("a01", "frost"),
                "Overlapping defenders must be rejected.");
            Require(!battle.TryPlaceTower("invalid-site", "thorn"),
                "Unknown sites must be rejected.");
            Require(battle.StartNextWave(), "Wave 1 must start.");
            battle.Step();
            Require(battle.Enemies.Count == 1, "Expected a real first enemy.");
            for (int tick = 0; tick < 2500 && battle.WaveActive &&
                 battle.Result == BattleResult.InProgress; tick++)
                battle.Step();
            Require(!battle.WaveActive, "First wave did not resolve in time.");
            Require(battle.Result != BattleResult.Defeat,
                "One wave must not exhaust all 15 lives.");
            Require(battle.CanStartWave, "Second wave should be available.");
            Debug.Log("FantasyTD FT2-003 Forest01: PASS (source art import, scene, bootstrap, " +
                      "12 sites, 3 defenders, 3 waves, placement, spawn and clear).");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException("Forest01 scene smoke FAILED: " + message);
        }
    }
}
#endif
