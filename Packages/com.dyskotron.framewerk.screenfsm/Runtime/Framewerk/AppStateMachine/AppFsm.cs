namespace Framewerk.AppStateMachine
{
    /// <summary>
    /// _fsm a.k.a Finite State Machine is class responsible for storing main state of the game
    /// and handling transitions from one state to another. It also notifies <see cref="EventRouter"/>
    /// that new state transition is finished so it can dispatch waiting events to it.
    /// </summary>
    public enum TransitionType
    {
        None,
        Enter,
        Exit
    }

    public interface IAppFsm
    {
        /// <summary>
        /// Returns current state.
        /// </summary>
        IAppState CurrentState { get; }

        /// <summary>
        /// Returns state of the transition.
        /// </summary>
        TransitionType CurrentTransition { get; }

	    /// <summary>
	    /// Switches current state for a new one. If fsm is already in some state, new state is queued.
	    /// First the current state is closed (transition out) then new state is opened (transition in).
	    /// </summary>
	    /// <param name="newState">New state to be opened</param>
	    void SwitchState(IAppState newState);

        void Destroy();
    }

    /// <summary>
    /// The app's state machine: an <see cref="Fsm"/> that also dispatches the app-wide
    /// <see cref="AppStateEnterSignal"/> / <see cref="AppStateExitSignal"/> for every finished transition.
    /// </summary>
    public class AppFsm : Fsm
    {
        [Inject] public AppStateEnterSignal AppStateEnterSignal { get; set; }
        [Inject] public AppStateExitSignal AppStateExitSignal { get; set; }

        protected override void EnterFinishedHandler()
        {
            AppStateEnterSignal.Dispatch(CurrentState.GetType());

            base.EnterFinishedHandler();
        }

        protected override void ExitFinishedHandler()
        {
            AppStateExitSignal.Dispatch(CurrentState.GetType());

            base.ExitFinishedHandler();
        }
    }
}
