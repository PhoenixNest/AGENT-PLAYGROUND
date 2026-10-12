#!/usr/bin/env python3
"""Validate an Orbital Salvage translation table against English.

Mirrors Assets/Scripts/Loc/LocValidator.cs (missing, extra, placeholder,
edge whitespace, control characters, Latin length) and adds the translation-kit
checks: empty strings, untranslated copies, key bytes, emoji, digits and units,
script, glossary, expansion budget, line-box fit, stale source hashes, and
missing glyphs.

    python3 validate_table.py --lang de [--table path] [--en path]
        [--fonts-dir path] [--max-ratio 2.5] [--json out.json]
        [--fit-exceptions path] [--ignore-fit-keys KEY ...]
        [--show-suppressed] [--register-check]

Exit 1 when any finding is an error. Warnings stay in the report and do not
change the exit code. Exit 2 when the invocation itself is wrong.
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import sys
import unicodedata
from pathlib import Path

TOOL_DIR = Path(__file__).resolve().parent
LOC_DIR = TOOL_DIR.parent
GAME_DIR = LOC_DIR.parent
WORKTREE = GAME_DIR.parent.parent
DEFAULT_EN = GAME_DIR / "unity" / "maze-puzzle" / "Assets" / "Resources" / "Localization" / "en.json"
DEFAULT_FONTS = GAME_DIR / "unity" / "maze-puzzle" / "Assets" / "Fonts"
DEFAULT_INDEX = LOC_DIR / "key-index.csv"
DEFAULT_GLOSSARY = LOC_DIR / "glossary.md"
DEFAULT_CHAINS = TOOL_DIR / "fonts-chains.json"

# Character-count budget relative to English. For Latin this is also the
# expected visual expansion. For CJK, 0.55 characters is about the same
# visual width as English because an ideograph is ~1 em and a Latin letter
# in Fredoka/Nunito is ~0.5 em. ru is not a target language.
EXPANSION = {
    "de": 1.35,
    "fr": 1.25,
    "es": 1.25,
    "it": 1.20,
    "pt-PT": 1.25,
    "vi": 1.15,
    "fil": 1.40,
    "id": 1.20,
    "uz": 1.25,
    "zh-Hans": 0.55,
    "zh-Hant": 0.55,
    "ja": 0.80,
    "ko": 0.75,
    "hi": 1.30,
    "km": 1.40,
    "ar": 1.10,
}

# Average advance in em (multiply by the CSS font size). Planning estimates,
# a little wide for Latin so a pass is likely to fit without shrinking.
# The 16px button inset is NOT subtracted: key-index max_width is usually the
# captured run width (run.r[2]), which already sits inside that inset.
ADVANCE_EM = {
    "space": 0.28,
    "digit": 0.50,
    "latin": 0.50,
    "punct": 0.32,
    "cjk": 1.00,
    "hangul": 1.00,
    "kana": 1.00,
    "arabic": 0.48,
    "devanagari": 0.40,
    "khmer": 0.45,
    "emoji": 1.00,
    "other": 0.50,
}

LATIN_CODES = ("de", "fr", "es", "it", "pt-PT", "vi", "fil", "id", "uz")
TARGET_CODES = (
    "zh-Hans",
    "zh-Hant",
    "ja",
    "ko",
    "de",
    "fr",
    "es",
    "it",
    "pt-PT",
    "vi",
    "fil",
    "id",
    "uz",
    "km",
    "hi",
    "ar",
)

# code, English name, endonym, script, rtl, font group, latin length budget
LANGUAGES = {
    "en": ("English", "English", "Latn", False, "latin", True),
    "zh-Hans": ("Chinese (Simplified)", "简体中文", "Hans", False, "cjk-sc", False),
    "zh-Hant": ("Chinese (Traditional)", "繁體中文", "Hant", False, "cjk-tc", False),
    "ja": ("Japanese", "日本語", "Jpan", False, "cjk-jp", False),
    "ko": ("Korean", "한국어", "Kore", False, "cjk-kr", False),
    "de": ("German", "Deutsch", "Latn", False, "latin", True),
    "fr": ("French", "Français", "Latn", False, "latin", True),
    "es": ("Spanish", "Español", "Latn", False, "latin", True),
    "it": ("Italian", "Italiano", "Latn", False, "latin", True),
    "pt-PT": ("Portuguese", "Português", "Latn", False, "latin", True),
    "vi": ("Vietnamese", "Tiếng Việt", "Latn", False, "latin-vi", True),
    "fil": ("Filipino", "Filipino", "Latn", False, "latin", True),
    "id": ("Indonesian", "Bahasa Indonesia", "Latn", False, "latin", True),
    "uz": ("Uzbek", "O\u02bbzbekcha", "Latn", False, "latin", True),
    "km": ("Khmer", "ភាសាខ្មែរ", "Khmr", False, "khmer", False),
    "hi": ("Hindi", "हिन्दी", "Deva", False, "devanagari", False),
    "ar": ("Arabic", "العربية", "Arab", True, "arabic", False),
}

BRAND_PHRASES = (
    "SPACE TRASH CLEAN",
    "ORBITAL SALVAGE",
    "Orbital Salvage",
    "Space Trash Clean",
    "localStorage",
    "ORBITAL",
    "SALVAGE",
    "Nibbles",
)

ALLOWED_LATIN_TOKENS = BRAND_PHRASES + ("kg",)

WARN_RATIO = 1.6
FAIL_RATIO = 2.5
SHRINK_FLOOR = 0.72


class Finding:
    def __init__(self, severity: str, kind: str, key: str, message: str, reason: str = ""):
        self.severity = severity
        self.kind = kind
        self.key = key
        self.message = message
        self.reason = reason

    def line(self) -> str:
        base = f"{self.severity}\t{self.kind}\t{self.key}\t{self.message}"
        if self.reason:
            return base + "\t" + self.reason
        return base


class PairObj(dict):
    """JSON object that remembers key order and duplicate keys."""

    def __init__(self, pairs):
        super().__init__()
        self.order = []
        self.duplicates = []
        for key, value in pairs:
            if key in self:
                self.duplicates.append(key)
            else:
                self.order.append(key)
            self[key] = value


def utf16_len(text: str) -> int:
    return len(text.encode("utf-16-le")) // 2


def ordinal_bytes(text: str) -> bytes:
    """UTF-16 code units in ordinal order, matching C# StringComparer.Ordinal."""
    return text.encode("utf-16-be")


