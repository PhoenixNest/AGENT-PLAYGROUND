using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class PageFitTests
    {
        [Test]
        public void PortraitPages_CoverTheScreen_AndIgnoreSafeArea()
        {
            int[][] screens =
            {
                new[] { 1170, 2532 },
                new[] { 1290, 2796 },
                new[] { 1179, 2556 }
            };
            for (int i = 0; i < screens.Length; i++)
            {
                int w = screens[i][0];
                int h = screens[i][1];
                var notch = Notch(w, h);
                var page = ParityViewer.PageRectFor(w, h, notch);
                var bare = ParityViewer.PageRectFor(w, h, null);
                Assert.AreEqual(bare.x, page.x, 0.02f, w + "x" + h + " safe area moved x");
                Assert.AreEqual(bare.y, page.y, 0.02f, w + "x" + h + " safe area moved y");
                Assert.AreEqual(bare.width, page.width, 0.02f, w + "x" + h + " safe area changed width");
                Assert.AreEqual(bare.height, page.height, 0.02f, w + "x" + h + " safe area changed height");
                AssertCover(w, h, page);
            }
        }

        [Test]
        public void LandscapeAndIpad_KeepAspectInsideTheScreen()
        {
            var landscapeSafe = new Rect(120f, 0f, 2292f, 1170f);
            AssertCover(2532, 1170, ParityViewer.PageRectFor(2532, 1170, landscapeSafe));
            Assert.AreEqual(
                ParityViewer.PageRectFor(2532, 1170, null).width,
                ParityViewer.PageRectFor(2532, 1170, landscapeSafe).width,
                0.02f);

            var ipadSafe = new Rect(0f, 40f, 2048f, 2652f);
            AssertCover(2048, 2732, ParityViewer.PageRectFor(2048, 2732, ipadSafe));
            Assert.AreEqual(
                ParityViewer.PageRectFor(2048, 2732, null).height,
                ParityViewer.PageRectFor(2048, 2732, ipadSafe).height,
                0.02f);

            var tiny = ParityViewer.PageRectFor(0, 0, null);
            Assert.Greater(tiny.width, 0f);
            Assert.Greater(tiny.height, 0f);
            var fallen = ParityViewer.PageRectFor(0, 0, new Rect(10f, 20f, 400f, 800f));
            Assert.GreaterOrEqual(fallen.x, 10f - 0.05f);
            Assert.LessOrEqual(fallen.xMax, 410f + 0.05f);
        }

        [Test]
        public void CssToScreen_TryCss_RoundTrips()
        {
            var box = ParityViewer.PageRectFor(1170, 2532, Notch(1170, 2532));
            double[] xs = { 0.5, 16, 215, 429.5 };
            double[] ys = { 0.5, 14, 64, 466, 864, 931.5 };
            for (int i = 0; i < xs.Length; i++)
            {
                for (int j = 0; j < ys.Length; j++)
                {
                    var screen = ParityViewer.CssToScreen(box, xs[i], ys[j]);
                    Assert.IsTrue(ParityViewer.TryCss(box, screen, out double backX, out double backY));
                    Assert.AreEqual(xs[i], backX, 0.05);
                    Assert.AreEqual(ys[j], backY, 0.05);
                }
            }

            if (box.x >= 2f)
                Assert.IsFalse(ParityViewer.TryCss(box, new Vector2(box.x - 2f, box.center.y), out _, out _));
            else
                Assert.IsFalse(ParityViewer.TryCss(box, new Vector2(-1f, box.center.y), out _, out _));

            var wide = ParityViewer.PageRectFor(2532, 1170, new Rect(120f, 0f, 2292f, 1170f));
            Assert.Greater(wide.x, 2f);
            Assert.IsFalse(ParityViewer.TryCss(wide, new Vector2(1f, wide.center.y), out _, out _));
            var mid = ParityViewer.CssToScreen(wide, 215, 466);
            Assert.IsTrue(ParityViewer.TryCss(wide, mid, out double cssX, out double cssY));
            Assert.AreEqual(215, cssX, 0.05);
            Assert.AreEqual(466, cssY, 0.05);
        }

        [Test]
        public void MarginColour_IsPageBackground()
        {
            Color32 c = ParityViewer.PageFill;
            Assert.AreEqual(30, c.r);
            Assert.AreEqual(27, c.g);
            Assert.AreEqual(75, c.b);
            Assert.AreEqual(255, c.a);
        }

        static Rect Notch(int w, int h)
        {
            float scale = h / 932f;
            float top = 59f * scale;
            float bottom = 34f * scale;
            return new Rect(0f, bottom, w, Mathf.Max(8f, h - top - bottom));
        }

        static void AssertCover(int w, int h, Rect page)
        {
            Assert.GreaterOrEqual(page.x, -0.05f);
            Assert.GreaterOrEqual(page.y, -0.05f);
            Assert.LessOrEqual(page.xMax, w + 0.05f);
            Assert.LessOrEqual(page.yMax, h + 0.05f);
            float left = page.x;
            float right = w - page.xMax;
            float bottom = page.y;
            float top = h - page.yMax;
            Assert.AreEqual(0f, left - right, 0.05f, w + "x" + h + " horizontal margins");
            Assert.AreEqual(0f, top - bottom, 0.05f, w + "x" + h + " vertical margins");
            Assert.AreEqual((float)w, left + page.width + right, 0.05f);
            Assert.AreEqual((float)h, bottom + page.height + top, 0.05f);
            float aspect = ParityPages.CssWidth / (float)ParityPages.CssHeight;
            Assert.AreEqual(aspect, page.width / page.height, 0.0005f);
            bool flushW = left < 0.05f && right < 0.05f;
            bool flushH = top < 0.05f && bottom < 0.05f;
            Assert.IsTrue(flushW || flushH, w + "x" + h + " does not meet an edge pair " + page);
            TestContext.WriteLine(
                w + "x" + h
                + " page=(" + page.x.ToString("F3") + "," + page.y.ToString("F3") + ","
                + page.width.ToString("F3") + "," + page.height.ToString("F3") + ")"
                + " L=" + left.ToString("F3") + " R=" + right.ToString("F3")
                + " T=" + top.ToString("F3") + " B=" + bottom.ToString("F3"));
        }
    }
}
