# Hindi notes — hi (हिन्दी)

Draft, 2026-10-13. Standard Hindi in Devanagari. Address is तुम (खेलो, देखो, लो), or no pronoun. Western digits. Nibbles stays in Latin. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/review/review-hi.md` and is not marked done.

## Coverage

|                           | Count |
| ------------------------- | ----: |
| spec keys                 |   355 |
| str keys                  |    81 |
| validator errors          |     0 |
| visible warnings          |     3 |
| suppressed fit warnings   |    11 |
| accepted identical copies |     0 |

Visible warnings: `fit` 2 (`Close`, `Nibbles Hats`), `font-pending` 1 (`Baloo2-700.ttf`). No glossary warnings. No expansion warnings. No identical-ok file: brand lines, `kg` strings, and strings with no letters stay English because the validator already allows them.

`hi-fit-exceptions.json` lists `LVL 1`–`LVL 50` and `Moon Chute 1`–`10`. Only `LVL 11` currently warns among the level chips. The other 49 chips are listed so a later estimate still suppresses the glossary word `स्तर`.

## Doubtful lines

| English                                 | Choice                                   | Alternative                                                              |
| --------------------------------------- | ---------------------------------------- | ------------------------------------------------------------------------ |
| `SHIFT INCOMPLETE!`                     | `पाली अभी बाकी!`                         | `पाली अधूरी!`                                                            |
| `LOCKED`                                | `ताला`                                   | `बंद`                                                                    |
| `Close`                                 | `बंद करो`                                | `बंद` (drops the verb; 16px in the 32px box)                             |
| `MISSION PAUSED`                        | `मिशन रुका`                              | `मिशन रोकें` (glossary आप-form; reads as a command)                      |
| `Party Popper Hole`                     | `पार्टी पॉपर होल`                        | `कंफेटी होल`                                                             |
| `Hangar`                                | `हैंगर`                                  | `बेस`                                                                    |
| `Lost In Void`                          | `शून्य में खोया`                         | `खाली शून्य`                                                             |
| `Nibbles the Void`                      | `शून्य Nibbles`                          | `खाली Nibbles`                                                           |
| `{0} NEW`                               | `{0} नए`                                 | `{0} नया` (1 नया / 2 नए / 3 नए are inflected; the template cannot agree) |
| `Resets in {0}h {1}m`                   | `{0}h {1}m में रीसेट`                    | `{0} घंटे {1} मिनट में रीसेट`                                            |
| `Goal: Feed Nibbles 20 kg space snacks` | `लक्ष्य: Nibbles को 20 kg स्नैक्स खिलाओ` | `अंतरिक्ष स्नैक्स` was about 14px over the 233px line                    |
| `Wear`                                  | `पहनो`                                   | glossary `पहनें`                                                         |

## Glossary terms not used as recommended

Checked terms (`check: yes`) all matched. The validator reported no glossary warnings.

These `check: no` words were changed so the line stays तुम-form or grammatical:

| Term         | Recommendation | Used                                                       | Why                                                                         |
| ------------ | -------------- | ---------------------------------------------------------- | --------------------------------------------------------------------------- |
| wear         | पहनें          | पहनो                                                       | पहनें is आप. The button is the तुम imperative.                              |
| pause        | रोकें          | मिशन रुका                                                  | The banner is a state, not a command. रोकें would tell the player to pause. |
| defeat       | हार            | not used                                                   | The fail line is `SHIFT INCOMPLETE!`. हार is harsher than the banner.       |
| map          | नक्शा          | नक्शे on `MAPS` and `CLEANUP MAPS`                         | Plural. `Next Map ▶` stays `अगला नक्शा`.                                    |
| ring         | छल्ला          | छल्ले / छल्लों where the grammar needs a plural or oblique | Singular छल्ला is the shop word `पुदीना कैंडी छल्ला`.                       |
| space snacks | स्नैक्स        | स्नैक्स, without अंतरिक्ष, on the one tight goal line      | `अंतरिक्ष स्नैक्स` did not fit. Other space lines keep अंतरिक्ष.            |

`TROPHIES` is the singular glossary form `ट्रॉफी`. `ट्रॉफियाँ` does not contain that string, so the dock uses the singular.

`अंतरिक्ष कचरा` stays uninflected in `अंतरिक्ष कचरा के 50 टुकड़े` so the checked phrase is intact. `कचरे` would be the ordinary oblique.

`सभी 3` keeps the digit `3`. `तीनों` failed the digit check.

## Placeholders

`{0}` is never glued to a Devanagari letter or matra. No ordinal suffix (`{0}वाँ`) was used.

| Key                           | Rephrase                                                                                |
| ----------------------------- | --------------------------------------------------------------------------------------- |
| `daily.countdown`             | `{0}h {1}m में रीसेट`. The clock stays in Latin `h`/`m`. `{0}` starts the line.         |
| `toast.sector.cleanup`        | `{0} साफ करो! 🚀`. The name is first so the verb can end the line.                      |
| `pause.quota`                 | `लक्ष्य: Nibbles को {0} kg खिलाओ ({1}/{0} kg खाया)`. `{0}` sits next to a space or `/`. |
| `home.mission.world_fallback` | `दुनिया {0}` with a space.                                                              |
| `dock.daily.new`              | `{0} नए` with a space.                                                                  |
| `shop.footer.unlock`          | `अनलॉक ({0} 🪙)`. `{0}` follows `(`.                                                    |

Gift banner fragments, read in order:

- `अपना मुफ़्त रोज़ का +100` + `जेब खर्च लो।`
- `कल एक और +100` + `केयर पैकेज!`

The verb `लो` sits on the allowance fragment so the pair is verb-final. The str lines use the same words, with the coin emoji kept after `+100`.

## Fit that is still visible

| Key            | Estimate                       | Why it stays                                                                                                    |
| -------------- | ------------------------------ | --------------------------------------------------------------------------------------------------------------- |
| `Close`        | 35px vs a 32px box, floor 45px | `बंद करो` is the verb. `बंद` fits and is not a verb. Shrink is about 93%.                                       |
| `Nibbles Hats` | 92px vs a 69px box, floor 96px | The checked phrase `Nibbles की टोपियाँ` is the whole string. Shrink is about 75%, above the 72% ellipsis floor. |

`Moon Chute 1`–`10` (`चाँद फिसलपट्टी`) are about 1px over. Suppressed. The virama in `फिसलपट्टी` is counted at 0.40 em.

## English source

- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut off before `)`. Counts match the source. The str shop button has the closing `)`.
- `Launch Backup Pod` and `Auxiliary Scrap Pod` are one object. Both use `बैकअप पॉड`.
- Coins are called gold coins, recycling coins, and gameplay coins. All three use `सोने के सिक्के`. The gold shift is `स्वर्ण`, not that phrase.
- `Goal: Feed Nibbles 20 kg (0/20 kg eaten)` writes `0/20` with no spaces. Other quotas use `0 / 20`.
- The privacy body says `localStorage`, `SDKs`, and `behavioral ads` to a parent. `localStorage` is kept. SDK stays as `SDK`.
- `Pure Gesture Mobile Web Puzzle` reads like a web-build subtitle.
- `Concentric Planetary Dual-Track Gearing` is engineer copy. The line is `दो गोल रास्ते, एक के अंदर एक`.
- `big anime eyes` is rendered as `बड़ी गोल आँखें`. `Fiesta` is `जश्न`.

## Font

Checked with fontTools against `/tmp/maze-i18n/fonts/dl/Baloo2[wght].ttf`, `fonts/out/baloo2-deva.ttf`, and `fonts/out/baloo2-700-latn-deva.ttf`. Every Devanagari and Latin character in the table is in all three, including danda, chandrabindu, and nukta (`ज़`, `फ़`, `ड़`, `ढ़`). `←` and `▶` are in Baloo. No Devanagari exception.

Not in Baloo, and not in Noto Sans Devanagari either: the emoji, `✓` (U+2713), `⏩` `⏪`, `⚡`, `✨`, `⭐`, `⚙`, and U+FE0F. Noto Sans Devanagari also lacks `←` and `▶`. Those symbols are the same ones the English table already uses. The validator still warns `font-pending` because `fonts-chains.json` has an empty chain for hi.

toast.infinite.round: agent draft, no native review
home.infinite.world/title/best: agent draft, no native review
