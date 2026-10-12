# Shaping spike: Arabic, Hindi, Khmer

Phase 0 feasibility. Throwaway proof only. Nothing here is a localisation implementation.

Samples below are engineer drafts so the shaper had real sentences to chew on. They are not translations.

## 1. Recommendation

Bake glyph runs offline with HarfBuzz (approach A) for the closed set of Arabic, Hindi, and Khmer strings, and draw those glyph ids with the existing outline rasteriser. Store font-unit advances, x/y offsets, bidi level, and cluster break flags. At runtime, scale advances by `px / unitsPerEm`, add letter-spacing once per cluster, wrap on the stored break flags, and reorder each line with the stored levels. Substitute numbers and Latin level names as their own runs, shaped with the cmap path the game already has, but only when the placeholder sits on a space or digit boundary. Confidence: **medium**. The offline shape-and-draw path produced joined Arabic, a Devanagari conjunct with matras, and Khmer coeng forms, and a `{0}` split matched a fully shaped sentence glyph-for-glyph in all three scripts. Confidence stays at medium because this spike did not run Unity, did not ask a reader of these languages to sign the pixels, and did not settle the font face or whether the Arabic UI mirrors.

## 2. Comparison

Estimates are judgment, not a tracked plan. "Verified" means this spike ran the check or read the named file. "Assumed" means it was not run.

| | A. Offline HarfBuzz | B. Runtime HarfBuzz plugin | C. Unity TextCore / ATG |
| --- | --- | --- | --- |
| Effort | 6–10 engineer-days for the bake, the draw/wrap path, and the gameplay call sites that splice numbers. Linguist QA is extra. | 10–20 engineer-days on top of the same draw/offset work, to build and bind a static lib. Not built here. | Replacing canvas text with UI Toolkit is a different renderer (weeks). Calling the internal shaper from game code is unsupported. |
| Risk | Medium. Glyph ids are pinned to a font file hash. Khmer line breaks and Arabic row direction are product risks, not shaper risks. | High. A second HarfBuzz next to the one already inside the engine, plus IL2CPP static-link bring-up that was not attempted. | High for the outline renderer. The public ATG entry is a UI Toolkit label, and it paints SDF meshes. |
| Runtime cost | Table lookup plus the current outline emit. No shaping on the frame. | Shaping cost on first use. Not measured on an A11. A cache would make the steady state look like A, with a native transition. | ATG shape + SDF raster inside UI Toolkit. Not timed. |
| Size | See §2.1. Order of 60–180 KB of glyph records per language, plus one subset font per script per weight. | HarfBuzz binary size not measured. The engine already contains a HarfBuzz face (`CreateHbFaceIfNeeded` in `UnityEngine.TextCoreTextEngineModule.dll`). | No extra native library. Font assets become TextCore SDF atlases, which this canvas does not consume. |
| iOS / Android | Same pure-C# IL2CPP path as today's outline text. | iOS static plugins are documented as `DllImport("__Internal")` in the 6000.4 manual `ios-native-plugin-create.html`. Android would be an arm64-v8a `.so`. Neither was linked. Upstream HarfBuzzSharp bindings import a library named `HARFBUZZ`, so the stock binding does not match `__Internal`. | ATG is documented for UI Toolkit on the platforms that ship UI Toolkit. It was not driven from this canvas. |
| Verified | uharfbuzz 0.56.3 shaped the samples; FreeType rasterised the glyph ids; placeholder stitch matched; Noto fonts are glyf. | HarfBuzz COPYING is the Old MIT licence (upstream `COPYING`). Unity documents `__Internal` for static iOS plugins. | Metadata of the 6000.4 editor assemblies, plus the local manual pages named below. |
| Assumed | A reader will accept Noto (or a rounder OFL face) in place of Fredoka. A11 memory is a non-issue at these sizes. | A11 shaping latency, stripped library size, and whether a patched HarfBuzzSharp actually loads under IL2CPP. | That internal `TextLib.ShapeText` would keep working across a Unity patch. It is non-public, so that is not a plan. |

### 2.1 Sizes measured in the proof

Fonts are the Google Fonts variable TTFs (glyf + gvar, default axis wght 400 / wdth 100), SIL OFL 1.1:

| Font | Full file | Glyphs | Sample-character subset (layout closure) |
| --- | ---: | ---: | ---: |
| Noto Sans Arabic | 844,676 B | 1,711 | 96,596 B |
| Noto Sans Devanagari | 641,944 B | 1,117 | 236,788 B |
| Noto Sans Khmer | 359,788 B | 673 | 129,692 B |

