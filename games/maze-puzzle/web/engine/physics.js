/**
 * Orbital Salvage: Physics & Spatial Simulation Engine
 * Simulates rotating orbital centrifuges, concentric tracks, debris rigid bodies,
 * continuous boundary collisions, radial shredder gates, and black hole gravitation.
 */

export class PhysicsEngine {
  constructor(audioEngine) {
    this.audio = audioEngine;
    this.debrisList = [];
    this.gates = [];
    this.dividers = [];
    this.trackWalls = [];
    this.nextDebrisId = 1;

    // Center of the orbital ring
    this.center = { x: 400, y: 320 };
    this.ringRadius = 180;
    this.corridorWidth = 40;
    this.innerRadius = 100;
    this.apertureAngle = Math.PI * 0.28;

    // Ring kinematics
    this.ringAngle = 0;
    this.angularVelocity = 0;
    this.dragSensitivity = 0.005;
    this.damping = 0.94;
    this.rcsThrusting = false;
    this.rcsDirection = 0;
    this.rcsBoostTimer = 0;
    this.lastRatchetAngle = 0;

    // Environmental Gravity (Gentle downward drift)
    this.gravity = { x: 0, y: 360 };

    // Micro-Black Hole (Nibbles)
    this.blackHole = {
      x: 400,
      y: 575,
      eventHorizonRadius: 28,
      gravityRadius: 240,
      gravityStrength: 480000,
    };

    // Performance Cap
    this.maxDebrisLimit = 320;

    // Deflector Bumpers (Pachinko Pins)
    this.deflectorPins = [];

    // One-Way Gravity Flappers
    this.oneWayFlaps = [];

    // Concentric Dual-Ring Gearing
    this.concentricRings = false;
    this.gearRatio = -1.0;
    this.innerRingAngle = 0;
    this.trackRadius = 130;

    // Callbacks
    this.onDebrisSwallowed = null;
    this.onDebrisExploded = null;
    this.onGatePassed = null;
    this.onBumperHit = null;
    this.onBoostTriggered = null;
  }

  initSector(sectorConfig, centerPos, blackHolePos) {
    this.center = { ...centerPos };
    this.ringRadius = sectorConfig.ringRadius;
    this.corridorWidth = sectorConfig.corridorWidth;
    this.innerRadius =
      sectorConfig.innerRadius || this.ringRadius - this.corridorWidth * 2;
    this.apertureAngle = sectorConfig.apertureAngle;

    if (blackHolePos) {
      this.blackHole.x = blackHolePos.x;
      this.blackHole.y = blackHolePos.y;
    }

    this.ringAngle = 0;
    this.angularVelocity = 0;
    this.lastRatchetAngle = 0;
    this.debrisList = [];

    // Concentric Dual-Ring Setup
    this.concentricRings = !!sectorConfig.concentricRings;
    this.gearRatio = sectorConfig.gearRatio !== undefined ? sectorConfig.gearRatio : -1.0;
    this.innerRingAngle = 0;
    this.trackRadius = sectorConfig.trackWalls && sectorConfig.trackWalls[0]
      ? sectorConfig.trackWalls[0].r
      : (this.innerRadius + this.ringRadius) * 0.5;

    // One-Way Flappers
    this.oneWayFlaps = (sectorConfig.oneWayFlaps || []).map((f, idx) => ({
      ...f,
      id: idx,
      rMin: f.rMin !== undefined ? f.rMin : this.innerRadius + 5,
      rMax: f.rMax !== undefined ? f.rMax : this.ringRadius - 5,
      angle: f.angle || 0,
      swing: 0,
      swingVel: 0,
      maxSwing: f.maxSwing || Math.PI * 0.35,
      normalSign: f.normalSign !== undefined ? f.normalSign : 1,
      isInner: !!f.isInner,
    }));

    // Support radial span [rMin, rMax] for each shredder gate
    this.gates = sectorConfig.gates.map((g, idx) => ({
      ...g,
      id: idx,
      rMin:
        g.rMin !== undefined ? g.rMin : (g.r || 120) - (g.length || 34) * 0.5,
      rMax:
        g.rMax !== undefined ? g.rMax : (g.r || 120) + (g.length || 34) * 0.5,
      isInner: g.isInner !== undefined ? g.isInner : (this.concentricRings && g.rMax <= this.trackRadius + 5),
      processedDebris: new Set(),
    }));

    this.dividers = (sectorConfig.dividers || []).map((d) => ({
      ...d,
      isInner: d.isInner !== undefined ? d.isInner : (this.concentricRings && d.rMax <= this.trackRadius + 5),
    }));

    this.trackWalls = sectorConfig.trackWalls
      ? [...sectorConfig.trackWalls]
      : [];

    this.deflectorPins = (sectorConfig.deflectorPins || []).map((p, idx) => ({
      ...p,
      id: idx,
      radius: p.radius || 9,
      restitution: p.restitution || 1.35,
      isInner: p.isInner !== undefined ? p.isInner : (this.concentricRings && p.r <= this.trackRadius),
      hitTimer: 0,
    }));

    // Spawn initial debris cluster in top portion of ring
    const count = sectorConfig.initialDebrisCount || 14;
    const types = sectorConfig.initialDebrisTypes || ["titanium"];

    for (let i = 0; i < count; i++) {
      const type = types[i % types.length];
      const spawnAngle = -Math.PI * 0.5 + (Math.random() - 0.5) * 0.7;
      const spawnRadius =
        this.innerRadius + Math.random() * (this.ringRadius - this.innerRadius);
      const x = this.center.x + Math.cos(spawnAngle) * spawnRadius;
      const y = this.center.y + Math.sin(spawnAngle) * spawnRadius;

      this.spawnDebris({
        x,
        y,
        vx: (Math.random() - 0.5) * 20,
        vy: (Math.random() - 0.5) * 20,
        type,
      });
    }
  }

