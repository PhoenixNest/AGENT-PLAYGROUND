# Physics frame-time sweep 2026-10-07

Live path: `LiveGameSession.Update` with `FixedStep` false, `RenderEnabled` false, simulated time 30 s. Session caps each call at `DtCap` 0.05 s. Boot matches `RecordedFrameDriver.Create` (renderer RNG, layout 430x932, discarded sector 1, `ConsumeBootRandom`, then the sector) except `FixedStep` is cleared so the passed dt is the step.

Inputs: physics golden programs `idle`, `drag`, `bursts` (`PhysicsRunner.Program`). A program frame f is an event at t = (f-1) * 0.016 s, applied on the first harness frame whose simulated time is at least that t, before `Update`. Drag values are pointer deltas in CSS pixels (`PointerMove` multiplies by 1.3, matching `ApplyInputDrag(delta * 1.3)`). RCS events use `KeyDown`.

Random profile seed 20261007 (LCG 1664525/1013904223, 24-bit unit interval, dt uniform on [14, 30) ms). Spike profile: 16 ms, and 100 ms when the 1-based frame index is a multiple of 25; the session cap shortens a 100 ms request to 50 ms. Editor traces cycle `frameMs` from tag `archive/parity-homeperf`.

## Boot hashes

```
boot
1 f82328cabb5db1ef f82328cabb5db1ef match
11 cf17162b82c840bc cf17162b82c840bc match
41 13f0dfc3a9d0cf91 13f0dfc3a9d0cf91 match
```

## Layout after Unity ApplyLayout

| when | center.x | center.y | blackHole.x | blackHole.y | eventHorizonRadius | gravityRadius | gravityStrength |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| after ApplyLayout(430, 932), before LoadSector | 215 | 354.16 | 215 | 708.32 | 28 | 240 | 480000 |
| after LoadSector campaign 1 | 215 | 354.16 | 215 | 708.32 | 28 | 240 | 480000 |

## Profile vs steady-16ms

Difference is profile minus `steady-16ms`, across every sector and stream. Swallowed is the `OnSwallow` count (event-horizon entry). Spilled is `SpilledMass`.

| profile | runs | mean spilled | mean swallowed | spilled delta mean | spilled delta min | spilled delta max | swallowed delta mean | swallowed delta min | swallowed delta max | runs spilled higher | runs swallowed lower |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| steady-16ms | 33 | 1599.6969696969697 | 220.21212121212122 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| steady-1-60 | 33 | 1701.4545454545455 | 219.33333333333334 | 101.75757575757575 | -2918 | 4364 | -0.87878787878787878 | -854 | 934 | 12 | 15 |
| steady-33.3ms | 33 | 2193.3939393939395 | 267.93939393939394 | 593.69696969696975 | -6612 | 7018 | 47.727272727272727 | -981 | 1017 | 23 | 14 |
| random-14-30 | 33 | 2412.848484848485 | 313.21212121212119 | 813.15151515151513 | -670 | 5820 | 93 | -351 | 1083 | 24 | 11 |
| spike-100-every-25 | 33 | 2206.121212121212 | 358.93939393939394 | 606.42424242424238 | -596 | 4558 | 138.72727272727272 | -68 | 1108 | 21 | 10 |
| editor-s1-baseline-1 | 33 | 2316.969696969697 | 342 | 717.27272727272725 | -776 | 5284 | 121.78787878787878 | -187 | 1054 | 18 | 11 |
| editor-s3-baseline-1 | 33 | 2769.212121212121 | 305.87878787878788 | 1169.5151515151515 | -20 | 6780 | 85.666666666666671 | -375 | 1055 | 29 | 11 |
| editor-auto-timeline-1 | 33 | 2555.151515151515 | 336.06060606060606 | 955.4545454545455 | -94 | 6206 | 115.84848484848484 | -208 | 1113 | 25 | 9 |

## Same differences by stream

