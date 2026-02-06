# Robotlegs Sharp vs Framewerk — Deep Analysis Report

## Executive Summary

Robotlegs Sharp is a complete ground-up redesign from the AS3 original. Unlike Framewerk (which extends StrangeIoC), Robotlegs Sharp built its own IoC container, lifecycle system, extension architecture, and everything else from scratch in C#. The architecture is significantly more modular — everything is an installable extension, even core features like event dispatching and mediation.

Key differences from AS3 Robotlegs:
- Uses C# events instead of AS3 events
- Uses `IExtension` + `IConfig` pattern instead of AS3's monolithic Context
- Has a full state machine lifecycle (UNINITIALIZED → ACTIVE → SUSPENDED → DESTROYED)
- TypeMatcher system for advanced view matching (AllOf/AnyOf/NoneOf)
- Guards and Hooks are first-class citizens at the framework level

---

## Concepts Framewerk is Missing

### 1. Guards — Conditional Execution Gates

**What it is:** Guards are injectable classes that implement `IGuard` with a single `Approve()` method returning bool. They gate whether a command executes or a mediator gets created. Multiple guards = AND logic (all must approve).

**How Robotlegs Sharp implements it:**
```csharp
public interface IGuard { bool Approve(); }

// Usage in command mapping:
commandMap.Map(EventType.ATTACK).ToCommand<AttackCommand>()
    .WithGuards<HasAmmoGuard, IsAliveGuard>();

// Usage in mediator mapping:
mediatorMap.Map<EnemyView>().ToMediator<EnemyMediator>()
    .WithGuards<IsVisibleGuard>();
```

Guards are instantiated via the injector (so they get dependencies injected), then `Approve()` is called. Also supports `Func<bool>` lambdas.

The `Guards.Approve()` static helper runs through all guard instances.

**Would it be useful for Framewerk?** **YES — Very useful.** Right now if you want conditional command execution in Framewerk, you put `if` checks inside the command itself. Guards externalize this concern, making it reusable and testable. Example: "only execute this command if the player is alive" — that guard can be reused across many commands.

**Priority: HIGH**

---

### 2. Hooks — Pre-Execution Interceptors

**What it is:** Hooks are injectable classes that implement `IHook` with a single `Hook()` method. They run *after* guards approve but *before* the command/mediator executes. Used for setup, logging, analytics, etc.

**How Robotlegs Sharp implements it:**
```csharp
public interface IHook { void Hook(); }

// Usage:
commandMap.Map(EventType.PURCHASE).ToCommand<PurchaseCommand>()
    .WithGuards<HasFundsGuard>()
    .WithHooks<LogPurchaseHook, AnalyticsHook>();
```

Hooks are injector-instantiated. The command itself is temporarily mapped into the injector so hooks can inject and modify it before execution.

**Would it be useful for Framewerk?** **YES — Moderately useful.** Less critical than Guards since you can do pre-work in the command itself, but hooks are great for cross-cutting concerns (logging, analytics, auth checks) without polluting command logic.

**Priority: MEDIUM**

---

### 3. Formal Context Lifecycle (State Machine)

**What it is:** A full lifecycle state machine for contexts with states: `UNINITIALIZED → INITIALIZING → ACTIVE → SUSPENDING → SUSPENDED → RESUMING → DESTROYING → DESTROYED`. Each transition has Before/When/After hooks. Supports async before-handlers.

**How Robotlegs Sharp implements it:**
```csharp
context.BeforeInitializing(() => LoadConfig());
context.WhenInitializing(() => SetupServices());
context.AfterInitializing(() => NotifyReady());

context.BeforeDestroying(() => SaveState());
context.WhenDestroying(() => CleanupConnections());

// Suspend/Resume for backgrounding:
context.WhenSuspending(() => PauseUpdates());
context.WhenResuming(() => ResumeUpdates());
```

