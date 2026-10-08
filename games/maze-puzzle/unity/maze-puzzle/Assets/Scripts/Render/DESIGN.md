# Canvas2D (OrbitalSalvage.Render) design

A native Unity re-implementation of the subset of the HTML Canvas 2D API used by `games/maze-puzzle/web/`, built to match headless Chromium (CPU Skia, DPR 3) pixel for pixel. No third-party rasterizer is used.

Pass bar from the SPEC: at least 99.0 % of pixels within a per-channel difference of 16, and SSIM at least 0.985. Our own target is 99.5 % / 0.99. Measured numbers live in `games/maze-puzzle/tools/parity/canvas-conformance/REPORT.md`.

## Pipeline

```
JS-style call -> CTM applied on the CPU -> flatten (tolerance 0.05 device px) -> [Stroker / Dasher] -> device-space edges
   -> one GPU quad per edge, additive signed-area accumulation (RFloat)
   -> resolve: coverage -> paint (solid / gradient) -> clip mask -> shadow tint -> premultiplied source-over into an ARGB32 target
```

All geometry is transformed on the CPU in double precision, so transforms, shear, and non-uniform scale need no shader support. A frame is one `CommandBuffer`, executed by `EndFrame`.

## Anti-aliasing: analytic exact-area coverage

Each edge is drawn as a quad covering the pixels from the edge's left side to the bounding-box right edge. The fragment adds the signed area of the trapezoid under the edge within that pixel to an RFloat accumulation target (additive blend). After all edges of a shape are in, the sum per pixel is the exact signed covered area. Non-zero fill is `saturate(|sum|)`, even-odd is a triangle wave of the sum.

Why this and not MSAA or supersampling:

- Skia's analytic AA (AAA) computes exact area coverage. With N samples, coverage is quantised to N+1 levels, so the maximum edge error is 1/(2N). 8x MSAA gives about 6 % (around 16/255) on a pixel, which sits on the SPEC tolerance line, and edge pixels dominate this content. 16x gives 3 %, but costs 16 samples of fill and still shows banding on near-horizontal edges. Exact area has no such error.
- It needs no MSAA resolve and no per-shape render targets of higher resolution; the cost scales with edge count and bbox height, not with shape area.
- One approach covers fills, strokes (the stroke is an outline polygon), clips, shadows, and text-less glyph masks.

This was not benchmarked against an MSAA implementation: the argument above is analytical, and the end-to-end measured result (all 30 gameplay streams and every non-text scene above the 99.0 % bar) is the evidence that exact area is sufficient.

### Matching Skia's quirk: Y snapping

Chromium rasterises with Skia AAA, which snaps edge Y coordinates to a 1/4 pixel grid. For near-horizontal edges this changes coverage by more than the SPEC tolerance, so `AddEdge` reproduces it: `y = floor(y*4 + 0.5)/4` with `x` re-derived along the original line, and edges with equal snapped Y values are dropped. `FillRect`/`ClearRect` pass the same snapping through the `SnapRects` switch.

### Clipping edges to the canvas

Y is clipped parametrically. X is split at 0 and W: pieces left of the canvas become vertical edges at x = 0 so their winding contribution is kept, pieces right of W are dropped. The bounding box is computed from the unclipped points clamped to the canvas.

## Compositing

The target is ARGB32 in a Gamma colour space project, treated as premultiplied 8-bit sRGB values, with `Blend One OneMinusSrcAlpha`. Chromium also blends in non-linear premultiplied 8-bit, so no linearisation is done. `ClearRect` is a dedicated pass writing transparent. Each frame starts by clearing the target to transparent, and the conformance tool composites the output over grey 128 for comparison.

## Paints

- **Solid colours**: `CssColor` parses hex, rgb(a), hsl(a) and named colours.
- **Gradients**: the CTM at fill time is baked into the inverse matrix rows passed to the shader, up to 8 stops. Radial gradients use two-point conical logic.
- **Interpolation mode (measured)**: `GradientInterpolatesPremultiplied` is false. With straight-alpha interpolation `gradients_linear` is 99.996 % / SSIM 0.998; with premultiplied interpolation it falls to 88.9 % / 0.980. Radial is identical in both modes.

## Shadows

Shadow geometry is the shape translated by the offset, rendered into a coverage mask (RHalf), blurred with a separable Gaussian of sigma = `shadowBlur / 2` (radius `ceil(3 sigma)`), then resolved tinted with the shadow colour times the paint alpha, and composited below the shape. Sigma is in device pixels; shadow offset and blur are not transformed by the CTM (as in the Canvas spec).

`ShadowScale` multiplies both the shadow offset and blur. Measured over `shadows_fill` and `shadows_stroke` at DPR 3: 0.8 gives mean 99.67 %, 1.0 gives 99.88 %, 1.2 gives 99.74 %. Sigma = blur/2 at unit scale is confirmed as the optimum, so the default is 1.

## Clipping

`Clip` renders the path coverage into an R8 mask: `mask = previous * coverage`. Clip masks are pooled per frame and released at `BeginFrame`.

## Strokes

`Stroker` follows Skia: the outline of each segment, joins and caps are polygons filled non-zero. Inner joins pivot through the centre point; miter joins beyond the limit fall back to bevel; round joins and caps use arc fans with an angular step tied to the flattening tolerance. `Dasher` follows Skia semantics, including merging the first and last dash of a closed contour. Zero-length sub-paths draw nothing (matches Chromium) unless the cap is round or square with a dot, and strokes narrower than one device pixel are drawn as one device pixel with alpha scaled by the width (Skia hairline).

## Text

