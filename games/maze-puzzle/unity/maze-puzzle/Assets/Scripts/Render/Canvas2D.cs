using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    public sealed class CanvasGradient
    {
        internal readonly bool radial;
        internal readonly double x0, y0, r0, x1, y1, r1;
        internal readonly List<KeyValuePair<float, Rgba>> stops = new List<KeyValuePair<float, Rgba>>(4);

        internal CanvasGradient(double x0, double y0, double x1, double y1)
        {
            radial = false; this.x0 = x0; this.y0 = y0; this.x1 = x1; this.y1 = y1;
        }

        internal CanvasGradient(double x0, double y0, double r0, double x1, double y1, double r1)
        {
            radial = true; this.x0 = x0; this.y0 = y0; this.r0 = r0; this.x1 = x1; this.y1 = y1; this.r1 = r1;
        }

        public void AddColorStop(double offset, string color)
        {
            if (!CssColor.TryParse(color, out var c)) return;
            AddColorStop(offset, c);
        }

        public void AddColorStop(double offset, Rgba color)
        {
            if (!(offset >= 0 && offset <= 1)) return;
            int at = stops.Count;
            while (at > 0 && stops[at - 1].Key > offset) at--;
            stops.Insert(at, new KeyValuePair<float, Rgba>((float)offset, color));
        }
    }

    /// <summary>
    /// GPU implementation of the subset of CanvasRenderingContext2D used by the game. Calls record into a
    /// CommandBuffer; <see cref="EndFrame"/> uploads the geometry and executes it.
    /// </summary>
    public sealed partial class Canvas2D : IDisposable
    {
        sealed class State
        {
            public Matrix2D ctm = Matrix2D.Identity;
            public Rgba fillColor = Rgba.Black, strokeColor = Rgba.Black;
            public CanvasGradient fillGrad, strokeGrad;
            public double lineWidth = 1;
            public LineCapStyle lineCap = LineCapStyle.Butt;
            public LineJoinStyle lineJoin = LineJoinStyle.Miter;
            public double miterLimit = 10;
            public double[] lineDash;
            public double lineDashOffset;
            public double globalAlpha = 1;
            public Rgba shadowColor = Rgba.Transparent;
            public double shadowBlur, shadowOffsetX, shadowOffsetY;
            public string font = "10px sans-serif";
            public string textAlign = "start";
            public string textBaseline = "alphabetic";
            public double letterSpacing;
            public bool imageSmoothing = true;
            public RenderTexture clip;

            public State Clone() => (State)MemberwiseClone();
        }

        public static bool GradientInterpolatesPremultiplied = false;
        public static double ShadowScale = 1.0;
        public static bool SnapRects = true;

        State state = new State();
        readonly Stack<State> stack = new Stack<State>();
        readonly PathData path = new PathData();
        readonly PathData tmpPath = new PathData();
        readonly PolygonSet polys = new PolygonSet();
        readonly PolygonSet userPolys = new PolygonSet();
        readonly Stroker stroker = new Stroker();
        readonly List<List<Vec2d>> dashList = new List<List<Vec2d>>();
        readonly Stack<List<Vec2d>> dashPool = new Stack<List<Vec2d>>();
        readonly List<Vec2d> userPts = new List<Vec2d>(256);

        public int Width { get; }
        public int Height { get; }

        public void Save() { stack.Push(state.Clone()); }

        public void Restore()
        {
            if (stack.Count > 0) state = stack.Pop();
        }

        public void ResetTransform() { state.ctm = Matrix2D.Identity; }

        public void SetTransform(double a, double b, double c, double d, double e, double f)
        {
            if (!Finite(a, b) || !Finite(c, d) || !Finite(e, f)) return;
            state.ctm = new Matrix2D(a, b, c, d, e, f);
        }

        public void Transform(double a, double b, double c, double d, double e, double f)
        {
            if (!Finite(a, b) || !Finite(c, d) || !Finite(e, f)) return;
            state.ctm = state.ctm.Multiply(new Matrix2D(a, b, c, d, e, f));
        }

        public void Translate(double x, double y)
        {
            if (!Finite(x, y)) return;
            state.ctm = state.ctm.Multiply(new Matrix2D(1, 0, 0, 1, x, y));
        }

        public void Scale(double x, double y)
        {
            if (!Finite(x, y)) return;
            state.ctm = state.ctm.Multiply(new Matrix2D(x, 0, 0, y, 0, 0));
        }

        public void Rotate(double angle)
        {
            if (!Finite(angle)) return;
            double c = Math.Cos(angle), s = Math.Sin(angle);
            state.ctm = state.ctm.Multiply(new Matrix2D(c, s, -s, c, 0, 0));
        }

        public Matrix2D CurrentTransform => state.ctm;

        public object FillStyle
        {
            set
            {
                if (value is string s) { if (CssColor.TryParse(s, out var c)) SetFill(c); }
                else if (value is CanvasGradient g) { state.fillGrad = g; }
                else if (value is Rgba c) SetFill(c);
            }
        }

        public void SetFill(Rgba color)
        {
            state.fillColor = color;
            state.fillGrad = null;
        }

        public void SetShadow(Rgba color) { state.shadowColor = color; }

        public object StrokeStyle
        {
            set
            {
                if (value is string s) { if (CssColor.TryParse(s, out var c)) { state.strokeColor = c; state.strokeGrad = null; } }
                else if (value is CanvasGradient g) { state.strokeGrad = g; }
            }
        }

        public double LineWidth { get => state.lineWidth; set { if (value > 0 && !double.IsInfinity(value)) state.lineWidth = value; } }

        public string LineCap
        {
            set
            {
                if (value == "butt") state.lineCap = LineCapStyle.Butt;
                else if (value == "round") state.lineCap = LineCapStyle.Round;
                else if (value == "square") state.lineCap = LineCapStyle.Square;
            }
        }

        public string LineJoin
        {
            set
            {
                if (value == "miter") state.lineJoin = LineJoinStyle.Miter;
                else if (value == "round") state.lineJoin = LineJoinStyle.Round;
                else if (value == "bevel") state.lineJoin = LineJoinStyle.Bevel;
            }
        }

        public double MiterLimit { set { if (value > 0 && !double.IsInfinity(value)) state.miterLimit = value; } }
        public double LineDashOffset { set { if (Finite(value)) state.lineDashOffset = value; } }
        public double GlobalAlpha { get => state.globalAlpha; set { if (value >= 0 && value <= 1) state.globalAlpha = value; } }
        public string ShadowColor { set { if (CssColor.TryParse(value, out var c)) state.shadowColor = c; } }
        public double ShadowBlur { set { if (value >= 0 && !double.IsInfinity(value)) state.shadowBlur = value; } }
        public double ShadowOffsetX { set { if (Finite(value)) state.shadowOffsetX = value; } }
        public double ShadowOffsetY { set { if (Finite(value)) state.shadowOffsetY = value; } }
        public double LetterSpacing { set { if (Finite(value)) state.letterSpacing = value; } }
        public bool ImageSmoothingEnabled { set { state.imageSmoothing = value; } }

        public void SetLineDash(double[] segments)
        {
            if (segments == null || segments.Length == 0) { state.lineDash = null; return; }
            foreach (double v in segments) if (!(v >= 0) || double.IsInfinity(v)) return;
            int n = segments.Length % 2 == 0 ? segments.Length : segments.Length * 2;
            var d = new double[n];
            for (int i = 0; i < n; i++) d[i] = segments[i % segments.Length];
            double sum = 0;
            foreach (double v in d) sum += v;
            state.lineDash = sum > 0 ? d : null;
        }

        public CanvasGradient CreateLinearGradient(double x0, double y0, double x1, double y1)
        {
            if (!Finite(x0, y0) || !Finite(x1, y1)) throw new ArgumentException("non-finite gradient coordinate");
            return new CanvasGradient(x0, y0, x1, y1);
        }

        public CanvasGradient CreateRadialGradient(double x0, double y0, double r0, double x1, double y1, double r1)
        {
            if (!Finite(x0, y0) || !Finite(x1, y1) || !Finite(r0, r1) || r0 < 0 || r1 < 0) throw new ArgumentException("invalid radial gradient");
            return new CanvasGradient(x0, y0, r0, x1, y1, r1);
        }

        public void BeginPath() { path.Reset(); }
        public void ClosePath() { path.ClosePath(); }
        public void MoveTo(double x, double y) { path.MoveTo(state.ctm, x, y); }
        public void LineTo(double x, double y) { path.LineTo(state.ctm, x, y); }
        public void QuadraticCurveTo(double cx, double cy, double x, double y) { path.QuadTo(state.ctm, cx, cy, x, y); }
        public void BezierCurveTo(double c1x, double c1y, double c2x, double c2y, double x, double y) { path.BezierTo(state.ctm, c1x, c1y, c2x, c2y, x, y); }
        public void ArcTo(double x1, double y1, double x2, double y2, double r) { path.ArcTo(state.ctm, x1, y1, x2, y2, r); }
        public void Arc(double x, double y, double r, double a0, double a1, bool anticlockwise = false) { path.Arc(state.ctm, x, y, r, a0, a1, anticlockwise); }

        public void Ellipse(double x, double y, double rx, double ry, double rotation, double a0, double a1, bool anticlockwise = false)
        {
            path.Ellipse(state.ctm, x, y, rx, ry, rotation, a0, a1, anticlockwise);
        }

        public void Rect(double x, double y, double w, double h) { path.Rect(state.ctm, x, y, w, h); }

        public void RoundRect(double x, double y, double w, double h, params double[] radii)
        {
            var v = new Vec2d[radii.Length];
            for (int i = 0; i < radii.Length; i++) v[i] = new Vec2d(radii[i], radii[i]);
            RoundRect(x, y, w, h, v);
        }

        public void RoundRect(double x, double y, double w, double h, params Vec2d[] radii)
        {
            if (radii == null || radii.Length == 0 || radii.Length > 4) return;
            foreach (var r in radii) if (!(r.x >= 0) || !(r.y >= 0) || double.IsInfinity(r.x) || double.IsInfinity(r.y)) return;
            Vec2d tl, tr, br, bl;
            switch (radii.Length)
            {
                case 1: tl = tr = br = bl = radii[0]; break;
                case 2: tl = br = radii[0]; tr = bl = radii[1]; break;
                case 3: tl = radii[0]; tr = bl = radii[1]; br = radii[2]; break;
                default: tl = radii[0]; tr = radii[1]; br = radii[2]; bl = radii[3]; break;
            }
            path.RoundRect(state.ctm, x, y, w, h, new[] { tl, tr, br, bl });
        }

        public void Fill(string rule = "nonzero")
        {
            int kind = KindOf(path.Simple);
            RenderStats.NoteCmd(kind);
            RenderStats.PendingKind = kind;
            RenderStats.CountShape = true;
            try
            {
                if (RenderOpts.FastShape && TryFastFill(path)) return;
                BuildFillPolygons(path, polys);
                RenderShape(polys, rule == "evenodd", false, 1f);
            }
            finally { RenderStats.CountShape = false; }
        }

        public void Stroke()
        {
            RenderStats.NoteCmd(5);
            RenderStats.PendingKind = 5;
            RenderStats.CountShape = true;
            try
            {
                if (RenderOpts.FastShape && TryFastStroke(path)) return;
                StrokePath(path);
            }
            finally { RenderStats.CountShape = false; }
        }

        static int KindOf(int simple)
        {
            if (simple == 1) return 2;
            if (simple == 2) return 3;
            if (simple == 3) return 4;
            if (simple == 4) return 1;
            return 0;
        }

        public void Clip(string rule = "nonzero")
        {
            BuildFillPolygons(path, polys);
            ApplyClip(polys, rule == "evenodd");
        }

        public void FillRect(double x, double y, double w, double h)
        {
            if (!Finite(x, y) || !Finite(w, h) || w == 0 || h == 0) return;
            tmpPath.Reset();
            tmpPath.Rect(state.ctm, x, y, w, h);
            RenderStats.NoteCmd(1);
            RenderStats.PendingKind = 1;
            RenderStats.CountShape = true;
            try
            {
                if (RenderOpts.FastShape && TryFastFill(tmpPath)) return;
                BuildFillPolygons(tmpPath, polys);
                RenderShape(polys, false, false, 1f, SnapRects);
            }
            finally { RenderStats.CountShape = false; }
        }

        public void StrokeRect(double x, double y, double w, double h)
        {
            if (!Finite(x, y) || !Finite(w, h) || (w == 0 && h == 0)) return;
            tmpPath.Reset();
            tmpPath.Rect(state.ctm, x, y, w, h);
            StrokePath(tmpPath);
        }

        public void ClearRect(double x, double y, double w, double h)
        {
            if (!Finite(x, y) || !Finite(w, h) || w == 0 || h == 0) return;
            tmpPath.Reset();
            tmpPath.Rect(state.ctm, x, y, w, h);
            BuildFillPolygons(tmpPath, polys);
            RenderClear(polys, SnapRects);
        }

        static void BuildFillPolygons(PathData p, PolygonSet dst)
        {
            PaintProfile.Begin(PaintProfile.FillPolys);
            dst.Clear();
            for (int i = 0; i < p.Count; i++)
            {
                var s = p[i];
                if (s.pts.Count < 3) continue;
                dst.Begin();
                dst.pts.AddRange(s.pts);
            }
            PaintProfile.End(PaintProfile.FillPolys);
        }

        void StrokePath(PathData p)
        {
            var st = state;
            if (!st.ctm.IsInvertible) return;
            double lw = st.lineWidth;
            if (!(lw > 0)) return;
            double scale = st.ctm.MaxScale();
            float alphaMul = 1f;
            if (lw * scale < 1.0)
            {
                alphaMul = (float)(lw * scale);
                lw = 1.0 / scale;
            }
            var inv = st.ctm.Inverse();
            stroker.halfWidth = lw * 0.5;
            stroker.cap = st.lineCap;
            stroker.join = st.lineJoin;
            stroker.miterLimit = st.miterLimit;
            double hwDev = stroker.halfWidth * scale;
            stroker.arcStep = Math.Min(Math.PI / 4, 2 * Math.Acos(Math.Max(0, 1 - 0.05 / Math.Max(hwDev, 0.05))));
            if (stroker.arcStep < 0.02) stroker.arcStep = 0.02;
            double eps = 1e-7 / Math.Max(scale, 1e-6);

            userPolys.Clear();
            for (int i = 0; i < p.Count; i++)
            {
                var sp = p[i];
                if (sp.pts.Count < 2 && !(sp.closed && sp.pts.Count == 1)) continue;
                userPts.Clear();
                foreach (var pt in sp.pts) userPts.Add(inv.Apply(pt));
                if (st.lineDash != null)
                {
                    foreach (var l in dashList) dashPool.Push(l);
                    dashList.Clear();
                    Dasher.Dash(userPts, sp.closed, st.lineDash, st.lineDashOffset, dashList, dashPool, eps);
                    foreach (var piece in dashList) stroker.Stroke(piece, false, userPolys, eps, true);
                }
                else
                {
                    stroker.Stroke(userPts, sp.closed, userPolys, eps);
                }
            }

            polys.Clear();
            for (int i = 0; i < userPolys.PolygonCount; i++)
            {
                polys.Begin();
                for (int j = userPolys.Start(i); j < userPolys.End(i); j++) polys.pts.Add(st.ctm.Apply(userPolys.pts[j]));
            }
            RenderShape(polys, false, true, alphaMul);
        }

        static bool Finite(double a) => !double.IsNaN(a) && !double.IsInfinity(a);
        static bool Finite(double a, double b) => Finite(a) && Finite(b);
    }
}
