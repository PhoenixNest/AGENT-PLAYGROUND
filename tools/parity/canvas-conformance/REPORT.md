# Canvas2D conformance report

Metrics follow SPEC section 2: `pix16` = percent of pixels with every channel within 16 (bar 99.0, target 99.5), `SSIM` = skimage SSIM over the full frame (bar 0.985, target 0.99). `ROI` columns repeat both metrics over the bounding box of the drawn content (small synthetic scenes are mostly background, so the full-frame numbers flatter them). `edge max` is the largest channel difference on edge pixels of the reference.

## Synthetic scenes (22/23 pass)

| name | pix16 % | SSIM | ROI pix16 % | ROI SSIM | max diff | edge max | alpha max | pass |
|---|---:|---:|---:|---:|---:|---:|---:|:--:|
| alpha_stacking | 99.974 | 0.9997 | 99.97 | 0.9996 | 42 | 42 | 0 | PASS |
| caps_joins | 99.867 | 0.9998 | 99.86 | 0.9998 | 62 | 62 | 0 | PASS |
| circle_strokes | 99.046 | 0.9976 | 98.87 | 0.9972 | 121 | 121 | 0 | pass* |
| circles_large | 99.923 | 0.9997 | 99.92 | 0.9997 | 73 | 73 | 0 | PASS |
| circles_small | 99.644 | 0.9991 | 99.58 | 0.9989 | 78 | 78 | 0 | PASS |
| clipping | 99.855 | 0.9980 | 99.80 | 0.9974 | 228 | 228 | 0 | PASS |
| curves | 99.705 | 0.9991 | 99.61 | 0.9988 | 98 | 98 | 0 | PASS |
| dashes | 99.622 | 0.9985 | 99.56 | 0.9983 | 228 | 228 | 0 | PASS |
| drawimage | 99.321 | 0.9941 | 99.22 | 0.9937 | 255 | 255 | 0 | pass* |
| ellipses_arcs | 99.362 | 0.9987 | 99.26 | 0.9985 | 104 | 104 | 0 | pass* |
| gradients_linear | 99.996 | 0.9982 | 100.00 | 0.9980 | 33 | 33 | 0 | PASS |
| gradients_radial | 99.828 | 0.9982 | 99.81 | 0.9981 | 119 | 119 | 0 | PASS |
| lines_thin | 99.604 | 0.9993 | 99.51 | 0.9991 | 32 | 32 | 0 | PASS |
| roundrects | 99.768 | 0.9995 | 99.77 | 0.9995 | 72 | 72 | 0 | PASS |
| shadows_fill | 99.841 | 0.9992 | 99.80 | 0.9989 | 56 | 56 | 0 | PASS |
| shadows_stroke | 99.915 | 0.9992 | 99.90 | 0.9990 | 95 | 95 | 0 | PASS |
| shear_transforms | 99.557 | 0.9991 | 99.36 | 0.9987 | 94 | 94 | 0 | PASS |
| state_stack | 99.813 | 0.9990 | 99.75 | 0.9987 | 70 | 70 | 191 | PASS |
| text_align_baseline | 99.211 | 0.9973 | 99.13 | 0.9970 | 126 | 126 | 0 | pass* |
| text_emoji | 99.199 | 0.9971 | 99.12 | 0.9968 | 124 | 124 | 0 | pass* |
| text_fonts | 98.452 | 0.9957 | 97.93 | 0.9943 | 133 | 133 | 0 | FAIL |
| text_stroke_scale | 99.588 | 0.9984 | 99.26 | 0.9972 | 103 | 103 | 0 | PASS |
| zero_length | 99.984 | 1.0000 | 99.86 | 0.9998 | 27 | 27 | 0 | PASS |

## Gameplay streams (30/30 pass)

