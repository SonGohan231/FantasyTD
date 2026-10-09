#if UNITY_EDITOR
using System;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace Heroes3QuestPrototype.Editor
{
    /// <summary>
    /// Android 11+ package visibility for the externally installed VCMI package.
    /// Runs after Gradle generation. Does not replace Unity or OpenXR's VR activity manifest.
    /// </summary>
    public sealed class VCMIQueryManifestPatcher : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder { get { return 850; } }

        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
            var manifests = new[] {
                Path.Combine(basePath, "src", "main", "AndroidManifest.xml"),
                Path.Combine(basePath, "unityLibrary", "src", "main", "AndroidManifest.xml"),
                Path.Combine(basePath, "launcher", "src", "main", "AndroidManifest.xml")
            };
            string manifest = null;
            foreach (var path in manifests)
                if (File.Exists(path)) { manifest = path; break; }
            if (manifest == null)
                throw new FileNotFoundException("VCMI visibility patch: Unity Android manifest not found", basePath);

            var document = new XmlDocument();
            document.Load(manifest);
            var root = document.DocumentElement;
            if (root == null || root.LocalName != "manifest")
                throw new InvalidDataException("VCMI visibility patch: invalid Android manifest");
            var ns = new XmlNamespaceManager(document.NameTable);
            ns.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            var queries = root.SelectSingleNode("queries") as XmlElement;
            if (queries == null)
            {
                queries = document.CreateElement("queries");
                root.AppendChild(queries);
            }
            bool found = false;
            foreach (XmlNode n in queries.ChildNodes)
                if (n is XmlElement e && e.LocalName == "package" &&
                    e.GetAttribute("name", "http://schemas.android.com/apk/res/android") == VCMIAndroidLauncher.AndroidPackageId)
                    found = true;
            if (!found)
            {
                var package = document.CreateElement("package");
                package.SetAttribute("name", "http://schemas.android.com/apk/res/android", VCMIAndroidLauncher.AndroidPackageId);
                queries.AppendChild(package);
            }
            document.Save(manifest);
            Debug.Log("VCMI_VISIBILITY_PATCHED " + manifest + " -> " + VCMIAndroidLauncher.AndroidPackageId);
        }
    }
}
#endif