  spawnDebris({ x, y, vx = 0, vy = 0, type = "titanium", weight = 1 }) {
    if (this.debrisList.length >= this.maxDebrisLimit) {
      // Scale weight of existing debris instead of overflowing entities
      const existing =
        this.debrisList[Math.floor(Math.random() * this.debrisList.length)];
      if (existing) existing.weight += 1;
      return null;
    }

    let radius = 6.5;
    let color = "#38bdf8";
    let mass = 1.0;

    if (type === "solar") {
      radius = 5.5;
      color = "#fbbf24";
      mass = 0.8;
    } else if (type === "volatile") {
      radius = 7.5;
      color = "#ef4444";
      mass = 1.2;
    } else if (type === "magnetic") {
      radius = 6.0;
      color = "#a855f7";
      mass = 1.1;
    }

    const debris = {
      id: this.nextDebrisId++,
      x,
      y,
      vx,
      vy,
      radius,
      mass,
      type,
      color,
      weight,
      state: "IN_RING", // IN_RING | FALLING | INGESTING | REMOVED
      ingestProgress: 0,
      sparkTrails: [],
    };

    this.debrisList.push(debris);
    return debris;
  }

  applyInputDrag(deltaX) {
    this.angularVelocity += deltaX * this.dragSensitivity;
    this.angularVelocity = Math.max(-12, Math.min(12, this.angularVelocity));
  }

  applyRcsBurst(direction = 1) {
    this.angularVelocity += direction * 3.8;
    this.rcsThrusting = true;
    this.rcsDirection = direction;
    this.rcsBoostTimer = 0.42;
    this.audio.playSpinRatchet(1.6);
    if (this.onBoostTriggered) {
      this.onBoostTriggered(direction);
    }
  }

