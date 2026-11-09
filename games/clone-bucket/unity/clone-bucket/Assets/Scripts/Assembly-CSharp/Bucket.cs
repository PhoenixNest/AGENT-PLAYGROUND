using DG.Tweening;
using Lean.Pool;
using Levels;
using UnityEngine;

public class Bucket : MonoBehaviour
{
	internal const float CollectFxCooldown = 0.25f;

	internal const float ShakeDuration = 0.2f;

	private const float ShakeStrength = 0.08f;

	private const float CollectFxLifetime = 1f;

	private const float CollectFxHeightOffset = 1.2f;

	private const float MoveEpsilon = 0.0001f;

	internal const float CollectFxThreshold = 0.15f;

	internal const int OverfillThreshold = 35;

	public int CurrentFillCount;

	public GameObject DestroyTrigger;

	public GameObject FillTrigger;

	public GameObject bucketCollectFX;

	private Tweener shakeTweener;

	private float bucketCollectFxTimer = CollectFxCooldown;

	private CameraMove cameraMove;

	private bool hasPosY;

	private float lastPosY;

	private bool shakeVisualSearched;

	private Transform shakeVisual;

	private Vector3 shakeVisualRestPosition;

	internal static bool ShouldSpawnCollectFx(bool filledBefore, float timeSinceLastFx)
	{
		return timeSinceLastFx > CollectFxThreshold;
	}

	public void OnFill(bool filledBefore, int amount = 1)
	{
		CurrentFillCount += amount;
		LevelsController.OnFillBucket(filledBefore, amount);
		OverFillCheck();
		if (ShouldSpawnCollectFx(filledBefore, bucketCollectFxTimer))
		{
			SpawnCollectFx();
		}
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayBucketFill();
		}
	}

	public void OnOut()
	{
		CurrentFillCount -= 1;
		OverFillCheck();
		KillShake();
	}

	public void Shake()
	{
		Transform visual = FindShakeVisual();
		if (visual == null)
		{
			return;
		}
		KillShake();
		shakeTweener = visual.DOShakePosition(ShakeDuration, ShakeStrength);
	}

	private void SpawnCollectFx()
	{
		if (LevelsController.MiniLevelView == null)
		{
			return;
		}
		if (bucketCollectFX == null)
		{
			return;
		}
		Vector3 position = transform.position + Vector3.up * CollectFxHeightOffset;
		GameObject clone = LeanPool.Spawn(bucketCollectFX, position, Quaternion.identity, LevelsController.MiniLevelView.transform);
		if (clone != null)
		{
			LeanPool.Despawn(clone, CollectFxLifetime);
		}
		bucketCollectFxTimer = 0f;
	}

	private void OverFillCheck()
	{
		if (DestroyTrigger != null)
		{
			DestroyTrigger.SetActive(CurrentFillCount > OverfillThreshold);
		}
	}

	private Transform FindShakeVisual()
	{
		if (!shakeVisualSearched)
		{
			shakeVisualSearched = true;
			Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
			for (int i = 0; i < renderers.Length; i++)
			{
				if (renderers[i].GetComponent<Collider>() == null)
				{
					shakeVisual = renderers[i].transform;
					shakeVisualRestPosition = shakeVisual.localPosition;
					break;
				}
			}
		}
		return shakeVisual;
	}

	private void KillShake()
	{
		if (shakeTweener != null)
		{
			shakeTweener.Kill();
			shakeTweener = null;
		}
		if (shakeVisual != null)
		{
			shakeVisual.localPosition = shakeVisualRestPosition;
		}
	}

	private void Update()
	{
		bucketCollectFxTimer += Time.deltaTime;

		if (cameraMove == null)
		{
			cameraMove = FindAnyObjectByType<CameraMove>();
		}
		if (cameraMove == null)
		{
			return;
		}

		float posY = transform.position.y;
		if (!hasPosY || Mathf.Abs(posY - lastPosY) > MoveEpsilon)
		{
			hasPosY = true;
			lastPosY = posY;
			cameraMove.PosYLimitUpdate(posY);
		}
	}

	private void OnDisable()
	{
		KillShake();
	}

	private void OnDestroy()
	{
		KillShake();
	}
}
