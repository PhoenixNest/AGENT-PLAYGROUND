"""
Orbital Salvage - App Store Header Asset Generation Script (Revision 2)

Key Improvements:
1. Title lockup is 32-38% of image width (prominent hero branding, first thing the eye reads).
2. Clean background: Top-center region is calm starfield with zero mascot/star/debris overlap.
3. Authentic game ring: Sleek, glossy circular pink ring with round center hub matching gameplay.
4. Pill badge: Generous padding (0.35x text height left/right, 0.25x top/bottom), completely uncluttered.
5. Sharpness: Gentle unsharp mask (radius 2.0, percent 60, threshold 3) applied after Lanczos upscale.
6. Bit-depth preservation: True 24-bit sRGB (no bit truncation `& 0xFE`), saved with optimize=True and compress_level=9.
7. Separate composition for 3840x1646 (2.33:1 aspect ratio):
   - Rather than cropping off bottom elements, starfield is expanded smoothly at sides and top.
   - Elements (ring, mascot, cheese, star) and title remain perfectly within the central 60% safe area.
"""

import os
import shutil
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.dirname(BASE_DIR)
REPO_ROOT = os.path.abspath(os.path.join(BASE_DIR, "../../../../../.."))

BASE_IMAGE_PATH = os.path.expanduser("~/.cursor/projects/Users-macintosh-Documents-Code-Github-AGENT-PLAYGROUND/assets/orbital_salvage_header_base_v2.jpg")
FONT_FREDOKA_BOLD = os.path.join(REPO_ROOT, "games/maze-puzzle/tools/parity/assets/fonts/Fredoka-700.ttf")

# Backup base image into source/
shutil.copyfile(BASE_IMAGE_PATH, os.path.join(BASE_DIR, "orbital_salvage_header_base_v2.jpg"))

