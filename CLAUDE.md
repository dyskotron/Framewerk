# CLAUDE.md - ShooterReborn Project Guide

## Project Overview

**ShooterReborn** is a Unity-based space shooter game featuring ship customization, combat, and a hangar system. It's built as both a playable game and a sandbox for experimenting with various game development patterns.

**Engine:** Unity (C#)  
**Architecture:** StrangeIoC / Framewerk MVCS framework

---

## Architecture & Patterns

### Core Framework: StrangeIoC + Framewerk

The project uses **StrangeIoC** dependency injection framework extended by a custom **Framewerk** layer. Key patterns:

- **MVCS** (Model-View-Controller-Service) with Signals
- **Mediator Pattern** - Views are dumb; Mediators handle logic
- **Command Pattern** - Actions triggered via Signals execute Commands
- **App State Machine (FSM)** - Game states managed via `IAppFsm`

### Entry Point

```
ShooterBootstrap.cs → ShooterContext.cs
```

- `ShooterBootstrap` is the MonoBehaviour attached to the scene
- `ShooterContext` sets up all DI bindings (injection, mediation, commands)

### Dependency Injection Flow

```csharp
// Injection binding (singletons, values, factories)
injectionBinder.Bind<IPlayerModel>().To<PlayerModel>().ToSingleton();

// View-Mediator binding
mediationBinder.Bind<ShipStatsView>().To<ShipStatsMediator>();

// Signal-Command binding
commandBinder.Bind<StartGameSignal>().To<StartGameCommand>();
```

---

## Directory Structure

```
Assets/
├── Scripts/
│   └── Shooter/
│       ├── ShooterBootstrap.cs      # Entry point MonoBehaviour
│       ├── ShooterContext.cs        # DI container setup
│       ├── Controller/
│       │   └── Commands/            # Command pattern implementations
│       ├── Model/                   # Data models and game state
│       │   ├── Game/                # Runtime game state models
│       │   └── ScriptableObjects/   # Unity SO definitions
│       ├── View/                    # UI views and mediators
│       │   ├── Screen/              # Full-screen UIs (Game, Hangar, Menu)
│       │   ├── Popups/              # Popup windows
│       │   └── Components/          # Reusable UI components
│       ├── GameActors/              # Entity component system for game objects
│       │   ├── Components/          # Actor components (Health, Weapon, etc.)
│       │   └── Model/               # Actor factories and models
│       ├── Enums/                   # Game enumerations
│       └── Utils/                   # Helper utilities
├── Prefabs/                         # Unity prefabs
├── Scenes/                          # Unity scenes
├── ScriptableObjects/               # SO asset instances
├── ToolsPackage/                    # Editor tools (LoadoutEditor)
└── RnD/                             # R&D / experimental stuff
```

---

## Key Systems

### 1. Game Actor System (ECS-like)

Located in `GameActors/Components/`. Ships, enemies, projectiles are built from components:

| Component | Purpose |
|-----------|---------|
| `HealthComponent` | HP and damage handling |
| `WeaponComponent` | Weapon firing logic |
| `ThrusterComponent` | Ship movement/thrust |
| `PhysicsBodyComponent` | Physics simulation |
| `EnergyComponent` | Power management |
| `ColliderComponent` | Collision detection |
| `IdentityComponent` | Actor identification |
| `AiShipController` | Enemy AI behavior |

Components inherit from `ActorComponent`. View components extend `ActorViewComponent`.

### 2. Ship Loadout System

- `IShipLoadoutsModel` - Manages saved ship configurations
- `ShipLoadoutManager` - Runtime loadout operations
- `IComponentsModel` / `IShipDefsModel` - Component and ship definitions
- `ComponentSlot` system for equipment slots

### 3. View/Mediator Pattern

Views are Unity MonoBehaviours with UI references. Mediators handle logic:

```csharp
// View - just UI elements and events
public class ShipStatsView : View {
    public TextMeshProUGUI healthText;
    public Signal<int> onHealthChanged;
}

// Mediator - logic and model interaction  
public class ShipStatsMediator : Mediator {
    [Inject] public IPlayerModel playerModel { get; set; }
    [Inject] public ShipStatsView view { get; set; }
    
    public override void OnRegister() {
        // Wire up view to models
    }
}
```

### 4. Screens & Navigation

Main screens (extend base Screen class):
- `MainMenuScreen` - Main menu
- `HangarScreen` - Ship customization
- `GameScreen` - Active gameplay

Navigation via `IAppFsm` state machine and signals.

### 5. Signal System

Signals are typed events for decoupled communication:

```csharp
// Define signal
public class StartGameSignal : Signal {}

// Dispatch
startGameSignal.Dispatch();

// Listen (usually in Commands via commandBinder)
commandBinder.Bind<StartGameSignal>().To<StartGameCommand>();
```

---

## Common Signals

| Signal | Purpose |
|--------|---------|
| `StartGameSignal` | Starts gameplay |
| `EnterHangarSignal` | Opens hangar |
| `ExitHangarSignal` | Leaves hangar |
| `MainMenuButtonPressedSignal` | Menu navigation |
| `LockTargetSignal` | Target locking |
| `GameActorDestroyedSignal` | Actor death events |
| `CoupledModeChangedSignal` | Flight mode toggle |

---

## Conventions

### Adding New Features

1. **New Model**: Create interface + implementation, bind in `ShooterContext`
2. **New View**: Create View class + Mediator, bind via `mediationBinder`
3. **New Action**: Create Signal + Command, bind via `commandBinder`
4. **New Actor Component**: Extend `ActorComponent`, add to actor prefab

### Naming Conventions

- `*View` - MonoBehaviour UI components
- `*Mediator` - View logic handlers
- `*Model` - Data/state classes (usually with `I*Model` interface)
- `*Command` - Signal handlers
- `*Signal` - Event definitions
- `*Component` - Game actor components
- `*Controller` - Runtime behavior controllers
- `*DefSo` - ScriptableObject definitions

### File Organization

- Keep View and Mediator in same folder
- Group by feature (Hangar, Game, etc.)
- ScriptableObjects definitions in `Model/ScriptableObjects/`

---

## External Dependencies

- **Framewerk** - Custom framework (in `Assets/Plugins/Framewerk` or separate repo `dyskotron/Framewerk`)
- **StrangeIoC** - DI framework (via Framewerk)
- **TextMesh Pro** - UI text rendering
- **Unity Input System** - `ShooterInputActions`

---

## Quick Start for Changes

### Modify UI
1. Find the `*View.cs` for the UI element
2. Find corresponding `*Mediator.cs`
3. Mediator has injected models - modify logic there

### Add New Ship Component
1. Create new class in `GameActors/Components/`
2. Extend `ActorComponent` or `ActorViewComponent`
3. Add to ship prefab in Unity Editor

### Add New Game Command
1. Create Signal class: `public class MySignal : Signal {}`
2. Create Command: `public class MyCommand : Command { public override void Execute() {...} }`
3. Bind in `ShooterContext`: `commandBinder.Bind<MySignal>().To<MyCommand>();`
4. Inject and dispatch signal where needed

---

## Unity MCP Rules

**MANDATORY:** This project uses Unity MCP for editor integration.

1. **Use MCP tools** for any Unity Editor operations (creating assets, modifying scenes, refreshing database)

2. **If MCP tools are unavailable** (not in your tool list, connection errors, etc.):
   - STOP immediately
   - Tell me "MCP tools not available - please check Unity MCP connection and restart the session"
   - Do NOT attempt workarounds (writing YAML directly, curl commands, etc.)

3. **After completing any work that modifies Unity assets** (.asset, .prefab, .cs, .meta files):
   - Call the MCP `refresh_asset_database` tool (or equivalent)
   - Do this once at the end of a batch of changes, not after every single file

**Check MCP availability** at session start before doing Unity work.

---

## Handling Unity Files — CRITICAL

Unity projects contain many non-text files. **You MUST handle them gracefully.**

### Files You CANNOT Read/Edit Directly
These are binary or serialized formats — do NOT try to `Read`, `cat`, or `Edit` them:
- `.asset` (binary serialized assets)
- `.prefab` (can be YAML but often huge/complex)
- `.unity` (scene files — YAML but massive)
- `.anim`, `.controller` (animation files)
- `.mat` (materials)
- `.physicsMaterial`, `.physicsMaterial2D`
- `.lighting`, `.shadervariants`
- `.png`, `.jpg`, `.tga`, `.psd`, `.tif`, `.exr` (textures)
- `.fbx`, `.obj`, `.blend` (3D models)
- `.wav`, `.mp3`, `.ogg` (audio)
- `.dll`, `.so`, `.dylib` (native plugins)
- Any file in `Library/`, `Temp/`, `obj/` folders

### Files You CAN Read/Edit
- `.cs` (C# scripts) ✅
- `.json`, `.xml`, `.txt`, `.md`, `.yml`, `.yaml` ✅
- `.meta` files — YAML, small, readable ✅ (but be careful with GUIDs)
- `.asmdef`, `.asmref` (assembly definitions) ✅
- `.shader`, `.hlsl`, `.cginc` (shader code) ✅
- `.inputactions` (Unity Input System — JSON) ✅

### .meta Files — Special Rules
Every asset in Unity has a `.meta` file containing its GUID and import settings.
- **NEVER delete a .meta file** — it breaks all references to that asset
- **NEVER change the `guid:` line** in a .meta file — same reason
- When **moving/renaming** a file, move its `.meta` file too
- When **creating** a new file, Unity generates the .meta automatically on next refresh
- You CAN read .meta files to find GUIDs for reference

### When You Hit a Binary/Asset File
If a task requires modifying any Unity asset (ScriptableObject, Prefab, Scene, etc.):
1. **Use Unity MCP tools** — that's what they're for
2. **ALWAYS solve the problem** — never skip asset changes. If code references an asset, that asset MUST be correct too, or the whole thing breaks
3. After making changes, **check the Unity console via MCP** to verify there are no errors
4. If MCP isn't available — STOP, report it, and wait for it to be enabled (see below)

### MCP Not Available?
If MCP tools aren't in your tool list or connections fail:
1. STOP immediately — do NOT continue without MCP
2. Tell the user: "MCP tools not available — please check Unity MCP connection"
3. The MCP window in Unity can be toggled from: **Window > MCP for Unity > Toggle MCP Window** (or the tab next to Inspector in the right panel)
4. Wait for confirmation before continuing

### Asset Operations via MCP
Use MCP tools for:
- Creating/modifying ScriptableObjects
- Modifying prefab properties
- Scene operations
- Asset database refresh
- Creating folders/moving assets
- **Reading Unity console output** — always check after changes to verify they work

### Verify Your Work
After making changes (code AND assets):
1. Use MCP to **refresh the asset database**
2. Use MCP to **check the Unity console** for errors/warnings
3. If there are errors, **fix them before moving on**
4. Do NOT assume things work — verify via console output
5. If something requires entering Play mode to test, tell the user

---

## Notes

- Playground project — experimental code in `RnD/`
- `dev` is main working branch
