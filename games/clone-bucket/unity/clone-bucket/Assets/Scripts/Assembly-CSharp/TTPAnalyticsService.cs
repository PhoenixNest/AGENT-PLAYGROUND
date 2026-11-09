using System.Collections.Generic;
using UnityEngine;
#if CLIK_SDK
using Tabtale.TTPlugins;
#endif

public class TTPAnalyticsService
{
	private const string IS_MISSION_ACTIVE = "IS_MISSION_ACTIVE";

	private const string MISSION_ID_KEY = "missionID";

	private static readonly Dictionary<string, object> sharedParameters = new Dictionary<string, object>();

	private static bool isMissionActive
	{
		get
		{
			return PlayerPrefs.GetInt(IS_MISSION_ACTIVE, 0) != 0;
		}
		set
		{
			PlayerPrefs.SetInt(IS_MISSION_ACTIVE, value ? 1 : 0);
		}
	}

	public static void MissionStarted(int missionId, Dictionary<string, object> additionalParams = null)
	{
		isMissionActive = true;
		SendMission("missionStarted", missionId, additionalParams);
	}

	public static void MissionCompleted(int missionId, Dictionary<string, object> additionalParams = null)
	{
		isMissionActive = false;
		SendMission("missionCompleted", missionId, additionalParams);
	}

	public static void MissionFailed(int missionId, Dictionary<string, object> additionalParams = null)
	{
		isMissionActive = false;
		SendMission("missionFailed", missionId, additionalParams);
	}

	public static void MissionAbandoned(int missionId, Dictionary<string, object> additionalParams = null)
	{
		isMissionActive = false;
		SendMission("missionAbandoned", missionId, additionalParams);
	}

	public static void CurrencyTransaction(int missionId, Dictionary<string, object> additionalParams = null)
	{
		Dictionary<string, object> parameters = BuildSharedParameters(missionId, additionalParams);
		Send("currencyTransaction", parameters, false);
	}

	public static void SendUIInteraction(string type, string uiName, string action, string source)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "type", type },
			{ "ui_name", uiName },
			{ "action", action },
			{ "source", source }
		};
		Send("ui_interaction", parameters, false);
	}

	public static void LevelUp(int level, Dictionary<string, object> additionalParams = null)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>(additionalParams ?? new Dictionary<string, object>());
		parameters["level"] = level;
		Send("LevelUp", parameters, true);
	}

	public static void LogCustomEvent(string eventName, Dictionary<string, object> eventParams = null)
	{
		if (string.IsNullOrEmpty(eventName))
		{
			Debug.LogError("[MM3:Analytics] LogCustomEvent called with an empty name");
			return;
		}
		if (eventParams != null && eventParams.ContainsKey(MISSION_ID_KEY) && isMissionActive)
		{
			eventParams.Remove(MISSION_ID_KEY);
		}
		Send(eventName, eventParams, false);
	}

	private static void SendMission(string eventName, int missionId, Dictionary<string, object> additionalParams)
	{
		Dictionary<string, object> parameters = BuildSharedParameters(missionId, additionalParams);
		Send(eventName, parameters, true);
	}

	private static Dictionary<string, object> BuildSharedParameters(int missionId, Dictionary<string, object> additionalParams)
	{
		sharedParameters.Clear();
		sharedParameters[MISSION_ID_KEY] = missionId;
		if (additionalParams != null)
		{
			foreach (KeyValuePair<string, object> pair in additionalParams)
			{
				sharedParameters[pair.Key] = pair.Value;
			}
		}
		return new Dictionary<string, object>(sharedParameters);
	}

	private static void Send(string eventName, Dictionary<string, object> parameters, bool gameProgression)
	{
#if CLIK_SDK
		TTPAnalytics.LogEvent(eventName, parameters, false);
#endif
	}
}
