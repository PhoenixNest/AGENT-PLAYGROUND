using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    public class GpuDirtyTests
    {
        [TearDown]
        public void Clean() { RenderOpts.Clear(); }

        [Test]
        public void DirtySecondHomeFrame_StaysNearTheFullRepaint()
        {
            HomeChrome full = null;
            HomeChrome dirty = null;
            try
            {
                full = Paint(false);
                dirty = Paint(true);
                var aPx = Read(full);
                var bPx = Read(dirty);
                int max = 0;
                int over = 0;
                long sum = 0;
                int n = 0;
                int w = full.Target.width;
                int h = full.Target.height;
                int step = 4;
                for (int y = 0; y < h; y += step)
                {
                    for (int x = 0; x < w; x += step)
                    {
                        int i = y * w + x;
                        int d = Diff(aPx[i], bPx[i]);
                        if (d > max) max = d;
                        if (d > 8) over++;
                        sum += d;
                        n++;
                    }
                }
                double mean = n == 0 ? 0 : sum / (double)n;
                Assert.LessOrEqual(max, 24, "max " + max + " mean " + mean.ToString("0.00") + " over8 " + over + "/" + n);
            }
            finally
            {
                RenderOpts.Clear();
                if (full != null) full.Dispose();
                if (dirty != null) dirty.Dispose();
            }
        }

        [Test]
        public void DirtyClip_SecondHomeFrame_StaysNearTheFullRepaint()
        {
            HomeChrome full = null;
            HomeChrome clip = null;
            try
            {
                full = PaintClip(false);
                clip = PaintClip(true);
                var aPx = Read(full);
                var bPx = Read(clip);
                int max = 0;
                int over = 0;
                long sum = 0;
                int n = 0;
                int w = full.Target.width;
                int h = full.Target.height;
                for (int y = 0; y < h; y += 2)
                {
                    for (int x = 0; x < w; x += 2)
                    {
                        int i = y * w + x;
                        int d = Diff(aPx[i], bPx[i]);
                        if (d > max) max = d;
                        if (d > 8) over++;
                        sum += d;
                        n++;
                    }
                }
                int minX = w, minY = h, maxX = 0, maxY = 0, hot = 0;
                for (int y = 0; y < h; y += 2)
                {
                    for (int x = 0; x < w; x += 2)
                    {
                        if (Diff(aPx[y * w + x], bPx[y * w + x]) <= 30) continue;
                        hot++;
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
                double mean = n == 0 ? 0 : sum / (double)n;
                // The shine band used to miss entirely (white vs the button, max well above 200).
                // What remains is the pedestal glow fringe and a few glyph edges.
                Assert.LessOrEqual(max, 160, "max " + max + " mean " + mean.ToString("0.00") + " over8 " + over + "/" + n
                    + " hot " + hot + " px " + minX + "," + minY + "-" + maxX + "," + maxY);
                Assert.Less(over * 100 / (n == 0 ? 1 : n), 4, "over8 fraction");
            }
            finally
            {
                RenderOpts.Clear();
                if (full != null) full.Dispose();
                if (clip != null) clip.Dispose();
            }
        }

        static HomeChrome PaintClip(bool clip)
        {
            RenderOpts.Clear();
            if (clip) RenderOpts.DirtyClip = true;
            var chrome = HomeChrome.Load("home_default", "zero");
            chrome.ResizeRaster(1);
            chrome.Draw(2000);
            chrome.Draw(2040);
            return chrome;
        }

        [Test]
        public void DirtyClip_NamesTheShineAndThenUsesHoles()
        {
            HomeChrome page = null;
            try
            {
                RenderOpts.Clear();
                RenderStats.DirtyWide = "";
                int area0 = RenderStats.DirtyFullArea;
                page = HomeChrome.Load("home_default", "zero");
                page.ResizeRaster(1);
                RenderOpts.Dirty = true;
                page.Draw(1200);
                page.Draw(1600);
                Assert.That(RenderStats.DirtyWide, Does.Contain("play-btn-glow"));
                Assert.Greater(RenderStats.DirtyFullArea, area0);
                int area1 = RenderStats.DirtyFullArea;
                int frames = RenderStats.DirtyFrames;
                RenderOpts.DirtyClip = true;
                page.Draw(2000);
                page.Draw(2400);
                Assert.Greater(RenderStats.DirtyFrames, frames);
                Assert.AreEqual(area1, RenderStats.DirtyFullArea);
            }
            finally
            {
                RenderOpts.Clear();
                if (page != null) page.Dispose();
            }
        }

        static HomeChrome Paint(bool dirty)
        {
            RenderOpts.Clear();
            if (dirty) RenderOpts.Dirty = true;
            var chrome = HomeChrome.Load("home_default", "zero");
            chrome.ResizeRaster(1);
            chrome.Draw(1200);
            chrome.Draw(1600);
            return chrome;
        }

        static int Diff(Color32 a, Color32 b)
        {
            int d = MathAbs(a.r - b.r);
            int g = MathAbs(a.g - b.g);
            int bl = MathAbs(a.b - b.b);
            if (g > d) d = g;
            if (bl > d) d = bl;
            return d;
        }

        static int MathAbs(int v) { return v < 0 ? -v : v; }

        static Color32[] Read(HomeChrome chrome)
        {
            var rt = chrome.Target;
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            var px = tex.GetPixels32();
            RenderTexture.active = prev;
            Object.DestroyImmediate(tex);
            return px;
        }
    }
}
