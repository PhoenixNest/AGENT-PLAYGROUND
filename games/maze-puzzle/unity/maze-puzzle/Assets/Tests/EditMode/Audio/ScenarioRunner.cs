using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace OrbitalSalvage.AudioDsp.Tests
{
    public sealed class Mulberry32
    {
        uint state;

        public Mulberry32(uint seed)
        {
            state = seed;
        }

        public double NextDouble()
        {
            unchecked
            {
                state += 0x6D2B79F5u;
                uint a = state;
                uint t = (a ^ (a >> 15)) * (1u | a);
                t = (t + (t ^ (t >> 7)) * (61u | t)) ^ t;
                return (t ^ (t >> 14)) / 4294967296.0;
            }
        }
    }

    public sealed class CountingRandom : IRandomSource
    {
        readonly Mulberry32 inner;

        public int Draws { get; private set; }

        public CountingRandom(uint seed)
        {
            inner = new Mulberry32(seed);
        }

        public double NextDouble()
        {
            Draws++;
            return inner.NextDouble();
        }
    }

    public sealed class TraceRecorder : IAudioTraceSink
    {
        public readonly List<object[]> Events = new List<object[]>();

        public void Event(string name, params object[] args)
        {
            object[] e = new object[args.Length + 1];
            e[0] = name;
            Array.Copy(args, 0, e, 1, args.Length);
            Events.Add(e);
        }

        public void Call(string method, List<object> args)
        {
            object[] e = new object[args.Count + 2];
            e[0] = "call";
            e[1] = method;
            for (int i = 0; i < args.Count; i++)
            {
                e[i + 2] = args[i];
            }

            Events.Add(e);
        }
    }

    public sealed class ScenarioCall
    {
        public double T;
        public string Method;
        public List<object> Args;
    }

    public sealed class ScenarioSuspend
    {
        public double At;
        public List<ScenarioCall> Calls;
    }

    public sealed class ParityScenario
    {
        public const uint Seed = 0x5eed1234u;

        public string Id;
        public string Cue;
        public List<object> Args;
        public List<ScenarioCall> Calls;
        public List<ScenarioCall> Setup;
        public List<ScenarioSuspend> Suspends;
        public bool NoInit;
        public bool RngOnly;
        public double Seconds;
        public int Frames;
        public double Rms;
        public double Peak;
        public string Sha256;

        static List<ScenarioCall> ParseCalls(object node)
        {
            List<ScenarioCall> list = new List<ScenarioCall>();
            List<object> raw = node as List<object>;
            if (raw == null)
            {
                return list;
            }

            foreach (object o in raw)
            {
                Dictionary<string, object> d = (Dictionary<string, object>)o;
                ScenarioCall c = new ScenarioCall();
                c.T = d.ContainsKey("t") ? (double)d["t"] : 0.0;
                c.Method = (string)d["m"];
                c.Args = (List<object>)d["a"];
                list.Add(c);
            }

            return list;
        }

        public static ParityScenario FromJson(Dictionary<string, object> d)
        {
            ParityScenario s = new ParityScenario();
            s.Id = (string)d["id"];
            s.Cue = (string)d["cue"];
            s.Args = d.ContainsKey("args") ? (List<object>)d["args"] : new List<object>();
            s.Calls = ParseCalls(d.ContainsKey("calls") ? d["calls"] : null);
            s.Setup = ParseCalls(d.ContainsKey("setup") ? d["setup"] : null);
            s.Suspends = new List<ScenarioSuspend>();
            if (d.ContainsKey("suspends") && d["suspends"] is List<object>)
            {
                foreach (object o in (List<object>)d["suspends"])
                {
                    Dictionary<string, object> sd = (Dictionary<string, object>)o;
                    ScenarioSuspend ss = new ScenarioSuspend();
                    ss.At = (double)sd["at"];
                    ss.Calls = ParseCalls(sd["calls"]);
                    s.Suspends.Add(ss);
                }
            }

            s.NoInit = d.ContainsKey("noInit") && d["noInit"] is bool && (bool)d["noInit"];
            s.RngOnly = d.ContainsKey("rngOnly") && d["rngOnly"] is bool && (bool)d["rngOnly"];
            s.Seconds = d.ContainsKey("seconds") ? (double)d["seconds"] : 0.0;
            s.Frames = d.ContainsKey("frames") ? (int)(double)d["frames"] : 0;
            s.Rms = d.ContainsKey("rms") ? (double)d["rms"] : 0.0;
            s.Peak = d.ContainsKey("peak") ? (double)d["peak"] : 0.0;
            s.Sha256 = d.ContainsKey("sha256") ? (string)d["sha256"] : null;
            return s;
        }
    }

    public sealed class ScenarioResult
    {
        public float[][] Output;
        public int RngDraws;
        public TraceRecorder Trace;
    }

    public static class ScenarioRunner
    {
        public static List<ParityScenario> LoadManifest(string path)
        {
            Dictionary<string, object> root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(path));
            List<ParityScenario> result = new List<ParityScenario>();
            foreach (object o in (List<object>)root["entries"])
            {
                result.Add(ParityScenario.FromJson((Dictionary<string, object>)o));
            }

            return result;
        }

        static double JsNumber(object o)
        {
            if (o == null)
            {
                return 0;
            }

            if (o is double)
            {
                return (double)o;
            }

            if (o is bool)
            {
                return (bool)o ? 1 : 0;
            }

            string s = ((string)o).Trim();
            if (s.Length == 0)
            {
                return 0;
            }

            double d;
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d) ? d : double.NaN;
        }

        static void Invoke(AudioEngineDsp e, string method, List<object> args, double nowForAt)
        {
            Func<int, object> arg = i =>
            {
                if (i >= args.Count)
                {
                    return null;
                }

                object a = args[i];
                return a is string && (string)a == "@now" ? (object)nowForAt : a;
            };

            switch (method)
            {
                case "publicInit":
                    e.PublicInit();
                    break;
                case "toggleMute":
                    e.ToggleMute();
                    break;
                case "setSfxVolume":
                    e.SetSfxVolume(JsNumber(arg(0)));
                    break;
                case "setAmbienceVolume":
                    e.SetAmbienceVolume(JsNumber(arg(0)));
                    break;
                case "playSpinRatchet":
                    if (args.Count > 0)
                    {
                        e.PlaySpinRatchet(JsNumber(arg(0)));
                    }
                    else
                    {
                        e.PlaySpinRatchet();
                    }

                    break;
                case "playDebrisCollision":
                    if (args.Count == 0)
                    {
                        e.PlayDebrisCollision();
                    }
                    else
                    {
                        e.PlayDebrisCollision(JsNumber(arg(0)), args.Count > 1 ? (string)arg(1) : "titanium");
                    }

                    break;
                case "playGatePass":
                    if (args.Count == 0)
                    {
                        e.PlayGatePass();
                    }
                    else
                    {
                        e.PlayGatePass((string)arg(0), args.Count > 1 ? JsNumber(arg(1)) : 2);
                    }

                    break;
                case "playShredderPass":
                    e.PlayShredderPass(arg(0) == null ? (double?)null : JsNumber(arg(0)), JsNumber(arg(1)));
                    break;
                case "playCompactorPass":
                    e.PlayCompactorPass(arg(0) == null ? (double?)null : JsNumber(arg(0)), JsNumber(arg(1)));
                    break;
                case "playSuperShredderPass":
                    e.PlaySuperShredderPass(arg(0) == null ? (double?)null : JsNumber(arg(0)), JsNumber(arg(1)));
                    break;
                case "playBoosterPass":
                    e.PlayBoosterPass(arg(0) == null ? (double?)null : JsNumber(arg(0)));
                    break;
                case "playRcsThrust":
                    e.PlayRcsThrust();
                    break;
                case "playBumperBounce":
                    e.PlayBumperBounce();
                    break;
                case "playDebrisSwallowed":
                    e.PlayDebrisSwallowed();
                    break;
                case "playHazardExplosion":
                    e.PlayHazardExplosion();
                    break;
                case "playVictoryFanfare":
                    e.PlayVictoryFanfare();
                    break;
                case "playDefeatSound":
                    e.PlayDefeatSound();
                    break;
                case "playBackupPodSound":
                    e.PlayBackupPodSound();
                    break;
                case "setThrusterActive":
                    e.SetThrusterActive(arg(0) is bool && (bool)arg(0));
                    break;
                default:
                    throw new InvalidOperationException("Unknown audio call " + method);
            }
        }

        static List<object> ResolveArgs(List<object> args, double now)
        {
            List<object> resolved = new List<object>(args.Count);
            foreach (object a in args)
            {
                resolved.Add(a is string && (string)a == "@now" ? (object)now : a);
            }

            return resolved;
        }

        public static ScenarioResult Run(ParityScenario sc, ReadbackMode readback, bool recordTrace, bool render)
        {
            CountingRandom rng = new CountingRandom(ParityScenario.Seed);
            TraceRecorder trace = recordTrace ? new TraceRecorder() : null;
            OfflineRenderer renderer = new OfflineRenderer(rng, sc.Seconds, trace);
            renderer.Engine.Readback = readback;

            Action<ScenarioCall> run = c =>
            {
                double now = renderer.ClockOverride ?? (renderer.Context != null ? renderer.Context.CurrentTime : 0.0);
                trace?.Call(c.Method, ResolveArgs(c.Args, now));
                Invoke(renderer.Engine, c.Method, c.Args, now);
            };

            if (!sc.NoInit)
            {
                renderer.ClockOverride = 0;
                trace?.Call("publicInit", new List<object>());
                renderer.Engine.PublicInit();
            }

            renderer.ClockOverride = 0.01;
            foreach (ScenarioCall c in sc.Setup)
            {
                run(c);
            }

            List<ScenarioCall> ordered = new List<ScenarioCall>(sc.Calls);
            for (int i = 1; i < ordered.Count; i++)
            {
                ScenarioCall key = ordered[i];
                int j = i - 1;
                while (j >= 0 && ordered[j].T > key.T)
                {
                    ordered[j + 1] = ordered[j];
                    j--;
                }

                ordered[j + 1] = key;
            }

            foreach (ScenarioCall c in ordered)
            {
                renderer.ClockOverride = c.T;
                run(c);
            }

            renderer.ClockOverride = null;

            if (render)
            {
                foreach (ScenarioSuspend s in sc.Suspends)
                {
                    renderer.RenderUntilFrame((long)Math.Round(s.At * OfflineRenderer.SampleRate));
                    foreach (ScenarioCall c in s.Calls)
                    {
                        run(c);
                    }
                }

                renderer.RenderRemaining();
            }
            else
            {
                foreach (ScenarioSuspend s in sc.Suspends)
                {
                    renderer.ClockOverride = s.At;
                    foreach (ScenarioCall c in s.Calls)
                    {
                        run(c);
                    }
                }
            }

            ScenarioResult result = new ScenarioResult();
            result.Output = renderer.Output();
            result.RngDraws = rng.Draws;
            result.Trace = trace;
            return result;
        }
    }
}
