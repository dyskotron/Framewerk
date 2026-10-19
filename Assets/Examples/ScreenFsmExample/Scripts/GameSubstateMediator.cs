using Framewerk.UI;

namespace Framewerk.Examples.ScreenFsmExample
{
    public class GameSubstateMediator : ExtendedMediator<GameSubstateView>
    {
        [Inject] public SwitchGameSubstateSignal SwitchGameSubstateSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            if (View.SwitchButton != null)
                AddButtonListener(View.SwitchButton, OnSwitchClicked);
        }

        private void OnSwitchClicked()
        {
            SwitchGameSubstateSignal.Dispatch();
        }
    }
}
