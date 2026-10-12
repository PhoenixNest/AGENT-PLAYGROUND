using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Loc
{
    public sealed class SpecScan
    {
        public int FileCount;
        public List<string> SpecKeys = new List<string>();
        public List<string> SkipKeys = new List<string>();
        public string EnglishJson = "";
        public string Csv = "";
    }

    /// <summary>
    /// Reads parity spec JSON and builds the English string table plus the translator CSV.
    /// A normalised run is not translated when it contains no Unicode letter. That covers digits
    /// (<c>250</c>, <c>+100</c>, <c>0/10</c>), joiners (<c>/</c>), symbols (<c>×</c>, <c>▶</c>),
    /// and emoji-only runs. Those strings are listed in <c>doNotTranslate</c>. A run is also
    /// not translated when it is a <see cref="LanguageRegistry"/> endonym, or that endonym with
    /// the current-language <see cref="LanguageMarker"/>. Endonyms stay in their own script in
    /// every locale. Runs that contain a letter and are not in that set stay in <c>spec</c> even
    /// when digits are baked in; the CSV flags <c>hasPlaceholderLike</c>. Empty text after
    /// normalisation is dropped. The placeholder legal URLs are not spec runs; they live in
    /// <c>LegalLinks</c>.
    /// <c>*__iphone.json</c> is not written; callers compare its key set with <c>*__zero.json</c>.
    /// </summary>
    public static class SpecExtractor
    {
        public const int MaxIds = 5;
        public const int MaxClasses = 3;
        public const int MaxGroups = 8;
        public const int MaxFilesListed = 3;

        static readonly HashSet<string> NeverTranslateKeys = BuildNeverTranslate();

        /// <summary>True for a normalised endonym, or the same endonym with the current-language marker.</summary>
        public static bool IsNeverTranslate(string normalisedKey)
        {
            return normalisedKey != null && NeverTranslateKeys.Contains(normalisedKey);
        }

        public static string HandoffCsvPath(string dataPath)
        {
            return Path.GetFullPath(Path.Combine(dataPath, "..", "..", "..", "localization", "key-index.csv"));
        }

        public static SpecScan Scan(string specDirectory, string fileSuffix)
        {
            if (string.IsNullOrEmpty(fileSuffix)) fileSuffix = "__zero.json";
            string[] files = Directory.GetFiles(specDirectory, "*" + fileSuffix);
            Array.Sort(files, StringComparer.Ordinal);

            var rows = new Dictionary<string, Row>(StringComparer.Ordinal);
            var skips = new HashSet<string>(StringComparer.Ordinal);

            for (int f = 0; f < files.Length; f++)
            {
                string name = Path.GetFileName(files[f]);
                JsonValue root = JsonParser.Parse(File.ReadAllText(files[f]));
                JsonValue elements = root.Get("elements");
                if (elements == null || elements.Kind != JsonKind.Array) continue;
                for (int ei = 0; ei < elements.Count; ei++)
                {
                    JsonValue el = elements[ei];
                    if (el == null || el.Kind != JsonKind.Object) continue;
                    ReadElement(name, ei, el, rows, skips);
                }
            }

            var specKeys = new List<string>(rows.Keys);
            specKeys.Sort(StringComparer.Ordinal);
            var skipKeys = new List<string>(skips);
            skipKeys.Sort(StringComparer.Ordinal);

            string endonym = "English";
            LanguageInfo english;
            if (LanguageRegistry.TryGet(LanguageRegistry.DefaultCode, out english)) endonym = english.Endonym;

            var scan = new SpecScan();
            scan.FileCount = files.Length;
            scan.SpecKeys = specKeys;
            scan.SkipKeys = skipKeys;
            scan.EnglishJson = LocJson.WriteTable(
                LanguageRegistry.DefaultCode,
                endonym,
                specKeys,
                delegate (string key) { return key; },
                null,
                skipKeys);
            scan.Csv = WriteCsv(specKeys, rows);
            return scan;
        }

        static void ReadElement(string fileName, int elementIndex, JsonValue el, Dictionary<string, Row> rows, HashSet<string> skips)
        {
            JsonValue runs = el.Get("runs");
            if (runs == null || runs.Kind != JsonKind.Array || runs.Count == 0) return;

            int textRuns = 0;
            var normalised = new string[runs.Count];
            var raw = new string[runs.Count];
            for (int i = 0; i < runs.Count; i++)
            {
                JsonValue run = runs[i];
                string text = run != null && run.Kind == JsonKind.Object ? StringField(run, "s") : "";
                raw[i] = text;
                string key = LocText.Normalize(text);
                normalised[i] = key;
                if (key.Length > 0) textRuns++;
            }

            string id = StringField(el, "id");
            string cls = StringField(el, "c");
            if (cls.Length == 0) cls = StringField(el, "cls");
            string fz = StringField(el, "fz");
            string tt = StringField(el, "tt");
            double elementWidth = WidthAt(el.Get("r"), 2);
            string group = id.Length > 0
                ? id
                : fileName + "#" + elementIndex.ToString(CultureInfo.InvariantCulture);
            bool fragment = textRuns >= 2;

            for (int i = 0; i < runs.Count; i++)
            {
                string key = normalised[i];
                if (key.Length == 0) continue;
                if (!LocText.HasLetter(key) || IsNeverTranslate(key))
                {
                    skips.Add(key);
                    continue;
                }
                Row row;
                if (!rows.TryGetValue(key, out row))
                {
                    row = new Row();
                    row.Key = key;
                    rows.Add(key, row);
                }
                row.NoteFile(fileName);
                if (id.Length > 0) row.NoteId(id);
                if (cls.Length > 0) row.NoteClass(cls);
                if (row.Fz.Length == 0 && fz.Length > 0) row.Fz = fz;
                if (string.Equals(tt, "uppercase", StringComparison.OrdinalIgnoreCase)) row.Uppercase = true;
                else if (row.Tt.Length == 0 && tt.Length > 0) row.Tt = tt;
                double runWidth = 0;
                JsonValue run = runs[i];
                if (run != null && run.Kind == JsonKind.Object) runWidth = WidthAt(run.Get("r"), 2);
                double width = runWidth > 0 ? runWidth : elementWidth;
                if (width > row.MaxWidth) row.MaxWidth = width;
                if (LocText.ContainsEmoji(raw[i])) row.Emoji = true;
                if (fragment) row.Fragment = true;
                row.NoteGroup(group, i);
            }
        }

        static HashSet<string> BuildNeverTranslate()
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            var languages = LanguageRegistry.All;
            for (int i = 0; i < languages.Count; i++)
            {
                string endonym = LocText.Normalize(languages[i].Endonym);
                if (endonym.Length > 0) set.Add(endonym);
                string marked = LocText.Normalize(LanguageMarker.Mark(languages[i].Endonym));
                if (marked.Length > 0) set.Add(marked);
            }
            return set;
        }

        static string WriteCsv(List<string> specKeys, Dictionary<string, Row> rows)
        {
            var sb = new StringBuilder();
            sb.Append("key,kind,files,element_ids,element_classes,fz,uppercase,max_width,chars,hasEmoji,hasPlaceholderLike,group,run_index,notes\n");
            for (int i = 0; i < specKeys.Count; i++)
            {
                Row row = rows[specKeys[i]];
                string upper = row.Uppercase ? "uppercase" : row.Tt;
                string width = row.MaxWidth > 0
                    ? row.MaxWidth.ToString("0.###", CultureInfo.InvariantCulture)
                    : "";
                Csv(sb, row.Key);
                sb.Append(',');
                Csv(sb, "spec");
                sb.Append(',');
                Csv(sb, row.FilesField());
                sb.Append(',');
                Csv(sb, JoinLimited(row.Ids, MaxIds));
                sb.Append(',');
                Csv(sb, JoinLimited(row.Classes, MaxClasses));
                sb.Append(',');
                Csv(sb, row.Fz);
                sb.Append(',');
                Csv(sb, upper);
                sb.Append(',');
                Csv(sb, width);
                sb.Append(',');
                Csv(sb, CodePoints(row.Key).ToString(CultureInfo.InvariantCulture));
                sb.Append(',');
                Csv(sb, row.Emoji ? "true" : "false");
                sb.Append(',');
                Csv(sb, LocText.HasDigit(row.Key) ? "true" : "false");
                sb.Append(',');
                Csv(sb, row.GroupsField());
                sb.Append(',');
                Csv(sb, row.RunIndexField());
                sb.Append(',');
                Csv(sb, row.Notes());
                sb.Append('\n');
            }
            return sb.ToString();
        }

        static int CodePoints(string text)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1])) i++;
                count++;
            }
            return count;
        }

        static string JoinLimited(List<string> values, int max)
        {
            if (values.Count == 0) return "";
            var sorted = new List<string>(values);
            sorted.Sort(StringComparer.Ordinal);
            int n = Math.Min(max, sorted.Count);
            var sb = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                if (i > 0) sb.Append("; ");
                sb.Append(sorted[i]);
            }
            return sb.ToString();
        }

        static void Csv(StringBuilder sb, string value)
        {
            if (value == null) value = "";
            bool quote = false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == ',' || c == '"' || c == '\n' || c == '\r')
                {
                    quote = true;
                    break;
                }
            }
            if (!quote)
            {
                sb.Append(value);
                return;
            }
            sb.Append('"');
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '"') sb.Append("\"\"");
                else sb.Append(c);
            }
            sb.Append('"');
        }

        static string StringField(JsonValue obj, string key)
        {
            JsonValue value = obj.Get(key);
            if (value == null || value.Kind != JsonKind.String || value.String == null) return "";
            return value.String;
        }

        static double WidthAt(JsonValue array, int index)
        {
            if (array == null || array.Kind != JsonKind.Array || array.Count <= index) return 0;
            JsonValue value = array[index];
            if (value == null || value.Kind != JsonKind.Number) return 0;
            if (double.IsNaN(value.Number) || double.IsInfinity(value.Number)) return 0;
            return value.Number;
        }

        sealed class Row
        {
            public string Key = "";
            public string Fz = "";
            public string Tt = "";
            public bool Uppercase;
            public double MaxWidth;
            public bool Emoji;
            public bool Fragment;
            public bool GroupsTruncated;
            readonly List<string> _files = new List<string>();
            readonly HashSet<string> _fileSet = new HashSet<string>(StringComparer.Ordinal);
            public readonly List<string> Ids = new List<string>();
            readonly HashSet<string> _idSet = new HashSet<string>(StringComparer.Ordinal);
            public readonly List<string> Classes = new List<string>();
            readonly HashSet<string> _classSet = new HashSet<string>(StringComparer.Ordinal);
            readonly List<string> _groups = new List<string>();
            readonly List<int> _runIndexes = new List<int>();
            readonly HashSet<string> _groupSet = new HashSet<string>(StringComparer.Ordinal);

            public void NoteFile(string name)
            {
                if (_fileSet.Add(name)) _files.Add(name);
            }

            public void NoteId(string id)
            {
                if (_idSet.Add(id)) Ids.Add(id);
            }

            public void NoteClass(string cls)
            {
                if (_classSet.Add(cls)) Classes.Add(cls);
            }

            public void NoteGroup(string group, int runIndex)
            {
                if (!_groupSet.Add(group)) return;
                if (_groups.Count >= MaxGroups)
                {
                    GroupsTruncated = true;
                    return;
                }
                _groups.Add(group);
                _runIndexes.Add(runIndex);
            }

            public string FilesField()
            {
                var sb = new StringBuilder();
                sb.Append(_files.Count.ToString(CultureInfo.InvariantCulture));
                sb.Append(": ");
                int n = Math.Min(MaxFilesListed, _files.Count);
                for (int i = 0; i < n; i++)
                {
                    if (i > 0) sb.Append("; ");
                    sb.Append(_files[i]);
                }
                return sb.ToString();
            }

            public string GroupsField()
            {
                return JoinRaw(_groups);
            }

            public string RunIndexField()
            {
                var sb = new StringBuilder();
                for (int i = 0; i < _runIndexes.Count; i++)
                {
                    if (i > 0) sb.Append("; ");
                    sb.Append(_runIndexes[i].ToString(CultureInfo.InvariantCulture));
                }
                return sb.ToString();
            }

            public string Notes()
            {
                var sb = new StringBuilder();
                if (Fragment) sb.Append("fragment");
                if (!LocText.HasTerminalPunctuation(Key))
                {
                    if (sb.Length > 0) sb.Append(';');
                    sb.Append("no-terminal-punct");
                }
                if (GroupsTruncated)
                {
                    if (sb.Length > 0) sb.Append(';');
                    sb.Append("groups-truncated");
                }
                return sb.ToString();
            }

            static string JoinRaw(List<string> values)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < values.Count; i++)
                {
                    if (i > 0) sb.Append("; ");
                    sb.Append(values[i]);
                }
                return sb.ToString();
            }
        }
    }
}
