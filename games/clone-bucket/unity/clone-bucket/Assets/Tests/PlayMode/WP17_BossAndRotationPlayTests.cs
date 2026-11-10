using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class WP17_BossAndRotationPlayTests
{
	private const float SceneTimeout = 60f;

	private const float FlowTimeout = 20f;

	private const float RingFraction = 0.25f;

	private const int DragFrames = 30;

	private const float DragStartDegrees = -60f;

	private const float DragSweepDegrees = 120f;

	private const float SettleSeconds = 1f;

	private const float BossRotationThresholdDeg = 5f;

	private const int MouseIndex = -2;

	private const int TargetLevel = 8;

	private const int BossLevel = 11;

	private static string ReportPath
	{
		get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "output", "diagnostics", "boss-rot.txt")); }
	}

	private const BindingFlags AnyMember = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
		| BindingFlags.Instance | BindingFlags.FlattenHierarchy;

	private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	private static readonly Type GameManagerType = GameType("GameManager");

	private static readonly Type LevelsControllerType = GameType("Levels.LevelsController");

	private static readonly Type BallsControllerType = GameType("Balls.BallsController");

	private static readonly Type RotateControllerType = GameType("RotateController");

	private static readonly Type LeanTouchType = FindType("Lean.Touch.LeanTouch");

	private readonly List<string> _logProblems = new List<string>();

	[SetUp]
	public void SetUp()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		Time.timeScale = 1f;
		_logProblems.Clear();
		ClearLeanTouchFingers();
		Application.logMessageReceived += OnLog;
	}

	[TearDown]
	public void TearDown()
	{
		Application.logMessageReceived -= OnLog;
		Time.timeScale = 1f;
		ClearLeanTouchFingers();
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}

	private void OnLog(string condition, string stackTrace, LogType type)
	{
		bool poolProblem = type == LogType.Warning && (condition.StartsWith("Despawn:", StringComparison.Ordinal)
			|| condition.StartsWith("Spawn:", StringComparison.Ordinal));
		if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert || poolProblem)
		{
			_logProblems.Add(type + ": " + condition);
		}
	}

	[UnityTest]
	public IEnumerator BossLevelSpawnsItsStartBalls()
	{
		yield return BootToMain();
		yield return OpenLevelAndStart(BossLevel);

		object data = Member(LevelsControllerType, null, "CurrentMiniLevelData");
		Assert.IsNotNull(data, "The boss level has mini-level data.");
		int expected = (int)Member(data.GetType(), data, "startBallAmount");
		Assert.Greater(expected, 0, "The boss level has start balls in its data.");
		Assert.AreEqual(expected, (int)Member(LevelsControllerType, null, "TotalBallCountInMiniLevel"),
			"The boss level total must equal its start ball amount.");

		object ballsController = UnityEngine.Object.FindAnyObjectByType(BallsControllerType);
		Assert.IsNotNull(ballsController, "A BallsController is live in the scene.");
		yield return WaitFor(() => ActiveBallViews(ballsController).Count >= expected, FlowTimeout,
			"The boss level does not spawn its start balls.");
		yield return null;

		Assert.AreEqual(expected, ActiveBallViews(ballsController).Count,
			"The boss level must have exactly its start balls as active BallViews.");
		Assert.IsEmpty(_logProblems, "Opening the boss level must log no error, exception or pool warning.");
	}

	[UnityTest]
	public IEnumerator BossMazeRotatesWithTheFinger()
	{
		yield return BootToMain();
		yield return OpenLevelAndStart(TargetLevel);

		Camera cam = Camera.main;
		Assert.IsNotNull(cam, "The main scene must have a main camera.");
		var lines = new List<string>();

		Component normal = CurrentRotateController();
		Assert.IsNotNull(normal, "Level 8 has a RotateController.");
		List<MazeProbe> normalProbes = SnapshotMaze(normal);
		yield return DragCounterClockwise(normal, cam);
		var normalMeasured = new List<float>();
		foreach (MazeProbe probe in normalProbes)
		{
			float signed = SignedCounterClockwise(probe, cam);
			normalMeasured.Add(signed);
			lines.Add(Describe("normal level " + TargetLevel, probe, signed));
		}

		yield return OpenLevelAndStart(BossLevel);

		Component boss = CurrentRotateController();
		Assert.IsNotNull(boss, "The boss level has a RotateController.");
		List<MazeProbe> bossProbes = SnapshotMaze(boss);
		yield return DragCounterClockwise(boss, cam);
		var measured = new List<float>();
		foreach (MazeProbe probe in bossProbes)
		{
			float signed = SignedCounterClockwise(probe, cam);
			measured.Add(signed);
			lines.Add(Describe("boss level " + BossLevel, probe, signed));
		}

		string report = string.Join(Environment.NewLine, lines);
		Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
		File.WriteAllText(ReportPath, report + Environment.NewLine);

		float normalTargetSign = Mathf.Sign(normalMeasured[0]);
		float referenceSign = Mathf.Sign(measured[0]);
		for (int i = 0; i < bossProbes.Count; i++)
		{
			Assert.Greater(Mathf.Abs(measured[i]), BossRotationThresholdDeg,
				bossProbes[i].Label + " of the boss maze must rotate by the finger drag. Measured signed angles:"
				+ Environment.NewLine + report);
			Assert.AreEqual(normalTargetSign, Mathf.Sign(measured[i]),
				bossProbes[i].Label + " of the boss maze must turn the same way as the normal level Target with the finger. Measured signed angles:"
				+ Environment.NewLine + report);
			Assert.AreEqual(referenceSign, Mathf.Sign(measured[i]),
				bossProbes[i].Label + " of the boss maze must turn the same way as the boss ReferencePoint. Measured signed angles:"
				+ Environment.NewLine + report);
		}
		Assert.IsEmpty(_logProblems, "The boss drag must log no error, exception or pool warning.");
	}

	[UnityTest]
	public IEnumerator MainSceneComponentsAllBindAtRuntime()
	{
		yield return BootToMain();

		var nulls = new List<string>();
		foreach (Transform transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			Component[] components = transform.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] == null)
				{
					nulls.Add(PathOf(transform) + " [component " + i + "]");
				}
			}
		}
		Assert.IsEmpty(nulls, "Null components in the loaded scenes (" + nulls.Count + "): " + string.Join("; ", nulls));

		Type particleImageType = FindType("AssetKits.ParticleImage");
		Assert.IsNotNull(particleImageType, "The AssetKits.ParticleImage.ParticleImage type must be loaded.");

		Transform stars = null;
		foreach (Transform transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			if (transform.name == "StarsParticles" && HasAncestorNamed(transform, "CompleteLevelScreen"))
			{
				stars = transform;
				break;
			}
		}
		Assert.IsNotNull(stars, "StarsParticles must exist under CompleteLevelScreen.");
		Assert.IsNotNull(stars.GetComponent(particleImageType),
			"StarsParticles must carry a bound AssetKits.ParticleImage.ParticleImage component.");
	}

	private static bool HasAncestorNamed(Transform transform, string name)
	{
		for (Transform current = transform.parent; current != null; current = current.parent)
		{
			if (current.name == name)
			{
				return true;
			}
		}
		return false;
	}

	private static string PathOf(Transform transform)
	{
		string path = transform.name;
		for (Transform current = transform.parent; current != null; current = current.parent)
		{
			path = current.name + "/" + path;
		}
		return path;
	}

	private static string Describe(string context, MazeProbe probe, float signed)
	{
		string dir = probe.Dir.HasValue ? "Dir " + probe.Dir.Value : "Dir n/a";
		return context + " | " + probe.Label + " | " + dir + " | signed CCW degrees " + signed.ToString("F3",
			System.Globalization.CultureInfo.InvariantCulture);
	}

	private static List<MazeProbe> SnapshotMaze(Component rotate)
	{
		var probes = new List<MazeProbe>();
		GameObject reference = (GameObject)Member(RotateControllerType, rotate, "Target");
		Assert.IsNotNull(reference, "The RotateController has a target.");
		probes.Add(NewProbe("Target (ReferencePoint)", reference.transform, null));

		var targets = (IList)Member(RotateControllerType, rotate, "Targets");
		for (int i = 0; i < targets.Count; i++)
		{
			object entry = targets[i];
			if (entry == null)
			{
				continue;
			}
			var transform = (Transform)Member(entry.GetType(), entry, "Target");
			if (transform == null)
			{
				continue;
			}
			int dir = (int)Member(entry.GetType(), entry, "Dir");
			probes.Add(NewProbe("Targets[" + i + "]", transform, dir));
		}
		return probes;
	}

	private static MazeProbe NewProbe(string label, Transform transform, int? dir)
	{
		return new MazeProbe { Label = label, Transform = transform, StartUp = transform.up, Dir = dir };
	}

	private static float SignedCounterClockwise(MazeProbe probe, Camera cam)
	{
		Vector3 forward = cam.transform.forward;
		Vector3 before = Vector3.ProjectOnPlane(probe.StartUp, forward);
		Vector3 after = Vector3.ProjectOnPlane(probe.Transform.up, forward);
		return Vector3.SignedAngle(before, after, -forward);
	}

	private static IEnumerator DragCounterClockwise(Component rotate, Camera cam)
	{
		GameObject reference = (GameObject)Member(RotateControllerType, rotate, "Target");
		Vector2 centre = cam.WorldToScreenPoint(reference.transform.position);
		float radius = Screen.width * RingFraction;
		Component lean = rotate.gameObject.GetComponent(LeanTouchType);
		Assert.IsNotNull(lean, "The level carries a LeanTouch component.");

		((Behaviour)lean).enabled = false;
		StepLean(lean, RingPoint(centre, radius, DragStartDegrees));
		for (int i = 1; i <= DragFrames; i++)
		{
			yield return null;
			float degrees = DragStartDegrees + DragSweepDegrees * i / DragFrames;
			StepLean(lean, RingPoint(centre, radius, degrees));
		}
		yield return null;
		StepLean(lean, null);
		yield return new WaitForSecondsRealtime(SettleSeconds);
	}

	private static Vector2 RingPoint(Vector2 centre, float radius, float degrees)
	{
		float radians = degrees * Mathf.Deg2Rad;
		return centre + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
	}

	private static IEnumerator OpenLevelAndStart(int level)
	{
		object game = Instance();
		object previousView = Member(LevelsControllerType, null, "MiniLevelView");
		Call(GameManagerType, game, "SaveData", level);
		SetMember(GameManagerType, game, "CurrentLevel", level);
		Call(GameManagerType, game, "RestartGame");

		yield return WaitFor(() =>
		{
			object view = Member(LevelsControllerType, null, "MiniLevelView");
			return view != null && !ReferenceEquals(view, previousView) && CurrentRotateController() != null;
		}, FlowTimeout, "Level " + level + " does not spawn.");
		Assert.AreEqual(level, (int)Member(GameManagerType, game, "CurrentLevel"), "The displayed level must be " + level + ".");

		yield return WaitFor(() => IsActive(MainScreen()), FlowTimeout, "The main screen does not show for level " + level + ".");
		Assert.IsTrue(TapToStart(), "The main screen has a tap-to-start trigger wired to StartPlayClick.");
		yield return WaitFor(() => (bool)Member(GameManagerType, Instance(), "GameStarted"), FlowTimeout,
			"Start does not begin level " + level + ".");
		yield return WaitFor(() => !IsActive(MainScreen()), FlowTimeout, "The main screen does not close on start.");
		yield return null;
		yield return null;
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

	private static List<object> ActiveBallViews(object ballsController)
	{
		var result = new List<object>();
		var data = (System.Collections.IDictionary)Member(BallsControllerType, ballsController, "_dataByBalls");
		foreach (System.Collections.DictionaryEntry entry in data)
		{
			var view = entry.Key as Component;
			if (view == null || !view.gameObject.activeInHierarchy)
			{
				continue;
			}
			if ((bool)Member(entry.Value.GetType(), entry.Value, "isActive"))
			{
				result.Add(entry.Key);
			}
		}
		return result;
	}

	private static object Instance()
	{
		return Member(GameManagerType, null, "instance");
	}

	private static object MainScreen()
	{
		return Member(GameManagerType, Instance(), "mainScreen");
	}

	private static bool IsActive(object o)
	{
		GameObject go = o is GameObject g ? g : (o as Component)?.gameObject;
		return go != null && go.activeSelf;
	}

	private static Component CurrentRotateController()
	{
		object view = Member(LevelsControllerType, null, "MiniLevelView");
		return view == null ? null : Member(view.GetType(), view, "rotateController") as Component;
	}

	private static bool TapToStart()
	{
		EventSystem eventSystem = EventSystem.current;
		Assert.IsNotNull(eventSystem, "The main scene must have an EventSystem.");
		foreach (EventTrigger trigger in UnityEngine.Object.FindObjectsByType<EventTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
		{
			foreach (EventTrigger.Entry entry in trigger.triggers)
			{
				if (entry.eventID != EventTriggerType.PointerDown)
				{
					continue;
				}
				for (int i = 0; i < entry.callback.GetPersistentEventCount(); i++)
				{
					if (entry.callback.GetPersistentMethodName(i) == "StartPlayClick")
					{
						entry.callback.Invoke(new PointerEventData(eventSystem));
						return true;
					}
				}
			}
		}
		return false;
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

	private static void ClearLeanTouchFingers()
	{
		if (LeanTouchType == null)
		{
			return;
		}
		((IList)Member(LeanTouchType, null, "Fingers")).Clear();
		((IList)Member(LeanTouchType, null, "InactiveFingers")).Clear();
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

	private static void SetMember(Type type, object target, string name, object value)
	{
		PropertyInfo property = type.GetProperty(name, AnyMember);
		if (property != null)
		{
			property.SetValue(target, value);
			return;
		}

		FieldInfo field = type.GetField(name, AnyMember);
		Assert.IsNotNull(field, "Missing member " + name);
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

	private sealed class MazeProbe
	{
		public string Label;

		public Transform Transform;

		public Vector3 StartUp;

		public int? Dir;
	}
}
