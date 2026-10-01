# Parity report iter-00

Generated 2026-10-01T05:17:34.  web: `C:\Users\ASUS\Documents\Code\Local\AGENT-PLAYGROUND\.worktrees\parity-int\tools\parity\out\web`  unity: `C:\Users\ASUS\Documents\Code\Local\AGENT-PLAYGROUND\.worktrees\parity-int\tools\parity\out\unity-iso`

**0 PASS, 276 FAIL, 0 MISSING** of 276 (state, profile) pairs; traces: 0 PASS, 138 FAIL, 7 MISSING of 145.

Pass bar (SPEC 2): >= 99.0 % pixels within 16 per channel, SSIM >= 0.985, every visible matched element within 3 px on x, y, w, h.

## States (worst first)

| state | profile | pixels % | SSIM | worst box delta px | trace | verdict | reasons |
| --- | --- | --- | --- | --- | --- | --- | --- |
| loading_t0000 | zero | 0.33 | 0.8794 | 173.89 (loading-tip-text w) | FAIL | FAIL | pixels 0.33% < 99.0%; ssim 0.8794 < 0.985; boxes: 65 failing; trace |
| loading_t0000 | iphone | 0.34 | 0.8787 | 173.89 (loading-tip-text w) | FAIL | FAIL | pixels 0.34% < 99.0%; ssim 0.8787 < 0.985; boxes: 65 failing; trace |
| pressed_back_achievements | iphone | 1.24 | 0.7744 | 116.34 (achievements-page-container h) | FAIL | FAIL | pixels 1.24% < 99.0%; ssim 0.7744 < 0.985; boxes: 94 failing; trace |
| pressed_back_achievements | zero | 1.37 | 0.7754 | 198.18 (achievements-page-container h) | FAIL | FAIL | pixels 1.37% < 99.0%; ssim 0.7754 < 0.985; boxes: 94 failing; trace |
| achievements_all | iphone | 1.46 | 0.7559 | 116.34 (achievements-page-container h) | FAIL | FAIL | pixels 1.46% < 99.0%; ssim 0.7559 < 0.985; boxes: 94 failing; trace |
| achievements_none | iphone | 1.49 | 0.7745 | 116.34 (achievements-page-container h) | FAIL | FAIL | pixels 1.49% < 99.0%; ssim 0.7745 < 0.985; boxes: 94 failing; trace |
| achievements_some | iphone | 1.49 | 0.7671 | 116.34 (achievements-page-container h) | FAIL | FAIL | pixels 1.49% < 99.0%; ssim 0.7671 < 0.985; boxes: 94 failing; trace |
| pressed_back_settings | iphone | 1.58 | 0.7516 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 1.58% < 99.0%; ssim 0.7516 < 0.985; boxes: 104 failing; trace |
| achievements_some | zero | 1.66 | 0.7678 | 198.18 (achievements-page-container h) | FAIL | FAIL | pixels 1.66% < 99.0%; ssim 0.7678 < 0.985; boxes: 94 failing; trace |
| achievements_none | zero | 1.66 | 0.7753 | 198.18 (achievements-page-container h) | FAIL | FAIL | pixels 1.66% < 99.0%; ssim 0.7753 < 0.985; boxes: 94 failing; trace |
| achievements_all | zero | 1.66 | 0.7566 | 198.18 (achievements-page-container h) | FAIL | FAIL | pixels 1.66% < 99.0%; ssim 0.7566 < 0.985; boxes: 94 failing; trace |
| settings_toggles_off | iphone | 1.81 | 0.7522 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 1.81% < 99.0%; ssim 0.7522 < 0.985; boxes: 104 failing; trace |
| pressed_back_settings | zero | 1.83 | 0.7513 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 1.83% < 99.0%; ssim 0.7513 < 0.985; boxes: 104 failing; trace |
| settings_default | iphone | 1.83 | 0.7517 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 1.83% < 99.0%; ssim 0.7517 < 0.985; boxes: 104 failing; trace |
| settings_scrolled | iphone | 1.83 | 0.7517 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 1.83% < 99.0%; ssim 0.7517 < 0.985; boxes: 104 failing; trace |
| settings_sliders_moved | iphone | 1.83 | 0.7517 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 1.83% < 99.0%; ssim 0.7517 < 0.985; boxes: 104 failing; trace |
| settings_sliders_profile | iphone | 1.83 | 0.7517 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 1.83% < 99.0%; ssim 0.7517 < 0.985; boxes: 104 failing; trace |
| settings_toggles_clicked | iphone | 1.84 | 0.7519 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 1.84% < 99.0%; ssim 0.7519 < 0.985; boxes: 104 failing; trace |
| settings_default | zero | 2.12 | 0.7512 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 2.12% < 99.0%; ssim 0.7512 < 0.985; boxes: 104 failing; trace |
| settings_scrolled | zero | 2.12 | 0.7512 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 2.12% < 99.0%; ssim 0.7512 < 0.985; boxes: 104 failing; trace |
| settings_toggles_off | zero | 2.12 | 0.7518 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 2.12% < 99.0%; ssim 0.7518 < 0.985; boxes: 104 failing; trace |
| settings_sliders_moved | zero | 2.12 | 0.7510 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 2.12% < 99.0%; ssim 0.7510 < 0.985; boxes: 104 failing; trace |
| settings_sliders_profile | zero | 2.12 | 0.7510 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 2.12% < 99.0%; ssim 0.7510 < 0.985; boxes: 104 failing; trace |
| settings_toggles_clicked | zero | 2.16 | 0.7508 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 2.16% < 99.0%; ssim 0.7508 < 0.985; boxes: 104 failing; trace |
| modal_pause_closing_t0150 | iphone | 2.60 | 0.4267 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 2.60% < 99.0%; ssim 0.4267 < 0.985; boxes: 71 failing; trace |
| modal_pause_closing_t0150 | zero | 2.94 | 0.4249 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 2.94% < 99.0%; ssim 0.4249 < 0.985; boxes: 71 failing; trace |
| pressed_hud_pause | iphone | 3.67 | 0.4080 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 3.67% < 99.0%; ssim 0.4080 < 0.985; boxes: 71 failing; trace |
| pressed_hud_restart | iphone | 3.67 | 0.4080 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 3.67% < 99.0%; ssim 0.4080 < 0.985; boxes: 71 failing; trace |
| game_lvl01_pickup | iphone | 3.74 | 0.3973 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 3.74% < 99.0%; ssim 0.3973 < 0.985; boxes: 73 failing; trace |
| game_lvl01_restart_toast | iphone | 3.77 | 0.3982 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 3.77% < 99.0%; ssim 0.3982 < 0.985; boxes: 73 failing; trace |
| game_lvl01_skins_kitty_party_mint | zero | 3.92 | 0.5202 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 3.92% < 99.0%; ssim 0.5202 < 0.985; boxes: 71 failing; trace |
| game_lvl01_quota_met_hud | iphone | 3.98 | 0.4082 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 3.98% < 99.0%; ssim 0.4082 < 0.985; boxes: 71 failing; trace |
| game_lvl01_near_victory | iphone | 3.98 | 0.4083 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 3.98% < 99.0%; ssim 0.4083 < 0.985; boxes: 71 failing; trace |
| game_lvl01_skins_kitty_party_mint | iphone | 4.05 | 0.5220 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.05% < 99.0%; ssim 0.5220 < 0.985; boxes: 71 failing; trace |
| pressed_hud_pause | zero | 4.13 | 0.4066 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.13% < 99.0%; ssim 0.4066 < 0.985; boxes: 71 failing; trace |
| pressed_hud_restart | zero | 4.13 | 0.4065 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.13% < 99.0%; ssim 0.4065 < 0.985; boxes: 71 failing; trace |
| game_lvl01_t0016ms | iphone | 4.31 | 0.4113 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.31% < 99.0%; ssim 0.4113 < 0.985; boxes: 71 failing; trace |
| game_lvl01_quota_met_hud | zero | 4.43 | 0.4067 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.43% < 99.0%; ssim 0.4067 < 0.985; boxes: 71 failing; trace |
| game_lvl01_near_victory | zero | 4.43 | 0.4068 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.43% < 99.0%; ssim 0.4068 < 0.985; boxes: 71 failing; trace |
| game_lvl11_t0016ms | iphone | 4.69 | 0.4080 | 98.66 (hud-quota-text x) | FAIL | FAIL | pixels 4.69% < 99.0%; ssim 0.4080 < 0.985; boxes: 71 failing; trace |
| game_lvl01_t0016ms | zero | 4.76 | 0.4098 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.76% < 99.0%; ssim 0.4098 < 0.985; boxes: 71 failing; trace |
| game_lvl01_hint_dismissing_t0200 | iphone | 4.78 | 0.4122 | 103.10 (hud-quota-text x) | FAIL | FAIL | pixels 4.78% < 99.0%; ssim 0.4122 < 0.985; boxes: 71 failing; trace |
| game_lvl01_t1000ms | iphone | 4.80 | 0.4109 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.80% < 99.0%; ssim 0.4109 < 0.985; boxes: 71 failing; trace |
| game_lvl01_hint_visible | iphone | 4.83 | 0.5726 | 101.39 (hud-quota-text x) | FAIL | FAIL | pixels 4.83% < 99.0%; ssim 0.5726 < 0.985; boxes: 72 failing; trace |
| game_lvl01_mid_drag | iphone | 4.83 | 0.4107 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.83% < 99.0%; ssim 0.4107 < 0.985; boxes: 71 failing; trace |
| game_lvl01_t3000ms | iphone | 4.85 | 0.4135 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.85% < 99.0%; ssim 0.4135 < 0.985; boxes: 71 failing; trace |
| game_lvl01_gate_pass | iphone | 4.85 | 0.4087 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 4.85% < 99.0%; ssim 0.4087 < 0.985; boxes: 71 failing; trace |
| game_lvl21_t0016ms | iphone | 4.99 | 0.4082 | 96.60 (hud-quota-text x) | FAIL | FAIL | pixels 4.99% < 99.0%; ssim 0.4082 < 0.985; boxes: 71 failing; trace |
| game_lvl01_restart_toast | zero | 5.01 | 0.3936 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 5.01% < 99.0%; ssim 0.3936 < 0.985; boxes: 73 failing; trace |
| game_lvl01_hint_dismissing_t0200 | zero | 5.02 | 0.4104 | 103.10 (hud-quota-text x) | FAIL | FAIL | pixels 5.02% < 99.0%; ssim 0.4104 < 0.985; boxes: 71 failing; trace |
| game_lvl11_skins_duckie_berry_cheese | zero | 5.07 | 0.5274 | 98.66 (hud-quota-text x) | FAIL | FAIL | pixels 5.07% < 99.0%; ssim 0.5274 < 0.985; boxes: 71 failing; trace |
| game_lvl11_t0016ms | zero | 5.13 | 0.4064 | 98.66 (hud-quota-text x) | FAIL | FAIL | pixels 5.13% < 99.0%; ssim 0.4064 < 0.985; boxes: 71 failing; trace |
| game_lvl01_hint_visible | zero | 5.17 | 0.5731 | 101.39 (hud-quota-text x) | FAIL | FAIL | pixels 5.17% < 99.0%; ssim 0.5731 < 0.985; boxes: 72 failing; trace |
| game_lvl11_skins_duckie_berry_cheese | iphone | 5.20 | 0.5310 | 98.66 (hud-quota-text x) | FAIL | FAIL | pixels 5.20% < 99.0%; ssim 0.5310 < 0.985; boxes: 71 failing; trace |
| game_lvl11_t3000ms | iphone | 5.21 | 0.4115 | 98.66 (hud-quota-text x) | FAIL | FAIL | pixels 5.21% < 99.0%; ssim 0.4115 < 0.985; boxes: 71 failing; trace |
| game_lvl01_t1000ms | zero | 5.25 | 0.4094 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 5.25% < 99.0%; ssim 0.4094 < 0.985; boxes: 71 failing; trace |
| game_lvl01_mid_drag | zero | 5.28 | 0.4091 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 5.28% < 99.0%; ssim 0.4091 < 0.985; boxes: 71 failing; trace |
| game_lvl01_gate_pass | zero | 5.29 | 0.4071 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 5.29% < 99.0%; ssim 0.4071 < 0.985; boxes: 71 failing; trace |
| game_lvl01_t3000ms | zero | 5.30 | 0.4120 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 5.30% < 99.0%; ssim 0.4120 < 0.985; boxes: 71 failing; trace |
| game_lvl11_t1000ms | iphone | 5.34 | 0.4195 | 98.66 (hud-quota-text x) | FAIL | FAIL | pixels 5.34% < 99.0%; ssim 0.4195 < 0.985; boxes: 71 failing; trace |
| game_lvl31_t0016ms | iphone | 5.35 | 0.4048 | 96.61 (hud-quota-text x) | FAIL | FAIL | pixels 5.35% < 99.0%; ssim 0.4048 < 0.985; boxes: 71 failing; trace |
| game_lvl01_pickup | zero | 5.37 | 0.3922 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 5.37% < 99.0%; ssim 0.3922 < 0.985; boxes: 73 failing; trace |
| game_lvl21_t0016ms | zero | 5.44 | 0.4068 | 96.60 (hud-quota-text x) | FAIL | FAIL | pixels 5.44% < 99.0%; ssim 0.4068 < 0.985; boxes: 71 failing; trace |
| game_lvl41_t0016ms | iphone | 5.52 | 0.4010 | 96.77 (hud-quota-text x) | FAIL | FAIL | pixels 5.52% < 99.0%; ssim 0.4010 < 0.985; boxes: 71 failing; trace |
| game_lvl21_t1000ms | iphone | 5.53 | 0.4272 | 96.60 (hud-quota-text x) | FAIL | FAIL | pixels 5.53% < 99.0%; ssim 0.4272 < 0.985; boxes: 71 failing; trace |
| game_lvl11_t3000ms | zero | 5.65 | 0.4099 | 98.66 (hud-quota-text x) | FAIL | FAIL | pixels 5.65% < 99.0%; ssim 0.4099 < 0.985; boxes: 71 failing; trace |
| game_lvl41_t1000ms | iphone | 5.69 | 0.3976 | 96.77 (hud-quota-text x) | FAIL | FAIL | pixels 5.69% < 99.0%; ssim 0.3976 < 0.985; boxes: 71 failing; trace |
| game_lvl31_t1000ms | iphone | 5.69 | 0.4042 | 96.61 (hud-quota-text x) | FAIL | FAIL | pixels 5.69% < 99.0%; ssim 0.4042 < 0.985; boxes: 71 failing; trace |
| game_lvl31_t3000ms | iphone | 5.72 | 0.4039 | 96.61 (hud-quota-text x) | FAIL | FAIL | pixels 5.72% < 99.0%; ssim 0.4039 < 0.985; boxes: 71 failing; trace |
| game_lvl11_t1000ms | zero | 5.79 | 0.4179 | 98.66 (hud-quota-text x) | FAIL | FAIL | pixels 5.79% < 99.0%; ssim 0.4179 < 0.985; boxes: 71 failing; trace |
| game_lvl31_t0016ms | zero | 5.80 | 0.4033 | 96.61 (hud-quota-text x) | FAIL | FAIL | pixels 5.80% < 99.0%; ssim 0.4033 < 0.985; boxes: 71 failing; trace |
| game_lvl41_t3000ms | iphone | 5.81 | 0.4011 | 96.77 (hud-quota-text x) | FAIL | FAIL | pixels 5.81% < 99.0%; ssim 0.4011 < 0.985; boxes: 71 failing; trace |
| game_lvl21_t1000ms | zero | 5.97 | 0.4258 | 96.60 (hud-quota-text x) | FAIL | FAIL | pixels 5.97% < 99.0%; ssim 0.4258 < 0.985; boxes: 71 failing; trace |
| game_lvl41_t0016ms | zero | 5.97 | 0.3995 | 96.77 (hud-quota-text x) | FAIL | FAIL | pixels 5.97% < 99.0%; ssim 0.3995 < 0.985; boxes: 71 failing; trace |
| game_lvl31_t1000ms | zero | 6.13 | 0.4028 | 96.61 (hud-quota-text x) | FAIL | FAIL | pixels 6.13% < 99.0%; ssim 0.4028 < 0.985; boxes: 71 failing; trace |
| game_lvl41_t1000ms | zero | 6.13 | 0.3963 | 96.77 (hud-quota-text x) | FAIL | FAIL | pixels 6.13% < 99.0%; ssim 0.3963 < 0.985; boxes: 71 failing; trace |
| game_lvl31_t3000ms | zero | 6.15 | 0.4024 | 96.61 (hud-quota-text x) | FAIL | FAIL | pixels 6.15% < 99.0%; ssim 0.4024 < 0.985; boxes: 71 failing; trace |
| game_lvl41_t3000ms | zero | 6.24 | 0.3997 | 96.77 (hud-quota-text x) | FAIL | FAIL | pixels 6.24% < 99.0%; ssim 0.3997 < 0.985; boxes: 71 failing; trace |
| shop_buy_insufficient_toast | iphone | 6.33 | 0.6702 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.33% < 99.0%; ssim 0.6702 < 0.985; boxes: 102 failing; trace |
| shop_buy_success_toast | iphone | 6.36 | 0.6754 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.36% < 99.0%; ssim 0.6754 < 0.985; boxes: 102 failing; trace |
| shop_equip_toast | iphone | 6.36 | 0.6757 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.36% < 99.0%; ssim 0.6757 < 0.985; boxes: 102 failing; trace |
| pressed_back_shop | iphone | 6.44 | 0.6848 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.44% < 99.0%; ssim 0.6848 < 0.985; boxes: 100 failing; trace |
| shop_skyboxes_default | iphone | 6.51 | 0.6963 | 348.34 (store-page-container h) | FAIL | FAIL | pixels 6.51% < 99.0%; ssim 0.6963 < 0.985; boxes: 94 failing; trace |
| shop_skyboxes_mixed | iphone | 6.53 | 0.7020 | 348.34 (store-page-container h) | FAIL | FAIL | pixels 6.53% < 99.0%; ssim 0.7020 < 0.985; boxes: 94 failing; trace |
| pressed_claim_gift | iphone | 6.64 | 0.6862 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.64% < 99.0%; ssim 0.6862 < 0.985; boxes: 100 failing; trace |
| shop_singularities_default | iphone | 6.67 | 0.6859 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.67% < 99.0%; ssim 0.6859 < 0.985; boxes: 100 failing; trace |
| shop_rings_default | iphone | 6.68 | 0.6850 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.68% < 99.0%; ssim 0.6850 < 0.985; boxes: 100 failing; trace |
| shop_scrolled | iphone | 6.68 | 0.6850 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.68% < 99.0%; ssim 0.6850 < 0.985; boxes: 100 failing; trace |
| shop_singularities_mixed | iphone | 6.69 | 0.6903 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.69% < 99.0%; ssim 0.6903 < 0.985; boxes: 100 failing; trace |
| pressed_shop_buy | iphone | 6.69 | 0.6840 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.69% < 99.0%; ssim 0.6840 < 0.985; boxes: 100 failing; trace |
| pressed_shop_tab | iphone | 6.69 | 0.6850 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.69% < 99.0%; ssim 0.6850 < 0.985; boxes: 100 failing; trace |
| shop_rings_mixed | iphone | 6.72 | 0.6940 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 6.72% < 99.0%; ssim 0.6940 < 0.985; boxes: 100 failing; trace |
| daily_master_claim_toast | iphone | 7.04 | 0.6599 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 7.04% < 99.0%; ssim 0.6599 < 0.985; boxes: 101 failing; trace |
| daily_master_claimed | iphone | 7.42 | 0.6722 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 7.42% < 99.0%; ssim 0.6722 < 0.985; boxes: 99 failing; trace |
| pressed_back_shop | zero | 7.68 | 0.6678 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.68% < 99.0%; ssim 0.6678 < 0.985; boxes: 100 failing; trace |
| shop_skyboxes_mixed | zero | 7.73 | 0.6919 | 430.18 (store-page-container h) | FAIL | FAIL | pixels 7.73% < 99.0%; ssim 0.6919 < 0.985; boxes: 94 failing; trace |
| shop_skyboxes_default | zero | 7.73 | 0.6824 | 430.18 (store-page-container h) | FAIL | FAIL | pixels 7.73% < 99.0%; ssim 0.6824 < 0.985; boxes: 94 failing; trace |
| shop_singularities_mixed | zero | 7.73 | 0.6741 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.73% < 99.0%; ssim 0.6741 < 0.985; boxes: 100 failing; trace |
| shop_singularities_default | zero | 7.74 | 0.6681 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.74% < 99.0%; ssim 0.6681 < 0.985; boxes: 100 failing; trace |
| pressed_claim_gift | zero | 7.77 | 0.6690 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.77% < 99.0%; ssim 0.6690 < 0.985; boxes: 100 failing; trace |
| game_lvl21_t3000ms | iphone | 7.89 | 0.6160 | 529.78 (defeat-message y) | FAIL | FAIL | pixels 7.89% < 99.0%; ssim 0.6160 < 0.985; boxes: 70 failing; trace |
| shop_rings_mixed | zero | 7.95 | 0.6794 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.95% < 99.0%; ssim 0.6794 < 0.985; boxes: 100 failing; trace |
| shop_rings_default | zero | 7.95 | 0.6678 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.95% < 99.0%; ssim 0.6678 < 0.985; boxes: 100 failing; trace |
| shop_scrolled | zero | 7.95 | 0.6678 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.95% < 99.0%; ssim 0.6678 < 0.985; boxes: 100 failing; trace |
| pressed_shop_buy | zero | 7.97 | 0.6677 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.97% < 99.0%; ssim 0.6677 < 0.985; boxes: 100 failing; trace |
| pressed_shop_tab | zero | 7.97 | 0.6678 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 7.97% < 99.0%; ssim 0.6678 < 0.985; boxes: 100 failing; trace |
| shop_buy_insufficient_toast | zero | 8.21 | 0.6554 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 8.21% < 99.0%; ssim 0.6554 < 0.985; boxes: 102 failing; trace |
| shop_buy_success_toast | zero | 8.21 | 0.6627 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 8.21% < 99.0%; ssim 0.6627 < 0.985; boxes: 102 failing; trace |
| modal_defeat_backup_used_toast | iphone | 8.22 | 0.6071 | 529.78 (defeat-message y) | FAIL | FAIL | pixels 8.22% < 99.0%; ssim 0.6071 < 0.985; boxes: 74 failing; trace |
| shop_equip_toast | zero | 8.22 | 0.6632 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 8.22% < 99.0%; ssim 0.6632 < 0.985; boxes: 102 failing; trace |
| game_lvl21_t3000ms | zero | 8.96 | 0.6358 | 534.55 (defeat-message y) | FAIL | FAIL | pixels 8.96% < 99.0%; ssim 0.6358 < 0.985; boxes: 70 failing; trace |
| game_daily_bronze_t1000 | iphone | 8.97 | 0.5991 | - | FAIL | FAIL | pixels 8.97% < 99.0%; ssim 0.5991 < 0.985; boxes: 77 failing; trace |
| modal_defeat_backup_used_toast | zero | 9.37 | 0.6217 | 534.55 (defeat-message y) | FAIL | FAIL | pixels 9.37% < 99.0%; ssim 0.6217 < 0.985; boxes: 74 failing; trace |
| shop_claim_gift_toast | iphone | 10.26 | 0.6754 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 10.26% < 99.0%; ssim 0.6754 < 0.985; boxes: 102 failing; trace |
| shop_gift_claimed | iphone | 10.62 | 0.6902 | 219.54 (store-page-container h) | FAIL | FAIL | pixels 10.62% < 99.0%; ssim 0.6902 < 0.985; boxes: 100 failing; trace |
| daily_master_ready | iphone | 10.90 | 0.6441 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 10.90% < 99.0%; ssim 0.6441 < 0.985; boxes: 100 failing; trace |
| shop_gift_claimed | zero | 11.37 | 0.6731 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 11.37% < 99.0%; ssim 0.6731 < 0.985; boxes: 100 failing; trace |
| shop_claim_gift_toast | zero | 11.39 | 0.6584 | 301.38 (store-page-container h) | FAIL | FAIL | pixels 11.39% < 99.0%; ssim 0.6584 < 0.985; boxes: 102 failing; trace |
| game_daily_bronze_t1000 | zero | 12.29 | 0.5974 | - | FAIL | FAIL | pixels 12.29% < 99.0%; ssim 0.5974 < 0.985; boxes: 77 failing; trace |
| daily_two_cleared | iphone | 13.26 | 0.6662 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 13.26% < 99.0%; ssim 0.6662 < 0.985; boxes: 100 failing; trace |
| modal_pause_t0150 | iphone | 13.76 | 0.7081 | 103.13 (hud-quota-text x) | FAIL | FAIL | pixels 13.76% < 99.0%; ssim 0.7081 < 0.985; boxes: 70 failing; trace |
| modal_pause_t0150 | zero | 14.19 | 0.7098 | 114.09 (pause-level-name y) | FAIL | FAIL | pixels 14.19% < 99.0%; ssim 0.7098 < 0.985; boxes: 70 failing; trace |
| pressed_dock_trophies | zero | 15.44 | 0.6664 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 15.44% < 99.0%; ssim 0.6664 < 0.985; boxes: 116 failing; trace |
| pressed_dock_maps | zero | 15.47 | 0.6661 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 15.47% < 99.0%; ssim 0.6661 < 0.985; boxes: 116 failing; trace |
| pressed_dock_shop | zero | 15.56 | 0.6665 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 15.56% < 99.0%; ssim 0.6665 < 0.985; boxes: 116 failing; trace |
| pressed_dock_daily | zero | 15.57 | 0.6666 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 15.57% < 99.0%; ssim 0.6666 < 0.985; boxes: 116 failing; trace |
| home_anim_t02500 | zero | 15.81 | 0.6650 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 15.81% < 99.0%; ssim 0.6650 < 0.985; boxes: 116 failing; trace |
| toast_t2300 | zero | 15.83 | 0.6647 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 15.83% < 99.0%; ssim 0.6647 < 0.985; boxes: 118 failing; trace |
| home_sound_toggle_toast | zero | 16.09 | 0.6543 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.09% < 99.0%; ssim 0.6543 < 0.985; boxes: 118 failing; trace |
| toast_t1000 | zero | 16.09 | 0.6541 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.09% < 99.0%; ssim 0.6541 < 0.985; boxes: 118 failing; trace |
| home_anim_t12000 | zero | 16.11 | 0.6658 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.11% < 99.0%; ssim 0.6658 < 0.985; boxes: 116 failing; trace |
| toast_long | zero | 16.12 | 0.6485 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.12% < 99.0%; ssim 0.6485 < 0.985; boxes: 118 failing; trace |
| home_equipped_kitty_confetti | zero | 16.17 | 0.6659 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.17% < 99.0%; ssim 0.6659 < 0.985; boxes: 116 failing; trace |
| home_anim_t00900 | zero | 16.17 | 0.6663 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.17% < 99.0%; ssim 0.6663 < 0.985; boxes: 116 failing; trace |
| toast_stacked | zero | 16.19 | 0.6473 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.19% < 99.0%; ssim 0.6473 < 0.985; boxes: 119 failing; trace |
| home_equipped_duckie_berry | zero | 16.19 | 0.6665 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.19% < 99.0%; ssim 0.6665 < 0.985; boxes: 116 failing; trace |
| home_mascot_click_t0700 | zero | 16.21 | 0.6664 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.21% < 99.0%; ssim 0.6664 < 0.985; boxes: 117 failing; trace |
| home_equipped_candy_sleepy | zero | 16.23 | 0.6687 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.23% < 99.0%; ssim 0.6687 < 0.985; boxes: 116 failing; trace |
| pressed_home_settings | zero | 16.23 | 0.6670 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.23% < 99.0%; ssim 0.6670 < 0.985; boxes: 116 failing; trace |
| toast_t0200 | zero | 16.25 | 0.6533 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.25% < 99.0%; ssim 0.6533 < 0.985; boxes: 118 failing; trace |
| pressed_home_audio | zero | 16.25 | 0.6670 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.25% < 99.0%; ssim 0.6670 < 0.985; boxes: 116 failing; trace |
| home_anim_t00400 | zero | 16.26 | 0.6666 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.26% < 99.0%; ssim 0.6666 < 0.985; boxes: 116 failing; trace |
| home_mascot_click_t0300 | zero | 16.28 | 0.6665 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.28% < 99.0%; ssim 0.6665 < 0.985; boxes: 117 failing; trace |
| home_anim_t06000 | zero | 16.28 | 0.6660 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.28% < 99.0%; ssim 0.6660 < 0.985; boxes: 116 failing; trace |
| home_sector_21_big_credits | zero | 16.29 | 0.6635 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.29% < 99.0%; ssim 0.6635 < 0.985; boxes: 116 failing; trace |
| home_badges_two_new | zero | 16.33 | 0.6652 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.33% < 99.0%; ssim 0.6652 < 0.985; boxes: 116 failing; trace |
| home_default | zero | 16.33 | 0.6652 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.33% < 99.0%; ssim 0.6652 < 0.985; boxes: 116 failing; trace |
| home_mascot_click_t0016 | zero | 16.33 | 0.6650 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.33% < 99.0%; ssim 0.6650 < 0.985; boxes: 117 failing; trace |
| home_badges_master_ready | zero | 16.33 | 0.6651 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.33% < 99.0%; ssim 0.6651 < 0.985; boxes: 116 failing; trace |
| home_badges_all_claimed | zero | 16.43 | 0.6661 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.43% < 99.0%; ssim 0.6661 < 0.985; boxes: 116 failing; trace |
| settings_modal_privacy | zero | 16.44 | 0.7007 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 16.44% < 99.0%; ssim 0.7007 < 0.985; boxes: 104 failing; trace |
| pressed_privacy_dismiss | zero | 16.44 | 0.7000 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 16.44% < 99.0%; ssim 0.7000 < 0.985; boxes: 104 failing; trace |
| loading_t0500 | zero | 16.67 | 0.7896 | 153.75 (loading-tip-text w) | FAIL | FAIL | pixels 16.67% < 99.0%; ssim 0.7896 < 0.985; boxes: 64 failing; trace |
| pressed_play_mission | zero | 16.91 | 0.6687 | 155.52 (home-mascot-stage h) | FAIL | FAIL | pixels 16.91% < 99.0%; ssim 0.6687 < 0.985; boxes: 116 failing; trace |
| loading_t1100 | zero | 16.92 | 0.7906 | 184.81 (loading-tip-text w) | FAIL | FAIL | pixels 16.92% < 99.0%; ssim 0.7906 < 0.985; boxes: 64 failing; trace |
| loading_t1100 | iphone | 16.92 | 0.7885 | 184.81 (loading-tip-text w) | FAIL | FAIL | pixels 16.92% < 99.0%; ssim 0.7885 < 0.985; boxes: 64 failing; trace |
| loading_t0500 | iphone | 16.99 | 0.7890 | 153.75 (loading-tip-text w) | FAIL | FAIL | pixels 16.99% < 99.0%; ssim 0.7890 < 0.985; boxes: 64 failing; trace |
| daily_master_ready | zero | 17.96 | 0.6503 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 17.96% < 99.0%; ssim 0.6503 < 0.985; boxes: 100 failing; trace |
| daily_master_claim_toast | zero | 19.09 | 0.6486 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 19.09% < 99.0%; ssim 0.6486 < 0.985; boxes: 101 failing; trace |
| daily_master_claimed | zero | 19.10 | 0.6634 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 19.10% < 99.0%; ssim 0.6634 < 0.985; boxes: 99 failing; trace |
| modal_pause_level_21 | zero | 19.16 | 0.7069 | 138.27 (btn-pause-resume y) | FAIL | FAIL | pixels 19.16% < 99.0%; ssim 0.7069 < 0.985; boxes: 70 failing; trace |
| settings_modal_privacy_t0150 | zero | 19.23 | 0.7191 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 19.23% < 99.0%; ssim 0.7191 < 0.985; boxes: 104 failing; trace |
| pressed_dock_trophies | iphone | 19.36 | 0.6773 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 19.36% < 99.0%; ssim 0.6773 < 0.985; boxes: 116 failing; trace |
| pressed_dock_maps | iphone | 19.38 | 0.6773 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 19.38% < 99.0%; ssim 0.6773 < 0.985; boxes: 116 failing; trace |
| pressed_dock_shop | iphone | 19.45 | 0.6775 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 19.45% < 99.0%; ssim 0.6775 < 0.985; boxes: 116 failing; trace |
| pressed_dock_daily | iphone | 19.47 | 0.6775 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 19.47% < 99.0%; ssim 0.6775 < 0.985; boxes: 116 failing; trace |
| settings_modal_reset_confirm | zero | 19.57 | 0.7485 | 190.38 (btn-reset-game-data y) | FAIL | FAIL | pixels 19.57% < 99.0%; ssim 0.7485 < 0.985; boxes: 104 failing; trace |
| game_lvl01_paused_t1000 | zero | 19.68 | 0.7113 | 154.82 (pause-level-name y) | FAIL | FAIL | pixels 19.68% < 99.0%; ssim 0.7113 < 0.985; boxes: 70 failing; trace |
| modal_pause_muted | zero | 19.71 | 0.7111 | 154.82 (pause-level-name y) | FAIL | FAIL | pixels 19.71% < 99.0%; ssim 0.7111 < 0.985; boxes: 70 failing; trace |
| modal_pause | zero | 19.72 | 0.7111 | 154.82 (pause-level-name y) | FAIL | FAIL | pixels 19.72% < 99.0%; ssim 0.7111 < 0.985; boxes: 70 failing; trace |
| pressed_modal_close | zero | 19.74 | 0.7114 | 154.82 (pause-level-name y) | FAIL | FAIL | pixels 19.74% < 99.0%; ssim 0.7114 < 0.985; boxes: 70 failing; trace |
| home_sector_21_big_credits | iphone | 19.85 | 0.6742 | 148.78 (home-mission-name w) | FAIL | FAIL | pixels 19.85% < 99.0%; ssim 0.6742 < 0.985; boxes: 116 failing; trace |
| home_badges_two_new | iphone | 19.92 | 0.6761 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 19.92% < 99.0%; ssim 0.6761 < 0.985; boxes: 116 failing; trace |
| home_default | iphone | 19.92 | 0.6761 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 19.92% < 99.0%; ssim 0.6761 < 0.985; boxes: 116 failing; trace |
| home_badges_master_ready | iphone | 19.92 | 0.6760 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 19.92% < 99.0%; ssim 0.6760 < 0.985; boxes: 116 failing; trace |
| home_mascot_click_t0016 | iphone | 20.00 | 0.6763 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.00% < 99.0%; ssim 0.6763 < 0.985; boxes: 117 failing; trace |
| home_badges_all_claimed | iphone | 20.01 | 0.6769 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.01% < 99.0%; ssim 0.6769 < 0.985; boxes: 116 failing; trace |
| home_equipped_kitty_confetti | iphone | 20.05 | 0.6771 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.05% < 99.0%; ssim 0.6771 < 0.985; boxes: 116 failing; trace |
| toast_t2300 | iphone | 20.05 | 0.6768 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.05% < 99.0%; ssim 0.6768 < 0.985; boxes: 118 failing; trace |
| home_anim_t02500 | iphone | 20.05 | 0.6770 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.05% < 99.0%; ssim 0.6770 < 0.985; boxes: 116 failing; trace |
| home_equipped_duckie_berry | iphone | 20.08 | 0.6775 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.08% < 99.0%; ssim 0.6775 < 0.985; boxes: 116 failing; trace |
| daily_bronze_cleared | iphone | 20.09 | 0.6599 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 20.09% < 99.0%; ssim 0.6599 < 0.985; boxes: 100 failing; trace |
| home_equipped_candy_sleepy | iphone | 20.10 | 0.6801 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.10% < 99.0%; ssim 0.6801 < 0.985; boxes: 116 failing; trace |
| pressed_home_settings | iphone | 20.11 | 0.6778 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.11% < 99.0%; ssim 0.6778 < 0.985; boxes: 116 failing; trace |
| pressed_home_audio | iphone | 20.12 | 0.6778 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.12% < 99.0%; ssim 0.6778 < 0.985; boxes: 116 failing; trace |
| pressed_modal_resume | zero | 20.15 | 0.7142 | 154.82 (pause-level-name y) | FAIL | FAIL | pixels 20.15% < 99.0%; ssim 0.7142 < 0.985; boxes: 70 failing; trace |
| home_mascot_click_t0700 | iphone | 20.15 | 0.6774 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.15% < 99.0%; ssim 0.6774 < 0.985; boxes: 117 failing; trace |
| home_anim_t00900 | iphone | 20.15 | 0.6774 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.15% < 99.0%; ssim 0.6774 < 0.985; boxes: 116 failing; trace |
| home_anim_t00400 | iphone | 20.16 | 0.6775 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.16% < 99.0%; ssim 0.6775 < 0.985; boxes: 116 failing; trace |
| home_mascot_click_t0300 | iphone | 20.16 | 0.6774 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.16% < 99.0%; ssim 0.6774 < 0.985; boxes: 117 failing; trace |
| home_anim_t06000 | iphone | 20.17 | 0.6775 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.17% < 99.0%; ssim 0.6775 < 0.985; boxes: 116 failing; trace |
| home_anim_t12000 | iphone | 20.18 | 0.6767 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.18% < 99.0%; ssim 0.6767 < 0.985; boxes: 116 failing; trace |
| pressed_play_mission | iphone | 20.28 | 0.6786 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.28% < 99.0%; ssim 0.6786 < 0.985; boxes: 116 failing; trace |
| toast_long | iphone | 20.32 | 0.6575 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.32% < 99.0%; ssim 0.6575 < 0.985; boxes: 118 failing; trace |
| home_sound_toggle_toast | iphone | 20.36 | 0.6635 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.36% < 99.0%; ssim 0.6635 < 0.985; boxes: 118 failing; trace |
| toast_t1000 | iphone | 20.36 | 0.6633 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.36% < 99.0%; ssim 0.6633 < 0.985; boxes: 118 failing; trace |
| toast_stacked | iphone | 20.39 | 0.6506 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.39% < 99.0%; ssim 0.6506 < 0.985; boxes: 119 failing; trace |
| toast_t0200 | iphone | 20.45 | 0.6644 | 136.80 (home-mascot-stage w) | FAIL | FAIL | pixels 20.45% < 99.0%; ssim 0.6644 < 0.985; boxes: 118 failing; trace |
| pressed_modal_pause_home | zero | 20.47 | 0.7102 | 154.82 (pause-level-name y) | FAIL | FAIL | pixels 20.47% < 99.0%; ssim 0.7102 < 0.985; boxes: 70 failing; trace |
| game_lvl01_paused_t1000 | iphone | 20.70 | 0.7011 | 137.14 (pause-level-quota y) | FAIL | FAIL | pixels 20.70% < 99.0%; ssim 0.7011 < 0.985; boxes: 70 failing; trace |
| modal_pause | iphone | 20.70 | 0.7010 | 137.14 (pause-level-quota y) | FAIL | FAIL | pixels 20.70% < 99.0%; ssim 0.7010 < 0.985; boxes: 70 failing; trace |
| modal_pause_muted | iphone | 20.71 | 0.7008 | 137.14 (pause-level-quota y) | FAIL | FAIL | pixels 20.71% < 99.0%; ssim 0.7008 < 0.985; boxes: 70 failing; trace |
| pressed_modal_close | iphone | 20.72 | 0.7013 | 137.14 (pause-level-quota y) | FAIL | FAIL | pixels 20.72% < 99.0%; ssim 0.7013 < 0.985; boxes: 70 failing; trace |
| modal_pause_level_21 | iphone | 20.78 | 0.7030 | 128.74 (btn-pause-resume y) | FAIL | FAIL | pixels 20.78% < 99.0%; ssim 0.7030 < 0.985; boxes: 70 failing; trace |
| settings_modal_privacy | iphone | 20.87 | 0.7032 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 20.87% < 99.0%; ssim 0.7032 < 0.985; boxes: 104 failing; trace |
| pressed_privacy_dismiss | iphone | 20.87 | 0.7020 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 20.87% < 99.0%; ssim 0.7020 < 0.985; boxes: 104 failing; trace |
| pressed_modal_resume | iphone | 21.15 | 0.7048 | 137.14 (pause-level-quota y) | FAIL | FAIL | pixels 21.15% < 99.0%; ssim 0.7048 < 0.985; boxes: 70 failing; trace |
| pressed_modal_pause_home | iphone | 21.31 | 0.7007 | 137.14 (pause-level-quota y) | FAIL | FAIL | pixels 21.31% < 99.0%; ssim 0.7007 < 0.985; boxes: 70 failing; trace |
| daily_two_cleared | zero | 21.64 | 0.6553 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 21.64% < 99.0%; ssim 0.6553 < 0.985; boxes: 100 failing; trace |
| modal_defeat_no_backup_used | zero | 21.87 | 0.7347 | 141.55 (defeat-message y) | FAIL | FAIL | pixels 21.87% < 99.0%; ssim 0.7347 < 0.985; boxes: 70 failing; trace |
| modal_defeat_no_backup_low | zero | 21.88 | 0.7349 | 141.55 (defeat-message y) | FAIL | FAIL | pixels 21.88% < 99.0%; ssim 0.7349 < 0.985; boxes: 70 failing; trace |
| modal_defeat_no_backup_used | iphone | 22.02 | 0.7259 | 136.78 (defeat-message y) | FAIL | FAIL | pixels 22.02% < 99.0%; ssim 0.7259 < 0.985; boxes: 70 failing; trace |
| modal_defeat_no_backup_low | iphone | 22.05 | 0.7262 | 136.78 (defeat-message y) | FAIL | FAIL | pixels 22.05% < 99.0%; ssim 0.7262 < 0.985; boxes: 70 failing; trace |
| modal_debrief_t0150 | zero | 22.51 | 0.7139 | 122.22 (debrief-mass-val w) | FAIL | FAIL | pixels 22.51% < 99.0%; ssim 0.7139 < 0.985; boxes: 70 failing; trace |
| modal_debrief_exact | iphone | 23.05 | 0.6993 | 122.22 (debrief-mass-val w) | FAIL | FAIL | pixels 23.05% < 99.0%; ssim 0.6993 < 0.985; boxes: 70 failing; trace |
| modal_debrief_surplus | iphone | 23.05 | 0.6993 | 122.23 (debrief-mass-val w) | FAIL | FAIL | pixels 23.05% < 99.0%; ssim 0.6993 < 0.985; boxes: 70 failing; trace |
| pressed_debrief_next | iphone | 23.11 | 0.7011 | 122.22 (debrief-mass-val w) | FAIL | FAIL | pixels 23.11% < 99.0%; ssim 0.7011 < 0.985; boxes: 70 failing; trace |
| modal_debrief_exact | zero | 23.19 | 0.7119 | 122.22 (debrief-mass-val w) | FAIL | FAIL | pixels 23.19% < 99.0%; ssim 0.7119 < 0.985; boxes: 70 failing; trace |
| modal_debrief_surplus | zero | 23.19 | 0.7119 | 122.23 (debrief-mass-val w) | FAIL | FAIL | pixels 23.19% < 99.0%; ssim 0.7119 < 0.985; boxes: 70 failing; trace |
| modal_debrief_world3 | iphone | 23.30 | 0.6983 | 116.11 (debrief-mass-val w) | FAIL | FAIL | pixels 23.30% < 99.0%; ssim 0.6983 < 0.985; boxes: 70 failing; trace |
| pressed_debrief_next | zero | 23.37 | 0.7124 | 122.22 (debrief-mass-val w) | FAIL | FAIL | pixels 23.37% < 99.0%; ssim 0.7124 < 0.985; boxes: 70 failing; trace |
| modal_debrief_world3 | zero | 23.38 | 0.7107 | 116.11 (debrief-mass-val w) | FAIL | FAIL | pixels 23.38% < 99.0%; ssim 0.7107 < 0.985; boxes: 70 failing; trace |
| modal_debrief_t0150 | iphone | 23.42 | 0.7090 | 122.22 (debrief-mass-val w) | FAIL | FAIL | pixels 23.42% < 99.0%; ssim 0.7090 < 0.985; boxes: 70 failing; trace |
| settings_modal_reset_confirm | iphone | 23.44 | 0.7513 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 23.44% < 99.0%; ssim 0.7513 < 0.985; boxes: 104 failing; trace |
| modal_debrief_world5 | iphone | 23.46 | 0.6951 | 112.36 (debrief-mass-val w) | FAIL | FAIL | pixels 23.46% < 99.0%; ssim 0.6951 < 0.985; boxes: 70 failing; trace |
| modal_debrief_world5 | zero | 23.57 | 0.7076 | 112.36 (debrief-mass-val w) | FAIL | FAIL | pixels 23.57% < 99.0%; ssim 0.7076 < 0.985; boxes: 70 failing; trace |
| pressed_debrief_replay | zero | 23.63 | 0.7116 | 122.22 (debrief-mass-val w) | FAIL | FAIL | pixels 23.63% < 99.0%; ssim 0.7116 < 0.985; boxes: 70 failing; trace |
| settings_modal_privacy_t0150 | iphone | 23.94 | 0.7244 | 113.86 (btn-reset-game-data y) | FAIL | FAIL | pixels 23.94% < 99.0%; ssim 0.7244 < 0.985; boxes: 104 failing; trace |
| modal_defeat_t0150 | zero | 24.06 | 0.7392 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 24.06% < 99.0%; ssim 0.7392 < 0.985; boxes: 72 failing; trace |
| pressed_debrief_replay | iphone | 24.13 | 0.6994 | 122.22 (debrief-mass-val w) | FAIL | FAIL | pixels 24.13% < 99.0%; ssim 0.6994 < 0.985; boxes: 70 failing; trace |
| daily_bronze_cleared | zero | 24.66 | 0.6492 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 24.66% < 99.0%; ssim 0.6492 < 0.985; boxes: 100 failing; trace |
| maps_world_4 | zero | 24.72 | 0.6972 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 24.72% < 99.0%; ssim 0.6972 < 0.985; boxes: 112 failing; trace |
| maps_world_5_scrolled | zero | 24.76 | 0.6974 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 24.76% < 99.0%; ssim 0.6974 < 0.985; boxes: 112 failing; trace |
| maps_world_5 | zero | 24.77 | 0.6972 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 24.77% < 99.0%; ssim 0.6972 < 0.985; boxes: 112 failing; trace |
| maps_world_3 | zero | 24.84 | 0.6995 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 24.84% < 99.0%; ssim 0.6995 < 0.985; boxes: 112 failing; trace |
| maps_world_1 | zero | 24.91 | 0.6994 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 24.91% < 99.0%; ssim 0.6994 < 0.985; boxes: 112 failing; trace |
| maps_world_2 | zero | 24.93 | 0.7014 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 24.93% < 99.0%; ssim 0.7014 < 0.985; boxes: 112 failing; trace |
| pressed_level_tile | zero | 25.53 | 0.6914 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 25.53% < 99.0%; ssim 0.6914 < 0.985; boxes: 112 failing; trace |
| modal_defeat_t0150 | iphone | 25.63 | 0.7427 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 25.63% < 99.0%; ssim 0.7427 < 0.985; boxes: 72 failing; trace |
| maps_world_1_all_complete | zero | 25.75 | 0.7023 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 25.75% < 99.0%; ssim 0.7023 < 0.985; boxes: 112 failing; trace |
| maps_world_4 | iphone | 25.92 | 0.6988 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 25.92% < 99.0%; ssim 0.6988 < 0.985; boxes: 112 failing; trace |
| maps_world_5 | iphone | 25.94 | 0.6990 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 25.94% < 99.0%; ssim 0.6990 < 0.985; boxes: 112 failing; trace |
| maps_world_5_scrolled | iphone | 26.08 | 0.7009 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 26.08% < 99.0%; ssim 0.7009 < 0.985; boxes: 112 failing; trace |
| maps_world_3 | iphone | 26.15 | 0.7022 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 26.15% < 99.0%; ssim 0.7022 < 0.985; boxes: 112 failing; trace |
| pressed_back_daily | zero | 26.26 | 0.6423 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 26.26% < 99.0%; ssim 0.6423 < 0.985; boxes: 99 failing; trace |
| maps_world_2 | iphone | 26.29 | 0.7050 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 26.29% < 99.0%; ssim 0.7050 < 0.985; boxes: 112 failing; trace |
| maps_world_1 | iphone | 26.33 | 0.7026 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 26.33% < 99.0%; ssim 0.7026 < 0.985; boxes: 112 failing; trace |
| pressed_daily_start_shift | zero | 26.53 | 0.6416 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 26.53% < 99.0%; ssim 0.6416 < 0.985; boxes: 99 failing; trace |
| daily_after_two_minutes | zero | 26.55 | 0.6423 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 26.55% < 99.0%; ssim 0.6423 < 0.985; boxes: 99 failing; trace |
| daily_default | zero | 26.55 | 0.6423 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 26.55% < 99.0%; ssim 0.6423 < 0.985; boxes: 99 failing; trace |
| daily_scrolled | zero | 26.55 | 0.6423 | 43.00 (daily-page-container w) | FAIL | FAIL | pixels 26.55% < 99.0%; ssim 0.6423 < 0.985; boxes: 99 failing; trace |
| pressed_level_tile | iphone | 26.64 | 0.6972 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 26.64% < 99.0%; ssim 0.6972 < 0.985; boxes: 112 failing; trace |
| pressed_daily_start_shift | iphone | 27.22 | 0.6528 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 27.22% < 99.0%; ssim 0.6528 < 0.985; boxes: 99 failing; trace |
| pressed_back_maps | zero | 27.27 | 0.6936 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 27.27% < 99.0%; ssim 0.6936 < 0.985; boxes: 112 failing; trace |
| pressed_back_daily | iphone | 27.30 | 0.6537 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 27.30% < 99.0%; ssim 0.6537 < 0.985; boxes: 99 failing; trace |
| daily_after_two_minutes | iphone | 27.55 | 0.6539 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 27.55% < 99.0%; ssim 0.6539 < 0.985; boxes: 99 failing; trace |
| daily_default | iphone | 27.55 | 0.6538 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 27.55% < 99.0%; ssim 0.6538 < 0.985; boxes: 99 failing; trace |
| daily_scrolled | iphone | 27.55 | 0.6538 | 64.68 (daily-page-container h) | FAIL | FAIL | pixels 27.55% < 99.0%; ssim 0.6538 < 0.985; boxes: 99 failing; trace |
| pressed_world_tab | zero | 27.56 | 0.6935 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 27.56% < 99.0%; ssim 0.6935 < 0.985; boxes: 112 failing; trace |
| pressed_defeat_retry | zero | 27.58 | 0.7280 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 27.58% < 99.0%; ssim 0.7280 < 0.985; boxes: 72 failing; trace |
| modal_defeat_backup | zero | 27.70 | 0.7280 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 27.70% < 99.0%; ssim 0.7280 < 0.985; boxes: 72 failing; trace |
| maps_world_3_none_complete | zero | 27.70 | 0.6953 | 43.00 (sectors-page-container w) | FAIL | FAIL | pixels 27.70% < 99.0%; ssim 0.6953 < 0.985; boxes: 112 failing; trace |
| pressed_defeat_backup | zero | 27.90 | 0.7286 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 27.90% < 99.0%; ssim 0.7286 < 0.985; boxes: 72 failing; trace |
| maps_world_1_all_complete | iphone | 28.00 | 0.7018 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 28.00% < 99.0%; ssim 0.7018 < 0.985; boxes: 112 failing; trace |
| pressed_defeat_home | zero | 28.12 | 0.7277 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 28.12% < 99.0%; ssim 0.7277 < 0.985; boxes: 72 failing; trace |
| modal_defeat_backup | iphone | 28.44 | 0.7178 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 28.44% < 99.0%; ssim 0.7178 < 0.985; boxes: 72 failing; trace |
| pressed_defeat_retry | iphone | 28.47 | 0.7190 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 28.47% < 99.0%; ssim 0.7190 < 0.985; boxes: 72 failing; trace |
| pressed_back_maps | iphone | 28.60 | 0.6986 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 28.60% < 99.0%; ssim 0.6986 < 0.985; boxes: 112 failing; trace |
| pressed_defeat_backup | iphone | 28.66 | 0.7188 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 28.66% < 99.0%; ssim 0.7188 < 0.985; boxes: 72 failing; trace |
| maps_world_3_none_complete | iphone | 28.80 | 0.7006 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 28.80% < 99.0%; ssim 0.7006 < 0.985; boxes: 112 failing; trace |
| pressed_world_tab | iphone | 28.85 | 0.6987 | 64.68 (sectors-page-container h) | FAIL | FAIL | pixels 28.85% < 99.0%; ssim 0.6987 < 0.985; boxes: 112 failing; trace |
| pressed_defeat_home | iphone | 28.95 | 0.7169 | 133.83 (defeat-spilled-val w) | FAIL | FAIL | pixels 28.95% < 99.0%; ssim 0.7169 < 0.985; boxes: 72 failing; trace |
| modal_debrief_daily_trophy | zero | 32.25 | 0.6733 | 122.51 (debrief-mass-val w) | FAIL | FAIL | pixels 32.25% < 99.0%; ssim 0.6733 < 0.985; boxes: 77 failing; trace |
| modal_debrief_daily_trophy | iphone | 32.74 | 0.6636 | 122.51 (debrief-mass-val w) | FAIL | FAIL | pixels 32.74% < 99.0%; ssim 0.6636 < 0.985; boxes: 77 failing; trace |
| settings_reset_confirmed_toast | iphone | 34.35 | 0.7178 | - | FAIL | FAIL | pixels 34.35% < 99.0%; ssim 0.7178 < 0.985; boxes: 127 failing; trace |
| settings_reset_confirmed_toast | zero | 36.13 | 0.7255 | - | FAIL | FAIL | pixels 36.13% < 99.0%; ssim 0.7255 < 0.985; boxes: 127 failing; trace |

