#!/usr/bin/env python3
"""Build App Store Connect screenshots for Orbital Salvage.

Portrait sets for the 6.3" slot (1206x2622) and the 6.1" slot (1179x2556).
Each source is produced as a captioned poster and a plain poster.

Re-run from this folder:
    python3 build_screenshots.py

Pillow is required. If it is missing:
    python3 -m venv /tmp/maze-infinite/shots-venv
    /tmp/maze-infinite/shots-venv/bin/pip install pillow
    /tmp/maze-infinite/shots-venv/bin/python3 build_screenshots.py
"""

from __future__ import annotations

import math
import random
from collections import deque
from pathlib import Path

from PIL import Image, ImageCms, ImageDraw, ImageFont

HERE = Path(__file__).resolve().parent
GAME = HERE.parents[2]
SOURCE_DIR = GAME / "web" / "screenshots"
FONT_PATH = GAME / "unity" / "maze-puzzle" / "Assets" / "Fonts" / "Fredoka-700.ttf"
FONT_FALLBACK = Path("/System/Library/Fonts/Supplemental/Arial Rounded Bold.ttf")

# Phone frame should land near this fraction of the canvas width.
# The source frame is only 860 px wide (68.7% of the 1252 px file), so
# 94% of a 1206 px canvas is an upscale. That upscale is required to hit
# the 92–96% width; the script prints the exact factor.
TARGET_WIDTH_FRAC = 0.94

# Background navy sampled from the source edges (flat 15,12,41) with the
# slightly darker bottom-centre margin (10,8,27) and the faint purple
# glow just outside the bezel (about 28,23,78).
GRADIENT_TOP = (26, 20, 68)
GRADIENT_BOTTOM = (10, 8, 27)
BG_SAMPLE = (15, 12, 41)

WHITE = (255, 255, 255)
GOLD = (255, 210, 30)  # #FFD21E
OUTLINE = (7, 5, 24)

CANVASES = ((1206, 2622), (1179, 2556))

# Approved public wording is "Daily Shift" / "daily shifts" (store manual
# section 3.3 and the Daily Operations screen). "missions" is not on that
# screen, so the daily caption says "shifts".
ITEMS = [
    {
        "source": "01_home_screen.png",
        "name": "01-home.png",
        "lines": [
            [("Spin the ", WHITE), ("ring!", GOLD)],
            [("Clean up ", WHITE), ("space!", GOLD)],
        ],
    },
    {
        "source": "03_daily_operations.png",
        "name": "02-daily.png",
        "lines": [
            [("New ", WHITE), ("daily", GOLD)],
            [("shifts!", WHITE)],
        ],
    },
    {
        "source": "04_toy_shop_donuts.png",
        "name": "03-shop.png",
        "lines": [
            [("Cute ", WHITE), ("rings, ", GOLD), ("hats", WHITE)],
            [("and ", WHITE), ("skies!", GOLD)],
        ],
    },
    {
        "source": "09_gameplay_sector_1.png",
        "name": "04-gameplay.png",
        "lines": [
            [("Swallow the", WHITE)],
            [("space ", GOLD), ("trash!", GOLD)],
        ],
    },
]

LONGEST_LINE = "Cute rings, hats"


def srgb_profile() -> bytes:
    return ImageCms.ImageCmsProfile(ImageCms.createProfile("sRGB")).tobytes()


def load_font(size: int) -> ImageFont.FreeTypeFont:
    path = FONT_PATH if FONT_PATH.is_file() else FONT_FALLBACK
    if not path.is_file():
        raise SystemExit(f"No display font found at {FONT_PATH} or {FONT_FALLBACK}")
    return ImageFont.truetype(str(path), size)


def font_in_use() -> Path:
    return FONT_PATH if FONT_PATH.is_file() else FONT_FALLBACK


