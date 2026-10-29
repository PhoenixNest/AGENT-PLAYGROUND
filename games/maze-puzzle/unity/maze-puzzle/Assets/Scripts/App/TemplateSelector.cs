using System.Collections.Generic;
using System.Text;

namespace OrbitalSalvage.App
{
    /// <summary>Controller snapshot the page picker reads. Strings are empty when unset, never null.</summary>
    public struct TemplateQuery
    {
        public string Page;
        public string Modal;
        public int WorldIndex;
        public int ActiveWorld;
        public int WorldDone;
        public int WorldTotal;
        public bool Levels1Through44;
        public string ShopTab;
        public string ShopSignature;
        public bool GiftClaimed;
        public int DailyCleared;
        public bool MasterReady;
        public bool MasterClaimed;
        public bool SoundOn;
        public bool HapticsOn;
        public bool SlidersDefault;
        public bool DebriefDaily;
        public bool DebriefSurplus;
        public bool DefeatUsed;
        public double DefeatRatio;
        public int AchievementsUnlocked;
        public int AchievementsTotal;
        public bool GameDaily;
        /// <summary>Campaign level number of the active sector, or 0 for a daily shift or before a sector is adopted.</summary>
        public int CurrentLevel;
    }

    /// <summary>Maps a controller snapshot to a distilled spec id. <paramref name="cardStamp"/> is set when no web state has this shop inventory.</summary>
    public static class TemplateSelector
    {
        public const string RingsDefault = "ring_donut|ring_donut";
        public const string RingsMixed = "ring_donut,ring_duckie,ring_candyland|ring_candyland";
        public const string SingsDefault = "singularity_nibbles|singularity_nibbles";
        public const string SingsMixed = "singularity_nibbles,singularity_strawberry|singularity_strawberry";
        public const string SkyDefault = "skybox_cotton_candy|skybox_cotton_candy";
        public const string SkyMixed = "skybox_cotton_candy,skybox_cheese_moon|skybox_cheese_moon";

        static readonly string[] Rings = { "ring_donut", "ring_duckie", "ring_candyland", "ring_spacecat" };
        static readonly string[] Sings = { "singularity_nibbles", "singularity_strawberry", "singularity_sleepy", "singularity_confetti" };
        static readonly string[] Skies = { "skybox_cotton_candy", "skybox_cheese_moon", "skybox_mint_soda" };

        public static string Signature(IList<string> unlockedInCatalogOrder, string equipped)
        {
            var sb = new StringBuilder();
            if (unlockedInCatalogOrder != null)
            {
                for (int i = 0; i < unlockedInCatalogOrder.Count; i++)
                {
                    if (sb.Length > 0) sb.Append(',');
                    sb.Append(unlockedInCatalogOrder[i]);
                }
            }
            sb.Append('|').Append(equipped ?? "");
            return sb.ToString();
        }

        public static string Select(TemplateQuery q, out string cardStamp)
        {
            cardStamp = null;
            string modal = q.Modal ?? "";
            if (modal == "debrief") return Debrief(q);
            if (modal == "defeat") return Defeat(q);
            if (modal == "pause") return "modal_pause";
            if (modal == "wardrobe") return "modal_wardrobe";
            if (modal == "language") return "settings_modal_language";
            if (modal == "privacy") return "settings_modal_privacy";
            if (modal == "reset") return "settings_modal_reset_confirm";
            switch (q.Page ?? "")
            {
                case "loading": return "loading_t0500";
                case "home": return "home_default";
                case "maps": return Maps(q);
                case "daily": return Daily(q);
                case "shop": return Shop(q, out cardStamp);
                case "achievements": return Achievements(q);
                case "settings": return Settings(q);
                case "game": return q.GameDaily ? "game_daily_bronze_t1000" : "game_lvl01_t1000ms";
                default: return "home_default";
            }
        }

        static string Debrief(TemplateQuery q)
        {
            if (q.DebriefDaily) return "modal_debrief_daily_trophy";
            if (q.WorldIndex == 5) return "modal_debrief_world5";
            if (q.WorldIndex == 3) return "modal_debrief_world3";
            if (q.DebriefSurplus) return "modal_debrief_surplus";
            return "modal_debrief_exact";
        }

        static string Defeat(TemplateQuery q)
        {
            if (q.DefeatUsed) return "modal_defeat_no_backup_used";
            if (q.DefeatRatio >= 0.70) return "modal_defeat_backup";
            if (q.DefeatRatio < 0.40) return "modal_defeat_no_backup_low";
            return "modal_defeat_12_of_20";
        }

