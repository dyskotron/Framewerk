# Framewerk 2.0 TODO

## Must-have before 2.0

### 1. UI Asset Scaffolding
Auto-create View/Mediator pairs with prefab, lists with items and data classes etc. Editor tooling to scaffold new UI components quickly.

### 2. Addressables Setup Streamlining
Think through the Addressables setup in projects that use Framewerk. Can we make onboarding smoother for users? Auto-setup, conventions, editor wizards?

### 3. Popup Info Passing Cleanup
Check if we can simplify passing info to popups — get rid of the second method where types need to be passed explicitly. Cleaner API.

### 4. Port UNet to Mirror
Port the networking package from UNet to Mirror. Make it a proper optional package.

### 5. Split Examples
Each example should be self-contained with its own scene. No monolithic demo — isolated examples that are easy to understand and reference.

### 6. Modular Package Split
Split all optional functionality into its own packages:
- **Core** (IoC, FSM, commands — always required)
- **UI** (UiManager, screens, popups)
- **Networking** (Mirror-based)
- **Whatever else makes sense**

Users install only what they need.