The subset is a floor for the characters in the 18 sample strings, not a prediction for 500 strings. Hindi is already 237 KB because conjunct closure pulls in many alternate glyphs from a short text. The full variable font is the ceiling if we refuse to subset. `HVAR` is in all three fonts, so advances baked at Regular are wrong for Bold. Glyph records have to be baked per weight instance. The runtime `TrueTypeFont` reader reads `glyf` and ignores `gvar`, so a variable file draws the default instance only.

Glyph records, packed as four int16s (gid, advance, dx, dy) = 8 bytes/glyph:

- Mean of the five short samples: ar 17.2, hi 15.4, km 14.2 glyphs. At 500 strings of that length: about 69 KB, 62 KB, 57 KB.
- The long confirm sample alone is ar 173, hi 157, km 99 glyphs. If a large share of the 500 strings are paragraphs, the table moves toward the 110–170 KB range.
- The compact JSON of the 18 samples is 71,394 bytes. That format repeats keys. Ship a binary or a flat array, not that JSON.

Cluster id and break flag add about 2 bytes/glyph if they are not implied by a parallel cluster array. Letter-spacing and font size must not be baked in: advances are in font units (these faces are 1000 UPM) and scale by `px / unitsPerEm`, and letter-spacing is a per-element CSS value.

## 3. Data format and runtime API

### 3.1 Record

One table per language, keyed by string id. Each string is a list of directional runs in logical order. Each run:

- `font` file hash (glyph ids are not stable across a font update)
- `level` from the bidi algorithm (odd = RTL)
- `glyphs[]`: `gid u16`, `adv i16`, `dx i16`, `dy i16`, `cluster u16` (index into the source string)
- `break_before` on the first glyph of each cluster that may start a line

A `{0}` slot is a run with no glyphs and a `slot` index. The proof's filled goal strings matched the concatenation of independently shaped fragments (`prefix` + `20` + `suffix`) on gid, dx, and dy for ar, hi, and km. The fragments in the proof are separated by spaces or are the digit run itself. A placeholder glued to a joining letter was not tested and should be rejected by the bake.

The proof's on-disk example is `/tmp/maze-i18n/shaping/out/shaped-runs.json`.

### 3.2 What already draws a glyph id

`OutlineFonts.Flat(TrueTypeFont, gid, px, devScale)` (`OutlineFonts.cs` lines 74–88) and `TrueTypeFont.Outline(gid)` (lines 155–160) already flatten a glyf outline, including compound glyphs (`Load`, lines 224–253). `EmitOutlineGlyphs` (`Canvas2D.OutlineText.cs` lines 209–258) already walks `LaidGlyph.gid`. The shaped Noto glyphs used for the titles, buttons, and goal lines have real `glyf` contours. The only empty outlines in those strings are the space glyph. No new glyf decoder is required for these faces. A CFF font would miss the `glyf` table and cannot be used; that was checked (these three files are TrueType, sfnt version `0x00010000`).

`TrueTypeFont.Substitute` (lines 403–437) applies only `ccmp` and `liga` for the `latn` / `DFLT` script (`CollectLookups`, lines 258–285). `Kerning` (lines 329–350) is GPOS pair adjustment only. Mark-to-base, Arabic joining, Devanagari half-forms, and Khmer coeng are outside that code. The proof's offsets show why a stored dx/dy is required: 61 of 259 Arabic glyphs, 15 of 234 Hindi glyphs, and 28 of 170 Khmer glyphs had a non-zero dx or dy.

### 3.3 API changes

`Canvas2D.OutlineText.cs`

- `LaidGlyph` (lines 9–14) gains `dx`, `dy`, and `cluster`.
- `Layout` (lines 67–138) stays the Latin/emoji path: cmap via `GlyphIndex`, `Substitute` at line 113, pair kerning at line 121, `letterSpacing` added after every glyph at line 122.
- Add a sibling that fills `laid` from a baked run: skip `GlyphIndex`, `Substitute`, and `Kerning`. Advance by the stored `adv * px / unitsPerEm`. Add `state.letterSpacing` only when `cluster` changes, and not after the last cluster. The UI's non-zero tracking is 0.2px–1px (`CssBox.ParseLength` treats `normal` as 0, `CssBox.cs` lines 48–56). The confirm samples have more glyphs than clusters (ar 173/133, hi 157/111, km 99/65), so a per-glyph add moves marks and coengs.
- `MeasureText` (lines 55–61) returns the scaled stored width for a baked string.
- `EmitOutlineGlyphs` (line 221) places the origin at `startX + pen + dx * scale`, `baseline - dy * scale` (HarfBuzz +y is up; the canvas +y is down).
- `DrawTextOutline` (lines 171–174) can keep painting a visual-order glyph list from the left edge of the line. `textAlign` `"start"` currently behaves as left.

