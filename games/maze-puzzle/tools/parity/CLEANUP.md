# Parity toolset cleanup (2026-10-08)

Move: repo-root `tools/parity` to `games/maze-puzzle/tools/parity` (`git mv`, 346 paths, commit on `agent/chore/tools-move`). Repo-root `tools/` does not remain. This file records keep / remove / ignore decisions made in that change. Byte sizes are working-tree lengths measured before `git rm`.

Search used for “who references it”: `rg` over the worktree, excluding `.git`, `Library`, and `node_modules`. Unity consumers that compute a path (not only the string `tools/parity`) were read in `ParityAssetBuilder.cs`, `Fixtures.cs`, `AppFlowTests.cs`, `ShopGiftTests.cs`, `ParityChecks.cs`, and `FrameRateHarness.cs`.

## Removed

Working-tree bytes removed: **18,783,157 (17.91 MiB)**. `git rm` deleted them from the index and the worktree (not `git rm --cached`). No tracked file was left on disk as an ignored copy.

| Path | Bytes | Files | Reason |
| --- | ---: | ---: | --- |
| `reports/iter-00/` | 4,691,256 | 2 | Generated 2026-10-01 diff dump (`report.md` starts “Generated 2026-10-01T06:36:11”, 0 pass / 314 fail). `montage.py` takes `--report` as a required argument and does not open this file. No test reads it. |
| `reports/iter-01/images/` | 5,425,232 | 6 | Heatmap and panel PNGs from the first home paint. `reports/iter-01/home.md` records the later 98.67% / 0.9799 measurement and says the 93% figures are superseded. |
| `reports/iter-01/report.json` | 167,940 | 1 | Generated machine report for that same 93% pass (`report.md` “Generated 2026-10-01T10:05:43”, 0 pass / 2 fail). |
| `reports/iter-01/report.md` | 4,785 | 1 | The generated 93% table. The written conclusion that is still current is `home.md`. |
| `reports/physics-framerate-2026-10-07-debris.csv` | 8,417,537 | 1 | `FrameRateHarness.Measure` writes this path (`debrisPath`). The markdown report keeps the summary tables. |
| `reports/physics-framerate-2026-10-07-runs.csv` | 31,332 | 1 | Same harness writes `runsPath`. |
| `reports/physics-framerate-2026-10-07-accum.csv` | 21,205 | 1 | `FrameRateHarness.MeasureAccumulated` writes this path. |
| `reports/physics-framerate-2026-10-07-accum.txt` | 24 | 1 | Same method writes this path. The markdown already states the 264-row / 0-mismatch result. |
| `reports/physics-framerate-2026-10-07-frames/` | 17,560 | 3 | Harness `frameDir` traces (`home-s1-baseline-1.txt`, `home-s3-baseline-1.txt`, `home-auto-timeline-1.txt`). |
| `scripts/probe-win.mjs` | 1,828 | 1 | `rg` found no reference outside the file. It imported `program-lib.mjs`, which `gen-states.mjs` and `gen-sequences.mjs` still import. |
| `scripts/probe-game.mjs` | 1,999 | 1 | `rg` found no reference outside the file. |
| `scripts/safe-area-probe.mjs` | 1,005 | 1 | `rg` found no reference. SPEC §3 records the safe-area measurement and does not name this file. |
| `scripts/compare-runs.mjs` | 1,454 | 1 | `rg` found no reference. It compared two capture directories and was not in README or SPEC. |
| `diff/.gitkeep`, `web/.gitkeep`, `audio/.gitkeep`, `golden/.gitkeep`, `reports/.gitkeep` | 0 | 5 | Each directory is tracked by other files. |

## Ignored (repo-root `.gitignore`)

`git check-ignore -v` was run for each pattern after the ignore lines were added.

| Pattern | What it matched in `git check-ignore -v` |
| --- | --- |
| `games/maze-puzzle/tools/parity/**/out/` | Also matched by the toolset `.gitignore` line `out/` (that line is the one `check-ignore` printed for `out/unity`, `audio/out`, and `canvas-conformance/out`). |
| `games/maze-puzzle/tools/parity/**/__pycache__/` | Present for regenerated Python caches. |
| `node_modules/` (already at repo root) and the toolset `.gitignore` line `node_modules/` | `check-ignore` printed the toolset line for `games/maze-puzzle/tools/parity/node_modules/playwright/package.json`. |
| `**/.local-browsers/` | `.gitignore:48` for `games/maze-puzzle/tools/parity/.local-browsers/chromium/chrome.exe`. |
| `**/ms-playwright/` | `.gitignore:49` for `games/maze-puzzle/tools/parity/ms-playwright/chromium/chrome.exe`. |
| `games/maze-puzzle/tools/parity/reports/**/*.csv` | `.gitignore:43` for `physics-framerate-2026-10-07-debris.csv`. |
| `games/maze-puzzle/tools/parity/reports/*-frames/` | `.gitignore:44` for `physics-framerate-2026-10-07-frames/home-s1-baseline-1.txt`. |
| `games/maze-puzzle/tools/parity/reports/*-accum.txt` | `.gitignore:45` for `physics-framerate-2026-10-07-accum.txt`. |
| `games/maze-puzzle/tools/parity/reports/iter-*/report.json` | `.gitignore:46` for `reports/iter-00/report.json`. |
| `games/maze-puzzle/tools/parity/reports/iter-*/images/` | `.gitignore:47` for `reports/iter-01/images/home.png`. |
| `**/[Bb]uilds/` (already present) | `.gitignore:30` for `Builds/Parity/OrbitalSalvageParity.exe`. |
| `*.log` (already present) | Player and Unity logs under `out/` are ignored by the toolset `out/` line (`out/unity-logs/build.log`). |

