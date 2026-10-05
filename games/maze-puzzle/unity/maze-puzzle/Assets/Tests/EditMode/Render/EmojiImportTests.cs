using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Render.Tests
{
    // SPEC 6.3: every Fluent emoji sprite is imported as uncompressed RGBA32 on every platform, no mipmaps,
    // bilinear, alphaIsTransparency. DXT block artefacts otherwise show up on emoji edges.
    public class EmojiImportTests
    {
        const string Dir = "Assets/Resources/Emojis/3D";
        static readonly string[] Platforms = { "Standalone", "iPhone", "Android", "WebGL", "Server" };

        static IEnumerable<string> EmojiPaths()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Dir }))
                yield return AssetDatabase.GUIDToAssetPath(guid);
        }

        [Test]
        public void All57EmojiArePresent()
        {
            int n = 0;
            foreach (var _ in EmojiPaths()) n++;
            Assert.AreEqual(57, n);
        }

        [Test]
        public void ImporterSettingsAreLossless()
        {
            foreach (var path in EmojiPaths())
            {
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.AreEqual(TextureImporterCompression.Uncompressed, imp.textureCompression, path);
                Assert.IsFalse(imp.mipmapEnabled, path + " mipmaps");
                Assert.AreEqual(FilterMode.Bilinear, imp.filterMode, path);
                Assert.IsTrue(imp.alphaIsTransparency, path + " alphaIsTransparency");
                Assert.IsTrue(imp.sRGBTexture, path + " sRGB");
                Assert.IsFalse(imp.crunchedCompression, path + " crunch");

                imp.GetSourceTextureWidthAndHeight(out int w, out int h);
                var def = imp.GetDefaultPlatformTextureSettings();
                Assert.GreaterOrEqual(def.maxTextureSize, System.Math.Max(w, h), path + " default max size would resize the source");
                Assert.AreEqual(TextureImporterCompression.Uncompressed, def.textureCompression, path + " default");

                foreach (var platform in Platforms)
                {
                    var ps = imp.GetPlatformTextureSettings(platform);
                    Assert.IsTrue(ps.overridden, path + " " + platform + " overridden");
                    Assert.AreEqual(TextureImporterFormat.RGBA32, ps.format, path + " " + platform);
                    Assert.AreEqual(TextureImporterCompression.Uncompressed, ps.textureCompression, path + " " + platform);
                    Assert.GreaterOrEqual(ps.maxTextureSize, System.Math.Max(w, h), path + " " + platform + " max size");
                }
            }
        }

        [Test]
        public void LoadedTexturesAreRgba32()
        {
            foreach (var path in EmojiPaths())
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.IsNotNull(sprite, path);
                Assert.AreEqual(TextureFormat.RGBA32, sprite.texture.format, path);
                Assert.AreEqual(1, sprite.texture.mipmapCount, path + " mip count");
            }
        }
    }
}
