using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OrbitalSalvage.Editor
{
    public static class ParityBuild
    {
        public const string Define = "ORBITAL_PARITY";
        public const string OutputRelative = "Builds/Parity/OrbitalSalvageParity.exe";

        public static void BuildPlayer()
        {
            int code;
            try
            {
                code = Build();
            }
            catch (Exception ex)
            {
                Debug.LogError("[ParityBuild] " + ex);
                code = 1;
            }

            if (Application.isBatchMode) EditorApplication.Exit(code);
        }

        static int Build()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[ParityBuild] no enabled scenes in EditorBuildSettings");
                return 1;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string exePath = Path.Combine(projectRoot, OutputRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(exePath));

            string settingsPath = Path.Combine(projectRoot, "ProjectSettings", "ProjectSettings.asset");
            byte[] settingsBytes = File.Exists(settingsPath) ? File.ReadAllBytes(settingsPath) : null;
            var saved = Snapshot();
            try
            {
                ApplyParitySettings();
                var options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = exePath,
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.None,
                    extraScriptingDefines = new[] { Define }
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                var summary = report.summary;
                Debug.Log($"[ParityBuild] result={summary.result} errors={summary.totalErrors} warnings={summary.totalWarnings} size={summary.totalSize} time={summary.totalTime} output={summary.outputPath}");
                return summary.result == BuildResult.Succeeded ? 0 : 1;
            }
            finally
            {
                Restore(saved);
                AssetDatabase.SaveAssets();
                if (settingsBytes != null && File.Exists(settingsPath))
                {
                    var after = File.ReadAllBytes(settingsPath);
                    if (!after.AsSpan().SequenceEqual(settingsBytes))
                    {
                        Debug.LogWarning("[ParityBuild] ProjectSettings.asset differs after restore; writing the original bytes back");
                        File.WriteAllBytes(settingsPath, settingsBytes);
                    }
                }
            }
        }

        sealed class Settings
        {
            public FullScreenMode FullScreenMode;
            public int Width;
            public int Height;
            public bool RunInBackground;
            public bool ResizableWindow;
            public bool DefaultIsNativeResolution;
            public bool Splash;
            public ScriptingImplementation Backend;
            public ManagedStrippingLevel Stripping;
            public bool VisibleInBackground;
        }

        static Settings Snapshot()
        {
            var target = NamedBuildTarget.Standalone;
            return new Settings
            {
                FullScreenMode = PlayerSettings.fullScreenMode,
                Width = PlayerSettings.defaultScreenWidth,
                Height = PlayerSettings.defaultScreenHeight,
                RunInBackground = PlayerSettings.runInBackground,
                ResizableWindow = PlayerSettings.resizableWindow,
                DefaultIsNativeResolution = PlayerSettings.defaultIsNativeResolution,
                Splash = PlayerSettings.SplashScreen.show,
                Backend = PlayerSettings.GetScriptingBackend(target),
                Stripping = PlayerSettings.GetManagedStrippingLevel(target),
                VisibleInBackground = PlayerSettings.visibleInBackground
            };
        }

        static void ApplyParitySettings()
        {
            var target = NamedBuildTarget.Standalone;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.defaultScreenWidth = 430;
            PlayerSettings.defaultScreenHeight = 932;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.runInBackground = true;
            PlayerSettings.visibleInBackground = true;
            PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.Mono2x);
            PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.Disabled);
            try
            {
                PlayerSettings.SplashScreen.show = false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ParityBuild] cannot disable the splash screen with this license: " + ex.Message);
            }
            if (PlayerSettings.SplashScreen.show) Debug.LogWarning("[ParityBuild] splash screen is still enabled (license restriction)");
        }

        static void Restore(Settings s)
        {
            var target = NamedBuildTarget.Standalone;
            PlayerSettings.fullScreenMode = s.FullScreenMode;
            PlayerSettings.defaultScreenWidth = s.Width;
            PlayerSettings.defaultScreenHeight = s.Height;
            PlayerSettings.runInBackground = s.RunInBackground;
            PlayerSettings.resizableWindow = s.ResizableWindow;
            PlayerSettings.defaultIsNativeResolution = s.DefaultIsNativeResolution;
            PlayerSettings.visibleInBackground = s.VisibleInBackground;
            PlayerSettings.SetScriptingBackend(target, s.Backend);
            PlayerSettings.SetManagedStrippingLevel(target, s.Stripping);
            try
            {
                PlayerSettings.SplashScreen.show = s.Splash;
            }
            catch (Exception)
            {
            }
        }
    }
}
