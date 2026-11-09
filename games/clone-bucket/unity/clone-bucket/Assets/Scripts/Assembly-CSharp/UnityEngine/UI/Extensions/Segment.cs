using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(Selectable))]
	[AddComponentMenu("UI/Extensions/Segmented Control/Segment")]
	public class Segment : UIBehaviour, IPointerClickHandler, IEventSystemHandler, ISubmitHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
	{
		[CompilerGenerated]
		private sealed class _003CDelayedInit_003Ed__14 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public Segment _003C_003E4__this;

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
			public _003CDelayedInit_003Ed__14(int _003C_003E1__state)
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

		internal int index;

		internal SegmentedControl segmentedControl;

		internal Sprite cutSprite;

		internal bool leftmost => false;

		internal bool rightmost => false;

		public bool selected
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		internal Selectable button => null;

		protected Segment()
		{
		}

		protected override void Start()
		{
		}

		[IteratorStateMachine(typeof(_003CDelayedInit_003Ed__14))]
		private IEnumerator DelayedInit()
		{
			return null;
		}

		public virtual void OnPointerClick(PointerEventData eventData)
		{
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
		}

		public virtual void OnPointerExit(PointerEventData eventData)
		{
		}

		public virtual void OnPointerDown(PointerEventData eventData)
		{
		}

		public virtual void OnPointerUp(PointerEventData eventData)
		{
		}

		public virtual void OnSelect(BaseEventData eventData)
		{
		}

		public virtual void OnDeselect(BaseEventData eventData)
		{
		}

		protected override void OnEnable()
		{
		}

		public virtual void OnSubmit(BaseEventData eventData)
		{
		}

		private void SetSelected(bool value)
		{
		}

		private void Deselect()
		{
		}

		private void MaintainSelection()
		{
		}

		internal void TransitionButton()
		{
		}

		internal void TransitionButton(bool instant)
		{
		}

		private void StartColorTween(Color targetColor, bool instant)
		{
		}

		private void ChangeTextColor(Color targetColor)
		{
		}

		private void DoSpriteSwap(Sprite newSprite)
		{
		}

		private void TriggerAnimation(string triggername)
		{
		}
	}
}
