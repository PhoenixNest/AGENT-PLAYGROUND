import { existsSync } from "node:fs";
import { dirname, join } from "node:path";

// games/maze-puzzle: the directory that contains both tools/parity and the web game.
export function mazeRoot(startDir) {
  let dir = startDir;
  for (let i = 0; i < 14; i++) {
    const marker = join(dir, "tools", "parity", "package.json");
    const web = join(dir, "web", "index.html");
    if (existsSync(marker) && existsSync(web)) return dir;
    const parent = dirname(dir);
    if (parent === dir) break;
    dir = parent;
  }
  throw new Error("games/maze-puzzle marker (tools/parity + web) not found from " + startDir);
}

export function webRoot(startDir) {
  return join(mazeRoot(startDir), "web");
}

export function parityRoot(startDir) {
  return join(mazeRoot(startDir), "tools", "parity");
}

export function unityProject(startDir) {
  return join(mazeRoot(startDir), "unity", "maze-puzzle");
}
