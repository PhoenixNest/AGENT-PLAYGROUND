# maze-puzzle Unity project

Unity 6000.4.0f1 project for Orbital Salvage. Open this folder in the Editor. The scene is `Assets/Scenes/MainGameScene.unity`.

Play mode boots `ParityViewer`. The Editor menu below is for preview, builds, and saved-profile tools. It does not replace Play mode.

Output folders are `games/maze-puzzle/output/` (gitignored), two levels above this project:

| Variant                     | Folder                                                            |
| --------------------------- | ----------------------------------------------------------------- |
| Windows play player         | `games/maze-puzzle/output/maze-puzzle-windows/OrbitalSalvage.exe` |
| iOS simulator Xcode project | `games/maze-puzzle/output/maze-puzzle-simulator`                  |
| iOS device Xcode project    | `games/maze-puzzle/output/maze-puzzle`                            |
| Page Preview PNGs           | `games/maze-puzzle/output/preview/`                               |

`OutputPaths.For` in `Assets/Scripts/Viewer/OutputPaths.cs` is the folder name. From this project the relative path is `../../output/<name>`.

## Editor menu

Priorities are 100 (Preview), 200 (Build), 300 (Play), and 400 (Tools). Adjacent groups differ by at least 11, so Unity draws separators. Checked items use a validate method and `Menu.SetChecked`.

| Path                                                  | What it does                                                                                                                                                                                                      | When to use                                                                    |
| ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| `Orbital Salvage/Preview/Open Window`                 | Opens Page Preview on `home_default`.                                                                                                                                                                             | Browse specs without choosing a page first.                                    |
| `Orbital Salvage/Preview/home_default`                | Opens Page Preview on that spec.                                                                                                                                                                                  | Home.                                                                          |
| `Orbital Salvage/Preview/maps_world_1_fresh`          | Opens Page Preview on that spec.                                                                                                                                                                                  | Maps, world 1 fresh.                                                           |
| `Orbital Salvage/Preview/daily_default`               | Opens Page Preview on that spec.                                                                                                                                                                                  | Daily shift.                                                                   |
| `Orbital Salvage/Preview/shop_rings_default`          | Opens Page Preview on that spec.                                                                                                                                                                                  | Shop, rings tab. Other shop specs are in the window dropdown.                  |
| `Orbital Salvage/Preview/achievements_none`           | Opens Page Preview on that spec.                                                                                                                                                                                  | Trophies / achievements.                                                       |
| `Orbital Salvage/Preview/settings_default`            | Opens Page Preview on that spec.                                                                                                                                                                                  | Settings.                                                                      |
| `Orbital Salvage/Preview/loading_t0500`               | Opens Page Preview on that spec.                                                                                                                                                                                  | Loading.                                                                       |
| `Orbital Salvage/Preview/game_lvl01_t1000ms`          | Opens Page Preview on that spec.                                                                                                                                                                                  | Game, level 1 at 1000 ms.                                                      |
| `Orbital Salvage/Preview/modal_pause`                 | Opens Page Preview on that spec.                                                                                                                                                                                  | Pause modal.                                                                   |
| `Orbital Salvage/Preview/modal_debrief_exact`         | Opens Page Preview on that spec.                                                                                                                                                                                  | Debrief modal.                                                                 |
| `Orbital Salvage/Preview/modal_defeat_12_of_20`       | Opens Page Preview on that spec.                                                                                                                                                                                  | Defeat modal.                                                                  |
| `Orbital Salvage/Build/Windows Play Player (D3D11)`   | Builds `OrbitalSalvage.exe` with Direct3D11 and restores `ProjectSettings.asset` byte for byte.                                                                                                                   | A Windows player that is not the parity capture build.                         |
| `Orbital Salvage/Build/iOS Xcode Project (Simulator)` | Exports a simulator Xcode project (`SimulatorSDK`, simulator architecture `ARM64`). Disabled when the iOS module is missing.                                                                                      | After iOS Build Support is installed. Signing and running need Xcode on a Mac. |
| `Orbital Salvage/Build/iOS Xcode Project (Device)`    | Exports a device Xcode project (`DeviceSDK`) to `output/maze-puzzle`. Disabled when the iOS module is missing.                                                                                                    | After the module is installed. Same Mac/Xcode requirement.                     |
| `Orbital Salvage/Play/Quality/Auto`                   | Sets `PlayQuality.Forced` to 0. Checkmark shows the current value.                                                                                                                                                | Let the play-mode governor choose the raster scale.                            |
| `Orbital Salvage/Play/Quality/1x`                     | Forces raster scale 1.                                                                                                                                                                                            | Readability check at the lowest product scale.                                 |
| `Orbital Salvage/Play/Quality/1.5x`                   | Forces raster scale 1.5.                                                                                                                                                                                          | Mid scale.                                                                     |
| `Orbital Salvage/Play/Quality/2x`                     | Forces raster scale 2.                                                                                                                                                                                            | Mid scale.                                                                     |
| `Orbital Salvage/Play/Quality/3x`                     | Forces raster scale 3.                                                                                                                                                                                            | Match the parity still raster.                                                 |
| `Orbital Salvage/Play/Parity Viewer Enabled`          | Toggles EditorPrefs `OrbitalSalvage.ParityViewer.Enabled` (default on). Applied the next time Play starts.                                                                                                        | Turn the Canvas2D viewer off for a comparison.                                 |
| `Orbital Salvage/Tools/Reset Saved Profile`           | Deletes `orbital-salvage-profile.json` under `Application.persistentDataPath` after a confirmation dialog. Batch mode skips the dialog and deletes. The log line is `[ParityViewer] reset profile` plus the path. | Clear a local profile. Cancel leaves the file in place.                        |

