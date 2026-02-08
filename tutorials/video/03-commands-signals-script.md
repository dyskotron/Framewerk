# Video Script: Chapter 3 — Commands & Signals

**Duration:** ~15-18 minutes  
**Style:** Code-heavy tutorial with diagrams  
**Prerequisite:** Chapter 2 (Dependency Injection)

---

## INTRO (0:00 - 0:45)

### Visual
- Framewerk logo animation
- Title card: "Chapter 3: Commands & Signals"
- Quick montage of code snippets

### Script
> Welcome to Chapter 3 of the Framewerk tutorial series. Today we're diving into the event-driven heart of Framewerk: **Signals and Commands**.
>
> If dependency injection is about *getting* things to where they're needed, signals and commands are about *communicating* between those things — without creating tight coupling.
>
> By the end of this video, you'll understand how to fire events with signals, handle them with commands, and build reactive systems where components don't need to know about each other.

---

## SECTION 1: Understanding Signals (0:45 - 3:30)

### SEGMENT 1.1: What Are Signals? (0:45 - 1:45)

#### Visual
- Animated diagram: traditional event system (strings, type-unsafe)
- Transition to Signal model (type-safe, compile-time checking)
- Show compiler error when wrong callback signature

#### Script
> Let's start with Signals. If you've used Unity's built-in events or C# delegates, signals will feel familiar — but with one crucial difference: **type safety**.
>
> Traditional event systems often use strings as event names. That means typos become runtime bugs. Signals flip this around. They're **types**, which means the compiler catches mistakes before you even run the game.
>
> Watch what happens if I try to add a listener with the wrong signature...
> 
> *(show compiler error)*
>
> That's a compile-time error. Not a runtime surprise.

---

### SEGMENT 1.2: Signal Variants (1:45 - 2:30)

#### Visual
- Code panel showing Signal variants side by side:
```csharp
Signal                           // No parameters
Signal<int>                      // One parameter
Signal<string, int>              // Two parameters
Signal<T, U, V>                  // Three parameters
Signal<T, U, V, W>               // Four parameters (max)
```
- Highlight the 4-parameter limit with callout

#### Script
> Signals come in flavors based on how many parameters they carry. No parameters, one, two, three, or four.
>
> Four is the maximum — that's a C# Action delegate limitation. If you need more, wrap your data in a value object. Honestly, if you need five parameters, a value object is probably cleaner anyway.

---

### SEGMENT 1.3: Adding Listeners and Dispatching (2:30 - 3:30)

#### Visual
- Live coding demo:
  1. Create a Signal<int>
  2. AddListener with matching callback
  3. Dispatch with value
  4. Show console output
  5. RemoveListener

#### Script
> Here's the basic flow. Create a signal, add a listener — notice the callback must match the signal's type signature — then dispatch.
>
> *(type and demonstrate)*
>
> `AddListener` is persistent. There's also `AddOnce` which auto-removes after the first dispatch — perfect for one-shot events.
>
> And always — **always** — remove your listeners when you're done. Memory leaks are no joke.

---

## SECTION 2: Local vs Global Signals (3:30 - 5:30)

### SEGMENT 2.1: Global Signals (3:30 - 4:30)

#### Visual
- Diagram: Signal bound in Context as Singleton
- Multiple classes injecting the same signal instance
- Arrows showing same instance everywhere

#### Code on screen:
```csharp
// In Context
injectionBinder.Bind<GameOverSignal>().ToSingleton();

// In any class
[Inject]
public GameOverSignal gameOver { get; set; }
```

#### Script
> There are two ways to use signals: globally and locally.
>
> **Global signals** are bound in your Context as singletons. Every class that injects `GameOverSignal` gets the exact same instance. When one class dispatches, everyone listening hears it.
>
> This is perfect for system-wide events — game over, score changes, scene transitions. Things the whole app cares about.

---

### SEGMENT 2.2: Local Signals (4:30 - 5:15)

#### Visual
- Diagram: Signal created locally in one class
- Only direct references can listen

#### Code on screen:
```csharp
public class EnemySpawner
{
    // Local — not injected
    public Signal<Enemy> enemySpawned { get; } = new Signal<Enemy>();
}
```

#### Script
> **Local signals** are different. You create them directly in a class, no Context binding. Only code with a direct reference to that instance can listen.
>
> Use these for component-internal communication. An enemy spawner might have a local `enemySpawned` signal. Only the systems that specifically watch that spawner receive the events.

---

### SEGMENT 2.3: Decision Framework (5:15 - 5:30)

#### Visual
- Decision flowchart:
  - "Multiple unrelated listeners?" → Global
  - "Component-specific event?" → Local

#### Script
> Quick rule of thumb: if multiple unrelated parts of your app need to listen, go global. If it's specific to one component's behavior, go local.

---

## SECTION 3: Commands (5:30 - 8:30)

