using Framewerk.ViewComponents.ListComponent;
using UnityEngine;

namespace FramewerkDemo.Examples.ExampleListPanel
{
    public class ExampleLisItemMediator : ListItemMediator<ExampleListItemView, ExampleListDataProvider>
    {
        public override void SetData(ExampleListDataProvider dataProvider, int index)
        {
            View.Label.text = dataProvider.Name;
            View.Image.enabled = dataProvider.HasKitten;

            View.Image.color = Color.white;
            
            base.SetData(dataProvider, index);
        }

        public override void SetSelected(bool selected)
        {
            View.SetSelected(selected);
            base.SetSelected(selected);
        }
    }
}