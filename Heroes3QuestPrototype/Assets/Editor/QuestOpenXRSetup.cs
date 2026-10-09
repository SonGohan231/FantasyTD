#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;

namespace Heroes3QuestPrototype.Editor
{
    /// <summary>
    /// Android OpenXR configuration for this isolated Unity 6 prototype.
    /// Fails loudly if a required feature or Android XR settings cannot be resolved.
    /// </summary>
    public static class QuestOpenXRSetup
    {
        [MenuItem("Tools/Heroes III Quest/Enable OpenXR for Quest (validate)")]
        public static void ConfigureForQuest()
        {
            const BuildTargetGroup group = BuildTargetGroup.Android;
            var general = XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget(BuildTarget.Android);
            if (general == null || general.AssignedSettings == null)
                throw new InvalidOperationException(
                    "XR Management Android settings missing. Open Edit > Project Settings > XR Plug-in Management and configure Android first.");

            const string loader = "UnityEngine.XR.OpenXR.OpenXRLoader";
            if (!XRPackageMetadataStore.AssignLoader(general.AssignedSettings, loader, group))
                throw new InvalidOperationException(
                    "OpenXRLoader assignment failed. Verify the com.unity.xr.openxr package in the manifest.");

            var openXR = OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if (openXR == null)
                throw new InvalidOperationException(
                    "OpenXRSettings Android asset missing; configure the Android OpenXR provider in Project Settings.");

            int questEnabled = 0;
            int touchEnabled = 0;
            foreach(var feature in openXR.GetFeatures<OpenXRFeature>())
            {
                var name=feature.GetType().Name;
                if (name == "MetaQuestFeature" || name == "MetaQuestSupport")
                {
                    feature.enabled = true;
                    questEnabled++;
                }
                if (name == "OculusTouchControllerProfile")
                {
                    feature.enabled = true;
                    touchEnabled++;
                }
            }
            EditorUtility.SetDirty(openXR);
            AssetDatabase.SaveAssets();

            if (questEnabled == 0 || touchEnabled == 0)
                throw new InvalidOperationException(
                    "Missing OpenXR Meta Quest Support or Oculus Touch Controller Profile. Enable these Android features in Project Settings.");

            Debug.Log("QUEST_OPENXR_CONFIGURED: Android loader, Meta Quest Support and Oculus Touch profile enabled.");
        }
    }
}
#endif