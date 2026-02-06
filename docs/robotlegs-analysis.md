# Robotlegs 2.0 vs Framewerk — Deep Analysis

## Executive Summary

Robotlegs 2.0 is architecturally more sophisticated than Framewerk in several key areas: **lifecycle management**, **guards/hooks**, **type matching**, and **modularity**. However, Framewerk's recent BindingBundle system is arguably *better* than Robotlegs' bundle/config system for Unity due to tracked cleanup and BindIfMissing. The biggest gaps are guards, hooks, and lifecycle — these would significantly improve Framewerk's flexibility.

---

## CONCEPTS FRAMEWERK IS MISSING

### 1. Guards (Conditional Execution)
**What it is:** A guard is an injectable object with an `approve():bool` method. Guards are checked before a command executes or a mediator is created. ALL guards must approve for the action to proceed.

**How Robotlegs implements it:**
```as3
// Interface (optional — duck typing works too)
public interface IGuard { function approve():Boolean; }

// Example guard
public class IsLoggedInGuard implements IGuard {
    [Inject] public var userModel:UserModel;
    public function approve():Boolean { return userModel.isLoggedIn; }
}

// Usage on commands
commandMap.map(UserEvent.DELETE_ACCOUNT).toCommand(DeleteAccountCommand)
    .withGuards(IsLoggedInGuard, IsAdminGuard);

// Usage on mediators  
mediatorMap.map(AdminPanel).toMediator(AdminPanelMediator)
    .withGuards(IsAdminGuard);

// Core function — works with classes, instances, or functions
public function guardsApprove(guards:Array, injector:IInjector):Boolean {
    for each (var guard in guards) {
        if (guard is Function) { if (!guard()) return false; continue; }
        if (guard is Class) guard = injector.instantiateUnmapped(guard);
        if (!guard.approve()) return false;
    }
    return true;
}
```

**Would it be useful?** **YES — EXTREMELY.** This is the single biggest missing concept. Currently in Framewerk, if a signal fires, the command ALWAYS executes. Guards would allow:
- Commands that only fire when conditions are met (e.g., player is alive, network is connected)
- Mediators that only attach when certain states are active
- Eliminating boilerplate `if (!condition) return;` at the top of every command

**Priority: HIGH** 🔴

---

### 2. Hooks (Pre-Execution Actions)
**What it is:** A hook is an injectable object with a `hook():void` method. Hooks run AFTER guards approve but BEFORE command execution. They can modify state that the command will use.

**How Robotlegs implements it:**
```as3
public interface IHook { function hook():void; }

// Example: Log every command execution
public class LogCommandHook implements IHook {
    [Inject] public var logger:ILogger;
    [Inject] public var command:SomeCommand; // The actual command instance is injectable!
    public function hook():void { logger.info("Executing: " + command); }
}

// Usage
commandMap.map(UserEvent.SIGN_IN).toCommand(SignInCommand)
    .withGuards(HasCredentialsGuard)
    .withHooks(LogCommandHook, AnalyticsHook);

// Core function
public function applyHooks(hooks:Array, injector:IInjector):void {
    for each (var hook in hooks) {
        if (hook is Function) { hook(); continue; }
        if (hook is Class) hook = injector.instantiateUnmapped(hook);
        hook.hook();
    }
}
```

**Key detail:** The command instance is temporarily mapped into the injector while hooks run, so hooks can inject and modify the command before execution.

**Would it be useful?** **YES.** Hooks enable:
- Cross-cutting concerns (logging, analytics, validation) without polluting commands
- Mediator setup hooks (e.g., always apply a theme when creating certain mediators)
- AOP-like patterns without a full AOP framework

**Priority: HIGH** 🔴

---

### 3. Context Lifecycle State Machine
**What it is:** A full state machine for context lifecycle with states: `UNINITIALIZED → INITIALIZING → ACTIVE ⟷ SUSPENDED ⟷ ACTIVE → DESTROYING → DESTROYED`. Each transition has three hook points: `before`, `when`, `after`.

