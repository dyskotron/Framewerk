# Chapter 6: Screen FSM

Application state machine for managing screen flow in Framewerk.

## Overview

The **Screen FSM** (Finite State Machine) provides a structured way to manage your application's high-level states and their corresponding screens. Instead of manually tracking "are we in the menu? in gameplay? in settings?", the FSM handles state lifecycle, transitions, and cleanup automatically.

```
┌─────────────────────────────────────────────────────────────────┐
│                         AppFsm                                   │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐          │
│  │ SplashState │───▶│ MenuState   │───▶│ GameState   │          │
│  │  + Screen   │    │  + Screen   │    │  + Screen   │          │
│  └─────────────┘    └─────────────┘    └─────────────┘          │
│                            │                   │                 │
│                            ▼                   ▼                 │
│                     ┌─────────────┐    ┌─────────────┐          │
│                     │SettingsState│    │GameOverState│          │
│                     │  + Screen   │    │  + Screen   │          │
│                     └─────────────┘    └─────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

## Table of Contents

1. [Understanding AppFsm](#1-understanding-appfsm)
2. [App States](#2-app-states)
3. [State Transitions](#3-state-transitions)
4. [AppStateScreen](#4-appstatescreen)
5. [Practical Example](#5-practical-example)

---

## 1. Understanding AppFsm

### What is a Finite State Machine?

A Finite State Machine (FSM) is a model where your application can only be in **one state at a time**, with clear rules for transitioning between states. Think of it like a flowchart:

- **States** = boxes (Splash, Menu, Game, GameOver)
- **Transitions** = arrows between boxes
- **Current State** = where you are right now

### Why Use FSM vs Manual Management?

**Without FSM (manual chaos):**
```csharp
// Scattered throughout your codebase
public void OnPlayClicked()
{
    menuUI.SetActive(false);           // Hope you remembered this
    gameUI.SetActive(true);            // And this
    currentScreen = "game";            // String-based tracking? Yikes
    audioManager.PlayGameMusic();      // Easy to forget
    // Did we clean up the menu properly?
}
```

**With FSM (structured flow):**
```csharp
public void OnPlayClicked()
{
    Fsm.SwitchState(new GameState());  // That's it. FSM handles the rest.
}
```

**Benefits of FSM:**
- **Single source of truth** — always know what state you're in
- **Automatic cleanup** — exiting a state cleans up its resources
- **Enforced lifecycle** — Enter/Exit methods guarantee proper setup/teardown
- **Queued transitions** — no race conditions when switching rapidly
- **Decoupled logic** — each state is self-contained

### The AppFsm Class

```csharp
public interface IAppFsm
{
    IAppState CurrentState { get; }
    TransitionType CurrentTransition { get; }
    void SwitchState(IAppState newState);
    void Destroy();
}
```

Key properties:
- `CurrentState` — the active state right now
- `CurrentTransition` — whether we're in `Enter`, `Exit`, or `None`

The `TransitionType` enum:
```csharp
public enum TransitionType
{
    None,   // Idle, no transition happening
    Enter,  // Entering a new state
    Exit    // Exiting the current state
}
```

---

## 2. App States

### Creating States with AppState<TScreen>

Each state in Framewerk pairs **logic** with a **screen**:

```csharp
public class MenuState : AppState<MenuScreen>
{
    // Screen is auto-injected — you don't create it manually
    // [Inject] public MenuScreen Screen { get; set; }  ← this happens automatically

    protected override void Enter()
    {
        Debug.Log("Entering menu state");
    }

    protected override void Exit()
    {
        Debug.Log("Exiting menu state");
    }
}
```

The generic parameter `<TScreen>` tells the FSM which screen class to inject.

### The Screen Property

The `Screen` property gives you access to the visual layer:

```csharp
public class GameState : AppState<GameScreen>
{
    [Inject] public IScoreManager ScoreManager { get; set; }

    protected override void Enter()
    {
        // Access your screen's UI
        Screen.UpdateScore(ScoreManager.CurrentScore);
    }