def format_ratio(ratio: float) -> str:
    return f"{ratio:.3f}".rstrip("0").rstrip(".")


def endonyms() -> set:
    return {row[1] for row in LANGUAGES.values()}


def load_json(path: Path):
    raw = path.read_text(encoding="utf-8")
    data = json.loads(raw, object_pairs_hook=PairObj)
    return raw, data


def placeholder_indexes(text: str) -> set:
    found = set()
    if not text:
        return found
    i = 0
    n = len(text)
    while i < n:
        if text[i] != "{":
            i += 1
            continue
        if i + 1 < n and text[i + 1] == "{":
            i += 2
            continue
        j = i + 1
        if j >= n or not text[j].isdigit():
            i += 1
            continue
        start = j
        while j < n and text[j].isdigit():
            j += 1
        if j < n and text[j] == "}":
            found.add(text[start:j])
            i = j + 1
        else:
            i += 1
    return found


def is_emoji_base(cp: int) -> bool:
    if 0x2600 <= cp <= 0x27BF:
        return True
    if 0x1F000 <= cp <= 0x1FAFF:
        return True
    return False


def emoji_spans(text: str) -> list:
    """Return (start, end) of emoji graphemes, LocText's BMP range plus emoji blocks."""
    spans = []
    i = 0
    n = len(text)
    while i < n:
        if not is_emoji_base(ord(text[i])):
            i += 1
            continue
        j = i + 1
        while j < n:
            cp = ord(text[j])
            if cp in (0xFE0F, 0xFE0E):
                j += 1
                continue
            if cp == 0x200D and j + 1 < n and is_emoji_base(ord(text[j + 1])):
                j += 2
                continue
            break
        spans.append((i, j))
        i = j
    return spans


def emoji_sequence(text: str) -> list:
    """Emoji graphemes in order, with a 'text' marker when words sit between them.

    The marker keeps a leading emoji from matching a trailing one. The words
    themselves are not compared here.
    """
    parts = []
    last = 0
    for start, end in emoji_spans(text):
        if text[last:start].strip():
            parts.append("text")
        parts.append(text[start:end])
        last = end
    if text[last:].strip():
        parts.append("text")
    return parts


def has_edge_whitespace(text: str) -> bool:
    if not text:
        return False
    return text[0].isspace() or text[-1].isspace()


def has_control(text: str) -> bool:
    for ch in text:
        cp = ord(ch)
        if cp < 0x20 or cp == 0x7F or unicodedata.category(ch) == "Cc":
            return True
    return False


def has_letter(text: str) -> bool:
    return any(ch.isalpha() for ch in text)


def _boundary_token_spans(text: str, token: str) -> list:
    """Spans of token that are not glued to a letter.

    `kg` counts after a digit (`105kg`), after a placeholder (`{0} kg`,
    `{0}kg`, `{0}` + U+00A0 + `kg`), or on its own at a boundary. It does
    not count inside a word (`Background`). `/` and `+` follow the same
    rule so a slash or plus inside a word is not a unit.
    """
    spans = []
    size = len(token)
    start = 0
    while True:
        index = text.find(token, start)
        if index < 0:
            return spans
        before = text[index - 1] if index else ""
        after_at = index + size
        after = text[after_at] if after_at < len(text) else ""
        if not (before.isalpha() or after.isalpha()):
            spans.append((index, after_at))
        start = after_at


def count_unit_token(text: str, token: str) -> int:
    if token in ("kg", "/", "+"):
        return len(_boundary_token_spans(text, token))
    return text.count(token)


def strip_boundary_token(text: str, token: str) -> str:
    spans = _boundary_token_spans(text, token)
    if not spans:
        return text
    parts = []
    cursor = 0
    for start, end in spans:
        parts.append(text[cursor:start])
        cursor = end
    parts.append(text[cursor:])
    return "".join(parts)


def brand_only(text: str) -> bool:
    rest = text
    for phrase in BRAND_PHRASES:
        rest = rest.replace(phrase, " ")
    rest = re.sub(r"v?\d+(?:\.\d+)*", " ", rest)
    rest = strip_boundary_token(rest, "kg")
    for token in ("•", "×", "▶", "←", "✓", "/", "+", "%"):
        rest = rest.replace(token, " ")
    return not has_letter(rest)


def is_unit_like(text: str) -> bool:
    """True when the only letters are the unit token kg.

    `{0}`, a normal space, and U+00A0 may sit next to kg. Braces do not
    block the exception: `{0} kg`, `{0}kg`, and `{0}\\u00a0kg` are unit strings.
    """
    rest = re.sub(r"\{[0-9]+\}", "", text)
    rest = strip_boundary_token(rest, "kg")
    for token in ("×", "▶", "←", "✓", "•"):
        rest = rest.replace(token, "")
    for ch in rest:
        if ch.isdigit() or ch.isspace() or ch in ".,:;()[]/+-%{}":
            continue
        return False
    return True


def _letters_only(text: str) -> str:
    return "".join(ch for ch in text if ch.isalpha())


def loanword_identical(value: str, lang: str, glossary: list) -> bool:
    """Allow a copy whose letters are exactly a glossary loanword.

    `Donut`, `Bronze`, and `Pause` are the right German words. `🍩 DONUT 🍩`
    is the same loanword with the emoji left in place. A longer sentence
    that merely contains the word is still untranslated.
    """
    letters = _letters_only(value).casefold()
    if len(letters) < 2 or not glossary or not lang:
        return False
    for term in glossary:
        recommended = (term.get(lang) or "").strip()
        if not recommended or _letters_only(recommended).casefold() != letters:
            continue
        for alternate in term.get("alternates") or []:
            if _letters_only(alternate).casefold() == letters:
                return True
    return False


def allowed_identical(value: str, do_not: set, lang: str | None = None, glossary: list | None = None) -> bool:
    if not has_letter(value):
        return True
    if value in do_not:
        return True
    marker = "\u2713 "
    if value.startswith(marker) and value[len(marker) :] in endonyms():
        return True
    if value in endonyms():
        return True
    if is_unit_like(value):
        return True
    if loanword_identical(value, lang or "", glossary or []):
        return True
    if brand_only(value):
        return True
    return False


def bracket_counts(text: str) -> tuple:
    return (text.count("("), text.count(")"), text.count("["), text.count("]"))


def digit_run(text: str) -> str:
    return "".join(ch for ch in text if ch.isdigit())


