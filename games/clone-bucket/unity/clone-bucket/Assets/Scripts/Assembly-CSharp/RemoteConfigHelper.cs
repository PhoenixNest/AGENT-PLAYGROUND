using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
#if CLIK_SDK
using Tabtale.TTPlugins;
#endif

public class RemoteConfigHelper : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CTimeOutRemoteConfig_003Ed__10 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float remainingTime;

		public RemoteConfigHelper _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return null;
			}
		}

		public _003CTimeOutRemoteConfig_003Ed__10(int _003C_003E1__state)
		{
		}

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

		void IEnumerator.Reset()
		{
		}
	}

#if CLIK_SDK
	private const bool SdkRemoteConfig = true;
#else
	private const bool SdkRemoteConfig = false;
#endif

	private const float DefaultRemoteTimeOut = 5f;

	private float RemoteTimeOut;

	private static bool isRemoteReady = !SdkRemoteConfig;

	private static bool isRemoteEventTriggered;

	private static Action<bool> m_OnRemoteReady;

	public static RemoteConfigHelper Instance;

	public static event Action<bool> OnRemoteReady
	{
		add
		{
			m_OnRemoteReady += value;
			if (isRemoteReady && value != null)
			{
				value(true);
			}
		}
		remove
		{
			m_OnRemoteReady -= value;
		}
	}

	private void Awake()
	{
		Instance = this;
		if (RemoteTimeOut <= 0f)
		{
			RemoteTimeOut = DefaultRemoteTimeOut;
		}
#if CLIK_SDK
		TTPAnalytics.OnRemoteFetchCompletedEvent += OnRemoteConfigReady;
		StartCoroutine(TimeOutRemoteConfig(RemoteTimeOut));
#endif
	}

	private void OnDestroy()
	{
#if CLIK_SDK
		TTPAnalytics.OnRemoteFetchCompletedEvent -= OnRemoteConfigReady;
#endif
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnRemoteConfigReady(bool isReady)
	{
		if (isRemoteEventTriggered)
		{
			return;
		}
		isRemoteEventTriggered = true;
		isRemoteReady = true;
		m_OnRemoteReady?.Invoke(true);
	}

	public static bool IsRemoteReady()
	{
		return isRemoteReady;
	}

	private IEnumerator TimeOutRemoteConfig(float remainingTime)
	{
		yield return new WaitForSecondsRealtime(remainingTime);
		if (!isRemoteEventTriggered)
		{
			OnRemoteConfigReady(false);
		}
	}

	public static int GetInt(string key, int defaultVal)
	{
		string raw = GetRawValue(key);
		if (raw != null && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
		{
			return value;
		}
		return defaultVal;
	}

	public static bool GetBool(string key, bool defaultVal)
	{
		string raw = GetRawValue(key);
		if (raw == null)
		{
			return defaultVal;
		}
		string trimmed = raw.Trim();
		if (trimmed == "1" || string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (trimmed == "0" || string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return defaultVal;
	}

	public static float GetFloat(string key, float defaultVal)
	{
		string raw = GetRawValue(key);
		if (raw != null && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
		{
			return value;
		}
		return defaultVal;
	}

	public static string GetString(string key, string defaultVal = "")
	{
		string raw = GetRawValue(key);
		return raw ?? defaultVal;
	}

	private static string GetRawValue(string key)
	{
		if (!isRemoteReady || string.IsNullOrEmpty(key))
		{
			return null;
		}
		return TTPAnalytics_GetStringValue(key);
	}

	private static string TTPAnalytics_GetStringValue(string key)
	{
#if CLIK_SDK
		return TTPAnalytics.GetStringValue(key);
#else
		return null;
#endif
	}
}
