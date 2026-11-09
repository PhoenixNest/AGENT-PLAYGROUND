using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

internal static class CloneBucketMenu
{
	internal const string MenuRoot = "Tools/Clone Bucket/";
	internal const string SimulatorMenuPath = MenuRoot + "Export Xcode Project for iOS Simulator";
	internal const string DeviceMenuPath = MenuRoot + "Export Xcode Project for iOS Device";
	internal const string ClearCacheMenuPath = MenuRoot + "Clear Cached Data";
	internal const string SimulatorProjectName = "clone-bucket-ios-emulator";
	internal const string DeviceProjectName = "clone-bucket";

	internal static string OutputFolder
	{
		get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "output")); }
	}

	internal static string XcodeProjectPath(string projectName)
	{
		return Path.Combine(OutputFolder, projectName);
	}

	internal static string[] EnabledScenePaths()
	{
		return EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
	}

	[MenuItem(SimulatorMenuPath, false, 100)]
	private static void ExportForSimulator()
	{
		ExportXcodeProject(iOSSdkVersion.SimulatorSDK, SimulatorProjectName);
	}

	[MenuItem(DeviceMenuPath, false, 101)]
	private static void ExportForDevice()
	{
		ExportXcodeProject(iOSSdkVersion.DeviceSDK, DeviceProjectName);
	}

	[MenuItem(SimulatorMenuPath, true)]
	[MenuItem(DeviceMenuPath, true)]
	private static bool ExportValidate()
	{
		return !EditorApplication.isPlayingOrWillChangePlaymode && !BuildPipeline.isBuildingPlayer;
	}

	[MenuItem(ClearCacheMenuPath, false, 200)]
	private static void ClearCachedDataWithConfirmation()
	{
		string message = "This deletes the saved data and caches of clone-bucket in this editor:\n\n"
			+ "- all PlayerPrefs (level, gems, scores, skins, sound and haptics settings)\n"
			+ "- the contents of " + Application.persistentDataPath + "\n"
			+ "- the contents of " + Application.temporaryCachePath + "\n"
			+ "- the asset bundle cache\n\n"
			+ "This cannot be undone.";
		if (!EditorUtility.DisplayDialog("Clear Cached Data", message, "Clear", "Cancel"))
		{
			return;
		}

		ClearCachedData(Application.persistentDataPath, Application.temporaryCachePath);
		Debug.Log("Clone Bucket: cleared PlayerPrefs, the asset bundle cache, " + Application.persistentDataPath + " and " + Application.temporaryCachePath + ".");
	}

	[MenuItem(ClearCacheMenuPath, true)]
	private static bool ClearCachedDataValidate()
	{
		return !EditorApplication.isPlayingOrWillChangePlaymode;
	}

	internal static void ClearCachedData(string persistentDataPath, string temporaryCachePath)
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		Caching.ClearCache();
		DeleteContents(persistentDataPath);
		DeleteContents(temporaryCachePath);
	}

	internal static void DeleteContents(string folder)
	{
		if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
		{
			return;
		}

		foreach (string file in Directory.GetFiles(folder))
		{
			File.Delete(file);
		}

		foreach (string directory in Directory.GetDirectories(folder))
		{
			Directory.Delete(directory, true);
		}
	}

	private static void ExportXcodeProject(iOSSdkVersion sdkVersion, string projectName)
	{
		if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
		{
			ShowMessage("iOS Build Support is missing", "Install the iOS Build Support module for Unity " + Application.unityVersion + " in Unity Hub, then try again.");
			return;
		}

		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			return;
		}

		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS && !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS))
		{
			ShowMessage("Export failed", "Unity could not switch the active platform to iOS.");
			return;
		}

		string path = XcodeProjectPath(projectName);
		Directory.CreateDirectory(OutputFolder);
		iOSSdkVersion previousSdkVersion = PlayerSettings.iOS.sdkVersion;
		PlayerSettings.iOS.sdkVersion = sdkVersion;
		BuildReport report;
		try
		{
			report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
			{
				scenes = EnabledScenePaths(),
				locationPathName = path,
				target = BuildTarget.iOS,
				targetGroup = BuildTargetGroup.iOS,
				options = BuildOptions.None
			});
		}
		finally
		{
			PlayerSettings.iOS.sdkVersion = previousSdkVersion;
		}

		if (report.summary.result == BuildResult.Succeeded)
		{
			Debug.Log("Clone Bucket: exported the Xcode project to " + path + ".");
			if (!Application.isBatchMode)
			{
				EditorUtility.RevealInFinder(path);
			}

			return;
		}

		ShowMessage("Export failed", "The Xcode export ended with " + report.summary.result + " and " + report.summary.totalErrors + " errors. See the Console for details.");
	}

	private static void ShowMessage(string title, string message)
	{
		if (Application.isBatchMode)
		{
			Debug.LogError(title + ": " + message);
			return;
		}

		EditorUtility.DisplayDialog(title, message, "OK");
	}
}
