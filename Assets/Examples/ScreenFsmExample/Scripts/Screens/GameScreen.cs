using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class GameScreen : AppStateScreen
    {
        protected override async void Enter()
        {
            await InstantiateViewAsync<GameContentView>();
        }
    }
}