        /// <summary>
        /// Baked world tab and PLAY marker in each captured maps spec.
        /// <c>maps_world_1_all_complete_other_active</c> was described as world 1 with level 11 active,
        /// but the capture followed <c>loadSector(10)</c> onto world 2, so the pixels are the 11–20 tab with level 11 current.
        /// </summary>
        public static bool TryBakedMaps(string templateId, out int worldIndex, out int currentLevel)
        {
            worldIndex = 0;
            currentLevel = 0;
            switch (templateId)
            {
                case "maps_world_1_fresh": worldIndex = 1; currentLevel = 1; return true;
                case "maps_world_1_all_complete": worldIndex = 1; currentLevel = 1; return true;
                case "maps_world_1_all_complete_other_active": worldIndex = 2; currentLevel = 11; return true;
                case "maps_world_2": worldIndex = 2; currentLevel = 14; return true;
                case "maps_world_3": worldIndex = 3; currentLevel = 24; return true;
                case "maps_world_3_none_complete": worldIndex = 3; currentLevel = 21; return true;
                case "maps_world_4": worldIndex = 4; currentLevel = 34; return true;
                case "maps_world_5": worldIndex = 5; currentLevel = 44; return true;
                case "maps_world_5_levels_1_44_done": worldIndex = 5; currentLevel = 44; return true;
                default: return false;
            }
        }

        /// <summary>True when the captured world tab and PLAY marker already match live state, so the page must not be restamped.</summary>
        public static bool MapsCaptureMatches(string templateId, int worldIndex, int currentLevel)
        {
            if (!TryBakedMaps(templateId, out int world, out int level)) return true;
            return world == worldIndex && level == currentLevel;
        }

        static string Maps(TemplateQuery q)
        {
            bool world1Done = q.WorldIndex == 1 && q.WorldTotal > 0 && q.WorldDone >= q.WorldTotal;
            // Pixels are world 2 / level 11, not the world-1 tab. Only select them when that is the live view.
            if (q.WorldIndex == 2 && q.WorldDone == 0 && q.CurrentLevel == 11)
                return "maps_world_1_all_complete_other_active";
            if (world1Done) return "maps_world_1_all_complete";
            if (q.WorldIndex == 3 && q.WorldDone == 0) return "maps_world_3_none_complete";
            if (q.WorldIndex == 5 && q.Levels1Through44) return "maps_world_5_levels_1_44_done";
            if (q.WorldIndex == 2) return "maps_world_2";
            if (q.WorldIndex == 3) return "maps_world_3";
            if (q.WorldIndex == 4) return "maps_world_4";
            if (q.WorldIndex == 5) return "maps_world_5";
            return "maps_world_1_fresh";
        }

        static string Daily(TemplateQuery q)
        {
            if (q.MasterClaimed) return "daily_master_claimed";
            if (q.MasterReady || q.DailyCleared >= 3) return "daily_master_ready";
            if (q.DailyCleared == 2) return "daily_two_cleared";
            if (q.DailyCleared == 1) return "daily_bronze_cleared";
            return "daily_default";
        }

        static string Shop(TemplateQuery q, out string cardStamp)
        {
            cardStamp = null;
            string tab = q.ShopTab ?? "rings";
            string sig = q.ShopSignature ?? "";
            if (tab == "singularities") return Known(sig, SingsDefault, "shop_singularities_default", SingsMixed, "shop_singularities_mixed", Sings, out cardStamp);
            if (tab == "skyboxes") return Known(sig, SkyDefault, "shop_skyboxes_default", SkyMixed, "shop_skyboxes_mixed", Skies, out cardStamp);
            if (sig == RingsMixed) return "shop_rings_mixed";
            if (sig == RingsDefault && q.GiftClaimed) return "shop_gift_claimed";
            if (sig == RingsDefault || sig.Length == 0) return "shop_rings_default";
            cardStamp = Kinds(Rings, sig);
            return q.GiftClaimed ? "shop_gift_claimed" : "shop_rings_default";
        }

        static string Known(string sig, string defSig, string defSpec, string mixSig, string mixSpec, string[] catalog, out string cardStamp)
        {
            cardStamp = null;
            if (sig == mixSig) return mixSpec;
            if (sig == defSig || sig.Length == 0) return defSpec;
            cardStamp = Kinds(catalog, sig);
            return defSpec;
        }

        static string Kinds(string[] catalog, string signature)
        {
            int bar = signature.IndexOf('|');
            string owned = bar < 0 ? signature : signature.Substring(0, bar);
            string equipped = bar < 0 ? "" : signature.Substring(bar + 1);
            var have = new HashSet<string>();
            if (owned.Length > 0)
            {
                var parts = owned.Split(',');
                for (int i = 0; i < parts.Length; i++) have.Add(parts[i]);
            }
            var sb = new StringBuilder();
            for (int i = 0; i < catalog.Length; i++)
            {
                if (i > 0) sb.Append(',');
                string kind = catalog[i] == equipped ? "equipped" : have.Contains(catalog[i]) ? "owned" : "locked";
                sb.Append(catalog[i]).Append(':').Append(kind);
            }
            return sb.ToString();
        }

        static string Achievements(TemplateQuery q)
        {
            if (q.AchievementsUnlocked <= 0) return "achievements_none";
            if (q.AchievementsTotal > 0 && q.AchievementsUnlocked >= q.AchievementsTotal) return "achievements_all";
            return "achievements_some";
        }

        static string Settings(TemplateQuery q)
        {
            if (!q.SlidersDefault) return "settings_sliders_moved";
            if (!q.SoundOn && !q.HapticsOn) return "settings_toggles_off";
            return "settings_default";
        }
    }
}
