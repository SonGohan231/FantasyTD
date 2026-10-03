#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using FantasyTD.MapA;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FantasyTD.Product.Editor
{
    [InitializeOnLoad]
    public static class ProductFlowEditorSetup
    {
        public const string MainMenuScenePath = "Assets/FantasyTD/Scenes/MAIN_MENU.unity";
        public const string CampaignScenePath = "Assets/FantasyTD/Scenes/CAMPAIGN_MAP_SELECT.unity";

        public static string[] ReleaseScenePaths =>
            new[] { MainMenuScenePath, CampaignScenePath, FantasyTD.MapA.Editor.MapAEditorSetup.ScenePath };

        static ProductFlowEditorSetup()
        {
            EditorApplication.delayCall += EnsureProductScenes;
        }

        [MenuItem("Tools/Fantasy TD/Open Main Menu")]
        public static void OpenMainMenu()
        {
            EnsureProductScenes();
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        }

        [MenuItem("Tools/Fantasy TD/Rebuild Product Flow Scenes")]
        public static void RebuildProductFlowScenes()
        {
            if (File.Exists(MainMenuScenePath)) AssetDatabase.DeleteAsset(MainMenuScenePath);
            if (File.Exists(CampaignScenePath)) AssetDatabase.DeleteAsset(CampaignScenePath);
            EnsureProductScenes();
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        }

        public static void EnsureProductScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            string previous = SceneManager.GetActiveScene().path;
            Directory.CreateDirectory("Assets/FantasyTD/Scenes");

            FantasyTD.MapA.Editor.MapAEditorSetup.EnsureMapSceneExists();
            EnsureScene<MainMenuBootstrap>(MainMenuScenePath, "MAIN MENU Bootstrap");
            EnsureScene<CampaignMapSelectBootstrap>(CampaignScenePath, "CAMPAIGN MAP SELECT Bootstrap");
            EnsureBuildOrder();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!string.IsNullOrEmpty(previous) && File.Exists(previous))
                EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
        }

        private static void EnsureScene<T>(string path, string rootName) where T : Component
        {
            if (File.Exists(path)) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject(rootName);
            root.AddComponent<T>();
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void EnsureBuildOrder()
        {
            List<EditorBuildSettingsScene> ordered = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(CampaignScenePath, true),
                new EditorBuildSettingsScene(FantasyTD.MapA.Editor.MapAEditorSetup.ScenePath, true)
            };

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == MainMenuScenePath ||
                    scene.path == CampaignScenePath ||
                    scene.path == FantasyTD.MapA.Editor.MapAEditorSetup.ScenePath)
                    continue;
                ordered.Add(scene);
            }

            EditorBuildSettings.scenes = ordered.ToArray();
        }
    }
}
#endif
