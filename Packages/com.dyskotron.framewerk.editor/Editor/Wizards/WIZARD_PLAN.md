# UI Component Scaffolding Wizard — Design Doc

## Overview
Editor wizard that automates creation of View/Mediator pairs with prefabs and Context wiring.
Menu: **Framewerk > Create UI Component**

## Architecture
- **EditorWindow** (not ScriptableWizard — more flexible layout)
- Lives in package: `Packages/com.dyskotron.framewerk/Editor/Wizards/`
- Regex-based Context injection (no Roslyn — keeps package light)
- Auto-marks prefab as Addressable
- **Two-phase generation**: scripts first → `[DidReloadScripts]` hook completes prefab/addressable/context after compile

## Wizard Flow

### Window Layout
```
┌─ Framewerk Component Wizard ────────────────────┐
│ Component Type: [Popup         ▼]               │
│ Component Name: [Leaderboard           ]        │
│ Namespace:      [MyGame.UI.Leaderboard ]        │
│                                                  │
│ Script Folder:  [Assets/Scripts/MyGame/UI/  📁] │
│ Prefab Folder:  [Assets/Prefabs/UI/        📁] │
│                                                  │
│ Target Context: [GameContext            ▼]      │
│                                                  │
│ [✓] Mark prefab as Addressable                  │
│ [✓] Open generated scripts after creation       │
│                                                  │
│ ┌─ Preview ─────────────────────────────────┐   │
│ │ Will generate:                            │   │
│ │ • LeaderboardView.cs                      │   │
│ │ • LeaderboardMediator.cs                  │   │
│ │ • Leaderboard.prefab                      │   │
│ │ • Context binding in GameContext          │   │
│ └───────────────────────────────────────────┘   │
│                                                  │
│                       [Generate]  [Cancel]       │
└──────────────────────────────────────────────────┘
```

### Component Types
- **Screen** → View extends `View`, Mediator extends `ExtendedMediator<TView>`
- **Popup** → View extends `PopupView` + `IPopupView`, Mediator extends `PopupMediator<TView>`
- **List Panel** → View extends `ListView`, Mediator extends `ListMediator<TView, TData>` + generates Data class, ItemView, ItemMediator
- **List Item** → View extends `ListItemView`, Mediator extends `ListItemMediator<TView, TData>`
- **Custom** → Pick base classes manually

### Generation Steps (Two-Phase)

**Phase 1 (immediate):**
1. Validate inputs (name, folders, namespace)
2. Create folders if missing
3. Generate View .cs file
4. Generate Mediator .cs file
5. For List: also generate Data, ItemView, ItemMediator .cs files
6. Save pending job to EditorPrefs as JSON
7. AssetDatabase.Refresh() → triggers recompile

**Phase 2 (after recompile, via [DidReloadScripts]):**
1. Read pending job from EditorPrefs
2. Create prefab with View component attached
3. Mark as Addressable with correct address (postfix convention: `callerPath/UI/ViewName`)
4. Inject mediationBinder line into Context class
5. For List: create Item prefab, wire ItemView, inject Item binding
6. Clear pending job
7. Show success notification
8. Open scripts in IDE (if opted)

## Namespace Auto-Detection
```
Assets/Scripts/MyGame/UI/Leaderboard/ → MyGame.UI.Leaderboard
```
- Parse folder path after "Scripts/" (or "Assets/" if no Scripts)
- Replace "/" with "."
- Editable field, stores last-used in EditorPrefs

## Context Injection (Regex)
- Find last `mediationBinder.Bind<...>().To<...>();` line
- Insert new binding after it with matching indentation
- Also adds required `using` statements at top if missing
- Fallback: append before closing brace of `mapBindings()`

## Addressable Address Convention
Uses the framework's postfix mode: `{callerPath}/{TypeKey}/{viewName}`
Example: If prefab folder is `Assets/Prefabs/UI/Leaderboards/` → address derives from folder structure

## Edge Cases
- **Name conflicts**: Check existing files, warn + offer overwrite/rename
- **Context not found**: Disable context injection, show warning
- **Invalid namespace**: Validate C# identifier rules, auto-fix common mistakes
- **List type**: Generates 5+ files — preview shows all before generation
- **Addressables not initialized**: Skip marking, log warning
- **Missing folders**: Auto-create with confirmation

## File Structure
```
Packages/com.dyskotron.framewerk/
  Editor/
    com.dyskotron.framewerk.Editor.asmdef
    Wizards/
      ComponentScaffoldWizard.cs    — Main EditorWindow
      ComponentScaffoldCompleter.cs — [DidReloadScripts] hook
      WizardJob.cs                  — Serializable job data
      CodeTemplates.cs              — Template strings per type
      ContextInjector.cs            — Regex-based Context modification
      NamespaceResolver.cs          — Namespace from folder path
      PrefabGenerator.cs            — Prefab creation + View attachment
      AddressableHelper.cs          — Addressable marking
      WIZARD_PLAN.md                — This file
```

## Design Decisions
| Decision | Choice | Why |
|----------|--------|-----|
| Window type | EditorWindow | More flexible than ScriptableWizard |
| Context injection | Regex | Roslyn adds 5MB, overkill for this |
| Recompile handling | [DidReloadScripts] + EditorPrefs | Seamless, no user action needed |
| Location | In Framewerk package | Reusable across all projects |
| Prefab content | Empty GO + View script | User adds their own UI layout |
| Addressable | Optional checkbox, auto-mark | Framework convention aware |
