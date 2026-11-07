# Changelog — superseded pre-recovery claims

On 2026-11-07 the specs in this folder were rewritten from the recovered build (Multi Maze 3D v4.1.0.0, see `tools/recovery/`).
The earlier specs were written from screenshots before the build was recovered. The values below came from those specs and are **not valid**; they are kept only so old notes and conversations can be traced. The previous text exists in git commit `3774e2e` (reachable only through the reflog).

| Topic | Pre-recovery claim | Recovered fact | Now in |
|---|---|---|---|
| Engine / rendering | Unity 6 (6000.4), URP Forward+, ACES, Bloom, SSAO | Unity 2022.3.62f2, built-in pipeline (Forward), Gamma, no post-processing | 04 |
| Physics step | 60 Hz, solver 8 / 4, gravity −18.5 | 30 Hz (0.033 s), solver 2 / 1, gravity −20 | 04 |
| Ball physics | mass 0.15, bounciness 0.62, drag 0.02, interpolation on | mass 0.1, bounciness 0, drag 0, no interpolation | 04 |
| Ball pool | `BallPoolManager.Instance`, `Queue<>`, 2,000 pre-warmed | static `BallsPool` over `ObjectPool<BallView>`, 200 pre-warmed, value tiers | 01, 02, 05 |
| Gate re-trigger | 0.35 s timed cooldown | per-ball ignored-collider list, no timer | 02 |
| Gate operations | ×, +, −, ÷ | ×, −, ÷ only (`GateType` has no Add) | 02 |
| Win / fail | 2.0 s settle timer, 3 s defeat timeout, cup coordinate | not serialized; logic unknown | 02 |
| Levels | 50 levels, tier table, JSON schema | 107-row CSV (remote `LevelFunnel` override), live-order rule, lock costs per prefab | 03 |
| Max target | 1,500 | 1,600 | 03 |
| Rotation tuning | γ = 0.08 damping, custom formula | `RotateController` Multiplier 3.5, Damping 16 | 02 |
| Camera | FOV 42 at (0, −0.3, −9.6) | FOV 50 at (0, −0.4, −16.93), 3° pitch | 04 |
| Performance budgets | < 40 draw calls, < 45k triangles | removed; unknown until measured | 04 |
| Skins | 9-slot grid, 9 named skins | 3 shop variants with 6 / 7 / 7 skins, price 500 | 06 |
| Assets | 28–32 modular `SM_*` meshes, 240-tri ball, LOD0/1, 2048² per-skin atlas | 2,199 recovered meshes, 28-tri ball, no LODs, per-skin material families | 07 |
| Ball size | 0.25 m diameter, collider r 0.125 | collider r 0.5466 at prefab scale 0.2; runtime scale 0.17–0.22 | 04, 07 |
| Implementation plan | doc 08: six phases on Unity 6 URP | deleted; a new plan follows the engine decision (README §5) | README |