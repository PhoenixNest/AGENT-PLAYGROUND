# Vietnamese notes — vi (Tiếng Việt)

Draft of 2026-10-12, wording pass 2 the same day. Address is `bạn`, or no pronoun. Not `mày`. Values are NFC (precomposed). No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-vi/review-vi.md` and is not marked done.

Widths below are Nunito 700 advances, the face that draws Vietnamese. Scale is the captured English run box divided by that advance. The renderer ellipsizes past a scale of 0.72.

## Coverage

|                         | Count |
| ----------------------- | ----: |
| spec keys               |   355 |
| str keys                |    81 |
| validator errors        |     0 |
| visible warnings        |     3 |
| suppressed fit warnings |     1 |
| accepted identical      |     0 |

Visible warnings by kind: `fit` 1, `glossary` 1, `font-pending` 1. No `stale`, `glyph`, `length`, or `expansion` warnings. The glyph check ran on Fredoka, Nunito, and the Noto symbol fonts and found no missing glyphs. `LVL 1`–`LVL 50` use `Màn` and do not warn.

## Wording pass 2

| English            | Before            | After                                                | Nunito 700                                                                                            |
| ------------------ | ----------------- | ---------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| `MAPS`             | `Bản đồ`          | `Sơ đồ`                                              | `BẢN ĐỒ` was 44.0px in a 30.9px tab (scale 0.70, the `BẢN…` ellipsis). `SƠ ĐỒ` is 35.6px, scale 0.87. |
| `Reset`            | `Reset`           | `Xóa`                                                | `Xóa` is 21.5px in a 30.4px button, scale 1.41.                                                       |
| `Hangar`           | `Nhà`             | `Về nhà`                                             | 38.9px in a 42.5px button, scale 1.09.                                                                |
| `Lost In Void`     | `Rơi vào không`   | `Rơi mất`                                            | `RƠI VÀO KHÔNG` was 90.7px in a 66.7px stat (scale 0.74). `RƠI MẤT` is 46.8px, scale 1.43.            |
| `Nibbles the Void` | `Nibbles trống`   | `Nibbles hư không`                                   | 114.0px in a 100.4px card title, scale 0.88.                                                          |
| `Kessler Hero`     | `Hùng Kessler`    | `Anh hùng Kessler`                                   | 115.4px. The same card paints `Vua lao công vũ trụ` at 126.9px in full, so this shorter title fits.   |
| `Snacks Eaten`     | `Đồ ăn vặt đã ăn` | `Đã ăn vặt`                                          | `ĐỒ ĂN VẶT ĐÃ ĂN` was 101.2px in a 78.8px stat (scale 0.78). `ĐÃ ĂN VẶT` is 61.8px, scale 1.28.       |
| reset confirm body | the long sentence | `Xóa bản đồ, đồ mặc và xu vàng. Không lấy lại được!` | 575px (scale 0.64, the `Không lấy …` ellipsis) became 308px in a 366px line, scale 1.19.              |

`Reset` decision: `Xóa hết` is 42.9px, scale 0.71, and would ellipsize. `Làm lại` is 40.5px, scale 0.75, under the 80% chrome bar and close to the floor. `Đặt lại` is 37.0px, scale 0.82, on a button that already shows a five-letter word in full. The red button says `Xóa`, the same verb as the row `Xóa màn đã mở, đồ mặc và xu vàng`. The wider confirm button stays `Đặt lại`, and the row title stays `Đặt lại hết`. The identical-ok waiver for `Reset` was removed.

The dock cannot hold `bản đồ`. `Sơ đồ` is only that tab. The page title stays `Bản đồ dọn dẹp`, next map stays `Bản đồ sau ▶`, and the reset sentence still says `bản đồ`.

## Warnings kept

| Kind            | Key                             | Why it stays                                                                                                                                                                           |
| --------------- | ------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| fit             | `Kessler Hero`                  | Estimator says 106px against the 81px English run. The achievement card already paints `Vua lao công vũ trụ` at 127px with no ellipsis. `Anh hùng Kessler` is 115px on that same card. |
| glossary        | `TOY SHOP`                      | Header is `Đồ chơi`. `cửa hàng đồ chơi` is about 125px on an 84px title, past the 117px shrink floor. The shop sentence uses the full glossary phrase.                                 |
| font-pending    | table                           | Baloo 2 is still pending. The glyph check did run on Fredoka, Nunito, and the Noto symbol fonts, and it found no missing glyphs.                                                       |
| fit, suppressed | `Launch Backup Pod (+8 Scrap!)` | `Mở khoang dự phòng (+8 phế liệu!)`. The verb, both checked phrases, and `(+8)` are all required. 190px in a 182px box, under the 253px shrink floor.                                  |

`settings_modal_privacy` was not edited in this pass. Its two bodies and the toy-shop sentence still ellipsize. That modal is outside the app flow.

## Left as they were

| Key                 | Choice        | Why a longer word was not used                                                                                                                                                                                            |
| ------------------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Skies`             | `Trời`        | `Bầu trời` is 44.3px. The tab does paint `Mũ Nibbles` at 63px, so the word would likely show, but the validator treats the 28px English run as the box and errors (45px past a 42px floor). A fit error cannot be waived. |
| `FREE`              | `Tặng`        | `Miễn phí` is 35.6px in a 23.4px badge, scale 0.66. `Quà` fits and is already the daily gift badge. `Tặng` is 21.2px, scale 1.10, and the home still shows it in full.                                                    |
| `Resets in 14h 59m` | `Còn 14g 59p` | The alarm icon covers the C, so the pill reads `on 14g 59p`. The string already fits the pill. A leading space would be a whitespace warning and would not move the icon.                                                 |
| `Hangar` was `Nhà`  | now `Về nhà`  | `Nhà chứa` ellipsizes. `Về nhà` fits the button that already shows `Thử lại`.                                                                                                                                             |

