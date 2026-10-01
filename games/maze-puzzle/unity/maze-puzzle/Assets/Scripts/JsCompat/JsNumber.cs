using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace OrbitalSalvage.JsCompat
{
    public static class JsNumber
    {
        public static string ToString(double x)
        {
            if (double.IsNaN(x)) return "NaN";
            if (x == 0.0) return "0";
            if (double.IsInfinity(x)) return x > 0 ? "Infinity" : "-Infinity";

            bool negative = x < 0;
            double v = negative ? -x : x;

            string digits;
            int exponent10;
            ShortestDigits(v, out digits, out exponent10);

            int k = digits.Length;
            int n = exponent10 + 1;
            var sb = new StringBuilder();
            if (negative) sb.Append('-');
            if (k <= n && n <= 21)
            {
                sb.Append(digits);
                sb.Append('0', n - k);
            }
            else if (0 < n && n <= 21)
            {
                sb.Append(digits, 0, n);
                sb.Append('.');
                sb.Append(digits, n, k - n);
            }
            else if (-6 < n && n <= 0)
            {
                sb.Append("0.");
                sb.Append('0', -n);
                sb.Append(digits);
            }
            else
            {
                int e = n - 1;
                sb.Append(digits[0]);
                if (k > 1)
                {
                    sb.Append('.');
                    sb.Append(digits, 1, k - 1);
                }
                sb.Append('e');
                sb.Append(e < 0 ? '-' : '+');
                sb.Append(Math.Abs(e).ToString(CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        // Burger-Dybvig free-format with exact integers; Mono's "E" formatting and Parse are not correctly rounded.
        private static void ShortestDigits(double v, out string digits, out int exponent10)
        {
            long bits = BitConverter.DoubleToInt64Bits(v);
            int biased = (int)((bits >> 52) & 0x7FF);
            long frac = bits & 0xFFFFFFFFFFFFFL;
            BigInteger m;
            int e;
            if (biased == 0)
            {
                m = frac;
                e = -1074;
            }
            else
            {
                m = frac | (1L << 52);
                e = biased - 1075;
            }
            bool lowerBoundaryCloser = frac == 0 && biased > 1;
            bool even = m.IsEven;

            BigInteger r, s, mp, mm;
            if (e >= 0)
            {
                BigInteger be = BigInteger.Pow(2, e);
                if (!lowerBoundaryCloser)
                {
                    r = m * be * 2; s = 2; mp = be; mm = be;
                }
                else
                {
                    r = m * be * 4; s = 4; mp = be * 2; mm = be;
                }
            }
            else
            {
                if (!lowerBoundaryCloser)
                {
                    r = m * 2; s = BigInteger.Pow(2, -e) * 2; mp = 1; mm = 1;
                }
                else
                {
                    r = m * 4; s = BigInteger.Pow(2, -e + 1) * 2; mp = 2; mm = 1;
                }
            }

            int k = (int)Math.Ceiling(Math.Log10(v) - 1e-10);
            if (k >= 0)
            {
                s *= BigInteger.Pow(10, k);
            }
            else
            {
                BigInteger scale = BigInteger.Pow(10, -k);
                r *= scale; mp *= scale; mm *= scale;
            }

            while (even ? r + mp >= s : r + mp > s)
            {
                s *= 10;
                k++;
            }
            while (even ? (r + mp) * 10 < s : (r + mp) * 10 <= s)
            {
                r *= 10; mp *= 10; mm *= 10;
                k--;
            }

            var sb = new StringBuilder();
            while (true)
            {
                r *= 10; mp *= 10; mm *= 10;
                BigInteger rem;
                int d = (int)BigInteger.DivRem(r, s, out rem);
                r = rem;
                bool low = even ? r <= mm : r < mm;
                bool high = even ? r + mp >= s : r + mp > s;
                if (!low && !high)
                {
                    sb.Append((char)('0' + d));
                    continue;
                }
                if (low && !high)
                {
                }
                else if (!low)
                {
                    d++;
                }
                else
                {
                    int cmp = (r * 2).CompareTo(s);
                    if (cmp > 0 || (cmp == 0 && (d & 1) == 1)) d++;
                }
                sb.Append((char)('0' + d));
                break;
            }

            digits = sb.ToString();
            exponent10 = k - 1;
        }
    }
}
