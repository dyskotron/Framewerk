# Framewerk 2.0 Documentation Outline

## Overview

This document outlines all documentation and tutorials needed for Framewerk 2.0. It identifies what exists, what needs to be created, and suggests an order for learning.

---

## 0. FOUNDATIONS (Read First!)

**Priority: HIGHEST — This comes before any tutorials**

A conceptual overview explaining how Framewerk works. Draw heavily from StrangeIoC and Robotlegs documentation for infographics and visual explanations.

### 0.1 What is Framewerk?
- MVCS architecture overview (Model-View-Controller-Service)
- How it differs from typical Unity development
- When to use it (and when not to)

### 0.2 Dependency Injection (DI) Explained
- What is DI and why it matters
- The problem it solves (tight coupling, testability)
- Visual diagram: how dependencies flow
- Constructor vs Property injection

### 0.3 The Context — The Heart of Everything
- What is a Context? (The wiring hub)
- How bindings work: `Bind<IFoo>().To<Foo>().ToSingleton()`
- The `[Inject]` attribute
- Context lifecycle: `mapBindings()` → `postBindings()` → `Launch()`
- Visual diagram: Context wiring everything together

### 0.4 Signals & Commands — Event-Driven Architecture
- Signals as type-safe events
- Commands as single-responsibility handlers
- Signal → Command binding
- Visual diagram: Signal dispatched → Command executes

### 0.5 The Mediation Pattern
- Why separate View from logic?
- View = dumb UI, Mediator = smart controller
- How MediationBinder connects them
- Visual diagram: View ↔ Mediator ↔ Services

### 0.6 How It All Fits Together
- Full architecture diagram (StrangeIoC/Robotlegs style)
- Data flow example: User clicks button → Signal → Command → Model update → View refresh

**Resources to reference:**
- StrangeIoC docs: http://strangeioc.github.io/strangeioc/
- Robotlegs diagrams: https://github.com/robotlegs/robotlegs-framework/wiki

---

## I. TUTORIALS (Step-by-Step Guides)

Tutorials are hands-on, project-based guides that teach by doing.

### 1. Getting Started (Priority: HIGH)
**Target: Complete beginners to Framewerk**

- **1.1 Installation**
  - Installing via Package Manager (Git URL)
  - Installing via manifest.json
  - Package dependencies (Addressables)
  - Unity version requirements

- **1.2 Your First Framewerk Scene**
  - Using the Scene Wizard (Framewerk > Create Scene)
  - Understanding the generated files:
    - Bootstrap.cs (entry point)
    - Context.cs (dependency wiring)
    - StartCommand.cs (initialization logic)
  - Scene hierarchy explained:
    - Bootstrap + ViewConfig
    - Camera setup
    - EventSystem
    - UI Canvases (Main, Popups)
  - Running and verifying the app works

- **1.3 Creating Your First View**
  - Using Component Wizard (Framewerk > Create UI Component)
  - Anatomy of a View + Mediator pair
  - Adding UI elements to the View
  - Wiring UI events in the Mediator
  - Binding View to Mediator in Context

**Gaps:** No existing tutorial. Need to create from scratch with screenshots.

---

### 2. Views & Mediation Tutorial (Priority: HIGH)
**Target: Developers ready to build UI**

- **2.1 View Basics**
  - What is a View? (MonoBehaviour with UI references)
  - Extending `View` base class
  - Serialized references to UI components
  - The auto-registration lifecycle

- **2.2 Mediator Basics**
  - What is a Mediator? (Business logic, event handling)
  - Extending `ExtendedMediator<TView>`
  - `OnRegister()` and `OnRemove()` lifecycle
  - Accessing the View via injection
  - Auto-cleanup of listeners

- **2.3 UI Event Helpers**
  - `AddButtonListener()`, `AddSliderListener()`, `AddToggleListener()`
  - `AddInputListener()`, `AddPointerListener()`, `AddDragListener()`
  - Why use these over manual `.onClick.AddListener()`

