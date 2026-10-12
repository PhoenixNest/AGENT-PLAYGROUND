# Fonts per language — Phase 0 spike

Unity outline text can only draw **static TrueType `glyf`**. Every face below was checked against that, subset with `games/maze-puzzle/localization/tools/subset_fonts.py`, and sized as uncompressed `.ttf` (what `Resources` stores) and gzip-9 (a stand-in for IPA/APK deflate). No font binaries are in this commit. Downloads used for the measurement are under `/tmp/maze-i18n/fonts/dl`.

**Ship one new file per script, not per language.** Ten Latin languages share Fredoka + Nunito (already in the app) plus one Baloo 2 for Vietnamese, Uzbek, and Hindi. CJK is one face used for both display and body.

| Plan | What it is | Added `.ttf` | gzip-9 |
| --- | --- | ---: | ---: |
| Low | ~800-glyph CJK, Do Hyeon, Huninn, shared Baloo 2 | 1.26 MB | 0.66 MB |
| High, tighter | Safe sets, Noto TC instead of Huninn, Do Hyeon instead of Jua | 4.4 MB | 2.6 MB |
| High, roundest | Safe sets, Huninn + Jua | 5.7 MB | 3.0 MB |

"Added" does not include Fredoka, Nunito, or the two Noto symbol fonts already shipped. iPhone X is capped at raster scale 2 (`QualityGovernor.LowRamMb` = 3500).

## 1. What the loader can draw

Evidence is `Assets/Scripts/Render/TrueTypeFont.cs` unless noted.

| Feature | Supported? | Evidence |
| --- | --- | --- |
| TrueType `glyf` + `loca` (simple glyphs) | Yes, required. Missing `glyf` throws. | Constructor comment lines 19–21; `Table("glyf")` at line 69 |
| CFF / CFF2 / OTTO `.otf` | No. The sfnt magic is not checked (table count is read at offset 4, line 46), then `glyf` is required. | line 69 |
| Variable fonts (`fvar` / `gvar` / `CFF2`) | No. Nothing reads `fvar`. A variable file would draw the default master only, which is often weight 100 or 400, not bold. | no `fvar` references in the file |
| cmap | Format **4 and 12 only**, Unicode platform 0, or platform 3 encoding 1 or 10. Format 12 is preferred, then format 4. Format 14 (UVS) is ignored. | lines 78–81, 108–110 |
| Composite glyphs | Yes, to depth 6, including uniform scale, xy scale, and a 2×2. | lines 174–177, 224–253 |
| Point-attached composites (`ARGS_ARE_XY_VALUES` clear) | No. Offsets become 0. | lines 232 and 237 |
| GSUB | Only scripts `latn` then `DFLT`, default language. Features `ccmp` and `liga` only. Lookup types 4 (ligature) and 2 (multiple), plus extension type 7. Types 1, 3, and 6 are not applied. | lines 263–269, 398–401 |
| GPOS | Only `kern` on `latn`/`DFLT`. Lookup type 2 formats 1 and 2, extension type 9. No mark, cursive, or kern-table. | lines 83–84, 329–348 |
| Hinting, `name`, bitmap, SVG, COLR, TTC collections | Not read. A `.ttc` header is not parsed. | table load list lines 52–69 |
| OS/2 | Optional. `sTypoAscender` / `sTypoDescender` at offsets 68 and 70 drive top/middle/bottom baselines. If the table is absent, hhea metrics are used. | lines 60–64; baselines in `Canvas2D.OutlineText.cs` lines 176–180 |

Chain, today (`OutlineFonts.cs` lines 47–61, `Canvas2D.OutlineText.cs` lines 95–101):

1. Primary is **Fredoka or Nunito only**. Any other family name is forced to Nunito.
2. Then `NotoSansSymbols2-Regular`, then `NotoSansSymbols-Regular`.
3. First font with a non-zero glyph id wins. A miss draws nothing.
4. `Substitute()` runs only when letter-spacing is 0 (line 113). It cannot shape Arabic, Devanagari, or Khmer.

Weight (`ParityFonts.cs` lines 12–15, 34–35, 101–117): there is **no synthetic bold**. Fredoka faces are 400/600/700 (CSS 800 and 900 become 700). Nunito faces are 700/800/900 (CSS 400 becomes 700). Desired weight above 500 picks the lightest face that is at least that heavy, else the heaviest face that exists.