**How Robotlegs implements it:**
```as3
// States
UNINITIALIZED → INITIALIZING → ACTIVE → SUSPENDING → SUSPENDED → RESUMING → ACTIVE → DESTROYING → DESTROYED

// Context lifecycle API
context.beforeInitializing(handler);   // Can be async!
context.whenInitializing(handler);     // Must be sync
context.afterInitializing(handler);    // Must be sync

context.beforeSuspending(handler);
context.whenSuspending(handler);
context.afterSuspending(handler);

context.beforeResuming(handler);
context.whenResuming(handler);
context.afterResuming(handler);

context.beforeDestroying(handler);
context.whenDestroying(handler);
context.afterDestroying(handler);

// Transitions are defined as state machines
_initialize = new LifecycleTransition("PRE_INITIALIZE", this)
    .fromStates(UNINITIALIZED)
    .toStates(INITIALIZING, ACTIVE)
    .withEvents(PRE_INITIALIZE, INITIALIZE, POST_INITIALIZE);

_suspend = new LifecycleTransition("PRE_SUSPEND", this)
    .fromStates(ACTIVE)
    .toStates(SUSPENDING, SUSPENDED)
    .withEvents(PRE_SUSPEND, SUSPEND, POST_SUSPEND)
    .inReverse();  // Teardown runs in reverse order!
```

**Framewerk currently:** Only has `OnRemove()`. No formal states, no suspend/resume, no before/after hooks on initialization or destruction. Context goes from "exists" to "doesn't exist".

**Would it be useful?** **YES.** For Unity specifically:
- `Suspend/Resume` maps perfectly to scene loading/unloading without destroying
- `beforeDestroying` could ensure cleanup ordering (e.g., save state before services disconnect)
- `afterInitializing` separates "bindings are ready" from "you can start doing things"
- The `.inReverse()` pattern ensures teardown happens in reverse order of setup (critical for dependencies)

**Priority: MEDIUM-HIGH** 🟡

---

### 4. DirectCommandMap (Manual Command Execution)
**What it is:** Execute commands directly from code without needing a signal/event trigger. Supports the full guards/hooks/injection pipeline.

**How Robotlegs implements it:**
```as3
// Injected into configs or other commands
[Inject] public var directCommandMap:IDirectCommandMap;

// Execute a command manually
directCommandMap.map(StartupCommand)
    .withGuards(IsFirstLaunchGuard)
    .withHooks(LogHook)
    .execute();

// With payload
var payload:CommandPayload = new CommandPayload([userId], [String]);
directCommandMap.map(LoadUserCommand).execute(payload);

// Detain/release for async commands
directCommandMap.detain(asyncCommand);  // Prevent GC
// ... later ...
directCommandMap.release(asyncCommand);
```

**Framewerk currently:** To execute a command, you must dispatch its signal. No way to programmatically fire commands with the full injection/guard/hook pipeline.

**Would it be useful?** **YES.** Useful for:
- Startup sequences (execute commands in order without signal dispatch)
- Conditional command chains in other commands
- Testing (execute commands directly with specific payloads)

**Priority: MEDIUM** 🟡

---

### 5. TypeMatcher / PackageMatcher (Rich View Matching)
**What it is:** Match views to mediators based on type rules: `allOf` (must implement all), `anyOf` (must implement at least one), `noneOf` (must not implement).

**How Robotlegs implements it:**
```as3
// Match any view that implements IListView AND ISelectable
mediatorMap.mapMatcher(new TypeMatcher().allOf(IListView, ISelectable))
    .toMediator(SelectableListMediator);

// Match views that are IPanel but NOT IPopup
mediatorMap.mapMatcher(new TypeMatcher().allOf(IPanel).noneOf(IPopup))
    .toMediator(PanelMediator);

// Match by namespace/package
mediatorMap.mapMatcher(new PackageMatcher().require("com.myapp.views.admin"))
    .toMediator(AdminViewMediator);
```

**TypeFilter matching logic:**
```as3
// ALL allOf types must match
// NONE of noneOf types must match
// At least ONE anyOf type must match (if any specified)
```

**Framewerk currently:** Direct 1:1 type mapping only: `mediationBinder.Bind<SomeView>().To<SomeMediator>()`. No interface-based matching, no composition rules.

**Would it be useful?** **SOMEWHAT.** In Unity, views are MonoBehaviours and interface-based matching is less common. But it could be useful for:
- Mapping mediators to interface types (e.g., all `IClickableView` get a click tracking mediator)
- Avoiding duplicate mediator mappings for similar views

**Priority: MEDIUM** 🟡

---

### 6. ViewProcessorMap (Lightweight View Processing)
**What it is:** Process views when they appear in the display list WITHOUT creating a full mediator. Can inject dependencies into views, set properties, or run custom processors.

