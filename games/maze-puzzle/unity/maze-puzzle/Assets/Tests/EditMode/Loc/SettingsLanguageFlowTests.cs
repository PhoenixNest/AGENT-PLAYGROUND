using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.App;

namespace OrbitalSalvage.Loc.Tests
{
    public class SettingsLanguageFlowTests : LocTestBase
    {
        [Test]
        public void Picker_OpensChoosesCloses_AndPolicyRowsOpenThePlaceholderUrl()
        {
            var urls = new List<string>();
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            app.OpenUrl = urls.Add;
            app.ShowPage("settings");
            Assert.AreEqual("settings_default", app.TemplateId);
            Assert.IsFalse(app.ModalOpen("modal-language"));
            Assert.AreEqual("English", Text(app, "settings-language-value"));
            Assert.AreEqual("", app.Economy.Profile.Settings.Language);

            app.Click("btn-open-language");
            Assert.IsTrue(app.ModalOpen("modal-language"));
            Assert.AreEqual("settings_modal_language", app.TemplateId);
            Assert.AreEqual(LanguageMarker.Mark("English"), Text(app, LanguageMarker.ButtonId("en")));
            Assert.IsFalse(app.Model.Elements.ContainsKey(LanguageMarker.ButtonId("ja")));

            app.KeyDown("Escape");
            Assert.IsTrue(app.ModalOpen("modal-language"), "Escape does not close the picker, same as privacy and reset");

            app.Click("btn-close-language");
            Assert.IsFalse(app.ModalOpen("modal-language"));
            Assert.AreEqual("settings_default", app.TemplateId);
            Assert.AreEqual("", app.Economy.Profile.Settings.Language);

            app.Click("btn-open-language");
            app.Click(LanguageMarker.ButtonId("en"));
            Assert.IsFalse(app.ModalOpen("modal-language"));
            Assert.AreEqual("en", app.Economy.Profile.Settings.Language);
            Assert.AreEqual(LanguageRegistry.DefaultCode, Loc.CurrentLanguage);

            app.Click("btn-open-language");
            app.Click(LanguageMarker.ButtonId("ja"));
            Assert.IsFalse(app.ModalOpen("modal-language"));
            Assert.AreEqual("ja", app.Economy.Profile.Settings.Language);
            Assert.AreEqual(LanguageRegistry.DefaultCode, Loc.CurrentLanguage, "missing ja.json keeps English on screen");
            Assert.AreEqual("日本語", Text(app, "settings-language-value"));
            Assert.AreEqual("English", Text(app, LanguageMarker.ButtonId("en")));
            Assert.AreEqual(LanguageMarker.Mark("日本語"), Text(app, LanguageMarker.ButtonId("ja")));

            app.Click("btn-open-language");
            app.Click(LanguageMarker.ButtonId("ja"));
            Assert.IsFalse(app.ModalOpen("modal-language"));
            Assert.AreEqual("ja", app.Economy.Profile.Settings.Language);

            app.Click("btn-open-privacy");
            Assert.IsFalse(app.ModalOpen("modal-privacy"));
            Assert.AreEqual("settings_default", app.TemplateId);
            app.Click("btn-open-user-agreement");
            CollectionAssert.AreEqual(new[] { LegalLinks.PrivacyPolicy, LegalLinks.UserAgreement }, urls);
            Assert.AreEqual("https://www.bing.com", LegalLinks.PrivacyPolicy);
            Assert.AreEqual("https://www.bing.com", LegalLinks.UserAgreement);
        }

        [Test]
        public void LegalLinks_CommentNamesTheParentalGate()
        {
            string source = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "Scripts", "App", "LegalLinks.cs"));
            StringAssert.Contains("Kids Category", source);
            StringAssert.Contains("parental gate", source);
            StringAssert.Contains("https://www.bing.com", source);
        }

        static string Text(AppController app, string id)
        {
            ElementOverride over;
            Assert.IsTrue(app.Model.Elements.TryGetValue(id, out over), id);
            Assert.IsTrue(over.HasText, id);
            return over.Text;
        }
    }
}
