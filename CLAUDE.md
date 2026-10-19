# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **📖 See also:**
> - `AI_CODING_GUIDE.md` — Scaffolding workflow examples and patterns
> - `CODE_STYLE.md` — Coding conventions, naming, binding patterns
> - `docs/CrossContext.md` — CrossContext sharing and cross-context communication

## ⚠️ Critical: Use the Scaffold Tooling for UI Components

**DO NOT** create Unity prefabs, scenes, or wire UI components manually. Use `FramewerkScaffold` through the Unity CLI:

```bash
unity command eval --project-path C:/DATA/PROJECTS/Framewerk --result-only --timeout 60000 'return Framewerk.Editor.Wizards.FramewerkScaffold.Execute("{\"action\":\"create_list\", ...}");'
```

Actions: `create_scene`, `create_popup`, `create_list`, `create_tabs`, `create_view`, `create_viewstack`, `mark_addressable`

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

## Project Overview

Framewerk is a Unity MVCS (Model-View-Controller-Service) framework built on **StrangeIoC** for dependency injection, signal-based communication, and command mapping. It is split into UPM packages under `Packages/` (`com.dyskotron.framewerk.core`, `.ui`, `.screenfsm`, `.editor`, plus the `com.dyskotron.framewerk` meta-package), with examples at `Assets/Examples/`.

## Build & Test

- **Unity version:** see `ProjectSettings/ProjectVersion.txt`
- **Fresh checkout:** `Assets/TextMesh Pro/` (TMP Essential Resources) is gitignored on purpose. Import it once or every TMP label renders empty: click Import in the TMP importer window, or headlessly `TMPro.EditorUtilities.TMP_PackageResourceImporter.ImportResources(true, false, false)` via eval. The import only progresses while the Editor window has focus, and it does not finish inside the eval call, so return immediately and poll `Assets/TextMesh Pro/` for files instead of forcing a synchronous refresh (that blocks the main thread until the command times out).
- **Open scene:** `Assets/Scenes/SampleScene.unity`, press Play
- **Run tests:** Unity Test Runner (Window > General > Test Runner), or `run_tests` through the Unity CLI — tests are in `Packages/com.dyskotron.framewerk.core/Tests/` covering Strange framework internals
- **Package location:** `Packages/com.dyskotron.framewerk*/` (local UPM, referenced as `file:` in manifest)
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
| `ExtendedMediator<TView>` | Generic mediator base with button/slider/toggle/pointer/hover listener helpers and auto-cleanup |
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
Packages/
  com.dyskotron.framewerk.core/      # IoC, signals, commands, mediation, pooling (embedded StrangeIoC) + Tests
  com.dyskotron.framewerk.ui/        # UiManager, popups, lists, tabs, view stacks, ExtendedMediator
  com.dyskotron.framewerk.screenfsm/ # AppFsm / AppState
  com.dyskotron.framewerk.editor/    # Wizards, templates, FramewerkScaffold
  com.dyskotron.framewerk/           # Meta-package

