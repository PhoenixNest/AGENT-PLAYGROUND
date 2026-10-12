# Vietnamese notes — vi (Tiếng Việt)

Draft of 2026-10-12. Address is `bạn`, or no pronoun. Not `mày`. Values are NFC (precomposed). No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-vi/review-vi.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 2 |
| suppressed fit warnings | 1 |
| accepted identical | 1 |

Visible warnings by kind: `glossary` 1, `font-pending` 1. No `stale`, `glyph`, `length`, `expansion`, or visible `fit` warnings. `LVL 1`–`LVL 50` use `Màn` and do not warn, so they are not in the fit-exception file.

## Warnings kept

| Kind | Key | Why it stays |
| --- | --- | --- |
| glossary | `TOY SHOP` | Header is `Đồ chơi`. `cửa hàng đồ chơi` is about 125px on an 84px title, past the 117px shrink floor. The shop sentence uses the full glossary phrase. |
| font-pending | table | Baloo 2 is still pending. The glyph check did run on Fredoka, Nunito, and the Noto symbol fonts, and it found no missing glyphs. |
| fit, suppressed | `Launch Backup Pod (+8 Scrap!)` | `Mở khoang dự phòng (+8 phế liệu!)`. The verb, both checked phrases, and `(+8)` are all required. 190px in a 182px box, under the 253px shrink floor. |
| identical | `Reset` | Loanword on the 30px data button. `Đặt lại` is 39px against a 42px shrink floor. `Reset Progress` and `Yes, Reset` say `Đặt lại`. |

`settings_modal_privacy` is translated and was not tuned for the known heading collision. That modal is outside the app flow.

## Doubtful lines

| Key | Choice | Alternative |
| --- | --- | --- |
| `Hangar` | `Nhà` | `Hangar` as a loanword. The control is the defeat home button, and `Nhà chứa` is 45px in a 42px box. |
| `Reset` | `Reset` | `Đặt lại`, which nearly hits the shrink floor on this one button. |
| `TOY SHOP` | `Đồ chơi` | `Tiệm đồ chơi` (94px, warning) or `cửa hàng đồ chơi` (fit error). |
| `Skies` | `Trời` | `bầu trời` is 45px past a 42px shrink floor. |
| `Donut Rings` | `Vòng bánh` | `vòng bánh vòng` is 79px in a 65px tab. The doubled `vòng` is also redundant in Vietnamese. |
| `Horizon Sector 1` | `Khu vực trời 1` | `Khu vực chân trời 1` is the real word for horizon and is 103px in a 91px chip. |
| `Cyber Orbit 1` | `Quỹ đạo số 1` | `Quỹ đạo điện tử 1` does not fit the chip. |
| `Cheese Moon Orbit` | `Quỹ đạo phô mai` | `Quỹ đạo trăng phô mai` warns. The moon stays on `Máng trăng phô mai`. |
| `Moon Chute 1` | `Máng trăng 1` | `máng trượt` plus the moon does not fit. Bubblegum chips use `Máng kẹo cao su`. |
| `Nibbles the Void` | `Nibbles trống` | `Nibbles hư vô` fits too and sounds literary. `khoảng không` does not fit. |
| `Lost In Void` | `Rơi vào không` | Read as the spilled-mass stat, following the German audit. |
| `Kessler Hero` | `Hùng Kessler` | `Anh hùng Kessler` is about 106px in an 81px chip, near the floor. |
| `FREE` | `Tặng` | `Miễn phí` passes the 23px badge's shrink floor. `STARTER FREE` is `Quà khởi đầu`. |
| `SHOP` | `Tiệm` | Dock badge. The full shop name stays `cửa hàng đồ chơi` in the sentence. |
| `← Back` | `← Về` | `← Quay lại` is 59px in a 44px control. |
| `Goal: 100 kg` | `Cần: 100 kg` | `Mục tiêu:` warns on the HUD. The long feed line says `Cho Nibbles ăn…`. |
| `EQUIPPED` | `Đã mặc` | Matches `Wear` → `Mặc`. `đã trang bị` is 61px in a 59px badge. |
| `0 / 3 CLEARED` | `0 / 3 đã qua` | Same word on `CLEARED ✓`. It fits, unlike the long German pill. |
| `Allowance Claimed!` | `Đã nhận tiền vặt!` | Full `tiền tiêu vặt` warns on the title. The gift sentence still uses `tiền tiêu vặt`. |
| `100% offline, kid-safe, no trackers` | `100% ngoại tuyến, an toàn cho trẻ` | `không theo dõi` does not fit beside kid-safe. Tracking stays in the privacy body. |
| `Singularity Horizon` | `Điểm kỳ dị chân trời` | Glossary word. It is a science term for a child. |
| `Tidy Space Cadet` | `Học viên gọn` | `Học viên gọn gàng` is 0.1px over the chip. |
| `Cute Cosmic Janitor` | `Lao công vũ trụ xinh` | `dễ thương` is 0.1px over. The checked phrase stays. |
| `fx.swallow.yum` | `NGON! ✨` | Same word as `TASTY`. `NGON QUÁ!` is 1.83× and warns. The emoji still differs. |
| `Resets in 14h 59m` | `Còn 14g 59p` | `g` and `p` are giờ and phút with no period. |

