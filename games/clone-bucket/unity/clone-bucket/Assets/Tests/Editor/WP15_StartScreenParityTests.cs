using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class WP15_StartScreenParityTests
{
	private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

	private const string SpawnError = "[MM3:Levels] Spawn called before LevelsController.Awake";

	private readonly List<GameObject> _objects = new List<GameObject>();

	private GameManager _previousInstance;

	private sealed class Rig
	{
		public GameObject MainScreenObject;

		public MainScreen MainScreen;

		public GameObject Title;

		public GameObject Setting;

		public GameObject Shop;

		public GameObject Gems;
	}

	[SetUp]
	public void SetUp()
	{
		_previousInstance = GameManager.instance;
		SetGameManagerInstance(null);
	}

	[TearDown]
	public void TearDown()
	{
		DOTween.KillAll();
		foreach (GameObject go in _objects)
		{
			if (go != null)
			{
				Object.DestroyImmediate(go);
			}
		}
		_objects.Clear();
		SetGameManagerInstance(_previousInstance);
	}

	private GameObject Make(string name)
	{
		GameObject go = new GameObject(name);
		_objects.Add(go);
		return go;
	}

	private GameObject MakeInactive(string name)
	{
		GameObject go = Make(name);
		go.SetActive(false);
		return go;
	}

	private GameObject MakeChild(string name, Transform parent)
	{
		GameObject go = Make(name);
		go.transform.SetParent(parent, false);
		return go;
	}

	private Rig CreateMainScreen()
	{
		Rig rig = new Rig();
		rig.MainScreenObject = MakeInactive("WP15MainScreen");
		Transform root = rig.MainScreenObject.transform;
		rig.Title = MakeChild("WP15GameTitle", root);
		rig.Title.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.6f, 1f);
		rig.Setting = MakeChild("WP15SettingBtn", root);
		rig.Shop = MakeChild("WP15ShopBtn", root);
		rig.Gems = MakeChild("WP15GemsCounterImg", root);
		Image gemsImage = rig.Gems.AddComponent<Image>();
		gemsImage.color = new Color(1f, 0.9f, 0.3f, 1f);
		rig.MainScreen = rig.MainScreenObject.AddComponent<MainScreen>();
		rig.MainScreen.GameTitle = rig.Title;
		rig.MainScreen.SettingBtn = rig.Setting;
		rig.MainScreen.ShopBtn = rig.Shop;
		rig.MainScreen.GemsCounterImg = gemsImage;
		CallInstance(rig.MainScreen, "Awake");
		rig.MainScreenObject.SetActive(true);
		CallInstance(rig.MainScreen, "OnEnable");
		return rig;
	}

	private GameManager CreateManager(Rig rig, out GameScreen gameScreen)
	{
		GameObject screenObject = MakeInactive("WP15GameScreen");
		gameScreen = screenObject.AddComponent<GameScreen>();
		GameManager gm = Make("WP15GameManager").AddComponent<GameManager>();
		gm.mainScreen = rig.MainScreen;
		gm.gameScreen = gameScreen;
		return gm;
	}

	private static void CallInstance(object target, string name)
	{
		MethodInfo method = target.GetType().GetMethod(name, AnyInstance);
		Assert.IsNotNull(method, target.GetType().Name + "." + name + " not found");
		method.Invoke(target, null);
	}

	private static void Reenable(Rig rig)
	{
		CallInstance(rig.MainScreen, "OnDisable");
		rig.MainScreenObject.SetActive(false);
		rig.MainScreenObject.SetActive(true);
		CallInstance(rig.MainScreen, "OnEnable");
	}

	private static Sequence GetMainScreenSequence(MainScreen mainScreen)
	{
		FieldInfo field = typeof(MainScreen).GetField("mainScreenSequence", AnyInstance);
		Assert.IsNotNull(field, "MainScreen.mainScreenSequence not found");
		return (Sequence)field.GetValue(mainScreen);
	}

	private static void CompleteMainScreenSequence(MainScreen mainScreen, string what)
	{
		Sequence sequence = GetMainScreenSequence(mainScreen);
		Assert.IsNotNull(sequence, "The " + what + " sequence should be running.");
		sequence.Complete(true);
	}

	private static void CompleteClose(MainScreen mainScreen)
	{
		CompleteMainScreenSequence(mainScreen, "close");
	}

	private static void CompleteIntro(MainScreen mainScreen)
	{
		CompleteMainScreenSequence(mainScreen, "intro");
	}

	private static void AssertVectorNear(Vector3 expected, Vector3 actual, string message)
	{
		Assert.That(Vector3.Distance(expected, actual), Is.LessThan(0.0001f), message + " expected " + expected + " but was " + actual);
	}

	private static void AssertColorNear(Color expected, Color actual, string message)
	{
		Assert.That(Mathf.Abs(expected.r - actual.r) + Mathf.Abs(expected.g - actual.g) + Mathf.Abs(expected.b - actual.b) + Mathf.Abs(expected.a - actual.a), Is.LessThan(0.0001f), message);
	}

	private static void AssertAtRest(Rig rig, Vector3 titlePosition, Color titleColor, Color gemsColor, Vector3 shopPosition)
	{
		AssertVectorNear(titlePosition, rig.Title.transform.localPosition, "The title position must be at rest.");
		AssertVectorNear(Vector3.one, rig.Title.transform.localScale, "The title scale must be at rest.");
		AssertColorNear(titleColor, rig.Title.GetComponent<Image>().color, "The title colour must be at rest.");
		AssertVectorNear(Vector3.one, rig.Setting.transform.localScale, "The settings button scale must be at rest.");
		AssertVectorNear(shopPosition, rig.Shop.transform.localPosition, "The shop button position must be at rest.");
		AssertVectorNear(Vector3.one, rig.Shop.transform.localScale, "The shop button scale must be at rest.");
		AssertColorNear(gemsColor, rig.Gems.GetComponent<Image>().color, "The gems icon colour must be at rest.");
	}

	private static int ReadInt(object target, string field)
	{
		FieldInfo info = target.GetType().GetField(field, AnyInstance);
		Assert.IsNotNull(info, "Missing private field " + field);
		return (int)info.GetValue(target);
	}

	private static void StepBallsCounter(GameScreen gameScreen, float deltaTime)
	{
		MethodInfo method = typeof(GameScreen).GetMethod("StepBallsCounter", AnyInstance);
		Assert.IsNotNull(method, "GameScreen.StepBallsCounter not found");
		method.Invoke(gameScreen, new object[] { deltaTime });
	}

	private static void SetTotalBallCount(int value)
	{
		FieldInfo field = typeof(LevelsController).GetField("<TotalBallCountInMiniLevel>k__BackingField", AnyStatic);
		Assert.IsNotNull(field, "LevelsController.TotalBallCountInMiniLevel backing field not found");
		field.SetValue(null, value);
	}

	private static void SetGameManagerInstance(GameManager value)
	{
		PropertyInfo property = typeof(SingletonBehaviour<GameManager>).GetProperty("instance", AnyStatic);
		Assert.IsNotNull(property);
		property.GetSetMethod(true).Invoke(null, new object[] { value });
	}

	[Test]
	public void MainScreenIntroReturnsElementsToRest()
	{
		Rig rig = CreateMainScreen();
		CompleteIntro(rig.MainScreen);
		Vector3 titlePosition = rig.Title.transform.localPosition;
		Color titleColor = rig.Title.GetComponent<Image>().color;
		Color gemsColor = rig.Gems.GetComponent<Image>().color;
		Vector3 shopPosition = rig.Shop.transform.localPosition;

		AssertAtRest(rig, titlePosition, titleColor, gemsColor, shopPosition);

		rig.MainScreen.Close();
		CompleteClose(rig.MainScreen);
		Reenable(rig);
		CompleteIntro(rig.MainScreen);
		AssertAtRest(rig, titlePosition, titleColor, gemsColor, shopPosition);
	}

	[Test]
	public void MainScreenIntroUsesTheRecoveredDurations()
	{
		Rig rig = CreateMainScreen();
		Sequence intro = GetMainScreenSequence(rig.MainScreen);
		Assert.IsNotNull(intro, "OnEnable should start the intro.");
		Assert.AreEqual(0.5f, intro.Duration(false), 0.0001f, "The intro lasts 0.5 s: the shop button slide is the longest step.");
		Assert.AreEqual(Vector3.zero, rig.Setting.transform.localScale, "The settings button zooms in from zero.");
		Assert.AreEqual(0f, rig.Gems.GetComponent<Image>().color.a, 0.0001f, "The gems icon fades in from transparent.");
		Assert.AreEqual(-250f, rig.Shop.transform.localPosition.x, 0.0001f, "The shop button starts 250 units to the left of its place and slides right.");
	}

	[Test]
	public void GameTitleIsVisibleWhenTheMainScreenReopens()
	{
		Rig rig = CreateMainScreen();
		Image titleImage = rig.Title.GetComponent<Image>();

		rig.MainScreen.Close();
		CompleteClose(rig.MainScreen);
		Assert.AreEqual(Vector3.zero, rig.Title.transform.localScale, "The close zooms the title out.");

		Reenable(rig);
		CompleteIntro(rig.MainScreen);

		AssertVectorNear(Vector3.one, rig.Title.transform.localScale, "The reopened title must be at its rest scale.");
		Assert.AreEqual(1f, titleImage.color.a, 0.0001f, "The reopened title graphic must be opaque.");
	}

	[Test]
	public void TutorialShowsOnResetAndHidesOnTheFirstTap()
	{
		GameObject tutorial = MakeInactive("WP15Tutorial");
		GameManager gm = Make("WP15GameManager").AddComponent<GameManager>();
		gm.Tutorial = tutorial;
		gm.CurrentLevel = 3;

		LogAssert.Expect(LogType.Error, SpawnError);
		gm.Reset(false);
		Assert.IsTrue(tutorial.activeSelf, "Reset shows the tutorial hint on every level, not only level 1.");

		gm.StartPlay();
		Assert.IsFalse(tutorial.activeSelf, "The first tap hides the tutorial hint.");

		tutorial.SetActive(true);
		gm.StartPlay();
		Assert.IsTrue(tutorial.activeSelf, "A second tap is ignored, so it does not touch the hint.");
	}

	[Test]
	public void StartPlayCountsTheBallsCounterUp()
	{
		Rig rig = CreateMainScreen();
		GameManager gm = CreateManager(rig, out GameScreen gameScreen);
		int savedTotal = LevelsController.TotalBallCountInMiniLevel;
		try
		{
			SetTotalBallCount(40);
			gm.StartPlay();
			CompleteClose(rig.MainScreen);

			Assert.IsTrue(gameScreen.gameObject.activeSelf, "The game screen must be open after the close.");
			Assert.AreEqual(40, ReadInt(gameScreen, "ballsCounterTargetValue"), "The target is the mini-level's ball count.");
			int shown = ReadInt(gameScreen, "ballsCounterCurrentValue");
			Assert.Less(shown, 40, "Right after the close the counter starts below the target.");

			int frames = 0;
			while (ReadInt(gameScreen, "ballsCounterCurrentValue") != 40 && frames < 1000)
			{
				StepBallsCounter(gameScreen, 1f / 60f);
				frames++;
			}

			Assert.AreEqual(40, ReadInt(gameScreen, "ballsCounterCurrentValue"), "The counter must reach the target.");
			Assert.Less(frames, 1000, "The counter must reach the target in a bounded number of frames.");
		}
		finally
		{
			SetTotalBallCount(savedTotal);
		}
	}
}
