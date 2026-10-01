using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.AudioDsp.Tests
{
    public static class AudioTestEnvironment
    {
        public const string AllowMissingVariable = "PARITY_ALLOW_MISSING_REFS";

        public const string RegenerateHint =
            "Reference data missing. Regenerate with: cd tools/parity; npm ci; cd audio; node render-ref.mjs; node rng-counts.mjs";

        public static bool AllowMissing
        {
            get { return Environment.GetEnvironmentVariable(AllowMissingVariable) == "1"; }
        }

        public static string AudioDir
        {
            get { return ParityChecks.FindAudioDir(Application.dataPath); }
        }

        public static void ReportMissing(string detail)
        {
            string message = detail + ". " + RegenerateHint + " (set " + AllowMissingVariable + "=1 to skip instead of fail)";
            if (AllowMissing)
            {
                Assert.Ignore(message);
            }

            Assert.Fail(message);
        }

        public static string RequireAudioDir()
        {
            string dir = AudioDir;
            if (dir == null)
            {
                ReportMissing("tools/parity/audio/ref-manifest.json not found");
            }

            return dir;
        }

        public static string RequireReferences()
        {
            string dir = RequireAudioDir();
            if (!Directory.Exists(Path.Combine(dir, "out", "ref")))
            {
                ReportMissing("tools/parity/audio/out/ref is missing");
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
                "{0}: corr={1:F12} maxAbs={2:E2} rms={3:E2} stft={4:E2}",
                id,
                row.Metrics.Correlation,
                row.Metrics.MaxAbsError,
                row.Metrics.RmsError,
                row.Metrics.StftDistance));
            Assert.IsTrue(row.Pass, id + " failed: " + row.Note);
        }

        [Test]
        public void WriteComparisonReport()
        {
            string dir = AudioTestEnvironment.RequireReferences();
            List<ComparisonRow> rows = ParityChecks.CompareAgainstReferences(dir, null, Path.Combine(dir, "out", "compare"));
            string table = ParityChecks.FormatTable(rows);
            File.WriteAllText(Path.Combine(dir, "out", "compare", "comparison.md"), table);
            TestContext.WriteLine(table);
            List<string> failed = rows.FindAll(r => !r.Pass).ConvertAll(r => r.Id + ": " + r.Note);
            Assert.IsEmpty(failed, string.Join("\n", failed.ToArray()));
        }
    }

    public class ReferenceGateTests
    {
        const string CueId = "playBumperBounce__none";

        string sourceDir;
        string tempDir;
        string manifestText;
        string localText;
        string originalSha;

        [SetUp]
        public void SetUp()
        {
            sourceDir = AudioTestEnvironment.RequireReferences();
            tempDir = Path.Combine(Path.GetTempPath(), "audio-gate-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(tempDir, "out", "ref"));
            manifestText = File.ReadAllText(Path.Combine(sourceDir, "ref-manifest.json"));
            string localPath = Path.Combine(sourceDir, "out", "ref", ParityChecks.LocalManifestName);
            Assert.IsTrue(File.Exists(localPath), "missing " + localPath + " (" + AudioTestEnvironment.RegenerateHint + ")");
            localText = File.ReadAllText(localPath);
            originalSha = ParityChecks.LoadLocalHashes(Path.Combine(sourceDir, "out", "ref"))[CueId + ".wav"];
            File.WriteAllText(Path.Combine(tempDir, "ref-manifest.json"), manifestText);
            File.WriteAllText(Path.Combine(tempDir, "out", "ref", ParityChecks.LocalManifestName), localText);
        }

        // Records the hash of the (modified) temp WAV in the temp manifest.local.json, as a fresh render would.
        void RehashTempReference()
        {
            string patched = localText.Replace(originalSha, ParityChecks.Sha256FileHex(RefPath(tempDir)));
            File.WriteAllText(Path.Combine(tempDir, "out", "ref", ParityChecks.LocalManifestName), patched);
        }

        [TearDown]
        public void TearDown()
        {
            if (tempDir != null && Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        ComparisonRow Compare()
        {
            List<ComparisonRow> rows = ParityChecks.CompareAgainstReferences(tempDir, "=" + CueId, null);
            Assert.AreEqual(1, rows.Count);
            return rows[0];
        }

        string RefPath(string dir)
        {
            return Path.Combine(dir, "out", "ref", CueId + ".wav");
        }

        [Test]
        public void CopiedReferencePassesAllGates()
        {
            File.Copy(RefPath(sourceDir), RefPath(tempDir));
            // temp manifest.local.json was copied in SetUp
            ComparisonRow row = Compare();
            Assert.IsTrue(row.Pass, row.Note);
        }

        [Test]
        public void MissingReferenceFileFailsInsteadOfPassing()
        {
            ComparisonRow row = Compare();
            Assert.IsFalse(row.Pass);
            StringAssert.Contains("missing reference", row.Note);
            StringAssert.Contains("node render-ref.mjs", row.Note);
        }

        [Test]
        public void CorruptedReferenceIsRejectedBySha256()
        {
            int rate;
            float[][] data = WavIo.ReadFloat32(RefPath(sourceDir), out rate);
            data[0][data[0].Length / 2] += 1e-3f;
            WavIo.WriteFloat32(RefPath(tempDir), data, rate);
            ComparisonRow row = Compare();
            Assert.IsFalse(row.Pass);
            StringAssert.Contains("corrupt reference", row.Note);
        }

        [Test]
        public void MissingLocalManifestFails()
        {
            File.Copy(RefPath(sourceDir), RefPath(tempDir));
            File.Delete(Path.Combine(tempDir, "out", "ref", ParityChecks.LocalManifestName));
            ComparisonRow row = Compare();
            Assert.IsFalse(row.Pass);
            StringAssert.Contains(ParityChecks.LocalManifestName, row.Note);
        }

        [Test]
        public void ReferenceThatNoLongerMatchesCommittedStatsIsStale()
        {
            // A different render (audio.js or the scenarios changed), correctly hashed: not corrupt, but stale.
            int rate;
            float[][] data = WavIo.ReadFloat32(RefPath(sourceDir), out rate);
            int from = data[0].Length / 4;
            for (int i = from; i < from + 2000 && i < data[0].Length; i++)
            {
                data[0][i] += 0.01f;
            }

            WavIo.WriteFloat32(RefPath(tempDir), data, rate);
            RehashTempReference();
            ComparisonRow row = Compare();
            Assert.IsFalse(row.Pass);
            StringAssert.Contains("stale reference", row.Note);
            StringAssert.Contains("node render-ref.mjs", row.Note);
        }

        [Test]
        public void ChromiumRerenderNoiseIsAccepted()
        {
            // Chromium re-renders differ by ~3e-8 per sample: new file bytes and hash, same statistics.
            int rate;
            float[][] data = WavIo.ReadFloat32(RefPath(sourceDir), out rate);
            for (int c = 0; c < data.Length; c++)
            {
                for (int i = 0; i < data[c].Length; i++)
                {
                    data[c][i] += (i & 1) == 0 ? 3e-8f : -3e-8f;
                }
            }

            WavIo.WriteFloat32(RefPath(tempDir), data, rate);
            RehashTempReference();
            ComparisonRow row = Compare();
            Assert.IsTrue(row.Pass, row.Note);
        }

        [Test]
        public void MaxAbsErrorGateRejectsLocalizedDeviationWithPerfectCorrelation()
        {
            int rate;
            float[][] data = WavIo.ReadFloat32(RefPath(sourceDir), out rate);
            int at = data[0].Length / 3;
            data[0][at] += 2e-5f;
            WavIo.WriteFloat32(RefPath(tempDir), data, rate);
            RehashTempReference();
            ComparisonRow row = Compare();
            Assert.GreaterOrEqual(row.Metrics.Correlation, ParityChecks.MinCorrelation);
            Assert.IsFalse(row.Pass, "a 2e-5 deviation must fail the 1e-5 max abs gate");
            StringAssert.Contains("max abs err", row.Note);
        }

        [Test]
        public void StrictGatesAreConfigured()
        {
            Assert.AreEqual(0.99, ParityChecks.MinCorrelation);
            Assert.LessOrEqual(ParityChecks.MaxAbsErrorLimit, 1e-5);
            Assert.LessOrEqual(ParityChecks.StftDistanceLimit, 1e-5);
        }

        [Test]
        public void MissingReferencesFailUnlessExplicitlyAllowed()
        {
            string previous = Environment.GetEnvironmentVariable(AudioTestEnvironment.AllowMissingVariable);
            try
            {
                Environment.SetEnvironmentVariable(AudioTestEnvironment.AllowMissingVariable, null);
                AssertionException failure = Assert.Throws<AssertionException>(() => AudioTestEnvironment.ReportMissing("out/ref is missing"));
                StringAssert.Contains("node render-ref.mjs", failure.Message);

                Environment.SetEnvironmentVariable(AudioTestEnvironment.AllowMissingVariable, "1");
                Assert.Throws<IgnoreException>(() => AudioTestEnvironment.ReportMissing("out/ref is missing"));
            }
            finally
            {
                Environment.SetEnvironmentVariable(AudioTestEnvironment.AllowMissingVariable, previous);
            }
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
                AudioTestEnvironment.ReportMissing("tools/parity/audio/trace-golden.json not found");
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
