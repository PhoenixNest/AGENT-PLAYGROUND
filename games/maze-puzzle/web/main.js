/**
 * Orbital Salvage: Pure Gesture Mobile App Controller
 * Full-screen tactile swipe gestures, Nibbles chewing animations,
 * procedural sound, and the toy shop metagame.
 */

import { AudioEngine } from "./engine/audio.js";
import {
  EconomyManager,
  COSMETIC_CATALOG,
  ACHIEVEMENTS,
} from "./engine/economy.js";
import { MazeLevelBuilder } from "./engine/maze.js";
import { PhysicsEngine } from "./engine/physics.js";
import { GameRenderer } from "./engine/renderer.js";

class OrbitalApp {
  constructor() {
    this.frame = document.getElementById("mobile-frame");
    this.canvas = document.getElementById("game-canvas");
    this.gestureHint = document.getElementById("gesture-hint");
    this.audio = new AudioEngine();
    this.economy = new EconomyManager();
    this.physics = new PhysicsEngine(this.audio);
    this.renderer = new GameRenderer(this.canvas);

    // 50-Level Campaign Progression
    this.sectors = MazeLevelBuilder.getSectors();
    this.worlds = MazeLevelBuilder.getSectorWorlds();
    this.selectedWorldIndex = 1;
    this.currentSectorIndex = 0;
    this.activeSector = null;
    this.collectedMass = 0;
    this.isLevelCleared = false;

    // Gesture Input State
    this.isDragging = false;
    this.lastPointerX = 0;
    this.lastTapTime = 0;
    this.lastTapX = 0;

    // Active Store Tab
    this.currentStoreTab = "rings";

    // Setup
    this.initLayout();
    this.bindEvents();
    this.loadSector(this.sectors[0]);

    // Dismiss gesture hint after 3.5 seconds automatically
    setTimeout(() => this.dismissGestureHint(), 3500);

    // Start Game Loop
    this.lastTime = performance.now();
    requestAnimationFrame((t) => this.loop(t));
  }

  initLayout() {
    const w = this.frame ? this.frame.clientWidth : window.innerWidth;
    const h = this.frame ? this.frame.clientHeight : window.innerHeight;
    this.renderer.resize(w, h);

    // Centered layout with generous vertical fall space
    const centerX = w * 0.5;
    const centerY = Math.max(170, h * 0.38);
    const blackHoleY = Math.min(h - 105, centerY + 230);

    this.physics.center = { x: centerX, y: centerY };
    this.physics.blackHole.x = centerX;
    this.physics.blackHole.y = blackHoleY;
  }

  dismissGestureHint() {
    if (this.gestureHint && !this.gestureHint.classList.contains("hidden")) {
      this.gestureHint.classList.add("hidden");
    }
  }

