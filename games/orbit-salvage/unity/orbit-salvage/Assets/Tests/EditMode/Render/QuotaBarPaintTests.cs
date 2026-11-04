using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    public class QuotaBarPaintTests
    {
        // hud-quota-bar rest box is x=166.672 y=26.5 h=6, width 0 in the capture. Sample inside a partial fill.
        const int SampleX = 190;
        const int SampleY = 29;

        [Test]
        public void WidthOverride_PaintsTheFill_ZeroWidthMatchesTheHiddenBar()
        {
            var chrome = HomeChrome.Load("game_lvl01_t1000ms", "zero");
            try
            {
                chrome.Draw();
                var hidden = Sample(chrome, SampleX, SampleY);

                chrome.WidthOverrides["hud-quota-bar"] = 80;
                chrome.Draw();
                var filled = Sample(chrome, SampleX, SampleY);
                Assert.Greater(filled.r, hidden.r + 40, "fill r " + filled + " hidden " + hidden);
                Assert.Greater(filled.g, 140, "yellow fill g " + filled);

                chrome.WidthOverrides["hud-quota-bar"] = 0;
                chrome.Draw();
                var cleared = Sample(chrome, SampleX, SampleY);
                Assert.Less(Mathf.Abs(cleared.r - hidden.r), 8, "width 0 r " + cleared + " hidden " + hidden);
                Assert.Less(Mathf.Abs(cleared.g - hidden.g), 8, "width 0 g " + cleared);
                Assert.Less(Mathf.Abs(cleared.b - hidden.b), 8, "width 0 b " + cleared);
            }
            finally
            {
                chrome.Dispose();
            }
        }

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
