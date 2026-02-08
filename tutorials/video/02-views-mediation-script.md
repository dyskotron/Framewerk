# Video Script: Chapter 2 — Views & Mediation

**Duration:** ~12-15 minutes  
**Style:** Screen recording + code walkthrough  
**Prerequisites:** Chapter 1 complete, working Framewerk project

---

## INTRO (0:00 - 0:45)

### [SCREEN: Title card "Chapter 2: Views & Mediation"]

**NARRATION:**
> "In chapter one, we set up a Framewerk project. Now we're diving into the pattern you'll use every day: Views and Mediators.
>
> This is how Framewerk handles UI. Your View is just a bucket of Unity references — buttons, text, images. Your Mediator is the brain that connects it to your game's logic.
>
> By the end of this video, you'll know how to create Views, wire up Mediators, handle button clicks safely, and use the ListensTo attribute to eliminate boilerplate. Let's go."

---

## SECTION 1: VIEW BASICS (0:45 - 3:30)

### [SCREEN: Unity Editor - Create new script PlayerHudView.cs]

**NARRATION:**
> "Let's create a simple HUD. I'll make a new script called PlayerHudView."

### [SCREEN: Code editor - Write the View class]

```csharp
using strange.extensions.mediation.impl;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHudView : View
{
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Button _pauseButton;
    
    public TextMeshProUGUI HealthText => _healthText;
    public TextMeshProUGUI ScoreText => _scoreText;
    public Button PauseButton => _pauseButton;
}
```

**NARRATION:**
> "Notice we extend View, not MonoBehaviour. This gives us auto-registration with the Context.
>
> I'm using SerializeField for the Unity references — keeps them private but visible in the Inspector. Then I expose them through properties.
>
> This View has zero logic. It's just a container. The Mediator will do all the work."

### [SCREEN: Unity Editor - Create Canvas with UI elements, attach View, wire references]

**NARRATION:**
> "In Unity, I'll set up a Canvas with a health text, score text, and pause button. Then I attach the View script and drag in the references.
>
> The key thing: this View must be somewhere under your ContextView in the hierarchy. When it Awakes, it walks up the tree looking for a Context to register with."

### [SCREEN: ASCII diagram animation showing registration flow]

```
ContextRoot (ContextView)
  └── Canvas
       └── PlayerHUD (PlayerHudView)  → Awake() → Finds Context → Registers
```

**NARRATION:**
> "That's auto-registration. You don't call anything. Just put your View in the scene, and Framewerk handles the rest."

---

## SECTION 2: MEDIATOR BASICS (3:30 - 7:00)

### [SCREEN: Create new script PlayerHudMediator.cs]

**NARRATION:**
> "Now the Mediator. This is where the magic happens."

### [SCREEN: Code editor - Write the Mediator class]

```csharp
using Framewerk.UI;

public class PlayerHudMediator : ExtendedMediator<PlayerHudView>
{
    public override void OnRegister()
    {
        base.OnRegister();
        Debug.Log("PlayerHudMediator registered!");
        Debug.Log($"Health text: {View.HealthText.text}");
    }
}
```

**NARRATION:**
> "I'm extending ExtendedMediator with a generic parameter — the View type. This automatically injects the View. I don't need to write an Inject attribute; I just access it through 'View' dot whatever.
>
> OnRegister fires after all injections complete. This is where you set up your listeners and initialize the display."

### [SCREEN: Context file - Add mediation binding]

```csharp
protected override void mapBindings()
{
    base.mapBindings();
    mediationBinder.Bind<PlayerHudView>().To<PlayerHudMediator>();
}
```

**NARRATION:**
> "In your Context, you bind the View to the Mediator. This tells Framewerk: whenever you see a PlayerHudView, create a PlayerHudMediator for it."

### [SCREEN: Unity Editor - Play mode, show console log]

**NARRATION:**
> "Let's run it... and there's our log. The Mediator was created and registered automatically. Notice it's added as a component on the same GameObject as the View."

### [SCREEN: Show Mediator lifecycle diagram]

