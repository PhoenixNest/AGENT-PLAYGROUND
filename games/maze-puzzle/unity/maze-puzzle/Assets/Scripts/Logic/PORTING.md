# Logic port: JS symbol to C# symbol

`OrbitalSalvage.Logic` and `OrbitalSalvage.JsCompat` are bit-exact ports of the web engine. Both assemblies use `noEngineReferences` and run `double` arithmetic only. JS paths are relative to `games/maze-puzzle/web/`. C# paths are relative to `Assets/Scripts/`. Each line number is the declaration line.

Proof: golden fixtures from Chromium 153 in `tools/parity/golden/fixtures` (see `tools/parity/golden/README.md`) and the EditMode tests in `Assets/Tests/EditMode/Logic`.

## JsCompat

| JS | C# |
| --- | --- |
| `Math.random` (mulberry32, SPEC §4.1) | `JsCompat/JsRandom.cs:17` `JsRandom`; `NextDouble()` :64, `NextDouble(tag)` :69, `Reset` :52, `State` property, call counter and tagged log |
| `Math.sin` / `cos` | `JsCompat/JsMath.Libm.cs:129` `Sin`, :146 `Cos` (correctly rounded, matches Chromium 153) |
| `Math.atan` / `atan2` | `JsCompat/JsMath.Libm.cs:177` `Atan`, :203 `Atan2` |
| `Math.pow` | `JsCompat/JsMath.cs:199` `Pow` |
| `Math.hypot` | `JsCompat/JsMath.cs:126` `Hypot(x, y)`, :164 `Hypot(params)` |
| `Math.sqrt/abs/floor/ceil/trunc/sign` | `JsCompat/JsMath.cs:46/41/51/56/61/66` |
| `Math.min` / `max` (NaN, -0) | `JsCompat/JsMath.cs:74` `Min`, :84 `Max` |
| `Math.round` | `JsCompat/JsMath.cs:94` `Round` |
| `%` on numbers | `JsCompat/JsMath.cs:101` `Mod` |
| `\| 0`, `>>> 0`, `Math.imul`, `&` | `JsCompat/JsMath.cs:106` `ToInt32`, :116 `ToUint32`, :121 `Imul` |
| `String(number)` | `JsCompat/JsNumber.cs:9` `ToString` |
| `JSON.parse` | `JsCompat/JsonParser.cs:9` `Parse`, :19 `TryParse` |
| `JSON.stringify` | `JsCompat/JsonWriter.cs:8` `Stringify` |

## maze.js (`engine/maze.js`) to `Logic/MazeLevelBuilder.cs`

| JS | C# |
| --- | --- |
| `MazeLevelBuilder` :7 | `MazeLevelBuilder` :7 |
| `sanitizeSector` :12 | `SanitizeSector` :33 |
| `getSectors` :167 | `GetSectors` :192 |
| `getSectorWorlds` :175 | `GetSectorWorlds` :202 |
| `buildLevel` :228 | `BuildLevel` :214 |
| `buildWorld1Level` :255 | `BuildWorld1Level` :271 |
| `buildWorld2Level` :329 | `BuildWorld2Level` :324 |
| `buildWorld3Level` :432 | `BuildWorld3Level` :388 |
| `buildWorld4Level` :554 | `BuildWorld4Level` :457 |
| `buildWorld5Level` :703 | `BuildWorld5Level` :537 |
| `buildDailyShift` :890 | `BuildDailyShift` :627 |

Config shapes: `Logic/MazeConfig.cs` (`SectorConfig`, `GateConfig`, `DividerConfig`, `OneWayFlapConfig`, `DeflectorPinConfig`, `TrackWallConfig`, `WorldInfo`, `DailyShiftInfo`). `Logic/MazeJson.cs` serialises them to the same `JSON.stringify` text as the JS objects, with the same key order.

## physics.js (`engine/physics.js`) to `Logic/PhysicsEngine.cs`