## Failing elements

### loading_t0000__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| loading-mascot-box#1 | missing_in_unity | - | - | - | - |
| loading-mascot-anim#1 | missing_in_unity | - | - | - | - |
| loading-title-box#1 | missing_in_unity | - | - | - | - |
| loading-bar-wrapper#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| ... 40 more | | | | | |

### loading_t0000__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| loading-mascot-box#1 | missing_in_unity | - | - | - | - |
| loading-mascot-anim#1 | missing_in_unity | - | - | - | - |
| loading-title-box#1 | missing_in_unity | - | - | - | - |
| loading-bar-wrapper#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| ... 40 more | | | | | |

### pressed_back_achievements__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#4 | missing_in_unity | - | - | - | - |
| page-header-title#4 | missing_in_unity | - | - | - | - |
| page-scroll-body#5 | missing_in_unity | - | - | - | - |
| ach-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| ach-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| ach-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| card-desc#3 | missing_in_unity | - | - | - | - |
| card-footer#3 | missing_in_unity | - | - | - | - |
| badge-cost#3 | missing_in_unity | - | - | - | - |
| ach-card#4 | missing_in_unity | - | - | - | - |
| card-title-row#4 | missing_in_unity | - | - | - | - |
| card-title#4 | missing_in_unity | - | - | - | - |
| card-desc#4 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### pressed_back_achievements__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#4 | missing_in_unity | - | - | - | - |
| page-header-title#4 | missing_in_unity | - | - | - | - |
| page-scroll-body#5 | missing_in_unity | - | - | - | - |
| ach-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| ach-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| ach-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| card-desc#3 | missing_in_unity | - | - | - | - |
| card-footer#3 | missing_in_unity | - | - | - | - |
| badge-cost#3 | missing_in_unity | - | - | - | - |
| ach-card#4 | missing_in_unity | - | - | - | - |
| card-title-row#4 | missing_in_unity | - | - | - | - |
| card-title#4 | missing_in_unity | - | - | - | - |
| card-desc#4 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### achievements_all__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#4 | missing_in_unity | - | - | - | - |
| page-header-title#4 | missing_in_unity | - | - | - | - |
| page-scroll-body#5 | missing_in_unity | - | - | - | - |
| ach-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| ach-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| ach-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| card-desc#3 | missing_in_unity | - | - | - | - |
| card-footer#3 | missing_in_unity | - | - | - | - |
| badge-cost#3 | missing_in_unity | - | - | - | - |
| ach-card#4 | missing_in_unity | - | - | - | - |
| card-title-row#4 | missing_in_unity | - | - | - | - |
| card-title#4 | missing_in_unity | - | - | - | - |
| card-desc#4 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### achievements_none__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#4 | missing_in_unity | - | - | - | - |
| page-header-title#4 | missing_in_unity | - | - | - | - |
| page-scroll-body#5 | missing_in_unity | - | - | - | - |
| ach-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| ach-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| ach-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| card-desc#3 | missing_in_unity | - | - | - | - |
| card-footer#3 | missing_in_unity | - | - | - | - |
| badge-cost#3 | missing_in_unity | - | - | - | - |
| ach-card#4 | missing_in_unity | - | - | - | - |
| card-title-row#4 | missing_in_unity | - | - | - | - |
| card-title#4 | missing_in_unity | - | - | - | - |
| card-desc#4 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### achievements_some__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#4 | missing_in_unity | - | - | - | - |
| page-header-title#4 | missing_in_unity | - | - | - | - |
| page-scroll-body#5 | missing_in_unity | - | - | - | - |
| ach-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| ach-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| ach-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| card-desc#3 | missing_in_unity | - | - | - | - |
| card-footer#3 | missing_in_unity | - | - | - | - |
| badge-cost#3 | missing_in_unity | - | - | - | - |
| ach-card#4 | missing_in_unity | - | - | - | - |
| card-title-row#4 | missing_in_unity | - | - | - | - |
| card-title#4 | missing_in_unity | - | - | - | - |
| card-desc#4 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### pressed_back_settings__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### achievements_some__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#4 | missing_in_unity | - | - | - | - |
| page-header-title#4 | missing_in_unity | - | - | - | - |
| page-scroll-body#5 | missing_in_unity | - | - | - | - |
| ach-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| ach-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| ach-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| card-desc#3 | missing_in_unity | - | - | - | - |
| card-footer#3 | missing_in_unity | - | - | - | - |
| badge-cost#3 | missing_in_unity | - | - | - | - |
| ach-card#4 | missing_in_unity | - | - | - | - |
| card-title-row#4 | missing_in_unity | - | - | - | - |
| card-title#4 | missing_in_unity | - | - | - | - |
| card-desc#4 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### achievements_none__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#4 | missing_in_unity | - | - | - | - |
| page-header-title#4 | missing_in_unity | - | - | - | - |
| page-scroll-body#5 | missing_in_unity | - | - | - | - |
| ach-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| ach-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| ach-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| card-desc#3 | missing_in_unity | - | - | - | - |
| card-footer#3 | missing_in_unity | - | - | - | - |
| badge-cost#3 | missing_in_unity | - | - | - | - |
| ach-card#4 | missing_in_unity | - | - | - | - |
| card-title-row#4 | missing_in_unity | - | - | - | - |
| card-title#4 | missing_in_unity | - | - | - | - |
| card-desc#4 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### achievements_all__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#4 | missing_in_unity | - | - | - | - |
| page-header-title#4 | missing_in_unity | - | - | - | - |
| page-scroll-body#5 | missing_in_unity | - | - | - | - |
| ach-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| ach-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| ach-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| card-desc#3 | missing_in_unity | - | - | - | - |
| card-footer#3 | missing_in_unity | - | - | - | - |
| badge-cost#3 | missing_in_unity | - | - | - | - |
| ach-card#4 | missing_in_unity | - | - | - | - |
| card-title-row#4 | missing_in_unity | - | - | - | - |
| card-title#4 | missing_in_unity | - | - | - | - |
| card-desc#4 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### settings_toggles_off__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### pressed_back_settings__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_default__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_scrolled__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_sliders_moved__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_sliders_profile__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_toggles_clicked__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_default__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_scrolled__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_toggles_off__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_sliders_moved__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_sliders_profile__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### settings_toggles_clicked__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### modal_pause_closing_t0150__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### modal_pause_closing_t0150__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### pressed_hud_pause__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### pressed_hud_restart__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_pickup__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| ... 48 more | | | | | |

