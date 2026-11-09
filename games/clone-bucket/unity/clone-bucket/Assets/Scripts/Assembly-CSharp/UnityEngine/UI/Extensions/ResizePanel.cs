using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/RescalePanels/ResizePanel")]
	public class ResizePanel : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler
	{
		public Vector2 minSize;

		public Vector2 maxSize;

		private RectTransform rectTransform;

		private Vector2 currentPointerPosition;

		private Vector2 previousPointerPosition;

		private float ratio;

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
