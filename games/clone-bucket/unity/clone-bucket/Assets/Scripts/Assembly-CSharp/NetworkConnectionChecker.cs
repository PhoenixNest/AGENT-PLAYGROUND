using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class NetworkConnectionChecker : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCheckInternetConnection_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public NetworkConnectionChecker _003C_003E4__this;

		private UnityWebRequest _003Crequest_003E5__2;

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
		public _003CCheckInternetConnection_003Ed__11(int _003C_003E1__state)
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

	[Header("Configuration")]
	[Tooltip("URL to ping for connectivity test")]
	public string pingURL;

	[Tooltip("Timeout in seconds for connectivity test")]
	public float timeout;

	[Tooltip("How often to check connection (in seconds)")]
	public float checkInterval;

	[Header("Events")]
	public UnityEvent onConnected;

	public UnityEvent onDisconnected;

	private bool _isConnected;

	private float _lastCheckTime;

	public bool IsConnected => false;

	private void Start()
	{
	}

	private void Update()
	{
	}

	[IteratorStateMachine(typeof(_003CCheckInternetConnection_003Ed__11))]
	private IEnumerator CheckInternetConnection()
	{
		return null;
	}

	private void UpdateConnectionStatus(bool connected)
	{
	}

	public void CheckConnection()
	{
	}

	public bool IsNetworkAvailable()
	{
		return false;
	}

	public string GetNetworkType()
	{
		return null;
	}
}
