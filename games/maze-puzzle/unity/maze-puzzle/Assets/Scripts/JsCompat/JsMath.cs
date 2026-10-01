using System;

namespace OrbitalSalvage.JsCompat
{
    public static partial class JsMath
    {
        public const double PI = Math.PI;

        public static long Bits(double x)
        {
            return BitConverter.DoubleToInt64Bits(x);
        }

        public static double FromBits(long bits)
        {
            return BitConverter.Int64BitsToDouble(bits);
        }

        internal static int HighWord(double x)
        {
            return unchecked((int)(BitConverter.DoubleToInt64Bits(x) >> 32));
        }

        internal static uint LowWord(double x)
        {
            return unchecked((uint)BitConverter.DoubleToInt64Bits(x));
        }

        internal static double FromWords(int hi, uint lo)
        {
            return BitConverter.Int64BitsToDouble(unchecked(((long)hi << 32) | lo));
        }

        public static double CopySign(double magnitude, double sign)
        {
            long m = BitConverter.DoubleToInt64Bits(magnitude) & 0x7FFFFFFFFFFFFFFFL;
            long s = BitConverter.DoubleToInt64Bits(sign) & unchecked((long)0x8000000000000000UL);
            return BitConverter.Int64BitsToDouble(m | s);
        }

        public static double Abs(double x)
        {
            return BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(x) & 0x7FFFFFFFFFFFFFFFL);
        }

        public static double Sqrt(double x)
        {
            return Math.Sqrt(x);
        }

        public static double Floor(double x)
        {
            return Math.Floor(x);
        }

        public static double Ceil(double x)
        {
            return Math.Ceiling(x);
        }

        public static double Trunc(double x)
        {
            return Math.Truncate(x);
        }

        public static double Sign(double x)
        {
            if (double.IsNaN(x)) return double.NaN;
            if (x > 0) return 1.0;
            if (x < 0) return -1.0;
            return x;
        }

        public static double Min(double a, double b)
        {
            if (double.IsNaN(a) || double.IsNaN(b)) return double.NaN;
            if (a == 0.0 && b == 0.0)
            {
                return BitConverter.DoubleToInt64Bits(a) < 0 ? a : b;
            }
            return a < b ? a : b;
        }

        public static double Max(double a, double b)
        {
            if (double.IsNaN(a) || double.IsNaN(b)) return double.NaN;
            if (a == 0.0 && b == 0.0)
            {
                return BitConverter.DoubleToInt64Bits(a) < 0 ? b : a;
            }
            return a > b ? a : b;
        }

        public static double Round(double x)
        {
            double r = Math.Ceiling(x);
            if (r - 0.5 > x) r -= 1.0;
            return r;
        }

        public static double Mod(double a, double b)
        {
            return a % b;
        }

        public static int ToInt32(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x)) return 0;
            if (x >= -2147483648.0 && x <= 2147483647.0) return (int)x;
            double t = Math.Truncate(x);
            double m = t % 4294967296.0;
            if (m < 0) m += 4294967296.0;
            return unchecked((int)(uint)m);
        }

        public static uint ToUint32(double x)
        {
            return unchecked((uint)ToInt32(x));
        }

        public static int Imul(int a, int b)
        {
            return unchecked(a * b);
        }

        public static double Hypot(double x, double y)
        {
            double a0 = 0.0, a1 = 0.0;
            bool nan = false;
            double max = 0.0;
            if (double.IsNaN(x)) nan = true;
            else
            {
                a0 = Abs(x);
                if (a0 > max) max = a0;
            }
            if (double.IsNaN(y)) nan = true;
            else
            {
                a1 = Abs(y);
                if (a1 > max) max = a1;
            }
            if (max == double.PositiveInfinity) return double.PositiveInfinity;
            if (nan) return double.NaN;
            if (max == 0.0) return 0.0;

            double sum = 0.0;
            double compensation = 0.0;
            double n = a0 / max;
            double summand = n * n - compensation;
            double preliminary = sum + summand;
            compensation = (preliminary - sum) - summand;
            sum = preliminary;

            n = a1 / max;
            summand = n * n - compensation;
            preliminary = sum + summand;
            compensation = (preliminary - sum) - summand;
            sum = preliminary;

            return Math.Sqrt(sum) * max;
        }

        public static double Hypot(params double[] values)
        {
            int length = values.Length;
            if (length == 0) return 0.0;
            var abs = new double[length];
            bool nan = false;
            double max = 0.0;
            for (int i = 0; i < length; i++)
            {
                double v = values[i];
                if (double.IsNaN(v)) nan = true;
                else
                {
                    double a = Abs(v);
                    abs[i] = a;
                    if (a > max) max = a;
                }
            }
            if (max == double.PositiveInfinity) return double.PositiveInfinity;
            if (nan) return double.NaN;
            if (max == 0.0) return 0.0;

            double sum = 0.0;
            double compensation = 0.0;
            for (int i = 0; i < length; i++)
            {
                double n = abs[i] / max;
                double summand = n * n - compensation;
                double preliminary = sum + summand;
                compensation = (preliminary - sum) - summand;
                sum = preliminary;
            }
            return Math.Sqrt(sum) * max;
        }

        public static double Pow(double x, double y)
        {
            if (double.IsNaN(y)) return double.NaN;
            if (double.IsInfinity(y) && (x == 1.0 || x == -1.0)) return double.NaN;
            if (double.IsNaN(x)) x = double.NaN;
            if (y == 2.0) return x * x;
            if (y == 0.5)
            {
                if (double.IsInfinity(x)) return double.PositiveInfinity;
                return Math.Sqrt(x + 0.0);
            }
            return Math.Pow(x, y);
        }
    }
}