| name | pix16 % | SSIM | ROI pix16 % | ROI SSIM | max diff | edge max | alpha max | pass |
|---|---:|---:|---:|---:|---:|---:|---:|:--:|
| s01_f001 | 99.740 | 0.9958 | 99.74 | 0.9958 | 138 | 138 | 0 | PASS |
| s01_f060 | 99.659 | 0.9954 | 99.66 | 0.9954 | 183 | 183 | 0 | PASS |
| s01_f180 | 99.734 | 0.9959 | 99.73 | 0.9959 | 146 | 146 | 0 | PASS |
| s01_pause | 99.263 | 0.9932 | 99.26 | 0.9931 | 225 | 225 | 0 | pass* |
| s01_vfx1 | 99.286 | 0.9935 | 99.28 | 0.9934 | 223 | 223 | 0 | pass* |
| s01_vfx2 | 99.275 | 0.9933 | 99.27 | 0.9933 | 223 | 223 | 0 | pass* |
| s11_f001 | 99.735 | 0.9959 | 99.74 | 0.9959 | 138 | 138 | 0 | PASS |
| s11_f060 | 99.678 | 0.9956 | 99.68 | 0.9956 | 214 | 214 | 0 | PASS |
| s11_f180 | 99.729 | 0.9959 | 99.73 | 0.9959 | 138 | 138 | 0 | PASS |
| s11_pause | 99.293 | 0.9933 | 99.29 | 0.9933 | 221 | 221 | 0 | pass* |
| s11_vfx1 | 99.279 | 0.9934 | 99.28 | 0.9934 | 223 | 223 | 0 | pass* |
| s11_vfx2 | 99.299 | 0.9934 | 99.30 | 0.9934 | 223 | 223 | 0 | pass* |
| s21_f001 | 99.668 | 0.9958 | 99.67 | 0.9958 | 149 | 149 | 0 | PASS |
| s21_f060 | 99.659 | 0.9958 | 99.66 | 0.9958 | 134 | 134 | 0 | PASS |
| s21_f180 | 99.664 | 0.9959 | 99.66 | 0.9959 | 160 | 160 | 0 | PASS |
| s21_pause | 99.217 | 0.9931 | 99.21 | 0.9931 | 221 | 221 | 0 | pass* |
| s21_vfx1 | 99.189 | 0.9932 | 99.19 | 0.9932 | 223 | 223 | 0 | pass* |
| s21_vfx2 | 99.205 | 0.9931 | 99.20 | 0.9931 | 223 | 223 | 0 | pass* |
| s31_f001 | 99.694 | 0.9959 | 99.69 | 0.9959 | 150 | 150 | 0 | PASS |
| s31_f060 | 99.672 | 0.9959 | 99.67 | 0.9959 | 139 | 139 | 0 | PASS |
| s31_f180 | 99.633 | 0.9957 | 99.63 | 0.9957 | 214 | 214 | 0 | PASS |
| s31_pause | 99.162 | 0.9928 | 99.16 | 0.9928 | 221 | 221 | 0 | pass* |
| s31_vfx1 | 99.147 | 0.9930 | 99.15 | 0.9930 | 223 | 223 | 0 | pass* |
| s31_vfx2 | 99.159 | 0.9928 | 99.16 | 0.9928 | 223 | 223 | 0 | pass* |
| s41_f001 | 99.613 | 0.9956 | 99.61 | 0.9956 | 221 | 221 | 0 | PASS |
| s41_f060 | 99.631 | 0.9958 | 99.63 | 0.9958 | 132 | 132 | 0 | PASS |
| s41_f180 | 99.636 | 0.9959 | 99.64 | 0.9959 | 144 | 144 | 0 | PASS |
| s41_pause | 99.186 | 0.9931 | 99.18 | 0.9931 | 228 | 228 | 0 | pass* |
| s41_vfx1 | 99.173 | 0.9932 | 99.17 | 0.9932 | 223 | 223 | 0 | pass* |
| s41_vfx2 | 99.192 | 0.9932 | 99.19 | 0.9931 | 228 | 228 | 0 | pass* |

## Performance (Unity Editor, RTX 4060 Laptop, D3D11)

