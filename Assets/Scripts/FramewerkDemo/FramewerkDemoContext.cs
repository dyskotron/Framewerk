using Framewerk.Events;
using Framewerk.Managers;
using Framewerk.Managers.Popup;
using Framewerk.Managers.StateMachine;
using Framewerk.Mvcs;
using Framewerk.ViewUtils;
using FramewerkDemo.MainMenu;
using FramewerkDemo.MainMenu.Controller;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo
{
    public class FramewerkDemoContext : Context
    {
        public FramewerkDemoContext(BaseViewSettings viewSettings) : base(viewSettings)
        {

        }

        protected override void Startup()
        {
            //FSM
            Injector.MapSingletonOf<IFsm,Fsm>();

            //MODEL
            Injector.MapSingletonOf<IMenuModel, MenuModel>();
            Injector.MapSingletonOf<IPlayerPrefsManager, PlayerPrefsManager>();

            //VIEW
            Injector.MapSingletonOf<IAssetManager, AssetManager>();
            Injector.MapSingletonOf<IUIManager, UIManager>();
            Injector.MapSingletonOf<IPopUpManager, PopUpManager>();

            //COMMANDS
            CommandMap.MapCommand<FramewerkStartEvent, FramewerkStartCommand>();
            CommandMap.MapCommand<MenuItemSelectedEvent, MenuItemSelectedCommand>();
            CommandMap.MapCommand<ShowMenuEvent, ShowMenuCommand>();

            base.Startup();
        }

        protected override void Exit()
        {

        }
    }
}