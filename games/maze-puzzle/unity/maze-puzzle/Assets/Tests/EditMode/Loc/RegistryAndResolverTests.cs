using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace OrbitalSalvage.Loc.Tests
{
    public class LanguageRegistryTests : LocTestBase
    {
        [Test]
        public void Registry_HasSeventeenLanguages()
        {
            IReadOnlyList<LanguageInfo> all = LanguageRegistry.All;
            Assert.AreEqual(17, all.Count);
            var codes = new HashSet<string>();
            int rtl = 0;
            for (int i = 0; i < all.Count; i++)
            {
                Assert.IsTrue(codes.Add(all[i].Code), all[i].Code);
                if (all[i].IsRtl) rtl++;
                Assert.AreEqual(all[i].Code == "ar", all[i].IsRtl);
            }
            Assert.AreEqual(17, codes.Count);
            Assert.AreEqual(1, rtl);
            Assert.AreEqual("en", LanguageRegistry.DefaultCode);
            Assert.IsTrue(LanguageRegistry.IsSupported("en"));
            Assert.IsTrue(LanguageRegistry.IsSupported("pt-PT"));
            Assert.IsFalse(LanguageRegistry.IsSupported("pt-BR"));
            Assert.IsFalse(LanguageRegistry.IsSupported("EN"));
            Assert.IsFalse(LanguageRegistry.IsSupported(null));

            var endonyms = new Dictionary<string, string>
            {
                { "en", "English" },
                { "zh-Hans", "简体中文" },
                { "zh-Hant", "繁體中文" },
                { "ja", "日本語" },
                { "ko", "한국어" },
                { "de", "Deutsch" },
                { "fr", "Français" },
                { "es", "Español" },
                { "it", "Italiano" },
                { "pt-PT", "Português" },
                { "vi", "Tiếng Việt" },
                { "fil", "Filipino" },
                { "id", "Bahasa Indonesia" },
                { "uz", "O\u02BBzbekcha" },
                { "km", "ភាសាខ្មែរ" },
                { "hi", "हिन्दी" },
                { "ar", "العربية" },
            };
            var fonts = new Dictionary<string, string>
            {
                { "en", "latin" },
                { "zh-Hans", "cjk-sc" },
                { "zh-Hant", "cjk-tc" },
                { "ja", "cjk-jp" },
                { "ko", "cjk-kr" },
                { "vi", "latin-vi" },
                { "km", "khmer" },
                { "hi", "devanagari" },
                { "ar", "arabic" },
            };
            for (int i = 0; i < all.Count; i++)
            {
                LanguageInfo info = all[i];
                Assert.AreEqual(endonyms[info.Code], info.Endonym, info.Code);
                string font;
                if (fonts.TryGetValue(info.Code, out font)) Assert.AreEqual(font, info.FontGroupId(), info.Code);
                else Assert.AreEqual("latin", info.FontGroupId(), info.Code);
            }
            Assert.AreEqual("Latn", all[13].Script);
            Assert.AreEqual("uz", all[13].Code);
        }
    }

    public class LocaleResolverTests : LocTestBase
    {
        [Test]
        public void ResolveTag_Table()
        {
            var cases = new Dictionary<string, string>
            {
                { "en", "en" },
                { "EN", "en" },
                { "en-US", "en" },
                { "en_GB", "en" },
                { "de", "de" },
                { "de-DE", "de" },
                { "fr-CA", "fr" },
                { "es-MX", "es" },
                { "es-419", "es" },
                { "it-IT", "it" },
                { "pt", "pt-PT" },
                { "pt-PT", "pt-PT" },
                { "pt-BR", "pt-PT" },
                { "PT-br", "pt-PT" },
                { "vi-VN", "vi" },
                { "fil", "fil" },
                { "fil-PH", "fil" },
                { "tl", "fil" },
                { "tl-PH", "fil" },
                { "id", "id" },
                { "id-ID", "id" },
                { "in", "id" },
                { "in-ID", "id" },
                { "uz", "uz" },
                { "uz-UZ", "uz" },
                { "uz-Latn", "uz" },
                { "uz-Latn-UZ", "uz" },
                { "ja-JP", "ja" },
                { "ko-KR", "ko" },
                { "km", "km" },
                { "km-KH", "km" },
                { "hi-IN", "hi" },
                { "ar-SA", "ar" },
                { "zh", "zh-Hans" },
                { "zh-CN", "zh-Hans" },
                { "zh-SG", "zh-Hans" },
                { "zh-Hans", "zh-Hans" },
                { "zh-Hans-CN", "zh-Hans" },
                { "ZH-hans-cn", "zh-Hans" },
                { "zh_TW", "zh-Hant" },
                { "zh-TW", "zh-Hant" },
                { "zh-HK", "zh-Hant" },
                { "zh-MO", "zh-Hant" },
                { "zh-Hant", "zh-Hant" },
                { "zh-Hant-TW", "zh-Hant" },
                { "  zh-Hans-CN  ", "zh-Hans" },
            };
            Assert.GreaterOrEqual(cases.Count, 40);
            foreach (KeyValuePair<string, string> pair in cases)
            {
                Assert.AreEqual(pair.Value, LocaleResolver.ResolveTag(pair.Key), pair.Key);
            }

            Assert.IsNull(LocaleResolver.ResolveTag(null));
            Assert.IsNull(LocaleResolver.ResolveTag(""));
            Assert.IsNull(LocaleResolver.ResolveTag("   "));
            Assert.IsNull(LocaleResolver.ResolveTag("uz-Cyrl"));
            Assert.IsNull(LocaleResolver.ResolveTag("uz-Cyrl-UZ"));
            Assert.IsNull(LocaleResolver.ResolveTag("nb"));
            Assert.IsNull(LocaleResolver.ResolveTag("no"));
            Assert.IsNull(LocaleResolver.ResolveTag("nn-NO"));
            Assert.IsNull(LocaleResolver.ResolveTag("iw"));
            Assert.IsNull(LocaleResolver.ResolveTag("und"));
        }

        [Test]
        public void ResolveSystemLanguage_PrefersCultureThenEnum()
        {
            Assert.AreEqual("de", LocaleResolver.ResolveSystemLanguage("German", null));
            Assert.AreEqual("de", LocaleResolver.ResolveSystemLanguage("German", ""));
            Assert.AreEqual("de", LocaleResolver.ResolveSystemLanguage("German", "de-DE"));
            Assert.AreEqual("fil", LocaleResolver.ResolveSystemLanguage("French", "fil-PH"));
            Assert.AreEqual("hi", LocaleResolver.ResolveSystemLanguage("English", "hi-IN"));
            Assert.AreEqual("km", LocaleResolver.ResolveSystemLanguage("English", "km-KH"));
            Assert.AreEqual("uz", LocaleResolver.ResolveSystemLanguage("English", "uz-Latn-UZ"));
            Assert.AreEqual("en", LocaleResolver.ResolveSystemLanguage("English", "uz-Cyrl"));
            Assert.AreEqual("zh-Hans", LocaleResolver.ResolveSystemLanguage("ChineseSimplified", null));
            Assert.AreEqual("zh-Hant", LocaleResolver.ResolveSystemLanguage("ChineseTraditional", null));
            Assert.AreEqual("zh-Hant", LocaleResolver.ResolveSystemLanguage("Chinese", "zh-HK"));
            Assert.AreEqual("zh-Hans", LocaleResolver.ResolveSystemLanguage("Chinese", null));
            Assert.AreEqual("pt-PT", LocaleResolver.ResolveSystemLanguage("Portuguese", "pt-BR"));
            Assert.IsNull(LocaleResolver.ResolveSystemLanguage("Unknown", null));
            Assert.AreEqual("fil", LocaleResolver.ResolveSystemLanguage("Unknown", "tl-PH"));
            Assert.IsNull(LocaleResolver.ResolveSystemLanguage("Norwegian", null));
            Assert.AreEqual("id", LocaleResolver.ResolveSystemLanguage("Indonesian", null));
            Assert.AreEqual("vi", LocaleResolver.ResolveSystemLanguage("Vietnamese", "vi-VN"));
            Assert.IsNull(LocaleResolver.ResolveSystemLanguage(null, null));
            Assert.AreEqual("ar", LocaleResolver.ResolveSystemLanguage("arabic", "not-a-tag"));

            Assert.AreEqual("fil", LocaleResolverUnity.ResolveSystemLanguage(SystemLanguage.German, "fil-PH"));
            Assert.AreEqual("zh-Hans", LocaleResolverUnity.ResolveSystemLanguage(SystemLanguage.ChineseSimplified, null));
            Assert.AreEqual("zh-Hant", LocaleResolverUnity.ResolveSystemLanguage(SystemLanguage.ChineseTraditional, ""));
            Assert.AreEqual("ar", LocaleResolverUnity.ResolveSystemLanguage(SystemLanguage.Arabic, null));
            Assert.AreEqual("pt-PT", LocaleResolverUnity.ResolveSystemLanguage(SystemLanguage.Portuguese, "pt-BR"));
        }
    }
}
