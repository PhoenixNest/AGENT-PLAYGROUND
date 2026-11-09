using System.Collections;
using System.Collections.Generic;
using Balls;
using CustomEventSystem;
using DG.Tweening;
using Lean.Pool;
using Levels;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : SingletonBehaviour<GameManager>
{
	public MainScreen mainScreen;

	public FailLevelScreen failLevelScreen;

	public CompleteLevelScreen completeLevelScreen;

	public GameScreen gameScreen;

	public SettingsScreen settingsScreen;

	public ShopScreen shopScreen;

	public Camera MainCam;

	public float ControllerMultiplier;

	public float ControllerDamping;

	public LeanGameObjectPool SubEffectPool;

	public LeanGameObjectPool DivideEffectPool;

	public SkinBallItem CurrentBallSkin;

	public GameObject Tutorial;

	public Transform ReferencePoint;

	public CameraMove cameraMove;

	public GameObject Fogs;

	[HideInInspector]
	public int CurrentLevel;

	[HideInInspector]
	public int GemCounter;

	[HideInInspector]
	public bool GameStarted;

	[HideInInspector]
	public bool LevelCompleted;

	[HideInInspector]
	public int BestScore;

	[HideInInspector]
	public int LeaderBoardScore;

	[HideInInspector]
	public int LastLeaderBoardScore;

	private Dictionary<string, object> eventParameters;

	public bool gameRestarted;

	private const string LAST_STARTED_MISSION_ID = "LAST_STARTED_MISSION_ID";

	private GameObject EndBlack;

	private float _startTime;

	private const string KeyCurrentLevel = "current_level";

	private const string KeyGemCounter = "gem_counter";

	private const string KeyBestScore = "best_score";

	private const string KeyLeaderBoardScore = "leader_board_score";

	private const string KeySoundsEnable = "sounds_enable";

	private const string KeyHapticEnable = "haptic_enable";

	private const string KeyCurrentBall = "current_ball";

	private const float DelayBeforeReward = 0.1f;

	private const float CompleteScreenDelay = 2f;

	private const float FailWindowDelay = 1f;

	private const string DefaultBallName = "Ball1";

	private const float ReferenceAspect = 9f / 16f;

	private const float DefaultFov = 60f;

	private float _baseFov = DefaultFov;

	private bool _soundsOn = true;

	private bool _hapticsOn = true;

	private bool _failOpen;

	private Coroutine _pendingWindow;

	public int LeaderBoardPosition => 15000 - (int)(LastLeaderBoardScore / 30f);

	protected override void Awake()
	{
		base.Awake();
		if (instance != this)
		{
			return;
		}
		EndBlack = FindEndBlack();
		_startTime = Time.realtimeSinceStartup;
		if (MainCam != null)
		{
			_baseFov = MainCam.fieldOfView;
		}
	}

	public void SendMissionCompleted(int missionIndex)
	{
		TTPAnalyticsService.MissionCompleted(missionIndex, Params("scope", "mission"));
	}

	public void SendMissionStart(int missionIndex, int chunkIndex)
	{
		TTPAnalyticsService.MissionStarted(missionIndex, Params("chunk", chunkIndex));
	}

	public void SendChunkStart(int chunkIndex)
	{
		TTPAnalyticsService.MissionStarted(chunkIndex, Params("scope", "chunk"));
	}

	public void SendChunkFail(int chunkIndex)
	{
		TTPAnalyticsService.MissionFailed(chunkIndex, Params("scope", "chunk"));
	}

	public void SendChunkSucceed(int chunkIndex)
	{
		TTPAnalyticsService.MissionCompleted(chunkIndex, Params("scope", "chunk"));
	}

	public void SendMissionFailed(int chunkIndex)
	{
		TTPAnalyticsService.MissionFailed(chunkIndex, Params("scope", "mission"));
	}

	public void SendItemTransaction(string name, int cost)
	{
		TTPAnalyticsService.CurrencyTransaction(CurrentLevel, new Dictionary<string, object> { { "item", name }, { "amount", cost } });
	}

	private void Start()
	{
		if (instance != this)
		{
			return;
		}
		Application.targetFrameRate = 60;
		AdjustFOV();
		LoadData();
		Reset(false);
		SetScreenActive(mainScreen, true);
	}

	private void AdjustFOV()
	{
		if (MainCam == null)
		{
			return;
		}
		MainCam.fieldOfView = FitVerticalFov(_baseFov, ReferenceAspect, MainCam.aspect);
	}

	public void SetSoundsEnable(bool value)
	{
		_soundsOn = value;
		PlayerPrefs.SetInt(KeySoundsEnable, value ? 1 : 0);
		if (AudioManager.instance != null)
		{
			AudioManager.instance.isSoundsPlay = value;
			if (value)
			{
				AudioManager.instance.SoundOn();
			}
			else
			{
				AudioManager.instance.SoundOff();
			}
		}
	}

	public void SetHapticEnable(bool value)
	{
		_hapticsOn = value;
		PlayerPrefs.SetInt(KeyHapticEnable, value ? 1 : 0);
		if (HapticManager.instance != null)
		{
			HapticManager.instance.isHapticPlay = value;
			if (value)
			{
				HapticManager.instance.HapticOn();
			}
			else
			{
				HapticManager.instance.HapticOff();
			}
		}
	}

	public void SaveData(int level)
	{
		PlayerPrefs.SetInt(KeyCurrentLevel, Mathf.Max(0, level));
		PlayerPrefs.SetInt(KeyGemCounter, GemCounter);
		PlayerPrefs.SetInt(KeyBestScore, BestScore);
		PlayerPrefs.SetInt(KeyLeaderBoardScore, LeaderBoardScore);
		PlayerPrefs.SetString(KeyCurrentBall, CurrentBallId());

		ABTestVariables ab = ABTestVariables.instance;
		if (ab != null)
		{
			PlayerPrefs.SetInt("white_balls", ab.WhiteBalls() ? 1 : 0);
			PlayerPrefs.SetInt("with_boss_levels", ab.WithBossLevels() ? 1 : 0);
			PlayerPrefs.SetInt("has_no_obstacles", ab.HasNoObstacles() ? 1 : 0);
			PlayerPrefs.SetInt("is_crazy_labs_themes", ab.IsCrazyLabsThemes() ? 1 : 0);
			PlayerPrefs.SetInt("mixed_levels", ab.IsMixedLevels() ? 1 : 0);
			PlayerPrefs.SetInt("boss_level_every_five", ab.BossLevelEveryFive() ? 1 : 0);
		}
		PlayerPrefs.Save();
	}

	private static readonly string[] ObsoleteSaveKeys =
	{
		"has_no_rv",
		"ttpStoredPurchases",
		"ttpInAppPurchases",
		"ttpNoAsdPurchased",
		"ttpLastStoredPurchaseDate",
		"ttpFirstPurchaseValue",
		"ttpPrivacySettingsConsent"
	};

	internal static void DeleteObsoleteSaveKeys()
	{
		foreach (string key in ObsoleteSaveKeys)
		{
			PlayerPrefs.DeleteKey(key);
		}
	}

	public void LoadData()
	{
		DeleteObsoleteSaveKeys();
		if (PlayerPrefs.HasKey(KeyCurrentLevel))
		{
			CurrentLevel = PlayerPrefs.GetInt(KeyCurrentLevel);
		}
		if (PlayerPrefs.HasKey(KeyGemCounter))
		{
			GemCounter = PlayerPrefs.GetInt(KeyGemCounter);
		}
		if (PlayerPrefs.HasKey(KeyBestScore))
		{
			BestScore = PlayerPrefs.GetInt(KeyBestScore);
		}
		if (PlayerPrefs.HasKey(KeyLeaderBoardScore))
		{
			LeaderBoardScore = PlayerPrefs.GetInt(KeyLeaderBoardScore);
		}

		_soundsOn = !PlayerPrefs.HasKey(KeySoundsEnable) || PlayerPrefs.GetInt(KeySoundsEnable) == 1;
		_hapticsOn = !PlayerPrefs.HasKey(KeyHapticEnable) || PlayerPrefs.GetInt(KeyHapticEnable) == 1;

		LoadBall();
		ApplySavedState();
	}

	private void LoadBall()
	{
		SkinManager skins = SkinManager.instance;
		if (!PlayerPrefs.HasKey(KeyCurrentBall))
		{
			CurrentBallSkin = skins != null ? skins.GetDefaultBall() : null;
			return;
		}
		string id = PlayerPrefs.GetString(KeyCurrentBall);
		if (IsStandardBallName(id))
		{
			CurrentBallSkin = skins != null ? (skins.GetBall(id) ?? skins.GetDefaultBall()) : null;
			return;
		}
		CurrentBallSkin = skins != null ? skins.GetDefaultBall() : null;
		PlayerPrefs.SetString(KeyCurrentBall, CurrentBallId());
	}

	private static bool IsStandardBallName(string id)
	{
		switch (id)
		{
			case "Ball1":
			case "Ball2":
			case "Ball3":
			case "Ball4":
			case "Ball5":
			case "Ball6":
				return true;
			default:
				return false;
		}
	}

	private string CurrentBallId()
	{
		if (CurrentBallSkin != null && !string.IsNullOrEmpty(CurrentBallSkin.Name))
		{
			return CurrentBallSkin.Name;
		}
		return DefaultBallName;
	}

	public void Reset(bool increaseLevelIndex = false)
	{
		LevelView view = FindAnyObjectByType<LevelView>();
		if (view != null)
		{
			view.Clear();
		}
		BallsController.SendAllToPool();
		DOTween.KillAll();
		StopPendingWindows();
		GameStarted = false;
		LevelCompleted = false;
		gameRestarted = false;
		_failOpen = false;
		if (increaseLevelIndex)
		{
			CurrentLevel++;
		}
		SetScreenActive(mainScreen, false);
		SetScreenActive(gameScreen, false);
		SetScreenActive(completeLevelScreen, false);
		SetScreenActive(failLevelScreen, false);
		SetEndBlackActive(false);
		if (Tutorial != null)
		{
			Tutorial.SetActive(true);
		}
		LevelsController.Spawn(CurrentLevel);
	}

	public void RestartGame()
	{
		Reset(false);
		gameRestarted = true;
		SetScreenActive(mainScreen, true);
	}

	public void StartPlay()
	{
		if (GameStarted)
		{
			return;
		}
		GameStarted = true;
		LevelCompleted = false;
		if (mainScreen != null)
		{
			mainScreen.Close(OnMainScreenClosed);
		}
		else
		{
			OnMainScreenClosed();
		}
		if (Tutorial != null)
		{
			Tutorial.SetActive(false);
		}
	}

	private void OnMainScreenClosed()
	{
		SetScreenActive(mainScreen, false);
		ShowGameScreen();
		RefreshGameScreen();
	}

	public IEnumerator DelayedNextLevel()
	{
		yield return new WaitForSeconds(DelayBeforeReward);
		int completedMiniLevels = LevelsController.TotalMiniLevelCount;
		CurrentLevel += 1;
		SaveData(CurrentLevel);
		Reset(false);
		SetScreenActive(mainScreen, true);
		if (mainScreen != null)
		{
			mainScreen.GetGem(completedMiniLevels);
		}
	}

	public void NextLevel()
	{
		CurrentLevel += 1;
		SaveData(CurrentLevel);
		Reset(false);
	}

	public void CompleteLevel()
	{
		if (LevelCompleted)
		{
			return;
		}
		gameRestarted = false;
		LevelCompleted = true;
		EventManager.TriggerEvent(EventConstants.LevelControllerEnabled, false);
		int reward = LevelRewardScore();
		LastLeaderBoardScore = LeaderBoardScore;
		LeaderBoardScore += reward;
		if (BestScore < reward)
		{
			BestScore = reward;
		}
		SetScreenActive(gameScreen, false);
		SetEndBlackActive(true);
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayEndPanel();
		}
		SendChunkSucceed(CurrentLevel);
		StopPendingWindows();
		_pendingWindow = StartCoroutine(ShowCompleteScreenAfter(CompleteScreenDelay));
	}

	private IEnumerator ShowCompleteScreenAfter(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		_pendingWindow = null;
		SetEndBlackActive(false);
		SetScreenActive(completeLevelScreen, true);
	}

	private void StopPendingWindows()
	{
		if (_pendingWindow != null)
		{
			StopCoroutine(_pendingWindow);
			_pendingWindow = null;
		}
	}

	public void OpenFailWindow()
	{
		SetScreenActive(failLevelScreen, true);
	}

	public void FailLevel()
	{
		if (LevelCompleted || _failOpen)
		{
			return;
		}
		_failOpen = true;
		SaveData(CurrentLevel);
		SendChunkFail(CurrentLevel);
		StopPendingWindows();
		_pendingWindow = StartCoroutine(OpenFailWindowAfter(FailWindowDelay));
	}

	private IEnumerator OpenFailWindowAfter(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		_pendingWindow = null;
		OpenFailWindow();
	}

	public void GemCollect(int value = 1)
	{
		GemCounter += value;
		EventManager.TriggerEvent(EventConstants.GemCollected);
		if (mainScreen != null)
		{
			mainScreen.SetGems(GemCounter);
		}
		if (completeLevelScreen != null)
		{
			completeLevelScreen.SetGems(GemCounter);
		}
		PlayerPrefs.SetInt(KeyGemCounter, GemCounter);
		PlayerPrefs.Save();
	}

	public void SetProgress(float value)
	{
		if (gameScreen != null)
		{
			gameScreen.SetProgress(value);
		}
	}

	public void HandleBeforeSpend(int price)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>
		{
			{ "balanceBefore", GemCounter },
			{ "currencyDelta", -price },
			{ "currencySink", "shop" }
		};
		TTPAnalyticsService.CurrencyTransaction(CurrentLevel, parameters);
	}

	public void MarkSkinOwned(string skinName)
	{
		if (string.IsNullOrEmpty(skinName) || SkinManager.instance == null)
		{
			return;
		}
		SkinManager.instance.MarkOwned(skinName);
	}

	private string GetChunkStartType(int attempt)
	{
		return attempt == 0 ? "first" : "retry";
	}

	private void ShowGameScreen()
	{
		SetScreenActive(completeLevelScreen, false);
		SetScreenActive(failLevelScreen, false);
		SetScreenActive(gameScreen, true);
	}

	private void RefreshGameScreen()
	{
		if (gameScreen == null)
		{
			return;
		}
		gameScreen.SetLevel(CurrentLevel);
		gameScreen.SetBalls(LevelsController.TotalBallCountInMiniLevel);
		gameScreen.SetProgress(0f);
	}

	private void ApplyAudioAndHapticState()
	{
		if (AudioManager.instance != null)
		{
			AudioManager.instance.isSoundsPlay = _soundsOn;
			if (_soundsOn)
			{
				AudioManager.instance.SoundOn();
			}
			else
			{
				AudioManager.instance.SoundOff();
			}
		}
		if (HapticManager.instance != null)
		{
			HapticManager.instance.isHapticPlay = _hapticsOn;
			if (_hapticsOn)
			{
				HapticManager.instance.HapticOn();
			}
			else
			{
				HapticManager.instance.HapticOff();
			}
		}
	}

	private void ApplySavedState()
	{
		ApplyAudioAndHapticState();
	}

	private static int LevelRewardScore()
	{
		return LevelsController.TotalFillCountAcrossLevels;
	}

	private static float FitVerticalFov(float fovDegrees, float referenceAspect, float aspect)
	{
		if (aspect <= 0f || referenceAspect <= 0f)
		{
			return fovDegrees;
		}
		float halfRadians = fovDegrees * 0.5f * Mathf.Deg2Rad;
		float fitted = 2f * Mathf.Atan(Mathf.Tan(halfRadians) * referenceAspect / aspect) * Mathf.Rad2Deg;
		return Mathf.Clamp(fitted, 1f, 170f);
	}

	private GameObject FindEndBlack()
	{
		if (completeLevelScreen == null || completeLevelScreen.transform.parent == null)
		{
			return null;
		}
		Transform card = completeLevelScreen.transform.parent.Find("EndBlack");
		return card != null ? card.gameObject : null;
	}

	private void SetEndBlackActive(bool active)
	{
		if (EndBlack == null)
		{
			EndBlack = FindEndBlack();
		}
		if (EndBlack != null && EndBlack.activeSelf != active)
		{
			EndBlack.SetActive(active);
		}
	}

	private static void SetScreenActive(Component screen, bool active)
	{
		if (screen != null && screen.gameObject.activeSelf != active)
		{
			screen.gameObject.SetActive(active);
		}
	}

	private static Dictionary<string, object> Params(string key, object value)
	{
		return new Dictionary<string, object> { { key, value } };
	}
}
