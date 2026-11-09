using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Segmented Control/Segmented Control")]
	[RequireComponent(typeof(RectTransform))]
	public class SegmentedControl : UIBehaviour
	{
		[Serializable]
		public class SegmentSelectedEvent : UnityEvent<int>
		{
		}

		[CompilerGenerated]
		private sealed class _003CDelayedInit_003Ed__27 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public SegmentedControl _003C_003E4__this;

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
			public _003CDelayedInit_003Ed__27(int _003C_003E1__state)
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

		private Selectable[] m_segments;

		[Tooltip("A GameObject with an Image to use as a separator between segments. Size of the RectTransform will determine the size of the separator used.\nNote, make sure to disable the separator GO so that it does not affect the scene")]
		[SerializeField]
		private Graphic m_separator;

		private float m_separatorWidth;

		[Tooltip("When True, it allows each button to be toggled on/off")]
		[SerializeField]
		private bool m_allowSwitchingOff;

		[Tooltip("The selected default for the control (zero indexed array)")]
		[SerializeField]
		private int m_selectedSegmentIndex;

		[Tooltip("Event to fire once the selection has been changed")]
		[SerializeField]
		private SegmentSelectedEvent m_onValueChanged;

		internal Selectable selectedSegment;

		protected float SeparatorWidth => 0f;

		public Selectable[] segments => null;

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

		public bool allowSwitchingOff
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public int selectedSegmentIndex
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public SegmentSelectedEvent onValueChanged
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		protected SegmentedControl()
		{
		}

		protected override void Start()
		{
		}

		protected override void OnEnable()
		{
		}

		[IteratorStateMachine(typeof(_003CDelayedInit_003Ed__27))]
		private IEnumerator DelayedInit()
		{
			return null;
		}

		private Selectable[] GetChildSegments()
		{
			return null;
		}

		private void RecreateSprites()
		{
		}

		internal static Sprite CutSprite(Sprite sprite, bool leftmost, bool rightmost)
		{
			return null;
		}

		public void LayoutSegments()
		{
		}
	}
}