def extract_phone(im: Image.Image) -> Image.Image:
    """Cut the framed phone out of the flat navy margin. Hard mask, full UI kept."""
    im = im.convert("RGB")
    width, height = im.size
    raw = im.tobytes()
    threshold = 46
    count = width * height
    outside = bytearray(count)
    queue: deque[int] = deque()

    def consider(index: int) -> None:
        if outside[index]:
            return
        offset = index * 3
        dist = (
            abs(raw[offset] - BG_SAMPLE[0])
            + abs(raw[offset + 1] - BG_SAMPLE[1])
            + abs(raw[offset + 2] - BG_SAMPLE[2])
        )
        if dist <= threshold:
            outside[index] = 1
            queue.append(index)

    for x in range(width):
        consider(x)
        consider((height - 1) * width + x)
    for y in range(height):
        consider(y * width)
        consider(y * width + width - 1)
    while queue:
        index = queue.popleft()
        y, x = divmod(index, width)
        if x > 0:
            consider(index - 1)
        if x + 1 < width:
            consider(index + 1)
        if y > 0:
            consider(index - width)
        if y + 1 < height:
            consider(index + width)

    min_x, min_y, max_x, max_y = width, height, -1, -1
    for index, flag in enumerate(outside):
        if flag:
            continue
        y, x = divmod(index, width)
        if x < min_x:
            min_x = x
        if y < min_y:
            min_y = y
        if x > max_x:
            max_x = x
        if y > max_y:
            max_y = y
    if max_x < 0:
        raise SystemExit("Phone frame was not found in the source image.")

    frame_w = max_x - min_x + 1
    frame_h = max_y - min_y + 1
    if not (820 <= frame_w <= 920 and 1400 <= frame_h <= 1520):
        raise SystemExit(f"Unexpected phone bounds {frame_w}x{frame_h} at {(min_x, min_y)}")

    phone = Image.new("RGBA", (frame_w, frame_h))
    src = im.load()
    dst = phone.load()
    for y in range(frame_h):
        for x in range(frame_w):
            if outside[(y + min_y) * width + (x + min_x)]:
                dst[x, y] = (0, 0, 0, 0)
            else:
                red, green, blue = src[x + min_x, y + min_y]
                dst[x, y] = (red, green, blue, 255)
    return phone


def vertical_gradient(width: int, height: int) -> Image.Image:
    column = Image.new("RGB", (1, height))
    pixels = column.load()
    last = max(1, height - 1)
    for y in range(height):
        blend = y / last
        pixels[0, y] = tuple(
            int(start + (end - start) * blend)
            for start, end in zip(GRADIENT_TOP, GRADIENT_BOTTOM)
        )
    return column.resize((width, height), Image.Resampling.NEAREST)


def draw_sparkle(draw: ImageDraw.ImageDraw, x: float, y: float, radius: float, color: tuple[int, int, int]) -> None:
    points = []
    for step in range(8):
        angle = math.radians(-90 + step * 45)
        arm = radius if step % 2 == 0 else radius * 0.34
        points.append((x + arm * math.cos(angle), y + arm * math.sin(angle)))
    draw.polygon(points, fill=color)


