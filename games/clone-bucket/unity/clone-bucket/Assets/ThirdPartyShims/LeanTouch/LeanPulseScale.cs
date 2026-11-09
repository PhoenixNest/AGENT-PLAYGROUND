using UnityEngine;

namespace Lean.Touch
{
    public class LeanPulseScale : MonoBehaviour
    {
        [SerializeField] private Vector3 baseScale = Vector3.one;
        [SerializeField] private float size = 1.0f;
        [SerializeField] private float pulseInterval = 1.0f;
        [SerializeField] private float pulseSize = 1.2f;
        [SerializeField] private float damping = 10.0f;

        private float counter;
        private float pulse = 1.0f;

        public Vector3 BaseScale { get { return baseScale; } set { baseScale = value; } }
        public float Size { get { return size; } }

        public void Pulse()
        {
            pulse = pulseSize;
        }

        protected virtual void Update()
        {
            float dt = Time.deltaTime;

            if (pulseInterval > 0.0f)
            {
                counter += dt;
                if (counter >= pulseInterval)
                {
                    counter -= pulseInterval;
                    Pulse();
                }
            }

            if (damping > 0.0f)
            {
                pulse = Mathf.Lerp(pulse, 1.0f, 1.0f - Mathf.Exp(-damping * dt));
            }
            else
            {
                pulse = 1.0f;
            }

            transform.localScale = baseScale * (size * pulse);
        }
    }
}
