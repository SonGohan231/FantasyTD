#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FantasyTD.V2.Editor
{
    /// <summary>
    /// Explicit Android DEVELOPMENT build of Forest01 only.
    /// Never substitutes the legacy MAP-A builder or claims 30-level completion.
    /// </summary>
    public static class Forest01AndroidBuild
    {
        public const string ScenePath = "Assets/FantasyTD/V2/Scenes/Forest01.unity";

        [MenuItem("Tools/Fantasy TD V2/Build Forest01 Android Debug APK")]
        public static void BuildApk()
        {
            if (!BuildPipeline.IsBuildTargetSupported(
                    BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Unity Android Build Support missing.");

            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
                throw new InvalidOperationException("Forest01 scene not imported.");

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android, BuildTarget.Android);
                if (!switched || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    throw new InvalidOperationException("Could not switch to Android.");
            }

            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            string buildDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "Builds", "Android");
            Directory.CreateDirectory(buildDir);
            string apkPath = Path.Combine(buildDir, "FantasyTD_FT2_Forest01_Debug.apk");
            if (File.Exists(apkPath)) File.Delete(apkPath);

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            });

            if (report.summary.result != BuildResult.Succeeded ||
                !File.Exists(apkPath))
                throw new InvalidOperationException(
                    "Forest01 Android APK build failed: " + report.summary.result);

            Debug.Log("FT2 Forest01 DEBUG APK built: " + apkPath +
                " bytes=" + new FileInfo(apkPath).Length);
        }
    }
}
#endif
