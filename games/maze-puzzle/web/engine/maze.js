/**
 * Orbital Salvage: Professional Maze & Corridor Topology Engine
 * Powers a 50-Level Campaign across 5 Cosmic Sectors with curated difficulty,
 * solvable non-blocking chute geometry, concentric orbital rails, one-way flappers, and shredder gates.
 */

export class MazeLevelBuilder {
  /**
   * Automatically inspects and resolves any angular overlaps between gates, dividers,
   * deflector pins, and flappers to guarantee 100% visual and rule clarity.
   */
  static sanitizeSector(config) {
    const norm = (a) => {
      let v = a % (Math.PI * 2);
      if (v < 0) v += Math.PI * 2;
      return v;
    };

    const angDiff = (a, b) => {
      let d = Math.abs(norm(a) - norm(b));
      if (d > Math.PI) d = Math.PI * 2 - d;
      return d;
    };

    const radOverlap = (min1, max1, min2, max2) => !(max1 <= min2 || min1 >= max2);

    const MIN_CLEARANCE = 0.12 * Math.PI; // ~21.6 degrees clearance from dividers
    const MIN_GATE_CLEARANCE = 0.18 * Math.PI; // ~32.4 degrees minimum separation between gates
    const gates = config.gates || [];
    const dividers = config.dividers || [];
    const pins = config.deflectorPins || [];
    const flaps = config.oneWayFlaps || [];

    // 1. Joint Radial Slot Search for each gate considering dividers, flaps, and other gates
    for (let i = 0; i < gates.length; i++) {
      const gate = gates[i];
      const gRMin = gate.rMin !== undefined ? gate.rMin : 85;
      const gRMax = gate.rMax !== undefined ? gate.rMax : 160;

      const calcClearance = (testAng) => {
        let minD = 999;
        // Dividers
        for (const div of dividers) {
          if (config.concentricRings && (gate.isInner !== div.isInner)) continue;
          const dRMin = div.rMin !== undefined ? div.rMin : 85;
          const dRMax = div.rMax !== undefined ? div.rMax : 160;
          if (!radOverlap(gRMin, gRMax, dRMin, dRMax)) continue;
          const diff = angDiff(testAng, div.angle);
          if (diff < minD) minD = diff;
        }
        // Other gates on same radial track
        for (let j = 0; j < gates.length; j++) {
          if (i === j) continue;
          const og = gates[j];
          if (config.concentricRings && (gate.isInner !== og.isInner)) continue;
          const oRMin = og.rMin !== undefined ? og.rMin : 85;
          const oRMax = og.rMax !== undefined ? og.rMax : 160;
          if (!radOverlap(gRMin, gRMax, oRMin, oRMax)) continue;
          const diff = angDiff(testAng, og.angle);
          if (diff < MIN_GATE_CLEARANCE && diff * 0.7 < minD) {
            minD = diff * 0.7;
          }
        }
        // Flaps
        for (const flap of flaps) {
          if (config.concentricRings && (gate.isInner !== flap.isInner)) continue;
          const fRMin = flap.rMin !== undefined ? flap.rMin : 85;
          const fRMax = flap.rMax !== undefined ? flap.rMax : 160;
          if (!radOverlap(gRMin, gRMax, fRMin, fRMax)) continue;
          const diff = angDiff(testAng, flap.angle);
          if (diff < minD) minD = diff;
        }
        return minD;
      };

      let curClearance = calcClearance(gate.angle);
      if (curClearance < MIN_CLEARANCE) {
        let bestAng = gate.angle;
        let bestDist = curClearance;
        for (let delta = -0.5 * Math.PI; delta <= 0.5 * Math.PI; delta += 0.01 * Math.PI) {
          const testAng = gate.angle + delta;
          const testMin = calcClearance(testAng);
          if (testMin > bestDist) {
            bestDist = testMin;
            bestAng = testAng;
          }
        }
        gate.angle = bestAng;
      }
    }

    // 2. Strict Gate-to-Gate Separation Pass
    for (let pass = 0; pass < 3; pass++) {
      for (let i = 0; i < gates.length; i++) {
        for (let j = i + 1; j < gates.length; j++) {
          const g1 = gates[i];
          const g2 = gates[j];
          if (config.concentricRings && g1.isInner !== g2.isInner) continue;
          const r1Min = g1.rMin !== undefined ? g1.rMin : 85;
          const r1Max = g1.rMax !== undefined ? g1.rMax : 160;
          const r2Min = g2.rMin !== undefined ? g2.rMin : 85;
          const r2Max = g2.rMax !== undefined ? g2.rMax : 160;
          if (!radOverlap(r1Min, r1Max, r2Min, r2Max)) continue;

          const diff = angDiff(g1.angle, g2.angle);
          if (diff < MIN_GATE_CLEARANCE) {
            let bestAng = g2.angle;
            let bestDist = -1;
            for (let dir of [1, -1]) {
              for (let step = 0.04 * Math.PI; step <= 0.5 * Math.PI; step += 0.02 * Math.PI) {
                const testAng = g1.angle + dir * (MIN_GATE_CLEARANCE + step);
                let minDivD = 999;
                for (const div of dividers) {
                  if (config.concentricRings && (g2.isInner !== div.isInner)) continue;
                  const dRMin = div.rMin !== undefined ? div.rMin : 85;
                  const dRMax = div.rMax !== undefined ? div.rMax : 160;
                  if (!radOverlap(r2Min, r2Max, dRMin, dRMax)) continue;
                  const dDiff = angDiff(testAng, div.angle);
                  if (dDiff < minDivD) minDivD = dDiff;
                }
                if (minDivD >= MIN_CLEARANCE) {
                  const sep1 = angDiff(testAng, g1.angle);
                  if (sep1 > bestDist) {
                    bestDist = sep1;
                    bestAng = testAng;
                  }
                }
              }
            }
            g2.angle = bestAng;
          }
        }
      }
    }

    // 3. Resolve Gate-Pin overlaps
    for (const pin of pins) {
      for (const gate of gates) {
        if (config.concentricRings && (pin.isInner !== gate.isInner)) continue;
        const gRMin = gate.rMin !== undefined ? gate.rMin : 85;
        const gRMax = gate.rMax !== undefined ? gate.rMax : 160;
        if (pin.r < gRMin - 12 || pin.r > gRMax + 12) continue;

        const diff = angDiff(pin.angle, gate.angle);
        if (diff < MIN_CLEARANCE) {
          let bestAng = pin.angle;
          let bestDist = diff;
          for (let delta = -0.35 * Math.PI; delta <= 0.35 * Math.PI; delta += 0.01 * Math.PI) {
            const testAng = pin.angle + delta;
            const testDiff = angDiff(testAng, gate.angle);
            if (testDiff > bestDist) {
              bestDist = testDiff;
              bestAng = testAng;
            }
          }
          pin.angle = bestAng;
        }
      }
    }

    return config;
  }

