# Chapter 1: Getting Started with Framewerk

Welcome to Framewerk! This hands-on guide takes you from zero to a running Framewerk scene in about 15 minutes. By the end, you'll understand the core patterns and be ready to build real UI.

## What You'll Learn

- How to install Framewerk via Unity Package Manager
- Using the Scene Wizard to bootstrap a project
- Creating your first View and Mediator with the Component Wizard
- Understanding how Views, Mediators, and Contexts wire together

## Prerequisites

- Unity 2021.3 LTS or newer
- Basic C# knowledge
- Familiarity with Unity Editor

---

## 1. Installation

Framewerk is distributed as a Unity package via Git URL.

### Step 1: Open Package Manager

1. In Unity: **Window → Package Manager**
2. Click the **+** button (top left)
3. Select **Add package from git URL...**

### Step 2: Add Framewerk

Enter this URL:
```
https://github.com/dyskotron/Framewerk.git?path=Packages/com.dyskotron.framewerk
```

Click **Add**. Unity will download and import all Framewerk packages automatically.

### Step 3: Verify Dependencies

Framewerk depends on these packages (auto-installed):

| Package | Version | Purpose |
|---------|---------|---------|
| Addressables | 1.21.0+ | Asset loading |
| TextMeshPro | 3.0.0+ | Text rendering |

If prompted to install Addressables, click **Install/Upgrade**.

### Alternative: manifest.json

For teams, add directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dyskotron.framewerk": "https://github.com/dyskotron/Framewerk.git?path=Packages/com.dyskotron.framewerk"
  }
}
```

> **Tip:** Pin to a specific tag for stability:
> `...Framewerk.git?path=Packages/com.dyskotron.framewerk#v2.0.0`

---

## 2. Your First Framewerk Scene

Framewerk uses a Scene Wizard to generate the boilerplate files every project needs. Let's create your first scene.

### Step 1: Open the Scene Wizard

Go to **Framewerk → Create Scene** in the menu bar.

```
┌─────────────────────────────────────┐
│    Framewerk Scene Wizard           │
├─────────────────────────────────────┤
│ Scene Name:     [MyGame           ] │
│ Namespace:      [MyGame           ] │
│ Script Folder:  [Assets/Scripts   ] │
│ Scene Folder:   [Assets/Scenes    ] │
├─────────────────────────────────────┤
│ Preview:                            │
│  • MyGameBootstrap.cs               │
│  • MyGameContext.cs                 │
│  • MyGameStartCommand.cs            │
│  • MyGame.unity                     │
├─────────────────────────────────────┤
│     [Generate]      [Cancel]        │
└─────────────────────────────────────┘
```

### Step 2: Configure Your Scene

| Field | Value | Notes |
|-------|-------|-------|
| Scene Name | `MyGame` | PascalCase, no spaces |
| Namespace | `MyGame` | Auto-detect with "Auto" button |
| Script Folder | `Assets/Scripts` | Where C# files go |
| Scene Folder | `Assets/Scenes` | Where .unity file goes |

### Step 3: Generate

Click **Generate**. Unity compiles, then the wizard:
1. Creates the scene file
2. Generates three C# files
3. Sets up the scene hierarchy
4. Adds required components

### Understanding the Generated Files

#### MyGameBootstrap.cs — The Entry Point

```csharp
using Plugins.Framewerk;
using strange.extensions.context.impl;
using UnityEngine;

namespace MyGame
{
    public class MyGameBootstrap : ContextView
    {
        public ViewConfig viewConfig;

        private MyGameContext _context;

        private void Start()
        {
            if (viewConfig == null)
            {
                Debug.LogError("[MyGame] ViewConfig is not assigned!");
                return;
            }

            _context = new MyGameContext(this, viewConfig);
            _context.Start();
        }

        private void OnApplicationQuit()
        {
            _context?.OnRemove();
        }
    }
}
```

**What it does:**
- Attached to a root GameObject in the scene
- Creates and starts the Context
- References `ViewConfig` (asset loading configuration)

#### MyGameContext.cs — The Binding Hub