  update(dt) {
    // 0. Update RCS Thruster Timer
    if (this.rcsBoostTimer > 0) {
      this.rcsBoostTimer = Math.max(0, this.rcsBoostTimer - dt);
      if (this.rcsBoostTimer === 0) {
        this.rcsThrusting = false;
        this.rcsDirection = 0;
      }
    }

    // 1. Update Ring Rotation & Ratchet Sound Feedback
    const prevAngle = this.ringAngle;
    this.ringAngle += this.angularVelocity * dt;
    this.angularVelocity *= Math.pow(this.damping, dt * 60);
    if (Math.abs(this.angularVelocity) < 0.001) {
      this.angularVelocity = 0;
    }

    // Concentric Differential Rotation
    if (this.concentricRings) {
      this.innerRingAngle += this.angularVelocity * this.gearRatio * dt;
    } else {
      this.innerRingAngle = this.ringAngle;
    }

    // Update One-Way Gravity Flappers (Spring return to stopper at 0)
    for (const f of this.oneWayFlaps) {
      f.swingVel += (-f.swing * 28 - f.swingVel * 10) * dt;
      f.swing += f.swingVel * dt;
      if (f.swing < 0) {
        f.swing = 0;
        f.swingVel = 0;
      } else if (f.swing > f.maxSwing) {
        f.swing = f.maxSwing;
        f.swingVel = 0;
      }
    }

    // Play subtle wooden ratchet clicks when wheel rotates through angle increments
    const angleTraveled = Math.abs(this.ringAngle - this.lastRatchetAngle);
    if (angleTraveled >= Math.PI / 16) {
      // ~11 degrees per ratchet step
      const spinSpeed = Math.abs(this.angularVelocity);
      this.audio.playSpinRatchet(Math.min(1.5, 0.8 + spinSpeed * 0.2));
      this.lastRatchetAngle = this.ringAngle;
    }

    // Aperture window in world space (exit centered at bottom PI/2)
    const worldApertureStart = this.normalizeAngle(
      this.ringAngle + Math.PI * 0.5 - this.apertureAngle * 0.5,
    );
    const worldApertureEnd = this.normalizeAngle(
      this.ringAngle + Math.PI * 0.5 + this.apertureAngle * 0.5,
    );

    // Update deflector pin bumper animation timers
    for (const pin of this.deflectorPins) {
      if (pin.hitTimer > 0) {
        pin.hitTimer = Math.max(0, pin.hitTimer - dt);
      }
    }

    // 2. Physics step for all debris
    for (let i = this.debrisList.length - 1; i >= 0; i--) {
      const d = this.debrisList[i];

      if (d.state === "REMOVED") {
        this.debrisList.splice(i, 1);
        continue;
      }

      if (d.state === "INGESTING") {
        this.updateIngestion(d, dt);
        continue;
      }

      // Check Black Hole Gravitational Pull
      const dxToBh = this.blackHole.x - d.x;
      const dyToBh = this.blackHole.y - d.y;
      const distToBh = Math.hypot(dxToBh, dyToBh);

      if (distToBh <= this.blackHole.eventHorizonRadius) {
        // Swallowed into the Singularity!
        d.state = "INGESTING";
        this.audio.playDebrisSwallowed();
        if (this.onDebrisSwallowed) {
          this.onDebrisSwallowed(d);
        }
        continue;
      }

      // Gravitational pull from Black Hole (inverse-square law)
      if (distToBh < this.blackHole.gravityRadius) {
        const force =
          (this.blackHole.gravityStrength / (distToBh * distToBh + 400)) *
          d.mass;
        const normX = dxToBh / (distToBh + 0.001);
        const normY = dyToBh / (distToBh + 0.001);
        d.vx += normX * force * dt;
        d.vy += normY * force * dt;
      }

      // Environmental Gravity
      d.vx += this.gravity.x * dt;
      d.vy += this.gravity.y * dt;

      // Centrifugal / Inertial Forces if inside the rotating ring
      const dxToCenter = d.x - this.center.x;
      const dyToCenter = d.y - this.center.y;
      const distToCenter = Math.hypot(dxToCenter, dyToCenter);

      if (d.state === "IN_RING") {
        const ringVel = (this.concentricRings && distToCenter < this.trackRadius)
          ? this.angularVelocity * this.gearRatio
          : this.angularVelocity;

        // Mild centrifugal outward force
        const centrifugal =
          distToCenter * (ringVel * ringVel) * 0.06;
        if (distToCenter > 1) {
          d.vx += (dxToCenter / distToCenter) * centrifugal * dt;
          d.vy += (dyToCenter / distToCenter) * centrifugal * dt;
        }

        // Tangential friction drag from moving corridors
        const tangX = -dyToCenter / distToCenter;
        const tangY = dxToCenter / distToCenter;
        const ringSurfaceSpeed = ringVel * distToCenter;
        const debrisTangSpeed = d.vx * tangX + d.vy * tangY;
        const speedDiff = ringSurfaceSpeed - debrisTangSpeed;
        d.vx += tangX * speedDiff * dt * 1.4;
        d.vy += tangY * speedDiff * dt * 1.4;
      }

      // Integrate velocity to position
      d.x += d.vx * dt;
      d.y += d.vy * dt;

      // Air / space resistance damping
      d.vx *= 0.994;
      d.vy *= 0.994;

      // Boundary Collisions
      this.resolveRingCollisions(d, worldApertureStart, worldApertureEnd);

      // Radial Dividers, One-Way Flappers, Track Walls, Bumpers & Shredder Gates
      if (d.state === "IN_RING") {
        this.resolveDividerCollisions(d);
        this.resolveFlapperCollisions(d);
        this.resolveTrackWallCollisions(d);
        this.resolveDeflectorCollisions(d);
        this.checkMultiplierGates(d);
      }
    }

    // Inter-debris collisions
    this.resolveDebrisCollisions();
  }

