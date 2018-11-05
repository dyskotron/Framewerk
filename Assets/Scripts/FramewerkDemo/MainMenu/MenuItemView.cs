using Framewerk.ViewComponents.ListComponent;
using UnityEngine.UI;

namespace FramewerkDemo.MainMenu
{
    public class MenuItemView : ListItemView
    {
        public Text IndexText;
        public Text LabelText;

        public void SetIndex(int index)
        {
            IndexText.text = index.ToString();
        }

        public void SetLabel(string label)
        {
            LabelText.text = label;
        }
    }
}