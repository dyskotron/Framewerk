# Chapter 2: Views & Mediation

The View/Mediator pattern is the heart of Framewerk's UI architecture. It keeps your Unity components clean while giving you full access to the application's signals, services, and models.

---

## 2.1 View Basics

### What Is a View?

A **View** is a MonoBehaviour that represents a piece of UI or a visual component in your scene. It holds serialized references to Unity components (buttons, text, images) but **knows nothing about the application's business logic**.

```
┌─────────────────────────────────────────────┐
│                    View                     │
│  ┌─────────┐  ┌─────────┐  ┌─────────────┐  │
│  │ Button  │  │  Text   │  │   Image     │  │
│  └─────────┘  └─────────┘  └─────────────┘  │
│        (Serialized Unity References)        │
└─────────────────────────────────────────────┘
```

### Extending the View Base Class

All Views inherit from `View`:

```csharp
using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHudView : View
{
    // Serialized references — drag in the Inspector
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Image _healthBar;
    
    // Public accessors for the Mediator
    public TextMeshProUGUI HealthText => _healthText;
    public TextMeshProUGUI ScoreText => _scoreText;
    public Button PauseButton => _pauseButton;
    public Image HealthBar => _healthBar;
}
```

**Key points:**
- Extend `View` (not raw `MonoBehaviour`)
- Mark Unity references with `[SerializeField]`
- Keep fields `private`, expose via properties
- **No logic** — just references and simple display methods

### Auto-Registration

Views automatically register with the Context when they `Awake()` or `Start()`. The framework walks up the Transform hierarchy looking for a `ContextView`, then triggers mediation.

```
Scene Hierarchy              Registration Flow
─────────────────────        ──────────────────────────────
ContextRoot (ContextView)    
  └── Canvas                 
       └── PlayerHUD         View.Awake() → bubbleToContext()
            └── HealthBar                  → Context.AddView()
                                           → MediationBinder.Trigger()
                                           → Mediator created + injected
```

You don't call anything — just add the View to your scene under the ContextView, and mediation happens automatically.

### Optional: Disable Auto-Registration

For Views that shouldn't register immediately:

```csharp
public class PooledItemView : View
{
    protected override void Awake()
    {
        autoRegisterWithContext = false; // Disable auto-registration
        base.Awake();
    }
    
    public void ManuallyRegister()
    {
        autoRegisterWithContext = true;
        // Re-trigger registration
    }
}
```

---

## 2.2 Mediator Basics

### What Is a Mediator?

A **Mediator** is the bridge between a View and the application. It:
- Receives the View via injection
- Listens to signals from the application
- Updates the View in response
- Dispatches signals when the user interacts

```
┌─────────────────────────────────────────────────────────────┐
│                      Application Layer                       │
│   [Signals]   [Commands]   [Models]   [Services]            │
└──────────────────────────┬──────────────────────────────────┘
                           │ inject
                           ▼
                   ┌───────────────┐
                   │   Mediator    │  ← Listens to signals
                   │               │  ← Updates View
                   │   View ──────►│  ← Handles UI events
                   └───────────────┘
                           │
                   ┌───────┴───────┐
                   │     View      │
                   │  (Unity UI)   │
                   └───────────────┘
```

### ExtendedMediator<T>

Framewerk provides `ExtendedMediator<T>` — a generic base class that:
1. Auto-injects the View
2. Provides UI event helpers (buttons, sliders, toggles)
3. Auto-cleans up listeners on removal

```csharp
using Framewerk.UI;

public class PlayerHudMediator : ExtendedMediator<PlayerHudView>
{
    // View is auto-injected — no [Inject] needed!
    // Access it via: View.HealthText, View.PauseButton, etc.
    
    [Inject] public PlayerModel PlayerModel { get; set; }
    [Inject] public PauseGameSignal PauseGameSignal { get; set; }
    
    public override void OnRegister()
    {
        base.OnRegister();
        
        // Subscribe to model changes
        PlayerModel.HealthChanged += UpdateHealthDisplay;
        PlayerModel.ScoreChanged += UpdateScoreDisplay;
        
        // Wire up button using helper
        AddButtonListener(View.PauseButton, OnPauseClicked);
        
        // Initial display
        UpdateHealthDisplay(PlayerModel.Health);
        UpdateScoreDisplay(PlayerModel.Score);
    }
    
    public override void OnRemove()
    {
        // Unsubscribe from model
        PlayerModel.HealthChanged -= UpdateHealthDisplay;
        PlayerModel.ScoreChanged -= UpdateScoreDisplay;
        
        // Button listeners cleaned up automatically by base.OnRemove()
        base.OnRemove();
    }
    
    private void UpdateHealthDisplay(int health)
    {
        View.HealthText.text = $"HP: {health}";
        View.HealthBar.fillAmount = health / 100f;
    }
    
    private void UpdateScoreDisplay(int score)
    {
        View.ScoreText.text = $"Score: {score}";
    }
    
    private void OnPauseClicked()
    {
        PauseGameSignal.Dispatch();
    }
}
```

### The OnRegister/OnRemove Lifecycle

