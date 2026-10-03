using System;
using System.Globalization;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// CSS easing. Cubic-bezier y is not clamped, so overshoot (control points outside 0..1) is preserved.
    /// The x component is solved with Newton, then bisection, which is safe because CSS requires x1 and x2 in 0..1.
    /// </summary>
    public static class CssEasing
    {
        public static double Ease(string name, double t)
        {
            if (t <= 0) return 0;
            if (t >= 1) return 1;
            if (string.IsNullOrEmpty(name) || name == "linear") return name == "linear" ? t : CubicBezier(0.25, 0.1, 0.25, 1, t);
            if (name == "ease") return CubicBezier(0.25, 0.1, 0.25, 1, t);
            if (name == "ease-in") return CubicBezier(0.42, 0, 1, 1, t);
            if (name == "ease-out") return CubicBezier(0, 0, 0.58, 1, t);
            if (name == "ease-in-out") return CubicBezier(0.42, 0, 0.58, 1, t);
            if (name.StartsWith("cubic-bezier(", StringComparison.Ordinal) && name.EndsWith(")", StringComparison.Ordinal))
            {
                var parts = name.Substring(13, name.Length - 14).Split(',');
                if (parts.Length == 4
                    && D(parts[0], out var x1) && D(parts[1], out var y1) && D(parts[2], out var x2) && D(parts[3], out var y2))
                    return CubicBezier(x1, y1, x2, y2, t);
            }
            return t;
        }

        public static double CubicBezier(double x1, double y1, double x2, double y2, double x)
        {
            if (x <= 0) return 0;
            if (x >= 1) return 1;
            double u = SolveX(x1, x2, x);
            return Bezier(u, y1, y2);
        }

        static double SolveX(double x1, double x2, double x)
        {
            double u = x;
            for (int i = 0; i < 8; i++)
            {
                double err = Bezier(u, x1, x2) - x;
                if (Math.Abs(err) < 1e-7) return u;
                double slope = BezierDeriv(u, x1, x2);
                if (Math.Abs(slope) < 1e-6) break;
                double next = u - err / slope;
                if (next <= 0 || next >= 1) break;
                u = next;
            }
            double lo = 0, hi = 1;
            for (int i = 0; i < 24; i++)
            {
                double mid = (lo + hi) * 0.5;
                if (Bezier(mid, x1, x2) < x) lo = mid;
                else hi = mid;
            }
            return (lo + hi) * 0.5;
        }

        static double Bezier(double u, double a, double b)
        {
            double iu = 1 - u;
            return 3 * iu * iu * u * a + 3 * iu * u * u * b + u * u * u;
        }

        static double BezierDeriv(double u, double a, double b)
        {
            double iu = 1 - u;
            return 3 * iu * iu * a + 6 * iu * u * (b - a) + 3 * u * u * (1 - b);
        }

        static bool D(string s, out double v) =>
            double.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }
}
