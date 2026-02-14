# Framewerk Code Style Guide

Coding conventions for Framewerk and projects built on it.

## Binding Conventions

### Injection Bindings

**Avoid redundant `.To<SameType>()`:**

```csharp
// ✅ Good — ToSingleton implies self-binding
injectionBinder.Bind<MyService>().ToSingleton();

// ❌ Bad — redundant, MyService is already the bound type
injectionBinder.Bind<MyService>().To<MyService>().ToSingleton();
```

**Use `.To<T>()` only for interface→implementation:**

```csharp
// ✅ Correct — interface to implementation
injectionBinder.Bind<IMyService>().To<MyService>().ToSingleton();

// ✅ Correct — polymorphic binding (multiple interfaces → one impl)
injectionBinder.Bind<IReader>().Bind<IWriter>().To<FileHandler>().ToSingleton();
```

**Prefer `.ToSingleton()` over `.ToValue()` when possible:**

```csharp
// ✅ Let the framework instantiate and inject
injectionBinder.Bind<ISettings>().To<AppSettings>().ToSingleton();

// ✅ Use ToValue only for pre-existing instances or manual construction
var existingInstance = new AppSettings(customConfig);
injectionBinder.Bind<ISettings>().ToValue(existingInstance);
```

### Signal Bindings

**Signals are auto-bound as singletons by SignalCommandBinder:**

```csharp
// ✅ Just bind the command — signal gets auto-bound
commandBinder.Bind<DoSomethingSignal>().To<DoSomethingCommand>();

// ❌ Don't manually bind signals used with commandBinder
injectionBinder.Bind<DoSomethingSignal>().ToSingleton();  // Redundant!
```

**Explicitly bind signals only for injection-only use:**

```csharp
// ✅ Signal used for injection only (no command mapping)
injectionBinder.Bind<DataChangedSignal>().ToSingleton();
```

### Mediation Bindings

```csharp
// ✅ Standard form
mediationBinder.Bind<MyView>().To<MyMediator>();

// ❌ Don't add ToSingleton — mediators are 1:1 with views
mediationBinder.Bind<MyView>().To<MyMediator>().ToSingleton();  // Wrong!
```

## Naming Conventions

| Type | Suffix | Example |
|------|--------|---------|
| View (MonoBehaviour) | `View` | `PlayerView`, `SettingsView` |
| Mediator | `Mediator` | `PlayerMediator`, `SettingsMediator` |
| Command | `Command` | `LoadPlayerCommand`, `SaveCommand` |
| Signal | `Signal` | `PlayerDiedSignal`, `GameStartSignal` |
| Service interface | `I*` | `IPlayerService`, `IAnalytics` |
| Service implementation | (no suffix) | `PlayerService`, `FirebaseAnalytics` |
| Model | `Model` or `Data` | `PlayerModel`, `SettingsData` |
| App state | `State` | `GameplayState`, `MenuState` |
| List item view | `ItemView` | `InventoryItemView`, `LeaderboardItemView` |
| List item mediator | `ItemMediator` | `InventoryItemMediator` |
| Popup view | `View` or `Popup` | `ConfirmPopupView`, `AlertView` |

## Class Structure

### Mediator Layout

```csharp
public class MyMediator : ExtendedMediator<MyView>
{
    // 1. Injections
    [Inject] public IMyService MyService { get; set; }
    [Inject] public SomeSignal SomeSignal { get; set; }
    
    // 2. OnRegister — wire listeners
    public override void OnRegister()
    {
        base.OnRegister();
        AddButtonListener(View.button, OnButtonClicked);
    }
    
    // 3. Signal listeners (using [ListensTo] attribute)
    [ListensTo(typeof(DataChangedSignal))]
    public void OnDataChanged(SomeData data)
    {
        View.UpdateDisplay(data);
    }
    
    // 4. Private handlers
    private void OnButtonClicked()
    {
        SomeSignal.Dispatch();
    }
    
    // 5. OnRemove — only if additional cleanup needed
    public override void OnRemove()
    {
        // Custom cleanup here
        base.OnRemove();  // Always call base — handles listener cleanup
    }
}
```

### Command Layout

```csharp
public class MyCommand : Command
{
    // 1. Injections
    [Inject] public IMyService MyService { get; set; }
    [Inject] public SomeData Data { get; set; }  // Signal payload
    
    // 2. Execute
    public override void Execute()
    {
        var result = MyService.DoSomething(Data);
        // ...
    }
}
```

### Context Layout

```csharp
public class MyContext : FramewerkMVCSContext
{
    // 1. Constructor
    public MyContext(MonoBehaviour view) : base(view) { }
    
    // 2. mapBindings — all bindings here
    protected override void mapBindings()
    {
        base.mapBindings();
        
        // Services
        injectionBinder.Bind<IMyService>().To<MyService>().ToSingleton();
        
        // Commands
        commandBinder.Bind<ContextStartSignal>().To<StartupCommand>();
        commandBinder.Bind<DoSomethingSignal>().To<DoSomethingCommand>();
        
        // Mediations
        mediationBinder.Bind<MyView>().To<MyMediator>();
    }
}
```

## General C# Style

### Braces

```csharp
// ✅ Allman style for methods and classes
public void DoSomething()
{
    if (condition)
    {
        // ...
    }
}

// ✅ Single-line for simple properties
public int Count { get; set; }
public int Value => _value;
```

### Access Modifiers

```csharp
// ✅ Explicit access modifiers
private int _count;
protected virtual void OnSomething() { }
public void DoThing() { }

// ❌ Don't rely on defaults
int _count;  // Bad — implicit private
```

### Fields and Properties

```csharp
// ✅ Private fields with underscore prefix
private int _count;
private IService _service;

// ✅ Auto-properties for injections
[Inject] public IService Service { get; set; }

// ✅ Expression body for simple getters
public bool IsReady => _initialized && _data != null;
```

### Null Handling

```csharp
// ✅ Null-conditional and null-coalescing
var name = user?.Name ?? "Anonymous";
callback?.Invoke();

// ✅ Guard clauses
if (data == null) return;
```

## Default Parameters

**Don't pass parameters that match defaults:**

```csharp
// ✅ Good — false is the default
Instantiate(prefab, parent);

// ❌ Bad — redundant parameter
Instantiate(prefab, parent, false);  // instantiateInWorldSpace defaults to false
```

## Documentation

- Use XML docs for public APIs in framework code
- Keep comments minimal in application code — code should be self-documenting
- Document "why", not "what"

```csharp
/// <summary>
/// Manages async asset loading via Addressables.
/// </summary>
public class AssetManager : IAssetManager
{
    // Local caching to prevent duplicate load requests
    private readonly Dictionary<string, AssetHandle> _loadingAssets = new();
}
```
