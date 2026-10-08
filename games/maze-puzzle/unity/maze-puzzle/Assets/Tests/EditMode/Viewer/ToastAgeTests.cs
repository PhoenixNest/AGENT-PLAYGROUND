using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class ToastAgeTests
    {
        const int SampleX = 215;
        const int SampleY = 96;

        [Test]
        public void ShopSpec_LoopsOnGiftBanner()
        {
            var shop = HomeChrome.Load("shop_rings_default", "zero");
            try { Assert.IsTrue(shop.Loops, "giftBounce and giftBtnPulse are visible infinite animations"); }
            finally { shop.Dispose(); }
        }

        [Test]
        public void ToastOnNonLoopingPage_FadesInHoldsAndClears()
        {
            var chrome = HomeChrome.Load("settings_default", "zero");
            var go = new GameObject("toast-probe");
            try
            {
                Assert.IsFalse(chrome.Loops, "settings has no visible infinite animation");
                var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
                app.ShowPage("settings");
                var viewer = go.AddComponent<ParityViewer>();
                viewer.BindToastProbe(chrome, "settings_default", app);
                viewer.SetPageAgeMs(1000);

                Assert.AreEqual(0, viewer.ProbeToastFrame(), "settled non-looping page does not redraw");

                app.Click("btn-home-audio");
                Assert.AreEqual(1, app.Model.Toasts.Count);
                int draws = viewer.ProbeToastFrame();
                Assert.AreEqual(1, draws, "toast age 0 still redraws");
                Assert.AreEqual(1, chrome.Toasts.Count);
                Assert.Less(chrome.Toasts[0].AgeMs, 1.0);
                int built = chrome.CacheBuilds;
                Assert.Greater(built, 0);
                var bare = Sample(chrome, SampleX, SampleY);
                Assert.IsFalse(IsWhite(bare), "opacity 0 toast must not leave a white box, got " + bare);

                app.Advance(390);
                Assert.AreEqual(1, viewer.ProbeToastFrame());
                Assert.AreEqual(390, chrome.Toasts[0].AgeMs, 0.01);
                Assert.IsTrue(IsWhite(Sample(chrome, SampleX, SampleY)), "15% of 2600 ms is fully opaque");
                Assert.AreEqual(built, chrome.CacheBuilds, "toast paint must not rebuild the layer cache");

                app.Advance(610);
                Assert.AreEqual(1, viewer.ProbeToastFrame());
                Assert.AreEqual(1000, chrome.Toasts[0].AgeMs, 0.01);
                Assert.IsTrue(IsWhite(Sample(chrome, SampleX, SampleY)), "mid hold is fully opaque");

                app.Advance(1132);
                Assert.AreEqual(1, viewer.ProbeToastFrame());
                Assert.AreEqual(2132, chrome.Toasts[0].AgeMs, 0.01);
                Assert.IsTrue(IsWhite(Sample(chrome, SampleX, SampleY)), "82% of 2600 ms is still fully opaque");
                Assert.AreEqual(built, chrome.CacheBuilds);

                app.Advance(368);
                Assert.AreEqual(0, app.Model.Toasts.Count, "controller removes the toast at 2500 ms");
                Assert.AreEqual(1, viewer.ProbeToastFrame(), "one redraw when the last toast disappears");
                Assert.AreEqual(0, chrome.Toasts.Count);
                var cleared = Sample(chrome, SampleX, SampleY);
                Assert.AreEqual(bare.r, cleared.r);
                Assert.AreEqual(bare.g, cleared.g);
                Assert.AreEqual(bare.b, cleared.b);
                Assert.AreEqual(bare.a, cleared.a);
                Assert.AreEqual(built, chrome.CacheBuilds);

                Assert.AreEqual(0, viewer.ProbeToastFrame(), "no further redraws after the toast is gone");
            }
            finally
            {
                chrome.Dispose();
                Object.DestroyImmediate(go);
            }
        }

        static bool IsWhite(Color32 c) => c.r >= 250 && c.g >= 250 && c.b >= 250;

        static Color32 Sample(HomeChrome chrome, int cssX, int cssY)
        {
            var rt = chrome.Target;
            int scale = System.Math.Max(1, rt.width / 430);
            int px = Mathf.Clamp(cssX * scale, 0, rt.width - 1);
            int py = Mathf.Clamp(rt.height - 1 - cssY * scale, 0, rt.height - 1);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var one = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            one.ReadPixels(new Rect(px, py, 1, 1), 0, 0, false);
            one.Apply(false);
            var c = one.GetPixel(0, 0);
            RenderTexture.active = prev;
            Object.DestroyImmediate(one);
            return new Color32((byte)Mathf.RoundToInt(c.r * 255), (byte)Mathf.RoundToInt(c.g * 255), (byte)Mathf.RoundToInt(c.b * 255), (byte)Mathf.RoundToInt(c.a * 255));
        }
    }
}
