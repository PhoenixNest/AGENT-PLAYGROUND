using System;

namespace OrbitalSalvage.AudioDsp.Tests
{
    public struct AudioComparison
    {
        public double Correlation;
        public double MaxAbsError;
        public double RmsError;
        public double StftDistance;
        public double ReferenceRms;
        public int Frames;
    }

    public static class AudioMetrics
    {
        const int FftSize = 512;
        const int Hop = 256;

        public static AudioComparison Compare(float[][] reference, float[][] actual)
        {
            AudioComparison r = new AudioComparison();
            double dot = 0;
            double ea = 0;
            double eb = 0;
            double sq = 0;
            double max = 0;
            long n = 0;
            int channels = Math.Min(reference.Length, actual.Length);
            int frames = Math.Min(reference[0].Length, actual[0].Length);

            for (int c = 0; c < channels; c++)
            {
                for (int i = 0; i < frames; i++)
                {
                    double a = reference[c][i];
                    double b = actual[c][i];
                    dot += a * b;
                    ea += a * a;
                    eb += b * b;
                    double d = a - b;
                    sq += d * d;
                    max = Math.Max(max, Math.Abs(d));
                    n++;
                }
            }

            r.Frames = frames;
            r.ReferenceRms = n > 0 ? Math.Sqrt(ea / n) : 0;
            r.Correlation = ea == 0 && eb == 0 ? 1.0 : (ea == 0 || eb == 0 ? 0.0 : dot / Math.Sqrt(ea * eb));
            r.MaxAbsError = max;
            r.RmsError = n > 0 ? Math.Sqrt(sq / n) : 0;
            r.StftDistance = StftMagnitudeDistance(reference[0], actual[0], frames);
            return r;
        }

        public static double StftMagnitudeDistance(float[] a, float[] b, int frames)
        {
            double[] window = new double[FftSize];
            for (int i = 0; i < FftSize; i++)
            {
                window[i] = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / FftSize);
            }

            double num = 0;
            double den = 0;
            double[] ra = new double[FftSize];
            double[] ia = new double[FftSize];
            double[] rb = new double[FftSize];
            double[] ib = new double[FftSize];

            for (int start = 0; start + FftSize <= frames; start += Hop)
            {
                for (int i = 0; i < FftSize; i++)
                {
                    ra[i] = a[start + i] * window[i];
                    rb[i] = b[start + i] * window[i];
                    ia[i] = 0;
                    ib[i] = 0;
                }

                Fft(ra, ia);
                Fft(rb, ib);
                for (int k = 0; k <= FftSize / 2; k++)
                {
                    double ma = Math.Sqrt(ra[k] * ra[k] + ia[k] * ia[k]);
                    double mb = Math.Sqrt(rb[k] * rb[k] + ib[k] * ib[k]);
                    num += (ma - mb) * (ma - mb);
                    den += ma * ma;
                }
            }

            if (den == 0)
            {
                return num == 0 ? 0 : double.PositiveInfinity;
            }

            return Math.Sqrt(num / den);
        }

        static void Fft(double[] re, double[] im)
        {
            int n = re.Length;
            for (int i = 1, j = 0; i < n; i++)
            {
                int bit = n >> 1;
                for (; (j & bit) != 0; bit >>= 1)
                {
                    j ^= bit;
                }

                j ^= bit;
                if (i < j)
                {
                    double tr = re[i];
                    re[i] = re[j];
                    re[j] = tr;
                    double ti = im[i];
                    im[i] = im[j];
                    im[j] = ti;
                }
            }

            for (int len = 2; len <= n; len <<= 1)
            {
                double ang = -2 * Math.PI / len;
                double wr = Math.Cos(ang);
                double wi = Math.Sin(ang);
                for (int i = 0; i < n; i += len)
                {
                    double cr = 1;
                    double ci = 0;
                    for (int k = 0; k < len / 2; k++)
                    {
                        int u = i + k;
                        int v = i + k + len / 2;
                        double xr = re[v] * cr - im[v] * ci;
                        double xi = re[v] * ci + im[v] * cr;
                        re[v] = re[u] - xr;
                        im[v] = im[u] - xi;
                        re[u] += xr;
                        im[u] += xi;
                        double ncr = cr * wr - ci * wi;
                        ci = cr * wi + ci * wr;
                        cr = ncr;
                    }
                }
            }
        }
    }
}
