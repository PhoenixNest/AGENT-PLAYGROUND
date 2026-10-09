using System;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.Render;
using OrbitalSalvage.Viewer.Editor;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class PagePreviewTests
    {
        static readonly string[] States =
        {
            "home_default",
            "shop_rings_default",
            "modal_pause",
            "settings_default"
        };

        [Test]
        public void Catalog_GroupsSpecsAndOnlyHomeHasAnIphonePair()
        {
            var specs = PageCatalog.LoadZeroSpecs();
            Assert.GreaterOrEqual(specs.Count, PageCatalog.MenuSpecIds.Length);
            var ids = new System.Collections.Generic.HashSet<string>();
            int iphone = 0;
            foreach (var spec in specs)
            {
                ids.Add(spec.Id);
                Assert.AreEqual(PageCatalog.GroupOf(spec.Id), spec.Group);
                if (spec.HasIphone) iphone++;
            }
            foreach (string id in PageCatalog.MenuSpecIds) Assert.IsTrue(ids.Contains(id), id);
            Assert.IsTrue(PageCatalog.HasIphone("home"));
            Assert.IsFalse(PageCatalog.HasIphone("home_default"));
            Assert.AreEqual(1, iphone, "profile toggle is only for specs that have both zero and iphone");
        }

        [Test]
        public void CapturePose_MatchesTheEditModePaintPath()
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string dir = Path.GetFullPath(Path.Combine(project, "..", "..", "output", "preview"));
            Directory.CreateDirectory(dir);
            foreach (string id in States)
            {
                byte[] window = Paint(id, true, 0);
                byte[] reference = IndependentCapturePose(id);
                Assert.AreEqual(reference.Length, window.Length, id);
                for (int i = 0; i < reference.Length; i++)
                    if (reference[i] != window[i]) Assert.Fail(id + " png differs at byte " + i);
                File.WriteAllBytes(Path.Combine(dir, id + "__zero.png"), window);
            }
        }

        [Test]
        public void AnimationClock_IsDeterministic()
        {
            byte[] a = Paint("home_default", false, 500);
            byte[] b = Paint("home_default", false, 500);
            Assert.AreEqual(a.Length, b.Length);
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) Assert.Fail("Draw(500) differed at byte " + i);
        }

        [Test]
        public void Window_DoesNotEnterPlayModeOrTouchTheProfile()
        {
            string profile = Path.Combine(Application.persistentDataPath, FileProfileStore.FileName);
            byte[] before = File.Exists(profile) ? File.ReadAllBytes(profile) : null;
            const string sentinel = "OrbitalSalvage.PagePreview.TestSentinel";
            EditorPrefs.DeleteKey(sentinel);
            int prefs = PlayerPrefs.GetInt(sentinel, 0);
            double quality = PlayQuality.Forced;
            var win = ScriptableObject.CreateInstance<PagePreviewWindow>();
            try
            {
                Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);
                win.Select("settings_default");
                win.EnsureRaster();
                win.ShowUtility();
                win.SendEvent(new Event { type = EventType.Layout });
                win.SendEvent(new Event { type = EventType.Repaint });
                Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);
                Assert.IsFalse(Application.isPlaying);
            }
            finally
            {
                win.Close();
                UnityEngine.Object.DestroyImmediate(win);
                PlayQuality.Forced = quality;
            }

            byte[] after = File.Exists(profile) ? File.ReadAllBytes(profile) : null;
            if (before == null) Assert.IsNull(after);
            else
            {
                Assert.IsNotNull(after);
                Assert.AreEqual(before.Length, after.Length);
                for (int i = 0; i < before.Length; i++)
                    if (before[i] != after[i]) Assert.Fail("profile file changed at byte " + i);
            }
            Assert.AreEqual(prefs, PlayerPrefs.GetInt(sentinel, 0));
            Assert.IsFalse(EditorPrefs.HasKey(sentinel));
            Assert.AreEqual(quality, PlayQuality.Forced);
        }

        static byte[] Paint(string id, bool capturePose, double timeMs)
        {
            Texture2D tex = null;
            try
            {
                tex = PagePreviewPainter.Render(id, "zero", capturePose, timeMs, 0, out byte[] png);
                Assert.IsNotNull(png, id);
                Assert.Greater(png.Length, 8, id);
                if (capturePose)
                {
                    Assert.AreEqual(PagePreviewPainter.LogicalWidth * 3, tex.width, id);
                    Assert.AreEqual(PagePreviewPainter.LogicalHeight * 3, tex.height, id);
                }
                return png;
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        /// <summary>
        /// Same steps as the parity player: HomeChrome.Draw (capture pose), copy into an ARGB32
        /// Default target, RGB24 ReadPixels with the linear flag. Not the painter's method.
        /// </summary>
        static byte[] IndependentCapturePose(string id)
        {
            var chrome = HomeChrome.Load(id, "zero");
            RenderTexture copy = null;
            Texture2D read = null;
            var previous = RenderTexture.active;
            try
            {
                chrome.Draw();
                var source = chrome.Target;
                copy = new RenderTexture(source.width, source.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
                {
                    name = "PagePreviewReference",
                    antiAliasing = 1,
                    filterMode = FilterMode.Point
                };
                copy.Create();
                Graphics.CopyTexture(source, copy);
                RenderTexture.active = copy;
                read = new Texture2D(copy.width, copy.height, TextureFormat.RGB24, false, true);
                read.ReadPixels(new Rect(0, 0, copy.width, copy.height), 0, 0, false);
                read.Apply(false);
                return read.EncodeToPNG();
            }
            finally
            {
                RenderTexture.active = previous;
                if (read != null) UnityEngine.Object.DestroyImmediate(read);
                if (copy != null)
                {
                    copy.Release();
                    UnityEngine.Object.DestroyImmediate(copy);
                }
                chrome.Dispose();
            }
        }
    }
}
