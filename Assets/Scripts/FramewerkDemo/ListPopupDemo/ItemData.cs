using Framewerk.UI.List;

namespace FramewerkDemo.ListPopupDemo
{
    public class ItemData : IListItemDataProvider
    {
        public string Value { get; private set; }

        public ItemData(string value)
        {
            Value = value;
        }
    }
}
