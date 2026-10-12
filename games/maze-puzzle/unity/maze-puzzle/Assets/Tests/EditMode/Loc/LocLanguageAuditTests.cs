using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Loc.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// Real Latin-script tables from Resources, same loader the app uses.
    /// Hard layout findings fail (outside, overlap, band, wrapped-in-control, missing-glyph, wider).
    /// Soft shrink and ellipsis are written under /tmp and are not asserted.
    /// </summary>
    public class LocLanguageAuditTests
    {
        const string Root = "/tmp/maze-i18n/audit-all";

        static readonly string[] HardKinds =
        {
            "hard-outside",
            "hard-overlap",
            "hard-band",
            "hard-wider",
            "wrapped-in-control",
            "missing-glyph",
            "icon-overlap"
        };

        static readonly string[] PlayfieldKeys =
        {
            "fx.swallow.nom", "fx.swallow.yum", "fx.swallow.gulp", "fx.swallow.tasty",
            "fx.gate.zap", "fx.gate.shred", "fx.gate.magnet", "fx.gate.boost",
            "fx.hub.orbital", "fx.hub.sort", "fx.hub.donut", "fx.hub.sweet",
            "fx.shred", "fx.compact", "fx.boost", "fx.poof", "fx.turbo.cw", "fx.turbo.ccw",
            "unit.kg", "unit.quota", "pause.quota", "hud.sector_code"
        };

        [SetUp]
        public void ResetBefore()
        {
            HomeChrome.RecordRuns = false;
            Loc.Reset(null);
        }

        [TearDown]
        public void RestoreEnglish()
        {
            HomeChrome.RecordRuns = false;
            Loc.SetLoader(new ResourcesLocTableLoader());
            Loc.SetLanguage("en");
            Assert.IsTrue(Loc.EnglishActive);
            Loc.Reset(null);
        }

        [TestCase("de")]
        [TestCase("fr")]
        [TestCase("es")]
        [TestCase("it")]
        [TestCase("pt-PT")]
        [TestCase("vi")]
        [TestCase("fil")]
        [TestCase("id")]
        [TestCase("uz")]
        [Timeout(1200000)]
        public void Language_HardFindingsFail_AndEnglishPaintStaysPut(string code)
        {
            string dir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            string englishHome = Snapshot("home_default", "zero");
            Assert.That(englishHome, Does.Contain("PLAY MISSION"));

            var labels = new GameplayLabels();
            string zap = labels.Zap(2d);
            Assert.AreEqual("\u26A1 \u00D7" + JsNumber.ToString(2d), zap);
            Assert.AreSame(zap, labels.Zap(2d));
            Assert.AreEqual("\u23E9 BOOST", labels.Boost());
            string emoji;
            string sub;
            labels.Hub("ring_donut", false, out emoji, out sub);
            Assert.AreEqual("\U0001F369 DONUT \U0001F369", emoji);
            Assert.AreEqual("SWEET RIG", sub);

            Loc.SetLoader(new ResourcesLocTableLoader());
            Assert.IsTrue(Loc.SetLanguage("en"));
            Assert.IsTrue(Loc.EnglishActive);
            Assert.AreEqual(englishHome, Snapshot("home_default", "zero"), "loading " + code + ".json changed the English paint");

            var session = new LiveGameSession(
                new PhysicsEngine(new ParityAudioCues(() => 0)),
                null,
                new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0)));
            session.ActiveSector = new SectorConfig { Quota = 20 };
            session.CollectedMass = 12;
            Assert.AreEqual(JsNumber.ToString(12d) + " / " + JsNumber.ToString(20d) + " kg", session.QuotaLabel());

            LocOverflowAudit.Report english = LocOverflowAudit.Run(dir);
            Assert.AreEqual(90, english.Files, "zero + iphone spec count");
            Assert.AreEqual(0, english.Runs, "English does not translate spec runs");
            Assert.AreEqual(0, english.Hard);
            Assert.AreEqual(0, english.Soft);

            Assert.IsTrue(Loc.SetLanguage(code), code + ".json did not load from Resources");
            Assert.AreEqual(code, Loc.CurrentLanguage);
            Assert.IsFalse(Loc.EnglishActive);

            LocOverflowAudit.Report report = LocOverflowAudit.Run(dir);
            string outDir = Path.Combine(Root, code);
            Directory.CreateDirectory(outDir);
            string reportPath = Path.Combine(outDir, "overflow.txt");
            var text = new StringBuilder();
            text.Append(report.Text);
            text.Append(PlayfieldAppendix());
            text.Append(IdenticalAppendix(code));
            text.Append(SoftAppendix(dir, report));
            File.WriteAllText(reportPath, text.ToString());
            Assert.IsTrue(File.Exists(reportPath));
            Assert.That(File.ReadAllText(reportPath), Does.StartWith("language=" + code));
            Assert.Greater(report.Runs, 0, code);
            Assert.AreEqual(90, report.Files);

            WritePlayfield(Path.Combine(outDir, "stills", "playfield-live.png"));

            Assert.IsTrue(Loc.SetLanguage("en"));
            Assert.IsTrue(Loc.EnglishActive);
            Assert.AreEqual(english.Text, LocOverflowAudit.Run(dir).Text, "English audit changed after " + code);
            Assert.AreEqual(englishHome, Snapshot("home_default", "zero"), "English home paint changed after " + code);
            Assert.AreEqual("\u26A1 \u00D7" + JsNumber.ToString(2d), labels.Zap(2d));
            Assert.AreEqual("\u23E9 BOOST", labels.Boost());

            var hard = new StringBuilder();
            int hardCount = 0;
            for (int i = 0; i < report.Findings.Count; i++)
            {
                LocOverflowAudit.Finding finding = report.Findings[i];
                if (Array.IndexOf(HardKinds, finding.Kind) < 0) continue;
                hardCount++;
                hard.Append(finding.Kind).Append('\t').Append(finding.File).Append('\t')
                    .Append(finding.ElementId).Append('\t').Append(finding.Detail).Append('\n');
            }
            Assert.AreEqual(0, report.Hard, hard.ToString());
            Assert.AreEqual(0, hardCount, hard.ToString());
        }

        static string PlayfieldAppendix()
        {
            var sb = new StringBuilder();
            sb.Append("\n# playfield Loc.Tr while language=").Append(Loc.CurrentLanguage).Append('\n');
            for (int i = 0; i < PlayfieldKeys.Length; i++)
            {
                string key = PlayfieldKeys[i];
                string english;
                LocStrings.TryGet(key, out english);
                sb.Append(key).Append('\t').Append(english ?? "").Append('\t').Append(Loc.Tr(key)).Append('\n');
            }
            sb.Append("unit.quota formatted\t{0} / {1} kg\t").Append(Loc.Tr("unit.quota", 12, 20)).Append('\n');
            sb.Append("fx.gate.zap formatted\t\t").Append(Loc.Tr("fx.gate.zap", 2)).Append('\n');
            return sb.ToString();
        }

        static string IdenticalAppendix(string code)
        {
            string path = Path.Combine(Application.dataPath, "Resources", "Localization", code + ".json");
            var root = MiniJson.Parse(File.ReadAllText(path)) as Dictionary<string, object>;
            var lines = new List<string>();
            AppendIdentical(root, "spec", lines, true);
            AppendIdentical(root, "str", lines, false);
            lines.Sort(StringComparer.Ordinal);
            var sb = new StringBuilder();
            sb.Append("\n# identical-to-english count=").Append(lines.Count.ToString()).Append('\n');
            for (int i = 0; i < lines.Count; i++) sb.Append(lines[i]).Append('\n');
            return sb.ToString();
        }

        static void AppendIdentical(Dictionary<string, object> root, string section, List<string> lines, bool spec)
        {
            if (root == null || !(root.TryGetValue(section, out var raw) && raw is Dictionary<string, object> map)) return;
            foreach (var kv in map)
            {
                string value = kv.Value as string ?? "";
                string english = kv.Key;
                if (!spec)
                {
                    string fromRegistry;
                    LocStrings.TryGet(kv.Key, out fromRegistry);
                    english = fromRegistry ?? "";
                }
                if (!string.Equals(value, english, StringComparison.Ordinal)) continue;
                if (spec) lines.Add("spec\t" + kv.Key);
                else lines.Add("str\t" + kv.Key + "\t" + english);
            }
        }

        static string SoftAppendix(string specDir, LocOverflowAudit.Report report)
        {
            var needed = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < report.Findings.Count; i++)
            {
                string kind = report.Findings[i].Kind;
                if (kind == "soft-shrink" || kind == "soft-ellipsis" || kind == "soft-control-ellipsis" || IsHard(kind))
                    needed.Add(report.Findings[i].File);
            }
            var runsByFile = new Dictionary<string, List<RunRef>>(StringComparer.Ordinal);
            foreach (string file in needed)
                runsByFile[file] = LoadRuns(Path.Combine(specDir, file));

            var shrinks = new Dictionary<string, ShrinkGroup>(StringComparer.Ordinal);
            var ellipses = new Dictionary<string, EllipsisGroup>(StringComparer.Ordinal);
            var hard = new StringBuilder();
            int hardPairs = 0;
            for (int i = 0; i < report.Findings.Count; i++)
            {
                LocOverflowAudit.Finding finding = report.Findings[i];
                List<RunRef> runs;
                if (!runsByFile.TryGetValue(finding.File, out runs)) runs = null;
                if (finding.Kind == "soft-shrink")
                {
                    string scale = finding.Detail.StartsWith("scale=", StringComparison.Ordinal)
                        ? finding.Detail.Substring("scale=".Length) : finding.Detail;
                    string key = finding.File + "\t" + finding.ElementId + "\t" + scale;
                    ShrinkGroup group;
                    if (!shrinks.TryGetValue(key, out group))
                    {
                        group = new ShrinkGroup { File = finding.File, Id = finding.ElementId, Scale = scale };
                        shrinks[key] = group;
                    }
                    group.Count++;
                    AddCandidates(group.Candidates, runs, finding.ElementId, null);
                }
                else if (finding.Kind == "soft-ellipsis" || finding.Kind == "soft-control-ellipsis")
                {
                    string pairKey = finding.Kind + "\t" + finding.Detail;
                    EllipsisGroup group;
                    if (!ellipses.TryGetValue(pairKey, out group))
                    {
                        group = new EllipsisGroup { Kind = finding.Kind, Shown = finding.Detail, File = finding.File, Id = finding.ElementId };
                        ellipses[pairKey] = group;
                        AddCandidates(group.Matched, runs, null, finding.Detail);
                    }
                    group.Count++;
                }
                else if (IsHard(finding.Kind))
                {
                    hardPairs++;
                    hard.Append(finding.Kind).Append('\t').Append(finding.File).Append('\t')
                        .Append(finding.ElementId).Append('\t').Append(finding.Detail);
                    if (runs != null)
                    {
                        for (int n = 0; n < runs.Count; n++)
                        {
                            RunRef run = runs[n];
                            if (finding.ElementId.Length > 0 && run.Key != finding.ElementId) continue;
                            if (finding.Detail.Length > 0 && finding.Detail != run.Shown && finding.Detail.IndexOf(run.Shown, StringComparison.Ordinal) < 0
                                && run.Shown.IndexOf(finding.Detail, StringComparison.Ordinal) < 0) continue;
                            hard.Append('\t').Append(run.English).Append('\t').Append(run.Translation);
                            break;
                        }
                    }
                    hard.Append('\n');
                }
            }

            var sb = new StringBuilder();
            sb.Append("\n# unique-soft shrinks are scale + english + translation; ambiguous ids list every translated run on that id\n");
            var shrinkLines = new List<string>(shrinks.Count);
            foreach (var kv in shrinks)
            {
                ShrinkGroup group = kv.Value;
                if (group.Candidates.Count == 1)
                {
                    RunRef run = group.Candidates[0];
                    shrinkLines.Add("shrink\t" + group.Scale + "\t" + group.Count.ToString() + "\t" + group.Id
                        + "\t" + run.English + "\t" + run.Translation + "\t" + group.File);
                }
                else
                {
                    var line = new StringBuilder();
                    line.Append("shrink-ambiguous\t").Append(group.Scale).Append('\t').Append(group.Count.ToString())
                        .Append('\t').Append(group.Id).Append('\t').Append(group.File);
                    for (int n = 0; n < group.Candidates.Count; n++)
                    {
                        RunRef run = group.Candidates[n];
                        line.Append('\t').Append(run.English).Append(" | ").Append(run.Translation);
                    }
                    shrinkLines.Add(line.ToString());
                }
            }
            shrinkLines.Sort(StringComparer.Ordinal);
            for (int i = 0; i < shrinkLines.Count; i++) sb.Append(shrinkLines[i]).Append('\n');

            var ellipseLines = new List<string>(ellipses.Count);
            foreach (var kv in ellipses)
            {
                EllipsisGroup group = kv.Value;
                if (group.Matched.Count == 0)
                {
                    ellipseLines.Add(group.Kind + "-unmatched\t" + group.Count.ToString() + "\t" + group.Id
                        + "\t" + group.File + "\t" + group.Shown);
                    continue;
                }
                for (int n = 0; n < group.Matched.Count; n++)
                {
                    RunRef run = group.Matched[n];
                    ellipseLines.Add(group.Kind + "\t" + group.Count.ToString() + "\t" + group.Id + "\t" + group.File
                        + "\t" + run.English + "\t" + run.Translation);
                }
            }
            ellipseLines.Sort(StringComparer.Ordinal);
            for (int i = 0; i < ellipseLines.Count; i++) sb.Append(ellipseLines[i]).Append('\n');

            sb.Append("\n# hard-pairs count=").Append(hardPairs.ToString()).Append('\n');
            sb.Append(hard);
            return sb.ToString();
        }

        static bool IsHard(string kind)
        {
            return Array.IndexOf(HardKinds, kind) >= 0;
        }

        static void AddCandidates(List<RunRef> dest, List<RunRef> runs, string elementId, string shown)
        {
            if (runs == null) return;
            for (int i = 0; i < runs.Count; i++)
            {
                RunRef run = runs[i];
                if (elementId != null && run.Key != elementId) continue;
                if (shown != null && run.Shown != shown && run.Translation != shown) continue;
                bool seen = false;
                for (int n = 0; n < dest.Count; n++)
                {
                    if (dest[n].English == run.English && dest[n].Translation == run.Translation)
                    {
                        seen = true;
                        break;
                    }
                }
                if (!seen) dest.Add(run);
            }
        }

        static List<RunRef> LoadRuns(string path)
        {
            var list = new List<RunRef>();
            if (!File.Exists(path)) return list;
            var root = MiniJson.Parse(File.ReadAllText(path)) as Dictionary<string, object>;
            if (root == null || !(root.TryGetValue("elements", out var raw) && raw is List<object> elements)) return list;
            for (int i = 0; i < elements.Count; i++)
            {
                var e = elements[i] as Dictionary<string, object>;
                if (e == null || Hidden(e)) continue;
                double[] er = Box(e);
                if (er == null || er[2] <= 0 || er[3] <= 0 || er[1] >= 932) continue;
                if (!(e.TryGetValue("runs", out var rv) && rv is List<object> runs)) continue;
                string id = Str(e, "id");
                if (id.Length == 0) id = Str(e, "c");
                bool upper = Str(e, "tt") == "uppercase";
                for (int n = 0; n < runs.Count; n++)
                {
                    var run = runs[n] as Dictionary<string, object>;
                    if (run == null) continue;
                    string source = Collapse(Str(run, "s"));
                    if (!LocText.HasLetter(LocText.Normalize(source))) continue;
                    string translated;
                    if (!Loc.TryTranslateSpecText(source, out translated) || string.IsNullOrEmpty(translated)) continue;
                    list.Add(new RunRef
                    {
                        Key = id,
                        English = source,
                        Translation = translated,
                        Shown = upper ? LocCase.Upper(translated, Loc.CurrentLanguage) : translated
                    });
                }
            }
            return list;
        }

        sealed class RunRef
        {
            public string Key;
            public string English;
            public string Translation;
            public string Shown;
        }

        sealed class ShrinkGroup
        {
            public string File;
            public string Id;
            public string Scale;
            public int Count;
            public readonly List<RunRef> Candidates = new List<RunRef>();
        }

        sealed class EllipsisGroup
        {
            public string Kind;
            public string Shown;
            public string File;
            public string Id;
            public int Count;
            public readonly List<RunRef> Matched = new List<RunRef>();
        }

        static string Snapshot(string spec, string profile)
        {
            HomeChrome.RecordRuns = true;
            HomeChrome chrome = HomeChrome.Load(spec, profile);
            try
            {
                chrome.Draw();
                var parts = new List<string>();
                for (int i = 0; i < chrome.RecordedRuns.Count; i++)
                {
                    string text = chrome.RecordedRuns[i].Text;
                    if (string.IsNullOrEmpty(text)) continue;
                    parts.Add(text);
                }
                parts.Sort(StringComparer.Ordinal);
                return string.Join("\n", parts);
            }
            finally
            {
                chrome.Dispose();
            }
        }

        static void WritePlayfield(string path)
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var renderer = new GameRenderer(430, 932, 2);
            var session = new LiveGameSession(
                new PhysicsEngine(new ParityAudioCues(() => 0)),
                renderer,
                new EconomyManager(new MemoryKeyValueStore(), new FixedWallClock(0)));
            RenderTexture copy = null;
            Texture2D read = null;
            RenderTexture previous = RenderTexture.active;
            try
            {
                session.ApplyLayout(430, 932);
                session.LoadSector(MazeLevelBuilder.GetSectors()[0]);
                session.InjectRecordedVfx();
                session.StepWebFrame(0.016, true);
                RenderTexture source = renderer.Canvas.Target;
                copy = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                copy.Create();
                Graphics.CopyTexture(source, copy);
                RenderTexture.active = copy;
                read = new Texture2D(copy.width, copy.height, TextureFormat.RGB24, false, true);
                read.ReadPixels(new Rect(0, 0, copy.width, copy.height), 0, 0, false);
                read.Apply(false);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, read.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                if (read != null) UnityEngine.Object.DestroyImmediate(read);
                if (copy != null)
                {
                    copy.Release();
                    UnityEngine.Object.DestroyImmediate(copy);
                }
                renderer.Dispose();
            }
        }

        static bool Hidden(Dictionary<string, object> e)
        {
            if (Num(e, "v", 1) == 0) return true;
            if (Str(e, "vis") == "hidden") return true;
            if (Num(e, "o", 1) <= 0) return true;
            string c = " " + Str(e, "c") + " ";
            return c.IndexOf(" page-hidden ", StringComparison.Ordinal) >= 0;
        }

        static double[] Box(Dictionary<string, object> e)
        {
            object v;
            if (!e.TryGetValue("r", out v) || !(v is List<object> l) || l.Count < 4) return null;
            return new[] { NumOf(l[0]), NumOf(l[1]), NumOf(l[2]), NumOf(l[3]) };
        }

        static string Collapse(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            bool space = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i])) { space = sb.Length > 0; continue; }
                if (space) sb.Append(' ');
                space = false;
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        static double Num(Dictionary<string, object> e, string key, double fallback)
        {
            object v;
            if (!e.TryGetValue(key, out v) || v == null) return fallback;
            return NumOf(v);
        }

        static double NumOf(object v)
        {
            if (v is double) return (double)v;
            if (v is long) return (long)v;
            if (v is int) return (int)v;
            double n;
            return double.TryParse(Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture),
                System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        static string Str(Dictionary<string, object> e, string key)
        {
            object v;
            if (!e.TryGetValue(key, out v) || v == null) return "";
            return v as string ?? "";
        }
    }
}