### game_lvl01_restart_toast__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 48 more | | | | | |

### game_lvl01_skins_kitty_party_mint__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_quota_met_hud__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_near_victory__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_skins_kitty_party_mint__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### pressed_hud_pause__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### pressed_hud_restart__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_t0016ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_quota_met_hud__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_near_victory__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl11_t0016ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_t0016ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_hint_dismissing_t0200__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_t1000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_hint_visible__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### game_lvl01_mid_drag__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_t3000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_gate_pass__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl21_t0016ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_restart_toast__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 48 more | | | | | |

### game_lvl01_hint_dismissing_t0200__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl11_skins_duckie_berry_cheese__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl11_t0016ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_hint_visible__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### game_lvl11_skins_duckie_berry_cheese__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl11_t3000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_t1000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_mid_drag__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_gate_pass__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_t3000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl11_t1000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl31_t0016ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl01_pickup__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| ... 48 more | | | | | |

### game_lvl21_t0016ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl41_t0016ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl21_t1000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl11_t3000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl41_t1000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl31_t1000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl31_t3000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl11_t1000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl31_t0016ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl41_t3000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl21_t1000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl41_t0016ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl31_t1000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl41_t1000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl31_t3000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### game_lvl41_t3000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 46 more | | | | | |

### shop_buy_insufficient_toast__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 77 more | | | | | |

