# Arabic, Hindi, and Khmer layout audit — 2026-10-13

Editor stills only (Unity 6000.4.0f1, macOS, Metal). No device. The three real tables are baked. Hard layout findings are 0. I read the stills below; I do not read Arabic, Hindi, or Khmer, so spelling is taken from the table and the picker marker, and the pixels are what I can actually see.

Full stills: `/tmp/maze-i18n/audit-all/<code>/stills/`. Overflow text: `/tmp/maze-i18n/audit-all/<code>/overflow.txt`. Eight crops sit next to this file.

## Counts

`LocLanguageAuditTests` and `LocLanguageStillTests` ran for `ar`, `hi`, and `km` (90 spec files each, zero and iPhone). All six tests passed.

| language | hard | soft shrink | soft ellipsis | left English (spec+str identical) |
| --- | ---: | ---: | ---: | --- |
| ar | 0 | 4 | 4 | brand, kg lines, gate emoji templates |
| hi | 0 | 4 | 4 | same set |
| km | 0 | 34 | 4 | same set |

Hard is outside, overlap, band, wider, wrapped-in-control, missing-glyph, and icon-overlap. All of those are 0. The 4 ellipses in each language are the two privacy bodies, on zero and on iPhone. That is the same pair every Latin table already ellipsizes. The 0.72 shrinks are on `settings_modal_privacy` and the audit could not tie them to one run (the version line `Orbital Salvage • Space Trash Clean v1.2.0` is still English). Khmer's extra shrinks are 0.78 on the daily medal chips and the reset line, ten pages, and they still fit.

## What the stills show

The wordmark `ORBITAL` / `SALVAGE` / `SPACE TRASH CLEAN` stays English on Home in all three. That string is identical in the tables.

Arabic Home keeps the English page order. The world chip stays on the left, the goal pill stays on the right, the play ▶ stays on the left of the green button, and the close and pause icons stay where English put them. The goal pill reads with `20` and `kg` in the Latin face and the Arabic word on the right of that pill. The play label sits to the right of the triangle. Dock labels sit under the four icons and are not blank. The language row draws `العربية` in Cairo with the check on the right of the word (`✓ العربية` in logical order). Joins on that word are connected. I did not see a gap inside the word.

Hindi Home draws the world chip, the goal pill (`20 kg`), the level line, the play label, and the dock in Baloo 2. The level line is one headline; the headstroke looks continuous at this size, and the matras are inside the chips rather than cut off by the pill edge. The Hindi picker row is `✓ हिन्दी` with the check on the left. The i-matra is visible above the line.

Khmer Home and Daily draw Kantumruy, not boxes. The goal pill keeps `20 kg`. The play ▶ stays on the left. Daily shows a countdown with Western `14h 59m`, three medal words, and a two-line task. Subscript and vowel marks are visible on the title and the body; I did not see a coeng sliced off by the pill, or a line colliding with the next. The picker row is `✓` then the Khmer endonym, check on the left. At 14px the clusters are small. I would not call the Khmer print-sharp, and I cannot vouch for the spelling from the pixels. The marker file records the drawn string as `✓ ភាសាខ្មែរ`.

Pause (Arabic): the title is centred, the pause icon stays on the left, the close button stays on the right.

## Too wide, not edited

These fit after shrink or ellipsis. Hard wider is 0. I did not change the tables.

| language | where | English | translation | what the fitter did |
| --- | --- | --- | --- | --- |
| ar, hi, km | privacy body | Orbital Salvage is built with strict privacy-by-design. … | the long privacy paragraph in each table | ellipsis, same as the Latin tables |
| ar, hi, km | privacy body | All game progression, gold coins, and cosmetic unlocks… | the localStorage paragraph | ellipsis |
| km | daily medals and reset | `BRONZE` / `SILVER` / `GOLD`, `Resets in 14h 59m` | `សំរឹទ្ធ` / `ប្រាក់` / `មាស`, `ឡើងវិញ 14h 59m` | scale 0.78, still inside the chip |

If the privacy paragraphs must show in full, they need a shorter translation or a taller card. The card-grow path is still English-only.

## Bake and dynamic text

Every spec and str key baked. A placeholder may sit next to punctuation, a Western digit, or a Latin token (`+{0}`, `{0}h`, `({1}/{0}`). It still fails if it is glued to an Arabic, Devanagari, or Khmer letter. `…` and `⏪` are symbol runs, so a script face that lacks them does not paint `.notdef`. No key failed.

`toast.trophy` and `toast.sector.cleanup` insert a translated name. The name is a table value, so the draw splices that value's baked runs into the template. A Western number in any other `{0}` stays on Fredoka. A script string that is not a table value and is not one of those slots still paints nothing.

Gate `zap` / `shred` / `magnet` have no script letters in these tables (`⚡ ×{0}` and the same shape), so they stay on the Latin and symbol path. Boost, the hub subtitle, NOM words, and the floating shred/compact lines do contain script and use Cairo, Baloo 2, or Kantumruy. `GameplayWords_UseTheShapedFace` checks that.
