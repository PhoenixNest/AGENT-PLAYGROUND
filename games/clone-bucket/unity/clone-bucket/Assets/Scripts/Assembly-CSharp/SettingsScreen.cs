using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
	private const float PixelsPerUnitFor3Buttons = 1.25f;

	private const float PixelsPerUnitFor2Buttons = 1.5f;

	private const float PixelsPerUnitFor1Button = 1.75f;

	private const float PixelsPerUnitDefault = 1f;

	public GameObject MuteBtn;

	public GameObject UnmuteBtn;

	public GameObject VibrationOffBtn;

	public GameObject VibrationOnBtn;

	public GameObject PS;

	public GameObject PP;

	public GameObject TOU;

	public TextMeshProUGUI versionText;

	public Image background;

	public int buttonCount;

	private void Awake()
	{
		buttonCount--;
		if (versionText != null)
		{
			versionText.text = Application.version;
		}

		ApplyPrivacyButtons();
		SetSliceOfBG();
	}

	private void ApplyPrivacyButtons()
	{
		bool showPrivacySettings = TTPManager.Instance != null && TTPManager.Instance.ShouldShowPrivacySettings();
		SetVisible(PS, showPrivacySettings);
		SetVisible(PP, !showPrivacySettings);
		SetVisible(TOU, !showPrivacySettings);
		buttonCount += showPrivacySettings ? 1 : 2;
	}

	[ContextMenu("SetSliceOfBG")]
	private void SetSliceOfBG()
	{
		if (background == null)
		{
			return;
		}

		switch (buttonCount)
		{
			case 3:
				background.pixelsPerUnitMultiplier = PixelsPerUnitFor3Buttons;
				break;
			case 2:
				background.pixelsPerUnitMultiplier = PixelsPerUnitFor2Buttons;
				break;
			case 1:
				background.pixelsPerUnitMultiplier = PixelsPerUnitFor1Button;
				break;
			default:
				background.pixelsPerUnitMultiplier = PixelsPerUnitDefault;
				break;
		}
	}

	private void OnEnable()
	{
		RefreshToggles();
	}

	public void SoundsSwitch()
	{
		if (GameManager.instance == null || AudioManager.instance == null)
		{
			return;
		}

		GameManager.instance.SetSoundsEnable(!AudioManager.instance.isSoundsPlay);
		RefreshToggles();
		AudioManager.instance.PlayButtonClick();
	}

	private void SetPrivacySettings()
	{
		if (TTPManager.Instance != null)
		{
			TTPManager.Instance.ShowPrivacySettings();
		}
	}

	public void VibrationSwitch()
	{
		if (GameManager.instance == null || HapticManager.instance == null)
		{
			return;
		}

		GameManager.instance.SetHapticEnable(!HapticManager.instance.isHapticPlay);
		RefreshToggles();
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayButtonClick();
		}
	}

	public void CloseBtnClick()
	{
		if (AudioManager.instance != null)
		{
			AudioManager.instance.PlayButtonClick();
		}
		gameObject.SetActive(false);
	}

	public void PSClick()
	{
		SetPrivacySettings();
	}

	public void PPClick()
	{
		if (TTPManager.Instance != null)
		{
			TTPManager.Instance.ShowPrivacyPolicy();
		}
	}

	public void TOUClick()
	{
		if (TTPManager.Instance != null)
		{
			TTPManager.Instance.ShowTermsOfUse();
		}
	}

	private void RefreshToggles()
	{
		bool sound = AudioManager.instance != null && AudioManager.instance.isSoundsPlay;
		bool haptic = HapticManager.instance != null && HapticManager.instance.isHapticPlay;
		SetVisible(MuteBtn, sound);
		SetVisible(UnmuteBtn, !sound);
		SetVisible(VibrationOnBtn, haptic);
		SetVisible(VibrationOffBtn, !haptic);
	}

	private static void SetVisible(GameObject target, bool visible)
	{
		if (target != null && target.activeSelf != visible)
		{
			target.SetActive(visible);
		}
	}
}
