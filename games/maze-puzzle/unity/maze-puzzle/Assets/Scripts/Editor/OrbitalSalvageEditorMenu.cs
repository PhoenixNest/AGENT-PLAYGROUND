using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using OrbitalSalvage.UI;
using OrbitalSalvage.Core;

namespace OrbitalSalvage.Editor
{
    /// <summary>
    /// Categorized Unity Editor Menu for Orbital Salvage.
    /// Provides 1-click Edit-Mode UI Previews (no Play Mode required),
    /// hierarchical submenus, and complete project build actions.
    /// </summary>
    public static class OrbitalSalvageEditorMenu
    {
        private const string ScenePath = "Assets/Scenes/MainGameScene.unity";

        private static UIManager EnsureSceneAndGetUI(bool force = false)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                if (File.Exists(ScenePath))
                {
                    EditorSceneManager.OpenScene(ScenePath);
                }
                else
                {
                    Debug.LogWarning("[OrbitalSalvage] MainGameScene.unity not found. Rebuilding scene...");
                    SceneBuilder.BuildAll();
                }
            }

            var uiMgr = Object.FindAnyObjectByType<UIManager>();
            if (uiMgr == null)
            {
                var canvas = GameObject.Find("UI_Canvas");
                if (canvas != null) uiMgr = canvas.AddComponent<UIManager>();
            }

            if (uiMgr != null)
            {
                uiMgr.EnsureHierarchyBuilt(force);
            }

            return uiMgr;
        }

        private static void RepaintAll()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        #region UI Previews (Edit Mode)
        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/1. Home Lobby", false, 10)]
        public static void PreviewHome()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowHome();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Screen: 1. Home Lobby (3D Mascot Diorama & Launch Console)");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/2. Cleanup Maps (50 Levels)", false, 11)]
        public static void PreviewMaps()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowMaps();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Screen: 2. Cleanup Maps (50 Levels Grid)");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/3. Daily Operations", false, 12)]
        public static void PreviewDaily()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowDaily();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Screen: 3. Daily Operations (Missions & Rewards)");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/4. Toy Shop & Hats", false, 13)]
        public static void PreviewShop()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowShop();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Screen: 4. Toy Shop & Hats (Catalog & Allowances)");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/5. Trophy Room", false, 14)]
        public static void PreviewTrophies()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowTrophies();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Screen: 5. Trophy Room (Achievements)");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/6. Game Settings", false, 15)]
        public static void PreviewSettings()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowSettings();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Screen: 6. Game Settings (Audio & Preferences)");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/7. In-Game HUD (Level 1)", false, 16)]
        public static void PreviewHUD()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowGameplay();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Screen: 7. In-Game HUD & Centrifuge World");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/Modals/Show: Pause Modal", false, 30)]
        public static void PreviewModalPause()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowModalPause(true);
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Modal: Pause Modal");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/Modals/Show: Victory Debrief", false, 31)]
        public static void PreviewModalVictory()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowModalVictory(true);
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Modal: Victory Debrief (3 Stars, Scrap Bonus)");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/Modals/Show: Defeat Modal", false, 32)]
        public static void PreviewModalDefeat()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.ShowModalDefeat(true);
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Previewing Modal: Defeat Debrief (Emergency Backup Pod)");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/Modals/Hide All Modals", false, 33)]
        public static void HideModals()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.HideAllModals();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] Modals Hidden.");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/Hide All UI (World Only)", false, 50)]
        public static void PreviewWorldOnly()
        {
            var ui = EnsureSceneAndGetUI();
            if (ui != null)
            {
                ui.HideAllPages();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] UI Hidden. Viewing 2D Centrifuge Physics World.");
            }
        }

        [MenuItem("Orbital Salvage/UI Previews (Edit Mode)/Force Refresh UI Hierarchy", false, 60)]
        public static void ForceRefreshUI()
        {
            var ui = EnsureSceneAndGetUI(true);
            if (ui != null)
            {
                ui.ShowHome();
                RepaintAll();
                Debug.Log("[OrbitalSalvage] UI Hierarchy forcefully refreshed with latest 3D Emoji Sprites!");
            }
        }
        #endregion

        #region Build & Setup
        [MenuItem("Orbital Salvage/Build & Setup/Rebuild Complete Game Scene & Prefabs", false, 100)]
        public static void BuildCompleteGame()
        {
            SceneBuilder.BuildAll();
        }

        [MenuItem("Orbital Salvage/Build & Setup/Configure Emoji Textures As 2D Sprites", false, 101)]
        public static void ConfigureEmojis()
        {
            ConfigureEmojiSprites.ReimportAllEmojisAsSprites();
        }

        [MenuItem("Orbital Salvage/Build & Setup/Batch Import 50 Sectors", false, 102)]
        public static void BatchImportSectors()
        {
            LevelImporter.BatchImportAll50Levels();
        }
        #endregion

        #region Documentation
        [MenuItem("Orbital Salvage/Documentation/Open Visual Parity Guide", false, 200)]
        public static void OpenParityGuide()
        {
            string docPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../09_VISUAL_PARITY_AND_SCREENSHOT_COMPARISON.md"));
            if (File.Exists(docPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(docPath) { UseShellExecute = true });
            }
            else
            {
                Debug.Log($"Visual Parity Guide available at: {docPath}");
            }
        }
        #endregion
    }
}
