using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	public class TTPPrivacySettings
	{
		public enum AudienceMode
		{
			NON_CHILDREN = 0,
			CHILDREN = 1,
			MIXED_NON_CHILDREN = 2,
			MIXED_CHILDREN = 3,
			MIXED_UNKNOWN = 4
		}

		private interface IPrivacySettings
		{
			void ForgetUser();

			void ShowPrivacySettings();

			void SetAge(int age);

			bool ShouldShowAgeGate();

			bool IsCcpaJurisdiction();

			bool ShouldShowPrivacySettings();
		}

		private class EditorImpl : IPrivacySettings
		{
			[Serializable]
			private class PrivacySettingsData
			{
				public string privacySettingsURL;

				public string audienceMode;
			}

			private const string PLAYER_PREFS_AGE = "ttpPrivacySettingsAge";

			private int _age;

			private AudienceMode _audienceMode;

			private GameObject _privacySettingsCanvas;

			public void ForgetUser()
			{
			}

			public void ShowPrivacySettings()
			{
			}

			public bool IsPrivacyWebViewDisplayed()
			{
				return false;
			}

			public void SetAge(int age)
			{
			}

			public bool ShouldShowAgeGate()
			{
				return false;
			}

			public bool IsCcpaJurisdiction()
			{
				return false;
			}

			public bool ShouldShowPrivacySettings()
			{
				return false;
			}
		}

		private class EmptyImpl : IPrivacySettings
		{
			public void ForgetUser()
			{
			}

			public void ShowPrivacySettings()
			{
			}

			public void SetAge(int age)
			{
			}

			public bool ShouldShowAgeGate()
			{
				return false;
			}

			public bool IsCcpaJurisdiction()
			{
				return false;
			}

			public bool ShouldShowPrivacySettings()
			{
				return false;
			}
		}

		[Preserve]
		public class PrivacySettingsDelegate : MonoBehaviour
		{
			public void OnPrivacySettingsPopUpClosed()
			{
			}

			public void OnForgetMe()
			{
			}
		}

		[CompilerGenerated]
		private static Action m_OnForgetMeEvent;

		private static Action _onPrivacySettingsClosedAction;

		private static IPrivacySettings _impl;

		private static IPrivacySettings Impl => null;

		public static event Action OnForgetMeEvent
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

		public static void ShowPrivacySettings(Action action)
		{
		}

		public static bool ShouldShowPrivacySettings()
		{
			return false;
		}

		public static void ForgetUser()
		{
		}

		public static void SetAge(int age)
		{
		}

		public static bool ShouldShowAgeGate()
		{
			return false;
		}

		public static bool IsCcpaJurisdiction()
		{
			return false;
		}

		private static AudienceMode StringToAudienceMode(string audeinceModeStr)
		{
			return AudienceMode.NON_CHILDREN;
		}
	}
}
