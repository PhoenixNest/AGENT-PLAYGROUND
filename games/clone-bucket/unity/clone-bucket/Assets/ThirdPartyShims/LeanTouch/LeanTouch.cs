using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lean.Touch
{
    [DefaultExecutionOrder(-100)]
    public class LeanTouch : MonoBehaviour
    {
        public const int MOUSE_FINGER_INDEX = -2;
        public const float DEFAULT_TAP_THRESHOLD = 0.2f;
        public const float DEFAULT_SWIPE_THRESHOLD = 100.0f;
        public const int DEFAULT_REFERENCE_DPI = 200;
        public const float DEFAULT_RECORD_THRESHOLD = 5.0f;
        public const float DEFAULT_RECORD_LIMIT = 10.0f;

        public static readonly List<LeanTouch> Instances = new List<LeanTouch>();
        public static readonly List<LeanFinger> Fingers = new List<LeanFinger>();
        public static readonly List<LeanFinger> InactiveFingers = new List<LeanFinger>();

        public static event System.Action<LeanFinger> OnFingerDown;
        public static event System.Action<LeanFinger> OnFingerUpdate;
        public static event System.Action<LeanFinger> OnFingerUp;
        public static event System.Action<LeanFinger> OnFingerTap;
        public static event System.Action<LeanFinger> OnFingerSwipe;
        public static event System.Action<LeanFinger> OnFingerExpired;

        [SerializeField] private float tapThreshold = DEFAULT_TAP_THRESHOLD;
        [SerializeField] private float swipeThreshold = DEFAULT_SWIPE_THRESHOLD;
        [SerializeField] private int referenceDpi = DEFAULT_REFERENCE_DPI;
        [SerializeField] private LayerMask guiLayers;
        [SerializeField] private bool useTouch = true;
        [SerializeField] private bool useHover;
        [SerializeField] private bool useMouse = true;
        [SerializeField] private bool useSimulator;
        [SerializeField] private bool recordFingers;
        [SerializeField] private float recordThreshold = DEFAULT_RECORD_THRESHOLD;
        [SerializeField] private float recordLimit = DEFAULT_RECORD_LIMIT;

        private static readonly List<KeyValuePair<int, Vector2>> pointers = new List<KeyValuePair<int, Vector2>>();

        public float TapThreshold { get { return tapThreshold; } }
        public float SwipeThreshold { get { return swipeThreshold; } }
        public int ReferenceDpi { get { return referenceDpi; } }
        public LayerMask GuiLayers { get { return guiLayers; } }
        public bool UseTouch { get { return useTouch; } }
        public bool UseHover { get { return useHover; } }
        public bool UseMouse { get { return useMouse; } }
        public bool UseSimulator { get { return useSimulator; } }
        public bool RecordFingers { get { return recordFingers; } }
        public float RecordThreshold { get { return recordThreshold; } }
        public float RecordLimit { get { return recordLimit; } }

        protected virtual void OnEnable()
        {
            Instances.Add(this);
        }

        protected virtual void OnDisable()
        {
            Instances.Remove(this);
        }

        protected virtual void Update()
        {
            if (Instances.Count == 0 || Instances[0] != this) return;

            pointers.Clear();
            if (useTouch && Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) continue;
                    pointers.Add(new KeyValuePair<int, Vector2>(touch.fingerId, touch.position));
                }
            }
            else if (useMouse && Input.GetMouseButton(0))
            {
                pointers.Add(new KeyValuePair<int, Vector2>(MOUSE_FINGER_INDEX, Input.mousePosition));
            }

            ProcessFrame(pointers, Time.unscaledDeltaTime);
        }

        internal void ProcessFrame(List<KeyValuePair<int, Vector2>> down, float dt)
        {
            for (int i = 0; i < Fingers.Count; i++)
            {
                var finger = Fingers[i];
                finger.LastSet = finger.Set;
                finger.Set = false;
                finger.Tap = false;
                finger.Swipe = false;
                finger.LastScreenPosition = finger.ScreenPosition;
            }

            for (int i = Fingers.Count - 1; i >= 0; i--)
            {
                var finger = Fingers[i];
                if (!finger.Set && !finger.LastSet)
                {
                    Fingers.RemoveAt(i);
                    finger.Expired = true;
                    InactiveFingers.Add(finger);
                    if (OnFingerExpired != null) OnFingerExpired(finger);
                }
            }

            for (int i = 0; i < down.Count; i++)
            {
                int index = down[i].Key;
                Vector2 position = down[i].Value;
                LeanFinger held = FindHeldFinger(index);
                if (held != null)
                {
                    held.Set = true;
                    held.ScreenPosition = position;
                }
                else
                {
                    BeginFinger(index, position);
                }
            }

            for (int i = Fingers.Count - 1; i >= 0; i--)
            {
                var finger = Fingers[i];
                if (finger.Set && finger.LastSet)
                {
                    UpdateFinger(finger, dt);
                }
                else if (!finger.Set && finger.LastSet)
                {
                    EndFinger(finger, dt);
                }
            }
        }

        private static LeanFinger FindHeldFinger(int index)
        {
            for (int i = 0; i < Fingers.Count; i++)
            {
                var finger = Fingers[i];
                if (finger.Index == index && finger.LastSet) return finger;
            }
            return null;
        }

        private void BeginFinger(int index, Vector2 position)
        {
            LeanFinger finger;
            if (InactiveFingers.Count > 0)
            {
                finger = InactiveFingers[InactiveFingers.Count - 1];
                InactiveFingers.RemoveAt(InactiveFingers.Count - 1);
            }
            else
            {
                finger = new LeanFinger();
            }

            finger.Index = index;
            finger.Age = 0.0f;
            finger.Set = true;
            finger.Expired = false;
            finger.Old = false;
            finger.StartScreenPosition = position;
            finger.LastScreenPosition = position;
            finger.ScreenPosition = position;
            finger.StartedOverGui = IsPointerOverGui(index);
            finger.Snapshots.Clear();

            Fingers.Add(finger);

            if (OnFingerDown != null) OnFingerDown(finger);
        }

        private void UpdateFinger(LeanFinger finger, float dt)
        {
            finger.Age += dt;

            if (recordFingers) RecordSnapshot(finger);

            if (finger.ScreenDelta.sqrMagnitude > 0.0f && OnFingerUpdate != null)
            {
                OnFingerUpdate(finger);
            }
        }

        private void EndFinger(LeanFinger finger, float dt)
        {
            finger.Age += dt;

            float travelled = finger.SwipeScreenDelta.magnitude;
            if (travelled >= swipeThreshold)
            {
                finger.Swipe = true;
                if (OnFingerSwipe != null) OnFingerSwipe(finger);
            }
            else if (finger.Age <= tapThreshold)
            {
                finger.Tap = true;
                finger.TapCount += 1;
                if (OnFingerTap != null) OnFingerTap(finger);
            }

            if (OnFingerUp != null) OnFingerUp(finger);
        }

        private void RecordSnapshot(LeanFinger finger)
        {
            var snapshots = finger.Snapshots;
            if (snapshots.Count == 0 ||
                (finger.ScreenPosition - snapshots[snapshots.Count - 1].ScreenPosition).magnitude >= recordThreshold)
            {
                snapshots.Add(new LeanSnapshot { Age = finger.Age, ScreenPosition = finger.ScreenPosition });
            }

            while (snapshots.Count > 0 && finger.Age - snapshots[0].Age > recordLimit)
            {
                snapshots.RemoveAt(0);
            }
        }

        private static bool IsPointerOverGui(int index)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            return index == MOUSE_FINGER_INDEX
                ? eventSystem.IsPointerOverGameObject()
                : eventSystem.IsPointerOverGameObject(index);
        }
    }
}
