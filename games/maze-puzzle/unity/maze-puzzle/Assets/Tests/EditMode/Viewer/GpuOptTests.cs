using NUnit.Framework;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class GpuOptTests
    {
        [TearDown]
        public void Clean()
        {
            GpuOpt.Apply("");
            RenderOpts.Clear();
            PlayfieldComposite.ReleaseGpu();
        }

        [Test]
        public void EmptyStringLeavesEveryOptionOff()
        {
            GpuOpt.Apply("shadowscope,dirty");
            GpuOpt.Apply("");
            Assert.IsFalse(GpuOpt.ShadowScope);
            Assert.IsFalse(RenderOpts.Dirty);
            Assert.AreEqual("none", GpuOpt.Active);
            Assert.AreEqual("", GpuOpt.Defaults);
        }

        [Test]
        public void ParsesEachOptionIndependently()
        {
            GpuOpt.Apply(" shadowlow, backdropscope ,backdroplow,gameone, ambhz=15 ,unknown");
            Assert.IsTrue(GpuOpt.ShadowLow);
            Assert.IsFalse(GpuOpt.ShadowScope);
            Assert.IsTrue(GpuOpt.BackdropScope);
            Assert.IsTrue(GpuOpt.BackdropLow);
            Assert.IsTrue(GpuOpt.GameOne);
            Assert.IsFalse(GpuOpt.Dirty);
            Assert.AreEqual(15, GpuOpt.AmbientHz);
            Assert.AreEqual(15, RenderOpts.AmbientHz);
            Assert.IsTrue(RenderOpts.ShadowLow);
            Assert.IsFalse(RenderOpts.ShadowScope);
        }

        [Test]
        public void ParsesRoundShadowAndPlayHalf()
        {
            GpuOpt.Apply("rrshadow,playhalf");
            Assert.IsTrue(GpuOpt.RoundShadow);
            Assert.IsTrue(GpuOpt.PlayHalf);
            Assert.IsTrue(RenderOpts.RoundShadow);
            Assert.IsTrue(RenderOpts.PlayHalf);
            Assert.IsFalse(RenderOpts.ShadowLow);
            GpuOpt.Apply("");
            Assert.IsFalse(RenderOpts.RoundShadow);
            Assert.IsFalse(RenderOpts.PlayHalf);
        }

        [Test]
        public void ParsesFollowUpTokens()
        {
            GpuOpt.Apply("dirtyclip,bdhold,rrplus,fastshape,noprewarm");
            Assert.IsTrue(RenderOpts.DirtyClip);
            Assert.IsTrue(RenderOpts.BackdropHold);
            Assert.IsTrue(RenderOpts.RoundPlus);
            Assert.IsTrue(RenderOpts.FastShape);
            Assert.IsTrue(RenderOpts.NoPrewarm);
            Assert.IsFalse(RenderOpts.Dirty);
            Assert.IsFalse(RenderOpts.RoundShadow);
            GpuOpt.Apply("");
            Assert.IsFalse(RenderOpts.DirtyClip);
            Assert.IsFalse(RenderOpts.FastShape);
            Assert.IsFalse(RenderOpts.NoPrewarm);
        }

        [Test]
        public void AmbhzIgnoresOutOfRange()
        {
            GpuOpt.Apply("ambhz=0,ambhz=90,ambhz=30");
            Assert.AreEqual(30, GpuOpt.AmbientHz);
        }

        [Test]
        public void BlitGpu_PremultipliesOverOnce()
        {
            var under = new RenderTexture(4, 4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var over = new RenderTexture(4, 4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            under.Create();
            over.Create();
            try
            {
                Fill(under, new Color(1f, 0f, 0f, 1f));
                Fill(over, new Color(0f, 0f, 0.5f, 0.5f));
                var gpu = PlayfieldComposite.BlitGpu(under, over);
                Assert.IsNotNull(gpu);
                var px = Read(gpu, 1, 1);
                Assert.Greater(px.r, 80, "under red remains " + px);
                Assert.Greater(px.b, 80, "over blue composites " + px);
                Assert.Less(px.g, 30, "no green " + px);
            }
            finally
            {
                under.Release();
                over.Release();
                Object.DestroyImmediate(under);
                Object.DestroyImmediate(over);
            }
        }

        static void Fill(RenderTexture rt, Color color)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(false, true, color);
            RenderTexture.active = prev;
        }

        static Color32 Read(RenderTexture rt, int x, int y)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            var c = tex.GetPixel(x, y);
            RenderTexture.active = prev;
            Object.DestroyImmediate(tex);
            return new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)(c.a * 255));
        }
    }
}
