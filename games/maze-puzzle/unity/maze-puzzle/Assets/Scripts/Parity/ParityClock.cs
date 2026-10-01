using System;
using UnityEngine;

namespace OrbitalSalvage.Parity
{
    public sealed class ParityClock : IParityClock
    {
        public const int FrameMs = 16;
        public const float FrameSeconds = 0.016f;
        public static readonly DateTime EpochUtc = new DateTime(2026, 10, 1, 16, 0, 1, DateTimeKind.Utc);

        public static ParityClock Current { get; private set; }

        static TimeZoneInfo cachedZone;

        public long NowMs { get; private set; }
        public long Frame { get; private set; }
        public bool Armed { get; set; }

        public ParityClock()
        {
            Current = this;
        }

        public TimeZoneInfo Zone => cachedZone ??= ResolveZone();
        public DateTime UtcNow => EpochUtc.AddMilliseconds(NowMs);
        public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, Zone);
        public string UtcDayKey => UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        public void Reset()
        {
            NowMs = 0;
            Frame = 0;
        }

        public void OnFrameBegin()
        {
            if (!Armed) return;
            Frame++;
            NowMs = Frame * FrameMs;
        }

        public long NextFrameMs => (Frame + 1) * FrameMs;

        public void SetNowMs(long ms)
        {
            NowMs = ms;
        }

        static TimeZoneInfo ResolveZone()
        {
            foreach (var id in new[] { "Pacific Standard Time", "America/Los_Angeles" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }
            Debug.LogWarning("[Parity] Pacific time zone not found; falling back to UTC-7 fixed offset");
            return TimeZoneInfo.CreateCustomTimeZone("parity-pdt", TimeSpan.FromHours(-7), "PDT", "PDT");
        }
    }

    [DefaultExecutionOrder(-32000)]
    public sealed class ParityFrameDriver : MonoBehaviour
    {
        public ParityClock Clock;
        public ParityRenderRig Rig;

        void Update()
        {
            Clock?.OnFrameBegin();
        }

        void LateUpdate()
        {
            Rig?.Refresh();
        }
    }

    public static class JsRandom
    {
        public const uint Seed = 0x5EED1234u;
        static uint state = Seed;

        public static void Reset(uint seed = Seed)
        {
            state = seed;
        }

        public static double NextDouble()
        {
            unchecked
            {
                state += 0x6D2B79F5u;
                uint t = state;
                t = (t ^ (t >> 15)) * (1u | t);
                t = (t + (t ^ (t >> 7)) * (61u | t)) ^ t;
                return (t ^ (t >> 14)) / 4294967296.0;
            }
        }
    }
}
