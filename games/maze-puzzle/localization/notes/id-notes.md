# Indonesian notes — id (Bahasa Indonesia)

Draft, 2026-10-12. Informal-polite `kamu` (not Anda, not kau). Warm, short, child-safe. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-id/review-id.md` and is not marked done.

Wording pass 2 (after the all-language audit) shortens the two controls that painted with an ellipsis, replaces leftover English where a short Indonesian word fits, and keeps one word per concept. Widths below are the kit estimate against `max(captured run, English estimate)`. The audit font is wider than that estimate: `PERUNGGU` and `TOKO MAINAN` were under the 72% floor in the kit and still painted as `PERUNG…` and `TOKO MAIN…`. A replacement that is the same width as the English control, or shorter, is the one that can sit at full size.

## Coverage

|                         | Count |
| ----------------------- | ----: |
| spec keys               |   355 |
| str keys                |    81 |
| validator errors        |     0 |
| visible warnings        |    25 |
| suppressed fit warnings |    50 |
| accepted identical      |     0 |

Visible warnings by kind: `fit` 21, `glossary` 3, `length` 1. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `id-fit-exceptions.json` (glossary word `level`). No `stale`, `glyph`, `expansion`, `placeholder`, `emoji`, or `number` warnings.

`id-identical-ok.json` is an empty list. `Reset` was the only waiver, and the button is now `Hapus`.

`str:unit.kg` and `str:unit.quota` stay `{0} kg` and `{0} / {1} kg`. Brand lines stay English: `ORBITAL`, `SALVAGE`, `SPACE TRASH CLEAN`, `ORBITAL SALVAGE`, and `Orbital Salvage • Space Trash Clean v1.2.0`. `Nibbles` is never translated. `localStorage` stays in the privacy paragraph. `Level` stays inside translated names (`Level 1`, `Level 1 • Luncur Permen 1`). That is the glossary loanword, not an identical copy of `LVL N`.

## Pass 2 changes

| English                         | Before                               | After                                                                | Measured fit                                                                          |
| ------------------------------- | ------------------------------------ | -------------------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| `TOY SHOP`                      | `TOKO MAINAN`                        | `MAINAN`                                                             | 51px vs 84px, scale 1.00. Was 90px and painted `TOKO MAIN…`.                          |
| `BRONZE`                        | `PERUNGGU`                           | `BRONS`                                                              | 28px vs 47px, scale 1.00. Shorter than `BRONZE` (33px). `PERUNGGU` painted `PERUNG…`. |
| `BRONZE ✓`                      | `PERUNGGU ✓`                         | `BRONS ✓`                                                            | 38px vs 58px, scale 1.00. Shorter than `BRONZE ✓`.                                    |
| `DAILY`                         | `Harian`                             | `Tugas`                                                              | 28px vs 32px, same width as `DAILY`. Dock was scale 0.76.                             |
| `Reset`                         | `Reset`                              | `Hapus`                                                              | 30px vs 30px, same width as `Reset`.                                                  |
| `Yes, Reset`                    | `Ya, reset`                          | `Ya, hapus`                                                          | 49px vs 55px, scale 1.00.                                                             |
| reset warning                   | long `dikembalikan` line             | `Kamu yakin? Hapus peta, kostum, dan koin emas. Tidak bisa kembali!` | 389px. The old line (507px) painted with an ellipsis.                                 |
| `Launch Backup Pod (+8 Scrap!)` | `Mulai pod cadangan (+8 rongsokan)!` | `Kapsul cadangan (+8 rongsokan)!`                                    | 186px vs 182px, scale 0.98. The longer old line painted in full on the wide button.   |
| `Master All-Clear Bounty`       | `Hadiah Master Tuntas`               | `Hadiah juara`                                                       | 75px vs 145px, scale 1.00.                                                            |
| `Lost In Void`                  | `Di hampa`                           | `Hilang`                                                             | 33px vs 67px, scale 1.00. `Hilang di hampa` is 78px, scale 0.86, and would shrink.    |
| `Resets in 14h 59m`             | `Ulang 14 jam 59m`                   | `Ulang 14j 59mnt`                                                    | 74px vs 82px, scale 1.00. The old line sat in the 0.72 daily bundle.                  |
| `Hangar`                        | `Beranda`                            | `Pulang`                                                             | 36px vs 43px, same width as `Hangar`. Pause already says `Pulang`.                    |

`BRONS` is the short loan children and parents use for the bronze medal. `PERUNGGU` clips the chip. `GANGSA` is the same length as `BRONZE` but in modern Indonesian it is an obscure gamelan-alloy word, so it was dropped. Shift titles use `brons` too. `PERAK` and `EMAS` were already short enough to paint.

The shop sentence in the privacy modal still says `toko mainan`. That modal was not edited. The header cannot hold the full glossary phrase.

`Level` is kept on purpose. The countdown template is `Ulang {0}j {1}mnt`. `j` is jam and `mnt` is menit. `Menit` in the spec line crosses the shrink floor.

## Left as they were

| Key                   | Choice      | Why                                                                                                                                      |
| --------------------- | ----------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `Sound: ON`           | `Ada suara` | 51px vs 59px, scale 1.00. The pause still reads it. `Suara nyala` is 63px, scale 0.92, and near-budget strings ellipsized in this audit. |
| `✓ UNLOCKED`          | `✓ DIRAIH`  | 51px vs 81px. The trophy still shows it in full next to `TERKUNCI`. It means achieved. `TERBUKA` can read as still open.                 |
| `LVL N` / chute names | `Level N`   | Glossary loanword. Recorded here, not in identical-ok.                                                                                   |
| privacy modal         | unchanged   | Out of scope. Those bodies still ellipsize.                                                                                              |

## Fit warnings left, with a reason

Every fit warning is under the 72% shrink floor. The stills that were read show these strings in full except where the note says otherwise.

| Keys                            |   n | Choice                            | Why it stays                                                                                                                                                |
| ------------------------------- | --: | --------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `LVL 1`–`LVL 50`                |  50 | `Level N`                         | Suppressed. Glossary word `level`. The chip is narrower than the word.                                                                                      |
| `Moon Chute 1`–`10`             |  10 | `Luncur Bulan N`                  | About 4–6px over. `Perosotan` nearly hits the shrink floor. `Luncur` is the short slide word for every chute. The map still shows the candy chutes in full. |
| `FREE`                          |   1 | `GRATIS`                          | 27px vs 23px, floor 33px. The dock badge paints `GRATIS` in full. No shorter word that means free-of-charge.                                                |
| `Start Shift ▶`                 |   1 | `Mulai giliran ▶`                 | 85px vs 73px. Verb plus glossary `giliran`. The daily button paints the whole label.                                                                        |
| `TODAY`                         |   1 | `HARI INI`                        | Fit 42px vs 38px, and length exactly 1.60×. The card badge paints it in full. `Kini` means now, not today.                                                  |
| `Wear`                          |   1 | `Pakai`                           | 30px vs 29px. Glossary verb.                                                                                                                                |
| `Cute Cosmic Janitor`           |   1 | `Petugas kebersihan kosmik`       | 157px vs 148px, floor 205px. Glossary job title.                                                                                                            |
| `Donut Rings`                   |   1 | `Cincin Donat`                    | 69px vs 65px. The shop tab paints it in full.                                                                                                               |
| `Prize`                         |   1 | `Hadiah`                          | 39px vs 34px, floor 47px.                                                                                                                                   |
| `Skies`                         |   1 | `Langit`                          | 36px vs 30px, floor 42px. The shop tab paints it in full.                                                                                                   |
| `Launch Backup Pod (+8 Scrap!)` |   1 | `Kapsul cadangan (+8 rongsokan)!` | 186px vs 182px. Shorter than the line the defeat still painted in full. Dropping `cadangan` or `rongsokan` would clear the warning and lose the concept.    |
| `allowance.`                    |   1 | `uang jajan.`                     | 56px vs 55px. Glossary fragment.                                                                                                                            |
| `care package!`                 |   1 | `paket bingkisan!`                | 84px vs 72px, floor 100px. The shop banner paints it in full.                                                                                               |

## Glossary warnings left, with a reason

| Key                             | Expected       | Used              | Why                                                                                                      |
| ------------------------------- | -------------- | ----------------- | -------------------------------------------------------------------------------------------------------- |
| `TOY SHOP`                      | `toko mainan`  | `MAINAN`          | The full phrase paints as `TOKO MAIN…`. The privacy sentence still has `toko mainan` and was not edited. |
| `Launch Backup Pod (+8 Scrap!)` | `pod cadangan` | `kapsul cadangan` | Drops the English word `pod`. The button is one word with the toast.                                     |
| `toast.backup.deployed`         | `pod cadangan` | `kapsul cadangan` | Same object as the button. Toasts have no line box.                                                      |

## Glossary words that changed shape

| Term             | Glossary                  | Used                 | Where                                                                                         |
| ---------------- | ------------------------- | -------------------- | --------------------------------------------------------------------------------------------- |
| chute            | perosotan                 | luncur               | Every chute name.                                                                             |
| goal (no row)    | —                         | misi                 | `Goal: 20 kg` and the pause quota.                                                            |
| hangar (no row)  | —                         | pulang               | Both home controls. Pause already said `Pulang`.                                              |
| pinball (no row) | —                         | pantul               | `Nebula Pantul`.                                                                              |
| toy shop         | toko mainan               | mainan on the header | Header only. See the glossary warning.                                                        |
| backup pod       | pod cadangan              | kapsul cadangan      | Button and toast.                                                                             |
| master bounty    | hadiah master             | hadiah juara         | Title and both daily toasts.                                                                  |
| bronze           | perunggu                  | brons                | Medal, badge, and the bronze shift. `Gangsa` is the gamelan alloy, not the word families use. |
| cosmic janitor   | petugas kebersihan kosmik | same, without `lucu` | Tagline only.                                                                                 |

`space junk` and `space trash` both use `sampah luar angkasa`. Recycling coins and gameplay coins use `koin emas`. Gold shift metal is `emas`, not `koin emas`. The settings name for the wipe is `Atur ulang` (row and `ATUR ULANG SEMUA?`). The short red button is `Hapus`, because `Atur ulang` does not fit that 30px control and `Ulang` means play again.

## Doubtful lines

| Key                            | Choice                   | Alternative                                                         |
| ------------------------------ | ------------------------ | ------------------------------------------------------------------- |
| `BRONZE`                       | `BRONS`                  | `Perunggu` clips the medal. `Gangsa` is obscure.                    |
| `Reset`                        | `Hapus`                  | The row stays `Atur ulang`. Two surfaces, one wipe.                 |
| `Lost In Void`                 | `Hilang`                 | `Hilang di hampa` names the void and does not fit the run.          |
| `Sound: ON`                    | `Ada suara`              | `Suara nyala` is clearer and wider than the pause run.              |
| `Resets in 14h 59m`            | `Ulang 14j 59mnt`        | `j` / `mnt` are the short clock marks.                              |
| `✓ UNLOCKED`                   | `✓ DIRAIH`               | Kept. The still reads it as achieved.                               |
| `← Back`                       | `← Balik`                | `← Kembali` overflows (56px vs 44px).                               |
| `Snacks Eaten`                 | `Camilan habis`          | `Camilan dimakan` is 1px over the line.                             |
| `Coins Won`                    | `Koin dapat`             | `Koin emas` names the currency and drops "won".                     |
| `Game Data`                    | `Data main`              | `Data permainan` overflows.                                         |
| `Sleepy Star Cloud`            | `Awan Mengantuk`         | Star and sleepy do not fit together.                                |
| `Silver Shift • Sector Hazard` | `Giliran perak • sektor` | `Bahaya` does not fit with both words.                              |
| Privacy `cookies`              | `kuki`                   | Homonym of the snack. Left from pass 1. The modal was not reworked. |

## Twelve least-sure keys

1. `BRONZE` → `BRONS` (short loan; `perunggu` clips, `gangsa` is obscure)
2. `Reset` → `Hapus` (row is still `Atur ulang`)
3. `Lost In Void` → `Hilang`
4. `Sound: ON` → `Ada suara`
5. `Resets in 14h 59m` → `Ulang 14j 59mnt`
6. `LVL 1` → `Level 1` (loanword, kept)
7. `Moon Chute 1` → `Luncur Bulan 1`
8. `Goal: 20 kg` → `Misi: 20 kg`
9. `✓ UNLOCKED` → `✓ DIRAIH`
10. `Silver Shift • Sector Hazard` → `Giliran perak • sektor`
11. `Nebula Pinball` → `Nebula Pantul`
12. `Sleepy Star Cloud` → `Awan Mengantuk`

## Glossary additions requested

- Bronze medal: `brons` when `perunggu` clips. `Gangsa` is a gamelan-alloy word, not the everyday metal. One word for the medal, the badge, and the bronze shift.
- Toy-shop header: `mainan` when `toko mainan` clips. The long phrase does not fit that title.
- Backup pod: `kapsul cadangan` instead of `pod cadangan`, so the English word is gone.
- Master bounty: `hadiah juara` instead of `hadiah master`.
- Chute chip: `luncur` when `perosotan` does not fit.
- No rows yet for goal, hangar, or pinball. This table uses `misi`, `pulang`, and `pantul`.

## English source

- `Unlock (1200` (and the other prices) and `Trophy: Daily Duty Star (+500` are cut off before the closing `)`. Indonesian does not add it.
- `Backup Pod` and `Auxiliary Scrap Pod` are one object. Both say `kapsul cadangan`.
- `Hangar` is the defeat home button. It now says `Pulang`, the same word as pause.
- Privacy headings and bodies still share a captured y. Left as a layout issue. The modal is out of the app flow and was not edited in pass 2.

## Kit

- `str` rows still have no line box, so toasts are not fit-checked.
- A length of exactly 1.60× warns. `HARI INI` is 8 UTF-16 units against `TODAY` at 5.
- The fit estimate missed the two real control ellipses on pass 1. Pass 2 treats "same width as the English control, or shorter" as the bar for a clipped control.

toast.infinite.round: agent draft, no native review
