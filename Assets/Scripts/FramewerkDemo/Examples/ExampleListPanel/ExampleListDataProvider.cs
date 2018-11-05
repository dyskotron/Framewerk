using Framewerk.ViewComponents.ListComponent;

namespace FramewerkDemo.Examples.ExampleListPanel
{
    public class ExampleListDataProvider : IListItemDataProvider
    {
        public bool HasKitten { get; private  set; }
        public string Name { get; private  set; }
        
        public ExampleListDataProvider(string name, bool hasKitten)
        {
            Name = name;
            HasKitten = hasKitten;
        }
    }
}