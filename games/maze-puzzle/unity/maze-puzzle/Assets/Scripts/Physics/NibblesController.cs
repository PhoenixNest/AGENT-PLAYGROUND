using System;
using UnityEngine;
using OrbitalSalvage.Core;

namespace OrbitalSalvage.Physics
{
    /// <summary>
    /// Nibbles the hungry micro-black hole mascot.
    /// Exerts inward gravitational pull, swallows mass, grows dynamically with quota,
    /// and exhibits bouncy squash-and-stretch cartoon elasticity.
    /// </summary>
    public class NibblesController : MonoBehaviour
    {
        [Header("Gravitational Field")]
        [SerializeField] private float attractionRadius = 3.5f;
        [SerializeField] private float gravityStrength = 18f;
        [SerializeField] private float eventHorizonRadius = 0.5f;

        [Header("Quota Scale Dynamics")]
        [SerializeField] private float minScale = 1.0f;
        [SerializeField] private float maxScale = 1.65f;
        [SerializeField] private Transform mascotVisualRoot;

        [Header("Wobble Elasticity")]
        [SerializeField] private float wobbleDamping = 0.84f;

        private float currentQuotaProgress = 0f;
        private float wobbleVelocity = 0f;
        private float wobbleOffset = 0f;
        private Vector3 baseScale;

        public static NibblesController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            if (mascotVisualRoot == null) mascotVisualRoot = transform;
            baseScale = mascotVisualRoot.localScale;

            var col = GetComponent<CircleCollider2D>();
            if (col != null) col.radius = eventHorizonRadius;
        }

        private void Start()
        {
            ConfigureVisuals();
        }

        public void ConfigureVisuals()
        {
            // 1. Mascot Body with Canonical Cute Anime Black Hole Face
            Transform bodyTr = transform.Find("MascotBody");
            if (bodyTr == null)
            {
                var bodyGo = new GameObject("MascotBody");
                bodyGo.transform.SetParent(transform, false);
                bodyTr = bodyGo.transform;
            }

            var sr = bodyTr.GetComponent<SpriteRenderer>();
            if (sr == null) sr = bodyTr.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = Utils.SpriteHelper.GetNibblesMascotSprite();
            sr.color = Color.white;
            sr.sortingOrder = 6;
            bodyTr.localScale = Vector3.one * 0.62f;

            var oldEyes = transform.Find("EyesRoot");
            if (oldEyes != null)
            {
                if (Application.isPlaying) Destroy(oldEyes.gameObject);
                else DestroyImmediate(oldEyes.gameObject);
            }

            // 2. Accretion Vortex Disk & Ribbons (1:1 Web: multi-layered dashed tracks & stardust tightly around Nibbles)
            Transform vortexTr = transform.Find("AccretionVortex");
            if (vortexTr == null)
            {
                var vGo = new GameObject("AccretionVortex");
                vGo.transform.SetParent(transform, false);
                vortexTr = vGo.transform;
            }
            var vsr = vortexTr.GetComponent<SpriteRenderer>();
            if (vsr == null) vsr = vortexTr.gameObject.AddComponent<SpriteRenderer>();
            vsr.sprite = Utils.SpriteHelper.GetAccretionVortexSprite();
            vsr.sortingOrder = 4;
            vortexTr.localScale = Vector3.one * 0.72f;

            // Clean up obsolete legacy objects
            var oldDisk = transform.Find("AccretionDisk");
            if (oldDisk != null) Destroy(oldDisk.gameObject);
            var oldInner = transform.Find("InnerRing");
            if (oldInner != null) Destroy(oldInner.gameObject);
            var oldGlow = transform.Find("AccretionGlow");
            if (oldGlow != null) Destroy(oldGlow.gameObject);
            var oldFunnel = transform.Find("SuctionFunnel");
            if (oldFunnel != null)
            {
                if (Application.isPlaying) Destroy(oldFunnel.gameObject);
                else DestroyImmediate(oldFunnel.gameObject);
            }
        }

        private void FixedUpdate()
        {
            ApplyGravitationalTractorPull();
            UpdateWobblePhysics();
        }

        private void ApplyGravitationalTractorPull()
        {
            Collider2D[] debrisInRange = UnityEngine.Physics2D.OverlapCircleAll(transform.position, attractionRadius);
            Vector2 holePos = transform.position;

            foreach (var col in debrisInRange)
            {
                var debris = col.GetComponent<DebrisPiece>();
                if (debris == null || debris.IsSwallowed) continue;

                Vector2 toHole = holePos - (Vector2)debris.transform.position;
                float dist = toHole.magnitude;

                if (dist > 0.05f)
                {
                    // Inverse square inward gravitational tractor pull
                    float forceMag = gravityStrength / Mathf.Max(dist * dist, 0.4f);
                    debris.Rb.AddForce(toHole.normalized * forceMag, ForceMode2D.Force);
                }
            }
        }

        private void UpdateWobblePhysics()
        {
            // Elastic spring calculation for cartoon wobble
            float springForce = -120f * wobbleOffset;
            wobbleVelocity += springForce * Time.fixedDeltaTime;
            wobbleVelocity *= Mathf.Pow(wobbleDamping, Time.fixedDeltaTime * 60f);
            wobbleOffset += wobbleVelocity * Time.fixedDeltaTime;

            float targetQuotaScale = Mathf.Lerp(minScale, maxScale, currentQuotaProgress);
            float finalScale = targetQuotaScale + wobbleOffset;

            mascotVisualRoot.localScale = baseScale * finalScale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var debris = other.GetComponent<DebrisPiece>();
            if (debris == null || debris.IsSwallowed) return;

            SwallowDebris(debris);
        }

        public void SwallowDebris(DebrisPiece debris)
        {
            debris.IsSwallowed = true;

            // Trigger squash & stretch punch
            wobbleOffset += 0.09f;
            wobbleVelocity += 1.8f;

            // Audio & VFX
            AudioManager.Instance?.PlaySwallow();

            // Notify Game Manager
            GameManager.Instance?.OnDebrisSwallowed(debris);

            // Destroy game object
            Destroy(debris.gameObject);
        }

        public void UpdateQuotaProgress(float progress01)
        {
            currentQuotaProgress = Mathf.Clamp01(progress01);
            if (currentQuotaProgress >= 1f && wobbleOffset < 0.15f)
            {
                // Full quota celebration bounce
                wobbleOffset = 0.25f;
                AudioManager.Instance?.PlayQuotaGrowth();
            }
        }
    }
}
