# Chapter 6: Screen FSM — Video Script

**Duration:** ~15-18 minutes  
**Style:** Screen recording with code walkthroughs, animated diagrams  
**Tone:** Conversational, practical

---

## INTRO (0:00 - 1:30)

### VISUAL: Game switching between screens — splash → menu → game → game over → menu

**NARRATION:**
> "Every game has screens. Splash screen, main menu, gameplay, game over. And somehow you need to manage them all — knowing when to show one, hide another, clean up resources, set up the next one.
>
> A lot of developers end up with spaghetti code. Flags everywhere. 'If we're in menu and not loading and game hasn't started yet...' — you know the drill.
>
> In this chapter, we're going to look at Framewerk's Screen FSM — Finite State Machine — which gives you a clean, predictable way to manage your entire app flow."

### VISUAL: Show the FSM diagram from the tutorial

**NARRATION:**
> "By the end of this video, you'll understand how to create states, pair them with screens, and build a complete game flow that transitions smoothly between each phase."

---

## SECTION 1: What is an FSM? (1:30 - 4:00)

### VISUAL: Simple animated diagram — boxes with arrows

**NARRATION:**
> "FSM stands for Finite State Machine. It's a fancy term for something simple: your app can only be in ONE state at a time. 
>
> Think of it like rooms in a house. You can be in the kitchen, or the bedroom, but not both. And there are doorways — transitions — between rooms."

### VISUAL: Side-by-side comparison — messy code vs clean code

**PROMPT: Show two code blocks:**

**LEFT (BAD):**
```csharp
public void OnPlayClicked()
{
    menuUI.SetActive(false);
    gameUI.SetActive(true);
    currentScreen = "game";
    audioManager.PlayGameMusic();
    // More scattered logic...
}
```

**RIGHT (GOOD):**
```csharp
public void OnPlayClicked()
{
    Fsm.SwitchState(new GameState());
}
```

**NARRATION:**
> "Without an FSM, you end up with manual management scattered everywhere. You're setting UI objects active, tracking strings, hoping you didn't forget something.
>
> With an FSM, you just say 'switch to this state' and the machine handles everything. Entry, exit, cleanup — all automatic."

### VISUAL: Highlight benefits as bullet points animate in

**NARRATION:**
> "The benefits are huge:
> - Single source of truth — you always know what state you're in
> - Automatic cleanup — exiting a state cleans up its mess
> - Queued transitions — no race conditions
> - Decoupled code — each state is its own little world"

---

## SECTION 2: AppState and Screens (4:00 - 7:30)

### VISUAL: Show AppState.cs source file

**NARRATION:**
> "In Framewerk, we have two pieces that work together: AppState and AppStateScreen.
>
> AppState is your logic layer. It handles business rules, signal subscriptions, game data. AppStateScreen is your visual layer — it creates the UI, plays animations, manages what the player sees."

### VISUAL: Type out a simple state class

**PROMPT: Live-code this:**
```csharp
public class MenuState : AppState<MenuScreen>
{
    [Inject] public IAppFsm Fsm { get; set; }

    protected override void Enter()
    {
        Debug.Log("We're in the menu!");
    }

    protected override void Exit()
    {
        Debug.Log("Leaving the menu...");
    }
}
```

**NARRATION:**
> "Here's a basic state. The generic parameter tells it which screen to use — MenuScreen in this case. That screen gets automatically injected, you don't create it yourself.
>
> The Enter method runs when we arrive at this state. Exit runs when we're leaving."

### VISUAL: Show the lifecycle diagram from tutorial

```
SwitchState(new GameState())
        │
        ▼
┌───────────────────────────────────────┐
│  1. Dependencies injected              │
├───────────────────────────────────────┤
│  2. RegisterHandlers()                 │
├───────────────────────────────────────┤
│  3. Enter()                            │
├───────────────────────────────────────┤
│  4. Screen.PerformEnter()              │
├───────────────────────────────────────┤
│  5. EnterFinished()                    │
├───────────────────────────────────────┤
│  6. AppStateEnterSignal dispatched     │
└───────────────────────────────────────┘
```

