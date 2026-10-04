using System;
using System.Collections.Generic;
using System.Globalization;

namespace OrbitalSalvage.Logic
{
    public static class PhysicsSnapshot
    {
        public const int GlobalCount = 12;
        public const int DebrisStride = 12;
        public const int FlapStride = 2;
        public const int PinStride = 1;

        public static readonly string[] GlobalNames =
        {
            "ringAngle", "angularVelocity", "innerRingAngle", "rcsBoostTimer", "lastRatchetAngle", "rcsThrusting",
            "rcsDirection", "nextDebrisId", "debrisCount", "flapCount", "pinCount", "gateCount",
        };

        public static readonly string[] DebrisFieldNames =
        {
            "id", "x", "y", "vx", "vy", "radius", "mass", "weight", "state", "ingestProgress", "gateCooldown", "type",
        };

        public static readonly string[] FlapFieldNames = { "swing", "swingVel" };
        public static readonly string[] PinFieldNames = { "hitTimer" };

        public static double TypeCode(string type)
        {
            switch (type)
            {
                case "titanium": return 0;
                case "solar": return 1;
                case "volatile": return 2;
                case "magnetic": return 3;
                default: return 4;
            }
        }

        public static string TypeName(double code)
        {
            switch ((int)code)
            {
                case 1: return "solar";
                case 2: return "volatile";
                case 3: return "magnetic";
                case 4: return "unknown";
                default: return "titanium";
            }
        }

        public static double GateTypeCode(string type)
        {
            switch (type)
            {
                case "MULTIPLY": return 0;
                case "ADD": return 1;
                case "BOOST": return 2;
                default: return 3;
            }
        }

        private static double B(bool v)
        {
            return v ? 1.0 : 0.0;
        }

        public static double[] CaptureStatic(PhysicsEngine e)
        {
            var v = new List<double>
            {
                e.Center.X, e.Center.Y, e.RingRadius, e.CorridorWidth, e.InnerRadius, e.ApertureAngle,
                B(e.ConcentricRings), e.GearRatio, e.TrackRadius, e.BlackHole.X, e.BlackHole.Y,
                e.Gates.Count, e.Dividers.Count, e.OneWayFlaps.Count, e.DeflectorPins.Count, e.TrackWalls.Count,
            };
            foreach (GateRuntime g in e.Gates)
            {
                v.Add(g.RMin); v.Add(g.RMax); v.Add(g.Angle); v.Add(B(g.IsInner)); v.Add(g.Value); v.Add(GateTypeCode(g.Type));
            }
            foreach (DividerRuntime d in e.Dividers)
            {
                v.Add(d.RMin); v.Add(d.RMax); v.Add(d.Angle); v.Add(B(d.IsInner));
            }
            foreach (FlapRuntime f in e.OneWayFlaps)
            {
                v.Add(f.RMin); v.Add(f.RMax); v.Add(f.Angle); v.Add(f.MaxSwing); v.Add(f.NormalSign); v.Add(B(f.IsInner));
            }
            foreach (PinRuntime p in e.DeflectorPins)
            {
                v.Add(p.R); v.Add(p.Angle); v.Add(p.Radius); v.Add(p.Restitution); v.Add(B(p.IsInner));
            }
            foreach (TrackWallRuntime t in e.TrackWalls)
            {
                v.Add(t.R); v.Add(t.GapAngle); v.Add(t.GapSize);
            }
            return v.ToArray();
        }

        public static double[] Capture(PhysicsEngine e)
        {
            var v = new List<double>(GlobalCount + e.DebrisList.Count * DebrisStride + 64);
            v.Add(e.RingAngle);
            v.Add(e.AngularVelocity);
            v.Add(e.InnerRingAngle);
            v.Add(e.RcsBoostTimer);
            v.Add(e.LastRatchetAngle);
            v.Add(B(e.RcsThrusting));
            v.Add(e.RcsDirection);
            v.Add(e.NextDebrisId);
            v.Add(e.DebrisList.Count);
            v.Add(e.OneWayFlaps.Count);
            v.Add(e.DeflectorPins.Count);
            v.Add(e.Gates.Count);
            foreach (Debris d in e.DebrisList)
            {
                v.Add(d.Id);
                v.Add(d.X);
                v.Add(d.Y);
                v.Add(d.Vx);
                v.Add(d.Vy);
                v.Add(d.Radius);
                v.Add(d.Mass);
                v.Add(d.Weight);
                v.Add((int)d.State);
                v.Add(d.IngestProgress);
                v.Add(d.GateCooldown);
                v.Add(TypeCode(d.Type));
            }
            foreach (FlapRuntime f in e.OneWayFlaps)
            {
                v.Add(f.Swing);
                v.Add(f.SwingVel);
            }
            foreach (PinRuntime p in e.DeflectorPins)
            {
                v.Add(p.HitTimer);
            }
            foreach (GateRuntime g in e.Gates)
            {
                var ids = new List<int>(g.ProcessedDebris);
                ids.Sort();
                v.Add(ids.Count);
                foreach (int id in ids) v.Add(id);
            }
            return v.ToArray();
        }