- **2.4 Signal-Based Mediation with [ListensTo]**
  - The `[ListensTo(typeof(MySignal))]` attribute
  - Automatic subscription/unsubscription
  - When to use [ListensTo] vs manual signal injection

**Gaps:** No existing examples showing progression. Need clear before/after code.

---

### 3. Commands & Signals Tutorial (Priority: HIGH)
**Target: Developers building app logic**

- **3.1 Understanding Signals**
  - What are Signals? (Type-safe events)
  - Creating custom signals: `Signal`, `Signal<T>`, up to 4 params
  - Adding/removing listeners
  - Dispatching signals

- **3.2 Local vs Global (Singleton) Signals**
  - Singleton signals bound in Context
  - Local signals for component-to-component communication
  - When to use each pattern

- **3.3 Commands**
  - What is a Command? (Single-responsibility business logic)
  - Extending `Command` base class
  - The `Execute()` method
  - Injecting dependencies into Commands

- **3.4 Signal-to-Command Binding**
  - Using `commandBinder.Bind<MySignal>().To<MyCommand>()`
  - Chaining multiple commands to one signal
  - `.Once()` for one-shot commands
  - Async commands with `Retain()` and `Release()`

- **3.5 Practical Example: User Login Flow**
  - LoginButtonClickedSignal → LoginCommand
  - LoginCommand calls service, handles response
  - LoginSuccessSignal → NavigateToHomeCommand

**Gaps:** Existing demo has commands but no explanatory documentation.

---

### 4. Lists Tutorial (Priority: MEDIUM)
**Target: Developers building data-driven UIs**

- **4.1 List Anatomy**
  - ListView (container with scroll, item prefab reference)
  - ListItemView (individual item UI)
  - IListItemDataProvider (data model interface)
  - ListMediator & ListItemMediator

- **4.2 Creating a List with the Wizard**
  - Using Component Wizard to generate List scaffolding
  - Understanding generated files:
    - MyListView, MyListMediator
    - MyListItemView, MyListItemMediator
    - MyListData

- **4.3 Populating Lists**
  - `SetData(List<TData>)` method
  - Data-to-view binding in SetItemData()
  - Item click handling via `ListItemClickedSignal`

- **4.4 Selection & Multiselect**
  - `SelectItemAt()`, `UnselectItemAt()`, `UnselectAll()`
  - `Multiselect` and `Unselectable` flags
  - Accessing selected items: `GetSelectedItem()`, `GetSelectedItems()`

- **4.5 Dynamic Lists**
  - Adding/removing items
  - Object pooling behavior
  - EmptyContent display when list is empty

**Gaps:** Existing ExampleListPanel demo but no step-by-step guide.

---

### 5. Popups Tutorial (Priority: MEDIUM)
**Target: Developers building modal dialogs**

- **5.1 Popup Basics**
  - PopupView (extends View, implements IPopupView)
  - PopupMediator (extends ExtendedMediator)
  - PopupManager service

- **5.2 Creating a Popup**
  - Using Component Wizard (Type: Popup)
  - PopupView structure (button container, button prefab)
  - Dynamic button generation

- **5.3 Showing Popups**
  - `PopupManager.InstantiatePopup<T>()`
  - Async version: `InstantiatePopupAsync<T>()`
  - Passing data via PopupButtonSetting[]
  - Passing custom text/caption

- **5.4 Popup Lifecycle**
  - PopupOpenedSignal / PopupClosedSignal
  - Closing popups programmatically
  - CloseAllPopups()

- **5.5 MessageBox (Built-in)**
  - ShowMessageBoxSignal for quick messages
  - Customizing MessageBox appearance

**Gaps:** Existing ExamplePopup demo. Need cleaner documentation.

---

### 6. Screen FSM Tutorial (Priority: MEDIUM)
**Target: Developers managing app flow**

