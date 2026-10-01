using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic.Tests
{
    internal static class Fixtures
    {
        private const string Relative = "tools/parity/golden/fixtures";

        public static string Dir
        {
            get
            {
                string env = Environment.GetEnvironmentVariable("PARITY_GOLDEN_FIXTURES");
                if (!string.IsNullOrEmpty(env) && Directory.Exists(env)) return env;

                var starts = new List<string>();
                starts.Add(Directory.GetCurrentDirectory());
                starts.Add(AppDomain.CurrentDomain.BaseDirectory);
                foreach (string start in starts)
                {
                    string cwdRelative = Path.GetFullPath(Path.Combine(start, "../../../../" + Relative));
                    if (Directory.Exists(cwdRelative)) return cwdRelative;
                    string dir = start;
                    for (int i = 0; i < 14 && !string.IsNullOrEmpty(dir); i++)
                    {
                        string cand = Path.Combine(dir, Relative);
                        if (Directory.Exists(cand)) return cand;
                        dir = Path.GetDirectoryName(dir);
                    }
                }
                throw new DirectoryNotFoundException("golden fixtures directory not found; run `node tools/parity/golden/generate.mjs` (see tools/parity/golden/README.md)");
            }
        }

        public static string Path_(string name)
        {
            return Path.Combine(Dir, name);
        }

        public static bool Exists(string name)
        {
            return File.Exists(Path_(name));
        }

        public static string ReadText(string name)
        {
            string path = Path_(name);
            if (!File.Exists(path)) throw new FileNotFoundException("missing fixture " + name + "; run `node tools/parity/golden/generate.mjs` (see tools/parity/golden/README.md)", path);
            if (name.EndsWith(".gz", StringComparison.Ordinal))
            {
                using (var fs = File.OpenRead(path))
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                using (var sr = new StreamReader(gz, new UTF8Encoding(false)))
                {
                    return sr.ReadToEnd();
                }
            }
            return File.ReadAllText(path, Encoding.UTF8);
        }

        public static JsonValue Load(string name)
        {
            return JsonParser.Parse(ReadText(name));
        }

        public static double D(string hex)
        {
            return BitConverter.Int64BitsToDouble(unchecked((long)ulong.Parse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)));
        }

        public static string Hex(double d)
        {
            return unchecked((ulong)BitConverter.DoubleToInt64Bits(d)).ToString("x16", CultureInfo.InvariantCulture);
        }

        public static bool SameBits(double expected, double actual)
        {
            if (double.IsNaN(expected)) return double.IsNaN(actual);
            return BitConverter.DoubleToInt64Bits(expected) == BitConverter.DoubleToInt64Bits(actual);
        }

        public static long UlpDistance(double a, double b)
        {
            if (double.IsNaN(a) || double.IsNaN(b)) return long.MaxValue;
            long ia = BitConverter.DoubleToInt64Bits(a);
            long ib = BitConverter.DoubleToInt64Bits(b);
            if (ia < 0) ia = long.MinValue - ia;
            if (ib < 0) ib = long.MinValue - ib;
            long d = ia - ib;
            return d < 0 ? -d : d;
        }
    }
}
