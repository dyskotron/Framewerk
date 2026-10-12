# Framewerk AI Coding Guide

**For AI coding assistants (Claude Code, Cursor, Copilot, etc.)**

This document explains how to properly work with Framewerk. **DO NOT** try to create Unity prefabs, scenes, or wire UI components manually. Framewerk provides scaffold tooling that handles all of this automatically.

---

## ⚠️ Critical Rules

1. **NEVER create Unity prefabs manually** — use `FramewerkScaffold` through the Unity CLI
2. **NEVER create Unity scenes manually** — use the `create_scene` action
3. **NEVER try to wire Unity serialized fields via code** — the wizard handles this
4. **ALWAYS use the framework's scaffold tooling** for creating UI components

Unity automation is hard. The framework has wizards specifically designed for this. Use them.

---

## Scaffold Tool: `FramewerkScaffold`

All Framewerk scaffolding is done through one static entry point, `Framewerk.Editor.Wizards.FramewerkScaffold.Execute(json)`, which runs inside the Editor. Call it through the Unity CLI's `eval` command (Pipeline package):

```bash
unity command eval --project-path <repo> --result-only --timeout 60000 'return Framewerk.Editor.Wizards.FramewerkScaffold.Execute("{\"action\":\"create_popup\",\"name\":\"MyPopup\",\"namespace\":\"MyNamespace\",\"viewTypeName\":\"MyNamespace.MyPopupView\",\"prefabFolder\":\"Assets/MyExample/Prefabs\"}");'
```

For anything longer, write the C# to a scratch file and run `unity command eval_file -- --file <path> --timeout 60000`. The result is a JSON string with `success`, `message` and action-specific paths. See `CLAUDE.md` for the full Editor workflow rules.

### Available Actions

| Action | Description | Parameters |
|--------|-------------|------------|
| `create_scene` | Creates a complete scene with Bootstrap, Context, and StartCommand scripts | `sceneName`, `namespace`, `sceneFolder`, `scriptFolder` |
| `create_popup` | Creates a popup prefab from template | `name`, `namespace`, `viewTypeName`, `prefabFolder`, `overwrite` (optional) |
| `create_list` | Creates a list prefab + item prefab, properly linked | `name`, `namespace`, `viewTypeName`, `itemViewTypeName`, `prefabFolder`, `overwrite` (optional) |
| `create_tabs` | Creates a tabs prefab + tab item prefab | `name`, `namespace`, `viewTypeName`, `itemViewTypeName`, `prefabFolder`, `orientation` (horizontal/vertical), `overwrite` (optional) |
| `create_view` | Creates a simple view prefab | `name`, `namespace`, `viewTypeName`, `prefabFolder`, `overwrite` (optional) |
| `create_viewstack` | Creates a viewstack prefab | `name`, `namespace`, `viewTypeName`, `prefabFolder`, `overwrite` (optional) |
| `mark_addressable` | Marks a prefab as addressable with specified address | `prefabPath`, `address` (optional) |

---

## Workflow: Creating a New Scene/Example

### Step 1: Create the Scene

```json
{
  "action": "create_scene",
  "sceneName": "MyExample",
  "namespace": "MyNamespace",
  "sceneFolder": "Assets/MyExample/Scenes",
  "scriptFolder": "Assets/MyExample/Scripts"
}
```

This creates:
- `MyExampleBootstrap.cs` — Scene entry point
- `MyExampleContext.cs` — Dependency injection bindings
- `MyExampleStartCommand.cs` — Startup logic
- `MyExample.unity` — The scene file

**Recompile and wait** after this step (`unity command recompile`, then poll `recompile_status`).

### Step 2: Create View/Mediator Scripts

Write the View and Mediator C# scripts manually. The wizard needs these types to exist before creating prefabs, so recompile again once they are written.

**View example:**
```csharp
using Framewerk.UI;
using UnityEngine.UI;
using TMPro;

namespace MyNamespace
{
    public class MyPopupView : PopupView
    {
        public Button CloseButton;
        public TextMeshProUGUI TitleText;
    }
}
```

**Mediator example:**
```csharp
using Framewerk.UI;

namespace MyNamespace
{
    public class MyPopupMediator : PopupMediator<MyPopupView>
    {
        public override void OnRegister()
        {
            base.OnRegister();
            AddButtonListener(View.CloseButton, Close);
        }
    }
}
```

### Step 3: Create Prefabs via FramewerkScaffold

Each JSON block below is the parameter object passed to `FramewerkScaffold.Execute`.

**For a Popup:**
```json
{
  "action": "create_popup",
  "name": "MyPopup",
  "namespace": "MyNamespace",
  "viewTypeName": "MyNamespace.MyPopupView",
  "prefabFolder": "Assets/MyExample/Prefabs"
}
```

**For a List (creates both list + item prefabs):**
```json
{
  "action": "create_list",
  "name": "MyList",
  "namespace": "MyNamespace",
  "viewTypeName": "MyNamespace.MyListView",
  "itemViewTypeName": "MyNamespace.MyListItemView",
  "prefabFolder": "Assets/MyExample/Prefabs"
}
```

