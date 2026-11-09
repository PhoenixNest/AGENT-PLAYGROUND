using NUnit.Framework;
using UnityEngine;

public class LeaderboardTests
{
	[Test]
	public void RankFromScore_ZeroScoreIsTheTopRank()
	{
		Assert.AreEqual(15000, LeaderBoardView.RankFromScore(0));
	}

	[Test]
	public void RankFromScore_NinetyScoreIsThreeRanksDown()
	{
		Assert.AreEqual(14997, LeaderBoardView.RankFromScore(90));
	}

	[Test]
	public void RankFromScore_HugeScoreReachesZero_AndCallerClampsToOne()
	{
		int rank = LeaderBoardView.RankFromScore(450000);
		Assert.AreEqual(0, rank);
		Assert.AreEqual(1, Mathf.Max(1, rank), "The climb uses rank 1 when the new rank is 0 or less.");
	}

	[Test]
	public void ScoreFromRank_TopRankIsZeroScore()
	{
		Assert.AreEqual(0, LeaderBoardView.ScoreFromRank(15000));
	}

	[Test]
	public void ScoreFromRank_RoundTripsWithRankFromScore()
	{
		Assert.AreEqual(90, LeaderBoardView.ScoreFromRank(14997));
		Assert.AreEqual(450000, LeaderBoardView.ScoreFromRank(0));
	}

	[Test]
	public void ScoreFromRank_ClampsBelowZeroToZero()
	{
		Assert.AreEqual(0, LeaderBoardView.ScoreFromRank(15001), "A rank below the top has no score.");
	}

	[Test]
	public void ScoreFromRank_ClampsAboveTheMaximum()
	{
		Assert.AreEqual(99999999, LeaderBoardView.ScoreFromRank(-5000000));
	}

	[Test]
	public void BelowCount_ByPosition()
	{
		Assert.AreEqual(6, LeaderBoardView.BelowCount(1));
		Assert.AreEqual(6, LeaderBoardView.BelowCount(2));
		Assert.AreEqual(5, LeaderBoardView.BelowCount(3));
		Assert.AreEqual(4, LeaderBoardView.BelowCount(50));
	}

	[Test]
	public void AboveCount_ByTargetRank()
	{
		Assert.AreEqual(0, LeaderBoardView.AboveCount(1));
		Assert.AreEqual(1, LeaderBoardView.AboveCount(2));
		Assert.AreEqual(2, LeaderBoardView.AboveCount(3));
		Assert.AreEqual(3, LeaderBoardView.AboveCount(4));
		Assert.AreEqual(3, LeaderBoardView.AboveCount(100));
	}

	[Test]
	public void AboveCount_NegativeTargetUsesThree()
	{
		Assert.AreEqual(3, LeaderBoardView.AboveCount(0));
		Assert.AreEqual(3, LeaderBoardView.AboveCount(-4));
	}

	[Test]
	public void ContentTargetY_ByAboveAndClimb()
	{
		Assert.AreEqual(0f, LeaderBoardView.ContentTargetY(1, 2, 143f), 0.0001f);
		Assert.AreEqual(143f * 2f, LeaderBoardView.ContentTargetY(0, 1, 143f), 0.0001f);
		Assert.AreEqual(143f * -5f, LeaderBoardView.ContentTargetY(3, 5, 143f), 0.0001f);
	}

	[Test]
	public void ClimbSteps_FromTheScoreGain()
	{
		Assert.AreEqual(10, LeaderBoardView.ClimbSteps(15000, 300));
		Assert.AreEqual(0, LeaderBoardView.ClimbSteps(15000, 0));
	}

	[Test]
	public void ClimbSteps_CountsRankOneWhenNewRankIsZero()
	{
		Assert.AreEqual(14999, LeaderBoardView.ClimbSteps(15000, 450000));
	}

	[Test]
	public void ClimbDuration_ScalesWithClimb()
	{
		Assert.AreEqual(0.5f, LeaderBoardView.ClimbDuration(0), 0.0001f);
		Assert.AreEqual(0.8f, LeaderBoardView.ClimbDuration(10), 0.0001f);
	}

	[Test]
	public void GameManager_LeaderBoardPosition_ReadsTheLastScore()
	{
		GameObject go = new GameObject("LeaderboardTests_GameManager");
		try
		{
			GameManager game = go.AddComponent<GameManager>();
			game.LeaderBoardScore = 900;
			game.LastLeaderBoardScore = 300;
			Assert.AreEqual(14990, game.LeaderBoardPosition, "Position is the rank held before the last level, not the current score.");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}
}
