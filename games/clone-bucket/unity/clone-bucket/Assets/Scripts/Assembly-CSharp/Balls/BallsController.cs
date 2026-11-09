using System.Collections.Generic;
using DG.Tweening;
using Levels;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Balls
{
	public class BallsController : MonoBehaviour
	{
		private const string MultiplicatorTag = "Multiplicator";

		private const string MazeSpaceTag = "MazeSpace";

		private const string BucketFillTriggerTag = "BucketFillTrigger";

		private const string BucketDestroyTriggerTag = "BucketDestroyTrigger";

		private const string SubGateTag = "SubGate";

		private const string DivideGateTag = "DivideGate";

		private const string FinishSpaceTag = "FinishSpace";

		private const string DestroyTriggerTag = "DestroyTrigger";

		private const string BucketShakeTriggerTag = "BucketShakeTrigger";

		private const int DefaultBallLayer = 9;

		private static BallsController _instance;

		private IColorSchemeController _colorSchemeController;

		[SerializeField]
		[FormerlySerializedAs("_androidConfig")]
		private BallsConfig _defaultConfig;

		[SerializeField]
		private BallsConfig _iosConfig;

		private BallsConfig _config;

		private CameraMove _cameraMover;

		private List<BallView> _allViews;

		private Dictionary<BallView, List<Collider>> _ignoredMultiplicatorsByBalls;

		private Dictionary<BallView, BallData> _dataByBalls;

		private Dictionary<Collider, BallView> _ballsByColliders;

		private bool _isBossLevel;

		private int _currentBallCount;

		private int _totalMultipliedBallsRequested;

		private Bucket _bucket;

		[Inject]
		private void Construct(IColorSchemeController colorSchemeController)
		{
			_colorSchemeController = colorSchemeController;
		}

		private void Awake()
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);
			_allViews = new List<BallView>();
			_ignoredMultiplicatorsByBalls = new Dictionary<BallView, List<Collider>>();
			_dataByBalls = new Dictionary<BallView, BallData>();
			_ballsByColliders = new Dictionary<Collider, BallView>();

			_config = SelectConfig(Application.platform, _defaultConfig, _iosConfig);
			if (_config == null)
			{
				Debug.LogError("[MM3:Balls] No BallsConfig assigned for this platform.");
				return;
			}

			BallsPool.OnBallInstantiated += HandleBallInstantiated;
			BallsPool.Initialize(_config, transform);
		}

		private void OnDestroy()
		{
			BallsPool.OnBallInstantiated -= HandleBallInstantiated;
			if (_instance == this)
			{
				_instance = null;
			}
		}

		public void SetBossLevel(bool isBossLevel)
		{
			_isBossLevel = isBossLevel;
		}

		private void FixedUpdate()
		{
			for (int i = _allViews.Count - 1; i >= 0; i--)
			{
				BallView view = _allViews[i];
				if (view == null || !_dataByBalls.TryGetValue(view, out BallData data) || !data.isActive)
				{
					continue;
				}

				HandleSpeedOnFinishSpace(view, data);
				HandleRotation(view);
			}
		}

		private static void HandleRotation(BallView view)
		{
			if (view == null || view.skinRotator == null)
			{
				return;
			}

			Transform rotator = view.skinRotator;
			Vector3 euler = rotator.rotation.eulerAngles;
			rotator.rotation = Quaternion.Euler(0f, 0f, euler.z);
		}

		private void HandleSpeedOnFinishSpace(BallView view, BallData data)
		{
			if (_isBossLevel || !data.inFinishSpace)
			{
				return;
			}

			Rigidbody body = view.rigidbodyCached;
			if (body == null)
			{
				return;
			}

			Vector3 velocity = body.linearVelocity;
			if (!data.inMaze && !data.inBucket)
			{
				velocity.y = -data.maxSpeed / 6f;
			}
			body.linearVelocity = CapVelocity(velocity, data.maxSpeed);
		}

		private void Cache(BallView view)
		{
			if (view == null)
			{
				return;
			}

			if (!_allViews.Contains(view))
			{
				_allViews.Add(view);
			}
			_dataByBalls[view] = new BallData { isActive = true };
			_ignoredMultiplicatorsByBalls[view] = NewGateHistory();
			if (view.colliderCached != null)
			{
				_ballsByColliders[view.colliderCached] = view;
			}
		}

		public static BallView Spawn(Vector3 position, Transform parent)
		{
			BallsController controller = _instance;
			if (controller == null || controller._config == null)
			{
				Debug.LogError("[MM3:Balls] Spawn called without an active BallsController.");
				return null;
			}

			if (controller._cameraMover == null)
			{
				controller._cameraMover = FindAnyObjectByType<CameraMove>();
			}

			BallView view = BallsPool.Get();
			if (view == null)
			{
				return null;
			}
			view.CacheFieldsIfNeeded();

			view.transformCached.SetParent(parent, false);
			view.transformCached.position = position;
			view.rigidbodyCached.linearVelocity = Vector3.zero;
			view.rigidbodyCached.angularVelocity = Vector3.zero;

			controller.Cache(view);
			controller.ClearBallData(view);
			LevelsController.AddTotal();
			controller.SetSizeOfBall(view);
			if (controller._isBossLevel && controller._cameraMover != null)
			{
				controller._cameraMover.AddBall(view.transformCached);
			}
			controller._currentBallCount++;
			if (controller._config.useBallLayers)
			{
				controller.ApplyPhysicalLayerToBall(view);
			}

			if (view.meshRendererCached != null)
			{
				Material material = controller._colorSchemeController != null
					? controller._colorSchemeController.GetItemMaterial(ColorSchemeItemType.Balls)
					: null;
				if (material == null && MaterialManager.instance != null)
				{
					material = MaterialManager.instance.GetBallMaterial();
				}
				if (material != null)
				{
					view.meshRendererCached.sharedMaterial = material;
				}
			}

			return view;
		}

		private void ClearBallData(BallView view)
		{
			if (view == null || !_dataByBalls.TryGetValue(view, out BallData data))
			{
				return;
			}

			data.maxSpeed = _config.inMazeMaxSpeed;
			data.isActive = true;
			data.bucketInteraction = false;
			data.inBucket = false;
			data.inFinishSpace = false;
			data.inMaze = true;
			data.divisionStatus = false;
			data.value = 1;
			if (_ignoredMultiplicatorsByBalls.TryGetValue(view, out List<Collider> history))
			{
				history.Clear();
			}
		}

		private void ApplyPhysicalLayerToBall(BallView ball)
		{
			if (ball == null || ball.gameObjectCached == null || _config == null)
			{
				return;
			}

			int layer = DefaultBallLayer;
			int reached = ThresholdsReached(_currentBallCount, _config.minimumBallsForLayers);
			int[] layers = _config.ballLayers;
			if (reached > 0 && layers != null && layers.Length > 0)
			{
				int index = Mathf.Min(RandomLayerIndex(reached), layers.Length - 1);
				layer = layers[index];
			}
			ball.gameObjectCached.layer = layer;
		}

		public static int GetBallValue(BallView view)
		{
			if (view == null || _instance == null || _instance._dataByBalls == null)
			{
				return 0;
			}
			return _instance._dataByBalls.TryGetValue(view, out BallData data) ? data.value : 0;
		}

		private void RemoveFromCamera(BallView view)
		{
			if (_isBossLevel && _cameraMover != null && view != null)
			{
				_cameraMover.DeleteBall(view.transformCached);
			}
		}

		public void Despawn(BallView view)
		{
			if (view == null || !_dataByBalls.TryGetValue(view, out BallData data) || !data.isActive)
			{
				return;
			}

			RemoveFromCamera(view);
			data.isActive = false;
			_currentBallCount = Mathf.Max(0, _currentBallCount - 1);
			BallsPool.Release(view);
		}

		private void Destroy(BallView view)
		{
			if (view == null || !_dataByBalls.ContainsKey(view))
			{
				return;
			}

			ClearBallData(view);
			RemoveFromCamera(view);
			_allViews.Remove(view);
			_currentBallCount = Mathf.Max(0, _currentBallCount - 1);
			BallsPool.Release(view);
		}

		private void SetSizeOfBall(BallView view)
		{
			BallSize size = SizeForRoll(Random.Range(0, 3));
			Vector3 scale;
			switch (size)
			{
				case BallSize.Small:
					scale = _config.smallScale;
					break;
				case BallSize.Medium:
					scale = _config.mediumScale;
					break;
				default:
					scale = _config.bigScale;
					break;
			}
			view.transformCached.localScale = scale;
		}

		private void OnBallTriggerEntered(BallView view, Collider other)
		{
			if (view == null || other == null || !_dataByBalls.TryGetValue(view, out BallData data))
			{
				return;
			}

			if (data.divisionStatus)
			{
				return;
			}

			if (other.CompareTag(MazeSpaceTag))
			{
				view.transformCached.SetParent(other.transform);
				data.maxSpeed = _config.inMazeMaxSpeed;
				data.inMaze = true;
				return;
			}

			if (other.CompareTag(BucketFillTriggerTag))
			{
				Bucket fillBucket = GetBucket(other);
				if (fillBucket != null)
				{
					fillBucket.OnFill(data.bucketInteraction, data.value);
				}
				data.inBucket = true;
				return;
			}

			if (other.CompareTag(BucketDestroyTriggerTag))
			{
				Bucket destroyBucket = GetBucket(other);
				if (destroyBucket != null)
				{
					destroyBucket.OnOut();
				}
				Despawn(view);
				return;
			}

			if (other.CompareTag(SubGateTag))
			{
				SubGate subGate = GatesController.GetSubGate(other.transform);
				if (subGate != null && subGate.Sub())
				{
					LevelsController.SubtractTotal(data.value);
					Despawn(view);
					LevelsController.StageCheck();
				}
				return;
			}

			if (other.CompareTag(DivideGateTag))
			{
				if (!RememberGate(view, other))
				{
					return;
				}
				DivideGate divideGate = GatesController.GetDivideGate(other.transform);
				if (divideGate != null)
				{
					divideGate.Divide(view);
				}
				return;
			}

			if (other.CompareTag(FinishSpaceTag))
			{
				data.inMaze = false;
				data.inFinishSpace = true;
				data.maxSpeed = _config.outMazeMaxSpeed;
				return;
			}

			if (other.CompareTag(MultiplicatorTag))
			{
				Multiplicator multiplicator = GatesController.GetMultiplicator(other.transform);
				if (multiplicator != null)
				{
					Multiply(view, multiplicator, other, view.transformCached.parent);
				}
			}
		}

		private bool Multiply(BallView view, Multiplicator multiplicator, Collider other, Transform parent)
		{
			if (!RememberGate(view, other))
			{
				return false;
			}

			if (multiplicator.isMoveX)
			{
				multiplicator.StopMove();
			}

			if (!_dataByBalls.TryGetValue(view, out BallData data))
			{
				return false;
			}

			int ballValue = data.value;
			int extraPerUnit = Multiplicator.MultiplyClones(multiplicator.Value);
			bool useTiers = _config.useBallValueTiers;
			for (int unit = 0; unit < ballValue; unit++)
			{
				for (int j = 0; j < extraPerUnit; j++)
				{
					if (!useTiers)
					{
						SpawnClone(view, parent, multiplicator, other);
						continue;
					}

					_totalMultipliedBallsRequested++;
					int tier = GetCurrentValueTierIndex();
					int cap = ValueForTier(_config, tier);
					if (cap < data.value + 1)
					{
						SpawnClone(view, parent, multiplicator, other);
					}
					else if (Random.value < ChanceForTier(_config, tier))
					{
						SpawnClone(view, parent, multiplicator, other);
					}
					else
					{
						LevelsController.AddTotal();
						data.value++;
					}
				}
			}

			multiplicator.MultiEffect();
			return true;
		}

		private void SpawnClone(BallView parentView, Transform parent, Multiplicator multiplicator, Collider gate)
		{
			Vector3 origin = gate.transform.position;
			float halfX = 0.1f;
			float halfY = 0.05f;
			float baseY = origin.y;
			if (_isBossLevel)
			{
				halfX = multiplicator.visualMeshRenderer.transform.localScale.x * 0.5f;
				halfY = 0.1f;
				baseY = 0.5f;
			}

			Vector3 position = new Vector3(
				origin.x + Random.Range(-halfX, halfX),
				baseY + Random.Range(-halfY, halfY),
				origin.z + Random.Range(-0.2f, 0.2f));

			BallView clone = Spawn(position, parent);
			if (clone == null)
			{
				return;
			}

			clone.transformCached.SetPositionAndRotation(position, parentView.transformCached.rotation);

			if (_ignoredMultiplicatorsByBalls.TryGetValue(parentView, out List<Collider> parentHistory))
			{
				_ignoredMultiplicatorsByBalls[clone] = new List<Collider>(parentHistory);
			}
		}

		private int GetCurrentValueTierIndex()
		{
			if (_config == null || _config.valuePerTier == null || _config.valuePerTier.Length == 0)
			{
				return 0;
			}
			return ValueTierIndex(_totalMultipliedBallsRequested, _config.totalBallsForNextTier, _config.valuePerTier.Length);
		}

		private void OnBallTriggerExited(BallView view, Collider other)
		{
			if (view == null || other == null || !_dataByBalls.TryGetValue(view, out BallData data))
			{
				return;
			}

			if (other.CompareTag(MazeSpaceTag))
			{
				MiniLevelView owner = other.GetComponentInParent<MiniLevelView>();
				if (owner != null)
				{
					view.transformCached.SetParent(owner.transform);
				}
				data.maxSpeed = _config.outMazeMaxSpeed;
				data.inMaze = false;
				return;
			}

			if (other.CompareTag(BucketFillTriggerTag))
			{
				data.bucketInteraction = true;
				data.inBucket = false;
				Bucket bucket = GetBucket(other);
				if (bucket != null)
				{
					bucket.OnOut();
				}
				return;
			}

			if (other.CompareTag(DestroyTriggerTag))
			{
				Despawn(view);
			}
		}

		public static void DoBuffAnimation(Vector3 position, Quaternion rotation, Transform parent)
		{
		}

		public static void Divide(BallView view)
		{
			BallsController controller = _instance;
			if (controller == null || view == null || !controller._dataByBalls.TryGetValue(view, out BallData data))
			{
				return;
			}

			data.divisionStatus = true;
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval(0.25f);
			sequence.Append(view.transform.DOScale(Vector3.zero, 0.5f));
			sequence.AppendCallback(() =>
			{
				if (controller != null)
				{
					controller.FinishDivide(view);
				}
			});
			sequence.SetLink(view.gameObject, LinkBehaviour.KillOnDisable);
		}

		private void FinishDivide(BallView view)
		{
			if (view == null || !_dataByBalls.TryGetValue(view, out BallData data) || !data.divisionStatus || !data.isActive)
			{
				return;
			}
			Despawn(view);
		}

		public static void SendAllToPool()
		{
			BallsController controller = _instance;
			if (controller == null)
			{
				return;
			}

			for (int i = controller._allViews.Count - 1; i >= 0; i--)
			{
				BallView view = controller._allViews[i];
				if (view != null)
				{
					controller.Despawn(view);
				}
			}

			controller._currentBallCount = 0;
			controller._totalMultipliedBallsRequested = 0;
		}

		public static int GetBallValue(Collider collider)
		{
			if (collider == null || _instance == null || _instance._ballsByColliders == null)
			{
				return 0;
			}
			return _instance._ballsByColliders.TryGetValue(collider, out BallView view) ? GetBallValue(view) : 0;
		}

		private void HandleBallInstantiated(BallView view)
		{
			view.OnTriggerEntered += other =>
			{
				if (this != null)
				{
					OnBallTriggerEntered(view, other);
				}
			};
			view.OnTriggerExited += other =>
			{
				if (this != null)
				{
					OnBallTriggerExited(view, other);
				}
			};
		}

		private Bucket GetBucket(Collider trigger)
		{
			Bucket found = trigger != null ? trigger.GetComponentInParent<Bucket>() : null;
			if (found == null)
			{
				if (_bucket == null)
				{
					_bucket = FindAnyObjectByType<Bucket>();
				}
				found = _bucket;
			}
			else
			{
				_bucket = found;
			}
			return found;
		}

		private List<Collider> NewGateHistory()
		{
			int capacity = _config != null ? _config.ignoreMultiplicatorCapacity : 0;
			return new List<Collider>(Mathf.Max(0, capacity));
		}

		private bool RememberGate(BallView view, Collider gate)
		{
			if (!_ignoredMultiplicatorsByBalls.TryGetValue(view, out List<Collider> history))
			{
				history = NewGateHistory();
				_ignoredMultiplicatorsByBalls[view] = history;
			}
			return TryRemember(history, gate);
		}

		private static BallsConfig SelectConfig(RuntimePlatform platform, BallsConfig defaultConfig, BallsConfig ios)
		{
			if (platform == RuntimePlatform.IPhonePlayer && ios != null)
			{
				return ios;
			}
			return defaultConfig != null ? defaultConfig : ios;
		}

		private static int ValueTierIndex(int totalRequested, int[] thresholds, int tierCount)
		{
			if (tierCount <= 0)
			{
				return 0;
			}

			int index = 0;
			if (thresholds != null)
			{
				for (int i = 0; i < thresholds.Length; i++)
				{
					index = totalRequested <= thresholds[i] ? index : i + 1;
				}
			}
			return Mathf.Min(index, tierCount - 1);
		}

		private static int ValueForTier(BallsConfig config, int tierIndex)
		{
			if (config == null || !config.useBallValueTiers || config.valuePerTier == null || config.valuePerTier.Length == 0)
			{
				return 1;
			}
			return config.valuePerTier[Mathf.Clamp(tierIndex, 0, config.valuePerTier.Length - 1)];
		}

		private static float ChanceForTier(BallsConfig config, int tierIndex)
		{
			if (config == null || config.chancesToOverrideTier == null || tierIndex < 0 || tierIndex >= config.chancesToOverrideTier.Length)
			{
				return 0f;
			}
			return config.chancesToOverrideTier[tierIndex];
		}

		private static int ThresholdsReached(int count, int[] minimums)
		{
			if (minimums == null)
			{
				return 0;
			}

			int reached = 0;
			for (int i = 0; i < minimums.Length; i++)
			{
				if (minimums[i] <= count)
				{
					reached++;
				}
			}
			return reached;
		}

		private static int RandomLayerIndex(int reached)
		{
			if (reached <= 1)
			{
				return 0;
			}
			return Random.Range(0, reached - 1);
		}

		private static Vector3 CapVelocity(Vector3 velocity, float maxSpeed)
		{
			if (maxSpeed <= 0f)
			{
				return velocity;
			}

			float sqr = velocity.sqrMagnitude;
			if (sqr <= maxSpeed * maxSpeed)
			{
				return velocity;
			}
			return velocity * (maxSpeed / Mathf.Sqrt(sqr));
		}

		private static BallSize SizeForRoll(int roll)
		{
			if (roll == 2)
			{
				return BallSize.Big;
			}
			return roll == 1 ? BallSize.Medium : BallSize.Small;
		}

		private static bool TryRemember<T>(List<T> list, T item)
		{
			if (list.Contains(item))
			{
				return false;
			}
			list.Add(item);
			return true;
		}
	}
}
