using Framewerk.Popups;
using FramewerkDemo.MainMenu.Controller;

namespace FramewerkDemo.Examples.ExamplePopup
{
    public class ExamplePopupMediator : PopupMediator<ExamplePopupView>
    {
        [Inject] public ShowMenuSignal ShowMenuSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            View.SetMessage("Hello, i am really simple popup.\n" +
                            "Click the button and you'll get back to menu");
            AddButtonListener(View.ConfirmButton, ButtonHandler);
        }

        private void ButtonHandler()
        {
            ShowMenuSignal.Dispatch();
        }
    }
}
