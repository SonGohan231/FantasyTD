#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace FantasyTD.V2.Editor
{
    /// <summary>
    /// Editor-only CI gate: proves the pinned MCP for Unity package is actually
    /// registered by Unity Package Manager, not merely present in manifest.json.
    /// Does not claim an MCP client or server is connected.
    /// </summary>
    public static class UnityMcpSetupSmoke
    {
        public static void Run()
        {
            PackageInfo installed = null;
            foreach (PackageInfo package in PackageInfo.GetAllRegisteredPackages())
                if (package.name == "com.coplaydev.unity-mcp")
                {
                    installed = package;
                    break;
                }
            if (installed == null)
                throw new InvalidOperationException(
                    "MCP for Unity is absent from Unity Package Manager registry.");
            if (installed.version != "10.3.0")
                throw new InvalidOperationException(
                    "MCP for Unity version mismatch; expected 10.3.0 got " +
                    installed.version);
            if (!AssetDatabase.IsValidFolder("Packages/com.coplaydev.unity-mcp"))
                throw new InvalidOperationException(
                    "MCP for Unity package content is not mounted in Assets database.");
            Debug.Log("FT2 Unity MCP 10.3.0: PACKAGE IMPORT PASS. " +
                "Server connection and Play Mode editor automation NOT TESTED.");
        }
    }
}
#endif
