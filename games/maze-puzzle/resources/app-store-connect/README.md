# App Store Connect images

Made on 2026-10-14 for the English store page. Details and caveats are in `13_APP_STORE_METADATA_AND_SUBMISSION_MANUAL.md` sections 7.2 and 7.2a.

## Header (ASC: Header and Search Results, Header)

| File                          | Size (px)   | Notes                         |
| :---------------------------- | :---------- | :---------------------------- |
| `header/header-5244x2950.png` | 5244 × 2950 | RGB, no transparency, 4.3 MB  |
| `header/header-3840x1646.png` | 3840 × 1646 | RGB, no transparency, 2.7 MB  |
| `header/header-preview.jpg`   | 1200 × 675  | Preview only, do not upload   |
| `header/source/`              | -           | Script, prompts, base artwork |

Upload one of the two sizes. They are separate compositions, not crops of each other.

## Screenshots (ASC: App Previews and Screenshots, iPhone with Dynamic Island)

| Folder                          | Size (px)   | Use                                              |
| :------------------------------ | :---------- | :----------------------------------------------- |
| `screenshots/1206x2622/`        | 1206 × 2622 | 6.3-inch slot. Primary.                          |
| `screenshots/1179x2556/`        | 1179 × 2556 | 6.1-inch slot.                                   |
| `captioned/` and `plain/`       | -           | Two versions of the same four screens. Pick one. |
| `screenshots/contact-sheet.jpg` | 1600 × 870  | Preview of the captioned set, do not upload.     |

Upload order: `01-home.png`, `02-daily.png`, `03-shop.png`, `04-gameplay.png`. `screenshots/build_screenshots.py` rebuilds all 16 files from `web/screenshots/`.

The four screens come from the web prototype, not from a device capture.
