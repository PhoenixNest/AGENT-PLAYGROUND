using System;
using UnityEngine.Scripting;

namespace Tabtale.TTPlugins
{
	public class TTPRateUs
	{
		[Serializable]
		private class RateUsConfiguration
		{
			public string iconUrl;
		}

		private const string PLAYER_PREFS_NEVER = "TTPRateUsNeverShow";

		private const string PLAYER_PREFS_LATER = "TTPRateUsLaterNextTime";

		private static string _iconFileExtenstion;

		private static TTPRateUsCanvas _rateUsCanvas;

		private static TTPRateUsNotConnectedCanvas _notConnectedCanvas;

		private static string IconFileExtenstion => null;

		public static void Button()
		{
		}

		public static void Popup()
		{
		}

		private static void Later()
		{
		}

		private static void SuggestRateUs()
		{
		}

		[Preserve]
		private static void InformNoInternet()
		{
		}

		private static void GoToRateFromPopup()
		{
		}

		private static void GoToRateFromButton()
		{
		}

		private static void GoToRate()
		{
		}

		private static bool ShouldSuggestRateUs()
		{
			return false;
		}

		private static void Closed()
		{
		}

		private static void LogPopUpEvent(string eventName, bool show, bool noInternet, string response)
		{
		}
	}
}
