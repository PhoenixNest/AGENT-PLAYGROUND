using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(ScrollRect))]
	[ExecuteInEditMode]
	[AddComponentMenu("UI/Extensions/ContentSnapScrollHorizontal")]
	public class ContentScrollSnapHorizontal : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler
	{
		[Serializable]
		public class StartMovementEvent : UnityEvent
		{
		}

		[Serializable]
		public class CurrentItemChangeEvent : UnityEvent<int>
		{
		}

		[Serializable]
		public class FoundItemToSnapToEvent : UnityEvent<int>
		{
		}

		[Serializable]
		public class SnappedToItemEvent : UnityEvent<int>
		{
		}

		[Serializable]
		public struct MoveInfo
		{
			public enum IndexType
			{
				childIndex = 0,
				positionIndex = 1
			}

			[Tooltip("Child Index means the Index corresponds to the content item at that index in the hierarchy.\nPosition Index means the Index corresponds to the content item in that snap position.\nA higher Position Index in a Horizontal Scroll Snap means it would be further to the right.")]
			public IndexType indexType;

			[Tooltip("Zero based")]
			public int index;

			[Tooltip("If this is true the snap scroll will jump to the index, otherwise it will lerp there.")]
			public bool jump;

			[Tooltip("If jump is false this is the time it will take to lerp to the index")]
			public float duration;

			public MoveInfo(IndexType _indexType, int _index)
			{
				indexType = IndexType.childIndex;
				index = 0;
				jump = false;
				duration = 0f;
			}

			public MoveInfo(IndexType _indexType, int _index, bool _jump, float _duration)
			{
				indexType = IndexType.childIndex;
				index = 0;
				jump = false;
				duration = 0f;
			}
		}

		[CompilerGenerated]
		private sealed class _003CLerpToContent_003Ed__71 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ContentScrollSnapHorizontal _003C_003E4__this;

			private Vector3 _003CoriginalContentPos_003E5__2;

			private float _003CelapsedTime_003E5__3;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CLerpToContent_003Ed__71(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CSlideAndLerp_003Ed__70 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ContentScrollSnapHorizontal _003C_003E4__this;

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return null;
				}
			}

			[DebuggerHidden]
			public _003CSlideAndLerp_003Ed__70(int _003C_003E1__state)
			{
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			private bool MoveNext()
			{
				return false;
			}

			bool IEnumerator.MoveNext()
			{
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		public bool ignoreInactiveItems;

		public MoveInfo startInfo;

		public GameObject prevButton;

		public GameObject nextButton;

		public GameObject pagination;

		[Tooltip("The velocity below which the scroll rect will start to snap")]
		public int snappingVelocityThreshold;

		[Header("Paging Info")]
		[Tooltip("Should the pagination & buttons jump or lerp to the items")]
		public bool jumpToItem;

		[Tooltip("The time it will take for the pagination or buttons to move between items")]
		public float lerpTime;

		[Header("Events")]
		[SerializeField]
		[Tooltip("Event is triggered whenever the scroll rect starts to move, even when triggered programmatically")]
		private StartMovementEvent m_StartMovementEvent;

		[SerializeField]
		[Tooltip("Event is triggered whenever the closest item to the center of the scrollrect changes")]
		private CurrentItemChangeEvent m_CurrentItemChangeEvent;

		[Tooltip("Event is triggered when the ContentSnapScroll decides which item it is going to snap to. Returns the index of the closest position.")]
		[SerializeField]
		private FoundItemToSnapToEvent m_FoundItemToSnapToEvent;

		[Tooltip("Event is triggered when we finally settle on an element. Returns the index of the item's position.")]
		[SerializeField]
		private SnappedToItemEvent m_SnappedToItemEvent;

		private ScrollRect scrollRect;

		private RectTransform scrollRectTransform;

		private RectTransform contentTransform;

		private List<Vector3> contentPositions;

		private Vector3 lerpTarget;

		private float totalScrollableWidth;

		private DrivenRectTransformTracker tracker;

		private float mLerpTime;

		private int _closestItem;

		private bool mSliding;

		private bool mLerping;

		public StartMovementEvent MovementStarted
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public CurrentItemChangeEvent CurrentItemChanged
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public FoundItemToSnapToEvent ItemFoundToSnap
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public SnappedToItemEvent ItemSnappedTo
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		private bool ContentIsHorizonalLayoutGroup => false;

		public bool Moving => false;

		public bool Sliding => false;

		public bool Lerping => false;

		public int ClosestItemIndex => 0;

		public int LerpTargetIndex => 0;

		private bool IsScrollRectAvailable => false;

		private void Awake()
		{
		}

		public void SetNewItems(ref List<Transform> newItems)
		{
		}

		private void OnDisable()
		{
		}

		private void SetupDrivenTransforms()
		{
		}

		private void SetupSnapScroll()
		{
		}

		private void SetupWithHorizontalLayoutGroup()
		{
		}

		private void SetupWithCalculatedSpacing()
		{
		}

		public void GoTo(MoveInfo info)
		{
		}

		private void GoToChild(int index, bool jump)
		{
		}

		private void GoToContentPos(int index, bool jump)
		{
		}

		public void NextItem()
		{
		}

		public void PreviousItem()
		{
		}

		public void UpdateLayout()
		{
		}

		public void UpdateLayoutAndMoveTo(MoveInfo info)
		{
		}

		public void OnBeginDrag(PointerEventData ped)
		{
		}

		public void OnEndDrag(PointerEventData ped)
		{
		}

		private void Update()
		{
		}

		[IteratorStateMachine(typeof(_003CSlideAndLerp_003Ed__70))]
		private IEnumerator SlideAndLerp()
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CLerpToContent_003Ed__71))]
		private IEnumerator LerpToContent()
		{
			return null;
		}

		private void StopMovement()
		{
		}

		private void ChangePaginationInfo(int targetScreen)
		{
		}

		private Vector2 DstFromTopLeftOfTransformToTopLeftOfParent(RectTransform rt)
		{
			return default;
		}

		private Vector3 FindClosestFrom(Vector3 start)
		{
			return default;
		}
	}
}
