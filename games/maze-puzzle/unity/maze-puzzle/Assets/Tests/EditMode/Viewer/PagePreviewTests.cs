using System;
using System.Collections.Generic;
using System.Globalization;
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
        public void Catalog_GroupsSpecsAndOffersIphoneForEveryPage()
        {
            var specs = PageCatalog.LoadZeroSpecs();
            Assert.GreaterOrEqual(specs.Count, PageCatalog.MenuSpecIds.Length);
            var ids = new System.Collections.Generic.HashSet<string>();
            int iphone = 0;
            foreach (var spec in specs)
            {
                ids.Add(spec.Id);
                Assert.AreEqual(PageCatalog.GroupOf(spec.Id), spec.Group);
                Assert.IsTrue(spec.HasIphone, spec.Id);
                if (spec.HasIphone) iphone++;
            }
            foreach (string id in PageCatalog.MenuSpecIds) Assert.IsTrue(ids.Contains(id), id);
            Assert.IsTrue(PageCatalog.HasIphone("home"));
            Assert.IsTrue(PageCatalog.HasIphone("home_default"));
            Assert.IsTrue(PageCatalog.HasIphone("daily_default"));
            Assert.AreEqual(specs.Count, iphone);
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

        /// <summary>
        /// Every <c>screenshot-states.json</c> entry, painted by <see cref="PagePreviewPainter"/>
        /// with the parity player's clock: <c>Draw()</c> when the state has no time offset, otherwise
        /// <c>Draw(CaptureTimeMs + timeOffsetMs)</c>. Bytes must match the D3D12 player still.
        /// The earlier shop PNG differed by 9 pixels on the SDF edge of "Claim +100"; a repaint
        /// through this path matches the still, so the bar is byte identity, including shop_rings_default.
        /// </summary>
        [Test]
        public void Stills_MatchTheParityPlayer_IncludingShopRingsDefault()
        {
            string project = Directory.GetParent(Application.dataPath).FullName;
            string maze = Path.GetFullPath(Path.Combine(project, "..", ".."));
            string statesPath = Path.Combine(maze, "tools", "parity", "unity", "screenshot-states.json");
            string stillDir = Path.Combine(maze, "tools", "parity", "out", "unity-before");
            if (!File.Exists(statesPath) || !Directory.Exists(stillDir))
                Assert.Ignore("parity stills are not in this checkout");

            var root = MiniJson.Parse(File.ReadAllText(statesPath)) as List<object>;
            Assert.IsNotNull(root);
            Assert.AreEqual(23, root.Count);
            // Earlier EditMode paints leave flattened glyphs in OutlineFonts. The parity player
            // starts empty, and a second shop paint in a dirty cache moves 9 pixels on "Claim +100".
            OutlineFonts.Clear();
            bool sawShop = false;
            foreach (var item in root)
            {
                var state = item as Dictionary<string, object>;
                string id = Convert.ToString(state["id"]);
                string spec = state.ContainsKey("spec") && state["spec"] != null ? Convert.ToString(state["spec"]) : id;
                bool hasOffset = state.ContainsKey("timeOffsetMs") && state["timeOffsetMs"] != null;
                double offset = hasOffset ? Convert.ToDouble(state["timeOffsetMs"], CultureInfo.InvariantCulture) : 0;
                string stillPath = Path.Combine(stillDir, id + "__zero.png");
                Assert.IsTrue(File.Exists(stillPath), id);
                byte[] still = File.ReadAllBytes(stillPath);
                byte[] preview = PaintLikeThePlayer(spec, hasOffset, offset);
                if (id == "shop_rings_default")
                {
                    sawShop = true;
                    Assert.IsFalse(hasOffset, "shop_rings_default is a capture-pose still");
                }
                if (!BytesEqual(still, preview))
                {
                    string dir = Path.Combine(maze, "output", "preview-diff");
                    Directory.CreateDirectory(dir);
                    File.WriteAllBytes(Path.Combine(dir, id + "__preview.png"), preview);
                    int pixels = CountPixelDiffs(still, preview, out int maxDelta, out int minX, out int minY, out int maxX, out int maxY);
                    Assert.Fail(id + " pixels=" + pixels + " maxDelta=" + maxDelta + " bbox=" + minX + "," + minY + "-" + maxX + "," + maxY
                        + " pngBytes still=" + still.Length + " preview=" + preview.Length);
                }
            }
            Assert.IsTrue(sawShop);

            OutlineFonts.Clear();
            byte[] shopStill = File.ReadAllBytes(Path.Combine(stillDir, "shop_rings_default__zero.png"));
            byte[] shopAlone = Paint("shop_rings_default", true, 0);
            if (!BytesEqual(shopStill, shopAlone))
            {
                int pixels = CountPixelDiffs(shopStill, shopAlone, out int maxDelta, out int minX, out int minY, out int maxX, out int maxY);
                Assert.Fail("shop alone pixels=" + pixels + " maxDelta=" + maxDelta + " bbox=" + minX + "," + minY + "-" + maxX + "," + maxY);
            }
        }

        /// <summary>
        /// The unstamped fresh maps still from <see cref="PagePreviewPainter"/> matches a direct
        /// HomeChrome readback. The published pin E8DB1961… is not asserted: on this Mac it was
        /// recorded next to the Windows game hash, which Metal does not reproduce.
        /// </summary>
        [Test]
        public void MapsWorld1Fresh_PainterMatchesDirectReadback()
        {
            OutlineFonts.Clear();
            byte[] painter = Paint("maps_world_1_fresh", true, 0);
            byte[] direct = IndependentCapturePose("maps_world_1_fresh");
            Assert.IsTrue(BytesEqual(painter, direct), "PagePreviewPainter and direct readback diverged");
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(painter);
                var sb = new System.Text.StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                Directory.CreateDirectory("/tmp/maze-maps");
                File.WriteAllText(Path.Combine("/tmp/maze-maps", "painter-maps-fresh-sha256.txt"), sb.ToString());
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

        static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        static int CountPixelDiffs(byte[] a, byte[] b, out int maxDelta, out int minX, out int minY, out int maxX, out int maxY)
        {
            maxDelta = 0;
            minX = minY = int.MaxValue;
            maxX = maxY = -1;
            var ta = new Texture2D(2, 2, TextureFormat.RGB24, false, true);
            var tb = new Texture2D(2, 2, TextureFormat.RGB24, false, true);
            try
            {
                if (!ImageConversion.LoadImage(ta, a) || !ImageConversion.LoadImage(tb, b)) return -1;
                if (ta.width != tb.width || ta.height != tb.height) return -1;
                var pa = ta.GetPixels32();
                var pb = tb.GetPixels32();
                int n = 0;
                int w = ta.width;
                int h = ta.height;
                for (int i = 0; i < pa.Length; i++)
                {
                    int d = Math.Max(Math.Abs(pa[i].r - pb[i].r), Math.Max(Math.Abs(pa[i].g - pb[i].g), Math.Abs(pa[i].b - pb[i].b)));
                    if (d == 0) continue;
                    n++;
                    if (d > maxDelta) maxDelta = d;
                    int x = i % w;
                    int top = h - 1 - (i / w);
                    if (x < minX) minX = x;
                    if (top < minY) minY = top;
                    if (x > maxX) maxX = x;
                    if (top > maxY) maxY = top;
                }
                return n;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ta);
                UnityEngine.Object.DestroyImmediate(tb);
            }
        }

        static byte[] PaintLikeThePlayer(string spec, bool hasOffset, double offset)
        {
            var chrome = HomeChrome.Load(spec, "zero");
            double timeMs = chrome.CaptureTimeMs + offset;
            chrome.Dispose();
            return Paint(spec, !hasOffset, timeMs);
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
