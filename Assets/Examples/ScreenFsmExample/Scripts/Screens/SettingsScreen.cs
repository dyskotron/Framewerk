using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class SettingsScreen : AppStateScreen
    {
        protected override async void Enter()
        {
            await InstantiateViewAsync<SettingsContentView>();
        }
    }
}