**NARRATION:**
> "When you switch to a state, here's what happens in order:
> 1. Dependencies get injected — your Screen, your managers, everything
> 2. RegisterHandlers runs — subscribe to signals here
> 3. Your Enter method executes
> 4. The Screen enters and sets up UI
> 5. Transition is marked complete
> 6. A global signal fires so other systems can react
>
> Exit is the reverse. Your Exit method, then screen exits, then handlers unregister, then cleanup."

---

## SECTION 3: AppStateScreen (7:30 - 10:30)

### VISUAL: Show AppStateScreen.cs source, highlight InstantiateView methods

**NARRATION:**
> "The Screen class is where your visuals live. Let's look at what it gives you."

### VISUAL: Type out a screen class

**PROMPT: Live-code this:**
```csharp
public class MenuScreen : AppStateScreen
{
    public Signal PlayClicked = new Signal();
    
    private MenuView _view;

    protected override void Enter()
    {
        _view = InstantiateView<MenuView>();
        _view.PlayButton.onClick.AddListener(() => PlayClicked.Dispatch());
    }

    protected override void Exit()
    {
        _view.PlayButton.onClick.RemoveAllListeners();
    }
}
```

**NARRATION:**
> "See InstantiateView? That's a helper method that creates your UI prefab and — crucially — tracks it. When this screen exits, all views you created this way get automatically destroyed.
>
> No more forgetting to clean up UI. The Screen handles it."

### VISUAL: Show async version

```csharp
protected override async void Enter()
{
    _view = await InstantiateViewAsync<MenuView>();
    await _view.PlayEnterAnimation();
}
```

**NARRATION:**
> "There are async versions too for Addressables loading. Perfect for when you need to load assets or play entrance animations before the state is 'ready'."

### VISUAL: Quick table — State vs Screen responsibilities

| AppState | AppStateScreen |
|----------|----------------|
| Business logic | UI instantiation |
| Signal handling | View animations |
| Data management | Visual transitions |

**NARRATION:**
> "Think of it this way: State is the brain, Screen is the face. State makes decisions, Screen shows them."

---

## SECTION 4: Transitions and Queueing (10:30 - 13:00)

### VISUAL: Show AppFsm.cs, highlight SwitchState and queue

**NARRATION:**
> "Now let's talk about how transitions work. When you call SwitchState, a few things can happen."

### VISUAL: Animated diagram — state A to state B

**NARRATION:**
> "If there's no current state — like at app launch — the new state just enters directly.
>
> If there IS a current state, it has to exit first. The FSM runs the exit sequence, THEN starts the new state."

### VISUAL: Show the queue scenario with animation

```
State A is active
  ↓
SwitchState(B) called
  ↓
B is queued, A starts exiting
  ↓
SwitchState(C) called during exit!
  ↓
C is queued too
  ↓
A finishes exit → B enters → B exits → C enters
```

**NARRATION:**
> "Here's the magic: if you call SwitchState while a transition is happening, the new state gets queued. You can even queue multiple states.
>
> This prevents race conditions. You never have two states trying to enter at once. Everything is orderly."

### VISUAL: Show TransitionType enum

```csharp
public enum TransitionType
{
    None,   // Idle
    Enter,  // Entering
    Exit    // Exiting
}
```

**NARRATION:**
> "You can check CurrentTransition to see if the FSM is busy. None means idle, Enter or Exit means a transition is in progress."

---

## SECTION 5: Global Signals (13:00 - 14:30)

### VISUAL: Show signal usage in an analytics class

```csharp
public class AnalyticsManager
{
    [Inject] public AppStateEnterSignal EnterSignal { get; set; }

    public void Initialize()
    {
        EnterSignal.AddListener(OnStateEntered);
    }

    private void OnStateEntered(Type stateType)
    {
        Analytics.TrackScreen(stateType.Name);
    }
}
```

**NARRATION:**
> "AppStateEnterSignal and AppStateExitSignal are dispatched globally whenever any state enters or exits. They pass the Type of the state.
>
> This is perfect for cross-cutting concerns — analytics, audio transitions, logging. You don't have to modify every single state."

---

## SECTION 6: Practical Example (14:30 - 17:30)

### VISUAL: Show the complete game flow diagram

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ SplashState │────▶│  MenuState  │────▶│  GameState  │────▶│GameOverState│
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                           ▲                                       │
                           └───────────────────────────────────────┘
