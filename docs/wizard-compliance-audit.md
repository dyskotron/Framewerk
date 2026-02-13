# Framewerk Examples - Wizard Compliance Audit

**Date:** 2025-02-12  
**Status:** PLANNING MODE - Analysis only, no changes made

## Summary

| Example | Naming | Prefabs | Addressables | Overall |
|---------|--------|---------|--------------|---------|
| ListExample | ✅ | ✅ | ⚠️ Partial | ⚠️ |
| PopupExample | ✅ | ✅ | ✅ | ✅ |
| TabsViewStackExample | ❌ | ⚠️ | ❌ | ❌ |
| ScreenFsmExample | ❌ | ⚠️ | ❌ | ❌ |

---

## 1. ListExample ⚠️ Partial Compliance

### Naming Conventions ✅ COMPLIANT
All components follow wizard naming patterns:
- `ListPanelListView.cs` ✅
- `ListPanelListMediator.cs` ✅
- `ListPanelListItemView.cs` ✅
- `ListPanelListItemMediator.cs` ✅
- `ListPanelListData.cs` ✅

### Prefab Naming ✅ COMPLIANT
- `ListPanelList.prefab` ✅
- `ListPanelListItem.prefab` ✅

### Addressables ⚠️ PARTIAL
| Prefab | Registered | Address | Issue |
|--------|------------|---------|-------|
| ListPanelList.prefab | ✅ | `UI/ListPanelList` | OK |
| ListPanelListItem.prefab | ❌ | - | **Missing** - should be `UI/List.ListItem/ListPanelListItem` |

### Recommended Fixes
1. Register `ListPanelListItem.prefab` in Addressables with key `UI/List.ListItem/ListPanelListItem`

---

## 2. PopupExample ✅ COMPLIANT

### Naming Conventions ✅ COMPLIANT
- `BasicPopupView.cs` ✅
- `BasicPopupMediator.cs` ✅

### Prefab Naming ✅ COMPLIANT
- `BasicPopup.prefab` ✅ (correct - wizard would produce this for popup named "Basic")

### Addressables ✅ COMPLIANT
| Prefab | Registered | Address |
|--------|------------|---------|
| BasicPopup.prefab | ✅ | `PopupExample/UI/Popup/BasicPopup` |

### Notes
- Has proper ContextPrefix ScriptableObject (`PopupExamplePrefix.asset`)
- This is the gold standard for example compliance

---

## 3. TabsViewStackExample ❌ NON-COMPLIANT

### Naming Conventions ❌ ISSUES FOUND

| Current Name | Expected (Wizard Pattern) | Issue |
|--------------|---------------------------|-------|
| `TabData.cs` | `TabsPanelData.cs` | Wrong prefix, should match container name |
| `TabItemView.cs` | `TabsPanelItemView.cs` | Wrong prefix |
| `TabItemMediator.cs` | `TabsPanelItemMediator.cs` | Wrong prefix |
| `TabsPanelView.cs` | ✅ | OK |
| `TabsPanelMediator.cs` | ✅ | OK |

### Prefab Naming ⚠️ MINOR ISSUE
| Current | Expected | Issue |
|---------|----------|-------|
| `TabsPanel.prefab` | OK | Container prefabs don't require View suffix |
| `TabsPanelItem.prefab` | ✅ | OK |

### Addressables ❌ NOT REGISTERED
| Prefab | Expected Address |
|--------|------------------|
| TabsPanel.prefab | `UI/Tabs/TabsPanel` |
| TabsPanelItem.prefab | `UI/Tabs.TabItem/TabsPanelItem` |

### Recommended Fixes
1. Rename `TabData.cs` → `TabsPanelData.cs`
2. Rename `TabItemView.cs` → `TabsPanelItemView.cs`
3. Rename `TabItemMediator.cs` → `TabsPanelItemMediator.cs`
4. Update class names inside files to match
5. Update references in mediators (generics use data/item types)
6. Rename prefab root GameObjects if they use old names
7. Register both prefabs in Addressables with proper keys

---

## 4. ScreenFsmExample ❌ NON-COMPLIANT

### Naming Conventions ❌ ISSUES FOUND

