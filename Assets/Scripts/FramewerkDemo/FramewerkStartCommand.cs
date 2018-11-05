using Framewerk.Core;
using Framewerk.Managers.StateMachine;
using Framewerk.Mvcs;
using FramewerkDemo.MainMenu;

namespace FramewerkDemo
{
    public class FramewerkStartCommand : Command
    {
        [Inject] private IFsm _fsm;
        
        public override void Execute()
        {
            _fsm.SwitchState(new MenuState());
        }
    }
}