  bindEvents() {
    window.addEventListener("resize", () => {
      this.initLayout();
    });

    // 1. Full-Screen Tactile Touch & Pointer Swipe Rotation
    this.canvas.addEventListener("pointerdown", (e) => {
      this.audio.publicInit();
      this.dismissGestureHint();
      this.isDragging = true;
      this.lastPointerX = e.clientX;

      // Double-Tap Quick Spin Boost
      const now = performance.now();
      const timeSinceLastTap = now - this.lastTapTime;
      const distFromLastTap = Math.abs(e.clientX - this.lastTapX);

      if (timeSinceLastTap < 280 && distFromLastTap < 45) {
        // Quick double tap on left or right half gives a fun whoosh burst
        const rect = this.canvas.getBoundingClientRect();
        const tapNormX = (e.clientX - rect.left) / rect.width;
        const dir = tapNormX > 0.5 ? 1 : -1;
        this.physics.applyRcsBurst(dir);
      }
      this.lastTapTime = now;
      this.lastTapX = e.clientX;
    });

    window.addEventListener("pointermove", (e) => {
      if (!this.isDragging) return;
      const deltaX = e.clientX - this.lastPointerX;
      this.lastPointerX = e.clientX;
      // 1:1 direct tactile angular response with inertia
      this.physics.applyInputDrag(deltaX * 1.3);
    });

    window.addEventListener("pointerup", () => {
      this.isDragging = false;
    });

    // 2. Keyboard Hotkeys (Convenient for PC Testing)
    window.addEventListener("keydown", (e) => {
      this.audio.publicInit();
      this.dismissGestureHint();
      if (e.key === "a" || e.key === "A" || e.key === "ArrowLeft") {
        this.physics.applyRcsBurst(-1);
      } else if (e.key === "d" || e.key === "D" || e.key === "ArrowRight") {
        this.physics.applyRcsBurst(1);
      } else if (e.key === " " || e.code === "Space") {
        this.physics.applyRcsBurst(1);
      } else if (e.key === "r" || e.key === "R") {
        this.restartSector();
      }
    });

    // 3. Audio Mute Toggle
    document
      .getElementById("btn-toggle-audio")
      .addEventListener("click", () => {
        this.audio.publicInit();
        const isMuted = this.audio.toggleMute();
        document.getElementById("audio-icon").textContent = isMuted
          ? "🔇"
          : "🔊";
        this.showToast(isMuted ? "Sound Off 🔇" : "Sound On 🔊");
      });

    // 4. Bottom Tab Modals
    this.bindModal(
      "btn-open-sectors",
      "modal-sectors",
      "btn-close-sectors",
      () => this.renderSectorsModal(),
    );
    this.bindModal("btn-open-store", "modal-store", "btn-close-store", () =>
      this.renderStoreModal(),
    );
    this.bindModal(
      "btn-open-achievements",
      "modal-achievements",
      "btn-close-achievements",
      () => this.renderAchievementsModal(),
    );
    document
      .getElementById("btn-restart-sector")
      .addEventListener("click", () => {
        this.restartSector();
      });

    // Toy Shop Tabs
    document.querySelectorAll(".tab-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        document
          .querySelectorAll(".tab-btn")
          .forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");
        this.currentStoreTab = btn.dataset.tab;
        this.renderStoreModal();
      });
    });

    // Debrief Victory Pop-up Buttons
    document
      .getElementById("btn-debrief-replay")
      .addEventListener("click", () => {
        document.getElementById("modal-debrief").classList.remove("active");
        this.restartSector();
      });
    document
      .getElementById("btn-debrief-next")
      .addEventListener("click", () => {
        document.getElementById("modal-debrief").classList.remove("active");
        this.nextSector();
      });

    // 5. Physics Simulation Callbacks
    this.physics.onDebrisSwallowed = (d) => {
      this.collectedMass += d.weight * 2;
      this.updateQuotaDisplay();

      // Trigger Nibbles chewing animation!
      this.renderer.triggerNibblesChew();

      // Cute pop text and star confetti
      const nomWords = ["NOM! 😋", "YUM! ✨", "GULP! 🍩", "TASTY! ⭐"];
      const word = nomWords[Math.floor(Math.random() * nomWords.length)];
      this.renderer.addFloatingText(
        this.physics.blackHole.x,
        this.physics.blackHole.y - 32,
        word,
        "#fde047",
      );
      this.renderer.addPopRing(
        this.physics.blackHole.x,
        this.physics.blackHole.y,
        46,
        "#facc15",
      );

      // Check achievement progression
      const newlyUnlocked = this.economy.recordDebrisSwallowed(1);
      newlyUnlocked.forEach((ach) => {
        this.showToast(`🏆 Trophy: ${ach.title} (+${ach.reward} 🪙)`);
      });

      // Check Victory
      if (
        !this.isLevelCleared &&
        this.collectedMass >= this.activeSector.quota
      ) {
        this.handleLevelVictory();
      }
    };

    this.physics.onGatePassed = (gate) => {
      const rot =
        gate.isInner && this.physics.concentricRings
          ? this.physics.innerRingAngle - this.physics.ringAngle
          : 0;
      const rMid =
        gate.rMin !== undefined && gate.rMax !== undefined
          ? (gate.rMin + gate.rMax) * 0.5
          : gate.r || 120;
      const gx =
        this.physics.center.x +
        Math.cos(gate.angle + rot + this.physics.ringAngle) * rMid;
      const gy =
        this.physics.center.y +
        Math.sin(gate.angle + rot + this.physics.ringAngle) * rMid;
      const label =
        gate.type === "MULTIPLY"
          ? `×${gate.value}! SHRED ⚡`
          : gate.type === "ADD"
            ? `+${gate.value}! COMPACT ✨`
            : `BOOST! ⏩`;

      this.renderer.addFloatingText(gx, gy - 16, label, "#fde047");
      this.renderer.addConfetti(gx, gy, 8);
    };

    this.physics.onDebrisExploded = (d) => {
      this.renderer.addConfetti(d.x, d.y, 22);
      this.renderer.addFloatingText(d.x, d.y - 20, "POOF! 💨", "#f472b6");
      this.showToast("💨 Spicy battery popped into confetti!");
    };
  }

  bindModal(openBtnId, modalId, closeBtnId, onOpen) {
    const modal = document.getElementById(modalId);
    document.getElementById(openBtnId).addEventListener("click", () => {
      if (onOpen) onOpen();
      modal.classList.add("active");
    });
    document.getElementById(closeBtnId).addEventListener("click", () => {
      modal.classList.remove("active");
    });
    modal.addEventListener("click", (e) => {
      if (e.target === modal) modal.classList.remove("active");
    });
  }

  loadSector(sectorConfig) {
    this.activeSector = sectorConfig;
    this.collectedMass = 0;
    this.isLevelCleared = false;
    this.renderer.resetNibblesScale();

    if (sectorConfig.worldIndex) {
      this.selectedWorldIndex = sectorConfig.worldIndex;
    }

    this.physics.initSector(
      sectorConfig,
      this.physics.center,
      this.physics.blackHole,
    );

    // Update HUD
    document.getElementById("hud-sector-code").textContent =
      `⭐ ${sectorConfig.code}`;
    this.updateQuotaDisplay();
    this.updateCreditsDisplay();
    this.showToast(`Clean up ${sectorConfig.name}! 🚀`);
  }

  restartSector() {
    this.loadSector(this.activeSector);
  }

  nextSector() {
    if (this.activeSector.isDaily) {
      this.loadSector(this.sectors[0]);
      return;
    }
    this.currentSectorIndex =
      (this.currentSectorIndex + 1) % this.sectors.length;
    this.loadSector(this.sectors[this.currentSectorIndex]);
  }

  handleLevelVictory() {
    this.isLevelCleared = true;
    this.audio.playVictoryFanfare();

    // Reward calculation
    const baseReward = this.activeSector.rewardCredits || 250;
    const surplus = Math.max(0, this.collectedMass - this.activeSector.quota);
    const totalPayout = baseReward + surplus * 4;

    this.economy.addCredits(totalPayout);
    this.updateCreditsDisplay();

    if (
      this.activeSector &&
      !this.activeSector.isDaily &&
      this.activeSector.id
    ) {
      this.economy.recordSectorCompleted(this.activeSector.id);
    }

    const newlyUnlocked = this.economy.recordContractComplete(
      !!this.activeSector.isDaily,
    );
    newlyUnlocked.forEach((ach) => {
      this.showToast(`🏆 Trophy: ${ach.title} (+${ach.reward} 🪙)`);
    });

    document.getElementById("debrief-mass-val").textContent =
      `${this.collectedMass} kg`;
    document.getElementById("debrief-credits-val").textContent =
      `+${totalPayout} 🪙`;

    setTimeout(() => {
      document.getElementById("modal-debrief").classList.add("active");
    }, 600);
  }

  updateQuotaDisplay() {
    const current = this.collectedMass;
    const target = this.activeSector ? this.activeSector.quota : 30;
    const pct = Math.min(100, (current / target) * 100);

    document.getElementById("hud-quota-text").textContent =
      `${current} / ${target} kg`;
    document.getElementById("hud-quota-bar").style.width = `${pct}%`;

    if (current >= target) {
      document.getElementById("hud-quota-bar").style.background =
        "linear-gradient(90deg, #4ade80, #facc15)";
    } else {
      document.getElementById("hud-quota-bar").style.background =
        "linear-gradient(90deg, #facc15, #f472b6)";
    }
  }

  updateCreditsDisplay() {
    document.getElementById("hud-credits-val").textContent =
      this.economy.profile.credits;
  }

  showToast(msg) {
    const container = document.getElementById("toast-container");
    const toast = document.createElement("div");
    toast.className = "toast";
    toast.textContent = msg;
    container.appendChild(toast);
    setTimeout(() => {
      if (toast.parentNode) toast.parentNode.removeChild(toast);
    }, 2500);
  }

  renderSectorsModal() {
    const container = document.getElementById("sectors-container");
    container.innerHTML = "";

    // 1. World Tabs Row (Worlds 1-5 + Daily)
    const tabsRow = document.createElement("div");
    tabsRow.className = "world-tabs-row";

    this.worlds.forEach((w) => {
      const tabBtn = document.createElement("button");
      tabBtn.className = `world-tab-btn ${
        this.selectedWorldIndex === w.worldIndex ? "active" : ""
      }`;
      tabBtn.textContent = `${w.icon} W${w.worldIndex}`;
      tabBtn.addEventListener("click", () => {
        this.selectedWorldIndex = w.worldIndex;
        this.renderSectorsModal();
      });
      tabsRow.appendChild(tabBtn);
    });

    // Daily Tab
    const dailyTab = document.createElement("button");
    dailyTab.className = `world-tab-btn ${
      this.selectedWorldIndex === "daily" ? "active" : ""
    }`;
    dailyTab.textContent = "📅 Daily Shift";
    dailyTab.addEventListener("click", () => {
      this.selectedWorldIndex = "daily";
      this.renderSectorsModal();
    });
    tabsRow.appendChild(dailyTab);
    container.appendChild(tabsRow);

    if (this.selectedWorldIndex === "daily") {
      // Render Daily Shift Card
      const daily = this.economy.getDailyShiftInfo();
      const dailyCard = document.createElement("div");
      dailyCard.className = "sector-card";
      dailyCard.style.borderColor = "var(--yellow-main)";

      const isDailyActive = this.activeSector && this.activeSector.isDaily;
      dailyCard.innerHTML = `
        <div class="card-title-row">
          <span class="card-title" style="color:var(--yellow-main);">★ ${daily.title}</span>
          <span style="font-size:11px; color:var(--yellow-main); font-weight:800;">${daily.isCompleted ? "COMPLETED" : "TODAY"}</span>
        </div>
        <div class="card-desc">${daily.hazardModifier}</div>
        <div class="card-footer">
          <span class="badge-cost">+${daily.rewardCredits} 🪙 Bounty</span>
          ${
            isDailyActive
              ? `<span class="badge-equipped">PLAYING</span>`
              : `<button class="btn-cartoon-primary btn-select-daily" style="background:var(--pink-main); color:#fff;">Start Party ▶</button>`
          }
        </div>
      `;
      container.appendChild(dailyCard);

      const dailyBtn = container.querySelector(".btn-select-daily");
      if (dailyBtn) {
        dailyBtn.addEventListener("click", () => {
          const dailyLevel = MazeLevelBuilder.buildDailyShift(daily);
          this.loadSector(dailyLevel);
          document.getElementById("modal-sectors").classList.remove("active");
        });
      }
      return;
    }

    // 2. Active World Banner
    const currentWorld =
      this.worlds.find((w) => w.worldIndex === this.selectedWorldIndex) ||
      this.worlds[0];
    const banner = document.createElement("div");
    banner.className = "world-banner";
    banner.innerHTML = `
      <div class="world-banner-info">
        <h4>${currentWorld.icon} World ${currentWorld.worldIndex}: ${currentWorld.title}</h4>
        <p>${currentWorld.tagline} • Levels ${currentWorld.levelRange[0]}-${currentWorld.levelRange[1]}</p>
      </div>
    `;
    container.appendChild(banner);

    // 3. 10-Level Responsive Grid for Selected World
    const grid = document.createElement("div");
    grid.className = "level-grid-50";

    const [startLvl, endLvl] = currentWorld.levelRange;
    for (let l = startLvl; l <= endLvl; l++) {
      const secIdx = l - 1;
      const sec = this.sectors[secIdx];
      if (!sec) continue;

      const isCurrent = this.activeSector && this.activeSector.id === sec.id;
      const isCompleted = (
        this.economy.profile.completedSectors || []
      ).includes(sec.id);

      const tile = document.createElement("div");
      tile.className = `level-tile ${isCurrent ? "active-level" : ""}`;
      tile.innerHTML = `
        <div class="level-tile-header">
          <span class="level-badge">${sec.code}</span>
          <span>${isCurrent ? "⭐ PLAY" : isCompleted ? "✅" : "▶"}</span>
        </div>
        <div class="level-name">${sec.name}</div>
        <div class="level-quota">Goal: ${sec.quota} kg</div>
      `;

      tile.addEventListener("click", () => {
        this.currentSectorIndex = secIdx;
        this.loadSector(sec);
        document.getElementById("modal-sectors").classList.remove("active");
      });

      grid.appendChild(tile);
    }

    container.appendChild(grid);
  }

  renderStoreModal() {
    const container = document.getElementById("store-container");
    container.innerHTML = "";
    const tab = this.currentStoreTab;
    const items = COSMETIC_CATALOG[tab] || [];
    const equippedId = this.economy.profile.equipped[tab.slice(0, -1)];

    items.forEach((item) => {
      const card = document.createElement("div");
      card.className = "store-card";
      const isUnlocked = this.economy.isUnlocked(tab, item.id);
      const isEquipped = equippedId === item.id;

      card.innerHTML = `
        <div class="card-title-row">
          <span class="card-title">${item.name}</span>
          ${item.color ? `<span style="display:inline-block; width:16px; height:16px; border-radius:50%; background:${item.color}; border:2px solid #1e1b4b;"></span>` : ""}
          ${item.haloColor ? `<span style="display:inline-block; width:16px; height:16px; border-radius:50%; background:${item.haloColor}; border:2px solid #1e1b4b;"></span>` : ""}
        </div>
        <div class="card-desc">${item.desc}</div>
        <div class="card-footer">
          ${
            isEquipped
              ? `<span class="badge-equipped">EQUIPPED ✨</span>`
              : isUnlocked
                ? `<button class="btn-cartoon-secondary btn-equip-item" data-tab="${tab}" data-id="${item.id}">Wear</button>`
                : `<button class="btn-cartoon-primary btn-buy-item" data-tab="${tab}" data-id="${item.id}" data-cost="${item.cost}">Unlock (${item.cost} 🪙)</button>`
          }
        </div>
      `;
      container.appendChild(card);
    });

    container.querySelectorAll(".btn-buy-item").forEach((btn) => {
      btn.addEventListener("click", () => {
        const cost = parseInt(btn.dataset.cost, 10);
        const success = this.economy.unlockItem(
          btn.dataset.tab,
          btn.dataset.id,
          cost,
        );
        if (success) {
          this.economy.equipItem(btn.dataset.tab.slice(0, -1), btn.dataset.id);
          this.updateCreditsDisplay();
          this.renderStoreModal();
          this.showToast("Item Unlocked & Equipped! 🎉");
        } else {
          this.showToast("Need more gold coins! 🪙");
        }
      });
    });

    container.querySelectorAll(".btn-equip-item").forEach((btn) => {
      btn.addEventListener("click", () => {
        this.economy.equipItem(btn.dataset.tab.slice(0, -1), btn.dataset.id);
        this.renderStoreModal();
        this.showToast("Item Equipped! ✨");
      });
    });
  }

  renderAchievementsModal() {
    const container = document.getElementById("achievements-container");
    container.innerHTML = "";
    const unlockedList = this.economy.profile.achievements;

    ACHIEVEMENTS.forEach((ach) => {
      const isDone = unlockedList.includes(ach.id);
      const card = document.createElement("div");
      card.className = "ach-card";
      card.style.opacity = isDone ? "1" : "0.6";

      card.innerHTML = `
        <div class="card-title-row">
          <span style="font-size:24px;">${ach.icon}</span>
          <span class="card-title" style="flex:1; margin-left:8px;">${ach.title}</span>
          <span style="font-size:12px; color:${isDone ? "var(--green-main)" : "rgba(255,255,255,0.6)"}; font-weight:900;">
            ${isDone ? "✓ UNLOCKED" : "LOCKED"}
          </span>
        </div>
        <div class="card-desc">${ach.desc}</div>
        <div class="card-footer">
          <span class="badge-cost">+${ach.reward} 🪙 Prize</span>
        </div>
      `;
      container.appendChild(card);
    });
  }

  loop(currentTime) {
    const dt = Math.min((currentTime - this.lastTime) / 1000, 0.05);
    this.lastTime = currentTime;

    try {
      // 1. Update Physics
      this.physics.update(dt);

      // 2. Render Canvas with Dynamic Black Hole Growth Info
      this.renderer.render(this.physics, this.economy, dt, {
        collectedMass: this.collectedMass,
        quota: this.activeSector ? this.activeSector.quota : 30,
      });
    } catch (err) {
      console.error("Simulation loop error:", err);
    }

    requestAnimationFrame((t) => this.loop(t));
  }
}

// Boot application on DOM ready
if (document.readyState === "loading") {
  window.addEventListener("DOMContentLoaded", () => {
    window.app = new OrbitalApp();
  });
} else {
  window.app = new OrbitalApp();
}
