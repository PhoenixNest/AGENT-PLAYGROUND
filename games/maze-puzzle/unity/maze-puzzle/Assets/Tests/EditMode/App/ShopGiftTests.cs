using System;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.App.Tests
{
    public class ShopGiftTests
    {
        const double BootEpoch = 1790870401000.0; // 2026-10-01T16:00:01.000Z
        const double NextUtcDay = 1790899200000.0; // 2026-10-02T00:00:00.000Z
        const string ClaimToast = "🎁 Claimed +100 🪙 Janitor Stipend!";

        static readonly string[] ShopSpecs =
        {
            "shop_rings_default",
            "shop_rings_mixed",
            "shop_singularities_default",
            "shop_singularities_mixed",
            "shop_skyboxes_default",
            "shop_skyboxes_mixed"
        };

        [Test]
        public void ClaimOnce_AcrossTabs_CreditsIncreaseOnce_OneReminderToast()
        {
            var audio = new SilentAudio();
            var app = NewApp(audio);
            app.ShowPage("shop");
            double start = app.Economy.Profile.Credits;
            Assert.AreEqual(250, start, 0.001);
            Assert.IsTrue(app.Economy.CanClaimDailyGift());
            Assert.AreEqual("shop_rings_default", app.TemplateId);

            app.Click("btn-claim-daily-gift");
            Assert.AreEqual(start + 100, app.Economy.Profile.Credits, 0.001);
            Assert.AreEqual("2026-10-01", app.Economy.Profile.LastDailyGiftDate);
            Assert.IsFalse(app.Economy.CanClaimDailyGift());
            Assert.AreEqual(1, app.Model.Toasts.Count);
            Assert.AreEqual(ClaimToast, app.Model.Toasts[0].Text);
            Assert.AreEqual(1, audio.Cues.Count);
            Assert.AreEqual("booster", audio.Cues[0]);
            Assert.AreEqual(1, app.Haptics.Count);
            Assert.AreEqual(40, app.Haptics[0]);
            Assert.AreEqual("shop_gift_claimed", app.TemplateId);
            Assert.AreEqual("Claimed Today ✨", app.Model.Elements["btn-claim-daily-gift"].Text);

            for (int tab = 0; tab < 3; tab++)
            {
                app.ClickClass("tab-btn", tab);
                for (int press = 0; press < 3; press++) app.Click("btn-claim-daily-gift");
            }
            app.ClickClass("tab-btn", 0);
            for (int press = 0; press < 3; press++) app.Click("btn-claim-daily-gift");

            Assert.AreEqual(start + 100, app.Economy.Profile.Credits, 0.001, "credits increased more than once");
            Assert.AreEqual("2026-10-01", app.Economy.Profile.LastDailyGiftDate);
            Assert.IsFalse(app.Economy.CanClaimDailyGift());
            Assert.AreEqual(2, app.Model.Toasts.Count, "later presses must add exactly one reminder toast");
            Assert.AreEqual(ClaimToast, app.Model.Toasts[0].Text);
            Assert.AreEqual(AppController.AlreadyClaimedToast, app.Model.Toasts[1].Text);
            Assert.AreEqual(1, audio.Cues.Count);
            Assert.AreEqual(1, app.Haptics.Count);
        }

        [Test]
        public void Templates_AfterClaim_OnEachTabAndMixedInventory()
        {
            var app = NewApp(new SilentAudio());
            app.ShowPage("shop");
            app.Click("btn-claim-daily-gift");
            Assert.AreEqual("shop_gift_claimed", TemplateOn(app, 0));
            Assert.AreEqual("shop_singularities_default", TemplateOn(app, 1));
            Assert.AreEqual("shop_skyboxes_default", TemplateOn(app, 2));
            Assert.AreEqual("Claimed Today ✨", app.Model.Elements["btn-claim-daily-gift"].Text);
            Assert.AreEqual("Allowance Claimed! ✨", app.Model.Elements["gift-banner-title"].Text);
            Assert.AreEqual("🎉", app.Model.Elements["gift-banner-icon"].Text);

            var mixed = NewApp(new SilentAudio());
            AddOwned(mixed, "rings", "ring_duckie", "ring_candyland");
            mixed.Economy.Profile.Equipped.Set("ring", "ring_candyland");
            AddOwned(mixed, "singularities", "singularity_strawberry");
            mixed.Economy.Profile.Equipped.Set("singularity", "singularity_strawberry");
            AddOwned(mixed, "skyboxes", "skybox_cheese_moon");
            mixed.Economy.Profile.Equipped.Set("skybox", "skybox_cheese_moon");
            mixed.ShowPage("shop");
            mixed.Click("btn-claim-daily-gift");
            Assert.AreEqual("shop_rings_mixed", TemplateOn(mixed, 0));
            Assert.AreEqual("shop_singularities_mixed", TemplateOn(mixed, 1));
            Assert.AreEqual("shop_skyboxes_mixed", TemplateOn(mixed, 2));

            var stamped = NewApp(new SilentAudio());
            AddOwned(stamped, "rings", "ring_spacecat");
            stamped.Economy.Profile.Equipped.Set("ring", "ring_spacecat");
            stamped.ShowPage("shop");
            stamped.Click("btn-claim-daily-gift");
            Assert.AreEqual("shop_gift_claimed", stamped.TemplateId);
            Assert.AreEqual("ring_donut:owned,ring_duckie:locked,ring_candyland:locked,ring_spacecat:equipped", stamped.CardStamp);
        }

        [Test]
        public void Claim_PersistsThroughSaveLoad()
        {
            var store = new MemoryProfileStore();
            var audio = new SilentAudio();
            var app = NewApp(store, audio);
            app.ShowPage("shop");
            app.Click("btn-claim-daily-gift");
            double credits = app.Economy.Profile.Credits;
            string saved = store.GetItem(EconomyManager.StorageKey);
            Assert.That(saved, Does.Contain("2026-10-01"));

            var again = NewApp(store, new SilentAudio());
            Assert.AreEqual(credits, again.Economy.Profile.Credits, 0.001);
            Assert.AreEqual("2026-10-01", again.Economy.Profile.LastDailyGiftDate);
            Assert.IsFalse(again.Economy.CanClaimDailyGift());
            again.ShowPage("shop");
            again.Click("btn-claim-daily-gift");
            Assert.AreEqual(credits, again.Economy.Profile.Credits, 0.001);
            Assert.AreEqual(1, again.Model.Toasts.Count);
            Assert.AreEqual(AppController.AlreadyClaimedToast, again.Model.Toasts[0].Text);
            Assert.AreEqual("shop_gift_claimed", again.TemplateId);
        }

        [Test]
        public void Claim_AgainOnNextUtcDay()
        {
            var app = NewApp(new SilentAudio());
            app.ShowPage("shop");
            app.Click("btn-claim-daily-gift");
            double once = app.Economy.Profile.Credits;

            app.CatchUp(0, NextUtcDay - 1);
            Assert.AreEqual("2026-10-01", app.Economy.Today());
            Assert.IsFalse(app.Economy.CanClaimDailyGift());
            app.Click("btn-claim-daily-gift");
            Assert.AreEqual(once, app.Economy.Profile.Credits, 0.001);
            Assert.AreEqual(2, app.Model.Toasts.Count);
            Assert.AreEqual(AppController.AlreadyClaimedToast, app.Model.Toasts[1].Text);

            app.CatchUp(0, NextUtcDay);
            Assert.AreEqual("2026-10-02", app.Economy.Today());
            Assert.IsTrue(app.Economy.CanClaimDailyGift());
            Assert.AreEqual("shop_rings_default", app.TemplateId);
            app.Click("btn-claim-daily-gift");
            Assert.AreEqual(once + 100, app.Economy.Profile.Credits, 0.001);
            Assert.AreEqual("2026-10-02", app.Economy.Profile.LastDailyGiftDate);
            Assert.IsFalse(app.Economy.CanClaimDailyGift());
            Assert.AreEqual(ClaimToast, app.Model.Toasts[app.Model.Toasts.Count - 1].Text);
            int toastsAfterClaim = app.Model.Toasts.Count;
            app.Click("btn-claim-daily-gift");
            Assert.AreEqual(once + 100, app.Economy.Profile.Credits, 0.001);
            Assert.LessOrEqual(app.Model.Toasts.Count, toastsAfterClaim + 1);
            int reminders = 0;
            for (int i = 0; i < app.Model.Toasts.Count; i++)
                if (app.Model.Toasts[i].Text == AppController.AlreadyClaimedToast) reminders++;
            Assert.AreEqual(1, reminders, "reminder toast must not stack");
        }

        [Test]
        public void ClaimedBanner_PixelsMatchGiftClaimedOnEveryShopState()
        {
            Color32[] reference = BannerOf("shop_gift_claimed", false, true);
            Color32[] referenceLive = BannerOf("shop_gift_claimed", false, true, false);
            for (int i = 0; i < ShopSpecs.Length; i++)
            {
                string spec = ShopSpecs[i];
                Color32[] painted = BannerOf(spec, true, true);
                AssertSame(spec + " frozen", reference, painted);
                Color32[] bare = BannerOf(spec, true, false);
                Color32[] bareRef = BannerOf("shop_gift_claimed", false, false);
                AssertSame(spec + " without text overrides", bareRef, bare);
            }

            Color32[] stamped = BannerOf("shop_rings_default", true, true, true, "ring_donut:owned,ring_duckie:locked,ring_candyland:locked,ring_spacecat:equipped");
            AssertSame("stamped rings", reference, stamped);
            Color32[] live = BannerOf("shop_singularities_default", true, true, false);
            AssertSame("singularities live vs claimed live", referenceLive, live);
        }

        [Test]
        public void ClaimedBanner_StaysStillAcrossTheGiftAnimationClock()
        {
            using (var chrome = HomeChrome.Load("shop_singularities_default", "zero"))
            {
                chrome.UseClaimedGiftBanner(true);
                ApplyClaimedText(chrome);
                chrome.Draw(1500);
                Color32[] early = ReadBanner(chrome);
                chrome.Draw(4000);
                Color32[] later = ReadBanner(chrome);
                AssertSame("singularities 1500 vs 4000", early, later);
            }
        }

        [Test]
        public void ClaimedBanner_CacheRebuildsWhenTheOverrideSetChanges()
        {
            using (var chrome = HomeChrome.Load("shop_skyboxes_mixed", "zero"))
            {
                ApplyClaimedText(chrome);
                chrome.Draw(4000);
                int baked = chrome.CacheBuilds;
                Assert.Greater(baked, 0);
                chrome.UseClaimedGiftBanner(true);
                chrome.Draw(4000);
                Assert.Greater(chrome.CacheBuilds, baked, "claimed banner ids did not drop the layer cache");
                int after = chrome.CacheBuilds;
                chrome.TextOverrides["gift-banner-title"] = "Allowance Claimed! ✨";
                chrome.Draw(4000);
                Assert.AreEqual(after, chrome.CacheBuilds, "a text value change rebuilt the cache");
                chrome.UseClaimedGiftBanner(false);
                chrome.Draw(4000);
                Assert.Greater(chrome.CacheBuilds, after, "clearing the claimed banner did not drop the cache");
            }
        }

        [Test]
        public void GiftClaimedSpec_IgnoresTheOverride()
        {
            Color32[] plain = BannerOf("shop_gift_claimed", false, true);
            Color32[] flagged = BannerOf("shop_gift_claimed", true, true);
            AssertSame("shop_gift_claimed", plain, flagged);

            using (var chrome = HomeChrome.Load("shop_gift_claimed", "zero"))
            {
                ApplyClaimedText(chrome);
                chrome.Draw(4000);
                int baked = chrome.CacheBuilds;
                chrome.UseClaimedGiftBanner(true);
                chrome.Draw(4000);
                Assert.AreEqual(baked, chrome.CacheBuilds, "shop_gift_claimed rebuilt its cache");
            }
        }

        static string TemplateOn(AppController app, int tab)
        {
            app.ClickClass("tab-btn", tab);
            return app.TemplateId;
        }

        static void AddOwned(AppController app, string plural, params string[] ids)
        {
            var list = app.Economy.Profile.Unlocked.Get(plural);
            for (int i = 0; i < ids.Length; i++) list.Add(ids[i]);
        }

        static AppController NewApp(SilentAudio audio)
        {
            return NewApp(new MemoryProfileStore(), audio);
        }

        static AppController NewApp(MemoryProfileStore store, SilentAudio audio)
        {
            return new AppController(store, new ManualAppClock(0, BootEpoch), audio, () => 0);
        }

        static string ReportDir()
        {
            var dir = new DirectoryInfo(Application.dataPath);
            for (int i = 0; i < 4 && dir != null; i++) dir = dir.Parent;
            if (dir == null) return null;
            string path = Path.Combine(dir.FullName, "tools", "parity", "reports", "shop-gift-2026-10-08");
            if (!Directory.Exists(path)) return null;
            return path;
        }

        static void ApplyClaimedText(HomeChrome chrome)
        {
            chrome.TextOverrides["gift-banner-title"] = "Allowance Claimed! ✨";
            chrome.TextOverrides["gift-banner-desc"] = "Come back tomorrow for another +100 🪙 care package!";
            chrome.TextOverrides["gift-banner-icon"] = "🎉";
            chrome.TextOverrides["btn-claim-daily-gift"] = "Claimed Today ✨";
        }

        static Color32[] BannerOf(string spec, bool claim, bool text, bool frozen = true, string stamp = null)
        {
            using (var chrome = HomeChrome.Load(spec, "zero"))
            {
                if (!string.IsNullOrEmpty(stamp))
                {
                    var heights = HomeChrome.ShopCardHeights(stamp);
                    Assert.IsNotNull(heights, spec);
                    var slots = CardInstancer.StackShop(heights);
                    var xs = new double[slots.Length];
                    var ys = new double[slots.Length];
                    for (int i = 0; i < slots.Length; i++)
                    {
                        xs[i] = slots[i].X;
                        ys[i] = slots[i].Y;
                    }
                    chrome.StampShopCards(stamp, xs, ys);
                }
                if (claim) chrome.UseClaimedGiftBanner(true);
                if (text) ApplyClaimedText(chrome);
                if (frozen) chrome.Draw();
                else chrome.Draw(4000);
                Color32[] pixels = ReadBanner(chrome);
                string dir = ReportDir();
                if (dir != null && text && frozen)
                {
                    string name = string.IsNullOrEmpty(stamp) ? spec : spec + "_stamped";
                    File.WriteAllBytes(Path.Combine(dir, name + ".png"), Encode(chrome));
                }
                return pixels;
            }
        }

        static void AssertSame(string label, Color32[] expect, Color32[] actual)
        {
            Assert.AreEqual(expect.Length, actual.Length, label);
            int diff = 0;
            int first = -1;
            for (int i = 0; i < expect.Length; i++)
            {
                if (expect[i].r != actual[i].r || expect[i].g != actual[i].g || expect[i].b != actual[i].b || expect[i].a != actual[i].a)
                {
                    if (first < 0) first = i;
                    diff++;
                }
            }
            if (diff == 0) return;
            var sample = first >= 0 ? actual[first] : new Color32();
            var want = first >= 0 ? expect[first] : new Color32();
            Assert.Fail(label + " banner pixels differ " + diff + "/" + expect.Length
                + " first index " + first
                + " got " + sample.r + "," + sample.g + "," + sample.b
                + " want " + want.r + "," + want.g + "," + want.b);
        }

        static Color32[] ReadBanner(HomeChrome chrome)
        {
            var rt = chrome.Target;
            double scale = chrome.RasterScale;
            int x = (int)Math.Round(16 * scale);
            int yTop = (int)Math.Round(86 * scale);
            int w = (int)Math.Round(398 * scale);
            int h = (int)Math.Round(86.5 * scale);
            int y = rt.height - (yTop + h);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(x, y, w, h), 0, 0, false);
            tex.Apply(false);
            var pixels = tex.GetPixels32();
            RenderTexture.active = prev;
            UnityEngine.Object.DestroyImmediate(tex);
            return pixels;
        }

        static byte[] Encode(HomeChrome chrome)
        {
            var rt = chrome.Target;
            double scale = chrome.RasterScale;
            int x = (int)Math.Round(16 * scale);
            int yTop = (int)Math.Round(86 * scale);
            int w = (int)Math.Round(398 * scale);
            int h = (int)Math.Round(86.5 * scale);
            int y = rt.height - (yTop + h);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false, false);
            tex.ReadPixels(new Rect(x, y, w, h), 0, 0, false);
            tex.Apply(false);
            byte[] png = tex.EncodeToPNG();
            RenderTexture.active = prev;
            UnityEngine.Object.DestroyImmediate(tex);
            return png;
        }
    }
}
