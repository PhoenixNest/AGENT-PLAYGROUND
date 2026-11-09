using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/UI Tween Scale")]
	public class UI_TweenScale : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CTween_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UI_TweenScale _003C_003E4__this;

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
			public _003CTween_003Ed__11(int _003C_003E1__state)
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

		[Tooltip("If true animation will loop, for best effect set animation curve to loop on start and end point")]
		public bool isLoop;

		[Tooltip("If true animation will start automatically, otherwise you need to call Play() method to start the animation")]
		public bool playAtAwake;

		[Tooltip("If true component will scale by the same amount in X and Y axis, otherwise use animCurve for X scale and animCurveY for Y scale")]
		[Space(10f)]
		[Header("Non uniform scale")]
		public bool isUniform;

		public AnimationCurve animCurveY;

		private Vector3 initScale;

		private Transform myTransform;

		private Vector3 newScale;

		private void Awake()
		{
		}

		public void Play()
		{
		}

		[IteratorStateMachine(typeof(_003CTween_003Ed__11))]
		private IEnumerator Tween()
		{
			return null;
		}

		public void ResetTween()
		{
		}
	}
}
