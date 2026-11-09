using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tabtale.TTPlugins
{
	public class TTPGameProgression
	{
		public class DDNAEvents
		{
			public static void MissionStarted(string id, string name, string type, string missionStartedType, Dictionary<string, object> additionalParams)
			{
			}

			public static void MissionComplete(Dictionary<string, object> additionalParams)
			{
			}

			public static void MissionAbandoned(Dictionary<string, object> additionalParams)
			{
			}

			public static void MissionFailed(Dictionary<string, object> additionalParams)
			{
			}

			public static void LevelUp(string skinName, string levelUpName, int level, Dictionary<string, object> additionalParams)
			{
			}

			public static void MiniLevelStarted(int miniMissionID, Dictionary<string, object> additionalParams)
			{
			}

			public static void MiniLevelCompleted()
			{
			}

			public static void MiniLevelFailed()
			{
			}
		}

		public class FirebaseEvents
		{
			public static void MissionStarted(int missionID, Dictionary<string, object> additionalParams)
			{
			}

			public static void MissionComplete(Dictionary<string, object> additionalParams)
			{
			}

			public static void MissionAbandoned(Dictionary<string, object> additionalParams)
			{
			}

			public static void MissionFailed(Dictionary<string, object> additionalParams)
			{
			}

			public static void LevelUp(int level, Dictionary<string, object> additionalParams)
			{
			}
		}

		private interface ITTPGameProgression
		{
			void PopUpMgrSetLevel(int level);

			void CoreSetMissionCompletedId(string id);
		}

		private class EditorImpl : ITTPGameProgression
		{
			public void PopUpMgrSetLevel(int level)
			{
			}

			public void CoreSetMissionCompletedId(string id)
			{
			}
		}

		private static int currentLevel;

		private static string currentMissionId;

		private static string currentMissionIdNewApi;

		private const string PLAYER_PREFS_GAME_PROGRESSION_LEVEL = "ttpGameProgressionLevel";

		private const string LEVEL_PARAMETER_KEY = "level";

		private const string LEVEL_COMPLETE_EVENT_NAME = "levelComplete";

		private static Dictionary<string, object> miniMissionParams;

		private static ITTPGameProgression _impl;

		private static ITTPGameProgression Impl => null;

		private static void InternalInit()
		{
		}

		private static void UpdateCurrentLevel(int level)
		{
		}

		[Obsolete("TutorialStep is deprecated. Use the new Analytics API instead.")]
		public static void TutorialStep(bool isMandatory, int tutorialStepID, string tutorialName, string tutorialStepName, Dictionary<string, object> additionalParams)
		{
		}

		public static void ReachedMainScreen(Dictionary<string, object> additionalParams)
		{
		}

		public static void MissionStarted(string id, string name, string type, string missionStartedType, Dictionary<string, object> additionalParams)
		{
		}

		private static void SetMissionStartedLevelUserProperty(string level)
		{
		}

		public static void MissionComplete(Dictionary<string, object> additionalParams)
		{
		}

		public static void MiniLevelStarted(int miniMissionID, Dictionary<string, object> additionalParams)
		{
		}

		public static void MiniLevelCompleted()
		{
		}

		public static void MiniLevelFailed()
		{
		}

		public static void MissionAbandoned(Dictionary<string, object> additionalParams)
		{
		}

		public static void MissionFailed(Dictionary<string, object> additionalParams)
		{
		}

		public static void LevelUp(string skinName, string levelUpName, int level, Dictionary<string, object> additionalParams)
		{
		}

		private static void AddOnlineParameter(Dictionary<string, object> additionalParams)
		{
		}

		private static void CallAnalyticsByReflection(string methodName, object[] parameters)
		{
		}
	}
}
