using System;
using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic
{
    public sealed class ParityAudioCues : IAudioCues
    {
        public const double SampleRate = 44100;

        private const double RatchetCooldown = 0.045;
        private const double CollisionCooldown = 0.048;
        private const double GatePassCooldown = 0.07;
        private const double RcsCooldown = 0.05;
        private const double BumperCooldown = 0.045;

        private readonly JsRandom _random;

        public ParityAudioCues(Func<double> nowMs, JsRandom random = null)
        {
            NowMs = nowMs;
            _random = random;
        }

        public Func<double> NowMs;
        public bool Initialized;
        public bool IsMuted;
        public double CreatedMs;
        public double LastRatchetTime;
        public double LastCollisionTime;
        public double LastGatePassTime;
        public double LastRcsThrustTime;
        public double LastBumperTime;
        public double LastSwallowTime;

        public List<string> CueLog = new List<string>();
        public bool RecordCues = true;
        public Action<string> CueSink;

        private JsRandom Rng
        {
            get { return _random ?? JsRandom.Global; }
        }

        public double CurrentTime
        {
            get { return (NowMs() - CreatedMs) / 1000; }
        }

        public void PublicInit()
        {
            if (Initialized) return;
            Initialized = true;
            CreatedMs = NowMs();
        }

        public bool ToggleMute()
        {
            IsMuted = !IsMuted;
            return IsMuted;
        }

        private bool Blocked()
        {
            return !Initialized || IsMuted;
        }

        private void Record(string name, string args)
        {
            string line = name + "|" + args;
            if (RecordCues) CueLog.Add(line);
            if (CueSink != null) CueSink(line);
        }

        private static string H(double d)
        {
            return BitConverter.DoubleToInt64Bits(d).ToString("x16");
        }

        public void PlaySpinRatchet(double pitchMultiplier)
        {
            Record("playSpinRatchet", H(pitchMultiplier));
            if (Blocked()) return;
            double now = CurrentTime;
            if (now - LastRatchetTime < RatchetCooldown) return;
            LastRatchetTime = now;
        }

        public void PlayDebrisCollision(double intensity, string debrisType)
        {
            Record("playDebrisCollision", H(intensity) + "," + debrisType);
            if (Blocked()) return;
            double now = CurrentTime;
            if (now - LastCollisionTime < CollisionCooldown) return;
            LastCollisionTime = now;
            Rng.NextDouble("audio.collision");
        }

        public void PlayDebrisSwallowed()
        {
            Record("playDebrisSwallowed", "");
            if (Blocked()) return;
            LastSwallowTime = CurrentTime;
        }

        public void PlayGatePass(string gateType, double value)
        {
            Record("playGatePass", gateType + "," + H(value));
            if (Blocked()) return;
            double now = CurrentTime;
            if (now - LastGatePassTime < GatePassCooldown) return;
            LastGatePassTime = now;
        }

        public void PlayRcsThrust()
        {
            Record("playRcsThrust", "");
            if (Blocked()) return;
            double now = CurrentTime;
            if (now - LastRcsThrustTime < RcsCooldown) return;
            LastRcsThrustTime = now;
            int bufferSize = (int)Math.Floor(SampleRate * 0.22);
            for (int i = 0; i < bufferSize; i++)
            {
                Rng.NextDouble("audio.rcsNoise");
            }
        }

        public void PlayBumperBounce()
        {
            Record("playBumperBounce", "");
            if (Blocked()) return;
            double now = CurrentTime;
            if (now - LastBumperTime < BumperCooldown) return;
            LastBumperTime = now;
            Rng.NextDouble("audio.bumper");
        }

        public void PlayHazardExplosion()
        {
            Record("playHazardExplosion", "");
        }

        public double[] CaptureState()
        {
            return new[]
            {
                Initialized ? 1.0 : 0.0, IsMuted ? 1.0 : 0.0, CreatedMs, LastRatchetTime, LastCollisionTime,
                LastGatePassTime, LastRcsThrustTime, LastBumperTime, LastSwallowTime,
            };
        }

        public void LoadState(double[] s)
        {
            Initialized = s[0] != 0;
            IsMuted = s[1] != 0;
            CreatedMs = s[2];
            LastRatchetTime = s[3];
            LastCollisionTime = s[4];
            LastGatePassTime = s[5];
            LastRcsThrustTime = s[6];
            LastBumperTime = s[7];
            LastSwallowTime = s[8];
        }
    }
}
