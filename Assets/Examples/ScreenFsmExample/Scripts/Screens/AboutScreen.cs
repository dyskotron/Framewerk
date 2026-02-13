using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class AboutScreen : AppStateScreen
    {
        protected override async void Enter()
        {
            await InstantiateViewAsync<AboutContentView>();
        }
    }
}
