using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("UI/Extensions/Stepper")]
	public class Stepper : UIBehaviour
	{
		[Serializable]
		public class StepperValueChangedEvent : UnityEvent<int>
		{
		}

		[CompilerGenerated]
		private sealed class _003CDelayedInit_003Ed__38 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public Stepper _003C_003E4__this;

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
			public _003CDelayedInit_003Ed__38(int _003C_003E1__state)
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

		private Selectable[] _sides;

		[Tooltip("The current step value of the control")]
		[SerializeField]
		private int _value;

		[SerializeField]
		[Tooltip("The minimum step value allowed by the control. When reached it will disable the '-' button")]
		private int _minimum;

		[Tooltip("The maximum step value allowed by the control. When reached it will disable the '+' button")]
		[SerializeField]
		private int _maximum;

		[SerializeField]
		[Tooltip("The step increment used to increment / decrement the step value")]
		private int _step;

		[SerializeField]
		[Tooltip("Does the step value loop around from end to end")]
		private bool _wrap;

		[SerializeField]
		[Tooltip("A GameObject with an Image to use as a separator between segments. Size of the RectTransform will determine the size of the separator used.\nNote, make sure to disable the separator GO so that it does not affect the scene")]
		private Graphic _separator;

		private float _separatorWidth;

		[SerializeField]
		private StepperValueChangedEvent _onValueChanged;

		private float separatorWidth => 0f;

		public Selectable[] sides => null;

		public int value
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public int minimum
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public int maximum
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public int step
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public bool wrap
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public Graphic separator
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public StepperValueChangedEvent onValueChanged
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		protected Stepper()
		{
		}

		protected override void Start()
		{
		}

		protected override void OnEnable()
		{
		}

		[IteratorStateMachine(typeof(_003CDelayedInit_003Ed__38))]
		private IEnumerator DelayedInit()
		{
			return null;
		}

		private Selectable[] GetSides()
		{
			return null;
		}

		public void StepUp()
		{
		}

		public void StepDown()
		{
		}

		private void Step(int amount)
		{
		}

		private void DisableAtExtremes(Selectable[] sides)
		{
		}

		private void RecreateSprites(Selectable[] sides)
		{
		}

		internal static Sprite CutSprite(Sprite sprite, bool leftmost)
		{
			return null;
		}

		public void LayoutSides(Selectable[] sides = null)
		{
		}
	}
}
