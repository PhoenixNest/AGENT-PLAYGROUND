import fs from "fs";
import path from "path";
import { fileURLToPath, pathToFileURL } from "url";
import vm from "vm";

import { webRoot } from "../lib/roots.mjs";

const here = path.dirname(fileURLToPath(import.meta.url));
const web = webRoot(here);
const physicsUrl = pathToFileURL(path.join(web, "engine/physics.js")).href;
const { PhysicsEngine } = await import(physicsUrl);

const main = fs.readFileSync(path.join(web, "main.js"), "utf8");
const start = main.indexOf("initLayout() {");
if (start < 0) throw new Error("initLayout not found");
const open = main.indexOf("{", start);
let depth = 0;
let end = -1;
for (let i = open; i < main.length; i++) {
  if (main[i] === "{") depth++;
  else if (main[i] === "}") {
    depth--;
    if (depth === 0) {
      end = i;
      break;
    }
  }
}
if (end < 0) throw new Error("initLayout brace match failed");
const body = main.slice(open + 1, end);

const audio = {
  playSpinRatchet() {},
  playDebrisCollision() {},
  playDebrisSwallowed() {},
  playGatePass() {},
  playRcsThrust() {},
  playBumperBounce() {},
  playHazardExplosion() {},
};
const physics = new PhysicsEngine(audio);
const ctx = {
  frame: { clientWidth: 430, clientHeight: 932 },
  renderer: { resize() {} },
  physics,
};
const sandbox = { ctx };
vm.createContext(sandbox);
vm.runInContext(
  `(function () {\n${body}\nthis.__scale = (typeof scale === "number") ? scale : null;\n}).call(ctx);`,
  sandbox,
  { filename: "main.js initLayout" }
);
const out = {
  source: "games/maze-puzzle/web/main.js initLayout + engine/physics.js blackHole defaults",
  w: 430,
  h: 932,
  scale: ctx.__scale,
  centerX: physics.center.x,
  centerY: physics.center.y,
  blackHoleX: physics.blackHole.x,
  blackHoleY: physics.blackHole.y,
  eventHorizonRadius: physics.blackHole.eventHorizonRadius,
  gravityRadius: physics.blackHole.gravityRadius,
  gravityStrength: physics.blackHole.gravityStrength,
};
const dest = path.resolve(here, "../reports/physics-framerate-2026-10-07-web-layout.json");
fs.mkdirSync(path.dirname(dest), { recursive: true });
fs.writeFileSync(dest, JSON.stringify(out, null, 2) + "\n");
console.log(JSON.stringify(out));