### shop_buy_success_toast__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 77 more | | | | | |

### shop_equip_toast__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 77 more | | | | | |

### pressed_back_shop__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_skyboxes_default__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### shop_skyboxes_mixed__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### pressed_claim_gift__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_singularities_default__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_rings_default__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_scrolled__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_singularities_mixed__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### pressed_shop_buy__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### pressed_shop_tab__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_rings_mixed__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### daily_master_claim_toast__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 76 more | | | | | |

### daily_master_claimed__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### pressed_back_shop__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_skyboxes_mixed__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### shop_skyboxes_default__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 69 more | | | | | |

### shop_singularities_mixed__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_singularities_default__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### pressed_claim_gift__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### game_lvl21_t3000ms__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### shop_rings_mixed__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_rings_default__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_scrolled__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### pressed_shop_buy__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### pressed_shop_tab__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_buy_insufficient_toast__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 77 more | | | | | |

### shop_buy_success_toast__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 77 more | | | | | |

### modal_defeat_backup_used_toast__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 49 more | | | | | |

### shop_equip_toast__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-secondary#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 77 more | | | | | |

### game_lvl21_t3000ms__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### game_daily_bronze_t1000__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-game | visibility | - | - | - | - |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| btn-game-pause | visibility | - | - | - | - |
| hud-quota-capsule | visibility | - | - | - | - |
| hud-sector-code | visibility | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-text | visibility | - | - | - | - |
| btn-game-restart | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| page-daily | visibility | - | - | - | - |
| btn-back-daily | visibility | - | - | - | - |
| daily-credits-val | visibility | - | - | - | - |
| daily-page-container | visibility | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| ... 52 more | | | | | |

