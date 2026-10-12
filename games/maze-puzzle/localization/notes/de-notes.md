# German notes — de (Deutsch)

Pilot language, 2026-10-12. Informal `du`. Nouns capitalised. No native speaker has reviewed this table, including the wording pass after the layout audit. The 30-key sample is at `/tmp/maze-i18n/tr-de/review-de.md` and is not marked done.

## Coverage after the audit pass

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 26 |
| suppressed fit warnings | 50 |

Visible warnings by kind: `fit` 24, `glossary` 2. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `de-fit-exceptions.json` (glossary word Level). No `stale`, `glyph`, `length`, or `expansion` warnings.

`str:unit.kg` and `str:unit.quota` are copies again (`{0} kg`, `{0} / {1} kg`). The kit now treats those as unit strings, so the earlier U+00A0 workaround is gone.

## Wording after the layout audit

| Item | Before | After | Fit |
| --- | --- | --- | --- |
| `Try Again` | `Versuch's` | `Nochmal` | 42px in a 55px box. `Play Again` was already `Nochmal`. |
| `✓ UNLOCKED` | `✓ OFFEN` | `✓ FREI` | 39px in an 81px box. Pair with `LOCKED` → `GESPERRT` (48px in a 50px box, no warning). |
| `0 / 3 CLEARED` family | `0 / 3 GESCHAFFT` (73px, warning; ellipsis on the pill) | `0 / 3 FERTIG` (and 1, 2, 3) | 57px against a 62px budget. No warning. `CLEARED ✓` stays `GESCHAFFT ✓`. |
| `Hangar` | `Heim` | `Hangar` | 36px in a 43px box. Identical to English. See the loanword waiver below. |
| `Close` | `Zu` | `Zu` kept | `Schließen` is 58px. The shrink floor of this 32px button is 45px, so it would ellipsize. |
| `Background space synth melody` | `Leise Background-Melodie` | `Leise Weltraum-Melodie` | 117px in a 165px box. The `kg` substring bug is fixed in the kit, so the English word is gone. |
| `100% offline, kid-safe, no trackers` | `100% offline, Kinder, ohne Tracker` (171px, fitted) | `100% offline, ohne Tracker` | `kindersicher` is 204px against a 176px budget (warning, under the 244px floor). The instructed fallback fits at 132px. |
| `TOY SHOP` | `SPIELZEUG` (76px, no fit warning) | `SPIELLADEN` | 85px against an 84px budget. Warning, under the 116px floor. `Spielzeugladen` is still a fit error at 119px. |
| Shop sentence | `Spielzeugladen` | `Spielladen` | Same word as the header. Both lines now glossary-warn (expected `Spielzeugladen`). |
| Care package | `Putz-Carepaket!` / `Carepaket!` | `Putz-Paket!` | Title 69px in 140px. Fragment `care package!` 57px in 72px. Toast and gift banner use the same word. |
| Backup pod | `Reservekapsel` on the button and the toast | unchanged | Glossary word for both English names (Backup Pod and Auxiliary Scrap Pod). Not `Reserve-Pod`. |
| `Reset` | `Neu` | `Reset` | 30px in a 30px box. The English key is `Reset` (`btn-reset-game-data`), not `Reset Progress` (that label stays `Zurücksetzen`). `Yes, Reset` stays `Ja, neu`. Identical to English. See the waiver. |
| Privacy modal | left as written | left as written | The modal is out of the app flow. Collision ignored. |

## Loanword waiver

`Hangar` and `Reset` are the German words and they are byte-identical to English. `de-fit-exceptions.json` only suppresses `fit` warnings. It cannot waive `untranslated`. The glossary loanword copy only matches a glossary row (`Donut`, `Pause`, `Bronze`), and these two are not rows.

After `merge_table.py` (which rewrites `doNotTranslate` from English), `Hangar` and `Reset` were added back onto the German table's `doNotTranslate` list. That is the checker’s identical-string allowlist. The next merge will drop them and the two keys will error again until the kit has an untranslated waiver or glossary rows for them.

## Accepted fit and glossary warnings

No shorter natural synonym exists, or the layout audit showed the painted control already fits, or the lead asked for this word.

| Keys | n | Decision |
| --- | ---: | --- |
| `LVL 1`–`LVL 50` | 50 | Suppressed. Glossary word Level. |
| `Horizon Sector 1`–`9` | 9 | `Horizontsektor`. About 2px over. The audit still painted the cards in full. A shorter form would drop the place or **Sektor**. |
| `Moon Chute 1`–`10` | 10 | `Mondrutsche`. About 1–2px over (several rows round to the same pixel as the box). Shortest Mond + Rutsche compound. Not in the ellipsis list. |
| `Nibbles Hats` | 1 | `Nibbles-Hüte`, 70px vs 69px. Glossary string. The shop tab still painted in full. |
| `Skies` | 1 | `Himmel`, 36px vs 30px. Glossary word. The tab still painted in full. |
| `TOY SHOP` | 1 | `SPIELLADEN`, 85px vs 84px. Lead asked for this over `SPIELZEUG` (which fitted at 76px) and over `Spielzeugladen` (fit error). |
| `allowance.` | 1 | `Taschengeld.`, 64px vs 55px. Glossary word. The banner line painted in full. |
| `← Back` | 1 | `← Zurück`, 49px vs 44px. Audit: keep. Arrow stays `←`. |
| Toy Shop sentence + header | 2 | Glossary warnings. Both say Spielladen, not Spielzeugladen. |

## Twelve least-sure keys

1. `Hangar` → `Hangar` (copy, local do-not-translate waiver)
2. `Reset` → `Reset` (copy; confirm button is still `Ja, neu`)
3. `Close` → `Zu` (`Schließen` crosses the shrink floor)
4. `100% offline, kid-safe, no trackers` → `100% offline, ohne Tracker` (`Kinder` fitted and was dropped because `kindersicher` did not)
5. `TOY SHOP` → `SPIELLADEN`
6. Shop sentence → `Spielladen` (glossary still wants Spielzeugladen)
7. Care package → `Putz-Paket!` (glossary still says Carepaket)
8. `Silver Shift • Sector Hazard` → `Silber-Schicht • Sektor` (Gefahr did not fit)
9. `Gold Shift • Vortex Apex` → `Gold-Schicht • Wirbel` (Wirbelspitze did not fit)
10. `Bronze Shift • Cadet Sweep` → `Bronze-Schicht • Kadett` (the sweep word did not fit)
11. `Singularity Horizon` → `Singular-Horizont`
12. `Daily Duty Star` → `Tagesstern`

## Kit notes that still matter

Fixed on `integration/i18n` since the first German commit, and this table now relies on them: `kg` is a token (so Background is not a unit), `{0} kg` may be copied, `LVL` / `space junk` / Auxiliary Scrap Pod are glossary aliases, and a logo hit is not a `space trash` hit.

Still open:

- Identical loanwords that are not glossary rows (`Hangar`, `Reset`) error, and `merge_table.py` will not keep a German-only `doNotTranslate` entry.
- `selftest_kit.py` `test_real_german_table` still pins 28 visible warnings and one Toy Shop glossary hit. This pass is 26 visible warnings and two glossary hits. The test was not edited.
- Fragment keys `Unlock (1200` and `Trophy: Daily Duty Star (+500` are still cut off in English. German does not add the missing `)`.
- `str` rows still have no line box, so toast width is not fit-checked.
