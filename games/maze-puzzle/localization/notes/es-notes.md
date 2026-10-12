# Spanish notes — es (Español)

Neutral Latin-American Spanish, `tú` throughout. No Spain-only words (`ordenador`, `vosotros`, `vos`). Curly quotes were not needed. Opening `¡` `¿` stay in the same run as the closing mark. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-es/review-es.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 32 |
| suppressed fit warnings | 50 |
| accepted identical | 1 |

Visible warnings by kind: `fit` 30, `glossary` 2. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `es-fit-exceptions.json` (glossary word nivel). No `stale`, `glyph`, `length`, or `expansion` warnings. Every visible fit warning is under the 72% shrink floor.

`str:unit.kg` and `str:unit.quota` stay `{0} kg` and `{0} / {1} kg`.

## Doubtful lines

| Key | Choice | Alternative | Why |
| --- | --- | --- | --- |
| `Hangar` | `Hangar` | `Inicio` | The defeat button is labeled Hangar, and hangar is a Spanish word. Listed in `es-identical-ok.json`. |
| `Cancel` | `Cancelar` | `Anular` | 48px in a 37px box, under the 51px floor. `Anular` would fit and can be read as “wipe”, next to `Sí, borra`. |
| `Close` | `Cierra` | `Cerrar` | 39px in a 32px box, under the 44px floor. Same width as `Cerrar`. `Cierra` is the `tú` form. No shorter close-verb. |
| `Wear` | `Poner` | `Ponte` | 30px in a 29px box. Glossary verb. `Ponte` is the same width. |
| `Claim +100` / `Claim +300` | `Toma +100` / `Toma +300` | `Reclama +100` | `Reclama +100` is 64px in a 59px box. Sentences with room still say `reclama`. |
| `Unlock (1200` family | `Abre (1200` | `Desbloquea (1200` | `Desbloquear` crosses the shrink floor on the price button. The promo sentence still says `Desbloquea`. |
| `✓ UNLOCKED` | `✓ LOGRADO` | `✓ LIBRE` | These chips are achievement states. `Libre` also means free of charge, which is `GRATIS` on the shop badge. |
| `0 / 3 CLEARED` and `CLEARED ✓` | `HECHO` | `SUPERADO` | `0 / 3 SUPERADO` is 68px against a 62px budget. One word on the pill and the badge. |
| `CLAIMED ✓` | `RECIBIDO ✓` | `RECLAMADO ✓` | `RECLAMADO ✓` is 64px in a 62px box. |
| `Nibbles Hats` | `Gorros Nibbles` | `gorros de Nibbles` | The glossary phrase is about 97px and the shrink floor is 96px, so it would be a fit error. |
| `TOY SHOP` | `TIENDA` | `TIENDA DE JUGUETES` | The header box is 84px. The full phrase is in the shop sentence. Dock tab is `Tienda` (33px in a 30px box). |
| `Skies` | `Cielo` | `Cielos` | Plural `Cielos` is 36px in a 30px budget. |
| `Donut Rings` | `Anillos` | `Anillos de dona` | The category chip is 65px. Item names still say `dona`. |
| `Launch Backup Pod (+8 Scrap!)` | `Cápsula de reserva (+8 chatarra)` | a second word for the toast | 192px in a 182px box, under the 253px floor. Button and toast share `cápsula de reserva` and `chatarra`. |
| `Cyber Orbit N` | `Órbita ciber N` | `Ronda ciber N` | About 6px over. `Ronda` fits and means a round, not an orbit. |
| `Horizon Sector N` | `Sector horizonte N` | `Sector límite N` | About 12px over. `sector` is required. `límite` fits and is a weaker place word. |
| `Daily Duty Star` | `Lucero diario` | `Estrella diaria` | `Estrella diaria` is 102px in a 99px box. `Lucero diario` fits the title and the trophy fragment. |
| `100% offline…` | `sin red` | `sin conexión` | `sin conexión` did not fit the heading or the tracker line. |
| `allowance.` | `bolsillo.` | `dinero de bolsillo.` | The fragment box is 55px. Read with the previous run: `Reclama tu +100 de bolsillo.` |
| `care package!` | `¡paquete!` | `¡paquete de regalo!` | The fragment is 72px. The banner title keeps `¡Paquete de regalo!`. |
| `Prize` | `Premio` | `Botín` | 39px in a 34px box, under the 47px floor. |
| `FREE` | `GRATIS` | `LIBRE` | 27px in a 23px box, under the 32px floor. `Libre` is the wrong meaning. |
| `DAILY` | `Diario` | `Turno` | 33px in a 32px box. `Turno` fits and is a single shift, not the daily tab. |
| `DAILY OPERATIONS` | `OPERACIONES DIARIAS` | a shorter title | 158px in a 156px box. The glossary phrase is required. |
| `Kessler Hero` | `Héroe Kessler` | `Kessler` | 88px in an 81px box, under the 112px floor. Dropping either word loses the achievement. |
| `Snacks Eaten` | `Comidas` | `Golosinas comidas` | The eaten header is 79px. `Golosinas recogidas` still fits the collected header. |
| `Privacy Policy` | `Privacidad` | `Política de privacidad` | The long form is a fit error (148px, floor 132px). |
| `chute` names | `rampa` | `tobogán` | `Tobogán lunar` does not fit the Moon Chute cards. One word on every chute. |
| `Reset` | `Borra` | `Reiniciar` | The data button is 30px. `Reiniciar` crosses the floor. `Sí, borra` and `Borra datos` use the same verb. |
| `PLAY` | `JUEGA` | `JUGAR` | Same width. `JUEGA` is the `tú` form on the home button. |