    protected override void RegisterHandlers()
    {
        // Listen to signals from the screen
        Screen.PauseButtonClicked.AddListener(OnPauseClicked);
    }

    protected override void UnregisterHandlers()
    {
        Screen.PauseButtonClicked.RemoveListener(OnPauseClicked);
    }
}
```

### Enter/Exit Lifecycle

The complete lifecycle when entering a state:

```
SwitchState(new GameState())
        │
        ▼
┌───────────────────────────────────────┐
│  1. InjectionBinder.Inject(state)     │  ← Dependencies injected
├───────────────────────────────────────┤
│  2. RegisterHandlers()                │  ← Subscribe to signals
├───────────────────────────────────────┤
│  3. Enter()                           │  ← Your state setup logic
├───────────────────────────────────────┤
│  4. Screen.PerformEnter()             │  ← Screen enters (UI setup)
├───────────────────────────────────────┤
│  5. EnterFinished()                   │  ← Transition complete
├───────────────────────────────────────┤
│  6. AppStateEnterSignal.Dispatch()    │  ← Global notification
└───────────────────────────────────────┘
```

And when exiting:

```
SwitchState(new NextState())  // or Destroy()
        │
        ▼
┌───────────────────────────────────────┐
│  1. Exit()                            │  ← Your cleanup logic
├───────────────────────────────────────┤
│  2. Screen.PerformExit()              │  ← Screen exits (UI teardown)
├───────────────────────────────────────┤
│  3. ExitFinished()                    │  ← Unregisters handlers
├───────────────────────────────────────┤
│  4. AppStateExitSignal.Dispatch()     │  ← Global notification
├───────────────────────────────────────┤
│  5. Destroy()                         │  ← Final cleanup
└───────────────────────────────────────┘
```

### Override Points

| Method | Purpose | When Called |
|--------|---------|-------------|
| `RegisterHandlers()` | Subscribe to signals | Before Enter |
| `Enter()` | State initialization | After handlers registered |
| `Exit()` | State cleanup | When leaving state |
| `UnregisterHandlers()` | Unsubscribe signals | During ExitFinished |
| `Destroy()` | Final cleanup | After exit complete |

---

## 3. State Transitions

### Fsm.SwitchState()

The primary way to change states:

```csharp
public class MenuState : AppState<MenuScreen>
{
    [Inject] public IAppFsm Fsm { get; set; }

    private void OnPlayClicked()
    {
        Fsm.SwitchState(new GameState());
    }

    private void OnSettingsClicked()
    {
        Fsm.SwitchState(new SettingsState());
    }
}
```

### Queued Transitions

What happens if you call `SwitchState()` during a transition?

```csharp
// State A is active, in the middle of exiting
Fsm.SwitchState(new StateB());  // Queued!
Fsm.SwitchState(new StateC());  // Also queued!

// Result: A exits → B enters → B exits → C enters
```

The FSM maintains a queue:
```csharp
private Queue<IAppState> _nextStates;
```

This prevents race conditions and ensures orderly transitions:

```
┌─────┐   Exit   ┌─────┐  Enter  ┌─────┐   Exit   ┌─────┐  Enter
│  A  │─────────▶│     │────────▶│  B  │─────────▶│     │────────▶│  C  │
└─────┘          └─────┘         └─────┘          └─────┘         └─────┘
   ▲                                                                  
   └── SwitchState(B), SwitchState(C) called here
