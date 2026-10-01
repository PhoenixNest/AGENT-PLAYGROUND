using System;
using System.Collections.Generic;

namespace OrbitalSalvage.AudioDsp
{
    /// <summary>
    /// Band-limited wavetables. Port of Chromium PeriodicWaveHandler (modules/webaudio/periodic_wave_handler.cc).
    /// </summary>
    public sealed class PeriodicWave
    {
        const uint NumberOfOctaveBands = 3;
        const uint MaxPeriodicWaveSize = 16384;
        const float CentsPerRange = 1200f / NumberOfOctaveBands;

        static readonly Dictionary<long, PeriodicWave> Cache = new Dictionary<long, PeriodicWave>();

        readonly float[][] tables;
        readonly float centsPerRange = CentsPerRange;
        readonly float lowestFundamentalFrequency;

        public readonly float SampleRate;
        public readonly uint PeriodicWaveSize;
        public readonly uint NumberOfRanges;
        public readonly float RateScale;

        public static PeriodicWave Get(float sampleRate, OscillatorType type)
        {
            long key = ((long)BitConverter.SingleToInt32Bits(sampleRate) << 8) | (uint)type;
            lock (Cache)
            {
                PeriodicWave wave;
                if (!Cache.TryGetValue(key, out wave))
                {
                    wave = new PeriodicWave(sampleRate, type);
                    Cache[key] = wave;
                }

                return wave;
            }
        }

        PeriodicWave(float sampleRate, OscillatorType type)
        {
            SampleRate = sampleRate;
            float nyquist = 0.5f * sampleRate;
            PeriodicWaveSize = sampleRate <= 24000 ? 2048u : (sampleRate <= 88200 ? 4096u : MaxPeriodicWaveSize);
            lowestFundamentalFrequency = nyquist / (PeriodicWaveSize / 2);
            RateScale = PeriodicWaveSize / sampleRate;
            NumberOfRanges = (uint)(0.5f + NumberOfOctaveBands * Log2f(PeriodicWaveSize));
            tables = new float[NumberOfRanges][];
            Generate(type);
        }

        static float Log2f(float x)
        {
            return (float)(Libm.Log(x) / Libm.Log(2.0));
        }

        uint NumberOfPartialsForRange(uint rangeIndex)
        {
            float centsToCull = rangeIndex * centsPerRange;
            float cullingScale = (float)Libm.Pow(2, -centsToCull / 1200);
            return (uint)(cullingScale * (PeriodicWaveSize / 2));
        }

        void Generate(OscillatorType shape)
        {
            uint fftSize = PeriodicWaveSize;
            uint halfSize = fftSize / 2;
            float[] b = new float[halfSize];

            for (uint n = 1; n < halfSize; n++)
            {
                float piFactor = 2f / (n * MathF.PI);
                switch (shape)
                {
                    case OscillatorType.Sine:
                        b[n] = n == 1 ? 1f : 0f;
                        break;
                    case OscillatorType.Square:
                        b[n] = (n & 1) != 0 ? 2 * piFactor : 0f;
                        break;
                    case OscillatorType.Sawtooth:
                        b[n] = piFactor * ((n & 1) != 0 ? 1f : -1f);
                        break;
                    case OscillatorType.Triangle:
                        b[n] = (n & 1) != 0 ? 2 * (piFactor * piFactor) * ((((n - 1) >> 1) & 1) != 0 ? -1f : 1f) : 0f;
                        break;
                }
            }

            float normalizationScale = 0.5f;
            double[] re = new double[fftSize];
            double[] im = new double[fftSize];

            for (uint range = 0; range < NumberOfRanges; range++)
            {
                uint partials = NumberOfPartialsForRange(range);
                Array.Clear(re, 0, re.Length);
                Array.Clear(im, 0, im.Length);
                uint limit = Math.Min(halfSize, partials + 1);
                for (uint n = 1; n < limit; n++)
                {
                    im[n] = -b[n];
                    im[fftSize - n] = b[n];
                }

                InverseFftUnscaled(re, im);

                float[] table = new float[fftSize];
                for (int i = 0; i < fftSize; i++)
                {
                    table[i] = (float)re[i];
                }

                if (range == 0)
                {
                    float max = 0f;
                    for (int i = 0; i < fftSize; i++)
                    {
                        float a = MathF.Abs(table[i]);
                        if (a > max)
                        {
                            max = a;
                        }
                    }

                    if (max != 0f)
                    {
                        normalizationScale = 1.0f / max;
                    }
                }

                for (int i = 0; i < fftSize; i++)
                {
                    table[i] *= normalizationScale;
                }

                tables[range] = table;
            }
        }

        static void InverseFftUnscaled(double[] re, double[] im)
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
                    double t = re[i];
                    re[i] = re[j];
                    re[j] = t;
                    t = im[i];
                    im[i] = im[j];
                    im[j] = t;
                }
            }

            for (int len = 2; len <= n; len <<= 1)
            {
                double ang = 2 * Math.PI / len;
                double wr = Libm.Cos(ang);
                double wi = Libm.Sin(ang);
                for (int i = 0; i < n; i += len)
                {
                    double cr = 1;
                    double ci = 0;
                    for (int k = 0; k < len / 2; k++)
                    {
                        int a = i + k;
                        int c = i + k + len / 2;
                        double tr = re[c] * cr - im[c] * ci;
                        double ti = re[c] * ci + im[c] * cr;
                        re[c] = re[a] - tr;
                        im[c] = im[a] - ti;
                        re[a] += tr;
                        im[a] += ti;
                        double nr = cr * wr - ci * wi;
                        ci = cr * wi + ci * wr;
                        cr = nr;
                    }
                }
            }
        }

        public void WaveDataForFundamentalFrequency(float fundamentalFrequency, out float[] lower, out float[] higher, out float tableInterpolationFactor)
        {
            fundamentalFrequency = MathF.Abs(fundamentalFrequency);
            float ratio = fundamentalFrequency > 0 ? fundamentalFrequency / lowestFundamentalFrequency : 0.5f;
            float centsAboveLowest = Log2f(ratio) * 1200f;
            float pitchRange = 1 + centsAboveLowest / centsPerRange;
            pitchRange = Math.Min(Math.Max(pitchRange, 0f), (float)(NumberOfRanges - 1));

            uint index1 = (uint)pitchRange;
            uint index2 = index1 < NumberOfRanges - 1 ? index1 + 1 : index1;
            lower = tables[index2];
            higher = tables[index1];
            tableInterpolationFactor = pitchRange - index1;
        }
    }
}
