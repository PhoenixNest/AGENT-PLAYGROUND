# iter-01: outline glyph text

Decision: SPEC 6.4 (v1.6). Text is drawn from TTF outlines through the exact-area Canvas2D path filler. SDF is no longer the parity path.

Metric: full-frame pix16 % (every channel within 16, bar 99.0) and SSIM (bar 0.985), from `canvas-conformance/compare.py`, scenes rendered with a real D3D11 device.

## Result

| scene | before pix16 / SSIM | after pix16 / SSIM | bar |
|---|---:|---:|:--:|
| text_align_baseline | 97.987 / 0.9876 | 99.211 / 0.9973 | pass |
| text_emoji | 98.605 / 0.9916 | 99.199 / 0.9971 | pass |
| text_stroke_scale | 99.244 / 0.9959 | 99.588 / 0.9984 | pass |
| text_fonts | 95.952 / 0.9746 | 98.452 / 0.9957 | pix16 short by 0.55 pp |

Gameplay streams re-measured with the outline text (the rest were not re-run, the old numbers are in `REPORT.md`): s01_f001 99.764 / 0.9960 (was 99.740 / 0.9958), s01_pause 99.500 / 0.9953 (was 99.263 / 0.9932).

## Iterations (text_fonts pix16 %)

| step | what changed | text_fonts |
|---|---|---:|
| 0 | SDF baseline | 95.95 |
| 1 | raw outline fill, plain exact-area coverage, naive layout | not recorded (geometry good, SSIM much better, glyphs too light) |
| 2 | Skia glyph-mask preblend (sRGB, contrast 0.5) in `Fill.shader` | not recorded |
| 3 | Blink layout: no kerning across spaces, GPOS pair kerning, GSUB ccmp + liga in lookup order; MeasureText matches `measureText` to 0.0003 css px | not recorded |
| 4 | glyph origin snap x 1/4 px, y integer; rounded hhea baseline metrics | 98.45 |
| 5 | sweeps: GlyphContrast 0/0.5/1, GlyphDx 0.04/0.08, GlyphRound 0.5/0.75, GlyphSubpixelY 1/2/4/64 | 98.45 to 98.53 (best +0.08) |

Step 5 improved by less than 0.3 pp in every sweep, so iteration stopped (rule: three non-improving iterations). Defaults stay at the sweep optimum (`GlyphSubpixelY=1`, `GlyphRound=0.5`, `GlyphDx=0`, `GlyphContrast=0.5`).

## What was built

- `TrueTypeFont.cs`: glyf/loca (simple and compound), cmap 4/12, hmtx, head, hhea, GPOS kern (PairPos 1/2, extension), GSUB types 2 and 4. Kerning source: **GPOS** (the fonts have no `kern` table).
- `OutlineFonts.cs`: face resolution, symbol fallback chain, flattened glyph cache.
- `Canvas2D.OutlineText.cs`: layout, alignment, baseline, origin snapping, fill and stroke, emoji runs unchanged.
- `Fill.shader`: glyph-mask preblend. `Canvas2D.Gpu.cs`: flag on `RenderShape`.
- `ParityAssetBuilder.SyncFontFiles`: copies the TTFs to `Resources/Fonts/Ttf/*.bytes`.
- `CanvasConformance.TextMetrics` and `probe-metrics.mjs`: width comparison against Chromium.

## Residual in text_fonts

Edge pixels of the reference (partial coverage) show a mean signed difference (unity minus reference) of about -5 on left edges, -17 on right edges, and about -1 / 0 on top and bottom edges. So the error is horizontal: the reference has slightly more ink on vertical stems, mostly on the right side, and none vertically. Constant shifts, vertical subpixel snapping, blur and contrast do not remove it. The likely cause is the reference rasteriser's horizontal coverage handling (Chromium on Windows rasterises glyphs through DirectWrite), which is not exact-area; reproducing it is outside this iteration.
