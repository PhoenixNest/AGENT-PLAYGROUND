# Funnel fix 2026-10-08

Branch `agent/fix/funnel` at worktree `.worktrees/fix-funnel`. Spill rates are from `node tools/parity/scripts/funnel-study.mjs` on Node v24.15.0, levels 1, 2, 3, 5, 11, 21, 31, 41, 6 seeds, 40 s, dt = 1/60 s. Layout printed by the script: center (215, 354.16), black hole y 708.32. A style's spill % is spilled / (swallowed + spilled). Levels that release nothing are omitted from that percentage.

Defeat in `web/main.js` is an empty playfield (no `IN_RING`, `FALLING`, or `INGESTING` debris) while `collectedMass` is under the sector quota, held for 1.2 s. `onDebrisSpilled` adds `weight * 2` to `spilledMass`. That counter is shown on the defeat modal. It is not itself the fail condition. Spilling is still a way to empty the level before the quota.

## Before

Unmodified engine, `d.vx += dx * 16 * dt`.

| style | swallowed | spilled | spill % | left of hole | right of hole | max FALLING s | still FALLING at 40 s | mean fall s | swallows / s |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| idle | 100 | 373 | 78.9 | 373 | 0 | 1.117 | 0 | 0.981 | 0.0521 |
| swipe | 3470 | 3375 | 49.3 | 1609 | 1766 | 1.767 | 36 | 1.006 | 1.8073 |
| spinleft | 774 | 2112 | 73.2 | 496 | 1616 | 1.783 | 370 | 1.119 | 0.4031 |
| spinright | 388 | 1206 | 75.7 | 823 | 383 | 1.683 | 157 | 1.159 | 0.2021 |

Idle by level (spill % / released): L1 100/24, L2 97.5/79, L3 95.4/87, L5 94.5/200, L11 none, L21 none, L31 0/83, L41 none. Every idle spill was left of the hole. Spill offsets on L1 were mean -38.1, p10 -39.5, p90 -37.1. All 373 idle spills were below the hole (`y` past `blackHole.y + 140`), none off the side edges.

## Variants

Same protocol. Geometry stays at bottom half-width 46 and rail end `blackHole.y + 25` except the two geometry rows. `c=crit` is `2 * sqrt(k)` inside `d.vx += (dx * k - d.vx * c) * dt`. `post` multiplies `vx` after the spring and before the rails.

| variant | idle | swipe | spinleft | spinright | max FALLING s | still FALLING | idle swallows / s | idle mean fall s |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| web k=16 | 78.9 | 49.3 | 73.2 | 75.7 | 1.783 | 563 | 0.0521 | 0.981 |
| rails +35 px | 78.9 | 48.9 | 73.2 | 75.7 | 1.783 | 570 | 0.0521 | 0.981 |
| narrow half-width 30 | 0.0 | 0.0 | 0.0 | 0.0 | 1.783 | 403 | 0.2464 | 0.715 |
| k=24 | 80.3 | 21.3 | 21.9 | 19.9 | 2.200 | 469 | 0.0484 | 1.000 |
| k=28 | 12.3 | 10.0 | 8.0 | 5.6 | 2.233 | 422 | 0.2161 | 0.778 |
| k=32 | 0.0 | 6.4 | 1.7 | 1.3 | 2.267 | 393 | 0.2464 | 0.717 |
| k=36 | 0.0 | 3.2 | 0.4 | 0.4 | 1.983 | 388 | 0.2464 | 0.702 |
| k=40 | 0.0 | 4.5 | 1.6 | 1.1 | 1.983 | 385 | 0.2464 | 0.691 |
| k=16 c=8 | 0.0 | 3.8 | 6.4 | 8.1 | 2.100 | 377 | 0.2464 | 0.679 |
| k=16 post=0.875 | 0.0 | 6.0 | 13.1 | 19.8 | 2.100 | 379 | 0.2464 | 0.681 |
| k=24 c=crit | 0.0 | 0.3 | 0.1 | 0.0 | 2.117 | 396 | 0.2464 | 0.675 |
| k=32 c=crit | 0.0 | 0.1 | 0.0 | 0.0 | 1.800 | 384 | 0.2464 | 0.674 |
| k=40 c=6 | 0.0 | 0.0 | 0.0 | 0.0 | 1.783 | 383 | 0.2464 | 0.673 |
| k=40 c=crit | 0.0 | 0.0 | 0.0 | 0.0 | 1.783 | 376 | 0.2464 | 0.673 |

