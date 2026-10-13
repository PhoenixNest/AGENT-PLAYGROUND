# Translation process — Orbital Salvage

Phase 1 kit for the 16 non-English languages. English stays the source. This adapts the Language Translation Module and the Translation Verification Report (TVR) in the AGENT-GLOBAL-BASE localisation skill to this game's JSON tables. The company names in that skill (CTO-L, linguist roster) are the shape of the report. Here the lead fills them.

Tools live in `localization/tools/`. Python 3, standard library, plus fontTools when the glyph check should read real cmaps. Use the virtualenv at `/tmp/maze-i18n/kit-venv`. Do not create a virtualenv inside the repo.

## 1. Extract

The extractor writes `Assets/Resources/Localization/en.json` and `localization/key-index.csv` from the parity specs. Run it again when UI copy changes. `str` keys are read from `en.json` dynamically; the kit does not keep its own key list.

A string freeze is the commit the lead names. After that commit, no new user-visible English string lands on the release branch without the lead's note. The freeze is a process rule. This kit does not add a CI workflow.

## 2. Hashes

`meta.sourceHashes` records a SHA-256 for every English value:

```text
sha256( section + "\n" + key + "\n" + english value )
```

`merge_table.py` rewrites those hashes from the current `en.json`. `make_translation_stubs.py` writes them too. `validate_table.py` warns `stale` when a stored hash does not match, or when `sourceHashes` is present and a key has no hash. If `sourceHashes` is absent, there is no stale warning (a first draft has not been merged yet).

When English changes after translation:

1. Re-extract or edit `en.json`.
2. Run `merge_table.py` so the canonical table gains new keys (still in English) and refreshes hashes.
3. The validator warns `stale` on every key whose English value changed. Retranslate those keys only.
4. A new key that is still English is `untranslated` (an error), unless it is on the do-not-translate list.

The warning does not by itself fail the process exit code. The TVR gate below still requires zero stale warnings, or a written waiver per key in the language notes.

## 3. Translate

One agent per language, in parallel. The prompt is `translator-brief.md`. The glossary is mandatory. The deliverable table is `Assets/Resources/Localization/<code>.json` (with a Unity `.meta` when the file is new). Notes are `localization/notes/<code>-notes.md`. Accepted fit warnings go in `localization/notes/<code>-fit-exceptions.json`. Keys that stay identical to English go in `localization/notes/<code>-identical-ok.json` (`{"key", "reason"}` per entry). Agents write those paths. They do not leave the table only in a scratch directory. They do not add language-specific strings to `doNotTranslate`.

Stubs, if you need a skeleton to diff against:

```bash
python3 localization/tools/make_translation_stubs.py --lang de --out /tmp/maze-i18n/translation-stubs
```

The default directory is that scratch folder. The script refuses to write inside the git worktree unless `--allow-repo` is passed. A stub is English with `meta` filled. It is not a translation. Do not commit it.

Merge a returned partial table:

```bash
python3 localization/tools/merge_table.py \
  --lang de --partial /tmp/de.json \
  --into Assets/Resources/Localization/de.json \
  --out /tmp/de.merged.json
```

Omit `--into` to start from English. The merge keeps English key order, drops unknown keys, and recomputes hashes. Point `--out` at `Assets/Resources/Localization/<code>.json` when that file should be updated. Validation runs on that path.

## 4. Validate

```bash
python3 localization/tools/validate_table.py --lang de \
  --table Assets/Resources/Localization/de.json \
  --json /tmp/de.report.json
```

Exit 1 means at least one error. Warnings are printed and do not change the exit code. `--max-ratio` overrides the Latin length error ratio (default 2.5). `--fonts-dir` defaults to `Assets/Fonts`. Chains are `tools/fonts-chains.json`.

`kg`, `/`, and `+` are counted as tokens, not as letters inside a word. `{0} kg`, `{0}kg`, and `{0}` with U+00A0 before `kg` may be copied; deleting `kg` is an error. Parenthesis and bracket counts must match English, including a fragment that is cut off (`Unlock (1200`). Glossary aliases live in the second block of `glossary.md` (`LVL` → `level`, `space junk` → `space trash`, `Auxiliary Scrap Pod` → `backup pod`). A hit inside the logo or another brand string does not count. A glossary loanword that is the whole string (`Donut`, `Pause`, `Bronze`, including `🍩 DONUT 🍩`) may be copied.

When `localization/notes/<code>-fit-exceptions.json` exists, those fit warnings are listed in the summary and omitted from the warning count. `--show-suppressed` prints them. `--ignore-fit-keys` adds keys for one run. Fit errors stay. `--register-check` warns once that imperative and telegraphic style is not scored. It does not add style rules.