def advance_of(cp: int) -> float:
    if cp == 0x20 or unicodedata.category(chr(cp)).startswith("Z"):
        return ADVANCE_EM["space"]
    if 0x30 <= cp <= 0x39:
        return ADVANCE_EM["digit"]
    if is_emoji_base(cp):
        return ADVANCE_EM["emoji"]
    if (
        0x4E00 <= cp <= 0x9FFF
        or 0x3400 <= cp <= 0x4DBF
        or 0xF900 <= cp <= 0xFAFF
    ):
        return ADVANCE_EM["cjk"]
    if 0x3040 <= cp <= 0x30FF or 0xFF66 <= cp <= 0xFF9D:
        return ADVANCE_EM["kana"]
    if 0xAC00 <= cp <= 0xD7AF or 0x1100 <= cp <= 0x11FF:
        return ADVANCE_EM["hangul"]
    if 0x0600 <= cp <= 0x06FF or 0x0750 <= cp <= 0x077F or 0x08A0 <= cp <= 0x08FF:
        return ADVANCE_EM["arabic"]
    if 0x0900 <= cp <= 0x097F:
        return ADVANCE_EM["devanagari"]
    if 0x1780 <= cp <= 0x17FF:
        return ADVANCE_EM["khmer"]
    if chr(cp).isalpha():
        return ADVANCE_EM["latin"]
    if unicodedata.category(chr(cp)).startswith("P"):
        return ADVANCE_EM["punct"]
    return ADVANCE_EM["other"]


def estimate_px(text: str, font_px: float) -> float:
    return font_px * sum(advance_of(ord(ch)) for ch in text)


def script_flags(text: str) -> dict:
    flags = {
        "han": False,
        "kana": False,
        "hangul": False,
        "deva": False,
        "khmer": False,
        "arab": False,
    }
    for ch in text:
        cp = ord(ch)
        if (
            0x4E00 <= cp <= 0x9FFF
            or 0x3400 <= cp <= 0x4DBF
            or 0xF900 <= cp <= 0xFAFF
            or 0x20000 <= cp <= 0x2A6DF
        ):
            flags["han"] = True
        elif 0x3040 <= cp <= 0x30FF or 0xFF66 <= cp <= 0xFF9D:
            flags["kana"] = True
        elif 0xAC00 <= cp <= 0xD7AF or 0x1100 <= cp <= 0x11FF:
            flags["hangul"] = True
        elif 0x0900 <= cp <= 0x097F:
            flags["deva"] = True
        elif 0x1780 <= cp <= 0x17FF:
            flags["khmer"] = True
        elif _arabic_letter(cp):
            flags["arab"] = True
    return flags


def _arabic_letter(cp: int) -> bool:
    if 0x0621 <= cp <= 0x063A or 0x0641 <= cp <= 0x064A:
        return True
    if 0x0671 <= cp <= 0x06D3:
        return True
    return False


def strip_allowed_latin(text: str) -> str:
    rest = text
    for token in sorted(ALLOWED_LATIN_TOKENS, key=len, reverse=True):
        rest = rest.replace(token, " ")
    rest = re.sub(r"\{[0-9]+\}", " ", rest)
    return rest


def has_disallowed_latin(text: str) -> bool:
    return re.search(r"[A-Za-z]", strip_allowed_latin(text)) is not None


def bad_digits(text: str) -> str:
    for ch in text:
        cp = ord(ch)
        if 0x0660 <= cp <= 0x0669:
            return "Arabic-Indic digit"
        if 0x06F0 <= cp <= 0x06F9:
            return "Eastern Arabic-Indic digit"
        if 0x0966 <= cp <= 0x096F:
            return "Devanagari digit"
        if 0xFF10 <= cp <= 0xFF19:
            return "fullwidth digit"
    return ""


def source_hash(section: str, key: str, value: str) -> str:
    payload = section + "\n" + key + "\n" + value
    return hashlib.sha256(payload.encode("utf-8")).hexdigest()


def parse_glossary(path: Path) -> list:
    """Read the TSV block in glossary.md. Returns a list of term dicts."""
    if path is None or not path.is_file():
        return []
    terms = []
    header = None
    for line in path.read_text(encoding="utf-8").splitlines():
        if header is None:
            if line.startswith("term\t") or line.startswith("id\t"):
                header = line.split("\t")
            continue
        if not line.strip() or line.startswith("```"):
            if terms:
                break
            continue
        cells = line.split("\t")
        if len(cells) < len(header):
            cells = cells + [""] * (len(header) - len(cells))
        row = dict(zip(header, cells))
        row["dnt"] = row.get("dnt", "").strip().lower() in ("yes", "true", "1")
        row["check"] = row.get("check", "").strip().lower() in ("yes", "true", "1")
        row["alternates"] = [part for part in row.get("en", "").split("/") if part]
        terms.append(row)
    return terms


def parse_aliases(path: Path) -> list:
    """Read the alias block in glossary.md. Each row maps a surface onto a term id."""
    if path is None or not path.is_file():
        return []
    aliases = []
    header = None
    for line in path.read_text(encoding="utf-8").splitlines():
        if header is None:
            if line.startswith("alias\t"):
                header = line.split("\t")
            continue
        if not line.strip() or line.startswith("```"):
            if aliases:
                break
            continue
        cells = line.split("\t")
        if len(cells) < 2 or not cells[0].strip() or not cells[1].strip():
            continue
        aliases.append({"alias": cells[0].strip(), "term": cells[1].strip()})
    return aliases


def protected_spans(text: str, do_not) -> list:
    """Brand phrases and do-not-translate strings. Glossary hits inside them do not count."""
    phrases = [phrase for phrase in list(BRAND_PHRASES) + list(do_not or []) if phrase]
    spans = []
    for phrase in phrases:
        start = 0
        while True:
            index = text.find(phrase, start)
            if index < 0:
                break
            spans.append((index, index + len(phrase)))
            start = index + len(phrase)
    return spans


def _inside_longer(span: tuple, spans: list) -> bool:
    start, end = span
    width = end - start
    for left, right in spans:
        if left <= start and end <= right and (right - left) > width:
            return True
    return False