```

**NARRATION:**
> "Let's build a real game flow. Splash, Menu, Game, Game Over — with the ability to retry or return to menu."

### VISUAL: Quick scroll through each state (pre-written, highlight key parts)

**PROMPT: Show each state briefly, 15-20 seconds each:**

1. **SplashState** — loads config, transitions to menu
2. **MenuState** — play and settings buttons
3. **GameState** — score tracking, game manager integration
4. **GameOverState** — shows score, retry/menu options

**NARRATION:**
> "SplashState loads our config while showing the logo, then automatically transitions to MenuState.
>
> MenuState wires up button clicks through signals — keeping the Screen dumb and the State smart.
>
> GameState is where gameplay happens. Notice how it uses RegisterHandlers to subscribe to game events, and UnregisterHandlers to clean up.
>
> GameOverState takes the final score as a constructor parameter. States can have data! When you retry, it creates a fresh GameState."

### VISUAL: Show the context bindings

```csharp
public override void Launch()
{
    var fsm = injectionBinder.GetInstance<IAppFsm>();
    fsm.SwitchState(new SplashState());
}
```

**NARRATION:**
> "To kick it all off, get your FSM instance and switch to your first state. That's it. The machine takes over from there."

---

## OUTRO (17:30 - 18:30)

### VISUAL: Summary slide with key points

**NARRATION:**
> "Let's recap:
> - AppFsm manages your app states — one active at a time
> - AppState handles logic, AppStateScreen handles visuals
> - Transitions are queued automatically — no race conditions
> - Enter, Exit, RegisterHandlers, UnregisterHandlers — your lifecycle hooks
> - Global signals let other systems react to state changes
>
> Use this for major screens. For popups and dialogs, use the Popup system we covered earlier — it's designed for overlays, not full-screen transitions."

### VISUAL: Next chapter teaser

**NARRATION:**
> "Next up: Asset Management. We'll look at loading, caching, and releasing assets properly so your game doesn't leak memory. See you there!"

---

## B-ROLL / SUPPLEMENTARY FOOTAGE NEEDED

1. **Diagram animations:**
   - FSM flow (states as boxes, arrows as transitions)
   - Lifecycle sequence (enter steps stacking)
   - Queue visualization (states lining up)

2. **Code recordings:**
   - Live typing of MenuState class
   - Live typing of MenuScreen class
   - Scrolling through complete example states

3. **Unity recordings:**
   - Game transitioning between screens (real project if available)
   - Console logs showing Enter/Exit messages

4. **Graphics:**
   - Side-by-side code comparison (messy vs clean)
   - State vs Screen responsibility table
   - Chapter title card

---

## TIMESTAMPS FOR EDITING

| Timestamp | Section |
|-----------|---------|
| 0:00 | Intro |
| 1:30 | What is FSM |
| 4:00 | AppState and Screens |
| 7:30 | AppStateScreen |
| 10:30 | Transitions and Queueing |
| 13:00 | Global Signals |
| 14:30 | Practical Example |
| 17:30 | Outro |

---

## KEY VISUALS / DIAGRAMS TO CREATE

### 1. FSM Overview Diagram
```
┌─────────────────────────────────────────────────────────────────┐
│                         AppFsm                                   │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐          │
│  │ SplashState │───▶│ MenuState   │───▶│ GameState   │          │
│  │  + Screen   │    │  + Screen   │    │  + Screen   │          │
│  └─────────────┘    └─────────────┘    └─────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

### 2. Lifecycle Flow
```
    SwitchState()
         │
         ▼
    ┌─────────┐
    │ Inject  │
    ├─────────┤
    │Register │
    ├─────────┤
    │ Enter   │
    ├─────────┤
    │ Screen  │
    │ Enter   │
    ├─────────┤
    │Complete │
    └─────────┘
```

### 3. Queue Animation
```
Frame 1: [A active] ──▶ SwitchState(B)
Frame 2: [A exiting] [B queued]
Frame 3: [A exiting] [B queued] [C queued]  ◀── SwitchState(C)
Frame 4: [B entering] [C queued]
Frame 5: [B active] ──▶ exits ──▶ [C entering]
Frame 6: [C active]
```

---

## SCRIPT NOTES

- **Pace:** Medium-fast, this is an intermediate topic
- **Demos:** Keep code on screen long enough to read (5-7 seconds minimum)
- **Transitions:** Use fade or slide, nothing flashy
- **Audio:** Light background music, lower during code explanations
