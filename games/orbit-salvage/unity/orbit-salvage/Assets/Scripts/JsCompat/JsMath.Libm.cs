using System;
using System.Numerics;

namespace OrbitalSalvage.JsCompat
{
    /// <summary>
    /// Chromium 153 (V8 15.3) routes Math.sin, Math.cos, Math.atan and Math.atan2 through LLVM-libc,
    /// whose double-precision implementations are correctly rounded. Results are therefore platform
    /// independent and are reproduced here with a double-double fast path plus a BigInteger fallback
    /// that is taken only when the rounding test is inconclusive.
    /// </summary>
    public static partial class JsMath
    {
        private const double ZivRelativeError = 2.0194839173657902e-28;

        private static class Consts
        {
            public static readonly double[] PioTwo = BuildPioTwo();
            public static readonly DD PiDD;
            public static readonly DD PioTwoDD;
            public static readonly DD[] SinCoef;
            public static readonly DD[] CosCoef;

            static Consts()
            {
                BigInteger pi = BigLibm.Pi(400);
                PiDD = ToDD(pi, 400);
                PioTwoDD = new DD(PiDD.Hi * 0.5, PiDD.Lo * 0.5);

                const int n = 16;
                SinCoef = new DD[n];
                CosCoef = new DD[n];
                DD a = new DD(1.0);
                DD b = new DD(1.0);
                SinCoef[0] = a;
                CosCoef[0] = b;
                for (int i = 1; i < n; i++)
                {
                    a = DDMath.Div(DDMath.Neg(a), (2.0 * i) * (2.0 * i + 1.0));
                    b = DDMath.Div(DDMath.Neg(b), (2.0 * i - 1.0) * (2.0 * i));
                    SinCoef[i] = a;
                    CosCoef[i] = b;
                }
            }

            private static DD ToDD(BigInteger fixedValue, int p)
            {
                double hi = BigLibm.RoundToDouble(fixedValue, p);
                BigInteger rem = fixedValue - BigLibm.FromDouble(hi, p);
                double lo = BigLibm.RoundToDouble(rem, p);
                return new DD(hi, lo);
            }

            private static double[] BuildPioTwo()
            {
                const int p = 400;
                BigInteger v = BigLibm.Pi(p) >> 1;
                var parts = new double[5];
                for (int i = 0; i < 4; i++)
                {
                    int bits = 30;
                    int top = BigLibm.BitLength(v);
                    int shift = top - bits;
                    BigInteger chunk = v >> shift;
                    parts[i] = BigLibm.RoundToDouble(chunk << shift, p);
                    v -= chunk << shift;
                }
                parts[4] = BigLibm.RoundToDouble(v, p);
                return parts;
            }
        }

        private static void SinCosDD(DD r, out DD s, out DD c)
        {
            DD r2 = DDMath.Mul(r, r);
            DD[] sc = Consts.SinCoef;
            DD[] cc = Consts.CosCoef;
            int n = sc.Length - 1;
            DD ps = sc[n];
            DD pc = cc[n];
            for (int i = n - 1; i >= 0; i--)
            {
                ps = DDMath.Add(DDMath.Mul(ps, r2), sc[i]);
                pc = DDMath.Add(DDMath.Mul(pc, r2), cc[i]);
            }
            s = DDMath.Mul(ps, r);
            c = pc;
        }

        private static bool ZivRound(DD v, out double result)
        {
            double e = Abs(v.Hi) * ZivRelativeError;
            double a = v.Hi + (v.Lo - e);
            double b = v.Hi + (v.Lo + e);
            result = a;
            return a == b;
        }

        private static bool Reduce(double x, out int quadrant, out DD r)
        {
            double ax = Abs(x);
            if (ax < 0.7853981633974483)
            {
                quadrant = 0;
                r = new DD(x);
                return true;
            }
            if (ax >= 4194304.0)
            {
                quadrant = 0;
                r = new DD(x);
                return false;
            }
            double k = Math.Round(x * 0.6366197723675814);
            double[] pp = Consts.PioTwo;
            double s, e;
            DDMath.TwoSum(x, -(k * pp[0]), out s, out e);
            DD d = new DD(s, e);
            d = DDMath.Add(d, -(k * pp[1]));
            d = DDMath.Add(d, -(k * pp[2]));
            d = DDMath.Add(d, -(k * pp[3]));
            d = DDMath.Add(d, -(k * pp[4]));
            r = d;
            int ik = (int)k;
            quadrant = ((ik % 4) + 4) % 4;
            return Abs(d.Hi) > 9.313225746154785e-10;
        }

