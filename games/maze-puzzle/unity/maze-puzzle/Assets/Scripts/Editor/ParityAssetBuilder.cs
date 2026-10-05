using System;
using System.Collections.Generic;
using System.IO;
using OrbitalSalvage.Render;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace OrbitalSalvage.Editor
{
    public static class ParityAssetBuilder
    {
        private const string FontsDir = "Assets/Fonts";
        // TMP assets live under Resources so OrbitalSalvage.Render.ParityFonts can Resources.Load them.
        private const string TmpDir = "Assets/Resources/Fonts/TMP";
        private const string LegacyTmpDir = "Assets/Fonts/TMP";
        private const string EmojiDir = "Assets/Resources/Emojis/3D";
        private const string EmojiIndexPath = "Assets/Resources/Emojis/emoji_index.json";
        private const string LicensesDir = "Assets/Licenses";

        // Fluent 3D PNGs are 256 px wide; 1fa90 (ringed planet) is 256x264, so 256 would resample it.
        // 512 is the smallest power-of-two cap that never resizes any pinned emoji.
        private const int EmojiMaxTextureSize = 512;
        private static readonly string[] EmojiPlatforms = { "Standalone", "iPhone", "Android", "WebGL", "Server" };

        private const int SamplingPointSize = 96;
        private const int AtlasPadding = 12;
        private const int AtlasSize = 4096;

        private static readonly string[] FallbackFonts = { "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular" };

        // Exactly the faces ParityFonts.FaceWeights lists: Fredoka 400/600/700 and Nunito 700/800/900.
        // These are the faces Google Fonts serves for the web app; other weights resolve by CSS font matching.
        private static string[] PrimaryFonts
        {
            get
            {
                var names = new List<string>();
                foreach (var family in ParityFonts.Families)
                    foreach (int w in ParityFonts.FaceWeights(family)) names.Add(family + "-" + w);
                return names.ToArray();
            }
        }

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
                BuildCanvasMaterials();
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

        /// <summary>
        /// A Player only contains shaders that something references: the Canvas2D shaders are found by name, so a material per shader
        /// in Resources keeps them in every build (Canvas2D.MakeMaterial falls back to these when Shader.Find misses).
        /// </summary>
        public static void BuildCanvasMaterials()
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources", "Canvas2D"));
            foreach (var name in new[] { "Accumulate", "Fill", "Mask", "Image", "Text" })
            {
                string path = "Assets/Resources/Canvas2D/" + name + ".mat";
                var shader = Shader.Find("Hidden/Canvas2D/" + name);
                if (shader == null) throw new InvalidOperationException("Canvas2D shader not found: " + name);
                var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existing != null && existing.shader == shader) continue;
                if (existing != null) AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(new Material(shader) { name = name }, path);
            }
            AssetDatabase.SaveAssets();
        }

        // Batchmode check (-executeMethod ...VerifyFonts): ParityFonts.Resolve must return the SPEC 5 effective face
        // for every family/weight pair, and every face asset must load from Resources.
        public static void VerifyFonts()
        {
            var expected = new (string family, int css, int face)[]
            {
                ("Fredoka", 400, 400), ("Fredoka", 600, 600), ("Fredoka", 700, 700), ("Fredoka", 800, 700), ("Fredoka", 900, 700),
                ("Nunito", 400, 700), ("Nunito", 700, 700), ("Nunito", 800, 800), ("Nunito", 900, 900),
            };
            int failures = 0;
            ParityFonts.ClearCache();
            foreach (var (family, css, face) in expected)
            {
                var asset = ParityFonts.Resolve(family, css);
                string want = ParityFonts.AssetName(family, face);
                if (asset == null || asset.name != want)
                {
                    failures++;
                    Debug.LogError($"[ParityFonts] {family} {css}: expected '{want}', got '{(asset == null ? "null" : asset.name)}'");
                }
                else Debug.Log($"[ParityFonts] {family} {css} -> {asset.name} (fallbacks {asset.fallbackFontAssetTable.Count})");
            }
            if (failures > 0) EditorApplication.Exit(1);
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
            // Outline text rasteriser (OrbitalSalvage.Render.OutlineFonts) loads the same TTFs as TextAssets from Resources.
            string ttfDir = Path.Combine(ProjectRoot, "Assets/Resources/Fonts/Ttf");
            foreach (var name in PrimaryFonts) CopyIfDifferent(Path.Combine(srcDir, name + ".ttf"), Path.Combine(ttfDir, name + ".bytes"));
            foreach (var name in FallbackFonts) CopyIfDifferent(Path.Combine(srcDir, name + ".ttf"), Path.Combine(ttfDir, name + ".bytes"));
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
                    importer.sRGBTexture = true;
                    importer.isReadable = false;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.maxTextureSize = EmojiMaxTextureSize;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.crunchedCompression = false;

                    // The default settings alone are not enough: a platform entry that exists in the .meta (the
                    // old importer left a Standalone entry with textureCompression 1) is applied by that platform.
                    // Every platform therefore gets an explicit, overridden RGBA32 entry.
                    var def = importer.GetDefaultPlatformTextureSettings();
                    def.maxTextureSize = EmojiMaxTextureSize;
                    def.textureCompression = TextureImporterCompression.Uncompressed;
                    def.crunchedCompression = false;
                    importer.SetPlatformTextureSettings(def);
                    foreach (var platform in EmojiPlatforms)
                    {
                        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                        {
                            name = platform,
                            overridden = true,
                            maxTextureSize = EmojiMaxTextureSize,
                            resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                            format = TextureImporterFormat.RGBA32,
                            textureCompression = TextureImporterCompression.Uncompressed,
                            compressionQuality = 50,
                            crunchedCompression = false,
                            allowsAlphaSplitting = false,
                        });
                    }
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
            RemoveObsoleteFontAssets();
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Fonts");
            EnsureFolder(TmpDir);

            var fallbacks = new List<TMP_FontAsset>();
            foreach (var name in FallbackFonts) fallbacks.Add(CreateTmpFont(name, null));
            foreach (var name in PrimaryFonts) CreateTmpFont(name, fallbacks);
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            string parent = Path.GetDirectoryName(assetFolder).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetFolder));
        }

        // Deletes outputs of earlier layouts: the old Assets/Fonts/TMP folder, static font files for weights
        // that Google does not serve (Fredoka 800/900, Nunito 400) and TMP assets no longer in the face set.
        private static void RemoveObsoleteFontAssets()
        {
            if (AssetDatabase.IsValidFolder(LegacyTmpDir)) AssetDatabase.DeleteAsset(LegacyTmpDir);

            var wantedTtf = new HashSet<string>(PrimaryFonts);
            foreach (var guid in AssetDatabase.FindAssets("t:Font", new[] { FontsDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                bool isParityFamily = name.StartsWith("Fredoka-", StringComparison.Ordinal) || name.StartsWith("Nunito-", StringComparison.Ordinal);
                if (isParityFamily && !wantedTtf.Contains(name)) AssetDatabase.DeleteAsset(path);
            }

            if (!AssetDatabase.IsValidFolder(TmpDir)) return;
            var wantedAssets = new HashSet<string>();
            foreach (var name in PrimaryFonts) wantedAssets.Add(name + " SDF");
            foreach (var name in FallbackFonts) wantedAssets.Add(name + " SDF");
            foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { TmpDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!wantedAssets.Contains(Path.GetFileNameWithoutExtension(path))) AssetDatabase.DeleteAsset(path);
            }
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

            MakeBuildStable(fontAsset);
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssetIfDirty(fontAsset);
            return fontAsset;
        }

        // TMP_PreBuildProcessor empties every dynamic font asset that has clearDynamicDataOnBuild set, which rewrites
        // the atlas subasset. Saving the assets already in that emptied state keeps player builds from touching them.
        private static void MakeBuildStable(TMP_FontAsset fontAsset)
        {
            var so = new SerializedObject(fontAsset);
            var flag = so.FindProperty("m_ClearDynamicDataOnBuild");
            if (flag == null) throw new InvalidOperationException("m_ClearDynamicDataOnBuild not found on " + fontAsset.name);
            flag.boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            var clear = typeof(TMP_FontAsset).GetMethod("ClearCharacterAndGlyphTablesInternal",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (clear == null) throw new MissingMethodException(nameof(TMP_FontAsset), "ClearCharacterAndGlyphTablesInternal");
            clear.Invoke(fontAsset, null);
        }
    }
}
