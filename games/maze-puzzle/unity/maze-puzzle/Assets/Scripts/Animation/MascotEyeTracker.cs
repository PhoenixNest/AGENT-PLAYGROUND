using System.Collections;
using UnityEngine;
using OrbitalSalvage.Physics;

namespace OrbitalSalvage.Animation
{
    /// <summary>
    /// Animates Nibbles' facial personality: pupil gaze tracking towards falling scrap,
    /// natural blinking loops, and emotional eye shapes.
    /// </summary>
    public class MascotEyeTracker : MonoBehaviour
    {
        [Header("Pupil Anchors")]
        [SerializeField] private Transform leftPupil;
        [SerializeField] private Transform rightPupil;
        [SerializeField] private Transform leftEyeRoot;
        [SerializeField] private Transform rightEyeRoot;

        [Header("Gaze Parameters")]
        [SerializeField] private float maxGazeOffset = 0.12f;
        [SerializeField] private float gazeSmoothing = 12f;

        private Vector2 currentGaze = Vector2.zero;
        private bool isBlinking = false;

        private void Start()
        {
            StartCoroutine(BlinkRoutine());
        }

        private void Update()
        {
            TrackGaze();
        }

        private void TrackGaze()
        {
            Vector2 targetGaze = Vector2.zero;

            // Find closest active debris piece
            var allDebris = FindObjectsByType<DebrisPiece>(FindObjectsSortMode.None);
            float closestDist = float.MaxValue;
            Vector2 myPos = transform.position;

            foreach (var debris in allDebris)
            {
                if (debris.IsSwallowed) continue;
                float d = Vector2.Distance(myPos, debris.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    Vector2 dir = (Vector2)debris.transform.position - myPos;
                    targetGaze = dir.normalized * Mathf.Clamp01(d / 4f) * maxGazeOffset;
                }
            }

            currentGaze = Vector2.Lerp(currentGaze, targetGaze, Time.deltaTime * gazeSmoothing);

            if (leftPupil != null) leftPupil.localPosition = currentGaze;
            if (rightPupil != null) rightPupil.localPosition = currentGaze;
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(2.5f, 5.0f));

                isBlinking = true;
                float elapsed = 0f;
                const float blinkDuration = 0.12f;

                Vector3 eyeScale = Vector3.one;
                while (elapsed < blinkDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / blinkDuration;
                    float y = Mathf.Abs(Mathf.Sin(t * Mathf.PI));
                    float scaleY = Mathf.Lerp(1.0f, 0.05f, y);

                    eyeScale.y = scaleY;
                    if (leftEyeRoot != null) leftEyeRoot.localScale = eyeScale;
                    if (rightEyeRoot != null) rightEyeRoot.localScale = eyeScale;
                    yield return null;
                }

                if (leftEyeRoot != null) leftEyeRoot.localScale = Vector3.one;
                if (rightEyeRoot != null) rightEyeRoot.localScale = Vector3.one;
                isBlinking = false;
            }
        }
    }
}
