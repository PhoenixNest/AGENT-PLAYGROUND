using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WP7_CameraTests
{
	private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
	private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	private GameObject _root;
	private CameraMove _move;

	private static T Invoke<T>(string name, params object[] args)
	{
		MethodInfo method = typeof(CameraMove).GetMethod(name, StaticFlags);
		Assert.IsNotNull(method, "CameraMove." + name + " not found");
		return (T)method.Invoke(null, args);
	}

	[SetUp]
	public void SetUp()
	{
		_root = new GameObject("WP7_CameraRoot");
		_move = _root.AddComponent<CameraMove>();
		_move.Damping = 2f;
		_move.BossLevelBucketOffset = 3f;
		_move.yOffset = 1.5f;
		_move.yOffsetDefault = 4f;
		_move.ZoomPadding = 7.25f;
		_move.MinZoomSize = 3f;
		_move.MaxZoomSize = 15f;
	}

	[TearDown]
	public void TearDown()
	{
		Object.DestroyImmediate(_root);
	}

	[Test]
	public void ZoomSize_ClampsToMaxZoom()
	{
		Assert.AreEqual(15f, Invoke<float>("ZoomSize", 40f, 0f, 3f, 15f), 0.0001f);
	}

	[Test]
	public void ZoomSize_ClampsToMinZoom()
	{
		Assert.AreEqual(3f, Invoke<float>("ZoomSize", 0f, 0f, 3f, 15f), 0.0001f);
	}

	[Test]
	public void ZoomSize_IsPaddingPlusHalfTheYRange()
	{
		Assert.AreEqual(7f, Invoke<float>("ZoomSize", 10f, 2f, 3f, 15f), 0.0001f);
	}

	[Test]
	public void ZoomSize_NeverExceedsClampForRandomRanges()
	{
		var rng = new System.Random(7);
		for (int i = 0; i < 500; i++)
		{
			float range = (float)(rng.NextDouble() * 60.0);
			float size = Invoke<float>("ZoomSize", range, 7.25f, 3f, 15f);
			Assert.GreaterOrEqual(size, 3f);
			Assert.LessOrEqual(size, 15f);
		}
	}

	[Test]
	public void ClampTargetY_NeverBelowPosYLimit()
	{
		float limit = 2.5f;
		var rng = new System.Random(11);
		for (int i = 0; i < 500; i++)
		{
			float desired = (float)(rng.NextDouble() * 40.0 - 20.0);
			float min = (float)(rng.NextDouble() * 10.0 - 5.0);
			float max = min + (float)(rng.NextDouble() * 20.0);
			float y = Invoke<float>("ClampTargetY", desired, min, max, limit);
			Assert.GreaterOrEqual(y, limit, "target fell below posYLimit");
		}
	}

	[Test]
	public void ClampTargetY_HoldsInsideMinMaxWhenAboveLimit()
	{
		Assert.AreEqual(5f, Invoke<float>("ClampTargetY", 5f, 0f, 10f, -100f), 0.0001f);
		Assert.AreEqual(10f, Invoke<float>("ClampTargetY", 25f, 0f, 10f, -100f), 0.0001f);
		Assert.AreEqual(0f, Invoke<float>("ClampTargetY", -3f, 0f, 10f, -100f), 0.0001f);
	}

	[Test]
	public void ClampTargetY_BucketLimitWinsOverMax()
	{
		Assert.AreEqual(12f, Invoke<float>("ClampTargetY", 5f, 0f, 10f, 12f), 0.0001f);
	}

	[Test]
	public void DampStep_ZeroTimeKeepsCurrent()
	{
		Assert.AreEqual(3f, Invoke<float>("DampStep", 3f, 10f, 2f, 0f), 0.0001f);
	}

	[Test]
	public void DampStep_ZeroDampingKeepsCurrent()
	{
		Assert.AreEqual(3f, Invoke<float>("DampStep", 3f, 10f, 0f, 0.5f), 0.0001f);
	}

	[Test]
	public void DampStep_IsLinearWithClampedFraction()
	{
		Assert.AreEqual(0.66f, Invoke<float>("DampStep", 0f, 10f, 2f, 0.033f), 0.0001f);
	}

	[Test]
	public void DampStep_LargeFractionLandsOnTarget()
	{
		Assert.AreEqual(10f, Invoke<float>("DampStep", 0f, 10f, 1000f, 0.5f), 0.0001f);
	}

	[Test]
	public void DampStep_MovesTowardTargetWithoutOvershoot()
	{
		float value = 0f;
		float previous = value;
		for (int i = 0; i < 200; i++)
		{
			value = Invoke<float>("DampStep", value, 10f, 2f, 0.033f);
			Assert.GreaterOrEqual(value, previous);
			Assert.LessOrEqual(value, 10f + 1e-4f);
			previous = value;
		}
		Assert.AreEqual(10f, value, 0.01f);
	}

	[Test]
	public void SizeToDistance_RoundTripsWithDefaultFov()
	{
		float size = 16.93f * Mathf.Tan(25f * Mathf.Deg2Rad);
		float distance = Invoke<float>("SizeToDistance", size, 50f);
		Assert.AreEqual(16.93f, distance, 0.001f);
	}

	[Test]
	public void SizeToDistance_ZeroFovReturnsSize()
	{
		Assert.AreEqual(7f, Invoke<float>("SizeToDistance", 7f, 0f), 0.0001f);
	}

	[Test]
	public void PosYLimitUpdate_NonBossLevelLeavesLimitUnchanged()
	{
		_move.SetBossLevel(true);
		_move.PosYLimitUpdate(2f);
		float before = ReadFloat("posYLimit");

		_move.SetBossLevel(false);
		_move.PosYLimitUpdate(5f);
		Assert.AreEqual(before, ReadFloat("posYLimit"), 0f);
	}

	[Test]
	public void PosYLimitUpdate_BossLevelAddsBossOffset()
	{
		_move.SetBossLevel(true);
		_move.PosYLimitUpdate(5f);
		Assert.AreEqual(8f, ReadFloat("posYLimit"), 0.0001f);
	}

	[Test]
	public void Reset_KeepsOffsetAndPosYLimit()
	{
		_move.SetBossLevel(true);
		_move.PosYLimitUpdate(5f);
		_move.yOffset = 2.25f;

		_move.Reset();

		Assert.AreEqual(2.25f, _move.yOffset, 0f);
		Assert.AreEqual(8f, ReadFloat("posYLimit"), 0.0001f);
	}

	[Test]
	public void SetBossLevel_StoresTheFlag()
	{
		_move.SetBossLevel(true);
		Assert.IsTrue(ReadBool("isBossLevel"));
		_move.SetBossLevel(false);
		Assert.IsFalse(ReadBool("isBossLevel"));
	}

	private float ReadFloat(string name)
	{
		FieldInfo field = typeof(CameraMove).GetField(name, InstanceFlags);
		Assert.IsNotNull(field, name);
		return (float)field.GetValue(_move);
	}

	private bool ReadBool(string name)
	{
		FieldInfo field = typeof(CameraMove).GetField(name, InstanceFlags);
		Assert.IsNotNull(field, name);
		return (bool)field.GetValue(_move);
	}
}
