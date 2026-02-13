using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class LeaderboardsScreen : AppStateScreen
    {
        protected override async void Enter()
        {
            await InstantiateViewAsync<LeaderboardsContentView>();
        }
    }
}