```

### Transition Flow Diagram

```
                    ┌─────────────────────────────────┐
                    │       SwitchState(newState)      │
                    └─────────────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    ▼                               ▼
        ┌─────────────────────┐         ┌─────────────────────┐
        │ CurrentState == null │         │ CurrentState != null │
        └─────────────────────┘         └─────────────────────┘
                    │                               │
                    ▼                               ▼
        ┌─────────────────────┐         ┌─────────────────────┐
        │   StartState(new)    │         │ Queue new state     │
        │   (enter directly)   │         │ CloseCurrentState() │
        └─────────────────────┘         └─────────────────────┘
                    │                               │
                    │                               ▼
                    │                   ┌─────────────────────┐
                    │                   │ Current.PerformExit │
                    │                   └─────────────────────┘
                    │                               │
                    │                               ▼
                    │                   ┌─────────────────────┐
                    │                   │ ExitFinishedHandler │
                    │                   │ Dequeue next state  │
                    │                   │ StartState(next)    │
                    │                   └─────────────────────┘
                    │                               │
                    └───────────────┬───────────────┘
                                    ▼
                    ┌─────────────────────────────────┐
                    │       new.PerformEnter()         │
                    └─────────────────────────────────┘
                                    │
                                    ▼
                    ┌─────────────────────────────────┐
                    │     EnterFinishedHandler()       │
                    │   AppStateEnterSignal.Dispatch   │
                    └─────────────────────────────────┘
```

### Global State Signals

Listen to state changes from anywhere:

```csharp
public class AnalyticsMediator : Mediator
{
    [Inject] public AppStateEnterSignal AppStateEnterSignal { get; set; }
    [Inject] public AppStateExitSignal AppStateExitSignal { get; set; }

    public override void OnRegister()
    {
        AppStateEnterSignal.AddListener(OnStateEntered);
        AppStateExitSignal.AddListener(OnStateExited);
    }

    private void OnStateEntered(Type stateType)
    {
        Analytics.TrackScreen(stateType.Name);
    }

    private void OnStateExited(Type stateType)
    {
        Analytics.TrackScreenExit(stateType.Name);
    }
}
```

---

## 4. AppStateScreen

### Screen as Visual Representation

While `AppState` handles logic, `AppStateScreen` handles the **visual layer**:

```csharp
public class MenuScreen : AppStateScreen
{
    private MenuView _menuView;

    protected override void Enter()
    {
        _menuView = InstantiateView<MenuView>();
        _menuView.ShowWithAnimation();
    }

    protected override void Exit()
    {
        _menuView.HideWithAnimation();
    }
}
```

### Key Responsibilities

| AppState | AppStateScreen |
|----------|----------------|
| Business logic | UI instantiation |
| Signal handling | View animations |
| Data management | Visual transitions |
| Game rules | Asset loading |

### View Instantiation Methods

`AppStateScreen` provides helpers for creating views:

```csharp
// Synchronous instantiation
protected GameObject InstantiateView(string path, Transform parent = null)
protected T InstantiateView<T>(string path, Transform parent = null)

// Async instantiation (for Addressables)
protected async Task<GameObject> InstantiateViewAsync(string path, Transform parent = null)
protected async Task<T> InstantiateViewAsync<T>(string path, Transform parent = null)

// 3D game objects
protected T InstantiateGamePrefab<T>(string path, Transform parent = null)
protected async Task<T> InstantiateGamePrefabAsync<T>(string path, Transform parent = null)
```

**Automatic cleanup:** All instantiated views are tracked and destroyed when the screen exits.

### PerformEnter/PerformExit Flow

```csharp
public void PerformEnter()
{
    TransitionType = TransitionType.Enter;
    Enter();                          // ← Your code runs here
    TransitionType = TransitionType.None;
    EnterFinishedSignal.Dispatch();
}

public void PerformExit()
{
    TransitionType = TransitionType.Exit;
    Exit();                           // ← Your cleanup code
    TransitionType = TransitionType.None;
    ExitFinishedSignal.Dispatch();
}
```

### Async Transitions

For async operations (loading assets, animations), override the signals:

```csharp
public class LoadingScreen : AppStateScreen
{
    private LoadingView _loadingView;

