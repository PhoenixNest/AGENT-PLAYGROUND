import json
import pathlib
import sys

from fontTools.ttLib import TTFont

root = pathlib.Path(__file__).resolve().parent.parent
web = root.parent.parent / "games" / "maze-puzzle" / "web"
fonts = root / "assets" / "fonts"
chain = ["Fredoka", "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"]
nunito_chain = ["Nunito", "NotoSansSymbols2-Regular", "NotoSansSymbols-Regular"]

index = json.loads((root / "assets" / "emoji" / "index.json").read_text(encoding="utf-8"))
emoji_cps = {ord(c) for g in index for c in g}

used = set()
for p in [web / "index.html", web / "main.js", web / "styles.css", *sorted((web / "engine").glob("*.js"))]:
    used |= {ord(c) for c in p.read_text(encoding="utf-8") if ord(c) > 0x7F}
used -= emoji_cps
required = {0x2190, 0xD7, 0x2713, 0x2022, 0x25B6, 0x2014, 0x2019} | set(range(0x20, 0x7F))
check = sorted(used | required)


def cmap(name):
    return TTFont(fonts / f"{name}.ttf").getBestCmap()


def resolve(base_weight_files, fallbacks):
    result = {}
    maps = [cmap(base_weight_files)] + [cmap(f) for f in fallbacks]
    names = [base_weight_files] + fallbacks
    for cp in check:
        result[cp] = next((n for n, m in zip(names, maps) if cp in m), None)
    return result


failed = False
for family, weights in (("Fredoka", [400, 600, 700, 800, 900]), ("Nunito", [400, 700, 800, 900])):
    for w in weights:
        r = resolve(f"{family}-{w}", chain[1:])
        uncovered = [cp for cp, n in r.items() if n is None]
        own_missing = [f"U+{cp:04X}" for cp, n in r.items() if n != f"{family}-{w}"]
        print(f"{family}-{w}: not in primary = {own_missing}; uncovered by chain = {[f'U+{c:04X}' for c in uncovered]}")
        failed |= bool(uncovered)

print("non-ASCII code points checked:", ", ".join(f"U+{cp:04X}" for cp in check if cp > 0x7F))
sys.exit(1 if failed else 0)
