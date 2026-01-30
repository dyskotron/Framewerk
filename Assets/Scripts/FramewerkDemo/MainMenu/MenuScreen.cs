using Framewerk.AppStateMachine;
using FramewerkDemo.MainMenu.Controller;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.MainMenu
{
    public class MenuScreen : AppStateScreen
    {
        [Inject] public IMenuModel MenuModel { get; set; }
        [Inject] public MenuItemSelectedSignal MenuItemSelectedSignal { get; set; }

        private MenuView _menuView;

        protected override void Enter()
        {
            _menuView = InstantiateView<MenuView>("Examples/Menu");
        }
    }
}