def english_has_term(text: str, term: str, case_sensitive: bool, protected=None, exact: bool = False) -> bool:
    flags = 0 if case_sensitive else re.IGNORECASE
    if exact or " " in term or term.endswith("s"):
        pattern = r"(?<![A-Za-z0-9])" + re.escape(term) + r"(?![A-Za-z0-9])"
    else:
        pattern = r"(?<![A-Za-z0-9])" + re.escape(term) + r"s?(?![A-Za-z0-9])"
    for match in re.finditer(pattern, text, flags):
        if protected and _inside_longer(match.span(), protected):
            continue
        return True
    return False


def translation_has_recommendation(text: str, recommended: str, dnt: bool) -> bool:
    if not recommended:
        return True
    if dnt:
        return recommended in text
    return recommended.casefold() in text.casefold()


def load_key_index(path: Path) -> dict:
    if path is None or not path.is_file():
        return {}
    rows = {}
    with path.open(encoding="utf-8", newline="") as handle:
        for row in csv.DictReader(handle):
            rows[(row.get("kind") or "spec", row.get("key") or "")] = row
    return rows


def parse_px(value: str):
    if value is None:
        return None
    text = value.strip().lower().replace("px", "")
    if not text:
        return None
    try:
        return float(text)
    except ValueError:
        return None


def load_chains(path: Path) -> dict:
    if path is None or not path.is_file():
        return {}
    data = json.loads(path.read_text(encoding="utf-8"))
    return data.get("languages") or {}


def cmap_for(path: Path) -> set:
    from fontTools.ttLib import TTFont

    font = TTFont(str(path), lazy=True)
    try:
        cmap = font.getBestCmap() or {}
        covered = set()
        for cp, name in cmap.items():
            if name and name != ".notdef":
                covered.add(cp)
        return covered
    finally:
        font.close()


def load_coverage(chain: list, fonts_dir: Path, findings: list) -> set:
    covered = set()
    loaded = False
    for name in chain:
        file_path = fonts_dir / name
        if not file_path.is_file():
            findings.append(
                Finding("warning", "font-missing", "table", f"font file not found: {name}")
            )
            continue
        try:
            covered |= cmap_for(file_path)
            loaded = True
        except Exception as exc:  # fontTools or a bad file
            findings.append(
                Finding("warning", "font-missing", "table", f"could not read {name}: {exc}")
            )
    if not loaded:
        return set()
    return covered


def glyph_missing(text: str, covered: set) -> list:
    skip = set()
    for start, end in emoji_spans(text):
        skip.update(range(start, end))
    missing = []
    seen = set()
    for index, ch in enumerate(text):
        if index in skip:
            continue
        cp = ord(ch)
        if ch.isspace() or unicodedata.category(ch) == "Cc":
            continue
        if cp in (0x200D, 0x200C, 0xFE0F, 0xFE0E):
            continue
        if cp in covered or cp in seen:
            continue
        seen.add(cp)
        missing.append(ch)
    return missing


def add(findings, severity, kind, key, message, reason=""):
    findings.append(Finding(severity, kind, key, message, reason))


def check_pair(
    section,
    key,
    source,
    target,
    lang,
    latin_budget,
    fail_ratio,
    do_not,
    glossary,
    index_row,
    covered,
    hashes,
    findings,
    table_flags,
):
    ident = section + ":" + key
    if target is None:
        add(findings, "error", "missing", ident, "missing translation")
        return
    if not isinstance(target, str):
        add(findings, "error", "type", ident, "translation value is not a string")
        return
    if source is None:
        add(findings, "error", "extra", ident, "key is not in the English table")
        return
    if not isinstance(source, str):
        source = str(source)

    nfc_key = unicodedata.normalize("NFC", key)
    if key != nfc_key:
        add(findings, "error", "key-bytes", ident, "key is not NFC; bytes differ from the English key")

    if target == "":
        add(findings, "error", "empty", ident, "empty translation")
    elif target == source and not allowed_identical(source, do_not, lang, glossary):
        reason = lookup_fit_exception(ident, table_flags.get("identical_ok") or {})
        if reason is None:
            add(findings, "error", "untranslated", ident, "translation is a copy of the English string")
        else:
            add(
                findings,
                "accepted",
                "identical",
                ident,
                "accepted identical copy of English",
                reason or "accepted identical copy",
            )

    if placeholder_indexes(source) != placeholder_indexes(target):
        add(findings, "error", "placeholder", ident, "placeholder indexes differ")
    if has_edge_whitespace(source) or has_edge_whitespace(target):
        add(findings, "warning", "whitespace", ident, "leading or trailing whitespace")
    if has_control(target):
        add(findings, "error", "control", ident, "control character in translation")

    if latin_budget and source and target is not None:
        ratio = utf16_len(target) / utf16_len(source) if utf16_len(source) else 0
        if utf16_len(source) > 0 and ratio >= fail_ratio:
            add(findings, "error", "length", ident, f"length is {format_ratio(ratio)}x English")
        elif utf16_len(source) > 0 and ratio >= WARN_RATIO:
            add(findings, "warning", "length", ident, f"length is {format_ratio(ratio)}x English")

    factor = EXPANSION.get(lang)
    if factor and source and utf16_len(source) > 0 and isinstance(target, str):
        ratio = utf16_len(target) / utf16_len(source)
        budget_warn = factor * WARN_RATIO
        budget_fail = factor * FAIL_RATIO
        if ratio >= budget_fail:
            add(
                findings,
                "error",
                "expansion",
                ident,
                f"length is {format_ratio(ratio)}x English; budget is {format_ratio(factor)}x, error at {format_ratio(budget_fail)}x",
            )
        elif ratio >= budget_warn:
            add(
                findings,
                "warning",
                "expansion",
                ident,
                f"length is {format_ratio(ratio)}x English; budget is {format_ratio(factor)}x, warn at {format_ratio(budget_warn)}x",
            )

    if isinstance(target, str) and emoji_sequence(source) != emoji_sequence(target):
        add(findings, "error", "emoji", ident, "emoji sequence differs from English")
    if isinstance(target, str) and digit_run(source) != digit_run(target):
        add(findings, "error", "number", ident, "digit sequence differs from English")
    if isinstance(target, str):
        for token in ("kg", "×", "/", "▶", "←", "%", "+"):
            if count_unit_token(source, token) != count_unit_token(target, token):
                add(findings, "error", "unit", ident, f"count of {token!r} differs from English")
                break
        if bracket_counts(source) != bracket_counts(target):
            add(
                findings,
                "warning",
                "balance",
                ident,
                "parenthesis or bracket counts differ from English; match the source, including a fragment that ends mid-token",
            )
        digit_kind = bad_digits(target)
        if digit_kind:
            add(findings, "warning", "digits", ident, f"use Western digits 0-9, found {digit_kind}")

    if isinstance(target, str) and target != "" and not allowed_identical(source, do_not):
        check_script(lang, target, ident, findings, table_flags)
        if lang == "uz" and re.search(r"[oOgG]['\u2019\u02bc]", target):
            add(
                findings,
                "warning",
                "orthography",
                ident,
                "use U+02BB in o\u02bb and g\u02bb, not an apostrophe",
            )
        if lang == "fr":
            if "\u202f" in target:
                add(
                    findings,
                    "warning",
                    "punctuation",
                    ident,
                    "U+202F is not in the shipped Latin fonts; use U+00A0 before ! ? : ;",
                )
            elif re.search(r" [!?:;]", target):
                add(
                    findings,
                    "warning",
                    "punctuation",
                    ident,
                    "use U+00A0 before ! ? : ; in French, not a normal space",
                )

    if index_row and isinstance(target, str):
        check_fit(index_row, source, target, ident, findings)
        if (index_row.get("uppercase") or "") == "uppercase":
            letters = [ch for ch in target if "A" <= ch <= "Z" or "a" <= ch <= "z"]
            if len(letters) >= 4 and all("A" <= ch <= "Z" for ch in letters):
                add(
                    findings,
                    "warning",
                    "uppercase",
                    ident,
                    "tt=uppercase is applied at runtime; write normal casing",
                )

    if isinstance(target, str) and target not in ("", source):
        check_glossary(
            glossary,
            lang,
            source,
            target,
            ident,
            findings,
            aliases=table_flags.get("aliases") or [],
            do_not=do_not,
        )

    if hashes is not None and isinstance(target, str):
        section_hashes = {}
        if isinstance(hashes, dict):
            section_hashes = hashes.get(section) or {}
        expected = source_hash(section, key, source)
        if not isinstance(section_hashes, dict) or key not in section_hashes:
            add(findings, "warning", "stale", ident, "no source hash for this key")
        elif section_hashes.get(key) != expected:
            add(findings, "warning", "stale", ident, "source hash does not match English")

    if covered is not None and isinstance(target, str) and target != "":
        missing = glyph_missing(target, covered)
        if missing:
            shown = "".join(missing[:24])
            extra = ""
            if len(missing) > 24:
                extra = f" (+{len(missing) - 24} more)"
            add(findings, "error", "glyph", ident, f"missing glyphs: {shown}{extra}")


