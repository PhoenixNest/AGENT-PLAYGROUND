using System;
using System.Collections.Generic;
using System.Text;

namespace OrbitalSalvage.Loc
{
    /// <summary>
    /// Reader for an <c>OSB1</c> shaped-run table produced by <c>bake_shaped.py</c>.
    /// Runs are logical. Glyphs inside a run are in HarfBuzz order. The table is
    /// refused when the font file hash does not match, so glyph ids from another
    /// subset are never drawn.
    /// </summary>
    public sealed class ShapedTable
    {
        public const int KindGlyph = 1;
        public const int KindLatin = 2;
        public const int KindSymbol = 3;
        public const int KindEmoji = 4;
        public const int KindSlot = 5;

        public readonly bool Refused;
        public readonly string FontName;
        public readonly int Weight;
        public readonly int UnitsPerEm;
        public readonly byte[] FontHash;

        readonly Entry[] entries;
        readonly Dictionary<ulong, int> byHash;
        readonly Dictionary<ulong, Entry> emojiTail;
        readonly int[] slotted;
        readonly Dictionary<string, Resolved> cache = new Dictionary<string, Resolved>();

        ShapedTable(bool refused, string fontName, int weight, int upem, byte[] fontHash, Entry[] entries, int[] slotted)
        {
            Refused = refused;
            FontName = fontName ?? "";
            Weight = weight;
            UnitsPerEm = upem;
            FontHash = fontHash ?? Array.Empty<byte>();
            this.entries = entries ?? Array.Empty<Entry>();
            byHash = new Dictionary<ulong, int>(this.entries.Length);
            for (int i = 0; i < this.entries.Length; i++)
                byHash[this.entries[i].Hash] = i;
            this.slotted = slotted ?? Array.Empty<int>();
            emojiTail = BuildEmojiTails(this.entries, byHash);
        }

        /// <summary>
        /// Home drops a leading emoji when the spec already draws it as a child image.
        /// The cut keeps the space that belonged to the next run, so the lookup string
        /// is not a table key. The remaining runs of that entry are the label.
        /// </summary>
        static Dictionary<ulong, Entry> BuildEmojiTails(Entry[] entries, Dictionary<ulong, int> byHash)
        {
            var tails = new Dictionary<ulong, Entry>();
            if (entries == null) return tails;
            for (int i = 0; i < entries.Length; i++)
            {
                Entry entry = entries[i];
                if (entry == null || entry.Runs == null || entry.Runs.Length < 2) continue;
                if (entry.Runs[0].Kind != KindEmoji) continue;
                string prefix = entry.Runs[0].Text;
                if (string.IsNullOrEmpty(prefix) || entry.Template == null || !entry.Template.StartsWith(prefix, StringComparison.Ordinal)) continue;
                string suffix = entry.Template.Substring(prefix.Length);
                if (suffix.Length == 0) continue;
                ulong hash = Hash(suffix);
                if (byHash.ContainsKey(hash) || tails.ContainsKey(hash)) continue;
                var runs = new Run[entry.Runs.Length - 1];
                Array.Copy(entry.Runs, 1, runs, 0, runs.Length);
                tails[hash] = new Entry
                {
                    Hash = hash,
                    BaseLevel = entry.BaseLevel,
                    Runs = runs,
                    Template = suffix,
                    HasSlot = false
                };
            }
            return tails;
        }

        public static ShapedTable Empty(string fontName)
        {
            return new ShapedTable(false, fontName, 0, 0, Array.Empty<byte>(), Array.Empty<Entry>(), Array.Empty<int>());
        }

        public static ShapedTable Refuse(string fontName)
        {
            return new ShapedTable(true, fontName, 0, 0, Array.Empty<byte>(), Array.Empty<Entry>(), Array.Empty<int>());
        }

