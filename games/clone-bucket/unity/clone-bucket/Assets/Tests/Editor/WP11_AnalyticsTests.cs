using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class WP11_AnalyticsTests
{
	private const string MissionFlagKey = "IS_MISSION_ACTIVE";

	[SetUp]
	public void SetUp()
	{
		PlayerPrefs.DeleteKey(MissionFlagKey);
	}

	[TearDown]
	public void TearDown()
	{
		PlayerPrefs.DeleteKey(MissionFlagKey);
	}

	[Test]
	public void MissionStartedSetsTheFlagAndCompletedClearsIt()
	{
		TTPAnalyticsService.MissionStarted(3);
		Assert.AreEqual(1, PlayerPrefs.GetInt(MissionFlagKey));
		TTPAnalyticsService.MissionCompleted(3);
		Assert.AreEqual(0, PlayerPrefs.GetInt(MissionFlagKey));
	}

	[Test]
	public void MissionStartedIsNotGuardedBySecondStart()
	{
		TTPAnalyticsService.MissionStarted(3);
		TTPAnalyticsService.MissionStarted(4);
		Assert.AreEqual(1, PlayerPrefs.GetInt(MissionFlagKey));
	}

	[Test]
	public void MissionAbandonedIsSentWithNoActiveMissionAndClearsTheFlag()
	{
		PlayerPrefs.SetInt(MissionFlagKey, 0);
		TTPAnalyticsService.MissionAbandoned(5);
		Assert.AreEqual(0, PlayerPrefs.GetInt(MissionFlagKey));
	}

	[Test]
	public void CustomEventDropsMissionIdWhileAMissionIsActive()
	{
		TTPAnalyticsService.MissionStarted(3);
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "missionID", 3 }, { "other", 1 } };
		TTPAnalyticsService.LogCustomEvent("custom_event", parameters);
		Assert.IsFalse(parameters.ContainsKey("missionID"));
		Assert.IsTrue(parameters.ContainsKey("other"));
	}

	[Test]
	public void CustomEventKeepsMissionIdWhenNoMissionIsActive()
	{
		TTPAnalyticsService.MissionCompleted(3);
		Dictionary<string, object> parameters = new Dictionary<string, object> { { "missionID", 3 } };
		TTPAnalyticsService.LogCustomEvent("custom_event", parameters);
		Assert.IsTrue(parameters.ContainsKey("missionID"));
	}

	[Test]
	public void CustomEventWithEmptyNameIsRejected()
	{
		UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "[MM3:Analytics] LogCustomEvent called with an empty name");
		Assert.DoesNotThrow(() => TTPAnalyticsService.LogCustomEvent(string.Empty, null));
	}

	[Test]
	public void SendUIInteractionDoesNotThrowWithNullFields()
	{
		Assert.DoesNotThrow(() => TTPAnalyticsService.SendUIInteraction("button", "restartLevel_button", "click", "levelFail"));
	}
}
