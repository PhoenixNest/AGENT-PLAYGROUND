/**
 * Orbital Salvage: Cartoon Visual Renderer ("Cute Cosmic Janitor")
 * Renders cute cartoon starfields, chunky candy-colored orbital rings,
 * adorable space trash (soda cans, smiling satellites, golden bolts),
 * and "Nibbles" the hungry cartoon black hole with animated eyes and mouth!
 */

import { COSMETIC_CATALOG } from "./economy.js";

// Bulletproof roundRect polyfill for maximum cross-browser/webview compatibility
if (
  typeof CanvasRenderingContext2D !== "undefined" &&
  !CanvasRenderingContext2D.prototype.roundRect
) {
  CanvasRenderingContext2D.prototype.roundRect = function (x, y, w, h, radii = 0) {
    let r = radii;
    if (typeof r === "number") r = [r, r, r, r];
    else if (!Array.isArray(r)) r = [0, 0, 0, 0];
    const tl = r[0] || 0;
    const tr = r[1] || r[0] || 0;
    const br = r[2] || r[0] || 0;
    const bl = r[3] || r[1] || r[0] || 0;
    this.moveTo(x + tl, y);
    this.lineTo(x + w - tr, y);
    this.quadraticCurveTo(x + w, y, x + w, y + tr);
    this.lineTo(x + w, y + h - br);
    this.quadraticCurveTo(x + w, y + h, x + w - br, y + h);
    this.lineTo(x + bl, y + h);
    this.quadraticCurveTo(x, y + h, x, y + h - bl);
    this.lineTo(x, y + tl);
    this.quadraticCurveTo(x, y, x + tl, y);
    this.closePath();
    return this;
  };
}

export class GameRenderer {
  constructor(canvas) {
    this.canvas = canvas;
    this.ctx = canvas.getContext("2d");

    this.width = canvas.width;
    this.height = canvas.height;

    // Background Stars & Floating Cartoon Planets
    this.stars = this.generateCartoonStars(65);
    this.planets = this.generateCartoonPlanets(3);
    this.cosmicTime = 0;

    // Nibbles Facial & Dynamic Growth State
    this.nibblesBlinkTimer = 0;
    this.isBlinking = false;
    this.mouthOpenAmount = 0; // 0 = smiling, 1 = wide open NOM
    this.chewTimer = 0;
    this.currentBhScale = 1.0;
    this.targetBhScale = 1.0;
    this.bhWobble = 0;

    // Cartoon VFX (Confetti, Stars, Comic Pop Text)
    this.particles = [];
    this.floatingTexts = [];
    this.popRings = [];

    // Accretion Vortex Particles (Singularity Orbit)
    this.singularityParticles = this.generateSingularityParticles(32);

    // Comic Thruster Smoke Puffs & Sparks
    this.thrusterPuffs = [];
    this.sparkEmitTimer = 0;
  }

  resetNibblesScale() {
    this.currentBhScale = 1.0;
    this.targetBhScale = 1.0;
    this.bhWobble = 0;
  }

  resize(w, h) {
    this.width = w;
    this.height = h;
    this.canvas.width = w;
    this.canvas.height = h;
  }

  generateCartoonStars(count) {
    const stars = [];
    for (let i = 0; i < count; i++) {
      stars.push({
        x: Math.random(),
        y: Math.random(),
        size: Math.random() * 6 + 4,
        type: Math.random() > 0.4 ? "star" : "circle",
        color:
          Math.random() > 0.5
            ? "#fde047"
            : Math.random() > 0.3
              ? "#ffffff"
              : "#f472b6",
        speed: Math.random() * 2 + 1,
      });
    }
    return stars;
  }

  generateCartoonPlanets(count) {
    return [
      {
        x: 0.15,
        y: 0.18,
        radius: 24,
        color: "#f472b6",
        ring: true,
        ringColor: "#fde047",
      },
      {
        x: 0.82,
        y: 0.25,
        radius: 32,
        color: "#38bdf8",
        ring: false,
        spots: true,
      },
      {
        x: 0.88,
        y: 0.82,
        radius: 20,
        color: "#facc15",
        ring: false,
        spots: false,
      },
    ];
  }

  addFloatingText(x, y, text, color = "#fde047") {
    this.floatingTexts.push({
      x,
      y,
      text,
      color,
      alpha: 1.0,
      vy: -30,
      scale: 1.3,
      life: 0.85,
    });
  }

  addPopRing(x, y, radiusMax = 40, color = "#fde047") {
    this.popRings.push({
      x,
      y,
      radius: 6,
      radiusMax,
      color,
      alpha: 1.0,
    });
  }

  addConfetti(x, y, count = 12) {
    const colors = [
      "#f472b6",
      "#fde047",
      "#38bdf8",
      "#4ade80",
      "#c084fc",
      "#fb923c",
    ];
    for (let i = 0; i < count; i++) {
      const angle = Math.random() * Math.PI * 2;
      const speed = Math.random() * 110 + 40;
      this.particles.push({
        x,
        y,
        vx: Math.cos(angle) * speed,
        vy: Math.sin(angle) * speed - 20,
        gravity: 120,
        rot: Math.random() * Math.PI,
        vRot: (Math.random() - 0.5) * 8,
        life: 0.65,
        maxLife: 0.65,
        w: Math.random() * 6 + 4,
        h: Math.random() * 4 + 3,
        color: colors[Math.floor(Math.random() * colors.length)],
      });
    }
  }

  generateSingularityParticles(count) {
    const list = [];
    const colors = ["#fde047", "#38bdf8", "#f472b6", "#c084fc", "#ffffff", "#4ade80", "#fbbf24"];
    for (let i = 0; i < count; i++) {
      list.push({
        rRatio: 1.05 + Math.random() * 2.0,
        angle: Math.random() * Math.PI * 2,
        speed: Math.random() * 1.6 + 1.2,
        inwardRate: Math.random() * 0.35 + 0.22,
        size: Math.random() * 2.6 + 1.8,
        color: colors[Math.floor(Math.random() * colors.length)],
        twinkle: Math.random() * Math.PI * 2,
        twinkleSpeed: Math.random() * 4 + 2,
      });
    }
    return list;
  }

  updateSingularityParticles(physics, dt) {
    const bh = physics.blackHole;
    let closestDist = 9999;
    for (const d of physics.debrisList) {
      if (d.state === "FALLING" || d.state === "INGESTING") {
        const dist = Math.hypot(bh.x - d.x, bh.y - d.y);
        if (dist < closestDist) closestDist = dist;
      }
    }
    const suctionBoost = closestDist < 140 || this.chewTimer > 0 ? 1.85 : 1.0;

    for (const p of this.singularityParticles) {
      p.twinkle += dt * p.twinkleSpeed;
      // Keplerian swirl: closer orbits revolve faster
      const swirlSpeed = (p.speed / (p.rRatio * 0.65 + 0.35)) * suctionBoost;
      p.angle += swirlSpeed * dt;
      // Inward logarithmic drift
      p.rRatio -= p.inwardRate * suctionBoost * dt;

      // When reaching event horizon, respawn at outer accretion edge
      if (p.rRatio <= 0.98) {
        p.rRatio = 2.4 + Math.random() * 0.7;
        p.angle = Math.random() * Math.PI * 2;
      }
    }
  }

  addBoostShockwave(x, y, radiusStart = 180, color = "#38bdf8") {
    this.popRings.push({
      x,
      y,
      radius: radiusStart * 0.88,
      radiusMax: radiusStart + 90,
      color,
      alpha: 1.0,
      lineWidth: 7,
      speed: 260,
      fadeRate: 2.4,
    });
    this.popRings.push({
      x,
      y,
      radius: radiusStart * 0.72,
      radiusMax: radiusStart + 60,
      color: "#fde047",
      alpha: 0.9,
      lineWidth: 4,
      speed: 190,
      fadeRate: 2.8,
    });
  }

  addThrusterPuff(x, y, vx, vy, radius = 7, color = "#ffffff") {
    if (this.thrusterPuffs.length > 25) return;
    this.thrusterPuffs.push({
      x,
      y,
      vx,
      vy,
      radius,
      color,
      life: 0.28,
      maxLife: 0.28,
    });
  }

