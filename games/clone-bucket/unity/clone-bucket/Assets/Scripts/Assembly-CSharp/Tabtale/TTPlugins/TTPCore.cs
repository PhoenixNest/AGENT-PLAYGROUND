using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	public class TTPCore
	{
		private interface ITTPCore
		{
			void Setup();

			bool OnBackPressed();

			bool IsConnectedToTheInternet();

			bool IsRemoteConfigExistAndEnabled();

			long GetSessionNumber();

			string GetEventFromLog(string agent, string eventName);

			void WriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON);

			void CheckInternetConnection();
		}

		public interface ITTPInternalService
		{
		}

		public interface ITTPCoreInternal
		{
			string GetPackageInfo();

			string GetConfigurationJson(string serviceName);
		}

		private class EditorImpl : ITTPCore, ITTPInternalService, ITTPCoreInternal
		{
			public void Setup()
			{
			}

			public bool OnBackPressed()
			{
				return false;
			}

			public string GetPackageInfo()
			{
				return null;
			}

			public string GetConfigurationJson(string serviceName)
			{
				return null;
			}

			public bool IsConnectedToTheInternet()
			{
				return false;
			}

			public void CheckInternetConnection()
			{
			}

			public bool IsRemoteConfigExistAndEnabled()
			{
				return false;
			}

			public long GetSessionNumber()
			{
				return 0L;
			}

			public string GetEventFromLog(string agent, string eventName)
			{
				return null;
			}

			public void WriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON)
			{
			}
		}

		public class TTPGameObject : MonoBehaviour
		{
			[CompilerGenerated]
			private Action m_OnApplicationFocusEvent;

			[CompilerGenerated]
			private Action m_OnApplicationPauseEvent;

			public event Action OnApplicationFocusEvent
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

			public event Action OnApplicationPauseEvent
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

			private void Start()
			{
			}

			private void OnApplicationFocus(bool focus)
			{
			}

			private void OnApplicationPause(bool pause)
			{
			}

			private void OnDestroy()
			{
			}
		}

		public class CoreDelegate : MonoBehaviour
		{
			public void OnNewTTPSession(string message)
			{
			}

			public void OnRemoteConfigUpdate(string message)
			{
			}

			public void OnPopupShown(string message)
			{
			}

			public void OnPopupClosed(string message)
			{
			}

			public void OnConnectivityStatusChanged(string message)
			{
			}

			public void OnInternetConnectionCheckFinished(string message)
			{
			}
		}

		[CompilerGenerated]
		private static Action<bool> m_PauseGameMusicEvent;

		[CompilerGenerated]
		private static Action m_OnNewTTPSessionEvent;

		[CompilerGenerated]
		private static Action<Dictionary<string, object>> m_OnRemoteConfigUpdateEvent;

		[CompilerGenerated]
		private static Action m_OnPopupShownEvent;

		[CompilerGenerated]
		private static Action m_OnPopupClosedEvent;

		[CompilerGenerated]
		private static Action<bool> m_OnConnectivityChangedEvent;

		private static Action<bool> _onCheckInternetConnectionResultAction;

		public const string TTP_LOCAL_CONFIGURATION_PATH = "Assets/Tabtale/TTPlugins/CLIK/Resources/ttpLocalConfiguration.asset";

		private static TTPIncludedServicesScriptableObject _includedServices;

		[CompilerGenerated]
		private static Action<bool> m_OnShouldAskForIDFA;

		private static bool _initialized;

		private static string[] CLASS_LIST;

		private static ITTPInternalService _impl;

		private static GameObject _ttpGameObject;

		public static bool DevMode => false;

		public static TTPIncludedServicesScriptableObject IncludedServices
		{
			get
			{
				return null;
			}
			private set
			{
			}
		}

		public static ITTPInternalService Impl => null;

		private static GameObject TTPluginsGameObject => null;

		public static event Action<bool> PauseGameMusicEvent
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

		public static event Action OnNewTTPSessionEvent
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

		public static event Action<Dictionary<string, object>> OnRemoteConfigUpdateEvent
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

		public static event Action OnPopupShownEvent
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

		public static event Action OnPopupClosedEvent
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

		public static event Action<bool> OnConnectivityChangedEvent
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

		[Obsolete("This method is not needed anymore, ttplugins is responsible for handling it now")]
		public static event Action<bool> OnShouldAskForIDFA
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

		public static void setGeoServiceAlwaysReturnLocation(string location)
		{
		}

		public static void clearGeoServiceAlwaysReturnLocation()
		{
		}

		[Obsolete("This method is not needed anymore, ttplugins is responsible for handling it now")]
		public static void AskForIDFA()
		{
		}

		[Obsolete("This method is not needed anymore, ttplugins is responsible for handling it now")]
		public static void RefuseToAskForIDFA()
		{
		}

		public static void Setup()
		{
		}

		public static bool OnBackPressed()
		{
			return false;
		}

		public static bool IsConnectedToTheInternet()
		{
			return false;
		}

		public static void CheckInternetConnection(Action<bool> onResultAction)
		{
		}

		public static bool IsRemoteConfigExistAndEnabled()
		{
			return false;
		}

		public static long GetSessionNumber()
		{
			return 0L;
		}

		public static string GetEventFromLog(string agent, string eventName)
		{
			return null;
		}

		public static void WriteEventToEventsFileHandler(string eventName, string agent, string paramsJSON)
		{
		}

		private static void InitDeltaDnaAgent()
		{
		}

		private static void InitGameProgression()
		{
		}

		[Preserve]
		private static GameObject GetTTPGameObject()
		{
			return null;
		}
	}
}
