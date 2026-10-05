using System;
using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic.Tests
{
    internal sealed class PhysicsRunResult
    {
        public List<double[]> Snapshots = new List<double[]>();
        public List<long> RngCalls = new List<long>();
        public List<string> Log = new List<string>();
        public uint RngStateEnd;
        public double[] Statics;
    }

    internal static class PhysicsRunner
    {
        public const double Dt = 16.0 / 1000;

        public struct Input
        {
            public bool IsDrag;
            public double Value;
        }

        public static List<Input> Program(string name, int f)
        {
            var list = new List<Input>();
            switch (name)
            {
                case "idle":
                    break;
                case "drag":
                    list.Add(new Input { IsDrag = true, Value = 40 * 1.3 });
                    break;
                case "bursts":
                    if (f >= 10 && f <= 40) list.Add(new Input { IsDrag = true, Value = 30 * 1.3 });
                    if (f >= 100 && f <= 130) list.Add(new Input { IsDrag = true, Value = -30 * 1.3 });
                    if (f >= 250 && f <= 300) list.Add(new Input { IsDrag = true, Value = 20 * 1.3 });
                    if (f == 60 || f == 330 || f == 481) list.Add(new Input { IsDrag = false, Value = 1 });
                    if (f == 200 || f == 480) list.Add(new Input { IsDrag = false, Value = -1 });
                    break;
                default:
                    throw new ArgumentException("unknown program " + name);
            }
            return list;
        }

        public sealed class Rig
        {
            public PhysicsEngine Engine;
            public ParityAudioCues Cues;
            public double NowMs;
            public int CurFrame;
            public List<string> Log = new List<string>();
            public SectorConfig Sector;
            public Vec2 Center;
            public Vec2 BlackHole;

            public void Rec(string text)
            {
                Log.Add(CurFrame + "\t" + text);
            }
        }

        public static Rig CreateRig(int level, JsonValue header)
        {
            var rig = new Rig();
            rig.Cues = new ParityAudioCues(() => rig.NowMs);
            rig.Cues.RecordCues = false;
            rig.Cues.CueSink = line => rig.Rec("cue:" + line);
            rig.Engine = new PhysicsEngine(rig.Cues);
            PhysicsEngine e = rig.Engine;
            e.OnDebrisSwallowed = d => rig.Rec("ev:swallowed:" + d.Id);
            e.OnDebrisExploded = d => rig.Rec("ev:exploded:" + d.Id);
            e.OnDebrisSpilled = d => rig.Rec("ev:spilled:" + d.Id);
            e.OnGatePassed = (g, d) => rig.Rec("ev:gate:" + g.Id + ":" + d.Id);
            e.OnBumperHit = h => rig.Rec("ev:bump:" + h.Pin.Id + ":" + h.Debris.Id + ":" + Fixtures.Hex(h.X) + ":" + Fixtures.Hex(h.Y));
            e.OnBoostTriggered = dir => rig.Rec("ev:boost:" + Fixtures.Hex(dir));
            rig.Sector = MazeLevelBuilder.BuildLevel(level);
            rig.Center = new Vec2(Fixtures.D(header["center"][0].String), Fixtures.D(header["center"][1].String));
            rig.BlackHole = new Vec2(Fixtures.D(header["blackHole"][0].String), Fixtures.D(header["blackHole"][1].String));
            return rig;
        }

        public static PhysicsRunResult RunFull(JsonValue header)
        {
            int level = (int)header["level"].Number;
            string program = header["program"].String;
            bool audio = header["audio"].Bool;
            int frames = (int)header["frames"].Number;

            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            Rig rig = CreateRig(level, header);
            PhysicsEngine e = rig.Engine;
            var result = new PhysicsRunResult();

            if (audio) rig.Cues.PublicInit();
            rig.CurFrame = 0;
            e.InitSector(rig.Sector, rig.Center, rig.BlackHole);
            result.Statics = PhysicsSnapshot.CaptureStatic(e);
            result.Snapshots.Add(PhysicsSnapshot.Capture(e));
            result.RngCalls.Add(JsRandom.Global.CallCount);

            for (int f = 1; f <= frames; f++)
            {
                rig.CurFrame = f;
                rig.NowMs = 16 * (f - 1);
                foreach (Input inp in Program(program, f))
                {
                    if (inp.IsDrag) e.ApplyInputDrag(inp.Value);
                    else e.ApplyRcsBurst(inp.Value);
                }
                rig.NowMs = 16 * f;
                e.Update(Dt);
                result.Snapshots.Add(PhysicsSnapshot.Capture(e));
                result.RngCalls.Add(JsRandom.Global.CallCount);
            }
            result.Log = rig.Log;
            result.RngStateEnd = JsRandom.Global.State;
            return result;
        }

        public static string FrameHash(double[] v)
        {
            unchecked
            {
                uint h1 = 0x811c9dc5u;
                uint h2 = 0x01000193u ^ 0x5bd1e995u;
                for (int i = 0; i < v.Length; i++)
                {
                    ulong bits = (ulong)BitConverter.DoubleToInt64Bits(v[i]);
                    uint lo = (uint)bits;
                    uint hi = (uint)(bits >> 32);
                    h1 = (h1 ^ lo) * 16777619u;
                    h2 = ((h2 ^ lo) * 0x85ebca6bu) ^ (h2 >> 13);
                    h1 = (h1 ^ hi) * 16777619u;
                    h2 = ((h2 ^ hi) * 0x85ebca6bu) ^ (h2 >> 13);
                }
                return h1.ToString("x8") + h2.ToString("x8");
            }
        }

        public static double[] FromBase64(string b64)
        {
            byte[] bytes = Convert.FromBase64String(b64);
            var d = new double[bytes.Length / 8];
            Buffer.BlockCopy(bytes, 0, d, 0, bytes.Length);
            return d;
        }
    }
}