| Method | When It Fires | What to Do |
|--------|---------------|------------|
| `PreRegister()` | Before injection | Rarely used |
| `OnRegister()` | After all injections complete | Set up listeners, init display |
| `OnEnabled()` | View enabled | Resume updates |
| `OnDisabled()` | View disabled | Pause updates |
| `OnRemove()` | View destroyed | Clean up listeners, references |

### Binding Views to Mediators

In your Context's `mapBindings()`:

```csharp
protected override void mapBindings()
{
    base.mapBindings();
    
    // Map View to Mediator
    mediationBinder.Bind<PlayerHudView>().To<PlayerHudMediator>();
    mediationBinder.Bind<InventoryView>().To<InventoryMediator>();
    mediationBinder.Bind<SettingsView>().To<SettingsMediator>();
}
```

When a `PlayerHudView` appears in the scene, the framework automatically:
1. Creates a `PlayerHudMediator` (adds as component to same GameObject)
2. Injects all dependencies
3. Injects the View
4. Calls `OnRegister()`

---

## 2.3 UI Event Helpers

### Why Use Helpers Over Manual Listeners?

**Manual approach (error-prone):**
```csharp
// Easy to forget cleanup!
View.Button.onClick.AddListener(OnClick);
// If you forget RemoveListener, you get:
// - Memory leaks
// - Duplicate handlers after re-enabling
// - Null reference exceptions
```

**ExtendedMediator helpers (safe):**
```csharp
AddButtonListener(View.Button, OnClick);
// Automatically:
// - Removes previous listeners
// - Tracks for cleanup
// - Cleans up in OnRemove()
```

### Available Helpers

| Helper | Component | Callback Signature |
|--------|-----------|-------------------|
| `AddButtonListener` | `Button` | `Action` |
| `AddToggleListener` | `Toggle` | `Action<bool>` |
| `AddSliderListener` | `Slider` | `Action<float>` |
| `AddInputListener` | `InputField` | `Action<string>` |
| `AddPointerListener` | `PointerElement` | `Action<bool, Vector2>` |
| `AddDragListener` | `DragElement` | `Action<DragData>` |

### Examples

```csharp
public override void OnRegister()
{
    base.OnRegister();
    
    // Button — fires on click
    AddButtonListener(View.ConfirmButton, OnConfirmClicked);
    
    // Toggle — fires when checked/unchecked
    AddToggleListener(View.MuteToggle, OnMuteChanged);
    
    // Slider — fires as value changes
    AddSliderListener(View.VolumeSlider, OnVolumeChanged);
    
    // InputField — fires on end edit (user presses Enter or loses focus)
    AddInputListener(View.NameInput, OnNameEntered);
}

private void OnConfirmClicked()
{
    ConfirmSignal.Dispatch();
}

private void OnMuteChanged(bool isMuted)
{
    AudioModel.IsMuted = isMuted;
}

private void OnVolumeChanged(float volume)
{
    AudioModel.Volume = volume;
}

private void OnNameEntered(string name)
{
    PlayerModel.Name = name;
}
```

### Manual Cleanup (If Needed)

If you need to remove specific listeners before `OnRemove()`:

```csharp
RemoveButtonListeners();    // Clears all button listeners
RemoveToggleListeners();    // Clears all toggle listeners
RemoveSliderListeners();    // Clears all slider listeners
RemoveInputListeners();     // Clears all input listeners
RemovePointerListeners();   // Clears all pointer listeners
RemoveDragListeners();      // Clears all drag listeners
RemoveListeners();          // Clears ALL of the above
```

---

## 2.4 Signal-Based Mediation with [ListensTo]

### The Problem

Traditional signal listening is verbose:

```csharp
public class PlayerHudMediator : ExtendedMediator<PlayerHudView>
{
    [Inject] public PlayerDiedSignal PlayerDiedSignal { get; set; }
    [Inject] public LevelCompletedSignal LevelCompletedSignal { get; set; }
    [Inject] public PowerUpCollectedSignal PowerUpCollectedSignal { get; set; }
    
    public override void OnRegister()
    {
        base.OnRegister();
        PlayerDiedSignal.AddListener(OnPlayerDied);
        LevelCompletedSignal.AddListener(OnLevelCompleted);
        PowerUpCollectedSignal.AddListener(OnPowerUp);
    }
    
    public override void OnRemove()
    {
        PlayerDiedSignal.RemoveListener(OnPlayerDied);
        LevelCompletedSignal.RemoveListener(OnLevelCompleted);
        PowerUpCollectedSignal.RemoveListener(OnPowerUp);
        base.OnRemove();
    }
    
    private void OnPlayerDied() { /* ... */ }
    private void OnLevelCompleted(int stars) { /* ... */ }
    private void OnPowerUp(PowerUpType type) { /* ... */ }
}
```

Every signal requires:
1. `[Inject]` property
2. `AddListener()` in `OnRegister()`
3. `RemoveListener()` in `OnRemove()`

That's 3 places to maintain for each signal!

### The [ListensTo] Solution

The `[ListensTo]` attribute collapses this to a single declaration:

