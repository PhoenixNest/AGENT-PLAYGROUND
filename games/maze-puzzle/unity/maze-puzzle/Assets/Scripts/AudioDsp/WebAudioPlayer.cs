using System;
using System.Threading;
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

    public enum PlayerCommandKind : byte
    {
        None,
        Custom,
        PublicInit,
        ToggleMute,
        SetSfxVolume,
        SetAmbienceVolume,
        PlaySpinRatchet,
        PlayDebrisCollision,
        PlayGatePass,
        PlayShredderPass,
        PlayCompactorPass,
        PlaySuperShredderPass,
        PlayBoosterPass,
        PlayRcsThrust,
        PlayBumperBounce,
        PlayDebrisSwallowed,
        PlayHazardExplosion,
        PlayVictoryFanfare,
        SetThrusterActive,
        PlayDefeatSound,
        PlayBackupPodSound,
    }

    public struct PlayerCommand
    {
        public PlayerCommandKind Kind;
        public double A;
        public double B;
        public string Text;
        public bool Flag;
        public Action<AudioEngineDsp> Custom;
    }

    /// <summary>
    /// Plays an AudioEngineDsp through an AudioSource. The graph always runs at 44100 Hz (so noise buffers consume
    /// the same RNG draws as the web build and filter/oscillator tables match the references) and is resampled with
    /// Catmull-Rom interpolation to AudioSettings.outputSampleRate. Every engine call is written into a preallocated
    /// command ring and executed on the audio thread at the start of a buffer, which keeps graph state and RNG
    /// single-threaded and keeps the render path free of allocations.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class WebAudioPlayer : MonoBehaviour
    {
        public const int GraphSampleRate = 44100;
        const int RingSize = 4096;
        const int RingMask = RingSize - 1;

        public const int CommandCapacity = 512;
        const int CommandMask = CommandCapacity - 1;

        readonly PlayerCommand[] commandRing = new PlayerCommand[CommandCapacity];
        readonly object producerLock = new object();
        int commandHead;
        int commandTail;
        int droppedCommands;
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

        public int DroppedCommands
        {
            get { return Volatile.Read(ref droppedCommands); }
        }

        public bool Enqueue(PlayerCommand command)
        {
            lock (producerLock)
            {
                int tail = commandTail;
                if (tail - Volatile.Read(ref commandHead) >= CommandCapacity)
                {
                    Interlocked.Increment(ref droppedCommands);
                    return false;
                }

                commandRing[tail & CommandMask] = command;
                Volatile.Write(ref commandTail, tail + 1);
                return true;
            }
        }

        bool Send(PlayerCommandKind kind, double a = 0, double b = 0, string text = null, bool flag = false)
        {
            PlayerCommand command = default(PlayerCommand);
            command.Kind = kind;
            command.A = a;
            command.B = b;
            command.Text = text;
            command.Flag = flag;
            return Enqueue(command);
        }

        public void Post(Action<AudioEngineDsp> command)
        {
            PlayerCommand c = default(PlayerCommand);
            c.Kind = PlayerCommandKind.Custom;
            c.Custom = command;
            Enqueue(c);
        }

        public void PublicInit()
        {
            Send(PlayerCommandKind.PublicInit);
        }

        public void ToggleMute()
        {
            Send(PlayerCommandKind.ToggleMute);
        }

        public void SetSfxVolume(double percent)
        {
            Send(PlayerCommandKind.SetSfxVolume, percent);
        }

        public void SetAmbienceVolume(double percent)
        {
            Send(PlayerCommandKind.SetAmbienceVolume, percent);
        }

        public void PlaySpinRatchet(double pitchMultiplier = 1.0)
        {
            Send(PlayerCommandKind.PlaySpinRatchet, pitchMultiplier);
        }

        public void PlayDebrisCollision(double intensity = 1.0, string debrisType = "titanium")
        {
            Send(PlayerCommandKind.PlayDebrisCollision, intensity, 0, debrisType);
        }

        public void PlayGatePass(string gateType = "MULTIPLY", double value = 2)
        {
            Send(PlayerCommandKind.PlayGatePass, value, 0, gateType);
        }

        public void PlayShredderPass(double value = 0)
        {
            Send(PlayerCommandKind.PlayShredderPass, value);
        }

        public void PlayCompactorPass(double value = 0)
        {
            Send(PlayerCommandKind.PlayCompactorPass, value);
        }

        public void PlaySuperShredderPass(double value = 0)
        {
            Send(PlayerCommandKind.PlaySuperShredderPass, value);
        }

        public void PlayBoosterPass()
        {
            Send(PlayerCommandKind.PlayBoosterPass);
        }

        public void PlayRcsThrust()
        {
            Send(PlayerCommandKind.PlayRcsThrust);
        }

        public void PlayBumperBounce()
        {
            Send(PlayerCommandKind.PlayBumperBounce);
        }

        public void PlayDebrisSwallowed()
        {
            Send(PlayerCommandKind.PlayDebrisSwallowed);
        }

        public void PlayHazardExplosion()
        {
            Send(PlayerCommandKind.PlayHazardExplosion);
        }

        public void PlayVictoryFanfare()
        {
            Send(PlayerCommandKind.PlayVictoryFanfare);
        }

        public void SetThrusterActive(bool isActive)
        {
            Send(PlayerCommandKind.SetThrusterActive, 0, 0, null, isActive);
        }

        public void PlayDefeatSound()
        {
            Send(PlayerCommandKind.PlayDefeatSound);
        }

        public void PlayBackupPodSound()
        {
            Send(PlayerCommandKind.PlayBackupPodSound);
        }

        static void Execute(AudioEngineDsp e, ref PlayerCommand c)
        {
            switch (c.Kind)
            {
                case PlayerCommandKind.Custom:
                    c.Custom(e);
                    break;
                case PlayerCommandKind.PublicInit:
                    e.PublicInit();
                    break;
                case PlayerCommandKind.ToggleMute:
                    e.ToggleMute();
                    break;
                case PlayerCommandKind.SetSfxVolume:
                    e.SetSfxVolume(c.A);
                    break;
                case PlayerCommandKind.SetAmbienceVolume:
                    e.SetAmbienceVolume(c.A);
                    break;
                case PlayerCommandKind.PlaySpinRatchet:
                    e.PlaySpinRatchet(c.A);
                    break;
                case PlayerCommandKind.PlayDebrisCollision:
                    e.PlayDebrisCollision(c.A, c.Text);
                    break;
                case PlayerCommandKind.PlayGatePass:
                    e.PlayGatePass(c.Text, c.A);
                    break;
                case PlayerCommandKind.PlayShredderPass:
                    e.PlayShredderPass(null, c.A);
                    break;
                case PlayerCommandKind.PlayCompactorPass:
                    e.PlayCompactorPass(null, c.A);
                    break;
                case PlayerCommandKind.PlaySuperShredderPass:
                    e.PlaySuperShredderPass(null, c.A);
                    break;
                case PlayerCommandKind.PlayBoosterPass:
                    e.PlayBoosterPass();
                    break;
                case PlayerCommandKind.PlayRcsThrust:
                    e.PlayRcsThrust();
                    break;
                case PlayerCommandKind.PlayBumperBounce:
                    e.PlayBumperBounce();
                    break;
                case PlayerCommandKind.PlayDebrisSwallowed:
                    e.PlayDebrisSwallowed();
                    break;
                case PlayerCommandKind.PlayHazardExplosion:
                    e.PlayHazardExplosion();
                    break;
                case PlayerCommandKind.PlayVictoryFanfare:
                    e.PlayVictoryFanfare();
                    break;
                case PlayerCommandKind.SetThrusterActive:
                    e.SetThrusterActive(c.Flag);
                    break;
                case PlayerCommandKind.PlayDefeatSound:
                    e.PlayDefeatSound();
                    break;
                case PlayerCommandKind.PlayBackupPodSound:
                    e.PlayBackupPodSound();
                    break;
            }
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
            int head = commandHead;
            while (head != Volatile.Read(ref commandTail))
            {
                int slot = head & CommandMask;
                try
                {
                    Execute(EnsureEngine(), ref commandRing[slot]);
                    muted = engine.IsMuted;
                }
                catch (Exception e)
                {
                    lastError = e.ToString();
                }

                commandRing[slot] = default(PlayerCommand);
                head++;
                Volatile.Write(ref commandHead, head);
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