  /**
   * Returns all 50 levels structured into 5 progressive cosmic worlds.
   */
  static getSectors() {
    const levels = [];
    for (let i = 1; i <= 50; i++) {
      levels.push(this.buildLevel(i));
    }
    return levels;
  }

  static getSectorWorlds() {
    return [
      {
        id: "world_1",
        worldIndex: 1,
        title: "Bubblegum Belt",
        tagline: "Orbital Gravity Basics & Clean Flow",
        icon: "🍬",
        color: "#f472b6",
        levelRange: [1, 10],
      },
      {
        id: "world_2",
        worldIndex: 2,
        title: "Cheese Moon Chutes",
        tagline: "Switchbacks & One-Way Flappers",
        icon: "🧀",
        color: "#facc15",
        levelRange: [11, 20],
      },
      {
        id: "world_3",
        worldIndex: 3,
        title: "Cyber Donut Rings",
        tagline: "Concentric Planetary Dual-Track Gearing",
        icon: "🍩",
        color: "#38bdf8",
        levelRange: [21, 30],
      },
      {
        id: "world_4",
        worldIndex: 4,
        title: "Nebula Pinball",
        tagline: "Pinball Bumpers, Forking Gauntlets & Boost",
        icon: "🌌",
        color: "#c084fc",
        levelRange: [31, 40],
      },
      {
        id: "world_5",
        worldIndex: 5,
        title: "Singularity Horizon",
        tagline: "Master Dual-Ring Orbital Gauntlets",
        icon: "🕳️",
        color: "#4ade80",
        levelRange: [41, 50],
      },
    ];
  }