    protected override async void Enter()
    {
        _loadingView = await InstantiateViewAsync<LoadingView>();
        await _loadingView.PlayEnterAnimation();
        
        // Signal enter is complete after async work
        EnterFinishedSignal.Dispatch();
    }
}
```

> ⚠️ **Note:** The default `PerformEnter()` dispatches `EnterFinishedSignal` immediately after `Enter()`. For async Enter, you'd need to override the whole `PerformEnter()` or structure your async work carefully.

---

## 5. Practical Example

Let's build a complete game flow:

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ SplashState │────▶│  MenuState  │────▶│  GameState  │────▶│GameOverState│
│             │     │             │◀────│             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                           │                                       │
                           └───────────────────────────────────────┘
```

### SplashState

```csharp
public class SplashScreen : AppStateScreen
{
    private SplashView _view;

    protected override async void Enter()
    {
        _view = await InstantiateViewAsync<SplashView>();
        await _view.PlayLogoAnimation();
        await Task.Delay(1000);  // Hold for a second
    }
}

public class SplashState : AppState<SplashScreen>
{
    [Inject] public IAppFsm Fsm { get; set; }
    [Inject] public IConfigLoader ConfigLoader { get; set; }

    protected override async void Enter()
    {
        // Load initial config while splash shows
        await ConfigLoader.LoadGameConfig();
        
        // Splash done, go to menu
        Fsm.SwitchState(new MenuState());
    }
}
```

### MenuState

```csharp
public class MenuScreen : AppStateScreen
{
    public Signal PlayClicked = new Signal();
    public Signal SettingsClicked = new Signal();

    private MenuView _view;

    protected override void Enter()
    {
        _view = InstantiateView<MenuView>();
        _view.PlayButton.onClick.AddListener(() => PlayClicked.Dispatch());
        _view.SettingsButton.onClick.AddListener(() => SettingsClicked.Dispatch());
    }

    protected override void Exit()
    {
        _view.PlayButton.onClick.RemoveAllListeners();
        _view.SettingsButton.onClick.RemoveAllListeners();
    }
}

public class MenuState : AppState<MenuScreen>
{
    [Inject] public IAppFsm Fsm { get; set; }

    protected override void RegisterHandlers()
    {
        Screen.PlayClicked.AddListener(OnPlayClicked);
        Screen.SettingsClicked.AddListener(OnSettingsClicked);
    }

    protected override void UnregisterHandlers()
    {
        Screen.PlayClicked.RemoveListener(OnPlayClicked);
        Screen.SettingsClicked.RemoveListener(OnSettingsClicked);
    }

    private void OnPlayClicked()
    {
        Fsm.SwitchState(new GameState());
    }

    private void OnSettingsClicked()
    {
        // Could use popup system instead of full state
        Debug.Log("Open settings popup");
    }
}
```

### GameState

```csharp
public class GameScreen : AppStateScreen
{
    public Signal<int> ScoreChanged = new Signal<int>();
    public Signal PauseClicked = new Signal();
    public Signal GameEnded = new Signal();

    private GameHudView _hud;
    private GameWorldView _world;

    protected override async void Enter()
    {
        _hud = await InstantiateViewAsync<GameHudView>();
        _world = await InstantiateGamePrefabAsync<GameWorldView>("Levels/Level1");
    }

    public void UpdateScore(int score)
    {
        _hud.SetScore(score);
    }

    public void TriggerGameEnd(bool won)
    {
        GameEnded.Dispatch();
    }
}

public class GameState : AppState<GameScreen>
{
    [Inject] public IAppFsm Fsm { get; set; }
    [Inject] public IGameManager GameManager { get; set; }

    private int _score;

    protected override void Enter()
    {
        GameManager.StartGame();
        _score = 0;
        Screen.UpdateScore(_score);
    }

    protected override void Exit()
    {
        GameManager.EndGame();
    }

    protected override void RegisterHandlers()
    {
        Screen.PauseClicked.AddListener(OnPauseClicked);
        Screen.GameEnded.AddListener(OnGameEnded);
        GameManager.EnemyDefeated.AddListener(OnEnemyDefeated);
    }

    protected override void UnregisterHandlers()
    {
        Screen.PauseClicked.RemoveListener(OnPauseClicked);
        Screen.GameEnded.RemoveListener(OnGameEnded);
        GameManager.EnemyDefeated.RemoveListener(OnEnemyDefeated);
    }

    private void OnEnemyDefeated(int points)
    {
        _score += points;
        Screen.UpdateScore(_score);
    }

    private void OnPauseClicked()
    {
        GameManager.TogglePause();
    }

    private void OnGameEnded()
    {
        Fsm.SwitchState(new GameOverState(_score));
    }
}
```

