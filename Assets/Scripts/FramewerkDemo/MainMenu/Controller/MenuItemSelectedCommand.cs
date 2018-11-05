using Framewerk.Core;
using Framewerk.Managers.StateMachine;
using Framewerk.Mvcs;
using FramewerkDemo.Examples;
using UnityEngine;

namespace FramewerkDemo.MainMenu
{
    public class MenuItemSelectedCommand : Command
    {
        [Inject] private MenuItemSelectedEvent _e;
        [Inject] private IFsm _fsm;
        
        public override void Execute()
        {
            //Switch to Framework example view
            _fsm.SwitchState(new ExamplesState(_e.ExampleId));
        }
    }
}