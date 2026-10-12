# Italian notes — it (Italiano)

Wording pass 2, 2026-10-12, after the all-language audit. Informal `tu`. Guillemets were not needed: the English source has no quoted speech. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-it/review-it.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 99 |
| suppressed fit warnings | 47 (file lists all 50 `LVL` chips) |
| accepted identical | 1 (`Hangar`) |

Visible warnings by kind: `glossary` 53, `fit` 46. No `stale`, `glyph`, `length`, `expansion`, `placeholder`, `number`, `emoji`, or `fit` errors. `it-fit-exceptions.json` lists only `spec:LVL 1` through `spec:LVL 50`. `LVL 1`, `LVL 21`, and `LVL 41` do not fit-warn, so the validator suppresses 47. `it-identical-ok.json` keeps `Hangar`.

## What this pass changed

Fredoka 700 widths, same size as the control. A scale is the index box, or the box implied by the string the audit clipped, divided into the new width. Scale 1.00 means the new string is inside that box at full size.

| English | Before | After | Fit |
| --- | --- | --- | --- |
| `DAILY OPERATIONS` | `OPERAZIONI GIORNALIERE` | `TURNI DI OGGI` | 112px at 17px. The old 208px title was clipped at the 72% floor on the header (`OPERAZIONI GIORNALIE…`) and the card (`OPERAZIONI GIORN…`). The new title is about half that width, so both sit at full size. Estimator 112 vs a 156px budget. |
| `SILVER` / `SILVER ✓` | `ARGENTO` / `ARGENTO ✓` | `Argento` / `Argento ✓` | 37.8px at 10px. `BRONZO` at 39.2px paints in full on the same medal, and `BRONZO ✓` paints in full on the card. Title case is narrower than those, so the medal no longer ends at `ARGEN…`. Estimator 35 vs 39, and 48 vs 51. |
| `BRONZE` / `GOLD` and the `✓` chips | `BRONZO` / `ORO` | `Bronzo` / `Oro` | Same words. Title case matches `Argento` on the medal row. Both were already inside the medal. |
| `SHOP` | `Negozio` (drawn `NEGO…`) | `Gioco` (drawn `GIOCO`) | 33.3px at 11px. `NEGOZIO` is 47.0px and was clipped at the 72% floor. Against that dock, `GIOCO` is about 0.84. Estimator 28 vs a 30px budget. |
| `TROPHY ROOM` | `SALA DEL TROFEO` | `SALA TROFEO` | Was scale 0.78 (148px). Now 113px, about full size in the header that held the old line at 0.78. Still contains `trofeo`. |
| `LOCKED` | `BLOCCATO` | `CHIUSO` | Was scale 0.76. `CHIUSO` is 42px at 12px against a box near 48px, so full size. Earned badges stay `✓ SBLOCCATO`, which already paints in full. |
| `Master All-Clear Bounty` | `Taglia maestra` | `Premio finale` | A child hears `taglia` as size. 78px at 13px inside a 144px line. The two toasts use `premio finale` too. The short bounty chip stays `Premio`. |
| `dock.daily.new` | `{0} nuovi` | `{0} novità` | `novità` does not change with the count, so 1 is grammatical. `3 novità` is 33.6px at 9px; `3 NUOVI` at 34.5px already paints in full on the home badge. Spec keys stay `1 NUOVO`, `2 NUOVI`, `3 NUOVI` because each key is one count. |

`Riprovaci` and `Prendi` stay. Both paint in full, and both are natural for a child. `Riscuoti` still does not fit the claim buttons.

## Visible warnings

