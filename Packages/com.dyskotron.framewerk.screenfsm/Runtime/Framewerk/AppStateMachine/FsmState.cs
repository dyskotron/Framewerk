using strange.extensions.signal.impl;
using UnityEngine;

namespace Framewerk.AppStateMachine
{
    /// <summary>
    /// State that is a state machine: to its own machine it is a state, to its substates it is the machine.
    /// It owns one screen, entered and exited with the state and shared by all its substates
    /// (<see cref="SubState{TScreen}"/>), which are switched with its <see cref="Fsm.SwitchState"/>.
    /// When the state leaves, its running substate exits on <see cref="Destroy"/>, then the screen is destroyed.
    /// </summary>
    /// <typeparam name="TScreen">Type of the screen shared by the state and its substates</typeparam>
    public class FsmState<TScreen> : Fsm, IAppState where TScreen : FsmStateScreen
    {
        [Inject] public TScreen Screen { get; set; }

        public Signal EnterFinishedSignal { get; } = new Signal();
        public Signal ExitFinishedSignal { get; } = new Signal();

        #region FSM overrides

        protected override void StartState(IAppState state)
        {
            if (!(state is SubState<TScreen>))
                Debug.LogWarning($"{this} : Substate not valid. Given state has to be {typeof(SubState<TScreen>)}");

            if (state is AppState<TScreen> appState)
                appState.Screen = Screen;

            base.StartState(state);
        }

        public override void Destroy()
        {
            if (CurrentState != null)
                base.Destroy();

            Screen.Destroy();
        }

        #endregion

        #region State API

        public virtual void PerformEnter()
        {
            RegisterHandlers();
            Enter();
            Screen.PerformEnter();
            EnterFinished();
        }

        public virtual void PerformExit()
        {
            Exit();
            Screen.PerformExit();
            ExitFinished();
        }

        protected virtual void Enter()
        {
        }

        protected virtual void Exit()
        {
        }

        protected virtual void EnterFinished()
        {
            EnterFinishedSignal.Dispatch();
        }

        protected virtual void ExitFinished()
        {
            UnregisterHandlers();
            ExitFinishedSignal.Dispatch();
        }

        protected virtual void RegisterHandlers()
        {
        }

        protected virtual void UnregisterHandlers()
        {
        }

        #endregion
    }
}
