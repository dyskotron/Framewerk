# Scriptable Address Resolver - Design Document

## Overview

A **ScriptableObject-based system** that lets users customize how Addressable IDs are generated/resolved without modifying framework code.

---

## Current System Analysis

### How Addresses Are Built Now

**`AddressBuilder.cs`** is the central utility:
```csharp
// Format: contextPrefix/customPrefix/UI/typeKey/className
BuildAddress(contextPrefix, customPrefix, typeKey, className)
```

**Variables currently used:**
| Variable | Source | Example |
|----------|--------|---------|
| `contextPrefix` | `ViewConfig.ContextPrefixSO.Prefix` | `"Examples"`, `"ListPopupDemo"` |
| `customPrefix` | Passed at runtime | `"Menu"`, `null` |
| `typeKey` | Component type constant | `""` (View), `"Popup"`, `"List"`, `"List.ListItem"` |
| `className` | Type name minus suffix | `"ExamplePopup"`, `"MenuItem"` |

**Example output:** `Examples/UI/Popup/ExamplePopup`

### Where Addresses Are Generated

1. **Runtime (loading)**
   - `UiManager.GetViewPath()` → `AddressBuilder.BuildAddress()`
   - `PopupManager.GetPopupPath()` → `AddressBuilder.BuildAddress()`

2. **Editor (wizard)**
   - `ComponentScaffoldWizard.Generate()` → `AddressBuilder.BuildAddress()`
   - Sets the address on the Addressable entry

---

## Proposed Design

### 1. The ScriptableObject: `AddressResolverConfig`

```csharp
[CreateAssetMenu(fileName = "AddressResolverConfig", menuName = "Framewerk/Address Resolver Config")]
public class AddressResolverConfig : ScriptableObject
{
    [Tooltip("Pattern template for address generation. Use {tokens} for variables.")]
    public string Pattern = "{ContextPrefix}/{CustomPrefix}/UI/{TypeKey}/{ClassName}";
    
    [Tooltip("How to handle empty/null tokens")]
    public EmptyTokenBehavior EmptyTokenBehavior = EmptyTokenBehavior.Skip;
    
    [Tooltip("Separator between path segments")]
    public string Separator = "/";
    
    [Header("Type Key Overrides (optional)")]
    public string ViewTypeKey = "";
    public string PopupTypeKey = "Popup";
    public string ListTypeKey = "List";
    public string ListItemTypeKey = "List.ListItem";
}

public enum EmptyTokenBehavior
{
    Skip,           // Omit empty segments (current behavior)
    KeepEmpty,      // Keep empty string
    UseDefault      // Use a default value (requires DefaultValue field)
}
```

### 2. Available Tokens

| Token | Description | Example Value |
|-------|-------------|---------------|
| `{ContextPrefix}` | From ViewConfig.ContextPrefixSO | `"Examples"` |
| `{CustomPrefix}` | Runtime grouping prefix | `"Menu"` |
| `{TypeKey}` | Component type key | `"Popup"` |
| `{ClassName}` | The component name | `"ExamplePopup"` |
| `{UI}` | Hardcoded constant | `"UI"` |
| `{TypeName}` | Full type name | `"ExamplePopupView"` |
| `{Namespace}` | Type's namespace | `"Game.UI.Popups"` |

### 3. Example Patterns

| Use Case | Pattern | Example Output |
|----------|---------|----------------|
| **Default (current)** | `{ContextPrefix}/{CustomPrefix}/UI/{TypeKey}/{ClassName}` | `Examples/UI/Popup/ExamplePopup` |
| **Flat structure** | `UI/{ClassName}` | `UI/ExamplePopup` |
| **By namespace** | `{Namespace}/{ClassName}` | `Game.UI.Popups/ExamplePopup` |
| **Type-first** | `{TypeKey}/{ContextPrefix}/{ClassName}` | `Popup/Examples/ExamplePopup` |
| **No UI root** | `{ContextPrefix}/{TypeKey}/{ClassName}` | `Examples/Popup/ExamplePopup` |
| **Simple** | `{TypeKey}/{ClassName}` | `Popup/ExamplePopup` |

---

## Integration Design

### Option A: Via ViewConfig (Recommended)

Add a reference to `AddressResolverConfig` on `ViewConfig`:

```csharp
public partial class ViewConfig : MonoBehaviour
{
    [Header("Addressable ID Configuration")]
    public ContextPrefix ContextPrefixSO;
    
    [Header("Address Resolution")]
    [Tooltip("Custom address pattern. If null, uses default pattern.")]
    public AddressResolverConfig AddressResolver;
}
```

**Pros:**
- Per-context customization (different games/modules can have different patterns)
- No global state
- Already injected everywhere

**Cons:**
- Slightly more setup per context

### Option B: Via FramewerkSettings (Project-wide)

Create a project-wide settings asset:

```csharp
public class FramewerkSettings : ScriptableObject
{
    public static FramewerkSettings Instance => ...; // Resources or preloaded
    
    public AddressResolverConfig DefaultAddressResolver;
}
```

**Pros:**
- Single configuration point
- Easier for simple projects

**Cons:**
- Less flexible for multi-context scenarios

### Recommendation: **Hybrid Approach**

1. `ViewConfig.AddressResolver` (per-context override)
2. Falls back to `FramewerkSettings.DefaultAddressResolver` (project default)
3. Falls back to hardcoded default pattern (current behavior)

---

## AddressBuilder Changes

