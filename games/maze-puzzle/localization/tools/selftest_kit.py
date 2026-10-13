#!/usr/bin/env python3
"""Plain asserts for the translation kit. No pytest.

Run: python3 games/maze-puzzle/localization/tools/selftest_kit.py
"""

from __future__ import annotations

import json
import subprocess
import sys
import tempfile
from pathlib import Path

import make_translation_stubs
import merge_table
import review_sample
import validate_table as kit

ROOT = Path(__file__).resolve().parent


def has(findings, severity, kind, key):
    return any(
        item.severity == severity and item.kind == kind and item.key == key for item in findings
    )


def kinds(findings, kind):
    return [item for item in findings if item.kind == kind]


def table(lang, spec, strings=None, meta_extra=None, do_not=None):
    info = kit.LANGUAGES[lang]
    meta = {"code": lang, "name": info[1], "version": 1, "source": "en"}
    if meta_extra:
        meta.update(meta_extra)
    body = {"meta": meta, "spec": spec, "str": strings or {}}
    if do_not is not None:
        body["doNotTranslate"] = do_not
    return kit.PairObj(list(body.items()))


def wrap_maps(spec, strings=None):
    """Build PairObj sections so order checks see real order."""
    spec_obj = kit.PairObj(list(spec.items()))
    str_obj = kit.PairObj(list((strings or {}).items()))
    return spec_obj, str_obj


def make(lang, spec, strings=None, meta_extra=None, do_not=None):
    spec_obj, str_obj = wrap_maps(spec, strings)
    info = kit.LANGUAGES[lang]
    meta = {"code": lang, "name": info[1], "version": 1, "source": "en"}
    if meta_extra:
        meta.update(meta_extra)
    pairs = [("meta", meta), ("spec", spec_obj), ("str", str_obj)]
    if do_not is not None:
        pairs.append(("doNotTranslate", do_not))
    return kit.PairObj(pairs)


def options(**extra):
    base = {
        "fail_ratio": kit.FAIL_RATIO,
        "glossary": [],
        "index": {},
        "chains": {},
        "fonts_dir": None,
        "raw": "",
        "skip_glyphs": True,
        "covered": None,
    }
    base.update(extra)
    return base


def check(english, translation, lang, **extra):
    return kit.validate(english, translation, lang, options(**extra))


def test_csharp_parity():
    english = make(
        "en",
        {"Hello": "Hello", "lost": "Lost", "gone": "{0} {1}", "Boom": "Hello"},
        {"ok": "OK", "bad": "ab"},
    )
    german = make(
        "de",
        {"Hello": "12345678", "gone": "{0}", "extra": "x", "Boom": "1234567890123"},
        {"ok": " OK", "bad": "a\nb"},
    )
    findings = check(english, german, "de")
    assert has(findings, "warning", "length", "spec:Hello"), findings
    assert has(findings, "error", "length", "spec:Boom"), findings
    assert has(findings, "error", "missing", "spec:lost"), findings
    assert has(findings, "error", "placeholder", "spec:gone"), findings
    assert has(findings, "error", "extra", "spec:extra"), findings
    assert has(findings, "warning", "whitespace", "str:ok"), findings
    assert has(findings, "error", "control", "str:bad"), findings
    lost = next(item for item in findings if item.key == "spec:lost" and item.kind == "missing")
    assert lost.line().startswith("error\tmissing\tspec:lost\t")

    zh_en = make("en", {"Hi": "Hi"})
    zh = make("zh-Hans", {"Hi": "字" * 40})
    cjk = check(zh_en, zh, "zh-Hans")
    assert not kinds(cjk, "length")
    assert has(cjk, "error", "expansion", "spec:Hi"), cjk


def test_untranslated_and_allowlist():
    english = make(
        "en",
        {
            "Hello": "Hello",
            "105 kg": "105 kg",
            "ORBITAL": "ORBITAL",
            "PLAY": "PLAY",
            "footer": "Orbital Salvage • Space Trash Clean v1.2.0",
        },
        do_not=["×"],
    )
    german = make(
        "de",
        {
            "Hello": "Hello",
            "105 kg": "105 kg",
            "ORBITAL": "ORBITAL",
            "PLAY": "PLAY",
            "footer": "Orbital Salvage • Space Trash Clean v1.2.0",
        },
    )
    findings = check(english, german, "de")
    assert has(findings, "error", "untranslated", "spec:Hello")
    assert has(findings, "error", "untranslated", "spec:PLAY")
    assert not has(findings, "error", "untranslated", "spec:105 kg")
    assert not has(findings, "error", "untranslated", "spec:ORBITAL")
    assert not has(findings, "error", "untranslated", "spec:footer")


