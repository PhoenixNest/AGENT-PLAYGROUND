using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>
    /// Resolves a CSS (family, font-weight) pair to the TMP font asset a real browser would render for the web
    /// prototype (games/maze-puzzle/tools/parity/SPEC.md section 5).
    ///
    /// The web app loads its fonts from Google Fonts, which serves only Fredoka 400/600/700 and Nunito 700/800/900
    /// for the styles.css @import. Every other requested weight is resolved by the CSS font-matching algorithm
    /// (CSS Fonts Level 4, 5.2) over those faces; there is no synthetic bold. This class implements the same
    /// algorithm over the same faces.
    ///
    /// TMP assets are produced by OrbitalSalvage.Editor.ParityAssetBuilder.BuildAll into
    /// Assets/Resources/Fonts/TMP/&lt;Family&gt;-&lt;faceWeight&gt; SDF.asset and are loaded with Resources.Load.
    /// Each Fredoka/Nunito asset carries the fallback table [NotoSansSymbols2-Regular SDF, NotoSansSymbols-Regular SDF].
    /// </summary>
    public static class ParityFonts
    {
        public const string Fredoka = "Fredoka";
        public const string Nunito = "Nunito";

        /// <summary>Resources-relative folder that holds the TMP font assets.</summary>
        public const string ResourcesDir = "Fonts/TMP";

        /// <summary>Symbol fallback chain (deliberate determinism choice, SPEC 5.1), in lookup order.</summary>
        public static readonly string[] FallbackFontNames = { "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular" };

        // Available faces per family, ascending. Must match what Google Fonts serves (SPEC 5.2).
        private static readonly string[] FamilyNames = { Fredoka, Nunito };
        private static readonly int[] FredokaFaces = { 400, 600, 700 };
        private static readonly int[] NunitoFaces = { 700, 800, 900 };

        private static readonly Dictionary<string, TMP_FontAsset> Cache = new Dictionary<string, TMP_FontAsset>();

        /// <summary>Family names that have faces, in the order they are built.</summary>
        public static IReadOnlyList<string> Families => FamilyNames;

        /// <summary>The available face weights of a family (ascending), or an empty list for an unknown family.</summary>
        public static IReadOnlyList<int> FaceWeights(string family)
        {
            switch (NormalizeFamily(family))
            {
                case Fredoka: return FredokaFaces;
                case Nunito: return NunitoFaces;
                default: return Array.Empty<int>();
            }
        }

        /// <summary>Name of the TMP asset (without extension) for a face: "Fredoka-700 SDF".</summary>
        public static string AssetName(string family, int faceWeight)
        {
            return NormalizeFamily(family) + "-" + faceWeight + " SDF";
        }

        /// <summary>Resources path (no extension) of the TMP asset for a face.</summary>
        public static string ResourcePath(string family, int faceWeight)
        {
            return ResourcesDir + "/" + AssetName(family, faceWeight);
        }

        /// <summary>
        /// Parses a CSS font-weight token as it appears in a font shorthand or property:
        /// "normal" = 400, "bold" = 700, or an integer. Returns 400 for anything else.
        /// </summary>
        public static int ParseCssWeight(string token)
        {
            if (string.IsNullOrEmpty(token)) return 400;
            token = token.Trim().ToLowerInvariant();
            if (token == "normal") return 400;
            if (token == "bold") return 700;
            return int.TryParse(token, out int w) ? w : 400;
        }

        /// <summary>
        /// CSS font-weight matching (CSS Fonts 4, 5.2) over <paramref name="available"/> (any order).
        /// Desired in [400, 500]: weights from desired up to 500 ascending, then lighter descending,
        /// then heavier than 500 ascending. Desired &lt; 400: lighter-or-equal descending, then heavier ascending.
        /// Desired &gt; 500: heavier-or-equal ascending, then lighter descending.
        /// Returns -1 when <paramref name="available"/> is empty.
        /// </summary>
        public static int MatchWeight(int desired, IReadOnlyList<int> available)
        {
            if (available == null || available.Count == 0) return -1;
            var asc = new List<int>(available);
            asc.Sort();

            if (desired >= 400 && desired <= 500)
            {
                foreach (int w in asc) if (w >= desired && w <= 500) return w;
                for (int i = asc.Count - 1; i >= 0; i--) if (asc[i] < desired) return asc[i];
                foreach (int w in asc) if (w > 500) return w;
            }
            else if (desired < 400)
            {
                for (int i = asc.Count - 1; i >= 0; i--) if (asc[i] <= desired) return asc[i];
                foreach (int w in asc) if (w > desired) return w;
            }
            else
            {
                foreach (int w in asc) if (w >= desired) return w;
                for (int i = asc.Count - 1; i >= 0; i--) if (asc[i] < desired) return asc[i];
            }
            return asc[0];
        }

        /// <summary>
        /// The face weight that renders for (family, cssWeight): 400 for Nunito resolves to 700,
        /// 800/900 for Fredoka resolve to 700. Returns -1 for an unknown family.
        /// </summary>
        public static int ResolveWeight(string family, int cssWeight)
        {
            int clamped = Mathf.Clamp(cssWeight, 1, 1000);
            return MatchWeight(clamped, FaceWeights(family));
        }

        /// <summary>
        /// The TMP font asset a browser would use for (family, cssWeight), or null (with an error log)
        /// when the family is unknown or BuildAll has not produced the asset.
        /// </summary>
        public static TMP_FontAsset Resolve(string family, int cssWeight)
        {
            int face = ResolveWeight(family, cssWeight);
            if (face < 0)
            {
                Debug.LogError($"[ParityFonts] Unknown font family '{family}'");
                return null;
            }

            string path = ResourcePath(family, face);
            if (Cache.TryGetValue(path, out var cached) && cached != null) return cached;

            var asset = Resources.Load<TMP_FontAsset>(path);
            if (asset == null)
            {
                Debug.LogError($"[ParityFonts] Missing TMP font asset Resources/{path} (run ParityAssetBuilder.BuildAll)");
                return null;
            }

            Cache[path] = asset;
            return asset;
        }

        /// <summary>Drops cached assets (domain reload and tests).</summary>
        public static void ClearCache()
        {
            Cache.Clear();
        }

        private static string NormalizeFamily(string family)
        {
            if (string.IsNullOrEmpty(family)) return string.Empty;
            string f = family.Trim().Trim('"', '\'');
            foreach (string name in FamilyNames)
            {
                if (string.Equals(name, f, StringComparison.OrdinalIgnoreCase)) return name;
            }
            return f;
        }
    }
}