```
PreRegister() → [Injections happen] → OnRegister() → [View lives] → OnRemove()
                                                   ↓
                                            OnEnabled() / OnDisabled()
```

**NARRATION:**
> "The lifecycle is simple. PreRegister before injections, OnRegister after. OnEnabled and OnDisabled when the View toggles. OnRemove when it's destroyed. That's where you clean up."

---

## SECTION 3: UI EVENT HELPERS (7:00 - 9:30)

### [SCREEN: Code showing the WRONG way first]

```csharp
// DON'T DO THIS
public override void OnRegister()
{
    View.PauseButton.onClick.AddListener(OnPauseClicked);
}
// Oops, forgot to remove it in OnRemove!
```

**NARRATION:**
> "Here's the trap. You add a listener manually, forget to remove it, and now you've got memory leaks and duplicate handlers. I've seen this bug in every Unity project."

### [SCREEN: Code showing the RIGHT way]

```csharp
// DO THIS INSTEAD
public override void OnRegister()
{
    base.OnRegister();
    AddButtonListener(View.PauseButton, OnPauseClicked);
}

private void OnPauseClicked()
{
    Debug.Log("Pause clicked!");
}
```

**NARRATION:**
> "ExtendedMediator gives you AddButtonListener. It removes previous listeners, tracks the handler, and cleans up automatically in OnRemove. You can't forget."

### [SCREEN: Show all available helpers]

```csharp
AddButtonListener(button, () => { });           // Button click
AddToggleListener(toggle, (isOn) => { });       // Toggle change
AddSliderListener(slider, (value) => { });      // Slider drag
AddInputListener(input, (text) => { });         // Input end edit
```

**NARRATION:**
> "There are helpers for all common UI elements. Buttons, toggles, sliders, input fields. Same pattern — add in OnRegister, forget about cleanup, it's handled."

### [SCREEN: Demo in Unity - clicking button shows log]

**NARRATION:**
> "Let's test... click the pause button... and there's our log. Clean, safe, automatic cleanup."

---

## SECTION 4: [LISTENSTO] ATTRIBUTE (9:30 - 13:00)

### [SCREEN: Code showing verbose signal subscription]

```csharp
public class PlayerHudMediator : ExtendedMediator<PlayerHudView>
{
    [Inject] public ScoreChangedSignal ScoreChangedSignal { get; set; }
    [Inject] public PlayerDiedSignal PlayerDiedSignal { get; set; }
    
    public override void OnRegister()
    {
        base.OnRegister();
        ScoreChangedSignal.AddListener(OnScoreChanged);
        PlayerDiedSignal.AddListener(OnPlayerDied);
    }
    
    public override void OnRemove()
    {
        ScoreChangedSignal.RemoveListener(OnScoreChanged);
        PlayerDiedSignal.RemoveListener(OnPlayerDied);
        base.OnRemove();
    }
    
    private void OnScoreChanged(int score) { }
    private void OnPlayerDied() { }
}
```

**NARRATION:**
> "This is the old way. For every signal you want to listen to, you need the Inject property, AddListener in OnRegister, RemoveListener in OnRemove. Three places to maintain. It adds up fast."

### [SCREEN: Same code with [ListensTo] — side by side comparison]

```csharp
public class PlayerHudMediator : ExtendedMediator<PlayerHudView>
{
    [ListensTo(typeof(ScoreChangedSignal))]
    public void OnScoreChanged(int score) 
    {
        View.ScoreText.text = $"Score: {score}";
    }
    
    [ListensTo(typeof(PlayerDiedSignal))]
    public void OnPlayerDied() 
    {
        View.ShowDeathScreen();
    }
}
```

**NARRATION:**
> "With ListensTo, this is all you need. One attribute, one method. No inject, no AddListener, no RemoveListener. The framework scans for these attributes and wires everything up automatically."

### [SCREEN: Create signal class, dispatch it, show Mediator responding]

**NARRATION:**
> "Let me show you it working. I'll create a ScoreChangedSignal, dispatch it from somewhere, and watch the Mediator respond..."

### [SCREEN: Highlight the PUBLIC keyword]

