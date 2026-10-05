using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Swaps shop card subtrees for the locked / owned / equipped sub-template measured from the web specs.
    /// Slot positions come from <c>CardInstancer</c> so the grid rule lives in one place.
    /// </summary>
    public static class CardStamp
    {
        public static List<Dictionary<string, object>> Extract(List<Dictionary<string, object>> elements, string kind)
        {
            var roots = CardRoots(elements);
            for (int i = 0; i < roots.Count; i++)
            {
                if (KindOf(elements, roots[i]) != kind) continue;
                return CopyTree(elements, roots[i]);
            }
            return null;
        }

        public static double[] Heights(string kindsCsv, IDictionary<string, List<Dictionary<string, object>>> templates)
        {
            if (string.IsNullOrEmpty(kindsCsv) || templates == null) return null;
            var kinds = kindsCsv.Split(',');
            var heights = new double[kinds.Length];
            for (int i = 0; i < kinds.Length; i++)
            {
                if (!templates.TryGetValue(kinds[i], out var tmpl) || tmpl == null || tmpl.Count == 0) return null;
                heights[i] = Rect(tmpl[0])[3];
            }
            return heights;
        }

        public static void ApplyShop(List<Dictionary<string, object>> elements, string kindsCsv, IDictionary<string, List<Dictionary<string, object>>> templates, double[] slotX, double[] slotY)
        {
            if (elements == null || string.IsNullOrEmpty(kindsCsv) || templates == null || slotX == null || slotY == null) return;
            var kinds = kindsCsv.Split(',');
            var roots = CardRoots(elements);
            if (roots.Count != kinds.Length || slotX.Length != kinds.Length || slotY.Length != kinds.Length) return;
            for (int i = 0; i < kinds.Length; i++)
                if (!templates.ContainsKey(kinds[i]) || templates[kinds[i]] == null) return;
            var drop = new HashSet<int>();
            for (int i = 0; i < roots.Count; i++)
            {
                var desc = Descendants(elements, roots[i]);
                for (int d = 0; d < desc.Count; d++) drop.Add(desc[d]);
            }
            var rootIndex = new Dictionary<int, int>();
            for (int i = 0; i < roots.Count; i++) rootIndex[roots[i]] = i;
            var oldToNew = new Dictionary<int, int>();
            var next = new List<Dictionary<string, object>>(elements.Count);
            for (int i = 0; i < elements.Count; i++)
            {
                if (drop.Contains(i)) continue;
                if (rootIndex.TryGetValue(i, out int slot))
                {
                    var tmpl = templates[kinds[slot]];
                    double[] origin = Rect(tmpl[0]);
                    double dx = slotX[slot] - origin[0];
                    double dy = slotY[slot] - origin[1];
                    int baseAt = next.Count;
                    int parent = ParentOf(elements[i]);
                    for (int t = 0; t < tmpl.Count; t++)
                    {
                        var copy = Clone(tmpl[t]);
                        Shift(copy, dx, dy);
                        int local = ParentOf(tmpl[t]);
                        if (local < 0)
                        {
                            if (parent >= 0 && oldToNew.TryGetValue(parent, out int mapped)) copy["p"] = (double)mapped;
                        }
                        else copy["p"] = (double)(baseAt + local);
                        next.Add(copy);
                    }
                    oldToNew[i] = baseAt;
                    continue;
                }
                var kept = Clone(elements[i]);
                int p = ParentOf(elements[i]);
                if (p >= 0 && oldToNew.TryGetValue(p, out int np)) kept["p"] = (double)np;
                oldToNew[i] = next.Count;
                next.Add(kept);
            }
            elements.Clear();
            elements.AddRange(next);
        }

        public static List<int> CardRoots(List<Dictionary<string, object>> elements)
        {
            var roots = new List<int>();
            for (int i = 0; i < elements.Count; i++)
                if (ClassOf(elements[i]) == "store-card") roots.Add(i);
            return roots;
        }

        /// <summary>Every store-card subtree, each reindexed so the card is element 0.</summary>
        public static List<List<Dictionary<string, object>>> ExtractAll(List<Dictionary<string, object>> elements)
        {
            var roots = CardRoots(elements);
            var all = new List<List<Dictionary<string, object>>>(roots.Count);
            for (int i = 0; i < roots.Count; i++)
            {
                var tree = CopyTree(elements, roots[i]);
                if (tree != null && tree.Count > 0) all.Add(tree);
            }
            return all;
        }

        public static string TitleOf(List<Dictionary<string, object>> tree)
        {
            if (tree == null) return "";
            for (int i = 0; i < tree.Count; i++)
            {
                if (!HasToken(ClassOf(tree[i]), "card-title")) continue;
                if (!tree[i].TryGetValue("runs", out var runs) || !(runs is List<object> list)) continue;
                for (int r = 0; r < list.Count; r++)
                {
                    if (!(list[r] is Dictionary<string, object> run)) continue;
                    if (run.TryGetValue("s", out var s) && s is string text && text.Trim().Length > 0) return text.Trim();
                }
            }
            return "";
        }

        /// <summary>
        /// Card for <paramref name="title"/> in <paramref name="kind"/>. Uses that item's measured card when the
        /// state was captured; otherwise keeps the item body (name, description, swatch) and grafts a footer of the requested state.
        /// </summary>
        public static List<Dictionary<string, object>> MatchItem(IList<List<Dictionary<string, object>>> cards, string title, string kind)
        {
            if (cards == null || string.IsNullOrEmpty(title)) return null;
            List<Dictionary<string, object>> exact = null, any = null, donor = null;
            for (int i = 0; i < cards.Count; i++)
            {
                var tree = cards[i];
                if (tree == null || tree.Count == 0) continue;
                string k = KindOf(tree, 0);
                if (donor == null && k == kind) donor = tree;
                if (TitleOf(tree) != title) continue;
                any = tree;
                if (k == kind) exact = tree;
            }
            if (exact != null) return CloneTree(exact);
            if (any == null) return donor == null ? null : CloneTree(donor);
            if (donor == null || KindOf(any, 0) == kind) return CloneTree(any);
            return GraftFooter(any, donor);
        }

        public static void ApplyTrees(List<Dictionary<string, object>> elements, IList<List<Dictionary<string, object>>> trees, double[] slotX, double[] slotY)
        {
            if (elements == null || trees == null || slotX == null || slotY == null) return;
            if (trees.Count != slotX.Length || trees.Count != slotY.Length) return;
            var roots = CardRoots(elements);
            if (roots.Count != trees.Count) return;
            for (int i = 0; i < trees.Count; i++)
                if (trees[i] == null || trees[i].Count == 0) return;
            var drop = new HashSet<int>();
            for (int i = 0; i < roots.Count; i++)
            {
                var desc = Descendants(elements, roots[i]);
                for (int d = 0; d < desc.Count; d++) drop.Add(desc[d]);
            }
            var rootIndex = new Dictionary<int, int>();
            for (int i = 0; i < roots.Count; i++) rootIndex[roots[i]] = i;
            var oldToNew = new Dictionary<int, int>();
            var next = new List<Dictionary<string, object>>(elements.Count);
            for (int i = 0; i < elements.Count; i++)
            {
                if (drop.Contains(i)) continue;
                if (rootIndex.TryGetValue(i, out int slot))
                {
                    oldToNew[i] = PasteTree(next, trees[slot], slotX[slot], slotY[slot], ParentOf(elements[i]));
                    continue;
                }
                var kept = Clone(elements[i]);
                int p = ParentOf(elements[i]);
                if (p >= 0 && oldToNew.TryGetValue(p, out int np)) kept["p"] = (double)np;
                oldToNew[i] = next.Count;
                next.Add(kept);
            }
            elements.Clear();
            elements.AddRange(next);
        }

        public static string KindOf(List<Dictionary<string, object>> elements, int root)
        {
            var desc = Descendants(elements, root);
            for (int i = 0; i < desc.Count; i++)
            {
                string c = ClassOf(elements[desc[i]]);
                if (c.IndexOf("badge-equipped", StringComparison.Ordinal) >= 0) return "equipped";
                if (c.IndexOf("btn-equip-item", StringComparison.Ordinal) >= 0) return "owned";
                if (c.IndexOf("btn-buy-item", StringComparison.Ordinal) >= 0) return "locked";
            }
            return "";
        }

        static List<Dictionary<string, object>> CopyTree(List<Dictionary<string, object>> elements, int root)
        {
            var desc = Descendants(elements, root);
            var order = new List<int> { root };
            order.AddRange(desc);
            order.Sort();
            var local = new Dictionary<int, int>();
            for (int i = 0; i < order.Count; i++) local[order[i]] = i;
            var tree = new List<Dictionary<string, object>>(order.Count);
            for (int i = 0; i < order.Count; i++)
            {
                var copy = Clone(elements[order[i]]);
                int p = ParentOf(elements[order[i]]);
                copy["p"] = p >= 0 && local.ContainsKey(p) ? (double)local[p] : -1d;
                tree.Add(copy);
            }
            return tree;
        }

        static List<int> Descendants(List<Dictionary<string, object>> elements, int root)
        {
            var set = new HashSet<int> { root };
            var list = new List<int>();
            bool grew = true;
            while (grew)
            {
                grew = false;
                for (int i = 0; i < elements.Count; i++)
                {
                    if (set.Contains(i)) continue;
                    int p = ParentOf(elements[i]);
                    if (p >= 0 && set.Contains(p))
                    {
                        set.Add(i);
                        list.Add(i);
                        grew = true;
                    }
                }
            }
            return list;
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
                if (i - start == token.Length && string.CompareOrdinal(classes, start, token, 0, token.Length) == 0) return true;
            }
            return false;
        }

        static int FindToken(List<Dictionary<string, object>> tree, string token)
        {
            for (int i = 0; i < tree.Count; i++)
                if (HasToken(ClassOf(tree[i]), token)) return i;
            return -1;
        }

        static List<Dictionary<string, object>> CloneTree(List<Dictionary<string, object>> tree)
        {
            var copy = new List<Dictionary<string, object>>(tree.Count);
            for (int i = 0; i < tree.Count; i++) copy.Add(Clone(tree[i]));
            return copy;
        }

        static void SetHeight(Dictionary<string, object> e, double height)
        {
            if (e == null || !e.TryGetValue("r", out var v) || !(v is List<object> list) || list.Count < 4) return;
            list[3] = height;
        }

        /// <summary>Keep <paramref name="item"/>'s body and replace its footer with <paramref name="donor"/>'s, shifting the card height by the footer delta.</summary>
        static List<Dictionary<string, object>> GraftFooter(List<Dictionary<string, object>> item, List<Dictionary<string, object>> donor)
        {
            int itemFooter = FindToken(item, "card-footer");
            int donorFooter = FindToken(donor, "card-footer");
            if (itemFooter < 0 || donorFooter < 0) return CloneTree(item);
            double[] itemFoot = Rect(item[itemFooter]);
            double[] donorFoot = Rect(donor[donorFooter]);
            double dh = donorFoot[3] - itemFoot[3];
            var drop = new HashSet<int> { itemFooter };
            var desc = Descendants(item, itemFooter);
            for (int i = 0; i < desc.Count; i++) drop.Add(desc[i]);
            var next = new List<Dictionary<string, object>>(item.Count);
            var map = new Dictionary<int, int>();
            for (int i = 0; i < item.Count; i++)
            {
                if (drop.Contains(i)) continue;
                map[i] = next.Count;
                next.Add(Clone(item[i]));
            }
            for (int i = 0; i < next.Count; i++)
            {
                int p = ParentOf(next[i]);
                if (p >= 0 && map.TryGetValue(p, out int np)) next[i]["p"] = (double)np;
            }
            if (map.ContainsKey(0))
            {
                double[] root = Rect(next[map[0]]);
                SetHeight(next[map[0]], root[3] + dh);
            }
            int parent = map.TryGetValue(ParentOf(item[itemFooter]), out int mappedParent) ? mappedParent : 0;
            var order = new List<int> { donorFooter };
            var donorDesc = Descendants(donor, donorFooter);
            order.AddRange(donorDesc);
            order.Sort();
            int baseAt = next.Count;
            var local = new Dictionary<int, int>();
            for (int i = 0; i < order.Count; i++) local[order[i]] = baseAt + i;
            double dx = itemFoot[0] - donorFoot[0];
            double dy = itemFoot[1] - donorFoot[1];
            for (int i = 0; i < order.Count; i++)
            {
                var copy = Clone(donor[order[i]]);
                Shift(copy, dx, dy);
                int p = ParentOf(donor[order[i]]);
                copy["p"] = local.ContainsKey(p) ? (double)local[p] : (double)parent;
                next.Add(copy);
            }
            return next;
        }

        static int PasteTree(List<Dictionary<string, object>> next, List<Dictionary<string, object>> tmpl, double slotX, double slotY, int parentSpecI)
        {
            double[] origin = Rect(tmpl[0]);
            double dx = slotX - origin[0];
            double dy = slotY - origin[1];
            int maxI = 0;
            for (int i = 0; i < next.Count; i++)
            {
                int id = ParentOf(next[i]);
                if (next[i].TryGetValue("i", out var iv) && iv is double idv) id = (int)idv;
                if (id > maxI) maxI = id;
            }
            var ids = new int[tmpl.Count];
            for (int t = 0; t < tmpl.Count; t++) ids[t] = ++maxI;
            int baseAt = next.Count;
            for (int t = 0; t < tmpl.Count; t++)
            {
                var copy = Clone(tmpl[t]);
                Shift(copy, dx, dy);
                copy["i"] = (double)ids[t];
                int local = ParentOf(tmpl[t]);
                if (local < 0) copy["p"] = parentSpecI >= 0 ? (double)parentSpecI : -1d;
                else if (local < ids.Length) copy["p"] = (double)ids[local];
                next.Add(copy);
            }
            return baseAt;
        }

        static string ClassOf(Dictionary<string, object> e)
        {
            return e != null && e.TryGetValue("c", out var v) && v is string s ? s : "";
        }

        public static int ParentOf(Dictionary<string, object> e)
        {
            if (e == null || !e.TryGetValue("p", out var v) || !(v is double d)) return -1;
            return (int)d;
        }

        public static double[] Rect(Dictionary<string, object> e)
        {
            if (e != null && e.TryGetValue("r", out var v) && v is List<object> list && list.Count >= 4)
                return new[] { Num(list[0]), Num(list[1]), Num(list[2]), Num(list[3]) };
            return new double[4];
        }

        static double Num(object v) => v is double d ? d : 0;

        static void Shift(Dictionary<string, object> e, double dx, double dy)
        {
            ShiftList(e, "r", dx, dy);
            ShiftList(e, "br", dx, dy);
            if (e.TryGetValue("runs", out var runs) && runs is List<object> list)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] is Dictionary<string, object> run)
                    {
                        ShiftList(run, "r", dx, dy);
                        ShiftList(run, "br", dx, dy);
                    }
            }
            if (e.TryGetValue("th", out var th) && th is Dictionary<string, object> thumb) ShiftList(thumb, "r", dx, dy);
            if (e.TryGetValue("bef", out var bef) && bef is Dictionary<string, object> b) ShiftList(b, "r", dx, dy);
            if (e.TryGetValue("aft", out var aft) && aft is Dictionary<string, object> a) ShiftList(a, "r", dx, dy);
        }

        static void ShiftList(Dictionary<string, object> e, string key, double dx, double dy)
        {
            if (!e.TryGetValue(key, out var v) || !(v is List<object> list) || list.Count < 2) return;
            if (list[0] is double x) list[0] = x + dx;
            if (list[1] is double y) list[1] = y + dy;
        }

        public static Dictionary<string, object> Clone(Dictionary<string, object> src)
        {
            var copy = new Dictionary<string, object>(src.Count);
            foreach (var kv in src) copy[kv.Key] = CloneValue(kv.Value);
            return copy;
        }

        static object CloneValue(object v)
        {
            if (v is Dictionary<string, object> d) return Clone(d);
            if (v is List<object> list)
            {
                var copy = new List<object>(list.Count);
                for (int i = 0; i < list.Count; i++) copy.Add(CloneValue(list[i]));
                return copy;
            }
            return v;
        }
    }
}
