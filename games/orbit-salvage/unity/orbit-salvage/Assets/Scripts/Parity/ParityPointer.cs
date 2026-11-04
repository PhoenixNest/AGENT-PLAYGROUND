using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OrbitalSalvage.Parity
{
    public sealed class ParityPointer
    {
        readonly ParityRenderRig rig;
        PointerEventData data;
        GameObject hover;
        GameObject pressed;
        bool isDown;

        public Vector2 PositionPx { get; private set; }
        public bool IsDown => isDown;
        public GameObject LastHit { get; private set; }
        public GameObject LastPressed { get; private set; }

        public ParityPointer(ParityRenderRig rig)
        {
            this.rig = rig;
        }

        public void Reset()
        {
            data = null;
            hover = null;
            pressed = null;
            isDown = false;
            PositionPx = Vector2.zero;
        }

        readonly Vector3[] corners = new Vector3[4];

        public GameObject HitTest(Vector2 screenPx)
        {
            Graphic best = null;
            int bestDepth = int.MinValue;
            int bestOrder = int.MinValue;
            var cam = rig.UiCamera;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int c = 0; c < canvases.Length; c++)
            {
                var canvas = canvases[c];
                if (!canvas.isRootCanvas || !canvas.isActiveAndEnabled) continue;
                var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
                for (int i = 0; i < graphics.Count; i++)
                {
                    var g = graphics[i];
                    if (g == null || !g.raycastTarget || g.depth == -1 || !g.isActiveAndEnabled) continue;
                    if (g.canvasRenderer.cull || BlockedByGroup(g.transform)) continue;
                    if (!ContainsPoint(g.rectTransform, cam, screenPx)) continue;
                    int order = canvas.sortingOrder;
                    if (order > bestOrder || (order == bestOrder && g.depth > bestDepth))
                    {
                        best = g;
                        bestOrder = order;
                        bestDepth = g.depth;
                    }
                }
            }
            return best != null ? best.gameObject : null;
        }

        bool ContainsPoint(RectTransform rt, Camera cam, Vector2 screenPx)
        {
            rt.GetWorldCorners(corners);
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = cam.WorldToScreenPoint(corners[i]);
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }
            return screenPx.x >= minX && screenPx.x <= maxX && screenPx.y >= minY && screenPx.y <= maxY;
        }

        static bool BlockedByGroup(Transform t)
        {
            for (; t != null; t = t.parent)
            {
                var group = t.GetComponent<CanvasGroup>();
                if (group == null) continue;
                if (!group.blocksRaycasts) return true;
                if (group.ignoreParentGroups) return false;
            }
            return false;
        }
        PointerEventData Data()
        {
            if (data == null)
            {
                var es = EventSystem.current;
                if (es == null)
                {
                    var go = new GameObject("ParityEventSystem", typeof(EventSystem));
                    Object.DontDestroyOnLoad(go);
                    es = go.GetComponent<EventSystem>();
                }
                data = new PointerEventData(es);
            }
            return data;
        }

        public GameObject Move(Vector2 screenPx)
        {
            var d = Data();
            PositionPx = screenPx;
            d.position = screenPx;
            var target = HitTest(screenPx);
            d.pointerCurrentRaycast = new RaycastResult { gameObject = target, screenPosition = screenPx };
            d.pointerEnter = target;

            if (target != hover)
            {
                if (hover != null) ExecuteEvents.ExecuteHierarchy(hover, d, ExecuteEvents.pointerExitHandler);
                if (target != null) ExecuteEvents.ExecuteHierarchy(target, d, ExecuteEvents.pointerEnterHandler);
                hover = target;
            }
            return target;
        }

        public void Down()
        {
            var d = Data();
            if (isDown) return;
            var target = HitTest(PositionPx);
            d.pointerCurrentRaycast = new RaycastResult { gameObject = target, screenPosition = PositionPx };
            d.pressPosition = PositionPx;
            d.pointerPressRaycast = d.pointerCurrentRaycast;
            d.eligibleForClick = true;
            d.rawPointerPress = target;
            d.button = PointerEventData.InputButton.Left;
            isDown = true;
            LastHit = target;
            LastPressed = null;
            if (target == null) return;

            pressed = ExecuteEvents.ExecuteHierarchy(target, d, ExecuteEvents.pointerDownHandler);
            if (pressed == null) pressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(target);
            d.pointerPress = pressed;
            LastPressed = pressed;
        }

        public void Up()
        {
            var d = Data();
            if (!isDown) return;
            isDown = false;
            var target = HitTest(PositionPx);
            if (pressed != null) ExecuteEvents.Execute(pressed, d, ExecuteEvents.pointerUpHandler);
            var clickTarget = target != null ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(target) : null;
            if (pressed != null && clickTarget == pressed && d.eligibleForClick)
                ExecuteEvents.Execute(pressed, d, ExecuteEvents.pointerClickHandler);
            pressed = null;
            d.pointerPress = null;
            d.rawPointerPress = null;
            d.eligibleForClick = false;
        }

        public void DisableInputModules()
        {
            foreach (var module in Object.FindObjectsByType<BaseInputModule>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                module.enabled = false;
        }
    }
}
