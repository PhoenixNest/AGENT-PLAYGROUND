# macOS Unity wrappers (not written)

No `.sh` files in this folder.

Planned, after they are run on a Mac:

- `build.sh` — equivalent of `../../windows/unity/build.ps1`
- `capture.sh` — equivalent of `../../windows/unity/capture.ps1`
- `perf.sh` — equivalent of `../../windows/unity/perf.ps1`

Unity binary: `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity`

Build target: `StandaloneOSX` or iOS. Graphics: Metal, not D3D11/D3D12.

`screenshot-states.json` and `smoke-states.json` stay in `../../unity/`. Screenshot hashes were measured on Windows D3D12 and must be recaptured on Mac. Physics goldens are pure math and should carry over; verify on the first Mac run.