Titles are uppercased with `ToUpperInvariant` when the spec says `tt=uppercase` (`HomeChrome.cs` lines 2188 and 2492). That is not Turkish-aware. Turkish is not a target language.

Raster: `EmitOutlineGlyphs` flattens with `ctm.MaxScale()` (`Canvas2D.OutlineText.cs` line 214). Tolerance is `0.02 / devScale` CSS px (`OutlineFonts.cs` lines 25 and 84). One `RenderShape` fills the whole run (line 254): edges go into one buffer (starts at 4096, doubles) and one stencil pass. There is no glyph atlas. The parity harness uses scale 3 (`ParityContracts.cs` `ParityProfiles.Scale`). The product governor never uses scale 3 when `systemMemorySize` is 1–3500 MB, and the comment names iPhone X at about 3 GB (`QualityGovernor.cs` lines 27–28 and 91–93).

## 2. Candidates

All files below are `glyf` TrueType (variable fonts included). None of the recommended faces needed a CFF conversion. Licence lines are the copyright preamble of the file we downloaded, not the OFL boilerplate. "Reserved Font Name" inside the OFL body is the standard clause; only Noto CJK actually declares a reserved name.

Source root: `https://github.com/google/fonts/tree/main/`.

| Group | Primary | Fallback | Weights | Licence preamble | Download |
| --- | --- | --- | --- | --- | --- |
| Latin display (vi, uz, and Hindi) | **Baloo 2** | Nunito (already shipped) | axis 400–800. Ship 700. No 900. | Copyright 2019 The Baloo 2 Project Authors (https://github.com/EkType/Baloo2). OFL 1.1. | `ofl/baloo2/Baloo2[wght].ttf` |
| Latin body, all Latin languages | **Nunito** (keep) | symbols | 700/800/900 already shipped. The repo `Nunito-700.ttf` is the full 1103-glyph face and already contains `ơ ư ă đ`. | already `Nunito-OFL.txt` | no new file |
| Western Latin display | **Fredoka** (keep) | Nunito | 400/600/700. Full Google Fonts variable is only 335 glyphs. | already `Fredoka-OFL.txt` | no new file |
| Cyrillic (not a target) | Nunito | none of Fredoka or Baloo 2 | Nunito covers Russian-range Cyrillic; 165 of 892 assigned block letters are missing (extensions). | same | — |
| Simplified Chinese | **ZCOOL KuaiLe** | Noto Sans SC | One file, OS/2 weight **400**. No bold master. Covers all 3755 GB2312 level-1 hanzi. | Copyright 2018 The ZCOOL KuaiLe Project Authors (https://github.com/googlefonts/zcool-kuaile). OFL. | `ofl/zcoolkuaile/ZCOOLKuaiLe-Regular.ttf` (1.5 MB full) |
| Traditional Chinese | **Huninn** | Noto Sans TC | One file, weight **400**. Rounded. Misses 48 of 5401 Big5 frequent hanzi (4 of those sit in the first 800 of that order). | Copyright 2025 The Huninn Project Authors (https://github.com/justfont/Huninn). OFL. `ofl/huninn/` | `ofl/huninn/Huninn-Regular.ttf` (4.7 MB full) |
| Japanese | **M PLUS Rounded 1c Bold** | Zen Maru Gothic Bold | Real weight **700**. ExtraBold 800 and Black 900 exist as sibling files (not sized). Covers all 2136 jōyō including `𠮟` U+20B9F. | OFL stated in the font (name IDs 0 and 13) and in `METADATA.pb` (`license: "OFL"`). **`OFL.txt` is missing** from `ofl/mplusrounded1c/`. Copyright 2016 The Rounded M+ Project Authors. | `ofl/mplusrounded1c/MPLUSRounded1c-Bold.ttf` (3.5 MB full) |
| Japanese fallback | Zen Maru Gothic Bold | Noto Sans JP | Weight 700. Black 900 exists. Misses 4 jōyō: `塡剝頰` and `𠮟`. | Copyright 2021 The Zen Maru Gothic Project Authors. OFL file is present. | `ofl/zenmarugothic/ZenMaruGothic-Bold.ttf` |
| Korean | **Jua** for the look, **Do Hyeon** if the safe set must be small | the other one | Both are a single weight-400 file. Both contain all 2350 KS X 1001 syllables. Jua is rounder and much heavier in bytes and contour points. | Jua: Copyright 2018 The Jua Project Authors. Do Hyeon: Copyright 2018 The Do Hyeon Project Authors. Both OFL. | `ofl/jua/Jua-Regular.ttf`, `ofl/dohyeon/DoHyeon-Regular.ttf` |
| Hindi | **Baloo 2** (same file as Latin) | Noto Sans Devanagari | Baloo 2 GSUB/GPOS scripts include `deva` and `dev2`. Core alphabet `अ`–`ह` plus matras is present. | same Baloo 2 OFL. Noto: Copyright 2022 The Noto Project Authors (https://github.com/notofonts/devanagari). | Noto: `ofl/notosansdevanagari/NotoSansDevanagari[wdth,wght].ttf` |
| Khmer | **Kantumruy Pro** | Battambang Bold | Axis 100–700. GSUB/GPOS script `khmr`. Core letters present. Battambang also has Black 900; its GPOS is only `DFLT`. | Copyright 2022 The Kantumruy Pro Project Authors (https://github.com/googlefonts/kantumruy). Battambang: Copyright 2019 The Battambang Project Authors. | `ofl/kantumruypro/KantumruyPro[wght].ttf` |
| Arabic | **Cairo** | Baloo Bhaijaan 2 | Cairo axis weight 200–1000, slant −11–11 (pin slant to 0). GSUB/GPOS scripts `arab` and `latn`. Core letters `ا`–`ي` present. | Copyright 2009 The Cairo Project Authors (https://github.com/Gue3bara/Cairo). Bhaijaan: Copyright 2019 The Baloo 2 Project Authors. | `ofl/cairo/Cairo[slnt,wght].ttf` |

Rejected or demoted:

- **Baloo Bhai 2 is Gujarati**, not Hindi. Its cmap has 2 Devanagari code points. GSUB scripts are `gujr` / `gjr2`. Do not use it for `hi`.
- **Kosugi Maru** is Apache-2.0 (`apache/kosugimaru/LICENSE.txt`), weight 400, and misses the same 4 jōyō as Zen Maru. Legal to ship, worse fit than M PLUS.
- **Tajawal** covers core Arabic and has Bold/Black, but GPOS is only `DFLT` and the face is less rounded than Cairo or Bhaijaan.
- **Noto Sans SC/TC/JP/KR** are variable `glyf` (not CFF), weights 100–900, and huge before subset: SC 17.8 MB / 31036 glyphs, TC 11.9 MB / 20950, JP 9.6 MB / 17936, KR 10.4 MB / 24964. They are the coverage ceiling. Copyright preamble: `Copyright 2014-2021 Adobe, with Reserved Font Name 'Source'`. A subset must not be renamed to Source. Shipping the OFL sidecar is required. Prefer them only where the rounded face has holes (Traditional Chinese, if those 48 characters show up).

Fredoka source coverage, measured on the full variable font: Western European sample (`äöüßàâæçéèêëîïôœùûÿñáíóúãõ`) is complete. Vietnamese is not (`ơưăđ` and the precomposed tone letters). Turkish `İ ş ğ` is not. Uzbek `ʻ` (U+02BB) is not. Nunito and Baloo 2 contain all of those.

## 3. Sizes

Charset construction (not translations; there are no translations yet):

- English UI, so the Latin set is real: 44 `*__zero.json` specs, 5365 `runs[].s` records, **380** unique trimmed strings, **82** code points (ASCII plus `× • ← ▶ ✓`). C# adds more English sentences in `AppController.cs`, `EconomyCatalog.cs`, `MazeLevelBuilder.cs`, `EconomyManager.cs`, and `Render/Game/*.cs`. Emoji are `CanvasEmoji` sprites, not font glyphs.
- Latin "full" set = Basic Latin + Latin-1 + Latin Extended-A + Latin Extended-B + Latin Extended Additional + Uzbek `ʻ`. That is what a Vietnamese-capable Latin subset must include. `ơ`/`ư` live in Extended-B; a subset that stops at Extended-A silently drops them.
- Simplified safe set = **GB2312 level 1 = 3755 hanzi**, not 3500. (The 3500 figure is the 通用规范一级 list, a different list.) Plus ASCII, UI symbols, CJK punctuation U+3000–303F, fullwidth U+FF00–FF60.
- Traditional safe set = Big5 frequently-used **5401** hanzi (bytes 0xA440–0xC67E), same punctuation.
- Japanese safe set = **2136** jōyō from [cjkvi `joyo2010.txt`](https://github.com/cjkvi/cjkvi-tables/blob/master/joyo2010.txt), including `𠮟` U+20B9F, plus hiragana and katakana.
- Korean safe set = **2350** KS X 1001 hangul syllables via EUC-KR.
- Realistic "~800" = the first 800 of that same order (for Japanese: 500 kanji + kana, 845 code points total). This is a size probe, not a corpus frequency cut. Real translation strings will be a bit different and should be re-subset later.
- Arabic / Devanagari / Khmer = the Unicode blocks plus Basic Latin. "Missing" counts below are **assigned** code points only (`unicodedata` category other than `Cn`). Many block slots are unassigned on purpose.

Bytes are the subset with hinting off, `name` / `post` / OS/2 dropped, cmap format 4 (and 12 when a non-BMP character is present). Complex-script rows keep GSUB, GPOS, and GDEF. KiB = bytes/1024.

### Latin, Hindi, Arabic, Khmer

| Face | Set | Glyphs | Assigned missing | `.ttf` | gzip-9 |
| --- | --- | ---: | ---: | ---: | ---: |
| Fredoka 700 | Latin blocks above | 218 | 382 (no Vietnamese) | 22.9 KiB | 14.1 KiB |
| Nunito 700, layout dropped | Latin + Extended-B | 665 | horn letters present | 42.4 KiB | 20.9 KiB |
| Nunito 700, **layout kept** | same | 758 | same | 60.8 KiB | 29.1 KiB |
| Baloo 2 700, layout dropped | Latin + Extended-B | 611 | horn letters present | 47.1 KiB | 23.1 KiB |
| Baloo 2 700, **layout kept** | Latin + Devanagari blocks | 1481 | core `deva` present | **378.8 KiB** | **142.3 KiB** |
| Noto Sans Devanagari 700 | Devanagari block, layout kept | 841 | 5 | 175.7 KiB | 72.3 KiB |
| Cairo 700 (slant 0) | Arabic blocks, layout kept | 525 | core letters present | **51.8 KiB** | **26.1 KiB** |
| Cairo 900 | same | 525 | same | 51.8 KiB | 26.2 KiB |
| Baloo Bhaijaan 2 700 | same | 640 | core letters present | 78.5 KiB | 40.3 KiB |
| Tajawal Bold | same | 293 | core letters present | 35.1 KiB | 17.6 KiB |
| Noto Sans Arabic 700 | same | 820 | 3 (the UI arrows) | 87.7 KiB | 46.8 KiB |
| Kantumruy Pro 700 | Khmer blocks, layout kept | 376 | core letters present | **64.8 KiB** | **28.1 KiB** |
| Battambang Bold | same | 352 | core letters present | 55.9 KiB | 24.9 KiB |
| Noto Sans Khmer 700 | same | 441 | 3 | 78.4 KiB | 30.0 KiB |

Nunito with layout kept is **smaller** than the shipped `Nunito-700.ttf` (129.2 KiB × 3 weights). Re-subsetting is optional and is not required for Vietnamese body text.

Keeping Latin `latn` GSUB/GPOS matters: the loader applies `ccmp`, `liga`, and `kern` for `latn`. The Baloo+Devanagari number above keeps them. Dropping OS/2 saves well under 1 KiB and changes baselines. Ship with `--keep-os2`.

### CJK

| Face | Set | Glyphs | Assigned missing | `.ttf` | gzip-9 |
| --- | --- | ---: | ---: | ---: | ---: |
| ZCOOL KuaiLe | GB2312 level 1 (3755) | 3969 | 0 hanzi (51 are punctuation/arrows) | **739.1 KiB** | **554.8 KiB** |
| ZCOOL KuaiLe | first 800 + punctuation | 1014 | same punctuation | 167.9 KiB | 127.1 KiB |
| Noto Sans SC 700 | GB2312 level 1 | 4018 | 0 | 1092.9 KiB | 730.1 KiB |
| Noto Sans SC 700 | first 800 | 1063 | 0 | 255.8 KiB | 176.0 KiB |
| Huninn | Big5 frequent (5401) | 5578 | 48 hanzi | **2193.1 KiB** | **1371.7 KiB** |
| Huninn | first 800 | 1021 | 4 of those hanzi, plus punctuation | 225.4 KiB | 147.3 KiB |
| Noto Sans TC 700 | Big5 frequent | 5664 | 0 | 1806.8 KiB | 1163.7 KiB |
| Noto Sans TC 700 | first 800 | 1063 | 0 | 185.0 KiB | 127.7 KiB |
| M PLUS Rounded 1c Bold | 2136 jōyō + kana | 2567 | 0 kanji (cmap **4 and 12**) | **1157.7 KiB** | **594.8 KiB** |
| M PLUS Rounded 1c Bold | ~800-glyph probe | 823 | 0 of that probe | 307.2 KiB | 167.0 KiB |
| Zen Maru Gothic Bold | jōyō + kana | 2538 | 4 (`塡剝頰𠮟`) | 1012.5 KiB | 607.2 KiB |
| Kosugi Maru | jōyō + kana | 2526 | same 4 | 974.3 KiB | 588.7 KiB |
| Noto Sans JP 700 | jōyō + kana | 2589 | 0 (cmap 4 and 12) | 683.0 KiB | 433.8 KiB |
| Jua | 2350 KS X 1001 | 2445 | 0 syllables (punctuation holes) | **1281.9 KiB** | **353.4 KiB** |
| Jua | first 800 syllables | 895 | 0 syllables | 437.1 KiB | 130.7 KiB |
| Do Hyeon | 2350 KS X 1001 | 2544 | 0 syllables | **298.0 KiB** | **121.1 KiB** |
| Do Hyeon | first 800 syllables | 994 | 0 syllables | 98.0 KiB | 42.4 KiB |
| Noto Sans KR 700 | 2350 KS X 1001 | 2614 | 0 | 427.7 KiB | 201.9 KiB |

`--retain-gids` keeps source glyph ids. Useful only if something bakes ids from the **unsubsetted** font. If HarfBuzz shapes against the subset, it is pure overhead:

| Face | Without | With `--retain-gids` | Glyph slots |
| --- | ---: | ---: | --- |
| Tajawal Bold | 35.1 KiB | 36.1 KiB | 293 → 474 |
| Noto Sans Arabic 700 | 87.7 KiB | 93.4 KiB | 820 → 1705 |
| ZCOOL, 800 set | 167.9 KiB | 215.1 KiB | 1014 → 7050 (the source font's gid space) |
| M PLUS, 800 set | 307.2 KiB | 364.2 KiB | 823 → 7937 |

Do not pass `--retain-gids`. Pre-shape against the subset file.

## 4. Assignment

Fallback chain is the order to implement. It is **not** what `OutlineFonts.Primary` does today.

| Languages | Display | Body | Chain | New file |
| --- | --- | --- | --- | --- |
| en, de, fr, es, it, pt-PT, fil, id | Fredoka 600/700 (existing) | Nunito 700/800/900 (existing) | Fredoka → Nunito → Symbols2 → Symbols | none |
| vi, uz | Baloo 2 **700** | Nunito (existing; already has Vietnamese) | Baloo 2 → Nunito → Symbols2 → Symbols | Baloo 2 |
| hi | Baloo 2 **700** | Baloo 2 **700** | Baloo 2 → Nunito → Symbols2 → Symbols | same Baloo 2 file |
| zh-Hans | ZCOOL KuaiLe | same | ZCOOL → Nunito → Symbols2 → Symbols | ZCOOL |
| zh-Hant | Huninn | same | Huninn → Nunito → Symbols2 → Symbols | Huninn |
| ja | M PLUS Rounded 1c **700** | same | M PLUS → Nunito → Symbols2 → Symbols | M PLUS Bold |
| ko | Jua (or Do Hyeon) | same | Jua → Nunito → Symbols2 → Symbols | one Korean file |
| km | Kantumruy Pro **700** | same | Kantumruy → Nunito → Symbols2 → Symbols | Kantumruy |
| ar | Cairo **700**, slant 0 | same | Cairo → Nunito → Symbols2 → Symbols | Cairo |

CSS weight once those faces exist: register only the shipped weight. `ResolveWeight` will map every CSS weight onto that single face (for a lone 700, CSS 400 and CSS 900 both become 700). That matches Fredoka, where 800/900 already become 700. There is still no synthetic bold, so a weight-400 design stays at that outline.

Style compromises:

- **Vietnamese titles cannot stay on Fredoka.** A word would mix Fredoka bases with a fallback for `ơ`/`ư`. Baloo 2 is the rounded replacement. Nunito-as-display is free (already shipped) and looks like body text.
- **ZCOOL KuaiLe, Huninn, Jua, and Do Hyeon have no 600–900 master.** They are drawn as their only outline no matter what CSS says. ZCOOL and Jua are already display-weight drawings. Huninn is a regular rounded gothic.
- **Traditional Chinese** will not match Fredoka. Huninn is the rounded option and has 48 rare holes. Noto Sans TC is complete, slightly smaller, and gothic.
- **Korean:** Jua matches the cartoon and costs 1.3 MB uncompressed for all 2350 syllables (353 KiB gzip). Do Hyeon covers the same syllables in 298 KiB (121 KiB gzip) and is less round. Jua outline points are much higher (see §6).
- **Arabic and Khmer** can be rounded (Cairo, Kantumruy) but will not look like Fredoka. Unshaped output is a renderer limit, not a font limit (§7).

## 5. Conversion

fontTools 4.66.1 `pyftsubset` has **no** `--variations` switch. The script pins axes first:

```bash
python3 -m fontTools.varLib.instancer --static -o Cairo-700.ttf \
  "Cairo[slnt,wght].ttf" wght=700 slnt=0
python3 games/maze-puzzle/localization/tools/subset_fonts.py \
  --font Cairo-700.ttf --text-file arabic.txt --layout keep --keep-os2 \
  --out Cairo-700.subset.ttf
```

Passing `--variations wght=700` on a variable input does both steps. Unspecified axes are pinned to their `fvar` default (Cairo slant 0, Noto width 100, Fredoka width 100).

CFF was tested on Adobe Source Sans 3 Bold (`OTTO`, `CFF `, OFL, not a candidate). `subset_fonts.py --cff-to-ttf` rebuilds `glyf` via `Cu2QuPen` (1 font-unit error, contours reversed). The result opened in fontTools as sfnt `\x00\x01\x00\x00`, with `glyf`, cmap format 4 (platform 0/3 and platform 3/1), and no `CFF `. None of the recommended faces need this.

Noto CJK sizes used a cheaper equivalent: subset the variable font while keeping `name`/`fvar`/`gvar`, instantiate `wght=700`, then run this script. A Nunito spot-check of that order matched instantiate-then-subset outlines (143 comparable glyphs, 0 diffs).

Every output above has `glyf`, no `fvar`, and cmap format 4. M PLUS and Noto JP jōyō subsets also have format 12 because of U+20B9F. The loader accepts both.

## 6. Performance estimate (not a measurement)

Sample of 80 BMP hanzi/hangul from the subsets, outline points before flattening:

| Face | Contours p50 / p90 | Points p50 / p90 | Composites in the sample |
| --- | --- | --- | --- |
| Noto Sans SC 700 | 5 / 8 | 53 / 81 | 0 |
| ZCOOL KuaiLe | 2 / 3 | 24 / 39 | 0 |
| Huninn | 2 / 5 | 71 / 107 | 0 |
| M PLUS Rounded Bold | 2 / 5 | 103 / 136 | 0 |
| Jua | 3 / 4 | 181 / 213 | 0 |
| Noto Sans KR 700 | 7 / 8 | 39 / 59 | 0 |

At iPhone X's cap of scale 2, tolerance is 0.01 CSS px, so rounded curves subdivide further (the flattener caps a quadratic at 64 segments). A rough ceiling for 100 glyphs is on the order of 100 × 200 points × a small curve factor, about 20–40k edges. `RenderShape` emits those edges once per text run, not once per glyph, then one fill. The edge buffer doubles from 4096, so 40k edges is a memory bump, not a new draw-call storm. Shadow text emits the edges again and then blurs. **That blur, and redrawing the whole canvas every frame, is the more likely cost.** ZCOOL is cheaper than Noto SC. Jua is the outlier. This was not timed on a device, and Unity was not launched.

## 7. Licence files to ship

Match the existing `Assets/Fonts/Fredoka-OFL.txt` pattern. Subsetting is a modification; the OFL still requires the copyright notice beside the font, and this spike drops the `name` table, so the sidecar is the only notice.

| File to add | When |
| --- | --- |
| `Baloo2-OFL.txt` | vi / uz / hi |
| `ZCOOLKuaiLe-OFL.txt` | zh-Hans |
| `Huninn-OFL.txt` | zh-Hant |
| `MPLUSRounded1c-OFL.txt` | ja — **write this from name ID 0 + the standard OFL 1.1 text; the Google Fonts folder does not contain one** |
| `Jua-OFL.txt` or `DoHyeon-OFL.txt` | ko |
| `KantumruyPro-OFL.txt` | km |
| `Cairo-OFL.txt` | ar |
| `NotoSansSC-OFL.txt` (and TC/JP/KR) | only if that Noto face is actually bundled. Reserved Font Name is **Source**, not Noto. |

Nunito and Fredoka OFL files are already in `Assets/Fonts/`. Kosugi Maru, if ever used, needs the Apache-2.0 `LICENSE.txt`, not an OFL.

## 8. Risks and open questions

1. **Arabic, Hindi, and Khmer will not shape** in this renderer even after the fonts land. GSUB for `arab` / `deva` / `khmr` is kept in the file for the other spike. Today's `Substitute()` only runs `latn`/`DFLT` `ccmp`+`liga`, and single-subst (type 1) is explicitly skipped. Expect isolated Arabic letters and uncomposed Devanagari until pre-shaping or a shaper exists.
2. **Four recommended faces have no bold master** (ZCOOL, Huninn, Jua, Do Hyeon; OS/2 weight 400). CSS 700 will not thicken them. M PLUS Bold, Cairo, Kantumruy, and Baloo 2 do have a real 700. Baloo 2 stops at 800, so CSS 900 becomes 800.
3. **Safe CJK subsets dominate the binary.** Roundest safe set is about **5.7 MB** uncompressed / **3.0 MB** gzip. The 800-glyph probe is about **1.26 MB** / **0.66 MB**. Those 800s are prefixes of the standard order, not the future translation. Re-subset when strings exist.
4. **Huninn misses 48 Big5 hanzi, Zen Maru and Kosugi miss 4 jōyō.** A character with no glyph in the chain draws nothing. M PLUS covers the jōyō list; Noto Sans TC covers the Big5 list. Do not bundle both a rounded face and Noto unless a hole is real.
5. **The loader cannot select these faces yet.** `OutlineFonts.Primary` maps every non-Fredoka family to Nunito. Per-language chain, OS/2 retention, and `--keep-os2` are follow-up work. M PLUS still needs an OFL sidecar before it can ship.

## 9. What was verified, and what was not

Verified, with fontTools 4.66.1 in `/tmp/maze-i18n/fonts/venv` (not run inside Unity):

- Loader limits by reading the C# cited above.
- Source coverage of Vietnamese, Turkish letters, Uzbek `ʻ`, jōyō, GB2312, Big5, KS X 1001, Devanagari core, Arabic core, and Khmer core, on the **unsubsetted** files.
- The shipped `Nunito-700.ttf` already contains `ơ ư ă đ`. The shipped Fredoka does not.
- Subset `.ttf` and gzip-9 sizes, cmap format, `glyf`, absence of `fvar`, and GSUB/GPOS script tags.
- Variable instantiation and one CFF→`glyf` conversion.
- `--retain-gids` size delta on four faces.
- Outline contour and point counts on 80 glyphs per face.

Not verified:

- Unity import, a device frame time, or a screenshot of any face next to Fredoka.
- HarfBuzz shaping, mark attachment, or whether Cairo/Kantumruy/Baloo 2 GSUB is enough for the actual strings.
- IL2CPP compression ratio (gzip-9 is a proxy).
- The 通用规范 3500-character list as a separate subset (GB2312 level 1 is 3755 and is what was measured).
- M PLUS Rounded OFL as a standalone file from upstream. The name table and `METADATA.pb` say OFL; the Google Fonts directory has no `OFL.txt`.
