# Chapter 3: Commands & Signals

## Introduction

Framewerk's event-driven architecture is built on two powerful concepts: **Signals** and **Commands**. Together, they create a clean, decoupled communication system that keeps your code modular and testable.

- **Signals** are type-safe event dispatchers — they announce that something happened
- **Commands** are single-purpose business logic units — they respond to those announcements

This separation means the code that *fires* an event doesn't need to know *who handles it* or *how*.

---

## 1. Understanding Signals

### What Are Signals?

Signals are Framewerk's type-safe alternative to traditional event systems. Unlike string-based events, Signals enforce type safety at compile time — if you try to listen with the wrong method signature, your code won't compile.

### Signal Variants

Signals come in variants based on how many parameters they carry:

```csharp
// No parameters
Signal gameStarted = new Signal();

// One parameter
Signal<int> scoreChanged = new Signal<int>();

// Two parameters
Signal<string, int> playerScored = new Signal<string, int>();

// Three parameters
Signal<Vector3, Quaternion, float> objectMoved = new Signal<Vector3, Quaternion, float>();

// Four parameters (maximum)
Signal<int, string, bool, float> complexEvent = new Signal<int, string, bool, float>();
```

> ⚠️ **Limitation**: Signals support a maximum of 4 parameters. If you need more, wrap them in a value object (VO).

### Adding and Removing Listeners

```csharp
public class ScoreDisplay : MonoBehaviour
{
    [Inject]
    public Signal<int> scoreChanged { get; set; }
    
    void OnEnable()
    {
        // Add a persistent listener
        scoreChanged.AddListener(OnScoreChanged);
        
        // Or add a one-time listener (auto-removes after first dispatch)
        scoreChanged.AddOnce(OnFirstScore);
    }
    
    void OnDisable()
    {
        // Always clean up listeners to prevent memory leaks!
        scoreChanged.RemoveListener(OnScoreChanged);
    }
    
    private void OnScoreChanged(int newScore)
    {
        Debug.Log($"Score: {newScore}");
    }
    
    private void OnFirstScore(int score)
    {
        Debug.Log("First score received!");
    }
}
```

### Dispatching Signals

```csharp
public class ScoreKeeper
{
    [Inject]
    public Signal<int> scoreChanged { get; set; }
    
    private int _score;
    
    public void AddPoints(int points)
    {
        _score += points;
        
        // Dispatch the signal with the new score
        scoreChanged.Dispatch(_score);
    }
}
```

### Key Signal Methods

| Method | Description |
|--------|-------------|
| `AddListener(callback)` | Add a persistent listener |
| `AddOnce(callback)` | Add a one-time listener (auto-removes after dispatch) |
| `RemoveListener(callback)` | Remove a specific listener |
| `RemoveAllListeners()` | Remove all listeners |
| `Dispatch(...)` | Fire the signal with parameters |

---

## 2. Local vs Global Signals

Signals can be used in two ways: **globally** (shared across the context) or **locally** (private to a component).

### Global Signals (Singleton Pattern)

Global signals are bound in the Context and shared across your application. They're ideal for system-wide events.

```csharp
// Define the signal class
public class GameOverSignal : Signal<bool> { } // bool = playerWon

// In your Context
protected override void mapBindings()
{
    // Bind as singleton — same instance everywhere
    injectionBinder.Bind<GameOverSignal>().ToSingleton();
}

// In any class that needs it
public class GameManager
{
    [Inject]
    public GameOverSignal gameOver { get; set; }
    
    public void EndGame(bool playerWon)
    {
        gameOver.Dispatch(playerWon);
    }
}

public class UIController
{
    [Inject]
    public GameOverSignal gameOver { get; set; }
    
    [PostConstruct]
    public void Init()
    {
        gameOver.AddListener(ShowGameOverScreen);
    }
}
```

### Local Signals (Instance Pattern)

Local signals are created within a specific component and not shared. They're useful for internal communication.

```csharp
public class EnemySpawner
{
    // Local signal — not injected, just a regular field
    public Signal<Enemy> enemySpawned { get; } = new Signal<Enemy>();
    
    public void SpawnEnemy()
    {
        var enemy = CreateEnemy();
        
        // Only listeners who have a reference to this spawner can hear it
        enemySpawned.Dispatch(enemy);
    }
}
```

### When to Use Which?

