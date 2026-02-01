using System.Collections.Generic;
using Framewerk.UI.List;
using FramewerkDemo.ListPopupDemo.Signals;

namespace FramewerkDemo.ListPopupDemo
{
    public class ItemListMediator : ListMediator<ItemListView, ItemData>
    {
        [Inject] public ItemClickedSignal ItemClickedSignal { get; set; }

        public override void OnRegister()
        {
            base.OnRegister();

            // Create 5 items with simple values
            var data = new List<ItemData>
            {
                new ItemData("Item 1"),
                new ItemData("Item 2"),
                new ItemData("Item 3"),
                new ItemData("Item 4"),
                new ItemData("Item 5")
            };

            SetData(data);
        }

        protected override void ListItemClicked(int index, ItemData dataProvider)
        {
            base.ListItemClicked(index, dataProvider);
            ItemClickedSignal.Dispatch(dataProvider);
        }
    }
}
