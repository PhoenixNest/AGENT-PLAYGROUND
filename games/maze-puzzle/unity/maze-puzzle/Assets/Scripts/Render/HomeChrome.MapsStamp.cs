using System.Collections.Generic;

namespace OrbitalSalvage.Render
{
    public sealed partial class HomeChrome
    {
        static readonly object Missing = new object();
        bool mapsSnapped;
        bool mapsDirty;
        readonly Dictionary<int, Dictionary<string, object>> mapsFields = new Dictionary<int, Dictionary<string, object>>();
        readonly Dictionary<int, string> mapsRuns = new Dictionary<int, string>();
        readonly Dictionary<int, object> mapsRunLists = new Dictionary<int, object>();

        /// <summary>True when the level tile whose badge is <paramref name="level"/> carries the captured current-level class.</summary>
        public bool MapsLevelMarkedCurrent(int level)
        {
            int tile = TileForLevel(level);
            return tile >= 0 && HasToken(S(elements[tile], "c"), "active-level");
        }

        /// <summary>1-based world tab that currently has the active class, or 0.</summary>
        public int MapsActiveWorldTab()
        {
            int n = 0;
            for (int i = 0; i < elements.Count; i++)
            {
                if (!HasToken(S(elements[i], "c"), "world-tab-btn")) continue;
                n++;
                if (HasToken(S(elements[i], "c"), "active")) return n;
            }
            return 0;
        }

        /// <summary>
        /// Move the baked PLAY tile and world tab onto the live level and tab.
        /// When <paramref name="captureMatches"/>, restore the spec so a matching capture (including maps_world_1_fresh) is unchanged.
        /// </summary>
        public void StampMapsCurrent(int liveWorld, int liveLevel, bool captureMatches)
        {
            if (captureMatches && !mapsDirty) return;
            if (!mapsSnapped) SnapshotMaps();
            RestoreMaps();
            mapsDirty = false;
            if (captureMatches)
            {
                DropMapsCaches();
                return;
            }
            ApplyMaps(liveWorld, liveLevel);
            mapsDirty = true;
            DropMapsCaches();
        }

        void DropMapsCaches()
        {
            InvalidateBannerCaches();
            classOccN = 0;
        }

        void SnapshotMaps()
        {
            mapsSnapped = true;
            var tiles = Tiles();
            for (int t = 0; t < tiles.Count; t++)
            {
                Remember(tiles[t], "c", "bg", "bc");
                int status = StatusNode(tiles[t]);
                if (status >= 0)
                {
                    RememberRun(status);
                    int pe = ChildWithToken(status, "pe");
                    if (pe >= 0) Remember(pe, "v");
                }
            }
            var tabs = Tabs();
            for (int t = 0; t < tabs.Count; t++)
            {
                Remember(tabs[t], "c", "bg", "bc", "col", "sh");
                var desc = new List<int>();
                Descendants(tabs[t], desc);
                for (int d = 0; d < desc.Count; d++) Remember(desc[d], "col");
            }
        }

        void ApplyMaps(int liveWorld, int liveLevel)
        {
            var tiles = Tiles();
            int baked = -1;
            int normal = -1;
            for (int i = 0; i < tiles.Count; i++)
            {
                string cls = S(elements[tiles[i]], "c");
                if (HasToken(cls, "active-level")) baked = tiles[i];
                else if (normal < 0) normal = tiles[i];
            }
            int live = TileForLevel(liveLevel);
            if (live >= 0 && baked >= 0 && live != baked) EnsureStatusRun(live, baked);
            if (baked >= 0 && live != baked)
            {
                if (normal >= 0) CopyFields(normal, baked, "c", "bg", "bc");
                SetStatus(baked, "▶", false);
            }
            if (live >= 0 && live != baked)
            {
                if (baked >= 0) CopyFields(baked, live, "c", "bg", "bc");
                SetStatus(live, " PLAY", true);
            }
            var tabs = Tabs();
            int bakedTab = -1;
            int idleTab = -1;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (HasToken(S(elements[tabs[i]], "c"), "active")) bakedTab = i;
                else if (idleTab < 0) idleTab = i;
            }
            int liveTab = liveWorld - 1;
            if (bakedTab >= 0 && liveTab >= 0 && liveTab < tabs.Count && liveTab != bakedTab)
            {
                SwapTab(tabs[bakedTab], tabs[liveTab], idleTab >= 0 ? tabs[idleTab] : -1);
            }
        }

        void SwapTab(int from, int to, int idle)
        {
            if (idle >= 0) CopyFields(idle, from, "c", "bg", "bc", "col", "sh");
            CopyFields(from, to, "c", "bg", "bc", "col", "sh");
            var fromDesc = new List<int>();
            var toDesc = new List<int>();
            var idleDesc = new List<int>();
            Descendants(from, fromDesc);
            Descendants(to, toDesc);
            if (idle >= 0) Descendants(idle, idleDesc);
            int n = fromDesc.Count < toDesc.Count ? fromDesc.Count : toDesc.Count;
            for (int i = 0; i < n; i++)
            {
                if (idle >= 0 && i < idleDesc.Count) CopyFields(idleDesc[i], fromDesc[i], "col");
                CopyFields(fromDesc[i], toDesc[i], "col");
            }
        }

        void SetStatus(int tile, string text, bool star)
        {
            int status = StatusNode(tile);
            if (status < 0) return;
            SetRun(status, text);
            int pe = ChildWithToken(status, "pe");
            if (pe < 0) return;
            if (star) CopyFields(pe, pe, "v");
            else elements[pe]["v"] = 0d;
        }

