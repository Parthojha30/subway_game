/* ==========================================================================
   ENHANCED PROCEDURAL AUDIO SYNTHESIZER FOR SUBWAY SURFERS 3D
   ========================================================================== */

class SoundSystem {
    constructor() {
        this.ctx = null;
        this.muted = false;
        this.bgMusicGain = null;
        this.isPlayingBg = false;
        this.synthTimer = null;
    }

    init() {
        if (!this.ctx) {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            this.ctx = new AudioContext();
        }
        if (this.ctx.state === 'suspended') {
            this.ctx.resume();
        }
    }

    toggleMute() {
        this.muted = !this.muted;
        if (this.muted && this.bgMusicGain) {
            this.bgMusicGain.gain.setValueAtTime(0, this.ctx.currentTime);
        } else if (!this.muted && this.bgMusicGain) {
            this.bgMusicGain.gain.setValueAtTime(0.08, this.ctx.currentTime);
        }
        return this.muted;
    }

    playCoin() {
        if (this.muted) return;
        this.init();
        const now = this.ctx.currentTime;
        
        // Double tone crystal chime
        const osc1 = this.ctx.createOscillator();
        const osc2 = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc1.type = 'sine';
        osc2.type = 'triangle';

        osc1.frequency.setValueAtTime(1046.50, now); // C6
        osc1.frequency.exponentialRampToValueAtTime(1567.98, now + 0.08); // G6

        osc2.frequency.setValueAtTime(1318.51, now); // E6
        osc2.frequency.exponentialRampToValueAtTime(2093.00, now + 0.08); // C7

        gain.gain.setValueAtTime(0.18, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.15);

        osc1.connect(gain);
        osc2.connect(gain);
        gain.connect(this.ctx.destination);

        osc1.start(now);
        osc2.start(now);
        osc1.stop(now + 0.15);
        osc2.stop(now + 0.15);
    }

    playJump() {
        if (this.muted) return;
        this.init();
        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(180, now);
        osc.frequency.exponentialRampToValueAtTime(520, now + 0.22);

        gain.gain.setValueAtTime(0.25, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.22);

        osc.connect(gain);
        gain.connect(this.ctx.destination);

        osc.start(now);
        osc.stop(now + 0.22);
    }

    playSlide() {
        if (this.muted) return;
        this.init();
        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'triangle';
        osc.frequency.setValueAtTime(450, now);
        osc.frequency.exponentialRampToValueAtTime(110, now + 0.18);

        gain.gain.setValueAtTime(0.2, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.18);

        osc.connect(gain);
        gain.connect(this.ctx.destination);

        osc.start(now);
        osc.stop(now + 0.18);
    }

    playHoverboard() {
        if (this.muted) return;
        this.init();
        const now = this.ctx.currentTime;
        
        // Power-up chord arpeggio
        const freqs = [392.00, 493.88, 587.33, 783.99]; // G4, B4, D5, G5
        freqs.forEach((f, idx) => {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();

            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(f, now + idx * 0.05);

            gain.gain.setValueAtTime(0.15, now + idx * 0.05);
            gain.gain.exponentialRampToValueAtTime(0.001, now + idx * 0.05 + 0.2);

            osc.connect(gain);
            gain.connect(this.ctx.destination);

            osc.start(now + idx * 0.05);
            osc.stop(now + idx * 0.05 + 0.2);
        });
    }

    playJetpack() {
        if (this.muted) return;
        this.init();
        const now = this.ctx.currentTime;

        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(120, now);
        osc.frequency.linearRampToValueAtTime(600, now + 0.4);

        gain.gain.setValueAtTime(0.3, now);
        gain.gain.exponentialRampToValueAtTime(0.01, now + 0.45);

        osc.connect(gain);
        gain.connect(this.ctx.destination);

        osc.start(now);
        osc.stop(now + 0.45);
    }

    playCrash() {
        if (this.muted) return;
        this.init();
        const now = this.ctx.currentTime;

        // Low frequency thud + white noise burst
        const osc = this.ctx.createOscillator();
        const oscGain = this.ctx.createGain();

        osc.type = 'square';
        osc.frequency.setValueAtTime(120, now);
        osc.frequency.exponentialRampToValueAtTime(30, now + 0.35);

        oscGain.gain.setValueAtTime(0.4, now);
        oscGain.gain.exponentialRampToValueAtTime(0.01, now + 0.35);

        osc.connect(oscGain);
        oscGain.connect(this.ctx.destination);

        osc.start(now);
        osc.stop(now + 0.35);
    }

    startBgMusic() {
        if (this.isPlayingBg) return;
        this.init();
        this.isPlayingBg = true;

        const now = this.ctx.currentTime;
        const masterGain = this.ctx.createGain();
        masterGain.gain.setValueAtTime(this.muted ? 0 : 0.08, now);
        masterGain.connect(this.ctx.destination);
        this.bgMusicGain = masterGain;

        // Catchy, upbeat melodic arcade loop (C Major pentatonic scale)
        const melody = [
            261.63, 329.63, 392.00, 523.25,
            392.00, 329.63, 261.63, 392.00,
            293.66, 349.23, 440.00, 587.33,
            440.00, 349.23, 293.66, 440.00
        ];
        let noteIdx = 0;

        const playNextNote = () => {
            if (!this.isPlayingBg) return;
            const t = this.ctx.currentTime;

            // Bass note
            const bassOsc = this.ctx.createOscillator();
            const bassGain = this.ctx.createGain();
            bassOsc.type = 'triangle';
            bassOsc.frequency.setValueAtTime(melody[noteIdx % 4] / 2, t);
            bassGain.gain.setValueAtTime(0.3, t);
            bassGain.gain.exponentialRampToValueAtTime(0.01, t + 0.18);
            bassOsc.connect(bassGain);
            bassGain.connect(masterGain);
            bassOsc.start(t);
            bassOsc.stop(t + 0.18);

            // Lead Melody
            const leadOsc = this.ctx.createOscillator();
            const leadGain = this.ctx.createGain();
            leadOsc.type = 'sine';
            leadOsc.frequency.setValueAtTime(melody[noteIdx % melody.length], t);
            leadGain.gain.setValueAtTime(0.2, t);
            leadGain.gain.exponentialRampToValueAtTime(0.01, t + 0.18);
            leadOsc.connect(leadGain);
            leadGain.connect(masterGain);
            leadOsc.start(t);
            leadOsc.stop(t + 0.18);

            noteIdx++;
            this.synthTimer = setTimeout(playNextNote, 220);
        };

        playNextNote();
    }

    stopBgMusic() {
        this.isPlayingBg = false;
        if (this.synthTimer) {
            clearTimeout(this.synthTimer);
            this.synthTimer = null;
        }
    }
}

window.soundSystem = new SoundSystem();
