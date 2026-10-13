#!/usr/bin/env python3
"""Self-test for the ar/hi/km bake. Does not touch translation tables."""

from __future__ import annotations

import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))

from shaped_engine import BakeError, Face, check_placeholders, shape_string, stitch_match
from shaped_format import KIND_GLYPH, KIND_LATIN, KIND_SLOT, KIND_SYMBOL, key_hash, pack_table, sha256_file, unpack_table

FONTS = HERE.parents[1] / "unity" / "maze-puzzle" / "Assets" / "Fonts"
FIXTURE = HERE / "fixtures" / "shaped-ui.json"


def face(lang: str) -> Face:
    name = {"ar": "Cairo-700.ttf", "hi": "Baloo2-700.ttf", "km": "KantumruyPro-700.ttf"}[lang]
    path = FONTS / name
    if not path.is_file():
        raise SystemExit(f"missing {path}; run build_shaped_fonts.py")
    return Face(str(path), lang)


def test_arabic_joins():
    shaped = shape_string(face("ar"), "كتاب")
    glyphs = [g for run in shaped["runs"] if run["kind"] == KIND_GLYPH for g in run["glyphs"]]
    assert len(glyphs) >= 1
    # Isolated cmap ids are not the joined forms. HarfBuzz must substitute.
    from fontTools.ttLib import TTFont

    cmap = TTFont(FONTS / "Cairo-700.ttf").getBestCmap()
    nominal = [cmap[ord(ch)] for ch in "كتاب"]
    got = [g["gid"] for g in glyphs]
    assert got != nominal, (got, nominal)
    assert all(g["gid"] != 0 for g in glyphs)


def test_devanagari_conjunct_offset():
    shaped = shape_string(face("hi"), "क्ष")
    glyphs = [g for run in shaped["runs"] if run["kind"] == KIND_GLYPH for g in run["glyphs"]]
    assert any(g["dx"] != 0 or g["dy"] != 0 for g in glyphs), glyphs
    # The virama and the following consonant stay one unbreakable chunk.
    middles = [g for g in glyphs if not g["break_before"] or g["cluster"] != 0]
    assert any(not g["break_before"] for g in glyphs[1:]), glyphs


def test_khmer_coeng_one_cluster():
    text = "ខ្មែរ"
    shaped = shape_string(face("km"), text)
    glyphs = [g for run in shaped["runs"] if run["kind"] == KIND_GLYPH for g in run["glyphs"]]
    clusters = {g["cluster"] for g in glyphs}
    # U+17D2 coeng must not open its own line-start cluster.
    coeng_at = text.index("\u17d2")
    assert all(g["cluster"] != coeng_at or not g["break_before"] for g in glyphs)
    assert len(clusters) < len(text)
    # A later base may start a line. The coeng sequence may not.
    assert any(g["break_before"] and g["cluster"] > 0 for g in glyphs) or len(clusters) == 1


def test_placeholder_stitch():
    samples = {
        "ar": "الهدف: {0} كغ",
        "hi": "लक्ष्य: {0} किग्रा",
        "km": "គោលដៅ៖ {0} គីឡូក្រាម",
    }
    for lang, template in samples.items():
        assert stitch_match(face(lang), template, "20"), lang


def test_glued_placeholder_fails():
    samples = ["كلمة{0}", "क्ष{0}", "ខ្មែរ{0}", "{0}كلمة"]
    for text in samples:
        try:
            check_placeholders(text)
        except BakeError as exc:
            assert "placeholder" in str(exc).lower()
            continue
        raise AssertionError("bake accepted a glued placeholder: " + text)
    check_placeholders("كلمة {0} كغ")
    check_placeholders("{0} كغ")
    check_placeholders("+{0}")
    check_placeholders("{0}h {1}m")
    check_placeholders("({1}/{0})")
    check_placeholders("{0}!")


def test_latin_only_stays_ltr():
    fc = face("ar")
    quota = shape_string(fc, "{0} / {1} kg")
    assert quota["base_level"] == 0, quota["base_level"]
    kg = shape_string(fc, "{0} kg")
    assert kg["base_level"] == 0, kg["base_level"]
    mixed = shape_string(fc, "هدف: {0} kg")
    assert mixed["base_level"] == 1, mixed["base_level"]


def test_roundtrip_and_symbol_run():
    fc = face("ar")
    entry = shape_string(fc, "← رجوع")
    kinds = [run["kind"] for run in entry["runs"]]
    assert KIND_SYMBOL in kinds, kinds
    assert KIND_GLYPH in kinds, kinds
    digest = sha256_file(FONTS / "Cairo-700.ttf")
    blob = pack_table("Cairo-700", 700, fc.upem, digest, [entry, shape_string(fc, "الهدف: {0} كغ")])
    unpacked = unpack_table(blob)
    assert unpacked["font"] == "Cairo-700"
    assert unpacked["font_sha256"] == digest
    assert len(unpacked["entries"]) == 2
    slot = shape_string(fc, "الهدف: {0} كغ")
    assert any(run["kind"] == KIND_SLOT for run in slot["runs"])
    assert any(run["kind"] == KIND_LATIN for run in shape_string(fc, "المرحلة 12 من Orbital Salvage")["runs"])
    assert key_hash(entry["text"]) == unpacked["entries"][0]["hash"] or key_hash(entry["text"]) in {
        item["hash"] for item in unpacked["entries"]
    }


def main() -> int:
    tests = [
        test_arabic_joins,
        test_devanagari_conjunct_offset,
        test_khmer_coeng_one_cluster,
        test_placeholder_stitch,
        test_glued_placeholder_fails,
        test_latin_only_stays_ltr,
        test_roundtrip_and_symbol_run,
    ]
    for test in tests:
        test()
        print(f"ok {test.__name__}")
    print(f"ok {len(tests)} shaping groups")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except AssertionError as exc:
        print("FAIL", exc, file=sys.stderr)
        raise
