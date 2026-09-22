/**
 * Orbital Salvage: Cartoon Visual Renderer ("Cute Cosmic Janitor")
 * Renders cute cartoon starfields, chunky candy-colored orbital rings,
 * adorable space trash (soda cans, smiling satellites, golden bolts),
 * and "Nibbles" the hungry cartoon black hole with animated eyes and mouth!
 */

import { COSMETIC_CATALOG } from "./economy.js";

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

  triggerNibblesChew() {
    this.chewTimer = 0.35;
    this.mouthOpenAmount = 1.0;
    this.bhWobble = 0.09; // Bouncy elastic scale pulse when snacking!
  }

  render(physics, economy, dt, progressInfo = null) {
    this.cosmicTime += dt;
    this.updateNibblesFace(physics, dt, progressInfo);

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

    ctx.save();
    // Dynamic Growth Scaling: visually scales from 1.0x to 1.65x without altering level completion conditions
    ctx.translate(bh.x, bh.y);
    ctx.scale(this.currentBhScale, this.currentBhScale);
    ctx.translate(-bh.x, -bh.y);

    // 1. Swirling Candy Accretion Donut Ring
    const haloRadius = bh.eventHorizonRadius * 2.5;
    ctx.save();
    ctx.translate(bh.x, bh.y);
    ctx.rotate(time * 0.5);

    ctx.fillStyle = skin.haloColor + "44";
    ctx.beginPath();
    ctx.arc(0, 0, haloRadius, 0, Math.PI * 2);
    ctx.fill();

    // Swirling candy sprinkles around Nibbles
    for (let i = 0; i < 8; i++) {
      const a = (Math.PI * 2 * i) / 8 + time * 0.8;
      const r = bh.eventHorizonRadius * 1.5 + Math.sin(time + i) * 6;
      ctx.fillStyle = i % 2 === 0 ? "#fde047" : "#f472b6";
      ctx.beginPath();
      ctx.arc(Math.cos(a) * r, Math.sin(a) * r, 3.5, 0, Math.PI * 2);
      ctx.fill();
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

  renderCartoonRing(ctx, physics, ringSkin) {
    const cx = physics.center.x;
    const cy = physics.center.y;
    const rOut = physics.ringRadius;
    const rIn = physics.innerRadius;
    const angle = physics.ringAngle;
    const apAngle = physics.apertureAngle;

    ctx.save();
    ctx.translate(cx, cy);

    // 1. Cartoon Air Puffs / Wind Lines when rotating
    if (Math.abs(physics.angularVelocity) > 0.4 || physics.rcsThrusting) {
      const dir = Math.sign(physics.angularVelocity || 1);
      this.renderCartoonWhoosh(ctx, rOut + 10, angle, dir);
    }

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
    ctx.beginPath();
    ctx.arc(0, 0, rOut, apStart, apEnd);
    ctx.stroke();

    // White glossy highlight line along the upper arc
    ctx.lineWidth = 3.5;
    ctx.strokeStyle = "rgba(255, 255, 255, 0.75)";
    ctx.beginPath();
    ctx.arc(0, 0, rOut - 4, Math.PI * 1.1, Math.PI * 1.9);
    ctx.stroke();

    // Cute decorative candy sprinkles / bolts on outer ring
    for (let i = 0; i < 10; i++) {
      const a = (Math.PI * 2 * i) / 10;
      // Skip aperture gap
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

    // 4. Inner Hub (Chunky central circle)
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

    // Happy cute station face
    ctx.fillStyle = "#1e1b4b";
    ctx.font = "bold 13px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText("✨ ORBITAL ✨", 0, -7);
    ctx.font = "11px Nunito, sans-serif";
    ctx.fillStyle = "#64748b";
    ctx.fillText("SORT RIG", 0, 9);

    // 4b. Concentric Track Partitions (if present)
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

        // Cartoon gateway indicators at transfer gap
        [-gapHalf, gapHalf].forEach((off) => {
          const capAng = gapAng + off;
          const capX = Math.cos(capAng) * tw.r;
          const capY = Math.sin(capAng) * tw.r;
          ctx.fillStyle = "#facc15";
          ctx.strokeStyle = "#1e1b4b";
          ctx.lineWidth = 2;
          ctx.beginPath();
          ctx.arc(capX, capY, 4, 0, Math.PI * 2);
          ctx.fill();
          ctx.stroke();
        });
      }
    }

    // 5. Chunky Radial Dividers (Candy Chutes)
    ctx.lineWidth = 7;
    ctx.lineCap = "round";
    for (const div of physics.dividers) {
      const dx1 = Math.cos(div.angle) * div.rMin;
      const dy1 = Math.sin(div.angle) * div.rMin;
      const dx2 = Math.cos(div.angle) * div.rMax;
      const dy2 = Math.sin(div.angle) * div.rMax;

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

  renderCartoonWhoosh(ctx, radius, currentAngle, dir) {
    ctx.save();
    const count = 3;
    for (let i = 0; i < count; i++) {
      const a = currentAngle + i * 0.35 * dir;
      const dist = radius + (i % 2) * 5;
      const wx = Math.cos(a) * dist;
      const wy = Math.sin(a) * dist;

      ctx.fillStyle = "#ffffff";
      ctx.globalAlpha = 0.75 - i * 0.2;
      ctx.beginPath();
      ctx.arc(wx, wy, 4.5 - i, 0, Math.PI * 2);
      ctx.fill();
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

  renderCartoonBumpers(ctx, physics) {
    if (!physics.deflectorPins || physics.deflectorPins.length === 0) return;

    const cx = physics.center.x;
    const cy = physics.center.y;
    const angle = physics.ringAngle;

    ctx.save();
    ctx.translate(cx, cy);
    ctx.rotate(angle);

    for (const pin of physics.deflectorPins) {
      const px = Math.cos(pin.angle) * pin.r;
      const py = Math.sin(pin.angle) * pin.r;
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
    ctx.lineWidth = 7;
    ctx.strokeStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Hazard orange beam
    ctx.lineWidth = 4;
    ctx.strokeStyle = mainColor;
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Warm inner core
    ctx.lineWidth = 1.6;
    ctx.strokeStyle = "#fef08a";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Dual spinning cartoon sawblade gears
    const spin = this.cosmicTime * 7;
    this.renderSpinningGear(ctx, x1, y1, 6.5, spin, mainColor);
    this.renderSpinningGear(ctx, x2, y2, 6.5, -spin, mainColor);

    // Hexagonal Badge
    ctx.save();
    ctx.translate(gx, gy);
    ctx.rotate(gAngle + Math.PI * 0.5);

    const bw = 24;
    const bh = 11;

    // Badge shadow
    ctx.fillStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.moveTo(-bw - 5, 2.5);
    ctx.lineTo(-bw, -bh + 2.5);
    ctx.lineTo(bw, -bh + 2.5);
    ctx.lineTo(bw + 5, 2.5);
    ctx.lineTo(bw, bh + 2.5);
    ctx.lineTo(-bw, bh + 2.5);
    ctx.closePath();
    ctx.fill();

    // Hex body
    ctx.fillStyle = "#ffffff";
    ctx.strokeStyle = mainColor;
    ctx.lineWidth = 2.5;
    ctx.beginPath();
    ctx.moveTo(-bw - 5, 0);
    ctx.lineTo(-bw, -bh);
    ctx.lineTo(bw, -bh);
    ctx.lineTo(bw + 5, 0);
    ctx.lineTo(bw, bh);
    ctx.lineTo(-bw, bh);
    ctx.closePath();
    ctx.fill();
    ctx.stroke();

    // Text: ⚡ ×N
    ctx.fillStyle = "#c2410c";
    ctx.font = "bold 11px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(`⚡ ×${gate.value}`, 0, 0.5);

    ctx.restore();
  }

  renderSuperShredderGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate) {
    const mainColor = "#ec4899";

    // Wide electric glow
    ctx.lineWidth = 10;
    ctx.strokeStyle = "rgba(236, 72, 153, 0.4)";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    // Jagged lightning crackle path
    const dx = x2 - x1;
    const dy = y2 - y1;
    const segCount = 6;
    ctx.lineWidth = 3;
    ctx.strokeStyle = "#ffffff";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    for (let i = 1; i < segCount; i++) {
      const frac = i / segCount;
      const px = x1 + dx * frac;
      const py = y1 + dy * frac;
      const perpX = -dy / (Math.hypot(dx, dy) + 0.001);
      const perpY = dx / (Math.hypot(dx, dy) + 0.001);
      const jitter = Math.sin(this.cosmicTime * 32 + i * 2) * 3.5;
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
      ctx.lineWidth = 1.8;
      ctx.beginPath();
      ctx.moveTo(0, -9);
      ctx.lineTo(3, -3);
      ctx.lineTo(9, 0);
      ctx.lineTo(3, 3);
      ctx.lineTo(0, 9);
      ctx.lineTo(-3, 3);
      ctx.lineTo(-9, 0);
      ctx.lineTo(-3, -3);
      ctx.closePath();
      ctx.fill();
      ctx.stroke();

      ctx.fillStyle = "#ffffff";
      ctx.beginPath();
      ctx.arc(0, 0, 2.5, 0, Math.PI * 2);
      ctx.fill();

      ctx.restore();
    });

    // Spiked Diamond Super Badge
    ctx.save();
    ctx.translate(gx, gy);
    ctx.rotate(gAngle + Math.PI * 0.5);

    // Glowing aura
    ctx.fillStyle = "rgba(236, 72, 153, 0.45)";
    ctx.beginPath();
    ctx.arc(0, 0, 20, 0, Math.PI * 2);
    ctx.fill();

    // Drop shadow
    ctx.fillStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.roundRect(-26, -11 + 2.5, 52, 22, 6);
    ctx.fill();

    // Magenta badge body
    ctx.fillStyle = "#ec4899";
    ctx.strokeStyle = "#ffffff";
    ctx.lineWidth = 2.2;
    ctx.beginPath();
    ctx.roundRect(-26, -11, 52, 22, 6);
    ctx.fill();
    ctx.stroke();

    // Text: 💥 ×N!
    ctx.fillStyle = "#ffffff";
    ctx.font = "900 12px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(`💥 ×${gate.value}!`, 0, 1);

    ctx.restore();
  }

  renderCompactorGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate) {
    const perpAngle = gAngle + Math.PI * 0.5;
    const offX = Math.cos(perpAngle) * 3;
    const offY = Math.sin(perpAngle) * 3;

    // Dual cyan magnetic guide rails
    ctx.lineWidth = 3.5;
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
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(wx, wy, 4 + wavePhase * 6, 0, Math.PI * 2);
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
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.roundRect(-5.5, -5.5, 11, 11, 3);
      ctx.fill();
      ctx.stroke();

      ctx.fillStyle = "#67e8f9";
      ctx.beginPath();
      ctx.arc(0, 0, 2.8, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    });

    // Smooth Rounded Pill Badge (🧲 +N)
    ctx.save();
    ctx.translate(gx, gy);
    ctx.rotate(gAngle + Math.PI * 0.5);

    // Drop shadow
    ctx.fillStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.roundRect(-24, -10 + 2.5, 48, 20, 10);
    ctx.fill();

    // Capsule body
    ctx.fillStyle = "#ffffff";
    ctx.strokeStyle = "#0891b2";
    ctx.lineWidth = 2.5;
    ctx.beginPath();
    ctx.roundRect(-24, -10, 48, 20, 10);
    ctx.fill();
    ctx.stroke();

    // Text: 🧲 +N
    ctx.fillStyle = "#0369a1";
    ctx.font = "bold 11px Fredoka, sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(`🧲 +${gate.value}`, 0, 0.5);

    ctx.restore();
  }

  renderBoosterGate(ctx, x1, y1, x2, y2, gx, gy, gAngle, gate) {
    // Glowing lime green accelerator track
    ctx.lineWidth = 7;
    ctx.strokeStyle = "rgba(132, 204, 22, 0.35)";
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.stroke();

    ctx.lineWidth = 3.5;
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
      ctx.lineWidth = 2;
      ctx.beginPath();
      ctx.arc(0, 0, 5, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();

      ctx.fillStyle = "#facc15";
      ctx.beginPath();
      ctx.arc(0, 0, 2.5, 0, Math.PI * 2);
      ctx.fill();
      ctx.restore();
    });

    // Skewed Aerodynamic Badge (⏩ BOOST)
    ctx.save();
    ctx.translate(gx, gy);
    ctx.rotate(gAngle + Math.PI * 0.5);

    ctx.transform(1, 0, -0.25, 1, 0, 0);

    // Shadow
    ctx.fillStyle = "#1e1b4b";
    ctx.beginPath();
    ctx.roundRect(-26, -10 + 2.5, 52, 20, 5);
    ctx.fill();

    // Body
    ctx.fillStyle = "#84cc16";
    ctx.strokeStyle = "#1e1b4b";
    ctx.lineWidth = 2.2;
    ctx.beginPath();
    ctx.roundRect(-26, -10, 52, 20, 5);
    ctx.fill();
    ctx.stroke();

    // Text: ⏩ BOOST
    ctx.fillStyle = "#ffffff";
    ctx.font = "bold 10.5px Fredoka, sans-serif";
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
      const gAngle = gate.angle;
      const rMin = gate.rMin !== undefined ? gate.rMin : (gate.r || 120) - 17;
      const rMax = gate.rMax !== undefined ? gate.rMax : (gate.r || 120) + 17;
      const rMid = (rMin + rMax) * 0.5;

      const cosA = Math.cos(gAngle);
      const sinA = Math.sin(gAngle);

      const x1 = cosA * rMin;
      const y1 = sinA * rMin;
      const x2 = cosA * rMax;
      const y2 = sinA * rMax;
      const gx = cosA * rMid;
      const gy = sinA * rMid;

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

    // 2. Comic Pop Rings
    for (let i = this.popRings.length - 1; i >= 0; i--) {
      const r = this.popRings[i];
      r.radius += dt * 80;
      r.alpha -= dt * 2.2;
      if (r.alpha <= 0 || r.radius >= r.radiusMax) {
        this.popRings.splice(i, 1);
        continue;
      }
      ctx.save();
      ctx.strokeStyle = r.color;
      ctx.globalAlpha = r.alpha;
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.arc(r.x, r.y, r.radius, 0, Math.PI * 2);
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