## Doubtful lines

| Key                                   | Choice                              | Alternative                                                                                                    |
| ------------------------------------- | ----------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| `MAPS`                                | `Sơ đồ`                             | `Bản` fits at full size and means “copy”. `Bản đồ` is the ellipsis.                                            |
| `Reset`                               | `Xóa`                               | `Làm lại` and `Đặt lại` shrink on this 30px button. See the decision above.                                    |
| `TOY SHOP`                            | `Đồ chơi`                           | `Tiệm đồ chơi` warns. `cửa hàng đồ chơi` is a fit error.                                                       |
| `Skies`                               | `Trời`                              | `bầu trời` is a fit error on the captured run.                                                                 |
| `Donut Rings`                         | `Vòng bánh`                         | `vòng bánh vòng` is 79px in a 65px tab. The doubled `vòng` is also redundant in Vietnamese.                    |
| `Horizon Sector 1`                    | `Khu vực trời 1`                    | `Khu vực chân trời 1` is the real word for horizon and is 103px in a 91px chip.                                |
| `Cyber Orbit 1`                       | `Quỹ đạo số 1`                      | `Quỹ đạo điện tử 1` does not fit the chip.                                                                     |
| `Cheese Moon Orbit`                   | `Quỹ đạo phô mai`                   | `Quỹ đạo trăng phô mai` warns. The moon stays on `Máng trăng phô mai`.                                         |
| `Moon Chute 1`                        | `Máng trăng 1`                      | `máng trượt` plus the moon does not fit. Bubblegum chips use `Máng kẹo cao su`.                                |
| `Nibbles the Void`                    | `Nibbles hư không`                  | `Nibbles trống` meant “empty”. `khoảng không` does not fit. `hư không` shrinks to 0.88 and does not ellipsize. |
| `Lost In Void`                        | `Rơi mất`                           | The spilled-mass stat. `Rơi vãi` also fits; `vãi` in all caps can be read as slang, so this table uses `mất`.  |
| `Kessler Hero`                        | `Anh hùng Kessler`                  | `Hùng` alone is not “hero”. The card has room; the estimator still warns.                                      |
| `FREE`                                | `Tặng`                              | `Miễn phí` passes the shrink floor the wrong way. `STARTER FREE` is `Quà khởi đầu`.                            |
| `SHOP`                                | `Tiệm`                              | Dock badge. The full shop name stays `cửa hàng đồ chơi` in the sentence.                                       |
| `← Back`                              | `← Về`                              | `← Quay lại` is 59px in a 44px control.                                                                        |
| `Goal: 100 kg`                        | `Cần: 100 kg`                       | `Mục tiêu:` warns on the HUD. The long feed line says `Cho Nibbles ăn…`.                                       |
| `EQUIPPED`                            | `Đã mặc`                            | Matches `Wear` → `Mặc`. `đã trang bị` is 61px in a 59px badge.                                                 |
| `0 / 3 CLEARED`                       | `0 / 3 đã qua`                      | Same word on `CLEARED ✓`.                                                                                      |
| `Allowance Claimed!`                  | `Đã nhận tiền vặt!`                 | Full `tiền tiêu vặt` warns on the title. The gift sentence still uses `tiền tiêu vặt`.                         |
| `100% offline, kid-safe, no trackers` | `100% ngoại tuyến, an toàn cho trẻ` | `không theo dõi` does not fit beside kid-safe. Tracking stays in the privacy body.                             |
| `Singularity Horizon`                 | `Điểm kỳ dị chân trời`              | Glossary word. It is a science term for a child.                                                               |
| `Tidy Space Cadet`                    | `Học viên gọn`                      | `Học viên gọn gàng` is 0.1px over the chip.                                                                    |
| `Cute Cosmic Janitor`                 | `Lao công vũ trụ xinh`              | `dễ thương` is 0.1px over. The checked phrase stays.                                                           |
| `fx.swallow.yum`                      | `NGON! ✨`                          | Same word as `TASTY`. `NGON QUÁ!` is 1.83× and warns. The emoji still differs.                                 |

