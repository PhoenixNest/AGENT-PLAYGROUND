using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitForRemoteConfigReady : MonoBehaviour
{
	private bool isRemoteReadyEventTriggered;

	public string sceneName;

	private void Awake()
	{
		RemoteConfigHelper.OnRemoteReady += LoadMainGameScene;
	}

	private void OnDestroy()
	{
		RemoteConfigHelper.OnRemoteReady -= LoadMainGameScene;
	}

	private void LoadMainGameScene(bool isRemoteConfigReady)
	{
		if (isRemoteReadyEventTriggered || !isRemoteConfigReady)
		{
			return;
		}
		isRemoteReadyEventTriggered = true;
		if (string.IsNullOrEmpty(sceneName))
		{
			Debug.LogError("[MM3:Boot] WaitForRemoteConfigReady has no scene name");
			return;
		}
		SceneManager.LoadScene(sceneName);
	}
}
