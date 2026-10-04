using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OrbitalSalvage.App;
using OrbitalSalvage.AudioDsp;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    public sealed partial class ParityViewer
    {
        AppController app;
        IAppClock appClock;
        bool followClock = true;
        bool debugNav;
        bool armedControl;
        ParityPages.ControlHit armedHit;
        int appliedEpoch = -1;
        string overrideSig = "";
        bool sessionWired;

        void BootApp()
        {
            var clock = new PlayerClock();
            BootApp(clock, false, true);
        }

        void BootApp(IAppClock clock, bool resetProfile, bool realtime)
        {
            followClock = realtime;
            appClock = clock;
            if (resetProfile)
            {
                string path = Path.Combine(Application.persistentDataPath, FileProfileStore.FileName);
                if (File.Exists(path)) File.Delete(path);
            }
            JsRandom.Global.Reset(JsRandom.DefaultSeed);
            var store = new FileProfileStore(Application.persistentDataPath);
            app = new AppController(store, clock, new PlayerAudio(gameObject));
            if (session != null) session.Renderer.Dispose();
            session = LiveGameSession.CreateBound(app.Economy, ParityPages.CssWidth, ParityPages.CssHeight);
            session.NowMs = () => app.NowMs;
            app.AdoptSector(session.ActiveSector);
            appliedEpoch = app.SectorEpoch;
            WireSession();
            app.BeginBoot();
            SyncApp(true);
        }

        void WireSession()
        {
            if (session == null || sessionWired) return;
            sessionWired = true;
            session.OnVictory += stats =>
            {
                if (app == null) return;
                app.PresentSessionVictory(stats.CollectedMass, stats.Payout, session.RecentUnlocks);
            };
            session.OnDefeat += stats =>
            {
                if (app == null) return;
                app.PresentSessionDefeat(stats.CollectedMass, stats.SpilledMass);
            };
            session.OnSwallow += _ =>
            {
                if (app == null) return;
                app.ShowToastsFor(session.RecentUnlocks);
                if (app.Audio != null) app.Audio.PlayDebrisSwallowed();
            };
            session.OnBoostCue += _ =>
            {
                if (app != null && app.Audio != null) app.Audio.PlayBoosterPass();
            };
            app.BackupDeployed += () =>
            {
                if (session != null) session.Physics.SpawnBackupScrap(8);
            };
        }

        static void StampShop(HomeChrome chrome, string kinds)
        {
            var heights = HomeChrome.ShopCardHeights(kinds);
            if (heights == null) return;
            var slots = CardInstancer.StackShop(heights);
            var xs = new double[slots.Length];
            var ys = new double[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                xs[i] = slots[i].X;
                ys[i] = slots[i].Y;
            }
            chrome.StampShopCards(kinds, xs, ys);
        }

        void SyncApp(bool forceDraw)
        {
            if (app == null) return;
            if (followClock && appClock is PlayerClock player)
            {
                player.Read();
                app.CatchUp(player.NowMs, player.EpochMs);
            }
            if (session != null && app.ConsumeLayoutRequest())
                session.ApplyLayout(ParityPages.CssWidth, ParityPages.CssHeight);
            if (session != null && app.SectorEpoch != appliedEpoch && app.ActiveSector != null)
            {
                session.LoadSector(app.ActiveSector);
                session.IsLevelCleared = app.IsLevelCleared;
                session.IsLevelFailed = app.IsLevelFailed;
                session.UsedBackupPod = app.UsedBackupPod;
                appliedEpoch = app.SectorEpoch;
            }
            if (session != null) session.IsPaused = app.IsPaused;
            string template = app.TemplateId;
            if (!string.IsNullOrEmpty(template)) Show(template, app.CardStamp);
            ApplyOverrides(forceDraw);
        }

        void SyncApp()
        {
            SyncApp(false);
        }

        void ApplyOverrides(bool forceDraw)
        {
            if (current == null || app == null) return;
            var sig = new StringBuilder();
            sig.Append(app.TemplateId).Append('|').Append(app.CurrentPage).Append('|').Append(app.CreditText)
                .Append('|').Append(app.QuotaText).Append('|').Append(app.LoadingBar).Append('|').Append(app.LoadingTip);
            foreach (var kv in app.Model.Elements)
            {
                sig.Append('\n').Append(kv.Key).Append('=').Append(kv.Value.Text).Append('#').Append(kv.Value.WidthPx)
                    .Append('%').Append(kv.Value.WidthPercent).Append('!').Append(kv.Value.Visible);
            }
            string text = sig.ToString();
            if (!forceDraw && text == overrideSig) return;
            overrideSig = text;
            current.TextOverrides.Clear();
            current.WidthOverrides.Clear();
            current.WidthPercentOverrides.Clear();
            current.OpacityOverrides.Clear();
            current.HiddenIds.Clear();
            current.ClassTextOverrides.Clear();
            foreach (var kv in app.Model.Elements)
            {
                var o = kv.Value;
                if (o.HasText && o.Text != null) current.TextOverrides[kv.Key] = o.Text;
                if (o.WidthPx.HasValue) current.WidthOverrides[kv.Key] = o.WidthPx.Value;
                if (o.WidthPercent.HasValue) current.WidthPercentOverrides[kv.Key] = o.WidthPercent.Value;
                if (o.Opacity.HasValue) current.OpacityOverrides[kv.Key] = o.Opacity.Value;
                if (o.Visible == false) current.HiddenIds.Add(kv.Key);
            }
            int buy = 0, wear = 0, equipped = 0;
            for (int i = 0; i < app.ShopFooters.Count; i++)
            {
                string footer = app.ShopFooters[i];
                if (footer == "Wear") current.ClassTextOverrides["btn-equip-item#" + wear++] = footer;
                else if (footer.StartsWith("EQUIPPED", StringComparison.Ordinal)) current.ClassTextOverrides["badge-equipped#" + equipped++] = footer;
                else current.ClassTextOverrides["btn-buy-item#" + buy++] = footer;
            }
            current.Toasts.Clear();
            if (app.Model.Toasts.Count > 0)
            {
                for (int i = 0; i < app.Model.Toasts.Count; i++)
                {
                    var toast = app.Model.Toasts[i];
                    double age = app.NowMs - (toast.ExpiresAtMs - AppController.ToastMs);
                    if (age < 0) age = 0;
                    current.Toasts.Add(new HomeChrome.ToastPaint { Text = toast.Text, AgeMs = age });
                }
            }
            BindModalPlayfield(current);
            if (selfTesting || currentId != null && currentId.StartsWith("loading", StringComparison.Ordinal)) current.Draw();
            else
            {
                double ms = clock.Milliseconds;
                bool modal = currentId != null && currentId.StartsWith("modal_", StringComparison.Ordinal);
                current.Draw(ms, pressIndex, pressCss, modal && ms < 400, modal);
            }
        }

        /// <summary>Same hit-test path as a pointer click, in CSS pixels of the current template.</summary>
        public void ClickCss(double cssX, double cssY)
        {
            if (currentId == null) return;
            string spec = Spec(currentId);
            if (ParityPages.TryPressAt(spec, cssX, cssY, out int index, out string transform))
            {
                pressIndex = index;
                pressCss = transform;
            }
            armedControl = ParityPages.TryHitControl(spec, cssX, cssY, out armedHit);
            armedEdge = null;
            Release();
            SyncApp(true);
        }

        double pageShownAt;
        double modalShownAt;
        string flowPage = "";
        string flowModal = "";
        byte[] blankFlowPng;

        IEnumerator FlowShots(string dir)
        {
            yield return null;
            Directory.CreateDirectory(dir);
            string golden = Path.GetFullPath(Path.Combine(dir, "..", "..", "..", "golden", "app"));
            int written = 0;
            string error = null;
            try
            {
                var files = Directory.GetFiles(golden, "*.json");
                Array.Sort(files, StringComparer.Ordinal);
                foreach (string file in files)
                {
                    JsonValue root = JsonParser.Parse(File.ReadAllText(file));
                    string id = root.Get("id").String;
                    JsonValue trace = root.Get("trace");
                    JsonValue first = trace[0].Get("snap");
                    double epoch0 = first.Get("epochMs").Number - first.Get("nowMs").Number;
                    for (int i = 0; i < trace.Count; i++)
                    {
                        JsonValue step = trace[i];
                        string op = step.Get("op").String;
                        if (op == "boot" || op == "bootHome")
                        {
                            BootApp(new ManualAppClock(0, epoch0), true, false);
                            ResetFlowClock();
                            if (op == "bootHome")
                            {
                                int guard = 0;
                                while (app.CurrentPage != "home" && guard++ < 2000)
                                {
                                    app.Advance(16);
                                    NoteFlowClock();
                                }
                                AdvanceFlow(240);
                            }
                        }
                        else if (op == "advance") AdvanceFlow(step.Get("ms").Number);
                        else if (op == "click")
                        {
                            JsonValue idNode = step.Get("id");
                            if (idNode != null && idNode.Kind == JsonKind.String) app.Click(idNode.String);
                            else app.ClickClass(step.Get("token").String, (int)step.Get("index").Number);
                            NoteFlowClock();
                        }
                        else if (op == "forceWin")
                        {
                            app.ForceVictory(step.Get("collectedMass").Number);
                            NoteFlowClock();
                        }
                        else if (op == "forceLose")
                        {
                            app.ForceDefeat(step.Get("collectedMass").Number, step.Get("spilledMass").Number);
                            NoteFlowClock();
                        }
                        else if (op == "settings")
                        {
                            app.SetSoundEnabled(step.Get("sound").Bool);
                            app.SetHaptics(step.Get("haptics").Bool);
                            app.SetSlider("settings-sfx-slider", (int)step.Get("sfx").Number);
                            app.SetSlider("settings-ambience-slider", (int)step.Get("ambience").Number);
                            NoteFlowClock();
                        }
                        SyncApp(true);
                        string name = id + "_" + (i + 1).ToString("00") + ".png";
                        File.WriteAllBytes(Path.Combine(dir, name), FlowFrame());
                        written++;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                Debug.LogError("[ParityViewer] flow shots " + ex);
            }
            File.WriteAllText(Path.Combine(dir, "flows.json"), "{\"ok\":" + (error == null ? "true" : "false") + ",\"steps\":" + written + ",\"error\":\"" + (error ?? "").Replace("\\", "\\\\").Replace("\"", "'") + "\"}");
            if (!Application.isEditor) Application.Quit(error == null ? 0 : 1);
        }

        void ResetFlowClock()
        {
            flowPage = app.CurrentPage ?? "";
            flowModal = FlowModalKey();
            pageShownAt = app.NowMs;
            modalShownAt = app.NowMs;
        }

        void NoteFlowClock()
        {
            string page = app.CurrentPage ?? "";
            string modal = FlowModalKey();
            if (page != flowPage)
            {
                flowPage = page;
                pageShownAt = app.NowMs;
            }
            if (modal != flowModal)
            {
                flowModal = modal;
                modalShownAt = app.NowMs;
            }
        }

        void AdvanceFlow(double ms)
        {
            double left = ms;
            while (left > 0.0001)
            {
                double step = left > 16 ? 16 : left;
                app.Advance(step);
                left -= step;
                NoteFlowClock();
            }
        }

        string FlowModalKey()
        {
            var open = app.OpenModals();
            if (open.Count == 0) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < open.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(open[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Web screenshots are taken on the harness clock. pageEnter (0.22s) is still at opacity 0
        /// until time advances, and a modal opened at the end of an advance is still at its
        /// transition start (overlay opacity 0, card translateY(100%)).
        /// </summary>
        byte[] FlowFrame()
        {
            double pageAge = app.NowMs - pageShownAt;
            if (pageAge < 0) pageAge = 0;
            if (pageAge < 1)
            {
                if (current != null) current.DrawBackdropToasts();
                return current != null && current.Target != null ? ReadPng(current.Target) : BlankFlowPng();
            }
            double modalAge = app.NowMs - modalShownAt;
            if (app.OpenModals().Count > 0 && modalAge < 24)
            {
                string stamp;
                string pageId = app.TemplateWithoutModal(out stamp);
                Show(pageId, stamp);
                ApplyOverrides(true);
            }
            byte[] png = FramePng();
            if (pageAge < 220) png = PageEnterPng(pageAge / 220.0);
            return png;
        }

        byte[] BlankFlowPng()
        {
            if (blankFlowPng != null) return blankFlowPng;
            const int w = 1290;
            const int h = 2796;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false, true) { hideFlags = HideFlags.HideAndDontSave };
            var px = new Color32[w * h];
            var bg = new Color32(30, 27, 75, 255);
            for (int i = 0; i < px.Length; i++) px[i] = bg;
            tex.SetPixels32(px);
            tex.Apply(false);
            blankFlowPng = tex.EncodeToPNG();
            Destroy(tex);
            return blankFlowPng;
        }

        byte[] PageEnterPng(double linearT)
        {
            if (readTex == null) return FramePng();
            float eased = EaseOut((float)linearT);
            float scale = Mathf.Lerp(0.96f, 1f, eased);
            float ty = Mathf.Lerp(8f, 0f, eased) * (readTex.height / 932f);
            int w = readTex.width;
            int h = readTex.height;
            Color32[] src = readTex.GetPixels32();
            var dst = new Color32[src.Length];
            var bg = new Color32(30, 27, 75, 255);
            float cx = (w - 1) * 0.5f;
            float cy = (h - 1) * 0.5f;
            float inv = scale > 0.0001f ? 1f / scale : 1f;
            for (int y = 0; y < h; y++)
            {
                int cssY = h - 1 - y;
                float ly = (cssY - cy) * inv - ty;
                int sy = (int)Math.Round(cy + ly);
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    float sx = cx + (x - cx) * inv;
                    int ix = (int)Math.Round(sx);
                    Color32 sample = bg;
                    if (ix >= 0 && ix < w && sy >= 0 && sy < h)
                    {
                        int texY = h - 1 - sy;
                        sample = src[texY * w + ix];
                    }
                    dst[row + x] = Blend(sample, bg, eased);
                }
            }
            readTex.SetPixels32(dst);
            readTex.Apply(false);
            return readTex.EncodeToPNG();
        }

        static Color32 Blend(Color32 sample, Color32 bg, float opacity)
        {
            float t = opacity;
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return new Color32(
                (byte)(sample.r * t + bg.r * (1f - t) + 0.5f),
                (byte)(sample.g * t + bg.g * (1f - t) + 0.5f),
                (byte)(sample.b * t + bg.b * (1f - t) + 0.5f),
                255);
        }

        /// <summary>CSS ease-out, cubic-bezier(0, 0, 0.58, 1).</summary>
        static float EaseOut(float t)
        {
            if (t <= 0) return 0;
            if (t >= 1) return 1;
            float lo = 0, hi = 1, u = t;
            for (int i = 0; i < 14; i++)
            {
                float x = Bezier(u, 0f, 0.58f);
                if (x < t) lo = u;
                else hi = u;
                u = (lo + hi) * 0.5f;
            }
            return Bezier(u, 0f, 1f);
        }

        static float Bezier(float u, float c1, float c2)
        {
            float o = 1f - u;
            return 3f * o * o * u * c1 + 3f * o * u * u * c2 + u * u * u;
        }

        IEnumerator AppSelfTest(string dir)
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            string error = null;
            var steps = new List<string>();
            yield return null;
            try
            {
                Directory.CreateDirectory(dir);
                var manual = new ManualAppClock(0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                BootApp(manual, true, false);
            }
            catch (Exception ex) { error = ex.ToString(); }

            if (error == null) yield return StartCoroutine(AppShot(dir, "01_loading", steps));
            int guard = 0;
            while (error == null && app != null && app.CurrentPage != "home" && guard++ < 40)
            {
                try { app.Advance(AppController.BootIntervalMs); SyncApp(true); }
                catch (Exception ex) { error = ex.ToString(); break; }
            }
            if (error == null) yield return StartCoroutine(AppShot(dir, "02_home", steps));
            if (error == null) error = AppClick("btn-open-shop-page", null, -1);
            if (error == null) yield return StartCoroutine(AppShot(dir, "03_shop", steps));
            string creditsBefore = "";
            if (error == null)
            {
                // The next ring costs 500 and a fresh profile has 250. The grant is only so the buy can succeed.
                try { app.Economy.AddCredits(250); app.SyncEconomyDisplay(); SyncApp(true); creditsBefore = app.CreditText; }
                catch (Exception ex) { error = ex.ToString(); }
            }
            if (error == null) error = AppClick(null, "btn-buy-item", 0);
            if (error == null) yield return StartCoroutine(AppShot(dir, "04_bought", steps));
            string creditsAfter = app != null ? app.CreditText : "";
            if (error == null && creditsBefore == creditsAfter)
                error = "credits did not change after buy (" + creditsBefore + ")";
            if (error == null) error = AppClick("btn-back-shop", null, -1);
            if (error == null) yield return StartCoroutine(AppShot(dir, "05_back", steps));
            if (error == null) error = AppClick("btn-open-maps-page", null, -1);
            if (error == null) yield return StartCoroutine(AppShot(dir, "06_maps", steps));
            if (error == null) error = AppClick(null, "level-tile", 0);
            if (error == null) yield return StartCoroutine(AppShot(dir, "07_level", steps));
            if (error == null) error = AppClick("btn-game-pause", null, -1);
            if (error == null) yield return StartCoroutine(AppShot(dir, "08_pause", steps));
            if (error == null) error = AppClick("btn-pause-resume", null, -1);
            if (error == null) yield return StartCoroutine(AppShot(dir, "09_resume", steps));
            if (error == null)
            {
                try
                {
                    double quota = app.ActiveSector != null ? app.ActiveSector.Quota : 30;
                    app.ForceVictory(quota + 2);
                    if (session != null) session.IsLevelCleared = true;
                    app.Advance(AppController.DebriefDelayMs);
                    SyncApp(true);
                }
                catch (Exception ex) { error = ex.ToString(); }
            }
            if (error == null) yield return StartCoroutine(AppShot(dir, "10_debrief", steps));
            if (error == null) error = AppClick("btn-debrief-next", null, -1);
            if (error == null) yield return StartCoroutine(AppShot(dir, "11_next", steps));
            if (error == null) yield return StartCoroutine(ModalStillProof(dir, steps));
            if (error == null) yield return StartCoroutine(ToastProof(dir));

            try
            {
                var sb = new StringBuilder();
                sb.Append("{\"ok\":").Append(error == null ? "true" : "false");
                sb.Append(",\"error\":");
                Quote(sb, error);
                sb.Append(",\"creditsBefore\":");
                Quote(sb, creditsBefore);
                sb.Append(",\"creditsAfter\":");
                Quote(sb, creditsAfter);
                sb.Append(",\"steps\":[");
                for (int i = 0; i < steps.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(steps[i]);
                }
                sb.Append("]}");
                File.WriteAllText(Path.Combine(dir, "selftest.json"), sb.ToString());
            }
            catch (Exception ex)
            {
                if (error == null) error = ex.ToString();
            }
            if (error != null) Debug.LogError("[ParityViewer] app selftest failed: " + error);
            else Debug.Log("[ParityViewer] app selftest ok credits " + creditsBefore + " -> " + creditsAfter);
            yield return null;
            if (!Application.isEditor) Application.Quit(error == null ? 0 : 1);
        }

        string AppClick(string id, string token, int index)
        {
            try
            {
                string spec = Spec(currentId);
                double x, y;
                bool found = id != null
                    ? ParityPages.TryCenter(spec, id, out x, out y)
                    : ParityPages.TryClassCenter(spec, token, index, out x, out y);
                if (!found) return "no control " + (id ?? token) + " on " + currentId;
                ClickCss(x, y);
                return null;
            }
            catch (Exception ex) { return ex.ToString(); }
        }

        IEnumerator AppShot(string dir, string name, List<string> steps)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                if (current == null || current.Target == null) throw new InvalidOperationException("no frame " + name);
                File.WriteAllBytes(Path.Combine(dir, name + ".png"), FramePng());
                WriteStep(steps, name);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ParityViewer] shot " + name + " " + ex.Message);
            }
        }

        /// <summary>Chrome with the live playfield under a SkipPlayfield hole. Other pages stay chrome-only.</summary>
        byte[] FramePng()
        {
            if (current.ExternalPlayfield == null && ShowPlayfield && session != null && session.Renderer != null && session.Renderer.Target != null)
            {
                bool modal = currentId != null && currentId.StartsWith("modal_", StringComparison.Ordinal);
                session.IsPaused = (app != null && app.IsPaused) || modal;
                if (session.RenderFrames == 0) session.Update(RecordedFrameDriver.FrameDt);
                return PlayfieldComposite.Png(session.Renderer.Target, current.Target, ReadPng);
            }
            return ReadPng(current.Target);
        }

        /// <summary>
        /// Pause, debrief, and defeat at the still pose: measured chrome with the canvas hole,
        /// and the Chromium playfield dump for that spec composited underneath.
        /// A second PNG uses a live session stepped on the screenshot timeline.
        /// </summary>
        IEnumerator ModalStillProof(string dir, List<string> steps)
        {
            yield return null;
            string[] ids = { "modal_pause", "modal_debrief_exact", "modal_defeat_12_of_20" };
            var notes = new StringBuilder();
            notes.Append('{');
            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i];
                HomeChrome chrome = null;
                Canvas2D field = null;
                LiveGameSession live = null;
                try
                {
                    chrome = HomeChrome.Load(id, ParityPages.Profile);
                    field = new Canvas2D(chrome.PixelWidth, chrome.PixelHeight);
                    if (!chrome.PaintPlayfield(field)) throw new InvalidOperationException("no playfield " + id);
                    // Blit the still's canvas before the overlay so backdrop-filter blurs the playfield, not the hole.
                    chrome.ExternalPlayfield = field.Target;
                    chrome.SkipPlayfield = false;
                    chrome.Draw();
                    File.WriteAllBytes(Path.Combine(dir, "still_" + id + ".png"), ReadPng(chrome.Target));
                    live = StillSession(id);
                    chrome.ExternalPlayfield = live.Renderer.Target;
                    chrome.Draw();
                    File.WriteAllBytes(Path.Combine(dir, "live_" + id + ".png"), ReadPng(chrome.Target));
                    chrome.ExternalPlayfield = null;
                    chrome.Draw();
                    File.WriteAllBytes(Path.Combine(dir, "full_" + id + ".png"), ReadPng(chrome.Target));
                    if (i > 0) notes.Append(',');
                    notes.Append('"').Append(id).Append("\":\"ok\"");
                    Debug.Log("[ParityViewer] modal proof " + id);
                    WriteStep(steps, "still_" + id);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[ParityViewer] modal proof " + id + " " + ex);
                    if (i > 0) notes.Append(',');
                    notes.Append('"').Append(id).Append("\":\"error\"");
                }
                finally
                {
                    if (live != null) live.Renderer.Dispose();
                    if (field != null) field.Dispose();
                    if (chrome != null) chrome.Dispose();
                }
                yield return null;
            }
            notes.Append('}');
            File.WriteAllText(Path.Combine(dir, "modal-proof.json"), notes.ToString());
        }

        IEnumerator ToastProof(string dir)
        {
            yield return null;
            HomeChrome chrome = null;
            try
            {
                chrome = HomeChrome.Load("home_default", ParityPages.Profile);
                WriteToast(dir, chrome, "toast_t0200", 200, "Item Equipped! ✨");
                WriteToast(dir, chrome, "toast_t1000", 1000, "Item Equipped! ✨");
                WriteToast(dir, chrome, "toast_t2300", 2300, "Item Equipped! ✨");
                WriteToast(dir, chrome, "toast_long", 1000, "🎉 All 3 Daily Shifts Cleared! Claim Master Bonus in Daily Tab! 🏆");
                chrome.Toasts.Clear();
                chrome.Toasts.Add(new HomeChrome.ToastPaint { Text = "Sound On 🔊", AgeMs = 1500 });
                chrome.Toasts.Add(new HomeChrome.ToastPaint { Text = "🏆 Trophy: First Nom Nom! (+100 🪙)", AgeMs = 900 });
                chrome.Draw();
                File.WriteAllBytes(Path.Combine(dir, "still_toast_stacked.png"), ReadPng(chrome.Target));
                chrome.Dispose();
                chrome = HomeChrome.Load("shop_rings_default", ParityPages.Profile);
                WriteToast(dir, chrome, "shop_buy_success_toast", 1000, "Item Unlocked & Equipped! 🎉");
                WriteToast(dir, chrome, "shop_equip_toast", 1000, "Item Equipped! ✨");
                WriteToast(dir, chrome, "shop_claim_gift_toast", 1000, "🎁 Claimed +100 🪙 Janitor Stipend!");
                WriteToast(dir, chrome, "shop_buy_insufficient_toast", 1000, "Need more gold coins! 🪙");
                chrome.Dispose();
                chrome = HomeChrome.Load("daily_master_ready", ParityPages.Profile);
                WriteToast(dir, chrome, "daily_master_claim_toast", 1000, "🏆 Claimed +300 🪙 Daily Master Bonus!");
                chrome.Dispose();
                chrome = HomeChrome.Load("settings_default", ParityPages.Profile);
                WriteToast(dir, chrome, "settings_reset_confirmed_toast", 1000, "Progress Reset to Day 1! 🧹");
                Debug.Log("[ParityViewer] toast proof");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ParityViewer] toast proof " + ex);
            }
            finally
            {
                if (chrome != null) chrome.Dispose();
            }
        }

        void WriteToast(string dir, HomeChrome chrome, string name, double age, string text)
        {
            chrome.Toasts.Clear();
            chrome.Toasts.Add(new HomeChrome.ToastPaint { Text = text, AgeMs = age });
            chrome.Draw();
            File.WriteAllBytes(Path.Combine(dir, "still_" + name + ".png"), ReadPng(chrome.Target));
        }

        static LiveGameSession StillSession(string id)
        {
            var session = RecordedFrameDriver.Create(1);
            int play = 400 / 16;
            for (int i = 0; i < play; i++) session.Update(RecordedFrameDriver.FrameDt);
            if (id == "modal_pause") session.IsPaused = true;
            else if (id == "modal_debrief_exact")
            {
                session.CollectedMass = 20;
                session.IsLevelCleared = true;
            }
            else
            {
                session.CollectedMass = 12;
                session.SpilledMass = 4;
                session.IsLevelFailed = true;
            }
            int settleMs = id == "modal_pause" ? 300 : id == "modal_debrief_exact" ? 900 : 700;
            int frames = settleMs / 16;
            for (int i = 0; i < frames; i++) session.Update(RecordedFrameDriver.FrameDt);
            int rem = settleMs - frames * 16;
            if (rem > 0) session.Update(rem / 1000.0);
            return session;
        }

        void WriteStep(List<string> steps, string name)
        {
            var sb = new StringBuilder();
            sb.Append("{\"name\":");
            Quote(sb, name);
            sb.Append(",\"page\":");
            Quote(sb, app.CurrentPage);
            sb.Append(",\"template\":");
            Quote(sb, app.TemplateId);
            sb.Append(",\"credits\":");
            Quote(sb, app.CreditText);
            sb.Append(",\"sector\":");
            Quote(sb, app.ActiveSector != null ? app.ActiveSector.Id : "");
            sb.Append(",\"paused\":").Append(app.IsPaused ? "true" : "false");
            sb.Append(",\"modals\":[");
            var modals = app.OpenModals();
            for (int i = 0; i < modals.Count; i++)
            {
                if (i > 0) sb.Append(',');
                Quote(sb, modals[i]);
            }
            sb.Append("]}");
            steps.Add(sb.ToString());
        }

        static void Quote(StringBuilder sb, string value)
        {
            sb.Append('"');
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (c == '\\' || c == '"') sb.Append('\\');
                    if (c == '\n') sb.Append("\\n");
                    else sb.Append(c);
                }
            }
            sb.Append('"');
        }

        sealed class PlayerClock : IAppClock
        {
            readonly double origin;
            readonly double epoch0;
            public double NowMs { get; set; }
            public double EpochMs { get; set; }

            public PlayerClock()
            {
                origin = Time.realtimeSinceStartupAsDouble;
                epoch0 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                NowMs = 0;
                EpochMs = epoch0;
            }

            public void Read()
            {
                NowMs = (Time.realtimeSinceStartupAsDouble - origin) * 1000.0;
                EpochMs = epoch0 + NowMs;
            }
        }

        sealed class PlayerAudio : IAppAudio
        {
            readonly WebAudioPlayer player;
            bool muted;

            public PlayerAudio(GameObject host)
            {
                player = host.GetComponent<WebAudioPlayer>();
                if (player == null) player = host.AddComponent<WebAudioPlayer>();
            }

            public bool IsMuted { get { return muted; } }
            public void PublicInit() { player.PublicInit(); }

            public bool ToggleMute()
            {
                muted = !muted;
                player.ToggleMute();
                return muted;
            }

            public void SetSfxVolume(double volume) { player.SetSfxVolume(volume); }
            public void SetAmbienceVolume(double volume) { player.SetAmbienceVolume(volume); }
            public void PlayBoosterPass() { player.PlayBoosterPass(); }
            public void PlayVictoryFanfare() { player.PlayVictoryFanfare(); }
            public void PlayDefeatSound() { player.PlayDefeatSound(); }
            public void PlayBackupPodSound() { player.PlayBackupPodSound(); }
            public void PlayDebrisSwallowed() { player.PlayDebrisSwallowed(); }
        }
    }
}
