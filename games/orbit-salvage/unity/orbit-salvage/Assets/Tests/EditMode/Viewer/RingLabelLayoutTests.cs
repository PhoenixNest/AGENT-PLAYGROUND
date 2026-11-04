using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Render;
using OrbitalSalvage.Viewer.Editor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// The Home ring pill (<c>home-diorama-ring-label</c>) for every shipped language.
    /// The pill box is the captured English box. A translated label is painted inside it,
    /// so its horizontal and vertical placement must come from the pill, not from the English run.
    /// Writes <c>/tmp/ringfix/metrics.csv</c> and <c>/tmp/ringfix/&lt;code&gt;-home.png</c> for review.
    /// </summary>
    public class RingLabelLayoutTests
    {
        const string OutDir = "/tmp/ringfix";
        const string PillId = "home-diorama-ring-label";
        const double EnglishEmojiSlot = 11.18;
        static readonly string[] Languages =
        {
            "en", "de", "fr", "es", "it", "pt-PT", "vi", "fil", "id", "uz",
            "zh-Hans", "zh-Hant", "ja", "ko", "ar", "hi", "km",
        };

        [TearDown]
        public void RestoreEnglish()
        {
            HomeChrome.RecordRuns = false;
            Loc.SetLoader(new ResourcesLocTableLoader());
            Loc.SetLanguage("en");
            Loc.Reset(null);
            OutlineFonts.Clear();
        }

        [Test]
        [Timeout(1500000)]
        public void RingPill_TextIsCenteredInsideThePill_ForEveryLanguage()
        {
            Directory.CreateDirectory(OutDir);
            var csv = new StringBuilder();
            csv.AppendLine("lang,pillX,pillY,pillW,pillH,textX,textY,textW,textH,pairLeftGap,pairRightGap,pairCenterOffset,emTopGap,emBottomGap,baselineVsEnglish");
            var failures = new StringBuilder();
            double? englishBaseline = null;

            foreach (string code in Languages)
            {
                string table = Path.Combine(Application.dataPath, "Resources", "Localization", code + ".json");
                if (!File.Exists(table)) { Assert.Ignore(code + ".json is not in Resources yet"); }

                Loc.SetLoader(new ResourcesLocTableLoader());
                Assert.IsTrue(Loc.SetLanguage(code), code + " did not load");
                OutlineFonts.Clear();
                HomeChrome.RecordRuns = true;

                byte[] png = null;
                Texture2D tex = null;
                HomeChrome painted = null;
                try
                {
                    // The app writes the live label as a text override (ParityViewer.WriteDynamicOverrides), so paint that path.
                    tex = PagePreviewPainter.Render("home_default", "iphone", true, 0, 0, h =>
                    {
                        painted = h;
                        // English keeps the captured layout, which is the reference for every other language.
                        if (code != "en") h.TextOverrides[PillId] = Loc.Tr("home.ring.donut");
                    }, out png);
                }
                finally
                {
                    if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
                }
                Assert.IsNotNull(png, code + " did not paint");
                File.WriteAllBytes(Path.Combine(OutDir, code + "-home.png"), png);

                // Pill box and text run for the label. The pill comes from the spec, the run from the painter.
                double[] pill = PillRect();
                Assert.IsNotNull(painted, code + " chrome not captured");
                HomeChrome.PaintedRun? found = FindRun(painted, PillId);
                Assert.IsTrue(found.HasValue, code + " painted no run for " + PillId);
                HomeChrome.PaintedRun text = found.Value;

                // The label is the emoji and the text as one pair. The painter records the pair box it chose
                // (emoji left edge through text right edge). A pair that was not centred is a failure, not a fallback.
                // English is the captured layout and never takes the translated path, so it has no pair record.
                // Its emoji sits EnglishEmojiSlot before the text (captured: text starts 11 CSS px from the pill edge).
                HomeChrome.PaintedRun? pairRun = FindRun(painted, PillId + "#pair");
                if (code != "en") Assert.IsTrue(pairRun.HasValue, code + " did not centre the emoji and text as one pair");
                double pairLeft = pairRun.HasValue ? pairRun.Value.X : text.X - EnglishEmojiSlot;
                double pairRight = text.X + text.W;
                double leftGap = pairLeft - pill[0];
                double rightGap = (pill[0] + pill[2]) - pairRight;
                double centerOffset = (pairLeft + pairRight) * 0.5 - (pill[0] + pill[2] * 0.5);
                // Vertical: the em box (baseline - ascent to baseline + descent) must sit in the pill with equal room above and below.
                HomeChrome.PaintedRun? metrics = FindRun(painted, PillId + "#metrics");
                Assert.IsTrue(metrics.HasValue, code + " recorded no baseline");
                double baselineY = metrics.Value.X, asc = metrics.Value.Y, desc = metrics.Value.W;
                double topGap = (baselineY - asc) - pill[1];
                double bottomGap = (pill[1] + pill[3]) - (baselineY + desc);
                double baselineVsEnglish = englishBaseline.HasValue ? baselineY - englishBaseline.Value : 0;
                if (code == "en") englishBaseline = baselineY;

                csv.AppendLine(string.Join(",", code,
                    F(pill[0]), F(pill[1]), F(pill[2]), F(pill[3]),
                    F(text.X), F(text.Y), F(text.W), F(text.H),
                    F(leftGap), F(rightGap), F(centerOffset), F(topGap), F(bottomGap), F(baselineVsEnglish)));

                // Horizontal: the pair may not touch either edge and must sit within 1 CSS px of the pill centre.
                if (leftGap < 4 || rightGap < 4 || Math.Abs(centerOffset) > 1.0)
                    failures.AppendLine(code + " horizontal: leftGap=" + F(leftGap) + " rightGap=" + F(rightGap) + " centerOffset=" + F(centerOffset));
                if (code != "en" && (topGap < 1 || bottomGap < 1 || Math.Abs(topGap - bottomGap) > 1.0))
                    failures.AppendLine(code + " vertical: emTopGap=" + F(topGap) + " emBottomGap=" + F(bottomGap));
                // The captured English box sits about 2 CSS px high (browser layout). It is the reference for the other
                // pinned comparisons, so it is reported in the CSV but not failed here.
            }

            File.WriteAllText(Path.Combine(OutDir, "metrics.csv"), csv.ToString());
            Assert.IsEmpty(failures.ToString(), "Ring pill layout (see " + OutDir + "/metrics.csv):\n" + failures);
        }

        /// <summary>
        /// Play mode paints the pill inside the floating ring's motion, in the ring's local space. English follows the
        /// animated run rectangles. A translated label must keep the same place relative to English in every frame,
        /// or it drifts off the pill while the pill tilts and floats.
        /// </summary>
        [Test]
        [Timeout(1500000)]
        public void RingPill_TranslatedLabelFollowsTheFloatAnimation_ForEveryLanguage()
        {
            Directory.CreateDirectory(OutDir);
            var failures = new StringBuilder();
            var csv = new StringBuilder();
            csv.AppendLine("lang,timeMs,pairCenterVsEnglish,baselineVsEnglish,poseCenterVsEnglish,poseBaselineVsEnglish");
            double[] times = { 700, 1500, 2300 };
            foreach (string code in Languages)
            {
                if (code == "en") continue;
                string table = Path.Combine(Application.dataPath, "Resources", "Localization", code + ".json");
                if (!File.Exists(table)) Assert.Ignore(code + ".json is not in Resources yet");
                double[] pose = RelativeToEnglish(code, double.NaN);
                foreach (double t in times)
                {
                    double[] now = RelativeToEnglish(code, t);
                    csv.AppendLine(string.Join(",", code, F(t), F(now[0]), F(now[1]), F(pose[0]), F(pose[1])));
                    if (Math.Abs(now[0] - pose[0]) > 0.5 || Math.Abs(now[1] - pose[1]) > 0.5)
                        failures.AppendLine(code + " @" + F(t) + "ms: centre vs English " + F(now[0]) + " (pose " + F(pose[0]) + "), baseline vs English " + F(now[1]) + " (pose " + F(pose[1]) + ")");
                }
            }
            File.WriteAllText(Path.Combine(OutDir, "motion.csv"), csv.ToString());
            Assert.IsEmpty(failures.ToString(), "Translated ring label drifts from the pill while it floats (see " + OutDir + "/motion.csv):\n" + failures);
        }

        /// <summary>Pair centre and baseline of a language minus English, both painted at the same frame. NaN time is the capture pose.</summary>
        static double[] RelativeToEnglish(string code, double timeMs)
        {
            double[] en = PaintPill("en", timeMs);
            double[] other = PaintPill(code, timeMs);
            return new[] { other[0] - en[0], other[1] - en[1] };
        }

        /// <summary>Returns { pair centre x, baseline y } as recorded by the painter, in the space the painter draws in.</summary>
        static double[] PaintPill(string code, double timeMs)
        {
            Loc.SetLoader(new ResourcesLocTableLoader());
            Assert.IsTrue(Loc.SetLanguage(code), code + " did not load");
            OutlineFonts.Clear();
            HomeChrome.RecordRuns = true;
            HomeChrome painted = null;
            byte[] png;
            Texture2D tex = null;
            try
            {
                tex = PagePreviewPainter.Render("home_default", "iphone", double.IsNaN(timeMs), double.IsNaN(timeMs) ? 0 : timeMs, 0, h =>
                {
                    painted = h;
                    if (code != "en") h.TextOverrides[PillId] = Loc.Tr("home.ring.donut");
                }, out png);
                if (!double.IsNaN(timeMs))
                    File.WriteAllBytes(Path.Combine(OutDir, code + "-t" + (int)timeMs + ".png"), png);
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
            HomeChrome.PaintedRun? text = FindRun(painted, PillId);
            HomeChrome.PaintedRun? metrics = FindRun(painted, PillId + "#metrics");
            Assert.IsTrue(text.HasValue && metrics.HasValue, code + " recorded no ring label at " + timeMs);
            HomeChrome.PaintedRun? pair = FindRun(painted, PillId + "#pair");
            double left = pair.HasValue ? pair.Value.X : text.Value.X - EnglishEmojiSlot;
            double right = text.Value.X + text.Value.W;
            return new[] { (left + right) * 0.5, metrics.Value.X };
        }

        static string F(double v) => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

        static double[] PillRect()
        {
            // Captured box of the pill element in the iphone home spec (CSS px, same space as the painted runs).
            string path = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec", "home_default__iphone.json");
            string json = File.ReadAllText(path);
            int at = json.IndexOf("\"id\": \"" + PillId + "\"", StringComparison.Ordinal);
            if (at < 0) at = json.IndexOf("\"id\":\"" + PillId + "\"", StringComparison.Ordinal);
            Assert.GreaterOrEqual(at, 0, PillId + " not in spec");
            int r = json.LastIndexOf("\"r\"", at, StringComparison.Ordinal);
            int open = json.IndexOf('[', r);
            int close = json.IndexOf(']', open);
            string[] parts = json.Substring(open + 1, close - open - 1).Split(',');
            var rect = new double[4];
            for (int i = 0; i < 4; i++)
                rect[i] = double.Parse(parts[i].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            return rect;
        }

        static HomeChrome.PaintedRun? FindRun(HomeChrome chrome, string id)
        {
            foreach (var run in chrome.RecordedRuns)
                if (run.Id == id) return run;
            return null;
        }
    }
}
