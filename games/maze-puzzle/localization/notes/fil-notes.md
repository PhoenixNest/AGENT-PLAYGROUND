# Filipino notes — fil (Filipino)

Standard Filipino, address `ka`. A little Taglish only where it is the everyday word (`Reset`, `offline`, `bonus`, `I-swipe`, `puzzle`, `cookies` in the privacy sentence). No native speaker has reviewed this table. The glossary row is marked native-review-recommended. The 30-key sample is at `/tmp/maze-i18n/tr-fil/review-fil.md` and is not marked done.

Wording pass 2 (after the 2026-10-12 all-language audit) shortened the daily card title, replaced leftover English that is not an everyday loanword, and fixed `dalawang daan` (that reads as 200). The privacy modal was left as it was.

## Coverage

|                         | Count |
| ----------------------- | ----: |
| spec keys               |   355 |
| str keys                |    81 |
| validator errors        |     0 |
| visible warnings        |    21 |
| suppressed fit warnings |    50 |
| accepted identical      |     1 |

Visible warnings by kind: `fit` 6, `glossary` 15. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `fil-fit-exceptions.json` (glossary word Antas). No `stale`, `glyph`, `length`, `expansion`, or `balance` warnings.

`str:unit.kg` and `str:unit.quota` stay `{0} kg` and `{0} / {1} kg`.

## Wording on tight controls

