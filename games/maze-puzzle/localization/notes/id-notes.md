# Indonesian notes — id (Bahasa Indonesia)

Draft, 2026-10-12. Informal-polite `kamu` (not Anda, not kau). Warm, short, child-safe. No native speaker has reviewed this table. The 30-key sample is at `/tmp/maze-i18n/tr-id/review-id.md` and is not marked done.

## Coverage

| | Count |
| --- | ---: |
| spec keys | 355 |
| str keys | 81 |
| validator errors | 0 |
| visible warnings | 24 |
| suppressed fit warnings | 50 |
| accepted identical | 1 (`Reset`) |

Visible warnings by kind: `fit` 23, `length` 1. The 50 suppressed warnings are `LVL 1`–`LVL 50` in `id-fit-exceptions.json` (glossary word `level`). No `stale`, `glyph`, `glossary`, `expansion`, `placeholder`, `emoji`, or `number` warnings.

`str:unit.kg` and `str:unit.quota` stay `{0} kg` and `{0} / {1} kg`. Brand lines stay English: `ORBITAL`, `SALVAGE`, `SPACE TRASH CLEAN`, `ORBITAL SALVAGE`, and `Orbital Salvage • Space Trash Clean v1.2.0`. `Nibbles` is never translated. `localStorage` stays in the privacy paragraph.

## Fit warnings left on purpose

Every one of these is under the 72% shrink floor. A shorter natural synonym either changes the meaning or drops a glossary word.

| Keys | n | Choice | Why it stays |
| --- | ---: | --- | --- |
| `LVL 1`–`LVL 50` | 50 | `Level N` | Suppressed. Glossary word `level`. The chip is narrower than the word. |
| `Moon Chute 1`–`10` | 10 | `Luncur Bulan N` | About 4–6px over. `Perosotan` (glossary chute) nearly hits the shrink floor. `Luncur` is the short slide word, used for every chute. |
| `DAILY` | 1 | `Harian` | 33px vs 32px. Dock label. `Tugas` fits and is the other half of `tugas harian`. |
| `FREE` | 1 | `GRATIS` | 27px vs 23px, floor 33px. No shorter word that means free-of-charge. |
| `Start Shift ▶` | 1 | `Mulai giliran ▶` | Verb plus glossary `giliran`. `Giliran ▶` drops the verb. |
| `TODAY` | 1 | `HARI INI` | Fit 42px vs 38px, and length 1.60× (the warn line). `Kini` means now, not today. |
| `TOY SHOP` | 1 | `TOKO MAINAN` | 90px vs 84px, floor 116px. Glossary `toko mainan`. |
| `Wear` | 1 | `Pakai` | 30px vs 29px. Glossary verb. No shorter verb. |
| `Cute Cosmic Janitor` | 1 | `Petugas kebersihan kosmik` | 157px vs 148px, floor 205px. Glossary job title. `Lucu` did not fit with it. |
| `Donut Rings` | 1 | `Cincin Donat` | 69px vs 65px. Glossary category. |
| `Prize` | 1 | `Hadiah` | 39px vs 34px, floor 47px. The word itself. |
| `Skies` | 1 | `Langit` | 36px vs 30px, floor 42px. Glossary backdrop word. |
| `Launch Backup Pod (+8 Scrap!)` | 1 | `Mulai pod cadangan (+8 rongsokan)!` | 203px vs 182px, floor 253px. Needs the verb plus both glossary words. |
| `allowance.` | 1 | `uang jajan.` | 56px vs 55px. Glossary fragment. |
| `care package!` | 1 | `paket bingkisan!` | 84px vs 72px, floor 100px. Glossary fragment. |

`settings_modal_privacy` is out of the app flow. The bodies are still translated, and they are shorter than a word-for-word pass, because the German audit showed headings sitting on wrapped paragraphs.

## Glossary words that changed shape

Checked terms are in the table. These preferred words (`check: no`, or a header that cannot hold the long form) changed:

| Term | Glossary | Used | Where |
| --- | --- | --- | --- |
| chute | perosotan | luncur | Every chute name. `Perosotan` does not fit the moon chips. |
| goal (no row) | — | misi | `Goal: 20 kg` and the pause quota. `Target` overflows the chip. |
| hangar (no row) | — | beranda | `btn-defeat-home`. The place label, paired with `Pulang` on pause. |
| pinball (no row) | — | pantul | `Nebula Pantul`. |
| cosmic janitor | petugas kebersihan kosmik | same, without `lucu` | Tagline only. The phrase is the whole line. |
| backup pod | pod cadangan | same | Button and the Auxiliary Scrap Pod toast. One word for both English names. |
| master bounty | hadiah master | same | Includes the lead's `master`. |

