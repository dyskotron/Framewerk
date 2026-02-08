# Chapter 0: Foundations — Video Script

**Duration:** ~12-15 minutes  
**Style:** Animated diagrams, code overlays, minimal talking head  
**Tone:** Friendly, clear, slightly informal

---

## INTRO (0:00-0:45)

### Scene: Animated title card
**Visual:** "Chapter 0: Foundations" fades in with Framewerk logo

**Narration:**
> Before you write a single line of code, let's talk about *why* Framewerk exists. This isn't a coding tutorial—it's a mental model. By the end, you'll understand how all the pieces fit together. And trust me, once this clicks, everything else becomes much easier.

### Scene: Split screen - chaos vs order
**Visual:** Left side shows tangled lines connecting random boxes (spaghetti). Right side shows clean organized boxes with clear flow.

**Narration:**
> On the left: typical Unity development. Everything talks to everything. Singletons everywhere. FindObjectOfType scattered across the codebase. On the right: Framewerk. Structure. Separation. Sanity.

---

## SECTION 1: What is Framewerk? (0:45-2:30)

### Scene: MVCS letters animate in
**Visual:** Letters M, V, C, S appear one by one with labels beneath each

**Narration:**
> Framewerk is an MVCS architecture framework. That's Model, View, Controller, and Service. It's built on top of StrangeIoC, which handles the dependency injection magic. Let's break down what each letter means.

### Scene: Four quadrants appear
**Visual:** Screen divides into 4 quadrants, each highlighting as discussed

**Animation:** 
- MODEL box pulses → shows "Data, State, Business Rules"
- VIEW box pulses → shows "Display, UI, MonoBehaviours"  
- CONTROLLER box pulses → shows "Commands, Action Handlers"
- SERVICE box pulses → shows "APIs, Network, Persistence"

**Narration:**
> Models hold your data and state. They know nothing about Unity. Views are your MonoBehaviours—they display things but don't think. Controllers, which we call Commands, handle actions. And Services talk to the outside world.

### Scene: Comparison slide
**Visual:** Side-by-side code snippets

**Left side (Bad):**
```csharp
void Start() {
    scoreManager = FindObjectOfType<ScoreManager>();
    playerData = PlayerData.Instance;
}
```

**Right side (Good):**
```csharp
[Inject] public IScoreModel ScoreModel { get; set; }
[Inject] public IPlayerData PlayerData { get; set; }
```

**Narration:**
> In typical Unity, you're always *hunting* for dependencies. FindObjectOfType here, a singleton there. In Framewerk, you just *declare* what you need. The framework delivers it. That's dependency injection.

---

## SECTION 2: Dependency Injection Explained (2:30-4:30)

### Scene: Restaurant analogy animation
**Visual:** Animated restaurant scene

**Animation sequence:**
1. Customer at table (a class)
2. Customer shouts across room "HEY! GIVE ME THE SALT!" (FindObjectOfType)
3. Waiter brings salt directly to table (injection)

**Narration:**
> Think of it like a restaurant. The old way: you're at your table, and you have to shout across the room, get up, hunt for what you need. The Framewerk way: you sit down, and the waiter brings exactly what you need to your table. You declared it on the menu.

### Scene: Code transformation
**Visual:** Code block animates from "before" to "after"

**Narration:**
> In code, this means instead of *finding* your ScoreManager, you just put an Inject attribute on a property. When your class is created, the injector scans for these attributes, looks up what's bound, and provides the instance automatically.

### Scene: Dependency flow diagram
**Visual:** Animated arrows flowing from Context down to classes

```
         ┌──────────┐
         │ CONTEXT  │
         └────┬─────┘
              │
    ┌─────────┼─────────┐
    ↓         ↓         ↓
┌───────┐ ┌───────┐ ┌───────┐
│Class A│ │Class B│ │Class C│
│[Inject]│ │[Inject]│ │[Inject]│
└───────┘ └───────┘ └───────┘
```

**Animation:** Arrows animate flowing from Context to each class, "injecting" the dependencies

