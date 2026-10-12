# Translator brief — Orbital Salvage: Space Trash Clean

Paste this file to the agent for one language. Fill the target line and do not change the rules.

**Target language:** `<code>` — `<endonym>`
**Date:** 2026-10-12
**Source:** English (`en.json`)

## Role

You are the translation agent for one language of *Orbital Salvage: Space Trash Clean*. You produce a translation table and a notes file. You do not edit Unity, C#, or the English source. You follow this brief, `glossary.md`, and `key-index.csv`.

The player is a child or a parent playing with a child. The tone is playful, warm, short, upbeat, and safe. No slang that will date. No sarcasm. Nibbles is a friend who is hungry, not a monster. A failed run is a gentle "not yet", not an insult.

You spin a ring to swallow space trash for Nibbles, a cartoon black hole. Coins are scrap's reward (gold coins). Worlds hold candy-named stages (Bubblegum Chute 1, Donut Float). The Toy Shop sells Donut Rings, Nibbles Hats, and Skies. Daily operations have bronze, silver, and gold shifts, trophies, and a Settings page. The game is offline and has no real-money shop.

## Read these first

1. `games/maze-puzzle/unity/maze-puzzle/Assets/Resources/Localization/en.json` — the English table. Read it when you start. Do not hard-code the key list. Another engineer may still add `str` keys.
2. `games/maze-puzzle/localization/key-index.csv` — one row per spec key, with the context columns below. `str` keys may not be in the CSV yet; translate them from `en.json` anyway.
3. `games/maze-puzzle/localization/glossary.md` — the terms you must reuse, and the style row for your language.

## CSV columns

