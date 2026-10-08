# Parity report iter-01

Generated 2026-10-01T10:05:43.  web: `C:\Users\ASUS\Documents\Code\Local\AGENT-PLAYGROUND\.worktrees\parity-int\tools\parity\out\web`  unity: `C:\Users\ASUS\Documents\Code\Local\AGENT-PLAYGROUND\.worktrees\parity-int\tools\parity\out\unity-home`

**0 PASS, 2 FAIL, 0 MISSING** of 2 (state, profile) pairs; traces: 0 PASS, 1 FAIL, 0 MISSING of 1.

Pass bar (SPEC 2): >= 99.0 % pixels within 16 per channel, SSIM >= 0.985, every visible matched element within 3 px on x, y, w, h.

## States (worst first)

| state | profile | pixels % | SSIM | worst box delta px | trace | verdict | reasons |
| --- | --- | --- | --- | --- | --- | --- | --- |
| home_default | zero | 93.22 | 0.9602 | - | FAIL | FAIL | pixels 93.22% < 99.0%; ssim 0.9602 < 0.985; boxes: 117 failing; trace |
| home_default | iphone | 93.31 | 0.9604 | - | FAIL | FAIL | pixels 93.31% < 99.0%; ssim 0.9604 < 0.985; boxes: 117 failing; trace |

## Failing elements

### home_default__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-home | visibility | - | - | - | - |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-credits-val | visibility | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| btn-home-audio | visibility | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
| btn-home-settings | visibility | - | - | - | - |
| page-scroll-body#1 | missing_in_unity | - | - | - | - |
| home-hero-section#1 | missing_in_unity | - | - | - | - |
| home-title-wrapper#1 | missing_in_unity | - | - | - | - |
| home-logo-title-3d#1 | missing_in_unity | - | - | - | - |
| title-sub#1 | missing_in_unity | - | - | - | - |
| home-logo-subtitle#1 | missing_in_unity | - | - | - | - |
| home-hero-stage#1 | missing_in_unity | - | - | - | - |
| stage-pedestal-3d#1 | missing_in_unity | - | - | - | - |
| pedestal-shadow#1 | missing_in_unity | - | - | - | - |
| pedestal-disc-base#1 | missing_in_unity | - | - | - | - |
| pedestal-disc-rim#1 | missing_in_unity | - | - | - | - |
| ... 92 more | | | | | |

### home_default__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-home | visibility | - | - | - | - |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-credits-val | visibility | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| btn-home-audio | visibility | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
| btn-home-settings | visibility | - | - | - | - |
| page-scroll-body#1 | missing_in_unity | - | - | - | - |
| home-hero-section#1 | missing_in_unity | - | - | - | - |
| home-title-wrapper#1 | missing_in_unity | - | - | - | - |
| home-logo-title-3d#1 | missing_in_unity | - | - | - | - |
| title-sub#1 | missing_in_unity | - | - | - | - |
| home-logo-subtitle#1 | missing_in_unity | - | - | - | - |
| home-hero-stage#1 | missing_in_unity | - | - | - | - |
| stage-pedestal-3d#1 | missing_in_unity | - | - | - | - |
| pedestal-shadow#1 | missing_in_unity | - | - | - | - |
| pedestal-disc-base#1 | missing_in_unity | - | - | - | - |
| pedestal-disc-rim#1 | missing_in_unity | - | - | - | - |
| ... 92 more | | | | | |

## Traces

| sequence | verdict | detail |
| --- | --- | --- |
| home_default | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |

## Harness settings

### web

- Chromium: 153.0.8010.12
- Launch args: `--force-color-profile=srgb --font-render-hinting=none --disable-lcd-text`
- Animation clock: registry+sweep (SPEC 4.5)
- perfOffset per run: 1, 10, 102, 103, 11, 12, 13, 15, 153, 17, 171, 19, 2, 20, 28, 29, 3, 35, 37, 4, 40, 41, 44, 45, 457, 47, 48, 49, 5, 51, 52, 6, 65, 68, 7, 72, 74, 77, 8, 84, 89, 9, 91, 92, 93, 94 (314 runs)

### unity

`_meta.json` not found.
