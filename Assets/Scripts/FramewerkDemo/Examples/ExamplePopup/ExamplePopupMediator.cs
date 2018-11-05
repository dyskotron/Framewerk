using Framewerk.Managers.Popup;
using FramewerkDemo.MainMenu.Controller;

namespace FramewerkDemo.Examples.ExamplePopup
{
    public class ExamplePopupMediator : PopupMediator<ExamplePopupView>
    {
        protected override void Init()
        {
            base.Init();

            View.SetMessage("Hello, i am really simple popup.\n" +
                            "Click the button and you'll get back to menu");
            AttachButtonListener(View.ConfirmButton, ButtonHandler);
        }

        private void ButtonHandler()
        {
            DispatchEvent(new ShowMenuEvent());        
        }
    }
}