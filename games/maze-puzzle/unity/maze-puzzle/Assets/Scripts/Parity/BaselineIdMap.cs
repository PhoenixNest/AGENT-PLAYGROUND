using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OrbitalSalvage.Parity
{
    public sealed class IdMapEntry
    {
        public string DomId;
        public string Path;
        public string Descendant;
        public string Text;
        public int Nth;
        public bool IsText;
    }

    public static class BaselineIdMap
    {
        const string Home = "AppRoot/Page_Home/SafeContent/";
        const string Maps = "AppRoot/Page_Maps/SafeContent/";
        const string Daily = "AppRoot/Page_Daily/SafeContent/";
        const string Shop = "AppRoot/Page_Shop/SafeContent/";
        const string Troph = "AppRoot/Page_Trophies/SafeContent/";
        const string Settings = "AppRoot/Page_Settings/SafeContent/";
        const string Hud = "AppRoot/Page_InGameHUD/SafeContent/";
        const string Load = "AppRoot/Page_Loading/SafeContent/";
        const string Pause = "AppRoot/Modal_Pause/SafeContent/ModalCard/";
        const string Victory = "AppRoot/Modal_Victory/SafeContent/ModalCard/";
        const string Defeat = "AppRoot/Modal_Defeat/SafeContent/ModalCard/";

        public static readonly string[] WebStaticIds =
        {
            "mobile-frame", "page-loading", "loading-bar-fill", "loading-tip-text", "page-home", "home-credits-val",
            "btn-home-audio", "home-audio-icon", "btn-home-settings", "home-mascot-stage", "home-diorama-ring",
            "home-diorama-ring-label", "home-diorama-nibbles", "home-diorama-face", "home-hearts-anchor",
            "home-mission-world", "home-mission-goal", "home-mission-name", "btn-home-play", "btn-open-maps-page",
            "btn-open-daily-shift", "dock-daily-badge", "btn-open-shop-page", "shop-dock-badge", "btn-open-trophies-page",
            "page-game", "game-canvas", "btn-game-pause", "hud-quota-capsule", "hud-sector-code", "hud-quota-bar",
            "hud-quota-text", "btn-game-restart", "btn-game-skin", "wardrobe-sheet", "btn-wardrobe-close", "wardrobe-backdrop", "wardrobe-items-list", "wardrobe-tab-rings", "wardrobe-tab-singularities", "wardrobe-tab-skyboxes", "gesture-hint", "page-maps", "btn-back-maps", "maps-credits-val",
            "sectors-page-container", "page-daily", "btn-back-daily", "daily-credits-val", "daily-page-container",
            "page-shop", "btn-back-shop", "shop-credits-val", "daily-gift-banner", "gift-banner-icon",
            "gift-banner-title", "gift-banner-desc", "btn-claim-daily-gift", "store-page-container",
            "page-achievements", "btn-back-achievements", "ach-credits-val", "achievements-page-container",
            "page-settings", "btn-back-settings", "settings-sound-toggle", "settings-sfx-slider",
            "settings-ambience-slider", "settings-haptics-toggle", "btn-open-privacy", "btn-reset-game-data",
            "modal-pause", "btn-close-pause", "pause-level-name", "pause-level-quota", "btn-pause-resume",
            "btn-pause-restart", "btn-pause-sound", "pause-sound-icon", "btn-pause-maps", "btn-pause-home",
            "modal-privacy", "btn-close-privacy", "btn-dismiss-privacy", "modal-reset-confirm", "btn-cancel-reset",
            "btn-confirm-reset", "modal-debrief", "debrief-mass-val", "debrief-credits-val", "btn-debrief-replay",
            "btn-debrief-next", "modal-defeat", "defeat-message", "defeat-mass-val", "defeat-spilled-val",
            "defeat-backup-container", "btn-defeat-backup", "btn-defeat-home", "btn-defeat-retry", "toast-container"
        };

        public static readonly IReadOnlyList<IdMapEntry> Entries = Build();

        public static readonly IReadOnlyDictionary<string, string> SelectorTargets = BuildSelectorTargets();

        static List<IdMapEntry> Build()
        {
            var list = new List<IdMapEntry>();
            void P(string id, string path) => list.Add(new IdMapEntry { DomId = id, Path = path });
            void D(string id, string path, string name, int nth = 0) => list.Add(new IdMapEntry { DomId = id, Path = path, Descendant = name, Nth = nth });
            void T(string id, string path, string contains = null, int nth = 0) => list.Add(new IdMapEntry { DomId = id, Path = path, Text = contains, Nth = nth, IsText = true });

            P("mobile-frame", "AppRoot");
            P("page-loading", "AppRoot/Page_Loading");
            P("loading-bar-fill", Load + "CenterBox/ProgressBarWrapper/ProgressBarFill");
            T("loading-tip-text", Load + "CenterBox", "Calibrating");

            P("page-home", "AppRoot/Page_Home");
            T("home-credits-val", Home + "HomeTopBar/CoinPill");
            P("btn-home-audio", Home + "HomeTopBar/Btn_Sound");
            P("btn-home-settings", Home + "HomeTopBar/Btn_Settings");
            P("home-mascot-stage", Home + "StageArea");
            P("home-diorama-ring", Home + "StageArea/DioramaFloaterRing");
            T("home-diorama-ring-label", Home + "StageArea/DonutBadge");
            P("home-diorama-nibbles", Home + "StageArea/DioramaNibbles");
            P("home-mission-world", Home + "LaunchConsole/WorldPill");
            P("home-mission-goal", Home + "LaunchConsole/GoalPill");
            T("home-mission-name", Home + "LaunchConsole", "LVL");
            P("btn-home-play", Home + "LaunchConsole/Btn_PlayMission");
            P("btn-open-maps-page", Home + "ArcadeDock/Tab_MAPS");
            P("btn-open-daily-shift", Home + "ArcadeDock/Tab_DAILY");
            P("dock-daily-badge", Home + "ArcadeDock/Tab_DAILY/Badge");
            P("btn-open-shop-page", Home + "ArcadeDock/Tab_SHOP");
            P("shop-dock-badge", Home + "ArcadeDock/Tab_SHOP/Badge");
            P("btn-open-trophies-page", Home + "ArcadeDock/Tab_TROPHIES");

            P("page-game", "AppRoot/Page_InGameHUD");
            P("btn-game-pause", Hud + "TopBar/Btn_Pause");
            P("hud-quota-capsule", Hud + "TopBar/UnifiedCapsule");
            P("hud-sector-code", Hud + "TopBar/UnifiedCapsule/LvlBadge");
            P("hud-quota-bar", Hud + "TopBar/UnifiedCapsule/ProgressBarTrack");
            T("hud-quota-text", Hud + "TopBar/UnifiedCapsule", "kg");
            P("btn-game-restart", Hud + "TopBar/Btn_Restart");
            P("btn-game-skin", Hud + "TopBar/Btn_Skin");
            P("wardrobe-sheet", "AppRoot/WardrobeSheet");
            P("btn-wardrobe-close", "AppRoot/WardrobeSheet/Btn_Close");
            P("wardrobe-backdrop", "AppRoot/WardrobeSheet/Backdrop");
            P("wardrobe-items-list", "AppRoot/WardrobeSheet/ItemsList");
            P("wardrobe-tab-rings", "AppRoot/WardrobeSheet/Tabs/Tab_Rings");
            P("wardrobe-tab-singularities", "AppRoot/WardrobeSheet/Tabs/Tab_Singularities");
            P("wardrobe-tab-skyboxes", "AppRoot/WardrobeSheet/Tabs/Tab_Skyboxes");

            P("page-maps", "AppRoot/Page_Maps");
            P("btn-back-maps", Maps + "HeaderBar/Btn_Back");
            T("maps-credits-val", Maps + "HeaderBar/CoinPill");
            P("sectors-page-container", Maps + "MapsContent");

            P("page-daily", "AppRoot/Page_Daily");
            P("btn-back-daily", Daily + "HeaderBar/Btn_Back");
            T("daily-credits-val", Daily + "HeaderBar/CoinPill");
            P("daily-page-container", Daily + "DailyContent");

            P("page-shop", "AppRoot/Page_Shop");
            P("btn-back-shop", Shop + "HeaderBar/Btn_Back");
            T("shop-credits-val", Shop + "HeaderBar/CoinPill");
            P("daily-gift-banner", Shop + "ShopContent/FreeStipendBanner");
            P("gift-banner-icon", Shop + "ShopContent/FreeStipendBanner/IconBox");
            T("gift-banner-title", Shop + "ShopContent/FreeStipendBanner", "Janitor");
            P("gift-banner-desc", Shop + "ShopContent/FreeStipendBanner/DescRow1");
            P("btn-claim-daily-gift", Shop + "ShopContent/FreeStipendBanner/Btn_Claim");
            P("store-page-container", Shop + "ShopContent");

            P("page-achievements", "AppRoot/Page_Trophies");
            P("btn-back-achievements", Troph + "HeaderBar/Btn_Back");
            T("ach-credits-val", Troph + "HeaderBar/CoinPill");
            P("achievements-page-container", Troph + "TrophiesContent");

            P("page-settings", "AppRoot/Page_Settings");
            P("btn-back-settings", Settings + "HeaderBar/Btn_Back");
            D("settings-sound-toggle", Settings + "SettingsContent/Group_Audio", "ToggleSwitch");
            D("settings-sfx-slider", Settings + "SettingsContent/Group_Audio", "Track", 0);
            D("settings-ambience-slider", Settings + "SettingsContent/Group_Audio", "Track", 1);
            D("settings-haptics-toggle", Settings + "SettingsContent/Group_Haptics", "ToggleSwitch");
            P("btn-open-privacy", Settings + "SettingsContent/Group_Privacy/Btn_Policy");
            P("btn-reset-game-data", Settings + "SettingsContent/Group_Data/Btn_Reset");

            P("modal-pause", "AppRoot/Modal_Pause");
            P("btn-close-pause", Pause + "Btn_Close");
            T("pause-level-name", Pause + "LevelInfo", null, 0);
            T("pause-level-quota", Pause + "LevelInfo", null, 1);
            P("btn-pause-resume", Pause + "Btn_Resume");
            P("btn-pause-restart", Pause + "Btn_Restart");
            P("btn-pause-sound", Pause + "Btn_SoundToggle");
            D("pause-sound-icon", Pause + "Btn_SoundToggle", "Emoji_emoji_3d_sound_on");
            P("btn-pause-maps", Pause + "Btn_ChooseSector");
            P("btn-pause-home", Pause + "Btn_ReturnHome");

            P("modal-debrief", "AppRoot/Modal_Victory");
            T("debrief-mass-val", Victory + "StatBox1", null, 1);
            T("debrief-credits-val", Victory + "StatBox2", null, 1);
            P("btn-debrief-replay", Victory + "Btn_PlayAgain");
            P("btn-debrief-next", Victory + "Btn_NextMap");

            P("modal-defeat", "AppRoot/Modal_Defeat");
            T("defeat-message", Defeat, "Nibbles is still");
            T("defeat-mass-val", Defeat + "DefBox1", null, 1);
            T("defeat-spilled-val", Defeat + "DefBox2", null, 1);
            P("btn-defeat-backup", Defeat + "Btn_BackupPod");
            P("btn-defeat-home", Defeat + "Btn_Hangar");
            P("btn-defeat-retry", Defeat + "Btn_TryAgain");

            return list;
        }

        static Dictionary<string, string> BuildSelectorTargets()
        {
            var map = new Dictionary<string, string>
            {
                ["tab-btn--rings"] = Shop + "ShopContent/ShopTabs/Tab_Donut Rings",
                ["tab-btn--singularities"] = Shop + "ShopContent/ShopTabs/Tab_Nibbles Hats",
                ["tab-btn--skyboxes"] = Shop + "ShopContent/ShopTabs/Tab_Skies",
                ["world-tab-btn--1"] = Maps + "MapsContent/WorldTabs/Tab_1-10",
                ["world-tab-btn--2"] = Maps + "MapsContent/WorldTabs/Tab_11-20",
                ["world-tab-btn--3"] = Maps + "MapsContent/WorldTabs/Tab_21-30",
                ["world-tab-btn--4"] = Maps + "MapsContent/WorldTabs/Tab_31-40",
                ["world-tab-btn--5"] = Maps + "MapsContent/WorldTabs/Tab_41-50"
            };
            for (int n = 1; n <= 10; n++) map[$"level-tile--{n}"] = Maps + $"MapsContent/LevelGrid/Tile_Lvl_{n}";
            return map;
        }

        public static string DeriveSelectorName(string selector)
        {
            selector = selector.Trim();
            if (selector.StartsWith("#")) return selector.Substring(1);

            var attr = System.Text.RegularExpressions.Regex.Match(selector, @"^\.([A-Za-z0-9_-]+)\[data-[A-Za-z0-9_-]+=['""]?([^'""\]]+)['""]?\]$");
            if (attr.Success) return attr.Groups[1].Value + "--" + System.Text.RegularExpressions.Regex.Replace(attr.Groups[2].Value, "[^A-Za-z0-9_-]", "-");

            var nth = System.Text.RegularExpressions.Regex.Match(selector, @"^\.([A-Za-z0-9_-]+):nth-child\((\d+)\)$");
            if (nth.Success) return nth.Groups[1].Value + "--" + nth.Groups[2].Value;

            return null;
        }

        public static Transform ResolvePath(Transform root, string path)
        {
            return root != null ? root.Find(path) : null;
        }

        public static Transform Resolve(Transform root, IdMapEntry entry)
        {
            var anchor = ResolvePath(root, entry.Path);
            if (anchor == null) return null;

            if (entry.IsText)
            {
                int seen = 0;
                foreach (var tmp in anchor.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (entry.Text != null && (tmp.text == null || !tmp.text.Contains(entry.Text))) continue;
                    if (seen++ == entry.Nth) return tmp.transform;
                }
                return null;
            }

            if (entry.Descendant != null)
            {
                int seen = 0;
                foreach (var t in anchor.GetComponentsInChildren<Transform>(true))
                {
                    if (t == anchor || t.name != entry.Descendant) continue;
                    if (seen++ == entry.Nth) return t;
                }
                return null;
            }

            return anchor;
        }

        public static Transform FindByName(Transform root, string name)
        {
            if (root == null) return null;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }
    }
}