        public static ShapedTable Parse(byte[] data)
        {
            if (data == null || data.Length < 16) throw new InvalidOperationException("shaped table is empty");
            if (data[0] != (byte)'O' || data[1] != (byte)'S' || data[2] != (byte)'B' || data[3] != (byte)'1')
                throw new InvalidOperationException("shaped table magic is not OSB1");
            int version = U16(data, 4);
            if (version != 1) throw new InvalidOperationException("shaped table version " + version);
            int weight = U16(data, 6);
            int upem = U16(data, 8);
            int nameLen = data[10];
            if (11 + nameLen + 32 + 4 > data.Length) throw new InvalidOperationException("shaped table header is truncated");
            string font = Encoding.UTF8.GetString(data, 11, nameLen);
            int hashAt = 11 + nameLen;
            var hash = new byte[32];
            Buffer.BlockCopy(data, hashAt, hash, 0, 32);
            int count = U32(data, hashAt + 32);
            int indexAt = hashAt + 36;
            var entries = new Entry[count];
            var slotted = new List<int>();
            for (int i = 0; i < count; i++)
            {
                int at = indexAt + i * 16;
                ulong key = U64(data, at);
                int off = U32(data, at + 8);
                int size = U32(data, at + 12);
                if (off < 0 || size < 0 || off + size > data.Length) throw new InvalidOperationException("shaped record is out of range");
                Entry entry = ReadRecord(data, off, size, key);
                entries[i] = entry;
                if (entry.HasSlot) slotted.Add(i);
            }
            return new ShapedTable(false, font, weight, upem, hash, entries, slotted.ToArray());
        }

        /// <summary>Same bytes as <see cref="Parse"/>, then refuse the table when <paramref name="fontBytes"/> hash differs.</summary>
        public static ShapedTable Load(byte[] data, byte[] fontBytes)
        {
            ShapedTable table = Parse(data);
            if (fontBytes == null || !HashEquals(table.FontHash, Sha256(fontBytes)))
                return new ShapedTable(true, table.FontName, table.Weight, table.UnitsPerEm, table.FontHash, Array.Empty<Entry>(), Array.Empty<int>());
            return table;
        }

        public int Count
        {
            get { return entries.Length; }
        }

        public Entry EntryAt(int index)
        {
            return entries[index];
        }

        public bool TryGet(string text, out Entry entry)
        {
            entry = null;
            if (Refused || string.IsNullOrEmpty(text)) return false;
            int index;
            if (!byHash.TryGetValue(Hash(text), out index)) return false;
            entry = entries[index];
            return true;
        }

        public bool TryResolve(string text, out Resolved resolved)
        {
            resolved = null;
            if (Refused || string.IsNullOrEmpty(text)) return false;
            if (cache.TryGetValue(text, out resolved)) return resolved != null && resolved.Entry != null;
            ulong hash = Hash(text);
            int index;
            if (byHash.TryGetValue(hash, out index))
            {
                resolved = new Resolved(entries[index], Array.Empty<string>(), this);
                cache[text] = resolved;
                return true;
            }
            Entry tail;
            if (emojiTail != null && emojiTail.TryGetValue(hash, out tail))
            {
                resolved = new Resolved(tail, Array.Empty<string>(), this);
                cache[text] = resolved;
                return true;
            }
            Resolved best = null;
            int bestScore = -1;
            int ties = 0;
            for (int i = 0; i < slotted.Length; i++)
            {
                Entry entry = entries[slotted[i]];
                string[] values;
                if (!TryFill(entry, text, byHash, out values)) continue;
                int score = entry.Template.Length;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = new Resolved(entry, values, this);
                    ties = 1;
                }
                else if (score == bestScore) ties++;
            }
            if (ties != 1) best = null;
            cache[text] = best;
            resolved = best;
            return best != null;
        }

        public static ulong Hash(string text)
        {
            if (text == null) text = "";
            string nfc = text.Normalize(NormalizationForm.FormC);
            byte[] utf = Encoding.UTF8.GetBytes(nfc);
            ulong h = 14695981039346656037UL;
            for (int i = 0; i < utf.Length; i++)
            {
                h ^= utf[i];
                h *= 1099511628211UL;
            }
            return h;
        }