def test_emoji_placeholder_empty_keybytes():
    english = make("en", {"Go": "Go \u2713", "Swap": "{1} then {0}", "Blank": "Name"})
    moved = make("de", {"Go": "\u2713 Los", "Swap": "{0} dann {1}", "Blank": ""})
    findings = check(english, moved, "de")
    assert has(findings, "error", "emoji", "spec:Go"), findings
    assert not has(findings, "error", "placeholder", "spec:Swap")
    assert has(findings, "error", "empty", "spec:Blank")

    kept = make("de", {"Go": "Los \u2713", "Swap": "{0} dann {1}", "Blank": "Name"})
    ok = check(english, kept, "de")
    assert not has(ok, "error", "emoji", "spec:Go")
    assert not has(ok, "error", "placeholder", "spec:Swap")

    source = make("en", {"Hello": "Hello"})
    funky = kit.PairObj(
        [
            ("meta", {"code": "de", "name": "Deutsch", "version": 1, "source": "en"}),
            ("spec", kit.PairObj([("Hello ", "Hallo")])),
            ("str", kit.PairObj([])),
        ]
    )
    bad_key = check(source, funky, "de")
    assert has(bad_key, "error", "key-bytes", "spec:Hello "), bad_key
    assert has(bad_key, "error", "missing", "spec:Hello")


def test_script_and_orthography():
    english = make("en", {"Hello": "Hello"})
    assert has(check(english, make("de", {"Hello": "汉字"}), "de"), "error", "script", "spec:Hello")
    assert has(check(english, make("zh-Hans", {"Hello": "テスト"}), "zh-Hans"), "error", "script", "spec:Hello")
    assert has(check(english, make("zh-Hant", {"Hello": "關閉"}), "zh-Hant"), "warning", "script", "spec:Hello") is False
    han = check(english, make("zh-Hans", {"Hello": "关闭"}), "zh-Hans")
    assert not has(han, "error", "script", "spec:Hello")
    assert not has(han, "warning", "script", "spec:Hello")

    ja = check(english, make("ja", {"Hello": "設定"}), "ja")
    assert not has(ja, "error", "script", "spec:Hello")
    assert has(ja, "warning", "script", "table")
    ja_kana = check(english, make("ja", {"Hello": "とじる"}), "ja")
    assert not kinds(ja_kana, "script") or not has(ja_kana, "error", "script", "spec:Hello")
    assert not has(ja_kana, "warning", "script", "table")

    assert has(check(english, make("ko", {"Hello": "안녕"}), "ko"), "error", "script", "spec:Hello") is False
    assert has(check(english, make("ko", {"Hello": "Hello"}), "ko"), "error", "script", "spec:Hello")
    assert has(check(english, make("hi", {"Hello": "नमस्ते"}), "hi"), "error", "script", "spec:Hello") is False
    assert has(check(english, make("hi", {"Hello": "Hello"}), "hi"), "error", "script", "spec:Hello")
    assert has(check(english, make("km", {"Hello": "សួស្តី"}), "km"), "error", "script", "spec:Hello") is False
    assert has(check(english, make("ar", {"Hello": "مرحبا"}), "ar"), "error", "script", "spec:Hello") is False
    assert has(check(english, make("ar", {"Hello": "Hello"}), "ar"), "error", "script", "spec:Hello")
    latin_ar = check(english, make("ar", {"Hello": "مرحبا Shop"}), "ar")
    assert has(latin_ar, "error", "script", "spec:Hello"), latin_ar
    kept = check(english, make("ar", {"Hello": "مرحبا Nibbles"}), "ar")
    assert not any(item.kind == "script" and item.severity == "error" for item in kept), kept

    uz_bad = check(english, make("uz", {"Hello": "o'zbekcha salom"}), "uz")
    assert has(uz_bad, "warning", "orthography", "spec:Hello"), uz_bad
    uz_ok = check(english, make("uz", {"Hello": "o\u02bbzbekcha salom"}), "uz")
    assert not has(uz_ok, "warning", "orthography", "spec:Hello")

    fr_bad = check(english, make("fr", {"Hello": "Bonjour !"}), "fr")
    assert has(fr_bad, "warning", "punctuation", "spec:Hello")
    fr_ok = check(english, make("fr", {"Hello": "Bonjour\u00a0!"}), "fr")
    assert not has(fr_ok, "warning", "punctuation", "spec:Hello")

    vi_bad = check(english, make("vi", {"Hello": "汉字"}), "vi")
    assert has(vi_bad, "error", "script", "spec:Hello")


