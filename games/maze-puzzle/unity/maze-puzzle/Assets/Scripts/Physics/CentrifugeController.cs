using System;
using UnityEngine;
using OrbitalSalvage.Core;

namespace OrbitalSalvage.Physics
{
    /// <summary>
    /// Controls the rotating centrifuge ring using tactile 1:1 drag kinematics,
    /// angular inertia with 0.94 damping, and double-tap RCS attitude thrusters.
    /// </summary>
    public class CentrifugeController : MonoBehaviour
    {
        [Header("Kinematics Tuning")]
        [SerializeField] private float damping = 0.94f;
        [SerializeField] private float maxAngularVelocity = 25f; // rad/s
        [SerializeField] private float swipeSensitivity = 1.0f;
        [SerializeField] private float thrusterBurstImpulse = 4.0f; // rad/s
        [SerializeField] private float ratchetStepDegrees = 15f;

        [Header("Ring Hierarchy")]
        [SerializeField] private Transform outerRingTransform;
        [SerializeField] private Transform innerRingTransform;

        public float CurrentAngle { get; private set; } = 0f;
        public float AngularVelocity { get; private set; } = 0f;
        public bool IsDragging { get; private set; } = false;

        private float lastDragAngle = 0f;
        private float lastRatchetAngle = 0f;
        private float lastTapTime = 0f;
        private const float DOUBLE_TAP_THRESHOLD = 0.28f;

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (outerRingTransform == null) outerRingTransform = transform;
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            if (!IsDragging)
            {
                // Apply rotational inertia damping
                AngularVelocity *= Mathf.Pow(damping, Time.fixedDeltaTime * 60f);
                if (Mathf.Abs(AngularVelocity) < 0.005f) AngularVelocity = 0f;
            }

            // Euler integration
            CurrentAngle += AngularVelocity * Time.fixedDeltaTime;

            // Apply rotation to outer ring
            if (outerRingTransform != null)
            {
                outerRingTransform.rotation = Quaternion.Euler(0f, 0f, CurrentAngle * Mathf.Rad2Deg);
            }

            // Check ratchet click
            float deg = CurrentAngle * Mathf.Rad2Deg;
            if (Mathf.Abs(deg - lastRatchetAngle) >= ratchetStepDegrees)
            {
                lastRatchetAngle = deg;
                float pitchMult = Mathf.Clamp(Mathf.Abs(AngularVelocity) / 5f, 0.7f, 1.4f);
                AudioManager.Instance?.PlayRatchet(pitchMult);
            }
        }

        private void HandleInput()
        {
            Vector2 pointerPos = Vector2.zero;
            bool pointerDown = false;
            bool pointerUp = false;
            bool pointerHeld = false;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0)) { pointerDown = true; pointerPos = Input.mousePosition; }
            if (Input.GetMouseButtonUp(0)) { pointerUp = true; pointerPos = Input.mousePosition; }
            if (Input.GetMouseButton(0)) { pointerHeld = true; pointerPos = Input.mousePosition; }
#else
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                pointerPos = t.position;
                if (t.phase == TouchPhase.Began) pointerDown = true;
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) pointerUp = true;
                else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) pointerHeld = true;
            }
#endif

            if (pointerDown)
            {
                // Double tap detection for RCS attitude thruster
                float timeSinceLastTap = Time.time - lastTapTime;
                if (timeSinceLastTap < DOUBLE_TAP_THRESHOLD)
                {
                    // Trigger attitude thruster burst in direction of tap side
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(pointerPos);
                    float dir = worldPos.x >= 0 ? -1f : 1f;
                    TriggerThruster(dir);
                }
                lastTapTime = Time.time;

                IsDragging = true;
                lastDragAngle = GetPointerAngle(pointerPos);
                AngularVelocity = 0f;
            }
            else if (pointerHeld && IsDragging)
            {
                float currentPointerAngle = GetPointerAngle(pointerPos);
                float deltaAngle = Mathf.DeltaAngle(lastDragAngle * Mathf.Rad2Deg, currentPointerAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                
                AngularVelocity = (deltaAngle / Time.deltaTime) * swipeSensitivity;
                AngularVelocity = Mathf.Clamp(AngularVelocity, -maxAngularVelocity, maxAngularVelocity);

                lastDragAngle = currentPointerAngle;
            }
            else if (pointerUp)
            {
                IsDragging = false;
            }
        }

        private float GetPointerAngle(Vector2 screenPos)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            Vector2 dir = (Vector2)(worldPos - transform.position);
            return Mathf.Atan2(dir.y, dir.x);
        }

        public void TriggerThruster(float direction)
        {
            AngularVelocity += direction * thrusterBurstImpulse;
            AngularVelocity = Mathf.Clamp(AngularVelocity, -maxAngularVelocity, maxAngularVelocity);
            AudioManager.Instance?.PlayThruster();
        }

        public void SetAngle(float angleRad)
        {
            CurrentAngle = angleRad;
            AngularVelocity = 0f;
            if (outerRingTransform != null)
            {
                outerRingTransform.rotation = Quaternion.Euler(0f, 0f, CurrentAngle * Mathf.Rad2Deg);
            }
        }
    }
}
