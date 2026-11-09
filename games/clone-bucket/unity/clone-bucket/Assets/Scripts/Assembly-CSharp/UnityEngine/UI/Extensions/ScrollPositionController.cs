using System;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	public class ScrollPositionController : UIBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler
	{
		private enum ScrollDirection
		{
			Vertical = 0,
			Horizontal = 1
		}

		private enum MovementType
		{
			Unrestricted = 0,
			Elastic = 1,
			Clamped = 2
		}

		[Serializable]
		private struct Snap
		{
			public bool Enable;

			public float VelocityThreshold;

			public float Duration;
		}

		private class AutoScrollState
		{
			public bool Enable;

			public bool Elastic;

			public float Duration;

			public float StartTime;

			public float EndScrollPosition;

			public void Reset()
			{
			}
		}

		[SerializeField]
		private RectTransform viewport;

		[SerializeField]
		private ScrollDirection directionOfRecognize;

		[SerializeField]
		private MovementType movementType;

		[SerializeField]
		private float elasticity;

		[SerializeField]
		private float scrollSensitivity;

		[SerializeField]
		private bool inertia;

		[SerializeField]
		[Tooltip("Only used when inertia is enabled")]
		private float decelerationRate;

		[SerializeField]
		[Tooltip("Only used when inertia is enabled")]
		private Snap snap;

		[SerializeField]
		private int dataCount;

		private readonly AutoScrollState autoScrollState;

		private Action<float> onUpdatePosition;

		private Action<int> onItemSelected;

		private Vector2 pointerStartLocalPosition;

		private float dragStartScrollPosition;

		private float prevScrollPosition;

		private float currentScrollPosition;

		private bool dragging;

		private float velocity;

		public void OnUpdatePosition(Action<float> onUpdatePosition)
		{
		}

		public void OnItemSelected(Action<int> onItemSelected)
		{
		}

		public void SetDataCount(int dataCount)
		{
		}

		public void ScrollTo(int index, float duration)
		{
		}

		public void JumpTo(int index)
		{
		}

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
		}

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
		}

		private float GetViewportSize()
		{
			return 0f;
		}

		private float CalculateOffset(float position)
		{
			return 0f;
		}

		private void UpdatePosition(float position)
		{
		}

		private void ItemSelected(int index)
		{
		}

		private float RubberDelta(float overStretching, float viewSize)
		{
			return 0f;
		}

		private void Update()
		{
		}

		private int CalculateDestinationIndex(int index)
		{
			return 0;
		}

		private int CalculateClosestIndex(int index)
		{
			return 0;
		}

		private float GetCircularPosition(float position, int length)
		{
			return 0f;
		}

		private float EaseInOutCubic(float start, float end, float value)
		{
			return 0f;
		}
	}
}