| Column | Meaning |
| --- | --- |
| key | The English string. It is the JSON key. Copy it byte for byte. |
| kind | `spec` (text captured from the UI) or `str` (a C# string). |
| files | How many spec files use it, then the file names. |
| element_ids | Up to five element ids. |
| element_classes | Up to three CSS classes. |
| fz | Font size in CSS px, such as `11px`. |
| uppercase | `uppercase` when the renderer will uppercase the translation at runtime. Otherwise the first text-transform, usually `none`. |
| max_width | The line box in CSS px (the captured run width, else the element width). Blank if unknown. |
| chars | English length in Unicode code points. |
| hasEmoji | `true` when the run contains an emoji, including `✓`. |
| hasPlaceholderLike | `true` when digits are baked into the text (`105 kg`). This is not the same as `{0}`. |
| group | The element id, or `file#index` when there is no id. Fragments of one element share a group. |
| run_index | Which run inside the element. Several indexes mean several files. |
| notes | `fragment` when the element has two or more text runs. `no-terminal-punct` when the key does not end with sentence punctuation. |

## Output

Return one JSON object, UTF-8, no BOM, LF newlines, 2-space indent, keys in the same ordinal order as `en.json`. Top-level keys are exactly `meta`, `spec`, `str`. You may also copy `doNotTranslate` from English; the merge step will replace it from English anyway.

```json
{
  "meta": { "code": "de", "name": "Deutsch", "version": 1, "source": "en" },
  "spec": { "<english key>": "<translation>" },
  "str": { "<dot.key>": "<translation with {0}>" }
}
```

- `meta.code` is the registry code (`de`, not `DE`). `meta.name` is the endonym from the glossary's language list (Deutsch, Français, 简体中文, Oʻzbekcha with U+02BB, …).
- Every `spec` and `str` key in `en.json` appears once. No extra keys. No missing keys. Keys are copied byte for byte, including spaces and punctuation.
- Values are strings. Do not leave a value empty. Do not leave a translatable value identical to English.
- If the English word is already the right word (`Hangar` in German or French, `Reset` in German, `level` or `orbit` in Indonesian), copy it and list the key in `localization/notes/<code>-identical-ok.json` as `{"key", "reason"}`. Do not add that key to `doNotTranslate`. Merge recopies `doNotTranslate` from English and drops language-specific entries.

## Placeholders, digits, emoji

- `{0}`, `{1}`, … stay character for character. You may reorder them. `{{` is an escape, not a placeholder. The set of indexes must match.
- Keep the Western digits in the same order. `1,000` may keep its comma; do not spell the number out and do not switch to Eastern or Devanagari digits.
- `kg` stays `kg` when it is the unit: after a number (`105 kg`, `105kg`), after `{0}` (`{0} kg`, `{0}kg`, or `{0}` plus U+00A0 plus `kg`), or standing alone. A word that only contains those letters (Background) is not a unit. `{0} kg` may be copied. Do not delete `kg`.
- Keep `×`, `/`, `%`, `+`, `▶`, and `←`. A slash or plus inside a word is not one of those tokens.
- Keep every emoji and `✓` in the same relative position. A trailing check stays trailing. Do not add emoji.
- Do not add leading or trailing spaces. No control characters, including newlines.

## Length

Fit the line **without shrinking** when you can. If it will not fit, choose a shorter synonym. Never abbreviate with a period.

The character budget is the English UTF-16 length times your expansion factor. The validator warns at 1.6× that budget and errors at 2.5× that budget. Latin languages also warn at 1.6× the raw English length and error at 2.5×, which is the same rule as `LocValidator.cs`.

| Code | Factor | Why |
| --- | ---: | --- |
| de | 1.35 | Compounds and case endings. |
| fr | 1.25 | French UI runs about a quarter longer. The no-break spaces add a little. |
| es | 1.25 | Neutral Spanish UI. |
| it | 1.20 | Slightly shorter than French and Spanish. |
| pt-PT | 1.25 | European Portuguese, same band as Spanish. |
| vi | 1.15 | Latin letters, modest growth. |
| fil | 1.40 | Filipino phrases often run long. |
| id | 1.20 | Indonesian stays compact. |
| uz | 1.25 | Suffixes on a Latin base. |
| zh-Hans | 0.55 | Character count. One ideograph is about one em, so 0.55× the English character count is about the same visual width. |
| zh-Hant | 0.55 | Same as Simplified. |
| ja | 0.80 | Kana keep the count higher than Chinese. |
| ko | 0.75 | Hangul syllables are wide, but the words are shorter. |
| hi | 1.30 | Devanagari code points include the vowel signs. |
| km | 1.40 | Coeng clusters add code points. |
| ar | 1.10 | Arabic is compact. Unshaped letters are a renderer limit, not a reason to write Latin. |

`ru` is not a target language.

There is a second check against `max_width` and `fz`. It estimates width with an average advance (Latin letter 0.50 em, space 0.28, digit 0.50, CJK/kana/Hangul 1.00, Arabic 0.48, Devanagari 0.40, Khmer 0.45). The budget is the wider of the CSV box and the English estimate, so the English string itself is not flagged. It warns when the translation is wider than that line, and it errors when the width passes the renderer's 72% shrink floor (`TextFit.FloorRatio`). The 16px button inset is already outside a captured run width, so it is not subtracted again. Single-line controls never wrap. Treat a fit warning on a single-line control as a request to shorten the line.

The fit check compares the whole string with one line box. A paragraph that wraps can warn even when the wrapped lines fit. That warning is a budget, not proof of a single-line overflow. `str` rows often have no `fz` or `max_width`, so they are not fit-checked. Use `max_chars` and the context note (a dock badge is about 6 characters).

The 50 spec keys `LVL 1` through `LVL 50` are narrower than the glossary word (`Level` in German). A fit warning on those chips is expected. Ignore it, or list the keys in `localization/notes/<code>-fit-exceptions.json` so the summary suppresses them. They stay visible with `--show-suppressed`. `--ignore-fit-keys` suppresses extra keys for one run and does not write the file. A fit error past the 72% shrink floor is never suppressed.

```json
{
  "reason": "Glossary keeps Level. The LVL chip is narrower than the word.",
  "keys": ["spec:LVL 1", "spec:LVL 2"]
}
```

A key may instead be `{"key": "spec:LVL 1", "reason": "..."}`. `spec:` or `str:` may be omitted when the English key is unique.

## Uppercase

If `uppercase` is `uppercase`, write normal casing. The renderer uppercases Latin at runtime. German `ß` becomes `SS` there. Chinese, Japanese, Korean, Khmer, Hindi, and Arabic are not uppercased. Do not pre-uppercase those strings.

If the English key is already all caps and `uppercase` is `none` (BRONZE, PLAY), the player sees the table text as written. Use the case you want on screen.

## Fragments

A `fragment` note means this key is one run inside a larger element. Translate each run so that reading them in `run_index` order, in the same `group`, sounds natural. Do not add a full stop when `no-terminal-punct` is set. Do not repeat a word that the neighbouring run already carries.

Some fragments end mid-sentence. The English key is cut off on purpose: `Unlock (1200`, `Unlock (500`, `Trophy: Daily Duty Star (+500`. Do not add the missing `)`. The next run continues the sentence. The validator compares `(` `)` `[` `]` counts with English and warns when they differ. Match the English counts, even when English itself is unbalanced.

`groups-truncated` means the CSV does not list every sibling run. Read the neighbouring keys in `en.json` before you split a sentence.

## Do not translate

- Nibbles, in any language. Do not phoneticise it.
- The logo: ORBITAL, SALVAGE, SPACE TRASH CLEAN, and the sentence name Orbital Salvage. Taglines around the logo are translated.
- kg, digits, `×`, `▶`, `←`, `/`, `%`, `+`, version tokens (`v1.2.0`), and `localStorage`.
- Every endonym: English, 简体中文, 繁體中文, 日本語, 한국어, Deutsch, Français, Español, Italiano, Português, Tiếng Việt, Filipino, Bahasa Indonesia, Oʻzbekcha, ភាសាខ្មែរ, हिन्दी, العربية, and the same endonym with a leading `✓ `.
- A string that is only numbers, units, and symbols (`105 kg`, `+100`).
- The arrow `←` stays as-is, including in Arabic. Flag it in your notes if it looks wrong. Do not mirror it yourself.

## Style for your language

Follow the address row in `glossary.md`. Short version:

- de: du. Capitalise nouns. Quotes „...“.
- fr: tu. U+00A0 before `! ? : ;`. Guillemets « » with U+00A0 inside. Do not use U+202F. It is missing from the shipped fonts, and the validator warns.
- es: tú, Latin America. No vosotros, no ordenador. Curly quotes. No space before `! ?`.
- it: tu. Guillemets.
- pt-PT: tu. ecrã, telemóvel. Not Brazilian. « » with U+00A0.
- vi: bạn, or no pronoun.
- fil: ka. Standard Filipino; a little Taglish is fine.
- id: kamu.
- uz: sen. Latin only. oʻ and gʻ use U+02BB, never an ASCII apostrophe.
- hi: तुम. Devanagari. Western digits.
- ar: Modern Standard Arabic, almost no vowel marks. RTL. Western digits. Arabic comma U+060C and question mark U+061F. Latin letters only inside the allowed names and kg.
- ja: です/ます for sentences. Short nouns for buttons. No slang, no だ・である.
- ko: 해요체. Noun labels may omit the ending.
- zh-Hans: 你. Mainland words. Full-width `。！？、`.
- zh-Hant: 你. Taiwan words (軟體, 螢幕, 影片, 首頁), not mainland words. Full-width punctuation.
- km: polite kid tone, not the royal register.

Checked glossary terms must appear as recommended. If a term cannot work grammatically, translate it so the sentence is correct, keep the rest of the glossary, and list the exception in the notes. That exception is a warning, not a free pass to rename Nibbles. A headword that does not fit a header still belongs in the sentence that has room (`Toy Shop` in a sentence, a shorter word on the header). Terms with `check: no` (Freischalten, Anziehen, Abholen, and the rest of that set) are still the preferred word when the line can hold them.

`LVL` is an alias of `level`. `space junk` is an alias of `space trash`. The logo `SPACE TRASH CLEAN` is not a `space trash` hit. Backup Pod and Auxiliary Scrap Pod are one thing: use the backup-pod row for both.

## Buttons

Close, Cancel, Back, Play, Claim, Wear, and Unlock are verbs. Translate them as verbs. The German pilot's `Close` → `Zu` was too short of a word: `Close` needs `Schließen`, not a particle. If the verb does not fit, choose a shorter verb and record it in the notes. Do not drop the verb only to clear a fit warning.

## Self-check

From `games/maze-puzzle/localization/tools`, with the scratch virtualenv if you want the glyph check (`/tmp/maze-i18n/kit-venv`):

```bash
python3 validate_table.py --lang <code> \
  --table games/maze-puzzle/unity/maze-puzzle/Assets/Resources/Localization/<code>.json
```

`--show-suppressed` prints fit warnings hidden by the allowlist. `--ignore-fit-keys` adds keys for that run. `--register-check` adds one warning that imperative and telegraphic style is not scored. It does not judge buttons.

Fix every error. Read every warning. Glyph errors mean a character is not in the shipped font chain. Languages whose chain is `pending` in `tools/fonts-chains.json` (zh, ja, ko, hi, km, ar) warn `font-pending` and do not fail the glyph check yet. vi and uz are checked against Fredoka, Nunito, and the Noto symbol fonts; Baloo 2 is still pending.

## What you return

Write the files into the repo. The table path is the deliverable. The lead does not copy it in afterwards.

1. The table at `games/maze-puzzle/unity/maze-puzzle/Assets/Resources/Localization/<code>.json`. Add the Unity `.meta` beside it when you create the file. UTF-8, no BOM, LF, 2-space indent.
2. Notes at `games/maze-puzzle/localization/notes/<code>-notes.md`, with:
   - doubtful lines (key, your choice, the alternative)
   - glossary terms you could not use, and the addition you want
   - any English source line that looks wrong
   - the validator summary (error count, warning count, suppressed count)
3. `games/maze-puzzle/localization/notes/<code>-fit-exceptions.json` when you accept fit warnings, including the 50 `LVL` chips if you keep the glossary word.
4. `games/maze-puzzle/localization/notes/<code>-identical-ok.json` when a key stays identical to English on purpose. The summary line `accepted identical` lists those keys. `--show-suppressed` prints them.

`merge_table.py` is only for folding a partial draft into that table. Do not translate `qps`. That pseudo-locale is built in memory from English.
