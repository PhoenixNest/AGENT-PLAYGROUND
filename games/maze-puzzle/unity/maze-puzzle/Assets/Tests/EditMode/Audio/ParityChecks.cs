using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OrbitalSalvage.AudioDsp.Tests
{
    public sealed class ComparisonRow
    {
        public string Id;
        public AudioComparison Metrics;
        public bool Pass;
        public string Note;
    }

    public static class ParityChecks
    {
        public const double MinCorrelation = 0.99;
        public const double MaxAbsErrorLimit = 1e-5;
        public const double StftDistanceLimit = 1e-5;
        public const double TraceTolerance = 1e-6;

        public static string FindAudioDir(string startDir)
        {
            string env = Environment.GetEnvironmentVariable("PARITY_AUDIO_DIR");
            if (!string.IsNullOrEmpty(env) && Directory.Exists(env))
            {
                return env;
            }

            DirectoryInfo dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "tools", "parity", "audio");
                if (File.Exists(Path.Combine(candidate, "ref-manifest.json")))
                {
                    return candidate;
                }

                dir = dir.Parent;
            }

            return null;
        }

        static bool MatchesFilter(string id, string filter)
        {
            if (filter[0] == '=')
            {
                return id == filter.Substring(1);
            }

            return id.IndexOf(filter, StringComparison.Ordinal) >= 0;
        }

        public static List<ComparisonRow> CompareAgainstReferences(string audioDir, string filter, string compareDir)
        {
            List<ParityScenario> manifest = ScenarioRunner.LoadManifest(Path.Combine(audioDir, "ref-manifest.json"));
            Dictionary<string, string> localHashes = LoadLocalHashes(Path.Combine(audioDir, "out", "ref"));
            List<ComparisonRow> rows = new List<ComparisonRow>();
            foreach (ParityScenario sc in manifest)
            {
                if (!string.IsNullOrEmpty(filter) && !MatchesFilter(sc.Id, filter))
                {
                    continue;
                }

                ComparisonRow row = new ComparisonRow();
                row.Id = sc.Id;
                string refPath = Path.Combine(audioDir, "out", "ref", sc.Id + ".wav");
                if (!File.Exists(refPath))
                {
                    row.Metrics = new AudioComparison();
                    row.Pass = false;
                    row.Note = "missing reference " + refPath + " (" + AudioTestEnvironment.RegenerateHint + ")";
                    rows.Add(row);
                    continue;
                }

                // 1. Corruption: the file must hash to what render-ref.mjs (or hash-ref.mjs) recorded locally.
                string localHash;
                string corruption = CheckLocalHash(refPath, sc.Id, localHashes, out localHash);
                if (corruption != null)
                {
                    row.Metrics = new AudioComparison();
                    row.Pass = false;
                    row.Note = corruption;
                    rows.Add(row);
                    continue;
                }

                // 2. Staleness: the committed statistics and fingerprint describe the current audio.js and scenarios.
                int rate;
                float[][] reference = WavIo.ReadFloat32(refPath, out rate);
                string stale = CheckCommittedStats(sc, reference);
                if (stale != null)
                {
                    row.Metrics = new AudioComparison();
                    row.Pass = false;
                    row.Note = "stale reference: " + stale + " (" + AudioTestEnvironment.RegenerateHint + ")";
                    rows.Add(row);
                    continue;
                }

                ScenarioResult result = ScenarioRunner.Run(sc, ReadbackMode.Rendered, false, true);
                row.Metrics = AudioMetrics.Compare(reference, result.Output);

                bool lengthOk = reference[0].Length == result.Output[0].Length;
                bool silentRef = row.Metrics.ReferenceRms == 0;
                List<string> failures = new List<string>();
                if (!lengthOk)
                {
                    failures.Add("length " + result.Output[0].Length + " != " + reference[0].Length);
                }

                if (silentRef)
                {
                    row.Note = "silent reference";
                    if (row.Metrics.MaxAbsError != 0)
                    {
                        failures.Add("silent reference but max abs err " + row.Metrics.MaxAbsError.ToString("E2", CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    if (!(row.Metrics.Correlation >= MinCorrelation))
                    {
                        failures.Add("correlation " + row.Metrics.Correlation.ToString("R", CultureInfo.InvariantCulture) + " < " + MinCorrelation.ToString(CultureInfo.InvariantCulture));
                    }

                    if (!(row.Metrics.MaxAbsError <= MaxAbsErrorLimit))
                    {
                        failures.Add("max abs err " + row.Metrics.MaxAbsError.ToString("E3", CultureInfo.InvariantCulture) + " > " + MaxAbsErrorLimit.ToString("E0", CultureInfo.InvariantCulture));
                    }

                    if (!(row.Metrics.StftDistance <= StftDistanceLimit))
                    {
                        failures.Add("stft distance " + row.Metrics.StftDistance.ToString("E3", CultureInfo.InvariantCulture) + " > " + StftDistanceLimit.ToString("E0", CultureInfo.InvariantCulture));
                    }
                }

                row.Pass = failures.Count == 0;
                if (failures.Count > 0)
                {
                    row.Note = string.Join("; ", failures.ToArray());
                }
                if (!string.IsNullOrEmpty(compareDir))
                {
                    int gap = (int)(0.25 * OfflineRenderer.SampleRate);
                    float[][] both = WavIo.Concat(reference, result.Output, gap);
                    WavIo.WriteFloat32(Path.Combine(compareDir, sc.Id + ".wav"), both, (int)OfflineRenderer.SampleRate);
                }

                rows.Add(row);
            }

            return rows;
        }

        // Committed statistics tolerance (absolute). Chromium re-renders differ by ~3e-8; the manifest is rounded to 1e-7.
        public const double StatsTolerance = 1e-6;
        public const int FingerprintSegments = 32;
        public const string LocalManifestName = "manifest.local.json";

        // File name -> sha256 of the WAV bytes; null when out/ref/manifest.local.json does not exist.
        public static Dictionary<string, string> LoadLocalHashes(string refDir)
        {
            string path = Path.Combine(refDir, LocalManifestName);
            if (!File.Exists(path))
            {
                return null;
            }

            Dictionary<string, object> root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(path));
            Dictionary<string, string> hashes = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object> kv in (Dictionary<string, object>)root["files"])
            {
                hashes[kv.Key] = (string)((Dictionary<string, object>)kv.Value)["sha256"];
            }

            return hashes;
        }

        public static string Sha256FileHex(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream fs = File.OpenRead(path))
            {
                StringBuilder hex = new StringBuilder();
                foreach (byte b in sha.ComputeHash(fs))
                {
                    hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }

                return hex.ToString();
            }
        }

        // Returns null when the file matches manifest.local.json, otherwise a failure note.
        static string CheckLocalHash(string refPath, string id, Dictionary<string, string> localHashes, out string actual)
        {
            actual = null;
            if (localHashes == null)
            {
                return "missing " + LocalManifestName + " next to the reference WAVs (" + AudioTestEnvironment.RegenerateHint + "; for WAVs copied from elsewhere: node hash-ref.mjs)";
            }

            string expected;
            if (!localHashes.TryGetValue(id + ".wav", out expected))
            {
                return "reference " + id + ".wav is not listed in " + LocalManifestName + " (" + AudioTestEnvironment.RegenerateHint + ")";
            }

            actual = Sha256FileHex(refPath);
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            {
                return "corrupt reference: sha256 " + actual + " != " + LocalManifestName + " " + expected + " (" + AudioTestEnvironment.RegenerateHint + ")";
            }

            return null;
        }

        public static double[][] Fingerprint(float[][] channels)
        {
            double[][] result = new double[channels.Length][];
            for (int c = 0; c < channels.Length; c++)
            {
                float[] ch = channels[c];
                result[c] = new double[FingerprintSegments];
                for (int s = 0; s < FingerprintSegments; s++)
                {
                    int from = (int)((long)s * ch.Length / FingerprintSegments);
                    int to = (int)((long)(s + 1) * ch.Length / FingerprintSegments);
                    double sum = 0;
                    for (int i = from; i < to; i++)
                    {
                        sum += (double)ch[i] * ch[i];
                    }

                    result[c][s] = to > from ? Math.Sqrt(sum / (to - from)) : 0;
                }
            }

            return result;
        }

        // Returns null when length, rms, peak and the fingerprint match the committed manifest within StatsTolerance.
        public static string CheckCommittedStats(ParityScenario sc, float[][] reference)
        {
            if (sc.Fingerprint == null)
            {
                return "ref-manifest.json entry " + sc.Id + " has no fingerprint";
            }

            if (reference.Length != sc.Fingerprint.Length)
            {
                return "channels " + reference.Length + " != manifest " + sc.Fingerprint.Length;
            }

            if (sc.Frames > 0 && reference[0].Length != sc.Frames)
            {
                return "frames " + reference[0].Length + " != manifest " + sc.Frames;
            }

            double sum = 0;
            double peak = 0;
            long count = 0;
            foreach (float[] ch in reference)
            {
                for (int i = 0; i < ch.Length; i++)
                {
                    double v = ch[i];
                    sum += v * v;
                    peak = Math.Max(peak, Math.Abs(v));
                    count++;
                }
            }

            double rms = count > 0 ? Math.Sqrt(sum / count) : 0;
            if (Math.Abs(rms - sc.Rms) > StatsTolerance)
            {
                return "rms " + rms.ToString("R", CultureInfo.InvariantCulture) + " != manifest " + sc.Rms.ToString("R", CultureInfo.InvariantCulture);
            }

            if (Math.Abs(peak - sc.Peak) > StatsTolerance)
            {
                return "peak " + peak.ToString("R", CultureInfo.InvariantCulture) + " != manifest " + sc.Peak.ToString("R", CultureInfo.InvariantCulture);
            }

            double[][] fp = Fingerprint(reference);
            for (int c = 0; c < fp.Length; c++)
            {
                if (fp[c].Length != sc.Fingerprint[c].Length)
                {
                    return "fingerprint length " + fp[c].Length + " != manifest " + sc.Fingerprint[c].Length;
                }

                for (int s = 0; s < fp[c].Length; s++)
                {
                    if (Math.Abs(fp[c][s] - sc.Fingerprint[c][s]) > StatsTolerance)
                    {
                        return "fingerprint channel " + c + " segment " + s + " " + fp[c][s].ToString("R", CultureInfo.InvariantCulture)
                            + " != manifest " + sc.Fingerprint[c][s].ToString("R", CultureInfo.InvariantCulture);
                    }
                }
            }

            return null;
        }

        public static string Sha256Hex(float[][] channels)
        {
            using (SHA256 sha = SHA256.Create())
            {
                foreach (float[] channel in channels)
                {
                    byte[] bytes = new byte[channel.Length * 4];
                    Buffer.BlockCopy(channel, 0, bytes, 0, bytes.Length);
                    sha.TransformBlock(bytes, 0, bytes.Length, null, 0);
                }

                sha.TransformFinalBlock(new byte[0], 0, 0);
                StringBuilder hex = new StringBuilder();
                foreach (byte b in sha.Hash)
                {
                    hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }

                return hex.ToString();
            }
        }

        public static string FormatTable(List<ComparisonRow> rows)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Gates: correlation >= " + MinCorrelation.ToString(CultureInfo.InvariantCulture)
                + ", max abs err <= " + MaxAbsErrorLimit.ToString("E0", CultureInfo.InvariantCulture)
                + ", stft dist <= " + StftDistanceLimit.ToString("E0", CultureInfo.InvariantCulture) + ", file sha256 matches manifest.local.json, rms/peak/fingerprint match ref-manifest.json within 1e-6.");
            sb.AppendLine();
            sb.AppendLine("| cue/args | correlation | 1 - corr | max abs err | rms err | stft dist | pass |");
            sb.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            double worstMax = 0;
            double worstStft = 0;
            double worstOneMinusCorr = 0;
            foreach (ComparisonRow r in rows)
            {
                double oneMinus = Math.Abs(1.0 - r.Metrics.Correlation);
                worstMax = Math.Max(worstMax, r.Metrics.MaxAbsError);
                worstStft = Math.Max(worstStft, r.Metrics.StftDistance);
                worstOneMinusCorr = Math.Max(worstOneMinusCorr, oneMinus);
                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "| {0} | {1:F12} | {2:E2} | {3:E2} | {4:E2} | {5:E2} | {6} |",
                    r.Id,
                    r.Metrics.Correlation,
                    oneMinus,
                    r.Metrics.MaxAbsError,
                    r.Metrics.RmsError,
                    r.Metrics.StftDistance,
                    r.Pass ? "yes" : "NO: " + r.Note));
            }

            sb.AppendLine();
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "Worst case over {0} scenarios: 1 - corr = {1:E2}, max abs err = {2:E2}, stft dist = {3:E2}.",
                rows.Count,
                worstOneMinusCorr,
                worstMax,
                worstStft));
            return sb.ToString();
        }
        public static List<string> RngCountFailures(string audioDir)
        {
            Dictionary<string, object> root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(Path.Combine(audioDir, "rng-counts.json")));
            Dictionary<string, object> scenarios = (Dictionary<string, object>)root["scenarios"];
            List<string> failures = new List<string>();
            foreach (KeyValuePair<string, object> kv in scenarios)
            {
                Dictionary<string, object> entry = (Dictionary<string, object>)kv.Value;
                int expected = (int)(double)entry["draws"];
                ParityScenario sc = ParityScenario.FromJson((Dictionary<string, object>)entry["scenario"]);
                ScenarioResult result = ScenarioRunner.Run(sc, ReadbackMode.Stub, false, false);
                if (result.RngDraws != expected)
                {
                    failures.Add(kv.Key + ": expected " + expected + " draws, got " + result.RngDraws);
                }
            }

            return failures;
        }

        public static int RngScenarioCount(string audioDir)
        {
            Dictionary<string, object> root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(Path.Combine(audioDir, "rng-counts.json")));
            return ((Dictionary<string, object>)root["scenarios"]).Count;
        }

        static object Normalize(object o)
        {
            if (o is int)
            {
                return (double)(int)o;
            }

            if (o is float)
            {
                return (double)(float)o;
            }

            if (o is long)
            {
                return (double)(long)o;
            }

            return o;
        }

        static List<object> FlattenGolden(List<object> raw)
        {
            List<object> flat = new List<object>();
            foreach (object o in raw)
            {
                Dictionary<string, object> d = o as Dictionary<string, object>;
                if (d != null)
                {
                    flat.Add(d["length"]);
                    flat.Add(d["sampleRate"]);
                    flat.Add(d["channels"]);
                }
                else
                {
                    flat.Add(o);
                }
            }

            return flat;
        }

        public static List<string> TraceFailures(string audioDir)
        {
            Dictionary<string, object> root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(Path.Combine(audioDir, "trace-golden.json")));
            Dictionary<string, object> traces = (Dictionary<string, object>)root["traces"];
            Dictionary<string, object> counts = (Dictionary<string, object>)((Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(Path.Combine(audioDir, "rng-counts.json"))))["scenarios"];
            List<string> failures = new List<string>();

            foreach (KeyValuePair<string, object> kv in traces)
            {
                ParityScenario sc = ParityScenario.FromJson((Dictionary<string, object>)((Dictionary<string, object>)counts[kv.Key])["scenario"]);
                ScenarioResult result = ScenarioRunner.Run(sc, ReadbackMode.Stub, true, false);
                List<object> golden = (List<object>)kv.Value;
                List<object[]> actual = result.Trace.Events;

                if (golden.Count != actual.Count)
                {
                    failures.Add(kv.Key + ": " + golden.Count + " golden events, " + actual.Count + " actual");
                    continue;
                }

                for (int i = 0; i < golden.Count; i++)
                {
                    string diff = DiffEvent(FlattenGolden((List<object>)golden[i]), actual[i]);
                    if (diff != null)
                    {
                        failures.Add(kv.Key + " event " + i + ": " + diff);
                        break;
                    }
                }
            }

            return failures;
        }

        static string DiffEvent(List<object> golden, object[] actual)
        {
            if (golden.Count != actual.Length)
            {
                return "arity " + golden.Count + " vs " + actual.Length + " (" + golden[0] + ")";
            }

            for (int i = 0; i < golden.Count; i++)
            {
                object g = golden[i];
                object a = Normalize(actual[i]);
                if (g is double && a is double)
                {
                    if (Math.Abs((double)g - (double)a) > TraceTolerance)
                    {
                        return "arg " + i + " of " + golden[0] + ": " + g + " vs " + a;
                    }
                }
                else if (!Equals(g, a))
                {
                    return "arg " + i + " of " + golden[0] + ": " + (g ?? "null") + " vs " + (a ?? "null");
                }
            }

            return null;
        }
    }
}