  /**
   * Builds a specific curated level configuration for Level 1 to 50.
   */
  static buildLevel(levelNumber) {
    const worldIndex = Math.ceil(levelNumber / 10);
    const subLevel = ((levelNumber - 1) % 10) + 1; // 1 to 10
    let config;

    if (worldIndex === 1) {
      config = this.buildWorld1Level(levelNumber, subLevel);
    } else if (worldIndex === 2) {
      config = this.buildWorld2Level(levelNumber, subLevel);
    } else if (worldIndex === 3) {
      config = this.buildWorld3Level(levelNumber, subLevel);
    } else if (worldIndex === 4) {
      config = this.buildWorld4Level(levelNumber, subLevel);
    } else {
      config = this.buildWorld5Level(levelNumber, subLevel);
    }

    return this.sanitizeSector(config);
  }

  // =========================================================================
  // WORLD 1: BUBBLEGUM BELT (Levels 1 - 10)
  // Theme: Funnel Baffles, introductory shredders, learning tilt & flow.
  // =========================================================================
  static buildWorld1Level(levelNum, sub) {
    const ringRadius = 155;
    const innerRadius = 82;
    const quota = 20 + (sub - 1) * 6;
    const initialDebrisCount = 10 + sub;
    const apertureAngle = Math.PI * (0.32 - sub * 0.005);

    // Dividers with staggered switchback shelves that catch sliding debris
    const dividers = [
      { rMin: 85, rMax: 155, angle: -Math.PI * 0.5 }, // Top barrier separating left/right
      { rMin: 85, rMax: 125, angle: 0 },              // Right retaining shelf (catches and holds right-sliding debris)
      { rMin: 115, rMax: 155, angle: Math.PI * (0.32 + sub * 0.005) }, // Lower right aperture baffle
      { rMin: 85, rMax: 125, angle: Math.PI },        // Left retaining shelf (catches and holds left-sliding debris)
      { rMin: 115, rMax: 155, angle: Math.PI * (0.68 - sub * 0.005) }, // Lower left aperture baffle
    ];

    if (sub >= 4) {
      dividers.push({ rMin: 115, rMax: 155, angle: -Math.PI * 0.28 });
    }
    if (sub >= 7) {
      dividers.push({ rMin: 115, rMax: 155, angle: Math.PI * 1.28 });
    }

    // Well-spaced gates with ample clearance from all walls and hoppers
    const gates = [
      {
        type: "MULTIPLY",
        value: 2,
        angle: -Math.PI * 0.12,
        rMin: 85,
        rMax: 155,
        color: "#facc15",
      },
      {
        type: "ADD",
        value: 3 + Math.floor(sub * 0.5),
        angle: Math.PI * 0.20,
        rMin: 85,
        rMax: 155,
        color: "#38bdf8",
      },
      {
        type: "MULTIPLY",
        value: sub >= 5 ? 3 : 2,
        angle: Math.PI * 0.85,
        rMin: 85,
        rMax: 155,
        color: "#f472b6",
      },
    ];

    return {
      id: `level_${levelNum}`,
      levelNumber: levelNum,
      worldIndex: 1,
      name: `Bubblegum Chute ${sub}`,
      code: `LVL ${levelNum}`,
      description: `Tilt the centrifuge gently to funnel candy scrap into Nibbles!`,
      quota,
      initialDebrisCount,
      initialDebrisTypes: sub >= 5 ? ["titanium", "solar"] : ["titanium"],
      ringRadius,
      corridorWidth: 36,
      innerRadius,
      apertureAngle,
      dividers,
      gates,
    };
  }

