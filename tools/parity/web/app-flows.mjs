/**
 * Drive the frozen web app through the parity harness and write golden traces
 * for the C# AppController. Does not modify games/maze-puzzle/web.
 *
 *   node web/app-flows.mjs
 *   node web/app-flows.mjs --only boot_home,shop_buy
 */
import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { launchBrowser, Session } from "./harness.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const outDir = join(here, "..", "golden", "app");

const norm = (s) => (s == null ? "" : String(s).replace(/\s+/g, " ").trim());

async function snap(session) {
  return session.page.evaluate(() => {
    const app = window.app;
    const norm = (s) => (s == null ? "" : String(s).replace(/\s+/g, " ").trim());
    const text = (id) => {
      const el = document.getElementById(id);
      return el ? norm(el.textContent) : null;
    };
    const modals = [...document.querySelectorAll(".modal-overlay.active")].map((e) => e.id);
    const toasts = [...document.querySelectorAll("#toast-container .toast")].map((e) => norm(e.textContent));
    const bar = document.getElementById("loading-bar-fill");
    const shopFooters = app.currentPage === "shop"
      ? [...document.querySelectorAll("#store-page-container .card-footer")].map((e) => norm(e.textContent))
      : null;
    const masterBtn = document.getElementById("btn-claim-master-bonus");
    let masterAction = null;
    if (app.currentPage === "daily") {
      if (masterBtn) masterAction = norm(masterBtn.textContent);
      else {
        const box = document.querySelector("#daily-master-box");
        const blob = box ? norm(box.textContent) : "";
        masterAction = blob.includes("CLAIMED") ? "CLAIMED ✓" : "+300 🪙";
      }
    }
    const backup = document.getElementById("defeat-backup-container");
    return {
      currentPage: app.currentPage,
      modals,
      credits: text("home-credits-val"),
      toasts,
      quota: text("hud-quota-text"),
      loadingBar: bar && bar.style && bar.style.width ? bar.style.width : "",
      loadingTip: text("loading-tip-text"),
      missionName: text("home-mission-name"),
      missionGoal: text("home-mission-goal"),
      missionWorld: text("home-mission-world"),
      dailyBounty: text("home-daily-bounty"),
      shopBadge: text("shop-dock-badge"),
      dailyBadge: text("dock-daily-badge"),
      giftBtn: text("btn-claim-daily-gift"),
      giftTitle: text("gift-banner-title"),
      soundIcon: text("home-audio-icon"),
      pauseName: text("pause-level-name"),
      debriefMass: text("debrief-mass-val"),
      debriefCredits: text("debrief-credits-val"),
      defeatMass: text("defeat-mass-val"),
      defeatSpilled: text("defeat-spilled-val"),
      backupVisible: !!(backup && backup.style.display !== "none" && modals.includes("modal-defeat")),
      storeTab: app.currentStoreTab,
      worldIndex: app.selectedWorldIndex,
      sectorIndex: app.currentSectorIndex,
      sectorId: app.activeSector ? app.activeSector.id : null,
      sectorCode: app.activeSector ? app.activeSector.code : null,
      isDaily: !!(app.activeSector && app.activeSector.isDaily),
      dailyTier: app.activeSector && app.activeSector.dailyTier ? app.activeSector.dailyTier : null,
      paused: !!app.isPaused,
      cleared: !!app.isLevelCleared,
      failed: !!app.isLevelFailed,
      usedBackup: !!app.usedBackupPod,
      shopFooters,
      masterAction,
      worldTitle: app.currentPage === "maps" ? norm((document.querySelector(".world-banner-title") || {}).textContent) : null,
      worldProgress: app.currentPage === "maps" ? norm((document.querySelector(".world-banner-progress") || {}).textContent) : null,
      firstLevelCode: app.currentPage === "maps" ? norm((document.querySelector(".level-badge") || {}).textContent) : null,
      dailyDone: app.economy.getDailyShifts().map((s) => !!s.isCompleted),
      nowMs: performance.now(),
      randomCalls: window.__parity.randomCalls(),
      epochMs: Date.now(),
      profile: JSON.stringify(app.economy.profile),
    };
  });
}

async function clickSel(session, selector, index = 0) {
  const ok = await session.page.evaluate(
    ({ selector, index }) => {
      const list = document.querySelectorAll(selector);
      const el = list[index];
      if (!el) return "missing " + selector + " [" + index + "] of " + list.length;
      el.click();
      return null;
    },
    { selector, index },
  );
  if (ok) throw new Error(ok);
  await session.endOp();
  session.checkErrors();
}

async function bootHome(session) {
  await session.boot({ completeLoading: true });
}

async function forceWin(session, mass) {
  await session.page.evaluate((m) => {
    window.app.collectedMass = m;
    window.app.updateQuotaDisplay();
    window.app.handleLevelVictory();
  }, mass);
  await session.endOp();
}

async function forceLose(session, collected, spilled) {
  await session.page.evaluate(({ collected, spilled }) => {
    window.app.collectedMass = collected;
    window.app.spilledMass = spilled;
    window.app.updateQuotaDisplay();
    window.app.handleLevelFailure();
  }, { collected, spilled });
  await session.endOp();
}