```
┌─────────────────────────────────────────────────────────────────┐
│                    SIGNAL SCOPE DECISION                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Need system-wide access?                                      │
│       YES → Global Signal (bind in Context as Singleton)        │
│       NO  → Local Signal (create inline in class)               │
│                                                                 │
│   Multiple unrelated listeners?                                 │
│       YES → Global Signal                                       │
│       NO  → Local Signal                                        │
│                                                                 │
│   Part of component's public API?                               │
│       YES → Local Signal (exposed as property)                  │
│       NO  → Depends on use case                                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Commands

### What Are Commands?

Commands are single-purpose classes that encapsulate business logic. They're instantiated, injected with dependencies, executed, and then cleaned up — all automatically.

### The Command Base Class

```csharp
using strange.extensions.command.impl;

public class ScoreCommand : Command
{
    // Dependencies are injected automatically
    [Inject]
    public IScoreModel scoreModel { get; set; }
    
    [Inject]
    public ScoreUpdatedSignal scoreUpdated { get; set; }
    
    // Signal parameters are injected too!
    [Inject]
    public int points { get; set; }
    
    public override void Execute()
    {
        scoreModel.AddPoints(points);
        scoreUpdated.Dispatch(scoreModel.CurrentScore);
    }
}
```

### Injecting Signal Parameters

When a signal triggers a command, its parameters become injectable:

```csharp
// Signal definition
public class PlayerHitSignal : Signal<int, string> { } // damage, weaponType

// Command
public class PlayerHitCommand : Command
{
    // Inject the signal parameters by type
    [Inject]
    public int damage { get; set; }  // First parameter
    
    [Inject]
    public string weaponType { get; set; }  // Second parameter
    
    [Inject]
    public IPlayerHealth playerHealth { get; set; }
    
    public override void Execute()
    {
        playerHealth.TakeDamage(damage);
        Debug.Log($"Hit by {weaponType} for {damage} damage");
    }
}
```

> ⚠️ **Important**: Signal parameters are matched by **type**, not by order. Each parameter type must be unique within a single signal.

---

## 4. Signal-to-Command Binding

### Basic Binding

In your Context's `mapBindings()`:

```csharp
protected override void mapBindings()
{
    // When PlayerHitSignal fires, execute PlayerHitCommand
    commandBinder.Bind<PlayerHitSignal>().To<PlayerHitCommand>();
}
```

### Chaining Multiple Commands

One signal can trigger multiple commands:

```csharp
// All three commands execute in parallel (default)
commandBinder.Bind<EnemyDefeatedSignal>()
    .To<AddScoreCommand>()
    .To<PlaySoundCommand>()
    .To<SpawnLootCommand>();
```

### Sequential Execution

For commands that must run in order:

```csharp
commandBinder.Bind<GameStartSignal>()
    .To<LoadPlayerDataCommand>()
    .To<InitializeWorldCommand>()
    .To<SpawnPlayerCommand>()
    .InSequence();
```

### One-Time Bindings

For commands that should only execute once:

```csharp
// StartupCommand runs once, then the binding is removed
commandBinder.Bind<ContextStartSignal>()
    .To<StartupCommand>()
    .Once();
```

### Pooled Commands

For frequently-used commands, pooling improves performance:

```csharp
commandBinder.Bind<BulletFiredSignal>()
    .To<ProcessBulletCommand>()
    .Pooled();
```

### Binding Summary

```
┌───────────────────────────────────────────────────────────────┐
│                  COMMAND BINDING OPTIONS                       │
├───────────────────────────────────────────────────────────────┤
│                                                                │
│   commandBinder.Bind<SignalType>()                            │
│       .To<CommandA>()           // Single command              │
│       .To<CommandB>()           // Chain multiple commands     │
│       .InSequence()             // Run sequentially            │
│       .InParallel()             // Run in parallel (default)   │
│       .Once()                   // Remove binding after use    │
│       .Pooled()                 // Reuse command instances     │
│                                                                │
└───────────────────────────────────────────────────────────────┘
```

---

## 5. Async Commands: Retain/Release

For commands that need to wait for async operations (like network calls):

### The Problem

By default, commands are cleaned up immediately after `Execute()` returns. If you have async code, the command might be destroyed before it completes.

### The Solution: Retain/Release

```csharp
public class LoginCommand : Command
{
    [Inject]
    public IAuthService authService { get; set; }
    
    [Inject]
    public string username { get; set; }
    
    [Inject]
    public string password { get; set; }
    
    [Inject]
    public LoginSuccessSignal loginSuccess { get; set; }
    
    [Inject]
    public LoginFailedSignal loginFailed { get; set; }
    
    public override void Execute()
    {
        // Retain prevents cleanup until we call Release
        Retain();
        
        authService.Login(username, password, OnLoginComplete);
    }
    
