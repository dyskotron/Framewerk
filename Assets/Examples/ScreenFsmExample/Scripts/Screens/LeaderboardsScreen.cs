using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class LeaderboardsScreen : AppStateScreen
    {
        protected override void Enter()
        {
            InstantiateView<LeaderboardsContentView>();
        }
    }
}