**How Robotlegs implements it:**
```as3
// Just inject dependencies into the view (no mediator needed)
viewProcessorMap.map(SimpleView).toInjection();

// Set specific property values
viewProcessorMap.map(ThemeableView).toProcess(
    new PropertyValueInjector({ theme: darkTheme, fontSize: 14 })
);

// Fast property injection (lookup from injector by type)
viewProcessorMap.map(ServiceAwareView).toProcess(
    new FastPropertyInjector({ userService: IUserService, config: AppConfig })
);

// Custom processor class
viewProcessorMap.map(DraggableView).toProcess(DragProcessor);

// With guards and hooks
viewProcessorMap.map(AdminView).toProcess(AdminSetupProcessor)
    .withGuards(IsAdminGuard)
    .withHooks(LogHook);
```

**Framewerk currently:** Every view that needs anything from the context must have a full mediator class.

**Would it be useful?** **YES for certain cases:**
- Views that just need a few injections (no logic) → ViewInjectionProcessor eliminates boilerplate mediators
- Common view setup patterns (themes, localization) applied automatically
- Performance: lighter than full mediation for simple cases

**Priority: LOW-MEDIUM** 🟢

---

### 7. Explicit Module Communication Channels
**What it is:** Instead of all cross-context events being global, modules can define explicit channels for communication with fine-grained control over which events are relayed in/out.

**How Robotlegs implements it:**
```as3
// In module config
[Inject] public var moduleConnector:IModuleConnector;

public function configure():void {
    moduleConnector.onDefaultChannel()
        .relayEvent(ChatEvent.MESSAGE_SENT)     // Send OUT to other modules
        .receiveEvent(ChatEvent.NEW_MESSAGE);    // Receive IN from other modules
    
    // Named channels for targeted communication
    moduleConnector.onChannel("analytics")
        .relayEvent(AnalyticsEvent.TRACK);
}

// Channels can be suspended/resumed
moduleConnector.onChannel("chat").suspend();
moduleConnector.onChannel("chat").resume();
```

**Implementation:** Uses `EventRelay` objects that forward specific event types between a local dispatcher and a shared channel dispatcher (stored in the root injector).

**Framewerk currently:** CrossContextBridge relays ALL events between contexts. No filtering, no channels, no suspend/resume.

**Would it be useful?** **YES.** For a multi-context Unity game:
- Only relay relevant events between modules (not everything)
- Named channels prevent event collision
- Suspend/resume module communication during transitions

**Priority: MEDIUM** 🟡

---

### 8. Extension System (IExtension interface)
**What it is:** A formal plugin system where extensions can hook into the context lifecycle to install capabilities. Extensions are singleton per context (installed once).

**How Robotlegs implements it:**
```as3
public interface IExtension {
    function extend(context:IContext):void;
}

// Example extension
public class MyExtension implements IExtension {
    public function extend(context:IContext):void {
        context.injector.map(IMyService).toSingleton(MyService);
        context.beforeInitializing(setup);
        context.whenDestroying(teardown);
    }
}

// Install
context.install(MyExtension, AnotherExtension);

// Bundle = collection of extensions
public class MyBundle implements IBundle {
    public function extend(context:IContext):void {
        context.install(ExtA, ExtB, ExtC);
        context.configure(ConfigA);
    }
}
```

**ExtensionInstaller guarantees:** Each extension class is installed only once per context (deduplication by class).

**Framewerk currently:** BindingBundle provides tracked bindings with Install/Uninstall, but there's no concept of *framework extensions* that hook into context lifecycle.

**Would it be useful?** **SOMEWHAT.** The BindingBundle already covers the main use case (modular bindings). But a formal extension system would enable:
- Extensions that modify context behavior (like Vigilance or EnhancedLogging)
- Third-party framework plugins
- Separating framework plumbing from application config

**Priority: LOW-MEDIUM** 🟢

---

### 9. Vigilance Extension (Development Mode)
**What it is:** Converts all framework warnings into hard errors during development. Also catches injector mapping overrides and missing metadata.

**How Robotlegs implements it:**
```as3
public class VigilanceExtension implements IExtension, ILogTarget {
    public function extend(context:IContext):void {
        context.addLogTarget(this);
        context.injector.addEventListener(MappingEvent.MAPPING_OVERRIDE, handler);
    }
    
    public function log(source, level, timestamp, message, params):void {
        if (level <= LogLevel.WARN) throw new VigilantError(message);
    }
}
```

