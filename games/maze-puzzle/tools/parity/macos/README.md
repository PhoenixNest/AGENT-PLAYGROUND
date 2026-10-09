# macOS parity wrappers (not written)

This folder is a placeholder. There are no `.sh` scripts here. Do not treat this file as a runner.

Windows launchers that need equivalents live in `../windows/`:

| Planned script | Windows file it would replace | Notes |
| --- | --- | --- |
| `unity/build.sh` | `windows/unity/build.ps1` | Unity player build |
| `unity/capture.sh` | `windows/unity/capture.ps1` | Still capture |
| `unity/perf.sh` | `windows/unity/perf.ps1` | Player perf matrix |
| `canvas-conformance/run-unity.sh` | `windows/canvas-conformance/run-unity.ps1` | Canvas conformance batch |

`windows/unity/run-framerate.ps1` and `windows/unity/build-play.ps1` are also Windows-only. Equivalents are not planned in this file beyond the four names above.

Editor binary, when a Mac is available:

`/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity`

The Windows scripts hardcode `C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe`.

Player target: `StandaloneOSX` or iOS. Graphics API: Metal. The Windows player is Win64 and the play build forces Direct3D11 (`PlayBuild.cs`).

Screenshot hashes in the docs were measured on Windows D3D12. Recapture baselines on the Mac before comparing pixels. Physics goldens under `../golden/fixtures/` are pure math and should carry over. Verify that on the first Mac run. Retire the `.ps1` files only after the `.sh` versions pass.
