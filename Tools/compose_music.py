"""Procedural composer for BOM Chess background music. Stdlib only.
Usage: python3 compose.py <outdir> [songname]
"""
import math, random, wave, array, sys, os

SR = 32000
TAU = 2 * math.pi
TABLE_N = 4096
SINE = [math.sin(TAU * i / TABLE_N) for i in range(TABLE_N)]

def mtof(m):
    return 440.0 * 2 ** ((m - 69) / 12.0)

class Mix:
    def __init__(self, seconds):
        self.n = int(seconds * SR)
        self.buf = array.array('f', bytes(4 * self.n))

    def add(self, start, samples, gain=1.0):
        s = int(start * SR)
        buf = self.buf
        n = self.n
        for i, v in enumerate(samples):
            j = s + i
            if j >= n:
                break
            buf[j] += v * gain

# ---------- instruments (return list of floats) ----------

def env_adsr(n, a, d, s, r, total):
    a = max(1, int(a * SR)); d = max(1, int(d * SR)); r = max(1, int(r * SR))
    hold = max(0, n - r)
    out = [0.0] * (n)
    for i in range(n):
        if i < a: e = i / a
        elif i < a + d: e = 1 - (1 - s) * (i - a) / d
        else: e = s
        if i >= hold: e *= max(0.0, 1 - (i - hold) / r)
        out[i] = e
    return out

def additive(freq, dur, harmonics, a=0.01, d=0.2, s=0.7, r=0.3, vib=0.0, vibrate=5.0, bright_decay=0.0):
    n = int((dur + r) * SR)
    env = env_adsr(n, a, d, s, r, dur)
    out = [0.0] * n
    for h, amp in harmonics:
        f = freq * h
        if f > SR / 2.2:
            continue
        ph = random.random() * TABLE_N
        inc = f * TABLE_N / SR
        vinc = vibrate * TABLE_N / SR
        vph = 0.0
        bd = bright_decay * (h - 1)
        for i in range(n):
            if vib:
                vph += vinc
                ph += inc * (1 + vib * SINE[int(vph) % TABLE_N] * min(1.0, i / (SR * 0.4)))
            else:
                ph += inc
            a_ = amp
            if bd:
                a_ = amp * math.exp(-bd * i / SR)
            out[i] += SINE[int(ph) % TABLE_N] * a_
    return [o * e for o, e in zip(out, env)]

def pad(freq, dur):
    # detuned soft strings/choir
    n = int((dur + 1.5) * SR)
    out = [0.0] * n
    for det in (-0.004, 0.0, 0.005):
        part = additive(freq * (1 + det), dur, [(1, .5), (2, .25), (3, .12), (4, .06), (5, .03)],
                        a=min(1.2, dur * 0.4), d=0.5, s=0.8, r=1.5, vib=0.003, vibrate=4.7)
        for i in range(min(n, len(part))):
            out[i] += part[i]
    return [v / 3 for v in out]

def choir(freq, dur):
    n = int((dur + 1.5) * SR)
    out = [0.0] * n
    for det in (-0.006, 0.0, 0.006):
        # "aah" formant-ish emphasis on harmonics 2-4
        part = additive(freq * (1 + det), dur, [(1, .35), (2, .3), (3, .25), (4, .12), (6, .05)],
                        a=min(1.0, dur * 0.35), d=0.4, s=0.85, r=1.5, vib=0.006, vibrate=5.3)
        for i in range(min(n, len(part))):
            out[i] += part[i]
    return [v / 3 for v in out]

def brass(freq, dur):
    return additive(freq, dur, [(1, .5), (2, .4), (3, .3), (4, .2), (5, .12), (6, .08), (7, .05)],
                    a=0.06, d=0.25, s=0.75, r=0.25, vib=0.004, vibrate=5.5, bright_decay=0.6)

def flute(freq, dur):
    out = additive(freq, dur, [(1, .8), (2, .12), (3, .06)], a=0.08, d=0.1, s=0.85, r=0.25,
                   vib=0.006, vibrate=5.0)
    # breath
    for i in range(min(len(out), int(0.08 * SR))):
        out[i] += (random.random() - .5) * 0.05 * (1 - i / (0.08 * SR))
    return out

