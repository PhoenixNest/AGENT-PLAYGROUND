using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
using UnityEngine;

namespace OrbitalSalvage.Loc.Tests
{
    public class LocStringTests : LocTestBase
    {
        [Test]
        public void English_ComesFromTheRegistryAndDoesNotLoadATable()
        {
            int loads = 0;
            Loc.SetLoader(new CountingLoader(delegate { loads++; return false; }));
            var defs = LocStrings.All;
            for (int i = 0; i < defs.Length; i++)
            {
                LocStringDef def = defs[i];
                if (LocText.PlaceholderIndexes(def.English).Count > 0) continue;
                Assert.AreEqual(def.English, Loc.Tr(def.Key), def.Key);
            }
            Assert.AreEqual(0, loads);
            Assert.AreEqual("missing-key", Loc.Tr("missing-key"));
        }

        [Test]
        public void English_FormattedSitesMatchTheOldLiterals()
        {
            Eq("Goal: " + JsNumber.ToString(20d) + " kg", Loc.Tr("home.mission.goal", 20d));
            Eq("World " + JsNumber.ToString(4d), Loc.Tr("home.mission.world_fallback", 4d));
            Eq("+" + JsNumber.ToString(350d) + " \U0001FA99 Bounty", Loc.Tr("home.daily.bounty", 350d));
            Eq("LVL 3 \u2022 Moon Chute 3", Loc.Tr("home.level.title", "LVL 3", "Moon Chute 3"));
            Eq("\u2B50 LVL 3", Loc.Tr("hud.sector_code", "LVL 3"));
            Eq(JsNumber.ToString(12.5d) + " kg", Loc.Tr("unit.kg", 12.5d));
            Eq(JsNumber.ToString(12d) + " / " + JsNumber.ToString(20d) + " kg", Loc.Tr("unit.quota", 12d, 20d));
            Eq("+" + JsNumber.ToString(40d) + " \U0001FA99", Loc.Tr("unit.plus_coins", 40d));
            Eq("3 NEW", Loc.Tr("dock.daily.new", 3));
            string quota = JsNumber.ToString(20d);
            string eaten = JsNumber.ToString(4d);
            Eq("Goal: Feed Nibbles " + quota + " kg (" + eaten + "/" + quota + " kg eaten)",
                Loc.Tr("pause.quota", 20d, 4d));
            Eq("Unlock (" + JsNumber.ToString(500d) + " \U0001FA99)", Loc.Tr("shop.footer.unlock", 500d));
            Eq("\U0001F3C6 Trophy: First Nom Nom! (+" + JsNumber.ToString(100d) + " \U0001FA99)",
                Loc.Tr("toast.trophy", "First Nom Nom!", 100d));
            Eq("Clean up Bubblegum Chute 1! \U0001F680", Loc.Tr("toast.sector.cleanup", "Bubblegum Chute 1"));
            Eq("Resets in " + JsNumber.ToString(14d) + "h " + JsNumber.ToString(59d) + "m",
                Loc.Tr("daily.countdown", 14d, 59d));
            Eq("\u26A1 \u00D7" + JsNumber.ToString(2d), Loc.Tr("fx.gate.zap", 2d));
            Eq("\U0001F4A5 \u00D7" + JsNumber.ToString(3d) + "!", Loc.Tr("fx.gate.shred", 3d));
            Eq("\U0001F9F2 +" + JsNumber.ToString(5d), Loc.Tr("fx.gate.magnet", 5d));
            Eq("\u00D7" + JsNumber.ToString(5d) + "! SHRED \u26A1", Loc.Tr("fx.shred", 5d));
            Eq("+" + JsNumber.ToString(7d) + "! COMPACT \u2728", Loc.Tr("fx.compact", 7d));
            Eq("Wear", Loc.Tr("shop.footer.wear"));
            Eq("EQUIPPED \u2728", Loc.Tr("shop.footer.equipped"));
            Eq("Free Starter", Loc.Tr("shop.footer.free"));
            Eq("Janitor Care Package!", Loc.Tr("gift.title.ready"));
            Eq("Claim +100 \U0001FA99", Loc.Tr("gift.button.claim"));
            Eq("Sound Off \U0001F507", Loc.Tr("toast.sound.off"));
            Eq("Sound On \U0001F50A", Loc.Tr("toast.sound.on"));
            Eq("\u23E9 BOOST", Loc.Tr("fx.gate.boost"));
            Eq("NOM! \U0001F60B", Loc.Tr("fx.swallow.nom"));
            Eq("POOF! \U0001F4A8", Loc.Tr("fx.poof"));
            Eq("TURBO! \u23E9", Loc.Tr("fx.turbo.cw"));
            Eq("\u23EA TURBO!", Loc.Tr("fx.turbo.ccw"));
            Eq("Progress Reset to Day 1! \U0001F9F9", Loc.Tr("toast.progress.reset"));
            Eq("\U0001F381 Claimed today. Come back tomorrow!", Loc.Tr("toast.gift.already"));
        }

