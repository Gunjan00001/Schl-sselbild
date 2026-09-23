using UnityEngine;
using UnityEngine.EventSystems;

namespace Schlusselbild.Ui
{
    public sealed class OvrUiInput : MonoBehaviour
    {
        private Canvas canvas;
        private Camera eyeCamera;
        private OVRInputModule module;
        private Transform leftAnchor;
        private Transform rightAnchor;
        private LineRenderer leftLine;
        private LineRenderer rightLine;
        private Material lineMaterial;

        public void Configure(Canvas uiCanvas, Camera camera, Transform left, Transform right)
        {
            canvas = uiCanvas;
            eyeCamera = camera;
            leftAnchor = left;
            rightAnchor = right;
        }

        private void Start()
        {
            SetupModule();
            lineMaterial = ResolveLineMaterial();
            leftLine = CreateLine("LeftRay", leftAnchor);
            rightLine = CreateLine("RightRay", rightAnchor);
        }

        private void SetupModule()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                GameObject created = new GameObject("EventSystem");
                eventSystem = created.AddComponent<EventSystem>();
            }

            BaseInputModule existing = eventSystem.GetComponent<BaseInputModule>();
            if (existing != null && !(existing is OVRInputModule))
            {
                Destroy(existing);
            }

            module = eventSystem.GetComponent<OVRInputModule>();
            if (module == null)
            {
                module = eventSystem.gameObject.AddComponent<OVRInputModule>();
            }

            module.joyPadClickButton = OVRInput.Button.PrimaryIndexTrigger;
            module.rayTransform = rightAnchor != null ? rightAnchor : transform;

            if (canvas != null && canvas.worldCamera == null)
            {
                canvas.worldCamera = eyeCamera;
            }
        }

        private void Update()
        {
            if (module == null)
            {
                return;
            }

            Transform chosen = ChooseAnchor();
            bool hasInput = HasInput();
            module.rayTransform = hasInput && chosen != null ? chosen : null;

            UpdateLine(leftLine, leftAnchor, hasInput && chosen != null && chosen == leftAnchor);
            UpdateLine(rightLine, rightAnchor, hasInput && chosen != null && chosen == rightAnchor);
        }

        private static bool HasInput()
        {
            try
            {
                OVRInput.Controller connected = OVRInput.GetConnectedControllers();
                OVRInput.Controller mask = OVRInput.Controller.LTouch | OVRInput.Controller.RTouch |
                    OVRInput.Controller.LHand | OVRInput.Controller.RHand;
                return (connected & mask) != OVRInput.Controller.None;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private Transform ChooseAnchor()
        {
            float leftDistance = CanvasDistance(leftAnchor);
            float rightDistance = CanvasDistance(rightAnchor);
            bool leftValid = !float.IsPositiveInfinity(leftDistance);
            bool rightValid = !float.IsPositiveInfinity(rightDistance);

            if (leftValid && rightValid)
            {
                return leftDistance <= rightDistance ? leftAnchor : rightAnchor;
            }

            if (leftValid)
            {
                return leftAnchor;
            }

            if (rightValid)
            {
                return rightAnchor;
            }

            return rightAnchor != null ? rightAnchor : leftAnchor;
        }

        private float CanvasDistance(Transform anchor)
        {
            if (anchor == null || canvas == null)
            {
                return float.PositiveInfinity;
            }

            RectTransform rect = (RectTransform)canvas.transform;
            Plane plane = new Plane(rect.forward, rect.position);
            Ray ray = new Ray(anchor.position, anchor.forward);

            float enter;
            if (!plane.Raycast(ray, out enter) || enter <= 0f || enter > 8f)
            {
                return float.PositiveInfinity;
            }

            Vector3 local = rect.InverseTransformPoint(ray.GetPoint(enter));
            Vector2 size = rect.rect.size;
            if (Mathf.Abs(local.x) > size.x * 0.5f || Mathf.Abs(local.y) > size.y * 0.5f)
            {
                return float.PositiveInfinity;
            }

            return enter;
        }

        private LineRenderer CreateLine(string name, Transform anchor)
        {
            if (anchor == null)
            {
                return null;
            }

            GameObject lineObject = new GameObject(name);
            lineObject.transform.SetParent(transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.numCapVertices = 4;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.startWidth = 0.006f;
            line.endWidth = 0.003f;
            if (lineMaterial != null)
            {
                line.sharedMaterial = lineMaterial;
            }

            line.enabled = false;
            return line;
        }

        private void UpdateLine(LineRenderer line, Transform anchor, bool active)
        {
            if (line == null || anchor == null)
            {
                return;
            }

            float distance = CanvasDistance(anchor);
            if (!active || float.IsPositiveInfinity(distance))
            {
                line.enabled = false;
                return;
            }

            line.enabled = true;
            Color color = UiTheme.Palette.Accent;
            line.startColor = UiTheme.WithAlpha(color, 0.95f);
            line.endColor = UiTheme.WithAlpha(color, 0.55f);

            RectTransform rect = (RectTransform)canvas.transform;
            Plane plane = new Plane(rect.forward, rect.position);
            Ray ray = new Ray(anchor.position, anchor.forward);
            float enter;
            Vector3 end = anchor.position + anchor.forward * 3f;
            if (plane.Raycast(ray, out enter) && enter > 0f)
            {
                end = ray.GetPoint(enter);
            }

            line.SetPosition(0, anchor.position);
            line.SetPosition(1, end);
        }

        private static Material ResolveLineMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            return shader == null ? null : new Material(shader);
        }
    }
}
