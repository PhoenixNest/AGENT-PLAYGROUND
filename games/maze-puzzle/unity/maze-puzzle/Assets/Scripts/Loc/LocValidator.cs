using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Loc
{
    public enum LocSeverity
    {
        Warning,
        Error
    }

    public sealed class LocFinding
    {
        public LocSeverity Severity;
        public string Kind;
        public string Key;
        public string Message;

        public override string ToString()
        {
            string severity = Severity == LocSeverity.Error ? "error" : "warning";
            return severity + "\t" + Kind + "\t" + Key + "\t" + Message;
        }
    }

    public sealed class LocLengthLimits
    {
        public double WarnRatio = 1.6;
        public double FailRatio = 2.5;
    }

    /// <summary>
    /// Compares an English table with one translation. Findings are a flat list with a severity.
    /// Length ratios apply only to latin and latin-vi font groups. Warn when the translation is
    /// at least <see cref="LocLengthLimits.WarnRatio"/> times the English length, and fail at
    /// <see cref="LocLengthLimits.FailRatio"/>. Lengths are UTF-16 code units.
    /// </summary>
    public static class LocValidator
    {
        public static List<LocFinding> Validate(LocTable english, LocTable translation)
        {
            return Validate(english, translation, null);
        }

        public static List<LocFinding> Validate(LocTable english, LocTable translation, LocLengthLimits limits)
        {
            if (english == null) throw new ArgumentNullException("english");
            if (translation == null) throw new ArgumentNullException("translation");
            if (limits == null) limits = new LocLengthLimits();

            bool latin = true;
            LanguageInfo info;
            if (!string.IsNullOrEmpty(translation.Code) && LanguageRegistry.TryGet(translation.Code, out info))
            {
                latin = LanguageRegistry.UsesLatinLengthBudget(info.FontGroup);
            }

            var findings = new List<LocFinding>();
            CheckMap("spec", english.Spec, translation.Spec, latin, limits, findings);
            CheckMap("str", english.Str, translation.Str, latin, limits, findings);
            findings.Sort(CompareFindings);
            return findings;
        }

        static void CheckMap(
            string section,
            Dictionary<string, string> source,
            Dictionary<string, string> target,
            bool latin,
            LocLengthLimits limits,
            List<LocFinding> findings)
        {
            var keys = new List<string>(source.Count + target.Count);
            foreach (string key in source.Keys) keys.Add(key);
            foreach (string key in target.Keys)
            {
                if (!source.ContainsKey(key)) keys.Add(key);
            }
            keys.Sort(StringComparer.Ordinal);

            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                string id = section + ":" + key;
                string sourceValue;
                string targetValue;
                bool inSource = source.TryGetValue(key, out sourceValue);
                bool inTarget = target.TryGetValue(key, out targetValue);
                if (!inTarget)
                {
                    findings.Add(Make(LocSeverity.Error, "missing", id, "missing translation"));
                    continue;
                }
                if (!inSource)
                {
                    findings.Add(Make(LocSeverity.Error, "extra", id, "key is not in the English table"));
                    continue;
                }
                if (!LocText.SamePlaceholders(sourceValue, targetValue))
                {
                    findings.Add(Make(LocSeverity.Error, "placeholder", id, "placeholder indexes differ"));
                }
                if (HasEdgeWhitespace(sourceValue) || HasEdgeWhitespace(targetValue))
                {
                    findings.Add(Make(LocSeverity.Warning, "whitespace", id, "leading or trailing whitespace"));
                }
                if (HasControl(targetValue))
                {
                    findings.Add(Make(LocSeverity.Error, "control", id, "control character in translation"));
                }
                if (latin && sourceValue != null && sourceValue.Length > 0 && targetValue != null)
                {
                    double ratio = (double)targetValue.Length / sourceValue.Length;
                    if (ratio >= limits.FailRatio)
                    {
                        findings.Add(Make(LocSeverity.Error, "length", id, "length is " + ratio.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "x English"));
                    }
                    else if (ratio >= limits.WarnRatio)
                    {
                        findings.Add(Make(LocSeverity.Warning, "length", id, "length is " + ratio.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "x English"));
                    }
                }
            }
        }

        static bool HasEdgeWhitespace(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return char.IsWhiteSpace(text[0]) || char.IsWhiteSpace(text[text.Length - 1]);
        }

        static bool HasControl(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c < 0x20 || c == 0x7F) return true;
            }
            return false;
        }

        static LocFinding Make(LocSeverity severity, string kind, string key, string message)
        {
            return new LocFinding { Severity = severity, Kind = kind, Key = key, Message = message };
        }

        static int CompareFindings(LocFinding a, LocFinding b)
        {
            int key = string.CompareOrdinal(a.Key, b.Key);
            if (key != 0) return key;
            return string.CompareOrdinal(a.Kind, b.Kind);
        }
    }
}
