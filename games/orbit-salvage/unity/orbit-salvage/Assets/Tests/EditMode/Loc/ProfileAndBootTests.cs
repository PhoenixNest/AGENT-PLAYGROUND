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
    public class ProfileAndBootTests : LocTestBase
    {
        [Test]
        public void UnsetLanguage_MatchesEconomyGolden()
        {
            string path = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "..", "tools", "parity", "golden", "fixtures", "economy.json"));
            Assert.IsTrue(File.Exists(path), path);
            JsonValue root = JsonParser.Parse(File.ReadAllText(path));
            string expected = null;
            JsonValue sequences = root["sequences"];
            for (int i = 0; i < sequences.Count && expected == null; i++)
            {
                JsonValue steps = sequences[i]["steps"];
                for (int s = 0; s < steps.Count; s++)
                {
                    if (steps[s]["op"].String == "getDefaultProfile")
                    {
                        expected = steps[s]["ret"].String;
                        break;
                    }
                }
            }
            Assert.IsNotNull(expected);
            string actual = Profile.CreateDefault().ToText();
            Assert.AreEqual(expected, actual);
            StringAssert.DoesNotContain("\"language\"", actual);

            Profile round = Profile.FromParsed(JsonParser.Parse(actual), "2099-01-01");
            Assert.AreEqual("", round.Settings.Language);
            Assert.AreEqual(actual, round.ToText());
        }

        [Test]
        public void Language_RoundTripsAndKeepsUnsupportedRawValues()
        {
            Profile profile = Profile.CreateDefault();
            profile.Settings.Language = "de";
            string text = profile.ToText();
            StringAssert.Contains("\"language\":\"de\"", text);
            Profile back = Profile.FromParsed(JsonParser.Parse(text), "2099-01-01");
            Assert.AreEqual("de", back.Settings.Language);
            Assert.AreEqual(text, back.ToText());

            profile.Settings.Language = "nb";
            Profile raw = Profile.FromParsed(JsonParser.Parse(profile.ToText()), "2099-01-01");
            Assert.AreEqual("nb", raw.Settings.Language);

            profile.Settings.Language = "";
            StringAssert.DoesNotContain("\"language\"", profile.ToText());
        }

        [Test]
        public void Reset_KeepsLanguage()
        {
            var store = new MemoryKeyValueStore();
            var eco = new EconomyManager(store, new FixedWallClock(0));
            Assert.IsTrue(eco.SetLanguage("de"));
            Assert.IsFalse(eco.SetLanguage("pt-BR"));
            Assert.AreEqual("de", eco.Profile.Settings.Language);
            eco.Profile.Credits = 999;
            Profile reset = eco.ResetProfile();
            Assert.AreEqual("de", reset.Settings.Language);
            Assert.AreEqual(250, reset.Credits, 0.001);
            StringAssert.Contains("\"language\":\"de\"", store.GetItem(EconomyManager.StorageKey));

            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            Assert.IsTrue(app.SetLanguage("fr"));
            app.Economy.Profile.Credits = 10;
            app.ResetSavedProgress();
            Assert.AreEqual("fr", app.Economy.Profile.Settings.Language);
            Assert.AreEqual(250, app.Economy.Profile.Credits, 0.001);
        }

        [Test]
        public void Boot_PersistsOnlyWhenANonEnglishTableLoads()
        {
            var idle = new CountingStore();
            var idleApp = new AppController(idle, new ManualAppClock(0, 0));
            int idleWrites = idle.Writes;
            idleApp.InitializeLanguage(new FixedDeviceLocale("English", "en-US"));
            Assert.AreEqual("en", Loc.CurrentLanguage);
            Assert.AreEqual("", idleApp.Economy.Profile.Settings.Language);
            Assert.AreEqual(idleWrites, idle.Writes);
            StringAssert.DoesNotContain("\"language\"", idleApp.Economy.Profile.ToText());

            var missing = new CountingStore();
            Loc.SetLoader(new MemoryTables());
            var german = new AppController(missing, new ManualAppClock(0, 0));
            int missingWrites = missing.Writes;
            german.InitializeLanguage(new FixedDeviceLocale("German", "de-DE"));
            Assert.AreEqual("en", Loc.CurrentLanguage);
            Assert.AreEqual("", german.Economy.Profile.Settings.Language);
            Assert.AreEqual(missingWrites, missing.Writes);

            german.Economy.Profile.Settings.Language = "de";
            german.InitializeLanguage(new FixedDeviceLocale("English", "en-US"));
            Assert.AreEqual("en", Loc.CurrentLanguage);
            Assert.AreEqual("de", german.Economy.Profile.Settings.Language);
            Assert.AreEqual(missingWrites, missing.Writes);

            german.Economy.Profile.Settings.Language = "zh-CN";
            german.InitializeLanguage(new FixedDeviceLocale("German", "de-DE"));
            Assert.AreEqual("zh-CN", german.Economy.Profile.Settings.Language);
            Assert.AreEqual("en", Loc.CurrentLanguage);
            Assert.AreEqual(missingWrites, missing.Writes);

            var tables = new MemoryTables();
            tables.Add("de", "", "");
            tables.Add("zh-Hant", "", "");
            Loc.SetLoader(tables);

            var detected = new CountingStore();
            var detectedApp = new AppController(detected, new ManualAppClock(0, 0));
            int before = detected.Writes;
            detectedApp.InitializeLanguage(new FixedDeviceLocale("German", "de-DE"));
            Assert.AreEqual("de", Loc.CurrentLanguage);
            Assert.AreEqual("de", detectedApp.Economy.Profile.Settings.Language);
            Assert.AreEqual(before + 1, detected.Writes);
            StringAssert.Contains("\"language\":\"de\"", detected.GetItem(EconomyManager.StorageKey));

            var canonical = new CountingStore();
            var canonicalApp = new AppController(canonical, new ManualAppClock(0, 0));
            canonicalApp.Economy.Profile.Settings.Language = "zh-HK";
            int canonicalWrites = canonical.Writes;
            canonicalApp.InitializeLanguage(new FixedDeviceLocale("English", "en-US"));
            Assert.AreEqual("zh-Hant", Loc.CurrentLanguage);
            Assert.AreEqual("zh-HK", canonicalApp.Economy.Profile.Settings.Language);
            Assert.AreEqual(canonicalWrites, canonical.Writes);

            var unsupported = new CountingStore();
            var unsupportedApp = new AppController(unsupported, new ManualAppClock(0, 0));
            unsupportedApp.Economy.Profile.Settings.Language = "nb";
            int unsupportedWrites = unsupported.Writes;
            unsupportedApp.InitializeLanguage(new FixedDeviceLocale("German", "de-DE"));
            Assert.AreEqual("en", Loc.CurrentLanguage);
            Assert.AreEqual("nb", unsupportedApp.Economy.Profile.Settings.Language);
            Assert.AreEqual(unsupportedWrites, unsupported.Writes);

            LocInitResult empty = Loc.InitializeFromProfile("", null);
            Assert.AreEqual("en", empty.ActiveLanguage);
            Assert.IsNull(empty.PersistLanguage);
        }

        sealed class CountingStore : IProfileStore
        {
            public int Writes;
            readonly MemoryProfileStore _inner = new MemoryProfileStore();

            public string GetItem(string key)
            {
                return _inner.GetItem(key);
            }

            public void SetItem(string key, string value)
            {
                Writes++;
                _inner.SetItem(key, value);
            }
        }
    }
}
