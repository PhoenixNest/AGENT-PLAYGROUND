using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class WP14_TapToStartTests
{
	private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

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
		rig.MainScreenObject = MakeInactive("WP14MainScreen");
		Transform root = rig.MainScreenObject.transform;
		rig.Title = MakeChild("WP14GameTitle", root);
		rig.Title.AddComponent<Image>().color = new Color(0.2f, 0.4f, 0.6f, 1f);
		rig.Setting = MakeChild("WP14SettingBtn", root);
		rig.Shop = MakeChild("WP14ShopBtn", root);
		rig.Gems = MakeChild("WP14GemsCounterImg", root);
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

	private GameManager CreateManager(Rig rig, out GameScreen gameScreen)
	{
		GameObject screenObject = MakeInactive("WP14GameScreen");
		gameScreen = screenObject.AddComponent<GameScreen>();
		GameManager gm = Make("WP14GameManager").AddComponent<GameManager>();
		gm.mainScreen = rig.MainScreen;
		gm.gameScreen = gameScreen;
		return gm;
	}

	private static Sequence GetCloseSequence(MainScreen mainScreen)
	{
		FieldInfo field = typeof(MainScreen).GetField("mainScreenSequence", AnyInstance);
		Assert.IsNotNull(field, "MainScreen.mainScreenSequence not found");
		return (Sequence)field.GetValue(mainScreen);
	}

	private static void CompleteClose(MainScreen mainScreen)
	{
		Sequence sequence = GetCloseSequence(mainScreen);
		Assert.IsNotNull(sequence, "The close sequence should be running.");
		sequence.Complete(true);
	}

	private static void CompleteIntro(MainScreen mainScreen)
	{
		Sequence sequence = GetCloseSequence(mainScreen);
		Assert.IsNotNull(sequence, "The intro sequence should be running.");
		sequence.Complete(true);
	}

	private static void SetGameManagerInstance(GameManager value)
	{
		PropertyInfo property = typeof(SingletonBehaviour<GameManager>).GetProperty("instance", AnyStatic);
		Assert.IsNotNull(property);
		property.GetSetMethod(true).Invoke(null, new object[] { value });
	}

	[Test]
	public void StartPlayKeepsTheGameScreenHiddenUntilTheMainScreenCloses()
	{
		Rig rig = CreateMainScreen();
		GameManager gm = CreateManager(rig, out GameScreen gameScreen);

		gm.StartPlay();

		Assert.IsFalse(gameScreen.gameObject.activeSelf, "The game screen must stay hidden while the main screen closes.");
		Assert.IsTrue(rig.MainScreenObject.activeSelf, "The main screen must still be on during the close.");

		CompleteClose(rig.MainScreen);

		Assert.IsFalse(rig.MainScreenObject.activeSelf, "The main screen must turn off when the close ends.");
		Assert.IsTrue(gameScreen.gameObject.activeSelf, "The game screen must open when the close ends.");
	}

	[Test]
	public void MainScreenCloseRunsTheCallbackOnce()
	{
		Rig rig = CreateMainScreen();
		int calls = 0;

		rig.MainScreen.Close(() => calls++);
		Assert.AreEqual(0, calls, "The callback must wait for the close animation.");

		CompleteClose(rig.MainScreen);
		Assert.AreEqual(1, calls, "The callback must run once when the close ends.");
		Assert.IsNull(GetCloseSequence(rig.MainScreen), "The finished close must release its sequence.");
	}

	[Test]
	public void MainScreenCloseUsesTheRecoveredDurations()
	{
		Rig rig = CreateMainScreen();

		rig.MainScreen.Close();

		Sequence sequence = GetCloseSequence(rig.MainScreen);
		Assert.IsNotNull(sequence);
		Assert.AreEqual(0.5f, sequence.Duration(false), 0.0001f, "The close lasts 0.5 s, as in the original.");
	}

	[Test]
	public void MainScreenElementsAreRestoredWhenShownAgain()
	{
		Rig rig = CreateMainScreen();
		CompleteIntro(rig.MainScreen);
		Image titleImage = rig.Title.GetComponent<Image>();
		Image gemsImage = rig.Gems.GetComponent<Image>();
		Color titleColor = titleImage.color;
		Color gemsColor = gemsImage.color;
		Vector3 titlePosition = rig.Title.transform.localPosition;
		Vector3 shopPosition = rig.Shop.transform.localPosition;

		rig.MainScreen.Close();
		CompleteClose(rig.MainScreen);

		Assert.AreEqual(0f, titleImage.color.a, 0.0001f, "The title must be faded out by the close.");
		Assert.AreEqual(Vector3.zero, rig.Title.transform.localScale, "The title must be zoomed out by the close.");

		Reenable(rig);
		CompleteIntro(rig.MainScreen);

		Assert.AreEqual(titleColor, titleImage.color, "The title colour must be restored.");
		Assert.AreEqual(gemsColor, gemsImage.color, "The gems icon colour must be restored.");
		Assert.AreEqual(Vector3.one, rig.Title.transform.localScale, "The title scale must be restored.");
		Assert.AreEqual(Vector3.one, rig.Setting.transform.localScale, "The settings button scale must be restored.");
		Assert.AreEqual(titlePosition, rig.Title.transform.localPosition, "The title position must be restored.");
		Assert.AreEqual(shopPosition, rig.Shop.transform.localPosition, "The shop button position must be restored.");
		Assert.AreEqual(Vector3.one, rig.Shop.transform.localScale, "The shop button scale must be restored.");
	}

	[Test]
	public void StartPlayIgnoresASecondTap()
	{
		Rig rig = CreateMainScreen();
		GameManager gm = CreateManager(rig, out GameScreen gameScreen);

		gm.StartPlay();
		Sequence first = GetCloseSequence(rig.MainScreen);
		Assert.IsNotNull(first);

		gm.StartPlay();

		Assert.AreSame(first, GetCloseSequence(rig.MainScreen), "A second tap must not start a second close.");
		Assert.IsTrue(first.IsActive(), "The first close must keep running.");
		Assert.IsTrue(gm.GameStarted);
		Assert.IsFalse(gameScreen.gameObject.activeSelf, "The game screen must still wait for the first close.");
	}
}
