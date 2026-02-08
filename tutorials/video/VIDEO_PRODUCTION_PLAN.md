# Framewerk Video Tutorial Production Plan

## Executive Summary

**Content:** 8 episodes, ~100-120 minutes total runtime  
**Scripts:** Already written (~16,500 words of narration)  
**Target:** Unity developers learning Framewerk  
**Goal:** Professional-quality tutorials with minimal budget  

---

## 1. Recommended Approach: Hybrid AI + Screen Recording

After researching all options, I recommend a **hybrid approach**:

| Component | Tool | Why |
|-----------|------|-----|
| **Voice** | Chatterbox (open-source) or ElevenLabs | Natural TTS, voice cloning optional |
| **Screen Recording** | OBS Studio | Free, reliable, Unity-friendly |
| **Animations/Graphics** | Motion Canvas | Built for code tutorials, free |
| **Final Editing** | DaVinci Resolve Free | Professional-grade, no cost |

### Why NOT AI Avatars (HeyGen/Synthesia)?

- **Overkill for coding tutorials** — viewers watch the screen, not a talking head
- **Expensive** — HeyGen starts at $29/mo, Synthesia at $22/mo
- **Mismatch** — AI avatars excel at corporate comms, not technical walkthroughs
- **Your scripts already specify "minimal talking head"** — they're designed for screen capture

---

## 2. Voice/Narration Options

### Option A: Chatterbox TTS (Recommended for Budget)

**What:** Open-source TTS that rivals ElevenLabs quality  
**Cost:** FREE (MIT license, runs locally)  
**Quality:** 64% preferred over ElevenLabs in blind tests  
**Features:**
- Zero-shot voice cloning (5 seconds of audio)
- Emotion control (monotone to expressive)
- Sub-200ms latency
- 23+ languages

**Setup:**
```bash
pip install chatterbox-tts
# Clone a voice with 5 sec sample, or use built-in voices
```

**Pros:**
- Free forever, no character limits
- Runs on your M4 Mac Mini
- Voice cloning included
- No API costs

**Cons:**
- Requires local setup
- Fewer voice options than ElevenLabs

### Option B: ElevenLabs (Best Quality)

**Cost:** $22/mo Creator plan (100K characters/month = ~100 min TTS)  
**What you get:**
- Professional voice cloning
- 192 kbps audio quality
- Huge voice library

**For 8 episodes (~16K words ≈ 80-90K characters):**
- One month at Creator tier covers it all
- Total cost: **~$22-44** for the whole series

**Pros:**
- Industry-leading quality
- Easy web interface
- Excellent for technical content

**Cons:**
- Monthly subscription
- Character limits

### Option C: Descript (Voice + Editing Combo)

**What:** Video editor with built-in voice cloning  
**Cost:** $15/mo Pro plan  
**Unique feature:** "Overdub" — record 30 min of your voice, then type corrections and it regenerates in your voice

**Best for:** If you or Matej want to narrate but hate re-recording mistakes

---

## 3. Screen Recording Setup for Unity

### Hardware
- **Resolution:** Record at 1920×1080 (your Mac outputs 2560×1440, so scale down or crop)
- **Frame rate:** 60 FPS (smooth UI interactions)
- **Audio:** Separate track (add voiceover in post)

### OBS Studio Settings (Recommended)

```
Settings > Video:
  Base Resolution: 1920x1080
  Output Resolution: 1920x1080
  FPS: 60

Settings > Output > Recording:
  Recording Format: MKV (remux to MP4 later)
  Encoder: Apple VT H.264
  Rate Control: CRF
  CRF Value: 18-20 (high quality)
  Keyframe Interval: 2
```

### Unity Editor Prep
1. **Clean layout** — Hide unnecessary panels
2. **Large fonts** — Edit > Preferences > UI Scaling (150%+)
3. **Consistent theme** — Pro skin (dark) for consistency
4. **Hide personal info** — Window titles, recent files, etc.
5. **Disable notifications** — macOS DND during recording

### Recording Workflow
1. Run through each section once without recording (rehearsal)
2. Record each section separately (easier to fix mistakes)
3. Leave 3-second gaps between takes
4. Use clapboard markers (keystroke beeps) for sync

