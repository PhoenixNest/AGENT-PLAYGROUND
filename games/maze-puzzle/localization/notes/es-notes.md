# Spanish notes — es (Español)

Neutral Latin-American Spanish, `tú` in sentences. Buttons that fit use the infinitive (`Jugar`, `Cerrar`, `Gana`). Opening `¡` `¿` stay in the same run as the closing mark. No native speaker has reviewed this table, including the wording pass after the 2026-10-12 all-language audit.

## Coverage after the audit pass

|                         | Count |
| ----------------------- | ----: |
| spec keys               |   355 |
| str keys                |    81 |
| validator errors        |     0 |
| visible warnings        |    32 |
| suppressed fit warnings |    50 |

Visible warnings by kind: `fit` 28, `glossary` 4. The 50 suppressed warnings are `LVL 1`–`LVL 50` (glossary word nivel). No `stale`, `glyph`, `length`, or `expansion` warnings. `es-identical-ok.json` is empty: `Hangar` is now `Inicio`.

The visible count is the same as before this pass. Two fit warnings became glossary warnings (`DAILY OPERATIONS`, `SECTOR`). The `DAILY` fit warning is gone.

## Changes after the all-language audit

Measured with the kit width estimate (font size × average advance). The card title is narrower than the captured header box, so the daily title was shortened well past that box.

| English               | Before                      | After           | Fit                                                                                                                            |
| --------------------- | --------------------------- | --------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| `DAILY OPERATIONS`    | `OPERACIONES DIARIAS`       | `TURNOS DE HOY` | 158px over a 156px header, and the card ellipsized to `OPERACIONES DIARI…` at the 72% floor. After: 103px in the 156px header. |
| `Try Again`           | `Intenta`                   | `Otra vez`      | 45px in a 55px box. Same word as `Play Again`.                                                                                 |
| `PLAY`                | `JUEGA`                     | `JUGAR`         | 38px in a 42px box. Same width.                                                                                                |
| `PLAY MISSION`        | `JUEGA MISIÓN`              | `JUGAR MISIÓN`  | 110px in a 133px box. Same width.                                                                                              |
| `Claim +100`          | `Toma +100`                 | `Gana +100`     | 49px in a 59px box. `Reclama +100` is 67px.                                                                                    |
| `Claim +300`          | `Toma +300`                 | `Gana +300`     | 51px in a 62px box.                                                                                                            |
| `Close`               | `Cierra`                    | `Cerrar`        | 39px in a 32px box, under the 45px floor. Same width as `Cierra`.                                                              |
| `Donut Rings`         | `Anillos`                   | `Donas`         | 30px in a 65px box. The donut is the tab.                                                                                      |
| `SECTOR`              | `Sector`                    | `Zona`          | 20px in a 42px box. The chip was uppercased to `SECTOR`.                                                                       |
| `Hangar`              | `Hangar`                    | `Inicio`        | 36px in a 43px box. Defeat home button.                                                                                        |
| `DAILY`               | `Diario`                    | `Día`           | 17px in a 32px box. Was 33px.                                                                                                  |
| `TROPHY ROOM`         | `SALA DE TROFEOS`           | `TROFEOS`       | 60px in a 123px box. Was 120px and painted at 0.78.                                                                            |
| `RESET ALL PROGRESS?` | `¿BORRAR TODO EL PROGRESO?` | `¿BORRAR TODO?` | 101px in a 196px box. Was 195px and painted at 0.76.                                                                           |

`Gana` is also on `gift.button.claim`, the claimed state (`Ganado hoy`, `GANADO ✓`), and the two claim toasts. Longer lines still say `reclama` where `reclamar` fits.

## What this pass did not change

