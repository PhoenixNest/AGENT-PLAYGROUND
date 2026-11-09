using System.Collections.Generic;
using System.Reflection;
using Lean.Touch;
using NUnit.Framework;
using UnityEngine;

public class WP2_RotationTests
{
	private GameObject _root;
	private GameObject _target;
	private RotateController _controller;
	private readonly List<LeanFinger> _fingers = new List<LeanFinger>();

	[SetUp]
	public void SetUp()
	{
		_root = new GameObject("WP2_RotateRoot");
		_target = new GameObject("WP2_RotateTarget");

		_controller = _root.AddComponent<RotateController>();
		_controller.Target = _target;
		_controller.Targets = new List<RotateTarget>();
		_controller.Space = Space.World;
		_controller.Axis = Vector3.forward;
		_controller.Multiplier = 3.5f;
		_controller.Damping = 20f;
		_controller.ScaleByTime = false;

		CallInstance("Awake");
	}

	[TearDown]
	public void TearDown()
	{
		RemoveFingers();
		Object.DestroyImmediate(_root);
		Object.DestroyImmediate(_target);
	}

	[Test]
	public void GetDampenFactor_IsExponentialAndBounded()
	{
		Assert.AreEqual(1f, LeanHelper.GetDampenFactor(-1f, 0.02f), 1e-6f, "negative damping applies everything");
		Assert.AreEqual(0f, LeanHelper.GetDampenFactor(20f, 0f), 1e-6f);
		Assert.AreEqual(1f - Mathf.Exp(-20f * 0.02f), LeanHelper.GetDampenFactor(20f, 0.02f), 1e-6f);
	}

	[Test]
	public void GetDeltaDegrees_IsSignedAngleAroundTheReferencePoint()
	{
		var finger = new LeanFinger
		{
			Set = true,
			LastScreenPosition = new Vector2(100f, 0f),
			ScreenPosition = new Vector2(0f, 100f),
		};
		float degrees = finger.GetDeltaDegrees(Vector2.zero);
		Assert.AreEqual(90f, Mathf.Abs(degrees), 0.01f);

		finger.ScreenPosition = new Vector2(0f, -100f);
		Assert.AreEqual(-degrees, finger.GetDeltaDegrees(Vector2.zero), 0.01f, "reversing the sweep flips the sign");
	}

	[Test]
	public void GetDeltaDegrees_ZeroAtReferencePoint()
	{
		var finger = new LeanFinger
		{
			Set = true,
			LastScreenPosition = Vector2.zero,
			ScreenPosition = new Vector2(5f, 5f),
		};
		Assert.AreEqual(0f, finger.GetDeltaDegrees(Vector2.zero), 0f);
	}

	[Test]
	public void RotateAB_WithoutFingersOrCamera_IsANoOp()
	{
		_target.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
		Quaternion start = _target.transform.rotation;

		_controller.RotateAB();
		for (int i = 0; i < 50; i++)
		{
			CallInstance("FixedUpdate");
		}

		Assert.AreEqual(0f, Quaternion.Angle(start, _target.transform.rotation), 1e-4f);
	}

	[Test]
	public void RemainingDelta_IsAppliedInFullWithDampening()
	{
		SetRemainingDelta(60f);
		for (int i = 0; i < 300; i++)
		{
			StepDampened();
		}

		Assert.AreEqual(60f, Quaternion.Angle(Quaternion.identity, _target.transform.rotation), 0.05f);
		Assert.AreEqual(0f, GetRemainingAngle(), 1e-3f);
	}

	[Test]
	public void RemainingDelta_FirstStepIsOnlyTheDampenedShare()
	{
		SetRemainingDelta(60f);
		UpdateRotation(LeanHelper.GetDampenFactor(20f, Time.fixedDeltaTime));

		float expected = 60f * (1f - Mathf.Exp(-20f * Time.fixedDeltaTime));
		Assert.AreEqual(expected, Quaternion.Angle(Quaternion.identity, _target.transform.rotation), 0.05f);
	}

	[Test]
	public void Rotators_DirMinusOneTurnsWithTheTargetAndDirOneTurnsTheOppositeWay()
	{
		GameObject child = new GameObject("WP2_RotateChild");
		GameObject mirror = new GameObject("WP2_RotateMirror");
		try
		{
			_controller.Targets.Add(new RotateTarget { Target = child.transform, Dir = 1 });
			_controller.Targets.Add(new RotateTarget { Target = mirror.transform, Dir = -1 });
			CallInstance("Awake");

			SetRemainingDelta(45f);
			for (int i = 0; i < 300; i++)
			{
				StepDampened();
			}

			Assert.AreEqual(45f, Mathf.DeltaAngle(0f, _target.transform.eulerAngles.z), 0.05f);
			Assert.AreEqual(-45f, Mathf.DeltaAngle(0f, child.transform.eulerAngles.z), 0.05f);
			Assert.AreEqual(45f, Mathf.DeltaAngle(0f, mirror.transform.eulerAngles.z), 0.05f);
		}
		finally
		{
			Object.DestroyImmediate(child);
			Object.DestroyImmediate(mirror);
		}
	}

	[Test]
	public void RotateToDefault_SnapsWithoutEasingAndKeepsRemainingDelta()
	{
		SetRemainingDelta(30f);
		_target.transform.rotation = Quaternion.Euler(0f, 0f, 40f);

		_controller.RotateToDefault();
		Assert.AreEqual(0f, Quaternion.Angle(Quaternion.identity, _target.transform.rotation), 1e-4f, "snap must be immediate");

		StepDampened();
		Assert.Greater(Quaternion.Angle(Quaternion.identity, _target.transform.rotation), 0f, "remaining delta survives the snap");
	}

	private void SetRemainingDelta(float degrees)
	{
		FieldInfo field = typeof(RotateController).GetField("remainingDelta", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field, "remainingDelta");
		field.SetValue(_controller, Quaternion.Euler(0f, 0f, degrees));
	}

	private float GetRemainingAngle()
	{
		FieldInfo field = typeof(RotateController).GetField("remainingDelta", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.IsNotNull(field, "remainingDelta");
		return Quaternion.Angle(Quaternion.identity, (Quaternion)field.GetValue(_controller));
	}

	private void StepDampened()
	{
		UpdateRotation(LeanHelper.GetDampenFactor(20f, 0.02f));
	}

	private void UpdateRotation(float factor)
	{
		MethodInfo method = typeof(RotateController).GetMethod("UpdateRotation",
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		Assert.IsNotNull(method, "UpdateRotation");
		method.Invoke(_controller, new object[] { factor });
	}

	private void RemoveFingers()
	{
		foreach (LeanFinger finger in _fingers)
		{
			LeanTouch.Fingers.Remove(finger);
		}
		_fingers.Clear();
	}

	private void CallInstance(string name)
	{
		MethodInfo method = typeof(RotateController).GetMethod(name,
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		Assert.IsNotNull(method, name);
		method.Invoke(_controller, null);
	}
}
