# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **📖 See also:**
> - `AI_CODING_GUIDE.md` — MCP workflow examples and patterns
> - `CODE_STYLE.md` — Coding conventions, naming, binding patterns
> - `docs/CrossContext.md` — CrossContext sharing and cross-context communication

## ⚠️ Critical: Use MCP Tools for UI Components

**DO NOT** create Unity prefabs, scenes, or wire UI components manually. Use the `framewerk_scaffold` MCP tool:

```bash
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {...}}'
```

Actions: `create_scene`, `create_popup`, `create_list`, `create_view`, `mark_addressable`

Unity automation is hard. The framework has wizards for this. **Use them.**

## 🎯 Development Rules: ALWAYS Use Wizards

Framewerk has wizards for scaffolding. **Use them.** Manual creation misses steps.

### Available Wizards

| Wizard | Menu Path | Use For |
|--------|-----------|---------|
| **Scene Scaffold** | `Framewerk > Create Scene` | New scenes (Bootstrap, ViewConfig, Camera, EventSystem, Canvases) |
| **View Panel** | `Framewerk > Create UI Component` | Views, Mediators, Popups |
| **List View** | `Framewerk > Create UI Component` | List components with items |

### When to use each:

**Scene Scaffold Wizard** — Creating new example/test/demo scenes:
- Bootstrap GameObject with ViewConfig pre-wired
- Camera setup (Main Camera with proper settings)
- EventSystem
- UI.Main Canvas (for regular UI)
- UI.Popups Canvas (for popups/overlays)

**Component Scaffold Wizard** — Creating UI components:
- Views, Mediators, Popups → select "View Panel" type
- List components with items → select "List View" type
- Handles naming conventions, Addressables registration, prefab wiring

### Why wizards are mandatory:
1. **Naming conventions** — Wizards enforce correct naming (e.g., `*View`, `*Mediator`, `*Data`, `*ItemView`)
2. **Addressables registration** — Wizards automatically mark prefabs as Addressable with correct addresses
3. **Proper wiring** — Prefabs get correct hierarchy, component attachment, and serialized references
4. **Scene setup** — ViewConfig fields correctly wired to canvases/camera
5. **Less error-prone** — Manual creation often misses steps (wrong namespace, missing registration, null refs)

### The rule:
- ✅ **Use Scene Scaffold** for ALL new scenes
- ✅ **Use Component Scaffold** for ALL new UI components
- ❌ **Never manually create** scenes or View/Mediator/List files by hand
- ⚠️ **Exception:** Only create manually if the wizard literally cannot handle your specific use case (rare)

## 🔄 Unity Refresh Rule: Always Call MCP After File Changes

After making **any changes** to Unity files (scripts, prefabs, scenes, Addressables, ScriptableObjects, etc.), **always call `refresh_unity` via MCP** before finishing the task:

```bash
python3 ~/clawd/scripts/unity_mcp.py call refresh_unity
```

**Why:** Unity doesn't auto-detect external file changes. Without refresh, the Editor shows stale state — scripts won't compile, prefabs won't update, Addressables won't register. Don't leave the human to manually refresh.

**Rule:** Every task that touches Unity files ends with `refresh_unity`. No exceptions.

## 📝 Asset Rename Rule: Use MCP manage_asset

When renaming Unity assets (scripts, prefabs, ScriptableObjects, etc.), **always use MCP `manage_asset` with `action: "rename"`** instead of renaming files directly via filesystem:

```bash
python3 ~/clawd/scripts/unity_mcp.py call manage_asset '{"action": "rename", "path": "Assets/Scripts/OldName.cs", "newName": "NewName.cs"}'
```

**Why:** Unity tracks assets by GUID. Renaming via filesystem breaks serialized references — prefabs, scenes, and ScriptableObjects that reference the renamed asset lose their connections. Using `manage_asset rename` goes through Unity's AssetDatabase, which preserves all references.

**Rule:** Never `mv` or rename Unity assets directly. Always use MCP `manage_asset` with `action: "rename"`.

## Project Overview

Framewerk is a Unity MVCS (Model-View-Controller-Service) framework built on **StrangeIoC** for dependency injection, signal-based communication, and command mapping. It is packaged as a UPM package (`com.dyskotron.framewerk` v2.1.0) at `Packages/com.dyskotron.framewerk/`, with a demo app at `Assets/Scripts/FramewerkDemo/`.

## Build & Test

- **Unity version:** 2021.3+
- **Open scene:** `Assets/Scenes/SampleScene.unity`, press Play
- **Run tests:** Use Unity Test Runner (Window > General > Test Runner) — tests are in `Packages/com.dyskotron.framewerk/Tests/` covering Strange framework internals
- **Package location:** `Packages/com.dyskotron.framewerk/` (local UPM, referenced as `file:com.dyskotron.framewerk` in manifest)
- **Key dependency:** `com.unity.addressables` (1.21+)

## Architecture

### Core Pattern: StrangeIoC MVCS

