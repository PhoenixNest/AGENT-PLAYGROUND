# AudioEngine inventory (`games/maze-puzzle/web/engine/audio.js`)

Source of truth for the C# port (`Assets/Scripts/AudioDsp`). All sound is synthesised with Web Audio; the repo has no audio files in the web prototype. `ctx` is `null` until `publicInit()`.

## 1. Totals

| Item                                          | Count                                                                                                                                                                   |
| --------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Public methods                                | 20 (`publicInit`, `ensureRunning`, `setSfxVolume`, `setAmbienceVolume`, `toggleMute`, 14 cue methods listed below, plus `setThrusterActive` which only forwards)       |
| Cue methods (produce sound)                   | 14 (`playSpinRatchet`, `playDebrisCollision`, `playGatePass`, `playShredderPass`, `playCompactorPass`, `playSuperShredderPass`, `playBoosterPass`, `playRcsThrust`, `playBumperBounce`, `playDebrisSwallowed`, `playHazardExplosion`, `playVictoryFanfare`, `playDefeatSound`, `playBackupPodSound`) + `setThrusterActive(true)` |
| Node types used                               | `OscillatorNode` (sine, triangle, sawtooth; no `square`), `GainNode`, `BiquadFilterNode` (bandpass, lowpass), `AudioBufferSourceNode` (non-looping, mono 44.1 kHz), `DynamicsCompressorNode`, `AudioDestinationNode` |
| AudioParam methods used                       | `setValueAtTime`, `linearRampToValueAtTime`, `exponentialRampToValueAtTime`, `cancelScheduledValues`; `.value` read once (`toggleMute`). **Not used:** `setTargetAtTime`, `setValueCurveAtTime`, `cancelAndHoldAtTime`, `detune`, `playbackRate`, `loop`. |
| `Math.random` sites                           | 5 (collision x3 branches share one draw, bumper, rcs noise); see section 5                                                                                              |
| Looping / ambience source                     | **None.** `ambienceGain` exists and feeds `masterGain` but nothing is ever connected to it. `setAmbienceVolume` only changes its gain.                                 |
| Audio files                                   | None in the web prototype                                                                                                                                               |

## 2. Master chain (built in `publicInit`, lines 48-80)

```
cue nodes ──► masterCompressor ──► sfxGain ──► masterGain ──► destination
              ambienceGain (no inputs) ─────────► masterGain
```

| Node               | Setting (all `setValueAtTime(v, ctx.currentTime)` at init time `t_i`)                    |
| ------------------ | ------------------------------------------------------------------------------------------ |
| `masterCompressor` | threshold −14 dB, knee 10 dB, ratio 8, attack 0.003 s, release 0.12 s (the per-node default makeup/pre-delay of Chromium applies: pre-delay 6 ms, makeup `1/Saturate(1,k)^0.6`) |
| `sfxGain`          | `sfxVolume` = 0.8 (default)                                                                |
| `ambienceGain`     | `ambienceVolume` = 0.7 (default)                                                           |
| `masterGain`       | `isMuted ? 0 : 0.85`                                                                       |

Creation order (node ids in the SPEC trace): compressor 1, sfxGain 2, ambienceGain 3, masterGain 4; cue nodes follow from id 5.

Volume setters: `setSfxVolume(p)` / `setAmbienceVolume(p)`: `v = clamp(Number(p) || 0, 0, 100) / 100`; stored always; `gain.setValueAtTime(v, now)` only when `ctx` exists.

`toggleMute()`: flips `isMuted`; when `ctx` exists: `ensureRunning`, then on `masterGain.gain`: `cancelScheduledValues(t)`, `setValueAtTime(gain.value, t)`, `linearRampToValueAtTime(isMuted ? 0 : 0.85, t + 0.02)`; returns `isMuted`. Without a context only the flag changes (init later applies it: master gain starts at 0 when muted).

## 3. Cue graphs

`now` = `ctx.currentTime`. Every voice ends `... -> gain -> masterCompressor` unless stated. "exp" ramps always go to `0.0001`.

