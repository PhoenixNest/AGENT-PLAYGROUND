using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class TabPrewarmTests
    {
        [Test]
        public void PrewarmTemplates_FreshProfile_ResolvesTheFiveMainTabs()
        {
            var app = FreshLoadingApp();
            Assert.AreEqual("loading", app.CurrentPage);
            var zero = ParityPages.PrewarmTemplates(app, "zero");
            Assert.AreEqual(ParityPages.MainTabPages.Length, zero.Length);
            Assert.AreEqual(5, zero.Length);
            string[] expect =
            {
                "home_default",
                "maps_world_1_fresh",
                "daily_default",
                "shop_rings_default",
                "achievements_none"
            };
            var seen = new HashSet<string>();
            for (int i = 0; i < zero.Length; i++)
            {
                Assert.AreEqual(ParityPages.MainTabPages[i], zero[i].Page);
                Assert.AreEqual(expect[i], zero[i].TemplateId, zero[i].Page);
                Assert.IsTrue(string.IsNullOrEmpty(zero[i].CardStamp), zero[i].Page);
                Assert.AreEqual("zero", zero[i].Profile);
                Assert.AreEqual(ParityPages.CacheKey(expect[i], "zero", null), zero[i].CacheKey);
                Assert.IsTrue(seen.Add(zero[i].TemplateId));
            }
            Assert.AreEqual("loading", app.CurrentPage, "resolving templates must not leave the loading page");

            var phone = ParityPages.PrewarmTemplates(app, ParityPages.IphoneProfile);
            Assert.AreEqual(zero.Length, phone.Length);
            for (int i = 0; i < phone.Length; i++)
            {
                Assert.AreEqual(zero[i].TemplateId, phone[i].TemplateId);
                Assert.AreEqual(ParityPages.IphoneProfile, phone[i].Profile);
                Assert.AreEqual(ParityPages.CacheKey(phone[i].TemplateId, ParityPages.IphoneProfile, phone[i].CardStamp), phone[i].CacheKey);
            }
        }

        [Test]
        public void CacheKey_IncludesProfileAndCardStamp()
        {
            Assert.AreEqual("home_default@zero", ParityPages.CacheKey("home_default", "zero", null));
            Assert.AreEqual("home_default@zero", ParityPages.CacheKey("home_default", "zero", ""));
            Assert.AreEqual("shop_rings_default@iphone#ring_donut:owned", ParityPages.CacheKey("shop_rings_default", "iphone", "ring_donut:owned"));
        }

        [Test]
        public void CacheCapacity_HoldsEveryMainTabPlusAModalMargin()
        {
            Assert.GreaterOrEqual(ParityPages.CacheCapacity, ParityPages.MainTabPages.Length + ParityPages.ModalCacheMargin);
            Assert.GreaterOrEqual(ParityPages.ModalCacheMargin, 1);
        }

        [Test]
        public void ShouldPrewarmTabs_IsOffWhileSelfTesting()
        {
            Assert.IsFalse(ParityPages.ShouldPrewarmTabs(true));
            Assert.IsTrue(ParityPages.ShouldPrewarmTabs(false));
        }

        [Test]
        public void RasterScale_IsStaleWhenTheLiveStepDiffers()
        {
            Assert.IsTrue(ParityPages.RasterScaleIsStale(3, 2));
            Assert.IsTrue(ParityPages.RasterScaleIsStale(2, 3));
            Assert.IsFalse(ParityPages.RasterScaleIsStale(3, 3));
            Assert.IsFalse(ParityPages.RasterScaleIsStale(2, 2.04));
            Assert.IsTrue(ParityPages.RasterScaleIsStale(2, 1.5));
        }

        [Test]
        public void PinnedTabs_SurviveRepeatedCycling_ModalsStayBounded()
        {
            int tabs = ParityPages.MainTabPages.Length;
            int capacity = tabs + ParityPages.ModalCacheMargin;
            var lru = new ChromeLru<string>(capacity);
            var evicted = new List<string>();
            var values = new string[tabs];
            for (int i = 0; i < tabs; i++)
            {
                string key = "tab" + i;
                values[i] = "T" + i;
                lru.Pin(key);
                lru.Add(key, values[i], evicted);
                Assert.IsTrue(lru.IsPinned(key));
            }
            Assert.AreEqual(0, evicted.Count);
            for (int cycle = 0; cycle < 6; cycle++)
            {
                for (int i = 0; i < tabs; i++)
                {
                    Assert.IsTrue(lru.TryGet("tab" + i, out var value));
                    Assert.AreEqual(values[i], value);
                    lru.Add("tab" + i, values[i], evicted);
                }
            }
            Assert.AreEqual(0, evicted.Count, "cycling the main tabs evicted a resident page");

            int opened = ParityPages.ModalCacheMargin + 5;
            for (int i = 0; i < opened; i++)
                lru.Add("modal" + i, "M" + i, evicted);
            Assert.LessOrEqual(lru.Count, capacity, "modal overflow grew the cache without bound");
            Assert.AreEqual(capacity, lru.Count);
            for (int i = 0; i < tabs; i++)
                Assert.IsTrue(lru.Contains("tab" + i), "modal evicted tab" + i);
            Assert.Greater(evicted.Count, 0);
            Assert.IsFalse(lru.Contains("modal0"), "the oldest modal should leave once the margin is full");
            Assert.IsTrue(lru.Contains("modal" + (opened - 1)));
        }

        [Test]
        public void TryTakeTabPrewarm_BakesOneTemplatePerCall()
        {
            var pending = new Queue<string>();
            pending.Enqueue("home_default");
            pending.Enqueue("maps_world_1_fresh");
            pending.Enqueue("daily_default");
            Assert.IsFalse(ParityPages.TryTakeTabPrewarm(pending, true, false, out _));
            Assert.AreEqual(3, pending.Count, "a deferred frame must not start a bake");
            Assert.IsFalse(ParityPages.TryTakeTabPrewarm(pending, false, true, out _));
            Assert.AreEqual(3, pending.Count, "the loading bar's override set is not ready yet");
            Assert.IsTrue(ParityPages.TryTakeTabPrewarm(pending, false, false, out string first));
            Assert.AreEqual("home_default", first);
            Assert.AreEqual(2, pending.Count);
            Assert.IsTrue(ParityPages.TryTakeTabPrewarm(pending, false, false, out string second));
            Assert.AreEqual("maps_world_1_fresh", second);
        }

        [Test]
        public void Resize_DropsBakedLayersUntilTheNewScaleIsBaked()
        {
            bool savedDefer = HomeChrome.DeferLayerCache;
            HomeChrome.DeferLayerCache = false;
            var chrome = HomeChrome.Load("home_default", "zero");
            try
            {
                chrome.BakeStaticLayers();
                Assert.IsTrue(chrome.LayerCacheReady);
                Assert.Greater(chrome.BakedLayerCount, 0);
                long bakedAt3 = chrome.BakedLayerBytes;
                Assert.Greater(bakedAt3, 0);
                Assert.IsFalse(chrome.InvalidateIfScaleDiffers(chrome.RasterScale));
                Assert.IsTrue(chrome.LayerCacheReady, "the same scale must keep the bake");
                Assert.IsTrue(chrome.InvalidateIfScaleDiffers(2));
                Assert.IsFalse(chrome.LayerCacheReady);
                Assert.AreEqual(0, chrome.BakedLayerBytes);
                Assert.IsTrue(ParityPages.RasterScaleIsStale(3, chrome.RasterScale));
                chrome.BakeStaticLayers();
                Assert.IsTrue(chrome.LayerCacheReady);
                Assert.AreEqual(2, chrome.RasterScale, 0.01);
                Assert.AreEqual(Mathf.RoundToInt(430 * 2f), chrome.PixelWidth);
                Assert.Greater(chrome.BakedLayerBytes, 0);
                Assert.Less(chrome.BakedLayerBytes, bakedAt3);
            }
            finally
            {
                HomeChrome.DeferLayerCache = savedDefer;
                chrome.Dispose();
            }
        }

        [Test]
        public void Dispose_ReleasesBakedRenderTextures()
        {
            bool savedDefer = HomeChrome.DeferLayerCache;
            HomeChrome.DeferLayerCache = false;
            var chrome = HomeChrome.Load("daily_default", "zero");
            try
            {
                chrome.BakeStaticLayers();
                long layers = chrome.BakedLayerBytes;
                Assert.Greater(layers, 0);
                var target = chrome.Target;
                Assert.IsTrue(target.IsCreated());
                chrome.Dispose();
                chrome = null;
                Assert.IsFalse(target.IsCreated());
            }
            finally
            {
                HomeChrome.DeferLayerCache = savedDefer;
                if (chrome != null) chrome.Dispose();
            }
        }

        [Test]
        public void MainTabs_ReportBakedBytesAt3xAnd2x()
        {
            bool savedDefer = HomeChrome.DeferLayerCache;
            HomeChrome.DeferLayerCache = false;
            var app = FreshLoadingApp();
            var entries = ParityPages.PrewarmTemplates(app, "zero");
            var report = new StringBuilder();
            try
            {
                for (int s = 0; s < 2; s++)
                {
                    double scale = s == 0 ? 3 : 2;
                    for (int i = 0; i < entries.Length; i++)
                    {
                        var chrome = HomeChrome.Load(entries[i].TemplateId, "zero");
                        try
                        {
                            if (scale < 2.5) chrome.InvalidateIfScaleDiffers(scale);
                            chrome.BakeStaticLayers();
                            Assert.IsTrue(chrome.LayerCacheReady, entries[i].TemplateId);
                            Assert.Greater(chrome.BakedLayerCount, 0, entries[i].TemplateId);
                            Assert.Greater(chrome.BakedLayerBytes, 0, entries[i].TemplateId);
                            Assert.AreEqual(scale, chrome.RasterScale, 0.01, entries[i].TemplateId);
                            string line = entries[i].TemplateId
                                + " scale=" + scale.ToString("0.#")
                                + " layers=" + chrome.BakedLayerCount
                                + " slots=" + chrome.BakedLayerSlots
                                + " px=" + chrome.PixelWidth + "x" + chrome.PixelHeight
                                + " layerBytes=" + chrome.BakedLayerBytes
                                + " canvasBytes=" + chrome.CanvasResidentBytes
                                + " totalBytes=" + chrome.ResidentBytes;
                            report.Append(line).Append('\n');
                            Debug.Log("[TabBake] " + line);
                        }
                        finally
                        {
                            chrome.Dispose();
                        }
                    }
                }
                File.WriteAllText("/tmp/prebake-tabs/memory.txt", report.ToString());
            }
            finally
            {
                HomeChrome.DeferLayerCache = savedDefer;
            }
        }

        static AppController FreshLoadingApp()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.BeginBoot();
            return app;
        }
    }
}
