using System;
using Tabtale.TTPlugins.SimpleJSON;

namespace Tabtale.TTPlugins
{
	public sealed class CLIKInfoService
	{
		private static readonly Lazy<CLIKInfoService> _lazy;

		public static CLIKInfoService Instance => null;

		private CLIKInfoService()
		{
		}

		public int GetTimeSincePreviousAppStart()
		{
			return 0;
		}

		public int GetTimeSinceFirstAppStart()
		{
			return 0;
		}

		public int CountSessionStartsToday()
		{
			return 0;
		}

		public int GetDistinctMissionIDs(int timePeriodInDays)
		{
			return 0;
		}

		public int GetCurrentMissionID()
		{
			return 0;
		}

		public int GetMissionFailedStreak()
		{
			return 0;
		}

		public string GetSavableCLIKInfo()
		{
			return null;
		}

		public void SetSavableCLIKInfo(string accountId, string jsonData)
		{
		}

		private JSONNode ConvertToJsonNode(object value)
		{
			return null;
		}
	}
}
