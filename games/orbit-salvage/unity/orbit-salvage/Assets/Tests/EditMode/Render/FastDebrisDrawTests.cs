using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using UnityEngine;
using UnityEngine.Rendering;

namespace OrbitalSalvage.Render.Tests
{
    /// <summary>
    /// Phone-path draw shortcuts: deferred circle points, and putting the debris
    /// transform back without cloning canvas state. Editor points stay the old polyline.
    /// </summary>
    public class FastDebrisDrawTests
    {
        bool _fastShape, _fastArc;

        [SetUp]
        public void SetUp()
        {
            _fastShape = RenderOpts.FastShape;
            _fastArc = RenderOpts.FastArc;
        }

        [TearDown]
        public void TearDown()
        {
            RenderOpts.FastShape = _fastShape;
            RenderOpts.FastArc = _fastArc;
        }

        [Test]
        public void DeferredCircle_MaterializeMatchesTheImmediatePolyline()
        {
            RenderOpts.FastShape = false;
            RenderOpts.FastArc = false;
            var slow = Points(40);

            RenderOpts.FastShape = true;
            RenderOpts.FastArc = true;
            var fast = new PathData();
            fast.Arc(Matrix2D.Identity, 50, 50, 40, 0, System.Math.PI * 2, false);
            var ready = fast[0].pts;
            Assert.AreEqual(slow.Count, ready.Count, "point count");
            for (int i = 0; i < slow.Count; i++)
            {
                Assert.AreEqual(slow[i].x, ready[i].x, 1e-9, "x " + i);
                Assert.AreEqual(slow[i].y, ready[i].y, 1e-9, "y " + i);
            }
        }

        [Test]
        public void FastCircleFill_DoesNotBuildEdges()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                Assert.Ignore("no graphics device");
            RenderOpts.FastShape = true;
            RenderOpts.FastArc = true;
            using (var c = new Canvas2D(64, 64))
            {
                var before = RenderStats.Read();
                c.BeginFrame();
                c.FillStyle = "#facc15";
                c.BeginPath();
                c.Arc(32, 32, 40, 0, JsMath.PI * 2);
                c.Fill();
                c.EndFrame();
                var d = RenderStats.Delta(RenderStats.Read(), before);
                Assert.Greater(d.FastCirc, 0);
                Assert.AreEqual(0, d.Edges, "analytic circle still flattened into edges");
                Assert.AreEqual(0, d.ShapeOther);
            }
        }

        [Test]
        public void PhoneTransformRestore_MatchesSaveRestore()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                Assert.Ignore("no graphics device");
            RenderOpts.FastShape = true;
            RenderOpts.FastArc = true;
            var saved = DrawPair(true);
            var plain = DrawPair(false);
            int max = 0;
            int differ = 0;
            Assert.AreEqual(saved.Length, plain.Length);
            for (int i = 0; i < saved.Length; i++)
            {
                int ch = Mathf.Max(
                    Mathf.Abs(saved[i].r - plain[i].r),
                    Mathf.Abs(saved[i].g - plain[i].g),
                    Mathf.Abs(saved[i].b - plain[i].b),
                    Mathf.Abs(saved[i].a - plain[i].a));
                if (ch > 0) differ++;
                if (ch > max) max = ch;
            }
            Assert.AreEqual(0, differ, "pixels " + differ + " max " + max);
        }

        static System.Collections.Generic.List<Vec2d> Points(double radius)
        {
            var path = new PathData();
            path.Arc(Matrix2D.Identity, 50, 50, radius, 0, System.Math.PI * 2, false);
            return path[0].pts;
        }

        static Color32[] DrawPair(bool saveRestore)
        {
            using (var c = new Canvas2D(96, 64))
            {
                c.BeginFrame();
                c.FillStyle = "#facc15";
                c.Save();
                Matrix2D keep = c.CurrentTransform;
                if (saveRestore) c.Save();
                c.Translate(30, 32);
                c.BeginPath();
                c.Arc(0, 0, 10, 0, JsMath.PI * 2);
                c.Fill();
                if (saveRestore) c.Restore();
                else c.SetTransform(keep.a, keep.b, keep.c, keep.d, keep.e, keep.f);
                c.Translate(66, 32);
                c.BeginPath();
                c.Arc(0, 0, 10, 0, JsMath.PI * 2);
                c.Fill();
                c.Restore();
                c.EndFrame();
                var prev = RenderTexture.active;
                RenderTexture.active = c.Target;
                var tex = new Texture2D(c.Width, c.Height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, c.Width, c.Height), 0, 0);
                tex.Apply(false);
                RenderTexture.active = prev;
                var px = tex.GetPixels32();
                Object.DestroyImmediate(tex);
                return px;
            }
        }
    }
}
