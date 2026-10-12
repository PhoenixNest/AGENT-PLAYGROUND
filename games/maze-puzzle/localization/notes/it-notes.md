# Italian notes — it (Italiano)

Draft table, 2026-10-12. Informal `tu`. Guillemets were not needed: the English source has no quoted speech. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-it/review-it.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 53 |
| suppressed fit warnings | 95 |
| accepted identical | 1 (`Hangar`) |

Visible warnings by kind: `glossary` 52, `length` 1. No `stale`, `glyph`, `expansion`, `placeholder`, `number`, `emoji`, or `fit` errors. The 95 suppressed fit warnings are listed in `it-fit-exceptions.json`. `it-identical-ok.json` keeps `Hangar`.

## Visible warnings

| Keys | n | Why it stays |
| --- | ---: | --- |
| `LVL 1`–`LVL 50` | 50 | Glossary wants `livello`. `Livello 1` is a fit **error** on the chip (past the 72% floor). The chip says `Fase`. The wide line `LVL 1 • Bubblegum Chute 1` still says `Livello`. |
| `Nibbles Hats` | 1 | `Cappelli Nibbles` keeps the name. `cappelli di Nibbles` crosses the shrink floor on the tab. |
| `TOY SHOP` | 1 | Header is `GIOCATTOLI` (1px over, suppressed as fit). `negozio di giocattoli` crosses the shrink floor. The shop sentence uses the full glossary phrase. |
| `SHOP` | 1 | `length` 1.75×. Dock label is `Negozio`. A 6-letter word would clear the warning and would read as a different place. |

`LVL 1`, `LVL 21`, and `LVL 41` do not fit-warn (`Fase` fits those boxes). They still glossary-warn.

## Buttons and short controls

| English | Italian | Why |
| --- | --- | --- |
| `Close` | `Chiudi` | The verb. Under the shrink floor. `Esci` would mean leave. |
| `Cancel` | `No` | Pairs with `Sì, azzera`. `Annulla` overflows the 37px button. |
| `Try Again` | `Riprovaci` | Warm imperative. Fits. |
| `Play Again` | `Rigioca` | Different button from the defeat retry. |
| `Claim +100` / `+300` | `Prendi +100` / `Prendi +300` | `Riscuoti` does not fit. `Prendi` is used for every claim. |
| `Wear` | `Metti` | `Indossa` crosses the shrink floor on the chip. Same word on `shop.footer.wear`. |
| `Unlock (500` | `Sblocca (500` | Verb kept. Missing `)` is still missing, as in English. |
| `Reset` / `Yes, Reset` | `Azzera` / `Sì, azzera` | Italian verb, not the English loanword. The small button shrinks and stays under the floor. |
| `← Back` | `← Torna` | `Indietro` crosses the shrink floor. Arrow stays `←`. |
| `0 / 3 CLEARED` | `0 / 3 FATTI` | Short pill word so the counter does not ellipsize. `CLEARED ✓` stays `SUPERATO ✓`. |
| `✓ UNLOCKED` | `✓ SBLOCCATO` | Not a word that means "still open". |
| `Hangar` | `Hangar` | Italian loanword, same spelling. See `it-identical-ok.json`. |
| `SHOP` | `Negozio` | See the length warning above. |
| `TOY SHOP` | `GIOCATTOLI` | See the glossary warning above. |

`settings_modal_privacy` is translated, including a shorter body, and was not tuned for the shared-heading collision. That modal is outside the app flow.

## Doubtful lines

