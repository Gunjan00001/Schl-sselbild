using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Schlusselbild.Ui
{
    public sealed class MouseUiInput : MonoBehaviour
    {
        private GraphicRaycaster raycaster;
        private PointerEventData eventData;
        private readonly List<RaycastResult> hits = new List<RaycastResult>(16);
        private GameObject hovered;
        private GameObject pressed;
        private Vector2 lastPosition;
        private bool blocked;

        public void Configure(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            GraphicRaycaster[] existing = canvas.GetComponents<GraphicRaycaster>();
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i].GetType() == typeof(GraphicRaycaster))
                {
                    raycaster = existing[i];
                    break;
                }
            }

            if (raycaster == null)
            {
                raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void OnDestroy()
        {
            ClearHover();
        }

        private void Update()
        {
            if (blocked || raycaster == null || EventSystem.current == null)
            {
                return;
            }

            Vector2 position;
            bool pressedNow;
            bool releasedNow;

            try
            {
                position = Input.mousePosition;
                pressedNow = Input.GetMouseButtonDown(0);
                releasedNow = Input.GetMouseButtonUp(0);
            }
            catch (System.Exception)
            {
                blocked = true;
                return;
            }

            if (eventData == null)
            {
                eventData = new PointerEventData(EventSystem.current);
            }

            eventData.position = position;
            eventData.delta = position - lastPosition;
            lastPosition = position;

            hits.Clear();
            raycaster.Raycast(eventData, hits);
            GameObject target = hits.Count > 0 ? hits[0].gameObject : null;

            UpdateHover(target);

            if (pressedNow)
            {
                BeginPress();
            }

            if (releasedNow)
            {
                EndPress();
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
        }

        private void BeginPress()
        {
            pressed = hovered;
            if (pressed == null)
            {
                return;
            }

            eventData.pressPosition = eventData.position;
            eventData.pointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pressed);
            eventData.rawPointerPress = pressed;

            ExecuteEvents.ExecuteHierarchy(pressed, eventData, ExecuteEvents.pointerDownHandler);
        }

        private void EndPress()
        {
            if (pressed != null)
            {
                ExecuteEvents.ExecuteHierarchy(pressed, eventData, ExecuteEvents.pointerUpHandler);

                if (pressed == hovered)
                {
                    ExecuteEvents.ExecuteHierarchy(pressed, eventData, ExecuteEvents.pointerClickHandler);
                }
            }

            pressed = null;
            eventData.pointerPress = null;
            eventData.rawPointerPress = null;
        }
    }
}
