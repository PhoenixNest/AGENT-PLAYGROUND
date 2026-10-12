# European Portuguese notes — pt-PT (Português)

Wording pass 2, 2026-10-12, after the all-language audit. Address is `tu`. European lexicon (`telemóvel`, `pequeno-almoço`, `pastilha`, `escorrega`, `cabaz`, `prenda`, `gasosa`, `hortelã`, `definições`, `navegador`, `separador`). Not Brazilian (`você`, `tela`, `celular`, `usuário` do not appear). No guillemets: no line needed a quotation. No native speaker has reviewed this table. Unity was not run again; paint scales below are the kit estimate, or the audit scale scaled by the change in estimated width.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 51 |
| suppressed fit warnings | 50 |

Visible warnings by kind: `fit` 48, `glossary` 3. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `pt-PT-fit-exceptions.json` (glossary word nível). No `stale`, `glyph`, `length`, or `expansion` warnings.

Pass 1 had 56 visible warnings (`fit` 54, `glossary` 2). This pass removed the 8 `Unlock` fit warnings and the `TOY SHOP` fit warning, and added fit warnings on `Close`, `Claim +100`, and `Claim +300`, plus a glossary warning on `DAILY OPERATIONS`.

## What changed in this pass

| English | Before | After | Fit |
| --- | --- | --- | --- |
| `LOCKED` (16 control ellipses) | `TRANCADO` | `PRESO` | Audit drew `TRANCA…` at 0.72. `PRESO` is 30px against a 50px kit box (scale 1.00). Extrapolated paint 0.86–1.00, no ellipsis. |
| `Close` | `Fecha` | `Fechar` | Kit 39px against a 32px box, scale 0.83, above the 72% floor. `Fecha` painted in full and was the clipped tu form. |
| `Claim +100` / gift button | `Toma +100` | `Recebe +100` | Kit 61px against 59px, scale 0.97. If the old pill was full size, paint is about 0.81. |
| `Claim +300` | `Toma +300` | `Recebe +300` | Kit 63px against 62px, scale 0.98. Paint about 0.81 if the old pill was full size. |
| `Unlock (500`–`3000` and `shop.footer.unlock` | `Desbloquear` | `Abre` | Audit 0.72–0.74 with the whole word still visible. `Abre (500` is 49px against a 67px box (scale 1.00). Longest price the same. |
| `DAILY OPERATIONS` | `OPERAÇÕES DIÁRIAS` | `TAREFAS DIÁRIAS` | Audit 0.76, whole title visible. Extrapolated paint 0.86. Kit box clears. |
| `TOY SHOP` | `BRINQUEDOS` | `BRINQUEDO` | Audit 0.76, whole title visible. Extrapolated paint 0.84. Kit box clears. |
| reset confirm | the long `Não dá para desfazer!` line | `Tens a certeza? Apaga os mapas, trajes e moedas de ouro. Sem volta!` | The still cut it at `Não dá par…` at 0.72. New line extrapolates to about 0.92. Not the privacy modal. |

`Recebe` is the one claim verb on the pills, the gift button, and the longer claim sentences. Shop buttons say `Abre`; the banner and the unlock toast still say `desbloquear`, because those lines fit and `abre` is the wrong verb in a sentence.

## Identical English kept on purpose

Recorded in `pt-PT-identical-ok.json`: `BRONZE`, `BRONZE ✓` (the Portuguese word for the metal), `Hangar`, `fx.turbo.cw`, `fx.turbo.ccw` (`TURBO!`), `fx.hub.donut` (`🍩 DONUT 🍩`, the glossary candy word). `DONUT FLUTUA` keeps the same loanword and is not an identical copy.

Left as copies because they are the brand, the unit, or a symbol, not wording to invent: `ORBITAL`, `SALVAGE`, `SPACE TRASH CLEAN`, `Orbital Salvage • Space Trash Clean v1.2.0`, `✨ ORBITAL ✨`, the `kg` lines, emoji, and placeholders. `✓` and the face emoji are the same in every language.

## Doubtful lines

| Key | Choice | Alternative |
| --- | --- | --- |
| `Close` | `Fechar` | `Fecha` is the tu imperative and fits at full size. It reads as a clipped word. `Fechar` is the full infinitive, scale about 0.83. |
| `Claim +100` / `Claim +300` | `Recebe +100` / `Recebe +300` | `Toma` fit the pill and is the wrong verb. `Recebe` is the glossary tu form and is 1–2px over the kit box. |
| `LOCKED` | `PRESO` | `TRANCADO` ellipsized to `TRANCA…`. `FECHADO` is still long enough to risk the same cut. |
| `Try Again` | `Outra vez` | `Repete` also fits. `Outra vez` is what you say to a child after a miss. It is not a finite verb. |
| `Wear` | `Usar` | `Vestir` is the glossary verb and overflows the 29px button. |
| `Nibbles Hats` | `Chapéus Nibbles` | `Chapéus do Nibbles` is a fit error (about 103px against a 96px floor). |
| `TOY SHOP` | `BRINQUEDO` | Plural `BRINQUEDOS` painted at 0.76. `Loja de brinquedos` is a fit error. Singular clears about 0.84. |
| `DAILY OPERATIONS` | `TAREFAS DIÁRIAS` | `OPERAÇÕES DIÁRIAS` painted at 0.76 and is the checked phrase. The shorter synonym clears about 0.86 and warns on glossary. |
| `Unlock` buttons | `Abre` | `Desbloquear` painted in full at 0.72–0.74. `Abre` is the tu imperative and sits at full size. Sentences keep `desbloquear`. |
| `FREE` | `GRÁTIS` | No shorter word means free of cost. `Livre` would mean unoccupied. |
| `Cute Cosmic Janitor` | `Auxiliar cósmico de limpeza` | The glossary phrase is stiff for a child and wider than the badge. Dropping words would miss the checked term. |
| `Launch Backup Pod (+8 Scrap!)` | `Lançar cápsula de reserva (+8 sucata!)` | The verb plus both checked words shrinks. Dropping `Lançar` still does not clear the box. |
| `Bubblegum Chute N` | `Escorrega pastilha N` | `Goma` fits and is the wrong candy. The map still shows the full name. |
| `Moon Chute N` | `Escorrega lua N` | Same slide word as the other chutes. |
| `0 / 3 CLEARED` and `CLEARED ✓` | `FEITO` | `Concluído` does not fit the daily pill. |
| privacy body | left as written | Not tuned. The three body ellipses are the privacy modal. |

