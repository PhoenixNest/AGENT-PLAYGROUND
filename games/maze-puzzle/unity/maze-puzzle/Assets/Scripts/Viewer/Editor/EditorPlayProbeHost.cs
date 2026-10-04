using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Editor
{
    /// <summary>
    /// Opens MainGameScene, sizes the Game view, and enters Play mode.
    /// Invoked with <c>-executeMethod OrbitalSalvage.Viewer.Editor.EditorPlayProbeHost.Run</c>
    /// plus <c>-editor-play-probe &lt;dir&gt; -probe-w -probe-h</c>. Not batch mode: the Game view needs a GPU.
    /// </summary>
    public static class EditorPlayProbeHost
    {
        const string ScenePath = "Assets/Scenes/MainGameScene.unity";

        public static void Run()
        {
            try
            {
                EditorPrefs.SetBool(ParityPages.EnabledPrefKey, true);
                int w = ArgInt("-probe-w", 1284);
                int h = ArgInt("-probe-h", 2778);
                EditorSceneManager.OpenScene(ScenePath);
                SetGameViewSize(w, h);
                Debug.Log("[EditorPlayProbe] enter play " + w + "x" + h + " api=" + SystemInfo.graphicsDeviceType + " " + SystemInfo.graphicsDeviceName);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex)
            {
                Debug.LogError("[EditorPlayProbe] host " + ex);
                EditorApplication.Exit(1);
            }
        }

        [InitializeOnLoadMethod]
        static void Watch()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-editor-play-probe") < 0) return;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode)
                    EditorPrefs.SetString("EditorPlayProbe.Entered", DateTime.UtcNow.ToString("o"));
            };
            EditorApplication.update += () =>
            {
                if (!EditorApplication.isPlaying) return;
                string raw = EditorPrefs.GetString("EditorPlayProbe.Entered", "");
                if (!DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out var entered)) return;
                if ((DateTime.UtcNow - entered).TotalSeconds > 420)
                {
                    Debug.LogError("[EditorPlayProbe] timed out in play mode");
                    EditorApplication.Exit(2);
                }
            };
        }

        static int ArgInt(string name, int fallback)
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, name);
            if (i < 0 || i + 1 >= args.Length) return fallback;
            return int.TryParse(args[i + 1], out int v) ? v : fallback;
        }

        static void SetGameViewSize(int width, int height)
        {
            var asm = typeof(UnityEditor.Editor).Assembly;
            var sizesType = asm.GetType("UnityEditor.GameViewSizes");
            var single = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instance = single.GetProperty("instance").GetValue(null, null);
            var groupEnum = asm.GetType("UnityEditor.GameViewSizeGroupType");
            var standalone = Enum.Parse(groupEnum, "Standalone");
            var group = instance.GetType().GetMethod("GetGroup").Invoke(instance, new[] { standalone });

            var sizeType = asm.GetType("UnityEditor.GameViewSize");
            var kindEnum = asm.GetType("UnityEditor.GameViewSizeType");
            var ctor = sizeType.GetConstructor(new[] { kindEnum, typeof(int), typeof(int), typeof(string) });
            var fixedRes = Enum.Parse(kindEnum, "FixedResolution");
            string label = "Probe " + width + "x" + height;
            var size = ctor.Invoke(new object[] { fixedRes, width, height, label });
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { size });
            int index = (int)group.GetType().GetMethod("IndexOf", new[] { sizeType }).Invoke(group, new[] { size });

            var gvType = asm.GetType("UnityEditor.GameView");
            var window = EditorWindow.GetWindow(gvType, false, "Game", true);
            var selected = gvType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (selected != null) selected.SetValue(window, index, null);
            else
            {
                var cb = gvType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                cb.Invoke(window, new object[] { index, null });
            }
            window.Repaint();
            Debug.Log("[EditorPlayProbe] game view size index " + index + " " + label);
        }
    }
}