---

## 4. Graphics & Animations with Motion Canvas

### Why Motion Canvas?

Your scripts include complex diagrams like MVCS architecture, dependency flow, and code transformations. Motion Canvas is **built exactly for this**:

- **Code syntax highlighting** — Built-in support for C# and 50+ languages
- **Animated code changes** — Shows diffs, insertions, deletions smoothly
- **Diagram animations** — Boxes, arrows, flow charts
- **Open source** — Free, TypeScript-based
- **Export to video** — Integrates into your pipeline

### Example: Animating Code Transformation

From your `00-foundations-script.md`:
```
Left side (Bad):
void Start() {
    scoreManager = FindObjectOfType<ScoreManager>();
    playerData = PlayerData.Instance;
}

Right side (Good):
[Inject] public IScoreModel ScoreModel { get; set; }
[Inject] public IPlayerData PlayerData { get; set; }
```

In Motion Canvas:
```typescript
yield* code().code.edit(0.6)`\
${remove(`void Start() {
    scoreManager = FindObjectOfType<ScoreManager>();
    playerData = PlayerData.Instance;
}`)}${insert(`[Inject] public IScoreModel ScoreModel { get; set; }
[Inject] public IPlayerData PlayerData { get; set; }`)}`;
```

### What to Create in Motion Canvas

Based on script analysis:
1. **MVCS diagram** — Four quadrants with animations (Ep 0)
2. **Dependency injection flow** — Context → Classes (Ep 0)
3. **Restaurant analogy** — Simple character animation (Ep 0)
4. **Context stack visualization** — App/Scene/Screen contexts (Ep 0)
5. **Command sequence diagrams** — Request → Handler → Response (Ep 3)
6. **FSM state machine diagrams** — State transitions (Ep 6)

### Alternative: Remotion

If you're more comfortable with React than TypeScript generators, Remotion is comparable. But Motion Canvas has **built-in code animation** which is killer for programming tutorials.

---

## 5. Example Footage Needs

Based on script analysis, prioritize recording:

### Must-Have Screen Recordings

| Episode | What to Record |
|---------|----------------|
| 01 | Package Manager installation, wizard interactions |
| 02 | View creation, mediator setup, inspector work |
| 03 | Command creation, signal wiring, debugging flow |
| 04 | List setup, binding configuration, runtime scroll |
| 05 | Popup prefabs, queue behavior, overlay layering |
| 06 | FSM editor, state transitions, debugging states |
| 07 | All wizards: Scene, View, Command, List, Popup |
| 08 | Binding bundles config, Addressables setup |

### B-Roll / Runtime Demos

- **Quick clips** (5-10 sec) of finished UI in action
- **Before/after** comparisons (spaghetti vs clean)
- **Inspector interactions** — show bindings auto-populating
- **Console logs** — command execution traces

### Recording Order

1. **Episode 07 (Editor Tooling) first** — captures all wizards
2. Re-use wizard footage in earlier episodes as needed
3. Episode 01 next — installation and first project
4. Remaining episodes follow script order

---

## 6. Workflow: Script to Published Video

### Phase 1: Prep (1-2 days)
1. Set up Motion Canvas project for diagrams
2. Configure OBS for Unity recording
3. Prepare sample Unity project with Framewerk
4. Set up TTS (Chatterbox local or ElevenLabs account)

### Phase 2: Audio Production (2-3 days)
1. Export narration scripts as plain text (strip markdown)
2. Generate TTS audio for all episodes
3. Review and regenerate any awkward pronunciations
4. Split into per-section audio files

### Phase 3: Screen Recording (3-5 days)
1. Record each episode's screen footage section by section
2. Follow scripts exactly — they include timing cues
3. Leave generous handles (3 sec before/after each action)
4. Organize into folders: `01-getting-started/raw/`

### Phase 4: Motion Canvas Animations (3-4 days)
1. Create reusable components (code blocks, diagrams)
2. Build animations matching script descriptions
3. Export as video clips
4. Organize: `00-foundations/animations/`

### Phase 5: Assembly & Editing (4-6 days)
1. Import all assets into DaVinci Resolve
2. Sync voiceover with footage
3. Add Motion Canvas animations at designated points
4. Add simple transitions (cross-dissolve, cuts)
5. Color correct for consistency
6. Export: 1080p, H.264, high bitrate