        [Test]
        public void MissingTranslation_FallsBackToTheRegistry()
        {
            var tables = new MemoryTables();
            tables.Add("de", "", "\"shop.footer.wear\":\"Tragen\"");
            Loc.SetLoader(tables);
            Assert.IsTrue(Loc.SetLanguage("de"));
            Assert.AreEqual("Tragen", Loc.Tr("shop.footer.wear"));
            Assert.AreEqual("FREE", Loc.Tr("dock.shop.free"));
            CollectionAssert.Contains(Loc.MissingKeys, "dock.shop.free");
        }

        [Test]
        public void EveryRegistryKeyIsUsedAndEveryCallExists()
        {
            string scripts = Path.Combine(Application.dataPath, "Scripts");
            var literals = new HashSet<string>(StringComparer.Ordinal);
            var calls = new List<string>();
            foreach (string file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
            {
                string rel = file.Substring(scripts.Length + 1).Replace('\\', '/');
                if (rel.StartsWith("Editor/", StringComparison.Ordinal)) continue;
                if (rel.StartsWith("Parity/", StringComparison.Ordinal)) continue;
                if (rel.IndexOf("/Tests/", StringComparison.Ordinal) >= 0) continue;
                if (rel.EndsWith("LocStrings.cs", StringComparison.Ordinal)) continue;
                string stripped = CommentStrip(File.ReadAllText(file));
                CollectLiterals(stripped, literals);
                int from = 0;
                while (from < stripped.Length)
                {
                    int at = stripped.IndexOf("Loc.Tr(", from, StringComparison.Ordinal);
                    if (at < 0) break;
                    int q = at + "Loc.Tr(".Length;
                    while (q < stripped.Length && char.IsWhiteSpace(stripped[q])) q++;
                    if (q < stripped.Length && stripped[q] == '"')
                    {
                        int end = stripped.IndexOf('"', q + 1);
                        if (end > q) calls.Add(stripped.Substring(q + 1, end - q - 1));
                    }
                    from = q + 1;
                }
            }

            var known = new HashSet<string>(StringComparer.Ordinal);
            var defs = LocStrings.All;
            for (int i = 0; i < defs.Length; i++) known.Add(defs[i].Key);
            for (int i = 0; i < calls.Count; i++)
                Assert.IsTrue(known.Contains(calls[i]), "Loc.Tr key missing from the registry: " + calls[i]);
            var missing = new List<string>();
            for (int i = 0; i < defs.Length; i++)
            {
                if (defs[i].Dynamic) continue;
                if (!literals.Contains(defs[i].Key)) missing.Add(defs[i].Key);
            }
            Assert.IsEmpty(missing, string.Join(", ", missing.ToArray()));
        }

        [Test]
        public void PseudoLocale_ChangesAppOverridesAndKeepsNumbers()
        {
            var tables = new MemoryTables();
            tables.Files["en"] = LocJson.WriteTable(
                "en",
                "English",
                new string[0],
                delegate (string key) { return key; },
                LocStrings.SortedPairs(),
                new string[0]);
            Loc.SetLoader(tables);
            Assert.IsTrue(Loc.SetDiagnostic(PseudoLocale.Code));
            Assert.IsFalse(Loc.EnglishActive);

            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 1790870401000.0), new SilentAudio(), () => 0);
            Assert.AreNotEqual("Calibrating cosmic centrifuges...", app.LoadingTip);
            Assert.IsTrue(app.LoadingTip.StartsWith("[", StringComparison.Ordinal), app.LoadingTip);

            app.LoadSector(app.Sectors[0], false);
            app.SetLiveMass(4, 1);
            string goal = app.Model.Elements["home-mission-goal"].Text;
            Assert.AreNotEqual("Goal: 20 kg", goal, goal);
            Assert.IsTrue(goal.Contains("20"), goal);
            string quota = app.Model.Elements["hud-quota-text"].Text;
            Assert.AreNotEqual("4 / 20 kg", quota, quota);
            Assert.IsTrue(quota.Contains("4"), quota);
            Assert.IsTrue(quota.Contains("20"), quota);

