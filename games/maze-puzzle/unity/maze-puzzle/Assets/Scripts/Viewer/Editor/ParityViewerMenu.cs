using System;
using System.IO;
using OrbitalSalvage.App;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Editor
{
    /// <summary>Editor opt-out. Default is on. The toggle is read the next time Play mode starts.</summary>
    public static class ParityViewerMenu
    {
        const string MenuPath = "Orbital Salvage/Play/Parity Viewer Enabled";
        const int Priority = 305;

        /// <summary>
        /// When set, tests supply the dialog result. Otherwise batch mode proceeds without a dialog
        /// and the Editor shows a confirmation.
        /// </summary>
        public static Func<bool> ConfirmReset;

        /// <summary>When set, tests point the reset at a temporary file.</summary>
        public static Func<string> ProfilePathOverride;

        [MenuItem(MenuPath, false, Priority)]
        static void Toggle()
        {
            EditorPrefs.SetBool(ParityPages.EnabledPrefKey, !Enabled);
        }

        [MenuItem(MenuPath, true)]
        static bool Validate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }

        static bool Enabled => EditorPrefs.GetBool(ParityPages.EnabledPrefKey, true);

        public static string ProfileFilePath()
        {
            if (ProfilePathOverride != null) return ProfilePathOverride();
            return Path.Combine(Application.persistentDataPath, FileProfileStore.FileName);
        }

        public static bool ProceedWithReset()
        {
            if (ConfirmReset != null) return ConfirmReset();
            if (Application.isBatchMode) return true;
            return EditorUtility.DisplayDialog(
                "Reset Saved Profile",
                "Delete the saved profile?\n\n" + ProfileFilePath(),
                "Reset",
                "Cancel");
        }

        [MenuItem("Orbital Salvage/Tools/Reset Saved Profile", false, 400)]
        public static void ResetProfile()
        {
            string path = ProfileFilePath();
            if (!ProceedWithReset()) return;
            if (File.Exists(path)) File.Delete(path);
            Debug.Log("[ParityViewer] reset profile " + path);
        }
    }
}