### Phase 6: Polish (1-2 days)
1. Add intro/outro cards (Framewerk branding)
2. Add chapter markers
3. Export thumbnails
4. Write video descriptions
5. Upload to hosting (YouTube, Vimeo, or self-hosted)

**Total Timeline: 2-3 weeks** (working on it part-time)

---

## 7. Cost Estimates

### Option A: Maximum Budget (Recommended)

| Item | Cost |
|------|------|
| Chatterbox TTS | $0 (open source) |
| OBS Studio | $0 |
| Motion Canvas | $0 (open source) |
| DaVinci Resolve Free | $0 |
| **Total** | **$0** |

### Option B: Premium Voice

| Item | Cost |
|------|------|
| ElevenLabs Creator (1-2 months) | $22-44 |
| Everything else | $0 |
| **Total** | **$22-44** |

### Option C: Premium Everything

| Item | Cost |
|------|------|
| ElevenLabs Creator | $44 |
| ScreenFlow (nicer recorder) | $169 (one-time) |
| DaVinci Resolve Studio | $295 (one-time) |
| Stock music (Epidemic Sound) | $15/mo |
| **Total** | **~$525** |

### NOT Recommended

| Approach | Cost | Why Skip |
|----------|------|----------|
| HeyGen AI Avatar | $29+/mo | Overkill for screen recordings |
| Synthesia | $22+/mo | Same — avatars not needed |
| Professional VO | $500-2000 | AI voice is good enough now |
| Motion graphics artist | $2000+ | Motion Canvas does the job |

---

## 8. Quality Tips for Technical Tutorials

### Voice
- **Pace:** 130-150 words/minute (not too fast)
- **Pauses:** 1-2 sec between concepts
- **Technical terms:** Test TTS pronunciation, add phonetic hints if needed

### Video
- **Mouse movements:** Slow, deliberate (use mouse highlighter)
- **Zoom:** Use zoom-ins for small UI elements
- **Highlight:** Add arrows/circles in post for emphasis

### Accessibility
- **Captions:** Auto-generate, then correct
- **Chapters:** Add YouTube-style timestamps
- **Code:** Provide links to example code repos

### Branding
- **Consistent colors:** Match Framewerk palette
- **Intro/Outro:** Keep short (3-5 sec)
- **Watermark:** Subtle corner logo

---

## 9. Recommended First Steps

1. **Install Chatterbox** — Test voice quality with a paragraph from the scripts
2. **Set up Motion Canvas** — Create one animated diagram (MVCS quadrants)
3. **Record a test section** — First 2 minutes of Episode 01
4. **Assemble in DaVinci** — Sync voice + video + animation
5. **Review quality** — Decide if voice needs upgrade (ElevenLabs)

If the test video looks/sounds good, proceed with full production. If not, iterate on the weak link (usually voice or animations).

---

## 10. File Organization

```
~/projects/framewerk/tutorials/video/
├── scripts/                    # Already exists - your .md files
├── audio/
│   ├── 00-foundations/
│   │   ├── section-01.wav
│   │   └── ...
│   └── ...
├── recordings/
│   ├── 00-foundations/
│   │   ├── raw/
│   │   └── edited/
│   └── ...
├── motion-canvas/              # Animation project
│   ├── src/
│   └── output/
├── projects/
│   ├── davinci/                # DaVinci Resolve project files
│   └── ...
├── exports/
│   ├── drafts/
│   └── final/
└── assets/
    ├── logos/
    ├── music/
    └── fonts/
```

---

## Summary

**Best approach for Framewerk tutorials:**

1. **Voice:** Start with Chatterbox (free), upgrade to ElevenLabs if needed
2. **Screen:** OBS Studio with Unity in a clean layout
3. **Animations:** Motion Canvas for code and diagrams
4. **Editing:** DaVinci Resolve Free
5. **Budget:** $0-44 total
6. **Timeline:** 2-3 weeks part-time

The scripts are already excellent — they include visual directions, timing, and clear structure. The production is mostly execution now. Start with a test video to validate the pipeline before committing to all 8 episodes.
