using System;
using System.Collections.Generic;

namespace OrbitalSalvage.AudioDsp
{
    /// <summary>
    /// Port of Chromium Biquad (platform/audio/biquad.cc) and BiquadFilterHandler (modules/webaudio/biquad_filter_handler.cc).
    /// </summary>
    sealed class Biquad
    {
        public readonly double[] B0 = new double[AudioMath.RenderQuantum];
        public readonly double[] B1 = new double[AudioMath.RenderQuantum];
        public readonly double[] B2 = new double[AudioMath.RenderQuantum];
        public readonly double[] A1 = new double[AudioMath.RenderQuantum];
        public readonly double[] A2 = new double[AudioMath.RenderQuantum];
        public bool HasSampleAccurateValues;

        double x1;
        double x2;
        double y1;
        double y2;

        public Biquad()
        {
            SetNormalizedCoefficients(0, 1, 0, 0, 1, 0, 0);
        }

        static double Pow10(double x)
        {
            return AudioMath.ExpF((float)(x * 2.302585092994046));
        }

        public void Process(float[] source, float[] dest, int frames)
        {
            if (HasSampleAccurateValues)
            {
                double lx1 = x1;
                double lx2 = x2;
                double ly1 = y1;
                double ly2 = y2;
                for (int k = 0; k < frames; k++)
                {
                    float x = source[k];
                    float y = (float)(B0[k] * x + B1[k] * lx1 + B2[k] * lx2 - A1[k] * ly1 - A2[k] * ly2);
                    dest[k] = y;
                    lx2 = lx1;
                    lx1 = x;
                    ly2 = ly1;
                    ly1 = y;
                }

                x1 = AudioMath.FlushDenormal(lx1);
                x2 = AudioMath.FlushDenormal(lx2);
                y1 = AudioMath.FlushDenormal(ly1);
                y2 = AudioMath.FlushDenormal(ly2);
            }
            else
            {
                double lx1 = x1;
                double lx2 = x2;
                double ly1 = y1;
                double ly2 = y2;
                double b0 = B0[0];
                double b1 = B1[0];
                double b2 = B2[0];
                double a1 = A1[0];
                double a2 = A2[0];
                for (int k = 0; k < frames; k++)
                {
                    float x = source[k];
                    float y = (float)(b0 * x + b1 * lx1 + b2 * lx2 - a1 * ly1 - a2 * ly2);
                    dest[k] = y;
                    lx2 = lx1;
                    lx1 = x;
                    ly2 = ly1;
                    ly1 = y;
                }

                x1 = AudioMath.FlushDenormal(lx1);
                x2 = AudioMath.FlushDenormal(lx2);
                y1 = AudioMath.FlushDenormal(ly1);
                y2 = AudioMath.FlushDenormal(ly2);
            }
        }

        public void SetLowpassParams(int index, double cutoff, double resonance)
        {
            cutoff = Math.Min(Math.Max(cutoff, 0.0), 1.0);
            if (cutoff == 1)
            {
                SetNormalizedCoefficients(index, 1, 0, 0, 1, 0, 0);
            }
            else if (cutoff > 0)
            {
                resonance = Pow10(resonance / 20);
                double theta = Math.PI * cutoff;
                double alpha = Libm.Sin(theta) / (2 * resonance);
                double cosw = Libm.Cos(theta);
                double beta = (1 - cosw) / 2;
                SetNormalizedCoefficients(index, beta, 2 * beta, beta, 1 + alpha, -2 * cosw, 1 - alpha);
            }
            else
            {
                SetNormalizedCoefficients(index, 0, 0, 0, 1, 0, 0);
            }
        }

