using System;
using System.Collections.Generic;
using OrbitalSalvage.App;
using OrbitalSalvage.Loc;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    /// <summary>One wired click: a control on <see cref="StateId"/> opens <see cref="TargetId"/>.</summary>
    public sealed class NavEdge
    {
        public NavEdge(string stateId, string targetId, string id, string classToken, int classIndex)
        {
            StateId = stateId;
            TargetId = targetId;
            Id = id;
            ClassToken = classToken;
            ClassIndex = classIndex;
        }

        public string StateId { get; }
        public string TargetId { get; }
        /// <summary>Element id, or null when matching <see cref="ClassToken"/>.</summary>
        public string Id { get; }
        /// <summary>Class token, or null when matching <see cref="Id"/>.</summary>
        public string ClassToken { get; }
        /// <summary>-1 matches every element with <see cref="ClassToken"/>. Otherwise the nth such element in document order (0-based).</summary>
        public int ClassIndex { get; }
    }

    /// <summary>
    /// The 13 painted pages and the clicks that move between them.
    /// Targets are only pages <see cref="HomeChrome"/> can paint. Controls that stay on the same
    /// page (mute, sliders, shop buy) or open an unpainted modal (privacy, reset) are omitted.
    /// </summary>
    public static class ParityPages
    {
        public const int CssWidth = 430;
        public const int CssHeight = 932;
        /// <summary>Extra LRU slots past the main-tab set, so a modal can open without evicting a tab.</summary>
        public const int ModalCacheMargin = 3;
        /// <summary>Dock destinations from home, in dock order, plus home itself. Not settings or game.</summary>
        public static readonly string[] MainTabPages = { "home", "maps", "daily", "shop", "achievements" };
        /// <summary>Main tabs stay resident. The margin is the only room for modals and other pages.</summary>
        public static int CacheCapacity => MainTabPages.Length + ModalCacheMargin;

        public struct PrewarmEntry
        {
            public string Page;
            public string TemplateId;
            public string CardStamp;
            public string Profile;
            public string CacheKey;
        }

        /// <summary>
        /// Templates <see cref="AppController"/> would show for the five main tabs, including the live maps
        /// and shop variants and the safe-area profile. Does not change the controller's current page.
        /// </summary>
        public static PrewarmEntry[] PrewarmTemplates(AppController app, string preferredProfile)
        {
            if (app == null) return Array.Empty<PrewarmEntry>();
            var list = new PrewarmEntry[MainTabPages.Length];
            for (int i = 0; i < MainTabPages.Length; i++)
            {
                string page = MainTabPages[i];
                string template = app.PeekPageTemplate(page, out string stamp);
                if (string.IsNullOrEmpty(stamp)) stamp = null;
                string profile = ResolveProfile(template, preferredProfile);
                list[i] = new PrewarmEntry
                {
                    Page = page,
                    TemplateId = template,
                    CardStamp = stamp,
                    Profile = profile,
                    CacheKey = CacheKey(template, profile, stamp)
                };
            }
            return list;
        }

        /// <summary>Same key <see cref="ParityViewer.Show"/> uses: template, profile, and an optional shop card stamp.</summary>
        public static string CacheKey(string stateId, string profile, string cardKinds)
        {
            string key = (stateId ?? "") + "@" + (string.IsNullOrEmpty(profile) ? Profile : profile);
            if (!string.IsNullOrEmpty(cardKinds)) key = key + "#" + cardKinds;
            return key;
        }

        /// <summary>
        /// Product boot pre-bakes the main tabs. Self-tests and parity captures leave it off so their
        /// frames stay on the path they already paint. <see cref="PlayQuality.Forced"/> does not disable
        /// it: stills are painted by Page Preview, which never boots the viewer.
        /// </summary>
        public static bool ShouldPrewarmTabs(bool selfTesting)
        {
            return !selfTesting;
        }

        /// <summary>True when a baked page's raster is not the live quality step. Matches <c>ResizeRaster</c>'s 0.05 band.</summary>
        public static bool RasterScaleIsStale(double baked, double live)
        {
            return Math.Abs(baked - live) >= 0.05;
        }

        /// <summary>Phone resident cap below 6 GB. One home page at 3× is already past this, so a 4 GB phone pre-bakes at 2× only.</summary>
        public const long Phone4GbBudgetBytes = 320L * 1024L * 1024L;
        /// <summary>Phone resident cap at 6 GB and above. Three of the five tabs fit at 2×; one fits at 3×.</summary>
        public const long Phone6GbBudgetBytes = 480L * 1024L * 1024L;
        /// <summary>Editor and desktop resident cap. Four tabs fit at 3×; all five fit at 2×.</summary>
        public const long DesktopBudgetBytes = 1536L * 1024L * 1024L;
        /// <summary><see cref="SystemInfo.systemMemorySize"/> at which a phone uses <see cref="Phone6GbBudgetBytes"/>.</summary>
        public const int Phone6GbMemoryMb = 6144;
        /// <summary>Phone budgets also stay under this share of physical RAM, so a 1 GB device is not given the 4 GB cap.</summary>
        public const int PhoneBudgetPercent = 35;

        /// <summary>Measured <c>home_default</c> canvas + layers at 2× (860×1864) on this Mac.</summary>
        public const long HomeBytesAt2x = 153891840L;
        /// <summary>Measured <c>maps_world_1_fresh</c> at 2×.</summary>
        public const long MapsBytesAt2x = 147479680L;
        /// <summary>Measured <c>daily_default</c> at 2×. Shop and trophies match it.</summary>
        public const long DailyBytesAt2x = 141067520L;
        /// <summary>Measured <c>shop_rings_default</c> at 2×.</summary>
        public const long ShopBytesAt2x = 141067520L;
        /// <summary>Measured <c>achievements_none</c> at 2×.</summary>
        public const long TrophiesBytesAt2x = 141067520L;

        /// <summary>
        /// Bytes of baked layers plus canvas the page cache may hold.
        /// Phones take the lesser of <see cref="PhoneBudgetPercent"/> of RAM and a hard cap:
        /// <see cref="Phone4GbBudgetBytes"/> under <see cref="Phone6GbMemoryMb"/>, otherwise <see cref="Phone6GbBudgetBytes"/>.
        /// Desktop and the editor use <see cref="DesktopBudgetBytes"/>. The page on screen may pass the cap;
        /// callers treat that one page as advisory.
        /// </summary>
        public static long ResidentBudgetBytes(int systemMemoryMb, bool phone)
        {
            if (!phone) return DesktopBudgetBytes;
            if (systemMemoryMb < 0) systemMemoryMb = 0;
            long cap = systemMemoryMb >= Phone6GbMemoryMb ? Phone6GbBudgetBytes : Phone4GbBudgetBytes;
            long fraction = (long)systemMemoryMb * 1024L * 1024L * PhoneBudgetPercent / 100L;
            if (fraction < cap) return fraction;
            return cap;
        }

        /// <summary>Canvas + layers for a main-tab template at <paramref name="scale"/>, from the 2× measurement. Scale² is exact for 2 and 3.</summary>
        public static long EstimateResidentBytes(string templateId, double scale)
        {
            long at2 = BytesAt2x(templateId);
            if (scale < 1) scale = 1;
            double factor = (scale / 2.0) * (scale / 2.0);
            return (long)Math.Round(at2 * factor);
        }

        /// <summary>Home, maps, daily, shop, trophies, in pre-bake order, at <paramref name="scale"/>.</summary>
        public static long[] MainTabBytes(double scale)
        {
            return new[]
            {
                EstimateResidentBytes("home_default", scale),
                EstimateResidentBytes("maps_world_1_fresh", scale),
                EstimateResidentBytes("daily_default", scale),
                EstimateResidentBytes("shop_rings_default", scale),
                EstimateResidentBytes("achievements_none", scale)
            };
        }

        /// <summary>
        /// How many leading pages fit in <paramref name="budget"/> when <paramref name="alreadyResident"/> is already charged.
        /// Stops at the first page that would pass the budget. Does not skip ahead.
        /// </summary>
        public static int CountFitting(long[] orderedBytes, long budget, long alreadyResident = 0)
        {
            long used = alreadyResident < 0 ? 0 : alreadyResident;
            if (orderedBytes == null) return 0;
            int n = 0;
            for (int i = 0; i < orderedBytes.Length; i++)
            {
                long next = orderedBytes[i] < 0 ? 0 : orderedBytes[i];
                if (used > budget || used + next > budget) break;
                used += next;
                n++;
            }
            return n;
        }

        /// <summary>
        /// Whether a background page may be baked at <paramref name="projectedBytes"/>.
        /// A shrink (or a same-size rebake) is always allowed. Growth is allowed only when the other
        /// resident pages plus the projection stay inside <paramref name="budget"/>. The current page is always allowed.
        /// </summary>
        public static bool MayRebakeResident(long otherBytes, long currentBytes, long projectedBytes, long budget, bool currentPage)
        {
            if (currentPage) return true;
            if (otherBytes < 0) otherBytes = 0;
            if (currentBytes < 0) currentBytes = 0;
            if (projectedBytes < 0) projectedBytes = 0;
            if (projectedBytes <= currentBytes) return true;
            return otherBytes + projectedBytes <= budget;
        }

        static long BytesAt2x(string templateId)
        {
            switch (templateId)
            {
                case "home_default": return HomeBytesAt2x;
                case "maps_world_1_fresh": return MapsBytesAt2x;
                case "daily_default": return DailyBytesAt2x;
                case "shop_rings_default": return ShopBytesAt2x;
                case "achievements_none": return TrophiesBytesAt2x;
                default: return DailyBytesAt2x;
            }
        }

        /// <summary>
        /// One queued template per call. Nothing is taken while layer baking is deferred, or while the
        /// loading bar has not been written yet (its width key is part of the baked override set).
        /// </summary>
        public static bool TryTakeTabPrewarm<T>(Queue<T> pending, bool deferred, bool holdForLoadingBar, out T entry)
        {
            entry = default;
            if (deferred || holdForLoadingBar || pending == null || pending.Count == 0) return false;
            entry = pending.Dequeue();
            return true;
        }
        /// <summary>Parity captures, Page Preview's default, and editor windows with a full safe area.</summary>
        public const string Profile = "zero";
        public const string IphoneProfile = "iphone";
        public const string EnabledPrefKey = "OrbitalSalvage.ParityViewer.Enabled";
        public const string HomeId = "home_default";

        // Same order as the capture list. Digit keys 1-9 then 0 cover the first ten; the rest are arrows.
        public static readonly string[] StateIds =
        {
            "home_default",
            "maps_world_1_fresh",
            "daily_default",
            "shop_rings_default",
            "shop_singularities_default",
            "shop_skyboxes_default",
            "achievements_none",
            "settings_default",
            "game_lvl01_t1000ms",
            "modal_pause",
            "modal_debrief_exact",
            "modal_defeat_12_of_20",
            "loading_t0500"
        };

        public static IReadOnlyList<NavEdge> Edges { get; }

        static readonly string[] ShopStates =
        {
            "shop_rings_default", "shop_singularities_default", "shop_skyboxes_default"
        };

        static ParityPages()
        {
            var list = new List<NavEdge>();

            // web/main.js showPage from index.html home controls.
            list.Add(ById("home_default", "btn-home-play", "game_lvl01_t1000ms"));
            list.Add(ById("home_default", "btn-open-maps-page", "maps_world_1_fresh"));
            list.Add(ById("home_default", "btn-open-daily-shift", "daily_default"));
            list.Add(ById("home_default", "btn-open-shop-page", "shop_rings_default"));
            list.Add(ById("home_default", "btn-open-trophies-page", "achievements_none"));
            list.Add(ById("home_default", "btn-home-settings", "settings_default"));

            // maps back button; level tiles call showPage("game"). Only level 1 is painted, so every tile lands there.
            list.Add(ById("maps_world_1_fresh", "btn-back-maps", HomeId));
            list.Add(ByClass("maps_world_1_fresh", "level-tile", "game_lvl01_t1000ms"));

            // daily back; .btn-play-daily-tier calls showPage("game") (same single painted game frame).
            list.Add(ById("daily_default", "btn-back-daily", HomeId));
            list.Add(ByClass("daily_default", "btn-play-daily-tier", "game_lvl01_t1000ms"));

            // Shop tabs are class tab-btn with no id. Document order matches index.html: rings, singularities, skyboxes.
            foreach (var shop in ShopStates)
            {
                list.Add(ById(shop, "btn-back-shop", HomeId));
                list.Add(ByClassNth(shop, "tab-btn", 0, "shop_rings_default"));
                list.Add(ByClassNth(shop, "tab-btn", 1, "shop_singularities_default"));
                list.Add(ByClassNth(shop, "tab-btn", 2, "shop_skyboxes_default"));
            }

            list.Add(ById("achievements_none", "btn-back-achievements", HomeId));
            list.Add(ById("settings_default", "btn-back-settings", HomeId));

            // In-game pause opens #modal-pause. Restart stays on the game frame, so it is not wired.
            list.Add(ById("game_lvl01_t1000ms", "btn-game-pause", "modal_pause"));
            list.Add(ById("game_lvl01_t1000ms", "btn-game-skin", "modal_wardrobe"));
            list.Add(ById("modal_wardrobe", "btn-wardrobe-close", "game_lvl01_t1000ms"));
            list.Add(ById("modal_wardrobe", "wardrobe-backdrop", "game_lvl01_t1000ms"));

            // Pause sheet: resume/close/restart return to the game frame; maps and home leave it.
            // Sound toggles mute and stays put.
            list.Add(ById("modal_pause", "btn-close-pause", "game_lvl01_t1000ms"));
            list.Add(ById("modal_pause", "btn-pause-resume", "game_lvl01_t1000ms"));
            list.Add(ById("modal_pause", "btn-pause-restart", "game_lvl01_t1000ms"));
            list.Add(ById("modal_pause", "btn-pause-maps", "maps_world_1_fresh"));
            list.Add(ById("modal_pause", "btn-pause-home", HomeId));

            // Debrief replay restarts the sector; next advances a sector. Both close the modal onto the game frame.
            list.Add(ById("modal_debrief_exact", "btn-debrief-replay", "game_lvl01_t1000ms"));
            list.Add(ById("modal_debrief_exact", "btn-debrief-next", "game_lvl01_t1000ms"));

            // Defeat "home" is showPage("maps") in main.js, not the home page. Retry restarts the sector.
            list.Add(ById("modal_defeat_12_of_20", "btn-defeat-retry", "game_lvl01_t1000ms"));
            list.Add(ById("modal_defeat_12_of_20", "btn-defeat-home", "maps_world_1_fresh"));

            var languages = LanguageRegistry.All;
            for (int i = 0; i < languages.Count; i++)
                InteractiveIds.Add(LanguageMarker.ButtonId(languages[i].Code));

            Edges = list;
        }

        static NavEdge ById(string state, string id, string target) => new NavEdge(state, target, id, null, -1);
        static NavEdge ByClass(string state, string token, string target) => new NavEdge(state, target, null, token, -1);
        static NavEdge ByClassNth(string state, string token, int index, string target) => new NavEdge(state, target, null, token, index);

        /// <summary>
        /// <c>iphone</c> when the safe area leaves a top or bottom inset. A full-rect safe area, an empty rect,
        /// and a window under 8 px on either side stay on <c>zero</c> (editor Game view, Page Preview, parity captures).
        /// Unity's safe area is bottom-left origin: bottom inset is <c>safeArea.y</c>, top inset is
        /// <c>screenH - (safeArea.y + safeArea.height)</c>.
        /// </summary>
        public static string ProfileFor(int screenW, int screenH, Rect safeArea)
        {
            if (screenW < 8 || screenH < 8) return Profile;
            if (safeArea.width < 8f || safeArea.height < 8f) return Profile;
            float top = screenH - (safeArea.y + safeArea.height);
            float bottom = safeArea.y;
            if (top > 1f || bottom > 1f) return IphoneProfile;
            return Profile;
        }

        /// <summary>Profile for the current screen. Editor play with a full safe area stays on zero.</summary>
        public static string ActiveProfile => ProfileFor(Screen.width, Screen.height, Screen.safeArea);

        /// <summary>Prefer <paramref name="preferred"/> when that spec exists. Otherwise the zero spec.</summary>
        public static string ResolveProfile(string specId, string preferred)
        {
            if (string.IsNullOrEmpty(preferred) || preferred == Profile) return Profile;
            if (Resources.Load<TextAsset>("Parity/Spec/" + specId + "__" + preferred) != null)
                return preferred;
            return Profile;
        }

        public static bool IsState(string id)
        {
            return IndexOf(id) >= 0;
        }

        public static int IndexOf(string id)
        {
            if (id == null) return -1;
            for (int i = 0; i < StateIds.Length; i++)
                if (StateIds[i] == id) return i;
            return -1;
        }

        public static string Cycle(string stateId, int delta)
        {
            int i = IndexOf(stateId);
            if (i < 0) i = 0;
            int n = StateIds.Length;
            int j = (i + delta) % n;
            if (j < 0) j += n;
            return StateIds[j];
        }

        /// <summary>Digit 1-9 select the first nine pages. Digit 0 selects the tenth (modal_pause).</summary>
        public static string Jump(int digit)
        {
            if (digit < 0 || digit > 9) return null;
            int index = digit == 0 ? 9 : digit - 1;
            return index < StateIds.Length ? StateIds[index] : null;
        }

        public static bool SelectorExists(string specJson, NavEdge edge)
        {
            var els = Parse(specJson);
            if (!string.IsNullOrEmpty(edge.Id))
            {
                for (int i = 0; i < els.Count; i++)
                    if (els[i].Id == edge.Id) return true;
                return false;
            }
            int count = 0;
            for (int i = 0; i < els.Count; i++)
                if (HasToken(els[i].Classes, edge.ClassToken)) count++;
            return edge.ClassIndex < 0 ? count > 0 : count > edge.ClassIndex;
        }

        /// <summary>CSS-pixel hit test. Returns a painted state id, or null when the click does not navigate.</summary>
        public static string Hit(string stateId, string specJson, double cssX, double cssY)
        {
            var edge = HitEdge(stateId, specJson, cssX, cssY);
            return edge != null ? edge.TargetId : null;
        }

        /// <summary>Same hit test as <see cref="Hit"/>, including which control was pressed.</summary>
        public static NavEdge HitEdge(string stateId, string specJson, double cssX, double cssY)
        {
            var els = Parse(specJson);
            int hit = Topmost(els, cssX, cssY);
            if (hit < 0) return null;

            // The topmost box wins. Children (icons, labels) walk up to the control that navigates.
            // Anything under an opaque overlay is not reached.
            for (int i = hit; i >= 0; i = els[i].Parent)
            {
                var edge = Match(stateId, els, i);
                if (edge != null) return edge;
            }
            return null;
        }

        /// <summary>Topmost control under the point that has a CSS <c>:active</c> press transform.</summary>
        public static bool TryPressAt(string specJson, double cssX, double cssY, out int index, out string transform)
        {
            return TryPressAt(Parse(specJson), cssX, cssY, out index, out transform);
        }

        /// <summary>Same as the string overload, for elements that were edited after loading (stamped shop cards).</summary>
        public static bool TryPressAt(IReadOnlyList<Dictionary<string, object>> elements, double cssX, double cssY, out int index, out string transform)
        {
            return TryPressAt(Parse(elements), cssX, cssY, out index, out transform);
        }

        static bool TryPressAt(List<El> els, double cssX, double cssY, out int index, out string transform)
        {
            index = -1;
            transform = null;
            int hit = Topmost(els, cssX, cssY);
            if (hit < 0) return false;
            for (int i = hit; i >= 0; i = els[i].Parent)
            {
                if (OrbitalSalvage.Render.CssAnimation.TryPress(els[i].Classes, out transform))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public struct ControlHit
        {
            public string Id;
            public string ClassToken;
            public int ClassIndex;
        }

        static readonly HashSet<string> InteractiveIds = new HashSet<string>
        {
            "btn-home-play", "btn-open-maps-page", "btn-open-shop-page", "btn-open-daily-shift",
            "btn-open-trophies-page", "btn-home-settings", "btn-open-settings-page", "btn-home-audio",
            "btn-toggle-audio", "btn-back-maps", "btn-back-daily", "btn-back-shop", "btn-back-achievements",
            "btn-back-settings", "btn-game-to-home", "btn-game-restart", "btn-game-pause", "btn-game-skin",
            "btn-wardrobe-close", "wardrobe-backdrop", "wardrobe-tab-rings", "wardrobe-tab-singularities", "wardrobe-tab-skyboxes",
            "btn-pause-resume", "btn-close-pause", "btn-pause-restart", "btn-pause-sound",
            "btn-pause-maps", "btn-pause-home", "btn-debrief-replay", "btn-debrief-next",
            "btn-defeat-retry", "btn-defeat-home", "btn-defeat-backup", "btn-claim-daily-gift",
            "btn-claim-master-bonus", "btn-open-privacy", "btn-close-privacy", "btn-dismiss-privacy",
            "btn-open-language", "btn-close-language", "btn-open-user-agreement",
            "btn-reset-game-data", "btn-cancel-reset", "btn-confirm-reset", "home-mascot-stage",
            "home-diorama-nibbles", "settings-sound-toggle", "settings-haptics-toggle",
            "settings-sfx-slider", "settings-ambience-slider"
        };

        static readonly string[] InteractiveClasses =
        {
            "tab-btn", "btn-buy-item", "btn-equip-item", "level-tile", "world-tab-btn", "btn-play-daily-tier",
            "wardrobe-tab-btn", "wardrobe-item-card"
        };

        /// <summary>Topmost interactive control under the point. The viewer dispatches this to <c>AppController</c>.</summary>
        public static bool TryHitControl(string specJson, double cssX, double cssY, out ControlHit hit)
        {
            return TryHitControl(Parse(specJson), cssX, cssY, out hit);
        }

        /// <summary>
        /// Same as the string overload, for elements that were edited after loading. Shop pages swap their cards
        /// (<c>CardStamp</c>), so the buttons on screen are not where the captured file puts them.
        /// </summary>
        public static bool TryHitControl(IReadOnlyList<Dictionary<string, object>> elements, double cssX, double cssY, out ControlHit hit)
        {
            return TryHitControl(Parse(elements), cssX, cssY, out hit);
        }

        static bool TryHitControl(List<El> els, double cssX, double cssY, out ControlHit hit)
        {
            hit = default;
            int top = Topmost(els, cssX, cssY);
            if (top < 0) return false;
            for (int i = top; i >= 0; i = els[i].Parent)
            {
                if (!string.IsNullOrEmpty(els[i].Id) && (InteractiveIds.Contains(els[i].Id) || els[i].Id.StartsWith("wardrobe-item-")))
                {
                    hit.Id = els[i].Id;
                    hit.ClassIndex = -1;
                    return true;
                }
                for (int c = 0; c < InteractiveClasses.Length; c++)
                {
                    if (!HasToken(els[i].Classes, InteractiveClasses[c])) continue;
                    hit.ClassToken = InteractiveClasses[c];
                    hit.ClassIndex = Ordinal(els, i, InteractiveClasses[c]);
                    hit.Id = null;
                    return true;
                }
            }
            return false;
        }

        public static bool TryCenter(string specJson, string id, out double cssX, out double cssY)
        {
            cssX = cssY = 0;
            if (!TryRect(specJson, id, out double x, out double y, out double w, out double h)) return false;
            cssX = x + w * 0.5;
            cssY = y + h * 0.5;
            return true;
        }

        public static bool TryCenter(IReadOnlyList<Dictionary<string, object>> elements, string id, out double cssX, out double cssY)
        {
            cssX = cssY = 0;
            if (!TryRect(Parse(elements), id, out double x, out double y, out double w, out double h)) return false;
            cssX = x + w * 0.5;
            cssY = y + h * 0.5;
            return true;
        }

        public static bool TryRect(string specJson, string id, out double x, out double y, out double w, out double h)
        {
            return TryRect(Parse(specJson), id, out x, out y, out w, out h);
        }

        static bool TryRect(List<El> els, string id, out double x, out double y, out double w, out double h)
        {
            x = y = w = h = 0;
            for (int i = 0; i < els.Count; i++)
            {
                if (els[i].Id != id || !els[i].Hittable) continue;
                x = els[i].X;
                y = els[i].Y;
                w = els[i].W;
                h = els[i].H;
                return true;
            }
            return false;
        }

        /// <summary>Range input 0–100 from a CSS x inside the control's box. Matches the web slider's 0–100.</summary>
        public static int SliderValue(double cssX, double x, double w)
        {
            if (!(w > 0)) return 0;
            double t = (cssX - x) / w;
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return (int)Math.Round(t * 100.0);
        }

        public static bool TryClassCenter(string specJson, string token, int index, out double cssX, out double cssY)
        {
            return TryClassCenter(Parse(specJson), token, index, out cssX, out cssY);
        }

        public static bool TryClassCenter(IReadOnlyList<Dictionary<string, object>> elements, string token, int index, out double cssX, out double cssY)
        {
            return TryClassCenter(Parse(elements), token, index, out cssX, out cssY);
        }

        static bool TryClassCenter(List<El> els, string token, int index, out double cssX, out double cssY)
        {
            cssX = cssY = 0;
            int seen = 0;
            for (int i = 0; i < els.Count; i++)
            {
                if (!HasToken(els[i].Classes, token) || !els[i].Hittable) continue;
                if (seen == index)
                {
                    cssX = els[i].X + els[i].W * 0.5;
                    cssY = els[i].Y + els[i].H * 0.5;
                    return true;
                }
                seen++;
            }
            return false;
        }

        static int Topmost(List<El> els, double cssX, double cssY)
        {
            for (int i = els.Count - 1; i >= 0; i--)
            {
                var e = els[i];
                if (!e.Hittable) continue;
                if (cssX < e.X || cssY < e.Y || cssX >= e.X + e.W || cssY >= e.Y + e.H) continue;
                return i;
            }
            return -1;
        }

        static NavEdge Match(string stateId, List<El> els, int index)
        {
            var e = els[index];
            for (int r = 0; r < Edges.Count; r++)
            {
                var edge = Edges[r];
                if (edge.StateId != stateId) continue;
                if (edge.Id != null)
                {
                    if (e.Id == edge.Id) return edge;
                    continue;
                }
                if (edge.ClassToken == null || !HasToken(e.Classes, edge.ClassToken)) continue;
                if (edge.ClassIndex < 0 || Ordinal(els, index, edge.ClassToken) == edge.ClassIndex)
                    return edge;
            }
            return null;
        }

        static int Ordinal(List<El> els, int index, string token)
        {
            int n = 0;
            for (int i = 0; i < index; i++)
                if (HasToken(els[i].Classes, token)) n++;
            return n;
        }

        static bool HasToken(string classes, string token)
        {
            if (string.IsNullOrEmpty(classes) || string.IsNullOrEmpty(token)) return false;
            int i = 0;
            while (i < classes.Length)
            {
                while (i < classes.Length && classes[i] == ' ') i++;
                int start = i;
                while (i < classes.Length && classes[i] != ' ') i++;
                if (i - start == token.Length && string.CompareOrdinal(classes, start, token, 0, token.Length) == 0)
                    return true;
            }
            return false;
        }

        sealed class El
        {
            public string Id;
            public string Classes;
            public double X, Y, W, H;
            public int Parent = -1;
            public bool Hittable;
        }

        static List<El> Parse(string specJson)
        {
            var root = (Dictionary<string, object>)OrbitalSalvage.Render.MiniJson.Parse(specJson);
            var raw = (List<object>)root["elements"];
            var list = new List<Dictionary<string, object>>(raw.Count);
            for (int k = 0; k < raw.Count; k++) list.Add((Dictionary<string, object>)raw[k]);
            return Parse(list);
        }

        static List<El> Parse(IReadOnlyList<Dictionary<string, object>> raw)
        {
            var els = new List<El>(raw.Count);
            var byI = new Dictionary<int, int>();
            for (int k = 0; k < raw.Count; k++)
            {
                var e = raw[k];
                var el = new El
                {
                    Id = Str(e, "id"),
                    Classes = Str(e, "c")
                };
                if (e.TryGetValue("r", out var rv) && rv is List<object> rect && rect.Count >= 4)
                {
                    el.X = (double)rect[0];
                    el.Y = (double)rect[1];
                    el.W = (double)rect[2];
                    el.H = (double)rect[3];
                }
                els.Add(el);
                byI[(int)Num(e, "i", k)] = k;
            }

            var skip = new bool[els.Count];
            var eff = new double[els.Count];
            for (int k = 0; k < raw.Count; k++)
            {
                var e = raw[k];
                int parentI = (int)Num(e, "p", -1);
                int pk = parentI >= 0 && byI.TryGetValue(parentI, out var found) ? found : -1;
                els[k].Parent = pk;
                bool parentSkipped = pk >= 0 && skip[pk];
                double parentEff = pk >= 0 ? eff[pk] : 1;
                bool hidden = Num(e, "v", 1) == 0 || Str(e, "vis") == "hidden" || HasToken(els[k].Classes, "page-hidden");
                eff[k] = parentEff * Num(e, "o", 1);
                skip[k] = parentSkipped || hidden;
                // Opacity 0 matches the inactive modal overlays (pointer-events: none). Their full-screen
                // rects must not swallow clicks meant for the page underneath.
                var el = els[k];
                el.Hittable = !skip[k] && eff[k] > 0.001 && el.W > 0 && el.H > 0;
            }
            return els;
        }

        static string Str(Dictionary<string, object> e, string key)
        {
            return e.TryGetValue(key, out var v) && v is string s ? s : "";
        }

        static double Num(Dictionary<string, object> e, string key, double fallback)
        {
            return e.TryGetValue(key, out var v) && v is double d ? d : fallback;
        }
    }
}
