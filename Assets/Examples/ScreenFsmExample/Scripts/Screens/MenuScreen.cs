using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class MenuScreen : AppStateScreen
    {
        protected override async void Enter()
        {
            await InstantiateViewAsync<MainMenuView>();
        }
    }
}
