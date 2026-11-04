using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace OrbitalSalvage.Editor
{
    /// <summary>
    /// Assigns <see cref="IconPath"/> as the default app icon. Platforms without their own override
    /// (including iOS) generate every required size from this one 1024x1024 opaque source.
    /// The bundle id is not touched. Batch mode exits 0 on success and 1 otherwise.
    /// </summary>
    public static class AppIconSetup
    {
        public const string IconPath = "Assets/Icons/AppIcon.png";
        public const string Menu = "Orbital Salvage/Build/Apply App Icon";
        public const int Size = 1024;

        [MenuItem(Menu, false, 210)]
        public static void ApplyMenu() => Apply();

        public static void ApplyBatch()
        {
            int code = Apply() ? 0 : 1;
            if (Application.isBatchMode) EditorApplication.Exit(code);
        }

        public static bool Apply()
        {
            var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError("[AppIcon] missing " + IconPath);
                return false;
            }
            ConfigureImporter(importer);
            importer.SaveAndReimport();

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (tex == null || tex.width != Size || tex.height != Size)
            {
                Debug.LogError("[AppIcon] " + IconPath + " must be " + Size + "x" + Size);
                return false;
            }
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { tex }, IconKind.Any);
            AssetDatabase.SaveAssets();
            Debug.Log("[AppIcon] default icon set to " + IconPath);
            return true;
        }

        public static void ConfigureImporter(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = false;
            importer.maxTextureSize = Size;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = true;
        }
    }
}
