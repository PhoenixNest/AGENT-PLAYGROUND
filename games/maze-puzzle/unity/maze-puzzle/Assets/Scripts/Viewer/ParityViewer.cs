using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OrbitalSalvage.Render;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Play-mode view of the Canvas2D pages. Starts on its own unless a parity capture
    /// (<c>--parity</c>, the same check as <c>ParityOptions.HasFlag</c>) is running.
    /// Opt out with the Editor menu or <c>-no-parity-viewer</c> (command line or the
    /// <c>NO_PARITY_VIEWER</c> / <c>-no-parity-viewer</c> environment variable).
    /// Keys: Left/Right or PageUp/PageDown cycle, 1-9 and 0 jump, Esc returns home, F1 toggles the hint.
    /// </summary>
    public sealed partial class ParityViewer : MonoBehaviour
    {
        const string SelfTestFlag = "-parity-viewer-selftest";
        const string LiveSelfTestFlag = "-parity-live-selftest";
        const string PerfFlag = "-parity-perf";
        const string AppSelfTestFlag = "-parity-app-selftest";
        const string ShipSelfTestFlag = "-ship-selftest";
        const string OptOutFlag = "-no-parity-viewer";
        const double QuotaTrackWidth = 162.328;

        static readonly KeyCode[] DigitKeys =
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
            KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
        };
        static readonly KeyCode[] PadKeys =
        {
            KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5,
            KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.Keypad0
        };

        readonly ChromeLru<HomeChrome> cache = new ChromeLru<HomeChrome>(ParityPages.CacheCapacity);
        readonly Dictionary<string, string> specs = new Dictionary<string, string>();
        readonly List<HomeChrome> evicted = new List<HomeChrome>(4);

        readonly UiClock clock = new UiClock();
        HomeChrome current;
        string currentId;
        string shownKey;
        LiveGameSession session;
        bool liveVisual = true;
        string lastQuota;
        int pressIndex = -1;
        string pressCss;
        NavEdge armedEdge;
        Texture2D black;
        GUIStyle hintStyle;
        bool hintVisible = true;
        bool selfTesting;
        bool shipTesting;
        string dragSlider;
        readonly Queue<PointerInject> pointerInject = new Queue<PointerInject>();
        GameObject eventSystem;
        RenderTexture readRt;
        Texture2D readTex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            var args = Environment.GetCommandLineArgs();
            Debug.Log("[ParityViewer] bootstrap " + string.Join(" ", args));
            if (!ShouldStart(args)) return;
            if (FindAnyObjectByType<ParityViewer>() != null) return;
            var go = new GameObject("ParityViewer");
            DontDestroyOnLoad(go);
            go.AddComponent<ParityViewer>();
        }

        /// <summary>False while parity capture owns the player, and when the viewer is opted out.</summary>
        public static bool ShouldStart(string[] args)
        {
            if (Array.IndexOf(args, "--parity") >= 0) return false;
            if (SelfTestDirectory(args) != null || LiveSelfTestDirectory(args) != null || PerfScenario(args) != null || AppSelfTestDirectory(args) != null || ShipSelfTestDirectory(args) != null) return true;
            if (IsOptOut(args)) return false;
#if UNITY_EDITOR
            if (!UnityEditor.EditorPrefs.GetBool(ParityPages.EnabledPrefKey, true)) return false;
#endif
            return true;
        }

        public static bool IsOptOut(string[] args)
        {
            if (Array.IndexOf(args, OptOutFlag) >= 0) return true;
            if (EnvOff("NO_PARITY_VIEWER")) return true;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(OptOutFlag))) return true;
            return false;
        }

        static bool EnvOff(string name)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(value)) return false;
            return value != "0" && !value.Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        public static string SelfTestDirectory(string[] args)
        {
            return ArgDirectory(args, SelfTestFlag);
        }

        public static string LiveSelfTestDirectory(string[] args)
        {
            return ArgDirectory(args, LiveSelfTestFlag);
        }

        /// <summary><c>-parity-perf &lt;home|live|fps-home|fps-game&gt; &lt;dir&gt;</c></summary>
        public static string PerfScenario(string[] args)
        {
            int i = Array.IndexOf(args, PerfFlag);
            if (i < 0 || i + 1 >= args.Length) return null;
            return args[i + 1];
        }

        public static string PerfDirectory(string[] args)
        {
            int i = Array.IndexOf(args, PerfFlag);
            if (i < 0 || i + 2 >= args.Length) return null;
            return args[i + 2];
        }

        public static string AppSelfTestDirectory(string[] args)
        {
            return ArgDirectory(args, AppSelfTestFlag);
        }

        public static string ShipSelfTestDirectory(string[] args)
        {
            return ArgDirectory(args, ShipSelfTestFlag);
        }

        /// <summary>True when a flag-free launch runs the Canvas2D app instead of a parity capture.</summary>
        public static bool ProductBoot(string[] args)
        {
            if (args == null) args = new string[0];
            if (Array.IndexOf(args, "--parity") >= 0) return false;
            if (SelfTestDirectory(args) != null || LiveSelfTestDirectory(args) != null || PerfScenario(args) != null || AppSelfTestDirectory(args) != null) return false;
            if (Array.IndexOf(args, "-parity-app-flows") >= 0) return false;
            if (ShipSelfTestDirectory(args) != null) return true;
            if (IsOptOut(args)) return false;
            return true;
        }

        static string ArgDirectory(string[] args, string flag)
        {
            int i = Array.IndexOf(args, flag);
            if (i < 0 || i + 1 >= args.Length) return null;
            return args[i + 1];
        }

        void Awake()
        {
            useGUILayout = false;
            black = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            black.SetPixel(0, 0, Color.black);
            black.Apply(false, true);
            black.hideFlags = HideFlags.HideAndDontSave;
        }

        void Start()
        {
            clock.NowSeconds = () => Time.realtimeSinceStartupAsDouble;
            SuppressLegacy();
            var args = Environment.GetCommandLineArgs();
            string perfScenario = PerfScenario(args);
            if (perfScenario != null)
            {
                selfTesting = true;
                StartCoroutine(PerfTest(perfScenario, PerfDirectory(args)));
                return;
            }
            string live = LiveSelfTestDirectory(args);
            if (live != null)
            {
                selfTesting = true;
                liveVisual = true;
                StartCoroutine(LiveSelfTest(live));
                return;
            }
            string self = SelfTestDirectory(args);
            if (self != null)
            {
                selfTesting = true;
                liveVisual = false;
                StartCoroutine(SelfTest(self));
                return;
            }
            string flowDir = ArgDirectory(args, "-parity-app-flows");
            if (flowDir != null)
            {
                selfTesting = true;
                liveVisual = true;
                StartCoroutine(FlowShots(flowDir));
                return;
            }
            string appDir = AppSelfTestDirectory(args);
            if (appDir != null)
            {
                selfTesting = true;
                liveVisual = true;
                StartCoroutine(AppSelfTest(appDir));
                return;
            }
            string shipDir = ShipSelfTestDirectory(args);
            if (shipDir != null)
            {
                selfTesting = true;
                shipTesting = true;
                liveVisual = true;
                StartCoroutine(ShipSelfTest(shipDir));
                return;
            }
            BootApp();
            string probeDir = ArgDirectory(args, "-editor-play-probe");
            if (probeDir != null) StartCoroutine(EditorPlayProbe(probeDir));
        }

        void LateUpdate()
        {
            SuppressLegacy();
            if (selfTesting && !shipTesting) return;
            if (!selfTesting) GovernQuality();
            if (app != null) SyncApp();
            HandleInput();
            Tick();
            TickLive();
        }

        void OnDestroy()
        {
            if (RequestReset == ResetRunningProgress) RequestReset = null;
            cache.Clear(chrome => chrome.Dispose());
            current = null;
            if (underlayChrome != null)
            {
                underlayChrome.Dispose();
                underlayChrome = null;
            }
            if (session != null)
            {
                session.Renderer.Dispose();
                session = null;
            }
            if (black != null) Destroy(black);
            if (readTex != null) Destroy(readTex);
            if (readRt != null)
            {
                readRt.Release();
                Destroy(readRt);
            }
        }

        /// <summary>Same entry the keyboard, the mouse, and the player self-test use.</summary>
        public void Show(string stateId) => Show(stateId, null);

        /// <summary>Load <paramref name="stateId"/>. When <paramref name="cardKinds"/> is set, shop cards are instanced from the measured sub-templates.</summary>
        public void Show(string stateId, string cardKinds)
        {
            string key = string.IsNullOrEmpty(cardKinds) ? stateId : stateId + "#" + cardKinds;
            if (!ParityPages.IsState(stateId) && !HomeChrome.Applies(stateId)) return;
            if (shownKey == key && current != null) return;
            if (!cache.TryGet(key, out var chrome))
            {
                try
                {
                    chrome = HomeChrome.Load(stateId, ParityPages.Profile);
                    if (!string.IsNullOrEmpty(cardKinds)) StampShop(chrome, cardKinds);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[ParityViewer] " + ex.Message);
                    return;
                }
                evicted.Clear();
                cache.Add(key, chrome, evicted);
                for (int i = 0; i < evicted.Count; i++) evicted[i].Dispose();
                if (WantLiveChrome(stateId)) chrome.SkipPlayfield = true;
            }
            else if (WantLiveChrome(stateId) && !chrome.SkipPlayfield)
                chrome.SkipPlayfield = true;
            bool modal = stateId.StartsWith("modal_", StringComparison.Ordinal);
            current = chrome;
            currentId = stateId;
            shownKey = key;
            if (WantLiveChrome(stateId)) EnsureLive(stateId);
            lastQuota = null;
            pressIndex = -1;
            pressCss = null;
            armedEdge = null;
            clock.Restart();
            BindModalPlayfield(chrome);
            PaintCurrent(true);
        }

        /// <summary>
        /// Modal overlays blur whatever is already on the canvas. Draw the live playfield at the
        /// canvas element so that blur matches the still, instead of blurring an empty hole.
        /// </summary>
        void BindModalPlayfield(HomeChrome chrome)
        {
            if (chrome == null) return;
            if (flowCapture && app != null && app.CurrentPage == "game" && session != null && session.Renderer != null && session.Renderer.Target != null)
            {
                chrome.SkipPlayfield = true;
                chrome.ExternalPlayfield = session.Renderer.Target;
                return;
            }
            bool modal = currentId != null && currentId.StartsWith("modal_", StringComparison.Ordinal) && IsGamePage(currentId);
            if (!modal || session == null || session.Renderer == null || session.Renderer.Target == null)
            {
                chrome.ExternalPlayfield = null;
                return;
            }
            session.IsPaused = true;
            if (session.RenderFrames == 0) session.Update(RecordedFrameDriver.FrameDt);
            chrome.ExternalPlayfield = session.Renderer.Target;
            chrome.SkipPlayfield = false;
        }

        void Tick()
        {
            if (current == null || currentId == null) return;
            double ms = clock.Milliseconds;
            bool modal = currentId.StartsWith("modal_", StringComparison.Ordinal);
            bool intro = modal && ms < 400;
            bool enter = ms < 260;
            // Toasts paint after the cached layers. A non-looping page still has to redraw while
            // one is on screen, and once more on the frame the last toast leaves.
            bool toastAnim = toastSlots > 0 || toastClearFrame;
            if (!current.Loops && !intro && !enter && pressIndex < 0 && !toastAnim) return;
            bool ambient = current.Loops && !intro && !enter && pressIndex < 0 && !toastAnim;
            if (HoldAmbientDraw(ambient)) return;
            if (toastClearFrame && toastSlots == 0) toastClearFrame = false;
            BindModalPlayfield(current);
            PaintCurrent(false);
        }

        /// <summary>
        /// IMGUI y grows down; <see cref="PageRect"/> and <c>Input.mousePosition</c> grow up.
        /// Drawing the screen rect with GUI.DrawTexture puts a portrait letterbox off the Game view.
        /// </summary>
        public static Rect ToGui(Rect screen)
        {
            return new Rect(screen.x, Screen.height - screen.y - screen.height, screen.width, screen.height);
        }

        void PaintCurrent(bool pinEnter)
        {
            if (current == null) return;
            ProductDraws++;
            if (selfTesting)
            {
                current.Draw();
                return;
            }
            double ms = clock.Milliseconds;
            bool modal = currentId != null && currentId.StartsWith("modal_", StringComparison.Ordinal);
            bool intro = modal && ms < 400;
            // Product path: never start a page at pageEnter opacity 0 (that is a clear RT, drawn
            // without alpha, so the Game view is a black letterbox). Loops still run.
            bool pin = true;
            try
            {
                current.Draw(ms, pressIndex, pressCss, intro, pin);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ParityViewer] draw " + currentId + ": " + ex);
            }
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.F1)) hintVisible = !hintVisible;
            if (Input.GetKeyDown(KeyCode.F2)) debugNav = !debugNav;
            if (Input.GetKeyDown(KeyCode.F3)) qualityHud = !qualityHud;
            if (debugNav)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) Show(ParityPages.HomeId);
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.PageUp))
                    Show(ParityPages.Cycle(currentId, -1));
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.PageDown))
                    Show(ParityPages.Cycle(currentId, 1));
                for (int i = 0; i < DigitKeys.Length; i++)
                {
                    if (!Input.GetKeyDown(DigitKeys[i]) && !Input.GetKeyDown(PadKeys[i])) continue;
                    int digit = i == 9 ? 0 : i + 1;
                    Show(ParityPages.Jump(digit));
                }
            }
            else if (app != null)
            {
                if (Input.GetKeyDown(KeyCode.A)) app.KeyDown("a");
                if (Input.GetKeyDown(KeyCode.D)) app.KeyDown("d");
                if (Input.GetKeyDown(KeyCode.LeftArrow)) app.KeyDown("ArrowLeft");
                if (Input.GetKeyDown(KeyCode.RightArrow)) app.KeyDown("ArrowRight");
                if (Input.GetKeyDown(KeyCode.Space)) app.KeyDown(" ");
                if (Input.GetKeyDown(KeyCode.R)) app.KeyDown("r");
                if (Input.GetKeyDown(KeyCode.Escape)) app.KeyDown("Escape");
                if (session != null && app.CurrentPage == "game" && !app.IsPaused)
                {
                    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) session.KeyDown("a");
                    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) session.KeyDown("d");
                    if (Input.GetKeyDown(KeyCode.Space)) session.KeyDown(" ");
                }
            }
            bool playing = app != null && app.CurrentPage == "game" && session != null && !app.ModalOpen("modal-pause")
                && !app.ModalOpen("modal-debrief") && !app.ModalOpen("modal-defeat");
            if (pointerInject.Count > 0)
            {
                var injected = pointerInject.Dequeue();
                if (injected.Up) Release();
                else Arm(injected.Screen);
            }
            else if (playing && !debugNav) HandlePlayPointer();
            else
            {
                if (PointerDown(out var down)) Arm(down);
                if (!string.IsNullOrEmpty(dragSlider) && PointerHeld(out var held)) DragSlider(held);
                if (PointerUp()) Release();
            }
        }

        struct PointerInject
        {
            public bool Up;
            public Vector2 Screen;
        }

        /// <summary>Same Arm/Release path a real pointer uses. Screen origin is bottom-left, as <c>Input.mousePosition</c>.</summary>
        public void InjectPointerDown(Vector2 screenBottomLeft)
        {
            pointerInject.Enqueue(new PointerInject { Up = false, Screen = screenBottomLeft });
        }

        public void InjectPointerUp()
        {
            pointerInject.Enqueue(new PointerInject { Up = true });
        }

        static bool PointerUp()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            }
            return Input.GetMouseButtonUp(0);
        }

        static bool PointerHeld(out Vector2 screen)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                screen = touch.position;
                return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            }
            screen = Input.mousePosition;
            return Input.GetMouseButton(0);
        }

        void Arm(Vector2 screen)
        {
            if (currentId == null) return;
            var box = PageRect();
            if (!TryCss(box, screen, out double cssX, out double cssY)) return;
            string spec = Spec(currentId);
            if (ParityPages.TryPressAt(spec, cssX, cssY, out int index, out string transform))
            {
                pressIndex = index;
                pressCss = transform;
            }
            dragSlider = null;
            if (debugNav || app == null)
                armedEdge = ParityPages.HitEdge(currentId, spec, cssX, cssY);
            else
            {
                armedControl = ParityPages.TryHitControl(spec, cssX, cssY, out armedHit);
                if (armedControl && (armedHit.Id == "settings-sfx-slider" || armedHit.Id == "settings-ambience-slider"))
                {
                    dragSlider = armedHit.Id;
                    ApplySlider(spec, cssX);
                }
            }
        }

        void DragSlider(Vector2 screen)
        {
            if (app == null || currentId == null) return;
            var box = PageRect();
            if (!TryCss(box, screen, out double cssX, out _)) return;
            ApplySlider(Spec(currentId), cssX);
        }

        void ApplySlider(string spec, double cssX)
        {
            if (app == null || string.IsNullOrEmpty(dragSlider)) return;
            if (!ParityPages.TryRect(spec, dragSlider, out double x, out _, out double w, out _)) return;
            app.SetSlider(dragSlider, ParityPages.SliderValue(cssX, x, w));
        }

        void Release()
        {
            var edge = armedEdge;
            bool control = armedControl;
            var hit = armedHit;
            pressIndex = -1;
            pressCss = null;
            armedEdge = null;
            armedControl = false;
            dragSlider = null;
            if (debugNav)
            {
                if (edge != null) Activate(edge);
                return;
            }
            if (control && app != null)
            {
                if (!string.IsNullOrEmpty(hit.Id)) app.Click(hit.Id);
                else if (!string.IsNullOrEmpty(hit.ClassToken)) app.ClickClass(hit.ClassToken, hit.ClassIndex);
            }
        }

        static bool PointerDown(out Vector2 screen)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screen = touch.position;
                    return true;
                }
                screen = default;
                return false;
            }
            if (Input.GetMouseButtonDown(0))
            {
                screen = Input.mousePosition;
                return true;
            }
            screen = default;
            return false;
        }

        string Spec(string stateId)
        {
            if (specs.TryGetValue(stateId, out var json)) return json;
            var asset = Resources.Load<TextAsset>("Parity/Spec/" + stateId + "__" + ParityPages.Profile);
            json = asset != null ? asset.text : "{\"elements\":[]}";
            specs[stateId] = json;
            return json;
        }

        /// <summary>Centered 430:932 rect in a full window. y is the bottom in screen pixels.</summary>
        public static Rect Letterbox(int screenW, int screenH)
        {
            return FitPage(new Rect(0f, 0f, screenW, screenH));
        }

        /// <summary>
        /// Scale the 430×932 portrait page into <paramref name="fit"/> (the window, or <see cref="Screen.safeArea"/>).
        /// Landscape keeps this portrait page and letterboxes it; there is no separate landscape layout.
        /// The painter raster is the spec viewport (its scale is the capture device pixel ratio) stretched into this rect.
        /// </summary>
        public static Rect FitPage(Rect fit)
        {
            float aspect = ParityPages.CssWidth / (float)ParityPages.CssHeight;
            float fw = Mathf.Max(1f, fit.width);
            float fh = Mathf.Max(1f, fit.height);
            float sa = fw / fh;
            float dw, dh, dx, dy;
            if (sa > aspect)
            {
                dh = fh;
                dw = dh * aspect;
                dx = fit.x + (fw - dw) * 0.5f;
                dy = fit.y;
            }
            else
            {
                dw = fw;
                dh = dw / aspect;
                dx = fit.x;
                dy = fit.y + (fh - dh) * 0.5f;
            }
            return new Rect(dx, dy, dw, dh);
        }

        /// <summary>Page rect for this frame: the portrait page fitted inside the device safe area.</summary>
        public static Rect PageRect()
        {
            Rect safe = Screen.safeArea;
            if (safe.width < 8f || safe.height < 8f)
                safe = new Rect(0f, 0f, Screen.width, Screen.height);
            return FitPage(safe);
        }

        public static Vector2 CssToScreen(Rect box, double cssX, double cssY)
        {
            float x = box.x + (float)(cssX / ParityPages.CssWidth * box.width);
            float y = box.y + box.height - (float)(cssY / ParityPages.CssHeight * box.height);
            return new Vector2(x, y);
        }

        public static bool TryCss(Rect box, Vector2 screenBottomLeft, out double cssX, out double cssY)
        {
            cssX = cssY = 0;
            if (box.width <= 0 || box.height <= 0) return false;
            if (screenBottomLeft.x < box.x || screenBottomLeft.y < box.y) return false;
            if (screenBottomLeft.x >= box.xMax || screenBottomLeft.y >= box.yMax) return false;
            cssX = (screenBottomLeft.x - box.x) / box.width * ParityPages.CssWidth;
            cssY = (box.y + box.height - screenBottomLeft.y) / box.height * ParityPages.CssHeight;
            return true;
        }

        void OnGUI()
        {
            if (black == null || Screen.width < 2 || Screen.height < 2) return;
            var full = new Rect(0, 0, Screen.width, Screen.height);
            GUI.DrawTexture(full, black, ScaleMode.StretchToFill, false);
            var letter = ToGui(PageRect());
            if (ShowPlayfield && session.Renderer.Target != null)
                GUI.DrawTexture(letter, session.Renderer.Target, ScaleMode.StretchToFill, true);
            if (current != null && current.Target != null)
                GUI.DrawTexture(letter, current.Target, ScaleMode.StretchToFill, ShowPlayfield);
            DrawQualityHud();
            if (!hintVisible || selfTesting) return;
            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
                hintStyle.normal.textColor = new Color(1f, 1f, 1f, 0.9f);
            }
            string label = (currentId ?? "") + "    <- / -> to switch pages";
            var hint = new Rect(8, Screen.height - 22, Screen.width - 16, 20);
            GUI.Label(hint, label, hintStyle);
        }

        void SuppressLegacy()
        {
            if (eventSystem == null) eventSystem = GameObject.Find("EventSystem");
            if (eventSystem != null && eventSystem.activeSelf) eventSystem.SetActive(false);

            var found = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < found.Length; i++)
            {
                var canvas = found[i];
                if (canvas != null && canvas.enabled) canvas.enabled = false;
            }
        }

        static bool IsGamePage(string id)
        {
            return id == "game_lvl01_t1000ms" || id == "game_daily_bronze_t1000" || id == "modal_pause"
                || id == "modal_debrief_exact" || id == "modal_defeat_12_of_20";
        }

        bool WantLiveChrome(string id)
        {
            return liveVisual && IsGamePage(id);
        }

        bool ShowPlayfield
        {
            get { return liveVisual && session != null && IsGamePage(currentId); }
        }

        void EnsureLive(string stateId)
        {
            if (session == null)
            {
                session = LiveGameSession.CreatePlay(ParityPages.CssWidth, ParityPages.CssHeight, 1);
                session.NowMs = () => Time.realtimeSinceStartupAsDouble * 1000.0;
            }
            session.IsPaused = stateId != "game_lvl01_t1000ms";
        }

        void Activate(NavEdge edge)
        {
            bool fresh = edge.Id == "btn-home-play" || edge.ClassToken == "level-tile" || edge.ClassToken == "btn-play-daily-tier";
            bool restart = edge.Id == "btn-pause-restart" || edge.Id == "btn-debrief-replay" || edge.Id == "btn-defeat-retry";
            if ((fresh || restart) && IsGamePage(edge.TargetId))
            {
                if (session == null) EnsureLive(edge.TargetId);
                else session.Restart();
            }
            Show(edge.TargetId);
        }

        void HandlePlayPointer()
        {
            bool down = false, held = false, up = false;
            Vector2 screen;
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                screen = touch.position;
                down = touch.phase == TouchPhase.Began;
                held = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
                up = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            }
            else
            {
                screen = Input.mousePosition;
                down = Input.GetMouseButtonDown(0);
                held = Input.GetMouseButton(0);
                up = Input.GetMouseButtonUp(0);
            }
            var box = PageRect();
            if (!TryCss(box, screen, out double cssX, out double cssY))
            {
                if (up) session.PointerUp();
                return;
            }
            if (down)
            {
                string spec = Spec(currentId);
                if (app != null && ParityPages.TryHitControl(spec, cssX, cssY, out var hit))
                {
                    if (!string.IsNullOrEmpty(hit.Id)) app.Click(hit.Id);
                    else if (!string.IsNullOrEmpty(hit.ClassToken)) app.ClickClass(hit.ClassToken, hit.ClassIndex);
                    return;
                }
                session.PointerDown(cssX, 0, ParityPages.CssWidth);
            }
            else if (held) session.PointerMove(cssX);
            if (up) session.PointerUp();
        }

        void TickLive()
        {
            if (!ShowPlayfield)
            {
                if (session != null) session.ClearAccum();
                return;
            }
            // Pause freezes via the controller. Debrief and defeat keep the web clock running, but the
            // stills are one frozen canvas; hold that frame while a modal covers the playfield.
            bool modal = currentId != null && currentId.StartsWith("modal_", StringComparison.Ordinal);
            session.IsPaused = (app != null && app.IsPaused) || modal;
            session.Accumulate = true;
            session.Update(Time.unscaledDeltaTime);
            if (app != null) app.SetLiveMass(session.CollectedMass, session.SpilledMass);
            ApplyQuota();
        }

        void ApplyQuota()
        {
            if (current == null || session == null) return;
            string label = session.QuotaLabel();
            if (label == lastQuota) return;
            lastQuota = label;
            current.TextOverrides["hud-quota-text"] = label;
            current.WidthOverrides["hud-quota-bar"] = session.QuotaBarWidth(QuotaTrackWidth);
            BindModalPlayfield(current);
            if (selfTesting) current.Draw();
            else
            {
                double ms = clock.Milliseconds;
                bool modal = currentId != null && currentId.StartsWith("modal_", StringComparison.Ordinal);
                current.Draw(ms, pressIndex, pressCss, modal && ms < 400, modal);
            }
        }

        IEnumerator PerfTest(string scenario, string dir)
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            yield return null;
            string error = null;
            try
            {
                if (string.IsNullOrEmpty(dir)) throw new InvalidOperationException("perf directory missing");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "device.txt"),
                    SystemInfo.graphicsDeviceType + "\n" + SystemInfo.graphicsDeviceName + "\n");
                Debug.Log("[ParityViewer] perf " + scenario + " " + SystemInfo.graphicsDeviceType + " -> " + dir);
            }
            catch (Exception ex) { error = ex.ToString(); }

            if (error == null && scenario == "home")
            {
                liveVisual = false;
                yield return StartCoroutine(WriteHomeTimes(dir));
                if (current == null) error = "home chrome did not load";
            }
            else if (error == null && scenario == "live")
            {
                liveVisual = true;
                try { Show("game_lvl01_t1000ms"); }
                catch (Exception ex) { error = ex.ToString(); }
                yield return new WaitForEndOfFrame();
                if (error == null && (session == null || current == null)) error = "live session did not start";
                if (error == null)
                {
                    string stats = null;
                    try { stats = RunLiveBench(dir); }
                    catch (Exception ex) { error = ex.ToString(); }
                    if (error == null) Debug.Log("[ParityViewer] perf live " + stats);
                }
            }
            else if (error == null && (scenario == "fps-home" || scenario == "fps-game"))
            {
                yield return StartCoroutine(MeasurePresented(dir, scenario == "fps-game"));
            }
            else if (error == null)
                error = "unknown perf scenario " + scenario;

            if (error != null) Debug.LogError("[ParityViewer] perf failed: " + error);
            else Debug.Log("[ParityViewer] perf " + scenario + " ok");
            yield return null;
            if (!Application.isEditor) Application.Quit(error == null ? 0 : 1);
        }

        IEnumerator MeasurePresented(string dir, bool game)
        {
            liveVisual = game;
            Show(game ? "game_lvl01_t1000ms" : ParityPages.HomeId);
            yield return null;
            if (game && session != null)
            {
                session.FixedStep = false;
                session.Accumulate = false;
                session.NowMs = () => Time.realtimeSinceStartupAsDouble * 1000.0;
            }
            double clock = 240;
            double started = Time.realtimeSinceStartupAsDouble;
            var samples = new List<double>(900);
            while (Time.realtimeSinceStartupAsDouble - started < 10.0)
            {
                double dt = Time.unscaledDeltaTime;
                if (dt < 0) dt = 0;
                if (game)
                {
                    if (session != null) session.Update(dt);
                    ApplyQuota();
                }
                else if (current != null)
                {
                    clock += dt * 1000.0;
                    current.Draw(clock);
                }
                yield return null;
                double presented = Time.unscaledDeltaTime;
                if (presented > 0) samples.Add(presented);
            }
            if (samples.Count > 2) samples.RemoveAt(0);
            double sum = 0, max = 0;
            for (int i = 0; i < samples.Count; i++)
            {
                sum += samples[i];
                if (samples[i] > max) max = samples[i];
            }
            double avg = samples.Count > 0 ? sum / samples.Count : 0;
            var sorted = samples.ToArray();
            Array.Sort(sorted);
            int p = sorted.Length == 0 ? 0 : (int)Math.Ceiling(sorted.Length * 0.95) - 1;
            if (p < 0) p = 0;
            double p95 = sorted.Length == 0 ? 0 : sorted[p];
            var inv = CultureInfo.InvariantCulture;
            string json = "{\"frames\":" + samples.Count
                + ",\"seconds\":" + (Time.realtimeSinceStartupAsDouble - started).ToString("R", inv)
                + ",\"avgFps\":" + (avg > 0 ? 1.0 / avg : 0).ToString("R", inv)
                + ",\"frameAvgMs\":" + (avg * 1000.0).ToString("R", inv)
                + ",\"frameP95Ms\":" + (p95 * 1000.0).ToString("R", inv)
                + ",\"frameMaxMs\":" + (max * 1000.0).ToString("R", inv) + "}";
            File.WriteAllText(Path.Combine(dir, "fps.json"), json);
            Debug.Log("[ParityViewer] perf fps " + json);
        }

        IEnumerator LiveSelfTest(string dir)
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            yield return null;
            string error = null;
            try
            {
                Directory.CreateDirectory(dir);
                Show("game_lvl01_t1000ms");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForEndOfFrame();
            if (error == null && (session == null || current == null))
                error = "live session did not start";
            string stats = null;
            if (error == null)
            {
                try { stats = RunLiveBench(dir); }
                catch (Exception ex) { error = ex.ToString(); }
            }
            if (error != null) UnityEngine.Debug.LogError("[ParityViewer] live selftest failed: " + error);
            else UnityEngine.Debug.Log("[ParityViewer] live selftest " + stats);
            yield return null;
            if (!Application.isEditor) Application.Quit(error == null ? 0 : 1);
        }

        string RunLiveBench(string dir)
        {
            session.FixedStep = true;
            session.FixedDt = 1.0 / 60.0;
            double clock = 0;
            session.NowMs = () => clock;
            var full = Sample(dir, "full", 1.0, 600, ref clock, out double fullCpu, out double fullFence);
            var half = Sample(dir, "half", 0.5, 600, ref clock, out double halfCpu, out double halfFence);
            bool same = Deterministic();
            string recommended = (full[0] > 12.0 && half[0] < full[0]) ? "0.5" : "1";
            var inv = CultureInfo.InvariantCulture;
            string json = "{\"full\":{\"avg\":" + full[0].ToString("R", inv) + ",\"p95\":" + full[1].ToString("R", inv) + ",\"max\":" + full[2].ToString("R", inv)
                + ",\"noFenceAvg\":" + fullCpu.ToString("R", inv) + ",\"fenceAvg\":" + fullFence.ToString("R", inv)
                + "},\"half\":{\"avg\":" + half[0].ToString("R", inv) + ",\"p95\":" + half[1].ToString("R", inv) + ",\"max\":" + half[2].ToString("R", inv)
                + ",\"noFenceAvg\":" + halfCpu.ToString("R", inv) + ",\"fenceAvg\":" + halfFence.ToString("R", inv)
                + "},\"frames\":600,\"dt\":" + (1.0 / 60.0).ToString("R", inv) + ",\"deterministic\":" + (same ? "true" : "false")
                + ",\"recommendedScale\":" + recommended + ",\"physicsSteps\":" + session.PhysicsSteps
                + ",\"device\":\"" + SystemInfo.graphicsDeviceType + "\"}";
            File.WriteAllText(Path.Combine(dir, "stats.json"), json);
            return json;
        }

        double[] Sample(string dir, string tag, double scale, int frames, ref double clock, out double cpuAvg, out double fenceAvg)
        {
            session.Renderer.SetRenderScale(scale);
            for (int i = 0; i < 20; i++) ScriptedStep(i, ref clock);
            var samples = new double[frames];
            var sections = new double[9];
            double cpuSum = 0, fenceSum = 0;
            for (int i = 0; i < frames; i++)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                ScriptedStep(i, ref clock);
                double cpu = sw.Elapsed.TotalMilliseconds;
                SyncGpu(session.Renderer.Target);
                sw.Stop();
                double total = sw.Elapsed.TotalMilliseconds;
                samples[i] = total;
                cpuSum += cpu;
                fenceSum += total - cpu;
                for (int s = 0; s < 9; s++) sections[s] += GameRenderer.SectionMs[s];
                if (tag == "full" && (i == 0 || i == 59 || i == 179))
                    File.WriteAllBytes(Path.Combine(dir, tag + "_f" + (i + 1).ToString() + ".png"), ReadPng(session.Renderer.Target));
            }
            cpuAvg = cpuSum / frames;
            fenceAvg = fenceSum / frames;
            if (tag == "full")
            {
                var inv = CultureInfo.InvariantCulture;
                var sb = new System.Text.StringBuilder();
                sb.Append("{\"cpuAvg\":").Append(cpuAvg.ToString("R", inv));
                sb.Append(",\"fenceAvg\":").Append(fenceAvg.ToString("R", inv));
                sb.Append(",\"sectionsMs\":[");
                for (int s = 0; s < 9; s++)
                {
                    if (s > 0) sb.Append(',');
                    sb.Append((sections[s] / frames).ToString("R", inv));
                }
                sb.Append("]}");
                File.WriteAllText(Path.Combine(dir, "sections.json"), sb.ToString());
            }
            return Summarize(samples);
        }

        void ScriptedStep(int i, ref double clock)
        {
            clock += 16;
            if (i == 90)
            {
                session.PointerDown(40, 0, 430);
                session.PointerUp();
                clock += 100;
                session.PointerDown(48, 0, 430);
                session.PointerUp();
            }
            if (i % 45 == 0)
            {
                session.PointerDown(180, 0, 430);
                session.PointerMove(240);
                session.PointerUp();
            }
            if (i == 200) session.KeyDown("d");
            if (i == 400) session.IsPaused = true;
            if (i == 430) session.IsPaused = false;
            session.Update(session.FixedDt);
            ApplyQuota();
        }

        static double[] Summarize(double[] samples)
        {
            double sum = 0, max = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i];
                if (samples[i] > max) max = samples[i];
            }
            var sorted = (double[])samples.Clone();
            Array.Sort(sorted);
            int p = (int)Math.Ceiling(sorted.Length * 0.95) - 1;
            if (p < 0) p = 0;
            return new[] { sum / sorted.Length, sorted[p], max };
        }

        static bool Deterministic()
        {
            byte[] a = RunHash();
            byte[] b = RunHash();
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }

        static byte[] RunHash()
        {
            var run = LiveGameSession.CreatePlay(430, 932, 1);
            run.FixedStep = true;
            run.FixedDt = 1.0 / 60.0;
            for (int i = 0; i < 30; i++) run.Update(run.FixedDt);
            var png = Encode(run.Renderer.Target);
            run.Renderer.Dispose();
            return png;
        }

        static Texture2D fenceTex;

        static void SyncGpu(RenderTexture rt)
        {
            if (rt == null) return;
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            if (fenceTex == null) fenceTex = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            fenceTex.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
            RenderTexture.active = prev;
        }

        static byte[] Encode(RenderTexture source)
        {
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(source, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(source.width, source.height, TextureFormat.RGB24, false, true);
            tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            tex.Apply(false);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            var bytes = tex.EncodeToPNG();
            UnityEngine.Object.Destroy(tex);
            return bytes;
        }

        IEnumerator SelfTest(string dir)
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            yield return null;
            string error = null;
            try
            {
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "started.txt"), "ok");
                Debug.Log("[ParityViewer] selftest start " + dir);
            }
            catch (Exception ex) { error = ex.ToString(); }

            string id = ParityPages.StateIds[0];
            for (int i = 0; i < ParityPages.StateIds.Length && error == null; i++)
            {
                Show(id);
                yield return null;
                try
                {
                    if (current == null || current.Target == null)
                        throw new InvalidOperationException("no frame for " + id);
                    File.WriteAllBytes(Path.Combine(dir, id + ".png"), ReadPng(current.Target));
                }
                catch (Exception ex) { error = ex.ToString(); }
                id = ParityPages.Cycle(id, 1);
            }

            if (error == null) yield return StartCoroutine(WriteHomeTimes(dir));
            if (error != null) Debug.LogError("[ParityViewer] selftest failed: " + error);
            else Debug.Log("[ParityViewer] selftest wrote " + ParityPages.StateIds.Length + " frames to " + dir);
            yield return null;
            if (!Application.isEditor) Application.Quit(error == null ? 0 : 1);
        }

        IEnumerator WriteHomeTimes(string dir)
        {
            Show(ParityPages.HomeId);
            if (current == null) yield break;
            double[] marks = { 0, 500, 1000, 2000 };
            for (int i = 0; i < marks.Length; i++)
            {
                current.Draw(marks[i]);
                File.WriteAllBytes(Path.Combine(dir, "home_t" + marks[i].ToString("0") + ".png"), ReadPng(current.Target));
                yield return null;
            }
            var samples = new double[20];
            var watch = new System.Diagnostics.Stopwatch();
            PaintProfile.Enabled = true;
            PaintProfile.Reset();
            long mem0 = GC.GetTotalMemory(false);
            for (int i = 0; i < samples.Length; i++)
            {
                watch.Restart();
                current.Draw(i * 16.0);
                watch.Stop();
                samples[i] = watch.Elapsed.TotalMilliseconds;
                yield return null;
            }
            long mem1 = GC.GetTotalMemory(false);
            PaintProfile.Enabled = false;
            WriteDrawStats(dir, samples, samples.Length, current != null && current.UsesLayerCache, mem1 - mem0);
            string table = PaintProfile.Report(samples.Length);
            File.WriteAllText(Path.Combine(dir, "paint-profile.txt"), table);
            Debug.Log("[ParityViewer] paint profile\n" + table);
        }

        static void WriteDrawStats(string dir, double[] samples, int n, bool cache, long gcBytes = 0)
        {
            var copy = new double[n];
            Array.Copy(samples, copy, n);
            Array.Sort(copy);
            double sum = 0;
            for (int i = 0; i < n; i++) sum += copy[i];
            double avg = sum / n;
            double p95 = copy[(int)(0.95 * (n - 1))];
            string text = "avg " + avg.ToString("0.000") + "\np95 " + p95.ToString("0.000") + "\nn " + n + "\ncache " + (cache ? "yes" : "no") + "\ngcBytes " + gcBytes + "\n";
            File.WriteAllText(Path.Combine(dir, "draw-ms.txt"), text);
            Debug.Log("[ParityViewer] Draw ms n " + n + " avg " + avg.ToString("0.000") + " p95 " + p95.ToString("0.000") + " cache " + (cache ? "yes" : "no"));
        }

        // Same readback as ParityRenderRig.ReadPng: copy into a Default ARGB32 target, then RGB24 linear EncodeToPNG.
        byte[] ReadPng(RenderTexture source)
        {
            if (readRt == null || readRt.width != source.width || readRt.height != source.height)
            {
                if (readRt != null) { readRt.Release(); Destroy(readRt); }
                if (readTex != null) Destroy(readTex);
                readRt = new RenderTexture(source.width, source.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
                {
                    name = "ParityViewer.Read",
                    antiAliasing = 1,
                    filterMode = FilterMode.Point,
                    hideFlags = HideFlags.HideAndDontSave
                };
                readRt.Create();
                readTex = new Texture2D(source.width, source.height, TextureFormat.RGB24, false, true)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            Graphics.CopyTexture(source, readRt);
            var prev = RenderTexture.active;
            RenderTexture.active = readRt;
            readTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            readTex.Apply(false);
            RenderTexture.active = prev;
            return ImageConversion.EncodeToPNG(readTex);
        }
    }
}
