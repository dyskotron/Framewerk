using Framewerk.UI.List;

namespace FramewerkDemo.ListPopupDemo
{
    public class ItemListItemMediator : ListItemMediator<ItemListItemView, ItemData>
    {
        public override void SetData(ItemData dataProvider, int index)
        {
            View.ValueText.text = dataProvider.Value;
            base.SetData(dataProvider, index);
        }
    }
}