def test_numbers_glossary_uppercase_fit_stale():
    english = make("en", {"Mass": "105 kg", "Sector": "Restart sector", "Title": "AUDIO"})
    bad = make("de", {"Mass": "105", "Sector": "Neu starten", "Title": "AUDIO"})
    glossary = [
        {
            "en": "sector",
            "dnt": False,
            "check": True,
            "alternates": ["sector"],
            "de": "Sektor",
        }
    ]
    index = {
        ("spec", "Title"): {"fz": "16px", "uppercase": "uppercase", "max_width": "400", "key": "Title"},
        ("spec", "Mass"): {"fz": "20px", "uppercase": "none", "max_width": "80", "key": "Mass"},
        ("spec", "Sector"): {"fz": "10px", "uppercase": "none", "max_width": "40", "key": "Sector"},
    }
    findings = check(english, bad, "de", glossary=glossary, index=index)
    assert has(findings, "error", "unit", "spec:Mass"), findings
    assert has(findings, "warning", "glossary", "spec:Sector"), findings
    assert has(findings, "warning", "uppercase", "spec:Title"), findings

    wide = make("de", {"Mass": "105 kg", "Sector": "Sektor neu starten bitte jetzt sofort extra", "Title": "Audio"})
    fit = check(english, wide, "de", glossary=glossary, index=index)
    assert has(fit, "error", "fit", "spec:Sector") or has(fit, "warning", "fit", "spec:Sector"), fit
    assert not has(fit, "warning", "uppercase", "spec:Title")
    assert not has(fit, "warning", "glossary", "spec:Sector")

    stale_meta = {
        "sourceHashes": {"spec": {"Mass": "deadbeef", "Sector": kit.source_hash("spec", "Sector", "Restart sector")}}
    }
    stale_table = make(
        "de",
        {"Mass": "105 kg", "Sector": "Sektor neu", "Title": "Audio"},
        meta_extra=stale_meta,
    )
    stale = check(english, stale_table, "de", index=index)
    assert has(stale, "warning", "stale", "spec:Mass"), stale
    assert not has(stale, "warning", "stale", "spec:Sector")
    assert has(stale, "warning", "stale", "spec:Title")

    digits = check(english, make("hi", {"Mass": "१०५ kg", "Sector": "सेक्टर", "Title": "ऑडियो"}), "hi")
    assert has(digits, "warning", "digits", "spec:Mass"), digits
    assert has(digits, "error", "number", "spec:Mass")


def test_glyphs_and_pending():
    english = make("en", {"Hi": "Hi", "Wide": "Wide"})
    good = make("vi", {"Hi": "Hãy mở cửa sổ", "Wide": "dọn rác vũ trụ"})
    chains = kit.load_chains(kit.DEFAULT_CHAINS)
    pre = []
    covered = kit.prepare_fonts(
        "vi",
        {"chains": chains, "fonts_dir": kit.DEFAULT_FONTS, "skip_glyphs": False},
        pre,
    )
    assert covered, "Vietnamese Latin cmap should load from the shipped fonts"
    findings = check(english, good, "vi", covered=covered, chains=chains)
    assert not kinds(findings, "glyph"), findings

    bad = make("vi", {"Hi": "Hãy mở \u0370", "Wide": "dọn"})
    glyph = check(english, bad, "vi", covered=covered)
    assert has(glyph, "error", "glyph", "spec:Hi"), glyph

    pending = []
    none_covered = kit.prepare_fonts(
        "ar",
        {"chains": chains, "fonts_dir": kit.DEFAULT_FONTS, "skip_glyphs": False},
        pending,
    )
    assert none_covered is not None, pending
    arabic = check(english, make("ar", {"Hi": "مرحبا", "Wide": "عالم"}), "ar", covered=none_covered, chains=chains)
    assert not kinds(arabic, "glyph"), arabic

    held = {code: dict(info) for code, info in chains.items()}
    held["ar"] = dict(held["ar"])
    held["ar"]["chain"] = []
    held["ar"]["pending"] = ["Cairo-700.ttf"]
    pending_findings = []
    still_pending = kit.prepare_fonts(
        "ar",
        {"chains": held, "fonts_dir": kit.DEFAULT_FONTS, "skip_glyphs": False},
        pending_findings,
    )
    assert still_pending is None
    assert any(item.kind == "font-pending" for item in pending_findings), pending_findings


