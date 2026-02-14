using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class GameScreen : AppStateScreen
    {
        protected override void Enter()
        {
            InstantiateView<GameContentView>();
        }
    }
}
