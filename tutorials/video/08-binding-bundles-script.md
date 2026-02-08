# Chapter 8: Binding Bundles — Video Script

## Video Overview
- **Duration:** ~14-18 minutes
- **Format:** Screen recording with voice-over
- **Goal:** Demonstrate bundle creation, installation, and best practices for modular architecture

---

## INTRO (0:00 - 0:45)

### Visual
- Framewerk logo animation
- Split screen: LEFT shows a massive 200-line Context file scrolling endlessly, RIGHT shows a clean 10-line Context with InstallBundle calls

### Script
> "As your game grows, your Context files grow with it. Hundreds of bindings crammed into one method. Finding anything becomes a nightmare. Today we fix that with Binding Bundles — modular, reusable, self-cleaning groups of bindings. Let's transform chaos into clarity."

---

## SECTION 1: The Problem (0:45 - 2:30)

### Visual
- Open a real project's GameContext.cs (or create an example with 150+ bindings)
- Scroll through slowly, highlighting sections

### Script
> "Here's what a real project Context can look like. We've got core services up here — logging, analytics, save system. Then audio bindings. Then inventory. Chat. Achievements. Quests. On and on."

### Visual
- Highlight line count: 200+ lines
- Try to Cmd+F for a specific binding, show it takes time

### Script
> "Two hundred lines in one method. Need to find the inventory binding? Good luck. Want to disable chat for testing? Delete 20 lines and pray you got them all."

### Visual
- Show diagram of problems:
  - Unmaintainable
  - Not reusable
  - No cleanup
  - No modularity

### Script
> "This approach has four major problems. It's unmaintainable. It's not reusable between projects. There's no way to clean up at runtime. And you can't load features on demand."

---

## SECTION 2: Bundle Architecture (2:30 - 4:30)

### Visual
- Show class hierarchy diagram:
  ```
  IBindingBundle
       ↓
  CoreBindingBundle (core package)
       ↓
  BindingBundle (ui package)
  ```

### Script
> "Framewerk provides two bundle base classes. CoreBindingBundle is pure .NET — it handles injections and commands. BindingBundle extends it with mediation support for Unity UI. Choose based on whether your feature has Views."

### Visual
- Open `CoreBindingBundle.cs` in IDE
- Highlight injected properties:
  - `InjectionBinder`
  - `CommandBinder`

### Script
> "CoreBindingBundle gets binders injected automatically. You don't create these — the Context provides them when you install the bundle."

### Visual
- Highlight `OnInstall()` abstract method

### Script
> "You override OnInstall to define your bindings. This is where all your feature's wiring goes."

### Visual
- Highlight tracking lists:
  - `_injectionBindings`
  - `_commandBindings`

### Script
> "Crucially, bundles track every binding they create. When you call Uninstall, everything gets cleaned up automatically. No manual bookkeeping."

### Visual
- Open `BindingBundle.cs`
- Highlight `MediationBinder` and `_mediationBindings`

### Script
> "BindingBundle adds mediation support. Same pattern — it tracks mediations for automatic cleanup."

---

## SECTION 3: Creating Your First Bundle (4:30 - 8:00)

### Visual
- Create new file: `AudioBundle.cs`
- Type out the class structure

### Script
> "Let's create an audio bundle from scratch. I'll extend CoreBindingBundle since this is pure service logic — no Views involved."

### Visual
```csharp
using Framewerk.StrangeCore.Bundles;

namespace MyGame.Bundles
{
    public class AudioBundle : CoreBindingBundle
    {
        protected override void OnInstall()
        {
            
        }
    }
}
```

### Script
> "Minimal setup. We extend CoreBindingBundle, implement OnInstall. Now let's add our bindings."

### Visual
- Add injection bindings one by one:
```csharp
protected override void OnInstall()
{
    // Services
    BindInjection<IAudioManager>().To<AudioManager>().ToSingleton();
    BindInjection<IMusicPlayer>().To<MusicPlayer>().ToSingleton();
    BindInjection<ISFXPlayer>().To<SFXPlayer>().ToSingleton();
```

### Script
> "BindInjection is the tracked version of InjectionBinder.Bind. Same fluent API, but the bundle remembers this binding for cleanup later."

### Visual
- Add command bindings:
```csharp
    // Commands
    BindCommand<PlaySFXSignal, PlaySFXCommand>();
    BindCommand<PlayMusicSignal, PlayMusicCommand>();
    BindCommand<StopMusicSignal, StopMusicCommand>();
}
```

### Script
> "BindCommand wires signals to commands. Again, tracked automatically."

### Visual
- Show complete bundle file

