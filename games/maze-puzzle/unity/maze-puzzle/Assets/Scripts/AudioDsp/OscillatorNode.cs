using System;

namespace OrbitalSalvage.AudioDsp
{
    /// <summary>
    /// Port of Chromium OscillatorHandler (modules/webaudio/oscillator_handler.cc).
    /// </summary>
    public sealed class OscillatorNode : AudioScheduledSourceNode
    {
        const float Interpolate2Point = 0.3f;
        const float Interpolate3Point = 0.16f;

        readonly float[] phaseIncrements = new float[AudioMath.RenderQuantum];
        readonly float[] detuneValues = new float[AudioMath.RenderQuantum];
        PeriodicWave wave;
        OscillatorType type = OscillatorType.Sine;
        double virtualReadIndex;

        public readonly AudioParam Frequency;
        public readonly AudioParam Detune;

        public OscillatorType Type
        {
            get { return type; }
            set
            {
                Ctx.Trace?.Event("node.set", Id, "type", TypeName(value));
                type = value;
                Ctx.Mutate(() => wave = PeriodicWave.Get(Ctx.SampleRate, value));
            }
        }

        internal OscillatorNode(AudioContextDsp ctx, int id)
            : base(ctx, "oscillator", id, 1)
        {
            float nyquist = ctx.SampleRate / 2;
            Frequency = new AudioParam(ctx, id, "frequency", 440f, -nyquist, nyquist, true);
            Detune = new AudioParam(ctx, id, "detune", 0f, -153600f, 153600f, true);
            wave = PeriodicWave.Get(ctx.SampleRate, OscillatorType.Sine);
        }

        public static string TypeName(OscillatorType t)
        {
            switch (t)
            {
                case OscillatorType.Square:
                    return "square";
                case OscillatorType.Sawtooth:
                    return "sawtooth";
                case OscillatorType.Triangle:
                    return "triangle";
                default:
                    return "sine";
            }
        }

        protected override bool PropagatesSilence()
        {
            return !IsPlayingOrScheduled || State == PlaybackState.Finished || wave == null;
        }

        protected override void ProcessOnlyAudioParams()
        {
        }

        static float DetuneToFrequencyMultiplier(float detune)
        {
            return (float)Libm.Pow(2.0, detune / 1200f);
        }

        static void ClampFrequency(float[] values, int n, float nyquist)
        {
            for (int i = 0; i < n; i++)
            {
                float f = values[i];
                values[i] = float.IsNaN(f) ? nyquist : Math.Min(Math.Max(f, -nyquist), nyquist);
            }
        }

        bool CalculateSampleAccuratePhaseIncrements(int frames)
        {
            bool hasSampleAccurate = false;
            bool hasFrequencyChanges = false;
            float finalScale = wave.RateScale;

            if (Frequency.HasSampleAccurateValues() && Frequency.IsAudioRate)
            {
                hasSampleAccurate = true;
                hasFrequencyChanges = true;
                Frequency.CalculateSampleAccurateValues(phaseIncrements, frames);
            }
            else
            {
                finalScale *= Frequency.FinalValue();
            }

            if (Detune.HasSampleAccurateValues() && Detune.IsAudioRate)
            {
                hasSampleAccurate = true;
                float[] det = hasFrequencyChanges ? detuneValues : phaseIncrements;
                Detune.CalculateSampleAccurateValues(det, frames);
                float k = 1.0f / 1200f;
                for (int i = 0; i < frames; i++)
                {
                    det[i] = (float)Libm.Pow(2.0, det[i] * k);
                }

                if (hasFrequencyChanges)
                {
                    for (int i = 0; i < frames; i++)
                    {
                        phaseIncrements[i] = detuneValues[i] * phaseIncrements[i];
                    }
                }
            }
            else
            {
                finalScale *= DetuneToFrequencyMultiplier(Detune.FinalValue());
            }

            if (hasSampleAccurate)
            {
                ClampFrequency(phaseIncrements, frames, Ctx.SampleRate / 2);
                for (int i = 0; i < frames; i++)
                {
                    phaseIncrements[i] = phaseIncrements[i] * finalScale;
                }
            }

            return hasSampleAccurate;
        }

        static float DoInterpolation(double virtualReadIndex, float incr, uint readIndexMask, float tableFactor, float[] lowerData, float[] higherData)
        {
            double sampleLower = 0;
            double sampleHigher = 0;
            uint readIndex0 = (uint)virtualReadIndex;

            if (incr >= Interpolate2Point)
            {
                uint readIndex2 = readIndex0 + 1;
                readIndex0 &= readIndexMask;
                readIndex2 &= readIndexMask;

                float s1l = lowerData[readIndex0];
                float s2l = lowerData[readIndex2];
                float s1h = higherData[readIndex0];
                float s2h = higherData[readIndex2];

                double factor = (float)virtualReadIndex - readIndex0;
                sampleHigher = (1 - factor) * s1h + factor * s2h;
                sampleLower = (1 - factor) * s1l + factor * s2l;
            }
            else if (incr >= Interpolate3Point)
            {
                uint r0 = (readIndex0 - 1) & readIndexMask;
                uint r1 = readIndex0 & readIndexMask;
                uint r2 = (readIndex0 + 1) & readIndexMask;
                double t = virtualReadIndex - readIndex0;
                double a0 = 0.5 * t * (t - 1);
                double a1 = 1 - t * t;
                double a2 = 0.5 * t * (t + 1);
                sampleLower = a0 * lowerData[r0] + a1 * lowerData[r1] + a2 * lowerData[r2];
                sampleHigher = a0 * higherData[r0] + a1 * higherData[r1] + a2 * higherData[r2];
            }
            else
            {
                double t = virtualReadIndex - readIndex0;
                double t2 = t * t;
                double a0 = t * (t2 - 1) * (t - 2) / 24;
                double a1 = -t * (t - 1) * (t2 - 4) / 6;
                double a2 = (t2 - 1) * (t2 - 4) / 4;
                double a3 = -t * (t + 1) * (t2 - 4) / 6;
                double a4 = t * (t2 - 1) * (t + 2) / 24;
                double[] a = { a0, a1, a2, a3, a4 };
                for (int k = 0; k < 5; k++)
                {
                    uint ri = (uint)((int)readIndex0 + k - 2) & readIndexMask;
                    sampleLower += a[k] * lowerData[ri];
                    sampleHigher += a[k] * higherData[ri];
                }
            }

            return (float)((1 - tableFactor) * sampleHigher + tableFactor * sampleLower);
        }

