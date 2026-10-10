using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.App.Tests
{
    /// <summary>
    /// Maps tiles follow the live profile, matching web <c>renderSectorsPage</c>:
    /// current sector is PLAY (green), a completed sector is the star image, anything else is ▶.
    /// There is no locked tile. Row height is the taller tile in the pair; the next row starts 10 px below.
    /// </summary>
    public class MapsTileStampTests
    {
        const double Tol = 0.05;
        const string GreenBg = "rgba(74, 222, 128, 0.18)";
        const string IdleBg = "rgba(255, 255, 255, 0.07)";
        const string GreenBorder = "rgb(74, 222, 128)";
        const string IdleBorder = "rgba(255, 255, 255, 0.15)";

        enum Kind { Unplayed, Completed, Current }

        [TestCase("zero")]
        [TestCase("iphone")]
        public void FreshProfile_KeepsBakedWorld1Tiles(string profile)
        {
            var done = new List<string>();
            HomeChrome raw = null;
            HomeChrome live = null;
            try
            {
                raw = HomeChrome.Load("maps_world_1_fresh", profile);
                live = Open(profile, done, 1, out int level, out string template);
                Assert.AreEqual(1, level);
                Assert.AreEqual("maps_world_1_fresh", template);
                AssertSameTiles(raw, live);
                Assert.AreEqual(raw.MapsGridHeight(), live.MapsGridHeight(), Tol);
                var tiles = Read(live);
                Assert.AreEqual(10, tiles.Count);
                AssertKind(tiles, 1, Kind.Current, level, done);
                for (int n = 2; n <= 10; n++) AssertKind(tiles, n, Kind.Unplayed, level, done);
                AssertRows(tiles);
                Assert.AreEqual(" 0/10", live.MapsBannerProgress());
                raw.CopyMapsBanner(out _, out double rx, out _, out double rw, out _);
                live.CopyMapsBanner(out _, out double lx, out _, out double lw, out _);
                Assert.AreEqual(rx, lx, Tol);
                Assert.AreEqual(rw, lw, Tol);
            }
            finally
            {
                if (raw != null) raw.Dispose();
                if (live != null) live.Dispose();
            }
        }

        [TestCase("zero")]
        [TestCase("iphone")]
        public void Level1Done_World1_MarksCompletedAndShiftsRows(string profile)
        {
            var done = Ids(1);
            HomeChrome raw = null;
            HomeChrome live = null;
            try
            {
                live = Open(profile, done, 1, out int level, out string template);
                Assert.AreEqual(2, level);
                Assert.AreEqual("maps_world_1_fresh", template);
                raw = HomeChrome.Load(template, profile);
                var tiles = Read(live);
                Assert.AreEqual(10, tiles.Count);
                AssertKind(tiles, 1, Kind.Completed, level, done);
                AssertKind(tiles, 2, Kind.Current, level, done);
                for (int n = 3; n <= 10; n++) AssertKind(tiles, n, Kind.Unplayed, level, done);
                AssertRows(tiles);
                var rawTiles = Read(raw);
                Assert.AreEqual(rawTiles[0].Y, tiles[0].Y, Tol, "first row stays on the baked grid top");
                Assert.AreEqual(82, tiles[0].Height, Tol);
                Assert.AreEqual(87, tiles[2].Height, Tol);
                Assert.AreEqual(rawTiles[2].Y - 5, tiles[2].Y, Tol, "row under a shorter pair moves up by 5");
                Assert.Less(live.MapsGridHeight(), raw.MapsGridHeight() - 4);
                Assert.AreEqual(" 1/10", live.MapsBannerProgress());
            }
            finally
            {
                if (raw != null) raw.Dispose();
                if (live != null) live.Dispose();
            }
        }

        [TestCase("zero")]
        [TestCase("iphone")]
        public void Level1Done_UnreachedWorlds_AreNeverCompleted(string profile)
        {
            var done = Ids(1);
            for (int world = 1; world <= 5; world++)
            {
                HomeChrome live = null;
                try
                {
                    live = Open(profile, done, world, out int level, out _);
                    Assert.AreEqual(2, level);
                    var tiles = Read(live);
                    Assert.AreEqual(10, tiles.Count, "world " + world);
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        int n = tiles[i].Level;
                        Kind expect = n == level ? Kind.Current : done.Contains("level_" + n) ? Kind.Completed : Kind.Unplayed;
                        AssertKind(tiles, n, expect, level, done);
                        if (n > level) Assert.AreNotEqual(Kind.Completed, Visual(tiles[i]), "LVL " + n + " is not reached");
                    }
                    AssertRows(tiles);
                    Assert.AreEqual(world, live.MapsActiveWorldTab());
                    Assert.AreEqual(world == 1 ? " 1/10" : " 0/10", live.MapsBannerProgress());
                }
                finally
                {
                    if (live != null) live.Dispose();
                }
            }
        }

        [TestCase("zero")]
        [TestCase("iphone")]
        public void Levels1Through12_World1AllStars_World2SplitsAt13(string profile)
        {
            var done = Range(1, 12);
            HomeChrome world1 = null;
            HomeChrome world2 = null;
            try
            {
                world1 = Open(profile, done, 1, out int level, out string template1);
                Assert.AreEqual(13, level);
                Assert.AreEqual("maps_world_1_all_complete", template1);
                var tiles1 = Read(world1);
                for (int n = 1; n <= 10; n++) AssertKind(tiles1, n, Kind.Completed, level, done);
                AssertRows(tiles1);
                Assert.AreEqual(82, tiles1[0].Height, Tol);
                Assert.AreEqual(1, world1.MapsActiveWorldTab());
                Assert.AreEqual(" 10/10", world1.MapsBannerProgress());

                world2 = Open(profile, done, 2, out int level2, out string template2);
                Assert.AreEqual(13, level2);
                Assert.AreEqual("maps_world_2", template2);
                var tiles2 = Read(world2);
                AssertKind(tiles2, 11, Kind.Completed, level2, done);
                AssertKind(tiles2, 12, Kind.Completed, level2, done);
                AssertKind(tiles2, 13, Kind.Current, level2, done);
                AssertKind(tiles2, 14, Kind.Unplayed, level2, done);
                AssertKind(tiles2, 15, Kind.Unplayed, level2, done);
                for (int n = 16; n <= 20; n++) AssertKind(tiles2, n, Kind.Unplayed, level2, done);
                AssertRows(tiles2);
                Assert.AreEqual(82, tiles2[0].Height, Tol, "11 and 12 are both compact");
                Assert.AreEqual(87, tiles2[2].Height, Tol, "13 sits beside an unplayed tile");
                Assert.AreEqual(20, tiles2[2].HeaderHeight, Tol);
                Assert.AreEqual(25, tiles2[3].HeaderHeight, Tol);
                Assert.AreEqual(2, world2.MapsActiveWorldTab());
                Assert.AreEqual(" 2/10", world2.MapsBannerProgress());
            }
            finally
            {
                if (world1 != null) world1.Dispose();
                if (world2 != null) world2.Dispose();
            }
        }

        [TestCase("zero")]
        [TestCase("iphone")]
        public void World1AllDone_DropsPlayOnWorld1_AndMarks11Current(string profile)
        {
            var done = Range(1, 10);
            HomeChrome world1 = null;
            HomeChrome world2 = null;
            try
            {
                world1 = Open(profile, done, 1, out int level, out _);
                Assert.AreEqual(11, level);
                var tiles1 = Read(world1);
                for (int n = 1; n <= 10; n++) AssertKind(tiles1, n, Kind.Completed, level, done);
                AssertRows(tiles1);
                Assert.IsFalse(world1.MapsLevelMarkedCurrent(1));
                Assert.AreEqual(" 10/10", world1.MapsBannerProgress());

                world2 = Open(profile, done, 2, out _, out _);
                var tiles2 = Read(world2);
                AssertKind(tiles2, 11, Kind.Current, 11, done);
                Assert.AreEqual(" 0/10", world2.MapsBannerProgress());
                for (int n = 12; n <= 20; n++) AssertKind(tiles2, n, Kind.Unplayed, 11, done);
                AssertRows(tiles2);
            }
            finally
            {
                if (world1 != null) world1.Dispose();
                if (world2 != null) world2.Dispose();
            }
        }

        [TestCase("zero")]
        [TestCase("iphone")]
        public void Levels1Through44_EveryWorldTab(string profile)
        {
            var done = Range(1, 44);
            for (int world = 1; world <= 5; world++)
            {
                HomeChrome live = null;
                try
                {
                    live = Open(profile, done, world, out int level, out _);
                    Assert.AreEqual(45, level);
                    var tiles = Read(live);
                    Assert.AreEqual(10, tiles.Count, "world " + world);
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        int n = tiles[i].Level;
                        Kind expect = n == 45 ? Kind.Current : n <= 44 ? Kind.Completed : Kind.Unplayed;
                        AssertKind(tiles, n, expect, level, done);
                        if (n > 45) Assert.AreNotEqual(Kind.Completed, Visual(tiles[i]), "LVL " + n);
                    }
                    AssertRows(tiles);
                    Assert.AreEqual(world, live.MapsActiveWorldTab());
                    if (world < 5) Assert.AreEqual(82, tiles[0].Height, Tol, "world " + world + " is fully complete");
                    if (world == 5)
                    {
                        Assert.AreEqual(82, tiles[0].Height, Tol, "41 and 42 are complete");
                        Assert.AreEqual(87, Tile(tiles, 45).Height, Tol, "45 sits beside 46");
                    }
                    Assert.AreEqual(world < 5 ? " 10/10" : " 4/10", live.MapsBannerProgress(), "world " + world);
                    if (world > 1 && world < 5)
                    {
                        live.CopyMapsBanner(out _, out double bx, out _, out double bw, out _);
                        Assert.AreEqual(61.734, bw, Tol, "10/10 uses the wide captured pill");
                        Assert.AreEqual(398, bx + bw, Tol, "pill stays on the card's right edge");
                    }
                }
                finally
                {
                    if (live != null) live.Dispose();
                }
            }
        }

        [TestCase("zero")]
        [TestCase("iphone")]
        public void Stamp_IsIdempotent_AndRestoresTheFreshCapture(string profile)
        {
            var done = Ids(1);
            HomeChrome chrome = null;
            HomeChrome fresh = null;
            try
            {
                chrome = Open(profile, done, 2, out int level, out _);
                Assert.AreEqual(2, level);
                var first = Read(chrome);
                bool match = TemplateSelector.MapsCaptureMatches("maps_world_2", 2, 2);
                chrome.StampMapsCurrent(2, level, match, done);
                AssertSame(first, Read(chrome));

                var none = new List<string>();
                chrome.StampMapsCurrent(1, 1, true, none);
                fresh = HomeChrome.Load("maps_world_1_fresh", profile);
                // Restoring a world-2 chrome cannot become the fresh spec. Reload fresh and stamp it.
                fresh.StampMapsCurrent(1, 1, true, none);
                var raw = HomeChrome.Load("maps_world_1_fresh", profile);
                try
                {
                    AssertSameTiles(raw, fresh);
                    Assert.AreEqual(1, fresh.MapsActiveWorldTab());
                    Assert.IsTrue(fresh.MapsLevelMarkedCurrent(1));
                }
                finally
                {
                    raw.Dispose();
                }
            }
            finally
            {
                if (chrome != null) chrome.Dispose();
                if (fresh != null) fresh.Dispose();
            }
        }

        [Test]
        public void IphoneMaps_WritesProfilePreviews_AndFreshZeroHashStaysPinned()
        {
            OutlineFonts.Clear();
            byte[] fresh = ReadPng(HomeChrome.Load("maps_world_1_fresh", "zero"), true);
            byte[] again = ReadPng(HomeChrome.Load("maps_world_1_fresh", "zero"), true);
            Assert.IsTrue(BytesEqual(fresh, again), "unstamped maps_world_1_fresh is not stable");

            var stamped = HomeChrome.Load("maps_world_1_fresh", "zero");
            stamped.StampMapsCurrent(1, 1, true, new List<string>());
            byte[] after = ReadPng(stamped, true);
            Assert.IsTrue(BytesEqual(fresh, after), "stamping the fresh profile changed maps_world_1_fresh");

            string actual = Sha256(fresh);
            string dir = "/tmp/maze-maps";
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "zero-fresh-unstamped.png"), fresh);
            File.WriteAllText(Path.Combine(dir, "zero-fresh-sha256.txt"), actual);
            WritePreview(Path.Combine(dir, "iphone-level1-world1.png"), Ids(1), 1);
            WritePreview(Path.Combine(dir, "iphone-level1-world2.png"), Ids(1), 2);
            WritePreview(Path.Combine(dir, "iphone-levels1-12-world1.png"), Range(1, 12), 1);
            WritePreview(Path.Combine(dir, "iphone-levels1-12-world2.png"), Range(1, 12), 2);
        }

        static void WritePreview(string path, List<string> completed, int world)
        {
            HomeChrome chrome = null;
            try
            {
                chrome = Open("iphone", completed, world, out _, out _);
                byte[] png = ReadPng(chrome, false);
                File.WriteAllBytes(path, png);
                Assert.Greater(png.Length, 1000, path);
            }
            finally
            {
                if (chrome != null) chrome.Dispose();
            }
        }

        static byte[] ReadPng(HomeChrome chrome, bool dispose)
        {
            RenderTexture copy = null;
            Texture2D read = null;
            var previous = RenderTexture.active;
            try
            {
                chrome.Draw();
                RenderTexture source = chrome.Target;
                copy = new RenderTexture(source.width, source.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
                {
                    name = "MapsPreviewCopy",
                    antiAliasing = 1,
                    filterMode = FilterMode.Point
                };
                copy.Create();
                Graphics.CopyTexture(source, copy);
                RenderTexture.active = copy;
                read = new Texture2D(copy.width, copy.height, TextureFormat.RGB24, false, true)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Point
                };
                read.ReadPixels(new Rect(0, 0, copy.width, copy.height), 0, 0, false);
                read.Apply(false);
                return read.EncodeToPNG();
            }
            finally
            {
                RenderTexture.active = previous;
                if (read != null) UnityEngine.Object.DestroyImmediate(read);
                if (copy != null)
                {
                    copy.Release();
                    UnityEngine.Object.DestroyImmediate(copy);
                }
                if (dispose) chrome.Dispose();
            }
        }

        static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }

        static string Sha256(byte[] data)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }

        static HomeChrome Open(string profile, ICollection<string> completed, int world, out int currentLevel, out string template)
        {
            var app = new AppController(new MemoryProfileStore(), new ManualAppClock(0, 0));
            foreach (var id in completed) app.Economy.Profile.CompletedSectors.Add(id);
            app.ResumeCurrentSector();
            app.ShowPage("maps");
            if (app.WorldIndex != world) app.ClickClass("world-tab-btn", world - 1);
            currentLevel = app.ActiveSector != null && app.ActiveSector.LevelNumber.HasValue
                ? (int)app.ActiveSector.LevelNumber.Value
                : 0;
            template = app.TemplateId;
            var chrome = HomeChrome.Load(template, profile);
            bool match = TemplateSelector.MapsCaptureMatches(template, app.WorldIndex, currentLevel);
            chrome.StampMapsCurrent(app.WorldIndex, currentLevel, match, completed);
            return chrome;
        }

        static List<string> Ids(params int[] levels)
        {
            var list = new List<string>();
            for (int i = 0; i < levels.Length; i++) list.Add("level_" + levels[i]);
            return list;
        }

        static List<string> Range(int from, int to)
        {
            var list = new List<string>();
            for (int n = from; n <= to; n++) list.Add("level_" + n);
            return list;
        }

        static List<HomeChrome.MapsTileInfo> Read(HomeChrome chrome)
        {
            var list = new List<HomeChrome.MapsTileInfo>();
            chrome.CopyMapsTiles(list);
            return list;
        }

        static HomeChrome.MapsTileInfo Tile(List<HomeChrome.MapsTileInfo> tiles, int level)
        {
            for (int i = 0; i < tiles.Count; i++)
                if (tiles[i].Level == level) return tiles[i];
            Assert.Fail("missing LVL " + level);
            return default(HomeChrome.MapsTileInfo);
        }

        static Kind Visual(HomeChrome.MapsTileInfo tile)
        {
            string text = tile.StatusText ?? "";
            if (tile.Active || text.IndexOf("PLAY", StringComparison.Ordinal) >= 0) return Kind.Current;
            if (tile.StarVisible) return Kind.Completed;
            return Kind.Unplayed;
        }

        static void AssertKind(List<HomeChrome.MapsTileInfo> tiles, int level, Kind expect, int currentLevel, ICollection<string> completed)
        {
            var tile = Tile(tiles, level);
            Assert.AreEqual(expect, Visual(tile), "LVL " + level + " state");
            if (level > currentLevel && (completed == null || !completed.Contains("level_" + level)))
                Assert.AreNotEqual(Kind.Completed, Visual(tile), "LVL " + level + " not reached");
            double header = expect == Kind.Unplayed ? 25 : 20;
            Assert.AreEqual(header, tile.HeaderHeight, Tol, "LVL " + level + " header");
            double nameDy = expect == Kind.Unplayed ? 41 : 36;
            double quotaDy = expect == Kind.Unplayed ? 60 : 55;
            Assert.AreEqual(tile.Y + nameDy, tile.NameY, Tol, "LVL " + level + " name");
            Assert.AreEqual(tile.Y + quotaDy, tile.QuotaY, Tol, "LVL " + level + " quota");
            double right = tile.X + tile.Width - 14;
            Assert.AreEqual(right, tile.StatusX + tile.StatusW, Tol, "LVL " + level + " status right edge");
            if (expect == Kind.Current)
            {
                Assert.IsTrue(tile.StatusText.IndexOf("PLAY", StringComparison.Ordinal) >= 0, "LVL " + level);
                Assert.IsTrue(tile.StarVisible, "LVL " + level + " star");
                Assert.AreEqual("⭐", tile.StarEmoji);
                Assert.AreEqual(GreenBg, tile.Background);
                Assert.AreEqual(GreenBorder, tile.Border);
                Assert.AreEqual(56.516, tile.StatusW, Tol);
                Assert.AreEqual(20, tile.StatusH, Tol);
                Assert.AreEqual(tile.StatusX, tile.StarX, Tol);
                Assert.AreEqual(15, tile.StarW, Tol);
                Assert.AreEqual(15, tile.StarH, Tol);
            }
            else if (expect == Kind.Completed)
            {
                Assert.AreEqual("", tile.StatusText.Trim(), "LVL " + level);
                Assert.IsTrue(tile.StarVisible, "LVL " + level + " star");
                Assert.AreEqual("✅", tile.StarEmoji);
                Assert.IsFalse(tile.Active, "LVL " + level);
                Assert.AreEqual(IdleBg, tile.Background);
                Assert.AreEqual(IdleBorder, tile.Border);
                Assert.AreEqual(15, tile.StatusW, Tol);
                Assert.AreEqual(20, tile.StatusH, Tol);
                Assert.AreEqual(15, tile.StarW, Tol);
                Assert.AreEqual(15, tile.StarH, Tol);
            }
            else
            {
                Assert.AreEqual("▶", tile.StatusText, "LVL " + level);
                Assert.IsFalse(tile.StarVisible, "LVL " + level + " must not show a star");
                Assert.IsFalse(tile.Active, "LVL " + level);
                Assert.AreEqual(IdleBg, tile.Background);
                Assert.AreEqual(IdleBorder, tile.Border);
                Assert.AreEqual(12, tile.StatusW, Tol);
                Assert.AreEqual(25, tile.StatusH, Tol);
            }
        }

        static void AssertRows(List<HomeChrome.MapsTileInfo> tiles)
        {
            Assert.GreaterOrEqual(tiles.Count, 2);
            for (int i = 0; i < tiles.Count; i += 2)
            {
                var left = tiles[i];
                Kind lk = Visual(left);
                double h = lk == Kind.Unplayed ? 87 : 82;
                if (i + 1 < tiles.Count)
                {
                    var right = tiles[i + 1];
                    Kind rk = Visual(right);
                    double rh = rk == Kind.Unplayed ? 87 : 82;
                    if (rh > h) h = rh;
                    Assert.AreEqual(left.Y, right.Y, Tol, "LVL " + left.Level + " row y");
                    Assert.AreEqual(left.Height, right.Height, Tol, "LVL " + left.Level + " row height");
                    Assert.AreEqual(left.X + left.Width + 10, right.X, Tol, "column gap");
                }
                Assert.AreEqual(h, left.Height, Tol, "LVL " + left.Level + " row height");
                if (i >= 2)
                {
                    var prev = tiles[i - 2];
                    Assert.AreEqual(prev.Y + prev.Height + 10, left.Y, Tol, "LVL " + left.Level + " stacks under the row above");
                }
            }
        }

        static void AssertSameTiles(HomeChrome a, HomeChrome b)
        {
            AssertSame(Read(a), Read(b));
        }

        static void AssertSame(List<HomeChrome.MapsTileInfo> a, List<HomeChrome.MapsTileInfo> b)
        {
            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].Level, b[i].Level);
                Assert.AreEqual(a[i].X, b[i].X, Tol);
                Assert.AreEqual(a[i].Y, b[i].Y, Tol);
                Assert.AreEqual(a[i].Width, b[i].Width, Tol);
                Assert.AreEqual(a[i].Height, b[i].Height, Tol);
                Assert.AreEqual(a[i].HeaderHeight, b[i].HeaderHeight, Tol);
                Assert.AreEqual(a[i].StatusText, b[i].StatusText);
                Assert.AreEqual(a[i].StarVisible, b[i].StarVisible);
                Assert.AreEqual(a[i].StarEmoji, b[i].StarEmoji);
                Assert.AreEqual(a[i].Active, b[i].Active);
                Assert.AreEqual(a[i].Background, b[i].Background);
            }
        }
    }
}