| Method (args, defaults)                                | Guard / cooldown (field)                          | Graph and automation                                                                                                                                                                                                                                           | Total length |
| ------------------------------------------------------ | ------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ |
| `playSpinRatchet(pitch = 1)`                           | ctx, !muted; `now - lastRatchetTime < 0.045` drops | osc `triangle` -> biquad `bandpass` (f=1200 @now, Q=1.5 @now) -> gain. osc f: set `800*clamp(pitch,0.6,1.8)` @now, exp 140 @now+0.022. gain: set 0.06 @now, exp 0.0001 @now+0.024. start now, stop now+0.026                                                  | 0.026 s      |
| `playDebrisCollision(intensity = 1, type = "titanium")` | `now - lastCollisionTime < 0.048` drops           | osc -> gain. One `Math.random` r. solar: tri, f0 = 620+80r, f1 = 340; volatile: sine, 260+40r -> 90; else: tri, 460+60r -> 160. osc f: set f0 @now, exp f1 @now+0.038. gain: set `min(0.12*intensity, 0.14)` @now, exp 0.0001 @now+0.04. start now, stop now+0.042 | 0.042 s      |
| `playGatePass(type = "MULTIPLY", value = 2)`           | `now - lastGatePassTime < 0.07` drops             | Dispatch (no own sound): `BOOST` -> `playBoosterPass(now)`; `ADD` -> `playCompactorPass(now, value)`; else `value >= 4` -> `playSuperShredderPass(now, value)`; else `playShredderPass(now, value)`. `value` does not change any sound otherwise                | see below    |
| `playShredderPass(now?, value)`                        | ctx, !muted; `now = now \|\| currentTime`        | osc `sawtooth` -> biquad `lowpass` (f=1600 @now; Q default 1) -> gain. osc f: set 450 @now, exp 880 @now+0.08. gain: set 0.14 @now, exp 0.0001 @now+0.12. start now, stop now+0.14                                                                                | 0.14 s       |
| `playCompactorPass(now?, value)`                       | same                                              | osc `sine` -> gain. f: set 140, exp 320 @now+0.14. gain: set 0.22, exp 0.0001 @now+0.22. start now, stop now+0.24                                                                                                                                              | 0.24 s       |
| `playSuperShredderPass(now?, value)`                   | same                                              | 3 voices (f = 523.25, 783.99, 1046.5; i = 0..2; `o = now + 0.02*i`): osc `triangle` -> gain. f: set f @o. gain: set 0 @o, linear 0.12 @o+0.015, exp 0.0001 @o+0.18. start o, stop o+0.2                                                                       | 0.24 s       |
| `playBoosterPass(now?)`                                | ctx, !muted; `t = isFinite(now) && now > 0 ? now : (currentTime \|\| 0.001)` | osc `sine` -> gain. f: set 350 @t, exp 1100 @t+0.12. gain: set 0.16 @t, exp 0.0001 @t+0.15. start t, stop t+0.17                                                                                                                                                  | 0.17 s       |
| `playRcsThrust()`                                      | `now - lastRcsThrustTime < 0.05` drops            | `createBuffer(1, 9702, 44100)`: `data[i] = (r*2-1) * (1 - i/9702)`, one `Math.random` per sample in index order. bufferSource -> biquad `bandpass` (f: set 1800 @now, exp 420 @now+0.18; Q: set 1.2 @now) -> gain (set 0.14 @now, exp 0.0001 @now+0.2). start now, stop now+0.22 | 0.22 s       |
| `playBumperBounce()`                                   | `now - lastBumperTime < 0.045` drops (`lastBumperTime` undefined-initialised, treated as 0) | osc `sine` -> gain. r once; `b = 720 + 120r`. f: set b @now, exp 1.5b @now+0.03, exp 0.8b @now+0.07. gain: set 0.16, exp 0.0001 @now+0.08. start now, stop now+0.085                                                                                           | 0.085 s      |
| `playDebrisSwallowed()`                                | ctx, !muted; no cooldown                          | `now - lastSwallowTime > 0.6` -> `chimeIndex = 0, queue = 0`, else `chimeIndex = (chimeIndex+1) % 11`, `queue++`. `lastSwallowTime = now`. `trig = now + min(queue*0.025, 0.2)`. Voice A: sine -> gain; f set 200 @trig, exp 560 @trig+0.05; gain set 0.12, exp 0.0001 @trig+0.06; start trig, stop trig+0.07. Voice B: sine -> gain; f set `scaleFreqs[chimeIndex]` @trig+0.01; gain set 0 @trig+0.01, linear 0.18 @trig+0.025, exp 0.0001 @trig+0.28; start trig+0.01, stop trig+0.3 | 0.3 s + stagger (max 0.5 s) |
| `playHazardExplosion()`                                | ctx, !muted; no cooldown                          | osc `triangle` -> gain. f: set 280, exp 55 @now+0.16. gain: set 0.18, exp 0.0001 @now+0.18. start now, stop now+0.2                                                                                                                                            | 0.2 s        |
| `playVictoryFanfare()`                                 | ctx, !muted; no cooldown                          | notes f/dur: 523.25/0.11, 659.25/0.11, 783.99/0.14, 1046.5/0.42; start offsets accumulate `t += dur*0.88`. Per note (start `t`): osc `triangle` -> gain; f set @t; gain set 0 @t, linear 0.2 @t+0.02, exp 0.0001 @t+dur; start t, stop t+dur+0.03                  | ~0.8 s       |
| `playDefeatSound()`                                    | same                                              | notes f/dur: 392/0.16, 329.63/0.16, 293.66/0.18, 261.63/0.38 (+exp f bend to 220 @t+dur); `t += dur*0.85`. Per note: osc `triangle` -> gain; gain set 0 @t, linear 0.18 @t+0.02, exp 0.0001 @t+dur; stop t+dur+0.03                                                  | ~0.85 s      |
| `playBackupPodSound()`                                 | same                                              | notes f/dur: 261.63/0.08, 392/0.08, 523.25/0.1, 659.25/0.28; `t += dur*0.75`. Per note: osc `sine` -> gain; gain set 0 @t, linear 0.2 @t+0.015, exp 0.0001 @t+dur; stop t+dur+0.02                                                                                  | ~0.5 s       |
| `setThrusterActive(on)`                                | none                                              | `on` -> `playSpinRatchet(1.2)`                                                                                                                                                                                                                                 | -            |

