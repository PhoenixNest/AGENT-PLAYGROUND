using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class CompleteLevelScreen : MonoBehaviour
{
	public GameObject NextLevelBtn;

	public GameObject CompleteLevelBG;

	public TextMeshProUGUI CompleteLevelText;

	public GameObject FilledStar1;

	public GameObject FilledStar2;

	public GameObject FilledStar3;

	public Text Score;

	public Text BestScore;

	public GameObject GemPrefab;

	public TextMeshProUGUI GemCounter;

	public Image GemImage;

	public GameObject GemsPanel;

	public LeaderBoardView LeaderBoard;

	public Animator GemAnimator;

	public GameObject particles1;

	public GameObject particles2;

	private bool nextButtonLock;

	private bool isFirstShownBtns;

	private Sequence completeLevelSequence;

	private bool nextLevelTriggered;

	private bool _nextLevelDone;

	private static readonly Vector3 NextButtonPunch = Vector3.one * 0.05f;

	private const float NextButtonPunchSeconds = 0.5f;

	private const int RateUsEveryLevels = 10;

	public void OnClickNextLevel()
	{
		if (nextButtonLock)
		{
			return;
		}
		nextButtonLock = true;
		NextLevel();
	}

	private void NextLevel()
	{
		if (nextLevelTriggered)
		{
			return;
		}
		nextLevelTriggered = true;
		SetActiveIf(NextLevelBtn, false);
		Proceed();
	}

	private void Proceed()
	{
		if (_nextLevelDone)
		{
			return;
		}
		_nextLevelDone = true;
		if (GameManager.instance != null)
		{
			GameManager.instance.StartCoroutine(GameManager.instance.DelayedNextLevel());
		}
	}

	private void OnEnable()
	{
		nextLevelTriggered = false;
		_nextLevelDone = false;
		nextButtonLock = true;
		isFirstShownBtns = false;

		int stars = LevelsController.FilledStarCount;
		SetActiveIf(FilledStar1, true);
		SetActiveIf(FilledStar2, stars > 1);
		SetActiveIf(FilledStar3, stars > 2);

		if (Score != null)
		{
			Score.text = UIUtils.ToSI(LevelsController.TotalFillCountAcrossLevels);
		}
		GameManager gm = GameManager.instance;
		if (gm != null)
		{
			if (BestScore != null)
			{
				BestScore.text = UIUtils.ToSI(gm.BestScore);
			}
			SetGems(gm.GemCounter);
			if (CompleteLevelText != null)
			{
				CompleteLevelText.text = LevelLabel(gm.CurrentLevel);
			}
		}

		SetActiveIf(NextLevelBtn, false);
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayCompleteLevel();
		}
	}

	internal static string LevelLabel(int level)
	{
		return "Level " + level;
	}

	private void OnDisable()
	{
		SetActiveIf(particles1, false);
		SetActiveIf(particles2, false);
		if (GemAnimator != null)
		{
			GemAnimator.enabled = false;
		}
		StopAllCoroutines();
		completeLevelSequence?.Kill();
	}

	private void HandleGameMusic(bool pauseMusic)
	{
	}

	public void ShowNextBtn()
	{
		nextButtonLock = false;
		if (GameManager.instance != null && GameManager.instance.CurrentLevel % RateUsEveryLevels == 0 && TTPManager.Instance != null)
		{
			TTPManager.Instance.ShowRateUsPopup();
		}
		ShowNextBtnExtend();
	}

	private void ShowNextBtnExtend()
	{
		SetActiveIf(NextLevelBtn, true);
		if (NextLevelBtn != null)
		{
			NextLevelBtn.transform.DOPunchScale(NextButtonPunch, NextButtonPunchSeconds, 10, 1f);
		}
		nextButtonLock = false;
	}

	public void SetGems(int value)
	{
		if (GemCounter != null)
		{
			GemCounter.text = UIUtils.ToSI(value);
		}
	}

	private static void SetActiveIf(GameObject target, bool active)
	{
		if (target != null && target.activeSelf != active)
		{
			target.SetActive(active);
		}
	}
}