```csharp
public class PlayerHudMediator : ExtendedMediator<PlayerHudView>
{
    // No [Inject] needed for listened signals!
    
    public override void OnRegister()
    {
        base.OnRegister();
        // No AddListener calls needed!
    }
    
    // OnRemove() not even needed if you have no other cleanup!
    
    [ListensTo(typeof(PlayerDiedSignal))]
    public void OnPlayerDied()
    {
        View.ShowDeathOverlay();
    }
    
    [ListensTo(typeof(LevelCompletedSignal))]
    public void OnLevelCompleted(int stars)
    {
        View.ShowVictory(stars);
    }
    
    [ListensTo(typeof(PowerUpCollectedSignal))]
    public void OnPowerUp(PowerUpType type)
    {
        View.FlashPowerUpIcon(type);
    }
}
```

### How It Works

```
┌────────────────────────────────────────────────────────────┐
│                  SignalMediationBinder                      │
├────────────────────────────────────────────────────────────┤
│  On Mediator Creation:                                      │
│    1. Scan mediator class for [ListensTo] attributes       │
│    2. For each attribute:                                   │
│       - Get signal instance from injectionBinder           │
│       - Create delegate from marked method                 │
│       - Call signal.AddListener(delegate)                  │
│                                                             │
│  On Mediator Destruction:                                   │
│    1. Same scan                                             │
│    2. signal.RemoveListener(delegate) for each             │
└────────────────────────────────────────────────────────────┘
```

### ⚠️ Important: Methods Must Be Public!

```csharp
// ✅ Works — public method
[ListensTo(typeof(MySignal))]
public void OnMySignal() { }

// ❌ SILENTLY IGNORED — private method
[ListensTo(typeof(MySignal))]
private void OnMySignal() { }

// ❌ SILENTLY IGNORED — protected method
[ListensTo(typeof(MySignal))]
protected void OnMySignal() { }
```

The reflection system only scans **public** methods. Private/protected methods with `[ListensTo]` will be silently ignored — no error, just no subscription.

### When to Use [ListensTo]

| Use [ListensTo] When... | Use Manual When... |
|-------------------------|-------------------|
| Just reacting to signals | Need to dispatch the signal too |
| Read-only responses | Need conditional subscription |
| Simple UI updates | Need to pass the signal to children |
| Want cleaner code | Need reference to signal object |

### Complete Before/After Example

**Before (verbose):**
```csharp
public class GameHudMediator : ExtendedMediator<GameHudView>
{
    [Inject] public ScoreChangedSignal ScoreChangedSignal { get; set; }
    [Inject] public LivesChangedSignal LivesChangedSignal { get; set; }
    [Inject] public GameOverSignal GameOverSignal { get; set; }
    [Inject] public CoinCollectedSignal CoinCollectedSignal { get; set; }
    
    public override void OnRegister()
    {
        base.OnRegister();
        ScoreChangedSignal.AddListener(UpdateScore);
        LivesChangedSignal.AddListener(UpdateLives);
        GameOverSignal.AddListener(ShowGameOver);
        CoinCollectedSignal.AddListener(AnimateCoin);
    }
    
    public override void OnRemove()
    {
        ScoreChangedSignal.RemoveListener(UpdateScore);
        LivesChangedSignal.RemoveListener(UpdateLives);
        GameOverSignal.RemoveListener(ShowGameOver);
        CoinCollectedSignal.RemoveListener(AnimateCoin);
        base.OnRemove();
    }
    
    private void UpdateScore(int score) => View.ScoreText.text = score.ToString();
    private void UpdateLives(int lives) => View.LivesText.text = $"x{lives}";
    private void ShowGameOver() => View.GameOverPanel.SetActive(true);
    private void AnimateCoin() => View.CoinIcon.Play("collect");
}
```

**After (clean):**
```csharp
public class GameHudMediator : ExtendedMediator<GameHudView>
{
    [ListensTo(typeof(ScoreChangedSignal))]
    public void UpdateScore(int score) => View.ScoreText.text = score.ToString();
    
    [ListensTo(typeof(LivesChangedSignal))]
    public void UpdateLives(int lives) => View.LivesText.text = $"x{lives}";
    
    [ListensTo(typeof(GameOverSignal))]
    public void ShowGameOver() => View.GameOverPanel.SetActive(true);
    
    [ListensTo(typeof(CoinCollectedSignal))]
    public void AnimateCoin() => View.CoinIcon.Play("collect");
}
```

From 30+ lines to 12. Same functionality, zero boilerplate.

---

## Summary

| Concept | Purpose |
|---------|---------|
| **View** | MonoBehaviour holding Unity references. No logic. |
| **Mediator** | Bridge between View and app. Handles events, updates display. |
| **ExtendedMediator<T>** | Generic base with auto View injection + UI helpers |
| **UI Helpers** | `AddButtonListener`, `AddSliderListener`, etc. — safe event wiring |
| **[ListensTo]** | Attribute for zero-boilerplate signal subscription |

**Data Flow:**
```
Signal dispatched → [ListensTo] method → Update View
User clicks → UI Helper → Dispatch Signal → Command executes
```

**Next Chapter:** Commands & Signal Flow — how to handle those dispatched signals and execute business logic.
