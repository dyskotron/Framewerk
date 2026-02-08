# Framewerk Video Tutorials — Shot List for Matej

This document lists all Unity screen recordings needed from you. Everything else (voice, animations, editing) I'll handle.

---

## Recording Setup

**Before you start:**
- Fresh Unity 2021.3+ project (or use existing clean project)
- Framewerk package installed
- OBS Studio set up (1920×1080, 60fps recommended)
- Disable notifications (macOS Do Not Disturb)
- Clean desktop, professional layout

---

## Episode 01: Getting Started
**Priority: HIGH** — Core workflow footage

| Shot ID | Description | Duration |
|---------|-------------|----------|
| 01-A | Open Unity Package Manager, click +, "Add package from git URL" | 15s |
| 01-B | Paste Framewerk URL, watch it install with dependencies | 30s |
| 01-C | Open Framewerk menu → Create Scene, show wizard window | 10s |
| 01-D | Fill Scene Wizard fields (Name: "MyGame", namespace auto-fills) | 20s |
| 01-E | Click Generate, show Unity recompiling, wizard finishing | 30s |
| 01-F | Open generated scene, show hierarchy (Bootstrap, Canvas, containers) | 15s |
| 01-G | Open Framewerk menu → Create UI Component, show wizard | 10s |
| 01-H | Fill Component Wizard (Name: "Welcome", Type: Popup, tick checkboxes) | 30s |
| 01-I | Click Generate, show files created, prefab appears | 20s |
| 01-J | Open Welcome prefab, add UI elements (title text, close button) | 45s |
| 01-K | Wire SerializeField references in Inspector | 20s |
| 01-L | Press Play, show popup appearing | 15s |

---

## Episode 02: Views & Mediation
**Priority: MEDIUM** — Reuses some footage from Ep 01

| Shot ID | Description | Duration |
|---------|-------------|----------|
| 02-A | Create new C# script in Project window | 10s |
| 02-B | Create Canvas, add UI elements (health text, score text, button) | 30s |
| 02-C | Attach View script to Canvas, wire Inspector references | 20s |
| 02-D | Press Play, show Console log when mediator registers | 10s |
| 02-E | Click button in Play mode, show Console log (demonstrating signals) | 15s |
| 02-F | Show Inspector with Mediator component auto-added to View GameObject | 10s |

---

## Episode 04: Lists
**Priority: MEDIUM**

| Shot ID | Description | Duration |
|---------|-------------|----------|
| 04-A | Component Wizard with Type: List selected, show expanded preview | 20s |
| 04-B | Generated files in Project window (ListView, ListMediator, ListItemView, etc.) | 15s |
| 04-C | Open PlayerList prefab, show structure in Inspector | 20s |
| 04-D | Press Play, show list populating with items | 20s |
| 04-E | Click a list item, show selection handling | 15s |

---

## Episode 05: Popups
**Priority: MEDIUM** — Reuses Ep 01 footage

| Shot ID | Description | Duration |
|---------|-------------|----------|
| 05-A | Component Wizard with Type: Popup, all options visible | 20s |
| 05-B | Building popup UI hierarchy in prefab mode | 30s |
| 05-C | Press Play, show popup opening with animation | 15s |
| 05-D | Close popup, show close animation | 10s |
| 05-E | Show multiple popups stacking (if applicable) | 15s |

---

## Episode 06: Screen FSM
**Priority: MEDIUM**

| Shot ID | Description | Duration |
|---------|-------------|----------|
| 06-A | Screen FSM editor window (if visual editor exists) | 20s |
| 06-B | State transitions in Play mode (Menu → Game → Pause → Game) | 30s |
| 06-C | Console showing state change logs | 15s |
| 06-D | Inspector showing current state | 10s |

---

## Episode 07: Editor Tooling
**Priority: HIGHEST** — Most wizard footage, can reuse in other episodes

| Shot ID | Description | Duration |
|---------|-------------|----------|
| 07-A | Clean Unity project with Framewerk visible in Packages | 10s |
| 07-B | Full Scene Wizard window, all fields visible | 15s |
| 07-C | Type scene name, show preview updating live | 20s |
| 07-D | Console showing wizard progress messages during generation | 15s |
| 07-E | Hierarchy view of complete generated scene structure | 15s |
| 07-F | Full Component Wizard window, all fields visible | 15s |
| 07-G | Component Wizard with each type selected (View, Popup, List, Screen) — 4 shots | 40s |
| 07-H | Generated prefab in Inspector showing addressable address | 15s |
| 07-I | ViewConfig Inspector fully expanded (cameras, containers) | 20s |
| 07-J | SkinConfig Inspector with template overrides | 15s |
| 07-K | ContextPrefix ScriptableObject in Inspector | 10s |

---

## Episode 00: Foundations
**Priority: LOW** — Mostly animated, minimal Unity footage

| Shot ID | Description | Duration |
|---------|-------------|----------|
| 00-A | Quick Unity Editor shot (any scene, professional look) | 5s |
| 00-B | Real game footage running Framewerk (if you have any) | 10-20s |

---

## Episodes 03 & 08
**Commands/Signals & Binding Bundles — mostly code walkthrough, no special footage needed**

Can reuse generic Play mode / Console shots from other episodes.

---

## Recording Tips

1. **Record Episode 07 first** — it captures all wizards, can reuse clips
2. **Record each shot separately** — easier to fix mistakes
3. **Leave 2 sec padding** at start and end of each shot
4. **Slow mouse movements** — fast clicking looks chaotic
5. **Clean workspace** — close unneeded tabs, hide personal files
6. **Consistent window layout** — same positions across all shots

---

## Total Footage Estimate

| Episode | Shots | Approx Duration |
|---------|-------|-----------------|
| 00 | 2 | 15-25s |
| 01 | 12 | ~5 min |
| 02 | 6 | ~2 min |
| 04 | 5 | ~1.5 min |
| 05 | 5 | ~1.5 min |
| 06 | 4 | ~1.5 min |
| 07 | 11 | ~3 min |

**Total: ~45 shots, ~15 minutes of raw footage**

Many shots can be reused across episodes (wizards especially).

---

## File Delivery

Drop recordings in: `~/projects/framewerk/tutorials/recordings/`

Name format: `{episode}-{shot-id}.mov` (e.g., `01-A.mov`, `07-G-popup.mov`)

Or just dump them and I'll sort through.
