using System;

namespace OrbitalSalvage.AudioDsp
{
    /// <summary>
    /// Drives an AudioEngineDsp against an offline context at 44100 Hz. ClockOverride mirrors how the reference
    /// page overrides currentTime so cooldown logic sees the scripted time while the graph renders on its own clock.
    /// </summary>
    public sealed class OfflineRenderer
    {
        public const float SampleRate = 44100f;

        readonly float[] left;
        readonly float[] right;
        readonly float[] quantumLeft = new float[AudioMath.RenderQuantum];
        readonly float[] quantumRight = new float[AudioMath.RenderQuantum];
        readonly int totalFrames;
        int renderedFrames;
        AudioContextDsp context;

        public readonly AudioEngineDsp Engine;
        public double? ClockOverride { get; set; }
        public IAudioTraceSink Trace { get; set; }

        public AudioContextDsp Context
        {
            get { return context; }
        }

        public int TotalFrames
        {
            get { return totalFrames; }
        }

        public int RenderedFrames
        {
            get { return renderedFrames; }
        }

        public OfflineRenderer(IRandomSource rng, double seconds, IAudioTraceSink trace = null)
        {
            Trace = trace;
            totalFrames = (int)Math.Ceiling(seconds * SampleRate);
            left = new float[totalFrames];
            right = new float[totalFrames];
            Engine = new AudioEngineDsp(
                rng,
                () =>
                {
                    context = new AudioContextDsp(SampleRate, Trace);
                    return context;
                },
                () => ClockOverride ?? (context != null ? context.CurrentTime : 0.0));
        }

        public void RenderUntilFrame(long frame)
        {
            AudioContextDsp ctx = Context;
            if (ctx == null)
            {
                return;
            }

            while (renderedFrames < totalFrames && ctx.CurrentSampleFrame < frame)
            {
                RenderOneQuantum(ctx);
            }
        }

        public void RenderRemaining()
        {
            AudioContextDsp ctx = Context;
            if (ctx == null)
            {
                return;
            }

            while (renderedFrames < totalFrames)
            {
                RenderOneQuantum(ctx);
            }
        }

        void RenderOneQuantum(AudioContextDsp ctx)
        {
            ctx.RenderQuantum(quantumLeft, quantumRight, 0);
            int n = Math.Min(AudioMath.RenderQuantum, totalFrames - renderedFrames);
            Array.Copy(quantumLeft, 0, left, renderedFrames, n);
            Array.Copy(quantumRight, 0, right, renderedFrames, n);
            renderedFrames += n;
        }

        public float[][] Output()
        {
            return new[] { left, right };
        }
    }
}
