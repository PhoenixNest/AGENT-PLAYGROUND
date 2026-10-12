# German notes — de (Deutsch)

Pilot language, 2026-10-12. Informal `du`. Nouns capitalised. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-de/review-de.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| values different from English | 394 |
| intentional copies | 42 |
| validator errors | 0 |
| validator warnings | 78 |

The 42 copies are brand lines, pure `kg` quotas, emoji, placeholder-only formats, and `fx.hub.orbital` (`✨ ORBITAL ✨`). `str:unit.kg` and `str:unit.quota` are not copies: they use U+00A0 before `kg` (see kit defects).

Warnings by kind: `fit` 77, `glossary` 1. No `stale`, `glyph`, `length`, `expansion`, `empty`, or `untranslated`.

## Warning reasons

Kept, not rewritten. A shorter string existed only by dropping a glossary word or the place name.

| Keys | n | Why it stays |
| --- | ---: | --- |
| `LVL 1` … `LVL 50` | 50 | Glossary says German keeps **Level**. `LVL` is shorter and does not contain the word `level`, so the checker never asks for it. `Level 10` is about 42px against a 31–38px chip. Still under the 72% shrink floor. |
| `Moon Chute 1` … `Moon Chute 10` | 10 | `Mondrutsche` is the shortest compound that keeps Mond and Rutsche. About 1–2px over the chip. |
| `Horizon Sector 1` … `Horizon Sector 9` | 9 | `Horizontsektor` is the shortest compound that keeps the place and **Sektor**. About 2px over. |
| `0 / 3 CLEARED` … `3 / 3 CLEARED` | 4 | **Geschafft** matches `CLEARED ✓` → `GESCHAFFT ✓`, which does fit. `FERTIG` would fit the counter and would split the word. |
| `Nibbles Hats` | 1 | `Nibbles-Hüte` is the glossary string. Estimated 70px against a 69px line. |
| `Skies` | 1 | **Himmel** is the glossary word. `Luft` fits and means the wrong thing. |
| `allowance.` | 1 | **Taschengeld.** is the glossary word on a run of about 55px. The word itself is about 64px. |
| `← Back` | 1 | `← Zurück` is the short back word. About 49px against 44px. The arrow stays `←`. |
| `TOY SHOP` | 1 | Glossary warning, not fit. **Spielzeugladen** is about 119px on an 84px / 17px header. The shrink-floor error line is about 116px, so the glossary word is an error, not a warning. Header is `SPIELZEUG`. The sentence that contains `Toy Shop` does say `Spielzeugladen`. |

## Doubtful choices

