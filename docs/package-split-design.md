# Framewerk Package Split Design — FINAL

## Goal

Split the monolithic Framewerk package into smaller, optional packages:
- **Pure .NET core** for server-side use without Unity
- **Meta-package** for easy "give me everything" install
- **Optional packages** for specific features

---

## Package Structure

### 1. `com.dyskotron.framewerk.core` (Pure .NET — No Unity!)

**Purpose:** IoC, Signals, Commands, Promises — headless server-compatible

```
Strange/
├── framework/                    # Binder, Binding, SemiBinding
├── extensions/
│   ├── injector/                # DI core
│   ├── signal/                  # Signals
│   ├── command/                 # Commands, SignalCommandBinder
│   ├── promise/                 # Promises
│   ├── dispatcher/              # Event dispatcher
│   ├── reflector/               # Reflection utils
│   ├── sequencer/               # Sequencer
│   ├── pool/                    # Object pooling
│   ├── implicitBind/            # Attributes
│   └── context/
│       ├── api/                 # IContext, ICrossContextCapable, etc.
│       └── impl/
│           ├── Context.cs
│           ├── CrossContext.cs
│           ├── CrossContextBridge.cs
│           └── ContextException.cs
│       # NOTE: MVCSContext.cs and ContextView.cs go to `ui`

Framewerk/
├── ContextStartSignal.cs
└── StrangeCore/
    ├── ViewlessContext.cs
    ├── FramewerkCrossContext.cs     # ✅ Moved from UI
    ├── DestroyingBinder.cs
    └── Bundles/
        ├── IBindingBundle.cs        # ✅ Moved from UI
        ├── CoreBindingBundle.cs     # ✅ NEW — base class for headless bundles
        └── NullInjectionBinding.cs  # ✅ Moved from UI
```

**Dependencies:** None (pure .NET Standard 2.1)

---

### 2. `com.dyskotron.framewerk.ui` (Unity)

**Purpose:** Views, Mediators, UiManager, Popups, Lists, AssetManager, ViewConfig — the full Unity UI framework

```
Strange/extensions/
├── mediation/                   # View, Mediator, MediationBinder
└── context/impl/
    ├── ContextView.cs           # MonoBehaviour bootstrap
    └── MVCSContext.cs           # Unity MVCS

Framewerk/
├── StrangeCore/
│   ├── FramewerkMVCSContext.cs
│   └── Bundles/
│       └── BindingBundle.cs     # ✅ Extends CoreBindingBundle, adds MediationBinder
├── Managers/
│   ├── AssetManager.cs
│   ├── UiManager.cs
│   └── CoroutineManager.cs
├── Popups/                      # Full folder
├── UI/
│   ├── ExtendedMediator.cs
│   ├── List/
│   └── Components/
├── ViewConfig.cs
├── SkinConfig.cs
├── ContextPrefix.cs
├── AddressBuilder.cs
├── MonoBinder.cs
├── SingletonMono.cs
├── Updater.cs
├── ActionUpdater.cs
├── AppMonitor.cs
├── LocalDataManager.cs
└── PlayerPrefsManager.cs
```

**Dependencies:**
- `com.dyskotron.framewerk.core`
- `com.unity.addressables`
- `com.unity.textmeshpro`

---

### 3. `com.dyskotron.framewerk.screenfsm` (Unity)

**Purpose:** Screen-based FSM for app state/view management

```
Framewerk/AppStateMachine/
├── AppFsm.cs
├── AppState.cs
├── AppStateScreen.cs
├── AppStateEnterSignal.cs
└── AppStateExitSignal.cs
```

**Dependencies:**
- `com.dyskotron.framewerk.ui`

---

### 4. `com.dyskotron.framewerk.networking` (Optional)

**Purpose:** Mirror networking integration

```
Framewerk/Networking/
├── NetworkHost.cs
├── Discovery/
├── Serialization/
└── StrangeIntegration/
    ├── NetworkCommandBinder.cs
    └── NetworkMessageReceivedSignal.cs
```

**Dependencies:**
- `com.dyskotron.framewerk.core` (can run headless — no Unity required!)
- Mirror

---

### 5. `com.dyskotron.framewerk.editor` (Unity Editor-only)

**Purpose:** Wizards, scaffolding, code generation

```
Editor/Wizards/
├── ComponentScaffoldWizard.cs
├── SceneScaffoldWizard.cs
├── CodeTemplates.cs
├── SkinResolver.cs
└── ...
```

**Dependencies:**
- `com.dyskotron.framewerk.ui`

---

### 6. `com.dyskotron.framewerk` (Meta-package)

**Purpose:** Convenience package — add this to get the full Framewerk workflow

**Contents:** No code, just dependencies

**Dependencies:**
- `com.dyskotron.framewerk.ui`
- `com.dyskotron.framewerk.screenfsm`
- `com.dyskotron.framewerk.editor`

---

## Dependency Graph

```
       core (pure .NET)
        ↙        ↘
      ui        networking
     ↙  ↘
screenfsm  editor
      ↘  ↙
   [framewerk]
```

---

## Package Install Scenarios

| Scenario | Package to Install | What You Get |
|----------|-------------------|--------------|
| Full Unity game | `framewerk` | Everything |
| Unity game (no FSM) | `ui` | Core + UI |
| Pure .NET server | `core` | IoC, Signals, Commands |
| Headless game server | `core` + `networking` | Server-side logic |

---

## Bundle Architecture

**CoreBindingBundle** (core package):
- `InjectionBinder` — DI bindings
- `CommandBinder` — Signal→Command bindings
- Pure .NET compatible, no Unity dependencies
- Use for headless server bundles

**BindingBundle** (ui package):
- Extends `CoreBindingBundle`
- Adds `MediationBinder` — View→Mediator bindings
- Use for Unity UI bundles that need mediation

```
IBindingBundle (interface)
     ↑
CoreBindingBundle (core)
     ↑
BindingBundle (ui)
```

---

## Cleanup Before Split

- [x] Remove unused `using UnityEngine` from `Promise.cs`
- [x] Split BindingBundle → CoreBindingBundle + BindingBundle
- [x] Move FramewerkCrossContext to core
- [x] Move IBindingBundle, NullInjectionBinding to core
- [ ] Remove unused `using UnityEngine` from `DestroyingBinder.cs`

---

## Implementation Steps

1. Create package folder structure:
   - `Packages/com.dyskotron.framewerk.core/`
   - `Packages/com.dyskotron.framewerk.ui/`
   - `Packages/com.dyskotron.framewerk.screenfsm/`
   - `Packages/com.dyskotron.framewerk.editor/`
   - `Packages/com.dyskotron.framewerk/`

2. For each package create:
   - `package.json`
   - `Runtime/` folder with `.asmdef`
   - `CHANGELOG.md`

3. Move files according to the structure above

4. Update `.asmdef` references

5. Test compilation

6. Update examples/demos

---

## Open Questions

- [ ] Version strategy — shared or independent?
- [ ] Mono-repo or separate repos?
- [ ] NuGet publishing for `core` package?