  // =========================================================================
  // WORLD 2: CHEESE MOON CHUTES (Levels 11 - 20)
  // Theme: Switchback Zigzags, One-Way Flappers, handling volatile batteries.
  // =========================================================================
  static buildWorld2Level(levelNum, sub) {
    const ringRadius = 162;
    const innerRadius = 86;
    const quota = 65 + (sub - 1) * 7;
    const initialDebrisCount = 14 + sub;
    const apertureAngle = Math.PI * 0.28;

    // Staggered switchback baffles
    const dividers = [
      { rMin: 86, rMax: 162, angle: -Math.PI * 0.5 }, // Top barrier
      { rMin: 86, rMax: 132, angle: -Math.PI * 0.20 },
      { rMin: 120, rMax: 162, angle: Math.PI * 0.12 },
      { rMin: 86, rMax: 132, angle: Math.PI * 0.42 },
      { rMin: 86, rMax: 132, angle: Math.PI * 1.20 },
      { rMin: 120, rMax: 162, angle: Math.PI * 0.88 },
      { rMin: 86, rMax: 132, angle: Math.PI * 0.60 },
    ];

    if (sub >= 5) {
      dividers.push({ rMin: 120, rMax: 162, angle: -Math.PI * 0.48 });
      dividers.push({ rMin: 120, rMax: 162, angle: Math.PI * 1.48 });
    }

    const gates = [
      {
        type: "MULTIPLY",
        value: 3,
        angle: -Math.PI * 0.35,
        rMin: 120,
        rMax: 162,
        color: "#f472b6",
      },
      {
        type: "ADD",
        value: 5 + Math.floor(sub * 0.6),
        angle: -Math.PI * 0.04,
        rMin: 86,
        rMax: 125,
        color: "#38bdf8",
      },
      {
        type: "MULTIPLY",
        value: 2,
        angle: Math.PI * 0.27,
        rMin: 120,
        rMax: 162,
        color: "#4ade80",
      },
      {
        type: "MULTIPLY",
        value: sub >= 6 ? 3 : 2,
        angle: Math.PI * 1.04,
        rMin: 86,
        rMax: 162,
        color: "#facc15",
      },
    ];

    // Introduce Mechanical One-Way Gravity Flapper starting from Level 14
    const oneWayFlaps = [];
    if (sub >= 4) {
      oneWayFlaps.push({
        rMin: 86,
        rMax: 132,
        angle: Math.PI * 0.25,
        maxSwing: Math.PI * 0.35,
        normalSign: 1, // Opens downward towards collection funnel
      });
    }
    if (sub >= 7) {
      oneWayFlaps.push({
        rMin: 86,
        rMax: 132,
        angle: Math.PI * 0.75,
        maxSwing: Math.PI * 0.35,
        normalSign: -1,
      });
    }

    return {
      id: `level_${levelNum}`,
      levelNumber: levelNum,
      worldIndex: 2,
      name: `Moon Chute ${sub}`,
      code: `LVL ${levelNum}`,
      description: `Watch the spicy batteries! Rock back and forth through switchbacks and one-way valves!`,
      quota,
      initialDebrisCount,
      initialDebrisTypes: ["titanium", "solar", "volatile"],
      ringRadius,
      corridorWidth: 38,
      innerRadius,
      apertureAngle,
      dividers,
      gates,
      oneWayFlaps,
    };
  }