## Glossary terms not used as written

Kept wherever the line can hold them: `chatarra`, `monedas de oro`, `sector`, `mundo`, `nivel`, `conserje cósmico`, `agujero negro`, `trofeo`, `operaciones diarias`, `turno`, `cápsula de reserva`, `asignación diaria`, `basura espacial`. `tienda de juguetes` is in the shop sentence. `Nibbles` is never translated.

| Term | Used instead | Where |
| --- | --- | --- |
| `gorros de Nibbles` | `Gorros Nibbles` | Shop tab. Full phrase is a fit error. |
| `tienda de juguetes` | `TIENDA` | Header and dock only. |
| `tobogán` | `rampa` | Every chute name. |
| `superado` | `hecho` | Cleared pill and `CLEARED ✓`. |
| `desbloquear` | `abre` | Price buttons. The promo line still says `Desbloquea`. |
| `reclamar` | `toma` | `Claim +100` and `Claim +300`. Longer lines still say `reclama`. |
| `dinero de bolsillo` | `bolsillo` | Allowance fragment and gift sentence. |
| `paquete de regalo` | `paquete` | Gift fragment only. Title keeps the full phrase. |
| `cielos` | `cielo` | Skies tab. |
| `anillos de dona` | `anillos` | Donut Rings tab. |
| `singularidad` | `horizonte singular` | `Singularity Horizon` only. `Singularidad Kessler` keeps the word. |

Additions worth a glossary row: `rampa` for chute chips, `tienda` as the header short form, `hecho` for the cleared pill, `abre` for the price button, `toma` for the tight claim buttons, `lucero` for Daily Duty Star.

## English source that looks wrong

- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut off. Spanish does not add the missing `)`.
- Backup Pod and Auxiliary Scrap Pod are one object. Both say `cápsula de reserva`.
- The same coins are called gold coins, recycling coins, and gameplay coins. The balance is `monedas de oro`.
- The same daily gift is stipend, allowance, care package, and janitor stipend.
- `settings_modal_privacy` is out of the app flow. The privacy strings are translated, and the layout collision is ignored.
- `←` stays left-pointing.

## Twelve least-sure keys

1. `Hangar` → `Hangar`
2. `Cancel` → `Cancelar`
3. `Close` → `Cierra`
4. `Nibbles Hats` → `Gorros Nibbles`
5. `TOY SHOP` → `TIENDA`
6. `Launch Backup Pod (+8 Scrap!)` → `Cápsula de reserva (+8 chatarra)`
7. `Claim +100` → `Toma +100`
8. `Unlock (500` → `Abre (500`
9. `0 / 3 CLEARED` → `0 / 3 HECHO`
10. `Cyber Orbit 1` → `Órbita ciber 1`
11. `Horizon Sector 1` → `Sector horizonte 1`
12. `100% offline, kid-safe, no trackers` → `100% sin red, seguro, sin rastreo`

## Kit notes

- A glossary string can be a fit error on the only key that must contain it (`gorros de Nibbles` on `Nibbles Hats`). The checker then warns if the short form drops a word.
- `str` rows still have no line box, so toast width is not fit-checked. Dock badges (`{0} NUEVO`, `REGALO`, `GRATIS`) follow the character hints and are not scored.
- Loanwords that are not glossary rows (`Hangar`) need `es-identical-ok.json`. `merge_table.py` will not keep a language-only `doNotTranslate` entry.
- `cookies` in the privacy paragraph and `pinball` in the nebula levels are loanwords inside a Spanish sentence, not whole-string copies.
