# Framewerk 2.0 — Current Issues Audit

**Audit Date:** 2025-02-14  
**Auditor:** Claude (fresh analysis)

## Summary by Severity

| Severity | Count | Categories |
|----------|-------|------------|
| **Critical** | 1 | CancellationToken ignored in async methods |
| **High** | 4 | Race conditions, null refs, resource leaks |
| **Medium** | 8 | Validation gaps, silent failures, inconsistencies |
| **Low** | 6 | TODOs, naming, documentation gaps |

---

## Critical Issues

### C-1: CancellationToken Parameters Ignored in AssetManager

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/Managers/AssetManager.cs`  
**Lines:** 47-52, 57-67, etc.

**Issue:** All async methods accept a `CancellationToken ct = default` parameter but never use it. The Addressables operations proceed regardless of cancellation requests.

```csharp
public async Task<T> GetAssetAsync<T>(string address, Transform parent = null, 
    CancellationToken ct = default) where T : Object
{
    // ct is never checked or passed to Addressables
    var go = await Addressables.InstantiateAsync(address, parent, false).Task;
    // ...
}
```

**Impact:** 
- Cannot cancel long-running asset loads
- Memory leaks when scene transitions occur during loads
- Zombie objects may spawn after cancellation expected

**Recommendation:** Either implement proper cancellation propagation or remove the misleading parameter:
```csharp
public async Task<T> GetAssetAsync<T>(..., CancellationToken ct = default)
{
    ct.ThrowIfCancellationRequested();
    var handle = Addressables.InstantiateAsync(address, parent, false);
    await using (ct.Register(() => Addressables.Release(handle)))
    {
        return await handle.Task;
    }
}
```

---

## High Severity Issues

### H-1: Race Condition in PopupManager Registration

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/Popups/PopupManager.cs`  
**Lines:** 84-92

**Issue:** A guard was added for destroyed MonoBehaviours, but there's still a race window:

```csharp
private void OnPopupOpenedHandler(IPopupMediator popup)
{
    // Guard exists but...
    if (popup is MonoBehaviour mb && mb == null)
        return;
        
    _popups.Add(popup);  // <- popup could be destroyed between check and add
    popup.PopupClosedSignal.AddListener(OnPopupClosed);  // <- NPE possible here
}
```

**Impact:** NullReferenceException during rapid popup open/close cycles.

**Recommendation:** Use a try-catch or additional validation at signal listener attachment.

---

### H-2: Double Dispatch of PopupClosedSignal

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/Popups/PopupMediator.cs`  
**Lines:** 40-54

**Issue:** The `Close()` method dispatches `PopupClosedSignal` and then calls `Destroy(gameObject)`. But `Destroy` will trigger Unity's `OnDestroy`, which calls `OnRemove()`, which also dispatches the signal:

```csharp
public void Close()
{
    PopupClosedSignal.Dispatch(this);  // First dispatch
    Destroy(gameObject);               // Triggers OnRemove -> second dispatch
}

public override void OnRemove()
{
    PopupClosedSignal.Dispatch(this);  // Second dispatch!
    // ...
}
```

**Impact:** Listeners receive duplicate closure events, potentially causing double-processing bugs.

**Recommendation:** Track closure state or dispatch only in `OnRemove()`:
```csharp
private bool _closed;

public void Close()
{
    if (_closed) return;
    _closed = true;
    Destroy(gameObject);  // OnRemove will dispatch
}
```

---

### H-3: SingletonMono Static State Not Reset on Domain Reload

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/SingletonMono.cs`  
**Lines:** 18-22

**Issue:** The static fields `_instanceInitilized` and `applicationIsQuitting` are not reset when entering play mode with domain reload disabled:

```csharp
private static T _instance = null;
private static bool _instanceInitilized = false;
// No [RuntimeInitializeOnLoadMethod] to reset these
private static bool applicationIsQuitting = false;
```

**Impact:** Stale singleton instances from previous play sessions, leading to "already destroyed" warnings and null references.

**Recommendation:** Add reset method:
```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    _instance = null;
    _instanceInitilized = false;
    applicationIsQuitting = false;
}
```

---

