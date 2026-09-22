/**
 * Orbital Salvage: Physics & Spatial Simulation Engine
 * Simulates rotating orbital centrifuges, debris rigid bodies,
 * continuous boundary collisions, multiplier gates, and black hole gravitation.
 */

export class PhysicsEngine {
  constructor(audioEngine) {
    this.audio = audioEngine;
    this.debrisList = [];
    this.gates = [];
    this.dividers = [];
    this.nextDebrisId = 1;

    // Center of the orbital ring
    this.center = { x: 400, y: 320 };
    this.ringRadius = 180;
    this.corridorWidth = 40;
    this.innerRadius = 100;
    this.apertureAngle = Math.PI * 0.25;

    // Ring kinematics
    this.ringAngle = 0;
    this.angularVelocity = 0;
    this.dragSensitivity = 0.005;
    this.damping = 0.94;
    this.rcsThrusting = false;

    // Environmental Gravity
    this.gravity = { x: 0, y: 380 };

    // Micro-Black Hole
    this.blackHole = {
      x: 400,
      y: 575,
      eventHorizonRadius: 28,
      gravityRadius: 220,
      gravityStrength: 450000,
    };

    // Performance Cap
    this.maxDebrisLimit = 320;

    // Callbacks
    this.onDebrisSwallowed = null;
    this.onDebrisExploded = null;
    this.onGatePassed = null;
  }

  initSector(sectorConfig, centerPos, blackHolePos) {
    this.center = { ...centerPos };
    this.ringRadius = sectorConfig.ringRadius;
    this.corridorWidth = sectorConfig.corridorWidth;
    this.innerRadius = this.ringRadius - this.corridorWidth * 2;
    this.apertureAngle = sectorConfig.apertureAngle;

    if (blackHolePos) {
      this.blackHole.x = blackHolePos.x;
      this.blackHole.y = blackHolePos.y;
    }

    this.ringAngle = 0;
    this.angularVelocity = 0;
    this.debrisList = [];
    this.gates = sectorConfig.gates.map((g, idx) => ({
      ...g,
      id: idx,
      processedDebris: new Set(),
    }));
    this.dividers = [...sectorConfig.dividers];

    // Spawn initial debris cluster in top portion of ring
    const count = sectorConfig.initialDebrisCount || 15;
    const types = sectorConfig.initialDebrisTypes || ["titanium"];

    for (let i = 0; i < count; i++) {
      const type = types[i % types.length];
      const spawnAngle = -Math.PI * 0.5 + (Math.random() - 0.5) * 0.8;
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
    // Cap max rotational velocity
    this.angularVelocity = Math.max(-12, Math.min(12, this.angularVelocity));
  }

  applyRcsBurst(direction = 1) {
    this.angularVelocity += direction * 3.5;
    this.rcsThrusting = true;
    this.audio.setThrusterActive(true);
    setTimeout(() => {
      this.rcsThrusting = false;
      this.audio.setThrusterActive(false);
    }, 280);
  }

  update(dt) {
    // 1. Update Ring Rotation
    this.ringAngle += this.angularVelocity * dt;
    this.angularVelocity *= Math.pow(this.damping, dt * 60);
    if (Math.abs(this.angularVelocity) < 0.001) {
      this.angularVelocity = 0;
    }

    // Audio cue for active continuous rotation
    if (Math.abs(this.angularVelocity) > 0.8) {
      this.audio.setThrusterActive(true);
    } else if (!this.rcsThrusting) {
      this.audio.setThrusterActive(false);
    }

    // Normalized aperture angle window in world space
    // Standard bottom exit is at angle PI/2 (straight down)
    const worldApertureStart = this.normalizeAngle(
      this.ringAngle + Math.PI * 0.5 - this.apertureAngle * 0.5,
    );
    const worldApertureEnd = this.normalizeAngle(
      this.ringAngle + Math.PI * 0.5 + this.apertureAngle * 0.5,
    );

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

      // Gravitational force from Black Hole (inverse-square law)
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
        // Centrifugal outward force
        const centrifugal =
          distToCenter * (this.angularVelocity * this.angularVelocity);
        if (distToCenter > 1) {
          d.vx += (dxToCenter / distToCenter) * centrifugal * dt * 0.15;
          d.vy += (dyToCenter / distToCenter) * centrifugal * dt * 0.15;
        }

        // Tangential friction drag from moving corridors
        const tangX = -dyToCenter / distToCenter;
        const tangY = dxToCenter / distToCenter;
        const ringSurfaceSpeed = this.angularVelocity * distToCenter;
        const debrisTangSpeed = d.vx * tangX + d.vy * tangY;
        const speedDiff = ringSurfaceSpeed - debrisTangSpeed;
        d.vx += tangX * speedDiff * dt * 1.8;
        d.vy += tangY * speedDiff * dt * 1.8;
      }

      // Integrate velocity to position
      d.x += d.vx * dt;
      d.y += d.vy * dt;

      // Air resistance
      d.vx *= 0.995;
      d.vy *= 0.995;

      // Boundary Collisions
      this.resolveRingCollisions(d, worldApertureStart, worldApertureEnd);

      // Radial Dividers Collisions
      if (d.state === "IN_RING") {
        this.resolveDividerCollisions(d);
        this.checkMultiplierGates(d);
      }
    }