| Key | Choice | Alternative |
| --- | --- | --- |
| `Hangar` | `Heim` | The control is `btn-defeat-home`. `Halle` is the literal hangar. `Start` matches the home glossary and also fits the 43px button. |
| `Close` | `Zu` | Language-modal close, 32px at 13px. `Schließen` crosses the shrink floor. |
| `Cancel` | `Nein` | Only `btn-cancel-reset`, next to `Ja, neu`. `Abbrechen` does not fit. |
| `View` | `Lies` | Opens the user agreement (`btn-open-user-agreement`). `Lesen` warns; `Ansehen` does not fit. |
| `Reset` / `Yes, Reset` | `Neu` / `Ja, neu` | The label above is `Zurücksetzen`. `Löschen` does not fit the 30px button. |
| `Lost In Void` | `In der Leere` | `stat-label`, uppercased at runtime. Could be a place or a lost-mass stat. `Verloren` is the other reading. |
| `Wear` / `shop.footer.wear` | `Trag` | **Anziehen** fits the shop footer hint (10 characters) and does not fit the 29px spec chip. Both say `Trag` so they match. |
| `Unlock (500` and the other prices / `shop.footer.unlock` | `Öffnen (…` | **Freischalten** does not fit. `Öffnen` is “open”, not “pay to unlock”. |
| `Claim +100` / `Claim +300` | `Hol +100` / `Hol +300` | **Abholen** warns on those chips. Longer claim sentences use `holen` too, not the infinitive `Abholen`. |
| `DAILY` | `Tag` | Runtime uppercases it to `TAG`. `Täglich` warns on a 32px tab. The screen title is still `Tägliche Einsätze`. |
| `SHOP` / `MAPS` | `Laden` / `Karte` | `Karten` warns. Singular `Karte` fits and is the glossary word. `CLEANUP MAPS` is `AUFRÄUMKARTEN`. |
| `Bronze Shift • Cadet Sweep` | `Bronze-Schicht • Kadett` | `Kadett-Putz` (sweep) overflows. The rank stayed; the sweep word did not. |
| `Silver Shift • Sector Hazard` | `Silber-Schicht • Sektor` | **Schicht** and **Sektor** both have to be in the string. `Gefahr` does not fit beside them. |
| `Gold Shift • Vortex Apex` | `Gold-Schicht • Wirbel` | `Wirbelspitze` overflows. Apex is only “Wirbel”. |
| `Cyber Orbit N` | `Cyberbahn N` | **Umlaufbahn** and even `Cyber-Orbit` overflow the chip. Body copy still says `Umlaufbahn`. |
| `Cheese Moon Orbit` | `Käsemond-Bahn` | `Käsemond-Umlaufbahn` warns. Orbit’s glossary check is off, so this is not a warning. |
| `Singularity Horizon` | `Singular-Horizont` | `Singularitäts-Horizont` warns. `Kessler-Singularität` keeps the full word. |
| `Daily Duty Star` | `Tagesstern` | Compresses “daily duty”. `Tagesdienst-Stern` does not fit the 97px title. |
| `First Nom Nom!` | `Erstes Mampf!` | The eating line uses `Mampf mampf!`. Two mampfs do not fit this title. |
| `100% offline, kid-safe, no trackers` | `100% offline, Kinder, ohne Tracker` | `kindersicher` and `für Kinder` both warn on this sublabel. The heading is `100% offline, für Kinder:`. |
| `Background space synth melody` | `Leise Background-Melodie` | See the `kg` substring defect. `Hintergrund-Melodie` is the real German and fails the unit check. |
| `Master Sound` | `Lautstärke` | This is the master volume, not **Meisterprämie**. |
| `Sound FX Volume` | `Ton-Effekte` | `Effekt-Lautstärke` warns. The slider has to carry “volume”. |
| `Swipe anywhere to spin!` | `Wisch zum Drehen!` | “Anywhere” was cut so the line fits. |
| `toast.backup.deployed` | `Reservekapsel gestartet! (+8 Schrott)` | The button says Backup Pod. The toast says Auxiliary Scrap Pod. Both became Reservekapsel. |

## Glossary

Used as written where the line can hold them: Nibbles, the logo, `kg`, Schrott, Goldmünzen, Sektor, Welt, Level, kosmischer Hausmeister, Schwarzes Loch, Nibbles-Hüte, Trophäe, Tägliche Einsätze, Schicht, Reservekapsel, Tagesprämie, Spielzeugladen (in the sentence, not the header), Weltraummüll, and the unchecked set (Ring, Donut, Himmel, Bronze/Silber/Gold, Meisterprämie, Taschengeld, Snacks, Umlaufbahn, Karte, Rutsche, Kadett, Geschafft, Angelegt, Preis, Start, Pause, Carepaket, Mampf).

Münzen vs Goldmünzen: `Goldmünzen` for the balance and for “gold coins”, “recycling coins”, and “gameplay coins”. `Münzen` only on `Clear unlocked levels, skins & coins`, where `Goldmünzen` overflowed the settings line.

Requested glossary additions, for the other languages as well:

- A short-UI column. German buttons that cannot hold the headword: `Hol` (not Abholen), `Öffnen` (not Freischalten), `Trag` (not Anziehen), `Zu` (not Schließen), `LOS!`, `Weiter` / `Zurück`, `SPIELZEUG` (not Spielzeugladen).
- Chip-length alternates for orbit, toy shop, and level (`Level` vs `LVL`).
- Say explicitly that `gold` on a shift is not `Goldmünzen`, and that `Master Sound` is not `Meisterprämie`.
- One German word for the backup pod. English uses both “Backup Pod” and “Auxiliary Scrap Pod”.

Terms not used, and why: Spielzeugladen on `TOY SHOP` (fit error), Freischalten, Anziehen, Abholen on the narrow buttons, Umlaufbahn on `Cyber Orbit` chips, Gefahr on the silver shift, Putz on the bronze shift. Those are width exceptions, not a rename of Nibbles.

## Fragments

Ready banner, in order: `Hol dir gratis +100` + `Taschengeld.` → “Hol dir gratis +100 Taschengeld.”