def check_script(lang, target, ident, findings, table_flags):
    flags = script_flags(target)
    if flags["kana"]:
        table_flags["kana"] = True
    foreign = []
    if lang in LATIN_CODES:
        if flags["han"] or flags["kana"]:
            foreign.append("CJK")
        if flags["hangul"]:
            foreign.append("Hangul")
        if flags["deva"]:
            foreign.append("Devanagari")
        if flags["khmer"]:
            foreign.append("Khmer")
        if flags["arab"]:
            foreign.append("Arabic")
        if foreign:
            add(findings, "error", "script", ident, "Latin translation contains " + ", ".join(foreign))
        return
    if lang in ("zh-Hans", "zh-Hant"):
        if flags["kana"]:
            add(findings, "error", "script", ident, "Chinese translation contains kana")
        if flags["hangul"] or flags["deva"] or flags["khmer"] or flags["arab"]:
            add(findings, "error", "script", ident, "Chinese translation contains another script")
        if not flags["han"]:
            add(findings, "warning", "script", ident, "Chinese translation has no Han character")
        return
    if lang == "ja":
        if flags["hangul"] or flags["deva"] or flags["khmer"] or flags["arab"]:
            add(findings, "error", "script", ident, "Japanese translation contains another script")
        if not (flags["kana"] or flags["han"]):
            add(findings, "error", "script", ident, "Japanese translation has no kana or kanji")
        return
    if lang == "ko":
        if flags["kana"] or flags["deva"] or flags["khmer"] or flags["arab"]:
            add(findings, "error", "script", ident, "Korean translation contains another script")
        if not flags["hangul"]:
            add(findings, "error", "script", ident, "Korean translation has no Hangul")
        return
    if lang == "hi":
        if flags["han"] or flags["kana"] or flags["hangul"] or flags["khmer"] or flags["arab"]:
            add(findings, "error", "script", ident, "Hindi translation contains another script")
        if not flags["deva"]:
            add(findings, "error", "script", ident, "Hindi translation has no Devanagari")
        return
    if lang == "km":
        if flags["han"] or flags["kana"] or flags["hangul"] or flags["deva"] or flags["arab"]:
            add(findings, "error", "script", ident, "Khmer translation contains another script")
        if not flags["khmer"]:
            add(findings, "error", "script", ident, "Khmer translation has no Khmer")
        return
    if lang == "ar":
        if flags["han"] or flags["kana"] or flags["hangul"] or flags["deva"] or flags["khmer"]:
            add(findings, "error", "script", ident, "Arabic translation contains another script")
        if not flags["arab"]:
            add(findings, "error", "script", ident, "Arabic translation has no Arabic letter")
        if has_disallowed_latin(target):
            add(
                findings,
                "error",
                "script",
                ident,
                "Arabic translation has a Latin letter outside allowed tokens",
            )


def check_fit(index_row, source, target, ident, findings):
    font_px = parse_px(index_row.get("fz") or "")
    box = parse_px(index_row.get("max_width") or "")
    if font_px is None or box is None or font_px <= 0 or box <= 0:
        return
    english_px = estimate_px(source, font_px)
    target_px = estimate_px(target, font_px)
    budget = max(box, english_px)
    if target_px > budget / SHRINK_FLOOR:
        add(
            findings,
            "error",
            "fit",
            ident,
            f"estimated width {target_px:.0f}px exceeds the 72% shrink floor of a {budget:.0f}px line",
        )
    elif target_px > budget:
        add(
            findings,
            "warning",
            "fit",
            ident,
            f"estimated width {target_px:.0f}px exceeds the {budget:.0f}px line box",
        )