def test_stubs_merge_sample_glossary():
    terms = kit.parse_glossary(kit.DEFAULT_GLOSSARY)
    assert 40 <= len(terms) <= 60, len(terms)
    for term in terms:
        for lang in kit.TARGET_CODES:
            assert (term.get(lang) or "").strip(), (term.get("term"), lang)
        if term.get("dnt"):
            for lang in kit.TARGET_CODES:
                assert term[lang] == term["alternates"][0] or term[lang] in term["alternates"]
    blob = kit.DEFAULT_GLOSSARY.read_text(encoding="utf-8")
    assert "\u02bb" in blob
    assert "\u202f" not in blob
    chains = json.loads(kit.DEFAULT_CHAINS.read_text(encoding="utf-8"))["languages"]
    for lang, factor in kit.EXPANSION.items():
        assert abs(chains[lang]["expansion"] - factor) < 1e-9

    with tempfile.TemporaryDirectory() as tmp:
        tmp_path = Path(tmp)
        code = make_translation_stubs.main(["--lang", "de", "--out", str(tmp_path)])
        assert code == 0
        stub = json.loads((tmp_path / "de.json").read_text(encoding="utf-8"))
        assert stub["meta"]["code"] == "de"
        assert stub["meta"]["name"] == "Deutsch"
        assert stub["meta"]["sourceHashes"]["spec"]
        refused = make_translation_stubs.main(["--lang", "de", "--out", str(ROOT)])
        assert refused == 2

        partial = {
            "meta": {"code": "de", "name": "Deutsch", "version": 1, "source": "en"},
            "spec": {"PLAY": "Spielen"},
            "str": {},
        }
        partial_path = tmp_path / "partial.json"
        partial_path.write_text(json.dumps(partial), encoding="utf-8")
        # A tiny English file so merge does not need the whole game table.
        tiny_en = {
            "meta": {"code": "en", "name": "English", "version": 1, "source": "en"},
            "spec": {"Zebra": "Zebra", "PLAY": "PLAY"},
            "str": {"b": "Bee", "a": "Ant"},
            "doNotTranslate": ["×", "▶"],
        }
        en_path = tmp_path / "en.json"
        en_path.write_text(json.dumps(tiny_en), encoding="utf-8")
        out = tmp_path / "merged.json"
        assert merge_table.main(["--lang", "de", "--partial", str(partial_path), "--en", str(en_path), "--out", str(out)]) == 0
        merged = json.loads(out.read_text(encoding="utf-8"))
        assert list(merged["spec"]) == ["PLAY", "Zebra"]
        assert merged["spec"]["PLAY"] == "Spielen"
        assert merged["spec"]["Zebra"] == "Zebra"
        assert list(merged["str"]) == ["a", "b"]
        assert merged["meta"]["sourceHashes"]["spec"]["PLAY"] == kit.source_hash("spec", "PLAY", "PLAY")
        assert merged["doNotTranslate"] == ["×", "▶"] or merged["doNotTranslate"] == sorted(["×", "▶"], key=kit.ordinal_bytes)

        sample_out = tmp_path / "sample.md"
        assert (
            review_sample.main(
                ["--lang", "de", "--table", str(out), "--en", str(en_path), "--key-index", str(tmp_path / "missing.csv"), "--out", str(sample_out), "--limit", "30"]
            )
            == 0
        )
        sample = sample_out.read_text(encoding="utf-8")
        assert "Spielen" in sample
        assert "Ratio" in sample