  // =========================================================================
  // WORLD 3: CYBER DONUT RINGS (Levels 21 - 30)
  // Theme: Concentric Planetary Dual-Track Gearing & Orbital Alignment.
  // =========================================================================
  static buildWorld3Level(levelNum, sub) {
    const ringRadius = 172;
    const innerRadius = 90;
    const trackRadius = 131;
    const quota = 105 + (sub - 1) * 9;
    const initialDebrisCount = 16 + sub;
    const apertureAngle = Math.PI * 0.26;

    // Track wall separating inner and outer orbit with transfer gateway
    const gapAngle = sub % 2 === 1 ? -Math.PI * 0.5 : 0;
    const trackWalls = [
      {
        r: trackRadius,
        gapAngle: gapAngle,
        gapSize: Math.PI * (0.44 - sub * 0.01),
      },
    ];

    // Inner track dividers (rotate with inner planetary gear)
    const dividers = [
      { rMin: 90, rMax: 131, angle: -Math.PI * 0.15, isInner: true },
      { rMin: 90, rMax: 131, angle: Math.PI * 0.45, isInner: true },
      { rMin: 90, rMax: 131, angle: Math.PI * 1.15, isInner: true },

      // Outer track dividers (rotate with outer player ring)
      { rMin: 131, rMax: 172, angle: -Math.PI * 0.15, isInner: false },
      { rMin: 131, rMax: 172, angle: Math.PI * 0.25, isInner: false },
      { rMin: 131, rMax: 172, angle: Math.PI * 0.75, isInner: false },
      { rMin: 131, rMax: 172, angle: Math.PI * 1.15, isInner: false },
    ];

    if (sub >= 6) {
      dividers.push({ rMin: 131, rMax: 172, angle: -Math.PI * 0.4, isInner: false });
    }

    const gates = [
      {
        type: "MULTIPLY",
        value: 2,
        angle: Math.PI * 0.15,
        rMin: 90,
        rMax: 131,
        isInner: true,
        color: "#38bdf8",
      },
      {
        type: "ADD",
        value: 8 + sub,
        angle: 0.04,
        rMin: 131,
        rMax: 172,
        isInner: false,
        color: "#facc15",
      },
      {
        type: "MULTIPLY",
        value: 3,
        angle: Math.PI * 0.95,
        rMin: 131,
        rMax: 172,
        isInner: false,
        color: "#c084fc",
      },
      {
        type: "MULTIPLY",
        value: 2,
        angle: -Math.PI * 0.28,
        rMin: 131,
        rMax: 172,
        isInner: false,
        color: "#4ade80",
      },
    ];

    const deflectorPins = [
      {
        r: 152,
        angle: Math.PI * 0.50,
        radius: 9.5,
        restitution: 1.35,
        isInner: false,
        color: "#38bdf8",
        accent: "#0284c7",
      },
      {
        r: 110,
        angle: -Math.PI * 0.45,
        radius: 9,
        restitution: 1.3,
        isInner: true,
        color: "#facc15",
        accent: "#eab308",
      },
    ];

    return {
      id: `level_${levelNum}`,
      levelNumber: levelNum,
      worldIndex: 3,
      name: `Cyber Orbit ${sub}`,
      code: `LVL ${levelNum}`,
      description: `Planetary Gears! Inner and outer tracks counter-rotate. Align airlocks to launch scrap!`,
      quota,
      initialDebrisCount,
      initialDebrisTypes: ["titanium", "solar", "magnetic"],
      ringRadius,
      corridorWidth: 41,
      innerRadius,
      apertureAngle,
      concentricRings: true,
      gearRatio: -1.0,
      trackWalls,
      dividers,
      gates,
      deflectorPins,
    };
  }