        public void SetHighpassParams(int index, double cutoff, double resonance)
        {
            cutoff = Math.Min(Math.Max(cutoff, 0.0), 1.0);
            if (cutoff == 1)
            {
                SetNormalizedCoefficients(index, 0, 0, 0, 1, 0, 0);
            }
            else if (cutoff > 0)
            {
                resonance = Pow10(resonance / 20);
                double theta = Math.PI * cutoff;
                double alpha = Libm.Sin(theta) / (2 * resonance);
                double cosw = Libm.Cos(theta);
                double beta = (1 + cosw) / 2;
                SetNormalizedCoefficients(index, beta, -2 * beta, beta, 1 + alpha, -2 * cosw, 1 - alpha);
            }
            else
            {
                SetNormalizedCoefficients(index, 1, 0, 0, 1, 0, 0);
            }
        }

        public void SetBandpassParams(int index, double frequency, double q)
        {
            frequency = Math.Max(0.0, frequency);
            q = Math.Max(0.0, q);

            if (frequency > 0 && frequency < 1)
            {
                double w0 = Math.PI * frequency;
                if (q > 0)
                {
                    double alpha = Libm.Sin(w0) / (2 * q);
                    double k = Libm.Cos(w0);
                    SetNormalizedCoefficients(index, alpha, 0, -alpha, 1 + alpha, -2 * k, 1 - alpha);
                }
                else
                {
                    SetNormalizedCoefficients(index, 1, 0, 0, 1, 0, 0);
                }
            }
            else
            {
                SetNormalizedCoefficients(index, 0, 0, 0, 1, 0, 0);
            }
        }

        void SetNormalizedCoefficients(int index, double b0, double b1, double b2, double a0, double a1, double a2)
        {
            double a0Inverse = 1 / a0;
            B0[index] = b0 * a0Inverse;
            B1[index] = b1 * a0Inverse;
            B2[index] = b2 * a0Inverse;
            A1[index] = a1 * a0Inverse;
            A2[index] = a2 * a0Inverse;
        }

        public double TailFrame(int coefIndex, double maxFrame)
        {
            const double maxTailAmplitude = 1 / 32768.0;
            double a1 = A1[coefIndex];
            double a2 = A2[coefIndex];
            double b0 = B0[coefIndex];
            double b1 = B1[coefIndex];
            double b2 = B2[coefIndex];

            if (double.IsNaN(a1) || double.IsInfinity(a1) || double.IsNaN(a2) || double.IsInfinity(a2)
                || double.IsNaN(b0) || double.IsInfinity(b0) || double.IsNaN(b1) || double.IsInfinity(b1)
                || double.IsNaN(b2) || double.IsInfinity(b2))
            {
                return 0;
            }

            double tailFrame;
            double discrim = a1 * a1 - 4 * a2;

            if (discrim > 0)
            {
                double rplus = (-a1 + Math.Sqrt(discrim)) / 2;
                double rminus = (-a1 - Math.Sqrt(discrim)) / 2;
                double r1 = Math.Abs(rplus) >= Math.Abs(rminus) ? rplus : rminus;
                double r2 = a2 / r1;
                double c1 = (b0 * r1 * r1 + b1 * r1 + b2) / (r2 - r1);
                double c2 = (b0 * r2 * r2 + b1 * r2 + b2) / (r2 - r1);
                tailFrame = Math.Max(0, 1 + Libm.Log(maxTailAmplitude / (Math.Abs(c1) + Math.Abs(c2))) / Libm.Log(Math.Abs(r1)));
            }
            else if (discrim < 0)
            {
                double x = -a1 / 2;
                double y = Math.Sqrt(-discrim) / 2;
                double r = Math.Sqrt(x * x + y * y);
                if (r == 1)
                {
                    tailFrame = maxFrame;
                }
                else
                {
                    System.Numerics.Complex z1 = new System.Numerics.Complex(x, y);
                    System.Numerics.Complex z2 = new System.Numerics.Complex(x, -y);
                    double c1 = System.Numerics.Complex.Abs((b0 * z1 * z1 + b1 * z1 + b2) / (z2 - z1));
                    double c2 = System.Numerics.Complex.Abs((b0 * z2 * z2 + b1 * z2 + b2) / (z2 - z1));
                    tailFrame = 1 + Libm.Log(maxTailAmplitude / (c1 + c2)) / Libm.Log(r);
                    if (c1 == 0 && c2 == 0)
                    {
                        tailFrame = 0;
                    }
                }
            }
            else
            {
                double r = -a1 / 2;
                if (r == 0)
                {
                    tailFrame = 2;
                }
                else if (Math.Abs(r) >= 1)
                {
                    tailFrame = double.PositiveInfinity;
                }
                else
                {
                    tailFrame = maxFrame;
                }
            }

            return tailFrame;
        }

