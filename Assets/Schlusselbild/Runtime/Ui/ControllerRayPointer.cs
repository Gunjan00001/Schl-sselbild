using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Schlusselbild.Ui
{
    public sealed class ControllerRayPointer : MonoBehaviour
    {
        public enum Hand
        {
            Left,
            Right
        }

        private static readonly List<GraphicRaycaster> Raycasters = new List<GraphicRaycaster>();
        private static readonly List<RaycastResult> Hits = new List<RaycastResult>(16);

        [SerializeField] private Hand hand = Hand.Right;
        [SerializeField] private Transform anchor;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private float lineWidth = 0.008f;
        [SerializeField] private float maxDistance = 6f;
        [SerializeField] private bool mouseFallback = true;

        private LineRenderer line;
        private PointerEventData eventData;
        private GameObject hovered;
        private GameObject pressed;
        private GameObject dragTarget;
        private bool triggerHeld;
        private bool clickHandled;
        private bool mouseBlocked;
        private Camera mouseCamera;
        private RaycastResult currentRaycast;
        private Vector3 pointerOrigin;
        private Vector3 pointerEnd;
        private bool pointerVisible;

        public Hand PointerHand
        {
            get { return hand; }
        }

        public Transform Anchor
        {
            get { return anchor; }
        }

        public float LineWidth
        {
            get { return lineWidth; }
            set
            {
                lineWidth = value;
                ApplyLineWidth();
            }
        }

        public float MaxDistance
        {
            get { return maxDistance; }
            set { maxDistance = Mathf.Max(0.1f, value); }
        }

        public Material LineMaterial
        {
            get { return lineMaterial; }
            set
            {
                lineMaterial = value;
                if (line != null && lineMaterial != null)
                {
                    line.sharedMaterial = lineMaterial;
                }
            }
        }

        public bool MouseFallback
        {
            get { return mouseFallback; }
            set { mouseFallback = value; }
        }

        public GameObject Hovered
        {
            get { return hovered; }
        }

        public bool IsPointerVisible
        {
            get { return line != null && line.enabled; }
        }

        public static void Register(GraphicRaycaster raycaster)
        {
            if (raycaster == null || Raycasters.Contains(raycaster))
            {
                return;
            }

            Raycasters.Add(raycaster);
        }

        public static void Unregister(GraphicRaycaster raycaster)
        {
            if (raycaster != null)
            {
                Raycasters.Remove(raycaster);
            }
        }

        public static void ClearRegistrations()
        {
            Raycasters.Clear();
        }

        public void Configure(Hand pointerHand, Transform pointerAnchor)
        {
            hand = pointerHand;
            anchor = pointerAnchor;
        }

        private void Awake()
        {
            EnsureLine();
        }

        private void OnDestroy()
        {
            ClearHover();
        }

        private void Update()
        {
            if (eventData == null)
            {
                if (EventSystem.current == null)
                {
                    return;
                }

                eventData = new PointerEventData(EventSystem.current);
            }

            GameObject target = ResolveTarget();

            if (!pointerVisible)
            {
                ClearHover();
                HandleTrigger(false);
                UpdateLine();
                return;
            }

            UpdateHover(target);
            HandleTrigger(ReadPressed());
            UpdateLine();
        }

        private GameObject ResolveTarget()
        {
            pointerVisible = false;
            currentRaycast = new RaycastResult();

            Vector2 screenPosition;
            bool hasScreenPosition;
            Ray ray;

            if (TryGetControllerRay(out ray))
            {
                hasScreenPosition = false;
                screenPosition = Vector2.zero;
            }
            else if (TryGetMouseRay(out ray, out screenPosition))
            {
                hasScreenPosition = true;
            }
            else
            {
                return null;
            }

            pointerVisible = true;
            pointerOrigin = ray.origin;
            pointerEnd = ray.origin + ray.direction * maxDistance;

            GameObject best = null;
            int bestSortingLayer = int.MinValue;
            int bestSortingOrder = int.MinValue;
            int bestDepth = int.MinValue;
            float bestDistance = float.MaxValue;
            Vector3 bestPoint = pointerEnd;

            for (int i = 0; i < Raycasters.Count; i++)
            {
                GraphicRaycaster raycaster = Raycasters[i];
                if (raycaster == null)
                {
                    continue;
                }

                Camera eventCamera = raycaster.eventCamera;
                if (eventCamera == null)
                {
                    continue;
                }

                RectTransform canvasRect = raycaster.transform as RectTransform;
                if (canvasRect == null)
                {
                    continue;
                }

                eventData.position = screenPosition;
                Vector3 planePoint = pointerEnd;

                if (!hasScreenPosition)
                {
                    Plane plane = new Plane(canvasRect.forward, canvasRect.position);
                    float enter;
                    if (!plane.Raycast(ray, out enter) || enter <= 0f || enter > maxDistance)
                    {
                        continue;
                    }

                    planePoint = ray.GetPoint(enter);
                    eventData.position = eventCamera.WorldToScreenPoint(planePoint);
                }

                Hits.Clear();
                raycaster.Raycast(eventData, Hits);

                for (int h = 0; h < Hits.Count; h++)
                {
                    RaycastResult hit = Hits[h];
                    if (hit.gameObject == null)
                    {
                        continue;
                    }

                    bool better = hit.sortingLayer > bestSortingLayer;
                    if (!better && hit.sortingLayer == bestSortingLayer)
                    {
                        better = hit.sortingOrder > bestSortingOrder;
                        if (!better && hit.sortingOrder == bestSortingOrder)
                        {
                            better = hit.depth > bestDepth;
                            if (!better && hit.depth == bestDepth)
                            {
                                better = hit.distance < bestDistance;
                            }
                        }
                    }

                    if (better)
                    {
                        bestSortingLayer = hit.sortingLayer;
                        bestSortingOrder = hit.sortingOrder;
                        bestDepth = hit.depth;
                        bestDistance = hit.distance;
                        best = hit.gameObject;
                        bestPoint = planePoint;
                        currentRaycast = hit;
                    }
                }
            }

            eventData.pointerCurrentRaycast = currentRaycast;
            pointerEnd = best == null ? pointerEnd : bestPoint;
            return best;
        }

        private bool TryGetControllerRay(out Ray ray)
        {
            ray = new Ray();

            if (anchor == null || !HasController())
            {
                return false;
            }

            ray = new Ray(anchor.position, anchor.forward);
            return true;
        }

        private bool TryGetMouseRay(out Ray ray, out Vector2 screenPosition)
        {
            ray = new Ray();
            screenPosition = Vector2.zero;

            if (!mouseFallback || mouseBlocked || !Application.isEditor)
            {
                return false;
            }

            try
            {
                if (mouseCamera == null)
                {
                    mouseCamera = Camera.main;
                }

                if (mouseCamera == null)
                {
                    return false;
                }

                Vector3 position = Input.mousePosition;
                ray = mouseCamera.ScreenPointToRay(position);
                screenPosition = position;
                return true;
            }
            catch (Exception)
            {
                mouseBlocked = true;
                return false;
            }
        }

        private bool ReadPressed()
        {
            if (TryReadTrigger(out bool trigger))
            {
                return trigger;
            }

            if (TryReadMouseButton(out bool mouse))
            {
                return mouse;
            }

            return false;
        }

        private bool TryReadTrigger(out bool pressedNow)
        {
            pressedNow = false;

            if (anchor == null)
            {
                return false;
            }

            try
            {
                OVRInput.Controller mask = hand == Hand.Left
                    ? OVRInput.Controller.LTouch | OVRInput.Controller.LHand
                    : OVRInput.Controller.RTouch | OVRInput.Controller.RHand;

                if ((OVRInput.GetConnectedControllers() & mask) == OVRInput.Controller.None)
                {
                    return false;
                }

                pressedNow = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, mask);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryReadMouseButton(out bool pressedNow)
        {
            pressedNow = false;

            if (!mouseFallback || mouseBlocked || !Application.isEditor)
            {
                return false;
            }

            try
            {
                pressedNow = Input.GetMouseButton(0);
                return true;
            }
            catch (Exception)
            {
                mouseBlocked = true;
                return false;
            }
        }

        private static bool HasController()
        {
            try
            {
                OVRInput.Controller connected = OVRInput.GetConnectedControllers();
                OVRInput.Controller mask = OVRInput.Controller.LTouch | OVRInput.Controller.RTouch |
                    OVRInput.Controller.LHand | OVRInput.Controller.RHand;
                return (connected & mask) != OVRInput.Controller.None;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void UpdateHover(GameObject target)
        {
            if (hovered == target)
            {
                return;
            }

            if (hovered != null)
            {
                ExecuteEvents.ExecuteHierarchy(hovered, eventData, ExecuteEvents.pointerExitHandler);
            }

            hovered = target;
            eventData.pointerEnter = hovered;

            if (hovered != null)
            {
                ExecuteEvents.ExecuteHierarchy(hovered, eventData, ExecuteEvents.pointerEnterHandler);
            }
        }

        private void ClearHover()
        {
            if (hovered != null && eventData != null)
            {
                ExecuteEvents.ExecuteHierarchy(hovered, eventData, ExecuteEvents.pointerExitHandler);
            }

            hovered = null;

            if (eventData != null)
            {
                eventData.pointerEnter = null;
            }
        }

        private void HandleTrigger(bool pressedNow)
        {
            if (pressedNow && !triggerHeld)
            {
                BeginPress();
                return;
            }

            if (!pressedNow && triggerHeld)
            {
                EndPress();
                return;
            }

            if (pressedNow && dragTarget != null)
            {
                ExecuteEvents.ExecuteHierarchy(dragTarget, eventData, ExecuteEvents.dragHandler);
            }
        }

        private void BeginPress()
        {
            triggerHeld = true;
            clickHandled = false;
            pressed = hovered;

            if (pressed == null)
            {
                return;
            }

            eventData.pressPosition = eventData.position;
            eventData.pointerPressRaycast = currentRaycast;
            eventData.pointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pressed);
            eventData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(pressed);
            eventData.rawPointerPress = pressed;

            ExecuteEvents.ExecuteHierarchy(pressed, eventData, ExecuteEvents.pointerDownHandler);

            if (eventData.pointerDrag != null)
            {
                dragTarget = eventData.pointerDrag;
                eventData.dragging = true;
                ExecuteEvents.ExecuteHierarchy(dragTarget, eventData, ExecuteEvents.initializePotentialDrag);
                ExecuteEvents.ExecuteHierarchy(dragTarget, eventData, ExecuteEvents.beginDragHandler);
            }

            GameObject buttonObject = ExecuteEvents.GetEventHandler<Button>(pressed);
            if (buttonObject != null)
            {
                Button button = buttonObject.GetComponent<Button>();
                if (button != null && button.IsInteractable())
                {
                    button.onClick.Invoke();
                    clickHandled = true;
                }
            }
        }

        private void EndPress()
        {
            triggerHeld = false;

            if (dragTarget != null)
            {
                ExecuteEvents.ExecuteHierarchy(dragTarget, eventData, ExecuteEvents.endDragHandler);
                dragTarget = null;
                eventData.dragging = false;
            }

            if (pressed != null)
            {
                ExecuteEvents.ExecuteHierarchy(pressed, eventData, ExecuteEvents.pointerUpHandler);

                if (!clickHandled && pressed == hovered)
                {
                    ExecuteEvents.ExecuteHierarchy(pressed, eventData, ExecuteEvents.pointerClickHandler);
                }
            }

            pressed = null;
            eventData.pointerPress = null;
            eventData.rawPointerPress = null;
            clickHandled = false;
        }

        private void EnsureLine()
        {
            if (line != null)
            {
                return;
            }

            GameObject lineObject = new GameObject("PointerLine");
            lineObject.transform.SetParent(transform, false);
            line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.numCapVertices = 2;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            Material material = ResolveLineMaterial();
            if (material != null)
            {
                line.sharedMaterial = material;
            }

            ApplyLineWidth();

            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position);
            line.enabled = false;
        }

        private Material ResolveLineMaterial()
        {
            if (lineMaterial != null)
            {
                return lineMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            return shader == null ? null : new Material(shader);
        }

        private void ApplyLineWidth()
        {
            if (line == null)
            {
                return;
            }

            line.startWidth = lineWidth;
            line.endWidth = lineWidth * 0.5f;
        }

        private void ApplyLineColor()
        {
            if (line == null)
            {
                return;
            }

            UiPalette palette = UiTheme.Palette;
            line.startColor = UiTheme.WithAlpha(palette.Accent, 0.9f);
            line.endColor = UiTheme.WithAlpha(palette.Accent, 0.25f);
        }

        private void UpdateLine()
        {
            if (line == null)
            {
                return;
            }

            if (!pointerVisible)
            {
                line.enabled = false;
                return;
            }

            line.enabled = true;
            ApplyLineColor();
            line.SetPosition(0, pointerOrigin);
            line.SetPosition(1, pointerEnd);
        }
    }
}