  // =========================================================================
  // WORLD 4: NEON NEBULA (Levels 31 - 40)
  // Theme: Forking divergent paths, risk/reward choices & deflector channels.
  // =========================================================================
  static buildWorld4Level(levelNum, sub) {
    const ringRadius = 176;
    const innerRadius = 92;
    const quota = 155 + (sub - 1) * 11;
    const initialDebrisCount = 18 + sub;
    const apertureAngle = Math.PI * 0.25;

    // Split divergent paths: Left branch safe, Right branch high-risk high-reward
    const dividers = [
      { rMin: 92, rMax: 176, angle: -Math.PI * 0.5 }, // Center splitter
      // Safe left branch baffles
      { rMin: 92, rMax: 145, angle: Math.PI * 1.25 },
      { rMin: 125, rMax: 176, angle: Math.PI * 0.92 },
      { rMin: 92, rMax: 145, angle: Math.PI * 0.60 },
      // High-yield right branch baffles
      { rMin: 120, rMax: 176, angle: -Math.PI * 0.26 },
      { rMin: 92, rMax: 148, angle: Math.PI * 0.06 },
      { rMin: 125, rMax: 176, angle: Math.PI * 0.38 },
    ];

    if (sub >= 4) {
      dividers.push({ rMin: 92, rMax: 140, angle: -Math.PI * 0.40 });
    }
    if (sub >= 7) {
      dividers.push({ rMin: 92, rMax: 140, angle: Math.PI * 1.42 });
    }

    const gates = [
      // Safe Left: High addition
      {
        type: "ADD",
        value: 10 + sub,
        angle: Math.PI * 1.08,
        rMin: 92,
        rMax: 176,
        color: "#38bdf8",
      },
      // Risky Right: Mega multiplication
      {
        type: "MULTIPLY",
        value: sub >= 5 ? 4 : 3,
        angle: -Math.PI * 0.10,
        rMin: 92,
        rMax: 176,
        color: "#f472b6",
      },
      {
        type: "MULTIPLY",
        value: 2,
        angle: Math.PI * 0.22,
        rMin: 92,
        rMax: 176,
        color: "#facc15",
      },
      {
        type: "MULTIPLY",
        value: 2,
        angle: Math.PI * 0.76,
        rMin: 92,
        rMax: 176,
        color: "#4ade80",
      },
    ];

    // High-energy pinball deflector bumpers placed with strict clearances
    const deflectorPins = [
      {
        r: 135,
        angle: -Math.PI * 0.38,
        radius: 10.5,
        restitution: 1.4,
        color: "#ec4899",
        accent: "#f43f5e",
      },
      {
        r: 135,
        angle: Math.PI * 0.54,
        radius: 10.5,
        restitution: 1.4,
        color: "#facc15",
        accent: "#eab308",
      },
      {
        r: 110,
        angle: Math.PI * 1.42,
        radius: 9.5,
        restitution: 1.35,
        color: "#06b6d4",
        accent: "#0891b2",
      },
    ];

    if (sub >= 4) {
      deflectorPins.push({
        r: 155,
        angle: -Math.PI * 0.48,
        radius: 11,
        restitution: 1.45,
        color: "#a855f7",
        accent: "#9333ea",
      });
    }

    if (sub >= 3) {
      gates.push({
        type: "BOOST",
        value: 160,
        angle: Math.PI * 1.40,
        rMin: 92,
        rMax: 130, // Inner return lane boost
        color: "#84cc16",
      });
    }

    const oneWayFlaps = [
      {
        rMin: 92,
        rMax: 145,
        angle: Math.PI * 0.76,
        maxSwing: Math.PI * 0.35,
        normalSign: 1,
      },
    ];

    return {
      id: `level_${levelNum}`,
      levelNumber: levelNum,
      worldIndex: 4,
      name: `Nebula Pinball ${sub}`,
      code: `LVL ${levelNum}`,
      description: `Pinball bumpers & forking routes! Ricochet scrap through multipliers and slingshot gates!`,
      quota,
      initialDebrisCount,
      initialDebrisTypes: ["titanium", "solar", "volatile", "magnetic"],
      ringRadius,
      corridorWidth: 42,
      innerRadius,
      apertureAngle,
      dividers,
      gates,
      deflectorPins,
      oneWayFlaps,
    };
  }

