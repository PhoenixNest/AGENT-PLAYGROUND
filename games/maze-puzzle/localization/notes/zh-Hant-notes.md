# Traditional Chinese notes — zh-Hant (繁體中文)

Taiwan wording, address 你. Full-width `，。！？、：`. ASCII `(` `)` kept where English has them, including the cut-off shop and trophy fragments. Western digits. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/review/review-zh-Hant.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 45 |
| suppressed fit warnings | 2 |
| accepted identical copies | 0 |

Visible warnings by kind: `expansion` 44, `font-pending` 1. The font-pending line is `Huninn-Regular.ttf`. The 44 expansion warnings are copies that have to stay identical to English (kg lines, logo lines, emoji, and `{0}` templates). They are 1.0× English, and the zh-Hant warn line is 0.88×, so a legal copy always warns. No translated sentence is in that list.

`zh-Hant-fit-exceptions.json` lists `LVL 1`–`LVL 50`. The estimator currently warns only on `LVL 11` (33px vs 31px) and `LVL 15` (33px vs 33px). Both are under the 72% shrink floor. The other 48 chips are inside the captured box, a little wider than `LVL n` because `關卡` is the glossary word.

There is no `zh-Hant-identical-ok.json`. Logo lines, kg strings, emoji-only strings, and unit templates stay English because the validator already allows them.

## Doubtful lines

| English | Choice | Alternative |
| --- | --- | --- |
| `Hangar` | 機庫 | 停機坪 |
| `Lost In Void` | 迷失虛空 | 掉進虛空 |
| `Party Popper Hole` | 拉炮黑洞 | 派對黑洞 |
| `Silver Shift • Sector Hazard` | 銀班次•扇區危機 | 銀班次•扇區險境 |
| `The hungriest… big anime eyes! Nom nom!` | 眼睛又大又圓！啊嗚啊嗚！ | 大大的動漫眼睛 |
| `SORT RIG` (`fx.hub.sort`) | 整理環 | 分類環 |
| `Cyber Orbit 1` | 電子軌道1 | 賽博軌道1 |
| `Switchbacks & One-Way Flappers` | 折返彎道和單向小門 | 折返和單向拍板 |
| `Awesome, Got It!` | 好，知道了！ | 太好了！ |
| `First Nom Nom!` | 初次啊嗚啊嗚！ | 第一次啊嗚 |

`初次啊嗚啊嗚！` is the short form that fits the 96px box. `第一次啊嗚啊嗚！` was 102px.

## Glossary

Checked terms are in the lines that contain them. Validator glossary warnings: 0.

`Nibbles Hats` is `Nibbles 帽子`, with the space the glossary string has. Every other Latin-Chinese join has no space (`關卡1`, `Nibbles吃飽啦`).

`+` and `kg` are boundary tokens in the validator. A Chinese character stuck to them is not counted, so those lines keep a space: `領取 +100`, `20 kg 太空點心`.

`NOM!` is the short eating burst `啊嗚！`. The full `nom nom` lines use `啊嗚啊嗚`.

`軟體` and `影片` do not appear. The English source never says software or video. `螢幕` is in `滑螢幕就能轉！`. `首頁` is `回首頁`. `設定` is `遊戲設定`.

`失敗` is not used. The fail banner is `班次還沒完成！`.

## English source

- The privacy body and `Pure Gesture Mobile Web Puzzle` still say browser, `localStorage`, and web. This build is a mobile game.
- The button says Backup Pod and the toast says Auxiliary Scrap Pod. Both are `備用艙`.
- The balance is called gold coins, recycling coins, and gameplay coins. All three are `金幣`.
- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut off on purpose. The closing `)` is another run. Counts match English.
- `Goal: Feed Nibbles 20 kg (0/20 kg eaten)` writes `0/20` with no spaces. The HUD quotas use `0 / 20`.
- `big anime eyes` and `Fiesta vortex` are left as meaning, not as loanwords.
- `SHIFT INCOMPLETE!` is harsher than the brief's gentle fail. The translation says the shift is not finished yet.

## Glyphs

Checked against `/tmp/maze-i18n/fonts/out/huninn-tc-big5.ttf` and `/tmp/maze-i18n/fonts/charset/tc-big5-frequent.txt`.

No Han character is outside the Big5 frequent set. No Han character is missing from Huninn.

Huninn has no glyph for `✓`, `▶`, or the emoji copied from English (`⏩✨🧲💥⚡🍬🍩🦆⚙️🐱💨😋⭐⏪🪙🎉🎁🥰💤🍓🔇🔊🚀🏆🧹`, plus U+FE0F on the gear). Those are already in the English table. The CJK chain is still pending, so the validator does not fail them.