| Keys | n | Why it stays |
| --- | ---: | --- |
| `LVL 1`–`LVL 50` | 50 | Glossary wants `livello`. `Livello` plus the number is a fit error on the chip. The chip says `Fase`. The mission line still says `Livello`. Fit is suppressed for every chip; three of them do not fit-warn. |
| `DAILY OPERATIONS` | 1 | Glossary wants `operazioni giornaliere`. That phrase is what the audit clipped. `TURNI DI OGGI` fits. `turni` does not contain `turno`, so the shift term would miss too if this header were a shift line. It is not. |
| `Nibbles Hats` | 2 | Fit and glossary. The shop tab paints `Cappelli Nibbles` in full. `cappelli di Nibbles` crosses the shrink floor. |
| `TOY SHOP` | 2 | Fit is 85px vs 84px. The header paints `GIOCATTOLI` in full. Glossary wants `negozio di giocattoli`, which crosses the shrink floor. The shop sentence keeps that phrase. |
| `Choose Sector` | 1 | `Scegli settore` is 81px vs 81px. The pause button paints it in full. `Apri settore` would fit and would be a different verb. |
| `Claim +100`, `Claim +300` | 2 | `Prendi +100` is 61px vs 59px. `Prendi +300` is 63px vs 62px. Both paint in full on the shop. Dropping the space would clear the estimator and would glue the number to the verb. |
| `Close` | 1 | `Chiudi` is 39px vs 33px. It is the close verb. `Esci` fits and means leave. |
| `FREE` | 1 | `GRATIS` is 27px vs 23px. The home badge paints `GRATIS` in full. There is no 5-letter word for free. |
| `Prize` | 1 | `Premio` is 39px vs 34px and paints in full next to the coin. `Dono` would fit and would split the bounty word. |
| `Reset` | 1 | `Azzera` is 36px vs 30px. The red button paints it in full. No 5-letter Italian verb. `Reset` would clear the warning and would be English. |
| `Wear` | 1 | `Metti` is 30px vs 29px and paints in full. `Indossa` crosses the shrink floor. |
| `Unlock (500`, `Unlock (1200`, `Unlock (1500` | 3 | About 1px over. The shop buttons paint `Sblocca (500`, `Sblocca (1200`, and `Sblocca (1500` in full. The other unlock prices are inside the box. |
| `Launch Backup Pod (+8 Scrap!)` | 1 | 232px vs 182px. The defeat button paints `Lancia capsula di riserva (+8 rottami!)` in full, with both glossary words. |
| `Moon Chute 1`–`10` | 10 | `Scivolo luna N` is a few pixels over the index box. World 2 paints every name in full. `Scivolo` stays, matching `Scivolo gommoso`. |
| `Horizon Sector 1`–`9` | 9 | `Settore orizzonte N` is over the index box and paints in full on world 5. It keeps `settore`. |
| `Nebula Pinball`, `Nebula Pinball 1`–`10` | 11 | `Flipper nebulosa` paints in full on world 4. `Flipper` is the Italian word for the game. |
| `Singularity Horizon` | 1 | `Orizzonte singolarità` paints in full as the world 5 title. 154px vs 139px. |
| `Nibbles the Void` | 1 | `Nibbles del vuoto` paints in full on the shop card. 113px vs 106px. |

## Buttons and short controls

| English | Italian | Why |
| --- | --- | --- |
| `Close` | `Chiudi` | The verb. See the fit warning. |
| `Cancel` | `No` | Pairs with `Sì, azzera`. |
| `Try Again` | `Riprovaci` | Warm imperative. The defeat button paints it in full. |
| `Play Again` | `Rigioca` | Different button from the defeat retry. |
| `Claim +100` / `+300` | `Prendi +100` / `Prendi +300` | Same verb on every claim, including the gift. |
| `Wear` | `Metti` | Same word on `shop.footer.wear`. |
| `Unlock (500` | `Sblocca (500` | Missing `)` stays missing, as in English. |
| `Reset` / `Yes, Reset` | `Azzera` / `Sì, azzera` | Italian verb. The button paints `Azzera` in full. |
| `← Back` | `← Torna` | Paints in full. `Indietro` crosses the shrink floor. |
| `LOCKED` | `CHIUSO` | `BLOCCATO` shrank to 0.76. |
| `✓ UNLOCKED` | `✓ SBLOCCATO` | Paints in full. Not a word that means "still open". |
| `0 / 3 CLEARED` | `0 / 3 FATTI` | The pill paints in full. `CLEARED ✓` stays `SUPERATO ✓`. |
| `Hangar` | `Hangar` | Accepted loanword. See `it-identical-ok.json`. |
| `SHOP` | `Gioco` | Dock only. The page title stays `GIOCATTOLI`. The sentence stays `negozio di giocattoli`. |
| `DAILY OPERATIONS` | `TURNI DI OGGI` | Header and card. The shift titles still say `turno`. |
| `SILVER` | `Argento` | Same word as `Turno argento`. Title case is what fits the medal. |

