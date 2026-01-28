using Framewerk.UI;
using FramewerkDemo.MainMenu.Controller;
using FramewerkDemo.MainMenu.Model;

namespace FramewerkDemo.Examples
{
    public class TopMenuMediator : ExtendedMediator<TopMenuView>
    {
        [Inject] public IMenuModel MenuModel { get; set; }
        [Inject] public ShowMenuSignal ShowMenuSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            AddButtonListener(View.CloseButton, CloseButtonClickedHandler);
        }

        private void CloseButtonClickedHandler()
        {
            ShowMenuSignal.Dispatch();
        }
    }
}
