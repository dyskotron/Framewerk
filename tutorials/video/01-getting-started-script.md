# Video Script: Chapter 1 — Getting Started with Framewerk

**Total Runtime:** ~12-15 minutes  
**Format:** Screen recording + voiceover  
**Resolution:** 1920×1080

---

## INTRO (0:00 - 0:30)

### VISUAL
- Title card: "Getting Started with Framewerk"
- Fade to Unity splash screen → empty Unity project

### NARRATION
> "Welcome to Framewerk — a battle-tested MVCS framework for Unity. In the next 15 minutes, you'll go from an empty project to a fully working UI scene with proper architecture. Let's get started."

---

## SECTION 1: Installation (0:30 - 2:30)

### VISUAL: Package Manager
- Open Unity (empty project, 2021.3+)
- Navigate: Window → Package Manager
- Click + button (zoom in on it)

### NARRATION
> "First, let's install Framewerk. Open the Package Manager from the Window menu."

---

### VISUAL: Git URL Input
- Click "Add package from git URL"
- Type/paste: `https://github.com/dyskotron/Framewerk.git?path=Packages/com.dyskotron.framewerk`
- Click Add

### NARRATION
> "Click the plus button and select 'Add package from git URL'. Paste in the Framewerk repository URL. This is the meta-package that includes everything you need — core, UI, screen FSM, and editor tools."

---

### VISUAL: Package importing
- Show the import progress
- Packages appearing in Package Manager list

### NARRATION
> "Unity downloads the package and its dependencies automatically. You'll see Addressables and TextMeshPro install as well — Framewerk uses Addressables for asset loading, and TextMeshPro for, well, text."

---

### VISUAL: Console tab
- Show Console — should be clean
- Quick scroll through imported packages

### NARRATION
> "Once complete, check the Console. No errors means we're good. Framewerk is now installed and ready."

**[PAUSE POINT: 2:30]**

---

## SECTION 2: Creating a Scene (2:30 - 6:00)

### VISUAL: Menu bar
- Click: Framewerk → Create Scene
- Scene Wizard window appears

### NARRATION
> "Framewerk includes wizards that generate boilerplate code for you. Let's create our first scene. Go to Framewerk menu, then Create Scene."

---

### VISUAL: Scene Wizard — filling fields
- Type "MyGame" in Scene Name
- Click Auto button for Namespace (shows "MyGame")
- Keep default folders

### NARRATION
> "Name your scene — I'll call this 'MyGame'. Click the Auto button to derive the namespace from the folder structure. For a new project, it just matches the scene name."

---

### VISUAL: Preview section
- Highlight the preview panel showing:
  - MyGameBootstrap.cs
  - MyGameContext.cs  
  - MyGameStartCommand.cs
  - MyGame.unity

### NARRATION
> "The preview shows what will be generated. Every Framewerk scene needs three things: a Bootstrap to start everything, a Context for dependency injection, and a StartCommand for initialization logic."

---

### VISUAL: Click Generate
- Click Generate button
- Unity recompiles (show bottom-right progress)
- Scripts appear in Project window

### NARRATION
> "Click Generate. Unity compiles the new scripts, then the wizard finishes setting up the scene."

---

### VISUAL: Project window
- Navigate to Assets/Scripts
- Double-click MyGameBootstrap.cs (opens in IDE)

### NARRATION
> "Let's look at what was created. The Bootstrap is a MonoBehaviour attached to the scene. It creates the Context on Start and cleans up on quit."

---

### VISUAL: IDE — Bootstrap.cs
- Highlight: ContextView, viewConfig field, _context.Start()
- Switch to MyGameContext.cs

### NARRATION
> "Notice it extends ContextView and references a ViewConfig — that's the configuration for loading assets. The Context is created and started here."

---

### VISUAL: IDE — Context.cs
- Highlight: mapBindings() method
- Highlight: commandBinder line

### NARRATION
> "The Context is where all the wiring happens. This mapBindings method is where you'll spend most of your configuration time. Right now it just binds the StartCommand to run when the context starts."

---

### VISUAL: IDE — StartCommand.cs
- Highlight: [Inject] attribute
- Highlight: Execute() method

### NARRATION
> "The StartCommand runs once at startup. Notice the Inject attribute — Framewerk automatically provides the ViewConfig here. This is dependency injection at work."

---

### VISUAL: Unity — Scene hierarchy
- Open Assets/Scenes/MyGame.unity
- Show hierarchy: MyGame object, Canvas, EventSystem

### NARRATION
> "Open the generated scene. The wizard created a clean hierarchy with the Bootstrap component attached and a Canvas ready for your UI."

