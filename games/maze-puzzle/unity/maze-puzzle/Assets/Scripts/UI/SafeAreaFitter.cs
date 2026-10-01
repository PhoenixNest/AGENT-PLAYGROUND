using UnityEngine;
using OrbitalSalvage.Parity;

namespace OrbitalSalvage.UI
{
    /// <summary>
    /// Anchors Canvas UI elements strictly within the Apple iPhone Safe Area,
    /// avoiding overlap with the Dynamic Island, camera notch, and Home indicator.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;

        private void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            var screenSize = ParityScreen.Size;
            if (lastSafeArea != SafeAreaProvider.SafeAreaPx || lastScreenSize != screenSize)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;

            lastSafeArea = SafeAreaProvider.SafeAreaPx;
            lastScreenSize = ParityScreen.Size;

            if (lastScreenSize.x <= 0 || lastScreenSize.y <= 0) return;

            Vector2 anchorMin = lastSafeArea.position;
            Vector2 anchorMax = lastSafeArea.position + lastSafeArea.size;

            anchorMin.x /= lastScreenSize.x;
            anchorMin.y /= lastScreenSize.y;
            anchorMax.x /= lastScreenSize.x;
            anchorMax.y /= lastScreenSize.y;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
