using System;
using System.Reflection;
using Levels;
using NUnit.Framework;
using UnityEngine;

public class WP8_GameFlowTests
{
	private const string KeyCurrentLevel = "current_level";
	private const string KeyGemCounter = "gem_counter";
	private const string KeyBestScore = "best_score";
	private const string KeyLeaderBoardScore = "leader_board_score";
	private const string KeySoundsEnable = "sounds_enable";
	private const string KeyHapticEnable = "haptic_enable";
	private const string KeyCurrentBall = "current_ball";

	private static readonly string[] AllKeys =
	{
		KeyCurrentLevel, KeyGemCounter, KeyBestScore, KeyLeaderBoardScore,
		KeySoundsEnable, KeyHapticEnable, KeyCurrentBall,
		"white_balls", "with_boss_levels", "has_no_obstacles",
		"is_crazy_labs_themes", "mixed_levels", "boss_level_every_five",
	};

	private GameObject _host;

	[SetUp]
	public void SetUp()
	{
		ClearKeys();
	}

	[TearDown]
	public void TearDown()
	{
		if (_host != null)
		{
			UnityEngine.Object.DestroyImmediate(_host);
			_host = null;
		}
		ClearKeys();
	}

	private static void ClearKeys()
	{
		foreach (string key in AllKeys)
		{
			PlayerPrefs.DeleteKey(key);
		}
	}

	private GameManager CreateGameManager()
	{
		_host = new GameObject("WP8GameManagerTest");
		return _host.AddComponent<GameManager>();
	}

	private static object CallPrivateStatic(Type type, string name, params object[] args)
	{
		MethodInfo method = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
		Assert.IsNotNull(method, "Missing private static helper " + name);
		return method.Invoke(null, args);
	}

	private static bool ReadPrivateBool(object target, string field)
	{
		FieldInfo info = target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(info, "Missing private field " + field);
		return (bool)info.GetValue(target);
	}

	[Test]
	public void SaveWritesTheOriginalKeysWithOriginalTypes()
	{
		GameManager gm = CreateGameManager();
		gm.GemCounter = 42;
		gm.BestScore = 9;
		gm.LeaderBoardScore = 77;
		gm.SaveData(7);

		Assert.AreEqual(7, PlayerPrefs.GetInt(KeyCurrentLevel));
		Assert.AreEqual(42, PlayerPrefs.GetInt(KeyGemCounter));
		Assert.AreEqual(9, PlayerPrefs.GetInt(KeyBestScore));
		Assert.AreEqual(77, PlayerPrefs.GetInt(KeyLeaderBoardScore));
		Assert.IsTrue(PlayerPrefs.HasKey(KeyCurrentBall));
		Assert.IsFalse(PlayerPrefs.HasKey("MM3_SAVE_V1"), "The rebuild's JSON blob must not be written.");
	}

	[Test]
	public void LoadReadsTheOriginalKeys()
	{
		PlayerPrefs.SetInt(KeyCurrentLevel, 7);
		PlayerPrefs.SetInt(KeyGemCounter, 42);
		PlayerPrefs.SetInt(KeyBestScore, 9);
		PlayerPrefs.SetInt(KeyLeaderBoardScore, 77);
		GameManager gm = CreateGameManager();

		gm.LoadData();

		Assert.AreEqual(7, gm.CurrentLevel);
		Assert.AreEqual(42, gm.GemCounter);
		Assert.AreEqual(9, gm.BestScore);
		Assert.AreEqual(77, gm.LeaderBoardScore);
	}

	[Test]
	public void AwakeDoesNotLoadTheSave()
	{
		PlayerPrefs.SetInt(KeyCurrentLevel, 7);
		GameManager gm = CreateGameManager();
		Assert.AreEqual(0, gm.CurrentLevel, "Awake must not load; Start does.");
	}

	[Test]
	public void MissingSoundAndHapticKeysMeanOn()
	{
		GameManager gm = CreateGameManager();
		gm.LoadData();
		Assert.IsTrue(ReadPrivateBool(gm, "_soundsOn"));
		Assert.IsTrue(ReadPrivateBool(gm, "_hapticsOn"));
	}

	[Test]
	public void SoundKeyZeroTurnsSoundOff()
	{
		PlayerPrefs.SetInt(KeySoundsEnable, 0);
		GameManager gm = CreateGameManager();
		gm.LoadData();
		Assert.IsFalse(ReadPrivateBool(gm, "_soundsOn"));
		Assert.IsTrue(ReadPrivateBool(gm, "_hapticsOn"));
	}

	[Test]
	public void SetSoundsEnableWritesOnlyItsKey()
	{
		GameManager gm = CreateGameManager();
		gm.SetSoundsEnable(false);
		Assert.AreEqual(0, PlayerPrefs.GetInt(KeySoundsEnable));
		Assert.IsFalse(PlayerPrefs.HasKey(KeyCurrentLevel), "The settings setter must not run a full save.");
	}

	[Test]
	public void StandardBallNameIsKept()
	{
		PlayerPrefs.SetString(KeyCurrentBall, "Ball3");
		GameManager gm = CreateGameManager();
		gm.LoadData();
		Assert.AreEqual("Ball3", PlayerPrefs.GetString(KeyCurrentBall));
	}

