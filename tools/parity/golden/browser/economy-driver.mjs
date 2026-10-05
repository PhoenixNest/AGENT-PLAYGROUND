import { hex } from "./driver.mjs";

const T0 = 1790870401000;
const KEY = "ORBITAL_SALVAGE_CARTOON_PROFILE_V1";
const D = (iso) => Date.parse(iso);

const t = (ms, note) => ({ op: "setTime", ms, note });
const c = () => ({ op: "construct" });

export const SEQUENCES = [
  {
    name: "shop",
    ops: [
      t(T0), c(),
      { op: "getDefaultProfile" },
      { op: "normalizeCategory", args: ["ring"] }, { op: "normalizeCategory", args: ["rings"] },
      { op: "normalizeCategory", args: ["singularity"] }, { op: "normalizeCategory", args: ["singularities"] },
      { op: "normalizeCategory", args: ["skybox"] }, { op: "normalizeCategory", args: ["skyboxes"] },
      { op: "normalizeCategory", args: ["gizmo"] },
      { op: "unlockItem", args: ["rings", "ring_duckie", 500] },
      { op: "equipItem", args: ["rings", "ring_duckie"] },
      { op: "addCredits", args: [400.9] },
      { op: "unlockItem", args: ["rings", "ring_duckie", 500] },
      { op: "equipItem", args: ["ring", "ring_duckie"] },
      { op: "unlockItem", args: ["ring", "ring_duckie", 500] },
      { op: "isUnlocked", args: ["rings", "ring_duckie"] },
      { op: "isUnlocked", args: ["ring", "ring_candyland"] },
      { op: "equipItem", args: ["ring", "ring_candyland"] },
      { op: "addCredits", args: [5000] },
      { op: "unlockItem", args: ["singularities", "singularity_strawberry", 600] },
      { op: "equipItem", args: ["singularity", "singularity_strawberry"] },
      { op: "unlockItem", args: ["skybox", "skybox_mint_soda", 2000] },
      { op: "equipItem", args: ["skyboxes", "skybox_mint_soda"] },
      { op: "unlockItem", args: ["gizmo", "gizmo_1", 10] },
      { op: "equipItem", args: ["gizmo", "gizmo_1"] },
      { op: "spendCredits", args: [1e9] },
      { op: "spendCredits", args: [1] },
      { op: "addCredits", args: [-50] },
      { op: "addCredits", args: [0.99] },
      { op: "addCredits", args: [1e21] },
      { op: "resetProfile" },
    ],
  },
  {
    name: "campaign",
    ops: [
      t(T0), c(),
      { op: "victoryPayout", args: [250, 20, 20] },
      { op: "recordSectorCompleted", args: ["level_1"] },
      { op: "recordSectorCompleted", args: ["level_1"] },
      { op: "victoryPayout", args: [495, 61, 26] },
      { op: "recordSectorCompleted", args: ["level_50"] },
      { op: "victoryPayout", args: [0, 10, 40] },
      { op: "recordContractComplete", args: [false] },
      { op: "recordDebrisSwallowed", args: [1] },
      { op: "recordDebrisSwallowed", args: [48] },
      { op: "recordDebrisSwallowed", args: [1] },
      { op: "recordDebrisSwallowed", args: [199] },
      { op: "recordDebrisSwallowed", args: [1] },
      { op: "recordDebrisSwallowed", args: [749] },
      { op: "recordDebrisSwallowed", args: [1] },
      { op: "recordDebrisSwallowed", args: [5000] },
      { op: "checkAchievements" },
      { op: "recordContractComplete", args: [false, "silver"] },
    ],
  },
  {
    name: "daily",
    ops: [
      t(T0), c(),
      { op: "countdown" },
      { op: "canClaimDailyGift" },
      { op: "claimDailyGift", args: [100] },
      { op: "claimDailyGift", args: [100] },
      { op: "getDailyShifts" },
      { op: "getDailyShiftInfo" },
      { op: "canClaimDailyMasterBonus" },
      { op: "recordContractComplete", args: [true, "bronze"] },
      { op: "recordContractComplete", args: [true, "bronze"] },
      { op: "getDailyShifts" },
      { op: "claimDailyMasterBonus", args: [300] },
      { op: "recordContractComplete", args: [true, "silver"] },
      { op: "recordContractComplete", args: [true, "gold"] },
      { op: "canClaimDailyMasterBonus" },
      { op: "claimDailyMasterBonus", args: [300] },
      { op: "isDailyMasterClaimed" },
      { op: "claimDailyMasterBonus", args: [300] },
      t(D("2026-10-01T23:59:59.999Z")), { op: "countdown" }, { op: "canClaimDailyGift" },
      t(D("2026-10-02T00:00:00.000Z")), { op: "countdown" }, { op: "canClaimDailyGift" },
      { op: "isDailyMasterClaimed" },
      { op: "getDailyShifts" },
      c(),
      { op: "getDailyShifts" },
      { op: "claimDailyGift", args: [] },
      { op: "recordContractComplete", args: [true] },
      { op: "recordContractComplete", args: [true, ""] },
      { op: "recordContractComplete", args: [true, "gold"] },
      t(D("2026-10-02T07:00:00.000Z")), { op: "countdown" },
      t(D("2026-10-02T06:59:59.000Z")), { op: "countdown" },
      t(D("2026-11-01T08:30:00.000Z")), { op: "countdown" }, { op: "getDailyShifts" },
      t(D("2026-11-01T09:30:00.000Z")), { op: "countdown" },
      t(D("2026-11-02T07:59:59.999Z")), { op: "countdown" },
      t(D("2026-12-31T23:59:59.999Z")), { op: "canClaimDailyGift" }, { op: "getDailyShifts" },
      t(D("2027-01-01T00:00:00.000Z")), { op: "canClaimDailyGift" }, { op: "getDailyShifts" },
      t(D("2028-02-29T12:00:00.000Z")), { op: "claimDailyGift", args: [100] }, { op: "getDailyShifts" },
      t(D("2027-03-14T09:59:59.000Z")), { op: "countdown" },
      t(D("2027-03-14T10:00:00.000Z")), { op: "countdown" },
      t(D("2027-03-15T06:59:00.000Z")), { op: "countdown" },
      t(D("2026-10-01T07:00:00.000Z")), { op: "countdown" },
    ],
  },
  {
    name: "load",
    ops: [
      t(T0),
      { op: "seedStorage", text: null }, c(), { op: "getDefaultProfile" },
      { op: "seedStorage", text: '{"credits":5}' }, c(), { op: "addCredits", args: [1] },
      { op: "seedStorage", text: '{"credits":700,"careerDebris":3,"mystery":{"a":[1,2,{"b":null}]},"zeta":"z"}' }, c(), { op: "recordDebrisSwallowed", args: [0] },
      { op: "seedStorage", text: JSON.stringify({ version: 2, credits: 1234.5, careerDebris: 60, careerContracts: 2, equipped: { ring: "ring_duckie", singularity: "singularity_nibbles", skybox: "skybox_cotton_candy" }, unlocked: { rings: ["ring_donut", "ring_duckie"], singularities: ["singularity_nibbles"], skyboxes: ["skybox_cotton_candy"] }, settings: { soundEnabled: false, sfxVolume: 55, ambientVolume: 0, hapticsEnabled: false }, achievements: ["ach_first_drop"], completedSectors: ["level_1", "level_2"], dailyShiftCompletedToday: true, lastDailyShiftDate: "2026-10-01", completedDailyTiers: ["bronze", "gold"], dailyMasterClaimedDate: "2026-10-01", lastDailyGiftDate: "2026-09-30" }) },
      c(), { op: "getDailyShifts" }, { op: "claimDailyGift" }, { op: "checkAchievements" }, { op: "recordDebrisSwallowed", args: [0] },
      { op: "seedStorage", text: JSON.stringify({ credits: 10, lastDailyShiftDate: "2026-09-30", completedDailyTiers: ["silver"], dailyShiftCompletedToday: true, dailyMasterClaimedDate: "2026-09-30" }) },
      c(), { op: "getDailyShifts" }, { op: "recordDebrisSwallowed", args: [0] },
      { op: "seedStorage", text: JSON.stringify({ credits: 10, settings: null, completedSectors: null, completedDailyTiers: null, dailyMasterClaimedDate: null }) },
      c(), { op: "recordDebrisSwallowed", args: [0] },
      { op: "seedStorage", text: "{not json" }, c(), { op: "getDefaultProfile" }, { op: "addCredits", args: [2] },
      { op: "seedStorage", text: JSON.stringify({ unlocked: { rings: ["ring_donut"] }, equipped: { ring: "ring_donut" } }) }, c(),
      { op: "unlockItem", args: ["skyboxes", "skybox_cheese_moon", 0] }, { op: "equipItem", args: ["skybox", "skybox_cheese_moon"] },
    ],
  },
];

