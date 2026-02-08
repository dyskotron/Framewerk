# Chapter 8: Binding Bundles

As projects grow, Context files become unwieldy. Binding Bundles provide modular, reusable groups of bindings that can be installed, uninstalled, and shared across contexts.

---

## 8.1 What are Binding Bundles?

### The Problem: Context Sprawl

In a large project, your Context's `MapBindings()` can balloon to hundreds of lines:

```csharp
// A real project's context... 200+ lines of bindings
protected override void MapBindings()
{
    base.MapBindings();
    
    // Core services
    injectionBinder.Bind<ILogger>().To<UnityLogger>().ToSingleton();
    injectionBinder.Bind<IAnalytics>().To<AnalyticsService>().ToSingleton();
    injectionBinder.Bind<ISaveSystem>().To<JsonSaveSystem>().ToSingleton();
    // ... 30 more services
    
    // Audio system
    injectionBinder.Bind<IAudioManager>().To<AudioManager>().ToSingleton();
    injectionBinder.Bind<IMusicPlayer>().To<MusicPlayer>().ToSingleton();
    injectionBinder.Bind<ISFXPlayer>().To<SFXPlayer>().ToSingleton();
    commandBinder.Bind<PlaySFXSignal>().To<PlaySFXCommand>();
    commandBinder.Bind<PlayMusicSignal>().To<PlayMusicCommand>();
    // ... 20 more audio bindings
    
    // Inventory system  
    injectionBinder.Bind<IInventory>().To<Inventory>().ToSingleton();
    injectionBinder.Bind<IItemDatabase>().To<ItemDatabase>().ToSingleton();
    mediationBinder.Bind<InventoryView>().To<InventoryMediator>();
    mediationBinder.Bind<ItemSlotView>().To<ItemSlotMediator>();
    // ... 40 more inventory bindings
    
    // And on, and on...
}
```

Problems:
- **Unmaintainable** — finding a binding takes forever
- **Not reusable** — audio bindings can't be shared with another project
- **No cleanup** — removing a feature means hunting through the file
- **No modularity** — can't load features on demand

### The Solution: Binding Bundles

Bundles group related bindings into self-contained, installable modules:

```csharp
// Clean, modular Context
protected override void MapBindings()
{
    base.MapBindings();
    
    InstallBundle<CoreServicesBundle>();
    InstallBundle<AudioBundle>();
    InstallBundle<InventoryBundle>();
    InstallBundle<ChatBundle>();
}
```

Each bundle manages its own bindings and can be:
- **Installed** with a single call
- **Uninstalled** at runtime (all bindings tracked automatically)
- **Reused** across projects
- **Tested** in isolation

### Bundle Hierarchy

Framewerk provides two bundle base classes:

| Class | Package | Use Case |
|-------|---------|----------|
| `CoreBindingBundle` | framewerk.core | Pure .NET — injections + commands only |
| `BindingBundle` | framewerk.ui | Unity UI — adds mediation support |

**Rule of thumb:**
- Building backend services, pure logic? → `CoreBindingBundle`
- Building UI features with Views/Mediators? → `BindingBundle`

---

## 8.2 Creating Custom Bundles

### Basic Bundle Structure

Extend `CoreBindingBundle` (or `BindingBundle` for UI) and override `OnInstall()`:

```csharp
using Framewerk.StrangeCore.Bundles;

namespace MyGame.Bundles
{
    public class AudioBundle : CoreBindingBundle
    {
        protected override void OnInstall()
        {
            // Injection bindings
            BindInjection<IAudioManager>().To<AudioManager>().ToSingleton();
            BindInjection<IMusicPlayer>().To<MusicPlayer>().ToSingleton();
            BindInjection<ISFXPlayer>().To<SFXPlayer>().ToSingleton();
            
            // Command bindings
            BindCommand<PlaySFXSignal, PlaySFXCommand>();
            BindCommand<PlayMusicSignal, PlayMusicCommand>();
            BindCommand<StopMusicSignal, StopMusicCommand>();
        }
    }
}
```

### Key Methods (CoreBindingBundle)

| Method | Purpose |
|--------|---------|
| `BindInjection<T>()` | Bind type for injection (tracked) |
| `BindInjection<T>(name)` | Bind named type (tracked) |
| `BindCommand<TSignal, TCommand>()` | Bind signal to command (tracked) |
| `BindIfMissing<T>()` | Cooperative — bind only if not already bound |
| `BindCommandIfMissing<TSignal, TCommand>()` | Cooperative command binding |