export const FLOWS = [
  {
    id: "boot_home",
    async run(session, trace) {
      await session.boot({ completeLoading: false });
      trace.push({ op: "boot", snap: await snap(session) });
      for (let i = 0; i < 30; i++) {
        if ((await session.page.evaluate(() => window.app.currentPage)) === "home") break;
        await session.advance(240);
        trace.push({ op: "advance", ms: 240, snap: await snap(session) });
      }
    },
  },
  {
    id: "shop_buy",
    async run(session, trace) {
      await bootHome(session);
      trace.push({ op: "bootHome", snap: await snap(session) });
      const quota = await session.page.evaluate(() => window.app.activeSector.quota);
      await forceWin(session, quota);
      trace.push({ op: "forceWin", collectedMass: quota, snap: await snap(session) });
      await session.advance(600);
      trace.push({ op: "advance", ms: 600, snap: await snap(session) });
      await clickSel(session, "#btn-debrief-next");
      trace.push({ op: "click", selector: "#btn-debrief-next", snap: await snap(session) });
      await clickSel(session, "#btn-open-shop-page");
      trace.push({ op: "click", selector: "#btn-open-shop-page", snap: await snap(session) });
      await clickSel(session, ".btn-buy-item", 0);
      trace.push({ op: "click", selector: ".btn-buy-item", index: 0, snap: await snap(session) });
    },
  },
  {
    id: "shop_equip",
    async run(session, trace) {
      await bootHome(session);
      trace.push({ op: "bootHome", snap: await snap(session) });
      const mass = await session.page.evaluate(() => window.app.activeSector.quota + 250);
      await forceWin(session, mass);
      trace.push({ op: "forceWin", collectedMass: mass, snap: await snap(session) });
      await session.advance(600);
      trace.push({ op: "advance", ms: 600, snap: await snap(session) });
      await clickSel(session, "#btn-debrief-next");
      trace.push({ op: "click", selector: "#btn-debrief-next", snap: await snap(session) });
      await clickSel(session, "#btn-open-shop-page");
      trace.push({ op: "click", selector: "#btn-open-shop-page", snap: await snap(session) });
      await clickSel(session, ".tab-btn", 1);
      trace.push({ op: "click", selector: ".tab-btn", index: 1, snap: await snap(session) });
      await clickSel(session, ".btn-buy-item", 0);
      trace.push({ op: "click", selector: ".btn-buy-item", index: 0, snap: await snap(session) });
      await clickSel(session, ".btn-equip-item", 0);
      trace.push({ op: "click", selector: ".btn-equip-item", index: 0, snap: await snap(session) });
    },
  },
  {
    id: "claim_gift",
    async run(session, trace) {
      await bootHome(session);
      trace.push({ op: "bootHome", snap: await snap(session) });
      await clickSel(session, "#btn-open-shop-page");
      trace.push({ op: "click", selector: "#btn-open-shop-page", snap: await snap(session) });
      await clickSel(session, "#btn-claim-daily-gift");
      trace.push({ op: "click", selector: "#btn-claim-daily-gift", snap: await snap(session) });
    },
  },
  {
    id: "daily_chain",
    async run(session, trace) {
      await bootHome(session);
      trace.push({ op: "bootHome", snap: await snap(session) });
      await clickSel(session, "#btn-open-daily-shift");
      trace.push({ op: "click", selector: "#btn-open-daily-shift", snap: await snap(session) });
      await clickSel(session, ".btn-play-daily-tier", 0);
      trace.push({ op: "click", selector: ".btn-play-daily-tier", index: 0, snap: await snap(session) });
      for (let i = 0; i < 3; i++) {
        const mass = await session.page.evaluate(() => window.app.activeSector.quota);
        await forceWin(session, mass);
        trace.push({ op: "forceWin", collectedMass: mass, snap: await snap(session) });
        await session.advance(600);
        trace.push({ op: "advance", ms: 600, snap: await snap(session) });
        await clickSel(session, "#btn-debrief-next");
        trace.push({ op: "click", selector: "#btn-debrief-next", snap: await snap(session) });
      }
      await session.advance(1200);
      trace.push({ op: "advance", ms: 1200, snap: await snap(session) });
      await clickSel(session, "#btn-claim-master-bonus");
      trace.push({ op: "click", selector: "#btn-claim-master-bonus", snap: await snap(session) });
    },
  },
  {
    id: "settings_reset",
    async run(session, trace) {
      await bootHome(session);
      trace.push({ op: "bootHome", snap: await snap(session) });
      await clickSel(session, "#btn-home-settings");
      trace.push({ op: "click", selector: "#btn-home-settings", snap: await snap(session) });
      await session.page.evaluate(() => {
        const sound = document.getElementById("settings-sound-toggle");
        sound.checked = false;
        sound.dispatchEvent(new Event("change", { bubbles: true }));
        const haptics = document.getElementById("settings-haptics-toggle");
        haptics.checked = false;
        haptics.dispatchEvent(new Event("change", { bubbles: true }));
        const sfx = document.getElementById("settings-sfx-slider");
        sfx.value = "40";
        sfx.dispatchEvent(new Event("input", { bubbles: true }));
        const amb = document.getElementById("settings-ambience-slider");
        amb.value = "15";
        amb.dispatchEvent(new Event("input", { bubbles: true }));
      });
      await session.endOp();
      trace.push({
        op: "settings",
        sound: false,
        haptics: false,
        sfx: 40,
        ambience: 15,
        snap: await snap(session),
      });
      await clickSel(session, "#btn-reset-game-data");
      trace.push({ op: "click", selector: "#btn-reset-game-data", snap: await snap(session) });
      await clickSel(session, "#btn-confirm-reset");
      trace.push({ op: "click", selector: "#btn-confirm-reset", snap: await snap(session) });
    },
  },
  {
    id: "maps_start",
    async run(session, trace) {
      await bootHome(session);
      trace.push({ op: "bootHome", snap: await snap(session) });
      await clickSel(session, "#btn-open-maps-page");
      trace.push({ op: "click", selector: "#btn-open-maps-page", snap: await snap(session) });
      await clickSel(session, ".world-tab-btn", 2);
      trace.push({ op: "click", selector: ".world-tab-btn", index: 2, snap: await snap(session) });
      await clickSel(session, ".world-tab-btn", 0);
      trace.push({ op: "click", selector: ".world-tab-btn", index: 0, snap: await snap(session) });
      await clickSel(session, ".level-tile", 3);
      trace.push({ op: "click", selector: ".level-tile", index: 3, snap: await snap(session) });
    },
  },
  {
    id: "level_win",
    async run(session, trace) {
      await bootHome(session);
      trace.push({ op: "bootHome", snap: await snap(session) });
      await clickSel(session, "#btn-home-play");
      trace.push({ op: "click", selector: "#btn-home-play", snap: await snap(session) });
      const mass = await session.page.evaluate(() => window.app.activeSector.quota + 5);
      await forceWin(session, mass);
      trace.push({ op: "forceWin", collectedMass: mass, snap: await snap(session) });
      await session.advance(600);
      trace.push({ op: "advance", ms: 600, snap: await snap(session) });
      await clickSel(session, "#btn-debrief-next");
      trace.push({ op: "click", selector: "#btn-debrief-next", snap: await snap(session) });
    },
  },
  {
    id: "defeat_backup",
    async run(session, trace) {
      await bootHome(session);
      trace.push({ op: "bootHome", snap: await snap(session) });
      await clickSel(session, "#btn-home-play");
      trace.push({ op: "click", selector: "#btn-home-play", snap: await snap(session) });
      const masses = await session.page.evaluate(() => {
        const quota = window.app.activeSector.quota;
        return { collected: Math.ceil(quota * 0.7), spilled: 9 };
      });
      await forceLose(session, masses.collected, masses.spilled);
      trace.push({
        op: "forceLose",
        collectedMass: masses.collected,
        spilledMass: masses.spilled,
        snap: await snap(session),
      });
      await session.advance(400);
      trace.push({ op: "advance", ms: 400, snap: await snap(session) });
      await clickSel(session, "#btn-defeat-backup");
      trace.push({ op: "click", selector: "#btn-defeat-backup", snap: await snap(session) });
      await forceLose(session, masses.collected, masses.spilled);
      trace.push({
        op: "forceLose",
        collectedMass: masses.collected,
        spilledMass: masses.spilled,
        snap: await snap(session),
      });
    },
  },
];

