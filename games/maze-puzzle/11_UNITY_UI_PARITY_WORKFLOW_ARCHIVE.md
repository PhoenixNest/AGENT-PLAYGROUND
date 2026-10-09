# 11 - Unity UI parity: workflow archive (2026-10-04)

Archive of how the Unity project was brought to the web prototype's UI design, animation, navigation and logic, written so the process can be repeated or audited. Numbers are measurements taken on this machine; nothing here claims that UI restoration is finished.

**Toolset location (2026-10-08, `agent/chore/tools-move`).** The parity toolset moved from repo-root `tools/parity` to `games/maze-puzzle/tools/parity`. Repo-root `tools/` is gone. Commands below use the new path. Older sentences in this file that still say `tools/parity/...` were the path on the day they were written; the same files are now under `games/maze-puzzle/tools/parity/...`.

Status at archive time: the Canvas2D painted UI is the default launch path, 5 of 11 reference stills pass the strict bar, 6 of 55 recorded flow steps pass, and the Editor Play lag workaround is merged (section 9). Not verified on a phone or tablet.

## 1. Goal and trigger

User feedback: the Unity UI looked static and "completely undynamic", especially home and game, and the old UI had been retained. Required: reproduce the web prototype's UI design, animation, navigation and all related logic in the Unity project. The web prototype (`games/maze-puzzle/web`) is frozen and is never edited.

## 2. Standing rules

- Web prototype is read-only. Parity tooling injects instrumentation at runtime.
- Strict metric: viewport 430x932 css, captured at 3x (1290x2796), profile `zero`. A state passes when at least 99.0% of pixels have per-channel max abs diff of 16 or less AND SSIM is at least 0.985 (Pillow + numpy + scikit-image, `channel_axis=2`, `data_range=255`).
- Web references: `games/maze-puzzle/tools/parity/out/web/<stateId>__zero.png`, from a Playwright harness with frozen clock, seed `0x5EED1234`, time zone America/Los_Angeles, instant 2026-10-01T09:00:01-07:00.
- BLANK guard: a frame with fewer than 50 unique colours, or more than 99.5% one colour, never counts as a pass (an early 26/59 flow score was two flat navy frames).
- Never replace a saved screenshot with a worse one. A state that does not improve for 3 iterations is escalated, not retried.
- No blitting of web PNGs, no TMP SDF text in the painter, no region masks.
- No pushes, no touching `stash@{0}`, `AGENT-GLOBAL-BASE` is not modified, processes are only killed by PID.
- Merge into `maze-puzzle` required explicit user approval (given on 2026-10-04 after the Editor lag report).

## 3. Roles and loop

- Lead reviewer: Claude Sonnet 5.5 (High). It plans, reviews every report against measured numbers, rejects unmeasured claims, and decides merges.
- Engineers: Grok 4.7 High sub-agents, one per track, each in its own git worktree under `.worktrees/parity-*` on a branch `agent/parity/<track>`. Tracks finish with a numbered report: root cause, before/after table, gate results, commit hashes.
- Loop: capture web reference -> capture Unity -> score -> attribute error -> fix at the cause -> rebuild -> recapture -> rescore -> regression gates -> commit. Merges into `agent/parity/integration` always run the full regression first.
- Tooling: Unity CLI (`games/maze-puzzle/tools/parity/windows/unity/build.ps1`, `capture.ps1`, `perf.ps1`, `build-play.ps1`). On 2026-10-08 (`agent/chore/tools-platform`) those Windows wrappers moved from `unity/` to `windows/unity/`. `run-framerate.ps1` moved with them. `windows/canvas-conformance/run-unity.ps1` moved from `canvas-conformance/`. Unity MCP was unavailable (authentication timed out).

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
| homefix-clean | Home layer-cache, blur, and garbage fixes (section 9.1) |