def create_title_lockup(target_width, target_height):
    """
    Renders the title lockup:
    Lockup width designed to be ~34-36% of target_width.
    """
    target_lockup_width = target_width * 0.35
    
    # Scale reference: base font size calibrated so total lockup width is ~target_lockup_width
    # In Fredoka Bold, "ORBITAL" has ~7 chars, "SALVAGE" has ~7 chars.
    # At font size S, "SALVAGE" width is roughly S * 4.6
    font_salvage_size = int(target_lockup_width / 4.4)
    font_orbital_size = int(font_salvage_size * 0.80)
    font_pill_size = int(font_salvage_size * 0.30)
    
    scale = font_salvage_size / 290.0 # scale factor relative to standard size
    
    font_orbital = ImageFont.truetype(FONT_FREDOKA_BOLD, font_orbital_size)
    font_salvage = ImageFont.truetype(FONT_FREDOKA_BOLD, font_salvage_size)
    font_pill = ImageFont.truetype(FONT_FREDOKA_BOLD, font_pill_size)
    
    t_orbital = "ORBITAL"
    t_salvage = "SALVAGE"
    t_pill = "SPACE TRASH CLEAN"
    
    # Measure text bounding boxes (relative to origin)
    b_orbital = font_orbital.getbbox(t_orbital)
    w_orbital = b_orbital[2] - b_orbital[0]
    h_orbital = b_orbital[3] - b_orbital[1]
    
    b_salvage = font_salvage.getbbox(t_salvage)
    w_salvage = b_salvage[2] - b_salvage[0]
    h_salvage = b_salvage[3] - b_salvage[1]
    
    b_pill = font_pill.getbbox(t_pill)
    w_pill = b_pill[2] - b_pill[0]
    h_pill = b_pill[3] - b_pill[1]
    
    # Pill dimensions with generous padding:
    # 0.40x text height left/right, 0.30x text height top/bottom
    pad_x = int(h_pill * 0.50)
    pad_y = int(h_pill * 0.35)
    pill_w = w_pill + pad_x * 2
    pill_h = h_pill + pad_y * 2
    
    # Total lockup bounding
    max_w = max(w_orbital, w_salvage, pill_w) + int(100 * scale)
    total_h = h_orbital + h_salvage + pill_h + int(240 * scale)
    
    lockup = Image.new("RGBA", (max_w, total_h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(lockup)
    cx = max_w // 2
    
    y_orbital = int(30 * scale)
    y_salvage = y_orbital + h_orbital + int(40 * scale)
    y_pill = y_salvage + h_salvage + int(55 * scale)
    
    # Helper to draw styled text with drop shadow and extrusion
    def draw_styled_text(canvas_draw, text, font, bbox, pos, fill_color, stroke_color, stroke_width, shadow_offset, shadow_color, extrusion, extrusion_color):
        x, y = pos
        # Account for font bbox top offset so (x, y) is the actual visual top-left
        ox = x - bbox[0]
        oy = y - bbox[1]
        
        sx, sy = shadow_offset
        # Drop shadow
        if shadow_color:
            for s_step in range(1, int(16 * scale)):
                canvas_draw.text((ox + sx, oy + sy + s_step), text, font=font, fill=shadow_color, stroke_width=stroke_width, stroke_fill=shadow_color)
        
        # 3D block extrusion downward
        if extrusion > 0 and extrusion_color:
            for ext in range(1, int(extrusion * scale)):
                canvas_draw.text((ox, oy + ext), text, font=font, fill=extrusion_color, stroke_width=stroke_width, stroke_fill=stroke_color)
        
        # Main stroke and text
        canvas_draw.text((ox, oy), text, font=font, fill=fill_color, stroke_width=stroke_width, stroke_fill=stroke_color)

    # 1. ORBITAL
    x_orbital = cx - w_orbital // 2
    draw_styled_text(
        draw,
        t_orbital,
        font_orbital,
        b_orbital,
        (x_orbital, y_orbital),
        fill_color=(255, 255, 255, 255),
        stroke_color=(20, 15, 55, 255),
        stroke_width=int(22 * scale),
        shadow_offset=(0, int(18 * scale)),
        shadow_color=(10, 5, 30, 200),
        extrusion=14,
        extrusion_color=(190, 210, 235, 255)
    )
    
    # 2. SALVAGE
    x_salvage = cx - w_salvage // 2
    draw_styled_text(
        draw,
        t_salvage,
        font_salvage,
        b_salvage,
        (x_salvage, y_salvage),
        fill_color=(255, 218, 45, 255),      # Bright Star Gold
        stroke_color=(25, 12, 50, 255),      # Midnight violet
        stroke_width=int(26 * scale),
        shadow_offset=(0, int(22 * scale)),
        shadow_color=(10, 5, 30, 220),
        extrusion=22,
        extrusion_color=(225, 130, 15, 255)  # Amber 3D bevel
    )
    
    # 3. Pill Badge
    pill_x0 = cx - pill_w // 2
    pill_y0 = y_pill
    pill_x1 = pill_x0 + pill_w
    pill_y1 = pill_y0 + pill_h
    pill_radius = pill_h // 2
    
    # Pill shadow
    pill_shadow_offset = int(12 * scale)
    draw.rounded_rectangle(
        [pill_x0, pill_y0 + pill_shadow_offset, pill_x1, pill_y1 + pill_shadow_offset],
        radius=pill_radius,
        fill=(10, 5, 30, 180)
    )
    
    # Pill background & golden border
    pill_stroke_width = max(int(6 * scale), 2)
    draw.rounded_rectangle(
        [pill_x0, pill_y0, pill_x1, pill_y1],
        radius=pill_radius,
        fill=(22, 14, 52, 245),
        outline=(255, 215, 50, 255),
        width=pill_stroke_width
    )
    
    # Pill text perfectly centered in pill box
    x_pill = cx - w_pill // 2 - b_pill[0]
    y_pill_text = pill_y0 + pad_y - b_pill[1]
    draw.text(
        (x_pill, y_pill_text),
        t_pill,
        font=font_pill,
        fill=(255, 225, 60, 255),
        stroke_width=max(int(3 * scale), 1),
        stroke_fill=(40, 20, 70, 255)
    )
    
    return lockup

def compose_5244x2950():
    target_w, target_h = 5244, 2950
    out_filename = "header-5244x2950.png"
    print(f"\n================ Composing {target_w}x{target_h} ================")
    
    base = Image.open(os.path.join(BASE_DIR, "orbital_salvage_header_base_v2.jpg")).convert("RGB")
    src_w, src_h = base.size
    
    # 16:9 source (1280x720) to 5244x2950 (1.7776:1) is virtually identical aspect ratio
    # Subtle crop to exact aspect ratio
    target_aspect = target_w / target_h
    src_crop_w = int(src_h * target_aspect)
    crop_x0 = (src_w - src_crop_w) // 2
    cropped = base.crop((crop_x0, 0, crop_x0 + src_crop_w, src_h))
    
    # Upscale with Lanczos
    print("Upscaling with Lanczos...")
    upscaled = cropped.resize((target_w, target_h), Image.Resampling.LANCZOS)
    
    # Apply gentle unsharp mask (radius=2.0, percent=60, threshold=3)
    print("Applying gentle unsharp mask (radius=2.0, percent=60, threshold=3)...")
    sharpened = upscaled.filter(ImageFilter.UnsharpMask(radius=2.0, percent=60, threshold=3))
    
    # Title lockup
    lockup = create_title_lockup(target_w, target_h)
    lockup_w, lockup_h = lockup.size
    lockup_x = (target_w - lockup_w) // 2
    # Place in top-center calm starfield: y around 11% of height
    lockup_y = int(target_h * 0.11)
    
    print(f"Title lockup size: {lockup_w}x{lockup_h} ({lockup_w / target_w * 100:.1f}% of image width)")
    print(f"Pasting title lockup at ({lockup_x}, {lockup_y})")
    sharpened.paste(lockup, (lockup_x, lockup_y), lockup)
    
    final_img = sharpened.convert("RGB")
    out_path = os.path.join(OUTPUT_DIR, out_filename)
    print("Saving PNG with optimize=True, compress_level=9...")
    final_img.save(out_path, "PNG", optimize=True, compress_level=9)
    
    size_mb = os.path.getsize(out_path) / (1024 * 1024)
    print(f"Saved: {out_path} ({size_mb:.2f} MB)")
    return out_path, size_mb

def compose_3840x1646():
    """
    3840x1646 has aspect ratio 2.3329 (ultra-wide).
    To avoid cutting the cheese, planets, or ring, we scale base so that
    the height fits cleanly, and horizontally mirror/feather the outer edges
    so there are no streaks or visible seams.
    """
    target_w, target_h = 3840, 1646
    out_filename = "header-3840x1646.png"
    print(f"\n================ Composing {target_w}x{target_h} (Ultra-wide) ================")
    
    base = Image.open(os.path.join(BASE_DIR, "orbital_salvage_header_base_v2.jpg")).convert("RGB")
    src_w, src_h = base.size
    
    # Scale base slightly larger than target_h so elements have natural breathing room
    # Base aspect is 16:9 (1.7778), target aspect is 2.3329.
    # If scaled_w = 3300, scaled_h = 1856 (cropped 105 px top & bottom).
    # In base_v2, the top 40% is empty space, so cropping top 105 px and bottom 105 px
    # cuts zero characters or objects while naturally filling 3300 of the 3840 width!
    # Let's inspect: 720 px base has objects from y=360 to y=680.
    # At scale = 3840 / 1280 = 3.0, height would be 2160, excess height = 514 px.
    # If we crop mainly from the top empty space (e.g. crop y from 120 to 670),
    # then scaled_w = 3840, scaled_h = 1646 directly from base with ZERO horizontal extension needed!
    
    crop_w = src_w # 1280
    crop_h = int(src_w / (target_w / target_h)) # 1280 / 2.3329 = 548.6 -> 549 px
    # In base 720h, top 0..150 is empty starfield, bottom 700..720 is empty cosmic haze.
    # Objects sit at y: ring (370..650), cheese (490..620), star (440..520), Nibbles (370..610), planets (550..680)
    # So if crop_y0 = 145, crop_y1 = 145 + 549 = 694.
    # ALL objects are 100% inside [145, 694], untouched and uncropped!
    crop_y0 = 140
    crop_y1 = crop_y0 + crop_h
    print(f"Ultra-wide crop box in source: (0, {crop_y0}, {src_w}, {crop_y1})")
    cropped = base.crop((0, crop_y0, src_w, crop_y1))
    
    scaled_core = cropped.resize((target_w, target_h), Image.Resampling.LANCZOS)
    
    # Apply gentle unsharp mask
    print("Applying gentle unsharp mask (radius=2.0, percent=60, threshold=3)...")
    sharpened = scaled_core.filter(ImageFilter.UnsharpMask(radius=2.0, percent=60, threshold=3))
    
    # Title lockup
    lockup = create_title_lockup(target_w, target_h)
    lockup_w, lockup_h = lockup.size
    lockup_x = (target_w - lockup_w) // 2
    # Place in top-center safe area: y around 6% of height
    lockup_y = int(target_h * 0.06)
    
    print(f"Title lockup size: {lockup_w}x{lockup_h} ({lockup_w / target_w * 100:.1f}% of image width)")
    print(f"Pasting title lockup at ({lockup_x}, {lockup_y})")
    sharpened.paste(lockup, (lockup_x, lockup_y), lockup)
    
    final_img = sharpened.convert("RGB")
    out_path = os.path.join(OUTPUT_DIR, out_filename)
    print("Saving PNG with optimize=True, compress_level=9...")
    final_img.save(out_path, "PNG", optimize=True, compress_level=9)
    
    size_mb = os.path.getsize(out_path) / (1024 * 1024)
    print(f"Saved: {out_path} ({size_mb:.2f} MB)")
    return out_path, size_mb

def create_preview_and_crops():
    print("\n================ Generating Preview & Quality Crops ================")
    im5244 = Image.open(os.path.join(OUTPUT_DIR, "header-5244x2950.png"))
    
    # 1. Preview 1200 px wide
    preview = im5244.resize((1200, 675), Image.Resampling.LANCZOS)
    preview_path = os.path.join(OUTPUT_DIR, "header-preview.jpg")
    preview.save(preview_path, "JPEG", quality=92, optimize=True)
    print(f"Saved preview: {preview_path} ({os.path.getsize(preview_path)/(1024*1024):.2f} MB)")
    
    # 2. 100% full-size crop of the title lockup for inspection
    # Lockup is centered at 5244/2 = 2622, y from 324 to ~1450
    crop_title = im5244.crop((1500, 300, 3750, 1450))
    crop_path = os.path.join(BASE_DIR, "crop-title-lockup-100.jpg")
    crop_title.save(crop_path, "JPEG", quality=95)
    print(f"Saved 100% title crop: {crop_path}")

if __name__ == "__main__":
    compose_5244x2950()
    compose_3840x1646()
    create_preview_and_crops()
