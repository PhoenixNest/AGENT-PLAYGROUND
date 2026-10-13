# Japanese notes — ja (日本語)

2026-10-13. Sentences use です/ます. Buttons and chips are short nouns or stems (とじる, もどる, あそぶ, きる, みる). No だ/である and no slang. Nibbles stays in Latin letters. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/review/review-ja.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 87 |
| suppressed fit warnings | 49 |
| accepted identical copies | 0 |

Visible warnings by kind: `fit` 84, `glossary` 2, `font-pending` 1. The 49 suppressed warnings are `LVL 1`–`LVL 10` and `LVL 12`–`LVL 50` in `ja-fit-exceptions.json`. There is no `ja-identical-ok.json`. Strings that stay in English are the logo, `kg` lines, emoji-only strings, and `{0} • {1}`. The validator already allows those.

## Glyphs

No kanji outside the Jōyō list. Checked against `/tmp/maze-i18n/fonts/out/mplus-ja-joyo.ttf` (2,566 glyphs). Every character that is not an emoji is in that font, including kana, the Jōyō kanji used here, `・`, `…`, and full-width `。！？、：`.

The emoji are the ones already in English (`⏩`, `🪙`, and the rest). They are not in M PLUS. `mplus-ja-800.ttf` has only 822 glyphs and is missing many Jōyō kanji and the full-width punctuation, so it was not treated as the JIS face.

## Doubtful lines

| English | Choice | Alternative |
| --- | --- | --- |
| `LVL 11` | `11面` | `レベル11` |
| `Lost In Void` | `宙のまいご` | `そらのまいご` |
| `Silver Shift • Sector Hazard` | `シルバーシフト•セクター注意` | `シルバーシフト•セクターきけん` |
| `NIBBLES IS FULL! YAY!` | `Nibblesは満腹！やった！` | `Nibblesはおなかいっぱい！` |
| `YUM! ✨` | `うまい！✨` | `おいしい！✨` |
| `SHOP` | `お店` | `ショップ` |
| `Reset` / `Yes, Reset` | `もどす` / `はい、もどす` | `リセット` |
| `Prize` | `ほうび` | `ごほうび` |
| `Claim +100` | `+100もらう` | `受け取る+100` |
| `Cyber Donut Rings` | `サイバードーナツ` | `サイバードーナツリング` |

`LVL 11` is the one glossary miss on `level`. `レベル11` estimates at 44.0px. That chip's shrink floor is 43.68px (captured box 31.45px). `LVL 10` and `LVL 12` are about 33.5px wide and `レベル10` / `レベル12` stay under their floors, so those chips use `レベル`. `11面` is only on that one chip.

`ショップ` on the `SHOP` dock is 44px against a 41px floor. `お店` fits. `リセット` on `Reset` is past the floor; `もどす` is the short verb. `ごほうび` on the `Prize` chip is past the floor; `ほうび` is the same word without `ご`. `受け取る+100` is wider than the claim button, and a kana glued to `+` is not counted as the `+` token, so the button is `+100もらう`. `サイバードーナツリング` is 165px against a 174px floor and a 125px box; the item name drops `リング`. The shop tab `Donut Rings` still says `ドーナツリング`.

`うまい！` is the yum shout, so it is not the same word as `おいしい！` (`TASTY`). `Hazard` is `注意` so the silver shift does not sound scary. `満腹` is there because `おなかいっぱい` plus `やった` passes the shrink floor on that banner.

## Glossary terms not used as written

| Term | Where | What was used | Why |
| --- | --- | --- | --- |
| `おもちゃやさん` | `TOY SHOP` header | `おもちゃや` | `おもちゃやさん` is 119px; the floor is 116px. The sentence about the shop uses `おもちゃやさん`. |
| `レベル` | `LVL 11` only | `11面` | See above. The other 49 chips use `レベル`. |
| `ごほうび` | `Prize` chip | `ほうび` | Four kana is past the floor. Longer reward lines still say `ごほうび`. |
| `受け取る` | `Claim +100`, `Claim +300` | `もらう` | The button cannot hold `受け取る`, and `+` must not sit against a kana. Toasts that have room say `受け取りました`. |
| `ドーナツリング` | `Cyber Donut Rings` | `サイバードーナツ` | The full name sits on the shrink floor. The category tab keeps `ドーナツリング`. |
| `ショップ` | `SHOP` | `お店` | Four kana is past the floor. |

`recycling coins` and `gameplay coins` are translated as `ゴールドコイン`, which is what the glossary asks for the balance. `Backup Pod` and `Auxiliary Scrap Pod` both use `バックアップポッド`.

Checked terms that did fit, sometimes with a fit warning under the shrink floor: `スクラップ`, `セクター`, `ワールド`, `ブラックホール`, `Nibblesのぼうし`, `トロフィー`, `まいにちのおしごと`, `シフト`, `まいにちのてあて`, `うちゅうゴミ`, `宇宙のおそうじ屋さん`.

## Other choices

ASCII `(` `)` are kept, including the cut-off `Unlock (1200` and `Trophy: … (+500` fragments. Full-width `（）` would not match the validator's parenthesis count. Other punctuation is full-width `。！？、：`, with `…` (U+2026). A katakana middle dot `・` stands in front of `+` when the previous character would be kana, because the validator only counts `+` when it is not touching a letter. Japanese has no spaces between kana and Latin or digits.

`kg`, Western digits, `×`, `▶`, `←`, `/`, `%`, emoji, and `✓` stay as in English. Nothing was pre-uppercased.

## English source

- `Unlock (1200` (and the other `Unlock (` lines) and `Trophy: Daily Duty Star (+500` are cut off. The next run is not in the key. The translations stay open on purpose.
- The privacy paragraph still talks about browser `localStorage`, tracking SDKs, cookies, and behavioral ads.
- The version subtitle still says `Pure Gesture Mobile Web Puzzle`.
- The spare pod has two English names (`Launch Backup Pod`, `Auxiliary Scrap Pod Deployed`). Both use one Japanese word.
- `LVL 11`'s captured width is about 2px narrower than `LVL 10` and `LVL 12`, which are the same shape of English string.

## Validator

`validate_table.py --lang ja` on `ja.json`: 0 errors, 87 warnings, 49 suppressed. Warnings are 84 `fit`, 2 `glossary` (`LVL 11`, `TOY SHOP`), and 1 `font-pending` (`MPLUSRounded1c-Bold.ttf`). Fit warnings are advisory while that chain is pending. The single-line ones that remain are under the 72% shrink floor.
