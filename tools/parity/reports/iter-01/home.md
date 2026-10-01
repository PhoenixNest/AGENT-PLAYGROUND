# iter-01: home_default from the web's measured layout

The home screen (`home_default`, both profiles) is drawn by `HomeChrome` + `CssBox` (Canvas2D, outline text) from the layout Chromium measured, instead of the legacy UI. Other states keep the legacy UI.

Pipeline: `node web/dump-spec.mjs --only home` (the dump entry is named `home`; `--only` already filters by entry) -> `out/web/spec/home__<profile>.json` -> `node web/distill-spec.mjs --page home` -> `Assets/Resources/Parity/Spec/home__<profile>.json` -> `HomeChrome.Draw()` at scale 3 into a 1290 x 2796 canvas -> `Graphics.CopyTexture` into the parity render target that `ReadPng` reads. `ParityRunner.Capture` deactivates `AppRoot/Page_Home` for this state.

## Result (diff.py, SPEC section 2 bar: 99.0 % pixels, SSIM 0.985)

| state | profile | pixels % (within 16) | SSIM |
|---|---|---:|---:|
| home_default | zero | 93.22 | 0.9602 |
| home_default | iphone | 93.31 | 0.9604 |

Not a pass, and not expected to be: the diorama, pedestal and CSS animations are deferred. Sub-regions (pixels % within 16):

| region | zero | iphone |
|---|---:|---:|
| top bar (coins pill, sound and settings buttons) | 99.31 | 99.51 |
| bottom dock (4 tabs, badges) | 98.71 | 99.14 |
| title block (ORBITAL / SALVAGE / subtitle pill) | 94.31 | 94.31 |
| launch console incl. PLAY MISSION | 88.96 | 88.96 |
| PLAY MISSION button with its shadow | 84.56 | 84.56 |
| everything outside the hero stage rect | 95.60 | 96.35 |

Where the remaining error is (zero profile, share of all bad pixels): hero stage (diorama, ring, pedestal) about 70 %, the play-button shine sweep rectangle about 53 % (it overlaps the stage rect numbers; the sweep is a diagonal white gradient across the button in the reference), console about 22 %.

The first capture had no blank frame, wrong page, flipped y or visible legacy UI (checked in the player, which gives the same numbers as the editor render), so no fix iteration was needed.

## Deferred (listed per capture in `*.unity-notes.json` under `homeChrome.deferred`)

- 3D pedestal diorama: `stage-pedestal-3d` subtree (matrix3d discs, glow beam, rim pulse, blurred shadow).
- Play button shine sweep (`play-btn-glow`, animated gradient clipped by the button) and the `heroPulse` scale.
- backdrop-filter (launch console, dock): only the translucent background colour is drawn.
- filter: drop-shadow (the 6 background debris emoji, star) and blur.
- Inset box-shadows (diorama ring, pedestal).
- CSS animations in general: elements are drawn once at the rect measured at the capture time (sunburst rotation, ring float, nibbles idle, tag pulse).
- Elliptical radial gradients (only circle is implemented), element transforms other than the rect they produce.

## Files

- `images/home_default__{zero,iphone}.png`: Unity captures; `images/*.diff.png`, `images/*.heatmap.png`: diff.py outputs.
- `report.md`, `report.json`: diff.py output (the layout-box and trace verdicts compare the legacy UI layout dump, which does not describe the Canvas2D drawing, so they are not meaningful for this state).