**Narration:**
> The Context is the source of truth. It knows all the bindings. When it creates a class—or mediates a view—it checks for Inject attributes and fills them in. Your classes never need to know *where* things come from.

### Scene: Benefits list
**Visual:** Benefits pop up one by one with icons

1. 🧪 **Testable** — Mock anything
2. 🔄 **Flexible** — Swap implementations
3. 👁️ **Clear** — Dependencies are explicit
4. 🔗 **Decoupled** — Classes don't know sources

**Narration:**
> Why does this matter? Testability—inject mocks instead of real services. Flexibility—swap implementations without changing code. Clarity—every dependency is visible, not hidden. Decoupling—your classes don't care where things come from.

---

## SECTION 3: The Context (4:30-6:30)

### Scene: Context as hub visualization
**Visual:** Central hub with spokes radiating out

**Animation:** Hub labeled "Context" in center, three binders orbit around it

**Narration:**
> The Context is the wiring hub of your application. It's where you define all your bindings. Think of it as the central nervous system—everything connects through here.

### Scene: Code walkthrough
**Visual:** Context code with sections highlighting as discussed

```csharp
public class GameContext : FramewerkMVCSContext
{
    protected override void mapBindings()
    {
        base.mapBindings();
        
        // INJECTION BINDINGS
        injectionBinder.Bind<IScoreModel>().To<ScoreModel>().ToSingleton();
        
        // MEDIATION BINDINGS
        mediationBinder.Bind<PlayerView>().To<PlayerMediator>();
        
        // COMMAND BINDINGS  
        commandBinder.Bind<PlayerDiedSignal>().To<HandleDeathCommand>();
    }
}
```

**Animation:** Each section highlights and zooms slightly as narrator discusses it

**Narration:**
> In your Context's mapBindings method, you define three kinds of bindings. First, injection bindings—when someone needs IScoreModel, give them ScoreModel as a singleton. Second, mediation bindings—when a PlayerView appears in the scene, create a PlayerMediator for it. Third, command bindings—when PlayerDiedSignal fires, execute HandleDeathCommand.

### Scene: Binding types
**Visual:** Three binding types with visual representations

| `.To<T>()` | 📦 New box each time |
| `.ToSingleton()` | 📦 One shared box |
| `.ToValue(x)` | 📦 This exact box |

**Animation:** Boxes multiply for To, stay single for Singleton, show specific box for ToValue

**Narration:**
> Binding types matter. To creates a new instance each time. ToSingleton creates once and shares everywhere. ToValue uses an exact instance you provide.

### Scene: Lifecycle diagram
**Visual:** Vertical timeline animation

```
Bootstrap Awake()
      ↓
Context Created
      ↓
mapBindings() ← YOUR CODE HERE
      ↓
Views Mediated
      ↓
Launch() → ContextStartSignal
      ↓
Application Running
```

**Animation:** Timeline draws from top to bottom, marker highlights each step

**Narration:**
> The lifecycle flows like this: your bootstrap wakes up, creates the Context, which calls mapBindings—that's where your code goes. Then existing views get mediated, Launch fires the ContextStartSignal, and your first command runs. Application started.

---

## SECTION 4: Signals & Commands (6:30-9:00)

### Scene: Signal definition
**Visual:** Signal class with type parameters highlighted

```csharp
public class ItemClickedSignal : Signal<ItemData> { }
```

**Animation:** Type parameter `<ItemData>` pulses and expands to show "This is the payload type"

**Narration:**
> Signals are type-safe events. Unlike string-based events where you might type "playerDied" in one place and "PlayerDied" in another, signals are actual types. The compiler catches mistakes. And they carry typed payloads—this signal carries ItemData.

### Scene: Dispatch and listen
**Visual:** Split screen showing dispatch on left, listen on right

**Left (Mediator dispatching):**
```csharp
ItemClickedSignal.Dispatch(itemData);
```

**Right (Command receiving):**
```csharp
[Inject] public ItemData ItemData { get; set; }
```

**Animation:** Arrow flows from left to right, "itemData" travels along it