### GameOverState

```csharp
public class GameOverScreen : AppStateScreen
{
    public Signal RetryClicked = new Signal();
    public Signal MenuClicked = new Signal();

    private GameOverView _view;

    protected override void Enter()
    {
        _view = InstantiateView<GameOverView>();
        _view.RetryButton.onClick.AddListener(() => RetryClicked.Dispatch());
        _view.MenuButton.onClick.AddListener(() => MenuClicked.Dispatch());
    }

    public void SetFinalScore(int score)
    {
        _view.SetScore(score);
    }
}

public class GameOverState : AppState<GameOverScreen>
{
    [Inject] public IAppFsm Fsm { get; set; }
    [Inject] public ILeaderboardManager Leaderboard { get; set; }

    private readonly int _finalScore;

    public GameOverState(int finalScore)
    {
        _finalScore = finalScore;
    }

    protected override void Enter()
    {
        Screen.SetFinalScore(_finalScore);
        Leaderboard.SubmitScore(_finalScore);
    }

    protected override void RegisterHandlers()
    {
        Screen.RetryClicked.AddListener(OnRetryClicked);
        Screen.MenuClicked.AddListener(OnMenuClicked);
    }

    protected override void UnregisterHandlers()
    {
        Screen.RetryClicked.RemoveListener(OnRetryClicked);
        Screen.MenuClicked.RemoveListener(OnMenuClicked);
    }

    private void OnRetryClicked()
    {
        Fsm.SwitchState(new GameState());
    }

    private void OnMenuClicked()
    {
        Fsm.SwitchState(new MenuState());
    }
}
```

### Bootstrapping the FSM

In your game context:

```csharp
public class GameContext : MVCSContext
{
    protected override void mapBindings()
    {
        // Bind the FSM as singleton
        injectionBinder.Bind<IAppFsm>().To<AppFsm>().ToSingleton();
        
        // Bind state signals
        injectionBinder.Bind<AppStateEnterSignal>().ToSingleton();
        injectionBinder.Bind<AppStateExitSignal>().ToSingleton();

        // Bind screens (they get injected into states)
        injectionBinder.Bind<SplashScreen>().To<SplashScreen>();
        injectionBinder.Bind<MenuScreen>().To<MenuScreen>();
        injectionBinder.Bind<GameScreen>().To<GameScreen>();
        injectionBinder.Bind<GameOverScreen>().To<GameOverScreen>();
    }

    public override void Launch()
    {
        var fsm = injectionBinder.GetInstance<IAppFsm>();
        fsm.SwitchState(new SplashState());
    }
}
```

---

## Summary

The Screen FSM provides:

| Feature | Benefit |
|---------|---------|
| **Single state at a time** | Clear application state |
| **Paired State + Screen** | Separation of logic and visuals |
| **Automatic lifecycle** | Guaranteed setup/cleanup |
| **Queued transitions** | No race conditions |
| **Global signals** | Cross-cutting concerns (analytics, audio) |
| **View management** | Automatic cleanup of instantiated views |

**When to use Screen FSM:**
- Major application screens (Menu, Game, Settings)
- Flows that need clean transitions
- States with distinct visual representations

**When NOT to use:**
- Popups/dialogs (use Popup system instead)
- In-game state (player alive/dead) — use separate game FSM
- Temporary UI overlays

---

## Next Steps

- **Chapter 7:** Asset Management — loading, caching, and releasing assets
- **Chapter 8:** Audio System — sound effects and music management
