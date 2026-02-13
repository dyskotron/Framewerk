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

## MCP Commands for Remaining Setup

### ListExample
```bash
# 1. Create scene
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {"action": "create_scene", "name": "ListExample", "namespace": "Framewerk.Examples.ListExample", "output_path": "Assets/Examples/ListExample"}}'

# 2. Create list prefabs  
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {"action": "create_list", "name": "ListPanel", "item_name": "ContactItem", "namespace": "Framewerk.Examples.ListExample", "output_path": "Assets/Examples/ListExample/Prefabs"}}'

# 3. Mark addressable
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {"action": "mark_addressable", "prefab_path": "Assets/Examples/ListExample/Prefabs/ListPanel.prefab", "address": "ListExample/ListPanel"}}'
```

### TabsViewStackExample
```bash
# 1. Create scene
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {"action": "create_scene", "name": "TabsViewStackExample", "namespace": "Framewerk.Examples.TabsViewStackExample", "output_path": "Assets/Examples/TabsViewStackExample"}}'

# 2. Create tabs list prefabs
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {"action": "create_list", "name": "TabsPanel", "item_name": "TabItem", "namespace": "Framewerk.Examples.TabsViewStackExample", "output_path": "Assets/Examples/TabsViewStackExample/Prefabs"}}'

# 3. Mark addressable
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {"action": "mark_addressable", "prefab_path": "Assets/Examples/TabsViewStackExample/Prefabs/TabsPanel.prefab", "address": "TabsViewStackExample/TabsPanel"}}'
```

### ScreenFsmExample
```bash
# 1. Create scene
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {"action": "create_scene", "name": "ScreenFsmExample", "namespace": "Framewerk.Examples.ScreenFsmExample", "output_path": "Assets/Examples/ScreenFsmExample"}}'

# 2. Create MainMenu list
python3 ~/clawd/scripts/unity_mcp.py call execute_custom_tool '{"tool_name": "framewerk_scaffold", "parameters": {"action": "create_list", "name": "MainMenu", "item_name": "MenuItem", "namespace": "Framewerk.Examples.ScreenFsmExample", "output_path": "Assets/Examples/ScreenFsmExample/Prefabs"}}'

# 3. Create view prefabs for each screen
# (Settings, Game, Leaderboards, About - use create_view action)

# 4. Mark all addressable
```

---

## Notes

- MCP server must be running in Unity for scene/prefab creation
- Each example needs ContextPrefixSO in Settings folder
- Scripts are complete and should compile once Unity refreshes
- See individual README.md files in each example folder for detailed setup
