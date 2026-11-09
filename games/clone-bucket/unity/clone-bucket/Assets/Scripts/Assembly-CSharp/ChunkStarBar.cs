using DG.Tweening;
using Levels;
using UnityEngine;
using UnityEngine.UI;

public class ChunkStarBar : MonoBehaviour
{
	private const float StarEffectCooldown = 0.25f;

	private const float StarPopScale = 1.2f;

	private const float StarPopDuration = 0.2f;

	private const float ParticleLifetime = 1.3f;

	private const float FirstSegment = 0.33f;

	private const float SecondSegmentStart = 0.33f;

	private const float LastSegmentStart = 0.66f;

	private const float LastSegment = 0.34f;

	private const int MaxStars = 3;

	public Image fill;

	public GameObject[] stars;

	public GameObject[] starParticles;

	private int _currentStarAmount;

	private float _lastStarEffectTime;

	private bool _isActive;

	private void Awake()
	{
		LevelsController.OnLevelSpawned += ActivateVisuals;
		LevelsController.OnBallEnterBucket += FillBar;
		LevelsController.OnLevelSpawned += ResetBar;
		ResetBar();
	}

	private void OnDestroy()
	{
		LevelsController.OnLevelSpawned -= ActivateVisuals;
		LevelsController.OnBallEnterBucket -= FillBar;
		LevelsController.OnLevelSpawned -= ResetBar;
		if (fill != null)
		{
			fill.DOKill();
		}
	}

	private void ActivateVisuals()
	{
		bool active = LevelChunkVisualizer.IsVisible(LevelsController.CurrentMiniLevelCount);
		gameObject.SetActive(active);
		_isActive = active;
	}

	private void ResetBar()
	{
		_currentStarAmount = 0;
		if (fill != null)
		{
			fill.fillAmount = 0f;
		}
		if (stars == null)
		{
			return;
		}
		for (int i = 0; i < stars.Length; i++)
		{
			if (stars[i] != null)
			{
				stars[i].SetActive(false);
			}
		}
	}

	internal static float? FillAmount(int starCount, int filled, int target, int secondStar, int perfect)
	{
		switch (starCount)
		{
			case 0:
				return target > 0 ? filled * FirstSegment / target : (float?)null;
			case 1:
				return secondStar > target ? (filled - target) * FirstSegment / (secondStar - target) + SecondSegmentStart : (float?)null;
			case 2:
				return perfect > secondStar ? (filled - secondStar) * LastSegment / (perfect - secondStar) + LastSegmentStart : (float?)null;
			default:
				return null;
		}
	}

	internal static bool ShouldShowStar(int starCount, int currentStarAmount)
	{
		return starCount >= 1 && starCount <= MaxStars && currentStarAmount == starCount - 1;
	}

	private void FillBar()
	{
		if (!_isActive)
		{
			return;
		}

		int starCount = LevelsController.FilledStarCount;
		float? amount = FillAmount(starCount, LevelsController.TotalFillCountAcrossLevels, LevelsController.GetTargetBallAmount(),
			LevelsController.GetSecondStarAmount(), LevelsController.GetPerfectBallAmount());
		if (amount.HasValue && fill != null)
		{
			fill.fillAmount = amount.Value;
		}

		if (ShouldShowStar(starCount, _currentStarAmount))
		{
			_currentStarAmount = starCount;
			ShowStar(starCount - 1);
		}

		if (_currentStarAmount >= MaxStars || !IsStarEffectReady(Time.time, _lastStarEffectTime, StarEffectCooldown))
		{
			return;
		}
		_lastStarEffectTime = Time.time;
		SpawnStarParticle(_currentStarAmount);
	}

	private void ShowStar(int index)
	{
		if (stars == null || index < 0 || index >= stars.Length || stars[index] == null)
		{
			return;
		}
		GameObject star = stars[index];
		star.SetActive(true);
		Transform starTransform = star.transform;
		starTransform.localScale = Vector3.zero;
		starTransform.DOScale(Vector3.one * StarPopScale, StarPopDuration);
		DOVirtual.DelayedCall(StarPopDuration, () =>
		{
			if (starTransform != null)
			{
				starTransform.DOScale(Vector3.one, StarPopDuration);
			}
		});
	}

	private void SpawnStarParticle(int index)
	{
		if (starParticles == null || index < 0 || index >= starParticles.Length || starParticles[0] == null || starParticles[index] == null)
		{
			return;
		}
		GameObject particle = Instantiate(starParticles[index], starParticles[0].transform.parent);
		particle.SetActive(true);
		Destroy(particle, ParticleLifetime);
	}

	private float Remap(float s, float a1, float a2, float b1, float b2)
	{
		return RemapLinear(s, a1, a2, b1, b2);
	}

	internal static float RemapLinear(float s, float a1, float a2, float b1, float b2)
	{
		if (Mathf.Approximately(a2, a1))
		{
			return b1;
		}
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}

	internal static bool IsStarEffectReady(float now, float lastEffectTime, float cooldown)
	{
		return now - lastEffectTime > cooldown;
	}

	public void Fill(float percent)
	{
		if (fill != null)
		{
			fill.fillAmount = percent;
		}
	}
}
