using System;
using System.Collections.Generic;
using CustomEventSystem;
using NUnit.Framework;

public class WP10_MiscTests
{
	[TearDown]
	public void TearDown()
	{
		EventManager.ClearAllListeners();
	}

	[Test]
	public void ToSI_SmallNumbersHaveNoSuffix()
	{
		Assert.AreEqual("0", UIUtils.ToSI(0));
		Assert.AreEqual("999", UIUtils.ToSI(999));
		Assert.AreEqual("-15", UIUtils.ToSI(-15));
	}

	[Test]
	public void ToSI_OneDecimalBelowTenOfAUnit()
	{
		Assert.AreEqual("1.2K", UIUtils.ToSI(1200));
		Assert.AreEqual("1.5K", UIUtils.ToSI(1500));
		Assert.AreEqual("2.5B", UIUtils.ToSI(2500000000d));
	}

	[Test]
	public void ToSI_WholeNumbersFromTenOfAUnit()
	{
		Assert.AreEqual("12K", UIUtils.ToSI(12000));
		Assert.AreEqual("123K", UIUtils.ToSI(123456));
		Assert.AreEqual("999K", UIUtils.ToSI(999000));
	}

	[Test]
	public void ToSI_RoundingCarriesIntoTheNextUnit()
	{
		Assert.AreEqual("1M", UIUtils.ToSI(999999));
		Assert.AreEqual("1M", UIUtils.ToSI(1000000));
	}

	[Test]
	public void ToSI_KeepsTheSignForNegativeValues()
	{
		Assert.AreEqual("-1.5K", UIUtils.ToSI(-1500));
	}

	[Test]
	public void Shuffle_YieldsAPermutationOfTheSameItems()
	{
		var list = new List<int>();
		for (int i = 0; i < 50; i++)
		{
			list.Add(i);
		}
		var before = list.ToArray();

		list.Shuffle();

		Assert.AreEqual(before.Length, list.Count);
		CollectionAssert.AreEquivalent(before, list);
	}

	[Test]
	public void Shuffle_EmptyAndSingleItemListsAreUnchanged()
	{
		var empty = new List<string>();
		empty.Shuffle();
		Assert.AreEqual(0, empty.Count);

		var single = new List<string> { "only" };
		single.Shuffle();
		CollectionAssert.AreEqual(new[] { "only" }, single);
	}

	[Test]
	public void AudioVoicePool_SameVoiceFreeAfterCooldown()
	{
		var pool = new float[] { float.NegativeInfinity };
		Assert.IsTrue(AudioManager.CanPlayAt(pool, 1.00f, 0.1f));
		Assert.IsFalse(AudioManager.CanPlayAt(pool, 1.05f, 0.1f), "single voice is busy inside 0.1 s");
		Assert.IsTrue(AudioManager.CanPlayAt(pool, 1.11f, 0.1f), "voice is free after 0.1 s");
	}

	[Test]
	public void AudioVoicePool_BallPoolHoldsFiveSimultaneousSounds()
	{
		var pool = new float[5];
		for (int i = 0; i < 5; i++)
		{
			Assert.IsTrue(AudioManager.CanPlayAt(pool, 2.0f, 0.1f), "voice " + i);
		}

		Assert.IsFalse(AudioManager.CanPlayAt(pool, 2.0f, 0.1f), "sixth sound in the same instant is dropped");
	}

	[Test]
	public void AudioVoicePool_BucketPoolHoldsTwo()
	{
		var pool = new float[2];
		Assert.IsTrue(AudioManager.CanPlayAt(pool, 3.0f, 0.1f));
		Assert.IsTrue(AudioManager.CanPlayAt(pool, 3.0f, 0.1f));
		Assert.IsFalse(AudioManager.CanPlayAt(pool, 3.0f, 0.1f));
	}

	[Test]
	public void AudioVoicePool_NullPoolNeverPlays()
	{
		Assert.IsFalse(AudioManager.CanPlayAt(null, 1f, 0.1f));
	}

	[Test]
	public void HapticGate_PulseAllowedOnlyAfterFiftyMilliseconds()
	{
		Assert.IsTrue(HapticManager.CanPulse(0.05f));
		Assert.IsTrue(HapticManager.CanPulse(0.5f));
		Assert.IsFalse(HapticManager.CanPulse(0f));
		Assert.IsFalse(HapticManager.CanPulse(0.049f));
	}

	[Test]
	public void HapticGate_TimerAccumulatesScaledTimeAndIgnoresNegativeSteps()
	{
		Assert.AreEqual(0.03f, HapticManager.TickTimer(0f, 0.03f), 0.0001f);
		Assert.AreEqual(0.03f, HapticManager.TickTimer(0.03f, -1f), 0.0001f);
	}

	[Test]
	public void HapticGate_FiftyMillisecondsBetweenPulses()
	{
		float timer = 0f;
		timer = HapticManager.TickTimer(timer, 0.02f);
		Assert.IsFalse(HapticManager.CanPulse(timer));
		timer = HapticManager.TickTimer(timer, 0.04f);
		Assert.IsTrue(HapticManager.CanPulse(timer));
	}
	[Test]
	public void EventManager_AddAndTriggerInvokesTheListener()
	{
		int count = 0;
		Action listener = () => count++;

		EventManager.AddListener("wp10.plain", listener);
		EventManager.TriggerEvent("wp10.plain");

		Assert.AreEqual(1, count);
	}

	[Test]
	public void EventManager_RemoveStopsInvocation()
	{
		int count = 0;
		Action listener = () => count++;

		EventManager.AddListener("wp10.remove", listener);
		EventManager.RemoveListener("wp10.remove", listener);
		EventManager.TriggerEvent("wp10.remove");

		Assert.AreEqual(0, count);
	}

	[Test]
	public void EventManager_SameListenerAddedTwiceRunsOnce()
	{
		int count = 0;
		Action listener = () => count++;

		EventManager.AddListener("wp10.twice", listener);
		EventManager.AddListener("wp10.twice", listener);
		EventManager.TriggerEvent("wp10.twice");

		Assert.AreEqual(1, count);
	}

	[Test]
	public void EventManager_GenericPayloadIsDelivered()
	{
		int received = 0;

		EventManager.AddListener<int>("wp10.generic", value => received = value);
		EventManager.TriggerEvent("wp10.generic", 7);

		Assert.AreEqual(7, received);
	}

	[Test]
	public void EventManager_EventDataPayloadIsDelivered()
	{
		string name = null;

		EventManager.AddListener("wp10.data", (EventData data) => name = data.EventName);
		EventManager.TriggerEvent("wp10.data", new EventData("wp10.data", this));

		Assert.AreEqual("wp10.data", name);
	}

	[Test]
	public void EventManager_TriggerWithoutListenersDoesNotThrow()
	{
		Assert.DoesNotThrow(() => EventManager.TriggerEvent("wp10.nobody"));
		Assert.DoesNotThrow(() => EventManager.TriggerEvent("wp10.nobody", 1));
	}

	[Test]
	public void EventManager_ClearAllListenersRemovesEverything()
	{
		int count = 0;
		Action listener = () => count++;
		EventManager.AddListener("wp10.clear", listener);

		EventManager.ClearAllListeners();
		EventManager.TriggerEvent("wp10.clear");

		Assert.AreEqual(0, count);
	}
}
