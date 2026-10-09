using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Editor
{
    public readonly struct SpecRef
    {
        public readonly string Id;
        public readonly string Group;
        public readonly bool HasIphone;

        public SpecRef(string id, string group, bool hasIphone)
        {
            Id = id;
            Group = group;
            HasIphone = hasIphone;
        }
    }

    public static class PageCatalog
    {
        public static readonly string[] GroupOrder =
        {
            "home", "maps", "daily", "shop", "achievements", "settings",
            "loading", "game", "pause", "debrief", "defeat", "other"
        };

        /// <summary>One default template id per page. Menu leaves use these ids.</summary>
        public static readonly string[] MenuSpecIds =
        {
            "home_default",
            "maps_world_1_fresh",
            "daily_default",
            "shop_rings_default",
            "achievements_none",
            "settings_default",
            "loading_t0500",
            "game_lvl01_t1000ms",
            "modal_pause",
            "modal_debrief_exact",
            "modal_defeat_12_of_20"
        };

        public static string GroupOf(string id)
        {
            if (string.IsNullOrEmpty(id)) return "other";
            if (id.StartsWith("modal_pause", StringComparison.Ordinal)) return "pause";
            if (id.StartsWith("modal_debrief", StringComparison.Ordinal)) return "debrief";
            if (id.StartsWith("modal_defeat", StringComparison.Ordinal)) return "defeat";
            if (id.StartsWith("home", StringComparison.Ordinal)) return "home";
            if (id.StartsWith("maps", StringComparison.Ordinal)) return "maps";
            if (id.StartsWith("daily", StringComparison.Ordinal)) return "daily";
            if (id.StartsWith("shop", StringComparison.Ordinal)) return "shop";
            if (id.StartsWith("achievements", StringComparison.Ordinal)) return "achievements";
            if (id.StartsWith("settings", StringComparison.Ordinal)) return "settings";
            if (id.StartsWith("loading", StringComparison.Ordinal)) return "loading";
            if (id.StartsWith("game", StringComparison.Ordinal)) return "game";
            return "other";
        }

        public static bool HasIphone(string id)
        {
            return AssetDatabase.LoadAssetAtPath<TextAsset>(SpecPath(id, "iphone")) != null;
        }

        public static bool HasZero(string id)
        {
            return AssetDatabase.LoadAssetAtPath<TextAsset>(SpecPath(id, "zero")) != null;
        }

        public static List<SpecRef> LoadZeroSpecs()
        {
            var list = new List<SpecRef>();
            string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Resources/Parity/Spec" });
            const string suffix = "__zero.json";
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string file = Path.GetFileName(path);
                if (!file.EndsWith(suffix, StringComparison.Ordinal)) continue;
                string id = file.Substring(0, file.Length - suffix.Length);
                list.Add(new SpecRef(id, GroupOf(id), HasIphone(id)));
            }
            list.Sort(Compare);
            return list;
        }

        static int Compare(SpecRef a, SpecRef b)
        {
            int ga = Array.IndexOf(GroupOrder, a.Group);
            int gb = Array.IndexOf(GroupOrder, b.Group);
            if (ga < 0) ga = 99;
            if (gb < 0) gb = 99;
            int c = ga.CompareTo(gb);
            return c != 0 ? c : string.CompareOrdinal(a.Id, b.Id);
        }

        static string SpecPath(string id, string profile)
        {
            return "Assets/Resources/Parity/Spec/" + id + "__" + profile + ".json";
        }
    }
}