### modal_defeat_backup_used_toast__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 49 more | | | | | |

### shop_claim_gift_toast__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 77 more | | | | | |

### shop_gift_claimed__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### daily_master_ready__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| btn-claim-master-bonus | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_gift_claimed__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### shop_claim_gift_toast__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#3 | missing_in_unity | - | - | - | - |
| page-header-title#3 | missing_in_unity | - | - | - | - |
| page-scroll-body#4 | missing_in_unity | - | - | - | - |
| gift-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| gift-banner-info#1 | missing_in_unity | - | - | - | - |
| gift-banner-tag#1 | missing_in_unity | - | - | - | - |
| tab-row#1 | missing_in_unity | - | - | - | - |
| tab-btn#1 | missing_in_unity | - | - | - | - |
| tab-btn#2 | missing_in_unity | - | - | - | - |
| tab-btn#3 | missing_in_unity | - | - | - | - |
| store-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| card-desc#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-equipped#1 | missing_in_unity | - | - | - | - |
| store-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| card-title#2 | missing_in_unity | - | - | - | - |
| card-desc#2 | missing_in_unity | - | - | - | - |
| card-footer#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| store-card#3 | missing_in_unity | - | - | - | - |
| card-title-row#3 | missing_in_unity | - | - | - | - |
| card-title#3 | missing_in_unity | - | - | - | - |
| ... 77 more | | | | | |

