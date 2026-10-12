# European Portuguese notes — pt-PT (Português)

Draft for 2026-10-12. Address is `tu`. European lexicon (`ecrã` was not required by any source line; `telemóvel`, `pequeno-almoço`, `pastilha`, `escorrega`, `cabaz`, `prenda`, `gasosa`, `hortelã`, `definições`, `navegador`, `separador`). Not Brazilian (`você`, `tela`, `celular`, `usuário` do not appear). No guillemets: no line needed a quotation. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-pt-PT/review-pt-PT.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 56 |
| suppressed fit warnings | 50 |

Visible warnings by kind: `fit` 54, `glossary` 2. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `pt-PT-fit-exceptions.json` (glossary word nível). No `stale`, `glyph`, `length`, or `expansion` warnings. Accepted identical copies: `Hangar`, `fx.turbo.ccw`, `fx.turbo.cw`.

## Doubtful lines

| Key | Choice | Alternative |
| --- | --- | --- |
| `Close` | `Fecha` | `Fechar` is the usual button infinitive and overflows the 32px control. `Fecha` is the tu imperative and fits. |
| `Claim +100` / `Claim +300` | `Toma +100` / `Toma +300` | `Recebe` is the glossary verb and is 1–2px over these pills. Longer lines still say `Recebe` / `Recebeste`. |
| `Try Again` | `Outra vez` | `Repete` also fits. `Outra vez` is what you say to a child after a miss. It is not a finite verb. |
| `Wear` | `Usar` | `Vestir` is the glossary verb and overflows the 29px button. |
| `Nibbles Hats` | `Chapéus Nibbles` | `Chapéus do Nibbles` is a fit error (about 103px against a 96px floor). |
| `TOY SHOP` | `BRINQUEDOS` | `Loja de brinquedos` is a fit error. `LOJA` fits and matches the dock, and drops “toy”. Plural is 1px over the header. |
| `FREE` | `GRÁTIS` | No shorter word means free of cost. `Livre` would mean unoccupied. |
| `Cute Cosmic Janitor` | `Auxiliar cósmico de limpeza` | The glossary phrase is stiff for a child and wider than the badge. Dropping words would miss the checked term. |
| `Launch Backup Pod (+8 Scrap!)` | `Lançar cápsula de reserva (+8 sucata!)` | The verb plus both checked words (`cápsula de reserva`, `sucata`) shrinks. Dropping `Lançar` still does not clear the box. |
| `Bubblegum Chute N` | `Escorrega pastilha N` | `Goma` fits and is the wrong candy (eraser / glue). `Pastilha` is the European word and shrinks the chip. |
| `Moon Chute N` | `Escorrega lua N` | `Deslize` fits. `Escorrega` is the one slide word used everywhere else, and it shrinks. |
| `Unlock (1200` (and the other prices) | `Desbloquear (1200` | `Abrir` fits and reads as “open”. The verb stays under the 72% floor by about 3px on the longest price. |
| `0 / 3 CLEARED` and `CLEARED ✓` | `FEITO` | `Concluído` is the glossary preference and does not fit the daily pill. The badge uses the same short word. |
| `Snacks Eaten` | `Lanche comido` | `Lanches comidos` is 1px over. The pair label is `Lanches apanhados`. |
| `First Nom Nom!` | `Nham nham!` | `Primeiro nham nham!` does not fit. The eating sound is the trophy. |
| `Gold Shift • Vortex Apex` | `Turno ouro • Ápice` | `Ápice do vórtice` does not fit. |
| `Bronze Shift • Cadet Sweep` | `Turno bronze • Cadete` | The sweep word does not fit beside `turno` and `cadete`. |
| `Nebula Pinball` | `Nebulosa` | Levels are `Flíper N`. The world title cannot hold both words. |
| `Singularity Horizon` | `Horizonte singular` | `Singularidade` does not fit this header. `A singularidade Kessler` uses the full noun. |
| `Privacy Policy` | `Política` | `Política de privacidade` does not fit. The section title is `Privacidade`. |
| `Game Data` | `Dados` | `Dados do jogo` is 2px over. The subtitle says what is erased. |
| privacy body | `cookies` kept | `Testemunhos` is the old coinage. Portuguese privacy notices say `cookies`. Out of the app flow. |
| `STARTER FREE` | `GRÁTIS` | Same word as `FREE`. The badge’s point is that the starter costs nothing. |

`settings_modal_privacy` is translated and not tuned for the heading/body collision. That collision is the layout already reported on German.

## Glossary terms not used as written

