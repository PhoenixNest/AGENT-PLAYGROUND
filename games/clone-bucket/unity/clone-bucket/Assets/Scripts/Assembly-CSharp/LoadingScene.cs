using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
	private const float _timeout = 10f;

	private const string MainSceneName = "Main";

	private const int WarmupFrames = 3;

	private const float FakeProgressStart = 0.4f;

	private const float PostLoadDelay = 0.3f;

	private static Action m_OnLoadingComplete;

	[SerializeField]
	private Image _image;

	private float _loadingTimer;

	private bool _isLoading;

	private AsyncOperation _pendingLoad;

	public static event Action OnLoadingComplete
	{
		add
		{
			m_OnLoadingComplete = (Action)Delegate.Combine(m_OnLoadingComplete, value);
		}
		remove
		{
			m_OnLoadingComplete = (Action)Delegate.Remove(m_OnLoadingComplete, value);
		}
	}

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		_loadingTimer = 0f;
		_isLoading = false;
		LoadGame(true);
	}

	private void Update()
	{
		if (_isLoading)
		{
			return;
		}
		_loadingTimer += Time.unscaledDeltaTime;
		if (_loadingTimer >= _timeout)
		{
			HandleLoadGameOnTimeout();
		}
	}

	private void HandleLoadGameOnTimeout()
	{
		Debug.LogWarning("[MM3:Boot] Main did not start loading within " + _timeout + " s; starting it now.");
		LoadGame(true);
	}

	private void LoadGame(bool _)
	{
		if (_isLoading)
		{
			return;
		}
		_isLoading = true;
		StartCoroutine(LoadScene());
	}

	private IEnumerator LoadScene()
	{
		for (int i = 0; i < WarmupFrames; i++)
		{
			yield return null;
		}
		_pendingLoad = SceneManager.LoadSceneAsync(MainSceneName);
		_pendingLoad.allowSceneActivation = false;

		float process = FakeProgressStart;
		SetFill(process);
		while (process < 1f)
		{
			yield return null;
			process += Time.deltaTime;
			SetFill(process);
		}

		yield return new WaitForSeconds(PostLoadDelay);
		_pendingLoad.allowSceneActivation = true;
		while (!_pendingLoad.isDone)
		{
			yield return null;
		}
		m_OnLoadingComplete?.Invoke();
		Destroy(gameObject, 0.5f);
	}

	private void SetFill(float value)
	{
		if (_image != null)
		{
			_image.fillAmount = Mathf.Clamp01(value);
		}
	}
}
