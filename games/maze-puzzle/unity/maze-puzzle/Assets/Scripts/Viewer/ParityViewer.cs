using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class ParityViewer : MonoBehaviour
    {
        const string SelfTestFlag = "-parity-viewer-selftest";
        const string OptOutFlag = "-no-parity-viewer";

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

        HomeChrome current;
        string currentId;
        Texture2D black;
        GUIStyle hintStyle;
        bool hintVisible = true;
        bool selfTesting;
        GameObject eventSystem;
        RenderTexture readRt;
        Texture2D readTex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (!ShouldStart(Environment.GetCommandLineArgs())) return;
            if (FindAnyObjectByType<ParityViewer>() != null) return;
            var go = new GameObject("ParityViewer");
            DontDestroyOnLoad(go);
            go.AddComponent<ParityViewer>();
        }

        /// <summary>False while parity capture owns the player, and when the viewer is opted out.</summary>
        public static bool ShouldStart(string[] args)
        {
            if (Array.IndexOf(args, "--parity") >= 0) return false;
            if (SelfTestDirectory(args) != null) return true;
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
            int i = Array.IndexOf(args, SelfTestFlag);
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
            SuppressLegacy();
            string self = SelfTestDirectory(Environment.GetCommandLineArgs());
            if (self != null)
            {
                selfTesting = true;
                StartCoroutine(SelfTest(self));
                return;
            }
            Show(ParityPages.HomeId);
        }

        void LateUpdate()
        {
            SuppressLegacy();
            if (!selfTesting) HandleInput();
        }

        void OnDestroy()
        {
            cache.Clear(chrome => chrome.Dispose());
            current = null;
            if (black != null) Destroy(black);
            if (readTex != null) Destroy(readTex);
            if (readRt != null)
            {
                readRt.Release();
                Destroy(readRt);
            }
        }

        /// <summary>Same entry the keyboard, the mouse, and the player self-test use.</summary>
        public void Show(string stateId)
        {
            if (!ParityPages.IsState(stateId)) return;
            if (currentId == stateId && current != null) return;
            if (!cache.TryGet(stateId, out var chrome))
            {
                try
                {
                    chrome = HomeChrome.Load(stateId, ParityPages.Profile);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[ParityViewer] " + ex.Message);
                    return;
                }
                evicted.Clear();
                cache.Add(stateId, chrome, evicted);
                for (int i = 0; i < evicted.Count; i++) evicted[i].Dispose();
                chrome.Draw();
            }
            current = chrome;
            currentId = stateId;
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.F1)) hintVisible = !hintVisible;
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
            if (PointerDown(out var screen)) Click(screen);
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

        void Click(Vector2 screen)
        {
            if (currentId == null) return;
            var box = Letterbox(Screen.width, Screen.height);
            if (!TryCss(box, screen, out double cssX, out double cssY)) return;
            string target = ParityPages.Hit(currentId, Spec(currentId), cssX, cssY);
            if (target != null) Show(target);
        }

        string Spec(string stateId)
        {
            if (specs.TryGetValue(stateId, out var json)) return json;
            var asset = Resources.Load<TextAsset>("Parity/Spec/" + stateId + "__" + ParityPages.Profile);
            json = asset != null ? asset.text : "{\"elements\":[]}";
            specs[stateId] = json;
            return json;
        }

        /// <summary>Centered 430:932 rect. y is the bottom in screen pixels; for a centered box that equals the GUI top.</summary>
        public static Rect Letterbox(int screenW, int screenH)
        {
            float aspect = ParityPages.CssWidth / (float)ParityPages.CssHeight;
            float sa = screenW / (float)Mathf.Max(1, screenH);
            float dw, dh, dx, dy;
            if (sa > aspect)
            {
                dh = screenH;
                dw = dh * aspect;
                dx = (screenW - dw) * 0.5f;
                dy = 0f;
            }
            else
            {
                dw = screenW;
                dh = dw / aspect;
                dx = 0f;
                dy = (screenH - dh) * 0.5f;
            }
            return new Rect(dx, dy, dw, dh);
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
            if (current != null && current.Target != null)
                GUI.DrawTexture(Letterbox(Screen.width, Screen.height), current.Target, ScaleMode.StretchToFill, false);
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

        IEnumerator SelfTest(string dir)
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            yield return null;
            string error = null;
            try { Directory.CreateDirectory(dir); }
            catch (Exception ex) { error = ex.ToString(); }

            string id = ParityPages.StateIds[0];
            for (int i = 0; i < ParityPages.StateIds.Length && error == null; i++)
            {
                Show(id);
                yield return new WaitForEndOfFrame();
                try
                {
                    if (current == null || current.Target == null)
                        throw new InvalidOperationException("no frame for " + id);
                    File.WriteAllBytes(Path.Combine(dir, id + ".png"), ReadPng(current.Target));
                }
                catch (Exception ex) { error = ex.ToString(); }
                id = ParityPages.Cycle(id, 1);
            }

            if (error != null) Debug.LogError("[ParityViewer] selftest failed: " + error);
            else Debug.Log("[ParityViewer] selftest wrote " + ParityPages.StateIds.Length + " frames to " + dir);
            yield return null;
            if (!Application.isEditor) Application.Quit(error == null ? 0 : 1);
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
