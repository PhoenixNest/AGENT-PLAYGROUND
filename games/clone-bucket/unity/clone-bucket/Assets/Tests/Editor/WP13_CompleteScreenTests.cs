using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class WP13_CompleteScreenTests
{
	private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

	private readonly List<GameObject> _objects = new List<GameObject>();

	[TearDown]
	public void TearDown()
	{
		foreach (GameObject go in _objects)
		{
			if (go != null)
			{
				Object.DestroyImmediate(go);
			}
		}
		_objects.Clear();
	}

	private GameObject Make(string name)
	{
		GameObject go = new GameObject(name);
		_objects.Add(go);
		return go;
	}

	private GameManager CreateCompleteLevelScene(bool endBlackActive, out GameObject completeObject, out GameObject endBlack)
	{
		GameObject root = Make("WP13Root");
		completeObject = Make("WP13CompleteScreen");
		completeObject.transform.SetParent(root.transform, false);
		completeObject.SetActive(false);
		CompleteLevelScreen complete = completeObject.AddComponent<CompleteLevelScreen>();
		endBlack = Make("EndBlack");
		endBlack.transform.SetParent(root.transform, false);
		endBlack.SetActive(endBlackActive);
		GameManager gm = Make("WP13GameManager").AddComponent<GameManager>();
		gm.completeLevelScreen = complete;
		return gm;
	}

	private static void CallInstance(object target, string name)
	{
		MethodInfo method = target.GetType().GetMethod(name, AnyInstance);
		Assert.IsNotNull(method, target.GetType().Name + "." + name + " not found");
		method.Invoke(target, null);
	}

	private static void SetGameManagerInstance(GameManager value)
	{
		PropertyInfo property = typeof(SingletonBehaviour<GameManager>).GetProperty("instance", AnyStatic);
		Assert.IsNotNull(property);
		property.GetSetMethod(true).Invoke(null, new object[] { value });
	}

	[Test]
	public void CompleteLevelHidesTheGameScreen()
	{
		GameManager gm = Make("WP13GameManager").AddComponent<GameManager>();
		GameObject screen = Make("WP13GameScreen");
		gm.gameScreen = screen.AddComponent<GameScreen>();
		int saved = LevelsController.TotalFillCountAcrossLevels;
		try
		{
			gm.CompleteLevel();
			Assert.IsTrue(gm.LevelCompleted);
			Assert.IsFalse(screen.activeSelf, "The game screen must close when the level completes.");
		}
		finally
		{
			LevelsController.TotalFillCountAcrossLevels = saved;
		}
	}

	[Test]
	public void CompleteLevelShowsTheEndBlackCard()
	{
		GameManager gm = CreateCompleteLevelScene(false, out _, out GameObject endBlack);
		GameObject screen = Make("WP13GameScreen");
		gm.gameScreen = screen.AddComponent<GameScreen>();
		int saved = LevelsController.TotalFillCountAcrossLevels;
		try
		{
			gm.CompleteLevel();
			Assert.IsTrue(endBlack.activeSelf, "The EndBlack card must show for the 2 s between the win and the complete screen.");
			Assert.IsFalse(screen.activeSelf, "The game screen must close when the level completes.");
		}
		finally
		{
			LevelsController.TotalFillCountAcrossLevels = saved;
		}
	}

	[Test]
	public void CompleteScreenReplacesTheEndBlackCard()
	{
		GameManager gm = CreateCompleteLevelScene(true, out GameObject completeObject, out GameObject endBlack);
		MethodInfo method = typeof(GameManager).GetMethod("ShowCompleteScreenAfter", AnyInstance);
		Assert.IsNotNull(method, "GameManager.ShowCompleteScreenAfter not found");
		IEnumerator routine = (IEnumerator)method.Invoke(gm, new object[] { 0f });
		int steps = 0;
		while (routine.MoveNext())
		{
			steps++;
			Assert.Less(steps, 5, "The complete screen coroutine should finish within a few steps.");
		}
		Assert.IsFalse(endBlack.activeSelf, "The EndBlack card must close when the complete screen opens.");
		Assert.IsTrue(completeObject.activeSelf, "The complete screen must open after the delay.");
	}

	[Test]
	public void ResetHidesTheEndBlackCard()
	{
		GameManager gm = CreateCompleteLevelScene(true, out _, out GameObject endBlack);
		LogAssert.Expect(LogType.Error, "[MM3:Levels] Spawn called before LevelsController.Awake");
		gm.Reset(false);
		Assert.IsFalse(endBlack.activeSelf, "Reset must hide the EndBlack card so a reset during the 2 s cannot leave it up.");
	}

	[Test]
	public void CompleteLevelClearsTheRestartFlag()
	{
		GameManager gm = Make("WP13GameManager").AddComponent<GameManager>();
		gm.gameRestarted = true;
		int saved = LevelsController.TotalFillCountAcrossLevels;
		try
		{
			gm.CompleteLevel();
			Assert.IsFalse(gm.gameRestarted);
		}
		finally
		{
			LevelsController.TotalFillCountAcrossLevels = saved;
		}
	}

	[Test]
	public void GameScreenDisableHidesStarAndBallPanels()
	{
		GameScreen screen = Make("WP13GameScreen").AddComponent<GameScreen>();
		GameObject stars = Make("WP13StarPanel");
		GameObject balls = Make("WP13BallsPanel");
		GameObject credit = Make("WP13BetterThan");
		screen.StarsPanel = stars;
		screen.BallsPanel = balls.AddComponent<Image>();
		screen.BetterThanCreditScore = credit.AddComponent<CreditScoreBar>();

		CallInstance(screen, "OnDisable");

		Assert.IsFalse(stars.activeSelf, "The star panel sits on another canvas and must close with the game screen.");
		Assert.IsFalse(balls.activeSelf, "The balls counter sits on another canvas and must close with the game screen.");
		Assert.IsFalse(credit.activeSelf);
	}

	[TestCase(0, true, false, false)]
	[TestCase(1, true, false, false)]
	[TestCase(2, true, true, false)]
	[TestCase(3, true, true, true)]
	public void CompleteScreenShowsStarsFromTheFilledCount(int filled, bool first, bool second, bool third)
	{
		CompleteLevelScreen screen = Make("WP13CompleteScreen").AddComponent<CompleteLevelScreen>();
		screen.FilledStar1 = Make("WP13Star1");
		screen.FilledStar2 = Make("WP13Star2");
		screen.FilledStar3 = Make("WP13Star3");
		screen.FilledStar1.SetActive(!first);
		screen.FilledStar2.SetActive(!second);
		screen.FilledStar3.SetActive(!third);
		int saved = LevelsController.FilledStarCount;
		try
		{
			LevelsController.FilledStarCount = filled;
			CallInstance(screen, "OnEnable");
			Assert.AreEqual(first, screen.FilledStar1.activeSelf, "star 1");
			Assert.AreEqual(second, screen.FilledStar2.activeSelf, "star 2");
			Assert.AreEqual(third, screen.FilledStar3.activeSelf, "star 3");
		}
		finally
		{
			LevelsController.FilledStarCount = saved;
		}
	}

	[Test]
	public void CompleteScreenKeepsTheCoinPanelVisible()
	{
		CompleteLevelScreen screen = Make("WP13CompleteScreen").AddComponent<CompleteLevelScreen>();
		screen.GemsPanel = Make("WP13GemsPanel");
		CallInstance(screen, "OnEnable");
		Assert.IsTrue(screen.GemsPanel.activeSelf);
		Assert.IsNull(typeof(CompleteLevelScreen).GetMethod("GemAnimation", AnyInstance),
			"GemAnimation belonged to the removed coins reward video and hid the coin panel.");
	}

	[Test]
	public void CompleteScreenLevelLabelUsesTheCurrentLevel()
	{
		MethodInfo label = typeof(CompleteLevelScreen).GetMethod("LevelLabel", AnyStatic);
		Assert.IsNotNull(label);
		Assert.AreEqual("Level 8", label.Invoke(null, new object[] { 8 }));
		Assert.AreEqual("Level 12", label.Invoke(null, new object[] { 12 }));
	}

	[Test]
	public void BucketFillsAreIgnoredAfterTheLevelCompletes()
	{
		GameManager previous = GameManager.instance;
		GameManager gm = Make("WP13GameManager").AddComponent<GameManager>();
		int savedFill = LevelsController.TotalFillCount;
		int savedAcross = LevelsController.TotalFillCountAcrossLevels;
		try
		{
			SetGameManagerInstance(gm);
			gm.LevelCompleted = true;
			LevelsController.TotalFillCount = 7;
			LevelsController.TotalFillCountAcrossLevels = 40;
			LevelsController.OnFillBucket(false, 5);
			Assert.AreEqual(7, LevelsController.TotalFillCount);
			Assert.AreEqual(40, LevelsController.TotalFillCountAcrossLevels,
				"A ball landing after completion must not change the score shown on the complete screen.");
		}
		finally
		{
			SetGameManagerInstance(previous);
			LevelsController.TotalFillCount = savedFill;
			LevelsController.TotalFillCountAcrossLevels = savedAcross;
		}
	}
}
