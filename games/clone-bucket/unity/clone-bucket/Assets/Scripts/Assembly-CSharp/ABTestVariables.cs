using UnityEngine;

public class ABTestVariables : SingletonBehaviour<ABTestVariables>
{
	private const string KeyCrazyLabsThemes = "isCrazyLabsThemes";

	private const string KeyNoObstacles = "hasNoObstacles";

	private const string KeyMixedLevels = "isMixedLevels";

	private const string KeyWithBossLevels = "withBossLevels";

	private const string KeyBossLevelEveryFive = "bossLevelEveryFive";

	private const string KeyWhiteBalls = "whiteBalls";

	private const string KeyExtraBalls = "extra_balls";

	private const string KeyBetterThanScore = "better_than_score";

	private bool isCrazyLabsThemes;

	private bool hasNoObstacles;

	private bool isMixedLevels;

	private bool withBossLevels;

	private bool bossLevelEveryFive;

	private bool whiteBalls;

	public bool extra_balls;

	public bool better_than_score;

	private void Start()
	{
		RemoteConfigHelper.OnRemoteReady += HandleRemoteReady;
	}

	private void OnDestroy()
	{
		RemoteConfigHelper.OnRemoteReady -= HandleRemoteReady;
	}

	private void HandleRemoteReady(bool carryOn)
	{
		RemoteConfigHelper.OnRemoteReady -= HandleRemoteReady;
		v();
	}

	[ContextMenu("sa")]
	private void v()
	{
		isCrazyLabsThemes = RemoteConfigHelper.GetBool(KeyCrazyLabsThemes, false);
		hasNoObstacles = RemoteConfigHelper.GetBool(KeyNoObstacles, false);
		isMixedLevels = RemoteConfigHelper.GetBool(KeyMixedLevels, false);
		withBossLevels = RemoteConfigHelper.GetBool(KeyWithBossLevels, false);
		bossLevelEveryFive = RemoteConfigHelper.GetBool(KeyBossLevelEveryFive, false);
		whiteBalls = RemoteConfigHelper.GetBool(KeyWhiteBalls, false);
		extra_balls = RemoteConfigHelper.GetBool(KeyExtraBalls, false);
		better_than_score = RemoteConfigHelper.GetBool(KeyBetterThanScore, false);
	}

	public bool IsCrazyLabsThemes()
	{
		return isCrazyLabsThemes;
	}

	public bool HasNoObstacles()
	{
		return hasNoObstacles;
	}

	public bool IsMixedLevels()
	{
		return isMixedLevels;
	}

	public bool WithBossLevels()
	{
		return withBossLevels;
	}

	public bool BossLevelEveryFive()
	{
		return bossLevelEveryFive;
	}

	public bool WhiteBalls()
	{
		return whiteBalls;
	}

	public bool GetExtraBalls()
	{
		return extra_balls;
	}

	public bool GetBetterThanScore()
	{
		return better_than_score;
	}
}
