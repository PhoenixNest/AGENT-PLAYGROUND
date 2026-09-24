/**
 * Orbital Salvage: Cartoon Sound Synthesis Engine
 * Playful, bouncy procedural audio using Web Audio API.
 * Features toy ratchet wheel clicks, cute bubble gulps, arpeggiated pentatonic
 * marimba scales, and bouncy cartoon shredder sounds with master dynamics limiting.
 */

export class AudioEngine {
  constructor() {
    this.ctx = null;
    this.isMuted = false;
    this.isInitialized = false;

    // Master Audio Chain
    this.masterGain = null;
    this.masterCompressor = null;

    // Concurrency & Rate Limiting State
    this.lastCollisionTime = 0;
    this.lastRatchetTime = 0;
    this.lastGatePassTime = 0;
    this.lastSwallowTime = 0;
    this.swallowQueueCount = 0;

    // Pentatonic Major Scale (C4 to C6) - Bright, playful cartoon chime
    this.scaleFreqs = [
      261.63, // C4
      293.66, // D4
      329.63, // E4
      392.0, // G4
      440.0, // A4
      523.25, // C5
      587.33, // D5
      659.25, // E5
      783.99, // G5
      880.0, // A5
      1046.5, // C6
    ];
    this.chimeIndex = 0;
  }

  publicInit() {
    if (this.isInitialized) return;
    try {
      const AudioContext = window.AudioContext || window.webkitAudioContext;
      this.ctx = new AudioContext();

      // Master Compressor to prevent any distortion or audio spikes
      this.masterCompressor = this.ctx.createDynamicsCompressor();
      this.masterCompressor.threshold.setValueAtTime(-14, this.ctx.currentTime);
      this.masterCompressor.knee.setValueAtTime(10, this.ctx.currentTime);
      this.masterCompressor.ratio.setValueAtTime(8, this.ctx.currentTime);
      this.masterCompressor.attack.setValueAtTime(0.003, this.ctx.currentTime);
      this.masterCompressor.release.setValueAtTime(0.12, this.ctx.currentTime);

      this.masterGain = this.ctx.createGain();
      this.masterGain.gain.setValueAtTime(0.85, this.ctx.currentTime);

      this.masterCompressor.connect(this.masterGain);
      this.masterGain.connect(this.ctx.destination);

      this.isInitialized = true;
    } catch (e) {
      console.warn("Web Audio API not supported or blocked", e);
    }
  }

  toggleMute() {
    this.isMuted = !this.isMuted;
    if (this.ctx) {
      if (this.ctx.state === "suspended" && !this.isMuted) {
        this.ctx.resume();
      }
      if (this.masterGain) {
        this.masterGain.gain.setValueAtTime(
          this.isMuted ? 0 : 0.85,
          this.ctx.currentTime,
        );
      }
    }
    return this.isMuted;
  }

  /**
   * Cute cartoon ratchet click when the player spins the wheel.
   * Replaces the previous pink-noise radio hiss.
   */
  playSpinRatchet(pitchMultiplier = 1.0) {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;
    if (now - this.lastRatchetTime < 0.045) return; // Rate-limit ratchet clicks
    this.lastRatchetTime = now;

    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    const filter = this.ctx.createBiquadFilter();

    // Warm, crisp wooden toy click (triangle wave with high pitch pop)
    osc.type = "triangle";
    const baseFreq = 800 * Math.max(0.6, Math.min(1.8, pitchMultiplier));
    osc.frequency.setValueAtTime(baseFreq, now);
    osc.frequency.exponentialRampToValueAtTime(140, now + 0.022);

    filter.type = "bandpass";
    filter.frequency.setValueAtTime(1200, now);
    filter.Q.setValueAtTime(1.5, now);

    gain.gain.setValueAtTime(0.06, now);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.024);

    osc.connect(filter);
    filter.connect(gain);
    gain.connect(this.masterCompressor);

