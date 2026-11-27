using System.IO;
using Schlusselbild.App;
using Schlusselbild.Page;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;

public static class DevTools
{
    [MenuItem("Schlusselbild/Dev/Build Android APK")]
    public static void BuildAndroid()
    {
        PlayerSettings.companyName = "Schlusselbild";
        PlayerSettings.productName = "Schlusselbild";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.schlusselbild.quest");
        PlayerSettings.bundleVersion = "0.7.0";
        PlayerSettings.Android.bundleVersionCode = 9;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
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

    [MenuItem("Schlusselbild/Dev/Capture Game View")]
    public static void CaptureGameView()
    {
        string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "quest_ui.png");

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        Camera cam = canvas != null && canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogError("[DevTools] no camera available for capture");
            return;
        }

        int w = 1240;
        int h = 940;
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 24);
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture prevTarget = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = prevTarget;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        RenderTexture.active = prevActive;
        RenderTexture.ReleaseTemporary(rt);

        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        Debug.Log("[DevTools] wrote " + path);
    }

    [MenuItem("Schlusselbild/Dev/Capture All Screens")]
    public static void CaptureAllScreens()
    {
        UiBootstrap boot = Object.FindAnyObjectByType<UiBootstrap>();
        if (boot == null || boot.Controller == null)
        {
            Debug.LogError("[DevTools] no UiBootstrap/AppController in the scene");
            return;
        }

        AppController controller = boot.Controller;
        controller.ShowHome();
        CaptureCanvas("home");
        controller.ShowCreate();
        CaptureCanvas("create");
        controller.ShowScan();
        CaptureCanvas("scan");
        controller.ShowDecoded("meet at dawn - 0400 pier 7");
        CaptureCanvas("decoded");
        controller.ShowSettings();
        CaptureCanvas("settings");
        controller.ShowHome();
        Debug.Log("[DevTools] captured all screens");
    }

    private static void CaptureCanvas(string name)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        Camera cam = canvas != null && canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogError("[DevTools] no camera available for capture");
            return;
        }

        if (canvas != null)
        {
            CanvasGroup[] groups = canvas.GetComponentsInChildren<CanvasGroup>(true);
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].alpha = 1f;
            }

            Transform screens = canvas.transform.Find("Screens");
            if (screens != null)
            {
                for (int i = 0; i < screens.childCount; i++)
                {
                    screens.GetChild(i).localScale = Vector3.one;
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)canvas.transform);
            Canvas.ForceUpdateCanvases();
        }

        int w = 1240;
        int h = 940;
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 24);
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture prevTarget = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = prevTarget;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        RenderTexture.active = prevActive;
        RenderTexture.ReleaseTemporary(rt);

        string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, "ui_" + name + ".png"), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        Debug.Log("[DevTools] captured " + name);
    }
}
