# Windows parity wrappers

PowerShell launchers for this machine. They stay tracked. macOS placeholders are in `../macos/` and contain no scripts.

| Script | Role |
| --- | --- |
| `unity/build.ps1` | Win64 parity player (`ParityBuild`, `OrbitalSalvageParity.exe`) |
| `unity/capture.ps1` | Still capture from that player |
| `unity/perf.ps1` | D3D11/D3D12 player perf |
| `unity/run-framerate.ps1` | EditMode `FrameRateHarness` via `Unity.exe` |
| `unity/build-play.ps1` | Win64 play player, Direct3D11 (`PlayBuild`), exe at `games/maze-puzzle/output/maze-puzzle-windows/OrbitalSalvage.exe` |
| `canvas-conformance/run-unity.ps1` | `CanvasConformance.Run` |

State JSON stays in `../unity/`. Path walking is `../lib/roots.ps1`.
