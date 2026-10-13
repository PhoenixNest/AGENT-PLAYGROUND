# Japanese notes — ja (日本語)

2026-10-13, lead-review pass. Sentences use です/ます. Buttons and chips are short. Nibbles stays in Latin letters. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/review/review-ja.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 187 |
| suppressed fit warnings | 0 |
| accepted identical copies | 0 |

Visible warnings by kind: `fit` 136, `glossary` 51. The 51 glossary warnings are the 50 `LVL` chips (they say `LvN`; sentences still say `レベル`) and `TOY SHOP` (`おもちゃや`). There is no `ja-identical-ok.json`.

## Level chips

Every chip is `Lv1`–`Lv50`. Measured with `/tmp/maze-i18n/fonts/out/mplus-ja-joyo.ttf` at the captured 11px size. Kana and kanji advance 1.00em. `L` is 0.599em, `v` is 0.540em, and each digit is 0.640em.

`レベル` plus the number is 40.04px for one digit and 47.08px for two. The safe line is the captured width divided by 0.72, then 3% under that. `レベル` misses that line on 26 chips: `LVL 2`–`LVL 19`, `LVL 25`, `LVL 31`, `LVL 35`, `LVL 45`, `LVL 46`, `LVL 47`, `LVL 49`, `LVL 50`. The tightest is `LVL 11`: box 31.453px, floor 43.68px, 3% line 42.37px, and `レベル11` is 47.08px.

`Lv` plus the number is 19.57px for one digit and 26.61px for two. `LVL 11` at 26.61px is 39% under its floor. No chip is within 5% of its floor.

Mission titles and other sentences keep `レベル` (`レベル1•バブルガムシュート1`). That glossary exception is also in `ja-fit-exceptions.json`. The chip keys are listed there. They do not fit-warn, so the suppressed count is 0.

`Lv1` has no kana. `validate_table.py` allows exactly `Lv` plus one or two digits, and still errors on any other Japanese string that has no kana or kanji.

## Kid-facing wording

`YUM! ✨` is `おいしい！✨`. `TASTY! ⭐` is also `おいしい`, with a different emoji.

`NIBBLES IS FULL! YAY!` is `Nibblesはおなかいっぱい！`. Adding `やった` puts the real width at 333px, past the 302px floor. The chosen line is 253px, 16% under that floor, and wider than the 217px title box, so the title shrinks.

`Lost In Void` is `そらのまいご` (66.0px in a 66.7px box, 29% under the floor). `Nibbles the Void` is `そらのNibbles`.

`Silver Shift • Sector Hazard` is `シルバーシフト•セクターきけん`. Real width 210px, floor 246px (14% under). It is wider than the 177px card title. `シフト` and `セクター` stay.

Other child-facing labels and blurbs whose kanji is above about grade 3 moved to kana when the kana stayed at least 3% under the real-font shrink floor. Buttons stayed inside their boxes. Examples: `獲得コイン` → `もらったコイン`, `砂糖ドーナツ` → `さとうドーナツ`, `装備中` → `きてる`, `効果音の音量` → `こうかおんのおおきさ`, `ゴールドシフト•渦の頂上` → `ゴールドシフト•うずのうえ`, `無料` → `ただ` on the free badges.

Left in kanji because the kana misses the 3% line or the box:

| English | Kept | Why |
| --- | --- | --- |
| `Goal: N kg` and the quota lines | `目標` | `もくひょう` is past the floor on the quota pills |
| `Kessler Hero` | `ケスラー勇者` | `ゆうしゃ` is 112px, past the 107px 3% line |
| `View Policy` | `規約をみる` | `きやくをみる` is wider than the button |
| vibration sublabel | `振動します` | `しんどう` pushes the line past the floor. The short label is `しんどう` |
| black-hole blurb | `お腹` | the line is already 468px against a 475px floor |
| `Cute Cosmic Janitor` and the 1,000-item line | `宇宙のおそうじ屋さん` | glossary phrase |
| privacy paragraphs | legal kanji | parent-facing, and already past the real-font floor |
| toy-shop money sentence | `購入` | parent-facing, and already past the real-font floor |

