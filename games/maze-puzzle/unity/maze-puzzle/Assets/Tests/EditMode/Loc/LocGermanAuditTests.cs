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
    /// Real Deutsch table from Resources, same loader the app uses. Hard layout findings fail.
    /// Soft shrink and ellipsis are written under /tmp and are not asserted.
    /// </summary>
    public class LocGermanAuditTests
    {
        const string OutDir = "/tmp/maze-i18n/de-audit";
        const string ReportPath = OutDir + "/overflow-de.txt";

        static readonly string[] HardKinds =
        {
            "hard-outside",
            "hard-overlap",
            "hard-band",
            "wrapped-in-control",
            "missing-glyph"
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

        [Test]
        public void GermanTable_HardFindingsFail_AndEnglishPaintStaysPut()
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
            Assert.AreEqual(englishHome, Snapshot("home_default", "zero"), "loading de.json changed the English paint");

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

            Assert.IsTrue(Loc.SetLanguage("de"));
            Assert.AreEqual("de", Loc.CurrentLanguage, "de.json did not load from Resources");
            Assert.IsFalse(Loc.EnglishActive);

            LocOverflowAudit.Report report = LocOverflowAudit.Run(dir);
            Directory.CreateDirectory(OutDir);
            File.WriteAllText(ReportPath, report.Text + PlayfieldAppendix());
            Assert.IsTrue(File.Exists(ReportPath));
            Assert.That(File.ReadAllText(ReportPath), Does.StartWith("language=de"));
            Assert.Greater(report.Runs, 0);
            Assert.AreEqual(90, report.Files);

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
            Assert.AreEqual(0, report.HardOutside + report.HardOverlap + report.HardBand + report.HardWrapped + report.HardMissing);
            Assert.AreEqual(0, hardCount, hard.ToString());

            WritePlayfield(Path.Combine(OutDir, "stills", "playfield-live.png"));

            Assert.IsTrue(Loc.SetLanguage("en"));
            Assert.IsTrue(Loc.EnglishActive);
            Assert.AreEqual(english.Text, LocOverflowAudit.Run(dir).Text, "English audit changed after German");
            Assert.AreEqual(englishHome, Snapshot("home_default", "zero"), "English home paint changed after German");
            Assert.AreEqual("\u26A1 \u00D7" + JsNumber.ToString(2d), labels.Zap(2d));
            Assert.AreEqual("\u23E9 BOOST", labels.Boost());
        }

        static string PlayfieldAppendix()
        {
            var sb = new StringBuilder();
            sb.Append("\n# playfield Loc.Tr while language=de\n");
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
    }
}
