using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.App.Tests
{
    public class TemplateSelectorTests
    {
        static TemplateQuery Q(string page) => new TemplateQuery { Page = page, Modal = "", ShopTab = "rings", ShopSignature = TemplateSelector.RingsDefault, SlidersDefault = true, SoundOn = true, HapticsOn = true, WorldIndex = 1, ActiveWorld = 1, WorldTotal = 10 };

        [Test]
        public void Select_MapsWorldsAndCompletion()
        {
            var fresh = Q("maps");
            Assert.AreEqual("maps_world_1_fresh", TemplateSelector.Select(fresh, out _));
            var w2 = fresh; w2.WorldIndex = 2;
            Assert.AreEqual("maps_world_2", TemplateSelector.Select(w2, out _));
            var w5 = fresh; w5.WorldIndex = 5;
            Assert.AreEqual("maps_world_5", TemplateSelector.Select(w5, out _));
            var done = fresh; done.WorldDone = 10;
            Assert.AreEqual("maps_world_1_all_complete", TemplateSelector.Select(done, out _));
            var other = done; other.ActiveWorld = 2;
            Assert.AreEqual("maps_world_1_all_complete_other_active", TemplateSelector.Select(other, out _));
            var none = fresh; none.WorldIndex = 3; none.WorldDone = 0;
            Assert.AreEqual("maps_world_3_none_complete", TemplateSelector.Select(none, out _));
            var prefix = fresh; prefix.WorldIndex = 5; prefix.Levels1Through44 = true;
            Assert.AreEqual("maps_world_5_levels_1_44_done", TemplateSelector.Select(prefix, out _));
        }

        [Test]
        public void Select_ShopDailySettingsAndModals()
        {
            var shop = Q("shop");
            Assert.AreEqual("shop_rings_default", TemplateSelector.Select(shop, out var stamp));
            Assert.IsNull(stamp);
            shop.ShopSignature = TemplateSelector.RingsMixed;
            Assert.AreEqual("shop_rings_mixed", TemplateSelector.Select(shop, out stamp));
            Assert.IsNull(stamp);
            shop.GiftClaimed = true;
            shop.ShopSignature = TemplateSelector.RingsDefault;
            Assert.AreEqual("shop_gift_claimed", TemplateSelector.Select(shop, out _));
            shop.ShopSignature = "ring_donut,ring_spacecat|ring_spacecat";
            Assert.AreEqual("shop_gift_claimed", TemplateSelector.Select(shop, out stamp));
            Assert.AreEqual("ring_donut:owned,ring_duckie:locked,ring_candyland:locked,ring_spacecat:equipped", stamp);

            var sings = Q("shop"); sings.ShopTab = "singularities"; sings.ShopSignature = TemplateSelector.SingsMixed;
            Assert.AreEqual("shop_singularities_mixed", TemplateSelector.Select(sings, out _));
            var sky = Q("shop"); sky.ShopTab = "skyboxes"; sky.ShopSignature = TemplateSelector.SkyMixed;
            Assert.AreEqual("shop_skyboxes_mixed", TemplateSelector.Select(sky, out _));

            var daily = Q("daily");
            Assert.AreEqual("daily_default", TemplateSelector.Select(daily, out _));
            daily.DailyCleared = 1;
            Assert.AreEqual("daily_bronze_cleared", TemplateSelector.Select(daily, out _));
            daily.DailyCleared = 2;
            Assert.AreEqual("daily_two_cleared", TemplateSelector.Select(daily, out _));
            daily.MasterClaimed = true;
            Assert.AreEqual("daily_master_claimed", TemplateSelector.Select(daily, out _));

            var settings = Q("settings");
            Assert.AreEqual("settings_default", TemplateSelector.Select(settings, out _));
            settings.SoundOn = false; settings.HapticsOn = false;
            Assert.AreEqual("settings_toggles_off", TemplateSelector.Select(settings, out _));
            settings.SlidersDefault = false;
            Assert.AreEqual("settings_sliders_moved", TemplateSelector.Select(settings, out _));
            settings.Modal = "privacy";
            Assert.AreEqual("settings_modal_privacy", TemplateSelector.Select(settings, out _));
            settings.Modal = "reset";
            Assert.AreEqual("settings_modal_reset_confirm", TemplateSelector.Select(settings, out _));

            var debrief = Q("game"); debrief.Modal = "debrief";
            Assert.AreEqual("modal_debrief_exact", TemplateSelector.Select(debrief, out _));
            debrief.DebriefSurplus = true;
            Assert.AreEqual("modal_debrief_surplus", TemplateSelector.Select(debrief, out _));
            debrief.WorldIndex = 3;
            Assert.AreEqual("modal_debrief_world3", TemplateSelector.Select(debrief, out _));
            debrief.WorldIndex = 5;
            Assert.AreEqual("modal_debrief_world5", TemplateSelector.Select(debrief, out _));
            debrief.DebriefDaily = true;
            Assert.AreEqual("modal_debrief_daily_trophy", TemplateSelector.Select(debrief, out _));

            var defeat = Q("game"); defeat.Modal = "defeat"; defeat.DefeatRatio = 0.6;
            Assert.AreEqual("modal_defeat_12_of_20", TemplateSelector.Select(defeat, out _));
            defeat.DefeatRatio = 0.75;
            Assert.AreEqual("modal_defeat_backup", TemplateSelector.Select(defeat, out _));
            defeat.DefeatRatio = 0.25;
            Assert.AreEqual("modal_defeat_no_backup_low", TemplateSelector.Select(defeat, out _));
            defeat.DefeatUsed = true;
            Assert.AreEqual("modal_defeat_no_backup_used", TemplateSelector.Select(defeat, out _));

            var trophies = Q("achievements");
            Assert.AreEqual("achievements_none", TemplateSelector.Select(trophies, out _));
            trophies.AchievementsUnlocked = 2; trophies.AchievementsTotal = 5;
            Assert.AreEqual("achievements_some", TemplateSelector.Select(trophies, out _));
            trophies.AchievementsUnlocked = 5;
            Assert.AreEqual("achievements_all", TemplateSelector.Select(trophies, out _));

            var game = Q("game"); game.GameDaily = true;
            Assert.AreEqual("game_daily_bronze_t1000", TemplateSelector.Select(game, out _));
        }

        [Test]
        public void CardPositions_MatchMeasuredSpecsWithinHalfPixel()
        {
            ExpectStack("shop_rings_mixed", "store-card", CardInstancer.StackShop);
            ExpectStack("shop_rings_default", "store-card", CardInstancer.StackShop);
            ExpectStack("shop_gift_claimed", "store-card", CardInstancer.StackShop);
            ExpectStack("achievements_some", "ach-card", CardInstancer.StackTrophies);
            ExpectStack("achievements_all", "ach-card", CardInstancer.StackTrophies);
            ExpectLevels("maps_world_2");
            ExpectLevels("maps_world_5");
            ExpectLevels("maps_world_1_all_complete");
            ExpectLevels("maps_world_5_levels_1_44_done");
            ExpectDaily("daily_bronze_cleared");
            ExpectDaily("daily_two_cleared");
            ExpectDaily("daily_master_claimed");
        }

        [Test]
        public void CardSubtree_PlacesBackOntoItsMeasuredSlot()
        {
            var elements = Load("shop_rings_default");
            int root = CardStamp.CardRoots(elements)[0];
            double[] origin = CardStamp.Rect(elements[root]);
            var slot = new CardRect(origin[0], origin[1], origin[2], origin[3]);
            for (int i = 0; i < elements.Count; i++)
            {
                if (CardStamp.ParentOf(elements[i]) != root) continue;
                double[] r = CardStamp.Rect(elements[i]);
                var placed = CardInstancer.Place(slot, origin[0], origin[1], r[0], r[1], r[2], r[3]);
                Assert.That(Math.Abs(placed.X - r[0]), Is.LessThan(0.5), "child x");
                Assert.That(Math.Abs(placed.Y - r[1]), Is.LessThan(0.5), "child y");
            }
        }

        [Test]
        public void Stamp_StacksKindTemplatesOnTheShopGrid()
        {
            var templates = new Dictionary<string, List<Dictionary<string, object>>>
            {
                ["equipped"] = CardStamp.Extract(Load("shop_rings_default"), "equipped"),
                ["locked"] = CardStamp.Extract(Load("shop_rings_default"), "locked"),
                ["owned"] = CardStamp.Extract(Load("shop_rings_mixed"), "owned")
            };
            const string kinds = "owned,owned,equipped,locked";
            var heights = CardStamp.Heights(kinds, templates);
            var slots = CardInstancer.StackShop(heights);
            var elements = Load("shop_rings_default");
            var xs = new double[slots.Length];
            var ys = new double[slots.Length];
            for (int i = 0; i < slots.Length; i++) { xs[i] = slots[i].X; ys[i] = slots[i].Y; }
            CardStamp.ApplyShop(elements, kinds, templates, xs, ys);
            var roots = CardStamp.CardRoots(elements);
            Assert.AreEqual(4, roots.Count);
            for (int i = 0; i < roots.Count; i++)
            {
                double[] r = CardStamp.Rect(elements[roots[i]]);
                Assert.That(Math.Abs(r[0] - slots[i].X), Is.LessThan(0.5));
                Assert.That(Math.Abs(r[1] - slots[i].Y), Is.LessThan(0.5));
                Assert.AreEqual(kinds.Split(',')[i], CardStamp.KindOf(elements, roots[i]));
            }
        }

        [Test]
        public void ItemCard_KeepsTheItemWhenTheFooterStateChanges()
        {
            var cards = new List<List<Dictionary<string, object>>>();
            cards.AddRange(CardStamp.ExtractAll(Load("shop_rings_default")));
            cards.AddRange(CardStamp.ExtractAll(Load("shop_rings_mixed")));
            var duckie = CardStamp.MatchItem(cards, "Rubber Duckie Floatie", "equipped");
            Assert.AreEqual("Rubber Duckie Floatie", CardStamp.TitleOf(duckie));
            Assert.AreEqual("equipped", CardStamp.KindOf(duckie, 0));
            var candy = CardStamp.MatchItem(cards, "Peppermint Candy Ring", "locked");
            Assert.AreEqual("Peppermint Candy Ring", CardStamp.TitleOf(candy));
            Assert.AreEqual("locked", CardStamp.KindOf(candy, 0));
            Assert.That(FooterText(candy), Does.Contain("1200"));
            var kitty = CardStamp.MatchItem(cards, "Cosmic Kitty Saucer", "owned");
            Assert.AreEqual("Cosmic Kitty Saucer", CardStamp.TitleOf(kitty));
            Assert.AreEqual("owned", CardStamp.KindOf(kitty, 0));
            Assert.That(FooterText(kitty), Does.Contain("Wear"));
        }

        static string FooterText(List<Dictionary<string, object>> tree)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < tree.Count; i++)
            {
                if (!tree[i].TryGetValue("runs", out var runs) || !(runs is List<object> list)) continue;
                for (int r = 0; r < list.Count; r++)
                {
                    if (list[r] is Dictionary<string, object> run && run.TryGetValue("s", out var s) && s is string text)
                        sb.Append(text);
                }
            }
            return sb.ToString();
        }

        [Test]
        public void SpecParse_StaysUnderTheLoadBudget()
        {
            string path = Path.Combine(Application.dataPath, "Resources/Parity/Spec/maps_world_1_all_complete__zero.json");
            string json = File.ReadAllText(path);
            var watch = Stopwatch.StartNew();
            Assert.IsNotNull(MiniJson.Parse(json));
            watch.Stop();
            Assert.That(watch.Elapsed.TotalMilliseconds, Is.LessThan(150), "parse ms " + watch.Elapsed.TotalMilliseconds);
        }

        static void ExpectStack(string spec, string cls, System.Func<double[], CardRect[]> place)
        {
            var rects = Roots(spec, cls);
            var heights = new double[rects.Count];
            for (int i = 0; i < rects.Count; i++) heights[i] = rects[i][3];
            var slots = place(heights);
            for (int i = 0; i < rects.Count; i++)
            {
                Assert.That(Math.Abs(slots[i].X - rects[i][0]), Is.LessThan(0.5), spec + " x " + i);
                Assert.That(Math.Abs(slots[i].Y - rects[i][1]), Is.LessThan(0.5), spec + " y " + i);
                Assert.That(Math.Abs(slots[i].W - rects[i][2]), Is.LessThan(0.5), spec + " w " + i);
            }
        }

        static void ExpectLevels(string spec)
        {
            var rects = Roots(spec, "level-tile", true);
            var heights = new double[rects.Count];
            for (int i = 0; i < rects.Count; i++) heights[i] = rects[i][3];
            var slots = CardInstancer.LevelGrid(heights);
            for (int i = 0; i < rects.Count; i++)
            {
                Assert.That(Math.Abs(slots[i].X - rects[i][0]), Is.LessThan(0.5), spec + " x " + i);
                Assert.That(Math.Abs(slots[i].Y - rects[i][1]), Is.LessThan(0.5), spec + " y " + i);
            }
        }

        static void ExpectDaily(string spec)
        {
            var rects = Roots(spec, "sector-card daily-shift-card");
            var heights = new double[rects.Count];
            for (int i = 0; i < rects.Count; i++) heights[i] = rects[i][3];
            var slots = CardInstancer.StackDaily(rects[0][1], heights);
            for (int i = 0; i < rects.Count; i++)
            {
                Assert.That(Math.Abs(slots[i].X - rects[i][0]), Is.LessThan(0.5), spec + " x");
                Assert.That(Math.Abs(slots[i].Y - rects[i][1]), Is.LessThan(0.5), spec + " y " + i);
            }
        }

        static List<double[]> Roots(string spec, string cls, bool prefix = false)
        {
            var elements = Load(spec);
            var rects = new List<double[]>();
            for (int i = 0; i < elements.Count; i++)
            {
                string c = elements[i].TryGetValue("c", out var v) && v is string s ? s : "";
                bool hit = prefix ? c == cls || c.StartsWith(cls + " ", StringComparison.Ordinal) : c == cls;
                if (hit) rects.Add(CardStamp.Rect(elements[i]));
            }
            Assert.That(rects.Count, Is.GreaterThan(0), spec + " " + cls);
            return rects;
        }

        static List<Dictionary<string, object>> Load(string spec)
        {
            string path = Path.Combine(Application.dataPath, "Resources/Parity/Spec/" + spec + "__zero.json");
            var root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(path));
            var raw = (List<object>)root["elements"];
            var list = new List<Dictionary<string, object>>(raw.Count);
            for (int i = 0; i < raw.Count; i++) list.Add((Dictionary<string, object>)raw[i]);
            return list;
        }
    }
}