| stream | profile | runs | spilled delta mean | spilled delta min | spilled delta max | swallowed delta mean | swallowed delta min | swallowed delta max | runs spilled higher | runs swallowed lower |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| idle | steady-16ms | 11 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| idle | steady-1-60 | 11 | 5.4545454545454541 | -10 | 32 | -0.72727272727272729 | -5 | 5 | 4 | 4 |
| idle | steady-33.3ms | 11 | 19.636363636363637 | -4 | 66 | -5 | -33 | 0 | 5 | 4 |
| idle | random-14-30 | 11 | 12.181818181818182 | 0 | 34 | -2 | -17 | 2 | 7 | 4 |
| idle | spike-100-every-25 | 11 | 4.5454545454545459 | -4 | 32 | -0.18181818181818182 | -3 | 3 | 4 | 3 |
| idle | editor-s1-baseline-1 | 11 | 1.4545454545454546 | -12 | 30 | -0.72727272727272729 | -4 | 2 | 2 | 4 |
| idle | editor-s3-baseline-1 | 11 | 27.272727272727273 | 0 | 74 | -5 | -29 | 0 | 8 | 4 |
| idle | editor-auto-timeline-1 | 11 | 24.363636363636363 | 0 | 62 | -2.4545454545454546 | -20 | 1 | 8 | 4 |
| drag | steady-16ms | 11 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| drag | steady-1-60 | 11 | 310.54545454545456 | -2918 | 4364 | -1.2727272727272727 | -854 | 934 | 6 | 6 |
| drag | steady-33.3ms | 11 | 1696.3636363636363 | -6612 | 7018 | 145.81818181818181 | -981 | 1017 | 8 | 5 |
| drag | random-14-30 | 11 | 2394.909090909091 | -670 | 5820 | 279.18181818181819 | -351 | 1083 | 9 | 4 |
| drag | spike-100-every-25 | 11 | 1793.2727272727273 | -596 | 4558 | 415.27272727272725 | -68 | 1108 | 8 | 4 |
| drag | editor-s1-baseline-1 | 11 | 2131.4545454545455 | -776 | 5284 | 365.18181818181819 | -187 | 1054 | 9 | 3 |
| drag | editor-s3-baseline-1 | 11 | 3418.3636363636365 | 136 | 6780 | 259.90909090909093 | -375 | 1055 | 11 | 5 |
| drag | editor-auto-timeline-1 | 11 | 2847.2727272727275 | 352 | 6206 | 347.36363636363637 | -208 | 1113 | 11 | 4 |
| bursts | steady-16ms | 11 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| bursts | steady-1-60 | 11 | -10.727272727272727 | -58 | 10 | -0.63636363636363635 | -5 | 2 | 2 | 5 |
| bursts | steady-33.3ms | 11 | 65.0909090909091 | -2 | 224 | 2.3636363636363638 | -4 | 23 | 10 | 5 |
| bursts | random-14-30 | 11 | 32.363636363636367 | -18 | 114 | 1.8181818181818181 | -5 | 11 | 8 | 3 |
| bursts | spike-100-every-25 | 11 | 21.454545454545453 | -24 | 72 | 1.0909090909090908 | -4 | 7 | 9 | 3 |
| bursts | editor-s1-baseline-1 | 11 | 18.90909090909091 | -32 | 96 | 0.90909090909090906 | -2 | 6 | 7 | 4 |
| bursts | editor-s3-baseline-1 | 11 | 62.909090909090907 | -20 | 106 | 2.0909090909090908 | -4 | 9 | 10 | 2 |
| bursts | editor-auto-timeline-1 | 11 | -5.2727272727272725 | -94 | 48 | 2.6363636363636362 | -1 | 7 | 6 | 1 |

## Means by profile

