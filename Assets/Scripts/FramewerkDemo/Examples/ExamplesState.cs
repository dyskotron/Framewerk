using Framewerk.Core;
using Framewerk.Managers.StateMachine;
using FramewerkDemo.MainMenu;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.Examples
{
    public class ExamplesState : AppState<ExamplesScreen>
    {
        [Inject] private IEventDispatcher _eventDispatcher;
        
        private ExampleId _exampleId;

        public ExamplesState(ExampleId exampleId)
        {
            _exampleId = exampleId;
        }

        protected override void TransitionInFinished()
        {
            Screen.InitExample(_exampleId);
            
            base.TransitionInFinished();
        }
    }
}