**For a simple View:**
```json
{
  "action": "create_view",
  "name": "MyView",
  "namespace": "MyNamespace",
  "viewTypeName": "MyNamespace.MyView",
  "prefabFolder": "Assets/MyExample/Prefabs"
}
```

### Step 4: Mark as Addressable

```json
{
  "action": "mark_addressable",
  "prefabPath": "Assets/MyExample/Prefabs/MyPopup.prefab",
  "address": "MyExample/UI/Popup/MyPopup"
}
```

### Step 5: Wire Up Context and StartCommand

**In Context (add mediation bindings):**
```csharp
mediationBinder.Bind<MyPopupView>().To<MyPopupMediator>();
```

**In StartCommand (show the UI):**
```csharp
PopupManager.InstantiatePopup<MyPopupView>();
// or
UIManager.Instantiate<MyView>();
```

---

## Component Types

### Popup
- Extends `PopupView` / `PopupMediator<T>`
- Shown via `PopupManager.InstantiatePopup<T>()`
- Has built-in show/hide animations and lifecycle

### List
- Extends `ListView` / `ListMediator<TView, TData>`
- Item extends `ListItemView` / `ListItemMediator<TView, TData>`
- Automatically handles item pooling and data binding

### View
- Extends `View` / `ExtendedMediator<T>`
- Basic UI component
- Shown via `UIManager.Instantiate<T>()`

### Tabs (Horizontal/Vertical)
- Special type of List that controls a ViewStack
- Use `create_tabs` action with orientation parameter (`horizontal` or `vertical`)
- Creates tab container prefab + tab item prefab

### ViewStack
- Container that holds child views, showing one at a time
- Extends `ViewStackView` / `ViewStackMediator<T>`
- Use `create_viewstack` action
- Typically controlled by a Tabs component

---

## Base Classes Reference

| Component | View Base | Mediator Base |
|-----------|-----------|---------------|
| Popup | `PopupView` | `PopupMediator<TView>` |
| List | `ListView` | `ListMediator<TView, TData>` |
| List Item | `ListItemView` | `ListItemMediator<TView, TData>` |
| Tabs | `ListView` | `TabContainerMediator<TView, TData, TItemView, TViewStack>` |
| Tab Item | `ListItemView` | `TabItemMediator<TView, TData>` |
| ViewStack | `ViewStackView` | `ViewStackMediator<TView>` |
| View | `View` | `ExtendedMediator<TView>` |

---

## Common Patterns

### Showing a Popup
```csharp
PopupManager.InstantiatePopup<MyPopupView>();
```

### Showing a Popup with Data
```csharp
var data = new MyPopupData { Title = "Hello", Message = "World" };
PopupManager.InstantiatePopup<MyPopupView>(new object[] { data });
```

### Instantiating a View
```csharp
UIManager.Instantiate<MyView>();
```

### Button Handling in Mediator
```csharp
public override void OnRegister()
{
    base.OnRegister();
    AddButtonListener(View.MyButton, OnMyButtonClicked);
}

private void OnMyButtonClicked()
{
    // Handle click
}
```

### Dispatching Signals
```csharp
[Inject] public MySignal MySignal { get; set; }

private void OnButtonClicked()
{
    MySignal.Dispatch();
}
```

---

## Addressable Naming Convention

```
{ContextPrefix}/UI/{TypeKey}/{PrefabName}
```

Examples:
- `MyExample/UI/Popup/MyPopup`
- `MyExample/UI/List/ContactList`
- `MyExample/UI/ListItem/ContactListItem`
- `MyExample/UI/View/SettingsView`

---

## Troubleshooting

### Editor not reachable
- `unity status --project-path <repo>` must report `ready`
- Unity is busy (domain reload, import) for a few seconds after changes: retry with a longer `--timeout`
- If the Editor started with compile errors it is in Safe Mode and the Pipeline package is off: fix the code, `unity close --force`, `unity open`

### Prefab creation fails
- Ensure View/Mediator scripts exist and compile
- Wait for Unity to finish compiling before creating prefabs
- Check that namespace and type names match exactly

### Addressable not found at runtime
- Verify prefab is marked as addressable
- Check address matches what code expects
- Ensure Addressable groups are built

---

## What NOT to Do

❌ Don't create `.prefab` files by writing YAML/JSON  
❌ Don't try to set Unity serialized fields via file manipulation  
❌ Don't create scene files manually  
❌ Don't create helper scripts/wizards — they already exist  
❌ Don't bypass the scaffold tooling  
❌ Don't install third-party Unity MCP plugins — the Unity CLI and Pipeline package are the only Editor bridge  

---

## Reference

- **Scaffold Tool Source:** `Packages/com.dyskotron.framewerk.editor/Editor/Wizards/FramewerkScaffold.cs`
- **Interactive Wizards:** `Framewerk > Create Scene` and `Framewerk > Create UI Component` menus
- **Templates:** `Packages/com.dyskotron.framewerk.editor/Editor/Wizards/Templates/`
