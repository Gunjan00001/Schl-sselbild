using System;
using Schlusselbild.Capture;
using Schlusselbild.Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Schlusselbild.App
{
    public sealed class UiBootstrap : MonoBehaviour
    {
        public const float CanvasWidthPixels = 1240f;
        public const float CanvasHeightPixels = 940f;
        public const float CanvasDistanceMeters = 1f;
        public const float CanvasVerticalOffset = -0.04f;

        private const int PlacementSettleFrames = 60;

        private AppSettings settings;
        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform screensRoot;
        private AppController controller;
        private PassthroughScanner scanner;
        private Camera eyeCamera;
        private int placementFramesRemaining;
        private bool built;

        public AppController Controller
        {
            get { return controller; }
        }

        public Canvas UiCanvas
        {
            get { return canvas; }
        }

        private void Awake()
        {
            settings = AppSettings.Load();
            UiTheme.SetTextScale(settings.TextScale);
            UiTheme.SetColorblindSafe(settings.ColorblindSafe);

            EnsureEventSystem();
            BuildCanvas();
            ApplyCanvasPlacement();

            scanner = GetComponent<PassthroughScanner>();

            controller = gameObject.AddComponent<AppController>();
            controller.Initialize(settings, screensRoot, scanner);

            BuildPointers();
            ApplyTrackingOrigin();
            ApplyPointerSettings();

            settings.Changed += HandleSettingsChanged;
            placementFramesRemaining = PlacementSettleFrames;
            built = true;
        }

        private void LateUpdate()
        {
            if (!built)
            {
                return;
            }

            if (placementFramesRemaining <= 0)
            {
                return;
            }

            placementFramesRemaining--;
            ApplyCanvasPlacement();
        }

        private void OnDestroy()
        {
            if (settings != null)
            {
                settings.Changed -= HandleSettingsChanged;
            }
        }

        public void ApplyCanvasPlacement()
        {
            if (canvasRect == null || settings == null)
            {
                return;
            }

            Transform eye = ResolveEyeAnchor();
            Vector3 origin = eye == null ? transform.position : eye.position;
            Vector3 forward = eye == null ? transform.forward : eye.forward;
            Vector3 up = eye == null ? Vector3.up : eye.up;

            float distance = AppSettings.ClampUiDistance(settings.UiDistance);
            Vector3 position = origin + forward * distance + up * CanvasVerticalOffset;

            canvasRect.position = position;
            canvasRect.rotation = Quaternion.LookRotation(position - origin, up);
            canvasRect.localScale = new Vector3(1f / UiTheme.PixelsPerMeter, 1f / UiTheme.PixelsPerMeter, 1f / UiTheme.PixelsPerMeter);
        }

        public void ApplyTrackingOrigin()
        {
            if (settings == null)
            {
                return;
            }

            OVRManager manager = FindAnyObjectByType<OVRManager>();
            if (manager == null)
            {
                return;
            }

            try
            {
                manager.trackingOriginType = settings.SeatedMode ? OVRManager.TrackingOrigin.EyeLevel : OVRManager.TrackingOrigin.FloorLevel;
            }
            catch (Exception)
            {
            }
        }

        private void HandleSettingsChanged()
        {
            ApplyCanvasPlacement();
            ApplyTrackingOrigin();
            ApplyPointerSettings();
        }

        private void ApplyPointerSettings()
        {
            if (eyeCamera == null)
            {
                eyeCamera = ResolveEyeCamera();
            }

            if (canvas != null && eyeCamera != null)
            {
                canvas.worldCamera = eyeCamera;
            }
        }

        private void EnsureEventSystem()
        {
            EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject created = new GameObject("EventSystem");
                created.transform.SetParent(transform, false);
                created.AddComponent<EventSystem>();
            }
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("UiCanvas", typeof(RectTransform), typeof(Canvas), typeof(OVRRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = ResolveEyeCamera();
            canvas.sortingOrder = 0;

            canvasRect = (RectTransform)canvasObject.transform;
            canvasRect.sizeDelta = new Vector2(CanvasWidthPixels, CanvasHeightPixels);
            canvasRect.localScale = new Vector3(1f / UiTheme.PixelsPerMeter, 1f / UiTheme.PixelsPerMeter, 1f / UiTheme.PixelsPerMeter);

            RectTransform background = UiFactory.Panel(canvasRect, "Background", UiTheme.Palette.Background, true);
            UiFactory.Stretch(background, 0f, 0f, 0f, 0f);

            screensRoot = UiFactory.CreateRect("Screens", canvasRect);
            UiFactory.Stretch(screensRoot, 0f, 0f, 0f, 0f);
        }

        private void BuildPointers()
        {
            OVRCameraRig rig = FindAnyObjectByType<OVRCameraRig>();
            Transform leftAnchor = rig == null ? null : rig.leftHandAnchor;
            Transform rightAnchor = rig == null ? null : rig.rightHandAnchor;

            OvrUiInput input = gameObject.AddComponent<OvrUiInput>();
            input.Configure(canvas, ResolveEyeCamera(), leftAnchor, rightAnchor);

            if (Application.isEditor)
            {
                MouseUiInput mouse = gameObject.AddComponent<MouseUiInput>();
                mouse.Configure(canvas);
            }
        }

        private Transform ResolveEyeAnchor()
        {
            Camera camera = ResolveEyeCamera();
            if (camera != null)
            {
                return camera.transform;
            }

            OVRCameraRig rig = FindAnyObjectByType<OVRCameraRig>();
            return rig == null ? null : rig.centerEyeAnchor;
        }

        private Camera ResolveEyeCamera()
        {
            if (eyeCamera != null)
            {
                return eyeCamera;
            }

            OVRCameraRig rig = FindAnyObjectByType<OVRCameraRig>();
            if (rig != null)
            {
                if (rig.centerEyeAnchor != null)
                {
                    Camera center = rig.centerEyeAnchor.GetComponent<Camera>();
                    if (center != null)
                    {
                        eyeCamera = center;
                        return eyeCamera;
                    }
                }

                if (rig.leftEyeCamera != null)
                {
                    eyeCamera = rig.leftEyeCamera;
                    return eyeCamera;
                }
            }

            eyeCamera = Camera.main;
            return eyeCamera;
        }
    }
}