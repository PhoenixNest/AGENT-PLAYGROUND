using System;

namespace OrbitalSalvage.AudioDsp
{
    public interface IRandomSource
    {
        double NextDouble();
    }

    public interface IAudioTraceSink
    {
        void Event(string name, params object[] args);
    }

    public enum OscillatorType
    {
        Sine = 0,
        Square = 1,
        Sawtooth = 2,
        Triangle = 3,
    }

    public enum BiquadFilterType
    {
        Lowpass,
        Highpass,
        Bandpass,
    }

    public static class AudioMath
    {
        public const int RenderQuantum = 128;

        public static float DecibelsToLinear(float decibels)
        {
            return PowF(10f, (float)(0.05f * decibels));
        }

        public static float LinearToDecibels(float linear)
        {
            return (float)(20f * (float)Libm.Log10(linear));
        }

        public static float PowF(float x, float y)
        {
            return (float)Libm.Pow(x, y);
        }

        public static float ExpF(float x)
        {
            return (float)Libm.Exp(x);
        }

        public static float AsinF(float x)
        {
            return (float)Libm.Asin(x);
        }

        public static double DiscreteTimeConstant(double timeConstant, double sampleRate)
        {
            return 1 - Libm.Exp(-1 / (sampleRate * timeConstant));
        }

        public static long TimeToSampleFrameCeil(double time, double sampleRate)
        {
            const double oversample = 1024;
            double frame = Math.Round(time * sampleRate * oversample, MidpointRounding.AwayFromZero) / oversample;
            return (long)Math.Ceiling(frame);
        }

        public static long TimeToSampleFrameNearest(double time, double sampleRate)
        {
            const double oversample = 1024;
            double frame = Math.Round(time * sampleRate * oversample, MidpointRounding.AwayFromZero) / oversample;
            return (long)Math.Round(frame, MidpointRounding.AwayFromZero);
        }

        public static float FlushDenormal(float f)
        {
            return MathF.Abs(f) < 1.17549435e-38f ? 0f : f;
        }

        public static double FlushDenormal(double f)
        {
            return Math.Abs(f) < 1.17549435e-38 ? 0.0 : f;
        }

        public static float Clamp(float v, float lo, float hi)
        {
            return v < lo ? lo : (v > hi ? hi : v);
        }
    }

    public sealed class AudioBus
    {
        public readonly int Length;
        public float[][] Channels;
        public bool Silent = true;

        public int ChannelCount
        {
            get { return Channels.Length; }
        }

        public AudioBus(int channels, int length)
        {
            Length = length;
            Channels = new float[channels][];
            for (int i = 0; i < channels; i++)
            {
                Channels[i] = new float[length];
            }
        }

        public void SetChannelCount(int channels)
        {
            if (channels == Channels.Length)
            {
                return;
            }

            float[][] next = new float[channels][];
            for (int i = 0; i < channels; i++)
            {
                next[i] = i < Channels.Length ? Channels[i] : new float[Length];
            }

            Channels = next;
        }

        public void Zero()
        {
            for (int c = 0; c < Channels.Length; c++)
            {
                Array.Clear(Channels[c], 0, Length);
            }

            Silent = true;
        }

        public void SumFrom(AudioBus source)
        {
            if (source.Silent)
            {
                return;
            }

            int dc = Channels.Length;
            int sc = source.Channels.Length;
            if (dc == sc)
            {
                for (int c = 0; c < dc; c++)
                {
                    float[] d = Channels[c];
                    float[] s = source.Channels[c];
                    for (int i = 0; i < Length; i++)
                    {
                        d[i] += s[i];
                    }
                }
            }
            else if (sc == 1 && dc == 2)
            {
                float[] s = source.Channels[0];
                for (int c = 0; c < 2; c++)
                {
                    float[] d = Channels[c];
                    for (int i = 0; i < Length; i++)
                    {
                        d[i] += s[i];
                    }
                }
            }
            else if (sc == 2 && dc == 1)
            {
                float[] d = Channels[0];
                float[] l = source.Channels[0];
                float[] r = source.Channels[1];
                for (int i = 0; i < Length; i++)
                {
                    d[i] += 0.5f * l[i];
                    d[i] += 0.5f * r[i];
                }
            }
            else
            {
                throw new NotSupportedException("AudioDsp mixes mono and stereo only");
            }

            Silent = false;
        }

        public void CopyWithGainFrom(AudioBus source, float gain)
        {
            if (source.Silent)
            {
                Zero();
                return;
            }

            for (int c = 0; c < Channels.Length; c++)
            {
                float[] d = Channels[c];
                float[] s = source.Channels[c];
                for (int i = 0; i < Length; i++)
                {
                    d[i] = s[i] * gain;
                }
            }

            Silent = false;
        }

        public void CopyWithSampleAccurateGainFrom(AudioBus source, float[] gains)
        {
            if (source.Silent)
            {
                Zero();
                return;
            }

            for (int c = 0; c < Channels.Length; c++)
            {
                float[] d = Channels[c];
                float[] s = source.Channels[c];
                for (int i = 0; i < Length; i++)
                {
                    d[i] = s[i] * gains[i];
                }
            }

            Silent = false;
        }
    }
}