| profile | mean collected | mean spilled | mean swallowed | mean in play | mean still ingesting | mean still falling | defeats | victories |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| steady-16ms | 1991.090909090909 | 1599.6969696969697 | 220.21212121212122 | 95.030303030303031 | 0.75757575757575757 | 24.545454545454547 | 3 | 9 |
| steady-1-60 | 2011.3939393939395 | 1701.4545454545455 | 219.33333333333334 | 93.727272727272734 | 0.54545454545454541 | 24.848484848484848 | 4 | 8 |
| steady-33.3ms | 2058.787878787879 | 2193.3939393939395 | 267.93939393939394 | 113.63636363636364 | 2.2424242424242422 | 22 | 0 | 11 |
| random-14-30 | 2406.060606060606 | 2412.848484848485 | 313.21212121212119 | 131.21212121212122 | 3.6969696969696968 | 36.060606060606062 | 0 | 12 |
| spike-100-every-25 | 2606.242424242424 | 2206.121212121212 | 358.93939393939394 | 136.24242424242425 | 1.3333333333333333 | 43.393939393939391 | 0 | 12 |
| editor-s1-baseline-1 | 2601.4545454545455 | 2316.969696969697 | 342 | 133.60606060606059 | 0.66666666666666663 | 39.878787878787875 | 0 | 12 |
| editor-s3-baseline-1 | 2154.909090909091 | 2769.212121212121 | 305.87878787878788 | 132.33333333333334 | 0.78787878787878785 | 40.636363636363633 | 0 | 11 |
| editor-auto-timeline-1 | 2429.4545454545455 | 2555.151515151515 | 336.06060606060606 | 133.21212121212122 | 9.545454545454545 | 26.878787878787879 | 0 | 12 |

Per-run rows were in `physics-framerate-2026-10-07-runs.csv`. Per-debris rows were in `physics-framerate-2026-10-07-debris.csv`. Those dumps, the frame traces, and `physics-framerate-2026-10-07-accum.txt` were removed from git on 2026-10-08. `FrameRateHarness` still writes them, and `.gitignore` ignores the regenerated files. The tables above are the kept conclusions.

## Web layout at 430x932

`node tools/parity/scripts/measure-web-layout-430.mjs` executed `main.js` `initLayout` against a `PhysicsEngine` from `engine/physics.js` with `frame` 430x932. Printed JSON:

| field | web | Unity ApplyLayout |
| --- | ---: | ---: |
| scale | 1 | not stored by ApplyLayout |
| center.x | 215 | 215 |
| center.y | 354.16 | 354.16 |
| blackHole.x | 215 | 215 |
| blackHole.y | 708.32 | 708.32 |
| eventHorizonRadius | 28 | 28 |
| gravityRadius | 240 | 240 |
| gravityStrength | 480000 | 480000 |

The six physics fields match the Unity table above. `initLayout` computes `scale` and does not assign it onto the black hole.

## Editor traces used

Tag `archive/parity-homeperf` has no `tools/parity/out/editor-probe-after/` (`git ls-tree` and `git rev-list --all --objects` found no `editor-probe-after` frames). The three cycled traces are the committed before-fix captures. `frameMs` summary from those CSVs:

| trace | n | min | median | mean | p95 | max | frames > 100 ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| home-s1-baseline-1 | 300 | 7.474 | 15.607 | 16.843 | 26.390 | 32.885 | 0 |
| home-s3-baseline-1 | 300 | 33.304 | 41.226 | 57.098 | 167.020 | 391.604 | 17 |
| home-auto-timeline-1 | 268 | 5.907 | 17.475 | 44.811 | 136.810 | 1660.255 | 18 |

## Idle and bursts, absolute means

The pooled table mixes streams. Drag runs shred gates and the swallowed count is hundreds of spawned pieces. Idle has no scripted input.

| stream | profile | mean spilled | mean swallowed | mean collected |
| --- | --- | ---: | ---: | ---: |
| idle | steady-16ms | 20.18 | 5.45 | 10.91 |
| idle | steady-1-60 | 25.64 | 4.73 | 9.45 |
| idle | steady-33.3ms | 39.82 | 0.45 | 0.91 |
| idle | editor-s1-baseline-1 | 21.64 | 4.73 | 9.45 |
| idle | editor-s3-baseline-1 | 47.45 | 0.45 | 0.91 |
| idle | editor-auto-timeline-1 | 44.55 | 3.00 | 6.00 |
| bursts | steady-16ms | 53.09 | 2.73 | 5.64 |
| bursts | steady-1-60 | 42.36 | 2.09 | 4.18 |
| bursts | steady-33.3ms | 118.18 | 5.09 | 12.36 |
| bursts | editor-s3-baseline-1 | 116.00 | 4.82 | 10.91 |

