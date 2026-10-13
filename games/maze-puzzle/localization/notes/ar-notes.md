# Arabic notes — ar (العربية)

Modern Standard Arabic, 2026-10-13. Masculine-neutral singular where a verb is needed. No dialect, no tashkeel, Western digits. No native speaker has reviewed this table.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 4 |
| suppressed fit warnings | 50 |
| accepted identical copies | 0 |

Visible warnings by kind: `fit` 2 (`Nibbles Hats`, `Bronze Shift • Cadet Sweep`), `glossary` 1 (`TOY SHOP`), `font-pending` 1 (Cairo chain). The 50 suppressed warnings are `LVL 1`–`LVL 50` in `ar-fit-exceptions.json`. Brand lines, `kg` strings, and emoji-only strings stay identical because the validator already allows them. They are not listed in an identical-ok file.

## Doubtful lines

| English | Choice | Alternative |
| --- | --- | --- |
| `Nebula Pinball` | `دبابيس السديم` | `كرة الدبابيس` (longer; Latin pinball is not allowed) |
| `Bronze Shift • Cadet Sweep` | `وردية برونزية • جولة المتدرب` | `جولة متدرب` (fits the 176px card; the form with `ال` is 181px) |
| `Cyber Orbit 1` | `مدار سيبر 1` | `مدار إلكتروني 1` (clearer, wider on the 71px card) |
| `The Kessler Singularity` | `تفرد كيسلر` | `نقطة كيسلر` (kids may not know `تفرد`; the glossary word is `تفرد`) |
| `Skies` | `سماء` | `سماوات` (glossary plural; about 35px in a 28px tab) |
| `Snacks Eaten` | `أكل الوجبات` | `أكلت وجبات خفيفة` (about 1px over the 79px stat) |
| `TOY SHOP` | `متجر لعب` | `متجر الألعاب` (glossary; about 95px in an 84px header, still above the 72% floor) |
| `TROPHIES` | `كأس` | `كؤوس` (does not contain the glossary string `كأس`) |
| `Resets in 14h 59m` | `بعد 14 س 59 د` | keeping `h` and `m` (Latin letters outside the allowlist are an error) |
| `← Back` | `← ارجع` | do not mirror the arrow |

`← ارجع` keeps the arrow at the start of the logical string. The layout is not mirrored. If the line's base direction is still left-to-right, the arrow should stay on the left. If a renderer treats the line as right-to-left, the arrow can jump to the right of `ارجع`. It was not mirrored here.

`Nibbles Hats` stays `قبعات Nibbles` (glossary). The estimator says 74px in a 69px tab, above the 72% shrink floor, so it should still paint in full.

## Button labels

`Cancel` is `إلغاء`. It is 29px in a 37px box. It does not overflow.

The same test on the other bare imperatives:

| English | Now | Why |
| --- | --- | --- |
| `Close` | `إغلاق` | Standard close label. 31px in a 32px box. |
| `View` | `عرض` | Standard view label. 17px in a 26px box. `اقرأ` was "read". |
| `Restart Sector` | `إعادة القطاع` | Same verbal noun as `Reset` → `إعادة`. 67px in an 82px box. |
| `RESUME PLAY` | `متابعة اللعب` | 89px in a 109px box. |
| `Launch Backup Pod (+8 Scrap!)` | `إطلاق حجرة احتياط (+8 خردة!)` | 159px in a 182px box. |
| `Wear` | `ارتد` (unchanged) | `ارتداء` is 35px in a 29px box (warning, under the 40px floor). |
| `Play Again` | `أعد اللعب` (unchanged) | `إعادة اللعب` is 61px in a 58px box. |
| `Try Again` | `جرب ثانية` (unchanged) | `إعادة المحاولة` is 78px, past the 76px shrink floor. |
| `Return to Home` | `عد إلى الرئيسية` (unchanged) | `العودة إلى الرئيسية` is 105px in an 88px box. |
| `← Back`, `PLAY`, unlock, `Claim`, `Start Shift` | `ارجع`, `العب`, `افتح`, `اجمع`, `ابدأ` | These stay imperatives. That is the normal game label. |

`Choose Language` is a modal title and stays `اختر اللغة`. `Clear unlocked levels...` is a settings subtitle, not a button, and stays `امسح...`.

## Cadet

`طالب` means student. Cadet is `متدرب`.

| English | Now |
| --- | --- |
| `Bronze Shift • Cadet Sweep` | `وردية برونزية • جولة المتدرب` |
| `Clean up Bronze Shift • Cadet Sweep!` | `نظف وردية برونزية • جولة المتدرب!` |
| `Tidy Space Cadet` | `متدرب فضاء مرتب` |

`جولة المتدرب` on the shift card is 181px in a 176px box (floor 244px). It stays the glossary-style phrase with `ال`. The cleanup line is 189px in a 195px box. Silver is `خطر القطاع` and gold is `قمة دوامة`. Neither used a person-word the way `طالب` did.

## Glossary terms not used as written

Checked terms are used except the shop header.