def check_glossary(glossary, lang, source, target, ident, findings, aliases=None, do_not=None):
    protected = protected_spans(source, do_not)
    by_term = {}
    for row in aliases or []:
        by_term.setdefault(row.get("term") or "", []).append(row.get("alias") or "")
    for term in glossary:
        if not term.get("check"):
            continue
        dnt = term.get("dnt", False)
        matched = False
        for alternate in term.get("alternates") or []:
            if english_has_term(source, alternate, case_sensitive=dnt, protected=protected):
                matched = True
                break
        if not matched:
            for alias in by_term.get(term.get("term") or "", []):
                if alias and english_has_term(source, alias, case_sensitive=dnt, protected=protected, exact=True):
                    matched = True
                    break
        if not matched:
            continue
        recommended = (term.get(lang) or "").strip()
        if not recommended:
            continue
        if translation_has_recommendation(target, recommended, dnt):
            continue
        add(
            findings,
            "warning",
            "glossary",
            ident,
            f"English uses {term.get('en', '')!r}; expected {recommended!r}",
        )


def section_map(table, name):
    value = table.get(name) if isinstance(table, dict) else None
    if value is None:
        return PairObj(())
    if not isinstance(value, dict):
        return None
    return value


def validate(english, translation, lang, options) -> list:
    findings = []
    if lang not in LANGUAGES or lang == "en":
        add(findings, "error", "shape", "table", f"unsupported language {lang}")
        return findings
    latin_budget = LANGUAGES[lang][5]
    fail_ratio = options.get("fail_ratio", FAIL_RATIO)

    if not isinstance(english, dict) or not isinstance(translation, dict):
        add(findings, "error", "shape", "table", "table root must be an object")
        return findings

    for label, table in (("en", english), ("translation", translation)):
        if not isinstance(table, PairObj):
            continue
        for dup in table.duplicates:
            add(findings, "error", "duplicate", "table", f"duplicate key in {label}: {dup}")

    spec_en = section_map(english, "spec")
    spec_tr = section_map(translation, "spec")
    str_en = section_map(english, "str")
    str_tr = section_map(translation, "str")
    for name, value in (("spec", spec_en), ("str", str_en), ("spec", spec_tr), ("str", str_tr)):
        if value is None:
            add(findings, "error", "type", name, f"{name} must be an object")
    if spec_en is None or spec_tr is None or str_en is None or str_tr is None:
        return findings

    allowed_top = {"meta", "spec", "str", "doNotTranslate"}
    if isinstance(translation, PairObj):
        for key in translation.order:
            if key not in allowed_top:
                add(findings, "warning", "shape", "table", f"unexpected top-level key {key}")
        for key in translation.duplicates:
            add(findings, "error", "duplicate", "table", f"duplicate key: {key}")

    meta = translation.get("meta") if isinstance(translation.get("meta"), dict) else {}
    if meta.get("code") not in (None, lang):
        add(findings, "error", "shape", "meta.code", f"meta.code is {meta.get('code')!r}, expected {lang!r}")
    endonym = LANGUAGES[lang][1]
    if meta.get("name") not in (None, endonym):
        add(findings, "warning", "shape", "meta.name", f"meta.name should be the endonym {endonym!r}")

    do_not = set()
    for table in (english, translation):
        listed = table.get("doNotTranslate") if isinstance(table, dict) else None
        if isinstance(listed, list):
            for item in listed:
                if isinstance(item, str):
                    do_not.add(item)

    raw = options.get("raw") or ""
    if raw.startswith("\ufeff"):
        add(findings, "warning", "encoding", "table", "UTF-8 BOM; save as UTF-8 without a BOM")
    if "\r\n" in raw or "\r" in raw:
        add(findings, "warning", "encoding", "table", "CRLF; tables are LF")

    for section, mapping in (("spec", spec_tr), ("str", str_tr)):
        if isinstance(mapping, PairObj):
            ordered = list(mapping.order)
            expect = sorted(set(ordered), key=ordinal_bytes)
            if ordered != expect:
                add(findings, "warning", "order", section, "keys are not in ordinal order")
            for dup in mapping.duplicates:
                add(findings, "error", "duplicate", section + ":" + dup, "duplicate key")

    glossary = options.get("glossary") or []
    index = options.get("index") or {}
    hashes = None
    if isinstance(meta, dict) and "sourceHashes" in meta:
        hashes = meta.get("sourceHashes")

    covered = options.get("covered")
    table_flags = {
        "kana": False,
        "aliases": options.get("aliases") or [],
        "identical_ok": options.get("identical_ok") or {},
    }
    keys = set(spec_en) | set(spec_tr)
    for key in sorted(keys, key=ordinal_bytes):
        check_pair(
            "spec",
            key,
            spec_en.get(key),
            spec_tr.get(key) if key in spec_tr else None,
            lang,
            latin_budget,
            fail_ratio,
            do_not,
            glossary,
            index.get(("spec", key)),
            covered,
            hashes,
            findings,
            table_flags,
        )
    keys = set(str_en) | set(str_tr)
    for key in sorted(keys, key=ordinal_bytes):
        check_pair(
            "str",
            key,
            str_en.get(key),
            str_tr.get(key) if key in str_tr else None,
            lang,
            latin_budget,
            fail_ratio,
            do_not,
            glossary,
            index.get(("str", key)),
            covered,
            hashes,
            findings,
            table_flags,
        )

    if lang == "ja" and not table_flags["kana"]:
        translated = [value for value in list(spec_tr.values()) + list(str_tr.values()) if isinstance(value, str) and value]
        if translated:
            add(findings, "warning", "script", "table", "Japanese table has no kana")

    # Pair near-miss keys (NFC or trimmed) so a lookalike is not only missing+extra.
    for section, source_map, target_map in (("spec", spec_en, spec_tr), ("str", str_en, str_tr)):
        source_by_sig = {}
        for key in source_map:
            source_by_sig.setdefault(_key_sig(key), key)
        for key in target_map:
            if key in source_map:
                continue
            canonical = source_by_sig.get(_key_sig(key))
            if canonical and canonical != key:
                add(
                    findings,
                    "error",
                    "key-bytes",
                    section + ":" + key,
                    f"key bytes differ from English key {canonical!r}",
                )

    if options.get("register_check"):
        add(
            findings,
            "warning",
            "register",
            "table",
            "imperative and telegraphic style is not checked; those strings are skipped",
        )

    findings.sort(key=lambda item: (ordinal_bytes(item.key), item.kind))
    suppress_known_fit(findings, options.get("fit_exceptions") or {})
    return findings


