namespace Framewerk.AppStateMachine
{
    /// <summary>
    /// State of an <see cref="FsmState{TScreen}"/>. Has no screen of its own: it shares its parent's screen, which
    /// the parent hands it before it enters. Instead of entering / exiting the screen it calls the screen's
    /// <see cref="FsmStateScreen.SubstateIn"/> / <see cref="FsmStateScreen.SubstateOut"/>.
    /// </summary>
    /// <typeparam name="TScreen">Type of the parent's screen</typeparam>
    public class SubState<TScreen> : AppState<TScreen> where TScreen : FsmStateScreen
    {
        public override void PerformEnter()
        {
            RegisterHandlers();
            Enter();
            Screen.SubstateIn(this);
            EnterFinished();
        }

        public override void PerformExit()
        {
            Exit();
            Screen.SubstateOut(this);
            ExitFinished();
        }

        /// <summary>
        /// The screen is the parent's: the parent destroys it.
        /// </summary>
        public override void Destroy()
        {
        }
    }
}
