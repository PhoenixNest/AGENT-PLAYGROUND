# Localization

English is the source language. Missing keys fall back to English. This folder is the string-table handoff. Painting translations is a later task, so an English build does not read these tables when it draws.

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

An endonym from `LanguageRegistry`, and that same endonym with the current-language marker (`✓ ` plus the endonym), is also `doNotTranslate`. Endonyms are never translated. The placeholder legal URLs are not spec strings. They live in `LegalLinks` (App) and `LEGAL_LINKS` in `web/main.js`.

## CSV

`localization/key-index.csv` is the translator handoff, one row per spec key, ordinal order.

`key`, `kind` (`spec` or, later, `str`), `files` (`count: first; second; third` zero-spec names), `element_ids` (up to 5), `element_classes` (up to 3), `fz` (first non-empty), `uppercase` (`uppercase` if any occurrence uses that text-transform, otherwise the first `tt`), `max_width` (largest `run.r[2]` in CSS px, else the element width, else blank), `chars` (Unicode code points), `hasEmoji`, `hasPlaceholderLike`, `group` (element id, or `file#index` when there is no id, up to 8), `run_index`, `notes`.

`notes` contains `fragment` when the run sits in an element with two or more text runs, and `no-terminal-punct` when the key does not end with sentence punctuation.

## Workflow

1. Extract. Menu `Orbital Salvage/Tools/Extract Localization Spec Strings`, or batch `OrbitalSalvage.Editor.LocExtract.RunBatch`. It reads `Assets/Resources/Parity/Spec/*__zero.json` and writes `en.json` plus this CSV. `*__iphone.json` must yield the same key set. Spec JSON is not modified.
2. Translate. Copy `en.json` to `<code>.json` and replace `spec` and `str` values. Keep placeholder indexes identical. Do not add edge whitespace or control characters.
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
