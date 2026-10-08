#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

namespace QuestGardenVR
{
    public static class QuestBuilder
    {
        const string ScenePath="Assets/Scenes/QuestGarden.unity";
        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("Garden 6DoF VR gameplay").AddComponent<GardenRuntime>();
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            PlayerSettings.companyName="Private VR Experiment";
            PlayerSettings.productName="Garden Defense Quest VR";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.personal.questgardenvr.prototype");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            XRGeneralSettingsPerBuildTarget store;
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out store)) store = null;
            if(store==null)
            {
                Directory.CreateDirectory("Assets/XR");
                store=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(store,"Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,store,true);
            }
            var xr=store.SettingsForBuildTarget(BuildTargetGroup.Android);
            if(xr==null||xr.Manager==null)
            {
                store.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
                xr=store.SettingsForBuildTarget(BuildTargetGroup.Android);
            }
            if(xr==null||xr.Manager==null)throw new Exception("Android XR manager missing");
            XRPackageMetadataStore.AssignLoader(xr.Manager,typeof(OpenXRLoader).FullName,BuildTargetGroup.Android);
            bool loader=false;
            foreach(var l in xr.Manager.activeLoaders)if(l is OpenXRLoader)loader=true;
            if(!loader)throw new Exception("Android OpenXRLoader not installed; refuse non-VR APK");
            xr.InitManagerOnStart=true;
            var openxr=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if(openxr==null)throw new Exception("Android OpenXR config missing");
            bool touch=false;
            foreach(var feature in openxr.GetFeatures())
            {
                if(feature.GetType().Name=="OculusTouchControllerProfile")
                {feature.enabled=true;touch=true;EditorUtility.SetDirty(feature);}
            }
            if(!touch)throw new Exception("OculusTouchControllerProfile unavailable");
            EditorUtility.SetDirty(store);EditorUtility.SetDirty(xr);EditorUtility.SetDirty(xr.Manager);EditorUtility.SetDirty(openxr);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Builds");
            var options=new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/QuestGardenVR.apk",target=BuildTarget.Android,options=BuildOptions.None};
            var report=BuildPipeline.BuildPlayer(options);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Unity Android build failed: "+report.summary.result);
            Debug.Log("Built Builds/QuestGardenVR.apk (Quest runtime test still required)");
        }
    }
}
#endif