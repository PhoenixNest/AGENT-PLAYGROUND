using System;
using System.Collections.Generic;

namespace OrbitalSalvage.Render
{
    /// <summary>One CSS <c>@keyframes</c> rule. Offsets are 0..1. A stop's easing applies to the segment that ends on it.</summary>
    public sealed class KeyframeSet
    {
        public struct Stop
        {
            public double T;
            public string Transform;
            public string Opacity;
            public string Shadow;
            public string Border;
            public string Easing;
        }

        readonly List<Stop> stops;

        public KeyframeSet(IEnumerable<Stop> stops)
        {
            this.stops = new List<Stop>(stops);
            this.stops.Sort((a, b) => a.T.CompareTo(b.T));
        }

        public int Count => stops.Count;

        public bool TryTransform(double directed, string animEase, out string from, out string to, out double u)
        {
            return Segment(directed, animEase, s => s.Transform, out from, out to, out u);
        }

        public bool TryOpacity(double directed, string animEase, out string from, out string to, out double u)
        {
            return Segment(directed, animEase, s => s.Opacity, out from, out to, out u);
        }

        public bool TryShadow(double directed, string animEase, out string from, out string to, out double u)
        {
            return Segment(directed, animEase, s => s.Shadow, out from, out to, out u);
        }

        public bool TryBorder(double directed, string animEase, out string from, out string to, out double u)
        {
            return Segment(directed, animEase, s => s.Border, out from, out to, out u);
        }

        bool Segment(double directed, string animEase, Func<Stop, string> pick, out string from, out string to, out double u)
        {
            from = to = null;
            u = 0;
            int n = 0;
            for (int i = 0; i < stops.Count; i++) if (!string.IsNullOrEmpty(pick(stops[i]))) n++;
            if (n == 0) return false;
            int prev = -1;
            for (int i = 0; i < stops.Count; i++)
            {
                string value = pick(stops[i]);
                if (string.IsNullOrEmpty(value)) continue;
                if (directed <= stops[i].T || prev < 0 && i == LastWith(pick))
                {
                    if (prev < 0 || Math.Abs(directed - stops[i].T) < 1e-9 || Math.Abs(stops[i].T - stops[prev].T) < 1e-9)
                    {
                        from = to = value;
                        u = 0;
                        return true;
                    }
                    double span = stops[i].T - stops[prev].T;
                    double local = span <= 1e-9 ? 0 : (directed - stops[prev].T) / span;
                    if (local < 0) local = 0;
                    if (local > 1) local = 1;
                    string ease = string.IsNullOrEmpty(stops[i].Easing) ? animEase : stops[i].Easing;
                    from = pick(stops[prev]);
                    to = value;
                    u = CssEasing.Ease(string.IsNullOrEmpty(ease) ? "ease" : ease, local);
                    return true;
                }
                prev = i;
            }
            string last = pick(stops[LastWith(pick)]);
            from = to = last;
            u = 0;
            return !string.IsNullOrEmpty(last);
        }

        int LastWith(Func<Stop, string> pick)
        {
            for (int i = stops.Count - 1; i >= 0; i--)
                if (!string.IsNullOrEmpty(pick(stops[i]))) return i;
            return 0;
        }

        public static Dictionary<string, KeyframeSet> ParseLibrary(string json)
        {
            var doc = new Dictionary<string, KeyframeSet>();
            var root = MiniJson.Parse(json) as Dictionary<string, object>;
            if (root == null) return doc;
            foreach (var kv in root)
            {
                if (!(kv.Value is List<object> list)) continue;
                var stops = new List<Stop>();
                foreach (var item in list)
                {
                    if (!(item is Dictionary<string, object> o)) continue;
                    stops.Add(new Stop
                    {
                        T = Num(o, "t"),
                        Transform = Str(o, "tf"),
                        Opacity = Str(o, "o"),
                        Shadow = Str(o, "sh"),
                        Border = Str(o, "bc"),
                        Easing = Str(o, "ea")
                    });
                }
                doc[kv.Key] = new KeyframeSet(stops);
            }
            return doc;
        }

        static string Str(Dictionary<string, object> o, string key)
        {
            if (!o.TryGetValue(key, out var v) || v == null) return null;
            return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
        }

        static double Num(Dictionary<string, object> o, string key)
        {
            return o.TryGetValue(key, out var v) && v is double d ? d : 0;
        }
    }
}
