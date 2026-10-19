namespace Framewerk.AppStateMachine
{
    /// <summary>
    /// Screen of an <see cref="FsmState{TScreen}"/>, shared by all its substates.
    /// The parent's screen enters and exits with the parent; the substates reach it through
    /// <see cref="SubstateIn"/> / <see cref="SubstateOut"/>.
    /// </summary>
    public class FsmStateScreen : AppStateScreen
    {
        /// <summary>
        /// Called when a substate of the screen's state enters.
        /// </summary>
        public virtual void SubstateIn(IAppState state)
        {
        }

        /// <summary>
        /// Called when a substate of the screen's state exits.
        /// </summary>
        public virtual void SubstateOut(IAppState state)
        {
        }
    }
}