  // =========================================================================
  // WORLD 5: SINGULARITY HORIZON (Levels 41 - 50)
  // Theme: Master orbital gauntlets, concentric planetary gears, grand finale.
  // =========================================================================
  static buildWorld5Level(levelNum, sub) {
    const ringRadius = 180;
    const innerRadius = 94;
    const trackRadius = 137;
    const quota = 210 + (sub - 1) * 16;
    const initialDebrisCount = 20 + sub;
    const apertureAngle = Math.PI * 0.24;

    // Dual tracks with multi-stage release
    const trackWalls = [
      {
        r: trackRadius,
        gapAngle: -Math.PI * 0.5 + (sub % 2 === 0 ? 0.3 : -0.3),
        gapSize: Math.PI * 0.4,
      },
    ];

    const dividers = [
      // Inner staging spokes (rotate with inner planetary ring)
      { rMin: 94, rMax: 137, angle: -Math.PI * 0.12, isInner: true },
      { rMin: 94, rMax: 137, angle: Math.PI * 0.42, isInner: true },
      { rMin: 94, rMax: 137, angle: Math.PI * 0.82, isInner: true },
      { rMin: 94, rMax: 137, angle: Math.PI * 1.22, isInner: true },

      // Outer release maze (rotate with outer ring)
      { rMin: 137, rMax: 180, angle: -Math.PI * 0.32, isInner: false },
      { rMin: 137, rMax: 180, angle: 0, isInner: false },
      { rMin: 137, rMax: 180, angle: Math.PI * 0.24, isInner: false },
      { rMin: 137, rMax: 180, angle: Math.PI * 0.76, isInner: false },
      { rMin: 137, rMax: 180, angle: Math.PI * 1.06, isInner: false },
      { rMin: 137, rMax: 180, angle: Math.PI * 1.36, isInner: false },
    ];

    const gates = [
      {
        type: "MULTIPLY",
        value: 3,
        angle: Math.PI * 0.15,
        rMin: 94,
        rMax: 137,
        isInner: true,
        color: "#c084fc",
      },
      {
        type: "ADD",
        value: 12 + sub,
        angle: -Math.PI * 0.16,
        rMin: 137,
        rMax: 180,
        isInner: false,
        color: "#facc15",
      },
      {
        type: "MULTIPLY",
        value: 3,
        angle: Math.PI * 0.90,
        rMin: 137,
        rMax: 180,
        isInner: false,
        color: "#f472b6",
      },
      {
        type: "MULTIPLY",
        value: 4,
        angle: -Math.PI * 0.48,
        rMin: 137,
        rMax: 180,
        isInner: false,
        color: "#4ade80",
      },
    ];

    const isFinale = levelNum === 50;

    // Grand gauntlet deflector pins with clear angular margins
    const deflectorPins = [
      {
        r: 115,
        angle: Math.PI * 0.35,
        radius: 10,
        restitution: 1.4,
        isInner: true,
        color: "#ec4899",
        accent: "#f43f5e",
      },
      {
        r: 160,
        angle: 0.12,
        radius: 11,
        restitution: 1.45,
        isInner: false,
        color: "#facc15",
        accent: "#eab308",
      },
      {
        r: 160,
        angle: Math.PI * 1.22,
        radius: 10,
        restitution: 1.4,
        isInner: false,
        color: "#06b6d4",
        accent: "#0891b2",
      },
      {
        r: 115,
        angle: Math.PI * 1.02,
        radius: 10,
        restitution: 1.35,
        isInner: true,
        color: "#a855f7",
        accent: "#9333ea",
      },
    ];

    if (sub >= 5) {
      deflectorPins.push({
        r: 158,
        angle: Math.PI * 0.46,
        radius: 12,
        restitution: 1.5,
        isInner: false,
        color: "#f43f5e",
        accent: "#e11d48",
      });
    }

    if (sub >= 2) {
      gates.push({
        type: "BOOST",
        value: 180,
        angle: -Math.PI * 0.48,
        rMin: 94,
        rMax: 137, // Inner lane booster
        isInner: true,
        color: "#84cc16",
      });
    }

    const oneWayFlaps = [
      {
        rMin: 137,
        rMax: 180,
        angle: Math.PI * 0.24,
        maxSwing: Math.PI * 0.35,
        normalSign: 1,
        isInner: false,
      },
      {
        rMin: 94,
        rMax: 137,
        angle: Math.PI * 0.82,
        maxSwing: Math.PI * 0.35,
        normalSign: 1,
        isInner: true,
      },
    ];

    return {
      id: `level_${levelNum}`,
      levelNumber: levelNum,
      worldIndex: 5,
      name: isFinale ? "The Kessler Singularity" : `Horizon Sector ${sub}`,
      code: `LVL ${levelNum}`,
      description: isFinale
        ? "⭐ GRAND FINALE! Clear humanity's greatest orbital debris cascade to save Earth!"
        : "Master planetary gear gauntlet! Route high-density debris through multi-stage shredders!",
      quota,
      initialDebrisCount,
      initialDebrisTypes: ["titanium", "solar", "volatile", "magnetic"],
      ringRadius,
      corridorWidth: 43,
      innerRadius,
      apertureAngle,
      concentricRings: true,
      gearRatio: -1.25,
      trackWalls,
      dividers,
      gates,
      deflectorPins,
      oneWayFlaps,
      isFinale,
    };
  }