def test_real_german_stub():
    with tempfile.TemporaryDirectory() as tmp:
        tmp_path = Path(tmp)
        assert make_translation_stubs.main(["--lang", "de", "--out", str(tmp_path)]) == 0
        proc = subprocess.run(
            [
                sys.executable,
                str(ROOT / "validate_table.py"),
                "--lang",
                "de",
                "--table",
                str(tmp_path / "de.json"),
                "--no-glyph",
            ],
            check=False,
            capture_output=True,
            text=True,
        )
        assert proc.returncode == 1, proc.stdout + proc.stderr
        report = proc.stdout
        untranslated_keys = [
            line.split("\t")[2]
            for line in report.splitlines()
            if "\tuntranslated\t" in "\t" + line or line.startswith("error\tuntranslated\t")
        ]
        assert "spec:105 kg" not in untranslated_keys
        assert "spec:ORBITAL" not in untranslated_keys
        assert any(key.startswith("spec:") for key in untranslated_keys)
        print("--- de stub (summary) ---")
        for line in report.splitlines():
            if line.startswith("lang:") or line.startswith("errors:") or line.startswith("warnings:") or line.startswith("by-kind:"):
                print(line)
        print(f"untranslated findings: {len(untranslated_keys)}")


def test_unit_token_not_substring():
    english = make(
        "en",
        {
            "Melody": "Background space synth melody",
            "Mass": "105 kg",
            "Slash": "Ready",
            "Plus": "Ready",
            "Quota": "0 / 3",
            "Bonus": "+100",
        },
    )
    german = make(
        "de",
        {
            "Melody": "Hintergrund-Melodie",
            "Mass": "105 kg im Background",
            "Slash": "und/oder",
            "Plus": "und+oder",
            "Quota": "0 / 3",
            "Bonus": "+100",
        },
    )
    findings = check(english, german, "de")
    assert not has(findings, "error", "unit", "spec:Melody"), findings
    assert not has(findings, "error", "unit", "spec:Mass"), findings
    assert not has(findings, "error", "unit", "spec:Slash"), findings
    assert not has(findings, "error", "unit", "spec:Plus"), findings

    dropped = make(
        "de",
        {
            "Melody": "Hintergrund-Melodie",
            "Mass": "105",
            "Slash": "und/oder",
            "Plus": "und+oder",
            "Quota": "0  3",
            "Bonus": "100",
        },
    )
    bad = check(english, dropped, "de")
    assert has(bad, "error", "unit", "spec:Mass"), bad
    assert has(bad, "error", "unit", "spec:Quota"), bad
    assert has(bad, "error", "unit", "spec:Bonus"), bad


def test_placeholder_unit_copy():
    for sample in ("{0} kg", "{0}kg", "{0}\u00a0kg", "{0} / {1} kg"):
        assert kit.is_unit_like(sample), sample
        assert kit.allowed_identical(sample, set()), sample
    assert not kit.is_unit_like("Goal: {0} kg")
    assert not kit.is_unit_like("Background")

    english = make("en", {}, {"unit.kg": "{0} kg", "unit.quota": "{0} / {1} kg"})
    copied = make("de", {}, {"unit.kg": "{0} kg", "unit.quota": "{0} / {1} kg"})
    same = check(english, copied, "de")
    assert not has(same, "error", "untranslated", "str:unit.kg"), same
    assert not has(same, "error", "untranslated", "str:unit.quota"), same
    assert not kinds(same, "unit")

    spaced = make("de", {}, {"unit.kg": "{0}\u00a0kg", "unit.quota": "{0} / {1}\u00a0kg"})
    nobreak = check(english, spaced, "de")
    assert not kinds(nobreak, "unit"), nobreak
    assert not has(nobreak, "error", "untranslated", "str:unit.kg")

    tight = make("de", {}, {"unit.kg": "{0}kg", "unit.quota": "{0}/{1} kg"})
    packed = check(english, tight, "de")
    assert not kinds(packed, "unit"), packed

    dropped = make("de", {}, {"unit.kg": "{0}", "unit.quota": "{0} / {1}"})
    gone = check(english, dropped, "de")
    assert has(gone, "error", "unit", "str:unit.kg"), gone
    assert has(gone, "error", "unit", "str:unit.quota"), gone