Idle sectors that stay at 0 swallowed and 0 spilled on every profile in this 30 s window: L11, L21, L41. Sectors that do feed the hole:

| sector | 16 ms sw/sp | 1/60 sw/sp | 33.3 ms sw/sp | editor s1 sw/sp | editor s3 sw/sp | 16 ms victory s | 33.3 ms victory s | s3 victory s |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| L06 | 7/76 | 4/88 | 0/112 | 6/64 | 0/112 | none | none | none |
| L31 | 16/0 | 14/0 | 3/46 | 14/0 | 0/74 | none | none | none |
| daily bronze 2026-10-01 seed 966561 | 34/52 | 29/62 | 1/118 | 30/60 | 5/112 | 9.664 | none | none |

Versus steady 1/60, idle `editor-s3-baseline-1` spill delta mean +21.82 (min 0, max 74, higher on 8/11), swallowed delta mean -4.27 (min -24, max 0, lower on 4/11). The seven idle runs that do not swallow less are already 0 at both rates.

## Step A reading

Sustained slow steps change outcomes on the sectors that feed the hole: L31 idle goes from 16 swallowed and 0 spilled at 16 ms to 0 swallowed and 74 spilled on the scale-3 editor trace; the daily bronze idle victory at 9.664 s disappears at 33.3 ms and on that trace. The scale-1 editor trace (p95 26.4 ms, max 32.9 ms) stays near the 16 ms and 1/60 rows. Drag deltas swing by thousands of kg and hundreds of swallows in both directions, so that stream is not a clean spill-up/swallow-down result.

## Accumulator rerun

`FrameRateHarness.MeasureAccumulated` sets `Accumulate` and integrates at 1/60 s, up to 3 steps per frame. Scripted inputs fire from `BeforeAccumStep` at the physics time of each step. Every profile runs 1800 steps (simulated time 29.999999999999577 s). Unity exit 0. Result file `physics-framerate-2026-10-07-accum.txt` (removed from git on 2026-10-08; the harness regenerates it): 264 rows, 0 mismatches against `steady-1-60` on hash, spilled mass, swallowed count, collected mass, and step count.

Daily bronze idle, which disagreed across profiles in the variable-step run, is one row for every profile: collected 58, spilled 62, swallowed 29, hash `27a58fb1c6d0389a`. That swallowed/spilled pair is the variable-step `steady-1-60` idle result from the first sweep.

## Verification after the live-path change

EditMode suite, GPU, no `-testFilter`, Unity exit 2 (failures present). XML `test-run`: total 385, passed 301, failed 82, skipped 2, duration 68.3 s. The two skipped cases are the explicit harness methods `FrameRateHarness.Measure` and `FrameRateHarness.MeasureAccumulated`. All 82 failures are `ReferenceComparisonTests` (73) and `ReferenceGateTests` (9); the message is `tools/parity/audio/out/ref is missing`. `AppFlowTests.Flow_MatchesWeb` passed 9/9. `PhysicsGoldenTests.FullRun_BitExact` passed 35/35. `LiveGameSessionTests.AccumulatorCapsAtThreeStepsAndDropsTimeWhilePaused` passed.

`LiveConformanceBatch.Run` (Unity exit 0) wrote 30 live PNGs and 30 stream PNGs. Byte compare: 30 pairs, 0 mismatches. `export.log` ends with `deterministic true`. Step counts in that log are 1, 60, 180, 181, 190, 190 for f001, f060, f180, vfx1, vfx2, pause on each of sectors 01, 11, 21, 31, 41.