- **6.1 Understanding AppFsm**
  - What is a Finite State Machine?
  - AppFsm vs manual screen management
  - TransitionType: Enter, Exit, None

- **6.2 App States**
  - Creating states: `class MyState : AppState<MyScreen>`
  - Screen property injection
  - Enter/Exit lifecycle

- **6.3 State Transitions**
  - `Fsm.SwitchState(new MyState())`
  - Queued transitions (safe chaining)
  - AppStateEnterSignal / AppStateExitSignal

- **6.4 AppStateScreen**
  - Screen as the visual representation
  - PerformEnter() / PerformExit()
  - Async transitions

- **6.5 Practical Example: Game Flow**
  - SplashState → MenuState → GameState → GameOverState

**Gaps:** No existing tutorial. Demo has MenuState but no documentation.

---

### 7. Editor Tooling Tutorial (Priority: MEDIUM)
**Target: Developers setting up projects**

- **7.1 Scene Wizard**
  - Creating new scenes with proper structure
  - Bootstrap, Context, StartCommand generation
  - ViewConfig setup

- **7.2 Component Wizard**
  - Creating Views, Popups, Lists
  - Namespace configuration
  - Context binding auto-generation
  - Address preview

- **7.3 Addressables Integration**
  - How Framewerk uses Addressables
  - ContextPrefix ScriptableObject
  - Address format: `[Context]/[Custom]/UI/[Type]/[Name]`
  - AddressBuilder utility

- **7.4 ViewConfig**
  - Camera references
  - UI container references
  - ContextPrefixSO configuration

**Gaps:** Wizards exist but no user documentation.

---

### 8. Binding Bundles Tutorial (Priority: LOW)
**Target: Developers organizing large projects**

- **8.1 What are Binding Bundles?**
  - Reusable binding groups
  - CoreBindingBundle vs BindingBundle
  - Install/Uninstall lifecycle

- **8.2 Creating Custom Bundles**
  - Extending CoreBindingBundle
  - `OnInstall()` method
  - Tracked bindings for cleanup

- **8.3 Using Bundles in Context**
  - `InstallBundle<MyBundle>()`
  - Cooperative bindings with `BindIfMissing()`
  - FramewerkCoreBundle example

**Gaps:** CoreBindingBundle exists. Need tutorial on usage patterns.

---

## II. REFERENCE DOCUMENTATION (API & Concepts)

Reference docs are lookup-oriented, explaining what things are and how they work.

### Core Package (com.dyskotron.framewerk.core)

#### Dependency Injection
- **InjectionBinder API**
  - `Bind<T>().To<TImpl>()`
  - `.ToSingleton()`, `.ToValue()`, `.ToName()`
  - Named injections
  - Cross-context injection
- **[Inject] Attribute**
  - Field injection, property injection
  - Named injection: `[Inject("name")]`
  - Injection timing and lifecycle

#### Signals (strange.extensions.signal)
- **Signal Classes**
  - `Signal`, `Signal<T>`, `Signal<T,U>`, `Signal<T,U,V>`, `Signal<T,U,V,W>`
  - Methods: `AddListener()`, `AddOnce()`, `RemoveListener()`, `Dispatch()`
  - `RemoveAllListeners()`

#### Commands (strange.extensions.command)
- **Command Base Class**
  - `Execute()` abstract method
  - `Retain()` / `Release()` for async
  - `Fail()` for error handling
  - `Cancel()` for interruption
- **CommandBinder API**
  - `Bind<TSignal>().To<TCommand>()`
  - `.Once()`, chaining multiple commands
  - Command pooling

#### Promises (strange.extensions.promise)
- **Promise Classes**
  - `Promise`, `Promise<T>` through `Promise<T,U,V,W>`
  - `Then()` callback chaining
  - `Dispatch()` for fulfillment
  - Error handling

#### Object Pooling (strange.extensions.pool)
- **Pool API**
  - `IPoolable` interface
  - `Pool<T>` class
  - `GetInstance()`, `ReturnInstance()`
  - Overflow behavior, inflation types

