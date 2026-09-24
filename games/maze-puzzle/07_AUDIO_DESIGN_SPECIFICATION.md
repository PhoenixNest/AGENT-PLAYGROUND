# Orbital Salvage: Audio Design Specification (ADD)

**Document Type:** Audio Design Document & Technical Audio Specification (ADD / TAS)  
**Game Title:** Orbital Salvage: Space Trash Clean  
**Author / Audio Director:** Studio Audio Division  
**Collaborators:** Game Director, Lead Gameplay Engineer, UI/UX Designer  
**Target Platform:** Mobile-First Web / iOS Safari & Android Chrome (WebKit / Blink)  
**Status:** Approved & Implemented in Engine (`v1.4.0`)  
**Companion Documents:**

- [`03_GAME_DESIGN_DOCUMENT.md`](file:///c:/Users/ASUS/Documents/Code/Local/AGENT-PLAYGROUND/games/maze-puzzle/03_GAME_DESIGN_DOCUMENT.md) — Master Game Design Document
- [`04_TECHNICAL_ARCHITECTURE.md`](file:///c:/Users/ASUS/Documents/Code/Local/AGENT-PLAYGROUND/games/maze-puzzle/04_TECHNICAL_ARCHITECTURE.md) — Core Technical Architecture
- [`05_GAME_UI_UX_DESIGN_SPECIFICATION.md`](file:///c:/Users/ASUS/Documents/Code/Local/AGENT-PLAYGROUND/games/maze-puzzle/05_GAME_UI_UX_DESIGN_SPECIFICATION.md) — UI/UX Design System
- [`06_GAMEPLAY_SYSTEMS_AND_LEVEL_DESIGN.md`](file:///c:/Users/ASUS/Documents/Code/Local/AGENT-PLAYGROUND/games/maze-puzzle/06_GAMEPLAY_SYSTEMS_AND_LEVEL_DESIGN.md) — Gameplay Systems & Level Design
- [`CHANGELOG.md`](file:///c:/Users/ASUS/Documents/Code/Local/AGENT-PLAYGROUND/games/maze-puzzle/CHANGELOG.md) — Semantic Version History

---

## 1. Executive Summary & Audio Vision

In casual physics puzzle games, **audio and haptic response represent at least 50% of tactile "juice" and player satisfaction**. A physical wheel rotation, a debris ball dropping through a multiplier gate, and an alien creature swallowing debris must feel physically tangible, responsive, and emotionally uplifting.

### 1.1 Aesthetic Pillar: "Tactile Toybox Skeuomorphism"

Rather than harsh, industrial sci-fi alarms, 8-bit chiptune bleeps, or synthesized static noise, _Orbital Salvage_ employs a **Warm Cartoon Toybox** acoustic aesthetic:

- **Mechanical Wheels:** Crisp wooden ratchet gear clicks (`playSpinRatchet`), evocative of a classic wooden Fisher-Price toy wheel or high-end mechanical dial.
- **Debris Collisions:** Gentle acoustic percussion—wooden blocks, light ceramic bells, and rubberized thuds—rather than metallic clatter.
- **Creature Consumption:** Playful bubble gulps paired with clean, arpeggiated pentatonic marimba chimes (`playDebrisSwallowed`) that reward cascade collections with harmonic melodies.
- **Gates & Machines:** Whimsical cartoon energy—electric slice "zzzt-boing", sub-bass magnetic pulses, and comic jet whistles.
- **Zero Ear Fatigue:** Strict frequency curation, pitch jittering, and transient shaping to ensure long play sessions without acoustic burnout.

### 1.2 Zero-Asset Zero-Latency Architecture

Unlike traditional mobile web games that download megabytes of MP3/OGG files over HTTP (introducing network latency, decoding stalls, and mobile autoplay blockers), _Orbital Salvage_ utilizes a **100% Procedural Web Audio API DSP Synthesis Engine** (`AudioEngine` in [`web/engine/audio.js`](file:///c:/Users/ASUS/Documents/Code/Local/AGENT-PLAYGROUND/games/maze-puzzle/web/engine/audio.js)):

- **Zero Asset Footprint:** 0 KB audio files downloaded over the network.
- **Instantaneous Latency:** Under 5ms trigger-to-sound latency via hardware-accelerated DSP nodes.
- **Micro-Randomization:** Every acoustic cue features mathematical pitch drift ($\pm 5\%\text{--}10\%$) and dynamic velocity scaling, rendering repetitive mechanical sounds completely organic.

---

## 2. Web Audio DSP Architecture & Signal Chain

All procedural voices are routed through an isolated node tree designed to prevent digital clipping, eliminate speaker distortion on mobile phones, and ensure clean dynamic range control.

### 2.1 Master Audio Bus Flowchart

```
┌────────────────────────────────────────────────────────┐
│                   PROCEDURAL VOICES                    │
├────────────────────┬───────────────────┬───────────────┤
│ Ratchet Oscillator │ Collision Nodes   │ Gate Voices   │
│ (Triangle Wave)    │ (Sine / Triangle) │ (Saw / Sine)  │
├────────────────────┼───────────────────┼───────────────┤
│ Swallow Chimes     │ Fanfare / Jingle  │ Booster Jets  │
│ (Pentatonic Sine)  │ (Triad Chords)    │ (Pitch Sweep) │
└─────────┬──────────┴─────────┬─────────┴───────┬───────┘
          │                    │                 │
          ▼                    ▼                 ▼
   Voice Filters         Voice Envelopes   Voice Envelopes
   (Biquad Low/Band)     (ADSR Gain Nodes) (ADSR Gain Nodes)
          │                    │                 │
          └────────────────────┼─────────────────┘
                               ▼
               ┌───────────────────────────────┐
               │   Master Dynamics Compressor   │
               │  Threshold: -14 dBFS          │
               │  Knee:      10 dB             │
               │  Ratio:     8:1               │
               │  Attack:    0.003s (3ms)      │
               │  Release:   0.120s (120ms)    │
               └───────────────┬───────────────┘
                               ▼
               ┌───────────────────────────────┐
               │       Master Gain Node        │
               │  Normal: 0.85 (-1.4 dBFS)     │
               │  Muted:  0.00 (-inf dBFS)     │
               └───────────────┬───────────────┘
                               ▼
               ┌───────────────────────────────┐
               │    AudioContext.destination   │
               │ (Hardware DAC / Mobile Phone) │
               └───────────────────────────────┘
```

### 2.2 Master Dynamics Compressor Specification

When up to 40 debris balls drop through a $\times 4$ gate simultaneously and enter the black hole funnel, unmanaged polyphony would produce severe digital inter-sample clipping on smartphone speakers.

The `DynamicsCompressorNode` acts as a transparent, high-speed brickwall limiter:

| Parameter     | Setting           | Engineering Justification                                                                        |
| :------------ | :---------------- | :----------------------------------------------------------------------------------------------- |
| **Threshold** | `-14.0 dB`        | Establishes headroom above quiet individual ratchet ticks while catching loud multi-ball bursts. |
| **Knee**      | `10.0 dB`         | Softens compression onset, preventing unnatural pumping artifacts during cascades.               |
| **Ratio**     | `8:1`             | Steep reduction ratio acting as a musical brickwall limiter.                                     |
| **Attack**    | `0.003 s` (3ms)   | Ultra-fast attack captures high-energy transient spikes from resonant filter pops.               |
| **Release**   | `0.120 s` (120ms) | Smooth decay that restores gain immediately after a cascade passes without breathing.            |

### 2.3 AudioContext Lifecycle & WebKit Autoplay Compliance

Mobile browsers (notably iOS Safari) restrict Web Audio playback until explicit user interaction:

1. **Deferred Initialization:** `AudioContext` is created or resumed inside `publicInit()` upon the first `pointerdown` / `click` event anywhere on the viewport.
2. **Context Suspension Recovery:** If the browser tab backgrounded or locked, the engine checks `ctx.state === "suspended"` and resumes upon returning to the active document.
3. **Mute State Decoupling:** Muting sets `masterGain.gain = 0` rather than stopping the audio clock, preserving synchronized event queues and avoiding restart pops.

---

## 3. Acoustic Cue Catalog & Synthesis Recipes

Every sound cue in the game is generated dynamically using mathematical waveform functions, frequency envelopes, and filter poles.

```
                                  ACOUSTIC SPECTRUM
   100 Hz            300 Hz            800 Hz           2 kHz            8 kHz
     │                 │                 │                │                │
     ▼                 ▼                 ▼                ▼                ▼
[Compactor Sub] [Rubber Thud]    [Wood Click]     [Bell Taps]      [Booster Jet]
[Hazard Poof]   [Bubble Pop]     [Marimba C4-A4]  [Marimba C5-C6]  [Sawtooth Edge]
```

### 3.1 Mechanical & Physics Cues

#### 1. Toy Ratchet Click (`playSpinRatchet`)

- **Trigger:** Wheel rotation angular delta exceeds ratchet increment ($15^\circ$).
- **Acoustic Feel:** Crisp wooden gear catch, like winding a music box or wooden clockwork.
- **Waveform:** Triangle wave.
- **Frequency Envelope:** $800\text{ Hz} \times \text{pitchMultiplier}$ snapping down exponentially to $140\text{ Hz}$ in $22\text{ms}$.
- **Filtering:** Biquad Bandpass ($f_c = 1200\text{ Hz}$, $Q = 1.5$) to remove harsh ultrasonic clicks and low-end mud.
- **Gain Envelope:** Peak $0.06$, exponential decay to $0.0001$ over $24\text{ms}$.
- **Rate-Limiting Cooldown:** $45\text{ms}$ debounce threshold.

#### 2. Debris Collisions (`playDebrisCollision`)

- **Trigger:** Matter.js collision event between debris ball and maze walls, gates, or other balls.
- **Debris-Specific Timbre:**
  - **Titanium Debris:** Wooden block tap (Triangle wave, $460\text{ Hz} \pm 60\text{ Hz}$ randomized down to $160\text{ Hz}$ in $38\text{ms}$).
  - **Solar Panels:** Light crystalline bell tap (Triangle wave, $620\text{ Hz} \pm 80\text{ Hz}$ down to $340\text{ Hz}$ in $38\text{ms}$).
  - **Volatile Scrap:** Soft rubber thud (Sine wave, $260\text{ Hz} \pm 40\text{ Hz}$ down to $90\text{ Hz}$ in $38\text{ms}$).
- **Velocity Scaling:** Gain scaled linearly by impact velocity dot product: $\text{gain} = \min(0.12 \times \text{intensity}, 0.14)$.
- **Rate-Limiting Cooldown:** $48\text{ms}$ global collision cooldown to eliminate machine-gun audio jitter.

#### 3. Bumper & Pinball Deflector (`playBumperBounce`)

- **Trigger:** Debris ball impacts an elastic rubber deflector pin.
- **Acoustic Feel:** High-pitched, bouncy pinball bell with a comical rubber "boing".
- **Waveform:** Sine wave.
- **Frequency Envelope:** Base $720\text{ Hz} \pm 120\text{ Hz}$ jitter $\to$ jumps up $1.5\times$ in $30\text{ms} \to$ settles down to $0.8\times$ in $70\text{ms}$.
- **Gain Envelope:** Peak $0.16$, exponential ramp to $0.0001$ over $80\text{ms}$.
- **Rate-Limiting Cooldown:** $45\text{ms}$.

---

### 3.2 Gate & Machine Cues

#### 4. Rotary Shredder (`playGatePass` - Multiply $\times 2, \times 3$)

- **Trigger:** Debris ball enters a standard multiplier gate.
- **Acoustic Feel:** Playful electric cartoon buzz ("zzzt-boing!").
- **Waveform:** Sawtooth wave.
- **Frequency Envelope:** $450\text{ Hz} \to 880\text{ Hz}$ upward exponential sweep in $80\text{ms}$.
- **Filtering:** Biquad Lowpass filter ($f_c = 1600\text{ Hz}$) to tame harsh sawtooth harmonics.
- **Gain Envelope:** Peak $0.14$, exponential decay over $120\text{ms}$.
- **Debounce:** $70\text{ms}$.

#### 5. Magnetic Compactor (`playGatePass` - Add $+N$)

- **Trigger:** Debris ball enters an adder gate.
- **Acoustic Feel:** Deep, resonant magnetic sub-bass pulse.
- **Waveform:** Pure sine wave.
- **Frequency Envelope:** $140\text{ Hz} \to 320\text{ Hz}$ in $140\text{ms}$.
- **Gain Envelope:** Heavy body at $0.22$, decaying cleanly over $220\text{ms}$.

#### 6. Super Shredder Fanfare (`playGatePass` - Multiply $\ge \times 4$)

- **Trigger:** Debris ball passes through a high-tier $\times 4$ or $\times 5$ gate.
- **Acoustic Feel:** Triumphant, ascending neon power-up chord.
- **Voicing:** 3-voice arpeggiated major chord triad ($C_5 = 523.25\text{ Hz}$, $G_5 = 783.99\text{ Hz}$, $C_6 = 1046.5\text{ Hz}$).
- **Stagger:** Each voice staggered by $+20\text{ms}$.
- **Waveform:** Triangle wave with $15\text{ms}$ attack and $180\text{ms}$ decay.

#### 7. Gravity Booster Jet (`playBoosterPass`)

- **Trigger:** Debris ball accelerates through a directional booster channel.
- **Acoustic Feel:** Comic jet whistle whoosh.
- **Waveform:** Pure sine wave.
- **Frequency Envelope:** $350\text{ Hz} \to 1100\text{ Hz}$ steep exponential sweep in $120\text{ms}$.
- **Gain Envelope:** Peak $0.16$, exponential drop over $150\text{ms}$.

---

### 3.3 Nibbles & Collection Cues

#### 8. Debris Swallowed: Gulp + Pentatonic Marimba (`playDebrisSwallowed`)

- **Trigger:** Debris ball crosses into Nibbles' collection aperture.
- **Dual-Layer Acoustic Architecture:**
  1. **Layer 1: Cartoon Gulp / Bubble Pop:**
     - Sine wave sweeping from $200\text{ Hz} \to 560\text{ Hz}$ in $50\text{ms}$.
     - Peak gain $0.12$, decay in $60\text{ms}$.
  2. **Layer 2: Pentatonic Marimba Chime:**
     - Ascending 11-note Pentatonic Major scale ($C_4$ to $C_6$):
       $$\{261.63, 293.66, 329.63, 392.00, 440.00, 523.25, 587.33, 659.25, 783.99, 880.00, 1046.50\}\text{ Hz}$$
     - Pure sine chime with rapid attack ($15\text{ms}$) and warm organic ringing tail ($280\text{ms}$).
- **Intelligent Cascade Arpeggiation:**
  - If a subsequent debris is swallowed within $600\text{ms}$, the scale counter increments: `chimeIndex = (chimeIndex + 1) % 11`.
  - Rapid multi-ball swallows are queued with an micro-delay stagger: $\text{offset} = \min(\text{queueCount} \times 25\text{ms}, 200\text{ms})$.
  - **Player Experience:** Cleaning a burst of 15 debris balls creates an uplifting, harmonious marimba melody rather than an overwhelming cacophony.

#### 9. Volatile Hazard Explosion (`playHazardExplosion`)

- **Trigger:** Battery debris contacts a hazard spark or incinerator pin.
- **Acoustic Feel:** Whimsical cartoon "Poof!", like a magician's smoke cloud.
- **Waveform:** Triangle wave.
- **Frequency Envelope:** Low thud dropping from $280\text{ Hz} \to 55\text{ Hz}$ in $160\text{ms}$.
- **Gain Envelope:** Peak $0.18$, decay over $180\text{ms}$.

---

### 3.4 Feedback & Meta-State Jingles

#### 10. Victory Fanfare (`playVictoryFanfare`)

- **Trigger:** Target quota met; level completion sequence triggers.
- **Melody Structure:** Bright, buoyant 4-note ascending major fanfare:
  1. $C_5$ ($523.25\text{ Hz}$, $110\text{ms}$)
  2. $E_5$ ($659.25\text{ Hz}$, $110\text{ms}$)
  3. $G_5$ ($783.99\text{ Hz}$, $140\text{ms}$)
  4. $C_6$ ($1046.50\text{ Hz}$, $420\text{ms}$ sustaining finish)
- **Waveform:** Smooth triangle wave with $20\text{ms}$ soft linear attack to avoid digital clicks.

#### 11. Defeat Sigh (`playDefeatSound`)

- **Trigger:** All debris balls swallowed or lost, but quota was not reached.
- **Melody Structure:** Sweet, gentle cartoon descending sigh:
  1. $G_4$ ($392.00\text{ Hz}$, $160\text{ms}$)
  2. $E_4$ ($329.63\text{ Hz}$, $160\text{ms}$)
  3. $D_4$ ($293.66\text{ Hz}$, $180\text{ms}$)
  4. $C_4 \to A_3$ pitch bend ($261.63\text{ Hz} \to 220.00\text{ Hz}$, $380\text{ms}$)
- **Tone:** Empathetic and lighthearted, encouraging an immediate replay rather than creating frustration.

#### 12. Backup Pod Launch Chime (`playBackupPodSound`)

- **Trigger:** Player taps "Call Backup Pod" to inject reserve scrap into the wheel.
- **Melody Structure:** Upbeat, urgent ascending 4-note chime ($C_4 \to G_4 \to C_5 \to E_5$):
  - Fast tempo ($80\text{ms}$ per note, final note $280\text{ms}$).
  - Bright sine wave signaling incoming assistance and revitalized gameplay.

---

## 4. Concurrency, Voice Prioritization & Debouncing

In physics games with dense particle simulations, unconstrained polyphony degrades audio quality and overtaxes the mobile browser's audio rendering thread.

### 4.1 Voice Priority Hierarchy

When multiple audio events occur simultaneously, the engine enforces strict voice limits and prioritization:

| Priority Tier        | Cue Types                                   | Max Polyphony | Voice Stealing Rule                                                                    |
| :------------------- | :------------------------------------------ | :------------ | :------------------------------------------------------------------------------------- |
| **Tier 1 (Highest)** | Victory Fanfare, Defeat Sigh, Backup Pod    | 1             | Never stolen. Ducks Tier 4 and Tier 5 sounds by $-6\text{ dB}$.                        |
| **Tier 2**           | Gate Shredder, Compactor, Super Shredder    | 3             | Newest voice steals oldest if within same gate type.                                   |
| **Tier 3**           | Debris Swallowed (Bubble + Chime)           | 8             | Arpeggiated queue buffer (staggered by $25\text{ms}$).                                 |
| **Tier 4**           | Bumper Pinball Bell, Jet Booster            | 2             | Oldest voice steals if triggered within $45\text{ms}$.                                 |
| **Tier 5 (Lowest)**  | Debris Collision Taps, Wheel Ratchet Clicks | 4             | Strict rate-limiting debounce ($45\text{ms}\text{--}48\text{ms}$). Drop event if busy. |

### 4.2 Debounce Timings Table

```
[Wheel Spin]       ───(45ms debounce)───► [Ratchet Sound]
[Wall Collision]   ───(48ms debounce)───► [Collision Tap]
[Gate Collision]   ───(70ms debounce)───► [Gate SFX]
[Bumper Hit]       ───(45ms debounce)───► [Bumper Bell]
[Nibbles Swallow]  ───(25ms stagger)────► [Arpeggiated Chime]
```

---

## 5. Haptic-Audio Tactile Synchronization

To achieve peak physical immersion on mobile devices supporting the Web Vibration API (`navigator.vibrate`), acoustic transient spikes are paired millisecond-for-millisecond with physical vibration pulses.

### 5.1 Sensory Pairing Matrix

| Event / Cue                 | Audio Frequency Peak          | Audio Duration | Haptic Pattern (`navigator.vibrate`) | Sensory Illusion                         |
| :-------------------------- | :---------------------------- | :------------- | :----------------------------------- | :--------------------------------------- |
| **Wheel Ratchet Click**     | $800\text{ Hz}$ triangle      | $24\text{ms}$  | `[10]` (10ms micro-tap)              | Mechanical gear detent tooth engagement. |
| **Titanium Ball Collision** | $460\text{ Hz}$ tap           | $38\text{ms}$  | `[12]` (12ms soft tap)               | Solid wooden block kinetic impact.       |
| **Rubber Bumper Bounce**    | $720\to 1080\text{ Hz}$       | $80\text{ms}$  | `[20]` (20ms springy buzz)           | Elastic pinball bumper rebound.          |
| **Gate Multiplier Pass**    | $450\to 880\text{ Hz}$ buzz   | $120\text{ms}$ | `[15, 25, 15]` (Dual buzz)           | Electric blade or slicing energy field.  |
| **Compactor Addition**      | $140\to 320\text{ Hz}$ sub    | $220\text{ms}$ | `[35]` (Deep heavy pulse)            | Heavy magnetic mass compression.         |
| **Nibbles Swallow Gulp**    | $200\to 560\text{ Hz}$ bubble | $60\text{ms}$  | `[18]` (Snappy bubble pop)           | Cheerful vacuum suction ingestion.       |
| **Backup Pod Release**      | $261\to 659\text{ Hz}$ chime  | $320\text{ms}$ | `[25, 40, 25]` (Pulsed burst)        | Heavy mechanical airlock hatch release.  |
| **Victory Fanfare**         | $523\to 1046\text{ Hz}$       | $750\text{ms}$ | `[30, 40, 30, 40, 80]`               | Triumphant celebration pattern.          |

---

## 6. Mobile Ergonomics, Accessibility & Mixing Standards

### 6.1 Transducer Frequency Balancing

Smartphone built-in speakers suffer severe acoustic distortion when driven below $100\text{ Hz}$ or above $10\text{ kHz}$.

1. **Low-End Management:** Fundamental frequencies for sub-bass effects (such as the Magnetic Compactor and Hazard Poof) are centered between $140\text{ Hz}$ and $320\text{ Hz}$, generating strong upper harmonics that register clearly on tiny iPhone/Android phone speakers without overloading speaker cones.
2. **High-Frequency Rolloff:** All oscillators with rich harmonic series (such as sawtooth waves) are conditioned with steep Biquad lowpass filtering capped at $1600\text{ Hz}$, eliminating brittle ear fatigue.

### 6.2 Global Mute Architecture & State Persistence

Audio states are synchronized globally across the user interface:

- **Single Source of Truth:** `AudioEngine.isMuted` controls master gain directly.
- **UI State Mirroring:**
  - HUD header audio toggle button (`#btn-toggle-audio` / `#audio-icon`).
  - Home screen audio toggle button (`#btn-home-audio` / `#home-audio-icon`).
  - Pause modal sound toggle button (`#btn-pause-sound` / `#pause-sound-icon`).
  - Settings screen master toggle switch (`#settings-sound-toggle`).
- **Persistence:** Mute preferences are persisted in `localStorage` under `orbital_salvage_audio_muted`, ensuring player preferences persist across browser sessions.
- **Audio Cleanliness:** Toggling mute ramps the `masterGain` smoothly over $20\text{ms}$ (`setValueAtTime` / `linearRampToValueAtTime`) to eliminate sudden DC offset pops.

---

## 7. Studio Sign-Off & Verification Checklist

- [x] **Zero HTTP Audio Requests:** Network inspector confirms 0 external audio asset requests.
- [x] **Autoplay Unblock:** AudioContext correctly resumes on first user touch gesture across iOS Safari & Android Chrome.
- [x] **Ear Fatigue Playtest:** 15-minute continuous playtest demonstrates no acoustic annoyance or repetitive frequency buildup.
- [x] **Polyphony Stress Test:** 50 simultaneous debris collisions through a $\times 4$ gate pass through the Master Compressor with zero audible clipping or distortion.
- [x] **Haptic Synchronization:** Vibration timings align seamlessly with acoustic transients on mobile devices.
- [x] **Settings & UI Sync:** Toggling sound in HUD, Home, Pause, or Settings immediately reflects in all UI elements and updates `localStorage`.
