using Framewerk.Popups;
using UnityEngine.UI;

namespace FramewerkDemo.ListPopupDemo
{
    public class ItemPopupView : PopupView, IPopupView
    {
        public Text ValueText;
        public Button CloseButton;

        public void SetValue(string value)
        {
            ValueText.text = value;
        }
    }
}
