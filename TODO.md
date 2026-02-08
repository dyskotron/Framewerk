# Framewerk 2.0 TODO

## Must-have before 2.0

- [x] **1. UI Asset Scaffolding** — Auto-create View/Mediator pairs with prefab, lists with items and data classes etc. Editor tooling to scaffold new UI components quickly.
  - [x] Wizard window (ComponentScaffoldWizard) — context discovery, component type selection, name input, path config
  - [x] Code templates (CodeTemplates.cs) — Panel, List, Popup templates
  - [x] Completer (ComponentScaffoldCompleter.cs) — post-compile asset creation + Addressable setup
  - [x] WizardList example — 3rd menu item to test wizard List output
  - [x] Fix Addressable address mismatches (postfix mode)
  - [x] **Template prefabs** — Template prefabs for each component type shipped with Framewerk. Use actual base framework View classes (ListView, PopupView, etc.) with serialized references wired up. Wizard clones template, generates real View script, swaps component — Unity preserves field references since field names match.
  - [x] **Skinning system** — SkinConfig ScriptableObject maps component types → blueprint prefab overrides. Project creates custom SkinConfig to replace framework defaults. No SkinConfig = use framework blueprints. Wizard reads active skin when scaffolding.
  - [x] Wizard creates prefab with real components (not empty GameObjects)
  - [x] **Wizard reads UiManager/PopupManager config** — Wizard currently hardcodes `UI/` TypeKey in postfix position for addressable IDs. Should read the actual UiManager and PopupManager config (TypeKey, TypeKeyIsPrefix) to build addresses that match how the managers resolve them at runtime. Currently assumes default postfix mode with TypeKey="UI".
  - [x] **MCP tool for wizard** — Expose wizard as an MCP tool so CC/AI agents can scaffold UI components headlessly (e.g. `create_ui_component(name, type, context, path)`). Would let CC create Lists, Popups etc. without manual UI interaction. → `framewerk_scaffold` tool (create_popup, create_list, mark_addressable)
- [x] **2. Addressables Setup Streamlining** — Think through the Addressables setup in projects that use Framewerk. Can we make onboarding smoother for users? Auto-setup, conventions, editor wizards?
- [x] **3. Popup Info Passing Cleanup** — BindingUtils extracted, auto-binds interfaces (on) and base classes (opt-in). ExplicitType overloads kept for edge cases.
- [x] **4. Port UNet to Mirror** — Port the networking package from UNet to Mirror. Make it a proper optional package. → `feature/mirror` branch (NetworkHost rewritten for Mirror Transport, custom UDP discovery, NetworkReader/Writer cleaned)
- [ ] **5. Split Examples** — Each example self-contained with its own scene. No monolithic demo — isolated examples easy to understand and reference.
  - [ ] Tabs example
  - [ ] State machine example
  - [x] Networking example → `feature/mirror` branch (NetworkDemoBootstrap + full StrangeIoC wiring)
- [x] **6. Modular Package Split** — Split optional functionality into own packages: Core (IoC, FSM, commands), UI (UiManager, screens, popups), Networking (Mirror-based), etc. Users install only what they need.
- [ ] **7. Documentation** — Comprehensive docs for all Framewerk features, APIs, and architecture.
- [ ] **8. Tutorials** — Step-by-step guides for common use cases (getting started, building a screen, creating lists, popups, networking, etc.).

- [x] **9. Scene creation wizard** — Add `create_scene` action to `framewerk_scaffold` (or new MCP tool) that creates a fully wired Framewerk scene from the Scene Setup Blueprint (Camera, EventSystem, Bootstrap, ViewConfig, dual Canvas hierarchy).
- [x] **10. SubBinder** — Reusable binding groups for contexts. Extract common binding sets (e.g. UI services, networking) into composable units that contexts can include instead of duplicating bindings.
- [x] **11. Scriptable address resolver** — Replace hard-coded Addressable address strings with an injectable ScriptableObject-based resolver. Decouple asset addresses from code, allow per-project overrides.
- [x] **12. Scene template** — Save SampleScene structure as a reusable template asset so new scenes can be created with correct Framewerk hierarchy without manual setup.
- [x] **13. Automatic references binder** — Component passed to bootstrap context that auto-binds all referenced GameObjects (prefabs, ScriptableObjects, etc.). Eliminates manual binding of asset references.
- [ ] **14. Fine-tune UI templates** — Polish the default wizard templates (Panel, List, Popup, etc.) to ensure they look nice and professional out of the box.
