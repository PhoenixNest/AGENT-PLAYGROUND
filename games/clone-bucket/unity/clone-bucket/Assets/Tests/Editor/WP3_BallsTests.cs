using System.Collections.Generic;
using System.Reflection;
using Balls;
using NUnit.Framework;
using UnityEngine;

public class WP3_BallsTests
{
	private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

	private GameObject _poolParent;

	private BallsConfig _config;

	[SetUp]
	public void SetUp()
	{
		_poolParent = new GameObject("WP3_PoolParent");

		GameObject template = new GameObject("WP3_BallTemplate");
		template.AddComponent<Rigidbody>();
		template.AddComponent<SphereCollider>();
		BallView view = template.AddComponent<BallView>();
		template.SetActive(false);

		_config = ScriptableObject.CreateInstance<BallsConfig>();
		_config.prefab = view;
		_config.initialPoolAmount = 200;
		_config.useBallValueTiers = true;
		_config.valuePerTier = new[] { 1, 2, 3, 999 };
		_config.totalBallsForNextTier = new[] { 300, 450, 650, 775 };
		_config.chancesToOverrideTier = new[] { 0.5f, 0.5f, 0.5f, 0.5f };
		_config.useBallLayers = true;
		_config.ballLayers = new[] { 9, 10, 11 };
		_config.minimumBallsForLayers = new[] { 200, 400, 600 };
		_config.inMazeMaxSpeed = 10f;
		_config.outMazeMaxSpeed = 40f;
		_config.ignoreMultiplicatorCapacity = 10;
		_config.smallScale = new Vector3(0.17f, 0.17f, 0.17f);
		_config.mediumScale = new Vector3(0.19f, 0.19f, 0.19f);
		_config.bigScale = new Vector3(0.22f, 0.22f, 0.22f);
	}

	[TearDown]
	public void TearDown()
	{
		if (_poolParent != null)
		{
			Object.DestroyImmediate(_poolParent);
		}
		Object.DestroyImmediate(_config.prefab.gameObject);
		Object.DestroyImmediate(_config);
	}

	[Test]
	public void PreWarm_CreatesInitialAmountInactive()
	{
		BallsPool.Initialize(_config, _poolParent.transform);

		Assert.AreEqual(200, BallsPool.InactiveCount);
		Assert.AreEqual(0, BallsPool.ActiveCount);
	}

	[Test]
	public void GetAndRelease_BalanceTheCounts()
	{
		BallsPool.Initialize(_config, _poolParent.transform);

		BallView view = BallsPool.Get();

		Assert.IsNotNull(view);
		Assert.IsTrue(view.gameObject.activeSelf);
		Assert.AreEqual(1, BallsPool.ActiveCount);
		Assert.AreEqual(199, BallsPool.InactiveCount);

		BallsPool.Release(view);

		Assert.IsFalse(view.gameObject.activeSelf);
		Assert.AreEqual(0, BallsPool.ActiveCount);
		Assert.AreEqual(200, BallsPool.InactiveCount);
	}

	[Test]
	public void Get_GrowsPastPrewarm()
	{
		BallsPool.Initialize(_config, _poolParent.transform);

		var taken = new List<BallView>();
		for (int i = 0; i < 201; i++)
		{
			taken.Add(BallsPool.Get());
		}

		Assert.AreEqual(201, BallsPool.ActiveCount);

		foreach (BallView view in taken)
		{
			BallsPool.Release(view);
		}

		Assert.AreEqual(0, BallsPool.ActiveCount);
		Assert.AreEqual(201, BallsPool.InactiveCount);
	}

	[Test]
	public void ValueTierIndex_StrictThresholdFold()
	{
		int[] thresholds = { 300, 450, 650, 775 };
		int tierCount = 4;

		Assert.AreEqual(0, ValueTierIndex(0, thresholds, tierCount));
		Assert.AreEqual(0, ValueTierIndex(300, thresholds, tierCount));
		Assert.AreEqual(1, ValueTierIndex(301, thresholds, tierCount));
		Assert.AreEqual(1, ValueTierIndex(450, thresholds, tierCount));
		Assert.AreEqual(2, ValueTierIndex(451, thresholds, tierCount));
		Assert.AreEqual(2, ValueTierIndex(650, thresholds, tierCount));
		Assert.AreEqual(3, ValueTierIndex(651, thresholds, tierCount));
		Assert.AreEqual(3, ValueTierIndex(775, thresholds, tierCount));
		Assert.AreEqual(3, ValueTierIndex(5000, thresholds, tierCount));
	}

	[Test]
	public void ValueForTier_UsesValuePerTier()
	{
		Assert.AreEqual(1, ValueForTier(_config, 0));
		Assert.AreEqual(3, ValueForTier(_config, 2));
		Assert.AreEqual(999, ValueForTier(_config, 3));
	}

	[Test]
	public void ThresholdsReached_DefaultThresholds()
	{
		int[] minimums = { 200, 400, 600 };

		Assert.AreEqual(0, ThresholdsReached(199, minimums));
		Assert.AreEqual(1, ThresholdsReached(200, minimums));
		Assert.AreEqual(2, ThresholdsReached(400, minimums));
		Assert.AreEqual(3, ThresholdsReached(600, minimums));
	}

	[Test]
	public void ThresholdsReached_IosThresholds()
	{
		int[] minimums = { 300, 600 };

		Assert.AreEqual(0, ThresholdsReached(299, minimums));
		Assert.AreEqual(1, ThresholdsReached(300, minimums));
		Assert.AreEqual(2, ThresholdsReached(600, minimums));
	}

	[Test]
	public void ThresholdsReached_NullIsZero()
	{
		Assert.AreEqual(0, ThresholdsReached(1000, null));
	}

