# Framewerk 2.0 TODO

## Must-have before 2.0

- [ ] **1. UI Asset Scaffolding** — Auto-create View/Mediator pairs with prefab, lists with items and data classes etc. Editor tooling to scaffold new UI components quickly.
  - [x] Wizard window (ComponentScaffoldWizard) — context discovery, component type selection, name input, path config
  - [x] Code templates (CodeTemplates.cs) — Panel, List, Popup templates
  - [x] Completer (ComponentScaffoldCompleter.cs) — post-compile asset creation + Addressable setup
  - [x] WizardList example — 3rd menu item to test wizard List output
  - [x] Fix Addressable address mismatches (postfix mode)
  - [ ] **Blueprint prefabs** — Base template prefabs for each component type (List, Popup, Panel, etc.) shipped with Framewerk. Use base View scripts with serialized references (itemContainer, label, button, etc.). Wizard clones blueprint, generates real View script, swaps component — Unity preserves field references since names match.
  - [ ] **Skinning system** — SkinConfig ScriptableObject maps component types → blueprint prefab overrides. Project creates custom SkinConfig to replace framework defaults. No SkinConfig = use framework blueprints. Wizard reads active skin when scaffolding.
  - [ ] Wizard creates prefab with real components (not empty GameObjects)
- [ ] **2. Addressables Setup Streamlining** — Think through the Addressables setup in projects that use Framewerk. Can we make onboarding smoother for users? Auto-setup, conventions, editor wizards?
- [x] **3. Popup Info Passing Cleanup** — BindingUtils extracted, auto-binds interfaces (on) and base classes (opt-in). ExplicitType overloads kept for edge cases.
- [ ] **4. Port UNet to Mirror** — Port the networking package from UNet to Mirror. Make it a proper optional package.
- [ ] **5. Split Examples** — Each example self-contained with its own scene. No monolithic demo — isolated examples easy to understand and reference.
- [ ] **6. Modular Package Split** — Split optional functionality into own packages: Core (IoC, FSM, commands), UI (UiManager, screens, popups), Networking (Mirror-based), etc. Users install only what they need.
