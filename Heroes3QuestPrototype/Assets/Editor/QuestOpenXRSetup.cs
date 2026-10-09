#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.Management;

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
            var general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(group);
            if (general == null || general.AssignedSettings == null)
            {
                XRGeneralSettingsPerBuildTarget collection;
                if (!EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(
                        XRGeneralSettings.k_SettingsKey, out collection) || collection == null)
                {
                    if (!AssetDatabase.IsValidFolder("Assets/XR"))
                        AssetDatabase.CreateFolder("Assets", "XR");
                    collection = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    AssetDatabase.CreateAsset(collection, "Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                    AssetDatabase.SaveAssets();
                    EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, collection, true);
                }
                if (collection.SettingsForBuildTarget(group) == null)
                    collection.CreateDefaultSettingsForBuildTarget(group);
                if (!collection.HasManagerSettingsForBuildTarget(group))
                    collection.CreateDefaultManagerSettingsForBuildTarget(group);
                AssetDatabase.SaveAssets();
                general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(group);
            }
            if (general == null || general.AssignedSettings == null)
                throw new InvalidOperationException("Android XR Manager settings could not be created.");

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