        double ProcessKRate(int n, float[] dest, int destIndex, double vri)
        {
            uint size = wave.PeriodicWaveSize;
            double invSize = 1.0 / size;
            uint mask = size - 1;

            float frequency = Frequency.FinalValue();
            frequency *= DetuneToFrequencyMultiplier(Detune.FinalValue());
            ClampOne(ref frequency, Ctx.SampleRate / 2);

            float[] lower;
            float[] higher;
            float factor;
            wave.WaveDataForFundamentalFrequency(frequency, out lower, out higher, out factor);

            float rateScale = wave.RateScale;
            float incr = frequency * rateScale;

            if (incr >= Interpolate2Point)
            {
                double v = vri;
                for (int k = 0; k < n; k++)
                {
                    uint r0 = (uint)v & mask;
                    uint r1 = (r0 + 1) & mask;
                    float s1l = lower[r0];
                    float s2l = lower[r1];
                    float s1h = higher[r0];
                    float s2h = higher[r1];
                    float f = (float)v - r0;
                    float sh = s1h + f * (s2h - s1h);
                    float sl = s1l + f * (s2l - s1l);
                    dest[destIndex + k] = sh + factor * (sl - sh);
                    v += incr;
                    v -= Math.Floor(v * invSize) * size;
                }

                vri += n * incr;
                vri -= Math.Floor(vri * invSize) * size;
            }
            else
            {
                for (int k = 0; k < n; k++)
                {
                    dest[destIndex + k] = DoInterpolation(vri, Math.Abs(incr), mask, factor, lower, higher);
                    vri += incr;
                    vri -= Math.Floor(vri * invSize) * size;
                }
            }

            return vri;
        }

        static void ClampOne(ref float f, float nyquist)
        {
            f = float.IsNaN(f) ? nyquist : Math.Min(Math.Max(f, -nyquist), nyquist);
        }

        double ProcessARate(int n, float[] dest, int destIndex, double vri)
        {
            float rateScale = wave.RateScale;
            float invRateScale = 1 / rateScale;
            uint size = wave.PeriodicWaveSize;
            double invSize = 1.0 / size;
            uint mask = size - 1;

            for (int k = 0; k < n; k++)
            {
                float incr = phaseIncrements[k];
                float frequency = invRateScale * incr;
                float[] lower;
                float[] higher;
                float factor;
                wave.WaveDataForFundamentalFrequency(frequency, out lower, out higher, out factor);
                dest[destIndex + k] = DoInterpolation(vri, Math.Abs(incr), mask, factor, lower, higher);
                vri += incr;
                vri -= Math.Floor(vri * invSize) * size;
            }

            return vri;
        }

        protected override void Process()
        {
            if (wave == null)
            {
                Out.Zero();
                return;
            }

            int quantumFrameOffset;
            int nonSilent;
            double startFrameOffset;
            UpdateSchedulingInfo(Out, out quantumFrameOffset, out nonSilent, out startFrameOffset);

            if (nonSilent == 0)
            {
                Out.Zero();
                return;
            }

            uint size = wave.PeriodicWaveSize;
            double vri = virtualReadIndex;
            float rateScale = wave.RateScale;
            bool hasSampleAccurate = CalculateSampleAccuratePhaseIncrements(AudioMath.RenderQuantum);

            float frequency = 0;
            if (!hasSampleAccurate)
            {
                frequency = Frequency.FinalValue();
                float detune = Detune.FinalValue();
                frequency *= DetuneToFrequencyMultiplier(detune);
                ClampOne(ref frequency, Ctx.SampleRate / 2);
            }

            int destIndex = quantumFrameOffset;
            int n = nonSilent;

            if (startFrameOffset > 0)
            {
                ++destIndex;
                --n;
                vri += (1 - startFrameOffset) * frequency * rateScale;
            }
            else if (startFrameOffset < 0)
            {
                vri = -startFrameOffset * frequency * rateScale;
            }

            float[] dest = Out.Channels[0];
            if (hasSampleAccurate)
            {
                vri = ProcessARate(n, dest, destIndex, vri);
            }
            else
            {
                vri = ProcessKRate(n, dest, destIndex, vri);
            }

            virtualReadIndex = vri;
            Out.Silent = false;
        }
    }
}
