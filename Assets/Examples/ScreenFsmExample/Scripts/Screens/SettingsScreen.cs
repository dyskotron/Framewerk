using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class SettingsScreen : AppStateScreen
    {
        protected override void Enter()
        {
            InstantiateView<SettingsContentView>();
        }
    }
}