        public static double Sin(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x)) return double.NaN;
            if (x == 0.0) return x;
            int q;
            DD r;
            if (Reduce(x, out q, out r))
            {
                DD s, c;
                SinCosDD(r, out s, out c);
                DD v = q == 0 ? s : q == 1 ? c : q == 2 ? DDMath.Neg(s) : DDMath.Neg(c);
                double result;
                if (ZivRound(v, out result)) return result;
            }
            return BigLibm.SinCos(x, true);
        }

        public static double Cos(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x)) return double.NaN;
            if (x == 0.0) return 1.0;
            int q;
            DD r;
            if (Reduce(x, out q, out r))
            {
                DD s, c;
                SinCosDD(r, out s, out c);
                DD v = q == 0 ? c : q == 1 ? DDMath.Neg(s) : q == 2 ? DDMath.Neg(c) : s;
                double result;
                if (ZivRound(v, out result)) return result;
            }
            return BigLibm.SinCos(x, false);
        }

        private static bool AtanCore(DD q, out DD result)
        {
            double theta0 = FdlibmAtan(q.Hi);
            DD s, c;
            SinCosDD(new DD(theta0), out s, out c);
            DD n = DDMath.Sub(DDMath.Mul(q, c), s);
            DD d = DDMath.Add(c, DDMath.Mul(q, s));
            double delta = (n.Hi + n.Lo) / d.Hi;
            double hi, lo;
            DDMath.TwoSum(theta0, delta, out hi, out lo);
            result = new DD(hi, lo);
            return true;
        }

        public static double Atan(double x)
        {
            if (double.IsNaN(x)) return x;
            double ax = Abs(x);
            if (ax < 1.4901161193847656e-8) return x == 0.0 ? x : x + x * -5.551115123125783e-17;
            if (ax >= 18014398509481984.0)
            {
                double hpi = 1.5707963267948966;
                return x < 0 ? -hpi : hpi;
            }
            DD core;
            if (ax > 1.0)
            {
                DD inv = DDMath.Div(new DD(1.0), new DD(ax));
                AtanCore(inv, out core);
                core = DDMath.Sub(Consts.PioTwoDD, core);
            }
            else
            {
                AtanCore(new DD(ax), out core);
            }
            double result;
            if (!ZivRound(core, out result)) result = BigLibm.Atan(ax);
            return x < 0 ? -result : result;
        }

        public static double Atan2(double y, double x)
        {
            const double tiny = 1.0e-300;
            const double zero = 0.0;
            const double pi_o_4 = 7.8539816339744827900E-01;
            const double pi_o_2 = 1.5707963267948965580E+00;
            const double pi = 3.1415926535897931160E+00;
            const double pi_3o4 = 2.3561944901923449288469825374596;

            long xb = BitConverter.DoubleToInt64Bits(x);
            long yb = BitConverter.DoubleToInt64Bits(y);
            int hx = unchecked((int)(xb >> 32));
            uint lx = unchecked((uint)xb);
            int ix = hx & 0x7FFFFFFF;
            int hy = unchecked((int)(yb >> 32));
            uint ly = unchecked((uint)yb);
            int iy = hy & 0x7FFFFFFF;

            uint nx = (lx | unchecked(0u - lx)) >> 31;
            uint ny = (ly | unchecked(0u - ly)) >> 31;
            if ((uint)(ix | (int)nx) > 0x7FF00000u || (uint)(iy | (int)ny) > 0x7FF00000u)
            {
                return x + y;
            }
            if ((unchecked((uint)(hx - 0x3FF00000)) | lx) == 0)
            {
                return Atan(y);
            }
            int m = ((hy >> 31) & 1) | ((hx >> 30) & 2);

            if (iy == 0 && ly == 0)
            {
                switch (m)
                {
                    case 0:
                    case 1:
                        return y;
                    case 2:
                        return pi + tiny;
                    default:
                        return -pi - tiny;
                }
            }
            if (ix == 0 && lx == 0) return hy < 0 ? -pi_o_2 - tiny : pi_o_2 + tiny;

            if (ix == 0x7FF00000)
            {
                if (iy == 0x7FF00000)
                {
                    switch (m)
                    {
                        case 0:
                            return pi_o_4 + tiny;
                        case 1:
                            return -pi_o_4 - tiny;
                        case 2:
                            return pi_3o4;
                        default:
                            return -pi_3o4;
                    }
                }
                switch (m)
                {
                    case 0:
                        return zero;
                    case 1:
                        return -zero;
                    case 2:
                        return pi + tiny;
                    default:
                        return -pi - tiny;
                }
            }
            if (iy == 0x7FF00000) return hy < 0 ? -pi_o_2 - tiny : pi_o_2 + tiny;

            double ay = Abs(y);
            double ax = Abs(x);
            bool xNeg = hx < 0;
            int k = (iy - ix) >> 20;
            double result;
            if (k > 58)
            {
                result = pi_o_2;
            }
            else if (k < -58)
            {
                result = xNeg ? pi : ay / ax;
            }
            else
            {
                bool normal = ix >= 0x20000000 && iy >= 0x20000000 && ix < 0x5FF00000 && iy < 0x5FF00000;
                if (normal)
                {
                    DD q = DDMath.Quotient(ay, ax);
                    DD core;
                    if (q.Hi > 1.0)
                    {
                        DD inv = DDMath.Quotient(ax, ay);
                        AtanCore(inv, out core);
                        core = DDMath.Sub(Consts.PioTwoDD, core);
                    }
                    else
                    {
                        AtanCore(q, out core);
                    }
                    DD v = xNeg ? DDMath.Sub(Consts.PiDD, core) : core;
                    if (!ZivRound(v, out result)) result = BigLibm.Atan2Positive(ay, ax, xNeg);
                }
                else
                {
                    result = BigLibm.Atan2Positive(ay, ax, xNeg);
                }
            }
            if (result == 0.0) return 0.0;
            return hy < 0 ? -result : result;
        }
    }
}
