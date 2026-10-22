#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
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

            // Validate and repair tracked legacy .meta files before Android packaging.
            Forest01ArtworkImporter.EnsureSpriteImported(
                "Assets/Resources/FT2/Forest01Art.png");
            foreach (string unitFile in new[] {
                "ThornShooter", "FrostBud", "EssenceFlower", "OakGuardian",
                "Wanderer", "Runner", "Armored", "ForestBoss"
            })
                Forest01ArtworkImporter.EnsureSpriteImported(
                    "Assets/Resources/FT2/Units/" + unitFile + ".png");

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
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            PlayerSettings.companyName = "Indie";
            PlayerSettings.productName = "Fantasy TD";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android, "com.indie.fantasytd");
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
            EditorBuildSettings.scenes = new[] {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) !=
                    ScriptingImplementation.IL2CPP ||
                PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64 ||
                PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) !=
                    "com.indie.fantasytd")
                throw new InvalidOperationException(
                    "Android settings did not persist: expected IL2CPP, ARM64 and com.indie.fantasytd.");

            Debug.Log("FT2 Forest01 Android settings: Unity=" + Application.unityVersion +
                " backend=IL2CPP architectures=ARM64 package=com.indie.fantasytd" +
                " minSdk=AndroidApiLevel23 targetSdk=AndroidApiLevelAuto" +
                " signing=UnityDebugKeystore scenes=" + ScenePath);

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
                " bytes=" + new FileInfo(apkPath).Length +
                " result=" + report.summary.result +
                " duration=" + report.summary.totalTime);
        }
    }
}
#endif
