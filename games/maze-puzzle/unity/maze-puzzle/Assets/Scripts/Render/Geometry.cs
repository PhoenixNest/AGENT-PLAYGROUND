using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Render
{
    public struct Vec2d
    {
        public double x, y;
        public Vec2d(double x, double y) { this.x = x; this.y = y; }
        public static Vec2d operator +(Vec2d a, Vec2d b) => new Vec2d(a.x + b.x, a.y + b.y);
        public static Vec2d operator -(Vec2d a, Vec2d b) => new Vec2d(a.x - b.x, a.y - b.y);
        public static Vec2d operator *(Vec2d a, double s) => new Vec2d(a.x * s, a.y * s);
        public static Vec2d operator *(double s, Vec2d a) => new Vec2d(a.x * s, a.y * s);
        public static double Length2(Vec2d a) => a.x * a.x + a.y * a.y;
        public double Length => Math.Sqrt(x * x + y * y);
        public static double Dot(Vec2d a, Vec2d b) => a.x * b.x + a.y * b.y;
        public static double Cross(Vec2d a, Vec2d b) => a.x * b.y - a.y * b.x;
    }

    /// <summary>Affine matrix in the canvas convention: x' = a x + c y + e, y' = b x + d y + f.</summary>
    public struct Matrix2D
    {
        public double a, b, c, d, e, f;
        public static readonly Matrix2D Identity = new Matrix2D(1, 0, 0, 1, 0, 0);
        public Matrix2D(double a, double b, double c, double d, double e, double f)
        {
            this.a = a; this.b = b; this.c = c; this.d = d; this.e = e; this.f = f;
        }

        public Vec2d Apply(double x, double y) => new Vec2d(a * x + c * y + e, b * x + d * y + f);
        public Vec2d Apply(Vec2d p) => Apply(p.x, p.y);
        public double Determinant => a * d - b * c;
        public bool IsInvertible => Math.Abs(Determinant) > 1e-12 && !double.IsNaN(Determinant);

        /// <summary>this * m: m is applied first, then this (post-multiplication used by translate/rotate/scale/transform).</summary>
        public Matrix2D Multiply(Matrix2D m)
        {
            return new Matrix2D(
                a * m.a + c * m.b, b * m.a + d * m.b,
                a * m.c + c * m.d, b * m.c + d * m.d,
                a * m.e + c * m.f + e, b * m.e + d * m.f + f);
        }

        public Matrix2D Inverse()
        {
            double det = Determinant;
            double id = 1.0 / det;
            return new Matrix2D(d * id, -b * id, -c * id, a * id, (c * f - d * e) * id, (b * e - a * f) * id);
        }

        /// <summary>Largest singular value (maximum stretch).</summary>
        public double MaxScale()
        {
            double s1 = a * a + b * b + c * c + d * d;
            double s2 = Math.Sqrt(Math.Max(0, (a * a + b * b - c * c - d * d) * (a * a + b * b - c * c - d * d) + 4 * (a * c + b * d) * (a * c + b * d)));
            return Math.Sqrt(Math.Max(1e-12, (s1 + s2) * 0.5));
        }
    }

    /// <summary>Axis-aligned integer pixel rectangle, max exclusive.</summary>
    public struct PixelRect
    {
        public int x0, y0, x1, y1;
        public PixelRect(int x0, int y0, int x1, int y1) { this.x0 = x0; this.y0 = y0; this.x1 = x1; this.y1 = y1; }
        public bool IsEmpty => x1 <= x0 || y1 <= y0;
        public int Width => x1 - x0;
        public int Height => y1 - y0;
        public PixelRect Expand(int n) => new PixelRect(x0 - n, y0 - n, x1 + n, y1 + n);
        public PixelRect Intersect(PixelRect o) => new PixelRect(Math.Max(x0, o.x0), Math.Max(y0, o.y0), Math.Min(x1, o.x1), Math.Min(y1, o.y1));
    }

    public sealed class Subpath
    {
        public readonly List<Vec2d> pts = new List<Vec2d>(64);
        public bool closed;
        public bool explicitClose;
        public void Clear() { pts.Clear(); closed = false; explicitClose = false; }
    }

    /// <summary>
    /// A canvas path stored as flattened polylines in device space (points are transformed by the CTM when they are
    /// added, as the HTML canvas specification and Chromium do). Curves are flattened at construction with a
    /// tolerance in device pixels.
    /// </summary>
    public sealed class PathData
    {
        public const double FlattenTolerance = 0.05;
        readonly List<Subpath> subpaths = new List<Subpath>(16);
        readonly Stack<Subpath> pool = new Stack<Subpath>();
        Subpath cur;
        bool needMove = true;
        Vec2d lastUser;
        Vec2d startPoint;

        public int Count => subpaths.Count;
        public Subpath this[int i] => subpaths[i];
        public bool HasCurrent => cur != null && cur.pts.Count > 0 && !needMove;
        public Vec2d LastDevice => cur.pts[cur.pts.Count - 1];

        public void Reset()
        {
            foreach (var s in subpaths) { s.Clear(); pool.Push(s); }
            subpaths.Clear();
            cur = null;
            needMove = true;
        }

        Subpath NewSubpath()
        {
            var s = pool.Count > 0 ? pool.Pop() : new Subpath();
            s.Clear();
            subpaths.Add(s);
            cur = s;
            return s;
        }

        public void MoveTo(Matrix2D m, double x, double y)
        {
            if (!IsFinite(x, y)) return;
            var s = NewSubpath();
            var p = m.Apply(x, y);
            s.pts.Add(p);
            lastUser = new Vec2d(x, y);
            startPoint = lastUser;
            needMove = false;
        }

        public void LineTo(Matrix2D m, double x, double y)
        {
            if (!IsFinite(x, y)) return;
            if (!PrepareConnect(m, x, y)) return;
            cur.pts.Add(m.Apply(x, y));
            lastUser = new Vec2d(x, y);
        }

        /// <summary>
        /// Makes sure a subpath is open. Returns false when the call created the subpath at (x, y) itself
        /// (nothing to connect), true when the caller must still append its connecting segment.
        /// </summary>
        bool PrepareConnect(Matrix2D m, double x, double y)
        {
            if (cur == null) { MoveTo(m, x, y); return false; }
            if (needMove)
            {
                MoveTo(m, startPoint.x, startPoint.y);
                return true;
            }
            return true;
        }

        public void ClosePath()
        {
            if (cur == null || needMove && cur.closed) return;
            if (cur.pts.Count == 0) return;
            cur.closed = true;
            cur.explicitClose = true;
            needMove = true;
            lastUser = startPoint;
        }

        public void Rect(Matrix2D m, double x, double y, double w, double h)
        {
            if (!IsFinite(x, y) || !IsFinite(w, h)) return;
            MoveTo(m, x, y);
            cur.pts.Add(m.Apply(x + w, y));
            cur.pts.Add(m.Apply(x + w, y + h));
            cur.pts.Add(m.Apply(x, y + h));
            cur.closed = true;
            cur.explicitClose = true;
            needMove = true;
            lastUser = new Vec2d(x, y);
            startPoint = lastUser;
        }

        public void QuadTo(Matrix2D m, double cx, double cy, double x, double y)
        {
            if (!IsFinite(cx, cy) || !IsFinite(x, y)) return;
            PrepareConnect(m, cx, cy);
            var p0 = cur.pts[cur.pts.Count - 1];
            var p1 = m.Apply(cx, cy);
            var p2 = m.Apply(x, y);
            double dd = Math.Max(Vec2d.Length2(p0 - 2 * p1 + p2), 0);
            int n = (int)Math.Ceiling(Math.Sqrt(Math.Sqrt(dd) * 0.25 / FlattenTolerance));
            n = Math.Max(1, Math.Min(n, 2000));
            for (int i = 1; i <= n; i++)
            {
                double t = (double)i / n, u = 1 - t;
                cur.pts.Add(new Vec2d(u * u * p0.x + 2 * u * t * p1.x + t * t * p2.x, u * u * p0.y + 2 * u * t * p1.y + t * t * p2.y));
            }
            lastUser = new Vec2d(x, y);
        }

        public void BezierTo(Matrix2D m, double c1x, double c1y, double c2x, double c2y, double x, double y)
        {
            if (!IsFinite(c1x, c1y) || !IsFinite(c2x, c2y) || !IsFinite(x, y)) return;
            PrepareConnect(m, c1x, c1y);
            var p0 = cur.pts[cur.pts.Count - 1];
            var p1 = m.Apply(c1x, c1y);
            var p2 = m.Apply(c2x, c2y);
            var p3 = m.Apply(x, y);
            double d1 = Vec2d.Length2(p0 - 2 * p1 + p2);
            double d2 = Vec2d.Length2(p1 - 2 * p2 + p3);
            double dd = Math.Sqrt(Math.Max(d1, d2));
            int n = (int)Math.Ceiling(Math.Sqrt(0.75 * dd / FlattenTolerance));
            n = Math.Max(1, Math.Min(n, 2000));
            for (int i = 1; i <= n; i++)
            {
                double t = (double)i / n, u = 1 - t;
                double b0 = u * u * u, b1 = 3 * u * u * t, b2 = 3 * u * t * t, b3 = t * t * t;
                cur.pts.Add(new Vec2d(b0 * p0.x + b1 * p1.x + b2 * p2.x + b3 * p3.x, b0 * p0.y + b1 * p1.y + b2 * p2.y + b3 * p3.y));
            }
            lastUser = new Vec2d(x, y);
        }

        public void Ellipse(Matrix2D m, double cx, double cy, double rx, double ry, double rotation, double a0, double a1, bool ccw)
        {
            if (!IsFinite(cx, cy) || !IsFinite(rx, ry) || !IsFinite(rotation) || !IsFinite(a0, a1)) return;
            if (rx < 0 || ry < 0) return;
            const double TwoPi = Math.PI * 2;
            double sweep;
            bool full = false;
            if (!ccw)
            {
                if (a1 - a0 >= TwoPi) { sweep = TwoPi; full = true; }
                else { sweep = Mod(a1 - a0, TwoPi); }
            }
            else
            {
                if (a0 - a1 >= TwoPi) { sweep = -TwoPi; full = true; }
                else { sweep = -Mod(a0 - a1, TwoPi); }
            }
            double cr = Math.Cos(rotation), sr = Math.Sin(rotation);
            Vec2d Pt(double ang)
            {
                double ex = rx * Math.Cos(ang), ey = ry * Math.Sin(ang);
                return new Vec2d(cx + ex * cr - ey * sr, cy + ex * sr + ey * cr);
            }
            var start = Pt(a0);
            bool connected = PrepareConnect(m, start.x, start.y);
            if (connected) cur.pts.Add(m.Apply(start));
            if (rx == 0 && ry == 0 || sweep == 0) { lastUser = start; return; }
            double scale = m.MaxScale();
            double rmax = Math.Max(rx, ry) * scale;
            double step = rmax < 1e-6 ? Math.PI : 2 * Math.Acos(Math.Max(0, 1 - FlattenTolerance / Math.Max(rmax, FlattenTolerance * 1.0001)));
            step = Math.Min(step, Math.PI / 4);
            int n = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / step));
            for (int i = 1; i <= n; i++)
            {
                double ang = a0 + sweep * i / n;
                cur.pts.Add(m.Apply(Pt(ang)));
            }
            var end = Pt(a0 + sweep);
            lastUser = end;
            if (full && !connected)
            {
                cur.pts.RemoveAt(cur.pts.Count - 1);
                cur.closed = true;
                cur.explicitClose = false;
                needMove = true;
                startPoint = end;
            }
        }

        public void Arc(Matrix2D m, double x, double y, double r, double a0, double a1, bool ccw)
        {
            if (r < 0) return;
            Ellipse(m, x, y, r, r, 0, a0, a1, ccw);
        }

        public void ArcTo(Matrix2D m, double x1, double y1, double x2, double y2, double r)
        {
            if (!IsFinite(x1, y1) || !IsFinite(x2, y2) || !IsFinite(r) || r < 0) return;
            if (!PrepareConnect(m, x1, y1)) return;
            var p0 = lastUser;
            var p1 = new Vec2d(x1, y1);
            var p2 = new Vec2d(x2, y2);
            if (Same(p0, p1) || Same(p1, p2) || r == 0) { LineTo(m, x1, y1); return; }
            var v0 = (p0 - p1);
            var v2 = (p2 - p1);
            double l0 = v0.Length, l2 = v2.Length;
            var u0 = v0 * (1 / l0);
            var u2 = v2 * (1 / l2);
            double cross = Vec2d.Cross(u0, u2);
            if (Math.Abs(cross) < 1e-9) { LineTo(m, x1, y1); return; }
            double dot = Vec2d.Dot(u0, u2);
            double ang = Math.Acos(Math.Max(-1, Math.Min(1, dot)));
            double tanHalf = Math.Tan(ang / 2);
            double dist = r / tanHalf;
            var t0 = p1 + u0 * dist;
            var t1 = p1 + u2 * dist;
            var bis = (u0 + u2);
            bis = bis * (1 / bis.Length);
            double cd = r / Math.Sin(ang / 2);
            var center = p1 + bis * cd;
            double aStart = Math.Atan2(t0.y - center.y, t0.x - center.x);
            double aEnd = Math.Atan2(t1.y - center.y, t1.x - center.x);
            double dAng = aEnd - aStart;
            while (dAng > Math.PI) dAng -= Math.PI * 2;
            while (dAng <= -Math.PI) dAng += Math.PI * 2;
            bool anticlockwise = dAng < 0;
            LineTo(m, t0.x, t0.y);
            Arc(m, center.x, center.y, r, aStart, aEnd, anticlockwise);
            if (cur != null && !cur.closed) lastUser = t1;
        }

        public void RoundRect(Matrix2D m, double x, double y, double w, double h, Vec2d[] radii)
        {
            if (!IsFinite(x, y) || !IsFinite(w, h)) return;
            if (w < 0) { x += w; w = -w; }
            if (h < 0) { y += h; h = -h; }
            double tlx = radii[0].x, tly = radii[0].y, trx = radii[1].x, try_ = radii[1].y;
            double brx = radii[2].x, bry = radii[2].y, blx = radii[3].x, bly = radii[3].y;
            double scale = 1;
            double top = tlx + trx, bottom = blx + brx, left = tly + bly, right = try_ + bry;
            if (top > w) scale = Math.Min(scale, w / top);
            if (bottom > w) scale = Math.Min(scale, w / bottom);
            if (left > h) scale = Math.Min(scale, h / left);
            if (right > h) scale = Math.Min(scale, h / right);
            if (scale < 1) { tlx *= scale; tly *= scale; trx *= scale; try_ *= scale; brx *= scale; bry *= scale; blx *= scale; bly *= scale; }
            const double HalfPi = Math.PI / 2;
            MoveTo(m, x + tlx, y);
            LineTo(m, x + w - trx, y);
            if (trx > 0 || try_ > 0) Ellipse(m, x + w - trx, y + try_, trx, try_, 0, -HalfPi, 0, false);
            LineTo(m, x + w, y + h - bry);
            if (brx > 0 || bry > 0) Ellipse(m, x + w - brx, y + h - bry, brx, bry, 0, 0, HalfPi, false);
            LineTo(m, x + blx, y + h);
            if (blx > 0 || bly > 0) Ellipse(m, x + blx, y + h - bly, blx, bly, 0, HalfPi, Math.PI, false);
            LineTo(m, x, y + tly);
            if (tlx > 0 || tly > 0) Ellipse(m, x + tlx, y + tly, tlx, tly, 0, Math.PI, Math.PI * 1.5, false);
            ClosePath();
            startPoint = new Vec2d(x + tlx, y);
            lastUser = new Vec2d(x + tlx, y);
        }

        public bool GetBounds(out double minX, out double minY, out double maxX, out double maxY)
        {
            minX = minY = double.MaxValue;
            maxX = maxY = double.MinValue;
            bool any = false;
            foreach (var s in subpaths)
                foreach (var p in s.pts)
                {
                    any = true;
                    if (p.x < minX) minX = p.x;
                    if (p.y < minY) minY = p.y;
                    if (p.x > maxX) maxX = p.x;
                    if (p.y > maxY) maxY = p.y;
                }
            return any;
        }

        static double Mod(double v, double m) { double r = v % m; if (r < 0) r += m; return r; }
        static bool Same(Vec2d a, Vec2d b) => Math.Abs(a.x - b.x) < 1e-12 && Math.Abs(a.y - b.y) < 1e-12;
        static bool IsFinite(double a) => !double.IsNaN(a) && !double.IsInfinity(a);
        static bool IsFinite(double a, double b) => IsFinite(a) && IsFinite(b);
    }

    public static class Vec2dExt
    {
    }
}
