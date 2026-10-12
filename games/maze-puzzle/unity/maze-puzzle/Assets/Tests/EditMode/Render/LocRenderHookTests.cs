using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public class TextFitTests
    {
        static double Measure(string text, double px)
        {
            return text.Length * px * 0.5;
        }

        [Test]
        public void Anchor_CenterRightLeft_AndButtonPair()
        {
            var centered = new TextFit.Input
            {
                RunX = 100, RunW = 40, ElX = 80, ElW = 80, FontPx = 16, ElH = 20, RunH = 16, SidePad = 16
            };
            Assert.AreEqual(TextFit.Anchor.Center, TextFit.ChooseAnchor(centered));

            var right = new TextFit.Input
            {
                RunX = 40, RunW = 100, ElX = 20, ElW = 120, FontPx = 16, RunH = 16, ElH = 20, SidePad = 16
            };
            Assert.AreEqual(TextFit.Anchor.Right, TextFit.ChooseAnchor(right));

            var left = new TextFit.Input
            {
                RunX = 10, RunW = 40, ElX = 0, ElW = 200, FontPx = 16, RunH = 16, ElH = 20, SidePad = 16
            };
            Assert.AreEqual(TextFit.Anchor.Left, TextFit.ChooseAnchor(left));

            // 288px pause button: the run alone is not within 2px of center, the icon+text pair is.
            var button = new TextFit.Input
            {
                RunX = 166.844, RunY = 651, RunW = 109.109, RunH = 20,
                ElX = 71, ElY = 634, ElW = 288, ElH = 59,
                IsButton = true, FontPx = 16, SidePad = 16,
                Icons = new[] { new TextFit.Icon { Index = 1, X = 154.047, Y = 651, W = 12.797, H = 20 } }
            };
            Assert.AreEqual(TextFit.Anchor.Center, TextFit.ChooseAnchor(button));
        }

        [Test]
        public void Fit_SingleShrinkWrapEllipsis()
        {
            var box = new TextFit.Input
            {
                RunX = 10, RunY = 10, RunW = 40, RunH = 16,
                ElX = 0, ElY = 0, ElW = 80, ElH = 18,
                FontPx = 16, SidePad = 4
            };
            TextFit.Layout same = TextFit.Plan(box, (px, ls) => Measure("Hi", px));
            Assert.AreEqual(TextFit.Action.Single, same.Action);
            Assert.GreaterOrEqual(same.X, box.ElX - 0.5);
            Assert.LessOrEqual(same.X + Measure("Hi", same.FontPx), box.ElX + box.ElW + 0.5);

            var tight = box;
            tight.ElH = 18;
            tight.RunH = 16;
            tight.ElW = 50;
            tight.RunW = 40;
            TextFit.Layout shrunk = TextFit.Plan(tight, (px, ls) => 80 * (px / 16.0));
            Assert.IsTrue(shrunk.Action == TextFit.Action.Shrink || shrunk.Action == TextFit.Action.Ellipsis, shrunk.Action.ToString());
            Assert.GreaterOrEqual(shrunk.Scale, TextFit.FloorRatio - 1e-9);
            Assert.Less(shrunk.Scale, 1);

            var tall = tight;
            tall.IsButton = true;
            tall.ElH = 59;
            tall.RunH = 20;
            TextFit.Layout button = TextFit.Plan(tall, (px, ls) => 200);
            Assert.AreEqual(TextFit.Action.Ellipsis, button.Action);
            Assert.IsTrue(TextFit.LocksToOneLine(tall));

            var block = new TextFit.Input
            {
                RunX = 10, RunY = 10, RunW = 40, RunH = 16,
                ElX = 0, ElY = 0, ElW = 80, ElH = 80,
                FontPx = 16, SidePad = 4
            };
            TextFit.Layout wrapped = TextFit.Plan(block, (px, ls) => 200);
            Assert.AreEqual(TextFit.Action.Wrap, wrapped.Action);
            Assert.GreaterOrEqual(wrapped.MaxLines, 2);

            var floor = tight;
            floor.ElH = 16;
            floor.RunH = 14;
            TextFit.Layout dots = TextFit.Plan(floor, (px, ls) => 400);
            Assert.AreEqual(TextFit.Action.Ellipsis, dots.Action);
            string clipped = TextFit.Ellipsize(new string('A', 40), 30, s => s.Length * 10);
            Assert.IsTrue(clipped.EndsWith("\u2026"), clipped);
            Assert.LessOrEqual(clipped.Length * 10, 30.5);
        }

        [Test]
        public void LeadingIcon_ShiftsWhenThePairFits()
        {
            var input = new TextFit.Input
            {
                RunX = 166.844, RunY = 651, RunW = 109, RunH = 20,
                ElX = 71, ElY = 634, ElW = 288, ElH = 59,
                IsButton = true, FontPx = 16, SidePad = 16,
                Icons = new[] { new TextFit.Icon { Index = 3, X = 154.047, Y = 651, W = 12.797, H = 20 } }
            };
            TextFit.Layout plan = TextFit.Plan(input, (px, ls) => 140);
            Assert.AreEqual(TextFit.Action.Single, plan.Action);
            Assert.AreEqual(3, plan.IconIndex);
            Assert.Greater(Math.Abs(plan.IconShift), 0.01);
            double pairLeft = 154.047 + plan.IconShift;
            double pairRight = plan.X + 140;
            Assert.GreaterOrEqual(pairLeft, input.ElX - 0.5);
            Assert.LessOrEqual(pairRight, input.ElX + input.ElW + 0.5);
            Assert.GreaterOrEqual(plan.X, pairLeft + 12.797 - 0.5);
        }

        [Test]
        public void SingleLineControl_DoesNotWrap_AndFooterCeilingStaysOneLine()
        {
            var view = new TextFit.Input
            {
                RunX = 315.656, RunY = 638.5, RunW = 62.344, RunH = 15,
                ElX = 297.656, ElY = 629.5, ElW = 98.344, ElH = 33,
                IsButton = true, FontPx = 12, SidePad = 16
            };
            TextFit.Layout clipped = TextFit.Plan(view, (px, ls) => 180);
            Assert.IsTrue(TextFit.LocksToOneLine(view));
            Assert.AreNotEqual(TextFit.Action.Wrap, clipped.Action);

            var footer = new TextFit.Input
            {
                RunX = 95, RunY = 856, RunW = 239, RunH = 16,
                ElX = 16, ElY = 856, ElW = 398, ElH = 32,
                FontPx = 12, SidePad = 16, CeilingY = 872
            };
            TextFit.Layout line = TextFit.Plan(footer, (px, ls) => 400);
            Assert.AreNotEqual(TextFit.Action.Wrap, line.Action);
        }

        [Test]
        public void OverlappingIcon_StaysPutUntilTheTranslationIsWider()
        {
            var input = new TextFit.Input
            {
                RunX = 20, RunY = 10, RunW = 40, RunH = 16,
                ElX = 0, ElY = 0, ElW = 120, ElH = 20,
                FontPx = 14, SidePad = 4,
                Icons = new[] { new TextFit.Icon { Index = 2, X = 12, Y = 10, W = 16, H = 16 } }
            };
            TextFit.Layout kept = TextFit.Plan(input, (px, ls) => 30);
            Assert.IsTrue(kept.KeepX);
            Assert.AreEqual(20, kept.X, 0.01);

            TextFit.Layout pushed = TextFit.Plan(input, (px, ls) => 80);
            Assert.IsFalse(pushed.KeepX);
            Assert.GreaterOrEqual(pushed.X, 28 - 0.01);
        }
    }

    public class LocRenderHookTests
    {
        [TearDown]
        public void ResetLanguage()
        {
            HomeChrome.RecordRuns = false;
            Loc.Reset(null);
        }

        [Test]
        public void Chain_EnglishFacesStayThree_GroupIsSeparate()
        {
            TrueTypeFont font = OutlineFonts.Primary("Fredoka", 700);
            TrueTypeFont[] chain = OutlineFonts.Chain(font);
            Assert.AreEqual(3, chain.Length);
            Assert.AreSame(font, chain[0]);
            Assert.AreSame(chain, OutlineFonts.Chain(font));
            Assert.AreSame(chain, OutlineFonts.Chain(font, null));
            Assert.AreSame(chain, OutlineFonts.Chain(font, ""));
            TrueTypeFont[] grouped = OutlineFonts.Chain(font, "cjk-sc");
            Assert.AreEqual(3, grouped.Length);
            Assert.AreNotSame(chain, grouped);
            Assert.AreSame(chain[0], grouped[0]);
            Assert.AreSame(chain[1], grouped[1]);
            Assert.AreSame(chain[2], grouped[2]);
        }

        [Test]
        public void WrapWords_MatchesLatinBreakerOnACanvas()
        {
            var canvas = new Canvas2D(32, 32);
            try
            {
                canvas.Font = "700 16px Fredoka";
                var method = typeof(HomeChrome).GetMethod("WrapWords", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.IsNotNull(method);
                string[] samples = { "PLAY MISSION", "Goal: Feed Nibbles 20 kg", "one two three four five" };
                foreach (string sample in samples)
                {
                    var legacy = new List<string>();
                    method.Invoke(null, new object[] { canvas, sample, 80.0, 8.0, legacy });
                    var live = new List<string>();
                    LineBreaker.WrapLatin(sample, 80, 8, canvas.MeasureText, live);
                    CollectionAssert.AreEqual(legacy, live, sample);
                }
            }
            finally
            {
                canvas.Dispose();
            }
        }

        [Test]
        public void LanguageSwitch_ChangesPaintedText_AndEnglishReturns()
        {
            var tables = new MemTables();
            tables.Add("en", "PLAY MISSION", "PLAY MISSION");
            tables.Add("de", "PLAY MISSION", "SPIEL STARTEN");
            Loc.Reset(tables);
            Assert.IsTrue(Loc.EnglishActive);
            var chrome = HomeChrome.Load("home_default", "zero");
            HomeChrome.RecordRuns = true;
            try
            {
                chrome.Draw();
                var english = Snapshot(chrome);
                Assert.That(english, Does.Contain("PLAY MISSION"));
                Assert.IsTrue(Loc.SetLanguage("de"));
                Assert.IsFalse(Loc.EnglishActive);
                chrome.Draw();
                var german = Snapshot(chrome);
                Assert.That(german, Does.Contain("SPIEL STARTEN"));
                Assert.IsFalse(german.Contains("PLAY MISSION"));
                Assert.IsTrue(Loc.SetLanguage("en"));
                chrome.Draw();
                Assert.AreEqual(english, Snapshot(chrome));
            }
            finally
            {
                chrome.Dispose();
            }
        }

        [Test]
        public void PseudoAudit_HasNoUnlistedHardViolations()
        {
            Loc.Reset(new ResourcesLocTableLoader());
            Assert.IsTrue(Loc.SetDiagnostic("qps"), "en.json did not load");
            string dir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            LocOverflowAudit.Report report = LocOverflowAudit.Run(dir);
            Directory.CreateDirectory("/tmp/maze-i18n/loc-render");
            File.WriteAllText("/tmp/maze-i18n/loc-render/overflow-qps.txt", report.Text);
            Debug.Log("[loc-audit] " + report.Text.Split('\n')[0]);
            Assert.AreEqual(0, report.HardWrapped, "a single-line control wrapped");
            // HARD is text outside the element, overlap with a sibling icon, past the home-indicator band,
            // or a second line inside a single-line control. hard-wider stays reported and does not fail.
            // An icon that already overlaps the English run is reserved only when the translation is wider.
            var whitelist = new HashSet<string>();
            for (int i = 0; i < report.Findings.Count; i++)
            {
                LocOverflowAudit.Finding f = report.Findings[i];
                if (f.Kind != "hard-outside" && f.Kind != "hard-overlap" && f.Kind != "hard-band" && f.Kind != "wrapped-in-control") continue;
                string key = f.File + "|" + f.ElementId + "|" + f.Kind;
                Assert.IsTrue(whitelist.Contains(key), "hard " + key + " " + f.Detail);
            }
        }

        static string Snapshot(HomeChrome chrome)
        {
            var parts = new List<string>();
            foreach (var run in chrome.RecordedRuns)
            {
                if (string.IsNullOrEmpty(run.Text)) continue;
                parts.Add(run.Text);
            }
            parts.Sort(StringComparer.Ordinal);
            return string.Join("\n", parts);
        }

        sealed class MemTables : ILocTableLoader
        {
            readonly Dictionary<string, string> files = new Dictionary<string, string>(StringComparer.Ordinal);

            public void Add(string code, string key, string value)
            {
                files[code] = "{\"meta\":{\"code\":\"" + code + "\",\"name\":\"" + code + "\",\"version\":1,\"source\":\"en\"},\"spec\":{\""
                    + key + "\":\"" + value + "\"},\"str\":{}}";
            }

            public bool TryLoad(string code, out string json)
            {
                return files.TryGetValue(code, out json);
            }
        }
    }
}