def star_field(
    draw: ImageDraw.ImageDraw,
    canvas_w: int,
    canvas_h: int,
    blocked: list[tuple[int, int, int, int]],
) -> None:
    """A few small sparkles in the source's star style. Not app UI."""
    rng = random.Random(14)
    placed: list[tuple[float, float]] = []
    target = 16
    for _ in range(80):
        if len(placed) >= target:
            break
        radius = rng.choice((3, 4, 4, 5, 6, 7))
        x = rng.uniform(28, canvas_w - 28)
        y = rng.uniform(28, canvas_h - 28)
        if any(left - 8 <= x <= right + 8 and top - 8 <= y <= bottom + 8 for left, top, right, bottom in blocked):
            continue
        if any((x - px) ** 2 + (y - py) ** 2 < 110**2 for px, py in placed):
            continue
        placed.append((x, y))
        kind = rng.randrange(5)
        if kind == 0:
            color = (255, 210, 40)
        elif kind == 1:
            color = (255, 255, 255)
        else:
            color = rng.choice(((186, 196, 230), (150, 160, 205), (210, 216, 245)))
        if kind <= 1 or radius >= 6:
            draw_sparkle(draw, x, y, radius, color)
        else:
            dot = max(1, radius // 2)
            draw.ellipse((x - dot, y - dot, x + dot, y + dot), fill=color)


def fit_font_size(canvas_w: int) -> int:
    pad = max(24, round(32 * canvas_w / 1206))
    low, high, best = 48, 280, 48
    while low <= high:
        mid = (low + high) // 2
        font = load_font(mid)
        stroke = max(4, round(mid * 0.04))
        width = font.getlength(LONGEST_LINE) + 2 * stroke
        if width <= canvas_w - 2 * pad:
            best = mid
            low = mid + 1
        else:
            high = mid - 1
    return best


def caption_metrics(font: ImageFont.FreeTypeFont, lines: list, stroke: int) -> tuple[int, int, list[float]]:
    ascent, descent = font.getmetrics()
    step = ascent + int(descent * 0.35)
    widths = [sum(font.getlength(text) for text, _color in parts) for parts in lines]
    return step, descent, widths


def draw_caption(
    draw: ImageDraw.ImageDraw,
    canvas_w: int,
    font: ImageFont.FreeTypeFont,
    lines: list,
    stroke: int,
    first_baseline: float,
    step: int,
) -> None:
    baseline = first_baseline
    for parts in lines:
        total = sum(font.getlength(text) for text, _color in parts)
        x = (canvas_w - total) / 2
        for text, color in parts:
            draw.text(
                (x, baseline),
                text,
                font=font,
                fill=color,
                stroke_width=stroke,
                stroke_fill=OUTLINE,
                anchor="ls",
            )
            x += font.getlength(text)
        baseline += step


def render(
    phone: Image.Image,
    canvas_w: int,
    canvas_h: int,
    lines: list | None,
) -> Image.Image:
    source_w, source_h = phone.size
    target_w = int(round(canvas_w * TARGET_WIDTH_FRAC))
    scale = target_w / source_w
    new_w = int(round(source_w * scale))
    new_h = int(round(source_h * scale))
    if abs((new_w / new_h) - (source_w / source_h)) > 0.002:
        raise SystemExit("Phone aspect ratio drifted during resize.")
    scaled = phone.resize((new_w, new_h), Image.Resampling.LANCZOS)

    canvas = vertical_gradient(canvas_w, canvas_h)
    phone_x = (canvas_w - new_w) // 2
    if lines is None:
        phone_y = (canvas_h - new_h) // 2
        blocked = [(phone_x, phone_y, phone_x + new_w, phone_y + new_h)]
        draw = ImageDraw.Draw(canvas)
        star_field(draw, canvas_w, canvas_h, blocked)
        canvas.paste(scaled, (phone_x, phone_y), scaled)
        return canvas

    font_size = fit_font_size(canvas_w)
    font = load_font(font_size)
    stroke = max(4, round(font_size * 0.04))
    ascent, descent = font.getmetrics()
    step, _descent, widths = caption_metrics(font, lines, stroke)
    bottom_margin = int(round(canvas_h * 0.042))
    gap = int(round(canvas_h * 0.032))
    phone_y = canvas_h - bottom_margin - new_h
    if phone_y < int(canvas_h * 0.18):
        raise SystemExit("Phone is too tall to leave room for the caption.")

    last_baseline = phone_y - gap - (descent + stroke)
    first_baseline = last_baseline - step * (len(lines) - 1)
    text_top = first_baseline - ascent - stroke
    ink_bottom = last_baseline + descent + stroke
    if text_top < 20:
        raise SystemExit(f"Caption is clipped at the top (y={text_top:.1f}).")
    if phone_y - ink_bottom < 48:
        raise SystemExit("Caption is too close to the phone frame.")
    widest = max(widths) + 2 * stroke
    if widest > canvas_w - 16:
        raise SystemExit(f"Caption is wider than the canvas ({widest:.0f} > {canvas_w}).")

    caption_left = int((canvas_w - widest) / 2) - 8
    caption_right = caption_left + int(widest) + 16
    blocked = [
        (phone_x, phone_y, phone_x + new_w, phone_y + new_h),
        (caption_left, int(text_top) - 8, caption_right, int(ink_bottom) + 8),
    ]
    draw = ImageDraw.Draw(canvas)
    star_field(draw, canvas_w, canvas_h, blocked)
    canvas.paste(scaled, (phone_x, phone_y), scaled)
    draw_caption(draw, canvas_w, font, lines, stroke, first_baseline, step)
    return canvas


def save_rgb(image: Image.Image, path: Path, icc: bytes) -> None:
    flat = image.convert("RGB")
    if flat.mode != "RGB" or "A" in flat.getbands():
        raise SystemExit(f"{path.name} still has an alpha channel.")
    flat.save(path, format="PNG", icc_profile=icc)


def build_contact_sheet(folder: Path, icc: bytes) -> None:
    thumbs = []
    thumb_w = 400
    thumb_h = int(round(2622 * thumb_w / 1206))
    for item in ITEMS:
        image = Image.open(folder / "1206x2622" / "captioned" / item["name"]).convert("RGB")
        thumbs.append(image.resize((thumb_w, thumb_h), Image.Resampling.LANCZOS))
    sheet_w = thumb_w * len(thumbs)
    sheet = Image.new("RGB", (sheet_w, thumb_h))
    for index, thumb in enumerate(thumbs):
        sheet.paste(thumb, (index * thumb_w, 0))
    sheet.save(HERE / "contact-sheet.jpg", format="JPEG", quality=90, icc_profile=icc, optimize=True)


def verify() -> list[tuple[str, str, int]]:
    expected = {
        "1206x2622": (1206, 2622),
        "1179x2556": (1179, 2556),
    }
    rows = []
    for folder, size in expected.items():
        for variant in ("captioned", "plain"):
            for item in ITEMS:
                path = HERE / folder / variant / item["name"]
                if not path.is_file():
                    raise SystemExit(f"Missing {path}")
                image = Image.open(path)
                image.load()
                if image.size != size:
                    raise SystemExit(f"{path.name} is {image.size}, expected {size}")
                if image.mode != "RGB" or image.getbands() != ("R", "G", "B"):
                    raise SystemExit(f"{path.name} mode is {image.mode} bands {image.getbands()}")
                if "transparency" in image.info:
                    raise SystemExit(f"{path.name} has a transparency key")
                rows.append((str(path.relative_to(HERE)), f"{image.size[0]}x{image.size[1]} {image.mode}", path.stat().st_size))
    contact = HERE / "contact-sheet.jpg"
    sheet = Image.open(contact)
    sheet.load()
    if sheet.mode != "RGB" or sheet.size[0] != 1600:
        raise SystemExit(f"Contact sheet is {sheet.size} {sheet.mode}")
    rows.append(("contact-sheet.jpg", f"{sheet.size[0]}x{sheet.size[1]} {sheet.mode}", contact.stat().st_size))
    return rows


def main() -> None:
    if GAME.name != "maze-puzzle":
        raise SystemExit(f"Unexpected game folder: {GAME}")
    icc = srgb_profile()
    phones: dict[str, Image.Image] = {}
    for item in ITEMS:
        source = Image.open(SOURCE_DIR / item["source"])
        if source.size != (1252, 1528) or source.mode != "RGB":
            raise SystemExit(f"{item['source']} is {source.size} {source.mode}, expected 1252x1528 RGB")
        phones[item["source"]] = extract_phone(source)

    frame_w, frame_h = next(iter(phones.values())).size
    print(f"font: {font_in_use()}")
    print(f"phone frame: {frame_w}x{frame_h} px inside 1252x1528 ({frame_w / 1252:.1%} of source width)")
    print(f"target width fraction: {TARGET_WIDTH_FRAC:.0%}")

    for canvas_w, canvas_h in CANVASES:
        folder = f"{canvas_w}x{canvas_h}"
        scale = (canvas_w * TARGET_WIDTH_FRAC) / frame_w
        font_size = fit_font_size(canvas_w)
        font = load_font(font_size)
        cap = font.getbbox("H")[3] - font.getbbox("H")[1]
        print(
            f"{folder}: scale {scale:.4f} -> phone {int(round(frame_w * scale))}x{int(round(frame_h * scale))}"
            f" ({(frame_w * scale) / canvas_w:.1%} of canvas width), "
            f"font {font_size}px, cap height {cap}px ({cap / canvas_h:.1%} of canvas)"
        )
        for variant, with_caption in (("captioned", True), ("plain", False)):
            out_dir = HERE / folder / variant
            out_dir.mkdir(parents=True, exist_ok=True)
            for item in ITEMS:
                image = render(
                    phones[item["source"]],
                    canvas_w,
                    canvas_h,
                    item["lines"] if with_caption else None,
                )
                save_rgb(image, out_dir / item["name"], icc)

    build_contact_sheet(HERE, icc)
    print()
    print(f"{'file':<42} {'size':<22} {'bytes':>10}")
    for name, size, nbytes in verify():
        print(f"{name:<42} {size:<22} {nbytes:10d}")


if __name__ == "__main__":
    main()
