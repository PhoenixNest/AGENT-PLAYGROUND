using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class WP16_PlaytestFixPlayTests
{
	private const float SceneTimeout = 60f;

	private const float FlowTimeout = 20f;

	private const float FxSpawnHeight = 1.2f;

	private const float PositionTolerance = 0.01f;

	private const float RotationThresholdDeg = 0.5f;

	private const int DragFrames = 15;

	private const int MouseIndex = -2;

	private const BindingFlags AnyMember = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
		| BindingFlags.Instance | BindingFlags.FlattenHierarchy;

	private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	private static readonly Type GameManagerType = GameType("GameManager");

	private static readonly Type FailLevelScreenType = GameType("FailLevelScreen");

	private static readonly Type BucketType = GameType("Bucket");

	private static readonly Type RotateControllerType = GameType("RotateController");

	private static readonly Type LevelsControllerType = GameType("Levels.LevelsController");

	private static readonly Type MiniLevelViewType = GameType("Levels.MiniLevelView");

	private static readonly Type BallsControllerType = GameType("Balls.BallsController");

	private static readonly Type LeanTouchType = FindType("Lean.Touch.LeanTouch");

	[SetUp]
	public void SetUp()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		ClearLeanTouchFingers();
	}

	[TearDown]
	public void TearDown()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	[UnityTest]
	public IEnumerator BucketCollectFxSpawnsAboveBucketUnderMiniLevelView()
	{
		yield return BootToMain();
		Call(GameManagerType, GameManagerInstance(), "StartPlay");
		yield return null;

		Component bucket = FindActiveBucket();
		Assert.IsNotNull(bucket, "An active bucket is in the level.");
		SetPrivateField(bucket, "bucketCollectFxTimer", 1f);
		Call(BucketType, bucket, "OnFill", false, 1);

		Transform view = ((Component)Member(LevelsControllerType, null, "MiniLevelView")).transform;
		Transform fx = FindActiveFxClone();
		Assert.IsNotNull(fx, "A BucketFX clone is spawned by the collect.");
		Assert.AreSame(view, fx.parent, "The collect FX must sit under the mini-level view.");
		Assert.That(Mathf.Abs(fx.position.y - (bucket.transform.position.y + FxSpawnHeight)), Is.LessThan(PositionTolerance),
			"The collect FX must spawn 1.2 units above the bucket.");
	}

	[UnityTest]
	public IEnumerator RotationWorksAfterFailRestartWithFingerHeldAcrossRestart()
	{
		yield return BootToMain();
		Call(GameManagerType, GameManagerInstance(), "StartPlay");
		yield return WaitFor(() => CurrentRotateController() != null, FlowTimeout, "The level has a RotateController.");

		Component firstRotate = CurrentRotateController();
		Assert.IsNotNull(firstRotate.gameObject.GetComponent(LeanTouchType),
			"The RotateController's level must carry a LeanTouch component.");
		yield return WaitFor(() => LeanInstances().Count > 0, FlowTimeout, "A LeanTouch instance is live on level 1.");

		Vector2 from = new Vector2(Screen.width * 0.7f, Screen.height * 0.35f);
		Vector2 to = new Vector2(Screen.width * 0.3f, Screen.height * 0.35f);
		object oldInstance = LeanInstances()[0];
		DisableLean(oldInstance);
		StepLean(oldInstance, from);
		Assert.IsTrue(AnyFingerSet(), "The finger must be held down before the fail.");

		Call(LevelsControllerType, null, "HandleMissionFail");
		yield return WaitFor(() => IsActive(FailScreen()), FlowTimeout, "The fail window opens.");
		Call(FailLevelScreenType, FailScreen(), "OnClickRestartLevel");
		yield return WaitFor(() => !IsActive(FailScreen()), FlowTimeout, "Restart closes the fail window.");
		yield return WaitFor(() => LeanInstances().Count > 0 && !ReferenceEquals(LeanInstances()[0], oldInstance), FlowTimeout,
			"A new LeanTouch instance is live after restart.");
		yield return null;
		yield return null;

		Assert.IsFalse(AnyFingerSet(), "The finger held across the restart must be released by the new instance.");

		Call(GameManagerType, GameManagerInstance(), "StartPlay");
		yield return WaitFor(() => !IsActive(MainScreen()), FlowTimeout, "The main screen closes on the second play.");
		yield return WaitFor(() => CurrentRotateController() != null, FlowTimeout, "The restarted level has a RotateController.");
		yield return WaitFor(() => LeanInstances().Count > 0, FlowTimeout, "A LeanTouch instance is live after the second play.");

		GameObject target = (GameObject)Member(RotateControllerType, CurrentRotateController(), "Target");
		Assert.IsNotNull(target, "The restarted RotateController has a target.");
		Quaternion before = target.transform.rotation;

		object lean = LeanInstances()[0];
		DisableLean(lean);
		StepLean(lean, from);
		for (int i = 1; i <= DragFrames; i++)
		{
			yield return null;
			StepLean(lean, Vector2.Lerp(from, to, i / (float)DragFrames));
		}
		yield return null;
		StepLean(lean, null);
		yield return new WaitForSecondsRealtime(0.6f);

		float angle = Quaternion.Angle(before, target.transform.rotation);
		Assert.That(angle, Is.GreaterThan(RotationThresholdDeg), "The level must rotate by the drag after a fail restart with a finger held across it.");
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

	private static Type FindType(string fullName)
	{
		foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			Type t = asm.GetType(fullName, false);
			if (t != null)
			{
				return t;
			}
		}
		return null;
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
		return GameManagerInstance() != null
			&& Member(LevelsControllerType, null, "MiniLevelView") != null
			&& FirstMiniLevelName() != null;
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

	private static object GameManagerInstance()
	{
		return Member(GameManagerType, null, "instance");
	}

	private static object FailScreen()
	{
		return Member(GameManagerType, GameManagerInstance(), "failLevelScreen");
	}

	private static object MainScreen()
	{
		return Member(GameManagerType, GameManagerInstance(), "mainScreen");
	}

	private static bool IsActive(object o)
	{
		GameObject go = o is GameObject g ? g : (o as Component)?.gameObject;
		return go != null && go.activeSelf;
	}

	private static Component CurrentRotateController()
	{
		object view = Member(LevelsControllerType, null, "MiniLevelView");
		return view == null ? null : Member(MiniLevelViewType, view, "rotateController") as Component;
	}

	private static IList LeanInstances()
	{
		return (IList)Member(LeanTouchType, null, "Instances");
	}

	private static void DisableLean(object instance)
	{
		((Behaviour)instance).enabled = false;
	}

	private static void StepLean(object instance, Vector2? pointer)
	{
		var pointers = new List<KeyValuePair<int, Vector2>>();
		if (pointer.HasValue)
		{
			pointers.Add(new KeyValuePair<int, Vector2>(MouseIndex, pointer.Value));
		}

		MethodInfo method = LeanTouchType.GetMethod("ProcessFrame", AnyInstance);
		Assert.IsNotNull(method, "LeanTouch.ProcessFrame not found");
		method.Invoke(instance, new object[] { pointers, Time.unscaledDeltaTime });
	}

	private static bool AnyFingerSet()
	{
		IList fingers = (IList)Member(LeanTouchType, null, "Fingers");
		foreach (object f in fingers)
		{
			if ((bool)Member(f.GetType(), f, "Set"))
			{
				return true;
			}
		}
		return false;
	}

	private static void ClearLeanTouchFingers()
	{
		if (LeanTouchType == null)
		{
			return;
		}
		((IList)Member(LeanTouchType, null, "Fingers")).Clear();
		((IList)Member(LeanTouchType, null, "InactiveFingers")).Clear();
	}

	private static Component FindActiveBucket()
	{
		foreach (UnityEngine.Object o in UnityEngine.Object.FindObjectsByType(BucketType, FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			return (Component)o;
		}
		return null;
	}

	private static Transform FindActiveFxClone()
	{
		foreach (Transform t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			if (t.name.StartsWith("BucketFX", StringComparison.Ordinal))
			{
				return t;
			}
		}
		return null;
	}

	private static void SetPrivateField(object target, string name, object value)
	{
		FieldInfo field = target.GetType().GetField(name, AnyInstance);
		Assert.IsNotNull(field, "Missing private field " + name);
		field.SetValue(target, value);
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
