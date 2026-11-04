using UnityEngine;

namespace OrbitalSalvage.Parity
{
    public sealed class ScreenSafeAreaProvider : ISafeAreaProvider
    {
        public Rect SafeAreaPx => Screen.safeArea;
    }

    public sealed class ProfileSafeAreaProvider : ISafeAreaProvider
    {
        readonly Rect rect;

        public ProfileSafeAreaProvider(string profile)
        {
            if (!ParityProfiles.TryGetInsets(profile, out float top, out float right, out float bottom, out float left))
                throw new System.ArgumentException($"Unknown safe-area profile '{profile}'");
            float s = ParityProfiles.Scale;
            float w = ParityProfiles.PixelW;
            float h = ParityProfiles.PixelH;
            rect = new Rect(left * s, bottom * s, w - (left + right) * s, h - (top + bottom) * s);
        }

        public Rect SafeAreaPx => rect;
    }

    public static class SafeAreaProvider
    {
        static ISafeAreaProvider current = new ScreenSafeAreaProvider();

        public static ISafeAreaProvider Current
        {
            get => current;
            set => current = value ?? new ScreenSafeAreaProvider();
        }

        public static Rect SafeAreaPx => current.SafeAreaPx;
    }

    public static class ParityScreen
    {
        public static Vector2Int? Override { get; set; }

        public static Vector2Int Size => Override ?? new Vector2Int(Screen.width, Screen.height);
        public static int Width => Size.x;
        public static int Height => Size.y;
    }
}