  updateIngestion(d, dt) {
    d.ingestProgress += dt * 3.8;
    // Spiral inward toward black hole center
    const dx = this.blackHole.x - d.x;
    const dy = this.blackHole.y - d.y;
    d.x += dx * dt * 8.5 - dy * dt * 4.5;
    d.y += dy * dt * 8.5 + dx * dt * 4.5;
    d.radius = Math.max(0.1, d.radius * (1 - dt * 4.5));

    if (d.ingestProgress >= 1.0 || d.radius <= 0.5) {
      d.state = "REMOVED";
    }
  }

  resolveRingCollisions(d, apStart, apEnd) {
    const dx = d.x - this.center.x;
    const dy = d.y - this.center.y;
    const dist = Math.hypot(dx, dy);
    if (dist < 1) return;

    const angle = this.normalizeAngle(Math.atan2(dy, dx));
    const isOverAperture = this.isAngleBetween(angle, apStart, apEnd);

    // Outer wall collision
    if (dist > this.ringRadius - d.radius) {
      if (isOverAperture) {
        // Cleanly exiting through aperture into void toward Nibbles!
        d.state = "FALLING";
      } else {
        // Push inside outer ring
        const overlap = dist - (this.ringRadius - d.radius);
        const nx = dx / dist;
        const ny = dy / dist;
        d.x -= nx * overlap;
        d.y -= ny * overlap;

        // Reflect velocity
        const dot = d.vx * nx + d.vy * ny;
        if (dot > 0) {
          d.vx = (d.vx - 1.25 * dot * nx) * 0.8;
          d.vy = (d.vy - 1.25 * dot * ny) * 0.8;
          this.audio.playDebrisCollision(Math.min(1.0, dot / 70), d.type);
          this.checkVolatileDetonation(d, dot);
        }
      }
    }

    // Inner hub collision
    if (d.state === "IN_RING" && dist < this.innerRadius + d.radius) {
      const overlap = this.innerRadius + d.radius - dist;
      const nx = dx / dist;
      const ny = dy / dist;
      d.x += nx * overlap;
      d.y += ny * overlap;

      const dot = d.vx * nx + d.vy * ny;
      if (dot < 0) {
        d.vx = (d.vx - 1.25 * dot * nx) * 0.8;
        d.vy = (d.vy - 1.25 * dot * ny) * 0.8;
        this.audio.playDebrisCollision(
          Math.min(1.0, Math.abs(dot) / 70),
          d.type,
        );
      }
    }
  }

