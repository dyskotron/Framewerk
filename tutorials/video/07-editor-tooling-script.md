# Chapter 7: Editor Tooling — Video Script

**Duration:** ~12 minutes  
**Format:** Screen recording with voiceover

---

## INTRO (0:00 - 0:30)

### Visual
- Empty Unity project with Framewerk imported

### Narration
> "Framewerk comes with powerful editor wizards that automate the boring parts of setting up scenes and UI components. In this chapter, we'll explore these tools and see how they enforce consistent architecture while saving you hours of repetitive work."

### B-Roll Prompt
`Unity editor, clean project view, Framewerk package visible in Packages folder`

---

## PART 1: Scene Wizard (0:30 - 3:30)

### Scene 1.1: Opening the Wizard (0:30 - 1:00)

#### Visual
- Click `Framewerk > Create Scene` menu
- Scene Wizard window opens

#### Narration
> "Let's start by creating a new scene using the Scene Wizard. Go to Framewerk, Create Scene."

> "The wizard opens with four fields: Scene Name, Namespace, Script Folder, and Scene Folder."

#### Screenshot Description
`Scene Wizard window showing all fields: Scene Name (empty), Namespace (empty), Script Folder (Assets/Scripts), Scene Folder (Assets/Scenes), with Browse buttons and Generate/Cancel buttons at bottom`

---

### Scene 1.2: Configuring the Scene (1:00 - 2:00)

#### Visual
- Type "Game" in Scene Name
- Click "Auto" button next to Namespace
- Namespace populates automatically
- Preview section shows generated files

#### Narration
> "I'll name this scene 'Game'. Notice the preview section at the bottom — it shows what files will be created."

> "For namespace, I'll click the Auto button. This derives the namespace from your folder path, automatically skipping 'Assets' and 'Scripts'."

> "The preview now shows: GameBootstrap.cs, GameContext.cs, GameStartCommand.cs, and Game.unity."

#### Screenshot Description
`Scene Wizard with "Game" entered, namespace showing "Game", preview section displaying four files that will be generated`

---

### Scene 1.3: Generation and Results (2:00 - 3:30)

#### Visual
- Click Generate button
- Console shows "Waiting for recompile..."
- Domain reload happens
- Console shows success message
- Scene opens in Hierarchy
- Show Bootstrap GameObject with components

#### Narration
> "Click Generate. The wizard creates the scripts and scene file, then triggers a domain reload. Unity needs to recompile to know about our new types."

> "After compilation, watch what happens — the wizard automatically attaches the Bootstrap component and wires up the ViewConfig reference."

> "Let's look at the scene hierarchy. We have a Bootstrap GameObject with our GameBootstrap script and a ViewConfig component. There's the 3D camera, UI camera, and all the UI container transforms we need."

#### Screenshot Description
`Hierarchy showing: Bootstrap (with GameBootstrap and ViewConfig), Camera3d, UICamera, Canvas with Container3d, UiBottom, UiDefault, Popups, UiOverlay`

---

## PART 2: Component Wizard (3:30 - 7:30)

### Scene 2.1: Creating a Popup (3:30 - 5:00)

#### Visual
- Click `Framewerk > Create UI Component`
- Component Wizard opens
- Fill in "Settings" as name
- Select "Popup" type
- Show preview updating

#### Narration
> "Now let's create a settings popup. Framewerk, Create UI Component."

> "The Component Wizard has more options. For name, I'll type 'Settings'. For type, I'll pick Popup. Notice how the preview updates — it shows SettingsView.cs, SettingsMediator.cs, and the prefab will be named SettingsPopup.prefab."

#### Screenshot Description
`Component Wizard with sections: Component (Name: "Settings", Type: Popup), Paths, Code Generation, Address Configuration. Preview shows scripts and prefab with addressable ID`

---

### Scene 2.2: Context Binding (5:00 - 5:45)

#### Visual
- Toggle "Setup in Context" checkbox
- Select GameContext from dropdown
- Show the address configuration section

#### Narration
> "Here's a time-saver — Setup in Context. When enabled, the wizard automatically injects the View-Mediator binding into your Context class."

> "I'll select GameContext as the target. The wizard will add the mediationBinder.Bind line for us."

> "Below that is Address Configuration. Target Bootstrap determines which ViewConfig to read the context prefix from."

---

### Scene 2.3: Generate and Inspect (5:45 - 7:30)

#### Visual
- Click Generate
- Wait for compilation
- Show generated scripts in Project
- Open SettingsView.cs briefly
- Show SettingsPopup.prefab in Inspector
- Open GameContext.cs, show injected binding

#### Narration
> "Click Generate, wait for compilation..."

> "Perfect. Let's see what we got. In our Scripts folder: SettingsView.cs — it extends PopupView and implements IPopupView. SettingsMediator.cs extends PopupMediator with our view type."

> "Here's the prefab — SettingsPopup. It's already marked as Addressable with the correct address: Game/UI/Popup/SettingsPopup."

> "And here's the magic — open GameContext.cs. The wizard injected this line: mediationBinder.Bind SettingsView to SettingsMediator. No manual wiring needed."

#### Screenshot Description
`GameContext.cs with highlighted line: mediationBinder.Bind<SettingsView>().To<SettingsMediator>();`

---

## PART 3: Creating a List (7:30 - 9:00)

### Visual
- Open Component Wizard again
- Enter "Player" as name
- Select "List" type
- Show expanded preview

#### Narration
> "Lists are special — they generate more files. Let me create a player list."

