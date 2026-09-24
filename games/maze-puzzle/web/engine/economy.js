/**
 * Orbital Salvage: Cartoon In-Game Economy & Persistence
 * 100% Non-Monetized. All items unlocked through earned Gold Coins / Scrap Credits ($SC).
 */

const STORAGE_KEY = "ORBITAL_SALVAGE_CARTOON_PROFILE_V1";

export const COSMETIC_CATALOG = {
  rings: [
    {
      id: "ring_donut",
      name: "Frosted Donut Rig",
      desc: "Sweet strawberry icing with sprinkles and bouncy cartoon dough!",
      cost: 0,
      color: "#f472b6",
      accent: "#fde047",
      unlocked: true,
    },
    {
      id: "ring_duckie",
      name: "Rubber Duckie Floatie",
      desc: "Squeaky bright yellow bath duck with cute orange beak and water bubbles!",
      cost: 500,
      color: "#facc15",
      accent: "#fb923c",
      unlocked: false,
    },
    {
      id: "ring_candyland",
      name: "Peppermint Candy Ring",
      desc: "Crisp red and white candy cane stripes with minty freshness!",
      cost: 1200,
      color: "#ef4444",
      accent: "#ffffff",
      unlocked: false,
    },
    {
      id: "ring_spacecat",
      name: "Cosmic Kitty Saucer",
      desc: "Lilac flying saucer with adorable cat ears and neon whiskers!",
      cost: 2500,
      color: "#c084fc",
      accent: "#38bdf8",
      unlocked: false,
    },
  ],
  singularities: [
    {
      id: "singularity_nibbles",
      name: "Nibbles the Void",
      desc: "The hungriest, cutest cartoon black hole with big anime eyes! Nom nom!",
      cost: 0,
      coreColor: "#1e1b4b",
      haloColor: "#818cf8",
      unlocked: true,
    },
    {
      id: "singularity_strawberry",
      name: "Berry Swirl Vortex",
      desc: "Sweet strawberry cyclone that eats trash like fruit smoothie!",
      cost: 600,
      coreColor: "#831843",
      haloColor: "#f472b6",
      unlocked: false,
    },
    {
      id: "singularity_sleepy",
      name: "Sleepy Star Cloud",
      desc: "A sleepy pastel blue cloud that yawns and gulps down space junk!",
      cost: 1500,
      coreColor: "#0c4a6e",
      haloColor: "#38bdf8",
      unlocked: false,
    },
    {
      id: "singularity_confetti",
      name: "Party Popper Hole",
      desc: "Fiesta vortex exploding with rainbow sparkles and party confetti!",
      cost: 3000,
      coreColor: "#3b0764",
      haloColor: "#facc15",
      unlocked: false,
    },
  ],
  skyboxes: [
    {
      id: "skybox_cotton_candy",
      name: "Cotton Candy Nebula",
      desc: "Fluffy pastel indigo and magenta clouds with sparkling star smileys.",
      cost: 0,
      bgGradient: ["#1e1b4b", "#312e81", "#4338ca"],
      unlocked: true,
    },
    {
      id: "skybox_cheese_moon",
      name: "Cheese Moon Orbit",
      desc: "A happy cartoon moon made of Swiss cheese floating in twilight.",
      cost: 800,
      bgGradient: ["#0f172a", "#1e293b", "#334155"],
      unlocked: false,
    },
    {
      id: "skybox_mint_soda",
      name: "Mint Soda Lagoon",
      desc: "Sparkling emerald and turquoise cosmos with fizzy space bubbles.",
      cost: 2000,
      bgGradient: ["#042f2e", "#064e3b", "#065f46"],
      unlocked: false,
    },
  ],
};

export const ACHIEVEMENTS = [
  {
    id: "ach_first_drop",
    title: "First Nom Nom!",
    desc: "Feed Nibbles your very first piece of space junk!",
    icon: "😋",
    reward: 100,
  },
  {
    id: "ach_cleaner_50",
    title: "Tidy Space Cadet",
    desc: "Vacuum up 50 pieces of space trash into Nibbles' tummy.",
    icon: "🧹",
    reward: 250,
  },
  {
    id: "ach_cleaner_250",
    title: "Kessler Hero",
    desc: "Clean up 250 space items to save the orbit from clutter!",
    icon: "⭐",
    reward: 750,
  },
  {
    id: "ach_cleaner_1000",
    title: "Galaxy Janitor King",
    desc: "1,000 items recycled! You are the ultimate cosmic cleaner!",
    icon: "👑",
    reward: 2000,
  },
  {
    id: "ach_daily_shift",
    title: "Daily Duty Star",
    desc: "Complete today's wacky Daily Shift cleanup job!",
    icon: "🎉",
    reward: 500,
  },
];

export class EconomyManager {
  constructor() {
    this.profile = this.loadProfile();
  }

  getDefaultProfile() {
    return {
      version: 2,
      credits: 250, // Starting piggy bank coins
      careerDebris: 0,
      careerContracts: 0,
      equipped: {
        ring: "ring_donut",
        singularity: "singularity_nibbles",
        skybox: "skybox_cotton_candy",
      },
      unlocked: {
        rings: ["ring_donut"],
        singularities: ["singularity_nibbles"],
        skyboxes: ["skybox_cotton_candy"],
      },
      settings: {
        soundEnabled: true,
        sfxVolume: 80,
        ambientVolume: 70,
        hapticsEnabled: true,
      },
      achievements: [],
      completedSectors: [],
      dailyShiftCompletedToday: false,
      lastDailyShiftDate: "",
      lastDailyGiftDate: "",
    };
  }

