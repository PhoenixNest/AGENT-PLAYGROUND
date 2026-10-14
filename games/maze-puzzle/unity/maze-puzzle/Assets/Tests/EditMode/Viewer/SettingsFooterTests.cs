using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    /// <summary>
    /// The Settings credits footer is two text runs under the Game Data card.
    /// Every committed settings spec (both profiles, including modal states) is the
    /// page the app paints. Other languages and the pseudo locale reuse these specs.
    /// </summary>
    public class SettingsFooterTests
    {
        [Test]
        public void SettingsSpecs_OmitTheCreditsFooter()
        {
            string dir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            string[] files = Directory.GetFiles(dir, "settings_*.json");
            Assert.AreEqual(12, files.Length, "six settings states, zero and iphone");
            foreach (string path in files)
            {
                var root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(path));
                var elements = (List<object>)root["elements"];
                bool reset = false;
                bool privacy = false;
                bool title = false;
                foreach (object item in elements)
                {
                    var el = (Dictionary<string, object>)item;
                    if (!el.TryGetValue("runs", out var runsObj) || !(runsObj is List<object> runs)) continue;
                    foreach (object runItem in runs)
                    {
                        var run = (Dictionary<string, object>)runItem;
                        string text = "";
                        if (run.TryGetValue("s", out var raw) && raw is string s) text = s;
                        StringAssert.DoesNotContain("Pure Gesture", text, path);
                        StringAssert.DoesNotContain("v1.2.0", text, path);
                        StringAssert.DoesNotContain("Space Trash Clean v", text, path);
                        if (text.Contains("Reset Progress")) reset = true;
                        if (text.Contains("Privacy Policy")) privacy = true;
                        if (text.Contains("GAME SETTINGS")) title = true;
                    }
                }
                Assert.IsTrue(reset, path);
                Assert.IsTrue(privacy, path);
                Assert.IsTrue(title, path);
            }
        }
    }
}
