# Spec requests: CANVAS track

Nothing in the SPEC blocks the gameplay streams (all 30 pass the bar). Two points need a decision from the SPEC owner.

## 1. Text parity bar on dense synthetic text scenes

SPEC 6.1 mandates TextMeshPro SDF text. SDF glyphs reconstructed from a 96 px atlas cannot match FreeType/Skia glyph coverage exactly: the residual is a one-to-two pixel ring on glyph edges. `TextBias`/`TextGamma` have been swept (see `Assets/Scripts/Render/DESIGN.md`); the optimum is flat.

Effect: gameplay frames pass (99.2 - 99.7 %), but the three stress scenes that are almost entirely text fail the 99.0 % bar:

| scene | pix16 % | SSIM |
|---|---:|---:|
| text_align_baseline | 97.99 | 0.9876 |
| text_emoji | 98.61 | 0.9916 |
| text_fonts | 95.95 | 0.9746 |

Request: either (a) accept that bar for synthetic text-only scenes as informational, with the gameplay streams as the gating metric, or (b) allow an outline-based glyph rasteriser (glyph outlines read from the .ttf/.otf files and filled by the same exact-area pipeline as every other path) in place of the TMP SDF path. (b) is the only route to more than about 99 % on text and would need a font-parsing component.

## 2. Emoji sprite import settings

`Assets/Resources/Emojis/*.png` are imported DXT-compressed, not readable, no mips. DXT block artifacts show as differences of up to 255 on emoji edges in `text_emoji` and `drawimage`. The Render track does not own import settings.

Request: import the emoji sprites uncompressed (RGBA32), sRGB, no mips, point or bilinear filter as the reference uses, via `ParityAssetBuilder`.