export function installFakeDate(clock) {
  const RealDate = Date;
  class FakeDate extends RealDate {
    constructor(...a) {
      if (a.length === 0) super(clock.epoch);
      else super(...a);
    }
    static now() {
      return clock.epoch;
    }
  }
  globalThis.Date = FakeDate;
  return () => {
    globalThis.Date = RealDate;
  };
}

function getDailyResetCountdown() {
  const now = new Date();
  const tomorrow = new Date(now);
  tomorrow.setHours(24, 0, 0, 0);
  const diff = tomorrow - now;
  const hours = Math.floor(diff / (1000 * 60 * 60));
  const mins = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
  return `Resets in ${hours}h ${mins}m`;
}

function memoryStorage() {
  const m = new Map();
  return {
    getItem: (k) => (m.has(k) ? m.get(k) : null),
    setItem: (k, v) => void m.set(k, String(v)),
    removeItem: (k) => void m.delete(k),
    clear: () => m.clear(),
  };
}

export function runEconomy(mods) {
  const { EconomyManager } = mods;
  const clock = { epoch: T0 };
  const restoreDate = installFakeDate(clock);
  const hadStorage = typeof globalThis.localStorage !== "undefined";
  if (!hadStorage) globalThis.localStorage = memoryStorage();
  try {
    const results = [];
    for (const seq of SEQUENCES) {
      localStorage.clear();
      clock.epoch = T0;
      let eco = null;
      const steps = [];
      for (const step of seq.ops) {
        const args = step.args ?? [];
        let ret;
        switch (step.op) {
          case "setTime": clock.epoch = step.ms; ret = undefined; break;
          case "construct": eco = new EconomyManager(); ret = undefined; break;
          case "seedStorage":
            if (step.text === null) localStorage.removeItem(KEY);
            else localStorage.setItem(KEY, step.text);
            ret = undefined;
            break;
          case "countdown": ret = getDailyResetCountdown(); break;
          case "victoryPayout": {
            const [reward, collected, quota] = args;
            const base = reward || 250;
            const surplus = Math.max(0, collected - quota);
            const total = base + surplus * 4;
            eco.addCredits(total);
            ret = total;
            break;
          }
          case "getDefaultProfile": ret = eco.getDefaultProfile(); break;
          case "resetProfile": ret = eco.resetProfile(); break;
          default: ret = eco[step.op](...args);
        }
        steps.push({
          op: step.op,
          args: args,
          ms: step.ms ?? null,
          text: step.text === undefined ? null : step.text,
          ret: ret === undefined ? "undefined" : JSON.stringify(ret),
          stored: localStorage.getItem(KEY),
          epoch: hex(clock.epoch),
        });
      }
      results.push({ name: seq.name, steps });
    }
    return results;
  } finally {
    restoreDate();
    if (!hadStorage) delete globalThis.localStorage;
  }
}