```csharp
// ✅ This works
[ListensTo(typeof(MySignal))]
public void OnMySignal() { }

// ❌ This is SILENTLY IGNORED
[ListensTo(typeof(MySignal))]
private void OnMySignal() { }
```

**NARRATION:**
> "Critical gotcha: the method MUST be public. Private and protected methods are silently ignored — no error, no warning, it just doesn't subscribe. This trips up everyone at least once. Make it public."

### [SCREEN: Decision chart]

```
Do I need to DISPATCH this signal too?
  YES → Use [Inject] + manual subscription
  NO  → Use [ListensTo]

Do I need conditional subscription?
  YES → Use [Inject] + manual
  NO  → Use [ListensTo]
```

**NARRATION:**
> "When should you use which? If you're just listening, use ListensTo. If you also need to dispatch the signal, or subscribe conditionally, use the manual approach. Most of the time, ListensTo is what you want."

---

## SECTION 5: PUTTING IT TOGETHER (13:00 - 14:30)

### [SCREEN: Complete working example]

```csharp
// View — just references
public class GameHudView : View
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _livesText;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private GameObject _gameOverPanel;
    
    public TextMeshProUGUI ScoreText => _scoreText;
    public TextMeshProUGUI LivesText => _livesText;
    public Button PauseButton => _pauseButton;
    public GameObject GameOverPanel => _gameOverPanel;
}

// Mediator — all the logic
public class GameHudMediator : ExtendedMediator<GameHudView>
{
    [Inject] public PauseGameSignal PauseGameSignal { get; set; }
    
    public override void OnRegister()
    {
        base.OnRegister();
        AddButtonListener(View.PauseButton, OnPauseClicked);
    }
    
    private void OnPauseClicked() => PauseGameSignal.Dispatch();
    
    [ListensTo(typeof(ScoreChangedSignal))]
    public void UpdateScore(int score) => View.ScoreText.text = $"Score: {score}";
    
    [ListensTo(typeof(LivesChangedSignal))]
    public void UpdateLives(int lives) => View.LivesText.text = $"x{lives}";
    
    [ListensTo(typeof(GameOverSignal))]
    public void ShowGameOver() => View.GameOverPanel.SetActive(true);
}
```

**NARRATION:**
> "Here's a complete example. The View is dead simple — just references. The Mediator handles everything: button clicks with AddButtonListener, signal responses with ListensTo. Clean separation, minimal code."

### [SCREEN: Data flow diagram]

```
                    ┌─────────────────┐
    Signals ───────►│    Mediator     │
                    │  [ListensTo]    │
                    │        │        │
                    │        ▼        │
                    │      View       │
                    │    (updates)    │
                    └────────┬────────┘
                             │
    User clicks ─────────────┘
         │
         ▼
    Dispatch Signal → Command → Game Logic
```

**NARRATION:**
> "The data flow is always the same. Signals come in through ListensTo, update the View. User actions dispatch signals out, which trigger Commands. The Mediator never knows what happens after it dispatches — that's the beauty of loose coupling."

---

## OUTRO (14:30 - 15:00)

### [SCREEN: Chapter summary slide]

**NARRATION:**
> "That's Views and Mediators. Views hold references, Mediators bridge to your app. Use AddButtonListener for safe UI wiring, ListensTo to eliminate boilerplate.
>
> Next chapter, we'll look at Commands — what happens when those signals get dispatched. See you there."

### [SCREEN: End card with links to docs/next chapter]

---

## B-ROLL / VISUAL NOTES

- Show hierarchy window when explaining auto-registration
- Split-screen before/after for ListensTo comparison
- Highlight Console window when demonstrating signals firing
- Show component inspector to prove Mediator is added to same GameObject
- Use callout boxes for warnings (especially "must be public")

## TIMESTAMPS FOR DESCRIPTION

```
0:00 - Intro
0:45 - View Basics
3:30 - Mediator Basics
7:00 - UI Event Helpers
9:30 - [ListensTo] Attribute
13:00 - Complete Example
14:30 - Summary
```