	[Test]
	public void BallNameOutsideBall1ToBall6FallsBackToDefault()
	{
		PlayerPrefs.SetString(KeyCurrentBall, "Cat");
		GameManager gm = CreateGameManager();
		gm.LoadData();
		Assert.AreEqual("Ball1", PlayerPrefs.GetString(KeyCurrentBall));
	}

	[Test]
	public void HandleBeforeSpendDoesNotDeductGems()
	{
		GameManager gm = CreateGameManager();
		gm.GemCounter = 500;
		gm.HandleBeforeSpend(100);
		Assert.AreEqual(500, gm.GemCounter, "The deduction is in ShopScreen (R5); HandleBeforeSpend only logs the spend.");
	}

	[Test]
	public void GemCollectAddsWithoutAGuard()
	{
		GameManager gm = CreateGameManager();
		gm.GemCounter = 10;
		gm.GemCollect(-5);
		Assert.AreEqual(5, gm.GemCounter, "The original has no value <= 0 guard.");
		Assert.AreEqual(5, PlayerPrefs.GetInt(KeyGemCounter));
	}

	[Test]
	public void GemRewardIs50PerMiniLevel()
	{
		Type type = typeof(MainScreen);
		Assert.AreEqual(150, (int)CallPrivateStatic(type, "GemReward", 3));
		Assert.AreEqual(0, (int)CallPrivateStatic(type, "GemReward", 0));
	}

	[Test]
	public void LevelRewardScoreIsTheTotalFillCountAcrossLevels()
	{
		int saved = LevelsController.TotalFillCountAcrossLevels;
		try
		{
			LevelsController.TotalFillCountAcrossLevels = 37;
			Assert.AreEqual(37, (int)CallPrivateStatic(typeof(GameManager), "LevelRewardScore"));
		}
		finally
		{
			LevelsController.TotalFillCountAcrossLevels = saved;
		}
	}

	[Test]
	public void RestartPathReachesResetAndSpawn()
	{
		GameManager gm = CreateGameManager();
		gm.GameStarted = false;
		UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "[MM3:Levels] Spawn called before LevelsController.Awake");
		gm.RestartGame();
		Assert.IsFalse(gm.GameStarted, "RestartGame returns to the start screen; the player taps to start, as in the original.");
		Assert.IsTrue(gm.gameRestarted);
	}

	[Test]
	public void ResetCancelsAPendingFailWindow()
	{
		GameManager gm = CreateGameManager();
		gm.FailLevel();
		FieldInfo pending = typeof(GameManager).GetField("_pendingWindow", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.IsNotNull(pending);
		Assert.IsNotNull(pending.GetValue(gm), "The fail window should be pending before the reset.");
		UnityEngine.TestTools.LogAssert.Expect(LogType.Error, "[MM3:Levels] Spawn called before LevelsController.Awake");
		gm.Reset(false);
		Assert.IsNull(pending.GetValue(gm), "Reset must cancel the pending fail window.");
	}

	[Test]
	public void CompleteLevelAddsRewardToLeaderboardAndBestScore()
	{
		GameObject host = new GameObject("WP8CompleteTest");
		try
		{
			GameManager gm = host.AddComponent<GameManager>();
			int saved = LevelsController.TotalFillCountAcrossLevels;
			try
			{
				LevelsController.TotalFillCountAcrossLevels = 20;
				gm.LeaderBoardScore = 100;
				gm.BestScore = 50;
				gm.CompleteLevel();
				Assert.AreEqual(100, gm.LastLeaderBoardScore);
				Assert.AreEqual(120, gm.LeaderBoardScore);
				Assert.AreEqual(50, gm.BestScore, "Best score keeps the higher value (50 > 20).");
			}
			finally
			{
				LevelsController.TotalFillCountAcrossLevels = saved;
			}
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(host);
		}
	}

	[Test]
	public void FailScreenHasNoContinueOptionAndKeepsRestart()
	{
		Type type = typeof(FailLevelScreen);
		const BindingFlags any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		Assert.IsNull(type.GetField("continueButton", any), "The continue button was removed.");
		Assert.IsNull(type.GetMethod("OnContinueClick", any), "The continue handler was removed.");
		Assert.IsNotNull(type.GetMethod("OnClickRestartLevel", any), "Restart stays on the fail screen.");
	}

	[Test]
	public void LoadDeletesObsoleteSaveKeys()
	{
		string[] retired = { "has_no_rv", "ttpStoredPurchases", "ttpInAppPurchases", "ttpNoAsdPurchased", "ttpLastStoredPurchaseDate", "ttpFirstPurchaseValue", "ttpPrivacySettingsConsent" };
		foreach (string key in retired)
		{
			PlayerPrefs.SetInt(key, 1);
		}
		GameManager gm = CreateGameManager();

		gm.LoadData();

		foreach (string key in retired)
		{
			Assert.IsFalse(PlayerPrefs.HasKey(key), key + " should be deleted when the save loads.");
		}
	}

	[Test]
	public void PortraitFramingIsKeptAtReferenceAspect()
	{
		Type type = typeof(GameManager);
		float reference = 9f / 16f;
		float fov = (float)CallPrivateStatic(type, "FitVerticalFov", 60f, reference, reference);
		Assert.AreEqual(60f, fov, 0.01f);

		float wider = (float)CallPrivateStatic(type, "FitVerticalFov", 60f, reference, 2f * reference);
		Assert.Less(wider, 60f);
	}
}
