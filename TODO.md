# Framewerk 2.0 TODO

## Must-have before 2.0

- [ ] **1. UI Asset Scaffolding** — Auto-create View/Mediator pairs with prefab, lists with items and data classes etc. Editor tooling to scaffold new UI components quickly.
- [ ] **2. Addressables Setup Streamlining** — Think through the Addressables setup in projects that use Framewerk. Can we make onboarding smoother for users? Auto-setup, conventions, editor wizards?
- [ ] **3. Popup Info Passing Cleanup** — Simplify passing info to popups. Use `GetType()` on passed objects to auto-bind the actual runtime type, not just the declared parameter type. Bind both the concrete type and base types so mediators can inject either way.
- [ ] **4. Port UNet to Mirror** — Port the networking package from UNet to Mirror. Make it a proper optional package.
- [ ] **5. Split Examples** — Each example self-contained with its own scene. No monolithic demo — isolated examples easy to understand and reference.
- [ ] **6. Modular Package Split** — Split optional functionality into own packages: Core (IoC, FSM, commands), UI (UiManager, screens, popups), Networking (Mirror-based), etc. Users install only what they need.