| Term | Where | What was written | Why |
| --- | --- | --- | --- |
| `Toy Shop` → `متجر الألعاب` | `TOY SHOP` header | `متجر لعب` | The full term is about 95px in an 84px header. The sentence `All items in the Toy Shop...` uses `متجر الألعاب`. |
| `Skies` → `سماوات` | shop tab `Skies` | `سماء` | `سماوات` is wider than the 28px tab. Check is no. |
| `snacks` → `وجبات خفيفة` | `Snacks Eaten` | `أكل الوجبات` | The full phrase plus a verb is about 1px over. `Snacks Collected` keeps `وجبات خفيفة`. Check is no. |
| `trophy` → `كأس` | `TROPHIES`, `TROPHY ROOM` | `كأس`, `غرفة الكأس` | `كؤوس` does not contain `كأس`. |
| `shift` → `وردية` | `All 3 shifts`, the daily toasts | `كل وردية من 3` | `الورديات` does not contain `وردية`. The number stays. Agreement is stiff. |
| `ring` → `حلقة` | plural labels | `حلقات` where the English is plural | `حلقات` does not contain `حلقة`. Check is no. `Donut Rings` is the glossary phrase `حلقات دونات`. |
| `cadet` → `طالب` | shift name and `Tidy Space Cadet` | `متدرب` | `طالب` means student. Check is no. |

`برونزية` / `فضية` / `ذهبية` are the feminine forms next to `وردية`. They still contain `برونزي` / `فضي` / `ذهبي`.

## Placeholders

`{0}` is never glued to an Arabic letter.

| Key | What changed |
| --- | --- |
| `daily.countdown` | `Resets in {0}h {1}m` → `بعد {0} س {1} د`. The CSV note says to keep `h` and `m`. Arabic cannot: those letters are outside the allowlist. `س` and `د` stand apart from the placeholders. |
| `pause.quota` and `Goal: Feed Nibbles 20 kg (0/20 kg eaten)` | `eaten` is `مأكول` after the second `kg`, so `{1}` stays against the parenthesis. |
| `gift.desc.ready` | Noun after the coin, because the emoji has text on both sides: `اجمع مجانا +100 🪙 من المصروف اليومي.` |
| `shop.footer.unlock` | `افتح ({0} 🪙)` — `{0}` is inside the parenthesis, not against `افتح`. |
| `home.mission.world_fallback` | `عالم {0}` |
| `toast.sector.cleanup` | `نظف {0}! 🚀` |
| `toast.trophy` | `🏆 كأس: {0} (+{1} 🪙)` |

## Fragments and RTL order

Text is logical order. No bidi marks, no presentation forms, no tatweel. The page stays left-to-right, so a multi-run line can read in the wrong order for an Arabic reader:

| Element | Runs in logical order | Why it may look wrong |
| --- | --- | --- |
| `gift-banner-desc` | `اجمع مجانا كل يوم +100` + `مصروف.` | The noun is the second run. Together it is grammatical but the object sits late. An RTL reader scanning a left-to-right row may hit `مصروف.` first. |
| `gift-banner-desc` | `عد غدا لمزيد من +100` + `صندوق هدية!` | Same split. The gift name is the second run. |
| Footer | `Orbital Salvage • Space Trash Clean v1.2.0` + `أحجية ويب للجوال بإيماءة فقط` | Brand stays Latin. The Arabic tagline is the next run. |
| Trophy line | `كأس: نجم الواجب (+500` | Cut on purpose. The closing `)` is the next run and is not in this table. |
| Unlock prices | `افتح (500` and the other `افتح (…` lines | Same cut. The price's closing run is not in this table. |

`Prize` is only `جائزة` and can stand alone.

## English source

- The privacy body still says the save is browser `localStorage`. This build is a Unity mobile game.
- `Pure Gesture Mobile Web Puzzle` still says Web. Translated as `أحجية ويب للجوال بإيماءة فقط` so the word is not dropped.
- `Resets in 14h 59m` and `Goal: Feed Nibbles 20 kg (0/20 kg eaten)` bake sample numbers. The `str` keys are the placeholders.
- `Unlock (1200` (and the other prices) and `Trophy: Daily Duty Star (+500` are cut fragments. The missing `)` was not added.
- `Launch Backup Pod` and `Auxiliary Scrap Pod` are two English names for one object. Both use `حجرة احتياط`, as the glossary says.

`Calm Orbit Sweep` drops the word centrifugal. The line says gentle turning, wide gates, easy flow.

## Font

Glyph check is `font-pending` (`Cairo-700.ttf`). Checked locally against Noto Sans Arabic (`/tmp/maze-i18n/shaping/fonts/NotoSansArabic-VF.ttf`) and Cairo (`/tmp/maze-i18n/fonts/dl/Cairo[slnt,wght].ttf`, plus the 203-glyph `cairo-700.ttf` subset).

Every Arabic letter in this table is in those faces, including `،` U+060C and `؟` U+061F. No tashkeel was used. Latin tokens in the table (`Nibbles`, `kg`, `ORBITAL`, `SALVAGE`, `Orbital Salvage`, `SPACE TRASH CLEAN`, `localStorage`) are in Noto Sans Arabic.

Not in Cairo or Noto Sans Arabic, and not changed: `✓` U+2713, `←` U+2190, `▶` U+25B6, and the emoji. `✓` and `▶` are in Noto Sans Symbols 2. `←` is in Noto Sans Symbols. They need that symbol font beside Cairo.

## Validator

```text
errors: 0
warnings: 4
suppressed fit: 50
by-kind: fit=2, glossary=1, font-pending=1
```

`ar-fit-exceptions.json` hides `LVL 1`–`LVL 50`. `--show-suppressed` prints them. No identical-ok file.
