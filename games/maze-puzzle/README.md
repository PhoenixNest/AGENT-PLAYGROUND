# Orbital Salvage: Maze Puzzle Project Repository

**Documentation & Design Archive**

Welcome to the project repository for the **Space Debris Cleanup Puzzle Game** (working title: _Orbital Salvage: Event Horizon_), derived from the mechanical deconstruction of _Multi Maze 3D_ and elevated into a distinct, high-concept, non-monetized indie title.

---

## 📁 Repository Contents

| File                                                                                             | Description                                                 | Focus Areas                                                                                                                             |
| :----------------------------------------------------------------------------------------------- | :---------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
| [`01_REFERENCE_GAME_ANALYSIS.md`](./01_REFERENCE_GAME_ANALYSIS.md)                               | Reverse-engineering of _Multi Maze 3D_                      | Core mechanics, physics loops, multiplier gates, collection counters, pseudocode, and complete UML models.                              |
| [`02_APP_STORE_SIMILARITY_AND_GUIDELINE_4_3.md`](./02_APP_STORE_SIMILARITY_AND_GUIDELINE_4_3.md) | Competitive landscape & Apple Review Guideline 4.3 analysis | App Store ecosystem scan, rejection vectors under 4.3(a)/(b), and concrete technical/gameplay differentiation strategies.               |
| [`03_GAME_DESIGN_DOCUMENT.md`](./03_GAME_DESIGN_DOCUMENT.md)                                     | Game Design Document (GDD) for Space Trash Cleanup          | Narrative lore, thematic mappings, zero-monetization salvage economy, cosmetic unlocks, daily orbital contracts, and technical systems. |
| [`04_TECHNICAL_ARCHITECTURE.md`](./04_TECHNICAL_ARCHITECTURE.md)                                 | Technical Architecture Specification                        | iOS 430pt × 932pt viewport geometry, physics engine pipeline, level sanitization algorithms, and failure recovery architecture.         |
| [`05_GAME_UI_UX_DESIGN_SPECIFICATION.md`](./05_GAME_UI_UX_DESIGN_SPECIFICATION.md)               | Game UI/UX Design Specification                             | "Cute Cosmic Janitor" design tokens, typography, mobile ergonomics, screen-by-screen wireframes, modal debriefs, and haptic feedback.   |
| [`06_GAMEPLAY_SYSTEMS_AND_LEVEL_DESIGN.md`](./06_GAMEPLAY_SYSTEMS_AND_LEVEL_DESIGN.md)           | Gameplay Systems & Level Design Specification               | Centrifuge kinematics, rigid body material traits, shredder gate archetypes, obstacle catalog, and 50-level campaign world matrix.      |
| [`07_AUDIO_DESIGN_SPECIFICATION.md`](./07_AUDIO_DESIGN_SPECIFICATION.md)                         | Audio Design Specification & Technical Sound Guide          | "Tactile Toybox Skeuomorphism" aesthetic, procedural Web Audio DSP synthesis chains, acoustic cue catalog, and haptic synchronization.  |
| [`CHANGELOG.md`](./CHANGELOG.md)                                                                 | Milestone Release History & Version Changelog               | Semantic versioning changelog tracing all milestone releases, feature additions, bug fixes, and commit hashes.                          |
| [`web/`](./web/)                                                                                 | Playable Mobile Web Prototype                               | Modern HTML5 Canvas/Web Audio game implementation adhering to iOS 430pt × 932pt ergonomics with 50 progressive campaign sectors.        |

---

## 🚀 Quick Summary of Project Direction

- **Core Gameplay Foundation:** Satisfying rotational physics puzzle where players rotate orbital centrifuges via tactile gestures to guide space scrap through multiplier shredders and funnel them into Nibbles the hungry cartoon black hole.
- **Thematic Overhaul:**
  - Balls $\rightarrow$ **Orbital Space Debris / Satellite Scrap** (Titanium brackets, solar cells, volatile batteries).
  - Maze $\rightarrow$ **Orbital Ring Debris Chutes / Salvage Station Centrifuges**.
  - Collection Cup $\rightarrow$ **Nibbles the Micro-Black Hole** (Animated cartoon eyes, cheeks, dynamic growth, and gulping mouth).
  - Daily Missions $\rightarrow$ **Orbital Sector Cleaning Contracts (Daily Shift)**.
  - Reward Economy $\rightarrow$ **Scrap Credits / In-Game Salvage Tokens** (strictly non-monetized; earned solely via skill and clearance volume).
- **Apple App Store Guideline 4.3 Compliance:** A complete divergence from generic hypercasual tropes by adding rich sci-fi lore, physics variations (gravity pulses, microgravity drift, deflector bumpers), distinct hazard elements, non-linear progression, and zero invasive ads or predatory paywalls.
- **Mobile-First iOS Architecture:**
  - Scaled natively for the iPhone 16 Pro Max viewport ($430\text{pt} \times 932\text{pt}$).
  - Hardware safe area insets for Dynamic Island and Home indicator bar.
  - Apple HIG minimum 44pt touch targets on all interactive controls.
- **Running the Prototype Locally:**
  ```bash
  cd games/maze-puzzle/web
  npx --yes http-server . -p 8080 -c-1
  ```
  Then navigate to `http://localhost:8080/`.
