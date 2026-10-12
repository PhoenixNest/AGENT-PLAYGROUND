# Localization

English is the source language. Missing keys fall back to English. This folder is the string-table handoff. English paint does not read these tables. Any other active language does.

Runtime code is `OrbitalSalvage.Loc` (no UnityEngine, so Logic and App can reference it). `OrbitalSalvage.Loc.Unity` holds the Resources loader and the device-locale provider.

## Languages

| Code    | English name          | Endonym          | Script | RTL | Font group |
| ------- | --------------------- | ---------------- | ------ | --- | ---------- |
| en      | English               | English          | Latn   |     | latin      |
| zh-Hans | Chinese (Simplified)  | 简体中文         | Hans   |     | cjk-sc     |
| zh-Hant | Chinese (Traditional) | 繁體中文         | Hant   |     | cjk-tc     |
| ja      | Japanese              | 日本語           | Jpan   |     | cjk-jp     |
| ko      | Korean                | 한국어           | Kore   |     | cjk-kr     |
| de      | German                | Deutsch          | Latn   |     | latin      |
| fr      | French                | Français         | Latn   |     | latin      |
| es      | Spanish               | Español          | Latn   |     | latin      |
| it      | Italian               | Italiano         | Latn   |     | latin      |
| pt-PT   | Portuguese            | Português        | Latn   |     | latin      |
| vi      | Vietnamese            | Tiếng Việt       | Latn   |     | latin-vi   |
| fil     | Filipino              | Filipino         | Latn   |     | latin      |
| id      | Indonesian            | Bahasa Indonesia | Latn   |     | latin      |
| uz      | Uzbek                 | Oʻzbekcha        | Latn   |     | latin      |
| km      | Khmer                 | ភាសាខ្មែរ        | Khmr   |     | khmer      |
| hi      | Hindi                 | हिन्दी           | Deva   |     | devanagari |
| ar      | Arabic                | العربية          | Arab   | yes | arabic     |

`en` is the default. `uz` is the stored code (BCP-47 `uz-Latn`). `pt`, `pt-BR`, and `pt-PT` resolve to `pt-PT`. `tl` resolves to `fil`. Legacy Android `in` resolves to `id`. `zh-TW`, `zh-HK`, and `zh-MO` resolve to `zh-Hant`. `zh-CN`, `zh-SG`, and bare `zh` resolve to `zh-Hans`. `uz-Cyrl` does not resolve. Unknown tags resolve to nothing, and the runtime then uses `en`.

## Table file

`Assets/Resources/Localization/<code>.json`, UTF-8, LF, 2-space indent:

```json
{
  "meta": { "code": "de", "name": "Deutsch", "version": 1, "source": "en" },
  "spec": { "<normalised English>": "<translation>" },
  "str": { "<dot.key>": "<translation with {0}>" },
  "doNotTranslate": ["250", "/"]
}
```

`name` is the endonym. `spec` keys are normalised English run text from the parity specs. `str` keys are dot keys for C# literals (`toast.shop.unlocked`, `loading.tip.3`). `{0}` placeholders are filled with invariant formatting. The English text for those keys lives in `LocStrings` and is copied into this file by the extractor. Numbers go through `JsNumber.ToString`, so a German device still sees `1.5`, not `1,5`. The loader ignores `doNotTranslate`.

## Key rules

Normalise: trim, and collapse every whitespace run to a single space. That string is the spec key.

`TryTranslateSpecText` returns the translation trimmed. It does not copy the source run's leading or trailing spaces (those spaces are layout around inline emoji). For `en` it returns false, so the original English run is left as captured.

A normalised run is not translated when it has no Unicode letter. That skips digits (`250`, `+100`), joiners (`/`), symbols (`×`, `▶`), and emoji-only runs. They are listed in `doNotTranslate`. A run that contains a letter stays translatable even if digits are baked in (`105 kg`); the CSV sets `hasPlaceholderLike`.

An endonym from `LanguageRegistry`, and that same endonym with the current-language marker (`✓ ` plus the endonym), is also `doNotTranslate`. Endonyms are never translated. The placeholder legal URLs are not spec strings. They live in `LegalLinks` (App) and `LEGAL_LINKS` in `web/main.js`.

`doNotTranslate` is the English list. `merge_table.py` recopies it and drops anything a language added by hand. A key that should stay identical to English (German `Hangar` and `Reset`, French `Hangar`, Indonesian `level` and `orbit`) is listed in `localization/notes/<code>-identical-ok.json` as `{"key", "reason"}`. `validate_table.py` then accepts that copy and prints it on the `accepted identical` summary line. `--show-suppressed` lists the rows.