```csharp
public static class AddressBuilder
{
    // New main entry point
    public static string BuildAddress(AddressResolverConfig resolver, AddressContext ctx)
    {
        if (resolver == null)
            return BuildAddressDefault(ctx);
            
        return resolver.Resolve(ctx);
    }
    
    // Context object containing all variables
    public struct AddressContext
    {
        public string ContextPrefix;
        public string CustomPrefix;
        public string TypeKey;
        public string ClassName;
        public string TypeName;      // Full type name
        public string Namespace;     // Type's namespace
    }
    
    // Backward-compatible overload
    public static string BuildAddress(ViewConfig viewConfig, string customPrefix, string typeKey, string className)
    {
        var ctx = new AddressContext
        {
            ContextPrefix = viewConfig?.ContextPrefixSO?.Prefix,
            CustomPrefix = customPrefix,
            TypeKey = typeKey,
            ClassName = className
        };
        
        var resolver = viewConfig?.AddressResolver;
        return BuildAddress(resolver, ctx);
    }
}
```

---

## AddressResolverConfig.Resolve() Implementation

```csharp
public string Resolve(AddressBuilder.AddressContext ctx)
{
    var tokens = new Dictionary<string, string>
    {
        { "ContextPrefix", ctx.ContextPrefix },
        { "CustomPrefix", ctx.CustomPrefix },
        { "TypeKey", ctx.TypeKey },
        { "ClassName", ctx.ClassName },
        { "TypeName", ctx.TypeName },
        { "Namespace", ctx.Namespace },
        { "UI", "UI" }
    };
    
    // Simple regex replacement: {TokenName} → value
    string result = Regex.Replace(Pattern, @"\{(\w+)\}", match =>
    {
        string key = match.Groups[1].Value;
        if (tokens.TryGetValue(key, out string value))
        {
            if (string.IsNullOrEmpty(value) && EmptyTokenBehavior == EmptyTokenBehavior.Skip)
                return ""; // Will create double separators, cleaned up below
            return value;
        }
        return match.Value; // Keep unknown tokens
    });
    
    // Clean up multiple separators
    result = Regex.Replace(result, $"{Regex.Escape(Separator)}+", Separator);
    result = result.Trim(Separator[0]);
    
    return result;
}
```

---

## Editor Integration (Wizard)

The wizard already uses `AddressBuilder.BuildAddress()`. With the new system:

1. Wizard reads `ViewConfig.AddressResolver` from selected bootstrap
2. Passes it to `AddressBuilder.BuildAddress()`
3. Preview updates live as pattern changes
4. Same code path for runtime and editor

**No separate logic needed** — wizard and runtime use the same resolver.

---

## Edge Cases & Considerations

### 1. Different Resolvers per Manager?

**Not recommended initially.** The resolver is on ViewConfig, which is shared. If truly needed:
- Add `typeKey` to pattern tokens (already there)
- Different patterns can handle different types via the same resolver

Future: Could add `Dictionary<string, AddressResolverConfig> TypeOverrides` if needed.

### 2. Migration Path

**Backward compatible by design:**
- If `AddressResolver` is null → existing `BuildAddressDefault()` runs
- No changes required for existing projects
- Can migrate gradually by creating resolvers

### 3. Runtime vs Editor

**Same code path.** `AddressResolverConfig` is a ScriptableObject:
- Loaded at runtime (included in build if referenced)
- Available in editor (wizard uses it)
- No conditional compilation needed

### 4. Validation

Add validation in the ScriptableObject:
```csharp
private void OnValidate()
{
    // Check pattern has required tokens
    if (!Pattern.Contains("{ClassName}"))
        Debug.LogWarning("Pattern should include {ClassName}");
        
    // Test with sample values
    var sample = Resolve(new AddressContext { ClassName = "Test", TypeKey = "Popup" });
    Debug.Log($"Sample output: {sample}");
}
```

---

## Files to Create/Modify

### New Files
1. `AddressResolverConfig.cs` — The ScriptableObject
2. `AddressContext.cs` — (optional, could be nested struct in AddressBuilder)

### Modified Files
1. `AddressBuilder.cs` — Add resolver support
2. `ViewConfig.cs` — Add `AddressResolver` field
3. `ComponentScaffoldWizard.cs` — Preview uses resolver (already uses BuildAddress)
4. (Optional) `FramewerkSettings.cs` — For project-wide default

---

## Open Questions

1. **Should TypeKey overrides live on the resolver?**
   - Current design: Yes, allows customizing per-type keys
   - Alternative: Keep TypeKeys as constants, let pattern handle it

2. **Do we need namespace-aware resolution?**
   - Adds `{Namespace}` token
   - Requires passing `Type` to `AddressBuilder` (not just className)
   - More complex but more powerful

3. **Custom prefix source?**
   - Currently passed at runtime
   - Could also come from the resolver (e.g., scene-based prefix)

4. **Advanced: Scripted resolution?**
   - Instead of pattern string, C# callback
   - Maximum flexibility, but harder to preview in editor
   - Could be future extension via `virtual` method

---

## Summary

| Aspect | Decision |
|--------|----------|
| **Core type** | `AddressResolverConfig : ScriptableObject` |
| **Pattern format** | String with `{Token}` interpolation |
| **Integration point** | `ViewConfig.AddressResolver` field |
| **Fallback** | Hardcoded default (current behavior) |
| **Editor support** | Same code path, works in wizard |
| **Migration** | Fully backward compatible |
| **Complexity** | Low — minimal new code |
