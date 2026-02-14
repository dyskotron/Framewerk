using Framewerk.AppStateMachine;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class AboutScreen : AppStateScreen
    {
        protected override void Enter()
        {
            InstantiateView<AboutContentView>();
        }
    }
}
