using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Render;
using OrbitalSalvage.Viewer.Editor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// Every zero and iphone spec, painted the same way <see cref="LocPseudoStillTests"/> paints qps.
    /// </summary>
    public class LocGermanStillTests
    {
        const string Dir = "/tmp/maze-i18n/de-audit/stills";

        [TearDown]
        public void RestoreEnglish()
        {
            Loc.SetLoader(new ResourcesLocTableLoader());
            Loc.SetLanguage("en");
            Loc.Reset(null);
            OutlineFonts.Clear();
        }

        [Test]
        [Timeout(1500000)]
        public void German_PaintsEveryZeroAndIphonePage()
        {
            Directory.CreateDirectory(Dir);
            Loc.SetLoader(new ResourcesLocTableLoader());
            Assert.IsTrue(Loc.SetLanguage("en"));
            byte[] englishHome = Paint("home_default", "zero");

            Assert.IsTrue(Loc.SetLanguage("de"));
            Assert.AreEqual("de", Loc.CurrentLanguage, "de.json did not load from Resources");
            OutlineFonts.Clear();

            string specDir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            string[] files = Directory.GetFiles(specDir, "*.json");
            Array.Sort(files, StringComparer.Ordinal);
            int painted = 0;
            var names = new List<string>();
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileName(files[i]);
                string profile = null;
                if (name.EndsWith("__zero.json", StringComparison.Ordinal)) profile = "zero";
                else if (name.EndsWith("__iphone.json", StringComparison.Ordinal)) profile = "iphone";
                if (profile == null) continue;
                string id = name.Substring(0, name.Length - ("__" + profile + ".json").Length);
                byte[] png = Paint(id, profile);
                string file = id + "__" + profile + ".png";
                File.WriteAllBytes(Path.Combine(Dir, file), png);
                Assert.Greater(png.Length, 8, file);
                names.Add(file + "\t" + png.Length.ToString());
                painted++;
            }
            Assert.AreEqual(90, painted);
            File.WriteAllLines(Path.Combine(Dir, "index.txt"), names);
            byte[] germanHome = File.ReadAllBytes(Path.Combine(Dir, "home_default__zero.png"));
            Assert.AreNotEqual(Sha(englishHome), Sha(germanHome), "home_default stayed on the English pixels");
        }

        static byte[] Paint(string id, string profile)
        {
            Texture2D tex = null;
            try
            {
                tex = PagePreviewPainter.Render(id, profile, true, 0, 0, out byte[] png);
                Assert.IsNotNull(png, id + " " + profile);
                return png;
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        static string Sha(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                var sb = new System.Text.StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }
    }
}