---

### VISUAL: Play the scene
- Click Play
- Console shows no errors

### NARRATION
> "Press Play to verify everything works. A clean Console means success — we have a working Framewerk scene."

**[PAUSE POINT: 6:00]**

---

## SECTION 3: The View-Mediator Pattern (6:00 - 7:00)

### VISUAL: Slide/diagram
- Show View-Mediator diagram:
```
┌─────────────────────────────┐
│         MEDIATOR            │
│  • Injects services         │
│  • Listens to Signals       │
│  • Updates the View         │
└────────────┬────────────────┘
             │
             ▼
┌─────────────────────────────┐
│           VIEW              │
│  • UI elements              │
│  • Button events            │
│  • No app knowledge         │
└─────────────────────────────┘
```

### NARRATION
> "Before we create UI, understand this pattern. Every UI component in Framewerk has two parts: a View and a Mediator. The View is a dumb MonoBehaviour — it just holds UI elements and fires events. The Mediator is smart — it injects services, listens for app events, and tells the View what to display. This separation keeps your UI components reusable."

**[PAUSE POINT: 7:00]**

---

## SECTION 4: Creating a View (7:00 - 11:00)

### VISUAL: Menu bar
- Click: Framewerk → Create UI Component
- Component Wizard window appears

### NARRATION
> "Let's create a welcome popup. Open the Component Wizard from the Framewerk menu."

---

### VISUAL: Component Wizard — filling fields
- Type "Welcome" in Name field
- Select "Popup" from Type dropdown
- Keep default paths
- Check "Setup in Context" is enabled
- Show Target Context dropdown (MyGameContext)

### NARRATION
> "Name it 'Welcome' and select Popup as the type. This generates a PopupView and PopupMediator instead of the basic versions. Keep 'Setup in Context' checked — this automatically adds the binding to your Context."

---

### VISUAL: Preview section
- Highlight preview showing:
  - WelcomeView.cs
  - WelcomeMediator.cs
  - WelcomePopup.prefab

### NARRATION
> "The preview shows the files that will be created — View, Mediator, and a prefab. The popup type also sets up the addressable address automatically."

---

### VISUAL: Click Generate
- Click Generate
- Unity recompiles
- Files appear in Project window

### NARRATION
> "Generate. After recompile, we have our View and Mediator ready."

---

### VISUAL: IDE — WelcomeView.cs
- Show the generated code
- Start adding UI fields

### NARRATION
> "Open the View. It's bare bones right now — just extends PopupView. Let's add our UI references."

---

### VISUAL: IDE — typing code
- Add SerializeField attributes
- Add titleText, messageText, closeButton
- Add the SetContent method

```csharp
[SerializeField] private TextMeshProUGUI titleText;
[SerializeField] private TextMeshProUGUI messageText;
[SerializeField] private Button closeButton;

public Button CloseButton => closeButton;

public void SetContent(string title, string message)
{
    titleText.text = title;
    messageText.text = message;
}
```

### NARRATION
> "We add SerializeField for our UI elements — title text, message text, and a close button. Expose the button as a property so the Mediator can subscribe to it. Add a SetContent method to update the text. Keep the View simple — it just holds references and sets values."

---

### VISUAL: IDE — WelcomeMediator.cs
- Show the generated code
- Add the IPopupManager injection
- Add button listener code

```csharp
[Inject] public IPopupManager PopupManager { get; set; }

public override void OnRegister()
{
    base.OnRegister();
    View.SetContent("Welcome!", "Thanks for trying Framewerk.");
    View.CloseButton.onClick.AddListener(OnCloseClicked);
}

public override void OnRemove()
{
    View.CloseButton.onClick.RemoveListener(OnCloseClicked);
    base.OnRemove();
}

private void OnCloseClicked()
{
    PopupManager.ClosePopup();
}
```

### NARRATION
> "Now the Mediator. Inject the PopupManager — this is how we close the popup. In OnRegister, set the content and subscribe to the button. Important: always unsubscribe in OnRemove to prevent memory leaks. The click handler simply tells the PopupManager to close."

---

### VISUAL: Unity — Prefab editing
- Open WelcomePopup.prefab
- Add Canvas children: Panel, Title, Message, Button
- Wire the SerializeField references in Inspector

### NARRATION
> "Now we build the prefab. Open WelcomePopup, add a background panel, title and message texts using TextMeshPro, and a close button. Select the root object and wire up the references in the WelcomeView component."

---

### VISUAL: Unity — Inspector
- Show the WelcomeView component
- Drag objects into SerializeField slots