All wiring happens in a **Context** class (e.g., `FramewerkDemoContext`):
- `injectionBinder` — binds interfaces to implementations (singletons/transients)
- `commandBinder` — maps Signals to Commands (`Signal → Command`)
- `mediationBinder` — maps Views to Mediators (`View → Mediator`)

**Execution flow:** Bootstrap (ContextView) → Context setup → StartCommand → AppFsm state transition → Screen/View mediation

### Key Components

| Component | Role |
|---|---|
| `AppFsm` / `AppState<TScreen>` | Queued state machine for app screens; async enter/exit |
| `ExtendedMediator<TView>` | Generic mediator base with button/slider/toggle listener helpers and auto-cleanup |
| `PopupManager` / `PopupMediator<TView>` | Modal popup lifecycle with dynamic buttons and promises |
| `AssetManager` | Addressables-based async asset loading (`GetAssetAsync`, `PreloadAssetAsync`, `GetSpriteAsync`) |
| `UiManager` | UI prefab instantiation on canvas |
| `ViewConfig` | Centralized UI camera/canvas references (partial class, extensible per-project) |

### Startup / Initialization Flow

`Bootstrap.Start()` → `Context.Start()` → `instantiateCoreComponents()` → `mapBindings()` → `postBindings()` → `Launch()` → `ContextStartSignal.Dispatch()`

Framework services (AssetManager, UiManager, PopupManager, etc.) are only available after `ContextStartSignal` fires. Do not use them before init completes (e.g. not in `Bootstrap.Start()` or `mapBindings()`). Map a startup command to `ContextStartSignal` to kick things off:
```csharp
commandBinder.Bind<ContextStartSignal>().To<MyStartCommand>();
```

### Signal → Command → Service Flow

1. View dispatches a **Signal** (type-safe event)
2. **CommandBinder** routes it to a **Command**
3. Command has dependencies `[Inject]`ed and executes business logic
4. Results dispatch new Signals or modify injected models

### Mediation Pattern

- Views are MonoBehaviours with UI references
- Mediators receive `[Inject]` dependencies and wire view events to signals
- `[ListensTo]` attribute on mediator methods for signal-based mediation
- `OnRegister()` / `OnRemove()` lifecycle; listeners auto-cleaned in `OnRemove()`

## Code Layout

```
Packages/com.dyskotron.framewerk/
  Runtime/
    Framewerk/          # Framework code (AppFsm, Managers, Popups, UI, StrangeCore)
    Strange/            # Embedded StrangeIoC (IoC, signals, commands, mediation, pooling)
  Tests/                # Strange framework unit tests

Assets/Scripts/FramewerkDemo/   # Demo app (Bootstrap, Context, States, Screens)
```

## MCP Tools (Unity ↔ AI)

