using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class WP4_GatesTests
{
	private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();

	[TearDown]
	public void TearDown()
	{
		for (int i = 0; i < _created.Count; i++)
		{
			if (_created[i] != null)
			{
				UnityEngine.Object.DestroyImmediate(_created[i]);
			}
		}
		_created.Clear();
	}

	private static object CallStatic(Type type, string name, params object[] args)
	{
		MethodInfo method = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		Assert.IsNotNull(method, "Missing helper " + type.Name + "." + name);
		return method.Invoke(null, args);
	}

	private static int MultiplyClones(int gateValue)
	{
		return (int)CallStatic(typeof(Multiplicator), "MultiplyClones", gateValue);
	}

	private static bool SubtractConsumes(ref int remaining)
	{
		object[] args = { remaining };
		bool result = (bool)CallStatic(typeof(SubGate), "SubtractConsumes", args);
		remaining = (int)args[0];
		return result;
	}

	private static bool DivisionHit(ref int counter, int divisor)
	{
		object[] args = { counter, divisor };
		bool result = (bool)CallStatic(typeof(DivideGate), "DivisionHit", args);
		counter = (int)args[0];
		return result;
	}

	private static float ApplyGateValue(float balls, GateType type, int value)
	{
		return (float)CallStatic(typeof(GatesController), "ApplyGateValue", balls, type, value);
	}

	private static int GetGateRollSeed(string name, int index)
	{
		return (int)CallStatic(typeof(GatesController), "GetGateRollSeed", name, index);
	}

	private static GateBase ChooseSpikeGate(float balls, GateBase a, GateBase b)
	{
		return (GateBase)CallStatic(typeof(GatesController), "ChooseSpikeGate", balls, a, b);
	}

	private static int CalculateMaxPossibleBalls(int start, IReadOnlyList<GateBase> gates)
	{
		return (int)CallStatic(typeof(GatesController), "CalculateMaxPossibleBalls", start, gates, null);
	}

	private Multiplicator MakeMultiplicator(int value, float x = 0f)
	{
		GameObject go = new GameObject("WP4_Mult");
		_created.Add(go);
		go.transform.localPosition = new Vector3(x, 0f, 0f);
		Multiplicator gate = go.AddComponent<Multiplicator>();
		gate.SetValue(value);
		return gate;
	}

	private SubGate MakeSubGate(int value, float x = 0f)
	{
		GameObject go = new GameObject("WP4_Sub");
		_created.Add(go);
		go.transform.localPosition = new Vector3(x, 0f, 0f);
		SubGate gate = go.AddComponent<SubGate>();
		gate.SetValue(value);
		return gate;
	}

	private DivideGate MakeDivideGate(int value, float x = 0f)
	{
		GameObject go = new GameObject("WP4_Div");
		_created.Add(go);
		go.transform.localPosition = new Vector3(x, 0f, 0f);
		DivideGate gate = go.AddComponent<DivideGate>();
		gate.SetValue(value);
		return gate;
	}

	[Test]
	public void Multiply_GivesValueMinusOneExtraUnitsPerUnitOfValue()
	{
		Assert.AreEqual(1, MultiplyClones(2));
		Assert.AreEqual(2, MultiplyClones(3));
		Assert.AreEqual(4, MultiplyClones(5));
		Assert.AreEqual(19, MultiplyClones(20));
	}

	[Test]
	public void Multiply_NoExtraUnitsForValueOneOrLess()
	{
		Assert.AreEqual(0, MultiplyClones(1));
		Assert.AreEqual(0, MultiplyClones(0));
	}

	[Test]
	public void Multiply_CountAfterGateIsBallsTimesValue()
	{
		Assert.AreEqual(12f, ApplyGateValue(3f, GateType.Multiply, 4), 1e-4f);
		Assert.AreEqual(1 + MultiplyClones(4), 4);
	}

	[Test]
	public void Subtract_RemovesExactlyValueBallsThenPasses()
	{
		int remaining = 5;
		int removed = 0;
		for (int i = 0; i < 8; i++)
		{
			if (SubtractConsumes(ref remaining))
			{
				removed++;
			}
		}
		Assert.AreEqual(5, removed, "balls removed");
		Assert.AreEqual(0, remaining, "value left");
	}

	[Test]
	public void Subtract_ValueZeroPassesEveryBall()
	{
		int remaining = 0;
		Assert.IsFalse(SubtractConsumes(ref remaining));
		Assert.IsFalse(SubtractConsumes(ref remaining));
		Assert.AreEqual(0, remaining);
	}

	[Test]
	public void Subtract_CountAfterGateClampsAtZero()
	{
		Assert.AreEqual(25f, ApplyGateValue(30f, GateType.Subtract, 5), 1e-4f);
		Assert.AreEqual(0f, ApplyGateValue(3f, GateType.Subtract, 5), 1e-4f);
	}

	[Test]
	public void SubGate_Sub_ConsumesUntilEmptyThenPasses()
	{
		SubGate gate = MakeSubGate(2);

		Assert.IsTrue(gate.Sub());
		Assert.AreEqual(1, gate.Value);
		Assert.IsTrue(gate.Sub());
		Assert.AreEqual(0, gate.Value);
		Assert.IsFalse(gate.Sub());
		Assert.AreEqual(0, gate.Value);
	}

	[Test]
	public void Divide_EveryVthBallIsDividedOthersPass()
	{
		int counter = 0;
		var divided = new List<int>();
		for (int ball = 1; ball <= 9; ball++)
		{
			if (DivisionHit(ref counter, 3))
			{
				divided.Add(ball);
			}
		}
		CollectionAssert.AreEqual(new[] { 3, 6, 9 }, divided);
	}

	[Test]
	public void Divide_ValueTwoDividesEverySecondBall()
	{
		int counter = 0;
		bool[] expected = { false, true, false, true };
		for (int i = 0; i < expected.Length; i++)
		{
			Assert.AreEqual(expected[i], DivisionHit(ref counter, 2), "ball " + (i + 1));
		}
	}

	[Test]
	public void Divide_ZeroValueNeverDivides()
	{
		int counter = 0;
		for (int i = 0; i < 5; i++)
		{
			Assert.IsFalse(DivisionHit(ref counter, 0));
		}
		Assert.AreEqual(5, counter, "the counter still counts every ball");
	}

	[Test]
	public void Divide_NegativeValueDividesOnMultiplesOfMagnitude()
	{
		int counter = 0;
		bool[] expected = { false, true, false, true };
		for (int i = 0; i < expected.Length; i++)
		{
			Assert.AreEqual(expected[i], DivisionHit(ref counter, -2), "ball " + (i + 1));
		}
	}

	[Test]
	public void Divide_GateDividesSecondBallAndReportsPassing()
	{
		DivideGate gate = MakeDivideGate(2);

		gate.Divide(null);
		Assert.IsTrue(gate.LastBallPassed);
		gate.Divide(null);
		Assert.IsFalse(gate.LastBallPassed);
	}

	[Test]
	public void ApplyGate_DivideByZeroLeavesCountUnchanged()
	{
		Assert.AreEqual(10f, ApplyGateValue(10f, GateType.Divide, 0), 1e-4f);
		Assert.AreEqual(5f, ApplyGateValue(10f, GateType.Divide, 2), 1e-4f);
	}

	[Test]
	public void ApplyGate_ChainFollowsTableOrder()
	{
		float balls = 10f;
		balls = ApplyGateValue(balls, GateType.Multiply, 3);
		Assert.AreEqual(30f, balls, 1e-4f);
		balls = ApplyGateValue(balls, GateType.Subtract, 5);
		Assert.AreEqual(25f, balls, 1e-4f);
		balls = ApplyGateValue(balls, GateType.Divide, 2);
		Assert.AreEqual(12.5f, balls, 1e-4f);
	}

	[Test]
	public void ChooseSpikeGate_MultiplicatorWinsOverSubtract()
	{
		Multiplicator mult = MakeMultiplicator(2);
		SubGate sub = MakeSubGate(1);

		Assert.AreSame(mult, ChooseSpikeGate(10f, sub, mult));
		Assert.AreSame(mult, ChooseSpikeGate(10f, mult, sub));
	}

	[Test]
	public void ChooseSpikeGate_TwoMultiplicatorsLargerValueWinsTieGoesToA()
	{
		Multiplicator x2 = MakeMultiplicator(2);
		Multiplicator x3 = MakeMultiplicator(3);
		Multiplicator x3b = MakeMultiplicator(3);

		Assert.AreSame(x3, ChooseSpikeGate(10f, x2, x3));
		Assert.AreSame(x3, ChooseSpikeGate(10f, x3, x2));
		Assert.AreSame(x3, ChooseSpikeGate(10f, x3, x3b), "tie goes to a");
	}

	[Test]
	public void ChooseSpikeGate_TwoSubtractsLargerResultWins()
	{
		SubGate minus3 = MakeSubGate(3);
		SubGate minus5 = MakeSubGate(5);

		Assert.AreSame(minus3, ChooseSpikeGate(10f, minus3, minus5));
		Assert.AreSame(minus3, ChooseSpikeGate(10f, minus5, minus3));
	}

	[Test]
	public void ChooseSpikeGate_DivideAgainstSubtractUsesResultingCount()
	{
		DivideGate half = MakeDivideGate(2);
		SubGate minus3 = MakeSubGate(3);

		Assert.AreSame(minus3, ChooseSpikeGate(10f, half, minus3));
	}

	[Test]
	public void CalculateMaxPossibleBalls_SpikePairUsesChosenGate()
	{
		Multiplicator mult = MakeMultiplicator(2, 0f);
		SubGate sub = MakeSubGate(3, 0f);

		Assert.AreEqual(20, CalculateMaxPossibleBalls(10, new List<GateBase> { mult, sub }));
	}

	[Test]
	public void CalculateMaxPossibleBalls_SeparatedGatesApplyInOrder()
	{
		Multiplicator mult = MakeMultiplicator(2, 0f);
		SubGate sub = MakeSubGate(3, 5f);

		Assert.AreEqual(17, CalculateMaxPossibleBalls(10, new List<GateBase> { mult, sub }));
	}

	[Test]
	public void CalculateMaxPossibleBalls_RoundsHalvesToEven()
	{
		DivideGate half = MakeDivideGate(2, 0f);
		Assert.AreEqual(2, CalculateMaxPossibleBalls(5, new List<GateBase> { half }));
		Assert.AreEqual(4, CalculateMaxPossibleBalls(7, new List<GateBase> { half }));
	}

	[Test]
	public void MultiplicatorLabel_IsCapitalXThenValue()
	{
		Assert.AreEqual("X6", (string)CallStatic(typeof(Multiplicator), "ValueLabel", 6));
	}

	[Test]
	public void SubGateLabel_IsMinusThenRemainingValueIncludingZero()
	{
		Assert.AreEqual("-3", (string)CallStatic(typeof(SubGate), "ValueLabel", 3));
		Assert.AreEqual("-0", (string)CallStatic(typeof(SubGate), "ValueLabel", 0));
	}

	[Test]
	public void GateRollSeed_IsStableForSameInput()
	{
		Assert.AreEqual(GetGateRollSeed("Level1", 3), GetGateRollSeed("Level1", 3));
	}

	[Test]
	public void GateRollSeed_ChangesWithIndexAndName()
	{
		Assert.AreNotEqual(GetGateRollSeed("Level1", 0), GetGateRollSeed("Level1", 1));
		Assert.AreNotEqual(GetGateRollSeed("Level1", 0), GetGateRollSeed("Level2", 0));
	}

	[Test]
	public void GateRollSeed_MatchesRecoveredArithmetic()
	{
		Assert.AreEqual(568912, GetGateRollSeed("A", 0));
		Assert.AreEqual(568913, GetGateRollSeed("A", 1));
		Assert.AreEqual(16337, GetGateRollSeed("", 0));
	}

	[Test]
	public void NeedsRespawn_OnlyWhenTheGateTypeDiffersFromTheRow()
	{
		Assert.IsTrue(GatesController.NeedsRespawn(GateType.Multiply, GateType.Subtract));
		Assert.IsTrue(GatesController.NeedsRespawn(GateType.Divide, GateType.Multiply));
		Assert.IsFalse(GatesController.NeedsRespawn(GateType.Subtract, GateType.Subtract));
	}
}
