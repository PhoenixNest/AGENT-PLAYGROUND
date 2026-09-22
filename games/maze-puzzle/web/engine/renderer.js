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

    // Nibbles Facial State
    this.nibblesBlinkTimer = 0;
    this.isBlinking = false;
    this.mouthOpenAmount = 0; // 0 = smiling, 1 = wide open NOM
    this.chewTimer = 0;

    // Cartoon VFX (Confetti, Stars, Comic Pop Text)
    this.particles = [];
    this.floatingTexts = [];
    this.popRings = [];
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
  }

  render(physics, economy, dt) {
    this.cosmicTime += dt;
    this.updateNibblesFace(physics, dt);

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

    // 4. Render Bouncy Comic Multiplier Gates
    this.renderCartoonGates(ctx, physics);

    // 5. Render Adorable Space Debris
    this.renderCartoonDebris(ctx, physics.debrisList);

    // 6. Render Cartoon Confetti & Comic Text Pop VFX
    this.renderVfx(ctx, dt);
  }

  updateNibblesFace(physics, dt) {
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
    ctx.fillText("✨ CLEAN ✨", 0, -7);
    ctx.font = "11px Nunito, sans-serif";
    ctx.fillStyle = "#64748b";
    ctx.fillText("SPACE RIG", 0, 9);

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

  renderCartoonGates(ctx, physics) {
    const cx = physics.center.x;
    const cy = physics.center.y;
    const angle = physics.ringAngle;

    ctx.save();
    ctx.translate(cx, cy);
    ctx.rotate(angle);

    for (const gate of physics.gates) {
      const gAngle = gate.angle;
      const r = gate.r;
      const halfLen = gate.length * 0.5;

      const gx = Math.cos(gAngle) * r;
      const gy = Math.sin(gAngle) * r;

      const perpAngle = gAngle + Math.PI * 0.5;
      const x1 = gx - Math.cos(perpAngle) * halfLen;
      const y1 = gy - Math.sin(perpAngle) * halfLen;
      const x2 = gx + Math.cos(perpAngle) * halfLen;
      const y2 = gy + Math.sin(perpAngle) * halfLen;

      // Colorful Bouncy Arch
      ctx.lineWidth = 5.5;
      ctx.strokeStyle = gate.color;
      ctx.beginPath();
      ctx.moveTo(x1, y1);
      ctx.lineTo(x2, y2);
      ctx.stroke();

      // Comic-Book Badge with 3D drop shadow
      ctx.save();
      ctx.translate(gx, gy);
      ctx.rotate(gAngle + Math.PI * 0.5);

      const label =
        gate.type === "MULTIPLY" ? `×${gate.value}!` : `+${gate.value}!`;

      // Shadow
      ctx.fillStyle = "#1e1b4b";
      ctx.beginPath();
      ctx.roundRect(-20, -10 + 2.5, 40, 22, 8);
      ctx.fill();

      // Main Badge
      ctx.fillStyle = "#ffffff";
      ctx.strokeStyle = gate.color;
      ctx.lineWidth = 2.5;
      ctx.beginPath();
      ctx.roundRect(-20, -10, 40, 22, 8);
      ctx.fill();
      ctx.stroke();

      // Text
      ctx.fillStyle = gate.color;
      ctx.font = "bold 12px Fredoka, sans-serif";
      ctx.textAlign = "center";
      ctx.textBaseline = "middle";
      ctx.fillText(label, 0, 1);

      ctx.restore();
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