k=24 idle by level: L1 100/24, L2 98.7/79, L3 94.3/87, L5 85.0/200, L31 31.3/83. L31 idle was 0 spills before that change. k=28 idle: L1 29.2, L2 15.2, L3 14.9, L5 4.5, L31 20.5. Rows with idle 0.0 had 0 spills on L1, L2, L3, L5, and L31 (released 24, 79, 87, 200, 83) and no release on L11, L21, L41.

## Choice

Shipped line, in both engines, after the existing per-step `vx *= 0.994` and before the rails:

```text
d.vx += (dx * 16 - d.vx * 8) * dt
```

c = 8 is `2 * sqrt(16)`. Idle spill is 0 on every level that released debris. Swipe, spinleft, and spinright stay inside (0, 10]: 3.8, 6.4, 8.1. That is the largest spin spill among the rows that meet the idle and 10% bounds. k=32 and k=40 also meet the bounds, with spin spill 1.7/1.3 and 1.6/1.1. k=40 with any of the damping terms tried, k=32 with critical damping, k=24 with critical damping (spinright 0), and the narrow rail all remove at least one play style's spills. k=16 with `vx *= 0.875` after the spring leaves spinleft at 13.1 and spinright at 19.8. Extending the rails 35 px did not change idle spill.

After the change, the same study (`--variant web --assert 1`, exit 0) printed:

| style | swallowed | spilled | spill % | max FALLING s | still FALLING | mean fall s | swallows / s |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| idle | 473 | 0 | 0.0 | 0.733 | 0 | 0.679 | 0.2464 |
| swipe | 6511 | 254 | 3.8 | 1.733 | 21 | 0.815 | 3.3911 |
| spinleft | 2814 | 192 | 6.4 | 2.100 | 253 | 0.860 | 1.4656 |
| spinright | 1517 | 134 | 8.1 | 1.683 | 103 | 0.893 | 0.7901 |

Those four total rows are the same numbers as the k=16 c=8 row above. Idle released count stays 473 (the old 100 swallows + 373 spills). Idle swallows per second go from 0.0521 to 0.2464 because those spills are now swallows, not because the fall got shorter than the hole. Mean completed-fall time goes from 0.981 s to 0.679 s. L31 idle stays 83 swallowed and 0 spilled; its mean fall goes from 0.616 s to 0.604 s. The longest FALLING time in the whole matrix for this fix is 2.100 s (baseline max 1.783 s). Nothing was still falling from the start of the 40 s window.

`FunnelIdleTests` runs level 5 and level 31 for 1800 steps at 1/60 s with `JsRandom` seed `0x5EED1234`, layout 430×932, no input. Both passed: swallowed > 0 and spilled = 0.

## Regeneration

Chromium used for goldens and the harness-hash script: 153.0.8010.12 (Playwright chromium-1243). `node tools/parity/golden/generate.mjs --only physics` wrote 72 files, 8.88 MiB, exit 0. `fixtures/engine.txt` remains `chromium 153.0.8010.12` (git hash-object equal to HEAD). Maze and economy fixtures were not regenerated.

23 physics runs changed (46 gzip files). Unchanged: all 7 `idle-silent` runs, `L21-bursts-audio`, `L21-bursts-silent`, `L31-bursts-audio`, `L31-bursts-silent`, `L38-bursts-silent`. `manifest.json` unchanged.

Re-ran and found byte-identical to HEAD (git hash-object, or no diff after the run):

