using System;
using System.Collections.Generic;

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
        public const int CacheCapacity = 3;
        public const string Profile = "zero";
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

            Edges = list;
        }

        static NavEdge ById(string state, string id, string target) => new NavEdge(state, target, id, null, -1);
        static NavEdge ByClass(string state, string token, string target) => new NavEdge(state, target, null, token, -1);
        static NavEdge ByClassNth(string state, string token, int index, string target) => new NavEdge(state, target, null, token, index);

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
            int hit = -1;
            for (int i = els.Count - 1; i >= 0; i--)
            {
                var e = els[i];
                if (!e.Hittable) continue;
                if (cssX < e.X || cssY < e.Y || cssX >= e.X + e.W || cssY >= e.Y + e.H) continue;
                hit = i;
                break;
            }
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
            var els = new List<El>(raw.Count);
            var byI = new Dictionary<int, int>();
            for (int k = 0; k < raw.Count; k++)
            {
                var e = (Dictionary<string, object>)raw[k];
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
                var e = (Dictionary<string, object>)raw[k];
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