def test_glossary_brand_and_aliases():
    glossary = [
        {
            "term": "space-trash",
            "en": "space trash",
            "dnt": False,
            "check": True,
            "alternates": ["space trash"],
            "de": "Weltraummüll",
        },
        {
            "term": "space-trash-clean",
            "en": "SPACE TRASH CLEAN",
            "dnt": True,
            "check": True,
            "alternates": ["SPACE TRASH CLEAN"],
            "de": "SPACE TRASH CLEAN",
        },
        {
            "term": "level",
            "en": "level/levels",
            "dnt": False,
            "check": True,
            "alternates": ["level", "levels"],
            "de": "Level",
        },
    ]
    aliases = [
        {"alias": "LVL", "term": "level"},
        {"alias": "space junk", "term": "space-trash"},
    ]
    english = make(
        "en",
        {
            "Logo": "SPACE TRASH CLEAN",
            "Body": "Eat space trash",
            "Junk": "Feed space junk",
            "Chip": "LVL 10",
            "Kept": "LVL 2",
            "Footer": "Eat space trash. SPACE TRASH CLEAN",
        },
    )
    german = make(
        "de",
        {
            "Logo": "WELTRAUM",
            "Body": "Essen",
            "Junk": "Futter",
            "Chip": "Stufe 10",
            "Kept": "Level 2",
            "Footer": "Weltraummüll. SPACE TRASH CLEAN",
        },
    )
    findings = check(english, german, "de", glossary=glossary, aliases=aliases)
    logo = [item for item in findings if item.key == "spec:Logo" and item.kind == "glossary"]
    assert logo, findings
    assert all("Weltraummüll" not in item.message for item in logo), logo
    assert any("SPACE TRASH CLEAN" in item.message for item in logo), logo
    assert has(findings, "warning", "glossary", "spec:Body"), findings
    assert has(findings, "warning", "glossary", "spec:Junk"), findings
    assert has(findings, "warning", "glossary", "spec:Chip"), findings
    assert not has(findings, "warning", "glossary", "spec:Kept"), findings
    assert not has(findings, "warning", "glossary", "spec:Footer"), findings


def test_fragment_paren_balance():
    english = make(
        "en",
        {
            "Unlock": "Unlock (1200",
            "Trophy": "Trophy: Daily Duty Star (+500",
            "Ok": "(ready)",
        },
    )
    matched = make(
        "de",
        {
            "Unlock": "Öffnen (1200",
            "Trophy": "Trophäe: Tagesstern (+500",
            "Ok": "(bereit)",
        },
    )
    good = check(english, matched, "de")
    assert not kinds(good, "balance"), good

    closed = make(
        "de",
        {
            "Unlock": "Öffnen (1200)",
            "Trophy": "Trophäe: Tagesstern (+500)",
            "Ok": "bereit",
        },
    )
    bad = check(english, closed, "de")
    assert has(bad, "warning", "balance", "spec:Unlock"), bad
    assert has(bad, "warning", "balance", "spec:Trophy"), bad
    assert has(bad, "warning", "balance", "spec:Ok"), bad


def test_fit_exceptions_suppressed():
    index = {
        ("spec", "LVL 1"): {"fz": "16px", "uppercase": "none", "max_width": "10", "key": "LVL 1"},
        ("spec", "Wide"): {"fz": "16px", "uppercase": "none", "max_width": "10", "key": "Wide"},
    }
    english = make("en", {"LVL 1": "Name", "Wide": "Name"})
    german = make("de", {"LVL 1": "Namen", "Wide": "Namen"})
    reason = "Glossary keeps Level. The LVL chip is narrower than the word."
    findings = check(english, german, "de", index=index, fit_exceptions={"spec:LVL 1": reason})
    assert has(findings, "suppressed", "fit", "spec:LVL 1"), findings
    assert not has(findings, "warning", "fit", "spec:LVL 1"), findings
    assert has(findings, "warning", "fit", "spec:Wide"), findings
    suppressed = next(item for item in findings if item.key == "spec:LVL 1" and item.kind == "fit")
    assert suppressed.reason == reason

    bare = check(english, german, "de", index=index, fit_exceptions={"LVL 1": reason})
    assert has(bare, "suppressed", "fit", "spec:LVL 1"), bare

    huge = make("de", {"LVL 1": "Namen" * 12, "Wide": "Namen"})
    forced = check(english, huge, "de", index=index, fit_exceptions={"spec:LVL 1": reason})
    assert has(forced, "error", "fit", "spec:LVL 1"), forced
    assert not has(forced, "suppressed", "fit", "spec:LVL 1"), forced