**Narration:**
> When you dispatch a signal with a payload, that payload becomes injectable in the command. The framework handles the handoff. You dispatch ItemData, the command receives it through injection.

### Scene: ListensTo attribute
**Visual:** Code comparison

**Before (verbose):**
```csharp
public override void OnRegister()
{
    ItemClicked.AddListener(OnItemClicked);
}
public override void OnRemove()
{
    ItemClicked.RemoveListener(OnItemClicked);
}
```

**After (clean):**
```csharp
[ListensTo(typeof(ItemClickedSignal))]
public void OnItemClicked(ItemData data) { }
```

**Animation:** Before code fades, after code slides in

**Narration:**
> In mediators, you can use the ListensTo attribute. It automatically subscribes on register and unsubscribes on remove. Less boilerplate, same result.

### Scene: Command structure
**Visual:** Command class with annotations

```csharp
public class ShowItemPopupCommand : Command
{
    [Inject] public IPopupManager PopupManager { get; set; }
    [Inject] public ItemData ItemData { get; set; }
    
    public override void Execute()
    {
        PopupManager.ShowPopup(ItemData);
    }
}
```

**Animation:** Arrows point to each Inject showing where values come from

**Narration:**
> Commands are single-purpose handlers. They get created when their signal fires, injected with everything they need, execute once, and disappear. One command, one job. This keeps your codebase clean and testable.

### Scene: Signal → Command flow animation
**Visual:** Animated flow diagram

```
[Mediator] → [Signal] → [Command] → [Model/Service]
     ↑                                    │
     └────────────[Signal]←───────────────┘
```

**Animation:** 
1. User clicks (spark on Mediator)
2. Arrow flows to Signal (pulse)
3. Arrow flows to Command (pulse)
4. Command updates Model
5. Model fires new Signal
6. Arrow flows back to Mediator
7. View updates

**Narration:**
> Here's the full flow. User clicks, mediator dispatches a signal, command executes and updates the model, model fires a state-change signal, mediator receives it and updates the view. Unidirectional. Predictable. Debuggable.

---

## SECTION 5: The Mediation Pattern (9:00-11:00)

### Scene: View vs Mediator split
**Visual:** Two boxes side by side

**VIEW box:**
- MonoBehaviour
- UI References
- Display Methods
- NO LOGIC

**MEDIATOR box:**
- Pure C#
- [Inject] Dependencies
- Signal Listeners
- ALL LOGIC

**Animation:** Items appear one by one in each box

**Narration:**
> Views and Mediators are partners but have completely different jobs. The View is a dumb MonoBehaviour—it knows how to display things, that's it. The Mediator is the brain—it gets injections, listens to signals, and tells the View what to do.

### Scene: Concrete example
**Visual:** Code split screen

**View:**
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

**Mediator:**
```csharp
public class ScoreMediator : Mediator<ScoreView>
{
    [Inject] public IScoreModel ScoreModel { get; set; }
    
    [ListensTo(typeof(ScoreChangedSignal))]
    public void OnScoreChanged(int newScore)
    {
        View.SetScore(newScore);
    }
}
```

**Animation:** Highlight View.SetScore in mediator, arrow points to SetScore in View

**Narration:**
> Here's a real example. The ScoreView just knows how to display a score—one method, SetScore. The ScoreMediator knows the ScoreModel, listens for ScoreChangedSignal, and calls View.SetScore when it fires. The View never knows about signals or models. Clean separation.

### Scene: Benefits visualization
**Visual:** Icons with labels

1. 🔁 **Reusable Views** — Different mediators, same view
2. 🧪 **Testable Views** — No dependencies to mock
3. 🎨 **Designer-Friendly** — Edit prefabs without code
4. 🧪 **Testable Mediators** — Mock the View interface

**Animation:** Each benefit animates in with icon

**Narration:**
> Why separate them? Views become reusable—same view, different mediator for different contexts. Views are testable—no dependencies. Designers can edit prefabs without touching code. Mediators are testable too—mock the view interface.

---

## SECTION 6: Full Architecture (11:00-13:00)

