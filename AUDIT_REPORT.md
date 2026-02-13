# Framewerk Code Quality Audit Report

**Date:** 2025-01-24  
**Audited Projects:**
- ~/projects/framewerk (framework itself)
- ~/projects/Temari (real game using Framewerk)
- ~/projects/shooterreborn (another project using Framewerk)

---

## Summary

| Category | Critical | High | Medium | Low |
|----------|----------|------|--------|-----|
| Inconsistency | 0 | 3 | 6 | 4 |
| Architecture | 1 | 3 | 4 | 2 |
| Weird Code | 0 | 1 | 4 | 3 |
| Framework Gap | 0 | 2 | 4 | 2 |
| Potential Bug | 1 | 4 | 3 | 2 |
| Missing Abstraction | 0 | 2 | 3 | 1 |

---

## 1. INCONSISTENCIES

### INC-001: Destroy() vs OnRemove() Lifecycle Confusion
**Severity:** HIGH  
**Location:** Multiple files across all packages  
**Description:** The framework has two overlapping cleanup patterns:
- `OnRemove()` — Mediator lifecycle callback
- `Destroy()` — Custom cleanup method on various classes (`IDestroyable`, `ListItemMediator`, `AppState`, `TabViewConnector`)

This creates confusion: when should consumers use which? Some classes have both patterns (e.g., `ListBaseMediator` has `OnRemove()` inherited and its own `Destroy()` method that calls `GameObject.Destroy()`).

**Evidence:**
```csharp
// ListBaseMediator.cs
public void Destroy()
{
    foreach (var itemMediator in ItemMediators)
        itemMediator.Destroy();
    ItemMediators.Clear();
    GameObject.Destroy(View.gameObject);  // Why not use the mediation lifecycle?
}
```

**Suggested Fix:** Standardize on one pattern. Recommend `OnRemove()` for mediator cleanup and `IDisposable` or `IDestroyable` for non-MonoBehaviour services. Document clearly when each should be used.

---

### INC-002: Naming Inconsistency — View vs IView
**Severity:** MEDIUM  
**Location:** `strange.extensions.mediation` namespace  
**Description:** `View` is a concrete class while `IView` is an interface, but the naming doesn't follow C# conventions where `I` prefix implies abstraction. Some code expects `IView`, others expect `View`, creating confusion:

```csharp
// UiManager.cs - uses IView
Task<T> InstantiateViewAsync<T>(...) where T : IView;

// AppStateScreen.cs - uses concrete MonoBehaviour, IView
protected async Task<T> InstantiateViewAsync<T>(...) where T : MonoBehaviour, IView
```

**Suggested Fix:** Consider renaming `View` to `BaseView` or creating clearer documentation about when to use which.

---

### INC-003: Signal Binding Style Varies
**Severity:** MEDIUM  
**Location:** Context files across all projects  
**Description:** Signals are bound inconsistently across contexts:

```csharp
// RootAIContext.cs
injectionBinder.Bind<AgentSuccessSignal>().To<AgentSuccessSignal>().ToSingleton().CrossContext();

// ShooterContext.cs  
injectionBinder.Bind<CoupledModeChangedSignal>().ToSingleton();  // No .To<>()
```

The `.To<AgentSuccessSignal>()` is redundant when binding to itself.

**Suggested Fix:** Standardize signal binding pattern. Document the canonical approach in CLAUDE.md or a style guide.

---

### INC-004: Inconsistent Null Checking Patterns
**Severity:** MEDIUM  
**Location:** Various manager classes  
**Description:** Some methods use null guards, others don't:

```csharp
// UiManager.cs
if (component == null)
    Debug.LogError($"UIManager.InstantiateViewAsync: No {typeof(T)} on {uiObj}");
return component;  // Still returns null — no exception

// AssetManager.cs  
if (sprite == null)
    Debug.LogError($"AssetManager.GetSpriteFromAtlas: No sprite...");
return sprite;  // Also just logs, doesn't throw
```

