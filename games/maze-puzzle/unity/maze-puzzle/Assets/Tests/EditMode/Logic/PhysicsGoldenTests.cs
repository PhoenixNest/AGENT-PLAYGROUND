using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic.Tests
{
    public class PhysicsGoldenTests
    {
        public static IEnumerable<string> RunNames()
        {
            if (!Fixtures.Exists("physics/manifest.json")) yield break;
            JsonValue manifest = Fixtures.Load("physics/manifest.json");
            JsonValue runs = manifest["runs"];
            for (int i = 0; i < runs.Count; i++) yield return runs[i].String;
        }

        public static IEnumerable<string> ResyncRunNames()
        {
            foreach (string name in RunNames())
            {
                if (name.EndsWith("-audio", StringComparison.Ordinal)) yield return name;
            }
        }

        [Test]
        public void Fixtures_Present()
        {
            Assert.IsTrue(Fixtures.Exists("physics/manifest.json"), "physics fixtures missing; run `node games/maze-puzzle/tools/parity/golden/generate.mjs`");
            Assert.Greater(new List<string>(RunNames()).Count, 0);
        }

        private static double[][] LoadFrames(JsonValue header, out bool allFrames)
        {
            string name = header["name"].String;
            int frames = (int)header["frames"].Number;
            JsonValue lengths = header["frameLengths"];
            var stored = new List<int>();
            if (header.Has("frameStored"))
            {
                JsonValue s = header["frameStored"];
                for (int i = 0; i < s.Count; i++) stored.Add((int)s[i].Number);
            }
            else
            {
                for (int i = 0; i <= frames; i++) stored.Add(i);
            }
            allFrames = stored.Count == frames + 1;
            byte[] bytes = Fixtures.ReadBytes("physics/" + name + ".frames.bin.gz");
            var result = new double[frames + 1][];
            int offset = 0;
            foreach (int idx in stored)
            {
                int count = (int)lengths[idx].Number;
                var d = new double[count];
                Buffer.BlockCopy(bytes, offset, d, 0, count * 8);
                offset += count * 8;
                result[idx] = d;
            }
            Assert.AreEqual(bytes.Length, offset, "frames.bin size mismatch for " + name);
            return result;
        }

        private static string DescribeDivergence(double[] expected, double[] actual)
        {
            var sb = new StringBuilder();
            int n = Math.Min(expected.Length, actual.Length);
            if (expected.Length != actual.Length)
            {
                sb.Append("vector length differs: expected " + expected.Length + " actual " + actual.Length + "\n");
            }
            int first = -1;
            int differing = 0;
            double maxAbs = 0;
            for (int i = 0; i < n; i++)
            {
                if (!Fixtures.SameBits(expected[i], actual[i]))
                {
                    if (first < 0) first = i;
                    differing++;
                    double diff = Math.Abs(expected[i] - actual[i]);
                    if (double.IsNaN(diff)) diff = double.PositiveInfinity;
                    if (diff > maxAbs) maxAbs = diff;
                }
            }
            if (first >= 0)
            {
                sb.Append("first differing field: " + PhysicsSnapshot.Describe(expected, first) + " expected " + Fixtures.Hex(expected[first]) + " (" + expected[first].ToString("R") + ") actual " + Fixtures.Hex(actual[first]) + " (" + actual[first].ToString("R") + ")\n");
                sb.Append(differing + " fields differ, max |delta| " + maxAbs.ToString("R") + (maxAbs <= 1e-4 ? " (within 1e-4)" : " (exceeds 1e-4)") + "\n");
            }
            return sb.ToString();
        }

        [Test]
        public void Static_Matches([ValueSource(nameof(RunNames))] string name)
        {
            JsonValue header = Fixtures.Load("physics/" + name + ".json.gz");
            JsonValue statics = header["statics"];
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            PhysicsRunner.Rig rig = PhysicsRunner.CreateRig((int)header["level"].Number, header);
            rig.Engine.InitSector(rig.Sector, rig.Center, rig.BlackHole);
            double[] actual = PhysicsSnapshot.CaptureStatic(rig.Engine);
            Assert.AreEqual(statics.Count, actual.Length, "static vector length");
            for (int i = 0; i < actual.Length; i++)
            {
                Assert.IsTrue(Fixtures.SameBits(Fixtures.D(statics[i].String), actual[i]), name + " static[" + i + "] expected " + statics[i].String + " actual " + Fixtures.Hex(actual[i]));
            }
        }

        [Test]
        public void FullRun_BitExact([ValueSource(nameof(RunNames))] string name)
        {
            JsonValue header = Fixtures.Load("physics/" + name + ".json.gz");
            bool allFrames;
            double[][] jsFrames = LoadFrames(header, out allFrames);
            PhysicsRunResult run = PhysicsRunner.RunFull(header);
            int frames = (int)header["frames"].Number;
            var failures = new StringBuilder();

            JsonValue hashes = header["frameHashes"];
            JsonValue rngCalls = header["rngCalls"];
            int firstHashMismatch = -1;
            for (int f = 0; f <= frames; f++)
            {
                string h = PhysicsRunner.FrameHash(run.Snapshots[f]);
                if (h != hashes[f].String)
                {
                    firstHashMismatch = f;
                    break;
                }
                if (jsFrames[f] != null)
                {
                    double[] a = run.Snapshots[f];
                    double[] e = jsFrames[f];
                    Assert.AreEqual(e.Length, a.Length, name + " frame " + f + " vector length");
                    for (int i = 0; i < e.Length; i++)
                    {
                        if (!Fixtures.SameBits(e[i], a[i]))
                        {
                            firstHashMismatch = f;
                            break;
                        }
                    }
                    if (firstHashMismatch >= 0) break;
                }
            }
            if (firstHashMismatch >= 0)
            {
                failures.Append(name + ": first divergent frame " + firstHashMismatch + "\n");
                if (jsFrames[firstHashMismatch] != null)
                {
                    failures.Append(DescribeDivergence(jsFrames[firstHashMismatch], run.Snapshots[firstHashMismatch]));
                }
                else
                {
                    failures.Append("(frame snapshot not stored; point PARITY_GOLDEN_FIXTURES at a `generate.mjs --full` directory for field-level detail)\n");
                }
            }

            for (int f = 0; f <= frames; f++)
            {
                if (run.RngCalls[f] != (long)rngCalls[f].Number)
                {
                    failures.Append("first RNG call-count divergence at frame " + f + ": expected " + (long)rngCalls[f].Number + " actual " + run.RngCalls[f] + "\n");
                    break;
                }
            }

            JsonValue jsLog = header["log"];
            int m = Math.Min(jsLog.Count, run.Log.Count);
            for (int i = 0; i < m; i++)
            {
                string expected = (long)jsLog[i][0].Number + "\t" + jsLog[i][1].String;
                if (expected != run.Log[i])
                {
                    failures.Append("first event/cue log divergence at entry " + i + ": expected [" + expected + "] actual [" + run.Log[i] + "]\n");
                    break;
                }
            }
            if (jsLog.Count != run.Log.Count && failures.Length == 0)
            {
                failures.Append("log length differs: expected " + jsLog.Count + " actual " + run.Log.Count + "\n");
            }
            if (run.RngStateEnd != (uint)header["rngStateEnd"].Number) failures.Append("final RNG state differs\n");

            Assert.AreEqual(string.Empty, failures.ToString(), name + " not bit-exact\n" + failures);
        }

        [Test]
        public void Resync_SingleStep_BitExact([ValueSource(nameof(ResyncRunNames))] string name)
        {
            JsonValue header = Fixtures.Load("physics/" + name + ".json.gz");
            JsonValue resync = header["resync"];
            Assert.Greater(resync.Count, 0, name + " has no resync fixtures");
            int level = (int)header["level"].Number;
            var failures = new StringBuilder();

            for (int r = 0; r < resync.Count; r++)
            {
                JsonValue fx = resync[r];
                int frame = (int)fx["frame"].Number;
                JsRandom.Global.Reset(JsRandom.DefaultSeed);
                PhysicsRunner.Rig rig = PhysicsRunner.CreateRig(level, header);
                PhysicsEngine e = rig.Engine;
                e.InitSector(rig.Sector, rig.Center, rig.BlackHole);
                PhysicsSnapshot.Load(e, PhysicsRunner.FromBase64(fx["pre"].String));
                var audioState = new double[9];
                for (int i = 0; i < 9; i++) audioState[i] = Fixtures.D(fx["preAudio"][i].String);
                rig.Cues.LoadState(audioState);
                JsRandom.Global.State = (uint)fx["preRng"].Number;
                rig.Log.Clear();
                rig.CurFrame = 0;

                rig.NowMs = fx["nowMsBefore"].Number;
                JsonValue inputs = fx["inputs"];
                for (int i = 0; i < inputs.Count; i++)
                {
                    if (inputs[i]["k"].String == "drag") e.ApplyInputDrag(Fixtures.D(inputs[i]["dx"].String));
                    else e.ApplyRcsBurst(Fixtures.D(inputs[i]["dir"].String));
                }
                rig.NowMs = fx["nowMsAfter"].Number;
                e.Update(PhysicsRunner.Dt);

                double[] expected = PhysicsRunner.FromBase64(fx["post"].String);
                double[] actual = PhysicsSnapshot.Capture(e);
                string detail = DescribeDivergence(expected, actual);
                if (detail.Length > 0) failures.Append(name + " resync frame " + frame + "->" + (frame + 1) + ":\n" + detail);
                if (JsRandom.Global.State != (uint)fx["postRng"].Number) failures.Append(name + " resync frame " + frame + ": RNG state differs\n");

                JsonValue log = fx["log"];
                var actualLog = new List<string>();
                foreach (string line in rig.Log)
                {
                    int tab = line.IndexOf('\t');
                    actualLog.Add(line.Substring(tab + 1));
                }
                if (actualLog.Count != log.Count) failures.Append(name + " resync frame " + frame + ": log count expected " + log.Count + " actual " + actualLog.Count + "\n");
                else
                {
                    for (int i = 0; i < log.Count; i++)
                    {
                        if (log[i].String != actualLog[i])
                        {
                            failures.Append(name + " resync frame " + frame + ": log[" + i + "] expected " + log[i].String + " actual " + actualLog[i] + "\n");
                            break;
                        }
                    }
                }
            }
            Assert.AreEqual(string.Empty, failures.ToString(), failures.ToString());
        }
    }
}
