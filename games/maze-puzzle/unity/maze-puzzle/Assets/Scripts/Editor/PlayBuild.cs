using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace OrbitalSalvage.Editor
{
    /// <summary>
    /// Standalone player that forces Direct3D11. The parity capture build
    /// (<see cref="ParityBuild"/>) does not touch the graphics API list, so its stills stay on Unity's default.
    /// ProjectSettings are restored, including a byte-for-byte writeback, before this returns.
    /// Editor Play mode uses the Editor process API: start Unity with <c>-force-d3d11</c>.
    /// </summary>
    public static class PlayBuild
    {
        public const string OutputRelative = "Builds/Play/OrbitalSalvage.exe";

        [MenuItem("Orbital Salvage/Build/Windows Play Player (D3D11)", false, 200)]
        public static void BuildPlayer()
        {
            int code;
            try
            {
                code = Build();
            }
            catch (Exception ex)
            {
                Debug.LogError("[PlayBuild] " + ex);
                code = 1;
            }

            if (Application.isBatchMode) EditorApplication.Exit(code);
        }

        static int Build()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[PlayBuild] no enabled scenes in EditorBuildSettings");
                return 1;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string exePath = Path.Combine(projectRoot, OutputRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(exePath));

            string settingsPath = Path.Combine(projectRoot, "ProjectSettings", "ProjectSettings.asset");
            byte[] settingsBytes = File.Exists(settingsPath) ? File.ReadAllBytes(settingsPath) : null;
            bool defaultApis = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64);
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            try
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
                var options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = exePath,
                    target = BuildTarget.StandaloneWindows64,
                    targetGroup = BuildTargetGroup.Standalone,
                    options = BuildOptions.None
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                var summary = report.summary;
                Debug.Log($"[PlayBuild] result={summary.result} api=Direct3D11 output={summary.outputPath}");
                return summary.result == BuildResult.Succeeded ? 0 : 1;
            }
            finally
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, defaultApis);
                if (!defaultApis && apis != null && apis.Length > 0)
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, apis);
                AssetDatabase.SaveAssets();
                if (settingsBytes != null && File.Exists(settingsPath))
                {
                    var after = File.ReadAllBytes(settingsPath);
                    if (!after.AsSpan().SequenceEqual(settingsBytes))
                    {
                        Debug.LogWarning("[PlayBuild] ProjectSettings.asset differs after restore; writing the original bytes back");
                        File.WriteAllBytes(settingsPath, settingsBytes);
                    }
                }
            }
        }
    }
}