This project uses **MCP Unity** (`com.gamelovers.mcp-unity`) from [CoderGamester/mcp-unity](https://github.com/CoderGamester/mcp-unity).

### Key Features
- **Auto-starts when Unity launches** — no manual "Start Server" button needed
- Uses stdio transport with Node.js server (auto-configured for Claude Code, Cursor, etc.)
- More tools than previous plugins: materials, batching, transforms, component updates

### Built-in MCP Tools
- `execute_menu_item` — Execute Unity menu items
- `select_gameobject` — Select GameObjects in hierarchy
- `update_gameobject` — Update or create GameObjects (name, tag, layer, active/static)
- `update_component` — Update component fields or add components
- `add_package` — Install Unity packages
- `run_tests` — Run Unity tests
- `send_console_log` — Log messages to Unity console
- `get_logs` — Read Unity console logs
- Plus: asset management, scene operations, prefab handling, material editing

### Framewerk custom tool: `framewerk_scaffold`

Creates UI component prefabs with correct hierarchy and wiring. Use this instead of manually creating prefabs — it handles View component attachment, serialized reference wiring, and Addressable marking.

**Actions:**

#### `create_popup`
Creates a Popup prefab with the View component attached.
```json
{
  "action": "create_popup",
  "name": "MyPopup",
  "namespace": "MyGame.UI",
  "viewTypeName": "MyGame.UI.MyPopupView",
  "prefabFolder": "Assets/Prefabs",
  "overwrite": false
}
```

#### `create_list`
Creates a List prefab + ListItem prefab, attaches View components, wires ItemPrefab reference.
```json
{
  "action": "create_list",
  "name": "MyList",
  "namespace": "MyGame.UI",
  "viewTypeName": "MyGame.UI.MyListView",
  "itemViewTypeName": "MyGame.UI.MyListItemView",
  "prefabFolder": "Assets/Prefabs",
  "overwrite": false
}
```

#### `mark_addressable`
Marks a prefab as Addressable with the given address.
```json
{
  "action": "mark_addressable",
  "prefabPath": "Assets/Prefabs/MyPopup.prefab",
  "address": "UI/MyPopup"
}
```

### Workflow: Creating a new UI component
1. **Write scripts** — Create View, Mediator (and Data/ItemView/ItemMediator for Lists)
2. **Wait for compile** — Unity must compile the scripts before prefabs can reference them
3. **Call `framewerk_scaffold`** — Use `create_popup` or `create_list` to create prefabs with components attached
4. **Call `mark_addressable`** — Mark prefabs as Addressable
5. **Edit Context** — Add mediation and injection bindings to the project's Context class

## Scene Setup Blueprint (MCP)

When creating a new Framewerk test/demo scene via MCP, replicate this exact structure from `SampleScene`. **Do NOT use a single Canvas with children — use separate root Canvas objects per UI layer.**

### Required hierarchy (root GameObjects)

```
Main Camera          — Transform, Camera (clearFlags: Skybox), AudioListener
Directional Light    — Transform, Light
EventSystem          — Transform, EventSystem, StandaloneInputModule
Bootstrap            — Transform, [YourBootstrap], ViewConfig
UI.Main              — RectTransform, Canvas, CanvasScaler, GraphicRaycaster
UI.Popups            — RectTransform, Canvas, CanvasScaler, GraphicRaycaster
```

### Canvas configuration (both UI.Main and UI.Popups)

| Component | Property | Value |
|---|---|---|
| Canvas | renderMode | `0` (Screen Space - Overlay) |
| CanvasScaler | uiScaleMode | `1` (Scale With Screen Size) |
| CanvasScaler | referenceResolution | `800 x 600` |
| CanvasScaler | screenMatchMode | `0` (Match Width Or Height) |
| CanvasScaler | matchWidthOrHeight | `0` |

### ViewConfig wiring on Bootstrap

| Field | Target |
|---|---|
| `Camera3d` | Main Camera → Camera component |
| `UICamera` | Main Camera → Camera component |
| `Container3d` | Main Camera → Transform component |
| `UiBottom` | `UI.Main` → RectTransform |
| `UiDefault` | `UI.Main` → RectTransform |
| `Popups` | `UI.Popups` → RectTransform |
| `UiOverlay` | `UI.Popups` → RectTransform |

### Common mistakes to avoid
- **Never** use World Space (`renderMode: 2`) for UI canvases — use Screen Space - Overlay (`0`)
- **Never** put UiDefault/Popups as children of a single Canvas — they must be separate root Canvas objects
- **Never** use Constant Pixel Size (`uiScaleMode: 0`) — use Scale With Screen Size (`1`)
- **Never** leave ViewConfig fields null — all 7 fields must be wired
- For text fields in Views, use `TMP_Text` (TextMeshPro), not `UnityEngine.UI.Text`

## Code Quality

- **Don't add parameters that match defaults.** If a method has `instantiateInWorldSpace = false` as default, don't explicitly pass `false`. It's noise.
- Keep changes minimal — only add what's necessary for the fix.

## Commits, Deprecation & Changelogs

**The line is: "Was it ever in the repo?"** — not "Does anyone use this?"

- ✅ **Document changes between commits** — repo history is real history.
- ❌ **Don't narrate local experiments** — only reference approaches that were actually committed.

- **Commits:** Document what changed between committed states. Don't reference approaches that were never committed.
- **Deprecation:** Only if the old way was actually committed and shipped. Local scratched attempts don't get deprecated.
- **Changelogs:** "Replaced X with Y" only makes sense if X was ever in the repo.

New feature from scratch? There's no "old way" — write it like the first implementation it is.

## Plan Mode
- Make the plan extremely concise. Sacrifice grammar for the sake of concision.
- At the end of each plan, give me a list of unresolved questions to answer, if any.

## Conventions

- All mediators should extend `ExtendedMediator<TView>` for auto-cleanup
- App states extend `AppState<TScreen>` where TScreen is the view type
- Contexts define all bindings; avoid service locator patterns
- Framework code goes in the UPM package; project-specific code in `Assets/Scripts/`

## Lifecycle: Destroy() vs OnRemove()

**For MonoBehaviours (Views, Mediators):**
- `Destroy(gameObject)` = **trigger** to destroy the object (call this externally)
- `OnRemove()` = **cleanup reaction** (called automatically by mediation system when view is destroyed)
- **Rule:** Put listener removal and de-init in `OnRemove()`, NOT in `Destroy()` override
- StrangeIoC's mediation system calls `OnRemove()` before Unity destroys the object

**For non-MonoBehaviours (services, managers, plain classes):**
- No automatic lifecycle callbacks exist
- Put cleanup logic directly in `Destroy()` or a custom `Dispose()` method
- Caller is responsible for invoking cleanup

**Pattern:**
```csharp
// MonoBehaviour (Mediator) - CORRECT
public override void OnRemove()
{
    someSignal.RemoveListener(OnSomeEvent);  // ✅ cleanup here
    base.OnRemove();
}

// MonoBehaviour - WRONG
void OnDestroy()  // ❌ Don't use - mediation system won't call this reliably
{
    someSignal.RemoveListener(OnSomeEvent);
}
```