```csharp
using Framewerk;
using Framewerk.StrangeCore;
using Plugins.Framewerk;
using strange.extensions.context.impl;

namespace MyGame
{
    public class MyGameContext : FramewerkMVCSContext
    {
        private readonly ViewConfig _viewConfig;

        public MyGameContext(ContextView view, ViewConfig viewConfig) 
            : base(view, true)
        {
            _viewConfig = viewConfig;
        }

        protected override void mapBindings()
        {
            base.mapBindings();

            // Core bindings
            injectionBinder.Bind<ViewConfig>().ToValue(_viewConfig);

            // Commands
            commandBinder.Bind<ContextStartSignal>().To<MyGameStartCommand>();
        }
    }
}
```

**What it does:**
- Central configuration for dependency injection
- Binds interfaces to implementations
- Maps Views to Mediators
- Connects Signals to Commands

#### MyGameStartCommand.cs — Initialization Logic

```csharp
using Plugins.Framewerk;
using strange.extensions.command.impl;

namespace MyGame
{
    public class MyGameStartCommand : Command
    {
        [Inject] public ViewConfig ViewConfig { get; set; }

        public override void Execute()
        {
            // Initialize your app here
        }
    }
}
```

**What it does:**
- Runs once when the Context starts
- Perfect place for loading screens, initial navigation, etc.

### The Scene Hierarchy

After generation, your scene looks like this:

```
MyGame.unity
├── Main Camera
├── EventSystem
├── MyGame                    ← Bootstrap component lives here
│   └── ViewConfig           ← Asset loading configuration
└── Canvas                   ← Your UI goes here
    └── ... views ...
```

### Step 4: Test the Scene

1. Open `Assets/Scenes/MyGame.unity`
2. Press **Play**
3. Check Console — no errors means success!

---

## 3. Creating Your First View

Now let's add actual UI. We'll create a simple "Welcome" popup using the Component Wizard.

### The View-Mediator Pattern

Before we code, understand this core pattern:

```
┌──────────────────────────────────────────────────────────┐
│                        MEDIATOR                          │
│  • Has access to injected services                       │
│  • Listens to Signals from other parts of the app        │
│  • Sends Signals to notify the app                       │
│  • Reads from / writes to the View                       │
└────────────────────────┬─────────────────────────────────┘
                         │ references
                         ▼
┌──────────────────────────────────────────────────────────┐
│                         VIEW                             │
│  • MonoBehaviour on the prefab                           │
│  • Exposes UI elements (buttons, texts, images)          │
│  • Fires events (button clicks)                          │
│  • Knows NOTHING about the rest of the app               │
└──────────────────────────────────────────────────────────┘
```

**Key insight:** The View is dumb. The Mediator is smart. This keeps your UI components reusable.

### Step 1: Open the Component Wizard

Go to **Framewerk → Create UI Component**

```
┌─────────────────────────────────────┐
│  Framewerk Component Wizard         │
├─────────────────────────────────────┤
│ Component                           │
│  Name:  [Welcome                  ] │
│  Type:  [Popup ▼]                   │
├─────────────────────────────────────┤
│ Paths                               │
│  Script Folder:  [Assets/Scripts  ] │
│  Prefab Folder:  [Assets/Prefabs  ] │
├─────────────────────────────────────┤
│ Code Generation                     │
│  Namespace:      [MyGame          ] │
│  Setup in Context: [✓]             │
│    Target Context: [MyGameContext ] │
├─────────────────────────────────────┤
│ Preview:                            │
│  Scripts:                           │
│    WelcomeView.cs                   │
│    WelcomeMediator.cs               │
│  Prefabs:                           │
│    WelcomePopup.prefab → popup/Welcome │
└─────────────────────────────────────┘
```

### Step 2: Configure the Component

| Field | Value | Notes |
|-------|-------|-------|
| Name | `Welcome` | Base name (Popup suffix added automatically) |
| Type | `Popup` | Creates PopupView and PopupMediator |
| Script Folder | `Assets/Scripts` | Same as scene scripts |
| Prefab Folder | `Assets/Prefabs` | Where prefab is created |
| Namespace | `MyGame` | Match your scene namespace |
| Setup in Context | ✓ | Auto-adds bindings to Context |
| Target Context | `MyGameContext` | Select your context |

### Step 3: Generate

