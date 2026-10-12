using System;
using System.IO;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Render;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Editor
{
    using Loc = OrbitalSalvage.Loc.Loc;

    /// <summary>
    /// Tools-menu entry for <see cref="LocOverflowAudit"/>. Priority 401 stays inside the Tools group.
    /// </summary>
    public static class LocOverflowMenu
    {
        public const string MenuPath = "Orbital Salvage/Tools/Localization Overflow Audit";

        [MenuItem(MenuPath, false, 401)]
        public static void Menu()
        {
            string path = EditorUtility.SaveFilePanel("Overflow audit", "", "loc-overflow.txt", "txt");
            if (string.IsNullOrEmpty(path)) return;
            Run(path, "qps");
        }

        public static void RunBatch()
        {
            string path = Arg("-loc-audit-out", Path.Combine("/tmp/maze-i18n/loc-render", "overflow.txt"));
            string lang = Arg("-loc-audit-lang", "qps");
            bool ok = false;
            try
            {
                ok = Run(path, lang);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ok = false;
            }
            if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
        }

        public static bool Run(string path, string language)
        {
            Loc.SetLoader(new ResourcesLocTableLoader());
            if (!Loc.SetDiagnostic(language))
            {
                Debug.LogError("[LocOverflow] language did not load: " + language);
                return false;
            }
            string dir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            LocOverflowAudit.Report report = LocOverflowAudit.Run(dir);
            string folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            File.WriteAllText(path, report.Text);
            Debug.Log("[LocOverflow] " + report.Text.Split('\n')[0] + " -> " + path);
            return true;
        }

        static string Arg(string name, string fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return fallback;
        }
    }
}
