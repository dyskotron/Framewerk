using Framewerk.AppStateMachine;
using Framewerk.UI;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class AboutContentMediator : ExtendedMediator<AboutContentView>
    {
        [Inject] public IAppFsm Fsm { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();
            
            if (View.BackButton != null)
                AddButtonListener(View.BackButton, OnBackClicked);
        }

        private void OnBackClicked()
        {
            Fsm.SwitchState(new MenuState());
        }
    }
}
