# Uzbek notes — uz (Oʻzbekcha)

Latin script only, informal `sen`. oʻ and gʻ use U+02BB. No native speaker has reviewed this table. Native review is especially needed: the glossary row is marked native-review-recommended, and several short labels are guesses a parent should confirm. The 30-key sample is at `/tmp/maze-i18n/tr-uz/review-uz.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 53 |
| suppressed fit warnings | 68 |

Visible warnings by kind: `glossary` 52, `font-pending` 1. No `stale`, `glyph`, `length`, `expansion`, `orthography`, or `fit` warnings remain visible. The 68 suppressed fit warnings are in `uz-fit-exceptions.json`.

The 52 glossary warnings are:

- `LVL 1`–`LVL 50` expect `bosqich`. The word crosses the 72% shrink floor on the narrowest chips (`Bosqich 5` is a fit error). The chip says `Qism`. Longer lines still say `bosqich` (`Bosqich 1 • Saqich sirpanma 1`, `Bosqich, kiyim va tanga oʻchadi`).
- `Nibbles Hats` expects `Nibbles qalpoqlari`. That phrase crosses the shrink floor. The tab says `Nibbles qalpoq`, which keeps Nibbles and the hat word.
- `TOY SHOP` expects `oʻyinchoq doʻkoni`. That phrase crosses the shrink floor at 17px. The header says `OʻYINCHOQ`. The shop sentence still says `oʻyinchoq doʻkoni`.

`font-pending` is Baloo 2, which the kit still lists as pending for Uzbek. Fredoka and Nunito already cover U+02BB, and the glyph check passed.

## Doubtful lines

| Key | Choice | Alternative |
| --- | --- | --- |
| `LVL 1`–`LVL 50` | `Qism N` | `bosqich` is the glossary word and does not fit |
| `TOY SHOP` | `OʻYINCHOQ` | `oʻyinchoq doʻkoni` does not fit the header |
| `Nibbles Hats` | `Nibbles qalpoq` | `Nibbles qalpoqlari` does not fit |
| `allowance.` | `choʻntak puli.` | the phrase is about 3px under the shrink floor |
| `Close` | `Yop` | `Yopish` overflows the 32px button |
| `Cancel` | `Bekor` | `Bekor qilish` meets the shrink floor |
| `Wear` | `Kiy` | `Kiyish` overflows the 29px button |
| `View` | `Oʻqi` | `Koʻrish` crosses the shrink floor; this opens the agreement |
| `Coins Won` | `oltin tanga` | plural `oltin tangalar` overflows the stat |
| `Sleepy Star Cloud` | `Uyquli bulut` | the star word did not fit with sleepy and cloud |
| `Bronze Shift • Cadet Sweep` | `Bronza smena • Kadet` | `supurish` did not fit |
| `Gold Shift • Vortex Apex` | `Oltin smena • Girdob` | `choʻqqi` did not fit |
| `Resets in 14h 59m` | `14 soat 59 daqiqada` | the bare `14 soat 59 daqiqa` fits; `daqiqada` keeps "in" |
| `Daily Duty Star` | `Kunlik yulduz` | `kunlik burch yulduzi` is heavier |
| `Kessler Hero` | `Kessler botir` | `qahramon` crosses the shrink floor |
| `Hangar` | `Angar` | the defeat-home button; `uy` is reserved for Home |
| chute names | `sirpanma` | glossary `sirpanchiq` did not fit the cards |
| `Skies` | `Osmon` | plural `osmonlar` crosses the shrink floor |
| `Donut Rings` | `Donut halqa` | plural `donut halqalari` crosses the shrink floor |
| `← Back` | `← Qayt` | `Orqaga` overflows; the arrow stays `←` |

Buttons are sen imperatives where the box allows (`Oʻyna`, `Yop`, `Kiy`, `Tikla`, `Oʻqi`, `Qayt`). `Bekor` is the usual short cancel button, not a full verb. `Olish` and `Ochish` fit on the claim and unlock lines.

## Glossary terms not used as written

| Term | Where | What was used | Addition requested |
| --- | --- | --- | --- |
| `bosqich` | `LVL` chips | `Qism` | a chip-length stage word, or accept `Qism` |
| `oʻyinchoq doʻkoni` | `TOY SHOP` header | `OʻYINCHOQ` | a header-length shop word; keep the full phrase in sentences |
| `Nibbles qalpoqlari` | hats tab | `Nibbles qalpoq` | accept the singular on the tab |
| `sirpanchiq` | chute cards | `sirpanma` | a shorter slide word for cards |
| `osmonlar` | `Skies` | `Osmon` | singular is the tab that fits |
| `donut halqalari` | ring tab | `Donut halqa` | singular fits the tab |
| `oltin tangalar` | `Coins Won` | `oltin tanga` | singular for the narrow stat; sentences keep the plural |
| `yengil taom` | snack stats | `yigʻilgan taom` / `yeyilgan taom` | the eaten stat cannot hold `yengil` and the verb together; the goal line still says `yengil taom` |

Checked terms that the line can hold are used: `temir parcha`, `sektor`, `dunyo`, `smena`, `zaxira kapsula`, `kunlik nafaqa`, `kunlik ishlar`, `kubok`, `koinot chiqindisi`, `koinot farroshi`, `qora tuynuk`. Backup Pod and Auxiliary Scrap Pod are both `zaxira kapsula`.

## English source lines that look wrong

- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut off on purpose. Uzbek does not add the missing `)`.
- Backup Pod and Auxiliary Scrap Pod are one object. Both use `zaxira kapsula`.
- The balance is called gold coins, recycling coins, and gameplay coins. Sentences use `oltin tangalar`.
- `Resets in 14h 59m` bakes English `h` and `m`. Uzbek spells the units (`soat`, `daqiqa`).
- `Hangar` is `btn-defeat-home`, not the Home label. Translated as the place name `Angar`, not `uy`.
- `Nibbles' tummy` uses an English possessive. Uzbek says `Nibbles qorniga` with no apostrophe.

## Twelve least-sure keys

1. `LVL 5` → `Qism 5` (`bosqich` is a fit error)
2. `TOY SHOP` → `OʻYINCHOQ`
3. `Nibbles Hats` → `Nibbles qalpoq`
4. `allowance.` → `choʻntak puli.` (about 3px under the shrink floor)
5. `Close` → `Yop`
6. `Cancel` → `Bekor`
7. `Coins Won` → `oltin tanga`
8. `Sleepy Star Cloud` → `Uyquli bulut`
9. `Bronze Shift • Cadet Sweep` → `Bronza smena • Kadet`
10. `Daily Duty Star` → `Kunlik yulduz`
11. `Kessler Hero` → `Kessler botir`
12. `Resets in 14h 59m` → `14 soat 59 daqiqada`

## Kit notes

- A fit exception cannot hide a glossary warning. The 50 `Qism` chips stay in the warning count on purpose.
- `bosqich` cannot be kept on `LVL` chips without a fit error. German `Level` only warned. Uzbek needs a shorter chip word or a wider chip.
- `font-pending` still fires for Baloo 2 even though the Latin chain already has U+02BB.
- `str` rows have no line box, so toast width is not fit-checked.
- `🍩 DONUT 🍩` stays identical because `donut` is the glossary loanword. No `uz-identical-ok.json` entry was needed.
- Privacy copy is translated. Those strings also show up in the achievement specs, not only in the modal that is out of the app flow.