  resolveDividerCollisions(d) {
    const dx = d.x - this.center.x;
    const dy = d.y - this.center.y;
    const dist = Math.hypot(dx, dy);

    for (const div of this.dividers) {
      if (dist < div.rMin - d.radius || dist > div.rMax + d.radius) continue;

      const ringRot = (div.isInner && this.concentricRings)
        ? this.innerRingAngle
        : this.ringAngle;

      const angleInRing = this.normalizeAngle(
        Math.atan2(dy, dx) - ringRot,
      );

      const divAngle = this.normalizeAngle(div.angle);
      let angleDiff = angleInRing - divAngle;
      while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
      while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

      const arcDist = angleDiff * dist;
      const thickness = 6; // divider wall thickness

      if (Math.abs(arcDist) < thickness * 0.5 + d.radius) {
        // Collision with divider normal
        const normalSign = arcDist > 0 ? 1 : -1;
        const worldDivAngle = divAngle + ringRot;
        const normalX = -Math.sin(worldDivAngle) * normalSign;
        const normalY = Math.cos(worldDivAngle) * normalSign;

        // Push out of wall
        const penetration = thickness * 0.5 + d.radius - Math.abs(arcDist);
        d.x += normalX * penetration;
        d.y += normalY * penetration;

        const dot = d.vx * normalX + d.vy * normalY;
        if (dot < 0) {
          d.vx = (d.vx - 1.3 * dot * normalX) * 0.75;
          d.vy = (d.vy - 1.3 * dot * normalY) * 0.75;
          this.audio.playDebrisCollision(
            Math.min(1.0, Math.abs(dot) / 65),
            d.type,
          );
          this.checkVolatileDetonation(d, Math.abs(dot));
        }
      }
    }
  }

  resolveFlapperCollisions(d) {
    if (!this.oneWayFlaps || !this.oneWayFlaps.length) return;
    const dx = d.x - this.center.x;
    const dy = d.y - this.center.y;
    const dist = Math.hypot(dx, dy);

    for (const f of this.oneWayFlaps) {
      if (dist < f.rMin - d.radius || dist > f.rMax + d.radius) continue;

      const ringRot = (f.isInner && this.concentricRings)
        ? this.innerRingAngle
        : this.ringAngle;

      const angleInRing = this.normalizeAngle(
        Math.atan2(dy, dx) - ringRot,
      );

      const currentFlapAngle = this.normalizeAngle(f.angle + f.swing * (f.normalSign || 1));
      let angleDiff = angleInRing - currentFlapAngle;
      while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
      while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

      const arcDist = angleDiff * dist;
      const thickness = 7;

      if (Math.abs(arcDist) < thickness * 0.5 + d.radius) {
        const forwardSign = f.normalSign || 1;
        const pushDirection = (arcDist * forwardSign);

        if (pushDirection < 0) {
          // Debris is pushing in opening direction
          const pushForce = Math.min(2.5, (thickness * 0.5 + d.radius - Math.abs(arcDist)) * 0.18);
          f.swing += pushForce * 0.3;
          f.swingVel += pushForce * 12;
          if (f.swing > f.maxSwing) f.swing = f.maxSwing;
        } else {
          // Debris is pushing against closed stopper
          if (f.swing <= 0.08) {
            const normalSign = arcDist > 0 ? 1 : -1;
            const worldFlapAngle = currentFlapAngle + ringRot;
            const normalX = -Math.sin(worldFlapAngle) * normalSign;
            const normalY = Math.cos(worldFlapAngle) * normalSign;

            const penetration = thickness * 0.5 + d.radius - Math.abs(arcDist);
            d.x += normalX * penetration;
            d.y += normalY * penetration;

            const dot = d.vx * normalX + d.vy * normalY;
            if (dot < 0) {
              d.vx = (d.vx - 1.25 * dot * normalX) * 0.75;
              d.vy = (d.vy - 1.25 * dot * normalY) * 0.75;
              this.audio.playDebrisCollision(Math.min(1.0, Math.abs(dot) / 60), d.type);
            }
          }
        }
      }
    }
  }

