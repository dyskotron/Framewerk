# Addressable ID Structure

## Full Format
```
[ContextPrefix]/[CustomPrefix]/UI/[TypeKey]/[ClassName]
```

## Parts Breakdown

| Part | Source | Required | Example |
|------|--------|----------|---------|
| **ContextPrefix** | `ViewConfig.ContextPrefixSO.Prefix` (ScriptableObject) | Optional | `"Examples"`, `"ListPopupDemo"` |
| **CustomPrefix** | Passed at runtime via method argument | Optional | `"Features/Shop"` |
| **UI** | Hardcoded root (always present) | Always | `"UI"` |
| **TypeKey** | Derived from component type | Depends on type | `"Popup"`, `"List"`, `"List.ListItem"`, or empty |
| **ClassName** | Derived from C# class name | Always | `"ItemPopup"`, `"MainMenuView"` |

## ContextPrefix ScriptableObject

```csharp
[CreateAssetMenu(fileName = "ContextPrefix", menuName = "Framewerk/Context Prefix")]
public class ContextPrefix : ScriptableObject
{
    public string Prefix;
}
```

**Benefits:**
- Project-wide reuse (same SO across multiple contexts)
- Or atomic per-context setting (unique SO per context)
- Optional — null reference means this part is skipped

## ViewConfig Fields

```csharp
public partial class ViewConfig : MonoBehaviour
{
    [Header("Addressable ID Configuration")]
    public ContextPrefix ContextPrefixSO;  // Optional ScriptableObject reference
    
    [Header("Cameras")]
    public Camera Camera3d;
    public Camera UICamera;
    
    [Header("UI Containers")]
    public Transform Container3d;
    public Transform UiBottom;
    public Transform UiDefault;
    public Transform Popups;
    public Transform UiOverlay;
}
```

## AddressBuilder Utility

Shared static utility used by both UiManager and PopupManager:

```csharp
public static class AddressBuilder
{
    public const string UI_ROOT = "UI";
    
    public static class TypeKeys
    {
        public const string View = "";           // Base views have no TypeKey
        public const string Popup = "Popup";
        public const string List = "List";
        public const string ListItem = "List.ListItem";
    }
    
    public static string BuildAddress(string contextPrefix, string customPrefix, string typeKey, string className);
    public static string BuildAddress(ViewConfig viewConfig, string customPrefix, string typeKey, string className);
    public static string BuildAddress(ViewConfig viewConfig, string typeKey, string className);
}
```

## TypeKey Values

| Component Type | TypeKey Constant | Value |
|----------------|------------------|-------|
| Base View/Mediator | `AddressBuilder.TypeKeys.View` | *(empty)* |
| Popup | `AddressBuilder.TypeKeys.Popup` | `"Popup"` |
| List | `AddressBuilder.TypeKeys.List` | `"List"` |
| List Item | `AddressBuilder.TypeKeys.ListItem` | `"List.ListItem"` |

## Example Addresses

```
# Full address (all parts present)
# ContextPrefixSO.Prefix="Examples", customPrefix="Features/Shop"
Examples/Features/Shop/UI/Popup/ShopPopup

# No CustomPrefix (not passed at runtime)
# ContextPrefixSO.Prefix="Examples"
Examples/UI/Popup/ItemPopup

# No ContextPrefix (ContextPrefixSO is null), no CustomPrefix
UI/Popup/SimplePopup

# Base view (no TypeKey)
# ContextPrefixSO.Prefix="Examples"
Examples/UI/MainMenuView

# List
# ContextPrefixSO.Prefix="ListPopupDemo"
ListPopupDemo/UI/List/ItemList

# List item
# ContextPrefixSO.Prefix="Examples"
Examples/UI/List.ListItem/PlayerListItem
```

## Nullable Parts

Any part can be null/empty, resulting in shorter addresses:

| ContextPrefix | CustomPrefix | Result |
|---------------|--------------|--------|
| "Examples" | "Shop" | `Examples/Shop/UI/...` |
| "Examples" | null | `Examples/UI/...` |
| null | "Shop" | `Shop/UI/...` |
| null | null | `UI/...` |

## Runtime Usage

Both UiManager and PopupManager accept optional `customPrefix` parameter:

```csharp
// Without custom prefix - uses ContextPrefixSO only
UiManager.InstantiateView<MainMenuView>();
// → "Examples/UI/MainMenuView"

// With custom prefix passed at runtime
UiManager.InstantiateView<MainMenuView>(customPrefix: "Shop");
// → "Examples/Shop/UI/MainMenuView"

// Popups follow same pattern
PopupManager.InstantiatePopup<ItemPopup>();
// → "Examples/UI/Popup/ItemPopup"

PopupManager.InstantiatePopup<ShopPopup>(customPrefix: "Shop");
// → "Examples/Shop/UI/Popup/ShopPopup"
```

## Wizard Behavior

The UI Component Wizard:
1. User selects target Bootstrap/ViewConfig from dropdown
2. Wizard reads ContextPrefixSO from that ViewConfig
3. User can optionally enter CustomPrefix (for preview/addressable assignment)
4. TypeKey auto-derived from selected component type (Popup/List/View)
5. ClassName auto-derived from entered class name
6. Preview shows final Addressable ID

## Bootstrap Configuration

Each Bootstrap should have a ViewConfig component with the appropriate ContextPrefixSO:

1. Create a ContextPrefix ScriptableObject: `Create > Framewerk > Context Prefix`
2. Set its `Prefix` field (e.g., "Examples")
3. Assign it to `ViewConfig.ContextPrefixSO` on your Bootstrap

```
FramewerkDemoBootstrap
├── ViewConfig
│   └── ContextPrefixSO: ExamplesPrefix.asset (Prefix="Examples")

ListPopupDemoBootstrap  
├── ViewConfig
│   └── ContextPrefixSO: ListPopupDemoPrefix.asset (Prefix="ListPopupDemo")
```

## Migration Notes

### Breaking Changes
- `PopupManager.Init(resourcePath, parent)` removed — PopupManager now auto-configures via ViewConfig injection
- `FramewerkSettingsWindow` deprecated — settings now on ViewConfig
- `ViewConfig.ContextPrefix` (string) replaced with `ViewConfig.ContextPrefixSO` (ScriptableObject)

### Existing Addresses Need Manual Renaming
```
# Old → New
Examples/Popups/ExamplePopup      → Examples/UI/Popup/ExamplePopup
ListPopupDemo/ItemList            → ListPopupDemo/UI/List/ItemList
ListPopupDemo/Popups/ItemPopup    → ListPopupDemo/UI/Popup/ItemPopup
```

### Start Command Changes
Before:
```csharp
public override void Execute()
{
    PopupManager.Init("Examples", ViewConfig.Popups);
    // ...
}
```

After:
```csharp
public override void Execute()
{
    // PopupManager auto-configures via ViewConfig injection
    // Just create a ContextPrefix SO and assign to ViewConfig.ContextPrefixSO
    // ...
}
```

### Migration Checklist
1. Create ContextPrefix ScriptableObject for each context
2. Assign to ViewConfig.ContextPrefixSO on each Bootstrap
3. Remove PopupManager.Init() calls from start commands
4. Update Addressable asset addresses to new format
5. Update any hardcoded address strings in code
