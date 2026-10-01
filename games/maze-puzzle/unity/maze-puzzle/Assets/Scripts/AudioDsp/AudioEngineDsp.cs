using System;

namespace OrbitalSalvage.AudioDsp
{
    public enum ReadbackMode
    {
        /// <summary>AudioParam.value reads back the last value passed to setValueAtTime (SPEC stub semantics).</summary>
        Stub,

        /// <summary>AudioParam.value reads back the rendered intrinsic value (Chromium semantics).</summary>
        Rendered,
    }

    /// <summary>
    /// C# port of web/engine/audio.js AudioEngine. Method names, arguments, cooldowns and RNG consumption follow the JS.
    /// </summary>
    public sealed class AudioEngineDsp
    {
        static readonly double[] ScaleFreqs =
        {
            261.63, 293.66, 329.63, 392.0, 440.0, 523.25, 587.33, 659.25, 783.99, 880.0, 1046.5,
        };

        const int PrewarmOscillators = 128;
        const int PrewarmGains = 128;
        const int PrewarmBiquads = 32;
        const int PrewarmBufferSources = 16;
        const int PrewarmEvents = 2048;

        static readonly double[] SuperShredderFreqs = { 523.25, 783.99, 1046.5 };

        readonly IRandomSource rng;
        readonly Func<AudioContextDsp> contextFactory;
        readonly Func<double> clockOverride;

        AudioContextDsp ctx;
        DynamicsCompressorNode masterCompressor;
        GainNode sfxGain;
        GainNode ambienceGain;
        GainNode masterGain;

        double lastCollisionTime;
        double lastRatchetTime;
        double lastGatePassTime;
        double lastRcsThrustTime;
        double lastSwallowTime;
        double lastBumperTime;
        int swallowQueueCount;
        int chimeIndex;

        public bool IsMuted { get; private set; }
        public bool IsInitialized { get; private set; }
        public double SfxVolume { get; private set; } = 0.8;
        public double AmbienceVolume { get; private set; } = 0.7;
        public ReadbackMode Readback { get; set; } = ReadbackMode.Stub;

        public AudioContextDsp Context
        {
            get { return ctx; }
        }

        public AudioEngineDsp(IRandomSource rng, Func<AudioContextDsp> contextFactory, Func<double> clock = null)
        {
            this.rng = rng;
            this.contextFactory = contextFactory;
            clockOverride = clock;
        }

        double Now
        {
            get { return clockOverride != null ? clockOverride() : ctx.CurrentTime; }
        }

        float Readable(AudioParam p)
        {
            return Readback == ReadbackMode.Rendered ? p.Value : p.ControlValue;
        }

        public void PublicInit()
        {
            if (IsInitialized)
            {
                EnsureRunning();
                return;
            }

            ctx = contextFactory();
            double t = Now;

            masterCompressor = ctx.CreateDynamicsCompressor();
            masterCompressor.Threshold.SetValueAtTime(-14, t);
            masterCompressor.Knee.SetValueAtTime(10, t);
            masterCompressor.Ratio.SetValueAtTime(8, t);
            masterCompressor.Attack.SetValueAtTime(0.003, t);
            masterCompressor.Release.SetValueAtTime(0.12, t);

            sfxGain = ctx.CreateGain();
            sfxGain.Gain.SetValueAtTime(SfxVolume, t);

            ambienceGain = ctx.CreateGain();
            ambienceGain.Gain.SetValueAtTime(AmbienceVolume, t);

            masterGain = ctx.CreateGain();
            masterGain.Gain.SetValueAtTime(IsMuted ? 0 : 0.85, t);

            masterCompressor.Connect(sfxGain);
            sfxGain.Connect(masterGain);
            ambienceGain.Connect(masterGain);
            masterGain.Connect(ctx.Destination);

            ctx.Prewarm(PrewarmOscillators, PrewarmGains, PrewarmBiquads, PrewarmBufferSources, PrewarmEvents, (int)Math.Floor(ctx.SampleRate * 0.22));
            ctx.ReuseNodes = true;

            IsInitialized = true;
            EnsureRunning();
        }

        public void EnsureRunning()
        {
            if (ctx == null)
            {
                return;
            }

            if (ctx.State == AudioContextState.Suspended)
            {
                ctx.Resume();
            }
        }

