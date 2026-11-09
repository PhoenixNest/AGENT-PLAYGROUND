using System;
using System.Collections.Generic;
using Tabtale.TTPlugins.SimpleJSON;

namespace Tabtale.TTPlugins
{
	public sealed class CLIKInfoGameProgressionManager
	{
		[Serializable]
		private struct MissionEntry
		{
			public int missionId;

			public int date;
		}

		[Serializable]
		private class MissionEntryList
		{
			public List<MissionEntry> missions;
		}

		private static readonly Lazy<CLIKInfoGameProgressionManager> _instance;

		private const string CLIK_INFO_CURRENT_MISSION_KEY = "CLIKInfoCurrentMission";

		private const string CLIK_INFO_MISSION_IDS_KEY = "CLIKInfoCurrentMissionArray";

		private const string CLIK_INFO_LAST_SESSION_COUNT_KEY = "CLIKInfoSessionCount";

		private const string CLIK_INFO_LAST_SESSION_LAST_DAY_KEY = "CLIKInfoSessionLastDay";

		private List<MissionEntry> _missions;

		private int _sessionStartsCount;

		private int _sessionCountDay;

		private int _missionFailedStreak;

		public static CLIKInfoGameProgressionManager Instance => null;

		private CLIKInfoGameProgressionManager()
		{
		}

		private void ReadInitialData()
		{
		}

		public int GetDistinctMissionIDs(int timePeriodInDays)
		{
			return 0;
		}

		public int CountSessionStartsToday()
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

		public void TrackMissionStarted(int missionID)
		{
		}

		public void TrackMissionFailed()
		{
		}

		public void TrackMissionComplete()
		{
		}

		public Dictionary<string, object> GetSavableCLIKInfo()
		{
			return null;
		}

		public void SetSavableCLIKInfo(JSONObject jsonData)
		{
		}

		private void SaveMissionsData()
		{
		}

		private int IntDateDaysAgo(int daysOffset)
		{
			return 0;
		}

		public void OnNewSessionStarted()
		{
		}
	}
}
