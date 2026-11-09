using System.Collections.Generic;
using System.Reflection;
using Lean.Touch;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class WP16_PlaytestFixTests
{
	private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	private const string MainScenePath = "Assets/Scenes/Main.unity";

	private const float AngleTolerance = 0.01f;

	private const float PositionTolerance = 0.01f;

	private const int MouseIndex = LeanTouch.MOUSE_FINGER_INDEX;

	private readonly List<GameObject> _objects = new List<GameObject>();

	[SetUp]
	public void SetUp()
	{
		ResetLeanTouchState();
	}

	[TearDown]
	public void TearDown()
	{
		ResetLeanTouchState();
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

	private LeanTouch CreateLeanTouch(string name)
	{
		GameObject go = Make(name);
		go.SetActive(false);
		LeanTouch touch = go.AddComponent<LeanTouch>();
		CallInstance(touch, "OnEnable");
		return touch;
	}

	private static object CallInstance(object target, string name, params object[] args)
	{
		MethodInfo method = target.GetType().GetMethod(name, AnyInstance);
		Assert.IsNotNull(method, target.GetType().Name + "." + name + " not found");
		return method.Invoke(target, args);
	}

	private static void ResetLeanTouchState()
	{
		LeanTouch.Instances.Clear();
		LeanTouch.Fingers.Clear();
		LeanTouch.InactiveFingers.Clear();
	}

	private static List<KeyValuePair<int, Vector2>> Pointers(int index, Vector2 position)
	{
		return new List<KeyValuePair<int, Vector2>> { new KeyValuePair<int, Vector2>(index, position) };
	}

	private static Transform FindTransformInScene(Scene scene, string name)
	{
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
			{
				if (t.name == name)
				{
					return t;
				}
			}
		}
		return null;
	}

	private static Transform FindDescendant(Transform root, string name)
	{
		foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
		{
			if (t.name == name)
			{
				return t;
			}
		}
		return null;
	}

	[Test]
	public void ChainSegmentKeepsPitchAndYawAndClearsRoll()
	{
		GameObject go = Make("WP16Chain");
		ChainSegment segment = go.AddComponent<ChainSegment>();
		go.transform.localRotation = Quaternion.Euler(10f, 20f, 30f);

		CallInstance(segment, "FixedUpdate");

		Vector3 euler = go.transform.localRotation.eulerAngles;
		Assert.That(Mathf.Abs(Mathf.DeltaAngle(10f, euler.x)), Is.LessThan(AngleTolerance), "The chain segment must keep its pitch.");
		Assert.That(Mathf.Abs(Mathf.DeltaAngle(20f, euler.y)), Is.LessThan(AngleTolerance), "The chain segment must keep its yaw.");
		Assert.That(Mathf.Abs(Mathf.DeltaAngle(0f, euler.z)), Is.LessThan(AngleTolerance), "The chain segment must clear its roll.");
	}

	[Test]
	public void RestartButtonIsCentredOnFailScreen()
	{
		Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Additive);
		try
		{
			Transform failScreen = FindTransformInScene(scene, "FailLevelScreen");
			Assert.IsNotNull(failScreen, "FailLevelScreen is not in Main.unity.");
			Transform restart = FindDescendant(failScreen, "RestartLevelBtn");
			Assert.IsNotNull(restart, "RestartLevelBtn is not under FailLevelScreen.");
			RectTransform rect = restart.GetComponent<RectTransform>();
			Assert.IsNotNull(rect, "RestartLevelBtn has no RectTransform.");
			Assert.AreEqual(0f, rect.anchoredPosition.x, PositionTolerance, "The restart button must be centred on the fail screen.");
		}
		finally
		{
			EditorSceneManager.CloseScene(scene, true);
		}
	}

	[Test]
	public void LeanTouchFingerSurvivesInstanceSwap()
	{
		LeanTouch first = CreateLeanTouch("WP16LeanFirst");
		CallInstance(first, "ProcessFrame", Pointers(MouseIndex, new Vector2(100f, 100f)), 0.016f);
		Assert.AreEqual(1, LeanTouch.Fingers.Count, "The first instance must begin one finger.");
		LeanFinger held = LeanTouch.Fingers[0];
		Assert.IsTrue(held.Set, "The begun finger must be set.");

		CallInstance(first, "OnDisable");
		Object.DestroyImmediate(first.gameObject);

		LeanTouch second = CreateLeanTouch("WP16LeanSecond");
		for (int i = 0; i < 2; i++)
		{
			CallInstance(second, "ProcessFrame", new List<KeyValuePair<int, Vector2>>(), 0.016f);
		}

		Assert.IsFalse(held.Set, "The new instance must release the finger begun by the old one.");
		Assert.IsTrue(held.Expired, "The released finger must expire.");
		Assert.AreEqual(0, LeanTouch.Fingers.Count, "No finger may stay set in the shared list after the swap.");

		CallInstance(second, "ProcessFrame", Pointers(MouseIndex, new Vector2(200f, 150f)), 0.016f);
		Assert.AreEqual(1, LeanTouch.Fingers.Count, "The new instance must begin a finger.");
		LeanFinger fresh = LeanTouch.Fingers[0];
		Assert.IsTrue(fresh.Set, "The new finger must be set.");
		Assert.AreEqual(MouseIndex, fresh.Index, "The new finger must be the mouse finger.");
		Assert.AreEqual(new Vector2(200f, 150f), fresh.ScreenPosition, "The new finger must be at its begin position.");
	}
}
