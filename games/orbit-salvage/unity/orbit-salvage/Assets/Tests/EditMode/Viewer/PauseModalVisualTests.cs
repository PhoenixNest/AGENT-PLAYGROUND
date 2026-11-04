using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.Render;
using OrbitalSalvage.Viewer.Editor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    /// <summary>
    /// Pause sheet corners, one-shot playfield blur, and the modal-open frames.
    /// PNGs land in /tmp/maze-pause for comparison with the web still.
    /// </summary>
    public class PauseModalVisualTests
    {
        const string Dir = "/tmp/maze-pause";

        [Test]
        public void PauseStill_LogCaptureHashes()
        {
            Directory.CreateDirectory(Dir);
            string zero = StillHash("modal_pause", "zero");
            string iphone = StillHash("modal_pause", "iphone");
            File.WriteAllText(Path.Combine(Dir, "capture-hashes.txt"), "zero " + zero + "\niphone " + iphone + "\n");
            Debug.Log("[pause-modal] capture zero " + zero);
            Debug.Log("[pause-modal] capture iphone " + iphone);
            Assert.That(zero.Length, Is.EqualTo(64));
            Assert.That(iphone.Length, Is.EqualTo(64));
        }

        static string StillHash(string id, string profile)
        {
            Texture2D tex = null;
            try
            {
                tex = PagePreviewPainter.Render(id, profile, true, 0, 0, out byte[] png);
                using (var sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(png);
                    var sb = new StringBuilder(hash.Length * 2);
                    for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                    return sb.ToString();
                }
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void PauseSheet_CornerOutsideRadiusIsNotCardFill()
        {
            Directory.CreateDirectory(Dir);
            var chrome = HomeChrome.Load("modal_pause", "iphone");
            try
            {
                chrome.ResizeRaster(2);
                chrome.Draw(400, -1, null, false, true);
                Save(chrome, "iphone_settled.png");
                // Card is [50,466,330,466], top radii 32. (54,470) is inside the border box
                // and outside the quarter-circle (centre 82,498; distance ~39.6).
                // (280,500) is header fill, clear of the title and the close button.
                var corner = Sample(chrome, 54, 470);
                var flat = Sample(chrome, 280, 500);
                Debug.Log("[pause-modal] corner " + corner + " flat " + flat + " hash " + Hash(chrome));
                Assert.That(IsCardFill(flat), Is.True, "flat top of the sheet should stay the card fill " + flat);
                Assert.That(IsCardFill(corner), Is.False, "top corner outside the 32px radius was the card fill " + corner);
            }
            finally
            {
                chrome.Dispose();
            }
        }

        [Test]
        public void PauseIntro_HasNoOpaqueWhiteSlab()
        {
            Directory.CreateDirectory(Dir);
            var chrome = HomeChrome.Load("modal_pause", "iphone");
            try
            {
                chrome.ResizeRaster(2);
                int worst = 0;
                string worstName = "";
                int nPx = 0;
                bool sheetOpenAtStart = false;
                bool sheetVisibleMidway = false;
                for (int ms = 0; ms <= 400; ms += 40)
                {
                    chrome.Draw(ms, -1, null, true, true);
                    string name = "iphone_intro_" + ms.ToString("000") + ".png";
                    var px = Read(chrome);
                    nPx = px.Length;
                    SavePixels(px, chrome.Target.width, chrome.Target.height, name);
                    int white = CountWhite(px);
                    var low = Sample(chrome, 200, 800);
                    var head = Sample(chrome, 280, 470);
                    Debug.Log("[pause-modal] intro " + ms + " white " + white + " / " + px.Length + " y800 " + low + " y470 " + head);
                    if (ms == 0) sheetOpenAtStart = IsCardFill(low);
                    if (ms == 200) sheetVisibleMidway = IsCardFill(head);
                    if (white > worst)
                    {
                        worst = white;
                        worstName = name;
                    }
                }
                // The playfield donut is white (about a tenth of the frame once the sheet starts off-screen).
                // The device flash covers most of the frame.
                Assert.That(worst, Is.LessThan(nPx / 4), "opaque white slab during intro (" + worstName + " " + worst + " px)");
                // t=0 keeps the sheet off the bottom. The layer cache used to blit it fully open.
                Assert.That(sheetOpenAtStart, Is.False, "sheet was already open on the first intro frame");
                Assert.That(sheetVisibleMidway, Is.True, "sheet was missing halfway through the intro");
            }
            finally
            {
                chrome.Dispose();
            }
        }

        [Test]
        public void PauseBackdrop_BlursSyntheticPlayfieldAndReusesIt()
        {
            Directory.CreateDirectory(Dir);
            RenderTexture pattern = null;
            RenderTexture resized = null;
            var chrome = HomeChrome.Load("modal_pause", "iphone");
            try
            {
                pattern = Pattern();
                chrome.ResizeRaster(1);
                chrome.ExternalPlayfield = pattern;
                chrome.Draw(400, -1, null, false, true);
                int builds = chrome.PlayfieldBlurBuilds;
                var edge = Sample(chrome, 208, 200);
                var far = Sample(chrome, 40, 200);
                Save(chrome, "iphone_blur_underlay.png");
                chrome.Draw(400, -1, null, false, true);
                var second = Sample(chrome, 208, 200);
                Debug.Log("[pause-modal] blur edge " + edge + " far " + far + " builds " + chrome.PlayfieldBlurBuilds);
                Assert.That(builds, Is.EqualTo(1), "opening the modal should blur the playfield once");
                Assert.That(chrome.PlayfieldBlurBuilds, Is.EqualTo(1), "a second settled draw must reuse the snapshot");
                // Edge is 1px left of the green/magenta split. An 8px blur pulls magenta (red) into it.
                Assert.That(edge.r, Is.GreaterThan(far.r + 8), "backdrop stayed sharp edge " + edge + " far " + far);
                Assert.That(edge.g, Is.GreaterThan(25), "backdrop lost the playfield " + edge);
                Assert.That(Math.Abs(edge.r - second.r) + Math.Abs(edge.g - second.g) + Math.Abs(edge.b - second.b), Is.LessThan(8));

                ReleasePattern(ref pattern);
                resized = Pattern(860, 1864);
                chrome.ExternalPlayfield = resized;
                chrome.Draw(400, -1, null, false, true);
                Assert.That(chrome.PlayfieldBlurBuilds, Is.EqualTo(2), "a playfield size change must rebuild the snapshot");
            }
            finally
            {
                chrome.ExternalPlayfield = null;
                chrome.Dispose();
                ReleasePattern(ref pattern);
                ReleasePattern(ref resized);
            }
        }

        static void ReleasePattern(ref RenderTexture rt)
        {
            if (rt == null) return;
            if (RenderTexture.active == rt) RenderTexture.active = null;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            rt = null;
        }

        static RenderTexture Pattern(int w = 430, int h = 932)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            var px = new Color32[w * h];
            int mid = w / 2;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                    px[y * w + x] = x < mid ? new Color32(0, 255, 0, 255) : new Color32(255, 0, 255, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "PauseModalPattern",
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };
            rt.Create();
            Graphics.Blit(tex, rt);
            UnityEngine.Object.DestroyImmediate(tex);
            return rt;
        }

        static bool IsCardFill(Color32 c)
        {
            return Math.Abs(c.r - 49) <= 6 && Math.Abs(c.g - 46) <= 6 && Math.Abs(c.b - 129) <= 6 && c.a > 200;
        }

        static int CountWhite(Color32[] px)
        {
            int n = 0;
            for (int i = 0; i < px.Length; i++)
            {
                var c = px[i];
                if (c.a > 240 && c.r > 250 && c.g > 250 && c.b > 250) n++;
            }
            return n;
        }

        static void Save(HomeChrome chrome, string name)
        {
            SavePixels(Read(chrome), chrome.Target.width, chrome.Target.height, name);
        }

        static void SavePixels(Color32[] px, int w, int h, string name)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true) { hideFlags = HideFlags.HideAndDontSave };
            tex.SetPixels32(px);
            tex.Apply(false, false);
            File.WriteAllBytes(Path.Combine(Dir, name), tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static Color32[] Read(HomeChrome chrome)
        {
            var rt = chrome.Target;
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            tex.Apply(false, false);
            RenderTexture.active = prev;
            var px = tex.GetPixels32();
            UnityEngine.Object.DestroyImmediate(tex);
            return px;
        }

        static Color32 Sample(HomeChrome chrome, int cssX, int cssY)
        {
            var rt = chrome.Target;
            int scale = Math.Max(1, rt.width / 430);
            int px = Mathf.Clamp(cssX * scale, 0, rt.width - 1);
            int py = Mathf.Clamp(rt.height - 1 - cssY * scale, 0, rt.height - 1);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var one = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            one.ReadPixels(new Rect(px, py, 1, 1), 0, 0, false);
            one.Apply(false, false);
            var c = one.GetPixel(0, 0);
            RenderTexture.active = prev;
            UnityEngine.Object.DestroyImmediate(one);
            return new Color32((byte)Mathf.RoundToInt(c.r * 255), (byte)Mathf.RoundToInt(c.g * 255), (byte)Mathf.RoundToInt(c.b * 255), (byte)Mathf.RoundToInt(c.a * 255));
        }

        static string Hash(HomeChrome chrome)
        {
            var rt = chrome.Target;
            var copy = new RenderTexture(rt.width, rt.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            copy.Create();
            Graphics.Blit(rt, copy);
            var prev = RenderTexture.active;
            RenderTexture.active = copy;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false, true);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            tex.Apply(false, false);
            RenderTexture.active = prev;
            byte[] png = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);
            copy.Release();
            UnityEngine.Object.DestroyImmediate(copy);
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(png);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }
    }
}
