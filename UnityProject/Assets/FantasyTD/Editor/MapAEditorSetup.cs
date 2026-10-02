#if UNITY_EDITOR
using System.IO;
using FantasyTD.MapA;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FantasyTD.MapA.Editor
{
    [InitializeOnLoad]
    public static class MapAEditorSetup
    {
        public const string ScenePath = "Assets/FantasyTD/Scenes/MAP_A_ANDROID_V2.unity";

        static MapAEditorSetup()
        {
            EditorApplication.delayCall += EnsureSceneExists;
        }

        [MenuItem("Tools/Fantasy TD/Open MAP-A Android v2")]
        public static void OpenMap()
        {
            EnsureSceneExists();
            EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        }

        [MenuItem("Tools/Fantasy TD/Rebuild MAP-A Android v2")]
        public static void Rebuild()
        {
            if (File.Exists(ScenePath))
                AssetDatabase.DeleteAsset(ScenePath);
            EnsureSceneExists();
        }

        [MenuItem("Tools/Fantasy TD/Configure Android")]
        public static void ConfigureAndroid()
        {
            PlayerSettings.productName = "Fantasy TD";
            PlayerSettings.companyName = "Indie";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.indie.fantasytd");
            Debug.Log("Fantasy TD: Android landscape settings applied.");
        }

        [MenuItem("Tools/Fantasy TD/Validate MAP-A")]
        public static void Validate()
        {
            bool ok = true;
            float length = MapAData.TotalPathLength;
            if (Mathf.Abs(length - 46.2111f) > 0.02f)
            {
                ok = false;
                Debug.LogError("MAP-A path length mismatch: " + length);
            }

            if (MapAData.PlantingSpots.Length != 12)
            {
                ok = false;
                Debug.LogError("MAP-A must contain exactly 12 planting spots.");
            }

            if (ok)
                Debug.Log("MAP-A Android v2 validation PASS. Path=" + length.ToString("0.000") + " RU, spots=12.");
        }

        private static void EnsureSceneExists()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Directory.CreateDirectory("Assets/FantasyTD/Scenes");

            if (!File.Exists(ScenePath))
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                GameObject bootstrap = new GameObject("MAP-A Android v2 Bootstrap");
                bootstrap.AddComponent<MapABootstrap>();
                EditorSceneManager.SaveScene(scene,ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            bool found = false;
            var scenes = EditorBuildSettings.scenes;
            for (int i=0;i<scenes.Length;i++)
            {
                if (scenes[i].path != ScenePath)
                    continue;

                found = true;
                if (!scenes[i].enabled)
                {
                    scenes[i] = new EditorBuildSettingsScene(ScenePath,true);
                    EditorBuildSettings.scenes = scenes;
                }
                break;
            }

            if (!found)
            {
                EditorBuildSettingsScene[] next = new EditorBuildSettingsScene[scenes.Length+1];
                for (int i=0;i<scenes.Length;i++) next[i]=scenes[i];
                next[next.Length-1] = new EditorBuildSettingsScene(ScenePath,true);
                EditorBuildSettings.scenes = next;
            }
        }
    }
}
#endif
