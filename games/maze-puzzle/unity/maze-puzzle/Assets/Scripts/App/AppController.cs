using System;
using System.Collections.Generic;
using System.Globalization;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.App
{
    /// <summary>
    /// Port of <c>web/main.js</c> <c>OrbitalApp</c>: pages, modals, boot timers, navigation,
    /// economy actions, and the win/lose UI. Physics stays in <c>LiveGameSession</c>; this type
    /// only keeps the session fields the pages display and applies economy on the test hook
    /// <see cref="ForceVictory"/> / <see cref="ForceDefeat"/>. A live session that already paid
    /// out calls <see cref="PresentSessionVictory"/> instead.
    /// </summary>
    public sealed class AppController
    {
        public const double BootIntervalMs = 240;
        public const double BootDoneDelayMs = 350;
        public const double ToastMs = 2500;
        public const double DebriefDelayMs = 600;
        public const double DefeatDelayMs = 400;
        public const double HintMs = 3500;
        public const double MasterToastDelayMs = 1200;
        public const double BackupProgress = 0.70;
        public const double QuotaTrackWidth = 162.328;

        static readonly string[] Tips =
        {
            "Calibrating cosmic centrifuges...",
            "Polishing space soda cans...",
            "Waking up Nibbles for breakfast...",
            "Sprinkling candy on orbital rings...",
            "Ready for cleanup launch! 🚀"
        };

        static readonly string[] ModalOrder =
        {
            "modal-pause", "modal-privacy", "modal-reset-confirm", "modal-debrief", "modal-defeat"
        };

        readonly IAppClock _clock;
        readonly IAppAudio _audio;
        readonly Func<double> _random;
        readonly EpochClock _epoch = new EpochClock();
        readonly List<Timer> _timers = new List<Timer>();
        readonly HashSet<string> _modals = new HashSet<string>();
        readonly List<ToastModel> _toasts = new List<ToastModel>();
        readonly Dictionary<string, ElementOverride> _elements = new Dictionary<string, ElementOverride>();

        int _timerSeq;
        int _tipIdx;
        bool _barWritten;
        double _progress = 12;
        int _bootTimer = -1;
        string _loadingTip;
        string _loadingBar = "";
        string _faceHold;
        double _faceHoldUntil = -1;
        bool _dockGift = true;

        sealed class Timer
        {
            public int Id;
            public double Due;
            public double Interval;
            public Action Fire;
            public bool Alive;
        }

        public AppController(IProfileStore store, IAppClock clock, IAppAudio audio = null, Func<double> random = null)
        {
            if (store == null) throw new ArgumentNullException("store");
            if (clock == null) throw new ArgumentNullException("clock");
            _clock = clock;
            _audio = audio ?? new SilentAudio();
            _random = random ?? (() => JsRandom.Global.NextDouble());
            _epoch.NowEpochMs = clock.EpochMs;
            NowMs = clock.NowMs;
            EpochMs = clock.EpochMs;
            Economy = new EconomyManager(new StoreAdapter(store), _epoch);
            Sectors = MazeLevelBuilder.GetSectors();
            Worlds = MazeLevelBuilder.GetSectorWorlds();
            CurrentPage = "loading";
            StoreTab = "rings";
            WorldIndex = 1;
            _loadingTip = Tips[0];
            Model = new PageModel();
            SoundIcon = "🔊";
            RefreshCredits();
            RefreshHome();
            Rebuild();
        }

        public EconomyManager Economy { get; }
        public List<SectorConfig> Sectors { get; }
        public List<WorldInfo> Worlds { get; }
        public PageModel Model { get; }

        public string CurrentPage { get; private set; }
        public string StoreTab { get; private set; }
        public int WorldIndex { get; private set; }
        public int SectorIndex { get; private set; }
        public int SectorEpoch { get; private set; }
        public SectorConfig ActiveSector { get; private set; }
        public double CollectedMass { get; private set; }
        public double SpilledMass { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsLevelCleared { get; private set; }
        public bool IsLevelFailed { get; private set; }
        public bool UsedBackupPod { get; private set; }
        public bool HintHidden { get; private set; }
        public double NowMs { get; private set; }
        public double EpochMs { get; private set; }
        public string TemplateId { get; private set; }
        /// <summary>Comma-separated shop card kinds when no distilled spec matches this inventory. Empty when the spec is exact.</summary>
        public string CardStamp { get; private set; }
        public bool LayoutRequested { get; private set; }

        public string CreditText { get; private set; }
        public string QuotaText { get; private set; }
        public string LoadingBar { get { return _loadingBar; } }
        public string LoadingTip { get { return _loadingTip; } }
        public string MissionName { get; private set; }
        public string MissionGoal { get; private set; }
        public string MissionWorld { get; private set; }
        public string DailyBounty { get; private set; }
        public string ShopBadge { get; private set; }
        public string DailyBadge { get; private set; }
        public string GiftButton { get; private set; }
        public string GiftTitle { get; private set; }
        public string SoundIcon { get; private set; }
        public string PauseName { get; private set; }
        public string PauseQuota { get; private set; }
        public string DebriefMass { get; private set; }
        public string DebriefCredits { get; private set; }
        public string DefeatMass { get; private set; }
        public string DefeatSpilled { get; private set; }
        public bool BackupVisible { get; private set; }
        public string MasterAction { get; private set; }
        public string WorldTitle { get; private set; }
        public string WorldProgress { get; private set; }
        public string FirstLevelCode { get; private set; }
        public readonly List<string> ShopFooters = new List<string>();
        public readonly List<bool> DailyDone = new List<bool>();
        public IAppAudio Audio { get { return _audio; } }
        public readonly List<int> Haptics = new List<int>();
        public Action BackupDeployed;

        /// <summary>Web constructor already rolled sector 0. Record it without rolling again.</summary>
        public void AdoptSector(SectorConfig sector)
        {
            ActiveSector = sector;
            CollectedMass = 0;
            SpilledMass = 0;
            IsPaused = false;
            IsLevelCleared = false;
            IsLevelFailed = false;
            UsedBackupPod = false;
            if (sector != null && sector.WorldIndex.HasValue) WorldIndex = (int)sector.WorldIndex.Value;
            SectorEpoch++;
            Rebuild();
        }

        /// <summary>Start the loading-bar interval. Call after the sector-0 random prefix.</summary>
        public void BeginBoot()
        {
            _progress = 12;
            _tipIdx = 0;
            _barWritten = false;
            _loadingTip = Tips[0];
            _loadingBar = "";
            _bootTimer = Every(BootIntervalMs, BootTick);
            Rebuild();
        }

        public void Advance(double ms)
        {
            if (ms < 0) throw new ArgumentOutOfRangeException("ms");
            Pump(NowMs + ms);
            Rebuild();
        }

        /// <summary>Viewer frame. <paramref name="nowMs"/> and <paramref name="epochMs"/> are absolute.</summary>
        public void CatchUp(double nowMs, double epochMs)
        {
            if (nowMs < NowMs) nowMs = NowMs;
            double delta = nowMs - NowMs;
            EpochMs = epochMs;
            _epoch.NowEpochMs = epochMs;
            _clock.NowMs = nowMs;
            _clock.EpochMs = epochMs;
            Pump(NowMs + delta);
            Rebuild();
        }

        public void Click(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            switch (id)
            {
                case "btn-home-play":
                    _audio.PublicInit();
                    ShowPage("game");
                    break;
                case "btn-open-maps-page":
                    _audio.PublicInit();
                    ShowPage("maps");
                    break;
                case "btn-open-shop-page":
                    _audio.PublicInit();
                    ShowPage("shop");
                    break;
                case "btn-open-daily-shift":
                    _audio.PublicInit();
                    ShowPage("daily");
                    break;
                case "btn-open-trophies-page":
                    _audio.PublicInit();
                    ShowPage("achievements");
                    break;
                case "btn-open-settings-page":
                case "btn-home-settings":
                    _audio.PublicInit();
                    ShowPage("settings");
                    break;
                case "btn-home-audio":
                case "btn-toggle-audio":
                    ToggleHudMute();
                    break;
                case "btn-back-maps":
                case "btn-back-daily":
                case "btn-back-shop":
                case "btn-back-achievements":
                case "btn-back-settings":
                case "btn-game-to-home":
                    ShowPage("home");
                    break;
                case "btn-game-restart":
                    RestartSector();
                    break;
                case "btn-game-pause":
                    OpenPause();
                    break;
                case "btn-pause-resume":
                case "btn-close-pause":
                    ClosePause();
                    break;
                case "btn-pause-restart":
                    ClosePause();
                    RestartSector();
                    break;
                case "btn-pause-sound":
                    TogglePauseSound();
                    break;
                case "btn-pause-maps":
                    ClosePause();
                    ShowPage("maps");
                    break;
                case "btn-pause-home":
                    ExitGameToHome();
                    break;
                case "btn-debrief-replay":
                    CloseModal("modal-debrief");
                    RestartSector();
                    break;
                case "btn-debrief-next":
                    CloseModal("modal-debrief");
                    NextSector();
                    break;
                case "btn-defeat-retry":
                    CloseModal("modal-defeat");
                    RestartSector();
                    break;
                case "btn-defeat-home":
                    ResetGameplaySessionUi();
                    ShowPage("maps");
                    break;
                case "btn-defeat-backup":
                    DeployBackupPod();
                    break;
                case "btn-claim-daily-gift":
                    ClaimDailyGift();
                    break;
                case "btn-claim-master-bonus":
                    ClaimMasterBonus();
                    break;
                case "btn-open-privacy":
                    _modals.Add("modal-privacy");
                    Rebuild();
                    break;
                case "btn-close-privacy":
                case "btn-dismiss-privacy":
                    CloseModal("modal-privacy");
                    break;
                case "btn-reset-game-data":
                    _modals.Add("modal-reset-confirm");
                    Rebuild();
                    break;
                case "btn-cancel-reset":
                    CloseModal("modal-reset-confirm");
                    break;
                case "btn-confirm-reset":
                    ConfirmReset();
                    break;
                case "home-mascot-stage":
                case "home-diorama-nibbles":
                    PokeMascot();
                    break;
                case "settings-sound-toggle":
                    SetSoundEnabled(!Economy.Profile.Settings.SoundEnabled);
                    break;
                case "settings-haptics-toggle":
                    SetHaptics(!Economy.Profile.Settings.HapticsEnabled);
                    break;
                default:
                    return;
            }
        }

        public void ClickClass(string token, int index)
        {
            if (token == "tab-btn")
            {
                string[] tabs = { "rings", "singularities", "skyboxes" };
                if (index < 0 || index >= tabs.Length) return;
                StoreTab = tabs[index];
                Rebuild();
            }
            else if (token == "btn-buy-item")
            {
                var slot = NthShop(index, false, false);
                if (slot == null) return;
                if (Economy.UnlockItem(StoreTab, slot.Id, slot.Cost))
                {
                    Economy.EquipItem(StoreTab, slot.Id);
                    RefreshCredits();
                    RefreshHome();
                    ShowToast("Item Unlocked & Equipped! 🎉");
                }
                else ShowToast("Need more gold coins! 🪙");
                Rebuild();
            }
            else if (token == "btn-equip-item")
            {
                var slot = NthShop(index, true, false);
                if (slot == null) return;
                Economy.EquipItem(StoreTab, slot.Id);
                RefreshHome();
                ShowToast("Item Equipped! ✨");
                Rebuild();
            }
            else if (token == "world-tab-btn")
            {
                if (index < 0 || index >= Worlds.Count) return;
                WorldIndex = Worlds[index].WorldIndex;
                Rebuild();
            }
            else if (token == "level-tile")
            {
                var world = CurrentWorld();
                int level = world.LevelRangeStart + index;
                int sec = level - 1;
                if (sec < 0 || sec >= Sectors.Count) return;
                SectorIndex = sec;
                LoadSector(Sectors[sec], true);
                ShowPage("game");
            }
            else if (token == "btn-play-daily-tier")
            {
                var shifts = Economy.GetDailyShifts();
                if (index < 0 || index >= shifts.Count) return;
                LoadSector(MazeLevelBuilder.BuildDailyShift(shifts[index]), true);
                ShowPage("game");
            }
        }

        public void SetSlider(string id, int value)
        {
            if (id == "settings-sfx-slider")
            {
                Economy.Profile.Settings.SfxVolume = value;
                _audio.PublicInit();
                _audio.SetSfxVolume(value);
                Economy.SaveProfile();
            }
            else if (id == "settings-ambience-slider")
            {
                Economy.Profile.Settings.AmbientVolume = value;
                _audio.PublicInit();
                _audio.SetAmbienceVolume(value);
                Economy.SaveProfile();
            }
            Rebuild();
        }

        public void SetSoundEnabled(bool enabled)
        {
            Economy.Profile.Settings.SoundEnabled = enabled;
            if (_audio.IsMuted == enabled) _audio.ToggleMute();
            Economy.SaveProfile();
            Rebuild();
        }

        public void SetHaptics(bool enabled)
        {
            Economy.Profile.Settings.HapticsEnabled = enabled;
            Economy.SaveProfile();
            Rebuild();
        }

        public void KeyDown(string key)
        {
            _audio.PublicInit();
            DismissHint();
            if (key == "r" || key == "R") RestartSector();
            else if (key == "Escape" && CurrentPage == "game") ExitGameToHome();
        }

        public bool ConsumeLayoutRequest()
        {
            bool v = LayoutRequested;
            LayoutRequested = false;
            return v;
        }

        public void SetLiveMass(double collected, double spilled)
        {
            CollectedMass = collected;
            SpilledMass = spilled;
            Rebuild();
        }

        /// <summary>Repaint credit and home labels after an external economy edit (self-test grant).</summary>
        public void SyncEconomyDisplay()
        {
            RefreshCredits();
            RefreshHome();
            Rebuild();
        }

        public void ShowToastsFor(IList<Achievement> unlocked)
        {
            if (unlocked == null) return;
            for (int i = 0; i < unlocked.Count; i++)
                ShowToast("🏆 Trophy: " + unlocked[i].Title + " (+" + Num(unlocked[i].Reward) + " 🪙)");
            Rebuild();
        }

        /// <summary>
        /// Test-only hook. Same body as web <c>handleLevelVictory</c> after <c>collectedMass</c> is assigned.
        /// Live play uses <see cref="PresentSessionVictory"/> because <c>LiveGameSession</c> already pays.
        /// </summary>
        public void ForceVictory(double collectedMass)
        {
            CollectedMass = collectedMass;
            HandleLevelVictory();
        }

        /// <summary>Test-only hook. Same body as web <c>handleLevelFailure</c>.</summary>
        public void ForceDefeat(double collectedMass, double spilledMass)
        {
            CollectedMass = collectedMass;
            SpilledMass = spilledMass;
            HandleLevelFailure();
        }

        public void PresentSessionVictory(double collectedMass, double payout, IList<Achievement> unlocked)
        {
            CollectedMass = collectedMass;
            IsLevelCleared = true;
            DebriefMass = Num(collectedMass) + " kg";
            DebriefCredits = "+" + Num(payout) + " 🪙";
            ShowToastsFor(unlocked);
            if (ActiveSector != null && ActiveSector.IsDaily == true)
            {
                var shifts = Economy.GetDailyShifts();
                int done = 0;
                for (int i = 0; i < shifts.Count; i++) if (shifts[i].IsCompleted) done++;
                if (done == 3 && Economy.CanClaimDailyMasterBonus())
                    After(MasterToastDelayMs, () => ShowToast("🎉 All 3 Daily Shifts Cleared! Claim Master Bonus in Daily Tab! 🏆"));
            }
            After(DebriefDelayMs, () => { _modals.Add("modal-debrief"); Rebuild(); });
            _audio.PlayVictoryFanfare();
            Rebuild();
        }

        public void PresentSessionDefeat(double collectedMass, double spilledMass)
        {
            if (IsLevelCleared || IsLevelFailed) return;
            CollectedMass = collectedMass;
            SpilledMass = spilledMass;
            IsLevelFailed = true;
            _audio.PlayDefeatSound();
            FillDefeat();
            After(DefeatDelayMs, () => { _modals.Add("modal-defeat"); Rebuild(); });
            Rebuild();
        }

        public void ShowPage(string pageId)
        {
            CurrentPage = pageId;
            RefreshCredits();
            if (pageId == "home") RefreshHome();
            else if (pageId == "game")
            {
                LayoutRequested = true;
                After(HintMs, DismissHint);
            }
            Rebuild();
        }

        public void LoadSector(SectorConfig sector, bool toast)
        {
            ActiveSector = sector;
            CollectedMass = 0;
            SpilledMass = 0;
            ResetGameplaySessionUi();
            if (sector != null && sector.WorldIndex.HasValue) WorldIndex = (int)sector.WorldIndex.Value;
            SectorEpoch++;
            RefreshCredits();
            if (toast && sector != null) ShowToast("Clean up " + sector.Name + "! 🚀");
            Rebuild();
        }

        public void RestartSector()
        {
            if (ActiveSector != null) LoadSector(ActiveSector, true);
        }

        public void NextSector()
        {
            if (ActiveSector != null && ActiveSector.IsDaily == true)
            {
                var shifts = Economy.GetDailyShifts();
                string tier = ActiveSector.DailyTier;
                DailyShiftInfo next = null;
                if (tier == "bronze") next = FindTier(shifts, "silver");
                else if (tier == "silver") next = FindTier(shifts, "gold");
                if (next != null) LoadSector(MazeLevelBuilder.BuildDailyShift(next), true);
                else ShowPage("daily");
                return;
            }
            if (Sectors.Count == 0) return;
            SectorIndex = (SectorIndex + 1) % Sectors.Count;
            LoadSector(Sectors[SectorIndex], true);
        }

        public void ExitGameToHome()
        {
            ResetGameplaySessionUi();
            ShowPage("home");
        }

        public bool ModalOpen(string id)
        {
            return _modals.Contains(id);
        }

        public IReadOnlyList<string> OpenModals()
        {
            var list = new List<string>();
            for (int i = 0; i < ModalOrder.Length; i++)
                if (_modals.Contains(ModalOrder[i])) list.Add(ModalOrder[i]);
            return list;
        }

        public IReadOnlyList<string> ToastTexts()
        {
            var list = new List<string>();
            for (int i = 0; i < _toasts.Count; i++) list.Add(_toasts[i].Text);
            return list;
        }

        void BootTick()
        {
            _progress += JsMath.Floor(_random() * 22) + 14;
            if (_progress > 100) _progress = 100;
            _barWritten = true;
            _loadingBar = Num(_progress) + "%";
            _tipIdx = (_tipIdx + 1) % Tips.Length;
            _loadingTip = Tips[_tipIdx];
            if (_progress >= 100)
            {
                Cancel(_bootTimer);
                _bootTimer = -1;
                After(BootDoneDelayMs, () => ShowPage("home"));
            }
        }

        void HandleLevelVictory()
        {
            IsLevelCleared = true;
            _audio.PlayVictoryFanfare();
            double reward = ActiveSector != null && ActiveSector.RewardCredits.HasValue ? ActiveSector.RewardCredits.Value : 0;
            double quota = ActiveSector != null ? ActiveSector.Quota : 30;
            double payout = EconomyManager.CampaignPayout(reward, CollectedMass, quota);
            Economy.AddCredits(payout);
            RefreshCredits();
            if (ActiveSector != null && ActiveSector.IsDaily != true && !string.IsNullOrEmpty(ActiveSector.Id))
                Economy.RecordSectorCompleted(ActiveSector.Id);
            bool daily = ActiveSector != null && ActiveSector.IsDaily == true;
            string tier = ActiveSector != null && !string.IsNullOrEmpty(ActiveSector.DailyTier) ? ActiveSector.DailyTier : "silver";
            var unlocked = Economy.RecordContractComplete(daily, tier);
            ShowToastsFor(unlocked);
            if (daily)
            {
                var shifts = Economy.GetDailyShifts();
                int done = 0;
                for (int i = 0; i < shifts.Count; i++) if (shifts[i].IsCompleted) done++;
                if (done == 3 && Economy.CanClaimDailyMasterBonus())
                    After(MasterToastDelayMs, () => ShowToast("🎉 All 3 Daily Shifts Cleared! Claim Master Bonus in Daily Tab! 🏆"));
            }
            DebriefMass = Num(CollectedMass) + " kg";
            DebriefCredits = "+" + Num(payout) + " 🪙";
            After(DebriefDelayMs, () => { _modals.Add("modal-debrief"); Rebuild(); });
            Rebuild();
        }

        void HandleLevelFailure()
        {
            if (IsLevelCleared || IsLevelFailed) return;
            IsLevelFailed = true;
            _audio.PlayDefeatSound();
            FillDefeat();
            After(DefeatDelayMs, () => { _modals.Add("modal-defeat"); Rebuild(); });
            Rebuild();
        }

        void FillDefeat()
        {
            double quota = ActiveSector != null ? ActiveSector.Quota : 30;
            DefeatMass = Num(CollectedMass) + " / " + Num(quota) + " kg";
            DefeatSpilled = Num(SpilledMass) + " kg";
            double ratio = CollectedMass / Math.Max(1, quota);
            BackupVisible = ratio >= BackupProgress && !UsedBackupPod;
        }

        void DeployBackupPod()
        {
            double quota = ActiveSector != null ? ActiveSector.Quota : 30;
            double ratio = CollectedMass / Math.Max(1, quota);
            if (!(ratio >= BackupProgress) || UsedBackupPod) return;
            CloseModal("modal-defeat");
            UsedBackupPod = true;
            IsLevelFailed = false;
            _audio.PlayBackupPodSound();
            Haptic(50);
            if (BackupDeployed != null) BackupDeployed();
            ShowToast("🚀 Auxiliary Scrap Pod Deployed! (+8 Scrap)");
            Rebuild();
        }

        void ClaimDailyGift()
        {
            if (!Economy.CanClaimDailyGift()) return;
            if (!Economy.ClaimDailyGift(100)) return;
            try { _audio.PublicInit(); _audio.PlayBoosterPass(); } catch (Exception) { }
            Haptic(40);
            RefreshCredits();
            ShowToast("🎁 Claimed +100 🪙 Janitor Stipend!");
            Rebuild();
        }

        void ClaimMasterBonus()
        {
            if (!Economy.ClaimDailyMasterBonus(300)) return;
            try { _audio.PublicInit(); _audio.PlayBoosterPass(); } catch (Exception) { }
            Haptic(50);
            RefreshCredits();
            ShowToast("🏆 Claimed +300 🪙 Daily Master Bonus!");
            Rebuild();
        }

        void ConfirmReset()
        {
            Economy.ResetProfile();
            CloseModal("modal-reset-confirm");
            if (Sectors.Count > 0)
            {
                SectorIndex = 0;
                LoadSector(Sectors[0], false);
            }
            ShowToast("Progress Reset to Day 1! 🧹");
            ShowPage("home");
        }

        void OpenPause()
        {
            IsPaused = true;
            _modals.Add("modal-pause");
            Rebuild();
        }

        void ClosePause()
        {
            IsPaused = false;
            CloseModal("modal-pause");
        }

        void CloseModal(string id)
        {
            _modals.Remove(id);
            Rebuild();
        }

        void ResetGameplaySessionUi()
        {
            IsPaused = false;
            IsLevelCleared = false;
            IsLevelFailed = false;
            UsedBackupPod = false;
            _modals.Remove("modal-pause");
            _modals.Remove("modal-debrief");
            _modals.Remove("modal-defeat");
        }

        void ToggleHudMute()
        {
            _audio.PublicInit();
            bool muted = _audio.ToggleMute();
            Economy.Profile.Settings.SoundEnabled = !muted;
            Economy.SaveProfile();
            SoundIcon = muted ? "🔇" : "🔊";
            ShowToast(muted ? "Sound Off 🔇" : "Sound On 🔊");
            Rebuild();
        }

        void TogglePauseSound()
        {
            _audio.PublicInit();
            bool muted = _audio.ToggleMute();
            Economy.Profile.Settings.SoundEnabled = !muted;
            Economy.SaveProfile();
            SoundIcon = muted ? "🔇" : "🔊";
            Rebuild();
        }

        void PokeMascot()
        {
            try { _audio.PublicInit(); _audio.PlayBoosterPass(); } catch (Exception) { }
            Haptic(30);
            _faceHold = "🥰";
            _faceHoldUntil = NowMs + 600;
            After(600, () => { _faceHold = null; Rebuild(); });
            Rebuild();
        }

        void DismissHint()
        {
            HintHidden = true;
        }

        void Haptic(int ms)
        {
            if (Economy.Profile.Settings != null && Economy.Profile.Settings.HapticsEnabled)
                Haptics.Add(ms);
        }

        void ShowToast(string msg)
        {
            _toasts.Add(new ToastModel { Text = msg, ExpiresAtMs = NowMs + ToastMs });
            After(ToastMs, () =>
            {
                for (int i = 0; i < _toasts.Count; i++)
                {
                    if (_toasts[i].Text == msg && _toasts[i].ExpiresAtMs <= NowMs + 0.001)
                    {
                        _toasts.RemoveAt(i);
                        break;
                    }
                }
                Rebuild();
            });
        }

        CosmeticItem NthShop(int index, bool unlocked, bool equipped)
        {
            var items = EconomyCatalog.Category(StoreTab);
            if (items == null) return null;
            string equippedId = Equipped(Economy.NormalizeCategory(StoreTab).Singular);
            int seen = 0;
            for (int i = 0; i < items.Length; i++)
            {
                bool own = Economy.IsUnlocked(StoreTab, items[i].Id);
                bool on = equippedId == items[i].Id;
                if (on) continue;
                if (own != unlocked) continue;
                if (equipped && !on) continue;
                if (seen == index) return items[i];
                seen++;
            }
            return null;
        }

        void RefreshCredits()
        {
            CreditText = Num(Economy.Profile.Credits);
            _dockGift = Economy.CanClaimDailyGift();
            ShopBadge = "FREE";
            var shifts = Economy.GetDailyShifts();
            int done = 0;
            for (int i = 0; i < shifts.Count; i++) if (shifts[i].IsCompleted) done++;
            if (done == 3) DailyBadge = Economy.CanClaimDailyMasterBonus() ? "GIFT" : "✓";
            else DailyBadge = (3 - done) + " NEW";
        }

        void RefreshHome()
        {
            var sec = ActiveSector ?? (Sectors.Count > 0 ? Sectors[0] : null);
            if (sec != null)
            {
                MissionName = sec.Code + " • " + sec.Name;
                MissionGoal = "Goal: " + Num(sec.Quota) + " kg";
                var world = FindWorld(sec.WorldIndex.HasValue ? (int)sec.WorldIndex.Value : 1);
                MissionWorld = world != null ? world.Title : "World " + Num(sec.WorldIndex ?? 1);
            }
            var silver = Economy.GetDailyShiftInfo();
            DailyBounty = "+" + Num(silver.RewardCredits ?? 0) + " 🪙 Bounty";
        }

        void Rebuild()
        {
            _epoch.NowEpochMs = EpochMs;
            double quota = ActiveSector != null ? ActiveSector.Quota : 30;
            QuotaText = Num(CollectedMass) + " / " + Num(quota) + " kg";

            bool gift = Economy.CanClaimDailyGift();
            var shifts = Economy.GetDailyShifts();
            DailyDone.Clear();
            for (int i = 0; i < shifts.Count; i++) DailyDone.Add(shifts[i].IsCompleted);

            if (gift)
            {
                GiftButton = "Claim +100 🪙";
                GiftTitle = "Janitor Care Package!";
            }
            else
            {
                GiftButton = "Claimed Today ✨";
                GiftTitle = "Allowance Claimed! ✨";
            }

            if (ActiveSector != null)
            {
                PauseName = ActiveSector.Code + " • " + ActiveSector.Name;
                PauseQuota = "Goal: Feed Nibbles " + Num(ActiveSector.Quota) + " kg (" + Num(CollectedMass) + "/" + Num(ActiveSector.Quota) + " kg eaten)";
            }

            bool canMaster = Economy.CanClaimDailyMasterBonus();
            bool masterClaimed = Economy.IsDailyMasterClaimed();
            if (canMaster) MasterAction = "Claim +300 🪙";
            else if (masterClaimed) MasterAction = "CLAIMED ✓";
            else MasterAction = "+300 🪙";

            var current = CurrentWorld();
            WorldTitle = current.Title;
            int worldDone = 0;
            int worldTotal = current.LevelRangeEnd - current.LevelRangeStart + 1;
            var completed = Economy.Profile.CompletedSectors ?? new List<string>();
            for (int level = current.LevelRangeStart; level <= current.LevelRangeEnd; level++)
            {
                int idx = level - 1;
                if (idx >= 0 && idx < Sectors.Count && completed.Contains(Sectors[idx].Id)) worldDone++;
            }
            WorldProgress = "⭐ " + worldDone + "/" + worldTotal;
            int first = current.LevelRangeStart - 1;
            FirstLevelCode = first >= 0 && first < Sectors.Count ? Sectors[first].Code : "";

            ShopFooters.Clear();
            var catalog = EconomyCatalog.Category(StoreTab);
            string singular = Economy.NormalizeCategory(StoreTab).Singular;
            string equippedId = Equipped(singular);
            if (catalog != null)
            {
                for (int i = 0; i < catalog.Length; i++)
                {
                    bool own = Economy.IsUnlocked(StoreTab, catalog[i].Id);
                    bool on = equippedId == catalog[i].Id;
                    if (on) ShopFooters.Add("EQUIPPED ✨");
                    else if (own) ShopFooters.Add("Wear");
                    else if (catalog[i].Cost == 0) ShopFooters.Add("Free Starter");
                    else ShopFooters.Add("Unlock (" + Num(catalog[i].Cost) + " 🪙)");
                }
            }

            if (IsLevelFailed || _modals.Contains("modal-defeat")) FillDefeat();

            _elements.Clear();
            Text("home-credits-val", CreditText);
            Text("hud-credits-val", CreditText);
            Text("maps-credits-val", CreditText);
            Text("daily-credits-val", CreditText);
            Text("shop-credits-val", CreditText);
            Text("ach-credits-val", CreditText);
            Text("home-mission-name", MissionName);
            Text("home-mission-goal", MissionGoal);
            Text("home-mission-world", MissionWorld);
            Text("home-daily-bounty", DailyBounty);
            Text("hud-quota-text", QuotaText);
            if (ActiveSector != null) Text("hud-sector-code", "⭐ " + ActiveSector.Code);
            double pct = quota > 0 ? Math.Min(100, (CollectedMass / quota) * 100) : 0;
            WidthPx("hud-quota-bar", QuotaTrackWidth * pct / 100.0);
            Text("dock-daily-badge", DailyBadge);
            if (_dockGift) Text("shop-dock-badge", "FREE");
            else Hide("shop-dock-badge");
            Text("gift-banner-title", GiftTitle);
            Text("gift-banner-desc", gift ? "Claim your free daily +100 🪙 allowance." : "Come back tomorrow for another +100 🪙 care package!");
            Text("gift-banner-icon", gift ? "🎁" : "🎉");
            Text("btn-claim-daily-gift", GiftButton);
            Text("home-audio-icon", SoundIcon);
            Text("audio-icon", SoundIcon);
            Text("home-diorama-ring-label", RingLabel(Equipped("ring")));
            Text("home-diorama-face", _faceHold != null && NowMs < _faceHoldUntil ? _faceHold : SingularityFace(Equipped("singularity")));
            Text("pause-level-name", PauseName);
            Text("pause-level-quota", PauseQuota);
            Text("pause-sound-icon", _audio.IsMuted ? "🔇" : "🔊");
            Text("debrief-mass-val", DebriefMass);
            Text("debrief-credits-val", DebriefCredits);
            Text("defeat-mass-val", DefeatMass);
            Text("defeat-spilled-val", DefeatSpilled);
            if (!BackupVisible) Hide("defeat-backup-container");
            if (_barWritten)
            {
                Text("loading-tip-text", _loadingTip);
                WidthPercent("loading-bar-fill", _progress);
            }
            if (HintHidden) Hide("gesture-hint");
            Skin("home-diorama-ring", Equipped("ring"));
            Skin("home-diorama-nibbles", Equipped("singularity"));

            string stamp;
            TemplateId = ChooseTemplate(canMaster, out stamp);
            CardStamp = stamp;
            Model.PageId = CurrentPage;
            Model.TemplateId = TemplateId;
            Model.Modals.Clear();
            var open = OpenModals();
            for (int i = 0; i < open.Count; i++) Model.Modals.Add(open[i]);
            Model.Toasts.Clear();
            for (int i = 0; i < _toasts.Count; i++) Model.Toasts.Add(_toasts[i]);
            Model.Elements.Clear();
            foreach (var kv in _elements) Model.Elements[kv.Key] = kv.Value;
        }

        string ChooseTemplate(bool canMaster, out string cardStamp)
        {
            var current = CurrentWorld();
            int worldDone = 0;
            int worldTotal = current.LevelRangeEnd - current.LevelRangeStart + 1;
            var completed = Economy.Profile.CompletedSectors ?? new List<string>();
            for (int level = current.LevelRangeStart; level <= current.LevelRangeEnd; level++)
            {
                int idx = level - 1;
                if (idx >= 0 && idx < Sectors.Count && completed.Contains(Sectors[idx].Id)) worldDone++;
            }
            bool through44 = true;
            for (int n = 1; n <= 44; n++)
                if (!completed.Contains("level_" + n)) { through44 = false; break; }
            if (through44 && completed.Contains("level_45")) through44 = false;
            int cleared = 0;
            for (int i = 0; i < DailyDone.Count; i++) if (DailyDone[i]) cleared++;
            double quota = ActiveSector != null ? ActiveSector.Quota : 30;
            var settings = Economy.Profile.Settings;
            string modal = "";
            if (_modals.Contains("modal-debrief")) modal = "debrief";
            else if (_modals.Contains("modal-defeat")) modal = "defeat";
            else if (_modals.Contains("modal-pause")) modal = "pause";
            else if (_modals.Contains("modal-privacy")) modal = "privacy";
            else if (_modals.Contains("modal-reset-confirm")) modal = "reset";
            var query = new TemplateQuery
            {
                Page = CurrentPage ?? "",
                Modal = modal,
                WorldIndex = WorldIndex,
                ActiveWorld = ActiveSector != null && ActiveSector.WorldIndex.HasValue ? (int)ActiveSector.WorldIndex.Value : WorldIndex,
                WorldDone = worldDone,
                WorldTotal = worldTotal,
                Levels1Through44 = through44,
                ShopTab = StoreTab ?? "rings",
                ShopSignature = ShopSignature(),
                GiftClaimed = !Economy.CanClaimDailyGift(),
                DailyCleared = cleared,
                MasterReady = canMaster,
                MasterClaimed = Economy.IsDailyMasterClaimed(),
                SoundOn = settings == null || settings.SoundEnabled,
                HapticsOn = settings == null || settings.HapticsEnabled,
                SlidersDefault = settings == null || (Math.Abs(settings.SfxVolume - 80) < 0.01 && Math.Abs(settings.AmbientVolume - 70) < 0.01),
                DebriefDaily = ActiveSector != null && !string.IsNullOrEmpty(ActiveSector.DailyTier),
                DebriefSurplus = ActiveSector != null && CollectedMass > ActiveSector.Quota,
                DefeatUsed = UsedBackupPod,
                DefeatRatio = CollectedMass / Math.Max(1, quota),
                AchievementsUnlocked = Economy.Profile.Achievements != null ? Economy.Profile.Achievements.Count : 0,
                AchievementsTotal = EconomyCatalog.Achievements.Length,
                GameDaily = ActiveSector != null && !string.IsNullOrEmpty(ActiveSector.DailyTier)
            };
            return TemplateSelector.Select(query, out cardStamp);
        }

        string ShopSignature()
        {
            var items = EconomyCatalog.Category(StoreTab);
            if (items == null) return "";
            var owned = new List<string>();
            for (int i = 0; i < items.Length; i++)
                if (Economy.IsUnlocked(StoreTab, items[i].Id)) owned.Add(items[i].Id);
            return TemplateSelector.Signature(owned, Equipped(Economy.NormalizeCategory(StoreTab).Singular));
        }

        WorldInfo CurrentWorld()
        {
            return FindWorld(WorldIndex) ?? Worlds[0];
        }

        WorldInfo FindWorld(int index)
        {
            for (int i = 0; i < Worlds.Count; i++)
                if (Worlds[i].WorldIndex == index) return Worlds[i];
            return null;
        }

        static DailyShiftInfo FindTier(List<DailyShiftInfo> shifts, string tier)
        {
            for (int i = 0; i < shifts.Count; i++)
                if (shifts[i].Tier == tier) return shifts[i];
            return null;
        }

        string Equipped(string singular)
        {
            string id = Economy.Profile.Equipped.Get(singular);
            if (!string.IsNullOrEmpty(id)) return id;
            if (singular == "ring") return "ring_donut";
            if (singular == "singularity") return "singularity_nibbles";
            return "skybox_cotton_candy";
        }

        static string RingLabel(string id)
        {
            if (id == "ring_duckie") return "🦆 DUCKIE FLOAT";
            if (id == "ring_candyland") return "🍬 CANDY RING";
            if (id == "ring_spacecat") return "🐱 KITTY SAUCER";
            return "🍩 DONUT FLOAT";
        }

        static string SingularityFace(string id)
        {
            if (id == "singularity_strawberry") return "🍓";
            if (id == "singularity_sleepy") return "💤";
            if (id == "singularity_confetti") return "🎉";
            return "😋";
        }

        void Text(string id, string value)
        {
            if (value == null) return;
            ElementOverride o;
            if (!_elements.TryGetValue(id, out o)) { o = new ElementOverride(); _elements[id] = o; }
            o.Text = value;
            o.HasText = true;
        }

        void SetText(string id, string value) { Text(id, value); }

        void Hide(string id)
        {
            ElementOverride o;
            if (!_elements.TryGetValue(id, out o)) { o = new ElementOverride(); _elements[id] = o; }
            o.Visible = false;
        }

        void WidthPercent(string id, double pct)
        {
            ElementOverride o;
            if (!_elements.TryGetValue(id, out o)) { o = new ElementOverride(); _elements[id] = o; }
            o.WidthPercent = pct;
        }

        void WidthPx(string id, double px)
        {
            ElementOverride o;
            if (!_elements.TryGetValue(id, out o)) { o = new ElementOverride(); _elements[id] = o; }
            o.WidthPx = px;
        }

        void Skin(string id, string skin)
        {
            ElementOverride o;
            if (!_elements.TryGetValue(id, out o)) { o = new ElementOverride(); _elements[id] = o; }
            o.Skin = skin;
        }

        int After(double delay, Action fire)
        {
            var t = new Timer { Id = ++_timerSeq, Due = NowMs + delay, Interval = 0, Fire = fire, Alive = true };
            _timers.Add(t);
            return t.Id;
        }

        int Every(double interval, Action fire)
        {
            var t = new Timer { Id = ++_timerSeq, Due = NowMs + interval, Interval = interval, Fire = fire, Alive = true };
            _timers.Add(t);
            return t.Id;
        }

        void Cancel(int id)
        {
            for (int i = 0; i < _timers.Count; i++)
                if (_timers[i].Id == id) _timers[i].Alive = false;
        }

        void Pump(double end)
        {
            while (true)
            {
                int best = -1;
                double due = 0;
                for (int i = 0; i < _timers.Count; i++)
                {
                    var t = _timers[i];
                    if (!t.Alive || t.Due > end) continue;
                    if (best < 0 || t.Due < due || (t.Due == due && t.Id < _timers[best].Id))
                    {
                        best = i;
                        due = t.Due;
                    }
                }
                if (best < 0) break;
                double delta = due - NowMs;
                NowMs = due;
                EpochMs += delta;
                _epoch.NowEpochMs = EpochMs;
                var timer = _timers[best];
                if (timer.Interval > 0) timer.Due += timer.Interval;
                else timer.Alive = false;
                if (timer.Fire != null) timer.Fire();
            }
            double rest = end - NowMs;
            NowMs = end;
            EpochMs += rest;
            _epoch.NowEpochMs = EpochMs;
            _clock.NowMs = NowMs;
            _clock.EpochMs = EpochMs;
        }

        public static string Num(double value)
        {
            return JsNumber.ToString(value);
        }
    }
}
