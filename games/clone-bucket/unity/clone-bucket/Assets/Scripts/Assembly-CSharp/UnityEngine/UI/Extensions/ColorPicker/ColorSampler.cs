using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions.ColorPicker
{
	public class ColorSampler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
	{
		private Vector2 m_screenPos;

		[SerializeField]
		protected Button sampler;

		private RectTransform sampleRectTransform;

		[SerializeField]
		protected Outline samplerOutline;

		protected Texture2D screenCapture;

		public ColorChangedEvent oncolorSelected;

		protected Color color;

		protected virtual void OnEnable()
		{
		}

		protected virtual void OnDisable()
		{
		}

		protected virtual void Update()
		{
		}

		protected virtual void HandleSamplerColoring()
		{
		}

		protected virtual void SelectColor()
		{
		}

		public void OnPointerDown(PointerEventData eventData)
		{
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
		}
	}
}