def pluck(freq, dur, damp=0.996, bright=0.5):
    # Karplus-Strong harp / plucked lute
    period = max(2, int(SR / freq))
    ring = [(random.random() * 2 - 1) for _ in range(period)]
    # soften initial noise
    for k in range(int(3 * (1 - bright)) + 1):
        ring = [(ring[i] + ring[i - 1]) * 0.5 for i in range(period)]
    n = int((dur + 1.2) * SR)
    out = [0.0] * n
    idx = 0
    for i in range(n):
        nxt = (idx + 1) % period
        v = ring[idx]
        ring[idx] = damp * 0.5 * (v + ring[nxt])
        out[i] = v
        idx = nxt
    rel = int(0.3 * SR); stop = int(dur * SR) + int(0.9 * SR)
    for i in range(stop, n):
        out[i] *= max(0.0, 1 - (i - stop) / rel)
    return out

def bass(freq, dur):
    return additive(freq, dur, [(1, .9), (2, .3), (3, .1)], a=0.01, d=0.3, s=0.6, r=0.15)

def strings_stac(freq, dur):
    return additive(freq, dur, [(1, .5), (2, .3), (3, .2), (4, .12), (5, .08), (6, .05)],
                    a=0.015, d=0.12, s=0.3, r=0.08, bright_decay=1.5)

def kick(vol=1.0):
    n = int(0.45 * SR); out = [0.0] * n; ph = 0.0
    for i in range(n):
        t = i / SR
        f = 45 + 90 * math.exp(-t * 25)
        ph += f / SR
        out[i] = math.sin(TAU * ph) * math.exp(-t * 7) * vol
    return out

def taiko(vol=1.0, pitch=70):
    n = int(0.9 * SR); out = [0.0] * n; ph = 0.0; lp = 0.0
    for i in range(n):
        t = i / SR
        f = pitch * (1 + 0.6 * math.exp(-t * 30))
        ph += f / SR
        lp += 0.08 * ((random.random() * 2 - 1) - lp)
        out[i] = (math.sin(TAU * ph) * 0.9 + lp * 1.5 * math.exp(-t * 20)) * math.exp(-t * 4.5) * vol
    return out

def snare(vol=1.0):
    n = int(0.25 * SR); out = [0.0] * n; prev = 0.0
    for i in range(n):
        t = i / SR
        w = random.random() * 2 - 1
        hp = w - prev; prev = w
        out[i] = (hp * 0.5 + math.sin(TAU * 190 * t) * 0.4 * math.exp(-t * 30)) * math.exp(-t * 18) * vol
    return out

def frame_drum(vol=1.0):
    n = int(0.35 * SR); out = [0.0] * n; lp = 0.0
    for i in range(n):
        t = i / SR
        lp += 0.25 * ((random.random() * 2 - 1) - lp)
        out[i] = (lp * 0.8 + math.sin(TAU * 150 * t) * 0.5) * math.exp(-t * 14) * vol
    return out

def shaker(vol=1.0):
    n = int(0.08 * SR); out = [0.0] * n; prev = 0.0
    for i in range(n):
        w = random.random() * 2 - 1
        out[i] = (w - prev) * 0.5 * math.sin(math.pi * i / n) * vol; prev = w
    return out

def cymbal(vol=1.0):
    n = int(2.5 * SR); out = [0.0] * n; prev = 0.0
    for i in range(n):
        w = random.random() * 2 - 1
        out[i] = (w - prev) * 0.3 * math.exp(-i / SR * 1.6) * vol; prev = w
    return out

# ---------- effects ----------