        public static void Load(PhysicsEngine e, double[] v)
        {
            e.RingAngle = v[0];
            e.AngularVelocity = v[1];
            e.InnerRingAngle = v[2];
            e.RcsBoostTimer = v[3];
            e.LastRatchetAngle = v[4];
            e.RcsThrusting = v[5] != 0;
            e.RcsDirection = v[6];
            e.NextDebrisId = (int)v[7];
            int debrisCount = (int)v[8];
            int flapCount = (int)v[9];
            int pinCount = (int)v[10];
            int gateCount = (int)v[11];
            int p = GlobalCount;

            e.DebrisList = new List<Debris>();
            for (int i = 0; i < debrisCount; i++)
            {
                string type = TypeName(v[p + 11]);
                var d = new Debris
                {
                    Id = (int)v[p],
                    X = v[p + 1],
                    Y = v[p + 2],
                    Vx = v[p + 3],
                    Vy = v[p + 4],
                    Radius = v[p + 5],
                    Mass = v[p + 6],
                    Weight = v[p + 7],
                    State = (DebrisState)(int)v[p + 8],
                    IngestProgress = v[p + 9],
                    GateCooldown = v[p + 10],
                    Type = type,
                    Color = DebrisColor(type),
                };
                e.DebrisList.Add(d);
                p += DebrisStride;
            }
            if (flapCount != e.OneWayFlaps.Count || pinCount != e.DeflectorPins.Count || gateCount != e.Gates.Count)
            {
                throw new InvalidOperationException("Snapshot topology does not match the initialised sector");
            }
            foreach (FlapRuntime f in e.OneWayFlaps)
            {
                f.Swing = v[p];
                f.SwingVel = v[p + 1];
                p += FlapStride;
            }
            foreach (PinRuntime pin in e.DeflectorPins)
            {
                pin.HitTimer = v[p];
                p += PinStride;
            }
            foreach (GateRuntime g in e.Gates)
            {
                int n = (int)v[p++];
                g.ProcessedDebris = new HashSet<int>();
                for (int i = 0; i < n; i++) g.ProcessedDebris.Add((int)v[p++]);
            }
        }

        private static string DebrisColor(string type)
        {
            switch (type)
            {
                case "solar": return "#fbbf24";
                case "volatile": return "#ef4444";
                case "magnetic": return "#a855f7";
                default: return "#38bdf8";
            }
        }

        /// <summary>FNV-1a 64 of <see cref="Capture"/>, little-endian IEEE doubles. Same bytes as <c>tools/parity/web/game-sync.mjs</c>.</summary>
        public static string Hash(PhysicsEngine e)
        {
            return HashDoubles(Capture(e));
        }

        public static string HashDoubles(double[] values)
        {
            unchecked
            {
                ulong hash = 14695981039346656037UL;
                const ulong prime = 1099511628211UL;
                for (int i = 0; i < values.Length; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(values[i]);
                    if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
                    for (int b = 0; b < 8; b++)
                    {
                        hash ^= bytes[b];
                        hash *= prime;
                    }
                }
                return hash.ToString("x16", CultureInfo.InvariantCulture);
            }
        }

        public static string Describe(double[] v, int index)
        {
            if (index < GlobalCount) return GlobalNames[index];
            int debrisCount = (int)v[8];
            int flapCount = (int)v[9];
            int pinCount = (int)v[10];
            int p = index - GlobalCount;
            if (p < debrisCount * DebrisStride)
            {
                int n = p / DebrisStride;
                return "debris[" + n + "(id " + (long)v[GlobalCount + n * DebrisStride] + ")]." + DebrisFieldNames[p % DebrisStride];
            }
            p -= debrisCount * DebrisStride;
            if (p < flapCount * FlapStride) return "flap[" + (p / FlapStride) + "]." + FlapFieldNames[p % FlapStride];
            p -= flapCount * FlapStride;
            if (p < pinCount * PinStride) return "pin[" + p + "].hitTimer";
            return "gateSets+" + (p - pinCount * PinStride);
        }
    }
}