            app.ShowPage("game");
            app.Click("btn-game-pause");
            string pause = app.Model.Elements["pause-level-quota"].Text;
            Assert.AreNotEqual("Goal: Feed Nibbles 20 kg (4/20 kg eaten)", pause, pause);
            Assert.IsTrue(pause.Contains("20"), pause);
            Assert.IsTrue(pause.Contains("4"), pause);

            app.ShowPage("shop");
            Assert.AreNotEqual("EQUIPPED \u2728", app.ShopFooters[0], app.ShopFooters[0]);
            Assert.IsTrue(app.ShopFooters[1].Contains("500"), app.ShopFooters[1]);
            Assert.AreNotEqual("Unlock (500 \U0001FA99)", app.ShopFooters[1], app.ShopFooters[1]);
            string claim = app.Model.Elements["btn-claim-daily-gift"].Text;
            Assert.AreNotEqual("Claim +100 \U0001FA99", claim, claim);
            Assert.IsTrue(claim.Contains("100"), claim);

            app.ForceVictory(15);
            Assert.AreNotEqual("15 kg", app.DebriefMass, app.DebriefMass);
            Assert.IsTrue(app.DebriefMass.Contains("15"), app.DebriefMass);
            Assert.AreNotEqual("+250 \U0001FA99", app.DebriefCredits, app.DebriefCredits);
            Assert.IsTrue(app.DebriefCredits.Contains("250"), app.DebriefCredits);

            app.ResetSavedProgress();
            Assert.Greater(app.ToastTexts().Count, 0);
            Assert.AreNotEqual("Progress Reset to Day 1! \U0001F9F9", app.ToastTexts()[app.ToastTexts().Count - 1]);

            var clock = new FixedWallClock(1790870401000.0);
            string countdown = DailyReset.GetCountdownText(clock, DailyReset.FindLosAngeles());
            Assert.AreNotEqual("Resets in 14h 59m", countdown, countdown);
            Assert.IsTrue(countdown.Contains("14"), countdown);
            Assert.IsTrue(countdown.Contains("59"), countdown);
        }

        static void Eq(string oldLiteral, string actual)
        {
            Assert.AreEqual(oldLiteral, actual);
        }

        static void CollectLiterals(string text, HashSet<string> literals)
        {
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] != '"') { i++; continue; }
                int end = i + 1;
                var sb = new StringBuilder();
                while (end < text.Length)
                {
                    char c = text[end];
                    if (c == '\\' && end + 1 < text.Length)
                    {
                        sb.Append(text[end]);
                        sb.Append(text[end + 1]);
                        end += 2;
                        continue;
                    }
                    if (c == '"') break;
                    sb.Append(c);
                    end++;
                }
                literals.Add(sb.ToString());
                i = end + 1;
            }
        }

        static string CommentStrip(string text)
        {
            var sb = new StringBuilder(text.Length);
            int i = 0;
            while (i < text.Length)
            {
                char c = text[i];
                if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
                {
                    while (i < text.Length && text[i] != '\n') i++;
                    continue;
                }
                if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/')) i++;
                    i = Math.Min(text.Length, i + 2);
                    sb.Append(' ');
                    continue;
                }
                if (c == '@' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    sb.Append(text[i]);
                    sb.Append(text[i + 1]);
                    i += 2;
                    while (i < text.Length)
                    {
                        if (text[i] == '"')
                        {
                            sb.Append('"');
                            i++;
                            if (i < text.Length && text[i] == '"') { sb.Append('"'); i++; continue; }
                            break;
                        }
                        sb.Append(text[i]);
                        i++;
                    }
                    continue;
                }
                if (c == '"')
                {
                    sb.Append('"');
                    i++;
                    while (i < text.Length)
                    {
                        sb.Append(text[i]);
                        if (text[i] == '\\' && i + 1 < text.Length)
                        {
                            i++;
                            sb.Append(text[i]);
                            i++;
                            continue;
                        }
                        if (text[i] == '"') { i++; break; }
                        i++;
                    }
                    continue;
                }
                sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        sealed class CountingLoader : ILocTableLoader
        {
            readonly Func<bool> _load;

            public CountingLoader(Func<bool> load)
            {
                _load = load;
            }

            public bool TryLoad(string code, out string json)
            {
                json = null;
                return _load();
            }
        }
    }
}
