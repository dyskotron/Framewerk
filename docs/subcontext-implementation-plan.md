# Sub-Context Implementation Plan

## Problem Summary

When `UiManager.InstantiateViewsAsync()` creates views with local dependencies (`mediatorInjects`), the current temp-binding approach fails because:

1. Views bubble up to find their Context
2. Context.AddView() triggers MediationBinder.Trigger()
3. MediationBinder creates mediator and injects it using its InjectionBinder
4. The temp bindings in UiManager never reach the mediator

We need a lightweight sub-context that provides:
- Its own InjectionBinder (parented to main, so it resolves global deps)
- Its own MediationBinder (only the needed view→mediator mappings)

---

## Architecture

### New Class: `ViewScope`

A minimal sub-context that lives only during view instantiation.

```
Location: Framewerk/Managers/ViewScope.cs
```

**Responsibilities:**
- Create child InjectionBinder (parented to main)
- Create local MediationBinder (copies needed bindings from main)
- Bind temp dependencies into child InjectionBinder
- Intercept view registration during instantiation
- Clean up on dispose

---

## Integration Flow

### Current API (unchanged):
```csharp
var group = new ViewGroup()
    .Add<PlayerHudView>()
    .Add<InventoryView>();

var result = await uiManager.InstantiateViewsAsync(group, ct, playerModel, inventoryData);
```

### Internal Flow (new):

```
InstantiateViewsAsync(group, bindings)
    │
    ▼
┌─────────────────────────────────────────────────────┐
│ 1. Determine which view types need local bindings   │
│    (have mediators with [FallbackInject] or params) │
└─────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────┐
│ 2. Create ViewScope                                 │
│    - Child InjectionBinder (parent = main binder)   │
│    - Local MediationBinder                          │
│    - Copy mediation bindings for group's view types │
│    - Bind provided dependencies                     │
└─────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────┐
│ 3. Set ViewScope as "active" (thread-local/static)  │
└─────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────┐
│ 4. Instantiate views                                │
│    Views call Context.AddView() → intercepted       │
│    → ViewScope.mediationBinder.Trigger()            │
└─────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────┐
│ 5. Clear active ViewScope, dispose                  │
└─────────────────────────────────────────────────────┘
```

---

## Implementation Details

### 1. ViewScope Class

```csharp
namespace Framewerk.Managers
{
    internal class ViewScope : IDisposable
    {
        // Thread-safe stack for nested scopes (unlikely but safe)
        [ThreadStatic]
        private static Stack<ViewScope> _activeScopes;
        
        public static ViewScope Current => 
            _activeScopes?.Count > 0 ? _activeScopes.Peek() : null;

        public IInjectionBinder InjectionBinder { get; }
        public IMediationBinder MediationBinder { get; }
        
        private readonly HashSet<Type> _managedViewTypes;
        
        public ViewScope(
            IInjectionBinder parentBinder,
            IMediationBinder parentMediationBinder,
            IEnumerable<Type> viewTypes,
            object[] bindings,
            bool bindInterfaces,
            bool bindBaseClasses)
        {
            _managedViewTypes = new HashSet<Type>(viewTypes);
            
            // 1. Create child injection binder
            InjectionBinder = CreateChildBinder(parentBinder);
            
            // 2. Create local mediation binder
            MediationBinder = CreateLocalMediationBinder(parentMediationBinder);
            
            // 3. Bind provided dependencies
            BindingUtils.Bind(InjectionBinder, bindInterfaces, bindBaseClasses, bindings);
            
            // 4. Push onto active stack
            _activeScopes ??= new Stack<ViewScope>();
            _activeScopes.Push(this);
        }
        
        public bool ManagesViewType(Type viewType) => _managedViewTypes.Contains(viewType);
        
        public void Dispose()
        {
            _activeScopes?.Pop();
            // Unbind happens automatically since binder is discarded
        }
        
        private IInjectionBinder CreateChildBinder(IInjectionBinder parent)
        {
            var child = new CrossContextInjectionBinder();
            child.CrossContextBinder = parent;  // Parent resolution
            return child;
        }
        
        private IMediationBinder CreateLocalMediationBinder(IMediationBinder source)
        {
            var local = new SignalMediationBinder();
            local.injectionBinder = InjectionBinder;
            
            // Copy ONLY the bindings we need
            foreach (var viewType in _managedViewTypes)
            {
                var binding = source.GetBinding(viewType) as IMediationBinding;
                if (binding != null)
                {
                    CopyMediationBinding(local, binding, viewType);
                }
            }
            return local;
        }
        
        private void CopyMediationBinding(IMediationBinder target, IMediationBinding source, Type viewType)
        {
            var newBinding = target.Bind(viewType);
            var values = source.value as object[];
            foreach (Type mediatorType in values)
            {
                newBinding.To(mediatorType);
            }
            if (source.abstraction != null && !source.abstraction.Equals(BindingConst.NULLOID))
            {
                newBinding.ToAbstraction(source.abstraction as Type);
            }
        }
    }
}
```

