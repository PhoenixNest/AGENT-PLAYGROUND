using System;
using System.IO;
using System.Linq;
using OrbitalSalvage.Viewer;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OrbitalSalvage.Editor
{
    /// <summary>
    /// Xcode project export. Batch mode exits 0 or 1. The iOS Build Support module is required;
    /// without it the menu items are disabled and these entry points log and return 1.
    /// Simulator architecture is ARM64 (<see cref="AppleMobileArchitectureSimulator"/>).
    /// Universal also contains x86_64, which this export does not need for Apple-silicon simulators.
    /// <see cref="BuildPipeline.BuildPlayer"/> runs <see cref="IosXcodePostProcess"/>, which sets the
    /// GameAssembly target id to <see cref="GameAssemblyBundleId"/>. The app id stays <see cref="BundleId"/>.
    /// </summary>
    public static class IosExport
    {
        public const string SimulatorMenu = "Orbital Salvage/Build/iOS Xcode Project (Simulator)";
        public const string DeviceMenu = "Orbital Salvage/Build/iOS Xcode Project (Device)";
        public const string BundleId = "io.dev.maze";
        public const string GameAssemblyBundleId = "com.unity3d.gameassembly";
        public const string GameAssemblyTargetName = "GameAssembly";
        public const string MinimumIosVersion = "15.0";

        public const string MissingModuleMessage =
            "iOS Build Support is not installed for this Editor (BuildPipeline.IsBuildTargetSupported is false). " +
            "Install the iOS module for Unity 6000.4.6f1 from Unity Hub, then run the export again. " +
            "Signing and running the Xcode project need Xcode on a Mac. The simulator build cannot be tested on this Windows machine.";

        public static bool ModuleInstalled =>
            BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS);

        [MenuItem(SimulatorMenu, false, 201)]
        public static void ExportSimulator() => Run(true);

        [MenuItem(DeviceMenu, false, 202)]
        public static void ExportDevice() => Run(false);

        [MenuItem(SimulatorMenu, true)]
        public static bool ValidateSimulator() => ModuleInstalled;

        [MenuItem(DeviceMenu, true)]
        public static bool ValidateDevice() => ModuleInstalled;

        public static void LogApplicationIdentifier()
        {
            string id = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);
            Debug.Log("[IosExport] iOS applicationIdentifier=" + id);
            if (Application.isBatchMode) EditorApplication.Exit(id == BundleId ? 0 : 1);
        }

        static void Run(bool simulator)
        {
            int code;
            try
            {
                code = Export(simulator);
            }
            catch (Exception ex)
            {
                Debug.LogError("[IosExport] " + ex);
                code = 1;
            }

            if (Application.isBatchMode) EditorApplication.Exit(code);
        }

        static int Export(bool simulator)
        {
            string which = simulator ? "simulator" : "device";
            if (!ModuleInstalled)
            {
                Debug.LogError("[IosExport] " + which + " export failed: " + MissingModuleMessage);
                if (!Application.isBatchMode)
                    EditorUtility.DisplayDialog("iOS Xcode project", MissingModuleMessage, "OK");
                return 1;
            }

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[IosExport] no enabled scenes in EditorBuildSettings");
                return 1;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outDir = OutputPaths.Absolute(projectRoot, simulator ? "simulator" : null);
            if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
            Directory.CreateDirectory(outDir);

            string settingsPath = Path.Combine(projectRoot, "ProjectSettings", "ProjectSettings.asset");
            byte[] settingsBytes = File.Exists(settingsPath) ? File.ReadAllBytes(settingsPath) : null;
            var previousSdk = PlayerSettings.iOS.sdkVersion;
            var previousArch = PlayerSettings.iOS.simulatorSdkArchitecture;
            bool previousTiming = PlayerSettings.enableFrameTimingStats;
            try
            {
                PlayerSettings.iOS.sdkVersion = simulator ? iOSSdkVersion.SimulatorSDK : iOSSdkVersion.DeviceSDK;
                if (simulator)
                    PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64;
                PlayerSettings.enableFrameTimingStats = true;

                var options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outDir,
                    target = BuildTarget.iOS,
                    targetGroup = BuildTargetGroup.iOS,
                    options = BuildOptions.None
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                var summary = report.summary;
                string sdk = simulator ? "SimulatorSDK" : "DeviceSDK";
                string arch = simulator ? "ARM64" : "unchanged";
                Debug.Log("[IosExport] result=" + summary.result + " sdk=" + sdk + " arch=" + arch + " frameTiming=1 output=" + summary.outputPath);
                if (summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError("[IosExport] " + which + " export failed: " + summary.result);
                    return 1;
                }

                if (!Application.isBatchMode)
                {
                    bool reveal = EditorUtility.DisplayDialog(
                        "iOS Xcode project",
                        "Export finished.\n\n" + outDir + "\n\nSigning and running need Xcode on a Mac.",
                        "Reveal in Explorer",
                        "OK");
                    if (reveal) EditorUtility.RevealInFinder(outDir);
                }
                return 0;
            }
            finally
            {
                PlayerSettings.iOS.sdkVersion = previousSdk;
                PlayerSettings.iOS.simulatorSdkArchitecture = previousArch;
                PlayerSettings.enableFrameTimingStats = previousTiming;
                AssetDatabase.SaveAssets();
                if (settingsBytes != null && File.Exists(settingsPath))
                {
                    var after = File.ReadAllBytes(settingsPath);
                    if (!after.AsSpan().SequenceEqual(settingsBytes))
                    {
                        Debug.LogWarning("[IosExport] ProjectSettings.asset differs after restore; writing the original bytes back");
                        File.WriteAllBytes(settingsPath, settingsBytes);
                    }
                }
            }
        }
    }
}