        static double ClampPercent(double percent)
        {
            double n = double.IsNaN(percent) ? 0 : percent;
            return Math.Max(0, Math.Min(100, n)) / 100;
        }

        public void SetSfxVolume(double percent)
        {
            double v = ClampPercent(percent);
            SfxVolume = v;
            if (sfxGain != null && ctx != null)
            {
                sfxGain.Gain.SetValueAtTime(v, Now);
            }
        }

        public void SetAmbienceVolume(double percent)
        {
            double v = ClampPercent(percent);
            AmbienceVolume = v;
            if (ambienceGain != null && ctx != null)
            {
                ambienceGain.Gain.SetValueAtTime(v, Now);
            }
        }

        public bool ToggleMute()
        {
            IsMuted = !IsMuted;
            if (ctx != null)
            {
                EnsureRunning();
                if (masterGain != null)
                {
                    double t = Now;
                    masterGain.Gain.CancelScheduledValues(t);
                    masterGain.Gain.SetValueAtTime(Readable(masterGain.Gain), t);
                    masterGain.Gain.LinearRampToValueAtTime(IsMuted ? 0 : 0.85, t + 0.02);
                }
            }

            return IsMuted;
        }

        public void PlaySpinRatchet(double pitchMultiplier = 1.0)
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;
            if (now - lastRatchetTime < 0.045)
            {
                return;
            }

            lastRatchetTime = now;

            OscillatorNode osc = ctx.CreateOscillator();
            GainNode gain = ctx.CreateGain();
            BiquadFilterNode filter = ctx.CreateBiquadFilter();

            osc.Type = OscillatorType.Triangle;
            double baseFreq = 800 * Math.Max(0.6, Math.Min(1.8, pitchMultiplier));
            osc.Frequency.SetValueAtTime(baseFreq, now);
            osc.Frequency.ExponentialRampToValueAtTime(140, now + 0.022);

            filter.Type = BiquadFilterType.Bandpass;
            filter.Frequency.SetValueAtTime(1200, now);
            filter.Q.SetValueAtTime(1.5, now);

            gain.Gain.SetValueAtTime(0.06, now);
            gain.Gain.ExponentialRampToValueAtTime(0.0001, now + 0.024);

            osc.Connect(filter);
            filter.Connect(gain);
            gain.Connect(masterCompressor);

            osc.Start(now);
            osc.Stop(now + 0.026);
        }

        public void PlayDebrisCollision(double intensity = 1.0, string debrisType = "titanium")
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;
            if (now - lastCollisionTime < 0.048)
            {
                return;
            }

            lastCollisionTime = now;

            OscillatorNode osc = ctx.CreateOscillator();
            GainNode gain = ctx.CreateGain();

            double startFreq;
            double endFreq;
            OscillatorType oscType;

            if (debrisType == "solar")
            {
                startFreq = 620 + rng.NextDouble() * 80;
                endFreq = 340;
                oscType = OscillatorType.Triangle;
            }
            else if (debrisType == "volatile")
            {
                startFreq = 260 + rng.NextDouble() * 40;
                endFreq = 90;
                oscType = OscillatorType.Sine;
            }
            else
            {
                startFreq = 460 + rng.NextDouble() * 60;
                endFreq = 160;
                oscType = OscillatorType.Triangle;
            }

            osc.Type = oscType;
            osc.Frequency.SetValueAtTime(startFreq, now);
            osc.Frequency.ExponentialRampToValueAtTime(endFreq, now + 0.038);

            double targetGain = Math.Min(0.12 * intensity, 0.14);
            gain.Gain.SetValueAtTime(targetGain, now);
            gain.Gain.ExponentialRampToValueAtTime(0.0001, now + 0.04);

            osc.Connect(gain);
            gain.Connect(masterCompressor);

