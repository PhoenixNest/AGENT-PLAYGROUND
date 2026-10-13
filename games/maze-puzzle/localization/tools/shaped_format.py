#!/usr/bin/env python3
"""Compact shaped-run table (OSB1).

Little-endian. One file per language, keyed by FNV-1a 64 of the NFC UTF-8
translation (the template, with ``{0}`` still in it). Glyph ids are valid
only for the font file whose SHA-256 is stored in the header.

Runs are in logical order. Glyphs inside a run are in HarfBuzz order.
``cluster`` is a UTF-16 code-unit index into that same string. ``flags``
bit 0 is ``break_before`` (a line may start at this cluster).

Kinds: 1 glyph, 2 latin, 3 symbol, 4 emoji, 5 placeholder slot.
"""

from __future__ import annotations

import hashlib
import struct
import unicodedata

MAGIC = b"OSB1"
VERSION = 1

KIND_GLYPH = 1
KIND_LATIN = 2
KIND_SYMBOL = 3
KIND_EMOJI = 4
KIND_SLOT = 5

FNV_OFFSET = 14695981039346656037
FNV_PRIME = 1099511628211


def fnv1a64(data: bytes) -> int:
    h = FNV_OFFSET
    for b in data:
        h ^= b
        h = (h * FNV_PRIME) & 0xFFFFFFFFFFFFFFFF
    return h


def key_hash(text: str) -> int:
    nfc = unicodedata.normalize("NFC", text)
    return fnv1a64(nfc.encode("utf-8"))


def utf16_len(text: str) -> int:
    n = 0
    for ch in text:
        n += 2 if ord(ch) > 0xFFFF else 1
    return n


def _i16(value: int, what: str) -> int:
    if value < -32768 or value > 32767:
        raise ValueError(f"{what} {value} does not fit in int16")
    return value


def _u16(value: int, what: str) -> int:
    if value < 0 or value > 65535:
        raise ValueError(f"{what} {value} does not fit in uint16")
    return value


def pack_table(font_name: str, weight: int, upem: int, font_sha256: bytes, entries: list[dict]) -> bytes:
    if len(font_sha256) != 32:
        raise ValueError("font hash must be 32 bytes")
    name = font_name.encode("utf-8")
    if len(name) > 255:
        raise ValueError("font name is too long")
    records = []
    index = []
    for entry in entries:
        blob = _pack_record(entry)
        index.append((key_hash(entry["text"]), blob))
    index.sort(key=lambda item: item[0])
    # Drop duplicate keys (same translation listed twice).
    dedup = []
    seen = set()
    for h, blob in index:
        if h in seen:
            continue
        seen.add(h)
        dedup.append((h, blob))
    count = len(dedup)
    header = bytearray()
    header += MAGIC
    header += struct.pack("<HHH", VERSION, weight, upem)
    header += struct.pack("<B", len(name))
    header += name
    header += font_sha256
    header += struct.pack("<I", count)
    index_bytes = count * (8 + 4 + 4)
    data = bytearray()
    offsets = []
    base = len(header) + index_bytes
    for h, blob in dedup:
        offsets.append((h, base + len(data), len(blob)))
        data += blob
    out = bytearray(header)
    for h, off, size in offsets:
        out += struct.pack("<QII", h, off, size)
    out += data
    return bytes(out)


def _pack_record(entry: dict) -> bytes:
    runs = entry["runs"]
    blob = bytearray()
    blob += struct.pack("<BH", entry["base_level"] & 0xFF, _u16(len(runs), "run count"))
    for run in runs:
        text = run.get("text") or ""
        raw = text.encode("utf-8")
        glyphs = run.get("glyphs") or []
        blob += struct.pack(
            "<BBB",
            run["kind"],
            run["level"] & 0xFF,
            run.get("slot", 0) & 0xFF,
        )
        blob += struct.pack("<H", _u16(len(raw), "text"))
        blob += raw
        blob += struct.pack("<H", _u16(len(glyphs), "glyph count"))
        for g in glyphs:
            blob += struct.pack(
                "<HhhhHB",
                _u16(g["gid"], "gid"),
                _i16(g["adv"], "adv"),
                _i16(g["dx"], "dx"),
                _i16(g["dy"], "dy"),
                _u16(g["cluster"], "cluster"),
                1 if g.get("break_before") else 0,
            )
    return bytes(blob)


def unpack_table(data: bytes) -> dict:
    if data[:4] != MAGIC:
        raise ValueError("not an OSB1 shaped table")
    version, weight, upem = struct.unpack_from("<HHH", data, 4)
    if version != VERSION:
        raise ValueError(f"unsupported shaped table version {version}")
    name_len = data[10]
    name = data[11 : 11 + name_len].decode("utf-8")
    hash_at = 11 + name_len
    font_hash = data[hash_at : hash_at + 32]
    count = struct.unpack_from("<I", data, hash_at + 32)[0]
    index_at = hash_at + 36
    entries = []
    for i in range(count):
        h, off, size = struct.unpack_from("<QII", data, index_at + i * 16)
        rec = data[off : off + size]
        base, run_count = struct.unpack_from("<BH", rec, 0)
        runs = []
        p = 3
        for _ in range(run_count):
            kind, level, slot = struct.unpack_from("<BBB", rec, p)
            p += 3
            text_len = struct.unpack_from("<H", rec, p)[0]
            p += 2
            text = rec[p : p + text_len].decode("utf-8")
            p += text_len
            glyph_count = struct.unpack_from("<H", rec, p)[0]
            p += 2
            glyphs = []
            for _g in range(glyph_count):
                gid, adv, dx, dy, cluster, flags = struct.unpack_from("<HhhhHB", rec, p)
                p += 11
                glyphs.append(
                    {
                        "gid": gid,
                        "adv": adv,
                        "dx": dx,
                        "dy": dy,
                        "cluster": cluster,
                        "break_before": bool(flags & 1),
                    }
                )
            runs.append(
                {
                    "kind": kind,
                    "level": level,
                    "slot": slot,
                    "text": text,
                    "glyphs": glyphs,
                }
            )
        entries.append({"hash": h, "base_level": base, "runs": runs})
    return {
        "version": version,
        "weight": weight,
        "upem": upem,
        "font": name,
        "font_sha256": font_hash,
        "entries": entries,
    }


def sha256_file(path) -> bytes:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 16), b""):
            h.update(chunk)
    return h.digest()
