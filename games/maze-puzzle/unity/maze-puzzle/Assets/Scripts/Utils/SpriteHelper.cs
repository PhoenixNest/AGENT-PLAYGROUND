using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitalSalvage.Utils
{
    /// <summary>
    /// Bulletproof sprite and material loader for Orbital Salvage.
    /// Provides 1:1 Fall Guys / Brawl Stars style 3D cartoon UI sprites with smooth anti-aliased
    /// rounded corners, 3D extruded bevels, drop shadows, and 9-slice sliced scaling.
    /// </summary>
    public static class SpriteHelper
    {
        private static readonly Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
        private static Sprite defaultCircleSprite;

        // Canonical Web Prototype Color Palette
        public static readonly Color ColorBorderDark = new Color32(0x1e, 0x1b, 0x4b, 0xff); // #1e1b4b
        public static readonly Color ColorCardBg = new Color32(0x31, 0x2e, 0x81, 0xee);     // #312e81
        public static readonly Color ColorCardOutline = new Color32(0x63, 0x66, 0xf1, 0x88); // rgba(99,102,241,0.53)
        public static readonly Color ColorYellowMain = new Color32(0xfa, 0xcc, 0x15, 0xff); // #facc15
        public static readonly Color ColorYellowShadow = new Color32(0xca, 0x8a, 0x04, 0xff); // #ca8a04
        public static readonly Color ColorGreenMain = new Color32(0x4a, 0xde, 0x80, 0xff);  // #4ade80
        public static readonly Color ColorGreenBottom = new Color32(0x16, 0xa3, 0x4a, 0xff); // #16a34a
        public static readonly Color ColorGreenShadow = new Color32(0x0d, 0x7a, 0x2e, 0xff); // #0d7a2e
        public static readonly Color ColorCyanMain = new Color32(0x38, 0xbd, 0xf8, 0xff);   // #38bdf8
        public static readonly Color ColorPinkMain = new Color32(0xf4, 0x72, 0xb6, 0xff);   // #f472b6
        public static readonly Color ColorBronze = new Color32(0xf5, 0x9e, 0x0b, 0xff);     // #f59e0b
        public static readonly Color ColorSilver = new Color32(0x38, 0xbd, 0xf8, 0xff);     // #38bdf8
        public static readonly Color ColorGold = new Color32(0xfa, 0xcc, 0x15, 0xff);       // #facc15

        public static Material GetGlowMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            return mat;
        }

        public static Material GetDashedLineMaterial(Color color)
        {
            int w = 64; int h = 8;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isDash = (x % 32) < 20;
                    cols[y * w + x] = isDash ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(cols);
            tex.Apply();

            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = tex;
            mat.color = color;
            return mat;
        }

        #region Standard Circle & Emojis
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
                        colors[y * res + x] = Color.white;
                    }
                    else if (d <= radius)
                    {
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

        public static Sprite GetCircleBadge(Color fillColor, Color borderColor, float borderWidth = 2f)
        {
            string key = $"CircleBadge_{fillColor.GetHashCode()}_{borderColor.GetHashCode()}_{borderWidth}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float center = res * 0.5f;
            float radius = center - 2f;

            Color[] colors = new Color[res * res];
            for (int y = 0; y < res; y++)
            {
                float py = y - center + 0.5f;
                for (int x = 0; x < res; x++)
                {
                    float px = x - center + 0.5f;
                    float d = Mathf.Sqrt(px * px + py * py);
                    if (d <= radius - borderWidth - 1f)
                    {
                        colors[y * res + x] = fillColor;
                    }
                    else if (d <= radius - borderWidth)
                    {
                        float t = Mathf.Clamp01((radius - borderWidth) - d);
                        colors[y * res + x] = Color.Lerp(borderColor, fillColor, t);
                    }
                    else if (d <= radius - 1f)
                    {
                        colors[y * res + x] = borderColor;
                    }
                    else if (d <= radius)
                    {
                        float alpha = Mathf.Clamp01(radius - d);
                        Color c = borderColor;
                        c.a *= alpha;
                        colors[y * res + x] = c;
                    }
                    else
                    {
                        colors[y * res + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        public static Sprite GetCrossMarkSprite()
        {
            string key = "CrossMark";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int res = 64;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[res * res];
            float half = res * 0.5f;
            float thickness = 4.0f;
            float lineLen = half * 0.55f;

            for (int y = 0; y < res; y++)
            {
                float py = y - half + 0.5f;
                for (int x = 0; x < res; x++)
                {
                    float px = x - half + 0.5f;
                    float d1 = Mathf.Abs(px - py) * 0.70710678f;
                    float l1 = Mathf.Abs(px + py) * 0.70710678f;
                    float d2 = Mathf.Abs(px + py) * 0.70710678f;
                    float l2 = Mathf.Abs(px - py) * 0.70710678f;

                    float dist1 = Mathf.Max(d1 - thickness, l1 - lineLen);
                    float dist2 = Mathf.Max(d2 - thickness, l2 - lineLen);
                    float dist = Mathf.Min(dist1, dist2);

                    float alpha = Mathf.Clamp01(0.5f - dist);
                    cols[y * res + x] = new Color(0.12f, 0.11f, 0.29f, alpha);
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        public static Sprite GetRoundedSquareSprite(Color fillColor, Color borderColor)
        {
            string key = $"RoundedSquare_{fillColor.GetHashCode()}_{borderColor.GetHashCode()}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            float half = size * 0.5f;
            float r = 26f;
            float cornerR = 8f;

            for (int y = 0; y < size; y++)
            {
                float py = Mathf.Abs(y - half + 0.5f);
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs(x - half + 0.5f);
                    float qx = Mathf.Max(0f, px - (r - cornerR));
                    float qy = Mathf.Max(0f, py - (r - cornerR));
                    float d = Mathf.Sqrt(qx * qx + qy * qy) - cornerR;
                    float alpha = Mathf.Clamp01(0.5f - d);
                    Color c = Color.clear;
                    if (alpha > 0f)
                    {
                        Color baseCol = d >= -3.5f ? borderColor : fillColor;
                        c = new Color(baseCol.r, baseCol.g, baseCol.b, alpha * baseCol.a);
                    }
                    cols[y * size + x] = c;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        private static readonly Dictionary<string, string> EmojiCodepointAlias = new Dictionary<string, string>
        {
            { "emoji_3d_booster", "23e9" },
            { "emoji_3d_bowl", "1f963" },
            { "emoji_3d_broom", "1f9f9" },
            { "emoji_3d_calendar", "1f4c5" },
            { "emoji_3d_candy", "1f36c" },
            { "emoji_3d_cheese", "1f9c0" },
            { "emoji_3d_coin", "1fa99" },
            { "emoji_3d_crown", "1f451" },
            { "emoji_3d_donut", "1f369" },
            { "emoji_3d_duck", "1f986" },
            { "emoji_3d_explosion", "1f4a5" },
            { "emoji_3d_gear", "2699" },
            { "emoji_3d_gift", "1f381" },
            { "emoji_3d_hole", "1f573" },
            { "emoji_3d_home", "1f3e0" },
            { "emoji_3d_kitty", "1f431" },
            { "emoji_3d_lightning", "26a1" },
            { "emoji_3d_locked", "1f512" },
            { "emoji_3d_magnet", "1f9f2" },
            { "emoji_3d_map", "1f5fa" },
            { "emoji_3d_milky_way", "1f30c" },
            { "emoji_3d_nom_nom", "1f60b" },
            { "emoji_3d_party_popper", "1f389" },
            { "emoji_3d_pause", "23f8" },
            { "emoji_3d_pleading_face", "1f97a" },
            { "emoji_3d_retry", "1f501" },
            { "emoji_3d_rocket", "1f680" },
            { "emoji_3d_sound_off", "1f507" },
            { "emoji_3d_sound_on", "1f50a" },
            { "emoji_3d_sparkles", "2728" },
            { "emoji_3d_star", "2b50" },
            { "emoji_3d_trophy", "1f3c6" },
        };

        public static Sprite GetEmojiSprite(string emojiName)
        {
            if (string.IsNullOrEmpty(emojiName)) return GetCircleSprite();
            if (emojiName.Contains("cross") || emojiName.Contains("close")) return GetCrossMarkSprite();

            string key = emojiName.StartsWith("emoji_3d_") ? emojiName : $"emoji_3d_{emojiName}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            string resourceName = EmojiCodepointAlias.TryGetValue(key, out var codepoint) ? codepoint : key;
            var resourceSprite = Resources.Load<Sprite>($"Emojis/3D/{resourceName}");
            if (resourceSprite != null)
            {
                cachedSprites[key] = resourceSprite;
                return resourceSprite;
            }

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
        #endregion

        #region SDF Procedural 9-Slice UI Sprites
        private static float RoundedBoxSDF(float x, float y, float w, float h, float r)
        {
            float qx = Mathf.Abs(x) - w + r;
            float qy = Mathf.Abs(y) - h + r;
            return Mathf.Min(Mathf.Max(qx, qy), 0.0f) + Mathf.Sqrt(Mathf.Max(qx, 0.0f) * Mathf.Max(qx, 0.0f) + Mathf.Max(qy, 0.0f) * Mathf.Max(qy, 0.0f)) - r;
        }

        /// <summary>
        /// Generates an antialiased 9-slice rounded rectangle card with border.
        /// </summary>
        public static Sprite Get9SliceCard(Color fillColor, Color borderColor, float borderWidth = 2.5f, float radius = 22f)
        {
            string key = $"Card_{fillColor.GetHashCode()}_{borderColor.GetHashCode()}_{borderWidth}_{radius}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 96;
            float half = size * 0.5f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                float py = y - half + 0.5f;
                for (int x = 0; x < size; x++)
                {
                    float px = x - half + 0.5f;
                    float dist = RoundedBoxSDF(px, py, half - 2f, half - 2f, radius);

                    float outerAlpha = Mathf.Clamp01(0.5f - dist);
                    if (outerAlpha > 0f)
                    {
                        float innerAlpha = Mathf.Clamp01(0.5f - (dist + borderWidth));
                        Color c = Color.Lerp(borderColor, fillColor, innerAlpha);
                        c.a *= outerAlpha;
                        cols[y * size + x] = c;
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius + 4, radius + 4, radius + 4, radius + 4));
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates a 3D extruded cartoon button with top gradient, dark outline, and bottom bevel shadow.
        /// Matches the 1:1 Fall Guys / Brawl Stars style in the Web Prototype.
        /// </summary>
        public static Sprite Get9SliceButton3D(Color topColor, Color bottomColor, Color shadowColor, Color borderColor, float bevelHeight = 6f, float radius = 18f)
        {
            string key = $"Btn3D_{topColor.GetHashCode()}_{bottomColor.GetHashCode()}_{shadowColor.GetHashCode()}_{bevelHeight}_{radius}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int width = 96;
            int height = 96;
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[width * height];

            float borderWidth = 3f;
            float faceHalfH = halfH - bevelHeight * 0.5f - 2f;
            float faceCenterY = bevelHeight * 0.5f;

            for (int y = 0; y < height; y++)
            {
                float py = y - halfH + 0.5f;
                for (int x = 0; x < width; x++)
                {
                    float px = x - halfW + 0.5f;

                    // Shadow box (occupies bottom)
                    float distShadow = RoundedBoxSDF(px, py + bevelHeight * 0.4f, halfW - 2f, halfH - 2f, radius);
                    float alphaShadow = Mathf.Clamp01(0.5f - distShadow);

                    Color shadowCol = Color.clear;
                    if (alphaShadow > 0f)
                    {
                        float innerShadow = Mathf.Clamp01(0.5f - (distShadow + borderWidth));
                        shadowCol = Color.Lerp(borderColor, shadowColor, innerShadow);
                        shadowCol.a *= alphaShadow;
                    }

                    // Face box (top face)
                    float distFace = RoundedBoxSDF(px, py - faceCenterY, halfW - 2f, faceHalfH, radius);
                    float alphaFace = Mathf.Clamp01(0.5f - distFace);

                    Color faceCol = Color.clear;
                    if (alphaFace > 0f)
                    {
                        float innerFace = Mathf.Clamp01(0.5f - (distFace + borderWidth));
                        float tGrad = Mathf.Clamp01((py - (faceCenterY - faceHalfH)) / (faceHalfH * 2f));
                        Color grad = Color.Lerp(bottomColor, topColor, tGrad);
                        faceCol = Color.Lerp(borderColor, grad, innerFace);
                        faceCol.a *= alphaFace;
                    }

                    // Composite face over shadow
                    Color finalCol;
                    if (alphaFace >= 1f)
                    {
                        finalCol = faceCol;
                    }
                    else if (alphaFace > 0f)
                    {
                        finalCol = Color.Lerp(shadowCol, faceCol, alphaFace);
                    }
                    else
                    {
                        finalCol = shadowCol;
                    }

                    cols[y * width + x] = finalCol;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius + 4, radius + bevelHeight + 4, radius + 4, radius + 4));
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates an antialiased circular white button with 3D drop shadow (e.g. Pause, Restart, Sound, Settings).
        /// </summary>
        public static Sprite GetWhiteCircleButton3D(int diameter = 64)
        {
            string key = $"CircleBtn3D_{diameter}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            var tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[diameter * diameter];

            float half = diameter * 0.5f;
            float radius = half - 5f;
            float borderWidth = 2.8f;
            float shadowOffsetY = -3.5f;
            float faceOffsetY = 1.5f;

            for (int y = 0; y < diameter; y++)
            {
                float py = y - half + 0.5f;
                for (int x = 0; x < diameter; x++)
                {
                    float px = x - half + 0.5f;

                    // Shadow circle
                    float dShadow = Vector2.Distance(new Vector2(px, py), new Vector2(0f, shadowOffsetY));
                    float alphaShadow = Mathf.Clamp01(radius - dShadow + 0.5f);
                    Color sCol = ColorBorderDark;
                    sCol.a = alphaShadow;

                    // Face circle
                    float dFace = Vector2.Distance(new Vector2(px, py), new Vector2(0f, faceOffsetY));
                    float alphaFace = Mathf.Clamp01(radius - dFace + 0.5f);
                    Color fCol = Color.clear;
                    if (alphaFace > 0f)
                    {
                        float innerFace = Mathf.Clamp01(radius - borderWidth - dFace + 0.5f);
                        fCol = Color.Lerp(ColorBorderDark, Color.white, innerFace);
                        fCol.a *= alphaFace;
                    }

                    Color finalCol = alphaFace >= 1f ? fCol : (alphaFace > 0f ? Color.Lerp(sCol, fCol, alphaFace) : sCol);
                    cols[y * diameter + x] = finalCol;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates a pill capsule badge (circular ends) with border.
        /// </summary>
        public static Sprite GetPillBadge(Color fillColor, Color borderColor, float borderWidth = 1.8f)
        {
            string key = $"Pill_{fillColor.GetHashCode()}_{borderColor.GetHashCode()}_{borderWidth}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 64;
            int h = 48;
            float halfW = w * 0.5f;
            float halfH = h * 0.5f;
            float radius = halfH - 2f;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                float py = y - halfH + 0.5f;
                for (int x = 0; x < w; x++)
                {
                    float px = x - halfW + 0.5f;
                    float dist = RoundedBoxSDF(px, py, halfW - 2f, halfH - 2f, radius);

                    float outerAlpha = Mathf.Clamp01(0.5f - dist);
                    if (outerAlpha > 0f)
                    {
                        float innerAlpha = Mathf.Clamp01(0.5f - (dist + borderWidth));
                        Color c = Color.Lerp(borderColor, fillColor, innerAlpha);
                        c.a *= outerAlpha;
                        cols[y * w + x] = c;
                    }
                    else
                    {
                        cols[y * w + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius + 2, radius, radius + 2, radius));
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates a smooth candy gradient pill sprite (e.g. yellow to green for loading/progress bars).
        /// </summary>
        public static Sprite GetGradientPill(Color leftColor, Color rightColor)
        {
            string key = $"GradientPill_{leftColor}_{rightColor}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 128;
            int h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];

            float halfW = w * 0.5f;
            float halfH = h * 0.5f;
            float radius = halfH - 2f;

            for (int y = 0; y < h; y++)
            {
                float py = y - halfH + 0.5f;
                for (int x = 0; x < w; x++)
                {
                    float px = x - halfW + 0.5f;
                    float dist = RoundedBoxSDF(px, py, halfW - 2f, halfH - 2f, radius);
                    float alpha = Mathf.Clamp01(0.5f - dist);
                    float t = Mathf.Clamp01((float)x / (w - 1));
                    Color c = Color.Lerp(leftColor, rightColor, t);
                    c.a *= alpha;
                    cols[y * w + x] = c;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(16, 6, 16, 6));
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates an iOS-style rounded pill toggle switch sprite (ON / OFF).
        /// </summary>
        public static Sprite GetToggleSwitch(bool isOn)
        {
            string key = $"ToggleSwitch_{isOn}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 76;
            int h = 40;
            float halfW = w * 0.5f;
            float halfH = h * 0.5f;
            float radius = halfH - 2f;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];

            Color trackColor = isOn ? ColorGreenMain : new Color32(0x33, 0x41, 0x55, 0xff);
            float knobCenterX = isOn ? (halfW - radius - 1f) : (-halfW + radius + 1f);
            float knobRadius = radius - 3f;

            for (int y = 0; y < h; y++)
            {
                float py = y - halfH + 0.5f;
                for (int x = 0; x < w; x++)
                {
                    float px = x - halfW + 0.5f;

                    // Track
                    float distTrack = RoundedBoxSDF(px, py, halfW - 2f, halfH - 2f, radius);
                    float alphaTrack = Mathf.Clamp01(0.5f - distTrack);

                    Color trackCol = Color.clear;
                    if (alphaTrack > 0f)
                    {
                        float innerTrack = Mathf.Clamp01(0.5f - (distTrack + 2f));
                        trackCol = Color.Lerp(ColorBorderDark, trackColor, innerTrack);
                        trackCol.a *= alphaTrack;
                    }

                    // Knob
                    float distKnob = Vector2.Distance(new Vector2(px, py), new Vector2(knobCenterX, 0f));
                    float alphaKnob = Mathf.Clamp01(knobRadius - distKnob + 0.5f);
                    Color knobCol = Color.clear;
                    if (alphaKnob > 0f)
                    {
                        float innerKnob = Mathf.Clamp01(knobRadius - 1.5f - distKnob + 0.5f);
                        knobCol = Color.Lerp(ColorBorderDark, Color.white, innerKnob);
                        knobCol.a *= alphaKnob;
                    }

                    Color finalCol = alphaKnob >= 1f ? knobCol : (alphaKnob > 0f ? Color.Lerp(trackCol, knobCol, alphaKnob) : trackCol);
                    cols[y * w + x] = finalCol;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates a slider track sprite.
        /// </summary>
        public static Sprite GetSliderTrack()
        {
            return GetPillBadge(new Color32(0x1e, 0x1b, 0x4b, 0xbb), new Color32(0x43, 0x38, 0xca, 0x88), 1.5f);
        }

        /// <summary>
        /// Generates a 3D yellow slider thumb/knob.
        /// </summary>
        public static Sprite GetSliderKnob()
        {
            string key = "SliderKnob";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int d = 36;
            float half = d * 0.5f;
            float r = half - 3f;
            var tex = new Texture2D(d, d, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[d * d];

            for (int y = 0; y < d; y++)
            {
                float py = y - half + 0.5f;
                for (int x = 0; x < d; x++)
                {
                    float px = x - half + 0.5f;
                    float dist = Vector2.Distance(new Vector2(px, py), Vector2.zero);
                    float alpha = Mathf.Clamp01(r - dist + 0.5f);
                    Color col = Color.clear;
                    if (alpha > 0f)
                    {
                        float inner = Mathf.Clamp01(r - 2f - dist + 0.5f);
                        col = Color.Lerp(ColorBorderDark, ColorYellowMain, inner);
                        col.a *= alpha;
                    }
                    cols[y * d + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, d, d), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates an antialiased 3D extruded cartoon pill button (border-radius: 999px) with bottom shadow lip.
        /// Matches the 1:1 Fall Guys / Brawl Stars style in the Web Prototype modals and headers.
        /// </summary>
        public static Sprite GetPillButton3D(Color topColor, Color bottomColor, Color shadowColor, Color borderColor, float bevelHeight = 4.5f)
        {
            string key = $"PillBtn3D_{topColor.GetHashCode()}_{bottomColor.GetHashCode()}_{shadowColor.GetHashCode()}_{borderColor.GetHashCode()}_{bevelHeight}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int width = 96;
            int height = 96;
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[width * height];

            float borderWidth = 2.8f;
            float faceHalfH = halfH - bevelHeight * 0.5f - 2f;
            float faceCenterY = bevelHeight * 0.5f;
            float pillRadius = faceHalfH;

            for (int y = 0; y < height; y++)
            {
                float py = y - halfH + 0.5f;
                for (int x = 0; x < width; x++)
                {
                    float px = x - halfW + 0.5f;

                    // Shadow box (occupies bottom)
                    float distShadow = RoundedBoxSDF(px, py + bevelHeight * 0.4f, halfW - 2f, halfH - 2f, pillRadius);
                    float alphaShadow = Mathf.Clamp01(0.5f - distShadow);

                    Color shadowCol = Color.clear;
                    if (alphaShadow > 0f)
                    {
                        float innerShadow = Mathf.Clamp01(0.5f - (distShadow + borderWidth));
                        shadowCol = Color.Lerp(borderColor, shadowColor, innerShadow);
                        shadowCol.a *= alphaShadow;
                    }

                    // Face box (top face)
                    float distFace = RoundedBoxSDF(px, py - faceCenterY, halfW - 2f, faceHalfH, pillRadius);
                    float alphaFace = Mathf.Clamp01(0.5f - distFace);

                    Color faceCol = Color.clear;
                    if (alphaFace > 0f)
                    {
                        float innerFace = Mathf.Clamp01(0.5f - (distFace + borderWidth));
                        float tGrad = Mathf.Clamp01((py - (faceCenterY - faceHalfH)) / (faceHalfH * 2f));
                        Color grad = Color.Lerp(bottomColor, topColor, tGrad);
                        faceCol = Color.Lerp(borderColor, grad, innerFace);
                        faceCol.a *= alphaFace;
                    }

                    Color finalCol = alphaFace >= 1f ? faceCol : (alphaFace > 0f ? Color.Lerp(shadowCol, faceCol, alphaFace) : shadowCol);
                    cols[y * width + x] = finalCol;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(pillRadius + 2, pillRadius + bevelHeight + 2, pillRadius + 2, pillRadius + 2));
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates the 3D holographic pedestal for the home mascot stage (pedestal-disc-base + pedestal-disc-rim).
        /// Matches the 1:1 Web prototype diorama: dark indigo base with purple border and inner glowing cyan dashed rim.
        /// </summary>
        public static Sprite GetPedestalRings()
        {
            string key = "HolographicPedestalV3";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 260;
            int h = 76;
            float halfW = w * 0.5f;
            float halfH = h * 0.5f;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];

            // Outer base disc ellipse radius
            float baseRx = halfW - 6f;
            float baseRy = halfH * 0.65f;
            float baseCenterY = -halfH * 0.05f;

            // Inner rim ellipse radius
            float rimRx = baseRx * 0.76f;
            float rimRy = baseRy * 0.76f;
            float rimCenterY = baseCenterY + 2f;

            Color baseBorderCol = new Color32(0x63, 0x66, 0xf1, 0xff); // #6366f1
            Color baseTopCol = new Color32(0x37, 0x30, 0xa3, 0xff);    // #3730a3
            Color baseBotCol = new Color32(0x0f, 0x0c, 0x29, 0xff);    // #0f0c29

            for (int y = 0; y < h; y++)
            {
                float py = y - halfH + 0.5f;
                for (int x = 0; x < w; x++)
                {
                    float px = x - halfW + 0.5f;

                    // 1. Soft dark shadow underneath
                    float shadowDy = py - (baseCenterY - 4f);
                    float dShadowEllip = Mathf.Sqrt((px * px) / (baseRx * baseRx * 1.08f) + (shadowDy * shadowDy) / (baseRy * baseRy * 1.08f));
                    float alphaShadow = Mathf.Clamp01(1f - dShadowEllip) * 0.6f;
                    Color pix = new Color(0f, 0f, 0f, alphaShadow);

                    // 2. Base disc
                    float baseDy = py - baseCenterY;
                    float dBase = Mathf.Sqrt((px * px) / (baseRx * baseRx) + (baseDy * baseDy) / (baseRy * baseRy));
                    if (dBase <= 1.05f)
                    {
                        float alphaBase = Mathf.Clamp01((1.05f - dBase) * 20f);
                        float tGrad = Mathf.Clamp01((baseDy + baseRy) / (baseRy * 2f));
                        Color faceGrad = Color.Lerp(baseBotCol, baseTopCol, tGrad);

                        float borderDist = (1.05f - dBase) * baseRy;
                        Color discCol = borderDist <= 3.2f ? Color.Lerp(baseBorderCol, faceGrad, borderDist / 3.2f) : faceGrad;
                        discCol.a = alphaBase;

                        // Composite over shadow
                        pix = Color.Lerp(pix, discCol, alphaBase);
                    }

                    // 3. Inner cyan dashed rim
                    float rimDy = py - rimCenterY;
                    float dRim = Mathf.Sqrt((px * px) / (rimRx * rimRx) + (rimDy * rimRy) / (rimRy * rimRy));
                    if (dRim <= 1.0f)
                    {
                        float rimEdge = Mathf.Abs(dRim - 0.92f) * rimRy;
                        float angle = Mathf.Atan2(rimDy, px);
                        bool isDash = Mathf.Sin(angle * 14f) > -0.2f;

                        if (rimEdge <= 2.5f && isDash)
                        {
                            float a = Mathf.Clamp01(2.5f - rimEdge);
                            Color cDash = new Color(0.22f, 0.74f, 0.97f, a * 0.95f);
                            pix = Color.Lerp(pix, cDash, a * 0.95f);
                        }
                        else
                        {
                            // Inner subtle glow
                            float glow = Mathf.Clamp01(1f - dRim) * 0.35f;
                            Color cGlow = new Color(0.22f, 0.74f, 0.97f, glow);
                            pix = Color.Lerp(pix, cGlow, glow);
                        }
                    }

                    cols[y * w + x] = pix;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates a crisp, antialiased right-pointing play triangle sprite (►).
        /// </summary>
        public static Sprite GetPlayTriangleSprite()
        {
            string key = "PlayTriangleSprite";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            float half = size * 0.5f;
            float left = half - 12f;
            float right = half + 15f;
            float top = half + 16f;
            float bot = half - 16f;

            for (int y = 0; y < size; y++)
            {
                float py = y + 0.5f;
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    if (px >= left && px <= right)
                    {
                        float t = (px - left) / (right - left);
                        float halfH = (top - bot) * 0.5f * (1f - t);
                        float midY = half;
                        float distY = Mathf.Abs(py - midY);
                        float alpha = Mathf.Clamp01(halfH - distY + 0.5f);
                        cols[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates the canonical cute cartoon face for Nibbles the Black Hole.
        /// Features chubby dark body with midnight outline, blushing pink cheeks,
        /// big anime eyes with upward tracking pupils, dual shine highlights,
        /// and cute smiling mouth with two white teeth and tongue.
        /// </summary>
        public static Sprite GetNibblesMascotSprite()
        {
            string key = "NibblesMascotFaceV4";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            float center = size * 0.5f;
            float bodyRadius = 52f;
            float outlineThickness = 4.5f;

            Color bodyFill = new Color32(0x0f, 0x17, 0x2a, 0xff); // #0f172a
            Color outlineCol = new Color32(0x1e, 0x1b, 0x4b, 0xff); // #1e1b4b
            Color blushCol = new Color32(0xf4, 0x72, 0xb6, 0xdd); // #f472b6
            Color whiteCol = Color.white;
            Color tongueCol = new Color32(0xfb, 0x71, 0x85, 0xff); // #fb7185

            Vector2 leftCheek = new Vector2(center - 24f, center - 5f);
            Vector2 rightCheek = new Vector2(center + 24f, center - 5f);
            float cheekRadius = 7.5f;

            // Two big expressive anime eyes flanking the center with a narrow 3-pixel gap
            Vector2 leftEye = new Vector2(center - 15f, center + 12f);
            Vector2 rightEye = new Vector2(center + 15f, center + 12f);
            float eyeRadius = 14.5f;
            Vector2 pupilOffset = new Vector2(0f, 2.5f); // Looking upwards towards falling debris!
            float pupilRadius = 8.5f;

            Vector2 mouthCenter = new Vector2(center, center - 9.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pt = new Vector2(x + 0.5f, y + 0.5f);
                    float dBody = Vector2.Distance(pt, new Vector2(center, center));
                    float alphaBody = Mathf.Clamp01(bodyRadius - dBody + 0.5f);

                    if (alphaBody <= 0f)
                    {
                        cols[y * size + x] = Color.clear;
                        continue;
                    }

                    Color col = dBody >= (bodyRadius - outlineThickness) ? outlineCol : bodyFill;

                    // Cheeks
                    float dLCheek = Vector2.Distance(pt, leftCheek);
                    float dRCheek = Vector2.Distance(pt, rightCheek);
                    if (dLCheek <= cheekRadius)
                    {
                        float a = Mathf.Clamp01(cheekRadius - dLCheek + 0.5f) * 0.75f;
                        col = Color.Lerp(col, blushCol, a);
                    }
                    else if (dRCheek <= cheekRadius)
                    {
                        float a = Mathf.Clamp01(cheekRadius - dRCheek + 0.5f) * 0.75f;
                        col = Color.Lerp(col, blushCol, a);
                    }

                    // Cute Smile Mouth with two white teeth and tongue (1:1 Web: 09_gameplay_sector_1.png)
                    float mx = pt.x - mouthCenter.x;
                    float my = pt.y - mouthCenter.y;
                    float mw = 9f;
                    float mh = 5.5f;
                    if (Mathf.Abs(mx) <= mw + 1f && my >= -mh - 1f && my <= mh + 1f)
                    {
                        float cornerDist = Mathf.Sqrt(Mathf.Max(0f, Mathf.Abs(mx) - (mw - 3.5f)) * Mathf.Max(0f, Mathf.Abs(mx) - (mw - 3.5f)) +
                                                      Mathf.Max(0f, Mathf.Abs(my) - (mh - 2.5f)) * Mathf.Max(0f, Mathf.Abs(my) - (mh - 2.5f)));
                        float mouthInside = 3.5f - cornerDist;
                        if (mouthInside > 0f)
                        {
                            float aM = Mathf.Clamp01(mouthInside);
                            Color mCol = outlineCol;

                            // White outline border
                            if (mouthInside <= 2.2f || my >= mh - 1.5f)
                            {
                                mCol = whiteCol;
                            }
                            // Two cute white teeth in upper center
                            else if (Mathf.Abs(mx) <= 4.5f && my >= mh - 3.5f)
                            {
                                mCol = whiteCol;
                            }
                            // Cute pink tongue at bottom
                            else if (my <= -mh + 3.0f && Mathf.Abs(mx) <= 5.5f)
                            {
                                mCol = tongueCol;
                            }

                            col = Color.Lerp(col, mCol, aM);
                        }
                    }

                    // Big Anime Eyes (Eyeball -> Outline -> Pupil -> Dual Sparkle Highlights)
                    for (int eyeIdx = 0; eyeIdx < 2; eyeIdx++)
                    {
                        Vector2 eCenter = eyeIdx == 0 ? leftEye : rightEye;
                        float dEye = Vector2.Distance(pt, eCenter);
                        float aEye = Mathf.Clamp01(eyeRadius - dEye + 0.5f);

                        if (aEye > 0f)
                        {
                            Color eCol = dEye >= (eyeRadius - 2.2f) ? outlineCol : whiteCol;

                            Vector2 pCenter = eCenter + pupilOffset;
                            float dPupil = Vector2.Distance(pt, pCenter);
                            float aPupil = Mathf.Clamp01(pupilRadius - dPupil + 0.5f);

                            if (aPupil > 0f)
                            {
                                Color pupilCol = outlineCol;

                                // Large top-left anime sparkle shine
                                Vector2 shine1 = pCenter + new Vector2(-2.5f, 2.5f);
                                float dShine1 = Vector2.Distance(pt, shine1);
                                if (dShine1 <= 2.8f)
                                {
                                    pupilCol = whiteCol;
                                }

                                // Secondary bottom-right anime sparkle shine
                                Vector2 shine2 = pCenter + new Vector2(2.4f, -1.8f);
                                float dShine2 = Vector2.Distance(pt, shine2);
                                if (dShine2 <= 1.4f)
                                {
                                    pupilCol = whiteCol;
                                }

                                eCol = Color.Lerp(eCol, pupilCol, aPupil);
                            }

                            col = Color.Lerp(col, eCol, aEye);
                        }
                    }

                    col.a *= alphaBody;
                    cols[y * size + x] = col;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates the multi-layered cosmic accretion vortex disk around Nibbles.
        /// 1:1 replica of lines 583-674 in web engine/renderer.js:
        /// - Outer glowing purple/lavender dashed ellipse track
        /// - Middle sunny yellow dashed ellipse track
        /// - Inner electric cyan dashed circle
        /// - Swirling colorful stardust particles with comet tails
        /// </summary>
        public static Sprite GetAccretionVortexSprite()
        {
            string key = "AccretionVortexDiskV1";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            float center = size * 0.5f;
            Vector2 cVec = new Vector2(center, center);

            Color cyanColor = new Color32(0x38, 0xbd, 0xf8, 0xee); // #38bdf8
            Color yellowColor = new Color32(0xfd, 0xe0, 0x47, 0xee); // #fde047
            Color purpleColor = new Color32(0x81, 0x8c, 0xf8, 0xcc); // #818cf8

            // Canonical stardust spark positions orbiting around Nibbles
            var sparkles = new (float x, float y, float r, Color col, float tailAng)[]
            {
                (42f, 48f, 3.8f, new Color32(0xfa, 0xcc, 0x15, 0xff), 0.8f),
                (-52f, 40f, 3.6f, new Color32(0x4a, 0xde, 0x80, 0xff), 2.2f),
                (-46f, -38f, 3.6f, new Color32(0xf4, 0x72, 0xb6, 0xff), 3.8f),
                (55f, -32f, 3.4f, new Color32(0x38, 0xbd, 0xf8, 0xff), 5.4f),
                (12f, 66f, 3.2f, new Color32(0xfd, 0xe0, 0x47, 0xff), 1.4f),
                (-65f, 5f, 3.4f, new Color32(0x38, 0xbd, 0xf8, 0xff), 3.1f),
                (68f, 15f, 3.2f, new Color32(0xf4, 0x72, 0xb6, 0xff), 0.2f),
                (-20f, -62f, 3.5f, new Color32(0xfa, 0xcc, 0x15, 0xff), 4.6f),
                (30f, -56f, 3.0f, new Color32(0xc0, 0x84, 0xfc, 0xff), 5.9f),
            };

            for (int y = 0; y < size; y++)
            {
                float py = y - center;
                for (int x = 0; x < size; x++)
                {
                    float px = x - center;
                    float dist = Mathf.Sqrt(px * px + py * py);
                    float ang = Mathf.Atan2(py, px);

                    Color c = Color.clear;

                    // Soft ambient halo glow
                    if (dist <= 124f)
                    {
                        float gAlpha = Mathf.Clamp01((124f - dist) / 60f) * 0.20f;
                        c = new Color(purpleColor.r, purpleColor.g, purpleColor.b, gAlpha);
                    }

                    // 1. Outer Lavender dashed ellipse (rx = 104, ry = 86)
                    float ellip3 = Mathf.Sqrt((px * px) / (104f * 104f) + (py * py) / (86f * 86f));
                    float d3 = Mathf.Abs(ellip3 - 1f) * 95f;
                    if (d3 <= 2.8f)
                    {
                        bool isDash3 = Mathf.Sin(ang * 12f) > -0.1f;
                        if (isDash3)
                        {
                            float a3 = Mathf.Clamp01(2.8f - d3);
                            Color c3 = new Color(purpleColor.r, purpleColor.g, purpleColor.b, a3 * 0.88f);
                            c = Color.Lerp(c, c3, c3.a);
                        }
                    }

                    // 2. Middle Yellow dashed ellipse (rx = 82, ry = 68)
                    float ellip2 = Mathf.Sqrt((px * px) / (82f * 82f) + (py * py) / (68f * 68f));
                    float d2 = Mathf.Abs(ellip2 - 1f) * 75f;
                    if (d2 <= 3.2f)
                    {
                        bool isDash2 = Mathf.Sin(-ang * 14f) > -0.15f;
                        if (isDash2)
                        {
                            float a2 = Mathf.Clamp01(3.2f - d2);
                            Color c2 = new Color(yellowColor.r, yellowColor.g, yellowColor.b, a2 * 0.95f);
                            c = Color.Lerp(c, c2, c2.a);
                        }
                    }

                    // 3. Inner Cyan dashed ring (r = 58)
                    float d1 = Mathf.Abs(dist - 58f);
                    if (d1 <= 2.6f)
                    {
                        bool isDash1 = Mathf.Sin(ang * 16f) > -0.2f;
                        if (isDash1)
                        {
                            float a1 = Mathf.Clamp01(2.6f - d1);
                            Color c1 = new Color(cyanColor.r, cyanColor.g, cyanColor.b, a1 * 0.95f);
                            c = Color.Lerp(c, c1, c1.a);
                        }
                    }

                    // 4. Stardust Sparkles with Comet Tails
                    for (int s = 0; s < sparkles.Length; s++)
                    {
                        var sp = sparkles[s];
                        float dx = px - sp.x;
                        float dy = py - sp.y;
                        float sDist = Mathf.Sqrt(dx * dx + dy * dy);

                        if (sDist <= sp.r + 3.5f)
                        {
                            // White-hot center
                            if (sDist <= sp.r * 0.45f)
                            {
                                c = Color.Lerp(c, Color.white, 0.95f);
                            }
                            // Colored glow bead
                            else if (sDist <= sp.r)
                            {
                                float aS = Mathf.Clamp01(sp.r - sDist + 0.5f);
                                c = Color.Lerp(c, sp.col, aS * 0.9f);
                            }
                            // Tangent streak tail
                            else
                            {
                                float tx = Mathf.Cos(sp.tailAng);
                                float ty = Mathf.Sin(sp.tailAng);
                                float proj = dx * tx + dy * ty;
                                float perp = Mathf.Abs(dx * (-ty) + dy * tx);
                                if (proj <= 0f && proj >= -8.0f && perp <= 1.5f)
                                {
                                    float aTail = (1f - (-proj / 8.0f)) * (1f - perp / 1.5f) * 0.85f;
                                    c = Color.Lerp(c, sp.col, aTail);
                                }
                            }
                        }
                    }

                    cols[y * size + x] = c;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        #region Procedural Gate Badges (1:1 Web: renderSawbladeGate & renderCompactorGate)
        private static float DistToSegment(float px, float py, float x0, float y0, float x1, float y1)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            float len2 = dx * dx + dy * dy;
            if (len2 <= 0.0001f) return Vector2.Distance(new Vector2(px, py), new Vector2(x0, y0));
            float t = Mathf.Clamp01(((px - x0) * dx + (py - y0) * dy) / len2);
            float nearX = x0 + t * dx;
            float nearY = y0 + t * dy;
            return Vector2.Distance(new Vector2(px, py), new Vector2(nearX, nearY));
        }

        private static float HexagonSDF(float px, float py, float hw, float hh, float pExtra)
        {
            float ax = Mathf.Abs(px);
            float ay = Mathf.Abs(py);
            if (ax <= hw)
            {
                return ay - hh;
            }
            float qx = ax - hw;
            float qy = ay;
            Vector2 N = new Vector2(hh, pExtra).normalized;
            float dLine = (qx * N.x + (qy - hh) * N.y);
            if (qy > hh)
            {
                return Vector2.Distance(new Vector2(qx, qy), new Vector2(0f, hh));
            }
            if (qx > pExtra)
            {
                return Vector2.Distance(new Vector2(qx, qy), new Vector2(pExtra, 0f));
            }
            return dLine;
        }

        private static float PillSDF(float px, float py, float flatW, float r)
        {
            float qx = Mathf.Max(0f, Mathf.Abs(px) - flatW);
            return Mathf.Sqrt(qx * qx + py * py) - r;
        }

        private static bool IsInsideLightningBolt(float lx, float ly)
        {
            // True 6-point zigzag lightning bolt polygon:
            // P0: Top-right tip (2.5, 12)
            // P1: Left jog outer corner (-6.5, 1.5)
            // P2: Center jog inner corner (1.0, 1.5)
            // P3: Bottom-left tip (-2.5, -12)
            // P4: Right jog outer corner (6.5, -1.5)
            // P5: Center jog inner corner (-1.0, -1.5)
            float[] vx = { 2.5f, -6.5f, 1.0f, -2.5f, 6.5f, -1.0f };
            float[] vy = { 12.0f, 1.5f, 1.5f, -12.0f, -1.5f, -1.5f };
            bool inside = false;
            for (int i = 0, j = 5; i < 6; j = i++)
            {
                if (((vy[i] > ly) != (vy[j] > ly)) &&
                    (lx < (vx[j] - vx[i]) * (ly - vy[i]) / (vy[j] - vy[i]) + vx[i]))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        private static float DistToCharX(float px, float py)
        {
            float d1 = DistToSegment(px, py, -3.5f, -3.5f, 3.5f, 3.5f);
            float d2 = DistToSegment(px, py, -3.5f, 3.5f, 3.5f, -3.5f);
            return Mathf.Min(d1, d2);
        }

        private static float DistToCharPlus(float px, float py)
        {
            float d1 = DistToSegment(px, py, -4.0f, 0f, 4.0f, 0f);
            float d2 = DistToSegment(px, py, 0f, -4.0f, 0f, 4.0f);
            return Mathf.Min(d1, d2);
        }

        private static float DistToDigit(float px, float py, int digit)
        {
            if (digit == 2)
            {
                float dArc = 999f;
                if (py >= 2.5f)
                {
                    float dCenter = Vector2.Distance(new Vector2(px, py), new Vector2(0f, 2.5f));
                    if (px >= -5.5f && px <= 5.5f) dArc = Mathf.Abs(dCenter - 5.5f);
                }
                float dDiag = DistToSegment(px, py, 5.5f, 2.5f, -5.5f, -8.5f);
                float dBase = DistToSegment(px, py, -5.5f, -8.5f, 5.5f, -8.5f);
                return Mathf.Min(dArc, Mathf.Min(dDiag, dBase));
            }
            if (digit == 3)
            {
                float dTop = DistToSegment(px, py, -5.5f, 8.0f, 5.5f, 8.0f);
                float dMid = DistToSegment(px, py, 5.5f, 8.0f, 0.0f, 0.5f);
                float dArc = 999f;
                if (px >= -2.0f)
                {
                    float dC = Vector2.Distance(new Vector2(px, py), new Vector2(0.0f, -4.0f));
                    dArc = Mathf.Abs(dC - 4.8f);
                }
                return Mathf.Min(dTop, Mathf.Min(dMid, dArc));
            }
            return DistToSegment(px, py, 0f, -8.5f, 0f, 8.5f);
        }

        private static Color GetMagnetColor(float mx, float my)
        {
            float rOut = 8.5f;
            float rIn = 4.0f;
            float archCenterY = 2.0f;
            float botY = -10.0f;
            float tipSplitY = -5.0f;

            bool inside = false;
            if (my >= archCenterY)
            {
                float d = Vector2.Distance(new Vector2(mx, my), new Vector2(0f, archCenterY));
                inside = d >= rIn && d <= rOut;
            }
            else if (my >= botY)
            {
                inside = (mx >= -rOut && mx <= -rIn) || (mx >= rIn && mx <= rOut);
            }

            if (!inside) return Color.clear;

            if (my < tipSplitY)
            {
                return new Color32(0xe2, 0xe8, 0xf0, 0xff); // Silver tips #e2e8f0
            }
            return new Color32(0xef, 0x44, 0x44, 0xff); // Red horseshoe body #ef4444
        }

        private static float DistToBoostWord(float px, float py)
        {
            // B
            float b1 = DistToSegment(px, py, -18f, -7f, -18f, 7f);
            float b2 = DistToSegment(px, py, -18f, 7f, -13f, 7f);
            float b3 = DistToSegment(px, py, -18f, 0f, -13f, 0f);
            float b4 = DistToSegment(px, py, -18f, -7f, -13f, -7f);
            // O
            float dO = Mathf.Abs(Vector2.Distance(new Vector2(px, py), new Vector2(-6f, 0f)) - 5.5f);
            // S
            float dS = DistToSegment(px, py, 6f, 5f, 2f, 5f);
            // T
            float t1 = DistToSegment(px, py, 12f, 7f, 20f, 7f);
            float t2 = DistToSegment(px, py, 16f, -7f, 16f, 7f);

            return Mathf.Min(Mathf.Min(b1, Mathf.Min(b2, Mathf.Min(b3, b4))), Mathf.Min(dO, Mathf.Min(dS, Mathf.Min(t1, t2))));
        }

        /// <summary>
        /// Generates the canonical 1:1 comic multiplier/adder gate badge matching web engine/renderer.js:
        /// - Multiplier: Hexagonal white badge, dark blue drop shadow, hazard orange border, ⚡ icon, bold ×N text
        /// - Adder: Rounded pill white badge, dark blue drop shadow, cyan border, 🧲 icon, bold +N text
        /// - Booster: Rounded pill lime badge, dark drop shadow, white BOOST text
        /// </summary>
        public static Sprite GetGateBadgeSprite(Physics.GateType type, int value)
        {
            string key = $"GateBadge_V3_{type}_{value}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 128;
            int h = 64;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];

            float cx = w * 0.5f;
            float cy = h * 0.5f;

            Color darkShadow = new Color32(0x1e, 0x1b, 0x4b, 0xff); // #1e1b4b
            Color orangeBorder = new Color32(0xf9, 0x73, 0x16, 0xff); // #f97316
            Color orangeText = new Color32(0xc2, 0x41, 0x0c, 0xff); // #c2410c
            Color cyanBorder = new Color32(0x08, 0x91, 0xb2, 0xff); // #0891b2
            Color cyanText = new Color32(0x03, 0x69, 0xa1, 0xff); // #0369a1
            Color boosterFill = new Color32(0x84, 0xcc, 0x16, 0xff); // #84cc16
            Color whiteCol = Color.white;

            float hexW = 44f;
            float hexH = 20f;
            float hexPoint = 10f;
            float shadowOffsetY = -3.5f;

            for (int y = 0; y < h; y++)
            {
                float py = y - cy + 0.5f;
                for (int x = 0; x < w; x++)
                {
                    float px = x - cx + 0.5f;
                    Color c = Color.clear;

                    if (type == Physics.GateType.Multiply)
                    {
                        // 1. Hexagon Shadow
                        float sPy = py - shadowOffsetY;
                        float dShadow = HexagonSDF(px, sPy, hexW, hexH, hexPoint);
                        float aShadow = Mathf.Clamp01(0.5f - dShadow);
                        if (aShadow > 0f)
                        {
                            c = new Color(darkShadow.r, darkShadow.g, darkShadow.b, aShadow);
                        }

                        // 2. Hexagon Body & Orange Border
                        float dBody = HexagonSDF(px, py, hexW, hexH, hexPoint);
                        float aBody = Mathf.Clamp01(0.5f - dBody);
                        if (aBody > 0f)
                        {
                            Color bodyCol = dBody >= -3.5f ? orangeBorder : whiteCol;
                            c = Color.Lerp(c, bodyCol, aBody);
                        }

                        // 3. Lightning Bolt Icon (Centered at cx - 16, cy = 32)
                        if (IsInsideLightningBolt(px - (-16f), py))
                        {
                            c = orangeBorder;
                        }

                        // 4. Character '×' (Centered at cx + 4, cy = 32)
                        float dX = DistToCharX(px - 4f, py);
                        if (dX <= 2.2f)
                        {
                            float aX = Mathf.Clamp01(2.2f - dX);
                            c = Color.Lerp(c, orangeText, aX);
                        }

                        // 5. Digit 'value' (Centered at cx + 16, cy = 32)
                        float dNum = DistToDigit(px - 16f, py, value);
                        if (dNum <= 2.2f)
                        {
                            float aNum = Mathf.Clamp01(2.2f - dNum);
                            c = Color.Lerp(c, orangeText, aNum);
                        }
                    }
                    else if (type == Physics.GateType.Add)
                    {
                        float pillR = 20f;
                        float pillFlatW = 34f;

                        // 1. Pill Shadow
                        float sPy = py - shadowOffsetY;
                        float dShadow = PillSDF(px, sPy, pillFlatW, pillR);
                        float aShadow = Mathf.Clamp01(0.5f - dShadow);
                        if (aShadow > 0f)
                        {
                            c = new Color(darkShadow.r, darkShadow.g, darkShadow.b, aShadow);
                        }

                        // 2. Pill Body & Cyan Border
                        float dBody = PillSDF(px, py, pillFlatW, pillR);
                        float aBody = Mathf.Clamp01(0.5f - dBody);
                        if (aBody > 0f)
                        {
                            Color bodyCol = dBody >= -3.5f ? cyanBorder : whiteCol;
                            c = Color.Lerp(c, bodyCol, aBody);
                        }

                        // 3. Horseshoe Magnet Icon (Centered at cx - 16, cy = 32)
                        Color magCol = GetMagnetColor(px - (-16f), py);
                        if (magCol.a > 0f)
                        {
                            c = Color.Lerp(c, magCol, magCol.a);
                        }

                        // 4. Character '+' (Centered at cx + 4, cy = 32)
                        float dPlus = DistToCharPlus(px - 4f, py);
                        if (dPlus <= 2.2f)
                        {
                            float aP = Mathf.Clamp01(2.2f - dPlus);
                            c = Color.Lerp(c, cyanText, aP);
                        }

                        // 5. Digit 'value' (Centered at cx + 16, cy = 32)
                        float dNum = DistToDigit(px - 16f, py, value);
                        if (dNum <= 2.2f)
                        {
                            float aNum = Mathf.Clamp01(2.2f - dNum);
                            c = Color.Lerp(c, cyanText, aNum);
                        }
                    }
                    else if (type == Physics.GateType.Booster)
                    {
                        float pillR = 20f;
                        float pillFlatW = 38f;

                        // 1. Pill Shadow
                        float sPy = py - shadowOffsetY;
                        float dShadow = PillSDF(px, sPy, pillFlatW, pillR);
                        float aShadow = Mathf.Clamp01(0.5f - dShadow);
                        if (aShadow > 0f)
                        {
                            c = new Color(darkShadow.r, darkShadow.g, darkShadow.b, aShadow);
                        }

                        // 2. Pill Body & Dark Border
                        float dBody = PillSDF(px, py, pillFlatW, pillR);
                        float aBody = Mathf.Clamp01(0.5f - dBody);
                        if (aBody > 0f)
                        {
                            Color bodyCol = dBody >= -3.5f ? darkShadow : boosterFill;
                            c = Color.Lerp(c, bodyCol, aBody);
                        }

                        // 3. BOOST text
                        float dText = DistToBoostWord(px, py);
                        if (dText <= 2.2f)
                        {
                            float aT = Mathf.Clamp01(2.2f - dText);
                            c = Color.Lerp(c, whiteCol, aT);
                        }
                    }

                    cols[y * w + x] = c;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }
        #endregion

        /// <summary>
        /// Generates the circular white station inner hub disc with dark outline and accent rim.
        /// </summary>
        public static Sprite GetInnerHubSprite(Color accentColor)
        {
            string key = $"InnerHub_{accentColor.GetHashCode()}";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            float center = size * 0.5f;
            float rOuter = center - 4f;
            float rInner = rOuter - 8f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(rOuter - dist + 0.5f);

                    Color c = Color.clear;
                    if (alpha > 0f)
                    {
                        if (dist >= rInner)
                        {
                            c = ColorBorderDark;
                        }
                        else
                        {
                            c = Color.white;
                        }
                        c.a = alpha;
                    }
                    cols[y * size + x] = c;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates the vertical standing pink torus ring for the Home Screen character diorama stage.
        /// Features 11px glowing strawberry pink border (#f472b6) with soft neon outer glow and transparent center.
        /// </summary>
        public static Sprite GetFloaterTorusSprite()
        {
            string key = "FloaterTorusRing";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            float center = size * 0.5f;
            float rOuter = 104f;
            float rInner = 82f;
            Color pink = ColorPinkMain;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                    Color c = Color.clear;

                    if (d <= rOuter && d >= rInner)
                    {
                        float aOuter = Mathf.Clamp01(rOuter - d + 0.5f);
                        float aInner = Mathf.Clamp01(d - rInner + 0.5f);
                        float alpha = Mathf.Min(aOuter, aInner);
                        c = new Color(pink.r, pink.g, pink.b, alpha);
                    }
                    else if (d > rOuter && d <= rOuter + 14f)
                    {
                        float glow = 1f - (d - rOuter) / 14f;
                        c = new Color(pink.r, pink.g, pink.b, glow * 0.45f);
                    }
                    else if (d < rInner && d >= rInner - 8f)
                    {
                        float glow = (d - (rInner - 8f)) / 8f;
                        c = new Color(pink.r, pink.g, pink.b, glow * 0.25f);
                    }

                    cols[y * size + x] = c;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates the circular character disc for Nibbles in the Home Screen diorama.
        /// Features 3.5px electric cyan border (#38bdf8), radial dark indigo gradient fill,
        /// blushing pink cheeks, and cute smiling face.
        /// </summary>
        public static Sprite GetNibblesDioramaSprite()
        {
            string key = "NibblesDioramaDisc";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            float center = size * 0.5f;
            float rOuter = 54f;
            float rInner = rOuter - 5.5f;
            Color cyanBorder = ColorCyanMain;
            Color darkIndigoCenter = new Color32(0x1e, 0x1b, 0x4b, 0xff);
            Color darkCore = new Color32(0x0f, 0x0c, 0x29, 0xff);

            Vector2 leftCheek = new Vector2(center - 24f, center - 6f);
            Vector2 rightCheek = new Vector2(center + 24f, center - 6f);
            float cheekRadius = 6.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pt = new Vector2(x + 0.5f, y + 0.5f);
                    float d = Vector2.Distance(pt, new Vector2(center, center));
                    Color c = Color.clear;

                    if (d <= rOuter)
                    {
                        float alpha = Mathf.Clamp01(rOuter - d + 0.5f);
                        if (d >= rInner)
                        {
                            c = cyanBorder;
                        }
                        else
                        {
                            float t = d / rInner;
                            c = Color.Lerp(darkIndigoCenter, darkCore, t);

                            float dL = Vector2.Distance(pt, leftCheek);
                            float dR = Vector2.Distance(pt, rightCheek);
                            if (dL <= cheekRadius)
                            {
                                float aCheek = Mathf.Clamp01(1f - dL / cheekRadius) * 0.65f;
                                c = Color.Lerp(c, ColorPinkMain, aCheek);
                            }
                            else if (dR <= cheekRadius)
                            {
                                float aCheek = Mathf.Clamp01(1f - dR / cheekRadius) * 0.65f;
                                c = Color.Lerp(c, ColorPinkMain, aCheek);
                            }

                            // Smiling eye arcs
                            float dEyeL = Vector2.Distance(pt, new Vector2(center - 15f, center + 10f));
                            if (dEyeL <= 9f && dEyeL >= 6f && pt.y >= center + 8f)
                            {
                                c = Color.white;
                            }
                            float dEyeR = Vector2.Distance(pt, new Vector2(center + 15f, center + 10f));
                            if (dEyeR <= 9f && dEyeR >= 6f && pt.y >= center + 8f)
                            {
                                c = Color.white;
                            }

                            // Smiling open mouth
                            float dMouth = Vector2.Distance(pt, new Vector2(center, center - 6f));
                            if (dMouth <= 11f && pt.y <= center - 5f)
                            {
                                float aMouth = Mathf.Clamp01(11f - dMouth);
                                c = Color.Lerp(c, new Color32(0xf4, 0x3f, 0x5e, 0xff), aMouth);
                            }
                        }
                        c.a *= alpha;
                    }
                    else if (d <= rOuter + 10f)
                    {
                        float glow = 1f - (d - rOuter) / 10f;
                        c = new Color(cyanBorder.r, cyanBorder.g, cyanBorder.b, glow * 0.5f);
                    }

                    cols[y * size + x] = c;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates the small yellow smiling space bolt bead for gameplay debris.
        /// 1:1 replica of lines 1840-1861 in renderer.js.
        /// </summary>
        public static Sprite GetSmileyDebrisSprite()
        {
            string key = "SmileyDebrisBead";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            float center = size * 0.5f;
            float rOuter = 26f;
            float rInner = rOuter - 3.8f;
            Color yellow = ColorYellowMain;
            Color dark = ColorBorderDark;

            Vector2 leftEye = new Vector2(center - 7.5f, center + 3f);
            Vector2 rightEye = new Vector2(center + 7.5f, center + 3f);
            float eyeRadius = 3.2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pt = new Vector2(x + 0.5f, y + 0.5f);
                    float d = Vector2.Distance(pt, new Vector2(center, center));
                    Color c = Color.clear;

                    if (d <= rOuter)
                    {
                        float alpha = Mathf.Clamp01(rOuter - d + 0.5f);
                        if (d >= rInner)
                        {
                            c = dark;
                        }
                        else
                        {
                            c = yellow;

                            float dEL = Vector2.Distance(pt, leftEye);
                            float dER = Vector2.Distance(pt, rightEye);
                            if (dEL <= eyeRadius || dER <= eyeRadius)
                            {
                                c = dark;
                            }

                            float dM = Vector2.Distance(pt, new Vector2(center, center - 2f));
                            if (dM <= 9.5f && dM >= 6.5f && pt.y <= center - 2f)
                            {
                                c = dark;
                            }
                        }
                        c.a *= alpha;
                    }

                    cols[y * size + x] = c;
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }

        /// <summary>
        /// Generates the cosmic background texture with Saturn, cyan planet, yellow moon,
        /// and twinkling stars (1:1 Web Prototype renderCartoonSky).
        /// </summary>
        public static Sprite GetCosmicBackgroundSprite()
        {
            string key = "CosmicBackgroundSky";
            if (cachedSprites.TryGetValue(key, out var cached) && cached != null) return cached;

            int w = 512;
            int h = 1024;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];

            Color colTop = new Color32(0x1e, 0x1b, 0x4b, 0xff);
            Color colMid = new Color32(0x27, 0x23, 0x69, 0xff);
            Color colBot = new Color32(0x31, 0x2e, 0x81, 0xff);

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                Color bgCol;
                if (t >= 0.5f)
                {
                    bgCol = Color.Lerp(colMid, colTop, (t - 0.5f) * 2f);
                }
                else
                {
                    bgCol = Color.Lerp(colBot, colMid, t * 2f);
                }

                for (int x = 0; x < w; x++)
                {
                    cols[y * w + x] = bgCol;
                }
            }

            void DrawDisc(Vector2 c, float r, Color col, float shineOffset = 0f)
            {
                int minX = Mathf.Max(0, (int)(c.x - r - 2));
                int maxX = Mathf.Min(w - 1, (int)(c.x + r + 2));
                int minY = Mathf.Max(0, (int)(c.y - r - 2));
                int maxY = Mathf.Min(h - 1, (int)(c.y + r + 2));

                for (int py = minY; py <= maxY; py++)
                {
                    for (int px = minX; px <= maxX; px++)
                    {
                        float d = Vector2.Distance(new Vector2(px, py), c);
                        if (d <= r)
                        {
                            float a = Mathf.Clamp01(r - d + 0.5f);
                            Color pixel = col;
                            if (shineOffset > 0f)
                            {
                                Vector2 shinePos = c + new Vector2(-r * 0.35f, r * 0.35f);
                                float dShine = Vector2.Distance(new Vector2(px, py), shinePos);
                                if (dShine <= r * 0.35f)
                                {
                                    pixel = Color.Lerp(pixel, Color.white, 0.45f);
                                }
                            }
                            int idx = py * w + px;
                            cols[idx] = Color.Lerp(cols[idx], pixel, a);
                        }
                    }
                }
            }

            void DrawRing(Vector2 c, float rx, float ry, float angleRad, Color col, float thickness)
            {
                float cosA = Mathf.Cos(angleRad);
                float sinA = Mathf.Sin(angleRad);
                float maxR = Mathf.Max(rx, ry) + thickness + 2f;

                int minX = Mathf.Max(0, (int)(c.x - maxR));
                int maxX = Mathf.Min(w - 1, (int)(c.x + maxR));
                int minY = Mathf.Max(0, (int)(c.y - maxR));
                int maxY = Mathf.Min(h - 1, (int)(c.y + maxR));

                for (int py = minY; py <= maxY; py++)
                {
                    for (int px = minX; px <= maxX; px++)
                    {
                        float dx = px - c.x;
                        float dy = py - c.y;
                        float rotX = dx * cosA - dy * sinA;
                        float rotY = dx * sinA + dy * cosA;

                        float ellip = (rotX * rotX) / (rx * rx) + (rotY * rotY) / (ry * ry);
                        float distFromRim = Mathf.Abs(Mathf.Sqrt(ellip) - 1.0f) * Mathf.Min(rx, ry);

                        if (distFromRim <= thickness * 0.5f + 1f)
                        {
                            float a = Mathf.Clamp01(thickness * 0.5f + 0.5f - distFromRim);
                            int idx = py * w + px;
                            cols[idx] = Color.Lerp(cols[idx], col, a);
                        }
                    }
                }
            }

            void DrawStar(Vector2 c, float size, Color col)
            {
                int r = (int)(size + 1);
                int minX = Mathf.Max(0, (int)(c.x - r));
                int maxX = Mathf.Min(w - 1, (int)(c.x + r));
                int minY = Mathf.Max(0, (int)(c.y - r));
                int maxY = Mathf.Min(h - 1, (int)(c.y + r));

                for (int py = minY; py <= maxY; py++)
                {
                    for (int px = minX; px <= maxX; px++)
                    {
                        float dx = Mathf.Abs(px - c.x) / size;
                        float dy = Mathf.Abs(py - c.y) / size;
                        if (dx <= 1f && dy <= 1f)
                        {
                            float starVal = Mathf.Sqrt(dx) + Mathf.Sqrt(dy);
                            if (starVal <= 1.05f)
                            {
                                float a = Mathf.Clamp01((1.05f - starVal) * 2f);
                                int idx = py * w + px;
                                cols[idx] = Color.Lerp(cols[idx], col, a);
                            }
                        }
                    }
                }
            }

            Vector2 saturnCenter = new Vector2(0.15f * w, 0.82f * h);
            float saturnR = 26f;
            DrawDisc(saturnCenter, saturnR, ColorPinkMain, 1f);
            DrawRing(saturnCenter, saturnR * 1.8f, saturnR * 0.45f, 0.32f, ColorYellowMain, 4.5f);

            Vector2 cyanCenter = new Vector2(0.82f * w, 0.75f * h);
            DrawDisc(cyanCenter, 34f, ColorCyanMain, 1f);

            Vector2 moonCenter = new Vector2(0.88f * w, 0.18f * h);
            DrawDisc(moonCenter, 22f, ColorYellowMain, 1f);

            var starPoints = new (float x, float y, float size, Color col, bool isStar)[]
            {
                (0.08f, 0.94f, 6f, ColorYellowMain, true),
                (0.48f, 0.95f, 5f, Color.white, false),
                (0.88f, 0.92f, 7f, ColorYellowMain, true),
                (0.32f, 0.86f, 6f, ColorPinkMain, true),
                (0.68f, 0.88f, 5f, Color.white, false),
                (0.05f, 0.72f, 7f, ColorYellowMain, true),
                (0.42f, 0.78f, 4f, Color.white, false),
                (0.94f, 0.68f, 6f, ColorPinkMain, true),
                (0.12f, 0.58f, 5f, Color.white, false),
                (0.92f, 0.52f, 7f, ColorYellowMain, true),
                (0.06f, 0.42f, 6f, ColorPinkMain, true),
                (0.90f, 0.38f, 5f, Color.white, false),
                (0.18f, 0.28f, 7f, ColorYellowMain, true),
                (0.72f, 0.32f, 5f, Color.white, false),
                (0.36f, 0.22f, 6f, ColorPinkMain, true),
                (0.62f, 0.15f, 7f, ColorYellowMain, true),
                (0.10f, 0.10f, 5f, Color.white, false),
                (0.50f, 0.08f, 6f, ColorPinkMain, true),
                (0.78f, 0.06f, 7f, ColorYellowMain, true),
                (0.24f, 0.04f, 5f, Color.white, false),
            };

            foreach (var sp in starPoints)
            {
                Vector2 pt = new Vector2(sp.x * w, sp.y * h);
                if (sp.isStar) DrawStar(pt, sp.size, sp.col);
                else DrawDisc(pt, sp.size * 0.5f, sp.col, 0f);
            }

            tex.SetPixels(cols);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            spr.name = key;
            cachedSprites[key] = spr;
            return spr;
        }
        #endregion
    }
}