Backup Pod and Auxiliary Scrap Pod are both `khoang dự phòng`. Scrap is `phế liệu` in both places.

## Glossary additions requested

- A header-length form of Toy Shop. `cửa hàng đồ chơi` cannot pass the title's shrink floor. This table uses `Đồ chơi` there and the full phrase in the sentence.
- A tab-length form of Skies that the validator will accept. `bầu trời` errors against the short English run even though the tab paints a longer label. This table uses `Trời`.
- A dock-length form of map. `bản đồ` ellipsizes. This table uses `Sơ đồ` on the dock only.
- `Hangar` is not a glossary row. The defeat control goes home, so this table says `Về nhà`.
- `stipend` (`trợ cấp hằng ngày`) and `allowance` (`tiền tiêu vặt`) are one gift in the UI and two words in the glossary. They were not merged, because they are separate rows. Backup pod was the row that asked for one word.
- `lao công vũ trụ` is checked and sounds like a real job. It is used wherever the English says cosmic janitor. `người dọn vũ trụ` would be warmer if the check is ever relaxed.

## English source

- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are still cut off. The missing `)` was not added.
- `Hangar` is the label of `btn-defeat-home`.
- `Lost In Void` is the spilled-mass stat on the defeat card, not a place name.
- Gold coins, recycling coins, and gameplay coins are one balance. This table says `xu vàng` for all three.
- The privacy modal still stacks a heading on its paragraph. Copy was not shortened to chase that collision.

## Twelve least-sure keys

1. `MAPS` → `Sơ đồ` (dock only; titles stay `bản đồ`)
2. `Reset` → `Xóa` (confirm stays `Đặt lại`)
3. `TOY SHOP` → `Đồ chơi`
4. `Skies` → `Trời`
5. `Horizon Sector 1` → `Khu vực trời 1`
6. `Nibbles the Void` → `Nibbles hư không`
7. `Lost In Void` → `Rơi mất`
8. `Kessler Hero` → `Anh hùng Kessler`
9. `Cyber Orbit 1` → `Quỹ đạo số 1`
10. `FREE` → `Tặng`
11. `Launch Backup Pod (+8 Scrap!)` → `Mở khoang dự phòng (+8 phế liệu!)`
12. `Singularity Horizon` → `Điểm kỳ dị chân trời`

## Kit

- `font-pending` for Baloo 2 is expected. Fredoka has no Vietnamese glyphs, so titles fall back to Nunito. The cmap check passes because Nunito is in the chain. The layout audit should expect that fallback on every diacritic.
- `str` rows still have no line box, so toast width is not fit-checked.
- The shift recommendation `ca` is two letters. A hit inside another word such as `cao` would satisfy the check. This table uses `ca` as its own word (`ca đồng`, `ca bạc`, `ca vàng`).
- `cookie` is left in the privacy sentence. It is the usual Vietnamese loanword there, not a whole-string copy.

toast.infinite.round: agent draft, no native review
