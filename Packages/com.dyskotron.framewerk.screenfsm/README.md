# Framewerk Screen FSM

Screen-based finite state machine for app state and view management, part of
[Framewerk](https://github.com/dyskotron/Framewerk).

## App states

`AppFsm` (bound as `IAppFsm`) holds one app state at a time. `SwitchState(state)` injects the state, exits the
current one, destroys it, then enters the new one; switches made during a transition are queued. Every finished
transition dispatches `AppStateEnterSignal` / `AppStateExitSignal` with the state's type.

An app state extends `AppState<TScreen>`. Its screen (`AppStateScreen`) is injected, enters and exits with the
state and releases the views it instantiated when the state is destroyed.

```csharp
injectionBinder.Bind<IAppFsm>().To<AppFsm>().ToSingleton();
injectionBinder.Bind<AppStateEnterSignal>().ToSingleton();
injectionBinder.Bind<AppStateExitSignal>().ToSingleton();
injectionBinder.Bind<MenuScreen>().ToSingleton();

Fsm.SwitchState(new MenuState());
```

## Substates

Use substates when a state keeps one screen and everything it set up while a part of the state changes: a game
state whose screen stays while it switches between playing, paused or a cutscene.

- **`FsmState<TScreen>`**: a state that is also a state machine (it extends `Fsm`, the machine `AppFsm` is built
  on). To the app's machine it is a state like any other; to its substates it is their machine. It owns one screen.
- **`SubState<TScreen>`**: a state of an `FsmState`. It has no screen of its own: it shares its parent's.
- **`FsmStateScreen`**: the parent's screen. Its `SubstateIn(state)` / `SubstateOut(state)` are called when a
  substate enters / exits.

```csharp
public class GameScreen : FsmStateScreen
{
    public override void SubstateIn(IAppState state) { /* show what the substate needs */ }
    public override void SubstateOut(IAppState state) { /* hide it again */ }
}

public class PlayingSubState : SubState<GameScreen> { }
public class PausedSubState : SubState<GameScreen> { }

public class GameState : FsmState<GameScreen>
{
    [Inject] public PauseSignal PauseSignal { get; set; }

    protected override void EnterFinished()
    {
        SwitchState(new PlayingSubState());   // the first substate, once the screen has entered
        base.EnterFinished();
    }

    protected override void RegisterHandlers() => PauseSignal.AddListener(OnPause);
    protected override void UnregisterHandlers() => PauseSignal.RemoveListener(OnPause);

    private void OnPause() => SwitchState(new PausedSubState());
}
```

### Lifecycle

- **Entering the parent:** `RegisterHandlers`, `Enter`, the screen's `Enter`, `EnterFinished`.
- **A substate entering:** its `RegisterHandlers`, `Enter`, then the screen's `SubstateIn`, `EnterFinished`.
- **A substate exiting:** its `Exit`, the screen's `SubstateOut`, `ExitFinished` (`UnregisterHandlers`).
- **Leaving the parent:** `Exit`, the screen's `Exit`, `ExitFinished` (`UnregisterHandlers`). The app's machine
  then destroys the parent: queued substate switches are dropped, the running substate exits (`Exit`,
  `SubstateOut`), then the screen is destroyed.

### Rules

- A substate is switched with the parent's `SwitchState`, typically by the parent itself from its handlers. It
  follows the machine's rules: one at a time, exit then enter, switches during a transition queued.
- There is no rule for the first substate: the parent switches one in where it fits (`EnterFinished`, as above, has
  the screen entered and the parent reporting entered with its substate in).
- The parent injects its substates through its own binder, in which `TScreen` is bound to the parent's screen; everything
  else resolves from the context. So a substate's injected `Screen` is the parent's, and no screen is created for it, however
  the screen is bound. A state that isn't a `SubState<TScreen>` gets a
  warning.
- A substate never destroys the screen: the parent does.
- Substates dispatch no `AppStateEnterSignal` / `AppStateExitSignal`; only the parent does, as an app state.
- One level: substates don't have substates of their own.

See `Assets/Examples/ScreenFsmExample` (its `GameState`) in the Framewerk repository.
