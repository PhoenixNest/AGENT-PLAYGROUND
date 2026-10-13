# German notes — de (Deutsch)

Pilot language, 2026-10-12. Informal `du`. Nouns capitalised. No native speaker has reviewed this table, including the wording pass after the all-language audit. The 30-key sample is at `/tmp/maze-i18n/tr-de/review-de.md` and is not marked done.

## Coverage

|                           | Count |
| ------------------------- | ----: |
| spec keys                 |   355 |
| str keys                  |    81 |
| validator errors          |     0 |
| visible warnings          |    26 |
| suppressed fit warnings   |    50 |
| accepted identical copies |     2 |

Visible warnings by kind: `fit` 24, `glossary` 2. Same counts as before this pass. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `de-fit-exceptions.json`. `Hangar` and `Reset` are in `de-identical-ok.json`.

## Changes after the all-language audit

The iPhone stills have no control ellipsis. The only ellipses are the two privacy bodies. That modal is out of the app flow and was not edited. `SCHICHT NOCH OFFEN!` stays: the shift really is still open.

| English                               | Before                                                         | After                           | Fit                                                                                                                                               |
| ------------------------------------- | -------------------------------------------------------------- | ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `✓ UNLOCKED`                          | `✓ FREI` (also means free of charge; `FREE` is already `FREI`) | `✓ ERREICHT`                    | 63px in an 81px box, ok. Paint should stay above 80% (`✓ FREI` was 39px and was not the 0.78 shrink).                                             |
| `LOCKED`                              | `GESPERRT`                                                     | `ZU`                            | 12px in a 50px box, ok. `GESPERRT` was 48px in the estimator and painted at 0.78. `ZU` is the short shut-word. The Close button is the same word. |
| `100% offline, kid-safe, no trackers` | `100% offline, ohne Tracker`                                   | `100% offline, ohne Schnüffler` | 148px in a 176px box, ok. `kindersicher` also fits (134px). Both claims together are 204px and would add a warning.                               |
| `PLAY MISSION`                        | `MISSION LOS!`                                                 | `LOS GEHT'S!`                   | 94px in a 133px box, ok.                                                                                                                          |
| Fiesta vortex line                    | `Fiesta-Wirbel`                                                | `Party-Wirbel`                  | 289px in a 367px box, ok.                                                                                                                         |
| Anime-eyes line                       | `Anime-Augen`                                                  | `Kulleraugen`                   | 360px in a 385px box, ok.                                                                                                                         |
| Nibbles' tummy                        | `in Nibbles' Bauch`                                            | `in den Bauch von Nibbles`      | 289px in a 317px box, ok.                                                                                                                         |
| `Yes, Reset`                          | `Ja, neu`                                                      | `Ja, Reset`                     | 49px in a 55px box, ok. Same word as the Reset button.                                                                                            |

`FREE` / `dock.shop.free` stay `FREI`. That badge means free of charge.

## Loanword decisions

| Word                                | Decision                                                                                                                           |
| ----------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `Hangar`                            | Kept. It is the German word. `de-identical-ok.json`: German loanword.                                                              |
| `Reset`                             | Kept on the settings button and on `Ja, Reset`. `de-identical-ok.json`: common German UI word. The row title stays `Zurücksetzen`. |
| `Snacks`                            | Kept. Glossary recommendation.                                                                                                     |
| `Donut`, `Level`, `Bronze`, `Pause` | Kept. Glossary.                                                                                                                    |
| `Smoothie`                          | Kept in the strawberry-vortex line. Everyday German; `Brei` fits and tastes wrong.                                                 |
| `Tracker`                           | Removed from the settings subtitle. Not a glossary loanword.                                                                       |
| `Fiesta`, `Anime`                   | Removed. Replaced above.                                                                                                           |
| `Browser`                           | Kept in `Handy-Browser`. German word. The privacy `localStorage` line was not edited.                                              |
| `offline`                           | Kept inside `100% offline`. The required shape of that line.                                                                       |
| `Mission`                           | Dropped from the play button. The word is German; `LOS GEHT'S!` is what the button should say.                                     |

## Could not shorten

| String               | Why it stays                                                                                                                                                                               |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `← Zurück`           | Paints at 0.78 on every back button. No shorter natural word. `← Zur` is not a word. Estimator warning: 49px vs 44px.                                                                      |
| `Trophäen`           | Dock paints at 0.78. Glossary word, check yes. `Pokale` would add a warning and split the word from `TROPHÄENRAUM`. Estimator says the dock box fits (44px vs 54px); the paint is tighter. |
| `Horizontsektor` 1–9 | About 2px over. Shortest place + Sektor compound. The cards painted in full.                                                                                                               |
| `Mondrutsche` 1–10   | About 1–2px over. Shortest Mond + Rutsche compound. Not ellipsized.                                                                                                                        |
| `Nibbles-Hüte`       | 70px vs 69px. Glossary string. The shop tab painted in full.                                                                                                                               |
| `Himmel`             | 36px vs 30px. Glossary word. The shop tab painted in full.                                                                                                                                 |
| `Taschengeld.`       | 64px vs 55px. Glossary word. The banner line painted in full.                                                                                                                              |
| `SPIELLADEN`         | 85px vs 84px. `Spielzeugladen` is a fit error at 119px. Header and sentence use the same word, so both glossary-warn.                                                                      |
| Unlock buttons       | `Freischalten (500` is a fit error (97px, floor 93px). `Öffnen (500` fits at 61px. It means open, which is the short verb that fits.                                                       |
| Privacy modal        | Out of the app flow. Headings still collide and two bodies still ellipsize. Not edited.                                                                                                    |

## Twelve least-sure keys

1. `✓ UNLOCKED` → `✓ ERREICHT`
2. `LOCKED` → `ZU` (same word as Close)
3. `100% offline, kid-safe, no trackers` → `100% offline, ohne Schnüffler`
4. `Hangar` → `Hangar`
5. `Reset` → `Reset`, confirm `Ja, Reset`
6. `TOY SHOP` → `SPIELLADEN` (glossary wants Spielzeugladen)
7. Care package → `Putz-Paket!` (glossary says Carepaket)
8. `Silver Shift • Sector Hazard` → `Silber-Schicht • Sektor`
9. `Gold Shift • Vortex Apex` → `Gold-Schicht • Wirbel`
10. `Bronze Shift • Cadet Sweep` → `Bronze-Schicht • Kadett`
11. `Singularity Horizon` → `Singular-Horizont`
12. `Close` → `Zu`

## Warnings still left

| Keys                   |                 n | Reason                                                                             |
| ---------------------- | ----------------: | ---------------------------------------------------------------------------------- |
| `Horizon Sector 1`–`9` |                 9 | `Horizontsektor`, about 2px over. No shorter natural name.                         |
| `Moon Chute 1`–`10`    |                10 | `Mondrutsche`, about 1–2px over. No shorter natural name.                          |
| `Nibbles Hats`         |                 1 | Glossary `Nibbles-Hüte`, 70px vs 69px.                                             |
| `Skies`                |                 1 | Glossary `Himmel`, 36px vs 30px.                                                   |
| `TOY SHOP`             | 1 fit, 1 glossary | `SPIELLADEN` is 1px over. Glossary expects `Spielzeugladen`, which is a fit error. |
| Toy Shop sentence      |        1 glossary | Same word as the header: `Spielladen`.                                             |
| `allowance.`           |                 1 | Glossary `Taschengeld.`, 64px vs 55px.                                             |
| `← Back`               |                 1 | `← Zurück`, 49px vs 44px. No shorter word.                                         |

toast.infinite.round: agent draft, no native review
