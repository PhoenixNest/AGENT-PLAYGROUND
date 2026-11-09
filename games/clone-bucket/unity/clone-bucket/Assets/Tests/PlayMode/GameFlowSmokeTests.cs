using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GameFlowSmokeTests
{
	private const float SceneTimeout = 60f;

	private const float FlowTimeout = 20f;

	private const int Level1StartBalls = 10;

	private const int Level1Target = 60;

	private const int Level1Perfect = 110;

	private const int GemsPerMiniLevel = 50;

	private const string KeyCurrentLevel = "current_level";

	private const string KeyGemCounter = "gem_counter";

	private const BindingFlags AnyMember = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
		| BindingFlags.Instance | BindingFlags.FlattenHierarchy;

	private static readonly Type LevelsControllerType = GameType("Levels.LevelsController");

	private static readonly Type MiniLevelViewType = GameType("Levels.MiniLevelView");

	private static readonly Type BallsPoolType = GameType("Balls.BallsPool");

	private static readonly Type BallsControllerType = GameType("Balls.BallsController");

	private static readonly Type GameManagerType = GameType("GameManager");

	[SetUp]
	public void SetUp()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	[TearDown]
	public void TearDown()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	[UnityTest]
	public IEnumerator Boot_SpawnsLevelOneWithStartBallsAndGateLabels()
	{
		yield return BootToMain();

		Assert.AreEqual(1, (int)Member(GameManagerType, Instance(), "CurrentLevel"), "A fresh save starts at level 1 (the scene value).");
		Assert.AreEqual("StartLevel1", FirstMiniLevelName(), "Level 1 is the first live row.");

		object view = Member(LevelsControllerType, null, "MiniLevelView");
		Assert.IsNotNull(view, "The level-1 mini-level view is spawned.");
		StringAssert.Contains("StartLevel1", ((Component)view).gameObject.name, "The level-1 prefab is instantiated.");

		Assert.AreEqual(Level1StartBalls, TotalBallCount(), "Start balls come from the table row.");
		Assert.AreEqual(Level1StartBalls, (int)Member(BallsPoolType, null, "ActiveCount"), "Exactly the start balls are spawned.");
		Assert.AreEqual(Level1Target, Call(LevelsControllerType, null, "GetTargetBallAmount"), "Target from the table row.");
		Assert.AreEqual(Level1Perfect, Call(LevelsControllerType, null, "GetPerfectBallAmount"), "Perfect from the table row.");

		Component[] gates = GateComponents(view);
		Assert.AreEqual(2, gates.Length, "Two gates, as in the row (Multiply;Multiply).");
		Assert.AreEqual("X11", GateLabel(gates[0]), "Gate 1 shows the first Gate Values entry (11).");
		Assert.AreEqual("X6", GateLabel(gates[1]), "Gate 2 shows the second Gate Values entry (6).");
	}

	[UnityTest]
	public IEnumerator Win_AdvancesToLevelTwoAndSavesProgressAndGems()
	{
		yield return BootToMain();

		int gemsBefore = (int)Member(GameManagerType, Instance(), "GemCounter");
		FillBucketUntilTargetReached();

		yield return WaitFor(() => (bool)Member(GameManagerType, Instance(), "LevelCompleted"), FlowTimeout,
			"The win does not complete the level.");
		object completeScreen = Member(GameManagerType, Instance(), "completeLevelScreen");
		Transform endBlackCard = ((Component)completeScreen).transform.parent.Find("EndBlack");
		Assert.IsNotNull(endBlackCard, "The EndBlack card sits next to the complete screen.");
		Assert.IsTrue(endBlackCard.gameObject.activeSelf, "The EndBlack card shows for the 2 s between the win and the complete screen.");
		yield return WaitFor(() => ((Component)completeScreen).gameObject.activeSelf, FlowTimeout,
			"The complete screen does not open.");
		object gameScreen = Member(GameManagerType, Instance(), "gameScreen");
		Assert.IsFalse(((Component)gameScreen).gameObject.activeSelf, "The game screen closes under the complete screen.");
		GameObject nextButton = (GameObject)Member(completeScreen.GetType(), completeScreen, "NextLevelBtn");
		yield return WaitFor(() => nextButton.activeSelf, FlowTimeout,
			"The Next button does not appear after the leaderboard climb.");
		Assert.IsFalse(endBlackCard.gameObject.activeSelf, "The EndBlack card closes when the complete screen opens.");

		Call(completeScreen.GetType(), completeScreen, "OnClickNextLevel");

		yield return WaitFor(() => (int)Member(GameManagerType, Instance(), "CurrentLevel") == 2, FlowTimeout,
			"The level does not advance to 2.");
		Assert.AreEqual(2, PlayerPrefs.GetInt(KeyCurrentLevel), "current_level is saved after the win.");

		yield return WaitFor(() => (int)Member(GameManagerType, Instance(), "GemCounter") == gemsBefore + GemsPerMiniLevel,
			FlowTimeout, "The completed level does not pay 50 gems.");
		Assert.AreEqual((int)Member(GameManagerType, Instance(), "GemCounter"), PlayerPrefs.GetInt(KeyGemCounter),
			"gem_counter is saved with the payout.");

		Assert.AreEqual("StartLevel2", FirstMiniLevelName(), "Level 2 is spawned.");
		Assert.AreEqual(10, TotalBallCount(), "Level 2 starts with its table start balls.");
	}

	[UnityTest]
	public IEnumerator Fail_OpensWindowAfterOneSecondAndRestartReloadsTheLevel()
	{
		yield return BootToMain();

		object firstView = Member(LevelsControllerType, null, "MiniLevelView");
		object failScreen = Member(GameManagerType, Instance(), "failLevelScreen");
		float failedAt = Time.realtimeSinceStartup;
		Call(LevelsControllerType, null, "HandleMissionFail");

		yield return new WaitForSecondsRealtime(0.3f);
		Assert.IsFalse(((Component)failScreen).gameObject.activeSelf, "The fail window does not open at once.");

		yield return WaitFor(() => ((Component)failScreen).gameObject.activeSelf, FlowTimeout, "The fail window does not open.");
		float openedAfter = Time.realtimeSinceStartup - failedAt;
		Assert.That(openedAfter, Is.InRange(0.9f, 2.5f), "The fail window opens about 1 s after the fail.");

		Assert.AreEqual(1, PlayerPrefs.GetInt(KeyCurrentLevel), "The failed level is saved, not advanced.");

		Call(GameManagerType, Instance(), "RestartGame");

		Assert.IsFalse(((Component)failScreen).gameObject.activeSelf, "Restart closes the fail window.");
		Assert.AreEqual(1, (int)Member(GameManagerType, Instance(), "CurrentLevel"), "Restart keeps the same level.");
		Assert.AreEqual("StartLevel1", FirstMiniLevelName(), "The same level is spawned again.");
		Assert.AreNotSame(firstView, Member(LevelsControllerType, null, "MiniLevelView"), "The level prefab is instantiated again.");
		Assert.AreEqual(0, (int)Member(LevelsControllerType, null, "TotalFillCount"), "Fill is reset on restart.");
		Assert.AreEqual(Level1StartBalls, TotalBallCount(), "Restart spawns the start balls again.");
	}

	private static IEnumerator BootToMain()
	{
		var persisted = Member(BallsControllerType, null, "_instance") as UnityEngine.Object;
		SceneManager.LoadScene(persisted != null ? "Main" : "Loading");

		yield return WaitFor(() => SceneManager.GetActiveScene().name == "Main", SceneTimeout, "Main did not load.");
		yield return WaitFor(LevelReady, FlowTimeout, "Level 1 did not spawn.");
	}

	private static bool LevelReady()
	{
		return Instance() != null
			&& Member(LevelsControllerType, null, "MiniLevelView") != null
			&& FirstMiniLevelName() != null;
	}

	private static void FillBucketUntilTargetReached()
	{
		int target = (int)Call(LevelsControllerType, null, "GetTargetBallAmount");
		while ((int)Member(LevelsControllerType, null, "TotalFillCount") < target)
		{
			int fill = (int)Member(LevelsControllerType, null, "TotalFillCount");
			if (fill + 1 == target)
			{
				Call(LevelsControllerType, null, "SubtractTotal", TotalBallCount() - target);
			}
			else
			{
				Call(LevelsControllerType, null, "AddTotal");
			}

			Call(LevelsControllerType, null, "OnFillBucket", false, 1);
		}
	}

	private static object Instance()
	{
		return Member(GameManagerType, null, "instance");
	}

	private static int TotalBallCount()
	{
		return (int)Member(LevelsControllerType, null, "TotalBallCountInMiniLevel");
	}

	private static string FirstMiniLevelName()
	{
		object data = Member(LevelsControllerType, null, "CurrentLevelData");
		object names = Member(data.GetType(), data, "miniLevelNames");
		if (names == null)
		{
			return null;
		}
		var list = (List<string>)names;
		return list.Count > 0 ? list[0] : null;
	}

	private static Component[] GateComponents(object view)
	{
		object[] gates = (object[])Member(MiniLevelViewType, view, "allGates");
		var result = new Component[gates.Length];
		for (int i = 0; i < gates.Length; i++)
		{
			result[i] = (Component)gates[i];
		}
		return result;
	}

	private static string GateLabel(Component gate)
	{
		TMP_Text label = gate.GetComponentInChildren<TMP_Text>(true);
		Assert.IsNotNull(label, "The gate has a label.");
		return label.text;
	}

	private static IEnumerator WaitFor(Func<bool> condition, float timeoutSeconds, string failMessage)
	{
		float deadline = Time.realtimeSinceStartup + timeoutSeconds;
		while (!condition())
		{
			if (Time.realtimeSinceStartup > deadline)
			{
				Assert.Fail(failMessage);
			}

			yield return null;
		}
	}

	private static Type GameType(string fullName)
	{
		Type type = Type.GetType(fullName + ", Assembly-CSharp");
		if (type == null)
		{
			throw new TypeLoadException("Game type not found: " + fullName);
		}
		return type;
	}

	private static object Member(Type type, object target, string name)
	{
		PropertyInfo property = type.GetProperty(name, AnyMember);
		if (property != null)
		{
			return property.GetValue(target);
		}

		FieldInfo field = type.GetField(name, AnyMember);
		if (field != null)
		{
			return field.GetValue(target);
		}

		throw new MissingMemberException(type.FullName, name);
	}

	private static object Call(Type type, object target, string name, params object[] args)
	{
		foreach (MethodInfo method in type.GetMethods(AnyMember))
		{
			if (method.Name != name || method.GetParameters().Length != args.Length)
			{
				continue;
			}

			try
			{
				return method.Invoke(target, args);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException ?? ex;
			}
		}

		throw new MissingMethodException(type.FullName, name);
	}
}
