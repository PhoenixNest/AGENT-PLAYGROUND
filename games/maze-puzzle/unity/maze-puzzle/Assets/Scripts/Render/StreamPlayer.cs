using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    /// <summary>Replays a recorded command stream or a synthetic scene (tools/parity/canvas-conformance) on a Canvas2D.</summary>
    public sealed class StreamPlayer
    {
        readonly Dictionary<int, CanvasGradient> gradients = new Dictionary<int, CanvasGradient>();
        readonly Dictionary<string, CanvasImage> images = new Dictionary<string, CanvasImage>();
        readonly List<Texture2D> owned = new List<Texture2D>();

        public int Width, Height;
        public double Dpr;
        public string Name;
        public List<object> Preamble, Ops;
        public Dictionary<string, object> Meta;

        public static StreamPlayer Load(string json)
        {
            var root = (Dictionary<string, object>)MiniJson.Parse(json);
            var p = new StreamPlayer();
            p.Name = root.TryGetValue("name", out var n) ? (string)n : "stream";
            p.Meta = (Dictionary<string, object>)root["meta"];
            p.Dpr = p.Meta.TryGetValue("dpr", out var d) ? (double)d : 3.0;
            p.Width = (int)Math.Round((double)p.Meta["width"] * p.Dpr);
            p.Height = (int)Math.Round((double)p.Meta["height"] * p.Dpr);
            p.Preamble = root.TryGetValue("preamble", out var pre) ? (List<object>)pre : new List<object>();
            p.Ops = (List<object>)root["ops"];
            if (root.TryGetValue("images", out var imgs))
                foreach (var kv in (Dictionary<string, object>)imgs)
                {
                    var spec = (Dictionary<string, object>)kv.Value;
                    int size = spec.TryGetValue("size", out var s) ? (int)(double)s : 64;
                    var tex = ProceduralTexture(size);
                    p.owned.Add(tex);
                    p.images[kv.Key] = new CanvasImage(tex);
                }
            return p;
        }

        public static Texture2D ProceduralTexture(int n)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    byte r = (byte)Math.Round(i * 255.0 / (n - 1));
                    byte g = (byte)Math.Round(j * 255.0 / (n - 1));
                    byte b = (byte)((((i >> 3) ^ (j >> 3)) & 1) * 255);
                    byte a = (byte)(((i + j) & 15) < 8 ? 255 : 128);
                    px[(n - 1 - j) * n + i] = new Color32(r, g, b, a);
                }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            return tex;
        }

        public void Dispose()
        {
            foreach (var t in owned) UnityEngine.Object.DestroyImmediate(t);
            owned.Clear();
        }

        public void Play(Canvas2D c)
        {
            gradients.Clear();
            foreach (var op in Preamble) Run(c, (List<object>)op);
            foreach (var op in Ops) Run(c, (List<object>)op);
        }

        static double N(object o)
        {
            if (o is double d) return d;
            if (o is string s)
            {
                if (s == "NaN") return double.NaN;
                if (s == "Infinity") return double.PositiveInfinity;
                if (s == "-Infinity") return double.NegativeInfinity;
                if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v)) return v;
                return double.NaN;
            }
            if (o is bool b) return b ? 1 : 0;
            return double.NaN;
        }

        object Resolve(object o)
        {
            if (o is Dictionary<string, object> d)
            {
                if (d.TryGetValue("g", out var g)) return gradients[(int)(double)g];
                if (d.TryGetValue("img", out var im)) return images[(string)im];
                if (d.TryGetValue("emoji", out var em)) return CanvasEmoji.Get((string)em);
            }
            return o;
        }

        static Vec2d Radius(object o)
        {
            if (o is Dictionary<string, object> d) return new Vec2d(N(d["x"]), N(d["y"]));
            double v = N(o);
            return new Vec2d(v, v);
        }

        void Run(Canvas2D c, List<object> op)
        {
            string name = (string)op[0];
            if (name[0] == '=') { SetProperty(c, name.Substring(1), Resolve(op[1])); return; }
            switch (name)
            {
                case "createLinearGradient":
                    gradients[(int)N(op[1])] = c.CreateLinearGradient(N(op[2]), N(op[3]), N(op[4]), N(op[5]));
                    return;
                case "createRadialGradient":
                    gradients[(int)N(op[1])] = c.CreateRadialGradient(N(op[2]), N(op[3]), N(op[4]), N(op[5]), N(op[6]), N(op[7]));
                    return;
                case "addColorStop":
                    gradients[(int)N(op[1])].AddColorStop(N(op[2]), (string)op[3]);
                    return;
                case "save": c.Save(); return;
                case "restore": c.Restore(); return;
                case "beginPath": c.BeginPath(); return;
                case "closePath": c.ClosePath(); return;
                case "resetTransform": c.ResetTransform(); return;
                case "setTransform": c.SetTransform(N(op[1]), N(op[2]), N(op[3]), N(op[4]), N(op[5]), N(op[6])); return;
                case "transform": c.Transform(N(op[1]), N(op[2]), N(op[3]), N(op[4]), N(op[5]), N(op[6])); return;
                case "translate": c.Translate(N(op[1]), N(op[2])); return;
                case "scale": c.Scale(N(op[1]), N(op[2])); return;
                case "rotate": c.Rotate(N(op[1])); return;
                case "moveTo": c.MoveTo(N(op[1]), N(op[2])); return;
                case "lineTo": c.LineTo(N(op[1]), N(op[2])); return;
                case "quadraticCurveTo": c.QuadraticCurveTo(N(op[1]), N(op[2]), N(op[3]), N(op[4])); return;
                case "bezierCurveTo": c.BezierCurveTo(N(op[1]), N(op[2]), N(op[3]), N(op[4]), N(op[5]), N(op[6])); return;
                case "arcTo": c.ArcTo(N(op[1]), N(op[2]), N(op[3]), N(op[4]), N(op[5])); return;
                case "arc": c.Arc(N(op[1]), N(op[2]), N(op[3]), N(op[4]), N(op[5]), op.Count > 6 && op[6] is bool b6 && b6); return;
                case "ellipse": c.Ellipse(N(op[1]), N(op[2]), N(op[3]), N(op[4]), N(op[5]), N(op[6]), N(op[7]), op.Count > 8 && op[8] is bool b8 && b8); return;
                case "rect": c.Rect(N(op[1]), N(op[2]), N(op[3]), N(op[4])); return;
                case "roundRect":
                {
                    var radii = new List<Vec2d>();
                    if (op.Count > 5)
                    {
                        if (op[5] is List<object> rl) foreach (var r in rl) radii.Add(Radius(r));
                        else radii.Add(Radius(op[5]));
                    }
                    else radii.Add(new Vec2d(0, 0));
                    c.RoundRect(N(op[1]), N(op[2]), N(op[3]), N(op[4]), radii.ToArray());
                    return;
                }
                case "fill": c.Fill(op.Count > 1 && op[1] is string fr ? fr : "nonzero"); return;
                case "stroke": c.Stroke(); return;
                case "clip": c.Clip(op.Count > 1 && op[1] is string cr ? cr : "nonzero"); return;
                case "fillRect": c.FillRect(N(op[1]), N(op[2]), N(op[3]), N(op[4])); return;
                case "strokeRect": c.StrokeRect(N(op[1]), N(op[2]), N(op[3]), N(op[4])); return;
                case "clearRect": c.ClearRect(N(op[1]), N(op[2]), N(op[3]), N(op[4])); return;
                case "setLineDash":
                {
                    var l = (List<object>)op[1];
                    var arr = new double[l.Count];
                    for (int i = 0; i < arr.Length; i++) arr[i] = N(l[i]);
                    c.SetLineDash(arr);
                    return;
                }
                case "fillText": c.FillText((string)op[1], N(op[2]), N(op[3]), op.Count > 4 ? N(op[4]) : double.NaN); return;
                case "strokeText": c.StrokeText((string)op[1], N(op[2]), N(op[3]), op.Count > 4 ? N(op[4]) : double.NaN); return;
                case "drawImage":
                {
                    var img = Resolve(op[1]) as CanvasImage;
                    if (img == null) return;
                    if (op.Count == 4) c.DrawImage(img, N(op[2]), N(op[3]));
                    else if (op.Count == 6) c.DrawImage(img, N(op[2]), N(op[3]), N(op[4]), N(op[5]));
                    else c.DrawImage(img, N(op[2]), N(op[3]), N(op[4]), N(op[5]), N(op[6]), N(op[7]), N(op[8]), N(op[9]));
                    return;
                }
                default:
                    throw new InvalidOperationException("unsupported canvas op: " + name);
            }
        }

        static void SetProperty(Canvas2D c, string prop, object v)
        {
            switch (prop)
            {
                case "fillStyle": c.FillStyle = v; break;
                case "strokeStyle": c.StrokeStyle = v; break;
                case "lineWidth": c.LineWidth = N(v); break;
                case "lineCap": c.LineCap = (string)v; break;
                case "lineJoin": c.LineJoin = (string)v; break;
                case "miterLimit": c.MiterLimit = N(v); break;
                case "lineDashOffset": c.LineDashOffset = N(v); break;
                case "shadowBlur": c.ShadowBlur = N(v); break;
                case "shadowColor": c.ShadowColor = (string)v; break;
                case "shadowOffsetX": c.ShadowOffsetX = N(v); break;
                case "shadowOffsetY": c.ShadowOffsetY = N(v); break;
                case "globalAlpha": c.GlobalAlpha = N(v); break;
                case "font": c.Font = (string)v; break;
                case "textAlign": c.TextAlign = (string)v; break;
                case "textBaseline": c.TextBaseline = (string)v; break;
                case "imageSmoothingEnabled": c.ImageSmoothingEnabled = v is bool b && b; break;
                case "globalCompositeOperation":
                    if ((string)v != "source-over") throw new InvalidOperationException("unsupported globalCompositeOperation " + v);
                    break;
            }
        }
    }
}