`HomeChrome.cs`

- `EnsureRunStyle` (line 2188) sets `upperOf` from CSS `text-transform: uppercase`. `CachedUpper` (lines 2191–2197) and the call sites at lines 2366 and 2486 run `ToUpperInvariant` on the whole override. Skip that for ar/hi/km. These scripts have no case, and the transform would still rewrite any Latin token inside the string.
- `letterOf` is applied at lines 2329, 2427, and 2515. The baked run must be measured with that same per-cluster spacing.
- `WrapToWidth` (lines 2570–2575) and `WrapWords` (lines 2577–2595) split on U+0020 only. Khmer has no spaces between words. Replace the wrap for baked strings with: walk logical clusters, keep a chunk together until the next `break_before`, break when the next chunk's stored advance exceeds the width, then reorder that line's runs by bidi level before paint.
- `PaintRuns` (lines 2440–2568) and `PaintReflowLines` (lines 2412–2438) call `MeasureText` and `CssBox.DrawText` with a `string`. They need the string id's baked runs, and a line must be a slice of glyphs at a break, not a `Substring` (a character slice re-breaks joining).

`CssBox.DrawText` (`CssBox.cs` lines 518–557) calls `FillText` four times (shadows and the fill). `Canvas2D.FillText` (`Canvas2D.Text.cs` line 160) ignores `maxWidth` and calls `DrawText`. Once `DrawText` can paint a baked run, these call sites stay. Emoji stays on `CanvasEmoji.SplitRuns` inside `Layout` (line 77): a baked run covers the text fragments, and emoji code points stay on the existing image path.

Gameplay canvas, direct `FillText` / `StrokeText`:

- `GameRenderer.Gates.cs` lines 66, 153, 227, 287 (fixed labels plus a number), and lines 468–470 (`ft.Text`).
- `GameRenderer.Ring.cs` lines 108 and 111 (`hubEmoji`, `hubSubtitle`), font set at lines 98 and 109.
- `LiveGameSession.cs` `NomWords` line 32, drawn at line 437; the recorded set at lines 408–422.
- `StreamPlayer.cs` line 166 replays captured `fillText` ops. Those strings are the English spec. Leave that path on `Layout`.

The gate labels and nom words are a fixed set plus a number (`"×" + JsNumber.ToString(...)`). Bake the fixed words. Draw the number with the Latin cmap path (Western digits do not need HarfBuzz). Emoji in those strings stay on `SplitRuns`.

## 4. RTL beyond glyph order

Glyph order in the proof is visual left-to-right after bidi resolution, so the pen still moves right. That is the model `DrawTextOutline` already uses.

- **Alignment.** `CssBox.DrawText` forces `textAlign = "start"` and `textBaseline = "alphabetic"` (lines 523–524). `PaintRuns` then places the run at the captured left edge, or centers it when `IsCenteredBlock` says so (`PaintReflowLines`, lines 2417–2431). An Arabic paragraph in a left-aligned English box is the wrong side of the box. The lead has to choose: right-align RTL text inside the existing rects, or mirror the row (back button, icons, padding).
- **Arrows.** `←` and `→` are not in these three Noto faces. The proof mapped them to glyph 0 and the PNG shows an empty box (`ar/back`, `hi/back`, `km/back`). `OutlineFonts.SymbolChain` (line 23) is the fallback for the cmap path only. A baked run has to name which font each glyph came from, or the arrow stays a separate symbol/emoji run. Mirroring "← Back" is a string change (`رجوع` plus a mirrored arrow), not something HarfBuzz will invent.
- **Play glyph `▶`.** Media convention often leaves it unmirrored. That is a product call. This spike did not shape it.
- **Digits.** Use Western digits `0-9` for every `{0}` that `JsNumber` fills, in Arabic and in Hindi. The proof's `الهدف: 20 كغ` and `المرحلة 12 من Orbital Salvage` put the Western number and the Latin name in the right visual slots (number between the Arabic words, `Orbital Salvage` on the left, `المرحلة` on the right). Arabic-Indic and Devanagari digits are a separate product choice. They do not need a runtime shaper (they do not join), but they must not be turned on by a `locl` feature behind the bake. Khmer digits were not requested by the samples; the same Western-digit rule fits.

`text-transform: uppercase` does nothing useful for these scripts. Do not run it.

## 5. Risks and questions for the lead

