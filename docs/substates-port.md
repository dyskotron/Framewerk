# Substates: porting 1.x's sub-state machine to 2.0

> **Status:** decisions settled (D1-D6, below), built in 2.0.0-preview.4 (package `com.dyskotron.framewerk.screenfsm`).
> Matej's ruling on all of them: **port 1.x as it was built**; deviate only where 2.0 makes 1.x's way impossible.

## Why

Framewerk 1.x had states with substates: an app state that runs its own little state machine, every substate
sharing the parent's screen. Everything the parent state set up (its screen, its views, its bindings) stays while
the substates switch only what is theirs (input, a piece of UI). It was used in many projects and did not make it
into 2.0, which has only the flat `AppFsm`. This port brings the same architecture to 2.0, as a Framewerk feature:
nothing in it is specific to any game.

## 1.x, as it was

Source: branch `dev` (and `master`), `Assets/Scripts/Framewerk/Managers/StateMachine/` (`Fsm.cs`, `AppState.cs`,
`AppStateScreen.cs`, `FsmState.cs`, `SubState.cs`, `FsmStateScreen.cs`).

- **`Fsm : IFsm`**: the machine. One state at a time; a switch closes the current state, then opens the next;
  switches made during a transition are queued. `SwitchState` injects the state. `Destroy` clears the queue and
  closes the current state. No app-wide signals.
- **`FsmState<T> : Fsm, IAppState where T : FsmStateScreen`**: a state that is a state machine. It **inherits the
  machine** and implements the state side itself (`TransitionIn` / `TransitionOut` run the screen's `In` / `Out`,
  then report to the parent machine; its own virtual `RegisterEventHandlers` / `UnregisterEventHandlers`). It also
  composes an `AppState<T>`, which only gets the screen and registers its (empty) handlers: no observable effect.
  `Init` creates the screen (`new T()` + `Injector.InjectInto`).
- **`SubState<T> : AppState<T> where T : FsmStateScreen`**: an `AppState` that overrides `TransitionIn` to call
  `Screen.SubstateIn(this)` and `TransitionOut` to call `Screen.SubstateOut(this)`; both finish at once.
- **`FsmStateScreen : AppStateScreen`**: the screen with the two hooks `SubstateIn(IAppState)` /
  `SubstateOut(IAppState)` (virtual, empty).
- **Sharing:** `FsmState.StartState` hands every substate its screen (`state.SetScreen(Screen)`, which
  `AppState.Init` then keeps instead of creating one) and warns, then goes on, when the state isn't a `SubState<T>`.
- **Switching:** the parent is an `IFsm`, so a substate is switched with the parent's `SwitchState`, by whoever holds
  the parent (typically the parent itself, from its event handlers). No guards, no other API, no rule for the first
  substate.
