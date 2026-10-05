#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using FantasyTD.Product.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FantasyTD.MapA.Editor
{
    public static class AndroidBuildMenu
    {
        public const string ApkPath = "Builds/Android/FantasyTD_MAP_A_v2.apk";

        public static string[] LastBuildScenes { get; private set; }

        [MenuItem("Tools/Fantasy TD/Build Android APK")]
        public static BuildReport BuildApk()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
                throw new InvalidOperationException("Unity Android Build Support is not available.");

            MapAEditorSetup.ConfigureAndroid();
            ProductFlowEditorSetup.EnsureProductScenes();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android,BuildTarget.Android);
                if (!switched || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    throw new InvalidOperationException("Could not switch the active build target to Android.");
            }

            foreach (string required in ProductFlowEditorSetup.ReleaseScenePaths)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes
                    .FirstOrDefault(item => item.path == required);
                if (scene == null || !scene.enabled)
                    throw new InvalidOperationException("Required release scene is missing or disabled: " + required);
            }

            Directory.CreateDirectory("Builds/Android");
            if (File.Exists(ApkPath))
                File.Delete(ApkPath);

            LastBuildScenes = ProductFlowEditorSetup.ReleaseScenePaths;
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = LastBuildScenes,
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log("APK built: " + options.locationPathName);
            else
                Debug.LogError("Android build failed: " + report.summary.result);

            return report;
        }
    }
}
#endif
