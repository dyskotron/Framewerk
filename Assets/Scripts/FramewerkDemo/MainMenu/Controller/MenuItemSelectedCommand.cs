using Framewerk.AppStateMachine;
using FramewerkDemo.Examples;
using FramewerkDemo.MainMenu.Model;
using strange.extensions.command.impl;

namespace FramewerkDemo.MainMenu
{
    public class MenuItemSelectedCommand : Command
    {
        [Inject] public ExampleId ExampleId { get; set; }
        [Inject] public IAppFsm Fsm { get; set; }

        public override void Execute()
        {
            Fsm.SwitchState(new ExamplesState(ExampleId));
        }
    }
}
