import { writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { spinProgram } from "./program-lib.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const TODAY = "2026-10-01";
const adv = (ms) => ({ op: "advance", ms });
const click = (target, extra = {}) => ({ op: "click", target, ...extra });
const sel = (selector) => ({ op: "clickSelector", selector });
const prof = (path, value) => ({ op: "setProfile", path, value });
const call = (method, ...args) => ({ op: "call", method, args });
const BOOT = { op: "boot" };

const WIN_TICKS = Number(process.env.WIN_TICKS ?? 190);

const sequences = [
  {
    id: "boot_to_home",
    description:
      "Loading sequence to home (interval ticks, home timeout, page, credits)",
    steps: [BOOT, adv(2000)],
  },
  {
    id: "play_sector_1",
    description:
      "Press PLAY MISSION, scripted spin drag program until Nibbles is full, debrief opens, press Next Map",
    expect: [{ type: "modal", name: "modal-debrief", args: ["open"] }],
    steps: [
      BOOT,
      click("btn-home-play"),
      ...spinProgram({
        dx: 22,
        tickFrames: 4,
        strokeTicks: 3,
        ticks: WIN_TICKS,
      }),
      adv(2000),
      click("btn-debrief-next"),
      adv(1500),
    ],
  },
  {
    id: "defeat_sector_1",
    description:
      "Press PLAY MISSION, no input for 5 s, force the out-of-scrap failure, retry from the defeat modal",
    steps: [
      BOOT,
      click("btn-home-play"),
      adv(5000),
      call("handleLevelFailure"),
      adv(1000),
      click("btn-defeat-retry"),
      adv(1500),
    ],
  },
  {
    id: "shop_purchase_and_equip",
    description:
      "Buy and auto-equip a ring, fail to buy a hat, switch tabs, wear the starter ring again",
    steps: [
      BOOT,
      prof("credits", 1000),
      click("btn-open-shop-page"),
      adv(400),
      sel(".btn-buy-item[data-id=ring_duckie]"),
      adv(1000),
      sel(".tab-btn[data-tab=singularities]"),
      adv(200),
      sel(".btn-buy-item[data-id=singularity_strawberry]"),
      adv(1000),
      sel(".tab-btn[data-tab=rings]"),
      adv(200),
      sel(".btn-equip-item[data-id=ring_donut]"),
      adv(1000),
      click("btn-back-shop"),
      adv(400),
    ],
  },
  {
    id: "daily_gift_claim",
    description:
      "Claim the shop stipend, then the daily master bonus after clearing all shifts",
    steps: [
      BOOT,
      click("btn-open-shop-page"),
      adv(400),
      click("btn-claim-daily-gift"),
      adv(1500),
      click("btn-back-shop"),
      adv(400),
      prof("lastDailyShiftDate", TODAY),
      prof("completedDailyTiers", ["bronze", "silver", "gold"]),
      click("btn-open-daily-shift"),
      adv(400),
      click("btn-claim-master-bonus"),
      adv(1500),
    ],
  },
  {
    id: "settings_toggles",
    description:
      "Settings: toggle haptics and sound, drag both sliders, haptic buzz only while enabled",
    steps: [
      BOOT,
      click("btn-home-settings"),
      adv(240),
      call("triggerHaptic", 30),
      click("settings-haptics-toggle"),
      adv(300),
      call("triggerHaptic", 30),
      click("settings-haptics-toggle"),
      adv(300),
      call("triggerHaptic", 45),
      click("settings-sfx-slider", { fx: 0.3 }),
      adv(100),
      click("settings-ambience-slider", { fx: 0.8 }),
      adv(100),
      click("settings-sound-toggle"),
      adv(300),
      click("settings-sound-toggle"),
      adv(300),
      click("btn-back-settings"),
      adv(400),
    ],
  },
  {
    id: "pause_resume_exit",
    description:
      "Play, pause, toggle sound twice, resume, pause again and return home",
    steps: [
      BOOT,
      click("btn-home-play"),
      adv(500),
      click("btn-game-pause"),
      adv(300),
      click("btn-pause-sound"),
      adv(100),
      click("btn-pause-sound"),
      adv(100),
      click("btn-pause-resume"),
      adv(600),
      click("btn-game-pause"),
      adv(300),
      click("btn-pause-home"),
      adv(600),
    ],
  },
];

writeFileSync(
  join(here, "..", "sequences.json"),
  `${JSON.stringify(sequences)}\n`.replace(/\},\{"id"/g, '},\n{"id"'),
);
console.log(
  `${sequences.length} sequences, play_sector_1 = ${sequences[1].steps.length} ops`,
);
