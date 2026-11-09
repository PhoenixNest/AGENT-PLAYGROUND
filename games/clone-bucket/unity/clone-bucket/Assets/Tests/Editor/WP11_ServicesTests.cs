using System;
using NUnit.Framework;
using UnityEngine;

public class WP11_ServicesTests
{
	[Test]
	public void RemoteConfig_StandIn_IsReady()
	{
		Assert.IsTrue(RemoteConfigHelper.IsRemoteReady());
	}

	[Test]
	public void RemoteConfig_StandIn_GettersReturnDefaults()
	{
		Assert.AreEqual(7, RemoteConfigHelper.GetInt("wp11.key", 7));
		Assert.IsTrue(RemoteConfigHelper.GetBool("wp11.key", true));
		Assert.IsFalse(RemoteConfigHelper.GetBool("wp11.key", false));
		Assert.AreEqual(1.5f, RemoteConfigHelper.GetFloat("wp11.key", 1.5f));
		Assert.AreEqual("fallback", RemoteConfigHelper.GetString("wp11.key", "fallback"));
		Assert.AreEqual(string.Empty, RemoteConfigHelper.GetString("wp11.key"));
	}

	[Test]
	public void RemoteConfig_OnRemoteReady_CalledOnceOnSubscribe()
	{
		int calls = 0;
		bool carryOn = false;
		Action<bool> handler = ok =>
		{
			calls++;
			carryOn = ok;
		};

		RemoteConfigHelper.OnRemoteReady += handler;
		try
		{
			Assert.AreEqual(1, calls);
			Assert.IsTrue(carryOn);
		}
		finally
		{
			RemoteConfigHelper.OnRemoteReady -= handler;
		}
	}

	[Test]
	public void ABTestVariables_DefaultsAreFalse()
	{
		GameObject go = new GameObject("WP11_ABTest");
		try
		{
			ABTestVariables flags = go.AddComponent<ABTestVariables>();
			Assert.IsFalse(flags.IsCrazyLabsThemes());
			Assert.IsFalse(flags.HasNoObstacles());
			Assert.IsFalse(flags.IsMixedLevels());
			Assert.IsFalse(flags.WithBossLevels());
			Assert.IsFalse(flags.BossLevelEveryFive());
			Assert.IsFalse(flags.WhiteBalls());
			Assert.IsFalse(flags.GetExtraBalls());
			Assert.IsFalse(flags.GetBetterThanScore());
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
