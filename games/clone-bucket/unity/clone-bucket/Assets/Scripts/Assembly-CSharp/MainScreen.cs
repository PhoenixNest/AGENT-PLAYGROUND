using System.Collections.Generic;
using CustomEventSystem;
using DG.Tweening;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScreen : MonoBehaviour
{
	public delegate void CloseCallback();

	public GameObject GameTitle;

	public GameObject SettingBtn;

	public GameObject SoundOffBtn;

	public GameObject SoundOnBtn;

	public GameObject HapticOffBtn;

	public GameObject HapticOnBtn;

	public GameObject ShopBtn;

	public TextMeshProUGUI GemsCounterText;

	public Image GemsCounterImg;

	public RectTransform TutorialHand;

	public TextMeshProUGUI levelText;

	public Animator GemAnimator;

	private readonly List<ElementSnapshot> closeSnapshots = new List<ElementSnapshot>();

	private bool shopBtnLock;

	private bool settingsBtnLock;

	private Sequence mainScreenSequence;

	private Sequence settingsSequence;

	private bool isGeming;

	private const float CloseDuration = 0.5f;

	private const float SettingZoomDuration = 0.3f;

	private const float TitleSlideDistance = 50f;

	private const float ShopSlideDistance = 158f;

	private const float IntroFadeDuration = 0.3f;

	private const float IntroZoomDuration = 0.3f;

	private const float IntroShopOffset = 250f;

	private const float IntroShopDuration = 0.5f;

	private const float GemCollectDelay = 1.1f;

	private const float GemCountDuration = 0.9f;

	private GameObject GemFx => GemAnimator != null ? GemAnimator.gameObject : null;

	private bool _settingsOpen;

	private bool _shopInitialised;

	private void Awake()
	{
		_settingsOpen = false;
		CaptureCloseSnapshots();
		RefreshToggles();
	}

	private void OnEnable()
	{
		transform.localScale = Vector3.one;
		RestoreCloseSnapshots();
		mainScreenSequence?.Kill();
		mainScreenSequence = StartIntro();
		_settingsOpen = false;
		HandleGameMusic(false);
		UpdateCounter();
		RefreshToggles();
		RefreshLevelText();
	}

	private void OnDisable()
	{
		mainScreenSequence?.Kill();
		settingsSequence?.Kill();
	}

	private void HandleGameMusic(bool pauseMusic)
	{
	}

	public void Close(CloseCallback callback = null)
	{
		if (!gameObject.activeInHierarchy)
		{
			callback?.Invoke();
			return;
		}
		mainScreenSequence?.Kill();
		CloseSettingsButtons();
		Sequence sequence = DOTween.Sequence();
		if (GameTitle != null)
		{
			sequence.Insert(0f, UIEffector.FadeOutElement(GameTitle, CloseDuration, Ease.OutQuad));
			sequence.Insert(0f, UIEffector.SlideOutUpElement(GameTitle, TitleSlideDistance, CloseDuration, Ease.InBack));
			sequence.Insert(0f, UIEffector.ZoomOutElement(GameTitle, CloseDuration, Ease.InBack));
		}
		if (GemsCounterText != null)
		{
			sequence.Insert(0f, UIEffector.FadeOutText(GemsCounterText, CloseDuration, Ease.OutQuad));
		}
		if (GemsCounterImg != null)
		{
			sequence.Insert(0f, UIEffector.FadeOutImage(GemsCounterImg, CloseDuration, Ease.OutQuad));
		}
		if (SettingBtn != null)
		{
			sequence.Insert(0f, UIEffector.ZoomOutElement(SettingBtn, SettingZoomDuration, Ease.InBack));
		}
		if (ShopBtn != null)
		{
			sequence.Insert(0f, UIEffector.SlideOutLeftElement(ShopBtn, ShopSlideDistance, CloseDuration, Ease.InBack));
		}
		sequence.OnComplete(() =>
		{
			mainScreenSequence = null;
			callback?.Invoke();
		});
		mainScreenSequence = sequence;
	}

	private void CaptureCloseSnapshots()
	{
		closeSnapshots.Clear();
		AddCloseSnapshot(GameTitle);
		AddCloseSnapshot(GemsCounterText != null ? GemsCounterText.gameObject : null);
		AddCloseSnapshot(GemsCounterImg != null ? GemsCounterImg.gameObject : null);
		AddCloseSnapshot(SettingBtn);
		AddCloseSnapshot(ShopBtn);
	}

	private void AddCloseSnapshot(GameObject target)
	{
		if (target != null)
		{
			closeSnapshots.Add(new ElementSnapshot(target));
		}
	}

	private void RestoreCloseSnapshots()
	{
		foreach (ElementSnapshot snapshot in closeSnapshots)
		{
			snapshot.Restore();
		}
	}

	private Sequence StartIntro()
	{
		Sequence intro = DOTween.Sequence();
		if (SettingBtn != null)
		{
			intro.Insert(0f, UIEffector.ZoomInElement(SettingBtn, IntroZoomDuration));
		}
		if (GemsCounterText != null)
		{
			intro.Insert(0f, UIEffector.FadeInText(GemsCounterText, IntroFadeDuration, Ease.OutQuad));
		}
		if (GemsCounterImg != null)
		{
			intro.Insert(0f, UIEffector.FadeInImage(GemsCounterImg, IntroFadeDuration, Ease.OutQuad));
		}
		if (ShopBtn != null)
		{
			intro.Insert(0f, UIEffector.SlideInRightElement(ShopBtn, IntroShopOffset, IntroShopDuration));
		}
		intro.OnComplete(() =>
		{
			if (mainScreenSequence == intro)
			{
				mainScreenSequence = null;
			}
		});
		return intro;
	}

	public void OnClickSetting()
	{
		if (GameManager.instance != null && GameManager.instance.settingsScreen != null)
		{
			GameManager.instance.settingsScreen.gameObject.SetActive(true);
			return;
		}
		if (_settingsOpen)
		{
			CloseSettingsButtons();
		}
		else
		{
			OpenSettingsButtons();
		}
	}

	private void OpenSettingsButtons()
	{
		_settingsOpen = true;
		RefreshToggles();
		settingsSequence?.Kill();
		settingsSequence = DOTween.Sequence();
		PopIn(SoundOnBtn);
		PopIn(SoundOffBtn);
		PopIn(HapticOnBtn);
		PopIn(HapticOffBtn);
	}

	private void CloseSettingsButtons()
	{
		_settingsOpen = false;
		settingsSequence?.Kill();
		RefreshToggles();
	}

	public void SoundsToggle()
	{
		bool current = AudioManager.instance != null && AudioManager.instance.isSoundsPlay;
		if (GameManager.instance != null)
		{
			GameManager.instance.SetSoundsEnable(!current);
		}
		RefreshToggles();
	}

	public void HapticToggle()
	{
		bool current = HapticManager.instance != null && HapticManager.instance.isHapticPlay;
		if (GameManager.instance != null)
		{
			GameManager.instance.SetHapticEnable(!current);
		}
		RefreshToggles();
	}

	public void ShopOnClick()
	{
		if (GameManager.instance == null || GameManager.instance.shopScreen == null)
		{
			return;
		}
		ShopScreen shop = GameManager.instance.shopScreen;
		if (!_shopInitialised)
		{
			shop.InitData();
			_shopInitialised = true;
		}
		shop.gameObject.SetActive(true);
	}

	public void UpdateCounter()
	{
		if (GameManager.instance != null)
		{
			SetGems(GameManager.instance.GemCounter);
		}
	}

	public void StartPlayClick()
	{
		if (GameManager.instance != null)
		{
			GameManager.instance.StartPlay();
		}
	}

	public void GetGem(int miniLevelCount)
	{
		isGeming = true;
		EventManager.TriggerEvent(EventConstants.LevelControllerEnabled, false);
		if (GemFx != null)
		{
			GemFx.SetActive(true);
		}
		int start = GameManager.instance != null ? GameManager.instance.GemCounter : 0;
		int amount = GemReward(miniLevelCount);
		int end = start + amount;
		if (GameManager.instance != null)
		{
			GameManager.instance.GemCollect(amount);
		}
		DOTween.Sequence()
			.AppendInterval(GemCollectDelay)
			.OnComplete(() =>
			{
				DOTween.To(() => start, SetGems, end, GemCountDuration)
					.OnComplete(() =>
					{
						if (GemFx != null)
						{
							GemFx.SetActive(false);
						}
						SetGems(end);
						EventManager.TriggerEvent(EventConstants.LevelControllerEnabled, true);
						isGeming = false;
					});
			});
	}

	internal static int GemReward(int miniLevelCount)
	{
		return 50 * Mathf.Max(0, miniLevelCount);
	}

	public void SetGems(int value)
	{
		if (GemsCounterText != null)
		{
			GemsCounterText.text = value.ToString();
		}
	}

	private void RefreshToggles()
	{
		bool sound = AudioManager.instance == null || AudioManager.instance.isSoundsPlay;
		bool haptic = HapticManager.instance == null || HapticManager.instance.isHapticPlay;
		SetActiveIf(SoundOnBtn, _settingsOpen && sound);
		SetActiveIf(SoundOffBtn, _settingsOpen && !sound);
		SetActiveIf(HapticOnBtn, _settingsOpen && haptic);
		SetActiveIf(HapticOffBtn, _settingsOpen && !haptic);
	}

	private void RefreshLevelText()
	{
		if (levelText != null && GameManager.instance != null)
		{
			levelText.text = GameManager.instance.CurrentLevel.ToString();
		}
	}

	private void PopIn(GameObject button)
	{
		if (button == null || !button.activeInHierarchy || settingsSequence == null)
		{
			return;
		}
		button.transform.localScale = Vector3.one * 0.6f;
		settingsSequence.Join(button.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
	}

	private static void SetActiveIf(GameObject target, bool active)
	{
		if (target != null && target.activeSelf != active)
		{
			target.SetActive(active);
		}
	}

	private sealed class ElementSnapshot
	{
		private readonly GameObject target;

		private readonly Vector3 localPosition;

		private readonly Vector3 localScale;

		private readonly Graphic[] graphics;

		private readonly Color[] colors;

		public ElementSnapshot(GameObject target)
		{
			this.target = target;
			localPosition = target.transform.localPosition;
			localScale = target.transform.localScale;
			graphics = target.GetComponentsInChildren<Graphic>(true);
			colors = new Color[graphics.Length];
			for (int i = 0; i < graphics.Length; i++)
			{
				colors[i] = graphics[i].color;
			}
		}

		public void Restore()
		{
			if (target == null)
			{
				return;
			}
			target.transform.localPosition = localPosition;
			target.transform.localScale = localScale;
			for (int i = 0; i < graphics.Length; i++)
			{
				if (graphics[i] != null)
				{
					graphics[i].color = colors[i];
				}
			}
		}
	}
}
