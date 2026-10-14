using System.Collections.Generic;
using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    /// <summary>
    /// The shop swaps its cards for the measured owned / equipped / locked ones (<c>CardStamp</c>), so the
    /// "Wear" buttons on screen are not where the captured file puts them. A tap must be matched against
    /// what is painted, otherwise "Wear" does nothing once a second item is bought.
    /// </summary>
    public class ShopEquipHitTests
    {
        static readonly string[] Tabs = { "rings", "singularities", "skyboxes" };

        [Test]
        public void Wear_OnEveryPreviouslyOwnedItem_EquipsThatItem_OnEveryTab()
        {
            for (int tab = 0; tab < Tabs.Length; tab++)
            {
                var app = NewApp();
                app.ShowPage("shop");
                app.ClickClass("tab-btn", tab);
                var catalog = EconomyCatalog.Category(Tabs[tab]);
                string singular = app.Economy.NormalizeCategory(Tabs[tab]).Singular;
                string start = app.Economy.Profile.Equipped.Get(singular);

                // Buy every locked item in turn; each purchase equips the new item.
                var bought = new List<string> { start };
                for (int i = 0; i < catalog.Length; i++)
                {
                    if (app.Economy.IsUnlocked(Tabs[tab], catalog[i].Id)) continue;
                    bool bought0 = TapNth(app, "btn-buy-item", 0, out string error0);
                    Assert.IsTrue(bought0, error0);
                    string now = app.Economy.Profile.Equipped.Get(singular);
                    Assert.AreNotEqual(start, now, Tabs[tab] + ": a purchase must equip the new item");
                    bought.Add(now);
                }
                Assert.GreaterOrEqual(bought.Count, 3, Tabs[tab] + ": need several owned items for this test");

                // Now wear each owned item that is not equipped, one tap each, by tapping the painted button.
                for (int round = 0; round < bought.Count * 2; round++)
                {
                    string equipped = app.Economy.Profile.Equipped.Get(singular);
                    string target = FirstOwnedNotEquipped(app, Tabs[tab], catalog, equipped, round);
                    int index = WearIndexOf(app, Tabs[tab], catalog, target);
                    string error;
                    bool hit = TapNth(app, "btn-equip-item", index, out error);
                    Assert.IsNull(error, Tabs[tab] + " round " + round + ": " + error);
                    Assert.IsTrue(hit);
                    Assert.AreEqual(target, app.Economy.Profile.Equipped.Get(singular),
                        Tabs[tab] + " round " + round + ": tapping Wear on " + target + " did not equip it");
                }
            }
        }

        [Test]
        public void StampedShop_ButtonsAreHitWhereTheyArePainted_NotWhereTheCapturedFilePutsThem()
        {
            var app = NewApp();
            app.ShowPage("shop");
            // One purchase gives "donut owned, duckie equipped": no captured page has that inventory, so the cards are stamped.
            Assert.IsTrue(TapNth(app, "btn-buy-item", 0, out _));
            Assert.AreEqual("ring_duckie", app.Economy.Profile.Equipped.Get("ring"));
            Assert.IsFalse(string.IsNullOrEmpty(app.CardStamp), "expected a stamped shop page");

            using (var chrome = Painted(app))
            {
                Assert.IsTrue(chrome.ShopStamped);
                // The captured file (what the viewer used before) has to be wrong somewhere, or this fix is moot.
                string raw = Resources.Load<TextAsset>("Parity/Spec/" + app.TemplateId + "__zero").text;
                int stampedButtons = 0, rawAgrees = 0;
                for (int i = 0; ; i++)
                {
                    if (!ParityPages.TryClassCenter(chrome.Elements, "btn-equip-item", i, out double x, out double y)) break;
                    stampedButtons++;
                    Assert.IsTrue(ParityPages.TryHitControl(chrome.Elements, x, y, out var painted), "painted button " + i);
                    Assert.AreEqual("btn-equip-item", painted.ClassToken);
                    Assert.AreEqual(i, painted.ClassIndex);
                    if (ParityPages.TryHitControl(raw, x, y, out var old) && old.ClassToken == "btn-equip-item" && old.ClassIndex == i) rawAgrees++;
                }
                Assert.GreaterOrEqual(stampedButtons, 1, "no painted Wear button");
                Assert.Less(rawAgrees, stampedButtons, "captured file already agrees with the painted buttons; nothing to fix");
            }
        }

        [Test]
        public void Viewer_TapOnPaintedWear_EquipsTheOlderItem_AfterBuyingANewOne()
        {
            var app = NewApp();
            app.ShowPage("shop");
            Assert.IsTrue(TapNth(app, "btn-buy-item", 0, out _));
            Assert.AreEqual("ring_duckie", app.Economy.Profile.Equipped.Get("ring"), "a purchase equips the new item");

            var go = new GameObject("shop-equip-probe");
            using (var chrome = Painted(app))
            {
                try
                {
                    var viewer = go.AddComponent<ParityViewer>();
                    viewer.BindToastProbe(chrome, app.TemplateId, app);
                    Assert.IsTrue(ParityPages.TryClassCenter(chrome.Elements, "btn-equip-item", 0, out double x, out double y));
                    viewer.ClickCss(x, y);
                    Assert.AreEqual("ring_donut", app.Economy.Profile.Equipped.Get("ring"), "tapping Wear on the donut must equip it");
                }
                finally
                {
                    Object.DestroyImmediate(go);
                }
            }
        }

        static string FirstOwnedNotEquipped(AppController app, string tab, CosmeticItem[] catalog, string equipped, int round)
        {
            var owned = new List<string>();
            for (int i = 0; i < catalog.Length; i++)
                if (app.Economy.IsUnlocked(tab, catalog[i].Id) && catalog[i].Id != equipped) owned.Add(catalog[i].Id);
            Assert.IsNotEmpty(owned);
            return owned[round % owned.Count];
        }

        static int WearIndexOf(AppController app, string tab, CosmeticItem[] catalog, string id)
        {
            string singular = app.Economy.NormalizeCategory(tab).Singular;
            string equipped = app.Economy.Profile.Equipped.Get(singular);
            int n = 0;
            for (int i = 0; i < catalog.Length; i++)
            {
                if (!app.Economy.IsUnlocked(tab, catalog[i].Id) || catalog[i].Id == equipped) continue;
                if (catalog[i].Id == id) return n;
                n++;
            }
            Assert.Fail(id + " has no Wear button");
            return -1;
        }

        /// <summary>
        /// Taps the nth painted button of <paramref name="token"/> exactly as the viewer does: hit test, then dispatch.
        /// Returns false with an error text when nothing was hit.
        /// </summary>
        static bool TapNth(AppController app, string token, int index, out string error)
        {
            error = null;
            using (var chrome = Painted(app))
            {
                if (!ParityPages.TryClassCenter(chrome.Elements, token, index, out double x, out double y))
                {
                    error = "no painted " + token + " #" + index + " on " + app.TemplateId + " (" + app.CardStamp + ")";
                    return false;
                }
                if (!ParityPages.TryHitControl(chrome.Elements, x, y, out var hit) || hit.ClassToken != token || hit.ClassIndex != index)
                {
                    error = "tap at " + x + "," + y + " did not reach " + token + " #" + index;
                    return false;
                }
                app.ClickClass(hit.ClassToken, hit.ClassIndex);
                return true;
            }
        }

        static HomeChrome Painted(AppController app)
        {
            var chrome = HomeChrome.Load(app.TemplateId, "zero");
            if (!string.IsNullOrEmpty(app.CardStamp)) ParityViewer.StampShop(chrome, app.CardStamp);
            return chrome;
        }

        static AppController NewApp()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 1790870401000.0), new SilentAudio(), () => 0);
            app.Economy.Profile.Credits = 100000;
            return app;
        }
    }
}