### UI Bundle with Mediation

For UI features, extend `BindingBundle` to get mediation support:

```csharp
using Framewerk.StrangeCore.Bundles;

namespace MyGame.Bundles
{
    public class ChatBundle : BindingBundle
    {
        protected override void OnInstall()
        {
            // Services
            BindInjection<IChatService>().To<ChatService>().ToSingleton();
            BindInjection<IChatHistory>().To<ChatHistory>().ToSingleton();
            
            // Commands
            BindCommand<SendMessageSignal, SendMessageCommand>();
            BindCommand<ReceiveMessageSignal, ReceiveMessageCommand>();
            
            // Mediations (only available in BindingBundle)
            BindMediation<ChatWindowView, ChatWindowMediator>();
            BindMediation<ChatBubbleView, ChatBubbleMediator>();
            BindMediation<ChatInputView, ChatInputMediator>();
        }
    }
}
```

### Additional Mediation Methods

| Method | Purpose |
|--------|---------|
| `BindMediation<TView, TMediator>()` | Bind view to mediator (tracked) |
| `BindMediationIfMissing<TView, TMediator>()` | Cooperative — bind only if view not bound |

---

## 8.3 Tracked Bindings and Automatic Cleanup

### Why Tracking Matters

Bundles automatically track every binding made through their helper methods. When `Uninstall()` is called, all bindings are removed cleanly:

```csharp
// Internally, bundles maintain lists:
private readonly List<TrackedInjectionBinding> _injectionBindings = new();
private readonly List<Type> _commandBindings = new();
private readonly List<Type> _mediationBindings = new();  // BindingBundle only

// Each BindX call adds to the appropriate list
protected IInjectionBinding BindInjection<T>()
{
    var binding = InjectionBinder.Bind<T>();
    _injectionBindings.Add(new TrackedInjectionBinding(typeof(T), null));
    return binding;
}
```

### Uninstall Behavior

Calling `Uninstall()` reverses all bindings in order:

1. Mediations removed first (BindingBundle)
2. Commands removed
3. Injections removed

```csharp
public void Uninstall()
{
    if (!IsInstalled) return;
    
    // Unbind injections
    foreach (var binding in _injectionBindings)
        InjectionBinder.Unbind(binding.Key, binding.Name);
    
    // Unbind commands
    foreach (var signalType in _commandBindings)
        CommandBinder.Unbind(signalType);
    
    IsInstalled = false;
}
```

### Manual Binding = Manual Cleanup

**Warning:** If you access binders directly, those bindings are NOT tracked:

```csharp
protected override void OnInstall()
{
    // ✅ Tracked — will be cleaned up on Uninstall
    BindInjection<IFoo>().To<Foo>().ToSingleton();
    
    // ❌ NOT tracked — you must clean this up yourself
    InjectionBinder.Bind<IBar>().To<Bar>().ToSingleton();
}
```

Stick to the helper methods unless you have a specific reason to manage bindings manually.

---

## 8.4 Using Bundles in Context

### Installing Bundles

Use `InstallBundle<T>()` in your Context's `MapBindings()`:

```csharp
public class GameContext : FramewerkContext
{
    protected override void MapBindings()
    {
        base.MapBindings();
        
        // Core infrastructure first
        InstallBundle<CoreServicesBundle>();
        InstallBundle<AudioBundle>();
        
        // Feature bundles
        InstallBundle<InventoryBundle>();
        InstallBundle<ChatBundle>();
        InstallBundle<AchievementsBundle>();
    }
}
```

### How InstallBundle Works

1. Creates the bundle via the injector (dependencies injected)
2. Calls `Install()` on the bundle
3. Tracks the bundle for cleanup on context removal

```csharp
protected T InstallBundle<T>(T instance = null) where T : CoreBindingBundle
{
    // Guard against duplicates
    foreach (var existing in _installedBundles)
        if (existing is T)
            throw new InvalidOperationException($"Bundle {typeof(T).Name} is already installed.");
    
    // Create via injector or use provided instance
    if (instance != null)
        injectionBinder.Bind<T>().ToValue(instance);
    else
        injectionBinder.Bind<T>().To<T>();
    
    var bundle = injectionBinder.GetInstance<T>();
    bundle.Install();
    _installedBundles.Add(bundle);
    return bundle;
}
```