Parity text (SPEC 6.4, v1.6) is an **outline glyph rasteriser**, selected by `Canvas2D.UseSdfText = false` (default).

- `TrueTypeFont` parses the static TTFs (glyf/loca simple and compound, cmap 4/12, hmtx/head/hhea, GPOS pair kerning, GSUB ccmp/liga). `OutlineFonts` loads them from `Resources/Fonts/Ttf`, resolves the face with `ParityFonts.ResolveWeight`, builds the fallback chain (primary, Noto Sans Symbols 2, Noto Sans Symbols) and caches flattened outlines per (face, glyph, quantised size).
- `Canvas2D.OutlineText.cs` lays text out like Blink (word-wise shaping, no kern or ligature across spaces, unrounded advances), places baselines from the rounded hhea metrics, snaps the device origin (x to 1/4 px, y to an integer) and fills the polygons non-zero with the normal exact-area filler. `Fill.shader` applies a Skia-style glyph-mask preblend (sRGB, contrast 0.5) when the draw is flagged as glyph mask.
- Strokes use the standard stroker on the unsnapped outline. Emoji runs stay on the sprite path below.
- Calibration (`GlyphContrast`, `GlyphSubpixel`, `GlyphSubpixelY`, `GlyphRound`, `GlyphDx/Dy`) was swept on the four text scenes; the defaults are the optimum, and the sweeps are flat to within 0.1 point. `text_fonts` ends at 98.45 % (bar 99.0), the rest of the text scenes pass.

### Legacy SDF path (off by default)

TextMeshPro is used only as an SDF glyph atlas provider. Layout is done manually so that it is deterministic and independent of TMP's component pipeline:

1. `ParityFonts.Resolve(family, weight)` returns the dynamic `TMP_FontAsset` (point size 96, padding 12, atlas rendering SDF).
2. Glyphs come from `characterLookupTable`, populated on demand with `TryAddCharacters(..., includeFontFeatures: true)`, then the fallback font table. Failed lookups are cached so absent glyphs do not retry every frame.
3. Kerning is built from `fontFeatureTable.glyphPairAdjustmentRecords` into a dictionary keyed `(second << 16) | first`.
4. The glyph origin is snapped in device space (x to 1/4 pixel, y to an integer) when the CTM has no rotation or skew, which is what Skia does with subpixel positioning on.
5. Baselines (`alphabetic`, `middle`, `top`, `hanging`, `bottom`) are placed from em box metrics checked against Chromium `measureText` output (`probe-metrics.mjs`).
6. The fragment shader reconstructs a signed distance in device pixels, `dist = (d - 0.5) * 2 * SdfRange * devPerTexel + TextBias`, then coverage = `saturate(dist + 0.5)` for fills and `saturate(halfW + 0.5 - |dist|)` for strokes.

Calibration (measured over the four text scenes, mean pix16): `TextBias` 0.25 is the optimum (-0.1: 96.98, 0.1: 97.56, 0.25: 97.95, 0.4: 97.75, 0.6: 97.01). `TextGamma` has a monotonic but tiny effect (0.8: 97.99, 1.0: 97.95, 1.3: 97.80), so it stays 1.0.

**Known ceiling**: an SDF reconstructed from a 96 px atlas cannot reproduce FreeType's outline coverage exactly; the residual is a one-to-two pixel ring around glyph edges. In gameplay frames this still clears the bar (text is a small share of pixels), but dense synthetic text scenes (`text_fonts`, `text_align_baseline`, `text_emoji`) end at 96-98.6 %. Closing that gap would require an exact outline rasteriser for glyphs, which means extracting outlines from the font files (TMP does not expose them).

**Emoji** are drawn per SPEC 6.2 as `fontPx` squares with `DrawImage` from `Resources` sprites listed in `emoji_index.json`. `strokeText` skips emoji. `CanvasEmoji` classifies graphemes (Extended_Pictographic plus FE0F, or Emoji_Presentation).

Not supported: shadows on text, `globalCompositeOperation` other than source-over, `strokeText` joins other than round.

## Images

`DrawImage` supports the three JS overloads through a dedicated shader. Rows are flipped in the shader (`Texture2D.Load` addresses file textures bottom-up). The emoji sprites are imported as compressed (DXT) textures, which shows up as block artifacts in `text_emoji` and `drawimage` (see the spec request).

## Performance notes and optimisation paths

See the Performance section of REPORT.md for per-stream numbers. Main costs are draw-call count (about 1 100 - 2 150 draws per gameplay frame) and per-op CPU recording. Ordered by expected gain:

1. Batch consecutive solid axis-aligned rectangle fills into a single instanced draw, skipping the accumulation pass for pixel-aligned rects.
2. Merge consecutive shapes with the same paint and no intervening clip or shadow into one accumulation batch (the accumulation target is cleared once per batch via the bbox union).
3. Reduce CPU allocations in the stroker and dasher (pooled lists exist; the remaining per-op `List` creation in path flattening can move to struct buffers).
4. Move the accumulation target from RFloat to RHalf where the device supports additive blending on it; verify on mobile, as RFloat blending is not guaranteed on all GPUs.
5. Texture-atlas the emoji sprites to cut state changes.

## Build notes

- Shaders are located with `Shader.Find("Hidden/Canvas2D/...")`. A Player build must add them to Project Settings > Graphics > Always Included Shaders, or the shaders will be stripped.
- Everything in `OrbitalSalvage.Render` is runtime code (no `UnityEditor` references); the conformance runner lives in `OrbitalSalvage.Render.Editor`, tests in `OrbitalSalvage.Render.Tests`.
- Orientation: render textures have row 0 at the bottom, so `_C2D_FlipY = 1` is set once per frame.
