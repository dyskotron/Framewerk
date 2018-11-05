using System;
using Framewerk.ViewComponents.ListComponent;
using FramewerkDemo.Examples.ExampleListPanel;
using UnityEngine;

namespace FramewerkDemo.Examples.ExampleVirtualListPanel
{
    public class ExampleVirtualLisItemMediator : ListItemMediator<ExampleListItemView, ExampleListDataProvider>
    {
        public override void SetData(ExampleListDataProvider dataProvider, int index)
        {
            View.Label.text = String.Format("{0}. {1}", index + 1, dataProvider.Name);
            View.Image.enabled = dataProvider.HasKitten;

            View.Image.color = Color.white;
            
            base.SetData(dataProvider, index);
        }

        public override void SetSelected(bool selected)
        {
            View.SetSelected(selected);
            View.transform.localScale = new Vector3(1,selected ? 1.5f : 1, 1); 
            base.SetSelected(selected);
        }
    }
}