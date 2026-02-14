using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class MenuScreen : AppStateScreen
    {
        protected override void Enter()
        {
            InstantiateView<MainMenuView>();
        }
    }
}
