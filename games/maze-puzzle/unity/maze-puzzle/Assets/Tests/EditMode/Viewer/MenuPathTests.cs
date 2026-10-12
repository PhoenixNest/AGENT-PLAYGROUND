using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using OrbitalSalvage.Viewer.Editor;
using UnityEditor;

namespace OrbitalSalvage.Viewer.Tests
{
    public class MenuPathTests
    {
        static readonly string[] Groups = { "Preview", "Build", "Play", "Localization", "Tools" };

        static readonly string[] CheckedToggles =
        {
            "Orbital Salvage/Play/Quality/Auto",
            "Orbital Salvage/Play/Quality/1x",
            "Orbital Salvage/Play/Quality/1.5x",
            "Orbital Salvage/Play/Quality/2x",
            "Orbital Salvage/Play/Quality/3x",
            "Orbital Salvage/Play/Parity Viewer Enabled"
        };

        struct Entry
        {
            public string Path;
            public bool Validate;
            public int Priority;
        }

        [Test]
        public void Menu_UsesFiveGroupsWithUniquePairedPaths()
        {
            var entries = Collect();
            Assert.GreaterOrEqual(entries.Count, 12, "expected Preview, Build, Play, Localization, and Tools items");

            var pairs = new HashSet<string>();
            var groups = new HashSet<string>();
            var byPath = new Dictionary<string, List<Entry>>();
            foreach (var e in entries)
            {
                Assert.IsTrue(e.Path.StartsWith("Orbital Salvage/", StringComparison.Ordinal), e.Path);
                Assert.IsFalse(e.Path.StartsWith("OrbitalSalvage/", StringComparison.Ordinal), e.Path);
                string key = e.Path + "|" + e.Validate;
                Assert.IsTrue(pairs.Add(key), "duplicate menu item " + key);
                string[] parts = e.Path.Split('/');
                Assert.GreaterOrEqual(parts.Length, 3, e.Path);
                groups.Add(parts[1]);
                if (!byPath.TryGetValue(e.Path, out var list))
                {
                    list = new List<Entry>();
                    byPath[e.Path] = list;
                }
                list.Add(e);
            }

            Assert.AreEqual(Groups.Length, groups.Count, string.Join(",", groups));
            foreach (string group in Groups) Assert.IsTrue(groups.Contains(group), group);

            foreach (string toggle in CheckedToggles)
            {
                Assert.IsTrue(byPath.ContainsKey(toggle), toggle);
                bool action = false, validate = false;
                foreach (var e in byPath[toggle])
                {
                    if (e.Validate) validate = true;
                    else action = true;
                }
                Assert.IsTrue(action && validate, "checked toggle needs an action and a validate pair: " + toggle);
            }

            foreach (var kv in byPath)
            {
                bool action = false, validate = false;
                foreach (var e in kv.Value)
                {
                    if (e.Validate) validate = true;
                    else action = true;
                }
                if (validate) Assert.IsTrue(action, "validate item has no menu action: " + kv.Key);
            }

            var zero = new HashSet<string>();
            foreach (var spec in PageCatalog.LoadZeroSpecs()) zero.Add(spec.Id);
            foreach (var e in entries)
            {
                if (e.Validate) continue;
                if (!e.Path.StartsWith("Orbital Salvage/Preview/", StringComparison.Ordinal)) continue;
                string leaf = e.Path.Substring("Orbital Salvage/Preview/".Length);
                if (leaf == "Open Window") continue;
                Assert.IsTrue(zero.Contains(leaf), "preview item does not name a zero spec: " + e.Path);
            }

            AssertPriorities(entries);
            Assert.IsTrue(Menu.GetEnabled("Orbital Salvage/Play/Quality/Auto"));
            Assert.IsFalse(Menu.GetEnabled("OrbitalSalvage/Play Quality/Auto"));
            Assert.IsFalse(Menu.GetEnabled("Orbital Salvage/Play Quality/Auto"));
        }

        static void AssertPriorities(List<Entry> entries)
        {
            var byGroup = new Dictionary<string, List<int>>();
            foreach (var e in entries)
            {
                if (e.Validate) continue;
                string group = e.Path.Split('/')[1];
                if (!byGroup.TryGetValue(group, out var list))
                {
                    list = new List<int>();
                    byGroup[group] = list;
                }
                list.Add(e.Priority);
            }

            var order = new List<string>(Groups);
            order.Sort((a, b) =>
            {
                int ma = Min(byGroup[a]);
                int mb = Min(byGroup[b]);
                return ma.CompareTo(mb);
            });
            for (int i = 0; i < order.Count; i++)
            {
                var priorities = byGroup[order[i]];
                priorities.Sort();
                for (int p = 1; p < priorities.Count; p++)
                    Assert.Less(priorities[p] - priorities[p - 1], 11, order[i] + " items would draw a separator");
                if (i == 0) continue;
                int gap = Min(priorities) - Max(byGroup[order[i - 1]]);
                Assert.GreaterOrEqual(gap, 11, order[i - 1] + " -> " + order[i]);
            }
        }

        static int Min(List<int> values)
        {
            int m = values[0];
            for (int i = 1; i < values.Count; i++) if (values[i] < m) m = values[i];
            return m;
        }

        static int Max(List<int> values)
        {
            int m = values[0];
            for (int i = 1; i < values.Count; i++) if (values[i] > m) m = values[i];
            return m;
        }

        static List<Entry> Collect()
        {
            var entries = new List<Entry>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types; }
                if (types == null) continue;
                for (int t = 0; t < types.Length; t++)
                {
                    var type = types[t];
                    if (type == null) continue;
                    MethodInfo[] methods;
                    try
                    {
                        methods = type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    }
                    catch (Exception) { continue; }
                    for (int m = 0; m < methods.Length; m++)
                    {
                        var attrs = methods[m].GetCustomAttributes(typeof(MenuItem), false);
                        for (int a = 0; a < attrs.Length; a++)
                        {
                            var item = (MenuItem)attrs[a];
                            if (item.menuItem.IndexOf("Orbital", StringComparison.Ordinal) < 0) continue;
                            entries.Add(new Entry { Path = item.menuItem, Validate = item.validate, Priority = item.priority });
                        }
                    }
                }
            }
            return entries;
        }
    }
}
