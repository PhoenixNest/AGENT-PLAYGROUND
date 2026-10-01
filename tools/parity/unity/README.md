# Unity capture track

Drives the Unity project in `games/maze-puzzle/unity/maze-puzzle` through the SPEC (`tools/parity/SPEC.md`) states and writes `<stateId>__<profile>.png`, `.layout.json`, `.unity-notes.json` and a trace per capture.

## Run

```powershell
pwsh tools/parity/unity/build.ps1
pwsh tools/parity/unity/capture.ps1 -Out tools/parity/out/unity
```

`build.ps1` runs Unity in `-batchmode -quit` with `OrbitalSalvage.Editor.ParityBuild.BuildPlayer` and produces `games/maze-puzzle/unity/maze-puzzle/Builds/Parity/OrbitalSalvageParity.exe` (Win64 Mono, define `ORBITAL_PARITY`). Log: `tools/parity/out/unity-logs/build.log`.

`capture.ps1` options:

| Option | Meaning |
| --- | --- |
| `-States` | states JSON; default `tools/parity/states.json` when present, else `smoke-states.json` |
| `-Out` | output directory (default `tools/parity/out/unity`) |
| `-Only a,b` / `-Profile zero\|iphone` | subset of states / one profile |
| `-Isolated` | one player process per (state, profile); results are merged into `timings.json` / `errors.json` |
| `-WindowStyle` | default `Minimized` |
| `-TimeoutSec` | player timeout |

Exit codes: 0 ok, 1 one or more captures failed, 2 usage or invalid states file. Unsupported ops are not failures: they are recorded in `<state>__<profile>.unity-notes.json` and the capture still happens.

Player CLI (SPEC section 11): `OrbitalSalvageParity.exe --parity <statesJson> <outDir> [--only ids] [--profile p] [--summary dir]`.

## Builds leave the working tree clean

`git status --porcelain` stays empty after `build.ps1` and `capture.ps1`; there is nothing to revert.

- `ProjectSettings.asset`: `ParityBuild` snapshots every `PlayerSettings` value it changes (fullscreen mode, resolution, resizable window, run in background, visible in background, scripting backend, stripping level, splash) and restores them in a `finally` block, then calls `AssetDatabase.SaveAssets()`. `ORBITAL_PARITY` is passed through `BuildPlayerOptions.extraScriptingDefines`, so no define is written to the project. Unity still re-serialises the file on that save and adds fields that were absent from the committed version (`targetPixelDensity: 30`, a `buildNumber` map, iOS/tvOS/visionOS target OS strings), which no `PlayerSettings` setter can undo. `ParityBuild` therefore keeps the original bytes of the file and writes them back if they differ after the save (logged as a warning in `build.log`).
- TMP font assets (`Assets/Resources/Fonts/TMP`): they are dynamic atlases with `m_ClearDynamicDataOnBuild: 1`, so `TMP_PreBuildProcessor` empties each atlas at the start of every player build (in the committed assets that rewrote `m_ColorSpace: 1` to `0` and `_typelessdata: cd` to `00` in the atlas subasset). `ParityAssetBuilder.BuildAll` now saves each font asset already in that emptied state (it sets `m_ClearDynamicDataOnBuild` and calls `ClearCharacterAndGlyphTablesInternal`), and the regenerated assets are committed, so builds no longer change them. Glyphs are generated at runtime from the same TTFs into the empty atlas, in the editor as in the player, so rendering is unaffected.
- Re-run `Unity -batchmode -quit -executeMethod OrbitalSalvage.Editor.ParityAssetBuilder.BuildAll` after changing fonts and commit the result.
- On this machine `core.autocrlf=true` / `core.eol=crlf` make git compare the stored CRLF size of text assets that Unity rewrites with LF. If `git status` lists a file whose `git diff` is empty, `git add` that file once to refresh its index entry.

## Window mode

| Mode | Result |
| --- | --- |
| `-batchmode` | renders black PNGs |
| `-nographics` | not used (it disables rendering) |
| Hidden window (`-WindowStyle Hidden`) | renders black PNGs |
| Minimized window (default) | renders correctly |
| Normal window | renders correctly (steals focus) |

The player is therefore launched windowed and minimised, 430x932, `runInBackground` on. A `-Batchmode` switch exists in `capture.ps1` only to reproduce the black output.

## How the screen is faked

- All canvases are forced to Screen Space - Camera and drawn by a parity UI camera (layer 5) into a 1290x2796 `RenderTexture`; gameplay cameras are retargeted to the same texture and exclude layer 5.
- The real `Screen` stays 430x932. `ParityScreen.Override` reports 1290x2796 to the safe-area code; the CanvasScaler resolves to `scaleFactor = 3` with a 430x932 canvas rect, which is the 430x932 reference at 3x.
- Safe areas come from `ISafeAreaProvider`: `zero` = full screen; `iphone` = top 59pt, bottom 34pt (x3, bottom-left origin) through `ProfileSafeAreaProvider`, consumed by `SafeAreaFitter`.
- PNG: `ReadPixels` from the RT, RGB24, `EncodeToPNG`. Colour space is Gamma, so values are exact (background `#131133` = 19,17,51).

## Determinism

- Virtual clock: `Time.captureDeltaTime = fixedDeltaTime = 0.016`; `ParityFrameDriver` (execution order -32000) sets `NowMs = frame * 16`. `advance(ms)` yields one Unity frame per 16 ms step; non-advancing waits run with `Time.timeScale = 0`.
- Wall clock: `IParityClock` returns UTC `2026-10-01T16:00:01Z + NowMs` in the Pacific zone.
- Per state: scene reload, statics reset, `PlayerPrefs` and the economy save file backed up, cleared and restored; `UnityEngine.Random.InitState(0x5EED1234)` and `JsRandom` (mulberry32, same seed).
- Pointer ops use a custom world-corner hit test and `ExecuteEvents` (uGUI `Graphic.Raycast` is unreliable with a RenderTexture camera).
- `Selectable` colour tweens use unscaled real time; after any pointer op the runner flushes 0.25 s of real time before capture.

Measured: two full smoke runs (16 captures) and two `-Isolated` runs gave byte-identical PNGs, and isolated output equals the in-process full run. An in-process run of a subset in a different order once differed by 212 px in the physics area of `game_sector_1__iphone` (Physics2D history, unreproduced); use `-Isolated` when exact repeatability matters.

Known remaining sources until the logic port lands: Physics2D state across scene reloads, legacy `Input` in gameplay (cannot be injected), `Selectable` real-time tweens, `Time.unscaledTime` use in runtime scripts.

## Timings

Build: 10-67 s (cold first run longest). Capture: ~0.25 s per state in-process after ~5 s player start; full smoke run (16 captures) 8-17 s in-process, ~125 s with `-Isolated` (~8 s process start per capture).

## Baseline adapter

`BaselineAdapter` maps SPEC ops onto the current `UIManager` / `GameManager` / `EconomyManager`. The DOM id to Unity path table is `BaselineIdMap.Entries` (80 mapped web ids, 13 unmapped); `layoutUnmappedIds` in each `.unity-notes.json` lists the unmapped ones. The baseline UI has no toast, gesture hint, privacy or reset modals, no scrollable page, and ignores world-tab clicks.
