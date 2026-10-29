using System;
using System.Collections.Generic;
using UnityEngine;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.Render
{
    public sealed partial class HomeChrome
    {
        public bool WardrobeOpen;
        public string WardrobeTab = "rings";
        public EconomyManager WardrobeEconomy;

        readonly List<Dictionary<string, object>> wardrobeHitElements = new List<Dictionary<string, object>>();
        string wardrobeHitTab;
        bool wardrobeHitOpen;

        public void SyncWardrobeHitElements()
        {
            if (WardrobeOpen == wardrobeHitOpen && WardrobeTab == wardrobeHitTab && wardrobeHitElements.Count > 0)
                return;

            wardrobeHitElements.Clear();
            wardrobeHitOpen = WardrobeOpen;
            wardrobeHitTab = WardrobeTab;

            if (!WardrobeOpen) return;

            double panelY = 540;

            // 1. Backdrop
            var backdrop = CreateHitElement("wardrobe-backdrop", "", 0, 0, cssW, panelY);
            wardrobeHitElements.Add(backdrop);

            // 2. Close button
            var closeBtn = CreateHitElement("btn-wardrobe-close", "btn-wardrobe-close", 382, panelY + 22, 32, 32);
            wardrobeHitElements.Add(closeBtn);

            // 3. Tabs
            var tabRings = CreateHitElement("wardrobe-tab-rings", "wardrobe-tab-btn" + (WardrobeTab == "rings" ? " active" : ""), 20, panelY + 68, 126, 34);
            var tabSings = CreateHitElement("wardrobe-tab-singularities", "wardrobe-tab-btn" + (WardrobeTab == "singularities" ? " active" : ""), 152, panelY + 68, 126, 34);
            var tabSkies = CreateHitElement("wardrobe-tab-skyboxes", "wardrobe-tab-btn" + (WardrobeTab == "skyboxes" ? " active" : ""), 284, panelY + 68, 126, 34);
            wardrobeHitElements.Add(tabRings);
            wardrobeHitElements.Add(tabSings);
            wardrobeHitElements.Add(tabSkies);

            // 4. Item cards
            var items = EconomyCatalog.Category(WardrobeTab ?? "rings");
            if (items != null)
            {
                double[] colX = { 16, 220 };
                double cardW = 194;
                double cardH = 58;
                double rowY0 = panelY + 116;
                double rowH = 68;

                for (int i = 0; i < items.Length && i < 4; i++)
                {
                    int col = i % 2;
                    int row = i / 2;
                    double x = colX[col];
                    double y = rowY0 + row * rowH;

                    var card = CreateHitElement("wardrobe-item-" + items[i].Id, "wardrobe-item-card", x, y, cardW, cardH);
                    wardrobeHitElements.Add(card);
                }
            }
        }

        Dictionary<string, object> CreateHitElement(string id, string cssClass, double x, double y, double w, double h)
        {
            var d = new Dictionary<string, object>
            {
                ["i"] = (double)(MaxElementI() + 1 + wardrobeHitElements.Count),
                ["p"] = 0.0,
                ["t"] = "div",
                ["id"] = id,
                ["c"] = cssClass ?? "",
                ["r"] = new List<object> { x, y, w, h },
                ["v"] = 1.0,
                ["d"] = "block",
                ["o"] = 1.0
            };
            return d;
        }

        public void PaintWardrobeSheet()
        {
            SyncWardrobeHitElements();

            double panelY = 540;
            double panelH = cssH - panelY;

            canvas.Save();

            // 1. Dark Backdrop
            canvas.Save();
            canvas.SetTransform(scale, 0, 0, scale, 0, 0);
            var backdropStyle = new CssBoxStyle
            {
                x = 0, y = 0, w = cssW, h = cssH, hasBg = true,
                bg = CssColor.Parse("rgba(15, 23, 42, 0.45)", Rgba.Black)
            };
            CssBox.Draw(canvas, backdropStyle, 1.0);
            canvas.Restore();

            // 2. Main Sheet Panel
            canvas.Save();
            canvas.SetTransform(scale, 0, 0, scale, 0, 0);
            var panelStyle = new CssBoxStyle
            {
                x = 0, y = panelY, w = cssW, h = panelH, hasBg = true,
                bg = CssColor.Parse("#1e1b4b", Rgba.Black),
                bgImage = "linear-gradient(180deg, rgba(30, 27, 75, 0.98) 0%, rgba(15, 23, 42, 0.99) 100%)"
            };
            panelStyle.radii[0] = new Vec2d(28, 28);
            panelStyle.radii[1] = new Vec2d(28, 28);
            panelStyle.radii[2] = new Vec2d(0, 0);
            panelStyle.radii[3] = new Vec2d(0, 0);
            panelStyle.borderW[0] = 3;
            panelStyle.borderC[0] = CssColor.Parse("#6366f1", Rgba.Black);
            panelStyle.borderOn[0] = true;
            panelStyle.shadows.AddRange(CssBox.ParseShadows("0 -8px 30px rgba(0, 0, 0, 0.55)"));
            CssBox.Draw(canvas, panelStyle, 1.0);

            // 3. Drag handle
            var handleStyle = new CssBoxStyle
            {
                x = 193, y = panelY + 12, w = 44, h = 5, hasBg = true,
                bg = CssColor.Parse("rgba(255, 255, 255, 0.35)", Rgba.White)
            };
            for (int c = 0; c < 4; c++) handleStyle.radii[c] = new Vec2d(2.5, 2.5);
            CssBox.Draw(canvas, handleStyle, 1.0);

            // 4. Header: Title + Close Button
            var paletteImg = CanvasEmoji.Get("🎨");
            if (paletteImg != null)
            {
                canvas.DrawImage(paletteImg, 18, panelY + 26, 22, 22);
            }
            canvas.Font = "800 17px Fredoka";
            double titleBaseline = panelY + 28 + canvas.FontAscent();
            CssBox.DrawText(canvas, "QUICK WARDROBE", 46, titleBaseline, CssColor.Parse("#ffffff", Rgba.White), CssBox.ParseShadows("0 2px 4px rgba(0,0,0,0.4)"), 1.0);

            // Close button (circle with ✕)
            var closeStyle = new CssBoxStyle
            {
                x = 382, y = panelY + 22, w = 32, h = 32, hasBg = true,
                bg = CssColor.Parse("rgba(255, 255, 255, 0.12)", Rgba.Black)
            };
            for (int c = 0; c < 4; c++)
            {
                closeStyle.radii[c] = new Vec2d(16, 16);
                closeStyle.borderW[c] = 1.5;
                closeStyle.borderC[c] = CssColor.Parse("rgba(255, 255, 255, 0.2)", Rgba.White);
                closeStyle.borderOn[c] = true;
            }
            CssBox.Draw(canvas, closeStyle, 1.0);
            canvas.Font = "800 14px Fredoka";
            double closeTw = canvas.MeasureText("✕");
            double closeBaseline = panelY + 22 + (32 - 14) * 0.5 + canvas.FontAscent() - 1;
            CssBox.DrawText(canvas, "✕", 382 + (32 - closeTw) * 0.5, closeBaseline, CssColor.Parse("#ffffff", Rgba.White), null, 1.0);

            // 5. Category Tabs Bar
            var tabsBarStyle = new CssBoxStyle
            {
                x = 16, y = panelY + 64, w = 398, h = 42, hasBg = true,
                bg = CssColor.Parse("rgba(15, 23, 42, 0.6)", Rgba.Black)
            };
            for (int c = 0; c < 4; c++)
            {
                tabsBarStyle.radii[c] = new Vec2d(14, 14);
                tabsBarStyle.borderW[c] = 1.5;
                tabsBarStyle.borderC[c] = CssColor.Parse("rgba(255, 255, 255, 0.08)", Rgba.White);
                tabsBarStyle.borderOn[c] = true;
            }
            CssBox.Draw(canvas, tabsBarStyle, 1.0);

            // Draw each tab
            string[] tabKeys = { "rings", "singularities", "skyboxes" };
            string[] tabLabels = { "Rings", "Nibbles", "Skies" };
            string[] tabEmojis = { "🍩", "😋", "🌌" };
            double[] tabX = { 20, 152, 284 };
            double tabW = 126;
            double tabH = 34;
            double tabY = panelY + 68;

            string currentTab = WardrobeTab ?? "rings";

            for (int ti = 0; ti < 3; ti++)
            {
                bool active = currentTab == tabKeys[ti];
                double tx = tabX[ti];

                if (active)
                {
                    var tabActiveStyle = new CssBoxStyle
                    {
                        x = tx, y = tabY, w = tabW, h = tabH, hasBg = true,
                        bg = CssColor.Parse("#6366f1", Rgba.Black),
                        bgImage = "linear-gradient(135deg, #6366f1, #818cf8)"
                    };
                    for (int c = 0; c < 4; c++) tabActiveStyle.radii[c] = new Vec2d(10, 10);
                    tabActiveStyle.shadows.AddRange(CssBox.ParseShadows("0 2px 8px rgba(99, 102, 241, 0.4)"));
                    CssBox.Draw(canvas, tabActiveStyle, 1.0);
                }

                // Emoji + Text
                canvas.Font = "700 13px Fredoka";
                var emojiImg = CanvasEmoji.Get(tabEmojis[ti]);
                double emojiW = 16, emojiH = 16;
                double textW = canvas.MeasureText(tabLabels[ti]);
                double totalW = (emojiImg != null ? emojiW + 6 : 0) + textW;
                double startX = tx + (tabW - totalW) * 0.5;

                if (emojiImg != null)
                {
                    canvas.DrawImage(emojiImg, startX, tabY + (tabH - emojiH) * 0.5, emojiW, emojiH);
                    startX += emojiW + 6;
                }

                double tabBaseline = tabY + (tabH - 13) * 0.5 + canvas.FontAscent() - 1;
                Rgba tabCol = active ? CssColor.Parse("#ffffff", Rgba.White) : CssColor.Parse("rgba(255, 255, 255, 0.65)", Rgba.White);
                CssBox.DrawText(canvas, tabLabels[ti], startX, tabBaseline, tabCol, null, 1.0);
            }

            // 6. 2-Column Item Cards Grid
            var items = EconomyCatalog.Category(currentTab);
            if (items != null)
            {
                double[] colX = { 16, 220 };
                double cardW = 194;
                double cardH = 58;
                double rowY0 = panelY + 116;
                double rowH = 68;

                string singular = currentTab == "rings" ? "ring" : currentTab == "singularities" ? "singularity" : "skybox";
                string equippedId = WardrobeEconomy != null && WardrobeEconomy.Profile != null && WardrobeEconomy.Profile.Equipped != null
                    ? WardrobeEconomy.Profile.Equipped.Get(singular)
                    : (currentTab == "rings" ? "ring_donut" : currentTab == "singularities" ? "singularity_nibbles" : "skybox_cotton_candy");

                for (int i = 0; i < items.Length && i < 4; i++)
                {
                    var item = items[i];
                    int col = i % 2;
                    int row = i / 2;
                    double cx = colX[col];
                    double cy = rowY0 + row * rowH;

                    bool isUnlocked = WardrobeEconomy != null ? WardrobeEconomy.IsUnlocked(currentTab, item.Id) : item.Unlocked;
                    bool isEquipped = item.Id == equippedId;

                    // Card background & border
                    var cardStyle = new CssBoxStyle
                    {
                        x = cx, y = cy, w = cardW, h = cardH, hasBg = true,
                        bg = isEquipped
                            ? CssColor.Parse("rgba(74, 222, 128, 0.16)", Rgba.Black)
                            : isUnlocked
                                ? CssColor.Parse("rgba(255, 255, 255, 0.07)", Rgba.Black)
                                : CssColor.Parse("rgba(255, 255, 255, 0.04)", Rgba.Black),
                        bgImage = isEquipped
                            ? "linear-gradient(135deg, rgba(74, 222, 128, 0.16) 0%, rgba(99, 102, 241, 0.28) 100%)"
                            : null
                    };
                    for (int c = 0; c < 4; c++)
                    {
                        cardStyle.radii[c] = new Vec2d(16, 16);
                        cardStyle.borderW[c] = 2;
                        cardStyle.borderC[c] = isEquipped
                            ? CssColor.Parse("#4ade80", Rgba.Green)
                            : isUnlocked
                                ? CssColor.Parse("rgba(255, 255, 255, 0.12)", Rgba.White)
                                : CssColor.Parse("rgba(255, 255, 255, 0.2)", Rgba.White);
                        cardStyle.borderOn[c] = true;
                        if (!isUnlocked) cardStyle.borderDashed[c] = true;
                    }
                    if (isEquipped)
                    {
                        cardStyle.shadows.AddRange(CssBox.ParseShadows("0 0 16px rgba(74, 222, 128, 0.4)"));
                    }

                    double cardAlpha = isUnlocked ? 1.0 : 0.72;
                    CssBox.Draw(canvas, cardStyle, cardAlpha);

                    // Swatch box
                    double swX = cx + 8;
                    double swY = cy + 9;
                    double swW = 40;
                    double swH = 40;

                    var swatchStyle = new CssBoxStyle
                    {
                        x = swX, y = swY, w = swW, h = swH, hasBg = true,
                        bg = CssColor.Parse(item.Color ?? item.CoreColor ?? (item.BgGradient != null && item.BgGradient.Length > 0 ? item.BgGradient[0] : "#ffffff"), Rgba.White)
                    };
                    if (currentTab == "rings")
                    {
                        swatchStyle.bgImage = $"linear-gradient(135deg, {item.Color ?? "#f472b6"}, {item.Accent ?? "#fde047"})";
                    }
                    else if (currentTab == "singularities")
                    {
                        swatchStyle.bgImage = $"linear-gradient(135deg, {item.CoreColor ?? "#1e1b4b"}, {item.HaloColor ?? "#818cf8"})";
                    }
                    else if (currentTab == "skyboxes" && item.BgGradient != null && item.BgGradient.Length >= 2)
                    {
                        swatchStyle.bgImage = $"linear-gradient(180deg, {item.BgGradient[0]}, {item.BgGradient[1]})";
                    }

                    for (int c = 0; c < 4; c++)
                    {
                        swatchStyle.radii[c] = new Vec2d(12, 12);
                        swatchStyle.borderW[c] = 2;
                        swatchStyle.borderC[c] = isEquipped
                            ? CssColor.Parse("#4ade80", Rgba.Green)
                            : CssColor.Parse("rgba(255, 255, 255, 0.25)", Rgba.White);
                        swatchStyle.borderOn[c] = true;
                    }
                    swatchStyle.shadows.AddRange(CssBox.ParseShadows("0 2px 5px rgba(0, 0, 0, 0.3)"));
                    CssBox.Draw(canvas, swatchStyle, cardAlpha);

                    // Swatch content (icon or lock)
                    string swatchEmojiStr = !isUnlocked
                        ? "🔒"
                        : currentTab == "rings" ? "🍩" : currentTab == "singularities" ? "😋" : "🌌";
                    var swImg = CanvasEmoji.Get(swatchEmojiStr);
                    if (swImg != null)
                    {
                        canvas.DrawImage(swImg, swX + 10, swY + 10, 20, 20);
                    }

                    // Item Info: Name & Badge
                    // Name
                    canvas.Font = "700 12px Fredoka";
                    string nameText = item.Name ?? "";
                    if (nameText.Length > 16) nameText = nameText.Substring(0, 15) + "…";
                    double nameBaseline = cy + 14 + canvas.FontAscent();
                    CssBox.DrawText(canvas, nameText, cx + 54, nameBaseline, CssColor.Parse("#ffffff", Rgba.White), null, cardAlpha);

                    // Badge
                    string badgeText;
                    Rgba badgeColor;
                    string badgeFont = "700 10px Fredoka";

                    if (isEquipped)
                    {
                        badgeText = "✓ EQUIPPED ✨";
                        badgeColor = CssColor.Parse("#4ade80", Rgba.Green);
                        badgeFont = "800 10px Fredoka";
                    }
                    else if (isUnlocked)
                    {
                        badgeText = "Tap to Wear";
                        badgeColor = CssColor.Parse("#cbd5e1", Rgba.White);
                    }
                    else
                    {
                        badgeText = $"🔒 {item.Cost} 🪙 (Shop)";
                        badgeColor = CssColor.Parse("#f59e0b", Rgba.Yellow);
                    }

                    canvas.Font = badgeFont;
                    double badgeBaseline = cy + 34 + canvas.FontAscent();
                    CssBox.DrawText(canvas, badgeText, cx + 54, badgeBaseline, badgeColor, null, cardAlpha);
                }
            }

            // 7. Footer Hint
            canvas.Font = "600 11px Fredoka";
            string hintText = "✨ Instant Live Preview — Tap any unlocked skin to wear!";
            double hintTw = canvas.MeasureText(hintText);
            double hintBaseline = panelY + 262 + canvas.FontAscent();
            CssBox.DrawText(canvas, hintText, (cssW - hintTw) * 0.5, hintBaseline, CssColor.Parse("rgba(255, 255, 255, 0.55)", Rgba.White), null, 1.0);

            canvas.Restore();
        }
    }
}

