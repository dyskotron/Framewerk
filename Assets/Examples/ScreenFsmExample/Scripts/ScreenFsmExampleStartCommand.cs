using Framewerk.AppStateMachine;
using strange.extensions.command.impl;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class ScreenFsmExampleStartCommand : Command
    {
        [Inject] public IAppFsm Fsm { get; set; }

        public override void Execute()
        {
            Fsm.SwitchState(new MenuState());
        }
    }
}