function selectorOf(step) {
  if (!step.selector) return null;
  const index = step.index || 0;
  if (step.selector.startsWith("#") && index === 0) return { id: step.selector.slice(1) };
  const token = step.selector.startsWith(".") ? step.selector.slice(1) : step.selector;
  return { token, index };
}

async function main() {
  const onlyArg = process.argv.indexOf("--only");
  const only = onlyArg >= 0 ? new Set(process.argv[onlyArg + 1].split(",")) : null;
  mkdirSync(outDir, { recursive: true });
  const browser = await launchBrowser();
  try {
    for (const flow of FLOWS) {
      if (only && !only.has(flow.id)) continue;
      const session = new Session(browser, { profile: "zero", stateId: flow.id });
      const trace = [];
      const started = Date.now();
      await flow.run(session, trace);
      const epochMs = trace[0] && trace[0].snap ? trace[0].snap.epochMs : null;
      const body = {
        id: flow.id,
        epochMs,
        trace: trace.map((step) => {
          const copy = { ...step };
          const sel = selectorOf(step);
          if (sel) Object.assign(copy, sel);
          return copy;
        }),
      };
      const file = join(outDir, flow.id + ".json");
      writeFileSync(file, JSON.stringify(body));
      console.log(flow.id, trace.length, "steps", Date.now() - started + "ms", file);
      await session.context.close();
    }
  } finally {
    await browser.close();
  }
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
