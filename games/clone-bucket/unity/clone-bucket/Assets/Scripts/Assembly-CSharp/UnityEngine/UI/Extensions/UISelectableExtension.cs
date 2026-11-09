using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/UI Selectable Extension")]
	[RequireComponent(typeof(Selectable))]
	public class UISelectableExtension : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
	{
		[Serializable]
		public class UIButtonEvent : UnityEvent<PointerEventData.InputButton>
		{
		}

		[Tooltip("Event that fires when a button is initially pressed down")]
		public UIButtonEvent OnButtonPress;

		[Tooltip("Event that fires when a button is released")]
		public UIButtonEvent OnButtonRelease;

		[Tooltip("Event that continually fires while a button is held down")]
		public UIButtonEvent OnButtonHeld;

		private bool _pressed;

		private PointerEventData _heldEventData;

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
		}

		private void Update()
		{
		}

		public void TestClicked()
		{
		}

		public void TestPressed()
		{
		}

		public void TestReleased()
		{
		}

		public void TestHold()
		{
		}

		private void OnDisable()
		{
		}
	}
}
