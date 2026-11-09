using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/UI Highlightable Extension")]
	[RequireComponent(typeof(RectTransform), typeof(Graphic))]
	public class UIHighlightable : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		[Serializable]
		public class InteractableChangedEvent : UnityEvent<bool>
		{
		}

		private Graphic m_Graphic;

		private bool m_Highlighted;

		private bool m_Pressed;

		[SerializeField]
		[Tooltip("Can this panel be interacted with or is it disabled? (does not affect child components)")]
		private bool m_Interactable;

		[SerializeField]
		[Tooltip("Does the panel remain in the pressed state when clicked? (default false)")]
		private bool m_ClickToHold;

		[Tooltip("The default color for the panel")]
		public Color NormalColor;

		[Tooltip("The color for the panel when a mouse is over it or it is in focus")]
		public Color HighlightedColor;

		[Tooltip("The color for the panel when it is clicked/held")]
		public Color PressedColor;

		[Tooltip("The color for the panel when it is not interactable (see Interactable)")]
		public Color DisabledColor;

		[Tooltip("Event for when the panel is enabled / disabled, to enable disabling / enabling of child or other gameobjects")]
		public InteractableChangedEvent OnInteractableChanged;

		public bool Interactable
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool ClickToHold
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		private void Awake()
		{
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		public void OnPointerExit(PointerEventData eventData)
		{
		}

		public void OnPointerDown(PointerEventData eventData)
		{
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		}

		private void HighlightInteractable(Graphic graphic)
		{
		}
	}
}