These stayed un-ignored (`git check-ignore` exit 1): `golden/fixtures/engine.txt`, `SPEC.md`, `states.json`, `canvas-conformance/streams/s01_f001.json`, `reports/physics-framerate-2026-10-07.md`, `reports/iter-01/home.md`, `unity/screenshot-states.json`, `scripts/funnel-study.mjs`.

There was no repo-root `tools/` ignore line to delete.

## Kept

| Group | Reason |
| --- | --- |
| `web/` harness (`harness.mjs`, `page-init.js`, `capture-web.mjs`, `trace-web.mjs`, `dump-spec.mjs`, `distill-spec.mjs`, `app-flows.mjs`, `record-playfield.mjs`, `game-sync.mjs`, `png.mjs`) | README run instructions. `app-flows.mjs` writes `golden/app`, which `AppFlowTests.GoldenDir` reads. `record-playfield.mjs` writes `Assets/Resources/Parity/Playfield`. `PhysicsSnapshot.cs` cites `game-sync.mjs`. |
| `golden/` generators, `fixtures/`, `golden/app/`, `golden/dotnet/` | `Fixtures.cs` reads `tools/parity/golden/fixtures`. `PhysicsGoldenTests` requires `physics/manifest.json`. `AppFlowTests` reads `golden/app`. |
| `canvas-conformance/` scenes, streams, `record.mjs`, `lib/`, `inventory.mjs`, `make-scenes.mjs`, `run-unity.ps1`, `compare.py`, `replay.mjs`, `replay.html`, `probe-metrics.mjs` | `record.mjs` writes the streams. `LiveConformanceBatch.Run` takes `-live-streams`. `probe-metrics.mjs` is named from `CanvasConformance.cs` and `DESIGN.md`. `make-scenes.mjs` and `run-unity.ps1` are the scene generator and the Unity runner; no other file spells those filenames. |
| `assets/` (fonts, emoji, `manifest.json`) | `ParityAssetBuilder` syncs `tools/parity/assets`. `THIRD_PARTY_NOTICES.md` cites `manifest.json` and `fetch-assets.mjs`. `ParityFonts` documents SPEC section 5. |
| `audio/` generators and committed JSON (`ref-manifest.json`, `rng-counts.json`, `trace-golden.json`, `cues.mjs`, `render-ref.mjs`) | `ParityChecks.FindAudioDir` requires `ref-manifest.json`. `AudioParityTests` reads that file, `trace-golden.json`, and `audio/out/ref` (the WAVs are not in the checkout). |
| `evidence/google-fonts-css2.css` | SPEC section 5 cites the committed Google Fonts response (9243 bytes). |
| `unity/build.ps1`, `capture.ps1`, `perf.ps1`, `build-play.ps1`, `run-framerate.ps1`, `screenshot-states.json`, `smoke-states.json` | Workflow archive and `unity/README.md`. `capture.ps1` falls back to `smoke-states.json` when `states.json` is absent. `screenshot-states.json` has 23 states; `record-playfield.mjs` reads it. |
| `states.json`, `sequences.json`, `tracked-classes.json` | `capture-web.mjs` / `capture.ps1` read states. `trace-web.mjs` reads sequences. `harness.mjs` reads `tracked-classes.json` (`readFileSync`). SPEC §9 says to regenerate it with `scripts/tracked-classes.mjs`. |
| `scripts/gen-states.mjs`, `gen-sequences.mjs`, `program-lib.mjs`, `fetch-assets.mjs`, `emoji-scan.mjs`, `font-usage-scan.mjs`, `font-coverage.py`, `contact_sheet.py`, `score_flows.py`, `check-transition.mjs`, `funnel-study.mjs`, `measure-web-layout-430.mjs` | README or SPEC names them, or another script imports them. `check-transition.mjs` is SPEC line 873. `funnel-study.mjs` and `measure-web-layout-430.mjs` are the physics measurement tools (funnel report and the 2026-10-07 layout note). |
| `diff/diff.py`, `diff/trace_diff.py` | README and SPEC. |
| `SPEC.md`, `README.md`, this file | Contract and run instructions. |
| Report markdown: `funnel-fix-2026-10-08.md`, `ui-fixes-2026-10-07.md`, `shop-gift-2026-10-08.md` plus its PNGs, `perf-2026-10-03.md`, `perf-2026-10-04.md`, `physics-framerate-2026-10-07.md`, `iter-01/home.md`, `iter-01-text.md`, `spec-requests/processed/` | CHANGELOG, the workflow archive, SPEC, or `ShopGiftTests.ReportDir` point at them. The shop-gift PNGs are the directory `ShopGiftTests` writes when the walk finds it. |
| `reports/physics-framerate-2026-10-07-web-layout.json` | 314 bytes. `measure-web-layout-430.mjs` writes this shape (`source` field matches the script). The markdown does not require the file; it was kept because it is small. |

## Kept, unsure

Nothing imports these. They stayed because a contract or a neighbouring tool names them, or because deleting them was not proven safe.

| Path | Why it is unsure |
| --- | --- |
| `scripts/paused-clock-probe.mjs`, `scripts/raf-probe.mjs`, `scripts/raf-probe.html`, `scripts/raf-probe-app.mjs` | SPEC §4.3 names these four files as the probe that recorded Playwright’s fake clock. No other script imports them. |
| `diff/montage.py` | Only its own docstring mentions it. README and SPEC name `diff.py` and `trace_diff.py`, not montage. |
| `canvas-conformance/hotspots.py` | `rg` hits were the file itself. |
| `canvas-conformance/textprobe.py` | `rg` hits were the file itself. |
| `reports/physics-framerate-2026-10-07-web-layout.json` | 314-byte script output. Not cited by a test. |
