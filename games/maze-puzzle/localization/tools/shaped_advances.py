"""Width estimate for ar/hi/km from the pinned static instances.

This is an hmtx sum of the cmap glyphs, not a HarfBuzz shape. Joined Arabic and
conjuncts are estimated wide. Latin self-tests keep the em-square guess when
the font file is missing.
"""

from pathlib import Path

from fontTools.ttLib import TTFont

HERE = Path(__file__).resolve().parent
FONTS = HERE.parents[1] / "unity" / "maze-puzzle" / "Assets" / "Fonts"
FACES = {
    "ar": "Cairo-700.ttf",
    "hi": "Baloo2-700.ttf",
    "km": "KantumruyPro-700.ttf",
}
_cache = {}


def _face(lang: str):
    if lang in _cache:
        return _cache[lang]
    path = FONTS / FACES.get(lang, "")
    if not path.is_file():
        _cache[lang] = None
        return None
    font = TTFont(path)
    cmap = font.getBestCmap() or {}
    order = font.getGlyphOrder()
    hmtx = font["hmtx"].metrics
    advances = {}
    for cp, gid in cmap.items():
        if gid < 0 or gid >= len(order):
            continue
        advances[cp] = hmtx[order[gid]][0]
    _cache[lang] = (font["head"].unitsPerEm, advances)
    return _cache[lang]


def estimate(text: str, lang: str, font_px: float):
    loaded = _face(lang)
    if loaded is None or font_px <= 0:
        return None
    upem, advances = loaded
    if upem <= 0:
        return None
    total = 0
    for ch in text:
        total += advances.get(ord(ch), upem // 2)
    return font_px * total / upem
