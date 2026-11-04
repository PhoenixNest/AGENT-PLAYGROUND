using System;

namespace OrbitalSalvage.AudioDsp
{
    // Unity's Mono runtime returns transcendental results that differ from .NET and the CRT by up to ~1e-8
    // relative, which flips the compressor's k bisection. These versions use only IEEE +,-,*,/ and are
    // therefore bit-identical on every runtime.
    public static class Libm
    {
        const double Ln2Hi = 6.93147180369123816490e-01;
        const double Ln2Lo = 1.90821492927058770002e-10;
        const double InvLn2 = 1.44269504088896338700e+00;
        const double Ln2 = 0.6931471805599453;
        const double InvLn10 = 0.4342944819032518;
        const double Pio2 = 1.5707963267948966;
        const double Pio2Lo = 6.123233995736766e-17;
        const double Pio2_1 = 1.57079632673412561417e+00;
        const double Pio2_1t = 6.07710050650619224932e-11;
        const double Pio2_2 = 6.07710050630396597660e-11;
        const double Pio2_2t = 2.02226624879595063154e-21;
        const double InvPio2 = 6.36619772367581382433e-01;

        static readonly double[] ExpCoefficients = BuildInverseFactorials(24);
        static readonly double[] SinCoefficients = BuildTrigCoefficients(true, 14);
        static readonly double[] CosCoefficients = BuildTrigCoefficients(false, 14);

        static double[] BuildInverseFactorials(int count)
        {
            double[] c = new double[count];
            double f = 1.0;
            c[0] = 1.0;
            for (int i = 1; i < count; i++)
            {
                f *= i;
                c[i] = 1.0 / f;
            }

            return c;
        }

        static double[] BuildTrigCoefficients(bool sine, int count)
        {
            double[] c = new double[count];
            double f = 1.0;
            int first = sine ? 1 : 0;
            for (int i = 0; i < count; i++)
            {
                int n = first + 2 * i;
                f = 1.0;
                for (int j = 2; j <= n; j++)
                {
                    f *= j;
                }

                c[i] = ((i & 1) == 0 ? 1.0 : -1.0) / f;
            }

            return c;
        }

        static double PowerOfTwo(int n)
        {
            return BitConverter.Int64BitsToDouble((long)(n + 1023) << 52);
        }

        public static double Exp(double x)
        {
            if (double.IsNaN(x))
            {
                return x;
            }

            if (x > 709.782712893384)
            {
                return double.PositiveInfinity;
            }

            if (x < -745.2)
            {
                return 0.0;
            }

            double kd = Math.Floor(x * InvLn2 + 0.5);
            int k = (int)kd;
            double r = (x - kd * Ln2Hi) - kd * Ln2Lo;

            double sum = ExpCoefficients[23];
            for (int i = 22; i >= 0; i--)
            {
                sum = sum * r + ExpCoefficients[i];
            }

            if (k >= -1021 && k <= 1023)
            {
                return sum * PowerOfTwo(k);
            }

            int half = k / 2;
            return sum * PowerOfTwo(half) * PowerOfTwo(k - half);
        }

        public static double Log(double x)
        {
            if (double.IsNaN(x) || x < 0.0)
            {
                return double.NaN;
            }

            if (x == 0.0)
            {
                return double.NegativeInfinity;
            }

            if (double.IsPositiveInfinity(x))
            {
                return x;
            }

            int e = 0;
            if (x < 2.2250738585072014e-308)
            {
                x *= 18014398509481984.0;
                e -= 54;
            }

            long bits = BitConverter.DoubleToInt64Bits(x);
            e += (int)((bits >> 52) & 0x7FF) - 1023;
            double m = BitConverter.Int64BitsToDouble((bits & 0x000FFFFFFFFFFFFFL) | 0x3FF0000000000000L);
            if (m > 1.4142135623730951)
            {
                m *= 0.5;
                e++;
            }

            double s = (m - 1.0) / (m + 1.0);
            double s2 = s * s;
            double series = 1.0 / 31.0;
            for (int j = 14; j >= 0; j--)
            {
                series = series * s2 + 1.0 / (2 * j + 1);
            }

            double logM = 2.0 * s * series;
            double ed = e;
            return ed * Ln2Hi + (logM + ed * Ln2Lo);
        }

        public static double Log10(double x)
        {
            return Log(x) * InvLn10;
        }

        public static double Pow(double x, double y)
        {
            if (y == 0.0 || x == 1.0)
            {
                return 1.0;
            }

            if (!(x > 0.0) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
            {
                return Math.Pow(x, y);
            }

            if (x == 2.0)
            {
                double yi = Math.Floor(y);
                if (yi == y && yi > -1000.0 && yi < 1000.0)
                {
                    return PowerOfTwo((int)yi);
                }

                if (yi > -1000.0 && yi < 1000.0)
                {
                    return Exp((y - yi) * Ln2) * PowerOfTwo((int)yi);
                }
            }

            return Exp(y * Log(x));
        }

        static void ReduceHalfPi(double x, out int quadrant, out double r)
        {
            double nd = Math.Floor(x * InvPio2 + 0.5);
            quadrant = (int)((long)nd & 3L);
            double r1 = x - nd * Pio2_1;
            double r2 = r1 - nd * Pio2_2;
            r = r2 - nd * Pio2_2t;
        }
        static double SinKernel(double r)
        {
            double r2 = r * r;
            double sum = SinCoefficients[13];
            for (int i = 12; i >= 0; i--)
            {
                sum = sum * r2 + SinCoefficients[i];
            }

            return sum * r;
        }

        static double CosKernel(double r)
        {
            double r2 = r * r;
            double sum = CosCoefficients[13];
            for (int i = 12; i >= 0; i--)
            {
                sum = sum * r2 + CosCoefficients[i];
            }

            return sum;
        }

        public static double Sin(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
            {
                return double.NaN;
            }

            int q;
            double r;
            ReduceHalfPi(x, out q, out r);
            switch (q)
            {
                case 0: return SinKernel(r);
                case 1: return CosKernel(r);
                case 2: return -SinKernel(r);
                default: return -CosKernel(r);
            }
        }

        public static double Cos(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
            {
                return double.NaN;
            }

            int q;
            double r;
            ReduceHalfPi(x, out q, out r);
            switch (q)
            {
                case 0: return CosKernel(r);
                case 1: return -SinKernel(r);
                case 2: return -CosKernel(r);
                default: return SinKernel(r);
            }
        }

        public static double Asin(double x)
        {
            if (double.IsNaN(x) || x > 1.0 || x < -1.0)
            {
                return double.NaN;
            }

            if (x == 0.0)
            {
                return x;
            }

            double ax = Math.Abs(x);
            double result;
            if (ax > 0.5)
            {
                double z = Math.Sqrt((1.0 - ax) * 0.5);
                result = (Pio2 - 2.0 * AsinSmall(z)) + Pio2Lo;
            }
            else
            {
                result = AsinSmall(ax);
            }

            return x < 0.0 ? -result : result;
        }

        static double AsinSmall(double x)
        {
            double x2 = x * x;
            double term = x;
            double sum = x;
            for (int n = 1; n < 120; n++)
            {
                term *= x2 * ((2 * n - 1) * (2 * n - 1)) / ((2.0 * n) * (2.0 * n + 1));
                sum += term;
                if (term < 1e-18 * x)
                {
                    break;
                }
            }

            return sum;
        }
    }
}