### game_daily_bronze_t1000__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-game | visibility | - | - | - | - |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| btn-game-pause | visibility | - | - | - | - |
| hud-quota-capsule | visibility | - | - | - | - |
| hud-sector-code | visibility | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-text | visibility | - | - | - | - |
| btn-game-restart | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| page-daily | visibility | - | - | - | - |
| btn-back-daily | visibility | - | - | - | - |
| daily-credits-val | visibility | - | - | - | - |
| daily-page-container | visibility | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| ... 52 more | | | | | |

### daily_two_cleared__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### modal_pause_t0150__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_pause_t0150__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### pressed_dock_trophies__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_dock_maps__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_dock_shop__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_dock_daily__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_anim_t02500__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### toast_t2300__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### home_sound_toggle_toast__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### toast_t1000__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### home_anim_t12000__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### toast_long__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### home_equipped_kitty_confetti__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_anim_t00900__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### toast_stacked__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 94 more | | | | | |

### home_equipped_duckie_berry__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_mascot_click_t0700__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| spawned-particle#1 | missing_in_unity | - | - | - | - |
| ... 92 more | | | | | |

### home_equipped_candy_sleepy__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_home_settings__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### toast_t0200__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### pressed_home_audio__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_anim_t00400__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_mascot_click_t0300__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| spawned-particle#1 | missing_in_unity | - | - | - | - |
| ... 92 more | | | | | |