def lookup_fit_exception(ident: str, exceptions: dict):
    if not exceptions:
        return None
    if ident in exceptions:
        return exceptions[ident]
    bare = ident.split(":", 1)[-1]
    if bare in exceptions:
        return exceptions[bare]
    return None


def suppress_known_fit(findings: list, exceptions: dict) -> None:
    """Accepted fit warnings stay in the list with severity suppressed.

    Fit errors (past the shrink floor) are never suppressed. Imperative
    style is not involved here.
    """
    if not exceptions:
        return
    for item in findings:
        if item.severity != "warning" or item.kind != "fit":
            continue
        reason = lookup_fit_exception(item.key, exceptions)
        if reason is None:
            continue
        item.severity = "suppressed"
        item.reason = reason or "accepted fit exception"


def load_fit_exceptions(path: Path) -> dict:
    """Map a key (`spec:LVL 1` or `LVL 1`) to a reason.

    The file is a list, or an object with `keys` and an optional shared
    `reason`. Each key may be a string or `{"key", "reason"}`.
    """
    return load_reason_map(path, "fit exceptions")


def load_identical_ok(path: Path) -> dict:
    """Map a key that may stay English (`Hangar` or `spec:Hangar`) to a reason."""
    return load_reason_map(path, "identical-ok")


def load_reason_map(path: Path, label: str) -> dict:
    """Read a list of `{key, reason}`, or an object with `keys` and a shared reason."""
    data = json.loads(path.read_text(encoding="utf-8"))
    shared = ""
    if isinstance(data, dict):
        shared = (data.get("reason") or "").strip()
        items = data.get("keys")
        if items is None:
            raise ValueError(f"{label} object needs a keys list")
    elif isinstance(data, list):
        items = data
    else:
        raise ValueError(f"{label} must be a list or an object")
    if not isinstance(items, list):
        raise ValueError(f"{label} keys must be a list")
    found = {}
    for item in items:
        if isinstance(item, str):
            key = item.strip()
            reason = shared
        elif isinstance(item, dict):
            key = (item.get("key") or "").strip()
            reason = (item.get("reason") or shared).strip()
        else:
            raise ValueError(f"{label} entry must be a string or an object")
        if not key:
            raise ValueError(f"{label} entry is missing a key")
        found[key] = reason
    return found


def _key_sig(key: str) -> str:
    return unicodedata.normalize("NFC", key).strip()


def prepare_fonts(lang, options, findings) -> set | None:
    """Return a cmap set, or None when the glyph check does not run."""
    if options.get("skip_glyphs"):
        return None
    chains = options.get("chains") or {}
    info = chains.get(lang) or {}
    chain = list(info.get("chain") or [])
    pending = list(info.get("pending") or [])
    fonts_dir = options.get("fonts_dir")
    if not chain:
        if pending:
            add(
                findings,
                "warning",
                "font-pending",
                "table",
                "font chain pending: " + ", ".join(pending),
            )
        return None
    if fonts_dir is None or not Path(fonts_dir).is_dir():
        add(findings, "warning", "glyph-skip", "table", "fonts directory is missing; glyph check skipped")
        return None
    try:
        import fontTools  # noqa: F401
    except ImportError:
        add(findings, "warning", "glyph-skip", "table", "fontTools is not installed; glyph check skipped")
        return None
    covered = load_coverage(chain, Path(fonts_dir), findings)
    if pending:
        add(
            findings,
            "warning",
            "font-pending",
            "table",
            "pending faces not checked: " + ", ".join(pending),
        )
    if not covered:
        add(findings, "warning", "glyph-skip", "table", "no font cmap loaded; glyph check skipped")
        return None
    return covered


def dump_table(table: dict) -> str:
    return json.dumps(table, ensure_ascii=False, indent=2) + "\n"


def sorted_map(pairs: dict) -> dict:
    return {key: pairs[key] for key in sorted(pairs, key=ordinal_bytes)}


def build_hashes(english: dict) -> dict:
    hashes = {"spec": {}, "str": {}}
    for section in ("spec", "str"):
        mapping = english.get(section) or {}
        for key in sorted(mapping, key=ordinal_bytes):
            value = mapping[key]
            if isinstance(value, str):
                hashes[section][key] = source_hash(section, key, value)
    return hashes


def skeleton(lang: str, english: dict, hashes: bool = True) -> dict:
    info = LANGUAGES[lang]
    spec = english.get("spec") or {}
    strings = english.get("str") or {}
    meta = {
        "code": lang,
        "name": info[1],
        "version": 1,
        "source": "en",
    }
    if hashes:
        meta["sourceHashes"] = build_hashes(english)
    table = {
        "meta": meta,
        "spec": sorted_map({key: spec[key] for key in spec}),
        "str": sorted_map({key: strings[key] for key in strings}),
    }
    listed = english.get("doNotTranslate")
    if isinstance(listed, list):
        table["doNotTranslate"] = sorted(listed, key=ordinal_bytes)
    return table


def _quiet(item) -> bool:
    return item.severity in ("suppressed", "accepted")


def summarise(findings: list) -> str:
    visible = [item for item in findings if not _quiet(item)]
    suppressed = [item for item in findings if item.severity == "suppressed"]
    accepted = [item for item in findings if item.severity == "accepted"]
    errors = sum(1 for item in visible if item.severity == "error")
    warnings = sum(1 for item in visible if item.severity == "warning")
    counts = {}
    for item in visible:
        counts[item.kind] = counts.get(item.kind, 0) + 1
    kinds = ", ".join(f"{kind}={counts[kind]}" for kind in sorted(counts))
    lines = [f"errors: {errors}", f"warnings: {warnings}", f"by-kind: {kinds or '(none)'}"]
    if suppressed:
        lines.append(f"suppressed: {len(suppressed)}")
        grouped = {}
        for item in suppressed:
            grouped.setdefault(item.reason or "(no reason)", []).append(item.key)
        for reason in sorted(grouped):
            keys = sorted(grouped[reason], key=ordinal_bytes)
            lines.append("suppressed-list: " + ", ".join(keys) + " — " + reason)
    if accepted:
        parts = []
        for item in sorted(accepted, key=lambda row: ordinal_bytes(row.key)):
            parts.append(f"{item.key} ({item.reason})" if item.reason else item.key)
        lines.append("accepted identical: " + "; ".join(parts))
    return "\n".join(lines)


