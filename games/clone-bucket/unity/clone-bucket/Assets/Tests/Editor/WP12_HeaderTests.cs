using System;
using System.Reflection;
using Levels;
using NUnit.Framework;

public class WP12_HeaderTests
{
	private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

	private static object Call(Type type, string name, object[] args)
	{
		MethodInfo method = type.GetMethod(name, StaticFlags);
		Assert.IsNotNull(method, type.Name + "." + name + " not found");
		return method.Invoke(null, args);
	}

	private static float? BarFill(int stars, int filled, int target, int second, int perfect)
	{
		return (float?)Call(typeof(ChunkStarBar), "FillAmount", new object[] { stars, filled, target, second, perfect });
	}

	private static int Stage(int filled, int target, int perfect, out float progress, out float secondProgress)
	{
		object[] args = { filled, target, perfect, 0f, 0f };
		int stage = (int)Call(typeof(LevelsController), "StarStage", args);
		progress = (float)args[3];
		secondProgress = (float)args[4];
		return stage;
	}

	[Test]
	public void StarBar_FirstSegmentFillsAThirdUpToTarget()
	{
		Assert.AreEqual(0f, BarFill(0, 0, 60, 85, 110).Value, 0.0001f);
		Assert.AreEqual(0.165f, BarFill(0, 30, 60, 85, 110).Value, 0.0001f);
		Assert.AreEqual(0.33f, BarFill(0, 60, 60, 85, 110).Value, 0.0001f);
	}

	[Test]
	public void StarBar_SecondSegmentRunsFromTargetToSecondStar()
	{
		Assert.AreEqual(0.33f, BarFill(1, 60, 60, 85, 110).Value, 0.0001f);
		Assert.AreEqual(0.66f, BarFill(1, 85, 60, 85, 110).Value, 0.0001f);
	}

	[Test]
	public void StarBar_LastSegmentRunsFromSecondStarToPerfect()
	{
		Assert.AreEqual(0.66f, BarFill(2, 85, 60, 85, 110).Value, 0.0001f);
		Assert.AreEqual(1f, BarFill(2, 110, 60, 85, 110).Value, 0.0001f);
	}

	[Test]
	public void StarBar_ThreeStarsOrEmptyRangesLeaveTheFillAlone()
	{
		Assert.IsNull(BarFill(3, 120, 60, 85, 110));
		Assert.IsNull(BarFill(0, 10, 0, 85, 110));
		Assert.IsNull(BarFill(1, 70, 60, 60, 110));
	}

	[Test]
	public void StarBar_ShowsEachStarOnceInOrder()
	{
		Assert.IsTrue((bool)Call(typeof(ChunkStarBar), "ShouldShowStar", new object[] { 1, 0 }));
		Assert.IsFalse((bool)Call(typeof(ChunkStarBar), "ShouldShowStar", new object[] { 1, 1 }));
		Assert.IsTrue((bool)Call(typeof(ChunkStarBar), "ShouldShowStar", new object[] { 3, 2 }));
		Assert.IsFalse((bool)Call(typeof(ChunkStarBar), "ShouldShowStar", new object[] { 2, 0 }), "The original skips a star when the count jumps.");
		Assert.IsFalse((bool)Call(typeof(ChunkStarBar), "ShouldShowStar", new object[] { 0, 0 }));
	}

	[Test]
	public void StarStage_UpToTargetIsStageZero()
	{
		Assert.AreEqual(0, Stage(30, 60, 110, out float progress, out _));
		Assert.AreEqual(0.5f, progress, 0.0001f);
		Assert.AreEqual(0, Stage(60, 60, 110, out progress, out _));
		Assert.AreEqual(1f, progress, 0.0001f);
	}

	[Test]
	public void StarStage_HalfwayToPerfectIsStageOne()
	{
		Assert.AreEqual(1, Stage(70, 60, 110, out float progress, out _));
		Assert.AreEqual(0.4f, progress, 0.0001f);
		Assert.AreEqual(1, Stage(85, 60, 110, out progress, out _));
		Assert.AreEqual(1f, progress, 0.0001f);
	}

	[Test]
	public void StarStage_AboveHalfwayIsStageTwo()
	{
		Assert.AreEqual(2, Stage(98, 60, 110, out float progress, out float second));
		Assert.AreEqual(0.52f, progress, 0.0001f);
		Assert.AreEqual(1.52f, second, 0.0001f);
	}

	[Test]
	public void StarStage_HalfIsTruncated()
	{
		Assert.AreEqual(1, Stage(84, 60, 111, out float progress, out _));
		Assert.AreEqual(24f / 25f, progress, 0.0001f);
		Assert.AreEqual(2, Stage(86, 60, 111, out progress, out _));
		Assert.AreEqual(1f / 26f, progress, 0.0001f);
	}

	[Test]
	public void Credit_IsPercentAboveTargetOfPerfect()
	{
		Assert.AreEqual(22, (int)Call(typeof(LevelsController), "CreditPercent", new object[] { 85, 60, 110 }));
		Assert.AreEqual(0, (int)Call(typeof(LevelsController), "CreditPercent", new object[] { 60, 60, 110 }));
		Assert.AreEqual(0, (int)Call(typeof(LevelsController), "CreditPercent", new object[] { 60, 60, 0 }));
	}

	[Test]
	public void ChunkTracker_ShowsOnlyForSeveralChunks()
	{
		Assert.IsFalse((bool)Call(typeof(LevelChunkVisualizer), "IsVisible", new object[] { 0 }));
		Assert.IsFalse((bool)Call(typeof(LevelChunkVisualizer), "IsVisible", new object[] { 1 }));
		Assert.IsTrue((bool)Call(typeof(LevelChunkVisualizer), "IsVisible", new object[] { 2 }));
	}

	[Test]
	public void ChunkTracker_BackgroundGrowsWithChunkCount()
	{
		Assert.AreEqual(1.4f, (float)Call(typeof(LevelChunkVisualizer), "BackgroundPixelsPerUnit", new object[] { 2 }), 0.0001f);
		Assert.AreEqual(2f, (float)Call(typeof(LevelChunkVisualizer), "BackgroundPixelsPerUnit", new object[] { 5 }), 0.0001f);
	}

	[Test]
	public void StarPanel_EffectNeedsCooldownOrAFullStar()
	{
		Assert.IsFalse((bool)Call(typeof(GameScreen), "IsStarFxAllowed", new object[] { 0.1f, 0.5f }));
		Assert.IsTrue((bool)Call(typeof(GameScreen), "IsStarFxAllowed", new object[] { 0.25f, 0.5f }));
		Assert.IsTrue((bool)Call(typeof(GameScreen), "IsStarFxAllowed", new object[] { 0.1f, 1f }));
	}
}
