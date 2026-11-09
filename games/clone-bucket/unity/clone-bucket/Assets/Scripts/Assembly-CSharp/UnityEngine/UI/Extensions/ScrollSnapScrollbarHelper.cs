using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[DisallowMultipleComponent]
	public class ScrollSnapScrollbarHelper : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		internal IScrollSnap ss;

		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
		}

		public void OnPointerDown(PointerEventData eventData)
		{
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		}

		private void OnScrollBarDown()
		{
		}

		private void OnScrollBarUp()
		{
		}
	}
}
