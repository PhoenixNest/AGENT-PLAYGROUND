using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Render
{
    public enum LineCapStyle { Butt, Round, Square }
    public enum LineJoinStyle { Miter, Round, Bevel }

    /// <summary>Flat list of polygons: points plus the start index of each polygon.</summary>
    public sealed class PolygonSet
    {
        public readonly List<Vec2d> pts = new List<Vec2d>(1024);
        public readonly List<int> starts = new List<int>(64);
        public void Clear() { pts.Clear(); starts.Clear(); }
        public void Begin() { starts.Add(pts.Count); }
        public int PolygonCount => starts.Count;
        public int Start(int i) => starts[i];
        public int End(int i) => i + 1 < starts.Count ? starts[i + 1] : pts.Count;
    }

    /// <summary>
    /// Converts polylines to the outline polygons of their stroke (Skia semantics: inner joins pivot through the
    /// centre point, miter limit falls back to a bevel, closed contours produce an outer and an inner polygon).
    /// Output is meant to be filled with the nonzero rule. Works in the coordinate system of the input.
    /// </summary>
    public sealed class Stroker
    {
        public double halfWidth;
        public LineCapStyle cap;
        public LineJoinStyle join;
        public double miterLimit = 10;
        public double arcStep = 0.1;

        readonly List<Vec2d> left = new List<Vec2d>(256);
        readonly List<Vec2d> right = new List<Vec2d>(256);
        readonly List<Vec2d> work = new List<Vec2d>(256);

        public static void Dedupe(IList<Vec2d> src, bool closed, List<Vec2d> dst, double eps)
        {
            dst.Clear();
            for (int i = 0; i < src.Count; i++)
            {
                var p = src[i];
                if (dst.Count > 0)
                {
                    var q = dst[dst.Count - 1];
                    if (Math.Abs(p.x - q.x) <= eps && Math.Abs(p.y - q.y) <= eps) continue;
                }
                dst.Add(p);
            }
            if (closed)
                while (dst.Count > 1 && Math.Abs(dst[0].x - dst[dst.Count - 1].x) <= eps && Math.Abs(dst[0].y - dst[dst.Count - 1].y) <= eps)
                    dst.RemoveAt(dst.Count - 1);
        }

        public void Stroke(IList<Vec2d> input, bool closed, PolygonSet output, double eps, bool allowDot = false)
        {
            Dedupe(input, closed, work, eps);
            int n = work.Count;
            if (n == 0) return;
            if (n == 1) { if (allowDot) Dot(work[0], output); return; }
            if (closed && n >= 2) StrokeClosed(work, output);
            else StrokeOpen(work, output);
        }

        void Dot(Vec2d p, PolygonSet output)
        {
            if (cap == LineCapStyle.Butt) return;
            output.Begin();
            if (cap == LineCapStyle.Square)
            {
                output.pts.Add(new Vec2d(p.x - halfWidth, p.y - halfWidth));
                output.pts.Add(new Vec2d(p.x + halfWidth, p.y - halfWidth));
                output.pts.Add(new Vec2d(p.x + halfWidth, p.y + halfWidth));
                output.pts.Add(new Vec2d(p.x - halfWidth, p.y + halfWidth));
                return;
            }
            int steps = Math.Max(8, (int)Math.Ceiling(Math.PI * 2 / arcStep));
            for (int i = 0; i < steps; i++)
            {
                double a = Math.PI * 2 * i / steps;
                output.pts.Add(new Vec2d(p.x + halfWidth * Math.Cos(a), p.y + halfWidth * Math.Sin(a)));
            }
        }

        static Vec2d Dir(Vec2d a, Vec2d b)
        {
            var d = b - a;
            double l = d.Length;
            return new Vec2d(d.x / l, d.y / l);
        }

        static Vec2d Normal(Vec2d d) => new Vec2d(-d.y, d.x);

        void ArcPoints(List<Vec2d> dst, Vec2d center, Vec2d u, double total, bool skipFirst, bool skipLast)
        {
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(total) / arcStep));
            int from = skipFirst ? 1 : 0;
            int to = skipLast ? steps - 1 : steps;
            for (int k = from; k <= to; k++)
            {
                double a = total * k / steps;
                double c = Math.Cos(a), s = Math.Sin(a);
                dst.Add(new Vec2d(center.x + halfWidth * (u.x * c - u.y * s), center.y + halfWidth * (u.x * s + u.y * c)));
            }
        }

        // Appends the join at vertex p between directions d0 and d1 to one offset side (s = +1 left, -1 right).
        void Join(List<Vec2d> side, Vec2d p, Vec2d d0, Vec2d d1, double s)
        {
            var n0 = Normal(d0);
            var n1 = Normal(d1);
            var a = new Vec2d(p.x + s * halfWidth * n0.x, p.y + s * halfWidth * n0.y);
            var b = new Vec2d(p.x + s * halfWidth * n1.x, p.y + s * halfWidth * n1.y);
            double dot = Vec2d.Dot(d0, d1);
            double cross = Vec2d.Cross(d0, d1);
            if (dot > 0.99999999 && Math.Abs(cross) < 1e-9) { side.Add(a); return; }
            bool inner = s * cross > 0;
            if (inner)
            {
                side.Add(a);
                if (dot < 0.9998) side.Add(p);
                side.Add(b);
                return;
            }
            switch (join)
            {
                case LineJoinStyle.Miter:
                {
                    double denom = 1 + dot;
                    if (denom > 1e-9 && 2.0 / denom <= miterLimit * miterLimit)
                    {
                        double k = s * halfWidth / denom;
                        side.Add(new Vec2d(p.x + k * (n0.x + n1.x), p.y + k * (n0.y + n1.y)));
                    }
                    else { side.Add(a); side.Add(b); }
                    break;
                }
                case LineJoinStyle.Round:
                {
                    double t = Math.Atan2(cross, dot);
                    if (Math.Abs(cross) < 1e-9 && dot < 0) t = -s * Math.PI;
                    var u = new Vec2d(s * n0.x, s * n0.y);
                    ArcPoints(side, p, u, t, false, false);
                    break;
                }
                default:
                    side.Add(a); side.Add(b);
                    break;
            }
        }

        void StrokeOpen(List<Vec2d> p, PolygonSet output)
        {
            int n = p.Count;
            left.Clear(); right.Clear();
            var d0 = Dir(p[0], p[1]);
            var n0 = Normal(d0);
            left.Add(new Vec2d(p[0].x + halfWidth * n0.x, p[0].y + halfWidth * n0.y));
            right.Add(new Vec2d(p[0].x - halfWidth * n0.x, p[0].y - halfWidth * n0.y));
            var prev = d0;
            for (int i = 1; i < n - 1; i++)
            {
                var d1 = Dir(p[i], p[i + 1]);
                Join(left, p[i], prev, d1, 1);
                Join(right, p[i], prev, d1, -1);
                prev = d1;
            }
            var dl = prev;
            var nl = Normal(dl);
            var last = p[n - 1];
            left.Add(new Vec2d(last.x + halfWidth * nl.x, last.y + halfWidth * nl.y));
            right.Add(new Vec2d(last.x - halfWidth * nl.x, last.y - halfWidth * nl.y));

            output.Begin();
            var o = output.pts;
            o.AddRange(left);
            switch (cap)
            {
                case LineCapStyle.Square:
                    o.Add(new Vec2d(last.x + halfWidth * (nl.x + dl.x), last.y + halfWidth * (nl.y + dl.y)));
                    o.Add(new Vec2d(last.x + halfWidth * (-nl.x + dl.x), last.y + halfWidth * (-nl.y + dl.y)));
                    break;
                case LineCapStyle.Round:
                    ArcPoints(o, last, nl, -Math.PI, true, true);
                    break;
            }
            for (int i = right.Count - 1; i >= 0; i--) o.Add(right[i]);
            var first = p[0];
            switch (cap)
            {
                case LineCapStyle.Square:
                    o.Add(new Vec2d(first.x + halfWidth * (-n0.x - d0.x), first.y + halfWidth * (-n0.y - d0.y)));
                    o.Add(new Vec2d(first.x + halfWidth * (n0.x - d0.x), first.y + halfWidth * (n0.y - d0.y)));
                    break;
                case LineCapStyle.Round:
                    ArcPoints(o, first, new Vec2d(-n0.x, -n0.y), -Math.PI, true, true);
                    break;
            }
        }

        void StrokeClosed(List<Vec2d> p, PolygonSet output)
        {
            int n = p.Count;
            left.Clear(); right.Clear();
            for (int i = 0; i < n; i++)
            {
                var prevPt = p[(i + n - 1) % n];
                var cur = p[i];
                var next = p[(i + 1) % n];
                var d0 = Dir(prevPt, cur);
                var d1 = Dir(cur, next);
                Join(left, cur, d0, d1, 1);
                Join(right, cur, d0, d1, -1);
            }
            output.Begin();
            output.pts.AddRange(left);
            output.Begin();
            for (int i = right.Count - 1; i >= 0; i--) output.pts.Add(right[i]);
        }
    }

    public static class Dasher
    {
        /// <summary>
        /// Splits a polyline into dash polylines with Skia's semantics (closed contours that start inside a dash merge
        /// the first and last dash). <paramref name="dashes"/> must have an even count and a positive sum.
        /// </summary>
        public static void Dash(IList<Vec2d> src, bool closed, double[] dashes, double offset, List<List<Vec2d>> result, Stack<List<Vec2d>> pool, double eps)
        {
            var pts = new List<Vec2d>(src.Count + 1);
            Stroker.Dedupe(src, closed, pts, eps);
            if (pts.Count == 0) return;
            if (pts.Count == 1) return;
            if (closed) pts.Add(pts[0]);

            double total = 0;
            foreach (double v in dashes) total += v;
            double ph = offset % total;
            if (ph < 0) ph += total;
            int idx = 0;
            double rem = dashes[0];
            while (ph >= rem)
            {
                ph -= rem;
                idx = (idx + 1) % dashes.Length;
                rem = dashes[idx];
                if (total <= 0) return;
            }
            rem -= ph;

            List<Vec2d> current = null;
            List<Vec2d> first = null;
            bool startedOn = (idx % 2) == 0;
            if (startedOn) { current = New(pool); current.Add(pts[0]); first = current; }
            int resultStart = result.Count;

            for (int s = 0; s + 1 < pts.Count; s++)
            {
                var a = pts[s];
                var b = pts[s + 1];
                double segLen = (b - a).Length;
                if (segLen <= 0) continue;
                double pos = 0;
                while (true)
                {
                    double avail = segLen - pos;
                    bool on = (idx % 2) == 0;
                    if (rem > avail)
                    {
                        if (on) current.Add(b);
                        rem -= avail;
                        break;
                    }
                    pos += rem;
                    var pt = new Vec2d(a.x + (b.x - a.x) * pos / segLen, a.y + (b.y - a.y) * pos / segLen);
                    if (on)
                    {
                        current.Add(pt);
                        if (current.Count == 1) current.Add(pt);
                        result.Add(current);
                        current = null;
                    }
                    idx = (idx + 1) % dashes.Length;
                    rem = dashes[idx];
                    if ((idx % 2) == 0)
                    {
                        current = New(pool);
                        current.Add(pt);
                    }
                }
            }
            if (current != null)
            {
                if (closed && startedOn && first != null && current != first && result.Count > resultStart && ReferenceEquals(result[resultStart], first))
                {
                    for (int i = 1; i < first.Count; i++) current.Add(first[i]);
                    result.RemoveAt(resultStart);
                }
                if (current.Count == 1) current.Add(current[0]);
                result.Add(current);
            }
        }

        static List<Vec2d> New(Stack<List<Vec2d>> pool)
        {
            if (pool.Count > 0) { var l = pool.Pop(); l.Clear(); return l; }
            return new List<Vec2d>(16);
        }
    }
}
