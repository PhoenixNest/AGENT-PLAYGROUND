using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Render
{
    public sealed partial class HomeChrome
    {
        static readonly object Missing = new object();
        static readonly string[] MapsDonorSpecs =
        {
            "maps_world_2",
            "maps_world_1_fresh",
            "maps_world_1_all_complete",
            "maps_world_1_all_complete_other_active",
            "maps_world_3",
            "maps_world_3_none_complete",
            "maps_world_4",
            "maps_world_5",
            "maps_world_5_levels_1_44_done"
        };
        static readonly Dictionary<string, MapsDonor[]> mapsDonorCache = new Dictionary<string, MapsDonor[]>();
        static readonly Dictionary<string, ProgressDonor[]> progressDonorCache = new Dictionary<string, ProgressDonor[]>();
        const int KindUnplayed = 0;
        const int KindCompleted = 1;
        const int KindCurrent = 2;
        static readonly string[] MapsNodeKeys = { "r", "c", "bg", "bc", "v", "runs", "emoji" };

        bool mapsSnapped;
        bool mapsDirty;
        string mapsLiveSig;
        readonly Dictionary<int, Dictionary<string, object>> mapsFields = new Dictionary<int, Dictionary<string, object>>();
        readonly Dictionary<int, string> mapsRuns = new Dictionary<int, string>();
        readonly Dictionary<int, object> mapsRunLists = new Dictionary<int, object>();
        readonly Dictionary<Dictionary<string, object>, Dictionary<string, object>> mapsNodeSnaps = new Dictionary<Dictionary<string, object>, Dictionary<string, object>>();
        readonly List<Dictionary<string, object>> mapsInserted = new List<Dictionary<string, object>>();

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
            mapsLiveSig = null;
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

        /// <summary>
        /// Paint every level tile from the live profile. Current sector is PLAY, a completed sector is the
        /// star image, anything else is ▶. Web <c>renderSectorsPage</c> has no locked tile. A capture whose
        /// tiles already match is left untouched, including <c>maps_world_1_fresh</c>.
        /// </summary>
        public void StampMapsCurrent(int liveWorld, int liveLevel, bool captureMatches, ICollection<string> completedSectorIds)
        {
            var done = new HashSet<string>();
            if (completedSectorIds != null)
            {
                foreach (var id in completedSectorIds)
                    if (!string.IsNullOrEmpty(id)) done.Add(id);
            }
            string sig = MapsSig(liveWorld, liveLevel, done);
            if (sig == mapsLiveSig) return;
            bool wasDirty = mapsDirty;
            if (!wasDirty && MapsLiveMatches(liveWorld, liveLevel, done))
            {
                mapsLiveSig = sig;
                return;
            }
            if (!mapsSnapped) SnapshotMaps();
            RestoreMaps();
            mapsDirty = false;
            if (MapsLiveMatches(liveWorld, liveLevel, done))
            {
                mapsLiveSig = sig;
                if (wasDirty) DropMapsCaches();
                return;
            }
            ApplyMapsLive(liveWorld, liveLevel, done);
            mapsDirty = true;
            mapsLiveSig = sig;
            DropMapsCaches();
        }

        public struct MapsTileInfo
        {
            public int Level;
            public double X, Y, Width, Height;
            public double HeaderHeight;
            public double NameY, QuotaY;
            public string StatusText;
            public double StatusX, StatusY, StatusW, StatusH;
            public bool StarVisible;
            public string StarEmoji;
            public double StarX, StarY, StarW, StarH;
            public bool Active;
            public string Background;
            public string Border;
        }

        /// <summary>Level tiles in document order (row-major). Reads the painted spec, including any stamp.</summary>
        public void CopyMapsTiles(List<MapsTileInfo> into)
        {
            if (into == null) return;
            into.Clear();
            var tiles = Tiles();
            for (int i = 0; i < tiles.Count; i++)
            {
                int tile = tiles[i];
                var info = new MapsTileInfo();
                info.Level = BadgeLevel(tile);
                ReadBox(elements[tile], out info.X, out info.Y, out info.Width, out info.Height);
                info.Active = HasToken(S(elements[tile], "c"), "active-level");
                info.Background = S(elements[tile], "bg");
                info.Border = FirstBorder(elements[tile]);
                int header = ChildWithToken(tile, "level-tile-header");
                if (header >= 0)
                {
                    double hx, hy, hw, hh;
                    ReadBox(elements[header], out hx, out hy, out hw, out hh);
                    info.HeaderHeight = hh;
                }
                int name = ChildWithToken(tile, "level-name");
                if (name >= 0)
                {
                    double nx, ny, nw, nh;
                    ReadBox(elements[name], out nx, out ny, out nw, out nh);
                    info.NameY = ny;
                }
                int quota = ChildWithToken(tile, "level-quota");
                if (quota >= 0)
                {
                    double qx, qy, qw, qh;
                    ReadBox(elements[quota], out qx, out qy, out qw, out qh);
                    info.QuotaY = qy;
                }
                int status = StatusNode(tile);
                if (status >= 0)
                {
                    info.StatusText = RunText(status) ?? "";
                    ReadBox(elements[status], out info.StatusX, out info.StatusY, out info.StatusW, out info.StatusH);
                    int pe = ChildWithToken(status, "pe");
                    if (pe >= 0)
                    {
                        ReadBox(elements[pe], out info.StarX, out info.StarY, out info.StarW, out info.StarH);
                        info.StarEmoji = S(elements[pe], "emoji");
                        double vis = N(elements[pe], "v", 1);
                        info.StarVisible = vis != 0 && info.StarW > 1 && info.StarH > 1 && info.StarEmoji.Length > 0;
                    }
                }
                if (info.StatusText == null) info.StatusText = "";
                if (info.StarEmoji == null) info.StarEmoji = "";
                if (info.Background == null) info.Background = "";
                if (info.Border == null) info.Border = "";
                into.Add(info);
            }
        }

        public double MapsGridHeight()
        {
            int grid = GridNode();
            return grid < 0 ? 0 : Box(elements[grid], 3);
        }

        int GridNode()
        {
            for (int i = 0; i < elements.Count; i++)
                if (HasToken(S(elements[i], "c"), "level-grid-50")) return i;
            return -1;
        }

        static void ReadBox(Dictionary<string, object> e, out double x, out double y, out double w, out double h)
        {
            x = y = w = h = 0;
            if (e == null || !e.TryGetValue("r", out var rv) || !(rv is List<object> box) || box.Count < 4) return;
            x = box[0] is double dx ? dx : 0;
            y = box[1] is double dy ? dy : 0;
            w = box[2] is double dw ? dw : 0;
            h = box[3] is double dh ? dh : 0;
        }

        static string FirstBorder(Dictionary<string, object> e)
        {
            if (e == null || !e.TryGetValue("bc", out var v) || v == null) return "";
            if (v is string s) return s;
            if (v is List<object> list && list.Count > 0 && list[0] is string first) return first;
            return "";
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
            for (int t = 0; t < tiles.Count; t++) SnapshotTree(tiles[t]);
            int progress = ProgressNode();
            if (progress >= 0) SnapshotTree(progress);
            int grid = GridNode();
            if (grid >= 0)
            {
                RememberNode(elements[grid]);
                double bottom = Box(elements[grid], 1) + Box(elements[grid], 3);
                int parent = (int)N(elements[grid], "p", -2);
                for (int i = 0; i < elements.Count; i++)
                {
                    if (i == grid) continue;
                    if ((int)N(elements[i], "p", -2) != parent) continue;
                    if (Box(elements[i], 1) + 0.5 < bottom) continue;
                    SnapshotTree(i);
                }
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
            ApplyWorldTab(liveWorld);
        }

        void ApplyWorldTab(int liveWorld)
        {
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
                SwapTab(tabs[bakedTab], tabs[liveTab], idleTab >= 0 ? tabs[idleTab] : -1);
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
            for (int i = mapsInserted.Count - 1; i >= 0; i--) elements.Remove(mapsInserted[i]);
            mapsInserted.Clear();
            foreach (var kv in mapsNodeSnaps) RestoreNode(kv.Key, kv.Value);
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

        sealed class MapsDonor
        {
            public double TileH;
            public double HeaderDx, HeaderDy, HeaderH;
            public double BadgeDy;
            public double NameDy, QuotaDy;
            public double StatusW, StatusH;
            public string StatusText;
            public bool HasRun;
            public double RunDx, RunDy, RunW, RunH;
            public bool HasPe;
            public string Emoji;
            public double PeW, PeH, PeDx, PeDy;
            public string TileClass;
            public string Bg;
            public List<object> Bc;
        }

        void ApplyMapsLive(int liveWorld, int liveLevel, HashSet<string> done)
        {
            var donors = MapsDonors(Profile);
            var tiles = Tiles();
            if (tiles.Count == 0 || donors[KindUnplayed] == null || donors[KindCompleted] == null || donors[KindCurrent] == null)
            {
                ApplyWorldTab(liveWorld);
                return;
            }
            int n = tiles.Count;
            var tileEls = new List<Dictionary<string, object>>(n);
            var kinds = new int[n];
            var intrinsic = new double[n];
            for (int i = 0; i < n; i++)
            {
                tileEls.Add(elements[tiles[i]]);
                int level = BadgeLevel(tiles[i]);
                int kind = WantedKind(level, liveLevel, done);
                kinds[i] = kind;
                intrinsic[i] = donors[kind].TileH;
            }
            int rows = (n + 1) / 2;
            var rowY = new double[rows];
            var rowH = new double[rows];
            double y = Box(tileEls[0], 1);
            for (int r = 0; r < rows; r++)
            {
                int a = r * 2;
                int b = a + 1;
                double h = intrinsic[a];
                if (b < n && intrinsic[b] > h) h = intrinsic[b];
                rowY[r] = y;
                rowH[r] = h;
                y += h + 10;
            }
            for (int i = 0; i < n; i++)
                PlaceTile(tileEls[i], rowY[i / 2], rowH[i / 2], donors[kinds[i]]);
            ReflowGrid(rowY[rows - 1] + rowH[rows - 1]);
            ApplyWorldTab(liveWorld);
            ApplyWorldProgress(liveWorld, done);
        }

        /// <summary>Captured banner text, including the leading space (" 4/10").</summary>
        public string MapsBannerProgress()
        {
            int node = ProgressNode();
            return node < 0 ? "" : RunText(node);
        }

        public void CopyMapsBanner(out string text, out double x, out double y, out double w, out double h)
        {
            text = "";
            x = y = w = h = 0;
            int node = ProgressNode();
            if (node < 0) return;
            text = RunText(node);
            ReadBox(elements[node], out x, out y, out w, out h);
        }

        int ProgressNode()
        {
            for (int i = 0; i < elements.Count; i++)
                if (HasToken(S(elements[i], "c"), "world-banner-progress")) return i;
            return -1;
        }

        static int WorldCompleted(int world, HashSet<string> done)
        {
            if (done == null || world < 1) return 0;
            int start = (world - 1) * 10 + 1;
            int n = 0;
            for (int level = start; level < start + 10; level++)
                if (done.Contains("level_" + level)) n++;
            return n;
        }

        static string ExpectedProgress(int world, HashSet<string> done)
        {
            return " " + WorldCompleted(world, done).ToString(System.Globalization.CultureInfo.InvariantCulture) + "/10";
        }

        void ApplyWorldProgress(int liveWorld, HashSet<string> done)
        {
            int node = ProgressNode();
            if (node < 0 || liveWorld < 1) return;
            int count = WorldCompleted(liveWorld, done);
            string text = ExpectedProgress(liveWorld, done);
            var donors = ProgressDonors(Profile);
            ProgressDonor donor = donors == null ? null : (count >= 10 ? donors[1] : donors[0]);
            if (donor == null)
            {
                SetRun(node, text);
                return;
            }
            ReadBox(elements[node], out double x, out double y, out double w, out double h);
            double right = x + w;
            double newX = right - donor.PillW;
            SetBox(elements[node], newX, y, donor.PillW, h);
            if (elements[node].TryGetValue("runs", out var rv) && rv is List<object> runs && runs.Count > 0
                && runs[0] is Dictionary<string, object> run)
            {
                run["s"] = text;
                run["r"] = new List<object> { newX + donor.TextDx, y + donor.TextDy, donor.TextW, donor.TextH };
            }
            int pe = ChildWithToken(node, "pe");
            if (pe >= 0 && donor.HasStar)
                SetBox(elements[pe], newX + donor.StarDx, y + donor.StarDy, donor.StarW, donor.StarH);
        }

        sealed class ProgressDonor
        {
            public double PillW, TextW, TextDx, TextDy, TextH, StarDx, StarDy, StarW, StarH;
            public bool HasStar;
        }

        static ProgressDonor[] ProgressDonors(string profile)
        {
            if (string.IsNullOrEmpty(profile)) profile = "zero";
            if (progressDonorCache.TryGetValue(profile, out var cached)) return cached;
            var pair = new ProgressDonor[2];
            pair[0] = MeasureProgress(LoadElements("maps_world_1_fresh", profile));
            pair[1] = MeasureProgress(LoadElements("maps_world_1_all_complete", profile));
            progressDonorCache[profile] = pair;
            return pair;
        }

        static ProgressDonor MeasureProgress(List<Dictionary<string, object>> els)
        {
            if (els == null) return null;
            int node = -1;
            for (int i = 0; i < els.Count; i++)
            {
                if (HasToken(S(els[i], "c"), "world-banner-progress")) { node = i; break; }
            }
            if (node < 0) return null;
            var donor = new ProgressDonor();
            ReadBox(els[node], out double x, out double y, out double w, out double h);
            donor.PillW = w;
            if (els[node].TryGetValue("runs", out var rv) && rv is List<object> runs && runs.Count > 0
                && runs[0] is Dictionary<string, object> run && run.TryGetValue("r", out var rr)
                && rr is List<object> box && box.Count >= 4)
            {
                double rx = box[0] is double dx ? dx : 0;
                double ry = box[1] is double dy ? dy : 0;
                donor.TextDx = rx - x;
                donor.TextDy = ry - y;
                donor.TextW = box[2] is double dw ? dw : 0;
                donor.TextH = box[3] is double dh ? dh : 0;
            }
            int pe = FindChild(els, node, "pe");
            if (pe >= 0)
            {
                ReadBox(els[pe], out double px, out double py, out double pw, out double ph);
                if (pw > 1 && ph > 1)
                {
                    donor.HasStar = true;
                    donor.StarDx = px - x;
                    donor.StarDy = py - y;
                    donor.StarW = pw;
                    donor.StarH = ph;
                }
            }
            return donor;
        }

        void PlaceTile(Dictionary<string, object> tileEl, double rowY, double rowH, MapsDonor donor)
        {
            int tile = elements.IndexOf(tileEl);
            if (tile < 0) return;
            ReadBox(tileEl, out double tx, out double ty, out double tw, out double th);
            SetBox(tileEl, tx, rowY, tw, rowH);
            if (!string.IsNullOrEmpty(donor.TileClass)) tileEl["c"] = donor.TileClass;
            if (!string.IsNullOrEmpty(donor.Bg)) tileEl["bg"] = donor.Bg;
            if (donor.Bc != null) tileEl["bc"] = CloneJson(donor.Bc);
            int header = ChildWithToken(tile, "level-tile-header");
            if (header < 0) return;
            double headerX = tx + donor.HeaderDx;
            double headerY = rowY + donor.HeaderDy;
            double headerW = tw - donor.HeaderDx * 2;
            SetBox(elements[header], headerX, headerY, headerW, donor.HeaderH);
            int badge = ChildWithToken(header, "level-badge");
            if (badge >= 0)
            {
                ReadBox(elements[badge], out double bx, out double by, out double bw, out double bh);
                double newBadgeY = headerY + donor.BadgeDy;
                SetBox(elements[badge], bx, newBadgeY, bw, bh);
                ShiftRunsY(elements[badge], newBadgeY - by);
            }
            int name = ChildWithToken(tile, "level-name");
            int quota = ChildWithToken(tile, "level-quota");
            int status = StatusNode(tile);
            if (name >= 0)
            {
                ReadBox(elements[name], out double nx, out double ny, out double nw, out double nh);
                double newNameY = rowY + donor.NameDy;
                SetBox(elements[name], nx, newNameY, nw, nh);
                ShiftRunsY(elements[name], newNameY - ny);
            }
            if (quota >= 0)
            {
                ReadBox(elements[quota], out double qx, out double qy, out double qw, out double qh);
                double newQuotaY = rowY + donor.QuotaDy;
                SetBox(elements[quota], qx, newQuotaY, qw, qh);
                ShiftRunsY(elements[quota], newQuotaY - qy);
            }
            if (status < 0) return;
            double statusX = headerX + headerW - donor.StatusW;
            double statusY = headerY;
            SetBox(elements[status], statusX, statusY, donor.StatusW, donor.StatusH);
            var runs = new List<object>();
            if (donor.HasRun)
            {
                var run = new Dictionary<string, object>();
                run["s"] = donor.StatusText ?? "";
                run["r"] = new List<object> { statusX + donor.RunDx, statusY + donor.RunDy, donor.RunW, donor.RunH };
                runs.Add(run);
            }
            elements[status]["runs"] = runs;
            PlacePe(status, donor, statusX, statusY);
        }

        void PlacePe(int status, MapsDonor donor, double statusX, double statusY)
        {
            int pe = ChildWithToken(status, "pe");
            if (!donor.HasPe)
            {
                if (pe >= 0) SetBox(elements[pe], statusX, statusY, 0, 0);
                return;
            }
            double x = statusX + donor.PeDx;
            double y = statusY + donor.PeDy;
            if (pe < 0)
            {
                var node = new Dictionary<string, object>();
                node["t"] = "img";
                node["c"] = "pe";
                node["emoji"] = donor.Emoji ?? "";
                node["i"] = (double)(MaxElementI() + 1);
                node["p"] = elements[status]["i"];
                node["r"] = new List<object> { x, y, donor.PeW, donor.PeH };
                int at = elements.IndexOf(elements[status]) + 1;
                if (at < 1) at = elements.Count;
                elements.Insert(at, node);
                mapsInserted.Add(node);
                return;
            }
            elements[pe]["emoji"] = donor.Emoji ?? "";
            if (elements[pe].ContainsKey("v")) elements[pe].Remove("v");
            SetBox(elements[pe], x, y, donor.PeW, donor.PeH);
        }

        void ReflowGrid(double bottom)
        {
            int grid = GridNode();
            if (grid < 0) return;
            ReadBox(elements[grid], out double gx, out double gy, out double gw, out double gh);
            double newH = bottom - gy;
            if (newH < 0) newH = 0;
            double delta = newH - gh;
            SetBox(elements[grid], gx, gy, gw, newH);
            if (Math.Abs(delta) < 0.01) return;
            int parent = (int)N(elements[grid], "p", -2);
            double oldBottom = gy + gh;
            var roots = new List<int>();
            for (int i = 0; i < elements.Count; i++)
            {
                if (i == grid) continue;
                if ((int)N(elements[i], "p", -2) != parent) continue;
                if (Box(elements[i], 1) + 0.5 < oldBottom) continue;
                roots.Add(i);
            }
            for (int i = 0; i < roots.Count; i++) ShiftTree(roots[i], delta);
        }

        void ShiftTree(int index, double dy)
        {
            ShiftElementY(elements[index], dy);
            var desc = new List<int>();
            Descendants(index, desc);
            for (int i = 0; i < desc.Count; i++) ShiftElementY(elements[desc[i]], dy);
        }

        static void ShiftElementY(Dictionary<string, object> el, double dy)
        {
            if (el == null || Math.Abs(dy) < 1e-9) return;
            ReadBox(el, out double x, out double y, out double w, out double h);
            SetBox(el, x, y + dy, w, h);
            ShiftRunsY(el, dy);
        }

        static void ShiftRunsY(Dictionary<string, object> el, double dy)
        {
            if (el == null || Math.Abs(dy) < 1e-9) return;
            if (!el.TryGetValue("runs", out var rv) || !(rv is List<object> runs)) return;
            for (int i = 0; i < runs.Count; i++)
            {
                if (!(runs[i] is Dictionary<string, object> run)) continue;
                if (!run.TryGetValue("r", out var rr) || !(rr is List<object> box) || box.Count < 2) continue;
                double y = box[1] is double d ? d : 0;
                box[1] = y + dy;
            }
        }

        static void SetBox(Dictionary<string, object> el, double x, double y, double w, double h)
        {
            el["r"] = new List<object> { x, y, w, h };
        }

        void SnapshotTree(int index)
        {
            if (index < 0 || index >= elements.Count) return;
            RememberNode(elements[index]);
            var desc = new List<int>();
            Descendants(index, desc);
            for (int i = 0; i < desc.Count; i++) RememberNode(elements[desc[i]]);
        }

        void RememberNode(Dictionary<string, object> el)
        {
            if (el == null || mapsNodeSnaps.ContainsKey(el)) return;
            mapsNodeSnaps[el] = (Dictionary<string, object>)CloneJson(el);
        }

        static void RestoreNode(Dictionary<string, object> el, Dictionary<string, object> snap)
        {
            if (el == null || snap == null) return;
            for (int i = 0; i < MapsNodeKeys.Length; i++)
            {
                string key = MapsNodeKeys[i];
                if (snap.ContainsKey(key)) el[key] = CloneJson(snap[key]);
                else el.Remove(key);
            }
        }

        bool MapsLiveMatches(int liveWorld, int liveLevel, HashSet<string> done)
        {
            if (liveWorld >= 1 && liveWorld <= 5 && MapsActiveWorldTab() != liveWorld) return false;
            var tiles = Tiles();
            if (tiles.Count == 0) return false;
            for (int i = 0; i < tiles.Count; i++)
            {
                int level = BadgeLevel(tiles[i]);
                if (VisualKind(tiles[i]) != WantedKind(level, liveLevel, done)) return false;
            }
            if (ProgressNode() >= 0 && RunText(ProgressNode()) != ExpectedProgress(liveWorld, done)) return false;
            return true;
        }

        int VisualKind(int tile)
        {
            if (HasToken(S(elements[tile], "c"), "active-level")) return KindCurrent;
            int status = StatusNode(tile);
            string text = status >= 0 ? RunText(status) : "";
            if (text.IndexOf("PLAY", StringComparison.Ordinal) >= 0) return KindCurrent;
            if (text.IndexOf('▶') >= 0) return KindUnplayed;
            if (status >= 0 && StarShown(status)) return KindCompleted;
            return KindUnplayed;
        }

        bool StarShown(int status)
        {
            int pe = ChildWithToken(status, "pe");
            if (pe < 0) return false;
            ReadBox(elements[pe], out double x, out double y, out double w, out double h);
            if (w <= 1 || h <= 1) return false;
            if (S(elements[pe], "emoji").Length == 0) return false;
            return N(elements[pe], "v", 1) != 0;
        }

        static int WantedKind(int level, int liveLevel, HashSet<string> done)
        {
            if (level > 0 && level == liveLevel) return KindCurrent;
            if (level > 0 && done != null && done.Contains("level_" + level)) return KindCompleted;
            return KindUnplayed;
        }

        static string MapsSig(int world, int level, HashSet<string> done)
        {
            var ids = new List<string>(done);
            ids.Sort(StringComparer.Ordinal);
            return world.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ":" + level.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + ":" + string.Join(",", ids);
        }

        static MapsDonor[] MapsDonors(string profile)
        {
            if (string.IsNullOrEmpty(profile)) profile = "zero";
            if (mapsDonorCache.TryGetValue(profile, out var cached)) return cached;
            var donors = new MapsDonor[3];
            for (int s = 0; s < MapsDonorSpecs.Length; s++)
            {
                var els = LoadElements(MapsDonorSpecs[s], profile);
                if (els == null) continue;
                var tiles = new List<int>();
                CollectTiles(els, tiles);
                for (int i = 0; i < tiles.Count; i++)
                {
                    int kind = KindOf(els, tiles[i]);
                    ReadBox(els[tiles[i]], out double tx, out double ty, out double tw, out double th);
                    if (donors[kind] != null && th >= donors[kind].TileH - 0.01) continue;
                    donors[kind] = MeasureDonor(els, tiles[i]);
                }
                if (donors[KindUnplayed] != null && donors[KindCompleted] != null && donors[KindCurrent] != null) break;
            }
            mapsDonorCache[profile] = donors;
            return donors;
        }

        static void CollectTiles(List<Dictionary<string, object>> els, List<int> into)
        {
            for (int i = 0; i < els.Count; i++)
            {
                string cls = S(els[i], "c");
                if (HasToken(cls, "level-tile") && !HasToken(cls, "level-tile-header")) into.Add(i);
            }
        }

        static int KindOf(List<Dictionary<string, object>> els, int tile)
        {
            if (HasToken(S(els[tile], "c"), "active-level")) return KindCurrent;
            int status = FindStatus(els, tile);
            string text = status >= 0 ? RunOf(els, status) : "";
            if (text.IndexOf("PLAY", StringComparison.Ordinal) >= 0) return KindCurrent;
            if (text.IndexOf('▶') >= 0) return KindUnplayed;
            if (status >= 0 && StarShownIn(els, status)) return KindCompleted;
            return KindUnplayed;
        }

        static bool StarShownIn(List<Dictionary<string, object>> els, int status)
        {
            int pe = FindChild(els, status, "pe");
            if (pe < 0) return false;
            ReadBox(els[pe], out double x, out double y, out double w, out double h);
            if (w <= 1 || h <= 1) return false;
            if (S(els[pe], "emoji").Length == 0) return false;
            double vis = N(els[pe], "v", 1);
            return vis != 0;
        }

        static MapsDonor MeasureDonor(List<Dictionary<string, object>> els, int tile)
        {
            var donor = new MapsDonor();
            ReadBox(els[tile], out double tx, out double ty, out double tw, out double th);
            donor.TileH = th;
            donor.TileClass = S(els[tile], "c");
            donor.Bg = S(els[tile], "bg");
            if (els[tile].TryGetValue("bc", out var bc) && bc is List<object>)
                donor.Bc = (List<object>)CloneJson(bc);
            int header = FindChild(els, tile, "level-tile-header");
            if (header >= 0)
            {
                ReadBox(els[header], out double hx, out double hy, out double hw, out double hh);
                donor.HeaderDx = hx - tx;
                donor.HeaderDy = hy - ty;
                donor.HeaderH = hh;
                int badge = FindChild(els, header, "level-badge");
                if (badge >= 0)
                {
                    ReadBox(els[badge], out double bx, out double by, out double bw, out double bh);
                    donor.BadgeDy = by - hy;
                }
            }
            int name = FindChild(els, tile, "level-name");
            if (name >= 0)
            {
                ReadBox(els[name], out double nx, out double ny, out double nw, out double nh);
                donor.NameDy = ny - ty;
            }
            int quota = FindChild(els, tile, "level-quota");
            if (quota >= 0)
            {
                ReadBox(els[quota], out double qx, out double qy, out double qw, out double qh);
                donor.QuotaDy = qy - ty;
            }
            int status = FindStatus(els, tile);
            if (status >= 0)
            {
                ReadBox(els[status], out double sx, out double sy, out double sw, out double sh);
                donor.StatusW = sw;
                donor.StatusH = sh;
                donor.StatusText = RunOf(els, status);
                if (els[status].TryGetValue("runs", out var rv) && rv is List<object> runs && runs.Count > 0 && runs[0] is Dictionary<string, object> run
                    && run.TryGetValue("r", out var rr) && rr is List<object> box && box.Count >= 4)
                {
                    donor.HasRun = true;
                    double rx = box[0] is double dx ? dx : 0;
                    double ry = box[1] is double dy ? dy : 0;
                    donor.RunDx = rx - sx;
                    donor.RunDy = ry - sy;
                    donor.RunW = box[2] is double dw ? dw : 0;
                    donor.RunH = box[3] is double dh ? dh : 0;
                }
                int pe = FindChild(els, status, "pe");
                if (pe >= 0)
                {
                    ReadBox(els[pe], out double px, out double py, out double pw, out double ph);
                    string emoji = S(els[pe], "emoji");
                    if (emoji.Length > 0 && pw > 1 && ph > 1)
                    {
                        donor.HasPe = true;
                        donor.Emoji = emoji;
                        donor.PeW = pw;
                        donor.PeH = ph;
                        donor.PeDx = px - sx;
                        donor.PeDy = py - sy;
                    }
                }
            }
            return donor;
        }

        static int FindChild(List<Dictionary<string, object>> els, int parent, string token)
        {
            if (parent < 0) return -1;
            int parentI = (int)N(els[parent], "i", -1);
            for (int i = 0; i < els.Count; i++)
            {
                if ((int)N(els[i], "p", -2) != parentI) continue;
                if (HasToken(S(els[i], "c"), token)) return i;
            }
            return -1;
        }

        static int FindStatus(List<Dictionary<string, object>> els, int tile)
        {
            int header = FindChild(els, tile, "level-tile-header");
            if (header < 0) return -1;
            int parentI = (int)N(els[header], "i", -1);
            int blank = -1;
            for (int i = 0; i < els.Count; i++)
            {
                if ((int)N(els[i], "p", -2) != parentI) continue;
                if (HasToken(S(els[i], "c"), "level-badge")) continue;
                if (HasToken(S(els[i], "c"), "level-name") || HasToken(S(els[i], "c"), "level-quota")) continue;
                if (RunOf(els, i).Length > 0) return i;
                if (blank < 0) blank = i;
            }
            return blank;
        }

        static string RunOf(List<Dictionary<string, object>> els, int index)
        {
            if (index < 0 || !els[index].TryGetValue("runs", out var rv) || !(rv is List<object> runs) || runs.Count == 0) return "";
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