Questions that can make these three languages miss a small budget:

1. **Does the Arabic UI mirror, or only the text?** Mirroring rows, the back arrow, and padding is a chrome change, not a shaping change. Text-only right alignment is the smaller change and will still look LTR in the navigation.
2. **Which face?** Fredoka and Nunito do not cover these scripts. Noto Sans (this spike) is OFL and shapes correctly, and it does not look like Fredoka. Baloo Bhaijaan 2, Baloo 2, and Battambang are rounder OFL faces and were not shaped here. A face without mark positioning will undo this spike.
3. **How many weights?** Floating combat text is `900 16px Fredoka` (`GameRenderer.Gates.cs` line 464). The variable fonts' default is wght 400, and `HVAR` means a Regular bake is the wrong width for Bold. Each shipped instance is another subset and another shaped table.
4. **Is syllable-level Khmer wrapping acceptable?** The confirm sentence wrapped to 11 lines at 56 px / 280 px by grapheme cluster, so words split. Arabic and Hindi wrapped on spaces and the lines are words (`هل أنت`, `क्या आप`). A Khmer dictionary (ICU) was not run. If the bar is dictionary word breaks, that is extra work, and it is still offline.
5. **Who signs the pixels?** The sample sentences were written for the shaper. A native reader has to accept both the translation and the line breaks before this is shippable. Shaping cannot repair a bad translation.
6. **Are placeholders always on a boundary?** The bake should fail a string whose `{0}` sits inside a joining sequence. Level names that are themselves Arabic or Hindi have to be baked as their own run, not inserted as raw characters after shaping.
7. **Emoji and symbols in gameplay strings** (`NOM! 😋`, `⚡ ×`) stay on the emoji/symbol path. They are not in the Noto text faces.

What does not threaten the budget, based on sizes measured here: RAM on an A11. A language table under 200 KB plus a subset font under 1 MB is small next to a 3 GB device. That was not measured on hardware.

Approach B does not remove the draw-offset work, and approach C does not return outlines. The Advanced Text Generator manual page (`UIE-advanced-text-generator.html`) says ATG uses HarfBuzz, ICU, and FreeType inside UI Toolkit. `TextGeneratorType.Advanced` is documented as comprehensive shaping and RTL. The types that would expose positions — `UnityEngine.TextCore.Text.TextGenerator`, `TextHandle`, `TextGenerationSettings`, `TextLib.ShapeText` — are non-public in `UnityEngine.TextCoreTextEngineModule.dll` from editor 6000.4.0f1 (System.Reflection.Metadata). `FontEngine.GetOpenTypeLayoutLookups` is internal. Public `FontEngine.TryGetGlyphWithIndexValue` looks up a glyph; it does not shape. The legacy public `UnityEngine.TextGenerator` is a different class (dynamic-font vertices in the ScriptReference) and has no glyph-id API.

## 6. Proof artefacts

All under `/tmp/maze-i18n/shaping`. Not in git.

| Path | What it is |
| --- | --- |
| `src/shape_poc.py` | uharfbuzz 0.56.3, python-bidi levels, FreeType raster of the glyph ids |
| `venv/` | throwaway virtualenv |
| `fonts/NotoSansArabic-VF.ttf` | OFL 1.1, `fonts/OFL-arabic.txt` |
| `fonts/NotoSansDevanagari-VF.ttf` | OFL 1.1, `fonts/OFL-devanagari.txt` |
| `fonts/NotoSansKhmer-VF.ttf` | OFL 1.1, `fonts/OFL-khmer.txt` |
| `out/shaped-runs.json` | proposed storage, visual glyph runs |
| `out/summary.json` | widths, wrap lines, placeholder match, subset sizes |
| `out/ar-sheet.png` `out/hi-sheet.png` `out/km-sheet.png` | the six samples per language |
| `out/{ar,hi,km}-confirm-wrapped.png` | logical wrap at 280 px, then each line redrawn from stored advances |
| `out/{ar,hi,km}-subset.ttf` | fontTools subset of the sample characters |

What the PNGs show, read back in this spike: Arabic goal is joined and ordered `كغ` / `20` / `الهدف` from left to right. Arabic mixed puts `Orbital Salvage` on the left and `المرحلة` on the right, with `12` between Arabic words. Hindi title shows `क्ष` and the matras in `कक्षीय सफ़ाई`. Hindi wrap line 0 is `क्या आप` with a space. Khmer title and `បន្តលេង` show coeng forms below the baseline. The back-arrow samples show a `.notdef` box. This was FreeType, not `TrueTypeFont.cs`. Unity was not launched.
