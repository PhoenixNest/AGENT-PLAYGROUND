using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/RescalePanels/RescalePanel")]
	public class RescalePanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler
	{
		public Vector2 minSize;

		public Vector2 maxSize;

		private RectTransform rectTransform;

		private Transform goTransform;

		private Vector2 currentPointerPosition;

		private Vector2 previousPointerPosition;

		private RectTransform thisRectTransform;

		private Vector2 sizeDelta;

		private void Awake()
		{
		}

		public void OnPointerDown(PointerEventData data)
		{
		}

		public void OnDrag(PointerEventData data)
		{
		}
	}
}
