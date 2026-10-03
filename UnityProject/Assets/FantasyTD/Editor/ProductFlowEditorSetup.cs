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
        public const string Map2ScenePath = "Assets/FantasyTD/Scenes/MAP_2_POSTAPO.unity";
        public const string Map3ScenePath = "Assets/FantasyTD/Scenes/MAP_3_TWILIGHT.unity";
        public const string Map4ScenePath = "Assets/FantasyTD/Scenes/MAP_4_ABYSSAL.unity";
        public const string Map5ScenePath = "Assets/FantasyTD/Scenes/MAP5_FUNGAL_ANDROID.unity";
        public const string Map6ScenePath = "Assets/FantasyTD/Scenes/MAP6_BAMBOO_ANDROID.unity";

        public static string[] ReleaseScenePaths =>
            new[] { MainMenuScenePath, CampaignScenePath, FantasyTD.MapA.Editor.MapAEditorSetup.ScenePath,
                Map2ScenePath, Map3ScenePath, Map4ScenePath, Map5ScenePath, Map6ScenePath };

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
            if (File.Exists(Map2ScenePath)) AssetDatabase.DeleteAsset(Map2ScenePath);
            foreach (string path in new[] { Map3ScenePath, Map4ScenePath, Map5ScenePath, Map6ScenePath })
                if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
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
            EnsureScene<Map2Bootstrap>(Map2ScenePath, "MAP2 Bootstrap");
            EnsureScene<Map3Bootstrap>(Map3ScenePath, "MAP3 Bootstrap");
            EnsureScene<Map4Bootstrap>(Map4ScenePath, "MAP4 Bootstrap");
            EnsureScene<Map5Bootstrap>(Map5ScenePath, "MAP5 Bootstrap");
            EnsureScene<Map6Bootstrap>(Map6ScenePath, "MAP6 Bootstrap");
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
                new EditorBuildSettingsScene(FantasyTD.MapA.Editor.MapAEditorSetup.ScenePath, true),
                new EditorBuildSettingsScene(Map2ScenePath, true),
                new EditorBuildSettingsScene(Map3ScenePath, true),
                new EditorBuildSettingsScene(Map4ScenePath, true),
                new EditorBuildSettingsScene(Map5ScenePath, true),
                new EditorBuildSettingsScene(Map6ScenePath, true)
            };

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == MainMenuScenePath ||
                    scene.path == CampaignScenePath ||
                    scene.path == Map2ScenePath ||
                    scene.path == Map3ScenePath || scene.path == Map4ScenePath ||
                    scene.path == Map5ScenePath || scene.path == Map6ScenePath ||
                    scene.path == FantasyTD.MapA.Editor.MapAEditorSetup.ScenePath)
                    continue;
                ordered.Add(scene);
            }

            EditorBuildSettings.scenes = ordered.ToArray();
        }
    }
}
#endif
