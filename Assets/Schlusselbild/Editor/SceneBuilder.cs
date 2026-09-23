using System.Collections.Generic;
using System.IO;
using Schlusselbild.App;
using Schlusselbild.Capture;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneBuilder
{
    private const string ScenePath = "Assets/Schlusselbild/Scenes/Main.unity";
    private const string SceneFolder = "Assets/Schlusselbild/Scenes";
    private const string RigPrefabPath = "Packages/com.meta.xr.sdk.core/Prefabs/OVRCameraRig.prefab";

    [MenuItem("Schlusselbild/Build Main Scene")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject rig = CreateRig();
        ConfigurePassthrough(rig);
        AddPassthroughLayer(rig);
        CreateAppRoot();

        Directory.CreateDirectory(SceneFolder);
        AssetDatabase.Refresh();

        bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        AddToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();

        Debug.Log("[SceneBuilder] Main scene saved: " + ScenePath + " (saved=" + saved + ")");
    }

    private static GameObject CreateRig()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefabPath);
        if (prefab != null)
        {
            Object instance = PrefabUtility.InstantiatePrefab(prefab);
            GameObject rig = instance as GameObject;
            if (rig != null)
            {
                rig.name = "OVRCameraRig";
                Debug.Log("[SceneBuilder] Instantiated " + RigPrefabPath);
                return rig;
            }
        }

        Debug.LogWarning("[SceneBuilder] " + RigPrefabPath + " was not found, creating a fallback camera rig instead.");
        return CreateFallbackRig();
    }

    private static GameObject CreateFallbackRig()
    {
        GameObject rig = new GameObject("OVRCameraRig");
        GameObject trackingSpace = new GameObject("TrackingSpace");
        trackingSpace.transform.SetParent(rig.transform, false);

        CreateEye(trackingSpace.transform, "CenterEyeAnchor", true);
        CreateEye(trackingSpace.transform, "LeftEyeAnchor", false);
        CreateEye(trackingSpace.transform, "RightEyeAnchor", false);

        CreateAnchor(trackingSpace.transform, "LeftHandAnchor");
        CreateAnchor(trackingSpace.transform, "RightHandAnchor");

        rig.AddComponent<OVRManager>();
        return rig;
    }

    private static void CreateEye(Transform parent, string name, bool mainCamera)
    {
        GameObject anchor = CreateAnchor(parent, name);
        GameObject cameraObject = new GameObject(name.Replace("Anchor", "Camera"));
        cameraObject.transform.SetParent(anchor.transform, false);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 1000f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;

        if (mainCamera)
        {
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
        }
    }

    private static GameObject CreateAnchor(Transform parent, string name)
    {
        GameObject anchor = new GameObject(name);
        anchor.transform.SetParent(parent, false);
        return anchor;
    }

    private static void ConfigurePassthrough(GameObject rig)
    {
        OVRManager manager = rig.GetComponentInChildren<OVRManager>(true);
        if (manager != null)
        {
            manager.isInsightPassthroughEnabled = true;
            EditorUtility.SetDirty(manager);
        }
        else
        {
            Debug.LogWarning("[SceneBuilder] No OVRManager on the rig; passthrough stays disabled.");
        }

        OVRProjectConfig projectConfig = OVRProjectConfig.CachedProjectConfig;
        if (projectConfig == null)
        {
            Debug.LogWarning("[SceneBuilder] OVRProjectConfig is unavailable; passthrough support was not set to Supported.");
            return;
        }

        projectConfig.insightPassthroughSupport = OVRProjectConfig.FeatureSupport.Supported;
        projectConfig.isPassthroughCameraAccessEnabled = true;
        OVRProjectConfig.CommitProjectConfig(projectConfig);
        Debug.Log("[SceneBuilder] Passthrough support = Supported, passthrough camera access enabled.");
    }

    private static void AddPassthroughLayer(GameObject rig)
    {
        if (rig.GetComponent<OVRPassthroughLayer>() == null)
        {
            rig.AddComponent<OVRPassthroughLayer>();
        }
    }

    private static void CreateAppRoot()
    {
        GameObject appRoot = new GameObject("AppRoot");
        appRoot.AddComponent<UiBootstrap>();
        appRoot.AddComponent<PassthroughScanner>();
    }

    private static void AddToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));

        EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i] != null && existing[i].path != scenePath)
            {
                scenes.Add(existing[i]);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[SceneBuilder] Build settings now start with " + scenePath + " (" + scenes.Count + " scene(s)).");
    }
}