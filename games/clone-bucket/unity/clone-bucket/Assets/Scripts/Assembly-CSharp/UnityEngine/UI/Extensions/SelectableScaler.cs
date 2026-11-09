using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.EventSystems;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Selectable Scalar")]
	[RequireComponent(typeof(Button))]
	public class SelectableScaler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
	{
		[CompilerGenerated]
		private sealed class _003CScaleIN_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public SelectableScaler _003C_003E4__this;

			private float _003Ct_003E5__2;

			private float _003CmaxT_003E5__3;

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
			public _003CScaleIN_003Ed__11(int _003C_003E1__state)
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
		private sealed class _003CScaleOUT_003Ed__12 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public SelectableScaler _003C_003E4__this;

			private float _003Ct_003E5__2;

			private float _003CmaxT_003E5__3;

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
			public _003CScaleOUT_003Ed__12(int _003C_003E1__state)
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

		public AnimationCurve animCurve;

		[Tooltip("Animation speed multiplier")]
		public float speed;

		private Vector3 initScale;

		public Transform target;

		private Selectable selectable;

		public Selectable Target => null;

		private void Awake()
		{
		}

		private void OnEnable()
		{
		}

		public void OnPointerDown(PointerEventData eventData)
		{
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		}

		[IteratorStateMachine(typeof(_003CScaleIN_003Ed__11))]
		private IEnumerator ScaleIN()
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CScaleOUT_003Ed__12))]
		private IEnumerator ScaleOUT()
		{
			return null;
		}
	}
}
