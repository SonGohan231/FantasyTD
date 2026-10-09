#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEngine;

namespace Heroes3QuestPrototype.Editor
{
    public static class QuestProjectSetup
    {
        private const string ScenePath = "Assets/Scenes/HeroesQuestBootstrap.unity";

        [MenuItem("Tools/Heroes III Quest/Create Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Heroes3 Quest VR prototype - bootstrap");
            root.AddComponent<QuestHeroesPrototype>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.Refresh();
            Debug.Log("Quest prototype scene created at "+ScenePath+". Configure Android/OpenXR before building.");
        }

        [MenuItem("Tools/Heroes III Quest/Configure Android Player")]
        public static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "HeroesQuestPrototype";
            PlayerSettings.productName = "Heroes3 Quest 2.5D Technical Prototype";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "org.somaskan.heroes3quest.prototype");
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            EditorUserBuildSettings.buildAppBundle = false;
            Debug.Log("Configured Android ARM64 player. OPENXR AND META QUEST SUPPORT MUST BE ENABLED MANUALLY in XR Plug-in Management.");
        }

        [MenuItem("Tools/Heroes III Quest/Build Quest APK (requires configured OpenXR)")]
        public static void BuildQuestApk()
        {
            if (!File.Exists(ScenePath))
                throw new System.InvalidOperationException("Run Create Prototype Scene first.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new System.InvalidOperationException("Switch build target to Android/Meta Quest first.");
            Directory.CreateDirectory("Builds");
            var options = new BuildPlayerOptions {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Heroes3Quest-Prototype.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            var result = BuildPipeline.BuildPlayer(options);
            if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("APK build failed. See Unity Editor logs.");
            Debug.Log("APK build completed: "+options.locationPathName+"; hardware test still required.");
        }
    }
}
#endif