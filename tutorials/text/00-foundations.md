# Chapter 0: Foundations

> *Before you write a single line of code, understand why Framewerk exists.*

This chapter explains the conceptual architecture of Framewerk. No coding here—just mental models that will make everything click once you start building.

---

## What is Framewerk?

Framewerk is an **MVCS (Model-View-Controller-Service) architecture framework** for Unity, built on top of StrangeIoC. It provides structure, separation of concerns, and dependency injection for scalable game development.

### The Problem with "Typical" Unity Development

In vanilla Unity, you end up with:

```
┌─────────────────────────────────────────────────────────────────┐
│                     THE SPAGHETTI PROBLEM                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   PlayerController ←──────→ GameManager ←──────→ UIManager     │
│         ↑                        ↑                    ↑         │
│         │                        │                    │         │
│         ↓                        ↓                    ↓         │
│   EnemySpawner ←──────→ ScoreManager ←──────→ AudioManager     │
│         ↑                        ↑                    ↑         │
│         │                        │                    │         │
│         └────────────────────────┴────────────────────┘         │
│                                                                 │
│   Everyone references everyone. GetComponent everywhere.       │
│   Singletons for days. Testing? Good luck.                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**Common symptoms:**
- `FindObjectOfType<>()` calls scattered everywhere
- Massive god-classes that "manage" everything
- Singletons that create hidden dependencies
- UI code mixed with game logic
- Changes in one place break things elsewhere
- Unit testing is nearly impossible

### The Framewerk Solution

Framewerk enforces separation through **architecture patterns**:

```
┌─────────────────────────────────────────────────────────────────┐
│                     THE FRAMEWERK WAY                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────┐                              ┌─────────┐         │
│   │  VIEW   │  (Unity MonoBehaviour)       │  MODEL  │         │
│   │ Button, │                              │  Data,  │         │
│   │ Sprite  │                              │  State  │         │
│   └────┬────┘                              └────┬────┘         │
│        │                                        │               │
│        ↓                                        │               │
│   ┌─────────┐      ┌─────────┐                 │               │
│   │MEDIATOR │ ───→ │ SIGNAL  │ ───→ ┌─────────┐│               │
│   │ Logic,  │      │ (Event) │      │ COMMAND ││               │
│   │ Binding │ ←─── └─────────┘ ←─── │ Handler │←┘               │
│   └─────────┘                       └─────────┘                │
│                                                                 │
│   Everything flows through defined channels.                    │
│   Dependencies are injected, not hunted.                        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**The MVCS breakdown:**
- **Model** — Data and state (no Unity dependencies)
- **View** — MonoBehaviours that display things (dumb, no logic)
- **Controller** — Commands that handle actions (single-responsibility)
- **Service** — External communication (APIs, databases, etc.)

---

## Dependency Injection Explained

### What is Dependency Injection?

Instead of a class *finding* or *creating* its dependencies, they're **given** to it from outside.

**Without DI (the bad way):**
```csharp
public class PlayerMediator : MonoBehaviour
{
    private ScoreManager scoreManager;
    
    void Start()
    {
        // Hunting for dependencies
        scoreManager = FindObjectOfType<ScoreManager>();
        // or worse:
        scoreManager = ScoreManager.Instance;
    }
}
```

**With DI (the Framewerk way):**
```csharp
public class PlayerMediator : Mediator
{
    [Inject] public IScoreModel ScoreModel { get; set; }
    // Just declare what you need. It appears. Magic. ✨
    
    public override void OnRegister()
    {
        // ScoreModel is already here, ready to use
    }
}
```

### Why Does This Matter?

1. **Testability** — Inject mock objects for unit tests
2. **Flexibility** — Swap implementations without changing code
3. **Clarity** — Dependencies are explicit, not hidden
4. **Decoupling** — Classes don't know *where* dependencies come from

### The Dependency Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    DEPENDENCY INJECTION FLOW                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│                      ┌──────────────┐                          │
│                      │   CONTEXT    │                          │
│                      │  (The Hub)   │                          │
│                      └──────┬───────┘                          │
│                             │                                   │
│              ┌──────────────┼──────────────┐                   │
│              │              │              │                   │
│              ↓              ↓              ↓                   │
│     ┌────────────┐  ┌────────────┐  ┌────────────┐            │
│     │ Injection  │  │ Mediation  │  │  Command   │            │
│     │   Binder   │  │   Binder   │  │   Binder   │            │
│     └─────┬──────┘  └─────┬──────┘  └─────┬──────┘            │
│           │               │               │                    │
│           ↓               ↓               ↓                    │
│      Binds types      Binds Views     Binds Signals            │
│      to instances     to Mediators    to Commands              │
│                                                                 │
│                                                                 │
│   When a class is created, the injector:                       │
│   1. Scans for [Inject] attributes                             │
│   2. Looks up bindings                                         │
│   3. Provides instances automatically                          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## The Context

