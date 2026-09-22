/**
 * Orbital Salvage: Cartoon Maze & Corridor Topology Engine
 * Defines geometry for chunky candy rings, divider chutes,
 * and cartoon multiplier gates sized for mobile portrait screens.
 */

export class MazeLevelBuilder {
  static getSectors() {
    return [
      {
        id: "sector_1",
        name: "Bubblegum Belt",
        code: "MAP 1",
        description:
          "Welcome to space cleanup! Spin the donut to send yummy space snacks into Nibbles!",
        quota: 30,
        initialDebrisCount: 12,
        initialDebrisTypes: ["titanium", "solar"],
        ringRadius: 155,
        corridorWidth: 36,
        apertureAngle: Math.PI * 0.28,
        dividers: [
          { rMin: 85, rMax: 155, angle: 0 },
          { rMin: 85, rMax: 155, angle: Math.PI * 0.5 },
          { rMin: 85, rMax: 155, angle: Math.PI },
          { rMin: 120, rMax: 155, angle: Math.PI * 1.35 },
          { rMin: 85, rMax: 120, angle: Math.PI * 1.7 },
        ],
        gates: [
          {
            type: "MULTIPLY",
            value: 2,
            angle: Math.PI * 0.25,
            r: 120,
            length: 34,
            color: "#facc15",
          },
          {
            type: "ADD",
            value: 4,
            angle: Math.PI * 0.75,
            r: 120,
            length: 34,
            color: "#38bdf8",
          },
          {
            type: "MULTIPLY",
            value: 3,
            angle: Math.PI * 1.5,
            r: 120,
            length: 34,
            color: "#f472b6",
          },
        ],
      },
      {
        id: "sector_2",
        name: "Cheese Moon Chutes",
        code: "MAP 2",
        description:
          "Watch out for spicy batteries! If they bump too fast, they pop into confetti!",
        quota: 75,
        initialDebrisCount: 15,
        initialDebrisTypes: ["titanium", "solar", "volatile"],
        ringRadius: 165,
        corridorWidth: 38,
        apertureAngle: Math.PI * 0.25,
        dividers: [
          { rMin: 90, rMax: 165, angle: Math.PI * 0.15 },
          { rMin: 90, rMax: 128, angle: Math.PI * 0.45 },
          { rMin: 128, rMax: 165, angle: Math.PI * 0.8 },
          { rMin: 90, rMax: 165, angle: Math.PI * 1.15 },
          { rMin: 128, rMax: 165, angle: Math.PI * 1.6 },
          { rMin: 90, rMax: 128, angle: Math.PI * 1.85 },
        ],
        gates: [
          {
            type: "MULTIPLY",
            value: 3,
            angle: Math.PI * 0.35,
            r: 128,
            length: 38,
            color: "#f472b6",
          },
          {
            type: "ADD",
            value: 6,
            angle: Math.PI * 0.95,
            r: 128,
            length: 38,
            color: "#38bdf8",
          },
          {
            type: "MULTIPLY",
            value: 2,
            angle: Math.PI * 1.4,
            r: 128,
            length: 38,
            color: "#facc15",
          },
          {
            type: "MULTIPLY",
            value: 2,
            angle: Math.PI * 1.75,
            r: 128,
            length: 38,
            color: "#4ade80",
          },
        ],
      },
      {
        id: "sector_3",
        name: "Rainbow Donut Swirl",
        code: "MAP 3",
        description:
          "Double donut corridors! Line up the openings for mega multiplication fun!",
        quota: 120,
        initialDebrisCount: 18,
        initialDebrisTypes: ["titanium", "solar", "magnetic"],
        ringRadius: 172,
        corridorWidth: 40,
        apertureAngle: Math.PI * 0.22,
        dividers: [
          // Outer track dividers
          { rMin: 132, rMax: 172, angle: 0 },
          { rMin: 132, rMax: 172, angle: Math.PI * 0.4 },
          { rMin: 132, rMax: 172, angle: Math.PI * 0.8 },
          { rMin: 132, rMax: 172, angle: Math.PI * 1.2 },
          { rMin: 132, rMax: 172, angle: Math.PI * 1.6 },
          // Inner track dividers
          { rMin: 92, rMax: 132, angle: Math.PI * 0.2 },
          { rMin: 92, rMax: 132, angle: Math.PI * 0.6 },
          { rMin: 92, rMax: 132, angle: Math.PI * 1.0 },
          { rMin: 92, rMax: 132, angle: Math.PI * 1.4 },
          { rMin: 92, rMax: 132, angle: Math.PI * 1.8 },
        ],
        gates: [
          {
            type: "MULTIPLY",
            value: 2,
            angle: Math.PI * 0.1,
            r: 152,
            length: 38,
            color: "#38bdf8",
          },
          {
            type: "MULTIPLY",
            value: 3,
            angle: Math.PI * 0.5,
            r: 112,
            length: 38,
            color: "#f472b6",
          },
          {
            type: "ADD",
            value: 10,
            angle: Math.PI * 0.9,
            r: 152,
            length: 38,
            color: "#facc15",
          },
          {
            type: "MULTIPLY",
            value: 2,
            angle: Math.PI * 1.3,
            r: 112,
            length: 38,
            color: "#4ade80",
          },
          {
            type: "MULTIPLY",
            value: 4,
            angle: Math.PI * 1.7,
            r: 152,
            length: 38,
            color: "#c084fc",
          },
        ],
      },
    ];
  }

  static buildDailyShift(dailyInfo) {
    const seed = dailyInfo.seed || 12345;
    let s = seed;
    const rnd = () => {
      s = (s * 1664525 + 1013904223) % 4294967296;
      return s / 4294967296;
    };

    const numDividers = 6 + Math.floor(rnd() * 3);
    const dividers = [];
    const step = (Math.PI * 2) / numDividers;

    for (let i = 0; i < numDividers; i++) {
      if (i === 0) continue;
      const ang = i * step + (rnd() - 0.5) * 0.2;
      const mid = 125 + (rnd() - 0.5) * 15;
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
        angle: Math.PI * 0.3,
        r: 125,
        length: 38,
        color: "#facc15",
      },
      {
        type: "ADD",
        value: 5 + Math.floor(rnd() * 6),
        angle: Math.PI * 0.8,
        r: 125,
        length: 38,
        color: "#38bdf8",
      },
      {
        type: "MULTIPLY",
        value: 3,
        angle: Math.PI * 1.4,
        r: 125,
        length: 38,
        color: "#f472b6",
      },
    ];

    return {
      id: "daily_shift",
      name: dailyInfo.title,
      code: "FIESTA",
      description: `Daily Fun Contract! ${dailyInfo.hazardModifier}`,
      quota: dailyInfo.quota,
      initialDebrisCount: 14,
      initialDebrisTypes: ["titanium", "solar", "volatile"],
      ringRadius: 165,
      corridorWidth: 38,
      apertureAngle: Math.PI * 0.26,
      dividers,
      gates,
      isDaily: true,
      rewardCredits: dailyInfo.rewardCredits,
    };
  }
}