  addRimSpark(x, y, vx, vy, color = "#fde047") {
    if (this.particles.length > 100) return;
    this.particles.push({
      x,
      y,
      vx: vx + (Math.random() - 0.5) * 40,
      vy: vy + (Math.random() - 0.5) * 40,
      gravity: 40,
      rot: Math.random() * Math.PI,
      vRot: (Math.random() - 0.5) * 12,
      life: 0.32,
      maxLife: 0.32,
      w: Math.random() * 4 + 3,
      h: Math.random() * 4 + 3,
      color,
    });
  }

  updateThrusterPuffs(dt) {
    for (let i = this.thrusterPuffs.length - 1; i >= 0; i--) {
      const puff = this.thrusterPuffs[i];
      puff.life -= dt;
      if (puff.life <= 0) {
        this.thrusterPuffs.splice(i, 1);
        continue;
      }
      puff.x += puff.vx * dt;
      puff.y += puff.vy * dt;
      puff.radius += dt * 32;
    }
  }

  triggerNibblesChew() {
    this.chewTimer = 0.35;
    this.mouthOpenAmount = 1.0;
    this.bhWobble = 0.09; // Bouncy elastic scale pulse when snacking!
  }

  render(physics, economy, dt, progressInfo = null) {
    this.cosmicTime += dt;
    this.updateNibblesFace(physics, dt, progressInfo);
    this.updateSingularityParticles(physics, dt);
    this.updateThrusterPuffs(dt);

    const ctx = this.ctx;
    ctx.clearRect(0, 0, this.width, this.height);

    const equipped = economy.profile.equipped;
    const ringSkin =
      COSMETIC_CATALOG.rings.find((r) => r.id === equipped.ring) ||
      COSMETIC_CATALOG.rings[0];
    const singularitySkin =
      COSMETIC_CATALOG.singularities.find(
        (s) => s.id === equipped.singularity,
      ) || COSMETIC_CATALOG.singularities[0];
    const skyboxSkin =
      COSMETIC_CATALOG.skyboxes.find((k) => k.id === equipped.skybox) ||
      COSMETIC_CATALOG.skyboxes[0];

    // 1. Render Cartoon Skybox & Twinkling Smiling Stars
    this.renderCartoonSky(ctx, skyboxSkin);

    // 2. Render Nibbles the Cartoon Black Hole (at bottom so debris falls into it)
    this.renderNibblesBlackHole(ctx, physics, singularitySkin);

    // 3. Render Chunky Cartoon Centrifuge Ring & Candy Chutes
    this.renderCartoonRing(ctx, physics, ringSkin);

    // 3b. Render Mechanical One-Way Gravity Flapper Valves
    this.renderOneWayFlappers(ctx, physics);

    // 4. Render Bouncy Cartoon Pinball Bumpers
    this.renderCartoonBumpers(ctx, physics);

    // 5. Render Differentiated Comic Multiplier Gates
    this.renderCartoonGates(ctx, physics);

    // 5. Render Adorable Space Debris
    this.renderCartoonDebris(ctx, physics.debrisList);

    // 6. Render Cartoon Confetti & Comic Text Pop VFX
    this.renderVfx(ctx, dt);
  }

  updateNibblesFace(physics, dt, progressInfo) {
    // Dynamic Black Hole Growth: smoothly scale from 1.0x to 1.65x as quota is reached
    if (progressInfo && progressInfo.quota > 0) {
      const ratio = Math.min(
        1.0,
        Math.max(0, (progressInfo.collectedMass || 0) / progressInfo.quota),
      );
      this.targetBhScale = 1.0 + ratio * 0.65;
    } else {
      this.targetBhScale = 1.0;
    }

    if (this.bhWobble > 0.001) {
      this.bhWobble *= Math.pow(0.84, dt * 60);
    } else {
      this.bhWobble = 0;
    }

    const targetWithWobble = this.targetBhScale + this.bhWobble;
    this.currentBhScale +=
      (targetWithWobble - this.currentBhScale) * Math.min(1.0, dt * 6.0);

    // Blinking animation
    this.nibblesBlinkTimer += dt;
    if (this.nibblesBlinkTimer > 3.2) {
      this.isBlinking = true;
      if (this.nibblesBlinkTimer > 3.4) {
        this.isBlinking = false;
        this.nibblesBlinkTimer = 0;
      }
    }

    // Mouth animation: open mouth wide when debris is falling close!
    const bh = physics.blackHole;
    let closestDist = 9999;
    for (const d of physics.debrisList) {
      if (d.state === "FALLING" || d.state === "INGESTING") {
        const dist = Math.hypot(bh.x - d.x, bh.y - d.y);
        if (dist < closestDist) closestDist = dist;
      }
    }

    if (this.chewTimer > 0) {
      this.chewTimer -= dt;
      this.mouthOpenAmount = 0.5 + Math.sin(this.cosmicTime * 25) * 0.4;
    } else if (closestDist < 120) {
      // Anticipate yummy snack!
      this.mouthOpenAmount = Math.min(1.0, this.mouthOpenAmount + dt * 6.0);
    } else {
      this.mouthOpenAmount = Math.max(0, this.mouthOpenAmount - dt * 3.0);
    }
  }

  renderCartoonSky(ctx, skyboxSkin) {
    // Warm, saturated cartoon gradient
    const grad = ctx.createLinearGradient(0, 0, 0, this.height);
    grad.addColorStop(0, skyboxSkin.bgGradient[0] || "#1e1b4b");
    grad.addColorStop(0.5, skyboxSkin.bgGradient[1] || "#312e81");
    grad.addColorStop(1, skyboxSkin.bgGradient[2] || "#4338ca");

    ctx.fillStyle = grad;
    ctx.fillRect(0, 0, this.width, this.height);

    // Cute background cartoon planets
    for (const p of this.planets) {
      const px = p.x * this.width;
      const py = p.y * this.height;

      ctx.save();
      // Planet body
      ctx.fillStyle = p.color;
      ctx.beginPath();
      ctx.arc(px, py, p.radius, 0, Math.PI * 2);
      ctx.fill();

      // Cute white shine highlight
      ctx.fillStyle = "rgba(255, 255, 255, 0.4)";
      ctx.beginPath();
      ctx.arc(
        px - p.radius * 0.35,
        py - p.radius * 0.35,
        p.radius * 0.35,
        0,
        Math.PI * 2,
      );
      ctx.fill();

      // Optional Ring
      if (p.ring) {
        ctx.strokeStyle = p.ringColor;
        ctx.lineWidth = 4;
        ctx.beginPath();
        ctx.ellipse(
          px,
          py,
          p.radius * 1.7,
          p.radius * 0.45,
          -0.3,
          0,
          Math.PI * 2,
        );
        ctx.stroke();
      }
      ctx.restore();
    }

    // Twinkling 4-point cartoon stars
    for (const s of this.stars) {
      const sx = s.x * this.width;
      const sy = s.y * this.height;
      const pulse = 1 + Math.sin(this.cosmicTime * s.speed + s.x * 10) * 0.25;
      const size = s.size * pulse;

      ctx.save();
      ctx.fillStyle = s.color;
      ctx.translate(sx, sy);

      if (s.type === "star") {
        ctx.beginPath();
        ctx.moveTo(0, -size);
        ctx.quadraticCurveTo(0, 0, size, 0);
        ctx.quadraticCurveTo(0, 0, 0, size);
        ctx.quadraticCurveTo(0, 0, -size, 0);
        ctx.quadraticCurveTo(0, 0, 0, -size);
        ctx.fill();
      } else {
        ctx.beginPath();
        ctx.arc(0, 0, size * 0.5, 0, Math.PI * 2);
        ctx.fill();
      }
      ctx.restore();
    }
  }

