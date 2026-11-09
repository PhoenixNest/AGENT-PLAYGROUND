using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Scroll Snap")]
	[ExecuteInEditMode]
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollSnap : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IDragHandler, IScrollSnap
	{
		public enum ScrollDirection
		{
			Horizontal = 0,
			Vertical = 1
		}

		public delegate void PageSnapChange(int page);

		private ScrollRect _scroll_rect;

		private RectTransform _scrollRectTransform;

		private Transform _listContainerTransform;

		private int _pages;

		private int _startingPage;

		private Vector3[] _pageAnchorPositions;

		private Vector3 _lerpTarget;

		private bool _lerp;

		private float _listContainerMinPosition;

		private float _listContainerMaxPosition;

		private float _listContainerSize;

		private RectTransform _listContainerRectTransform;

		private Vector2 _listContainerCachedSize;

		private float _itemSize;

		private int _itemsCount;

		private bool _startDrag;

		private Vector3 _positionOnDragStart;

		private int _pageOnDragStart;

		private bool _fastSwipeTimer;

		private int _fastSwipeCounter;

		private int _fastSwipeTarget;

		[Tooltip("Button to go to the next page. (optional)")]
		public Button NextButton;

		[Tooltip("Button to go to the previous page. (optional)")]
		public Button PrevButton;

		[Tooltip("Number of items visible in one page of scroll frame.")]
		[Range(1f, 100f)]
		public int ItemsVisibleAtOnce;

		[Tooltip("Sets minimum width of list items to 1/itemsVisibleAtOnce.")]
		public bool AutoLayoutItems;

		[Tooltip("If you wish to update scrollbar numberOfSteps to number of active children on list.")]
		public bool LinkScrolbarSteps;

		[Tooltip("If you wish to update scrollrect sensitivity to size of list element.")]
		public bool LinkScrolrectScrollSensitivity;

		public bool UseFastSwipe;

		public int FastSwipeThreshold;

		[CompilerGenerated]
		private PageSnapChange m_onPageChange;

		public ScrollDirection direction;

		private bool fastSwipe;

		public event PageSnapChange onPageChange
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		private void Start()
		{
		}

		public void UpdateListItemsSize()
		{
		}

		public void UpdateListItemPositions()
		{
		}

		public void ResetPage()
		{
		}

		private void UpdateScrollbar(bool linkSteps)
		{
		}

		private void LateUpdate()
		{
		}

		public void NextScreen()
		{
		}

		public void PreviousScreen()
		{
		}

		private void NextScreenCommand()
		{
		}

		private void PrevScreenCommand()
		{
		}

		public int CurrentPage()
		{
			return 0;
		}

		public void SetLerp(bool value)
		{
		}

		public void ChangePage(int page)
		{
		}

		private void PageChanged(int currentPage)
		{
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		public void StartScreenChange()
		{
		}
	}
}
