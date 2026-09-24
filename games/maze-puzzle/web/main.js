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
      "shop-credits-val",
      "ach-credits-val",
    ].forEach((id) => {
      const el = document.getElementById(id);
      if (el) el.textContent = credits;
    });
    this.updateShopDockBadge();
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

  updateHomeView() {
    this.updateAllCreditDisplays();
    this.updateShopDockBadge();

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
          this.showPage("home");
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
        this.selectedWorldIndex = "daily";
        this.showPage("maps");
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
        closePause();
        this.showPage("home");
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

    // 8. Physics Simulation Callbacks
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
        this.economy.saveProfile();
      });
    }

    if (ambienceSlider) {
      ambienceSlider.addEventListener("input", (e) => {
        this.economy.profile.settings.ambientVolume = parseInt(e.target.value, 10);
        this.economy.saveProfile();
      });
    }

    if (hapticsToggle) {
      hapticsToggle.addEventListener("change", (e) => {
        this.economy.profile.settings.hapticsEnabled = e.target.checked;
        this.economy.saveProfile();
      });
    }

    // Privacy Policy Modal
    const privacyModal = document.getElementById("modal-privacy");
    document.getElementById("btn-open-privacy").addEventListener("click", () => {
      privacyModal.classList.add("active");
    });
    document.getElementById("btn-close-privacy").addEventListener("click", () => {
      privacyModal.classList.remove("active");
    });
    document.getElementById("btn-dismiss-privacy").addEventListener("click", () => {
      privacyModal.classList.remove("active");
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
  }

  /* =========================================================
     Gameplay Sector Management
     ========================================================= */
  loadSector(sectorConfig, showToastMsg = true) {
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

    const baseReward = this.activeSector.rewardCredits || 250;
    const surplus = Math.max(0, this.collectedMass - this.activeSector.quota);
    const totalPayout = baseReward + surplus * 4;

    this.economy.addCredits(totalPayout);
    this.updateAllCreditDisplays();

    if (this.activeSector && !this.activeSector.isDaily && this.activeSector.id) {
      this.economy.recordSectorCompleted(this.activeSector.id);
    }

    const newlyUnlocked = this.economy.recordContractComplete(!!this.activeSector.isDaily);
    newlyUnlocked.forEach((ach) => {
      this.showToast(`🏆 Trophy: ${ach.title} (+${ach.reward} 🪙)`);
    });

    document.getElementById("debrief-mass-val").textContent = `${this.collectedMass} kg`;
    document.getElementById("debrief-credits-val").textContent = `+${totalPayout} 🪙`;

    setTimeout(() => {
      document.getElementById("modal-debrief").classList.add("active");
    }, 600);
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
      tabBtn.textContent = `${w.icon} W${w.worldIndex}`;
      tabBtn.addEventListener("click", () => {
        this.selectedWorldIndex = w.worldIndex;
        this.renderSectorsPage();
      });
      tabsRow.appendChild(tabBtn);
    });

    const dailyTab = document.createElement("button");
    dailyTab.className = `world-tab-btn ${
      this.selectedWorldIndex === "daily" ? "active" : ""
    }`;
    dailyTab.textContent = "📅 Daily Shift";
    dailyTab.addEventListener("click", () => {
      this.selectedWorldIndex = "daily";
      this.renderSectorsPage();
    });
    tabsRow.appendChild(dailyTab);
    container.appendChild(tabsRow);

    if (this.selectedWorldIndex === "daily") {
      const daily = this.economy.getDailyShiftInfo();
      const dailyCard = document.createElement("div");
      dailyCard.className = "sector-card";
      dailyCard.style.borderColor = "var(--yellow-main)";

      dailyCard.innerHTML = `
        <div class="card-title-row">
          <span class="card-title" style="color:var(--yellow-main);">★ ${daily.title}</span>
          <span style="font-size:11px; color:var(--yellow-main); font-weight:800;">${daily.isCompleted ? "COMPLETED" : "TODAY"}</span>
        </div>
        <div class="card-desc">${daily.hazardModifier}</div>
        <div class="card-footer">
          <span class="badge-cost">+${daily.rewardCredits} 🪙 Bounty</span>
          <button class="btn-cartoon-primary" id="btn-play-daily">
            ${daily.isCompleted ? "Play Again" : "Start Shift ▶"}
          </button>
        </div>
      `;
      container.appendChild(dailyCard);

      dailyCard.querySelector("#btn-play-daily").addEventListener("click", () => {
        const sector = MazeLevelBuilder.buildDailyShift(daily);
        this.loadSector(sector);
        this.showPage("game");
      });
      return;
    }

    const currentWorld =
      this.worlds.find((w) => w.worldIndex === this.selectedWorldIndex) ||
      this.worlds[0];

    const banner = document.createElement("div");
    banner.className = "world-banner-card";
    banner.innerHTML = `
      <div style="font-size: 32px;">${currentWorld.icon}</div>
      <div class="world-banner-info">
        <h3>${currentWorld.title || currentWorld.name}</h3>
        <p>${currentWorld.tagline} • Levels ${currentWorld.levelRange[0]}-${currentWorld.levelRange[1]}</p>
      </div>
    `;
    container.appendChild(banner);

    const grid = document.createElement("div");
    grid.className = "level-grid-50";

    const [startLvl, endLvl] = currentWorld.levelRange;
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