#### Context (strange.extensions.context)
- **Context Lifecycle**
  - `addCoreComponents()`, `instantiateCoreComponents()`
  - `mapBindings()`, `postBindings()`
  - `Launch()`, `OnRemove()`
- **CrossContext**
  - Context communication
  - CrossContextBridge
  - ICrossContextCapable

#### Reflection & Attributes
- **[ListensTo] Attribute**
  - Auto signal subscription in Mediators
- **IReflectedClass**
  - Runtime type introspection

---

### UI Package (com.dyskotron.framewerk.ui)

#### Mediation
- **View & Mediator Lifecycle**
  - `OnRegister()`, `OnRemove()`
  - `OnEnable()`, `OnDisable()` triggers
  - MediationEvent types
- **ExtendedMediator<T>**
  - Generic View injection
  - Button/Slider/Toggle/Input listeners
  - PointerElement & DragElement handling
  - Automatic listener cleanup
- **MediationBinder API**
  - `Bind<TView>().To<TMediator>()`
  - SignalMediationBinder (default)

#### Managers
- **AssetManager**
  - Addressables wrapper
  - Sync: `GetAsset<T>()`, `LoadAsset<T>()`, `GetSprite()`, `GetTexture()`
  - Async: `GetAssetAsync<T>()`, `PreloadAssetAsync()`
  - Sprite atlas support
  - `ReleaseInstance()`, `ReleaseAsset()`
- **UiManager**
  - `InstantiateView<T>()` methods
  - Sync and async versions
  - Parent container injection
  - Address resolution via ViewConfig
- **PopupManager**
  - `InstantiatePopup<T>()` methods
  - PopupButtonSetting configuration
  - Popup tracking and CloseAllPopups()
- **CoroutineManager**
  - Coroutine execution helper

#### UI Components
- **List System**
  - ListView, ListItemView
  - ListMediator, ListItemMediator
  - IListItemDataProvider
  - Selection handling, pooling
- **Popup System**
  - PopupView, PopupMediator
  - PopupListView, PopupListMediator
  - PopupButtonSetting
  - PopupOpenedSignal, PopupClosedSignal
- **UI Helpers**
  - PointerElement (press/release events)
  - DragElement (drag events)
  - UiGradient (gradient effects)

#### Configuration
- **ViewConfig**
  - Camera references
  - UI container hierarchy
  - ContextPrefixSO for addresses
- **AddressBuilder**
  - Static address construction
  - TypeKeys constants
  - BuildAddress() methods

#### Utilities
- **Updater** (Update/FixedUpdate hook)
- **ActionUpdater** (frame-based callbacks)
- **PlayerPrefsManager**
- **BindingUtils** (temporary injection binding)
- **AppMonitor**

---

### Screen FSM Package (com.dyskotron.framewerk.screenfsm)

- **AppFsm**
  - `SwitchState()`, `CurrentState`, `CurrentTransition`
  - State queue handling
  - Destroy lifecycle
- **AppState<TScreen>**
  - Screen injection
  - `Enter()`, `Exit()`, `Destroy()`
  - `EnterFinishedSignal`, `ExitFinishedSignal`
  - Handler registration pattern
- **AppStateScreen**
  - `PerformEnter()`, `PerformExit()`
  - Visual representation of state
- **Signals**
  - AppStateEnterSignal, AppStateExitSignal

---

### Editor Package (com.dyskotron.framewerk.editor)

- **ComponentScaffoldWizard**
  - View, Popup, List creation
  - Code generation templates
  - Prefab creation post-compile
  - Context binding injection
- **SceneScaffoldWizard**
  - Scene creation with templates
  - Bootstrap/Context/Command generation
- **CodeTemplates**
  - View templates by type
  - Mediator templates
- **AddressableHelper**
  - Mark assets as Addressable
  - Address configuration
- **NamespaceResolver**
  - Auto-detect namespace from path
