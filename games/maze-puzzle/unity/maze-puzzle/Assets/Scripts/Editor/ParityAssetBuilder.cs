using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace OrbitalSalvage.Editor
{
    public static class ParityAssetBuilder
    {
        private const string FontsDir = "Assets/Fonts";
        private const string TmpDir = "Assets/Fonts/TMP";
        private const string EmojiDir = "Assets/Resources/Emojis/3D";
        private const string EmojiIndexPath = "Assets/Resources/Emojis/emoji_index.json";
        private const string LicensesDir = "Assets/Licenses";

        private const int SamplingPointSize = 96;
        private const int AtlasPadding = 12;
        private const int AtlasSize = 4096;

        private static readonly string[] FallbackFonts = { "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular" };

        private static readonly string[] PrimaryFonts =
        {
            "Fredoka-400", "Fredoka-600", "Fredoka-700", "Fredoka-800", "Fredoka-900",
            "Nunito-400", "Nunito-700", "Nunito-800", "Nunito-900",
        };

        private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        private static string ParityAssets => Path.GetFullPath(Path.Combine(ProjectRoot, "..", "..", "..", "..", "tools", "parity", "assets"));

        public static void BuildAll()
        {
            try
            {
                SyncFontFiles();
                SyncEmojiFiles();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ConfigureEmojiImporters();
                BuildTmpFontAssets();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[ParityAssetBuilder] BuildAll completed");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ParityAssetBuilder] BuildAll failed: {e}");
                throw;
            }
        }

        private static void CopyIfDifferent(string src, string dst)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dst));
            if (File.Exists(dst))
            {
                var a = File.ReadAllBytes(src);
                var b = File.ReadAllBytes(dst);
                if (a.Length == b.Length && a.AsSpan().SequenceEqual(b)) return;
            }
            File.Copy(src, dst, true);
        }

        private static void SyncFontFiles()
        {
            string srcDir = Path.Combine(ParityAssets, "fonts");
            string dstDir = Path.Combine(ProjectRoot, FontsDir);
            foreach (var name in PrimaryFonts) CopyIfDifferent(Path.Combine(srcDir, name + ".ttf"), Path.Combine(dstDir, name + ".ttf"));
            foreach (var name in FallbackFonts) CopyIfDifferent(Path.Combine(srcDir, name + ".ttf"), Path.Combine(dstDir, name + ".ttf"));
            foreach (var file in Directory.GetFiles(srcDir, "*-OFL.txt"))
            {
                CopyIfDifferent(file, Path.Combine(dstDir, Path.GetFileName(file)));
                CopyIfDifferent(file, Path.Combine(ProjectRoot, LicensesDir, Path.GetFileName(file)));
            }
            CopyIfDifferent(Path.Combine(ParityAssets, "emoji", "Fluent-LICENSE.txt"), Path.Combine(ProjectRoot, LicensesDir, "Fluent-LICENSE.txt"));
        }

        private static void SyncEmojiFiles()
        {
            string srcDir = Path.Combine(ParityAssets, "emoji");
            string dstDir = Path.Combine(ProjectRoot, EmojiDir);
            Directory.CreateDirectory(dstDir);

            var wanted = new HashSet<string>();
            foreach (var file in Directory.GetFiles(srcDir, "*.png"))
            {
                wanted.Add(Path.GetFileName(file));
                CopyIfDifferent(file, Path.Combine(dstDir, Path.GetFileName(file)));
            }

            foreach (var file in Directory.GetFiles(dstDir, "*.png"))
            {
                if (wanted.Contains(Path.GetFileName(file))) continue;
                AssetDatabase.DeleteAsset(EmojiDir + "/" + Path.GetFileName(file));
            }

            CopyIfDifferent(Path.Combine(srcDir, "index.json"), Path.Combine(ProjectRoot, EmojiIndexPath));
        }

        private static void ConfigureEmojiImporters()
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { EmojiDir }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.maxTextureSize = 256;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static void BuildTmpFontAssets()
        {
            Directory.CreateDirectory(Path.Combine(ProjectRoot, TmpDir));

            var fallbacks = new List<TMP_FontAsset>();
            foreach (var name in FallbackFonts) fallbacks.Add(CreateTmpFont(name, null));
            foreach (var name in PrimaryFonts) CreateTmpFont(name, fallbacks);
        }

        private static TMP_FontAsset CreateTmpFont(string fontName, List<TMP_FontAsset> fallbacks)
        {
            string ttfPath = $"{FontsDir}/{fontName}.ttf";
            string assetPath = $"{TmpDir}/{fontName} SDF.asset";

            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null) throw new FileNotFoundException(ttfPath);

            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null) AssetDatabase.DeleteAsset(assetPath);

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                SamplingPointSize,
                AtlasPadding,
                GlyphRenderMode.SDFAA,
                AtlasSize,
                AtlasSize,
                AtlasPopulationMode.Dynamic,
                true);
            if (fontAsset == null) throw new InvalidOperationException($"CreateFontAsset failed for {ttfPath}");

            fontAsset.name = fontName + " SDF";
            if (fallbacks != null) fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>(fallbacks);

            AssetDatabase.CreateAsset(fontAsset, assetPath);
            if (fontAsset.material != null)
            {
                fontAsset.material.name = fontName + " SDF Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }
            if (fontAsset.atlasTextures != null)
            {
                for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                {
                    var tex = fontAsset.atlasTextures[i];
                    if (tex == null) continue;
                    tex.name = fontName + " SDF Atlas" + (i == 0 ? string.Empty : " " + i);
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                }
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssetIfDirty(fontAsset);
            return fontAsset;
        }
    }
}
