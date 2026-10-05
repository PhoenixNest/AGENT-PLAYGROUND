import { dumpMaze, runPhysicsScenario, toBase64 } from "./driver.mjs";
import { runEconomy } from "./economy-driver.mjs";

export function collectStatic(mods) {
  return {
    maze: dumpMaze(mods),
    economy: runEconomy(mods),
    catalog: {
      cosmetics: JSON.stringify(mods.COSMETIC_CATALOG),
      achievements: JSON.stringify(mods.ACHIEVEMENTS),
    },
  };
}

export function collectPhysics(mods, spec) {
  const r = runPhysicsScenario(mods, spec);
  return { header: r.header, framesB64: toBase64(r.framesF64) };
}