### SEGMENT 3.1: What Are Commands? (5:30 - 6:30)

#### Visual
- Animated diagram: Signal fires → Command created → Injected → Execute → Cleanup
- Lifecycle visualization

#### Script
> Now let's talk Commands. A command is a **single-purpose chunk of business logic**.
>
> When a signal fires, the command binder creates a new command instance, injects its dependencies, calls `Execute()`, and then cleans it up. All automatic.
>
> This keeps your logic modular. Each command does one thing. Easy to test, easy to understand.

---

### SEGMENT 3.2: Basic Command Structure (6:30 - 7:30)

#### Visual
- Code editor with Command class:

```csharp
public class AddScoreCommand : Command
{
    [Inject]
    public IScoreModel scoreModel { get; set; }
    
    [Inject]
    public int points { get; set; }  // From signal!
    
    public override void Execute()
    {
        scoreModel.AddPoints(points);
    }
}
```

#### Script
> Here's a basic command. It extends `Command` and overrides `Execute()`.
>
> Notice the injections. `scoreModel` is a regular dependency. But `points`? That comes from the signal that triggered this command.
>
> When you dispatch `scoreSignal.Dispatch(100)`, that `100` gets injected as `points`. The command doesn't know or care who sent it.

---

### SEGMENT 3.3: Parameter Injection Deep Dive (7:30 - 8:30)

#### Visual
- Diagram showing Signal<int, string> → Command
- Type matching visualization (arrows from signal params to command properties)
- Warning callout about unique types

#### Script
> This is important: signal parameters are matched by **type**, not position.
>
> If your signal is `Signal<int, string>` and your command injects `string` and `int`, it still works — the binder matches types.
>
> But here's the catch: you can't have two parameters of the same type. `Signal<string, string>` with a command needing both? The binder can't tell which is which. Use a value object instead.

---

## SECTION 4: Signal-to-Command Binding (8:30 - 11:00)

### SEGMENT 4.1: Basic Binding (8:30 - 9:15)

#### Visual
- Code in Context:
```csharp
commandBinder.Bind<PlayerHitSignal>().To<PlayerHitCommand>();
```
- Animation showing signal → binding → command flow

#### Script
> Connecting signals to commands happens in your Context. One line: `commandBinder.Bind<SignalType>().To<CommandType>()`.
>
> When that signal dispatches, the command executes. Simple.

---

### SEGMENT 4.2: Chaining and Options (9:15 - 10:15)

#### Visual
- Multiple code examples side by side:
```csharp
// Multiple commands
.To<CmdA>().To<CmdB>().To<CmdC>();

// Sequential
.To<CmdA>().To<CmdB>().InSequence();

// One-time
.To<StartupCommand>().Once();

// Pooled
.To<FrequentCommand>().Pooled();
```

#### Script
> You can chain multiple commands to one signal — they run in parallel by default.
>
> Add `.InSequence()` to run them one after another. The next command waits for the previous to finish.
>
> `.Once()` removes the binding after it fires — great for startup commands.
>
> `.Pooled()` reuses command instances instead of creating new ones — use this for performance-critical, frequently-fired commands.

---

### SEGMENT 4.3: Visual Binding Summary (10:15 - 11:00)

#### Visual
- Animated binding options chart with all modifiers

#### Script
> Here's your cheat sheet. Memorize these modifiers and you'll cover 99% of use cases.

---

## SECTION 5: Async Commands - Retain/Release (11:00 - 13:00)

### SEGMENT 5.1: The Async Problem (11:00 - 11:45)

#### Visual
- Animation: Command executes → starts async call → Command destroyed → callback fires → null reference explosion

#### Script
> Here's a gotcha. By default, commands are cleaned up **immediately** after `Execute()` returns.
>
> What if you're waiting for a server response? Your callback fires, tries to use the command's properties... and they're gone. Null reference. Crash.

---

### SEGMENT 5.2: The Solution (11:45 - 12:30)

#### Visual
- Code with Retain/Release highlighted:
```csharp
public override void Execute()
{
    Retain();  // Keep me alive!
    
    authService.Login(user, pass, OnComplete);
}

private void OnComplete(bool success)
{
    // Do stuff...
    
    Release();  // Now you can clean me up
}
```

#### Script
> The fix is simple: `Retain()` at the start, `Release()` when you're done.
>
> `Retain()` tells the binder "don't clean me up yet." `Release()` says "okay, I'm finished."
>
> Forget `Release()` and you've got a memory leak. Set a reminder. Tattoo it on your arm. Whatever it takes.

---

### SEGMENT 5.3: Fail() for Sequences (12:30 - 13:00)

#### Visual
- Sequence diagram with Fail() stopping the chain

#### Script
> One more: `Fail()`. If you're in a sequence and something goes wrong, call `Fail()` to stop subsequent commands from running. Validation failed? `Fail()`. Skip the rest.

---