**Framewerk currently:** No equivalent. Misconfigurations may silently fail.

**Would it be useful?** **YES.** Catching binding overrides and other misconfigurations early would prevent subtle bugs, especially with the new BindingBundle system where multiple bundles might conflict.

**Priority: MEDIUM** 🟡

---

### 10. Mediator Lifecycle Phases
**What it is:** 6-phase mediator lifecycle: `preInitialize → viewComponent set → initialize → postInitialize` and `preDestroy → destroy → viewComponent nulled → postDestroy`.

**How Robotlegs implements it:**
```as3
// MediatorManager calls these phases via duck typing
private function initializeMediator(mediator, mediatedItem):void {
    if ('preInitialize' in mediator) mediator.preInitialize();
    if ('viewComponent' in mediator) mediator.viewComponent = mediatedItem;
    if ('initialize' in mediator) mediator.initialize();
    if ('postInitialize' in mediator) mediator.postInitialize();
}

private function destroyMediator(mediator):void {
    if ('preDestroy' in mediator) mediator.preDestroy();
    if ('destroy' in mediator) mediator.destroy();
    if ('viewComponent' in mediator) mediator.viewComponent = null;
    if ('postDestroy' in mediator) mediator.postDestroy();
}
```

The `postDestroy` in the base Mediator class auto-cleans up the EventMap:
```as3
public function postDestroy():void { eventMap.unmapListeners(); }
```

**Framewerk currently:** `OnRegister()` and `OnRemove()` (2-phase). ExtendedMediator adds auto-cleanup of UI listeners in OnRemove.

**Would it be useful?** **SOMEWHAT.** The 2-phase lifecycle is usually sufficient. But `preInitialize` (before view assignment) and `postDestroy` (after view nulled) could help with:
- Setting up non-view-dependent state before the view reference arrives
- Ensuring cleanup doesn't reference the view after it's gone

**Priority: LOW** 🟢

---

### 11. LocalEventMap with Suspend/Resume
**What it is:** A per-mediator event listener registry that can temporarily suspend all listeners (removing them from dispatchers) and resume them later.

**How Robotlegs implements it:**
```as3
[Inject] public var eventMap:IEventMap;

// In mediator
eventMap.mapListener(view, "click", onClick);
eventMap.mapListener(dispatcher, "dataLoaded", onDataLoaded);

// Suspend (removes all listeners temporarily)
eventMap.suspend();

// Resume (re-adds all listeners)
eventMap.resume();

// Clean up all
eventMap.unmapListeners();
```

**Framewerk currently:** ExtendedMediator tracks Unity UI listeners for cleanup, but no suspend/resume.

**Would it be useful?** **SOMEWHAT.** Suspend/resume is niche but useful for:
- Pausing mediator reactivity during transitions or overlays
- Temporarily disabling UI handling without destroying the mediator

**Priority: LOW** 🟢

---

### 12. Context Pin/Detain/Release
**What it is:** Prevents objects from being garbage collected by holding strong references in the context.

**How Robotlegs implements it:**
```as3
context.detain(asyncCommand);  // Keep alive
context.release(asyncCommand); // Allow GC
```

**Framewerk currently:** Not needed — C#/Unity GC is deterministic enough, and the DestroyingBinder handles cleanup.

