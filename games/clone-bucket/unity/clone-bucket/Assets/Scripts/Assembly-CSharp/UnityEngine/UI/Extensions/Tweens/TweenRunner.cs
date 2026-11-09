using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.UI.Extensions.Tweens
{
	internal class TweenRunner<T> where T : struct, ITweenValue
	{
		[CompilerGenerated]
		private sealed class _003CStart_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public T tweenInfo;

			private float _003CelapsedTime_003E5__2;

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
			public _003CStart_003Ed__2(int _003C_003E1__state)
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

		protected MonoBehaviour m_CoroutineContainer;

		protected IEnumerator m_Tween;

		[IteratorStateMachine(typeof(TweenRunner<>._003CStart_003Ed__2))]
		private static IEnumerator Start(T tweenInfo)
		{
			return null;
		}

		public void Init(MonoBehaviour coroutineContainer)
		{
		}

		public void StartTween(T info)
		{
		}
	}
}
