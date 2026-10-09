using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace OrbitalSalvage.Editor
{
    /// <summary>
    /// Text edit of a pbxproj. No reference to UnityEditor.iOS.Xcode, which is absent without the iOS module.
    /// Finds a PBXNativeTarget by <c>name</c> (ordinal), otherwise by <c>productName</c>, then edits only the
    /// PRODUCT_BUNDLE_IDENTIFIER values inside that target's XCBuildConfiguration buildSettings.
    /// Comments, PBXAggregateTarget entries, and every other target are left untouched.
    /// </summary>
    public static class PbxprojBundleId
    {
        public enum Status
        {
            NotFound,
            Updated,
            Unchanged,
            Ambiguous
        }

        public sealed class Result
        {
            public Result(Status kind, int configurationsUpdated, int configurationsExamined, string[] nativeTargetNames)
            {
                Kind = kind;
                ConfigurationsUpdated = configurationsUpdated;
                ConfigurationsExamined = configurationsExamined;
                NativeTargetNames = nativeTargetNames ?? Array.Empty<string>();
            }

            public Status Kind { get; }
            public int ConfigurationsUpdated { get; }
            public int ConfigurationsExamined { get; }
            public string[] NativeTargetNames { get; }
        }

        public static string SetTargetBundleId(string pbxprojText, string targetName, string bundleId, out Result result)
        {
            if (pbxprojText == null)
            {
                result = new Result(Status.NotFound, 0, 0, Array.Empty<string>());
                return null;
            }
            if (targetName == null) throw new ArgumentNullException(nameof(targetName));
            if (bundleId == null) throw new ArgumentNullException(nameof(bundleId));
            if (bundleId.IndexOf('\n') >= 0 || bundleId.IndexOf('\r') >= 0)
                throw new ArgumentException("bundle id must be a single line", nameof(bundleId));

            var reader = new Reader(pbxprojText);
            if (reader.Peek() != '{')
                throw new FormatException("pbxproj root is not a dictionary");
            Dict root = reader.ParseDict();

            Dict objects = null;
            for (int i = 0; i < root.Entries.Count; i++)
            {
                if (root.Entries[i].Key == "objects" && root.Entries[i].Value is DictValue dictValue)
                    objects = dictValue.Dict;
            }
            if (objects == null)
            {
                result = new Result(Status.NotFound, 0, 0, Array.Empty<string>());
                return pbxprojText;
            }

            var byId = new Dictionary<string, Obj>();
            var order = new List<Obj>();
            for (int i = 0; i < objects.Entries.Count; i++)
            {
                if (!(objects.Entries[i].Value is DictValue dictValue)) continue;
                Obj obj = ReadObj(objects.Entries[i].Key, dictValue.Dict);
                order.Add(obj);
                if (!byId.ContainsKey(obj.Id)) byId.Add(obj.Id, obj);
            }

            var names = new List<string>();
            var byName = new List<Obj>();
            var byProduct = new List<Obj>();
            for (int i = 0; i < order.Count; i++)
            {
                Obj obj = order[i];
                if (obj.Isa != "PBXNativeTarget") continue;
                if (!string.IsNullOrEmpty(obj.Name)) names.Add(obj.Name);
                else if (!string.IsNullOrEmpty(obj.ProductName)) names.Add(obj.ProductName);
                if (obj.Name == targetName) byName.Add(obj);
                else if (obj.ProductName == targetName) byProduct.Add(obj);
            }
            string[] nameArray = names.ToArray();

            if (byName.Count > 1 || (byName.Count == 0 && byProduct.Count > 1))
            {
                result = new Result(Status.Ambiguous, 0, 0, nameArray);
                return pbxprojText;
            }

            Obj match = byName.Count == 1 ? byName[0] : (byProduct.Count == 1 ? byProduct[0] : null);
            if (match == null)
            {
                result = new Result(Status.NotFound, 0, 0, nameArray);
                return pbxprojText;
            }

            if (match.ConfigListId == null || !byId.TryGetValue(match.ConfigListId, out Obj list) ||
                list.Isa != "XCConfigurationList" || list.ConfigIds == null)
            {
                result = new Result(Status.Unchanged, 0, 0, nameArray);
                return pbxprojText;
            }

            var edits = new List<Edit>();
            int updated = 0;
            int examined = 0;
            for (int i = 0; i < list.ConfigIds.Count; i++)
            {
                if (!byId.TryGetValue(list.ConfigIds[i], out Obj config)) continue;
                if (config.Isa != "XCBuildConfiguration" || config.BuildSettings == null) continue;
                examined++;
                if (config.BundleIds.Count == 0)
                {
                    if (config.NonLiteralBundleId) continue;
                    edits.Add(new Edit
                    {
                        Start = config.BuildSettings.Close,
                        End = config.BuildSettings.Close,
                        Replacement = Insertion(pbxprojText, config.BuildSettings, bundleId)
                    });
                    updated++;
                    continue;
                }

                bool changed = false;
                for (int b = 0; b < config.BundleIds.Count; b++)
                {
                    LiteralValue literal = config.BundleIds[b];
                    if (literal.Decoded == bundleId) continue;
                    string replacement = literal.Quoted ? EscapeQuoted(bundleId) : bundleId;
                    if (!literal.Quoted && !IsSafeUnquoted(bundleId))
                        throw new ArgumentException("bundle id must be quoted", nameof(bundleId));
                    edits.Add(new Edit { Start = literal.Start, End = literal.End, Replacement = replacement });
                    changed = true;
                }
                if (changed) updated++;
            }

            if (updated == 0)
            {
                result = new Result(Status.Unchanged, 0, examined, nameArray);
                return pbxprojText;
            }

            result = new Result(Status.Updated, updated, examined, nameArray);
            return ApplyEdits(pbxprojText, edits);
        }

        static Obj ReadObj(string id, Dict dict)
        {
            var obj = new Obj { Id = id };
            for (int i = 0; i < dict.Entries.Count; i++)
            {
                Entry entry = dict.Entries[i];
                if (entry.Key == "isa" && entry.Value is LiteralValue isa) obj.Isa = isa.Decoded;
                else if (entry.Key == "name" && entry.Value is LiteralValue name) obj.Name = name.Decoded;
                else if (entry.Key == "productName" && entry.Value is LiteralValue product) obj.ProductName = product.Decoded;
                else if (entry.Key == "buildConfigurationList" && entry.Value is LiteralValue list) obj.ConfigListId = list.Decoded;
                else if (entry.Key == "buildConfigurations" && entry.Value is ListValue configs)
                {
                    obj.ConfigIds = new List<string>();
                    for (int c = 0; c < configs.Items.Count; c++)
                    {
                        if (configs.Items[c] is LiteralValue item) obj.ConfigIds.Add(item.Decoded);
                    }
                }
                else if (entry.Key == "buildSettings" && entry.Value is DictValue settings)
                {
                    obj.BuildSettings = settings.Dict;
                    for (int s = 0; s < settings.Dict.Entries.Count; s++)
                    {
                        Entry setting = settings.Dict.Entries[s];
                        if (setting.Key != "PRODUCT_BUNDLE_IDENTIFIER") continue;
                        if (setting.Value is LiteralValue bundle) obj.BundleIds.Add(bundle);
                        else obj.NonLiteralBundleId = true;
                    }
                }
            }
            return obj;
        }

        static string Insertion(string text, Dict settings, string bundleId)
        {
            bool quote = ShouldQuote(settings, bundleId);
            string value = quote ? "\"" + EscapeQuoted(bundleId) + "\"" : bundleId;
            string line = "PRODUCT_BUNDLE_IDENTIFIER = " + value + ";";
            string newline = NewlineBefore(text, settings.Close);
            if (newline == null) return " " + line;
            return newline + KeyIndent(text, settings) + line;
        }

        static bool ShouldQuote(Dict settings, string bundleId)
        {
            if (!IsSafeUnquoted(bundleId)) return true;
            bool quoted = false;
            bool unquoted = false;
            for (int i = 0; i < settings.Entries.Count; i++)
            {
                if (!(settings.Entries[i].Value is LiteralValue literal)) continue;
                if (literal.Quoted) quoted = true;
                else unquoted = true;
            }
            if (unquoted) return false;
            return quoted;
        }

        static bool IsSafeUnquoted(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c >= 'a' && c <= 'z') continue;
                if (c >= 'A' && c <= 'Z') continue;
                if (c >= '0' && c <= '9') continue;
                if (c == '.' || c == '_' || c == '-' || c == '/') continue;
                return false;
            }
            return true;
        }

        static string EscapeQuoted(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        static string NewlineBefore(string text, int close)
        {
            int i = close - 1;
            while (i >= 0 && (text[i] == ' ' || text[i] == '\t')) i--;
            if (i >= 0 && text[i] == '\n')
                return (i > 0 && text[i - 1] == '\r') ? "\r\n" : "\n";
            return null;
        }

        static string KeyIndent(string text, Dict settings)
        {
            for (int e = 0; e < settings.Entries.Count; e++)
            {
                int keyStart = settings.Entries[e].KeyStart;
                int line = keyStart == 0 ? -1 : text.LastIndexOf('\n', keyStart - 1);
                int start = line < 0 ? 0 : line + 1;
                int i = start;
                while (i < keyStart && (text[i] == ' ' || text[i] == '\t')) i++;
                if (i == keyStart) return text.Substring(start, keyStart - start);
            }
            int close = settings.Close;
            int braceLine = close == 0 ? -1 : text.LastIndexOf('\n', close - 1);
            int braceStart = braceLine < 0 ? 0 : braceLine + 1;
            int b = braceStart;
            while (b < close && (text[b] == ' ' || text[b] == '\t')) b++;
            string braceIndent = text.Substring(braceStart, b - braceStart);
            if (braceIndent.IndexOf('\t') >= 0) return braceIndent + "\t";
            return braceIndent + "    ";
        }

        static string ApplyEdits(string text, List<Edit> edits)
        {
            edits.Sort(delegate(Edit a, Edit b) { return a.Start.CompareTo(b.Start); });
            var sb = new StringBuilder(text.Length + edits.Count * 48);
            int cursor = 0;
            for (int i = 0; i < edits.Count; i++)
            {
                Edit edit = edits[i];
                if (edit.Start < cursor || edit.End < edit.Start || edit.End > text.Length)
                    throw new FormatException("overlapping pbxproj edit");
                sb.Append(text, cursor, edit.Start - cursor);
                sb.Append(edit.Replacement);
                cursor = edit.End;
            }
            sb.Append(text, cursor, text.Length - cursor);
            return sb.ToString();
        }

        struct Edit
        {
            public int Start;
            public int End;
            public string Replacement;
        }

        sealed class Obj
        {
            public string Id;
            public string Isa;
            public string Name;
            public string ProductName;
            public string ConfigListId;
            public List<string> ConfigIds;
            public Dict BuildSettings;
            public List<LiteralValue> BundleIds = new List<LiteralValue>();
            public bool NonLiteralBundleId;
        }

        sealed class Dict
        {
            public int Close;
            public List<Entry> Entries = new List<Entry>();
        }

        sealed class Entry
        {
            public string Key;
            public int KeyStart;
            public Value Value;
        }

        abstract class Value { }

        sealed class LiteralValue : Value
        {
            public string Decoded;
            public int Start;
            public int End;
            public bool Quoted;
        }

        sealed class DictValue : Value
        {
            public Dict Dict;
        }

        sealed class ListValue : Value
        {
            public List<Value> Items = new List<Value>();
        }

        sealed class Reader
        {
            public readonly string Text;
            public int Index;

            public Reader(string text)
            {
                Text = text;
            }

            public char Peek()
            {
                Skip();
                return Index < Text.Length ? Text[Index] : '\0';
            }

            public void Skip()
            {
                while (Index < Text.Length)
                {
                    char c = Text[Index];
                    if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                    {
                        Index++;
                        continue;
                    }
                    if (c == '/' && Index + 1 < Text.Length && Text[Index + 1] == '/')
                    {
                        Index += 2;
                        while (Index < Text.Length && Text[Index] != '\n') Index++;
                        continue;
                    }
                    if (c == '/' && Index + 1 < Text.Length && Text[Index + 1] == '*')
                    {
                        Index += 2;
                        while (Index + 1 < Text.Length && !(Text[Index] == '*' && Text[Index + 1] == '/')) Index++;
                        if (Index + 1 < Text.Length) Index += 2;
                        else Index = Text.Length;
                        continue;
                    }
                    return;
                }
            }

            public Dict ParseDict()
            {
                Skip();
                if (Index >= Text.Length || Text[Index] != '{')
                    throw new FormatException("expected { at " + Index);
                var dict = new Dict();
                Index++;
                while (true)
                {
                    char c = Peek();
                    if (c == '\0') throw new FormatException("unterminated dictionary at " + Index);
                    if (c == '}')
                    {
                        dict.Close = Index;
                        Index++;
                        return dict;
                    }
                    int keyStart = Index;
                    string key = ReadLiteral().Decoded;
                    if (Peek() != '=') throw new FormatException("expected = after " + key);
                    Index++;
                    Value value = ParseValue();
                    if (Peek() != ';') throw new FormatException("expected ; after " + key);
                    Index++;
                    dict.Entries.Add(new Entry { Key = key, KeyStart = keyStart, Value = value });
                }
            }

            Value ParseValue()
            {
                char c = Peek();
                if (c == '{') return new DictValue { Dict = ParseDict() };
                if (c == '(') return ParseList();
                if (c == '\0') throw new FormatException("expected value at " + Index);
                return ReadLiteral();
            }

            ListValue ParseList()
            {
                Skip();
                if (Index >= Text.Length || Text[Index] != '(')
                    throw new FormatException("expected ( at " + Index);
                Index++;
                var list = new ListValue();
                while (true)
                {
                    char c = Peek();
                    if (c == '\0') throw new FormatException("unterminated list at " + Index);
                    if (c == ')')
                    {
                        Index++;
                        return list;
                    }
                    list.Items.Add(ParseValue());
                    if (Peek() == ',') Index++;
                }
            }

            LiteralValue ReadLiteral()
            {
                Skip();
                if (Index >= Text.Length) throw new FormatException("expected value at " + Index);
                if (Text[Index] == '"') return ReadQuoted();
                return ReadUnquoted();
            }

            LiteralValue ReadQuoted()
            {
                Index++;
                int contentStart = Index;
                var sb = new StringBuilder();
                while (Index < Text.Length)
                {
                    char c = Text[Index];
                    if (c == '\\')
                    {
                        if (Index + 1 >= Text.Length) throw new FormatException("bad escape at " + Index);
                        sb.Append(Text[Index + 1]);
                        Index += 2;
                        continue;
                    }
                    if (c == '"')
                    {
                        int contentEnd = Index;
                        Index++;
                        return new LiteralValue
                        {
                            Decoded = sb.ToString(),
                            Start = contentStart,
                            End = contentEnd,
                            Quoted = true
                        };
                    }
                    sb.Append(c);
                    Index++;
                }
                throw new FormatException("unterminated string at " + contentStart);
            }

            LiteralValue ReadUnquoted()
            {
                int start = Index;
                while (Index < Text.Length)
                {
                    char c = Text[Index];
                    if (IsStop(c)) break;
                    if (c == '/' && Index + 1 < Text.Length && (Text[Index + 1] == '*' || Text[Index + 1] == '/')) break;
                    if (c == '$' && Index + 1 < Text.Length && Text[Index + 1] == '(')
                    {
                        Index++;
                        int depth = 0;
                        while (Index < Text.Length)
                        {
                            char d = Text[Index];
                            Index++;
                            if (d == '(') depth++;
                            else if (d == ')')
                            {
                                depth--;
                                if (depth == 0) break;
                            }
                        }
                        continue;
                    }
                    Index++;
                }
                if (Index == start) throw new FormatException("empty token at " + start);
                return new LiteralValue
                {
                    Decoded = Text.Substring(start, Index - start),
                    Start = start,
                    End = Index,
                    Quoted = false
                };
            }

            static bool IsStop(char c)
            {
                return c == ' ' || c == '\t' || c == '\r' || c == '\n' ||
                    c == ';' || c == ',' || c == '=' || c == '{' || c == '}' ||
                    c == '(' || c == ')' || c == '"';
            }
        }
    }

    /// <summary>
    /// Runs after an iOS player build and sets the GameAssembly bundle id.
    /// Export entry points do not call this directly. BuildPipeline.BuildPlayer invokes it.
    /// </summary>
    public static class IosXcodePostProcess
    {
        public const int CallbackOrder = 100;
        const string MissingPbxproj = "[IosExport] project.pbxproj not found; no change made.";

        [PostProcessBuild(CallbackOrder)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;
            try
            {
                string pbx = Locate(pathToBuiltProject);
                if (pbx == null)
                {
                    Debug.LogWarning(MissingPbxproj);
                    return;
                }

                string text = ReadUtf8(pbx);
                string updated = PbxprojBundleId.SetTargetBundleId(
                    text,
                    IosExport.GameAssemblyTargetName,
                    IosExport.GameAssemblyBundleId,
                    out PbxprojBundleId.Result result);

                if (result.Kind == PbxprojBundleId.Status.NotFound)
                {
                    Debug.LogWarning(
                        "[IosExport] target " + IosExport.GameAssemblyTargetName +
                        " not found in project.pbxproj; targets: " + string.Join(", ", result.NativeTargetNames) +
                        ". No change made.");
                    return;
                }
                if (result.Kind == PbxprojBundleId.Status.Ambiguous)
                {
                    Debug.LogWarning(
                        "[IosExport] target " + IosExport.GameAssemblyTargetName +
                        " matched more than one PBXNativeTarget; targets: " + string.Join(", ", result.NativeTargetNames) +
                        ". No change made.");
                    return;
                }
                if (result.Kind != PbxprojBundleId.Status.Updated || string.Equals(updated, text, StringComparison.Ordinal))
                {
                    if (result.ConfigurationsExamined == 0)
                    {
                        Debug.LogWarning(
                            "[IosExport] target " + IosExport.GameAssemblyTargetName +
                            " has no XCBuildConfiguration buildSettings to edit. No change made.");
                    }
                    else
                    {
                        Debug.Log(
                            "[IosExport] GameAssembly bundle id already " + IosExport.GameAssemblyBundleId +
                            ". No change made.");
                    }
                    return;
                }

                File.WriteAllBytes(pbx, new UTF8Encoding(false).GetBytes(updated));
                Debug.Log(
                    "[IosExport] GameAssembly bundle id set to " + IosExport.GameAssemblyBundleId +
                    " in " + result.ConfigurationsUpdated + " configuration(s)");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[IosExport] GameAssembly bundle id post-process failed: " + ex.Message + ". No change made.");
            }
        }

        static string Locate(string pathToBuiltProject)
        {
            if (string.IsNullOrEmpty(pathToBuiltProject) || !Directory.Exists(pathToBuiltProject))
                return null;
            string preferred = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj", "project.pbxproj");
            if (File.Exists(preferred)) return preferred;

            string[] projects;
            try
            {
                projects = Directory.GetDirectories(pathToBuiltProject, "*.xcodeproj");
            }
            catch (Exception)
            {
                return null;
            }

            string found = null;
            int count = 0;
            for (int i = 0; i < projects.Length; i++)
            {
                string candidate = Path.Combine(projects[i], "project.pbxproj");
                if (!File.Exists(candidate)) continue;
                count++;
                found = candidate;
            }
            return count == 1 ? found : null;
        }

        static string ReadUtf8(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            int start = 0;
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                start = 3;
            return new UTF8Encoding(false, true).GetString(bytes, start, bytes.Length - start);
        }
    }
}