| Item                                  | Choice                                                                | Why                                                                                                                                                                                          |
| ------------------------------------- | --------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Close`                               | `Isara`                                                               | Full verb. Fits the 32px button.                                                                                                                                                             |
| `Cancel`                              | `Itigil`                                                              | Verb for the reset confirm. `Kansela` overruns the box.                                                                                                                                      |
| `← Back`                              | `← Balik`                                                             | Real command. `Bumalik` overruns the box. Arrow stays `←`.                                                                                                                                   |
| `PLAY`                                | `LARO`                                                                | Fits the 42px map chip at full size. `LARUIN` is about 52px (scale about 0.80). `MAGLARO` ellipsizes.                                                                                        |
| `PLAY MISSION`                        | `LARUIN MISYON`                                                       | Verb, and it fits.                                                                                                                                                                           |
| `Try Again` / `Play Again`            | `Ulitin` / `Maglaro`                                                  | Two different buttons.                                                                                                                                                                       |
| `Wear`                                | `Isuot`                                                               | Glossary verb. 30px in a 29px box, under the 40px floor. `Suot` is a state, not the verb.                                                                                                    |
| `Unlock (500`                         | `Buksan (500`                                                         | Same cut-off `(` as English. No added `)`.                                                                                                                                                   |
| `Claim`                               | `Kunin`                                                               |                                                                                                                                                                                              |
| `View`                                | `Basa`                                                                | Opens the user agreement. `Tingnan` exceeds the shrink floor.                                                                                                                                |
| `Change`                              | `Palit`                                                               | Language button. `Palitan` is 1px over.                                                                                                                                                      |
| `Reset`                               | `Reset`                                                               | Everyday UI loanword. `Burahin` sits on the shrink floor of the 30px button. See identical-ok. `Yes, Reset` is `Oo, reset`. `Reset Progress` is `Burahin lahat`.                             |
| `Hangar`                              | `Garahe`                                                              | Defeat button back to base.                                                                                                                                                                  |
| `SHOP`                                | `Bili`                                                                | Fits the 30px dock tab. `Tindahan` ellipsizes that tab.                                                                                                                                      |
| `DAILY`                               | `Araw`                                                                | Dock tab. The screen title is `GAWAING ARAW`.                                                                                                                                                |
| `MAPS`                                | `Mapa`                                                                |                                                                                                                                                                                              |
| `TROPHIES`                            | `Tropeo`                                                              | `Mga tropeo` was the dock label at scale 0.72. `Tropeo` fits the 54px tab and keeps the glossary word.                                                                                       |
| `TOY SHOP`                            | `LARUAN`                                                              | `MGA LARUAN` was the shop header at scale 0.74. `LARUAN` fits. `Tindahan ng laruan` is a fit error on this header. The shop sentence uses the full glossary phrase.                          |
| `DAILY OPERATIONS`                    | `GAWAING ARAW`                                                        | `ARAW-ARAW NA GAWAIN` ellipsized the card to `ARAW-ARAW NA GA…` at the 72% floor. `GAWAING ARAW` is about 112px in a card of about 107px (scale 0.94, no ellipsis) and fits the page header. |
| `Nibbles Hats`                        | `Sumbrero Nibbles`                                                    | `mga sumbrero ni Nibbles` is a fit error (130px, floor 96px). 93px vs a 69px box, under the floor.                                                                                           |
| `Donut Rings`                         | `Singsing donat`                                                      | 81px vs a 65px tab, under the 91px floor (scale about 0.82, no ellipsis). `mga singsing na donat` is a fit error.                                                                            |
| `Skies`                               | `Langit`                                                              | `Kalangitan` is a fit error (60px, floor 42px). 36px vs a 30px box.                                                                                                                          |
| `CLEARED` / `0 / 3 CLEARED`           | `TAPOS`                                                               | `Nalampasan` overruns the pill.                                                                                                                                                              |
| `LOCKED` / `✓ UNLOCKED`               | `SARADO` / `✓ NABUKSAN`                                               | `Bukas` also means tomorrow.                                                                                                                                                                 |
| `Start Shift ▶`                       | `Tara turno ▶`                                                        | `Turno` is the everyday word for a work shift. `shift` is not. 67px in a 69px button.                                                                                                        |
| `SHIFT INCOMPLETE!`                   | `TURNO DI PA TAPOS!`                                                  | Same banner, everyday word. About 188px in a 199px title.                                                                                                                                    |
| `Next Map ▶`                          | `Susunod ▶`                                                           | `Susunod na mapa ▶` passes the shrink floor.                                                                                                                                                 |
| `Choose Sector`                       | `Pili ng sektor`                                                      | `Pumili ng sektor` overruns the box.                                                                                                                                                         |
| `Restart Sector`                      | `Ulitin sektor`                                                       |                                                                                                                                                                                              |
| `Return to Home`                      | `Balik tahanan`                                                       |                                                                                                                                                                                              |
| `Goal: 20 kg`                         | `Layon 20 kg`                                                         | The colon overruns the small chips. The pause sentence keeps a colon.                                                                                                                        |
| `Bronze / Silver / Gold`              | `Tanso / Pilak / Ginto`                                               | Metal colours, not `gintong barya`.                                                                                                                                                          |
| `Bronze Shift • Cadet Sweep`          | `Turno Tanso • Walis Kadete`                                          | About 176px in a 175px title (scale 0.98). Toast drops `Walis`: `Linisin ang Turno Tanso • Kadete!`                                                                                          |
| `Silver Shift • Sector Hazard`        | `Turno Pilak • Sektor Gulo`                                           | `sektor` is required. `Gulo` is the kid-safe hazard.                                                                                                                                         |
| `Gold Shift • Vortex Apex`            | `Turno Ginto • Ipoipo`                                                | Apex does not fit beside `Ipoipo`.                                                                                                                                                           |
| `Master All-Clear Bounty`             | `Punong gantimpala`                                                   | `Puno` is the one word used for master. Fits the 144px label. Toasts use the same phrase.                                                                                                    |
| `Master Sound`                        | `Punong tunog`                                                        | Drops `master`. About 88px in an 87px label (scale 0.98). The kit estimate fits.                                                                                                             |
| `Master Dual-Ring Orbital Gauntlets`  | `Punong kambal-singsing sa orbit`                                     | Same `puno`. Fits the 183px line.                                                                                                                                                            |
| `Launch Backup Pod`                   | `Ekstrang kapsula`                                                    | `Pod` is not an everyday word. `Kapsula` is. 205px vs 182px, under the 253px floor (scale about 0.88). `sirang-bakal` stays. The toast uses the same `ekstrang kapsula`.                     |
| `Cyber Orbit 1`–`10`                  | `Ikot Sayber N`                                                       | `Ikot` is the orbit. `Sayber` is the everyday spelling of cyber. Same length as the English name, so it fits the 71px and 78px chips.                                                        |
| `Cyber Donut Rings`                   | `Singsing Sayber`                                                     | Same `Sayber` as the orbit names.                                                                                                                                                            |
| `LVL 1`–`LVL 50`                      | `Antas N`                                                             | Suppressed. Glossary word.                                                                                                                                                                   |
| `Bubblegum Chute` / `Moon Chute`      | `Dulas Bubblegum` / `Dulas Buwan`                                     | `Padulas` overruns the cards.                                                                                                                                                                |
| `Horizon Sector`                      | `Sektor Horizon`                                                      | `Sektor Abot-tanaw` overruns the cards.                                                                                                                                                      |
| `Singularity Horizon`                 | `Horizon Singular`                                                    | `Singularidad` overruns this title. `The Kessler Singularity` keeps `Kessler Singularidad`.                                                                                                  |
| `Cheese Moon`                         | `Kesong Buwan`                                                        |                                                                                                                                                                                              |
| `100% offline, kid-safe, no trackers` | `100% offline, ligtas, walang bakas`                                  | All three claims fit. `offline` stays.                                                                                                                                                       |
| `FREE DAILY STIPEND`                  | `Araw-araw na baon`                                                   | The phrase fits. Adding `Libreng` overruns the header.                                                                                                                                       |
| `Cute Cosmic Janitor`                 | `Kosmikong tagalinis`                                                 | Adding `Kyut` overruns the header.                                                                                                                                                           |
| `Come back tomorrow for another +100` | `Bukas ulit para sa +100 na`                                          | Read with `pakete!`. The old line was the gift banner at scale 0.76.                                                                                                                         |
| `care package!`                       | `pakete!`                                                             | Read with the previous run: `… +100 na pakete!` `pakete ng alaga` overruns the fragment. The title is `Pakete ng tagalinis!`                                                                 |
| `allowance.`                          | `baon.`                                                               | Read with the previous run: `Kunin ang libreng +100 na baon.`                                                                                                                                |
| `Prize`                               | `Ganti`                                                               | `Premyo` is about 45px in a 34px badge (scale 0.76). `Ganti` fits. It can also mean payback. No shorter prize word fits.                                                                     |
| `Lost In Void`                        | `Sa kawalan`                                                          |                                                                                                                                                                                              |
| `Cosmic Vortex Overdrive`             | `Ipoipong kosmiko: siksik na basura at dalawang hiwa!`                | `dalawang daang pumuputol` reads as two hundred cutters. `Hiwa` is the shred word used on the gates. The old line was about 415px in a 320px blurb (scale 0.76).                             |
| `Reset confirm body`                  | `Mabubura ang mapa, kasuotan, at gintong barya. Hindi na maibabalik!` | The old sentence ellipsized. This is about 423px in a 366px line (scale 0.86, no ellipsis). The title already asks `BURAHIN ANG LAHAT?`. `gintong barya` stays.                              |
| `Kessler Hero`                        | `Bayani Kessler`                                                      | 95px vs 81px, under the 112px floor. Dropping either word loses the name.                                                                                                                    |
| Privacy modal                         | translated, not polished                                              | Out of the app flow. Left unchanged in pass 2. `localStorage` and `cookies` stay. `Orbital Salvage` stays.                                                                                   |

Backup Pod and Auxiliary Scrap Pod are both `ekstrang kapsula`. The `+8 Scrap` on those lines is `sirang-bakal`. Master sound, the all-clear bounty, and the dual-ring gauntlets all use `puno`. Every daily job uses `turno`.

## Accepted fit and glossary warnings

No shorter natural form keeps the meaning, or the glossary string is the English word the audit asked to replace. Fit warnings below are under the 72% shrink floor.

| Keys                                              |   n | Decision                                                                                                                                                        |
| ------------------------------------------------- | --: | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `LVL 1`–`LVL 50`                                  |  50 | Suppressed. Glossary word Antas.                                                                                                                                |
| `Wear`                                            |   1 | `Isuot`, 30px vs 29px. Glossary verb. Under the 40px floor.                                                                                                     |
| `Skies`                                           |   1 | `Langit`, 36px vs 30px. `Kalangitan` is past the shrink floor.                                                                                                  |
| `Nibbles Hats`                                    |   1 | Fit: `Sumbrero Nibbles`, 93px vs 69px, under the 96px floor. Glossary: the full phrase is a fit error.                                                          |
| `Kessler Hero`                                    |   1 | `Bayani Kessler`, 95px vs 81px, under the 112px floor.                                                                                                          |
| `Donut Rings`                                     |   1 | `Singsing donat`, 81px vs 65px, under the 91px floor. The full phrase is a fit error.                                                                           |
| `Launch Backup Pod (+8 Scrap!)`                   |   1 | Fit: `Ekstrang kapsula (+8 sirang-bakal)`, 205px vs 182px, under the 253px floor. Glossary: `pod` is not everyday Filipino; `kapsula` is. `sirang-bakal` stays. |
| `str:toast.backup.deployed`                       |   1 | Glossary, same `kapsula` decision as the button.                                                                                                                |
| `DAILY OPERATIONS`                                |   1 | Glossary. `araw-araw na gawain` ellipsizes the card. `GAWAING ARAW` fits.                                                                                       |
| `TOY SHOP`                                        |   1 | Glossary. Header is `LARUAN`. The sentence uses `tindahan ng laruan`.                                                                                           |
| `All 3 shifts completed! Claim your reward!`      |   1 | Glossary. `Turno` replaces `shift` on every daily job.                                                                                                          |
| `Bronze Shift • Cadet Sweep`                      |   1 | Glossary. Same `turno`.                                                                                                                                         |
| `Clean up Bronze Shift • Cadet Sweep!`            |   1 | Glossary. Same `turno`.                                                                                                                                         |
| `Complete all 3 daily shifts to unlock +300`      |   1 | Glossary. Same `turno`.                                                                                                                                         |
| `Complete today's wacky Daily Shift cleanup job!` |   1 | Glossary. Same `turno`.                                                                                                                                         |
| `Gold Shift • Vortex Apex`                        |   1 | Glossary. Same `turno`.                                                                                                                                         |
| `SHIFT INCOMPLETE!`                               |   1 | Glossary. Same `turno`.                                                                                                                                         |
| `Silver Shift • Sector Hazard`                    |   1 | Glossary. Same `turno`.                                                                                                                                         |
| `Start Shift ▶`                                   |   1 | Glossary. Same `turno`.                                                                                                                                         |
| `str:toast.daily.all_clear`                       |   1 | Glossary. Same `turno`, and `punong gantimpala` for the master bonus.                                                                                           |

## Least-sure keys after pass 2

1. `Prize` → `Ganti` (`Premyo` shrinks the 34px badge to about 0.76; `ganti` can also mean payback)
2. `PLAY` → `LARO` (`LARUIN` is on the edge of the 42px chip; `MAGLARO` ellipsizes)
3. `SHOP` → `Bili` (`Tindahan` ellipsizes the dock tab)
4. `Reset` → `Reset` (loanword; confirm is `Oo, reset`)
5. `Donut Rings` → `Singsing donat` (fit warning, under the floor)
6. `Nibbles Hats` → `Sumbrero Nibbles` (glossary still wants `mga sumbrero ni Nibbles`)
7. `TOY SHOP` → `LARUAN` (glossary still wants `tindahan ng laruan`)
8. `Skies` → `Langit`
9. `Singularity Horizon` → `Horizon Singular` (`Kessler Singularidad` keeps the long word)
10. `Kessler Hero` → `Bayani Kessler`
11. `Launch Backup Pod` → `Ekstrang kapsula` (fit and glossary warnings, under the floor)
12. `Cyber Orbit 1`–`10` → `Ikot Sayber N` (`Sayber` is the spelling chosen here; a native speaker may prefer `Cyber`)

## Glossary additions requested

- A short daily-operations header. `araw-araw na gawain` ellipsizes the card. This table uses `Gawaing araw` there.
- A Filipino word for shift. The glossary cell is the English word `shift`. This table uses `turno` on every daily job.
- A Filipino word for the backup pod. The glossary cell is `ekstrang pod`. This table uses `ekstrang kapsula` on the button and the toast.
- A short Toy Shop header. `tindahan ng laruan` cannot fit `TOY SHOP` (fit error). This table uses `Laruan` there and the full phrase in the sentence.
- A short Nibbles Hats tab. `mga sumbrero ni Nibbles` cannot fit (fit error). This table uses `Sumbrero Nibbles`.
- A short skies word. `kalangitan` cannot fit (fit error). This table uses `Langit`.
- A short Donut Rings tab. `mga singsing na donat` cannot fit. This table uses `Singsing donat`.
- A short cleared word for the pill. `nalampasan` overruns `0 / 3 CLEARED`. This table uses `Tapos`.
- A short chute on level cards. `padulas` overruns the cards. This table uses `Dulas`.
- A dock word for Shop of about four letters. `Tindahan` passes the shrink floor. This table uses `Bili`.
- A prize word that fits the 34px badge. `premyo` shrinks it to about 0.76. This table uses `Ganti`.

`orbit` stays inside `Ikot` only where the chip cannot hold the English loanword. `sirang-bakal`, `gintong barya`, `antas`, `sektor`, `mundo`, `tropeo`, `araw-araw na baon`, `kosmikong tagalinis`, `itim na butas`, and `basura sa kalawakan` are used wherever the line can hold them.

## Kit notes

- Fragment keys `Unlock (1200` and `Trophy: Daily Duty Star (+500` are still cut off in English. Filipino does not add the missing `)`.
- `str` rows still have no line box, so toast and hud width are not fit-checked. Dock badges were kept inside the `max_chars` notes (`REGALO` 6, `LIBRE` 5, `{0} BAGO` 8).
- A `check: yes` phrase that cannot fit the header (`Toy Shop`, `Nibbles Hats`, `daily operations`) becomes a glossary warning on the short form. That is expected. The fit error cannot be suppressed.
- The shift glossary cell is the English word. Replacing it with `turno` is a glossary warning on every daily-job key. That is expected.
- The LVL alias requires `Antas` on chips narrower than the word. Those 50 fit warnings are suppressed, not fixed.
- `settings_modal_privacy` is out of the app flow. Pass 2 did not edit it. `cookies` stays as the web word. `localStorage` stays.
- Identical loanwords are listed in `fil-identical-ok.json`. They are not added to `doNotTranslate`. Pass 2 keeps only `Reset`.

toast.infinite.round: agent draft, no native review
home.infinite.world/title/best: agent draft, no native review
