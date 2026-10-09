# macOS canvas-conformance wrapper (not written)

No `.sh` file in this folder.

Planned, after it is run on a Mac: `run-unity.sh`, equivalent of `../../windows/canvas-conformance/run-unity.ps1`.

Unity binary: `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity`

Scenes and streams stay in `../../canvas-conformance/`. The Windows comment on `CanvasConformance.Run` names a D3D11 device. On Mac the device is Metal, and screenshot baselines measured on Windows D3D12 must be recaptured. Physics goldens are pure math and should carry over; verify on the first Mac run.