## CSV

`localization/key-index.csv` is the translator handoff, one row per spec key and then one row per `str` key. Spec keys stay in ordinal order. `str` keys follow, also ordinal.

`key`, `kind` (`spec` or `str`), `files` (`count: first; second; third` zero-spec names), `element_ids` (up to 5), `element_classes` (up to 3), `fz` (first non-empty), `uppercase` (`uppercase` if any occurrence uses that text-transform, otherwise the first `tt`), `max_width` (largest `run.r[2]` in CSS px, else the element width, else blank), `chars` (Unicode code points of a spec key, or of the English text for a `str` row), `hasEmoji`, `hasPlaceholderLike`, `group` (element id, or `file#index` when there is no id, up to 8), `run_index`, `notes`, `placeholder_count` (distinct `{n}` indexes; blank on spec rows), `max_chars` (chip or button budget; blank when there is no tight budget, and blank on spec rows).

`notes` contains `fragment` when the run sits in an element with two or more text runs, and `no-terminal-punct` when the key does not end with sentence punctuation. On a `str` row, `notes` is the translator context from `LocStrings`.

## C# strings

Player copy that C# builds itself (toasts, quota lines, shop footers, loading tips, floating words) is a `LocStrings` row, not a spec key. Add a row: key in `area.screen.element` form, English with `{0}` placeholders so a translation can reorder, and a short context note (where it shows, a character budget when the control is a chip or button, and which emoji must stay). Call `Loc.Tr(key)` or `Loc.Tr(key, args)`. English reads the registry and does not need `en.json`. Units such as `kg` and the coin emoji belong in the English text.

Do not put catalog, level, world, achievement, or daily-shift sentences into `LocStrings`. Those stay English in the Logic tables. Captured spec runs translate them. A C# `Text()` override is the exception: that override replaces the spec run, so the override itself goes through `Loc.Tr`. When the override inserts one of those English data names, pass it through `Loc.TrName`. English returns the same string and does not open a table. Any other language uses a `data.*` registry row when the English matches, then the spec table. `LocNameCache` remembers that result per text and language version on the per-frame rebuild.

Per-frame playfield words use `LocMemo` or `LocNumCache` so a repeated English label does not allocate. Run the extractor after adding a row. `LocHardcodedStringTests` fails if a new English literal in the UI sources is not `Loc.Tr` or listed in `localization/hardcoded-allowlist.txt`. `LocStringTests` fails if a `Loc.Tr("...")` key is missing from the registry, or a registry key is unused (unless the row is marked dynamic).

## Workflow

1. Extract. Menu `Orbital Salvage/Tools/Extract Localization Spec Strings`, or batch `OrbitalSalvage.Editor.LocExtract.RunBatch`. It reads `Assets/Resources/Parity/Spec/*__zero.json` and writes `en.json` plus this CSV. `*__iphone.json` must yield the same key set. Spec JSON is not modified.
2. Translate. Copy `en.json` to `<code>.json` and replace `spec` and `str` values. Keep placeholder indexes identical. Do not add edge whitespace or control characters. Leave `doNotTranslate` as the English list. Keys that stay English on purpose go in `localization/notes/<code>-identical-ok.json`.
3. Validate. `LocValidator.Validate(english, translation)` returns a flat list of findings (`error` or `warning`): missing keys, extra keys, placeholder mismatches, edge whitespace, control characters, and — for latin and latin-vi only — length at or above 1.6× English (warning) or 2.5× (error).

## Runtime

`Loc.SetLanguage` accepts a registry code only (`de`, not `DE` or `pt-BR`). The language is active only when its table loads. `en` is always active. `Loc.Version` increments when the active language changes so later paint caches can drop.

`settings.language` is omitted while empty, so an unset profile stays byte-identical to today's save. `SetLanguage` is what writes a registry code. Boot does not rewrite a stored value: a hand-edited tag is resolved only to choose the active language. A stored code whose table is missing, or that does not resolve, stays stored and the active language is English. Device detection runs only when the setting is empty. A non-English result is persisted only when that table loaded, and from then on the stored value is authoritative. English detection is not persisted. A progress reset keeps the language.

