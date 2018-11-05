using Framewerk.ViewComponents.ListComponent;

namespace FramewerkDemo.MainMenu.Model
{
    public class MenuDataProvider : IListItemDataProvider
    {
        public ExampleId ItemIdId { get; private set; }
        public string Label { get; private set; }
        
        public MenuDataProvider(ExampleId itemIdId, string label)
        {
            ItemIdId = itemIdId;
            Label = label;
        }
    }
}