using Framewerk.AppStateMachine;
using Framewerk.Managers;
using FramewerkDemo.MainMenu.Controller;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.MainMenu
{
    public class MenuScreen : AppStateScreen
    {
        [Inject] public IMenuModel MenuModel { get; set; }
        [Inject] public IUiManager UiManager { get; set; }
        [Inject] public MenuItemSelectedSignal MenuItemSelectedSignal { get; set; }

        private MenuView _menuView;

        protected override void Enter()
        {
            base.Enter();

            _menuView = InstantiateView<MenuView>("Menu/");
        }

        protected override void Exit()
        {
            base.Exit();
        }
    }
}