### Script
> "That's a complete bundle. All audio-related bindings in one place. Let's see how to use it."

---

## SECTION 4: Installing Bundles (8:00 - 10:00)

### Visual
- Open `GameContext.cs`
- Show the old massive MapBindings method

### Script
> "Here's our bloated Context. Watch what happens when we extract to bundles."

### Visual
- Replace audio section with:
```csharp
protected override void MapBindings()
{
    base.MapBindings();
    
    InstallBundle<AudioBundle>();
}
```

### Script
> "InstallBundle creates the bundle via the injector — so binders get injected — then calls Install. One line replaces fifteen."

### Visual
- Add more bundle installs:
```csharp
protected override void MapBindings()
{
    base.MapBindings();
    
    InstallBundle<CoreServicesBundle>();
    InstallBundle<AudioBundle>();
    InstallBundle<InventoryBundle>();
    InstallBundle<ChatBundle>();
}
```

### Script
> "Do this for each feature. Core services, audio, inventory, chat. Our 200-line method becomes 10 lines."

### Visual
- Side by side: Before (scrolling) vs After (fits on screen)

### Script
> "The difference is night and day. Each feature is now a single, documented, testable module."

---

## SECTION 5: UI Bundles with Mediation (10:00 - 12:30)

### Visual
- Create `ChatBundle.cs`
- Extend `BindingBundle` instead of `CoreBindingBundle`

### Script
> "Chat has UI — we need mediations. So we extend BindingBundle from the UI package."

### Visual
```csharp
using Framewerk.StrangeCore.Bundles;

namespace MyGame.Chat
{
    public class ChatBundle : BindingBundle
    {
        protected override void OnInstall()
        {
            // Services
            BindInjection<IChatService>().To<ChatService>().ToSingleton();
            BindInjection<IChatHistory>().To<ChatHistory>().ToSingleton();
            
            // Commands
            BindCommand<SendMessageSignal, SendMessageCommand>();
            BindCommand<ReceiveMessageSignal, ReceiveMessageCommand>();
            
            // Mediations — only available in BindingBundle!
            BindMediation<ChatWindowView, ChatWindowMediator>();
            BindMediation<ChatBubbleView, ChatBubbleMediator>();
            BindMediation<ChatInputView, ChatInputMediator>();
        }
    }
}
```

### Script
> "Same pattern as before, but now we have BindMediation. This binds Views to Mediators and tracks them for cleanup. Everything related to chat — services, commands, mediations — lives in one file."

### Visual
- Highlight the BindMediation lines

### Script
> "When you uninstall this bundle, all these mediations are unbound. The Views won't get Mediators anymore. Perfect for dynamically loaded features."

---

## SECTION 6: Cooperative Bindings (12:30 - 15:00)

### Visual
- Show two bundles that both need ILogger:
```csharp
// InventoryBundle
BindInjection<ILogger>().To<UnityLogger>().ToSingleton();

// ChatBundle
BindInjection<ILogger>().To<UnityLogger>().ToSingleton();
```

### Script
> "Here's a common problem. Both bundles need a logger. If both try to bind it, you get a duplicate binding error at runtime."

### Visual
- Show error message in console

### Script
> "Strange IoC throws when you bind the same key twice. We need a way for bundles to share dependencies peacefully."

### Visual
- Change to BindIfMissing:
```csharp
// InventoryBundle
BindIfMissing<ILogger>().To<UnityLogger>().ToSingleton();

// ChatBundle  
BindIfMissing<ILogger>().To<UnityLogger>().ToSingleton();
```

### Script
> "BindIfMissing is the solution. It checks if the binding already exists. If so, it does nothing. If not, it creates the binding."

### Visual
- Show console with VerboseLogging enabled:
```
[InventoryBundle] Bound ILogger
[ChatBundle] Skipping binding for ILogger — already bound.
```

### Script
> "With verbose logging, you can see exactly what happens. Inventory binds the logger first. Chat sees it's already there and skips."

### Visual
- Show NullInjectionBinding class briefly

### Script
> "When skipped, BindIfMissing returns a NullInjectionBinding — a no-op object that lets you chain methods without null checks. The fluent API just works, even when nothing happens."

### Visual
- Diagram showing ownership:
  ```
  InventoryBundle OWNS: ILogger, IInventory
  ChatBundle OWNS: IChatService
  ChatBundle SKIPS: ILogger
  ```

### Script
> "Critical point: skipped bindings aren't tracked by the skipping bundle. Inventory owns the logger. If you uninstall Chat, the logger stays — because Inventory bound it first."

---

## SECTION 7: Best Practices (15:00 - 17:00)