### home_anim_t06000__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_sector_21_big_credits__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_badges_two_new__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_default__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_mascot_click_t0016__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| spawned-particle#1 | missing_in_unity | - | - | - | - |
| ... 92 more | | | | | |

### home_badges_master_ready__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_badges_all_claimed__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### settings_modal_privacy__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### pressed_privacy_dismiss__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### loading_t0500__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| loading-mascot-box#1 | missing_in_unity | - | - | - | - |
| loading-mascot-anim#1 | missing_in_unity | - | - | - | - |
| loading-title-box#1 | missing_in_unity | - | - | - | - |
| loading-bar-wrapper#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| ... 39 more | | | | | |

### pressed_play_mission__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### loading_t1100__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| loading-mascot-box#1 | missing_in_unity | - | - | - | - |
| loading-mascot-anim#1 | missing_in_unity | - | - | - | - |
| loading-title-box#1 | missing_in_unity | - | - | - | - |
| loading-bar-wrapper#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| ... 39 more | | | | | |

### loading_t1100__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| loading-mascot-box#1 | missing_in_unity | - | - | - | - |
| loading-mascot-anim#1 | missing_in_unity | - | - | - | - |
| loading-title-box#1 | missing_in_unity | - | - | - | - |
| loading-bar-wrapper#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| ... 39 more | | | | | |

### loading_t0500__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| loading-mascot-box#1 | missing_in_unity | - | - | - | - |
| loading-mascot-anim#1 | missing_in_unity | - | - | - | - |
| loading-title-box#1 | missing_in_unity | - | - | - | - |
| loading-bar-wrapper#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| ... 39 more | | | | | |

### daily_master_ready__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| btn-claim-master-bonus | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#2 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### daily_master_claim_toast__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 76 more | | | | | |

### daily_master_claimed__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### modal_pause_level_21__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### settings_modal_privacy_t0150__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### pressed_dock_trophies__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_dock_maps__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_dock_shop__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_dock_daily__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### settings_modal_reset_confirm__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### game_lvl01_paused_t1000__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_pause_muted__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_pause__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### pressed_modal_close__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### home_sector_21_big_credits__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_badges_two_new__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_default__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_badges_master_ready__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_mascot_click_t0016__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| spawned-particle#1 | missing_in_unity | - | - | - | - |
| ... 92 more | | | | | |

### home_badges_all_claimed__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_equipped_kitty_confetti__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### toast_t2300__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### home_anim_t02500__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_equipped_duckie_berry__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### daily_bronze_cleared__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### home_equipped_candy_sleepy__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_home_settings__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_home_audio__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_modal_resume__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### home_mascot_click_t0700__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| spawned-particle#1 | missing_in_unity | - | - | - | - |
| ... 92 more | | | | | |

### home_anim_t00900__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_anim_t00400__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_mascot_click_t0300__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| spawned-particle#1 | missing_in_unity | - | - | - | - |
| ... 92 more | | | | | |

### home_anim_t06000__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### home_anim_t12000__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### pressed_play_mission__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 91 more | | | | | |

### toast_long__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### home_sound_toggle_toast__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### toast_t1000__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### toast_stacked__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 94 more | | | | | |

### toast_t0200__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| home-space-bg#1 | missing_in_unity | - | - | - | - |
| bg-debris#1 | missing_in_unity | - | - | - | - |
| bg-debris#2 | missing_in_unity | - | - | - | - |
| bg-debris#3 | missing_in_unity | - | - | - | - |
| bg-debris#4 | missing_in_unity | - | - | - | - |
| bg-debris#5 | missing_in_unity | - | - | - | - |
| bg-debris#6 | missing_in_unity | - | - | - | - |
| home-top-bar#1 | missing_in_unity | - | - | - | - |
| home-top-actions#1 | missing_in_unity | - | - | - | - |
| home-audio-icon | missing_in_unity | - | - | - | - |
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
| pedestal-glow-beam#1 | missing_in_unity | - | - | - | - |
| diorama-sunburst#1 | missing_in_unity | - | - | - | - |
| home-diorama-face | missing_in_unity | - | - | - | - |
| home-launch-console#1 | missing_in_unity | - | - | - | - |
| ... 93 more | | | | | |

### pressed_modal_pause_home__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### game_lvl01_paused_t1000__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_pause__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_pause_muted__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### pressed_modal_close__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_pause_level_21__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### settings_modal_privacy__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### pressed_privacy_dismiss__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### pressed_modal_resume__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### pressed_modal_pause_home__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| modal-header#2 | missing_in_unity | - | - | - | - |
| modal-title#2 | missing_in_unity | - | - | - | - |
| btn-close-privacy | missing_in_unity | - | - | - | - |
| modal-body#2 | missing_in_unity | - | - | - | - |
| btn-dismiss-privacy | missing_in_unity | - | - | - | - |
| modal-reset-confirm | missing_in_unity | - | - | - | - |
| modal-card#3 | missing_in_unity | - | - | - | - |
| modal-header#3 | missing_in_unity | - | - | - | - |
| modal-title#3 | missing_in_unity | - | - | - | - |
| modal-body#3 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### daily_two_cleared__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### modal_defeat_no_backup_used__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_defeat_no_backup_low__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_defeat_no_backup_used__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_defeat_no_backup_low__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_t0150__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_exact__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_surplus__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### pressed_debrief_next__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_exact__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_surplus__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_world3__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### pressed_debrief_next__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_world3__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_t0150__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### settings_modal_reset_confirm__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### modal_debrief_world5__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### modal_debrief_world5__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### pressed_debrief_replay__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### settings_modal_privacy_t0150__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#5 | missing_in_unity | - | - | - | - |
| page-header-title#5 | missing_in_unity | - | - | - | - |
| page-scroll-body#6 | missing_in_unity | - | - | - | - |
| settings-section-card#1 | missing_in_unity | - | - | - | - |
| settings-section-title#1 | missing_in_unity | - | - | - | - |
| settings-row#1 | missing_in_unity | - | - | - | - |
| settings-label-box#1 | missing_in_unity | - | - | - | - |
| settings-label#1 | missing_in_unity | - | - | - | - |
| settings-sublabel#1 | missing_in_unity | - | - | - | - |
| cartoon-switch#1 | missing_in_unity | - | - | - | - |
| settings-sound-toggle | visibility | - | - | - | - |
| slider-round#1 | missing_in_unity | - | - | - | - |
| settings-row#2 | missing_in_unity | - | - | - | - |
| settings-label-box#2 | missing_in_unity | - | - | - | - |
| settings-label#2 | missing_in_unity | - | - | - | - |
| settings-sublabel#2 | missing_in_unity | - | - | - | - |
| settings-row#3 | missing_in_unity | - | - | - | - |
| settings-label-box#3 | missing_in_unity | - | - | - | - |
| settings-label#3 | missing_in_unity | - | - | - | - |
| settings-sublabel#3 | missing_in_unity | - | - | - | - |
| settings-section-card#2 | missing_in_unity | - | - | - | - |
| settings-section-title#2 | missing_in_unity | - | - | - | - |
| settings-row#4 | missing_in_unity | - | - | - | - |
| settings-label-box#4 | missing_in_unity | - | - | - | - |
| settings-label#4 | missing_in_unity | - | - | - | - |
| ... 79 more | | | | | |

### modal_defeat_t0150__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### pressed_debrief_replay__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 45 more | | | | | |

### daily_bronze_cleared__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-progress-fill#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| ... 75 more | | | | | |

### maps_world_4__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_5_scrolled__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_5__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_3__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_1__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_2__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_level_tile__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### modal_defeat_t0150__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### maps_world_1_all_complete__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_4__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_5__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_5_scrolled__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_3__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_back_daily__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### maps_world_2__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### maps_world_1__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_daily_start_shift__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### daily_after_two_minutes__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### daily_default__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### daily_scrolled__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### pressed_level_tile__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_daily_start_shift__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### pressed_back_maps__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_back_daily__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### daily_after_two_minutes__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### daily_default__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### daily_scrolled__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#2 | missing_in_unity | - | - | - | - |
| page-header-title#2 | missing_in_unity | - | - | - | - |
| daily-hub-banner#1 | missing_in_unity | - | - | - | - |
| daily-hub-header#1 | missing_in_unity | - | - | - | - |
| daily-hub-title#1 | missing_in_unity | - | - | - | - |
| daily-hub-pill#1 | missing_in_unity | - | - | - | - |
| daily-progress-container#1 | missing_in_unity | - | - | - | - |
| daily-progress-track#1 | missing_in_unity | - | - | - | - |
| daily-master-box | missing_in_unity | - | - | - | - |
| daily-master-info#1 | missing_in_unity | - | - | - | - |
| daily-master-icon#1 | missing_in_unity | - | - | - | - |
| daily-master-label#1 | missing_in_unity | - | - | - | - |
| daily-master-sub#1 | missing_in_unity | - | - | - | - |
| badge-cost#1 | missing_in_unity | - | - | - | - |
| sector-card#1 | missing_in_unity | - | - | - | - |
| card-title-row#1 | missing_in_unity | - | - | - | - |
| daily-tier-tag#1 | missing_in_unity | - | - | - | - |
| card-title#1 | missing_in_unity | - | - | - | - |
| daily-hazard-chip#1 | missing_in_unity | - | - | - | - |
| card-footer#1 | missing_in_unity | - | - | - | - |
| badge-cost#2 | missing_in_unity | - | - | - | - |
| btn-cartoon-primary#1 | missing_in_unity | - | - | - | - |
| sector-card#2 | missing_in_unity | - | - | - | - |
| card-title-row#2 | missing_in_unity | - | - | - | - |
| daily-tier-tag#2 | missing_in_unity | - | - | - | - |
| ... 74 more | | | | | |

### pressed_world_tab__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_defeat_retry__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### modal_defeat_backup__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### maps_world_3_none_complete__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_defeat_backup__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### maps_world_1_all_complete__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_defeat_home__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### modal_defeat_backup__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### pressed_defeat_retry__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### pressed_back_maps__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_defeat_backup__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### maps_world_3_none_complete__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_world_tab__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-header-bar#1 | missing_in_unity | - | - | - | - |
| page-header-title#1 | missing_in_unity | - | - | - | - |
| world-tabs-row#1 | missing_in_unity | - | - | - | - |
| active#1 | missing_in_unity | - | - | - | - |
| world-banner-card#1 | missing_in_unity | - | - | - | - |
| world-banner-icon-box#1 | missing_in_unity | - | - | - | - |
| world-banner-info#1 | missing_in_unity | - | - | - | - |
| world-banner-title#1 | missing_in_unity | - | - | - | - |
| world-banner-desc#1 | missing_in_unity | - | - | - | - |
| world-banner-progress#1 | missing_in_unity | - | - | - | - |
| level-grid-50#1 | missing_in_unity | - | - | - | - |
| level-tile-header#1 | missing_in_unity | - | - | - | - |
| level-badge#1 | missing_in_unity | - | - | - | - |
| level-name#1 | missing_in_unity | - | - | - | - |
| level-quota#1 | missing_in_unity | - | - | - | - |
| level-tile-header#2 | missing_in_unity | - | - | - | - |
| level-badge#2 | missing_in_unity | - | - | - | - |
| level-name#2 | missing_in_unity | - | - | - | - |
| level-quota#2 | missing_in_unity | - | - | - | - |
| level-tile-header#3 | missing_in_unity | - | - | - | - |
| level-badge#3 | missing_in_unity | - | - | - | - |
| level-name#3 | missing_in_unity | - | - | - | - |
| level-quota#3 | missing_in_unity | - | - | - | - |
| level-tile-header#4 | missing_in_unity | - | - | - | - |
| level-badge#4 | missing_in_unity | - | - | - | - |
| ... 87 more | | | | | |

