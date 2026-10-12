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

`name` is the endonym. `spec` keys are normalised English run text from the parity specs. `str` keys are dot keys for C# literals (empty until those are wired). `{0}` placeholders are filled with invariant formatting. Numbers go through `JsNumber.ToString`, so a German device still sees `1.5`, not `1,5`. The loader ignores `doNotTranslate`.

## Key rules

Normalise: trim, and collapse every whitespace run to a single space. That string is the spec key.

`TryTranslateSpecText` returns the translation trimmed. It does not copy the source run's leading or trailing spaces (those spaces are layout around inline emoji). For `en` it returns false, so the original English run is left as captured.

A normalised run is not translated when it has no Unicode letter. That skips digits (`250`, `+100`), joiners (`/`), symbols (`×`, `▶`), and emoji-only runs. They are listed in `doNotTranslate`. A run that contains a letter stays translatable even if digits are baked in (`105 kg`); the CSV sets `hasPlaceholderLike`.

## CSV

`localization/key-index.csv` is the translator handoff, one row per spec key, ordinal order.

`key`, `kind` (`spec` or, later, `str`), `files` (`count: first; second; third` zero-spec names), `element_ids` (up to 5), `element_classes` (up to 3), `fz` (first non-empty), `uppercase` (`uppercase` if any occurrence uses that text-transform, otherwise the first `tt`), `max_width` (largest `run.r[2]` in CSS px, else the element width, else blank), `chars` (Unicode code points), `hasEmoji`, `hasPlaceholderLike`, `group` (element id, or `file#index` when there is no id, up to 8), `run_index`, `notes`.

`notes` contains `fragment` when the run sits in an element with two or more text runs, and `no-terminal-punct` when the key does not end with sentence punctuation.

## Workflow

1. Extract. Menu `Orbital Salvage/Localization/Extract Spec Strings`, or batch `OrbitalSalvage.Editor.LocExtract.RunBatch`. It reads `Assets/Resources/Parity/Spec/*__zero.json` and writes `en.json` plus this CSV. `*__iphone.json` must yield the same key set. Spec JSON is not modified.
2. Translate. Copy `en.json` to `<code>.json` and replace `spec` and `str` values. Keep placeholder indexes identical. Do not add edge whitespace or control characters.
3. Validate. `LocValidator.Validate(english, translation)` returns a flat list of findings (`error` or `warning`): missing keys, extra keys, placeholder mismatches, edge whitespace, control characters, and — for latin and latin-vi only — length at or above 1.6× English (warning) or 2.5× (error).

## Runtime

`Loc.SetLanguage` accepts a registry code only (`de`, not `DE` or `pt-BR`). The language is active only when its table loads. `en` is always active. `Loc.Version` increments when the active language changes so later paint caches can drop.

`settings.language` is omitted while empty, so an unset profile stays byte-identical to today's save. On boot, a stored code is used when it resolves and its table loads (a tag such as `zh-HK` is rewritten to the canonical code). A stored code whose table is missing stays stored and the active language is English. Otherwise the device provider is asked. A non-English result is persisted only when that table loaded, and from then on the stored value is authoritative. English detection is not persisted. A progress reset keeps the language.

The default provider reads `Application.systemLanguage` and `CultureInfo.CurrentUICulture.Name`. Tests inject `IDeviceLocaleProvider`. Native `NSLocale` / Android `Locale` lookup is later.

## Rendering rules

`HomeChrome.PaintRuns` reads `Loc.EnglishActive` once per run. When that bool is set, the run stays on the captured English path (cached `ToUpperInvariant`, baked boxes, `WrapWords`). A language change bumps `Loc.Version`. The next frame drops chrome bakes, layout caches, and the text-layout cache, then rebuilds one resident page per frame.

Anchoring keeps the center when the captured run is within 2px of the element center, or a centered button's icon+text pair is. It keeps the right edge when the run ends within 1.5px of the element and starts at least 8px in. Otherwise it keeps the left edge. Available width is the element width minus 16px (`.btn-cartoon-primary`). A shrink-wrapped run that is already wider than that inset relaxes the pad to the element edge.

Fit order for a translated or overridden string: one line if it fits; else wrap when the run is already multi-line, the run is at least 1.9× the font, or the element is a button that tall; else shrink in 2% steps to 72% with letter-spacing scaled the same way; else wrap at that floor if the element has room for two lines; else clip with `…`. The result stays inside the element box within 0.5px.

A child emoji or one-glyph symbol beside the run reserves its box. A centered button shifts that leading icon with the text when the pair still fits. The home play label is a shrink-wrapped span and its ▶ is a sibling, so the label stays in its own box and the icon is not moved.

`LineBreaker.For` is Latin for `en`, `de`, `fr`, `es`, `it`, `pt-PT`, `vi`, `fil`, `id`, `uz`, and for `qps`. Latin splits on spaces and matches `WrapWords`. CJK (`zh-Hans`, `zh-Hant`, `ja`) breaks between characters, keeps Latin words and number+unit tokens together, and applies kinsoku. Korean breaks on spaces, then per syllable when one word is too wide. `km`, `hi`, and `ar` still use the Latin breaker.

Non-English `text-transform: uppercase` goes through `LocCase.Upper`. German `ß` becomes `SS`. `zh-Hans`, `zh-Hant`, `ja`, `ko`, `km`, `hi`, and `ar` are not uppercased, so a Latin token such as `kg` stays as written.

`qps` is not in the language list and is never written to the profile. Set PlayerPrefs `os.parity.lang` to `qps` or a registry code before launch. Boot reads it once, and only when the key is set. The `qps` table is built in memory from `en.json`: Latin-1 look-alikes, padded to +40% with `~`, wrapped in `[` `]`. Digits, `{n}`, emoji, `/`, `×`, and `▶` stay as they are.

Overflow audit: menu `Orbital Salvage/Tools/Localization Overflow Audit`, or batch `OrbitalSalvage.Editor.LocOverflowMenu.RunBatch` with `-loc-audit-lang` (default `qps`) and `-loc-audit-out`. It walks `*__zero.json` and `*__iphone.json` without painting. Hard findings are text outside the element, overlap with a sibling icon, and text past y 898.25 on iPhone. Shrink, ellipsis, extra wrap, and a line that is still wider than the available width are listed and are not hard. Elements with two or more runs are counted and still translated one run at a time.