### Visual
- Show recommended folder structure:
```
Assets/Scripts/
├── Bundles/
│   ├── CoreServicesBundle.cs
│   └── AudioBundle.cs
├── Features/
│   ├── Chat/
│   │   ├── ChatBundle.cs
│   │   ├── Services/
│   │   ├── Views/
│   │   └── Commands/
│   └── Inventory/
```

### Script
> "Organize bundles near their features. I like a top-level Bundles folder for infrastructure, then each feature folder contains its own bundle."

### Visual
- Show Context with bundles in order:
```csharp
// 1. Core infrastructure (no dependencies)
InstallBundle<LoggingBundle>();
InstallBundle<NetworkBundle>();

// 2. Shared services (depend on core)
InstallBundle<AudioBundle>();

// 3. Features (depend on services)
InstallBundle<InventoryBundle>();
InstallBundle<ChatBundle>();
```

### Script
> "Install order matters. Core infrastructure first — things with no dependencies. Then shared services. Then features. Dependencies flow downward."

### Visual
- Show the "when to use bundles" checklist:
  - ✅ 5+ related bindings
  - ✅ Might be reused
  - ✅ Needs runtime enable/disable
  - ✅ Want isolated testing
  - ❌ Just 2-3 bindings
  - ❌ One-off wiring

### Script
> "When should you create a bundle? Five or more related bindings. Something you might reuse. Something you might disable at runtime. Or when you want to test a feature in isolation. For just a couple simple bindings, don't bother with the ceremony."

---

## SECTION 8: Recap (17:00 - 18:00)

### Visual
- Split screen showing before/after Context
- Key points overlay:
  - CoreBindingBundle = .NET only
  - BindingBundle = Unity UI
  - InstallBundle<T>() = one-line install
  - BindIfMissing = cooperative sharing
  - Automatic cleanup on Uninstall

### Script
> "Let's recap. CoreBindingBundle for pure .NET logic. BindingBundle when you need mediations. InstallBundle to wire them up. BindIfMissing for shared dependencies. Everything tracked and cleaned up automatically."

### Visual
- Final message: "Transform 200 lines into 10. Bundles are your friend."

### Script
> "Bundles transform unmaintainable monsters into clean, modular architecture. Start extracting them early. Your future self — and your teammates — will thank you."

---

## OUTRO (18:00 - 18:15)

### Visual
- Framewerk logo
- "Next: Chapter 9" teaser (if applicable)

### Script
> "That's binding bundles. In the next chapter, we'll explore [next topic]. Thanks for watching."

---

## B-Roll / Cutaway Suggestions

1. **Context scrolling** — Slow scroll through a massive file, slightly sped up
2. **Bundle file creation** — Real-time typing with keystroke sounds
3. **Split comparison** — Before/after side by side
4. **Dependency diagram** — Animated arrows showing install order
5. **Console output** — VerboseLogging showing bundle installation
6. **Error message** — Duplicate binding error (then fix)

---

## Key Timestamps for Chapters

| Time | Section |
|------|---------|
| 0:00 | Intro |
| 0:45 | The Problem |
| 2:30 | Bundle Architecture |
| 4:30 | Creating Your First Bundle |
| 8:00 | Installing Bundles |
| 10:00 | UI Bundles with Mediation |
| 12:30 | Cooperative Bindings |
| 15:00 | Best Practices |
| 17:00 | Recap |

---

## Prompt Ideas for AI Video Generation

### Intro Scene
> "Generate a split-screen animation. Left side: endless scrolling code in a dark IDE theme, text blurring as it moves. Right side: clean, minimal code with white space. Text overlay: 'From Chaos to Clarity'. Style: modern tech tutorial, purple/blue accent colors."

### Bundle Diagram
> "Create an animated hierarchy diagram. Top box: 'IBindingBundle' (interface icon). Arrow down to 'CoreBindingBundle' (gear icon). Arrow down to 'BindingBundle' (UI/window icon). Each box lights up in sequence. Style: clean lines, dark background, glowing edges."

### Ownership Visualization
> "Animate a Venn diagram. Circle A labeled 'InventoryBundle' contains: ILogger, IInventory. Circle B labeled 'ChatBundle' contains: IChatService. ILogger in circle A glows, arrow to ChatBundle shows 'SKIPPED'. Style: clean, minimal, color-coded circles."

### Before/After Transform
> "Show a code editor morphing. Start with 200 lines of densely packed code (blur details). Animate compression into 10 clean lines with InstallBundle calls. Numbers in corner: 200 → 10. Style: satisfying transformation, smooth animation."