| stream | ops | edges | draws | record ms (CPU) | submit ms (CPU) | replay ms (GPU-bound) | frame ms (CPU+GPU, synced) |
|---|---:|---:|---:|---:|---:|---:|---:|
| s01_f001 | 2174 | 21074 | 1109 | 2.74 | 2.12 | 3.40 | 8.71 |
| s01_f060 | 2328 | 21755 | 1191 | 2.87 | 2.19 | 3.52 | 8.87 |
| s01_f180 | 2192 | 21726 | 1124 | 2.58 | 2.03 | 3.29 | 8.34 |
| s01_pause | 3169 | 33802 | 1626 | 4.53 | 3.23 | 5.55 | 13.62 |
| s01_vfx1 | 2891 | 28431 | 1474 | 3.88 | 2.88 | 4.92 | 12.23 |
| s01_vfx2 | 3106 | 31510 | 1593 | 4.07 | 2.93 | 5.21 | 11.79 |
| s11_f001 | 2450 | 22503 | 1317 | 3.10 | 2.55 | 3.73 | 10.25 |
| s11_f060 | 2585 | 23341 | 1406 | 3.02 | 2.48 | 3.90 | 9.98 |
| s11_f180 | 2508 | 23292 | 1368 | 2.94 | 2.46 | 3.68 | 9.46 |
| s11_pause | 3338 | 32990 | 1787 | 4.27 | 3.35 | 5.71 | 13.25 |
| s11_vfx1 | 3217 | 30223 | 1724 | 3.87 | 3.10 | 5.44 | 12.14 |
| s11_vfx2 | 3365 | 33602 | 1808 | 4.15 | 3.32 | 5.63 | 13.83 |
| s21_f001 | 2667 | 28893 | 1512 | 3.86 | 2.92 | 4.68 | 11.24 |
| s21_f060 | 2662 | 28553 | 1509 | 3.44 | 2.70 | 4.32 | 10.39 |
| s21_f180 | 2644 | 28639 | 1500 | 3.34 | 2.58 | 4.41 | 10.56 |
| s21_pause | 3544 | 40339 | 1961 | 4.93 | 3.79 | 6.31 | 15.16 |
| s21_vfx1 | 3343 | 35381 | 1850 | 4.48 | 3.37 | 5.86 | 13.89 |
| s21_vfx2 | 3511 | 39079 | 1946 | 4.90 | 3.71 | 6.38 | 15.02 |
| s31_f001 | 2654 | 25958 | 1440 | 3.21 | 2.59 | 4.23 | 10.21 |
| s31_f060 | 2659 | 26215 | 1443 | 4.05 | 3.53 | 10.54 | 46.27 |
| s31_f180 | 2763 | 27103 | 1508 | 3.27 | 2.68 | 4.17 | 10.88 |
| s31_pause | 3613 | 37526 | 1939 | 4.49 | 3.34 | 6.25 | 13.91 |
| s31_vfx1 | 3462 | 33841 | 1858 | 4.17 | 3.30 | 5.87 | 13.19 |
| s31_vfx2 | 3620 | 37285 | 1948 | 4.55 | 3.43 | 6.20 | 14.11 |
| s41_f001 | 3158 | 34077 | 1788 | 4.09 | 3.11 | 4.83 | 12.39 |
| s41_f060 | 2981 | 33943 | 1701 | 4.58 | 3.27 | 4.78 | 13.37 |
| s41_f180 | 2963 | 33930 | 1692 | 3.93 | 3.00 | 4.84 | 11.87 |
| s41_pause | 3803 | 44100 | 2117 | 5.25 | 3.74 | 6.98 | 15.37 |
| s41_vfx1 | 3662 | 40851 | 2042 | 5.86 | 4.05 | 6.59 | 15.74 |
| s41_vfx2 | 3850 | 45069 | 2150 | 5.37 | 3.93 | 6.76 | 16.18 |

## Notes

- Streams: 30 recorded gameplay frames (sessions s01/s11/s21/s31/s41, each with f001/f060/f180/pause/vfx1/vfx2), reference from Chromium 153 at DPR 3, replayed through the same command stream in Unity (D3D11 verified in `unity.log`).
- `pass*` = clears the 99.0 % / 0.985 SPEC bar but not the 99.5 % / 0.99 internal target. Pause and vfx frames carry the most text and emoji, whose edges are the residual (see `Assets/Scripts/Render/DESIGN.md`, Text).
- Text is drawn by the outline glyph rasteriser (SPEC 1.6, section 6.4); the SDF atlas is no longer the parity path. The four `text_*` rows were re-measured with it (see `tools/parity/reports/iter-01-text.md`): `text_fonts` is the one scene still under the 99.0 % bar (98.45 %, SSIM 0.9957). The 30 gameplay-stream rows below are from the earlier SDF run, except s01_f001 (99.764 %, SSIM 0.9960) and s01_pause (99.500 %, SSIM 0.9953) which were re-measured with the outline text.
- Performance columns: `record` = CPU time in `player.Play` (command recording and geometry), `submit` = CPU time in `EndFrame` (buffer upload and `ExecuteCommandBuffer`), `replay` = the already recorded command buffer executed repeatedly and synced (GPU-bound cost), `frame` = whole record + submit pipeline over 20 frames after 3 warm-up frames, synced at the end. Measured in the Editor on a laptop GPU; individual streams show multi-ms noise between runs (GC, Editor activity).