        void EnsureStatusRun(int destTile, int donorTile)
        {
            int dest = StatusNode(destTile);
            int donor = StatusNode(donorTile);
            if (dest < 0 || donor < 0) return;
            if (!elements[donor].TryGetValue("runs", out var src) || !(src is List<object> srcRuns) || srcRuns.Count == 0) return;
            if (elements[dest].TryGetValue("runs", out var have) && have is List<object> destRuns && destRuns.Count > 0) return;
            if (!(srcRuns[0] is Dictionary<string, object> srcRun) || !srcRun.TryGetValue("r", out var rr) || !(rr is List<object> box) || box.Count < 4) return;
            double dx = Box(elements[destTile], 0) - Box(elements[donorTile], 0);
            double dy = Box(elements[destTile], 1) - Box(elements[donorTile], 1);
            var clone = new Dictionary<string, object>();
            foreach (var kv in srcRun) clone[kv.Key] = kv.Value;
            clone["r"] = new List<object> { (double)box[0] + dx, (double)box[1] + dy, box[2], box[3] };
            clone["s"] = "";
            elements[dest]["runs"] = new List<object> { clone };
        }

        static double Box(Dictionary<string, object> e, int axis)
        {
            if (!e.TryGetValue("r", out var rv) || !(rv is List<object> box) || box.Count <= axis) return 0;
            return box[axis] is double d ? d : 0;
        }

        void CopyFields(int from, int to, params string[] keys)
        {
            if (!mapsFields.TryGetValue(from, out var snap)) return;
            var dest = elements[to];
            for (int i = 0; i < keys.Length; i++)
            {
                if (!snap.TryGetValue(keys[i], out var value) || ReferenceEquals(value, Missing)) continue;
                dest[keys[i]] = value;
            }
        }

        void Remember(int index, params string[] keys)
        {
            if (mapsFields.ContainsKey(index)) return;
            var src = elements[index];
            var snap = new Dictionary<string, object>();
            for (int i = 0; i < keys.Length; i++)
                snap[keys[i]] = src.TryGetValue(keys[i], out var value) ? value : Missing;
            mapsFields[index] = snap;
        }

        void RememberRun(int index)
        {
            if (mapsRuns.ContainsKey(index)) return;
            mapsRuns[index] = RunText(index);
            mapsRunLists[index] = elements[index].TryGetValue("runs", out var runs) ? runs : Missing;
        }

        void RestoreMaps()
        {
            foreach (var kv in mapsFields)
            {
                var dest = elements[kv.Key];
                foreach (var field in kv.Value)
                {
                    if (ReferenceEquals(field.Value, Missing)) dest.Remove(field.Key);
                    else dest[field.Key] = field.Value;
                }
            }
            foreach (var kv in mapsRunLists)
            {
                if (ReferenceEquals(kv.Value, Missing)) elements[kv.Key].Remove("runs");
                else elements[kv.Key]["runs"] = kv.Value;
            }
            foreach (var kv in mapsRuns) SetRun(kv.Key, kv.Value);
        }

        void SetRun(int index, string text)
        {
            if (!elements[index].TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0) return;
            if (runs[0] is Dictionary<string, object> run) run["s"] = text ?? "";
        }

        string RunText(int index)
        {
            if (!elements[index].TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0) return "";
            if (runs[0] is Dictionary<string, object> run && run.TryGetValue("s", out var s) && s is string text) return text;
            return "";
        }

        List<int> Tiles()
        {
            var list = new List<int>();
            for (int i = 0; i < elements.Count; i++)
            {
                string cls = S(elements[i], "c");
                if (HasToken(cls, "level-tile") && !HasToken(cls, "level-tile-header")) list.Add(i);
            }
            return list;
        }

        List<int> Tabs()
        {
            var list = new List<int>();
            for (int i = 0; i < elements.Count; i++)
                if (HasToken(S(elements[i], "c"), "world-tab-btn")) list.Add(i);
            return list;
        }

        int TileForLevel(int level)
        {
            if (level <= 0) return -1;
            var tiles = Tiles();
            for (int i = 0; i < tiles.Count; i++)
                if (BadgeLevel(tiles[i]) == level) return tiles[i];
            return -1;
        }

        int BadgeLevel(int tile)
        {
            int header = ChildWithToken(tile, "level-tile-header");
            if (header < 0) return 0;
            int badge = ChildWithToken(header, "level-badge");
            if (badge < 0) return 0;
            string text = RunText(badge).Trim();
            int space = text.LastIndexOf(' ');
            if (space < 0) return 0;
            int n;
            return int.TryParse(text.Substring(space + 1), out n) ? n : 0;
        }

        int StatusNode(int tile)
        {
            int header = ChildWithToken(tile, "level-tile-header");
            if (header < 0) return -1;
            int parentI = (int)N(elements[header], "i", -1);
            int blank = -1;
            for (int i = 0; i < elements.Count; i++)
            {
                if ((int)N(elements[i], "p", -2) != parentI) continue;
                if (HasToken(S(elements[i], "c"), "level-badge")) continue;
                if (HasToken(S(elements[i], "c"), "level-name") || HasToken(S(elements[i], "c"), "level-quota")) continue;
                if (RunText(i).Length > 0) return i;
                if (blank < 0) blank = i;
            }
            return blank;
        }

        int ChildWithToken(int parent, string token)
        {
            int parentI = (int)N(elements[parent], "i", -1);
            for (int i = 0; i < elements.Count; i++)
            {
                if ((int)N(elements[i], "p", -2) != parentI) continue;
                if (HasToken(S(elements[i], "c"), token)) return i;
            }
            return -1;
        }

        void Descendants(int parent, List<int> into)
        {
            int parentI = (int)N(elements[parent], "i", -1);
            for (int i = 0; i < elements.Count; i++)
            {
                if ((int)N(elements[i], "p", -2) != parentI) continue;
                into.Add(i);
                Descendants(i, into);
            }
        }
    }
}