def test_backup_pod_glossary_note():
    blob = kit.DEFAULT_GLOSSARY.read_text(encoding="utf-8")
    assert "Auxiliary Scrap Pod" in blob
    assert "Backup Pod" in blob
    assert "two names" in blob
    aliases = kit.parse_aliases(kit.DEFAULT_GLOSSARY)
    assert {"alias": "LVL", "term": "level"} in aliases
    assert {"alias": "space junk", "term": "space-trash"} in aliases
    assert {"alias": "Auxiliary Scrap Pod", "term": "backup-pod"} in aliases
    terms = kit.parse_glossary(kit.DEFAULT_GLOSSARY)
    english = make(
        "en",
        {"Launch": "Launch Backup Pod (+8 Scrap!)"},
        {"toast.backup.deployed": "Auxiliary Scrap Pod Deployed! (+8 Scrap)"},
    )
    split = make(
        "de",
        {"Launch": "Reservekapsel (+8 Schrott)"},
        {"toast.backup.deployed": "Hilfskapsel gestartet! (+8 Schrott)"},
    )
    warned = check(english, split, "de", glossary=terms, aliases=aliases)
    assert has(warned, "warning", "glossary", "str:toast.backup.deployed"), warned
    assert not has(warned, "warning", "glossary", "spec:Launch"), warned
    one_word = make(
        "de",
        {"Launch": "Reservekapsel (+8 Schrott)"},
        {"toast.backup.deployed": "Reservekapsel gestartet! (+8 Schrott)"},
    )
    quiet = check(english, one_word, "de", glossary=terms, aliases=aliases)
    assert not kinds(quiet, "glossary"), quiet


def test_loanword_identical_copy():
    glossary = [
        {
            "term": "donut",
            "en": "donut",
            "dnt": False,
            "check": False,
            "alternates": ["donut"],
            "de": "Donut",
        },
        {
            "term": "pause",
            "en": "pause",
            "dnt": False,
            "check": False,
            "alternates": ["pause"],
            "de": "Pause",
        },
        {
            "term": "bronze",
            "en": "bronze",
            "dnt": False,
            "check": False,
            "alternates": ["bronze"],
            "de": "Bronze",
        },
    ]
    english = make("en", {"Donut": "🍩 DONUT 🍩", "Pause": "Pause", "Bronze": "Bronze", "Hello": "Hello"})
    copied = make("de", {"Donut": "🍩 DONUT 🍩", "Pause": "Pause", "Bronze": "Bronze", "Hello": "Hello"})
    findings = check(english, copied, "de", glossary=glossary)
    assert not has(findings, "error", "untranslated", "spec:Donut"), findings
    assert not has(findings, "error", "untranslated", "spec:Pause"), findings
    assert not has(findings, "error", "untranslated", "spec:Bronze"), findings
    assert has(findings, "error", "untranslated", "spec:Hello"), findings


def test_register_check_skips_style():
    english = make("en", {"Close": "Close", "Play": "Play"})
    german = make("de", {"Close": "Schließen", "Play": "Spiel"})
    off = check(english, german, "de")
    assert not kinds(off, "register"), off
    assert not any(item.kind == "style" for item in off), off
    on = check(english, german, "de", register_check=True)
    assert has(on, "warning", "register", "table"), on
    assert not any(item.kind == "style" for item in on), on
    assert not any(item.key == "spec:Close" and item.kind == "register" for item in on), on


