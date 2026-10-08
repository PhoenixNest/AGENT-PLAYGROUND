using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;

namespace OrbitalSalvage.Viewer.Tests
{
    public class MenuPathTests
    {
        [Test]
        public void MenuPaths_UseSpacedOrbitalSalvage()
        {
            int seen = 0;
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
                            string path = ((MenuItem)attrs[a]).menuItem;
                            if (path.IndexOf("Orbital", StringComparison.Ordinal) < 0) continue;
                            seen++;
                            Assert.IsTrue(path.StartsWith("Orbital Salvage/", StringComparison.Ordinal), path);
                            Assert.IsFalse(path.StartsWith("OrbitalSalvage/", StringComparison.Ordinal), path);
                        }
                    }
                }
            }
            Assert.GreaterOrEqual(seen, 4, "expected Play Quality and Parity Viewer items");
            Assert.IsTrue(Menu.GetEnabled("Orbital Salvage/Play Quality/Auto"), "spaced Play Quality menu is not registered");
            Assert.IsFalse(Menu.GetEnabled("OrbitalSalvage/Play Quality/Auto"), "spaceless Play Quality menu is still registered");
        }
    }
}