            osc.Start(now);
            osc.Stop(now + 0.042);
        }

        public void PlayGatePass(string gateType = "MULTIPLY", double value = 2)
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;
            if (now - lastGatePassTime < 0.07)
            {
                return;
            }

            lastGatePassTime = now;

            if (gateType == "BOOST")
            {
                PlayBoosterPass(now);
            }
            else if (gateType == "ADD")
            {
                PlayCompactorPass(now, value);
            }
            else if (value >= 4)
            {
                PlaySuperShredderPass(now, value);
            }
            else
            {
                PlayShredderPass(now, value);
            }
        }

        double ResolveNow(double? now)
        {
            double n = now ?? (ctx != null ? Now : 0);
            if (n == 0 || double.IsNaN(n))
            {
                n = Now;
            }

            return n;
        }

        public void PlayShredderPass(double? now = null, double value = 0)
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            double t = ResolveNow(now);

            OscillatorNode osc = ctx.CreateOscillator();
            GainNode gain = ctx.CreateGain();
            BiquadFilterNode filter = ctx.CreateBiquadFilter();

            osc.Type = OscillatorType.Sawtooth;
            osc.Frequency.SetValueAtTime(450, t);
            osc.Frequency.ExponentialRampToValueAtTime(880, t + 0.08);

            filter.Type = BiquadFilterType.Lowpass;
            filter.Frequency.SetValueAtTime(1600, t);

            gain.Gain.SetValueAtTime(0.14, t);
            gain.Gain.ExponentialRampToValueAtTime(0.0001, t + 0.12);

            osc.Connect(filter);
            filter.Connect(gain);
            gain.Connect(masterCompressor);

            osc.Start(t);
            osc.Stop(t + 0.14);
        }

        public void PlayCompactorPass(double? now = null, double value = 0)
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            double t = ResolveNow(now);

            OscillatorNode osc = ctx.CreateOscillator();
            GainNode gain = ctx.CreateGain();

            osc.Type = OscillatorType.Sine;
            osc.Frequency.SetValueAtTime(140, t);
            osc.Frequency.ExponentialRampToValueAtTime(320, t + 0.14);

            gain.Gain.SetValueAtTime(0.22, t);
            gain.Gain.ExponentialRampToValueAtTime(0.0001, t + 0.22);

            osc.Connect(gain);
            gain.Connect(masterCompressor);

            osc.Start(t);
            osc.Stop(t + 0.24);
        }

        public void PlaySuperShredderPass(double? now = null, double value = 0)
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            double t = ResolveNow(now);
            double[] freqs = SuperShredderFreqs;
            for (int idx = 0; idx < freqs.Length; idx++)
            {
                OscillatorNode osc = ctx.CreateOscillator();
                GainNode gain = ctx.CreateGain();

                osc.Type = OscillatorType.Triangle;
                osc.Frequency.SetValueAtTime(freqs[idx], t + idx * 0.02);

                gain.Gain.SetValueAtTime(0, t + idx * 0.02);
                gain.Gain.LinearRampToValueAtTime(0.12, t + idx * 0.02 + 0.015);
                gain.Gain.ExponentialRampToValueAtTime(0.0001, t + idx * 0.02 + 0.18);

                osc.Connect(gain);
                gain.Connect(masterCompressor);

                osc.Start(t + idx * 0.02);
                osc.Stop(t + idx * 0.02 + 0.2);
            }
        }

        public void PlayBoosterPass(double? now = null)
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double n = now ?? 0;
            double t;
            if (!double.IsNaN(n) && !double.IsInfinity(n) && n > 0)
            {
                t = n;
            }
            else
            {
                double cur = Now;
                t = cur != 0 && !double.IsNaN(cur) ? cur : 0.001;
            }

            OscillatorNode osc = ctx.CreateOscillator();
            GainNode gain = ctx.CreateGain();

            osc.Type = OscillatorType.Sine;
            osc.Frequency.SetValueAtTime(350, t);
            osc.Frequency.ExponentialRampToValueAtTime(1100, t + 0.12);

            gain.Gain.SetValueAtTime(0.16, t);
            gain.Gain.ExponentialRampToValueAtTime(0.0001, t + 0.15);

            osc.Connect(gain);
            gain.Connect(masterCompressor);

            osc.Start(t);
            osc.Stop(t + 0.17);
        }

        public void PlayRcsThrust()
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;
            if (now - lastRcsThrustTime < 0.05)
            {
                return;
            }

            lastRcsThrustTime = now;

            double sampleRate = ctx.SampleRate;
            int bufferSize = (int)Math.Floor(sampleRate * 0.22);
            AudioBufferDsp buffer = ctx.AcquireBuffer(1, bufferSize, ctx.SampleRate);
            float[] data = buffer.GetChannelData(0);
            for (int i = 0; i < bufferSize; i++)
            {
                data[i] = (float)((rng.NextDouble() * 2 - 1) * (1 - (double)i / bufferSize));
            }

            AudioBufferSourceNode noise = ctx.CreateBufferSource();
            noise.Buffer = buffer;

            BiquadFilterNode filter = ctx.CreateBiquadFilter();
            filter.Type = BiquadFilterType.Bandpass;
            filter.Frequency.SetValueAtTime(1800, now);
            filter.Frequency.ExponentialRampToValueAtTime(420, now + 0.18);
            filter.Q.SetValueAtTime(1.2, now);

            GainNode gain = ctx.CreateGain();
            gain.Gain.SetValueAtTime(0.14, now);
            gain.Gain.ExponentialRampToValueAtTime(0.0001, now + 0.2);

            noise.Connect(filter);
            filter.Connect(gain);
            gain.Connect(masterCompressor);

            noise.Start(now);
            noise.Stop(now + 0.22);
        }

        public void PlayBumperBounce()
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;
            if (now - lastBumperTime < 0.045)
            {
                return;
            }

            lastBumperTime = now;

            OscillatorNode osc = ctx.CreateOscillator();
            GainNode gain = ctx.CreateGain();

            osc.Type = OscillatorType.Sine;
            double baseFreq = 720 + rng.NextDouble() * 120;
            osc.Frequency.SetValueAtTime(baseFreq, now);
            osc.Frequency.ExponentialRampToValueAtTime((baseFreq * 1.5), now + 0.03);
            osc.Frequency.ExponentialRampToValueAtTime((baseFreq * 0.8), now + 0.07);

            gain.Gain.SetValueAtTime(0.16, now);
            gain.Gain.ExponentialRampToValueAtTime(0.0001, now + 0.08);

            osc.Connect(gain);
            gain.Connect(masterCompressor);

            osc.Start(now);
            osc.Stop(now + 0.085);
        }

        public void PlayDebrisSwallowed()
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;

            if (now - lastSwallowTime > 0.6)
            {
                chimeIndex = 0;
                swallowQueueCount = 0;
            }
            else
            {
                chimeIndex = (chimeIndex + 1) % ScaleFreqs.Length;
                swallowQueueCount++;
            }

            lastSwallowTime = now;

            double staggerOffset = Math.Min(swallowQueueCount * 0.025, 0.2);
            double triggerTime = now + staggerOffset;

            OscillatorNode popOsc = ctx.CreateOscillator();
            GainNode popGain = ctx.CreateGain();
            popOsc.Type = OscillatorType.Sine;
            popOsc.Frequency.SetValueAtTime(200, triggerTime);
            popOsc.Frequency.ExponentialRampToValueAtTime(560, triggerTime + 0.05);

            popGain.Gain.SetValueAtTime(0.12, triggerTime);
            popGain.Gain.ExponentialRampToValueAtTime(0.0001, triggerTime + 0.06);

            popOsc.Connect(popGain);
            popGain.Connect(masterCompressor);
            popOsc.Start(triggerTime);
            popOsc.Stop(triggerTime + 0.07);

            double freq = ScaleFreqs[chimeIndex];
            OscillatorNode xyloOsc = ctx.CreateOscillator();
            GainNode xyloGain = ctx.CreateGain();

            xyloOsc.Type = OscillatorType.Sine;
            xyloOsc.Frequency.SetValueAtTime(freq, triggerTime + 0.01);

            xyloGain.Gain.SetValueAtTime(0, triggerTime + 0.01);
            xyloGain.Gain.LinearRampToValueAtTime(0.18, triggerTime + 0.025);
            xyloGain.Gain.ExponentialRampToValueAtTime(0.0001, triggerTime + 0.28);

            xyloOsc.Connect(xyloGain);
            xyloGain.Connect(masterCompressor);

            xyloOsc.Start(triggerTime + 0.01);
            xyloOsc.Stop(triggerTime + 0.3);
        }

        public void PlayHazardExplosion()
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;

            OscillatorNode osc = ctx.CreateOscillator();
            GainNode gain = ctx.CreateGain();

            osc.Type = OscillatorType.Triangle;
            osc.Frequency.SetValueAtTime(280, now);
            osc.Frequency.ExponentialRampToValueAtTime(55, now + 0.16);

            gain.Gain.SetValueAtTime(0.18, now);
            gain.Gain.ExponentialRampToValueAtTime(0.0001, now + 0.18);

            osc.Connect(gain);
            gain.Connect(masterCompressor);

            osc.Start(now);
            osc.Stop(now + 0.2);
        }

        struct Note
        {
            public double F;
            public double Dur;
            public double Bend;

            public Note(double f, double dur, double bend = 0)
            {
                F = f;
                Dur = dur;
                Bend = bend;
            }
        }

        static readonly Note[] VictoryMelody =
        {
            new Note(523.25, 0.11),
            new Note(659.25, 0.11),
            new Note(783.99, 0.14),
            new Note(1046.5, 0.42),
        };

        static readonly Note[] DefeatMelody =
        {
            new Note(392.0, 0.16),
            new Note(329.63, 0.16),
            new Note(293.66, 0.18),
            new Note(261.63, 0.38, 220.0),
        };

        static readonly Note[] BackupPodMelody =
        {
            new Note(261.63, 0.08),
            new Note(392.0, 0.08),
            new Note(523.25, 0.1),
            new Note(659.25, 0.28),
        };

        public void PlayVictoryFanfare()
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;
            Note[] melody = VictoryMelody;

            double t = now;
            foreach (Note note in melody)
            {
                OscillatorNode osc = ctx.CreateOscillator();
                GainNode gain = ctx.CreateGain();

                osc.Type = OscillatorType.Triangle;
                osc.Frequency.SetValueAtTime(note.F, t);

                gain.Gain.SetValueAtTime(0, t);
                gain.Gain.LinearRampToValueAtTime(0.2, t + 0.02);
                gain.Gain.ExponentialRampToValueAtTime(0.0001, t + note.Dur);

                osc.Connect(gain);
                gain.Connect(masterCompressor);

                osc.Start(t);
                osc.Stop(t + note.Dur + 0.03);

                t += note.Dur * 0.88;
            }
        }

        public void SetThrusterActive(bool isActive)
        {
            if (isActive)
            {
                PlaySpinRatchet(1.2);
            }
        }

        public void PlayDefeatSound()
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;
            Note[] melody = DefeatMelody;

            double t = now;
            foreach (Note note in melody)
            {
                OscillatorNode osc = ctx.CreateOscillator();
                GainNode gain = ctx.CreateGain();

                osc.Type = OscillatorType.Triangle;
                osc.Frequency.SetValueAtTime(note.F, t);
                if (note.Bend != 0)
                {
                    osc.Frequency.ExponentialRampToValueAtTime(note.Bend, t + note.Dur);
                }

                gain.Gain.SetValueAtTime(0, t);
                gain.Gain.LinearRampToValueAtTime(0.18, t + 0.02);
                gain.Gain.ExponentialRampToValueAtTime(0.0001, t + note.Dur);

                osc.Connect(gain);
                gain.Connect(masterCompressor);

                osc.Start(t);
                osc.Stop(t + note.Dur + 0.03);

                t += note.Dur * 0.85;
            }
        }

        public void PlayBackupPodSound()
        {
            if (ctx == null || IsMuted)
            {
                return;
            }

            EnsureRunning();
            double now = Now;
            Note[] melody = BackupPodMelody;

            double t = now;
            foreach (Note note in melody)
            {
                OscillatorNode osc = ctx.CreateOscillator();
                GainNode gain = ctx.CreateGain();

                osc.Type = OscillatorType.Sine;
                osc.Frequency.SetValueAtTime(note.F, t);

                gain.Gain.SetValueAtTime(0, t);
                gain.Gain.LinearRampToValueAtTime(0.2, t + 0.015);
                gain.Gain.ExponentialRampToValueAtTime(0.0001, t + note.Dur);

                osc.Connect(gain);
                gain.Connect(masterCompressor);

                osc.Start(t);
                osc.Stop(t + note.Dur + 0.02);

                t += note.Dur * 0.75;
            }
        }
    }
}
