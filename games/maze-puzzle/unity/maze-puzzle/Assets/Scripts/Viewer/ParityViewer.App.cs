using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OrbitalSalvage.App;
using OrbitalSalvage.AudioDsp;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Loc;
using OrbitalSalvage.Logic;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    using Loc = OrbitalSalvage.Loc.Loc;

    public sealed partial class ParityViewer
    {
        ParityPages.PrewarmEntry[] tabPlan = Array.Empty<ParityPages.PrewarmEntry>();
        int tabPlanCursor;
        double tabPlanScale;
        bool tabPrewarmArmed;
        int locFrameVersion;
        bool locFrameReady;
        bool langBakeActive;
        int langBakeCursor;
        bool residentBudgetReady;
        AppController app;
        IAppClock appClock;
        bool followClock = true;
        bool debugNav;
        bool armedControl;
        ParityPages.ControlHit armedHit;
        int appliedEpoch = -1;
        string snapTemplate = "";
        string snapPage = "";
        string snapCredit = "";
        string snapQuota = "";
        string snapBar = "";
        string snapTip = "";
        int snapElemN = -1;
        string[] snapElemKey = Array.Empty<string>();
        string[] snapElemText = Array.Empty<string>();
        double?[] snapElemWidth = Array.Empty<double?>();
        double?[] snapElemPct = Array.Empty<double?>();
        bool?[] snapElemVis = Array.Empty<bool?>();
        bool snapWardrobeOpen;
        string snapWardrobeTab = "";
        string snapEquippedRing = "";
        string snapEquippedSing = "";
        string snapEquippedSky = "";
        bool sessionWired;
        bool flowClockActive;
        double flowClockMs;
        HomeChrome underlayChrome;
        string underlayKey;
        bool flowCapture;
        readonly List<string> playMismatches = new List<string>();
        int toastSlots;
        bool toastClearFrame;
        internal int ProductDraws;

        /// <summary>Set while a product boot is alive so Tools → Reset Saved Profile can reset that running app.</summary>
        public static Action RequestReset;

        void BootApp()
        {
            if (!selfTesting) ConfigurePresentation();
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
            IProfileStore store = benchSession
                ? (IProfileStore)new MemoryProfileStore()
                : new FileProfileStore(Application.persistentDataPath);
            app = new AppController(store, clock, new PlayerAudio(gameObject));
            // Kids Category parental gate is not applied. See LegalLinks.
            app.OpenUrl = Application.OpenURL;
            // Earliest point after the profile load. With only the English table present this
            // does not persist a language and does not change any painted string.
            Loc.SetLoader(new ResourcesLocTableLoader());
            app.InitializeLanguage(new UnityDeviceLocaleProvider());
            // os.parity.lang, including qps, overrides the profile for this process only.
            LocBootPref.Apply();
            if (session != null) session.Renderer.Dispose();
            session = LiveGameSession.CreateBound(app.Economy, ParityPages.CssWidth, ParityPages.CssHeight);
            session.NowMs = () => app.NowMs;
            app.NotifyAudioInit = () =>
            {
                if (session != null && session.Cues != null) session.Cues.PublicInit();
            };
            app.NotifyAudioMute = () =>
            {
                if (session != null && session.Cues != null) session.Cues.IsMuted = app.Audio.IsMuted;
            };
            app.AdoptSector(session.ActiveSector);
            appliedEpoch = app.SectorEpoch;
            app.ResumeCurrentSector();
            RequestReset = ResetRunningProgress;
            sessionWired = false;
            WireSession();
            app.Playfield = new BoundPlayfield(session);
            app.AfterPlayFrame = () =>
            {
                if (session != null) app.SetLiveMass(session.CollectedMass, session.SpilledMass);
            };
            app.BeginBoot();
            if (realtime) WireLiveAudio();
            SyncApp(true);
            if (realtime) BeginTabPrewarm();
        }

        /// <summary>
        /// Plan the five main-tab chromes, home first. One is baked per frame after the loading bar's
        /// override set exists, and only while the next page fits in the resident byte budget.
        /// A page that does not fit is baked on first visit. Baking never blocks input.
        /// </summary>
        void BeginTabPrewarm()
        {
            if (!ParityPages.ShouldPrewarmTabs(selfTesting) || app == null || tabPrewarmArmed) return;
            tabPrewarmArmed = true;
            EnsureResidentBudget();
            string profile = ParityPages.ActiveProfile;
            RefreshTabPins(profile);
            tabPlan = ParityPages.PrewarmTemplates(app, profile);
            tabPlanCursor = 0;
            tabPlanScale = 0;
        }

        void EnsureResidentBudget()
        {
            if (residentBudgetReady) return;
            residentBudgetReady = true;
            bool phone = QualityGovernor.IsPhone(Application.isMobilePlatform, SystemInfo.deviceType);
            int memoryMb = SystemInfo.systemMemorySize;
            cache.ByteBudget = ParityPages.ResidentBudgetBytes(memoryMb, phone);
            cache.Cost = chrome => chrome == null ? 0L : chrome.ResidentBytes;
            Debug.Log("[ParityViewer] resident budget " + cache.ByteBudget.ToString(CultureInfo.InvariantCulture)
                + " bytes, phone=" + phone.ToString()
                + ", systemMemoryMb=" + memoryMb.ToString(CultureInfo.InvariantCulture));
        }

        void TrimResident()
        {
            if (!residentBudgetReady) return;
            cache.ProtectKey = shownKey;
            evicted.Clear();
            cache.Trim(evicted);
            DisposeEvictedExcept(current);
        }

        void DisposeEvictedExcept(HomeChrome keep)
        {
            for (int i = 0; i < evicted.Count; i++)
            {
                var chrome = evicted[i];
                if (chrome == null || ReferenceEquals(chrome, keep)) continue;
                chrome.Dispose();
            }
            evicted.Clear();
        }

        /// <summary>One int compare per frame. A change drops resident bakes and spreads the rebuild.</summary>
        void PollLanguageFrame()
        {
            int version = Loc.Version;
            if (!locFrameReady)
            {
                locFrameReady = true;
                locFrameVersion = version;
                return;
            }
            if (version == locFrameVersion) return;
            locFrameVersion = version;
            OnLanguageChanged();
        }

        void OnLanguageChanged()
        {
            cache.ForEach((key, chrome) =>
            {
                if (chrome != null) chrome.InvalidateForLanguage();
            });
            if (current != null) current.InvalidateForLanguage();
            langBakeActive = true;
            langBakeCursor = 0;
        }

        /// <summary>One resident page per frame, same idea as tab prewarm, so a language switch does not rebake every tab at once.</summary>
        bool PumpLanguageBake()
        {
            if (!langBakeActive || HomeChrome.DeferLayerCache) return false;
            HomeChrome job = null;
            int index = 0;
            cache.ForEach((key, chrome) =>
            {
                if (job != null || chrome == null) return;
                if (index++ < langBakeCursor) return;
                if (chrome.LayerCacheReady) return;
                if (ReferenceEquals(chrome, current)) return;
                job = chrome;
            });
            if (job == null)
            {
                langBakeActive = false;
                return false;
            }
            langBakeCursor++;
            try
            {
                job.BakeStaticLayers();
            }
            catch (Exception ex)
            {
                Debug.LogError("[ParityViewer] language rebake: " + ex.Message);
            }
            return true;
        }

        void PumpTabPrewarm()
        {
            if (PumpLanguageBake()) return;
            if (RenderOpts.NoPrewarm && app != null && app.CurrentPage == "game") return;
            if (!ParityPages.ShouldPrewarmTabs(selfTesting) || app == null || tabPlan == null || tabPlan.Length == 0) return;
            bool hold = string.IsNullOrEmpty(app.LoadingBar) && app.CurrentPage == "loading";
            if (HomeChrome.DeferLayerCache || hold) return;
            EnsureResidentBudget();
            double scale = DesiredRasterScale();
            if (tabPlanScale <= 0 || ParityPages.RasterScaleIsStale(tabPlanScale, scale))
            {
                tabPlanCursor = 0;
                tabPlanScale = scale;
            }
            while (tabPlanCursor < tabPlan.Length && cache.Contains(tabPlan[tabPlanCursor].CacheKey))
                tabPlanCursor++;
            if (tabPlanCursor < tabPlan.Length)
            {
                var entry = tabPlan[tabPlanCursor];
                long estimate = ParityPages.EstimateResidentBytes(entry.TemplateId, scale);
                if (ParityPages.MayRebakeResident(ResidentCacheBytes, 0, estimate, cache.ByteBudget, false))
                {
                    BakeTabEntry(entry);
                    tabPlanCursor++;
                    return;
                }
            }
            RepairOneResident();
        }

        void BakeTabEntry(ParityPages.PrewarmEntry entry)
        {
            if (string.IsNullOrEmpty(entry.TemplateId) || cache.Contains(entry.CacheKey)) return;
            HomeChrome chrome = null;
            try
            {
                long loadStamp = PerfLoadStamp();
                chrome = HomeChrome.Load(entry.TemplateId, entry.Profile);
                PerfLoadAdd(loadStamp);
                if (!string.IsNullOrEmpty(entry.CardStamp)) StampShop(chrome, entry.CardStamp);
                chrome.InvalidateIfScaleDiffers(DesiredRasterScale());
                chrome.UseClaimedGiftBanner(entry.Page == "shop" && !app.Economy.CanClaimDailyGift());
                WriteDynamicOverrides(chrome, entry.Page, entry.TemplateId);
                chrome.BakeStaticLayers();
                RenderStats.PrewarmBake++;
                cache.ProtectKey = shownKey;
                evicted.Clear();
                cache.Add(entry.CacheKey, chrome, evicted);
                DisposeEvictedExcept(current);
                chrome = null;
            }
            catch (Exception ex)
            {
                Debug.LogError("[ParityViewer] prewarm " + entry.TemplateId + ": " + ex.Message);
                if (chrome != null) chrome.Dispose();
            }
        }

        /// <summary>
        /// After a raster change, one resident pinned page per frame is resized and re-baked.
        /// A scale-up that would pass the byte budget is left until that page is shown.
        /// A scale-down is baked because it frees memory. Deferred while the governor asks.
        /// </summary>
        void RepairOneResident()
        {
            if (HomeChrome.DeferLayerCache || !HomeChrome.UseLayerCache) return;
            double want = DesiredRasterScale();
            HomeChrome job = null;
            cache.ForEach((key, chrome) =>
            {
                if (job != null || chrome == null || !cache.IsPinned(key) || key == shownKey) return;
                bool stale = ParityPages.RasterScaleIsStale(chrome.RasterScale, want);
                if (!stale && chrome.LayerCacheReady) return;
                long projected = ParityPages.EstimateResidentBytes(TemplateOf(key), want);
                long others = ResidentCacheBytes - chrome.ResidentBytes;
                if (!ParityPages.MayRebakeResident(others, chrome.ResidentBytes, projected, cache.ByteBudget, false))
                    return;
                job = chrome;
            });
            if (job == null) return;
            try
            {
                job.InvalidateIfScaleDiffers(want);
                job.BakeStaticLayers();
                RenderStats.PrewarmBake++;
                TrimResident();
            }
            catch (Exception ex)
            {
                Debug.LogError("[ParityViewer] rebake: " + ex.Message);
            }
        }

        static string TemplateOf(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            int at = key.IndexOf('@');
            int hash = key.IndexOf('#');
            int end = at >= 0 ? at : (hash >= 0 ? hash : key.Length);
            if (end < 0) end = 0;
            return key.Substring(0, end);
        }

        /// <summary>Profile volumes plus physics cues into the DSP player. Parity boots leave <c>Audible</c> null.</summary>
        void WireLiveAudio()
        {
            if (app == null || app.Economy == null || app.Economy.Profile == null || app.Economy.Profile.Settings == null) return;
            var settings = app.Economy.Profile.Settings;
            app.Audio.SetSfxVolume(settings.SfxVolume);
            app.Audio.SetAmbienceVolume(settings.AmbientVolume);
            if (!settings.SoundEnabled && !app.Audio.IsMuted)
            {
                app.Audio.ToggleMute();
                if (session != null && session.Cues != null) session.Cues.IsMuted = true;
            }
            var player = gameObject.GetComponent<WebAudioPlayer>();
            if (player == null || session == null || session.Cues == null) return;
            session.Cues.Audible = (name, amount, text) =>
            {
                if (name == "spin") player.PlaySpinRatchet(amount);
                else if (name == "collision") player.PlayDebrisCollision(amount, string.IsNullOrEmpty(text) ? "titanium" : text);
                else if (name == "swallow") player.PlayDebrisSwallowed();
                else if (name == "gate") player.PlayGatePass(string.IsNullOrEmpty(text) ? "MULTIPLY" : text, amount);
                else if (name == "rcs") player.PlayRcsThrust();
                else if (name == "bumper") player.PlayBumperBounce();
                else if (name == "hazard") player.PlayHazardExplosion();
            };
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
            session.OnCelebrationCue += cue =>
            {
                if (app == null || app.Audio == null) return;
                if (cue == "cannon") app.Audio.PlayCelebrationCannon();
                else if (cue == "launch") app.Audio.PlayFireworkLaunch();
                else if (cue == "burst") app.Audio.PlayFireworkBurst();
            };
            app.BackupDeployed += () =>
            {
                if (session != null) session.Physics.SpawnBackupScrap(8);
            };
        }

        public static void StampShop(HomeChrome chrome, string kinds)
        {
            var heights = HomeChrome.ShopCardHeights(kinds, chrome.Profile);
            if (heights == null) return;
            double originY = CardInstancer.ShopY;
            if (chrome.TryGridOrigin("store-card", out _, out double measuredY)) originY = measuredY;
            var slots = CardInstancer.Stack(CardInstancer.ShopX, originY, CardInstancer.ShopW, CardInstancer.ShopGap, heights);
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
            if (session != null && app.ActiveSector != null && SectorNeedsLoad(session))
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
            RefreshProductToasts();
        }

        void SyncApp()
        {
            SyncApp(false);
        }

        bool ModalOpen(string id)
        {
            if (app == null || string.IsNullOrEmpty(id)) return false;
            var open = app.OpenModals();
            for (int i = 0; i < open.Count; i++)
                if (open[i] == id) return true;
            return false;
        }

        /// <summary>Copy the controller's live text, shop footers, and maps stamp onto a chrome that may not be on screen.</summary>
        void WriteDynamicOverrides(HomeChrome chrome, string page, string templateId)
        {
            chrome.TextOverrides.Clear();
            chrome.TextLangOverrides.Clear();
            chrome.WidthOverrides.Clear();
            chrome.WidthPercentOverrides.Clear();
            chrome.OpacityOverrides.Clear();
            chrome.HiddenIds.Clear();
            chrome.ClassTextOverrides.Clear();
            foreach (var kv in app.Model.Elements)
            {
                var o = kv.Value;
                if (o.HasText && o.Text != null) chrome.TextOverrides[kv.Key] = o.Text;
                if (o.WidthPx.HasValue) chrome.WidthOverrides[kv.Key] = o.WidthPx.Value;
                if (o.WidthPercent.HasValue) chrome.WidthPercentOverrides[kv.Key] = o.WidthPercent.Value;
                if (o.Opacity.HasValue) chrome.OpacityOverrides[kv.Key] = o.Opacity.Value;
                if (o.Visible == false) chrome.HiddenIds.Add(kv.Key);
            }
            int buy = 0, wear = 0, equipped = 0;
            for (int i = 0; i < app.ShopFooters.Count; i++)
            {
                string footer = app.ShopFooters[i];
                if (footer == Loc.Tr("shop.footer.wear")) chrome.ClassTextOverrides["btn-equip-item#" + wear++] = footer;
                else if (footer == Loc.Tr("shop.footer.equipped")) chrome.ClassTextOverrides["badge-equipped#" + equipped++] = footer;
                else chrome.ClassTextOverrides["btn-buy-item#" + buy++] = footer;
            }
            chrome.Toasts.Clear();
            if (app.Model.Toasts.Count > 0)
            {
                for (int i = 0; i < app.Model.Toasts.Count; i++)
                {
                    var toast = app.Model.Toasts[i];
                    double age = app.NowMs - (toast.ExpiresAtMs - AppController.ToastMs);
                    if (age < 0) age = 0;
                    chrome.Toasts.Add(new HomeChrome.ToastPaint { Text = toast.Text, AgeMs = age });
                }
            }
            chrome.WardrobeOpen = (app.CurrentPage == "game" && (app.IsWardrobeOpen || app.ModalOpen("modal-wardrobe")));
            chrome.WardrobeTab = app.WardrobeTab;
            chrome.WardrobeEconomy = app.Economy;
            if (page == "settings"
                || (templateId != null && templateId.IndexOf("settings_modal_language", StringComparison.Ordinal) >= 0)
                || ModalOpen("modal-language"))
            {
                LocFonts.FillPickerOverrides(chrome.TextLangOverrides);
                string endonym;
                if (chrome.TextOverrides.TryGetValue("settings-language-value", out endonym))
                {
                    string code = LocFonts.CodeForEndonym(endonym);
                    if (!string.IsNullOrEmpty(code)) chrome.TextLangOverrides["settings-language-value"] = code;
                }
            }
            if (page == "maps")
            {
                int level = CampaignLevel(app);
                bool match = TemplateSelector.MapsCaptureMatches(templateId, app.WorldIndex, level);
                ICollection<string> completed = app.Economy != null && app.Economy.Profile != null
                    ? app.Economy.Profile.CompletedSectors
                    : null;
                chrome.StampMapsCurrent(app.WorldIndex, level, match, completed);
            }
        }

        /// <summary>Copy the controller's sector, layout, and win flags onto the session before replay.</summary>
        void SyncPlayfield()
        {
            if (app == null || session == null) return;
            if (app.ConsumeLayoutRequest())
                session.ApplyLayout(ParityPages.CssWidth, ParityPages.CssHeight);
            if (app.ActiveSector != null && SectorNeedsLoad(session))
            {
                session.LoadSector(app.ActiveSector);
                appliedEpoch = app.SectorEpoch;
            }
            session.CollectedMass = app.CollectedMass;
            session.SpilledMass = app.SpilledMass;
            session.IsLevelCleared = app.IsLevelCleared;
            session.IsLevelFailed = app.IsLevelFailed;
            session.IsPaused = app.IsPaused;
            session.UsedBackupPod = app.UsedBackupPod;
        }

        /// <summary>Reload when the controller advanced a sector, or the session is still showing a different id.</summary>
        bool SectorNeedsLoad(LiveGameSession live)
        {
            if (live == null || app == null || app.ActiveSector == null) return false;
            bool idDrift = live.ActiveSector == null || live.ActiveSector.Id != app.ActiveSector.Id;
            return app.SectorEpoch != appliedEpoch || idDrift;
        }

        static int CampaignLevel(AppController controller)
        {
            if (controller == null || controller.ActiveSector == null) return 0;
            if (controller.ActiveSector.IsDaily == true || !controller.ActiveSector.LevelNumber.HasValue) return 0;
            return (int)controller.ActiveSector.LevelNumber.Value;
        }

        /// <summary>Tools → Reset Saved Profile. Returns the running app to level 1 and rewrites its store.</summary>
        public void ResetRunningProgress()
        {
            if (app == null) return;
            app.ResetSavedProgress();
            appliedEpoch = -1;
            SyncApp(true);
        }

        bool OverridesUnchanged()
        {
            if (snapTemplate != app.TemplateId || snapPage != app.CurrentPage || snapCredit != app.CreditText
                || snapQuota != app.QuotaText || snapBar != app.LoadingBar || snapTip != app.LoadingTip)
                return false;
            bool wardrobeOpen = app.IsWardrobeOpen || app.ModalOpen("modal-wardrobe");
            if (snapWardrobeOpen != wardrobeOpen) return false;
            if (wardrobeOpen)
            {
                if (snapWardrobeTab != (app.WardrobeTab ?? "")) return false;
                string eqRing = app.Economy != null && app.Economy.Profile != null ? app.Economy.Profile.Equipped.Get("ring") : "";
                string eqSing = app.Economy != null && app.Economy.Profile != null ? app.Economy.Profile.Equipped.Get("singularity") : "";
                string eqSky = app.Economy != null && app.Economy.Profile != null ? app.Economy.Profile.Equipped.Get("skybox") : "";
                if (snapEquippedRing != eqRing || snapEquippedSing != eqSing || snapEquippedSky != eqSky)
                    return false;
            }
            var els = app.Model.Elements;
            if (els.Count != snapElemN) return false;
            int i = 0;
            foreach (var kv in els)
            {
                if (snapElemKey[i] != kv.Key || snapElemText[i] != kv.Value.Text) return false;
                if (snapElemWidth[i] != kv.Value.WidthPx || snapElemPct[i] != kv.Value.WidthPercent || snapElemVis[i] != kv.Value.Visible)
                    return false;
                i++;
            }
            return true;
        }

        void RememberOverrides()
        {
            snapTemplate = app.TemplateId;
            snapPage = app.CurrentPage;
            snapCredit = app.CreditText;
            snapQuota = app.QuotaText;
            snapBar = app.LoadingBar;
            snapTip = app.LoadingTip;
            snapWardrobeOpen = app.IsWardrobeOpen || app.ModalOpen("modal-wardrobe");
            snapWardrobeTab = app.WardrobeTab ?? "";
            snapEquippedRing = app.Economy != null && app.Economy.Profile != null ? app.Economy.Profile.Equipped.Get("ring") : "";
            snapEquippedSing = app.Economy != null && app.Economy.Profile != null ? app.Economy.Profile.Equipped.Get("singularity") : "";
            snapEquippedSky = app.Economy != null && app.Economy.Profile != null ? app.Economy.Profile.Equipped.Get("skybox") : "";
            var els = app.Model.Elements;
            snapElemN = els.Count;
            if (snapElemKey.Length != snapElemN)
            {
                snapElemKey = new string[snapElemN];
                snapElemText = new string[snapElemN];
                snapElemWidth = new double?[snapElemN];
                snapElemPct = new double?[snapElemN];
                snapElemVis = new bool?[snapElemN];
            }
            int i = 0;
            foreach (var kv in els)
            {
                snapElemKey[i] = kv.Key;
                snapElemText[i] = kv.Value.Text;
                snapElemWidth[i] = kv.Value.WidthPx;
                snapElemPct[i] = kv.Value.WidthPercent;
                snapElemVis[i] = kv.Value.Visible;
                i++;
            }
        }

        void ApplyOverrides(bool forceDraw)
        {
            if (current == null || app == null) return;
            current.UseClaimedGiftBanner(app.CurrentPage == "shop" && !app.Economy.CanClaimDailyGift());
            if (!forceDraw && OverridesUnchanged()) return;
            RememberOverrides();
            WriteDynamicOverrides(current, app.CurrentPage, app.TemplateId);
            BindModalPlayfield(current);
            current.Underlay = null;
            if (flowClockActive) BindFlowUnderlay(current);
            if (flowCapture && app != null) current.PoseGestureHint(app.HintAgeMs);
            if (flowClockActive)
            {
                bool modal = currentId != null && currentId.StartsWith("modal_", StringComparison.Ordinal);
                current.Draw(flowClockMs, pressIndex, pressCss, modal && flowClockMs < 400, modal);
            }
            else if (selfTesting || currentId != null && currentId.StartsWith("loading", StringComparison.Ordinal)) current.Draw();
            else PaintCurrent(true);
        }

        /// <summary>
        /// Product path only. Toast ages are not part of <see cref="OverridesUnchanged"/>, so they are
        /// copied every frame outside that guard. Flow capture and self-tests keep the snapshot
        /// <see cref="ApplyOverrides"/> already wrote. Slots are reused; <see cref="HomeChrome.ToastPaint"/> is a struct.
        /// </summary>
        void RefreshProductToasts()
        {
            if (current == null || app == null || flowClockActive || selfTesting) return;
            var src = app.Model.Toasts;
            int n = src.Count;
            if (n == 0 && toastSlots > 0) toastClearFrame = true;
            var list = current.Toasts;
            while (list.Count > n) list.RemoveAt(list.Count - 1);
            while (list.Count < n) list.Add(default);
            for (int i = 0; i < n; i++)
            {
                var toast = src[i];
                double age = app.NowMs - (toast.ExpiresAtMs - AppController.ToastMs);
                if (age < 0) age = 0;
                var slot = list[i];
                slot.Text = toast.Text;
                slot.AgeMs = age;
                list[i] = slot;
            }
            toastSlots = n;
        }

        public void BindToastProbe(HomeChrome chrome, string stateId, AppController controller)
        {
            current = chrome;
            currentId = stateId;
            app = controller;
            followClock = false;
            selfTesting = false;
            flowClockActive = false;
            shipTesting = false;
            toastSlots = 0;
            toastClearFrame = false;
            ProductDraws = 0;
        }

        public void SetPageAgeMs(double ms)
        {
            clock.NowSeconds = () => ms / 1000.0;
        }

        public int ProbeToastFrame()
        {
            int before = ProductDraws;
            RefreshProductToasts();
            Tick();
            return ProductDraws - before;
        }

        void BindFlowUnderlay(HomeChrome modalChrome)
        {
            if (app == null || modalChrome == null) return;
            if (app.OpenModals().Count == 0) return;
            string baked = modalChrome.BakedPageId();
            string want = "page-" + (app.CurrentPage ?? "");
            if (baked.Length == 0 || baked == want) return;
            string underId = app.TemplateWithoutModal(out string stamp);
            if (string.IsNullOrEmpty(underId)) return;
            string key = underId + "#" + (stamp ?? "");
            if (underlayChrome == null || underlayKey != key)
            {
                if (underlayChrome != null) underlayChrome.Dispose();
                underlayChrome = HomeChrome.Load(underId, ParityPages.ResolveProfile(underId, ParityPages.ActiveProfile));
                underlayKey = key;
                if (!string.IsNullOrEmpty(stamp)) StampShop(underlayChrome, stamp);
            }
            CopyOverrides(modalChrome, underlayChrome);
            underlayChrome.Toasts.Clear();
            underlayChrome.Draw(flowClockMs);
            modalChrome.Underlay = underlayChrome.Target;
        }

        static void CopyOverrides(HomeChrome from, HomeChrome to)
        {
            to.TextOverrides.Clear();
            foreach (var kv in from.TextOverrides) to.TextOverrides[kv.Key] = kv.Value;
            to.WidthOverrides.Clear();
            foreach (var kv in from.WidthOverrides) to.WidthOverrides[kv.Key] = kv.Value;
            to.WidthPercentOverrides.Clear();
            foreach (var kv in from.WidthPercentOverrides) to.WidthPercentOverrides[kv.Key] = kv.Value;
            to.OpacityOverrides.Clear();
            foreach (var kv in from.OpacityOverrides) to.OpacityOverrides[kv.Key] = kv.Value;
            to.HiddenIds.Clear();
            foreach (var id in from.HiddenIds) to.HiddenIds.Add(id);
            to.ClassTextOverrides.Clear();
            foreach (var kv in from.ClassTextOverrides) to.ClassTextOverrides[kv.Key] = kv.Value;
        }

        /// <summary>Same hit-test path as a pointer click, in CSS pixels of the current template.</summary>
        public void ClickCss(double cssX, double cssY)
        {
            if (currentId == null) return;
            string spec = Spec(currentId);
            var painted = PaintedElements();
            bool pressed = painted != null
                ? ParityPages.TryPressAt(painted, cssX, cssY, out int index, out string transform)
                : ParityPages.TryPressAt(spec, cssX, cssY, out index, out transform);
            if (pressed)
            {
                pressIndex = index;
                pressCss = transform;
            }
            armedControl = painted != null
                ? ParityPages.TryHitControl(painted, cssX, cssY, out armedHit)
                : ParityPages.TryHitControl(spec, cssX, cssY, out armedHit);
            armedEdge = null;
            Release();
            SyncApp(true);
        }

        IEnumerator FlowShots(string dir)
        {
            yield return null;
            flowCapture = true;
            playMismatches.Clear();
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
                            if (op == "bootHome")
                            {
                                int guard = 0;
                                while (app.CurrentPage != "home" && guard++ < 2000) app.Advance(16);
                                app.Advance(240);
                            }
                        }
                        else if (op == "advance") app.Advance(step.Get("ms").Number);
                        else if (op == "click")
                        {
                            JsonValue idNode = step.Get("id");
                            if (idNode != null && idNode.Kind == JsonKind.String) app.Click(idNode.String);
                            else app.ClickClass(step.Get("token").String, (int)step.Get("index").Number);
                        }
                        else if (op == "forceWin") app.ForceVictory(step.Get("collectedMass").Number);
                        else if (op == "forceLose") app.ForceDefeat(step.Get("collectedMass").Number, step.Get("spilledMass").Number);
                        else if (op == "settings")
                        {
                            app.SetSoundEnabled(step.Get("sound").Bool);
                            app.SetHaptics(step.Get("haptics").Bool);
                            app.SetSlider("settings-sfx-slider", (int)step.Get("sfx").Number);
                            app.SetSlider("settings-ambience-slider", (int)step.Get("ambience").Number);
                        }
                        SyncPlayfield();
                        JsonValue play = step.Get("play");
                        if (play != null && play.Kind == JsonKind.Object)
                        {
                            string actual = app.ReplayPlay(play);
                            JsonValue expect = play.Get("hash");
                            int ticks = play.Get("physicsTicks") != null && play.Get("physicsTicks").Kind == JsonKind.Number
                                ? (int)play.Get("physicsTicks").Number : -1;
                            string line = id + "_" + (i + 1).ToString("00")
                                + " ticks " + ticks
                                + " hash " + (actual ?? "")
                                + (expect != null && expect.Kind == JsonKind.String && actual != expect.String ? " EXPECT " + expect.String : " ok");
                            playMismatches.Add(line);
                            if (expect != null && expect.Kind == JsonKind.String && actual != expect.String && error == null)
                                error = "play hash " + line;
                        }
                        JsonValue settle = step.Get("settleMs");
                        if (settle != null && settle.Kind == JsonKind.Number && settle.Number > 0)
                            app.Advance(settle.Number);
                        JsonValue snap = step.Get("snap");
                        // Home and loading sample each animation at its own clock. The live game page does not:
                        // PoseGestureHint owns #gesture-hint, and sampling handSwipe here overwrites that pose.
                        bool liveGame = app.CurrentPage == "game";
                        JsonValue anim = snap != null ? snap.Get("animMs") : null;
                        flowClockActive = !liveGame && anim != null && anim.Kind == JsonKind.Number;
                        if (flowClockActive) flowClockMs = anim.Number;
                        CssAnimation.Running = null;
                        JsonValue anims = !liveGame && snap != null ? snap.Get("anims") : null;
                        if (anims != null && anims.Kind == JsonKind.Object && anims.Count > 0)
                        {
                            var map = new Dictionary<string, double>(anims.Count);
                            for (int a = 0; a < anims.Members.Count; a++)
                            {
                                var mem = anims.Members[a];
                                if (mem.Value != null && mem.Value.Kind == JsonKind.Number)
                                    map[mem.Key] = mem.Value.Number;
                            }
                            if (map.Count > 0) CssAnimation.Running = map;
                        }
                        SyncApp(true);
                        string name = id + "_" + (i + 1).ToString("00") + ".png";
                        File.WriteAllBytes(Path.Combine(dir, name), FramePng());
                        written++;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                Debug.LogError("[ParityViewer] flow shots " + ex);
            }
            var hashLog = new StringBuilder();
            hashLog.Append("{\"ok\":").Append(error == null ? "true" : "false");
            hashLog.Append(",\"steps\":").Append(written);
            hashLog.Append(",\"error\":\"").Append((error ?? "").Replace("\\", "\\\\").Replace("\"", "'")).Append('"');
            hashLog.Append(",\"play\":[");
            for (int h = 0; h < playMismatches.Count; h++)
            {
                if (h > 0) hashLog.Append(',');
                hashLog.Append('"').Append(playMismatches[h].Replace("\\", "\\\\").Replace("\"", "'")).Append('"');
            }
            hashLog.Append("]}");
            File.WriteAllText(Path.Combine(dir, "flows.json"), hashLog.ToString());
            if (!Application.isEditor) Application.Quit(error == null ? 0 : 1);
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
                var painted = PaintedElements();
                double x, y;
                bool found;
                if (painted != null)
                    found = id != null
                        ? ParityPages.TryCenter(painted, id, out x, out y)
                        : ParityPages.TryClassCenter(painted, token, index, out x, out y);
                else
                    found = id != null
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
                if (!flowCapture)
                {
                    bool modal = currentId != null && currentId.StartsWith("modal_", StringComparison.Ordinal);
                    session.IsPaused = (app != null && app.IsPaused) || modal;
                    if (session.RenderFrames == 0) session.Update(RecordedFrameDriver.FrameDt);
                }
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

        string shipError;

        /// <summary>
        /// Default boot (real clock, profile, audio) with scripted pointers through <see cref="InjectPointerDown"/>.
        /// Home, 500 ms later, Play, pause, resume, back home.
        /// </summary>
        IEnumerator ShipSelfTest(string dir)
        {
            Application.runInBackground = true;
            shipError = null;
            bool animated = false;
            string profilePath = Path.Combine(Application.persistentDataPath, FileProfileStore.FileName);
            try
            {
                Directory.CreateDirectory(dir);
                BootApp();
            }
            catch (Exception ex) { shipError = ex.ToString(); }

            float deadline = Time.realtimeSinceStartup + 25f;
            while (shipError == null && (app == null || app.CurrentPage != "home"))
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    shipError = "home timeout page=" + (app == null ? "null" : app.CurrentPage);
                    break;
                }
                yield return null;
            }

            if (shipError == null) yield return StartCoroutine(ShipShot(dir, "01_home_a"));
            byte[] first = shipError == null ? File.ReadAllBytes(Path.Combine(dir, "01_home_a.png")) : null;
            if (shipError == null) yield return new WaitForSecondsRealtime(0.5f);
            if (shipError == null) yield return StartCoroutine(ShipShot(dir, "01_home_b"));
            if (shipError == null)
            {
                byte[] second = File.ReadAllBytes(Path.Combine(dir, "01_home_b.png"));
                animated = first != null && second != null && !BytesEqual(first, second);
                if (!animated) shipError = "home frames 500ms apart are identical";
            }

            if (shipError == null) yield return StartCoroutine(TapControl("btn-home-play"));
            if (shipError == null) yield return StartCoroutine(WaitShip("game", false));
            if (shipError == null) yield return StartCoroutine(ShipShot(dir, "02_game"));
            if (shipError == null) yield return StartCoroutine(TapControl("btn-game-pause"));
            if (shipError == null) yield return StartCoroutine(WaitShip("game", true));
            if (shipError == null) yield return StartCoroutine(ShipShot(dir, "03_pause"));
            if (shipError == null) yield return StartCoroutine(TapControl("btn-pause-resume"));
            if (shipError == null) yield return StartCoroutine(WaitShip("game", false));
            if (shipError == null) yield return StartCoroutine(ShipShot(dir, "04_resume"));
            if (shipError == null) yield return StartCoroutine(TapControl("btn-game-pause"));
            if (shipError == null) yield return StartCoroutine(WaitShip("game", true));
            if (shipError == null) yield return StartCoroutine(TapControl("btn-pause-home"));
            if (shipError == null) yield return StartCoroutine(WaitShip("home", false));
            if (shipError == null) yield return StartCoroutine(ShipShot(dir, "05_home"));

            try
            {
                if (app != null) app.Economy.SaveProfile();
                if (File.Exists(profilePath))
                    File.Copy(profilePath, Path.Combine(dir, "profile.json"), true);
                else if (shipError == null)
                    shipError = "profile file missing " + profilePath;
                var sb = new StringBuilder();
                sb.Append("{\"ok\":").Append(shipError == null ? "true" : "false");
                sb.Append(",\"animated\":").Append(animated ? "true" : "false");
                sb.Append(",\"page\":");
                Quote(sb, app != null ? app.CurrentPage : "");
                sb.Append(",\"profile\":");
                Quote(sb, profilePath);
                sb.Append(",\"error\":");
                Quote(sb, shipError);
                sb.Append('}');
                File.WriteAllText(Path.Combine(dir, "ship.json"), sb.ToString());
            }
            catch (Exception ex)
            {
                if (shipError == null) shipError = ex.ToString();
            }

            if (shipError != null) Debug.LogError("[ParityViewer] ship selftest failed: " + shipError);
            else Debug.Log("[ParityViewer] ship selftest ok " + dir);
            yield return null;
            if (!Application.isEditor) Application.Quit(shipError == null ? 0 : 1);
        }

        IEnumerator TapControl(string id)
        {
            if (currentId == null || app == null)
            {
                shipError = "no page for " + id;
                yield break;
            }
            string spec = Spec(currentId);
            if (!ParityPages.TryCenter(spec, id, out double cssX, out double cssY))
            {
                shipError = "no control " + id + " on " + currentId;
                yield break;
            }
            var screen = CssToScreen(PageRect(), cssX, cssY);
            InjectPointerDown(screen);
            yield return null;
            InjectPointerUp();
            yield return null;
            yield return null;
        }

        IEnumerator WaitShip(string page, bool pause)
        {
            float deadline = Time.realtimeSinceStartup + 8f;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (app != null && app.CurrentPage == page && app.ModalOpen("modal-pause") == pause)
                    yield break;
                yield return null;
            }
            shipError = "timeout want " + page + " pause=" + pause + " got " + (app == null ? "null" : app.CurrentPage);
        }

        IEnumerator ShipShot(string dir, string name)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                if (current == null || current.Target == null) throw new InvalidOperationException("no frame " + name);
                File.WriteAllBytes(Path.Combine(dir, name + ".png"), FramePng());
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + "_window.png"));
            }
            catch (Exception ex) { shipError = name + " " + ex.Message; }
            yield return new WaitForEndOfFrame();
            yield return null;
        }

        static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
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
            public void PlayCelebrationCannon() { player.PlayCelebrationCannon(); }
            public void PlayFireworkLaunch() { player.PlayFireworkLaunch(); }
            public void PlayFireworkBurst() { player.PlayFireworkBurst(); }
            public void PlayPop() { player.PlayPop(); }
            public void PlayScrapAbsorb() { player.PlayDebrisSwallowed(); }
        }

        sealed class BoundPlayfield : IFixedPlayfield, IFailureClock
        {
            readonly LiveGameSession _session;
            public BoundPlayfield(LiveGameSession session) { _session = session; }
            public void ResetFailureClock() { _session.FailureCheckTimer = 0; }
            public void Step(double dtSeconds, bool simulate)
            {
                _session.AdvanceReplayAudio(dtSeconds * 1000.0);
                _session.StepWebFrame(dtSeconds, simulate);
            }
            public void FinishReplay() { _session.FinishReplayAudio(); }
            public void PointerDown(double clientX, double canvasLeft, double canvasWidth) { _session.PointerDown(clientX, canvasLeft, canvasWidth); }
            public void PointerMove(double clientX) { _session.PointerMove(clientX); }
            public void PointerUp() { _session.PointerUp(); }
            public void KeyDown(string key) { _session.KeyDown(key); }
            public string StateHash() { return _session.StateHash(); }
        }
    }
}