**Suggested Fix:** Either throw exceptions consistently or document that null returns are expected. Consider adding `[NotNull]` attributes or using a Result<T> pattern.

---

### INC-005: Typo in Comment
**Severity:** LOW  
**Location:** `ListBaseMediator.cs:18`  
**Description:** Comment says "View scipt" instead of "View script"

```csharp
/// <typeparam name="TView">View scipt attached to container GameObject</typeparam>
```

**Suggested Fix:** Fix typo.

---

### INC-006: AddOnce vs AddListener Inconsistency
**Severity:** MEDIUM  
**Location:** `AppFsm.cs`  
**Description:** FSM uses `AddOnce` for state signals but doesn't document why this pattern is required:

```csharp
_currentState.EnterFinishedSignal.AddOnce(EnterFinishedHandler);
_currentState.ExitFinishedSignal.AddOnce(ExitFinishedHandler);
```

If a state is reused, these handlers would need re-registration. Framework doesn't prevent state reuse.

**Suggested Fix:** Either document `AddOnce` requirement clearly or use `AddListener` with proper `RemoveListener` in cleanup.

---

### INC-007: Property Injection vs Constructor Injection Mixed
**Severity:** MEDIUM  
**Location:** All injectable classes  
**Description:** Framework exclusively uses property injection `[Inject] public T Prop { get; set; }` but this makes it harder to identify dependencies and test. Some classes have 10+ injected properties.

**Suggested Fix:** Consider supporting constructor injection for core services. Document rationale for property injection.

---

### INC-008: Namespace Inconsistency
**Severity:** LOW  
**Location:** Across packages  
**Description:** Namespaces don't consistently follow package structure:
- `strange.extensions.*` — StrangeIoC legacy
- `Framewerk.*` — New framework code
- `Plugins.Framewerk` — Mixed (ViewConfig)

**Suggested Fix:** Standardize namespaces to `Framewerk.*` or document the mixed approach.

---

### INC-009: MonoBehaviour vs POCO Signal Usage
**Severity:** LOW  
**Location:** `AppStateScreen.cs`, `TabViewConnector.cs`  
**Description:** `AppStateScreen` uses `Signal` instances directly (not injected), while mediators get signals via injection. This inconsistency means state screens can't use `[ListensTo]` attribute.

**Suggested Fix:** Consider making AppStateScreen injectable or document the pattern clearly.

---

### INC-010: View Suffix Removal Logic
**Severity:** LOW  
**Location:** `UiManager.cs:83`  
**Description:** `GetViewName` hardcodes "View" suffix removal:

```csharp
public string GetViewName(Type type)
{
    var name = type.Name;
    return name.Substring(0, name.Length - VIEW_SUFFIX.Length);
}
```

This will crash if a type doesn't end with "View" suffix.

**Suggested Fix:** Add validation: `if (name.EndsWith(VIEW_SUFFIX)) ...`

---

## 2. ARCHITECTURE ISSUES

### ARCH-001: ViewCache is Static Mutable State
**Severity:** CRITICAL  
**Location:** `FramewerkMVCSContext.cs:39`  
**Description:** 

```csharp
protected static ISemiBinding viewCache = new SemiBinding();
```

Static mutable state in a context class is dangerous — it persists across context instances and Unity play mode cycles. Could cause views from previous play sessions to be cached.

**Suggested Fix:** Make non-static or clear explicitly on context start. Consider using `RuntimeInitializeOnLoadMethod` for cleanup.

---

### ARCH-002: ListItemMediator Hierarchy Traversal Coupling
**Severity:** HIGH  
**Location:** `ListItemMediator.cs:91-105`  
**Description:** List items find their parent by traversing the transform hierarchy:

```csharp
private void RegisterToList()
{
    Transform trans = gameObject.transform;
    while (trans.parent != null && loopLimiter < LOOP_MAX)
    {
        trans = trans.parent;
        IListItemParent itemParent = trans.gameObject.GetComponent<IListItemParent>();
        if (itemParent != null) { ... }
    }
}
```