### Automatic Cleanup on Context Removal

When the context is removed, all bundles are uninstalled in reverse order:

```csharp
public override void OnRemove()
{
    for (int i = _installedBundles.Count - 1; i >= 0; i--)
    {
        _installedBundles[i].Uninstall();
    }
    _installedBundles.Clear();
    base.OnRemove();
}
```

---

## 8.5 Cooperative Bindings with BindIfMissing

### The Problem: Shared Dependencies

Multiple bundles often need the same service:

```csharp
public class InventoryBundle : BindingBundle
{
    protected override void OnInstall()
    {
        BindInjection<ILogger>().To<UnityLogger>().ToSingleton();  // Logger needed here
        BindInjection<IInventory>().To<Inventory>().ToSingleton();
    }
}

public class ChatBundle : BindingBundle
{
    protected override void OnInstall()
    {
        BindInjection<ILogger>().To<UnityLogger>().ToSingleton();  // Logger needed here too!
        BindInjection<IChatService>().To<ChatService>().ToSingleton();
    }
}
```

If both bundles are installed, you get a duplicate binding error.

### The Solution: BindIfMissing

Use `BindIfMissing<T>()` for shared dependencies:

```csharp
public class InventoryBundle : BindingBundle
{
    protected override void OnInstall()
    {
        // Cooperative: only bind if ILogger isn't already bound
        BindIfMissing<ILogger>().To<UnityLogger>().ToSingleton();
        
        // Exclusive: this bundle owns IInventory
        BindInjection<IInventory>().To<Inventory>().ToSingleton();
    }
}

public class ChatBundle : BindingBundle
{
    protected override void OnInstall()
    {
        // Cooperative: uses existing ILogger if present
        BindIfMissing<ILogger>().To<UnityLogger>().ToSingleton();
        
        // Exclusive: this bundle owns IChatService
        BindInjection<IChatService>().To<ChatService>().ToSingleton();
    }
}
```

Now both bundles can be installed in any order without conflict.

### How BindIfMissing Works

```csharp
protected IInjectionBinding BindIfMissing<T>()
{
    // Already bound? Return no-op binding
    if (InjectionBinder.GetBinding<T>() != null)
    {
        if (VerboseLogging)
            Debug.WriteLine($"[{GetType().Name}] Skipping {typeof(T).Name} — already bound.");
        return NullInjectionBinding.Instance;
    }
    
    // Not bound — bind and track it
    var binding = InjectionBinder.Bind<T>();
    _injectionBindings.Add(new TrackedInjectionBinding(typeof(T), null));
    return binding;
}
```

### The Null Object Pattern

`BindIfMissing` returns a `NullInjectionBinding` when skipped, allowing fluent chaining without null checks:

```csharp
// This works whether the binding happens or not
BindIfMissing<ILogger>().To<UnityLogger>().ToSingleton().CrossContext();
```

`NullInjectionBinding` implements `IInjectionBinding` with no-op methods:

```csharp
public sealed class NullInjectionBinding : IInjectionBinding
{
    public static readonly NullInjectionBinding Instance = new();
    
    public IInjectionBinding ToSingleton() => this;
    public IInjectionBinding ToValue(object o) => this;
    public IInjectionBinding CrossContext() => this;
    // ... all methods return this (no-op)
}
```

### Ownership Rule

**Critical:** Skipped bindings are NOT tracked by the skipping bundle. The bundle that actually created the binding owns it.

```csharp
// Order: CoreBundle installed first, ChatBundle second

public class CoreBundle : CoreBindingBundle
{
    protected override void OnInstall()
    {
        BindInjection<ILogger>().To<UnityLogger>().ToSingleton();  // CoreBundle OWNS this
    }
}

public class ChatBundle : BindingBundle
{
    protected override void OnInstall()
    {
        BindIfMissing<ILogger>().To<UnityLogger>();  // SKIPPED — ChatBundle does NOT own it
        BindInjection<IChatService>().To<ChatService>().ToSingleton();
    }
}

// If ChatBundle.Uninstall() is called:
// - IChatService is unbound (ChatBundle owns it)
// - ILogger remains (CoreBundle owns it)
```