| Key | Choice | Alternative |
| --- | --- | --- |
| `LVL 1`–`LVL 50` | `Fase N` | `Livello N` is a fit error on the chip. |
| `Cyber Orbit N`, `Cyber Donut Rings` | `Orbita neon N`, `Ciambella neon` | Keeping `cyber` would leave an English theme word. Neon ties the orbits to the ring. |
| `dock.daily.new` | `{0} nuovi` | Italian cannot agree `1 nuovo` / `2 nuovi` in one template. Spec keys are grammatical (`1 NUOVO`, `2 NUOVI`, `3 NUOVI`). The live badge is wrong when the count is 1. |
| `Silver Shift • Sector Hazard` | `Turno argento • Settore` | `Rischio` did not fit beside `settore`. |
| `Gold Shift • Vortex Apex` | `Turno oro • Vortice` | `Apice` did not fit. |
| `Bronze Shift • Cadet Sweep` | `Turno bronzo • Cadetto` | The sweep word did not fit. `Cadetto` is the rank. |
| `Daily Duty Star` | `Stella di oggi` | `Stella del dovere` overflowed the 97px title. |
| `Master All-Clear Bounty` | `Taglia maestra` | Glossary word. A child may hear "size", not "reward". `Premio` is used on the short bounty chip. |
| `Nibbles Hats` | `Cappelli Nibbles` | Full `cappelli di Nibbles` is a fit error. |
| `TOY SHOP` | `GIOCATTOLI` | Full `negozio di giocattoli` is a fit error. |
| `Claim` family | `Prendi` | Glossary prefers `riscuoti`, which does not fit the buttons. |
| `Wear` | `Metti` | Glossary prefers `indossa`. |
| `Hangar` | `Hangar` | `Base` fits and is less specific. |
| `Reset` | `Azzera` | `Reset` fits with no warning and is the loanword. |
| `0 / 3 CLEARED` | `FATTI` | `SUPERATO` is the glossary word and is too long for the pill. |
| `First Nom Nom!` | `Primo gnam!` | `Primo gnam gnam!` overflowed. Check is off for this term. |
| `Nebula Pinball` | `Flipper nebulosa` | `Flipper` is the Italian word for the game. |
| Privacy body | `cookie` | The legal loanword. The rest of that modal was not layout-tuned. |

## Twelve least-sure keys

1. `LVL 1` → `Fase 1` (and the rest of the chips)
2. `Cyber Orbit 1` → `Orbita neon 1`
3. `dock.daily.new` → `{0} nuovi`
4. `Silver Shift • Sector Hazard` → `Turno argento • Settore`
5. `Gold Shift • Vortex Apex` → `Turno oro • Vortice`
6. `SHOP` → `Negozio`
7. `TOY SHOP` → `GIOCATTOLI`
8. `Nibbles Hats` → `Cappelli Nibbles`
9. `Claim +100` → `Prendi +100`
10. `Daily Duty Star` → `Stella di oggi`
11. `Master All-Clear Bounty` → `Taglia maestra`
12. `Hangar` → `Hangar`

## Glossary additions requested

- A chip-length form of `livello`. `Livello` plus the number is a fit error, not a warning, so it cannot go in the fit allowlist.
- A header-length form of `negozio di giocattoli`, and a tab-length form of `cappelli di Nibbles`. `Ciambelle` stands in for `anelli ciambella` for the same reason (`check` is off there).
- Stem matching for plurals. `turni` does not contain `turno`, `livelli` does not contain `livello`, and `trofei` does not contain `trofeo`. This table uses a grammatical singular (`ogni turno`, `ogni livello`, `Trofeo`) where the line can hold it.
- If the cyber theme must stay, add `cyber` as an allowed theme loanword. This table uses `neon` instead.
- `riscuoti` and `indossa` need short button forms (`Prendi`, `Metti`) or wider controls.

## English source

- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut off on purpose. Italian does not add the `)`.
- Backup Pod and Auxiliary Scrap Pod are both `capsula di riserva`. The toast also keeps `rottami` for Scrap.
- The privacy modal still stacks a heading on the paragraph under it. Shorter Italian does not move that shared top.

## Validator summary

`validate_table.py --lang it`: errors 0, warnings 53 (`glossary` 52, `length` 1), suppressed fit 95, accepted identical 1. Glyph check passed (Fredoka / Nunito). No native speaker has reviewed this table.
