# Logic golden fixtures

Golden data for the C# port of `maze.js`, `physics.js` and `economy.js` (`OrbitalSalvage.Logic`). The generators import the unmodified web engine modules from `games/maze-puzzle/web/engine/` and seed `Math.random` with mulberry32 (`0x5EED1234`, SPEC §4.1).

## Authoritative engine

Chromium 153 (Playwright) is the source of truth. Node 24 computes `sin`/`cos`/`atan2` with a different libm than Chromium 153, so Node-generated physics goldens drift from Chromium's by 1 ulp at about frame 17. Chaotic collisions amplify that into different RNG call counts by frame ~265. The C# port matches Chromium, not Node. `--engine node` exists only as a cross-check.

Timezone is pinned to `America/Los_Angeles` for the economy and countdown fixtures (SPEC §4.2).

## Commands

Run from the repository root. Playwright resolves from `tools/parity/node_modules` (`cd tools/parity; npm install; npx playwright install chromium` if missing).

```powershell
node tools/parity/golden/jsmath-vectors.mjs
node tools/parity/golden/generate.mjs
node tools/parity/golden/generate.mjs --full --out $env:TEMP\gfull
node tools/parity/golden/generate.mjs --engine node --only physics --filter L21-drag-silent --full --out $env:TEMP\gnode
```

`generate.mjs` flags: `--engine chromium|node`, `--out <dir>`, `--only maze,economy,physics`, `--filter <substr>`, `--full`.

- Default (sampled) mode stores a hash of every frame plus the full state vector at every 100th frame. About 11 MiB in total, and this is what is committed.
- `--full` stores the full state vector for all 601 frames (about 100 MiB). Use it to get field-level reports. Point the tests at it with `PARITY_GOLDEN_FIXTURES=<dir>`.
- Any run is bit-compared against the hashes, so the sampled fixtures prove all 600 frames. Full mode only improves the diagnostics.

## Fixture layout (`fixtures/`)

| File | Content |
| --- | --- |
| `engine.txt` | Engine that produced the fixtures (`chromium 153...`) |
| `jsmath-vectors.json` | Hex-bit input/output vectors for every `JsMath`/`JsNumber` function (`jsmath-vectors.mjs`) |
| `rng-reference.json` | First mulberry32 outputs as hex bits |
| `maze.json` | `getSectors`, `getSectorWorlds`, all 50 `buildLevel` results, 48 daily-shift cases and a no-title case, as exact JSON text |
| `economy.json` | Catalog and achievements JSON; operation sequences (shop, campaign, daily, load) with return values and the stored localStorage JSON text; countdown cases |
| `physics/<run>.json.gz` | Static sector config, RNG call counts and state, event/cue log, per-frame hash, sampled snapshots, resync fixtures |
| `physics/<run>.frames.bin.gz` | Float64 snapshot vectors for the sampled frames (all frames with `--full`) |

Run names are `L<level>-<program>-<audio|silent>`. Levels 1, 11, 21, 31 and 41 are the SPEC sectors. Levels 38 and 50 are added for mechanics coverage:

| Level | Mechanics |
| --- | --- |
| 1, 11 | dividers and gates |
| 21 | + deflector pins, track wall, concentric rings |
| 31, 38 | + deflector pins, one-way flap |
| 41, 50 | + deflector pins, two one-way flaps, track wall, concentric rings |

Programs:

- `idle`: no input.
- `drag`: constant drag input.
- `bursts`: drag bursts plus double-tap RCS bursts.

All runs are 600 frames at `dt = 16/1000`. Audio runs initialise the stub audio per SPEC §4.6, so they draw RNG exactly like the web audio engine. Silent runs make no RNG draws for audio. The `drag-audio` and `bursts-audio` runs also carry per-step resync fixtures at frames 0, 50, 200 and 400. Each has the pre-state vector, post-state vector, audio state, RNG state, inputs and the log for one step.

## Snapshot vector

One `Float64` vector per frame, produced by `browser/driver.mjs` (`capture`) and `PhysicsSnapshot.Capture`. Layout:

1. 12 globals (`ringAngle`, `angularVelocity`, `innerRingAngle`, `rcsBoostTimer`, `lastRatchetAngle`, `rcsThrusting`, `rcsDirection`, `nextDebrisId`, `debrisCount`, `flapCount`, `pinCount`, `gateCount`).
2. 12 values per debris (`id x y vx vy radius mass weight state ingestProgress gateCooldown type`).
3. 2 values per flap (`swing swingVel`).
4. 1 value per pin (`hitTimer`).
5. Per gate: the processed-debris count, then the sorted ids.

The static sector configuration (geometry that never changes during a run) is stored separately in `CaptureStatic`. `PhysicsSnapshot.Describe(vector, index)` names the field at an index for failure reports.

The frame hash is a dual 32-bit FNV-style hash over the vector as uint words. Log entries are `[frame, "cue:name|args"]` or `[frame, "ev:..."]` with doubles written as 16-digit hex.

## Tests

- Unity EditMode: `OrbitalSalvage.Logic.Tests` (`Assets/Tests/EditMode/Logic`). Fixtures resolve via `../../../../tools/parity/golden/fixtures` relative to the Unity project.
- Fast loop: `dotnet test tools/parity/golden/dotnet`. It compiles the same sources and tests on .NET 10.

```powershell
Unity.exe -batchmode -nographics -projectPath games\maze-puzzle\unity\maze-puzzle -runTests -testPlatform EditMode -testResults $env:TEMP\logic-results.xml -logFile $env:TEMP\logic-tests.log
```

Do not pass `-quit` with `-runTests`.
