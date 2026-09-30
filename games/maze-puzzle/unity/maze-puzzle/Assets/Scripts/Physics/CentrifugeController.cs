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
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            float scale = 0.015f;
            float outerR = sector.ringRadius * scale;
            float apHalf = Mathf.Max(0.35f, sector.apertureAngle * 0.5f);

            // 1. Build Inner Station Hub
            GameObject innerHubGo = new GameObject("Dynamic_InnerHub");
            innerHubGo.transform.SetParent(outerRingTransform, false);
            innerHubGo.transform.localPosition = Vector3.zero;

            var hubSr = innerHubGo.AddComponent<SpriteRenderer>();
            hubSr.sprite = Utils.SpriteHelper.GetInnerHubSprite(Color.white);
            hubSr.sortingOrder = 5;
            float hubRadius = Mathf.Max(0.75f, sector.innerRadius * scale * 0.75f);
            float hubDiameter = hubRadius * 2f;
            float spriteDiameter = 1.28f;
            float hubScale = hubDiameter / spriteDiameter;
            innerHubGo.transform.localScale = new Vector3(hubScale, hubScale, 1f);

            GameObject textGo = new GameObject("HubText");
            textGo.transform.SetParent(innerHubGo.transform, false);
            var tmp = textGo.AddComponent<TMPro.TextMeshPro>();
            tmp.text = "<b>DONUT</b>\n<size=65%><color=#64748b>SWEET RIG</color></size>";
            tmp.fontSize = 1.25f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color32(0x1e, 0x1b, 0x4b, 0xff);
            tmp.sortingOrder = 6;
            var rt = textGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(3f, 2f);
            rt.anchoredPosition = Vector2.zero;

            // Two flanking 3D donuts (matching Web 1:1: compact 13px emoji flanking DONUT)
            for (int side = -1; side <= 1; side += 2)
            {
                var dGo = new GameObject($"DonutIcon_{side}");
                dGo.transform.SetParent(innerHubGo.transform, false);
                dGo.transform.localPosition = new Vector3(side * 0.34f, 0.045f, 0f);
                var dsr = dGo.AddComponent<SpriteRenderer>();
                dsr.sprite = Utils.SpriteHelper.GetEmojiSprite("emoji_3d_donut");
                dsr.sortingOrder = 6;
                dGo.transform.localScale = Vector3.one * 0.055f;
            }

            // 2. Build Outer Perimeter Arc (Thick Strawberry Frosting with Dark Outline)
            // 2a. Dark outline behind main rim
            GameObject outerShadowGo = new GameObject("Dynamic_OuterWallShadow");
            outerShadowGo.transform.SetParent(outerRingTransform, false);
            var shadowLr = outerShadowGo.AddComponent<LineRenderer>();
            shadowLr.useWorldSpace = false;
            shadowLr.numCapVertices = 6;
            shadowLr.startWidth = 0.22f;
            shadowLr.endWidth = 0.22f;
            shadowLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
            shadowLr.startColor = new Color32(0x1e, 0x1b, 0x4b, 0xff);
            shadowLr.endColor = new Color32(0x1e, 0x1b, 0x4b, 0xff);
            shadowLr.sortingOrder = 1;

            // 2b. Main Strawberry Pink Rim
            GameObject outerWallGo = new GameObject("Dynamic_OuterWall");
            outerWallGo.transform.SetParent(outerRingTransform, false);

            var lr = outerWallGo.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.numCapVertices = 6;
            lr.startWidth = 0.16f;
            lr.endWidth = 0.16f;
            lr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
            // #f472b6 Frosted Donut Pink
            lr.startColor = new Color(0.957f, 0.447f, 0.714f, 1f);
            lr.endColor = new Color(0.957f, 0.447f, 0.714f, 1f);
            lr.sortingOrder = 2;

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

            shadowLr.positionCount = linePoints.Length;
            shadowLr.SetPositions(linePoints);

            lr.positionCount = linePoints.Length;
            lr.SetPositions(linePoints);
            edgeCol.points = colPoints;

            // 2. Build Dividers (Two-tone: dark cartoon outline + bright yellow core)
            if (sector.dividers != null)
            {
                for (int i = 0; i < sector.dividers.Count; i++)
                {
                    var div = sector.dividers[i];
                    // Convert Web Canvas (Y-down) to Unity Cartesian (Y-up)
                    float divAngle = -div.angle;
                    float rMin = div.rMin * scale;
                    float rMax = div.rMax * scale;

                    float x0 = Mathf.Cos(divAngle) * rMin;
                    float y0 = Mathf.Sin(divAngle) * rMin;
                    float x1 = Mathf.Cos(divAngle) * rMax;
                    float y1 = Mathf.Sin(divAngle) * rMax;

                    GameObject divGo = new GameObject($"Dynamic_Divider_{i}");
                    divGo.transform.SetParent(outerRingTransform, false);

                    // Dark outline layer
                    var divShadow = new GameObject("Shadow");
                    divShadow.transform.SetParent(divGo.transform, false);
                    var sLr = divShadow.AddComponent<LineRenderer>();
                    sLr.useWorldSpace = false;
                    sLr.numCapVertices = 6;
                    sLr.startWidth = 0.12f;
                    sLr.endWidth = 0.12f;
                    sLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                    sLr.startColor = Utils.SpriteHelper.ColorBorderDark;
                    sLr.endColor = Utils.SpriteHelper.ColorBorderDark;
                    sLr.sortingOrder = 2;
                    sLr.positionCount = 2;
                    sLr.SetPositions(new Vector3[] { new Vector3(x0, y0, 0f), new Vector3(x1, y1, 0f) });

                    // Yellow core layer
                    var divLr = divGo.AddComponent<LineRenderer>();
                    divLr.useWorldSpace = false;
                    divLr.numCapVertices = 6;
                    divLr.startWidth = 0.075f;
                    divLr.endWidth = 0.075f;
                    divLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                    divLr.startColor = Utils.SpriteHelper.ColorYellowMain;
                    divLr.endColor = Utils.SpriteHelper.ColorYellowMain;
                    divLr.sortingOrder = 3;
                    divLr.positionCount = 2;
                    divLr.SetPositions(new Vector3[] { new Vector3(x0, y0, 0f), new Vector3(x1, y1, 0f) });

                    var divCol = divGo.AddComponent<EdgeCollider2D>();
                    divCol.points = new Vector2[] { new Vector2(x0, y0), new Vector2(x1, y1) };
                }
            }

            // 3. Build Multiplier Gates (1:1 Web: radial guide line + end emitters + tangential badge)
            if (sector.gates != null)
            {
                for (int i = 0; i < sector.gates.Count; i++)
                {
                    var gate = sector.gates[i];
                    // Convert Web Canvas (Y-down) to Unity Cartesian (Y-up)
                    float gAngle = -gate.angle;
                    float rMin = gate.rMin * scale;
                    float rMax = gate.rMax * scale;
                    float rBadge = rMin + (rMax - rMin) * 0.82f;

                    float cosA = Mathf.Cos(gAngle);
                    float sinA = Mathf.Sin(gAngle);

                    float x1 = cosA * rMin;
                    float y1 = sinA * rMin;
                    float x2 = cosA * rMax;
                    float y2 = sinA * rMax;
                    float gx = cosA * rBadge;
                    float gy = sinA * rBadge;

                    Color beamColor = gate.type.ToUpper().Contains("ADD") ? new Color32(0x08, 0x91, 0xb2, 0xff) :
                                      gate.type.ToUpper().Contains("BOOST") ? new Color32(0x84, 0xcc, 0x16, 0xff) :
                                      new Color32(0xf9, 0x73, 0x16, 0xff);

                    // A. Radial spoke guide beam from rMin to rMax
                    GameObject spokeGo = new GameObject($"Dynamic_GateSpoke_{i}");
                    spokeGo.transform.SetParent(outerRingTransform, false);

                    bool isAddGate = gate.type.ToUpper().Contains("ADD");

                    if (isAddGate)
                    {
                        float perpAngle = gAngle + Mathf.PI * 0.5f;
                        float offX = Mathf.Cos(perpAngle) * 0.038f;
                        float offY = Mathf.Sin(perpAngle) * 0.038f;

                        // Dual cyan magnetic guide rails
                        for (int railSide = -1; railSide <= 1; railSide += 2)
                        {
                            var railGo = new GameObject($"Rail_{railSide}");
                            railGo.transform.SetParent(spokeGo.transform, false);
                            var rLr = railGo.AddComponent<LineRenderer>();
                            rLr.useWorldSpace = false;
                            rLr.numCapVertices = 4;
                            rLr.startWidth = 0.045f;
                            rLr.endWidth = 0.045f;
                            rLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                            rLr.startColor = new Color32(0x08, 0x91, 0xb2, 0xff);
                            rLr.endColor = new Color32(0x08, 0x91, 0xb2, 0xff);
                            rLr.sortingOrder = 3;
                            rLr.positionCount = 2;
                            rLr.SetPositions(new Vector3[] {
                                new Vector3(x1 + railSide * offX, y1 + railSide * offY, 0f),
                                new Vector3(x2 + railSide * offX, y2 + railSide * offY, 0f)
                            });
                        }

                        // Inner square magnetic bracket coil at (x1, y1)
                        var coilGo = new GameObject("MagneticCoil_0");
                        coilGo.transform.SetParent(spokeGo.transform, false);
                        coilGo.transform.localPosition = new Vector3(x1, y1, 0f);
                        var coilSr = coilGo.AddComponent<SpriteRenderer>();
                        coilSr.sprite = Utils.SpriteHelper.GetRoundedSquareSprite(new Color32(0x02, 0x84, 0xc7, 0xff), Utils.SpriteHelper.ColorBorderDark);
                        coilSr.sortingOrder = 4;
                        coilGo.transform.localScale = Vector3.one * 0.16f;

                        // Inner cyan glowing center
                        var dotGo = new GameObject("CoilCenter");
                        dotGo.transform.SetParent(coilGo.transform, false);
                        dotGo.transform.localPosition = Vector3.zero;
                        var dsr = dotGo.AddComponent<SpriteRenderer>();
                        dsr.sprite = Utils.SpriteHelper.GetCircleSprite();
                        dsr.color = new Color32(0x67, 0xe8, 0xf9, 0xff);
                        dsr.sortingOrder = 5;
                        dotGo.transform.localScale = Vector3.one * 0.5f;
                    }
                    else
                    {
                        var spokeShadow = new GameObject("Shadow");
                        spokeShadow.transform.SetParent(spokeGo.transform, false);
                        var ssLr = spokeShadow.AddComponent<LineRenderer>();
                        ssLr.useWorldSpace = false;
                        ssLr.numCapVertices = 4;
                        ssLr.startWidth = 0.10f;
                        ssLr.endWidth = 0.10f;
                        ssLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                        ssLr.startColor = Utils.SpriteHelper.ColorBorderDark;
                        ssLr.endColor = Utils.SpriteHelper.ColorBorderDark;
                        ssLr.sortingOrder = 2;
                        ssLr.positionCount = 2;
                        ssLr.SetPositions(new Vector3[] { new Vector3(x1, y1, 0f), new Vector3(x2, y2, 0f) });

                        var sLr = spokeGo.AddComponent<LineRenderer>();
                        sLr.useWorldSpace = false;
                        sLr.numCapVertices = 4;
                        sLr.startWidth = 0.055f;
                        sLr.endWidth = 0.055f;
                        sLr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                        sLr.startColor = beamColor;
                        sLr.endColor = beamColor;
                        sLr.sortingOrder = 3;
                        sLr.positionCount = 2;
                        sLr.SetPositions(new Vector3[] { new Vector3(x1, y1, 0f), new Vector3(x2, y2, 0f) });

                        // Dual spinning/emitter gears at aperture anchors (x1, y1) and (x2, y2)
                        for (int pt = 0; pt < 2; pt++)
                        {
                            Vector3 ep = pt == 0 ? new Vector3(x1, y1, 0f) : new Vector3(x2, y2, 0f);
                            var dotGo = new GameObject($"Emitter_{pt}");
                            dotGo.transform.SetParent(spokeGo.transform, false);
                            dotGo.transform.localPosition = ep;

                            var outDot = new GameObject("Outline");
                            outDot.transform.SetParent(dotGo.transform, false);
                            outDot.transform.localPosition = Vector3.zero;
                            var osr = outDot.AddComponent<SpriteRenderer>();
                            osr.sprite = Utils.SpriteHelper.GetCircleSprite();
                            osr.color = Utils.SpriteHelper.ColorBorderDark;
                            osr.sortingOrder = 4;
                            outDot.transform.localScale = Vector3.one * 0.16f;

                            var isr = dotGo.AddComponent<SpriteRenderer>();
                            isr.sprite = Utils.SpriteHelper.GetCircleSprite();
                            isr.color = beamColor;
                            isr.sortingOrder = 5;
                            dotGo.transform.localScale = Vector3.one * 0.11f;
                        }
                    }

                    // C. Compact Outer-Anchored Badge (oriented tangentially along the ring)
                    GameObject gateGo = new GameObject($"Dynamic_Gate_{i}");
                    gateGo.transform.SetParent(outerRingTransform, false);
                    gateGo.transform.localPosition = new Vector3(gx, gy, 0f);
                    gateGo.transform.localRotation = Quaternion.Euler(0f, 0f, gAngle * Mathf.Rad2Deg - 90f);

                    var boxCol = gateGo.AddComponent<BoxCollider2D>();
                    boxCol.size = new Vector2(0.6f, 0.35f);
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