Assets/Examples/   # Self-contained example scenes (List, Popup, TabsViewStack, ScreenFsm)
```

---

# Unity Editor workflow (Unity CLI + Pipeline): rules for the agent

## Tooling: official Unity agent tooling only
- Use only Unity's official agent tooling. Do NOT install or use third-party Unity MCP plugins (CoplayDev `unity-mcp`, CoderGamester `mcp-unity`).
- **Unity CLI** (beta) drives the Editor: open/close projects, recompile, run tests/builds, batch mode.
  - macOS: `~/.unity/bin/unity`
  - Windows: `/c/Users/<user>/AppData/Local/Unity/bin/unity`. Add it to PATH in each Bash call.
- **Pipeline package** (`com.unity.pipeline`, experimental) must be in `Packages/manifest.json`. It runs inside the Editor and exposes its commands (console, play mode, eval C#, screenshots, input simulation, scene/asset edits).
- **MCP server** `unity-editor` = `unity mcp --project-path <repo>`, a stdio process owned by Claude Code. Its tools appear as `mcp__unity-editor__*`.
- **Claude Code plugin** `unity@unity-agent-plugin` provides the Unity skills (`/unity:*`).

### Setup on a new machine
```bash
curl -fsSL https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.sh | UNITY_CLI_CHANNEL=beta bash
claude mcp add --scope local --transport stdio unity-editor -- ~/.unity/bin/unity mcp --project-path <repo>
claude plugin marketplace add Unity-Technologies/unity-agent-plugin && claude plugin install unity@unity-agent-plugin
```
On Apple Silicon, open the project with the native Editor (`unity open . -a arm64`), not the Intel build under Rosetta.

## Reaching the Editor
1. Prefer the `mcp__unity-editor__*` tools.
2. If the MCP session dropped (it can die on a domain reload, e.g. entering Play mode), use the CLI through Bash. It is equivalent and always available:
   ```bash
   unity command <name> --project-path <repo> --timeout 90 --result-only -- --arg value
   unity command eval --project-path <repo> --result-only 'return UnityEngine.Application.version;'
   unity status --project-path <repo>        # state "ready" = reachable
   ```
   Multi-line C#: write it to a scratch file and run `unity command eval_file -- --file <path> --timeout <ms>`.
3. "Unreachable" for a few seconds while Unity is busy (domain reload, package resolve) is normal. Retry with a longer `--timeout`.
4. You MAY start, close and restart Unity yourself (`unity open`, `unity close --force`), EXCEPT when the user is at the computer with a GUI Editor open. Then never close or restart it without asking, unless it is frozen or stuck in Safe Mode. Before closing, check for unsaved scenes (`list_open_scenes` → `isDirty`).
5. If a task needs the Editor (recompile, asset changes, tests) and it is in Play mode: once the user has given the go for the task, take over the Editor and stop, enter or pause Play mode (`editor_stop`) as the task needs. The user accepts the consequences. Without a go (only discussing or investigating), don't touch Play mode while the user is working in the Editor.
6. If neither MCP nor the CLI can reach a running Editor, say so plainly. Do NOT hand-edit scenes, prefabs or assets as a workaround.
7. **Standalone tooling goes in a lab project.** When the user has the main Editor open and you are building a standalone editor tool or utility, build and verify it in a separate scratch Unity project with its own Editor (`unity open <lab path>`, then `--project-path <lab>` on every command). Its manifest needs at least `com.unity.pipeline` and `com.unity.test-framework`, and its `ProjectVersion.txt` must match the main project's version. Copy the finished folder (with its `.meta` files) into the repo only once it works, then close the lab with `unity close --force`. Never focus or screenshot the screen through the lab Editor while the main one is in front.

## Compile before reloading Unity
Safe Mode (compile errors at Editor startup) disables the Pipeline package, so the Editor becomes unreachable. Avoid it:
- Check the compile **outside Unity** first, using the Unity-generated project files:
  ```bash
  msbuild Assembly-CSharp.csproj -nologo -v:minimal -p:OutputPath=<scratch>/bin/ -p:BaseIntermediateOutputPath=<scratch>/obj/ -p:IntermediateOutputPath=<scratch>/obj/
  ```
  New `.cs` files (and new asmdefs' `.csproj` files) only appear after Unity regenerates them. A compile check outside Unity for a new assembly needs its own project with the Unity/NUnit references.
- Or a batch-mode compile with no dialogs: `unity run . --log-file <scratch>/batch.log --no-tail` (exit 0 = clean; grep the log for `error CS`).
- If there is no msbuild/dotnet (e.g. on Windows): use `unity command recompile` and poll `recompile_status` in the running Editor, which reports the errors. Safe Mode is only a risk when the Editor *starts* with broken code.
- Trigger `recompile` in a running Editor only once the code compiles.
- If Unity still lands in Safe Mode: `unity pipeline list` confirms it. Read the Editor log (`Logs/Editor.log` in the project since Unity 6.6), fix the code, then `unity close --force` and `unity open`.

## Verify your work (code AND assets)
1. **Make sure Play mode is off.** With the user's go for the task, stop it yourself.
2. **Compile outside Unity** (above), then `recompile` in the Editor and wait for `recompile_status` = completed.
3. **Check the console** (`console`, level `warn`) for compile errors and warnings. Fix any that appear.
4. **Enter Play mode** (`editor_play`) and drive the game (signals or methods via eval, `simulate_key`, `simulate_pointer`).
5. Let it run, then check the console again for runtime errors. Take a `capture_game_view` screenshot when visuals matter.
6. If there are runtime errors, stop Play mode, fix them, and repeat.
7. Do NOT assume things work. Always verify through the console after Play mode.

## Tests
- **While developing a feature on a branch, run only the tests for the code it touches.** Running the whole suite at every step wastes time.
  - `run_tests --filter <Full.Class.Name> --filter_type testName`: one class per call (a `;`-separated list matches nothing).
  - Whole suite: `run_tests --async_tests true`, then poll `test_status`.
- **Run the full EditMode suite when a branch that touches scripts, shaders or Unity assets is ready to merge** into its target branch. Never skip it for such a branch. A branch that changes only docs or tooling outside `Assets/` merges after its own checks.
- **If tests fail, compare with the branch point.** Run the full suite again at `git merge-base <branch> <target>` (check it out in place once everything is committed, then switch back).
  - Fails **the same way** at the branch point: not caused by the branch, so the merge may go ahead.
  - Passes at the branch point but fails on the branch: a regression. Fix it before merging.
- The EditMode test runner reloads the open scene. Check it isn't dirty first (`SceneManager.GetActiveScene().isDirty`) and don't run tests over unsaved scene work. If it is dirty with the user's throwaway changes, reload it from disk (`EditorSceneManager.OpenScene(path)`) instead of saving it.
- A test run may rewrite assets as a side effect (e.g. cached hashes in a settings asset). Check `git status` after a run and revert anything that isn't a real change.
- Tests write only into uniquely named folders they create themselves. They check code behaviour, never designer-tuned values in prefabs or assets.

## Gotchas
- **An unfocused Editor stops advancing Play mode** (a CLI-started Play mode can sit at frame 2). For automated play tests set `UnityEngine.Application.runInBackground = true` via eval. This IS persisted into `ProjectSettings.asset` (`runInBackground: 1`), so when done set `UnityEditor.PlayerSettings.runInBackground = false` + `AssetDatabase.SaveAssets()` and check `git diff ProjectSettings/`.
- **An unfocused Editor can also stall a domain reload or import** (e.g. after a package install): CPU near 0, `unity status` unreachable, the log stuck after `Begin MonoManager ReloadAssembly`. Bring Unity to the front (macOS: `osascript -e 'tell application "System Events" to set frontmost of (first process whose name is "Unity") to true'`).
- **A modal dialog blocks the main thread**, so every command times out (on macOS, `sample <pid> 2` shows `NSAlert runModal`). Press Esc on it yourself rather than waiting for the user: bring Unity to the front and send Esc (macOS: `osascript -e 'tell application "System Events" to key code 53'`). Esc picks Cancel, which loses nothing. The Test Runner asks to save modified scenes when the open scene is dirty: reload the scene from disk instead.
- **Changes the user makes in the Editor while you are working are throwaway tests** (of your work). Never commit them and don't let them block you (e.g. discard a dirty scene as above). Work the user cares about is committed by them before you run.
- `eval` / `eval_file` default to a 5 s main-thread timeout. Pass `--timeout <ms>` for heavier work (prefab edits, imports, preview renders).
- `capture_game_view` with `save_path` writes into `Assets/` (an asset + `.meta`). Prefer the inline base64 result and decode it into a scratch folder.
- After a `recompile`, `unity status` reports "ready" too early. Before `editor_play`, poll `unity command editor_status` until `status` = ready, `compiling` = false and `domainReloadInProgress` = false.
- A command that is in flight when Play mode's domain reload starts (e.g. an `editor_status` poll right after `editor_play`) is aborted and the Pipeline package logs `Failed to handle /api/exec request: Thread was being aborted.` as a console **error**. It is not a project error; give the reload a couple of seconds before polling, and don't count that entry against your own code.
- `simulate_pointer` / `simulate_key` drive the Input System package. This project uses the legacy Input Manager (`StandaloneInputModule`), so drive uGUI in Play mode via `eval` with `ExecuteEvents.Execute(go, pointerEventData, ExecuteEvents.pointerEnterHandler)` etc. instead.
- Don't trust `eval` arithmetic blindly: it runs in an interpreter. For gameplay diagnostics, add a temporary `Debug.Log` in compiled code, read the console, then revert it.
- Editor code saves only the assets it touched (`AssetDatabase.SaveAssetIfDirty`), never `AssetDatabase.SaveAssets()`, so the user's other unsaved edits are not written. (The one exception is the `runInBackground` reset above.)
- A `ScriptableObject` or `MonoBehaviour` class used as an asset or component must live in a file of the same name, or the asset loses its script.

## Handling Unity files
- **Do not Read/Edit directly** (binary or huge serialized YAML): `.asset`, `.prefab`, `.unity`, `.anim`, `.controller`, `.mat`, `.physicsMaterial(2D)`, `.lighting`, `.shadervariants`, textures, models, audio, native plugins, anything in `Library/`, `Temp/`, `obj/`.
- **Fine to read/edit:** `.cs`, `.json`, `.xml`, `.txt`, `.md`, `.yml/.yaml`, `.asmdef/.asmref`, `.shader/.hlsl/.cginc`, `.inputactions`, `.meta` (carefully).
- **`.meta` rules:** never delete a `.meta` and never change its `guid:` line (both break every reference). Move/rename the `.meta` with its file. Unity creates `.meta` files for new files on the next refresh.
- **Asset changes go through the Unity tooling** (MCP tools or `unity command`/eval). Never skip an asset change: if code references an asset, the asset must be correct too. Check the console afterwards.
- **Renaming assets:** go through `AssetDatabase.RenameAsset` via eval, never `mv`. Unity tracks assets by GUID and a filesystem rename breaks serialized references.
- **After any file change**, the Editor must pick it up: `unity command recompile` for scripts, or `AssetDatabase.Refresh()` via eval for assets. Don't leave the Editor stale for the human.

---

## Framewerk scaffold tool: `FramewerkScaffold`

Headless entry point for the wizards, in `Packages/com.dyskotron.framewerk.editor/Editor/Wizards/FramewerkScaffold.cs`. Creates UI component prefabs with correct hierarchy and wiring. Use this instead of manually creating prefabs — it handles View component attachment, serialized reference wiring, and Addressable marking.

Call it through eval with a JSON object (quote carefully; for anything non-trivial write the C# to a scratch file and use `eval_file`):

```bash
unity command eval --project-path C:/DATA/PROJECTS/Framewerk --result-only --timeout 60000 'return Framewerk.Editor.Wizards.FramewerkScaffold.Execute("{\"action\":\"create_popup\",\"name\":\"MyPopup\",\"namespace\":\"MyGame.UI\",\"viewTypeName\":\"MyGame.UI.MyPopupView\",\"prefabFolder\":\"Assets/Prefabs\"}");'
```

The result is a JSON string with `success`, `message`, and action-specific paths.

**Actions:**

#### `create_popup`
```json
{ "action": "create_popup", "name": "MyPopup", "namespace": "MyGame.UI", "viewTypeName": "MyGame.UI.MyPopupView", "prefabFolder": "Assets/Prefabs", "overwrite": false }
```

#### `create_list`
Creates a List prefab + ListItem prefab, attaches View components, wires ItemPrefab reference.
```json
{ "action": "create_list", "name": "MyList", "namespace": "MyGame.UI", "viewTypeName": "MyGame.UI.MyListView", "itemViewTypeName": "MyGame.UI.MyListItemView", "prefabFolder": "Assets/Prefabs", "overwrite": false }
```

#### `create_tabs`
Same parameters as `create_list` plus `"orientation": "horizontal" | "vertical"`.

#### `create_view` / `create_viewstack`
Same parameters as `create_popup`.

#### `mark_addressable`
```json
{ "action": "mark_addressable", "prefabPath": "Assets/Prefabs/MyPopup.prefab", "address": "UI/MyPopup" }
```

#### `create_scene`
```json
{ "action": "create_scene", "sceneName": "MyExample", "namespace": "MyNamespace", "sceneFolder": "Assets/MyExample/Scenes", "scriptFolder": "Assets/MyExample/Scripts" }
```
Generates Bootstrap, Context and StartCommand scripts and copies the scene template; scene wiring completes after the recompile.

### Workflow: Creating a new UI component
1. **Write scripts** — Create View, Mediator (and Data/ItemView/ItemMediator for Lists)
2. **Recompile** — `unity command recompile`, wait for `recompile_status` completed, check the console
3. **Call `FramewerkScaffold`** — `create_popup` / `create_list` / etc. to create prefabs with components attached
4. **Call `mark_addressable`** — Mark prefabs as Addressable
5. **Edit Context** — Add mediation and injection bindings to the project's Context class
6. **Verify** — enter Play mode, drive the UI, read the console

## Scene Setup Blueprint

When creating a new Framewerk test/demo scene, replicate this exact structure from `SampleScene`. **Do NOT use a single Canvas with children — use separate root Canvas objects per UI layer.**

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
- An `[Inject]` member is set only by the injector, never by hand. To give a consumer a different instance, inject it through a binder that binds that instance (see `CODE_STYLE.md`, *Injected Members*)
- Framework code goes in the UPM packages; project-specific code in `Assets/`

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
