/**
 * Orbital Salvage: Professional Maze & Corridor Topology Engine
 * Powers a 50-Level Campaign across 5 Cosmic Sectors with curated difficulty,
 * solvable non-blocking chute geometry, concentric orbital rails, and shredder gates.
 */

export class MazeLevelBuilder {
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
        tagline: "Orbital Gravity Basics & Flow",
        icon: "🍬",
        color: "#f472b6",
        levelRange: [1, 10],
      },
      {
        id: "world_2",
        worldIndex: 2,
        title: "Cheese Moon Chutes",
        tagline: "Switchbacks & Volatile Scrap",
        icon: "🧀",
        color: "#facc15",
        levelRange: [11, 20],
      },
      {
        id: "world_3",
        worldIndex: 3,
        title: "Cyber Donut Rings",
        tagline: "Concentric Dual-Track Transfers",
        icon: "🍩",
        color: "#38bdf8",
        levelRange: [21, 30],
      },
      {
        id: "world_4",
        worldIndex: 4,
        title: "Nebula Pinball",
        tagline: "Pinball Bumpers & Boost Slingshots",
        icon: "🌌",
        color: "#c084fc",
        levelRange: [31, 40],
      },
      {
        id: "world_5",
        worldIndex: 5,
        title: "Singularity Horizon",
        tagline: "Master Orbital Gauntlets",
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

    if (worldIndex === 1) {
      return this.buildWorld1Level(levelNumber, subLevel);
    } else if (worldIndex === 2) {
      return this.buildWorld2Level(levelNumber, subLevel);
    } else if (worldIndex === 3) {
      return this.buildWorld3Level(levelNumber, subLevel);
    } else if (worldIndex === 4) {
      return this.buildWorld4Level(levelNumber, subLevel);
    } else {
      return this.buildWorld5Level(levelNumber, subLevel);
    }
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

    // Progressive divider complexity
    const dividers = [
      { rMin: 85, rMax: 155, angle: -Math.PI * 0.5 }, // Top barrier separating left/right
      { rMin: 115, rMax: 155, angle: 0 },
      { rMin: 85, rMax: 125, angle: Math.PI * (0.16 + sub * 0.01) },
      { rMin: 115, rMax: 155, angle: Math.PI },
      { rMin: 85, rMax: 125, angle: Math.PI * (0.84 - sub * 0.01) },
    ];

    if (sub >= 4) {
      dividers.push({ rMin: 120, rMax: 155, angle: -Math.PI * 0.2 });
    }
    if (sub >= 7) {
      dividers.push({ rMin: 120, rMax: 155, angle: Math.PI * 1.2 });
    }

    const gates = [
      {
        type: "MULTIPLY",
        value: 2,
        angle: -Math.PI * 0.15,
        rMin: 85,
        rMax: 155,
        color: "#facc15",
      },
      {
        type: "ADD",
        value: 3 + Math.floor(sub * 0.5),
        angle: Math.PI * 0.15,
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
  // Theme: Switchback Zigzags, pacing rotation, handling volatile batteries.
  // =========================================================================
  static buildWorld2Level(levelNum, sub) {
    const ringRadius = 162;
    const innerRadius = 86;
    const quota = 65 + (sub - 1) * 7;
    const initialDebrisCount = 14 + sub;
    const apertureAngle = Math.PI * 0.28;

    // Switchback baffles: alternating inner/outer teeth requiring back-and-forth rocking
    const dividers = [
      { rMin: 86, rMax: 162, angle: -Math.PI * 0.5 }, // Top barrier
      { rMin: 86, rMax: 135, angle: -Math.PI * 0.15 },
      { rMin: 118, rMax: 162, angle: Math.PI * 0.08 },
      { rMin: 86, rMax: 135, angle: Math.PI * 0.24 }, // Steers toward bottom right
      { rMin: 86, rMax: 135, angle: Math.PI * 1.15 },
      { rMin: 118, rMax: 162, angle: Math.PI * 0.92 },
      { rMin: 86, rMax: 135, angle: Math.PI * 0.76 }, // Steers toward bottom left
    ];

    if (sub >= 5) {
      dividers.push({ rMin: 120, rMax: 162, angle: -Math.PI * 0.35 });
      dividers.push({ rMin: 120, rMax: 162, angle: Math.PI * 1.35 });
    }

    const gates = [
      {
        type: "MULTIPLY",
        value: 3,
        angle: 0,
        rMin: 86,
        rMax: 162,
        color: "#f472b6",
      },
      {
        type: "ADD",
        value: 5 + Math.floor(sub * 0.6),
        angle: Math.PI * 0.2,
        rMin: 86,
        rMax: 162,
        color: "#38bdf8",
      },
      {
        type: "MULTIPLY",
        value: 2,
        angle: Math.PI * 0.8,
        rMin: 86,
        rMax: 162,
        color: "#4ade80",
      },
      {
        type: "MULTIPLY",
        value: sub >= 6 ? 3 : 2,
        angle: Math.PI * 1.0,
        rMin: 86,
        rMax: 162,
        color: "#facc15",
      },
    ];

    return {
      id: `level_${levelNum}`,
      levelNumber: levelNum,
      worldIndex: 2,
      name: `Moon Chute ${sub}`,
      code: `LVL ${levelNum}`,
      description: `Watch the spicy batteries! Rock back and forth through the lunar switchbacks!`,
      quota,
      initialDebrisCount,
      initialDebrisTypes: ["titanium", "solar", "volatile"],
      ringRadius,
      corridorWidth: 38,
      innerRadius,
      apertureAngle,
      dividers,
      gates,
    };
  }

  // =========================================================================
  // WORLD 3: CYBER DONUT RINGS (Levels 21 - 30)
  // Theme: Dual concentric orbital tracks with transfer gateways.
  // =========================================================================
  static buildWorld3Level(levelNum, sub) {
    const ringRadius = 172;
    const innerRadius = 90;
    const quota = 105 + (sub - 1) * 9;
    const initialDebrisCount = 16 + sub;
    const apertureAngle = Math.PI * 0.26;

    // Track wall separating inner and outer orbit with transfer gateway
    const gapAngle = sub % 2 === 1 ? -Math.PI * 0.5 : 0;
    const trackWalls = [
      {
        r: 131,
        gapAngle: gapAngle,
        gapSize: Math.PI * (0.44 - sub * 0.01),
      },
    ];

    const dividers = [
      // Inner track guide baffles
      { rMin: 90, rMax: 131, angle: Math.PI * 0.25 },
      { rMin: 90, rMax: 131, angle: Math.PI * 0.75 },
      { rMin: 90, rMax: 131, angle: Math.PI * 1.25 },
      // Outer track guide baffles directing to bottom exit
      { rMin: 131, rMax: 172, angle: -Math.PI * 0.15 },
      { rMin: 131, rMax: 172, angle: Math.PI * 0.22 },
      { rMin: 131, rMax: 172, angle: Math.PI * 0.78 },
      { rMin: 131, rMax: 172, angle: Math.PI * 1.15 },
    ];

    if (sub >= 6) {
      dividers.push({ rMin: 131, rMax: 172, angle: -Math.PI * 0.4 });
    }

    const gates = [
      {
        type: "MULTIPLY",
        value: 2,
        angle: Math.PI * 0.5,
        rMin: 90,
        rMax: 131,
        color: "#38bdf8",
      },
      {
        type: "ADD",
        value: 8 + sub,
        angle: 0,
        rMin: 131,
        rMax: 172,
        color: "#facc15",
      },
      {
        type: "MULTIPLY",
        value: 3,
        angle: Math.PI,
        rMin: 131,
        rMax: 172,
        color: "#c084fc",
      },
      {
        type: "MULTIPLY",
        value: 2,
        angle: -Math.PI * 0.2,
        rMin: 131,
        rMax: 172,
        color: "#4ade80",
      },
    ];

    const deflectorPins = [
      {
        r: 152,
        angle: Math.PI * 0.5,
        radius: 9.5,
        restitution: 1.35,
        color: "#38bdf8",
        accent: "#0284c7",
      },
      {
        r: 110,
        angle: -Math.PI * 0.2,
        radius: 9,
        restitution: 1.3,
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
      description: `Dual tracks! Transfer scrap from inner orbit into outer launch channels!`,
      quota,
      initialDebrisCount,
      initialDebrisTypes: ["titanium", "solar", "magnetic"],
      ringRadius,
      corridorWidth: 41,
      innerRadius,
      apertureAngle,
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

    // Split divergent paths: Left branch is safe with ADD gates;
    // Right branch is narrower with MEGA MULTIPLY and volatile battery hazards!
    const dividers = [
      { rMin: 92, rMax: 176, angle: -Math.PI * 0.5 }, // Center splitter
      // Safe left branch baffles
      { rMin: 92, rMax: 145, angle: Math.PI * 1.15 },
      { rMin: 125, rMax: 176, angle: Math.PI * 0.95 },
      { rMin: 92, rMax: 145, angle: Math.PI * 0.78 },
      // High-yield right branch baffles
      { rMin: 120, rMax: 176, angle: -Math.PI * 0.15 },
      { rMin: 92, rMax: 148, angle: Math.PI * 0.08 },
      { rMin: 125, rMax: 176, angle: Math.PI * 0.22 },
    ];

    if (sub >= 4) {
      dividers.push({ rMin: 92, rMax: 140, angle: -Math.PI * 0.3 });
    }
    if (sub >= 7) {
      dividers.push({ rMin: 92, rMax: 140, angle: Math.PI * 1.3 });
    }

    const gates = [
      // Safe Left: High addition
      {
        type: "ADD",
        value: 10 + sub,
        angle: Math.PI * 1.05,
        rMin: 92,
        rMax: 176,
        color: "#38bdf8",
      },
      // Risky Right: Mega multiplication
      {
        type: "MULTIPLY",
        value: sub >= 5 ? 4 : 3,
        angle: 0,
        rMin: 92,
        rMax: 176,
        color: "#f472b6",
      },
      {
        type: "MULTIPLY",
        value: 2,
        angle: Math.PI * 0.2,
        rMin: 92,
        rMax: 176,
        color: "#facc15",
      },
      {
        type: "MULTIPLY",
        value: 2,
        angle: Math.PI * 0.8,
        rMin: 92,
        rMax: 176,
        color: "#4ade80",
      },
    ];

    // High-energy pinball deflector bumpers placed along chutes!
    const deflectorPins = [
      {
        r: 135,
        angle: -Math.PI * 0.35,
        radius: 10.5,
        restitution: 1.4,
        color: "#ec4899",
        accent: "#f43f5e",
      },
      {
        r: 135,
        angle: Math.PI * 0.12,
        radius: 10.5,
        restitution: 1.4,
        color: "#facc15",
        accent: "#eab308",
      },
      {
        r: 110,
        angle: Math.PI * 1.0,
        radius: 9.5,
        restitution: 1.35,
        color: "#06b6d4",
        accent: "#0891b2",
      },
    ];

    if (sub >= 4) {
      deflectorPins.push({
        r: 155,
        angle: Math.PI * 0.5,
        radius: 11,
        restitution: 1.45,
        color: "#a855f7",
        accent: "#9333ea",
      });
    }
    if (sub >= 7) {
      deflectorPins.push({
        r: 115,
        angle: -Math.PI * 0.05,
        radius: 10,
        restitution: 1.4,
        color: "#f97316",
        accent: "#ea580c",
      });
    }

    if (sub >= 3) {
      gates.push({
        type: "BOOST",
        value: 160,
        angle: Math.PI * 0.5,
        rMin: 92,
        rMax: 176,
        color: "#84cc16",
      });
    }

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
    };
  }

  // =========================================================================
  // WORLD 5: SINGULARITY HORIZON (Levels 41 - 50)
  // Theme: Master orbital gauntlets, dual tracks, grand finale.
  // =========================================================================
  static buildWorld5Level(levelNum, sub) {
    const ringRadius = 180;
    const innerRadius = 94;
    const quota = 210 + (sub - 1) * 16; // Level 50 quota = 354!
    const initialDebrisCount = 20 + sub;
    const apertureAngle = Math.PI * 0.24;

    // Dual tracks with multi-stage release
    const trackWalls = [
      {
        r: 137,
        gapAngle: -Math.PI * 0.5 + (sub % 2 === 0 ? 0.3 : -0.3),
        gapSize: Math.PI * 0.4,
      },
    ];

    const dividers = [
      // Inner staging spokes
      { rMin: 94, rMax: 137, angle: -Math.PI * 0.1 },
      { rMin: 94, rMax: 137, angle: Math.PI * 0.4 },
      { rMin: 94, rMax: 137, angle: Math.PI * 0.8 },
      { rMin: 94, rMax: 137, angle: Math.PI * 1.2 },
      // Outer release maze
      { rMin: 137, rMax: 180, angle: -Math.PI * 0.3 },
      { rMin: 137, rMax: 180, angle: 0 },
      { rMin: 137, rMax: 180, angle: Math.PI * 0.22 },
      { rMin: 137, rMax: 180, angle: Math.PI * 0.78 },
      { rMin: 137, rMax: 180, angle: Math.PI * 1.05 },
      { rMin: 137, rMax: 180, angle: Math.PI * 1.35 },
    ];

    const gates = [
      {
        type: "MULTIPLY",
        value: 3,
        angle: Math.PI * 0.15,
        rMin: 94,
        rMax: 137,
        color: "#c084fc",
      },
      {
        type: "ADD",
        value: 12 + sub,
        angle: -Math.PI * 0.15,
        rMin: 137,
        rMax: 180,
        color: "#facc15",
      },
      {
        type: "MULTIPLY",
        value: 3,
        angle: Math.PI * 0.95,
        rMin: 137,
        rMax: 180,
        color: "#f472b6",
      },
      {
        type: "MULTIPLY",
        value: 4,
        angle: -Math.PI * 0.45,
        rMin: 137,
        rMax: 180,
        color: "#4ade80",
      },
    ];

    const isFinale = levelNum === 50;

    // Grand gauntlet deflector pins
    const deflectorPins = [
      {
        r: 115,
        angle: Math.PI * 0.15,
        radius: 10,
        restitution: 1.4,
        color: "#ec4899",
        accent: "#f43f5e",
      },
      {
        r: 160,
        angle: -Math.PI * 0.1,
        radius: 11,
        restitution: 1.45,
        color: "#facc15",
        accent: "#eab308",
      },
      {
        r: 160,
        angle: Math.PI * 1.2,
        radius: 10,
        restitution: 1.4,
        color: "#06b6d4",
        accent: "#0891b2",
      },
      {
        r: 115,
        angle: Math.PI * 1.0,
        radius: 10,
        restitution: 1.35,
        color: "#a855f7",
        accent: "#9333ea",
      },
    ];

    if (sub >= 5) {
      deflectorPins.push({
        r: 158,
        angle: Math.PI * 0.45,
        radius: 12,
        restitution: 1.5,
        color: "#f43f5e",
        accent: "#e11d48",
      });
    }

    if (sub >= 2) {
      gates.push({
        type: "BOOST",
        value: 180,
        angle: -Math.PI * 0.35,
        rMin: 137,
        rMax: 180,
        color: "#84cc16",
      });
    }

    return {
      id: `level_${levelNum}`,
      levelNumber: levelNum,
      worldIndex: 5,
      name: isFinale ? "The Kessler Singularity" : `Horizon Sector ${sub}`,
      code: `LVL ${levelNum}`,
      description: isFinale
        ? "⭐ GRAND FINALE! Clear humanity's greatest orbital debris cascade to save Earth!"
        : "Master orbital gauntlet! Route high-density debris through multi-stage shredder fields!",
      quota,
      initialDebrisCount,
      initialDebrisTypes: ["titanium", "solar", "volatile", "magnetic"],
      ringRadius,
      corridorWidth: 43,
      innerRadius,
      apertureAngle,
      trackWalls,
      dividers,
      gates,
      deflectorPins,
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

    for (let i = 0; i < numDividers; i++) {
      const ang = i * step + (rnd() - 0.5) * 0.15;
      let diff = Math.abs(ang - apCenter);
      while (diff > Math.PI) diff = Math.PI * 2 - diff;
      if (diff < apHalfWidth) continue;

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
        angle: 0.15 * Math.PI,
        rMin: 88,
        rMax: 165,
        color: "#facc15",
      },
      {
        type: "ADD",
        value: 5 + Math.floor(rnd() * 5),
        angle: 0.85 * Math.PI,
        rMin: 88,
        rMax: 165,
        color: "#38bdf8",
      },
      {
        type: "MULTIPLY",
        value: 3,
        angle: -0.5 * Math.PI,
        rMin: 88,
        rMax: 165,
        color: "#f472b6",
      },
    ];

    return {
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
      isDaily: true,
      rewardCredits: dailyInfo.rewardCredits,
    };
  }
}