The **Context** is the wiring hub—the central nervous system of your Framewerk application. It's where you define all bindings.

### What Happens in a Context

```csharp
public class GameContext : FramewerkMVCSContext
{
    protected override void mapBindings()
    {
        base.mapBindings();
        
        // 1. INJECTION BINDINGS — "When someone needs X, give them Y"
        injectionBinder.Bind<IScoreModel>().To<ScoreModel>().ToSingleton();
        injectionBinder.Bind<IPlayerData>().To<PlayerData>();
        
        // 2. MEDIATION BINDINGS — "This View gets this Mediator"
        mediationBinder.Bind<PlayerView>().To<PlayerMediator>();
        mediationBinder.Bind<ScoreView>().To<ScoreMediator>();
        
        // 3. SIGNAL BINDINGS — "This Signal is available for injection"
        injectionBinder.Bind<PlayerDiedSignal>().ToSingleton();
        injectionBinder.Bind<ScoreChangedSignal>().ToSingleton();
        
        // 4. COMMAND BINDINGS — "When this Signal fires, run this Command"
        commandBinder.Bind<PlayerDiedSignal>().To<HandlePlayerDeathCommand>();
        commandBinder.Bind<ContextStartSignal>().To<GameStartCommand>();
    }
}
```

### Binding Types Explained

| Binding | What it does |
|---------|--------------|
| `.To<T>()` | Create a new instance each time |
| `.ToSingleton()` | Create once, share everywhere |
| `.ToValue(instance)` | Use this exact instance |

### The [Inject] Attribute

Mark properties with `[Inject]` to receive dependencies:

```csharp
public class ScoreMediator : Mediator
{
    // Interface injection (recommended)
    [Inject] public IScoreModel ScoreModel { get; set; }
    
    // Signal injection
    [Inject] public ScoreChangedSignal ScoreChanged { get; set; }
    
    // Named injection (for multiple implementations)
    [Inject("Player1")] public IPlayerData Player1Data { get; set; }
    [Inject("Player2")] public IPlayerData Player2Data { get; set; }
}
```

### Context Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                      CONTEXT LIFECYCLE                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   1. Bootstrap Awake()                                         │
│         │                                                       │
│         ↓                                                       │
│   2. Context Created                                           │
│         │                                                       │
│         ↓                                                       │
│   3. mapBindings() ←── You define your bindings here           │
│         │                                                       │
│         ↓                                                       │
│   4. Views Mediated (Mediators created & injected)             │
│         │                                                       │
│         ↓                                                       │
│   5. Launch() → ContextStartSignal dispatched                  │
│         │                                                       │
│         ↓                                                       │
│   6. Start Command runs                                        │
│         │                                                       │
│         ↓                                                       │
│   7. Application running...                                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Signals & Commands

### Signals: Type-Safe Events

Signals are **strongly-typed events**. They replace string-based events and magic method calls.

```csharp
// Define a signal with a payload
public class ItemClickedSignal : Signal<ItemData> { }

// Define a signal without payload
public class GamePausedSignal : Signal { }

// Define a signal with multiple parameters
public class DamageDealtSignal : Signal<IEnemy, int, DamageType> { }
```

**Dispatching signals:**
```csharp
[Inject] public ItemClickedSignal ItemClicked { get; set; }

// Fire the signal
ItemClicked.Dispatch(itemData);
```

**Listening to signals:**
```csharp
[Inject] public ItemClickedSignal ItemClicked { get; set; }

public override void OnRegister()
{
    ItemClicked.AddListener(OnItemClicked);
}

public override void OnRemove()
{
    ItemClicked.RemoveListener(OnItemClicked);
}

private void OnItemClicked(ItemData data)
{
    // Handle the event
}
```

**Or use the [ListensTo] attribute (cleaner):**
```csharp
[ListensTo(typeof(ItemClickedSignal))]
public void OnItemClicked(ItemData data)
{
    // Automatically subscribed on OnRegister, unsubscribed on OnRemove
}
```

### Commands: Single-Responsibility Handlers

Commands are **one-shot action handlers**. They execute when a signal fires, then disappear.

```csharp
public class ShowItemPopupCommand : Command
{
    // Dependencies injected automatically
    [Inject] public IPopupManager PopupManager { get; set; }
    
    // Signal payload injected as well!
    [Inject] public ItemData ItemData { get; set; }
    
    public override void Execute()
    {
        PopupManager.ShowPopup<ItemPopup>(ItemData);
    }
}
```

