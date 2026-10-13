#!/usr/bin/env python3
"""Shape ar/hi/km UI strings into logical runs.

Hindi uses HarfBuzz script ``dev2``. On Baloo 2, script ``deva`` leaves
``क्ष`` as separate letters. Khmer is ``khmr``. Arabic is ``arab``.

Letter-spacing is not applied here. Advances stay in font units.
"""

from __future__ import annotations

import re
import unicodedata

import uharfbuzz as hb
from bidi.algorithm import (
    PARAGRAPH_LEVELS,
    explicit_embed_and_overrides,
    get_embedding_levels,
    get_empty_storage,
    resolve_implicit_levels,
    resolve_neutral_types,
    resolve_weak_types,
)

from shaped_format import (
    KIND_EMOJI,
    KIND_GLYPH,
    KIND_LATIN,
    KIND_SLOT,
    KIND_SYMBOL,
)

PLACEHOLDER = re.compile(r"\{(\d+)\}")

# Drawn by the existing cmap / emoji path. Cairo and the other text faces
# do not contain the UI arrows; a baked gid would be .notdef.
SYMBOLS = {
    0x00D7,  # ×
    0x2022,  # •
    0x2190,  # ←
    0x2192,  # →
    0x2194,
    0x2026,  # …  not in the script faces; the symbol chain has it
    0x23E9,  # ⏩
    0x23EA,  # ⏪
    0x25B6,  # ▶
    0x25C0,
    0x2605,
    0x2699,  # ⚙
    0x26A1,  # ⚡
    0x2713,  # ✓
    0x2714,
    0x2717,
    0x2728,  # ✨
    0x2B50,  # ⭐
}

LANGS = {
    "ar": {"script": "arab", "base": "R", "font": "Cairo-700"},
    "hi": {"script": "dev2", "base": "L", "font": "Baloo2-700"},
    "km": {"script": "khmr", "base": "L", "font": "KantumruyPro-700"},
}

ENDONYMS = {
    "ar": "العربية",
    "hi": "हिन्दी",
    "km": "ភាសាខ្មែរ",
}


class BakeError(Exception):
    pass


def bidi_levels(text: str, base_dir: str) -> tuple[int, list[int]]:
    storage = get_empty_storage()
    base_level = PARAGRAPH_LEVELS[base_dir]
    storage["base_level"] = base_level
    storage["base_dir"] = base_dir
    get_embedding_levels(text, storage, False, False)
    explicit_embed_and_overrides(storage, False)
    resolve_weak_types(storage, False)
    resolve_neutral_types(storage, False)
    resolve_implicit_levels(storage, False)
    levels = [ch["level"] for ch in storage["chars"]]
    if len(levels) != len(text):
        raise BakeError(f"bidi level count {len(levels)} != {len(text)} for {text!r}")
    return base_level, levels


def _is_script(cp: int, lang: str) -> bool:
    if lang == "ar":
        return (
            0x0600 <= cp <= 0x06FF
            or 0x0750 <= cp <= 0x077F
            or 0x08A0 <= cp <= 0x08FF
            or 0xFB50 <= cp <= 0xFDFF
            or 0xFE70 <= cp <= 0xFEFF
        )
    if lang == "hi":
        return (
            0x0900 <= cp <= 0x097F
            or 0x1CD0 <= cp <= 0x1CFF
            or 0xA8E0 <= cp <= 0xA8FF
            or cp in (0x200C, 0x200D)
        )
    if lang == "km":
        return 0x1780 <= cp <= 0x17FF or 0x19E0 <= cp <= 0x19FF or cp in (0x200C, 0x200D)
    return False


def _emoji_span(text: str, i: int) -> int:
    cp = ord(text[i])
    if not _is_emoji(cp):
        return 0
    j = i + 1
    while j < len(text):
        nxt = ord(text[j])
        if nxt in (0xFE0F, 0x200D) or _is_emoji(nxt) or 0x1F3FB <= nxt <= 0x1F3FF:
            j += 1
            continue
        break
    return j - i


def _is_emoji(cp: int) -> bool:
    if cp in SYMBOLS:
        return False
    if 0x1F300 <= cp <= 0x1FAFF:
        return True
    if 0x1F1E6 <= cp <= 0x1F1FF:
        return True
    if cp in (0x2764, 0xFE0F):
        return True
    return False


def _kind_of(cp: int, lang: str) -> str:
    if cp in SYMBOLS:
        return "symbol"
    if _is_emoji(cp):
        return "emoji"
    if _is_script(cp, lang):
        return "script"
    if cp == 0x20 or unicodedata.category(chr(cp)).startswith("Z"):
        return "space"
    if 0x30 <= cp <= 0x39:
        return "digit"
    if unicodedata.category(chr(cp)).startswith("L") and cp < 0x0250:
        return "latin"
    o = unicodedata.category(chr(cp))
    if o.startswith("M"):
        return "mark"
    return "punct"


