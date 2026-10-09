using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrbitalSalvage.Render;
using OrbitalSalvage.Viewer.Editor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Tests
{
    /// <summary>
    /// iPhone specs are captured at fixed insets (top 59, bottom 34 CSS px). Devices with other insets are off by a few px.
    /// Full-bleed containers are allowed in the inset bands: <c>mobile-frame</c>, each <c>app-page</c>, <c>game-canvas</c>
    /// (the playfield fills the frame; safe-area does not resize it), <c>page-header-bar</c> and <c>home-top-bar</c>
    /// (their padding box includes the top inset; the title and buttons sit below y=59), <c>home-arcade-dock</c>
    /// (its height includes the bottom inset; tab labels stay above y=898), <c>home-space-bg</c> and <c>bg-debris</c>
    /// (decorative), and modal sheet chrome. Inactive <c>modal-overlay</c> nodes (opacity 0, or missing <c>active</c>)
    /// and sheets parked at y &gt;= 932 are not on screen.
    /// Four bottom-sheet buttons extend a few px into the home-indicator band while their labels stay above y=898:
    /// <c>btn-pause-home</c>, <c>btn-dismiss-privacy</c>, <c>btn-cancel-reset</c>, <c>btn-confirm-reset</c>.
    /// </summary>
    public class IphoneSafeAreaTests
    {
        const double Top = 59;
        const double Bottom = 932 - 34;
        const double ViewH = 932;

        static readonly string[] BoxMayCrossHomeIndicator =
        {
            "btn-pause-home",
            "btn-dismiss-privacy",
            "btn-cancel-reset",
            "btn-confirm-reset"
        };

        [Test]
        public void EveryZeroSpec_HasAnIphoneSpec()
        {
            string dir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            var zeros = Directory.GetFiles(dir, "*__zero.json");
            Assert.GreaterOrEqual(zeros.Length, 44);
            foreach (string zero in zeros)
            {
                string id = Path.GetFileName(zero).Replace("__zero.json", "");
                string iphone = Path.Combine(dir, id + "__iphone.json");
                Assert.IsTrue(File.Exists(iphone), id);
            }
        }

        [Test]
        public void VisibleTextAndControls_StayOutOfTheNotchAndHomeIndicator()
        {
            string dir = Path.Combine(Application.dataPath, "Resources", "Parity", "Spec");
            var allow = new HashSet<string>(BoxMayCrossHomeIndicator);
            foreach (string path in Directory.GetFiles(dir, "*__iphone.json"))
            {
                string page = Path.GetFileName(path).Replace("__iphone.json", "");
                var root = (Dictionary<string, object>)MiniJson.Parse(File.ReadAllText(path));
                var raw = (List<object>)root["elements"];
                var els = new List<El>(raw.Count);
                var byI = new Dictionary<int, int>();
                for (int k = 0; k < raw.Count; k++)
                {
                    var e = (Dictionary<string, object>)raw[k];
                    var el = new El
                    {
                        Id = Str(e, "id"),
                        Classes = Str(e, "c"),
                        Parent = (int)Num(e, "p", -1),
                        Opacity = Num(e, "o", 1),
                        Visible = Num(e, "v", 1) != 0 && Str(e, "vis") != "hidden"
                    };
                    if (e.TryGetValue("r", out var rv) && rv is List<object> rect && rect.Count >= 4)
                    {
                        el.Y = (double)rect[1];
                        el.H = (double)rect[3];
                    }
                    if (e.TryGetValue("runs", out var runs) && runs is List<object> list)
                    {
                        foreach (object item in list)
                        {
                            if (!(item is Dictionary<string, object> run)) continue;
                            if (!(run.TryGetValue("r", out var rr) && rr is List<object> box && box.Count >= 4)) continue;
                            el.Runs.Add(new Run
                            {
                                Text = Str(run, "s"),
                                Y = (double)box[1],
                                H = (double)box[3]
                            });
                        }
                    }
                    els.Add(el);
                    byI[(int)Num(e, "i", k)] = k;
                }

                var hidden = new bool[els.Count];
                for (int k = 0; k < els.Count; k++)
                {
                    int p = els[k].Parent;
                    int pk = p >= 0 && byI.TryGetValue(p, out var found) ? found : -1;
                    bool parentHidden = pk >= 0 && hidden[pk];
                    bool inactiveModal = HasToken(els[k].Classes, "modal-overlay") && !HasToken(els[k].Classes, "active");
                    hidden[k] = parentHidden || !els[k].Visible || els[k].Opacity <= 0.01 || inactiveModal
                        || HasToken(els[k].Classes, "page-hidden");
                }

                for (int k = 0; k < els.Count; k++)
                {
                    if (hidden[k]) continue;
                    var el = els[k];
                    foreach (var run in el.Runs)
                    {
                        if (run.H <= 0 || run.Y + run.H <= 0 || run.Y >= ViewH) continue;
                        Assert.GreaterOrEqual(run.Y, Top - 0.25, page + " text in notch: " + Trim(run.Text));
                        Assert.LessOrEqual(run.Y + run.H, Bottom + 0.25, page + " text in home indicator: " + Trim(run.Text));
                    }
                    if (!Interactive(el) || el.H <= 0 || el.Y + el.H <= 0 || el.Y >= ViewH) continue;
                    Assert.GreaterOrEqual(el.Y, Top - 0.25, page + " control in notch: " + Label(el));
                    if (el.Y >= Bottom - 0.25)
                        Assert.Fail(page + " control sits in the home indicator: " + Label(el));
                    if (el.Y + el.H > Bottom + 0.25)
                        Assert.IsTrue(allow.Contains(el.Id), page + " " + Label(el) + " crosses y=" + Bottom);
                }
            }
        }

        [Test]
        public void IphonePaints_KeepTheTopBandFreeOfText()
        {
            string[] pages =
            {
                "home_default",
                "daily_default",
                "shop_rings_default",
                "maps_world_1_fresh",
                "game_lvl01_t1000ms",
                "modal_pause"
            };
            string project = Directory.GetParent(Application.dataPath).FullName;
            string dir = Path.GetFullPath(Path.Combine(project, "..", "..", "output", "preview"));
            Directory.CreateDirectory(dir);
            OutlineFonts.Clear();
            foreach (string id in pages)
            {
                Texture2D tex = null;
                try
                {
                    tex = PagePreviewPainter.Render(id, "iphone", true, 0, 0, out byte[] png);
                    Assert.IsNotNull(png, id);
                    File.WriteAllBytes(Path.Combine(dir, id + "__iphone.png"), png);
                    int light = LightPixelsInTopBand(tex, 59 * 3);
                    Assert.AreEqual(0, light, id + " top 59 css px has " + light + " near-white pixels");
                }
                finally
                {
                    if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
                }
            }
        }

        static int LightPixelsInTopBand(Texture2D tex, int bandRows)
        {
            var px = tex.GetPixels32();
            int w = tex.width;
            int h = tex.height;
            int rows = Math.Min(bandRows, h);
            int n = 0;
            for (int row = 0; row < rows; row++)
            {
                int y = h - 1 - row;
                int start = y * w;
                for (int x = 0; x < w; x++)
                {
                    var c = px[start + x];
                    if (c.r > 210 && c.g > 210 && c.b > 210) n++;
                }
            }
            return n;
        }

        static bool Interactive(El el)
        {
            if (!string.IsNullOrEmpty(el.Id) && el.Id.StartsWith("btn-", StringComparison.Ordinal)) return true;
            return HasToken(el.Classes, "tab-btn") || HasToken(el.Classes, "level-tile")
                || HasToken(el.Classes, "world-tab-btn") || HasToken(el.Classes, "coins-pill")
                || HasToken(el.Classes, "dock-tab-btn");
        }

        static bool HasToken(string classes, string token)
        {
            if (string.IsNullOrEmpty(classes) || string.IsNullOrEmpty(token)) return false;
            return (" " + classes + " ").Contains(" " + token + " ");
        }

        static string Label(El el) => string.IsNullOrEmpty(el.Id) ? el.Classes : el.Id;

        static string Trim(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Trim().Replace("\n", " ");
            return s.Length > 40 ? s.Substring(0, 40) : s;
        }

        static string Str(Dictionary<string, object> e, string key)
        {
            return e.TryGetValue(key, out var v) && v is string s ? s : "";
        }

        static double Num(Dictionary<string, object> e, string key, double fallback)
        {
            return e.TryGetValue(key, out var v) && v is double d ? d : fallback;
        }

        sealed class El
        {
            public string Id;
            public string Classes;
            public int Parent;
            public double Opacity;
            public bool Visible;
            public double Y, H;
            public readonly List<Run> Runs = new List<Run>();
        }

        struct Run
        {
            public string Text;
            public double Y, H;
        }
    }
}