  resolveTrackWallCollisions(d) {
    if (!this.trackWalls.length) return;

    const dx = d.x - this.center.x;
    const dy = d.y - this.center.y;
    const dist = Math.hypot(dx, dy);
    const angleInRing = this.normalizeAngle(
      Math.atan2(dy, dx) - this.ringAngle,
    );

    for (const tw of this.trackWalls) {
      const thickness = 6;
      if (Math.abs(dist - tw.r) < thickness * 0.5 + d.radius) {
        // Check if inside the transfer gap
        const gapAng = this.normalizeAngle(tw.gapAngle);
        let diff = angleInRing - gapAng;
        while (diff > Math.PI) diff -= Math.PI * 2;
        while (diff < -Math.PI) diff += Math.PI * 2;

        if (Math.abs(diff) < (tw.gapSize || 0.4) * 0.5) {
          // Debris is crossing through the transfer gap!
          continue;
        }

        // Push out radially
        const isOutside = dist > tw.r;
        const nx = dx / dist;
        const ny = dy / dist;
        const targetDist = isOutside
          ? tw.r + thickness * 0.5 + d.radius
          : tw.r - thickness * 0.5 - d.radius;
        const shift = targetDist - dist;

        d.x += nx * shift;
        d.y += ny * shift;

        // Reflect radial velocity
        const dot = d.vx * nx + d.vy * ny;
        if ((isOutside && dot < 0) || (!isOutside && dot > 0)) {
          d.vx = (d.vx - 1.3 * dot * nx) * 0.75;
          d.vy = (d.vy - 1.3 * dot * ny) * 0.75;
          this.audio.playDebrisCollision(
            Math.min(1.0, Math.abs(dot) / 65),
            d.type,
          );
        }
      }
    }
  }

  resolveDeflectorCollisions(d) {
    if (!this.deflectorPins || this.deflectorPins.length === 0) return;

    for (const pin of this.deflectorPins) {
      const ringRot = (pin.isInner && this.concentricRings)
        ? this.innerRingAngle
        : this.ringAngle;
      const ringVel = (pin.isInner && this.concentricRings)
        ? this.angularVelocity * this.gearRatio
        : this.angularVelocity;

      // Calculate current world position of pin inside rotating ring
      const worldAngle = ringRot + pin.angle;
      const pinX = this.center.x + Math.cos(worldAngle) * pin.r;
      const pinY = this.center.y + Math.sin(worldAngle) * pin.r;

      const dx = d.x - pinX;
      const dy = d.y - pinY;
      const dist = Math.hypot(dx, dy);
      const minDist = pin.radius + d.radius;

      if (dist < minDist && dist > 0.001) {
        const nx = dx / dist;
        const ny = dy / dist;
        const overlap = minDist - dist;

        // Push debris clear of bumper dome
        d.x += nx * overlap;
        d.y += ny * overlap;

        // Bumper tangential surface velocity due to ring rotation
        const pinVx = -Math.sin(worldAngle) * pin.r * ringVel;
        const pinVy = Math.cos(worldAngle) * pin.r * ringVel;

        // Relative velocity
        const relVx = d.vx - pinVx;
        const relVy = d.vy - pinVy;
        const velAlongNorm = relVx * nx + relVy * ny;

        if (velAlongNorm < 0) {
          const restitution = pin.restitution || 1.35;
          const impulse = -(1 + restitution) * velAlongNorm;

          d.vx += nx * impulse;
          d.vy += ny * impulse;

          // Scatter jitter to break up clusters into dynamic streams
          d.vx += (Math.random() - 0.5) * 20;
          d.vy += (Math.random() - 0.5) * 20;

          // Trigger visual flash and bumper audio
          pin.hitTimer = 0.25;
          if (this.audio && this.audio.playBumperBounce) {
            this.audio.playBumperBounce();
          }
          if (this.onBumperHit) {
            this.onBumperHit({ x: pinX, y: pinY, pin, debris: d });
          }
        }
      }
    }
  }

