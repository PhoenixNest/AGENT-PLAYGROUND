/**
 * Orbital Salvage: Cartoon Sound Synthesis Engine
 * Playful, bouncy procedural audio using Web Audio API.
 * Features cartoon boings, bubbly pops, slurps, xylophone scales, and cheerful fanfares.
 */

export class AudioEngine {
  constructor() {
    this.ctx = null;
    this.isMuted = false;
    this.isInitialized = false;

    // Ambient nodes
    this.ambientGain = null;
    this.thrusterGain = null;
    this.thrusterNoise = null;

    // Cheerful Xylophone / Marimba Scale (Pentatonic Major in C/G)
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
    this.lastChimeTime = 0;
  }

  publicInit() {
    if (this.isInitialized) return;
    try {
      const AudioContext = window.AudioContext || window.webkitAudioContext;
      this.ctx = new AudioContext();
      this.isInitialized = true;
      this.setupPlayfulHum();
      this.setupThrusterWhoosh();
    } catch (e) {
      console.warn("Web Audio API not supported or blocked", e);
    }
  }

  toggleMute() {
    this.isMuted = !this.isMuted;
    if (this.ctx && this.ctx.state === "suspended" && !this.isMuted) {
      this.ctx.resume();
    }
    return this.isMuted;
  }

  setupPlayfulHum() {
    if (!this.ctx) return;

    // Soft warm cartoon background chord (F major 7th warm pad)
    const freqs = [174.61, 220.0, 261.63, 329.63]; // F3, A3, C4, E4
    this.ambientGain = this.ctx.createGain();
    this.ambientGain.gain.value = 0.05;

    freqs.forEach((f) => {
      const osc = this.ctx.createOscillator();
      const filter = this.ctx.createBiquadFilter();

      osc.type = "sine";
      osc.frequency.value = f;

      filter.type = "lowpass";
      filter.frequency.value = 280;

      osc.connect(filter);
      filter.connect(this.ambientGain);
      osc.start();
    });

    this.ambientGain.connect(this.ctx.destination);
  }

  setupThrusterWhoosh() {
    if (!this.ctx) return;

    // 1-second pink noise buffer for cartoon air whooshes
    const bufferSize = this.ctx.sampleRate;
    const noiseBuffer = this.ctx.createBuffer(
      1,
      bufferSize,
      this.ctx.sampleRate,
    );
    const output = noiseBuffer.getChannelData(0);
    let b0 = 0,
      b1 = 0,
      b2 = 0;
    for (let i = 0; i < bufferSize; i++) {
      const white = Math.random() * 2 - 1;
      b0 = 0.99765 * b0 + white * 0.05;
      b1 = 0.963 * b1 + white * 0.11;
      b2 = 0.57 * b2 + white * 0.25;
      output[i] = (b0 + b1 + b2) * 0.4;
    }

    const noise = this.ctx.createBufferSource();
    noise.buffer = noiseBuffer;
    noise.loop = true;

    const filter = this.ctx.createBiquadFilter();
    filter.type = "bandpass";
    filter.frequency.value = 900;
    filter.Q.value = 1.8;

    this.thrusterGain = this.ctx.createGain();
    this.thrusterGain.gain.value = 0;

    noise.connect(filter);
    filter.connect(this.thrusterGain);
    this.thrusterGain.connect(this.ctx.destination);

    noise.start();
  }

  setThrusterActive(isActive) {
    if (!this.ctx || this.isMuted || !this.thrusterGain) return;
    const now = this.ctx.currentTime;
    const targetGain = isActive ? 0.14 : 0;
    this.thrusterGain.gain.cancelScheduledValues(now);
    this.thrusterGain.gain.linearRampToValueAtTime(targetGain, now + 0.05);
  }

  playDebrisCollision(intensity = 1.0) {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;

    // Cartoon clack / wooden block tap
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = "sine";
    const startFreq = 480 + Math.random() * 150;
    osc.frequency.setValueAtTime(startFreq, now);
    osc.frequency.exponentialRampToValueAtTime(120, now + 0.06);

    gain.gain.setValueAtTime(Math.min(0.25 * intensity, 0.25), now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.05);

    osc.connect(gain);
    gain.connect(this.ctx.destination);

    osc.start(now);
    osc.stop(now + 0.06);
  }