### Scene: Complete architecture diagram
**Visual:** Full architecture diagram building piece by piece

**Animation sequence:**
1. Context appears at top
2. Three binders appear below Context
3. Model, View, Command boxes appear in row
4. Mediator appears in center
5. Service and Signals appear at bottom
6. Connection arrows animate in

**Narration:**
> Let's see the whole picture. At the top, the Context—the wiring hub. It manages three binders: injection, mediation, and command. Below, your application components: Models for data, Views for display, Commands for actions. Mediators bridge Views to the application. Services handle external communication. And Signals connect everything with loose coupling.

### Scene: Request flow animation
**Visual:** Step-by-step flow with numbered steps

**Narration:**
> Let's trace a real interaction. User clicks an item.

**Animation step 1:** User icon clicks ItemView
> Step one: the click happens in the View.

**Animation step 2:** Arrow to ItemMediator
> Step two: View notifies its Mediator.

**Animation step 3:** Arrow to ItemClickedSignal
> Step three: Mediator dispatches ItemClickedSignal.

**Animation step 4:** Arrow to ShowItemPopupCommand
> Step four: Command binder triggers ShowItemPopupCommand.

**Animation step 5:** Arrow to PopupManager
> Step five: Command calls PopupManager to show the popup.

**Animation step 6:** Arrow to PopupOpenedSignal
> Step six: PopupManager fires PopupOpenedSignal.

**Animation step 7:** Arrow to PopupMediator
> Step seven: PopupMediator receives signal, updates its View.

**Narration:**
> Seven steps, but completely traceable. Every connection is explicit. No magic. No hidden dependencies. That's the power of architecture.

---

## SECTION 7: Key Takeaways (13:00-14:00)

### Scene: Summary cards
**Visual:** Flashcard-style summaries flip through

| **Context** | The central hub where all bindings live |
| **DI** | Ask for what you need, don't hunt for it |
| **Signals** | Type-safe events that decouple communication |
| **Commands** | Single-purpose handlers triggered by signals |
| **Views** | Dumb display objects, no business logic |
| **Mediators** | Smart bridges between Views and application |

**Animation:** Cards flip in sequence

**Narration:**
> Let's recap. Context is your wiring hub. Dependency injection means asking, not hunting. Signals are type-safe events. Commands are single-purpose handlers. Views display, Mediators think. Remember these, and you've got the foundation.

---

## OUTRO (14:00-14:30)

### Scene: Transition to next chapter
**Visual:** "Chapter 1: Your First Framewerk Project" teaser

**Narration:**
> That's the conceptual foundation. You understand the architecture. Now it's time to build. In Chapter 1, we'll create a complete Framewerk project from scratch. You'll set up a Context, create Views and Mediators, wire signals and commands, and have something working by the end. See you there!

### Scene: End card
**Visual:** Framewerk logo, links to docs and next video

**Text overlay:**
- Next: Chapter 1 — Your First Framewerk Project
- Docs: github.com/dyskotron/framewerk
- Discord: [community link]

---

## PRODUCTION NOTES

### Visual Style
- Clean, minimal design with dark background
- Bright accent colors for different components (Model=blue, View=green, Command=orange, etc.)
- Smooth animations with 0.3-0.5s transitions
- Code uses a readable monospace font (Fira Code or JetBrains Mono)

### Diagram Colors
- **Context:** Purple #8B5CF6
- **Model:** Blue #3B82F6
- **View:** Green #22C55E
- **Mediator:** Teal #14B8A6
- **Command:** Orange #F97316
- **Signal:** Yellow #EAB308
- **Service:** Pink #EC4899

### Animation Suggestions
- Use motion blur on fast movements
- Pulse effects when highlighting
- Particle effects for "data flowing" between components
- Subtle glow on active elements

### Audio
- Calm background music (lo-fi or ambient)
- Subtle UI sounds for animations (clicks, whooshes)
- Clear voice narration, moderate pace

### B-Roll Ideas
- Quick Unity Editor shots when mentioning MonoBehaviours
- IDE shots when showing code examples
- Real game footage showing "what you can build" in intro/outro