**Binding signals to commands:**
```csharp
// In your Context:
commandBinder.Bind<ItemClickedSignal>().To<ShowItemPopupCommand>();

// Chain multiple commands
commandBinder.Bind<PlayerDiedSignal>()
    .To<PlayDeathAnimationCommand>()
    .To<UpdateLeaderboardCommand>()
    .To<ShowGameOverCommand>();

// Run once then unbind
commandBinder.Bind<ContextStartSignal>().To<InitializeGameCommand>().Once();
```

### Signal → Command Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    SIGNAL → COMMAND FLOW                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌──────────┐        ┌─────────────┐        ┌──────────────┐  │
│   │ MEDIATOR │        │   SIGNAL    │        │   COMMAND    │  │
│   │          │───────→│             │───────→│              │  │
│   │ User     │ fires  │ ItemClicked │triggers│ ShowPopup    │  │
│   │ clicked  │        │   Signal    │        │   Command    │  │
│   └──────────┘        └─────────────┘        └──────────────┘  │
│                                                     │           │
│                              ┌──────────────────────┘           │
│                              ↓                                  │
│                       ┌──────────────┐                         │
│                       │    MODEL     │                         │
│                       │              │                         │
│                       │ Update state │                         │
│                       │ Fire signal  │                         │
│                       └──────┬───────┘                         │
│                              │                                  │
│                              ↓                                  │
│                       ┌─────────────┐        ┌──────────────┐  │
│                       │   SIGNAL    │        │   MEDIATOR   │  │
│                       │             │───────→│              │  │
│                       │ StateChanged│updates │ Update View  │  │
│                       └─────────────┘        └──────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## The Mediation Pattern

### View vs Mediator: Why Separate?

The **View** is a dumb MonoBehaviour. It knows how to display things but nothing about the application.

The **Mediator** is the brain. It receives injections, listens to signals, and tells the View what to do.

```
┌─────────────────────────────────────────────────────────────────┐
│                   VIEW / MEDIATOR SEPARATION                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐  │
│   │                        VIEW                              │  │
│   │  • MonoBehaviour                                         │  │
│   │  • References to UI elements (Button, Text, Image)       │  │
│   │  • Animation triggers                                    │  │
│   │  • NO business logic                                     │  │
│   │  • NO knowledge of signals, models, or other Views       │  │
│   │  • Exposes events: OnButtonClicked, OnSliderChanged      │  │
│   └─────────────────────────────────────────────────────────┘  │
│                             ↕ bridge                            │
│   ┌─────────────────────────────────────────────────────────┐  │
│   │                       MEDIATOR                           │  │
│   │  • Pure C# class (created by framework)                  │  │
│   │  • Receives [Inject] dependencies                        │  │
│   │  • Listens to signals from the application               │  │
│   │  • Listens to events from the View                       │  │
│   │  • Translates between application and View               │  │
│   │  • Dispatches signals when user acts                     │  │
│   └─────────────────────────────────────────────────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Example: Score Display

**The View (dumb):**
```csharp
public class ScoreView : View
{
    [SerializeField] private Text scoreText;
    
    public void SetScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }
}
```

**The Mediator (smart):**
```csharp
public class ScoreMediator : Mediator<ScoreView>
{
    [Inject] public IScoreModel ScoreModel { get; set; }
    
    public override void OnRegister()
    {
        // Initialize view with current state
        View.SetScore(ScoreModel.CurrentScore);
    }
    
