using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.AudioDsp.Tests
{
    public static class AudioTestEnvironment
    {
        public const string RegenerateHint =
            "Reference data missing. Regenerate with: cd tools/parity; npm ci; cd audio; node render-ref.mjs; node rng-counts.mjs";

        public static string AudioDir
        {
            get { return ParityChecks.FindAudioDir(Application.dataPath); }
        }

        public static string RequireAudioDir()
        {
            string dir = AudioDir;
            if (dir == null)
            {
                Assert.Ignore(RegenerateHint);
            }

            return dir;
        }

        public static string RequireReferences()
        {
            string dir = RequireAudioDir();
            if (!Directory.Exists(Path.Combine(dir, "out", "ref")))
            {
                Assert.Ignore(RegenerateHint + " (out/ref is gitignored)");
            }

            return dir;
        }

        public static IEnumerable<string> ManifestIds()
        {
            string dir = AudioDir;
            if (dir == null)
            {
                return new[] { "<missing>" };
            }

            List<string> ids = new List<string>();
            foreach (ParityScenario sc in ScenarioRunner.LoadManifest(Path.Combine(dir, "ref-manifest.json")))
            {
                ids.Add(sc.Id);
            }

            return ids;
        }

        public static IEnumerable<string> RngScenarioIds()
        {
            string dir = AudioDir;
            if (dir == null || !File.Exists(Path.Combine(dir, "rng-counts.json")))
            {
                return new[] { "<missing>" };
            }

            Dictionary<string, object> root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(Path.Combine(dir, "rng-counts.json")));
            return new List<string>(((Dictionary<string, object>)root["scenarios"]).Keys);
        }
    }

    public class ReferenceComparisonTests
    {
        [Test]
        public void ManifestIsPresentAndComplete()
        {
            string dir = AudioTestEnvironment.RequireAudioDir();
            List<ParityScenario> manifest = ScenarioRunner.LoadManifest(Path.Combine(dir, "ref-manifest.json"));
            Assert.GreaterOrEqual(manifest.Count, 60, "manifest should cover every cue family");
        }

        [Test, TestCaseSource(typeof(AudioTestEnvironment), nameof(AudioTestEnvironment.ManifestIds))]
        public void CueMatchesChromiumReference(string id)
        {
            string dir = AudioTestEnvironment.RequireReferences();
            string compareDir = Path.Combine(dir, "out", "compare");
            List<ComparisonRow> rows = ParityChecks.CompareAgainstReferences(dir, "=" + id, compareDir);
            ComparisonRow row = rows.Find(r => r.Id == id);
            Assert.IsNotNull(row, "no manifest entry " + id);
            TestContext.WriteLine(string.Format(
                "{0}: corr={1:F6} maxAbs={2:E2} rms={3:E2} stft={4:E2}",
                id,
                row.Metrics.Correlation,
                row.Metrics.MaxAbsError,
                row.Metrics.RmsError,
                row.Metrics.StftDistance));
            Assert.IsTrue(row.Pass, id + " failed: corr=" + row.Metrics.Correlation + " " + row.Note);
            Assert.GreaterOrEqual(row.Metrics.Correlation, ParityChecks.MinCorrelation);
        }

        [Test]
        public void WriteComparisonReport()
        {
            string dir = AudioTestEnvironment.RequireReferences();
            List<ComparisonRow> rows = ParityChecks.CompareAgainstReferences(dir, null, Path.Combine(dir, "out", "compare"));
            string table = ParityChecks.FormatTable(rows);
            File.WriteAllText(Path.Combine(dir, "out", "compare", "comparison.md"), table);
            TestContext.WriteLine(table);
            Assert.IsEmpty(rows.FindAll(r => !r.Pass), "some cues fell below the correlation threshold");
        }
    }

    public class RngConsumptionTests
    {
        [Test, TestCaseSource(typeof(AudioTestEnvironment), nameof(AudioTestEnvironment.RngScenarioIds))]
        public void ScenarioConsumesSameDrawsAsJs(string id)
        {
            string dir = AudioTestEnvironment.RequireAudioDir();
            Dictionary<string, object> root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(Path.Combine(dir, "rng-counts.json")));
            Dictionary<string, object> entry = (Dictionary<string, object>)((Dictionary<string, object>)root["scenarios"])[id];
            int expected = (int)(double)entry["draws"];
            ParityScenario sc = ParityScenario.FromJson((Dictionary<string, object>)entry["scenario"]);
            ScenarioResult result = ScenarioRunner.Run(sc, ReadbackMode.Stub, false, false);
            Assert.AreEqual(expected, result.RngDraws, id);
        }

        [Test]
        public void AllScenariosMatch()
        {
            string dir = AudioTestEnvironment.RequireAudioDir();
            List<string> failures = ParityChecks.RngCountFailures(dir);
            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        [Test]
        public void Mulberry32MatchesSpecSequence()
        {
            Mulberry32 r = new Mulberry32(0x5eed1234u);
            Assert.AreEqual(0.41157995723187923, r.NextDouble(), 0.0);
            Assert.AreEqual(0.841819531749934, r.NextDouble(), 0.0);
            Assert.AreEqual(0.21625563758425415, r.NextDouble(), 0.0);
            Assert.AreEqual(0.9828756144270301, r.NextDouble(), 0.0);
        }

        [Test]
        public void NoiseBufferDrawsFloorOfSampleRateTimesDuration()
        {
            CountingRandom rng = new CountingRandom(0x5eed1234u);
            OfflineRenderer renderer = new OfflineRenderer(rng, 1.0);
            renderer.ClockOverride = 0;
            renderer.Engine.PublicInit();
            renderer.ClockOverride = 1.0;
            renderer.Engine.PlayRcsThrust();
            Assert.AreEqual((int)Math.Floor(44100 * 0.22), rng.Draws);
        }
    }

    public class TraceConformanceTests
    {
        [Test]
        public void TraceMatchesGoldenWithinTolerance()
        {
            string dir = AudioTestEnvironment.RequireAudioDir();
            if (!File.Exists(Path.Combine(dir, "trace-golden.json")))
            {
                Assert.Ignore(AudioTestEnvironment.RegenerateHint);
            }

            List<string> failures = ParityChecks.TraceFailures(dir);
            Assert.IsEmpty(failures, string.Join("\n", failures));
        }
    }

    public class WebAudioPlayerTests
    {
        const int BufferFrames = 1024;
        const int WarmupBuffers = 20;
        const int PlayBuffers = 60;

        GameObject host;
        WebAudioPlayer player;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("WebAudioPlayerTest");
            player = host.AddComponent<WebAudioPlayer>();
            player.SetRandomSource(new CountingRandom(0x5eed1234u));
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(host);
        }

        float[][] PlayFanfare(int outputRate, int channels, out double cueTime)
        {
            double captured = -1;
            int total = (WarmupBuffers + PlayBuffers) * BufferFrames;
            float[][] output = { new float[total], new float[total] };
            float[] buffer = new float[BufferFrames * channels];

            player.PublicInit();
            for (int b = 0; b < WarmupBuffers + PlayBuffers; b++)
            {
                if (b == WarmupBuffers)
                {
                    player.Post(e =>
                    {
                        captured = e.Context.CurrentTime;
                        e.PlayVictoryFanfare();
                    });
                }

                player.FillBuffer(buffer, channels, outputRate);
                for (int i = 0; i < BufferFrames; i++)
                {
                    output[0][b * BufferFrames + i] = buffer[i * channels];
                    output[1][b * BufferFrames + i] = channels > 1 ? buffer[i * channels + 1] : buffer[i * channels];
                }
            }

            cueTime = captured;
            return output;
        }

        static float[][] OfflineFanfare(double cueTime, double seconds)
        {
            OfflineRenderer renderer = new OfflineRenderer(new CountingRandom(0x5eed1234u), seconds);
            renderer.ClockOverride = 0;
            renderer.Engine.PublicInit();
            renderer.ClockOverride = cueTime;
            renderer.Engine.PlayVictoryFanfare();
            renderer.ClockOverride = null;
            renderer.RenderRemaining();
            return renderer.Output();
        }

        [Test]
        public void SilentUntilInitialized()
        {
            float[] buffer = new float[BufferFrames * 2];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 1f;
            }

            player.FillBuffer(buffer, 2, 48000);
            CollectionAssert.AreEqual(new float[buffer.Length], buffer);
        }

        [Test]
        public void MatchesOfflineRenderAtGraphRate()
        {
            double cueTime;
            float[][] live = PlayFanfare(44100, 2, out cueTime);
            float[][] offline = OfflineFanfare(cueTime, live[0].Length / 44100.0);
            AudioComparison c = AudioMetrics.Compare(offline, live);
            TestContext.WriteLine(string.Format("44100: corr={0:F8} maxAbs={1:E2}", c.Correlation, c.MaxAbsError));
            Assert.Greater(c.ReferenceRms, 0.0);
            Assert.Less(c.MaxAbsError, 1e-6);
        }

        [Test]
        public void ResamplesToDeviceRate()
        {
            double cueTime;
            float[][] live = PlayFanfare(48000, 2, out cueTime);
            int frames = live[0].Length;
            float[][] offline = OfflineFanfare(cueTime, frames / 48000.0 + 0.01);
            float[][] expected = { new float[frames], new float[frames] };
            double step = 44100.0 / 48000.0;
            for (int i = 0; i < frames; i++)
            {
                double pos = i * step;
                int k = (int)pos;
                float t = (float)(pos - k);
                for (int c = 0; c < 2; c++)
                {
                    expected[c][i] = offline[c][k] + t * (offline[c][k + 1] - offline[c][k]);
                }
            }

            AudioComparison cmp = AudioMetrics.Compare(expected, live);
            TestContext.WriteLine(string.Format("48000: corr={0:F6} maxAbs={1:E2}", cmp.Correlation, cmp.MaxAbsError));
            Assert.Greater(cmp.ReferenceRms, 0.0);
            Assert.GreaterOrEqual(cmp.Correlation, 0.999);
        }

        [Test]
        public void MonoOutputAveragesChannels()
        {
            double cueTime;
            float[][] mono = PlayFanfare(44100, 1, out cueTime);
            float[][] stereo = OfflineFanfare(cueTime, mono[0].Length / 44100.0);
            for (int i = 0; i < mono[0].Length; i++)
            {
                Assert.AreEqual(0.5f * (stereo[0][i] + stereo[1][i]), mono[0][i], 1e-6f);
            }
        }

        [Test]
        public void MuteStateIsMirroredToMainThread()
        {
            player.PublicInit();
            player.ToggleMute();
            player.FillBuffer(new float[BufferFrames * 2], 2, 44100);
            Assert.IsTrue(player.IsMuted);
        }
    }

    public class EngineSemanticsTests
    {
        static OfflineRenderer NewRenderer(double seconds)
        {
            return new OfflineRenderer(new CountingRandom(0x5eed1234u), seconds);
        }

        [Test]
        public void ContextExistsOnlyAfterPublicInit()
        {
            OfflineRenderer r = NewRenderer(0.5);
            r.ClockOverride = 1;
            r.Engine.PlayBumperBounce();
            Assert.IsNull(r.Context);
            r.Engine.PublicInit();
            Assert.IsNotNull(r.Context);
            Assert.IsTrue(r.Engine.IsInitialized);
        }

        [Test]
        public void MuteBlocksCuesWithoutConsumingRandomDraws()
        {
            CountingRandom rng = new CountingRandom(0x5eed1234u);
            OfflineRenderer r = new OfflineRenderer(rng, 0.5);
            r.ClockOverride = 0;
            r.Engine.PublicInit();
            Assert.IsTrue(r.Engine.ToggleMute());
            r.ClockOverride = 1;
            r.Engine.PlayDebrisCollision(1, "solar");
            Assert.AreEqual(0, rng.Draws);
        }

        [Test]
        public void CooldownUsesScriptedClock()
        {
            CountingRandom rng = new CountingRandom(0x5eed1234u);
            OfflineRenderer r = new OfflineRenderer(rng, 0.5);
            r.ClockOverride = 0;
            r.Engine.PublicInit();
            r.ClockOverride = 1.0;
            r.Engine.PlayBumperBounce();
            r.ClockOverride = 1.01;
            r.Engine.PlayBumperBounce();
            Assert.AreEqual(1, rng.Draws);
            r.ClockOverride = 1.05;
            r.Engine.PlayBumperBounce();
            Assert.AreEqual(2, rng.Draws);
        }

        [Test]
        public void VolumeIsClampedAndNonNumbersBecomeZero()
        {
            OfflineRenderer r = NewRenderer(0.1);
            r.Engine.SetSfxVolume(250);
            Assert.AreEqual(1.0, r.Engine.SfxVolume, 0.0);
            r.Engine.SetSfxVolume(double.NaN);
            Assert.AreEqual(0.0, r.Engine.SfxVolume, 0.0);
            r.Engine.SetAmbienceVolume(-5);
            Assert.AreEqual(0.0, r.Engine.AmbienceVolume, 0.0);
        }

        [Test]
        public void RenderOfflineProducesAudibleCue()
        {
            float[][] audio = WebAudioPlayer.RenderOffline(e => e.PlayVictoryFanfare(), 1.0, new CountingRandom(0x5eed1234u));
            double energy = 0;
            foreach (float s in audio[0])
            {
                energy += s * s;
            }

            Assert.Greater(energy, 0.0);
            Assert.AreEqual(44100, audio[0].Length);
        }

        [Test]
        public void RenderingIsRepeatable()
        {
            float[][] a = WebAudioPlayer.RenderOffline(e => e.PlayDebrisCollision(0.8, "solar"), 0.4, new CountingRandom(0x5eed1234u));
            float[][] b = WebAudioPlayer.RenderOffline(e => e.PlayDebrisCollision(0.8, "solar"), 0.4, new CountingRandom(0x5eed1234u));
            CollectionAssert.AreEqual(a[0], b[0]);
            CollectionAssert.AreEqual(a[1], b[1]);
        }
    }
}
