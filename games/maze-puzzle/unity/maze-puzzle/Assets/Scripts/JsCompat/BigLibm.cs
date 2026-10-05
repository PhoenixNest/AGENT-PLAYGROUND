using System;
using System.Numerics;

namespace OrbitalSalvage.JsCompat
{
    internal static class BigLibm
    {
        private static BigInteger _pi;
        private static int _piBits;

        public static BigInteger Pi(int bits)
        {
            if (_piBits < bits)
            {
                int q = bits + 64;
                BigInteger one = BigInteger.One << q;
                BigInteger pi = 16 * AtanInv(5, one, q) - 4 * AtanInv(239, one, q);
                _pi = pi;
                _piBits = q;
            }
            return _pi >> (_piBits - bits);
        }

        private static BigInteger AtanInv(int n, BigInteger one, int q)
        {
            BigInteger term = one / n;
            BigInteger sum = term;
            BigInteger n2 = (BigInteger)n * n;
            int k = 1;
            while (!term.IsZero)
            {
                term /= n2;
                BigInteger t = term / (2 * k + 1);
                if ((k & 1) == 1) sum -= t;
                else sum += t;
                k++;
            }
            return sum;
        }

        public static void Decompose(double x, out BigInteger mantissa, out int exponent)
        {
            long bits = BitConverter.DoubleToInt64Bits(x);
            int e = (int)((bits >> 52) & 0x7FF);
            long m = bits & 0xFFFFFFFFFFFFFL;
            if (e == 0)
            {
                exponent = -1074;
            }
            else
            {
                m |= 1L << 52;
                exponent = e - 1075;
            }
            mantissa = m;
            if (bits < 0) mantissa = -mantissa;
        }

        public static BigInteger FromDouble(double x, int p)
        {
            BigInteger m;
            int e;
            Decompose(x, out m, out e);
            int sh = e + p;
            return sh >= 0 ? m << sh : m >> (-sh);
        }

        public static double RoundToDouble(BigInteger v, int p)
        {
            if (v.IsZero) return 0.0;
            bool neg = v.Sign < 0;
            if (neg) v = -v;
            int bl = BitLength(v);
            int shift = bl - 53;
            BigInteger m;
            if (shift > 0)
            {
                m = v >> shift;
                BigInteger rem = v - (m << shift);
                BigInteger half = BigInteger.One << (shift - 1);
                int cmp = rem.CompareTo(half);
                if (cmp > 0 || (cmp == 0 && !m.IsEven)) m += 1;
                if (BitLength(m) > 53)
                {
                    m >>= 1;
                    shift++;
                }
            }
            else
            {
                m = v << (-shift);
            }
            int e2 = shift - p;
            int biased = e2 + 52 + 1023;
            if (biased < 1 || biased > 2046)
            {
                double approx = (double)m * Math.Pow(2.0, e2);
                return neg ? -approx : approx;
            }
            long bits = ((long)biased << 52) | ((long)m & 0xFFFFFFFFFFFFFL);
            double r = BitConverter.Int64BitsToDouble(bits);
            return neg ? -r : r;
        }

        public static int BitLength(BigInteger v)
        {
            if (v.IsZero) return 0;
            byte[] bytes = v.ToByteArray();
            int top = bytes.Length - 1;
            while (top > 0 && bytes[top] == 0) top--;
            int b = bytes[top];
            int bits = 0;
            while (b != 0)
            {
                bits++;
                b >>= 1;
            }
            return top * 8 + bits;
        }

        private static BigInteger ISqrt(BigInteger n)
        {
            if (n < 2) return n;
            BigInteger x = BigInteger.One << ((BitLength(n) + 1) / 2);
            while (true)
            {
                BigInteger y = (x + n / x) >> 1;
                if (y >= x) return x;
                x = y;
            }
        }

        private static BigInteger RoundDiv(BigInteger a, BigInteger b)
        {
            BigInteger two = a * 2 + (a.Sign >= 0 ? b : -b);
            return two / (b * 2);
        }

        public static void SinCosFixed(BigInteger r, int p, out BigInteger s, out BigInteger c)
        {
            BigInteger one = BigInteger.One << p;
            BigInteger r2 = (r * r) >> p;
            s = BigInteger.Zero;
            c = BigInteger.Zero;
            BigInteger ts = r;
            BigInteger tc = one;
            for (int n = 0; n < 4000; n++)
            {
                if (ts.IsZero && tc.IsZero) break;
                if ((n & 1) == 0)
                {
                    s += ts;
                    c += tc;
                }
                else
                {
                    s -= ts;
                    c -= tc;
                }
                ts = ((ts * r2) >> p) / ((2 * n + 2) * (2 * n + 3));
                tc = ((tc * r2) >> p) / ((2 * n + 1) * (2 * n + 2));
            }
        }

        public static double SinCos(double x, bool wantSin)
        {
            BigInteger m;
            int e;
            Decompose(x, out m, out e);
            int p = 320 + Math.Max(0, e + 53);
            BigInteger bx = FromDouble(x, p);
            BigInteger pio2 = Pi(p) >> 1;
            BigInteger k = RoundDiv(bx, pio2);
            BigInteger r = bx - k * pio2;
            BigInteger s, c;
            SinCosFixed(r, p, out s, out c);
            int q = (int)(((k % 4) + 4) % 4);
            BigInteger result;
            if (wantSin)
            {
                result = q == 0 ? s : q == 1 ? c : q == 2 ? -s : -c;
            }
            else
            {
                result = q == 0 ? c : q == 1 ? -s : q == 2 ? -c : s;
            }
            return RoundToDouble(result, p);
        }

        private static BigInteger AtanSmall(BigInteger x, int p)
        {
            BigInteger x2 = (x * x) >> p;
            BigInteger term = x;
            BigInteger sum = BigInteger.Zero;
            for (int n = 1; n < 4000; n += 2)
            {
                BigInteger t = term / n;
                if (((n / 2) & 1) == 0) sum += t;
                else sum -= t;
                term = (term * x2) >> p;
                if (term.IsZero) break;
            }
            return sum;
        }

        private static BigInteger AtanSeries(BigInteger x, int p)
        {
            BigInteger one = BigInteger.One << p;
            BigInteger lim = one >> 3;
            int k = 0;
            while (x > lim || x < -lim)
            {
                BigInteger root = ISqrt((one + ((x * x) >> p)) << p);
                x = (x << p) / (one + root);
                k++;
            }
            return AtanSmall(x, p) << k;
        }

        public static BigInteger AtanFixed(BigInteger q, int p)
        {
            BigInteger one = BigInteger.One << p;
            bool neg = q.Sign < 0;
            if (neg) q = -q;
            BigInteger r;
            if (q > one)
            {
                BigInteger inv = (one << p) / q;
                r = (Pi(p) >> 1) - AtanSeries(inv, p);
            }
            else
            {
                r = AtanSeries(q, p);
            }
            return neg ? -r : r;
        }

        public static double Atan(double x)
        {
            const int p = 360;
            BigInteger q = FromDouble(x, p);
            return RoundToDouble(AtanFixed(q, p), p);
        }

        public static double Atan2Positive(double ay, double ax, bool xNegative)
        {
            BigInteger my, mx;
            int ey, ex;
            Decompose(ay, out my, out ey);
            Decompose(ax, out mx, out ex);
            const int p = 360;
            int sh = p + ey - ex;
            BigInteger q = sh >= 0 ? (my << sh) / mx : my / (mx << (-sh));
            BigInteger a = AtanFixed(q, p);
            BigInteger r = xNegative ? Pi(p) - a : a;
            return RoundToDouble(r, p);
        }
    }
}