    private void OnLoginComplete(bool success, string error)
    {
        if (success)
        {
            loginSuccess.Dispatch();
        }
        else
        {
            loginFailed.Dispatch(error);
        }
        
        // Release allows the command to be cleaned up
        Release();
    }
}
```

### Retain/Release Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    ASYNC COMMAND LIFECYCLE                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   Signal.Dispatch()                                              │
│         │                                                        │
│         ▼                                                        │
│   ┌─────────────┐                                               │
│   │   Create    │  Command instantiated, dependencies injected  │
│   │   Command   │                                               │
│   └──────┬──────┘                                               │
│          │                                                       │
│          ▼                                                       │
│   ┌─────────────┐                                               │
│   │  Execute()  │  Call Retain() at the start                   │
│   │  + Retain() │                                               │
│   └──────┬──────┘                                               │
│          │                                                       │
│          ▼                                                       │
│   ┌─────────────┐                                               │
│   │   Async     │  Waiting for callback...                      │
│   │   Work      │  (Command stays alive)                        │
│   └──────┬──────┘                                               │
│          │                                                       │
│          ▼                                                       │
│   ┌─────────────┐                                               │
│   │  Callback   │  Work complete                                │
│   │  + Release()│                                               │
│   └──────┬──────┘                                               │
│          │                                                       │
│          ▼                                                       │
│   ┌─────────────┐                                               │
│   │   Cleanup   │  Command is destroyed/returned to pool        │
│   └─────────────┘                                               │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Fail() - Stopping Sequences

If a command in a sequence fails and you need to stop subsequent commands:

```csharp
public class ValidateUserCommand : Command
{
    [Inject]
    public IUserData userData { get; set; }
    
    public override void Execute()
    {
        if (!userData.IsValid)
        {
            // Stop the sequence — subsequent commands won't run
            Fail();
            return;
        }
        
        // Continue with sequence...
    }
}
```

---

## 6. Practical Example: User Login Flow

Let's build a complete login system using signals and commands.

### Step 1: Define the Signals

```csharp
// Request signals (triggered by UI)
public class LoginRequestSignal : Signal<string, string> { } // username, password
public class LogoutRequestSignal : Signal { }

// Response signals (triggered by commands)  
public class LoginSuccessSignal : Signal<UserData> { }
public class LoginFailedSignal : Signal<string> { } // error message
public class LogoutCompleteSignal : Signal { }
```

### Step 2: Create the Service Interface

```csharp
public interface IAuthService
{
    void Login(string username, string password, Action<UserData, string> callback);
    void Logout(Action callback);
}

public class AuthService : IAuthService
{
    public void Login(string username, string password, Action<UserData, string> callback)
    {
        // Simulate network call
        DOVirtual.DelayedCall(1f, () =>
        {
            if (username == "admin" && password == "1234")
            {
                callback(new UserData { Username = username }, null);
            }
            else
            {
                callback(null, "Invalid credentials");
            }
        });
    }
    
    public void Logout(Action callback)
    {
        DOVirtual.DelayedCall(0.5f, callback);
    }
}
```

### Step 3: Create the Commands

```csharp
public class LoginCommand : Command
{
    [Inject]
    public IAuthService authService { get; set; }
    
    [Inject]
    public string username { get; set; }
    
    [Inject]
    public string password { get; set; }
    
    [Inject]
    public LoginSuccessSignal loginSuccess { get; set; }
    
    [Inject]
    public LoginFailedSignal loginFailed { get; set; }
    
    public override void Execute()
    {
        Retain();
        authService.Login(username, password, OnComplete);
    }
    
    private void OnComplete(UserData user, string error)
    {
        if (user != null)
        {
            loginSuccess.Dispatch(user);
        }
        else
        {
            loginFailed.Dispatch(error);
        }
        Release();
    }
}

public class LogoutCommand : Command
{
    [Inject]
    public IAuthService authService { get; set; }
    
    [Inject]
    public LogoutCompleteSignal logoutComplete { get; set; }
    
    public override void Execute()
    {
        Retain();
        authService.Logout(() =>
        {
            logoutComplete.Dispatch();
            Release();
        });
    }
}
```

### Step 4: Bind in Context

```csharp
protected override void mapBindings()
{
    // Services
    injectionBinder.Bind<IAuthService>().To<AuthService>().ToSingleton();
    
    // Signals
    injectionBinder.Bind<LoginRequestSignal>().ToSingleton();
    injectionBinder.Bind<LogoutRequestSignal>().ToSingleton();
    injectionBinder.Bind<LoginSuccessSignal>().ToSingleton();
    injectionBinder.Bind<LoginFailedSignal>().ToSingleton();
    injectionBinder.Bind<LogoutCompleteSignal>().ToSingleton();
    
    // Commands
    commandBinder.Bind<LoginRequestSignal>().To<LoginCommand>();
    commandBinder.Bind<LogoutRequestSignal>().To<LogoutCommand>();
}
```

### Step 5: Use in Mediator

```csharp
public class LoginMediator : Mediator
{
    [Inject]
    public LoginView view { get; set; }
    
