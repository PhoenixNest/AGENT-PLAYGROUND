using System.IO;
using OrbitalSalvage.App;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Editor
{
    /// <summary>Editor opt-out. Default is on. The toggle is read the next time Play mode starts.</summary>
    static class ParityViewerMenu
    {
        const string MenuPath = "Orbital Salvage/Parity Viewer/Enabled";

        [MenuItem(MenuPath, false, 200)]
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

        [MenuItem("Orbital Salvage/Parity Viewer/Reset Profile", false, 201)]
        static void ResetProfile()
        {
            string path = Path.Combine(Application.persistentDataPath, FileProfileStore.FileName);
            if (File.Exists(path)) File.Delete(path);
            Debug.Log("[ParityViewer] reset profile " + path);
        }
    }
}
