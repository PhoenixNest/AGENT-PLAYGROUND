# Korean notes — ko (한국어)

2026-10-13. 해요체 for sentences (`~해요`, `~예요/이에요`). Buttons and chips are short nouns or verb nouns (`닫기`, `시작`, `홈`). No 반말, no 합니다체, no Hanja. Nibbles stays `Nibbles`. Western digits. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/review/review-ko.md` and is not marked done.

## Coverage

|                           | Count |
| ------------------------- | ----: |
| spec keys                 |   355 |
| str keys                  |    81 |
| validator errors          |     0 |
| visible warnings          |     1 |
| suppressed fit warnings   |    48 |
| accepted identical copies |     0 |

The visible warning is `font-pending` (`Jua-Regular.ttf`). Suppressed fit warnings: 47 `LVL` chips plus `TOY SHOP`. `ko-fit-exceptions.json` also lists `LVL 1`, `LVL 21`, and `LVL 41`; those three do not warn at the current widths. No `ko-identical-ok.json`. Brand lines, `kg` strings, and emoji-only strings stay English because the validator already allows them.

## Glyphs

Every Hangul syllable in the table is in `/tmp/maze-i18n/fonts/out/jua-kr-ks.ttf` (KS X 1001, 2350 syllables). No syllable outside that set. `jua-kr-800.ttf` has only 800 Hangul and is missing ordinary syllables (`먹`, `모`, and many others). The font chain should ship the KS face, not the 800 cut.

Jua has no U+0060. None used. The English ellipsis is three ASCII periods; U+2026 is not in Jua, so those stayed `...`. Emoji, `×`, `•`, `←`, `▶`, and `✓` are not in Jua. They are kept for the symbol fallback. `← 뒤로` still points left, which is right for left-to-right Korean.

## Doubtful lines

| Key                                    | Choice                                                                          | Alternative                                                                                                                                          |
| -------------------------------------- | ------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `SHOP`                                 | `상점`                                                                          | `가게`. The dock is about 30px. `장난감 가게` is a fit error there. The screen title stays `장난감 가게`.                                            |
| `Unlock (500` and `shop.footer.unlock` | `해제 (500`, `해제 ({0} 🪙)`                                                    | `잠금 해제 (500`. That is about 77px in a 67px button. It stays under the 72% floor, but it is wider than the line. Sentences still say `잠금 해제`. |
| `3 NEW`, `dock.daily.new`              | `신규 3`, `신규 {0}`                                                            | `남음 3`. The English word is NEW. The key note says `{0}` is how many shifts are left.                                                              |
| `Hangar`                               | `격납고`                                                                        | `창고`. `격납고` fits the 43px button (3 syllables). `우주 창고` does not.                                                                           |
| `SHIFT INCOMPLETE!`                    | `근무가 남았어요!`                                                              | `근무를 아직 못 끝냈어요!` The longer line was 223px in a 199px modal title.                                                                         |
| `Coins Won`                            | `받은 코인`                                                                     | `받은 골드 코인`. The stat label is about 61px. The glossary phrase does not fit. The English here does not say gold coins.                          |
| `DONUT FLOAT`, `home.ring.donut`       | `도넛 둥실`                                                                     | `도넛 플로트`                                                                                                                                        |
| `Lost In Void`                         | `길 잃은 허공`                                                                  | `허공에 길을 잃음`                                                                                                                                   |
| `Party Popper Hole`                    | `파티 폭죽 구멍`                                                                | `파티 폭죽 홀`                                                                                                                                       |
| `Horizon Sector N`                     | `지평선 섹터`                                                                   | `수평선 섹터`                                                                                                                                        |
| Gift banner fragments                  | `지금 받아요, 무료 +100` + `용돈.` and `내일 다시 와요, 또 +100` + `돌봄 상자!` | `용돈이에요.` does not fit the 55px second run.                                                                                                      |

`Nibbles` is read as 니블스 (no batchim), so `Nibbles를` in the breakfast line takes `를`. The hungry line avoids a particle: `아직 배고파요, Nibbles!`

`daily.countdown` and `Resets in 14h 59m` keep Latin `h` and `m`. The key note says to keep those units.

## Glossary

No `check: yes` warnings. Terms not used as the full glossary word:

| Term          | Where                                         | What I used         | Addition worth having                                          |
| ------------- | --------------------------------------------- | ------------------- | -------------------------------------------------------------- |
| `잠금 해제`   | Price buttons and `shop.footer.unlock`        | `해제`              | Short button verb next to `잠금 해제`.                         |
| `장난감 가게` | Dock `SHOP` only                              | `상점`              | A 2-syllable dock word. The header already uses the full term. |
| `받기`        | Past or soft sentences (`받았어요`, `받아요`) | button stays `받기` | Past form will not contain `받기`.                             |
| `장착됨`      | `toast.shop.equipped`                         | `장착했어요! ✨`    | Badge stays `장착됨`.                                          |

`Galaxy Janitor King` is `은하 청소왕`. The English does not say cosmic janitor, so `우주 청소부` is not required there. `1,000 items recycled!` does use `우주 청소부` for "cosmic cleaner".

`recycling coins` and `gameplay coins` are `골드 코인`, same as `gold coins`. Backup Pod and Auxiliary Scrap Pod are both `예비 포드`.

## English source

- `Unlock (1200` (and the other prices) and `Trophy: Daily Duty Star (+500` are cut off. The closing `)` is a later run. Counts were matched, not repaired.
- One spare pod has two English names: `Launch Backup Pod` and `Auxiliary Scrap Pod Deployed`.
- The coin is `gold coins`, `recycling coins`, and `gameplay coins` in different sentences.
- The mascot is `Nibbles` in some strings and `NIBBLES` in `NIBBLES IS FULL! YAY!`.
- `Orbital Salvage • Space Trash Clean v1.2.0` is followed by the fragment `Pure Gesture Mobile Web Puzzle`.
- `Goal: Feed Nibbles 20 kg (0/20 kg eaten)` repeats the quota inside the goal.

## Validator

`validate_table.py --lang ko` on this table: errors 0, warnings 1 (`font-pending`), suppressed 48. Glyph check does not run while the chain is pending. The manual cmap check against `jua-kr-ks.ttf` found no missing Hangul.