    [ListensTo(typeof(ScoreChangedSignal))]
    public void OnScoreChanged(int newScore)
    {
        View.SetScore(newScore);
    }
}
```

### Why This Matters

1. **Views are reusable** — Same View, different Mediator for different contexts
2. **Views are testable** — No dependencies to mock
3. **Views are designer-friendly** — Artists edit prefabs without touching logic
4. **Mediators are testable** — Mock the View interface
5. **Clean separation** — Display logic never mixes with application logic

---

## How It All Fits Together

### The Complete Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         FRAMEWERK ARCHITECTURE                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ╔═══════════════════════════════════════════════════════════════════════╗ │
│  ║                           CONTEXT                                      ║ │
│  ║                    (The Wiring Hub)                                    ║ │
│  ║                                                                        ║ │
│  ║   ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐         ║ │
│  ║   │ InjectionBinder │ │ MediationBinder │ │  CommandBinder  │         ║ │
│  ║   │                 │ │                 │ │                 │         ║ │
│  ║   │ Type → Instance │ │ View → Mediator │ │ Signal → Command│         ║ │
│  ║   └────────┬────────┘ └────────┬────────┘ └────────┬────────┘         ║ │
│  ╚════════════╪══════════════════╪══════════════════╪════════════════════╝ │
│               │                  │                  │                       │
│               ↓                  ↓                  ↓                       │
│                                                                             │
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐      │
│  │      MODEL       │    │      VIEW        │    │     COMMAND      │      │
│  │                  │    │                  │    │                  │      │
│  │  • Data/State    │    │  • MonoBehaviour │    │  • Action Handler│      │
│  │  • Business Rules│    │  • Display only  │    │  • Single task   │      │
│  │  • Fire Signals  │    │  • No logic      │    │  • Gets injected │      │
│  └────────┬─────────┘    └────────┬─────────┘    └────────┬─────────┘      │
│           │                       │                       │                 │
│           │              ┌────────┴────────┐              │                 │
│           │              │    MEDIATOR     │              │                 │
│           │              │                 │              │                 │
│           │              │ • Bridge V↔App  │              │                 │
│           │              │ • [Inject] deps │              │                 │
│           └─────────────→│ • Listen signals│←─────────────┘                 │
│                          │ • Dispatch acts │                                │
│                          └─────────────────┘                                │
│                                                                             │
│  ┌──────────────────┐                          ┌──────────────────┐        │
│  │     SERVICE      │←────── [Inject] ────────→│     SIGNALS      │        │
│  │                  │                          │                  │        │
│  │  • External APIs │                          │  • Type-safe     │        │
│  │  • Persistence   │                          │  • Event bus     │        │
│  │  • Network calls │                          │  • Loose coupling│        │
│  └──────────────────┘                          └──────────────────┘        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### A Typical Request Flow

Let's trace what happens when a player clicks an item:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        REQUEST FLOW EXAMPLE                                 │
│                     "Player Clicks Inventory Item"                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. USER CLICKS BUTTON                                                      │
│     ┌──────────────┐                                                        │
│     │   ItemView   │  →  OnClick() fires                                   │
│     └──────┬───────┘                                                        │
│            ↓                                                                │
│  2. VIEW NOTIFIES MEDIATOR                                                  │
│     ┌──────────────┐                                                        │
│     │ItemMediator  │  →  Receives click event from View                    │
│     └──────┬───────┘                                                        │
│            ↓                                                                │
│  3. MEDIATOR DISPATCHES SIGNAL                                              │
│     ┌──────────────┐                                                        │
│     │ItemClicked   │  →  Signal.Dispatch(itemData)                         │
│     │   Signal     │                                                        │
│     └──────┬───────┘                                                        │
│            ↓                                                                │
│  4. COMMAND BINDER TRIGGERS COMMAND                                         │
│     ┌──────────────┐                                                        │
│     │ShowItemPopup │  →  Command created, injected, executed               │
│     │   Command    │                                                        │
│     └──────┬───────┘                                                        │
│            ↓                                                                │
│  5. COMMAND UPDATES MODEL / CALLS SERVICE                                   │
│     ┌──────────────┐                                                        │
│     │ PopupManager │  →  Shows popup, updates state                        │
│     └──────┬───────┘                                                        │
│            ↓                                                                │
│  6. MODEL FIRES STATE CHANGE SIGNAL                                         │
│     ┌──────────────┐                                                        │
│     │ PopupOpened  │  →  Signal.Dispatch(popupData)                        │
│     │   Signal     │                                                        │
│     └──────┬───────┘                                                        │
│            ↓                                                                │
│  7. MEDIATORS LISTENING UPDATE THEIR VIEWS                                  │
│     ┌──────────────┐                                                        │
│     │PopupMediator │  →  View.Show(data)                                   │
│     └──────────────┘                                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Key Takeaways

| Concept | Remember |
|---------|----------|
| **Context** | The central hub where all bindings live |
| **Dependency Injection** | Ask for what you need, don't hunt for it |
| **Signals** | Type-safe events that decouple communication |
| **Commands** | Single-purpose handlers triggered by signals |
| **Views** | Dumb display objects, no business logic |
| **Mediators** | Smart bridges between Views and the application |
| **Models** | Data and state, no Unity dependencies |
| **Services** | External communication (APIs, persistence) |

---

## What's Next?

Now that you understand the architecture, you're ready to build. In **Chapter 1: Your First Framewerk Project**, we'll create a complete mini-application from scratch.

You'll learn:
- Setting up a Context
- Creating your first View and Mediator
- Wiring signals and commands
- Building a simple interactive feature

See you there! 🚀
