using System.IO;
using NUnit.Framework;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Viewer.Editor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public class LocPseudoStillTests
    {
        static readonly string[] Pages =
        {
            "home_default",
            "modal_pause",
            "settings_default",
            "shop_rings_default",
            "maps_world_1_fresh",
            "modal_debrief_exact",
            "settings_modal_language"
        };

        [TearDown]
        public void ResetLanguage()
        {
            Loc.Reset(null);
        }

        [Test]
        public void PseudoLocale_PaintsSixPages()
        {
            string dir = "/tmp/maze-i18n/loc-render";
            Directory.CreateDirectory(dir);
            Loc.SetLoader(new ResourcesLocTableLoader());
            Assert.IsTrue(Loc.SetLanguage("en"));
            var english = new byte[Pages.Length][];
            for (int i = 0; i < Pages.Length; i++)
                english[i] = Paint(Pages[i]);
            Assert.IsTrue(Loc.SetDiagnostic("qps"));
            for (int i = 0; i < Pages.Length; i++)
            {
                byte[] png = Paint(Pages[i]);
                string path = Path.Combine(dir, Pages[i] + "__qps.png");
                File.WriteAllBytes(path, png);
                Assert.Greater(png.Length, 8, Pages[i]);
                Assert.AreNotEqual(Sha(english[i]), Sha(png), Pages[i] + " stayed on the English pixels");
            }
        }

        static byte[] Paint(string id)
        {
            Texture2D tex = null;
            try
            {
                tex = PagePreviewPainter.Render(id, "iphone", true, 0, 0, out byte[] png);
                Assert.IsNotNull(png, id);
                return png;
            }
            finally
            {
                if (tex != null) Object.DestroyImmediate(tex);
            }
        }

        static string Sha(byte[] data)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                var sb = new System.Text.StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }
    }
}