def _joining_neighbor(ch: str | None) -> bool:
    """True when the character is a letter of Arabic, Devanagari, or Khmer.

    A placeholder may sit next to punctuation, a Western digit, or a Latin
    token (``+{0}``, ``{0}h``, ``({1}/{0}``). It may not sit against a
    joining letter or a conjunct piece.
    """
    if ch is None or ch.isspace():
        return False
    cp = ord(ch)
    return _is_script(cp, "ar") or _is_script(cp, "hi") or _is_script(cp, "km")


def check_placeholders(text: str) -> None:
    """``{n}`` must sit on a space or digit boundary, or a string edge.

    A placeholder glued to a joining letter, or dropped inside a conjunct,
    has a letter on one side. That fails the bake instead of shaping a
    broken join.
    """
    for match in PLACEHOLDER.finditer(text):
        before = text[match.start() - 1] if match.start() > 0 else None
        after = text[match.end()] if match.end() < len(text) else None
        if not _joining_neighbor(before) and not _joining_neighbor(after):
            continue
        left = before if before is not None else "start"
        right = after if after is not None else "end"
        raise BakeError(
            "placeholder {"
            + match.group(1)
            + "} in "
            + repr(text)
            + " is glued to a joining letter "
            + f"(left={left!r}, right={right!r}). "
            + "It must not sit inside a joining or conjunct sequence."
        )


def _atoms(text: str, lang: str) -> list[dict]:
    atoms = []
    i = 0
    while i < len(text):
        match = PLACEHOLDER.match(text, i)
        if match:
            atoms.append({"kind": "slot", "text": match.group(0), "slot": int(match.group(1)), "i": i})
            i = match.end()
            continue
        span = _emoji_span(text, i)
        if span:
            atoms.append({"kind": "emoji", "text": text[i : i + span], "i": i})
            i += span
            continue
        kind = _kind_of(ord(text[i]), lang)
        j = i + 1
        if kind != "mark":
            while j < len(text) and not PLACEHOLDER.match(text, j) and _emoji_span(text, j) == 0:
                nxt = _kind_of(ord(text[j]), lang)
                if nxt != kind:
                    break
                j += 1
        atoms.append({"kind": kind, "text": text[i:j], "i": i})
        i = j
    return atoms


def _merge(atoms: list[dict]) -> list[dict]:
    # Marks and punctuation that sit on a script letter stay in the script
    # slice so HarfBuzz can position them. Spaces between script letters stay
    # in that slice too (Arabic and Hindi break on those spaces).
    out: list[dict] = []
    for atom in atoms:
        kind = atom["kind"]
        if kind == "mark" and out and out[-1]["kind"] == "script":
            out[-1]["text"] += atom["text"]
            continue
        if kind == "punct" and out and out[-1]["kind"] == "script":
            out[-1]["text"] += atom["text"]
            continue
        if kind in ("digit", "latin"):
            kind = "latin"
            atom = dict(atom, kind="latin")
        if kind == "space" and out and out[-1]["kind"] == "script":
            # Keep the space on the script run when the next atom is also
            # script. Decided in a second pass; stash as its own atom first.
            pass
        if out and out[-1]["kind"] == kind and kind in ("script", "latin", "symbol", "emoji", "punct"):
            out[-1]["text"] += atom["text"]
            continue
        out.append(dict(atom, kind=kind))
    glued: list[dict] = []
    for idx, atom in enumerate(out):
        if atom["kind"] != "space":
            glued.append(atom)
            continue
        prev_script = glued and glued[-1]["kind"] == "script"
        nxt = out[idx + 1]["kind"] if idx + 1 < len(out) else ""
        if prev_script and nxt == "script":
            glued[-1]["text"] += atom["text"]
        elif prev_script and nxt != "script":
            glued[-1]["text"] += atom["text"]
        elif nxt == "script" and glued:
            # Leading space of a script word. Attach it so the advance is the
            # script font's space, not Fredoka's.
            atom = dict(atom)
            # Prepend onto the following script atom when we see it.
            atom["kind"] = "lead-space"
            glued.append(atom)
        else:
            atom = dict(atom, kind="latin")
            if glued and glued[-1]["kind"] == "latin":
                glued[-1]["text"] += atom["text"]
            else:
                glued.append(atom)
    merged: list[dict] = []
    for atom in glued:
        if atom["kind"] == "lead-space" and False:
            pass
        merged.append(atom)
    final: list[dict] = []
    i = 0
    while i < len(merged):
        if merged[i]["kind"] == "lead-space" and i + 1 < len(merged) and merged[i + 1]["kind"] == "script":
            nxt = dict(merged[i + 1])
            nxt["text"] = merged[i]["text"] + nxt["text"]
            nxt["i"] = merged[i]["i"]
            final.append(nxt)
            i += 2
            continue
        if merged[i]["kind"] == "lead-space":
            atom = dict(merged[i], kind="latin")
            final.append(atom)
            i += 1
            continue
        final.append(merged[i])
        i += 1
    # Adjacent latin fragments ("Orbital " + "Salvage") must be one run.
    # Two level-2 runs would be reversed against each other by bidi L2.
    coalesced: list[dict] = []
    for atom in final:
        if (
            coalesced
            and atom["kind"] == coalesced[-1]["kind"]
            and atom["kind"] in ("latin", "symbol", "emoji")
        ):
            coalesced[-1]["text"] += atom["text"]
            continue
        coalesced.append(atom)
    return coalesced


