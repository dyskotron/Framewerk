namespace Framewerk.Managers.StateMachine
{
    public class SubState<T> : AppState<T> where T : FsmStateScreen, new()
    {
        public override void TransitionIn()
        {
            Screen.SubstateIn(this);
            TransitionInFinished();
        }

        public override void TransitionOut()
        {
            Screen.SubstateOut(this);
            TransitionOutFinished();
        }
    }
}