using Framewerk.AppStateMachine;
using Framewerk.Popups;
using FramewerkDemo.MainMenu;
using Plugins.Framewerk;
using strange.extensions.command.impl;

namespace FramewerkDemo
{
    public class FramewerkStartCommand : Command
    {
        [Inject] public IAppFsm Fsm { get; set; }
        [Inject] public IPopupManager PopupManager { get; set; }
        [Inject] public ViewConfig ViewConfig { get; set; }

        public override void Execute()
        {
            PopupManager.Init(Framewerk.Popups.PopupManager.UI_PREFABS_ROOT, ViewConfig.Popups);
            Fsm.SwitchState(new MenuState());
        }
    }
}
