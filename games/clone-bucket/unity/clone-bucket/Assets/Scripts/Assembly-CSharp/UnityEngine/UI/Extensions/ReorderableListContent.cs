using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.UI.Extensions
{
	[DisallowMultipleComponent]
	public class ReorderableListContent : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CRefreshChildren_003Ed__9 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ReorderableListContent _003C_003E4__this;

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
			public _003CRefreshChildren_003Ed__9(int _003C_003E1__state)
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

		private List<Transform> _cachedChildren;

		private List<ReorderableListElement> _cachedListElement;

		private ReorderableListElement _ele;

		private ReorderableList _extList;

		private RectTransform _rect;

		private bool _started;

		private void OnEnable()
		{
		}

		public void OnTransformChildrenChanged()
		{
		}

		public void Init(ReorderableList extList)
		{
		}

		[IteratorStateMachine(typeof(_003CRefreshChildren_003Ed__9))]
		private IEnumerator RefreshChildren()
		{
			return null;
		}
	}
}
