using UnityEditor;

namespace OrbitalSalvage.Viewer.Editor
{
    static class PagePreviewMenu
    {
        const int Priority = 100;

        [MenuItem("Orbital Salvage/Preview/Open Window", false, Priority)]
        static void OpenWindow() { PagePreviewWindow.Open("home_default"); }

        [MenuItem("Orbital Salvage/Preview/home_default", false, Priority + 1)]
        static void Home() { PagePreviewWindow.Open("home_default"); }

        [MenuItem("Orbital Salvage/Preview/maps_world_1_fresh", false, Priority + 2)]
        static void Maps() { PagePreviewWindow.Open("maps_world_1_fresh"); }

        [MenuItem("Orbital Salvage/Preview/daily_default", false, Priority + 3)]
        static void Daily() { PagePreviewWindow.Open("daily_default"); }

        [MenuItem("Orbital Salvage/Preview/shop_rings_default", false, Priority + 4)]
        static void Shop() { PagePreviewWindow.Open("shop_rings_default"); }

        [MenuItem("Orbital Salvage/Preview/achievements_none", false, Priority + 5)]
        static void Trophies() { PagePreviewWindow.Open("achievements_none"); }

        [MenuItem("Orbital Salvage/Preview/settings_default", false, Priority + 6)]
        static void Settings() { PagePreviewWindow.Open("settings_default"); }

        [MenuItem("Orbital Salvage/Preview/loading_t0500", false, Priority + 7)]
        static void Loading() { PagePreviewWindow.Open("loading_t0500"); }

        [MenuItem("Orbital Salvage/Preview/game_lvl01_t1000ms", false, Priority + 8)]
        static void Game() { PagePreviewWindow.Open("game_lvl01_t1000ms"); }

        [MenuItem("Orbital Salvage/Preview/modal_pause", false, Priority + 9)]
        static void Pause() { PagePreviewWindow.Open("modal_pause"); }

        [MenuItem("Orbital Salvage/Preview/modal_debrief_exact", false, Priority + 10)]
        static void Debrief() { PagePreviewWindow.Open("modal_debrief_exact"); }

        [MenuItem("Orbital Salvage/Preview/modal_defeat_12_of_20", false, Priority + 11)]
        static void Defeat() { PagePreviewWindow.Open("modal_defeat_12_of_20"); }
    }
}
