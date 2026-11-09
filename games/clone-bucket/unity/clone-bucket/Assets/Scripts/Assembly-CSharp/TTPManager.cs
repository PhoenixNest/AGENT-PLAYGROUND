using System;
using Tabtale.TTPlugins;
using UnityEngine;

public class TTPManager : MonoBehaviour
{
	public static TTPManager Instance;

	public Action<bool> OnGPGSLogin;

	public Action OnGPGSLogout;

	private string PrivacyPolicyLink;

	private string TermsOfUseLink;

	private bool shoulHandlePauseGameMusic;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public bool IsConnectedToTheInternet()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}

	public bool OnBackPressed()
	{
		return false;
	}

	public void ShowRateUsPopup()
	{
#if CLIK_SDK
		TTPRateUs.Popup();
#endif
	}

	public void ClickRateUsButton()
	{
#if CLIK_SDK
		TTPRateUs.Button();
#endif
	}

	public void ShowPrivacySettings(Action PrivacySettingsClosedAction = null)
	{
#if CLIK_SDK
		TTPPrivacySettings.ShowPrivacySettings(PrivacySettingsClosedAction ?? (() => { }));
#endif
	}

	public bool ShouldShowPrivacySettings()
	{
#if CLIK_SDK
		return TTPPrivacySettings.ShouldShowPrivacySettings();
#else
		return false;
#endif
	}

	public void ShowPrivacyPolicy()
	{
		OpenLink(PrivacyPolicyLink);
	}

	public void ShowTermsOfUse()
	{
		OpenLink(TermsOfUseLink);
	}

	public void ForgetUser()
	{
#if CLIK_SDK
		TTPPrivacySettings.ForgetUser();
#endif
	}

	public bool ShouldShowAgeGate()
	{
#if CLIK_SDK
		return TTPPrivacySettings.ShouldShowAgeGate();
#else
		return false;
#endif
	}

	public void SetAge(int age)
	{
#if CLIK_SDK
		TTPPrivacySettings.SetAge(age);
#endif
	}

	public void AddBreadCrumb(string log)
	{
	}

	public void LoginToGPGS()
	{
		OnGPGSLogin?.Invoke(false);
	}

	public bool IsLoggedinToGPGS()
	{
		return false;
	}

	public void LogoutFromGPGS()
	{
		OnGPGSLogout?.Invoke();
	}

	public void ReportProgress(string achievement, double progress = 100.0)
	{
	}

	public void ShowAchievements()
	{
	}

	public void SetPlayerScore(string leaderboardId, long score)
	{
	}

	public void ShowLeaderboard(string leaderboardId)
	{
	}

	private void TTPCore_PauseGameMusicEvent(bool pauseMusic)
	{
	}

	private void TTPCore_OnNewTTPSessionEvent()
	{
	}

	private void OnApplicationFocus(bool hasFocus)
	{
	}

	private void OnApplicationQuit()
	{
	}

	private void OnApplicationPause(bool pause)
	{
	}

	private void OnApplicationLowMemory()
	{
	}

	private void HandleException(string exception, string stackTrace, LogType type)
	{
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void OnDisable()
	{
	}

	private static void OpenLink(string link)
	{
		if (!string.IsNullOrEmpty(link))
		{
			Application.OpenURL(link);
		}
	}
}