`space junk` and `space trash` both use `sampah luar angkasa`. Recycling coins and gameplay coins use `koin emas`. Gold shift metal is `emas`, not `koin emas`.

## Doubtful lines

| Key | Choice | Alternative |
| --- | --- | --- |
| `Reset` | `Reset` | `Ulang` fits and reads as play-again. |
| `Yes, Reset` | `Ya, reset` | Matches the short button. The dialog title is `ATUR ULANG SEMUA?`. |
| `Reset Progress` | `Atur ulang` | `Atur ulang progres` overflows the row. |
| `Hangar` | `Beranda` | Keep `Hangar` as the KBBI loanword for the building. |
| `← Back` | `← Balik` | `← Kembali` is the standard UI verb and overflows (56px vs 44px). |
| `Lost In Void` | `Di hampa` | `Tumpah` for the spilled mass, without the void. |
| `✓ UNLOCKED` | `✓ DIRAIH` | `✓ TERBUKA` pairs with `TERKUNCI` and can read as still open. |
| `Sound: ON` | `Ada suara` | `Suara: nyala` overflows the pause button. |
| `Next Map ▶` | `Lanjut ▶` | `Peta lanjut ▶` overflows. |
| `Snacks Eaten` | `Camilan habis` | `Camilan dimakan` is 1px over the line. |
| `Coins Won` | `Koin dapat` | Telegraphic. `Koin emas` names the currency and drops "won". |
| `Game Data` | `Data main` | `Data permainan` overflows. |
| `Resets in 14h 59m` | `Ulang 14 jam 59m` | `Menit` crosses the shrink floor. `m` is the English minute mark. |
| `Sleepy Star Cloud` | `Awan Mengantuk` | `Bintang Mengantuk` is 3px over. Star and sleepy do not fit together. |
| `Cosmic Kitty Saucer` | `Piring Kucing` | `Piring Kucing Kosmik` is 7px over. |
| `Cyber Donut Rings` | `Donat Siber` | `Cincin Donat Siber` is 3px over. The tab already says `Cincin Donat`. |
| `Silver Shift • Sector Hazard` | `Giliran perak • sektor` | `Bahaya` does not fit with `giliran` and `sektor`. |
| `Gold Shift • Vortex Apex` | `Giliran emas • pusaran` | `Puncak` does not fit. |
| `Bronze Shift • Cadet Sweep` | `Giliran perunggu • kadet` | `Sapu` does not fit with both words. |
| `Mint Soda Lagoon` / `Peppermint Candy Ring` | `mint` kept | Flavor loanword. `Segar` was the other option. |
| Privacy `cookies` | `kuki` | Homonym of the snack. `Cookie` is plainer and more English. |

## Twelve least-sure keys

1. `Reset` → `Reset`
2. `Hangar` → `Beranda`
3. `Goal: 20 kg` → `Misi: 20 kg`
4. `Lost In Void` → `Di hampa`
5. `✓ UNLOCKED` → `✓ DIRAIH`
6. `Moon Chute 1` → `Luncur Bulan 1` (and 2–10)
7. `Silver Shift • Sector Hazard` → `Giliran perak • sektor`
8. `Sound: ON` → `Ada suara`
9. `Resets in 14h 59m` → `Ulang 14 jam 59m`
10. `Next Map ▶` → `Lanjut ▶`
11. `Nebula Pinball` → `Nebula Pantul`
12. `Sleepy Star Cloud` → `Awan Mengantuk`

## Glossary additions requested

- Chute chip: `luncur` as the short form when `perosotan` does not fit. One word is used for every chute.
- Backup pod: `kapsul cadangan` if `pod` should not remain inside the glossary recommendation. This table follows `pod cadangan`.
- Master bounty: `hadiah juara` if `master` should not remain. This table follows `hadiah master`.
- No rows yet for goal, hangar, or pinball. This table uses `misi`, `beranda`, and `pantul`.

## English source

- `Unlock (1200` (and the other prices) and `Trophy: Daily Duty Star (+500` are cut off before the closing `)`. Indonesian does not add it.
- `Backup Pod` and `Auxiliary Scrap Pod` are one object. Both say `pod cadangan`.
- `Hangar` is the defeat home button (`btn-defeat-home`), not a separate place in the copy.
- Privacy headings and bodies still share a captured y. Shorter Indonesian does not move the next element. Left as a layout issue. The modal is out of the app flow.

## Kit

- `str` rows still have no line box, so toasts and floating words are not fit-checked. They were kept near the character notes in `key-index.csv`.
- A length of exactly 1.60× warns. `HARI INI` is 8 UTF-16 units against `TODAY` at 5, so it sits on the line.
- The fit estimate is a planning width. The German audit painted some warned compounds in full. These 23 warnings may do the same. None cross the shrink floor.
