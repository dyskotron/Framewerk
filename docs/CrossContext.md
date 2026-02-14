# CrossContext — Sharing Bindings Across Contexts

CrossContext enables multiple IoC contexts to share bindings and communicate via events. This is essential in modular applications where different features run in separate contexts but need access to shared services or signals.

## When to Use CrossContext

- **Shared services** — global singletons accessible from any context (e.g., analytics, auth, user settings)
- **Cross-module communication** — events that need to propagate between independent contexts
- **Hierarchical contexts** — parent/child context relationships with inherited bindings

## `.CrossContext()` — Sharing Injections

Add `.CrossContext()` to any injection binding to share it across all contexts:

```csharp
// In parent context — shared singleton accessible everywhere
injectionBinder.Bind<IUserService>().To<UserService>().ToSingleton().CrossContext();

// Factory binding shared across contexts (new instance per injection)
injectionBinder.Bind<ILogger>().To<ConsoleLogger>().CrossContext();

// Named binding shared across contexts
injectionBinder.Bind<IStorage>().To<LocalStorage>().ToSingleton().ToName("local").CrossContext();
```

### How It Works

1. CrossContext bindings are stored in a shared `CrossContextInjectionBinder`
2. Child contexts automatically inherit access to this shared binder
3. When resolving a type, the local binder is checked first, then the cross-context binder
4. Local bindings override cross-context bindings (allows context-specific overrides)

### Example: Shared vs Local Override

```csharp
// Parent context
injectionBinder.Bind<IAnalytics>().To<FirebaseAnalytics>().ToSingleton().CrossContext();

// Child context can override locally
injectionBinder.Bind<IAnalytics>().To<MockAnalytics>().ToSingleton();  // No CrossContext

// In child context:
// - Local injections get MockAnalytics
// - Other contexts still get FirebaseAnalytics
```

## `crossContextDispatcher` — Cross-Context Events

The `crossContextDispatcher` is a shared `IEventDispatcher` for sending events between contexts.

```csharp
// Inject the cross-context dispatcher
[Inject(ContextKeys.CROSS_CONTEXT_DISPATCHER)]
public IEventDispatcher crossContextDispatcher { get; set; }

// Dispatch event to all contexts
crossContextDispatcher.Dispatch(GameEvent.PLAYER_LOGGED_IN, userData);

// Listen in any context's local dispatcher
localDispatcher.AddListener(GameEvent.PLAYER_LOGGED_IN, OnPlayerLoggedIn);
```

### Signal-Based Pattern (Recommended)

For signal-based contexts, bind signals CrossContext:

```csharp
// Shared signal — same instance across all contexts
injectionBinder.Bind<PlayerLoggedInSignal>().ToSingleton().CrossContext();
```

Any context can dispatch or listen to this signal.

## `crossContextBridge` — Event Relay

The `CrossContextBridge` gates which events propagate across contexts. Map events to the bridge to enable cross-context relay:

```csharp
// In any context — enable cross-context relay for this event
crossContextBridge.Bind(GameEvent.MISSILE_HIT);

// Now when ANY context's local dispatcher fires MISSILE_HIT,
// it relays to all other contexts automatically

// To stop relaying:
crossContextBridge.Unbind(GameEvent.MISSILE_HIT);
```

### How the Bridge Works

1. Local dispatcher fires an event
2. If event is mapped to `crossContextBridge`, it triggers
3. Bridge dispatches to `crossContextDispatcher`
4. All contexts' local dispatchers receive the event

**Recommendation:** Map all cross-context events in `firstContext` to avoid confusion.

## Complete Example

```csharp
public class MainContext : FramewerkCrossContext
{
    protected override void mapBindings()
    {
        // Shared singleton — accessible from all contexts
        injectionBinder.Bind<IUserService>().To<UserService>().ToSingleton().CrossContext();
        
        // Shared signal — same instance across contexts
        injectionBinder.Bind<UserLoggedInSignal>().ToSingleton().CrossContext();
        
        // Local-only binding
        injectionBinder.Bind<IScreenManager>().To<ScreenManager>().ToSingleton();
    }
    
    protected override void postBindings()
    {
        // Enable cross-context relay for specific events
        crossContextBridge.Bind(AppEvent.USER_LOGGED_OUT);
    }
}

public class FeatureContext : FramewerkCrossContext
{
    protected override void mapBindings()
    {
        // Can inject IUserService and UserLoggedInSignal from parent
        // No need to bind them here — they're CrossContext
        
        // Feature-specific bindings
        injectionBinder.Bind<IFeatureService>().To<FeatureService>().ToSingleton();
    }
}
```

## Best Practices

1. **Bind shared services in firstContext** — ensures consistent initialization order
2. **Use `.ToSingleton().CrossContext()` for shared state** — factory CrossContext bindings create separate instances per context
3. **Bridge sparingly** — only relay events that truly need cross-context propagation
4. **Prefer signals over events** — type-safe and easier to trace
5. **Local overrides for testing** — bind locally without CrossContext to mock shared services