### 2. Intercept View Registration

**Option A: Modify FramewerkMVCSContext.AddView** (recommended)

```csharp
// In FramewerkMVCSContext.cs
override public void AddView(object view)
{
    var viewType = view.GetType();
    var scope = ViewScope.Current;
    
    // If active scope manages this view type, use its binder
    if (scope != null && scope.ManagesViewType(viewType))
    {
        scope.MediationBinder.Trigger(MediationEvent.AWAKE, view as IView);
        return;
    }
    
    // Otherwise, normal flow
    if (mediationBinder != null)
    {
        mediationBinder.Trigger(MediationEvent.AWAKE, view as IView);
    }
    else
    {
        cacheView(view as MonoBehaviour);
    }
}
```

Same pattern for `RemoveView`, `EnableView`, `DisableView`.

### 3. UiManager Changes

```csharp
// In UiManager.cs
public async Task<ViewGroupResult> InstantiateViewsAsync(
    ViewGroup group, 
    CancellationToken ct = default, 
    params object[] bindings)
{
    if (group == null || group.Entries.Count == 0)
        return new ViewGroupResult();

    // Collect view types
    var viewTypes = group.Entries.Select(e => e.ViewType).ToList();
    
    // Prepare all bindings (provided + fallback instances)
    var allBindings = PrepareBindings(viewTypes, bindings);
    
    // Create scope if we have any bindings
    using var scope = allBindings.Length > 0 
        ? new ViewScope(
            InjectionBinder, 
            MediationBinder, 
            viewTypes, 
            allBindings,
            BindInterfaces,
            BindBaseClasses)
        : null;
    
    var result = new ViewGroupResult();
    foreach (var entry in group.Entries)
    {
        var parent = entry.Parent ?? _uiParent;
        var path = GetViewPath(entry.ViewType, null);
        var go = await AssetManager.GetAssetAsync<GameObject>(path, parent, ct);
        result.Add(entry.ViewType, go);
    }
    
    return result;
}

private object[] PrepareBindings(List<Type> viewTypes, object[] provided)
{
    // Existing FallbackInject logic, refactored
    var allBindings = new List<object>();
    if (provided != null) allBindings.AddRange(provided);
    
    var providedTypes = new HashSet<Type>();
    foreach (var b in allBindings)
        foreach (var t in BindingUtils.GetBindTypes(b, BindInterfaces, BindBaseClasses))
            providedTypes.Add(t);
    
    foreach (var viewType in viewTypes)
    {
        var mediatorType = GetMediatorTypeForView(viewType);
        if (mediatorType != null)
            CollectFallbackInjectInstances(mediatorType, providedTypes, allBindings);
    }
    
    return allBindings.ToArray();
}
```

---

## Lifecycle

| Event | Action |
|-------|--------|
| `InstantiateViewsAsync` called | ViewScope created, pushed to stack |
| View instantiates, Awake fires | `Context.AddView` checks `ViewScope.Current` |
| View type in scope? | Use scope's MediationBinder |
| View type not in scope? | Use main MediationBinder |
| All views instantiated | `using` disposes ViewScope |
| Scope disposed | Popped from stack, bindings discarded |

---

## Key Design Decisions

1. **Thread-static stack** — Handles async properly, supports rare nested cases
2. **Type whitelist** — Scope only intercepts its own view types
3. **Copy bindings, don't share** — Isolation, no side effects
4. **Child InjectionBinder** — Resolves global deps via parent chain
5. **Dispose pattern** — Automatic cleanup via `using`

---

## Files to Modify

1. **NEW**: `Framewerk/Managers/ViewScope.cs`
2. **EDIT**: `Framewerk/StrangeCore/FramewerkMVCSContext.cs` — AddView/RemoveView/EnableView/DisableView
3. **EDIT**: `Framewerk/Managers/UiManager.cs` — InstantiateViewsAsync, remove old temp-bind logic

---

## Testing Strategy

1. **Unit test**: ViewScope creation/disposal, stack behavior
2. **Integration test**: ViewGroup with local bindings, verify mediator injection
3. **Edge case**: Nested ViewGroups (scope stacking)
4. **Edge case**: View type not in scope falls through to main binder

---

## Open Questions

1. **RemoveView timing** — When view is destroyed, scope is already gone. Do we need to track which mediators were created by which scope? *Probably not* — MediationBinder stores mediator on the GameObject, so destruction just removes the component.

2. **SignalMediationBinder specifics** — Need to verify the copy approach works with signal bindings. May need to copy `[ListensTo]` setup too.

3. **Async caveat** — Thread-static works for single-threaded Unity main thread. If instantiation truly parallelizes (unlikely but possible), would need `AsyncLocal<T>` instead.
