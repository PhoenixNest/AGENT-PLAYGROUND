using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
internal static class PlayFromLoadingScene
{
	private const string ScenePath = "Assets/Scenes/Loading.unity";
	private const string PrefKey = "CloneBucket.PlayFromLoadingScene";
	private const string MenuPath = "Tools/Clone Bucket/Play From Loading Scene";
	private const string TestSceneNamePrefix = "InitTestScene";

	static PlayFromLoadingScene()
	{
		EditorApplication.delayCall += Apply;
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.ExitingEditMode && IsTestRunScene())
		{
			EditorSceneManager.playModeStartScene = null;
		}
		else if (state == PlayModeStateChange.EnteredEditMode)
		{
			Apply();
		}
	}

	private static bool IsTestRunScene()
	{
		return EditorSceneManager.GetActiveScene().name.StartsWith(TestSceneNamePrefix, System.StringComparison.Ordinal);
	}

	[MenuItem(MenuPath)]
	private static void Toggle()
	{
		EditorPrefs.SetBool(PrefKey, !IsEnabled());
		Apply();
	}

	[MenuItem(MenuPath, true)]
	private static bool ToggleValidate()
	{
		Menu.SetChecked(MenuPath, IsEnabled());
		return true;
	}

	private static bool IsEnabled()
	{
		return EditorPrefs.GetBool(PrefKey, true);
	}

	private static void Apply()
	{
		if (!IsEnabled())
		{
			EditorSceneManager.playModeStartScene = null;
			return;
		}

		SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
		if (scene == null)
		{
			Debug.LogWarning("PlayFromLoadingScene: could not find " + ScenePath + ".");
		}

		EditorSceneManager.playModeStartScene = scene;
	}
}
