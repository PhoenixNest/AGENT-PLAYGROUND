using System;
using System.IO;
using System.Text;
using OrbitalSalvage.Loc;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Editor
{
    /// <summary>
    /// Menu wrapper around <see cref="SpecExtractor"/>. The extractor itself is in the runtime
    /// assembly so EditMode tests can run it without referencing the Editor assembly.
    /// </summary>
    public static class LocExtract
    {
        public const string MenuPath = "Orbital Salvage/Localization/Extract Spec Strings";

        // 320 sits between Play (305) and Tools (400) so the menu-path test keeps its group gap.
        [MenuItem(MenuPath, false, 320)]
        public static void ExtractMenu()
        {
            Run();
        }

        public static void RunBatch()
        {
            bool ok = false;
            try
            {
                ok = Run();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                ok = false;
            }
            if (Application.isBatchMode) EditorApplication.Exit(ok ? 0 : 1);
        }

        public static bool Run()
        {
            string specDir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            SpecScan scan = SpecExtractor.Scan(specDir, "__zero.json");
            string jsonPath = Path.Combine(Application.dataPath, "Resources", "Localization", "en.json");
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            var utf8 = new UTF8Encoding(false);
            File.WriteAllText(jsonPath, scan.EnglishJson, utf8);
            string csvPath = SpecExtractor.HandoffCsvPath(Application.dataPath);
            Directory.CreateDirectory(Path.GetDirectoryName(csvPath));
            File.WriteAllText(csvPath, scan.Csv, utf8);
            Debug.Log("[LocExtract] spec=" + scan.SpecKeys.Count + " skip=" + scan.SkipKeys.Count + " files=" + scan.FileCount);
            return true;
        }
    }
}