	[Test]
	public void RandomLayerIndex_NeverReachesTheLastReachedIndex()
	{
		for (int i = 0; i < 1000; i++)
		{
			int index = RandomLayerIndex(3);
			Assert.GreaterOrEqual(index, 0);
			Assert.LessOrEqual(index, 1);
		}
	}

	[Test]
	public void RandomLayerIndex_OneOrZeroReachedIsZero()
	{
		Assert.AreEqual(0, RandomLayerIndex(0));
		Assert.AreEqual(0, RandomLayerIndex(1));
		for (int i = 0; i < 50; i++)
		{
			Assert.AreEqual(0, RandomLayerIndex(2));
		}
	}

	[Test]
	public void CapVelocity_ClampsMagnitudeAndKeepsDirection()
	{
		Vector3 capped = CapVelocity(new Vector3(3f, 4f, 0f), 2.5f);

		Assert.AreEqual(2.5f, capped.magnitude, 0.0001f);
		Assert.AreEqual(1.5f, capped.x, 0.0001f);
		Assert.AreEqual(2.0f, capped.y, 0.0001f);
	}

	[Test]
	public void CapVelocity_UnderCapIsUnchanged()
	{
		Vector3 velocity = new Vector3(3f, 4f, 0f);

		Assert.AreEqual(velocity, CapVelocity(velocity, 10f));
	}

	[Test]
	public void CapVelocity_ZeroCapMeansNoCap()
	{
		Vector3 velocity = new Vector3(30f, 40f, 0f);

		Assert.AreEqual(velocity, CapVelocity(velocity, 0f));
	}

	[Test]
	public void CloneCount_IsValueMinusOne()
	{
		Assert.AreEqual(0, MultiplyClones(0));
		Assert.AreEqual(0, MultiplyClones(1));
		Assert.AreEqual(1, MultiplyClones(2));
		Assert.AreEqual(4, MultiplyClones(5));
	}

	[Test]
	public void TryRemember_IgnoreListGrowsAndKeepsOldest()
	{
		var list = new List<int>();

		for (int i = 1; i <= 15; i++)
		{
			TryRemember(list, i);
		}

		Assert.AreEqual(15, list.Count);
		Assert.IsTrue(list.Contains(1), "oldest entry is kept");
		Assert.IsTrue(list.Contains(15));
	}

	[Test]
	public void TryRemember_RejectsDuplicates()
	{
		var list = new List<int>();

		Assert.IsTrue(TryRemember(list, 7));
		Assert.IsFalse(TryRemember(list, 7));
		Assert.AreEqual(1, list.Count);
	}

	[Test]
	public void SizeForRoll_IsUniformSmallMediumBig()
	{
		Assert.AreEqual(BallSize.Small, SizeForRoll(0));
		Assert.AreEqual(BallSize.Medium, SizeForRoll(1));
		Assert.AreEqual(BallSize.Big, SizeForRoll(2));
	}

	[Test]
	public void SelectConfig_PicksByPlatform()
	{
		BallsConfig defaultConfig = ScriptableObject.CreateInstance<BallsConfig>();
		BallsConfig ios = ScriptableObject.CreateInstance<BallsConfig>();

		Assert.AreSame(ios, SelectConfig(RuntimePlatform.IPhonePlayer, defaultConfig, ios));
		Assert.AreSame(defaultConfig, SelectConfig(RuntimePlatform.WindowsPlayer, defaultConfig, ios));
		Assert.AreSame(ios, SelectConfig(RuntimePlatform.WindowsPlayer, null, ios));

		Object.DestroyImmediate(defaultConfig);
		Object.DestroyImmediate(ios);
	}

	private static T Call<T>(string name, params object[] args)
	{
		MethodInfo method = typeof(BallsController).GetMethod(name, StaticFlags);
		Assert.IsNotNull(method, "missing helper " + name);
		return (T)method.Invoke(null, args);
	}

	private static int ValueTierIndex(int total, int[] thresholds, int tierCount)
	{
		return Call<int>("ValueTierIndex", total, thresholds, tierCount);
	}

	private static int ValueForTier(BallsConfig config, int tierIndex)
	{
		return Call<int>("ValueForTier", config, tierIndex);
	}

	private static int ThresholdsReached(int count, int[] minimums)
	{
		return Call<int>("ThresholdsReached", count, minimums);
	}

	private static int RandomLayerIndex(int reached)
	{
		return Call<int>("RandomLayerIndex", reached);
	}

	private static Vector3 CapVelocity(Vector3 velocity, float maxSpeed)
	{
		return Call<Vector3>("CapVelocity", velocity, maxSpeed);
	}

	private static int MultiplyClones(int gateValue)
	{
		System.Type type = typeof(Multiplicator);
		MethodInfo method = type.GetMethod("MultiplyClones", StaticFlags);
		Assert.IsNotNull(method, "missing helper Multiplicator.MultiplyClones");
		return (int)method.Invoke(null, new object[] { gateValue });
	}

	private static BallSize SizeForRoll(int roll)
	{
		return Call<BallSize>("SizeForRoll", roll);
	}

	private static BallsConfig SelectConfig(RuntimePlatform platform, BallsConfig defaultConfig, BallsConfig ios)
	{
		return Call<BallsConfig>("SelectConfig", platform, defaultConfig, ios);
	}

	private static bool TryRemember<T>(List<T> list, T item)
	{
		MethodInfo method = typeof(BallsController).GetMethod("TryRemember", StaticFlags);
		Assert.IsNotNull(method, "missing helper TryRemember");
		MethodInfo generic = method.MakeGenericMethod(typeof(T));
		return (bool)generic.Invoke(null, new object[] { list, item });
	}
}
