using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using OrbitalSalvage.Core;

namespace OrbitalSalvage.UI
{
    /// <summary>
    /// Tactile 3D button press animation replicating the Fall Guys / Brawl Stars bouncy feel
    /// from the Orbital Salvage web prototype.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class UIBounceButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float pressYOffset = -4f;

        private Vector3 originalScale = Vector3.one;
        private Vector3 originalPos = Vector3.zero;
        private RectTransform rt;
        private bool initialized = false;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (initialized) return;
            rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                originalScale = rt.localScale;
                originalPos = rt.localPosition;
                initialized = true;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Init();
            if (rt != null)
            {
                rt.localScale = originalScale * pressScale;
                rt.localPosition = originalPos + new Vector3(0, pressYOffset, 0);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Init();
            if (rt != null)
            {
                rt.localScale = originalScale;
                rt.localPosition = originalPos;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            AudioManager.Instance?.PlayButtonClick();
        }
    }
}