The default provider reads `Application.systemLanguage` and `CultureInfo.CurrentUICulture.Name`. Tests inject `IDeviceLocaleProvider`. Native `NSLocale` / Android `Locale` lookup is later.

## Settings picker

Settings has a Language row. The value is the saved language's endonym (English when nothing is saved). Change opens `settings_modal_language`, a two-column grid of the 17 endonyms with no scroll. The current language is a leading `✓ ` (`U+2713`) on that button. The English capture marks English. Unity replaces the run text of `settings-language-value`, removes the marker from `btn-lang-en` when another code is saved, and adds it on `btn-lang-<code>`. Title and Close stay in the spec and are translated later with the other spec strings. Run positions stay the captured ones. `HomeChrome.PaintRuns` is not changed for this.

A tap saves the registry code and closes the picker, including a tap on the code that is already saved. A code whose table is missing is still saved. The painted chrome stays English until that table loads. The marker follows the saved code.

Privacy Policy and User Agreement both open `https://www.bing.com` for now (`LegalLinks`, and `LEGAL_LINKS` on the web). The privacy modal markup and its spec remain. The button does not open the modal. Those URLs are placeholders.

TODO: Kids Category. An external link in a kids app may need a parental gate before the App Store Kids Category. None is built here.

## Rendering rules

`HomeChrome.PaintRuns` reads `Loc.EnglishActive` once per run. When that bool is set, the run stays on the captured English path (cached `ToUpperInvariant`, baked boxes, `WrapWords`). A language change bumps `Loc.Version`. The next frame drops chrome bakes, layout caches, and the text-layout cache, then rebuilds one resident page per frame.

Anchoring keeps the center when the captured run is within 2px of the element center, or a centered button's icon+text pair is. It keeps the right edge when the run ends within 1.5px of the element and starts at least 8px in. Otherwise it keeps the left edge. Available width is the element width minus 16px (`.btn-cartoon-primary`). A shrink-wrapped run that is already wider than that inset relaxes the pad to the element edge.

Fit order for a translated or overridden string: one line if it fits. A single-line control (a button, or a run no taller than `1.5 * font + 2` inside an element shorter than `2.2 * font`) shrinks in 2% steps to 72% and then ellipsizes on that one line. Wrapping is only for a run that was already multi-line, or a text block taller than two lines. Extra lines shift the next run in the same element down, and stop at the element bottom, the next sibling, and the home-indicator band. Otherwise shrink, then clip with `…`. The result stays inside the element box within 0.5px.

A child emoji, including one wrapped in a span, or a one-glyph symbol beside the run reserves its box. One that already overlaps the English run is reserved only when the translation is wider; a shorter translation stays on the captured x. A centered button shifts a leading icon with the text when the pair still fits. The home play label is a shrink-wrapped span and its ▶ is a sibling, so the label stays in its own box and the icon is not moved.

`LineBreaker.For` is Latin for `en`, `de`, `fr`, `es`, `it`, `pt-PT`, `vi`, `fil`, `id`, `uz`, and for `qps`. Latin splits on spaces and matches `WrapWords`. CJK (`zh-Hans`, `zh-Hant`, `ja`) breaks between characters, keeps Latin words and number+unit tokens together, and applies kinsoku. Korean breaks on spaces, then per syllable when one word is too wide. `km`, `hi`, and `ar` still use the Latin breaker.

Non-English `text-transform: uppercase` goes through `LocCase.Upper`. German `ß` becomes `SS`. `zh-Hans`, `zh-Hant`, `ja`, `ko`, `km`, `hi`, and `ar` are not uppercased, so a Latin token such as `kg` stays as written.

`qps` is not in the language list and is never written to the profile. Set PlayerPrefs `os.parity.lang` to `qps` or a registry code before launch. Boot reads it once, and only when the key is set. The `qps` table is built in memory from `en.json`: Latin-1 look-alikes, padded to +40% with `~`, wrapped in `[` `]`. Digits, `{n}`, emoji, `/`, `×`, and `▶` stay as they are.

