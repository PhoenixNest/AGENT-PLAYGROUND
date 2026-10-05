using System;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Product-path raster override. 0 means the governor chooses. Parity, self-test, and capture never read this.
    /// </summary>
    public static class PlayQuality
    {
        public static double Forced;

        public static double FromCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-parity-quality");
            if (i < 0 || i + 1 >= args.Length) return 0;
            string s = args[i + 1];
            if (s == "auto") return 0;
            return double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double v) ? v : 0;
        }
    }
}