- **PrefabGenerator**
  - Template-based prefab creation
- **ContextInjector**
  - Auto-add bindings to Context files
- **SkinResolver / SkinConfigPostprocessor**
  - Skinning system (future)

---

## III. ARCHITECTURE & CONCEPTS

Conceptual docs explaining the "why" behind Framewerk.

### MVCS Pattern
- Model-View-Controller-Service in Unity
- How Framewerk implements MVCS
- When to use each layer

### Dependency Injection Primer
- What is DI and why use it?
- Constructor vs Property injection
- Singletons vs Transients
- Named bindings

### Event-Driven Architecture
- Signals vs Unity Events
- Type-safety benefits
- Decoupling with events

### Mediation Pattern
- Why separate View from logic?
- Benefits for testing
- Benefits for team collaboration

### StrangeIoC Heritage
- Framewerk's relationship to StrangeIoC
- What's different in Framewerk

---

## IV. EXISTING RESOURCES

### Already Documented
- `README.md` — Basic overview, installation
- `CLAUDE.md` — Architecture for AI assistance
- `Documentation/AddressableID_Structure.md` — Address format spec
- `docs/package-split-design.md` — Package structure
- `docs/skinning-system-design.md` — Skinning design doc
- Code comments in Signal.cs, Command.cs, MVCSContext.cs

### Examples That Can Be Leveraged
- `FramewerkDemo/` — Main demo scene
  - MainMenu — State machine example
  - Examples/ExamplePopup — Popup usage
  - Examples/ExampleListPanel — List usage
  - ListPopupDemo — Combined example

### Tests That Show Usage
- `Tests/Strange/extensions/` — Unit tests show API usage
  - signal/TestSignal.cs
  - command/TestCommand.cs
  - injector/TestInjector.cs
  - promise/TestPromise.cs

---

## V. SUGGESTED LEARNING ORDER

### For Beginners
1. Getting Started (Tutorial 1)
2. Views & Mediation (Tutorial 2)
3. Commands & Signals (Tutorial 3)
4. Screen FSM (Tutorial 6)

### For UI Developers
1. Getting Started (Tutorial 1)
2. Views & Mediation (Tutorial 2)
3. Lists (Tutorial 4)
4. Popups (Tutorial 5)

### For Project Leads
1. Getting Started (Tutorial 1)
2. Editor Tooling (Tutorial 7)
3. Binding Bundles (Tutorial 8)
4. Architecture Concepts

---

## VI. IDENTIFIED GAPS

### Critical (Must Have for 2.0)
- [ ] **Foundations chapter** — Conceptual overview of DI, Context, MVCS (NEW - comes first!)
- [ ] **Getting Started tutorial** — No onboarding path exists
- [x] **Wizard documentation** — Covered by tutorials that USE the wizards
- [x] **Context setup guide** — Covered in Foundations chapter
- [ ] **Signal/Command reference** — API documentation

### Important
- [ ] **List tutorial** — Demo exists, needs walkthrough
- [ ] **Popup tutorial** — Demo exists, needs walkthrough
- [ ] **FSM tutorial** — Most complex topic, needs attention
- [ ] **Architecture overview** — Explain the big picture

### Nice to Have
- [ ] **Migration guide** — From older Framewerk or StrangeIoC
- [ ] **Best practices** — Common patterns, anti-patterns
- [ ] **Troubleshooting** — Common errors and solutions
- [ ] **Video tutorials** — Screencasts of wizard usage

---

## VII. DOCUMENTATION PRIORITY

### Phase 1: Core Tutorials
1. Getting Started
2. Views & Mediation
3. Commands & Signals

### Phase 2: Feature Tutorials
4. Lists
5. Popups
6. Screen FSM

### Phase 3: Reference Docs
7. Core API reference
8. UI API reference
9. Editor tools reference

### Phase 4: Advanced
10. Binding Bundles
11. Architecture concepts
12. Best practices
