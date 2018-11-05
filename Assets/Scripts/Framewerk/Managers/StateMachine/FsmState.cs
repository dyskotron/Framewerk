using UnityEngine;

namespace Framewerk.Managers.StateMachine
{
    public class FsmState<T> : Fsm, IAppState where T : FsmStateScreen, new()
    {
        protected Fsm ParentFsm;
        protected T Screen;
        protected IAppState state;

        /// <summary>
        /// Sub FSM has to act as fsm and state together so create state as we are extending basic fsm
        /// </summary>
        public FsmState()
        {
            state = new AppState<T>();  
        }
        
        #region FSM overrides
        
        protected override void StartState(IAppState state)
        {
            if(!(state is SubState<T>))
                Debug.LogWarningFormat("<color=\"aqua\">{0} : Substate not valid. Given state has to be {1}</color>", this, typeof(SubState<T>));
            
            state.SetScreen(Screen);
            base.StartState(state);
        }
        
        #endregion
        
        #region State API

        public void SetScreen(AppStateScreen screen)
        {
            
        }

        public void Init(Fsm fsm)
        {
            Screen = new T();
            Injector.InjectInto(Screen);
            
            state.SetScreen(Screen);
            state.Init(this);  
            
            ParentFsm = fsm;
        }
        
        public virtual void TransitionIn()
        {
            Screen.In();
            //TransitionInFinished is now called directly, enable this for closing state after screen animations
            //Screen.TransitionInFinished += TransitionInFinished;
            TransitionInFinished();
        }

        public virtual void TransitionOut()
        {
            Screen.Out();
            //TransitionOutFinished is now called directly, enable this for closing state after screen animations
            //Screen.TransitionOutFinished += TransitionOutFinished;
            TransitionOutFinished();
        }

        /// <summary>
        /// Notifies fsm that opening transition is finished. 
        /// </summary>
        protected virtual void TransitionInFinished()
        {
            //Screen.TransitionInFinished -= TransitionInFinished;
            ParentFsm.SetTransitionInFinished();
        }

        /// <summary>
        /// Notifies fsm that closing transition is finished. 
        /// </summary>
        protected virtual void TransitionOutFinished()
        {
            //Screen.TransitionOutFinished -= TransitionOutFinished;
            UnregisterEventHandlers();
            ParentFsm.SetTransitionOutFinished();
        }

        /// <summary>
        /// Registers handlers for all view events.
        /// </summary>
        protected virtual void RegisterEventHandlers()
        {
        }

        /// <summary>
        /// Unregisters handlers for all view events.
        /// </summary>
        protected virtual void UnregisterEventHandlers()
        {
        }
        
        #endregion
    }
}