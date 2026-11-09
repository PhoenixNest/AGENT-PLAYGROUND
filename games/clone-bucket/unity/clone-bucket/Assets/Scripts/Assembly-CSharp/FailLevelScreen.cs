using DG.Tweening;
using Levels;
using TMPro;
using UnityEngine;

public class FailLevelScreen : MonoBehaviour
{
	public TextMeshProUGUI ballRatioText;

	private Sequence failLevelScreenSequence;

	private Sequence notAvaliableSequence;

	private Tweener skipLevelBtnTweener;

	private Sequence restartBtnSequence;

	public void OnClickRestartLevel()
	{
		LevelsController.HandleMissionFail();
		failLevelScreenSequence?.Kill();
		notAvaliableSequence?.Kill();
		restartBtnSequence?.Kill();
		if (GameManager.instance != null)
		{
			GameManager.instance.RestartGame();
		}
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayButtonClick();
		}
		TTPAnalyticsService.SendUIInteraction("button", "restartLevel_button", "click", "levelFail");
	}

	private void OnEnable()
	{
		int target = LevelsController.GetTargetBallAmount();
		int filled = LevelsController.TotalFillCount;
		if (ballRatioText != null)
		{
			ballRatioText.text = "<color=\"red\">" + filled + "</color>/" + target;
		}
		HandleGameMusic(false);
	}

	private void HandleGameMusic(bool pauseMusic)
	{
	}

	private void OnDisable()
	{
		failLevelScreenSequence?.Kill();
		notAvaliableSequence?.Kill();
		restartBtnSequence?.Kill();
		skipLevelBtnTweener?.Kill();
	}
}