Claimed banner: `Morgen gibt's wieder +100` + `Carepaket!` → “Morgen gibt's wieder +100 Carepaket!”

`Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut in the English key. The German keeps the open parenthesis and does not add the missing `)`.

## Kit defects

1. **`kg` is counted as a substring.** `Background` contains the letters `kg`, so `Background space synth melody` errors unless the translation also contains `kg`. Every language will hit this. The check should be a token (`kg` as a unit), not `str.count("kg")`. Same risk for `/` and `+` inside words.
2. **`{0} kg` cannot stay English and cannot be translated.** `is_unit_like` rejects `{` and `}`, so `str:unit.kg` and `str:unit.quota` are “untranslated” if copied and have no other letters to change. This table uses U+00A0 before `kg` (German number-unit spacing, and the character is in Fredoka). Please treat placeholder-plus-unit as do-not-translate.
3. **`space trash` matches the logo.** `english_has_term` finds `space trash` inside `SPACE TRASH CLEAN`. The glossary check is skipped only because the logo is copied verbatim. A later translated logo line would be warned to contain `Weltraummüll`.
4. **`LVL` does not match `level`.** The style row says German keeps Level. The checker does not, and the chip is narrower than the word. Fifty fit warnings are that gap.
5. **Spielzeugladen cannot pass both checks.** On `TOY SHOP` the glossary word is a fit error. The header warning is the cheaper failure. Other long headwords (Freischalten, Anziehen, Abholen) are `check: no`, so a short button fails no glossary warning and a translator can miss them.
6. **Fragment groups are truncated.** Several CSV notes say `groups-truncated`, so sibling runs are not all visible. The gift banner was recoverable. Others may not be.
7. **Source strings are cut mid-token.** `Unlock (1200` and `Trophy: Daily Duty Star (+500` have no closing parenthesis. Translators should not “fix” the English key.
8. **Two English names for one pod.** Button: `Launch Backup Pod (+8 Scrap!)`. Toast: `Auxiliary Scrap Pod Deployed! (+8 Scrap)`.
9. **`space junk` is not the checked phrase.** The glossary prose says body copy should use the space-trash term. The checker only looks for `space trash`. This table still uses Weltraummüll for space junk.
10. **Loanwords that are correct German fail `untranslated` if the bytes match.** `Donut`, `Bronze`, and `Pause` are the right words. `🍩 DONUT 🍩` had to change case (`🍩 Donut 🍩`) or it was an error. A glossary-identical loanword should be allowed.
11. **Instructions disagree, which will hit the other 15 agents.** `translator-brief.md` says do not write into `Assets/Resources/Localization/` and return `<code>.notes.md`. `process.md` says the lead copies the file in after validation. This pilot was told to commit `de.json`, `de.json.meta`, and `localization/notes/de-notes.md`. Pick one path before the other languages run.
12. **`str` rows have no `fz` / `max_width`.** The character hints live in `notes` and `max_chars` and are not fit-checked. Easy to miss a 6-character dock badge.
13. **Fit uses the whole string against one line box.** Wrapping paragraphs warn as soon as they are wider than the English estimate. That is a useful budget, but it is not a single-line control. Agents should know which rows wrap.

## Twelve least-sure keys

1. `Hangar` → `Heim`
2. `Close` → `Zu`
3. `Lost In Void` → `In der Leere`
4. `Background space synth melody` → `Leise Background-Melodie`
5. `Silver Shift • Sector Hazard` → `Silber-Schicht • Sektor`
6. `Gold Shift • Vortex Apex` → `Gold-Schicht • Wirbel`
7. `Bronze Shift • Cadet Sweep` → `Bronze-Schicht • Kadett`
8. `Singularity Horizon` → `Singular-Horizont`
9. `Daily Duty Star` → `Tagesstern`
10. `100% offline, kid-safe, no trackers` → `100% offline, Kinder, ohne Tracker`
11. `TOY SHOP` → `SPIELZEUG`
12. `str:unit.kg` → `{0}` + U+00A0 + `kg`

## Validator

`validate_table.py --lang de` against the merged table, fonts from `Assets/Fonts`, chain from `fonts-chains.json`. Exit would be 0. Errors 0. Warnings 78 (`fit` 77, `glossary` 1). Glyph check ran (German chain is not pending). No native review.
