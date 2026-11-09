using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Lean.Touch
{
    public class LeanMultiUpdate : MonoBehaviour
    {
        public enum CoordinateType
        {
            Screen = 0,
            Viewport = 1,
        }

        [System.Serializable]
        public class LeanFingerListEvent : UnityEvent<List<LeanFinger>> { }

        [System.Serializable]
        public class Vector2Event : UnityEvent<Vector2> { }

        [System.Serializable]
        public class FloatEvent : UnityEvent<float> { }

        [System.Serializable]
        public class Vector3Event : UnityEvent<Vector3> { }

        [System.Serializable]
        public class Vector3Vector3Event : UnityEvent<Vector3, Vector3> { }

        public LeanFingerFilter Use = new LeanFingerFilter();

        [SerializeField] private bool ignoreIfStatic;
        [SerializeField] private LeanFingerListEvent onFingers = new LeanFingerListEvent();
        [SerializeField] private CoordinateType coordinate;
        [SerializeField] private float multiplier = 1.0f;
        [SerializeField] private Vector2Event onDelta = new Vector2Event();
        [SerializeField] private FloatEvent onDistance = new FloatEvent();

        public LeanScreenDepth ScreenDepth = new LeanScreenDepth();

        [SerializeField] private Vector3Event onWorldFrom = new Vector3Event();
        [SerializeField] private Vector3Event onWorldTo = new Vector3Event();
        [SerializeField] private Vector3Event onWorldDelta = new Vector3Event();
        [SerializeField] private Vector3Vector3Event onWorldFromTo = new Vector3Vector3Event();

        private readonly List<LeanFinger> filtered = new List<LeanFinger>();

        public bool IgnoreIfStatic { get { return ignoreIfStatic; } }
        public CoordinateType Coordinate { get { return coordinate; } }
        public float Multiplier { get { return multiplier; } }

        protected virtual void Update()
        {
            Use.UpdateAndGetFingers(LeanTouch.Fingers, filtered);
            if (filtered.Count == 0) return;

            if (onFingers != null) onFingers.Invoke(filtered);

            Vector2 sumDelta = Vector2.zero;
            Vector2 sumCurrent = Vector2.zero;
            Vector2 sumLast = Vector2.zero;
            float sumDistance = 0.0f;

            for (int i = 0; i < filtered.Count; i++)
            {
                var finger = filtered[i];
                sumDelta += finger.ScreenDelta;
                sumCurrent += finger.ScreenPosition;
                sumLast += finger.LastScreenPosition;
                sumDistance += finger.ScreenDelta.magnitude;
            }

            float count = filtered.Count;
            Vector2 averageDelta = sumDelta / count;

            if (ignoreIfStatic && averageDelta.sqrMagnitude <= 0.0f) return;

            Vector2 delta = averageDelta * multiplier;
            float distance = (sumDistance / count) * multiplier;

            if (coordinate == CoordinateType.Viewport)
            {
                float width = Mathf.Max(1.0f, UnityEngine.Screen.width);
                float height = Mathf.Max(1.0f, UnityEngine.Screen.height);
                delta = new Vector2(delta.x / width, delta.y / height);
                distance /= height;
            }

            if (onDelta != null) onDelta.Invoke(delta);
            if (onDistance != null) onDistance.Invoke(distance);

            Vector3 from = ScreenDepth.Convert(sumLast / count);
            Vector3 to = ScreenDepth.Convert(sumCurrent / count);

            if (onWorldFrom != null) onWorldFrom.Invoke(from);
            if (onWorldTo != null) onWorldTo.Invoke(to);
            if (onWorldDelta != null) onWorldDelta.Invoke((to - from) * multiplier);
            if (onWorldFromTo != null) onWorldFromTo.Invoke(from, to);
        }
    }
}
