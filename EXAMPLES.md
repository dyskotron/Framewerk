# Framewerk Examples Plan

## Structure

Each example lives in its own dedicated folder with everything self-contained:
```
Assets/Examples/
├── PopupExample/
│   ├── Scenes/
│   │   └── PopupExample.unity
│   ├── Prefabs/
│   ├── Scripts/
│   ├── Settings/
│   └── README.md
├── ListExample/
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Scripts/
│   ├── Settings/
│   └── README.md
├── TabsViewStackExample/
│   ├── Scenes/
│   ├── Prefabs/
│   ├── Scripts/
│   ├── Settings/
│   └── README.md
└── ScreenFsmExample/
    ├── Scenes/
    ├── Prefabs/
    ├── Scripts/
    │   ├── Screens/
    │   ├── States/
    │   └── Views/
    ├── Settings/
    └── README.md
```

---

## Examples Overview

### 1. Popup Example ✅
- [x] Basic popup showing/hiding
- [x] Scene, Bootstrap, Context, StartCommand
- [x] BasicPopupView/Mediator
- [x] Prefab created and addressable

### 2. List Example 📝
- [x] Scripts created (8 files)
- [ ] Scene creation via MCP
- [ ] Prefabs (ListPanel, ContactItem)
- [ ] Addressable setup
- [ ] ContextPrefixSO

Shows: Data list, selection, dynamic add/remove

### 3. Tabs + ViewStack Example 📝
- [x] Scripts created (8 files)
- [ ] Scene creation via MCP
- [ ] Prefabs (TabsPanel, TabItem)
- [ ] ViewStack integration
- [ ] Addressable setup
- [ ] ContextPrefixSO

Shows: Horizontal tabs controlling a ViewStack

### 4. Screen FSM Example 📝
- [x] Scripts created (27 files)
- [ ] Scene creation via MCP
- [ ] Prefabs (MainMenu, MenuItem, Settings, Game, Leaderboards, About)
- [ ] Addressable setup
- [ ] ContextPrefixSO

Shows: Full navigation flow with state machine
- MainMenuScreen → Settings/Game/Leaderboards/About
- Each screen has Back button → returns to MainMenu

---

## Progress

| Example | Scripts | Scene | Prefabs | Addressable | Status |
|---------|---------|-------|---------|-------------|--------|
| Popup | ✅ | ✅ | ✅ | ✅ | **Done** |
| List | ✅ | ⬜ | ⬜ | ⬜ | Scripts ready |
| Tabs + ViewStack | ✅ | ⬜ | ⬜ | ⬜ | Scripts ready |
| Screen FSM | ✅ | ⬜ | ⬜ | ⬜ | Scripts ready |

---

## Scaffold Commands for Remaining Setup

Each step passes a JSON parameter object to `Framewerk.Editor.Wizards.FramewerkScaffold.Execute` through the Unity CLI. Define a helper once per shell:

```bash
export PATH="/c/Users/$USER/AppData/Local/Unity/bin:$PATH"
scaffold() { unity command eval --project-path "$(pwd)" --result-only --timeout 60000 "return Framewerk.Editor.Wizards.FramewerkScaffold.Execute(@\"$(printf '%s' "$1" | sed 's/"/""/g')\");"; }
```

### ListExample
```bash
# 1. Create scene (recompile afterwards)
scaffold '{"action":"create_scene","sceneName":"ListExample","namespace":"Framewerk.Examples.ListExample","sceneFolder":"Assets/Examples/ListExample/Scenes","scriptFolder":"Assets/Examples/ListExample/Scripts"}'

# 2. Create list prefabs (View scripts must exist and compile first)
scaffold '{"action":"create_list","name":"ContactList","namespace":"Framewerk.Examples.ListExample","viewTypeName":"Framewerk.Examples.ListExample.ContactListView","itemViewTypeName":"Framewerk.Examples.ListExample.ContactListItemView","prefabFolder":"Assets/Examples/ListExample/Prefabs"}'

# 3. Mark addressable
scaffold '{"action":"mark_addressable","prefabPath":"Assets/Examples/ListExample/Prefabs/ContactList.prefab","address":"ListExample/UI/ContactList"}'
```

### TabsViewStackExample
```bash
# 1. Create scene
scaffold '{"action":"create_scene","sceneName":"TabsViewStackExample","namespace":"Framewerk.Examples.TabsViewStackExample","sceneFolder":"Assets/Examples/TabsViewStackExample/Scenes","scriptFolder":"Assets/Examples/TabsViewStackExample/Scripts"}'

# 2. Create tabs prefabs
scaffold '{"action":"create_tabs","name":"TabContainer","namespace":"Framewerk.Examples.TabsViewStackExample","viewTypeName":"Framewerk.Examples.TabsViewStackExample.TabContainerView","itemViewTypeName":"Framewerk.Examples.TabsViewStackExample.TabItemView","prefabFolder":"Assets/Examples/TabsViewStackExample/Prefabs","orientation":"horizontal"}'

# 3. Mark addressable
scaffold '{"action":"mark_addressable","prefabPath":"Assets/Examples/TabsViewStackExample/Prefabs/TabContainer.prefab","address":"TabsViewStackExample/UI/TabContainer"}'
```

### ScreenFsmExample
```bash
# 1. Create scene
scaffold '{"action":"create_scene","sceneName":"ScreenFsmExample","namespace":"Framewerk.Examples.ScreenFsmExample","sceneFolder":"Assets/Examples/ScreenFsmExample/Scenes","scriptFolder":"Assets/Examples/ScreenFsmExample/Scripts"}'

# 2. Create MainMenu list
scaffold '{"action":"create_list","name":"MainMenu","namespace":"Framewerk.Examples.ScreenFsmExample","viewTypeName":"Framewerk.Examples.ScreenFsmExample.MainMenuView","itemViewTypeName":"Framewerk.Examples.ScreenFsmExample.MainMenuItemView","prefabFolder":"Assets/Examples/ScreenFsmExample/Prefabs"}'

# 3. Create view prefabs for each screen (Settings, Game, Leaderboards, About) with create_view

# 4. Mark all addressable
```

---

## Notes

- MCP server must be running in Unity for scene/prefab creation
- Each example needs ContextPrefixSO in Settings folder
- Scripts are complete and should compile once Unity refreshes
- See individual README.md files in each example folder for detailed setup
