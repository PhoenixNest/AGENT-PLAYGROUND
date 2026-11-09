using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/ScrollRectTweener")]
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectTweener : MonoBehaviour, IDragHandler, IEventSystemHandler
	{
		[CompilerGenerated]
		private sealed class _003CDoMove_003Ed__17 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public float duration;

			public ScrollRectTweener _003C_003E4__this;

			private Vector2 _003CposOffset_003E5__2;

			private float _003CcurrentTime_003E5__3;

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
			public _003CDoMove_003Ed__17(int _003C_003E1__state)
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

		private ScrollRect scrollRect;

		private Vector2 startPos;

		private Vector2 targetPos;

		private bool wasHorizontal;

		private bool wasVertical;

		public float moveSpeed;

		public bool disableDragWhileTweening;

		private void Awake()
		{
		}

		public void ScrollHorizontal(float normalizedX)
		{
		}

		public void ScrollHorizontal(float normalizedX, float duration)
		{
		}

		public void ScrollVertical(float normalizedY)
		{
		}

		public void ScrollVertical(float normalizedY, float duration)
		{
		}

		public void Scroll(Vector2 normalizedPos)
		{
		}

		private float GetScrollDuration(Vector2 normalizedPos)
		{
			return 0f;
		}

		private Vector2 DeNormalize(Vector2 normalizedPos)
		{
			return default;
		}

		private Vector2 GetCurrentPos()
		{
			return default;
		}

		public void Scroll(Vector2 normalizedPos, float duration)
		{
		}

		[IteratorStateMachine(typeof(_003CDoMove_003Ed__17))]
		private IEnumerator DoMove(float duration)
		{
			return null;
		}

		public Vector2 EaseVector(float currentTime, Vector2 startValue, Vector2 changeInValue, float duration)
		{
			return default;
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		private void StopScroll()
		{
		}

		private void LockScrollability()
		{
		}

		private void RestoreScrollability()
		{
		}
	}
}
