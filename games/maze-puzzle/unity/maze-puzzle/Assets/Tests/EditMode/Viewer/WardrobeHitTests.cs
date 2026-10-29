using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;

namespace OrbitalSalvage.Viewer.Tests
{
    public class WardrobeHitTests
    {
        [Test]
        public void TryHitControl_OnWardrobeElements_FindsAllInteractiveControls()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.LoadSector(app.Sectors[0], false);
            app.Click("btn-game-skin");

            var chrome = HomeChrome.Load(app.TemplateId, "iphone");
            chrome.WardrobeOpen = true;
            chrome.WardrobeTab = "rings";
            chrome.WardrobeEconomy = app.Economy;

            var elements = chrome.Elements;

            // 1. Backdrop
            bool hitBackdrop = ParityPages.TryHitControl(elements, 200, 200, out var hit1);
            Assert.IsTrue(hitBackdrop);
            Assert.AreEqual("wardrobe-backdrop", hit1.Id);

            // 2. Close button: x=382, y=562, w=32, h=32 -> center (398, 578)
            bool hitClose = ParityPages.TryHitControl(elements, 398, 578, out var hit2);
            Assert.IsTrue(hitClose);
            Assert.AreEqual("btn-wardrobe-close", hit2.Id);

            // 3. Tab buttons: y=608, h=34
            // Rings tab: x=20, w=126 -> center (83, 625)
            bool hitTab1 = ParityPages.TryHitControl(elements, 83, 625, out var hit3);
            Assert.IsTrue(hitTab1);
            Assert.AreEqual("wardrobe-tab-rings", hit3.Id);

            // Singularities tab: x=152, w=126 -> center (215, 625)
            bool hitTab2 = ParityPages.TryHitControl(elements, 215, 625, out var hit4);
            Assert.IsTrue(hitTab2);
            Assert.AreEqual("wardrobe-tab-singularities", hit4.Id);

            // Skyboxes tab: x=284, w=126 -> center (347, 625)
            bool hitTab3 = ParityPages.TryHitControl(elements, 347, 625, out var hit5);
            Assert.IsTrue(hitTab3);
            Assert.AreEqual("wardrobe-tab-skyboxes", hit5.Id);

            // 4. Item cards:
            // Item 0 (ring_donut): x=16, y=656, w=194, h=58 -> center (113, 685)
            bool hitCard0 = ParityPages.TryHitControl(elements, 113, 685, out var hit6);
            Assert.IsTrue(hitCard0);
            Assert.AreEqual("wardrobe-item-ring_donut", hit6.Id);

            // Item 1 (ring_duckie): x=220, y=656, w=194, h=58 -> center (317, 685)
            bool hitCard1 = ParityPages.TryHitControl(elements, 317, 685, out var hit7);
            Assert.IsTrue(hitCard1);
            Assert.AreEqual("wardrobe-item-ring_duckie", hit7.Id);
        }

        [Test]
        public void WardrobeItemHit_DispatchesClickAndEquipsItem()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.LoadSector(app.Sectors[0], false);
            app.Click("btn-game-skin");

            var chrome = HomeChrome.Load(app.TemplateId, "iphone");
            chrome.WardrobeOpen = true;
            chrome.WardrobeTab = "rings";
            chrome.WardrobeEconomy = app.Economy;

            // Unlock Rubber Duckie Floatie
            app.Economy.AddCredits(1000);
            app.Economy.UnlockItem("rings", "ring_duckie", 500);

            // Hit card 1 (Rubber Duckie)
            bool hit = ParityPages.TryHitControl(chrome.Elements, 317, 685, out var hitResult);
            Assert.IsTrue(hit);
            Assert.AreEqual("wardrobe-item-ring_duckie", hitResult.Id);

            // Simulate dispatching click
            app.Click(hitResult.Id);

            // Verify equipped
            Assert.AreEqual("ring_duckie", app.Economy.Profile.Equipped.Get("ring"));
        }

        [Test]
        public void SwitchWardrobeTab_UpdatesHitElementsAndCardsImmediately()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.LoadSector(app.Sectors[0], false);
            app.Click("btn-game-skin");

            var chrome = HomeChrome.Load(app.TemplateId, "iphone");
            chrome.WardrobeOpen = true;
            chrome.WardrobeTab = app.WardrobeTab;
            chrome.WardrobeEconomy = app.Economy;

            // Initially on rings
            Assert.AreEqual("rings", chrome.WardrobeTab);
            bool hitRing0 = ParityPages.TryHitControl(chrome.Elements, 113, 685, out var hitRingRes);
            Assert.IsTrue(hitRing0);
            Assert.AreEqual("wardrobe-item-ring_donut", hitRingRes.Id);

            // Switch to singularities tab
            app.Click("wardrobe-tab-singularities");
            chrome.WardrobeTab = app.WardrobeTab;
            Assert.AreEqual("singularities", chrome.WardrobeTab);

            bool hitSing0 = ParityPages.TryHitControl(chrome.Elements, 113, 685, out var hitSingRes);
            Assert.IsTrue(hitSing0);
            Assert.AreEqual("wardrobe-item-singularity_nibbles", hitSingRes.Id);

            // Switch to skyboxes tab
            app.Click("wardrobe-tab-skyboxes");
            chrome.WardrobeTab = app.WardrobeTab;
            Assert.AreEqual("skyboxes", chrome.WardrobeTab);

            bool hitSky0 = ParityPages.TryHitControl(chrome.Elements, 113, 685, out var hitSkyRes);
            Assert.IsTrue(hitSky0);
            Assert.AreEqual("wardrobe-item-skybox_cotton_candy", hitSkyRes.Id);
        }
    }
}
