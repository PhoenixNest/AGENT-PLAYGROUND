using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Balls;
using CrazyLabs.Extensions.Database;
using Lean.Pool;
using Lean.Touch;
using Levels;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class WP17_PlaytestRound2Tests
{
	private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	private const int MouseIndex = LeanTouch.MOUSE_FINGER_INDEX;

	private const int SubGateQuota = 20;

	private const float FrameDt = 0.016f;

	private readonly List<GameObject> _objects = new List<GameObject>();

	private readonly List<LeanGameObjectPool> _pools = new List<LeanGameObjectPool>();

	[SetUp]
	public void SetUp()
	{
		ResetLeanTouchState();
	}

	[TearDown]
	public void TearDown()
	{
		ResetLeanTouchState();
		foreach (LeanGameObjectPool pool in _pools)
		{
			if (pool != null)
			{
				Object.DestroyImmediate(pool.gameObject);
			}
		}
		_pools.Clear();
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
		return go.AddComponent<LeanTouch>();
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

	private static List<KeyValuePair<int, Vector2>> Empty()
	{
		return new List<KeyValuePair<int, Vector2>>();
	}

	private static void Frame(LeanTouch touch, List<KeyValuePair<int, Vector2>> pointers)
	{
		CallInstance(touch, "ProcessFrame", pointers, FrameDt);
	}

	[Test]
	public void FingerAbsentFromNextFrameIsReleasedThenExpires()
	{
		LeanTouch touch = CreateLeanTouch("WP17LeanRelease");
		Frame(touch, Pointers(MouseIndex, new Vector2(100f, 100f)));
		Assert.AreEqual(1, LeanTouch.Fingers.Count, "The press must create one finger.");
		LeanFinger finger = LeanTouch.Fingers[0];

		Frame(touch, Empty());
		Assert.IsFalse(finger.Set, "A finger missing from the frame is released.");
		Assert.AreEqual(1, LeanTouch.Fingers.Count, "The released finger stays listed for its release frame.");

		Frame(touch, Empty());
		Assert.IsTrue(finger.Expired, "The released finger expires the frame after its release.");
		Assert.AreEqual(0, LeanTouch.Fingers.Count, "Fingers must be empty after the expiry.");
	}

	[Test]
	public void ReleaseNeverSeenByTheShimCannotLeaveAStaleFinger()
	{
		LeanTouch touch = CreateLeanTouch("WP17LeanStale");
		for (int cycle = 0; cycle < 3; cycle++)
		{
			Frame(touch, Pointers(MouseIndex, new Vector2(100f + cycle, 100f)));
			Frame(touch, Empty());
			Frame(touch, Empty());
			Assert.AreEqual(0, LeanTouch.Fingers.Count, "No finger may survive two empty frames, cycle " + cycle);
		}
	}

	[Test]
	public void NewPressAfterReleaseBecomesFingerZeroAndDeltaFollowsMovement()
	{
		LeanTouch touch = CreateLeanTouch("WP17LeanRepress");
		Frame(touch, Pointers(MouseIndex, new Vector2(100f, 100f)));
		Frame(touch, Empty());
		Frame(touch, Empty());

		Frame(touch, Pointers(MouseIndex, new Vector2(200f, 200f)));
		Assert.AreEqual(1, LeanTouch.Fingers.Count, "The new press must be the only finger.");
		LeanFinger fresh = LeanTouch.Fingers[0];
		Assert.IsTrue(fresh.Set, "The new finger must be set.");
		Assert.AreEqual(new Vector2(200f, 200f), fresh.ScreenPosition, "The new finger must start at its press position.");

		Frame(touch, Pointers(MouseIndex, new Vector2(210f, 205f)));
		Assert.AreEqual(new Vector2(10f, 5f), fresh.ScreenDelta, "The first move must show as the screen delta.");

		Frame(touch, Pointers(MouseIndex, new Vector2(230f, 215f)));
		Assert.AreEqual(new Vector2(20f, 10f), fresh.ScreenDelta, "The next move must show as the screen delta.");
	}

	[Test]
	public void OnlyThePointersPassedInCreateFingers()
	{
		LeanTouch touch = CreateLeanTouch("WP17LeanPointers");
		Frame(touch, Pointers(7, new Vector2(300f, 300f)));

		Assert.AreEqual(1, LeanTouch.Fingers.Count, "Only the given touch may create a finger.");
		Assert.AreEqual(7, LeanTouch.Fingers[0].Index, "The finger must belong to the touch.");
		Assert.IsFalse(LeanTouch.Fingers.Exists(f => f.Index == MouseIndex), "The mouse must not create a finger while a touch is down.");
	}

	[Test]
	public void TwoTouchesAreTrackedAsTwoFingers()
	{
		LeanTouch touch = CreateLeanTouch("WP17LeanTwoTouches");
		var pointers = new List<KeyValuePair<int, Vector2>>
		{
			new KeyValuePair<int, Vector2>(3, new Vector2(100f, 100f)),
			new KeyValuePair<int, Vector2>(9, new Vector2(400f, 100f)),
		};
		Frame(touch, pointers);
		Assert.AreEqual(2, LeanTouch.Fingers.Count, "Each touch must be its own finger.");

		Frame(touch, new List<KeyValuePair<int, Vector2>> { new KeyValuePair<int, Vector2>(9, new Vector2(410f, 100f)) });
		Assert.AreEqual(1, LeanTouch.Fingers.FindAll(f => f.Set).Count, "Only the touch still down stays set.");
		Assert.AreEqual(new Vector2(10f, 0f), LeanTouch.Fingers.Find(f => f.Index == 9 && f.Set).ScreenDelta, "The held touch keeps its delta.");
	}

	private GameObject MakePrefab()
	{
		GameObject prefab = Make("WP17Prefab");
		prefab.SetActive(false);
		return prefab;
	}

	private GameObject SpawnClone(GameObject prefab, Transform parent)
	{
		GameObject clone = LeanPool.Spawn(prefab, Vector3.zero, Quaternion.identity, parent);
		Assert.IsNotNull(clone, "The pool must return a clone.");
		LeanGameObjectPool pool;
		if (LeanPool.Links.TryGetValue(clone, out pool) && pool != null && !_pools.Contains(pool))
		{
			CallInstance(pool, "Awake");
			_pools.Add(pool);
		}
		return clone;
	}

	private LeanGameObjectPool PoolOf(GameObject clone)
	{
		LeanGameObjectPool pool;
		Assert.IsTrue(LeanPool.Links.TryGetValue(clone, out pool) && pool != null, "The clone must belong to a pool.");
		return pool;
	}

	[Test]
	public void DespawnParentsTheCloneUnderThePoolAndDeactivatesIt()
	{
		GameObject prefab = MakePrefab();
		GameObject temp = Make("WP17TempParent");
		GameObject clone = SpawnClone(prefab, temp.transform);
		Assert.AreSame(temp.transform, clone.transform.parent, "The spawned clone sits under the requested parent.");
		Assert.IsTrue(clone.activeSelf, "The spawned clone is active.");

		LeanPool.Despawn(clone);

		LeanGameObjectPool pool = PoolOf(clone);
		Assert.AreSame(pool.transform, clone.transform.parent, "A despawned clone must be parented under its pool.");
		Assert.IsFalse(clone.activeSelf, "A despawned clone must be inactive.");
		Assert.IsNull(pool.transform.parent, "The pool must be a root object, not a child of a level.");
	}

	[Test]
	public void SpawnAfterTheDespawnedCloneIsDestroyedReturnsAValidClone()
	{
		GameObject prefab = MakePrefab();
		GameObject temp = Make("WP17TempParent");
		GameObject first = SpawnClone(prefab, temp.transform);
		LeanPool.Despawn(first);
		Object.DestroyImmediate(first);

		GameObject second = null;
		Assert.DoesNotThrow(() => second = SpawnClone(prefab, temp.transform));
		Assert.IsNotNull(second, "A destroyed despawned clone must be skipped, not returned.");
		Assert.IsTrue(second.activeSelf, "The returned clone is active.");
		Assert.AreSame(temp.transform, second.transform.parent, "The returned clone sits under the requested parent.");

		Object.DestroyImmediate(second);
		GameObject third = null;
		Assert.DoesNotThrow(() => third = SpawnClone(prefab, temp.transform));
		Assert.IsNotNull(third, "A destroyed spawned clone must not block the next spawn.");
		Assert.IsTrue(third.activeSelf, "The next clone is active.");
	}

	[Test]
	public void DespawnAllToleratesADestroyedSpawnedClone()
	{
		GameObject prefab = MakePrefab();
		GameObject temp = Make("WP17TempParent");
		GameObject destroyed = SpawnClone(prefab, temp.transform);
		GameObject alive = SpawnClone(prefab, temp.transform);
		Object.DestroyImmediate(destroyed);

		Assert.DoesNotThrow(() => LeanPool.DespawnAll(), "DespawnAll must skip a destroyed clone.");

		Assert.IsFalse(alive.activeSelf, "The live clone is despawned by DespawnAll.");
		Assert.AreSame(PoolOf(alive).transform, alive.transform.parent, "The live clone is parented under its pool.");

		GameObject reused = null;
		Assert.DoesNotThrow(() => reused = SpawnClone(prefab, temp.transform));
		Assert.IsNotNull(reused, "The pool must still spawn after DespawnAll.");
	}

	[Test]
	public void SubGateQuotaIsNotRestoredByReconcile()
	{
		SubGate gate = MakeSubGate(SubGateQuota);
		for (int i = 0; i < SubGateQuota; i++)
		{
			Assert.IsTrue(gate.Sub(), "Ball " + (i + 1) + " must consume the quota.");
		}
		Assert.IsFalse(gate.Sub(), "Ball " + (SubGateQuota + 1) + " must pass.");
		Assert.AreEqual(0, gate.Value, "The quota is used up.");

		MiniLevelView view = MakeView(gate);
		MiniLevelData mini = new MiniLevelData
		{
			gateTypes = new List<GateType> { GateType.Subtract },
			gateValues = new List<int> { SubGateQuota },
		};
		object controller = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(GatesController));
		for (int frame = 0; frame < 3; frame++)
		{
			CallInstance(controller, "ReconcileGates", view, mini);
		}

		Assert.AreEqual(0, gate.Value, "The per-frame reconcile must not restore the quota.");
		Assert.IsFalse(gate.Sub(), "An exhausted gate must keep passing balls after the reconcile.");
	}

	[Test]
	public void SubGateExhaustionDisablesVisualColliderAndLabelAndResetRestoresThem()
	{
		GameObject go = Make("WP17SubExhaust");
		SubGate gate = go.AddComponent<SubGate>();
		gate.visual = go.AddComponent<MeshRenderer>();
		gate.collider = go.AddComponent<BoxCollider>();
		gate.ValueText = go.AddComponent<TextMeshPro>();
		gate.SetValue(2);

		Assert.IsTrue(gate.Sub(), "The first ball consumes the quota.");
		Assert.IsTrue(gate.Sub(), "The second ball consumes the quota.");
		Assert.IsFalse(gate.Sub(), "The third ball passes an exhausted gate.");
		Assert.AreEqual(0, gate.Value, "The exhausted gate holds no quota.");
		Assert.IsFalse(gate.visual.enabled, "The exhausted gate hides its visual.");
		Assert.IsFalse(gate.collider.enabled, "The exhausted gate disables its collider.");
		Assert.IsFalse(gate.ValueText.enabled, "The exhausted gate hides its label.");

		gate.ResetGate();

		Assert.IsTrue(gate.visual.enabled, "The reset gate shows its visual.");
		Assert.IsTrue(gate.collider.enabled, "The reset gate enables its collider.");
		Assert.IsTrue(gate.ValueText.enabled, "The reset gate shows its label.");
		Assert.AreEqual(2, gate.Value, "The reset gate restores its start value.");
		Assert.IsTrue(gate.Sub(), "The reset gate consumes balls again.");
	}

	[Test]
	public void DuplicateMiniLevelNameKeepsTheFirstRow()
	{
		var database = (DatabaseController)RuntimeHelpers.GetUninitializedObject(typeof(DatabaseController));
		try
		{
			typeof(DatabaseController).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, database);
			SetPrivate(database, "_dataSetKeys", new Dictionary<string, List<string>>());
			SetPrivate(database, "_dataSetStringData", new Dictionary<string, DataSetParsedData<string>>());
			typeof(DatabaseController).GetProperty("IsInitialized").GetSetMethod(true).Invoke(null, new object[] { true });
			InjectStringDataSet(database, "LevelsData", new List<string> { "first", "second" },
				new Dictionary<string, Dictionary<string, string>>
				{
					{ "first", MiniLevelRow("WP17Dup", "600") },
					{ "second", MiniLevelRow("WP17Dup", "900") },
				});

			var controller = (LevelsController)RuntimeHelpers.GetUninitializedObject(typeof(LevelsController));
			SetPrivate(controller, "_allLevels", new List<LevelData>());
			SetPrivate(controller, "_loopingLevelss", new List<LevelData>());
			SetPrivate(controller, "_miniLevels", new List<MiniLevelData>());
			SetPrivate(controller, "_miniLevelsByNames", new Dictionary<string, MiniLevelData>());
			SetPrivate(controller, "_isDataLoaded", false);
			typeof(LevelsController).GetMethod("LoadData", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(controller, null);

			var miniLevels = (List<MiniLevelData>)GetPrivate(controller, "_miniLevels");
			var byName = (Dictionary<string, MiniLevelData>)GetPrivate(controller, "_miniLevelsByNames");
			Assert.AreEqual(2, miniLevels.Count, "Both rows are parsed as mini levels.");
			Assert.AreEqual(1, byName.Count, "A duplicated name maps to a single entry.");
			Assert.AreEqual(600, byName["WP17Dup"].targetBallAmount, "The first row with a name must win.");
		}
		finally
		{
			typeof(DatabaseController).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
			typeof(DatabaseController).GetProperty("IsInitialized").GetSetMethod(true).Invoke(null, new object[] { false });
		}
	}

	private static Dictionary<string, string> MiniLevelRow(string levelName, string targetBallAmount)
	{
		return new Dictionary<string, string>
		{
			{ "Level Name", levelName },
			{ "Target Ball Amount", targetBallAmount },
		};
	}

	private static void InjectStringDataSet(DatabaseController database, string dataSetId, List<string> keys,
		Dictionary<string, Dictionary<string, string>> rows)
	{
		var keyMap = (IDictionary)GetPrivate(database, "_dataSetKeys");
		keyMap[dataSetId] = keys;

		object parsed = Activator.CreateInstance(typeof(DataSetParsedData<string>), new object[] { null });
		typeof(DataSetParsedData<string>).GetField("parsedData").SetValue(parsed, rows);
		var stringMap = (IDictionary)GetPrivate(database, "_dataSetStringData");
		stringMap[dataSetId] = parsed;
	}

	private static object GetPrivate(object target, string name)
	{
		FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field, "Missing private field " + name);
		return field.GetValue(target);
	}

	private static void SetPrivate(object target, string name, object value)
	{
		FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field, "Missing private field " + name);
		field.SetValue(target, value);
	}

	[Test]
	public void RotateUpdateSplitsTheRemainingDeltaAndConverges()
	{
		GameObject root = Make("WP17RotateRoot");
		RotateController controller = root.AddComponent<RotateController>();
		GameObject target = Make("WP17RotateTarget");
		controller.Target = target;
		controller.Targets = new List<RotateTarget>();
		controller.Axis = Vector3.forward;
		controller.Space = Space.World;
		SetRemainingDelta(controller, 30f);

		CallInstance(controller, "UpdateRotation", 0.5f);
		Assert.AreEqual(15f, Mathf.DeltaAngle(0f, target.transform.eulerAngles.z), 0.01f, "half the remaining delta is applied in the first step");
		Assert.AreEqual(15f, RemainingAngle(controller), 0.01f, "half the remaining delta is kept for the next steps");

		for (int i = 0; i < 40; i++)
		{
			CallInstance(controller, "UpdateRotation", 0.5f);
		}
		Assert.AreEqual(30f, Mathf.DeltaAngle(0f, target.transform.eulerAngles.z), 0.01f, "repeated steps converge to the full delta");
		Assert.AreEqual(0f, RemainingAngle(controller), 0.01f, "nothing remains once converged");
	}

	[Test]
	public void RotateTargetWithDirMinusOneFollowsAndDirOneTurnsOpposite()
	{
		GameObject root = Make("WP17RotateDirRoot");
		RotateController controller = root.AddComponent<RotateController>();
		GameObject target = Make("WP17RotateDirTarget");
		GameObject same = Make("WP17RotateDirSame");
		GameObject opposite = Make("WP17RotateDirOpposite");
		controller.Target = target;
		controller.Targets = new List<RotateTarget>
		{
			new RotateTarget { Target = same.transform, Dir = 1 },
			new RotateTarget { Target = opposite.transform, Dir = -1 },
		};
		controller.Axis = Vector3.forward;
		controller.Space = Space.World;
		SetRemainingDelta(controller, 30f);

		CallInstance(controller, "UpdateRotation", 0.5f);

		Assert.AreEqual(15f, Mathf.DeltaAngle(0f, target.transform.eulerAngles.z), 0.01f, "the reference target turns by the step");
		Assert.AreEqual(-15f, Mathf.DeltaAngle(0f, same.transform.eulerAngles.z), 0.01f, "Dir 1 turns by the opposite angle");
		Assert.AreEqual(15f, Mathf.DeltaAngle(0f, opposite.transform.eulerAngles.z), 0.01f, "Dir -1 turns by the same angle as the target");
	}

	[Test]
	public void HandleRotationKeepsOnlyTheZAngleOfTheSkinRotator()
	{
		GameObject ballGo = Make("WP17BallView");
		BallView view = ballGo.AddComponent<BallView>();
		GameObject rotator = Make("WP17SkinRotator");
		rotator.transform.rotation = Quaternion.Euler(20f, 30f, 40f);
		float zAngle = rotator.transform.eulerAngles.z;
		view.skinRotator = rotator.transform;

		MethodInfo method = typeof(BallsController).GetMethod("HandleRotation", BindingFlags.Static | BindingFlags.NonPublic);
		Assert.IsNotNull(method, "BallsController.HandleRotation not found");
		method.Invoke(null, new object[] { view });

		Vector3 euler = rotator.transform.eulerAngles;
		Assert.AreEqual(0f, Mathf.DeltaAngle(0f, euler.x), 0.01f, "the x tilt is removed");
		Assert.AreEqual(0f, Mathf.DeltaAngle(0f, euler.y), 0.01f, "the y tilt is removed");
		Assert.AreEqual(zAngle, euler.z, 0.01f, "the z angle is kept");
	}

	private static void SetRemainingDelta(RotateController controller, float degrees)
	{
		FieldInfo field = typeof(RotateController).GetField("remainingDelta", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field, "remainingDelta");
		field.SetValue(controller, Quaternion.Euler(0f, 0f, degrees));
	}

	private static float RemainingAngle(RotateController controller)
	{
		FieldInfo field = typeof(RotateController).GetField("remainingDelta", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field, "remainingDelta");
		return Quaternion.Angle(Quaternion.identity, (Quaternion)field.GetValue(controller));
	}

	private SubGate MakeSubGate(int value)
	{
		GameObject go = Make("WP17Sub");
		SubGate gate = go.AddComponent<SubGate>();
		gate.SetValue(value);
		return gate;
	}

	private MiniLevelView MakeView(GateBase gate)
	{
		GameObject go = Make("WP17View");
		MiniLevelView view = go.AddComponent<MiniLevelView>();
		view.allGates = new GateBase[] { gate };
		return view;
	}
}