`scaleFreqs` = [261.63, 293.66, 329.63, 392, 440, 523.25, 587.33, 659.25, 783.99, 880, 1046.5] (11 entries, index wraps with `% 11`).

Every cue method first checks `!this.ctx || this.isMuted` and returns; all (but `playShredderPass`, `playCompactorPass`, `playSuperShredderPass`) then call `ensureRunning()`; only `playSpinRatchet`, `playDebrisCollision`, `playGatePass`, `playRcsThrust`, `playBumperBounce` consult a cooldown.

All oscillators have `detune = 0`; every oscillator frequency uses `setValueAtTime` + `exponentialRampToValueAtTime`, which sets the frequency param to a-rate sample accurate processing in Chromium.

## 4. Callers (`file:line`) and argument ranges

| Method                    | Callers                                                                                                                                  | Arguments                                                                                                                                |
| ------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `publicInit`              | `main.js` 289, 388, 423, 442, 462, 467, 472, 479, 485, 490, 563, 756, 765, 1152, 1245                                                  | -                                                                                                                                        |
| `playBoosterPass`         | `main.js` 290 (mascot click), 1153 (master bonus), 1246 (daily gift): no args; 716 (turbo boost): `ctx.currentTime`; via `playGatePass(BOOST)` | `now` absent or `ctx.currentTime`                                                                                                        |
| `setSfxVolume`            | `main.js` 371, 757                                                                                                                       | integer 0..100                                                                                                                           |
| `setAmbienceVolume`       | `main.js` 372, 766                                                                                                                       | integer 0..100 (`ambientVolume`; first call passes `s.ambientVolume`, default 70)                                                       |
| `toggleMute`              | `main.js` 443, 564, 747                                                                                                                  | -                                                                                                                                        |
| `playBackupPodSound`      | `main.js` 634                                                                                                                            | -                                                                                                                                        |
| `playVictoryFanfare`      | `main.js` 887                                                                                                                            | -                                                                                                                                        |
| `playDefeatSound`         | `main.js` 928                                                                                                                            | -                                                                                                                                        |
| `playSpinRatchet`         | `physics.js` 240 (`1.6`, boost), 289 (`min(1.5, 0.8 + spinSpeed*0.2)`, range 0.8..1.5); `setThrusterActive` (`1.2`)                      | pitch 0.8..1.6                                                                                                                           |
| `playDebrisSwallowed`     | `physics.js` 330                                                                                                                         | -                                                                                                                                        |
| `playDebrisCollision`     | `physics.js` 509 (`min(1, dot/70)`), 527 (`min(1, abs(dot)/70)`), 575 (`/65`), 636 (`/60`), 685 (`/65`)                                  | intensity in (0, 1]; type from `d.type`: `titanium`, `solar`, `volatile`, `magnetic` (`magnetic` falls in the default branch)             |
| `playBumperBounce`        | `physics.js` 746-747 (guarded by `this.audio && this.audio.playBumperBounce`)                                                            | -                                                                                                                                        |
| `playGatePass`            | `physics.js` 804-805                                                                                                                     | `(gate.type, gate.value)`: MULTIPLY value 2, 3, 4 (e.g. `sub >= 5 ? 4 : 3`); ADD value 3..15; BOOST value 160, 180                       |
| `playRcsThrust`           | `physics.js` 819 (after a BOOST gate)                                                                                                    | -                                                                                                                                        |
| `playHazardExplosion`     | `physics.js` 865                                                                                                                         | -                                                                                                                                        |
| `setThrusterActive`       | none                                                                                                                                     | boolean                                                                                                                                  |
| `playShredderPass`, `playCompactorPass`, `playSuperShredderPass` | only through `playGatePass`                                                                                          | `(now, value)`                                                                                                                           |

## 5. Random draws (SPEC section 4.6)

| Cue                   | Draws                                                                                    |
| --------------------- | ---------------------------------------------------------------------------------------- |
| `playDebrisCollision` | exactly 1, after the guards and cooldown; never in the cooldown/muted/no-ctx paths       |
| `playBumperBounce`    | exactly 1                                                                                |
| `playRcsThrust`       | 9702 (`floor(44100 * 0.22)`), per sample in index order, then the filter/gain graph      |
| everything else       | 0                                                                                        |

## 6. Reference scenario set (`cues.mjs`)

`render-ref.mjs` renders 72 scenarios (single cues with argument sweeps, cooldown/arpeggio/overlap sequences, two 3-second random mixes, volume and mute cases, sub-sample start offsets). `ref-manifest.json` records every call list. Three further scenarios (`pre_init_calls`, `init_late_cooldown`, `init_twice_then_cues`) are RNG/trace only (`rngOnly`).

Chromium renders are not bit-reproducible when several sources are summed (the order in which a node's connections are summed is not stable between runs); the manifest records `bitExactRepeat` and `repeatMaxAbsDiff` per entry (largest observed 1.8e-7).