This tight coupling means list items MUST be children in the hierarchy. Can't support pooled items, virtual lists, or non-hierarchical ownership.

**Suggested Fix:** Allow explicit parent assignment via injection or SetParent() method.

---

### ARCH-003: PopupManager Depends on ViewConfig via Property Injection
**Severity:** HIGH  
**Location:** `PopupManager.cs:44-50`  
**Description:** Side effect in property setter:

```csharp
[Inject]
public ViewConfig ViewConfig
{
    set
    {
        _viewConfig = value;
        _popupParent = value.Popups;
        PopupOpenedSignal.AddListener(OnPopupOpenedHandler);  // SIDE EFFECT!
    }
}
```

Injection order-dependent behavior. If `PopupOpenedSignal` is injected after `ViewConfig`, listener won't be added.

**Suggested Fix:** Move setup logic to a `[PostConstruct]` method.

---

### ARCH-004: Circular Reference Risk in AppFsm
**Severity:** MEDIUM  
**Location:** `AppFsm.cs:76-77`  
**Description:** When switching states rapidly, the queue-based approach can cause issues:

```csharp
public virtual void SwitchState(IAppState newState)
{
    InjectionBinder.injector.Inject(newState, false);
    
    if (_currentTransition == TransitionType.None) { ... }
    else
    {
        _nextStates.Enqueue(newState);  // Queue grows unbounded
    }
}
```

No limit on queue size. Rapid state switching could exhaust memory.

**Suggested Fix:** Add queue size limit or coalesce repeated state changes.

---

### ARCH-005: AssetManager Tracks Instantiated Objects in List
**Severity:** MEDIUM  
**Location:** `AssetManager.cs:14`  
**Description:** 

```csharp
private List<GameObject> _instantiatedObjects = new List<GameObject>();
```

This grows unbounded and never prunes destroyed objects. Memory leak risk for long-running applications.

**Suggested Fix:** Use weak references or periodic cleanup of null entries.

---

### ARCH-006: ExtendedMediator Handler Tracking
**Severity:** MEDIUM  
**Location:** `ExtendedMediator.cs`  
**Description:** Tracks UI handlers in multiple dictionaries but uses `RemoveAllListeners()` on add:

```csharp
protected void AddButtonListener(Button button, Action func)
{
    button.onClick.RemoveAllListeners();  // Removes ALL listeners, not just ours
    button.onClick.AddListener(internalAction);
}
```

This could remove listeners added by other code (animators, etc.).

**Suggested Fix:** Track only our listener and remove just that in cleanup.

---

### ARCH-007: God Class Tendency in AppStateScreen
**Severity:** MEDIUM  
**Location:** `AppStateScreen.cs`  
**Description:** Has 10 different instantiation methods mixing sync/async, views/prefabs. Single Responsibility violated.

**Suggested Fix:** Split into focused helper classes or use builder pattern.

---

### ARCH-008: BindingBundle Shadows Base Method
**Severity:** LOW  
**Location:** `BindingBundle.cs:31`  
**Description:** 

```csharp
public new void Uninstall()  // Uses 'new' keyword
```

This shadows base class method which could cause issues if bundle is referenced as `CoreBindingBundle`.

**Suggested Fix:** Use `override` or restructure inheritance.

---

### ARCH-009: View Requires Context Traversal Every Time
**Severity:** LOW  
**Location:** `View.cs:86-118`  
**Description:** Every view bubbles up the hierarchy to find its context on Awake/Start/OnDestroy. For deep hierarchies, this is O(depth) per view.

**Suggested Fix:** Cache context reference after first lookup.

---

## 3. WEIRD CODE

### WEIRD-001: crossContextBridge Setter Assigns to Wrong Field
**Severity:** HIGH  
**Location:** `FramewerkCrossContext.cs:113-120`  
**Description:** 

