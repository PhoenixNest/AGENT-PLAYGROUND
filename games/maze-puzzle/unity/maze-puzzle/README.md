# maze-puzzle Unity project

Unity 6000.4.6f1 project for Orbital Salvage. Open this folder in the Editor. The scene is `Assets/Scenes/MainGameScene.unity`.

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

## Page Preview

`Page Preview` paints `Assets/Resources/Parity/Spec/<id>__<profile>.json` with `HomeChrome` and does not enter Play mode. The dropdown lists every `__zero` spec, grouped by page. The iPhone profile toggle is shown only when that id also has an `__iphone` spec (today that is `home` only).

The default raster is the spec scale, 3×, so the texture is 1290×2796 for the 430×932 page. `Fast 1x raster` calls `HomeChrome.ResizeRaster(1)` and is not the still. Fit and zoom only change how the texture is drawn in the window.

Animation ms at 0 calls `HomeChrome.Draw()` (capture pose, no clock). That is the pose the parity player uses when the state has no `timeOffsetMs`. A value above 0 calls `HomeChrome.Draw(timeMs)`. The CSS clock is deterministic. Save PNG writes `games/maze-puzzle/output/preview/<id>__<profile>.png`.

Batch entry points, from this project:

```powershell
-executeMethod OrbitalSalvage.Editor.PlayBuild.BuildPlayer
-executeMethod OrbitalSalvage.Editor.IosExport.ExportSimulator
-executeMethod OrbitalSalvage.Editor.IosExport.ExportDevice
```

The Windows wrapper is `games/maze-puzzle/tools/parity/windows/unity/build-play.ps1`.
