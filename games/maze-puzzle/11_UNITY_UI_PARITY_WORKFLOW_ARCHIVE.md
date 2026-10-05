# 11 - Unity UI parity: workflow archive (2026-10-04)

Archive of how the Unity project was brought to the web prototype's UI design, animation, navigation and logic, written so the process can be repeated or audited. Numbers are measurements taken on this machine; nothing here claims that UI restoration is finished.

Status at archive time: the Canvas2D painted UI is the default launch path, 5 of 11 reference stills pass the strict bar, 6 of 55 recorded flow steps pass, and the Editor Play lag workaround is merged (section 9). Not verified on a phone or tablet.

## 1. Goal and trigger

User feedback: the Unity UI looked static and "completely undynamic", especially home and game, and the old UI had been retained. Required: reproduce the web prototype's UI design, animation, navigation and all related logic in the Unity project. The web prototype (`games/maze-puzzle/web`) is frozen and is never edited.

## 2. Standing rules

- Web prototype is read-only. Parity tooling injects instrumentation at runtime.
- Strict metric: viewport 430x932 css, captured at 3x (1290x2796), profile `zero`. A state passes when at least 99.0% of pixels have per-channel max abs diff of 16 or less AND SSIM is at least 0.985 (Pillow + numpy + scikit-image, `channel_axis=2`, `data_range=255`).
- Web references: `tools/parity/out/web/<stateId>__zero.png`, from a Playwright harness with frozen clock, seed `0x5EED1234`, time zone America/Los_Angeles, instant 2026-10-01T09:00:01-07:00.
- BLANK guard: a frame with fewer than 50 unique colours, or more than 99.5% one colour, never counts as a pass (an early 26/59 flow score was two flat navy frames).
- Never replace a saved screenshot with a worse one. A state that does not improve for 3 iterations is escalated, not retried.
- No blitting of web PNGs, no TMP SDF text in the painter, no region masks.
- No pushes, no touching `stash@{0}`, `AGENT-GLOBAL-BASE` is not modified, processes are only killed by PID.
- Merge into `maze-puzzle` required explicit user approval (given on 2026-10-04 after the Editor lag report).

## 3. Roles and loop

- Lead reviewer: Claude Sonnet 5.5 (High). It plans, reviews every report against measured numbers, rejects unmeasured claims, and decides merges.
- Engineers: Grok 4.7 High sub-agents, one per track, each in its own git worktree under `.worktrees/parity-*` on a branch `agent/parity/<track>`. Tracks finish with a numbered report: root cause, before/after table, gate results, commit hashes.
- Loop: capture web reference -> capture Unity -> score -> attribute error -> fix at the cause -> rebuild -> recapture -> rescore -> regression gates -> commit. Merges into `agent/parity/integration` always run the full regression first.
- Tooling: Unity CLI (`tools/parity/unity/build.ps1`, `capture.ps1`, `perf.ps1`, `build-play.ps1`); Unity MCP was unavailable (authentication timed out).

## 4. Architecture decisions

- Native C# Canvas2D painter, no SkiaForUnity. Pages are painted from distilled Chromium DOM spec JSON (`Assets/Resources/Parity/Spec/<id>__zero.json`, 46 specs plus `_keyframes.json`) by `HomeChrome` / `CssBox`.
- Hybrid: measured spec snapshots are templates; slot overrides, `TemplateSelector` and card instancing add dynamic content. There is no CSS layout engine.
- Outline-glyph text rasterizer with exact-area coverage.
- CSS animation player (`CssAnimation`, `CssEasing`, `CssTransform`) with six easings; per-animation clocks recorded in the flow goldens.
- Live game: `GameRenderer` + `LiveGameSession`, 30/30 scenes exact against recorded streams; logic port is bit-exact against Chromium.
- App layer: `AppController` / `AppHost` port the web controller (pointer input, wall clock, `FileProfileStore` JSON profile, DSP audio). Parity modes keep a frozen clock and silent audio.
- Play-mode `ParityViewer` (OnGUI) shows the raster; F1 hint, F2 page browser.
- Legacy uGUI (`UIManager`, `SpriteHelper`, Physics2D gameplay, old managers, scene-builder tools, TMP examples) was removed in the ship track; `Assets/Audio/*.wav` was kept because it could not be proven unused.

## 5. Branch map (agent/parity/*)

| Branch | Content |
| --- | --- |
| infra, unity-capture, logic, canvas, audio | Phase 0/1: harness, golden dumps, bit-exact logic port, Canvas2D layer, C# Web Audio DSP |
| integration | Merge target for all tracks (includes ship + lag as of `0b6f054`) |
| live | Live game renderer and conformance |
| anim | CSS animation player and animated specs |
| perf | Layer cache, GPU blur, home draw 750 ms -> 3-6 ms |
| app | AppController, templates, toasts, settled flows |
| gamesync | Replay to the same physics frame as the web run, collision-cue clock, gesture-hint pose |
| pages | Shop inventories, modal underlay, per-animation clocks |
| home | Pedestal discs through perspective x rotateX, rotated shadows, dashed rim, daily regression fix |
| glyph | Glyph tone model experiments (not merged: page gain under 0.01 pp) |
| ship | Default launch boots the Canvas2D app, legacy uGUI removed |
| lag | Editor Play quality governor, live-home unbake, Simulator-size probe |

## 6. Measured results at archive time