def reverb(buf, wet=0.25, size=1.0):
    n = len(buf)
    combs = [(int(d * size * SR / 1000), g) for d, g in ((29.7, .80), (37.1, .78), (41.1, .76), (43.7, .74))]
    acc = array.array('f', bytes(4 * n))
    for d, g in combs:
        line = array.array('f', bytes(4 * n))
        for i in range(n):
            v = buf[i] + (line[i - d] * g if i >= d else 0.0)
            line[i] = v
            acc[i] += v
    for d, g in ((int(5.0 * SR / 1000), .7), (int(1.7 * SR / 1000), .7)):
        out = array.array('f', bytes(4 * n))
        for i in range(n):
            x = acc[i]
            yd = out[i - d] if i >= d else 0.0
            xd = acc[i - d] if i >= d else 0.0
            out[i] = -g * x + xd + g * yd
        acc = out
    res = array.array('f', bytes(4 * n))
    for i in range(n):
        res[i] = buf[i] * (1 - wet) + acc[i] * wet * 0.25
    return res

def write_wav(path, buf, fade_in=0.5, fade_out=4.0):
    n = len(buf)
    peak = max(1e-6, max(abs(v) for v in buf))
    g = 0.89 / peak
    fi = int(fade_in * SR); fo = int(fade_out * SR)
    data = array.array('h', bytes(2 * n))
    for i in range(n):
        v = buf[i] * g
        if i < fi: v *= i / fi
        if i > n - fo: v *= max(0.0, (n - i) / fo)
        # gentle soft clip
        v = math.tanh(v * 1.1) / math.tanh(1.1)
        data[i] = int(max(-1, min(1, v)) * 32000)
    with wave.open(path, 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(data.tobytes())

# ---------- composition helpers ----------

# chord = (root midi, quality) ; quality intervals
Q = {'m': (0, 3, 7), 'M': (0, 4, 7), 'sus': (0, 5, 7), 'm7': (0, 3, 7, 10), 'M7': (0, 4, 7, 11), '5': (0, 7, 12)}

def chord_notes(root, q):
    return [root + i for i in Q[q]]

def melody_line(rng, scale, chord, beats, octave_base, last, rhythm_choices):
    """Generate a phrase of (beat_offset, length_beats, midi) over one chord."""
    tones = []
    for o in (0, 12, 24):
        tones += [octave_base + s + o for s in scale]
    ctones = set(c % 12 for c in chord)
    out = []
    t = 0.0
    rhythm = rng.choice(rhythm_choices)
    ri = 0
    while t < beats - 1e-6:
        L = min(rhythm[ri % len(rhythm)], beats - t); ri += 1
        # prefer chord tones on strong beats, stepwise motion otherwise
        cands = [p for p in tones if abs(p - last) <= 5] or tones
        strong = abs(t - round(t)) < 1e-6 and int(round(t)) % 2 == 0
        weights = []
        for p in cands:
            w = 1.0
            if p % 12 in ctones: w *= 4.0 if strong else 1.5
            d = abs(p - last)
            w *= {0: 0.4, 1: 3, 2: 3, 3: 2, 4: 1.5, 5: 1}.get(d, 0.5)
            weights.append(w)
        p = rng.choices(cands, weights)[0]
        out.append((t, L, p))
        last = p
        t += L
    return out, last

# ---------- songs ----------

def render_song(spec, seed):
    rng = random.Random(seed)
    random.seed(seed)
    bpm = spec['bpm']; beat = 60.0 / bpm
    bpb = spec.get('beats_per_bar', 4)
    prog = spec['prog']               # list of (root, q) per bar
    form = spec['form']               # list of section names
    total_bars = len(prog) * len(form)
    seconds = total_bars * bpb * beat + 6
    mix = Mix(seconds)
    scale = spec['scale']; tonic = spec['tonic']
    last = tonic + 12 + spec.get('mel_oct', 12)
    motif_cache = {}
    bar = 0
    for sec_i, sec in enumerate(form):
        layers = spec['sections'][sec]
        for ci, (root, q) in enumerate(prog):
            t0 = bar * bpb * beat
            cn = chord_notes(root, q)
            barlen = bpb * beat
            if 'pad' in layers:
                for p in cn:
                    mix.add(t0, pad(mtof(p), barlen * 0.98), 0.10 * layers['pad'])
            if 'choir' in layers:
                for p in cn:
                    mix.add(t0, choir(mtof(p + 12), barlen * 0.98), 0.09 * layers['choir'])
            if 'bass' in layers:
                pat = spec.get('bass_pat', [(0, bpb)])
                for (b, L) in pat:
                    mix.add(t0 + b * beat, bass(mtof(root - 12), L * beat * 0.95), 0.30 * layers['bass'])
            if 'arp' in layers:
                inst = spec.get('arp_inst', 'pluck')
                step = spec.get('arp_step', 0.5)
                order = spec.get('arp_order', [0, 1, 2, 3, 2, 1])
                ext = cn + [cn[0] + 12, cn[1] + 12]
                k = 0; tt = 0.0
                while tt < bpb - 1e-6:
                    p = ext[order[k % len(order)] % len(ext)] + spec.get('arp_oct', 12)
                    if inst == 'pluck':
                        s = pluck(mtof(p), step * beat, damp=0.995)
                        g = 0.16
                    else:
                        s = strings_stac(mtof(p), step * beat * 0.8)
                        g = 0.10
                    acc = 1.2 if abs(tt - round(tt)) < 1e-6 else 0.85
                    mix.add(t0 + tt * beat, s, g * acc * layers['arp'])
                    tt += step; k += 1
            if 'mel' in layers:
                inst = {'flute': flute, 'brass': brass, 'pluck': pluck}[spec.get('mel_inst', 'flute')]
                # motif reuse: same melody returns for same chord index on repeated sections
                key = (spec['sections'][sec].get('mel_id', sec), ci)
                if key not in motif_cache:
                    motif_cache[key], last = melody_line(rng, scale, cn, bpb, tonic + spec.get('mel_oct', 12),
                                                         last, spec['rhythms'])
                for (b, L, p) in motif_cache[key]:
                    g = {'flute': 0.17, 'brass': 0.13, 'pluck': 0.22}[spec.get('mel_inst', 'flute')]
                    mix.add(t0 + b * beat, inst(mtof(p), L * beat * 0.92), g * layers['mel'])
            if 'drums' in layers:
                for (b, kind, v) in spec['drum_pat']:
                    fn = {'kick': kick, 'taiko': taiko, 'snare': snare, 'frame': frame_drum,
                          'shaker': shaker, 'lowtaiko': lambda vol: taiko(vol, 50)}[kind]
                    vv = v * (0.9 + 0.2 * rng.random())
                    mix.add(t0 + b * beat, fn(vv), 0.35 * layers['drums'])
            if ci == 0 and layers.get('crash'):
                mix.add(t0, cymbal(1.0), 0.12)
            bar += 1
    # final sustained tonic chord
    tend = bar * bpb * beat
    for p in chord_notes(tonic, spec['final_q']):
        mix.add(tend, pad(mtof(p), 3.5), 0.12)
    if 'drums_end' in spec:
        mix.add(tend, taiko(1.0, 55), 0.4)
    return reverb(mix.buf, wet=spec.get('wet', 0.3), size=spec.get('room', 1.2))

# Scales relative to tonic
DORIAN = [0, 2, 3, 5, 7, 9, 10]
AEOLIAN = [0, 2, 3, 5, 7, 8, 10]
HARM_MIN = [0, 2, 3, 5, 7, 8, 11]
PHRYG = [0, 1, 3, 5, 7, 8, 10]
MAJOR = [0, 2, 4, 5, 7, 9, 11]
MIXO = [0, 2, 4, 5, 7, 9, 10]

SONGS = {
    # ---------------- Main menu: calm, regal, wondrous ----------------
    'Plates of Gold': dict(
        bpm=76, tonic=50, scale=DORIAN, final_q='m',
        prog=[(50, 'm'), (48, 'M'), (55, 'M'), (50, 'm'), (46, 'M'), (48, 'M'), (45, 'm'), (50, 'sus')],
        form=['intro', 'A', 'B', 'A', 'outro'],
        sections={'intro': {'pad': 1, 'arp': 0.8},
                  'A': {'pad': 1, 'arp': 1, 'mel': 1, 'bass': 0.7, 'mel_id': 'A'},
                  'B': {'pad': 1, 'arp': 1, 'mel': 1, 'bass': 0.8, 'choir': 0.6, 'drums': 0.5, 'mel_id': 'B'},
                  'outro': {'pad': 1, 'arp': 0.7}},
        arp_inst='pluck', arp_step=0.5, arp_order=[0, 1, 2, 3, 4, 3, 2, 1], arp_oct=12,
        mel_inst='flute', mel_oct=24, rhythms=[[1, 1, 2], [2, 1, 1], [1.5, 0.5, 2], [1, 0.5, 0.5, 2]],
        bass_pat=[(0, 4)], drum_pat=[(0, 'frame', 0.8), (2.5, 'frame', 0.5), (3, 'frame', 0.6)],
        wet=0.38, room=1.4),
    'Land of Promise': dict(
        bpm=68, tonic=53, scale=MAJOR, final_q='M',
        prog=[(53, 'M'), (58, 'M'), (50, 'm'), (48, 'M'), (53, 'M'), (46, 'M'), (48, 'sus'), (48, 'M')],
        form=['intro', 'A', 'A', 'B', 'A'],
        sections={'intro': {'pad': 1, 'choir': 0.5},
                  'A': {'pad': 1, 'mel': 1, 'bass': 0.6, 'arp': 0.6, 'mel_id': 'A'},
                  'B': {'pad': 1, 'choir': 0.9, 'mel': 1, 'bass': 0.8, 'arp': 0.8, 'crash': 1, 'mel_id': 'B'}},
        arp_inst='pluck', arp_step=1.0, arp_order=[0, 2, 1, 3], arp_oct=0,
        mel_inst='flute', mel_oct=24, rhythms=[[2, 2], [1, 1, 2], [3, 1], [2, 1, 1]],
        bass_pat=[(0, 2), (2, 2)], wet=0.42, room=1.5),
    # ---------------- Duel: tense, rhythmic, determined ----------------
    'Title of Liberty': dict(
        bpm=104, tonic=50, scale=AEOLIAN, final_q='5',
        prog=[(50, 'm'), (50, 'm'), (46, 'M'), (48, 'M'), (50, 'm'), (43, 'm'), (45, 'M'), (45, 'M')],
        form=['intro', 'A', 'B', 'A', 'B'],
        sections={'intro': {'drums': 1, 'arp': 0.8, 'bass': 0.8},
                  'A': {'drums': 1, 'arp': 1, 'bass': 1, 'mel': 1, 'pad': 0.6, 'mel_id': 'A'},
                  'B': {'drums': 1, 'arp': 1, 'bass': 1, 'mel': 1, 'choir': 0.8, 'crash': 1, 'mel_id': 'B'}},
        arp_inst='str', arp_step=0.5, arp_order=[0, 0, 2, 0, 1, 0, 2, 3], arp_oct=0,
        mel_inst='brass', mel_oct=12, rhythms=[[1, 1, 1, 1], [1.5, 0.5, 2], [0.5, 0.5, 1, 2], [2, 1, 1]],
        bass_pat=[(0, 1), (1.5, 0.5), (2, 1), (3, 1)],
        drum_pat=[(0, 'taiko', 1), (1, 'snare', 0.5), (1.5, 'snare', 0.3), (2, 'taiko', 0.8),
                  (2.75, 'snare', 0.4), (3, 'snare', 0.6), (3.5, 'taiko', 0.5)],
        wet=0.25, room=1.1),
    'Clash at Zarahemla': dict(
        bpm=116, tonic=52, scale=PHRYG, final_q='5',
        prog=[(52, '5'), (53, 'M'), (52, '5'), (50, 'm'), (48, 'M'), (53, 'M'), (50, 'm'), (52, '5')],
        form=['intro', 'A', 'A', 'B', 'A'],
        sections={'intro': {'drums': 1, 'arp': 1},
                  'A': {'drums': 1, 'arp': 1, 'bass': 1, 'mel': 1, 'mel_id': 'A'},
                  'B': {'drums': 1, 'arp': 0.8, 'bass': 1, 'mel': 1, 'pad': 0.8, 'choir': 0.7, 'crash': 1, 'mel_id': 'B'}},
        arp_inst='str', arp_step=0.25, arp_order=[0, 0, 3, 0, 0, 2, 0, 1], arp_oct=0,
        mel_inst='brass', mel_oct=12, rhythms=[[1, 0.5, 0.5, 2], [0.5, 0.5, 0.5, 0.5, 2], [2, 2], [1, 1, 2]],
        bass_pat=[(0, 0.5), (0.75, 0.25), (1, 0.5), (2, 0.5), (2.75, 0.25), (3, 1)],
        drum_pat=[(0, 'taiko', 1), (0.75, 'frame', 0.5), (1, 'kick', 0.7), (2, 'taiko', 0.9),
                  (2.5, 'frame', 0.5), (3, 'kick', 0.7), (3.5, 'frame', 0.6), (3.75, 'frame', 0.4)]
                 + [(i * 0.5, 'shaker', 0.6) for i in range(8)],
        wet=0.22, room=1.0, drums_end=True),
    # ---------------- Nephites' Last Stand: dark, desperate, epic ----------------
    'Cumorah': dict(
        bpm=72, tonic=48, scale=HARM_MIN, final_q='m',
        prog=[(48, 'm'), (44, 'M'), (41, 'm'), (43, 'M'), (48, 'm'), (46, 'M'), (44, 'M'), (43, 'M')],
        form=['intro', 'A', 'B', 'B', 'outro'],
        sections={'intro': {'choir': 0.8, 'pad': 0.8, 'drums': 0.6},
                  'A': {'choir': 1, 'pad': 1, 'bass': 1, 'mel': 1, 'drums': 0.8, 'mel_id': 'A'},
                  'B': {'choir': 1, 'pad': 1, 'bass': 1, 'mel': 1, 'arp': 0.8, 'drums': 1, 'crash': 1, 'mel_id': 'B'},
                  'outro': {'choir': 0.9, 'pad': 1, 'drums': 0.5}},
        arp_inst='str', arp_step=0.5, arp_order=[0, 1, 2, 1], arp_oct=0,
        mel_inst='brass', mel_oct=12, rhythms=[[2, 2], [3, 1], [1, 1, 2], [4]],
        bass_pat=[(0, 3), (3, 1)],
        drum_pat=[(0, 'lowtaiko', 1), (1.5, 'taiko', 0.5), (2, 'lowtaiko', 0.8), (3, 'taiko', 0.6), (3.5, 'taiko', 0.4)],
        wet=0.4, room=1.5, drums_end=True),
    'The Last Watch': dict(
        bpm=132, tonic=45, scale=HARM_MIN, final_q='5', beats_per_bar=6,
        prog=[(45, 'm'), (45, 'm'), (41, 'M'), (43, 'M'), (45, 'm'), (38, 'm'), (40, 'M'), (40, 'M')],
        form=['intro', 'A', 'B', 'A', 'B'],
        sections={'intro': {'drums': 1, 'arp': 1, 'bass': 0.8},
                  'A': {'drums': 1, 'arp': 1, 'bass': 1, 'mel': 1, 'pad': 0.7, 'mel_id': 'A'},
                  'B': {'drums': 1, 'arp': 1, 'bass': 1, 'mel': 1, 'choir': 1, 'crash': 1, 'mel_id': 'B'}},
        arp_inst='str', arp_step=0.5, arp_order=[0, 1, 2, 3, 2, 1], arp_oct=12,
        mel_inst='brass', mel_oct=12, rhythms=[[3, 3], [2, 1, 3], [1, 1, 1, 3], [3, 2, 1]],
        bass_pat=[(0, 2), (3, 2), (5, 1)],
        drum_pat=[(0, 'taiko', 1), (2, 'frame', 0.5), (3, 'taiko', 0.8), (4, 'snare', 0.4), (5, 'snare', 0.6), (5.5, 'snare', 0.4)],
        wet=0.28, room=1.2, drums_end=True),
}

if __name__ == '__main__':
    outdir = sys.argv[1]
    names = sys.argv[2:] or list(SONGS)
    for i, name in enumerate(names):
        buf = render_song(SONGS[name], seed=hash(name) & 0xffff if False else sum(map(ord, name)))
        path = os.path.join(outdir, name + '.wav')
        write_wav(path, buf)
        print('wrote', path, round(len(buf) / SR, 1), 's')
