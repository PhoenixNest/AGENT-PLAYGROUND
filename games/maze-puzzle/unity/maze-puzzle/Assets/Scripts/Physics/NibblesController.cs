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
            // 1. Mascot Body with 3D Hole Emoji
            Transform bodyTr = transform.Find("MascotBody");
            if (bodyTr == null)
            {
                var bodyGo = new GameObject("MascotBody");
                bodyGo.transform.SetParent(transform, false);
                bodyTr = bodyGo.transform;
            }

            var sr = bodyTr.GetComponent<SpriteRenderer>();
            if (sr == null) sr = bodyTr.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = Utils.SpriteHelper.GetEmojiSprite("emoji_3d_hole");
            sr.sortingOrder = 5;
            bodyTr.localScale = Vector3.one * 1.5f;

            // 2. Accretion Disk Swirl Ring
            Transform diskTr = transform.Find("AccretionDisk");
            if (diskTr == null)
            {
                var diskGo = new GameObject("AccretionDisk");
                diskGo.transform.SetParent(transform, false);
                var lr = diskGo.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.loop = true;
                lr.positionCount = 36;
                lr.startWidth = 0.05f;
                lr.endWidth = 0.05f;
                lr.material = Utils.SpriteHelper.GetGlowMaterial(Color.white);
                lr.startColor = new Color(0.7f, 0.2f, 1f, 0.6f);
                lr.endColor = new Color(0.3f, 0.8f, 1f, 0.6f);

                float r = 0.85f;
                for (int i = 0; i < 36; i++)
                {
                    float a = i * Mathf.PI * 2f / 36f;
                    lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
                }
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