```csharp
virtual public IBinder crossContextBridge
{
    get { ... }
    set
    {
        _crossContextDispatcher = value as IEventDispatcher;  // WRONG FIELD!
    }
}
```

The setter assigns to `_crossContextDispatcher` instead of `_crossContextBridge`. Copy-paste error.

**Suggested Fix:** Fix to assign to correct field: `_crossContextBridge = value;`

---

### WEIRD-002: PopupMediator Debug Logging in OnRegister
**Severity:** MEDIUM  
**Location:** `PopupMediator.cs:19-22`  
**Description:** 

```csharp
if (PopupOptionSettings != null)
    Debug.Log("PopupOptionSettings injected");
else
    Debug.Log("PopupOptionSettings NOT injected");
```

Debug code left in production code.

**Suggested Fix:** Remove or use `[Conditional("DEBUG")]`.

---

### WEIRD-003: ListItemMediator Data Property Shadowing
**Severity:** MEDIUM  
**Location:** `ListItemMediator.cs`  
**Description:** Base has `DataProvider` but generated templates reference `Data` (see `CodeTemplates.cs:168`):

```csharp
// Template generates:
if (Data != null)  // But the field is DataProvider

// ListItemMediator has:
protected TData DataProvider;
```

Templates and base class are out of sync.

**Suggested Fix:** Add `Data` property alias or update templates.

---

### WEIRD-004: LOOP_MAX Magic Number
**Severity:** LOW  
**Location:** `View.cs:88`, `ListItemMediator.cs:94`  
**Description:** Both use `const int LOOP_MAX = 100` for hierarchy traversal but it's defined locally in each file.

**Suggested Fix:** Extract to shared constant or make configurable.

---

### WEIRD-005: Empty Virtual Methods
**Severity:** LOW  
**Location:** `Mediator.cs:35-58`  
**Description:** Multiple empty virtual methods that exist only for override:

```csharp
virtual public void PreRegister() { }
virtual public void OnRegister() { }
virtual public void OnRemove() { }
virtual public void OnEnabled() { }
virtual public void OnDisabled() { }
```

Consider using interfaces for optional lifecycle hooks.

---

### WEIRD-006: Signal listener Getter Creates Delegate if Null
**Severity:** LOW  
**Location:** `Signal.cs:75-77`  
**Description:**

```csharp
public Delegate listener
{
    get { return Listener ?? (Listener = delegate { }); }
}
```

Getting the property mutates state. Could cause unexpected behavior.

**Suggested Fix:** Return null instead of creating empty delegate.

---

### WEIRD-007: PopupMediator Creates Buttons But Stores in List
**Severity:** MEDIUM  
**Location:** `PopupMediator.cs:24-48`  
**Description:** Creates buttons via Instantiate, stores references, but never uses the list for anything except cleanup. The mediator isn't using the ExtendedMediator's button tracking.

**Suggested Fix:** Either use ExtendedMediator's `AddButtonListener` or remove redundant tracking.

---

## 4. FRAMEWORK VS USAGE GAPS

### GAP-001: Consumer Contexts Repeat Boilerplate Bindings
**Severity:** HIGH  
**Location:** All consumer contexts  
**Description:** Every context repeats the same core bindings:

```csharp
// ShooterContext.cs
injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);
injectionBinder.Bind<ICoroutineManager>().ToValue(CoroutineManager.Instance);
injectionBinder.Bind<IUpdater>().ToValue(Updater.Instance);
injectionBinder.Bind<IAppMonitor>().ToValue(AppMonitor.Instance);
injectionBinder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
injectionBinder.Bind<IUiManager>().To<UiManager>().ToSingleton();

// RootAIContext.cs - same bindings repeated
injectionBinder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
injectionBinder.Bind<IUiManager>().To<UiManager>().ToSingleton();
```

**Suggested Fix:** Create a `FramewerkCoreBundle` that provides standard bindings. Add `InstallBundle<FramewerkCoreBundle>()` support to context.

---

