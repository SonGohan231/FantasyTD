#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
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
            MapAEditorSetup.OpenMap();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android,BuildTarget.Android);
                if (!switched || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    throw new InvalidOperationException(
                        "Could not switch the active build target to Android.");
            }

            EditorBuildSettingsScene mapScene = EditorBuildSettings.scenes
                .FirstOrDefault(scene => scene.path == MapAEditorSetup.ScenePath);
            if (mapScene == null || !mapScene.enabled)
                throw new InvalidOperationException(
                    "MAP-A scene is missing or disabled in Build Settings: " +
                    MapAEditorSetup.ScenePath);

            Directory.CreateDirectory("Builds/Android");
            if (File.Exists(ApkPath))
                File.Delete(ApkPath);

            LastBuildScenes = new[] { MapAEditorSetup.ScenePath };
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
