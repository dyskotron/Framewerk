using Framewerk.ViewComponents.ListComponent;
using UnityEngine;
using UnityEngine.UI;

namespace FramewerkDemo.Examples.ExampleListPanel
{
    public class ExampleListItemView : ListItemView
    {
        public Text Label;
        public Image Image;
        public Image Background;
        public Color DefaultColor;
        public Color SelectedColor;

        public void SetSelected(bool selected)
        {
            Background.color = selected ? SelectedColor : DefaultColor;
        }
    }
}