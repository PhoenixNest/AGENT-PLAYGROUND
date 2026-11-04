using System;
using System.Collections.Generic;
using System.IO;
using OrbitalSalvage.Render;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Editor
{
    /// <summary>
    /// Static spec preview. Open is what the Preview menu calls.
    /// Slider value 0 uses the capture pose (HomeChrome.Draw with no clock), which is the parity still.
    /// A positive value samples HomeChrome.Draw(timeMs). That clock is the spec's CSS clock.
    /// </summary>
    public sealed class PagePreviewWindow : EditorWindow
    {
        const string Title = "Page Preview";

        readonly List<SpecRef> specs = new List<SpecRef>();
        string[] labels = Array.Empty<string>();
        int index;
        string selectedId = "home_default";
        bool useIphone;
        bool fast1x;
        bool fit = true;
        float zoom = 1f;
        float animMs;
        string cacheKey;
        Texture2D preview;
        byte[] png;
        string error;
        int pixelW;
        int pixelH;

        public static void Open(string specId)
        {
            var win = GetWindow<PagePreviewWindow>(false, Title, true);
            win.selectedId = string.IsNullOrEmpty(specId) ? "home_default" : specId;
            win.cacheKey = null;
            win.Show();
        }

        public void Select(string specId)
        {
            selectedId = string.IsNullOrEmpty(specId) ? "home_default" : specId;
            cacheKey = null;
        }

        void OnDisable()
        {
            DestroyPreview();
        }

        void OnGUI()
        {
            try
            {
                DrawContents();
            }
            catch (ExitGUIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox(ex.Message, MessageType.Error);
            }
        }

        public void DrawContents()
        {
            EnsureSpecList();
            DrawToolbar();
            EnsureRaster();
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
            DrawImage();
        }

        void DrawToolbar()
        {
            EditorGUILayout.LabelField("Static spec preview. Does not enter Play mode.");
            if (labels.Length == 0)
            {
                EditorGUILayout.HelpBox("No zero-profile specs were found.", MessageType.Warning);
                return;
            }
            int next = EditorGUILayout.Popup("Page", index, labels);
            if (next != index && next >= 0 && next < specs.Count)
            {
                index = next;
                selectedId = specs[index].Id;
                if (!specs[index].HasIphone) useIphone = false;
                cacheKey = null;
            }

            bool iphoneAvailable = index >= 0 && index < specs.Count && specs[index].HasIphone;
            if (iphoneAvailable)
            {
                bool nextProfile = EditorGUILayout.Toggle("iPhone profile", useIphone);
                if (nextProfile != useIphone)
                {
                    useIphone = nextProfile;
                    cacheKey = null;
                }
            }

            bool nextFast = EditorGUILayout.Toggle("Fast 1x raster", fast1x);
            if (nextFast != fast1x)
            {
                fast1x = nextFast;
                cacheKey = null;
            }

            EditorGUI.BeginChangeCheck();
            animMs = EditorGUILayout.Slider("Animation ms (0 = capture pose)", animMs, 0f, 8000f);
            if (EditorGUI.EndChangeCheck()) cacheKey = null;

            fit = EditorGUILayout.Toggle("Fit", fit);
            if (!fit) zoom = EditorGUILayout.Slider("Zoom", zoom, 0.25f, 2f);

            EditorGUILayout.LabelField("Viewport", PagePreviewPainter.LogicalWidth + "x" + PagePreviewPainter.LogicalHeight
                + " css, raster " + pixelW + "x" + pixelH);

            if (GUILayout.Button("Save PNG") && png != null) SavePng();
        }

        void EnsureSpecList()
        {
            if (specs.Count > 0) return;
            specs.AddRange(PageCatalog.LoadZeroSpecs());
            labels = new string[specs.Count];
            index = 0;
            for (int i = 0; i < specs.Count; i++)
            {
                labels[i] = specs[i].Group + "/" + specs[i].Id;
                if (specs[i].Id == selectedId) index = i;
            }
            if (specs.Count > 0) selectedId = specs[index].Id;
        }

        public void EnsureRaster()
        {
            EnsureSpecList();
            string profile = useIphone && PageCatalog.HasIphone(selectedId) ? "iphone" : "zero";
            bool capture = animMs <= 0.0001f;
            string key = selectedId + "|" + profile + "|" + (capture ? "pose" : animMs.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                + "|" + (fast1x ? "1" : "spec");
            if (key == cacheKey && (preview != null || error != null)) return;
            DestroyPreview();
            try
            {
                // A dirty OutlineFonts cache from an earlier editor paint shifts the shop claim label by a few pixels.
                // The parity stills come from a fresh process. One page view starts from that empty cache.
                OutlineFonts.Clear();
                preview = PagePreviewPainter.Render(selectedId, profile, capture, animMs, fast1x ? 1 : 0, out png);
                pixelW = preview.width;
                pixelH = preview.height;
                error = null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                png = null;
            }
            cacheKey = key;
        }

        void DrawImage()
        {
            if (preview == null) return;
            Rect row = GUILayoutUtility.GetRect(4, 100000, 4, 100000);
            float scale = fit ? Math.Max(0.05f, row.width / PagePreviewPainter.LogicalWidth) : zoom;
            float w = PagePreviewPainter.LogicalWidth * scale;
            float h = PagePreviewPainter.LogicalHeight * scale;
            GUI.DrawTexture(new Rect(row.x, row.y, w, h), preview, ScaleMode.ScaleToFit, false);
        }

        void SavePng()
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string dir = Path.GetFullPath(Path.Combine(project, "..", "..", "output", "preview"));
            Directory.CreateDirectory(dir);
            string profile = useIphone && PageCatalog.HasIphone(selectedId) ? "iphone" : "zero";
            string path = Path.Combine(dir, selectedId + "__" + profile + ".png");
            File.WriteAllBytes(path, png);
            Debug.Log("[PagePreview] wrote " + path);
        }

        void DestroyPreview()
        {
            if (preview != null) DestroyImmediate(preview);
            preview = null;
            png = null;
        }
    }
}
