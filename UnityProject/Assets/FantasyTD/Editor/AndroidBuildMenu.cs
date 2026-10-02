#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FantasyTD.MapA.Editor
{
    public static class AndroidBuildMenu
    {
        [MenuItem("Tools/Fantasy TD/Build Android APK")]
        public static void BuildApk()
        {
            MapAEditorSetup.ConfigureAndroid();
            MapAEditorSetup.OpenMap();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);
                if (!switched)
                {
                    Debug.LogError("Could not switch to Android. Install Android Build Support in Unity Hub.");
                    return;
                }
            }

            Directory.CreateDirectory("Builds/Android");
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = new[] { MapAEditorSetup.ScenePath };
            options.locationPathName = "Builds/Android/FantasyTD_MAP_A_v2.apk";
            options.target = BuildTarget.Android;
            options.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log("APK built: " + options.locationPathName);
            else
                Debug.LogError("Android build failed: " + report.summary.result);
        }
    }
}
#endif
