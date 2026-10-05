using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// CSS transform lists: translate / translateX / translateY, scale, rotate, matrix, and
    /// <c>calc()</c> with <c>var(--dx)</c>. Matching function lists interpolate component-wise.
    /// A different count or kind falls back to matrix decomposition, as CSS does.
    /// </summary>
    public static class CssTransform
    {
        enum Kind { Translate, TranslateX, TranslateY, Scale, Rotate, Matrix }

        struct Len
        {
            public double PxPart;
            public double Pct;
            public double Resolve(double basis) => PxPart + Pct / 100.0 * basis;
            public static Len Px(double v) => new Len { PxPart = v };
            public static Len Percent(double v) => new Len { Pct = v };
        }

        struct Fn
        {
            public Kind Kind;
            public Len X, Y;
            public double A, B, C, D, E, F;
        }

        public static Matrix2D ToMatrix(string css, double width, double height, double dx)
        {
            var list = Parse(css ?? "none", dx);
            return Compose(list, width, height);
        }

        public static string Format(Matrix2D m)
        {
            return string.Format(CultureInfo.InvariantCulture, "matrix({0},{1},{2},{3},{4},{5})", m.a, m.b, m.c, m.d, m.e, m.f);
        }

        public static Matrix2D Interpolate(string from, string to, double u, double width, double height, double dx)
        {
            // Exact endpoints stay the sampled keyframe. Values outside 0..1 are real:
            // cubic-bezier overshoot (cartoonToast) must extrapolate, not clamp.
            if (u == 0) return ToMatrix(from, width, height, dx);
            if (u == 1) return ToMatrix(to, width, height, dx);
            var a = Parse(from ?? "none", dx);
            var b = Parse(to ?? "none", dx);
            if (a.Count == b.Count && SameKinds(a, b))
            {
                var mid = new List<Fn>(a.Count);
                for (int i = 0; i < a.Count; i++) mid.Add(LerpFn(a[i], b[i], u, width, height));
                return Compose(mid, width, height);
            }
            return LerpMatrix(ToMatrix(from, width, height, dx), ToMatrix(to, width, height, dx), u);
        }

        static bool SameKinds(List<Fn> a, List<Fn> b)
        {
            for (int i = 0; i < a.Count; i++) if (a[i].Kind != b[i].Kind) return false;
            return true;
        }

        static Fn LerpFn(Fn a, Fn b, double u, double width, double height)
        {
            var m = a;
            if (a.Kind == Kind.Rotate)
            {
                m.X = Len.Px(a.X.PxPart + (b.X.PxPart - a.X.PxPart) * u);
                return m;
            }
            if (a.Kind == Kind.Scale)
            {
                m.X = Len.Px(a.X.PxPart + (b.X.PxPart - a.X.PxPart) * u);
                m.Y = Len.Px(a.Y.PxPart + (b.Y.PxPart - a.Y.PxPart) * u);
                return m;
            }
            if (a.Kind == Kind.Matrix)
            {
                var blended = LerpMatrix(FnMatrix(a), FnMatrix(b), u);
                m.A = blended.a; m.B = blended.b; m.C = blended.c; m.D = blended.d; m.E = blended.e; m.F = blended.f;
                return m;
            }
            double ax = a.X.Resolve(width), bx = b.X.Resolve(width);
            double ay = a.Y.Resolve(height), by = b.Y.Resolve(height);
            m.X = Len.Px(ax + (bx - ax) * u);
            m.Y = Len.Px(ay + (by - ay) * u);
            return m;
        }

        static Matrix2D FnMatrix(Fn f) => new Matrix2D(f.A, f.B, f.C, f.D, f.E, f.F);

        public static Matrix2D LerpMatrix(Matrix2D a, Matrix2D b, double u)
        {
            Decompose(a, out var tx0, out var ty0, out var r0, out var sx0, out var sy0, out var sh0);
            Decompose(b, out var tx1, out var ty1, out var r1, out var sx1, out var sy1, out var sh1);
            return Recompose(
                tx0 + (tx1 - tx0) * u, ty0 + (ty1 - ty0) * u,
                r0 + (r1 - r0) * u,
                sx0 + (sx1 - sx0) * u, sy0 + (sy1 - sy0) * u,
                sh0 + (sh1 - sh0) * u);
        }

        public static void Decompose(Matrix2D m, out double tx, out double ty, out double rot, out double sx, out double sy, out double shear)
        {
            tx = m.e; ty = m.f;
            double a = m.a, b = m.b, c = m.c, d = m.d;
            sx = Math.Sqrt(a * a + b * b);
            if (sx > 1e-12) { a /= sx; b /= sx; }
            shear = a * c + b * d;
            c -= a * shear; d -= b * shear;
            sy = Math.Sqrt(c * c + d * d);
            if (sy > 1e-12) { c /= sy; d /= sy; shear /= sy; }
            if (a * d - b * c < 0) { a = -a; b = -b; sx = -sx; shear = -shear; }
            rot = Math.Atan2(b, a);
        }

        public static Matrix2D Recompose(double tx, double ty, double rot, double sx, double sy, double shear)
        {
            double cos = Math.Cos(rot), sin = Math.Sin(rot);
            return new Matrix2D(
                sx * cos, sx * sin,
                sy * (shear * cos - sin), sy * (shear * sin + cos),
                tx, ty);
        }

        static Matrix2D Compose(List<Fn> list, double width, double height)
        {
            var m = Matrix2D.Identity;
            for (int i = list.Count - 1; i >= 0; i--)
                m = MatrixOf(list[i], width, height).Multiply(m);
            return m;
        }

        static Matrix2D MatrixOf(Fn f, double width, double height)
        {
            switch (f.Kind)
            {
                case Kind.Translate:
                    return new Matrix2D(1, 0, 0, 1, f.X.Resolve(width), f.Y.Resolve(height));
                case Kind.TranslateX:
                    return new Matrix2D(1, 0, 0, 1, f.X.Resolve(width), 0);
                case Kind.TranslateY:
                    return new Matrix2D(1, 0, 0, 1, 0, f.X.Resolve(height));
                case Kind.Scale:
                    return new Matrix2D(f.X.PxPart, 0, 0, f.Y.PxPart, 0, 0);
                case Kind.Rotate:
                    double rad = f.X.PxPart * Math.PI / 180.0;
                    double c = Math.Cos(rad), s = Math.Sin(rad);
                    return new Matrix2D(c, s, -s, c, 0, 0);
                default:
                    return FnMatrix(f);
            }
        }

        static List<Fn> Parse(string css, double dx)
        {
            var list = new List<Fn>();
            if (string.IsNullOrWhiteSpace(css) || css == "none") return list;
            foreach (var raw in SplitFunctions(css))
            {
                int paren = raw.IndexOf('(');
                if (paren < 0 || !raw.EndsWith(")", StringComparison.Ordinal)) continue;
                string name = raw.Substring(0, paren).Trim();
                string body = raw.Substring(paren + 1, raw.Length - paren - 2);
                var args = SplitArgs(body);
                if (name == "translate")
                {
                    var x = ArgLen(args, 0, dx);
                    var y = args.Count > 1 ? ArgLen(args, 1, dx) : Len.Px(0);
                    list.Add(new Fn { Kind = Kind.Translate, X = x, Y = y });
                }
                else if (name == "translateX")
                    list.Add(new Fn { Kind = Kind.TranslateX, X = ArgLen(args, 0, dx) });
                else if (name == "translateY")
                    list.Add(new Fn { Kind = Kind.TranslateY, X = ArgLen(args, 0, dx) });
                else if (name == "scale")
                {
                    double x = ArgNum(args, 0);
                    double y = args.Count > 1 ? ArgNum(args, 1) : x;
                    list.Add(new Fn { Kind = Kind.Scale, X = Len.Px(x), Y = Len.Px(y) });
                }
                else if (name == "rotate")
                    list.Add(new Fn { Kind = Kind.Rotate, X = Len.Px(ArgAngle(args, 0)) });
                else if (name == "matrix" && args.Count >= 6)
                {
                    list.Add(new Fn
                    {
                        Kind = Kind.Matrix,
                        A = ArgNum(args, 0), B = ArgNum(args, 1), C = ArgNum(args, 2),
                        D = ArgNum(args, 3), E = ArgNum(args, 4), F = ArgNum(args, 5)
                    });
                }
            }
            return list;
        }

        static Len ArgLen(List<string> args, int i, double dx)
        {
            if (i >= args.Count) return Len.Px(0);
            return ParseLen(ResolveVars(args[i], dx));
        }

        static double ArgNum(List<string> args, int i)
        {
            if (i >= args.Count) return 0;
            return double.TryParse(args[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;
        }

        static double ArgAngle(List<string> args, int i)
        {
            if (i >= args.Count) return 0;
            string s = args[i].Trim();
            double n = CssBox.ParseLength(s.Replace("deg", "px").Replace("rad", "px").Replace("turn", "px").Replace("grad", "px"));
            if (s.EndsWith("rad", StringComparison.Ordinal)) return n * 180.0 / Math.PI;
            if (s.EndsWith("turn", StringComparison.Ordinal)) return n * 360.0;
            if (s.EndsWith("grad", StringComparison.Ordinal)) return n * 0.9;
            return n;
        }

        static Len ParseLen(string s)
        {
            s = s.Trim();
            if (s.StartsWith("calc(", StringComparison.Ordinal) && s.EndsWith(")", StringComparison.Ordinal))
            {
                double px = 0, pct = 0;
                EvalCalc(s.Substring(5, s.Length - 6), ref px, ref pct);
                return new Len { PxPart = px, Pct = pct };
            }
            if (s.EndsWith("%", StringComparison.Ordinal))
                return Len.Percent(double.TryParse(s.Substring(0, s.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var p) ? p : 0);
            return Len.Px(CssBox.ParseLength(s));
        }

        // calc() that mixes % and px is resolved in ArgLen's caller only if we keep both. FloatUpFade does mix them.
        // ParseLen above drops the px when a percent is present. Fix by resolving against a basis inside Interpolate,
        // which already converts to px. Store mixed lengths as px==NaN sentinel? Better: resolve calc to (px, pct) fully here
        // and fold px into Value when Percent is set by using a pair. I'll redo ParseLen properly below via MixedLen.
        static void EvalCalc(string inner, ref double px, ref double pct)
        {
            int i = 0;
            while (i < inner.Length)
            {
                while (i < inner.Length && inner[i] == ' ') i++;
                if (i >= inner.Length) break;
                int sign = 1;
                if (inner[i] == '+') i++;
                else if (inner[i] == '-') { sign = -1; i++; }
                while (i < inner.Length && inner[i] == ' ') i++;
                int start = i, depth = 0;
                while (i < inner.Length)
                {
                    char ch = inner[i];
                    if (ch == '(') depth++;
                    else if (ch == ')') depth--;
                    else if (depth == 0 && (ch == '+' || ch == '-')) break;
                    i++;
                }
                string term = inner.Substring(start, i - start).Trim();
                if (term.Length == 0) continue;
                if (term.EndsWith("%", StringComparison.Ordinal))
                {
                    if (double.TryParse(term.Substring(0, term.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out var p))
                        pct += sign * p;
                }
                else px += sign * CssBox.ParseLength(term);
            }
        }

        public static string ResolveVars(string s, double dx)
        {
            if (string.IsNullOrEmpty(s) || s.IndexOf("var(", StringComparison.Ordinal) < 0) return s;
            var sb = new StringBuilder();
            int i = 0;
            while (i < s.Length)
            {
                int v = s.IndexOf("var(", i, StringComparison.Ordinal);
                if (v < 0) { sb.Append(s, i, s.Length - i); break; }
                sb.Append(s, i, v - i);
                int depth = 0, end = v;
                for (int k = v; k < s.Length; k++)
                {
                    if (s[k] == '(') depth++;
                    else if (s[k] == ')') { depth--; if (depth == 0) { end = k; break; } }
                }
                string body = s.Substring(v + 4, end - (v + 4));
                int comma = IndexComma(body);
                string varName = (comma >= 0 ? body.Substring(0, comma) : body).Trim();
                string fallback = comma >= 0 ? body.Substring(comma + 1).Trim() : "0px";
                if (varName == "--dx")
                    sb.Append(dx.ToString("0.####", CultureInfo.InvariantCulture)).Append("px");
                else sb.Append(fallback);
                i = end + 1;
            }
            return sb.ToString();
        }

        static int IndexComma(string s)
        {
            int depth = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')') depth--;
                else if (s[i] == ',' && depth == 0) return i;
            }
            return -1;
        }

        static List<string> SplitArgs(string body) => SplitTop(body, ',');

        static List<string> SplitFunctions(string css)
        {
            var list = new List<string>();
            int i = 0;
            while (i < css.Length)
            {
                while (i < css.Length && css[i] == ' ') i++;
                if (i >= css.Length) break;
                int start = i;
                while (i < css.Length && css[i] != '(' && css[i] != ' ') i++;
                if (i >= css.Length || css[i] != '(') { i++; continue; }
                int depth = 0;
                for (; i < css.Length; i++)
                {
                    if (css[i] == '(') depth++;
                    else if (css[i] == ')') { depth--; if (depth == 0) { i++; break; } }
                }
                list.Add(css.Substring(start, i - start).Trim());
            }
            return list;
        }

        static List<string> SplitTop(string s, char sep)
        {
            var parts = new List<string>();
            int depth = 0, start = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')') depth--;
                else if (s[i] == sep && depth == 0)
                {
                    parts.Add(s.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }
            if (start < s.Length) parts.Add(s.Substring(start).Trim());
            return parts;
        }
    }
}
