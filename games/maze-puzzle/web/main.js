/**
 * Orbital Salvage: Pure Gesture Mobile App Controller
 * Multi-page game architecture: Loading Screen, Homepage, Settings,
 * dedicated Maps/Shop/Trophies pages, and active gameplay canvas.
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

// The only web place that names the legal pages. Both are placeholders.
// TODO: replace these with the real Privacy Policy and User Agreement URLs.
// TODO: Kids Category — opening an external link from a kids app may require a
// parental gate before App Store Kids Category review. This file does not build one.
const LEGAL_LINKS = {
  privacyPolicy: "https://www.bing.com",
  userAgreement: "https://www.bing.com",
};

// Leading marker on the current language button. U+2713 is not an emoji, so capture
// does not treat it as one. Endonyms are never translated; they stay in their own script.
const LANGUAGE_MARKER = "✓ ";

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
    this.spilledMass = 0;
    this.isLevelCleared = false;
    this.isLevelFailed = false;
    this.failureCheckTimer = 0;
    this.usedBackupPod = false;
    this.isPaused = false;

    // Gesture Input State
    this.isDragging = false;
    this.lastPointerX = 0;
    this.lastTapTime = 0;
    this.lastTapX = 0;

    // Active Store Tab
    this.currentStoreTab = "rings";
    this.currentPage = "loading";

    // Setup Layout & Listeners
    this.initLayout();
    this.bindEvents();
    this.setupMascotInteractions();
    this.setupSettingsPage();
    this.applyAudioVolumesFromProfile();
    this.loadSector(this.sectors[0], false);

    // Start Boot Sequence
    this.startBootSequence();

    // Start Simulation Loop
    this.lastTime = performance.now();
    requestAnimationFrame((t) => this.loop(t));
  }

  /* =========================================================
     Page Navigation Router
     ========================================================= */
  showPage(pageId) {
    this.currentPage = pageId;

    const allPages = [
      "loading",
      "home",
      "game",
      "maps",
      "daily",
      "shop",
      "achievements",
      "settings",
    ];

    allPages.forEach((p) => {
      const el = document.getElementById(`page-${p}`);
      if (el) {
        if (p === pageId) {
          el.classList.remove("page-hidden");
          el.classList.add("page-active");
        } else {
          el.classList.add("page-hidden");
          el.classList.remove("page-active");
        }
      }
    });

    this.updateAllCreditDisplays();

    if (pageId === "home") {
      this.updateHomeView();
    } else if (pageId === "maps") {
      this.renderSectorsPage();
    } else if (pageId === "daily") {
      this.renderDailyPage();
    } else if (pageId === "shop") {
      this.renderStorePage();
    } else if (pageId === "achievements") {
      this.renderAchievementsPage();
    } else if (pageId === "settings") {
      this.syncSettingsUI();
    } else if (pageId === "game") {
      this.initLayout();
      // Dismiss gesture hint after 3.5 seconds automatically
      setTimeout(() => this.dismissGestureHint(), 3500);
    }
  }

  startBootSequence() {
    const bar = document.getElementById("loading-bar-fill");
    const tip = document.getElementById("loading-tip-text");
    const tips = [
      "Calibrating cosmic centrifuges...",
      "Polishing space soda cans...",
      "Waking up Nibbles for breakfast...",
      "Sprinkling candy on orbital rings...",
      "Ready for cleanup launch! 🚀",
    ];

    let progress = 12;
    let tipIdx = 0;

    const interval = setInterval(() => {
      progress += Math.floor(Math.random() * 22) + 14;
      if (progress > 100) progress = 100;

      if (bar) bar.style.width = `${progress}%`;
      tipIdx = (tipIdx + 1) % tips.length;
      if (tip) tip.textContent = tips[tipIdx];

      if (progress >= 100) {
        clearInterval(interval);
        setTimeout(() => {
          this.showPage("home");
        }, 350);
      }
    }, 240);
  }

  updateAllCreditDisplays() {
    const credits = this.economy.profile.credits;
    [
      "home-credits-val",
      "hud-credits-val",
      "maps-credits-val",
      "daily-credits-val",
      "shop-credits-val",
      "ach-credits-val",
    ].forEach((id) => {
      const el = document.getElementById(id);
      if (el) el.textContent = credits;
    });
    this.updateShopDockBadge();
    this.updateDailyDockBadge();
  }

  updateShopDockBadge() {
    const badge = document.getElementById("shop-dock-badge");
    if (!badge) return;
    const canClaim = this.economy.canClaimDailyGift();
    if (canClaim) {
      badge.style.display = "inline-block";
      badge.textContent = "FREE";
    } else {
      badge.style.display = "none";
    }
  }

  updateDailyDockBadge() {
    const badge = document.getElementById("dock-daily-badge");
    if (!badge) return;
    const shifts = this.economy.getDailyShifts();
    const completedCount = shifts.filter((s) => s.isCompleted).length;
    if (completedCount === 3) {
      if (this.economy.canClaimDailyMasterBonus()) {
        badge.textContent = "GIFT";
        badge.className = "dock-badge dock-badge-free";
        badge.style.background = "";
        badge.style.color = "";
      } else {
        badge.textContent = "✓";
        badge.className = "dock-badge";
        badge.style.background = "#22c55e";
        badge.style.color = "#ffffff";
      }
    } else {
      badge.textContent = `${3 - completedCount} NEW`;
      badge.className = "dock-badge dock-badge-hot";
      badge.style.background = "";
      badge.style.color = "";
    }
  }

  updateHomeView() {
    this.updateAllCreditDisplays();
    this.updateShopDockBadge();
    this.updateDailyDockBadge();

    // 1. Mission Info
    const sec = this.activeSector || this.sectors[0];
    const nameEl = document.getElementById("home-mission-name");
    const goalEl = document.getElementById("home-mission-goal");
    const worldEl = document.getElementById("home-mission-world");
    if (nameEl) nameEl.textContent = `${sec.code} • ${sec.name}`;
    if (goalEl) goalEl.textContent = `Goal: ${sec.quota} kg`;
    if (worldEl) {
      const curWorld = this.worlds.find((w) => w.worldIndex === (sec.worldIndex || 1));
      worldEl.textContent = curWorld ? curWorld.title : `World ${sec.worldIndex || 1}`;
    }

    // 2. Daily Shift Bounty Preview
    const dailyBountyEl = document.getElementById("home-daily-bounty");
    if (dailyBountyEl) {
      const dailyInfo = this.economy.getDailyShiftInfo();
      dailyBountyEl.textContent = `+${dailyInfo.rewardCredits} 🪙 Bounty`;
    }

    // 3. Update Mascot Diorama with Equipped Skins
    const equippedRing = this.economy.profile.equipped.ring || "ring_donut";
    const equippedSingularity = this.economy.profile.equipped.singularity || "singularity_nibbles";

    const ringEl = document.getElementById("home-diorama-ring");
    const ringLabelEl = document.getElementById("home-diorama-ring-label");
    const nibblesEl = document.getElementById("home-diorama-nibbles");
    const faceEl = document.getElementById("home-diorama-face");

    if (ringEl && ringLabelEl) {
      if (equippedRing === "ring_duckie") {
        ringEl.style.borderColor = "#facc15";
        ringEl.style.boxShadow = "0 0 25px rgba(250, 204, 21, 0.5), inset 0 0 15px rgba(250, 204, 21, 0.3)";
        ringLabelEl.textContent = "🦆 DUCKIE FLOAT";
      } else if (equippedRing === "ring_candyland") {
        ringEl.style.borderColor = "#ef4444";
        ringEl.style.boxShadow = "0 0 25px rgba(239, 68, 68, 0.5), inset 0 0 15px rgba(239, 68, 68, 0.3)";
        ringLabelEl.textContent = "🍬 CANDY RING";
      } else if (equippedRing === "ring_spacecat") {
        ringEl.style.borderColor = "#c084fc";
        ringEl.style.boxShadow = "0 0 25px rgba(192, 132, 252, 0.5), inset 0 0 15px rgba(192, 132, 252, 0.3)";
        ringLabelEl.textContent = "🐱 KITTY SAUCER";
      } else {
        ringEl.style.borderColor = "#f472b6";
        ringEl.style.boxShadow = "0 0 25px rgba(244, 114, 182, 0.5), inset 0 0 15px rgba(244, 114, 182, 0.3)";
        ringLabelEl.textContent = "🍩 DONUT FLOAT";
      }
    }

    if (nibblesEl && faceEl) {
      if (equippedSingularity === "singularity_strawberry") {
        nibblesEl.style.background = "radial-gradient(circle at 35% 35%, #f43f5e 0%, #881337 75%, #4c0519 100%)";
        nibblesEl.style.borderColor = "#fda4af";
        nibblesEl.style.boxShadow = "0 0 22px rgba(244, 63, 94, 0.6)";
        faceEl.textContent = "🍓";
      } else if (equippedSingularity === "singularity_sleepy") {
        nibblesEl.style.background = "radial-gradient(circle at 35% 35%, #38bdf8 0%, #0369a1 75%, #082f49 100%)";
        nibblesEl.style.borderColor = "#7dd3fc";
        nibblesEl.style.boxShadow = "0 0 22px rgba(56, 189, 248, 0.6)";
        faceEl.textContent = "💤";
      } else if (equippedSingularity === "singularity_confetti") {
        nibblesEl.style.background = "radial-gradient(circle at 35% 35%, #a855f7 0%, #581c87 75%, #3b0764 100%)";
        nibblesEl.style.borderColor = "#facc15";
        nibblesEl.style.boxShadow = "0 0 22px rgba(168, 85, 247, 0.6)";
        faceEl.textContent = "🎉";
      } else {
        nibblesEl.style.background = "radial-gradient(circle at 35% 35%, #6366f1 0%, #1e1b4b 75%, #0f0c29 100%)";
        nibblesEl.style.borderColor = "#38bdf8";
        nibblesEl.style.boxShadow = "0 0 22px rgba(56, 189, 248, 0.6)";
        faceEl.textContent = "😋";
      }
    }
  }

  setupMascotInteractions() {
    const stage = document.getElementById("home-mascot-stage");
    const nibbles = document.getElementById("home-diorama-nibbles");
    const anchor = document.getElementById("home-hearts-anchor");
    if (!stage || !nibbles) return;

    stage.addEventListener("click", () => {
      try {
        this.audio.publicInit();
        this.audio.playBoosterPass();
      } catch (err) {
        // audio context may be restricted in headless testing
      }
      this.triggerHaptic(30);

      // 1. Squash & Stretch animation
      nibbles.classList.remove("squash");
      void nibbles.offsetWidth; // re-flow
      nibbles.classList.add("squash");

      // 2. Change face momentarily
      const face = document.getElementById("home-diorama-face");
      if (face) {
        const orig = face.textContent;
        face.textContent = "🥰";
        setTimeout(() => {
          if (face) face.textContent = orig;
        }, 600);
      }

      // 3. Spawn floating heart / star particle
      if (anchor) {
        const p = document.createElement("div");
        p.className = "spawned-particle";
        const emojis = ["❤️", "✨", "⭐", "💖", "🌟"];
        p.textContent = emojis[Math.floor(Math.random() * emojis.length)];
        const dx = (Math.random() - 0.5) * 80;
        p.style.setProperty("--dx", `${dx}px`);
        p.style.left = "50%";
        p.style.top = "50%";
        anchor.appendChild(p);
        setTimeout(() => {
          if (p.parentNode) p.parentNode.removeChild(p);
        }, 1200);
      }
    });
  }

  initLayout() {
    const w = this.frame ? this.frame.clientWidth : window.innerWidth;
    const h = this.frame ? this.frame.clientHeight : window.innerHeight;
    this.renderer.resize(w, h);

    // Centered layout proportionally balanced for iOS 430pt x 932pt
    const scale = Math.min(1.0, w / 430);
    const centerX = w * 0.5;
    const centerY = Math.max(175, h * 0.38);
    const blackHoleY = Math.min(h - 110, Math.max(centerY + 240, h * 0.76));

    this.physics.center = { x: centerX, y: centerY };
    this.physics.blackHole.x = centerX;
    this.physics.blackHole.y = blackHoleY;
  }

  dismissGestureHint() {
    if (this.gestureHint && !this.gestureHint.classList.contains("hidden")) {
      this.gestureHint.classList.add("hidden");
    }
  }

  resetGameplaySessionUi() {
    this.isPaused = false;
    this.isLevelCleared = false;
    this.isLevelFailed = false;
    this.failureCheckTimer = 0;
    this.usedBackupPod = false;
    ["modal-pause", "modal-debrief", "modal-defeat"].forEach((id) => {
      const el = document.getElementById(id);
      if (el) el.classList.remove("active");
    });
  }

  exitGameToHome() {
    this.resetGameplaySessionUi();
    this.showPage("home");
  }

  applyAudioVolumesFromProfile() {
    const s = (this.economy.profile && this.economy.profile.settings) || {};
    if (this.audio) {
      this.audio.setSfxVolume(s.sfxVolume !== undefined ? s.sfxVolume : 80);
      this.audio.setAmbienceVolume(
        s.ambientVolume !== undefined ? s.ambientVolume : 70,
      );
    }
  }

  /* =========================================================
     Event Listeners & Input Binding
     ========================================================= */
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
        // Quick double tap gives a fun whoosh burst
        const rect = this.canvas.getBoundingClientRect();
        const tapNormX = (e.clientX - rect.left) / rect.width;
        const dir = tapNormX > 0.5 ? 1 : -1;
        this.physics.applyRcsBurst(dir);
        this.triggerHaptic(50);
      }
      this.lastTapTime = now;
      this.lastTapX = e.clientX;
    });

    window.addEventListener("pointermove", (e) => {
      if (!this.isDragging) return;
      const deltaX = e.clientX - this.lastPointerX;
      this.lastPointerX = e.clientX;
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
      } else if (e.key === "Escape") {
        if (this.currentPage === "game") {
          this.exitGameToHome();
        }
      }
    });

    // 3. Audio Mute Toggle (HUD & Home)
    const toggleAudioHandler = () => {
      this.audio.publicInit();
      const isMuted = this.audio.toggleMute();
      const icon = isMuted ? "🔇" : "🔊";
      const hudIcon = document.getElementById("audio-icon");
      const homeIcon = document.getElementById("home-audio-icon");
      if (hudIcon) hudIcon.textContent = icon;
      if (homeIcon) homeIcon.textContent = icon;
      this.economy.profile.settings.soundEnabled = !isMuted;
      this.economy.saveProfile();
      this.showToast(isMuted ? "Sound Off 🔇" : "Sound On 🔊");
    };

    const btnToggleAudio = document.getElementById("btn-toggle-audio");
    if (btnToggleAudio) btnToggleAudio.addEventListener("click", toggleAudioHandler);

    const btnHomeAudio = document.getElementById("btn-home-audio");
    if (btnHomeAudio) btnHomeAudio.addEventListener("click", toggleAudioHandler);

    // 4. Homepage Navigation Buttons
    document.getElementById("btn-home-play").addEventListener("click", () => {
      this.audio.publicInit();
      this.showPage("game");
    });

    document.getElementById("btn-open-maps-page").addEventListener("click", () => {
      this.audio.publicInit();
      this.showPage("maps");
    });

    document.getElementById("btn-open-shop-page").addEventListener("click", () => {
      this.audio.publicInit();
      this.showPage("shop");
    });

    const btnDailyShift = document.getElementById("btn-open-daily-shift");
    if (btnDailyShift) {
      btnDailyShift.addEventListener("click", () => {
        this.audio.publicInit();
        this.showPage("daily");
      });
    }

    document.getElementById("btn-open-trophies-page").addEventListener("click", () => {
      this.audio.publicInit();
      this.showPage("achievements");
    });

    const openSettings = () => {
      this.audio.publicInit();
      this.showPage("settings");
    };
    const btnOpenSettings = document.getElementById("btn-open-settings-page");
    if (btnOpenSettings) btnOpenSettings.addEventListener("click", openSettings);
    const btnHomeSettings = document.getElementById("btn-home-settings");
    if (btnHomeSettings) btnHomeSettings.addEventListener("click", openSettings);

    // 5. Back to Home Buttons from Sub-Pages
    const backMaps = document.getElementById("btn-back-maps");
    if (backMaps) backMaps.addEventListener("click", () => this.showPage("home"));
    const backDaily = document.getElementById("btn-back-daily");
    if (backDaily) backDaily.addEventListener("click", () => this.showPage("home"));
    const backShop = document.getElementById("btn-back-shop");
    if (backShop) backShop.addEventListener("click", () => this.showPage("home"));
    const backAch = document.getElementById("btn-back-achievements");
    if (backAch) backAch.addEventListener("click", () => this.showPage("home"));
    const backSet = document.getElementById("btn-back-settings");
    if (backSet) backSet.addEventListener("click", () => this.showPage("home"));
    const btnGameToHome = document.getElementById("btn-game-to-home");
    if (btnGameToHome) btnGameToHome.addEventListener("click", () => this.showPage("home"));

    const btnRestart = document.getElementById("btn-game-restart");
    if (btnRestart) {
      btnRestart.addEventListener("click", () => {
        this.restartSector();
      });
    }

    // 5b. In-Game Pause Modal Handlers
    const pauseModal = document.getElementById("modal-pause");
    const openPause = () => {
      this.isPaused = true;
      const pauseLvlName = document.getElementById("pause-level-name");
      const pauseLvlQuota = document.getElementById("pause-level-quota");
      if (pauseLvlName && this.activeSector) {
        pauseLvlName.textContent = `${this.activeSector.code} • ${this.activeSector.name}`;
      }
      if (pauseLvlQuota && this.activeSector) {
        pauseLvlQuota.textContent = `Goal: Feed Nibbles ${this.activeSector.quota} kg (${this.collectedMass}/${this.activeSector.quota} kg eaten)`;
      }
      const pauseSoundBtn = document.getElementById("btn-pause-sound");
      if (pauseSoundBtn) {
        pauseSoundBtn.innerHTML = `<span id="pause-sound-icon">${this.audio.isMuted ? "🔇" : "🔊"}</span> Sound: ${this.audio.isMuted ? "OFF" : "ON"}`;
      }
      if (pauseModal) pauseModal.classList.add("active");
    };

    const closePause = () => {
      this.isPaused = false;
      if (pauseModal) pauseModal.classList.remove("active");
    };

    const btnGamePause = document.getElementById("btn-game-pause");
    if (btnGamePause) btnGamePause.addEventListener("click", openPause);

    const btnPauseResume = document.getElementById("btn-pause-resume");
    if (btnPauseResume) btnPauseResume.addEventListener("click", closePause);

    const btnClosePause = document.getElementById("btn-close-pause");
    if (btnClosePause) btnClosePause.addEventListener("click", closePause);

    const btnPauseRestart = document.getElementById("btn-pause-restart");
    if (btnPauseRestart) {
      btnPauseRestart.addEventListener("click", () => {
        closePause();
        this.restartSector();
      });
    }

    const btnPauseSound = document.getElementById("btn-pause-sound");
    if (btnPauseSound) {
      btnPauseSound.addEventListener("click", () => {
        this.audio.publicInit();
        const isMuted = this.audio.toggleMute();
        btnPauseSound.innerHTML = `<span id="pause-sound-icon">${isMuted ? "🔇" : "🔊"}</span> Sound: ${isMuted ? "OFF" : "ON"}`;
        const homeIcon = document.getElementById("home-audio-icon");
        if (homeIcon) homeIcon.textContent = isMuted ? "🔇" : "🔊";
        this.economy.profile.settings.soundEnabled = !isMuted;
        this.economy.saveProfile();
      });
    }

    const btnPauseMaps = document.getElementById("btn-pause-maps");
    if (btnPauseMaps) {
      btnPauseMaps.addEventListener("click", () => {
        closePause();
        this.showPage("maps");
      });
    }

    const btnPauseHome = document.getElementById("btn-pause-home");
    if (btnPauseHome) {
      btnPauseHome.addEventListener("click", () => {
        this.exitGameToHome();
      });
    }

    // 6. Toy Shop Tabs
    document.querySelectorAll(".tab-btn").forEach((btn) => {
      btn.addEventListener("click", () => {
        document.querySelectorAll(".tab-btn").forEach((b) => b.classList.remove("active"));
        btn.classList.add("active");
        this.currentStoreTab = btn.dataset.tab;
        this.renderStorePage();
      });
    });

    // 7. Debrief Victory Pop-up Buttons
    document.getElementById("btn-debrief-replay").addEventListener("click", () => {
      document.getElementById("modal-debrief").classList.remove("active");
      this.restartSector();
    });
    document.getElementById("btn-debrief-next").addEventListener("click", () => {
      document.getElementById("modal-debrief").classList.remove("active");
      this.nextSector();
    });

    // Defeat (Shift Incomplete) Buttons
    const modalDefeat = document.getElementById("modal-defeat");
    const btnDefeatRetry = document.getElementById("btn-defeat-retry");
    if (btnDefeatRetry) {
      btnDefeatRetry.addEventListener("click", () => {
        if (modalDefeat) modalDefeat.classList.remove("active");
        this.restartSector();
      });
    }

    const btnDefeatHome = document.getElementById("btn-defeat-home");
    if (btnDefeatHome) {
      btnDefeatHome.addEventListener("click", () => {
        this.resetGameplaySessionUi();
        this.showPage("maps");
      });
    }

    const btnDefeatBackup = document.getElementById("btn-defeat-backup");
    if (btnDefeatBackup) {
      btnDefeatBackup.addEventListener("click", () => {
        if (modalDefeat) modalDefeat.classList.remove("active");
        this.usedBackupPod = true;
        this.isLevelFailed = false;
        this.failureCheckTimer = 0;
        this.physics.spawnBackupScrap(8);
        this.audio.playBackupPodSound();
        this.triggerHaptic(50);
        this.showToast("🚀 Auxiliary Scrap Pod Deployed! (+8 Scrap)");
      });
    }

    // 8. Physics Simulation Callbacks
    this.physics.onDebrisSpilled = (d) => {
      this.spilledMass += d.weight * 2;
    };

    this.physics.onDebrisSwallowed = (d) => {
      this.collectedMass += d.weight * 2;
      this.updateQuotaDisplay();
      this.renderer.triggerNibblesChew();
      this.triggerHaptic(20);

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

      const newlyUnlocked = this.economy.recordDebrisSwallowed(1);
      newlyUnlocked.forEach((ach) => {
        this.showToast(`🏆 Trophy: ${ach.title} (+${ach.reward} 🪙)`);
      });

      if (!this.isLevelCleared && this.collectedMass >= this.activeSector.quota) {
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

    this.physics.onBoostTriggered = (dir) => {
      const cx = this.physics.center.x;
      const cy = this.physics.center.y;
      const rOut = this.physics.ringRadius;
      this.renderer.addBoostShockwave(cx, cy, rOut, "#38bdf8");
      const label = dir > 0 ? "TURBO! ⏩" : "⏪ TURBO!";
      this.renderer.addFloatingText(cx, cy - rOut * 0.4, label, "#38bdf8");
      if (this.audio && this.audio.ctx) {
        this.audio.playBoosterPass(this.audio.ctx.currentTime);
      }
      this.triggerHaptic(40);
    };
  }

  triggerHaptic(ms = 30) {
    if (
      this.economy.profile.settings &&
      this.economy.profile.settings.hapticsEnabled &&
      navigator.vibrate
    ) {
      try {
        navigator.vibrate(ms);
      } catch (e) {}
    }
  }

  /* =========================================================
     (3) Settings Page Wiring
     ========================================================= */
  setupSettingsPage() {
    const soundToggle = document.getElementById("settings-sound-toggle");
    const sfxSlider = document.getElementById("settings-sfx-slider");
    const ambienceSlider = document.getElementById("settings-ambience-slider");
    const hapticsToggle = document.getElementById("settings-haptics-toggle");

    if (soundToggle) {
      soundToggle.addEventListener("change", (e) => {
        this.economy.profile.settings.soundEnabled = e.target.checked;
        if (this.audio.isMuted === e.target.checked) {
          this.audio.toggleMute();
        }
        this.economy.saveProfile();
      });
    }

    if (sfxSlider) {
      sfxSlider.addEventListener("input", (e) => {
        this.economy.profile.settings.sfxVolume = parseInt(e.target.value, 10);
        this.audio.publicInit();
        this.audio.setSfxVolume(this.economy.profile.settings.sfxVolume);
        this.economy.saveProfile();
      });
    }

    if (ambienceSlider) {
      ambienceSlider.addEventListener("input", (e) => {
        this.economy.profile.settings.ambientVolume = parseInt(e.target.value, 10);
        this.audio.publicInit();
        this.audio.setAmbienceVolume(this.economy.profile.settings.ambientVolume);
        this.economy.saveProfile();
      });
    }

    if (hapticsToggle) {
      hapticsToggle.addEventListener("change", (e) => {
        this.economy.profile.settings.hapticsEnabled = e.target.checked;
        this.economy.saveProfile();
      });
    }

    // Privacy Policy and User Agreement leave the app. The privacy modal markup stays
    // for the captured spec; the button no longer opens it. See openPrivacyModal.
    document.getElementById("btn-open-privacy").addEventListener("click", () => {
      this.openExternal(LEGAL_LINKS.privacyPolicy);
    });
    document.getElementById("btn-open-user-agreement").addEventListener("click", () => {
      this.openExternal(LEGAL_LINKS.userAgreement);
    });
    const privacyModal = document.getElementById("modal-privacy");
    document.getElementById("btn-close-privacy").addEventListener("click", () => {
      privacyModal.classList.remove("active");
    });
    document.getElementById("btn-dismiss-privacy").addEventListener("click", () => {
      privacyModal.classList.remove("active");
    });

    const languageModal = document.getElementById("modal-language");
    document.getElementById("btn-open-language").addEventListener("click", () => {
      this.syncLanguageRow();
      languageModal.classList.add("active");
    });
    document.getElementById("btn-close-language").addEventListener("click", () => {
      languageModal.classList.remove("active");
    });
    document.querySelectorAll(".lang-option").forEach((btn) => {
      btn.addEventListener("click", () => {
        const code = btn.getAttribute("data-lang");
        if (!code) return;
        if (!this.economy.profile.settings) this.economy.profile.settings = {};
        this.economy.profile.settings.language = code;
        this.economy.saveProfile();
        this.syncLanguageRow();
        languageModal.classList.remove("active");
      });
    });

    // Reset Progress Modal
    const resetModal = document.getElementById("modal-reset-confirm");
    document.getElementById("btn-reset-game-data").addEventListener("click", () => {
      resetModal.classList.add("active");
    });
    document.getElementById("btn-cancel-reset").addEventListener("click", () => {
      resetModal.classList.remove("active");
    });
    document.getElementById("btn-confirm-reset").addEventListener("click", () => {
      this.economy.resetProfile();
      resetModal.classList.remove("active");
      this.loadSector(this.sectors[0], false);
      this.syncSettingsUI();
      this.updateAllCreditDisplays();
      this.updateHomeView();
      this.showToast("Progress Reset to Day 1! 🧹");
      this.showPage("home");
    });
  }

  syncSettingsUI() {
    const s = this.economy.profile.settings || {
      soundEnabled: true,
      sfxVolume: 80,
      ambientVolume: 70,
      hapticsEnabled: true,
    };
    const soundToggle = document.getElementById("settings-sound-toggle");
    const sfxSlider = document.getElementById("settings-sfx-slider");
    const ambienceSlider = document.getElementById("settings-ambience-slider");
    const hapticsToggle = document.getElementById("settings-haptics-toggle");

    if (soundToggle) soundToggle.checked = !!s.soundEnabled;
    if (sfxSlider) sfxSlider.value = s.sfxVolume !== undefined ? s.sfxVolume : 80;
    if (ambienceSlider) ambienceSlider.value = s.ambientVolume !== undefined ? s.ambientVolume : 70;
    if (hapticsToggle) hapticsToggle.checked = !!s.hapticsEnabled;
    this.syncLanguageRow();
    this.applyAudioVolumesFromProfile();
  }

  // Capture-only. The privacy row opens LEGAL_LINKS.privacyPolicy instead.
  openPrivacyModal() {
    const privacyModal = document.getElementById("modal-privacy");
    if (privacyModal) privacyModal.classList.add("active");
  }

  openExternal(url) {
    if (!url) return;
    window.open(url, "_blank", "noopener");
  }

  syncLanguageRow() {
    const settings = (this.economy.profile && this.economy.profile.settings) || {};
    const code = settings.language || "en";
    const buttons = document.querySelectorAll(".lang-option");
    let endonym = "English";
    buttons.forEach((btn) => {
      const lang = btn.getAttribute("data-lang");
      const name = btn.getAttribute("data-endonym") || btn.textContent.replace(/^✓\s*/, "");
      if (!btn.getAttribute("data-endonym")) btn.setAttribute("data-endonym", name);
      const selected = lang === code;
      btn.textContent = (selected ? LANGUAGE_MARKER : "") + name;
      if (selected) endonym = name;
    });
    const value = document.getElementById("settings-language-value");
    if (value) value.textContent = endonym;
  }

  /* =========================================================
     Gameplay Sector Management
     ========================================================= */
  loadSector(sectorConfig, showToastMsg = true) {
    this.activeSector = sectorConfig;
    this.collectedMass = 0;
    this.spilledMass = 0;
    this.resetGameplaySessionUi();
    this.renderer.resetNibblesScale();

    if (sectorConfig.worldIndex) {
      this.selectedWorldIndex = sectorConfig.worldIndex;
    }

    this.physics.initSector(
      sectorConfig,
      this.physics.center,
      this.physics.blackHole,
    );

    document.getElementById("hud-sector-code").textContent = `⭐ ${sectorConfig.code}`;
    this.updateQuotaDisplay();
    this.updateAllCreditDisplays();
    if (showToastMsg) {
      this.showToast(`Clean up ${sectorConfig.name}! 🚀`);
    }
  }

  restartSector() {
    this.loadSector(this.activeSector);
  }

  nextSector() {
    if (this.activeSector && this.activeSector.isDaily) {
      const shifts = this.economy.getDailyShifts();
      const currentTier = this.activeSector.dailyTier;
      let nextTier = null;
      if (currentTier === "bronze") {
        nextTier = shifts.find((s) => s.tier === "silver");
      } else if (currentTier === "silver") {
        nextTier = shifts.find((s) => s.tier === "gold");
      }

      if (nextTier) {
        const sector = MazeLevelBuilder.buildDailyShift(nextTier);
        this.loadSector(sector);
      } else {
        this.showPage("daily");
      }
      return;
    }
    this.currentSectorIndex =
      (this.currentSectorIndex + 1) % this.sectors.length;
    this.loadSector(this.sectors[this.currentSectorIndex]);
  }

  handleLevelVictory() {
    this.isLevelCleared = true;
    this.audio.playVictoryFanfare();

    const baseReward = this.activeSector.rewardCredits || 250;
    const surplus = Math.max(0, this.collectedMass - this.activeSector.quota);
    const totalPayout = baseReward + surplus * 4;

    this.economy.addCredits(totalPayout);
    this.updateAllCreditDisplays();

    if (this.activeSector && !this.activeSector.isDaily && this.activeSector.id) {
      this.economy.recordSectorCompleted(this.activeSector.id);
    }

    const isDaily = !!(this.activeSector && this.activeSector.isDaily);
    const dailyTier = this.activeSector ? this.activeSector.dailyTier || "silver" : "silver";
    const newlyUnlocked = this.economy.recordContractComplete(isDaily, dailyTier);
    newlyUnlocked.forEach((ach) => {
      this.showToast(`🏆 Trophy: ${ach.title} (+${ach.reward} 🪙)`);
    });

    if (isDaily) {
      const shifts = this.economy.getDailyShifts();
      const completedCount = shifts.filter((s) => s.isCompleted).length;
      if (completedCount === 3 && this.economy.canClaimDailyMasterBonus()) {
        setTimeout(() => {
          this.showToast("🎉 All 3 Daily Shifts Cleared! Claim Master Bonus in Daily Tab! 🏆");
        }, 1200);
      }
    }

    document.getElementById("debrief-mass-val").textContent = `${this.collectedMass} kg`;
    document.getElementById("debrief-credits-val").textContent = `+${totalPayout} 🪙`;

    setTimeout(() => {
      document.getElementById("modal-debrief").classList.add("active");
    }, 600);
  }

  handleLevelFailure() {
    if (this.isLevelCleared || this.isLevelFailed) return;
    this.isLevelFailed = true;
    this.audio.playDefeatSound();

    const targetQuota = this.activeSector ? this.activeSector.quota : 30;
    const massEl = document.getElementById("defeat-mass-val");
    if (massEl) massEl.textContent = `${this.collectedMass} / ${targetQuota} kg`;

    const spilledEl = document.getElementById("defeat-spilled-val");
    if (spilledEl) spilledEl.textContent = `${this.spilledMass} kg`;

    const backupContainer = document.getElementById("defeat-backup-container");
    const progressRatio = this.collectedMass / Math.max(1, targetQuota);
    if (backupContainer) {
      if (progressRatio >= 0.70 && !this.usedBackupPod) {
        backupContainer.style.display = "block";
      } else {
        backupContainer.style.display = "none";
      }
    }

    setTimeout(() => {
      const modalDefeat = document.getElementById("modal-defeat");
      if (modalDefeat) modalDefeat.classList.add("active");
    }, 400);
  }

  updateQuotaDisplay() {
    const current = this.collectedMass;
    const target = this.activeSector ? this.activeSector.quota : 30;
    const pct = Math.min(100, (current / target) * 100);

    const txt = document.getElementById("hud-quota-text");
    const bar = document.getElementById("hud-quota-bar");
    if (txt) txt.textContent = `${current} / ${target} kg`;
    if (bar) {
      bar.style.width = `${pct}%`;
      bar.style.background =
        current >= target
          ? "linear-gradient(90deg, #4ade80, #facc15)"
          : "linear-gradient(90deg, #facc15, #f472b6)";
    }
  }

  showToast(msg) {
    const container = document.getElementById("toast-container");
    if (!container) return;
    const toast = document.createElement("div");
    toast.className = "toast";
    toast.textContent = msg;
    container.appendChild(toast);
    setTimeout(() => {
      if (toast.parentNode) toast.parentNode.removeChild(toast);
    }, 2500);
  }

  /* =========================================================
     (4) Dedicated Maps Page
     ========================================================= */
  renderSectorsPage() {
    const container = document.getElementById("sectors-page-container");
    if (!container) return;
    container.innerHTML = "";

    // 1. World Tabs Row (Worlds 1-5 + Daily)
    const tabsRow = document.createElement("div");
    tabsRow.className = "world-tabs-row";

    this.worlds.forEach((w) => {
      const tabBtn = document.createElement("button");
      tabBtn.className = `world-tab-btn ${
        this.selectedWorldIndex === w.worldIndex ? "active" : ""
      }`;
      tabBtn.innerHTML = `<span>${w.icon}</span> <span>${w.levelRange[0]}-${w.levelRange[1]}</span>`;
      tabBtn.addEventListener("click", () => {
        this.selectedWorldIndex = w.worldIndex;
        this.renderSectorsPage();
      });
      tabsRow.appendChild(tabBtn);
    });

    container.appendChild(tabsRow);

    if (typeof this.selectedWorldIndex !== "number" || this.selectedWorldIndex < 1 || this.selectedWorldIndex > 5) {
      this.selectedWorldIndex = 1;
    }

    const currentWorld =
      this.worlds.find((w) => w.worldIndex === this.selectedWorldIndex) ||
      this.worlds[0];

    const [startLvl, endLvl] = currentWorld.levelRange;
    const completedSectors = this.economy.profile.completedSectors || [];
    let worldCompletedCount = 0;
    for (let l = startLvl; l <= endLvl; l++) {
      const sec = this.sectors[l - 1];
      if (sec && completedSectors.includes(sec.id)) {
        worldCompletedCount++;
      }
    }
    const worldTotalCount = endLvl - startLvl + 1;

    const banner = document.createElement("div");
    banner.className = "world-banner-card";
    banner.innerHTML = `
      <div class="world-banner-icon-box">${currentWorld.icon}</div>
      <div class="world-banner-info">
        <div class="world-banner-title">${currentWorld.title || currentWorld.name}</div>
        <div class="world-banner-desc">${currentWorld.tagline}</div>
      </div>
      <div class="world-banner-progress">⭐ ${worldCompletedCount}/${worldTotalCount}</div>
    `;
    container.appendChild(banner);

    const grid = document.createElement("div");
    grid.className = "level-grid-50";

    for (let l = startLvl; l <= endLvl; l++) {
      const secIdx = l - 1;
      const sec = this.sectors[secIdx];
      if (!sec) continue;

      const isCurrent = this.activeSector && this.activeSector.id === sec.id;
      const isCompleted = (this.economy.profile.completedSectors || []).includes(sec.id);

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
        this.showPage("game");
      });

      grid.appendChild(tile);
    }

    container.appendChild(grid);
  }

  /* =========================================================
     (4b) Dedicated Daily Operations Page
     ========================================================= */
  getDailyResetCountdown() {
    const now = new Date();
    const tomorrow = new Date(now);
    tomorrow.setHours(24, 0, 0, 0);
    const diff = tomorrow - now;
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const mins = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    return `Resets in ${hours}h ${mins}m`;
  }

  renderDailyPage() {
    const container = document.getElementById("daily-page-container");
    if (!container) return;
    container.innerHTML = "";

    const shifts = this.economy.getDailyShifts();
    const completedCount = shifts.filter((s) => s.isCompleted).length;
    const canClaimMaster = this.economy.canClaimDailyMasterBonus();
    const isMasterClaimed = this.economy.isDailyMasterClaimed();
    const resetTime = this.getDailyResetCountdown();

    // 1. Daily Operations Header Banner & Master Bonus Container
    const hubBanner = document.createElement("div");
    hubBanner.className = "daily-hub-banner";
    hubBanner.innerHTML = `
      <div class="daily-hub-header">
        <div class="daily-hub-title"><span>📅</span> DAILY OPERATIONS</div>
        <div style="display:flex; gap:6px; align-items:center;">
          <span style="font-size:10.5px; color:rgba(255,255,255,0.75); background:rgba(255,255,255,0.1); padding:2px 8px; border-radius:999px;">⏰ ${resetTime}</span>
          <div class="daily-hub-pill">${completedCount} / 3 CLEARED</div>
        </div>
      </div>
      <div class="daily-progress-container">
        <div style="display:flex; justify-content:space-between; font-size:10px; font-weight:800; color:rgba(255,255,255,0.7); margin-bottom:5px; letter-spacing:0.3px;">
          <span style="color:${shifts[0].isCompleted ? '#4ade80' : 'rgba(255,255,255,0.7)'}">🥉 BRONZE ${shifts[0].isCompleted ? '✓' : ''}</span>
          <span style="color:${shifts[1].isCompleted ? '#4ade80' : 'rgba(255,255,255,0.7)'}">🥈 SILVER ${shifts[1].isCompleted ? '✓' : ''}</span>
          <span style="color:${shifts[2].isCompleted ? '#4ade80' : 'rgba(255,255,255,0.7)'}">🥇 GOLD ${shifts[2].isCompleted ? '✓' : ''}</span>
        </div>
        <div class="daily-progress-track">
          <div class="daily-progress-fill" style="width: ${(completedCount / 3) * 100}%"></div>
        </div>
      </div>
      <div class="daily-master-box ${canClaimMaster ? "unlocked" : isMasterClaimed ? "claimed" : ""}" id="daily-master-box">
        <div class="daily-master-info">
          <div class="daily-master-icon">${isMasterClaimed ? "✨" : canClaimMaster ? "🎁" : "🔒"}</div>
          <div>
            <div class="daily-master-label">Master All-Clear Bounty</div>
            <div class="daily-master-sub">
              ${
                isMasterClaimed
                  ? "Bonus claimed today! Refreshes tomorrow."
                  : canClaimMaster
                  ? "All 3 shifts completed! Claim your reward!"
                  : "Complete all 3 daily shifts to unlock +300 🪙"
              }
            </div>
          </div>
        </div>
        ${
          canClaimMaster
            ? `<button class="btn-cartoon-primary" id="btn-claim-master-bonus" style="padding: 6px 14px; white-space: nowrap;">Claim +300 🪙</button>`
            : isMasterClaimed
            ? `<span style="font-size: 11px; font-weight: 800; color: #4ade80;">CLAIMED ✓</span>`
            : `<span class="badge-cost" style="opacity: 0.6; font-size: 11px;">+300 🪙</span>`
        }
      </div>
    `;
    container.appendChild(hubBanner);

    if (canClaimMaster) {
      const claimBtn = hubBanner.querySelector("#btn-claim-master-bonus");
      if (claimBtn) {
        claimBtn.addEventListener("click", () => {
          if (this.economy.claimDailyMasterBonus(300)) {
            try {
              this.audio.publicInit();
              this.audio.playBoosterPass();
            } catch (e) {}
            this.triggerHaptic(50);
            this.updateAllCreditDisplays();
            this.showToast("🏆 Claimed +300 🪙 Daily Master Bonus!");
            this.renderDailyPage();
          }
        });
      }
    }

    // 2. Render 3 Tier Shift Cards
    shifts.forEach((shift) => {
      const card = document.createElement("div");
      card.className = "sector-card daily-shift-card";
      card.style.borderColor = shift.themeColor;
      card.style.background = shift.isCompleted
        ? "rgba(255, 255, 255, 0.04)"
        : "rgba(255, 255, 255, 0.08)";

      card.innerHTML = `
        <div class="card-title-row">
          <span class="daily-tier-tag" style="background: ${shift.themeColor}25; color: ${shift.themeColor}; border: 1.5px solid ${shift.themeColor};">
            ${shift.tierBadge}
          </span>
          <span style="font-size:11px; font-weight:800; color: ${shift.isCompleted ? "#4ade80" : "var(--yellow-main);"}">
            ${shift.isCompleted ? "CLEARED ✓" : "TODAY"}
          </span>
        </div>
        <div class="card-title" style="font-size:14.5px; margin-top:2px;">
          ${shift.title}
        </div>
        <div class="daily-hazard-chip">
          <span>⚠️</span>
          <span>${shift.hazardModifier}</span>
        </div>
        <div class="card-footer">
          <div style="display:flex; gap:8px; align-items:center;">
            <span class="badge-cost">+${shift.rewardCredits} 🪙</span>
            <span style="font-size:11px; color:rgba(255,255,255,0.6);">Goal: ${shift.quota} kg</span>
          </div>
          <button class="btn-cartoon-primary btn-play-daily-tier" style="${shift.isCompleted ? "background:rgba(255,255,255,0.2); color:#fff;" : ""}">
            ${shift.isCompleted ? "Play Again" : "Start Shift ▶"}
          </button>
        </div>
      `;
      container.appendChild(card);

      card.querySelector(".btn-play-daily-tier").addEventListener("click", () => {
        const sector = MazeLevelBuilder.buildDailyShift(shift);
        this.loadSector(sector);
        this.showPage("game");
      });
    });
  }

  /* =========================================================
     (5) Dedicated Toy Shop Page
     ========================================================= */
  renderStorePage() {
    // 1. Update Daily Janitor Stipend Banner
    const giftBanner = document.getElementById("daily-gift-banner");
    const giftBtn = document.getElementById("btn-claim-daily-gift");
    const giftTitle = document.getElementById("gift-banner-title");
    const giftDesc = document.getElementById("gift-banner-desc");
    const giftIcon = document.getElementById("gift-banner-icon");

    if (giftBanner && giftBtn) {
      const canClaim = this.economy.canClaimDailyGift();
      if (canClaim) {
        giftBanner.classList.remove("claimed");
        giftBtn.textContent = "Claim +100 🪙";
        giftBtn.disabled = false;
        if (giftTitle) giftTitle.textContent = "Janitor Care Package!";
        if (giftDesc) giftDesc.textContent = "Claim your free daily +100 🪙 allowance.";
        if (giftIcon) giftIcon.textContent = "🎁";
      } else {
        giftBanner.classList.add("claimed");
        giftBtn.textContent = "Claimed Today ✨";
        giftBtn.disabled = true;
        if (giftTitle) giftTitle.textContent = "Allowance Claimed! ✨";
        if (giftDesc) giftDesc.textContent = "Come back tomorrow for another +100 🪙 care package!";
        if (giftIcon) giftIcon.textContent = "🎉";
      }

      if (!giftBtn._bound) {
        giftBtn._bound = true;
        giftBtn.addEventListener("click", () => {
          if (!this.economy.canClaimDailyGift()) return;
          const claimed = this.economy.claimDailyGift(100);
          if (claimed) {
            try {
              this.audio.publicInit();
              this.audio.playBoosterPass();
            } catch (e) {}
            this.triggerHaptic(40);
            this.updateAllCreditDisplays();
            this.renderStorePage();
            this.showToast("🎁 Claimed +100 🪙 Janitor Stipend!");
          }
        });
      }
    }

    const container = document.getElementById("store-page-container");
    if (!container) return;
    container.innerHTML = "";
    const tab = this.currentStoreTab;
    const items = COSMETIC_CATALOG[tab] || [];
    const singularCat = this.economy.normalizeCategory(tab).singular;
    const equippedId = this.economy.profile.equipped[singularCat];

    items.forEach((item) => {
      const card = document.createElement("div");
      card.className = "store-card";
      const isUnlocked = this.economy.isUnlocked(tab, item.id);
      const isEquipped = equippedId === item.id;
      const isFreeStarter = item.cost === 0;

      card.innerHTML = `
        <div class="card-title-row">
          <span class="card-title">${item.name}</span>
          <div style="display:flex; align-items:center; gap:6px;">
            ${isFreeStarter ? `<span style="font-family:var(--font-bubble); font-size:10px; color:var(--green-main); font-weight:900; background:rgba(74,222,128,0.15); border:1.5px solid var(--green-main); border-radius:999px; padding:1px 6px;">STARTER FREE</span>` : ""}
            ${item.color ? `<span style="display:inline-block; width:16px; height:16px; border-radius:50%; background:${item.color}; border:2px solid #1e1b4b;"></span>` : ""}
            ${item.haloColor ? `<span style="display:inline-block; width:16px; height:16px; border-radius:50%; background:${item.haloColor}; border:2px solid #1e1b4b;"></span>` : ""}
          </div>
        </div>
        <div class="card-desc">${item.desc}</div>
        <div class="card-footer">
          ${
            isEquipped
              ? `<span class="badge-equipped">EQUIPPED ✨</span>`
              : isUnlocked
                ? `<button class="btn-cartoon-secondary btn-equip-item" data-tab="${tab}" data-id="${item.id}">Wear</button>`
                : `<button class="btn-cartoon-primary btn-buy-item" data-tab="${tab}" data-id="${item.id}" data-cost="${item.cost}">${isFreeStarter ? "Free Starter" : `Unlock (${item.cost} 🪙)`}</button>`
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
          this.economy.equipItem(btn.dataset.tab, btn.dataset.id);
          this.updateAllCreditDisplays();
          this.renderStorePage();
          this.updateHomeView();
          this.showToast("Item Unlocked & Equipped! 🎉");
        } else {
          this.showToast("Need more gold coins! 🪙");
        }
      });
    });

    container.querySelectorAll(".btn-equip-item").forEach((btn) => {
      btn.addEventListener("click", () => {
        this.economy.equipItem(btn.dataset.tab, btn.dataset.id);
        this.renderStorePage();
        this.updateHomeView();
        this.showToast("Item Equipped! ✨");
      });
    });
  }

  /* =========================================================
     (6) Dedicated Trophies Page
     ========================================================= */
  renderAchievementsPage() {
    const container = document.getElementById("achievements-page-container");
    if (!container) return;
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

  /* =========================================================
     Main Simulation Loop
     ========================================================= */
  loop(currentTime) {
    const dt = Math.min((currentTime - this.lastTime) / 1000, 0.05);
    this.lastTime = currentTime;

    try {
      if (this.currentPage === "game") {
        // 1. Update Physics (only when not paused)
        if (!this.isPaused) {
          this.physics.update(dt);

          // Check defeat / out-of-scrap condition
          if (!this.isLevelCleared && !this.isLevelFailed && this.activeSector) {
            const hasDebrisInPlay = this.physics.debrisList.some(
              (d) =>
                d.state === "IN_RING" ||
                d.state === "FALLING" ||
                d.state === "INGESTING",
            );
            if (!hasDebrisInPlay && this.collectedMass < this.activeSector.quota) {
              this.failureCheckTimer += dt;
              if (this.failureCheckTimer >= 1.2) {
                this.handleLevelFailure();
              }
            } else {
              this.failureCheckTimer = 0;
            }
          }
        }

        // 2. Render Canvas with Dynamic Black Hole Growth Info
        this.renderer.render(this.physics, this.economy, dt, {
          collectedMass: this.collectedMass,
          quota: this.activeSector ? this.activeSector.quota : 30,
        });
      }
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
