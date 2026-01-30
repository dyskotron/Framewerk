using Framewerk.AppStateMachine;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.Examples
{
    public class ExamplesState : AppState<ExamplesScreen>
    {
        private ExampleId _exampleId;

        public ExamplesState(ExampleId exampleId)
        {
            _exampleId = exampleId;
        }

        protected override void Enter()
        {
            Screen.InitExample(_exampleId);
            base.Enter();
        }
    }
}
