using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Emoji sprite lookup (SPEC 6) and the grapheme classification rule shared with the web harness:
    /// a grapheme is an emoji iff it contains an Extended_Pictographic code point and (contains U+FE0F or its
    /// first code point has Emoji_Presentation).
    /// </summary>
    public static class CanvasEmoji
    {
        const string IndexPath = "Emojis/emoji_index";
        const string TexturePrefix = "Emojis/3D/";

        static Dictionary<string, string> files;
        static readonly Dictionary<string, CanvasImage> Images = new Dictionary<string, CanvasImage>();

        // Extended_Pictographic ranges that occur in graphemes used by the game plus the standard emoji blocks.
        static readonly int[][] Pictographic =
        {
            new[] { 0x00A9, 0x00A9 }, new[] { 0x00AE, 0x00AE }, new[] { 0x203C, 0x203C }, new[] { 0x2049, 0x2049 },
            new[] { 0x2122, 0x2122 }, new[] { 0x2139, 0x2139 }, new[] { 0x2194, 0x2199 }, new[] { 0x21A9, 0x21AA },
            new[] { 0x231A, 0x231B }, new[] { 0x2328, 0x2328 }, new[] { 0x2388, 0x2388 }, new[] { 0x23CF, 0x23CF },
            new[] { 0x23E9, 0x23F3 }, new[] { 0x23F8, 0x23FA }, new[] { 0x24C2, 0x24C2 }, new[] { 0x25AA, 0x25AB },
            new[] { 0x25B6, 0x25B6 }, new[] { 0x25C0, 0x25C0 }, new[] { 0x25FB, 0x25FE }, new[] { 0x2600, 0x2605 },
            new[] { 0x2607, 0x2612 }, new[] { 0x2614, 0x2685 }, new[] { 0x2690, 0x2705 }, new[] { 0x2708, 0x2712 },
            new[] { 0x2714, 0x2714 }, new[] { 0x2716, 0x2716 }, new[] { 0x271D, 0x271D }, new[] { 0x2721, 0x2721 },
            new[] { 0x2728, 0x2728 }, new[] { 0x2733, 0x2734 }, new[] { 0x2744, 0x2744 }, new[] { 0x2747, 0x2747 },
            new[] { 0x274C, 0x274C }, new[] { 0x274E, 0x274E }, new[] { 0x2753, 0x2755 }, new[] { 0x2757, 0x2757 },
            new[] { 0x2763, 0x2767 }, new[] { 0x2795, 0x2797 }, new[] { 0x27A1, 0x27A1 }, new[] { 0x27B0, 0x27B0 },
            new[] { 0x27BF, 0x27BF }, new[] { 0x2934, 0x2935 }, new[] { 0x2B05, 0x2B07 }, new[] { 0x2B1B, 0x2B1C },
            new[] { 0x2B50, 0x2B50 }, new[] { 0x2B55, 0x2B55 }, new[] { 0x3030, 0x3030 }, new[] { 0x303D, 0x303D },
            new[] { 0x3297, 0x3297 }, new[] { 0x3299, 0x3299 }, new[] { 0x1F000, 0x1F0FF }, new[] { 0x1F10D, 0x1F10F },
            new[] { 0x1F12F, 0x1F12F }, new[] { 0x1F16C, 0x1F171 }, new[] { 0x1F17E, 0x1F17F }, new[] { 0x1F18E, 0x1F18E },
            new[] { 0x1F191, 0x1F19A }, new[] { 0x1F1AD, 0x1F1E5 }, new[] { 0x1F201, 0x1F20F }, new[] { 0x1F21A, 0x1F21A },
            new[] { 0x1F22F, 0x1F22F }, new[] { 0x1F232, 0x1F23A }, new[] { 0x1F23C, 0x1F23F }, new[] { 0x1F249, 0x1F3FA },
            new[] { 0x1F400, 0x1F53D }, new[] { 0x1F546, 0x1F64F }, new[] { 0x1F680, 0x1F6FF }, new[] { 0x1F774, 0x1F77F },
            new[] { 0x1F7D5, 0x1F7FF }, new[] { 0x1F80C, 0x1F80F }, new[] { 0x1F848, 0x1F84F }, new[] { 0x1F85A, 0x1F85F },
            new[] { 0x1F888, 0x1F88F }, new[] { 0x1F8AE, 0x1F8FF }, new[] { 0x1F90C, 0x1F93A }, new[] { 0x1F93C, 0x1F945 },
            new[] { 0x1F947, 0x1FAFF }, new[] { 0x1FC00, 0x1FFFD },
        };

        // Emoji_Presentation code points outside the 1F000 plane that default to emoji style.
        static readonly int[][] PresentationBmp =
        {
            new[] { 0x231A, 0x231B }, new[] { 0x23E9, 0x23EC }, new[] { 0x23F0, 0x23F0 }, new[] { 0x23F3, 0x23F3 },
            new[] { 0x25FD, 0x25FE }, new[] { 0x2614, 0x2615 }, new[] { 0x2648, 0x2653 }, new[] { 0x267F, 0x267F },
            new[] { 0x2693, 0x2693 }, new[] { 0x26A1, 0x26A1 }, new[] { 0x26AA, 0x26AB }, new[] { 0x26BD, 0x26BE },
            new[] { 0x26C4, 0x26C5 }, new[] { 0x26CE, 0x26CE }, new[] { 0x26D4, 0x26D4 }, new[] { 0x26EA, 0x26EA },
            new[] { 0x26F2, 0x26F3 }, new[] { 0x26F5, 0x26F5 }, new[] { 0x26FA, 0x26FA }, new[] { 0x26FD, 0x26FD },
            new[] { 0x2705, 0x2705 }, new[] { 0x270A, 0x270B }, new[] { 0x2728, 0x2728 }, new[] { 0x274C, 0x274C },
            new[] { 0x274E, 0x274E }, new[] { 0x2753, 0x2755 }, new[] { 0x2757, 0x2757 }, new[] { 0x2795, 0x2797 },
            new[] { 0x27B0, 0x27B0 }, new[] { 0x27BF, 0x27BF }, new[] { 0x2B1B, 0x2B1C }, new[] { 0x2B50, 0x2B50 },
            new[] { 0x2B55, 0x2B55 },
        };

        static bool InRanges(int[][] ranges, int cp)
        {
            foreach (var r in ranges) if (cp >= r[0] && cp <= r[1]) return true;
            return false;
        }

        static bool HasPresentation(int cp)
        {
            if (cp < 0x1F000) return InRanges(PresentationBmp, cp);
            if (!InRanges(Pictographic, cp) || !PresentationSmp(cp)) return false;
            if (cp >= 0x1F000 && cp <= 0x1F0FF) return cp == 0x1F004 || cp == 0x1F0CF;
            if (cp >= 0x1F10D && cp <= 0x1F10F || cp == 0x1F12F || cp >= 0x1F16C && cp <= 0x1F171 || cp == 0x1F17E || cp == 0x1F17F) return false;
            return true;
        }

        static bool PresentationSmp(int cp)
        {
            return !((cp >= 0x1F321 && cp <= 0x1F32C) || cp == 0x1F336 || cp == 0x1F37D || (cp >= 0x1F394 && cp <= 0x1F39F) ||
                     (cp >= 0x1F3CB && cp <= 0x1F3CE) || (cp >= 0x1F3D4 && cp <= 0x1F3DF) || (cp >= 0x1F3F3 && cp <= 0x1F3F5) ||
                     cp == 0x1F3F7 || cp == 0x1F43F || cp == 0x1F441 || cp == 0x1F4FD || cp == 0x1F4FE || cp == 0x1F53F ||
                     (cp >= 0x1F549 && cp <= 0x1F54A) || (cp >= 0x1F56F && cp <= 0x1F570) || cp == 0x1F573 || (cp >= 0x1F574 && cp <= 0x1F579) ||
                     cp == 0x1F587 || (cp >= 0x1F58A && cp <= 0x1F58D) || cp == 0x1F590 || (cp >= 0x1F5A5 && cp <= 0x1F5A8) ||
                     cp == 0x1F5B1 || cp == 0x1F5B2 || cp == 0x1F5BC || (cp >= 0x1F5C2 && cp <= 0x1F5C4) || (cp >= 0x1F5D1 && cp <= 0x1F5D3) ||
                     (cp >= 0x1F5DC && cp <= 0x1F5DE) || cp == 0x1F5E1 || cp == 0x1F5E3 || cp == 0x1F5E8 || cp == 0x1F5EF || cp == 0x1F5F3 ||
                     cp == 0x1F5FA || (cp >= 0x1F6CB && cp <= 0x1F6CF) || cp == 0x1F6E0 || cp == 0x1F6E1 || cp == 0x1F6E2 || cp == 0x1F6E3 ||
                     cp == 0x1F6E9 || cp == 0x1F6F0 || cp == 0x1F6F3);
        }

        public static bool IsEmoji(string grapheme)
        {
            bool ext = false, fe0f = false;
            int first = -1;
            for (int i = 0; i < grapheme.Length; i++)
            {
                int cp = char.ConvertToUtf32(grapheme, i);
                if (char.IsHighSurrogate(grapheme[i])) i++;
                if (first < 0) first = cp;
                if (cp == 0xFE0F) fe0f = true;
                if (InRanges(Pictographic, cp)) ext = true;
            }
            if (!ext) return false;
            return fe0f || HasPresentation(first);
        }

        /// <summary>Splits into maximal text runs and single emoji graphemes, in order. Emoji runs have isEmoji set.</summary>
        public static void SplitRuns(string s, List<KeyValuePair<string, bool>> runs)
        {
            runs.Clear();
            var cur = new StringBuilder();
            var en = StringInfo.GetTextElementEnumerator(s);
            while (en.MoveNext())
            {
                string g = (string)en.Current;
                if (IsEmoji(g))
                {
                    if (cur.Length > 0) { runs.Add(new KeyValuePair<string, bool>(cur.ToString(), false)); cur.Length = 0; }
                    runs.Add(new KeyValuePair<string, bool>(g, true));
                }
                else cur.Append(g);
            }
            if (cur.Length > 0) runs.Add(new KeyValuePair<string, bool>(cur.ToString(), false));
        }

        public static string CodepointName(string grapheme)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < grapheme.Length; i++)
            {
                int cp = char.ConvertToUtf32(grapheme, i);
                if (char.IsHighSurrogate(grapheme[i])) i++;
                if (cp == 0xFE0F) continue;
                if (sb.Length > 0) sb.Append('-');
                sb.Append(cp.ToString("x"));
            }
            return sb.ToString();
        }

        static void LoadIndex()
        {
            if (files != null) return;
            files = new Dictionary<string, string>();
            var ta = Resources.Load<TextAsset>(IndexPath);
            if (ta == null) { Debug.LogError("[CanvasEmoji] missing Resources/" + IndexPath); return; }
            var root = MiniJson.Parse(ta.text) as Dictionary<string, object>;
            foreach (var kv in root) files[kv.Key] = (string)((Dictionary<string, object>)kv.Value)["file"];
        }

        public static CanvasImage Get(string grapheme)
        {
            if (Images.TryGetValue(grapheme, out var img) && img != null && img.texture != null) return img;
            LoadIndex();
            string name = files.TryGetValue(grapheme, out var f) ? f.Substring(0, f.LastIndexOf('.')) : CodepointName(grapheme);
            var tex = Resources.Load<Texture2D>(TexturePrefix + name);
            if (tex == null) { Debug.LogWarning("[CanvasEmoji] missing sprite for " + grapheme + " (" + name + ")"); return null; }
            img = new CanvasImage(tex);
            Images[grapheme] = img;
            return img;
        }
    }
}
