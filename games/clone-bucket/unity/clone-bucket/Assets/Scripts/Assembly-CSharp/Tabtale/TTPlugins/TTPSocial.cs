using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	public class TTPSocial
	{
		private interface ITTPSocial
		{
			void SignIn(bool force);

			void SignOut();

			bool IsSignedIn();

			bool ShouldShowSignInButton();

			void SetPlayerScore(long score, string leaderboardId);

			void ShowLeaderboard(string leaderboardId);

			void ReportProgress(string achievementId, double progress);

			void ShowAchievements();

			void ResetAllAchievements();
		}

		private class EditorImpl : ITTPSocial
		{
			public bool IsSignedIn()
			{
				return false;
			}

			public void ReportProgress(string achievementId, double progress)
			{
			}

			public void SetPlayerScore(long score, string leaderboardId)
			{
			}

			public bool ShouldShowSignInButton()
			{
				return false;
			}

			public void ShowAchievements()
			{
			}

			public void ResetAllAchievements()
			{
			}

			public void ShowLeaderboard(string leaderboardId)
			{
			}

			public void SignIn(bool force)
			{
			}

			public void SignOut()
			{
			}
		}

		[Preserve]
		public class SocialDelegate : MonoBehaviour
		{
			public void OnSocialSignInSuccess(string message)
			{
			}

			public void OnSocialSignInFail(string message)
			{
			}

			public void OnSocialSignOut(string message)
			{
			}
		}

		private static SocialNotConnectedCanvas _notConnectedCanvas;

		[CompilerGenerated]
		private static Action<bool> m__onSignIn;

		[CompilerGenerated]
		private static Action m__onSignOut;

		private static ITTPSocial _impl;

		private static ITTPSocial Impl => null;

		private static event Action<bool> _onSignIn
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

		private static event Action _onSignOut
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

		public static void SignIn(bool force, Action<bool> onSignInAction)
		{
		}

		public static void SignOut(Action onSignOutAction)
		{
		}

		public static bool IsSignedIn()
		{
			return false;
		}

		public static bool ShouldShowSignInButton()
		{
			return false;
		}

		public static void SetPlayerScore(long score, string leaderboardId)
		{
		}

		public static void ShowLeaderboard(string leaderboardId)
		{
		}

		public static void ReportProgress(string achievementId, double progress)
		{
		}

		public static void ShowAchievements()
		{
		}

		public static void ResetAllAchievements()
		{
		}

		private static void InformNoInternet()
		{
		}
	}
}