The `Lifecycle` class manages `LifecycleTransition` objects that enforce valid state transitions (e.g., can't destroy from UNINITIALIZED). Transitions can be reversed (destroy fires callbacks in reverse order). Supports async before-handlers via `HandlerMessageCallbackDelegate`.

**Would it be useful for Framewerk?** **YES — Useful but secondary.** Framewerk's contexts have `addCoreComponents/mapBindings/Launch/OnRemove` but no formal suspend/resume, no before/after hooks on lifecycle transitions, and no state validation. The suspend/resume pattern is particularly useful for mobile games. However, Framewerk's current approach works for most Unity scenarios.

**Priority: MEDIUM**

---

### 4. IConfig — Deferred Configuration Objects

**What it is:** Configuration classes that implement `IConfig` with a `Configure()` method. They are instantiated via the injector and have their `Configure()` called during context initialization. Unlike extensions (which set up infrastructure), configs set up application-level bindings.

**How Robotlegs Sharp implements it:**
```csharp
public interface IConfig { void Configure(); }

public class GameConfig : IConfig
{
    [Inject] public IMediatorMap mediatorMap { get; set; }
    [Inject] public IEventCommandMap commandMap { get; set; }
    
    public void Configure()
    {
        mediatorMap.Map<PlayerView>().ToMediator<PlayerMediator>();
        commandMap.Map(GameEvent.START).ToCommand<StartGameCommand>();
    }
}

// Usage:
context.Configure<GameConfig>();
```

Configs are queued until the context initializes, then processed in order. This separates "what extensions the framework has" from "how the app uses them."

**Would it be useful for Framewerk?** **Partially — Framewerk's BindingBundle already covers this.** Our `BindingBundle` system with `OnInstall()` is essentially a more powerful version (tracked bindings, uninstall, cooperative BindIfMissing). The key difference is Robotlegs configs are fire-and-forget (no uninstall), while our bundles support cleanup. **We're actually ahead here.**

**Priority: LOW** (already covered by BindingBundle)

---

### 5. IExtension — Pluggable Framework Extensions

**What it is:** Extensions implement `IExtension` with an `Extend(IContext context)` method. They add capabilities to the framework — things like the event dispatcher, command map, mediator map, etc. are ALL extensions, not built-in.

**How Robotlegs Sharp implements it:**
```csharp
public interface IExtension { void Extend(IContext context); }

public class EventCommandMapExtension : IExtension
{
    public void Extend(IContext context)
    {
        context.injector.Map(typeof(IEventCommandMap)).ToSingleton(typeof(EventCommandMap));
    }
}

// Bundles group extensions:
public class MVCSBundle : IExtension
{
    public void Extend(IContext context)
    {
        context.Install(typeof(EventDispatcherExtension));
        context.Install(typeof(EventCommandMapExtension));
        context.Install(typeof(MediatorMapExtension));
        // etc.
    }
}
```

The `ExtensionInstaller` ensures each extension type is only installed once (deduplication).

**Would it be useful for Framewerk?** **Interesting but different philosophy.** Framewerk's approach is monolithic — the context inherits everything (StrangeIoC baked in). Robotlegs Sharp's approach is fully compositional. Our BindingBundle is closer to IConfig than IExtension. To adopt this pattern would mean a major architectural shift. Not practical now.

**Priority: LOW** (architectural mismatch — BindingBundle serves our needs)

---

### 6. TypeMatcher — Advanced View Matching

**What it is:** A composable type matching system for views: `AllOf(types)`, `AnyOf(types)`, `NoneOf(types)`. Instead of just mapping `ViewType → MediatorType`, you can match views by interface combinations.

**How Robotlegs Sharp implements it:**
```csharp
// Match any view that implements both IAnimatable AND IClickable:
mediatorMap.MapMatcher(new TypeMatcher().AllOf(typeof(IAnimatable), typeof(IClickable)))
    .ToMediator<AnimClickMediator>();

// Match any view implementing IListItem but NOT IDisabled:
mediatorMap.MapMatcher(new TypeMatcher()
    .AllOf(typeof(IListItem))
    .NoneOf(typeof(IDisabled)))
    .ToMediator<ListItemMediator>();
```

TypeFilter stores AllOf/AnyOf/NoneOf type lists and uses `IsAssignableFrom` for matching.

**Would it be useful for Framewerk?** **YES — Nice to have.** Currently Framewerk only does direct type mapping (`BindMediation<ViewType, MediatorType>()`). Interface-based matching would allow more flexible mediator assignment, especially for component-based views. However, most Unity projects do fine with direct type mapping.

**Priority: LOW-MEDIUM** (nice but rarely needed)

---

### 7. DirectCommandMap — Fire-and-Forget Commands

**What it is:** Execute commands directly without event/signal binding. Map a command, configure it with guards/hooks, then call `Execute()`. Useful for programmatic command execution rather than event-driven.

**How Robotlegs Sharp implements it:**
```csharp
[Inject] public IDirectCommandMap directCommandMap { get; set; }

directCommandMap.Map<RefreshDataCommand>()
    .WithGuards<IsOnlineGuard>()
    .WithHooks<ShowLoadingHook>()
    .Execute();
```

It creates a sandboxed child injector, maps the command, executes it, and cleans up.

**Would it be useful for Framewerk?** **Moderately.** In Framewerk, if you want to execute a command programmatically, you dispatch the signal it's bound to. DirectCommandMap removes the need for a signal when you just want to run a command imperatively. It's cleaner for one-off operations.

**Priority: LOW-MEDIUM**

---

### 8. Pin / Detain / Release — Object Pinning

**What it is:** Pin objects in memory to prevent garbage collection. The `Pin` class maintains a dictionary of detained objects. Context delegates to it. Fires `Detained`/`Released` events.

**How Robotlegs Sharp implements it:**
```csharp
context.Detain(myAsyncCommand);  // prevent GC
// ... async work completes ...
context.Release(myAsyncCommand); // allow GC
```

Used mainly for async commands that need to survive GC between dispatch and completion.

**Would it be useful for Framewerk?** **Not really.** Unity's C# GC won't collect objects referenced from MonoBehaviours or static fields. The main use case (async commands) is better handled by coroutines, UniTask, or async/await in Unity. StrangeIoC's singleton bindings already pin objects.

**Priority: LOW**

---

### 9. LocalEventMap — Scoped Listener Management

**What it is:** A per-mediator event listener tracker that auto-removes all listeners on mediator destroy. Supports suspend/resume of all tracked listeners.

**How Robotlegs Sharp implements it:**
```csharp
public class Mediator
{
    [Inject] public IEventMap eventMap { get; set; }
    
    public void Initialize()
    {
        // These are all auto-cleaned up on destroy:
        AddViewListener(ViewEvent.CLICK, OnClick);
        AddContextListener(GameEvent.PAUSE, OnPause);
    }
    
    // PostDestroy auto-calls eventMap.UnmapListeners()
}
```

The `EventMap` stores all listener configs and can bulk `Suspend()` / `Resume()` / `UnmapListeners()`.

**Would it be useful for Framewerk?** **Partially.** Framewerk's `ExtendedMediator` already tracks UI listeners (buttons, toggles, sliders) and auto-removes them in `OnRemove()`. Framewerk also has `[ListensTo]` attribute for signal auto-wiring. However, our approach is Unity-specific while Robotlegs Sharp's is more generic. The suspend/resume pattern for listener groups is interesting but niche.

**Priority: LOW** (mostly covered by ExtendedMediator + [ListensTo])

---

### 10. ViewProcessorMap — Non-Mediator View Processing

**What it is:** Process views without creating mediators. Map a view type to a "processor" that runs custom logic (injection, property setting, etc.). Unlike mediation, processors don't create long-lived companion objects.

**How Robotlegs Sharp implements it:**
```csharp
// Inject dependencies directly into views (no mediator):
viewProcessorMap.Map(typeof(ScoreDisplay))
    .ToProcess(new ViewInjectionProcessor());

// Or custom processing:
viewProcessorMap.Map(typeof(EnemyView))
    .ToProcess(new SpawnEffectProcessor())
    .WithGuards<IsVisibleGuard>();
```

Includes `ViewInjectionProcessor` (inject dependencies into view), `FastPropertyInjector` (skip reflection, use a field name → type map), and `PropertyValueInjector` (set specific values).

**Would it be useful for Framewerk?** **Interesting but low priority.** View injection without mediators goes against Framewerk's clean MVCS pattern where views are kept dumb. The FastPropertyInjector is interesting for performance but Framewerk's reflection-based injection works fine for Unity.

**Priority: LOW**

---

### 11. VigilanceExtension — Strict Mode

**What it is:** An extension that converts framework warnings into exceptions. Catches injection mapping overrides and throws. Makes the framework fail-fast during development.

**How Robotlegs Sharp implements it:**
```csharp
public class VigilanceExtension : IExtension, ILogTarget
{
    public void Extend(IContext context)
    {
        context.AddLogTarget(this);
        context.injector.MappingOverride += MappingOverrideHandler;
    }
    
    public void Log(object source, LogLevel level, ...)
    {
        if (level <= LogLevel.WARN)
            throw new VigilantException(...);
    }
    
    void MappingOverrideHandler(...)
    {
        throw new InjectorException("Mapping override for...");
    }
}
```

**Would it be useful for Framewerk?** **YES — Quick win.** A debug-only mode that makes binding conflicts and warnings throw exceptions would catch bugs earlier. Could be implemented as a simple flag on the context.

**Priority: MEDIUM** (easy to implement, good dev experience)

---

### 12. ModuleConnector — Channel-Based Cross-Context Communication

**What it is:** A typed channel system for communication between contexts. Instead of a single cross-context dispatcher, you can create named channels and explicitly control which events are relayed vs received per channel.

**How Robotlegs Sharp implements it:**
```csharp
[Inject] public IModuleConnector connector { get; set; }

// In context A:
connector.OnDefaultChannel()
    .RelayEvent(GameEvent.SCORE_CHANGED)
    .ReceiveEvent(GameEvent.PAUSE);

// In context B:
connector.OnChannel("audio")
    .RelayEvent(AudioEvent.PLAY_SOUND)
    .ReceiveEvent(AudioEvent.MUTE);
```

Uses `EventRelay` to pipe specific event types between local and channel dispatchers. Channels are auto-created on the root injector. Supports suspend/resume/destroy.

**Would it be useful for Framewerk?** **Interesting but Framewerk's CrossContextBridge already handles this.** Our approach is simpler (everything goes across) while Robotlegs Sharp is more selective (opt-in per event type). The channel concept could reduce cross-context noise, but adds complexity.

**Priority: LOW** (CrossContextBridge works fine for most cases)

---

### 13. Mediator Lifecycle Methods (Pre/Post)

**What it is:** Robotlegs Sharp mediators have a 5-step lifecycle: `PreInitialize → set viewComponent → Initialize → PostInitialize` and `PreDestroy → Destroy → clear viewComponent → PostDestroy`. This gives hooks for setup before/after the view is set.

**How Robotlegs Sharp implements it:**
```csharp
public class MediatorManager
{
    private void InitializeMediator(object mediator, object mediatedItem)
    {
        CallMethod("PreInitialize", ...);      // Before view is set
        SetFieldOrProperty("viewComponent", ...); // Set the view
        CallMethod("Initialize", ...);          // Main init (view available)
        CallMethod("PostInitialize", ...);      // After init
    }
    
    private void DestroyMediator(object mediator)
    {
        CallMethod("PreDestroy", ...);
        CallMethod("Destroy", ...);
        SetFieldOrProperty("viewComponent", null);
        CallMethod("PostDestroy", ...);  // Auto-cleanup (EventMap etc)
    }
}
```

**Would it be useful for Framewerk?** **Not particularly.** StrangeIoC's `OnRegister`/`OnRemove` with `[PostConstruct]` gives us enough lifecycle hooks. Our `[ListensTo]` handles the PostDestroy cleanup automatically.

**Priority: LOW**

---

### 14. AddConfigHandler — Extensible Config Processing

**What it is:** Extensions can register custom matchers + handlers for config objects. When `context.Configure(obj)` is called, the config is run through all registered handlers. This allows extensions to react to specific config types.

**How Robotlegs Sharp implements it:**
```csharp
// In an extension:
context.AddConfigHandler(
    new InstanceOfMatcher(typeof(IContextView)),
    HandleContextView
);

// When someone calls context.Configure(new ContextView(transform)):
// HandleContextView gets called automatically
```

Uses the `ObjectProcessor` class — a chain of matcher + handler pairs.

**Would it be useful for Framewerk?** **Architecturally interesting but not needed.** This enables the compositional extension pattern. Since Framewerk uses inheritance-based contexts rather than compositional ones, there's no equivalent need.

**Priority: LOW**

---

### 15. Namespace/Assembly-Based Matching

**What it is:** Match views by namespace or assembly in addition to type. `NamespaceMatcher` and `NamespaceFilter` allow matching all views in a given namespace.

**How Robotlegs Sharp implements it:**
```csharp
// Match all views in a namespace:
mediatorMap.MapMatcher(new NamespaceMatcher()
    .Require("Game.Views.HUD"))
    .ToMediator<HUDMediator>();
```

**Would it be useful for Framewerk?** **Not really for Unity.** Unity's view hierarchy is scene-based, not namespace-based. Mediator mapping by type is the standard approach.

**Priority: LOW**

---

## Summary Priority Matrix

| Concept | Priority | Effort | Value |
|---------|----------|--------|-------|
| **Guards** (conditional command/mediator execution) | **HIGH** | Medium | High — reusable conditional logic |
| **Hooks** (pre-execution interceptors) | **MEDIUM** | Medium | Medium — cross-cutting concerns |
| **Vigilance / Strict Mode** | **MEDIUM** | Low | High — catches bugs early |
| **Context Lifecycle (Suspend/Resume)** | **MEDIUM** | High | Medium — mobile backgrounding |
| **TypeMatcher** (AllOf/AnyOf/NoneOf) | **LOW-MED** | Medium | Low — rarely needed |
| **DirectCommandMap** | **LOW-MED** | Low | Low — signal dispatch works |
| IConfig | LOW | — | Covered by BindingBundle |
| IExtension | LOW | — | Architectural mismatch |
| Pin/Detain | LOW | — | Not needed in Unity |
| LocalEventMap | LOW | — | Covered by ExtendedMediator |
| ViewProcessorMap | LOW | — | Against MVCS philosophy |
| ModuleConnector channels | LOW | — | CrossContextBridge works |
| Mediator Pre/Post lifecycle | LOW | — | OnRegister/OnRemove sufficient |
| AddConfigHandler | LOW | — | Needs compositional arch |
| Namespace matching | LOW | — | Not relevant for Unity |

## Recommendation

**Top 3 things to consider implementing:**

1. **Guards** — This is the biggest win. A simple `IGuard` interface + `Guards.Approve()` utility would let commands and mediator mappings have injectable conditional logic. Could be added to the BindingBundle DSL: `BindCommand<TSignal, TCommand>().WithGuards<TGuard>()`.

2. **Vigilance/Strict Mode** — Trivially implementable. Add a static `FramewerkConfig.StrictMode` flag that makes duplicate bindings, missing bindings, etc. throw exceptions instead of silently continuing.

3. **Hooks** — Secondary to Guards but follows the same pattern. Could be added alongside Guards for completeness.

## What Framewerk Does BETTER Than Robotlegs Sharp

1. **BindingBundle with tracked cleanup** — Robotlegs Sharp's IConfig is fire-and-forget. Our bundles track everything and can cleanly uninstall. This is objectively superior for modular apps.

2. **BindIfMissing cooperative pattern** — Robotlegs Sharp has nothing like this. Their extensions just install and hope there are no conflicts.

3. **Unity-native integration** — Framewerk builds on StrangeIoC which has years of Unity battle-testing. Robotlegs Sharp's Unity integration feels bolted on (separate Platform namespace, custom View/MonoBehaviour management).

4. **Signal-based mediation with [ListensTo]** — Robotlegs Sharp uses the event-based approach requiring explicit listener management. Framewerk's signal + [ListensTo] attribute is more concise and less error-prone.

5. **DestroyingBinder** — Auto-calling `Destroy()` on `IDestroyable` objects when unbound. Robotlegs Sharp has no equivalent; you need to manually clean up.

6. **ExtendedMediator** — Tracked UI listener management (buttons, toggles, sliders, drag, etc.) with auto-cleanup. Robotlegs Sharp's Mediator only handles EventMap listeners.