| JS | C# |
| --- | --- |
| `PhysicsEngine` :7, `constructor` :8 | `PhysicsEngine` :7, ctor :53 |
| `initSector` :69 | `InitSector` :78 |
| `spawnDebris` :161 | `SpawnDebris` :213 (+ :269 overload) |
| `spawnBackupScrap` :209 | `SpawnBackupScrap` :274 |
| `applyInputDrag` :230 | `ApplyInputDrag` :291 |
| `applyRcsBurst` :235 | `ApplyRcsBurst` :297 |
| `update` :246 | `Update` :310 |
| `resolveMagneticFunnel` :430 | `ResolveMagneticFunnel` :491 |
| `updateIngestion` :461 | `UpdateIngestion` :527 |
| `resolveRingCollisions` :475 | `ResolveRingCollisions` :542 |
| `resolveDividerCollisions` :535 | `ResolveDividerCollisions` :602 |
| `resolveFlapperCollisions` :585 | `ResolveFlapperCollisions` :647 |
| `resolveTrackWallCollisions` :644 | `ResolveTrackWallCollisions` :708 |
| `resolveDeflectorCollisions` :694 | `ResolveDeflectorCollisions` :754 |
| `checkMultiplierGates` :757 | `CheckMultiplierGates` :826 |
| `syncAdjacentGates` (inner arrow, :789) | `SyncAdjacentGates` :810 |
| `checkVolatileDetonation` :860 | `CheckVolatileDetonation` :910 |
| `resolveDebrisCollisions` :885 | `ResolveDebrisCollisions` :937 |
| `normalizeAngle` :928 | `NormalizeAngle` :983 |
| `isAngleBetween` :934 | `IsAngleBetween` :990 |

Runtime types are in `Logic/PhysicsTypes.cs` (`Debris`, `GateRuntime`, `DividerRuntime`, `FlapRuntime`, `PinRuntime`, `TrackWallRuntime`, `BlackHole`, `BumperHit`, `Vec2`). The JS callbacks (`onDebrisSwallowed`, `onGatePassed`, ...) become `Action` fields on `PhysicsEngine`.

State capture for goldens and resync: `Logic/PhysicsSnapshot.cs` (`Capture` :98, `CaptureStatic` :67, `Load` :147, `Describe` :220).

### JS behaviours reproduced deliberately

| Behaviour | JS | C# |
| --- | --- | --- |
| `nextDebrisId` is not reset by `initSector` | `physics.js:14`, :189 | `PhysicsEngine.cs` field init and `SpawnDebris` |
| Debris cap of 320 still draws one RNG value | `physics.js:46`, :165 | `SpawnDebris` |
| Spawn argument order fixes the RNG draw order (angle, radius, vx, vy) | `physics.js:145-155`, :213-223 | `InitSector`, `SpawnBackupScrap` |
| Backwards loop with `splice` | `physics.js:313` | `Update` (`RemoveAt`) |
| `x \|\| default` on numbers, where 0 and NaN fall to the default | `physics.js:101-140`, :602, :611, :663, :734, :814 | `Truthy` :68 / `OrDefault` :73 |
| `Math.sign(angularVelocity) \|\| 1` | `physics.js:815` | `PhysicsEngine.cs:864` |
| Stale `dx`/`dy`/`dist` inside divider and gate loops | `physics.js:535-583`, :757-858 | the same loops, locals not re-declared |
| `Math.pow(damping, dt * 60)` | `physics.js:259` | `JsMath.Pow` (see FP notes) |
| `Math.hypot` | `physics.js:325`, :355, :386, :480, :538, :589, :649, :712, :762, :876, :897 | `JsMath.Hypot` |

## Audio (`engine/audio.js`) to `Logic/IAudioCues.cs`

`IAudioCues` lists only the cue calls that physics makes. `Logic/ParityAudioCues.cs` is the SPEC §4.6 stub:

- Before init (or while muted) it blocks every cue and draws no RNG.
- After `PublicInit`, cues draw RNG in the same order as the web engine and record `cue:name|args` log entries.
- `CaptureState` / `LoadState` carry the audio timers for resync.

The real synthesis (oscillators, buffers) is not ported. It belongs to the controller port.

## economy.js (`engine/economy.js`) to `Logic/EconomyManager.cs`

