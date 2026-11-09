using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WP5_ChainsTests
{
	private GameObject host;
	private Chain chain;

	[SetUp]
	public void SetUp()
	{
		host = new GameObject("WP5_ChainHost");
		chain = host.AddComponent<Chain>();
		chain.UnlockNumber = 3;
	}

	[TearDown]
	public void TearDown()
	{
		if (host != null)
		{
			Object.DestroyImmediate(host);
		}
	}

	[Test]
	public void LockOpensAtExactlyUnlockNumber()
	{
		chain.AddBall(1);
		chain.AddBall(1);
		Assert.AreEqual(2, chain.ChainCount);
		Assert.IsFalse(IsUnlocked(chain), "Locked after 2 of 3 balls.");

		chain.AddBall(1);
		Assert.AreEqual(3, chain.ChainCount);
		Assert.IsTrue(IsUnlocked(chain), "Unlocked on the 3rd ball.");
	}

	[Test]
	public void LockStaysOpenForTheLevel()
	{
		for (int i = 0; i < 5; i++)
		{
			chain.AddBall(1);
		}

		Assert.IsTrue(IsUnlocked(chain));
		Assert.AreEqual(5, chain.ChainCount);
	}

	[Test]
	public void BallValueIsAddedOnEnterAndRemovedOnExit()
	{
		chain.AddBall(3);
		Assert.AreEqual(3, chain.ChainCount);
		Assert.IsTrue(IsUnlocked(chain), "A value-3 ball meets a cost of 3.");

		chain.RemoveBall(2);
		Assert.AreEqual(1, chain.ChainCount, "Leaving the chain with value 2 breaks it down by 2.");
		Assert.IsTrue(IsUnlocked(chain), "Unlock is not undone by a break.");
	}

	[Test]
	public void CounterFractionIsClampedToUnitRange()
	{
		Assert.AreEqual(0f, Chain.CounterFraction(0, 40));
		Assert.AreEqual(0f, Chain.CounterFraction(-3, 40), "Negative counts clamp to 0.");
		Assert.AreEqual(0.5f, Chain.CounterFraction(20, 40), 1e-6f);
		Assert.AreEqual(1f, Chain.CounterFraction(40, 40));
		Assert.AreEqual(1f, Chain.CounterFraction(55, 40), "Counts above the cost clamp to 1.");
		Assert.AreEqual(1f, Chain.CounterFraction(0, 0), "A cost of 0 shows full.");
	}

	[Test]
	public void ReachesUnlockMatchesCostBoundary()
	{
		Assert.IsFalse(Chain.ReachesUnlock(39, 40));
		Assert.IsTrue(Chain.ReachesUnlock(40, 40));
		Assert.IsTrue(Chain.ReachesUnlock(41, 40));
	}

	private static bool IsUnlocked(Chain target)
	{
		FieldInfo field = typeof(Chain).GetField("unlocked", BindingFlags.NonPublic | BindingFlags.Instance);
		return (bool)field.GetValue(target);
	}

}
