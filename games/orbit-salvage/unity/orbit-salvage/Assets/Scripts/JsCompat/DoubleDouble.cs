using System;

namespace OrbitalSalvage.JsCompat
{
    internal struct DD
    {
        public double Hi;
        public double Lo;

        public DD(double hi, double lo)
        {
            Hi = hi;
            Lo = lo;
        }

        public DD(double hi)
        {
            Hi = hi;
            Lo = 0.0;
        }
    }

    internal static class DDMath
    {
        private const double Splitter = 134217729.0;

        public static void TwoSum(double a, double b, out double s, out double e)
        {
            s = a + b;
            double bb = s - a;
            e = (a - (s - bb)) + (b - bb);
        }

        public static void QuickTwoSum(double a, double b, out double s, out double e)
        {
            s = a + b;
            e = b - (s - a);
        }

        public static void TwoProd(double a, double b, out double p, out double e)
        {
            p = a * b;
            double t = Splitter * a;
            double ahi = t - (t - a);
            double alo = a - ahi;
            t = Splitter * b;
            double bhi = t - (t - b);
            double blo = b - bhi;
            e = ((ahi * bhi - p) + ahi * blo + alo * bhi) + alo * blo;
        }

        public static DD Add(DD a, DD b)
        {
            double s1, s2, t1, t2;
            TwoSum(a.Hi, b.Hi, out s1, out s2);
            TwoSum(a.Lo, b.Lo, out t1, out t2);
            s2 += t1;
            QuickTwoSum(s1, s2, out s1, out s2);
            s2 += t2;
            QuickTwoSum(s1, s2, out s1, out s2);
            return new DD(s1, s2);
        }

        public static DD Add(DD a, double b)
        {
            double s1, s2;
            TwoSum(a.Hi, b, out s1, out s2);
            s2 += a.Lo;
            QuickTwoSum(s1, s2, out s1, out s2);
            return new DD(s1, s2);
        }

        public static DD Neg(DD a)
        {
            return new DD(-a.Hi, -a.Lo);
        }

        public static DD Sub(DD a, DD b)
        {
            return Add(a, Neg(b));
        }

        public static DD Mul(DD a, DD b)
        {
            double p1, p2;
            TwoProd(a.Hi, b.Hi, out p1, out p2);
            p2 += (a.Hi * b.Lo + a.Lo * b.Hi);
            QuickTwoSum(p1, p2, out p1, out p2);
            return new DD(p1, p2);
        }

        public static DD Mul(DD a, double b)
        {
            double p1, p2;
            TwoProd(a.Hi, b, out p1, out p2);
            p2 += a.Lo * b;
            QuickTwoSum(p1, p2, out p1, out p2);
            return new DD(p1, p2);
        }

        public static DD Div(DD a, double b)
        {
            double q1 = a.Hi / b;
            double p1, p2, s, e;
            TwoProd(q1, b, out p1, out p2);
            TwoSum(a.Hi, -p1, out s, out e);
            e -= p2;
            e += a.Lo;
            double q2 = (s + e) / b;
            QuickTwoSum(q1, q2, out q1, out q2);
            return new DD(q1, q2);
        }

        public static DD Div(DD a, DD b)
        {
            double q1 = a.Hi / b.Hi;
            DD r = Sub(a, Mul(b, q1));
            double q2 = r.Hi / b.Hi;
            r = Sub(r, Mul(b, q2));
            double q3 = r.Hi / b.Hi;
            double h, l;
            QuickTwoSum(q1, q2, out h, out l);
            return Add(new DD(h, l), q3);
        }

        public static DD Quotient(double y, double x)
        {
            double q = y / x;
            double p, e;
            TwoProd(q, x, out p, out e);
            double r = ((y - p) - e);
            return new DD(q, r / x);
        }
    }
}