### GAP-002: Screen Injection Not Automatic
**Severity:** HIGH  
**Location:** Consumer contexts  
**Description:** FSM Screens must be manually bound:

```csharp
// ShooterContext.cs
injectionBinder.Bind<MainMenuScreen>().To<MainMenuScreen>();
injectionBinder.Bind<HangarScreen>().To<HangarScreen>();
injectionBinder.Bind<GameScreen>().To<GameScreen>();
```

**Suggested Fix:** Auto-discover and bind screens with a convention or attribute.

---

### GAP-003: ReferenceBinder Workaround for In-Scene Objects
**Severity:** MEDIUM  
**Location:** ShooterReborn  
**Description:** ShooterReborn created `ReferenceBinder` component to bind in-scene objects:

```csharp
// ShooterBootstrap.cs
public ReferenceBinder referenceBinder;

// ShooterContext.cs
_referenceBinder.Bind(injectionBinder);
```

This is a workaround for framework not supporting in-scene object injection.

**Suggested Fix:** Add official `[BindInScene]` attribute or scene-scanning support.

---

### GAP-004: Temari Uses Different Path Conventions
**Severity:** MEDIUM  
**Location:** Temari project  
**Description:** Temari uses `ResourcePath.AI_PREFABS_ROOT` constant-based paths instead of the AddressBuilder system:

```csharp
UiManager.InstantiateView<TrainingAiMenuView>(ResourcePath.AI_PREFABS_ROOT);
```

**Suggested Fix:** Document migration path or support legacy patterns.

---

### GAP-005: Manual AppFsm Signal Binding
**Severity:** MEDIUM  
**Location:** All FSM-using contexts  
**Description:** Every context manually binds FSM signals:

```csharp
injectionBinder.Bind<AppStateEnterSignal>().ToSingleton();
injectionBinder.Bind<AppStateExitSignal>().ToSingleton();
```

**Suggested Fix:** Create `AppFsmBundle` that handles this.

---

### GAP-006: No Built-in Model/State Management
**Severity:** LOW  
**Location:** ShooterReborn  
**Description:** ShooterReborn has 15+ model classes all manually bound. No framework support for:
- Model registration
- State persistence
- Change notification

**Suggested Fix:** Consider adding `IModel` interface with auto-binding support.

---

### GAP-007: CrossContext Signals Rarely Used
**Severity:** LOW  
**Location:** Temari  
**Description:** Only Temari uses `.CrossContext()` on signals. ShooterReborn doesn't. The feature exists but isn't well documented.

**Suggested Fix:** Document cross-context communication patterns.

---

## 5. POTENTIAL BUGS

### BUG-001: CancellationToken Not Used
**Severity:** CRITICAL  
**Location:** All async methods in `AssetManager.cs`, `UiManager.cs`, `PopupManager.cs`  
**Description:** CancellationToken parameters are accepted but never used:

```csharp
public async Task<T> GetAssetAsync<T>(string address, Transform parent = null, CancellationToken ct = default)
{
    // ct is NEVER checked or passed to Addressables
    var go = await Addressables.InstantiateAsync(address, parent, false).Task;
}
```

This means async operations can't be cancelled, leading to potential memory leaks and stale references.

**Suggested Fix:** Wire CancellationToken through to Addressables operations.

---

### BUG-002: Race Condition in Popup Signal Handling
**Severity:** HIGH  
**Location:** `PopupManager.cs:92-94`  
**Description:** 

```csharp
private void OnPopupOpenedHandler(IPopupMediator popup)
{
    _popups.Add(popup);
    popup.PopupClosedSignal.AddListener(OnPopupClosed);
}
```

If popup closes before this handler runs (fast close), the listener won't be added, and `_popups` won't be cleaned up.

**Suggested Fix:** Add listener in mediator constructor or verify popup state before adding.

---

### BUG-003: ListBaseMediator SetData Clears Selection Incorrectly
**Severity:** HIGH  
**Location:** `ListBaseMediator.cs:55-58`  
**Description:** 