    // Inter-debris collisions
    this.resolveDebrisCollisions();
  }

  updateIngestion(d, dt) {
    d.ingestProgress += dt * 3.5;
    // Spiral inward toward black hole center
    const dx = this.blackHole.x - d.x;
    const dy = this.blackHole.y - d.y;
    d.x += dx * dt * 8.0 - dy * dt * 4.0;
    d.y += dy * dt * 8.0 + dx * dt * 4.0;
    d.radius = Math.max(0.1, d.radius * (1 - dt * 4.0));

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
        // Exiting through aperture into the void/black hole!
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
          this.audio.playDebrisCollision(Math.min(1.0, dot / 100));
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
      }
    }
  }

  resolveDividerCollisions(d) {
    const dx = d.x - this.center.x;
    const dy = d.y - this.center.y;
    const dist = Math.hypot(dx, dy);

    // Relative angle in ring coordinate frame
    const angleInRing = this.normalizeAngle(
      Math.atan2(dy, dx) - this.ringAngle,
    );

    for (const div of this.dividers) {
      if (dist < div.rMin - d.radius || dist > div.rMax + d.radius) continue;

      const divAngle = this.normalizeAngle(div.angle);
      let angleDiff = angleInRing - divAngle;
      while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
      while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

      const arcDist = angleDiff * dist;
      const thickness = 6; // divider wall thickness

      if (Math.abs(arcDist) < thickness * 0.5 + d.radius) {
        // Collision with divider normal
        const normalSign = arcDist > 0 ? 1 : -1;
        const worldDivAngle = divAngle + this.ringAngle;
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
          this.audio.playDebrisCollision(0.7);
          this.checkVolatileDetonation(d, Math.abs(dot));
        }
      }
    }
  }

  checkMultiplierGates(d) {
    const dx = d.x - this.center.x;
    const dy = d.y - this.center.y;
    const dist = Math.hypot(dx, dy);
    const angleInRing = this.normalizeAngle(
      Math.atan2(dy, dx) - this.ringAngle,
    );

    for (const gate of this.gates) {
      if (gate.processedDebris.has(d.id)) continue;

      const gateAngle = this.normalizeAngle(gate.angle);
      let angleDiff = angleInRing - gateAngle;
      while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
      while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

      // Check if crossing gate threshold
      if (
        Math.abs(angleDiff * dist) < 14 &&
        Math.abs(dist - gate.r) < gate.length * 0.6
      ) {
        gate.processedDebris.add(d.id);
        this.audio.playGatePass();

        if (this.onGatePassed) {
          this.onGatePassed(gate, d);
        }

        // Duplication or Addition
        let spawnCount = 0;
        if (gate.type === "MULTIPLY") {
          spawnCount = gate.value - 1;
        } else if (gate.type === "ADD") {
          spawnCount = gate.value;
        }

        for (let s = 0; s < spawnCount; s++) {
          const jitterTheta = Math.random() * Math.PI * 2;
          const jitterDist = Math.random() * (d.radius * 0.75);
          const clone = this.spawnDebris({
            x: d.x + Math.cos(jitterTheta) * jitterDist,
            y: d.y + Math.sin(jitterTheta) * jitterDist,
            vx: d.vx * 0.95 + (Math.random() - 0.5) * 15,
            vy: d.vy * 0.95 + (Math.random() - 0.5) * 15,
            type: d.type,
          });
          if (clone) {
            gate.processedDebris.add(clone.id);
          }
        }
      }
    }
  }

  checkVolatileDetonation(d, impactVelocity) {
    if (d.type !== "volatile" || impactVelocity < 180) return;

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
