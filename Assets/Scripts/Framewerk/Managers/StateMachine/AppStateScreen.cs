using System;

namespace Framewerk.Managers.StateMachine
{
    public abstract class AppStateScreen
    {

        public Action TransitionInFinished;
        public Action TransitionOutFinished;

        protected TransitionType TransitionType;

        public virtual void In()
        {
            TransitionType = TransitionType.TransitionIn;
        }

        public virtual void Out()
        {
            TransitionType = TransitionType.TransitionOut;
        }

        public virtual void Destroy()
        {

        }

        protected void InvokeTransitionInFinished()
        {
            TransitionType = TransitionType.None;

            if (TransitionInFinished != null)
            {
                TransitionInFinished.Invoke();
            }
        }

        protected void InvokeTransitionOutFinished()
        {
            TransitionType = TransitionType.None;

            if (TransitionOutFinished != null)
            {
                TransitionOutFinished.Invoke();
            }
        }
    }
}
