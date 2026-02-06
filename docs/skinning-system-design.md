# Skinning System Design

## Overview

The skinning system lets projects override the default UI template prefabs used by the Component Scaffold Wizard. Instead of always using Framewerk's built-in templates, projects can define their own styled versions.

---

## How Override Works

### The Problem

Currently, when you create a Popup via the wizard, it always clones:
```
Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/PopupTemplate.prefab
```

Every project gets the same base look. To customize, you'd have to manually edit every generated prefab.

### The Solution: SkinConfig Override

**Step 1: Framework has defaults**
```
Packages/com.dyskotron.framewerk/Editor/Wizards/Templates/
├── PopupTemplate.prefab      ← Framework default
├── ListTemplate.prefab       ← Framework default
├── ListItemTemplate.prefab   ← Framework default
└── ViewTemplate.prefab       ← Framework default
```

**Step 2: Project creates SkinConfig**
```
Assets/Settings/Framewerk/SkinConfig.asset
```

This ScriptableObject has fields for each template type:
```
SkinConfig
├── PopupTemplate:    MyStyledPopup.prefab    ← OVERRIDE
├── ListTemplate:     MyStyledList.prefab     ← OVERRIDE
├── ListItemTemplate: (empty)                  ← USE DEFAULT
└── ViewTemplate:     (empty)                  ← USE DEFAULT
```

**Step 3: Wizard checks SkinConfig first**

When wizard needs a template, it asks SkinResolver:

```
GetTemplatePath(ComponentType.Popup)
    │
    ▼
┌─────────────────────────────┐
│ Does SkinConfig exist?      │
└─────────────────────────────┘
         │
        YES
         │
         ▼
┌─────────────────────────────┐
│ Does SkinConfig.PopupTemplate│
│ have a prefab assigned?      │
└─────────────────────────────┘
         │
        YES → Return "Assets/Prefabs/MyStyledPopup.prefab"
         │
        NO  → Return "Packages/.../Templates/PopupTemplate.prefab"
```

**Result:** Project's popup template is used, framework's list template is used (because ListItemTemplate was empty in SkinConfig).

---

## Override Rules

| SkinConfig State | Template Field State | Result |
|------------------|---------------------|--------|
| No SkinConfig exists | - | Framework default |
| SkinConfig exists | Field has prefab | **Project override** |
| SkinConfig exists | Field is empty/null | Framework default |

This allows **partial overrides** — customize only what you need.

---

## Template Compatibility Requirements

For override to work, your custom template must be **structurally compatible** with the framework default:

1. **Same base View component** — Popup templates need `PopupView`, List templates need `ListView`, etc. The wizard swaps this component with your generated one, preserving serialized field references.

2. **Same child object names** — If framework's PopupTemplate has a child called "Content", yours should too. The wizard may reference these by name.

3. **Same serialized fields on the View** — Field names must match so Unity can copy values when swapping components.

**Example — Valid override:**
```
Framework PopupTemplate:
├── PopupView (component)
├── Background
├── Content
└── CloseButton

Your MyStyledPopup:
├── PopupView (component)      ← Same component type ✓
├── Background                  ← Same name ✓ (can be styled differently)
├── Content                     ← Same name ✓
├── CloseButton                 ← Same name ✓
└── FancyBorder                 ← Extra stuff is fine ✓
```

---

## Open Questions

1. **Where exactly does SkinConfig live?**
   - Conventional path: `Assets/Settings/Framewerk/SkinConfig.asset`
   - Or scan entire project for any `SkinConfig` asset?
   - Or both (check conventional first, then scan)?

2. **What if user has multiple SkinConfigs?**
   - Error?
   - Use first found + warn?
   - Add a "SkinRegistry" that points to the active one?

3. **Should wizard UI show which skin is active?**
   - Nice for debugging
   - Shows "Using: MyProjectSkin" or "Using: Framework Defaults"

4. **Runtime vs Editor-time?**
   - Skinning is purely editor-time (wizard uses it to pick template)
   - No runtime impact — prefabs are just prefabs once created

---

## Next Steps

- [ ] Finalize SkinConfig fields and structure
- [ ] Decide on discovery mechanism (conventional path vs scan vs both)
- [ ] Implement SkinResolver
- [ ] Update ComponentScaffoldCompleter to use SkinResolver
- [ ] Optional: Wizard UI enhancement

