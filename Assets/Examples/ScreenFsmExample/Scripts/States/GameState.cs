using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    /// <summary>
    /// A state with substates: Playing and Paused share the GameScreen and its views,
    /// switched by the SwitchGameSubstateSignal.
    /// </summary>
    public class GameState : FsmState<GameScreen>
    {
        [Inject] public SwitchGameSubstateSignal SwitchGameSubstateSignal { get; set; }

        protected override void EnterFinished()
        {
            SwitchState(new PlayingSubState());
            base.EnterFinished();
        }

        protected override void RegisterHandlers()
        {
            SwitchGameSubstateSignal.AddListener(OnSwitchGameSubstate);
        }

        protected override void UnregisterHandlers()
        {
            SwitchGameSubstateSignal.RemoveListener(OnSwitchGameSubstate);
        }

        private void OnSwitchGameSubstate()
        {
            if (CurrentState is PlayingSubState)
                SwitchState(new PausedSubState());
            else
                SwitchState(new PlayingSubState());
        }
    }
}
