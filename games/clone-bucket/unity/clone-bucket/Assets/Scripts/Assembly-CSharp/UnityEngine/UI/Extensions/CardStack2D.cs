using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityEngine.UI.Extensions
{
	public class CardStack2D : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CButtonCooldown_003Ed__14 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public CardStack2D _003C_003E4__this;

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
			public _003CButtonCooldown_003Ed__14(int _003C_003E1__state)
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

		[SerializeField]
		private float cardMoveSpeed;

		[SerializeField]
		private float buttonCooldownTime;

		[SerializeField]
		private int cardZMultiplier;

		[SerializeField]
		private bool useDefaultUsedXPos;

		[SerializeField]
		private int usedCardXPos;

		[SerializeField]
		private KeyCode leftButton;

		[SerializeField]
		private KeyCode rightButton;

		[SerializeField]
		private Transform[] cards;

		private int cardArrayOffset;

		private Vector3[] cardPositions;

		private int xPowerDifference;

		public static bool canUseHorizontalAxis;

		private void Start()
		{
		}

		private void Update()
		{
		}

		[IteratorStateMachine(typeof(_003CButtonCooldown_003Ed__14))]
		private IEnumerator ButtonCooldown()
		{
			return null;
		}
	}
}
