using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	public class ScrollSnapBase : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IScrollSnap, IPointerClickHandler
	{
		[Serializable]
		public class SelectionChangeStartEvent : UnityEvent
		{
		}

		[Serializable]
		public class SelectionPageChangedEvent : UnityEvent<int>
		{
		}

		[Serializable]
		public class SelectionChangeEndEvent : UnityEvent<int>
		{
		}

		internal Rect panelDimensions;

		internal RectTransform _screensContainer;

		internal bool _isVertical;

		internal int _screens;

		internal float _scrollStartPosition;

		internal float _childSize;

		private float _childPos;

		private float _maskSize;

		internal Vector2 _childAnchorPoint;

		internal ScrollRect _scroll_rect;

		internal Vector3 _lerp_target;

		internal bool _lerp;

		internal bool _pointerDown;

		internal bool _settled;

		internal Vector3 _startPosition;

		[Tooltip("The currently active page")]
		internal int _currentPage;

		internal int _previousPage;

		internal int _halfNoVisibleItems;

		internal bool _isInfinite;

		internal int _infiniteWindow;

		internal float _infiniteOffset;

		private int _bottomItem;

		private int _topItem;

		internal bool _startEventCalled;

		internal bool _endEventCalled;

		internal bool _suspendEvents;

		[SerializeField]
		[Tooltip("The screen / page to start the control on\n*Note, this is a 0 indexed array")]
		public int StartingScreen;

		[Tooltip("The distance between two pages based on page height, by default pages are next to each other")]
		[SerializeField]
		[Range(0f, 8f)]
		public float PageStep;

		[Tooltip("The gameobject that contains toggles which suggest pagination. (optional)")]
		public GameObject Pagination;

		[Tooltip("Button to go to the previous page. (optional)")]
		public GameObject PrevButton;

		[Tooltip("Button to go to the next page. (optional)")]
		public GameObject NextButton;

		[Tooltip("Transition speed between pages. (optional)")]
		public float transitionSpeed;

		[Tooltip("Hard Swipe forces to swiping to the next / previous page (optional)")]
		public bool UseHardSwipe;

		[Tooltip("Fast Swipe makes swiping page next / previous (optional)")]
		public bool UseFastSwipe;

		[Tooltip("Swipe Delta Threshold looks at the speed of input to decide if a swipe will be initiated (optional)")]
		public bool UseSwipeDeltaThreshold;

		[Tooltip("Offset for how far a swipe has to travel to initiate a page change (optional)")]
		public int FastSwipeThreshold;

		[Tooltip("Speed at which the ScrollRect will keep scrolling before slowing down and stopping (optional)")]
		public int SwipeVelocityThreshold;

		[Tooltip("Threshold for swipe speed to initiate a swipe, below threshold will return to closest page (optional)")]
		public float SwipeDeltaThreshold;

		[Tooltip("Use time scale instead of unscaled time (optional)")]
		public bool UseTimeScale;

		[Tooltip("The visible bounds area, controls which items are visible/enabled. *Note Should use a RectMask. (optional)")]
		public RectTransform MaskArea;

		[Tooltip("Pixel size to buffer around Mask Area. (optional)")]
		public float MaskBuffer;

		[Tooltip("By default the container will lerp to the start when enabled in the scene, this option overrides this and forces it to simply jump without lerping")]
		public bool JumpOnEnable;

		[Tooltip("By default the container will return to the original starting page when enabled, this option overrides this behaviour and stays on the current selection")]
		public bool RestartOnEnable;

		[Tooltip("(Experimental)\nBy default, child array objects will use the parent transform\nHowever you can disable this for some interesting effects")]
		public bool UseParentTransform;

		[Tooltip("Scroll Snap children. (optional)\nEither place objects in the scene as children OR\nPrefabs in this array, NOT BOTH")]
		public GameObject[] ChildObjects;

		[SerializeField]
		[Tooltip("Event fires when a user starts to change the selection")]
		private SelectionChangeStartEvent m_OnSelectionChangeStartEvent;

		[Tooltip("Event fires as the page changes, while dragging or jumping")]
		[SerializeField]
		private SelectionPageChangedEvent m_OnSelectionPageChangedEvent;

		[Tooltip("Event fires when the page settles after a user has dragged")]
		[SerializeField]
		private SelectionChangeEndEvent m_OnSelectionChangeEndEvent;

		public int CurrentPage
		{
			get
			{
				return 0;
			}
			internal set
			{
			}
		}

		public SelectionChangeStartEvent OnSelectionChangeStartEvent
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public SelectionPageChangedEvent OnSelectionPageChangedEvent
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public SelectionChangeEndEvent OnSelectionChangeEndEvent
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		private void Awake()
		{
		}

		internal void InitialiseChildObjects()
		{
		}

		internal void InitialiseChildObjectsFromScene()
		{
		}

		internal void InitialiseChildObjectsFromArray()
		{
		}

		internal void UpdateVisible()
		{
		}

		public void NextScreen()
		{
		}

		public void PreviousScreen()
		{
		}

		public void GoToScreen(int screenIndex)
		{
		}

		internal int GetPageforPosition(Vector3 pos)
		{
			return 0;
		}

		internal bool IsRectSettledOnaPage(Vector3 pos)
		{
			return false;
		}

		internal void GetPositionforPage(int page, ref Vector3 target)
		{
		}

		internal void ScrollToClosestElement()
		{
		}

		internal void OnCurrentScreenChange(int currentScreen)
		{
		}

		private void ChangeBulletsInfo(int targetScreen)
		{
		}

		private void ToggleNavigationButtons(int targetScreen)
		{
		}

		private void OnValidate()
		{
		}

		public void StartScreenChange()
		{
		}

		internal void ScreenChange()
		{
		}

		internal void EndScreenChange()
		{
		}

		public Transform CurrentPageObject()
		{
			return null;
		}

		public void CurrentPageObject(out Transform returnObject)
		{
			returnObject = null;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		public virtual void OnEndDrag(PointerEventData eventData)
		{
		}

		int IScrollSnap.CurrentPage()
		{
			return 0;
		}

		public void SetLerp(bool value)
		{
		}

		public void ChangePage(int page)
		{
		}

		public void OnPointerClick(PointerEventData eventData)
		{
		}
	}
}
