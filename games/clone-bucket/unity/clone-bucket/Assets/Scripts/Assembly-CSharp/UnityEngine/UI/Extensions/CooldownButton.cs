using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Extensions/Cooldown Button")]
	public class CooldownButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
	{
		[Serializable]
		public class CooldownButtonEvent : UnityEvent<PointerEventData.InputButton>
		{
		}

		[SerializeField]
		private float cooldownTimeout;

		[SerializeField]
		private float cooldownSpeed;

		[SerializeField]
		[ReadOnly]
		private bool cooldownActive;

		[SerializeField]
		[ReadOnly]
		private bool cooldownInEffect;

		[SerializeField]
		[ReadOnly]
		private float cooldownTimeElapsed;

		[ReadOnly]
		[SerializeField]
		private float cooldownTimeRemaining;

		[ReadOnly]
		[SerializeField]
		private int cooldownPercentRemaining;

		[SerializeField]
		[ReadOnly]
		private int cooldownPercentComplete;

		private PointerEventData buttonSource;

		[Tooltip("Event that fires when a button is initially pressed down")]
		public CooldownButtonEvent OnCooldownStart;

		[Tooltip("Event that fires when a button is released")]
		public CooldownButtonEvent OnButtonClickDuringCooldown;

		[Tooltip("Event that continually fires while a button is held down")]
		public CooldownButtonEvent OnCoolDownFinish;

		public float CooldownTimeout
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float CooldownSpeed
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool CooldownInEffect => false;

		public bool CooldownActive
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public float CooldownTimeElapsed
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float CooldownTimeRemaining => 0f;

		public int CooldownPercentRemaining => 0;

		public int CooldownPercentComplete => 0;

		private void Update()
		{
		}

		public void PauseCooldown()
		{
		}

		public void RestartCooldown()
		{
		}

		public void StartCooldown()
		{
		}

		public void StopCooldown()
		{
		}

		public void CancelCooldown()
		{
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
		}
	}
}