### NARRATION
> "Drag each UI element into its corresponding slot. Title text goes to titleText, message goes to messageText, button goes to closeButton."

**[PAUSE POINT: 11:00]**

---

## SECTION 5: Wiring and Testing (11:00 - 13:00)

### VISUAL: IDE — MyGameContext.cs
- Scroll to mapBindings
- Show the auto-generated mediationBinder line

### NARRATION
> "Let's verify the Context. Because we checked 'Setup in Context', the wizard already added this binding — it maps WelcomeView to WelcomeMediator. When Unity instantiates a WelcomeView, Framewerk automatically creates its Mediator."

---

### VISUAL: IDE — MyGameStartCommand.cs
- Add IPopupManager injection
- Add PopupManager.OpenPopup<WelcomeView>() call

```csharp
[Inject] public IPopupManager PopupManager { get; set; }

public override void Execute()
{
    PopupManager.OpenPopup<WelcomeView>();
}
```

### NARRATION
> "To show the popup, open StartCommand. Inject the PopupManager and call OpenPopup with our View type. This tells Framewerk to load the prefab and display it."

---

### VISUAL: Unity — Press Play
- Game starts
- Welcome popup appears
- Click close button
- Popup dismisses

### NARRATION
> "Press Play... and there's our popup! Click the close button... and it's gone. We've built a complete, properly architected UI component."

**[PAUSE POINT: 13:00]**

---

## SECTION 6: Recap (13:00 - 14:00)

### VISUAL: Split screen — code files
- Show Bootstrap, Context, View, Mediator side by side

### NARRATION
> "Let's recap. The Bootstrap starts the Context. The Context binds everything together — Views to Mediators, Signals to Commands. The View holds UI elements and stays dumb. The Mediator has the logic and injects what it needs. This separation might seem like overhead for a simple popup, but as your project grows, you'll thank yourself."

---

### VISUAL: Quick reference slide
```
Framewerk → Create Scene     = Bootstrap, Context, StartCommand
Framewerk → Create UI Component = View, Mediator, Prefab

View      = UI elements, no logic
Mediator  = Logic, injected services
Context   = The wiring hub
```

### NARRATION
> "Quick reference: the Scene wizard creates your foundation. The Component wizard creates View-Mediator pairs. Views are dumb, Mediators are smart, Context ties everything together."

---

## OUTRO (14:00 - 14:30)

### VISUAL
- Return to Unity scene view
- Fade to end card: "Next: Signals, Commands & Lists"

### NARRATION
> "You now have a working Framewerk scene with proper architecture. In the next video, we'll explore Signals and Commands for decoupled communication, and create a scrollable list. See you there."

---

## B-ROLL SUGGESTIONS

| Timestamp | B-Roll |
|-----------|--------|
| 0:00-0:30 | Quick cuts of complex UIs built with Framewerk |
| 2:30 | Close-up of Package Manager package list |
| 6:00 | Animated diagram of View-Mediator pattern |
| 13:00 | Code scrolling montage |

---

## SCREEN RECORDING CHECKLIST

### Before Recording:
- [ ] Unity 2021.3+ with fresh project
- [ ] 1920×1080 resolution
- [ ] Light theme for better visibility
- [ ] Increase font size in IDE (14-16pt)
- [ ] Close unnecessary panels
- [ ] Disable notifications

### Project Settings:
- [ ] Company Name: "Demo"
- [ ] Product Name: "Framewerk Tutorial"

### Files to Have Ready:
- [ ] Git URL in clipboard
- [ ] WelcomeView code snippet
- [ ] WelcomeMediator code snippet
- [ ] StartCommand popup code snippet

---

## RECORDING NOTES

**Pacing:**
- Normal speed for explanations
- 1.5x speed for typing/waiting on compilation
- Pause 1-2 seconds before major section changes

**Voice:**
- Conversational, not robotic
- Emphasize: "dumb View, smart Mediator"
- Slight excitement when popup appears

**Mistakes to Avoid:**
- Don't show compilation errors (edit those out)
- Don't fumble with mouse (practice navigation)
- Don't read code character-by-character

---

## POST-PRODUCTION

### Edits:
- Speed up compilation waits (show progress bar at 2x)
- Add zoom effects on small UI elements
- Add code highlighting overlays
- Add chapter markers

### Audio:
- Normalize to -16 LUFS
- Remove breaths between sentences
- Add subtle background music (royalty-free, low volume)

### Captions:
- Auto-generate then review
- Include code terms exactly: "PopupView" not "popup view"