        public static bool ContainsScript(string text, string lang)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                int cp = char.ConvertToUtf32(text, i);
                if (char.IsHighSurrogate(text[i])) i++;
                if (InScript(cp, lang)) return true;
            }
            return false;
        }

        public static bool InScript(int cp, string lang)
        {
            if (lang == "ar")
                return (cp >= 0x0600 && cp <= 0x06FF) || (cp >= 0x0750 && cp <= 0x077F) || (cp >= 0x08A0 && cp <= 0x08FF)
                    || (cp >= 0xFB50 && cp <= 0xFDFF) || (cp >= 0xFE70 && cp <= 0xFEFF);
            if (lang == "hi")
                return (cp >= 0x0900 && cp <= 0x097F) || (cp >= 0x1CD0 && cp <= 0x1CFF) || (cp >= 0xA8E0 && cp <= 0xA8FF);
            if (lang == "km")
                return (cp >= 0x1780 && cp <= 0x17FF) || (cp >= 0x19E0 && cp <= 0x19FF);
            return false;
        }

        static Entry ReadRecord(byte[] data, int off, int size, ulong hash)
        {
            int end = off + size;
            int baseLevel = data[off];
            int runCount = U16(data, off + 1);
            int p = off + 3;
            var runs = new Run[runCount];
            bool hasSlot = false;
            var template = new StringBuilder();
            for (int r = 0; r < runCount; r++)
            {
                if (p + 5 > end) throw new InvalidOperationException("shaped run is truncated");
                int kind = data[p];
                int level = data[p + 1];
                int slot = data[p + 2];
                int textLen = U16(data, p + 3);
                p += 5;
                if (p + textLen > end) throw new InvalidOperationException("shaped run text is truncated");
                string text = Encoding.UTF8.GetString(data, p, textLen);
                p += textLen;
                int glyphCount = U16(data, p);
                p += 2;
                var glyphs = new Glyph[glyphCount];
                for (int g = 0; g < glyphCount; g++)
                {
                    if (p + 11 > end) throw new InvalidOperationException("shaped glyph is truncated");
                    glyphs[g] = new Glyph
                    {
                        Gid = U16(data, p),
                        Adv = I16(data, p + 2),
                        Dx = I16(data, p + 4),
                        Dy = I16(data, p + 6),
                        Cluster = U16(data, p + 8),
                        BreakBefore = (data[p + 10] & 1) != 0
                    };
                    p += 11;
                }
                if (kind == KindSlot) hasSlot = true;
                template.Append(text);
                runs[r] = new Run
                {
                    Kind = kind,
                    Level = level,
                    Slot = slot,
                    Text = text,
                    Glyphs = glyphs
                };
            }
            return new Entry
            {
                Hash = hash,
                BaseLevel = baseLevel,
                Runs = runs,
                Template = template.ToString(),
                HasSlot = hasSlot
            };
        }

        static bool TryFill(Entry entry, string painted, Dictionary<ulong, int> byHash, out string[] values)
        {
            values = null;
            string template = entry.Template;
            var found = new Dictionary<int, string>();
            int ti = 0;
            int pi = 0;
            while (ti < template.Length)
            {
                int slot;
                int token;
                if (TrySlotAt(template, ti, out slot, out token))
                {
                    int lit = ti + token;
                    int litEnd = lit;
                    while (litEnd < template.Length && !TrySlotAt(template, litEnd, out _, out _)) litEnd++;
                    string literal = template.Substring(lit, litEnd - lit);
                    int search = pi;
                    bool matched = false;
                    while (true)
                    {
                        int end;
                        if (literal.Length == 0) end = painted.Length;
                        else
                        {
                            end = painted.IndexOf(literal, search, StringComparison.Ordinal);
                            if (end < 0) return false;
                        }
                        if (end < pi) return false;
                        string mid = painted.Substring(pi, end - pi);
                        if (SlotOk(mid, byHash))
                        {
                            found[slot] = mid;
                            pi = end;
                            ti = lit;
                            matched = true;
                            break;
                        }
                        if (literal.Length == 0) return false;
                        search = end + 1;
                    }
                    if (!matched) return false;
                    continue;
                }
                if (pi >= painted.Length || painted[pi] != template[ti]) return false;
                pi++;
                ti++;
            }
            if (pi != painted.Length) return false;
            int max = 0;
            foreach (int key in found.Keys) if (key > max) max = key;
            values = new string[max + 1];
            foreach (KeyValuePair<int, string> kv in found) values[kv.Key] = kv.Value;
            return true;
        }

        static bool SlotOk(string mid, Dictionary<ulong, int> byHash)
        {
            if (string.IsNullOrEmpty(mid)) return false;
            if (IsWesternNumber(mid)) return true;
            if (!ContainsAnyScript(mid)) return true;
            int index;
            return byHash.TryGetValue(Hash(mid), out index);
        }

        static bool ContainsAnyScript(string text)
        {
            return ContainsScript(text, "ar") || ContainsScript(text, "hi") || ContainsScript(text, "km");
        }

        static bool TrySlotAt(string template, int index, out int slot, out int length)
        {
            slot = 0;
            length = 0;
            if (index >= template.Length || template[index] != '{') return false;
            int i = index + 1;
            if (i >= template.Length || template[i] < '0' || template[i] > '9') return false;
            int value = 0;
            while (i < template.Length && template[i] >= '0' && template[i] <= '9')
            {
                value = value * 10 + (template[i] - '0');
                i++;
            }
            if (i >= template.Length || template[i] != '}') return false;
            slot = value;
            length = i - index + 1;
            return true;
        }

        public static bool IsWesternNumber(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            int i = 0;
            if (text[0] == '+' || text[0] == '-') i++;
            if (i >= text.Length) return false;
            bool digit = false;
            bool dot = false;
            for (; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= '0' && c <= '9') { digit = true; continue; }
                if (c == '.' && !dot) { dot = true; continue; }
                return false;
            }
            return digit;
        }

        public static bool HashEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }

        public static byte[] Sha256(byte[] data)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
                return sha.ComputeHash(data ?? Array.Empty<byte>());
        }

        static int U16(byte[] d, int o) { return d[o] | (d[o + 1] << 8); }
        static int I16(byte[] d, int o)
        {
            int v = U16(d, o);
            return v >= 0x8000 ? v - 0x10000 : v;
        }
        static int U32(byte[] d, int o)
        {
            return d[o] | (d[o + 1] << 8) | (d[o + 2] << 16) | (d[o + 3] << 24);
        }
        static ulong U64(byte[] d, int o)
        {
            ulong lo = (uint)U32(d, o);
            ulong hi = (uint)U32(d, o + 4);
            return lo | (hi << 32);
        }

        public sealed class Entry
        {
            public ulong Hash;
            public int BaseLevel;
            public Run[] Runs;
            public string Template;
            public bool HasSlot;
        }

        public sealed class Run
        {
            public int Kind;
            public int Level;
            public int Slot;
            public string Text;
            public Glyph[] Glyphs;
        }

        public struct Glyph
        {
            public int Gid;
            public int Adv;
            public int Dx;
            public int Dy;
            public int Cluster;
            public bool BreakBefore;
        }

        public sealed class Resolved
        {
            public readonly Entry Entry;
            public readonly string[] Slots;
            public readonly ShapedTable Source;

            public Resolved(Entry entry, string[] slots, ShapedTable source)
            {
                Entry = entry;
                Slots = slots ?? Array.Empty<string>();
                Source = source;
            }

            public bool RightAlign
            {
                get { return Entry != null && (Entry.BaseLevel & 1) == 1; }
            }

            public string SlotText(int slot)
            {
                if (Slots == null || slot < 0 || slot >= Slots.Length || string.IsNullOrEmpty(Slots[slot])) return "";
                return Slots[slot];
            }
        }
    }
}