**Priority: N/A** (not applicable to Unity/C#)

---

## THINGS DONE DIFFERENTLY — POTENTIAL IMPROVEMENTS

### Bundle/Config System
| Aspect | Robotlegs | Framewerk |
|--------|-----------|-----------|
| Install | `context.install(Bundle)` | `InstallBundle<T>()` |
| Uninstall | No built-in uninstall! | Full tracked Uninstall() |
| Cooperative binding | Not supported | `BindIfMissing<T>()` |
| Cleanup on context remove | Extensions destroy manually | Auto-uninstall all bundles |
| Binding tracking | Not tracked | Full tracking per bundle |

**Verdict: Framewerk WINS.** The BindingBundle system is more practical and more robust than Robotlegs' bundle system. Robotlegs bundles are one-way (install only); Framewerk bundles are fully reversible.

### Context Hierarchy
| Aspect | Robotlegs | Framewerk |
|--------|-----------|-----------|
| Parent-child | `context.addChild(child)` | `AddContext(context)` |
| Injector chain | Auto `child.injector.parent = parent.injector` | Via CrossContextInjectionBinder |
| Auto-cleanup | Child destroy → auto removeChild | Manual |
| Communication | Explicit channels via ModuleConnector | Global CrossContextBridge |

**Verdict: MIXED.** Robotlegs has cleaner hierarchy management and explicit channels. Framewerk has simpler setup but less control.

### Command Binding DSL
| Aspect | Robotlegs | Framewerk |
|--------|-----------|-----------|
| Basic | `map(event).toCommand(Cmd)` | `Bind<Signal>().To<Cmd>()` |
| Guards | `.withGuards(Guard)` | ❌ Not available |
| Hooks | `.withHooks(Hook)` | ❌ Not available |
| Once | `.once()` | `.Once()` ✅ |
| Multiple commands | `.toCommand(A).toCommand(B)` | `.To<A>().To<B>()` ✅ |
| Custom execute | `.withExecuteMethod("run")` | ❌ Always Execute() |

### Mediation
| Aspect | Robotlegs | Framewerk |
|--------|-----------|-----------|
| Basic | `map(View).toMediator(Med)` | `Bind<View>().To<Med>()` |
| Type matching | `mapMatcher(new TypeMatcher().allOf(I1, I2))` | ❌ Direct type only |
| Guards | `.withGuards(Guard)` | ❌ |
| Hooks | `.withHooks(Hook)` | ❌ |
| Auto-remove | `.autoRemove(false)` | Always auto-remove |
| Lifecycle | 6-phase | 2-phase (OnRegister/OnRemove) |

---

## PRIORITY SUMMARY

| # | Concept | Priority | Effort | Impact |
|---|---------|----------|--------|--------|
| 1 | **Guards** (command + mediation) | 🔴 HIGH | Medium | Eliminates boilerplate conditionals, enables conditional mediation |
| 2 | **Hooks** (command + mediation) | 🔴 HIGH | Medium | Cross-cutting concerns, AOP-like patterns |
| 3 | **Context Lifecycle** (suspend/resume + hooks) | 🟡 MED-HIGH | High | Scene management, proper shutdown ordering |
| 4 | **DirectCommandMap** | 🟡 MEDIUM | Low | Programmatic command execution |
| 5 | **TypeMatcher for mediation** | 🟡 MEDIUM | Medium | Interface-based mediator mapping |
| 6 | **Module Communication Channels** | 🟡 MEDIUM | Medium | Explicit cross-context event routing |
| 7 | **Vigilance/Dev Mode** | 🟡 MEDIUM | Low | Catch misconfigurations early |
| 8 | **ViewProcessorMap** | 🟢 LOW-MED | Medium | Lightweight view processing |
| 9 | **Extension System** | 🟢 LOW-MED | High | Framework plugin architecture |
| 10 | **Mediator Lifecycle Phases** | 🟢 LOW | Low | More granular mediator lifecycle |
| 11 | **LocalEventMap Suspend/Resume** | 🟢 LOW | Low | Temporary listener suppression |

---

## RECOMMENDED IMPLEMENTATION ORDER

### Phase 1: Guards + Hooks (Biggest bang for buck)
```csharp
// Proposed Framewerk API:

// Guard interface
public interface IGuard { bool Approve(); }

// Hook interface  
public interface IHook { void Hook(); }

// Command usage
commandBinder.Bind<AttackSignal>().To<AttackCommand>()
    .WithGuards<IsAliveGuard, HasAmmoGuard>()
    .WithHooks<LogCommandHook>();

// Mediation usage
mediationBinder.Bind<AdminPanel>().To<AdminPanelMediator>()
    .WithGuards<IsAdminGuard>();
```

### Phase 2: DirectCommandMap
```csharp
// Proposed API
[Inject] public IDirectCommandMap DirectCommands { get; set; }

DirectCommands.Map<LoadUserCommand>()
    .WithGuards<IsOnlineGuard>()
    .Execute(userId);
```

### Phase 3: Context Lifecycle
```csharp
// Proposed API additions to FramewerkCrossContext
public enum ContextState { Uninitialized, Active, Suspended, Destroyed }
public ContextState State { get; }

public void Suspend();   // Pause context, optionally suspend mediator listeners
public void Resume();    // Resume context
// OnRemove already exists for destroy
```

### Phase 4: Vigilance (Quick win)
```csharp
// Simple: check for binding overrides and log as errors in debug builds
#if UNITY_EDITOR || DEBUG
BindingBundle.VerboseLogging = true;
// + Add override detection to DestroyingBinder
#endif
```