### pressed_defeat_home__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-bar | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| pause-level-quota | visibility | - | - | - | - |
| btn-pause-resume | visibility | - | - | - | - |
| btn-pause-restart | visibility | - | - | - | - |
| btn-pause-sound | visibility | - | - | - | - |
| pause-sound-icon | visibility | - | - | - | - |
| btn-pause-maps | visibility | - | - | - | - |
| btn-pause-home | visibility | - | - | - | - |
| modal-privacy | missing_in_unity | - | - | - | - |
| modal-card#2 | missing_in_unity | - | - | - | - |
| ... 47 more | | | | | |

### modal_debrief_daily_trophy__zero

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-game | visibility | - | - | - | - |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| btn-game-pause | visibility | - | - | - | - |
| hud-quota-capsule | visibility | - | - | - | - |
| hud-sector-code | visibility | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-text | visibility | - | - | - | - |
| btn-game-restart | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| page-daily | visibility | - | - | - | - |
| btn-back-daily | visibility | - | - | - | - |
| daily-credits-val | visibility | - | - | - | - |
| daily-page-container | visibility | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| ... 52 more | | | | | |

### modal_debrief_daily_trophy__iphone

| element | reason | axis | delta px | web rect | unity rect |
| --- | --- | --- | --- | --- | --- |
| page-game | visibility | - | - | - | - |
| game-canvas | missing_in_unity | - | - | - | - |
| hud-top-mobile#1 | missing_in_unity | - | - | - | - |
| btn-game-pause | visibility | - | - | - | - |
| hud-quota-capsule | visibility | - | - | - | - |
| hud-sector-code | visibility | - | - | - | - |
| capsule-progress-area#1 | missing_in_unity | - | - | - | - |
| capsule-bar-track#1 | missing_in_unity | - | - | - | - |
| hud-quota-text | visibility | - | - | - | - |
| btn-game-restart | visibility | - | - | - | - |
| gesture-hint | missing_in_unity | - | - | - | - |
| hand-swipe-anim#1 | missing_in_unity | - | - | - | - |
| hint-text#1 | missing_in_unity | - | - | - | - |
| page-daily | visibility | - | - | - | - |
| btn-back-daily | visibility | - | - | - | - |
| daily-credits-val | visibility | - | - | - | - |
| daily-page-container | visibility | - | - | - | - |
| modal-pause | visibility | - | - | - | - |
| modal-card#1 | missing_in_unity | - | - | - | - |
| modal-header#1 | missing_in_unity | - | - | - | - |
| modal-title#1 | missing_in_unity | - | - | - | - |
| btn-close-pause | visibility | - | - | - | - |
| modal-body#1 | missing_in_unity | - | - | - | - |
| pause-level-info#1 | missing_in_unity | - | - | - | - |
| pause-level-name | visibility | - | - | - | - |
| ... 52 more | | | | | |

### settings_reset_confirmed_toast__iphone

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
| ... 102 more | | | | | |

### settings_reset_confirmed_toast__zero

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
| ... 102 more | | | | | |

## Traces

| sequence | verdict | detail |
| --- | --- | --- |
| achievements_all | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| achievements_none | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| achievements_some | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| boot_to_home | MISSING | MISSING (unity) |
| daily_after_two_minutes | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| daily_bronze_cleared | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| daily_default | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| daily_gift_claim | MISSING | MISSING (unity) |
| daily_master_claim_toast | FAIL | FAIL (46 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| daily_master_claimed | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| daily_master_ready | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| daily_scrolled | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| daily_two_cleared | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| defeat_sector_1 | MISSING | MISSING (unity) |
| game_daily_bronze_t1000 | FAIL | FAIL (1581 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_gate_pass | FAIL | FAIL (1565 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_hint_dismissing_t0200 | FAIL | FAIL (30 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_hint_visible | FAIL | FAIL (10 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_mid_drag | FAIL | FAIL (1163 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_near_victory | FAIL | FAIL (66 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_paused_t1000 | FAIL | FAIL (65 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_pickup | FAIL | FAIL (3435 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_quota_met_hud | FAIL | FAIL (66 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_restart_toast | FAIL | FAIL (396 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_skins_kitty_party_mint | FAIL | FAIL (607 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_t0016ms | FAIL | FAIL (66 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_t1000ms | FAIL | FAIL (607 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl01_t3000ms | FAIL | FAIL (2236 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl11_skins_duckie_berry_cheese | FAIL | FAIL (844 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl11_t0016ms | FAIL | FAIL (130 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl11_t1000ms | FAIL | FAIL (844 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl11_t3000ms | FAIL | FAIL (2639 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl21_t0016ms | FAIL | FAIL (137 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl21_t1000ms | FAIL | FAIL (729 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl21_t3000ms | FAIL | FAIL (2691 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl31_t0016ms | FAIL | FAIL (178 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl31_t1000ms | FAIL | FAIL (792 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl31_t3000ms | FAIL | FAIL (2949 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl41_t0016ms | FAIL | FAIL (130 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl41_t1000ms | FAIL | FAIL (913 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| game_lvl41_t3000ms | FAIL | FAIL (3515 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_anim_t00400 | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_anim_t00900 | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_anim_t02500 | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_anim_t06000 | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_anim_t12000 | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_badges_all_claimed | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_badges_master_ready | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_badges_two_new | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_default | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_equipped_candy_sleepy | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_equipped_duckie_berry | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_equipped_kitty_confetti | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_mascot_click_t0016 | FAIL | FAIL (41 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_mascot_click_t0300 | FAIL | FAIL (41 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_mascot_click_t0700 | FAIL | FAIL (42 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_sector_21_big_credits | FAIL | FAIL (8 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| home_sound_toggle_toast | FAIL | FAIL (33 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| loading_t0000 | FAIL | FAIL (0 web / 1 unity events, first extra on unity #0: {"t":0,"type":"page","name":"loading","args":[]}) |
| loading_t0500 | FAIL | FAIL (2 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"loading","args":[]}) |
| loading_t1100 | FAIL | FAIL (4 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"loading","args":[]}) |
| maps_world_1 | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| maps_world_1_all_complete | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| maps_world_2 | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| maps_world_3 | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| maps_world_3_none_complete | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| maps_world_4 | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| maps_world_5 | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| maps_world_5_scrolled | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_debrief_daily_trophy | FAIL | FAIL (1481 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_debrief_exact | FAIL | FAIL (539 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_debrief_surplus | FAIL | FAIL (539 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_debrief_t0150 | FAIL | FAIL (435 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_debrief_world3 | FAIL | FAIL (653 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_debrief_world5 | FAIL | FAIL (809 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_defeat_backup | FAIL | FAIL (401 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_defeat_backup_used_toast | FAIL | FAIL (1278 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_defeat_no_backup_low | FAIL | FAIL (401 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_defeat_no_backup_used | FAIL | FAIL (401 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_defeat_t0150 | FAIL | FAIL (299 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_pause | FAIL | FAIL (65 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_pause_closing_t0150 | FAIL | FAIL (108 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_pause_level_21 | FAIL | FAIL (132 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_pause_muted | FAIL | FAIL (90 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| modal_pause_t0150 | FAIL | FAIL (65 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pause_resume_exit | MISSING | MISSING (unity) |
| play_sector_1 | MISSING | MISSING (unity) |
| pressed_back_achievements | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_back_daily | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_back_maps | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_back_settings | FAIL | FAIL (32 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_back_shop | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_claim_gift | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_daily_start_shift | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_debrief_next | FAIL | FAIL (647 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_debrief_replay | FAIL | FAIL (647 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_defeat_backup | FAIL | FAIL (508 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_defeat_home | FAIL | FAIL (508 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_defeat_retry | FAIL | FAIL (508 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_dock_daily | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_dock_maps | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_dock_shop | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_dock_trophies | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_home_audio | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_home_settings | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_hud_pause | FAIL | FAIL (100 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_hud_restart | FAIL | FAIL (100 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_level_tile | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_modal_close | FAIL | FAIL (65 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_modal_pause_home | FAIL | FAIL (65 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_modal_resume | FAIL | FAIL (65 web / 2 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_play_mission | FAIL | FAIL (7 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_privacy_dismiss | FAIL | FAIL (33 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_shop_buy | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_shop_tab | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| pressed_world_tab | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_default | FAIL | FAIL (32 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_modal_privacy | FAIL | FAIL (33 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_modal_privacy_t0150 | FAIL | FAIL (33 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_modal_reset_confirm | FAIL | FAIL (33 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_reset_confirmed_toast | FAIL | FAIL (42 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_scrolled | FAIL | FAIL (32 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_sliders_moved | FAIL | FAIL (40 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_sliders_profile | FAIL | FAIL (32 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_toggles | MISSING | MISSING (unity) |
| settings_toggles_clicked | FAIL | FAIL (37 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| settings_toggles_off | FAIL | FAIL (32 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_buy_insufficient_toast | FAIL | FAIL (29 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_buy_success_toast | FAIL | FAIL (31 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_claim_gift_toast | FAIL | FAIL (46 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_equip_toast | FAIL | FAIL (29 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_gift_claimed | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_purchase_and_equip | MISSING | MISSING (unity) |
| shop_rings_default | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_rings_mixed | FAIL | FAIL (29 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_scrolled | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_singularities_default | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_singularities_mixed | FAIL | FAIL (29 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_skyboxes_default | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| shop_skyboxes_mixed | FAIL | FAIL (28 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| toast_long | FAIL | FAIL (8 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| toast_stacked | FAIL | FAIL (9 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| toast_t0200 | FAIL | FAIL (8 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| toast_t1000 | FAIL | FAIL (8 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |
| toast_t2300 | FAIL | FAIL (8 web / 1 unity events, first mismatch #0 [type,name,args,t]: web={"t":240,"type":"timer","name":"interval","args":[240]} unity={"t":0,"type":"page","name":"home","args":[]}) |

## Harness settings

### web

- Chromium: 153.0.8010.12
- Launch args: `--force-color-profile=srgb --font-render-hinting=none --disable-lcd-text`
- Animation clock: registry+sweep (SPEC 4.5)
- perfOffset per run: 1, 10, 100, 101, 11, 117, 12, 13, 2, 20, 21, 22, 24, 249, 3, 33, 34, 37, 39, 4, 42, 47, 5, 58, 6, 61, 67, 7, 70, 74, 75, 79, 8, 80, 81, 9, 92 (276 runs)

### unity

`_meta.json` not found.
