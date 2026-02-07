# Framewerk Package Split Design

## Goal

Split the monolithic Framewerk package into smaller, optional packages. Users install only what they need.

---

## Proposed Packages

### 1. `com.dyskotron.framewerk.core` (Required base)

**Contents:**
- `Strange/` — Full StrangeIoC framework (IoC/DI, signals, commands, mediation)
- `Framewerk/StrangeCore/` — FramewerkMVCSContext, FramewerkCrossContext, DestroyingBinder, ViewlessContext
- `Framewerk/StrangeCore/Bundles/` — BindingBundle, IBindingBundle, NullInjectionBinding
- `Framewerk/Utils/` — BindingUtils
- `ContextStartSignal.cs`
- `ActionUpdater.cs`, `Updater.cs`
- `SingletonMono.cs`

**Dependencies:** None

---

### 2. `com.dyskotron.framewerk.ui` (Optional)

**Contents:**
- `Framewerk/Managers/` — UiManager, AssetManager, CoroutineManager
- `Framewerk/Popups/` — PopupManager, PopupMediator, PopupView, MessageBox, OkCancelWindow
- `Framewerk/UI/` — ExtendedMediator, List system, DragElement, PointerElement, UiGradient
- `ViewConfig.cs`, `SkinConfig.cs`
- `AddressBuilder.cs`, `ContextPrefix.cs`, `MonoBinder.cs`
- `LocalDataManager.cs`, `PlayerPrefsManager.cs`

**Dependencies:**
- `com.dyskotron.framewerk.core`
- `com.unity.addressables`
- `com.unity.textmeshpro`

---

### 3. `com.dyskotron.framewerk.fsm` (Optional)

**Contents:**
- `Framewerk/AppStateMachine/` — AppFsm, AppState, AppStateScreen, signals

**Dependencies:**
- `com.dyskotron.framewerk.core`

**Note:** Only ~6 files, clean boundaries. Easy to extract.

---

### 4. `com.dyskotron.framewerk.networking` (Optional)

**Contents:** (from `feature/mirror` branch)
- `Framewerk/Networking/` — NetworkHost, Discovery, Serialization
- `Framewerk/Networking/StrangeIntegration/` — NetworkCommandBinder, NetworkMessageReceivedSignal

**Dependencies:**
- `com.dyskotron.framewerk.core`
- Mirror

---

### 5. `com.dyskotron.framewerk.editor` (Optional, Editor-only)

**Contents:**
- `Editor/Wizards/` — Component scaffolding, MCP tools, templates, SkinResolver

**Dependencies:**
- `com.dyskotron.framewerk.core`
- `com.dyskotron.framewerk.ui`
- `com.dyskotron.framewerk.fsm`

---

## Dependency Graph

```
              Core
            /   |   \
          UI   FSM   Networking
           \    |
            Editor
```

- **Core** has no dependencies
- **UI, FSM, Networking** each depend only on Core
- **Editor** depends on Core + UI + FSM (generates code for screens, popups, etc.)

---

## What Stays Together

| Component | Package | Reasoning |
|-----------|---------|-----------|
| Popups | UI | Tightly coupled to UiManager, ViewConfig |
| Lists | UI | Specialized view pattern |
| ExtendedMediator | UI | View-specific helper |
| AssetManager | UI | Addressables loading for views |
| BindingBundle | Core | Core architecture pattern |

---

## Cleanup Before Split

- [ ] **Delete `ClosePopupsOnAppStateExitCommand`** — One-liner wrapper, users can inline `PopupManager.CloseAllPopups()` in their own command if needed

---

## Implementation Plan

1. **Phase 1: Core + UI split** — Biggest impact, most files
2. **Phase 2: FSM extraction** — Quick win, only 6 files
3. **Phase 3: Networking** — When Mirror branch is ready to merge
4. **Phase 4: Editor** — After runtime packages stabilize

Each package needs:
- Own folder structure
- Own `package.json`
- Own `.asmdef` with proper references
- Own `CHANGELOG.md`

---

## Package Install Scenarios

| Scenario | Packages |
|----------|----------|
| Minimal (IoC only) | `core` |
| Typical game | `core` + `ui` + `fsm` |
| UI-less server | `core` + `networking` |
| Full install | all packages |

---

## Open Questions

- [ ] Version strategy — all packages share version, or independent versioning?
- [ ] Mono-repo or separate repos?
- [ ] How to handle cross-package examples/demos?

