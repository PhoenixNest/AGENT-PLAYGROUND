using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    public class Canvas2DGpuTests
    {
        const int Size = 16;

        static Color32[] Render(System.Action<Canvas2D> draw)
        {
            using (var c = new Canvas2D(Size, Size))
            {
                c.BeginFrame();
                draw(c);
                c.EndFrame();
                var prev = RenderTexture.active;
                RenderTexture.active = c.Target;
                var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false, true);
                tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0, false);
                RenderTexture.active = prev;
                var px = tex.GetPixels32();
                Object.DestroyImmediate(tex);
                return px;
            }
        }

        static Color32 At(Color32[] px, int x, int yTop) => px[(Size - 1 - yTop) * Size + x];

        [Test]
        public void FrameStartsTransparent()
        {
            var px = Render(c => { });
            Assert.AreEqual(0, At(px, 3, 3).a);
        }

        [Test]
        public void OpaqueRectFillsExactPixels()
        {
            var px = Render(c => { c.FillStyle = "#ff0000"; c.FillRect(4, 4, 4, 4); });
            Assert.AreEqual(new Color32(255, 0, 0, 255), At(px, 4, 4));
            Assert.AreEqual(new Color32(255, 0, 0, 255), At(px, 7, 7));
            Assert.AreEqual(0, At(px, 8, 8).a);
            Assert.AreEqual(0, At(px, 3, 4).a);
        }

        [Test]
        public void HalfPixelRectGivesHalfCoverage()
        {
            var px = Render(c => { c.FillStyle = "#ffffff"; c.FillRect(4.5, 4, 3, 3); });
            Assert.AreEqual(128, At(px, 4, 5).a, 1);
            Assert.AreEqual(255, At(px, 5, 5).a);
        }

        [Test]
        public void GlobalAlphaScalesSourceOver()
        {
            var px = Render(c =>
            {
                c.FillStyle = "#000000"; c.FillRect(0, 0, Size, Size);
                c.GlobalAlpha = 0.5; c.FillStyle = "#ffffff"; c.FillRect(0, 0, Size, Size);
            });
            var p = At(px, 8, 8);
            Assert.AreEqual(255, p.a);
            Assert.AreEqual(128, p.r, 1);
        }

        [Test]
        public void SaveRestoreRevertsTransform()
        {
            var px = Render(c =>
            {
                c.FillStyle = "#00ff00";
                c.Save(); c.Translate(8, 8); c.FillRect(0, 0, 2, 2); c.Restore();
                c.FillRect(0, 0, 2, 2);
            });
            Assert.AreEqual(255, At(px, 8, 8).a);
            Assert.AreEqual(255, At(px, 0, 0).a);
        }
    }
}