| Term | What the table says | Why |
| --- | --- | --- |
| `chapéus do Nibbles` | `Chapéus Nibbles` | Full phrase is a fit error on the tab. |
| `loja de brinquedos` on `TOY SHOP` | `BRINQUEDOS` | Full phrase is a fit error. The shop sentence still says `loja de brinquedos`. |
| `vestir` | `Usar` | Button box. |
| `receber` on the coin pills | `Toma` | Pill box. Other claim lines say `Recebe`. |
| `concluído` | `Feito` | Daily pill. One word on the pill and the badge. |
| `singularidade` on `Singularity Horizon` | `Horizonte singular` | Header box. |

Checked terms that do appear, and still warn on fit: `nível` (LVL chips, suppressed), `auxiliar cósmico de limpeza`, `cápsula de reserva`, `escorrega` on the chute chips, `sucata` on the backup button.

## Glossary additions requested

- A short child-facing job title beside `auxiliar cósmico de limpeza`.
- Tab form `Chapéus Nibbles` when `chapéus do Nibbles` does not fit.
- Header form `Brinquedos` when `loja de brinquedos` does not fit.
- Short claim verb `toma` for the `+100` / `+300` pills.
- `usar` when `vestir` does not fit a button.
- The checker looks for the substring `nível`, which is not inside `níveis`. Plurals had to be rephrased (`cada nível`).

## English source lines that look wrong

- `Unlock (1200` (and the other prices) and `Trophy: Daily Duty Star (+500` are cut off before the closing `)`. The translation does not invent it.
- Backup Pod and Auxiliary Scrap Pod are one object. Both say `cápsula de reserva`.
- Recycling coins, gameplay coins, and gold coins are one balance. The table says `moedas de ouro`.
- Stipend and allowance are one gift. The header says `subsídio diário` and the banner says `mesada`, as the glossary splits them.
- `FREE` and `STARTER FREE` both become `GRÁTIS`.

## Accepted fit warnings

All of these stay under the 72% shrink floor. A shorter string would drop the glossary word or change the meaning.

| Keys | n | Decision |
| --- | ---: | --- |
| `LVL 1`–`LVL 50` | 50 | Suppressed. Glossary word nível. |
| `Bubblegum Chute 1`–`10` | 10 | `Escorrega pastilha`. About 12px over. `Pastilha` is the candy word. |
| `Moon Chute 1`–`10` | 10 | `Escorrega lua`. About 12px over. Same slide word as the other chutes. |
| `Cyber Orbit 1`–`10` | 10 | `Ciberórbita`. About 2px over. |
| `Horizon Sector 1`–`9` | 9 | `Setor horizonte`. About 6px over. `Setor` is required. |
| `Unlock (500`–`Unlock (3000` | 8 | `Desbloquear`. Longest price is about 97px against a 100px floor. |
| `Cute Cosmic Janitor` | 1 | Glossary phrase. |
| `Launch Backup Pod (+8 Scrap!)` | 1 | Verb plus `cápsula de reserva` and `sucata`. |
| `Nibbles Hats` | 1 | `Chapéus Nibbles`. Also a glossary warning for the full phrase. |
| `TOY SHOP` | 1 | `BRINQUEDOS`, 1px over. Also a glossary warning. |
| `FREE` | 1 | `GRÁTIS`. |
| `Kessler Hero` | 1 | `Herói Kessler`. |
| `Prize` | 1 | `Prémio`. |

## Twelve least-sure keys

1. `Hangar` → `Hangar` (Portuguese loanword, identical on purpose)
2. `Close` → `Fecha`
3. `Claim +100` → `Toma +100` (the `+300` pill matches; sentences still say `Recebe`)
4. `Try Again` → `Outra vez`
5. `Nibbles Hats` → `Chapéus Nibbles`
6. `TOY SHOP` → `BRINQUEDOS`
7. `FREE` → `GRÁTIS`
8. `Cute Cosmic Janitor` → `Auxiliar cósmico de limpeza`
9. `Launch Backup Pod (+8 Scrap!)` → `Lançar cápsula de reserva (+8 sucata!)`
10. `Bubblegum Chute 1` → `Escorrega pastilha 1`
11. `Moon Chute 1` → `Escorrega lua 1`
12. `Unlock (1200` → `Desbloquear (1200`

## Kit notes

- `nível` is not a substring of `níveis`, so a grammatical plural does not satisfy the checker.
- `loja de brinquedos` and `chapéus do Nibbles` are fit errors on the header and the tab. The shortened forms warn on glossary instead.
- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are still cut off in English.
- `str` rows have no line box, so toast width is not fit-checked.
- Identical loanwords that are not glossary rows (`Hangar`, `turbo`) are accepted through `pt-PT-identical-ok.json`. Merge recopies `doNotTranslate` from English and does not need a language-specific entry.
