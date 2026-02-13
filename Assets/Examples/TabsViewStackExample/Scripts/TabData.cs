using Framewerk.UI.List;

namespace Framewerk.Examples.TabsViewStackExample
{
    public class TabData : IListItemDataProvider
    {
        public string Title { get; set; }
        public int ContentIndex { get; set; }
    }
}
