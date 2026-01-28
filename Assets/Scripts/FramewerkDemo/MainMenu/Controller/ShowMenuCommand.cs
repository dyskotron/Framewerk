using Framewerk.AppStateMachine;
using strange.extensions.command.impl;

namespace FramewerkDemo.MainMenu
{
    public class ShowMenuCommand : Command
    {
        [Inject] public IAppFsm Fsm { get; set; }

        public override void Execute()
        {
            Fsm.SwitchState(new MenuState());
        }
    }
}
