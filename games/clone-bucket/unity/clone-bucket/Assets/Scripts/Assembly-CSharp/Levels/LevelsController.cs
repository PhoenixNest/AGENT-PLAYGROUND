using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Balls;
using CrazyLabs.Extensions.Database;
using DG.Tweening;
using Game.Levels;
using Lean.Pool;
using UnityEngine;

namespace Levels
{
	public class LevelsController : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CSpawn_003Ed__135 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public int playerProgressIndex;

			private TaskAwaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				this.MoveNext();
			}

			[System.Diagnostics.DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				this.SetStateMachine(stateMachine);
			}
		}

		private const string DataSetId = "LevelsData";

		private const string LevelNameId = "Level Name";

		private const string MiniLevelIndexId = "Mini Level Index";

		private const string LevelDifficultyId = "Level Difficulty";

		private const string IsLoopingId = "Is Looping";

		private const string IsBossLevelId = "Is Boss Level";

		private const string StartBallAmountId = "Start Ball Amount";

		private const string TargetBallAmountId = "Target Ball Amount";

		private const string PerfectBallAmountId = "Perfect Ball Amount";

		private const string BallGoalPercentTargetId = "Ball Goal Percent Target";

		private const string BallGoalPercentPerfectId = "Ball Goal Percent Perfect";

		private const string GateTypesId = "Gate Types";

		private const string GateValuesId = "Gate Values";

		private const string BackgroundMaterialNameId = "Background Material Name";

		private const string MazeMaterialNameId = "Maze Material Name";

		private const string LevelTypeId = "Level Type";

		private const string LevelIndexId = "Level Index";

		private const string LastLevelIndexId = "Last Level Index";

		private const string LevelAttemptId = "LevelAttempt";

		private const string ChunkAttemptId = "ChunkAttempt";

		private static readonly string[] RowColumns =
		{
			LevelIndexId, MiniLevelIndexId, LevelNameId, LevelTypeId, LevelDifficultyId, IsLoopingId,
			IsBossLevelId, StartBallAmountId, TargetBallAmountId, PerfectBallAmountId, GateTypesId,
			GateValuesId, BackgroundMaterialNameId, MazeMaterialNameId
		};

		private const string LiveFilterExcludedFragment = "2.8";

		private const string LiveFilterExcludedName = "LevelSquareRein1";

		private const int DynamicGoalStep = 25;

		[CompilerGenerated]
		private static Action m_OnLevelSpawned;

		[CompilerGenerated]
		private static Action m_OnChunkCompleted;

		[CompilerGenerated]
		private static Action m_OnChunkAboutToBeCompleted;

		[CompilerGenerated]
		private static Action m_OnChunkStarted;

		[CompilerGenerated]
		private static Action m_OnBallEnterBucket;

		[SerializeField]
		private LevelsConfig _config;

		private List<LevelData> _allLevels;

		private List<LevelData> _loopingLevelss;

		private List<MiniLevelData> _miniLevels;

		private Dictionary<string, MiniLevelData> _miniLevelsByNames;

		private static LevelView _view;

		private bool _isDataLoaded;

		private static bool _useOnlyLoopingLevels;

		private static int _completeTimer;

		private static int _loopIndex;

		private static CreditScoreBar _betterThanCreditScore;

		private static LevelsController _instance;

		private static int _miniLevelCount;

		private static bool _isNewChunkStarted;

		private static readonly Dictionary<string, int> _chunkAttempts = new Dictionary<string, int>();

		private static List<MiniLevelData> _levelMiniLevels = new List<MiniLevelData>();

		private static MiniLevelView _currentMiniLevelView;

		private static CameraMove _cameraMove;

		private static int _currentTargetBalls;

		private static int _currentPerfectBalls;

		private static bool _isBossLevel;

		private static bool _transitionScheduled;

		private static Sequence _pendingTransition;

		private static BallsController _ballsController;

		private static GatesController _gatesController;

		private static int _levelAttempt;

		private static int _lastLevelIndex;

		public static LevelData CurrentLevelData { get; private set; }

		public static int CurrentMiniLevelCount => CurrentLevelData.miniLevelNames != null ? CurrentLevelData.miniLevelNames.Count : 0;

		public static int TotalLevelCount => _instance?._allLevels?.Count ?? 0;

		public static int LoopIndex => 0;

		public static bool IsLoopingRerollActive => false;

		public static bool NoChunks { get; private set; }

		public static bool MostlyVertical { get; private set; }

		public static MiniLevelView MiniLevelView => _currentMiniLevelView;

		public static MiniLevelData CurrentMiniLevelData => CurrentMiniLevel;

		private const float ChunkCameraMoveDuration = 0.75f;

		private const float StarFxHeight = 1.4f;

		private const float StarFxSpread = 0.5f;

		private const float CreditPopDuration = 0.5f;

		public static IReadOnlyList<MiniLevelView> MiniLevelViews => _view != null ? _view.miniLevelViews : null;

		public static Vector3 LevelDistance => _instance?._config != null ? _instance._config.distanceBetweenMiniLevels : default;

		public static int TotalBallCountInMiniLevel { get; private set; }

		public static int TotalMiniLevelCount { get; private set; }

		public static int TotalNonLoopingMiniLevelCount { get; private set; }

		public static int TotalFillCount { get; set; }

		public static int TotalFillCountAcrossLevels { get; set; }

		public static int FilledStarCount { get; set; }

		public static bool Completed { get; set; }

		public static float ChunkStartTime { get; private set; }

		public static int MiniLevelIndex { get; private set; }

		public static bool IsNewLevel { get; private set; }

		public static int LastLevelIndex
		{
			get
			{
				return _lastLevelIndex;
			}
			set
			{
				_lastLevelIndex = value;
			}
		}

		public static int LevelAttempt
		{
			get
			{
				return _levelAttempt;
			}
			set
			{
				_levelAttempt = value;
			}
		}

		public static event Action OnLevelSpawned
		{
			add
			{
				m_OnLevelSpawned = (Action)Delegate.Combine(m_OnLevelSpawned, value);
			}
			remove
			{
				m_OnLevelSpawned = (Action)Delegate.Remove(m_OnLevelSpawned, value);
			}
		}

		public static event Action OnChunkCompleted
		{
			add
			{
				m_OnChunkCompleted = (Action)Delegate.Combine(m_OnChunkCompleted, value);
			}
			remove
			{
				m_OnChunkCompleted = (Action)Delegate.Remove(m_OnChunkCompleted, value);
			}
		}

		public static event Action OnChunkAboutToBeCompleted
		{
			add
			{
				m_OnChunkAboutToBeCompleted = (Action)Delegate.Combine(m_OnChunkAboutToBeCompleted, value);
			}
			remove
			{
				m_OnChunkAboutToBeCompleted = (Action)Delegate.Remove(m_OnChunkAboutToBeCompleted, value);
			}
		}

		public static event Action OnChunkStarted
		{
			add
			{
				m_OnChunkStarted = (Action)Delegate.Combine(m_OnChunkStarted, value);
			}
			remove
			{
				m_OnChunkStarted = (Action)Delegate.Remove(m_OnChunkStarted, value);
			}
		}

		public static event Action OnBallEnterBucket
		{
			add
			{
				m_OnBallEnterBucket = (Action)Delegate.Combine(m_OnBallEnterBucket, value);
			}
			remove
			{
				m_OnBallEnterBucket = (Action)Delegate.Remove(m_OnBallEnterBucket, value);
			}
		}

		private static MiniLevelData CurrentMiniLevel => MiniLevelIndex >= 0 && MiniLevelIndex < _levelMiniLevels.Count ? _levelMiniLevels[MiniLevelIndex] : default;

		private static bool HasNextChunk => MiniLevelIndex + 1 < _levelMiniLevels.Count;

		private void Awake()
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
			_allLevels = new List<LevelData>();
			_loopingLevelss = new List<LevelData>();
			_miniLevels = new List<MiniLevelData>();
			_miniLevelsByNames = new Dictionary<string, MiniLevelData>();
			_isDataLoaded = false;
			ResetLevelState();
		}

		private void SpawnLevelPrefab()
		{
			if (_view != null)
			{
				LeanPool.DespawnAll();
				_view.Clear();
				Destroy(_view.gameObject);
				_view = null;
			}

			if (_config == null || _config.prefab == null)
			{
				Debug.LogError("[MM3:Levels] LevelsConfig.prefab is not set");
				return;
			}

			var prefabs = new List<GameObject>();
			foreach (MiniLevelData mini in _levelMiniLevels)
			{
				if (mini.levelType == LevelType.Json)
				{
					Debug.LogError("[MM3:Levels] Json levels are not supported in v1 (Q18): " + mini.levelName);
					return;
				}

				GameObject prefab = Resources.Load<GameObject>("prefabs/minilevels/" + mini.levelName);
				if (prefab == null)
				{
					Debug.LogError("[MM3:Levels] Missing level prefab Resources/prefabs/minilevels/" + mini.levelName);
					return;
				}

				prefabs.Add(prefab);
			}

			_view = Instantiate(_config.prefab);
			if (_view.miniLevelViews == null)
			{
				_view.miniLevelViews = new List<MiniLevelView>();
			}

			Transform root = _view.transformCached != null ? _view.transformCached : _view.transform;
			for (int i = 0; i < prefabs.Count; i++)
			{
				GameObject instance = Instantiate(prefabs[i], root);
				instance.transform.localPosition = _config.distanceBetweenMiniLevels * i;
				MiniLevelView miniView = instance.GetComponent<MiniLevelView>();
				if (miniView == null)
				{
					miniView = instance.GetComponentInChildren<MiniLevelView>(true);
				}

				if (miniView == null)
				{
					Debug.LogError("[MM3:Levels] Level prefab has no MiniLevelView: " + prefabs[i].name);
					continue;
				}

				_view.miniLevelViews.Add(miniView);
			}
		}

		public static void Spawn(int playerProgressIndex)
		{
			LevelsController controller = _instance;
			if (controller == null)
			{
				Debug.LogError("[MM3:Levels] Spawn called before LevelsController.Awake");
				return;
			}

			controller.LoadData();
			int count = TotalLevelCount;
			if (count == 0)
			{
				Debug.LogError("[MM3:Levels] No live levels available; check DatabaseController and LevelsData");
				return;
			}

			int index0 = playerProgressIndex - 1;
			int levelIndex = index0;
			bool useLooping = false;
			if (index0 >= count)
			{
				int loopingCount = controller._loopingLevelss.Count;
				if (loopingCount > 0)
				{
					int shifted = loopingCount - count + index0;
					_loopIndex = shifted / loopingCount;
					levelIndex = shifted % loopingCount;
					useLooping = true;
				}
				else
				{
					levelIndex = ((index0 % count) + count) % count;
					Debug.LogWarning("[MM3:Levels] Progress index " + playerProgressIndex + " is past the live list and no looping levels exist; wrapping to " + levelIndex);
				}
			}

			BallsController.SendAllToPool();
			ResetLevelState();

			IsNewLevel = playerProgressIndex != LastLevelIndex;
			LastLevelIndex = playerProgressIndex;
			LevelAttempt = IsNewLevel ? 1 : LevelAttempt + 1;

			_loopIndex = useLooping ? _loopIndex : 0;
			_useOnlyLoopingLevels = useLooping;
			CurrentLevelData = useLooping ? controller._loopingLevelss[levelIndex] : controller._allLevels[levelIndex];

			_levelMiniLevels.Clear();
			foreach (string miniName in CurrentLevelData.miniLevelNames)
			{
				if (controller._miniLevelsByNames.TryGetValue(miniName, out MiniLevelData mini))
				{
					_levelMiniLevels.Add(mini);
				}
			}

			_miniLevelCount = _levelMiniLevels.Count;
			TotalMiniLevelCount = _levelMiniLevels.Count;
			TotalNonLoopingMiniLevelCount = CurrentLevelData.isLooping ? 0 : _levelMiniLevels.Count;
			NoChunks = _levelMiniLevels.Count <= 1;
			MostlyVertical = false;

			controller.SpawnLevelPrefab();
			if (_view == null || _view.miniLevelViews == null || _view.miniLevelViews.Count != _levelMiniLevels.Count)
			{
				Debug.LogError("[MM3:Levels] Level " + CurrentLevelData.miniLevelNames[0] + " did not spawn; the level is not started");
				return;
			}

			if (_cameraMove == null)
			{
				_cameraMove = FindAnyObjectByType<CameraMove>();
			}

			if (_cameraMove != null)
			{
				_cameraMove.Reset();
			}

			_isBossLevel = CurrentMiniLevel.isBossLevel;
			if (_ballsController == null)
			{
				_ballsController = FindAnyObjectByType<BallsController>();
			}

			if (_ballsController != null)
			{
				_ballsController.SetBossLevel(_isBossLevel);
			}

			if (_cameraMove != null)
			{
				_cameraMove.SetBossLevel(_isBossLevel);
			}

			if (_gatesController == null)
			{
				_gatesController = FindAnyObjectByType<GatesController>();
			}

			if (_gatesController != null)
			{
				_gatesController.ResetSubGates();
			}

			for (int i = 0; i < _view.miniLevelViews.Count && i < _levelMiniLevels.Count; i++)
			{
				ApplyGateValues(_view.miniLevelViews[i], _levelMiniLevels[i]);
			}

			StartMiniLevel(true);

			controller.SetControllerActive(true);
			controller.SetControllerEnabled(true);

			if (m_OnLevelSpawned != null)
			{
				m_OnLevelSpawned();
			}
		}

		private static void StartMiniLevel(bool isFirstChunk = false)
		{
			int index = isFirstChunk ? 0 : MiniLevelIndex;
			if (index < 0 || index >= _levelMiniLevels.Count)
			{
				Debug.LogError("[MM3:Levels] StartMiniLevel: no mini-level at index " + index);
				return;
			}

			MiniLevelIndex = index;
			MiniLevelData mini = _levelMiniLevels[index];
			_isBossLevel = mini.isBossLevel;
			ChunkStartTime = Time.time;
			_transitionScheduled = false;

			_currentTargetBalls = GetTargetBallAmount(mini.levelName);
			_currentPerfectBalls = GetPerfectBallAmount(mini.levelName);

			_currentMiniLevelView = _view != null && index < _view.miniLevelViews.Count ? _view.miniLevelViews[index] : null;
			if (_currentMiniLevelView == null)
			{
				Debug.LogError("[MM3:Levels] No MiniLevelView for " + mini.levelName);
			}

			if (index == 0)
			{
				InitializeMiniLevel();
			}
			else
			{
				ResetChunkState(mini.startBallAmount);
				MoveCameraToChunk();
			}

			UpdateCounter(index);

			if (isFirstChunk)
			{
				TTPAnalyticsService.MissionStarted(GetMissionIndex(mini));
			}

			if (GameManager.instance != null)
			{
				if (isFirstChunk)
				{
					GameManager.instance.SendMissionStart(GetMissionIndex(mini), index);
				}
				else
				{
					GameManager.instance.SendChunkStart(index);
				}
			}

			if (m_OnChunkStarted != null)
			{
				m_OnChunkStarted();
			}
		}

		internal static List<(int gateIndex, int value)> PlanGateValues(int gateCount, IReadOnlyList<int> values)
		{
			var plan = new List<(int gateIndex, int value)>();
			if (values == null)
			{
				return plan;
			}

			int count = Math.Min(gateCount, values.Count);
			for (int i = 0; i < count; i++)
			{
				plan.Add((i, values[i]));
			}

			return plan;
		}

		private static void ApplyGateValues(MiniLevelView view, MiniLevelData mini)
		{
			if (view == null || view.allGates == null)
			{
				return;
			}

			foreach ((int gateIndex, int value) in PlanGateValues(view.allGates.Length, mini.gateValues))
			{
				GateBase gate = view.allGates[gateIndex];
				if (gate != null)
				{
					gate.SetValue(value);
				}
			}
		}

		private static void InitializeMiniLevel()
		{
			GameScreen gameScreen = GameManager.instance != null ? GameManager.instance.gameScreen : null;
			if (gameScreen != null)
			{
				gameScreen.DeactivateStarPanels();
			}

			if (_cameraMove != null && _currentMiniLevelView != null && _currentMiniLevelView.bucket != null)
			{
				_cameraMove.PosYLimitUpdate(_currentMiniLevelView.bucket.transform.position.y);
			}

			ResetChunkState(CurrentMiniLevel.startBallAmount);
		}

		private static void ResetChunkState(int startBalls)
		{
			_isNewChunkStarted = true;
			TotalFillCount = 0;
			Completed = false;
			SpawnBallsAtStart(startBalls);
			TotalBallCountInMiniLevel = startBalls;
			PushBallsLeft();
			UpdateCounter(MiniLevelIndex);
		}

		private static void MoveCameraToChunk()
		{
			if (_cameraMove == null || _currentMiniLevelView == null || _instance == null || _instance._config == null)
			{
				return;
			}

			float targetX = _currentMiniLevelView.transform.position.x + _instance._config.cameraLocalPosition.x;
			_cameraMove.MoveToChunkX(targetX, ChunkCameraMoveDuration);
		}

		private static void PushBallsLeft()
		{
			GameScreen gameScreen = GameManager.instance != null ? GameManager.instance.gameScreen : null;
			if (gameScreen != null)
			{
				gameScreen.SetBalls(TotalBallCountInMiniLevel);
			}
		}

		public static int GetMissionIndex(MiniLevelData data)
		{
			return data.missionIndex;
		}

		public static void RequestSpawnInitialBalls()
		{
			LevelsController controller = _instance;
			if (controller == null || _currentMiniLevelView == null)
			{
				Debug.LogError("[MM3:Levels] RequestSpawnInitialBalls: no active mini-level");
				return;
			}

			SpawnBallsAtStart(CurrentMiniLevel.startBallAmount);
		}

		private static void SpawnBallsAtStart(int count)
		{
			LevelsController controller = _instance;
			if (controller == null || _currentMiniLevelView == null || count <= 0)
			{
				return;
			}

			Vector3[] spawns = controller._config != null ? controller._config.ballSpawnPositions : null;
			Vector3 origin = _currentMiniLevelView.transform.position;
			float y = origin.y;
			float z = origin.z;
			for (int i = 0; i < count; i++)
			{
				Vector3 slot = spawns != null && spawns.Length > 0 ? spawns[i % spawns.Length] : Vector3.zero;
				y += slot.y;
				z += slot.z;
				BallsController.Spawn(new Vector3(origin.x + slot.x, y, z), _currentMiniLevelView.ballsParentInsideMaze);
			}
		}

		private void SetControllerActive(bool isActive)
		{
			if (_currentMiniLevelView == null)
			{
				return;
			}

			if (_currentMiniLevelView.rotateController != null)
			{
				_currentMiniLevelView.rotateController.gameObject.SetActive(isActive);
			}

			if (_currentMiniLevelView.bucket != null)
			{
				_currentMiniLevelView.bucket.gameObject.SetActive(isActive);
			}
		}

		private void SetControllerEnabled(bool isEnabled)
		{
			if (_currentMiniLevelView != null && _currentMiniLevelView.rotateController != null)
			{
				_currentMiniLevelView.rotateController.enabled = isEnabled;
			}
		}

		private void LoadData()
		{
			if (_isDataLoaded)
			{
				return;
			}

			if (!DatabaseController.IsInitialized)
			{
				Debug.LogError("[MM3:Levels] DatabaseController is not initialised; levels not loaded");
				return;
			}

			List<string> orderedIds = DatabaseController.GetAllKeys(DataSetId);
			List<string> liveIds = FilterLiveItemIds(orderedIds, id => DatabaseController.GetString(DataSetId, id, LevelNameId));

			_allLevels.Clear();
			_loopingLevelss.Clear();
			_miniLevels.Clear();
			_miniLevelsByNames.Clear();

			foreach (string id in liveIds)
			{
				var row = new Dictionary<string, string>();
				foreach (string column in RowColumns)
				{
					row[column] = DatabaseController.GetString(DataSetId, id, column) ?? string.Empty;
				}

				LevelData level = ParseLevelData(row);
				MiniLevelData mini = ParseMiniLevelData(row);
				_allLevels.Add(level);
				if (level.isLooping)
				{
					_loopingLevelss.Add(level);
				}

				_miniLevels.Add(mini);
				_miniLevelsByNames.TryAdd(mini.levelName, mini);
			}

			_isDataLoaded = true;
		}

		public static void OnFillBucket(bool filledBefore, int amount = 1)
		{
			if (GameManager.instance != null && GameManager.instance.LevelCompleted)
			{
				return;
			}
			_completeTimer = 0;
			if (!filledBefore)
			{
				TotalFillCount += amount;
				TotalFillCountAcrossLevels += amount;
			}

			UpdateCounter(MiniLevelIndex);

			string levelName = CurrentMiniLevel.levelName;
			int target = GetTargetBallAmount(levelName);
			int perfect = GetPerfectBallAmount(levelName);

			if (target <= TotalFillCount && !Completed)
			{
				Completed = true;
				GameScreen gameScreen = GameManager.instance != null ? GameManager.instance.gameScreen : null;
				if (gameScreen != null)
				{
					gameScreen.ShowCompleteLevelBtns();
				}
			}

			if (TotalBallCountInMiniLevel == TotalFillCount)
			{
				if (Completed && !filledBefore && _isNewChunkStarted)
				{
					_isNewChunkStarted = false;
					if (m_OnChunkAboutToBeCompleted != null)
					{
						m_OnChunkAboutToBeCompleted();
					}

					ScheduleTransition(2.0f, StartNextChunkFromTimer);
				}
				else if (!Completed)
				{
					ScheduleTransition(1.6f, OpenFailWindowFromTimer);
				}
			}

			if (CurrentLevelData.miniLevelNames != null && CurrentLevelData.miniLevelNames.Count == 1)
			{
				FilledStarCount = CalculateSingleLevelStars(TotalFillCount, target, perfect);
				UpdateStars(perfect, target);
			}
			else
			{
				int stars = CalculateStars(TotalFillCountAcrossLevels, target, perfect);
				if (stars > 0)
				{
					FilledStarCount = stars;
				}
			}

			if (m_OnBallEnterBucket != null)
			{
				m_OnBallEnterBucket();
			}
		}

		internal static int StarStage(int filled, int target, int perfect, out float progress, out float secondStarProgress)
		{
			int half = (perfect - target) / 2;
			secondStarProgress = 0f;
			if (filled <= target)
			{
				progress = Ratio(filled, target);
				return 0;
			}
			if (filled <= target + half)
			{
				progress = Ratio(filled - target, half);
				return 1;
			}
			secondStarProgress = Ratio(filled - target, half);
			progress = Ratio(filled - target - half, perfect - target - half);
			return 2;
		}

		internal static int CreditPercent(int filled, int target, int perfect)
		{
			return perfect > 0 ? (int)((filled - target) * 100f / perfect) : 0;
		}

		private static float Ratio(int value, int range)
		{
			return range > 0 ? (float)value / range : 1f;
		}

		private static void UpdateStars(int perfectBallAmount, int targetBallAmount)
		{
			GameScreen gameScreen = GameManager.instance != null ? GameManager.instance.gameScreen : null;
			if (gameScreen == null || _currentMiniLevelView == null || _currentMiniLevelView.bucket == null)
			{
				return;
			}

			Vector3 position = _currentMiniLevelView.bucket.transform.position + Vector3.up * StarFxHeight
				+ Vector3.left * UnityEngine.Random.Range(-StarFxSpread, StarFxSpread);
			int stage = StarStage(TotalFillCount, targetBallAmount, perfectBallAmount, out float progress, out float secondStarProgress);
			if (stage == 2 && secondStarProgress > 1f)
			{
				gameScreen.FillStarEffect(1, secondStarProgress, position);
			}
			gameScreen.FillStarEffect(stage, progress, position);

			if (gameScreen.FilledStar2 == null || gameScreen.FilledStar2.fillAmount < 1f)
			{
				return;
			}
			if (_betterThanCreditScore == null)
			{
				_betterThanCreditScore = gameScreen.BetterThanCreditScore;
			}
			CreditScoreBar credit = _betterThanCreditScore;
			if (credit == null)
			{
				return;
			}
			if (!credit.gameObject.activeInHierarchy && GameScreen.IsActive())
			{
				credit.transform.localScale = Vector3.zero;
				credit.transform.DOScale(Vector3.one, CreditPopDuration).SetEase(Ease.OutBack);
				credit.gameObject.SetActive(true);
			}
			credit.SetValue(CreditPercent(TotalFillCount, targetBallAmount, perfectBallAmount));
		}

		public static void HandleMissionFail()
		{
			if (CurrentLevelData.miniLevelNames == null || _levelMiniLevels.Count == 0)
			{
				return;
			}

			int missionIndex = GetMissionIndex(CurrentMiniLevel);
			if (_instance != null)
			{
				_instance.SetControllerEnabled(false);
			}

			TTPAnalyticsService.MissionFailed(missionIndex);
			if (GameManager.instance != null)
			{
				GameManager.instance.SendMissionFailed(missionIndex);
			}

			MiniLevelIndex = 0;
			if (GameManager.instance != null)
			{
				GameManager.instance.FailLevel();
			}
		}

		public static void StartNextChunk()
		{
			CancelPendingTransition();
			if (_levelMiniLevels.Count == 0)
			{
				return;
			}

			GameManager gameManager = GameManager.instance;
			if (gameManager != null)
			{
				gameManager.SendMissionCompleted(GetMissionIndex(CurrentMiniLevel));
			}

			MiniLevelIndex += 1;
			if (MiniLevelIndex >= _levelMiniLevels.Count)
			{
				MiniLevelIndex = 0;
				TTPAnalyticsService.MissionCompleted(GetMissionIndex(CurrentMiniLevel));
				if (gameManager != null)
				{
					gameManager.CompleteLevel();
				}

				return;
			}

			if (m_OnChunkCompleted != null)
			{
				m_OnChunkCompleted();
			}

			StartMiniLevel(false);
		}

		private static void StartNextChunkFromTimer()
		{
			StartNextChunk();
		}

		private static void OpenFailWindowFromTimer()
		{
			GameManager gameManager = GameManager.instance;
			if (gameManager != null)
			{
				gameManager.OpenFailWindow();
			}
		}

		private static void FailLevelFromTimer()
		{
			GameManager gameManager = GameManager.instance;
			if (gameManager != null)
			{
				gameManager.FailLevel();
			}
		}

		private static void CompleteLevelFromTimer()
		{
			GameManager gameManager = GameManager.instance;
			if (gameManager != null)
			{
				gameManager.CompleteLevel();
			}
		}

		private static void ScheduleTransition(float delay, TweenCallback callback)
		{
			if (_transitionScheduled)
			{
				return;
			}

			_transitionScheduled = true;
			_pendingTransition = DOTween.Sequence().AppendInterval(delay).AppendCallback(callback);
		}

		private static void CancelPendingTransition()
		{
			Sequence pending = _pendingTransition;
			_pendingTransition = null;
			if (pending != null && pending.IsActive())
			{
				pending.Kill();
			}
		}

		public static void StageCheck()
		{
			if (TotalBallCountInMiniLevel - TotalFillCount > 0)
			{
				return;
			}

			if (!Completed)
			{
				ScheduleTransition(1.6f, FailLevelFromTimer);
			}
			else
			{
				ScheduleTransition(2.0f, CompleteLevelFromTimer);
			}
		}

		private static void UpdateCounter(int index)
		{
			MiniLevelView view = _view != null && index >= 0 && index < _view.miniLevelViews.Count ? _view.miniLevelViews[index] : null;
			if (view == null || view.bucketBallCounter == null)
			{
				return;
			}

			view.bucketBallCounter.text = TotalFillCount + "/" + GetTargetBallAmount();
		}

		public static void AddTotal()
		{
			TotalBallCountInMiniLevel += 1;
			PushBallsLeft();
		}

		public static void SubtractTotal(int amount = 1)
		{
			TotalBallCountInMiniLevel -= amount;
			PushBallsLeft();
		}

		private static int FloorToMultipleOf25(float value)
		{
			return (int)Math.Floor(value / DynamicGoalStep) * DynamicGoalStep;
		}

		private static int GetTargetBallAmount(string miniLevelName)
		{
			if (_instance == null || miniLevelName == null)
			{
				return 0;
			}

			return _instance._miniLevelsByNames.TryGetValue(miniLevelName, out MiniLevelData mini) ? mini.targetBallAmount : 0;
		}

		private static int GetPerfectBallAmount(string miniLevelName)
		{
			if (_instance == null || miniLevelName == null)
			{
				return 0;
			}

			return _instance._miniLevelsByNames.TryGetValue(miniLevelName, out MiniLevelData mini) ? mini.perfectBallAmount : 0;
		}

		public static int GetTargetBallAmount()
		{
			return _currentTargetBalls;
		}

		public static int GetPerfectBallAmount()
		{
			return _currentPerfectBalls;
		}

		public static int GetSecondStarAmount()
		{
			int target = _currentTargetBalls;
			int perfect = _currentPerfectBalls;
			return target + (int)((perfect - target) / 2f);
		}

		public static MiniLevelData GetMiniLevelData(string id)
		{
			if (_instance == null || id == null)
			{
				return default;
			}

			return _instance._miniLevelsByNames.TryGetValue(id, out MiniLevelData mini) ? mini : default;
		}

		public static int GetChunkAttempt(int level, int index)
		{
			return _chunkAttempts.TryGetValue(AttemptKey(level, index), out int attempt) ? attempt : 0;
		}

		public static void SetChunkAttempt(int level, int index, int attempt)
		{
			_chunkAttempts[AttemptKey(level, index)] = Math.Max(0, attempt);
		}

		private static string AttemptKey(int level, int index)
		{
			return level + "_" + index;
		}

		private static void ResetLevelState()
		{
			TotalBallCountInMiniLevel = 0;
			TotalMiniLevelCount = 0;
			TotalNonLoopingMiniLevelCount = 0;
			TotalFillCount = 0;
			TotalFillCountAcrossLevels = 0;
			FilledStarCount = 0;
			Completed = false;
			MiniLevelIndex = 0;
			ChunkStartTime = 0f;
			NoChunks = false;
			MostlyVertical = false;
			_miniLevelCount = 0;
			_isNewChunkStarted = false;
			_isBossLevel = false;
			_currentTargetBalls = 0;
			_currentPerfectBalls = 0;
			_currentMiniLevelView = null;
			_levelMiniLevels.Clear();
		}

		public static bool IsLiveLevelName(string levelName)
		{
			if (string.IsNullOrEmpty(levelName))
			{
				return false;
			}

			return !levelName.Contains(LiveFilterExcludedFragment) && levelName != LiveFilterExcludedName;
		}

		public static List<string> FilterLiveItemIds(IReadOnlyList<string> orderedIds, Func<string, string> levelNameOf)
		{
			var live = new List<string>();
			foreach (string id in orderedIds)
			{
				if (IsLiveLevelName(levelNameOf(id)))
				{
					live.Add(id);
				}
			}

			return live;
		}

		public static int CalculateStars(int filled, int target, int perfect)
		{
			if (filled < target)
			{
				return -1;
			}

			int half = (int)Math.Round((double)((perfect - target) / 2f), MidpointRounding.ToEven);
			if (filled < target + half)
			{
				return 1;
			}

			return filled < perfect ? 2 : 3;
		}

		public static int CalculateSingleLevelStars(int filled, int target, int perfect)
		{
			if (perfect > 0 && filled >= perfect)
			{
				return 2;
			}

			if (target > 0 && filled >= target)
			{
				return 1;
			}

			return 0;
		}

		public static LevelData ParseLevelData(IReadOnlyDictionary<string, string> row)
		{
			return new LevelData
			{
				miniLevelNames = new List<string> { ReadText(row, LevelNameId) },
				isLooping = ReadBool(row, IsLoopingId),
				levelDifficulty = ReadInt(row, LevelDifficultyId),
				backgroundMaterialName = ReadText(row, BackgroundMaterialNameId)
			};
		}

		public static MiniLevelData ParseMiniLevelData(IReadOnlyDictionary<string, string> row)
		{
			string levelTypeText = ReadText(row, LevelTypeId);
			LevelType levelType = LevelType.Prefab;
			if (levelTypeText.Length > 0 && !Enum.TryParse(levelTypeText, true, out levelType))
			{
				Debug.LogWarning("[MM3:Levels] Unknown Level Type '" + levelTypeText + "', using Prefab");
				levelType = LevelType.Prefab;
			}

			List<GateType> gateTypes = ParseGateTypes(ReadText(row, GateTypesId));
			List<int> gateValues = ParseGateValues(ReadText(row, GateValuesId));
			if (gateTypes.Count != gateValues.Count)
			{
				int count = Math.Min(gateTypes.Count, gateValues.Count);
				Debug.LogWarning("[MM3:Levels] " + ReadText(row, LevelNameId) + ": gate types and values differ in length; using " + count);
				gateTypes.RemoveRange(count, gateTypes.Count - count);
				gateValues.RemoveRange(count, gateValues.Count - count);
			}

			return new MiniLevelData
			{
				miniLevelIndex = ReadInt(row, MiniLevelIndexId),
				missionIndex = ReadInt(row, LevelIndexId),
				levelName = ReadText(row, LevelNameId),
				startBallAmount = ReadInt(row, StartBallAmountId),
				targetBallAmount = ReadInt(row, TargetBallAmountId),
				perfectBallAmount = ReadInt(row, PerfectBallAmountId),
				isBossLevel = ReadBool(row, IsBossLevelId),
				levelType = levelType,
				gateTypes = gateTypes,
				gateValues = gateValues,
				mazeMaterialName = ReadText(row, MazeMaterialNameId)
			};
		}

		private static List<GateType> ParseGateTypes(string text)
		{
			var result = new List<GateType>();
			foreach (string part in SplitList(text))
			{
				if (Enum.TryParse(part, true, out GateType gateType))
				{
					result.Add(gateType);
				}
				else
				{
					Debug.LogWarning("[MM3:Levels] Unknown gate type '" + part + "'");
				}
			}

			return result;
		}

		private static List<int> ParseGateValues(string text)
		{
			var result = new List<int>();
			foreach (string part in SplitList(text))
			{
				result.Add(int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0);
			}

			return result;
		}

		private static List<string> SplitList(string text)
		{
			var parts = new List<string>();
			if (string.IsNullOrEmpty(text))
			{
				return parts;
			}

			foreach (string part in text.Split(';'))
			{
				string trimmed = part.Trim();
				if (trimmed.Length > 0)
				{
					parts.Add(trimmed);
				}
			}

			return parts;
		}

		private static string ReadText(IReadOnlyDictionary<string, string> row, string column)
		{
			return row.TryGetValue(column, out string value) && value != null ? value.Trim() : string.Empty;
		}

		private static int ReadInt(IReadOnlyDictionary<string, string> row, string column)
		{
			return int.TryParse(ReadText(row, column), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
		}

		private static bool ReadBool(IReadOnlyDictionary<string, string> row, string column)
		{
			return bool.TryParse(ReadText(row, column), out bool value) && value;
		}
	}
}
