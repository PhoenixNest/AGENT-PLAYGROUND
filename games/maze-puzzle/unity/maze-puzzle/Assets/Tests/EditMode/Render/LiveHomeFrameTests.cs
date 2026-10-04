using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    public class LiveHomeFrameTests
    {
        [Test]
        public void LiveFramesKeepThePurplePageAndHeroFace()
        {
            var chrome = HomeChrome.Load("home_default", "zero");
            try
            {
                chrome.Draw();
                var frozenBg = Sample(chrome, 20, 200);
                var frozenFace = Sample(chrome, 215, 431);
                Assert.Greater(frozenFace.r, 180, "frozen hero face");
                Assert.Less(frozenBg.r, 80, "frozen page is not gold");

                for (int i = 0; i < 45; i++) chrome.Draw(900.0 + i * 40.0);

                var liveBg = Sample(chrome, 20, 200);
                var liveFace = Sample(chrome, 215, 431);
                Assert.Less(Mathf.Abs(liveBg.r - frozenBg.r), 48, "live bg r " + liveBg);
                Assert.Less(Mathf.Abs(liveBg.g - frozenBg.g), 48, "live bg g " + liveBg);
                Assert.Less(Mathf.Abs(liveBg.b - frozenBg.b), 48, "live bg b " + liveBg);
                Assert.Greater(liveFace.r, 180, "live hero face drifted off the sphere: " + liveFace);
                Assert.Less(liveFace.b, 140, "live hero is the blue sphere: " + liveFace);
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
