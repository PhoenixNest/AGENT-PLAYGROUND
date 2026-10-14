# Generation Source & Prompt Log (Revision 2)

## Prompt used for Base Art Generation (v2)

- **Tool:** Cursor native `GenerateImage` dynamic tool
- **Aspect Ratio:** 16:9 (Native maximum tool output: 1280 × 720)
- **Reference Images:**
  - `games/maze-puzzle/resources/app-icon/orbital-salvage-icon-1024.png`
  - `games/maze-puzzle/web/screenshots/01_home_screen.png`
  - `games/maze-puzzle/web/screenshots/09_gameplay_sector_1.png`
- **Text Prompt:**
  > "A vibrant wide cartoon cosmic illustration for a kids mobile casual game named Orbital Salvage. Wide 16:9 landscape composition with a clean deep dark purple-navy space background filled with subtle glowing stars and cosmic nebulae.
  >
  > IMPORTANT COMPOSITION CONSTRAINTS:
  >
  > - Leave the entire top-center third of the canvas completely empty of characters, rings, or large objects (only quiet dark starfield) so that a large title logo can sit there without any clutter or overlap.
  > - On the left side: one single clean, glossy pink circular orbital ring (matching the game's pink ring with an open slot/gap and a round center hub, sleek and toy-like, not thick spiral tubes or mechanical clutter).
  > - On the right side: Nibbles the adorable black hole mascot, a smooth dark navy-blue sphere with huge sparkling anime chibi eyes, pink blush cheeks, a happy open smile, and a glowing neon pink accretion ring around his body.
  > - In the lower-middle and between the ring and Nibbles: cute smiling emoji-style space trash pieces floating gently towards Nibbles (a happy little yellow star, a cute smiling wedge of Swiss cheese, a glowing blue gem, colorful orbital particles).
  > - Bottom and side margins: clean cosmic void with small colorful planets in corners.
  > - Bright, friendly, glossy 3D cartoon style (Nintendo / Pixar feel), cheerful, child-safe, no text, no letters, no words."

## Post-Processing and Typography Pipeline (v2)

- **Script:** `games/maze-puzzle/resources/app-store-connect/header/source/generate_headers.py`
- **Upscaling & Sharpening:**
  - Scaled from 1280×720 using Lanczos interpolation (`Image.Resampling.LANCZOS`).
  - Applied gentle unsharp mask (`ImageFilter.UnsharpMask(radius=2.0, percent=60, threshold=3)`), eliminating softness without introducing edge haloing or grain.
  - **No Bit Stripping:** True 24-bit sRGB depth preserved intact (no `& 0xFE` masking). PNG saved strictly with `optimize=True, compress_level=9`.
- **Typography & Title Lockup:**
  - Font: `Fredoka-700.ttf` from `games/maze-puzzle/tools/parity/assets/fonts/Fredoka-700.ttf`.
  - Prominence: Exactly **39.2% of total image width** (prominent hero branding, eye reads first).
  - Placed in top-center calm starfield with **zero overlap** over Nibbles, the pink ring, or floating debris.
  - Title Text:
    - **`ORBITAL`**: Pure crisp white fill, deep midnight violet stroke (`#140f37`), light-blue 3D block extrusion (`#becde7`).
    - **`SALVAGE`**: Vibrant star gold fill (`#ffda2d`), deep midnight violet stroke, warm amber 3D bevel extrusion (`#e1820f`), soft drop shadow.
  - Tagline Pill:
    - Text: **`SPACE TRASH CLEAN`** in star gold with generous padding (`pad_x = 0.50 * text_height`, `pad_y = 0.35 * text_height`).
    - Centered pill box with deep cosmic purple fill and golden outer ring.
- **Aspect Ratio Compositions:**
  - **5244 × 2950 px**: Scaled and centered with Lanczos + unsharp mask.
  - **3840 × 1646 px (2.33:1 Ultra-wide)**: Separately framed from vertical source range `[140, 688]` px, keeping cheese, star, gem, mascot, ring, and planets completely intact within canvas boundaries and central safe area.

## Intermediate Files & QC Artifacts in `source/`

- `orbital_salvage_header_base_v2.jpg`: Raw generator output.
- `crop-title-lockup-100.jpg`: 100% full-resolution 1:1 pixel inspection crop of the lockup.
