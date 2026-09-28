using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OrbitalSalvage.Core;
using OrbitalSalvage.Utils;

namespace OrbitalSalvage.UI
{
    /// <summary>
    /// 1:1 Complete UI Architecture matching the Web Prototype.
    /// Utilizes high-res 3D Emoji Sprites (zero missing glyph boxes),
    /// responsive layouts, and full Edit-Mode preview support.
    /// </summary>
    [ExecuteAlways]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Main Pages")]
        [SerializeField] private GameObject homePanel;
        [SerializeField] private GameObject mapsPanel;
        [SerializeField] private GameObject dailyPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject trophiesPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject inGameHudPanel;

        [Header("Modals")]
        [SerializeField] private GameObject pauseModal;
        [SerializeField] private GameObject victoryModal;
        [SerializeField] private GameObject defeatModal;

        [Header("In-Game HUD References")]
        [SerializeField] private TextMeshProUGUI hudLevelBadge;
        [SerializeField] private TextMeshProUGUI hudQuotaText;
        [SerializeField] private Image hudQuotaBarFill;
        [SerializeField] private TextMeshProUGUI homeCreditsText;
        [SerializeField] private TextMeshProUGUI mapsCreditsText;

        [Header("Debrief References")]
        [SerializeField] private TextMeshProUGUI victoryStarsText;
        [SerializeField] private TextMeshProUGUI victoryStatsText;
        [SerializeField] private TextMeshProUGUI defeatStatsText;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            EnsureHierarchyBuilt();
            if (Application.isPlaying)
            {
                ShowHome();
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                UpdateHudData();
            }
        }

        public void EnsureHierarchyBuilt(bool force = false)
        {
            if (force)
            {
                homePanel = null;
                mapsPanel = null;
                dailyPanel = null;
                shopPanel = null;
                trophiesPanel = null;
                settingsPanel = null;
                inGameHudPanel = null;
                pauseModal = null;
                victoryModal = null;
                defeatModal = null;
            }
            BuildCompleteUIHierarchy();
        }

        private void UpdateHudData()
        {
            if (GameManager.Instance == null) return;

            int credits = EconomyManager.Instance != null ? EconomyManager.Instance.SaveData.scrapCredits : 250;
            if (homeCreditsText != null) homeCreditsText.text = $"{credits}";
            if (mapsCreditsText != null) mapsCreditsText.text = $"{credits}";

            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (hudLevelBadge != null)
                {
                    hudLevelBadge.text = $"LVL {GameManager.Instance.CurrentLevelNumber}";
                }

                if (hudQuotaText != null)
                {
                    hudQuotaText.text = $"{GameManager.Instance.SwallowedDebrisCount} / {GameManager.Instance.CurrentQuota} kg";
                }

                if (hudQuotaBarFill != null)
                {
                    float p = (float)GameManager.Instance.SwallowedDebrisCount / Mathf.Max(1, GameManager.Instance.CurrentQuota);
                    hudQuotaBarFill.fillAmount = Mathf.Clamp01(p);
                }
            }
        }

        #region Navigation Page Switching
        public void SetWorldVisibility(bool visible)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameplayWorldVisible(visible);
            }
            else
            {
                var centrifuge = GameObject.Find("CentrifugeRoot");
                if (centrifuge != null) centrifuge.SetActive(visible);

                var debris = GameObject.Find("DebrisContainer");
                if (debris != null) debris.SetActive(visible);

                var funnel = GameObject.Find("ContainmentFunnel");
                if (funnel != null) funnel.SetActive(visible);

                var nibbles = GameObject.Find("NibblesMascot");
                if (nibbles != null) nibbles.SetActive(visible);
            }
        }

        public void ShowHome()
        {
            EnsureHierarchyBuilt();
            SetPage(homePanel);
            SetWorldVisibility(false);
            if (Application.isPlaying)
            {
                Time.timeScale = 1f;
                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.SetState(GameState.MainMenu);
                }
                AudioManager.Instance?.PlayTabSwitch();
            }
        }

        public void ShowMaps()
        {
            EnsureHierarchyBuilt();
            SetPage(mapsPanel);
            SetWorldVisibility(false);
            if (Application.isPlaying) AudioManager.Instance?.PlayTabSwitch();
        }

        public void ShowDaily()
        {
            EnsureHierarchyBuilt();
            SetPage(dailyPanel);
            SetWorldVisibility(false);
            if (Application.isPlaying) AudioManager.Instance?.PlayTabSwitch();
        }

        public void ShowShop()
        {
            EnsureHierarchyBuilt();
            SetPage(shopPanel);
            SetWorldVisibility(false);
            if (Application.isPlaying) AudioManager.Instance?.PlayTabSwitch();
        }

        public void ShowTrophies()
        {
            EnsureHierarchyBuilt();
            SetPage(trophiesPanel);
            SetWorldVisibility(false);
            if (Application.isPlaying) AudioManager.Instance?.PlayTabSwitch();
        }

        public void ShowSettings()
        {
            EnsureHierarchyBuilt();
            SetPage(settingsPanel);
            SetWorldVisibility(false);
            if (Application.isPlaying) AudioManager.Instance?.PlayTabSwitch();
        }

        public void ShowGameplay()
        {
            EnsureHierarchyBuilt();
            SetPage(inGameHudPanel);
            SetWorldVisibility(true);
            if (Application.isPlaying)
            {
                Time.timeScale = 1f;
                AudioManager.Instance?.PlayButtonClick();
            }
        }

        public void HideAllPages()
        {
            if (homePanel != null) homePanel.SetActive(false);
            if (mapsPanel != null) mapsPanel.SetActive(false);
            if (dailyPanel != null) dailyPanel.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (trophiesPanel != null) trophiesPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (inGameHudPanel != null) inGameHudPanel.SetActive(false);
            HideAllModals();
            SetWorldVisibility(true);
        }

        public void StartSector(int sectorNum)
        {
            ShowGameplay();
            GameManager.Instance?.StartSector(sectorNum);
        }

        public void TogglePause()
        {
            if (pauseModal == null) return;
            bool isPaused = pauseModal.activeSelf;
            ShowModalPause(!isPaused);
        }

        public void ShowModalPause(bool show)
        {
            EnsureHierarchyBuilt();
            if (pauseModal != null) pauseModal.SetActive(show);
            if (Application.isPlaying)
            {
                Time.timeScale = show ? 0f : 1f;
                AudioManager.Instance?.PlayButtonClick();
            }
        }

        public void ShowModalVictory(bool show)
        {
            EnsureHierarchyBuilt();
            if (victoryModal != null) victoryModal.SetActive(show);
            if (show && Application.isPlaying)
            {
                if (victoryStatsText != null && GameManager.Instance != null)
                {
                    victoryStatsText.text = $"Swallowed {GameManager.Instance.SwallowedDebrisCount} pieces of space scrap!";
                }
            }
        }

        public void ShowModalDefeat(bool show)
        {
            EnsureHierarchyBuilt();
            if (defeatModal != null) defeatModal.SetActive(show);
            if (show && Application.isPlaying)
            {
                if (defeatStatsText != null && GameManager.Instance != null)
                {
                    defeatStatsText.text = $"Quota: {GameManager.Instance.SwallowedDebrisCount} / {GameManager.Instance.CurrentQuota} kg\nNibbles is still hungry!";
                }
            }
        }

        public void HideAllModals()
        {
            if (pauseModal != null) pauseModal.SetActive(false);
            if (victoryModal != null) victoryModal.SetActive(false);
            if (defeatModal != null) defeatModal.SetActive(false);
            if (Application.isPlaying) Time.timeScale = 1f;
        }

        public void OnVictory() => ShowModalVictory(true);
        public void OnDefeat() => ShowModalDefeat(true);

        private void SetPage(GameObject target)
        {
            if (homePanel != null) homePanel.SetActive(homePanel == target);
            if (mapsPanel != null) mapsPanel.SetActive(mapsPanel == target);
            if (dailyPanel != null) dailyPanel.SetActive(dailyPanel == target);
            if (shopPanel != null) shopPanel.SetActive(shopPanel == target);
            if (trophiesPanel != null) trophiesPanel.SetActive(trophiesPanel == target);
            if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == target);
            if (inGameHudPanel != null) inGameHudPanel.SetActive(inGameHudPanel == target);

            HideAllModals();
        }
        #endregion

        #region Procedural UI Hierarchy Builder (100% 3D Emoji Sprites, No Unicode Box Failures)
        private void BuildCompleteUIHierarchy()
        {
            Transform safeArea = transform.Find("SafeAreaPanel");
            if (safeArea == null)
            {
                var sa = new GameObject("SafeAreaPanel", typeof(RectTransform));
                sa.transform.SetParent(transform, false);
                var saRt = sa.GetComponent<RectTransform>();
                saRt.anchorMin = Vector2.zero;
                saRt.anchorMax = Vector2.one;
                saRt.sizeDelta = Vector2.zero;
                safeArea = sa.transform;
            }

            // Clean previous hierarchy if rebuilding in Editor
            if (homePanel == null || homePanel.transform.childCount == 0)
            {
                // Clear old panels if any
                for (int i = safeArea.childCount - 1; i >= 0; i--)
                {
                    var child = safeArea.GetChild(i).gameObject;
                    if (Application.isPlaying) Destroy(child);
                    else DestroyImmediate(child);
                }

                // 1. Build Home Page
                homePanel = CreatePanel(safeArea, "Page_Home");
                BuildHomeScreen(homePanel.transform);

                // 2. Build Maps Page
                mapsPanel = CreatePanel(safeArea, "Page_Maps");
                BuildMapsScreen(mapsPanel.transform);

                // 3. Build Daily Page
                dailyPanel = CreatePanel(safeArea, "Page_Daily");
                BuildDailyScreen(dailyPanel.transform);

                // 4. Build Shop Page
                shopPanel = CreatePanel(safeArea, "Page_Shop");
                BuildShopScreen(shopPanel.transform);

                // 5. Build Trophies Page
                trophiesPanel = CreatePanel(safeArea, "Page_Trophies");
                BuildTrophiesScreen(trophiesPanel.transform);

                // 6. Build Settings Page
                settingsPanel = CreatePanel(safeArea, "Page_Settings");
                BuildSettingsScreen(settingsPanel.transform);

                // 7. Build In-Game HUD
                inGameHudPanel = CreatePanel(safeArea, "Page_GameplayHUD");
                BuildGameplayHud(inGameHudPanel.transform);

                // 8. Build Modals
                BuildModals(safeArea);
            }
        }

        private GameObject CreatePanel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            return go;
        }

        private void BuildHomeScreen(Transform parent)
        {
            // Background cosmic starfield tint
            var bgGo = CreateRect(parent, "SpaceBackground", Vector2.zero, Vector2.one);
            var bgImg = bgGo.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.07f, 0.18f, 1f); // Deep indigo space

            // Decorative Floating Space Debris (3D Emojis)
            CreateEmojiImage(bgGo, "emoji_3d_star", new Vector2(28f, 28f), new Vector2(0.12f, 0.82f), new Vector2(0.12f, 0.82f), Vector2.zero);
            CreateEmojiImage(bgGo, "emoji_3d_sparkles", new Vector2(32f, 32f), new Vector2(0.85f, 0.78f), new Vector2(0.85f, 0.78f), Vector2.zero);
            CreateEmojiImage(bgGo, "emoji_3d_coin", new Vector2(24f, 24f), new Vector2(0.15f, 0.35f), new Vector2(0.15f, 0.35f), Vector2.zero);
            CreateEmojiImage(bgGo, "emoji_3d_gear", new Vector2(26f, 26f), new Vector2(0.84f, 0.38f), new Vector2(0.84f, 0.38f), Vector2.zero);

            // Top Bar
            var topBar = CreateRect(parent, "TopBar", new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.99f));
            
            // Coins Pill (3D Coin Sprite + Text)
            var coinsBox = CreatePill(topBar, "CoinsPill", new Vector2(0f, 0.1f), new Vector2(0.38f, 0.9f), new Color(0.14f, 0.16f, 0.32f, 0.92f));
            CreateEmojiImage(coinsBox, "emoji_3d_coin", new Vector2(22f, 22f), new Vector2(0.08f, 0.5f), new Vector2(0.08f, 0.5f), Vector2.zero);
            homeCreditsText = CreateText(coinsBox, "250", 17, new Color(0.98f, 0.8f, 0.1f, 1f), TextAlignmentOptions.Left);
            var hRt = homeCreditsText.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0.35f, 0f);
            hRt.anchorMax = new Vector2(0.95f, 1f);
            hRt.sizeDelta = Vector2.zero;

            // Audio Button (3D Sound Sprite)
            var soundBtn = CreateCircularButton(topBar, "Btn_Sound", "emoji_3d_sound_on", new Vector2(0.72f, 0.1f), new Vector2(0.83f, 0.9f), () => {
                AudioManager.Instance?.PlayButtonClick();
            });

            // Settings Button (3D Gear Sprite)
            var setBtn = CreateCircularButton(topBar, "Btn_Settings", "emoji_3d_gear", new Vector2(0.87f, 0.1f), new Vector2(0.98f, 0.9f), () => {
                ShowSettings();
            });

            // 3D Orbital Salvage Logo Header
            var titleBox = CreateRect(parent, "TitleBox", new Vector2(0.05f, 0.77f), new Vector2(0.95f, 0.91f));
            var title1 = CreateText(titleBox, "ORBITAL", 40, new Color(0.98f, 0.8f, 0.08f, 1f), TextAlignmentOptions.Center);
            title1.fontStyle = FontStyles.Bold;
            var subText = CreateText(titleBox, "SALVAGE • SPACE TRASH CLEAN", 13, new Color(0.22f, 0.74f, 0.97f, 0.95f), TextAlignmentOptions.Center);
            var subRt = subText.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 0f);
            subRt.anchorMax = new Vector2(1f, 0.32f);

            // Mascot Center Diorama (Holographic Pedestal + 3D Donut Mascot)
            var diorama = CreateRect(parent, "MascotDiorama", new Vector2(0.12f, 0.40f), new Vector2(0.88f, 0.74f));
            
            // Pedestal glowing disc
            var pedestal = CreatePill(diorama, "PedestalDisc", new Vector2(0.15f, 0.04f), new Vector2(0.85f, 0.22f), new Color(0.25f, 0.22f, 0.6f, 0.6f));
            var pGlow = CreatePill(pedestal, "GlowRim", new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.8f), new Color(0.35f, 0.75f, 1f, 0.45f));

            // Floating 3D Donut Mascot with Nom-Nom face
            var donutGo = CreateEmojiImage(diorama, "emoji_3d_donut", new Vector2(115f, 115f), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), Vector2.zero);
            var faceGo = CreateEmojiImage(donutGo.transform, "emoji_3d_nom_nom", new Vector2(58f, 58f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);

            var mascotBtn = diorama.gameObject.AddComponent<Button>();
            mascotBtn.onClick.AddListener(() => {
                AudioManager.Instance?.PlaySwallow();
            });

            // Launch Console (Mission Deck + Play Mission Button)
            var consoleBox = CreateRect(parent, "LaunchConsole", new Vector2(0.06f, 0.13f), new Vector2(0.94f, 0.38f));
            var consoleBg = consoleBox.gameObject.AddComponent<Image>();
            consoleBg.color = new Color(0.12f, 0.14f, 0.30f, 0.95f);

            // Sector badge & Goal
            var badgeGo = CreatePill(consoleBox, "SectorBadge", new Vector2(0.06f, 0.75f), new Vector2(0.44f, 0.92f), new Color(0.22f, 0.74f, 0.97f, 0.25f));
            var bTxt = CreateText(badgeGo, "SECTOR World 1", 12, new Color(0.22f, 0.74f, 0.97f, 1f), TextAlignmentOptions.Center);
            bTxt.fontStyle = FontStyles.Bold;

            var goalPill = CreatePill(consoleBox, "GoalPill", new Vector2(0.56f, 0.75f), new Vector2(0.94f, 0.92f), new Color(0.98f, 0.8f, 0.1f, 0.2f));
            var gTxt = CreateText(goalPill, "Goal: 20 kg", 12, new Color(0.98f, 0.8f, 0.1f, 1f), TextAlignmentOptions.Center);

            // Level title
            var lvlRow = CreateRect(consoleBox, "LevelRow", new Vector2(0.06f, 0.50f), new Vector2(0.94f, 0.72f));
            CreateEmojiImage(lvlRow, "emoji_3d_star", new Vector2(22f, 22f), new Vector2(0.05f, 0.5f), new Vector2(0.05f, 0.5f), Vector2.zero);
            var lvlText = CreateText(lvlRow, "LVL 1 • Bubblegum Chute 1", 17, Color.white, TextAlignmentOptions.Left);
            lvlText.fontStyle = FontStyles.Bold;
            var lRt = lvlText.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0.18f, 0f);
            lRt.anchorMax = new Vector2(0.98f, 1f);

            // Big 3D Play Button (Gold / Amber with 3D Bevel)
            var playBtnGo = CreatePill(consoleBox, "Btn_PlayMission", new Vector2(0.06f, 0.10f), new Vector2(0.94f, 0.44f), new Color(0.98f, 0.75f, 0.08f, 1f));
            var pBtn = playBtnGo.gameObject.AddComponent<Button>();
            pBtn.onClick.AddListener(() => {
                StartSector(1);
            });

            // Play Triangle Icon + Label
            CreateEmojiImage(playBtnGo, "emoji_3d_rocket", new Vector2(26f, 26f), new Vector2(0.18f, 0.5f), new Vector2(0.18f, 0.5f), Vector2.zero);
            var pTxt = CreateText(playBtnGo, "PLAY MISSION", 19, new Color(0.12f, 0.1f, 0.28f, 1f), TextAlignmentOptions.Center);
            pTxt.fontStyle = FontStyles.Bold;
            var ptRt = pTxt.GetComponent<RectTransform>();
            ptRt.anchorMin = new Vector2(0.28f, 0f);
            ptRt.anchorMax = new Vector2(0.88f, 1f);

            // Bottom Arcade Dock Navigation (4 Tabs with 3D Emojis)
            var dock = CreatePill(parent, "ArcadeDock", new Vector2(0.04f, 0.015f), new Vector2(0.96f, 0.105f), new Color(0.1f, 0.12f, 0.25f, 0.98f));
            CreateDockTab(dock, "MAPS", "emoji_3d_map", 0f, 0.25f, null, () => ShowMaps());
            CreateDockTab(dock, "DAILY", "emoji_3d_calendar", 0.25f, 0.50f, "HOT", () => ShowDaily());
            CreateDockTab(dock, "SHOP", "emoji_3d_gift", 0.50f, 0.75f, "FREE", () => ShowShop());
            CreateDockTab(dock, "TROPHIES", "emoji_3d_trophy", 0.75f, 1.0f, null, () => ShowTrophies());
        }

        private void BuildMapsScreen(Transform parent)
        {
            var header = CreateHeaderBar(parent, "CLEANUP MAPS (50 LVLS)", () => ShowHome());
            
            // Scroll Area with 50 level cards
            var scrollGo = CreateRect(parent, "MapsScroll", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.90f));
            var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(scrollGo, false);
            var gridRt = gridGo.GetComponent<RectTransform>();
            gridRt.anchorMin = Vector2.zero;
            gridRt.anchorMax = Vector2.one;
            gridRt.sizeDelta = Vector2.zero;

            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(74f, 74f);
            grid.spacing = new Vector2(8f, 8f);
            grid.childAlignment = TextAnchor.UpperCenter;

            for (int i = 1; i <= 50; i++)
            {
                int lvl = i;
                var card = CreatePill(gridGo.transform, $"Lvl_{lvl}", Vector2.zero, Vector2.one, new Color(0.15f, 0.18f, 0.38f, 0.95f));
                var btn = card.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => StartSector(lvl));

                CreateEmojiImage(card, "emoji_3d_star", new Vector2(18f, 18f), new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero);
                var txt = CreateText(card, $"{lvl}", 16, Color.white, TextAlignmentOptions.Center);
                var tRt = txt.GetComponent<RectTransform>();
                tRt.anchorMin = new Vector2(0f, 0.08f);
                tRt.anchorMax = new Vector2(1f, 0.50f);
                txt.fontStyle = FontStyles.Bold;
            }
        }

        private void BuildDailyScreen(Transform parent)
        {
            CreateHeaderBar(parent, "DAILY OPERATIONS", () => ShowHome());

            var content = CreateRect(parent, "DailyContent", new Vector2(0.06f, 0.1f), new Vector2(0.94f, 0.90f));
            CreateCardWithEmoji(content, "Mission 1: Space Sweeper", "Vacuum 30 titanium scrap", "emoji_3d_broom", "Claim +50", new Vector2(0f, 0.68f), new Vector2(1f, 0.96f));
            CreateCardWithEmoji(content, "Mission 2: Multiplier Master", "Trigger 5 gate duplicates", "emoji_3d_lightning", "Claim +80", new Vector2(0f, 0.36f), new Vector2(1f, 0.64f));
            CreateCardWithEmoji(content, "Mission 3: Sector Dominance", "Clear any sector with 3 stars", "emoji_3d_crown", "Claim +120", new Vector2(0f, 0.04f), new Vector2(1f, 0.32f));
        }

        private void BuildShopScreen(Transform parent)
        {
            CreateHeaderBar(parent, "TOY SHOP & HATS", () => ShowHome());

            var content = CreateRect(parent, "ShopContent", new Vector2(0.06f, 0.1f), new Vector2(0.94f, 0.90f));
            CreateCardWithEmoji(content, "Free Daily Allowance", "+100 Recycling Coins daily", "emoji_3d_gift", "CLAIM FREE", new Vector2(0f, 0.70f), new Vector2(1f, 0.96f));
            CreateCardWithEmoji(content, "Donut Float Ring", "Sweet pink glazed centrifuge", "emoji_3d_donut", "EQUIPPED", new Vector2(0f, 0.40f), new Vector2(1f, 0.66f));
            CreateCardWithEmoji(content, "Royal Crown Nibbles", "Majestic golden king style", "emoji_3d_crown", "EQUIP", new Vector2(0f, 0.10f), new Vector2(1f, 0.36f));
        }

        private void BuildTrophiesScreen(Transform parent)
        {
            CreateHeaderBar(parent, "TROPHY ROOM", () => ShowHome());

            var content = CreateRect(parent, "TrophiesContent", new Vector2(0.06f, 0.1f), new Vector2(0.94f, 0.90f));
            CreateCardWithEmoji(content, "First Crunch", "Feed Nibbles 10 scrap pieces", "emoji_3d_nom_nom", "CLAIMED", new Vector2(0f, 0.68f), new Vector2(1f, 0.96f));
            CreateCardWithEmoji(content, "Centrifuge Apprentice", "Clear 10 sectors with 3 stars", "emoji_3d_star", "6 / 10", new Vector2(0f, 0.36f), new Vector2(1f, 0.64f));
            CreateCardWithEmoji(content, "Cosmic Janitor Supreme", "Complete all 50 orbital sectors", "emoji_3d_trophy", "1 / 50", new Vector2(0f, 0.04f), new Vector2(1f, 0.32f));
        }

        private void BuildSettingsScreen(Transform parent)
        {
            CreateHeaderBar(parent, "GAME SETTINGS", () => ShowHome());

            var content = CreateRect(parent, "SettingsContent", new Vector2(0.06f, 0.15f), new Vector2(0.94f, 0.90f));
            CreateCardWithEmoji(content, "Sound Effects", "Master game audio volume", "emoji_3d_sound_on", "ENABLED", new Vector2(0f, 0.68f), new Vector2(1f, 0.96f));
            CreateCardWithEmoji(content, "Haptic Vibration", "Tactile rotation ratchet feedback", "emoji_3d_gear", "ENABLED", new Vector2(0f, 0.36f), new Vector2(1f, 0.64f));
            CreateCardWithEmoji(content, "Offline & Kid-Safe", "100% offline • No data tracking", "emoji_3d_locked", "VERIFIED", new Vector2(0f, 0.04f), new Vector2(1f, 0.32f));
        }

        private void BuildGameplayHud(Transform parent)
        {
            // Top Bar
            var topBar = CreateRect(parent, "TopBar", new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.99f));

            // Circular Pause Button (with 3D Pause Sprite)
            CreateCircularButton(topBar, "Btn_Pause", "emoji_3d_pause", new Vector2(0f, 0.05f), new Vector2(0.14f, 0.95f), () => TogglePause());

            // Central Unified Capsule (Star Sprite + Level + Quota Bar + Progress)
            var capsule = CreatePill(topBar, "UnifiedCapsule", new Vector2(0.18f, 0.05f), new Vector2(0.82f, 0.95f), new Color(0.1f, 0.13f, 0.26f, 0.95f));
            
            // Star icon
            CreateEmojiImage(capsule, "emoji_3d_star", new Vector2(20f, 20f), new Vector2(0.06f, 0.5f), new Vector2(0.06f, 0.5f), Vector2.zero);
            
            hudLevelBadge = CreateText(capsule, "LVL 1", 15, new Color(0.98f, 0.8f, 0.1f, 1f), TextAlignmentOptions.Left);
            hudLevelBadge.fontStyle = FontStyles.Bold;
            var lvlRt = hudLevelBadge.GetComponent<RectTransform>();
            lvlRt.anchorMin = new Vector2(0.18f, 0f);
            lvlRt.anchorMax = new Vector2(0.48f, 1f);

            hudQuotaText = CreateText(capsule, "0 / 20 kg", 14, Color.white, TextAlignmentOptions.Right);
            var qRt = hudQuotaText.GetComponent<RectTransform>();
            qRt.anchorMin = new Vector2(0.52f, 0f);
            qRt.anchorMax = new Vector2(0.94f, 1f);

            // Circular Restart Button (with 3D Retry Sprite)
            CreateCircularButton(topBar, "Btn_Restart", "emoji_3d_retry", new Vector2(0.86f, 0.05f), new Vector2(1f, 0.95f), () => {
                if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber);
            });

            // Bottom Tutorial Prompt
            var hint = CreateText(parent, "SWIPE TO SPIN CENTRIFUGE", 13, new Color(0.6f, 0.75f, 0.95f, 0.85f), TextAlignmentOptions.Center);
            hint.fontStyle = FontStyles.Bold;
            var hRt = hint.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0.05f, 0.02f);
            hRt.anchorMax = new Vector2(0.95f, 0.06f);
        }

        private void BuildModals(Transform parent)
        {
            // Pause Modal
            pauseModal = CreateModal(parent, "Modal_Pause", "MISSION PAUSED", "emoji_3d_pause");
            CreateButton(pauseModal.transform, "Btn_Resume", "RESUME", new Vector2(0.15f, 0.52f), new Vector2(0.85f, 0.62f), () => TogglePause());
            CreateButton(pauseModal.transform, "Btn_Restart", "RESTART", new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.48f), () => {
                TogglePause();
                if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber);
            });
            CreateButton(pauseModal.transform, "Btn_Hangar", "HANGAR", new Vector2(0.15f, 0.24f), new Vector2(0.85f, 0.34f), () => {
                TogglePause();
                ShowHome();
            });

            // Victory Modal
            victoryModal = CreateModal(parent, "Modal_Victory", "NIBBLES IS FULL! YAY!", "emoji_3d_party_popper");
            
            // 3D Star Row
            var starsRow = CreateRect(victoryModal.transform, "StarsRow", new Vector2(0.15f, 0.58f), new Vector2(0.85f, 0.72f));
            CreateEmojiImage(starsRow, "emoji_3d_star", new Vector2(48f, 48f), new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f), Vector2.zero);
            CreateEmojiImage(starsRow, "emoji_3d_star", new Vector2(58f, 58f), new Vector2(0.50f, 0.5f), new Vector2(0.50f, 0.5f), Vector2.zero);
            CreateEmojiImage(starsRow, "emoji_3d_star", new Vector2(48f, 48f), new Vector2(0.75f, 0.5f), new Vector2(0.75f, 0.5f), Vector2.zero);

            victoryStatsText = CreateText(victoryModal.transform, "The space orbit is sparkling clean!", 16, Color.white, TextAlignmentOptions.Center);
            var vStatRt = victoryStatsText.GetComponent<RectTransform>();
            vStatRt.anchorMin = new Vector2(0.1f, 0.44f);
            vStatRt.anchorMax = new Vector2(0.9f, 0.54f);

            CreateButton(victoryModal.transform, "Btn_NextSector", "NEXT SECTOR >", new Vector2(0.15f, 0.28f), new Vector2(0.85f, 0.40f), () => {
                victoryModal.SetActive(false);
                if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber + 1);
            });
            CreateButton(victoryModal.transform, "Btn_VictoryHangar", "HANGAR", new Vector2(0.15f, 0.14f), new Vector2(0.85f, 0.24f), () => {
                victoryModal.SetActive(false);
                ShowHome();
            });

            // Defeat Modal
            defeatModal = CreateModal(parent, "Modal_Defeat", "OUT OF SCRAP!", "emoji_3d_pleading_face");
            defeatStatsText = CreateText(defeatModal.transform, "Nibbles is still hungry!", 16, Color.white, TextAlignmentOptions.Center);
            var defStatRt = defeatStatsText.GetComponent<RectTransform>();
            defStatRt.anchorMin = new Vector2(0.1f, 0.50f);
            defStatRt.anchorMax = new Vector2(0.9f, 0.65f);

            CreateButton(defeatModal.transform, "Btn_BackupPod", "BACKUP POD (+8)", new Vector2(0.15f, 0.34f), new Vector2(0.85f, 0.46f), () => {
                defeatModal.SetActive(false);
                GameManager.Instance?.LaunchBackupPod();
            });
            CreateButton(defeatModal.transform, "Btn_Retry", "TRY AGAIN", new Vector2(0.15f, 0.20f), new Vector2(0.85f, 0.30f), () => {
                defeatModal.SetActive(false);
                if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber);
            });

            pauseModal.SetActive(false);
            victoryModal.SetActive(false);
            defeatModal.SetActive(false);
        }
        #endregion

        #region Helper Factories (100% Sprite Driven)
        private Transform CreateHeaderBar(Transform parent, string title, Action onBack)
        {
            var header = CreateRect(parent, "HeaderBar", new Vector2(0.04f, 0.93f), new Vector2(0.96f, 0.99f));
            CreateButton(header, "Btn_Back", "< BACK", new Vector2(0f, 0.1f), new Vector2(0.24f, 0.9f), onBack);
            var titleTxt = CreateText(header, title, 18, Color.white, TextAlignmentOptions.Center);
            titleTxt.fontStyle = FontStyles.Bold;
            return header;
        }

        private Transform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = Vector2.zero;
            return go.transform;
        }

        private Transform CreatePill(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color col)
        {
            var tr = CreateRect(parent, name, anchorMin, anchorMax);
            var img = tr.gameObject.AddComponent<Image>();
            img.color = col;
            return tr;
        }

        private GameObject CreateCircularButton(Transform parent, string name, string emojiName, Vector2 anchorMin, Vector2 anchorMax, Action onClick)
        {
            var tr = CreatePill(parent, name, anchorMin, anchorMax, new Color(0.15f, 0.18f, 0.34f, 0.95f));
            var btn = tr.gameObject.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            CreateEmojiImage(tr, emojiName, new Vector2(22f, 22f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
            return tr.gameObject;
        }

        private void CreateDockTab(Transform parent, string label, string emojiName, float xMin, float xMax, string badge, Action onClick)
        {
            var tr = CreateRect(parent, $"Tab_{label}", new Vector2(xMin, 0f), new Vector2(xMax, 1f));
            var btn = tr.gameObject.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            // 3D Emoji Icon
            CreateEmojiImage(tr, emojiName, new Vector2(24f, 24f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero);

            // Label
            var txt = CreateText(tr, label, 11, Color.white, TextAlignmentOptions.Center);
            txt.fontStyle = FontStyles.Bold;
            var tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0.08f);
            tRt.anchorMax = new Vector2(1f, 0.38f);

            // Badge if specified
            if (!string.IsNullOrEmpty(badge))
            {
                var badgePill = CreatePill(tr, "Badge", new Vector2(0.55f, 0.65f), new Vector2(0.92f, 0.95f), badge == "HOT" ? new Color(0.95f, 0.25f, 0.25f, 1f) : new Color(0.25f, 0.85f, 0.4f, 1f));
                var bTxt = CreateText(badgePill, badge, 8, Color.white, TextAlignmentOptions.Center);
                bTxt.fontStyle = FontStyles.Bold;
            }
        }

        private void CreateCardWithEmoji(Transform parent, string title, string sub, string emojiName, string actionLabel, Vector2 anchorMin, Vector2 anchorMax)
        {
            var card = CreatePill(parent, title, anchorMin, anchorMax, new Color(0.12f, 0.15f, 0.32f, 0.95f));

            CreateEmojiImage(card, emojiName, new Vector2(34f, 34f), new Vector2(0.08f, 0.5f), new Vector2(0.08f, 0.5f), Vector2.zero);

            var t = CreateText(card, title, 16, Color.white, TextAlignmentOptions.Left);
            t.fontStyle = FontStyles.Bold;
            var tRt = t.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.20f, 0.52f);
            tRt.anchorMax = new Vector2(0.68f, 0.92f);

            var s = CreateText(card, sub, 12, new Color(0.6f, 0.75f, 0.95f, 0.85f), TextAlignmentOptions.Left);
            var sRt = s.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.20f, 0.12f);
            sRt.anchorMax = new Vector2(0.68f, 0.52f);

            CreateButton(card, "CardAction", actionLabel, new Vector2(0.70f, 0.22f), new Vector2(0.96f, 0.78f), () => {
                AudioManager.Instance?.PlayButtonClick();
            });
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Action onClick)
        {
            var tr = CreatePill(parent, name, anchorMin, anchorMax, new Color(0.22f, 0.32f, 0.62f, 0.95f));
            var btn = tr.gameObject.AddComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var txt = CreateText(tr, label, 15, Color.white, TextAlignmentOptions.Center);
            txt.fontStyle = FontStyles.Bold;
            return btn;
        }

        private GameObject CreateModal(Transform parent, string name, string title, string headerEmoji)
        {
            var overlay = CreatePill(parent, name, Vector2.zero, Vector2.one, new Color(0.03f, 0.05f, 0.12f, 0.88f));
            var card = CreatePill(overlay, "ModalCard", new Vector2(0.08f, 0.18f), new Vector2(0.92f, 0.82f), new Color(0.12f, 0.14f, 0.28f, 0.98f));

            if (!string.IsNullOrEmpty(headerEmoji))
            {
                CreateEmojiImage(card, headerEmoji, new Vector2(40f, 40f), new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), Vector2.zero);
            }

            var header = CreateText(card, title, 20, new Color(0.98f, 0.8f, 0.1f, 1f), TextAlignmentOptions.Center);
            header.fontStyle = FontStyles.Bold;
            var hRt = header.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0.05f, 0.76f);
            hRt.anchorMax = new Vector2(0.95f, 0.86f);

            return overlay.gameObject;
        }

        private GameObject CreateEmojiImage(Transform parent, string emojiName, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
        {
            var go = new GameObject($"Emoji_{emojiName}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.sprite = SpriteHelper.GetEmojiSprite(emojiName);
            img.preserveAspect = true;
            return go;
        }

        private TextMeshProUGUI CreateText(Transform parent, string text, float size, Color color, TextAlignmentOptions align)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            return tmp;
        }
        #endregion
    }
}
