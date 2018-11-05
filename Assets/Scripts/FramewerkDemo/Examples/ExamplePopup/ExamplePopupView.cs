using Framewerk.Mvcs;
using UnityEngine.UI;

namespace FramewerkDemo.Examples.ExamplePopup
{
    public class ExamplePopupView : View
    {
        public Text MessageText;
        public Button ConfirmButton;
        
        public void SetMessage(string message)
        {
            MessageText.text = message;
        }
    }
}