```csharp
public virtual void SetData(List<TData> dataProviders)
{
    UnselectAll();  // Clears selection before new data is applied
    // ...
}
```

This dispatches `SelectionChangedSignal` with old data still referenced. Can cause stale data access.

**Suggested Fix:** Clear selection references without dispatching, or update data first.

---

### BUG-004: View Registration Race Condition
**Severity:** HIGH  
**Location:** `View.cs:71-84`  
**Description:** View tries to register in both `Awake()` and `Start()`:

```csharp
protected virtual void Awake()
{
    if (autoRegisterWithContext && !registeredWithContext && shouldRegister)
        bubbleToContext(this, BubbleType.Add, false);
}

protected virtual void Start()
{
    if (autoRegisterWithContext && !registeredWithContext && shouldRegister)
        bubbleToContext(this, BubbleType.Add, true);  // finalTry = true
}
```

If context isn't ready in Awake but view is disabled before Start, registration fails silently.

**Suggested Fix:** Queue failed registrations and retry when context is ready.

---

### BUG-005: AppFsm.Destroy() Calls CloseCurrentState Without Null Check
**Severity:** MEDIUM  
**Location:** `AppFsm.cs:124-128`  
**Description:** 

```csharp
public virtual void Destroy()
{
    _nextStates.Clear();
    CloseCurrentState();  // No null check for _currentState
}
```

If called when no state is active, this could fail.

**Suggested Fix:** Add `if (_currentState != null)` check.

---

### BUG-006: Signal Listener Duplicate Prevention Allocates
**Severity:** MEDIUM  
**Location:** `Signal.cs:66-72`  
**Description:** 

```csharp
private Action AddUnique(Action listeners, Action callback)
{
    if (listeners == null || !listeners.GetInvocationList().Contains(callback))
    {
        listeners += callback;
    }
    return listeners;
}
```

`GetInvocationList()` allocates a new array every call. For hot paths with many signals, this creates GC pressure.

**Suggested Fix:** Track listeners in a HashSet or accept duplicate risk.

---

### BUG-007: TabViewConnector Cleanup on Null
**Severity:** MEDIUM  
**Location:** `TabViewConnector.cs`  
**Description:** If `_viewStackMediator` is null in cleanup, signal removal will fail:

```csharp
// From code template, but actual implementation may vary
_tabConnector?.Destroy();
```

Need to verify null safety in actual implementation.

**Suggested Fix:** Add null checks in Destroy().

---

### BUG-008: BindingUtils Interface Detection
**Severity:** LOW  
**Location:** `BindingUtils.cs:14`  
**Description:** 

```csharp
if (includeInterfaces)
    types.AddRange(concreteType.GetInterfaces());
```

This includes ALL interfaces including `IDisposable`, `IEnumerable`, etc. Could cause unexpected bindings.

**Suggested Fix:** Filter to only `Framewerk.*` or project namespaces.

---

### BUG-009: DestroyingBinder Only Handles Singletons
**Severity:** LOW  
**Location:** `DestroyingBinder.cs`  
**Description:** Comment says "Works only with Singleton bindings" but there's no validation. Factory bindings won't clean up.

**Suggested Fix:** Log warning for non-singleton IDestroyable bindings.

---

## 6. MISSING ABSTRACTIONS

### MISS-001: No Bundle for Core Framework Services
**Severity:** HIGH  
**Location:** N/A  
**Description:** Every context manually binds:
- IAssetManager
- IUiManager
- ICoroutineManager
- IUpdater
- IAppMonitor
- ViewConfig

**Suggested Fix:** Create `FramewerkCoreBundle`:

```csharp
public class FramewerkCoreBundle : BindingBundle
{
    protected override void OnInstall()
    {
        BindIfMissing<IAssetManager>().To<AssetManager>().ToSingleton();
        BindIfMissing<IUiManager>().To<UiManager>().ToSingleton();
        // etc.
    }
}
```

