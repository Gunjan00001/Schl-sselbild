using System;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

/// <summary>
/// One-shot, idempotent package installer for the Quest 3 port.
/// The Meta scoped registry lives in Packages/manifest.json.
/// Run headlessly:
///   Unity.exe -batchmode -quit -projectPath "D:\Schl-sselbild" \
///             -executeMethod ProjectBootstrap.AddPackages -logFile -
/// </summary>
public static class ProjectBootstrap
{
    // UPM packages resolved to the latest compatible version.
    static readonly string[] Packages =
    {
        "com.unity.xr.management",
        "com.unity.xr.openxr",
        "com.unity.xr.meta-openxr",
        "com.unity.inputsystem",
        "com.meta.xr.sdk.all",
        "com.meta.xr.mrutilitykit",
        // Unity Editor bridge for MCP (CoderGamester).
        "https://github.com/CoderGamester/mcp-unity.git",
    };

    public static void AddPackages()
    {
        int failures = 0;
        foreach (var pkg in Packages)
        {
            Debug.Log($"[ProjectBootstrap] Adding: {pkg}");
            var req = Client.Add(pkg);
            Wait(req);
            if (req.Status == StatusCode.Success)
                Debug.Log($"[ProjectBootstrap] OK  {req.Result.packageId}");
            else
            {
                failures++;
                Debug.LogError($"[ProjectBootstrap] FAIL {pkg}: {req.Error?.message}");
            }
        }
        Debug.Log($"[ProjectBootstrap] Done. failures={failures}");
    }

    static void Wait(Request req)
    {
        while (!req.IsCompleted)
            Thread.Sleep(100);
    }
}
