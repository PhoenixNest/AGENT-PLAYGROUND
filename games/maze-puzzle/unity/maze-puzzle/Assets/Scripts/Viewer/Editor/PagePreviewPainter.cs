using System;
using System.IO;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Render;
using UnityEditor;
using UnityEngine;

namespace OrbitalSalvage.Viewer.Editor
{
    /// <summary>
    /// Paints a stored spec with <see cref="HomeChrome"/> and encodes it the same way the parity
    /// player does: <c>Draw</c> (capture pose) or <c>Draw(timeMs)</c>, copy into an ARGB32 target,
    /// then RGB24 <c>ReadPixels</c> with the linear flag set. No Play mode and no profile writes.
    /// </summary>
    public static class PagePreviewPainter
    {
        public const int LogicalWidth = 430;
        public const int LogicalHeight = 932;

        /// <param name="capturePose">True calls <see cref="HomeChrome.Draw()"/> (the still). False calls <see cref="HomeChrome.Draw(double)"/>.</param>
        /// <param name="rasterScale">Values below 1 keep the spec scale (3 for these pages). 1 is the fast preview.</param>
        public static Texture2D Render(string specId, string profile, bool capturePose, double animationTimeMs, double rasterScale, out byte[] png)
        {
            return Render(specId, profile, capturePose, animationTimeMs, rasterScale, null, out png);
        }

        /// <param name="prepare">Optional edits, such as text overrides, applied after load and before the draw.</param>
        public static Texture2D Render(string specId, string profile, bool capturePose, double animationTimeMs, double rasterScale, Action<HomeChrome> prepare, out byte[] png)
        {
            if (string.IsNullOrEmpty(profile)) profile = "zero";
            var chrome = HomeChrome.Load(specId, profile);
            RenderTexture copy = null;
            Texture2D read = null;
            var previous = RenderTexture.active;
            try
            {
            if (rasterScale >= 1) chrome.ResizeRaster(rasterScale);
            if (specId == "settings_modal_language") chrome.ApplyPickerFontOverrides();
            if (prepare != null) prepare(chrome);
                if (capturePose) chrome.Draw();
                else chrome.Draw(animationTimeMs);

                RenderTexture source = chrome.Target;
                copy = new RenderTexture(source.width, source.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
                {
                    name = "PagePreviewCopy",
                    antiAliasing = 1,
                    filterMode = FilterMode.Point
                };
                copy.Create();
                Graphics.CopyTexture(source, copy);
                RenderTexture.active = copy;
                read = new Texture2D(copy.width, copy.height, TextureFormat.RGB24, false, true)
                {
                    name = specId + "__" + profile,
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Point
                };
                read.ReadPixels(new Rect(0, 0, copy.width, copy.height), 0, 0, false);
                read.Apply(false);
                png = read.EncodeToPNG();
                var owned = read;
                read = null;
                return owned;
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
                chrome.Dispose();
            }
        }

        /// <summary>
        /// Writes the settings stills used to read the language picker back, including one frame
        /// whose marker has moved to Japanese. Batch: <c>OrbitalSalvage.Viewer.Editor.PagePreviewPainter.DumpSettingsLanguageBatch</c>.
        /// </summary>
        public static void DumpSettingsLanguageBatch()
        {
            string dir = "/tmp/maze-i18n/loc-settings/unity";
            Directory.CreateDirectory(dir);
            OutlineFonts.Clear();
            WriteShot(dir, "settings_default__zero.png", "settings_default", "zero", null);
            WriteShot(dir, "settings_default__iphone.png", "settings_default", "iphone", null);
            WriteShot(dir, "settings_modal_language__zero.png", "settings_modal_language", "zero", null);
            WriteShot(dir, "settings_modal_language__ja.png", "settings_modal_language", "zero", MarkJapanese);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        /// <summary>
        /// The 13 pictures in <c>games/maze-puzzle/unity/screenshots</c>, in the order doc 09 lists them:
        /// file name, then the spec painted in the capture pose at the spec's own scale (1290 x 2796).
        /// </summary>
        public static readonly string[][] DocScreenshots =
        {
            new[] { "01_unity_home_screen.png", "home_default" },
            new[] { "02_unity_maps_screen.png", "maps_world_1_fresh" },
            new[] { "03_unity_daily_operations.png", "daily_default" },
            new[] { "04_unity_toy_shop_donuts.png", "shop_rings_default" },
            new[] { "05_unity_toy_shop_hats.png", "shop_singularities_default" },
            new[] { "06_unity_toy_shop_skies.png", "shop_skyboxes_default" },
            new[] { "07_unity_trophy_room.png", "achievements_none" },
            new[] { "08_unity_settings_screen.png", "settings_default" },
            new[] { "09_unity_gameplay_sector_1.png", "game_lvl01_t1000ms" },
            new[] { "10_unity_gameplay_paused.png", "modal_pause" },
            new[] { "11_unity_victory_debrief.png", "modal_debrief_exact" },
            new[] { "12_unity_defeat_modal.png", "modal_defeat_12_of_20" },
            new[] { "13_unity_loading_screen.png", "loading_t0500" }
        };

        /// <summary>
        /// Repaints the 13 doc screenshots. Batch:
        /// <c>-executeMethod OrbitalSalvage.Viewer.Editor.PagePreviewPainter.DumpDocScreenshotsBatch -docShotDir &lt;folder&gt;</c>.
        /// Without <c>-docShotDir</c> the files go to <c>games/maze-puzzle/unity/screenshots</c>.
        /// Needs a graphics device, so do not pass <c>-nographics</c>.
        /// </summary>
        public static void DumpDocScreenshotsBatch()
        {
            string dir = null;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-docShotDir") dir = args[i + 1];
            if (string.IsNullOrEmpty(dir))
            {
                string project = Directory.GetParent(Application.dataPath).FullName;
                dir = Path.GetFullPath(Path.Combine(project, "..", "screenshots"));
            }
            Directory.CreateDirectory(dir);
            foreach (var item in DocScreenshots)
            {
                // The parity player starts with empty glyph outlines; a dirty cache moves a few edge pixels.
                OutlineFonts.Clear();
                WriteShot(dir, item[0], item[1], "zero", null);
            }
            Debug.Log("[DocShots] wrote " + DocScreenshots.Length + " files to " + dir);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        static void MarkJapanese(HomeChrome chrome)
        {
            chrome.TextOverrides["settings-language-value"] = "日本語";
            chrome.TextOverrides[LanguageMarker.ButtonId("en")] = "English";
            chrome.TextOverrides[LanguageMarker.ButtonId("ja")] = LanguageMarker.Mark("日本語");
        }

        static void WriteShot(string dir, string file, string specId, string profile, Action<HomeChrome> prepare)
        {
            Texture2D tex = null;
            try
            {
                tex = Render(specId, profile, true, 0, 0, prepare, out byte[] png);
                File.WriteAllBytes(Path.Combine(dir, file), png);
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }
    }
}
