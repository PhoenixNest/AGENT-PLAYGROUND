using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("UI/Extensions/Tooltip/Tooltip Trigger")]
	public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
	{
		public enum TooltipPositioningType
		{
			mousePosition = 0,
			mousePositionAndFollow = 1,
			transformPosition = 2
		}

		[CompilerGenerated]
		private sealed class _003CHoveredMouseFollowingLoop_003Ed__10 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public TooltipTrigger _003C_003E4__this;

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
			public _003CHoveredMouseFollowingLoop_003Ed__10(int _003C_003E1__state)
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

		[TextArea]
		public string text;

		[Tooltip("Defines where the tooltip will be placed and how that placement will occur. Transform position will always be used if this element wasn't selected via mouse")]
		public TooltipPositioningType tooltipPositioningType;

		private bool isChildOfOverlayCanvas;

		private bool hovered;

		public Vector3 offset;

		public bool WorldToScreenIsRequired => false;

		private void Start()
		{
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		[IteratorStateMachine(typeof(_003CHoveredMouseFollowingLoop_003Ed__10))]
		private IEnumerator HoveredMouseFollowingLoop()
		{
			return null;
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

		private void StartHover(Vector3 position, bool shouldCanvasUpdate = false)
		{
		}

		private void StopHover()
		{
		}
	}
}
