# Framewerk AI Coding Guide

**For AI coding assistants (Claude Code, Cursor, Copilot, etc.)**

This document explains how to properly work with Framewerk. **DO NOT** try to create Unity prefabs, scenes, or wire UI components manually. Framewerk provides MCP tools that handle all of this automatically.

---

## ⚠️ Critical Rules

1. **NEVER create Unity prefabs manually** — use the `framewerk_scaffold` MCP tool
2. **NEVER create Unity scenes manually** — use the `create_scene` action
3. **NEVER try to wire Unity serialized fields via code** — the wizard handles this
4. **ALWAYS use the framework's MCP tools** for creating UI components

Unity automation is hard. The framework has wizards specifically designed for this. Use them.

---

## MCP Tool: `framewerk_scaffold`

All Framewerk scaffolding is done through this single MCP tool. Call it via:

```bash
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {...}}'
```

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

**Wait for Unity to compile** after this step.

### Step 2: Create View/Mediator Scripts

Write the View and Mediator C# scripts manually. The wizard needs these types to exist before creating prefabs.

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

### Step 3: Create Prefabs via MCP

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

### MCP not responding
- Check if Unity is running
- Open MCP window: `Window > MCP for Unity > Toggle MCP Window`
- Verify port 8080 is accessible

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
❌ Don't bypass the MCP tools  

---

## Reference

- **MCP Tool Source:** `Packages/com.dyskotron.framewerk.editor/Editor/Wizards/ComponentScaffoldMcpTools.cs`
- **Interactive Wizards:** `Framewerk > Create Scene` and `Framewerk > Create UI Component` menus
- **Templates:** `Packages/com.dyskotron.framewerk.editor/Editor/Wizards/Templates/`