## SECTION 6: Practical Example - Login Flow (13:00 - 16:30)

### SEGMENT 6.1: Overview (13:00 - 13:30)

#### Visual
- Architecture diagram: View → Mediator → Signal → Command → Service → Signal → Mediator → View

#### Script
> Let's build a real login system. This will tie together everything we've learned.
>
> User clicks login. Mediator dispatches a signal. Command calls the auth service. On response, command dispatches success or failure. Mediator updates the view. Clean separation all the way through.

---

### SEGMENT 6.2: Define Signals (13:30 - 14:00)

#### Visual
- Code file with signal definitions

```csharp
public class LoginRequestSignal : Signal<string, string> { }
public class LoginSuccessSignal : Signal<UserData> { }
public class LoginFailedSignal : Signal<string> { }
```

#### Script
> First, our signals. Login request takes username and password. Success carries user data. Failure carries the error message.

---

### SEGMENT 6.3: Build the Command (14:00 - 15:00)

#### Visual
- Full LoginCommand implementation with highlighting

#### Script
> The command injects the auth service and both signal parameters. `Retain()` at the start because we're going async.
>
> On callback, dispatch the appropriate result signal, then `Release()`.
>
> Notice how the command doesn't know anything about UI. It just talks to the service and fires signals.

---

### SEGMENT 6.4: Wire It Up (15:00 - 15:45)

#### Visual
- Context bindings split screen with mediator usage

#### Script
> In the Context, bind the service, the signals, and connect `LoginRequestSignal` to `LoginCommand`.
>
> In the mediator, dispatch the request signal when the user clicks login. Use `[ListensTo]` attributes to handle the response signals. Update the view accordingly.
>
> That's it. No direct dependencies between UI and business logic.

---

### SEGMENT 6.5: Flow Animation (15:45 - 16:30)

#### Visual
- Full animated flow diagram walking through a login attempt

#### Script
> Let's trace the full flow. Click. Dispatch. Command. Service call. Callback. Success signal. Mediator. View update. 
>
> Every step is decoupled. You could swap the auth service for a mock and the UI would never know. You could replace the view entirely and the command logic stays the same.
>
> That's the power of signals and commands.

---

## SECTION 7: Common Patterns & Anti-Patterns (16:30 - 17:30)

### SEGMENT 7.1: Good Patterns (16:30 - 17:00)

#### Visual
- Pattern cards:
  - Request/Response pairs
  - Progress signals for long ops
  - State change signals with old/new values

#### Script
> Some patterns to embrace. Always pair request signals with response signals. For long operations, add progress signals. For state changes, include both old and new values.

---

### SEGMENT 7.2: Anti-Patterns (17:00 - 17:30)

#### Visual
- Anti-pattern cards with X marks:
  - Mutable objects through signals
  - Forgetting Release
  - Duplicate parameter types
  - Mega-command chains

#### Script
> And what to avoid. Don't pass mutable objects — listeners might change them. Don't forget Release. Don't use duplicate parameter types. And if you're chaining more than three or four commands, consider refactoring.

---

## OUTRO (17:30 - 18:00)

### Visual
- Recap bullet points
- "Next: Chapter 4 - Views & Mediators"
- Subscribe/like callout

### Script
> That's Commands and Signals. Type-safe events with Signals. Decoupled business logic with Commands. Reactive, testable, clean.
>
> Next chapter, we'll connect this to the UI layer with Views and Mediators.
>
> Drop questions in the comments. Like and subscribe if this helped. See you in Chapter 4.

---

## B-ROLL / ASSET NOTES

### Diagrams Needed
1. Traditional events vs Signals comparison
2. Signal lifecycle (create → dispatch → listeners)
3. Global vs Local signal scope
4. Command lifecycle (create → inject → execute → cleanup)
5. Signal parameter → Command injection type matching
6. Retain/Release async timeline
7. Full login flow architecture

### Code Files to Show
- `Signal.cs` (framework source, briefly)
- `Command.cs` (framework source, briefly)
- Custom signals (3-4 examples)
- Custom commands (2-3 examples)
- Context bindings
- Mediator with ListensTo

### Screen Recordings
- Creating a Signal and listener in IDE (with autocomplete)
- Compile-time error demo (wrong callback signature)
- Runtime demo of login flow (Unity play mode)

---

## TIMING BREAKDOWN

| Section | Duration | Cumulative |
|---------|----------|------------|
| Intro | 0:45 | 0:45 |
| Understanding Signals | 2:45 | 3:30 |
| Local vs Global | 2:00 | 5:30 |
| Commands | 3:00 | 8:30 |
| Signal-to-Command Binding | 2:30 | 11:00 |
| Async (Retain/Release) | 2:00 | 13:00 |
| Login Example | 3:30 | 16:30 |
| Patterns & Anti-Patterns | 1:00 | 17:30 |
| Outro | 0:30 | 18:00 |

**Total: ~18 minutes**