---

## 8.6 Before/After: Context Cleanup

### Before Bundles

```csharp
public class GameContext : FramewerkContext
{
    protected override void MapBindings()
    {
        base.MapBindings();
        
        // Core services (30 lines)
        injectionBinder.Bind<ILogger>().To<UnityLogger>().ToSingleton();
        injectionBinder.Bind<IAnalytics>().To<AnalyticsService>().ToSingleton();
        injectionBinder.Bind<ISaveSystem>().To<JsonSaveSystem>().ToSingleton();
        injectionBinder.Bind<IConfigProvider>().To<ConfigProvider>().ToSingleton();
        injectionBinder.Bind<ILocalization>().To<LocalizationService>().ToSingleton();
        // ... 25 more
        
        // Audio (15 lines)
        injectionBinder.Bind<IAudioManager>().To<AudioManager>().ToSingleton();
        injectionBinder.Bind<IMusicPlayer>().To<MusicPlayer>().ToSingleton();
        injectionBinder.Bind<ISFXPlayer>().To<SFXPlayer>().ToSingleton();
        commandBinder.Bind<PlaySFXSignal>().To<PlaySFXCommand>();
        commandBinder.Bind<PlayMusicSignal>().To<PlayMusicCommand>();
        commandBinder.Bind<StopMusicSignal>().To<StopMusicCommand>();
        // ... 10 more
        
        // Inventory (25 lines)
        injectionBinder.Bind<IInventory>().To<Inventory>().ToSingleton();
        injectionBinder.Bind<IItemDatabase>().To<ItemDatabase>().ToSingleton();
        injectionBinder.Bind<IItemFactory>().To<ItemFactory>().ToSingleton();
        commandBinder.Bind<AddItemSignal>().To<AddItemCommand>();
        commandBinder.Bind<RemoveItemSignal>().To<RemoveItemCommand>();
        mediationBinder.Bind<InventoryView>().To<InventoryMediator>();
        mediationBinder.Bind<ItemSlotView>().To<ItemSlotMediator>();
        // ... 20 more
        
        // Chat (20 lines)
        // Achievements (15 lines)
        // Quests (30 lines)
        // Shop (25 lines)
        // ... 
        
        // Total: 200+ lines in one method
    }
}
```

### After Bundles

```csharp
public class GameContext : FramewerkContext
{
    protected override void MapBindings()
    {
        base.MapBindings();
        
        // Infrastructure
        InstallBundle<CoreServicesBundle>();
        InstallBundle<AudioBundle>();
        
        // Features
        InstallBundle<InventoryBundle>();
        InstallBundle<ChatBundle>();
        InstallBundle<AchievementsBundle>();
        InstallBundle<QuestsBundle>();
        InstallBundle<ShopBundle>();
    }
}
```

**Benefits:**
- **10 lines vs 200+** — instantly scannable
- **Feature isolation** — each bundle is self-contained
- **Easy removal** — delete one line to disable a feature
- **Reusable** — `AudioBundle` can be used in multiple games
- **Testable** — bundles can be installed in test contexts

---

## 8.7 Complete Bundle Example

Here's a full-featured bundle for a multiplayer chat system:

