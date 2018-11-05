using Framewerk.Core;
using UnityEngine;

namespace Framewerk.Managers.StateMachine
{
    /// <summary>
    /// Interface of Application State. All methods are called by Fsm
    /// </summary>
    public interface IAppState
    {
        /// <summary>
        /// Starts opening transition process of the state
        /// </summary>
        void TransitionIn();

        /// <summary>
        /// Starts closing transition process of the state
        /// </summary>
        void TransitionOut();

        /// <summary>
        /// Is called before Init in case that we need to set screen externally (substates share parent's state screen)
        /// </summary>
        void SetScreen(AppStateScreen screen);

        /// <summary>
        /// Inits AppState
        /// </summary>
        void Init(Fsm fsm);

        /// <summary>
        /// Destroys AppState
        /// </summary>
        void Destroy();
    }

    /// <summary>
    /// AppState class serves as both state definition and it's controller.
    /// State should represent one main state of the application which is usually consistent in terms of display e.g. loading, main menu, game, minigame...
    /// State is connected with ViewScreen which is aggregator of the view logic of the state.
    /// </summary>
    /// <typeparam name="TScreen">Type of view screen connected with the state</typeparam>
    public class AppState<TScreen> : IAppState where TScreen : AppStateScreen, new()
    {
        [Inject]
        protected IEventDispatcher EventDispatcher;

        [Inject]
        protected IInjector Injector;

        protected Fsm Fsm;

        protected TScreen Screen;

        protected bool IsDestroyed;

        public AppState()
        {
        }

        public void SetScreen(AppStateScreen screen)
        {
            if (screen is TScreen)
                Screen = screen as TScreen;
            else
                Debug.LogErrorFormat("<color=\"aqua\">{0} : Given screen {1} is not of the type {2}</color>", this, screen.GetType(), typeof(TScreen));
        }

        public virtual void Init(Fsm fsm)
        {
            if (Screen == null)
            {
                Screen = new TScreen();
                Injector.InjectInto(Screen);
            }

            Fsm = fsm;
            RegisterEventHandlers();
        }

        public virtual void Destroy()
        {
            IsDestroyed = true;
            Screen.Destroy();
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
            Fsm.SetTransitionInFinished();
        }

        /// <summary>
        /// Notifies fsm that closing transition is finished. 
        /// </summary>
        protected virtual void TransitionOutFinished()
        {
            //Screen.TransitionOutFinished -= TransitionOutFinished;
            UnregisterEventHandlers();
            Fsm.SetTransitionOutFinished();
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
    }
}