        public void Reset()
        {
            x1 = x2 = y1 = y2 = 0;
        }
    }

    public sealed class BiquadFilterNode : AudioNodeDsp
    {
        const double MaxTailTime = 30.0;

        readonly float[] cutoffValues = new float[AudioMath.RenderQuantum];
        readonly float[] qValues = new float[AudioMath.RenderQuantum];
        readonly float[] gainValues = new float[AudioMath.RenderQuantum];
        readonly float[] detuneValues = new float[AudioMath.RenderQuantum];
        readonly List<Biquad> biquads = new List<Biquad>();
        readonly double nyquist;
        BiquadFilterType type = BiquadFilterType.Lowpass;
        bool hasJustReset = true;
        float prevCutoff = float.NaN;
        float prevQ = float.NaN;
        float prevGain = float.NaN;
        float prevDetune = float.NaN;
        double tailTime;

        public readonly AudioParam Frequency;
        public readonly AudioParam Q;
        public readonly AudioParam Gain;
        public readonly AudioParam Detune;

        public BiquadFilterType Type
        {
            get { return type; }
            set
            {
                Ctx.Trace?.Event("node.set", Id, "type", TypeName(value));
                type = value;
                Ctx.Mutate(() => type = value);
            }
        }

        protected override double TailTime
        {
            get { return tailTime; }
        }

        protected override bool OutputFollowsInput
        {
            get { return true; }
        }

        internal BiquadFilterNode(AudioContextDsp ctx, int id)
            : base(ctx, "biquad", id, 1)
        {
            nyquist = 0.5 * ctx.SampleRate;
            float nyq = ctx.SampleRate / 2;
            Frequency = new AudioParam(ctx, id, "frequency", 350f, 0f, nyq, true);
            Q = new AudioParam(ctx, id, "Q", 1f, -float.MaxValue, float.MaxValue, true);
            Gain = new AudioParam(ctx, id, "gain", 0f, -float.MaxValue, 1541.2736f, true);
            Detune = new AudioParam(ctx, id, "detune", 0f, -153600f, 153600f, true);
        }

        static string TypeName(BiquadFilterType t)
        {
            switch (t)
            {
                case BiquadFilterType.Highpass:
                    return "highpass";
                case BiquadFilterType.Bandpass:
                    return "bandpass";
                default:
                    return "lowpass";
            }
        }

        static bool HasConstantValues(float[] values, int n)
        {
            for (int i = 1; i < n; i++)
            {
                if (values[i] != values[0])
                {
                    return false;
                }
            }

            return true;
        }

        static double NormalizeFrequency(float frequency, double nyquist, float detune)
        {
            double normalized = frequency / nyquist;
            return detune != 0f ? normalized * Libm.Pow(2.0, detune / 1200f) : normalized;
        }

        void SetParams(Biquad b, int index, double frequency, double q, double gain)
        {
            switch (type)
            {
                case BiquadFilterType.Lowpass:
                    b.SetLowpassParams(index, frequency, q);
                    break;
                case BiquadFilterType.Highpass:
                    b.SetHighpassParams(index, frequency, q);
                    break;
                case BiquadFilterType.Bandpass:
                    b.SetBandpassParams(index, frequency, q);
                    break;
            }
        }