- `node tools/parity/web/record-playfield.mjs` (exit 0). SHA256: `game_lvl01_t1000ms.json` `F431C9B0AF0AEE016BFAB28F5F82BF861581BEFAD22E12B839FBCD9B45D8A69C`, `modal_pause.json` `154DC283AA37DDEB9F45B8149E8747CB2A64A7BAAD8671FE83856F36DE146709`, `modal_debrief_exact.json` `C8DBB010CBCA8853E27A8EE603EC7A2D7A45DE79AE9DC235E1C11C8CFC6040FE`, `modal_defeat_12_of_20.json` `A9F68E73295F570D070811B5B961E420E4AE585ECCABDF5EA016A4FE556DD69F`.
- `node tools/parity/canvas-conformance/record.mjs` (exit 0, 30 streams).
- `node tools/parity/web/app-flows.mjs` (exit 0, 9 flow files).

Profile-zero page PNGs were not recaptured. This worktree has no `Builds/Parity/OrbitalSalvageParity.exe`. The game and pause stills paint `game_lvl01_t1000ms.json` and `modal_pause.json`. Those files' SHA256 did not change, so the debris draw commands in them did not change. No before/after pixel scores were computed. The previously recorded still hashes were not re-measured.

## Unity

EditMode, GPU, no `-nographics`, no `-testFilter`. Unity pid 41624, exit 2. XML: total 394, passed 310, failed 82, skipped 2. The two skips are `FrameRateHarness.Measure` and `FrameRateHarness.MeasureAccumulated`. The 82 failures are `ReferenceComparisonTests` 73 and `ReferenceGateTests` 9, message `tools/parity/audio/out/ref is missing`. `Flow_MatchesWeb` 9/9 passed. `FullRun_BitExact` 35/35 passed against the regenerated Chromium fixtures. `IdleThirtySeconds_SpillsNothing` passed for levels 5 and 31. Baseline before these two tests was 392, 308, 82, 2.

`LiveConformanceBatch.Run` (Unity pid 31880, exit 0) wrote 30 live PNGs and 30 stream PNGs. Byte compare: 30 pairs, 0 mismatches. `export.log` ends with `deterministic true`. Step counts: 1, 60, 180, 181, 190, 190 for f001, f060, f180, vfx1, vfx2, pause on sectors 01, 11, 21, 31, 41.

`FrameRateHarness.MeasureAccumulated` (Unity pid 38032, exit 0): 264 rows, 0 mismatches against `steady-1-60` on hash, spilled mass, swallowed count, collected mass, and step count. The run overwrote `physics-framerate-2026-10-07-accum.csv` and `.txt`; those two files were restored from copies taken before the run, so the 2026-10-07 report's files are unchanged. The new `steady-1-60` hashes below were read from the overwritten csv before the restore.

A Chromium page then ran the same boot the harness uses (starfield and singularity `Math.random`, discarded sector 1, loading-bar random, target sector, 1800 steps of 1/60 s, drag via `applyInputDrag(40 * 1.3)` plus one opening `applyRcsBurst(-1)`, nom-word and confetti random on swallow / gate / explosion) and hashed with the FNV-1a 64 of `driver.capture`. Those hashes matched the Unity `steady-1-60` rows:

| sector | stream | hash | swallowed |
| --- | --- | --- | ---: |
| L01 | idle | `db9d9e0e2b59e809` | 0 |
| L01 | drag | `b83408cdb1e7c2c7` | 21 |
| L11 | idle | `64d44961709a2c68` | 0 |
| L11 | drag | `9a395d3b3d41adf1` | 22 |
| L21 | idle | `e8856bc06b0859f6` | 0 |
| L21 | drag | `946e7e2701fd7667` | 1482 |

`FullRun_BitExact` is the other bit-exact check: Chromium fixtures at dt = 16/1000 s, 600 frames, levels 1, 11, 21, 31, 38, 41, 50, programs idle, drag, and bursts, audio and silent. All 35 passed.

No `ProjectSettings` or TMP font assets were left modified. Library was not staged.