```csharp
using Framewerk.StrangeCore.Bundles;
using strange.extensions.command.impl;
using strange.extensions.signal.impl;

namespace MyGame.Chat
{
    /// <summary>
    /// Complete chat feature bundle.
    /// Provides messaging, history, and UI for multiplayer chat.
    /// </summary>
    public class ChatBundle : BindingBundle
    {
        protected override void OnInstall()
        {
            // === Shared Dependencies (Cooperative) ===
            // These might be provided by other bundles
            BindIfMissing<ILogger>().To<UnityLogger>().ToSingleton();
            BindIfMissing<INetworkClient>().To<NetworkClient>().ToSingleton();
            
            // === Owned Services (Exclusive) ===
            BindInjection<IChatService>().To<ChatService>().ToSingleton();
            BindInjection<IChatHistory>().To<ChatHistory>().ToSingleton();
            BindInjection<IChatFormatter>().To<ChatFormatter>().ToSingleton();
            BindInjection<IProfanityFilter>().To<ProfanityFilter>().ToSingleton();
            
            // === Commands ===
            BindCommand<SendMessageSignal, SendMessageCommand>();
            BindCommand<ReceiveMessageSignal, ReceiveMessageCommand>();
            BindCommand<JoinChannelSignal, JoinChannelCommand>();
            BindCommand<LeaveChannelSignal, LeaveChannelCommand>();
            BindCommand<MuteUserSignal, MuteUserCommand>();
            
            // === Mediations ===
            BindMediation<ChatWindowView, ChatWindowMediator>();
            BindMediation<ChatBubbleView, ChatBubbleMediator>();
            BindMediation<ChatInputView, ChatInputMediator>();
            BindMediation<ChannelTabView, ChannelTabMediator>();
            BindMediation<UserListView, UserListMediator>();
        }
    }
    
    // === Signals ===
    public class SendMessageSignal : Signal<string, string> { }      // channel, message
    public class ReceiveMessageSignal : Signal<ChatMessage> { }
    public class JoinChannelSignal : Signal<string> { }              // channelId
    public class LeaveChannelSignal : Signal<string> { }
    public class MuteUserSignal : Signal<string, bool> { }           // userId, muted
    
    // === Command Example ===
    public class SendMessageCommand : Command
    {
        [Inject] public IChatService ChatService { get; set; }
        [Inject] public IProfanityFilter Filter { get; set; }
        [Inject] public ILogger Logger { get; set; }
        
        public override void Execute()
        {
            var channel = (string)data[0];
            var message = (string)data[1];
            
            var filtered = Filter.Clean(message);
            ChatService.SendMessage(channel, filtered);
            Logger.Log($"[Chat] Sent to {channel}: {filtered}");
        }
    }
}
```

---

## 8.8 Best Practices

### When to Use Bundles

✅ **Use bundles when:**
- Feature has 5+ related bindings
- Feature might be reused in other projects
- Feature might be enabled/disabled at runtime
- You want feature-level testing isolation
- Context file exceeds ~50 bindings

❌ **Skip bundles when:**
- Just 2-3 simple bindings
- One-off project-specific wiring
- Bindings that truly belong to the context level

### Bundle Naming Convention

```
{Feature}Bundle.cs

Examples:
- AudioBundle
- InventoryBundle  
- ChatBundle
- MatchmakingBundle
- TutorialBundle
```

### Folder Structure

```
Assets/Scripts/
├── Bundles/
│   ├── CoreServicesBundle.cs
│   ├── AudioBundle.cs
│   └── ...
├── Features/
│   ├── Chat/
│   │   ├── ChatBundle.cs
│   │   ├── ChatService.cs
│   │   ├── Views/
│   │   └── Commands/
│   └── Inventory/
│       ├── InventoryBundle.cs
│       └── ...
```

### Dependency Order

Install bundles in dependency order — core infrastructure first:

```csharp
protected override void MapBindings()
{
    base.MapBindings();
    
    // 1. Core infrastructure (no dependencies)
    InstallBundle<LoggingBundle>();
    InstallBundle<NetworkBundle>();
    InstallBundle<SaveBundle>();
    
    // 2. Shared services (depend on core)
    InstallBundle<AudioBundle>();
    InstallBundle<LocalizationBundle>();
    
    // 3. Features (depend on services)
    InstallBundle<InventoryBundle>();
    InstallBundle<ChatBundle>();
}
```

### Verbose Logging for Debugging

Enable bundle logging to debug installation order and skipped bindings:

```csharp
CoreBindingBundle.VerboseLogging = true;

// Output:
// [ChatBundle] Skipping binding for ILogger — already bound.
// [ChatBundle] Skipping binding for INetworkClient — already bound.
```

---

## Summary

| Concept | Purpose |
|---------|---------|
| **CoreBindingBundle** | Base class for .NET bundles (injection + commands) |
| **BindingBundle** | UI bundle with mediation support |
| **InstallBundle<T>()** | Install a bundle in your Context |
| **OnInstall()** | Override to define your bindings |
| **Tracked bindings** | Automatic cleanup on Uninstall() |
| **BindIfMissing<T>()** | Cooperative binding for shared deps |
| **NullInjectionBinding** | No-op binding for fluent chaining |

Bundles transform chaotic 200-line contexts into clean, modular, maintainable architecture. Start extracting bundles as soon as your Context feels crowded — your future self will thank you.
