using System;
using UnityEngine;

namespace Heroes3QuestPrototype
{
    /// <summary>
    /// Real Android OS hand-off to the separately installed VCMI app.
    /// This is NOT a JNI bridge to VCMI game state and is NOT a VR renderer for VCMI.
    /// VCMI and proprietary game data are never embedded inside this APK.
    /// </summary>
    public static class VCMIAndroidLauncher
    {
        // Checked against VCMI's F-Droid package listing.
        public const string AndroidPackageId = "is.xyz.vcmi";

        public static bool IsInstalled()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
                using (var intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", AndroidPackageId))
                    return intent != null;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogWarning("VCMI package query failed: " + e.Message);
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns a human-readable, truthful state. Does not report launched until startActivity returns.
        /// </summary>
        public static string TryLaunch()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
                using (var intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", AndroidPackageId))
                {
                    if (intent == null)
                        return "VCMI missing. Sideload official VCMI ARM64 APK and import legal Heroes III data.";
                    activity.Call("startActivity", intent);
                    return "VCMI Android launch intent sent. Game runs as a separate 2D app, not inside VR.";
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("VCMI launch failed: " + e);
                return "Could not launch VCMI; check installed package and Quest app-switching permissions.";
            }
#else
            return "VCMI launcher is available in Android/Quest builds only.";
#endif
        }
    }
}
