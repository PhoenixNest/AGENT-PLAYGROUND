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

I read Home, Settings (the page and the language picker), Shop, Daily, Maps, Trophies, Pause, Defeat, and Win for `ar`, `hi`, and `km`. I do not read these scripts. Spelling below is from the table or the picker marker. Where I only describe the picture, I am not claiming the letters spell the table.

The wordmark stays English. Page chrome is not mirrored: the back arrow stays on the left and still points left, the gear stays left of the settings title, the close × stays on the right of Pause, and the play ▶ stays on the left of the green Home button and on the left of the yellow resume button.

Arabic text is joined. `العربية` on the marked picker is one connected word, and the check sits on the right (`✓ العربية` in the marker). Home, Settings, Shop, Maps, Trophies, Pause, Defeat, and Win all draw Cairo rather than boxes. Mixed lines keep Western digits and `kg` / `Nibbles` in the Latin face. The goal pill puts the Arabic word on the right and `20 kg` toward the left of that pill. Settings rows are right-aligned: the speaker icon stays on the left, the yellow section title sits on the right, and the toggle and sliders stay on the right. Shop tabs, the gift button, and the skybox card are filled. Maps level cards keep the play triangle on the right of the card and the stage chip on the left. Trophy titles and the green unlocked mark are on the card; the mark is on the right. Defeat and Win banners are centred in the coloured strip, with the emoji on the left of the Arabic words. I did not see a letter colliding with an icon.

Hindi uses Baloo 2. The Home level line is one headline and the headstroke looks continuous inside the card. Matras on the dock labels and the play button sit inside the chips; I did not see one sliced off at the top. The marked picker draws `✓ हिन्दी` with the check on the left, and the same picker also draws `ភាសាខ្មែរ` and `العربية` (marker files: `✓ ភាសាខ្មែរ`, `العربية` without a check on that row). Settings, Shop, Daily, Maps, Trophies, Pause, Defeat, and Win are filled. Daily shows `14h 59m` and `0 / 3` in Latin order. Stat chips read `20 kg` and `12 / 20 kg`.

Khmer uses Kantumruy. Home, Daily, Maps, and the picker are not tofu. The marked picker is `✓` then the Khmer endonym, check on the left. The marker records `✓ ភាសាខ្មែរ`. Daily keeps `14h 59m` and `0 / 3`, three short medal words, and a two-line task. Subscript clusters are visible under the title and the body; I did not see a coeng cut off by the pill or a line sitting on the next line. At this size the clusters are small, and I would not call them print-sharp. Defeat and Win banners and the stat chips (`20 kg`, `12 / 20 kg`, `4 kg`) are drawn. Pause buttons are filled, with the icons on the left of the label.

One Arabic direction bug was in those stills. `unit.quota` is the English string `{0} / {1} kg`, and the win total is `{0} kg`. Both were baked as Arabic paragraphs, so the chips painted `kg 20 / 12` and `kg 20`. Hindi and Khmer painted `12 / 20 kg` and `20 kg`. A string with no Arabic letter is now a left-to-right paragraph. The stills above were read before that rebake. `Placeholder_SplicesAWesternNumber` resolves `12 / 20 kg` and checks the first visual piece starts with `12`. A mixed line that does contain Arabic, such as the goal pill, stays right-to-left.

## Too wide, not edited

These fit after shrink or ellipsis. Hard wider is 0. I did not change the tables.

The audit records a scale, not a pixel width. 0.72 means the unscaled line was about 1.4 times the box. 0.78 is about 1.3 times. Hard wider is 0 after that shrink.

| key | language | English | translation | measured | suggestion |
| --- | --- | --- | --- | --- | --- |
| spec privacy-by-design paragraph | ar, hi, km | Orbital Salvage is built with strict privacy-by-design. … | each table's privacy paragraph | ellipsis, same two pages as every Latin table | shorter paragraph, or a taller card |
| spec localStorage paragraph | ar, hi, km | All game progression, gold coins, and cosmetic unlocks… | each table's localStorage paragraph | ellipsis | same |
| `BRONZE` / `SILVER` / `GOLD` | km | BRONZE / SILVER / GOLD | សំរឹទ្ធ / ប្រាក់ / មាស | scale 0.78, still inside the chip | shorter medal names if the chip must stay at the English size |
| `daily.countdown` | km | Resets in {0}h {1}m | ឡើងវិញ {0}h {1}m | scale 0.78, still inside the chip | a shorter lead-in |

If the privacy paragraphs must show in full, they need a shorter translation or a taller card. The card-grow path is still English-only.

## Bake and dynamic text

Every spec and str key baked. A placeholder may sit next to punctuation, a Western digit, or a Latin token (`+{0}`, `{0}h`, `({1}/{0}`). It still fails if it is glued to an Arabic, Devanagari, or Khmer letter. `…` and `⏪` are symbol runs, so a script face that lacks them does not paint `.notdef`. No key failed.

`toast.trophy` and `toast.sector.cleanup` insert a translated name. The name is a table value, so the draw splices that value's baked runs into the template. A Western number in any other `{0}` stays on Fredoka. A script string that is not a table value and is not one of those slots still paints nothing.

Gate `zap` / `shred` / `magnet` have no script letters in these tables (`⚡ ×{0}` and the same shape), so they stay on the Latin and symbol path. Boost, the hub subtitle, NOM words, and the floating shred/compact lines do contain script and use Cairo, Baloo 2, or Kantumruy. `GameplayWords_UseTheShapedFace` checks that.
