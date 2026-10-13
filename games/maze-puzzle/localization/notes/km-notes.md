# Khmer notes — km (ភាសាខ្មែរ)

Draft, 2026-10-13. Polite everyday kid tone. Not the royal register. Western digits. Khmer sentence mark `។` on sentences; buttons stay without it. No zero-width spaces or joiners. Placeholders are spaced off Khmer letters, or they sit next to a digit or symbol.

No native speaker has reviewed this table. Khmer is high-risk for an agent draft. The 30-key sample is at `/tmp/maze-i18n/review/review-km.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 8 |
| suppressed fit warnings | 50 |
| accepted identical copies | 0 |

Visible warnings by kind: `fit` 7, `font-pending` 1. No `glossary`, `stale`, `glyph`, `expansion`, `placeholder`, `emoji`, or `number` findings. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `km-fit-exceptions.json`. There is no `km-identical-ok.json`. Brand lines, unit strings, and emoji-only strings stay as English because the validator already allows them.

`font-pending` is the Kantumruy Pro chain, which is not wired yet. A local cmap check (Kantumruy Pro variable, Kantumruy Pro 700, Noto Sans Khmer) found no missing Khmer-block character. `…`, `។`, `←`, Latin, and digits are in Kantumruy Pro. `▶` (U+25B6) is not in Kantumruy Pro; it needs the symbol fallback, same as English. Emoji, `✓`, and U+FE0F are outside both Khmer faces. Noto Sans Khmer also lacks `←`; Kantumruy Pro has it.

## Five least sure

| English | This draft | Alternative |
| --- | --- | --- |
| `Bronze Shift • Cadet Sweep` | `វេនសំរឹទ្ធ • សិស្សសម្អាត` | `វេនសំរឹទ្ធ • សម្អាតថ្នាក់សិស្ស` (clearer, about 13px over the title) |
| `TOY SHOP` | `ហាងតុក្កតា` | `ហាងប្រដាប់ក្មេងលេង` (toy shop; glossary word means doll shop) |
| `0 / 3 CLEARED` / `CLEARED ✓` | `បានឆ្លង` | `បានឆ្លងផុត` (`ឆ្លង` also means contagious) |
| `Lost In Void` | `បាត់ចន្លោះ` | `វង្វេងក្នុងចន្លោះ` if this is a failure title, not a stat |
| `Silver Shift • Sector Hazard` | `វេនប្រាក់ • ផ្នែកលំបាក` | `វេនប្រាក់ • ផ្នែកគ្រោះ` (closer to hazard, harsher for children) |

## Every line that still needs a native check

Not a full reread list. These are the lines where the wording, the spelling, or the reading is not settled.

| English | Draft | Why it is doubtful |
| --- | --- | --- |
| `Bronze Shift • Cadet Sweep` | `វេនសំរឹទ្ធ • សិស្សសម្អាត` | `សិស្សសម្អាត` can be read as "clean the pupil". Same stem in `Clean up Bronze Shift • Cadet Sweep!` and `Tidy Space Cadet` (`សិស្សសម្អាតអវកាស`). |
| `TOY SHOP` and the shop sentence | `ហាងតុក្កតា` | Required glossary string. `តុក្កតា` is a doll, not toys in general. |
| `CLEARED`, `បានឆ្លង` chips and toasts | `បានឆ្លង` | Glossary word. Also the everyday word for catching a disease. |
| `Lost In Void` | `បាត់ចន្លោះ` | Read as a result stat next to snacks and coins. If it is a fail banner, this is the wrong meaning. |
| `Silver Shift • Sector Hazard` | `ផ្នែកលំបាក` | Softened from "hazard". `គ្រោះ` is nearer and sounds like an accident. |
| `FREE DAILY STIPEND` and the stipend toast | `ប្រាក់ឧបត្ថម្ភប្រចាំថ្ងៃ` | Required glossary string. Formal for a child's pocket money. The claim banner uses `លុយហោប៉ៅ` for allowance. |
| `Hangar` | `ផ្ទះ` | The button is the defeat home control. `យានដ្ឋាន` is the hangar word and is about 1px over a 42px button. `Return to Home` uses `ទំព័រដើម`. |
| `Yes, Reset` | `ចាស លុប` | `ចាស` is a spoken yes-particle. `យល់ព្រម លុប` is more button-like. `Reset` itself is `លុប` (erase), not a literal reset. |
| `The Kessler Singularity` / `Kessler Hero` | `ស៊ីងហ្គុយឡារីតេខេស្លឺ` / `វីរបុរសខេស្លឺ` | Transliteration of Kessler. Keeping `Kessler` in Latin also fits the script rules. |
| `Singularity Horizon` | `ជើងមេឃស៊ីងហ្គុយឡារីតេ` | Glossary spelling of singularity. 142px vs a 139px title. |
| `Nebula Pinball` / `Cotton Candy Nebula` | `ភីនបលផ្កាយ` / `ពពកស្ករស` | "Nebula" became "star" or "cloud". `ណេប៊ុយឡា` is an uncertain loan. |
| `Moon Chute N` | `ផ្លូវចន្ទ N` | Dropped `រអិល` from glossary `ផ្លូវរអិល` so the card fits. The world title `Cheese Moon Chutes` keeps `ផ្លូវរអិលចន្ទឈីស`. |
| `Bubblegum Chute N` | `ផ្លូវរអិលស្ករសៀវ N` | `ស្ករសៀវ` is chewing gum. `ស្ករកៅស៊ូ` is closer to bubblegum and is about 4px over. The belt name keeps `ខ្សែស្ករកៅស៊ូ`. |
| `Cyber Donut Rings` / `Cyber Orbit` | `សាយប័រ` | Usual tech loan for "cyber". 3px over on the world title. |
| `100% Offline & Kid-Safe:` | `100% អហ្វឡាញ សុវត្ថិភាពកុមារ:` | `អហ្វឡាញ` is the app loan. `គ្មានបណ្ដាញ` does not fit with `កុមារ`. 164px vs 153px. |
| `100% offline, kid-safe, no trackers` | `100% អហ្វឡាញ សុវត្ថិភាព គ្មានតាមដាន` | "Kid" is only `សុវត្ថិភាព` here. All three claims plus `កុមារ` do not fit. |
| `Next Map ▶` | `ទៅផែនទី ▶` | "Next" is only the arrow. `ផែនទីបន្ទាប់ ▶` is 74px vs 64px. |
| `LVL N` chips | `កម្រិតN` | Digit is glued to the word. A space makes `LVL 11` a fit error on a 31px chip. |
| `Sound FX Volume` | `សំឡេងបែបផែន` | Did not use `កម្រិតសំឡេង`, because `កម្រិត` is the stage word. |
| `Galaxy Janitor King` | `ស្តេចសម្អាតកាឡាក់ស៊ី` | `ស្តេច` is playful, not royal vocabulary. Still worth a check. |
| `Party Popper Hole` | `ប្រហោងបាញ់បុណ្យ` | "Popper" is a guess. |
| `Fiesta vortex…` | `ខ្យល់កួចបុណ្យ…` | Dropped the loan `Fiesta`. |
| `big anime eyes` | `ភ្នែកធំ` | Dropped `anime`. |
| `wacky Daily Shift` | `វេនថ្ងៃនេះ` | Dropped `wacky`. |
| `centrifugal` | `ល្បឿនទន់` | Dropped the science word. |
| `gauntlets` | `ច្រក` | Read as gates, not gloves. |
| `POOF!` | `ផូវ!` | Sound, not a translation of a verb. |
| `YAY!` | `យេ!` | Cheer loan. |
| privacy `SDKs` | `ការតាមដានពីក្រៅ` | Meaning kept, acronym dropped. |
| privacy `cookies` | `ខូគី` | Common tech loan. |
| `Master Dual-Ring Orbital Gauntlets` | `ច្រកចិញ្ចៀនពីរលើគន្លង` | Compressed. |
| `Pinball Bumpers, Forking Gauntlets & Boost` | `កាងភីនបល ច្រកបំបែក និងបង្កើន` | Mechanic names are guessed. |
| `Concentric Planetary Dual-Track Gearing` | `ធ្មេញពីរផ្លូវជុំវិញភព` | Compressed. |
| `Switchbacks & One-Way Flappers` | `ផ្លូវបត់ និងស្លាបម្ខាង` | "Flappers" as one-way flaps. |
| `Ratchet clicks, pops, bursts` | `សំឡេងចុច ផ្ទុះ និងបែក` | Sound list, not the word ratchet. |
| `Haptic Vibration` | `ការញ័រ` | Dropped "haptic". The detail line still says the phone buzzes while spinning. |
| `Background space synth melody` | `ភ្លេងផ្ទៃអវកាស` | Dropped "synth". |
| `Cosmic Ambience` | `បរិយាកាសអវកាស` | A bit formal for a music row. |
| `Nibbles the Void` | `Nibbles ទីទទេ` | Skin name. `ទីទទេ` is "empty place". |
| `Rubber Duckie Floatie` | `ពោងទាលេង` | "Floatie" as a pool float. |
| `Frosted Donut Rig` | `ដូណាត់ស្រោបស្ករ` | "Rig" dropped. |
| `Berry Swirl Vortex` | `ខ្យល់កួចផ្លែប៊ឺរី` | `ប៊ឺរី` is a loan. |
| `Mint Soda Lagoon` | `បឹងសូដាមីន` | `មីន` is a loan. |
| `User Agreement` | `កិច្ចព្រមព្រៀង` | "User" dropped so the row fits. The subtitle says it is the rules of the game. |
| `Restart Sector` | `ចាប់ផ្ដើមផ្នែក` | "Start the sector", not an explicit "again". |
| `Awesome, Got It!` | `អស្ចារ្យ យល់ហើយ!` | Button. |
| `SHIFT INCOMPLETE!` | `វេនមិនទាន់រួច!` | Gentle "not finished yet", not `ចាញ់`. |
| `Launch Backup Pod (+8 Scrap!)` | `បាញ់កន្សោមបម្រុង (+8 សំណល់ដែក!)` | `បាញ់` is "fire/launch". `ដាក់` would be "send out". |

Spelling I would want checked even where the glossary fixed the string: `សំរឹទ្ធ`, `ប្រាក់` (with `៍`), `អណ្ដែត`, `ប្ដូរ`, `ចាប់ផ្ដើម`, `បណ្ដាញ`, `ស្ត្របឺរី`, `ខេស្លឺ`, `ស៊ីងហ្គុយឡារីតេ`, `សាយប័រ`, `ភីនបល`.

## Glossary terms not used as the headword

Checked terms (`check: yes`) are in the strings that contain them. The validator reported no glossary miss.

| Term | What happened |
| --- | --- |
| `chute` `ផ្លូវរអិល` | Kept on bubblegum chutes and on `Cheese Moon Chutes`. Moon level cards are `ផ្លូវចន្ទ` so they fit. |
| `Toy Shop` `ហាងតុក្កតា` | Used, with the doll-shop doubt above. |
| `stipend` | Used. It is formal next to `លុយហោប៉ៅ`. |
| `cleared` `បានឆ្លង` | Used, with the disease reading above. |
| `level` `កម្រិត` | Used. Not used for the volume slider. |
| `cadet` `សិស្ស` | Used. The shift name is the awkward line. |
| `singularity` | Used as `ស៊ីងហ្គុយឡារីតេ`. |

`check: no` words that were shortened on a control: moon-chute `រអិល`, bubblegum `ស្ករកៅស៊ូ` on the numbered cards, `Next Map` "next".

## `{0}` lines that were reshaped

The indexes and the digit order match English. Khmer does not sit against `{0}`.

| Key | English shape | Khmer shape |
| --- | --- | --- |
| `pause.quota` | `Feed Nibbles {0} kg ({1}/{0} kg eaten)` | `ឱ្យ Nibbles ញ៉ាំ {0} kg (បានញ៉ាំ {1}/{0} kg)` |
| `Goal: Feed Nibbles 20 kg (0/20 kg eaten)` | same pattern, fixed numbers | `ញ៉ាំ 20 kg (ញ៉ាំ 0/20 kg)` so the pause line fits |
| `home.mission.goal` | `Goal: {0} kg` | `គោលដៅ: {0} kg` |
| `home.mission.world_fallback` | `World {0}` | `ពិភព {0}` |
| `dock.daily.new` | `{0} NEW` | `{0} ថ្មី` |
| `daily.countdown` | `Resets in {0}h {1}m` | `ឡើងវិញ {0}h {1}m` (`h` and `m` stay; `{0}` touches `h`, not Khmer) |
| `shop.footer.unlock` | `Unlock ({0} 🪙)` | `ដោះសោ ({0} 🪙)` |
| `toast.trophy` | `Trophy: {0} (+{1} 🪙)` | `ពាន: {0} (+{1} 🪙)` |
| `toast.sector.cleanup` | `Clean up {0}!` | `សម្អាត {0}!` |
| `unit.kg`, `unit.quota`, `unit.plus_coins`, `home.level.title` | unit or symbol templates | copied |

Gift fragments, read in order: `ទទួលឥតថ្លៃប្រចាំថ្ងៃ +100` + `លុយហោប៉ៅ។`, and `ថ្ងៃស្អែកមកយក +100 ទៀត` + `កញ្ចប់អំណោយ!`. The noun is the second run, so it is not the most natural Khmer order. The one-string shop lines put the noun earlier: `ទទួលលុយហោប៉ៅឥតថ្លៃ +100 🪙។`

`Unlock (500` and the other prices stay open, matching English. The closing `)` is the next run and is not a key.

## English source that looks wrong

- Gift runs in the spec HTML have a trailing or leading space (`+100 ` / ` care package!`). The keys are trimmed, so a lookup on the raw run can miss.
- `Unlock (1200` and `Trophy: Daily Duty Star (+500` are cut off on purpose. The next run is `)`.
- Backup Pod and Auxiliary Scrap Pod are the same object. Both use `កន្សោមបម្រុង`.
- The balance is called recycling coins, gameplay coins, and gold coins. All three became `កាក់មាស`.
- `Lost In Void` is title case and sits with the stat labels. The role is not labeled in the key index.
- Loading tips use ASCII `...`. The Khmer uses `…` (U+2026).
- `localStorage`, `SDKs`, `cookies`, `centrifugal`, `anime`, `Fiesta`, and `wacky` are odd in child copy.
- `SHIFT INCOMPLETE!` is harsher than the hungry-Nibbles line under it.
- `Prize` is run index 1 and `groups-truncated`. The amount is a sibling run, not part of this key.
- Endonyms and bare `+100` / `)` live in `doNotTranslate` and are not spec or str keys.

## Fit warnings left visible

| Key | Estimate | Why it stays |
| --- | --- | --- |
| `100% Offline & Kid-Safe:` | 164px vs 153px | Full `អហ្វឡាញ` plus `សុវត្ថិភាពកុមារ`. The fitting line drops `កុមារ`. |
| `Cyber Donut Rings` | 128px vs 125px | `ចិញ្ចៀនដូណាត់សាយប័រ`. |
| `Donut Rings` | 70px vs 65px | Glossary `ចិញ្ចៀនដូណាត់`. |
| `FREE` | 24px vs 23px | `ឥតថ្លៃ`. Bare `ឥត` fits and is incomplete. |
| `Kessler Hero` | 82px vs 81px | `វីរបុរសខេស្លឺ`. |
| `Prize` | 41px vs 34px | `រង្វាន់`. Already the short noun. |
| `Singularity Horizon` | 142px vs 139px | Glossary spelling plus horizon. |

None of these cross the 72% shrink floor. `LVL 11` did, until the space before the digits was removed.