| Check | Result |
| --- | --- |
| Stills passing | loading 99.55/0.9908, game 99.51/0.9919, pause 99.25/0.9877, settings 99.20/0.9890, debrief 99.05/0.9856 |
| Stills failing | trophy (achievements_some) 98.64/0.9804, skies 98.72/0.9782, home 98.67/0.9799, daily 98.05/0.9706, hats 98.01/0.9649, donuts 97.78/0.9614 |
| Hashes | game SHA256 `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A`, pause `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078` |
| Animated home (+500/+1500/+4000 ms) | 98.76 / 98.55 / 98.43 |
| Flows | 55 steps, 6 at the bar (defeat_backup_02, level_win_02, level_win_03, level_win_05, maps_start_05, settings_reset_02), 0 blank |
| Live conformance | 30/30 byte-identical vs StreamPlayer; 99.41-99.77% against Chromium streams |
| Controller goldens | 9/9 |
| EditMode (with GPU) | 380/380 on the ship branch |
| HomeChrome.Draw | avg 3.0 ms, p95 4.4 ms (D3D11 player) |
| Live frame | avg 9.4 ms, p95 14.3 ms (D3D11 player, 600 steps) |

## 7. Lessons

- Metric artifacts: flat frames can look identical. Add the BLANK guard and verify the web frame shows the expected page.
- Profile before optimising: the home draw cost was a backdrop `ReadPixels` (700 ms of 713 ms); a GPU blur brought it to 3-6 ms.
- Settle both sides (400 ms, 800 ms where the settle opens a modal) before flow screenshots; the animation clock origin is page insertion, not the screenshot.
- Prove physics identity with state hashes before judging pixels; the largest game-step gap was a baked, already-dismissed gesture hint and an unsynchronised replay, not the ring raster.
- Text edges are the ceiling: Chromium on Windows rasterises through DirectWrite; a fitted tone curve raised an isolated text harness from 98.48 to 98.88% but moved full pages by under 0.01 pp.
- Run EditMode with a GPU; without one, 5 `Canvas2DGpuTests` fail spuriously.
- Unity rewrites `Assets/Resources/Fonts/TMP/*.asset` and `LiberationSans SDF - Fallback.asset` on import: restore with `git checkout`, never commit.
- Builds can fail once with access-denied on the exe; do not build while a capture holds it. `robocopy` exit code 1 is success.
- Use `.py` / `.ps1` files for anything with nested quotes in PowerShell.

## 8. How to repeat

```powershell
cd tools/parity
npm ci
pip install numpy scikit-image Pillow opencv-python
# web references: see tools/parity/README.md (Playwright harness)
./unity/build.ps1      # parity player
./unity/capture.ps1    # 1290x2796 stills, profile zero
# score stills and flows with the diff/score scripts under tools/parity (SPEC.md v1.6 is the contract)
```

Preview in the Editor: open the project at `games/maze-puzzle/unity/maze-puzzle` (Unity 6000.4.6f1), open `Assets/Scenes/MainGameScene.unity`, press Play. F1 hint, F2 page browser; menu Orbital Salvage -> Parity Viewer -> Enabled / Reset Profile.

## 9. User-reported Editor Play lag (closed 2026-10-04)

- Report: after Play in the Editor (Simulator, iOS Notch Device Large, width 1284) the game lagged or was unplayable; the home page also rendered incorrectly (mustard-yellow background, dark-blue sphere in place of the emoji).
- Causes, measured: (1) Wrong colours were live-frame drift. `CssFrame.Build` unbaked a reused local rect every frame, so the hero face left the sphere and a debris sprite grew into a mustard page. Frozen `Draw()` never unbakes, which is why the parity stills stayed correct. (2) Lag was the 3x raster plus vSync, not the CPU painter. Before, at a real D3D12 1284x2778: frame avg 51.08 / p95 61.67 ms while Draw was avg 8.23 / p95 11.55 ms; background `(162,129,72)`, hero `(77,78,188)`.
- Workaround on the product path only: unbake once; vSync 0 and target 60; the raster snaps to the window and steps 3 to 2 to 1.5 to 1 when p95 over ~1 s exceeds 22 ms. Parity stills stay frozen `Draw` at 3x. Commits `6f871e3`, `478b974`, `90a9245`; merged to integration as `0b6f054`.
- After, governor on, true 1284x2778: D3D12 frame avg 29.02 / p95 24.69 ms, raster 430x932, background `(39,36,101)`, hero `(255,182,75)`. D3D11 p95 24.40 ms with the GPU at 100%. Canvas PNG shows the purple page and winking face.
- Remaining: phones untested; this Game view is a fixed 1284x2778, not the Simulator notch; at that size the governor stays at 1x so the page is softer than the 3x parity stills; D3D12 p99 is still 62.7 ms. A one-off `loading_t0000` 0.00 score during scoring was a missing-spec artifact against the camera clear, not a regression of `13_unity_loading_screen.png` (99.55/0.9908).

## 10. Open items

- Real device verification (touch, safe area, rotation, thermals).
- Six stills and 49 flow steps below the strict bar, mostly text-edge anti-aliasing; `maps_start_03` bakes an active-level PLAY tile; `daily_chain` steps at 95-97%.
- Remaining audio cues are not wired, to protect deterministic parity paths.
- `Assets/Audio/*.wav` kept pending a proven-unused check.
