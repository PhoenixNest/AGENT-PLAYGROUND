using System;
using System.Reflection;
using NUnit.Framework;

public class WP6_BucketTests
{
	private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

	private static T Invoke<T>(Type type, string name, params object[] args)
	{
		MethodInfo method = type.GetMethod(name, StaticFlags);
		Assert.IsNotNull(method, type.Name + "." + name + " not found");
		return (T)method.Invoke(null, args);
	}

	[Test]
	public void Remap_IsLinear_AtEndsAndMiddle()
	{
		Assert.AreEqual(0.5f, Invoke<float>(typeof(ChunkStarBar), "RemapLinear", 5f, 0f, 10f, 0f, 1f), 0.0001f);
		Assert.AreEqual(2f, Invoke<float>(typeof(ChunkStarBar), "RemapLinear", 0f, 0f, 10f, 2f, 4f), 0.0001f);
		Assert.AreEqual(4f, Invoke<float>(typeof(ChunkStarBar), "RemapLinear", 10f, 0f, 10f, 2f, 4f), 0.0001f);
		Assert.AreEqual(3f, Invoke<float>(typeof(ChunkStarBar), "RemapLinear", 5f, 0f, 10f, 2f, 4f), 0.0001f);
	}

	[Test]
	public void Remap_SupportsReversedOutputRange()
	{
		Assert.AreEqual(50f, Invoke<float>(typeof(ChunkStarBar), "RemapLinear", 5f, 0f, 10f, 100f, 0f), 0.0001f);
	}

	[Test]
	public void Remap_EmptyInputRange_ReturnsOutputStart()
	{
		Assert.AreEqual(7f, Invoke<float>(typeof(ChunkStarBar), "RemapLinear", 3f, 4f, 4f, 7f, 9f), 0.0001f);
	}

	[Test]
	public void ProgressBar_ClampsBelowZeroAndAboveOne()
	{
		Assert.AreEqual(0f, Invoke<float>(typeof(ProgressBar), "ClampProgress", -0.5f), 0.0001f);
		Assert.AreEqual(1f, Invoke<float>(typeof(ProgressBar), "ClampProgress", 1.7f), 0.0001f);
	}

	[Test]
	public void ProgressBar_KeepsValuesInside()
	{
		Assert.AreEqual(0.3f, Invoke<float>(typeof(ProgressBar), "ClampProgress", 0.3f), 0.0001f);
		Assert.AreEqual(0f, Invoke<float>(typeof(ProgressBar), "ClampProgress", 0f), 0.0001f);
		Assert.AreEqual(1f, Invoke<float>(typeof(ProgressBar), "ClampProgress", 1f), 0.0001f);
	}

	[Test]
	public void ProgressBar_NaNBecomesZero()
	{
		Assert.AreEqual(0f, Invoke<float>(typeof(ProgressBar), "ClampProgress", float.NaN), 0.0001f);
	}

	[Test]
	public void CollectEffect_NeedsTimerAboveFifteenHundredths()
	{
		Assert.IsFalse(Invoke<bool>(typeof(Bucket), "ShouldSpawnCollectFx", false, 0f));
		Assert.IsFalse(Invoke<bool>(typeof(Bucket), "ShouldSpawnCollectFx", true, 0.1f));
		Assert.IsFalse(Invoke<bool>(typeof(Bucket), "ShouldSpawnCollectFx", true, 0.15f), "Exactly 0.15 does not show.");
		Assert.IsTrue(Invoke<bool>(typeof(Bucket), "ShouldSpawnCollectFx", true, 0.151f));
		Assert.IsTrue(Invoke<bool>(typeof(Bucket), "ShouldSpawnCollectFx", true, 1f));
	}

	[Test]
	public void StarEffect_RespectsCooldown()
	{
		Assert.IsFalse(Invoke<bool>(typeof(ChunkStarBar), "IsStarEffectReady", 1.0f, 0.8f, 0.25f));
		Assert.IsFalse(Invoke<bool>(typeof(ChunkStarBar), "IsStarEffectReady", 1.0f, 0.75f, 0.25f), "Exactly the cooldown does not show.");
		Assert.IsTrue(Invoke<bool>(typeof(ChunkStarBar), "IsStarEffectReady", 1.0f, 0.7f, 0.25f));
	}

	[Test]
	public void CreditArrow_MapsPercentAcrossWidth()
	{
		Assert.AreEqual(-100f, Invoke<float>(typeof(CreditScoreBar), "ArrowXForPercent", 0, 200f), 0.0001f);
		Assert.AreEqual(0f, Invoke<float>(typeof(CreditScoreBar), "ArrowXForPercent", 50, 200f), 0.0001f);
		Assert.AreEqual(100f, Invoke<float>(typeof(CreditScoreBar), "ArrowXForPercent", 100, 200f), 0.0001f);
		Assert.AreEqual(100f, Invoke<float>(typeof(CreditScoreBar), "ArrowXForPercent", 150, 200f), 0.0001f);
	}
}