- **Leaving the parent:** the parent's `TransitionOut` runs the screen's `Out` and reports its exit. The parent
  machine then calls the parent's `Destroy`, which is `Fsm.Destroy`: the queue is cleared and the running substate
  exits (`TransitionOut` → `SubstateOut`). So a substate does exit when its parent leaves, after the parent's screen
  `Out`. (The first draft of this doc called this a gap; it isn't one.)
- **One level:** substates are plain `SubState`s; a substate with substates of its own was not supported.

## The 2.0 machinery it sits on

`Packages/com.dyskotron.framewerk.screenfsm/Runtime/Framewerk/AppStateMachine/`:

- **`IAppFsm` / `AppFsm`:** 1.x's `IFsm` / `Fsm`, renamed. `SwitchState` injects the state
  (`InjectionBinder.injector.Inject(state, false)`), then starts it or queues it; a state reports the end of its
  transitions through its `EnterFinishedSignal` / `ExitFinishedSignal`; after an exit the state is `Destroy`ed. Every
  finished transition dispatches the injected `AppStateEnterSignal` / `AppStateExitSignal` (the state's `Type`).
- **`IAppState`:** `EnterFinishedSignal`, `ExitFinishedSignal`, `PerformEnter()`, `PerformExit()`, `Destroy()`. No
  `Init`, no `SetScreen`.
- **`AppState<TScreen>`:** the screen is injected (`[Inject] public TScreen Screen { get; set; }`); `PerformEnter` =
  `RegisterHandlers`, `Enter`, `Screen.PerformEnter`, `EnterFinished`; `PerformExit` (virtual) = `Exit`,
  `Screen.PerformExit`, `ExitFinished` (which runs `UnregisterHandlers`); `Destroy` destroys the screen.
- **`AppStateScreen`:** abstract, injected; `PerformEnter` / `PerformExit` around virtual `Enter` / `Exit`; `Destroy`
  releases every view it instantiated (1.x's `Destroy` was empty).

## Decisions (settled)

Each one is 1.x's design; the second paragraph names what 2.0 forces, if anything.

**D1 · Which side is inherited. → As 1.x: the machine.** `FsmState<TScreen> : Fsm, IAppState`. It implements the
state side itself with 2.0's state contract: `[Inject] public TScreen Screen`, `EnterFinishedSignal` /
`ExitFinishedSignal`, virtual `PerformEnter` / `PerformExit` (as 1.x's `TransitionIn` / `TransitionOut` were),
virtual `Enter`, `Exit`, `EnterFinished`, `ExitFinished`, `RegisterHandlers`, `UnregisterHandlers`, in `AppState`'s
order. 1.x's composed `AppState<T>` had no observable effect and is not ported.

**D2 · `SubState`. → As 1.x: `SubState<TScreen> : AppState<TScreen>`**, overriding the enter and exit to call
`Screen.SubstateIn(this)` / `Screen.SubstateOut(this)` in place of the screen's own enter / exit:
`PerformEnter` = `RegisterHandlers`, `Enter`, `SubstateIn`, `EnterFinished`; `PerformExit` = `Exit`, `SubstateOut`,
`ExitFinished`. 2.0 forces two things: `AppState.PerformEnter` becomes `virtual` (no behaviour change), and
`SubState.Destroy` leaves the screen alone, because 2.0's screen `Destroy` releases the parent's views (1.x's was
empty, so a substate destroying the shared screen did nothing). The screen arrives by injection, as for any
`AppState`, but the parent decides what is injected: it injects its substates through its own binder
(`FsmState.StatesBinder`, created in its `[PostConstruct]`), a `CrossContextInjectionBinder` that binds `TScreen` to the parent's screen and resolves
everything else from the context's binder. (preview.4 let the context inject a screen into the substate and then
overwrote it with the parent's, so a per-injection screen binding created an unused screen per switch; fixed in
2.0.0-preview.5.)

**D3 · The machine. → As 1.x: `Fsm`, without signals.** `AppFsm`'s queue and transition logic moves into a base
`Fsm : IAppFsm` (1.x's `Fsm`); `AppFsm : Fsm` adds only the two app-wide signals, dispatched exactly where it
dispatched them before; `AppFsm`'s behaviour is unchanged. Substates dispatch no signals.

**D4 · Leaving the parent. → As 1.x.** The parent's `PerformExit` runs `Exit`, `Screen.PerformExit`, `ExitFinished`;
the app machine then calls its `Destroy`, which is `Fsm.Destroy` (queue cleared, the running substate's `Exit` and
`SubstateOut`), and then the screen's `Destroy` (in 2.0 the screen's views must be released, and the parent is the
one owning the screen). With no substate running, `Destroy` only destroys the screen.

**D5 · The first substate. → As 1.x: no rule, no API.** The parent switches one in itself, wherever it fits; e.g.
from an `EnterFinished` override before calling `base.EnterFinished()`, so the screen has entered and the parent
reports entered with its substate in (what the example does).

**D6 · When `SwitchState` is accepted. → As 1.x: always,** with `Fsm`'s rules (start, or queue during a transition).
A state that isn't a `SubState<TScreen>` gets a warning and goes on, as in 1.x; it receives the shared screen if it
is an `AppState<TScreen>`.

## What stays the same

1. A parent state **is** a state (to its machine) and **is** a machine (to its substates).
2. The parent owns one screen; its substates have none of their own and share the parent's.
3. A substate's enter and exit reach the shared screen through `SubstateIn` / `SubstateOut` on `FsmStateScreen`.
4. Substates follow the machine's rules: one at a time, exit then enter, switches during a transition queued.
5. A substate is switched with the parent's `SwitchState`; nothing else.
6. One level of substates.
7. The names: `Fsm`, `FsmState`, `SubState`, `FsmStateScreen`.

## Not in the port

Anything 1.x didn't have: more than one level, a history or "back" to the previous substate, guards on switches,
substates running side by side, a dedicated API for switching from outside the parent, signals for substate
transitions. An app builds those on top, if it needs them.

## Tests

A new assembly `com.dyskotron.framewerk.screenfsm.tests` (`Packages/com.dyskotron.framewerk.screenfsm/Tests/`,
EditMode, set up like the `ui` package's tests). Test states and screens record their calls in order:

- `AppFsm` pinned: enter/exit order, queueing, `Destroy` after exit, the two signals per transition.
- Entering a parent: its `Enter`, the screen's `Enter`, then whatever substate it switches in.
- Switching: the old substate's `Exit` and `SubstateOut`, then the new one's `Enter` and `SubstateIn`; every
  substate gets the parent's screen instance; a switch during a transition is queued.
- Leaving a parent: the parent's `Exit` and the screen's `Exit`, then the substate's `Exit` and `SubstateOut`, then
  the screen destroyed, once; queued switches dropped.
- Signals: `AppStateEnterSignal` / `AppStateExitSignal` fire for the parent, never for a substate.

## Example

`Assets/Examples/ScreenFsmExample`: its `GameState` is an `FsmState<GameScreen>` with two substates (`Playing`,
`Paused`) and a button to switch between them; the screen shows which one is in through `SubstateIn` /
`SubstateOut` (a view made with the scaffold tooling). A sample of the pattern, not a feature.

## Docs

The screenfsm package's README gets a section on substates: when to use them, the three classes, the lifecycle,
the rules above.

## Release

1. The work on a branch off `feature/framewerk2.0` (the 2.0 line, where the `v2.0.0-preview.*` tags are), merged
   back into it.
2. All five packages and their internal dependency pins to `2.0.0-preview.4`; `Packages/com.dyskotron.framewerk/CHANGELOG.md`
   gets `## [2.0.0-preview.4]` with *Added* (substates: `FsmState`, `SubState`, `FsmStateScreen`; the `Fsm` base;
   the screenfsm tests) and *Changed* (`AppFsm` built on `Fsm`, its behaviour unchanged; `AppState.PerformEnter`
   virtual), as preview.3 did.
3. An annotated tag `v2.0.0-preview.4` ("Framewerk 2.0.0-preview.4") on that commit.
4. Pushing the branch and the tag to `origin` (`git@github.com:dyskotron/Framewerk.git`) publishes it: OpenUPM builds
   the packages from the tag. Check the packages' OpenUPM pages until 2.0.0-preview.4 is listed.
