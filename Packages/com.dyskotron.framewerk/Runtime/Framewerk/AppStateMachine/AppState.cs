using System.Threading.Tasks;
using strange.extensions.injector.api;
using strange.extensions.signal.impl;

namespace Framewerk.AppStateMachine
{
    public interface IAppState
    {
        Signal EnterFinishedSignal { get; }
        Signal ExitFinishedSignal { get; }

        void PerformEnter();
        void PerformExit();
        void Destroy();
    }

    public class AppState<TScreen> : IAppState where TScreen : AppStateScreen
    {
        [Inject] public TScreen Screen { get; set; }

        public Signal EnterFinishedSignal { get; } = new Signal();
        public Signal ExitFinishedSignal { get; } = new Signal();

        public virtual void Destroy()
        {
            Screen.Destroy();
        }

        public void PerformEnter()
        {
            RegisterHandlers();
            Enter();
            PerformEnterInternalAsync();
        }

        public virtual void PerformExit()
        {
            Exit();
            PerformExitInternalAsync();
        }

        private async void PerformEnterInternalAsync()
        {
            await Screen.PerformEnterAsync();
            EnterFinished();
        }

        private async void PerformExitInternalAsync()
        {
            await Screen.PerformExitAsync();
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
    }
}
