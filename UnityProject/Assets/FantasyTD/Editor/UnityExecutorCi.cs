#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using FantasyTD.MapA.Editor;

namespace FantasyTD.CI
{
    public static class UnityExecutorCi
    {
        private const string ReportDirectory = "Artifacts/UnityExecutor";
        private const string BaselineReport = ReportDirectory + "/baseline.txt";
        private const string MapAIntegrityReport = ReportDirectory + "/map-a-integrity.txt";
        private const string AndroidReport = ReportDirectory + "/android-build.txt";

        public static void Baseline()
        {
            Directory.CreateDirectory(ReportDirectory);

            // Opening the expected map is allowed to create its generated scene through
            // the existing project bootstrap. This is repository-defined behavior.
            MapAEditorSetup.OpenMap();

            bool sceneExists = File.Exists(MapAEditorSetup.ScenePath);
            bool sceneInBuildSettings = EditorBuildSettings.scenes
                .Any(scene => scene.enabled && scene.path == MapAEditorSetup.ScenePath);
            bool androidSupported = BuildPipeline.IsBuildTargetSupported(
                BuildTargetGroup.Android,
                BuildTarget.Android);

            string report =
                "DYRYGENT UNITY EXECUTOR BASELINE\n" +
                "timestampUtc=" + DateTime.UtcNow.ToString("O") + "\n" +
                "unityVersion=" + Application.unityVersion + "\n" +
                "projectPath=" + Directory.GetCurrentDirectory() + "\n" +
                "compileReachedExecutor=true\n" +
                "scenePath=" + MapAEditorSetup.ScenePath + "\n" +
                "sceneExists=" + sceneExists + "\n" +
                "sceneEnabledInBuildSettings=" + sceneInBuildSettings + "\n" +
                "activeBuildTarget=" + EditorUserBuildSettings.activeBuildTarget + "\n" +
                "androidModuleSupported=" + androidSupported + "\n" +
                "isBatchMode=" + Application.isBatchMode + "\n";

            File.WriteAllText(BaselineReport, report);
            Debug.Log(report);

            if (!sceneExists)
                throw new Exception("Expected MAP-A scene was not generated.");
            if (!sceneInBuildSettings)
                throw new Exception("MAP-A scene is not enabled in Build Settings.");
        }

        public static void MapAIntegrity()
        {
            Directory.CreateDirectory(ReportDirectory);
            MapAIntegrityValidator.Result result = MapAIntegrityValidator.Run();
            string report = result.ToReportText();
            File.WriteAllText(MapAIntegrityReport, report);
            Debug.Log(report);

            if (!result.Success)
                throw new Exception("MAP-A integrity validation failed. See " + MapAIntegrityReport);
        }

        public static void BuildAndroid()
        {
            Directory.CreateDirectory(ReportDirectory);
            AndroidBuildMenu.BuildApk();

            string apkPath = Path.GetFullPath("Builds/Android/FantasyTD_MAP_A_v2.apk");
            bool exists = File.Exists(apkPath);
            long size = exists ? new FileInfo(apkPath).Length : 0L;

            string report =
                "DYRYGENT UNITY EXECUTOR ANDROID\n" +
                "timestampUtc=" + DateTime.UtcNow.ToString("O") + "\n" +
                "unityVersion=" + Application.unityVersion + "\n" +
                "apkPath=" + apkPath + "\n" +
                "apkExists=" + exists + "\n" +
                "apkBytes=" + size + "\n";

            File.WriteAllText(AndroidReport, report);
            Debug.Log(report);

            if (!exists || size <= 0)
                throw new Exception("Android build did not produce the expected APK.");
        }
    }
}
#endif
