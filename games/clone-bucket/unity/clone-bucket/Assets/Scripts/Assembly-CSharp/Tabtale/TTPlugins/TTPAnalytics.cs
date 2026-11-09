using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	public class TTPAnalytics
	{
		public delegate int GetUserScoreDelegate();

		private interface IAnalytics
		{
			void LogEvent(long targets, string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal = false);

			void EndLogEvent(string eventName, IDictionary<string, object> eventParams);

			void AddExtras(IDictionary<string, object> extras);

			void RemoveExtras(IList<string> keys);

			bool GetRemoteValue(string key, double timeoutInSecs);

			string GetRemoteStringSync(string key);

			bool GetRemoteDictionary(IList<string> keys, double timeoutInSecs);

			bool IsRemoteFetchComplete();

			string GetFirebaseInstanceId();

			string GetCurrentConfig();

			void DdnaIsReady(bool isReady, string userId);

			void GetGeo();

			string GetAdditionalParams();

			void SetUserProperties(Dictionary<string, string> properties);
		}

		private class EditorImpl : IAnalytics
		{
			public void LogEvent(long targets, string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal)
			{
			}

			public void EndLogEvent(string eventName, IDictionary<string, object> eventParams)
			{
			}

			public bool CallDecisionPoint(string decisionPoint, Dictionary<string, object> parameters, double timeoutInSecs)
			{
				return false;
			}

			public void AddExtras(IDictionary<string, object> extras)
			{
			}

			public void RemoveExtras(IList<string> keys)
			{
			}

			public bool GetRemoteValue(string key, double timeoutInSecs)
			{
				return false;
			}

			public string GetRemoteStringSync(string key)
			{
				return null;
			}

			public bool GetRemoteDictionary(IList<string> keys, double timeoutInSecs)
			{
				return false;
			}

			public bool ShowImageMessage(string decisionPoint)
			{
				return false;
			}

			public bool IsRemoteFetchComplete()
			{
				return false;
			}

			public string GetFirebaseInstanceId()
			{
				return null;
			}

			public string GetCurrentConfig()
			{
				return null;
			}

			public void DdnaIsReady(bool isReady, string userId)
			{
			}

			public void GetGeo()
			{
			}

			public string GetAdditionalParams()
			{
				return null;
			}

			public void SetUserProperties(Dictionary<string, string> properties)
			{
			}

		}

		[Preserve]
		private class AnalyticsDelegate : MonoBehaviour
		{
			private class OnRemoteFetchCompleteMessage
			{
				public bool fetchSucceeded;
			}

			private class OnRemoteValueProviderReadyMessage
			{
				public bool ready;
			}

			private class OnGotGetCodeMessage
			{
				public string geo;
			}

			private class OnReceivedInstallationIdMessage
			{
				public string installationId;
			}

			public void RemoteConfigReady(string remoteConfig)
			{
			}

			public void onRemoteFetchCompleted(string messageStr)
			{
			}

			public void onRemoteValueProviderReady(string messageStr)
			{
			}

			public void onRequestValueComplete(string messageStr)
			{
			}

			public void onRequestValueDictionaryComplete(string messageDictionaryStr)
			{
			}

			public void OnDDNALogEventCalled(string message)
			{
			}

			public void OnGotGeoCode(string message)
			{
			}

			public void OnReceivedInstallationId(string message)
			{
			}

			public void MidSessionConfigReady(string messageDictionaryStr)
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CCallDecisionPointResponseAction_003Ed__62 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public string decisionPoint;

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
			public _003CCallDecisionPointResponseAction_003Ed__62(int _003C_003E1__state)
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
		private sealed class _003CCallGetValueDicResponseAction_003Ed__52 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public IList<string> keys;

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
			public _003CCallGetValueDicResponseAction_003Ed__52(int _003C_003E1__state)
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
		private sealed class _003CCallGetValueResponseAction_003Ed__50 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public string key;

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
			public _003CCallGetValueResponseAction_003Ed__50(int _003C_003E1__state)
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
		private static Action<bool> m_OnDeltaDnaReadyEvent;

		[CompilerGenerated]
		private static Action<bool> m_OnRemoteValueProviderReadyEvent;

		[CompilerGenerated]
		private static Action<bool> m_OnRemoteFetchCompletedEvent;

		[CompilerGenerated]
		private static Action<string> m_OnRemoteConfigReadyEvent;

		private static Action<string> _midSessionRemoteConfigUpdate;

		[CompilerGenerated]
		private static Action<string, bool, string> m_OnDidFetchCompleteForImageMessage;

		[CompilerGenerated]
		private static Action<string> m_OnDismissImageMessage;

		[CompilerGenerated]
		private static Action<string> m_OnRecievedFirebaseInstallationIdEvent;

		[CompilerGenerated]
		private static Action<string, IDictionary<string, object>> m_OnDDNALogEvent;

		[CompilerGenerated]
		private static Action<string, string, IDictionary<string, object>> m_OnActionImageMessage;

		private static Action<string, Dictionary<string, object>> _onDecisionPointResponseAction;

		private static Action<string> _onRequestValueResponseAction;

		private static Action<IDictionary<string, object>> _onRequestValueDictionaryResponseAction;

		private static GetUserScoreDelegate _getUserScoreDelegate;

		private static Dictionary<string, object> _overrideConfig;

		private static Action<string> _getGeoCallback;

		private const int MAX_ADDITIONAL_PARAMS = 10;

		private static IAnalytics _impl;

		private static IAnalytics Impl => null;

		public static event Action<bool> OnDeltaDnaReadyEvent
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public static event Action<bool> OnRemoteValueProviderReadyEvent
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		[Obsolete("Please use OnRemoteConfigReadyEvent instead.")]
		public static event Action<bool> OnRemoteFetchCompletedEvent
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public static event Action<string> OnRemoteConfigReadyEvent
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public static event Action<string> MidSessionRemoteConfigUpdate
		{
			add
			{
			}
			remove
			{
			}
		}

		public static event Action<string, bool, string> OnDidFetchCompleteForImageMessage
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public static event Action<string> OnDismissImageMessage
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public static event Action<string> OnRecievedFirebaseInstallationIdEvent
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public static event Action<string, IDictionary<string, object>> OnDDNALogEvent
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public static event Action<string, string, IDictionary<string, object>> OnActionImageMessage
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public static void FetchMidSession(List<string> keys)
		{
		}

		public static string GetStringValue(string key)
		{
			return null;
		}

		public static void SetCustomSegmentNameUserProperty(string value)
		{
		}

		public static void SetHardCurrencyUserProperty(string value)
		{
		}

		public static void SetSoftCurrencyUserProperty(string value)
		{
		}

		public static void SetUserProperties(Dictionary<string, string> properties)
		{
		}

		public static void RestoreUserProperties(string accountId, Dictionary<string, string> properties)
		{
		}

		private static void TtpSetUserProperties(Dictionary<string, string> properties)
		{
		}

		public static bool IsRemoteFetchComplete()
		{
			return false;
		}

		private static void NotifyOnRemoteFetchCompletedEvent(bool fetchSucceeded)
		{
		}

		[Obsolete("Do not use this method. Use GetStringValue() instead.")]
		public static bool GetRemoteValue(string key, Action<string> onRequestValueResponseAction, double timeout = 4.0)
		{
			return false;
		}

		private static string TTPUserPropertiesToJsonString(Dictionary<string, string> properties)
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CCallGetValueResponseAction_003Ed__50))]
		private static IEnumerator CallGetValueResponseAction(string key)
		{
			return null;
		}

		[Preserve]
		[Obsolete("Do not use this method. Use GetStringValue() instead.")]
		public static bool GetRemoteValueDictionary(IList<string> keys, Action<IDictionary<string, object>> onRequestValueDictionaryResponseAction, double timeout = 4.0)
		{
			return false;
		}

		[IteratorStateMachine(typeof(_003CCallGetValueDicResponseAction_003Ed__52))]
		private static IEnumerator CallGetValueDicResponseAction(IList<string> keys)
		{
			return null;
		}

		public static void SetGetUserScoreDelegate(GetUserScoreDelegate getUserScoreDelegate)
		{
		}

		[Preserve]
		public static void LogEvent(string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal = false)
		{
		}

		[Obsolete("Use method LogEvent(string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal = false) instead")]
		[Preserve]
		public static void LogEvent(long targets, string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal = false)
		{
		}

		public static void EndLogEvent(string eventName, IDictionary<string, object> eventParams)
		{
		}

		public static bool CallDecisionPoint(string decisionPoint, Dictionary<string, object> parameters, Action<string, Dictionary<string, object>> onDecisionPointResponseAction, double timeout = 4.0)
		{
			return false;
		}

		public static void GetGeoCodeWhenReady(Action<string> callback)
		{
		}

		public static Dictionary<string, object> GetAdditionalParams()
		{
			return null;
		}

		public static Dictionary<string, string> GetUserProperties()
		{
			return null;
		}

		public static bool ShowImageMessage(string decisionPoint)
		{
			return false;
		}

		[IteratorStateMachine(typeof(_003CCallDecisionPointResponseAction_003Ed__62))]
		private static IEnumerator CallDecisionPointResponseAction(string decisionPoint)
		{
			return null;
		}

		private static void AddAdditionalParams(IDictionary<string, object> eventParams, Dictionary<string, object> additionalParams, string eventName)
		{
		}

		[Preserve]
		public static void ReportShopEntered(string shopSource, string shopId, string shopName, Dictionary<string, object> additionalParams = null)
		{
		}

		[Preserve]
		public static void TutorialStep(int missionID, bool isMandatory, int tutorialStepID, string tutorialName, string tutorialStepName, string stepStatus, Dictionary<string, object> additionalParams = null)
		{
		}

		[Preserve]
		public static void LoadingTime(int loadingTime, int gameLoadingTime, Dictionary<string, object> additionalParams = null)
		{
		}

		[Preserve]
		public static void GamePopup(string popupName, string popupResult, string popupOpenType, string popupLocation, string notEnoughResources, Dictionary<string, object> additionalParams = null)
		{
		}

		[Preserve]
		public static void SaveState(string action, string saveUuid, string userProgressID)
		{
		}

		[Preserve]
		public static void SaveStateError(string action, string saveUuid, string errorMessage, string userProgressID)
		{
		}

		[Preserve]
		public static void ForceUpdateResponse(string updateRequirement, string updateType, int cooldownHours, bool applySegmentation, bool enableForceUpdate, bool cached, Dictionary<string, object> additionalParams = null)
		{
		}

		[Preserve]
		public static void ForceUpdatePopup(string updateType, int cooldownHours, string userChoice, Dictionary<string, object> additionalParams = null)
		{
		}

		[Preserve]
		public static void GameSessionStarted(string activeLiveopsEvents, string virtualCurrencyBalance, Dictionary<string, object> additionalParams = null)
		{
		}

		[Preserve]
		public static void ReportVirtualCurrencyTransaction(string buttonID, string buttonName, string location, string virtualTransactionType, int virtualCurrencyAmount, int virtualCurrencyBalance, string virtualCurrencyName, string virtualTransactionReason)
		{
		}

		[Preserve]
		public static void ReportVirtualCurrencyDelta(List<string> virtualCurrencyName, List<double> virtualCurrencyAmount, int virtualCurrencyBalance)
		{
		}

		[Preserve]
		public static void LogTransaction(string transactionName, IDictionary<string, object> productsReceived, IDictionary<string, object> productsSpent, IDictionary<string, object> otherEventParams)
		{
		}

		[Preserve]
		private static string GetInstanceIdFirebase()
		{
			return null;
		}

		[Preserve]
		private static Dictionary<string, object> OverrideCurrentConfig()
		{
			return null;
		}

		[Preserve]
		private static void OverrideParameter(string key, object val)
		{
		}

		[Preserve]
		private static void LevelUp(string skinName, string levelUpName, int level, Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void LevelUpFirebase(int level, Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void MissionStarted(string id, string name, string type, string missionStartedType, Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void MissionStartedFirebase(int id, Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void MissionComplete(string eventName, Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void LogEventToFirebase(string eventName, Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void MissionCompleteFirebase(string eventName, Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void TutorialStep(bool isMandatory, int tutorialStepID, string tutorialName, string tutorialStepName, Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void ReachedMainScreen(Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void ExcludeFromABTest(string decisionPoint)
		{
		}

		[Preserve]
		private static void AttDidShow(Dictionary<string, object> additionalParams)
		{
		}

		[Preserve]
		private static void TriggerOnDeltaDnaReady(bool isReady, string userId)
		{
		}

		[Preserve]
		private static void OnDecisionPointResponse(string decisionPoint, Dictionary<string, object> parameters)
		{
		}

		[Preserve]
		private static void onImageMessageFetchCompleted(string decisionPoint, bool success, string error)
		{
		}

		[Preserve]
		private static void onImageMessageDismissed(string decisionPoint)
		{
		}

		[Preserve]
		private static void onImageMessageAction(string decisionPoint, string action, Dictionary<string, object> parameters)
		{
		}
	}
}
