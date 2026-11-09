using UnityEngine.Events;

namespace UnityEngine.UI.Extensions.Tweens
{
	public struct FloatTween : ITweenValue
	{
		public class FloatTweenCallback : UnityEvent<float>
		{
		}

		public class FloatFinishCallback : UnityEvent
		{
		}

		private float m_StartFloat;

		private float m_TargetFloat;

		private float m_Duration;

		private bool m_IgnoreTimeScale;

		private FloatTweenCallback m_Target;

		private FloatFinishCallback m_Finish;

		public float startFloat
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float targetFloat
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public float duration
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool ignoreTimeScale
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public void TweenValue(float floatPercentage)
		{
		}

		public void AddOnChangedCallback(UnityAction<float> callback)
		{
		}

		public void AddOnFinishCallback(UnityAction callback)
		{
		}

		public bool GetIgnoreTimescale()
		{
			return false;
		}

		public float GetDuration()
		{
			return 0f;
		}

		public bool ValidTarget()
		{
			return false;
		}

		public void Finished()
		{
		}
	}
}
