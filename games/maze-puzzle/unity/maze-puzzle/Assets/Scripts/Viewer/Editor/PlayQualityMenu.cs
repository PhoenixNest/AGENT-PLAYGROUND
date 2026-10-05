using System;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Editor
{
    static class PlayQualityMenu
    {
        const string Root = "OrbitalSalvage/Play Quality/";

        [MenuItem(Root + "Auto", false, 1)]
        static void Auto() { PlayQuality.Forced = 0; }

        [MenuItem(Root + "Force 1x", false, 12)]
        static void X1() { PlayQuality.Forced = 1; }

        [MenuItem(Root + "Force 1.5x", false, 13)]
        static void X15() { PlayQuality.Forced = 1.5; }

        [MenuItem(Root + "Force 2x", false, 14)]
        static void X2() { PlayQuality.Forced = 2; }

        [MenuItem(Root + "Force 3x", false, 15)]
        static void X3() { PlayQuality.Forced = 3; }

        [MenuItem(Root + "Auto", true)]
        static bool AutoOn() { Menu.SetChecked(Root + "Auto", PlayQuality.Forced <= 0); return true; }

        [MenuItem(Root + "Force 1x", true)]
        static bool X1On() { Menu.SetChecked(Root + "Force 1x", MathNear(1)); return true; }

        [MenuItem(Root + "Force 1.5x", true)]
        static bool X15On() { Menu.SetChecked(Root + "Force 1.5x", MathNear(1.5)); return true; }

        [MenuItem(Root + "Force 2x", true)]
        static bool X2On() { Menu.SetChecked(Root + "Force 2x", MathNear(2)); return true; }

        [MenuItem(Root + "Force 3x", true)]
        static bool X3On() { Menu.SetChecked(Root + "Force 3x", MathNear(3)); return true; }

        static bool MathNear(double v) => Math.Abs(PlayQuality.Forced - v) < 0.05;
    }
}