def _utf16_map(text: str) -> list[int]:
    out = []
    u = 0
    for ch in text:
        out.append(u)
        u += 2 if ord(ch) > 0xFFFF else 1
    return out


def _khmer_cluster_start(text: str, index: int) -> bool:
    if index <= 0:
        return True
    cp = ord(text[index])
    prev = ord(text[index - 1])
    if prev == 0x17D2:
        return False
    if cp == 0x17D2:
        return False
    if 0x17B6 <= cp <= 0x17D3 or cp == 0x17DD:
        return False
    return True


class Face:
    def __init__(self, path: str, lang: str):
        spec = LANGS[lang]
        self.lang = lang
        self.script = spec["script"]
        self.base_dir = spec["base"]
        self.path = path
        blob = hb.Blob.from_file_path(path)
        face = hb.Face(blob)
        self.font = hb.Font(face)
        self.upem = face.upem
        self.font.scale = (self.upem, self.upem)

    def shape_slice(self, text: str, level: int) -> list[dict]:
        buf = hb.Buffer()
        buf.cluster_level = hb.BufferClusterLevel.MONOTONE_GRAPHEMES
        buf.add_str(text)
        buf.direction = "rtl" if level % 2 else "ltr"
        buf.script = self.script
        buf.language = self.lang
        hb.shape(self.font, buf)
        glyphs = []
        for info, pos in zip(buf.glyph_infos, buf.glyph_positions):
            if pos.y_advance:
                raise BakeError(f"non-zero y_advance {pos.y_advance} in {text!r}; the table stores dx/dy only")
            glyphs.append(
                {
                    "gid": int(info.codepoint),
                    "cluster": int(info.cluster),
                    "adv": int(pos.x_advance),
                    "dx": int(pos.x_offset),
                    "dy": int(pos.y_offset),
                }
            )
        return glyphs


def _level_at(levels: list[int], index: int) -> int:
    if index < 0:
        return levels[0] if levels else 0
    if index >= len(levels):
        return levels[-1] if levels else 0
    return levels[index]


def shape_string(face: Face, text: str) -> dict:
    text = unicodedata.normalize("NFC", text)
    check_placeholders(text)
    base, levels = bidi_levels(text, face.base_dir)
    u16 = _utf16_map(text)
    atoms = _merge(_atoms(text, face.lang))
    runs = []
    for atom in atoms:
        start = atom["i"]
        raw = atom["text"]
        if atom["kind"] == "slot":
            # The digit inside {n} carries the European-number level.
            digit_at = start
            for k, ch in enumerate(raw):
                if ch.isdigit():
                    digit_at = start + k
                    break
            runs.append(
                {
                    "kind": KIND_SLOT,
                    "level": _level_at(levels, digit_at),
                    "slot": atom["slot"],
                    "text": raw,
                    "glyphs": [],
                }
            )
            continue
        if atom["kind"] == "script":
            level = _level_at(levels, start)
            # Split the slice if the bidi level changes inside it.
            pieces = _split_levels(raw, start, levels)
            for piece, piece_start, piece_level in pieces:
                glyphs = face.shape_slice(piece, piece_level)
                mapped = []
                for g in glyphs:
                    cp_index = piece_start + g["cluster"]
                    if cp_index < 0 or cp_index >= len(text):
                        raise BakeError(f"cluster {cp_index} outside {text!r}")
                    src = text[cp_index]
                    if g["gid"] == 0 and not src.isspace():
                        raise BakeError(
                            f"font has no glyph for U+{ord(src):04X} ({src}) in {text!r}. "
                            "Refusing to bake a .notdef box."
                        )
                    break_before = _break_before(text, cp_index, face.lang)
                    mapped.append(
                        {
                            "gid": g["gid"],
                            "adv": g["adv"],
                            "dx": g["dx"],
                            "dy": g["dy"],
                            "cluster": u16[cp_index],
                            "break_before": break_before,
                        }
                    )
                # break_before only on the first glyph of each cluster.
                seen = set()
                for g in mapped:
                    if g["cluster"] in seen:
                        g["break_before"] = False
                    else:
                        seen.add(g["cluster"])
                runs.append(
                    {
                        "kind": KIND_GLYPH,
                        "level": piece_level,
                        "slot": 0,
                        "text": piece,
                        "glyphs": mapped,
                    }
                )
            continue
        kind = {"latin": KIND_LATIN, "symbol": KIND_SYMBOL, "emoji": KIND_EMOJI, "punct": KIND_LATIN}[atom["kind"]]
        for piece, piece_start, piece_level in _split_levels(raw, start, levels):
            runs.append(
                {
                    "kind": kind,
                    "level": piece_level,
                    "slot": 0,
                    "text": piece,
                    "glyphs": [],
                }
            )
    return {"text": text, "base_level": base, "runs": runs}