    osc.start(now);
    osc.stop(now + 0.026);
  }

  /**
   * Rate-limited cute toy tap for debris collisions.
   */
  playDebrisCollision(intensity = 1.0, debrisType = "titanium") {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;
    if (now - this.lastCollisionTime < 0.048) return; // Strict cooldown to prevent machine-gun clatter
    this.lastCollisionTime = now;

    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    let startFreq = 420;
    let endFreq = 180;
    let oscType = "sine";

    if (debrisType === "solar") {
      // Light playful bell tap
      startFreq = 620 + Math.random() * 80;
      endFreq = 340;
      oscType = "triangle";
    } else if (debrisType === "volatile") {
      // Bouncy rubber thud
      startFreq = 260 + Math.random() * 40;
      endFreq = 90;
      oscType = "sine";
    } else {
      // Cute wooden block tap
      startFreq = 460 + Math.random() * 60;
      endFreq = 160;
      oscType = "triangle";
    }

    osc.type = oscType;
    osc.frequency.setValueAtTime(startFreq, now);
    osc.frequency.exponentialRampToValueAtTime(endFreq, now + 0.038);

    const targetGain = Math.min(0.12 * intensity, 0.14);
    gain.gain.setValueAtTime(targetGain, now);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.04);

    osc.connect(gain);
    gain.connect(this.masterCompressor);

    osc.start(now);
    osc.stop(now + 0.042);
  }

  /**
   * Distinct audio signatures for the 4 gate archetypes:
   * 1. Rotary Shredder (Multiply x2, x3): Electric slicing buzz.
   * 2. Magnetic Compactor (Add +N): Deep resonant sub-bass magnetic pulse.
   * 3. Super Shredder (Multiply >= 4): Triumphant neon power-up chord.
   * 4. Gravity Booster: Cartoon jet whoosh.
   */
  playGatePass(gateType = "MULTIPLY", value = 2) {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;
    if (now - this.lastGatePassTime < 0.07) return; // Debounce multi-ball cascades
    this.lastGatePassTime = now;

    if (gateType === "BOOST") {
      this.playBoosterPass(now);
    } else if (gateType === "ADD") {
      this.playCompactorPass(now, value);
    } else if (value >= 4) {
      this.playSuperShredderPass(now, value);
    } else {
      this.playShredderPass(now, value);
    }
  }

  playShredderPass(now = (this.ctx ? this.ctx.currentTime : 0), value) {
    if (!this.ctx || this.isMuted) return;
    now = now || this.ctx.currentTime;
    // Electric cartoon slicing "Zzzt-Boing!"
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();
    const filter = this.ctx.createBiquadFilter();

    osc.type = "sawtooth";
    osc.frequency.setValueAtTime(450, now);
    osc.frequency.exponentialRampToValueAtTime(880, now + 0.08);

    filter.type = "lowpass";
    filter.frequency.setValueAtTime(1600, now);

    gain.gain.setValueAtTime(0.14, now);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.12);

    osc.connect(filter);
    filter.connect(gain);
    gain.connect(this.masterCompressor);

    osc.start(now);
    osc.stop(now + 0.14);
  }

  playCompactorPass(now = (this.ctx ? this.ctx.currentTime : 0), value) {
    if (!this.ctx || this.isMuted) return;
    now = now || this.ctx.currentTime;
    // Deep magnetic resonant chime (Sine with gentle sub-bass sweep)
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = "sine";
    osc.frequency.setValueAtTime(140, now);
    osc.frequency.exponentialRampToValueAtTime(320, now + 0.14);

    gain.gain.setValueAtTime(0.22, now);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.22);

    osc.connect(gain);
    gain.connect(this.masterCompressor);

    osc.start(now);
    osc.stop(now + 0.24);
  }

  playSuperShredderPass(now = (this.ctx ? this.ctx.currentTime : 0), value) {
    if (!this.ctx || this.isMuted) return;
    now = now || this.ctx.currentTime;
    // Dramatic power-up chord (Ascending dual sine oscillators)
    [523.25, 783.99, 1046.5].forEach((f, idx) => {
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = "triangle";
      osc.frequency.setValueAtTime(f, now + idx * 0.02);

      gain.gain.setValueAtTime(0, now + idx * 0.02);
      gain.gain.linearRampToValueAtTime(0.12, now + idx * 0.02 + 0.015);
      gain.gain.exponentialRampToValueAtTime(0.0001, now + idx * 0.02 + 0.18);

      osc.connect(gain);
      gain.connect(this.masterCompressor);

      osc.start(now + idx * 0.02);
      osc.stop(now + idx * 0.02 + 0.2);
    });
  }

  playBoosterPass(now) {
    if (!this.ctx || this.isMuted) return;
    const t = (isFinite(now) && now > 0) ? now : (this.ctx.currentTime || 0.001);
    // Comic jet whistle whoosh
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = "sine";
    osc.frequency.setValueAtTime(350, t);
    osc.frequency.exponentialRampToValueAtTime(1100, t + 0.12);

    gain.gain.setValueAtTime(0.16, t);
    gain.gain.exponentialRampToValueAtTime(0.0001, t + 0.15);

    osc.connect(gain);
    gain.connect(this.masterCompressor);

    osc.start(t);
    osc.stop(t + 0.17);
  }

  /**
   * Bouncy pinball bumper bell / rubber boing for deflector pins.
   */
  playBumperBounce() {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;
    if (now - (this.lastBumperTime || 0) < 0.045) return;
    this.lastBumperTime = now;

    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    // High-pitched playful pinball bell
    osc.type = "sine";
    const baseFreq = 720 + Math.random() * 120;
    osc.frequency.setValueAtTime(baseFreq, now);
    osc.frequency.exponentialRampToValueAtTime(baseFreq * 1.5, now + 0.03);
    osc.frequency.exponentialRampToValueAtTime(baseFreq * 0.8, now + 0.07);

    gain.gain.setValueAtTime(0.16, now);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.08);

    osc.connect(gain);
    gain.connect(this.masterCompressor);

    osc.start(now);
    osc.stop(now + 0.085);
  }

  /**
   * Nibbles swallows a piece of debris:
   * 1. Cute cartoon "Gulp!" / bubble pop.
   * 2. Pentatonic marimba note that cleanly arpeggiates up the scale.
   */
  playDebrisSwallowed() {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;

    // Stagger rapidly arriving debris to form a pleasing ascending arpeggio
    if (now - this.lastSwallowTime > 0.6) {
      this.chimeIndex = 0;
      this.swallowQueueCount = 0;
    } else {
      this.chimeIndex = (this.chimeIndex + 1) % this.scaleFreqs.length;
      this.swallowQueueCount++;
    }
    this.lastSwallowTime = now;

    // Small delay offset for rapid multi-swallows (staggered arpeggiation)
    const staggerOffset = Math.min(this.swallowQueueCount * 0.025, 0.2);
    const triggerTime = now + staggerOffset;

    // 1. Cute Bubble Pop / Gulp
    const popOsc = this.ctx.createOscillator();
    const popGain = this.ctx.createGain();
    popOsc.type = "sine";
    popOsc.frequency.setValueAtTime(200, triggerTime);
    popOsc.frequency.exponentialRampToValueAtTime(560, triggerTime + 0.05);

    popGain.gain.setValueAtTime(0.12, triggerTime);
    popGain.gain.exponentialRampToValueAtTime(0.0001, triggerTime + 0.06);

    popOsc.connect(popGain);
    popGain.connect(this.masterCompressor);
    popOsc.start(triggerTime);
    popOsc.stop(triggerTime + 0.07);

    // 2. Playful Pentatonic Marimba Chime
    const freq = this.scaleFreqs[this.chimeIndex];
    const xyloOsc = this.ctx.createOscillator();
    const xyloGain = this.ctx.createGain();

    xyloOsc.type = "sine";
    xyloOsc.frequency.setValueAtTime(freq, triggerTime + 0.01);

    xyloGain.gain.setValueAtTime(0, triggerTime + 0.01);
    xyloGain.gain.linearRampToValueAtTime(0.18, triggerTime + 0.025);
    xyloGain.gain.exponentialRampToValueAtTime(0.0001, triggerTime + 0.28);

    xyloOsc.connect(xyloGain);
    xyloGain.connect(this.masterCompressor);

    xyloOsc.start(triggerTime + 0.01);
    xyloOsc.stop(triggerTime + 0.3);
  }

  /**
   * Silly cartoon "Poof!" for volatile battery pops.
   */
  playHazardExplosion() {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;

    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = "triangle";
    osc.frequency.setValueAtTime(280, now);
    osc.frequency.exponentialRampToValueAtTime(55, now + 0.16);

    gain.gain.setValueAtTime(0.18, now);
    gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.18);

    osc.connect(gain);
    gain.connect(this.masterCompressor);

    osc.start(now);
    osc.stop(now + 0.2);
  }

  /**
   * Cheerful, bouncy cartoon fanfare jingle when completing a level.
   */
  playVictoryFanfare() {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;

    // Cheerful cartoon fanfare (C5 - E5 - G5 - C6)
    const melody = [
      { f: 523.25, dur: 0.11 }, // C5
      { f: 659.25, dur: 0.11 }, // E5
      { f: 783.99, dur: 0.14 }, // G5
      { f: 1046.5, dur: 0.42 }, // C6
    ];

    let t = now;
    melody.forEach((note) => {
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = "triangle";
      osc.frequency.setValueAtTime(note.f, t);

      gain.gain.setValueAtTime(0, t);
      gain.gain.linearRampToValueAtTime(0.2, t + 0.02);
      gain.gain.exponentialRampToValueAtTime(0.0001, t + note.dur);

      osc.connect(gain);
      gain.connect(this.masterCompressor);

      osc.start(t);
      osc.stop(t + note.dur + 0.03);

      t += note.dur * 0.88;
    });
  }

  // Retain for API compatibility, but thrusters now use pleasant spin clicks
  setThrusterActive(isActive) {
    if (isActive) {
      this.playSpinRatchet(1.2);
    }
  }
}
