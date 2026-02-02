# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

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

This project has **MCP For Unity** (`com.coplaydev.unity-mcp`) which lets you call Unity Editor functions via MCP. The server runs on `http://localhost:8080/mcp`.

### Built-in MCP tools
Standard tools: `manage_asset`, `manage_gameobject`, `manage_scene`, `manage_prefabs`, `create_script`, `apply_text_edits`, `validate_script`, `read_console`, `execute_menu_item`, `find_gameobjects`, `manage_components`, `manage_material`, `manage_editor`, etc.

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

## Plan Mode
- Make the plan extremely concise. Sacrifice grammar for the sake of concision.
- At the end of each plan, give me a list of unresolved questions to answer, if any.

## Conventions

- All mediators should extend `ExtendedMediator<TView>` for auto-cleanup
- App states extend `AppState<TScreen>` where TScreen is the view type
- Contexts define all bindings; avoid service locator patterns
- Framework code goes in the UPM package; project-specific code in `Assets/Scripts/`
