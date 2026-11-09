using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(ScrollRect))]
	[AddComponentMenu("UI/Extensions/UI Magnetic Infinite Scroll")]
	public class UI_MagneticInfiniteScroll : UI_InfiniteScroll, IDragHandler, IEventSystemHandler, IEndDragHandler, IScrollHandler
	{
		[CompilerGenerated]
		private sealed class _003CSetInitContent_003Ed__25 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UI_MagneticInfiniteScroll _003C_003E4__this;

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
			public _003CSetInitContent_003Ed__25(int _003C_003E1__state)
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
		private Action<GameObject> m_OnNewSelect;

		[SerializeField]
		[Tooltip("The pointer to the pivot, the visual element for centering objects.")]
		private RectTransform pivot;

		[Tooltip("The maximum speed that allows you to activate the magnet to center on the pivot")]
		[SerializeField]
		private float maxSpeedForMagnetic;

		[Tooltip("The index of the object which must be initially centered")]
		[SerializeField]
		private int indexStart;

		[Tooltip("The time to decelerate and aim to the pivot")]
		[SerializeField]
		private float timeForDeceleration;

		private float _pastPositionMouseSpeed;

		private float _initMovementDirection;

		private float _pastPosition;

		private float _currentSpeed;

		private float _stopValue;

		private readonly float _waitForContentSet;

		private float _currentTime;

		private int _nearestIndex;

		private bool _useMagnetic;

		private bool _isStopping;

		private bool _isMovement;

		public List<RectTransform> Items { get; }

		public event Action<GameObject> OnNewSelect
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

		protected override void Awake()
		{
		}

		private void Update()
		{
		}

		public override void SetNewItems(ref List<Transform> newItems)
		{
		}

		public void SetContentInPivot(int index)
		{
		}

		[IteratorStateMachine(typeof(_003CSetInitContent_003Ed__25))]
		private IEnumerator SetInitContent()
		{
			return null;
		}

		private float GetAnchoredPositionForPivot(int index)
		{
			return 0f;
		}

		private void FinishPrepareMovement()
		{
		}

		private float GetRightAxis(Vector2 vector)
		{
			return 0f;
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
		}

		public void OnScroll(PointerEventData eventData)
		{
		}
	}
}
