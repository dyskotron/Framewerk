using Framewerk.Popups;
using UnityEngine.UI;

namespace FramewerkDemo.Examples.ExamplePopup
{
    public class ExamplePopupView : PopupView, IPopupView
    {
        public Text MessageText;
        public Button ConfirmButton;

        public void SetMessage(string message)
        {
            MessageText.text = message;
        }
    }
}