`settings_modal_privacy` is translated and not tuned. That collision is the layout already reported on German.

## Glossary terms not used as written

| Term | What the table says | Why |
| --- | --- | --- |
| `operações diárias` | `Tarefas diárias` | The checked phrase painted at 0.76 on the daily header and the card. |
| `chapéus do Nibbles` | `Chapéus Nibbles` | Full phrase is a fit error on the tab. |
| `loja de brinquedos` on `TOY SHOP` | `Brinquedo` | Full phrase is a fit error. The shop sentence still says `loja de brinquedos`. |
| `vestir` | `Usar` | Button box. |
| `concluído` | `Feito` | Daily pill. One word on the pill and the badge. |
| `singularidade` on `Singularity Horizon` | `Horizonte singular` | Header box. |
| `desbloquear` on the price button | `Abre` | The button sat at the 72% floor. The banner and the toast still say `desbloquear`. |

Checked terms that do appear, and still warn on fit: `nível` (LVL chips, suppressed), `auxiliar cósmico de limpeza`, `cápsula de reserva`, `escorrega` on the chute chips, `sucata` on the backup button. `receber` now appears on the claim pills.

## Every visible warning left

| Keys | n | Kind | Why it stays |
| --- | ---: | --- | --- |
| `LVL 1`–`LVL 50` | 50 | fit, suppressed | Glossary word nível. |
| `Bubblegum Chute 1`–`10` | 10 | fit | `Escorrega pastilha`. About 12px over. `Pastilha` is the candy word. The map still shows the full name. |
| `Moon Chute 1`–`10` | 10 | fit | `Escorrega lua`. About 12px over. Same slide word. |
| `Cyber Orbit 1`–`10` | 10 | fit | `Ciberórbita`. About 2px over. |
| `Horizon Sector 1`–`9` | 9 | fit | `Setor horizonte`. About 6px over. `Setor` is required. |
| `Close` | 1 | fit | `Fechar`, 39px against 32px. Above the 72% floor. |
| `Claim +100` | 1 | fit | `Recebe +100`, 61px against 59px. Glossary verb. |
| `Claim +300` | 1 | fit | `Recebe +300`, 63px against 62px. Same verb. |
| `Cute Cosmic Janitor` | 1 | fit | Glossary phrase. |
| `Launch Backup Pod (+8 Scrap!)` | 1 | fit | Verb plus `cápsula de reserva` and `sucata`. |
| `Nibbles Hats` | 1 | fit and glossary | `Chapéus Nibbles`. The full phrase does not fit. |
| `FREE` | 1 | fit | `GRÁTIS`. |
| `Kessler Hero` | 1 | fit | `Herói Kessler`. |
| `Prize` | 1 | fit | `Prémio`. |
| `DAILY OPERATIONS` | 1 | glossary | `Tarefas diárias` so the header clears about 0.86. |
| `TOY SHOP` | 1 | glossary | `Brinquedo`. `Loja de brinquedos` does not fit the header. |

## Twelve least-sure keys

1. `Hangar` → `Hangar` (Portuguese word, identical on purpose)
2. `Close` → `Fechar` (infinitive; the tu form `Fecha` was the clipped button)
3. `Claim +100` → `Recebe +100` (1–2px over; the `+300` pill matches)
4. `LOCKED` → `PRESO`
5. `DAILY OPERATIONS` → `TAREFAS DIÁRIAS` (drops the checked phrase)
6. `TOY SHOP` → `BRINQUEDO` (singular, so the header clears 80%)
7. `Unlock (500` → `Abre (500` (sentences still say `desbloquear`)
8. `Nibbles Hats` → `Chapéus Nibbles`
9. `FREE` → `GRÁTIS`
10. `Cute Cosmic Janitor` → `Auxiliar cósmico de limpeza`
11. `Bubblegum Chute 1` → `Escorrega pastilha 1`
12. `BRONZE` → `BRONZE` (the Portuguese word)

## Kit notes

- `nível` is not a substring of `níveis`, so a grammatical plural does not satisfy the checker.
- `loja de brinquedos` and `chapéus do Nibbles` are fit errors on the header and the tab. The shortened forms warn on glossary instead.
- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are still cut off in English. The translation does not invent the closing `)`.
- `str` rows have no line box, so toast width is not fit-checked.
- Identical loanwords are accepted through `pt-PT-identical-ok.json`. `BRONZE` and `🍩 DONUT 🍩` are also glossary loanwords, so the checker allows them even before that file. The file still records the reason.
- The privacy modal was not edited.