| JS | C# |
| --- | --- |
| `STORAGE_KEY` :6 | `EconomyManager.StorageKey` :16 (`ORBITAL_SALVAGE_CARTOON_PROFILE_V1`) |
| `COSMETIC_CATALOG` :8 | `Logic/EconomyCatalog.cs:61` `EconomyCatalog`, item type `CosmeticItem` :6 |
| `ACHIEVEMENTS` :113 | `EconomyCatalog`, item type `Achievement` :41 |
| `EconomyManager` :151 | `EconomyManager` :14 |
| `getDefaultProfile` :156 | `GetDefaultProfile` :41 |
| `canClaimDailyGift` :188 | `CanClaimDailyGift` :46 |
| `claimDailyGift` :193 | `ClaimDailyGift` :51 |
| `normalizeCategory` :202 | `NormalizeCategory` :61 |
| `loadProfile` :209 | `LoadProfile` :69 |
| `resetProfile` :238 | `ResetProfile` :86 |
| `recordSectorCompleted` :244 | `RecordSectorCompleted` :93 |
| `saveProfile` :254 | `SaveProfile` :106 |
| `addCredits` :262 | `AddCredits` :117 |
| `spendCredits` :268 | `SpendCredits` :124 |
| `isUnlocked` :277 | `IsUnlocked` :135 |
| `unlockItem` :283 | `UnlockItem` :142 |
| `equipItem` :297 | `EquipItem` :159 |
| `recordDebrisSwallowed` :307 | `RecordDebrisSwallowed` :171 |
| `recordContractComplete` :313 | `RecordContractComplete` :178 |
| `canClaimDailyMasterBonus` :334 | `CanClaimDailyMasterBonus` :204 |
| `isDailyMasterClaimed` :341 | `IsDailyMasterClaimed` :212 |
| `claimDailyMasterBonus` :346 | `ClaimDailyMasterBonus` :217 |
| `checkAchievements` :355 | `CheckAchievements` :227 |
| `getDailyShifts` :385 | `GetDailyShifts` :258 |
| `getDailyShiftInfo` :457 | `GetDailyShiftInfo` :334 |
| `new Date().toISOString().slice(0, 10)` (:189 and others) | `ToIsoDay` :30, `Today` :36 via `IWallClock` |
| `localStorage` | `IKeyValueStore` (`Logic/EconomyPlatform.cs:6`); `MemoryKeyValueStore` for tests |
| `main.js:891` payout `baseReward + surplus * 4` | `CampaignPayout` :340 |
| `main.js:1077` `getDailyResetCountdown` | `Logic/DailyReset.cs:19` `GetCountdownText`, `FindLosAngeles` :7 |

`Profile.cs` is a typed `Profile` that reproduces `{ ...defaults, ...parsed }` key order and the stored `JSON.stringify` text. A malformed or hand-edited save, such as `equipped` holding non-string values, is the only input where the typed model differs from the dynamic JS object.

## Not ported (left for the controller port)

- `renderer.js`: all canvas drawing.
- `main.js`: game loop, UI and DOM, debrief screens, input handling. Only the payout formula (`:891`) and the reset countdown (`:1077`) are ported.
- `audio.js` synthesis. Only the cue interface and the RNG-parity stub are ported.
- Unity wiring: a `MonoBehaviour` that feeds `Update(16/1000)`, a `PlayerPrefs`-backed `IKeyValueStore` and a real `IWallClock` (`SystemWallClock` exists).

## Floating-point notes

- Mono EditMode runs the `double` code as written, and the tests pass bit-exact.
- IL2CPP or any AOT build may fuse `a * b + c` into an FMA. That would change physics results, and it would break the double-double trig path (`DoubleDouble.cs`), which assumes unfused products. Verify with the golden tests in a player build, and keep `JsCompat` and `Logic` on a toolchain or flag setting that does not contract.
- `JsMath.Pow` uses `Math.Pow`, which matches the only parameter pair in use (`0.94`, `dt * 60`) but differs from V8 on about 3 in 6000 random inputs. It is exact for the physics path and ≤1 ulp otherwise.
- Mono's `double.Parse` throws `OverflowException` instead of returning infinity, and its `ToString("E..")` / `Parse` are not correctly rounded (7 of 1532 vectors differed). `JsonParser` catches the overflow. `JsNumber.ToString` uses exact `BigInteger` shortest-digit generation (Burger-Dybvig) instead of runtime formatting.