### H-4: ListItemMediator Parent Search Has Silent Failure

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/UI/List/ListItemMediator.cs`  
**Lines:** 72-87

**Issue:** The `RegisterToList()` method logs an error but continues execution when no parent is found:

```csharp
private void RegisterToList()
{
    // ... search loop ...
    
    Debug.LogError($"ListItemMediator.RegisterToList() : IListItemParent not found");
    // No return, no exception — mediator exists but is orphaned
}
```

**Impact:** List items that fail registration remain functional but disconnected from their list, causing subtle data synchronization bugs.

**Recommendation:** Make failure explicit:
```csharp
throw new InvalidOperationException(
    $"ListItemMediator on '{gameObject.name}' could not find IListItemParent in hierarchy");
```

---

## Medium Severity Issues

### M-1: No Null Check on ViewConfig.ContextPrefixSO Access

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/AddressBuilder.cs`  
**Lines:** 142-149

**Issue:** `viewConfig?.ContextPrefixSO?.Prefix` uses null-conditional but the pattern is inconsistently applied:

```csharp
var ctx = new AddressContext
{
    ContextPrefix = viewConfig?.ContextPrefixSO?.Prefix,  // Fine
    // ...
};
```

However, many callsites assume `viewConfig` exists without validation.

**Impact:** Potential NullReferenceException when ViewConfig is misconfigured.

---

### M-2: AppFsm.Destroy() Can NPE on CurrentState

**File:** `Packages/com.dyskotron.framewerk.screenfsm/Runtime/Framewerk/AppStateMachine/AppFsm.cs`  
**Lines:** 131-135

**Issue:** 
```csharp
public virtual void Destroy()
{
    _nextStates.Clear();
    CloseCurrentState();  // Calls _currentState.PerformExit() 
                          // but _currentState could be null
}
```

**Impact:** NullReferenceException if `Destroy()` called before any state entered.

**Recommendation:** Add guard:
```csharp
public virtual void Destroy()
{
    _nextStates.Clear();
    if (_currentState != null)
        CloseCurrentState();
}
```

---

### M-3: ListMediator.SetData Missing Bounds Check

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/UI/List/ListMediator.cs`  
**Lines:** 17-35

**Issue:** The loop accesses `ItemMediators[i]` and `dataProviders[i]` with bounds checking on `ItemMediators.Count` but creates new mediators when `i >= CreatedMediatorsCount`:

```csharp
for (var i = 0; i < dataProviders.Count; i++)
{
    if (i < ItemMediators.Count)
    {
        var listItem = ItemMediators[i];  // Safe
        // ...
    }
    else if (i >= CreatedMediatorsCount)
    {
        CreateItemMediator(i, View.ItemPrefab, View.ContentsParent);
        // After this, ItemMediators[i] accessed but might not exist yet
    }
}
```

**Impact:** Index out of range if mediator registration is async or fails.

---

### M-4: ActionUpdater Swallows Exceptions After First Report

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/ActionUpdater.cs`  
**Lines:** 49-60

**Issue:** After an exception is reported once, subsequent identical exceptions are silently ignored:

```csharp
if (fr.ReportedException == null || fr.ReportedException.Message != ex.Message)
{
    fr.ReportedException = ex;
    Debug.LogErrorFormat(...);
}
// Otherwise: exception swallowed silently
```

**Impact:** Repeated errors become invisible, making debugging difficult.

---

### M-5: Addressables Memory Not Released on Preload Override

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/Managers/AssetManager.cs`  
**Lines:** 44-51, 107-114

**Issue:** If `PreloadAsset(Async)` is called twice for the same address, the second call returns early but doesn't release the potential duplicate handle:

```csharp
public async Task PreloadAssetAsync(string address, CancellationToken ct = default)
{
    if (_preloadedHandles.ContainsKey(address))
        return;  // No release of any in-flight operation
    
    var handle = Addressables.LoadAssetAsync<Object>(address);
    // ...
}
```

**Impact:** Potential memory leaks if same asset preloaded from multiple code paths.

---

### M-6: ExtendedMediator.AddButtonListener Removes All Listeners

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/UI/ExtendedMediator.cs`  
**Lines:** 69-74

**Issue:** 
```csharp
protected void AddButtonListener(Button button, Action func)
{
    button.onClick.RemoveAllListeners();  // Removes ALL listeners, not just ours
    button.onClick.AddListener(internalAction);
    // ...
}
```

**Impact:** External/Unity-set listeners are destroyed, breaking inspector-configured buttons.

**Recommendation:** Only remove previously tracked handler:
```csharp
if (ButtonHandlers.TryGetValue(button.gameObject, out var oldHandler))
    button.onClick.RemoveListener(oldHandler);
```