        protected override void ProcessOnlyAudioParams()
        {
            Frequency.CalculateSampleAccurateValues(cutoffValues, AudioMath.RenderQuantum);
            Q.CalculateSampleAccurateValues(qValues, AudioMath.RenderQuantum);
            Gain.CalculateSampleAccurateValues(gainValues, AudioMath.RenderQuantum);
            Detune.CalculateSampleAccurateValues(detuneValues, AudioMath.RenderQuantum);
        }

        protected override void Process()
        {
            int channels = In.ChannelCount;
            Out.SetChannelCount(channels);
            if (biquads.Count != channels)
            {
                biquads.Clear();
                for (int i = 0; i < channels; i++)
                {
                    biquads.Add(new Biquad());
                }

                hasJustReset = true;
            }

            bool dirty = false;
            bool hasSampleAccurate = false;
            bool isAudioRate = false;

            if (Frequency.HasSampleAccurateValues() || Q.HasSampleAccurateValues() || Gain.HasSampleAccurateValues() || Detune.HasSampleAccurateValues())
            {
                dirty = true;
                hasSampleAccurate = true;
                isAudioRate = Frequency.IsAudioRate || Q.IsAudioRate || Gain.IsAudioRate || Detune.IsAudioRate;
            }
            else if (hasJustReset)
            {
                prevCutoff = float.NaN;
                prevQ = float.NaN;
                prevGain = float.NaN;
                prevDetune = float.NaN;
                dirty = true;
                hasJustReset = false;
            }
            else
            {
                float cutoff = Frequency.FinalValue();
                float q = Q.FinalValue();
                float gain = Gain.FinalValue();
                float detune = Detune.FinalValue();
                if (prevCutoff != cutoff || prevQ != q || prevGain != gain || prevDetune != detune)
                {
                    dirty = true;
                    prevCutoff = cutoff;
                    prevQ = q;
                    prevGain = gain;
                    prevDetune = detune;
                }
            }

            if (dirty)
            {
                int frames = AudioMath.RenderQuantum;
                if (hasSampleAccurate && isAudioRate)
                {
                    Frequency.CalculateSampleAccurateValues(cutoffValues, frames);
                    Q.CalculateSampleAccurateValues(qValues, frames);
                    Gain.CalculateSampleAccurateValues(gainValues, frames);
                    Detune.CalculateSampleAccurateValues(detuneValues, frames);

                    bool isConstant = HasConstantValues(cutoffValues, frames) && HasConstantValues(qValues, frames)
                        && HasConstantValues(gainValues, frames) && HasConstantValues(detuneValues, frames);
                    int needed = isConstant ? 1 : frames;
                    foreach (Biquad b in biquads)
                    {
                        b.HasSampleAccurateValues = needed > 1;
                        for (int k = 0; k < needed; k++)
                        {
                            double nf = NormalizeFrequency(cutoffValues[k], nyquist, detuneValues[k]);
                            SetParams(b, k, nf, qValues[k], gainValues[k]);
                        }
                    }

                    double tail = biquads[0].TailFrame(needed - 1, MaxTailTime * Ctx.SampleRate) / Ctx.SampleRate;
                    tailTime = Math.Min(Math.Max(tail, 0.0), MaxTailTime);
                }
                else
                {
                    cutoffValues[0] = Frequency.FinalValue();
                    qValues[0] = Q.FinalValue();
                    gainValues[0] = Gain.FinalValue();
                    detuneValues[0] = Detune.FinalValue();
                    foreach (Biquad b in biquads)
                    {
                        b.HasSampleAccurateValues = false;
                        double nf = NormalizeFrequency(cutoffValues[0], nyquist, detuneValues[0]);
                        SetParams(b, 0, nf, qValues[0], gainValues[0]);
                    }

                    double tail = biquads[0].TailFrame(0, MaxTailTime * Ctx.SampleRate) / Ctx.SampleRate;
                    tailTime = Math.Min(Math.Max(tail, 0.0), MaxTailTime);
                }
            }

            for (int i = 0; i < biquads.Count; i++)
            {
                biquads[i].Process(In.Channels[i], Out.Channels[i], AudioMath.RenderQuantum);
            }
        }
    }
}
