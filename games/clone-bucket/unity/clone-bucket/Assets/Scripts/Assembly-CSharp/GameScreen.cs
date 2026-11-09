using System;
using System.Runtime.CompilerServices;
using AssetKits;
using DG.Tweening;
using Lean.Pool;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : MonoBehaviour
{
	[CompilerGenerated]
	private static Action m_OnClickContinue;

	private static GameScreen _instance;

	public ProgressBar progressBar;

	public Button RestartBtn;

	public Image BallsPanel;

	public TextMeshProUGUI LevelText;

	public GameObject LevelTextbg;

	public TextMeshProUGUI BallsCounterText;

	public Button CompleteLevelBtn;

	public Button RestartLevelBtn;

	public GameObject StarsPanel;

	public GameObject ChunkVisuals;

	public Image UnfilledStar1;

	public Image UnfilledStar2;

	public Image UnfilledStar3;

	public Image FilledStar1;

	public Image FilledStar2;

	public Image FilledStar3;

	public Animator fillStar1Anim;

	public Animator fillStar2Anim;

	public Animator fillStar3Anim;

	public ParticleSystem StarFX1;

	public ParticleImage StarFX2;

	public ParticleImage StarFX3;

	public GameObject StarFX;

	public Transform Stars;

	public CreditScoreBar BetterThanCreditScore;

	public Animator ballsCounterAnim;

	public Image ballLightImage;

	public Transform BallsCounter;

	public bool completeBtnLock;

	private bool restartBtnLock;

	private Vector2 defaultCompleteLevelBtnPos;

	private Vector2 defaultRestartLevelBtnPos;

	private int ballsCounterCurrentValue;

	private int ballsCounterTargetValue;

	private Sequence gameScreenSequence;

	private Tweener starTween;

	private Tweener ballsTween;

	private float starFXTimer;

	private const float StarFXCooldown = 0.25f;

	private const float BallsCounterRate = 4f;

	private const float RestartLockSeconds = 0.5f;

	private const float LevelTextRefreshDelay = 0.05f;

	private const float IntroFadeDuration = 0.5f;

	private const float IntroZoomDuration = 0.3f;

	private const float ButtonsInDuration = 0.5f;

	private const float ButtonSlideOffset = 250f;

	private const float StarProgressDelay = 1.3f;

	private static readonly Vector3 StarFXLocalPosition = new Vector3(30f, -434f, 0f);

	public static event Action OnClickContinue
	{
		[CompilerGenerated]
		add
		{
			m_OnClickContinue = (Action)Delegate.Combine(m_OnClickContinue, value);
		}
		[CompilerGenerated]
		remove
		{
			m_OnClickContinue = (Action)Delegate.Remove(m_OnClickContinue, value);
		}
	}

	public static bool IsActive()
	{
		return _instance != null && _instance.isActiveAndEnabled;
	}

	private void Awake()
	{
		_instance = this;
		defaultCompleteLevelBtnPos = AnchoredPosition(CompleteLevelBtn);
		defaultRestartLevelBtnPos = AnchoredPosition(RestartLevelBtn);
		LevelsController.OnChunkStarted += DisableRestartLock;
		LevelsController.OnChunkCompleted += DisableCompleteButton;
	}

	private void OnDestroy()
	{
		LevelsController.OnChunkStarted -= DisableRestartLock;
		LevelsController.OnChunkCompleted -= DisableCompleteButton;
		if (_instance == this)
		{
			_instance = null;
		}
		ballsTween?.Kill();
	}

	private void OnEnable()
	{
		completeBtnLock = true;
		restartBtnLock = true;
		Canvas.ForceUpdateCanvases();
		SetAnchoredPosition(CompleteLevelBtn, defaultCompleteLevelBtnPos);
		SetAnchoredPosition(RestartLevelBtn, defaultRestartLevelBtnPos);
		SetActive(CompleteLevelBtn, false);
		SetActive(RestartLevelBtn, false);
		SetActive(LevelText, true);
		if (LevelTextbg != null)
		{
			LevelTextbg.SetActive(true);
		}
		DOTween.Sequence().AppendInterval(LevelTextRefreshDelay).OnComplete(RefreshLevelTextBackground);
		if (LevelText != null)
		{
			LevelText.color = Color.white;
		}

		gameScreenSequence = DOTween.Sequence();
		if (LevelText != null)
		{
			gameScreenSequence.Insert(0f, UIEffector.FadeInText(LevelText, IntroFadeDuration, Ease.OutQuad));
		}
		if (RestartBtn != null)
		{
			gameScreenSequence.Insert(0f, UIEffector.ZoomInElement(RestartBtn.gameObject, IntroZoomDuration));
		}
		if (BallsPanel != null)
		{
			gameScreenSequence.Insert(0f, UIEffector.ZoomInElement(BallsPanel.gameObject, IntroZoomDuration));
		}
		gameScreenSequence.OnComplete(ReleaseButtonLocks);

		if (StarsPanel != null)
		{
			StarsPanel.SetActive(false);
		}
		SetFill(FilledStar1, 0f);
		SetFill(FilledStar2, 0f);
		SetFill(FilledStar3, 0f);
		SetScale(UnfilledStar1, Vector3.one);
		SetScale(UnfilledStar2, Vector3.one);
		SetScale(UnfilledStar3, Vector3.one);
		starFXTimer = 0f;
		ballsCounterCurrentValue = 0;
		SetActive(BetterThanCreditScore, false);
		if (BallsPanel != null)
		{
			BallsPanel.gameObject.SetActive(true);
		}
	}

	private void OnDisable()
	{
		ballsTween?.Kill();
		gameScreenSequence?.Kill();
		starTween?.Kill();
		CancelInvoke();
		DeactivateStarPanels();
		if (BallsPanel != null)
		{
			BallsPanel.gameObject.SetActive(false);
		}
	}

	private void RefreshLevelTextBackground()
	{
		if (LevelTextbg == null)
		{
			return;
		}
		LevelTextbg.SetActive(false);
		LevelTextbg.SetActive(true);
		Canvas.ForceUpdateCanvases();
	}

	private void ReleaseButtonLocks()
	{
		completeBtnLock = false;
		restartBtnLock = false;
	}

	public void DeactivateStarPanels()
	{
		if (StarsPanel != null)
		{
			StarsPanel.SetActive(false);
		}
		SetActive(BetterThanCreditScore, false);
		SetFill(UnfilledStar1, 0f);
		SetFill(UnfilledStar2, 0f);
		SetFill(UnfilledStar3, 0f);
		SetFill(FilledStar1, 0f);
		SetFill(FilledStar2, 0f);
		SetFill(FilledStar3, 0f);
		if (fillStar2Anim != null)
		{
			fillStar2Anim.enabled = false;
		}
		if (fillStar3Anim != null)
		{
			fillStar3Anim.enabled = false;
		}
	}

	private void DisableCompleteButton()
	{
		SetActive(CompleteLevelBtn, false);
	}

	private void DisableRestartLock()
	{
		ReleaseButtonLocks();
		if (RestartBtn != null)
		{
			RestartBtn.interactable = true;
		}
	}

	public void OnClickRestart()
	{
		if (restartBtnLock)
		{
			return;
		}
		restartBtnLock = true;
		if (RestartBtn != null)
		{
			RestartBtn.interactable = false;
		}
		if (GameManager.instance != null)
		{
			GameManager.instance.RestartGame();
		}
		Invoke(nameof(DisableRestartLock), RestartLockSeconds);
	}

	public void OnClickComplete()
	{
		if (completeBtnLock)
		{
			return;
		}
		TTPAnalyticsService.SendUIInteraction("button", "complete_button", "click", "main_screen");
		completeBtnLock = true;
		restartBtnLock = true;
		m_OnClickContinue?.Invoke();
		LevelsController.StartNextChunk();
	}

	public void ShowCompleteLevelBtns()
	{
		SetActive(CompleteLevelBtn, true);
		SetActive(RestartLevelBtn, true);
		if (LevelsController.CurrentMiniLevelCount == 1)
		{
			if (ChunkVisuals != null)
			{
				ChunkVisuals.SetActive(false);
			}
			SetActive(LevelText, false);
			if (LevelTextbg != null)
			{
				LevelTextbg.SetActive(false);
			}
			if (StarsPanel != null)
			{
				StarsPanel.SetActive(true);
			}
		}
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayStarAppears();
		}
		SetFill(FilledStar1, 1f);

		gameScreenSequence = DOTween.Sequence();
		if (CompleteLevelBtn != null)
		{
			gameScreenSequence.Insert(0f, UIEffector.SlideInRightElement(CompleteLevelBtn.gameObject, -ButtonSlideOffset, ButtonsInDuration));
		}
		if (RestartLevelBtn != null)
		{
			gameScreenSequence.Insert(0f, UIEffector.SlideInRightElement(RestartLevelBtn.gameObject, ButtonSlideOffset, ButtonsInDuration));
		}
		if (RestartBtn != null)
		{
			gameScreenSequence.Insert(0f, UIEffector.ZoomOutElement(RestartBtn.gameObject, ButtonsInDuration));
		}
		if (StarsPanel != null)
		{
			gameScreenSequence.Insert(0f, UIEffector.ZoomInElement(StarsPanel, ButtonsInDuration));
		}
	}

	public void SetProgress(float progressValue)
	{
		if (progressBar != null)
		{
			progressBar.SetProgress(progressValue);
		}
	}

	public void SetBalls(int value)
	{
		ballsCounterTargetValue = value;
	}

	public void SetLevel(int value)
	{
		if (LevelText != null)
		{
			LevelText.text = value.ToString();
		}
	}

	internal static bool IsStarFxAllowed(float starFxTimer, float progress)
	{
		return starFxTimer >= StarFXCooldown || progress == 1f;
	}

	public void FillStarEffect(int startIndex, float progress, Vector3 startFXpos)
	{
		bool allowFx = IsStarFxAllowed(starFXTimer, progress);
		if (startIndex == 0)
		{
			SetFill(UnfilledStar1, progress);
			return;
		}

		starFXTimer = 0f;
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayStarCup();
		}

		bool skipFx = progress > 1f || !allowFx;
		Image filledStar = FilledStar1;
		GameObject fxPrefab = null;
		if (startIndex == 2)
		{
			filledStar = FilledStar3;
			fxPrefab = !skipFx && StarFX3 != null ? StarFX3.gameObject : null;
		}
		else if (startIndex == 1)
		{
			filledStar = FilledStar2;
			fxPrefab = !skipFx && StarFX2 != null ? StarFX2.gameObject : null;
		}

		GameObject fx = null;
		Camera main = Camera.main;
		if (fxPrefab != null && main != null)
		{
			Vector3 screenPosition = main.WorldToScreenPoint(startFXpos);
			fx = LeanPool.Spawn(fxPrefab, screenPosition, Quaternion.identity, Stars);
			if (fx != null)
			{
				fx.transform.localPosition = StarFXLocalPosition;
				fx.SetActive(true);
			}
		}

		DOVirtual.DelayedCall(StarProgressDelay, () => UpdateStarProgress(startIndex, progress, filledStar, fx));
	}

	private void UpdateStarProgress(int startIndex, float progress, Image filledStar, GameObject starFX)
	{
		if (this == null || filledStar == null)
		{
			return;
		}
		filledStar.fillAmount = progress;
		if (progress >= 1f)
		{
			if (startIndex == 2)
			{
				if (fillStar3Anim != null)
				{
					fillStar3Anim.enabled = true;
				}
				if (AudioManager.instance != null)
				{
					AudioManager.instance.Play3StarAppears();
				}
			}
			else if (startIndex == 1 && fillStar2Anim != null)
			{
				fillStar2Anim.enabled = true;
			}
		}
		if (starFX != null)
		{
			LeanPool.Despawn(starFX);
		}
	}

	private void Update()
	{
		starFXTimer += Time.deltaTime;
		StepBallsCounter(Time.deltaTime);
	}

	private void StepBallsCounter(float deltaTime)
	{
		if (ballsCounterCurrentValue == ballsCounterTargetValue)
		{
			if (ballsCounterAnim != null)
			{
				ballsCounterAnim.enabled = false;
				if (BallsCounter != null)
				{
					BallsCounter.localScale = Vector3.one;
					if (ballLightImage != null)
					{
						ballLightImage.color = new Color(1f, 1f, 1f, 0f);
					}
				}
			}
			return;
		}

		float progress = Mathf.Clamp01(deltaTime * BallsCounterRate);
		float raw = ballsCounterCurrentValue + (ballsCounterTargetValue - ballsCounterCurrentValue) * progress;
		ballsCounterCurrentValue = ballsCounterTargetValue > ballsCounterCurrentValue ? Mathf.CeilToInt(raw) : Mathf.FloorToInt(raw);
		if (BallsCounterText != null)
		{
			BallsCounterText.text = ballsCounterCurrentValue.ToString();
		}
		if (ballsCounterAnim != null)
		{
			ballsCounterAnim.enabled = true;
		}
	}

	private static Vector2 AnchoredPosition(Component target)
	{
		RectTransform rect = target != null ? target.GetComponent<RectTransform>() : null;
		return rect != null ? rect.anchoredPosition : Vector2.zero;
	}

	private static void SetAnchoredPosition(Component target, Vector2 position)
	{
		RectTransform rect = target != null ? target.GetComponent<RectTransform>() : null;
		if (rect != null)
		{
			rect.anchoredPosition = position;
		}
	}

	private static void SetActive(Component target, bool active)
	{
		if (target != null)
		{
			target.gameObject.SetActive(active);
		}
	}

	private static void SetFill(Image image, float amount)
	{
		if (image != null)
		{
			image.fillAmount = amount;
		}
	}

	private static void SetScale(Component target, Vector3 scale)
	{
		if (target != null)
		{
			target.transform.localScale = scale;
		}
	}
}
