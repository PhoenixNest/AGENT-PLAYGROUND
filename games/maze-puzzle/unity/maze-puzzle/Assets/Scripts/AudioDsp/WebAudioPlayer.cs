using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace OrbitalSalvage.AudioDsp
{
    public sealed class SystemRandomSource : IRandomSource
    {
        readonly System.Random random;

        public SystemRandomSource(int seed)
        {
            random = new System.Random(seed);
        }

        public SystemRandomSource()
        {
            random = new System.Random();
        }

        public double NextDouble()
        {
            return random.NextDouble();
        }
    }

    /// <summary>
    /// Plays an AudioEngineDsp through an AudioSource. The graph always runs at 44100 Hz (so noise buffers consume
    /// the same RNG draws as the web build and filter/oscillator tables match the references) and is resampled with
    /// Catmull-Rom interpolation to AudioSettings.outputSampleRate. Every engine call is queued and executed on the
    /// audio thread at the start of a buffer, which keeps graph state and RNG single-threaded.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class WebAudioPlayer : MonoBehaviour
    {
        public const int GraphSampleRate = 44100;
        const int RingSize = 4096;
        const int RingMask = RingSize - 1;

        readonly ConcurrentQueue<Action<AudioEngineDsp>> commands = new ConcurrentQueue<Action<AudioEngineDsp>>();
        readonly float[] ringLeft = new float[RingSize];
        readonly float[] ringRight = new float[RingSize];
        readonly float[] quantumLeft = new float[AudioMath.RenderQuantum];
        readonly float[] quantumRight = new float[AudioMath.RenderQuantum];

        AudioEngineDsp engine;
        IRandomSource randomSource;
        AudioSource source;
        AudioClip silence;
        volatile int deviceRate = GraphSampleRate;
        volatile bool muted;
        volatile string lastError;
        long renderedFrames;
        double readPosition;

        public bool IsMuted
        {
            get { return muted; }
        }

        public void SetRandomSource(IRandomSource random)
        {
            randomSource = random;
            engine = null;
        }

        public void Post(Action<AudioEngineDsp> command)
        {
            commands.Enqueue(command);
        }

        public void PublicInit()
        {
            Post(e => e.PublicInit());
        }

        public void ToggleMute()
        {
            Post(e => e.ToggleMute());
        }

        public void SetSfxVolume(double percent)
        {
            Post(e => e.SetSfxVolume(percent));
        }

        public void SetAmbienceVolume(double percent)
        {
            Post(e => e.SetAmbienceVolume(percent));
        }

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            silence = AudioClip.Create("WebAudioPlayer.Silence", 1024, 1, GraphSampleRate, false);
            source.clip = silence;
            deviceRate = Mathf.Max(8000, AudioSettings.outputSampleRate);
        }

        void OnEnable()
        {
            AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            deviceRate = Mathf.Max(8000, AudioSettings.outputSampleRate);
            source.Play();
        }

        void OnDisable()
        {
            AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
            source.Stop();
        }

        void OnDestroy()
        {
            if (silence != null)
            {
                Destroy(silence);
            }
        }

        void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            deviceRate = Mathf.Max(8000, AudioSettings.outputSampleRate);
        }

        void Update()
        {
            string error = lastError;
            if (error != null)
            {
                lastError = null;
                Debug.LogError("WebAudioPlayer command failed: " + error);
            }
        }

        AudioEngineDsp EnsureEngine()
        {
            if (engine == null)
            {
                if (randomSource == null)
                {
                    randomSource = new SystemRandomSource();
                }

                engine = new AudioEngineDsp(randomSource, () => new AudioContextDsp(GraphSampleRate));
                engine.Readback = ReadbackMode.Rendered;
            }

            return engine;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            FillBuffer(data, channels, deviceRate);
        }

        public void FillBuffer(float[] data, int channels, int outputSampleRate)
        {
            Action<AudioEngineDsp> command;
            while (commands.TryDequeue(out command))
            {
                try
                {
                    command(EnsureEngine());
                    muted = engine.IsMuted;
                }
                catch (Exception e)
                {
                    lastError = e.ToString();
                }
            }

            if (engine == null || engine.Context == null)
            {
                Array.Clear(data, 0, data.Length);
                return;
            }

            int frames = data.Length / channels;
            double step = GraphSampleRate / (double)outputSampleRate;
            AudioContextDsp ctx = engine.Context;

            for (int i = 0; i < frames; i++)
            {
                long index = (long)readPosition;
                while (renderedFrames < index + 3)
                {
                    ctx.RenderQuantum(quantumLeft, quantumRight, 0);
                    int start = (int)(renderedFrames & RingMask);
                    Array.Copy(quantumLeft, 0, ringLeft, start, AudioMath.RenderQuantum);
                    Array.Copy(quantumRight, 0, ringRight, start, AudioMath.RenderQuantum);
                    renderedFrames += AudioMath.RenderQuantum;
                }

                float t = (float)(readPosition - index);
                float l = Interpolate(ringLeft, index, t);
                float r = Interpolate(ringRight, index, t);
                int o = i * channels;
                if (channels == 1)
                {
                    data[o] = 0.5f * (l + r);
                }
                else
                {
                    data[o] = l;
                    data[o + 1] = r;
                    for (int c = 2; c < channels; c++)
                    {
                        data[o + c] = 0f;
                    }
                }

                readPosition += step;
            }
        }

        static float Interpolate(float[] ring, long index, float t)
        {
            float x0 = ring[(index - 1) & RingMask];
            float x1 = ring[index & RingMask];
            float x2 = ring[(index + 1) & RingMask];
            float x3 = ring[(index + 2) & RingMask];
            float c1 = 0.5f * (x2 - x0);
            float c2 = x0 - 2.5f * x1 + 2f * x2 - 0.5f * x3;
            float c3 = 0.5f * (x3 - x0) + 1.5f * (x1 - x2);
            return ((c3 * t + c2) * t + c1) * t + x1;
        }

        public static float[][] RenderOffline(Action<AudioEngineDsp> script, double seconds, IRandomSource random = null, double scriptTime = 0.1)
        {
            OfflineRenderer renderer = new OfflineRenderer(random ?? new SystemRandomSource(), seconds);
            renderer.ClockOverride = 0;
            renderer.Engine.PublicInit();
            renderer.ClockOverride = scriptTime;
            script(renderer.Engine);
            renderer.ClockOverride = null;
            renderer.RenderRemaining();
            return renderer.Output();
        }
    }
}
