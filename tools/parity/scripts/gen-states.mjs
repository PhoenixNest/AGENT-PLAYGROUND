import { writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { spinProgram } from "./program-lib.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const BOTH = ["zero", "iphone"];
const TODAY = "2026-10-01";

const boot = (completeLoading = true) =>
  completeLoading ? { op: "boot" } : { op: "boot", completeLoading: false };
const adv = (ms) => ({ op: "advance", ms });
const click = (target, extra = {}) => ({ op: "click", target, ...extra });
const sel = (selector) => ({ op: "clickSelector", selector });
const hold = (target) => ({ op: "holdPressed", target });
const holdSel = (selector) => ({ op: "holdPressed", selector });
const prof = (path, value) => ({ op: "setProfile", path, value });
const app = (field, value) => ({ op: "setApp", field, value });
const call = (method, ...args) => ({ op: "call", method, args });
const page = (p) => ({ op: "showPage", page: p });
const load = (index) => ({ op: "loadSector", index });
const CAP = { op: "capture" };

const states = [];
const add = (id, description, steps, profiles = BOTH) =>
  states.push({ id, description, profiles, steps });

const gameStart = (index) => [
  load(index),
  page("game"),
  call("dismissGestureHint"),
  adv(400),
];
const levelIds = (from, to) =>
  Array.from({ length: to - from + 1 }, (_, i) => `level_${from + i}`);
const dailyDone = (tiers) => [
  prof("lastDailyShiftDate", TODAY),
  prof("completedDailyTiers", tiers),
];

add(
  "loading_t0000",
  "Loading screen at boot, no frame run (bar at CSS default, first tip)",
  [boot(false), CAP],
);
add(
  "loading_t0500",
  "Loading screen 500 ms in (two interval ticks, bar transitions running)",
  [boot(false), adv(500), CAP],
);
add(
  "loading_t1100",
  "Loading screen 1100 ms in (bar at 100 %, home shows at 1310)",
  [boot(false), adv(1100), CAP],
);

add("home_default", "Home after loading, settled (boot + 240 ms)", [
  boot(),
  CAP,
]);
for (const ms of [400, 900, 2500, 6000, 12000]) {
  add(
    `home_anim_t${String(ms).padStart(5, "0")}`,
    `Home ${ms} ms after settle: samples mascotFloat, debris, sunburstRotate, shineSweep, tagPulse`,
    [boot(), adv(ms), CAP],
  );
}
for (const ms of [16, 300, 700]) {
  add(
    `home_mascot_click_t${String(ms).padStart(4, "0")}`,
    `Home, Nibbles tapped, captured ${ms} ms later (squash, heart particle, face swap)`,
    [boot(), click("home-mascot-stage"), adv(ms), CAP],
  );
}
add(
  "home_badges_two_new",
  "Home, bronze shift cleared: dock daily badge shows 2 NEW",
  [boot(), ...dailyDone(["bronze"]), call("updateHomeView"), CAP],
);
add(
  "home_badges_master_ready",
  "Home, all 3 daily shifts cleared and master bonus unclaimed: daily badge GIFT",
  [
    boot(),
    ...dailyDone(["bronze", "silver", "gold"]),
    call("updateHomeView"),
    CAP,
  ],
);
add(
  "home_badges_all_claimed",
  "Home, shifts cleared, master bonus and stipend claimed: green check badge, shop badge hidden",
  [
    boot(),
    ...dailyDone(["bronze", "silver", "gold"]),
    prof("dailyMasterClaimedDate", TODAY),
    prof("lastDailyGiftDate", TODAY),
    call("updateHomeView"),
    CAP,
  ],
);
add(
  "home_equipped_duckie_berry",
  "Home with Rubber Duckie ring and Berry Swirl singularity equipped",
  [
    boot(),
    prof("equipped.ring", "ring_duckie"),
    prof("equipped.singularity", "singularity_strawberry"),
    call("updateHomeView"),
    adv(300),
    CAP,
  ],
);
add(
  "home_equipped_candy_sleepy",
  "Home with Peppermint ring and Sleepy Star singularity equipped",
  [
    boot(),
    prof("equipped.ring", "ring_candyland"),
    prof("equipped.singularity", "singularity_sleepy"),
    call("updateHomeView"),
    adv(300),
    CAP,
  ],
);
add(
  "home_equipped_kitty_confetti",
  "Home with Kitty Saucer ring and Party Popper singularity equipped",
  [
    boot(),
    prof("equipped.ring", "ring_spacecat"),
    prof("equipped.singularity", "singularity_confetti"),
    call("updateHomeView"),
    adv(300),
    CAP,
  ],
);
add(
  "home_sector_21_big_credits",
  "Home after selecting level 21 with 123456 credits (mission pill and coin pill widths)",
  [boot(), prof("credits", 123456), load(20), call("updateHomeView"), CAP],
);
add(
  "home_sound_toggle_toast",
  "Home, speaker button clicked: muted icon and Sound Off toast on plateau",
  [boot(), click("btn-home-audio"), adv(1000), CAP],
);

const pressedHome = [
  ["pressed_play_mission", "btn-home-play", "PLAY MISSION held down"],
  ["pressed_dock_maps", "btn-open-maps-page", "MAPS dock tab held down"],
  ["pressed_dock_daily", "btn-open-daily-shift", "DAILY dock tab held down"],
  ["pressed_dock_shop", "btn-open-shop-page", "SHOP dock tab held down"],
  [
    "pressed_dock_trophies",
    "btn-open-trophies-page",
    "TROPHIES dock tab held down",
  ],
  ["pressed_home_audio", "btn-home-audio", "home speaker button held down"],
  ["pressed_home_settings", "btn-home-settings", "home gear button held down"],
];
for (const [id, target, what] of pressedHome)
  add(id, `Home, ${what} (:active settled after 150 ms)`, [
    boot(),
    hold(target),
    CAP,
  ]);

for (let w = 1; w <= 5; w++) {
  const first = (w - 1) * 10 + 1;
  const done = [
    ...levelIds(first, first + 2),
    `level_${first + 4}`,
    `level_${first + 6}`,
  ].slice(0, 2 + w);
  add(
    `maps_world_${w}`,
    `Maps page, world tab ${w}: ${done.length} completed tiles, level ${first + 3} current`,
    [
      boot(),
      prof("completedSectors", done),
      load(first + 2),
      click("btn-open-maps-page"),
      adv(240),
      sel(`.world-tab-btn:nth-child(${w})`),
      adv(150),
      CAP,
    ],
  );
}
add(
  "maps_world_1_all_complete",
  "Maps page, world 1 fully completed (10/10 banner, all check marks)",
  [
    boot(),
    prof("completedSectors", levelIds(1, 10)),
    click("btn-open-maps-page"),
    adv(240),
    CAP,
  ],
);
add(
  "maps_world_3_none_complete",
  "Maps page, world 3 with nothing completed, level 21 current",
  [
    boot(),
    load(20),
    click("btn-open-maps-page"),
    adv(240),
    sel(".world-tab-btn:nth-child(3)"),
    adv(150),
    CAP,
  ],
);
add(
  "maps_world_5_scrolled",
  "Maps page, world 5 scrolled to the bottom of the grid",
  [
    boot(),
    prof("completedSectors", levelIds(1, 44)),
    load(43),
    click("btn-open-maps-page"),
    adv(240),
    sel(".world-tab-btn:nth-child(5)"),
    adv(150),
    { op: "scroll", y: 600 },
    CAP,
  ],
);
add("pressed_back_maps", "Maps page, back button held down", [
  boot(),
  click("btn-open-maps-page"),
  adv(240),
  hold("btn-back-maps"),
  CAP,
]);
add("pressed_world_tab", "Maps page, world tab 3 held down", [
  boot(),
  click("btn-open-maps-page"),
  adv(240),
  holdSel(".world-tab-btn:nth-child(3)"),
  CAP,
]);
add(
  "pressed_level_tile",
  "Maps page, level tile 2 held down (:active and :hover)",
  [
    boot(),
    click("btn-open-maps-page"),
    adv(240),
    holdSel(".level-tile:nth-child(2)"),
    CAP,
  ],
);

add(
  "daily_default",
  "Daily page, no shifts cleared, master box locked, countdown 14h 59m",
  [boot(), click("btn-open-daily-shift"), adv(400), CAP],
);
add("daily_bronze_cleared", "Daily page, bronze cleared (progress 1/3)", [
  boot(),
  ...dailyDone(["bronze"]),
  click("btn-open-daily-shift"),
  adv(400),
  CAP,
]);
add(
  "daily_two_cleared",
  "Daily page, bronze and silver cleared (progress 2/3)",
  [
    boot(),
    ...dailyDone(["bronze", "silver"]),
    click("btn-open-daily-shift"),
    adv(400),
    CAP,
  ],
);
add(
  "daily_master_ready",
  "Daily page, all cleared, master bonus claimable (unlocked glow, claim button)",
  [
    boot(),
    ...dailyDone(["bronze", "silver", "gold"]),
    click("btn-open-daily-shift"),
    adv(400),
    CAP,
  ],
);
add(
  "daily_master_claimed",
  "Daily page, all cleared and master bonus already claimed",
  [
    boot(),
    ...dailyDone(["bronze", "silver", "gold"]),
    prof("dailyMasterClaimedDate", TODAY),
    click("btn-open-daily-shift"),
    adv(400),
    CAP,
  ],
);
add(
  "daily_master_claim_toast",
  "Daily page right after claiming the master bonus: toast on plateau, 550 credits",
  [
    boot(),
    ...dailyDone(["bronze", "silver", "gold"]),
    click("btn-open-daily-shift"),
    adv(400),
    click("btn-claim-master-bonus"),
    adv(1000),
    CAP,
  ],
);
add(
  "daily_after_two_minutes",
  "Daily page opened after 120 s of fake time (countdown 14h 57m)",
  [boot(), adv(120000), click("btn-open-daily-shift"), adv(400), CAP],
);
add("daily_scrolled", "Daily page scrolled to the bottom (gold shift card)", [
  boot(),
  click("btn-open-daily-shift"),
  adv(400),
  { op: "scroll", y: 600 },
  CAP,
]);
add("pressed_back_daily", "Daily page, back button held down", [
  boot(),
  click("btn-open-daily-shift"),
  adv(400),
  hold("btn-back-daily"),
  CAP,
]);
add(
  "pressed_daily_start_shift",
  "Daily page, first Start Shift button held down",
  [
    boot(),
    click("btn-open-daily-shift"),
    adv(400),
    holdSel(".btn-play-daily-tier:nth-child(2)"),
    CAP,
  ],
);

const shopOpen = [boot(), click("btn-open-shop-page"), adv(400)];
add(
  "shop_rings_default",
  "Shop, rings tab, default profile (stipend claimable, starter equipped, others locked)",
  [...shopOpen, CAP],
);
add("shop_singularities_default", "Shop, singularities tab, default profile", [
  ...shopOpen,
  sel(".tab-btn[data-tab=singularities]"),
  adv(150),
  CAP,
]);
add("shop_skyboxes_default", "Shop, skyboxes tab, default profile", [
  ...shopOpen,
  sel(".tab-btn[data-tab=skyboxes]"),
  adv(150),
  CAP,
]);
add(
  "shop_rings_mixed",
  "Shop, rings tab with duckie and candy unlocked, candy equipped, 5000 credits",
  [
    boot(),
    prof("credits", 5000),
    prof("unlocked.rings", ["ring_donut", "ring_duckie", "ring_candyland"]),
    prof("equipped.ring", "ring_candyland"),
    click("btn-open-shop-page"),
    adv(400),
    CAP,
  ],
);
add(
  "shop_singularities_mixed",
  "Shop, singularities tab with berry unlocked and equipped",
  [
    boot(),
    prof("credits", 900),
    prof("unlocked.singularities", [
      "singularity_nibbles",
      "singularity_strawberry",
    ]),
    prof("equipped.singularity", "singularity_strawberry"),
    click("btn-open-shop-page"),
    adv(400),
    sel(".tab-btn[data-tab=singularities]"),
    adv(150),
    CAP,
  ],
);
add(
  "shop_skyboxes_mixed",
  "Shop, skyboxes tab with cheese moon unlocked and equipped",
  [
    boot(),
    prof("unlocked.skyboxes", ["skybox_cotton_candy", "skybox_cheese_moon"]),
    prof("equipped.skybox", "skybox_cheese_moon"),
    click("btn-open-shop-page"),
    adv(400),
    sel(".tab-btn[data-tab=skyboxes]"),
    adv(150),
    CAP,
  ],
);
add(
  "shop_gift_claimed",
  "Shop with the daily stipend already claimed (banner claimed state)",
  [
    boot(),
    prof("lastDailyGiftDate", TODAY),
    click("btn-open-shop-page"),
    adv(400),
    CAP,
  ],
);
add(
  "shop_claim_gift_toast",
  "Shop right after claiming the stipend: banner transitions to claimed, toast on plateau, 350 credits",
  [...shopOpen, click("btn-claim-daily-gift"), adv(1000), CAP],
);
add(
  "shop_buy_insufficient_toast",
  "Shop, tried to buy a 500-coin ring with 250 credits: need-more-coins toast",
  [...shopOpen, sel(".btn-buy-item[data-id=ring_duckie]"), adv(1000), CAP],
);
add(
  "shop_buy_success_toast",
  "Shop, bought and equipped the duckie ring with 1000 credits: unlocked toast, 500 credits left",
  [
    boot(),
    prof("credits", 1000),
    click("btn-open-shop-page"),
    adv(400),
    sel(".btn-buy-item[data-id=ring_duckie]"),
    adv(1000),
    CAP,
  ],
);
add("shop_equip_toast", "Shop, wore an owned ring: equipped toast on plateau", [
  boot(),
  prof("unlocked.rings", ["ring_donut", "ring_duckie"]),
  click("btn-open-shop-page"),
  adv(400),
  sel(".btn-equip-item[data-id=ring_duckie]"),
  adv(1000),
  CAP,
]);
add("shop_scrolled", "Shop rings tab scrolled to the bottom", [
  ...shopOpen,
  { op: "scroll", y: 600 },
  CAP,
]);
add("pressed_shop_buy", "Shop, an Unlock button held down", [
  ...shopOpen,
  holdSel(".btn-buy-item[data-id=ring_candyland]"),
  CAP,
]);
add("pressed_shop_tab", "Shop, Nibbles Hats tab held down", [
  ...shopOpen,
  holdSel(".tab-btn[data-tab=singularities]"),
  CAP,
]);
add("pressed_claim_gift", "Shop, stipend claim button held down", [
  ...shopOpen,
  hold("btn-claim-daily-gift"),
  CAP,
]);
add("pressed_back_shop", "Shop, back button held down", [
  ...shopOpen,
  hold("btn-back-shop"),
  CAP,
]);

add("achievements_none", "Trophy room, nothing unlocked", [
  boot(),
  click("btn-open-trophies-page"),
  adv(240),
  CAP,
]);
add("achievements_some", "Trophy room, first nom and tidy cadet unlocked", [
  boot(),
  prof("achievements", ["ach_first_drop", "ach_cleaner_50"]),
  click("btn-open-trophies-page"),
  adv(240),
  CAP,
]);
add("achievements_all", "Trophy room, all five unlocked", [
  boot(),
  prof("achievements", [
    "ach_first_drop",
    "ach_cleaner_50",
    "ach_cleaner_250",
    "ach_cleaner_1000",
    "ach_daily_shift",
  ]),
  click("btn-open-trophies-page"),
  adv(240),
  CAP,
]);
add("pressed_back_achievements", "Trophy room, back button held down", [
  boot(),
  click("btn-open-trophies-page"),
  adv(240),
  hold("btn-back-achievements"),
  CAP,
]);

const settingsOpen = [boot(), click("btn-home-settings"), adv(240)];
add("settings_default", "Settings, default profile", [...settingsOpen, CAP]);
add(
  "settings_toggles_off",
  "Settings with sound and haptics switched off via profile",
  [
    boot(),
    prof("settings.soundEnabled", false),
    prof("settings.hapticsEnabled", false),
    click("btn-home-settings"),
    adv(240),
    CAP,
  ],
);
add(
  "settings_toggles_clicked",
  "Settings, sound and haptics switches clicked off, transition settled",
  [
    ...settingsOpen,
    click("settings-sound-toggle"),
    click("settings-haptics-toggle"),
    adv(300),
    CAP,
  ],
);
add(
  "settings_sliders_moved",
  "Settings, sound FX slider clicked at 20 % and ambience slider at 90 %",
  [
    ...settingsOpen,
    click("settings-sfx-slider", { fx: 0.2 }),
    click("settings-ambience-slider", { fx: 0.9 }),
    adv(16),
    CAP,
  ],
);
add("settings_sliders_profile", "Settings, sliders at 10 and 100 via profile", [
  boot(),
  prof("settings.sfxVolume", 10),
  prof("settings.ambientVolume", 100),
  click("btn-home-settings"),
  adv(240),
  CAP,
]);
add("settings_scrolled", "Settings scrolled to the bottom (version footer)", [
  ...settingsOpen,
  { op: "scroll", y: 600 },
  CAP,
]);
add("settings_modal_privacy", "Settings with the privacy policy modal open", [
  ...settingsOpen,
  click("btn-open-privacy"),
  adv(300),
  CAP,
]);
add(
  "settings_modal_privacy_t0150",
  "Privacy modal 150 ms into its open transition",
  [...settingsOpen, click("btn-open-privacy"), adv(150), CAP],
);
add(
  "settings_modal_reset_confirm",
  "Settings with the reset-progress confirm modal open",
  [...settingsOpen, click("btn-reset-game-data"), adv(300), CAP],
);
add(
  "settings_reset_confirmed_toast",
  "Reset confirmed after spending: home with Progress Reset toast on plateau, 250 credits",
  [
    boot(),
    prof("credits", 4000),
    click("btn-home-settings"),
    adv(240),
    click("btn-reset-game-data"),
    adv(300),
    click("btn-confirm-reset"),
    adv(1000),
    CAP,
  ],
);
add("pressed_back_settings", "Settings, back button held down", [
  ...settingsOpen,
  hold("btn-back-settings"),
  CAP,
]);
add("pressed_privacy_dismiss", "Privacy modal, dismiss button held down", [
  ...settingsOpen,
  click("btn-open-privacy"),
  adv(300),
  hold("btn-dismiss-privacy"),
  CAP,
]);

for (const [ms, label] of [
  [200, "rising"],
  [1000, "plateau"],
  [2300, "fading"],
]) {
  add(
    `toast_t${String(ms).padStart(4, "0")}`,
    `Toast ${ms} ms after showToast (${label})`,
    [boot(), call("showToast", "Item Equipped! ✨"), adv(ms), CAP],
  );
}
add("toast_long", "Long toast text on plateau (wrapping and width limit)", [
  boot(),
  call(
    "showToast",
    "🎉 All 3 Daily Shifts Cleared! Claim Master Bonus in Daily Tab! 🏆",
  ),
  adv(1000),
  CAP,
]);
add("toast_stacked", "Two toasts, the second 600 ms after the first", [
  boot(),
  call("showToast", "Sound On 🔊"),
  adv(600),
  call("showToast", "🏆 Trophy: First Nom Nom! (+100 🪙)"),
  adv(900),
  CAP,
]);

const pauseOpen = [boot(), ...gameStart(0), click("btn-game-pause")];
add("modal_pause", "Pause modal over level 1, settled", [
  ...pauseOpen,
  adv(300),
  CAP,
]);
add("modal_pause_t0150", "Pause modal 150 ms into its open transition", [
  ...pauseOpen,
  adv(150),
  CAP,
]);
add(
  "modal_pause_muted",
  "Pause modal after toggling sound off (OFF label, muted icon, toast)",
  [...pauseOpen, adv(300), click("btn-pause-sound"), adv(500), CAP],
);
add(
  "modal_pause_level_21",
  "Pause modal over level 21 with partial progress text",
  [
    boot(),
    ...gameStart(20),
    app("collectedMass", 40),
    click("btn-game-pause"),
    adv(300),
    CAP,
  ],
);
add(
  "modal_pause_closing_t0150",
  "Pause modal 150 ms into its close transition",
  [...pauseOpen, adv(300), click("btn-pause-resume"), adv(150), CAP],
);
add("pressed_modal_resume", "Pause modal, RESUME PLAY held down", [
  ...pauseOpen,
  adv(300),
  hold("btn-pause-resume"),
  CAP,
]);
add("pressed_modal_close", "Pause modal, close X held down", [
  ...pauseOpen,
  adv(300),
  hold("btn-close-pause"),
  CAP,
]);
add("pressed_modal_pause_home", "Pause modal, Return to Home held down", [
  ...pauseOpen,
  adv(300),
  hold("btn-pause-home"),
  CAP,
]);

const winSetup = (index, mass) => [
  boot(),
  ...gameStart(index),
  app("collectedMass", mass),
  call("handleLevelVictory"),
];
add(
  "modal_debrief_exact",
  "Debrief after clearing level 1 at exactly the quota (no surplus), settled",
  [...winSetup(0, 20), adv(900), CAP],
);
add(
  "modal_debrief_surplus",
  "Debrief at level 1 with 13 kg surplus (payout 302 coins)",
  [...winSetup(0, 33), adv(900), CAP],
);
add(
  "modal_debrief_world3",
  "Debrief after clearing level 21 (quota 105, payout 350)",
  [...winSetup(20, 105), adv(900), CAP],
);
add(
  "modal_debrief_world5",
  "Debrief after clearing level 41 with 40 kg surplus",
  [...winSetup(40, 250), adv(900), CAP],
);
add("modal_debrief_t0150", "Debrief 150 ms into its open transition", [
  ...winSetup(0, 20),
  adv(750),
  CAP,
]);
add(
  "modal_debrief_daily_trophy",
  "Debrief after a daily shift: Daily Duty Star trophy toast over the modal",
  [
    boot(),
    click("btn-open-daily-shift"),
    adv(400),
    sel(".btn-play-daily-tier:nth-child(2)"),
    adv(16),
    call("dismissGestureHint"),
    adv(400),
    app("collectedMass", 40),
    call("handleLevelVictory"),
    adv(900),
    CAP,
  ],
);
add("pressed_debrief_next", "Debrief, Next Map held down", [
  ...winSetup(0, 20),
  adv(900),
  hold("btn-debrief-next"),
  CAP,
]);
add("pressed_debrief_replay", "Debrief, Play Again held down", [
  ...winSetup(0, 20),
  adv(900),
  hold("btn-debrief-replay"),
  CAP,
]);

const loseSetup = (mass, spilled, extra = []) => [
  boot(),
  ...gameStart(0),
  app("collectedMass", mass),
  app("spilledMass", spilled),
  ...extra,
  call("handleLevelFailure"),
];
add(
  "modal_defeat_backup",
  "Defeat at level 1 with 15/20 kg (75 %): Backup Pod offered",
  [...loseSetup(15, 4), adv(700), CAP],
);
add(
  "modal_defeat_no_backup_low",
  "Defeat at level 1 with 5/20 kg: no Backup Pod (below 70 %)",
  [...loseSetup(5, 9), adv(700), CAP],
);
add(
  "modal_defeat_no_backup_used",
  "Defeat at level 1 with 15/20 kg after the Backup Pod was already used",
  [...loseSetup(15, 4, [app("usedBackupPod", true)]), adv(700), CAP],
);
add(
  "modal_defeat_t0150",
  "Defeat modal 150 ms into its open transition (Backup Pod offered)",
  [...loseSetup(15, 4), adv(550), CAP],
);
add(
  "modal_defeat_backup_used_toast",
  "After launching the Backup Pod: scrap deployed toast on plateau, modal closed",
  [...loseSetup(15, 4), adv(700), click("btn-defeat-backup"), adv(1000), CAP],
);
add("pressed_defeat_retry", "Defeat modal, Try Again held down", [
  ...loseSetup(15, 4),
  adv(700),
  hold("btn-defeat-retry"),
  CAP,
]);
add("pressed_defeat_backup", "Defeat modal, Backup Pod button held down", [
  ...loseSetup(15, 4),
  adv(700),
  hold("btn-defeat-backup"),
  CAP,
]);
add("pressed_defeat_home", "Defeat modal, Hangar held down", [
  ...loseSetup(15, 4),
  adv(700),
  hold("btn-defeat-home"),
  CAP,
]);

const LEVELS = [
  [0, "01"],
  [10, "11"],
  [20, "21"],
  [30, "31"],
  [40, "41"],
];
for (const [index, label] of LEVELS) {
  for (const [ms, name] of [
    [16, "0016"],
    [1000, "1000"],
    [3000, "3000"],
  ]) {
    add(
      `game_lvl${label}_t${name}ms`,
      `Level ${label} gameplay ${ms} ms after the hint settled (hint dismissed, no input)`,
      [boot(), ...gameStart(index), adv(ms), CAP],
    );
  }
}
add(
  "game_lvl01_hint_visible",
  "Level 1 gameplay, gesture hint visible 16 ms after entering the game page",
  [boot(), load(0), page("game"), adv(16), CAP],
);
add(
  "game_lvl01_hint_dismissing_t0200",
  "Level 1 gameplay, gesture hint 200 ms into its fade-out",
  [boot(), load(0), page("game"), call("dismissGestureHint"), adv(200), CAP],
);
const DRAG = { dx: 22, tickFrames: 4, strokeTicks: 3 };
add(
  "game_lvl01_mid_drag",
  "Level 1 gameplay, 20 drag ticks into a spin gesture, pointer still down",
  [
    boot(),
    ...gameStart(0),
    ...spinProgram({ ...DRAG, ticks: 20 }).slice(0, -1),
    CAP,
  ],
);
add(
  "game_lvl01_gate_pass",
  "Level 1 gameplay, 28 drag ticks in: first multiplier gate pass with floating text",
  [
    boot(),
    ...gameStart(0),
    ...spinProgram({ ...DRAG, ticks: 28 }).slice(0, -1),
    CAP,
  ],
);
add(
  "game_lvl01_pickup",
  "Level 1 gameplay, 55 drag ticks in: first debris swallowed by Nibbles with NOM text",
  [
    boot(),
    ...gameStart(0),
    ...spinProgram({ ...DRAG, ticks: 55 }).slice(0, -1),
    CAP,
  ],
);
add(
  "game_lvl01_near_victory",
  "Level 1 gameplay with 19/20 kg collected (HUD bar at 95 %)",
  [
    boot(),
    ...gameStart(0),
    app("collectedMass", 19),
    call("updateQuotaDisplay"),
    adv(16),
    CAP,
  ],
);
add(
  "game_lvl01_quota_met_hud",
  "Level 1 gameplay with the quota bar full (green gradient, no victory yet)",
  [
    boot(),
    ...gameStart(0),
    app("collectedMass", 20),
    call("updateQuotaDisplay"),
    adv(16),
    CAP,
  ],
);
add(
  "game_lvl01_paused_t1000",
  "Level 1 gameplay paused, 1000 ms after opening the pause modal (physics frozen, canvas still drawing)",
  [boot(), ...gameStart(0), click("btn-game-pause"), adv(1000), CAP],
);
add(
  "game_lvl01_skins_kitty_party_mint",
  "Level 1 gameplay with Kitty ring, Party Popper hole and Mint Soda sky equipped",
  [
    boot(),
    prof("equipped.ring", "ring_spacecat"),
    prof("equipped.singularity", "singularity_confetti"),
    prof("equipped.skybox", "skybox_mint_soda"),
    ...gameStart(0),
    adv(1000),
    CAP,
  ],
);
add(
  "game_lvl11_skins_duckie_berry_cheese",
  "Level 11 gameplay with Duckie ring, Berry hole and Cheese Moon sky equipped",
  [
    boot(),
    prof("equipped.ring", "ring_duckie"),
    prof("equipped.singularity", "singularity_strawberry"),
    prof("equipped.skybox", "skybox_cheese_moon"),
    ...gameStart(10),
    adv(1000),
    CAP,
  ],
);
add(
  "game_daily_bronze_t1000",
  "Bronze daily shift gameplay 1000 ms after the hint settled",
  [
    boot(),
    click("btn-open-daily-shift"),
    adv(400),
    sel(".btn-play-daily-tier:nth-child(2)"),
    adv(16),
    call("dismissGestureHint"),
    adv(400),
    adv(1000),
    CAP,
  ],
);
add("pressed_hud_pause", "Gameplay, HUD pause button held down", [
  boot(),
  ...gameStart(0),
  hold("btn-game-pause"),
  CAP,
]);
add("pressed_hud_restart", "Gameplay, HUD restart button held down", [
  boot(),
  ...gameStart(0),
  hold("btn-game-restart"),
  CAP,
]);
add(
  "game_lvl01_restart_toast",
  "Gameplay after pressing HUD restart: Clean up toast on plateau",
  [boot(), ...gameStart(0), click("btn-game-restart"), adv(1000), CAP],
);

writeFileSync(
  join(here, "..", "states.json"),
  `${JSON.stringify(states, null, 1)}\n`,
);
console.log(`${states.length} states`);
