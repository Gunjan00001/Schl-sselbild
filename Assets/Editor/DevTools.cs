using System.IO;
using Schlusselbild.Page;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class DevTools
{
    [MenuItem("Schlusselbild/Dev/Build Android APK")]
    public static void BuildAndroid()
    {
        PlayerSettings.companyName = "Schlusselbild";
        PlayerSettings.productName = "Schlusselbild";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.schlusselbild.quest");
        PlayerSettings.bundleVersion = "0.3.0";
        PlayerSettings.Android.bundleVersionCode = 3;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.colorSpace = ColorSpace.Linear;

        string androidPlayer = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines", "AndroidPlayer");
        UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath = Path.Combine(androidPlayer, "SDK");
        UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath = Path.Combine(androidPlayer, "NDK");
        UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath = Path.Combine(androidPlayer, "OpenJDK");
        Debug.Log("[BuildAndroid] sdk=" + UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath
                  + " ndk=" + UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath
                  + " jdk=" + UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);

        bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Android, BuildTarget.Android);
        Debug.Log("[BuildAndroid] switched=" + switched + " active=" + EditorUserBuildSettings.activeBuildTarget);

        Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds")));
        string outPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", "Schlusselbild.apk"));

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Schlusselbild/Scenes/Main.unity" },
            locationPathName = outPath,
            target = BuildTarget.Android,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("[BuildAndroid] result=" + report.summary.result
                  + " size=" + report.summary.totalSize
                  + " errors=" + report.summary.totalErrors
                  + " path=" + outPath);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    [MenuItem("Schlusselbild/Dev/Export Interop Page")]
    public static void ExportInteropPage()
    {
        byte[] key = new byte[32];
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = (byte)i;
        }

        string secret = "quest-export interop check :: " + new string('z', 300);
        Texture2D page = PageComposer.Compose(secret, key);
        byte[] png = page.EncodeToPNG();
        Object.DestroyImmediate(page);

        string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, "quest_export_page.png"), png);
        File.WriteAllText(Path.Combine(dir, "quest_export_secret.txt"), secret);
        Debug.Log("[DevTools] exported interop page to " + dir);
    }
}
