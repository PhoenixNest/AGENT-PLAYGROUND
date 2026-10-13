# Uzbek notes — uz (Oʻzbekcha)

Latin script only, informal `sen`. oʻ and gʻ use U+02BB. No native speaker has reviewed this table. Native review is especially needed: the glossary row is marked native-review-recommended, and several short labels are guesses a parent should confirm. The 30-key sample is at `/tmp/maze-i18n/tr-uz/review-uz.md` and is not marked done.

Wording pass 2 follows the 2026-10-12 all-language audit. U+02BB was not changed. The audit drew it as a detached apostrophe (`O ' YNA`); that is a font issue for the lead.

## Coverage

|                         | Count |
| ----------------------- | ----: |
| spec keys               |   355 |
| str keys                |    81 |
| validator errors        |     0 |
| visible warnings        |    54 |
| suppressed fit warnings |    48 |

Visible warnings by kind: `glossary` 52, `fit` 1, `font-pending` 1. No `stale`, `glyph`, `length`, `expansion`, `orthography`, or `balance` warnings.

The 52 glossary warnings are:

- `LVL 1`–`LVL 50` expect `bosqich`. `Bosqich 5` is a fit error on the chip (47px vs a 40px floor). The chip says `Qism`. Longer lines still say `bosqich`.
- `Nibbles Hats` expects `Nibbles qalpoqlari`. That phrase is a fit error (105px vs a 96px floor). The tab says `Nibbles qalpoq`.
- `TOY SHOP` expects `oʻyinchoq doʻkoni`. That phrase is a fit error on the header (132px vs a 116px floor). The header and the dock now say `bozor`. The shop sentence still says `oʻyinchoq doʻkoni`.

The one visible fit warning is `Nibbles Hats`: `Nibbles qalpoq` is 81px against a 69px box, under the 96px floor (scale 0.85). The audit still showed the full tab. `Qalpoqlar` fits the box and drops the name Nibbles, which the do-not-translate check rejects. No shorter natural hat word keeps Nibbles.

`font-pending` is Baloo 2. Fredoka and Nunito already cover U+02BB.

## Fit exceptions

`uz-fit-exceptions.json` has the 50 `LVL` chips plus one other key.

The validator suppresses 47 of those chips (`LVL 1`, `LVL 21`, and `LVL 41` already fit `Qism N`) and `Launch Backup Pod (+8 Scrap!)`. That button is the one non-chip suppression. It has to keep `zaxira kapsula`, `temir parcha`, `+8`, and the English parentheses. `Zaxira kapsula (+8 temir parcha!)` is 196px against a 182px box and under the 253px floor (scale 0.93). Dropping a parenthesis mismatches the source.

Every other previous suppression was removed and the string was shortened so the width model fits the box:

| English              | Before                  | After               | Fit                            |
| -------------------- | ----------------------- | ------------------- | ------------------------------ |
| `SHOP`               | `doʻkon`                | `bozor`             | 33/30 warn, scale 0.90 → 28/30 |
| `MAPS`               | `xarita`                | `karta`             | 33/31 warn → 28/31             |
| `Cyber Orbit 1`–`10` | `Kiber orbita N`        | `Kiber davra N`     | 79/73 warn → 73/73             |
| `Kessler Hero`       | `Kessler botir`         | `Kessler mard`      | 88/81 warn → 81/81             |
| `Nibbles the Void`   | `Boʻshliq Nibbles`      | `Hechlik Nibbles`   | 109/106 warn → 102/106         |
| `Prize`              | `Sovrin`                | `Yutuq`             | 39/34 warn → 33/34             |
| `allowance.`         | `choʻntak puli.`        | `choʻntak.`         | 73/55 warn, scale 0.75 → 48/55 |
| `Allowance Claimed!` | `Choʻntak puli olindi!` | `Choʻntak olindi!`  | 133/128 warn → 103/128         |
| `care package!`      | `sovgʻa qutisi!`        | `sovgʻa quti!`      | 73/72 warn → 62/72             |
| `Resets in 14h 59m`  | `14 soat 59 daqiqada`   | `14 soat 59 daqiqa` | 93/82 warn → 82/82             |

`CLEANUP MAPS` and `Next Map ▶` use `karta` as well, so the map label is one word. The prize sentence uses `yutuq`. `daily.countdown` uses `daqiqa`, matching the clock.

## Wording pass

| English      | Before      | After       | Why                                                                                                                |
| ------------ | ----------- | ----------- | ------------------------------------------------------------------------------------------------------------------ |
| `SHOP`       | `doʻkon`    | `bozor`     | The dock drew `DOʻK…`. `bozor` fits the box and has no modifier letter, so the apostrophe gap cannot push it over. |
| `TOY SHOP`   | `OʻYINCHOQ` | `BOZOR`     | Same shop word as the dock. The full glossary phrase does not fit the header.                                      |
| `✓ UNLOCKED` | `✓ OCHIQ`   | `✓ OLINGAN` | `OCHIQ` means open. `OLINGAN` means obtained. `OLINDI` stays on claimed rewards. 57px in an 81px badge.            |
| `Close`      | `Yop`       | `Bekit`     | `Bekit` is the full sen imperative and sits on the 33px budget. `Yopish` is 39px and warns.                        |
| `Cancel`     | `Bekor`     | `Bekor`     | `Bekor qil` is 51px against a 37px box and a 51px floor (scale 0.72), which is the ellipsis edge.                  |
| `LVL N`      | `Qism N`    | `Qism N`    | `Bosqich 5` is a fit error. No other stage word fits.                                                              |

`PLAY` / `OʻYNA` is inside the model box (38/42). The audit shrink to 0.78 is the detached apostrophe, not a longer synonym. The character stays.

