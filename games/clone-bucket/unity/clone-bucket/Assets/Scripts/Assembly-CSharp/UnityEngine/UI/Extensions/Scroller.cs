using System;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions.EasingCore;

namespace UnityEngine.UI.Extensions
{
	public class Scroller : UIBehaviour, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
	{
		[Serializable]
		private class Snap
		{
			public bool Enable;

			public float VelocityThreshold;

			public float Duration;

			public Ease Easing;
		}

		private class AutoScrollState
		{
			public bool Enable;

			public bool Elastic;

			public float Duration;

			public EasingFunction EasingFunction;

			public float StartTime;

			public float EndPosition;

			public Action OnComplete;

			public void Reset()
			{
			}

			public void Complete()
			{
			}
		}

		[SerializeField]
		private RectTransform viewport;

		[SerializeField]
		private ScrollDirection scrollDirection;

		[SerializeField]
		private MovementType movementType;

		[SerializeField]
		private float elasticity;

		[SerializeField]
		private float scrollSensitivity;

		[SerializeField]
		private bool inertia;

		[SerializeField]
		private float decelerationRate;

		[SerializeField]
		private Snap snap;

		[SerializeField]
		private bool draggable;

		[SerializeField]
		private Scrollbar scrollbar;

		private readonly AutoScrollState autoScrollState;

		private Action<float> onValueChanged;

		private Action<int> onSelectionChanged;

		private Vector2 beginDragPointerPosition;

		private float scrollStartPosition;

		private float prevPosition;

		private float currentPosition;

		private int totalCount;

		private bool hold;

		private bool scrolling;

		private bool dragging;

		private float velocity;

		private static readonly EasingFunction DefaultEasingFunction;

		public float ViewportSize => 0f;

		public ScrollDirection ScrollDirection => ScrollDirection.Vertical;

		public MovementType MovementType
		{
			get
			{
				return MovementType.Unrestricted;
			}
			set
			{
			}
		}

		public float Elasticity
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float ScrollSensitivity
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool Inertia
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public float DecelerationRate
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool SnapEnabled
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool Draggable
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public Scrollbar Scrollbar => null;

		public float Position
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		protected override void Start()
		{
		}

		public void OnValueChanged(Action<float> callback)
		{
		}

		public void OnSelectionChanged(Action<int> callback)
		{
		}

		public void SetTotalCount(int totalCount)
		{
		}

		public void ScrollTo(float position, float duration, Action onComplete = null)
		{
		}

		public void ScrollTo(float position, float duration, Ease easing, Action onComplete = null)
		{
		}

		public void ScrollTo(float position, float duration, EasingFunction easingFunction, Action onComplete = null)
		{
		}

		public void JumpTo(int index)
		{
		}

		public MovementDirection GetMovementDirection(int sourceIndex, int destIndex)
		{
			return MovementDirection.Left;
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
		}

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
		}

		void IScrollHandler.OnScroll(PointerEventData eventData)
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

		private float CalculateOffset(float position)
		{
			return 0f;
		}

		private void UpdatePosition(float position, bool updateScrollbar = true)
		{
		}

		private void UpdateSelection(int index)
		{
		}

		private float RubberDelta(float overStretching, float viewSize)
		{
			return 0f;
		}

		private void Update()
		{
		}

		private float CalculateMovementAmount(float sourcePosition, float destPosition)
		{
			return 0f;
		}

		private float CircularPosition(float p, int size)
		{
			return 0f;
		}
	}
}
