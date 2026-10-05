using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    public class LayerCacheTests
    {
        [Test]
        public void TextValueChangeDoesNotRebuildOrMoveStaticPixels()
        {
            var chrome = HomeChrome.Load("home_default", "zero");
            try
            {
                chrome.TextOverrides["home-diorama-face"] = "😋";
                chrome.Draw(4000);
                int built = chrome.CacheBuilds;
                Assert.Greater(built, 0, "cache should bake on the first settled draw");
                Assert.IsTrue(chrome.UsesLayerCache);
                var still = Sample(chrome, 20, 200);

                chrome.TextOverrides["home-diorama-face"] = "🥰";
                chrome.Draw(4000);
                Assert.AreEqual(built, chrome.CacheBuilds, "face text is already dynamic");
                var after = Sample(chrome, 20, 200);
                Assert.AreEqual(still.r, after.r);
                Assert.AreEqual(still.g, after.g);
                Assert.AreEqual(still.b, after.b);
                Assert.AreEqual(still.a, after.a);

                chrome.HiddenIds.Add("home-diorama-face");
                chrome.Draw(4000);
                Assert.Greater(chrome.CacheBuilds, built, "a new hidden id changes the baked set");
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
            UnityEngine.Object.DestroyImmediate(one);
            return new Color32((byte)Mathf.RoundToInt(c.r * 255), (byte)Mathf.RoundToInt(c.g * 255), (byte)Mathf.RoundToInt(c.b * 255), (byte)Mathf.RoundToInt(c.a * 255));
        }
    }
}