On 2026-10-05 the merged branches and their worktrees were deleted; the commits stay in `maze-puzzle` history. Glyph is kept as tag `archive/parity-glyph`.

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
cd games/maze-puzzle/tools/parity
npm ci
pip install numpy scikit-image Pillow opencv-python
# web references: see games/maze-puzzle/tools/parity/README.md (Playwright harness)
./windows/unity/build.ps1      # Windows parity player
./windows/unity/capture.ps1 -States unity/screenshot-states.json -Profile zero
# score stills and flows with the diff/score scripts under games/maze-puzzle/tools/parity (SPEC.md v1.6 is the contract)
```

Preview in the Editor: open the project at `games/maze-puzzle/unity/maze-puzzle` (Unity 6000.4.6f1), open `Assets/Scenes/MainGameScene.unity`, press Play. F1 hint, F2 page browser; menu Orbital Salvage -> Parity Viewer -> Enabled / Reset Profile.

## 9. User-reported Editor Play lag (closed 2026-10-04)

- Report: after Play in the Editor (Simulator, iOS Notch Device Large, width 1284) the game lagged or was unplayable; the home page also rendered incorrectly (mustard-yellow background, dark-blue sphere in place of the emoji).
- Causes, measured: (1) Wrong colours were live-frame drift. `CssFrame.Build` unbaked a reused local rect every frame, so the hero face left the sphere and a debris sprite grew into a mustard page. Frozen `Draw()` never unbakes, which is why the parity stills stayed correct. (2) Lag was the 3x raster plus vSync, not the CPU painter. Before, at a real D3D12 1284x2778: frame avg 51.08 / p95 61.67 ms while Draw was avg 8.23 / p95 11.55 ms; background `(162,129,72)`, hero `(77,78,188)`.
- Workaround on the product path only: unbake once; vSync 0 and target 60; the raster snaps to the window and steps 3 to 2 to 1.5 to 1 when p95 over ~1 s exceeds 22 ms. Parity stills stay frozen `Draw` at 3x. Commits `6f871e3`, `478b974`, `90a9245`; merged to integration as `0b6f054`.
- After, governor on, true 1284x2778: D3D12 frame avg 29.02 / p95 24.69 ms, raster 430x932, background `(39,36,101)`, hero `(255,182,75)`. D3D11 p95 24.40 ms with the GPU at 100%. Canvas PNG shows the purple page and winking face.
- Remaining: phones untested; this Game view is a fixed 1284x2778, not the Simulator notch; at that size the governor stays at 1x so the page is softer than the 3x parity stills; D3D12 p99 is still 62.7 ms. A one-off `loading_t0000` 0.00 score during scoring was a missing-spec artifact against the camera clear, not a regression of `13_unity_loading_screen.png` (99.55/0.9908).

### 9.1 Home-page lag (closed 2026-10-05)

- Report: lag persisted and was mostly on home. Ablation probe (`agent/parity/homeperf`, flags only with `-editor-play-probe`, report `tools/parity/reports/perf-home-2026-10-05.md`) found: the layer cache split home into 37 full-raster layers (534 MB at 3x) baked through `ReadPixels`, and dropped all of them on every quality step and every override value change. Nibbles tap: two rebuilds of 2.8-3.5 s at 3x. Ramp: three rebuilds of 0.45-1.9 s. Steady 3x: Draw cost 25-27 ms of the median, 15-20 ms of it full-target backdrop blur. GC 229-234 KB/frame. Shadows, layer blits, app sync, and scene searches were not causes; the GPU timer stayed under 2 ms.
- Fix (`agent/parity/homefix-clean`): cache key is the set of overridden ids, not their values; GPU-side layer copies, hidden-only runs skipped (29 layers); governor defers baking 0.9 s after a resize and remembers the settled scale; blur limited to the element rect plus reach; per-frame string work cached. Regression test `LayerCacheTests`.
- After: tap max 33-158 ms with no rebuild; ramp rebuilds 2.7-4.6 ms; 3x median 20-23 ms; 1x median 16.0-16.3 ms; GC ~107-112 KB/frame. Stills, game/pause hashes, and animated home dumps SHA256-identical. Not reached: GC under 60 KB/frame; some forced-3x runs still have frames over 100 ms outside the paint path.

### 9.2 UI fixes (2026-10-07, `agent/fix/ui`)

- Menu, toast age, pause text reflow, gesture hint, and the failure clock are on `agent/fix/ui` (not merged). Report: `tools/parity/reports/ui-fixes-2026-10-07.md`.
- Centered block: a div whose captured run is centered in the element (gaps match within 1.5 px). Extra lines wrap to that width and the modal card grows upward. One line keeps the captured run x, so the pause still stayed `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`.
- 1x blur stays skipped. The D3D12 probe did not meet the keep-blur rule: draw cost was under 1 ms, but frame medians were 8.0-11.9 ms rather than 15.5-16.5 ms, and two p95 pairs worsened by more than 2 ms. Paint raster stayed 430x932 at both Game view sizes.

### 9.3 Funnel damping (2026-10-08, `agent/fix/funnel`)

The web prototype is no longer frozen for this one change. `games/maze-puzzle/web/engine/physics.js` `resolveMagneticFunnel` and the C# port use the same centering line, `(dx * 16 - vx * 8) * dt`. Nothing else under `web/` was edited. Report: `tools/parity/reports/funnel-fix-2026-10-08.md`.

Regenerating after a physics edit: from the repo root, with Playwright Chromium installed (`cd games/maze-puzzle/tools/parity; npm ci; npx playwright install chromium`), run `node games/maze-puzzle/tools/parity/golden/generate.mjs --only physics` (Chromium is authoritative; `--engine node` is a cross-check). Also re-run `node games/maze-puzzle/tools/parity/web/record-playfield.mjs`, `node games/maze-puzzle/tools/parity/canvas-conformance/record.mjs`, and `node games/maze-puzzle/tools/parity/web/app-flows.mjs`, then Unity `OrbitalSalvage.Render.Editor.LiveConformanceBatch.Run` (`-live-out` and `-live-streams`) and EditMode `FullRun_BitExact`. On 2026-10-08 the playfield JSON, the 30 canvas streams, the app-flow goldens, and `fixtures/engine.txt` regenerated byte-identical to the previous commit. 23 of 35 physics runs changed. Profile-zero page PNGs were not recaptured.

## 10. Open items

- Real device verification (touch, safe area, rotation, thermals).
- Six stills and 49 flow steps below the strict bar, mostly text-edge anti-aliasing; `maps_start_03` bakes an active-level PLAY tile; `daily_chain` steps at 95-97%.
- Remaining audio cues are not wired, to protect deterministic parity paths.
- `Assets/Audio/*.wav` kept pending a proven-unused check.
