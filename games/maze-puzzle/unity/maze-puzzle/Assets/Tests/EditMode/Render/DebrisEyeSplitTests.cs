using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OrbitalSalvage.Render.Tests
{
    /// <summary>
    /// Phone opts (FastShape + FastArc) draw each ordinary-debris eye as its own circle.
    /// The editor path (FastShape off) keeps both eyes on one path. The compound sequence
    /// is redrawn here because GameRenderer no longer emits it while FastShape is on.
    /// </summary>
    public class DebrisEyeSplitTests
    {
        const int PieceCount = 100;
        const int PatchCount = 9;
        const int EyeCanvas = 176;
        const int PatchCanvas = 256;
        const double PatchScale = 3;

        // Metal patch, 9 pieces at scale 3: max per-channel delta 55, 504 of 65536 pixels.
        const int PatchMaxChannel = 55;
        const int PatchDifferPixels = 504;

        bool _fastShape, _fastArc, _playBatch, _playHalf;

        [SetUp]
        public void SetUp()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                Assert.Ignore("no graphics device; debris eye tests need a real GPU");
            _fastShape = RenderOpts.FastShape;
            _fastArc = RenderOpts.FastArc;
            _playBatch = RenderOpts.PlayBatch;
            _playHalf = RenderOpts.PlayHalf;
        }

        [TearDown]
        public void TearDown()
        {
            RenderOpts.FastShape = _fastShape;
            RenderOpts.FastArc = _fastArc;
            RenderOpts.PlayBatch = _playBatch;
            RenderOpts.PlayHalf = _playHalf;
        }

        [Test]
        public void FastShape_SplitEyesCostLessThanCompound()
        {
            PhoneEyes();
            var split = MeasureEyes(PieceCount, true);
            var compound = MeasureEyes(PieceCount, false);
            Debug.Log("[DebrisEye] eyes split other=" + split.ShapeOther + " circ=" + split.ShapeCirc
                + " fastCirc=" + split.FastCirc + " draws=" + split.Draws + " commands=" + split.Commands
                + " compound other=" + compound.ShapeOther + " circ=" + compound.ShapeCirc
                + " fastCirc=" + compound.FastCirc + " draws=" + compound.Draws + " commands=" + compound.Commands);

            Assert.AreEqual(0, split.ShapeOther, "split eyes ShapeOther");
            Assert.AreEqual(2 * PieceCount, split.FastCirc, "split eyes FastCirc");
            Assert.AreEqual(2 * PieceCount, split.Draws, "split eyes draws");
            Assert.AreEqual(2 * PieceCount, split.Commands, "split eyes commands");
            Assert.AreEqual(0, compound.FastCirc, "compound eyes FastCirc");
            Assert.AreEqual(PieceCount, compound.ShapeOther, "compound eyes ShapeOther");
            Assert.AreEqual(3 * PieceCount, compound.Draws, "compound eyes draws");
            Assert.AreEqual(3 * PieceCount, compound.Commands, "compound eyes commands");

            var extra = DebrisFrameExtra(PieceCount);
            int debrisDraws = extra.SecDraw[6];
            int compoundFrameDraws = debrisDraws - split.Draws + compound.Draws;
            Debug.Log("[DebrisEye] frame extra other=" + extra.ShapeOther + " fastCirc=" + extra.FastCirc
                + " fastArc=" + extra.FastArc + " fastLine=" + extra.FastLine
                + " draws=" + extra.Draws + " commands=" + extra.Commands
                + " secDraw6=" + debrisDraws + " compoundFrameDraws=" + compoundFrameDraws);

            Assert.AreEqual(3 * PieceCount, extra.FastCirc, "playfield FastCirc");
            Assert.AreEqual(0, extra.ShapeOther, "playfield ShapeOther");
            Assert.AreEqual(5 * PieceCount, debrisDraws, "playfield debris draws");
            Assert.AreEqual(6 * PieceCount, compoundFrameDraws, "compound eyes would add one accum draw per piece");
        }

        [Test]
        public void FastShape_SplitAndCompoundPatchStayClose()
        {
            PhoneEyes();
            var pieces = PatchPieces();
            var splitPx = DrawPatch(pieces, true, out RenderStats.Snap split);
            var compoundPx = DrawPatch(pieces, false, out RenderStats.Snap compound);
            Diff(splitPx, compoundPx, out int maxChannel, out int differ);
            Debug.Log("[DebrisEye] patch maxChannel=" + maxChannel + " differ=" + differ
                + "/" + splitPx.Length
                + " realSplit fastCirc=" + split.FastCirc + " other=" + split.ShapeOther
                + " draws=" + split.Draws
                + " compound fastCirc=" + compound.FastCirc + " other=" + compound.ShapeOther
                + " draws=" + compound.Draws);

            Assert.AreEqual(splitPx.Length, compoundPx.Length);
            Assert.AreEqual(3 * PatchCount, split.FastCirc, "real debris path analytic circles");
            Assert.AreEqual(0, split.ShapeOther, "real split patch ShapeOther");
            Assert.AreEqual(5 * PatchCount, split.Draws, "real split patch draws");
            Assert.AreEqual(PatchCount, compound.FastCirc, "compound patch keeps the body circle analytic");
            Assert.AreEqual(PatchCount, compound.ShapeOther, "compound patch ShapeOther");
            Assert.AreEqual(6 * PatchCount, compound.Draws, "compound patch draws");
            Assert.LessOrEqual(maxChannel, PatchMaxChannel, "max channel " + maxChannel + " differ " + differ);
            Assert.LessOrEqual(differ, PatchDifferPixels, "differing pixels " + differ + " max " + maxChannel);
        }

        [Test]
        public void FastShapeOff_OrdinaryDebrisUsesCompoundEyes()
        {
            RenderOpts.FastShape = false;
            RenderOpts.FastArc = false;
            RenderOpts.PlayBatch = false;
            RenderOpts.PlayHalf = false;
            var extra = DebrisFrameExtra(PieceCount);
            Debug.Log("[DebrisEye] editor extra other=" + extra.ShapeOther + " circ=" + extra.ShapeCirc
                + " fastCirc=" + extra.FastCirc + " draws=" + extra.Draws
                + " secDraw6=" + extra.SecDraw[6]);
            Assert.AreEqual(PieceCount, extra.ShapeOther,
                "ShapeOther " + extra.ShapeOther + " for " + PieceCount + " ordinary pieces");
        }

        static void PhoneEyes()
        {
            RenderOpts.FastShape = true;
            RenderOpts.FastArc = true;
            RenderOpts.PlayBatch = false;
            RenderOpts.PlayHalf = false;
        }

        static RenderStats.Snap MeasureEyes(int n, bool split)
        {
            using (var c = new Canvas2D(EyeCanvas, EyeCanvas))
            {
                var before = RenderStats.Read();
                c.BeginFrame();
                c.FillStyle = "#1e1b4b";
                for (int i = 0; i < n; i++)
                {
                    double x = 16 + (i % 10) * 14;
                    double y = 16 + (i / 10) * 14;
                    if (split)
                    {
                        c.BeginPath();
                        c.Arc(x - 2, y - 1, 1.2, 0, JsMath.PI * 2);
                        c.Fill();
                        c.BeginPath();
                        c.Arc(x + 2, y - 1, 1.2, 0, JsMath.PI * 2);
                        c.Fill();
                    }
                    else
                    {
                        c.BeginPath();
                        c.Arc(x - 2, y - 1, 1.2, 0, JsMath.PI * 2);
                        c.Arc(x + 2, y - 1, 1.2, 0, JsMath.PI * 2);
                        c.Fill();
                    }
                }
                c.EndFrame();
                return RenderStats.Delta(RenderStats.Read(), before);
            }
        }

        static RenderStats.Snap DebrisFrameExtra(int n)
        {
            var physics = new PhysicsEngine(new ParityAudioCues(() => 0));
            physics.Center = new Vec2(88, 88);
            physics.RingRadius = 60;
            physics.InnerRadius = 28;
            physics.BlackHole.X = 88;
            physics.BlackHole.Y = 900;
            var renderer = new GameRenderer(EyeCanvas, EyeCanvas, 1);
            try
            {
                renderer.CosmicTime = 0;
                var empty = FrameDelta(renderer, physics);
                AddOrdinary(physics, n);
                renderer.CosmicTime = 0;
                var full = FrameDelta(renderer, physics);
                return RenderStats.Delta(full, empty);
            }
            finally
            {
                renderer.Dispose();
            }
        }

        static RenderStats.Snap FrameDelta(GameRenderer renderer, PhysicsEngine physics)
        {
            var before = RenderStats.Read();
            renderer.Render(physics, null, 0, null);
            return RenderStats.Delta(RenderStats.Read(), before);
        }

        static void AddOrdinary(PhysicsEngine physics, int n)
        {
            physics.DebrisList.Clear();
            for (int i = 0; i < n; i++)
            {
                string type = (i & 1) == 0 ? "titanium" : "magnetic";
                physics.DebrisList.Add(new Debris
                {
                    Id = i + 1,
                    X = 16 + (i % 10) * 14,
                    Y = 16 + (i / 10) * 14,
                    Radius = type == "magnetic" ? 6.0 : 6.5,
                    Type = type,
                    State = DebrisState.InRing,
                    Mass = 1,
                    Weight = 1,
                    Color = type == "magnetic" ? "#a855f7" : "#38bdf8"
                });
            }
        }

        static List<Debris> PatchPieces()
        {
            var list = new List<Debris>(PatchCount);
            for (int i = 0; i < PatchCount; i++)
            {
                list.Add(new Debris
                {
                    Id = i + 1,
                    X = 20 + (i % 3) * 22,
                    Y = 24 + (i / 3) * 22,
                    Radius = 6.5,
                    Type = (i & 1) == 0 ? "titanium" : "magnetic",
                    State = DebrisState.InRing
                });
            }
            return list;
        }

        static Color32[] DrawPatch(List<Debris> pieces, bool realSplit, out RenderStats.Snap delta)
        {
            using (var c = new Canvas2D(PatchCanvas, PatchCanvas))
            {
                var before = RenderStats.Read();
                c.BeginFrame();
                c.SetTransform(PatchScale, 0, 0, PatchScale, 0, 0);
                if (realSplit) DrawRealDebris(c, pieces);
                else DrawCompoundDebris(c, pieces);
                c.EndFrame();
                delta = RenderStats.Delta(RenderStats.Read(), before);
                return ReadPixels(c);
            }
        }

        static void DrawRealDebris(Canvas2D ctx, List<Debris> list)
        {
            var renderer = new GameRenderer(4, 4, 1);
            try
            {
                var method = typeof(GameRenderer).GetMethod("RenderCartoonDebris", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(method, "RenderCartoonDebris");
                method.Invoke(renderer, new object[] { ctx, list });
            }
            finally
            {
                renderer.Dispose();
            }
        }

        /// <summary>Ordinary-piece calls with both eyes on one path, even when FastShape is on.</summary>
        static void DrawCompoundDebris(Canvas2D ctx, List<Debris> list)
        {
            ctx.Save();
            for (int i = 0; i < list.Count; i++)
            {
                var d = list[i];
                ctx.Save();
                ctx.Translate(d.X, d.Y);
                ctx.FillStyle = "#facc15";
                ctx.StrokeStyle = "#1e1b4b";
                ctx.LineWidth = 2;
                ctx.BeginPath();
                ctx.Arc(0, 0, d.Radius, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.Stroke();
                ctx.FillStyle = "#1e1b4b";
                ctx.BeginPath();
                ctx.Arc(-2, -1, 1.2, 0, JsMath.PI * 2);
                ctx.Arc(2, -1, 1.2, 0, JsMath.PI * 2);
                ctx.Fill();
                ctx.BeginPath();
                ctx.Arc(0, 1.5, 2, 0, JsMath.PI);
                ctx.Stroke();
                ctx.Restore();
            }
            ctx.Restore();
        }

        static Color32[] ReadPixels(Canvas2D c)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = c.Target;
            var tex = new Texture2D(c.Width, c.Height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, c.Width, c.Height), 0, 0, false);
            RenderTexture.active = prev;
            var px = tex.GetPixels32();
            Object.DestroyImmediate(tex);
            return px;
        }

        static void Diff(Color32[] a, Color32[] b, out int maxChannel, out int pixels)
        {
            maxChannel = 0;
            pixels = 0;
            int n = a.Length < b.Length ? a.Length : b.Length;
            for (int i = 0; i < n; i++)
            {
                int dr = a[i].r - b[i].r; if (dr < 0) dr = -dr;
                int dg = a[i].g - b[i].g; if (dg < 0) dg = -dg;
                int db = a[i].b - b[i].b; if (db < 0) db = -db;
                int da = a[i].a - b[i].a; if (da < 0) da = -da;
                int m = dr;
                if (dg > m) m = dg;
                if (db > m) m = db;
                if (da > m) m = da;
                if (m > maxChannel) maxChannel = m;
                if (m > 0) pixels++;
            }
        }
    }
}