  playGatePass() {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;

    // Cartoon Spring "Boing!" / Comic Pitch Slide
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = "triangle";
    osc.frequency.setValueAtTime(280, now);
    osc.frequency.exponentialRampToValueAtTime(750, now + 0.12);
    osc.frequency.linearRampToValueAtTime(620, now + 0.18);

    gain.gain.setValueAtTime(0.2, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.2);

    osc.connect(gain);
    gain.connect(this.ctx.destination);

    osc.start(now);
    osc.stop(now + 0.22);
  }

  playDebrisSwallowed() {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;

    // 1. Cute "GULP!" / Bubble Pop
    const popOsc = this.ctx.createOscillator();
    const popGain = this.ctx.createGain();
    popOsc.type = "sine";
    popOsc.frequency.setValueAtTime(180, now);
    popOsc.frequency.exponentialRampToValueAtTime(520, now + 0.07);
    popGain.gain.setValueAtTime(0.22, now);
    popGain.gain.exponentialRampToValueAtTime(0.001, now + 0.08);

    popOsc.connect(popGain);
    popGain.connect(this.ctx.destination);
    popOsc.start(now);
    popOsc.stop(now + 0.09);

    // 2. Cheerful Xylophone Chime climbing the scale
    if (now - this.lastChimeTime > 0.7) {
      this.chimeIndex = 0;
    } else {
      this.chimeIndex = (this.chimeIndex + 1) % this.scaleFreqs.length;
    }
    this.lastChimeTime = now;

    const freq = this.scaleFreqs[this.chimeIndex];
    const xyloOsc = this.ctx.createOscillator();
    const xyloGain = this.ctx.createGain();

    xyloOsc.type = "sine";
    xyloOsc.frequency.setValueAtTime(freq, now + 0.02);

    xyloGain.gain.setValueAtTime(0, now);
    xyloGain.gain.setValueAtTime(0.25, now + 0.02);
    xyloGain.gain.exponentialRampToValueAtTime(0.001, now + 0.32);

    xyloOsc.connect(xyloGain);
    xyloGain.connect(this.ctx.destination);

    xyloOsc.start(now + 0.02);
    xyloOsc.stop(now + 0.35);
  }

  playHazardExplosion() {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;

    // Silly cartoon "Poof!" / Balloon Pop
    const osc = this.ctx.createOscillator();
    const gain = this.ctx.createGain();

    osc.type = "sawtooth";
    osc.frequency.setValueAtTime(380, now);
    osc.frequency.exponentialRampToValueAtTime(60, now + 0.2);

    gain.gain.setValueAtTime(0.28, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.22);

    osc.connect(gain);
    gain.connect(this.ctx.destination);

    osc.start(now);
    osc.stop(now + 0.24);
  }

  playVictoryFanfare() {
    if (!this.ctx || this.isMuted) return;
    const now = this.ctx.currentTime;

    // Cheerful, bouncy cartoon fanfare jingle (C - E - G - C6 with bouncy vibrato)
    const melody = [
      { f: 523.25, dur: 0.12 }, // C5
      { f: 659.25, dur: 0.12 }, // E5
      { f: 783.99, dur: 0.15 }, // G5
      { f: 1046.5, dur: 0.45 }, // C6
    ];

    let t = now;
    melody.forEach((note) => {
      const osc = this.ctx.createOscillator();
      const gain = this.ctx.createGain();

      osc.type = "triangle";
      osc.frequency.setValueAtTime(note.f, t);

      gain.gain.setValueAtTime(0, t);
      gain.gain.linearRampToValueAtTime(0.25, t + 0.03);
      gain.gain.exponentialRampToValueAtTime(0.001, t + note.dur);

      osc.connect(gain);
      gain.connect(this.ctx.destination);

      osc.start(t);
      osc.stop(t + note.dur + 0.05);

      t += note.dur * 0.85;
    });
  }
}