  renderNibblesBlackHole(ctx, physics, skin) {
    const bh = physics.blackHole;
    const time = this.cosmicTime * 2.5;

    // Upward Gravitational Funnel & Cosmic Radiation Cone
    const funnelTopY = bh.y - 125;
    const funnelGrad = ctx.createLinearGradient(bh.x, bh.y, bh.x, funnelTopY);
    funnelGrad.addColorStop(0, (skin.haloColor || "#818cf8") + "44");
    funnelGrad.addColorStop(0.65, (skin.haloColor || "#818cf8") + "15");
    funnelGrad.addColorStop(1, "rgba(255, 255, 255, 0)");

    ctx.save();
    ctx.fillStyle = funnelGrad;
    ctx.beginPath();
    ctx.moveTo(bh.x - bh.eventHorizonRadius * 1.5, bh.y);
    ctx.lineTo(bh.x - bh.eventHorizonRadius * 2.6, funnelTopY);
    ctx.lineTo(bh.x + bh.eventHorizonRadius * 2.6, funnelTopY);
    ctx.lineTo(bh.x + bh.eventHorizonRadius * 1.5, bh.y);
    ctx.closePath();
    ctx.fill();

    // Animated downward gravity ripples towards Nibbles
    for (let b = 0; b < 3; b++) {
      const offset = ((this.cosmicTime * 0.9 + b * 0.33) % 1.0);
      const bY = funnelTopY + offset * 125;
      const bW = (bh.eventHorizonRadius * 2.6) - offset * (bh.eventHorizonRadius * 1.1);
      const bAlpha = Math.sin(offset * Math.PI) * 0.35;
      ctx.strokeStyle = `rgba(255, 255, 255, ${bAlpha})`;
      ctx.lineWidth = 2.2;
      ctx.setLineDash([8, 8]);
      ctx.beginPath();
      ctx.moveTo(bh.x - bW * 0.5, bY);
      ctx.lineTo(bh.x + bW * 0.5, bY);
      ctx.stroke();
    }
    ctx.restore();

    ctx.save();
    // Dynamic Growth Scaling: visually scales from 1.0x to 1.65x without altering level completion conditions
    ctx.translate(bh.x, bh.y);
    ctx.scale(this.currentBhScale, this.currentBhScale);
    ctx.translate(-bh.x, -bh.y);

    // 1. Swirling Comic Accretion Vortex & Multi-Layered Ribbons
    const rHorizon = bh.eventHorizonRadius;
    const haloRadius = rHorizon * 2.8 + Math.sin(time * 1.6) * 3;

    ctx.save();
    ctx.translate(bh.x, bh.y);

    // A. Soft Pulsing Outer Glow
    const haloGrad = ctx.createRadialGradient(0, 0, rHorizon * 0.7, 0, 0, haloRadius);
    haloGrad.addColorStop(0, (skin.haloColor || "#818cf8") + "66");
    haloGrad.addColorStop(0.45, (skin.haloColor || "#818cf8") + "28");
    haloGrad.addColorStop(1, "rgba(0, 0, 0, 0)");
    ctx.fillStyle = haloGrad;
    ctx.beginPath();
    ctx.arc(0, 0, haloRadius, 0, Math.PI * 2);
    ctx.fill();

    // B. Outer Accretion Ribbon (Dashed pastel track)
    ctx.save();
    ctx.rotate(time * 0.4);
    ctx.setLineDash([14, 16]);
    ctx.lineCap = "round";
    ctx.lineWidth = 4;
    ctx.strokeStyle = (skin.haloColor || "#818cf8") + "aa";
    ctx.beginPath();
    ctx.ellipse(0, 0, rHorizon * 2.35, rHorizon * 1.95, 0, 0, Math.PI * 2);
    ctx.stroke();
    ctx.restore();

    // C. Middle Fast Ribbon (Vibrant Sunny Yellow)
    ctx.save();
    ctx.rotate(-time * 0.85);
    ctx.setLineDash([10, 14]);
    ctx.lineCap = "round";
    ctx.lineWidth = 4.5;
    ctx.strokeStyle = "#fde047dd";
    ctx.beginPath();
    ctx.ellipse(0, 0, rHorizon * 1.75, rHorizon * 1.48, 0, 0, Math.PI * 2);
    ctx.stroke();
    ctx.restore();

    // D. Inner Relativistic Ring (Electric Cyan Core Accent)
    ctx.save();
    ctx.rotate(time * 1.8);
    ctx.lineWidth = 3.2;
    ctx.strokeStyle = "#38bdf8ee";
    ctx.setLineDash([6, 8]);
    ctx.lineCap = "round";
    ctx.beginPath();
    ctx.arc(0, 0, rHorizon + 6.5, 0, Math.PI * 2);
    ctx.stroke();
    ctx.restore();

    // E. Spiraling Inward Cosmic Stardust & Candies
    for (const p of this.singularityParticles) {
      const pr = rHorizon * p.rRatio;
      const px = Math.cos(p.angle) * pr;
      const py = Math.sin(p.angle) * (pr * 0.82); // elliptical perspective

      const horizonDist = p.rRatio - 1.0;
      if (horizonDist < 0) continue;
      const alpha = Math.min(1.0, horizonDist * 2.2) * (0.55 + Math.sin(p.twinkle) * 0.45);

      ctx.save();
      ctx.globalAlpha = Math.max(0, Math.min(1, alpha));
      ctx.fillStyle = p.color;
      ctx.translate(px, py);

      // Star or Sparkle
      ctx.beginPath();
      ctx.arc(0, 0, p.size, 0, Math.PI * 2);
      ctx.fill();

      // White hot center
      ctx.fillStyle = "#ffffff";
      ctx.beginPath();
      ctx.arc(0, 0, p.size * 0.45, 0, Math.PI * 2);
      ctx.fill();

      // Tangent streak
      const tanAngle = p.angle + Math.PI * 0.5;
      ctx.strokeStyle = p.color;
      ctx.lineWidth = 1.3;
      ctx.beginPath();
      ctx.moveTo(0, 0);
      ctx.lineTo(-Math.cos(tanAngle) * (p.size * 2.2), -Math.sin(tanAngle) * (p.size * 2.2));
      ctx.stroke();

      ctx.restore();
    }
    ctx.restore();

    // 2. Nibbles' Round Chubby Body
    ctx.save();
    ctx.translate(bh.x, bh.y);

    // Thick cartoon border
    ctx.lineWidth = 4.5;
    ctx.strokeStyle = "#1e1b4b";
    ctx.fillStyle = skin.coreColor || "#0f172a";
    ctx.beginPath();
    ctx.arc(0, 0, bh.eventHorizonRadius + 3, 0, Math.PI * 2);
    ctx.fill();
    ctx.stroke();

    // Cute blush cheeks
    ctx.fillStyle = "rgba(244, 114, 182, 0.6)";
    ctx.beginPath();
    ctx.arc(
      -bh.eventHorizonRadius * 0.58,
      bh.eventHorizonRadius * 0.15,
      6,
      0,
      Math.PI * 2,
    );
    ctx.arc(
      bh.eventHorizonRadius * 0.58,
      bh.eventHorizonRadius * 0.15,
      6,
      0,
      Math.PI * 2,
    );
    ctx.fill();

    // 3. Animated Cartoon Eyes
    const eyeOffsetX = bh.eventHorizonRadius * 0.38;
    const eyeOffsetY = -bh.eventHorizonRadius * 0.22;
    const eyeRadius = 7.5;

    // Find nearest falling debris to look at it
    let lookDx = 0;
    let lookDy = -1;
    let minD = 9999;
    for (const d of physics.debrisList) {
      const dist = Math.hypot(d.x - bh.x, d.y - bh.y);
      if (dist < minD) {
        minD = dist;
        lookDx = (d.x - bh.x) / (dist + 0.1);
        lookDy = (d.y - bh.y) / (dist + 0.1);
      }
    }

    [-eyeOffsetX, eyeOffsetX].forEach((ex) => {
      ctx.save();
      ctx.translate(ex, eyeOffsetY);

      if (this.isBlinking) {
        // Happy squint curve when blinking
        ctx.strokeStyle = "#ffffff";
        ctx.lineWidth = 2.5;
        ctx.beginPath();
        ctx.arc(0, 0, eyeRadius * 0.8, 0, Math.PI);
        ctx.stroke();
      } else {
        // Big white eyeball with thick border
        ctx.fillStyle = "#ffffff";
        ctx.strokeStyle = "#1e1b4b";
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.arc(0, 0, eyeRadius, 0, Math.PI * 2);
        ctx.fill();
        ctx.stroke();

        // Big Pupil tracking debris
        const pX = lookDx * 2.5;
        const pY = lookDy * 2.5;
        ctx.fillStyle = "#1e1b4b";
        ctx.beginPath();
        ctx.arc(pX, pY, eyeRadius * 0.58, 0, Math.PI * 2);
        ctx.fill();

        // Anime white shine highlight
        ctx.fillStyle = "#ffffff";
        ctx.beginPath();
        ctx.arc(pX - 1.5, pY - 1.8, 2.0, 0, Math.PI * 2);
        ctx.arc(pX + 1.5, pY + 1.2, 1.0, 0, Math.PI * 2);
        ctx.fill();
      }
      ctx.restore();
    });

    // 4. Cute Cartoon Mouth (NOM NOM GULP!)
    ctx.save();
    ctx.translate(0, bh.eventHorizonRadius * 0.28);
    if (this.mouthOpenAmount > 0.2) {
      // Big hungry open mouth!
      const openH = 6 + this.mouthOpenAmount * 11;
      const openW = 8 + this.mouthOpenAmount * 6;
      ctx.fillStyle = "#e11d48";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 2.2;
      ctx.beginPath();
      ctx.ellipse(0, 0, openW, openH, 0, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();

      // Cute tongue
      ctx.fillStyle = "#fb7185";
      ctx.beginPath();
      ctx.arc(0, openH * 0.45, openW * 0.55, 0, Math.PI);
      ctx.fill();
    } else {
      // Happy cat smile :3
      ctx.strokeStyle = "#ffffff";
      ctx.lineWidth = 2.2;
      ctx.beginPath();
      ctx.arc(-3.5, 0, 3.5, 0, Math.PI);
      ctx.arc(3.5, 0, 3.5, 0, Math.PI);
      ctx.stroke();
    }
    ctx.restore();

    ctx.restore();
    ctx.restore();
  }

  renderOneWayFlappers(ctx, physics) {
    if (!physics.oneWayFlaps || !physics.oneWayFlaps.length) return;

    const cx = physics.center.x;
    const cy = physics.center.y;
    const angle = physics.ringAngle;

    ctx.save();
    ctx.translate(cx, cy);
    ctx.rotate(angle);

    for (const flap of physics.oneWayFlaps) {
      const rot = (flap.isInner && physics.concentricRings)
        ? (physics.innerRingAngle - physics.ringAngle)
        : 0;
      const baseAngle = flap.angle + rot;
      const currentAngle = baseAngle + flap.swing * (flap.normalSign || 1);
      const isLocked = flap.swing <= 0.05;

      const rMin = flap.rMin;
      const rMax = flap.rMax;

      const cosCur = Math.cos(currentAngle);
      const sinCur = Math.sin(currentAngle);

      // 1. Rigid Stopper Peg
      const stopAng = baseAngle - 0.08 * (flap.normalSign || 1);
      const stopX = Math.cos(stopAng) * (rMin + 14);
      const stopY = Math.sin(stopAng) * (rMin + 14);
      ctx.fillStyle = "#64748b";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(stopX, stopY, 4, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();

      // 2. Swinging Flapper Arm
      const x1 = cosCur * rMin;
      const y1 = sinCur * rMin;
      const x2 = cosCur * rMax;
      const y2 = sinCur * rMax;

      ctx.lineWidth = 8;
      ctx.lineCap = "round";
      ctx.strokeStyle = "#1e1b4b";
      ctx.beginPath();
      ctx.moveTo(x1, y1);
      ctx.lineTo(x2, y2);
      ctx.stroke();

      ctx.lineWidth = 4;
      ctx.strokeStyle = isLocked ? "#f59e0b" : "#22c55e"; // Amber when locked, emerald green when open
      ctx.beginPath();
      ctx.moveTo(x1, y1);
      ctx.lineTo(x2, y2);
      ctx.stroke();

      // 3. Heavy Industrial Hinge Bolt
      ctx.fillStyle = "#e2e8f0";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 2.5;
      ctx.beginPath();
      ctx.arc(x1, y1, 7, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();

      // Status LED indicator
      ctx.fillStyle = isLocked ? "#f59e0b" : "#4ade80";
      ctx.beginPath();
      ctx.arc(x1, y1, 2.8, 0, Math.PI * 2);
      ctx.fill();

      // Flap tip bumper roller
      ctx.fillStyle = "#38bdf8";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 1.5;
      ctx.beginPath();
      ctx.arc(x2, y2, 4, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();
    }

    ctx.restore();
  }

  renderCartoonBumpers(ctx, physics) {
    if (!physics.deflectorPins || physics.deflectorPins.length === 0) return;

    const cx = physics.center.x;
    const cy = physics.center.y;
    const angle = physics.ringAngle;

    ctx.save();
    ctx.translate(cx, cy);
    ctx.rotate(angle);

    for (const pin of physics.deflectorPins) {
      const rot = (pin.isInner && physics.concentricRings)
        ? (physics.innerRingAngle - physics.ringAngle)
        : 0;
      const finalAngle = pin.angle + rot;
      const px = Math.cos(finalAngle) * pin.r;
      const py = Math.sin(finalAngle) * pin.r;
      const r = pin.radius || 9.5;
      const isHit = pin.hitTimer > 0;
      const hitProgress = isHit ? pin.hitTimer / 0.25 : 0; // 1 down to 0
      const scale = isHit ? 1.0 + Math.sin(hitProgress * Math.PI) * 0.35 : 1.0;

      ctx.save();
      ctx.translate(px, py);
      ctx.scale(scale, scale);

      // 1. Bumper Drop Shadow
      ctx.fillStyle = "rgba(15, 23, 42, 0.4)";
      ctx.beginPath();
      ctx.arc(0, 3, r + 2, 0, Math.PI * 2);
      ctx.fill();

      // 2. Heavy Chrome Outer Trim Ring (Pinball arcade aesthetic)
      ctx.lineWidth = 3;
      ctx.strokeStyle = "#1e1b4b";
      ctx.fillStyle = isHit ? "#fef08a" : pin.color || "#ec4899";
      ctx.beginPath();
      ctx.arc(0, 0, r + 1, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();

      // 3. Inner Illuminated Dome Gradient
      const domeGrad = ctx.createRadialGradient(-r * 0.3, -r * 0.3, 1, 0, 0, r);
      domeGrad.addColorStop(0, "#ffffff");
      domeGrad.addColorStop(0.45, isHit ? "#fbbf24" : pin.accent || "#f43f5e");
      domeGrad.addColorStop(1, isHit ? "#f59e0b" : pin.baseColor || "#be123c");

      ctx.fillStyle = domeGrad;
      ctx.beginPath();
      ctx.arc(0, 0, r * 0.8, 0, Math.PI * 2);
      ctx.fill();

      // 4. Shiny Dome Highlight
      ctx.fillStyle = "rgba(255, 255, 255, 0.85)";
      ctx.beginPath();
      ctx.arc(-r * 0.28, -r * 0.28, r * 0.26, 0, Math.PI * 2);
      ctx.fill();

      // 5. Animated Shockwave Pulse on Hit
      if (isHit) {
        const shockRadius = r + (1 - hitProgress) * 16;
        ctx.lineWidth = 2.5 * hitProgress;
        ctx.strokeStyle = `rgba(254, 240, 138, ${hitProgress})`;
        ctx.beginPath();
        ctx.arc(0, 0, shockRadius, 0, Math.PI * 2);
        ctx.stroke();
      }

      ctx.restore();
    }

    ctx.restore();
  }

  renderCartoonRing(ctx, physics, ringSkin) {
    const cx = physics.center.x;
    const cy = physics.center.y;
    const rOut = physics.ringRadius;
    const rIn = physics.innerRadius;
    const angle = physics.ringAngle;
    const apAngle = physics.apertureAngle;

    ctx.save();
    ctx.translate(cx, cy);

    // 1. Dynamic Speed Arcs, RCS Jets & Boost VFX
    this.renderRotationBoostVFX(ctx, physics, rOut, rIn, angle, ringSkin);

    ctx.rotate(angle);

    // 2. Chunky Outer Ring (Thick Cartoon Border)
    const apStart = Math.PI * 0.5 + apAngle * 0.5;
    const apEnd = Math.PI * 0.5 - apAngle * 0.5;

    // Dark bottom drop shadow
    ctx.lineWidth = 18;
    ctx.strokeStyle = "rgba(15, 23, 42, 0.45)";
    ctx.beginPath();
    ctx.arc(0, 4, rOut, apStart, apEnd);
    ctx.stroke();

    // Main colorful cartoon outer rim
    ctx.lineWidth = 14;
    ctx.strokeStyle = ringSkin.color;
    if (physics.rcsThrusting) {
      ctx.shadowBlur = 18;
      ctx.shadowColor = "#38bdf8";
    }
    ctx.lineCap = "round";
    ctx.beginPath();
    ctx.arc(0, 0, rOut, apStart, apEnd);
    ctx.stroke();
    ctx.shadowBlur = 0;

    // Dark cartoon border outlines
    ctx.lineWidth = 3;
    ctx.strokeStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.arc(0, 0, rOut + 7, apStart, apEnd);
    ctx.stroke();
    ctx.beginPath();
    ctx.arc(0, 0, rOut - 7, apStart, apEnd);
    ctx.stroke();

    // Outer rim glossy top highlight
    ctx.lineWidth = 3;
    ctx.strokeStyle = "rgba(255, 255, 255, 0.65)";
    ctx.beginPath();
    ctx.arc(0, 0, rOut - 4, Math.PI * 1.1, Math.PI * 1.9);
    ctx.stroke();

    // Decorative candy sprinkles / bolts on outer ring
    for (let i = 0; i < 10; i++) {
      const a = (Math.PI * 2 * i) / 10;
      if (a > apEnd && a < apStart) continue;
      const bx = Math.cos(a) * (rOut - 1);
      const by = Math.sin(a) * (rOut - 1);
      ctx.fillStyle = ringSkin.accent;
      ctx.beginPath();
      ctx.arc(bx, by, 3.5, 0, Math.PI * 2);
      ctx.fill();
    }

    // 3. Aperture Cartoon Chevron Arrows (Pointing out to Nibbles)
    this.renderCartoonChevrons(ctx, rOut, apEnd, apStart);

    // 4. Inner Hub (Rotating in sync with planetary gears if concentric)
    ctx.save();
    if (physics.concentricRings) {
      ctx.rotate(physics.innerRingAngle - physics.ringAngle);
    }
    ctx.lineWidth = 10;
    ctx.strokeStyle = "#1e1b4b";
    ctx.fillStyle = ringSkin.color;
    ctx.beginPath();
    ctx.arc(0, 0, rIn, 0, Math.PI * 2);
    ctx.fill();
    ctx.stroke();

    // Inner hub face
    ctx.fillStyle = "#ffffff";
    ctx.beginPath();
    ctx.arc(0, 0, rIn - 8, 0, Math.PI * 2);
    ctx.fill();

    // Station face text
    ctx.fillStyle = "#1e1b4b";
    ctx.font = "bold 13px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";

    let hubEmoji = "✨ ORBITAL ✨";
    let hubSubtitle = "SORT RIG";
    if (ringSkin.id === "ring_duckie") {
      hubEmoji = "🦆 DUCKIE 🦆";
      hubSubtitle = "FLOAT RIG";
    } else if (ringSkin.id === "ring_candyland") {
      hubEmoji = "🍬 CANDY 🍬";
      hubSubtitle = "MINT RIG";
    } else if (ringSkin.id === "ring_spacecat") {
      hubEmoji = "🐱 KITTY 🐱";
      hubSubtitle = "COSMIC RIG";
    } else if (ringSkin.id === "ring_donut") {
      hubEmoji = "🍩 DONUT 🍩";
      hubSubtitle = "SWEET RIG";
    }
    if (physics.concentricRings) {
      hubEmoji = "⚙️ GEAR RIG ⚙️";
      hubSubtitle = "PLANETARY";
    }

    ctx.fillText(hubEmoji, 0, -7);
    ctx.font = "11px Nunito, sans-serif";
    ctx.fillStyle = "#64748b";
    ctx.fillText(hubSubtitle, 0, 9);
    ctx.restore();

    // 4b. Concentric Track Partitions & Planetary Gear Teeth
    if (physics.trackWalls && physics.trackWalls.length > 0) {
      for (const tw of physics.trackWalls) {
        const gapHalf = (tw.gapSize || 0.45) * 0.5;
        const gapAng = tw.gapAngle;
        const startA = gapAng + gapHalf;
        const endA = gapAng - gapHalf + Math.PI * 2;

        // Dark track outline
        ctx.lineWidth = 8;
        ctx.strokeStyle = "#1e1b4b";
        ctx.beginPath();
        ctx.arc(0, 0, tw.r, startA, endA);
        ctx.stroke();

        // Bright candy rail
        ctx.lineWidth = 4;
        ctx.strokeStyle = ringSkin.accent;
        ctx.beginPath();
        ctx.arc(0, 0, tw.r, startA, endA);
        ctx.stroke();

        // Animated Planetary Gear Teeth along track wall (if concentric)
        if (physics.concentricRings) {
          const numCogs = 24;
          const cogStep = (Math.PI * 2) / numCogs;
          const cogRot = (physics.innerRingAngle - physics.ringAngle) * 0.6;
          ctx.fillStyle = "#facc15";
          ctx.strokeStyle = "#1e1b4b";
          ctx.lineWidth = 1.5;

          for (let i = 0; i < numCogs; i++) {
            const cogAng = i * cogStep + cogRot;
            let diff = cogAng - gapAng;
            while (diff > Math.PI) diff -= Math.PI * 2;
            while (diff < -Math.PI) diff += Math.PI * 2;
            if (Math.abs(diff) < gapHalf + 0.1) continue;

            const cx1 = Math.cos(cogAng) * (tw.r - 4);
            const cy1 = Math.sin(cogAng) * (tw.r - 4);
            ctx.beginPath();
            ctx.arc(cx1, cy1, 2.5, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
          }
        }

        // Cartoon gateway indicators at transfer gap
        [-gapHalf, gapHalf].forEach((off) => {
          const capAng = gapAng + off;
          const capX = Math.cos(capAng) * tw.r;
          const capY = Math.sin(capAng) * tw.r;
          ctx.fillStyle = "#facc15";
          ctx.strokeStyle = "#1e1b4b";
          ctx.lineWidth = 2;
          ctx.beginPath();
          ctx.arc(capX, capY, 4.5, 0, Math.PI * 2);
          ctx.fill();
          ctx.stroke();
        });
      }
    }

    // 5. Chunky Radial Dividers (Candy Chutes)
    ctx.lineWidth = 7;
    ctx.lineCap = "round";
    for (const div of physics.dividers) {
      const rot = (div.isInner && physics.concentricRings)
        ? (physics.innerRingAngle - physics.ringAngle)
        : 0;
      const finalAngle = div.angle + rot;

      const dx1 = Math.cos(finalAngle) * div.rMin;
      const dy1 = Math.sin(finalAngle) * div.rMin;
      const dx2 = Math.cos(finalAngle) * div.rMax;
      const dy2 = Math.sin(finalAngle) * div.rMax;

      // Dark cartoon outline
      ctx.strokeStyle = "#1e1b4b";
      ctx.beginPath();
      ctx.moveTo(dx1, dy1);
      ctx.lineTo(dx2, dy2);
      ctx.stroke();

      // Bright candy center
      ctx.lineWidth = 3.5;
      ctx.strokeStyle = ringSkin.accent;
      ctx.beginPath();
      ctx.moveTo(dx1, dy1);
      ctx.lineTo(dx2, dy2);
      ctx.stroke();
      ctx.lineWidth = 7;
    }

    ctx.restore();
  }

  renderRotationBoostVFX(ctx, physics, rOut, rIn, angle, ringSkin) {
    const absSpeed = Math.abs(physics.angularVelocity);
    const isBoosting = physics.rcsThrusting;
    const hasMotion = absSpeed > 0.15 || isBoosting;

    if (!hasMotion) return;

    const dir = Math.sign(physics.angularVelocity || physics.rcsDirection || 1);
    const speedRatio = Math.min(1.0, absSpeed / 5.0);
    const intensity = isBoosting ? Math.max(0.9, speedRatio) : speedRatio;

    ctx.save();

    // 1. Dynamic Curved Speed Streaks (Whoosh Ribbons) along outer & inner rims
    const streakTracks = [
      { r: rOut + 14, color: "#38bdf8", width: 5.5, arc: 0.55 + intensity * 0.65, count: 3 },
      { r: rOut + 8, color: "#fde047", width: 4.0, arc: 0.45 + intensity * 0.55, count: 4 },
      { r: rIn - 8, color: "#f472b6", width: 3.5, arc: 0.40 + intensity * 0.50, count: 3 },
    ];

    for (const track of streakTracks) {
      for (let i = 0; i < track.count; i++) {
        // Distribute arcs around the perimeter
        const baseOffset = (Math.PI * 2 * i) / track.count + (angle * 0.35);
        const headAngle = baseOffset;
        const tailAngle = headAngle - track.arc * dir;

        ctx.save();
        ctx.strokeStyle = track.color;
        ctx.globalAlpha = 0.35 + intensity * 0.55;
        ctx.lineWidth = track.width * (0.7 + intensity * 0.5);
        ctx.lineCap = "round";

        ctx.beginPath();
        if (dir > 0) {
          ctx.arc(0, 0, track.r, tailAngle, headAngle);
        } else {
          ctx.arc(0, 0, track.r, headAngle, tailAngle);
        }
        ctx.stroke();

        // Inner white hot core line
        ctx.strokeStyle = "#ffffff";
        ctx.lineWidth = track.width * 0.38;
        ctx.globalAlpha = 0.65 + intensity * 0.35;
        ctx.beginPath();
        if (dir > 0) {
          ctx.arc(0, 0, track.r, tailAngle + track.arc * 0.35, headAngle);
        } else {
          ctx.arc(0, 0, track.r, headAngle, tailAngle - track.arc * 0.35);
        }
        ctx.stroke();

        // Comic trailing spark dots at tail
        for (let d = 1; d <= 2; d++) {
          const dotAngle = dir > 0 ? tailAngle - d * 0.08 : tailAngle + d * 0.08;
          ctx.fillStyle = track.color;
          ctx.globalAlpha = 0.5 - d * 0.15;
          ctx.beginPath();
          ctx.arc(Math.cos(dotAngle) * track.r, Math.sin(dotAngle) * track.r, track.width * 0.45, 0, Math.PI * 2);
          ctx.fill();
        }

        ctx.restore();
      }
    }

    // 2. Centrifugal Rim Sparks (flying off tangentially at high speed)
    if (absSpeed > 2.0 || isBoosting) {
      this.sparkEmitTimer = (this.sparkEmitTimer || 0) + 1;
      if (this.sparkEmitTimer % 2 === 0) {
        const emitAngle = Math.random() * Math.PI * 2;
        const ex = Math.cos(emitAngle) * (rOut + 4);
        const ey = Math.sin(emitAngle) * (rOut + 4);
        const tanAngle = emitAngle + Math.PI * 0.5 * dir;
        const sparkSpeed = (120 + Math.random() * 80) * intensity;
        const vx = Math.cos(tanAngle) * sparkSpeed;
        const vy = Math.sin(tanAngle) * sparkSpeed;
        const colors = ["#fde047", "#38bdf8", "#ffffff", "#f472b6"];
        this.addRimSpark(
          physics.center.x + ex,
          physics.center.y + ey,
          vx,
          vy,
          colors[Math.floor(Math.random() * colors.length)],
        );
      }
    }

    // 3. Comic RCS Thruster Jets on outer flanks
    // Left thruster at (-rOut, 0), Right thruster at (rOut, 0)
    const thrusters = [
      { x: -rOut, y: 0, plumeDirY: dir > 0 ? -1 : 1 },
      { x: rOut, y: 0, plumeDirY: dir > 0 ? 1 : -1 },
    ];

    for (const th of thrusters) {
      const flameLength = (16 + intensity * 28) * (isBoosting ? 1.5 : 1.0);
      const flameWidth = (8 + intensity * 8) * (isBoosting ? 1.4 : 1.0);

      ctx.save();
      ctx.translate(th.x, th.y);

      // Nozzle mount bracket
      ctx.fillStyle = "#1e1b4b";
      ctx.strokeStyle = "#38bdf8";
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.roundRect(-5, -6, 10, 12, 3);
      ctx.fill();
      ctx.stroke();

      // Outer Flame (Neon Cyan or Orange)
      ctx.fillStyle = isBoosting ? "#f59e0b" : "#38bdf8";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 1.8;
      ctx.beginPath();
      ctx.moveTo(-flameWidth * 0.5, 0);
      ctx.lineTo(flameWidth * 0.5, 0);
      ctx.lineTo(0, th.plumeDirY * flameLength);
      ctx.closePath();
      ctx.fill();
      ctx.stroke();

      // Inner Core Flame (Bright Yellow / White)
      ctx.fillStyle = "#ffffff";
      ctx.beginPath();
      ctx.moveTo(-flameWidth * 0.25, 0);
      ctx.lineTo(flameWidth * 0.25, 0);
      ctx.lineTo(0, th.plumeDirY * flameLength * 0.65);
      ctx.closePath();
      ctx.fill();

      // Puff smoke bubbles during boost
      if (isBoosting && Math.random() > 0.4) {
        this.addThrusterPuff(
          physics.center.x + th.x,
          physics.center.y + th.y + th.plumeDirY * (flameLength * 0.8),
          (Math.random() - 0.5) * 20,
          th.plumeDirY * 50,
          5 + Math.random() * 4,
          isBoosting ? "#fef08a" : "#e0f2fe",
        );
      }

      ctx.restore();
    }

    ctx.restore();
  }

  renderCartoonChevrons(ctx, rOut, apEnd, apStart) {
    ctx.save();
    [-0.07, 0.07].forEach((offset) => {
      const ang = Math.PI * 0.5 + offset;
      const x = Math.cos(ang) * (rOut + 4);
      const y = Math.sin(ang) * (rOut + 4);
      ctx.fillStyle = "#facc15";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(x, y, 4.5, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();
    });
    ctx.restore();
  }

  renderSpinningGear(ctx, x, y, radius, rot, color) {
    ctx.save();
    ctx.translate(x, y);
    ctx.rotate(rot);

    const numTeeth = 6;
    ctx.fillStyle = color;
    ctx.strokeStyle = "#1e1b4b";
    ctx.lineWidth = 1.5;

    // Gear teeth
    for (let i = 0; i < numTeeth; i++) {
      const a = (i * Math.PI * 2) / numTeeth;
      ctx.save();
      ctx.rotate(a);
      ctx.beginPath();
      ctx.rect(-radius * 0.35, -radius * 1.3, radius * 0.7, radius * 0.55);
      ctx.fill();
      ctx.stroke();
      ctx.restore();
    }

    // Gear hub
    ctx.beginPath();
    ctx.arc(0, 0, radius, 0, Math.PI * 2);
    ctx.fill();
    ctx.stroke();

    // Rivet center
    ctx.fillStyle = "#ffffff";
    ctx.beginPath();
    ctx.arc(0, 0, radius * 0.38, 0, Math.PI * 2);
    ctx.fill();
    ctx.strokeStyle = "#1e1b4b";
    ctx.lineWidth = 1.2;
    ctx.stroke();

    ctx.restore();
  }

  renderSawbladeGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate) {
    const mainColor = "#f97316";

    // Track outline
    ctx.lineWidth = 6;
    ctx.strokeStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Hazard orange beam
    ctx.lineWidth = 3.5;
    ctx.strokeStyle = mainColor;
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Warm inner core
    ctx.lineWidth = 1.4;
    ctx.strokeStyle = "#fef08a";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Dual spinning cartoon sawblade gears at aperture anchors
    const spin = this.cosmicTime * 7;
    this.renderSpinningGear(ctx, x1, y1, 5.5, spin, mainColor);
    this.renderSpinningGear(ctx, x2, y2, 5.5, -spin, mainColor);

    // Compact Outer-Anchored Hexagonal Badge
    ctx.save();
    ctx.translate(gx, gy);
    ctx.rotate(gAngle + Math.PI * 0.5);

    const bw = 17;
    const bh = 8.5;

    // Badge shadow
    ctx.fillStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.moveTo(-bw - 4, 2);
    ctx.lineTo(-bw, -bh + 2);
    ctx.lineTo(bw, -bh + 2);
    ctx.lineTo(bw + 4, 2);
    ctx.lineTo(bw, bh + 2);
    ctx.lineTo(-bw, bh + 2);
    ctx.closePath();
    ctx.fill();

    // Hex body
    ctx.fillStyle = "#ffffff";
    ctx.strokeStyle = mainColor;
    ctx.lineWidth = 2.2;
    ctx.beginPath();
    ctx.moveTo(-bw - 4, 0);
    ctx.lineTo(-bw, -bh);
    ctx.lineTo(bw, -bh);
    ctx.lineTo(bw + 4, 0);
    ctx.lineTo(bw, bh);
    ctx.lineTo(-bw, bh);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    // Text: ⚡ ×N
    ctx.fillStyle = "#c2410c";
    ctx.font = "bold 10px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(`⚡ ×${gate.value}`, 0, 0.5);

    ctx.restore();
  }

  renderSuperShredderGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate) {
    const mainColor = "#ec4899";

    // Wide electric glow
    ctx.lineWidth = 8;
    ctx.strokeStyle = "rgba(236, 72, 153, 0.4)";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Jagged lightning crackle path
    const dx = x2 - x1;
    const dy = y2 - y1;
    const segCount = 6;
    ctx.lineWidth = 2.5;
    ctx.strokeStyle = "#ffffff";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    for (let i = 1; i < segCount; i++) {
      const frac = i / segCount;
      const px = x1 + dx * frac;
      const py = y1 + dy * frac;
      const perpX = -dy / (Math.hypot(dx, dy) + 0.001);
      const perpY = dx / (Math.hypot(dx, dy) + 0.001);
      const jitter = Math.sin(this.cosmicTime * 32 + i * 2) * 3;
      ctx.lineTo(px + perpX * jitter, py + perpY * jitter);
    }
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Spiked Diamond Emitters at x1, y1 and x2, y2
    [
      { x: x1, y: y1 },
      { x: x2, y: y2 },
    ].forEach((p) => {
      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.rotate(this.cosmicTime * 5);

      ctx.fillStyle = mainColor;
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 1.6;
      ctx.beginPath();
      ctx.moveTo(0, -7.5);
      ctx.lineTo(2.5, -2.5);
      ctx.lineTo(7.5, 0);
      ctx.lineTo(2.5, 2.5);
      ctx.lineTo(0, 7.5);
      ctx.lineTo(-2.5, 2.5);
      ctx.lineTo(-7.5, 0);
      ctx.lineTo(-2.5, -2.5);
      ctx.closePath();
      ctx.fill();
      ctx.stroke();

      ctx.fillStyle = "#ffffff";
      ctx.beginPath();
      ctx.arc(0, 0, 2.2, 0, Math.PI * 2);
      ctx.fill();

      ctx.restore();
    });

    // Spiked Super Badge
    ctx.save();
    ctx.translate(gx, gy);
    ctx.rotate(gAngle + Math.PI * 0.5);

    const bw = 19;
    const bh = 9;

    // Glowing aura
    ctx.fillStyle = "rgba(236, 72, 153, 0.4)";
    ctx.beginPath();
    ctx.arc(0, 0, 16, 0, Math.PI * 2);
    ctx.fill();

    // Drop shadow
    ctx.fillStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.roundRect(-bw, -bh + 2, bw * 2, bh * 2, 5);
    ctx.fill();

    // Magenta badge body
    ctx.fillStyle = "#ec4899";
    ctx.strokeStyle = "#ffffff";
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.roundRect(-bw, -bh, bw * 2, bh * 2, 5);
    ctx.fill();
    ctx.stroke();

    // Text: 💥 ×N!
    ctx.fillStyle = "#ffffff";
    ctx.font = "900 10.5px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(`💥 ×${gate.value}!`, 0, 0.5);

    ctx.restore();
  }

  renderCompactorGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate) {
    const perpAngle = gAngle + Math.PI * 0.5;
    const offX = Math.cos(perpAngle) * 2.5;
    const offY = Math.sin(perpAngle) * 2.5;

    // Dual cyan magnetic guide rails
    ctx.lineWidth = 3;
    ctx.strokeStyle = "#0891b2";
    ctx.beginPath();
    ctx.moveTo(x1 - offX, y1 - offY);
    ctx.lineTo(x2 - offX, y2 - offY);
    ctx.moveTo(x1 + offX, y1 + offY);
    ctx.lineTo(x2 + offX, y2 + offY);
    ctx.stroke();

    // Pulsing inward magnetic field wave rings
    const dx = x2 - x1;
    const dy = y2 - y1;
    const waveCount = 3;
    for (let i = 0; i < waveCount; i++) {
      const wavePhase = (this.cosmicTime * 1.8 + i / waveCount) % 1.0;
      const frac = wavePhase < 0.5 ? wavePhase : 1.0 - (wavePhase - 0.5);
      const wx = x1 + dx * frac;
      const wy = y1 + dy * frac;

      ctx.strokeStyle = `rgba(6, 182, 212, ${0.8 - frac * 0.6})`;
      ctx.lineWidth = 1.8;
      ctx.beginPath();
      ctx.arc(wx, wy, 3.5 + wavePhase * 5, 0, Math.PI * 2);
      ctx.stroke();
    }

    // Heavy Magnetic Bracket Coils at ends
    [
      { x: x1, y: y1 },
      { x: x2, y: y2 },
    ].forEach((p) => {
      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.fillStyle = "#0284c7";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 1.8;
      ctx.beginPath();
      ctx.roundRect(-4.5, -4.5, 9, 9, 2.5);
      ctx.fill();
      ctx.stroke();

      ctx.fillStyle = "#67e8f9";
      ctx.beginPath();
      ctx.arc(0, 0, 2.2, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    });

    // Smooth Rounded Pill Badge (🧲 +N)
    ctx.save();
    ctx.translate(gx, gy);
    ctx.rotate(gAngle + Math.PI * 0.5);

    const bw = 17;
    const bh = 8.5;

    // Drop shadow
    ctx.fillStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.roundRect(-bw, -bh + 2, bw * 2, bh * 2, 8.5);
    ctx.fill();

    // Capsule body
    ctx.fillStyle = "#ffffff";
    ctx.strokeStyle = "#0891b2";
    ctx.lineWidth = 2.2;
    ctx.beginPath();
    ctx.roundRect(-bw, -bh, bw * 2, bh * 2, 8.5);
    ctx.fill();
    ctx.stroke();

    // Text: 🧲 +N
    ctx.fillStyle = "#0369a1";
    ctx.font = "bold 10px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(`🧲 +${gate.value}`, 0, 0.5);

    ctx.restore();
  }

  renderBoosterGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate) {
    // Glowing lime green accelerator track
    ctx.lineWidth = 6;
    ctx.strokeStyle = "rgba(132, 204, 22, 0.35)";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    ctx.lineWidth = 3;
    ctx.strokeStyle = "#65a30d";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Swept-wing aerodynamic pylons at ends
    [
      { x: x1, y: y1 },
      { x: x2, y: y2 },
    ].forEach((p) => {
      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.fillStyle = "#84cc16";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 1.8;
      ctx.beginPath();
      ctx.arc(0, 0, 4.5, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();

      ctx.fillStyle = "#facc15";
      ctx.beginPath();
      ctx.arc(0, 0, 2.2, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    });

    // Skewed Aerodynamic Badge (⏩ BOOST)
    ctx.save();
    ctx.translate(gx, gy);
    ctx.rotate(gAngle + Math.PI * 0.5);

    ctx.transform(1, 0, -0.22, 1, 0, 0);

    const bw = 18;
    const bh = 8.5;

    // Shadow
    ctx.fillStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.roundRect(-bw, -bh + 2, bw * 2, bh * 2, 4.5);
    ctx.fill();

    // Body
    ctx.fillStyle = "#84cc16";
    ctx.strokeStyle = "#1e1b4b";
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.roundRect(-bw, -bh, bw * 2, bh * 2, 4.5);
    ctx.fill();
    ctx.stroke();

    // Text: ⏩ BOOST
    ctx.fillStyle = "#ffffff";
    ctx.font = "bold 9.5px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText("⏩ BOOST", 0, 0.5);

    ctx.restore();
  }

  renderCartoonGates(ctx, physics) {
    const cx = physics.center.x;
    const cy = physics.center.y;
    const angle = physics.ringAngle;

    ctx.save();
    ctx.translate(cx, cy);
    ctx.rotate(angle);

    for (const gate of physics.gates) {
      const rot = (gate.isInner && physics.concentricRings)
        ? (physics.innerRingAngle - physics.ringAngle)
        : 0;
      const gAngle = gate.angle + rot;
      const rMin = gate.rMin !== undefined ? gate.rMin : (gate.r || 120) - 17;
      const rMax = gate.rMax !== undefined ? gate.rMax : (gate.r || 120) + 17;
      // Phase 2: Outer-anchored badge position leaves center channel open
      const rBadge = rMin + (rMax - rMin) * 0.82;

      const cosA = Math.cos(gAngle);
      const sinA = Math.sin(gAngle);

      const x1 = cosA * rMin;
      const y1 = sinA * rMin;
      const x2 = cosA * rMax;
      const y2 = sinA * rMax;
      const gx = cosA * rBadge;
      const gy = sinA * rBadge;

      if (gate.type === "BOOST") {
        this.renderBoosterGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate);
      } else if (gate.type === "ADD") {
        this.renderCompactorGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate);
      } else if (gate.value >= 4) {
        this.renderSuperShredderGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate);
      } else {
        this.renderSawbladeGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate);
      }
    }

    ctx.restore();
  }

  renderCartoonDebris(ctx, debrisList) {
    ctx.save();
    for (const d of debrisList) {
      if (d.state === "REMOVED") continue;

      ctx.save();
      ctx.translate(d.x, d.y);

      // 1. Cute Broken Satellite with wiggly antenna & solar wings
      if (d.type === "solar") {
        ctx.fillStyle = "#38bdf8";
        ctx.strokeStyle = "#1e1b4b";
        ctx.lineWidth = 1.8;

        // Satellite body
        ctx.beginPath();
        ctx.roundRect(
          -d.radius,
          -d.radius * 0.75,
          d.radius * 2,
          d.radius * 1.5,
          3,
        );
        ctx.fill();
        ctx.stroke();

        // Cute yellow solar wings
        ctx.fillStyle = "#facc15";
        ctx.fillRect(-d.radius - 5, -3, 4, 6);
        ctx.fillRect(d.radius + 1, -3, 4, 6);

        // Antenna
        ctx.strokeStyle = "#1e1b4b";
        ctx.beginPath();
        ctx.moveTo(0, -d.radius * 0.75);
        ctx.lineTo(0, -d.radius * 0.75 - 4);
        ctx.stroke();
        ctx.fillStyle = "#f472b6";
        ctx.beginPath();
        ctx.arc(0, -d.radius * 0.75 - 5, 2, 0, Math.PI * 2);
        ctx.fill();
      }
      // 2. Spicy Battery (Volatile) with cute sweat drop
      else if (d.type === "volatile") {
        ctx.fillStyle = "#ef4444";
        ctx.strokeStyle = "#1e1b4b";
        ctx.lineWidth = 2;

        ctx.beginPath();
        ctx.roundRect(
          -d.radius,
          -d.radius * 1.1,
          d.radius * 2,
          d.radius * 2.2,
          4,
        );
        ctx.fill();
        ctx.stroke();

        // Battery positive terminal nub
        ctx.fillStyle = "#facc15";
        ctx.fillRect(-3, -d.radius * 1.1 - 3, 6, 3);

        // Sweating drops!
        ctx.fillStyle = "#38bdf8";
        ctx.beginPath();
        ctx.arc(d.radius * 0.6, -d.radius * 0.7, 2, 0, Math.PI * 2);
        ctx.fill();
      }
      // 3. Smiling Golden Space Nut / Bolt (Standard)
      else {
        // Chunky golden circle / polygon with smiling face
        ctx.fillStyle = "#facc15";
        ctx.strokeStyle = "#1e1b4b";
        ctx.lineWidth = 2;

        ctx.beginPath();
        ctx.arc(0, 0, d.radius, 0, Math.PI * 2);
        ctx.fill();
        ctx.stroke();

        // Cute face
        ctx.fillStyle = "#1e1b4b";
        ctx.beginPath();
        ctx.arc(-2, -1, 1.2, 0, Math.PI * 2);
        ctx.arc(2, -1, 1.2, 0, Math.PI * 2);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(0, 1.5, 2, 0, Math.PI);
        ctx.stroke();
      }

      ctx.restore();
    }
    ctx.restore();
  }

  renderVfx(ctx, dt) {
    // 1. Cartoon Confetti Particles
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const p = this.particles[i];
      p.life -= dt;
      if (p.life <= 0) {
        this.particles.splice(i, 1);
        continue;
      }
      p.vy += p.gravity * dt;
      p.x += p.vx * dt;
      p.y += p.vy * dt;
      p.rot += p.vRot * dt;

      ctx.save();
      ctx.translate(p.x, p.y);
      ctx.rotate(p.rot);
      ctx.fillStyle = p.color;
      ctx.fillRect(-p.w * 0.5, -p.h * 0.5, p.w, p.h);
      ctx.restore();
    }

    // 2. Comic Pop Rings & Boost Shockwaves
    for (let i = this.popRings.length - 1; i >= 0; i--) {
      const r = this.popRings[i];
      r.radius += dt * (r.speed || 80);
      r.alpha -= dt * (r.fadeRate || 2.2);
      if (r.alpha <= 0 || r.radius >= r.radiusMax) {
        this.popRings.splice(i, 1);
        continue;
      }
      ctx.save();
      ctx.strokeStyle = r.color;
      ctx.globalAlpha = Math.max(0, Math.min(1, r.alpha));
      ctx.lineWidth = r.lineWidth || 3;
      ctx.beginPath();
      ctx.arc(r.x, r.y, r.radius, 0, Math.PI * 2);
      ctx.stroke();
      ctx.restore();
    }

    // 2b. Comic Thruster Smoke Puffs
    for (let i = this.thrusterPuffs.length - 1; i >= 0; i--) {
      const puff = this.thrusterPuffs[i];
      const alpha = (puff.life / puff.maxLife) * 0.85;

      ctx.save();
      ctx.globalAlpha = Math.max(0, Math.min(1, alpha));
      ctx.fillStyle = puff.color || "#ffffff";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(puff.x, puff.y, puff.radius, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();
      ctx.restore();
    }

    // 3. Bouncy Comic Pop Floating Text
    for (let i = this.floatingTexts.length - 1; i >= 0; i--) {
      const ft = this.floatingTexts[i];
      ft.life -= dt;
      if (ft.life <= 0) {
        this.floatingTexts.splice(i, 1);
        continue;
      }
      ft.y += ft.vy * dt;
      ft.alpha = ft.life / 0.85;

      ctx.save();
      ctx.translate(ft.x, ft.y);
      ctx.scale(ft.scale, ft.scale);

      // Dark cartoon stroke outline
      ctx.font = "900 16px Fredoka, sans-serif";
      ctx.textAlign = "center";
      ctx.strokeStyle = "#1e1b4b";
      ctx.lineWidth = 3.5;
      ctx.strokeText(ft.text, 0, 0);

      // Vibrant fill
      ctx.fillStyle = ft.color;
      ctx.fillText(ft.text, 0, 0);
      ctx.restore();
    }

    ctx.globalAlpha = 1.0;
  }
}
