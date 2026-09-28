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

        public void BuildSectorMaze(Data.SectorDefinition sector)
        {
            if (outerRingTransform == null) outerRingTransform = transform;

            // Clean previous dynamically built geometry
            for (int i = outerRingTransform.childCount - 1; i >= 0; i--)
            {
                var child = outerRingTransform.GetChild(i);
                if (child.name.StartsWith("Dynamic_"))
                {
                    Destroy(child.gameObject);
                }
            }

            float scale = 0.015f;
            float outerR = sector.ringRadius * scale;
            float apHalf = Mathf.Max(0.35f, sector.apertureAngle * 0.5f);

            // 1. Build Outer Perimeter Arc
            GameObject outerWallGo = new GameObject("Dynamic_OuterWall");
            outerWallGo.transform.SetParent(outerRingTransform, false);

            var lr = outerWallGo.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.startWidth = 0.09f;
            lr.endWidth = 0.09f;
            lr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
            lr.startColor = new Color(0.2f, 0.85f, 1f, 1f);
            lr.endColor = new Color(0.15f, 0.65f, 1f, 1f);

            var edgeCol = outerWallGo.AddComponent<EdgeCollider2D>();

            int segCount = 48;
            Vector3[] linePoints = new Vector3[segCount + 1];
            Vector2[] colPoints = new Vector2[segCount + 1];

            // Arc from aperture start to aperture end (gap centered at bottom -PI/2)
            float startAngle = -Mathf.PI * 0.5f + apHalf;
            float endAngle = -Mathf.PI * 0.5f + (Mathf.PI * 2f - apHalf);

            for (int i = 0; i <= segCount; i++)
            {
                float t = (float)i / segCount;
                float ang = Mathf.Lerp(startAngle, endAngle, t);
                float x = Mathf.Cos(ang) * outerR;
                float y = Mathf.Sin(ang) * outerR;

                linePoints[i] = new Vector3(x, y, 0f);
                colPoints[i] = new Vector2(x, y);
            }

            lr.positionCount = linePoints.Length;
            lr.SetPositions(linePoints);
            edgeCol.points = colPoints;

            // 2. Build Dividers
            if (sector.dividers != null)
            {
                for (int i = 0; i < sector.dividers.Count; i++)
                {
                    var div = sector.dividers[i];
                    float rMin = div.rMin * scale;
                    float rMax = div.rMax * scale;

                    GameObject divGo = new GameObject($"Dynamic_Divider_{i}");
                    divGo.transform.SetParent(outerRingTransform, false);

                    var divLr = divGo.AddComponent<LineRenderer>();
                    divLr.useWorldSpace = false;
                    divLr.startWidth = 0.07f;
                    divLr.endWidth = 0.07f;
                    divLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                    divLr.startColor = new Color(0.5f, 0.95f, 1f, 0.95f);
                    divLr.endColor = new Color(0.4f, 0.85f, 1f, 0.95f);

                    float x0 = Mathf.Cos(div.angle) * rMin;
                    float y0 = Mathf.Sin(div.angle) * rMin;
                    float x1 = Mathf.Cos(div.angle) * rMax;
                    float y1 = Mathf.Sin(div.angle) * rMax;

                    divLr.positionCount = 2;
                    divLr.SetPositions(new Vector3[] { new Vector3(x0, y0, 0f), new Vector3(x1, y1, 0f) });

                    var divCol = divGo.AddComponent<EdgeCollider2D>();
                    divCol.points = new Vector2[] { new Vector2(x0, y0), new Vector2(x1, y1) };
                }
            }

            // 3. Build Multiplier Gates
            if (sector.gates != null)
            {
                for (int i = 0; i < sector.gates.Count; i++)
                {
                    var gate = sector.gates[i];
                    float rMid = (gate.rMin + gate.rMax) * 0.5f * scale;
                    float gateSpan = Mathf.Max(0.5f, (gate.rMax - gate.rMin) * scale);

                    GameObject gateGo = new GameObject($"Dynamic_Gate_{i}");
                    gateGo.transform.SetParent(outerRingTransform, false);

                    float gx = Mathf.Cos(gate.angle) * rMid;
                    float gy = Mathf.Sin(gate.angle) * rMid;
                    gateGo.transform.localPosition = new Vector3(gx, gy, 0f);
                    gateGo.transform.localRotation = Quaternion.Euler(0f, 0f, gate.angle * Mathf.Rad2Deg);

                    var boxCol = gateGo.AddComponent<BoxCollider2D>();
                    boxCol.size = new Vector2(0.4f, gateSpan);
                    boxCol.isTrigger = true;

                    var gateComp = gateGo.AddComponent<MultiplierGate>();
                    GateType gType = GateType.Multiply;
                    if (gate.type.ToUpper().Contains("ADD")) gType = GateType.Add;
                    else if (gate.type.ToUpper().Contains("BOOST")) gType = GateType.Booster;
                    else if (gate.type.ToUpper().Contains("FLAP")) gType = GateType.Flapper;

                    gateComp.Setup(gType, gate.value);
                }
            }
        }
    }
}
