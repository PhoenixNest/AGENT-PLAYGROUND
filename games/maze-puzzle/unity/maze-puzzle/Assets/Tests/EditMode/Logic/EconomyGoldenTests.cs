using System;
using System.Collections.Generic;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic.Tests
{
    public class EconomyGoldenTests
    {
        private static JsonValue _doc;

        private static JsonValue Doc
        {
            get { return _doc ?? (_doc = Fixtures.Load("economy.json")); }
        }

        public static IEnumerable<string> SequenceNames()
        {
            if (!Fixtures.Exists("economy.json")) yield break;
            JsonValue seqs = Doc["sequences"];
            for (int i = 0; i < seqs.Count; i++) yield return seqs[i]["name"].String;
        }

        [Test]
        public void Catalog_ExactJsonText()
        {
            Assert.AreEqual(Doc["catalog"]["cosmetics"].String, JsonWriter.Stringify(EconomyCatalog.CatalogToJson()));
            Assert.AreEqual(Doc["catalog"]["achievements"].String, JsonWriter.Stringify(EconomyCatalog.AchievementsToJson()));
        }

        private static string ListText<T>(IEnumerable<T> items, Func<T, JsonValue> map)
        {
            JsonValue arr = JsonValue.NewArray();
            foreach (T i in items) arr.Add(map(i));
            return JsonWriter.Stringify(arr);
        }

        private static string Text(bool b)
        {
            return b ? "true" : "false";
        }

        private static double Num(JsonValue args, int i)
        {
            return args[i].Number;
        }

        private static string Str(JsonValue args, int i)
        {
            return args[i].String;
        }

        [Test]
        public void Sequence_ExactReturnsAndProfileJson([ValueSource(nameof(SequenceNames))] string name)
        {
            JsonValue seqs = Doc["sequences"];
            JsonValue seq = null;
            for (int i = 0; i < seqs.Count; i++)
            {
                if (seqs[i]["name"].String == name) seq = seqs[i];
            }
            Assert.IsNotNull(seq);

            var store = new MemoryKeyValueStore();
            var clock = new FixedWallClock(1790870401000.0);
            TimeZoneInfo la = DailyReset.FindLosAngeles();
            EconomyManager eco = null;
            JsonValue steps = seq["steps"];
            for (int s = 0; s < steps.Count; s++)
            {
                JsonValue step = steps[s];
                string op = step["op"].String;
                JsonValue args = step["args"];
                string ret = "undefined";
                switch (op)
                {
                    case "setTime":
                        clock.NowEpochMs = step["ms"].Number;
                        break;
                    case "construct":
                        eco = new EconomyManager(store, clock);
                        break;
                    case "seedStorage":
                        if (step["text"].Kind == JsonKind.Null) store.Items.Remove(EconomyManager.StorageKey);
                        else store.Items[EconomyManager.StorageKey] = step["text"].String;
                        break;
                    case "countdown":
                        ret = JsonWriter.Stringify(JsonValue.FromString(DailyReset.GetCountdownText(clock, la)));
                        break;
                    case "victoryPayout":
                    {
                        double total = EconomyManager.CampaignPayout(Num(args, 0), Num(args, 1), Num(args, 2));
                        eco.AddCredits(total);
                        ret = JsNumber.ToString(total);
                        break;
                    }
                    case "getDefaultProfile":
                        ret = eco.GetDefaultProfile().ToText();
                        break;
                    case "resetProfile":
                        ret = eco.ResetProfile().ToText();
                        break;
                    case "normalizeCategory":
                    {
                        CategoryNames n = eco.NormalizeCategory(Str(args, 0));
                        JsonValue o = JsonValue.NewObject();
                        o.Set("singular", n.Singular);
                        o.Set("plural", n.Plural);
                        ret = JsonWriter.Stringify(o);
                        break;
                    }
                    case "canClaimDailyGift":
                        ret = Text(eco.CanClaimDailyGift());
                        break;
                    case "claimDailyGift":
                        ret = Text(args.Count > 0 ? eco.ClaimDailyGift(Num(args, 0)) : eco.ClaimDailyGift());
                        break;
                    case "recordSectorCompleted":
                        eco.RecordSectorCompleted(Str(args, 0));
                        break;
                    case "addCredits":
                        ret = JsNumber.ToString(eco.AddCredits(Num(args, 0)));
                        break;
                    case "spendCredits":
                        ret = Text(eco.SpendCredits(Num(args, 0)));
                        break;
                    case "isUnlocked":
                        ret = Text(eco.IsUnlocked(Str(args, 0), Str(args, 1)));
                        break;
                    case "unlockItem":
                        ret = Text(eco.UnlockItem(Str(args, 0), Str(args, 1), Num(args, 2)));
                        break;
                    case "equipItem":
                        ret = Text(eco.EquipItem(Str(args, 0), Str(args, 1)));
                        break;
                    case "recordDebrisSwallowed":
                        ret = ListText(eco.RecordDebrisSwallowed(args.Count > 0 ? Num(args, 0) : 1), a => a.ToJson());
                        break;
                    case "recordContractComplete":
                        if (args.Count == 0) ret = ListText(eco.RecordContractComplete(), a => a.ToJson());
                        else if (args.Count == 1) ret = ListText(eco.RecordContractComplete(args[0].Bool), a => a.ToJson());
                        else ret = ListText(eco.RecordContractComplete(args[0].Bool, Str(args, 1)), a => a.ToJson());
                        break;
                    case "canClaimDailyMasterBonus":
                        ret = Text(eco.CanClaimDailyMasterBonus());
                        break;
                    case "isDailyMasterClaimed":
                        ret = Text(eco.IsDailyMasterClaimed());
                        break;
                    case "claimDailyMasterBonus":
                        ret = Text(args.Count > 0 ? eco.ClaimDailyMasterBonus(Num(args, 0)) : eco.ClaimDailyMasterBonus());
                        break;
                    case "checkAchievements":
                        ret = ListText(eco.CheckAchievements(), a => a.ToJson());
                        break;
                    case "getDailyShifts":
                        ret = ListText(eco.GetDailyShifts(), a => a.ToJson());
                        break;
                    case "getDailyShiftInfo":
                        ret = JsonWriter.Stringify(eco.GetDailyShiftInfo().ToJson());
                        break;
                    default:
                        Assert.Fail("unknown op " + op);
                        break;
                }
                string where = name + " step " + s + " (" + op + ")";
                Assert.AreEqual(step["ret"].String, ret, where + " return value");
                string stored = store.GetItem(EconomyManager.StorageKey);
                if (step["stored"].Kind == JsonKind.Null) Assert.IsNull(stored, where + " stored profile");
                else Assert.AreEqual(step["stored"].String, stored, where + " stored profile JSON text");
            }
        }

        [Test]
        public void DailyReset_UsesLosAngelesMidnight()
        {
            var clock = new FixedWallClock(1790870401000.0);
            Assert.AreEqual("Resets in 14h 59m", DailyReset.GetCountdownText(clock, DailyReset.FindLosAngeles()));
        }
    }
}
