using Framewerk.UI.List;

namespace Framewerk.Examples.ListExample
{
    public class ContactData : IListItemDataProvider
    {
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}