    [Inject]
    public LoginRequestSignal loginRequest { get; set; }
    
    public override void OnRegister()
    {
        view.OnLoginClicked += HandleLoginClicked;
    }
    
    public override void OnRemove()
    {
        view.OnLoginClicked -= HandleLoginClicked;
    }
    
    private void HandleLoginClicked(string username, string password)
    {
        loginRequest.Dispatch(username, password);
    }
    
    // Using ListensTo attribute for response signals
    [ListensTo(typeof(LoginSuccessSignal))]
    public void OnLoginSuccess(UserData user)
    {
        view.ShowWelcome(user.Username);
    }
    
    [ListensTo(typeof(LoginFailedSignal))]
    public void OnLoginFailed(string error)
    {
        view.ShowError(error);
    }
}
```

### Complete Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         LOGIN FLOW                                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   ┌──────────┐                                                          │
│   │   View   │ ──── User clicks "Login" ────►                           │
│   └──────────┘                                                          │
│        │                                                                 │
│        ▼                                                                 │
│   ┌──────────┐                                                          │
│   │ Mediator │ ──── loginRequest.Dispatch(user, pass) ────►            │
│   └──────────┘                                                          │
│        │                                                                 │
│        ▼                                                                 │
│   ┌──────────────────┐                                                  │
│   │ LoginRequestSignal│                                                 │
│   └──────────────────┘                                                  │
│        │                                                                 │
│        │  (commandBinder intercepts)                                    │
│        ▼                                                                 │
│   ┌──────────────┐         ┌─────────────┐                              │
│   │ LoginCommand │ ───────►│ AuthService │ (async call)                 │
│   └──────────────┘         └─────────────┘                              │
│        │                          │                                      │
│        │◄─────────────────────────┘ (callback)                          │
│        │                                                                 │
│        ├── SUCCESS ──► LoginSuccessSignal.Dispatch(userData)            │
│        │                       │                                         │
│        │                       ▼                                         │
│        │               ┌──────────┐                                     │
│        │               │ Mediator │ ──► view.ShowWelcome()              │
│        │               └──────────┘                                     │
│        │                                                                 │
│        └── FAILURE ──► LoginFailedSignal.Dispatch(error)                │
│                                │                                         │
│                                ▼                                         │
│                        ┌──────────┐                                     │
│                        │ Mediator │ ──► view.ShowError()                │
│                        └──────────┘                                     │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 7. Common Patterns

### Pattern 1: Request/Response Signals

Always pair request signals with response signals:

```csharp
// Request
public class LoadDataSignal : Signal<string> { } // dataId

// Responses
public class DataLoadedSignal : Signal<DataModel> { }
public class DataLoadFailedSignal : Signal<string> { }
```

### Pattern 2: Progress Updates

For long operations:

```csharp
public class DownloadStartedSignal : Signal { }
public class DownloadProgressSignal : Signal<float> { } // 0-1
public class DownloadCompleteSignal : Signal<byte[]> { }
public class DownloadFailedSignal : Signal<string> { }
```

### Pattern 3: State Change Signals

For observable state:

```csharp
public class GameStateChangedSignal : Signal<GameState, GameState> { } // oldState, newState
public class PlayerHealthChangedSignal : Signal<int, int> { } // oldValue, newValue
```

### Anti-Patterns to Avoid

❌ **Don't pass mutable objects through signals** — listeners might modify them

❌ **Don't forget to Release** — retained commands leak memory

❌ **Don't use duplicate parameter types** — signal parameter injection matches by type

❌ **Don't chain too many commands** — consider refactoring if you have more than 3-4

---

## Summary

| Concept | Purpose |
|---------|---------|
| **Signal** | Type-safe event dispatcher |
| **Signal&lt;T&gt;** | Signal with typed parameters (up to 4) |
| **Command** | Single-purpose business logic unit |
| **commandBinder.Bind&lt;&gt;().To&lt;&gt;()** | Connect signals to commands |
| **Once()** | Remove binding after first use |
| **InSequence()** | Run commands sequentially |
| **Retain()/Release()** | Keep async commands alive |
| **Fail()** | Stop a command sequence |

### Next Steps

- **Chapter 4**: Views & Mediators — connecting signals to UI
- **Chapter 5**: Models & Services — data and external communication