Click **Generate**. The wizard creates:

```
Assets/
├── Scripts/
│   ├── WelcomeView.cs
│   └── WelcomeMediator.cs
└── Prefabs/
    └── WelcomePopup.prefab
```

And adds to `MyGameContext.cs`:
```csharp
mediationBinder.Bind<WelcomeView>().To<WelcomeMediator>();
```

### Understanding the Generated Code

#### WelcomeView.cs — The UI Layer

```csharp
using Framewerk.Popups;

namespace MyGame
{
    public class WelcomeView : PopupView, IPopupView
    {
    }
}
```

This is the base. Let's add UI references:

```csharp
using Framewerk.Popups;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MyGame
{
    public class WelcomeView : PopupView, IPopupView
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button closeButton;

        public Button CloseButton => closeButton;

        public void SetContent(string title, string message)
        {
            titleText.text = title;
            messageText.text = message;
        }
    }
}
```

**Key principles:**
- UI elements are `[SerializeField] private` (set in Inspector)
- Expose only what the Mediator needs
- Keep methods simple — set data, not logic

#### WelcomeMediator.cs — The Logic Layer

```csharp
using Framewerk.Popups;

namespace MyGame
{
    public class WelcomeMediator : PopupMediator<WelcomeView>
    {
        public override void OnRegister()
        {
            base.OnRegister();
        }
    }
}
```

Let's wire up the button:

```csharp
using Framewerk.Popups;

namespace MyGame
{
    public class WelcomeMediator : PopupMediator<WelcomeView>
    {
        [Inject] public IPopupManager PopupManager { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            // Set content
            View.SetContent("Welcome!", "Thanks for trying Framewerk.");

            // Wire button
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
    }
}
```

**Key principles:**
- Inject dependencies via `[Inject]` attribute
- Subscribe in `OnRegister()`, unsubscribe in `OnRemove()`
- Call View methods to update UI

### Step 4: Setup the Prefab

1. Open `Assets/Prefabs/WelcomePopup.prefab`
2. Add UI elements under the root:
   - Background panel
   - Title text (TextMeshPro)
   - Message text (TextMeshPro)
   - Close button (Button)
3. Wire references in the `WelcomeView` component

### Step 5: Show the Popup

Open `MyGameStartCommand.cs` and trigger the popup:

```csharp
using Framewerk.Popups;
using Plugins.Framewerk;
using strange.extensions.command.impl;

namespace MyGame
{
    public class MyGameStartCommand : Command
    {
        [Inject] public ViewConfig ViewConfig { get; set; }
        [Inject] public IPopupManager PopupManager { get; set; }

        public override void Execute()
        {
            // Show welcome popup when game starts
            PopupManager.OpenPopup<WelcomeView>();
        }
    }
}
```

### Step 6: Run It!

1. Press Play
2. The Welcome popup should appear
3. Click Close — popup dismisses

**Congratulations!** You've built your first Framewerk scene with a working popup.

---

## Quick Reference

### Wizard Locations

| Wizard | Menu Path | Purpose |
|--------|-----------|---------|
| Scene Wizard | Framewerk → Create Scene | Bootstrap new project |
| Component Wizard | Framewerk → Create UI Component | Create View/Mediator pairs |

### Common Component Types

| Type | Base Classes | Use Case |
|------|--------------|----------|
| Popup | PopupView, PopupMediator | Modal dialogs |
| View | View, ExtendedMediator | Any UI component |
| List | ListView, ListMediator | Scrollable item lists |

### Binding Cheat Sheet

```csharp
// In Context.mapBindings():

// View → Mediator (UI components)
mediationBinder.Bind<MyView>().To<MyMediator>();

// Interface → Implementation (services)
injectionBinder.Bind<IMyService>().To<MyService>().ToSingleton();

// Signal → Command (event handling)
commandBinder.Bind<MySignal>().To<MyCommand>();

// Signal only (for manual dispatch)
injectionBinder.Bind<MySignal>().ToSingleton();
```

---

## What's Next?

In the next chapter, we'll dive deeper into:
- The Signal/Command pattern for decoupled communication
- Creating List components with data binding
- The App State Machine for screen navigation

You now have the foundation. Let's build on it!