def test_identical_ok_and_merge_dnt():
    english = make("en", {"Hangar": "Hangar", "Reset": "Reset", "Hello": "Hello"}, do_not=["×"])
    copied = make("de", {"Hangar": "Hangar", "Reset": "Reset", "Hello": "Hello"}, do_not=["×"])
    blocked = check(english, copied, "de")
    assert has(blocked, "error", "untranslated", "spec:Hangar"), blocked
    assert has(blocked, "error", "untranslated", "spec:Reset"), blocked
    assert has(blocked, "error", "untranslated", "spec:Hello"), blocked

    waived = check(
        english,
        copied,
        "de",
        identical_ok={"Hangar": "German loanword", "spec:Reset": "common German UI word"},
    )
    assert not has(waived, "error", "untranslated", "spec:Hangar"), waived
    assert not has(waived, "error", "untranslated", "spec:Reset"), waived
    assert has(waived, "error", "untranslated", "spec:Hello"), waived
    assert has(waived, "accepted", "identical", "spec:Hangar"), waived
    assert has(waived, "accepted", "identical", "spec:Reset"), waived
    summary = kit.summarise(waived)
    assert "accepted identical:" in summary
    assert "Hangar" in summary and "Reset" in summary

    with tempfile.TemporaryDirectory() as tmp:
        tmp_path = Path(tmp)
        tiny_en = {
            "meta": {"code": "en", "name": "English", "version": 1, "source": "en"},
            "spec": {"Hangar": "Hangar", "Hello": "Hello"},
            "str": {},
            "doNotTranslate": ["×", "▶"],
        }
        partial = {
            "meta": {"code": "de", "name": "Deutsch", "version": 1, "source": "en"},
            "spec": {"Hangar": "Hangar", "Hello": "Hallo"},
            "str": {},
            "doNotTranslate": ["×", "Hangar", "Reset"],
        }
        into = {
            "meta": {"code": "de", "name": "Deutsch", "version": 1, "source": "en"},
            "spec": {"Hangar": "Hangar", "Hello": "Hello"},
            "str": {},
            "doNotTranslate": ["×", "Reset"],
        }
        en_path = tmp_path / "en.json"
        partial_path = tmp_path / "partial.json"
        into_path = tmp_path / "into.json"
        out = tmp_path / "merged.json"
        en_path.write_text(json.dumps(tiny_en), encoding="utf-8")
        partial_path.write_text(json.dumps(partial), encoding="utf-8")
        into_path.write_text(json.dumps(into), encoding="utf-8")
        assert (
            merge_table.main(
                [
                    "--lang",
                    "de",
                    "--partial",
                    str(partial_path),
                    "--en",
                    str(en_path),
                    "--into",
                    str(into_path),
                    "--out",
                    str(out),
                ]
            )
            == 0
        )
        merged = json.loads(out.read_text(encoding="utf-8"))
        assert merged["doNotTranslate"] == ["×", "▶"] or merged["doNotTranslate"] == sorted(
            ["×", "▶"], key=kit.ordinal_bytes
        )
        assert "Hangar" not in merged["doNotTranslate"]
        assert "Reset" not in merged["doNotTranslate"]
        assert merged["spec"]["Hangar"] == "Hangar"
        assert merged["spec"]["Hello"] == "Hallo"


def test_real_german_table():
    _en_raw, english = kit.load_json(kit.DEFAULT_EN)
    table_path = kit.DEFAULT_EN.with_name("de.json")
    _raw, german = kit.load_json(table_path)
    fit_path = kit.LOC_DIR / "notes" / "de-fit-exceptions.json"
    identical_path = kit.LOC_DIR / "notes" / "de-identical-ok.json"
    fit = kit.load_fit_exceptions(fit_path)
    identical = kit.load_identical_ok(identical_path)
    assert fit, "suppressed fit list did not load"
    assert "Hangar" in identical and "Reset" in identical, identical
    findings = check(
        english,
        german,
        "de",
        glossary=kit.parse_glossary(kit.DEFAULT_GLOSSARY),
        aliases=kit.parse_aliases(kit.DEFAULT_GLOSSARY),
        index=kit.load_key_index(kit.DEFAULT_INDEX),
        fit_exceptions=fit,
        identical_ok=identical,
    )
    errors = [item for item in findings if item.severity == "error"]
    suppressed = [item for item in findings if item.severity == "suppressed"]
    accepted = [item for item in findings if item.severity == "accepted"]
    assert not errors, errors[:8]
    assert suppressed, "suppressed fit list did not apply"
    assert accepted, "accepted identical list did not apply"
    assert has(findings, "accepted", "identical", "spec:Hangar"), findings
    assert has(findings, "accepted", "identical", "spec:Reset"), findings
    summary = kit.summarise(findings)
    assert summary.startswith("errors: 0\n")
    assert "suppressed:" in summary
    assert "accepted identical:" in summary


def main() -> int:
    tests = [
        test_csharp_parity,
        test_untranslated_and_allowlist,
        test_emoji_placeholder_empty_keybytes,
        test_script_and_orthography,
        test_numbers_glossary_uppercase_fit_stale,
        test_glyphs_and_pending,
        test_stubs_merge_sample_glossary,
        test_real_german_stub,
        test_unit_token_not_substring,
        test_placeholder_unit_copy,
        test_glossary_brand_and_aliases,
        test_fragment_paren_balance,
        test_fit_exceptions_suppressed,
        test_backup_pod_glossary_note,
        test_loanword_identical_copy,
        test_register_check_skips_style,
        test_identical_ok_and_merge_dnt,
        test_real_german_table,
    ]
    for test in tests:
        test()
        print(f"ok {test.__name__}")
    print(f"ok {len(tests)} groups")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except AssertionError as exc:
        print("FAIL", exc, file=sys.stderr)
        raise
