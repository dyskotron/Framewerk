# Substates: porting 1.x's sub-state machine to 2.0

> **Status:** design, not built. Target release: **2.0.0-preview.4** (package `com.dyskotron.framewerk.screenfsm`).
> The decisions marked **D1-D6** are Matej's; each has a recommendation.

## Why

Framewerk 1.x had states with substates: an app state that runs its own little state machine, every substate
sharing the parent's screen. Everything the parent state set up (its screen, its views, its bindings) stays while
the substates switch only what is theirs (input, a piece of UI). It was used in many projects and did not make it
into 2.0, which has only the flat `AppFsm`. This port brings the same architecture to 2.0, as a Framewerk feature:
nothing in it is specific to any game.

## 1.x, as it was

Source: branch `dev` (and `master`), `Assets/Scripts/Framewerk/Managers/StateMachine/` (`Fsm.cs`, `AppState.cs`,
`AppStateScreen.cs`, `FsmState.cs`, `SubState.cs`, `FsmStateScreen.cs`).

- **`FsmState<T> : Fsm, IAppState where T : FsmStateScreen`**: a state that is a state machine. It inherits the
  machine (`Fsm`: one state at a time, a switch closes the current state, then opens the next; switches made during
  a transition are queued) and composes an `AppState<T>` for the state side. `Init` creates the screen once
  (`new T()` + `Injector.InjectInto`); `TransitionIn` / `TransitionOut` run the screen's `In` / `Out`.
- **`SubState<T> : AppState<T> where T : FsmStateScreen`**: a state without a screen of its own. `TransitionIn` calls
  `Screen.SubstateIn(this)`, `TransitionOut` calls `Screen.SubstateOut(this)`; both finish at once.
- **`FsmStateScreen : AppStateScreen`**: the screen with the two hooks `SubstateIn(IAppState)` /
  `SubstateOut(IAppState)` (virtual, empty).
- **Sharing:** `FsmState.StartState` hands every substate its own screen (`state.SetScreen(Screen)`, which
  `AppState.Init` then keeps instead of creating one) and warns when the state isn't a `SubState<T>`.
- **Switching:** the parent is an `IFsm`, so a substate is switched with the parent's `SwitchState`, by whoever holds
  the parent (typically the parent itself, from its event handlers). No other API.
- **One level:** substates are plain `SubState`s; a substate with substates of its own was not supported.
- **A gap:** `FsmState.TransitionOut` runs only the screen's `Out`; it never closes the running substate, so that
  substate's `TransitionOut` (and its `SubstateOut` hook) never runs when the parent leaves. (D4.)

## What stays the same

The architecture, unchanged:

1. A parent state **is** a state (to its machine) and **is** a machine (to its substates).
2. The parent owns one screen; its substates have none and share the parent's.
3. A substate's enter and exit reach the shared screen through `SubstateIn` / `SubstateOut` on `FsmStateScreen`.
4. Substates follow the machine's rules: one at a time, exit then enter, switches during a transition queued.
5. A substate is switched with the parent's `SwitchState`; nothing else.
6. One level of substates.
7. The names: `FsmState`, `SubState`, `FsmStateScreen`.

## The 2.0 machinery it sits on

`Packages/com.dyskotron.framewerk.screenfsm/Runtime/Framewerk/AppStateMachine/`:

