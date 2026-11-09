using System.Collections.Generic;
using UnityEngine;

namespace Lean.Touch
{
    public class LeanFinger
    {
        public int Index;
        public float Age;
        public bool Set;
        public bool LastSet;
        public bool Tap;
        public int TapCount;
        public bool Swipe;
        public bool Old;
        public bool Expired;
        public float LastPressure;
        public float Pressure = 1.0f;
        public Vector2 StartScreenPosition;
        public Vector2 LastScreenPosition;
        public Vector2 ScreenPosition;
        public bool StartedOverGui;
        public List<LeanSnapshot> Snapshots = new List<LeanSnapshot>();

        public Vector2 ScreenDelta { get { return ScreenPosition - LastScreenPosition; } }
        public Vector2 SwipeScreenDelta { get { return ScreenPosition - StartScreenPosition; } }

        public float GetDeltaDegrees(Vector2 referencePoint)
        {
            return GetDeltaDegrees(referencePoint, referencePoint);
        }

        public float GetDeltaDegrees(Vector2 referencePoint, Vector2 lastReferencePoint)
        {
            Vector2 last = LastScreenPosition - lastReferencePoint;
            Vector2 current = ScreenPosition - referencePoint;
            if (last.sqrMagnitude <= 0.0f || current.sqrMagnitude <= 0.0f)
            {
                return 0.0f;
            }
            return Vector2.SignedAngle(last, current);
        }

        public Vector3 GetWorldPosition(float distance, Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            Vector3 screen = new Vector3(ScreenPosition.x, ScreenPosition.y, distance);
            if (camera == null)
            {
                return screen;
            }
            return camera.ScreenToWorldPoint(screen);
        }
    }

    public static class LeanHelper
    {
        public static float GetDampenFactor(float damping, float elapsed)
        {
            if (damping < 0.0f)
            {
                return 1.0f;
            }
            return 1.0f - Mathf.Exp(-damping * Mathf.Max(0.0f, elapsed));
        }
    }

    public class LeanSnapshot
    {
        public float Age;
        public Vector2 ScreenPosition;
    }

    [System.Serializable]
    public class LeanScreenDepth
    {
        public enum ConversionType
        {
            Distance = 0,
            Object = 1,
        }

        public ConversionType Conversion;
        public UnityEngine.Camera Camera;
        public UnityEngine.Object Object;
        public LayerMask Layers;
        public float Distance;

        public Vector3 Convert(Vector2 screenPosition)
        {
            var cam = Camera != null ? Camera : UnityEngine.Camera.main;
            if (cam == null) return new Vector3(screenPosition.x, screenPosition.y, 0.0f);

            if (Conversion == ConversionType.Object)
            {
                var ray = cam.ScreenPointToRay(screenPosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue, Layers))
                {
                    return hit.point;
                }
            }

            return cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Distance));
        }
    }

    [System.Serializable]
    public class LeanFingerFilter
    {
        public enum FilterType
        {
            All = 0,
            SelectableOnly = 1,
        }

        public FilterType Filter;
        public bool IgnoreStartedOverGui = true;
        public int RequiredFingerCount;
        public int RequiredMouseButtons;
        public Lean.Common.LeanSelectable RequiredSelectable;

        public List<LeanFinger> UpdateAndGetFingers(List<LeanFinger> source, List<LeanFinger> results)
        {
            results.Clear();

            for (int i = 0; i < source.Count; i++)
            {
                var finger = source[i];
                if (!finger.Set) continue;
                if (IgnoreStartedOverGui && finger.StartedOverGui) continue;
                if (Filter == FilterType.SelectableOnly && RequiredSelectable != null && !RequiredSelectable.IsSelected) continue;

                results.Add(finger);
            }

            if (RequiredFingerCount > 0 && results.Count < RequiredFingerCount)
            {
                results.Clear();
            }

            return results;
        }
    }
}
