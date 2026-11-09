using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Bound Tooltip/Bound Tooltip Trigger")]
	public class BoundTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		[TextArea]
		public string text;

		public bool useMousePosition;

		public Vector3 offset;

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		public void OnSelect(BaseEventData eventData)
		{
		}

		public void OnPointerExit(PointerEventData eventData)
		{
		}

		public void OnDeselect(BaseEventData eventData)
		{
		}

		private void StartHover(Vector3 position)
		{
		}

		private void StopHover()
		{
		}
	}
}
