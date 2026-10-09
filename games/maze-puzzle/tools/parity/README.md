# Web-to-Unity pixel parity tooling

`SPEC.md` is the binding contract (v1.6). This file only says how to run things. Run commands from `games/maze-puzzle/tools/parity`. Windows launchers are under `windows/`. Node, Python, and the goldens are cross-platform. See Platforms.

Measured on this machine on 2026-10-04 (D3D11, RTX 4060 Laptop, windowed). These are harness numbers, not a claim that UI restoration is finished.

| Check | Result |
| --- | --- |
| 11 stills, zero profile, bar 99.0% / SSIM 0.985 | 5 pass: loading 99.55/0.9908, game 99.51/0.9919, pause 99.25/0.9877, settings 99.20/0.9890, debrief 99.05/0.9856. 6 fail: trophy 98.80/0.9815, skies 98.72/0.9782, home 98.67/0.9799, daily 98.05/0.9706, hats 98.01/0.9649, donuts 97.78/0.9614. Game SHA256 `38730953997DE30D033C10E6CFF0B9AE2E47FBAB1671D07178039B484609A12A`, pause `CE4A6AE83A5563E98F7F722334E17A7FC06FA1375C26390A9714FC838FEE7078`. |
| Gameplay streams | 30/30 byte-identical, live `GameRenderer` vs `StreamPlayer` replay (`LiveConformance.Export`). |
| Controller goldens | 9/9 `AppFlowTests.Flow_MatchesWeb` (inside EditMode 380/380). |
| Flow shots | 55 steps, 6 at the bar, 0 BLANK (`score_flows.py`). |
| HomeChrome.Draw | 1 run, n=20, avg 3.049 ms, p95 4.409 ms. |
| Live frame (600 steps, scale 1) | avg 9.38 ms, p95 14.32 ms (fence included). |

The default player (Editor Play and `games/maze-puzzle/output/maze-puzzle-windows/OrbitalSalvage.exe`, no flags) boots `ParityViewer` → `AppHost`/`AppController` with pointer input, a wall clock, `FileProfileStore`, and the DSP player. `--parity` and the parity self-test flags keep the frozen clock and silent audio. Landscape keeps the 430×932 page letterboxed. Not verified on a phone.

## Platforms

Windows is the machine these wrappers run on today. macOS (Xcode, App Store) is later. The Windows scripts stay tracked. `macos/` is a placeholder for the equivalents; it has no shell scripts.

Layout choice: one `windows/` directory and one `macos/` directory at the toolset root, each with `unity/` and `canvas-conformance/`. State JSON stays in `unity/` (`screenshot-states.json`, `smoke-states.json`). `lib/roots.ps1` stays in `lib/` and is dot-sourced by the wrappers. `lib/roots.mjs` is the Node walker.

| Class | Where | Evidence |
| --- | --- | --- |
| Windows-only launchers (6) | `windows/unity/build.ps1`, `build-play.ps1`, `capture.ps1`, `perf.ps1`, `run-framerate.ps1`; `windows/canvas-conformance/run-unity.ps1` | Each calls `Start-Process` on `C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe` or `OrbitalSalvageParity.exe`. `build.ps1` and `build-play.ps1` pass `-buildTarget Win64`. `perf.ps1` and `build-play.ps1` name D3D11/D3D12. `run-framerate.ps1` writes `$env:TEMP`. |
| Windows-only helper (not moved) | `lib/roots.ps1` | PowerShell only. `Get-MazeRoot` joins `tools\parity\package.json` and `web\index.html` with backslashes. No `.exe`. The six launchers dot-source it. |
| Windows-only Editor builds (not moved) | `Assets/Scripts/Editor/ParityBuild.cs`, `PlayBuild.cs` | `BuildTarget.StandaloneWindows64`. `PlayBuild` sets `GraphicsDeviceType.Direct3D11`. |
| Cross-platform | The other tracked files under this directory: Node/Playwright generators, Python diff and canvas tools, goldens, fixtures, recordings, `states.json`, `sequences.json`, `scripts/funnel-study.mjs`, `scripts/measure-web-layout-430.mjs`, `SPEC.md`, fonts, emoji | `git ls-files` listed 323 files before this doc change. Seven are `.ps1` (the six launchers plus `roots.ps1`). A search of the rest for `Start-Process`, `Program Files`, `Direct3D`, `force-d3d`, and `StandaloneWindows64` hit documentation of the Windows player (`SPEC.md`, `unity/README.md`, `CLEANUP.md`) and dated reports, not generators or fixtures. |