  // =========================================================================
  // PROCEDURAL DAILY SHIFT GENERATOR (Endless retention contract)
  // =========================================================================
  static buildDailyShift(dailyInfo) {
    const seed = dailyInfo.seed || 12345;
    let s = seed;
    const rnd = () => {
      s = (s * 1664525 + 1013904223) % 4294967296;
      return s / 4294967296;
    };

    const numDividers = 6;
    const dividers = [];
    const step = (Math.PI * 2) / numDividers;

    // Aperture exclusion window around bottom PI/2
    const apCenter = Math.PI * 0.5;
    const apHalfWidth = 0.22 * Math.PI;

    // Defined gate angles at corridor midpoints
    const gateAngles = [-Math.PI * 0.5, step * 0.5, step * 2.5];

    for (let i = 0; i < numDividers; i++) {
      const ang = i * step + (rnd() - 0.5) * 0.08;
      let diff = Math.abs(ang - apCenter);
      while (diff > Math.PI) diff = Math.PI * 2 - diff;
      if (diff < apHalfWidth) continue;

      // Skip if too close to planned gates
      let tooCloseToGate = false;
      for (const ga of gateAngles) {
        let gDiff = Math.abs(ang - ga);
        while (gDiff > Math.PI) gDiff = Math.PI * 2 - gDiff;
        if (gDiff < 0.16 * Math.PI) {
          tooCloseToGate = true;
          break;
        }
      }
      if (tooCloseToGate) continue;

      const mid = 125;
      if (rnd() > 0.5) {
        dividers.push({ rMin: 88, rMax: mid, angle: ang });
      } else {
        dividers.push({ rMin: mid, rMax: 165, angle: ang });
      }
    }

    const gates = [
      {
        type: "MULTIPLY",
        value: 2 + Math.floor(rnd() * 2),
        angle: gateAngles[1], // ~0.167pi
        rMin: 88,
        rMax: 165,
        color: "#facc15",
      },
      {
        type: "ADD",
        value: 5 + Math.floor(rnd() * 5),
        angle: gateAngles[2], // ~0.833pi
        rMin: 88,
        rMax: 165,
        color: "#38bdf8",
      },
      {
        type: "MULTIPLY",
        value: 3,
        angle: gateAngles[0], // -0.5pi
        rMin: 88,
        rMax: 165,
        color: "#f472b6",
      },
    ];

    const oneWayFlaps = [
      {
        rMin: 88,
        rMax: 135,
        angle: -0.15 * Math.PI,
        maxSwing: Math.PI * 0.35,
        normalSign: 1,
      },
    ];

    const config = {
      id: "daily_shift",
      name: dailyInfo.title,
      code: "FIESTA",
      description: `Daily Fun Contract! ${dailyInfo.hazardModifier}`,
      quota: dailyInfo.quota,
      initialDebrisCount: 15,
      initialDebrisTypes: ["titanium", "solar", "volatile"],
      ringRadius: 165,
      corridorWidth: 38,
      innerRadius: 89,
      apertureAngle: Math.PI * 0.28,
      dividers,
      gates,
      oneWayFlaps,
      isDaily: true,
      rewardCredits: dailyInfo.rewardCredits,
    };

    return this.sanitizeSector(config);
  }
}