def findings_json(lang: str, findings: list) -> dict:
    visible = [item for item in findings if not _quiet(item)]
    suppressed = [item for item in findings if item.severity == "suppressed"]
    accepted = [item for item in findings if item.severity == "accepted"]
    return {
        "lang": lang,
        "errors": sum(1 for item in visible if item.severity == "error"),
        "warnings": sum(1 for item in visible if item.severity == "warning"),
        "suppressed": len(suppressed),
        "acceptedIdentical": len(accepted),
        "findings": [
            {
                "severity": item.severity,
                "kind": item.kind,
                "key": item.key,
                "message": item.message,
            }
            for item in visible
        ],
        "suppressedFindings": [
            {
                "severity": "suppressed",
                "kind": item.kind,
                "key": item.key,
                "message": item.message,
                "reason": item.reason,
            }
            for item in suppressed
        ],
        "acceptedIdenticalFindings": [
            {
                "severity": "accepted",
                "kind": item.kind,
                "key": item.key,
                "message": item.message,
                "reason": item.reason,
            }
            for item in accepted
        ],
    }


def run_validate(args) -> int:
    lang = args.lang
    if lang == "qps":
        print("qps is an in-memory pseudo-locale, not a translation table", file=sys.stderr)
        return 2
    if lang not in LANGUAGES or lang == "en":
        print(f"unknown language {lang}; expected one of: {', '.join(TARGET_CODES)}", file=sys.stderr)
        return 2
    en_path = Path(args.en) if args.en else DEFAULT_EN
    table_path = Path(args.table) if args.table else en_path.with_name(lang + ".json")
    if not en_path.is_file():
        print(f"English table not found: {en_path}", file=sys.stderr)
        return 2
    if not table_path.is_file():
        print(f"translation table not found: {table_path}", file=sys.stderr)
        return 2
    try:
        _en_raw, english = load_json(en_path)
        raw, translation = load_json(table_path)
    except json.JSONDecodeError as exc:
        print(f"invalid JSON: {exc}", file=sys.stderr)
        return 2

    index_path = Path(args.key_index) if args.key_index else DEFAULT_INDEX
    glossary_path = Path(args.glossary) if args.glossary else DEFAULT_GLOSSARY
    chains_path = Path(args.fonts_chains) if args.fonts_chains else DEFAULT_CHAINS
    fonts_dir = Path(args.fonts_dir) if args.fonts_dir else DEFAULT_FONTS

    fit_path = None
    if args.fit_exceptions:
        fit_path = Path(args.fit_exceptions)
        if not fit_path.is_file():
            print(f"fit exceptions not found: {fit_path}", file=sys.stderr)
            return 2
    else:
        default_fit = LOC_DIR / "notes" / f"{lang}-fit-exceptions.json"
        if default_fit.is_file():
            fit_path = default_fit
    try:
        fit_exceptions = load_fit_exceptions(fit_path) if fit_path is not None else {}
    except (json.JSONDecodeError, ValueError, OSError) as exc:
        print(f"invalid fit exceptions: {exc}", file=sys.stderr)
        return 2
    for group in args.ignore_fit_keys or []:
        for key in group:
            fit_exceptions.setdefault(key, "ignored with --ignore-fit-keys")

    identical_path = LOC_DIR / "notes" / f"{lang}-identical-ok.json"
    try:
        identical_ok = load_identical_ok(identical_path) if identical_path.is_file() else {}
    except (json.JSONDecodeError, ValueError, OSError) as exc:
        print(f"invalid identical-ok file: {exc}", file=sys.stderr)
        return 2

    options = {
        "fail_ratio": args.max_ratio if args.max_ratio is not None else FAIL_RATIO,
        "glossary": parse_glossary(glossary_path) if glossary_path.is_file() else [],
        "aliases": parse_aliases(glossary_path) if glossary_path.is_file() else [],
        "index": load_key_index(index_path) if index_path.is_file() else {},
        "chains": load_chains(chains_path),
        "fonts_dir": fonts_dir,
        "raw": raw,
        "skip_glyphs": args.no_glyph,
        "fit_exceptions": fit_exceptions,
        "identical_ok": identical_ok,
        "register_check": args.register_check,
    }
    pre = []
    if not glossary_path.is_file():
        add(pre, "warning", "glossary", "table", f"glossary not found: {glossary_path}")
    options["covered"] = prepare_fonts(lang, options, pre)
    findings = pre + validate(english, translation, lang, options)
    findings.sort(key=lambda item: (ordinal_bytes(item.key), item.kind))

    print(f"lang: {lang}")
    print(f"table: {table_path}")
    print(summarise(findings))
    for item in findings:
        if item.severity in ("suppressed", "accepted") and not args.show_suppressed:
            continue
        print(item.line())
    if args.json:
        out = Path(args.json)
        out.parent.mkdir(parents=True, exist_ok=True)
        out.write_text(json.dumps(findings_json(lang, findings), ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    errors = sum(1 for item in findings if item.severity == "error")
    return 1 if errors else 0


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description="Validate an Orbital Salvage translation table.")
    parser.add_argument("--lang", required=True, help="registry code, for example de")
    parser.add_argument("--table", help="translation JSON (default: next to en.json)")
    parser.add_argument("--en", help="English source JSON")
    parser.add_argument("--key-index", dest="key_index", help="key-index.csv")
    parser.add_argument("--glossary", help="glossary.md")
    parser.add_argument("--fonts-dir", help="directory of .ttf files")
    parser.add_argument("--fonts-chains", help="fonts-chains.json")
    parser.add_argument("--max-ratio", type=float, help="Latin length error ratio (default 2.5)")
    parser.add_argument("--json", help="also write a JSON report to this path")
    parser.add_argument("--no-glyph", action="store_true", help="skip the cmap check")
    parser.add_argument(
        "--fit-exceptions",
        help="JSON allowlist of accepted fit warnings (default: localization/notes/<code>-fit-exceptions.json when present)",
    )
    parser.add_argument(
        "--ignore-fit-keys",
        action="append",
        nargs="+",
        default=[],
        metavar="KEY",
        help="suppress fit warnings for these keys (spec:Key or the English key)",
    )
    parser.add_argument(
        "--show-suppressed",
        action="store_true",
        help="print suppressed fit warnings and accepted identical copies",
    )
    parser.add_argument(
        "--register-check",
        action="store_true",
        help="warn that imperative and telegraphic style is not checked",
    )
    args = parser.parse_args(argv)
    return run_validate(args)


if __name__ == "__main__":
    sys.exit(main())
