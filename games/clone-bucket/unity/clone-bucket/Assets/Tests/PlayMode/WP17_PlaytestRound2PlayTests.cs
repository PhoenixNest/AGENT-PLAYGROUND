using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class WP17_PlaytestRound2PlayTests
{
	private const float SceneTimeout = 60f;

	private const float FlowTimeout = 20f;

	private const float RingFraction = 0.25f;

	private const int DragFrames = 20;

	private const float DragRotationThresholdDeg = 5f;

	private const int MouseIndex = -2;

	private const int MinusQuota = 3;

	private const int MinusBallsSent = 8;

	private const int TargetLevel = 8;

	private const int GateQuota = 20;

	private const int ExtraBallCount = 25;

	private const int PostExhaustionBalls = 3;

	private const string SubGateTag = "SubGate";

	private const string BucketFxPrefix = "BucketFX";

	private const BindingFlags AnyMember = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
		| BindingFlags.Instance | BindingFlags.FlattenHierarchy;

	private const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	private static readonly Type GameManagerType = GameType("GameManager");

	private static readonly Type BucketType = GameType("Bucket");

	private static readonly Type LevelsControllerType = GameType("Levels.LevelsController");

	private static readonly Type MiniLevelViewType = GameType("Levels.MiniLevelView");

	private static readonly Type BallsControllerType = GameType("Balls.BallsController");

	private static readonly Type GatesControllerType = GameType("GatesController");

	private static readonly Type GateTypeType = GameType("GateType");

	private static readonly Type SubGateType = GameType("SubGate");

	private static readonly Type FailLevelScreenType = GameType("FailLevelScreen");

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
	public IEnumerator BucketFxStaysValidAcrossWinAndNextLevel()
	{
		yield return BootToMain();

		int firstFills = FillBucketWithCollectFx();
		Assert.That(firstFills, Is.GreaterThan(1), "Level 1 must fill the bucket through several collect FX.");

		yield return WaitFor(() => (bool)Member(GameManagerType, Instance(), "LevelCompleted"), FlowTimeout,
			"The win does not complete level 1.");
		object completeScreen = Member(GameManagerType, Instance(), "completeLevelScreen");
		yield return WaitFor(() => ((Component)completeScreen).gameObject.activeSelf, FlowTimeout,
			"The complete screen does not open.");
		GameObject nextButton = (GameObject)Member(completeScreen.GetType(), completeScreen, "NextLevelBtn");
		yield return WaitFor(() => nextButton.activeSelf, FlowTimeout,
			"The Next button does not appear after the leaderboard climb.");

		Call(completeScreen.GetType(), completeScreen, "OnClickNextLevel");

		yield return WaitFor(() => (int)Member(GameManagerType, Instance(), "CurrentLevel") == 2
			&& FirstMiniLevelName() == "StartLevel2", FlowTimeout, "Level 2 does not spawn after Next.");
		yield return null;

		int secondFills = FillBucketWithCollectFx();
		Assert.That(secondFills, Is.GreaterThan(1), "Level 2 must fill the bucket through several collect FX.");

		Transform view = ((Component)Member(LevelsControllerType, null, "MiniLevelView")).transform;
		List<Transform> fx = ActiveFxClones();
		Assert.IsNotEmpty(fx, "The collect FX must spawn again on level 2.");
		foreach (Transform clone in fx)
		{
			Assert.IsNotNull(clone, "A collect FX clone must be valid.");
			Assert.AreSame(view, clone.parent, "A collect FX clone must sit under the current mini-level view.");
		}

		Assert.IsEmpty(_logProblems, "The win and next-level flow must log no error, exception or pool warning.");
	}

	[UnityTest]
	public IEnumerator SubGateRemovesExactlyItsQuotaThroughTheBallPath()
	{
		yield return BootToMain();

		object view = Member(LevelsControllerType, null, "MiniLevelView");
		Transform viewTransform = ((Component)view).transform;
		object gates = UnityEngine.Object.FindAnyObjectByType(GatesControllerType);
		Assert.IsNotNull(gates, "A GatesController is live in the scene.");

		object subtract = Enum.Parse(GateTypeType, "Subtract");
		Component gate = (Component)Call(GatesControllerType, gates, "SpawnGate", subtract, Vector3.zero, Quaternion.identity,
			Vector3.one, viewTransform);
		Assert.IsNotNull(gate, "A sub gate is spawned from the GatesConfig.");
		Assert.IsInstanceOf(SubGateType, gate, "The spawned gate must be a SubGate.");
		Call(SubGateType, gate, "SetValue", MinusQuota);

		Collider gateCollider = gate.GetComponentInChildren<Collider>(true);
		Assert.IsNotNull(gateCollider, "The sub gate has a collider.");
		Assert.IsTrue(gateCollider.CompareTag(SubGateTag), "The sub gate collider must carry the SubGate tag.");
		object registered = Call(GatesControllerType, null, "GetSubGate", gateCollider.transform);
		Assert.IsNotNull(registered, "The sub gate must be registered for its collider.");
		Assert.AreSame(gate, registered, "The collider must resolve to the spawned sub gate.");

		object ballsController = UnityEngine.Object.FindAnyObjectByType(BallsControllerType);
		Assert.IsNotNull(ballsController, "A BallsController is live in the scene.");
		List<object> balls = ActiveBallViews(ballsController);
		Assert.That(balls.Count, Is.GreaterThanOrEqualTo(MinusBallsSent), "The level must have enough balls to send through the gate.");

		for (int i = 0; i < MinusBallsSent; i++)
		{
			Call(BallsControllerType, ballsController, "OnBallTriggerEntered", balls[i], gateCollider);
		}

		int removed = 0;
		for (int i = 0; i < MinusBallsSent; i++)
		{
			if (!IsBallActive(ballsController, balls[i]))
			{
				removed++;
			}
		}

		Assert.AreEqual(MinusQuota, removed, "Exactly the quota of balls must be removed by the gate.");
		Assert.AreEqual(0, (int)Member(SubGateType, gate, "Value"), "The gate must be exhausted after its quota.");
	}

	[UnityTest]
	public IEnumerator RealFailThroughMinusGateThenRestartKeepsInputAlive()
	{
		yield return BootToMain();

		object game = Instance();
		Call(GameManagerType, game, "SaveData", TargetLevel);
		SetMember(GameManagerType, game, "CurrentLevel", TargetLevel);
		Call(GameManagerType, game, "Reset", false);
		yield return null;
		yield return WaitFor(() => CurrentRotateController() != null, FlowTimeout, "Level 8 does not spawn.");
		Assert.AreEqual(TargetLevel, (int)Member(GameManagerType, game, "CurrentLevel"), "The displayed level must be 8.");

		Component oldRotate = CurrentRotateController();
		object view = Member(LevelsControllerType, null, "MiniLevelView");
		List<Component> subGates = LevelSubGates(view);
		Assert.AreEqual(2, subGates.Count, "Level 8 has two sub gates.");
		foreach (Component subGate in subGates)
		{
			Assert.AreEqual(GateQuota, (int)Member(SubGateType, subGate, "Value"), "Each level sub gate has a quota of 20.");
		}

		Time.timeScale = 0f;
		object ballsController = UnityEngine.Object.FindAnyObjectByType(BallsControllerType);
		Assert.IsNotNull(ballsController, "A BallsController is live in the scene.");
		Transform viewTransform = ((Component)view).transform;
		Vector3 gatePosition = subGates[0].transform.position;
		for (int i = 0; i < ExtraBallCount; i++)
		{
			Call(BallsControllerType, null, "Spawn", gatePosition, viewTransform);
		}
		yield return null;

		List<object> balls = ActiveBallViews(ballsController);
		Assert.That(balls.Count, Is.GreaterThanOrEqualTo(ExtraBallCount), "The level must have at least 25 live balls.");

		Component gateA = subGates[0];
		Collider colliderA = (Collider)Field(SubGateType, gateA, "collider");
		Assert.IsNotNull(colliderA, "The first level sub gate has a collider.");
		for (int i = 0; i < GateQuota; i++)
		{
			Call(BallsControllerType, ballsController, "OnBallTriggerEntered", balls[i], colliderA);
			yield return null;
			Assert.AreEqual(i + 1, RemovedCount(ballsController, balls), "After ball " + (i + 1) + " the gate must have removed exactly " + (i + 1) + " balls.");
		}

		Assert.AreEqual(0, (int)Member(SubGateType, gateA, "Value"), "The first sub gate is exhausted after its quota.");

		for (int i = GateQuota; i < GateQuota + PostExhaustionBalls; i++)
		{
			Call(BallsControllerType, ballsController, "OnBallTriggerEntered", balls[i], colliderA);
			yield return null;
		}

		Assert.AreEqual(GateQuota, RemovedCount(ballsController, balls), "Balls sent after exhaustion must not be removed.");
		for (int i = GateQuota; i < GateQuota + PostExhaustionBalls; i++)
		{
			Assert.IsTrue(IsBallActive(ballsController, balls[i]), "A ball sent after exhaustion must stay active.");
		}
		Assert.IsFalse(colliderA.enabled, "The exhausted sub gate disables its collider once a ball passes it.");
		Assert.IsFalse(((Renderer)Field(SubGateType, gateA, "visual")).enabled, "The exhausted sub gate hides its visual.");
		Assert.IsFalse(((Behaviour)Field(SubGateType, gateA, "ValueText")).enabled, "The exhausted sub gate hides its label.");

		Component gateB = subGates[1];
		Collider colliderB = (Collider)Field(SubGateType, gateB, "collider");
		List<object> remaining = LiveBalls(ballsController, balls);
		Assert.That(remaining.Count, Is.GreaterThan(0), "Some balls must remain to drain through the second sub gate.");
		foreach (object ball in remaining)
		{
			Call(BallsControllerType, ballsController, "OnBallTriggerEntered", ball, colliderB);
			yield return null;
		}
		Assert.IsEmpty(LiveBalls(ballsController, balls), "The second sub gate must drain every remaining ball.");

		Time.timeScale = 1f;
		yield return WaitFor(() => IsActive(FailScreen()), FlowTimeout, "The fail window does not open after the real ball drain.");

		Button restart = FindButtonCalling((Component)FailScreen(), "OnClickRestartLevel");
		Assert.IsNotNull(restart, "The fail screen has a restart button wired to OnClickRestartLevel.");
		restart.onClick.Invoke();
		yield return WaitFor(() => !IsActive(FailScreen()), FlowTimeout, "Restart does not close the fail window.");
		yield return WaitFor(() => IsActive(MainScreen()), FlowTimeout, "Restart does not show the main screen.");

		Assert.IsTrue(TapToStart(), "The main screen has a tap-to-start trigger wired to StartPlayClick.");
		yield return WaitFor(() => (bool)Member(GameManagerType, Instance(), "GameStarted"), FlowTimeout, "Start does not begin the level.");
		yield return WaitFor(() => !IsActive(MainScreen()), FlowTimeout, "The main screen does not close on start.");
		yield return WaitFor(() => CurrentRotateController() != null && !ReferenceEquals(CurrentRotateController(), oldRotate), FlowTimeout,
			"The restarted level has no new RotateController.");
		yield return null;
		yield return null;

		Assert.That(Time.timeScale, Is.EqualTo(1f).Within(0.0001f), "Time must run at normal speed after the restart.");
		Component rotate = CurrentRotateController();
		Assert.IsNotNull(rotate, "The restarted level has a RotateController.");
		Assert.IsFalse(ReferenceEquals(rotate, oldRotate), "The restarted level must be a new level instance.");

		foreach (Component subGate in LevelSubGates(Member(LevelsControllerType, null, "MiniLevelView")))
		{
			Assert.AreEqual(GateQuota, (int)Member(SubGateType, subGate, "Value"), "The restarted level's sub gates are full again.");
			Assert.IsTrue(((Collider)Field(SubGateType, subGate, "collider")).enabled, "The restarted level's sub gate collider is enabled.");
		}

		AssertNoBlockingUi(Member(LevelsControllerType, null, "MiniLevelView") as Component);

		Assert.AreEqual(0, ((IList)Member(LeanTouchType, null, "Fingers")).Count, "No finger may be listed after restart and start.");

		Component lean = rotate.gameObject.GetComponent(LeanTouchType);
		Assert.IsNotNull(lean, "The restarted RotateController's level carries a LeanTouch component.");
		Assert.IsTrue(LeanInstances().Contains(lean), "The restarted level's LeanTouch is the live instance.");

		GameObject target = (GameObject)Member(RotateControllerType, rotate, "Target");
		Assert.IsNotNull(target, "The restarted RotateController has a target.");
		Quaternion before = target.transform.rotation;
		Vector2 from = new Vector2(Screen.width * 0.7f, Screen.height * 0.35f);
		Vector2 to = new Vector2(Screen.width * 0.3f, Screen.height * 0.35f);

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
		Assert.That(angle, Is.GreaterThan(DragRotationThresholdDeg), "The maze must rotate by the drag after the restart; measured " + angle + " degrees.");

		Assert.IsEmpty(_logProblems, "The fail, restart and start flow must log no error, exception or pool warning.");
	}

	private void AssertNoBlockingUi(Component mazeView)
	{
		EventSystem eventSystem = EventSystem.current;
		Assert.IsNotNull(eventSystem, "The main scene must have an EventSystem.");
		Camera camera = Camera.main;
		Assert.IsNotNull(camera, "The main scene must have a main camera.");
		Assert.IsNotNull(mazeView, "The restarted level has a mini-level view.");

		Vector2 centre = camera.WorldToScreenPoint(mazeView.transform.position);
		var points = new List<Vector2> { centre };
		float radius = Screen.width * RingFraction;
		for (int k = 0; k < 8; k++)
		{
			float angle = k * 45f * Mathf.Deg2Rad;
			points.Add(centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
		}

		var allHits = new List<string>();
		var blocking = new List<string>();
		foreach (Vector2 point in points)
		{
			var pointer = new PointerEventData(eventSystem) { position = point };
			var results = new List<RaycastResult>();
			eventSystem.RaycastAll(pointer, results);
			foreach (RaycastResult hit in results)
			{
				if (hit.gameObject == null)
				{
					continue;
				}
				allHits.Add(hit.gameObject.name + " at " + point);
				Graphic graphic = hit.gameObject.GetComponent<Graphic>();
				if (graphic != null && graphic.raycastTarget)
				{
					blocking.Add(hit.gameObject.name + " at " + point);
				}
			}
		}

		Assert.IsEmpty(blocking, "UI raycast hits block the maze. All hits: " + string.Join("; ", allHits));
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

	private static int FillBucketWithCollectFx()
	{
		Component bucket = CurrentBucket();
		Assert.IsNotNull(bucket, "An active bucket is in the level.");

		int target = (int)Call(LevelsControllerType, null, "GetTargetBallAmount");
		int limit = target * 2 + 10;
		int fills = 0;
		while ((int)Member(LevelsControllerType, null, "TotalFillCount") < target)
		{
			Assert.That(fills, Is.LessThan(limit), "The bucket must reach its target.");
			int fill = (int)Member(LevelsControllerType, null, "TotalFillCount");
			if (fill + 1 == target)
			{
				Call(LevelsControllerType, null, "SubtractTotal", (int)Member(LevelsControllerType, null, "TotalBallCountInMiniLevel") - target);
			}
			else
			{
				Call(LevelsControllerType, null, "AddTotal");
			}
			SetPrivateField(bucket, "bucketCollectFxTimer", 1f);
			Call(BucketType, bucket, "OnFill", false, 1);
			fills++;
		}
		return fills;
	}

	private static Component CurrentBucket()
	{
		object view = Member(LevelsControllerType, null, "MiniLevelView");
		if (view != null)
		{
			Component inView = ((Component)view).GetComponentInChildren(BucketType, false);
			if (inView != null)
			{
				return inView;
			}
		}

		foreach (UnityEngine.Object o in UnityEngine.Object.FindObjectsByType(BucketType, FindObjectsInactive.Exclude))
		{
			return (Component)o;
		}
		return null;
	}

	private static List<Transform> ActiveFxClones()
	{
		var result = new List<Transform>();
		foreach (Transform t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
		{
			if (t.name.StartsWith(BucketFxPrefix, StringComparison.Ordinal))
			{
				result.Add(t);
			}
		}
		return result;
	}

	private static List<Component> LevelSubGates(object view)
	{
		var result = new List<Component>();
		object gates = Member(MiniLevelViewType, view, "allGates");
		Assert.IsNotNull(gates, "The mini-level view lists its gates.");
		foreach (object gate in (IEnumerable)gates)
		{
			if (gate != null && SubGateType.IsInstanceOfType(gate))
			{
				result.Add((Component)gate);
			}
		}
		return result;
	}

	private static List<object> ActiveBallViews(object ballsController)
	{
		var result = new List<object>();
		var data = (IDictionary)Member(BallsControllerType, ballsController, "_dataByBalls");
		foreach (DictionaryEntry entry in data)
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

	private static List<object> LiveBalls(object ballsController, List<object> balls)
	{
		var result = new List<object>();
		foreach (object ball in balls)
		{
			if (IsBallActive(ballsController, ball))
			{
				result.Add(ball);
			}
		}
		return result;
	}

	private static int RemovedCount(object ballsController, List<object> balls)
	{
		return balls.Count - LiveBalls(ballsController, balls).Count;
	}

	private static bool IsBallActive(object ballsController, object view)
	{
		var data = (IDictionary)Member(BallsControllerType, ballsController, "_dataByBalls");
		return data.Contains(view) && (bool)Member(data[view].GetType(), data[view], "isActive");
	}

	private static object Instance()
	{
		return Member(GameManagerType, null, "instance");
	}

	private static object FailScreen()
	{
		return Member(GameManagerType, Instance(), "failLevelScreen");
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
		return view == null ? null : Member(MiniLevelViewType, view, "rotateController") as Component;
	}

	private static Button FindButtonCalling(Component root, string methodName)
	{
		Button[] buttons = root != null
			? root.GetComponentsInChildren<Button>(true)
			: UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (Button button in buttons)
		{
			for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
			{
				if (button.onClick.GetPersistentMethodName(i) == methodName)
				{
					return button;
				}
			}
		}
		return null;
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

	private static void SetPrivateField(object target, string name, object value)
	{
		FieldInfo field = target.GetType().GetField(name, AnyInstance);
		Assert.IsNotNull(field, "Missing private field " + name);
		field.SetValue(target, value);
	}

	private static object Field(Type type, object target, string name)
	{
		FieldInfo field = type.GetField(name, AnyMember);
		Assert.IsNotNull(field, "Missing field " + name);
		return field.GetValue(target);
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
}