## Glyphs

No kanji outside the Jōyō list. Checked against `mplus-ja-joyo.ttf` (2,566 glyphs). Every character that is not an emoji is in that font. Emoji stay the English ones and are absent from M PLUS. `mplus-ja-800.ttf` is the short subset and was not used as the face.

## Other choices

ASCII `(` `)` stay, including the cut-off `Unlock (` and trophy fragments. A katakana middle dot `・` stands in front of `+` when the previous character would be kana. `kg`, Western digits, `×`, `▶`, `←`, `/`, `%`, emoji, and `✓` stay as in English.

`おもちゃやさん` is still `おもちゃや` on the `TOY SHOP` header (119px against a 116px floor). The sentence about the shop uses `おもちゃやさん`.

## Two labels that ellipsized

Measured on `Assets/Fonts/MPLUSRounded1c-700.ttf` (upem 1000, kana and kanji 1.00em). Every character in these two strings is in that subset. The sparkle on the shop title is the English emoji, drawn as a sprite.

| English | New | Width | Box | Floor | Under the floor |
| --- | --- | ---: | ---: | ---: | ---: |
| `GOLD` | `金` | 10.0px at 10px | 33.1px | 46.0px | 78% |
| `Allowance Claimed!` | `おこづかいゲット！` | 121.5px at 13.5px | 128.3px | 178.1px | 32% |

`金` is the one-character gold-medal label. `ゴールド` was 40px and the daily chip painted `ゴー…`. `GOLD ✓` stays `ゴールド✓` because that wider chip already fits. `ゴールドコイン` and `ゴールドシフト` are unchanged.

The claim title fits the 128px box at full size. The shop string `gift.title.claimed` is the same words plus `✨` (about 135px if the sparkle is 1em). That is a small shrink, still about 32% above the 72% floor, so it does not ellipsize. The banner sentences stay `まいにち・+100のおこづかい。` and `あしたも・+100のケアパッケージ！`.

## English source

- `Unlock (1200` and the other `Unlock (` lines, and `Trophy: Daily Duty Star (+500`, are cut off. The translations stay open.
- The privacy paragraph still talks about browser `localStorage`, tracking SDKs, cookies, and behavioral ads.
- The version subtitle still says `Pure Gesture Mobile Web Puzzle`.
- `LVL 11`'s captured width is about 2px narrower than `LVL 10` and `LVL 12`.

## Real-font shrink floor

These lines are within 5% of the shrink floor on `mplus-ja-joyo.ttf`, or past it. The estimator still reports them as fit warnings, because it counts Latin at 0.50em and full-width punctuation at 0.32em. This pass did not widen them.

| English | Real px | Floor px | Ratio |
| --- | ---: | ---: | ---: |
| `All 3 shifts completed! Claim your reward!` | 249 | 241 | 1.04 |
| `Clean up Bronze Shift • Cadet Sweep!` | 280 | 271 | 1.03 |
| `Nibbles is still hungry! Out of orbital scrap!` | 387 | 389 | 1.00 |
| black-hole blurb (`お腹`) | 468 | 475 | 0.99 |
| `Nibbles Hats` | 92 | 94 | 0.97 |
| vibration sublabel | 275 | 285 | 0.96 |

The two privacy paragraphs, the reset confirmation, and the toy-shop money sentence are also past the real-font floor. They wrap, and the estimator still has them under its floor.

## Validator

`validate_table.py --lang ja` on `ja.json`: 0 errors, 187 warnings, 0 suppressed. Warnings are 136 `fit` and 51 `glossary`. The shipped face is `MPLUSRounded1c-700.ttf`, so the old `font-pending` warning is gone. The fit count rose because the estimator now uses that face's advances.
