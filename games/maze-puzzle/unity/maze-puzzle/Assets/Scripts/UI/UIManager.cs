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
    /// Features true 9-slice rounded corners, 3D extruded cartoon bevels,
    /// tactile button press feedback, and authentic Brawl Stars / Fall Guys visual polish.
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
        [SerializeField] private GameObject loadingPanel;

        private GameObject shopRingsContainer;
        private GameObject shopHatsContainer;
        private GameObject shopSkiesContainer;
        private Transform shopTabRings;
        private Transform shopTabHats;
        private Transform shopTabSkies;

        [Header("Modals")]
        [SerializeField] private GameObject pauseModal;
        [SerializeField] private GameObject victoryModal;
        [SerializeField] private GameObject defeatModal;

        [Header("In-Game HUD References")]
        [SerializeField] private TextMeshProUGUI hudLevelBadge;
        [SerializeField] private TextMeshProUGUI hudQuotaText;
        [SerializeField] private RectTransform hudQuotaBarFillRect;
        [SerializeField] private TextMeshProUGUI homeCreditsText;
        [SerializeField] private TextMeshProUGUI mapsCreditsText;

        [Header("Debrief References")]
        [SerializeField] private TextMeshProUGUI victoryStatsText;
        [SerializeField] private TextMeshProUGUI defeatStatsText;
        [SerializeField] private TextMeshProUGUI victoryMassText;
        [SerializeField] private TextMeshProUGUI victoryCreditsText;
        [SerializeField] private TextMeshProUGUI defeatMassText;
        [SerializeField] private TextMeshProUGUI defeatLostText;
        [SerializeField] private GameObject defeatBackupButton;
        [SerializeField] private RectTransform defeatHangarRt;
        [SerializeField] private RectTransform defeatTryAgainRt;

        private readonly List<TextMeshProUGUI> headerCreditsTexts = new List<TextMeshProUGUI>();
        private int lastDisplayedCredits = int.MinValue;
        private string lastQuotaLabel;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            GameManager.OnStateChanged -= HandleGameStateChanged;
            GameManager.OnStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleGameStateChanged;
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
            if (!force && homePanel != null && transform.Find("AppRoot") != null)
            {
                return;
            }

            homePanel = null;
            mapsPanel = null;
            dailyPanel = null;
            shopPanel = null;
            trophiesPanel = null;
            settingsPanel = null;
            inGameHudPanel = null;
            loadingPanel = null;
            pauseModal = null;
            victoryModal = null;
            defeatModal = null;
            victoryMassText = null;
            victoryCreditsText = null;
            defeatMassText = null;
            defeatLostText = null;
            defeatBackupButton = null;
            defeatHangarRt = null;
            defeatTryAgainRt = null;
            hudQuotaBarFillRect = null;
            headerCreditsTexts.Clear();
            lastDisplayedCredits = int.MinValue;
            lastQuotaLabel = null;

            BuildCompleteUIHierarchy();
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (this == null || !Application.isPlaying) return;

            switch (state)
            {
                case GameState.VictoryDebrief:
                    PopulateVictoryModalRuntime();
                    ShowModalVictory(true);
                    break;
                case GameState.DefeatDebrief:
                    PopulateDefeatModalRuntime();
                    ShowModalDefeat(true);
                    break;
                default:
                    if (victoryModal != null) victoryModal.SetActive(false);
                    if (defeatModal != null) defeatModal.SetActive(false);
                    break;
            }
        }

        private void PopulateVictoryModalRuntime()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (victoryMassText != null) victoryMassText.text = $"{gm.SwallowedDebrisCount} kg";
            if (victoryCreditsText != null) victoryCreditsText.text = $"+{gm.LastCreditsEarned}";
        }

        private void PopulateDefeatModalRuntime()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (defeatMassText != null) defeatMassText.text = $"{gm.SwallowedDebrisCount} / {gm.CurrentQuota} kg";
            if (defeatLostText != null) defeatLostText.text = $"{gm.MassLost} kg";

            bool eligible = gm.IsBackupPodEligible;
            if (defeatBackupButton != null) defeatBackupButton.SetActive(eligible);
            ApplyDefeatActionLayout(eligible);
        }

        private void ApplyDefeatActionLayout(bool backupVisible)
        {
            if (defeatHangarRt == null || defeatTryAgainRt == null) return;
            if (backupVisible)
            {
                defeatHangarRt.anchorMin = new Vector2(0.05f, 0.04f);
                defeatHangarRt.anchorMax = new Vector2(0.48f, 0.18f);
                defeatTryAgainRt.anchorMin = new Vector2(0.52f, 0.04f);
                defeatTryAgainRt.anchorMax = new Vector2(0.95f, 0.18f);
            }
            else
            {
                defeatHangarRt.anchorMin = new Vector2(0.05f, 0.06f);
                defeatHangarRt.anchorMax = new Vector2(0.48f, 0.20f);
                defeatTryAgainRt.anchorMin = new Vector2(0.52f, 0.06f);
                defeatTryAgainRt.anchorMax = new Vector2(0.95f, 0.20f);
            }
        }

        private void UpdateHudData()
        {
            int credits = EconomyManager.Instance != null ? EconomyManager.Instance.SaveData.scrapCredits : 250;
            if (credits != lastDisplayedCredits)
            {
                lastDisplayedCredits = credits;
                string creditsLabel = $"{credits}";
                for (int i = 0; i < headerCreditsTexts.Count; i++)
                {
                    if (headerCreditsTexts[i] != null) headerCreditsTexts[i].text = creditsLabel;
                }
            }

            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            if (hudLevelBadge != null)
            {
                hudLevelBadge.text = $"LVL {GameManager.Instance.CurrentLevelNumber}";
            }

            string quotaLabel = $"{GameManager.Instance.SwallowedDebrisCount} / {GameManager.Instance.CurrentQuota} kg";
            if (hudQuotaText != null && quotaLabel != lastQuotaLabel)
            {
                lastQuotaLabel = quotaLabel;
                hudQuotaText.text = quotaLabel;
            }

            if (hudQuotaBarFillRect != null)
            {
                float p = (float)GameManager.Instance.SwallowedDebrisCount / Mathf.Max(1, GameManager.Instance.CurrentQuota);
                p = Mathf.Clamp01(p);
                var max = hudQuotaBarFillRect.anchorMax;
                if (!Mathf.Approximately(max.x, p))
                {
                    hudQuotaBarFillRect.anchorMax = new Vector2(p, max.y);
                    hudQuotaBarFillRect.offsetMin = Vector2.zero;
                    hudQuotaBarFillRect.offsetMax = Vector2.zero;
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

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == "CentrifugeRoot" ||
                        root.name == "ContainmentFunnel" ||
                        root.name == "NibblesMascot" ||
                        root.name == "DebrisContainer" ||
                        root.name == "CosmicBackground")
                    {
                        root.SetActive(visible);
                    }
                }
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
            }
        }

        public void ShowMaps()
        {
            EnsureHierarchyBuilt();
            SetPage(mapsPanel);
            SetWorldVisibility(false);
        }

        public void ShowDaily()
        {
            EnsureHierarchyBuilt();
            SetPage(dailyPanel);
            SetWorldVisibility(false);
        }

        public void ShowShop(int tabIndex = 0)
        {
            EnsureHierarchyBuilt();
            SetPage(shopPanel);
            SetWorldVisibility(false);
            SetShopTab(tabIndex);
        }

        public void ShowShopDonuts() => ShowShop(0);
        public void ShowShopHats() => ShowShop(1);
        public void ShowShopSkies() => ShowShop(2);

        public void ShowLoading()
        {
            EnsureHierarchyBuilt();
            SetPage(loadingPanel);
            SetWorldVisibility(false);
        }

        public void SetShopTab(int tabIndex)
        {
            if (shopRingsContainer != null) shopRingsContainer.SetActive(tabIndex == 0);
            if (shopHatsContainer != null) shopHatsContainer.SetActive(tabIndex == 1);
            if (shopSkiesContainer != null) shopSkiesContainer.SetActive(tabIndex == 2);

            UpdateTabVisual(shopTabRings, tabIndex == 0);
            UpdateTabVisual(shopTabHats, tabIndex == 1);
            UpdateTabVisual(shopTabSkies, tabIndex == 2);
        }

        private void UpdateTabVisual(Transform tabTr, bool active)
        {
            if (tabTr == null) return;
            var img = tabTr.GetComponent<Image>();
            if (img != null)
            {
                Color fill = active ? SpriteHelper.ColorYellowMain : new Color32(0x31, 0x2e, 0x81, 0xcc);
                Color border = active ? SpriteHelper.ColorBorderDark : new Color32(0x63, 0x66, 0xf1, 0x44);
                img.sprite = SpriteHelper.GetPillBadge(fill, border, 1.8f);
            }
            var txt = tabTr.Find("TabText")?.GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.color = active ? SpriteHelper.ColorBorderDark : Color.white;
            }
        }

        public void ShowTrophies()
        {
            EnsureHierarchyBuilt();
            SetPage(trophiesPanel);
            SetWorldVisibility(false);
        }

        public void ShowSettings()
        {
            EnsureHierarchyBuilt();
            SetPage(settingsPanel);
            SetWorldVisibility(false);
        }

        public void ShowGameplay()
        {
            EnsureHierarchyBuilt();
            SetPage(inGameHudPanel);
            SetWorldVisibility(true);
        }

        private void SetPage(GameObject target)
        {
            if (homePanel != null) homePanel.SetActive(homePanel == target);
            if (mapsPanel != null) mapsPanel.SetActive(mapsPanel == target);
            if (dailyPanel != null) dailyPanel.SetActive(dailyPanel == target);
            if (shopPanel != null) shopPanel.SetActive(shopPanel == target);
            if (trophiesPanel != null) trophiesPanel.SetActive(trophiesPanel == target);
            if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == target);
            if (inGameHudPanel != null) inGameHudPanel.SetActive(inGameHudPanel == target);
            if (loadingPanel != null) loadingPanel.SetActive(loadingPanel == target);

            HideAllModals();
        }

        public void HideAllModals()
        {
            if (pauseModal != null) pauseModal.SetActive(false);
            if (victoryModal != null) victoryModal.SetActive(false);
            if (defeatModal != null) defeatModal.SetActive(false);
        }

        public void HideAllPages()
        {
            SetPage(null);
            SetWorldVisibility(true);
        }

        public void ShowModalPause(bool show)
        {
            if (pauseModal != null) pauseModal.SetActive(show);
        }

        public void ShowModalVictory(bool show)
        {
            if (victoryModal != null) victoryModal.SetActive(show);
        }

        public void ShowModalDefeat(bool show)
        {
            if (defeatModal != null) defeatModal.SetActive(show);
        }

        public void TogglePause()
        {
            if (pauseModal != null)
            {
                bool active = !pauseModal.activeSelf;
                pauseModal.SetActive(active);
                Time.timeScale = active ? 0f : 1f;
            }
        }

        public void StartSector(int sectorNumber)
        {
            ShowGameplay();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartSector(sectorNumber);
            }
        }
        #endregion

        #region Hierarchy Construction (1:1 Web Prototype Replica)
        private void BuildCompleteUIHierarchy()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(430, 932);
            scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Purge ALL existing children of Canvas to eliminate legacy scene hierarchy (SafeAreaPanel, old buttons, etc.)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }

            var appRoot = new GameObject("AppRoot", typeof(RectTransform));
            appRoot.transform.SetParent(transform, false);
            var rootRt = appRoot.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.sizeDelta = Vector2.zero;

            homePanel = CreatePage(appRoot.transform, "Page_Home");
            mapsPanel = CreatePage(appRoot.transform, "Page_Maps");
            dailyPanel = CreatePage(appRoot.transform, "Page_Daily");
            shopPanel = CreatePage(appRoot.transform, "Page_Shop");
            trophiesPanel = CreatePage(appRoot.transform, "Page_Trophies");
            settingsPanel = CreatePage(appRoot.transform, "Page_Settings");
            inGameHudPanel = CreatePage(appRoot.transform, "Page_InGameHUD");
            loadingPanel = CreatePage(appRoot.transform, "Page_Loading");

            // Page backgrounds stay full-bleed on the page root; interactive UI builds into SafeContent.
            BuildHomeScreen(GetPageContent(homePanel));
            BuildMapsScreen(GetPageContent(mapsPanel));
            BuildDailyScreen(GetPageContent(dailyPanel));
            BuildShopScreen(GetPageContent(shopPanel));
            BuildTrophiesScreen(GetPageContent(trophiesPanel));
            BuildSettingsScreen(GetPageContent(settingsPanel));
            BuildGameplayHud(GetPageContent(inGameHudPanel));
            BuildLoadingScreen(GetPageContent(loadingPanel));

            BuildModals(appRoot.transform);

            SetPage(homePanel);
            SetWorldVisibility(false);
        }

        private Transform GetPageContent(GameObject page)
        {
            if (page == null) return null;
            var safe = page.transform.Find("SafeContent");
            return safe != null ? safe : page.transform;
        }

        private GameObject CreatePage(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Give non-HUD pages an opaque cosmic dark background (#131133) matching Web prototype
            if (name != "Page_InGameHUD")
            {
                var bg = CreateSlicedCard(go.transform, "PageBackground", Vector2.zero, Vector2.one,
                    new Color32(0x13, 0x11, 0x33, 0xff), Color.clear, 0f, 0f);
                bg.transform.SetAsFirstSibling();
            }

            var safeGo = new GameObject("SafeContent", typeof(RectTransform));
            safeGo.transform.SetParent(go.transform, false);
            var safeRt = safeGo.GetComponent<RectTransform>();
            safeRt.anchorMin = Vector2.zero;
            safeRt.anchorMax = Vector2.one;
            safeRt.sizeDelta = Vector2.zero;
            safeGo.AddComponent<SafeAreaFitter>();

            return go;
        }

        #region Screen 1: Home / Hangar Screen
        private void BuildHomeScreen(Transform parent)
        {
            // Background cosmic deep indigo
            var bg = CreateSlicedCard(parent, "SpaceBackground", Vector2.zero, Vector2.one,
                new Color32(0x13, 0x11, 0x33, 0xff), Color.clear, 0f, 0f);

            // Floating Space Parallax Debris (1:1 Web Replica: 🥫, 🔩, ⭐, ✨, 🐟, 🪐)
            CreateEmojiImage(bg, "emoji_3d_gear", new Vector2(24f, 24f), new Vector2(0.12f, 0.65f));
            CreateEmojiImage(bg, "emoji_3d_star", new Vector2(20f, 20f), new Vector2(0.88f, 0.78f));
            CreateEmojiImage(bg, "emoji_3d_sparkles", new Vector2(22f, 22f), new Vector2(0.90f, 0.70f));
            CreateEmojiImage(bg, "emoji_3d_star", new Vector2(16f, 16f), new Vector2(0.14f, 0.38f));
            CreateEmojiImage(bg, "emoji_3d_sparkles", new Vector2(18f, 18f), new Vector2(0.84f, 0.28f));
            CreateEmojiImage(bg, "emoji_3d_milky_way", new Vector2(28f, 28f), new Vector2(0.82f, 0.88f));
            CreateEmojiImage(bg, "emoji_3d_candy", new Vector2(20f, 20f), new Vector2(0.10f, 0.18f));

            // Top Bar
            var topBar = CreateRect(parent, "HomeTopBar", new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.98f));

            // Coin Pill Capsule on Left: dark translucent pill, gold coin icon, bold number
            var coinPill = CreatePill(topBar, "CoinPill", new Vector2(0f, 0.1f), new Vector2(0.36f, 0.9f),
                new Color32(0x1e, 0x1b, 0x4b, 0xdd), new Color32(0x43, 0x38, 0xca, 0x88));
            CreateEmojiImage(coinPill, "emoji_3d_coin", new Vector2(24f, 24f), new Vector2(0.20f, 0.5f));
            homeCreditsText = CreateText(coinPill, "250", 16, Color.white, TextAlignmentOptions.MidlineLeft);
            homeCreditsText.fontStyle = FontStyles.Bold;
            headerCreditsTexts.Add(homeCreditsText);
            var cRt = homeCreditsText.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.42f, 0f);
            cRt.anchorMax = new Vector2(0.95f, 1f);

            // Circular 3D White Discs on Right: Sound & Settings
            CreateCircleButton3D(topBar, "Btn_Sound", "emoji_3d_sound_on", new Vector2(0.72f, 0.05f), new Vector2(0.83f, 0.95f), () => {});
            CreateCircleButton3D(topBar, "Btn_Settings", "emoji_3d_gear", new Vector2(0.86f, 0.05f), new Vector2(0.98f, 0.95f), () => ShowSettings());

            // 3D Extruded Arcade Title
            var titleArea = CreateRect(parent, "TitleArea", new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.90f));
            Create3DTitleText(titleArea, "ORBITAL", 32, Color.white,
                new Color[] {
                    new Color32(0x31, 0x2e, 0x81, 0xff), // #312e81
                    new Color32(0x1e, 0x1b, 0x4b, 0xff), // #1e1b4b
                    new Color32(0x0f, 0x0c, 0x29, 0xff)  // #0f0c29
                },
                new Vector2(0f, 0.54f), new Vector2(1f, 1f));

            Create3DTitleText(titleArea, "SALVAGE", 36, SpriteHelper.ColorYellowMain,
                new Color[] {
                    new Color32(0xca, 0x8a, 0x04, 0xff), // #ca8a04
                    new Color32(0x78, 0x35, 0x0f, 0xff), // #78350f
                    new Color32(0x1e, 0x1b, 0x4b, 0xff)  // #1e1b4b
                },
                new Vector2(0f, 0.16f), new Vector2(1f, 0.62f));

            // Subtitle yellow bordered pill: ✨ SPACE TRASH CLEAN ✨
            var subPill = CreatePill(titleArea, "SubPill", new Vector2(0.18f, 0f), new Vector2(0.82f, 0.22f),
                new Color32(0xfa, 0xcc, 0x15, 0x25), SpriteHelper.ColorYellowMain, 2f);
            CreateEmojiImage(subPill, "emoji_3d_sparkles", new Vector2(14f, 14f), new Vector2(0.08f, 0.5f));
            CreateEmojiImage(subPill, "emoji_3d_sparkles", new Vector2(14f, 14f), new Vector2(0.92f, 0.5f));
            var subTxt = CreateText(subPill, "SPACE TRASH CLEAN", 11.5f, SpriteHelper.ColorYellowMain, TextAlignmentOptions.Center);
            subTxt.fontStyle = FontStyles.Bold;

            // Mascot Central Hero Stage & Pedestal (1:1 Web Diorama Replica)
            var stageArea = CreateRect(parent, "StageArea", new Vector2(0.12f, 0.38f), new Vector2(0.88f, 0.74f));

            // Floor Pedestal Ellipse
            var pedestalGo = new GameObject("PedestalEllipse", typeof(RectTransform), typeof(Image));
            pedestalGo.transform.SetParent(stageArea, false);
            var pedRt = pedestalGo.GetComponent<RectTransform>();
            pedRt.anchorMin = new Vector2(0.18f, 0.03f);
            pedRt.anchorMax = new Vector2(0.82f, 0.28f);
            pedRt.sizeDelta = Vector2.zero;
            var pedImg = pedestalGo.GetComponent<Image>();
            pedImg.sprite = SpriteHelper.GetPedestalRings();

            // Vertical Standing Torus Ring (148x148 with 11px pink frosting border #f472b6)
            var torusGo = new GameObject("DioramaFloaterRing", typeof(RectTransform), typeof(Image));
            torusGo.transform.SetParent(stageArea, false);
            var torusRt = torusGo.GetComponent<RectTransform>();
            torusRt.anchorMin = new Vector2(0.5f, 0.54f);
            torusRt.anchorMax = new Vector2(0.5f, 0.54f);
            torusRt.sizeDelta = new Vector2(148f, 148f);
            var torusImg = torusGo.GetComponent<Image>();
            torusImg.sprite = SpriteHelper.GetFloaterTorusSprite();

            // Cyan ring rim behind mascot face (76x76)
            var cyanRimGo = new GameObject("MascotCyanRim", typeof(RectTransform), typeof(Image));
            cyanRimGo.transform.SetParent(stageArea, false);
            var crRt = cyanRimGo.GetComponent<RectTransform>();
            crRt.anchorMin = new Vector2(0.5f, 0.54f);
            crRt.anchorMax = new Vector2(0.5f, 0.54f);
            crRt.sizeDelta = new Vector2(76f, 76f);
            var crImg = cyanRimGo.GetComponent<Image>();
            crImg.sprite = SpriteHelper.GetCircleSprite();
            crImg.color = SpriteHelper.ColorCyanMain;

            // Mascot Face: 3D Nom-Nom Face (68x68)
            var nibblesGo = new GameObject("DioramaNibbles", typeof(RectTransform), typeof(Image));
            nibblesGo.transform.SetParent(stageArea, false);
            var nibRt = nibblesGo.GetComponent<RectTransform>();
            nibRt.anchorMin = new Vector2(0.5f, 0.54f);
            nibRt.anchorMax = new Vector2(0.5f, 0.54f);
            nibRt.sizeDelta = new Vector2(68f, 68f);
            var nibImg = nibblesGo.GetComponent<Image>();
            nibImg.sprite = SpriteHelper.GetEmojiSprite("emoji_3d_nom_nom");
            nibImg.preserveAspect = true;

            // "DONUT FLOAT" badge sitting right on top edge of the torus ring
            var donutBadge = CreatePill(stageArea, "DonutBadge", new Vector2(0.31f, 0.81f), new Vector2(0.69f, 0.90f),
                Color.white, SpriteHelper.ColorBorderDark, 2f);
            CreateEmojiImage(donutBadge, "emoji_3d_donut", new Vector2(14f, 14f), new Vector2(0.16f, 0.5f));
            var donutTxt = CreateText(donutBadge, "DONUT FLOAT", 10.5f, SpriteHelper.ColorBorderDark, TextAlignmentOptions.MidlineLeft);
            donutTxt.fontStyle = FontStyles.Bold;
            var dtRt = donutTxt.GetComponent<RectTransform>();
            dtRt.anchorMin = new Vector2(0.32f, 0f);
            dtRt.anchorMax = new Vector2(0.95f, 1f);

            // Unified Launch Console Card
            var consoleCard = CreateSlicedCard(parent, "LaunchConsole", new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.36f),
                new Color32(0x1e, 0x1b, 0x4b, 0xf2), new Color32(0x63, 0x66, 0xf1, 0x88), 2.5f, 22f);

            // Meta row: Sector tag + "Bubblegum Belt" cyan pill + "Goal: 20 kg" yellow pill
            var sectorTag = CreateText(consoleCard, "SECTOR", 10, new Color32(0xa5, 0xb4, 0xfc, 0xff), TextAlignmentOptions.MidlineLeft);
            sectorTag.fontStyle = FontStyles.Bold;
            var stRt = sectorTag.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.05f, 0.80f);
            stRt.anchorMax = new Vector2(0.22f, 0.96f);

            var worldPill = CreatePill(consoleCard, "WorldPill", new Vector2(0.24f, 0.78f), new Vector2(0.62f, 0.96f),
                new Color32(0x38, 0xbd, 0xf8, 0x25), SpriteHelper.ColorCyanMain);
            var wTxt = CreateText(worldPill, "Bubblegum Belt", 11, SpriteHelper.ColorCyanMain, TextAlignmentOptions.Center);
            wTxt.fontStyle = FontStyles.Bold;

            var goalPill = CreatePill(consoleCard, "GoalPill", new Vector2(0.66f, 0.78f), new Vector2(0.95f, 0.96f),
                new Color32(0xfa, 0xcc, 0x15, 0x25), SpriteHelper.ColorYellowMain);
            var gTxt = CreateText(goalPill, "Goal: 20 kg", 11, SpriteHelper.ColorYellowMain, TextAlignmentOptions.Center);
            gTxt.fontStyle = FontStyles.Bold;

            // Level title row: Star + "LVL 1 • Bubblegum Chute 1"
            CreateEmojiImage(consoleCard, "emoji_3d_star", new Vector2(22f, 22f), new Vector2(0.08f, 0.62f));
            var lvlTitle = CreateText(consoleCard, "LVL 1 • Bubblegum Chute 1", 16, Color.white, TextAlignmentOptions.MidlineLeft);
            lvlTitle.fontStyle = FontStyles.Bold;
            var ltRt = lvlTitle.GetComponent<RectTransform>();
            ltRt.anchorMin = new Vector2(0.16f, 0.50f);
            ltRt.anchorMax = new Vector2(0.95f, 0.74f);

            // Big 3D Emerald "PLAY MISSION" Pill Button with play triangle icon & 6px shadow lip
            CreatePillButton3D(consoleCard, "Btn_PlayMission", "PLAY MISSION",
                SpriteHelper.ColorGreenMain, SpriteHelper.ColorGreenBottom, SpriteHelper.ColorGreenShadow, SpriteHelper.ColorBorderDark,
                new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.44f), 18f, () => StartSector(1), "play_triangle", Color.white, 6f);

            // Bottom Navigation Arcade Dock
            var dock = CreateSlicedCard(parent, "ArcadeDock", new Vector2(0.04f, 0.015f), new Vector2(0.96f, 0.095f),
                new Color32(0x13, 0x10, 0x38, 0xf8), new Color32(0x43, 0x38, 0xca, 0x77), 2f, 24f);

            CreateDockTab(dock, "MAPS", "emoji_3d_map", 0f, 0.25f, null, () => ShowMaps());
            CreateDockTab(dock, "DAILY", "emoji_3d_calendar", 0.25f, 0.50f, "3 NEW", () => ShowDaily());
            CreateDockTab(dock, "SHOP", "emoji_3d_gift", 0.50f, 0.75f, "FREE", () => ShowShop());
            CreateDockTab(dock, "TROPHIES", "emoji_3d_trophy", 0.75f, 1f, null, () => ShowTrophies());
        }
        #endregion

        #region Screen 2: Cleanup Maps Screen (50 Levels)
        private void BuildMapsScreen(Transform parent)
        {
            CreateHeaderBarWithCoins(parent, "CLEANUP MAPS", "emoji_3d_rocket", () => ShowHome());

            var content = CreateRect(parent, "MapsContent", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.90f));

            // World Tabs Row: 1-10 (active), 11-20, 21-30, 31-40, 41-50
            var tabsRow = CreateRect(content, "WorldTabs", new Vector2(0f, 0.93f), new Vector2(1f, 0.99f));
            CreatePillTab(tabsRow, "emoji_3d_candy", "1-10", 0f, 0.19f, true);
            CreatePillTab(tabsRow, "emoji_3d_cheese", "11-20", 0.20f, 0.39f, false);
            CreatePillTab(tabsRow, "emoji_3d_donut", "21-30", 0.40f, 0.59f, false);
            CreatePillTab(tabsRow, "emoji_3d_milky_way", "31-40", 0.60f, 0.79f, false);
            CreatePillTab(tabsRow, "emoji_3d_hole", "41-50", 0.80f, 1.0f, false);

            // World Banner Card
            var banner = CreateSlicedCard(content, "WorldBanner", new Vector2(0f, 0.81f), new Vector2(1f, 0.91f),
                new Color32(0x27, 0x23, 0x69, 0xf0), new Color32(0x63, 0x66, 0xf1, 0x66), 2f, 18f);

            CreateEmojiImage(banner, "emoji_3d_candy", new Vector2(38f, 38f), new Vector2(0.08f, 0.5f));
            var bTitle = CreateText(banner, "Bubblegum Belt", 15, Color.white, TextAlignmentOptions.MidlineLeft);
            bTitle.fontStyle = FontStyles.Bold;
            var btRt = bTitle.GetComponent<RectTransform>();
            btRt.anchorMin = new Vector2(0.18f, 0.52f);
            btRt.anchorMax = new Vector2(0.72f, 0.95f);

            var bDesc = CreateText(banner, "Orbital Gravity Basics & Clean Flow", 11, new Color32(0xa5, 0xb4, 0xfc, 0xee), TextAlignmentOptions.MidlineLeft);
            var bdRt = bDesc.GetComponent<RectTransform>();
            bdRt.anchorMin = new Vector2(0.18f, 0.12f);
            bdRt.anchorMax = new Vector2(0.72f, 0.52f);

            var starPill = CreatePill(banner, "StarPill", new Vector2(0.74f, 0.28f), new Vector2(0.96f, 0.72f),
                new Color32(0xfa, 0xcc, 0x15, 0x25), SpriteHelper.ColorYellowMain);
            CreateEmojiImage(starPill, "emoji_3d_star", new Vector2(16f, 16f), new Vector2(0.24f, 0.5f));
            var stTxt = CreateText(starPill, "0/10", 11, SpriteHelper.ColorYellowMain, TextAlignmentOptions.Center);
            stTxt.fontStyle = FontStyles.Bold;
            var stRt = stTxt.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.38f, 0f);
            stRt.anchorMax = new Vector2(0.95f, 1f);

            // 2-Column Level Grid (10 Levels displayed)
            var grid = CreateRect(content, "LevelGrid", new Vector2(0f, 0.01f), new Vector2(1f, 0.79f));

            float rowH = 0.185f;
            float pad = 0.015f;
            for (int i = 0; i < 5; i++)
            {
                float yTop = 1f - i * (rowH + pad);
                float yBot = yTop - rowH;

                int lvlLeft = i * 2 + 1;
                int lvlRight = i * 2 + 2;

                CreateLevelTile(grid, lvlLeft, $"Bubblegum Chute {lvlLeft}", $"{14 + lvlLeft * 6} kg", new Vector2(0f, yBot), new Vector2(0.485f, yTop), lvlLeft == 1);
                CreateLevelTile(grid, lvlRight, $"Bubblegum Chute {lvlRight}", $"{14 + lvlRight * 6} kg", new Vector2(0.515f, yBot), new Vector2(1f, yTop), false);
            }
        }

        private void CreateLevelTile(Transform parent, int levelNum, string name, string goal, Vector2 aMin, Vector2 aMax, bool isActive)
        {
            Color borderCol = isActive ? SpriteHelper.ColorGreenMain : new Color32(0x63, 0x66, 0xf1, 0x55);
            Color fillCol = isActive ? new Color32(0x1e, 0x38, 0x56, 0xf0) : new Color32(0x27, 0x23, 0x69, 0xee);

            var tile = CreateSlicedCard(parent, $"Tile_Lvl_{levelNum}", aMin, aMax, fillCol, borderCol, isActive ? 2.5f : 1.8f, 16f);
            var btn = tile.gameObject.AddComponent<Button>();
            tile.gameObject.AddComponent<UIBounceButton>();
            btn.onClick.AddListener(() => StartSector(levelNum));

            // Level Pill Badge
            var badge = CreatePill(tile, "Badge", new Vector2(0.08f, 0.65f), new Vector2(0.42f, 0.90f),
                SpriteHelper.ColorYellowMain, SpriteHelper.ColorBorderDark, 1.5f);
            var bTxt = CreateText(badge, $"LVL {levelNum}", 10, SpriteHelper.ColorBorderDark, TextAlignmentOptions.Center);
            bTxt.fontStyle = FontStyles.Bold;

            if (isActive)
            {
                CreateEmojiImage(tile, "emoji_3d_star", new Vector2(14f, 14f), new Vector2(0.58f, 0.77f));
                var playTxt = CreateText(tile, "PLAY", 11.5f, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineRight);
                playTxt.fontStyle = FontStyles.Bold;
                var ptRt = playTxt.GetComponent<RectTransform>();
                ptRt.anchorMin = new Vector2(0.66f, 0.65f);
                ptRt.anchorMax = new Vector2(0.92f, 0.90f);
            }
            else
            {
                var triGo = new GameObject("PlayIcon", typeof(RectTransform), typeof(Image));
                triGo.transform.SetParent(tile, false);
                var trRt = triGo.GetComponent<RectTransform>();
                trRt.anchorMin = new Vector2(0.88f, 0.77f);
                trRt.anchorMax = new Vector2(0.88f, 0.77f);
                trRt.sizeDelta = new Vector2(13f, 13f);
                var trImg = triGo.GetComponent<Image>();
                trImg.sprite = SpriteHelper.GetPlayTriangleSprite();
                trImg.color = Color.white;
            }

            var tName = CreateText(tile, name, 12, Color.white, TextAlignmentOptions.MidlineLeft);
            tName.fontStyle = FontStyles.Bold;
            var tnRt = tName.GetComponent<RectTransform>();
            tnRt.anchorMin = new Vector2(0.08f, 0.34f);
            tnRt.anchorMax = new Vector2(0.92f, 0.62f);

            var tGoal = CreateText(tile, $"Goal: {goal}", 10, new Color32(0xa5, 0xb4, 0xfc, 0xcc), TextAlignmentOptions.MidlineLeft);
            var tgRt = tGoal.GetComponent<RectTransform>();
            tgRt.anchorMin = new Vector2(0.08f, 0.08f);
            tgRt.anchorMax = new Vector2(0.92f, 0.32f);
        }

        private Transform CreatePillTab(Transform parent, string label, float xMin, float xMax, bool active, Action onClick = null)
        {
            return CreatePillTab(parent, null, label, xMin, xMax, active, onClick);
        }

        private Transform CreatePillTab(Transform parent, string emojiName, string label, float xMin, float xMax, bool active, Action onClick = null)
        {
            Color fill = active ? SpriteHelper.ColorYellowMain : new Color32(0x31, 0x2e, 0x81, 0xcc);
            Color border = active ? SpriteHelper.ColorBorderDark : new Color32(0x63, 0x66, 0xf1, 0x44);
            Color textCol = active ? SpriteHelper.ColorBorderDark : Color.white;

            var pill = CreatePill(parent, $"Tab_{label}", new Vector2(xMin, 0f), new Vector2(xMax, 1f), fill, border);
            var btn = pill.gameObject.AddComponent<Button>();
            pill.gameObject.AddComponent<UIBounceButton>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            if (!string.IsNullOrEmpty(emojiName))
            {
                CreateEmojiImage(pill, emojiName, new Vector2(15f, 15f), new Vector2(0.25f, 0.5f));
                var txt = CreateText(pill, label, 10.5f, textCol, TextAlignmentOptions.MidlineLeft);
                txt.name = "TabText";
                txt.fontStyle = FontStyles.Bold;
                var tRt = txt.GetComponent<RectTransform>();
                tRt.anchorMin = new Vector2(0.44f, 0f);
                tRt.anchorMax = new Vector2(0.96f, 1f);
            }
            else
            {
                var txt = CreateText(pill, label, 11, textCol, TextAlignmentOptions.Center);
                txt.name = "TabText";
                txt.fontStyle = FontStyles.Bold;
            }
            return pill;
        }
        #endregion

        #region Screen 3: Daily Operations Screen
        private void BuildDailyScreen(Transform parent)
        {
            CreateHeaderBarWithCoins(parent, "DAILY OPERATIONS", "emoji_3d_calendar", () => ShowHome());

            var content = CreateRect(parent, "DailyContent", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.90f));

            // Top Status Card (Resets in 23h 6m, 0/3 Cleared, Bronze/Silver/Gold progress bar)
            var topBanner = CreateSlicedCard(content, "DailyBanner", new Vector2(0f, 0.77f), new Vector2(1f, 0.99f),
                new Color32(0x27, 0x23, 0x69, 0xf0), SpriteHelper.ColorYellowMain, 2f, 20f);

            CreateEmojiImage(topBanner, "emoji_3d_calendar", new Vector2(32f, 32f), new Vector2(0.08f, 0.78f));
            var dbTitle = CreateText(topBanner, "DAILY OPERATIONS", 14, Color.white, TextAlignmentOptions.MidlineLeft);
            dbTitle.fontStyle = FontStyles.Bold;
            var dtRt = dbTitle.GetComponent<RectTransform>();
            dtRt.anchorMin = new Vector2(0.18f, 0.65f);
            dtRt.anchorMax = new Vector2(0.48f, 0.92f);

            var timerPill = CreatePill(topBanner, "TimerPill", new Vector2(0.48f, 0.68f), new Vector2(0.75f, 0.92f),
                new Color32(0x1e, 0x1b, 0x4b, 0xdd), new Color32(0x63, 0x66, 0xf1, 0x66));
            var tmTxt = CreateText(timerPill, "Resets in 23h 6m", 9, Color.white, TextAlignmentOptions.Center);
            tmTxt.fontStyle = FontStyles.Bold;

            var clrPill = CreatePill(topBanner, "ClrPill", new Vector2(0.77f, 0.68f), new Vector2(0.96f, 0.92f),
                new Color32(0xfa, 0xcc, 0x15, 0x33), SpriteHelper.ColorYellowMain);
            var clrTxt = CreateText(clrPill, "0/3 CLEARED", 9, SpriteHelper.ColorYellowMain, TextAlignmentOptions.Center);
            clrTxt.fontStyle = FontStyles.Bold;

            // Milestones Row: BRONZE, SILVER, GOLD
            var mRow = CreateRect(topBanner, "Milestones", new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.66f));
            CreateText(mRow, "BRONZE", 10, SpriteHelper.ColorBronze, TextAlignmentOptions.MidlineLeft).fontStyle = FontStyles.Bold;
            CreateText(mRow, "SILVER", 10, SpriteHelper.ColorSilver, TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
            CreateText(mRow, "GOLD", 10, SpriteHelper.ColorGold, TextAlignmentOptions.MidlineRight).fontStyle = FontStyles.Bold;

            CreatePill(topBanner, "MilestoneTrack", new Vector2(0.06f, 0.38f), new Vector2(0.94f, 0.45f),
                new Color32(0x0f, 0x17, 0x2a, 0xcc), new Color32(0x63, 0x66, 0xf1, 0x44), 1f);

            // All-Clear Bounty Row
            var bBox = CreateSlicedCard(topBanner, "BountyBox", new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.32f),
                new Color32(0x1e, 0x1b, 0x4b, 0xaa), new Color32(0x63, 0x66, 0xf1, 0x44), 1.5f, 12f);
            CreateEmojiImage(bBox, "emoji_3d_locked", new Vector2(20f, 20f), new Vector2(0.08f, 0.5f));
            var bTxt = CreateText(bBox, "Master All-Clear Bounty: Complete all 3 shifts", 10, new Color32(0xa5, 0xb4, 0xfc, 0xff), TextAlignmentOptions.MidlineLeft);
            var btRt = bTxt.GetComponent<RectTransform>();
            btRt.anchorMin = new Vector2(0.18f, 0f);
            btRt.anchorMax = new Vector2(0.74f, 1f);

            var bCost = CreateText(bBox, "+300", 11, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineRight);
            bCost.fontStyle = FontStyles.Bold;
            var bcRt = bCost.GetComponent<RectTransform>();
            bcRt.anchorMin = new Vector2(0.74f, 0f);
            bcRt.anchorMax = new Vector2(0.88f, 1f);
            CreateEmojiImage(bBox, "emoji_3d_coin", new Vector2(13f, 13f), new Vector2(0.93f, 0.5f));

            // Mission 1: Bronze Card
            CreateDailyCard(content, "Bronze Shift • Cadet Sweep",
                "Calm Orbit Sweep: Gentle centrifugal speed, wide gates & easy flow.",
                "emoji_3d_broom", "BRONZE", SpriteHelper.ColorBronze, 200, 31,
                new Vector2(0f, 0.52f), new Vector2(1f, 0.75f));

            // Mission 2: Silver Card
            CreateDailyCard(content, "Silver Shift • Sector Hazard",
                "Snack Attack: Nibbles is extra hungry today! Vacuum quickly.",
                "emoji_3d_lightning", "SILVER", SpriteHelper.ColorSilver, 350, 53,
                new Vector2(0f, 0.27f), new Vector2(1f, 0.50f));

            // Mission 3: Gold Card
            CreateDailyCard(content, "Gold Shift • Asteroid Flux",
                "Cosmic Surge: Multiplier gates hyper-charged with heavy titanium.",
                "emoji_3d_crown", "GOLD", SpriteHelper.ColorGold, 500, 78,
                new Vector2(0f, 0.02f), new Vector2(1f, 0.25f));
        }

        private void CreateDailyCard(Transform parent, string title, string desc, string emojiName, string badgeLabel, Color badgeColor, int rewardCoins, int goalKg, Vector2 aMin, Vector2 aMax)
        {
            var card = CreateSlicedCard(parent, $"Card_{badgeLabel}", aMin, aMax,
                new Color32(0x27, 0x23, 0x69, 0xf4), badgeColor, 2.5f, 20f);

            // Badge + TODAY
            var badgePill = CreatePill(card, "Badge", new Vector2(0.05f, 0.74f), new Vector2(0.32f, 0.94f),
                new Color32(0x1e, 0x1b, 0x4b, 0xee), badgeColor, 1.8f);
            var bTxt = CreateText(badgePill, badgeLabel, 10, badgeColor, TextAlignmentOptions.Center);
            bTxt.fontStyle = FontStyles.Bold;

            var tToday = CreateText(card, "TODAY", 10, new Color32(0xa5, 0xb4, 0xfc, 0xff), TextAlignmentOptions.MidlineRight);
            tToday.fontStyle = FontStyles.Bold;
            var ttRt = tToday.GetComponent<RectTransform>();
            ttRt.anchorMin = new Vector2(0.70f, 0.74f);
            ttRt.anchorMax = new Vector2(0.95f, 0.94f);

            // Title
            var tTitle = CreateText(card, title, 13.5f, Color.white, TextAlignmentOptions.MidlineLeft);
            tTitle.fontStyle = FontStyles.Bold;
            var tRt = tTitle.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.05f, 0.52f);
            tRt.anchorMax = new Vector2(0.95f, 0.72f);

            // Hazard modifier box with rounded rectangle corners (not pill)
            var dBox = CreateSlicedCard(card, "HazardBox", new Vector2(0.05f, 0.26f), new Vector2(0.95f, 0.48f),
                new Color32(0x1e, 0x1b, 0x4b, 0x88), new Color32(0x63, 0x66, 0xf1, 0x33), 1.5f, 10f);
            CreateEmojiImage(dBox, emojiName, new Vector2(16f, 16f), new Vector2(0.06f, 0.5f));
            var tDesc = CreateText(dBox, desc, 10, new Color32(0xa5, 0xb4, 0xfc, 0xdd), TextAlignmentOptions.MidlineLeft);
            var tdRt = tDesc.GetComponent<RectTransform>();
            tdRt.anchorMin = new Vector2(0.13f, 0f);
            tdRt.anchorMax = new Vector2(0.98f, 1f);

            // Footer: +XXX [coin] Goal: XX kg + 3D Yellow Start Shift Pill Button
            var tRew = CreateText(card, $"+{rewardCoins}", 11.5f, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineLeft);
            tRew.fontStyle = FontStyles.Bold;
            var trRt = tRew.GetComponent<RectTransform>();
            trRt.anchorMin = new Vector2(0.05f, 0.04f);
            trRt.anchorMax = new Vector2(0.16f, 0.24f);

            CreateEmojiImage(card, "emoji_3d_coin", new Vector2(13f, 13f), new Vector2(0.18f, 0.14f));

            var tGoal = CreateText(card, $"Goal: {goalKg} kg", 11.5f, Color.white, TextAlignmentOptions.MidlineLeft);
            tGoal.fontStyle = FontStyles.Bold;
            var tgRt = tGoal.GetComponent<RectTransform>();
            tgRt.anchorMin = new Vector2(0.22f, 0.04f);
            tgRt.anchorMax = new Vector2(0.58f, 0.24f);

            CreatePillButton3D(card, "Btn_StartShift", "Start Shift",
                SpriteHelper.ColorYellowMain, new Color32(0xf5, 0x9e, 0x0b, 0xff), SpriteHelper.ColorYellowShadow, SpriteHelper.ColorBorderDark,
                new Vector2(0.60f, 0.04f), new Vector2(0.96f, 0.24f), 11.5f, () => StartSector(1), "play_triangle_right", SpriteHelper.ColorBorderDark, 3.5f);
        }
        #endregion

        #region Screen 4: Toy Shop & Hats Screen
        private void BuildShopScreen(Transform parent)
        {
            CreateHeaderBarWithCoins(parent, "TOY SHOP", "emoji_3d_gift", () => ShowHome());

            var content = CreateRect(parent, "ShopContent", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.90f));

            // Free Daily Stipend Banner with glowing yellow border (1:1 Web Replica)
            var freeBanner = CreateSlicedCard(content, "FreeStipendBanner", new Vector2(0f, 0.77f), new Vector2(1f, 0.99f),
                new Color32(0x2d, 0x22, 0x5a, 0xf8), SpriteHelper.ColorYellowMain, 2.5f, 20f);

            var iconBox = CreateSlicedCard(freeBanner, "IconBox", new Vector2(0.035f, 0.18f), new Vector2(0.185f, 0.82f),
                new Color32(0xf5, 0x9e, 0x0b, 0x30), new Color32(0xf5, 0x9e, 0x0b, 0x60), 1.5f, 14f);
            CreateEmojiImage(iconBox, "emoji_3d_gift", new Vector2(30f, 30f), new Vector2(0.5f, 0.5f));

            var tagTxt = CreateText(freeBanner, "FREE DAILY STIPEND", 9, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineLeft);
            tagTxt.fontStyle = FontStyles.Bold;
            var tgRt = tagTxt.GetComponent<RectTransform>();
            tgRt.anchorMin = new Vector2(0.22f, 0.70f);
            tgRt.anchorMax = new Vector2(0.62f, 0.92f);

            var sTitle = CreateText(freeBanner, "Janitor Care Package!", 14, Color.white, TextAlignmentOptions.MidlineLeft);
            sTitle.fontStyle = FontStyles.Bold;
            var stRt = sTitle.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.22f, 0.44f);
            stRt.anchorMax = new Vector2(0.62f, 0.70f);

            // Description Row 1: "Claim your free daily +100" + [coin]
            var descRow1 = new GameObject("DescRow1", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            descRow1.transform.SetParent(freeBanner, false);
            var drRt = descRow1.GetComponent<RectTransform>();
            drRt.anchorMin = new Vector2(0.22f, 0.25f);
            drRt.anchorMax = new Vector2(0.62f, 0.44f);
            drRt.offsetMin = Vector2.zero;
            drRt.offsetMax = Vector2.zero;

            var drHlg = descRow1.GetComponent<HorizontalLayoutGroup>();
            drHlg.childAlignment = TextAnchor.MiddleLeft;
            drHlg.spacing = 3f;
            drHlg.childControlWidth = true;
            drHlg.childControlHeight = true;
            drHlg.childForceExpandWidth = false;
            drHlg.childForceExpandHeight = false;

            var sDesc1 = CreateText(descRow1.transform, "Claim your free daily +100", 9.5f, new Color32(0xa5, 0xb4, 0xfc, 0xee), TextAlignmentOptions.MidlineLeft);
            sDesc1.textWrappingMode = TextWrappingModes.NoWrap;
            var leD1 = sDesc1.gameObject.AddComponent<LayoutElement>();
            leD1.preferredWidth = sDesc1.preferredWidth;
            leD1.preferredHeight = 16f;

            var coinImg = CreateEmojiImage(descRow1.transform, "emoji_3d_coin", new Vector2(11f, 11f));
            var leCoin = coinImg.AddComponent<LayoutElement>();
            leCoin.preferredWidth = 11f;
            leCoin.preferredHeight = 11f;

            // Description Row 2: "allowance."
            var sDesc2 = CreateText(freeBanner, "allowance.", 9.5f, new Color32(0xa5, 0xb4, 0xfc, 0xee), TextAlignmentOptions.MidlineLeft);
            var sdRt2 = sDesc2.GetComponent<RectTransform>();
            sdRt2.anchorMin = new Vector2(0.22f, 0.08f);
            sdRt2.anchorMax = new Vector2(0.62f, 0.25f);

            // 3D Yellow Pill Claim Button with dark text and coin icon
            CreatePillButton3D(freeBanner, "Btn_Claim", "Claim +100",
                SpriteHelper.ColorYellowMain, new Color32(0xf5, 0x9e, 0x0b, 0xff), SpriteHelper.ColorYellowShadow, SpriteHelper.ColorBorderDark,
                new Vector2(0.63f, 0.26f), new Vector2(0.97f, 0.68f), 12f, () => AudioManager.Instance?.PlayButtonClick(), "coin_right", SpriteHelper.ColorBorderDark, 3.5f);

            // Subtitle
            var subTxt = CreateText(content, "100% Free • Unlock cute costumes with your earned recycling coins!", 10.5f, new Color32(0xa5, 0xb4, 0xfc, 0xcc), TextAlignmentOptions.Center);
            var sbtRt = subTxt.GetComponent<RectTransform>();
            sbtRt.anchorMin = new Vector2(0f, 0.725f);
            sbtRt.anchorMax = new Vector2(1f, 0.765f);

            CreateEmojiImage(content, "emoji_3d_coin", new Vector2(13f, 13f), new Vector2(0.5f, 0.71f));

            // Filter Tabs: Donut Rings (active), Nibbles Hats, Skies
            var tabs = CreateRect(content, "ShopTabs", new Vector2(0f, 0.64f), new Vector2(1f, 0.70f));
            shopTabRings = CreatePillTab(tabs, "Donut Rings", 0f, 0.32f, true, () => SetShopTab(0));
            shopTabHats = CreatePillTab(tabs, "Nibbles Hats", 0.34f, 0.66f, false, () => SetShopTab(1));
            shopTabSkies = CreatePillTab(tabs, "Skies", 0.68f, 1.0f, false, () => SetShopTab(2));

            // Content containers for each tab
            shopRingsContainer = CreateRect(content, "RingsContainer", new Vector2(0f, 0f), new Vector2(1f, 0.63f)).gameObject;
            shopHatsContainer = CreateRect(content, "HatsContainer", new Vector2(0f, 0f), new Vector2(1f, 0.63f)).gameObject;
            shopSkiesContainer = CreateRect(content, "SkiesContainer", new Vector2(0f, 0f), new Vector2(1f, 0.63f)).gameObject;

            // TAB 0: DONUT RINGS ITEMS
            CreateShopItemCard(shopRingsContainer.transform, "Frosted Donut Rig",
                "Sweet strawberry icing with sprinkles and bouncy cartoon dough!",
                new Color32(0xf4, 0x72, 0xb6, 0xff), "STARTER FREE", "EQUIPPED", true,
                new Vector2(0f, 0.67f), new Vector2(1f, 0.99f));

            CreateShopItemCard(shopRingsContainer.transform, "Rubber Duckie Floatie",
                "Squeaky bright yellow bath duck with cute orange beak and water bubbles!",
                new Color32(0xfa, 0xcc, 0x15, 0xff), null, "Unlock (500)", false,
                new Vector2(0f, 0.34f), new Vector2(1f, 0.66f));

            CreateShopItemCard(shopRingsContainer.transform, "Peppermint Candy Ring",
                "Crisp red and white candy cane stripes with minty freshness!",
                new Color32(0xef, 0x44, 0x44, 0xff), null, "Unlock (1200)", false,
                new Vector2(0f, 0.01f), new Vector2(1f, 0.33f));

            // TAB 1: NIBBLES HATS ITEMS
            CreateShopItemCard(shopHatsContainer.transform, "Nibbles the Void",
                "The hungriest, cutest cartoon black hole with big anime eyes! Nom nom!",
                new Color32(0x81, 0x8c, 0xf8, 0xff), "STARTER FREE", "EQUIPPED", true,
                new Vector2(0f, 0.67f), new Vector2(1f, 0.99f));

            CreateShopItemCard(shopHatsContainer.transform, "Berry Swirl Vortex",
                "Sweet strawberry cyclone that eats trash like fruit smoothie!",
                new Color32(0xf4, 0x72, 0xb6, 0xff), null, "Unlock (600)", false,
                new Vector2(0f, 0.34f), new Vector2(1f, 0.66f));

            CreateShopItemCard(shopHatsContainer.transform, "Sleepy Star Cloud",
                "A sleepy pastel blue cloud that yawns and gulps down space junk!",
                new Color32(0x38, 0xbd, 0xf8, 0xff), null, "Unlock (1500)", false,
                new Vector2(0f, 0.01f), new Vector2(1f, 0.33f));

            // TAB 2: SKIES ITEMS
            CreateShopItemCard(shopSkiesContainer.transform, "Cotton Candy Nebula",
                "Fluffy pastel indigo and magenta clouds with sparkling star smileys.",
                new Color32(0x81, 0x8c, 0xf8, 0xff), "STARTER FREE", "EQUIPPED", true,
                new Vector2(0f, 0.67f), new Vector2(1f, 0.99f));

            CreateShopItemCard(shopSkiesContainer.transform, "Cheese Moon Orbit",
                "A happy cartoon moon made of Swiss cheese floating in twilight.",
                new Color32(0xfa, 0xcc, 0x15, 0xff), null, "Unlock (800)", false,
                new Vector2(0f, 0.34f), new Vector2(1f, 0.66f));

            CreateShopItemCard(shopSkiesContainer.transform, "Mint Soda Lagoon",
                "Sparkling emerald and turquoise cosmos with fizzy space bubbles.",
                new Color32(0x4a, 0xde, 0x80, 0xff), null, "Unlock (2000)", false,
                new Vector2(0f, 0.01f), new Vector2(1f, 0.33f));

            SetShopTab(0);
        }

        private void CreateShopItemCard(Transform parent, string title, string desc, Color swatchColor, string badge, string actionLabel, bool isEquipped, Vector2 aMin, Vector2 aMax)
        {
            var card = CreateSlicedCard(parent, $"Item_{title}", aMin, aMax,
                new Color32(0x27, 0x23, 0x69, 0xf0), new Color32(0x63, 0x66, 0xf1, 0x66), 2f, 18f);

            // Top row: Title on left, Badge + Swatch on right
            var tTitle = CreateText(card, title, 14, Color.white, TextAlignmentOptions.MidlineLeft);
            tTitle.fontStyle = FontStyles.Bold;
            var ttRt = tTitle.GetComponent<RectTransform>();
            ttRt.anchorMin = new Vector2(0.05f, 0.66f);
            ttRt.anchorMax = new Vector2(0.60f, 0.94f);

            if (!string.IsNullOrEmpty(badge))
            {
                var bPill = CreatePill(card, "Badge", new Vector2(0.60f, 0.70f), new Vector2(0.88f, 0.92f),
                    new Color32(0x4a, 0xde, 0x80, 0x25), SpriteHelper.ColorGreenMain, 1.5f);
                var bTxt = CreateText(bPill, badge, 9, SpriteHelper.ColorGreenMain, TextAlignmentOptions.Center);
                bTxt.fontStyle = FontStyles.Bold;
            }

            // Circular Color Swatch (16x16) on top right
            var swatch = new GameObject("ColorSwatch", typeof(RectTransform), typeof(Image));
            swatch.transform.SetParent(card, false);
            var swRt = swatch.GetComponent<RectTransform>();
            swRt.anchorMin = new Vector2(0.93f, 0.81f);
            swRt.anchorMax = new Vector2(0.93f, 0.81f);
            swRt.sizeDelta = new Vector2(16f, 16f);
            var swImg = swatch.GetComponent<Image>();
            swImg.sprite = SpriteHelper.GetCircleSprite();
            swImg.color = swatchColor;

            // Middle: Multi-line description text
            var tDesc = CreateText(card, desc, 11, new Color32(0xa5, 0xb4, 0xfc, 0xdd), TextAlignmentOptions.MidlineLeft);
            var tdRt = tDesc.GetComponent<RectTransform>();
            tdRt.anchorMin = new Vector2(0.05f, 0.35f);
            tdRt.anchorMax = new Vector2(0.95f, 0.64f);

            // Bottom row: Aligned to BOTTOM-LEFT (1:1 Web Replica)
            if (isEquipped)
            {
                var eqRow = new GameObject("EquippedRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                eqRow.transform.SetParent(card, false);
                var eqRt = eqRow.GetComponent<RectTransform>();
                eqRt.anchorMin = new Vector2(0.05f, 0.08f);
                eqRt.anchorMax = new Vector2(0.40f, 0.34f);
                eqRt.offsetMin = Vector2.zero;
                eqRt.offsetMax = Vector2.zero;

                var hlg = eqRow.GetComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.spacing = 5f;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                var eqTxt = CreateText(eqRow.transform, "EQUIPPED", 12.5f, SpriteHelper.ColorGreenMain, TextAlignmentOptions.MidlineLeft);
                eqTxt.fontStyle = FontStyles.Bold;
                eqTxt.textWrappingMode = TextWrappingModes.NoWrap;
                var leEq = eqTxt.gameObject.AddComponent<LayoutElement>();
                leEq.preferredWidth = eqTxt.preferredWidth;
                leEq.preferredHeight = 22f;

                var spk = CreateEmojiImage(eqRow.transform, "emoji_3d_sparkles", new Vector2(15f, 15f));
                var leSpk = spk.AddComponent<LayoutElement>();
                leSpk.preferredWidth = 15f;
                leSpk.preferredHeight = 15f;
            }
            else
            {
                CreatePillButton3D(card, "Btn_Unlock", actionLabel,
                    SpriteHelper.ColorYellowMain, new Color32(0xf5, 0x9e, 0x0b, 0xff), SpriteHelper.ColorYellowShadow, SpriteHelper.ColorBorderDark,
                    new Vector2(0.05f, 0.08f), new Vector2(0.55f, 0.34f), 11.5f, () => AudioManager.Instance?.PlayButtonClick(),
                    "coin_right", SpriteHelper.ColorBorderDark, 3f);
            }
        }
        #endregion

        #region Screen 5: Trophy Room Screen
        private void BuildTrophiesScreen(Transform parent)
        {
            CreateHeaderBarWithCoins(parent, "TROPHY ROOM", "emoji_3d_trophy", () => ShowHome());

            var content = CreateRect(parent, "TrophiesContent", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.90f));

            CreateTrophyCard(content, "First Nom Nom!", "Feed Nibbles your very first piece of space junk!", "emoji_3d_nom_nom", 100, false, new Vector2(0f, 0.78f), new Vector2(1f, 0.98f));
            CreateTrophyCard(content, "Tidy Space Cadet", "Vacuum up 50 pieces of space trash into Nibbles' tummy.", "emoji_3d_broom", 250, false, new Vector2(0f, 0.58f), new Vector2(1f, 0.76f));
            CreateTrophyCard(content, "Kessler Hero", "Clean up 250 space items to save the orbit from clutter!", "emoji_3d_star", 750, false, new Vector2(0f, 0.38f), new Vector2(1f, 0.56f));
            CreateTrophyCard(content, "Galaxy Janitor King", "1,000 items recycled! You are the ultimate cosmic cleaner!", "emoji_3d_crown", 2000, false, new Vector2(0f, 0.18f), new Vector2(1f, 0.36f));
            CreateTrophyCard(content, "Daily Duty Star", "Complete today's wacky Daily Shift cleanup job!", "emoji_3d_party_popper", 500, false, new Vector2(0f, 0.00f), new Vector2(1f, 0.16f));
        }

        private void CreateTrophyCard(Transform parent, string title, string desc, string emojiName, int prizeAmount, bool claimed, Vector2 aMin, Vector2 aMax)
        {
            var card = CreateSlicedCard(parent, $"Trophy_{title}", aMin, aMax,
                new Color32(0x27, 0x23, 0x69, 0xf0), new Color32(0x63, 0x66, 0xf1, 0x66), 2f, 18f);

            // Top Row: Emoji Icon on left
            CreateEmojiImage(card, emojiName, new Vector2(24f, 24f), new Vector2(0.08f, 0.78f));

            // Title next to emoji
            var tTitle = CreateText(card, title, 13.5f, Color.white, TextAlignmentOptions.MidlineLeft);
            tTitle.fontStyle = FontStyles.Bold;
            var ttRt = tTitle.GetComponent<RectTransform>();
            ttRt.anchorMin = new Vector2(0.16f, 0.66f);
            ttRt.anchorMax = new Vector2(0.68f, 0.92f);

            // Plain Status Text on top right (NO pill badge, 1:1 Web Replica)
            var stTxt = CreateText(card, claimed ? "UNLOCKED" : "LOCKED", 11.5f, claimed ? SpriteHelper.ColorGreenMain : new Color(0.6f, 0.65f, 0.75f, 0.8f), TextAlignmentOptions.MidlineRight);
            stTxt.fontStyle = FontStyles.Bold;
            var stRt = stTxt.GetComponent<RectTransform>();
            stRt.anchorMin = new Vector2(0.68f, 0.66f);
            stRt.anchorMax = new Vector2(0.95f, 0.92f);

            // Middle: Description spans full width
            var tDesc = CreateText(card, desc, 11f, new Color32(0xa5, 0xb4, 0xfc, 0xcc), TextAlignmentOptions.MidlineLeft);
            var tdRt = tDesc.GetComponent<RectTransform>();
            tdRt.anchorMin = new Vector2(0.05f, 0.30f);
            tdRt.anchorMax = new Vector2(0.95f, 0.64f);

            // Bottom: +XXX [coin] Prize in yellow bold on left (grouped via HorizontalLayoutGroup)
            var pRow = new GameObject("PrizeRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            pRow.transform.SetParent(card, false);
            var prRt = pRow.GetComponent<RectTransform>();
            prRt.anchorMin = new Vector2(0.05f, 0.05f);
            prRt.anchorMax = new Vector2(0.60f, 0.28f);
            prRt.offsetMin = Vector2.zero;
            prRt.offsetMax = Vector2.zero;

            var pHlg = pRow.GetComponent<HorizontalLayoutGroup>();
            pHlg.childAlignment = TextAnchor.MiddleLeft;
            pHlg.spacing = 4f;
            pHlg.childControlWidth = true;
            pHlg.childControlHeight = true;
            pHlg.childForceExpandWidth = false;
            pHlg.childForceExpandHeight = false;

            var tAmt = CreateText(pRow.transform, $"+{prizeAmount}", 12f, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineLeft);
            tAmt.fontStyle = FontStyles.Bold;
            tAmt.textWrappingMode = TextWrappingModes.NoWrap;
            var leAmt = tAmt.gameObject.AddComponent<LayoutElement>();
            leAmt.preferredWidth = tAmt.preferredWidth;
            leAmt.preferredHeight = 18f;

            var coinImg = CreateEmojiImage(pRow.transform, "emoji_3d_coin", new Vector2(13f, 13f));
            var leCoin = coinImg.AddComponent<LayoutElement>();
            leCoin.preferredWidth = 13f;
            leCoin.preferredHeight = 13f;

            var tPrize = CreateText(pRow.transform, "Prize", 12f, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineLeft);
            tPrize.fontStyle = FontStyles.Bold;
            tPrize.textWrappingMode = TextWrappingModes.NoWrap;
            var lePrize = tPrize.gameObject.AddComponent<LayoutElement>();
            lePrize.preferredWidth = tPrize.preferredWidth;
            lePrize.preferredHeight = 18f;
        }
        #endregion

        #region Screen 6: Game Settings Screen
        private void BuildSettingsScreen(Transform parent)
        {
            // Settings header does NOT show coins pill (1:1 Web Replica)
            CreateHeaderBar(parent, "GAME SETTINGS", "emoji_3d_gear", () => ShowHome(), false);

            var content = CreateRect(parent, "SettingsContent", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.90f));

            // Group 1: Audio & Sound FX (Master Sound, Sound FX Volume, Cosmic Ambience)
            var g1 = CreateSlicedCard(content, "Group_Audio", new Vector2(0f, 0.65f), new Vector2(1f, 1.0f),
                new Color32(0x27, 0x23, 0x69, 0xf0), new Color32(0x63, 0x66, 0xf1, 0x66), 2f, 20f);

            CreateGroupHeader(g1, "AUDIO & SOUND FX", "emoji_3d_sound_on", 0.85f);
            CreateToggleRow(g1, "Master Sound", "Enable or mute all game audio", true, 0.58f);
            CreateSliderRow(g1, "Sound FX Volume", "Ratchet clicks, pops, bursts", 0.8f, 0.32f);
            CreateSliderRow(g1, "Cosmic Ambience", "Background space synth melody", 0.65f, 0.06f);

            // Group 2: Gameplay & Feel (Haptic Vibration)
            var g2 = CreateSlicedCard(content, "Group_Haptics", new Vector2(0f, 0.46f), new Vector2(1f, 0.63f),
                new Color32(0x27, 0x23, 0x69, 0xf0), new Color32(0x63, 0x66, 0xf1, 0x66), 2f, 20f);

            CreateGroupHeader(g2, "GAMEPLAY & FEEL", "emoji_3d_booster", 0.70f);
            CreateToggleRow(g2, "Haptic Vibration", "Tactile tap buzzes during spins & bumps", true, 0.15f);

            // Group 3: Privacy & Safety (100% Offline, Kid-Safe)
            var g3 = CreateSlicedCard(content, "Group_Privacy", new Vector2(0f, 0.27f), new Vector2(1f, 0.44f),
                new Color32(0x27, 0x23, 0x69, 0xf0), new Color32(0x63, 0x66, 0xf1, 0x66), 2f, 20f);

            CreateGroupHeader(g3, "PRIVACY & SAFETY", "emoji_3d_locked", 0.70f);
            var pTitle = CreateText(g3, "Privacy Policy", 12, Color.white, TextAlignmentOptions.MidlineLeft);
            pTitle.fontStyle = FontStyles.Bold;
            var ptRt = pTitle.GetComponent<RectTransform>();
            ptRt.anchorMin = new Vector2(0.06f, 0.35f);
            ptRt.anchorMax = new Vector2(0.60f, 0.60f);

            var pSub = CreateText(g3, "100% offline • Kid-safe, no trackers", 10, new Color32(0xa5, 0xb4, 0xfc, 0xcc), TextAlignmentOptions.MidlineLeft);
            var psRt = pSub.GetComponent<RectTransform>();
            psRt.anchorMin = new Vector2(0.06f, 0.10f);
            psRt.anchorMax = new Vector2(0.60f, 0.35f);

            CreatePillButton3D(g3, "Btn_Policy", "View Policy",
                SpriteHelper.ColorYellowMain, new Color32(0xf5, 0x9e, 0x0b, 0xff), SpriteHelper.ColorYellowShadow, SpriteHelper.ColorBorderDark,
                new Vector2(0.62f, 0.18f), new Vector2(0.95f, 0.58f), 11.5f, () => {}, null, SpriteHelper.ColorBorderDark, 3.5f);

            // Group 4: Game Data (Reset Progress)
            var g4 = CreateSlicedCard(content, "Group_Data", new Vector2(0f, 0.08f), new Vector2(1f, 0.25f),
                new Color32(0x27, 0x23, 0x69, 0xf0), new Color32(0x63, 0x66, 0xf1, 0x66), 2f, 20f);

            CreateGroupHeader(g4, "GAME DATA", "emoji_3d_gear", 0.70f);
            var rTitle = CreateText(g4, "Reset Progress", 12, Color.white, TextAlignmentOptions.MidlineLeft);
            rTitle.fontStyle = FontStyles.Bold;
            var rtRt = rTitle.GetComponent<RectTransform>();
            rtRt.anchorMin = new Vector2(0.06f, 0.35f);
            rtRt.anchorMax = new Vector2(0.60f, 0.60f);

            var rSub = CreateText(g4, "Clear unlocked levels, skins & coins", 10, new Color32(0xa5, 0xb4, 0xfc, 0xcc), TextAlignmentOptions.MidlineLeft);
            var rsRt = rSub.GetComponent<RectTransform>();
            rsRt.anchorMin = new Vector2(0.06f, 0.10f);
            rsRt.anchorMax = new Vector2(0.60f, 0.35f);

            CreatePillButton3D(g4, "Btn_Reset", "Reset",
                new Color32(0xef, 0x44, 0x44, 0xff), new Color32(0xdc, 0x26, 0x26, 0xff), new Color32(0x99, 0x1b, 0x1b, 0xff), SpriteHelper.ColorBorderDark,
                new Vector2(0.66f, 0.18f), new Vector2(0.95f, 0.58f), 11.5f, () => {}, null, Color.white, 3.5f);

            // App Credits & Version (1:1 Web Replica)
            var footerTxt = CreateText(content, "Orbital Salvage • Space Trash Clean v1.2.0\nPure Gesture Mobile Web Puzzle", 11.5f, new Color(1f, 1f, 1f, 0.5f), TextAlignmentOptions.Center);
            var fRt = footerTxt.GetComponent<RectTransform>();
            fRt.anchorMin = new Vector2(0f, 0.005f);
            fRt.anchorMax = new Vector2(1f, 0.065f);
        }

        private void CreateGroupHeader(Transform parent, string title, string emojiName, float yCenter)
        {
            CreateEmojiImage(parent, emojiName, new Vector2(18f, 18f), new Vector2(0.06f, yCenter));
            var t = CreateText(parent, title, 11, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineLeft);
            t.fontStyle = FontStyles.Bold;
            var rt = t.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.12f, yCenter - 0.08f);
            rt.anchorMax = new Vector2(0.95f, yCenter + 0.08f);
        }

        private void CreateToggleRow(Transform parent, string title, string sub, bool isOn, float yPos)
        {
            var t = CreateText(parent, title, 12, Color.white, TextAlignmentOptions.MidlineLeft);
            t.fontStyle = FontStyles.Bold;
            var trRt = t.GetComponent<RectTransform>();
            trRt.anchorMin = new Vector2(0.06f, yPos + 0.10f);
            trRt.anchorMax = new Vector2(0.65f, yPos + 0.22f);

            var s = CreateText(parent, sub, 10, new Color32(0xa5, 0xb4, 0xfc, 0xcc), TextAlignmentOptions.MidlineLeft);
            var srRt = s.GetComponent<RectTransform>();
            srRt.anchorMin = new Vector2(0.06f, yPos);
            srRt.anchorMax = new Vector2(0.65f, yPos + 0.10f);

            // iOS style toggle switch sprite (Fixed aspect ratio 52x30)
            var swGo = new GameObject("ToggleSwitch", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIBounceButton));
            swGo.transform.SetParent(parent, false);
            var swRt = swGo.GetComponent<RectTransform>();
            swRt.anchorMin = new Vector2(0.86f, yPos + 0.10f);
            swRt.anchorMax = new Vector2(0.86f, yPos + 0.10f);
            swRt.sizeDelta = new Vector2(52f, 30f);
            swRt.anchoredPosition = Vector2.zero;
            var img = swGo.GetComponent<Image>();
            img.sprite = SpriteHelper.GetToggleSwitch(isOn);
        }

        private void CreateSliderRow(Transform parent, string title, string sub, float val, float yPos)
        {
            var t = CreateText(parent, title, 12, Color.white, TextAlignmentOptions.MidlineLeft);
            t.fontStyle = FontStyles.Bold;
            var trRt = t.GetComponent<RectTransform>();
            trRt.anchorMin = new Vector2(0.06f, yPos + 0.10f);
            trRt.anchorMax = new Vector2(0.55f, yPos + 0.22f);

            var s = CreateText(parent, sub, 10, new Color32(0xa5, 0xb4, 0xfc, 0xcc), TextAlignmentOptions.MidlineLeft);
            var srRt = s.GetComponent<RectTransform>();
            srRt.anchorMin = new Vector2(0.06f, yPos);
            srRt.anchorMax = new Vector2(0.55f, yPos + 0.10f);

            // Slider Track
            var track = CreatePill(parent, "Track", new Vector2(0.58f, yPos + 0.07f), new Vector2(0.95f, yPos + 0.13f),
                new Color32(0x1e, 0x1b, 0x4b, 0xdd), new Color32(0x63, 0x66, 0xf1, 0x66));

            // Slider Yellow 3D Knob
            var knob = new GameObject("Knob", typeof(RectTransform), typeof(Image));
            knob.transform.SetParent(track, false);
            var kRt = knob.GetComponent<RectTransform>();
            kRt.anchorMin = new Vector2(val, 0.5f);
            kRt.anchorMax = new Vector2(val, 0.5f);
            kRt.sizeDelta = new Vector2(22f, 22f);
            var kImg = knob.GetComponent<Image>();
            kImg.sprite = SpriteHelper.GetSliderKnob();
        }
        #endregion

        #region Screen 7: Active Gameplay HUD
        private void BuildGameplayHud(Transform parent)
        {
            var topBar = CreateRect(parent, "TopBar", new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.985f));

            // White 3D Circular Pause Button
            CreateCircleButton3D(topBar, "Btn_Pause", "emoji_3d_pause", new Vector2(0f, 0f), new Vector2(0.13f, 1f), () => TogglePause());

            // Unified Quota Capsule: dark glass pill, yellow LVL 1 badge, quota text
            var capsule = CreatePill(topBar, "UnifiedCapsule", new Vector2(0.16f, 0f), new Vector2(0.84f, 1f),
                new Color32(0x1e, 0x1b, 0x4b, 0xee), SpriteHelper.ColorBorderDark, 2.5f);

            // Yellow level pill badge inside capsule
            var lvlBadgePill = CreatePill(capsule, "LvlBadge", new Vector2(0.03f, 0.15f), new Vector2(0.35f, 0.85f),
                SpriteHelper.ColorYellowMain, SpriteHelper.ColorBorderDark, 1.5f);
            CreateEmojiImage(lvlBadgePill, "emoji_3d_star", new Vector2(14f, 14f), new Vector2(0.24f, 0.5f));
            hudLevelBadge = CreateText(lvlBadgePill, "LVL 1", 11, SpriteHelper.ColorBorderDark, TextAlignmentOptions.MidlineLeft);
            hudLevelBadge.fontStyle = FontStyles.Bold;
            var hlbRt = hudLevelBadge.GetComponent<RectTransform>();
            hlbRt.anchorMin = new Vector2(0.44f, 0f);
            hlbRt.anchorMax = new Vector2(0.96f, 1f);

            // Quota Progress Bar track in center
            var track = CreatePill(capsule, "ProgressBarTrack", new Vector2(0.38f, 0.38f), new Vector2(0.68f, 0.62f),
                new Color32(0x0f, 0x17, 0x2a, 0xee), new Color32(0x63, 0x66, 0xf1, 0x44), 1f);
            var fill = CreatePill(track, "ProgressBarFill", new Vector2(0f, 0f), new Vector2(0f, 1f),
                SpriteHelper.ColorCyanMain, Color.clear, 0f);
            hudQuotaBarFillRect = fill.GetComponent<RectTransform>();
            hudQuotaBarFillRect.offsetMin = Vector2.zero;
            hudQuotaBarFillRect.offsetMax = Vector2.zero;

            hudQuotaText = CreateText(capsule, "0 / 20 kg", 12.5f, Color.white, TextAlignmentOptions.MidlineRight);
            hudQuotaText.fontStyle = FontStyles.Bold;
            var qRt = hudQuotaText.GetComponent<RectTransform>();
            qRt.anchorMin = new Vector2(0.70f, 0f);
            qRt.anchorMax = new Vector2(0.96f, 1f);

            // White 3D Circular Restart Button
            CreateCircleButton3D(topBar, "Btn_Restart", "emoji_3d_retry", new Vector2(0.87f, 0f), new Vector2(1f, 1f), () => {
                if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber);
            });
        }
        #endregion

        #region Screens 8, 9, 10: Modals
        private void BuildModals(Transform parent)
        {
            // Screen 8: Pause Modal (1:1 Web Replica: bottom sheet, max-width 365px in 430px frame)
            Transform pauseCardTr;
            pauseModal = CreateModalCard(parent, "Modal_Pause", 0.61f, 0.85f, 0.035f, out pauseCardTr);

            // Header row with Pause icon, Title, and Close button
            CreateEmojiImage(pauseCardTr, "emoji_3d_pause", new Vector2(22f, 22f), new Vector2(0.09f, 0.94f));
            var pTitle = CreateText(pauseCardTr, "MISSION PAUSED", 16.5f, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineLeft);
            pTitle.fontStyle = FontStyles.Bold;
            var ptRt = pTitle.GetComponent<RectTransform>();
            ptRt.anchorMin = new Vector2(0.18f, 0.89f);
            ptRt.anchorMax = new Vector2(0.80f, 0.98f);

            CreateCircleButton3D(pauseCardTr, "Btn_Close", "emoji_3d_cross_mark", new Vector2(0.85f, 0.89f), new Vector2(0.96f, 0.98f), () => TogglePause());

            // Mission info box with bold yellow LVL 1 and soft subtitle
            var infoBox = CreateSlicedCard(pauseCardTr, "LevelInfo", new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.87f),
                new Color32(0x27, 0x23, 0x69, 0xdd), new Color32(0x63, 0x66, 0xf1, 0x55), 2f, 16f);
            var infTitle = CreateText(infoBox, "LVL 1 • Bubblegum Chute 1", 13.5f, SpriteHelper.ColorYellowMain, TextAlignmentOptions.Center);
            infTitle.fontStyle = FontStyles.Bold;
            var itRt = infTitle.GetComponent<RectTransform>();
            itRt.anchorMin = new Vector2(0.02f, 0.48f);
            itRt.anchorMax = new Vector2(0.98f, 0.95f);

            var infGoal = CreateText(infoBox, "Goal: Feed Nibbles 20 kg (0/20 kg eaten)", 10.5f, new Color32(0xa5, 0xb4, 0xfc, 0xee), TextAlignmentOptions.Center);
            var igRt = infGoal.GetComponent<RectTransform>();
            igRt.anchorMin = new Vector2(0.02f, 0.05f);
            igRt.anchorMax = new Vector2(0.98f, 0.52f);

            // 5 3D Cartoon Pill Buttons (Compact 10px gaps, 1:1 Web CSS)
            // 1. RESUME PLAY (Yellow pill, dark text, 4.5px shadow)
            CreatePillButton3D(pauseCardTr, "Btn_Resume", "RESUME PLAY",
                SpriteHelper.ColorYellowMain, new Color32(0xf5, 0x9e, 0x0b, 0xff), SpriteHelper.ColorYellowShadow, SpriteHelper.ColorBorderDark,
                new Vector2(0.05f, 0.60f), new Vector2(0.95f, 0.71f), 14.5f, () => TogglePause(), "play_triangle", SpriteHelper.ColorBorderDark, 4.5f);

            // 2. Restart Sector (Secondary pill)
            CreatePillButton3D(pauseCardTr, "Btn_Restart", "Restart Sector",
                new Color32(0x43, 0x38, 0xca, 0xff), new Color32(0x37, 0x30, 0xa3, 0xff), SpriteHelper.ColorBorderDark, SpriteHelper.ColorBorderDark,
                new Vector2(0.05f, 0.47f), new Vector2(0.95f, 0.57f), 12.5f, () => {
                    TogglePause();
                    if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber);
                }, "emoji_3d_retry", Color.white, 4f);

            // 3. Sound Toggle (Secondary pill)
            CreatePillButton3D(pauseCardTr, "Btn_SoundToggle", "Sound: ON",
                new Color32(0x43, 0x38, 0xca, 0xff), new Color32(0x37, 0x30, 0xa3, 0xff), SpriteHelper.ColorBorderDark, SpriteHelper.ColorBorderDark,
                new Vector2(0.05f, 0.34f), new Vector2(0.95f, 0.44f), 12.5f, () => {}, "emoji_3d_sound_on", Color.white, 4f);

            // 4. Choose Sector (Secondary pill)
            CreatePillButton3D(pauseCardTr, "Btn_ChooseSector", "Choose Sector",
                new Color32(0x43, 0x38, 0xca, 0xff), new Color32(0x37, 0x30, 0xa3, 0xff), SpriteHelper.ColorBorderDark, SpriteHelper.ColorBorderDark,
                new Vector2(0.05f, 0.21f), new Vector2(0.95f, 0.31f), 12.5f, () => {
                    TogglePause();
                    ShowMaps();
                }, "emoji_3d_map", Color.white, 4f);

            // 5. Return to Home (Distinct pink #f472b6 border & text)
            CreatePillButton3D(pauseCardTr, "Btn_ReturnHome", "Return to Home",
                new Color32(0x27, 0x23, 0x69, 0xff), new Color32(0x1e, 0x1b, 0x4b, 0xff), new Color32(0x9d, 0x17, 0x4d, 0xff), new Color32(0xf4, 0x72, 0xb6, 0xff),
                new Vector2(0.05f, 0.07f), new Vector2(0.95f, 0.17f), 12.5f, () => {
                    TogglePause();
                    ShowHome();
                }, "emoji_3d_home", SpriteHelper.ColorPinkMain, 4f);


            // Screen 9: Victory Debrief Modal (Bottom sheet 1:1 Web Replica)
            Transform vicCardTr;
            victoryModal = CreateModalCard(parent, "Modal_Victory", 0.57f, 0.92f, 0.035f, out vicCardTr);

            // Yellow celebratory header banner
            var vicHeader = CreateSlicedCard(vicCardTr, "VicHeader", new Vector2(0f, 0.85f), new Vector2(1f, 1f),
                SpriteHelper.ColorYellowMain, SpriteHelper.ColorBorderDark, 2.5f, 24f);
            CreateEmojiImage(vicHeader, "emoji_3d_party_popper", new Vector2(22f, 22f), new Vector2(0.12f, 0.52f));
            var vhTxt = CreateText(vicHeader, "NIBBLES IS FULL! YAY!", 15, SpriteHelper.ColorBorderDark, TextAlignmentOptions.Center);
            vhTxt.fontStyle = FontStyles.Bold;

            // Celebratory Emoji Row (Donut, Nom Nom face, Sparkles)
            CreateEmojiImage(vicCardTr, "emoji_3d_donut", new Vector2(40f, 40f), new Vector2(0.32f, 0.73f));
            CreateEmojiImage(vicCardTr, "emoji_3d_nom_nom", new Vector2(46f, 46f), new Vector2(0.50f, 0.73f));
            CreateEmojiImage(vicCardTr, "emoji_3d_sparkles", new Vector2(40f, 40f), new Vector2(0.68f, 0.73f));

            victoryStatsText = CreateText(vicCardTr, "Awesome job! The space orbit is sparkling clean and trash-free!", 11.5f, Color.white, TextAlignmentOptions.Center);
            var vsRt = victoryStatsText.GetComponent<RectTransform>();
            vsRt.anchorMin = new Vector2(0.06f, 0.56f);
            vsRt.anchorMax = new Vector2(0.94f, 0.67f);

            // Two Stat Boxes: Snacks Eaten (45 kg) and Coins Won (+250)
            var sBox1 = CreateSlicedCard(vicCardTr, "StatBox1", new Vector2(0.05f, 0.28f), new Vector2(0.48f, 0.52f),
                new Color32(0x27, 0x23, 0x69, 0xdd), new Color32(0x63, 0x66, 0xf1, 0x55), 1.5f, 16f);
            CreateText(sBox1, "SNACKS EATEN", 9, new Color32(0xa5, 0xb4, 0xfc, 0xdd), TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
            var sb1Val = CreateText(sBox1, "45 kg", 18, SpriteHelper.ColorYellowMain, TextAlignmentOptions.Center);
            sb1Val.fontStyle = FontStyles.Bold;
            victoryMassText = sb1Val;
            var sb1Rt = sb1Val.GetComponent<RectTransform>();
            sb1Rt.anchorMin = new Vector2(0f, 0.1f);
            sb1Rt.anchorMax = new Vector2(1f, 0.6f);

            var sBox2 = CreateSlicedCard(vicCardTr, "StatBox2", new Vector2(0.52f, 0.28f), new Vector2(0.95f, 0.52f),
                new Color32(0x27, 0x23, 0x69, 0xdd), new Color32(0x63, 0x66, 0xf1, 0x55), 1.5f, 16f);
            CreateText(sBox2, "COINS WON", 9, new Color32(0xa5, 0xb4, 0xfc, 0xdd), TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
            var sb2Val = CreateText(sBox2, "+250", 18, SpriteHelper.ColorYellowMain, TextAlignmentOptions.MidlineRight);
            sb2Val.fontStyle = FontStyles.Bold;
            victoryCreditsText = sb2Val;
            var sb2Rt = sb2Val.GetComponent<RectTransform>();
            sb2Rt.anchorMin = new Vector2(0.05f, 0.1f);
            sb2Rt.anchorMax = new Vector2(0.68f, 0.6f);
            CreateEmojiImage(sBox2, "emoji_3d_coin", new Vector2(18f, 18f), new Vector2(0.79f, 0.35f));

            // Two Bottom Action Buttons: Play Again & 3D Yellow Next Map (Pill style)
            CreatePillButton3D(vicCardTr, "Btn_PlayAgain", "Play Again",
                new Color32(0x43, 0x38, 0xca, 0xff), new Color32(0x37, 0x30, 0xa3, 0xff), SpriteHelper.ColorBorderDark, SpriteHelper.ColorBorderDark,
                new Vector2(0.05f, 0.06f), new Vector2(0.48f, 0.20f), 13.5f, () => {
                    victoryModal.SetActive(false);
                    if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber);
                }, null, Color.white, 3.5f);

            CreatePillButton3D(vicCardTr, "Btn_NextMap", "Next Map",
                SpriteHelper.ColorYellowMain, new Color32(0xf5, 0x9e, 0x0b, 0xff), SpriteHelper.ColorYellowShadow, SpriteHelper.ColorBorderDark,
                new Vector2(0.52f, 0.06f), new Vector2(0.95f, 0.20f), 13.5f, () => {
                    victoryModal.SetActive(false);
                    if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber + 1);
                }, "play_triangle_right", SpriteHelper.ColorBorderDark, 3.5f);


            // Screen 10: Defeat Modal (Bottom sheet 1:1 Web Replica)
            Transform defCardTr;
            defeatModal = CreateModalCard(parent, "Modal_Defeat", 0.57f, 0.92f, 0.035f, out defCardTr);

            // Coral Header Banner
            var defHeader = CreateSlicedCard(defCardTr, "DefHeader", new Vector2(0f, 0.85f), new Vector2(1f, 1f),
                new Color32(0xf8, 0x71, 0x71, 0xff), SpriteHelper.ColorBorderDark, 2.5f, 24f);
            CreateEmojiImage(defHeader, "emoji_3d_pleading_face", new Vector2(22f, 22f), new Vector2(0.12f, 0.52f));
            var dhTxt = CreateText(defHeader, "SHIFT INCOMPLETE!", 15, SpriteHelper.ColorBorderDark, TextAlignmentOptions.Center);
            dhTxt.fontStyle = FontStyles.Bold;

            CreateEmojiImage(defCardTr, "emoji_3d_pleading_face", new Vector2(40f, 40f), new Vector2(0.32f, 0.73f));
            CreateEmojiImage(defCardTr, "emoji_3d_bowl", new Vector2(40f, 40f), new Vector2(0.50f, 0.73f));
            CreateEmojiImage(defCardTr, "emoji_3d_sparkles", new Vector2(40f, 40f), new Vector2(0.68f, 0.73f));

            defeatStatsText = CreateText(defCardTr, "Nibbles is still hungry! Out of orbital scrap!", 11.5f, Color.white, TextAlignmentOptions.Center);
            var dfRt = defeatStatsText.GetComponent<RectTransform>();
            dfRt.anchorMin = new Vector2(0.06f, 0.56f);
            dfRt.anchorMax = new Vector2(0.94f, 0.67f);

            var dBox1 = CreateSlicedCard(defCardTr, "DefBox1", new Vector2(0.05f, 0.28f), new Vector2(0.48f, 0.52f),
                new Color32(0x27, 0x23, 0x69, 0xdd), new Color32(0x63, 0x66, 0xf1, 0x55), 1.5f, 16f);
            CreateText(dBox1, "SNACKS COLLECTED", 8.5f, new Color32(0xa5, 0xb4, 0xfc, 0xdd), TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
            var db1Val = CreateText(dBox1, "12 / 20 kg", 16, SpriteHelper.ColorYellowMain, TextAlignmentOptions.Center);
            db1Val.fontStyle = FontStyles.Bold;
            defeatMassText = db1Val;
            var db1Rt = db1Val.GetComponent<RectTransform>();
            db1Rt.anchorMin = new Vector2(0f, 0.1f);
            db1Rt.anchorMax = new Vector2(1f, 0.6f);

            var dBox2 = CreateSlicedCard(defCardTr, "DefBox2", new Vector2(0.52f, 0.28f), new Vector2(0.95f, 0.52f),
                new Color32(0x27, 0x23, 0x69, 0xdd), new Color32(0x63, 0x66, 0xf1, 0x55), 1.5f, 16f);
            CreateText(dBox2, "LOST IN VOID", 8.5f, new Color32(0xa5, 0xb4, 0xfc, 0xdd), TextAlignmentOptions.Center).fontStyle = FontStyles.Bold;
            var db2Val = CreateText(dBox2, "4 kg", 16, new Color32(0xf8, 0x71, 0x71, 0xff), TextAlignmentOptions.Center);
            db2Val.fontStyle = FontStyles.Bold;
            defeatLostText = db2Val;
            var db2Rt = db2Val.GetComponent<RectTransform>();
            db2Rt.anchorMin = new Vector2(0f, 0.1f);
            db2Rt.anchorMax = new Vector2(1f, 0.6f);

            var backupBtn = CreatePillButton3D(defCardTr, "Btn_BackupPod", "Launch Backup Pod (+8)",
                new Color32(0x06, 0xb6, 0xd4, 0xff), new Color32(0x3b, 0x82, 0xf6, 0xff), new Color32(0x02, 0x84, 0xc7, 0xff), SpriteHelper.ColorBorderDark,
                new Vector2(0.05f, 0.21f), new Vector2(0.95f, 0.27f), 11.5f, () => {
                    defeatModal.SetActive(false);
                    GameManager.Instance?.LaunchBackupPod();
                }, "emoji_3d_rocket", Color.white, 3f);
            defeatBackupButton = backupBtn.gameObject;
            defeatBackupButton.SetActive(false);

            var hangarBtn = CreatePillButton3D(defCardTr, "Btn_Hangar", "Hangar",
                new Color32(0x43, 0x38, 0xca, 0xff), new Color32(0x37, 0x30, 0xa3, 0xff), SpriteHelper.ColorBorderDark, SpriteHelper.ColorBorderDark,
                new Vector2(0.05f, 0.06f), new Vector2(0.48f, 0.20f), 13.5f, () => {
                    defeatModal.SetActive(false);
                    ShowHome();
                }, "emoji_3d_home", Color.white, 3.5f);
            defeatHangarRt = hangarBtn.GetComponent<RectTransform>();

            var tryAgainBtn = CreatePillButton3D(defCardTr, "Btn_TryAgain", "Try Again",
                SpriteHelper.ColorYellowMain, new Color32(0xf5, 0x9e, 0x0b, 0xff), SpriteHelper.ColorYellowShadow, SpriteHelper.ColorBorderDark,
                new Vector2(0.52f, 0.06f), new Vector2(0.95f, 0.20f), 13.5f, () => {
                    defeatModal.SetActive(false);
                    if (GameManager.Instance != null) StartSector(GameManager.Instance.CurrentLevelNumber);
                }, "emoji_3d_retry", SpriteHelper.ColorBorderDark, 3.5f);
            defeatTryAgainRt = tryAgainBtn.GetComponent<RectTransform>();

            pauseModal.SetActive(false);
            victoryModal.SetActive(false);
            defeatModal.SetActive(false);
        }

        private GameObject CreateModalCard(Transform parent, string name, float heightFrac, float widthFrac, float bottomY, out Transform cardTransform)
        {
            // Dark blurred backdrop overlay (full-bleed)
            var overlay = CreateRect(parent, name, Vector2.zero, Vector2.one);
            var oImg = overlay.gameObject.AddComponent<Image>();
            oImg.color = new Color32(0x0f, 0x0c, 0x29, 0xb8);

            var safeGo = new GameObject("SafeContent", typeof(RectTransform));
            safeGo.transform.SetParent(overlay, false);
            var safeRt = safeGo.GetComponent<RectTransform>();
            safeRt.anchorMin = Vector2.zero;
            safeRt.anchorMax = Vector2.one;
            safeRt.sizeDelta = Vector2.zero;
            safeGo.AddComponent<SafeAreaFitter>();

            // Bottom-anchored modal sheet card (1:1 Web Replica) inside safe area
            float halfW = widthFrac * 0.5f;
            var card = CreateSlicedCard(safeGo.transform, "ModalCard",
                new Vector2(0.5f - halfW, bottomY), new Vector2(0.5f + halfW, bottomY + heightFrac),
                new Color32(0x31, 0x2e, 0x81, 0xfa), SpriteHelper.ColorBorderDark, 3.5f, 28f);

            cardTransform = card.transform;
            return overlay.gameObject;
        }
        #endregion

        #region Screen 13: Loading Screen (1:1 Web Replica)
        private void BuildLoadingScreen(Transform parent)
        {
            // Radial background deep cosmic gradient #131133
            var bg = CreateSlicedCard(parent, "LoadingBackground", Vector2.zero, Vector2.one,
                new Color32(0x13, 0x11, 0x33, 0xff), Color.clear, 0f, 0f);

            // Centered layout container
            var centerBox = CreateRect(parent, "CenterBox", new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.78f));

            // Animated Mascot Donut (72px, tilted 6 degrees)
            var mascot = CreateEmojiImage(centerBox, "emoji_3d_donut", new Vector2(90f, 90f), new Vector2(0.5f, 0.74f));
            mascot.transform.localRotation = Quaternion.Euler(0f, 0f, -6f);

            // 3D Extruded Title: ORBITAL SALVAGE (bold, yellow #facc15, dark shadow)
            Create3DTitleText(centerBox, "ORBITAL SALVAGE", 26, SpriteHelper.ColorYellowMain,
                new Color[] {
                    new Color32(0xca, 0x8a, 0x04, 0xff), // #ca8a04
                    new Color32(0x78, 0x35, 0x0f, 0xff), // #78350f
                    new Color32(0x1e, 0x1b, 0x4b, 0xff)  // #1e1b4b
                },
                new Vector2(0f, 0.50f), new Vector2(1f, 0.62f));

            // Subtitle: CUTE COSMIC JANITOR (13px, pink #f472b6, uppercase)
            var sub = CreateText(centerBox, "CUTE COSMIC JANITOR", 13, SpriteHelper.ColorPinkMain, TextAlignmentOptions.Center);
            sub.fontStyle = FontStyles.Bold;
            var sRt = sub.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0f, 0.42f);
            sRt.anchorMax = new Vector2(1f, 0.49f);

            // Progress Bar Wrapper (270x22px, border 3px #1e1b4b, dark fill)
            var barWrapper = CreateRect(centerBox, "ProgressBarWrapper", new Vector2(0.5f, 0.32f), new Vector2(0.5f, 0.32f));
            var bwRt = barWrapper.GetComponent<RectTransform>();
            bwRt.sizeDelta = new Vector2(270f, 22f);
            var bwImg = barWrapper.gameObject.AddComponent<Image>();
            bwImg.sprite = SpriteHelper.GetPillBadge(new Color32(0x0f, 0x17, 0x2a, 0xee), SpriteHelper.ColorBorderDark, 3f);
            bwImg.type = Image.Type.Sliced;

            // Progress Bar Fill (Gradient yellow to green, 68% fill)
            var barFill = CreateRect(barWrapper, "ProgressBarFill", new Vector2(0f, 0f), new Vector2(0.68f, 1f));
            var bfRt = barFill.GetComponent<RectTransform>();
            bfRt.offsetMin = new Vector2(3f, 3f);
            bfRt.offsetMax = new Vector2(-3f, -3f);
            var fillImg = barFill.gameObject.AddComponent<Image>();
            fillImg.sprite = SpriteHelper.GetGradientPill(SpriteHelper.ColorYellowMain, SpriteHelper.ColorGreenMain);
            fillImg.type = Image.Type.Sliced;
            fillImg.color = Color.white;

            // Tip Text: Calibrating cosmic centrifuges...
            var tip = CreateText(centerBox, "Calibrating cosmic centrifuges...", 12, new Color(1f, 1f, 1f, 0.75f), TextAlignmentOptions.Center);
            tip.fontStyle = FontStyles.Bold;
            var tipRt = tip.GetComponent<RectTransform>();
            tipRt.anchorMin = new Vector2(0f, 0.18f);
            tipRt.anchorMax = new Vector2(1f, 0.26f);
        }
        #endregion

        #endregion

        #region Primitives & Helper Factories (100% Sliced 9-Slice & 3D Cartoon)
        private void CreateHeaderBarWithCoins(Transform parent, string title, string iconEmoji, Action onBack)
        {
            CreateHeaderBar(parent, title, iconEmoji, onBack, true);
        }

        private void CreateHeaderBar(Transform parent, string title, string iconEmoji, Action onBack, bool showCoins = true)
        {
            var header = CreateRect(parent, "HeaderBar", new Vector2(0.04f, 0.925f), new Vector2(0.96f, 0.985f));

            // 3D Cartoon Pill Back Button
            CreatePillButton3D(header, "Btn_Back", "← Back",
                new Color32(0x3e, 0x3a, 0x75, 0xee), new Color32(0x31, 0x2e, 0x81, 0xee), new Color32(0x1e, 0x1b, 0x4b, 0xee), SpriteHelper.ColorBorderDark,
                new Vector2(0f, 0.05f), new Vector2(0.24f, 0.95f), 12.5f, onBack, null, Color.white, 3f);

            // Title with Emoji Icon
            CreateEmojiImage(header, iconEmoji, new Vector2(24f, 24f), new Vector2(0.29f, 0.5f));
            var titleTxt = CreateText(header, title, 16, Color.white, TextAlignmentOptions.MidlineLeft);
            titleTxt.fontStyle = FontStyles.Bold;
            var tRt = titleTxt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0.36f, 0f);
            tRt.anchorMax = showCoins ? new Vector2(0.74f, 1f) : new Vector2(0.98f, 1f);

            // Coin Pill on Right (only when showCoins == true)
            if (showCoins)
            {
                var coinPill = CreatePill(header, "CoinPill", new Vector2(0.74f, 0.05f), new Vector2(1f, 0.95f),
                    new Color32(0x1e, 0x1b, 0x4b, 0xdd), new Color32(0x43, 0x38, 0xca, 0x88), 1.5f);
                CreateEmojiImage(coinPill, "emoji_3d_coin", new Vector2(20f, 20f), new Vector2(0.22f, 0.5f));
                var cTxt = CreateText(coinPill, "250", 14, Color.white, TextAlignmentOptions.MidlineLeft);
                cTxt.fontStyle = FontStyles.Bold;
                headerCreditsTexts.Add(cTxt);
                if (mapsCreditsText == null) mapsCreditsText = cTxt;
                var ctRt = cTxt.GetComponent<RectTransform>();
                ctRt.anchorMin = new Vector2(0.48f, 0f);
                ctRt.anchorMax = new Vector2(0.95f, 1f);
            }
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

        private Transform CreateSlicedCard(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color fillColor, Color borderColor, float borderWidth = 2.5f, float radius = 22f)
        {
            var tr = CreateRect(parent, name, anchorMin, anchorMax);
            var img = tr.gameObject.AddComponent<Image>();
            img.sprite = SpriteHelper.Get9SliceCard(fillColor, borderColor, borderWidth, radius);
            img.type = Image.Type.Sliced;
            return tr;
        }

        private Transform CreatePill(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color fillColor, Color borderColor, float borderWidth = 1.8f)
        {
            var tr = CreateRect(parent, name, anchorMin, anchorMax);
            var img = tr.gameObject.AddComponent<Image>();
            img.sprite = SpriteHelper.GetPillBadge(fillColor, borderColor, borderWidth);
            img.type = Image.Type.Sliced;
            return tr;
        }

        private Button CreateButton3D(Transform parent, string name, string label, Color topCol, Color botCol, Color shadowCol, Color borderCol, Vector2 anchorMin, Vector2 anchorMax, int fontSize = 14, Action onClick = null, string emojiIcon = null, Color? textColor = null)
        {
            var tr = CreateRect(parent, name, anchorMin, anchorMax);
            var img = tr.gameObject.AddComponent<Image>();
            img.sprite = SpriteHelper.Get9SliceButton3D(topCol, botCol, shadowCol, borderCol);
            img.type = Image.Type.Sliced;

            var btn = tr.gameObject.AddComponent<Button>();
            tr.gameObject.AddComponent<UIBounceButton>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            Color tCol = textColor ?? Color.white;

            if (emojiIcon == "play_triangle")
            {
                var triGo = new GameObject("PlayIcon", typeof(RectTransform), typeof(Image));
                triGo.transform.SetParent(tr, false);
                var trRt = triGo.GetComponent<RectTransform>();
                trRt.anchorMin = new Vector2(0.22f, 0.52f);
                trRt.anchorMax = new Vector2(0.22f, 0.52f);
                trRt.sizeDelta = new Vector2(fontSize, fontSize);
                var trImg = triGo.GetComponent<Image>();
                trImg.sprite = SpriteHelper.GetPlayTriangleSprite();
                trImg.color = tCol;

                var txt = CreateText(tr, label, fontSize, tCol, TextAlignmentOptions.MidlineLeft);
                txt.fontStyle = FontStyles.Bold;
                var txtRt = txt.GetComponent<RectTransform>();
                txtRt.anchorMin = new Vector2(0.32f, 0f);
                txtRt.anchorMax = new Vector2(0.95f, 1f);
            }
            else if (emojiIcon == "play_triangle_right")
            {
                var txt = CreateText(tr, label, fontSize, tCol, TextAlignmentOptions.MidlineLeft);
                txt.fontStyle = FontStyles.Bold;
                var txtRt = txt.GetComponent<RectTransform>();
                txtRt.anchorMin = new Vector2(0.20f, 0f);
                txtRt.anchorMax = new Vector2(0.76f, 1f);

                var triGo = new GameObject("PlayIcon", typeof(RectTransform), typeof(Image));
                triGo.transform.SetParent(tr, false);
                var trRt = triGo.GetComponent<RectTransform>();
                trRt.anchorMin = new Vector2(0.80f, 0.52f);
                trRt.anchorMax = new Vector2(0.80f, 0.52f);
                trRt.sizeDelta = new Vector2(fontSize, fontSize);
                var trImg = triGo.GetComponent<Image>();
                trImg.sprite = SpriteHelper.GetPlayTriangleSprite();
                trImg.color = tCol;
            }
            else if (!string.IsNullOrEmpty(emojiIcon))
            {
                CreateEmojiImage(tr, emojiIcon, new Vector2(fontSize + 6f, fontSize + 6f), new Vector2(0.18f, 0.52f));
                var txt = CreateText(tr, label, fontSize, tCol, TextAlignmentOptions.Center);
                txt.fontStyle = FontStyles.Bold;
                var txtRt = txt.GetComponent<RectTransform>();
                txtRt.anchorMin = new Vector2(0.20f, 0f);
                txtRt.anchorMax = new Vector2(0.95f, 1f);
            }
            else
            {
                var txt = CreateText(tr, label, fontSize, tCol, TextAlignmentOptions.Center);
                txt.fontStyle = FontStyles.Bold;
            }

            return btn;
        }

        private Button CreatePillButton3D(Transform parent, string name, string label, Color topCol, Color botCol, Color shadowCol, Color borderCol, Vector2 anchorMin, Vector2 anchorMax, float fontSize = 13f, Action onClick = null, string emojiIcon = null, Color? textColor = null, float bevelHeight = 4.5f)
        {
            var tr = CreateRect(parent, name, anchorMin, anchorMax);
            var img = tr.gameObject.AddComponent<Image>();
            img.sprite = SpriteHelper.GetPillButton3D(topCol, botCol, shadowCol, borderCol, bevelHeight);
            img.type = Image.Type.Sliced;

            var btn = tr.gameObject.AddComponent<Button>();
            tr.gameObject.AddComponent<UIBounceButton>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            Color tCol = textColor ?? Color.white;

            if (emojiIcon == "play_triangle")
            {
                var triGo = new GameObject("PlayIcon", typeof(RectTransform), typeof(Image));
                triGo.transform.SetParent(tr, false);
                var trRt = triGo.GetComponent<RectTransform>();
                trRt.anchorMin = new Vector2(0.24f, 0.52f);
                trRt.anchorMax = new Vector2(0.24f, 0.52f);
                trRt.sizeDelta = new Vector2(fontSize + 1f, fontSize + 1f);
                var trImg = triGo.GetComponent<Image>();
                trImg.sprite = SpriteHelper.GetPlayTriangleSprite();
                trImg.color = tCol;

                var txt = CreateText(tr, label, fontSize, tCol, TextAlignmentOptions.MidlineLeft);
                txt.fontStyle = FontStyles.Bold;
                var txtRt = txt.GetComponent<RectTransform>();
                txtRt.anchorMin = new Vector2(0.36f, 0f);
                txtRt.anchorMax = new Vector2(0.95f, 1f);
            }
            else if (emojiIcon == "play_triangle_right")
            {
                var rowGo = new GameObject("ContentRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                rowGo.transform.SetParent(tr, false);
                var rowRt = rowGo.GetComponent<RectTransform>();
                rowRt.anchorMin = Vector2.zero;
                rowRt.anchorMax = Vector2.one;
                rowRt.offsetMin = Vector2.zero;
                rowRt.offsetMax = Vector2.zero;

                var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.spacing = 6f;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                var txt = CreateText(rowGo.transform, label, fontSize, tCol, TextAlignmentOptions.MidlineLeft);
                txt.fontStyle = FontStyles.Bold;
                txt.textWrappingMode = TextWrappingModes.NoWrap;
                var leTxt = txt.gameObject.AddComponent<LayoutElement>();
                leTxt.preferredWidth = txt.preferredWidth;
                leTxt.preferredHeight = fontSize + 8f;

                var triGo = new GameObject("PlayIcon", typeof(RectTransform), typeof(Image));
                triGo.transform.SetParent(rowGo.transform, false);
                var trImg = triGo.GetComponent<Image>();
                trImg.sprite = SpriteHelper.GetPlayTriangleSprite();
                trImg.color = tCol;
                var leTri = triGo.AddComponent<LayoutElement>();
                leTri.preferredWidth = fontSize;
                leTri.preferredHeight = fontSize;
            }
            else if (emojiIcon == "coin_right")
            {
                var rowGo = new GameObject("ContentRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                rowGo.transform.SetParent(tr, false);
                var rowRt = rowGo.GetComponent<RectTransform>();
                rowRt.anchorMin = Vector2.zero;
                rowRt.anchorMax = Vector2.one;
                rowRt.offsetMin = Vector2.zero;
                rowRt.offsetMax = Vector2.zero;

                var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.spacing = 5f;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                var txt = CreateText(rowGo.transform, label, fontSize, tCol, TextAlignmentOptions.MidlineLeft);
                txt.fontStyle = FontStyles.Bold;
                txt.textWrappingMode = TextWrappingModes.NoWrap;
                var leTxt = txt.gameObject.AddComponent<LayoutElement>();
                leTxt.preferredWidth = txt.preferredWidth;
                leTxt.preferredHeight = fontSize + 8f;

                var coin = CreateEmojiImage(rowGo.transform, "emoji_3d_coin", new Vector2(fontSize + 3f, fontSize + 3f));
                var leCoin = coin.AddComponent<LayoutElement>();
                leCoin.preferredWidth = fontSize + 3f;
                leCoin.preferredHeight = fontSize + 3f;
            }
            else if (emojiIcon == "coin_inside_paren")
            {
                var rowGo = new GameObject("ContentRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                rowGo.transform.SetParent(tr, false);
                var rowRt = rowGo.GetComponent<RectTransform>();
                rowRt.anchorMin = Vector2.zero;
                rowRt.anchorMax = Vector2.one;
                rowRt.offsetMin = Vector2.zero;
                rowRt.offsetMax = Vector2.zero;

                var hlg = rowGo.GetComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.spacing = 3f;
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;

                var txt = CreateText(rowGo.transform, label, fontSize, tCol, TextAlignmentOptions.MidlineLeft);
                txt.fontStyle = FontStyles.Bold;
                txt.textWrappingMode = TextWrappingModes.NoWrap;
                var leTxt = txt.gameObject.AddComponent<LayoutElement>();
                leTxt.preferredWidth = txt.preferredWidth;
                leTxt.preferredHeight = fontSize + 8f;

                var coin = CreateEmojiImage(rowGo.transform, "emoji_3d_coin", new Vector2(fontSize + 2f, fontSize + 2f));
                var leCoin = coin.AddComponent<LayoutElement>();
                leCoin.preferredWidth = fontSize + 2f;
                leCoin.preferredHeight = fontSize + 2f;

                var rParen = CreateText(rowGo.transform, ")", fontSize, tCol, TextAlignmentOptions.MidlineLeft);
                rParen.fontStyle = FontStyles.Bold;
                rParen.textWrappingMode = TextWrappingModes.NoWrap;
                var leParen = rParen.gameObject.AddComponent<LayoutElement>();
                leParen.preferredWidth = rParen.preferredWidth;
                leParen.preferredHeight = fontSize + 8f;
            }
            else if (!string.IsNullOrEmpty(emojiIcon))
            {
                CreateEmojiImage(tr, emojiIcon, new Vector2(fontSize + 6f, fontSize + 6f), new Vector2(0.20f, 0.52f));
                var txt = CreateText(tr, label, fontSize, tCol, TextAlignmentOptions.MidlineLeft);
                txt.fontStyle = FontStyles.Bold;
                var txtRt = txt.GetComponent<RectTransform>();
                txtRt.anchorMin = new Vector2(0.34f, 0f);
                txtRt.anchorMax = new Vector2(0.95f, 1f);
            }
            else
            {
                var txt = CreateText(tr, label, fontSize, tCol, TextAlignmentOptions.Center);
                txt.fontStyle = FontStyles.Bold;
            }

            return btn;
        }

        private GameObject CreateCircleButton3D(Transform parent, string name, string emojiName, Vector2 anchorMin, Vector2 anchorMax, Action onClick)
        {
            var tr = CreateRect(parent, name, anchorMin, anchorMax);
            var img = tr.gameObject.AddComponent<Image>();
            img.sprite = SpriteHelper.GetWhiteCircleButton3D();
            img.preserveAspect = true;

            var btn = tr.gameObject.AddComponent<Button>();
            tr.gameObject.AddComponent<UIBounceButton>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            CreateEmojiImage(tr, emojiName, new Vector2(24f, 24f), new Vector2(0.5f, 0.54f));
            return tr.gameObject;
        }

        private void CreateDockTab(Transform parent, string label, string emojiName, float xMin, float xMax, string badge, Action onClick)
        {
            var tr = CreateRect(parent, $"Tab_{label}", new Vector2(xMin, 0f), new Vector2(xMax, 1f));
            var btn = tr.gameObject.AddComponent<Button>();
            tr.gameObject.AddComponent<UIBounceButton>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            // 3D Emoji Icon
            CreateEmojiImage(tr, emojiName, new Vector2(26f, 26f), new Vector2(0.5f, 0.64f));

            // Label
            var txt = CreateText(tr, label, 11, new Color32(0xa5, 0xb4, 0xfc, 0xee), TextAlignmentOptions.Center);
            txt.fontStyle = FontStyles.Bold;
            var tRt = txt.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0f, 0.05f);
            tRt.anchorMax = new Vector2(1f, 0.38f);

            // Badge if specified
            if (!string.IsNullOrEmpty(badge))
            {
                Color bCol = badge == "HOT" || badge == "3 NEW" ? new Color32(0xf8, 0x71, 0x71, 0xff) : new Color32(0x4a, 0xde, 0x80, 0xff);
                var bPill = CreatePill(tr, "Badge", new Vector2(0.52f, 0.65f), new Vector2(0.96f, 0.96f), bCol, SpriteHelper.ColorBorderDark, 1.2f);
                var bTxt = CreateText(bPill, badge, 8, Color.white, TextAlignmentOptions.Center);
                bTxt.fontStyle = FontStyles.Bold;
            }
        }

        private GameObject CreateEmojiImage(Transform parent, string emojiName, Vector2 size)
        {
            return SpriteHelper.CreateEmojiImage(parent, emojiName, size);
        }

        private GameObject CreateEmojiImage(Transform parent, string emojiName, Vector2 size, Vector2 anchor)
        {
            var go = SpriteHelper.CreateEmojiImage(parent, emojiName, size);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = Vector2.zero;
            return go;
        }

        private TextMeshProUGUI CreateText(Transform parent, string content, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }

        private GameObject Create3DTitleText(Transform parent, string content, float fontSize, Color faceColor, Color[] shadowColors, Vector2 anchorMin, Vector2 anchorMax)
        {
            var root = new GameObject($"Title3D_{content}", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = Vector2.zero;

            if (shadowColors != null)
            {
                for (int i = shadowColors.Length - 1; i >= 0; i--)
                {
                    float yOffset = -(i + 1) * 2.2f;
                    var sGo = new GameObject($"Shadow_{i}", typeof(RectTransform), typeof(TextMeshProUGUI));
                    sGo.transform.SetParent(root.transform, false);
                    var sRt = sGo.GetComponent<RectTransform>();
                    sRt.anchorMin = Vector2.zero;
                    sRt.anchorMax = Vector2.one;
                    sRt.offsetMin = new Vector2(0f, yOffset);
                    sRt.offsetMax = new Vector2(0f, yOffset);

                    var sTmp = sGo.GetComponent<TextMeshProUGUI>();
                    sTmp.text = content;
                    sTmp.fontSize = fontSize;
                    sTmp.fontStyle = FontStyles.Bold;
                    sTmp.color = shadowColors[i];
                    sTmp.alignment = TextAlignmentOptions.Center;
                    sTmp.raycastTarget = false;
                }
            }

            var fGo = new GameObject("FaceText", typeof(RectTransform), typeof(TextMeshProUGUI));
            fGo.transform.SetParent(root.transform, false);
            var fRt = fGo.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;

            var fTmp = fGo.GetComponent<TextMeshProUGUI>();
            fTmp.text = content;
            fTmp.fontSize = fontSize;
            fTmp.fontStyle = FontStyles.Bold;
            fTmp.color = faceColor;
            fTmp.alignment = TextAlignmentOptions.Center;
            fTmp.raycastTarget = false;

            return root;
        }
        #endregion
    }
}
