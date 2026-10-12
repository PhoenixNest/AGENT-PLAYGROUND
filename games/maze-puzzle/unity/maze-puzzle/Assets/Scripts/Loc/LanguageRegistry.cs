using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Loc
{
    /// <summary>Placeholder for the font family a language will use when rendering lands.</summary>
    public enum FontGroup
    {
        Latin,
        LatinVi,
        CjkSc,
        CjkTc,
        CjkJp,
        CjkKr,
        Devanagari,
        Khmer,
        Arabic
    }

    public sealed class LanguageInfo
    {
        public string Code;
        public string EnglishName;
        public string Endonym;
        public string Script;
        public bool IsRtl;
        public FontGroup FontGroup;

        public string FontGroupId()
        {
            return LanguageRegistry.FontGroupName(FontGroup);
        }
    }

    /// <summary>The 17 product languages. Default and fallback is <c>en</c>.</summary>
    public static class LanguageRegistry
    {
        public const string DefaultCode = "en";

        static readonly LanguageInfo[] Languages =
        {
            Lang("en", "English", "English", "Latn", false, FontGroup.Latin),
            Lang("zh-Hans", "Chinese (Simplified)", "简体中文", "Hans", false, FontGroup.CjkSc),
            Lang("zh-Hant", "Chinese (Traditional)", "繁體中文", "Hant", false, FontGroup.CjkTc),
            Lang("ja", "Japanese", "日本語", "Jpan", false, FontGroup.CjkJp),
            Lang("ko", "Korean", "한국어", "Kore", false, FontGroup.CjkKr),
            Lang("de", "German", "Deutsch", "Latn", false, FontGroup.Latin),
            Lang("fr", "French", "Français", "Latn", false, FontGroup.Latin),
            Lang("es", "Spanish", "Español", "Latn", false, FontGroup.Latin),
            Lang("it", "Italian", "Italiano", "Latn", false, FontGroup.Latin),
            Lang("pt-PT", "Portuguese", "Português", "Latn", false, FontGroup.Latin),
            Lang("vi", "Vietnamese", "Tiếng Việt", "Latn", false, FontGroup.LatinVi),
            Lang("fil", "Filipino", "Filipino", "Latn", false, FontGroup.Latin),
            Lang("id", "Indonesian", "Bahasa Indonesia", "Latn", false, FontGroup.Latin),
            Lang("uz", "Uzbek", "O\u02BBzbekcha", "Latn", false, FontGroup.Latin),
            Lang("km", "Khmer", "ភាសាខ្មែរ", "Khmr", false, FontGroup.Khmer),
            Lang("hi", "Hindi", "हिन्दी", "Deva", false, FontGroup.Devanagari),
            Lang("ar", "Arabic", "العربية", "Arab", true, FontGroup.Arabic),
        };

        static readonly Dictionary<string, LanguageInfo> ByCode = Build();

        public static IReadOnlyList<LanguageInfo> All
        {
            get { return Languages; }
        }

        public static bool IsSupported(string code)
        {
            return code != null && ByCode.ContainsKey(code);
        }

        public static bool TryGet(string code, out LanguageInfo info)
        {
            if (code == null)
            {
                info = null;
                return false;
            }
            return ByCode.TryGetValue(code, out info);
        }

        public static string FontGroupName(FontGroup group)
        {
            switch (group)
            {
                case FontGroup.Latin: return "latin";
                case FontGroup.LatinVi: return "latin-vi";
                case FontGroup.CjkSc: return "cjk-sc";
                case FontGroup.CjkTc: return "cjk-tc";
                case FontGroup.CjkJp: return "cjk-jp";
                case FontGroup.CjkKr: return "cjk-kr";
                case FontGroup.Devanagari: return "devanagari";
                case FontGroup.Khmer: return "khmer";
                case FontGroup.Arabic: return "arabic";
                default: return "latin";
            }
        }

        public static bool UsesLatinLengthBudget(FontGroup group)
        {
            return group == FontGroup.Latin || group == FontGroup.LatinVi;
        }

        static Dictionary<string, LanguageInfo> Build()
        {
            var map = new Dictionary<string, LanguageInfo>(StringComparer.Ordinal);
            for (int i = 0; i < Languages.Length; i++) map.Add(Languages[i].Code, Languages[i]);
            return map;
        }

        static LanguageInfo Lang(string code, string englishName, string endonym, string script, bool rtl, FontGroup font)
        {
            return new LanguageInfo
            {
                Code = code,
                EnglishName = englishName,
                Endonym = endonym,
                Script = script,
                IsRtl = rtl,
                FontGroup = font,
            };
        }
    }
}