---

### M-7: MonoBinder Doesn't Handle Null Bindings Array

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/MonoBinder.cs`  
**Lines:** 57-72

**Issue:** While there's a null check for the array, individual binding validation is sparse:

```csharp
public void Bind(IInjectionBinder binder)
{
    if (bindings == null) return;
    
    foreach (var binding in bindings)
    {
        if (binding.reference == null) continue;
        
        // No validation of binder parameter being null
        binder.injector.Inject(binding.reference);  // NPE if binder null
        // ...
    }
}
```

---

### M-8: AppStateScreen View List Not Protected

**File:** `Packages/com.dyskotron.framewerk.screenfsm/Runtime/Framewerk/AppStateMachine/AppStateScreen.cs`  
**Lines:** 13-15

**Issue:** `_views` list is private but can be modified through reflection or by subclasses calling protected methods without tracking:

```csharp
private List<GameObject> _views = new List<GameObject>();

protected GameObject InstantiateView(...)
{
    var view = UiManager.InstantiateView(path, parent);
    _views.Add(view);  // Only tracked if this method used
    return view;
}
// Subclass could bypass and instantiate directly, causing leaks
```

---

## Low Severity Issues

### L-1: TODO Comments in Production Code

**Files:**
- `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/UI/List/ListBaseMediator.cs:51` — "TODO: just remove selected indexes..."
- `Packages/com.dyskotron.framewerk.editor/Editor/Wizards/TemplateSetResolver.cs` — "TODO: Support template set override for buttons"

---

### L-2: Inconsistent Naming Conventions

**Examples:**
- `_uiParent` vs `UiManager` (underscore prefix inconsistency)
- `contextView` (camelCase property) vs `ViewConfig` (PascalCase property)
- `TData` vs `TView` type parameter naming is good, but `T` alone used in some places

---

### L-3: Dead Code in Updater

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/Updater.cs`  
**Lines:** 25-30

**Issue:** The old implementation fields are declared but never used:

```csharp
private List<FrameAction> frameActions;    // Never used (ActionUpdater used instead)
private List<FrameAction> toAddActions;    // Never used
private List<Action> toRemoveActions;      // Never used
private long frameCounter;                  // Never used
private bool reset = false;                 // Never used
```

---

### L-4: Missing Interface Documentation

**Files:**
- `IAssetManager` — missing param docs for many methods
- `IUiManager` — missing return value documentation
- `IPopupManager` — no usage examples

---

### L-5: ListView Commented-Out Fields

**File:** `Packages/com.dyskotron.framewerk.ui/Runtime/Framewerk/UI/List/ListView.cs`  
**Lines:** 13-15

```csharp
//public VirtualListScroller ListScroller;
//public DragElement DragElement;
//public bool UnselectAllOnDrag = false;
```

These appear to be planned features or legacy code.

---

### L-6: Signal Listener Naming Inconsistency

**Issue:** Some signal handlers are named `*Handler`, others `On*Handler`, others just `On*`:

- `EnterFinishedHandler` (AppFsm)
- `OnPopupOpenedHandler` (PopupManager)
- `ListItemClickedHandler` (ListBaseMediator)

**Recommendation:** Standardize on one pattern (e.g., `On*` for all).

---

## Architecture Observations

### Not Issues, But Worth Noting:

1. **StrangeIoC Integration** — The framework extends StrangeIoC significantly. This is well-architected but creates tight coupling to the DI framework.

2. **Editor/Runtime Separation** — Good separation between editor tools and runtime code via packages.

3. **Pending View System** — The `FramewerkMVCSContext.RegisterPendingView` system handles View awakening before context initialization elegantly.

4. **Bundle System** — The `InstallBundle` pattern in `FramewerkCrossContext` is clean but underutilized in examples.

---

## Test Coverage Gaps

The test files exist but appear limited to Strange core functionality:
- No tests for `ListMediator` edge cases
- No tests for `PopupManager` lifecycle
- No tests for `AppFsm` state transitions
- No integration tests for UI components

---

## Recommendations Priority

1. **Immediate (Critical):** Implement CancellationToken support or remove misleading parameters
2. **Soon (High):** Fix double-dispatch in PopupMediator, add SingletonMono static reset
3. **Next Sprint (Medium):** Add null guards, improve error handling consistency
4. **Backlog (Low):** Clean up dead code, standardize naming, add documentation