def _split_levels(piece: str, start: int, levels: list[int]) -> list[tuple[str, int, int]]:
    out = []
    i = 0
    while i < len(piece):
        level = levels[start + i]
        j = i + 1
        while j < len(piece) and levels[start + j] == level:
            j += 1
        out.append((piece[i:j], start + i, level))
        i = j
    return out


def _break_before(text: str, index: int, lang: str) -> bool:
    if index <= 0:
        return True
    if lang == "km":
        return _khmer_cluster_start(text, index)
    prev = text[index - 1]
    return prev.isspace()


def visual_glyphs(entry: dict) -> list[dict]:
    """L2 reorder of logical runs. Glyphs inside a run stay in HarfBuzz order."""
    runs = [dict(r, glyphs=list(r["glyphs"])) for r in entry["runs"]]
    if not runs:
        return []
    max_level = max(r["level"] for r in runs)
    seq = runs
    for level in range(max_level, 0, -1):
        i = 0
        while i < len(seq):
            if seq[i]["level"] < level:
                i += 1
                continue
            j = i + 1
            while j < len(seq) and seq[j]["level"] >= level:
                j += 1
            seq[i:j] = list(reversed(seq[i:j]))
            i = j
    out = []
    for run in seq:
        out.extend(run["glyphs"])
    return out


def level_runs(levels: list[int]) -> list[tuple[int, int, int]]:
    runs = []
    i = 0
    while i < len(levels):
        j = i + 1
        while j < len(levels) and levels[j] == levels[i]:
            j += 1
        runs.append((i, j, levels[i]))
        i = j
    return runs


def hb_visual(face: Face, text: str) -> list[dict]:
    """Shape the whole string with one face, including digits. Proof helper."""
    text = unicodedata.normalize("NFC", text)
    _base, levels = bidi_levels(text, face.base_dir)
    parts = []
    for start, end, level in level_runs(levels):
        glyphs = face.shape_slice(text[start:end], level)
        tagged = []
        for g in glyphs:
            tagged.append(dict(g, level=level, cluster=start + g["cluster"]))
        parts.append(tagged)
    return _reorder_parts(parts)


def stitch_match(face: Face, template: str, number: str) -> bool:
    """Independent fragments match a fully shaped sentence on gid, dx, and dy.

    The number is shaped with the same face here. At runtime it is drawn
    with the Latin cmap face instead; this check only proves the split
    does not change the script glyphs.
    """
    if template.count("{0}") != 1:
        raise BakeError("stitch check wants a single {0}")
    check_placeholders(template)
    filled = template.replace("{0}", number)
    full = hb_visual(face, filled)
    pre, post = template.split("{0}")
    parts = []
    for part in (pre, number, post):
        if part == "":
            continue
        glyphs = hb_visual(face, part)
        if face.base_dir == "R":
            level = 2 if part == number else 1
            glyphs = [dict(g, level=level) for g in glyphs]
        parts.append(glyphs)
    stitched = _reorder_parts(parts) if face.base_dir == "R" else [g for part in parts for g in part]
    full_ids = [(g["gid"], g["dx"], g["dy"]) for g in full]
    stitch_ids = [(g["gid"], g["dx"], g["dy"]) for g in stitched]
    return full_ids == stitch_ids


def _reorder_parts(parts: list[list[dict]]) -> list[dict]:
    runs = []
    for glyphs in parts:
        if not glyphs:
            continue
        runs.append(glyphs)
    if not runs:
        return []
    max_level = max(g[0]["level"] for g in runs)
    for level in range(max_level, 0, -1):
        i = 0
        while i < len(runs):
            if runs[i][0]["level"] < level:
                i += 1
                continue
            j = i + 1
            while j < len(runs) and runs[j][0]["level"] >= level:
                j += 1
            runs[i:j] = list(reversed(runs[i:j]))
            i = j
    out = []
    for g in runs:
        out.extend(g)
    return out


def endonym_strings(lang: str) -> list[str]:
    name = ENDONYMS[lang]
    return [name, "\u2713 " + name]
