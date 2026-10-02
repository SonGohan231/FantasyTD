#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
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
        private const string ExpectedAndroidIdentifier = "com.indie.fantasytd";

        public static void Baseline()
        {
            Directory.CreateDirectory(ReportDirectory);

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

        public static void MobilePlanting()
        {
            MobilePlantingPlayModeValidator.Start();
        }

        public static void PlantBehaviors()
        {
            PlantBehaviorsPlayModeValidator.Start();
        }

        public static void EnemyWaveFlow()
        {
            EnemyWaveFlowPlayModeValidator.Start();
        }

        public static void BossEndStates()
        {
            BossEndStatesPlayModeValidator.Start();
        }

        public static void MobileLayout()
        {
            MobileLayoutPlayModeValidator.Start();
        }

        public static void BuildAndroid()
        {
            Directory.CreateDirectory(ReportDirectory);

            DateTime startedUtc = DateTime.UtcNow;
            bool androidSupported = BuildPipeline.IsBuildTargetSupported(
                BuildTargetGroup.Android, BuildTarget.Android);

            string sdkPath = GetAndroidExternalToolPath("sdkRootPath");
            string ndkPath = GetAndroidExternalToolPath("ndkRootPath");
            string jdkPath = GetAndroidExternalToolPath("jdkRootPath");
            string sdkPlatforms = ListDirectoryNames(sdkPath, "platforms", "android-");
            string sdkBuildTools = ListDirectoryNames(sdkPath, "build-tools", null);
            string ndkVersion = ReadKeyValueFile(
                Path.Combine(ndkPath ?? string.Empty, "source.properties"), "Pkg.Revision");
            string jdkVersion = ReadKeyValueFile(
                Path.Combine(jdkPath ?? string.Empty, "release"), "JAVA_VERSION");

            BuildReport buildReport = null;
            Exception buildException = null;
            try
            {
                buildReport = AndroidBuildMenu.BuildApk();
            }
            catch (Exception ex)
            {
                buildException = ex;
            }

            DateTime finishedUtc = DateTime.UtcNow;

            string apkPath = Path.GetFullPath(AndroidBuildMenu.ApkPath);
            bool apkExists = File.Exists(apkPath);
            long apkBytes = apkExists ? new FileInfo(apkPath).Length : 0L;
            string apkSha256 = apkExists && apkBytes > 0 ? ComputeSha256(apkPath) : string.Empty;

            EditorBuildSettingsScene mapScene = EditorBuildSettings.scenes
                .FirstOrDefault(scene => scene.path == MapAEditorSetup.ScenePath);
            bool mapPresent = mapScene != null;
            bool mapEnabled = mapScene != null && mapScene.enabled;
            bool mapIncludedInActualBuildOptions =
                AndroidBuildMenu.LastBuildScenes != null &&
                AndroidBuildMenu.LastBuildScenes.Contains(MapAEditorSetup.ScenePath);

            string identifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            string targetSdk = PlayerSettings.Android.targetSdkVersion.ToString();
            string minSdk = PlayerSettings.Android.minSdkVersion.ToString();

            BuildSummary summary = buildReport != null ? buildReport.summary : default(BuildSummary);
            string buildResult = buildReport != null ? summary.result.ToString() : "NoBuildReport";
            double buildDurationSeconds = buildReport != null
                ? summary.totalTime.TotalSeconds
                : (finishedUtc - startedUtc).TotalSeconds;
            ulong reportTotalSize = buildReport != null ? summary.totalSize : 0UL;
            int reportErrors = buildReport != null ? summary.totalErrors : -1;
            int reportWarnings = buildReport != null ? summary.totalWarnings : -1;
            string reportOutputPath = buildReport != null ? summary.outputPath : string.Empty;

            bool outputPathMatches = !string.IsNullOrEmpty(reportOutputPath) &&
                PathsEqual(Path.GetFullPath(reportOutputPath), apkPath);

            string buildScenes = AndroidBuildMenu.LastBuildScenes == null
                ? string.Empty
                : string.Join(";", AndroidBuildMenu.LastBuildScenes);

            string report =
                "DYRYGENT UNITY EXECUTOR T0008 ANDROID BUILD\n" +
                "timestampUtc=" + finishedUtc.ToString("O") + "\n" +
                "startedUtc=" + startedUtc.ToString("O") + "\n" +
                "unityVersion=" + Application.unityVersion + "\n" +
                "projectPath=" + Directory.GetCurrentDirectory() + "\n" +
                "androidBuildSupport=" + androidSupported + "\n" +
                "activeBuildTarget=" + EditorUserBuildSettings.activeBuildTarget + "\n" +
                "sdkPath=" + Safe(sdkPath) + "\n" +
                "sdkPathExists=" + Directory.Exists(sdkPath) + "\n" +
                "sdkInstalledPlatforms=" + Safe(sdkPlatforms) + "\n" +
                "sdkInstalledBuildTools=" + Safe(sdkBuildTools) + "\n" +
                "sdkVersionEvidence=installed platform/build-tools directories under Unity-reported SDK root\n" +
                "ndkPath=" + Safe(ndkPath) + "\n" +
                "ndkPathExists=" + Directory.Exists(ndkPath) + "\n" +
                "ndkVersion=" + Safe(ndkVersion) + "\n" +
                "ndkVersionEvidence=Pkg.Revision from source.properties under Unity-reported NDK root\n" +
                "jdkPath=" + Safe(jdkPath) + "\n" +
                "jdkPathExists=" + Directory.Exists(jdkPath) + "\n" +
                "jdkVersion=" + Safe(jdkVersion) + "\n" +
                "jdkVersionEvidence=JAVA_VERSION from release file under Unity-reported JDK root\n" +
                "toolchainPathSource=UnityEditor.Android.AndroidExternalToolsSettings\n" +
                "androidTargetSdkSetting=" + targetSdk + "\n" +
                "androidMinSdkSetting=" + minSdk + "\n" +
                "applicationIdentifier=" + identifier + "\n" +
                "mapScenePath=" + MapAEditorSetup.ScenePath + "\n" +
                "mapScenePresentInBuildSettings=" + mapPresent + "\n" +
                "mapSceneEnabledInBuildSettings=" + mapEnabled + "\n" +
                "actualBuildScenes=" + Safe(buildScenes) + "\n" +
                "mapIncludedInActualBuildOptions=" + mapIncludedInActualBuildOptions + "\n" +
                "buildResult=" + buildResult + "\n" +
                "buildDurationSeconds=" + buildDurationSeconds.ToString("0.###") + "\n" +
                "buildReportTotalSize=" + reportTotalSize + "\n" +
                "buildReportErrors=" + reportErrors + "\n" +
                "buildReportWarnings=" + reportWarnings + "\n" +
                "buildReportOutputPath=" + Safe(reportOutputPath) + "\n" +
                "buildReportOutputPathMatchesExpected=" + outputPathMatches + "\n" +
                "buildException=" + Safe(buildException == null ? string.Empty :
                    buildException.GetType().Name + ": " + buildException.Message) + "\n" +
                "apkPath=" + apkPath + "\n" +
                "apkExists=" + apkExists + "\n" +
                "apkBytes=" + apkBytes + "\n" +
                "apkSha256=" + apkSha256 + "\n" +
                "deviceInstallRun=False\n" +
                "emulatorSmokeRun=False\n" +
                "physicalDeviceSmokeRun=False\n" +
                "t0009Started=False\n";

            File.WriteAllText(AndroidReport, report);
            Debug.Log(report);

            if (buildException != null)
                throw new Exception("Android build threw before successful completion. See " +
                    AndroidReport, buildException);
            if (!androidSupported)
                throw new Exception("Android Build Support is unavailable.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new Exception("Final active build target is not Android.");
            if (!Directory.Exists(sdkPath) || !Directory.Exists(ndkPath) || !Directory.Exists(jdkPath))
                throw new Exception("Unity Android SDK/NDK/JDK path verification failed.");
            if (!mapPresent || !mapEnabled || !mapIncludedInActualBuildOptions)
                throw new Exception("MAP-A was not present, enabled, and included in the actual build options.");
            if (identifier != ExpectedAndroidIdentifier)
                throw new Exception("Unexpected Android application identifier: " + identifier);
            if (buildReport == null || summary.result != BuildResult.Succeeded)
                throw new Exception("Android BuildReport did not report Succeeded.");
            if (summary.totalErrors != 0)
                throw new Exception("Android BuildReport contains errors: " + summary.totalErrors);
            if (!outputPathMatches)
                throw new Exception("BuildReport output path does not match the expected APK path.");
            if (!apkExists || apkBytes <= 0 || string.IsNullOrEmpty(apkSha256))
                throw new Exception("Android build did not produce a verifiable APK.");
        }

        private static string GetAndroidExternalToolPath(string propertyName)
        {
            const string TypeName = "UnityEditor.Android.AndroidExternalToolsSettings";
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(TypeName, false);
                if (type == null)
                    continue;

                PropertyInfo property = type.GetProperty(
                    propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != null)
                {
                    object value = property.GetValue(null, null);
                    if (value != null)
                        return value.ToString();
                }

                FieldInfo field = type.GetField(
                    propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    object value = field.GetValue(null);
                    if (value != null)
                        return value.ToString();
                }
            }

            return string.Empty;
        }

        private static string ReadKeyValueFile(string path, string key)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return string.Empty;

            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();
                int equals = line.IndexOf('=');
                if (equals <= 0)
                    continue;

                string foundKey = line.Substring(0, equals).Trim();
                if (!string.Equals(foundKey, key, StringComparison.OrdinalIgnoreCase))
                    continue;

                return line.Substring(equals + 1).Trim().Trim('"');
            }

            return string.Empty;
        }

        private static string ListDirectoryNames(string root, string child, string prefix)
        {
            if (string.IsNullOrEmpty(root))
                return string.Empty;

            string path = Path.Combine(root, child);
            if (!Directory.Exists(path))
                return string.Empty;

            string[] names = Directory.GetDirectories(path)
                .Select(Path.GetFileName)
                .Where(name => string.IsNullOrEmpty(prefix) ||
                               name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return string.Join(";", names);
        }

        private static string ComputeSha256(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(
                Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.Ordinal);
        }

        private static string Safe(string value)
        {
            return (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        }
    }
}
#endif