`localization/notes/<code>-identical-ok.json` lists keys that may equal English (German `Hangar` and `Reset`, French `Hangar`, Indonesian `level` and `orbit`). The summary line `accepted identical` names them, and `--show-suppressed` prints each one. A key that is not on that list is still an `untranslated` error. `merge_table.py` recopies `doNotTranslate` from English and ignores extra entries in the partial or the canonical table.

Fix every error before review. Read the warnings. Glossary hits are warnings on purpose: the lead accepts or rejects them at review.

## 5. Sample review

Thirty keys per language, for the user to read side by side:

```bash
python3 localization/tools/review_sample.py --lang de \
  --table Assets/Resources/Localization/de.json \
  --out /tmp/de.review.md
```

The picker takes the 5 strings with the most `{n}` placeholders (digit runs break ties; this table is still mostly spec text), then the 10 longest, then fills to 30 by walking the first five keys of each screen area (trophies, daily, maps, debrief, defeat, pause, settings, shop, play). Columns are English, translation, and the UTF-16 character ratio.

The user marks the sample done in the language notes. That mark is a TVR input. It is not a native-speaker certification. Khmer, Uzbek, Filipino, and Hindi still need the native review named in the glossary.

## 6. Layout audit

`qps` is the pseudo-locale. It is not a language and it is not saved. It is built in memory from `en.json`: Latin-1 look-alikes, padded about 40% with `~`, wrapped in `[` `]`. Digits, placeholders, emoji, `/`, `×`, and `▶` stay put. Set PlayerPrefs `os.parity.lang` to `qps` or a registry code before launch.

The overflow audit walks the spec files without painting. Hard findings are text outside the element, overlap with a sibling icon, text past the home-indicator band, and a wrap inside a single-line control. Shrink, ellipsis, and a line that is still wider than the box are soft. Single-line controls shrink to 72% and then ellipsize. They never wrap.

Run the audit on `qps` first (expansion), then on the real table. Hard count must be 0 before the TVR.

## 7. Device check

Read a device or a simulator for Arabic (RTL, arrow `←` still left-pointing until the lead says otherwise), a long German button, and one CJK language when its font is no longer pending. Shaping for Arabic, Hindi, and Khmer is not in the renderer yet. That is an engine limit. Translations are still written in the real script.

## 8. TVR

The lead writes the Translation Verification Report when the gates below pass. Structure, adapted from the localisation skill:

```markdown
# Translation Verification Report — Orbital Salvage: Space Trash Clean

**Date:** YYYY-MM-DD
**Build:** <commit>
**String freeze:** <commit>

## Coverage

| Locale | Strings | Translated | Errors | Stale | Glyph | Review |
| ------ | ------: | ---------- | -----: | ----: | ----: | ------ |
| de     |     355 | 355        |      0 |     0 |     0 | done   |

## Automated QA

| Check                    | Result                     |
| ------------------------ | -------------------------- |
| Placeholder integrity    | pass / fail                |
| Missing and extra keys   | pass / fail                |
| Untranslated copies      | pass / fail                |
| Glyph check              | pass / fail / pending font |
| Pseudo-locale audit hard | 0                          |
| Real-table audit hard    | 0                          |

## Glossary and tone

Controlled glossary used: yes. Exceptions listed in the language notes.

## Verdict

APPROVED or BLOCKED, and why.
```

Linguist attestation stays blank until a native reviewer has signed the notes. A draft glossary is not that signature.

## Quality gates

All of these, or the TVR stays blocked:

| Gate      | Rule                                                                                                                                                 |
| --------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| Coverage  | 100% of `spec` and `str` keys. No missing, empty, or untranslated strings outside the do-not-translate list.                                         |
| Validator | 0 errors. Fit warnings in `notes/<code>-fit-exceptions.json` and identical copies in `notes/<code>-identical-ok.json` are accepted and do not count. |
| Glyphs    | 0 `glyph` errors. A `font-pending` warning is allowed only for a language whose chain is still pending, and it is written in the TVR.                |
| Audit     | Hard findings 0 on `qps` and on the real table.                                                                                                      |
| Stale     | 0 `stale` warnings, or one waiver per key in the notes.                                                                                              |
| Review    | The 30-key sample is marked done by the user.                                                                                                        |
| Glossary  | Warnings are triaged in the notes. They do not fail the exit code. An accepted exception is listed in the TVR.                                       |

LQA severities from the skill, mapped onto this kit: a broken placeholder, an offensive line, or a wrong meaning is Critical (block). A glossary miss the lead does not accept, or a grammar error a player will see, is Major (fix before the TVR). A punctuation nit is Minor (next pass). A nicer synonym is a suggestion.

## What this kit does not do

It does not run Unity, or shape Arabic, Hindi, or Khmer. Baloo 2, Kantumruy Pro, and Cairo are still pending. The CJK subsets (ZCOOL KuaiLe, Huninn, M PLUS Rounded 1c, Jua) are shipped. It does not decide the app name. Logo strings stay in English until that decision.