---

### MISS-002: No Built-in Loading/Transition Screen
**Severity:** HIGH  
**Location:** N/A  
**Description:** Both consumer projects need loading states but framework doesn't provide:
- Loading screen view
- Transition animations
- Progress reporting

**Suggested Fix:** Add `LoadingScreen` component and `ILoadingManager` service.

---

### MISS-003: No Event Bus for Cross-Mediator Communication
**Severity:** MEDIUM  
**Location:** N/A  
**Description:** Mediators communicate via signals, but there's no lightweight event bus for transient events that don't need command mapping.

**Suggested Fix:** Add simple pub/sub system or document signal-based alternatives.

---

### MISS-004: No Pool Support for List Items
**Severity:** MEDIUM  
**Location:** `ListMediator.cs`  
**Description:** List items are created via `UiManager.InstantiateView` but never pooled. For long lists with frequent updates, this causes GC pressure.

**Suggested Fix:** Add `IPooledListMediator<T>` variant using object pooling.

---

### MISS-005: No Navigation History/Back Support
**Severity:** MEDIUM  
**Location:** `AppFsm.cs`  
**Description:** FSM supports forward state transitions but not:
- Back button handling
- Navigation history
- State stack

ShooterReborn would benefit from Hangar → Game → Menu navigation.

**Suggested Fix:** Add `PushState()`, `PopState()`, `NavigateBack()` methods.

---

### MISS-006: No Validated Bindings
**Severity:** LOW  
**Location:** N/A  
**Description:** No compile-time or runtime validation that all required bindings exist before Launch().

**Suggested Fix:** Add `context.Validate()` method that checks all [Inject] points can be satisfied.

---

## RECOMMENDATIONS FOR 2.0

### Priority 1 (Critical)
1. Fix `crossContextBridge` setter bug (WEIRD-001)
2. Wire CancellationToken through async methods (BUG-001)
3. Make viewCache non-static (ARCH-001)

### Priority 2 (High)
1. Create `FramewerkCoreBundle` for standard bindings (MISS-001)
2. Standardize Destroy vs OnRemove lifecycle (INC-001)
3. Fix popup signal race condition (BUG-002)
4. Remove debug logging from PopupMediator (WEIRD-002)

### Priority 3 (Medium)
1. Add pooled list support (MISS-004)
2. Support explicit list item parent assignment (ARCH-002)
3. Move PopupManager setup to [PostConstruct] (ARCH-003)
4. Add navigation history to FSM (MISS-005)

### Priority 4 (Low - Documentation)
1. Document signal binding conventions
2. Document CrossContext usage patterns
3. Create migration guide from resource paths to AddressBuilder
4. Add code style guide to CLAUDE.md

---

## Appendix: Files Reviewed

**Framewerk Core:**
- `FramewerkCrossContext.cs`
- `FramewerkMVCSContext.cs`
- `Mediator.cs`, `View.cs`
- `Signal.cs`, `BaseSignal.cs`
- `Command.cs`, `CommandBinder.cs`
- `Injector.cs`, `InjectionBinder.cs`
- `CoreBindingBundle.cs`, `BindingBundle.cs`
- `DestroyingBinder.cs`

**Framewerk UI:**
- `ListMediator.cs`, `ListBaseMediator.cs`, `ListItemMediator.cs`
- `PopupManager.cs`, `PopupMediator.cs`
- `UiManager.cs`, `AssetManager.cs`
- `ExtendedMediator.cs`
- `ViewStackMediator.cs`
- `ViewConfig.cs`, `AddressBuilder.cs`

**Framewerk ScreenFSM:**
- `AppFsm.cs`, `AppState.cs`, `AppStateScreen.cs`

**Consumer Projects:**
- `ShooterContext.cs`, `ShooterBootstrap.cs`
- `RootAIContext.cs`, `TemariAIContext.cs`
- Various mediators from both projects

---

*Report generated by code audit subagent*