`ConfigureEmojiSprites.cs` and `ParityAssetBuilder.cs` only rewrite `\` to `/`. They are not Windows build targets.

Migration plan:

1. Add macOS `.sh` equivalents beside the Windows wrappers (`macos/unity/` for build, capture, perf; `macos/canvas-conformance/` for run-unity). Do not add them until they have been run on a Mac.
2. Recapture screenshot baselines on Mac. The committed still hashes were measured on Windows D3D12. Physics goldens are pure math and should carry over; verify that on the first Mac run.
3. Retire the `.ps1` files only after the `.sh` versions pass the same checks.

## Setup

```powershell
cd games/maze-puzzle/tools/parity
npm ci
pip install numpy scikit-image Pillow opencv-python
```

Playwright 1.63.0 (Chromium build 1243, headless shell) is pinned in `package.json`. The harness never touches `games/maze-puzzle/web/`; all instrumentation is injected at runtime.

## Layout

| Path                       | What                                                                                           |
| -------------------------- | ---------------------------------------------------------------------------------------------- |
| `web/harness.mjs`          | The library: static server, clock, RNG, animation clock, audio stub, emoji, all ops, traces    |
| `web/page-init.js`         | Browser-side init script injected before the app (clock, registry, emoji, canvas, trace)       |
| `web/capture-web.mjs`      | CLI: `states.json` to PNG + layout JSON (+ one state trace per state)                          |
| `web/trace-web.mjs`        | CLI: `sequences.json` to `<id>.trace.json`                                                     |
| `web/dump-spec.mjs`        | CLI: full element tree, computed styles, matched `:active`/`:hover`/`:disabled` rules per page |
| `states.json`              | Generated list of states (`node scripts/gen-states.mjs`, then Prettier)                        |
| `sequences.json`           | Generated list of trace sequences (`node scripts/gen-sequences.mjs`, then Prettier)            |
| `diff/diff.py`             | Pixel + SSIM + box + trace diff, writes `reports/iter-NN/` and `out/reports/iter-NN/`          |
| `diff/trace_diff.py`       | Trace comparison (library and CLI)                                                             |
| `scripts/contact_sheet.py` | Labelled thumbnail sheets per profile                                                          |
| `out/`                     | Git-ignored outputs: `out/web`, `out/unity`, `out/reports`                                     |

`states.json` and `sequences.json` are generated; edit `scripts/gen-states.mjs` / `scripts/gen-sequences.mjs` and regenerate, then run `npx --yes prettier@3 --write states.json sequences.json`.

## Capture the web reference

```powershell
node web/capture-web.mjs                                  # all states, both profiles, out/web
node web/capture-web.mjs --only home_default,shop_rings_default --profile iphone
node web/capture-web.mjs --states states.json --out out/web --concurrency 4
```

Writes `<stateId>__<profile>.png` (1290 x 2796, RGB), `<stateId>__<profile>.layout.json`, `traces/<stateId>.trace.json` (from the first profile of the state) and `_meta.json` (Chromium version, launch args, per-run `perfOffset`, timings). Each job runs in a fresh browser context; jobs run in a pool (default 4). Exit code 1 if any job failed, 2 on CLI errors. The full set takes about 10 minutes on a loaded machine.

`_meta.json` contains wall-clock timings, so it is the only output that differs between identical runs.

## Traces

```powershell
node web/trace-web.mjs                          # all sequences into out/web/traces
node web/trace-web.mjs --only play_sector_1 --out out/web
```

Sequences are flat op lists using the vocabulary of SPEC section 8 so Unity can replay them. `play_sector_1` replays a scripted drag program and fails if the debrief never opens (`expect` in `sequences.json`). Traces are very large on purpose: the audio stub records every node and param call (SPEC 4.6, 10).

## Dump the CSS/DOM spec for Unity

```powershell
node web/dump-spec.mjs                 # every page and modal, both profiles, into out/web/spec
node web/dump-spec.mjs --only home,game
```

Writes `spec/<page>__<profile>.json` (element tree with rects, computed paint and layout properties, `::before`/`::after`, text runs, matched pseudo-state rules), `spec/keyframes.json` and `spec/css-vars.json`.

## Diff against Unity

```powershell
python diff/diff.py --web out/web --unity out/unity --iter 01
python diff/trace_diff.py --web out/web/traces --unity out/unity/traces
```

`diff.py` writes `reports/iter-NN/report.md` and `report.json`, plus heatmap and three-panel diff PNGs under `out/reports/iter-NN/`. `report.json` and `reports/iter-*/images/` are gitignored. The 2026-10-01 iter-00 dump and the iter-01 images were removed; `reports/iter-01/home.md` is the kept home note (`CLEANUP.md`). Missing Unity files are reported as `MISSING`. Exit code: 0 all pass, 1 any fail, 2 when something is missing or the arguments are invalid (2 wins over 1).

## Contact sheets

```powershell
python scripts/contact_sheet.py
```

Writes `out/web/contact_sheet_zero.png` and `out/web/contact_sheet_iphone.png`.

## Determinism check

```powershell
node web/capture-web.mjs --out out/web
node web/capture-web.mjs --out out/web-b
node web/trace-web.mjs --out out/web
node web/trace-web.mjs --out out/web-b
Get-ChildItem out/web -Recurse -File -Include *.png,*.layout.json,*.trace.json | ForEach-Object {
  $rel = $_.FullName.Substring((Resolve-Path out/web).Path.Length)
  if ((Get-FileHash $_.FullName).Hash -ne (Get-FileHash (Join-Path (Resolve-Path out/web-b) $rel)).Hash) { "DIFF $rel" }
}
```

No output means every PNG, layout and trace is byte-identical.

## Other scripts

`scripts/fetch-assets.mjs`, `font-usage-scan.mjs`, `emoji-scan.mjs`, `tracked-classes.mjs` maintain the pinned assets and `tracked-classes.json`. `scripts/paused-clock-probe.mjs` and `scripts/raf-probe*.mjs` are the SPEC §4.3 clock probes. `probe-win.mjs`, `probe-game.mjs`, and `safe-area-probe.mjs` were removed on 2026-10-08; nothing else referenced them (`CLEANUP.md`).