`settings_modal_privacy` was not edited in this pass. The two shared bodies still ellipsize. `cookie` stays in that body.

The daily countdown `Tra {0}h {1}m` still loses its first letter under the alarm icon. That is the icon sitting on the start of the line, on every language that puts a word before the numbers. Shortening `Tra` would not move the start of the line.

## Doubtful lines

| Key | Choice | Alternative |
| --- | --- | --- |
| `LVL 1`–`LVL 50` | `Fase N` | `Livello N` is a fit error on the chip. |
| `DAILY OPERATIONS` | `TURNI DI OGGI` | `OPERAZIONI GIORNALIERE` is the glossary phrase and was clipped. |
| `SHOP` | `Gioco` | `Negozio` was clipped to `NEGO…`. `Gioco` is the short form of `giocattoli`, not a second shop. |
| `SILVER` | `Argento` | All-caps `ARGENTO` was clipped to `ARGEN…`. There is no shorter word for silver. |
| `LOCKED` | `CHIUSO` | `BLOCCATO` matches `SBLOCCATO` and shrank to 0.76. |
| `dock.daily.new` | `{0} novità` | `{0} nuovi` is wrong when the count is 1. |
| `Master All-Clear Bounty` | `Premio finale` | `Taglia maestra` reads as size. |
| `Cyber Orbit N` | `Orbita neon N` | `cyber` would be an English theme word. |
| `Silver Shift • Sector Hazard` | `Turno argento • Settore` | `Rischio` did not fit beside `settore`. |
| `Nibbles Hats` | `Cappelli Nibbles` | Full `cappelli di Nibbles` is a fit error. |
| `TOY SHOP` | `GIOCATTOLI` | Full `negozio di giocattoli` is a fit error on the header. |
| `Hangar` | `Hangar` | `Base` fits and is less specific. |

## Twelve least-sure keys

1. `SHOP` → `Gioco` (dock). The page title is still `GIOCATTOLI`.
2. `DAILY OPERATIONS` → `TURNI DI OGGI`
3. `SILVER` → `Argento` (title case, same word as the shift)
4. `LOCKED` → `CHIUSO` beside `✓ SBLOCCATO`
5. `LVL 1` → `Fase 1` (and the rest of the chips)
6. `dock.daily.new` → `{0} novità`
7. `Master All-Clear Bounty` → `Premio finale`
8. `Cyber Orbit 1` → `Orbita neon 1`
9. `Nibbles Hats` → `Cappelli Nibbles`
10. `TOY SHOP` → `GIOCATTOLI`
11. `Silver Shift • Sector Hazard` → `Turno argento • Settore`
12. `Hangar` → `Hangar`

## Glossary additions requested

- A chip-length form of `livello`. `Livello` plus the number is a fit error, so it cannot go in the fit allowlist.
- A header-length form of `operazioni giornaliere`. The full phrase is what the daily header clips.
- A dock-length form of the toy shop. `Negozio` clips. This table uses `Gioco` on the dock, `GIOCATTOLI` on the header, and `negozio di giocattoli` in the sentence.
- A tab-length form of `cappelli di Nibbles`.
- Stem matching for plurals. `turni` does not contain `turno`, `livelli` does not contain `livello`, and `trofei` does not contain `trofeo`.
- If the cyber theme must stay, add `cyber` as an allowed theme loanword. This table uses `neon` instead.
- `riscuoti` and `indossa` need short button forms (`Prendi`, `Metti`) or wider controls.
- A medal-length all-caps form of `argento`. Title case fits. All caps does not.

## English source

- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut off on purpose. Italian does not add the `)`.
- Backup Pod and Auxiliary Scrap Pod are both `capsula di riserva`. The toast also keeps `rottami` for Scrap.
- The privacy modal still stacks a heading on the paragraph under it. This pass did not edit that modal.

## Validator summary

`validate_table.py --lang it`: errors 0, warnings 99 (`glossary` 53, `fit` 46), suppressed fit 47, accepted identical 1. The fit-exception file has 50 `LVL` keys and nothing else. Glyph check passed (Fredoka / Nunito). No native speaker has reviewed this table.
