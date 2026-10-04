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