- **`IAppFsm` / `AppFsm`:** `SwitchState` injects the state (`InjectionBinder.injector.Inject(state, false)`), then
  starts it or queues it; a state reports the end of its transitions through its `EnterFinishedSignal` /
  `ExitFinishedSignal`; after an exit the state is `Destroy`ed. Every finished transition dispatches the injected
  `AppStateEnterSignal` / `AppStateExitSignal` (the state's `Type`), without a null check.
- **`IAppState`:** `EnterFinishedSignal`, `ExitFinishedSignal`, `PerformEnter()`, `PerformExit()`, `Destroy()`. No
  `Init`, no `SetScreen`.
- **`AppState<TScreen>`:** the screen is injected (`[Inject] public TScreen Screen`); `PerformEnter` (not virtual) =
  `RegisterHandlers`, `Enter`, `Screen.PerformEnter`, `EnterFinished`; `PerformExit` (virtual) = `Exit`,
  `Screen.PerformExit`, `ExitFinished` (which runs `UnregisterHandlers`); `Destroy` destroys the screen. A subclass
  overrides `Enter`, `Exit`, `RegisterHandlers`, `UnregisterHandlers`.
- **`AppStateScreen`:** abstract, injected (asset and UI managers, `ViewConfig`); `PerformEnter` / `PerformExit`
  around virtual `Enter` / `Exit`; `Destroy` releases every view it instantiated. Screens are bound by the app's
  context (the example binds them `ToSingleton`; an app may bind them per injection).

So, against 1.x: the state gets its screen by injection instead of creating it; transitions are
`PerformEnter` / `PerformExit` with finished signals instead of `TransitionIn` / `TransitionOut` with
`SetTransitionInFinished`; the machine dispatches app-wide signals; and a state's `Destroy` destroys its screen.

## The port

### `FsmStateScreen : AppStateScreen` (new)
As 1.x: `public virtual void SubstateIn(IAppState state)` and `SubstateOut(IAppState state)`, empty. A parent's
screen type must be one.

### `SubState<TScreen> : IAppState where TScreen : FsmStateScreen` (new)
The state side of `AppState<TScreen>` without a screen of its own (D2):
- `TScreen Screen { get; }`, set by its parent before it enters (not injected).
- `PerformEnter` = `RegisterHandlers`, `Enter`, `Screen.SubstateIn(this)`, `EnterFinished`.
- `PerformExit` = `Exit`, `Screen.SubstateOut(this)`, `ExitFinished` (`UnregisterHandlers`).
- `Destroy` does nothing to the screen (it is the parent's).
- The same virtuals as `AppState`: `Enter`, `Exit`, `RegisterHandlers`, `UnregisterHandlers`, `EnterFinished`,
  `ExitFinished`; the same order as `AppState` (the state's own code first, then the screen).

### `FsmState<TScreen> : AppState<TScreen>, IAppFsm where TScreen : FsmStateScreen` (new)
A state (to the app's machine) and a machine (to its substates) (D1):
- **As a state:** everything `AppState<TScreen>` does: its screen injected, `Enter` / `Exit` / handlers for the
  subclass, `PerformEnter` / `PerformExit`.
- **As a machine:** `IAppFsm`'s `CurrentState`, `CurrentTransition`, `SwitchState`, driven by a private machine it
  owns (D3). `SwitchState` injects the substate with the context's injector (as `AppFsm`), hands it the parent's
  screen and starts or queues it with `AppFsm`'s rules. A state that isn't a `SubState<TScreen>` is refused with a
  warning (1.x warned and went on; it can't go on in 2.0: such a state has no way to receive the shared screen).
- **Entering:** `RegisterHandlers`, `Enter`, `Screen.PerformEnter`, then the first substate if there is one (D5),
  then `EnterFinished`.
- **Leaving:** the running substate exits first (its `Exit`, `SubstateOut`), queued switches are dropped, then the
  parent's `Exit`, `Screen.PerformExit`, `ExitFinished` (D4). If the substate's exit finishes later (an overridden
  `ExitFinished`), the parent's exit waits for it.
- **Destroy:** `IAppState.Destroy` and `IAppFsm.Destroy` are the same method: the app's machine destroys a state only
  after its exit, so by then no substate is running; it destroys the screen, as `AppState` does.
- A `SwitchState` while the parent isn't entered (before its enter, after its exit) is refused with a warning.

### Not in the port
Anything 1.x didn't have: more than one level, a history or "back" to the previous substate, guards on switches,
substates running side by side, a dedicated API for switching from outside the parent. An app builds those on top,
if it needs them.

## Decisions

**D1 · Which side is inherited.** 1.x inherited the machine and composed the state, because its `AppState` was
thin. In 2.0 the state side carries what a subclass overrides (the injected screen, `Enter` / `Exit`, the handlers),
while the machine side is a small engine. **Recommended:** inherit `AppState<TScreen>`, implement `IAppFsm`, compose
the machine. Both are "a state that is a machine"; only the direction of the composition turns.

**D2 · `SubState` as its own `IAppState`.** `AppState<TScreen>` injects its screen and its `PerformEnter` isn't
virtual, so a `SubState` deriving from it would get a screen of its own (a new one, when screens aren't singletons),
run the screen's whole enter, and destroy the shared screen on its `Destroy`. **Recommended:** `SubState<TScreen>`
implements `IAppState` itself with `AppState`'s virtuals and order, its screen set by the parent. The alternative
changes `AppState` (a virtual `PerformEnter`, the screen's injection made optional), which touches every app state.

**D3 · The engine, and the app-wide signals.** `AppFsm` always dispatches `AppStateEnterSignal` /
`AppStateExitSignal`, so an `AppFsm` used inside a parent would report substates as app states (and would throw where
those signals aren't bound). 1.x had no such signals. **Recommended:** move `AppFsm`'s queue and transition logic
into a base (`Fsm`, as in 1.x, with no signals) that `AppFsm` extends, adding only the two signals; `AppFsm`'s
behaviour unchanged. `FsmState` owns an `Fsm`; substates dispatch no signals. Signals for substates can come later if
an app needs them.

**D4 · Leaving the parent exits its substate first.** 1.x didn't (see *A gap*), so a substate's `Exit` and
`SubstateOut` never ran when the parent left. **Recommended:** fix it in the port; the one behaviour that differs
from 1.x, named in the changelog.

**D5 · The first substate.** 1.x had no rule: whoever started the parent switched a substate in. In 2.0 a
`SwitchState` from the parent's `Enter` would run `SubstateIn` before the screen's own `Enter` built its views.
**Recommended:** a `protected virtual IAppState InitialSubstate()` (default `null`: none), started after the screen's
enter and before the parent reports its enter finished, so the app's machine sees the parent entered with its first
substate in. `SwitchState` from `Enter` is refused with a warning (D6's rule: the parent isn't entered yet).

**D6 · When `SwitchState` is accepted.** From the moment the parent's screen has entered (the initial substate) to
the start of its exit. **Recommended** as written; outside it, a warning and nothing switched.

## Tests

A new assembly `com.dyskotron.framewerk.screenfsm.tests` (`Packages/com.dyskotron.framewerk.screenfsm/Tests/`,
EditMode, set up like the `ui` package's tests). Test states and screens record their calls in order:

- Entering a parent: its `Enter`, the screen's `Enter`, the initial substate's `Enter` and `SubstateIn`, then the
  parent's enter finished; no initial substate when `InitialSubstate` returns `null`.
- Switching: the old substate's `Exit` and `SubstateOut`, then the new one's `Enter` and `SubstateIn`; the old one
  destroyed; every substate gets the parent's screen instance; a switch during a transition is queued.
- Leaving a parent: the substate's `Exit` and `SubstateOut` first, then the parent's `Exit` and the screen's `Exit`;
  queued switches dropped; the screen destroyed once, by the parent only.
- Refusals: a state that isn't a `SubState<TScreen>`; a switch before the parent's screen entered or after its exit
  began.
- Signals: `AppStateEnterSignal` / `AppStateExitSignal` fire for the parent, never for a substate; `AppFsm` behaves
  as before (its existing behaviour pinned before the refactor of D3).

## Example

`Assets/Examples/ScreenFsmExample`: its `GameState` becomes an `FsmState<GameScreen>` with two substates and a way
to switch between them (a key or a button), the screen showing which one is in through `SubstateIn` / `SubstateOut`
(a view instantiated through the scaffold tooling). A sample of the pattern, not a feature.

## Docs

The package's README (or `docs/`) gets a section on substates: when to use them (one screen and its setup kept while
a part of the state changes), the three classes, the lifecycle above, the rules (D4-D6).

## Release

1. The work on a branch off `feature/framewerk2.0` (the 2.0 line, where the `v2.0.0-preview.*` tags are), merged
   back into it.
2. All five packages and their internal dependency pins to `2.0.0-preview.4`; `Packages/com.dyskotron.framewerk/CHANGELOG.md`
   gets `## [2.0.0-preview.4]` with *Added* (substates: `FsmState`, `SubState`, `FsmStateScreen`; the screenfsm tests)
   and *Changed* (`AppFsm` built on the new `Fsm` base, its behaviour unchanged), as preview.3 did.
3. An annotated tag `v2.0.0-preview.4` ("Framewerk 2.0.0-preview.4") on that commit.
4. Pushing the branch and the tag to `origin` (`git@github.com:dyskotron/Framewerk.git`) publishes it: OpenUPM builds
   the packages from the tag. Check the packages' OpenUPM pages until 2.0.0-preview.4 is listed.