- The six privacy ellipses (two bodies and the toy-shop sentence, zero and iPhone). That modal is out of the app flow.
- `Turno plata • Sector` still says `sector`. That line has to contain the glossary word. The all-caps chip is the one that looked English.
- `Tienda` on the dock is 33px in a 30px box. The audit still painted `TIENDA` in full. `Bazar` would fit and would split the shop word (`tienda` in the sentence, `bazar` on the tab).
- `Cancelar`, `GRATIS`, `Premio`, `Poner`, `Órbita ciber`, `Sector horizonte`, `Héroe Kessler`, `Cápsula de reserva (+8 chatarra)`, and `Gorros Nibbles` stay. Shorter substitutes either change the meaning or drop a required glossary word. All of those fit warnings are under the shrink floor.

## Warnings still left

| Keys                            | Why it stays                                                                                                            |
| ------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `Cancel`                        | `Cancelar` is 48px in a 37px box, under the 51px floor. `Anular` fits and can be read as wipe.                          |
| `Close`                         | `Cerrar` is 39px in a 32px box, under the 45px floor. No shorter close-verb.                                            |
| `Cyber Orbit 1`–`10`            | `Órbita ciber` is about 6px over. `Ronda` fits and means a round, not an orbit.                                         |
| `DAILY OPERATIONS`              | `TURNOS DE HOY` drops `operaciones diarias` so the card can stop ellipsizing.                                           |
| `FREE`                          | `GRATIS` is 27px in a 23px box, under the 32px floor. `Libre` means available, not free of charge.                      |
| `Horizon Sector 1`–`9`          | `Sector horizonte` is about 12px over. `sector` is required.                                                            |
| `Kessler Hero`                  | `Héroe Kessler` is 88px in an 81px box, under the 112px floor.                                                          |
| `Launch Backup Pod (+8 Scrap!)` | `Cápsula de reserva (+8 chatarra)` is 192px in a 182px box, under the 253px floor. Both glossary words are in the line. |
| `Nibbles Hats`                  | `Gorros Nibbles` is 81px in a 69px box, under the 96px floor. `gorros de Nibbles` crosses that floor.                   |
| `Prize`                         | `Premio` is 39px in a 34px box, under the 47px floor.                                                                   |
| `SECTOR`                        | `Zona` does not contain `sector`. The uppercased chip was the English lookalike.                                        |
| `SHOP`                          | `Tienda` is 33px in a 30px box, under the 41px floor. Same word as the shop header.                                     |
| `TOY SHOP`                      | `TIENDA` does not contain `tienda de juguetes`. The sentence does.                                                      |
| `Wear`                          | `Poner` is 30px in a 29px box, under the 40px floor. Glossary verb.                                                     |

## Twelve least-sure keys

1. `DAILY OPERATIONS` → `TURNOS DE HOY`
2. `SECTOR` → `Zona`
3. `Claim +100` → `Gana +100`
4. `Donut Rings` → `Donas`
5. `Hangar` → `Inicio`
6. `Close` → `Cerrar`
7. `DAILY` → `Día`
8. `TROPHY ROOM` → `TROFEOS`
9. `RESET ALL PROGRESS?` → `¿BORRAR TODO?`
10. `PLAY` → `JUGAR`
11. `Try Again` → `Otra vez` (same as `Play Again`)
12. `Silver Shift • Sector Hazard` → `Turno plata • Sector` (glossary `sector` kept; the chip is `Zona`)

## Glossary gaps

`turno` is the shift word everywhere, including the new daily title. `dona` is the donut word on the tab and on the items. `inicio` is the home word on the defeat button and on `Ir al inicio`. `chatarra`, `monedas de oro`, `nivel`, `cápsula de reserva`, and `basura espacial` are unchanged.

Still short of the glossary string, on purpose: `operaciones diarias` (title), `sector` (the uppercased chip only), `gorros de Nibbles`, `tienda de juguetes` (header and dock), `tobogán` (`rampa` on every chute), `superado` (`hecho`), `desbloquear` (`abre` on price buttons), `dinero de bolsillo` (`bolsillo` on the gift line), `cielos` (`cielo`).

toast.infinite.round: agent draft, no native review
