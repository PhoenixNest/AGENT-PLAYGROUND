# French notes — fr (Français)

France usage, 2026-10-12. Informal `tu`. U+00A0 before `! ? : ;`. No U+202F. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-fr/review-fr.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 100 |
| suppressed fit warnings | 50 |

Visible warnings by kind: `fit` 43, `glossary` 53, `length` 4. No `stale`, `glyph`, `punctuation`, `expansion`, or `balance` warnings. Accepted identical: `Hangar`.

`str:unit.kg` and `str:unit.quota` stay `{0} kg` and `{0} / {1} kg`.

## Wording that the boxes forced

| Item | Choice | Why |
| --- | --- | --- |
| `LVL 1`–`LVL 50` | `Étape N` | `Niveau` crosses the 72% shrink floor on the tight chips (an error, which cannot be suppressed). `Étape` stays under that floor and still warns, so those 50 fit warnings are suppressed. The longer title uses `Étape` too, so the chip and the title are the same word. |
| `Reset` | `Vider` | `Effacer` is 42px against a 42px shrink floor, so it would ellipsize. Confirm is `Oui, vider`. The dialog title is `TOUT VIDER ?`. |
| `Close` | `Fermer` | Standard button infinitive. 39px in a 32px box, under the 45px floor. `Ferme` fits and sounds like an order. |
| `Claim +100` | `Prends +100` | `Récupérer +100` is past a comfortable button width. `Prends` is the tu imperative and sits 2px over the box. |
| `Wear` | `Porte` | Tu imperative of `porter`. 30px in a 29px box. `Mets` fits and also means a dish, which is wrong next to the candy shop. |
| `Unlock (1200` family | `Débloquer (N` | Glossary verb. Under the shrink floor. `Ouvrir` would read as open, the same miss as the German `OFFEN`. |
| `TOY SHOP` | `BOUTIQUE` | `boutique de jouets` is a fit error at 17px in an 84px header. The shop sentence keeps the full glossary phrase. The dock tab is `Jouets` because `Boutique` crosses that tab's shrink floor. |
| `Nibbles Hats` | `Chapeaux Nibbles` | `chapeaux de Nibbles` crosses the shrink floor. Dropping Nibbles fails the do-not-translate check. `de` is the word that went. |
| `Hangar` | `Hangar` | French word, identical to English. See `fr-identical-ok.json`. |
| `FREE` | `OFFERT` | `LIBRE` would mean available, not free of charge. Under the shrink floor. |
| `✓ UNLOCKED` | `✓ DÉBLOQUÉ` | `OUVERT` would mean still open. |
| `0 / 3 CLEARED` | `0 / 3 RÉUSSI` | Shorter than `CLEARED`, same word on `CLEARED ✓`. |
| `1 NEW` family | `NOUVEAU` | `NEUF` is also the number nine. `1 NEUF` is ambiguous. Under the shrink floor on `3 NEW`. |
| Backup pod, both English names | `capsule de secours` | Button and toast. `ferraille` for scrap on both. |
| Care package | `colis cadeau` | Title, fragment, toast, and gift banner. |
| Chute | `glisse` | `Toboggan` does not fit `Moon Chute`. One word on every chute. |
| Nebula Pinball | `Astroflipper` | `Flipper nébuleuse` crosses the shrink floor on the level rows. |
| Gold tier badge | `OR` | With `BRONZE` and `ARGENT`, the medal set. Not `pièces d'or`. |
| `settings` privacy modal | translated, not tuned for collisions | Out of the app flow. |

## Accepted fit warnings

No shorter natural synonym exists, or the shorter synonym would mean something else. All of these are under the 72% shrink floor.

| Keys | n | Decision |
| --- | ---: | --- |
| `LVL 1`–`LVL 50` | 50 | Suppressed. See above. |
| `Horizon Sector 1`–`9` | 9 | `Secteur horizon`. About 6px over. Glossary word `secteur`. |
| `Cyber Orbit 1`–`10` | 10 | `Cyberorbite`. About 2px over. `orbite` is inside the compound. |
| `Unlock (500` through `Unlock (3000` | 8 | `Débloquer`. Glossary verb. |
| `Donut Rings` | 1 | `Anneaux donut`. Glossary category. |
| `Nibbles Hats` | 1 | `Chapeaux Nibbles`. See above. |
| `Launch Backup Pod (+8 Scrap!)` | 1 | Both glossary terms, `capsule de secours` and `ferraille`. |
| `Close`, `Cancel`, `Change`, `← Back` | 4 | `Fermer`, `Annuler`, `Changer`, `← Retour`. Full verbs. |
| `Choose Sector` | 1 | `Choisir secteur`. Verb kept. |
| `Claim +100`, `Claim +300` | 2 | `Prends`. About 1–2px over. |
| `Wear` | 1 | `Porte`. About 1px over. |
| `SHOP` | 1 | `Jouets`. `Boutique` is a fit error on this tab. |
| `FREE` | 1 | `OFFERT`. |
| `3 NEW` | 1 | `3 NOUVEAU`. |
| `Kessler Hero` | 1 | `Héros Kessler`. About 7px over. |
| `Trophy: Daily Duty Star (+500` | 1 | `Trophée : Étoile du jour (+500`. Same trophy name as `Daily Duty Star`. About 4px over. The missing `)` is English. |

