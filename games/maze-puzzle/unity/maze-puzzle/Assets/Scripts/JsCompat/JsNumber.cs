using System;
using System.Globalization;
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

            string digits = null;
            int exponent10 = 0;
            for (int p = 1; p <= 17; p++)
            {
                string s = v.ToString("E" + (p - 1).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                double back = double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (back == v || p == 17)
                {
                    int ePos = s.IndexOf('E');
                    string mant = s.Substring(0, ePos).Replace(".", "");
                    exponent10 = int.Parse(s.Substring(ePos + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                    digits = mant.TrimEnd('0');
                    if (digits.Length == 0) digits = "0";
                    break;
                }
            }

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
    }
}
