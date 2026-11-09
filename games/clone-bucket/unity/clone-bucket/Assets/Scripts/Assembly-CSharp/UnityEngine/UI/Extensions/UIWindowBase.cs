using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/UI Window Base")]
	[RequireComponent(typeof(RectTransform))]
	public class UIWindowBase : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
	{
		private bool _isDragging;

		public static bool ResetCoords;

		private Vector3 m_originalCoods;

		private Canvas m_canvas;

		private RectTransform m_canvasRectTransform;

		[Tooltip("Number of pixels of the window that must stay inside the canvas view.")]
		public int KeepWindowInCanvas;

		[Tooltip("The transform that is moved when dragging, can be left empty in which case its own transform is used.")]
		public RectTransform RootTransform;

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
		}

		private void resetCoordinatePosition()
		{
		}

		private Vector3 ScreenToCanvas(Vector3 screenPosition)
		{
			return default;
		}
	}
}
