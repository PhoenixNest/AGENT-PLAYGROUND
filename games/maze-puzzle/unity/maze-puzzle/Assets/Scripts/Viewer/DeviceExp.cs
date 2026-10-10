using System;
using System.Text;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// One parsed <c>os.parity.exp</c> value. Default (all zeroes) is the product path.
    /// Tokens are comma-separated. Unknown tokens are ignored. <c>none</c> and an empty
    /// string are the same. The last <c>scale*</c> token wins, and the last of <c>f15</c>
    /// / <c>f30</c> wins. See the table on <see cref="DevicePerf"/>.
    /// </summary>
    public struct DeviceExp
    {
        public bool NoShadow;
        public bool NoBackdrop;
        public bool NoLoops;
        public bool NoCache;
        public bool NoGameHud;
        public bool NoGameFx;
        public bool NoImgui;
        public bool Rt16;
        public double Scale;
        public int FrameRate;
        /// <summary>Ambient loop cap in frames per second. Zero leaves the governor in charge.</summary>
        public int AmbientFps;

        public string Text()
        {
            var sb = new StringBuilder(64);
            Add(sb, NoShadow, "noshadow");
            Add(sb, NoBackdrop, "nobackdrop");
            Add(sb, NoLoops, "noloops");
            Add(sb, NoCache, "nocache");
            Add(sb, NoGameHud, "nogamehud");
            Add(sb, NoGameFx, "nogamefx");
            Add(sb, NoImgui, "noimgui");
            if (Scale == 1d) Add(sb, true, "scale1");
            else if (Scale == 1.5d) Add(sb, true, "scale1.5");
            else if (Scale == 2d) Add(sb, true, "scale2");
            else if (Scale == 3d) Add(sb, true, "scale3");
            if (FrameRate == 15) Add(sb, true, "f15");
            else if (FrameRate == 30) Add(sb, true, "f30");
            if (AmbientFps == 30) Add(sb, true, "amb30");
            else if (AmbientFps == 20) Add(sb, true, "amb20");
            else if (AmbientFps == 15) Add(sb, true, "amb15");
            else if (AmbientFps == 10) Add(sb, true, "amb10");
            Add(sb, Rt16, "rt16");
            return sb.Length == 0 ? "none" : sb.ToString();
        }

        public static DeviceExp Parse(string csv)
        {
            var exp = new DeviceExp();
            if (string.IsNullOrEmpty(csv)) return exp;
            int i = 0;
            while (i <= csv.Length)
            {
                int comma = csv.IndexOf(',', i);
                if (comma < 0) comma = csv.Length;
                int start = i;
                int end = comma;
                while (start < end && char.IsWhiteSpace(csv[start])) start++;
                while (end > start && char.IsWhiteSpace(csv[end - 1])) end--;
                if (end > start)
                {
                    string tok = csv.Substring(start, end - start);
                    ApplyToken(ref exp, tok);
                }
                if (comma >= csv.Length) break;
                i = comma + 1;
            }
            return exp;
        }

        static void ApplyToken(ref DeviceExp exp, string tok)
        {
            if (Equals(tok, "none")) return;
            if (Equals(tok, "noshadow")) exp.NoShadow = true;
            else if (Equals(tok, "nobackdrop")) exp.NoBackdrop = true;
            else if (Equals(tok, "noloops")) exp.NoLoops = true;
            else if (Equals(tok, "nocache")) exp.NoCache = true;
            else if (Equals(tok, "nogamehud")) exp.NoGameHud = true;
            else if (Equals(tok, "nogamefx")) exp.NoGameFx = true;
            else if (Equals(tok, "noimgui")) exp.NoImgui = true;
            else if (Equals(tok, "rt16")) exp.Rt16 = true;
            else if (Equals(tok, "scale1.5")) exp.Scale = 1.5;
            else if (Equals(tok, "scale1")) exp.Scale = 1;
            else if (Equals(tok, "scale2")) exp.Scale = 2;
            else if (Equals(tok, "scale3")) exp.Scale = 3;
            else if (Equals(tok, "f15")) exp.FrameRate = 15;
            else if (Equals(tok, "f30")) exp.FrameRate = 30;
            else if (Equals(tok, "amb30")) exp.AmbientFps = 30;
            else if (Equals(tok, "amb20")) exp.AmbientFps = 20;
            else if (Equals(tok, "amb15")) exp.AmbientFps = 15;
            else if (Equals(tok, "amb10")) exp.AmbientFps = 10;
        }

        static bool Equals(string tok, string name)
        {
            return string.Equals(tok, name, StringComparison.OrdinalIgnoreCase);
        }

        static void Add(StringBuilder sb, bool on, string name)
        {
            if (!on) return;
            if (sb.Length > 0) sb.Append(',');
            sb.Append(name);
        }
    }
}