> "Name: Player. Type: List. Look at the preview now — we get five scripts: PlayerListView, PlayerListMediator, PlayerListData, PlayerListItemView, and PlayerListItemMediator."

> "Two prefabs: PlayerList.prefab and PlayerListItem.prefab. The list prefab references the item prefab automatically."

> "Notice the addresses: the list is in UI/List, but the item is in UI/List.ListItem. This organizational pattern keeps your addressables tidy."

#### Screenshot Description
`Component Wizard preview showing all List-related files: 5 scripts, 2 prefabs with their respective addressable IDs`

---

## PART 4: Addressables Deep Dive (9:00 - 10:30)

### Scene 4.1: Address Format (9:00 - 9:45)

#### Visual
- Show AddressBuilder.cs code
- Diagram showing address segments

#### Narration
> "Let's understand how Framewerk builds addressable IDs. The format is: ContextPrefix, CustomPrefix, UI, TypeKey, ClassName."

> "ContextPrefix comes from a ScriptableObject on your ViewConfig. CustomPrefix is optional, passed at runtime. UI is always present. TypeKey depends on component type — empty for views, 'Popup' for popups, 'List' for lists."

#### Diagram Description
```
[ContextPrefix] / [CustomPrefix] / UI / [TypeKey] / [ClassName]
     |               |                      |           |
   "Game"         optional              "Popup"   "SettingsPopup"
```

---

### Scene 4.2: ContextPrefix Setup (9:45 - 10:30)

#### Visual
- Create ContextPrefix asset
- Assign to ViewConfig
- Show wizard reading the prefix

#### Narration
> "The context prefix is a ScriptableObject. Right-click, Create, Framewerk, Context Prefix."

> "Give it a name and set the Prefix field — for example, 'Game'. Then assign this to your ViewConfig's ContextPrefixSO field."

> "Now when you use the Component Wizard, it reads this prefix from the Target Bootstrap and includes it in the addressable ID."

#### Screenshot Description
`ViewConfig Inspector showing ContextPrefixSO field with GameContextPrefix asset assigned`

---

## PART 5: ViewConfig Overview (10:30 - 11:30)

### Visual
- Select Bootstrap GameObject
- Expand ViewConfig in Inspector
- Point out each field group

#### Narration
> "ViewConfig is your scene's central configuration hub. Let's look at what it provides."

> "At the top: Context Prefix SO — we just covered this. Address Resolver is optional, for custom address patterns."

> "Cameras section: reference to your 3D camera and UI camera. The UI camera properties give you width and height for responsive layouts."

> "UI Containers: five transforms for layered UI. Container3d for world-space UI, UiBottom and UiDefault for standard elements, Popups for the popup stack, UiOverlay for always-on-top elements like notifications."

> "PopupManager uses these containers. Your UI elements instantiate into the appropriate container based on their type."

#### Screenshot Description
`ViewConfig Inspector fully expanded showing: Addressable ID Configuration (ContextPrefixSO, AddressResolver), Cameras (Camera3d, UICamera), UI Containers (Container3d, UiBottom, UiDefault, Popups, UiOverlay)`

---

## PART 6: Skin System (11:30 - 12:00)

### Visual
- Create SkinConfig asset
- Show template override fields
- Briefly show SkinResolver code

#### Narration
> "One last feature: the Skin System. If you want to override the default prefab templates, create a SkinConfig."

> "Right-click, Create, Framewerk, Skin Config. Place it at Assets/Settings/Framewerk/SkinConfig.asset for auto-discovery."

> "Assign your custom templates — maybe your popups need a different look, or your lists have a custom scroll implementation. The wizard checks for these overrides before falling back to framework defaults."

#### Screenshot Description
`SkinConfig Inspector showing: Skin Name, Description, Template Overrides (PopupTemplate, ListTemplate, ListItemTemplate, ScreenTemplate, ViewTemplate)`

---

## OUTRO (12:00 - 12:30)

### Visual
- Quick montage of all wizards
- Final scene with multiple components

### Narration
> "That's Framewerk's editor tooling. The Scene Wizard gives you a complete scene in one click. The Component Wizard generates View-Mediator pairs, prefabs, context bindings, and addressable IDs — all wired up and ready to use."

> "These tools enforce consistent architecture across your project. Use them, and you'll spend less time on boilerplate and more time on actual game logic."

> "Next chapter, we'll look at signals and commands — how Framewerk handles communication between components."

---

## B-Roll Shot List

1. `Empty Unity project, fresh import of Framewerk`
2. `Scene Wizard window, full view`
3. `Scene Wizard preview section updating as name is typed`
4. `Console showing wizard progress messages`
5. `Hierarchy view of generated scene structure`
6. `Component Wizard window, full view`
7. `Component Wizard with Popup selected, preview showing files`
8. `Generated scripts in Project window`
9. `Prefab Inspector showing addressable address`
10. `Context file with injected binding highlighted`
11. `Component Wizard with List selected, expanded preview`
12. `Diagram: Addressable address format breakdown`
13. `ContextPrefix ScriptableObject in Inspector`
14. `ViewConfig component fully expanded`
15. `SkinConfig ScriptableObject in Inspector`

---

## Key Points to Emphasize

- **One-click scene setup** — Bootstrap, Context, StartCommand, ViewConfig
- **Automatic context binding** — Less manual wiring
- **Consistent addressable format** — Predictable asset loading
- **Skin system** — Project-level template customization
- **Preferences remembered** — Paths saved between sessions
