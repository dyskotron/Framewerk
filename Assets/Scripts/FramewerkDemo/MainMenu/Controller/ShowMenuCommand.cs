using Framewerk.Core;
using Framewerk.Managers.StateMachine;
using Framewerk.Mvcs;

namespace FramewerkDemo.MainMenu
{
    public class ShowMenuCommand : Command
    {
        [Inject] private IFsm _fsm;
        
        public override void Execute()
        {
            _fsm.SwitchState(new MenuState());    
        }
    }
}