Backup Pod and Auxiliary Scrap Pod are both `khoang dự phòng`. Scrap is `phế liệu` in both places.

## Glossary additions requested

- A header-length form of Toy Shop. `cửa hàng đồ chơi` cannot pass the title's shrink floor. This table uses `Đồ chơi` there and the full phrase in the sentence.
- A tab-length form of Skies. `bầu trời` cannot pass the tab's shrink floor. This table uses `Trời`.
- `Hangar` is not a glossary row. The defeat control goes home, so this table says `Nhà`. Add `Hangar` if the place name should stay.
- `stipend` (`trợ cấp hằng ngày`) and `allowance` (`tiền tiêu vặt`) are one gift in the UI and two words in the glossary. They were not merged, because they are separate rows. Backup pod was the row that asked for one word.
- `lao công vũ trụ` is checked and sounds like a real job. It is used wherever the English says cosmic janitor. `người dọn vũ trụ` would be warmer if the check is ever relaxed.

## English source

- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are still cut off. The missing `)` was not added.
- `Hangar` is the label of `btn-defeat-home`.
- `Lost In Void` is the spilled-mass stat on the defeat card, not a place name.
- Gold coins, recycling coins, and gameplay coins are one balance. This table says `xu vàng` for all three.
- The privacy modal still stacks a heading on its paragraph. Copy was not shortened to chase that collision.

## Twelve least-sure keys

1. `Hangar` → `Nhà`
2. `Reset` → `Reset`
3. `TOY SHOP` → `Đồ chơi`
4. `Skies` → `Trời`
5. `Horizon Sector 1` → `Khu vực trời 1`
6. `Nibbles the Void` → `Nibbles trống`
7. `Lost In Void` → `Rơi vào không`
8. `Kessler Hero` → `Hùng Kessler`
9. `Cyber Orbit 1` → `Quỹ đạo số 1`
10. `FREE` → `Tặng`
11. `Launch Backup Pod (+8 Scrap!)` → `Mở khoang dự phòng (+8 phế liệu!)`
12. `Singularity Horizon` → `Điểm kỳ dị chân trời`

## Kit

- `font-pending` for Baloo 2 is expected. Fredoka has no Vietnamese glyphs, so titles fall back to Nunito. The cmap check passes because Nunito is in the chain. The layout audit should expect that fallback on every diacritic.
- `str` rows still have no line box, so toast width is not fit-checked.
- The shift recommendation `ca` is two letters. A hit inside another word such as `cao` would satisfy the check. This table uses `ca` as its own word (`ca đồng`, `ca bạc`, `ca vàng`).
- `cookie` is left in the privacy sentence. It is the usual Vietnamese loanword there, not a whole-string copy.
