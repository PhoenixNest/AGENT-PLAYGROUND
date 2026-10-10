using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
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

        [Test]
        public void SkipSubmitOff_EndFrameStillExecutes()
        {
            using (var c = new Canvas2D(Size, Size))
            {
                Assert.IsFalse(c.SkipSubmit);
                c.BeginFrame();
                c.FillStyle = "#00ff00";
                c.FillRect(0, 0, 4, 4);
                PaintProfile.Enabled = true;
                PaintProfile.Reset();
                try
                {
                    c.EndFrame();
                    Assert.AreEqual(1, c.ExecuteCount);
                    Assert.AreEqual(1, PaintProfile.Flushes);
                }
                finally
                {
                    PaintProfile.Enabled = false;
                    PaintProfile.Reset();
                }
                var prev = RenderTexture.active;
                RenderTexture.active = c.Target;
                var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false, true);
                tex.ReadPixels(new Rect(0, 0, Size, Size), 0, 0, false);
                RenderTexture.active = prev;
                var px = tex.GetPixels32();
                Object.DestroyImmediate(tex);
                Assert.AreEqual(255, At(px, 1, 1).a);
            }
        }

        [Test]
        public void SkipSubmit_EndFrameDoesNotExecuteAndReusesCpuState()
        {
            using (var c = new Canvas2D(Size, Size))
            {
                c.SkipSubmit = true;
                PaintProfile.Enabled = true;
                PaintProfile.Reset();
                int edges = 0, prm = 0, glyphs = 0;
                try
                {
                    for (int i = 0; i < 20; i++)
                    {
                        c.BeginFrame();
                        Assert.AreEqual(0, c.EdgeCount);
                        Assert.AreEqual(0, c.DrawCount);
                        Assert.AreEqual(0, IntField(c, "glyphRecords"));
                        Assert.AreEqual(0, ListCount(c, "snapshots"));
                        c.FillStyle = "#ff0000";
                        c.FillRect(1, 1, 10, 10);
                        c.EndFrame();
                        Assert.AreEqual(0, c.ExecuteCount);
                        Assert.AreEqual(0, PaintProfile.Flushes);
                        if (i == 0)
                        {
                            edges = ArrayLen(c, "edges");
                            prm = ArrayLen(c, "prm");
                            glyphs = ArrayLen(c, "glyphData");
                        }
                        else
                        {
                            Assert.AreEqual(edges, ArrayLen(c, "edges"));
                            Assert.AreEqual(prm, ArrayLen(c, "prm"));
                            Assert.AreEqual(glyphs, ArrayLen(c, "glyphData"));
                        }
                    }
                }
                finally
                {
                    PaintProfile.Enabled = false;
                    PaintProfile.Reset();
                }
                Assert.Throws<System.InvalidOperationException>(() => c.Flush());
                Assert.Throws<System.InvalidOperationException>(() => c.Replay());
                Assert.AreEqual(0, c.ExecuteCount);
            }
        }

        [Test]
        public void SkipSubmit_RenderedFramesKeepTheSameRngStream()
        {
            const int frames = 24;
            long submitted = RenderFrames(true, frames, out int executesOn, out int flushesOn);
            long skipped = RenderFrames(false, frames, out int executesOff, out int flushesOff);
            Assert.AreEqual(frames, executesOn);
            Assert.AreEqual(frames, flushesOn);
            Assert.AreEqual(0, executesOff);
            Assert.AreEqual(0, flushesOff);
            Assert.AreEqual(submitted, skipped);
            Assert.Greater(submitted, 0);
        }

        static long RenderFrames(bool submit, int frames, out int executes, out int flushes)
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var renderer = new GameRenderer(48, 48, 1);
            PaintProfile.Enabled = true;
            PaintProfile.Reset();
            try
            {
                renderer.SubmitEnabled = submit;
                var economy = new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0));
                var session = new LiveGameSession(new PhysicsEngine(new ParityAudioCues(() => 0)), renderer, economy);
                session.LoadSector(MazeLevelBuilder.GetSectors()[0]);
                for (int i = 0; i < frames; i++) session.StepWebFrame(0.016, true);
                executes = renderer.Canvas.ExecuteCount;
                flushes = PaintProfile.Flushes;
                return JsRandom.Global.CallCount;
            }
            finally
            {
                PaintProfile.Enabled = false;
                PaintProfile.Reset();
                renderer.Dispose();
            }
        }

        static int ArrayLen(Canvas2D canvas, string field)
        {
            var value = typeof(Canvas2D).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(canvas);
            return ((System.Array)value).Length;
        }

        static int ListCount(Canvas2D canvas, string field)
        {
            var value = typeof(Canvas2D).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(canvas);
            return ((System.Collections.ICollection)value).Count;
        }

        static int IntField(Canvas2D canvas, string field)
        {
            return (int)typeof(Canvas2D).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(canvas);
        }

        static Color32[] RenderAt(int size, System.Action<Canvas2D> draw)
        {
            using (var c = new Canvas2D(size, size))
            {
                c.BeginFrame();
                draw(c);
                c.EndFrame();
                var prev = RenderTexture.active;
                RenderTexture.active = c.Target;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
                tex.ReadPixels(new Rect(0, 0, size, size), 0, 0, false);
                RenderTexture.active = prev;
                var px = tex.GetPixels32();
                Object.DestroyImmediate(tex);
                return px;
            }
        }

        static void Shadow(Canvas2D c)
        {
            c.ShadowColor = "rgba(0,0,0,0.65)";
            c.ShadowBlur = 6;
            c.ShadowOffsetX = 3;
            c.ShadowOffsetY = 2;
            c.FillStyle = "#33aaee";
            c.FillRect(16, 18, 20, 14);
        }

        static int MaxDiff(Color32[] a, Color32[] b)
        {
            int max = 0;
            for (int i = 0; i < a.Length; i++)
            {
                max = System.Math.Max(max, System.Math.Abs(a[i].r - b[i].r));
                max = System.Math.Max(max, System.Math.Abs(a[i].g - b[i].g));
                max = System.Math.Max(max, System.Math.Abs(a[i].b - b[i].b));
                max = System.Math.Max(max, System.Math.Abs(a[i].a - b[i].a));
            }
            return max;
        }

        [Test]
        public void ShadowScope_StaysNearTheFullTargetShadow()
        {
            RenderOpts.Clear();
            try
            {
                var off = RenderAt(64, Shadow);
                RenderOpts.ShadowScope = true;
                var on = RenderAt(64, Shadow);
                int diff = MaxDiff(off, on);
                Assert.LessOrEqual(diff, 8, "max channel diff " + diff);
            }
            finally { RenderOpts.Clear(); }
        }

        [Test]
        public void ShadowLow_ReusesTheBitmapAndStaysSoft()
        {
            RenderOpts.Clear();
            try
            {
                RenderOpts.ShadowLow = true;
                var a = RenderAt(64, Shadow);
                var b = RenderAt(64, c =>
                {
                    Shadow(c);
                    c.ShadowColor = "rgba(0,0,0,0.65)";
                    c.ShadowBlur = 6;
                    c.ShadowOffsetX = 3;
                    c.ShadowOffsetY = 2;
                    c.FillStyle = "#33aaee";
                    c.FillRect(40, 18, 20, 14);
                });
                Assert.Greater(a[30 * 64 + 20].a, 0, "shadow has coverage");
                Assert.Greater(b[30 * 64 + 44].a, 0, "second card reused a shadow");
            }
            finally { RenderOpts.Clear(); }
        }

        [Test]
        public void BackdropScope_MatchesTheGpuBlur()
        {
            RenderOpts.Clear();
            try
            {
                System.Action<Canvas2D> draw = c =>
                {
                    c.ExactBackdrop = false;
                    c.FillStyle = "#2040c0";
                    c.FillRect(0, 0, 48, 48);
                    c.FillStyle = "#e0c040";
                    c.FillRect(4, 30, 40, 8);
                    c.BackdropBlur(10, 12, 28, 18, 3);
                };
                var off = RenderAt(48, draw);
                RenderOpts.BackdropScope = true;
                var on = RenderAt(48, draw);
                int diff = MaxDiff(off, on);
                Assert.LessOrEqual(diff, 2, "backdrop scope max diff " + diff);
            }
            finally { RenderOpts.Clear(); }
        }

        [Test]
        public void KeepTarget_ClearsOnlyTheDirtyRect()
        {
            using (var c = new Canvas2D(32, 32))
            {
                c.BeginFrame();
                c.FillStyle = "#ff0000";
                c.FillRect(0, 0, 32, 32);
                c.EndFrame();
                c.KeepTarget = true;
                c.DirtyX = 8;
                c.DirtyY = 8;
                c.DirtyW = 8;
                c.DirtyH = 8;
                c.BeginFrame();
                c.EndFrame();
                var prev = RenderTexture.active;
                RenderTexture.active = c.Target;
                var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false, true);
                tex.ReadPixels(new Rect(0, 0, 32, 32), 0, 0, false);
                RenderTexture.active = prev;
                var px = tex.GetPixels32();
                Object.DestroyImmediate(tex);
                Color32 At(int x, int yTop) => px[(32 - 1 - yTop) * 32 + x];
                Assert.AreEqual(255, At(1, 1).r, "outside the dirty rect");
                Assert.AreEqual(0, At(10, 10).a, "inside the dirty rect");
            }
        }

        static void DrawCard(Canvas2D c)
        {
            var s = new CssBoxStyle();
            s.x = 24;
            s.y = 20;
            s.w = 40;
            s.h = 28;
            for (int i = 0; i < 4; i++) s.radii[i] = new Vec2d(8, 8);
            s.hasBg = true;
            s.bg = new Rgba(0.2f, 0.45f, 0.9f, 1f);
            s.shadows.Add(new CssShadow { dx = 0, dy = 3, blur = 8, spread = 0, color = new Rgba(0f, 0f, 0f, 0.55f) });
            CssBox.Draw(c, s, 1);
        }

        [Test]
        public void RoundShadow_OffMatchesItselfAndOnStaysSoft()
        {
            try
            {
                RenderOpts.Clear();
                var a = RenderAt(96, DrawCard);
                var b = RenderAt(96, DrawCard);
                Assert.AreEqual(0, MaxDiff(a, b), "options off drifted");
                int before = RenderStats.RoundShadow;
                RenderOpts.RoundShadow = true;
                var on = RenderAt(96, DrawCard);
                int diff = MaxDiff(a, on);
                Assert.Greater(RenderStats.RoundShadow, before, "analytic path did not run");
                Assert.LessOrEqual(diff, 40, "round shadow max diff " + diff);
            }
            finally { RenderOpts.Clear(); }
        }

        [Test]
        public void FastCircle_OffIsStableAndOnStaysFilled()
        {
            try
            {
                RenderOpts.Clear();
                var a = RenderAt(96, DrawCirc);
                var b = RenderAt(96, DrawCirc);
                Assert.AreEqual(0, MaxDiff(a, b), "options off drifted");
                int before = RenderStats.FastCirc;
                RenderOpts.FastShape = true;
                var on = RenderAt(96, DrawCirc);
                Assert.Greater(RenderStats.FastCirc, before);
                Assert.LessOrEqual(MaxDiff(a, on), 48, "circle max diff");
                Assert.Greater(on[(96 - 1 - 48) * 96 + 48].a, 200);
            }
            finally { RenderOpts.Clear(); }
        }

        [Test]
        public void RoundPlus_CountsRotatedRejectsUntilEnabled()
        {
            try
            {
                RenderOpts.Clear();
                RenderOpts.RoundShadow = true;
                int miss = RenderStats.RoundMissXform;
                int hit = RenderStats.RoundShadow;
                RenderAt(96, DrawRotatedCard);
                Assert.Greater(RenderStats.RoundMissXform, miss, "rotated shadow was not rejected");
                RenderOpts.RoundPlus = true;
                int miss2 = RenderStats.RoundMissXform;
                RenderAt(96, DrawRotatedCard);
                Assert.AreEqual(miss2, RenderStats.RoundMissXform);
                Assert.Greater(RenderStats.RoundShadow, hit);
            }
            finally { RenderOpts.Clear(); }
        }

        static void DrawCirc(Canvas2D c)
        {
            c.SetFill(new Rgba(0.9f, 0.2f, 0.1f, 1f));
            c.BeginPath();
            c.Arc(48, 48, 18, 0, System.Math.PI * 2);
            c.Fill();
        }

        static void DrawRotatedCard(Canvas2D c)
        {
            c.Translate(48, 48);
            c.Rotate(0.45);
            var s = new CssBoxStyle();
            s.x = -16;
            s.y = -12;
            s.w = 32;
            s.h = 22;
            for (int i = 0; i < 4; i++) s.radii[i] = new Vec2d(6, 6);
            s.hasBg = true;
            s.bg = new Rgba(0.2f, 0.45f, 0.9f, 1f);
            s.shadows.Add(new CssShadow { dx = 0, dy = 2, blur = 6, spread = 0, color = new Rgba(0f, 0f, 0f, 0.6f) });
            CssBox.Draw(c, s, 1);
        }
    }
}