  canClaimDailyGift() {
    const today = new Date().toISOString().slice(0, 10);
    return this.profile.lastDailyGiftDate !== today;
  }

  claimDailyGift(amount = 100) {
    if (!this.canClaimDailyGift()) return false;
    const today = new Date().toISOString().slice(0, 10);
    this.profile.lastDailyGiftDate = today;
    this.profile.credits += amount;
    this.saveProfile();
    return true;
  }

  normalizeCategory(cat) {
    if (cat === "ring" || cat === "rings") return { singular: "ring", plural: "rings" };
    if (cat === "singularity" || cat === "singularities") return { singular: "singularity", plural: "singularities" };
    if (cat === "skybox" || cat === "skyboxes") return { singular: "skybox", plural: "skyboxes" };
    return { singular: cat, plural: cat };
  }

  loadProfile() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw);
        if (!parsed.completedSectors) parsed.completedSectors = [];
        if (!parsed.settings) {
          parsed.settings = {
            soundEnabled: true,
            sfxVolume: 80,
            ambientVolume: 70,
            hapticsEnabled: true,
          };
        }
        return { ...this.getDefaultProfile(), ...parsed };
      }
    } catch (e) {
      console.warn("Could not load save profile from localStorage", e);
    }
    return this.getDefaultProfile();
  }

  resetProfile() {
    this.profile = this.getDefaultProfile();
    this.saveProfile();
    return this.profile;
  }

  recordSectorCompleted(sectorId) {
    if (!this.profile.completedSectors) {
      this.profile.completedSectors = [];
    }
    if (!this.profile.completedSectors.includes(sectorId)) {
      this.profile.completedSectors.push(sectorId);
      this.saveProfile();
    }
  }

  saveProfile() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.profile));
    } catch (e) {
      console.warn("Could not save profile to localStorage", e);
    }
  }

  addCredits(amount) {
    this.profile.credits += Math.max(0, Math.floor(amount));
    this.saveProfile();
    return this.profile.credits;
  }

  spendCredits(amount) {
    if (this.profile.credits >= amount) {
      this.profile.credits -= amount;
      this.saveProfile();
      return true;
    }
    return false;
  }

  isUnlocked(category, itemId) {
    const { plural } = this.normalizeCategory(category);
    const list = this.profile.unlocked[plural] || [];
    return list.includes(itemId);
  }

  unlockItem(category, itemId, cost) {
    const { plural } = this.normalizeCategory(category);
    if (this.isUnlocked(plural, itemId)) return true;
    if (this.spendCredits(cost)) {
      if (!this.profile.unlocked[plural]) {
        this.profile.unlocked[plural] = [];
      }
      this.profile.unlocked[plural].push(itemId);
      this.saveProfile();
      return true;
    }
    return false;
  }

  equipItem(category, itemId) {
    const { singular, plural } = this.normalizeCategory(category);
    if (this.isUnlocked(plural, itemId)) {
      this.profile.equipped[singular] = itemId;
      this.saveProfile();
      return true;
    }
    return false;
  }

  recordDebrisSwallowed(count = 1) {
    this.profile.careerDebris += count;
    this.saveProfile();
    return this.checkAchievements();
  }

  recordContractComplete(isDaily = false) {
    this.profile.careerContracts++;
    if (isDaily) {
      const today = new Date().toISOString().slice(0, 10);
      this.profile.lastDailyShiftDate = today;
      this.profile.dailyShiftCompletedToday = true;
    }
    this.saveProfile();
    return this.checkAchievements();
  }

  checkAchievements() {
    const newlyUnlocked = [];
    const p = this.profile;

    ACHIEVEMENTS.forEach((ach) => {
      if (p.achievements.includes(ach.id)) return;

      let qualified = false;
      if (ach.id === "ach_first_drop" && p.careerDebris >= 1) qualified = true;
      if (ach.id === "ach_cleaner_50" && p.careerDebris >= 50) qualified = true;
      if (ach.id === "ach_cleaner_250" && p.careerDebris >= 250)
        qualified = true;
      if (ach.id === "ach_cleaner_1000" && p.careerDebris >= 1000)
        qualified = true;
      if (ach.id === "ach_daily_shift" && p.dailyShiftCompletedToday)
        qualified = true;

      if (qualified) {
        p.achievements.push(ach.id);
        p.credits += ach.reward;
        newlyUnlocked.push(ach);
      }
    });

    if (newlyUnlocked.length > 0) {
      this.saveProfile();
    }
    return newlyUnlocked;
  }

  getDailyShiftInfo() {
    const today = new Date().toISOString().slice(0, 10);
    const isDone = this.profile.lastDailyShiftDate === today;

    let seed = 0;
    for (let i = 0; i < today.length; i++) {
      seed = (seed * 31 + today.charCodeAt(i)) & 0xffffff;
    }

    const funModifiers = [
      "Fizzy Soda Storm: Bubble bursts push trash!",
      "Super Bouncy Day: Extra springy corridors!",
      "Double Sprinkles: Bonus coins for sweet recycling!",
      "Snack Attack: Nibbles is extra hungry today!",
    ];
    const mod = funModifiers[seed % funModifiers.length];

    return {
      date: today,
      isCompleted: isDone,
      title: `Daily Trash Fiesta`,
      sectorCode: `MAP-${(seed % 89) + 10}`,
      quota: 40 + (seed % 35),
      rewardCredits: 350 + (seed % 200),
      hazardModifier: mod,
      seed: seed,
    };
  }
}