  checkMultiplierGates(d) {
    const dx = d.x - this.center.x;
    const dy = d.y - this.center.y;
    const dist = Math.hypot(dx, dy);

    for (const gate of this.gates) {
      if (gate.processedDebris.has(d.id)) continue;

      const ringRot = (gate.isInner && this.concentricRings)
        ? this.innerRingAngle
        : this.ringAngle;

      const angleInRing = this.normalizeAngle(
        Math.atan2(dy, dx) - ringRot,
      );

      const gateAngle = this.normalizeAngle(gate.angle);
      let angleDiff = angleInRing - gateAngle;
      while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
      while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

      // Check if crossing radial shredder gate line [gate.rMin, gate.rMax]
      const arcDist = Math.abs(angleDiff * dist);
      if (
        arcDist < 16 &&
        dist >= gate.rMin - d.radius &&
        dist <= gate.rMax + d.radius
      ) {
        gate.processedDebris.add(d.id);
        if (this.audio && this.audio.playGatePass) {
          this.audio.playGatePass(gate.type, gate.value);
        }

        if (this.onGatePassed) {
          this.onGatePassed(gate, d);
        }

        if (gate.type === "BOOST") {
          // Tangential and radial slingshot acceleration
          const boostMagnitude = gate.value || 150;
          const ringDir = Math.sign(this.angularVelocity) || 1;
          const boostAngle = ringRot + gateAngle + Math.PI * 0.5 * ringDir;
          d.vx += Math.cos(boostAngle) * boostMagnitude;
          d.vy += Math.sin(boostAngle) * boostMagnitude;
          this.audio.playRcsThrust();
          continue;
        }

        // Clone or multiply debris
        let newCount = 0;
        if (gate.type === "MULTIPLY") {
          newCount = Math.max(0, gate.value - 1);
        } else if (gate.type === "ADD") {
          newCount = gate.value;
        }

        for (let k = 0; k < newCount; k++) {
          const jitterX = (Math.random() - 0.5) * d.radius * 0.8;
          const jitterY = (Math.random() - 0.5) * d.radius * 0.8;
          const spawned = this.spawnDebris({
            x: d.x + jitterX,
            y: d.y + jitterY,
            vx: d.vx * (0.85 + Math.random() * 0.3) + (Math.random() - 0.5) * 15,
            vy: d.vy * (0.85 + Math.random() * 0.3) + (Math.random() - 0.5) * 15,
            type: d.type,
            weight: 1,
          });
          if (spawned) {
            gate.processedDebris.add(spawned.id);
          }
        }
      }
    }
  }

  checkVolatileDetonation(d, impactVelocity) {
    if (d.type !== "volatile" || impactVelocity < 170) return;

    // Volatile scrap explodes upon severe impact!
    d.state = "REMOVED";
    this.audio.playHazardExplosion();

    if (this.onDebrisExploded) {
      this.onDebrisExploded(d);
    }

    // Blast shockwave pushing adjacent debris
    for (const other of this.debrisList) {
      if (other.id === d.id) continue;
      const dx = other.x - d.x;
      const dy = other.y - d.y;
      const dist = Math.hypot(dx, dy);
      if (dist < 55) {
        const force = (55 - dist) * 8;
        other.vx += (dx / (dist + 0.1)) * force;
        other.vy += (dy / (dist + 0.1)) * force;
      }
    }
  }

  resolveDebrisCollisions() {
    const len = this.debrisList.length;
    for (let i = 0; i < len; i++) {
      const a = this.debrisList[i];
      if (a.state === "REMOVED" || a.state === "INGESTING") continue;

      for (let j = i + 1; j < len; j++) {
        const b = this.debrisList[j];
        if (b.state === "REMOVED" || b.state === "INGESTING") continue;

        const dx = b.x - a.x;
        const dy = b.y - a.y;
        const dist = Math.hypot(dx, dy);
        const minDist = a.radius + b.radius;

        if (dist < minDist && dist > 0.001) {
          const overlap = minDist - dist;
          const nx = dx / dist;
          const ny = dy / dist;

          // Push apart
          a.x -= nx * overlap * 0.5;
          a.y -= ny * overlap * 0.5;
          b.x += nx * overlap * 0.5;
          b.y += ny * overlap * 0.5;

          // Impulse exchange
          const kx = a.vx - b.vx;
          const ky = a.vy - b.vy;
          const p = (2 * (nx * kx + ny * ky)) / (a.mass + b.mass);

          if (p > 0) {
            a.vx -= p * b.mass * nx * 0.8;
            a.vy -= p * b.mass * ny * 0.8;
            b.vx += p * a.mass * nx * 0.8;
            b.vy += p * a.mass * ny * 0.8;
          }
        }
      }
    }
  }

  normalizeAngle(rad) {
    let a = rad % (Math.PI * 2);
    if (a < 0) a += Math.PI * 2;
    return a;
  }

  isAngleBetween(angle, start, end) {
    if (start <= end) {
      return angle >= start && angle <= end;
    }
    return angle >= start || angle <= end;
  }
}
