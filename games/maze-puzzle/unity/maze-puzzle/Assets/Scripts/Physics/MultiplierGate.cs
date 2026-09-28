using System;
using UnityEngine;
using OrbitalSalvage.Core;

namespace OrbitalSalvage.Physics
{
    public enum GateType
    {
        Multiply,
        Add,
        Booster,
        Flapper
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class MultiplierGate : MonoBehaviour
    {
        [Header("Gate Configuration")]
        [SerializeField] private GateType gateType = GateType.Multiply;
        [SerializeField] private int value = 2;
        [SerializeField] private float boosterForce = 6.0f;
        [SerializeField] private float cloneFanningSpread = 0.4f;

        [Header("Visual References")]
        [SerializeField] private SpriteRenderer gateRenderer;
        [SerializeField] private TMPro.TextMeshPro labelText;

        public GateType Type => gateType;
        public int Value => value;

        private void Awake()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            UpdateVisuals();
        }

        public void Setup(GateType type, int val)
        {
            gateType = type;
            value = val;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (labelText != null)
            {
                switch (gateType)
                {
                    case GateType.Multiply:
                        labelText.text = $"x{value}";
                        break;
                    case GateType.Add:
                        labelText.text = $"+{value}";
                        break;
                    case GateType.Booster:
                        labelText.text = "BOOST";
                        break;
                    case GateType.Flapper:
                        labelText.text = "ONE-WAY";
                        break;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var debris = other.GetComponent<DebrisPiece>();
            if (debris == null || debris.IsSwallowed) return;

            // Debounce check: don't process same piece repeatedly
            if (Time.time - debris.LastGatePassTime < 0.25f) return;
            debris.LastGatePassTime = Time.time;

            switch (gateType)
            {
                case GateType.Multiply:
                    ProcessMultiply(debris);
                    break;
                case GateType.Add:
                    ProcessAdd(debris);
                    break;
                case GateType.Booster:
                    ProcessBooster(debris);
                    break;
            }
        }

        private void ProcessMultiply(DebrisPiece original)
        {
            AudioManager.Instance?.PlayMultiplier();
            int clonesToSpawn = value - 1;

            Vector2 basePos = original.transform.position;
            Vector2 baseVel = original.Rb.linearVelocity;

            for (int k = 0; k < clonesToSpawn; k++)
            {
                float offsetAngle = ((k + 1) % 2 == 0 ? 1 : -1) * Mathf.Ceil((k + 1) / 2f) * 0.12f;
                Vector2 spawnPos = basePos + new Vector2(Mathf.Cos(offsetAngle), Mathf.Sin(offsetAngle)) * 0.15f;

                var cloneGo = Instantiate(original.gameObject, spawnPos, Quaternion.identity, original.transform.parent);
                var cloneDebris = cloneGo.GetComponent<DebrisPiece>();
                cloneDebris.SetType(original.Type);
                cloneDebris.LastGatePassTime = Time.time;

                // Fanned impulse
                Vector2 fannedVel = Quaternion.Euler(0, 0, offsetAngle * Mathf.Rad2Deg) * baseVel;
                cloneDebris.Rb.linearVelocity = fannedVel.magnitude > 0.5f ? fannedVel : new Vector2(0, -2f);
            }
        }

        private void ProcessAdd(DebrisPiece original)
        {
            AudioManager.Instance?.PlayAdder();
            Vector2 basePos = original.transform.position;

            for (int k = 0; k < value; k++)
            {
                float offsetAngle = UnityEngine.Random.Range(-cloneFanningSpread, cloneFanningSpread);
                Vector2 spawnPos = basePos + UnityEngine.Random.insideUnitCircle * 0.2f;

                var cloneGo = Instantiate(original.gameObject, spawnPos, Quaternion.identity, original.transform.parent);
                var cloneDebris = cloneGo.GetComponent<DebrisPiece>();
                cloneDebris.SetType(original.Type);
                cloneDebris.LastGatePassTime = Time.time;
                cloneDebris.Rb.linearVelocity = original.Rb.linearVelocity + new Vector2(Mathf.Cos(offsetAngle), Mathf.Sin(offsetAngle)) * 1.5f;
            }
        }

        private void ProcessBooster(DebrisPiece debris)
        {
            AudioManager.Instance?.PlayBooster();
            Vector2 boostDir = transform.up; // along gate orientation
            debris.Rb.linearVelocity = boostDir * Mathf.Max(debris.Rb.linearVelocity.magnitude * 1.5f, boosterForce);
        }
    }
}
