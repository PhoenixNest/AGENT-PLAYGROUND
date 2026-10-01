using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OrbitalSalvage.Parity
{
    public sealed class ParityRunner : MonoBehaviour
    {
        public const long BootSettleMs = 1552;
        public const long HoldSettleMs = 150;
        public const float UiTweenFlushSeconds = 0.25f;

        ParityOptions options;
        ParityClock clock;
        ParityRenderRig rig;
        IParityTarget target;
        ParityContext ctx;
        int sceneIndex;
        bool holding;
        bool booted;
        bool pointerTouched;
        int exitCode;

        readonly List<Dictionary<string, object>> errors = new List<Dictionary<string, object>>();
        readonly List<object> timings = new List<object>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (!ParityOptions.HasFlag(Environment.GetCommandLineArgs())) return;
            var go = new GameObject("ParityRunner");
            DontDestroyOnLoad(go);
            go.AddComponent<ParityRunner>();
        }

        void Start()
        {
            StartCoroutine(Main());
        }

        IEnumerator Main()
        {
            if (!ParityOptions.TryParse(Environment.GetCommandLineArgs(), out options, out string usage))
            {
                Debug.LogError("[Parity] " + usage);
                Application.Quit(2);
                yield break;
            }

            List<ParityState> states;
            try
            {
                states = ParityStateLoader.Load(options.StatesPath);
                Directory.CreateDirectory(options.OutDir);
                Directory.CreateDirectory(Path.Combine(options.OutDir, "traces"));
            }
            catch (Exception ex)
            {
                Debug.LogError("[Parity] cannot load states: " + ex);
                Application.Quit(2);
                yield break;
            }

            sceneIndex = SceneManager.GetActiveScene().buildIndex;
            Time.captureDeltaTime = ParityClock.FrameSeconds;
            Time.fixedDeltaTime = ParityClock.FrameSeconds;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            Application.runInBackground = true;

            clock = new ParityClock();
            rig = new ParityRenderRig();
            rig.Create();
            var driver = gameObject.AddComponent<ParityFrameDriver>();
            driver.Clock = clock;
            driver.Rig = rig;
            ParityScreen.Override = new Vector2Int(rig.Width, rig.Height);

            target = new BaselineAdapter();
            float totalStart = Time.realtimeSinceStartup;

            foreach (var state in states)
            {
                if (options.Only != null && !options.Only.Contains(state.Id)) continue;
                foreach (var profile in state.Profiles)
                {
                    if (options.Profile != null && options.Profile != profile) continue;
                    float started = Time.realtimeSinceStartup;
                    yield return StartCoroutine(RunState(state, profile));
                    timings.Add(new Dictionary<string, object>
                    {
                        ["state"] = state.Id,
                        ["profile"] = profile,
                        ["seconds"] = Math.Round(Time.realtimeSinceStartup - started, 3)
                    });
                }
            }

            target.Shutdown();
            WriteSummary(Time.realtimeSinceStartup - totalStart);
            exitCode = errors.Count > 0 ? 1 : 0;
            Debug.Log($"[Parity] done, {errors.Count} error(s), exit code {exitCode}");
            rig.Dispose();
            Application.Quit(exitCode);
        }

        void WriteSummary(float totalSeconds)
        {
            string summaryDir = options.SummaryDir ?? options.OutDir;
            Directory.CreateDirectory(summaryDir);
            File.WriteAllText(Path.Combine(summaryDir, "timings.json"), MiniJson.Serialize(new Dictionary<string, object>
            {
                ["totalSeconds"] = Math.Round(totalSeconds, 3),
                ["states"] = timings
            }, true));

            string errorsPath = Path.Combine(summaryDir, "errors.json");
            if (errors.Count > 0) File.WriteAllText(errorsPath, MiniJson.Serialize(errors, true));
            else if (File.Exists(errorsPath)) File.Delete(errorsPath);
        }

        IEnumerator RunState(ParityState state, string profile)
        {
            ctx = new ParityContext { StateId = state.Id, Profile = profile, Clock = clock, Rig = rig };
            booted = false;
            holding = false;
            pointerTouched = false;
            bool failed = false;

            if (!ParityProfiles.TryGetInsets(profile, out _, out _, out _, out _))
            {
                AddError(state.Id, profile, -1, "profile", "unknown profile " + profile);
                yield break;
            }

            target.Attach(ctx);

            for (int i = 0; i < state.Steps.Count; i++)
            {
                var step = state.Steps[i];
                ctx.CurrentStep = i;
                ctx.CurrentOp = step.Op;
                Exception error = null;
                yield return StartCoroutine(Guard(ExecuteStep(step), e => error = e));
                if (error != null)
                {
                    failed = true;
                    AddError(state.Id, profile, i, step.Op, error.ToString());
                    if (step.Op == "boot") break;
                }
            }

            if (holding)
            {
                target.Release();
                holding = false;
            }

            if (failed) Debug.LogWarning($"[Parity] {state.Id}/{profile} finished with errors");
        }

        IEnumerator ExecuteStep(ParityStep step)
        {
            if (!booted && step.Op != "boot") throw new InvalidOperationException("first step must be boot");

            switch (step.Op)
            {
                case "boot":
                    yield return Boot(step.Bool("completeLoading", true));
                    break;
                case "advance":
                    yield return Advance((long)step.Num("ms"));
                    break;
                case "holdPressed":
                    pointerTouched = true;
                    Record(target.HoldPressed(step.Str("target")));
                    holding = true;
                    yield return Advance(HoldSettleMs);
                    break;
                case "capture":
                    yield return Capture();
                    break;
                case "showPage":
                    Record(target.ShowPage(step.Str("page")));
                    break;
                case "setProfile":
                    Record(target.SetProfile(step.Str("path"), step.Json("value")));
                    break;
                case "setApp":
                    Record(target.SetApp(step.Str("field"), step.Json("value")));
                    break;
                case "call":
                    Record(target.Call(step.Str("method"), step.Raw.ContainsKey("args") ? step.Json("args") : "[]"));
                    break;
                case "click":
                    pointerTouched = true;
                    Record(target.Click(step.Str("target")));
                    break;
                case "clickSelector":
                    pointerTouched = true;
                    Record(target.ClickSelector(step.Str("selector")));
                    break;
                case "pointer":
                    pointerTouched = true;
                    Record(target.Pointer(step.Str("kind"), (float)step.Num("x"), (float)step.Num("y")));
                    break;
                case "key":
                    Record(target.Key(step.Str("key")));
                    break;
                case "scroll":
                    Record(target.Scroll(step.Str("target"), (float)step.Num("y")));
                    break;
                case "loadSector":
                    Record(target.LoadSector((int)step.Num("index")));
                    break;
            }
        }

        void Record(OpResult result)
        {
            switch (result.Status)
            {
                case OpStatus.Ok:
                    if (!string.IsNullOrEmpty(result.Detail)) ctx.Note("ok", result.Detail);
                    break;
                case OpStatus.Noop:
                    ctx.Note("noop", result.Detail);
                    break;
                case OpStatus.Unsupported:
                    ctx.Note("unsupported", result.Detail);
                    break;
                case OpStatus.Failed:
                    throw new InvalidOperationException(result.Detail);
            }
        }

        IEnumerator Boot(bool completeLoading)
        {
            clock.Armed = false;
            target.PrepareFreshStart();
            JsRandom.Reset();
            UnityEngine.Random.InitState(unchecked((int)JsRandom.Seed));
            SafeAreaProvider.Current = new ProfileSafeAreaProvider(ctx.Profile);
            ParityScreen.Override = new Vector2Int(rig.Width, rig.Height);

            Time.timeScale = 0f;
            SceneManager.LoadScene(sceneIndex);
            yield return null;
            yield return null;
            rig.Refresh();

            Time.timeScale = 1f;
            clock.Reset();
            clock.Armed = true;
            booted = true;
            target.Boot(completeLoading);

            if (completeLoading) yield return Advance(BootSettleMs);
        }

        IEnumerator Advance(long ms)
        {
            long goal = clock.NowMs + ms;
            while (clock.NextFrameMs <= goal) yield return null;
            clock.SetNowMs(goal);
        }

        IEnumerator Capture()
        {
            float savedScale = Time.timeScale;
            clock.Armed = false;
            Time.timeScale = 0f;
            try
            {
                yield return null;
                yield return null;
                if (pointerTouched)
                {
                    float until = Time.realtimeSinceStartup + UiTweenFlushSeconds;
                    while (Time.realtimeSinceStartup < until) yield return null;
                }
                rig.Refresh();
                yield return new WaitForEndOfFrame();

                string stem = Path.Combine(options.OutDir, ctx.StateId + "__" + ctx.Profile);
                File.WriteAllBytes(stem + ".png", rig.ReadPng());
                WriteLayout(stem + ".layout.json");
                WriteNotes(stem + ".unity-notes.json");
                WriteTrace(Path.Combine(options.OutDir, "traces", ctx.StateId + ".trace.json"));
            }
            finally
            {
                Time.timeScale = savedScale;
                clock.Armed = true;
            }

            if (holding)
            {
                target.Release();
                holding = false;
            }
        }

        void WriteLayout(string path)
        {
            var elements = new List<object>();
            foreach (var el in target.CollectLayout())
            {
                elements.Add(new Dictionary<string, object>
                {
                    ["id"] = el.Id,
                    ["tag"] = el.Tag,
                    ["classes"] = el.Classes,
                    ["rect"] = new Dictionary<string, object> { ["x"] = el.X, ["y"] = el.Y, ["w"] = el.W, ["h"] = el.H },
                    ["visible"] = el.Visible,
                    ["opacity"] = el.Opacity,
                    ["text"] = el.Text
                });
            }

            var doc = new Dictionary<string, object>
            {
                ["state"] = ctx.StateId,
                ["profile"] = ctx.Profile,
                ["viewport"] = new Dictionary<string, object> { ["w"] = ParityProfiles.ViewportW, ["h"] = ParityProfiles.ViewportH, ["scale"] = (int)ParityProfiles.Scale },
                ["elements"] = elements
            };
            File.WriteAllText(path, MiniJson.Serialize(doc, true));
        }

        void WriteNotes(string path)
        {
            var notes = new List<object>();
            foreach (var n in ctx.Notes)
                notes.Add(new Dictionary<string, object> { ["step"] = n.Step, ["op"] = n.Op, ["status"] = n.Status, ["detail"] = n.Detail });

            var canvases = new List<object>();
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!canvas.isRootCanvas) continue;
                var rt = (RectTransform)canvas.transform;
                canvases.Add(new Dictionary<string, object>
                {
                    ["name"] = canvas.name,
                    ["renderMode"] = canvas.renderMode.ToString(),
                    ["scaleFactor"] = canvas.scaleFactor,
                    ["rectW"] = rt.rect.width,
                    ["rectH"] = rt.rect.height
                });
            }

            var doc = new Dictionary<string, object>
            {
                ["state"] = ctx.StateId,
                ["profile"] = ctx.Profile,
                ["target"] = target.Name,
                ["virtualNowMs"] = clock.NowMs,
                ["screen"] = new Dictionary<string, object> { ["width"] = Screen.width, ["height"] = Screen.height, ["reported"] = ParityScreen.Width + "x" + ParityScreen.Height },
                ["canvases"] = canvases,
                ["notes"] = notes
            };
            foreach (var kv in ctx.Extra) doc[kv.Key] = kv.Value;
            File.WriteAllText(path, MiniJson.Serialize(doc, true));
        }

        void WriteTrace(string path)
        {
            var events = new List<object>();
            foreach (var ev in ctx.Trace)
                events.Add(new Dictionary<string, object> { ["t"] = ev.T, ["type"] = ev.Type, ["name"] = ev.Name, ["args"] = ev.Args });
            File.WriteAllText(path, MiniJson.Serialize(new Dictionary<string, object> { ["sequence"] = ctx.StateId, ["events"] = events }, true));
        }

        void AddError(string stateId, string profile, int step, string op, string message)
        {
            Debug.LogError($"[Parity] {stateId}/{profile} step {step} ({op}): {message}");
            errors.Add(new Dictionary<string, object>
            {
                ["state"] = stateId,
                ["profile"] = profile,
                ["step"] = step,
                ["op"] = op,
                ["message"] = message
            });
        }

        static IEnumerator Guard(IEnumerator inner, Action<Exception> onError)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(inner);
            while (stack.Count > 0)
            {
                var top = stack.Peek();
                object current;
                try
                {
                    if (!top.MoveNext())
                    {
                        stack.Pop();
                        continue;
                    }
                    current = top.Current;
                }
                catch (Exception ex)
                {
                    onError(ex);
                    yield break;
                }
                if (current is IEnumerator nested)
                {
                    stack.Push(nested);
                    continue;
                }
                yield return current;
            }
        }
    }
}
