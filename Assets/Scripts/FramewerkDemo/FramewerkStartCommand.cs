using Framewerk.AppStateMachine;
using FramewerkDemo.MainMenu;
using strange.extensions.command.impl;

namespace FramewerkDemo
{
    public class FramewerkStartCommand : Command
    {
        [Inject] public IAppFsm Fsm { get; set; }

        public override void Execute()
        {
            // PopupManager is now auto-configured via ViewConfig injection
            Fsm.SwitchState(new MenuState());
        }
    }
}