Auto steps the product raster through 1, 1.5, 2, and 3. Phones stay on 2 or 3. A scale that falls back is not tried again until the next launch. A forced menu value or `-parity-quality` still wins, including 1×. The settled scale is stored as `os.parity.scale.v2.<width>x<height>` after it has stayed put. F3 toggles the frame-time readout. A four-finger tap does the same in the Editor, in a Development build, or when PlayerPrefs `os.parity.hud` is 1.

## Page Preview

`Page Preview` paints `Assets/Resources/Parity/Spec/<id>__<profile>.json` with `HomeChrome` and does not enter Play mode. The dropdown lists every `__zero` spec, grouped by page. The iPhone profile toggle is shown for every id that also has an `__iphone` spec. That is every painted page. The default toggle stays on `zero`, which is the parity-player capture.

Play mode loads `__iphone` when `Screen.safeArea` has a top or bottom inset, and `__zero` otherwise (the editor Game view, a full-rect safe area). A missing iphone spec falls back to zero. The page is still contain-fitted in the full screen; the notch padding is inside the iphone spec. Those specs were captured at fixed insets of 59 CSS px top and 34 CSS px bottom, so a device with a different inset is off by a few px. Game-page playfield frames stay the zero recordings: the web canvas fills the frame and its size does not follow the insets.

iOS Player Settings target iPhone only (`targetDevice: 0`). The page-switch hint (`F1`) starts visible in the Editor and in Development builds, and hidden in a release player.

The default raster is the spec scale, 3×, so the texture is 1290×2796 for the 430×932 page. `Fast 1x raster` calls `HomeChrome.ResizeRaster(1)` and is not the still. Fit and zoom only change how the texture is drawn in the window.

Animation ms at 0 calls `HomeChrome.Draw()` (capture pose, no clock). That is the pose the parity player uses when the state has no `timeOffsetMs`. A positive slider value calls `HomeChrome.Draw(timeMs)`. A screenshot state with `timeOffsetMs` is `Draw(CaptureTimeMs + timeOffsetMs)`. On 2026-10-08, after `OutlineFonts.Clear()`, a D3D11 repaint of all 23 `screenshot-states.json` states through `PagePreviewPainter` was byte-identical to the parity-player stills, including `shop_rings_default`. An earlier shop PNG differed by 9 pixels (max channel delta 2, bbox (956,384)-(986,401)) on the edge of "Claim +100" because a previous editor paint had filled the static glyph cache. The window clears that cache before each page. HomeChrome was not changed. Save PNG writes `games/maze-puzzle/output/preview/<id>__<profile>.png`.