**MainMenu List Components:**
| Current Name | Expected (Wizard Pattern) | Issue |
|--------------|---------------------------|-------|
| `MenuItemData.cs` | `MainMenuData.cs` | Wrong prefix |
| `MenuItemView.cs` | `MainMenuItemView.cs` | Wrong prefix |
| `MenuItemMediator.cs` | `MainMenuItemMediator.cs` | Wrong prefix |
| `MainMenuView.cs` | ✅ | OK |
| `MainMenuMediator.cs` | ✅ | OK |

**Other Views (OK):**
- `AboutView.cs` / `AboutMediator.cs` ✅
- `GameView.cs` / `GameMediator.cs` ✅
- `SettingsView.cs` / `SettingsMediator.cs` ✅
- `LeaderboardsView.cs` / `LeaderboardsMediator.cs` ✅

### Prefab Naming ⚠️ INCONSISTENT
| Prefab | Issue |
|--------|-------|
| `MainMenu.prefab` | Should be `MainMenuView.prefab` for consistency |
| `MainMenuItem.prefab` | ✅ OK |
| `AboutView.prefab` | ✅ OK |
| `GameView.prefab` | ✅ OK |
| `SettingsView.prefab` | ✅ OK |
| `LeaderboardsView.prefab` | ✅ OK |

### Addressables ❌ NONE REGISTERED
All 6 prefabs need Addressable registration:

| Prefab | Expected Address |
|--------|------------------|
| MainMenu.prefab | `UI/List/MainMenu` or `UI/MainMenuView` |
| MainMenuItem.prefab | `UI/List.ListItem/MainMenuItem` |
| AboutView.prefab | `UI/AboutView` |
| GameView.prefab | `UI/GameView` |
| SettingsView.prefab | `UI/SettingsView` |
| LeaderboardsView.prefab | `UI/LeaderboardsView` |

### Recommended Fixes
1. Rename `MenuItemData.cs` → `MainMenuData.cs`
2. Rename `MenuItemView.cs` → `MainMenuItemView.cs`
3. Rename `MenuItemMediator.cs` → `MainMenuItemMediator.cs`
4. Update class names inside files to match
5. Consider renaming `MainMenu.prefab` → `MainMenuView.prefab` for consistency
6. Register all prefabs in Addressables with proper keys
7. Create ContextPrefix ScriptableObject if the example should use one

---

## Addressables Summary

### Currently Registered (2 prefabs)
```
PopupExample/UI/Popup/BasicPopup  → BasicPopup.prefab
UI/ListPanelList                  → ListPanelList.prefab
```

### Missing Registration (10 prefabs)
1. `ListPanelListItem.prefab` → `UI/List.ListItem/ListPanelListItem`
2. `TabsPanel.prefab` → `UI/Tabs/TabsPanel`
3. `TabsPanelItem.prefab` → `UI/Tabs.TabItem/TabsPanelItem`
4. `MainMenu.prefab` → `UI/List/MainMenu`
5. `MainMenuItem.prefab` → `UI/List.ListItem/MainMenuItem`
6. `AboutView.prefab` → `UI/AboutView`
7. `GameView.prefab` → `UI/GameView`
8. `SettingsView.prefab` → `UI/SettingsView`
9. `LeaderboardsView.prefab` → `UI/LeaderboardsView`
10. `PopupButton.prefab` → (optional, only if loaded dynamically)

---

## Wizard Naming Pattern Reference

Based on `ComponentScaffoldWizard.cs`, the wizard generates:

### For List type (input: "Foo")
- Container: `FooListView.cs`, `FooListMediator.cs`, `FooList.prefab`
- Item: `FooListItemView.cs`, `FooListItemMediator.cs`, `FooListItem.prefab`
- Data: `FooListData.cs`

### For Tabs type (input: "Foo")
- Container: `FooView.cs`, `FooMediator.cs`, `Foo.prefab`
- Item: `FooItemView.cs`, `FooItemMediator.cs`, `FooItem.prefab`
- Data: `FooData.cs`

### For Popup type (input: "Foo")
- `FooPopupView.cs`, `FooPopupMediator.cs`, `FooPopup.prefab`

### For View type (input: "Foo")
- `FooView.cs`, `FooMediator.cs`, `Foo.prefab` or `FooView.prefab`

---

## Next Steps

1. **Priority 1:** Fix ListExample Addressables (add missing ListItem)
2. **Priority 2:** Fix TabsViewStackExample naming + Addressables
3. **Priority 3:** Fix ScreenFsmExample naming + Addressables
4. Consider adding ContextPrefix SOs to examples that lack them
