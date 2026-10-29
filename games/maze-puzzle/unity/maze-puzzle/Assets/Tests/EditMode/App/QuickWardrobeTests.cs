using System.Linq;

using NUnit.Framework;
using OrbitalSalvage.App;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;

namespace OrbitalSalvage.App.Tests
{
    public class QuickWardrobeTests
    {
        [Test]
        public void OpenWardrobe_SetsIsWardrobeOpenAndPausesGame()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.ShowPage("game");
            Assert.IsFalse(app.IsWardrobeOpen);
            Assert.IsFalse(app.IsPaused);

            app.Click("btn-game-skin");

            Assert.IsTrue(app.IsWardrobeOpen);
            Assert.IsTrue(app.IsPaused);
            Assert.IsTrue(app.ModalOpen("modal-wardrobe"));
            Assert.AreEqual("rings", app.WardrobeTab);
        }

        [Test]
        public void CloseWardrobe_ViaButton_ResumesGameAndClosesModal()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.ShowPage("game");
            app.Click("btn-game-skin");
            Assert.IsTrue(app.IsWardrobeOpen);

            app.Click("btn-wardrobe-close");

            Assert.IsFalse(app.IsWardrobeOpen);
            Assert.IsFalse(app.IsPaused);
            Assert.IsFalse(app.ModalOpen("modal-wardrobe"));
        }

        [Test]
        public void CloseWardrobe_ViaBackdrop_ResumesGame()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.ShowPage("game");
            app.Click("btn-game-skin");
            Assert.IsTrue(app.IsWardrobeOpen);

            app.Click("wardrobe-backdrop");

            Assert.IsFalse(app.IsWardrobeOpen);
            Assert.IsFalse(app.IsPaused);
            Assert.IsFalse(app.ModalOpen("modal-wardrobe"));
        }

        [Test]
        public void SwitchWardrobeTabs_UpdatesWardrobeTab()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.ShowPage("game");
            app.Click("btn-game-skin");
            Assert.AreEqual("rings", app.WardrobeTab);

            app.Click("wardrobe-tab-singularities");
            Assert.AreEqual("singularities", app.WardrobeTab);

            app.Click("wardrobe-tab-skyboxes");
            Assert.AreEqual("skyboxes", app.WardrobeTab);

            app.Click("wardrobe-tab-rings");
            Assert.AreEqual("rings", app.WardrobeTab);
        }

        [Test]
        public void EquipUnlockedSkin_SucceedsAndUpdatesProfile()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.ShowPage("game");
            app.Click("btn-game-skin");

            // Unlock Rubber Duckie Floatie
            app.Economy.AddCredits(1000);
            bool unlocked = app.Economy.UnlockItem("rings", "ring_duckie", 500);
            Assert.IsTrue(unlocked);

            // Click Rubber Duckie in wardrobe
            app.Click("wardrobe-item-ring_duckie");

            Assert.AreEqual("ring_duckie", app.Economy.Profile.Equipped.Get("ring"));
            Assert.IsTrue(app.ToastTexts().Any(t => t.Contains("Equipped Rubber Duckie Floatie!")));
        }

        [Test]
        public void EquipLockedSkin_IsStrictlyPreventedAndShowsWarningToast()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.ShowPage("game");
            app.Click("btn-game-skin");

            // Verify Peppermint Candy Ring is locked
            Assert.IsFalse(app.Economy.IsUnlocked("rings", "ring_candyland"));
            string equippedBefore = app.Economy.Profile.Equipped.Get("ring");
            Assert.AreEqual("ring_donut", equippedBefore);

            // Attempt to equip locked skin directly from wardrobe
            app.Click("wardrobe-item-ring_candyland");

            // Assertion: must NOT equip the locked skin
            Assert.AreEqual("ring_donut", app.Economy.Profile.Equipped.Get("ring"));
            Assert.IsTrue(app.ToastTexts().Any(t => t.Contains("locked! Unlock in Toy Shop")));
        }

        [Test]
        public void ResetGameplaySessionUi_ClosesWardrobeSheet()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.LoadSector(app.Sectors[0], false);
            app.Click("btn-game-skin");
            Assert.IsTrue(app.IsWardrobeOpen);

            app.RestartSector();

            Assert.IsFalse(app.IsWardrobeOpen);
            Assert.IsFalse(app.ModalOpen("modal-wardrobe"));
        }

        [Test]
        public void DrawModalWardrobe_SuccessiveFrames_DoesNotThrowIndexOutOfRange()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.LoadSector(app.Sectors[0], false);
            app.Click("btn-game-skin");

            var session = LiveGameSession.CreateBound(app.Economy, 430, 932);
            session.LoadSector(app.ActiveSector);

            var chrome = HomeChrome.Load(app.TemplateId, "iphone");
            chrome.ExternalPlayfield = session.Renderer.Target;
            chrome.WardrobeOpen = true;
            chrome.WardrobeTab = app.WardrobeTab;
            chrome.WardrobeEconomy = app.Economy;

            // Frame 1
            Assert.DoesNotThrow(() => chrome.Draw(0.0, -1, null, true, true));

            // Frame 2 (verifies that successive draw calls do not cause index out of bounds)
            Assert.DoesNotThrow(() => chrome.Draw(16.0, -1, null, false, true));

            // Frame 3 (switching tabs and redrawing)
            chrome.WardrobeTab = "singularities";
            Assert.DoesNotThrow(() => chrome.Draw(32.0, -1, null, false, true));

            session.Renderer.Dispose();
        }

        [Test]
        public void Session_WhenPaused_DoesNotRespondToPointerDragOrThrust()
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.LoadSector(app.Sectors[0], false);
            var session = LiveGameSession.CreateBound(app.Economy, 430, 932);
            session.LoadSector(app.ActiveSector);

            session.IsPaused = true;
            double initialAngle = session.Physics.RingAngle;

            // Attempt drag
            session.PointerDown(200, 300);
            session.PointerMove(250, 300);
            Assert.AreEqual(initialAngle, session.Physics.RingAngle, "RingAngle should not change while session is paused");

            // Attempt double tap burst
            session.PointerDown(300, 0, 430);
            session.PointerDown(300, 0, 430);
            Assert.IsFalse(session.Physics.RcsThrusting, "RcsThrusting should not activate while paused");

            // Attempt keyboard thrust
            session.KeyDown("a");
            Assert.IsFalse(session.Physics.RcsThrusting, "KeyDown should not apply thrust while paused");

            session.Renderer.Dispose();
        }
    }
}