The reset body was ellipsized in the audit and is not the privacy modal. It is now `Ishonching komilmi? Karta, kiyim va oltin tangalar oʻchadi. Qaytmaydi!` (423px vs 613px before, both under the 688px model box).

## Doubtful lines

| Key                 | Choice              | Alternative                                                                  |
| ------------------- | ------------------- | ---------------------------------------------------------------------------- |
| `LVL 1`–`LVL 50`    | `Qism N`            | `bosqich` is a fit error                                                     |
| `SHOP` / `TOY SHOP` | `bozor`             | `doʻkon` ellipsizes on the dock; `oʻyinchoq doʻkoni` does not fit the header |
| `MAPS`              | `karta`             | `xarita` is 2px over the dock                                                |
| `Cyber Orbit N`     | `Kiber davra N`     | `orbita` stays in sentences; the level card cannot hold `Kiber orbita`       |
| `Nibbles Hats`      | `Nibbles qalpoq`    | `Qalpoqlar` fits and drops Nibbles                                           |
| `Nibbles the Void`  | `Hechlik Nibbles`   | `boʻshliq` is the place word (`boʻshliqda`) and is 3px over this title       |
| `Kessler Hero`      | `Kessler mard`      | `botir` is 7px over; `mard` means brave                                      |
| `Prize`             | `Yutuq`             | `sovrin` is 5px over the chip                                                |
| `allowance.`        | `choʻntak.`         | `choʻntak puli` crosses the floor; `nafaqa` is the stipend word              |
| `Close`             | `Bekit`             | `Yop` is a real stem imperative and looked clipped                           |
| `Cancel`            | `Bekor`             | `Bekor qil` meets the shrink floor                                           |
| `Resets in 14h 59m` | `14 soat 59 daqiqa` | `daqiqada` was 11px over; the clock icon carries "in"                        |
| `care package!`     | `sovgʻa quti!`      | `sovgʻa qutisi` stays on the longer title, which fits                        |
| chute names         | `sirpanma`          | glossary `sirpanchiq` did not fit the cards                                  |
| `Skies`             | `Osmon`             | plural `osmonlar` crosses the shrink floor                                   |
| `Donut Rings`       | `Donut halqa`       | plural `donut halqalari` crosses the shrink floor                            |
| `← Back`            | `← Qayt`            | `Orqaga` overflows; the arrow stays `←`                                      |

## Glossary terms not used as written

| Term                 | Where                | What was used    | Addition requested                                          |
| -------------------- | -------------------- | ---------------- | ----------------------------------------------------------- |
| `bosqich`            | `LVL` chips          | `Qism`           | a chip-length stage word, or accept `Qism`                  |
| `oʻyinchoq doʻkoni`  | shop chrome          | `bozor`          | a dock-length shop word; the sentence keeps the full phrase |
| `Nibbles qalpoqlari` | hats tab             | `Nibbles qalpoq` | accept the singular beside the name                         |
| `xarita`             | map chrome           | `karta`          | the dock cannot hold `xarita`                               |
| `orbita`             | `Cyber Orbit` titles | `davra`          | a 5-letter orbit word, or a wider level card                |
| `sovrin`             | prize chip           | `yutuq`          | a 5-letter prize word                                       |
| `sirpanchiq`         | chute cards          | `sirpanma`       | a shorter slide word for cards                              |
| `osmonlar`           | `Skies`              | `Osmon`          | singular is the tab that fits                               |
| `donut halqalari`    | ring tab             | `Donut halqa`    | singular fits the tab                                       |
| `oltin tangalar`     | `Coins Won`          | `oltin tanga`    | singular for the narrow stat; sentences keep the plural     |

Checked terms that the line can hold are used: `temir parcha`, `sektor`, `dunyo`, `smena`, `zaxira kapsula`, `kunlik nafaqa`, `kunlik ishlar`, `kubok`, `koinot chiqindisi`, `koinot farroshi`, `qora tuynuk`. Backup Pod and Auxiliary Scrap Pod are both `zaxira kapsula`.

## Twelve least-sure keys

1. `LVL 5` → `Qism 5` (`bosqich` is a fit error)
2. `SHOP` / `TOY SHOP` → `bozor` / `BOZOR`
3. `Nibbles Hats` → `Nibbles qalpoq` (81px vs a 69px box)
4. `MAPS` → `karta`
5. `Cyber Orbit 1` → `Kiber davra 1`
6. `Close` → `Bekit`
7. `Cancel` → `Bekor`
8. `allowance.` → `choʻntak.`
9. `Nibbles the Void` → `Hechlik Nibbles`
10. `Kessler Hero` → `Kessler mard`
11. `✓ UNLOCKED` → `✓ OLINGAN`
12. `Resets in 14h 59m` → `14 soat 59 daqiqa`

## Kit notes

- A fit exception cannot hide a glossary warning. The 50 `Qism` chips stay in the warning count on purpose.
- `bosqich` cannot be kept on `LVL` chips without a fit error. German `Level` only warned. Uzbek needs a shorter chip word or a wider chip.
- `Nibbles qalpoqlari` and `oʻyinchoq doʻkoni` are fit errors on their headers. The hats tab is the one remaining visible fit warning.
- `font-pending` still fires for Baloo 2 even though the Latin chain already has U+02BB. The audit still draws U+02BB as a spaced apostrophe. That is not fixed by a different character.
- `str` rows have no line box, so toast width is not fit-checked.
- `🍩 DONUT 🍩` stays identical because `donut` is the glossary loanword. No `uz-identical-ok.json` entry was needed.
- Privacy copy was left as it was. The audit ellipsized those bodies in every language.

toast.infinite.round: agent draft, no native review
