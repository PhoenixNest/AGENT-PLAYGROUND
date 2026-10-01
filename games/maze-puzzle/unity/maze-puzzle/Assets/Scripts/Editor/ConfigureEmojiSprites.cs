using System.IO;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Editor
{
    public static class ConfigureEmojiSprites
    {
        public static void ReimportAllEmojisAsSprites()
        {
            string emojiFolder = "Assets/Resources/Emojis/3D";
            if (!Directory.Exists(emojiFolder)) return;

            int count = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                string[] pngFiles = Directory.GetFiles(emojiFolder, "*.png");
                foreach (var file in pngFiles)
                {
                    string assetPath = file.Replace('\\', '/');
                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer != null)
                    {
                        bool needsReimport = false;
                        if (importer.textureType != TextureImporterType.Sprite)
                        {
                            importer.textureType = TextureImporterType.Sprite;
                            needsReimport = true;
                        }
                        if (importer.spriteImportMode != SpriteImportMode.Single)
                        {
                            importer.spriteImportMode = SpriteImportMode.Single;
                            needsReimport = true;
                        }
                        if (!importer.alphaIsTransparency)
                        {
                            importer.alphaIsTransparency = true;
                            needsReimport = true;
                        }

                        if (needsReimport)
                        {
                            importer.SaveAndReimport();
                            count++;
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            Debug.Log($"[ConfigureEmojiSprites] Successfully configured and re-imported {count} emoji textures as 2D Sprites!");
        }
    }
}