Overflow audit: menu `Orbital Salvage/Tools/Localization Overflow Audit`, or batch `OrbitalSalvage.Editor.LocOverflowMenu.RunBatch` with `-loc-audit-lang` (default `qps`) and `-loc-audit-out`. It walks `*__zero.json` and `*__iphone.json` without painting. Hard findings are text outside the element, overlap with a sibling icon, text past y 898.25 on iPhone, and `missing-glyph` (a translated character with no cmap hit in that language's chain). Shrink, ellipsis, extra wrap, and a line that is still wider than the available width are listed and are not hard. Elements with two or more runs are counted and still translated one run at a time. The glyph check ignores whitespace, controls, U+FE0F, U+200D, U+200B, and emoji graphemes drawn as sprites.

## Fonts

`Assets/Resources/Localization/fonts.json` is the face manifest. `LocFonts.GetChain(language, family, cssWeight)` reads it (the loader is injectable) and returns ordered face names. `OutlineFonts.ChainFor` loads those files from `Resources/Fonts/Ttf/<name>.bytes` and skips a missing file. It does not throw. A missing script face falls back to the German Latin chain.

A face lists the CSS weights it serves. One weight is used for every other CSS weight. There is no synthetic bold. Fredoka still resolves 800 and 900 to 700. Nunito still resolves 400 to 700. An unknown family still resolves to Nunito 700, matching `OutlineFonts.Primary`.

| Group | Display / body | Chain |
| --- | --- | --- |
| en | Fredoka 400/600/700, Nunito 700/800/900 | primary, NotoSansSymbols2, NotoSansSymbols |
| de, fr, es, it, pt-PT, fil, id, uz, vi, qps | same faces | primary, Nunito at the same CSS weight when the family is Fredoka, then the two symbol fonts |
| cjk-sc | ZCOOL KuaiLe, one weight, all CSS weights | ZCOOL, Nunito, symbols |
| cjk-tc | Huninn, one weight | Huninn, Nunito, symbols |
| cjk-jp | M PLUS Rounded 1c Bold (700) | M PLUS, Nunito, symbols |
| cjk-kr | Jua, one weight | Jua, Nunito, symbols |
| devanagari | Baloo 2 Bold (700), not shipped | Baloo 2, Nunito, symbols, or the Latin chain when the file is missing |
| khmer | Kantumruy Pro Bold (700), not shipped | same shape |
| arabic | Cairo Bold, slant 0 (700), not shipped | same shape |

English paint does not call `LocFonts`. `HomeChrome.PaintRuns` takes the captured loop when `Loc.EnglishActive` is set and `TextLangOverrides` is empty.

The 17 `btn-lang-<code>` runs use that language's display chain even when the active language is English. `ParityViewer.WriteDynamicOverrides` fills the map on the settings page and while the language modal is open. `km`, `hi`, and `ar` still draw `.notdef` boxes until those files and shaping land. Baloo 2 is not shipped in this task: Nunito already covers Vietnamese and Uzbek, and Hindi boxes are expected until shaping.

To add or replace a face: put a static `glyf` `.ttf` in `Assets/Fonts`, add the name to `fonts.json` for that group, add it to `ParityAssetBuilder.OutlineExtraFonts`, and copy the bytes to `Resources/Fonts/Ttf`. Rebuild with `localization/tools/make_fonts.sh` (venv `/tmp/maze-i18n/fonts-venv`, downloads `/tmp/maze-i18n/fonts/dl`). Do not commit a variable font. `subset_fonts.py --variations wght=700` pins axes with `fontTools.varLib.instancer` before subsetting.

The four CJK files are temporary endonym subsets (the endonym plus printable ASCII). A later wave replaces the file for the same group. Jua's source has no U+0060, so that one ASCII slot is absent. Ship the OFL text beside the font.

| File | Bytes | Licence |
| --- | ---: | --- |
| `ZCOOLKuaiLe-endonym.ttf` / `.bytes` | 7004 | OFL 1.1, Copyright 2018 The ZCOOL KuaiLe Project Authors. `ZCOOLKuaiLe-OFL.txt` (4398) |
| `Huninn-endonym.ttf` / `.bytes` | 13708 | OFL 1.1, Copyright 2025 The Huninn Project Authors. `Huninn-OFL.txt` (4382) |
| `MPLUSRounded1c-endonym.ttf` / `.bytes` | 12208 | OFL 1.1, Copyright 2016 The Rounded M+ Project Authors. `MPLUSRounded1c-OFL.txt` (4350). Upstream has no `OFL.txt`; the sidecar is name IDs 0 and 13 plus the OFL 1.1 text |
| `Jua-endonym.ttf` / `.bytes` | 22868 | OFL 1.1, Copyright 2018 The Jua Project Authors. `Jua-OFL.txt` (4342) |

Fredoka, Nunito, and the two Noto symbol fonts are unchanged. Their OFL files were already in `Assets/Fonts`.
