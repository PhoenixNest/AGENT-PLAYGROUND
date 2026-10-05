using System;
using System.Collections.Generic;
using System.IO;

namespace OrbitalSalvage.Parity
{
    public sealed class ParityOptions
    {
        public string StatesPath;
        public string OutDir;
        public HashSet<string> Only;
        public string Profile;
        public string SummaryDir;

        public static bool HasFlag(string[] args)
        {
            return Array.IndexOf(args, "--parity") >= 0;
        }

        public static bool TryParse(string[] args, out ParityOptions options, out string error)
        {
            options = null;
            error = null;
            int idx = Array.IndexOf(args, "--parity");
            if (idx < 0 || idx + 2 >= args.Length || args[idx + 1].StartsWith("-") || args[idx + 2].StartsWith("-"))
            {
                error = "usage: --parity <statesJson> <outDir> [--only id1,id2] [--profile zero|iphone]";
                return false;
            }

            var result = new ParityOptions
            {
                StatesPath = Path.GetFullPath(args[idx + 1]),
                OutDir = Path.GetFullPath(args[idx + 2])
            };

            for (int i = idx + 3; i < args.Length; i++)
            {
                if (args[i] == "--only" && i + 1 < args.Length)
                {
                    result.Only = new HashSet<string>(args[++i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                }
                else if (args[i] == "--summary" && i + 1 < args.Length)
                {
                    result.SummaryDir = Path.GetFullPath(args[++i]);
                }
                else if (args[i] == "--profile" && i + 1 < args.Length)
                {
                    result.Profile = args[++i];
                    if (result.Profile != "zero" && result.Profile != "iphone")
                    {
                        error = $"unknown profile '{result.Profile}'";
                        return false;
                    }
                }
            }

            options = result;
            return true;
        }
    }

    public sealed class ParityStep
    {
        public string Op;
        public Dictionary<string, object> Raw;

        public string Str(string key, string fallback = null)
        {
            return Raw.TryGetValue(key, out var v) && v != null ? Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) : fallback;
        }

        public double Num(string key, double fallback = 0)
        {
            return Raw.TryGetValue(key, out var v) && v != null ? Convert.ToDouble(v, System.Globalization.CultureInfo.InvariantCulture) : fallback;
        }

        public bool Bool(string key, bool fallback)
        {
            return Raw.TryGetValue(key, out var v) && v is bool b ? b : fallback;
        }

        public string Json(string key)
        {
            return Raw.TryGetValue(key, out var v) ? MiniJson.Serialize(v) : "null";
        }
    }

    public sealed class ParityState
    {
        public string Id;
        public string Spec;
        public string Description;
        public bool HasTimeOffset;
        public double TimeOffsetMs;
        public List<string> Profiles = new List<string>();
        public List<ParityStep> Steps = new List<ParityStep>();
    }

    public static class ParityStateLoader
    {
        static readonly HashSet<string> KnownOps = new HashSet<string>
        {
            "boot", "showPage", "setProfile", "setApp", "clickSelector", "scroll", "call", "click",
            "pointer", "key", "advance", "loadSector", "holdPressed", "capture"
        };

        public static List<ParityState> Load(string path)
        {
            var root = MiniJson.Parse(File.ReadAllText(path)) as List<object>;
            if (root == null) throw new FormatException("states.json must be an array");

            var states = new List<ParityState>();
            foreach (var item in root)
            {
                var obj = item as Dictionary<string, object>;
                if (obj == null) throw new FormatException("state entry must be an object");

                var state = new ParityState
                {
                    Id = Convert.ToString(obj["id"]),
                    Description = obj.TryGetValue("description", out var d) ? Convert.ToString(d) : ""
                };

                if (obj.TryGetValue("spec", out var spec) && spec != null) state.Spec = Convert.ToString(spec);
                if (obj.TryGetValue("timeOffsetMs", out var off) && off != null)
                {
                    // Parity MiniJson stores JSON integers as long, not double.
                    state.HasTimeOffset = true;
                    state.TimeOffsetMs = Convert.ToDouble(off, System.Globalization.CultureInfo.InvariantCulture);
                }
                if (obj.TryGetValue("profiles", out var profs) && profs is List<object> plist)
                    foreach (var p in plist) state.Profiles.Add(Convert.ToString(p));

                if (!(obj["steps"] is List<object> steps) || steps.Count == 0)
                    throw new FormatException($"state '{state.Id}' has no steps");

                foreach (var s in steps)
                {
                    var so = s as Dictionary<string, object>;
                    if (so == null || !so.TryGetValue("op", out var opv)) throw new FormatException($"state '{state.Id}': step without op");
                    string op = Convert.ToString(opv);
                    if (!KnownOps.Contains(op)) throw new FormatException($"state '{state.Id}': unknown op '{op}'");
                    state.Steps.Add(new ParityStep { Op = op, Raw = so });
                }

                states.Add(state);
            }
            return states;
        }
    }
}
