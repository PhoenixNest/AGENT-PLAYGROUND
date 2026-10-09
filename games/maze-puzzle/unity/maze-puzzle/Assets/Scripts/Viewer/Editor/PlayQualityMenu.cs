using System;
using UnityEditor;

namespace OrbitalSalvage.Viewer.Editor
{
    static class PlayQualityMenu
    {
        const string Root = "Orbital Salvage/Play/Quality/";
        const int Priority = 300;

        [MenuItem(Root + "Auto", false, Priority)]
        static void Auto() { PlayQuality.Forced = 0; }

        [MenuItem(Root + "1x", false, Priority + 1)]
        static void X1() { PlayQuality.Forced = 1; }

        [MenuItem(Root + "1.5x", false, Priority + 2)]
        static void X15() { PlayQuality.Forced = 1.5; }

        [MenuItem(Root + "2x", false, Priority + 3)]
        static void X2() { PlayQuality.Forced = 2; }

        [MenuItem(Root + "3x", false, Priority + 4)]
        static void X3() { PlayQuality.Forced = 3; }

        [MenuItem(Root + "Auto", true)]
        static bool AutoOn() { Menu.SetChecked(Root + "Auto", PlayQuality.Forced <= 0); return true; }

        [MenuItem(Root + "1x", true)]
        static bool X1On() { Menu.SetChecked(Root + "1x", MathNear(1)); return true; }

        [MenuItem(Root + "1.5x", true)]
        static bool X15On() { Menu.SetChecked(Root + "1.5x", MathNear(1.5)); return true; }

        [MenuItem(Root + "2x", true)]
        static bool X2On() { Menu.SetChecked(Root + "2x", MathNear(2)); return true; }

        [MenuItem(Root + "3x", true)]
        static bool X3On() { Menu.SetChecked(Root + "3x", MathNear(3)); return true; }

        static bool MathNear(double v) => Math.Abs(PlayQuality.Forced - v) < 0.05;
    }
}
