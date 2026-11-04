using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    public class AppIconTests
    {
        const string IconPath = "Assets/Icons/AppIcon.png";
        const int Size = 1024;
        const string BundleId = "io.dev.maze";

        [Test]
        public void IconAsset_IsSquare1024()
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            Assert.IsNotNull(tex, IconPath);
            Assert.AreEqual(Size, tex.width);
            Assert.AreEqual(Size, tex.height);
        }

        [Test]
        public void IconSource_HasNoAlphaChannel()
        {
            var bytes = System.IO.File.ReadAllBytes(IconPath);
            var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.IsTrue(src.LoadImage(bytes));
                foreach (var c in src.GetPixels32())
                    Assert.AreEqual(255, c.a, "app icons must be fully opaque");
            }
            finally { Object.DestroyImmediate(src); }
        }

        [Test]
        public void DefaultIcon_IsTheProjectIcon()
        {
            var icons = PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any);
            Assert.IsNotNull(icons);
            Assert.GreaterOrEqual(icons.Length, 1);
            Assert.AreEqual(IconPath, AssetDatabase.GetAssetPath(icons[0]));
        }

        [Test]
        public void BundleId_IsUnchanged()
        {
            Assert.AreEqual(BundleId, PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS));
        }
    }
}
