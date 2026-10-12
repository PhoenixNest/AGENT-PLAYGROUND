# Filipino notes — fil (Filipino)

Standard Filipino, address `ka`. A little Taglish only where it is the everyday word (`shift`, `orbit`, `Reset`, `offline`, `bonus`, `I-swipe`, `puzzle`, `cookies` in the privacy sentence). No native speaker has reviewed this table. The glossary row is marked native-review-recommended. The 30-key sample is at `/tmp/maze-i18n/tr-fil/review-fil.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 6 |
| suppressed fit warnings | 50 |
| accepted identical | 11 |

Visible warnings by kind: `fit` 4, `glossary` 2. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `fil-fit-exceptions.json` (glossary word Antas). No `stale`, `glyph`, `length`, `expansion`, or `balance` warnings.

`str:unit.kg` and `str:unit.quota` stay `{0} kg` and `{0} / {1} kg`.

## Wording on tight controls

| Item | Choice | Why |
| --- | --- | --- |
| `Close` | `Isara` | Full verb. Fits the 32px button. |
| `Cancel` | `Itigil` | Verb for the reset confirm. `Kansela` overruns the box. |
| `← Back` | `← Balik` | Real command. `Bumalik` overruns the box. Arrow stays `←`. |
| `PLAY` | `LARO` | Everyday play-button word. `MAGLARO` overruns the 42px line. |
| `PLAY MISSION` | `LARUIN MISYON` | Verb, and it fits. |
| `Try Again` / `Play Again` | `Ulitin` / `Maglaro` | Two different buttons. |
| `Wear` | `Isuot` | Glossary verb. 30px in a 29px box, under the 40px floor. `Suot` is a state, not the verb. |
| `Unlock (500` | `Buksan (500` | Same cut-off `(` as English. No added `)`. |
| `Claim` | `Kunin` | |
| `View` | `Basa` | Opens the user agreement. `Tingnan` exceeds the shrink floor. |
| `Change` | `Palit` | Language button. `Palitan` is 1px over. |
| `Reset` | `Reset` | Everyday UI loanword. `Burahin` sits on the shrink floor of the 30px button. See identical-ok. `Yes, Reset` is `Oo, reset`. `Reset Progress` is `Burahin lahat`. |
| `Hangar` | `Garahe` | Defeat button back to base. |
| `SHOP` | `Bili` | `Tindahan` passes the shrink floor of the 30px tab. |
| `DAILY` | `Araw` | `Araw-araw` passes the shrink floor. The screen title keeps `ARAW-ARAW NA GAWAIN`. |
| `MAPS` | `Mapa` | |
| `TOY SHOP` | `MGA LARUAN` | `Tindahan ng laruan` is a fit error (145px, floor 116px). The shop sentence uses the full glossary phrase. |
| `Nibbles Hats` | `Sumbrero Nibbles` | `mga sumbrero ni Nibbles` is a fit error (130px, floor 96px). 93px vs a 69px box, under the floor. |
| `Donut Rings` | `Singsing` | `mga singsing na donat` is a fit error. `Singsing donat` still overruns the 65px tab. |
| `Skies` | `Langit` | `Kalangitan` is a fit error (60px, floor 42px). 36px vs a 30px box. |
| `CLEARED` / `0 / 3 CLEARED` | `TAPOS` | `Nalampasan` overruns the pill. |
| `LOCKED` / `✓ UNLOCKED` | `SARADO` / `✓ NABUKSAN` | `Bukas` also means tomorrow. |
| `Start Shift ▶` | `Tara shift ▶` | `Simulan ang shift ▶` passes the shrink floor. `shift` stays, as the glossary requires. |
| `Next Map ▶` | `Susunod ▶` | `Susunod na mapa ▶` passes the shrink floor. |
| `Choose Sector` | `Pili ng sektor` | `Pumili ng sektor` overruns the box. |
| `Restart Sector` | `Ulitin sektor` | |
| `Return to Home` | `Balik tahanan` | |
| `Goal: 20 kg` | `Layon 20 kg` | The colon overruns the small chips. The pause sentence keeps a colon. |
| `Bronze / Silver / Gold` | `Tanso / Pilak / Ginto` | Metal colours, not `gintong barya`. |
| `Bronze Shift • Cadet Sweep` | `Shift Tanso • Walis Kadete` | Toast drops `Walis` so the line fits: `Linisin ang Shift Tanso • Kadete!` |
| `Silver Shift • Sector Hazard` | `Shift Pilak • Sektor Gulo` | `sektor` is required. `Gulo` is the kid-safe hazard. |
| `Gold Shift • Vortex Apex` | `Shift Ginto • Ipoipo` | Apex does not fit beside `Ipoipo`. |
| `LVL 1`–`LVL 50` | `Antas N` | Suppressed. Glossary word. |
| `Cyber Orbit 1`–`10` | copied | `cyber` and glossary `orbit` are already the right words. Identical-ok. |
| `Bubblegum Chute` / `Moon Chute` | `Dulas Bubblegum` / `Dulas Buwan` | `Padulas` overruns the cards. |
| `Horizon Sector` | `Sektor Horizon` | `Sektor Abot-tanaw` overruns the cards. |
| `Singularity Horizon` | `Horizon Singular` | `Singularidad` overruns this title. `The Kessler Singularity` keeps `Kessler Singularidad`. |
| `Cheese Moon` | `Kesong Buwan` | |
| `100% offline, kid-safe, no trackers` | `100% offline, ligtas, walang bakas` | All three claims fit. `offline` stays. |
| `FREE DAILY STIPEND` | `Araw-araw na baon` | The phrase fits. Adding `Libreng` overruns the header. |
| `Cute Cosmic Janitor` | `Kosmikong tagalinis` | Adding `Kyut` overruns the header. |
| `care package!` | `pakete!` | Read with the previous run: `… +100 na pakete!` `pakete ng alaga` overruns the fragment. The title is `Pakete ng tagalinis!` |
| `allowance.` | `baon.` | Read with the previous run: `Kunin ang libreng +100 na baon.` |
| `Prize` | `Ganti` | `Premyo` is 39px in a 34px badge. |
| `Lost In Void` | `Sa kawalan` | |
| `Kessler Hero` | `Bayani Kessler` | 95px vs 81px, under the 112px floor. Dropping either word loses the name. |
| Privacy modal | translated, not polished | Out of the app flow. `localStorage` and `cookies` stay. `Orbital Salvage` stays. |

Backup Pod and Auxiliary Scrap Pod are both `ekstrang pod`. The `+8 Scrap` on those lines is `sirang-bakal`.

## Accepted fit and glossary warnings

No shorter natural form keeps the meaning, or the full glossary string passes the 72% shrink floor.

| Keys | n | Decision |
| --- | ---: | --- |
| `LVL 1`–`LVL 50` | 50 | Suppressed. Glossary word Antas. |
| `Wear` | 1 | `Isuot`, 30px vs 29px. Glossary verb. Under the 40px floor. |
| `Skies` | 1 | `Langit`, 36px vs 30px. `Kalangitan` is past the shrink floor. |
| `Nibbles Hats` | 1 | `Sumbrero Nibbles`, 93px vs 69px, under the 96px floor. Full phrase is a fit error. Glossary warning expected. |
| `Kessler Hero` | 1 | `Bayani Kessler`, 95px vs 81px, under the 112px floor. |
| `TOY SHOP` | 1 | Glossary warning. Header is `MGA LARUAN`. The sentence uses `tindahan ng laruan`. |

## Twelve least-sure keys

1. `Reset` → `Reset` (copy; confirm is `Oo, reset`; the longer button is `Burahin lahat`)
2. `Cyber Orbit 1`–`10` → copied (`cyber` + glossary `orbit`)
3. `TOY SHOP` → `MGA LARUAN` (glossary still wants `tindahan ng laruan`)
4. `Nibbles Hats` → `Sumbrero Nibbles` (glossary still wants `mga sumbrero ni Nibbles`)
5. `Donut Rings` → `Singsing` (drops `donat` on the tab)
6. `SHOP` → `Bili` (`Tindahan` passes the shrink floor)
7. `Prize` → `Ganti` (`Premyo` overruns the badge; `ganti` can also mean payback)
8. `Start Shift ▶` → `Tara shift ▶`
9. `PLAY` → `LARO` (`MAGLARO` overruns the line)
10. `Skies` → `Langit`
11. `Singularity Horizon` → `Horizon Singular` (`Kessler Singularidad` keeps the long word)
12. `Kessler Hero` → `Bayani Kessler`

## Glossary additions requested

- A short Toy Shop header. `tindahan ng laruan` cannot fit `TOY SHOP` (fit error). This table uses `Mga laruan` there and the full phrase in the sentence.
- A short Nibbles Hats tab. `mga sumbrero ni Nibbles` cannot fit (fit error). This table uses `Sumbrero Nibbles`.
- A short skies word. `kalangitan` cannot fit (fit error). This table uses `Langit`.
- A short Donut Rings tab. `mga singsing na donat` cannot fit. This table uses `Singsing`.
- A short cleared word for the pill. `nalampasan` overruns `0 / 3 CLEARED`. This table uses `Tapos`.
- A short chute on level cards. `padulas` overruns the cards. This table uses `Dulas`.
- A dock word for Shop of about four letters. `Tindahan` passes the shrink floor. This table uses `Bili`.

`shift`, `orbit`, `ekstrang pod`, `sirang-bakal`, `gintong barya`, `antas`, `sektor`, `mundo`, `tropeo`, `araw-araw na gawain`, `araw-araw na baon`, `kosmikong tagalinis`, `itim na butas`, and `basura sa kalawakan` are used wherever the line can hold them.

## Kit notes

- Fragment keys `Unlock (1200` and `Trophy: Daily Duty Star (+500` are still cut off in English. Filipino does not add the missing `)`.
- `str` rows still have no line box, so toast and hub width are not fit-checked. Dock badges were kept inside the `max_chars` notes (`REGALO` 6, `LIBRE` 5, `{0} BAGO` 8).
- A `check: yes` phrase that cannot fit the header (`Toy Shop`, `Nibbles Hats`) becomes a glossary warning on the short form. That is expected. The fit error cannot be suppressed.
- The LVL alias requires `Antas` on chips narrower than the word. Those 50 fit warnings are suppressed, not fixed.
- `settings_modal_privacy` is out of the app flow. It is translated and not polished. `cookies` stays as the web word. `localStorage` stays.
- Identical loanwords are listed in `fil-identical-ok.json`. They are not added to `doNotTranslate`.
