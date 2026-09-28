using System;
using UnityEngine;
using OrbitalSalvage.Core;

namespace OrbitalSalvage.Physics
{
    public enum DebrisType
    {
        Titanium,
        Solar,
        Volatile
    }

    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class DebrisPiece : MonoBehaviour
    {
        [Header("Debris Properties")]
        [SerializeField] private DebrisType type = DebrisType.Titanium;
        [SerializeField] private int scrapValue = 1;
        [SerializeField] private float bounceRestitution = 0.45f;

        public DebrisType Type => type;
        public int ScrapValue => scrapValue;
        public Rigidbody2D Rb { get; private set; }
        public CircleCollider2D Col { get; private set; }

        public float LastGatePassTime { get; set; } = -1f;
        public bool IsSwallowed { get; set; } = false;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Col = GetComponent<CircleCollider2D>();
            ConfigurePhysics();
        }

        public void ConfigurePhysics()
        {
            if (Rb == null) Rb = GetComponent<Rigidbody2D>();
            Rb.gravityScale = 1.0f;
            Rb.linearDamping = 0.05f;
            Rb.angularDamping = 0.1f;
            Rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            switch (type)
            {
                case DebrisType.Titanium:
                    Rb.mass = 1.2f;
                    scrapValue = 1;
                    break;
                case DebrisType.Solar:
                    Rb.mass = 0.8f;
                    scrapValue = 2;
                    break;
                case DebrisType.Volatile:
                    Rb.mass = 1.0f;
                    scrapValue = 3;
                    break;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsSwallowed) return;

            // Rate-limited collision sound based on relative velocity
            float impactVel = collision.relativeVelocity.magnitude;
            if (impactVel > 1.2f)
            {
                AudioManager.Instance?.PlayDebrisBounce(impactVel);
            }
        }

        public void SetType(DebrisType newType)
        {
            type = newType;
            ConfigurePhysics();
        }
    }
}