Batch entry points, from this project:

```powershell
-executeMethod OrbitalSalvage.Editor.PlayBuild.BuildPlayer
-executeMethod OrbitalSalvage.Editor.IosExport.ExportSimulator
-executeMethod OrbitalSalvage.Editor.IosExport.ExportDevice
```

The Windows wrapper is `games/maze-puzzle/tools/parity/windows/unity/build-play.ps1`.

After every iOS export, a post-process step sets the Xcode target `GameAssembly` to bundle id `com.unity3d.gameassembly`. The app id stays `io.dev.maze`. `UnityFramework` keeps `com.unity3d.framework`. This step is untested against a real Unity-generated Xcode project: this machine has no iOS Build Support module. On a Mac, run an export and look for `[IosExport] GameAssembly bundle id set to com.unity3d.gameassembly`. If the console warns `target GameAssembly not found`, send the target names from that line.

## Phone drawing path

An iOS or Android player starts from `GpuOpt.MobileDefaults` (`rrshadow,rrplus,backdroplow,playhalf,fastshape,fastarc`) when the `os.parity.opt` pref is empty. The Editor, the stills tooling, and the hash tests use `GpuOpt.Defaults`, which is empty, so their pictures stay pinned. A saved pref replaces the set entirely; `none` and `default` turn every option off for an on-device A/B. `playbatch` is a pref token that stays off the default: it corrupted the frame on an iPhone X.

- `rrshadow` draws a rounded-rectangle shadow in one quad instead of a blurred mask. `playhalf` draws the playfield at half size.
- `fastshape` takes the quick fill and stroke path for the many small pieces. Each piece's two eyes are separate circles on that path (`RenderOpts.FastShape`); the Editor path still draws both eyes as one path. The render target is bound once per run of pieces, and a piece's ellipse points are only built when a path needs them.
- `rrplus`, `backdroplow`, and `fastarc` are described, with their measurements, in `CHANGELOG.md`.

Measured on a Mac (Apple M4 Pro, Metal) with 320 pieces, render call including the GPU submit: level 1 went from 26.0 to 9.5 ms, level 21 from 29.0 to 15.8 ms, level 40 from 43.0 to 16.8 ms. Not measured on a phone. The ring's physics is unchanged. Tests: `DebrisEyeSplitTests`, `FastDebrisDrawTests`.

## Tapping the shop

The shop page is a captured page whose cards are swapped for owned, equipped, or locked ones (`CardStamp`, `HomeChrome.StampShopCards`) when no captured page has the player's inventory. After that swap the buttons are not where the captured file puts them. `HomeChrome.ShopStamped` is true and `HomeChrome.Elements` holds what is painted. `ParityViewer` hit-tests against `Elements` on a stamped page, through the `IReadOnlyList` overloads of `ParityPages.TryHitControl`, `TryPressAt`, `TryCenter`, and `TryClassCenter`. Every other page still uses the captured file. A new page whose elements change after loading must do the same, or its buttons will not answer. `ShopEquipHitTests` covers Wear and Buy on a stamped shop.

## Doc screenshots

`games/maze-puzzle/unity/screenshots/` holds the 13 pictures that doc 09 links, each 1290 x 2796 (the 430 x 932 page at 3x). `PagePreviewPainter.DocScreenshots` lists the spec behind each file. Repaint them with this project closed in the Editor and a graphics device (no `-nographics`):

```bash
Unity -batchmode -quit -projectPath games/maze-puzzle/unity/maze-puzzle \
  -executeMethod OrbitalSalvage.Viewer.Editor.PagePreviewPainter.DumpDocScreenshotsBatch
```

`-docShotDir <folder>` writes somewhere else. These are the stored page specs painted in the capture pose, so the game page shows the recorded playfield, not a live level. Restore `ProjectSettings/ProjectSettings.asset` with `git checkout` after a batch run if Unity changed it.
