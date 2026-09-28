using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitalSalvage.Utils
{
    /// <summary>
    /// Bulletproof sprite and material loader for Orbital Salvage.
    /// Supports Unity Resources, direct PNG disk loading, and crisp procedural fallbacks.
    /// </summary>
    public static class SpriteHelper
    {
        private static readonly Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
        private static Material glowMaterial;
        private static Sprite defaultCircleSprite;

        public static Material GetGlowMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            return mat;
        }

        public static Sprite GetCircleSprite()
        {
            if (defaultCircleSprite != null) return defaultCircleSprite;

            int res = 128;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = res * 0.5f;
            float radius = center - 2f;

            Color[] colors = new Color[res * res];
            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (d <= radius - 2f)
                    {
                        // Slight specular radial gradient
                        float light = Mathf.Clamp01(1f - (d / radius) * 0.4f);
                        colors[y * res + x] = new Color(light, light, light, 1f);
                    }
                    else if (d <= radius)
                    {
                        // Anti-aliased outer rim
                        float alpha = Mathf.Clamp01((radius - d) / 2f);
                        colors[y * res + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        colors[y * res + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            defaultCircleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            return defaultCircleSprite;
        }

        public static Sprite GetEmojiSprite(string emojiName)
        {
            if (string.IsNullOrEmpty(emojiName)) return GetCircleSprite();

            string key = emojiName.StartsWith("emoji_3d_") ? emojiName : $"emoji_3d_{emojiName}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

#if UNITY_EDITOR
            // In Unity Editor, load asset directly from canonical folder
            string assetPath = $"Assets/Sprites/Emojis/3D/{key}.png";
            var editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (editorSprite != null)
            {
                cachedSprites[key] = editorSprite;
                return editorSprite;
            }
#endif

            // Direct file load from canonical Assets/Sprites/Emojis/3D/
            string filePath = Path.Combine(Application.dataPath, "Sprites", "Emojis", "3D", $"{key}.png");
            if (File.Exists(filePath))
            {
                try
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (tex.LoadImage(data))
                    {
                        tex.filterMode = FilterMode.Bilinear;
                        var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                        spr.name = key;
                        cachedSprites[key] = spr;
                        return spr;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SpriteHelper] Failed to load {filePath}: {ex.Message}");
                }
            }

            // Fallback to procedural circle
            return GetCircleSprite();
        }

        public static GameObject CreateEmojiImage(Transform parent, string emojiName, Vector2 size)
        {
            var go = new GameObject($"Emoji_{emojiName}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.sprite = GetEmojiSprite(emojiName);
            img.preserveAspect = true;
            return go;
        }
    }
}
