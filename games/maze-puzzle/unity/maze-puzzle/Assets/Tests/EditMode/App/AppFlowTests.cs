using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.App.Tests
{
    public class AppFlowTests
    {
        public static IEnumerable<string> FlowIds()
        {
            string dir = GoldenDir();
            if (!Directory.Exists(dir)) yield break;
            foreach (string file in Directory.GetFiles(dir, "*.json"))
                yield return Path.GetFileNameWithoutExtension(file);
        }

        [TestCaseSource(nameof(FlowIds))]
        public void Flow_MatchesWeb(string id)
        {
            string path = Path.Combine(GoldenDir(), id + ".json");
            Assert.IsTrue(File.Exists(path), "missing golden " + path);
            JsonValue root = JsonParser.Parse(File.ReadAllText(path));
            JsonValue trace = root.Get("trace");
            Assert.IsNotNull(trace, id);
            Assert.Greater(trace.Count, 0, id);

            JsonValue first = trace[0].Get("snap");
            double epoch0 = first.Get("epochMs").Number - first.Get("nowMs").Number;
            var clock = new ManualAppClock(0, epoch0);
            AppController app = null;
            PhysicsEngine physics = null;
            int syncedEpoch = -1;
            bool physicsDiverged = false;
            var sectors = MazeLevelBuilder.GetSectors();

            for (int i = 0; i < trace.Count; i++)
            {
                JsonValue step = trace[i];
                string op = step.Get("op").String;
                if (op == "boot" || op == "bootHome")
                {
                    physics = AlignBootRandom();
                    clock = new ManualAppClock(0, epoch0);
                    app = new AppController(new MemoryProfileStore(), clock);
                    app.AdoptSector(sectors[0]);
                    app.BackupDeployed += () => physics.SpawnBackupScrap(8);
                    syncedEpoch = app.SectorEpoch;
                    app.BeginBoot();
                    if (op == "bootHome")
                    {
                        int guard = 0;
                        while (app.CurrentPage != "home" && guard++ < 2000) app.Advance(16);
                        app.Advance(240);
                    }
                }
                else if (op == "advance")
                {
                    app.Advance(step.Get("ms").Number);
                    if (app.CurrentPage == "game") physicsDiverged = true;
                }
                else if (op == "click")
                {
                    JsonValue idNode = step.Get("id");
                    if (idNode != null && idNode.Kind == JsonKind.String) app.Click(idNode.String);
                    else app.ClickClass(step.Get("token").String, (int)step.Get("index").Number);
                }
                else if (op == "forceWin") app.ForceVictory(step.Get("collectedMass").Number);
                else if (op == "forceLose") app.ForceDefeat(step.Get("collectedMass").Number, step.Get("spilledMass").Number);
                else if (op == "key") app.KeyDown(step.Get("key").String);
                else if (op == "settings")
                {
                    app.SetSoundEnabled(step.Get("sound").Bool);
                    app.SetHaptics(step.Get("haptics").Bool);
                    app.SetSlider("settings-sfx-slider", (int)step.Get("sfx").Number);
                    app.SetSlider("settings-ambience-slider", (int)step.Get("ambience").Number);
                }
                else Assert.Fail(id + " unknown op " + op);

                if (physics != null && app.SectorEpoch != syncedEpoch && app.ActiveSector != null)
                {
                    syncedEpoch = app.SectorEpoch;
                    var hole = physics.BlackHole;
                    physics.InitSector(app.ActiveSector, physics.Center, new Vec2(hole.X, hole.Y));
                }
                Compare(id, i, op, app, step.Get("snap"), physicsDiverged);
            }
        }

        static PhysicsEngine AlignBootRandom()
        {
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var renderer = new GameRenderer(430, 932, 3);
            renderer.Dispose();
            var physics = new PhysicsEngine(new ParityAudioCues(() => 0));
            var sectors = MazeLevelBuilder.GetSectors();
            var bh = physics.BlackHole;
            physics.InitSector(sectors[0], physics.Center, new Vec2(bh.X, bh.Y));
            return physics;
        }

        static void Compare(string flow, int index, string op, AppController app, JsonValue snap, bool physicsDiverged)
        {
            string at = flow + " step " + index + " " + op;
            Eq(at, "page", snap.Get("currentPage").String, app.CurrentPage);
            Eq(at, "credits", Str(snap, "credits"), app.CreditText);
            Eq(at, "quota", Str(snap, "quota"), app.QuotaText);
            Eq(at, "loadingBar", Str(snap, "loadingBar"), app.LoadingBar);
            Eq(at, "loadingTip", Str(snap, "loadingTip"), app.LoadingTip);
            Eq(at, "storeTab", Str(snap, "storeTab"), app.StoreTab);
            Eq(at, "sectorId", Str(snap, "sectorId"), app.ActiveSector != null ? app.ActiveSector.Id : null);
            Eq(at, "sectorCode", Str(snap, "sectorCode"), app.ActiveSector != null ? app.ActiveSector.Code : null);
            Assert.AreEqual((int)snap.Get("worldIndex").Number, app.WorldIndex, at + " world");
            Assert.AreEqual((int)snap.Get("sectorIndex").Number, app.SectorIndex, at + " sectorIndex");
            Assert.AreEqual(snap.Get("paused").Bool, app.IsPaused, at + " paused");
            Assert.AreEqual(snap.Get("cleared").Bool, app.IsLevelCleared, at + " cleared");
            Assert.AreEqual(snap.Get("failed").Bool, app.IsLevelFailed, at + " failed");
            Assert.AreEqual(snap.Get("usedBackup").Bool, app.UsedBackupPod, at + " backupFlag");
            Assert.AreEqual(snap.Get("isDaily").Bool, app.ActiveSector != null && app.ActiveSector.IsDaily == true, at + " daily");
            string tier = app.ActiveSector != null ? app.ActiveSector.DailyTier : null;
            if (string.IsNullOrEmpty(tier)) tier = null;
            Eq(at, "dailyTier", Str(snap, "dailyTier"), tier);
            Near(at, "nowMs", snap.Get("nowMs").Number, app.NowMs);
            if (!physicsDiverged)
                Assert.AreEqual((long)snap.Get("randomCalls").Number, JsRandom.Global.CallCount, at + " randomCalls");
            ListEq(at, "modals", snap.Get("modals"), app.OpenModals());
            ListEq(at, "toasts", snap.Get("toasts"), app.ToastTexts());
            BoolListEq(at, "dailyDone", snap.Get("dailyDone"), app.DailyDone);
            Eq(at, "dailyBadge", Str(snap, "dailyBadge"), app.DailyBadge);
            Eq(at, "shopBadge", Str(snap, "shopBadge"), app.ShopBadge);
            Eq(at, "profile", Str(snap, "profile"), app.Economy.Profile.ToText());

            if (app.CurrentPage == "home")
            {
                Eq(at, "missionName", Str(snap, "missionName"), app.MissionName);
                Eq(at, "missionGoal", Str(snap, "missionGoal"), app.MissionGoal);
                Eq(at, "missionWorld", Str(snap, "missionWorld"), app.MissionWorld);
                if (Str(snap, "dailyBounty") != null) Eq(at, "dailyBounty", Str(snap, "dailyBounty"), app.DailyBounty);
                Eq(at, "soundIcon", Str(snap, "soundIcon"), app.SoundIcon);
            }
            if (app.CurrentPage == "shop")
            {
                ListEq(at, "shopFooters", snap.Get("shopFooters"), app.ShopFooters);
                Eq(at, "giftBtn", Str(snap, "giftBtn"), app.GiftButton);
                Eq(at, "giftTitle", Str(snap, "giftTitle"), app.GiftTitle);
            }
            if (app.CurrentPage == "maps")
            {
                Eq(at, "worldTitle", Str(snap, "worldTitle"), app.WorldTitle);
                Eq(at, "worldProgress", Str(snap, "worldProgress"), app.WorldProgress);
                Eq(at, "firstLevelCode", Str(snap, "firstLevelCode"), app.FirstLevelCode);
            }
            if (app.CurrentPage == "daily")
                Eq(at, "masterAction", Str(snap, "masterAction"), app.MasterAction);
            var modals = app.OpenModals();
            if (Has(modals, "modal-pause")) Eq(at, "pauseName", Str(snap, "pauseName"), app.PauseName);
            if (Has(modals, "modal-debrief"))
            {
                Eq(at, "debriefMass", Str(snap, "debriefMass"), app.DebriefMass);
                Eq(at, "debriefCredits", Str(snap, "debriefCredits"), app.DebriefCredits);
            }
            if (Has(modals, "modal-defeat"))
            {
                Eq(at, "defeatMass", Str(snap, "defeatMass"), app.DefeatMass);
                Eq(at, "defeatSpilled", Str(snap, "defeatSpilled"), app.DefeatSpilled);
                Assert.AreEqual(snap.Get("backupVisible").Bool, app.BackupVisible, at + " backupVisible");
            }
        }

        static bool Has(IReadOnlyList<string> items, string value)
        {
            for (int i = 0; i < items.Count; i++) if (items[i] == value) return true;
            return false;
        }

        static void Eq(string at, string field, string expected, string actual)
        {
            Assert.AreEqual(expected ?? "", actual ?? "", at + " " + field);
        }

        static void Near(string at, string field, double expected, double actual)
        {
            Assert.Less(Math.Abs(expected - actual), 0.01, at + " " + field + " expected " + expected + " actual " + actual);
        }

        static string Str(JsonValue snap, string key)
        {
            JsonValue v = snap.Get(key);
            if (v == null || v.Kind == JsonKind.Null) return null;
            return v.String;
        }

        static void ListEq(string at, string field, JsonValue expected, IReadOnlyList<string> actual)
        {
            if (expected == null || expected.Kind == JsonKind.Null)
            {
                Assert.AreEqual(0, actual.Count, at + " " + field);
                return;
            }
            Assert.AreEqual(expected.Count, actual.Count, at + " " + field + " count");
            for (int i = 0; i < expected.Count; i++)
                Assert.AreEqual(expected[i].String, actual[i], at + " " + field + "[" + i + "]");
        }

        static void BoolListEq(string at, string field, JsonValue expected, IReadOnlyList<bool> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, at + " " + field);
            for (int i = 0; i < expected.Count; i++)
                Assert.AreEqual(expected[i].Bool, actual[i], at + " " + field + "[" + i + "]");
        }

        static string GoldenDir()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "..", "..", "..", "tools", "parity", "golden", "app"));
        }
    }
}
