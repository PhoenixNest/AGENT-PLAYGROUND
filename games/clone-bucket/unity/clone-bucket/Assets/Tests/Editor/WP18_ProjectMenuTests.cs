using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class WP18_ProjectMenuTests
{
	[Test]
	public void OutputFolderIsTheOutputFolderOfGamesCloneBucket()
	{
		string expected = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", ".."));

		Assert.AreEqual(Path.Combine(expected, "output"), CloneBucketMenu.OutputFolder);
		Assert.IsTrue(Directory.Exists(Path.Combine(expected, "docs")));
		Assert.AreEqual("clone-bucket", new DirectoryInfo(expected).Name);
	}

	[Test]
	public void XcodeProjectsAreNamedByTarget()
	{
		Assert.AreEqual(Path.Combine(CloneBucketMenu.OutputFolder, "clone-bucket-ios-emulator"), CloneBucketMenu.XcodeProjectPath(CloneBucketMenu.SimulatorProjectName));
		Assert.AreEqual(Path.Combine(CloneBucketMenu.OutputFolder, "clone-bucket"), CloneBucketMenu.XcodeProjectPath(CloneBucketMenu.DeviceProjectName));
	}

	[Test]
	public void MenuHasTheThreeItems()
	{
		var menuItems = typeof(CloneBucketMenu)
			.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
			.SelectMany(method => method.GetCustomAttributes<MenuItem>())
			.Where(attribute => !attribute.validate)
			.Select(attribute => attribute.menuItem)
			.ToList();

		CollectionAssert.IsSubsetOf(new[]
		{
			"Tools/Clone Bucket/Export Xcode Project for iOS Simulator",
			"Tools/Clone Bucket/Export Xcode Project for iOS Device",
			"Tools/Clone Bucket/Clear Cached Data"
		}, menuItems);
	}

	[Test]
	public void ExportUsesTheEnabledBuildScenes()
	{
		CollectionAssert.AreEqual(new[] { "Assets/Scenes/Loading.unity", "Assets/Scenes/Main.unity" }, CloneBucketMenu.EnabledScenePaths());
	}

	[Test]
	public void IosBundleIdentifierIsSet()
	{
		Assert.AreEqual("io.dev.clone-bucket", PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS));
	}

	[Test]
	public void DeleteContentsEmptiesTheFolderButKeepsIt()
	{
		string root = Path.Combine(Path.GetTempPath(), "clone-bucket-menu-test-" + System.Guid.NewGuid().ToString("N"));
		try
		{
			Directory.CreateDirectory(Path.Combine(root, "sub"));
			File.WriteAllText(Path.Combine(root, "a.txt"), "a");
			File.WriteAllText(Path.Combine(root, "sub", "b.txt"), "b");

			CloneBucketMenu.DeleteContents(root);

			Assert.IsTrue(Directory.Exists(root));
			Assert.AreEqual(0, Directory.GetFileSystemEntries(root).Length);
		}
		finally
		{
			if (Directory.Exists(root))
			{
				Directory.Delete(root, true);
			}
		}
	}

	[Test]
	public void DeleteContentsIgnoresAMissingFolder()
	{
		Assert.DoesNotThrow(() => CloneBucketMenu.DeleteContents(Path.Combine(Path.GetTempPath(), "clone-bucket-missing-" + System.Guid.NewGuid().ToString("N"))));
		Assert.DoesNotThrow(() => CloneBucketMenu.DeleteContents(null));
		Assert.DoesNotThrow(() => CloneBucketMenu.DeleteContents(""));
	}

	[Test]
	public void SimulatorExportBuildsForArm64()
	{
		iOSSdkVersion previousSdk = PlayerSettings.iOS.sdkVersion;
		AppleMobileArchitectureSimulator previousArchitecture = PlayerSettings.iOS.simulatorSdkArchitecture;
		try
		{
			PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.X86_64;
			CloneBucketMenu.SavedIosExportSettings saved = CloneBucketMenu.ApplyIosExportSettings(iOSSdkVersion.SimulatorSDK);
			try
			{
				Assert.AreEqual(AppleMobileArchitectureSimulator.ARM64, CloneBucketMenu.SimulatorExportArchitecture);
				Assert.AreEqual(iOSSdkVersion.SimulatorSDK, PlayerSettings.iOS.sdkVersion);
				Assert.AreEqual(CloneBucketMenu.SimulatorExportArchitecture, PlayerSettings.iOS.simulatorSdkArchitecture);
			}
			finally
			{
				CloneBucketMenu.RestoreIosExportSettings(saved);
			}
		}
		finally
		{
			PlayerSettings.iOS.sdkVersion = previousSdk;
			PlayerSettings.iOS.simulatorSdkArchitecture = previousArchitecture;
		}
	}

	[Test]
	public void SimulatorExportRestoresTheSimulatorArchitecture()
	{
		iOSSdkVersion previousSdk = PlayerSettings.iOS.sdkVersion;
		AppleMobileArchitectureSimulator previousArchitecture = PlayerSettings.iOS.simulatorSdkArchitecture;
		try
		{
			PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
			PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.Universal;
			CloneBucketMenu.SavedIosExportSettings saved = CloneBucketMenu.ApplyIosExportSettings(iOSSdkVersion.SimulatorSDK);
			Assert.AreEqual(AppleMobileArchitectureSimulator.ARM64, PlayerSettings.iOS.simulatorSdkArchitecture);
			CloneBucketMenu.RestoreIosExportSettings(saved);
			Assert.AreEqual(AppleMobileArchitectureSimulator.Universal, PlayerSettings.iOS.simulatorSdkArchitecture);
			Assert.AreEqual(iOSSdkVersion.DeviceSDK, PlayerSettings.iOS.sdkVersion);
		}
		finally
		{
			PlayerSettings.iOS.sdkVersion = previousSdk;
			PlayerSettings.iOS.simulatorSdkArchitecture = previousArchitecture;
		}
	}

	[Test]
	public void DeviceExportLeavesTheSimulatorArchitectureAlone()
	{
		iOSSdkVersion previousSdk = PlayerSettings.iOS.sdkVersion;
		AppleMobileArchitectureSimulator previousArchitecture = PlayerSettings.iOS.simulatorSdkArchitecture;
		try
		{
			PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.Universal;
			CloneBucketMenu.SavedIosExportSettings saved = CloneBucketMenu.ApplyIosExportSettings(iOSSdkVersion.DeviceSDK);
			Assert.AreEqual(AppleMobileArchitectureSimulator.Universal, PlayerSettings.iOS.simulatorSdkArchitecture);
			Assert.AreEqual(iOSSdkVersion.DeviceSDK, PlayerSettings.iOS.sdkVersion);
			PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.X86_64;
			CloneBucketMenu.RestoreIosExportSettings(saved);
			Assert.AreEqual(AppleMobileArchitectureSimulator.X86_64, PlayerSettings.iOS.simulatorSdkArchitecture);
			Assert.AreEqual(previousSdk, PlayerSettings.iOS.sdkVersion);
		}
		finally
		{
			PlayerSettings.iOS.sdkVersion = previousSdk;
			PlayerSettings.iOS.simulatorSdkArchitecture = previousArchitecture;
		}
	}
}