## Accepted glossary warnings

| Keys | n | Decision |
| --- | ---: | --- |
| `LVL 1`–`LVL 50` and `LVL 1 • Bubblegum Chute 1` | 51 | Checked word is `niveau`. It cannot be drawn on the chip. `Étape` is the stage word everywhere a number is shown. `niveaux` is still used in `Clear unlocked levels, skins & coins`. |
| `TOY SHOP` | 1 | Header is `BOUTIQUE`. The sentence `All items in the Toy Shop…` says `boutique de jouets`. |
| `Nibbles Hats` | 1 | `Chapeaux Nibbles`, not `chapeaux de Nibbles`. |

## Accepted length warnings

| Keys | Decision |
| --- | --- |
| `1 NEW`, `2 NEW`, `3 NEW` | `NOUVEAU` is 1.8× English. `NEUF` collides with the number nine. |
| `str:fx.hub.mint` | `ANNEAU MENTHE` is 1.625× `MINT RIG`. Same `ANNEAU` pattern as the other rig labels. No line box on `str`. |

## Doubtful lines

| Key | Choice | Alternative |
| --- | --- | --- |
| `allowance.` | `poche.` | The run before it is `Prends ton +100 d'argent de`, so the line reads `Prends ton +100 d'argent de poche.` |
| `Lost In Void` | `Dans le vide` | `Perdu dans le vide` does not fit the stat label. |
| `Cute Cosmic Janitor` | `Joli concierge cosmique` | `Joli` is masculine. `Concierge cosmique` drops `cute` and stays neutral. |
| `100% offline, kid-safe, no trackers` | `100% hors ligne, sûr, sans mouchard` | `sûr` is masculine and stands alone in the list. |
| `Restart Sector` | `Rejoue secteur` | Tu imperative, while `Choisir secteur` is an infinitive. `Rejouer le secteur` does not fit. |
| `Nebula Pinball` | `Astroflipper` | Coined. `Nébuleuse` fits and drops pinball. `Flipper` fits and drops the nebula. |

## Twelve least-sure keys

1. `LVL 1` → `Étape 1` (glossary still wants `niveau`)
2. `Reset` → `Vider` (confirm is `Oui, vider`)
3. `TOY SHOP` → `BOUTIQUE` (sentence keeps `boutique de jouets`; dock tab is `Jouets`)
4. `Nibbles Hats` → `Chapeaux Nibbles`
5. `allowance.` → `poche.` (only correct when read after the previous run)
6. `Nebula Pinball` → `Astroflipper`
7. `Moon Chute 1` → `Glisse lune 1` (glossary chute word is `toboggan`)
8. `Close` → `Fermer`
9. `Unlock (1200` → `Débloquer (1200`
10. `GOLD` → `OR`
11. `Bronze Shift • Cadet Sweep` → `Service bronze • Cadet` (`service` is the glossary word for shift; the sweep word did not fit)
12. `Claim +100` → `Prends +100` (glossary button word is `récupérer`)

## Glossary additions requested

- A chip-length stage word. `niveau` is a fit error on `LVL 2` through `LVL 19` and several later chips. This table uses `Étape` and accepts the glossary warnings.
- A header-length shop word. `boutique de jouets` does not fit `TOY SHOP`. `Boutique` does.
- A tab-length hat category. `chapeaux de Nibbles` does not fit. `Chapeaux Nibbles` does, under the floor.
- A short chute word. `toboggan` does not fit `Moon Chute`. `glisse` is used for every chute.
- A short claim verb for `Claim +100`. `récupérer` does not fit. `Prends` is the tu imperative.

Checked terms that did fit, and are used: `ferraille`, `pièces d'or`, `secteur`, `monde`, `concierge cosmique`, `trou noir`, `trophée`, `opérations du jour`, `service`, `capsule de secours`, `allocation du jour`, `déchets de l'espace`. `boutique de jouets` is in the shop sentence only.

## English source lines that look wrong

- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut off on purpose. French does not add the missing `)`.
- `Launch Backup Pod` and `Auxiliary Scrap Pod` are one object. Both are `capsule de secours`.
- The privacy modal is out of the app flow. It is translated and not fit-tuned.

## Kit notes

- `fr-identical-ok.json` now waives `Hangar`. `merge_table.py` does not need a French-only `doNotTranslate` entry. The German pilot's open item for that waiver is fixed on this branch.
- A fit error past the 72% floor cannot be suppressed. That is why `Niveau` is not on the chips.
- The glossary check wants the whole recommended phrase as a substring. `d'or` satisfies `or`. `Étape` cannot satisfy `niveau`. `Chapeaux Nibbles` cannot satisfy `chapeaux de Nibbles`.
- `str` rows have no line box, so toast width is not fit-checked. `fx.hub.mint` is a length warning instead.
- The 30-key sample spends several slots on `kg` strings because those keys sort first in the area. That is a picker quirk, not a translation gap.
